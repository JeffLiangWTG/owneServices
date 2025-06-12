using System;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;

namespace CargoWise.eHub.Products.NZCustoms.Client.Wcf
{
	internal class ClientCredentialsExtensionElement : ClientCredentialsElement
	{
		ConfigurationPropertyCollection properties;

		public override Type BehaviorType
		{
			get { return typeof(ClientCredentialsWithTransportCertificate); }
		}

		[ConfigurationProperty("transportCertificate")]
		public X509InitiatorCertificateClientElement TransportCertificate
		{
			get
			{
				return base["transportCertificate"] as X509InitiatorCertificateClientElement;
			}
		}

		protected override ConfigurationPropertyCollection Properties
		{
			get
			{
				if (properties == null)
				{
					ConfigurationPropertyCollection configurationProperties = base.Properties;
					configurationProperties.Add(new ConfigurationProperty(
									   "transportCertificate",
									   typeof(X509InitiatorCertificateClientElement),
									   null, null, null,
									   ConfigurationPropertyOptions.None));
					properties = configurationProperties;
				}
				return properties;
			}
		}

		protected override object CreateBehavior()
		{
			ClientCredentialsWithTransportCertificate creds =
				new ClientCredentialsWithTransportCertificate(base.CreateBehavior() as ClientCredentials);

			creds.SetTransportCertificate(TransportCertificate.StoreLocation,
										  TransportCertificate.StoreName,
										  TransportCertificate.X509FindType,
										  TransportCertificate.FindValue);

			ApplyConfiguration(creds);
			return creds;
		}
	}
}
