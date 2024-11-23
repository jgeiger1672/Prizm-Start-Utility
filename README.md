# Prizm Start Utility - README

## Introduction
The **Prizm Start Utility** is a tool designed to simplify the process of starting and managing Docker containers for Accusoft's PrizmDoc Server and PrizmDoc Application Services (PAS).

This utility allows users to set up and start specific versions of PrizmDoc and PAS, and easily configure licensing, MSO/LibreOffice rendering settings, and database connection settings.

## Prerequisites
Before using the Prizm Start Utility, ensure that the following requirements are met:

- **Docker Engine**: Docker must be installed and running on your machine. You can install Docker Desktop from [Docker's official site](https://www.docker.com/products/docker-desktop/).
  
- **MySQL Server**: For PrizmDoc version 14.1 and above, a MySQL server must be running. See [Accusoft Documentation](https://help.accusoft.com/PrizmDoc/latest/HTML/configure-the-central-database.html) for details. You can download and install MySQL from [MySQL's official website](https://dev.mysql.com/downloads/installer/).
  - For version 14 and higher, the tool will automatically check if the MySQL service (`MySQL80`) is running. If not, the service will be started automatically.

- **PowerShell**: The project uses PowerShell commands to manage Docker containers. Ensure that PowerShell is available and properly configured.

## Project Setup

#### **Running the Project in Visual Studio**

1. **Clone the Repository**:  
   Clone the project repository to your local machine:
   ```bash
   git clone https://your-git-repo-url.git
   ```

2. **Open the Project**:  
   - Open the project in Visual Studio.
   - Ensure that the **config.json** file is correctly set up (see next section).



3. **Run the Project**:  
   Once everything is set up, build and run the project. Ensure that Docker is running in the background (and MySQL, if you plan to use v14.1+).

#### **Building the Project into an Executable**
To create a standalone executable that you can easily run without opening Visual Studio every time:

1. **Set Project to Release mode**:
    - In Visual Studio, go to **Build** > **Configuration Manager** > **Active Solution configuration** > set to **"Release"**

2. **Build the Project**:  
   - Go to the **Build** menu and select **Build Solution** or press `Ctrl+Shift+B`.

3. **Find the Executable**:  
   - After building the project, the executable can be found in the project's `bin` folder:
     ```
     Prizm Start Utility/bin/Debug/netX.X-windows/PrizmStartUtility.exe
     ```
     Replace `netX.X` with the actual framework version you are using.

4. **Run the Executable**:  
   - Navigate to the folder containing the executable and double-click on **PrizmStartUtility.exe** to run the utility. You can also pin this executable to your start menu/taskbar for easy access.

## Configuring `config.json`
The **config.json** file is used to specify paths and settings required by the Prizm Start Utility. Below is an example structure of `config.json`:

```json
{
  "PrizmDocDirectory": "C:\\Users\\your-username\\Documents\\PrizmDoc",
  "LicensePath": "C:\\Users\\your-username\\Documents\\PrizmDoc\\licenses.json",
  "Database": {
    "Adapter": "mysql",
    "Username": "root",
    "Password": "your_password",
    "Host": "localhost",
    "Port": "3306",
    "Schema": "prizmdoc_server_db"
  },
  "MsOffice": {
    "Host": "http://your-ms-office-host",
    "Port": "18681"
  },
  "PAS": {
    "PCCServer": {
      "Host": "localhost",
      "Port": "3000"
    }
  },
  "Docker": {
    "Namespace": "accusoft",
    "ServerRepository": "prizmdoc-server",
    "PASRepository": "prizmdoc-application-services"
  },
  "DefaultLicenseSKU": "PZM-DOC-OEM-ALL"
}
```

### Key Configuration Fields:
- **PrizmDocDirectory**: The main local folder where all versions of PrizmDoc will be stored.
- **LicensePath**: The path to your `licenses.json` file, which contains your license keys.
- **Database**: Connection settings for the MySQL database, required for PrizmDoc version 14.1 and higher.
- **MsOffice**: Host/Port of the PrizmDoc Server with MSO installed and enabled, to connect to in order to use MSO rendering. See [Accusoft Documentation](https://help.accusoft.com/PrizmDoc/latest/HTML/configure-ms-office-conversion.html) for more details.
- **PAS**: Host/Port of the PrizmDoc Server to connect to PrizmDoc Application Services.
- **Docker**: Namespace and repository details for pulling Docker images.

### License Configuration:
- You can download the `license.json` file [here](https://git.jpg.com/prizmdoc/viewer/prizmdoc-test-license/-/blob/master/licenses.json?ref_type=heads). **This file is for internal  use only, do not share with customers!!**
- Place the `license.json` file at the specified `LicensePath` in the `config.json` file.

## General Project Structure
Once the utility is running and images are pulled, your **PrizmDoc** folder will have the following structure:

```text
PrizmDoc/
│
├── licenses.json                  # License keys for PrizmDoc
├── 13.28.0.14042/                 # Example folder for PrizmDoc version 13.28
│   ├── Server/
│   │   ├── config/
│   │   ├── logs/
│   │   ├── data/
│   │   └── ...                    # PrizmDoc server files for this version
│   └── Server/
│       ├── config/
│       ├── logs/
│       ├── data/
│       └── ...                    # PAS files for this version
├── 14.1.0.13866/                  # Example folder for PrizmDoc version 14.1
│   ├── Server/
│   ├── PAS/
│   └── ...
└── ...
```

### Folder Contents:
- **Server**: PrizmDoc Server [Central Configuration File](https://help.accusoft.com/PrizmDoc/latest/HTML/central-configuration.html) (prizm-services-config.yml), logs, and data files for the specific version.
- **PAS**: PrizmDoc Application Services [Configuration File](https://help.accusoft.com/PrizmDoc/latest/HTML/pas-configuration.html) (pcc.nix.yml), logs, and data files for the specific version.

## How to Use the Utility
1. **Pull New Version**:  
   - Enter the version number (e.g., `14.1`) in the "Pull New Version" text box, and click "Start Prizm" to pull the image, configure the server, and start a Docker Container for the selected version of PrizmDoc Server and PAS.
   - Note that pulling the docker images for new versions usually takes ~2 mins

2. **Select Existing Version**:  
   - Use the "Existing Version" drop-down to select a previously pulled version of PrizmDoc Server and PAS from your local directory. You can use this option after you have already pulled/configured a version using the "Pull New Version" option.



3. **Start PrizmDoc and PAS**:  
   - After selecting or pulling a version, click "Start Prizm" to start the PrizmDoc Server and PAS containers. The logs will show the pull progress and any other relevant information.

4. **License Selection**:  
   - Based on the version selected, the utility will automatically load the available SKUs from the `license.json` file. Select the SKU from the drop-down menu.

5. **MSO/LibreOffice Rendering**:
    - Choose which rendering engine you want to use with your PrizmDoc Server. 

6. **Open Admin Page**:
    - You can use this button to open up the admin page, to check the status of your PrizmDoc Server

## Additional Notes:
- **Version Validation**: The utility validates the version entered in the "Pull New Version" field by checking Docker Hub for the existence of the corresponding tag.
- **Latest Version Support**: The tool **does not currently support** pulling and running the "latest" version of PrizmDoc Server; this button doesn't work yet.
- **Logging/Output Console**: The application has a built-in output console to show the status of the current process. These logs are currently manually reported, and may be inaccurate/out of order. Logs will be improved/pulled directly from "docker pull" output in a future version.
