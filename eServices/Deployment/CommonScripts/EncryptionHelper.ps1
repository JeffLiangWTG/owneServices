function GetSecureString
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$True,
        Position=0,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Encrypted Password')]
		[AllowEmptyString()]
        [string]$EncryptedPassword,

		[Parameter(Mandatory=$True,
		  ValueFromPipeline=$True,
		  ValueFromPipelineByPropertyName=$True,
		  HelpMessage='Path to DAT.Integration.dll')]
		[string]$DatIntegrationAssemblyFilePath,

		[Parameter(Mandatory=$True,
		  ValueFromPipeline=$True,
		  ValueFromPipelineByPropertyName=$True,
		  HelpMessage='Logger')]
		$Logger
    )

    begin
    {
    }

    process
    {
        if ($EncryptedPassword -eq '') { return '' }
		$Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", "[$env:ComputerName]	Getting Secure String" )
        return $EncryptedPassword | Decrypt -Logger $Logger -DatIntegrationAssemblyFilePath $DatIntegrationAssemblyFilePath | ConvertTo-SecureString -AsPlainText -Force
    }

    end
    {
    }
}

function Decrypt
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$True,
        Position=0,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Encrypted Password')]
		[AllowEmptyString()]
        [string]$EncryptedPassword,

		[Parameter(Mandatory=$True,
		  ValueFromPipeline=$True,
		  ValueFromPipelineByPropertyName=$True,
		  HelpMessage='Path to DAT.Integration.dll')]
		[string]$DatIntegrationAssemblyFilePath,

		[Parameter(Mandatory=$True,
		  ValueFromPipeline=$True,
		  ValueFromPipelineByPropertyName=$True,
		  HelpMessage='Logger')]
		$Logger
    )

    begin
    {
    }

    process
    {
        if ($EncryptedPassword -eq '') { return '' }
		Add-Type -Path $DatIntegrationAssemblyFilePath
		$decryptor = New-Object Dat.Integration.SecureStorage
		$Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", "[$env:ComputerName]	Decrypt" )
        return $decryptor.Decrypt($EncryptedPassword)
    }

    end
    {
    }
}