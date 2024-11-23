namespace Prizm_Start_Utility
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            string configFilePath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName, "config.json");

            Application.EnableVisualStyles();
            ApplicationConfiguration.Initialize();
            var config = ConfigLoader.LoadConfig(configFilePath);

            if (!Directory.Exists(config.PrizmDocDirectory))
            {
                using (var folderBrowserDialog = new FolderBrowserDialog())
                {
                    folderBrowserDialog.Description = "Select or create your PrizmDoc Directory (where folders for each server version will be set up)";
                    folderBrowserDialog.ShowNewFolderButton = true;
                    folderBrowserDialog.UseDescriptionForTitle = true;

                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        config.PrizmDocDirectory = folderBrowserDialog.SelectedPath;
                        ConfigLoader.SaveConfig(config, configFilePath);
                    }
                    else
                    {
                        return;
                    }
                }
            }

            if (!File.Exists(config.LicensePath))
            {
                using (var openFileDialog = new  OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = config.PrizmDocDirectory;
                    openFileDialog.Filter = "JSON files (*.JSON)|*.json";
                    openFileDialog.Title = "Select your \"licenses.json\" file";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        config.LicensePath = openFileDialog.FileName;
                        ConfigLoader.SaveConfig(config, configFilePath);
                    }
                    else
                    {
                        return;
                    }
                }
            }

            var prizmStartUtilityForm = new PrizmStartUtility(configFilePath);
            prizmStartUtilityForm.Load += (s, e) => prizmStartUtilityForm.Activate();

            Application.Run(prizmStartUtilityForm);
        }
    }
}