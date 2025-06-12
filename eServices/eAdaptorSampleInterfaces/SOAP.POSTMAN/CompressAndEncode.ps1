$fileName = Read-Host "Please enter the path to the file you want to process"
$fileContent = Get-Content -Path $fileName
$bytesToCompress = [System.Text.Encoding]::UTF8.GetBytes($fileContent)
$memoryStream = [System.IO.MemoryStream]::new($bytesToCompress)
$gZipStream = [System.IO.Compression.GZipStream]::new($memoryStream, [System.IO.Compression.CompressionMode]::Compress)
$compressedBytes = $memoryStream.ToArray()
$gZipStream.Dispose()
$memoryStream.Dispose()
$encodedString = [System.Convert]::ToBase64String($compressedBytes)
Write-Host "The result is"
Write-Host $encodedString
Write-Host "Press any key to exit..."
[System.Console]::ReadKey()