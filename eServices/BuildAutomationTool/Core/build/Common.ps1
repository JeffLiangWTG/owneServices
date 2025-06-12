function Connect
{
	param([string]$MachineName,[string]$UserName,[string]$UserPassword)
	If ($MachineName -eq '')
    {
	    $Session = New-PSSession
    }
    elseif ($UserName -eq '')
    {
	    $Session = New-PSSession -computername $MachineName
    }
    else
    {
	    $IsDomain = $UserName -match "(.*)[\\@](.*)"
	    If ($IsDomain)
	    {
		    $pw = convertto-securestring -AsPlainText -Force -String $UserPassword
			$UserCred = new-object -typename System.Management.Automation.PSCredential -argumentlist $UserName,$pw
			$Session = New-PSSession -computername $MachineName -credential $UserCred
	    }
	    else
	    {
            try
			{
                $pw = convertto-securestring -AsPlainText -Force -String $UserPassword
			    $UserCred = new-object -typename System.Management.Automation.PSCredential -argumentlist $UserName,$pw
			    $sessopts = New-PSSessionOption -SkipCACheck -SkipCNCheck
			    $Session = New-PSSession -computername $MachineName -credential $UserCred -UseSSL -SessionOption $sessopts 
            }
			finally
			{
				if ($null -ne $error[0])
				{ Throw "Connection Error: Please set up the server for SSL connection and try again."}
			}
			
	    }
    }
	return new-object pscustomobject –property @{
		Session = $Session
        IsDomain = $IsDomain
	}
}