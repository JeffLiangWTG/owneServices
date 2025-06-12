$encodedString = Read-Host "Please enter the content you want to process"
$compressedBytes = [System.Convert]::FromBase64String($encodedString)
$memoryStream = [System.IO.MemoryStream]::new($compressedBytes)
$gZipStream = [System.IO.Compression.GZipStream]::new($memoryStream, [System.IO.Compression.CompressionMode]::Decompress)
$streamReader = [System.IO.StreamReader]::new($memoryStream, [System.Text.Encoding]::UTF8)
$decompressedString = $streamReader.ReadToEnd()
$streamReader.Dispose()
$gZipStream.Dispose()
$memoryStream.Dispose()
Write-Host "The result is"
Write-Host $decompressedString
Write-Host "Press any key to exit..."
[System.Console]::ReadKey()