using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.TR.Business.XmlSerializers")]
	public class NCTSPhase5Credentials : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string BasicAuthUsername = "BasicAuthUsername";
			public const string BasicAuthPassword = "BasicAuthPassword";
			public const string FirmID = "FirmID";
			public const string RequestUserID = "RequestUserID";
			public const string RequestPassword = "RequestPassword";
		}

		#endregion

		#region Constructors

		public NCTSPhase5Credentials() : base()
		{
		}

		public NCTSPhase5Credentials(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		#endregion

		#region BasicAuthUsername

		[ResourceStringData("Enterprise.Customs.TR.Business.NCTSPhase5Credentials|BasicAuthUsername", Caption = "Basic Auth Username")]
		public ZString BasicAuthUsername
		{
			get => basicAuthUsername;
			set
			{
				if (basicAuthUsername != value)
				{
					SetNonPersistentPropertyValue(BasicAuthUsernameInfo, ref basicAuthUsername, value);
				}
			}
		}

		ZString basicAuthUsername;

		public ZPropertyInfo BasicAuthUsernameInfo => GetZPropertyInfo(Schema.BasicAuthUsername);

		#endregion

		#region BasicAuthPassword

		[ResourceStringData("Enterprise.Customs.TR.Business.NCTSPhase5Credentials|BasicAuthPassword", Caption = "Basic Auth Password")]
		public ZString BasicAuthPassword
		{
			get => basicAuthPassword;
			set
			{
				if (basicAuthPassword != value)
				{
					SetNonPersistentPropertyValue(BasicAuthPasswordInfo, ref basicAuthPassword, value);
				}
			}
		}

		ZString basicAuthPassword;

		public ZPropertyInfo BasicAuthPasswordInfo => GetZPropertyInfo(Schema.BasicAuthPassword);

		#endregion

		#region FirmID

		[ResourceStringData("Enterprise.Customs.TR.Business.NCTSPhase5Credentials|FirmID", Caption = "Firm ID")]
		public ZString FirmID
		{
			get => firmID;
			set
			{
				if (firmID != value)
				{
					SetNonPersistentPropertyValue(FirmIDInfo, ref firmID, value);
				}
			}
		}

		ZString firmID;

		public ZPropertyInfo FirmIDInfo => GetZPropertyInfo(Schema.FirmID);

		#endregion

		#region RequestUserID

		[ResourceStringData("Enterprise.Customs.TR.Business.NCTSPhase5Credentials|RequestUserID", Caption = "Request User ID")]
		public ZString RequestUserID
		{
			get => requestUserID;
			set
			{
				if (requestUserID != value)
				{
					SetNonPersistentPropertyValue(RequestUserIDInfo, ref requestUserID, value);
				}
			}
		}

		ZString requestUserID;

		public ZPropertyInfo RequestUserIDInfo => GetZPropertyInfo(Schema.RequestUserID);

		#endregion

		#region RequestPassword

		[ResourceStringData("Enterprise.Customs.TR.Business.NCTSPhase5Credentials|RequestPassword", Caption = "Request Password")]
		public ZString RequestPassword
		{
			get => requestPassword;
			set
			{
				if (requestPassword != value)
				{
					SetNonPersistentPropertyValue(RequestPasswordInfo, ref requestPassword, value);
				}
			}
		}

		ZString requestPassword;

		public ZPropertyInfo RequestPasswordInfo => GetZPropertyInfo(Schema.RequestPassword);

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new NCTSPhase5Credentials(fallbackLevel, factory);
		}

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			BasicAuthUsername = reader.ReadElementString(Schema.BasicAuthUsername);
			BasicAuthPassword = reader.ReadElementString(Schema.BasicAuthPassword);
			FirmID = reader.ReadElementString(Schema.FirmID);
			RequestUserID = reader.ReadElementString(Schema.RequestUserID);
			RequestPassword = reader.ReadElementString(Schema.RequestPassword);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.BasicAuthUsername, BasicAuthUsername);
			writer.WriteElementString(Schema.BasicAuthPassword, BasicAuthPassword);
			writer.WriteElementString(Schema.FirmID, FirmID);
			writer.WriteElementString(Schema.RequestUserID, RequestUserID);
			writer.WriteElementString(Schema.RequestPassword, RequestPassword);
		}

		#endregion

		#region Default Values

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			BasicAuthUsername = (NoResString)"NCTSTraderUser";
			BasicAuthPassword = (NoResString)"3vAT.2G98Ar!";
			FirmID = (NoResString)"WiseTech";
		}

		#endregion

		#region Validation

		NCTSPhase5CredentialsValidation validation;

		public NCTSPhase5CredentialsValidation Validation => validation ?? (validation = new NCTSPhase5CredentialsValidation(this));

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion
	}
}
