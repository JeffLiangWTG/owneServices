using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.NO.Registry;

[XmlSerializerAssembly("Enterprise.Customs.NO.Business.XmlSerializers")]
public class FTPSettingsCustomsRegistry : FTPSettingsRegistry
{
	public FTPSettingsCustomsRegistry()
	{
	}

	public FTPSettingsCustomsRegistry(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
	{
	}

	public new static class Schema
	{
		public const string Username = nameof(FTPSettingsRegistry.Username);
		public const string Password = nameof(FTPSettingsRegistry.Password);
		public const string Url = nameof(FTPSettingsRegistry.Url);
		public const string Port = nameof(FTPSettingsRegistry.Port);
		public const string SendToCustomFolder = nameof(FTPSettingsCustomsRegistry.SendToCustomFolder);
		public const string ReceiveFromCustomFolder = nameof(FTPSettingsCustomsRegistry.ReceiveFromCustomFolder);
	}

	#region SendToCustomFolder

	[ResourceStringData("11B11AC9-00B5-4F1B-956B-243D884A2C62", Caption = "Folder for send to Customs")]
	public ZString SendToCustomFolder
	{
		get => sendToCustomFolder;
		set
		{
			SetNonPersistentPropertyValue(SendToCustomFolderInfo, ref sendToCustomFolder, value);
			if (!IsValidationSuspended)
			{
				ValidateSendToCustomFolder();
			}
		}
	}
	ZString sendToCustomFolder;

	public ZPropertyInfo SendToCustomFolderInfo => GetZPropertyInfo(Schema.SendToCustomFolder);

	void ValidateSendToCustomFolder()
	{
		SendToCustomFolderInfo.ClearAllNotifications();
		MandatoryValidation.CheckEntered(SendToCustomFolderInfo);
	}

	#endregion

	#region ReceiveFromCustomFolder

	[ResourceStringData("EAB4A212-1B9D-4AEC-9BF9-3BDFEDEE642A", Caption = "Folder for receive from Customs")]
	public ZString ReceiveFromCustomFolder
	{
		get => receiveFromCustomFolder;
		set
		{
			SetNonPersistentPropertyValue(ReceiveFromCustomFolderInfo, ref receiveFromCustomFolder, value);
			if (!IsValidationSuspended)
			{
				ValidateReceiveFromCustomFolder();
			}
		}
	}
	ZString receiveFromCustomFolder;

	public ZPropertyInfo ReceiveFromCustomFolderInfo => GetZPropertyInfo(Schema.ReceiveFromCustomFolder);

	void ValidateReceiveFromCustomFolder()
	{
		ReceiveFromCustomFolderInfo.ClearAllNotifications();
		MandatoryValidation.CheckEntered(ReceiveFromCustomFolderInfo);
	}

	#endregion

	#region DefaultValues

	public static FTPSettingsCustomsRegistry DefaultValues => defaultValueLazy.Value;

	[ThreadSafe]
	static readonly Lazy<FTPSettingsCustomsRegistry> defaultValueLazy = new(() => new()
	{
		Url = Constants.URL,
		Port = Constants.Port,
		SendToCustomFolder = Constants.SendToCustomFolder,
		ReceiveFromCustomFolder = Constants.ReceiveFromCustomFolder,
	});

	static class Constants
	{
		public const string URL = "ftp.ec.evry.com";
		public const string Port = "21";
		public static readonly string SendToCustomFolder = (NoResString)"in";
		public static readonly string ReceiveFromCustomFolder = (NoResString)"out";
	}

	#endregion

	protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		=> new FTPSettingsCustomsRegistry(fallbackLevel, factory);

	protected override void ReadElements(XmlReaderWrapper reader)
	{
		base.ReadElements(reader);
		SendToCustomFolder = reader.ReadElementString(Schema.SendToCustomFolder);
		ReceiveFromCustomFolder = reader.ReadElementString(Schema.ReceiveFromCustomFolder);
	}

	protected override void WriteElements(XmlWriter writer)
	{
		base.WriteElements(writer);
		writer.WriteElementString(Schema.SendToCustomFolder, SendToCustomFolder);
		writer.WriteElementString(Schema.ReceiveFromCustomFolder, ReceiveFromCustomFolder);
	}

	protected override void RunPreSaveValidationCore()
	{
		if (!IsValidationSuspended)
		{
			base.RunPreSaveValidationCore();
			ValidateSendToCustomFolder();
			ValidateReceiveFromCustomFolder();
		}
	}
}
