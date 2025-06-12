$CertificateInfo = @{
    CertificateEncryptedPassword='XOeAZlIkRh2i772ynZaalk+Qrvp8ceJfHlCz9Ma4qcUyHIaXqx6NYdfmVwUJ9FR6ijSCfy+VOLybnb9sZ7F6LhoCe5l8mllMm5NvHJ42OnwEVkiSnQ+eSr77OpaKHUq2U8VcG1ZGf2WwrT2LT/XSQ7fdVhCFJFREMZ4Fyf/y9ys=';
    PhysicalLocation='..\..\..\Certificates\extb2b.tsw.mutual.ssl.cargowise.net.pfx';
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