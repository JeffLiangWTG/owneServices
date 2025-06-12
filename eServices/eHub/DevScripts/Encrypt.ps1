function EncryptPlainText 
{
  [CmdletBinding()]
  param
  (
    [Parameter(Mandatory=$True,
      ValueFromPipeline=$True,
      ValueFromPipelineByPropertyName=$True,
      HelpMessage='Text to encrypt')]
    [string]$Text,

	[Parameter(Mandatory=$True,
      ValueFromPipeline=$True,
      ValueFromPipelineByPropertyName=$True,
      HelpMessage='Path to DAT.Integration.dll')]
    [string]$DatIntegrationAssemblyFilePath,

    [Parameter(Mandatory=$False,
      ValueFromPipeline=$True,
      ValueFromPipelineByPropertyName=$True,
      HelpMessage='File to store the encrypted text')]
    [string]$OutFile
  )

  begin 
  {
	Add-Type -Path $DatIntegrationAssemblyFilePath
	$encryptor = New-Object Dat.Integration.SecureStorage
  }

  process 
  {
    $encrypted = $encryptor.Encrypt($Text)
    if ($OutFile -ne "")
    {
        $encrypted | Out-File $OutFile
    }
    $encrypted
  }
}
