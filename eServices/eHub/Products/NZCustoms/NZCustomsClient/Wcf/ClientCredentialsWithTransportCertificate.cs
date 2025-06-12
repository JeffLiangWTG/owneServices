using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Description;

namespace CargoWise.eHub.Products.NZCustoms.Client.Wcf
{
    /// 
    /// Class that extends Client Credentials so that the certificate for the
    /// Transport layer encryption can be separate
    /// 
    public class ClientCredentialsWithTransportCertificate : ClientCredentials
    {
        /// <summary>
        /// The X509 Certificate that is to be used for https
        /// </summary>
        public X509Certificate2 TransportCertificate { get; set; }

        public ClientCredentialsWithTransportCertificate(ClientCredentials existingCredentials)
            : base(existingCredentials)
        {
        }

        protected ClientCredentialsWithTransportCertificate(ClientCredentialsWithTransportCertificate other)
            : base(other)
        {
            TransportCertificate = other.TransportCertificate;
        }

        protected override ClientCredentials CloneCore()
        {
            return new ClientCredentialsWithTransportCertificate(this);
        }

        public override SecurityTokenManager CreateSecurityTokenManager()
        {
            return new TokenManagerWithTransportCredentials(this);
        }

        public void SetTransportCertificate(string subjectName, StoreLocation storeLocation, StoreName storeName)
        {
            SetTransportCertificate(storeLocation, storeName, X509FindType.FindBySubjectDistinguishedName, subjectName);
        }

        public void SetTransportCertificate(StoreLocation storeLocation, StoreName storeName, X509FindType x509FindType, string subjectName)
        {
            TransportCertificate = FindCertificate(storeLocation, storeName, x509FindType, subjectName);
        }

        private static X509Certificate2 FindCertificate(StoreLocation location, StoreName name,
                                                        X509FindType findType, string findValue)
        {
            X509Store store = new X509Store(name, location);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection col = store.Certificates.Find(findType, findValue, true);
                return col[0]; // return first certificate found
            }
            finally
            {
                store.Close();
            }
        }
    }
}