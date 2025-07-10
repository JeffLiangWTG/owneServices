using System;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NO.Registry;

[XmlSerializerAssembly("Enterprise.Customs.NO.Business.XmlSerializers")]
public class NodiRegistry : RegistryBusinessObjectTemplate
{
	public NodiRegistry()
		: base()
	{
	}

	public NodiRegistry(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(fallbackLevel, factory)
	{
	}

	public static class Schema
	{
		public const string SystemName = nameof(NodiRegistry.SystemName);
		public const string NodiNumber = nameof(NodiRegistry.NodiNumber);
	}

	[ReadOnly(true)]
	[ResourceStringData("58B58C47-FDB7-401A-A8D8-2A3DCAB1DE8B", Caption = "System")]
	public ZString SystemName
	{
		get => systemName;
		set
		{
			SetNonPersistentPropertyValue(SystemNameInfo, ref systemName, value);
			SystemNameInfo.RefreshBinding();
		}
	}
	ZString systemName;

	public ZPropertyInfo SystemNameInfo => GetZPropertyInfo(Schema.SystemName);

	[ResourceStringData("8F9E6C85-0AFC-402D-A963-2DCDCF5F40FB", Caption = "NODI Id")]
	public ZString NodiNumber
	{
		get => nodiNumber;
		set
		{
			SetNonPersistentPropertyValue(NodiNumberInfo, ref nodiNumber, value);
			if (!IsValidationSuspended)
			{
				ValidateNodiNumber();
			}
			NodiNumberInfo.RefreshBinding();
		}
	}
	ZString nodiNumber;

	public ZPropertyInfo NodiNumberInfo => GetZPropertyInfo(Schema.NodiNumber);

	public void ValidateNodiNumber()
	{
		NodiNumberInfo.ClearAllNotifications();
		MandatoryValidation.CheckEntered(NodiNumberInfo);
	}

	public static ZString CurrentCustomsProductionNodiNumber => NOCustomsDataRegistry.Instance.CustomsNodiId.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)
																.Cast<NodiRegistry>()
																.Single(x => x.SystemName == CustomsProductionSystemName)
																.NodiNumber;
	public static ZString CurrentCustomsTestNodiNumber => NOCustomsDataRegistry.Instance.CustomsNodiId.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)
															.Cast<NodiRegistry>()
															.Single(x => x.SystemName == CustomsTestSystemName)
															.NodiNumber;
	public static ZString CurrentNctsProductionNodiNumber => NOCustomsDataRegistry.Instance.CustomsNodiId.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)
																.Cast<NodiRegistry>()
																.Single(x => x.SystemName == NctsProductionSystemName)
																.NodiNumber;
	public static ZString CurrentNctsTestNodiNumber => NOCustomsDataRegistry.Instance.CustomsNodiId.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)
														.Cast<NodiRegistry>()
														.Single(x => x.SystemName == NctsTestSystemName)
														.NodiNumber;

	public const string CustomsProductionSystemName = "TVINN";
	public const string CustomsProductionDefaultNodiNumber = "NO000101";
	public const string CustomsTestSystemName = "TVINN_TEST";
	public const string CustomsTestDefaultNodiNumber = "NO000119";
	public const string NctsProductionSystemName = "NCTS";
	public const string NctsProductionDefaultNodiNumber = "NCTS.PROD";
	public const string NctsTestSystemName = "NCTS_TEST";
	public const string NctsTestDefaultNodiNumber = "NCTS.TEST";

	protected override void ReadElements(XmlReaderWrapper reader)
	{
		SystemName = reader.ReadElementString(Schema.SystemName);
		NodiNumber = reader.ReadElementString(Schema.NodiNumber);
	}

	protected override void WriteElements(XmlWriter writer)
	{
		base.WriteElements(writer);
		writer.WriteElementString(Schema.SystemName, SystemName);
		writer.WriteElementString(Schema.NodiNumber, NodiNumber);
	}

	protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new NodiRegistry(fallbackLevel, factory);
}
