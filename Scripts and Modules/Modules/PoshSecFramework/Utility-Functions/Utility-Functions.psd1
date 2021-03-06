@{

# Script module or binary module file associated with this manifest.
ModuleToProcess = 'Utility-Functions.psm1'

# Version number of this module.
ModuleVersion = '0.2.0.0'

# ID used to uniquely identify this module
GUID = '7f701b23-9af6-4871-b2f2-985f4609cc58'

# Author of this module
Author = 'PoshSec'

# Copyright statement for this module
Copyright = 'BSD 3-Clause'

# Description of the functionality provided by this module
Description = 'PoshSec Utility Functions Module'

# Minimum version of the Windows PowerShell engine required by this module
PowerShellVersion = '2.0'

# Cmdlets to export from this module
CmdletsToExport = '*'

# Functions to export from this module
FunctionsToExport = '*'

# List of all modules packaged with this module.
ModuleList = @(@{ModuleName = 'Utility-Functions'; ModuleVersion = '0.1.0.0'; GUID = '7f701b23-9af6-4871-b2f2-985f4609cc58'})

# List of all files packaged with this module
FileList = 'Utility-Functions.psd1', 'Utility-Functions.psm1', 'Confirm-SecIsAdministrator.ps1', 'Invoke-RemoteProcess.ps1', 'Invoke-RemoteWmiProcess', 'Get-RemoteRegistry.ps1', 'Get-RemoteRegistryValue.ps1', 'Get-RemoteRegistryKey.ps1', 'Get-RemotePSVersion.ps1', 'Get-RemoteOS.ps1', 'Get-RemoteArchitecture.ps1', 'Get-RemoteNETVersion.ps1', 'Get-RemoteProcess.ps1', 'Set-PSRemoting.ps1', 'Execute-RemoteWmiProcess.ps1'
}