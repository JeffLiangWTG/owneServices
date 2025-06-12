$CertificateInfo = @{
    CertificateEncryptedPassword='h7FbKZ8/Q+BOY2Aw8WqCc9Bzg9jLGAKjiGPCI/FtrzgNqcqbBs++BmDeskeCm1QwgPRSce279Lct4rmx6qeSL7U8S0oNMplspNw2R1c5lysUlSec4z7cAmYiJNUvQ768os2BmgWPaR5UF9rP2Tt+UEWAAvgVwwjNtgMRWeE59xU=';
    PhysicalLocation='..\..\..\Certificates\b2b.tsw.mutual.ssl.cargowise.net.pfx';
    Installations=@(
        @{
            Store='LocalMachine\My';
            Permissions=@(
                @{
                    User='IIS_IUSRS';
                    Permission='FullControl';
                    Rule='Allow'
                }
            )
        }
    )
}