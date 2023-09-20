# My IP Address Environment Variable

Sets a machine-level environment variable (default `MyIPAddress`) with the host system's IP address. Monitors for changes to the IP address and updates the variable as needed.

_This only works on Windows_ and requires administrative permission (e.g., as a service running as Local System).

## Build

It's recommended that services use a self-contained binary, so use the Publish feature to build the project.

## Setup Config in `appsettings.json`

In `appsettings.json`, replace the MAC address with the adapter you want to monitor, similar to the following. Hyphens, colons, or any other non-hex separator are optional.

```json
{
  "config": {
    "macAddress": "AB-CD-EF-12-34-56"
  }
}
```

<details>
<summary markdown="span">Changing the Variable Name</summary>

The default variable name is `MyIPAddress` but it can be customized in `appsettings.json` as shown below.

```json
{
  "config": {
    "environmentVariableName": "MyCustomIPAddress"
  }
}
```

</details>

## Install as a Windows Service

1. Copy the executable and your `appsettings.json` file to `C:\Program Files (x86)\MyIPVariable`.
2. Execute the following in an _administrative_ PowerShell/cmd:

   ```
   sc.exe create "MyIPVariable" binpath="C:\Program Files (x86)\MyIPVariable\MyIPVariable.exe" start=auto
   sc.exe description MyIPVariable "MyIPVariable maintains an environment variable with the host's IP address."
   sc.exe start MyIPVariable
   ```

## Uninstall Windows Service

1. Execute the following in an _administrative_ PowerShell/cmd:

   ```
   sc.exe stop MyIPVariable
   sc.exe delete MyIPVariable
   ```

2. Delete the folder `C:\Program Files (x86)\MyIPVariable`.
