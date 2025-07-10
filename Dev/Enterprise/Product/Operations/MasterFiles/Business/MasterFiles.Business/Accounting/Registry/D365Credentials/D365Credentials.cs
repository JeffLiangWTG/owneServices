using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class D365Credentials : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string ClientID = nameof(ClientID);
			public const string ClientSecret = nameof(ClientSecret);
			public const string TenantID = nameof(TenantID);
		}

		#endregion

		[ResourceStringData("d7d82f45-827f-4405-80d8-000349610454", Caption = "Client ID")]
		public ZString ClientID
		{
			get => fClientID;
			set
			{
				SetNonPersistentPropertyValue(ClientIDInfo, ref fClientID, value);
				if (!IsValidationSuspended)
				{
					ValidateClientID();
				}
			}
		}
		ZString fClientID;

		public ZPropertyInfo ClientIDInfo => GetZPropertyInfo(Schema.ClientID);

		[ResourceStringData("90f2de8a-3ecb-4b52-8763-e73bc32c6105", Caption = "Client Secret")]
		public ZString ClientSecret
		{
			get => fClientSecret;
			set
			{
				SetNonPersistentPropertyValue(ClientSecretInfo, ref fClientSecret, value);
				if (!IsValidationSuspended)
				{
					ValidateClientSecret();
				}
			}
		}
		ZString fClientSecret;

		public ZPropertyInfo ClientSecretInfo => GetZPropertyInfo(Schema.ClientSecret);

		[ResourceStringData("dfe40c9f-2a22-45ac-b964-fff142292141", Caption = "Tenant ID")]
		public ZString TenantID
		{
			get => fTenantID;
			set
			{
				SetNonPersistentPropertyValue(TenantIDInfo, ref fTenantID, value);
				if (!IsValidationSuspended)
				{
					ValidateTenantID();
				}
			}
		}
		ZString fTenantID;

		public ZPropertyInfo TenantIDInfo => GetZPropertyInfo(Schema.TenantID);

		#region Validation

		void ValidateAll()
		{
			ValidateClientID();
			ValidateClientSecret();
			ValidateTenantID();
		}

		void ValidateClientID()
		{
			ClientIDInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ClientIDInfo);
		}

		void ValidateClientSecret()
		{
			ClientSecretInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ClientSecretInfo);
		}

		void ValidateTenantID()
		{
			TenantIDInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TenantIDInfo);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAll();
		}

		#endregion
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new D365Credentials();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ClientID, ClientID);
			writer.WriteElementString(Schema.ClientSecret, ClientSecret);
			writer.WriteElementString(Schema.TenantID, TenantID);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ClientID = reader.ReadElementString(Schema.ClientID);
			ClientSecret = reader.ReadElementString(Schema.ClientSecret);
			TenantID = reader.ReadElementString(Schema.TenantID);
		}
	}
}
