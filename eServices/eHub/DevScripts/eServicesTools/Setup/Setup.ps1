Using Namespace System.IO

$ScriptPath = split-path -parent (split-path -parent $MyInvocation.MyCommand.Definition)
$env:PSModulePath -split ";" | Where-Object {
	$_ -like "$($env:USERPROFILE)*"
} | ForEach-Object {
	$RootDirectory = $_
	Get-ChildItem $ScriptPath -Exclude Setup, Samples | ?{ $_.PSIsContainer } | ForEach-Object {
		$SourceDirectory = $_
		$ModuleName = $_.Name
		$DestinationDirectory = "$RootDirectory\$ModuleName"
		Remove-Item $DestinationDirectory -Recurse -Force -ErrorAction Ignore
		New-Item $DestinationDirectory -ItemType Directory -Force | Out-Null
        Get-ChildItem -Path $SourceDirectory\* -Include "*.psd1", "*.psm1" | ForEach-Object { 
            Copy-Item -Path $_.FullName -Destination $DestinationDirectory
        }
	}
}