Add-Type -AssemblyName System.Web

function ReplaceTokensWithValues {
  param(
    [Parameter(Mandatory=$true)]
    [string]$xmlFilePath,
    [Parameter(Mandatory=$true)]
    [string]$jsonFilePath)

  $xmlFile = Get-Item $xmlFilePath
  $jsonFile = Get-Item $jsonFilePath

  if ((Test-Path $xmlFile) -and (Test-Path $jsonFile)){

    $xmlFileContent = (Get-Content $xmlFilePath)
    $json = Get-Content -Raw -Path $jsonFile | Out-String | ConvertFrom-Json

    if ($json.PsObject.Properties) {

      $json.PsObject.Properties | % {
        $token = FormatToken($_.Name)
        $xmlFileContent = $xmlFileContent.Replace($token, [System.Web.HttpUtility]::HtmlEncode($_.Value))
      }

      $xmlFileContent | Set-Content $xmlFilePath
    }
  }
}

function FormatToken ([string]$name) {
  return ("{0}{1}{2}" -f "`${",$name,"}")
}

# $xmlFilePath = "D:\CargoWise.eHub.Products.Core\Bindings\Orchestrations\CargoWise.eHub.AlertService.Orchestrations.xml"
# $jsonFilePath = "D:\CargoWise.eHub.Products.Core\Bindings\Orchestrations\CargoWise.eHub.AlertService.Orchestrations.json"
# ReplaceTokensWithValues $xmlFilePath $jsonFilePath