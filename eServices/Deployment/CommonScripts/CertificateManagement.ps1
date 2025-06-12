function InstallCertificate
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$True,
        Position=0,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Target Machine Name')]
        [string]$MachineName,

        [Parameter(Mandatory=$True,
        Position=1,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='UserName')]
        [string]$UserName,

        [Parameter(Mandatory=$False,
        Position=2,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Password')]
        [string]$UserPassword,
        
        [Parameter(Mandatory=$False,
        Position=3,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Metadata File')]
        [string]$MetadataFile,

		[Parameter(Mandatory=$True,
        Position=4,
		ValueFromPipeline=$True,
		ValueFromPipelineByPropertyName=$True,
		HelpMessage='Dat Integration Assembly File Path')]
		[string]$DatIntegrationAssemblyFilePath,

		[Parameter(Mandatory=$True,
        Position=5,
		ValueFromPipeline=$True,
		ValueFromPipelineByPropertyName=$True,
		HelpMessage='Logger')]
		$Logger,

        [Parameter(Mandatory=$False,
        Position=6,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Custom connection port')]
        [Int32]$PortNumber
    )

    begin 
    {
    }

    process 
    {
        $s = (Connect -MachineName $MachineName -UserName $UserName -UserPassword $UserPassword -PortNumber $PortNumber)
        $Metadata = $MetadataFile | GetMetadata
        $CertificateFile = Join-Path ([System.IO.Path]::GetDirectoryName($MetadataFile)) -ChildPath $Metadata.PhysicalLocation
        $TargetCertificateFile = TransferFile -LocalPath $CertificateFile -UseTemporaryPathForTarget -Session $s.Session -Logger $Logger
      
        Try {
            $Password = GetSecureString -EncryptedPassword $Metadata.CertificateEncryptedPassword -DatIntegrationAssemblyFilePath $DatIntegrationAssemblyFilePath -Logger $Logger
            $Metadata.Installations | ForEach-Object  {
                $User = $_.InstallForUser
                $Store = $_.Store
                if ($User -ne $null)
                {
                    $Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", "[$env:ComputerName]   Installing certificate $CertificateFile Into cert:\$Store on $MachineName for user:$($User.UserName)" ) | Out-Null
                    $Configuration = $_.Configuration
                    $Credential = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $User.UserName, (GetSecureString -Logger $Logger -EncryptedPassword $User.Password -DatIntegrationAssemblyFilePath $DatIntegrationAssemblyFilePath)
                    RegisterConfig -SessionInfo $s -TargetUser $Credential -ConfigName $Configuration -Logger $Logger | Out-Null
                    $Result = RemoteJobRun -Function ImportCertificate -SessionInfo $s -TargetUser $Credential -ConfigName $Configuration -Arguments $Store, $TargetCertificateFile, $Password, $false -Logger $Logger
                    if ($Result.Error -ne $null)
                    {
                        throw $Result.Error
                    }
                    $Thumbprint = $Result.Data.Thumbprint
                    $Permissions = $_.Permissions
                    if ($Permissions)
                    {
                        $Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", "[$env:ComputerName]   Applying permissions for certificate $CertificateFile." ) | Out-Null
                        $Permissions | ForEach-Object {
                            $PermissionResult = RemoteJobRun -Function SetAccessRule -SessionInfo $s -TargetUser $Credential -ConfigName $Configuration -Arguments $Store, $Thumbprint, $_.User, $_.Permission, $_.Rule -Logger $Logger
                            if ($PermissionResult.Error -ne $null) 
                            {
                                $Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"High", $PermissionResult.Error ) | Out-Null
                                $Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"High", "[$env:ComputerName]    Failed to apply permission $($_.Permission) for Certificate $Thumbprint (User: $($_.User) | Rule: $($_.Rule))" ) | Out-Null
                            }
                        }
                        $Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", "[$env:ComputerName]   Finished applying permissions." ) | Out-Null
                    }
                }
                else
                {
                    $Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", "[$env:ComputerName]   Installing certificate $CertificateFile Into cert:\$Store on $MachineName" ) | Out-Null
                    $Result = RemoteRun -Function ImportCertificate -Session $s.Session -Arguments $Store, $TargetCertificateFile, $Password, $false -Logger $Logger
                    if ($Result.Error -ne $null)
                    {
                        throw $Result.Error
                    }
                    $Thumbprint = $Result.Data.Thumbprint
                    $Permissions = $_.Permissions
                    if ($Permissions)
                    {
                        $Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", "[$env:ComputerName]   Applying permissions for certificate $CertificateFile." ) | Out-Null
                        $Permissions | ForEach-Object {
                            $PermissionResult = RemoteRun -Function SetAccessRule -Session $s.Session -Arguments $Store, $Thumbprint, $_.User, $_.Permission, $_.Rule -Logger $Logger
                            if ($PermissionResult.Error -ne $null) 
                            {
                                $Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"High", $PermissionResult.Error ) | Out-Null
                                $Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"High", "[$env:ComputerName]    Failed to apply permission $($_.Permission) for Certificate $Thumbprint (User: $($_.User) | Rule: $($_.Rule))" ) | Out-Null
                            }
                        }
                        $Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", "[$env:ComputerName]   Finished applying permissions." ) | Out-Null
                    }
                }
            }
        }
        Catch {
            $Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"High", $error[0] )
            $Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"High", "Could not install Certificate $CertificateFile. Please install it manually" )
        }
        Finally {
            RemoveItem -TargetFile $TargetCertificateFile -Session $s.Session
            Remove-PSSession $s.Session
        }
    }

    end
    {
    }
}

