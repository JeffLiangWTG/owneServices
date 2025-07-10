using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NO.Registry;

[XmlSerializerAssembly("Enterprise.Customs.NO.Business.XmlSerializers")]
public class FTPSettingsRegistry : RegistryBusinessObjectTemplate
{
	public FTPSettingsRegistry()
	{
	}

	public FTPSettingsRegistry(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
	{
	}

	public static class Schema
	{
		public const string Username = nameof(FTPSettingsRegistry.Username);
		public const string Password = nameof(FTPSettingsRegistry.Password);
		public const string Url = nameof(FTPSettingsRegistry.Url);
		public const string Port = nameof(FTPSettingsRegistry.Port);
	}

	#region Username

	[ResourceStringData("A4EE5CAD-3F3A-45A3-B3F9-C7606D69DE2D", Caption = "Username")]
	public ZString Username
	{
		get => username;
		set
		{
			SetNonPersistentPropertyValue(UsernameInfo, ref username, value);
			if (!IsValidationSuspended)
			{
				ValidateUserName();
			}
		}
	}
	ZString username;

	public ZPropertyInfo UsernameInfo => GetZPropertyInfo(Schema.Username);

	void ValidateUserName()
	{
		UsernameInfo.ClearAllNotifications();
		MandatoryValidation.CheckEntered(UsernameInfo);
	}

	#endregion

	#region Password

	[Password]
	[ResourceStringData("C3561612-63B3-46AB-9501-A87CA6C14876", Caption = "Password")]
	public ZString Password
	{
		get => password;
		set
		{
			SetNonPersistentPropertyValue(PasswordInfo, ref password, value);
			if (!IsValidationSuspended)
			{
				ValidatePassword();
			}
		}
	}
	ZString password;

	public ZPropertyInfo PasswordInfo => GetZPropertyInfo(Schema.Password);

	void ValidatePassword()
	{
		PasswordInfo.ClearAllNotifications();
		MandatoryValidation.CheckEntered(PasswordInfo);
	}

	#endregion

	#region Url

	[ResourceStringData("D68926CD-3679-4F5E-820D-DD9577ABAB5F", Caption = "URL")]
	public ZString Url
	{
		get => url;
		set
		{
			SetNonPersistentPropertyValue(UrlInfo, ref url, value);
			if (!IsValidationSuspended)
			{
				ValidateUrl();
			}
		}
	}
	ZString url;

	public ZPropertyInfo UrlInfo => GetZPropertyInfo(Schema.Url);

	void ValidateUrl()
	{
		UrlInfo.ClearAllNotifications();
		MandatoryValidation.CheckEntered(UrlInfo);
	}

	#endregion

	#region Port

	[ResourceStringData("D90DE772-DF23-4DEC-B02A-8A8F56FE54C4", Caption = "Port")]
	public ZString Port
	{
		get => port;
		set
		{
			SetNonPersistentPropertyValue(PortInfo, ref port, value);
			if (!IsValidationSuspended)
			{
				ValidatePort();
			}
		}
	}
	ZString port;

	public ZPropertyInfo PortInfo => GetZPropertyInfo(Schema.Port);

	void ValidatePort()
	{
		PortInfo.ClearAllNotifications();
		MandatoryValidation.CheckEntered(PortInfo);
	}

	#endregion

	protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		=> new FTPSettingsRegistry(fallbackLevel, factory);

	protected override void ReadElements(XmlReaderWrapper reader)
	{
		Username = reader.ReadElementString(Schema.Username);
		Password = reader.ReadElementString(Schema.Password);
		Url = reader.ReadElementString(Schema.Url);
		Port = reader.ReadElementString(Schema.Port);
	}

	protected override void WriteElements(XmlWriter writer)
	{
		base.WriteElements(writer);
		writer.WriteElementString(Schema.Username, Username);
		writer.WriteElementString(Schema.Password, Password);
		writer.WriteElementString(Schema.Url, Url);
		writer.WriteElementString(Schema.Port, Port);
	}

	protected override void RunPreSaveValidationCore()
	{
		if (!IsValidationSuspended)
		{
			ValidateUserName();
			ValidatePassword();
			ValidateUrl();
			ValidatePort();
		}
	}
}
