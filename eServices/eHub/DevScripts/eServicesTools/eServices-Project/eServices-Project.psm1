Using Namespace System.IO
Using Module eServices-TFS
Using Module eServices-DevEnv

class Project
{
    hidden [TFS] $TFS
    hidden [Settings] $Settings
    hidden [Context] $Context
    hidden [xml] $Content
    hidden [string] $Schema
    hidden [string] $Namespace
    hidden $ValidSchemas = @(
        "http://schemas.microsoft.com/developer/msbuild/2003#Project"
    )
    [bool] $IsValid = $true
    hidden [string] $ProjectDirectory
    hidden [string] $ProjectName
    hidden [string] $Path
    hidden [string] $RootDirectory
    hidden [PSCustomObject[]] $ProjectTypes = @()
    hidden [string] $TargetFrameworkVersion

    Project ([string] $projectPath, [string] $settingsPath)
    {
        try
        {
			Write-Host "Preparing project `"$projectPath`""
            $this.Context = [Context]::new()
            $this.Path = $projectPath
            $this.ProjectDirectory = [Path]::GetDirectoryName($projectPath)
            $this.ProjectName = [Path]::GetFileName($projectPath)
            $this.Settings = [Settings]::Load($settingsPath)
            $this.TFS = [TFS]::new($this.Settings.TFSServer, $this.Settings.WorkspaceName)
            [xml] $this.Content = Get-Content $projectPath
            $this.ValidateProject()
            $this.RootDirectory = $this.TFS.GetLocalItem($this.Settings.Branch)
            Select-Xml -Xml $this.Content -XPath "s0:Project/s0:PropertyGroup/s0:TargetFrameworkVersion" -Namespace @{ "s0" = $this.NameSpace } | ForEach-Object {
                $this.TargetFrameworkVersion = $_.Node.InnerText
            }
            Select-Xml -Xml $this.Content -XPath "s0:Project/s0:PropertyGroup/s0:ProjectTypeGuids" -Namespace @{ "s0" = $this.NameSpace } | ForEach-Object {
                $_.Node.InnerText -split ";" | ForEach-Object { 
                    $this.ProjectTypes += [GLOBALSETTINGS]::GetProjectType($_)
                }
            }
        }
        catch [Exception]
        {
            Write-Host $_
            $this.IsValid = $false
        }
    }

    [string] GetDeploymentType()
    {
        $output = "Inconclusive"
        $this.ProjectTypes | Where-Object { $_.DeploymentType -ne "" } | ForEach-Object {
            $output = $_.DeploymentType
        }
        return $output
    }

    hidden [void] ValidateProject()
    {
        $this.Namespace = $this.Content.DocumentElement.NamespaceURI
        $this.Schema = "$($this.Namespace)#$($this.Content.DocumentElement.LocalName)"
        if ($this.Schema -notin $this.ValidSchemas)
        {
            Write-Host Red "The specified file is not valid Project."
            $this.IsValid = $false
        }
    }

    hidden [string] GetRelativePath([string] $from, [string] $to)
    {
         Set-Location (Get-Item -Path $from).Directory.FullName
         $RelativePath = $to | Resolve-Path -Relative
         Set-Location $this.Context.CWD
         return $RelativePath
    }

    [void] IncludeDeployment([string] $deploymentType)
    {
         $PG = $this.Content.CreateElement("PropertyGroup", $this.NameSpace)
         $ExistingPGs = @()
         $this.Content.DocumentElement.AppendChild($PG) | Out-Null
         $Items = @(Get-Item "$($this.GetEnvironmentVariable('DeploymentDirectory', @($deploymentType)))\*.proj")
         $Items += Get-Item $this.GetEnvironmentVariable('DeploymentCommonScript', @())
         $Items | ForEach-Object {
             $Item = $_
             $PropertyElementName = [GLOBALSETTINGS]::GetDeploymentVariable($deploymentType, $Item.Name)
             $RelativePath = $this.GetRelativePath($this.Path, $Item.FullName)
             $PropertyNode = Select-Xml -Xml $this.Content -XPath "s0:Project/s0:PropertyGroup/s0:$PropertyElementName" -Namespace @{ "s0" = $this.NameSpace }
             if ($PropertyNode -eq $null)
             {
                 $PropertyNode = $this.Content.CreateElement($PropertyElementName, $this.NameSpace)
                 $PropertyNode.InnerText = "$RelativePath"
                 $PG.AppendChild($PropertyNode) | Out-Null
             }
             else
             {
                 $ExistingPGs += $PropertyNode.Node.ParentNode
                 $PG.AppendChild($PropertyNode.Node) | Out-Null
                 if ($PropertyNode.Node.InnerText -ne $RelativePath)
                 {
                     $PropertyNode.Node.InnerText = "$RelativePath"
                 }
             }
             if ($_.Extension -eq ".proj")
             {
                 Select-Xml -Xml $this.Content -XPath "s0:Project/s0:Import" -Namespace @{ "s0" = $this.NameSpace } | Where-Object {
                     $_.Node.Attributes["Project"].'#text' -eq "`$($PropertyElementName)" 
                 } | ForEach-Object {
                     $this.Content.DocumentElement.RemoveChild($_.Node) | Out-Null
                 }
                 $ImportNode = $this.Content.CreateElement("Import", $this.NameSpace)
                 $ImportNode.Attributes.Append($this.Content.CreateAttribute("Project")) | Out-Null
                 $ImportNode.SetAttribute("Project", "`$($PropertyElementName)")
                 $ImportNode.Attributes.Append($this.Content.CreateAttribute("Condition")) | Out-Null
                 $ImportNode.SetAttribute("Condition", "Exists('`$($PropertyElementName)')")
                 $this.Content.DocumentElement.AppendChild($ImportNode) | Out-Null
             }
         }
         if (-not ($PG.HasChildNodes))
         {
             $this.Content.DocumentElement.RemoveChild($PG) | Out-Null
         }
         $ProcessedPGs = @()
         $ExistingPGs | ForEach-Object {
             if (-not ($ProcessedPGs -contains $_) -and -not ($_.HasChildNodes))
             {
                 $this.Content.DocumentElement.RemoveChild($_) | Out-Null
                 $ProcessedPGs += $_
             }
         }
         $this.SaveContent()
         $this.CreateDefaultProfileIfNotExists() | Out-Null
    }

    hidden [string] CreateDefaultProfileIfNotExists()
    {
         $DeploymentDirectory = "$($this.ProjectDirectory)\Deployment"
         if (-not (Test-Path $DeploymentDirectory))
         {
             New-Item -ItemType Directory -Path $DeploymentDirectory
         }
         if (-not (Test-Path "$DeploymentDirectory\Profiles"))
         {
             New-Item -ItemType Directory -Path "$DeploymentDirectory\Profiles"
         }
         if (-not (Test-Path "$DeploymentDirectory\Deployment.props"))
         {
             $PropsFile = New-Item -ItemType File -Path "$DeploymentDirectory\Deployment.props"
             @'
<?xml version="1.0" encoding="utf-8" ?>
<Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
	<PropertyGroup>
        <_DeploymentPropsImported>$true</_DeploymentPropsImported>
		<!-- Add the required properties -->
	</PropertyGroup>
</Project>
'@ | Out-File $PropsFile
			$this.TFS.CheckOut($PropsFile)
         }
         $PropsNode = Select-Xml -Xml $this.Content -XPath "s0:Project/s0:ItemGroup/*" -Namespace @{ "s0" = $this.NameSpace } | Where-Object {
             $_.Node.Attributes["Include"].'#text' -eq "Deployment\Deployment.props"
         }
         if ($PropsNode -eq $null)
         {
             $PropsNode = $this.Content.CreateElement("None", $this.NameSpace)
             $PropsNode.Attributes.Append($this.Content.CreateAttribute("Include"))
             $PropsNode.SetAttribute("Include", "Deployment\Deployment.props")
             (Select-Xml -Xml $this.Content -XPath "s0:Project/s0:ItemGroup[1]" -Namespace @{ "s0" = $this.NameSpace }).Node.AppendChild($PropsNode) | Out-Null
			 $this.Content.Save($this.Path)
         }
         return $DeploymentDirectory
    }

    hidden [string] GetEnvironmentVariable([string] $variableName, [Array] $injections)
    {
        return [GLOBALSETTINGS]::GetEnvironmentVariable($this.TFS, $this.Settings.Branch, $variableName, $injections)
    }

    hidden [string] GetRelativeEnvironmentVariable([string] $variableName, [Array] $injections)
    {
        return $this.GetRelativePath($this.Path, $this.GetEnvironmentVariable($variableName, $injections));
    }

    hidden [void] SaveContent()
    {
        $this.TFS.CheckOut($this.Path)
        $this.Content.Save($this.Path)
    }

    [void] OrganiseAssemblyInfo([string] $infoVersion, [string] $assemblyVersion)
    {
         $InfoNode = Select-Xml -Xml $this.Content -XPath "s0:Project/s0:ItemGroup/*" -Namespace @{ "s0" = $this.NameSpace } | Where-Object { 
             $_.Node.Attributes["Include"].'#text' -match ".*?CommonAssemblyInfo.*?" 
         }
         $RelativePath = $this.GetRelativeEnvironmentVariable("CommonAssemblyInfo", @($infoVersion))
         if ($RelativePath -eq "") { break }
         if ($InfoNode -eq $null)
         {
             $InfoNode = $this.Content.CreateElement("Compile", $this.NameSpace)
             $InfoNode.Attributes.Append($this.Content.CreateAttribute("Include"))
             $InfoNode.SetAttribute("Include", $RelativePath)
             $LinkNode = $this.Content.CreateElement("Link", $this.NameSpace)
             $LinkNode.InnerText = "Properties\CommonAssemblyInfo$infoVersion.cs"
             $InfoNode.AppendChild($LinkNode)
             (Select-Xml -Xml $this.Content -XPath "s0:Project/s0:ItemGroup[1]" -Namespace @{ "s0" = $this.NameSpace }).Node.AppendChild($InfoNode) | Out-Null
         }
         else
         {
             $InfoNode.Node.SetAttribute("Include", $RelativePath)
             $InfoNode.Node.ChildNodes | Where-Object { $_.LocalName -eq "Link" } | ForEach-Object { 
                 ([System.Xml.XmlElement] $_).InnerText = "Properties\CommonAssemblyInfo$infoVersion.cs" 
             }
         }        
         $AssemblyInfoPattern = '^\[assembly: (.*)?\((.*)?\)\]'
         $AssemblyInfo = (Get-Content "$($this.ProjectDirectory)\Properties\AssemblyInfo.cs")
         (Get-Content $($this.GetEnvironmentVariable("CommonAssemblyInfo", @($infoVersion)))) | ForEach-Object {
             if ($_ -match $AssemblyInfoPattern)
             {
                 $AssemblyInfo = $AssemblyInfo -replace "^\[assembly: $($Matches[1])(.*)?\]", ''
             }
         }
         $AssemblyInfo = $AssemblyInfo -replace "^\[assembly: AssemblyVersion\((.*)?\)\]", "[assembly: AssemblyVersion(`"$assemblyVersion`")]"
         $AssemblyInfo = $AssemblyInfo -replace "^\[assembly: AssemblyFileVersion\((.*)?\)\]", "[assembly: AssemblyFileVersion(`"$assemblyVersion`")]"
         $AssemblyInfo = $AssemblyInfo -replace "^//(.*)", ''
         $this.TFS.CheckOut("$($this.ProjectDirectory)\Properties\AssemblyInfo.cs")
         Set-Content -Value $AssemblyInfo -Path "$($this.ProjectDirectory)\Properties\AssemblyInfo.cs"
         $this.SaveContent()
    }

    [void] Sign()
    {
         $SignNode = Select-Xml -Xml $this.Content -XPath "s0:Project/s0:ItemGroup/*" -Namespace @{ "s0" = $this.NameSpace } | Where-Object { 
             $_.Node.Attributes["Include"].'#text' -match ".*?EDI-Release-private.*?" 
         }
         $RelativePath = $this.GetRelativeEnvironmentVariable("SignatureKey", @())
         if ($RelativePath -eq "") { break }
         if ($SignNode -eq $null)
         {
             $SignNode = $this.Content.CreateElement("None", $this.NameSpace)
             $SignNode.Attributes.Append($this.Content.CreateAttribute("Include"))
             $SignNode.SetAttribute("Include", $RelativePath)
             $LinkNode = $this.Content.CreateElement("Link", $this.NameSpace)
             $LinkNode.InnerText = "EDI-Release-private.snk"
             $SignNode.AppendChild($LinkNode)
             (Select-Xml -Xml $this.Content -XPath "s0:Project/s0:ItemGroup[1]" -Namespace @{ "s0" = $this.NameSpace }).Node.AppendChild($SignNode) | Out-Null
         }
         else
         {
             $SignNode.Node.SetAttribute("Include", $RelativePath)
             $SignNode.Node.ChildNodes | Where-Object { $_.LocalName -eq "Link" } | ForEach-Object { 
                 ([System.Xml.XmlElement] $_).InnerText = "EDI-Release-private.snk" 
             }
         }
         
         $KeyNode = Select-Xml -Xml $this.Content -XPath "s0:Project/s0:PropertyGroup/s0:AssemblyOriginatorKeyFile" -Namespace @{ "s0" = $this.NameSpace }
         if ($KeyNode -eq $null)
         {
             $PG = $this.Content.CreateElement("PropertyGroup", $this.NameSpace)
             $KeyNode = $this.Content.CreateElement("AssemblyOriginatorKeyFile", $this.NameSpace)
             $KeyNode.InnerText = $RelativePath
             $PG.AppendChild($KeyNode) | Out-Null
             $this.Content.DocumentElement.AppendChild($PG) | Out-Null
         }
         else
         {
             $KeyNode.Node.InnerText = $RelativePath
         }

         $SignAssemblyNode = Select-Xml -Xml $this.Content -XPath "s0:Project/s0:PropertyGroup/s0:SignAssembly" -Namespace @{ "s0" = $this.NameSpace }
         if ($SignAssemblyNode -eq $null)
         {
             $PG = $this.Content.CreateElement("PropertyGroup", $this.NameSpace)
             $SignAssemblyNode = $this.Content.CreateElement("SignAssembly", $this.NameSpace)
             $SignAssemblyNode.InnerText = "true"
             $PG.AppendChild($SignAssemblyNode) | Out-Null
             $this.Content.DocumentElement.AppendChild($PG) | Out-Null
         }
         else
         {
             if ($null -ne $SignAssemblyNode.Node -and $SignAssemblyNode.Node.InnerText -ne "true") {
                 $SignAssemblyNode.Node.InnerText = "true"
             }
         }
         $this.SaveContent()
    }

    [void] IncludeLogging()
    {
        $LogConfigNode = Select-Xml -Xml $this.Content -XPath "s0:Project/s0:ItemGroup/*" -Namespace @{ "s0" = $this.NameSpace } | Where-Object { 
            $_.Node.Attributes["Include"].'#text' -match ".*?log4net.config" 
        }
        if ($LogConfigNode -eq $null)
        {
            $LogConfigNode = $this.Content.CreateElement("Content", $this.NameSpace)
            $LogConfigNode.Attributes.Append($this.Content.CreateAttribute("Include"))
            $LogConfigNode.SetAttribute("Include", "$($this.ProjectName).log4net.config")
            (Select-Xml -Xml $this.Content -XPath "s0:Project/s0:ItemGroup[1]" -Namespace @{ "s0" = $this.NameSpace }).Node.AppendChild($LogConfigNode) | Out-Null
            $ConfigFileName = "$($this.ProjectDirectory)\$($this.ProjectName).log4net.config"
            $this.CreateFile($ConfigFileName, @'
<?xml version="1.0" encoding="utf-8" ?>
<log4net>
	<appender name="RollingFile" type="log4net.Appender.RollingFileAppender">
		<file value="logs/Logs.txt" />
		<appendToFile value="true" />
		<maximumFileSize value="10MB" />
		<maxSizeRollBackups value="5" />
		<layout type="log4net.Layout.PatternLayout">
			<conversionPattern value="%date %level %logger - %message%newline" />
		</layout>
	</appender>
	<root>
		<level value="All" />
		<appender-ref ref="RollingFile" />
	</root>
</log4net>
'@)
        }
        $this.ReferencePackage("Common.Logging", "2.2.0.0", ($this.TargetFrameworkVersion -replace "[v.]", ""))
        $this.ReferencePackage("Common.Logging.Core", "2.2.0.0", ($this.TargetFrameworkVersion -replace "[v.]", ""))
        $this.ReferencePackage("Common.Logging.Log4Net1211", "2.2.0.0", ($this.TargetFrameworkVersion -replace "[v.]", ""))
        $this.ReferencePackage("log4net", "1.2.11.0", ($this.TargetFrameworkVersion -replace "[v.]", ""))
        $this.SaveContent()
    }

    hidden [void] ReferencePackage([string] $packageName, [string] $version, [string] $netVersion)
    {
        $Node = Select-Xml -Xml $this.Content -XPath "s0:Project/s0:ItemGroup/Reference" -Namespace @{ "s0" = $this.NameSpace } | Where-Object { 
            $_.Node.Attributes["Include"].'#text' -match "$packageName,.*?" 
        }
        if ($Node -eq $null)
        {
            $Package = [GLOBALSETTINGS]::GetPackage($packageName)
            $Node = $this.Content.CreateElement("Reference", $this.NameSpace)
            $Node.Attributes.Append($this.Content.CreateAttribute("Include"))
            $Node.SetAttribute("Include", $Package.Include -f $version)
            $PrivateNode = $this.Content.CreateElement("Private", $this.NameSpace)
            $PrivateNode.InnerText = "True"
            $Node.AppendChild($PrivateNode) | Out-Null
            $HintPathNode = $this.Content.CreateElement("Private", $this.NameSpace)
            $HintPathNode.InnerText = "$($this.GetRelativeEnvironmentVariable("Packages", @()))\$($Package.HintPath -f $netVersion)"
            $Node.AppendChild($HintPathNode) | Out-Null
            (Select-Xml -Xml $this.Content -XPath "s0:Project/s0:ItemGroup[1]" -Namespace @{ "s0" = $this.NameSpace }).Node.AppendChild($Node) | Out-Null
        }
    } 

    hidden [void] CreateFile([string] $fileName, [string] $content)
    {
        if (-not (Test-Path $fileName))
        {
            $file = New-Item -ItemType File -Path $fileName
            $content | Out-File $file
            $this.TFS.CheckOut($file)
        }
    }
    
    [void] AddDeploymentProfile([string] $profileName, [string] $deploymentType)
    {
         $DeploymentDirectory = $this.CreateDefaultProfileIfNotExists()
		 $ProfileDirectory = "$DeploymentDirectory\Profiles\$ProfileName"
         if (-not (Test-Path $ProfileDirectory))
         {
             New-Item -ItemType Directory -Path $ProfileDirectory
         }
         $this.CreateFile("$ProfileDirectory\Deployment.props", @'
<?xml version="1.0" encoding="utf-8" ?>
<Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
	<PropertyGroup>
        <_ProfilePropsImported>$true</_ProfilePropsImported>
		<!-- Add the required properties -->
	</PropertyGroup>
</Project>
'@)
         $PropsNode = Select-Xml -Xml $this.Content -XPath "s0:Project/s0:ItemGroup/*" -Namespace @{ "s0" = $this.NameSpace } | Where-Object {
             $_.Node.Attributes["Include"].'#text' -eq "Deployment\Profiles\$ProfileName\Deployment.props"
         }
         if ($PropsNode -eq $null)
         {
             $PropsNode = $this.Content.CreateElement("None", $this.NameSpace)
             $PropsNode.Attributes.Append($this.Content.CreateAttribute("Include"))
             $PropsNode.SetAttribute("Include", "Deployment\Profiles\$ProfileName\Deployment.props")
             (Select-Xml -Xml $this.Content -XPath "s0:Project/s0:ItemGroup[1]" -Namespace @{ "s0" = $this.NameSpace }).Node.AppendChild($PropsNode) | Out-Null
         }
		 $ConfigFileName = [GLOBALSETTINGS]::GetDeploymentVariable($deploymentType, "ConfigFile")
         if ($ConfigFileName -ne $null)
         {
             $this.CreateFile("$ProfileDirectory\$ConfigFileName", @'
<?xml version="1.0" encoding="UTF-8"?>
<configuration xmlns:xdt="http://schemas.microsoft.com/XML-Document-Transform">
	<!-- Add the required transforms here -->
</configuration>
'@)
         }
         $ConfigNode = Select-Xml -Xml $this.Content -XPath "s0:Project/s0:ItemGroup/*" -Namespace @{ "s0" = $this.NameSpace } | Where-Object {
             $_.Node.Attributes["Include"].'#text' -eq "Deployment\Profiles\$ProfileName\$ConfigFileName"
         }
         if ($ConfigNode -eq $null)
         {
             $ConfigNode = $this.Content.CreateElement("Content", $this.NameSpace)
             $ConfigNode.Attributes.Append($this.Content.CreateAttribute("Include"))
             $ConfigNode.SetAttribute("Include", "Deployment\Profiles\$ProfileName\$ConfigFileName")
             (Select-Xml -Xml $this.Content -XPath "s0:Project/s0:ItemGroup[1]" -Namespace @{ "s0" = $this.NameSpace }).Node.AppendChild($ConfigNode) | Out-Null
         }
         $this.Content.Save($this.Path)
    }
}

class Context
{
    [string] $CWD

    Context()
    {
        $this.CWD = Get-Location
    }
}

class Settings
{
    [string] $Branch
    [string] $TFSServer
    [string] $WorkspaceName

    static [Settings] Load([string] $settingsFilePath)
    {
        $properties = Get-Content $settingsFilePath | ConvertFrom-StringData
        $obj = [Settings]::new()
        $properties.Keys | ForEach-Object {
            $name = $_
            $value = $properties.$name 
            $obj.$name = $value
        }
        return $obj
    }
}