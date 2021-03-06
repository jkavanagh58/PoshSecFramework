<#    
.SYNOPSIS
  Returns the values for a given key.
.DESCRIPTION
  This function will return the values for
  the specified key in path.
  
  For the hkey use the following:
  
  HKEY_CLASSES_ROOT  = 1
  HKEY_CURRENT_USER = 2
  HKEY_LOCAL_MACHINE = 3
  HKEY_USERS  = 4
  HKEY_CURRENT_CONFIG  = 5
  HKEY_DYN_DATA = 6
  
AUTHOR
  Ben0xA
  
.PARAMETER computer
  The remote system on which to run the command.
  
.PARAMETER hkey
  The base HKEY to use for the registry.
  
.PARAMETER path
  The path to the key to query.
  
.EXAMPLE
  PS> $values = Get-RemoteRegistryValue -computer REMOTEPC -hkey 3 -path "Software\Microsoft\Powershell\"
  
.EXAMPLE
  PS> $values = Get-RemoteRegistryValue REMOTEPC 3 "Software\Microsoft\Powershell\"

.LINK
   www.poshsec.com
.NOTES
  This function is a utility function for the PoshSec module.
#>

function Get-RemoteRegistryValue {
  Param(
    [Parameter(Mandatory=$true,Position=1)]
    [string]$computer,
    
    [Parameter(Mandatory=$true,Position=2)]
    [int]$hkey,
    
    [Parameter(Mandatory=$true,Position=3)]
    [string]$path
  )
  
  [long]$hkeyint = 0
  
  switch($hkey)
  {
    1 { $hkeyint = 2147483648 }
    2 { $hkeyint = 2147483649 }
    3 { $hkeyint = 2147483650 }
    4 { $hkeyint = 2147483651 }
    5 { $hkeyint = 2147483653 }
    6 { $hkeyint = 2147483654 }
    default { $hkeyint = 2147483650 }
  }
  
  $reg = Get-RemoteRegistry $computer
  
  $rtn = @()
  
  if($reg) {
    $vals = $reg.EnumValues($hkeyint, $path).sNames
    if($vals) {
      foreach($val in $vals) {
        $value = $reg.GetStringValue($hkeyint, $path, $val)
        if($value){
          $regval = New-Object PSObject
          $regval | Add-Member -MemberType NoteProperty -Name "Computer" -Value $computer
          $regval | Add-Member -MemberType NoteProperty -Name "Path" -Value $path
          $regval | Add-Member -MemberType NoteProperty -Name "Name" -Value $val
          $regval | Add-Member -MemberType NoteProperty -Name "Value" -Value $value.sValue
          $rtn += $regval
        }        
      }
    }
  }  
  
  return $rtn
}