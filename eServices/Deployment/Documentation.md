
## Install Certificates
**Project Types**:
* WebService
* WindowsService

**File System Structure**:
--Project Directory
---- Deployment
------ Certificates
-------- {Certificate.pfx}
------ Profiles
-------- {Profile}
---------- Certificates
------------ {Metadata.ps1}

**Metadata Content**:
```powershell
$CertificateInfo = @{
    CertificateEncryptedPassword='{DAT Encrypted Password}';
    PhysicalLocation='..\..\..\Certificates\{Certificate Name}.pfx';
    Installations=@{
        Store='{e.g. CurrentUser\My}';
        InstallForUser=@{
            UserName='{e.g. CORP\ehsan.keshavarzian}';
            Password='{DAT Encrypted Password}';
        };
        Configuration='eHubDeployment';
        Permissions=@(
            @{User='{e.g. IIS AppPool\eHubPortal}'; Permission='Read'; Rule='Allow'}
        )
    }
}
```
