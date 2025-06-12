function ZIP-Decompress
{
  [CmdletBinding()]
  param
  (
    [Parameter(Mandatory=$True,
      ValueFromPipeline=$True,
      ValueFromPipelineByPropertyName=$True,
      HelpMessage='GZIP file to be decompressed')]
    [Alias('GZIPSource')]
    [object]$source
  )

  begin 
  {
  }

  process 
  {
    if ($source.ServerItem.ToString().EndsWith(".gz"))
    {
      $outFile = [System.IO.Path]::GetTempFileName()
      $input = New-Object System.IO.FileStream $source.LocalFile, ([IO.FileMode]::Open), ([IO.FileAccess]::Read), ([IO.FileShare]::Read)
      $output = New-Object System.IO.FileStream $outFile, ([IO.FileMode]::Create), ([IO.FileAccess]::Write), ([IO.FileShare]::None)
      $gzipStream = New-Object System.IO.Compression.GzipStream $input, ([IO.Compression.CompressionMode]::Decompress)

      $buffer = New-Object byte[](1024)
      while($true){
          $read = $gzipstream.Read($buffer, 0, 1024)
          if ($read -le 0){break}
          $output.Write($buffer, 0, $read)
      }

      $gzipStream.Close()
      $output.Close()
      $input.Close()
      return New-Object pscustomobject –property @{
        ServerItem = $item
        LocalFile = $outFile
      }
    }
    return $source
  }
}