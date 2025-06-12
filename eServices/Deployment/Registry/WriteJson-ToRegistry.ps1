function Get-ObjectMembers {
    Param(
        [PSCustomObject]$obj
    )

    $obj | Get-Member -MemberType NoteProperty | % {
        $key = $_.Name
        [PSCustomObject]@{Key = $key; Value = $obj."$key"}
    }
}

function Parse-PropertyValue {
    Param ([String] $value) 

    if ([string]::IsNullOrWhiteSpace($value)) {
        [PSCustomObject]@{ type = "String"; value = $value }
    } else {
        $match = $value | Select-String -Pattern "(?<type>(binary|dword|expandstring|multistring|string)):(?<value>.*)"
        if (($null -ne $match) -and $match.Matches.Success) {
            
            $pType = $match.Matches.Captures.Groups["type"].Value
            $pValue = $match.Matches.Captures.Groups["value"].Value
            if ($pType -eq "dword") {
                $pValue = [System.Convert]::ToInt32($match.Matches.Captures.Groups["value"].Value, 16)
            }

            [PSCustomObject] @{ 
                type = $pType;
                value = $pValue }

        } else {
            [PSCustomObject]@{ type = "String"; value = $value }
        }
    }
}

function WriteJson-ToRegistry {
    Param(
        [PSCustomObject]$obj,
        [String]$parentPath
    )
    
    if ($null -ne $obj) {
        Get-ObjectMembers $obj | % {

            $childPath = ("{0}\\{1}" -f $parentPath, $_."key")
            if ($null -ne $_.value) {
                
                if ($_.value -is [String]) {
                    $property = Parse-PropertyValue $_."value"
                    New-ItemProperty -Path "$parentPath" -Name $_."key" -Value $property."value" -PropertyType $property."type" -Force | Out-Null
                    Write-Output "Create item: $property"

                } else {

                    New-Item -Path $childPath -Force | Out-Null
                    WriteJson-ToRegistry $_.value $childPath
                    Write-Output "Create path: $childPath"
                }
            }
        }
    }
}

if (Test-Path "HKCR:") {
    Remove-PSDrive -Name HKCR -Force -PSProvider Registry
}

New-PSDrive -Name HKCR -PSProvider Registry -Root HKEY_CLASSES_ROOT

# load values from json file and write to the registry
# example
# ---------------------------------------------------------------------------------------------
# $jsonFilePath = "C:\BS\eServices\eHub\Deploy\BizTalk\BizTalkAdapters\Scripts\FTPEx.json"
# $HKCR_CLSID_PATH = "HKCR:\\WOW6432Node\\CLSID"
# $HKLM_Wow6432Node_CLSID_PATH = "HKLM:\\SOFTWARE\\Classes\\Wow6432Node\\CLSID"
# $HKLM_Classes_CLSID_PATH = "HKLM:\\SOFTWARE\\Wow6432Node\\Classes\\CLSID"
# $json = Get-Content -Raw -Path $jsonFilePath | ConvertFrom-Json
# ($HKCR_CLSID_PATH, $HKLM_Wow6432Node_CLSID_PATH, $HKLM_Classes_CLSID_PATH) | % {
    # WriteJson-ToRegistry $json $_
# }
