Param(
    [Parameter(Mandatory=$true)]
    [string] $Folder,
    [string] $DependenciesFilePath = "BindingDependencies.txt"
)

$scriptDir = Split-Path $script:MyInvocation.MyCommand.Path

if ([string]::IsNullOrEmpty($Folder)) {
    $Folder = Join-Path $scriptDir "..\"
}

$assemblyNameGroup = "AssemblyName"
$assemblyFullNamePattern = "[\w\.]+,\s(?<$assemblyNameGroup>[\w\.]+),\sVersion=[\d\.]+,\sCulture=\w+,\sPublicKeyToken=[0-9a-fA-F]{16}"

if (Test-Path $DependenciesFilePath) { Remove-Item $DependenciesFilePath }

$assemblyNames = [System.Collections.ArrayList]@()

# extract dependencies defined in the binding info xml files
#
@("ReceivePorts", "SendPorts") | ForEach {
    $subDir = Join-Path $Folder $_

    dir $subDir *.xml -recurse | % {

        if (Test-Path  $_.FullName) {

            $match = Get-Content -Raw -Path  $_.FullName | Out-String | Select-String -Pattern $assemblyFullNamePattern
            $match.Matches | foreach {
                if ($_.Success) {

                    $assemblyName = "$($_.Groups[$assemblyNameGroup].Value).dll"
                    if (($null -ne $assemblyName) -and (-not $assemblyNames.Contains($assemblyName))) {
                        $assemblyNames.Add("$assemblyName")
                    }
                }
            }
        }
    }
}

$assemblyNames | Sort-Object | Out-File $DependenciesFilePath -Encoding ascii