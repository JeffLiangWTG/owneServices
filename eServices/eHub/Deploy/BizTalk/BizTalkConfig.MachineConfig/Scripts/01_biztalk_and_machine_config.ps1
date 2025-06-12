Param([Parameter(Mandatory=$false)][string] $Profile)

$xdtPath = Join-Path $PSScriptRoot "Profiles"
if (![string]::IsNullOrEmpty($Profile)) {
  $xdtPath = Join-Path $xdtPath $Profile
}

$xmlTransformAssemblyFilePath = Join-Path $PSScriptRoot "Microsoft.Web.XmlTransform.dll"
Add-Type -Path "$xmlTransformAssemblyFilePath"

$xws = new-object "Xml.XmlWriterSettings"
$xws.Indent = $true
$xws.OmitXmlDeclaration = $true

function ApplyTransformation ([string]$config, [string]$transform) {
  if (!(Test-Path $config)) {
    "<configuration><configSections/><eHub/></configuration>" | Out-File $config 
  }

  [xml]$cfg = Get-Content $config
  if ($cfg.configuration.configSections -eq $null) {
    $cfg["configuration"].InsertAfter($cfg.CreateElement("configSections"), $null)
  }

  $trans = New-Object 'Microsoft.Web.XmlTransform.XmlTransformation' -ArgumentList $transform
  $trans.Apply($cfg) > $null
  $cfg.Save($config)

  $config | Write-Output
}

if (!(Test-Path $env:BTSINSTALLPATH)) { Write-Error "BizTalk not installed" }

$TransformBizTalk = $xdtPath + "\Config.BizTalk.xdt"
$TransformMachine = $xdtPath + "\Config.Machine.xdt"

ApplyTransformation ($env:windir + "\Microsoft.NET\Framework\v2.0.50727\CONFIG\machine.config") $TransformMachine
ApplyTransformation ($env:windir + "\Microsoft.NET\Framework\v4.0.30319\Config\machine.config") $TransformMachine
ApplyTransformation ($env:windir + "\Microsoft.NET\Framework64\v2.0.50727\CONFIG\machine.config") $TransformMachine
ApplyTransformation ($env:windir + "\Microsoft.NET\Framework64\v4.0.30319\Config\machine.config") $TransformMachine

ApplyTransformation ($env:BTSINSTALLPATH + "BTSNTSvc.exe.config") $TransformBizTalk
ApplyTransformation ($env:BTSINSTALLPATH + "BTSNTSvc64.exe.config") $TransformBizTalk

ApplyTransformation ($env:windir + "\SysWOW64\inetsrv\w3wp.exe.config") $TransformBizTalk
ApplyTransformation ($env:windir + "\System32\inetsrv\w3wp.exe.config") $TransformBizTalk
