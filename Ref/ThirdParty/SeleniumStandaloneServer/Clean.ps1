$userProfile = [System.Environment]::GetFolderPath("UserProfile")
$directory = "$userProfile\AppData\Local\Temp"
$logFile = "C:\Selenium\clean.log"

$currentDate = Get-Date

$folders = Get-ChildItem -Path $directory -Directory | Where-Object { $_.Name -like "scoped_dir*" -and $_.LastWriteTime -lt $currentDate.AddDays(-30) }

foreach ($folder in $folders) {
    Remove-Item -Path $folder.FullName -Recurse -Force
    $logEntry = "Deleted: $($folder.FullName) at $($currentDate)"
    Write-Output $logEntry
}

$summary = "Deleted $($folders.Count) folders older than 30 days from $directory at $($currentDate)"
Write-Output $summary
Add-Content -Path $logFile -Value $summary