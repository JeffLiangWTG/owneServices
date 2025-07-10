using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.TR.Business.XmlSerializers")]
	public class FTPSettings : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string FTPAddress = "FTPAddress";
			public const string Port = "Port";
			public const string Inbox = "Inbox";
			public const string Outbox = "Outbox";
			public const string ExportUnionUserCode = "ExportUnionUserCode";
			public const string ExportUnionUserPassword = "ExportUnionUserPassword";
			public const string ExportUnionPaymentPassword = "ExportUnionPaymentPassword";
		}

		#endregion

		#region Constructors

		public FTPSettings() : base()
		{
		}

		public FTPSettings(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		#endregion

		#region FTP Address

		[ResourceStringData("Enterprise.Customs.TR.Business.FTPSettings|FTPAddress", Caption = "FTP Address")]
		[List(nameof(FTPAddressList))]
		public ZString FTPAddress
		{
			get => ftpAddress;
			set
			{
				if (ftpAddress != value)
				{
					SetNonPersistentPropertyValue(FTPAddressInfo, ref ftpAddress, value);
				}
			}
		}

		ZString ftpAddress;

		public ZPropertyInfo FTPAddressInfo => GetZPropertyInfo(Schema.FTPAddress);

		public CodeDescriptionPairList FTPAddressList
		{
			get { return CurrentFactory.GetCachedValue<ExportUnionFTPAddressList>(); }
		}

		#endregion

		#region Port

		[ResourceStringData("Enterprise.Customs.TR.Business.FTPSettings|Port", Caption = "Port")]
		public ZInt Port
		{
			get => port;
			set
			{
				if (port != value)
				{
					SetNonPersistentPropertyValue(PortInfo, ref port, value);
				}
			}
		}

		ZInt port;

		public ZPropertyInfo PortInfo => GetZPropertyInfo(Schema.Port);

		#endregion

		#region Inbox

		[ResourceStringData("Enterprise.Customs.TR.Business.FTPSettings|Inbox", Caption = "Inbox")]
		public ZString Inbox
		{
			get => inbox;
			set
			{
				if (inbox != value)
				{
					SetNonPersistentPropertyValue(InboxInfo, ref inbox, value);
				}
			}
		}

		ZString inbox;

		public ZPropertyInfo InboxInfo => GetZPropertyInfo(Schema.Inbox);

		#endregion

		#region Outbox

		[ResourceStringData("Enterprise.Customs.TR.Business.FTPSettings|Outbox", Caption = "Outbox")]
		public ZString Outbox
		{
			get => outbox;
			set
			{
				if (outbox != value)
				{
					SetNonPersistentPropertyValue(OutboxInfo, ref outbox, value);
				}
			}
		}

		ZString outbox;

		public ZPropertyInfo OutboxInfo => GetZPropertyInfo(Schema.Outbox);

		#endregion

		#region Export Union User Code

		[MaxLength(15)]
		[ResourceStringData("Enterprise.Customs.TR.Business.FTPSettings|ExportUnionUserCode", Caption = "Export Union User Code", ShortCaption = "Ex.Un.User Code")]
		public ZString ExportUnionUserCode
		{
			get => exportUnionUsercode;
			set
			{
				if (exportUnionUsercode != value)
				{
					SetNonPersistentPropertyValue(ExportUnionUserCodeInfo, ref exportUnionUsercode, value);
					Validation.ValidateAll();
				}
			}
		}

		ZString exportUnionUsercode;

		public ZPropertyInfo ExportUnionUserCodeInfo => GetZPropertyInfo(Schema.ExportUnionUserCode);

		#endregion

		#region Export Union User Password

		[MaxLength(15)]
		[ResourceStringData("Enterprise.Customs.TR.Business.FTPSettings|ExportUnionUserPassword", Caption = "Export Union User Password", ShortCaption = "Ex.Un.User Pass")]
		public ZString ExportUnionUserPassword
		{
			get => exportUnionUserPassword;
			set
			{
				if (exportUnionUserPassword != value)
				{
					SetNonPersistentPropertyValue(ExportUnionUserPasswordInfo, ref exportUnionUserPassword, value);
					Validation.ValidateAll();
				}
			}
		}

		ZString exportUnionUserPassword;

		public ZPropertyInfo ExportUnionUserPasswordInfo => GetZPropertyInfo(Schema.ExportUnionUserPassword);

		#endregion

		#region Export Union Payment Password

		[MaxLength(15)]
		[ResourceStringData("Enterprise.Customs.TR.Business.FTPSettings|ExportUnionPaymentPassword", Caption = "Export Union Payment Password", ShortCaption = "Ex.Un.Pay Pass")]
		public ZString ExportUnionPaymentPassword
		{
			get => exportUnionPaymentPassword;
			set
			{
				if (exportUnionPaymentPassword != value)
				{
					SetNonPersistentPropertyValue(ExportUnionPaymentPasswordInfo, ref exportUnionPaymentPassword, value);
					Validation.ValidateAll();
				}
			}
		}

		ZString exportUnionPaymentPassword;

		public ZPropertyInfo ExportUnionPaymentPasswordInfo => GetZPropertyInfo(Schema.ExportUnionPaymentPassword);

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new FTPSettings(fallbackLevel, factory);
		}

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			FTPAddress = reader.ReadElementString(Schema.FTPAddress);
			Port = reader.ReadElementStringAsZInt(Schema.Port);
			Inbox = reader.ReadElementString(Schema.Inbox);
			Outbox = reader.ReadElementString(Schema.Outbox);
			ExportUnionUserCode = reader.ReadElementString(Schema.ExportUnionUserCode);
			ExportUnionUserPassword = reader.ReadElementString(Schema.ExportUnionUserPassword);
			ExportUnionPaymentPassword = reader.ReadElementString(Schema.ExportUnionPaymentPassword);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.FTPAddress, FTPAddress);
			writer.WriteElementString(Schema.Port, Port.ToString());
			writer.WriteElementString(Schema.Inbox, Inbox);
			writer.WriteElementString(Schema.Outbox, Outbox);
			writer.WriteElementString(Schema.ExportUnionUserCode, ExportUnionUserCode);
			writer.WriteElementString(Schema.ExportUnionUserPassword, ExportUnionUserPassword);
			writer.WriteElementString(Schema.ExportUnionPaymentPassword, ExportUnionPaymentPassword);
		}

		#endregion

		#region Default Values

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			FTPAddress = ExportUnionFTPAddressList.Codes.FtpistanbulEbirlikNet;
			Port = 21;
			Inbox = (NoResString)"inbox";
			Outbox = (NoResString)"outbox";
		}

		#endregion

		#region Validation

		FTPSettingsValidation validation;

		public FTPSettingsValidation Validation => validation ?? (validation = new FTPSettingsValidation(this));

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion
	}
}