function ImportCertificate
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$True,
        Position=0,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Store Name')]
        [string]$StoreName,

        [Parameter(Mandatory=$True,
        Position=1,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Certificate File')]
        [string]$CertificateFile,

        [Parameter(Mandatory=$False,
        Position=2,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Password')]
		[AllowEmptyString()]
        $Password,
        
        [Parameter(Mandatory=$False,
        Position=3,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Delete Certificate')]
        [switch]$DeleteCertificateAfterInstallation
    )

    begin 
    {
        $Log = @()
        $Result = [PSCustomObject]@{
            Data  = $null
            Error = $null
        }
    }

    process 
    {
        try
        {
		    $File = (Get-ChildItem -Path $CertificateFile -ErrorAction Stop)
            $output = switch -Wildcard ($CertificateFile)
            {
                '*.cer'
                {
                    $Log += "[$env:ComputerName]    Importing certificate matching the pattern '*.cer' ..."
                    $File | Import-Certificate -CertStoreLocation cert:\$StoreName -ErrorAction Stop
                }
                '*.pfx'
                {
                    if ($Password.Length -ne 0)
                    {
                        $Log += "[$env:ComputerName]    Importing private certificate with provided password matching the pattern '*.pfx' ..."
                        $File | Import-PfxCertificate -CertStoreLocation cert:\$StoreName -Password $Password -ErrorAction Stop
                    }
                    else
                    {
                        $Log += "[$env:ComputerName]    Importing private certificate matching the pattern '*.pfx' ..."
                        $File | Import-PfxCertificate -CertStoreLocation cert:\$StoreName -ErrorAction Stop
                    }
                }
                default
                {
                    throw "Not supported certificate file [$CertificateFile]"
                }
            }
            $Log += "[$env:ComputerName]    Certificate imported. Thumbprint: $($output.Thumbprint)"
            $Result.Data = $output
        }
        catch
        {
            $Log += "[$env:ComputerName]    An error occurred importing the certificate: $($_.Exception.Message)"
            $Result.Error = $_.Exception.Message
        }
        finally
        {
            if ($DeleteCertificateAfterInstallation)
            {
                $Folder = [System.IO.Path]::GetDirectoryName($CertificateFile)
                Remove-Item ($Folder) -Force -Recurse
                $Log += "[$env:ComputerName]    Temp directory removed: $CertificateFile"
            }
        }
        
        return $Result
    }

    end
    {
        $Result
        $Log
    }
}

function GetMetadata
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$True,
        Position=0,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Store Name')]
        [string]$MetadataFile
    )

    begin 
    {
        $Log = @()
        $Result = $null
    }

    process 
    {
        try
        {
            . $($MetadataFile)
            return $CertificateInfo
        }
        finally
        {
        }
    }

    end
    {
        $Result
        $Log
    }
}

function GetThumbprint
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$True,
        Position=0,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Certificate File Path')]
        [string]$CertificateFile,
        
        [Parameter(Mandatory=$False,
        Position=1,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Password')]
		[AllowEmptyString()]
        [securestring]$Password
    )

    begin 
    {
        $Log = @()
        $Result = $null
    }

    process 
    {
        $Certificate = ( Get-ChildItem -Path $CertificateFile )
        $CertPrint = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2
        try
        {
            if ($Password -ne '')
			{
				$CertPrint.Import($Certificate, $Password, [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::DefaultKeySet)
			}
			else
			{
				$CertPrint.Import($Certificate)
			}
			return $CertPrint.Thumbprint
            
        }
        finally
        {
        }
    }

    end
    {
        $Result
        $Log
    }
}

function UninstallCertificate
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$True,
        Position=0,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Target Machine Name')]
        [string]$MachineName,

        [Parameter(Mandatory=$True,
        Position=1,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='UserName')]
        [string]$UserName,

        [Parameter(Mandatory=$False,
        Position=2,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Password')]
        [string]$UserPassword,
        
        [Parameter(Mandatory=$False,
        Position=3,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Metadata File')]
        [string]$MetadataFile,

		[Parameter(Mandatory=$True,
        Position=4,
		ValueFromPipeline=$True,
		ValueFromPipelineByPropertyName=$True,
		HelpMessage='Dat Integration Assembly File Path')]
		[string]$DatIntegrationAssemblyFilePath,

		[Parameter(Mandatory=$True,
        Position=5,
		ValueFromPipeline=$True,
		ValueFromPipelineByPropertyName=$True,
		HelpMessage='Logger')]
		$Logger,

        [Parameter(Mandatory=$False,
        Position=6,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Custom connection port')]
        [Int32]$PortNumber
    )

    begin 
    {
    }

    process 
    {
        $s = (Connect -MachineName $MachineName -UserName $UserName -UserPassword $UserPassword -PortNumber $PortNumber)
      
        $Metadata = $MetadataFile | GetMetadata
        $Password = $Metadata.CertificateEncryptedPassword | GetSecureString -DatIntegrationAssemblyFilePath $DatIntegrationAssemblyFilePath
        $CertificateFile = Join-Path ([System.IO.Path]::GetDirectoryName($MetadataFile)) -ChildPath $Metadata.PhysicalLocation
        $Thumbprint = GetThumbprint -CertificateFile $CertificateFile -Password $Password
                    
        $Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", "[$env:ComputerName]  Uninstalling certificate: $CertificateFile" )
                  
        Try
        {
            $Metadata.Installations | ForEach-Object  {
                $User = $_.InstallForUser
                $Store = $_.Store
                if ($User -ne $null)
                {
                    $Configuration = $_.Configuration
                    $Credential = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $User.UserName, (GetSecureString -EncryptedPassword $User.Password -DatIntegrationAssemblyFilePath $DatIntegrationAssemblyFilePath)
                    RegisterConfig -SessionInfo $s -TargetUser $Credential -ConfigName $Configuration -Logger $Logger
                    RemoteJobRun -Function RemoveCertificate -SessionInfo $s -TargetUser $Credential -ConfigName $Configuration -Arguments $Store, $Thumbprint -Logger $Logger
                }
                else
                {
                    RemoteRun -Function RemoveCertificate -Session $s.Session -Arguments $Store, $Thumbprint -Logger $Logger
                }
            }
        }
        Catch
        {
            $Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"High", $error[0] )
            $Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"High", "[$env:ComputerName]    Could not uninstall Certificate $CertificateFile. Please uninstall it manually" )
        }
        Finally
        {
            Remove-PSSession $s.Session
        }
    }

    end
    {
    }
}

function RemoveCertificate
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$True,
        Position=0,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Store name')]
        [string]$Store,
        
        [Parameter(Mandatory=$True,
        Position=1,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Thumbprint')]
        [string]$Thumbprint
    )

    begin 
    {
        $Log = @()
        $Result = $null
    }

    process 
    {
        try
        {
            $StoreDirectory = (Join-Path "cert:\" $Store)
			$Cert = (Join-Path $StoreDirectory $Thumbprint)
            Remove-Item $Cert
            $Log += "[$env:ComputerName]    Certificate removed: $Cert"
        }
        finally
        {
        }
    }

    end
    {
        $Result
        $Log
    }
}

function SetAccessRule
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$True,
        Position=0,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Store name')]
        [string]$Store,
        
        [Parameter(Mandatory=$True,
        Position=1,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Thumbprint')]
        [string]$Thumbprint,
        
        [Parameter(Mandatory=$True,
        Position=2,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='User Account')]
        [string]$UserAccount,
        
        [Parameter(Mandatory=$True,
        Position=3,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Permission')]
        [string]$Permission,
        
        [Parameter(Mandatory=$True,
        Position=4,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Access Rule')]
        [string]$AccessRule
    )

    begin 
    {
        $Log = @()
        $Result = [PSCustomObject]@{
            Data  = $null
            Error = $null
        }
    }

    process 
    {
        $StoreDirectory = (Join-Path "cert:\" $Store)
        $Cert = (Join-Path $StoreDirectory $Thumbprint)
        $CertObj = Get-ChildItem $Cert
        try
        {
            $Key = [System.Security.Cryptography.X509Certificates.RSACertificateExtensions]::GetRSAPrivateKey($CertObj)
            if ($Key -ne $null) {
                $RSAFile = $Key.Key.UniqueName
                $Log += "[$env:ComputerName] Looking for $RSAFile"
                
                $UserSid = [System.Security.Principal.WindowsIdentity]::GetCurrent().User.Value
                $LocationsToCheck = @(
                    (Join-Path $env:ALLUSERSPROFILE 'Application Data\Microsoft\Crypto\RSA\MachineKeys'),   # CAPI - shared private
                    (Join-Path $env:ALLUSERSPROFILE 'Application Data\Microsoft\Crypto\Keys'),              # CNG - shared private
                    (Join-Path $env:ALLUSERSPROFILE 'Application Data\Microsoft\Crypto\RSA\S-1-5-18'),      # CAPI - local system private
                    (Join-Path $env:ALLUSERSPROFILE 'Application Data\Microsoft\Crypto\SystemKeys'),        # CNG - local system private
                    (Join-Path $env:APPDATA 'Microsoft\Crypto\RSA' | Join-Path -ChildPath $UserSid),        # CAPI - user private
                    (Join-Path $env:APPDATA 'Microsoft\Crypto\Keys')                                        # CNG - user private
                )
                $FullPath = $null
                foreach ($Location in $LocationsToCheck) {
                    $PathToTest = (Join-Path $Location $RSAFile)
                    $Log += "[$env:ComputerName] Looking in: $PathToTest"
                    if (Test-Path $PathToTest) {
                        $FullPath = $PathToTest
                        break
                    }
                    $Log += "[$env:ComputerName] Couldn't find in $PathToTest"
                }
                if ($FullPath -eq $null) {
                    throw "Failed to apply permission to certificate with thumbprint $Thumbprint. Path to private key file not found."
                }
                
                $ACL = $FullPath | Get-Acl -ErrorAction Stop
                $Rule = (New-Object System.Security.AccessControl.FileSystemAccessRule $UserAccount, $Permission, $AccessRule -ErrorAction Stop)
                $ACL.AddAccessRule($Rule)
                Set-Acl $FullPath $ACL
                $Log += "[$env:ComputerName] Set AccessRule: $Store, $Thumbprint, $UserAccount, $Permission, $AccessRule"
            }
        }
        catch 
        {
            $Result.Error = $_.Exception.Message
        }
        finally
        {
        }
        
        return $Result
    }

    end
    {
        $Result
        $Log
    }
}