. .\WriteJson-ToRegistry.ps1

# my target registry paths
$HKCR_CLSID_PATH = "HKCR:\\WOW6432Node\\CLSID"
$HKLM_Wow6432Node_CLSID_PATH = "HKLM:\\SOFTWARE\\Classes\\Wow6432Node\\CLSID"
$HKLM_Classes_CLSID_PATH = "HKLM:\\SOFTWARE\\Wow6432Node\\Classes\\CLSID"

# load values from json file and write to the registry
$dir = "Registry"
(Get-ChildItem $dir *.json) | % {
    $json = Get-Content -Raw -Path $_.FullName | ConvertFrom-Json
    ($HKCR_CLSID_PATH, $HKLM_Wow6432Node_CLSID_PATH, $HKLM_Classes_CLSID_PATH) | % {
        WriteJson-ToRegistry $json $_
    }
}