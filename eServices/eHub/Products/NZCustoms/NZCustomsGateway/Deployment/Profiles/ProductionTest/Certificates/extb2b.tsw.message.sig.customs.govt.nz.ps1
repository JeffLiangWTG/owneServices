$CertificateInfo = @{
    PhysicalLocation='..\..\..\Certificates\extb2b.tsw.message.sig.customs.govt.nz.cer';
    Installations=@(
        @{
            Store='LocalMachine\TrustedPeople'
        }
    )
}