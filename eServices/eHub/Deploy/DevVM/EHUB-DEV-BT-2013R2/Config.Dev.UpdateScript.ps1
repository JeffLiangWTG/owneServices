<#
    .SYNOPSIS
      Applies Dev XDT file to configs referenced by BizTalk.
    .DESCRIPTION
#>

add-type -AssemblyName "Microsoft.Web.XmlTransform, Version=2.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL"
$xws = new-object "Xml.XmlWriterSettings"
$xws.Indent = $true
$xws.OmitXmlDeclaration = $true

function ApplyTransformation ([string]$config, [string]$transform) {
    if (!(Test-Path $config)) { "<configuration><configSections/><eHub/></configuration>" | Out-File $config }
    [xml]$cfg = Get-Content $config
    if ($cfg.configuration.configSections -eq $null) { $cfg["configuration"].InsertAfter($cfg.CreateElement("configSections"), $null) }
    $trans = New-Object 'Microsoft.Web.XmlTransform.XmlTransformation' -ArgumentList $transform
    $trans.Apply($cfg) > $null
    $cfg.Save($config)
}

if (!(Test-Path $env:BTSINSTALLPATH)) { Write-Error "BizTalk not installed" }

$TransformBT = $PSScriptRoot + "\Config.Dev.BizTalk.xdt"
$TransformMachine = $PSScriptRoot + "\Config.Dev.machine.xdt"

ApplyTransformation ($env:windir + "\Microsoft.NET\Framework\v2.0.50727\CONFIG\machine.config") $TransformMachine
ApplyTransformation ($env:windir + "\Microsoft.NET\Framework\v4.0.30319\Config\machine.config") $TransformMachine
ApplyTransformation ($env:windir + "\Microsoft.NET\Framework64\v2.0.50727\CONFIG\machine.config") $TransformMachine
ApplyTransformation ($env:windir + "\Microsoft.NET\Framework64\v4.0.30319\Config\machine.config") $TransformMachine

ApplyTransformation ($env:BTSINSTALLPATH + "BTSNTSvc.exe.config") $TransformBT
ApplyTransformation ($env:BTSINSTALLPATH + "BTSNTSvc64.exe.config") $TransformBT

ApplyTransformation ($env:windir + "\SysWOW64\inetsrv\w3wp.exe.config") $TransformBT
ApplyTransformation ($env:windir + "\System32\inetsrv\w3wp.exe.config") $TransformBT
