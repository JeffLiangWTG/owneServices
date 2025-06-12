<#
.SYNOPSIS
Launch a dialog asking for user's password.
Print encrypted password.
Windows Data Protection API (DPAPI) is used to encrypt the standard string representation.

.EXAMPLE
PS > C:\eServices\Deployment\Get-EncryptedPassword corp\FirstName.LastName -ValidatePassword
PS > C:\eServices\Deployment\Get-EncryptedPassword corp\FirstName.LastName
#>

param(
[string] $userName,
[Switch] $ValidatePassword
)

Set-StrictMode -Version 3

while ($true)
{
    $credential = Get-Credential -Message "Please enter password:" -UserName $userName
    if(-not ($credential -is "System.Management.Automation.PsCredential")) { return }

    if ($ValidatePassword)
    {
        $plainTextPassword = $credential.GetNetworkCredential().password

        $currentDomain = "LDAP://" + ([ADSI]"").distinguishedName
        $domain = New-Object System.DirectoryServices.DirectoryEntry($currentDomain,$userName,$plainTextPassword)

        if ($domain.name -eq $null)
        {
            $wscriptShell = New-Object -ComObject Wscript.Shell
            $wscriptShell.Popup("Validation failed - please verify your password.", 0, "Error", 0x0)
        }
        else { break }
    }
    else { break }
}

$credential.Password | ConvertFrom-SecureString | Out-String

