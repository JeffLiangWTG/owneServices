using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class EInvoicingCredentials : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string APIKey = nameof(APIKey);
			public const string ClientId = nameof(ClientId);
			public const string ClientSecret = nameof(ClientSecret);
		}

		#endregion

		#region Bound Properties

		#region ApiKey

		[ResourceStringData("fa33f845-999c-4df8-b2ae-867c0f5ef3e1", Caption = "API Key")]
		public ZString APIKey
		{
			get => fAPIKey;
			set
			{
				SetNonPersistentPropertyValue(APIKeyInfo, ref fAPIKey, value);
				ValidateAll();
			}
		}
		ZString fAPIKey;

		public ZPropertyInfo APIKeyInfo => GetZPropertyInfo(Schema.APIKey);

		#endregion

		#region Client Id

		[ResourceStringData("d7d82f45-827f-4405-80d8-000349610454", Caption = "Client ID")]
		public ZString ClientId
		{
			get => fClientId;
			set
			{
				SetNonPersistentPropertyValue(ClientIdInfo, ref fClientId, value);
				ValidateAll();
			}
		}
		ZString fClientId;

		public ZPropertyInfo ClientIdInfo => GetZPropertyInfo(Schema.ClientId);

		#endregion

		#region Client Secret

		[ResourceStringData("90f2de8a-3ecb-4b52-8763-e73bc32c6105", Caption = "Client Secret")]
		public ZString ClientSecret
		{
			get => fClientSecret;
			set
			{
				SetNonPersistentPropertyValue(ClientSecretInfo, ref fClientSecret, value);
				ValidateAll();
			}
		}
		ZString fClientSecret;

		public ZPropertyInfo ClientSecretInfo => GetZPropertyInfo(Schema.ClientSecret);

		#endregion

		#endregion

		#region Validation

		void ValidateAll()
		{
			ValidateClientId();
			ValidateClientSecret();
		}

		void ValidateClientId()
		{
			if (!IsValidationSuspended)
			{
				ClientIdInfo.ClearAllNotifications();
				if (!ClientSecret.IsEmpty)
				{
					MandatoryValidation.CheckEntered(ClientIdInfo);
				}
			}
		}

		void ValidateClientSecret()
		{
			if (!IsValidationSuspended)
			{
				ClientSecretInfo.ClearAllNotifications();
				if (!ClientId.IsEmpty)
				{
					MandatoryValidation.CheckEntered(ClientSecretInfo);
				}
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAll();
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EInvoicingCredentials();
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.APIKey, Encoder.Encrypt(APIKey));
			writer.WriteElementString(Schema.ClientId, ClientId);
			writer.WriteElementString(Schema.ClientSecret, Encoder.Encrypt(ClientSecret));
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			APIKey = Encoder.Decrypt(reader.ReadElementString(Schema.APIKey));
			ClientId = reader.ReadElementString(Schema.ClientId);
			ClientSecret = Encoder.Decrypt(reader.ReadElementString(Schema.ClientSecret));
		}

		TwoWayEncoder Encoder => encoder ?? (encoder = TwoWayEncoder.NewWithStandardInitialisationVector());
		TwoWayEncoder encoder;

		#endregion
	}
}
