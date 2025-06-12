$CertificateInfo = @{
    PhysicalLocation='..\..\..\Certificates\BillingKafkaCertificateRoot.cer';
    Installations=@(
        @{
            Store='LocalMachine\Root'
        }
    )
}