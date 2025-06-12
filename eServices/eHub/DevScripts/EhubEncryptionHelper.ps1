function Decrypt 
{
  [CmdletBinding()]
  param
  (
    [Parameter(Mandatory=$True,
      ValueFromPipeline=$True,
      ValueFromPipelineByPropertyName=$True,
      HelpMessage='Text to decrypt')]
    [string]$Text,

	[Parameter(Mandatory=$True,
      ValueFromPipeline=$True,
      ValueFromPipelineByPropertyName=$True,
      HelpMessage='Path to CargoWise.eServices.Encryption.Server.Decryptor.dll')]
    [string]$ServerDecryptorAssemblyFilePath
  )

  begin 
  {
	Add-Type -Path $ServerDecryptorAssemblyFilePath
  }

  process 
  {
    return [CargoWise.eServices.Encryption.Server.Decryptor.EhubServerDecryptor]::Decrypt($Text)
  }
}
