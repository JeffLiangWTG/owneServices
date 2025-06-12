Param
(
    [parameter (mandatory = $true)]
    [string] $Type,
    [parameter (mandatory = $true)]
    [string] $Name,
    [parameter (mandatory = $true)]
    [string] $Output
)

function Get-ProjectFile ([string] $Path) {
    if (!(Test-Path $Path)) {
        Write-Host "Creating folder: $Path"
        New-Item $Path -ItemType Directory -Force | out-null
    }

    Get-Item "$Path/*.csproj"
}

$ProjectFile = Get-ProjectFile $Output

$TestName = "$Name.Tests"

if (!($ProjectFile)) {
    dotnet new $Type --name $Name --output $Output
    $ProjectFile = Get-ProjectFile $Output
    
    Remove-Item $Output\*.cs

    New-Item "$Output\Properties" -ItemType Directory -Force
    Add-Content "$Output\Properties\AssemblyInfo.cs" `
        'using System.Runtime.CompilerServices;','',"[assembly: InternalsVisibleTo(`"$TestName`")]"
    
    if ($Type -eq "console") {
        Add-Content "$Output\Properties\AssemblyInfo.cs" "[assembly: InternalsVisibleTo(`"OcmPoc.Common.Tests`")]"
    }

    $projectXml = [Xml] (Get-Content $ProjectFile)
    $langVersion = $projectXml.CreateElement("LangVersion")
    $langVersion.InnerText = "latest"
    $projectXml.Project.PropertyGroup.AppendChild($langVersion)
    $projectXml.Save($ProjectFile)

    dotnet sln OcmPoc.sln add $ProjectFile
}

$TestPath = "Tests\$Output"
$TestProjectFile = Get-ProjectFile $TestPath

if (!($TestProjectFile)) {
    dotnet new xunit --name $TestName --output $TestPath
    $TestProjectFile = Get-ProjectFile $TestPath

    Remove-Item $TestPath\*.cs
    New-Item $TestPath\Unit -ItemType Directory -Force

    dotnet add $TestProjectFile reference $ProjectFile
    dotnet add $TestProjectFile package FluentAssertions
    dotnet add $TestProjectFile package Moq
    dotnet sln OcmPoc.sln add $TestProjectFile
}
