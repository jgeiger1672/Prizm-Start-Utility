using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Prizm_Start_Utility
{
    public partial class PrizmStartUtility : Form
    {
        private AppConfig config;

        public PrizmStartUtility(string configFilPath)
        {
            InitializeComponent();
            config = ConfigLoader.LoadConfig(configFilPath);
            InitializeExistingVersionList();
            if (existingVersionComboBox.SelectedValue != null)
            {
                string serverVersion = existingVersionComboBox.SelectedValue.ToString();
                InitializeLicenseSKUList(serverVersion);
                startPrizmButton.Enabled = true;
            }
        }

        private async void Start_Prizm_Click(object sender, EventArgs e)
        {
            try
            {
                if (licenseComboBox.SelectedValue == null)
                {
                    string message = "Please enter a valid version string.\nLicense selection list will populate if version string is valid.";
                    string caption = "Error: Invalid version string";

                    MessageBox.Show(message, caption);
                    return;
                }

                var watch = Stopwatch.StartNew();
                progressBar.Visible = true;
                LogOutput("Starting Prizm...");

                if (!await IsDockerEngineRunningAsync())
                {
                    await StartDockerEngineAsync();
                }

                string serverVersion;
                string pasVersion;
                bool startPAS = pasCheckbox.Checked;

                if (useExistingVersionButton.Checked)
                {
                    serverVersion = existingVersionComboBox.Text;
                    pasVersion = await GetClosestPASVersionAsync(serverVersion);
                }
                else if (pullNewVersionButton.Checked)
                {
                    serverVersion = newVersionTextBox.Text;
                    pasVersion = await GetClosestPASVersionAsync(serverVersion);
                }
                else
                {
                    serverVersion = "latest";
                    pasVersion = "latest";
                }

                if (!await DockerHubVersionTagExistsAsync(config.Docker.ServerRepository, serverVersion))
                {
                    LogOutput($"Error: \"{serverVersion}\" tag does not exist for \"{config.Docker.Namespace}/{config.Docker.ServerRepository}\" Docker image");
                    progressBar.Visible = false;
                    watch.Stop();
                    LogOutput($"Elapsed time: {watch.Elapsed}");
                    return;
                }

                if (serverVersion != pasVersion)
                {
                    string message = $"Version tag \"{serverVersion}\" was found for \"{config.Docker.Namespace}/{config.Docker.ServerRepository}\", but was not found for \"{config.Docker.Namespace}/{config.Docker.PASRepository}\".\n\n" +
                        $"The closest available version of PAS is \"{pasVersion}\". Would you like to use this version of PAS instead?\n\n" +
                        $"Select \"Yes\" to run Server version \"{serverVersion}\", and PAS version \"{pasVersion}\"\n" +
                        $"Select \"No\" to run Server version \"{serverVersion}\", without running PAS";

                    string caption = "Error: Same version of PAS not available";

                    MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                    DialogResult result;

                    result = MessageBox.Show(message, caption, buttons);
                    if (result == DialogResult.Yes)
                    {
                        startPAS = true;
                    }
                    else
                    {
                        startPAS = false;
                    }
                }

                string majorVersion = GetMajorVersion(serverVersion);

                if (majorVersion == "14")
                {
                    while (!IsMySQLServerRunning())
                    {
                        StartMySQLServerAsync();
                    }
                }

                if (!await LocalDockerImageExistsAsync_Server(serverVersion))
                {
                    LogOutput($"Docker image \"accusoft/prizmdoc-server:{serverVersion}\" not found locally. Pulling image...");
                }

                Task serverPullTask = PullNewServerVersionAsync(serverVersion);
                Task pasPullTask = Task.CompletedTask; // initialized as completed, in case startPAS is false

                if (startPAS)
                {
                    if (await DockerHubVersionTagExistsAsync(config.Docker.PASRepository, pasVersion))
                    {
                        if (!await LocalDockerImageExistsAsync_PAS(pasVersion))
                        {
                            LogOutput($"Docker image \"accusoft/prizmdoc-application-services:{pasVersion}\" not found locally. Pulling image...");
                        }
                        pasPullTask = PullNewPASVersionAsync(pasVersion);
                    }
                    else
                    {
                        LogOutput($@"Error: Version ""{pasVersion}"" not found in {config.Docker.PASRepository}. PAS not started.");
                    }
                }

                await Task.WhenAll(serverPullTask, pasPullTask);

                await ConfigurePrizmServerAsync(serverVersion);
                await ConfigurePasAsync(pasVersion);

                LogOutput($"Starting Server version {serverVersion}");
                StartPrizmServer(serverVersion);

                if (startPAS)
                {
                    LogOutput($"Starting PAS version {pasVersion}");
                    StartPAS(pasVersion);
                }

                LogOutput($"Prizm version {serverVersion} started successfully. Open Admin Page to view Prizm Services Status.");
                progressBar.Visible = false;
                watch.Stop();
                LogOutput($"Elapsed time: {watch.Elapsed}");
                return;
            }
            catch (Exception ex)
            {
                progressBar.Visible = false;
                LogOutput($"Error: {ex.Message}");
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void StartPrizmServer(string serverVersion)
        {
            try
            {
                string serverDirectory = Path.Combine(config.PrizmDocDirectory, serverVersion, "Server");
                string script = $@"
                cd {serverDirectory}
                docker run --rm --env ACCEPT_EULA=YES --publish 18681:18681 --volume $pwd/config:/config --volume $pwd/logs:/logs --volume $pwd/data:/data --name prizmdoc-server accusoft/prizmdoc-server:{serverVersion}";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };

                await Task.Run(() =>
                {
                    using (Process process = Process.Start(startInfo))
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        if (!string.IsNullOrEmpty(output))
                        {
                            MessageBox.Show(output, "Output", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        if (!string.IsNullOrEmpty(error))
                        {
                            MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                });

                LogOutput($"\"accusoft/prizmdoc-server:{serverVersion}\" started successfully.");
            }
            catch (Exception ex)
            {
                LogOutput($"Error: {ex.Message}");
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void StartPAS(string serverVersion)
        {
            try
            {
                string pasDirectory = Path.Combine(config.PrizmDocDirectory, serverVersion, "PAS");
                string script = $@"
                cd {pasDirectory}
                docker run --rm --env ACCEPT_EULA=YES --publish 3000:3000 --volume $pwd/config:/config --volume $pwd/logs:/logs --volume $pwd/data:/data --name pas accusoft/prizmdoc-application-services:{serverVersion}";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };

                await Task.Run(() =>
                {
                    using (Process process = Process.Start(startInfo))
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        if (!string.IsNullOrEmpty(output))
                        {
                            MessageBox.Show(output, "Output", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        if (!string.IsNullOrEmpty(error))
                        {
                            MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                });

                LogOutput($"\"accusoft/prizmdoc-application-services:{serverVersion}\" started successfully.");
            }
            catch (Exception ex)
            {
                LogOutput($"Error: {ex.Message}");
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task PullNewServerVersionAsync(string serverVersion)
        {
            try
            {
                CreateRequiredDirectories_Server(serverVersion);
                await RunInitConfigDockerCommandAsync_Server(serverVersion);
            }
            catch (Exception ex)
            {
                LogOutput($"Error: {ex.Message}");
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task PullNewPASVersionAsync(string pasVersion)
        {
            try
            {
                CreateRequiredDirectories_PAS(pasVersion);
                await RunInitConfigDockerCommandAsync_PAS(pasVersion);
            }
            catch (Exception ex)
            {
                LogOutput($"Error: {ex.Message}");
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task ConfigurePrizmServerAsync(string serverVersion)
        {
            string licensePath = config.LicensePath;
            string centralConfigFilePath = Path.Combine(config.PrizmDocDirectory, serverVersion, "Server", "config", "prizm-services-config.yml");
            string majorVersion = GetMajorVersion(serverVersion);
            string sku = "PZM-DOC-OEM-ALL";

            string msOfficeHost = config.MsOffice.Host;
            string msOfficePort = config.MsOffice.Port;

            // database connection info
            string databaseAdapter = config.Database.Adapter;
            string databaseUser = config.Database.Username;
            string databasePassword = config.Database.Password;
            string databaseHost = config.Database.Host;
            string databasePort = config.Database.Port;
            string databaseSchema = config.Database.Schema;
            string databaseConnectionString = $"\"{databaseAdapter}://{databaseUser}:{databasePassword}@{databaseHost}:{databasePort}/{databaseSchema}\"";

            // read license file
            string licensesString = await File.ReadAllTextAsync(licensePath);

            // parse json
            JObject licensesJson = JObject.Parse(licensesString);

            // find license object based on SKU
            var licenseObject = licensesJson["licenses"][majorVersion]
                .FirstOrDefault(obj => obj["sku"]?.ToString() == sku);

            if (licenseObject == null)
            {
                MessageBox.Show($"License key for SKU '{sku}' is missing in the licenses.json file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // extract values from license object
            string licenseKey = licenseObject["key"]?.ToString();
            string solutionName = licenseObject["solution"]?.ToString();

            if (licenseKey == null || solutionName == null)
            {
                MessageBox.Show($"License key or solution name for SKU '{sku}' is missing in the licenses.json file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // read config file
            string[] configLines = await File.ReadAllLinesAsync(centralConfigFilePath);

            // modify config file
            for (int i = 0; i < configLines.Length; i++)
            {
                // configure license key
                if (configLines[i].Contains("license.key:"))
                {
                    configLines[i] = configLines[i].Replace("YOUR_LICENSE_KEY", licenseKey);
                }
                if (configLines[i].Contains("license.solutionName:"))
                {
                    configLines[i] = configLines[i].Replace("YOUR_SOLUTION_NAME", solutionName);
                }

                // configure MSO / Libre
                if (msoButton.Checked)
                {
                    if (configLines[i].Contains("fidelity.msOfficeDocumentsRenderer:"))
                    {
                        configLines[i] = "fidelity.msOfficeDocumentsRenderer: msoffice";
                    }
                    if (configLines[i].Contains("fidelity.msOfficeCluster.host:"))
                    {
                        configLines[i] = $"fidelity.msOfficeCluster.host: \"{msOfficeHost}\"";
                    }
                    if (configLines[i].Contains("fidelity.msOfficeCluster.port:"))
                    {
                        configLines[i] = $"fidelity.msOfficeCluster.port: {msOfficePort}";
                    }
                }
                else if (libreButton.Checked)
                {
                    if (configLines[i].Contains("fidelity.msOfficeDocumentsRenderer:"))
                    {
                        configLines[i] = "fidelity.msOfficeDocumentsRenderer: libreoffice";
                    }
                }

                // configure database for v14
                if (majorVersion == "14")
                {
                    if (configLines[i].Contains("database.connectionString:"))
                    {
                        configLines[i] = $"database.connectionString: {databaseConnectionString}";
                    }
                }
            }

            // write updated config file
            await File.WriteAllLinesAsync(centralConfigFilePath, configLines);
            LogOutput($"Prizm Server License configured");
        }

        private async Task ConfigurePasAsync(string pasVersion)
        {
            string configFilePath = Path.Combine(config.PrizmDocDirectory, pasVersion, "PAS", "config", "pcc.nix.yml");
            string majorVersion = GetMajorVersion(pasVersion);
            string pccServerHost = config.PAS.PCCServer.Host;

            // database connection info
            string databaseAdapter = config.Database.Adapter;
            string databaseUser = config.Database.Username;
            string databasePassword = config.Database.Password;
            string databaseHost = config.Database.Host;
            string databasePort = config.Database.Port;
            string databaseSchema = config.Database.Schema;
            string databaseConnectionString = $"\"{databaseAdapter}://{databaseUser}:{databasePassword}@{databaseHost}:{databasePort}/{databaseSchema}\"";

            // read config file
            string[] configLines = File.ReadAllLines(configFilePath);

            // modify config file
            for (int i = 0; i < configLines.Length; i++)
            {
                if (configLines[i].Contains("pccServer.hostName:"))
                {
                    configLines[i] = configLines[i].Replace("localhost", pccServerHost);
                }

                // configure database for v14
                if (majorVersion == "14")
                {
                    if (configLines[i].Contains("database.adapter:"))
                    {
                        configLines[i] = $"database.adapter: {databaseAdapter}";
                    }
                    if (configLines[i].Contains("database.connectionString:"))
                    {
                        configLines[i] = $"database.connectionString: {databaseConnectionString}";
                    }
                }
            }

            // write updated config file
            await File.WriteAllLinesAsync(configFilePath, configLines);
            LogOutput($"PAS configured.");
        }

        private async Task RunInitConfigDockerCommandAsync_Server(string serverVersion)
        {
            string serverDirectory = Path.Combine(config.PrizmDocDirectory, serverVersion, "Server");

            var StartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"cd {serverDirectory}; docker run --rm -e ACCEPT_EULA=YES --volume $pwd/config:/config accusoft/prizmdoc-server:{serverVersion} init-config\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(StartInfo))
            {
                await process.WaitForExitAsync();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                if (!string.IsNullOrEmpty(error))
                {
                    LogOutput($"Error: {error}");
                }
            }
            LogOutput($"Docker image \"accusoft/prizmdoc-server:{serverVersion}\" pulled successfully.");
            LogOutput($"Server container configuration successfully initialized.");
        }

        private async Task RunInitConfigDockerCommandAsync_PAS(string pasVersion)
        {
            string pasDirectory = Path.Combine(config.PrizmDocDirectory, pasVersion, "PAS");

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"cd {pasDirectory}; docker run --rm -e ACCEPT_EULA=YES --volume $pwd/config:/config accusoft/prizmdoc-application-services:{pasVersion} init-config\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                await process.WaitForExitAsync();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                if (!string.IsNullOrEmpty(error))
                {
                    LogOutput($"Error: {error}");
                }
            }
            LogOutput($"Docker image \"accusoft/prizmdoc-application-services:{pasVersion}\" pulled successfully.");
            LogOutput($"PAS container configuration successfully initialized.");
        }

        private void CreateRequiredDirectories_Server(string serverVersion)
        {
            string serverConfigDirectory = Path.Combine(config.PrizmDocDirectory, serverVersion, "Server", "config");
            string serverLogDirectory = Path.Combine(config.PrizmDocDirectory, serverVersion, "Server", "logs");

            if (!Directory.Exists(serverConfigDirectory))
            {
                Directory.CreateDirectory(serverConfigDirectory);
            }

            if (!Directory.Exists(serverLogDirectory))
            {
                Directory.CreateDirectory(serverLogDirectory);
            }
        }

        private void CreateRequiredDirectories_PAS(string pasVersion)
        {
            string pasConfigDirectory = Path.Combine(config.PrizmDocDirectory, pasVersion, "PAS", "config");
            string pasLogsDirectory = Path.Combine(config.PrizmDocDirectory, pasVersion, "PAS", "logs");
            string pasDataDirectory = Path.Combine(config.PrizmDocDirectory, pasVersion, "PAS", "data");

            if (!Directory.Exists(pasConfigDirectory))
            {
                Directory.CreateDirectory(pasConfigDirectory);
            }

            if (!Directory.Exists(pasLogsDirectory))
            {
                Directory.CreateDirectory(pasLogsDirectory);
            }

            if (!Directory.Exists(pasDataDirectory))
            {
                Directory.CreateDirectory(pasDataDirectory);
            }
        }

        private static async Task<bool> IsDockerEngineRunningAsync()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "info",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                return !string.IsNullOrEmpty(error);
            }
        }

        private async Task StartDockerEngineAsync()
        {
            LogOutput("Starting Docker Engine...");

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-Command \"Start-Service docker\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                await process.WaitForExitAsync();
            }

            // wait until docker engine is started
            while (!await IsDockerEngineRunningAsync())
            {
                await Task.Delay(1000);
            }
            LogOutput("Docker Engine started successfully.");
        }

        private static bool IsMySQLServerRunning()
        {
            var StartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"(Get-Service -Name 'MySQL80').Status\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(StartInfo))
            {
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();
                return output.Equals("Running", StringComparison.OrdinalIgnoreCase);
            }
        }

        private static async void StartMySQLServerAsync()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"Start-Service -Name 'MySQL80'\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            await Task.Run(() =>
            {
                using (var process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                }
            });
        }

        private void InitializeExistingVersionList()
        {
            string[] directories = Directory.GetDirectories(config.PrizmDocDirectory);
            List<string> versionList = new List<string>();

            foreach (string dir in directories)
            {
                string dirName = new DirectoryInfo(dir).Name;
                if (IsValidVersionString(dirName))
                {
                    versionList.Add(dirName);
                }
            }

            versionList.Sort();
            versionList.Reverse();

            List<string> previewVersions = new List<string>();
            for (int i = versionList.Count - 1; i >= 0; i--)
            {
                if (versionList[i].StartsWith("preview"))
                {
                    previewVersions.Add(versionList[i]);
                    versionList.RemoveAt(i);
                }
            }

            previewVersions.Reverse();
            versionList.AddRange(previewVersions);

            existingVersionComboBox.DataSource = versionList;

            // Find the width of the longest string
            int maxWidth = 0;
            using (Graphics g = existingVersionComboBox.CreateGraphics())
            {
                foreach (string version in versionList)
                {
                    int width = TextRenderer.MeasureText(g, version, existingVersionComboBox.Font).Width;
                    if (width > maxWidth)
                    {
                        maxWidth = width;
                    }
                }
            }

            existingVersionComboBox.DropDownWidth = maxWidth + 10;
        }

        private void InitializeLicenseSKUList(string serverVersion)
        {
            if (IsValidVersionString(serverVersion))
            {
                UpdateLicenseSKUOptions(serverVersion);
            }
            else
            {
                licenseComboBox.DataSource = null;
                licenseComboBox.SelectedItem = null;
                licenseComboBox.Text = string.Empty;
                licenseComboBox.DropDownHeight = licenseComboBox.Height;
                licenseComboBox.DropDownWidth = licenseComboBox.Width;
            }
        }

        private List<string> GetSKUsForMajorVersion(string majorVersion)
        {
            string licensePath = config.LicensePath;
            string json = File.ReadAllText(licensePath);
            var licensesJson = JObject.Parse(json);

            var skus = new List<string>();

            if (licensesJson["licenses"][majorVersion] != null)
            {
                foreach (var license in licensesJson["licenses"][majorVersion])
                {
                    string sku = license["sku"]?.ToString();
                    if (!string.IsNullOrEmpty(sku))
                    {
                        skus.Add(sku);
                    }
                }
            }
            return skus;
        }

        private void UpdateLicenseSKUOptions(string serverVersion)
        {
            if (string.IsNullOrEmpty(serverVersion))
                return;

            string majorVersion = GetMajorVersion(serverVersion);
            List<string> skus = GetSKUsForMajorVersion(majorVersion);

            licenseComboBox.DataSource = skus;

            // Find the width of the longest string
            int maxWidth = 0;
            using (Graphics g = existingVersionComboBox.CreateGraphics())
            {
                foreach (string sku in skus)
                {
                    int width = TextRenderer.MeasureText(g, sku, existingVersionComboBox.Font).Width;
                    if (width > maxWidth)
                    {
                        maxWidth = width;
                    }
                }
            }

            licenseComboBox.DropDownWidth = maxWidth + 10;
            licenseComboBox.DropDownHeight = PreferredSize.Height;

            // default to OEM - all features enabled
            if (skus.Contains(config.DefaultLicenseSKU))
            {
                licenseComboBox.SelectedItem = config.DefaultLicenseSKU;
            }
            else if (licenseComboBox.Items.Count > 0)
            {
                licenseComboBox.SelectedIndex = 0;
            }
        }

        private async Task<bool> LocalDockerImageExistsAsync_Server(string serverVersion)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "images --format \"{{.Repository}}:{{.Tag}}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(error))
                    {
                        MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    return lines.Any(line => line.Equals($"{config.Docker.Namespace}/{config.Docker.ServerRepository}:{serverVersion}"));
                }
            }
            catch (Exception ex)
            {
                LogOutput($"Error: {ex.Message}");
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task<bool> LocalDockerImageExistsAsync_PAS(string pasVersion)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "images --format \"{{.Repository}}:{{.Tag}}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(error))
                    {
                        MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    return lines.Any(line => line.Equals($"{config.Docker.Namespace}/{config.Docker.PASRepository}:{pasVersion}"));
                }
            }
            catch (Exception ex)
            {
                LogOutput($"Error: {ex.Message}");
                MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task<bool> DockerHubVersionTagExistsAsync(string repository, string version)
        {
            UriBuilder tagExistsUriBuilder = new UriBuilder
            {
                Scheme = "https",
                Host = "hub.docker.com",
                Path = $"/v2/namespaces/{config.Docker.Namespace}/repositories/{repository}/tags/{version}"
            };

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(tagExistsUriBuilder.Uri);
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return false;
                    }
                    else
                    {
                        LogOutput($"Error while checking Docker Hub tag: {response.StatusCode}");
                        return false;
                    }

                }
                catch (Exception ex)
                {
                    LogOutput($"Error while checking Docker Hub tag: {ex.Message}");
                    return false;
                }
            }

        }

        private async Task<string> GetClosestPASVersionAsync(string serverVersion)
        {
            if (await DockerHubVersionTagExistsAsync(config.Docker.ServerRepository, serverVersion) &&
                await DockerHubVersionTagExistsAsync(config.Docker.PASRepository, serverVersion))
            {
                return serverVersion;
            }
            else if (await DockerHubVersionTagExistsAsync(config.Docker.PASRepository, GetMajorMinorVersion(serverVersion)))
            {
                return GetMajorMinorVersion(serverVersion);
            }
            else
            {
                return "";
            }
        }

        private static string GetMajorVersion(string version)
        {
            var regex = new Regex(@"\d+");

            var match = regex.Match(version);

            if (match.Success)
            {
                return match.Value;
            }
            else
            {
                return string.Empty;
            }
        }

        private static string GetMajorMinorVersion(string version)
        {
            var regex = new Regex(@"^(\d+\.\d+)");

            var match = regex.Match(version);

            if (match.Success)
            {
                return match.Value;
            }
            else
            {
                return string.Empty;
            }
        }

        private static bool IsValidVersionString(string version)
        {
            Regex versionNumberRegex = new Regex(@"^(preview-)?[0-9]+(\.[0-9]+)*$");

            if (versionNumberRegex.IsMatch(version))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void LogOutput(string message)
        {
            outputConsole.AppendText($"{message}{Environment.NewLine}");
            outputConsole.ScrollToCaret();
        }

        private void ExistingVersionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string serverVersion = existingVersionComboBox.Text;
            if (serverVersion != null)
            {
                UpdateLicenseSKUOptions(serverVersion);
                startPrizmButton.Enabled = true;
            }
            else
            {
                startPrizmButton.Enabled = false;
            }
        }

        private void UseExistingVersionButton_CheckedChanged(object sender, EventArgs e)
        {
            if (useExistingVersionButton.Checked)
            {
                existingVersionComboBox.Enabled = true;
                if (existingVersionComboBox.SelectedValue != null)
                {
                    startPrizmButton.Enabled = true;
                }
                else
                {
                    startPrizmButton.Enabled = false;
                }
            }
            else
            {
                existingVersionComboBox.Enabled = false;

            }
        }

        private void PullNewVersionButton_CheckedChanged(object sender, EventArgs e)
        {
            if (pullNewVersionButton.Checked)
            {
                newVersionTextBox.Enabled = true;
                startPrizmButton.Enabled = true;
            }
            else
            {
                newVersionTextBox.Enabled = false;
            }
        }

        private void NewVersionTextBox_Leave(object sender, EventArgs e)
        {
            string serverVersion = newVersionTextBox.Text;

            if (IsValidVersionString(serverVersion))
            {
                UpdateLicenseSKUOptions(serverVersion);
            }
            else
            {
                InitializeLicenseSKUList(serverVersion);
            }
        }

        private void LatestVersionButton_CheckedChanged(object sender, EventArgs e)
        {
            if (latestVersionButton.Checked)
            {
                startPrizmButton.Enabled = false;
            }
            else
            {
                startPrizmButton.Enabled |= true;
            }
        }

        private void AdminPageButton_Click(object sender, EventArgs e)
        {
            const string adminURL = "http://localhost:18681/admin";
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = adminURL,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                LogOutput($"Error: {ex.Message}");
                MessageBox.Show($"Failed to open URL: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void PrizmStartUtility_Click(object sender, EventArgs e)
        {
            if (newVersionTextBox.Focused)
            {
                this.ActiveControl = null;
            }
        }
    }
}
