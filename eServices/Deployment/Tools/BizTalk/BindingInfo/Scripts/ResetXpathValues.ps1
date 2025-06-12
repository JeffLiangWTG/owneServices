<#
    .SYNOPSIS
      Given an xml file and a template jsonConfig file which contains an array of XPaths, this script will reset the value
      of the selected node by the given XPath to empty string. The original values are saved to ~.json file so that
      a custom value can be configured and used to build the bindings later on.

      This script works on a given folder and process all xml files found.
    .PARAMETER Folder
      Path to the xml file which the values to the list of xpath specified in the $XPathJsonFile will be reset
    .PARAMETER XPathJsonFile
      Contains a list of xpath paths

.EXAMPLE
    $Folder = "c:\SandBox\eServices\DATify\eHub\Deploy\BizTalkServer\BizTalk EDI Application\Bindings"
    $XPathJsonFile="C:\SandBox\eServices\DATify\eHub\Deployment\BizTalkServer\Tools\BindingInfo\Templates\XPath-Bindings.json"
#>

Param([Parameter(Mandatory=$true)][string] $Folder, [Parameter(Mandatory=$true)][string] $XPathJsonFile)
dir $Folder *.xml -recurse | % {

    $xmlPath = $_.FullName
    $jsonXPathConfigFile = [IO.Path]::ChangeExtension($_.Name, ".json")
    $jsonXPathConfigFilePath = Join-Path $_.DirectoryName $jsonXPathConfigFile
    $jsonConfig = @()

    [xml]$item = get-content $xmlPath
    if (test-path $XPathJsonFile) {
        Get-Content -Raw -Path $XPathJsonFile | Out-String | ConvertFrom-Json | ForEach-Object {

        $_.XPath | foreach {

            $xpath = $_
            $item | Select-Xml -XPath $xpath | ForEach-Object {

            if ($null -ne $_) {
                $node = $_.Node
                $Value = ""

                switch ($_.Node.NodeType) {
                    "Element" {
                        $Value = $node.InnerText
                        $node.InnerText = ""
                    }
                    "Attribute" {
                        $Value = $node.'#text'
                        $node.'#text' = ""
                    }
                }

                $jsonConfig += [pscustomobject]@{
                    'XPath'=$xpath;
                    'Value'=$Value
                }
             }
        }}}

        # save the updated xml
        $item.Save($xmlPath)

        # save the list of XPath-Value pair jsonConfig config to a file named after the input xml file
        $jsonConfig | ConvertTo-Json | % { [System.Text.RegularExpressions.Regex]::Unescape($_) } | Out-File $jsonXPathConfigFilePath -Encoding utf8
    }
}

function EscapeXml ([Xml.XmlNode]$node)
{
    if ($node -ne $null) { $node.InnerText = $node.InnerXml }
}
