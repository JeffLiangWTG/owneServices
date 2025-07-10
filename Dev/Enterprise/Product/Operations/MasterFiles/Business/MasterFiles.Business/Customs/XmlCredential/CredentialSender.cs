using System.Collections.Generic;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.MasterFiles.Business.Customs.XmlCredential
{
	public class CredentialSender
	{
		public CredentialSender(ZString configurationName)
			: this(configurationName, EDIInterchangeTypeList.Codes.Configuration)
		{
		}

		public CredentialSender(ZString configurationName, ZString interchangeType)
			: this(configurationName, interchangeType, CredentialRecipient.eHub)
		{
		}

		public CredentialSender(ZString configurationName, ZString interchangeType, CredentialRecipient recipient)
		{
			this.configurationName = Argument.NotNullOrEmpty(configurationName, nameof(configurationName));
			this.interchangeType = interchangeType;
			this.recipient = recipient;
		}

		readonly ZString configurationName;
		readonly ZString interchangeType;
		readonly CredentialRecipient recipient;

		public void AddItems(params object[] items)
		{
			if (!items.IsNullOrEmpty())
			{
				groupItems = groupItems ?? new List<object>();
				groupItems.AddRange(items);
			}
		}
		List<object> groupItems;

		public Configuration ToConfiguration()
		{
			var configuration = new Configuration
			{
				Name = configurationName,
				Version = Constants.Configuration.Version
			};

			configuration.Group = new GroupCollection { CreateSystemGroup() };

			return configuration;
		}

		public static Credential CreateCredential(ZString name, ZString userName, ZString password)
		{
			return new Credential
			{
				Name = name,
				UserName = userName,
				Password = password.IsEmpty ? System.Array.Empty<byte>() : EncryptPassword(password)
			};
		}

		public static Certificate CreateCertificate(ZBlob file, ZString passphrase)
		{
			return new Certificate
			{
				Name = Constants.CertificateDetails.Name,
				File = new File
				{
					IsSpecified = true,
					Value = file
				},
				Passphrase = passphrase.IsEmpty ? System.Array.Empty<byte>() : EncryptPassword(passphrase)
			};
		}

		public static Group CreateGroup(ZString groupType, ZString reference, ZString status)
		{
			var group = new Group()
			{
				Type = groupType
			};
			if (!reference.IsEmpty)
			{
				group.Reference = reference;
			}
			if (!status.IsEmpty)
			{
				group.Status = status;
			}

			return group;
		}

		public static FTP CreateFTP(ZString server, ZInt port, ZString userName, ZString password)
		{
			var ftp = new FTP
			{
				Server = server,
				Port = port,
				UserName = userName
			};

			if (!password.IsEmpty)
			{
				ftp.Password = EncryptPassword(password);
			}

			return ftp;
		}

		public static Item CreateItem(ZString name, ZString value)
		{
			return new Item
			{
				Name = name,
				Value = value
			};
		}

		public static byte[] EncryptPasswordAsBytes(byte[] password)
		{
			return CargoWise.eServices.Encryption.Client.Encryptor.EhubClientEncryptor.EncryptBinary(password);
		}

		public static byte[] EncryptPassword(ZString password)
		{
			return CargoWise.eServices.Encryption.Client.Encryptor.EhubClientEncryptor.EncryptBinary(Encoding.ASCII.GetBytes(password));
		}

		public static string EncryptPasswordAsString(ZString password)
		{
			return CargoWise.eServices.Encryption.Client.Encryptor.EhubClientEncryptor.Encrypt(password);
		}

		public IEDIInterchange SendCredential(BusinessObjectFactory factory)
		{
			var isRecipientDirectxT = recipient == CredentialRecipient.DirectxT;

			var interchange = factory.New<IEDIInterchange>();
			interchange.EI_InterchangeType = interchangeType;
			interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			interchange.EI_From = ((IGlbCompany)Env.CurrentCompany).LicenceKeyIdentifier;
			interchange.EI_SessionGUID = interchange.PK;
			interchange.EI_BodyText = ToConfiguration().ToXml();

			interchange.EI_ApplicationCode = isRecipientDirectxT ? ApplicationCodeList.Codes.XHCredentialConfig : ApplicationCodeList.Codes.eHub;
			interchange.EI_To = isRecipientDirectxT ? Constants.Configuration.XHRecipient : Constants.Configuration.EHubRecipient;
			interchange.EI_Status = isRecipientDirectxT ? EDIInterchangeStatusList.Codes.Queued : EDIInterchangeStatusList.Codes.eHubQueued;
			interchange.EI_TransportType = isRecipientDirectxT ? EDIInterchangeTransportTypeList.Codes.xT : EDIInterchangeTransportTypeList.Codes.eHub;

			return interchange;
		}

		public void SendCredential()
		{
			var factory = new BusinessObjectFactory();
			SendCredential(factory);

			try
			{
				factory.Save();
			}
			catch (ZSaveException e)
			{
				ZExceptionReporting.HandleSaveException(e);
			}
		}

		Group CreateSystemGroup()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var result = new Group()
			{
				Type = Constants.GroupTypes.SystemType,
				Reference = registrationKey.EnterpriseCode + registrationKey.ServerCode
			};

			if (groupItems != null)
			{
				result.Items = groupItems.ToArray();
			}
			return result;
		}
	}
}
