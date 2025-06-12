$assemblyNamePattern = "^(?<typeName>.*),\s(?<assemblyName>.*),\sVersion=(?<versionNumber>.*),\sCulture=(?<culture>.*),\s+PublicKeyToken=(?<publicKeyToken>.*)$"
$passwordKeyPlaceHolder = ("{0}{1}:{2}{3}" -f "`${","key","please_replace_this_word_with_a_specific_password_key_name_that_a_keystore_can_provide_a_value","}")

Add-Type -AssemblyName "Microsoft.Build.Framework"

function Tokenize {
  param(
    [string]$xmlFilePath,
    $xpathJson,
    [string]$profile)

  [string[]]$assemblyNames = @()
  $xmlFile = Get-Item $xmlFilePath
  $jsonConfigFileName = [IO.Path]::ChangeExtension($xmlFile.Name, ".json")

  $jsonFileDirector = $xmlFile.DirectoryName
  if (-not [string]::IsNullOrEmpty($profile)) {
    $jsonFileDirector = Join-Path $xmlFile.DirectoryName $profile
    if (-not (Test-Path -Path $jsonFileDirector)) {
        New-Item -ItemType Directory -Path $jsonFileDirector
    }
  }

  $jsonConfigFilePath = Join-Path $jsonFileDirector $jsonConfigFileName

  if ($null -ne $error) { $error.Clear() }

  if (Test-Path $jsonConfigFilePath) {
    $jsonConfig = Get-Content -Raw -Path $jsonConfigFilePath -Encoding utf8 | Out-String | ConvertFrom-Json
  } else {
    $jsonConfig = @{}
  }

  $isDirty = $false
  [xml] $xml = Get-Content $xmlFile
  $xpathJson | % {

    $name = $_.Name
    $xpath = $_.XPath
    $isKey = $_.IsKey

    $counter = 0
    $xml | Select-Xml -XPath $xpath | % {

      if ($null -ne $_) {
        $isDirty = $true

        $counter++
        $key = $name

        if ($counter -gt 1) {
          $key = ("{0}_{1}" -f $name,$counter)
        }

        $token = FormatToken($key)
        $node = $_.Node
        $value = ""

        switch ($_.Node.NodeType) {
          "Element" {
            $value = $node.InnerText
            $node.InnerText = $token

            $vt = $node.Attributes["vt"]
            if (($null -ne $vt) -and ($vt.'#text' -eq "1")) {
              $vt.'#text' = "8"
            }
          }

          "Attribute" {
            $value = $node.'#text'
            $node.'#text' = $token
          }
        }

        if ($null -eq $jsonConfig."$key") {

          # config file doesn't contain the key yet
          if ($isKey) {
            $jsonConfig | Add-Member "$key" $passwordKeyPlaceHolder
          } else {
            $jsonConfig | Add-Member "$key" $value
          }

        } else {

          # they key had been processed and placed in the config file previously already
          [string]$existValue = $jsonConfig.PsObject.Properties[$name].Value

          if (($existValue -eq $key) -or ($existValue -eq $token)){

            if ($isKey) {
              $jsonConfig."$key" = $passwordKeyPlaceHolder
            } else {
              $jsonConfig."$key" = $value
            }

          } else {

            $value = $existValue
          }
        }

        # extract reference assemblyName from the binding config
        $match = $value | Select-String -Pattern $assemblyNamePattern
        if (($null -ne $match) -and $match.Matches.Success) {
            $assemblyDll = $match.Matches.Captures.Groups["assemblyName"].Value +".dll"
            if (-not $assemblyNames.Contains($assemblyDll)) {
                $assemblyNames += $assemblyDll
            }
        }
      }
    }
  }

  if ($isDirty) {
    # save the updated xml
    $xml.Save($xmlFilePath)

    # save the list of XPath-Value pair jsonConfig config to a file named after the input xml file
    $jsonConfig | ConvertTo-Json | Out-File $jsonConfigFilePath -Encoding utf8

    $log.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", "$xmlFilePath.FullName")
    $log.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", "$jsonConfigFilePath")
  }

  return $assemblyNames
}

function FormatToken ([string]$name) {
  return ("{0}{1}{2}" -f "`${",$name,"}")
}

function EscapeXml ([Xml.XmlNode]$node) {
  if ($node -ne $null) {
    $node.InnerText = $node.InnerXml
  }
}

function Log([string]$message) {
  if ($null -ne $log) {
    $log.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", $message)
  }
}

# $folder = "C:\BS\eServices\eHub\Products\AirMessaging\Deployment\Bindings"
# $AssemblyReferencesFilePath = "C:\BS\eServices\eHub\Products\AirMessaging\Deployment\AssemblyFiles.json"
# $assemblyNames = @()
# if (Test-Path $AssemblyReferencesFilePath) {
    # $assemblyNames = Get-Content -Raw -Path $AssemblyReferencesFilePath | Out-String | ConvertFrom-Json
# }

# $xpathfile = "c:\bs\eservices\ehub\devscripts\tokenize\config\biztalkbindinginfo-xpath.json"
# $xpathjson = get-content -raw -path $xpathfile | out-string | convertfrom-json
# (get-childitem -recurse -path $folder -include *.xml) | % {
    # $xmlFilePath = $_
    # $assemblyReferences = Tokenize -xmlFilePath $xmlFilePath -xpathJson $xpathJson

    # if ($null -ne $assemblyReferences) {
      # $assemblyReferences | % {
        # if (-not $assemblyNames.Contains($_)) {
            # $assemblyNames += $_
        # }
      # }
    # }
# }

# if ($assemblyNames.Count -gt 0) {
    # $assemblyNames | ConvertTo-Json | Set-Content $AssemblyReferencesFilePath
# }
