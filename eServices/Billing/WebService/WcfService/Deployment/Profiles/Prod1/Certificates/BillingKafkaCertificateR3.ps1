$CertificateInfo = @{
    PhysicalLocation='..\..\..\Certificates\BillingKafkaCertificateR3.cer';
    Installations=@(
        @{
            Store='LocalMachine\Root'
        }
    )
}