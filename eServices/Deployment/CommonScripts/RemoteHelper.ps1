function Connect
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$False,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Machine Name to connect to')]
        [string]$MachineName,

        [Parameter(Mandatory=$False,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='User Name')]
        [string]$UserName,

        [Parameter(Mandatory=$False,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Password')]
        [string]$UserPassword,

        [Parameter(Mandatory=$False,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Configuration Name')]
        [string]$ConfigurationName,

        [Parameter(Mandatory=$False,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Password is Encrypted')]
        [switch]$AsEncryptedPassword,

        [Parameter(Mandatory=$False,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Use CredSSP for Authentication')]
        [switch]$CredSSP,

        [Parameter(Mandatory=$False,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Custom connection port')]
        [Int32]$PortNumber,

        [Parameter(Mandatory=$False,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Use SSL for Domain Session')]
        [switch]$UseSslForDomainSession
    )

    begin 
    {
    }

    process
    {
        if ($MachineName -eq '' -or $MachineName -eq "localhost" -or ($MachineName -as [IPAddress] -and [IPAddress]::IsLoopback($MachineName))) 
        {
            try
            {
                $Session = New-PSSession -ConfigurationName $ConfigurationName -EnableNetworkAccess
            }
            finally
            {
                if ($null -ne $error[0])
                { Throw "Could not create a session on local machine: $error[0]"}
            }
        }
        elseif (($UserName -eq '') -or ($env:UserName -eq $UserName))
        {
            try
            {
                $Session = New-PSSession -computername $MachineName -ConfigurationName $ConfigurationName
            }
            finally
            {
                if ($null -ne $error[0])
                { Throw "Could not create a session on [$MachineName]: $error[0]"}
            }
        }
        else
        {
            $IsDomain = $UserName -match ".*\@.*"

            if ($AsEncryptedPassword)
            {
                $pw = ConvertTo-SecureString -String $UserPassword
            }
            else
            {
                $pw = convertto-securestring -AsPlainText -Force -String $UserPassword
            }
            $UserCred = new-object -typename System.Management.Automation.PSCredential -argumentlist $UserName,$pw

            if ($IsDomain)
            {
                try
                {
                    if ($CredSSP)
                    {
                        $Session = New-PSSession -computername $MachineName -credential $UserCred -Authentication Credssp
                    }
                    elseif ($UseSslForDomainSession)
                    {
                        $sessionOptions = New-PSSessionOption -SkipRevocationCheck -SkipCACheck -SkipCNCheck
                        $Session = New-PSSession -computername $MachineName -credential $UserCred -ConfigurationName $ConfigurationName -UseSSL -SessionOption $sessionOptions
                    }
                    else
                    {
                        $Session = New-PSSession -computername $MachineName -credential $UserCred -ConfigurationName $ConfigurationName
                    }
                }
                finally
                {
                    if ($null -ne $error[0])
                    { Throw "Could not create a session on [$MachineName]: $error[0]"}
                }
            }
            else
            {
                try
                {
                    $sessopts = New-PSSessionOption -SkipRevocationCheck -SkipCACheck -SkipCNCheck
                    $arguments = @{}
                    if (0 -ne $PortNumber)
                    {
                        $arguments.Add('Port', $PortNumber)
                    }

                    $Session = New-PSSession @arguments -computername $MachineName -credential $UserCred -UseSSL -SessionOption $sessopts -ConfigurationName $ConfigurationName
                }
                finally
                {
                    if ($null -ne $error[0])
                    { Throw "Could not create a SSL session on [$MachineName]: $error[0]"}
                }
            
            }
        }
        return new-object PSCustomObject –property @{
            Session = $Session
            IsDomain = $IsDomain
            UserName = $UserName
            UserPassword = $UserPassword
            MachineName = $MachineName
        }
    }

    end
    {
    }
}

function RemoteRun
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$True,
        Position=0,
        ParameterSetName='Function',
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Function to run')]
        $Function,

        [Parameter(Mandatory=$True,
        Position=0,
        ParameterSetName='ScriptBlock',
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='ScriptBlock to run')]
        $ScriptBlock,

        [Parameter(Mandatory=$True,
        Position=1,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Remote session')]
        $Session,

        [Parameter(Mandatory=$False,
        Position=2,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Arguments for the function')]
        $Arguments,

        [Parameter(Mandatory=$True,
        Position=3,
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
        $ScriptToRun = $ScriptBlock
        if ($PSCmdlet.ParameterSetName -eq 'Function')
        {
            $ScriptToRun = ((Get-Item function:$Function).ScriptBlock)
        }
        $Result, $Logs = Invoke-Command -ScriptBlock $ScriptToRun -Session $Session -ArgumentList $Arguments
        $Logs | ForEach-Object {
            $Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", $_ )
        }
        return $Result
    }

    end
    {
    }
}

function RegisterConfig
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$True,
        Position=0,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Remote session')]
        $SessionInfo,

        [Parameter(Mandatory=$True,
        Position=1,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='User to run as')]
        $TargetUser,

        [Parameter(Mandatory=$True,
        Position=2,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Configuration Name')]
        $ConfigName,

        [Parameter(Mandatory=$True,
        Position=3,
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
        $Session = $SessionInfo.Session
        $RegisterConfig = {param($ConfigName, $Credential)
            $Config = Get-PSSessionConfiguration -Name $ConfigName
            if ($Config -eq $null) {
                Register-PSSessionConfiguration -Name $ConfigName -SessionType DefaultRemoteShell -AccessMode Remote -RunAsCredential $Credential -Force
            }
        }
        Invoke-Command -ScriptBlock $RegisterConfig -Session ($Session) -ArgumentList $ConfigName, $TargetUser
        $Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", "[$env:ComputerName] Session Configuration is ready: $ConfigName")
    }

    end
    {
    }
}

function RemoteJobRun
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$True,
        Position=0,
        ParameterSetName='Function',
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Function to run')]
        $Function,

        [Parameter(Mandatory=$True,
        Position=0,
        ParameterSetName='ScriptBlock',
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='ScriptBlock to run')]
        $ScriptBlock,

        [Parameter(Mandatory=$True,
        Position=1,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Remote session')]
        $SessionInfo,

        [Parameter(Mandatory=$True,
        Position=2,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='User to run as')]
        $TargetUser,

        [Parameter(Mandatory=$True,
        Position=3,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Configuration Name')]
        $ConfigName,

        [Parameter(Mandatory=$False,
        Position=4,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Arguments for the function')]
        $Arguments,

        [Parameter(Mandatory=$True,
        Position=5,
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
        $ScriptToRun = $ScriptBlock
        if ($PSCmdlet.ParameterSetName -eq 'Function')
        {
            $ScriptToRun = ((Get-Item function:$Function).ScriptBlock)
        }
        $ComputerName = $SessionInfo.MachineName
        $UserName = $SessionInfo.UserName
        $UserPassword = $SessionInfo.UserPassword
        $JobSession = (Connect -MachineName $ComputerName -UserName $UserName -UserPassword $UserPassword -ConfigurationName $ConfigName).Session
        $ScriptBlock = {param($TargetScript, $TargetCredential, $TargetArgumnts)
            $Job = Start-Job -ScriptBlock ([scriptblock]::Create($TargetScript)) -Credential $TargetCredential -ArgumentList $TargetArgumnts
            $Result = $Job | Wait-Job | Receive-Job
            $Job | Remove-Job | Out-Null
            return $Result
        }
        $Result, $Logs = Invoke-Command -ScriptBlock $ScriptBlock -Session $JobSession -ArgumentList $ScriptToRun, $TargetUser, $Arguments
        $Logs | ForEach-Object {
            $Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", $_ )
        }
        return $Result
    }

    end
    {
    }
}

function TransferFile
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$True,
        Position=0,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Function to run')]
        [string]$LocalPath,

        [Parameter(Mandatory=$True,
        Position=1,
        ParameterSetName='SpecificTarget',
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='ScriptBlock to run')]
        [string]$TargetPath,

        [Parameter(Mandatory=$True,
        Position=1,
        ParameterSetName='TemporaryTarget',
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='ScriptBlock to run')]
        [switch]$UseTemporaryPathForTarget,

        [Parameter(Mandatory=$True,
        Position=2,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Remote session')]
        [System.Management.Automation.Runspaces.PSSession]$Session,

        [Parameter(Mandatory=$True,
          Position=3,
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
        if ($UseTemporaryPathForTarget)
        {
            $TargetPath = Join-Path $env:TEMP -ChildPath ([System.Guid]::NewGuid()) | Join-Path -ChildPath ([System.IO.Path]::GetFileName($LocalPath))
        }
        $Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", "[$env:ComputerName] Transfering $LocalPath to $TargetPath")
        $Data = Get-Content $LocalPath -Encoding Byte
        $ScriptBlock = {
            New-Item -Path ([System.IO.Path]::GetDirectoryName($Using:TargetPath)) -ItemType directory
            $Using:Data | Set-Content -Path $Using:TargetPath -Encoding Byte
        }
        Invoke-Command -Session $Session -ScriptBlock $ScriptBlock | Out-Null
        $Logger.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", "[$($Session.ComputerName)] Transfer Completed")
        return $TargetPath
    }

    end
    {
    }
}

function RemoveItem
{
    [CmdletBinding()]
    param
    (
        [Parameter(Mandatory=$True,
        Position=0,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='File to remove')]
        [string]$TargetFile,

        [Parameter(Mandatory=$True,
        Position=1,
        ValueFromPipeline=$True,
        ValueFromPipelineByPropertyName=$True,
        HelpMessage='Remote session')]
        [System.Management.Automation.Runspaces.PSSession]$Session
    )

    begin 
    {
    }

    process 
    {
        Invoke-Command -Session $Session -ScriptBlock { 
            $Folder = [System.IO.Path]::GetDirectoryName($using:TargetFile)
            Remove-Item ($Folder) -Force -Recurse 
        }
    }

    end
    {
    }
}
