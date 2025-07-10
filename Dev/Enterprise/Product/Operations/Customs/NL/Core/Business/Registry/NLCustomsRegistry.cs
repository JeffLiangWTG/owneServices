using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NL.Business;

public sealed class NLCustomsRegistry : RegistryItemSet
{
	public static NLCustomsRegistry Instance
	{
		get { return instance ?? (instance = new NLCustomsRegistry()); }
	}

	[ThreadStatic]
	static NLCustomsRegistry instance;

	NLCustomsRegistry()
	{
	}

	protected override void SetDefaultsForNewItem(IRegistryItem item)
	{
		base.SetDefaultsForNewItem(item);
		item.CountryFilterPKs = CountryFilterPKs.Netherlands;
	}

	public override bool IsForProductivityWise => false;

	public abstract class Categories : RawDataRegistry.Categories
	{
		public static MultilingualString Customs_Netherlands_DMS { get { return CombineCategories(Customs_Netherlands, (NoResString)"DMS"); } }
		public static MultilingualString Customs_Netherlands_DVA { get { return CombineCategories(Customs_Netherlands, (NoResString)"DVA"); } }
		public static MultilingualString Customs_Netherlands_Fallback { get { return CombineCategories(Customs_Netherlands, (NoResString)"Fallback"); } }
		public static MultilingualString Customs_Netherlands_Fallback_FallbackTimerInMinutes { get { return CombineCategories(Customs_Netherlands_Fallback, (NoResString)"Fallback timer in minutes"); } }
		public static MultilingualString Customs_Netherlands_Fallback_FallbackConfiguration { get { return CombineCategories(Customs_Netherlands_Fallback, (NoResString)"Fallback Configuration"); } }
	}

	public const int EntryReferenceNumberMaxLength = 35;

	public MessageVersionRegistryItem CustomsMessageVersion
	{
		get
		{
			return GetItem("NLCustomsMessageVersion", delegate
			{
				return new MessageVersionRegistryItem(
					"NLCustomsMessageVersion",
					Categories.Customs_Netherlands,
					ResString.GetMultilingualString("AD5B6084-6DD1-41CB-BDFD-B4ED1B1C55D3", "Customs Message Recipient IDs"),
					ResString.GetMultilingualString("B2215A4F-FDE9-46EA-920F-D499BA6BB234", "Current version of customs messages the company is configured at customs to submit."),
					RegistryStorageFlags.Company,
					MessageVersionRegistryCollection.DefaultCollection);
			});
		}
	}

	public SenderInfoRegistryItem SenderIDs
	{
		get
		{
			return GetItem("NLSenderIDs", delegate
			{
				return new SenderInfoRegistryItem(
					"NLSenderIDs",
					Categories.Customs_Netherlands,
					ResString.GetMultilingualString("7024B665-1FB5-48A2-B410-0D140806B115", "HTG Sender ID"),
					ResString.GetMultilingualString("4474CF1B-E231-4DC2-A7B1-08DD3751FED0", "A unique sender ID for the message header of declarations."),
					RegistryStorageFlags.Company);
			});
		}
	}

	public BooleanRegistryItem IsNLTestingSystem
	{
		get
		{
			return GetItem("IsNLTestingSystem", delegate
			{
				return new BooleanRegistryItem(
					"IsNLTestingSystem",
					RawDataRegistry.Categories.Customs_Netherlands,
					ResString.GetMultilingualString("6D7439E1-B1FB-4561-9845-EDA60D1A1FFD", "Is NL Testing System?"),
					ResString.GetMultilingualString("DB38E5CE-FA5C-4724-B081-E91F0F4709D8", "NL Messages be sent to the Test rather than Production System?"),
					RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false);
			});
		}
	}

	public BillCustomisationRegistryItem EntryNumberCustomisation
	{
		get
		{
			return GetItem("EntryNumberCustomisation", () => new BillCustomisationRegistryItem(
				"EntryNumberCustomisation",
				Categories.Customs_Netherlands_DMS,
				ResString.GetMultilingualString("5B739EE0-7CAC-4192-8E94-198E67D7DD44", "Entry Number Customization"),
				ResString.GetMultilingualString("D9A17C5E-E50F-42EE-8D6B-302AE9724D91", "Override this value to customize how Entry Number values are formatted"),
				RegistryStorageFlags.Company,
				new EntryNumberCustomisationRegistryDataType()
			));
		}
	}

	public BillCustomisationRegistryItem NLNctsFallbackEntryNumberCustomisation
	{
		get
		{
			return GetItem("NLNctsFallbackEntryNumberCustomisation", () => new BillCustomisationRegistryItem(
				"NLNctsFallbackEntryNumberCustomisation",
				Categories.Customs_Netherlands_DVA,
				ResString.GetMultilingualString("BA16EA1E-E288-4881-B10C-91695A256429", "DVA Departure - Emergency procedure sequence numbers"),
				ResString.GetMultilingualString("B18A0809-4F15-4928-8A7D-967F04252C43", "The sequence preferences for DVA Departure emergency procedure documents"),
				RegistryStorageFlags.Company,
				new NLNctsFallbackEntryNumberCustomisationRegistryDataType()
			));
		}
	}

	public CalCalculationMethodRegistryItem CalCalculationMethod
	{
		get
		{
			return GetItem("NLCalCalculationMethod", delegate
			{
				return new CalCalculationMethodRegistryItem(
					"NLCalCalculationMethod",
					Categories.Customs_Netherlands_DVA,
					ResString.GetMultilingualString("70E3C616-CBAF-4579-84D4-655FA814B774", "CAL Calculation Method"),
					ResString.GetMultilingualString("E9F2F15A-7893-4556-BFBD-1B4BC03F0A04", "The default CAL calculation method for DVA declarations"),
					RegistryStorageFlags.Company,
					CalCalculationMethodRegistryCollection.DefaultCollection);
			});
		}
	}

	public	IntRegistryItem FallbackTimerInMinutes_DMSExport
	{
		get
		{
			return GetItem("NLFallbackTimerInMinutesDMSExport", delegate
			{
				return new IntRegistryItem(
					"NLFallbackTimerInMinutesDMSExport",
					Categories.Customs_Netherlands_Fallback_FallbackTimerInMinutes,
					ResString.GetMultilingualString("C5F1C82C-BFFB-4884-888F-776820F35BF8", "DMS Export"),
					ResString.GetMultilingualString("D407C1B8-259C-4B1C-B538-CBECF700E096", "Fallback timer in minutes for DMS Export"),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.Default,
					30);
			});
		}
	}

	public IntRegistryItem FallbackTimerInMinutes_DMSImport
	{
		get
		{
			return GetItem("NLFallbackTimerInMinutesDMSImport", delegate
			{
				return new IntRegistryItem(
					"NLFallbackTimerInMinutesDMSImport",
					Categories.Customs_Netherlands_Fallback_FallbackTimerInMinutes,
					ResString.GetMultilingualString("25C536B3-3339-4607-B488-51D785AE99F9", "DMS Import"),
					ResString.GetMultilingualString("4186D407-6C63-40AC-83CC-470395B1E945", "Fallback timer in minutes for DMS Import"),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.Default,
					30);
			});
		}
	}

	public IntRegistryItem FallbackTimerInMinutes_DVA
	{
		get
		{
			return GetItem("NLFallbackTimerInMinutesDVA", delegate
			{
				return new IntRegistryItem(
					"NLFallbackTimerInMinutesDVA",
					Categories.Customs_Netherlands_Fallback_FallbackTimerInMinutes,
					ResString.GetMultilingualString("B5F8DF1F-FC35-4C29-A1F5-C90253F21276", "DVA"),
					ResString.GetMultilingualString("AB74CC44-E720-43EB-96E4-5C41E46F6C49", "Fallback timer in minutes for DVA"),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.Default,
					30);
			});
		}
	}

	public IntRegistryItem FallbackTimerInMinutes_ECS
	{
		get
		{
			return GetItem("NLFallbackTimerInMinutesECS", delegate
			{
				return new IntRegistryItem(
					"NLFallbackTimerInMinutesECS",
					Categories.Customs_Netherlands_Fallback_FallbackTimerInMinutes,
					ResString.GetMultilingualString("423D3CB4-252B-4DC3-B7CB-170A29B06644", "ECS"),
					ResString.GetMultilingualString("E225F47E-3328-4F3B-A3A1-6D7B1983C948", "Fallback timer in minutes for ECS"),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.Default,
					30);
			});
		}
	}

	public IntRegistryItem FallbackTimerInMinutes_Portbase
	{
		get
		{
			return GetItem("NLFallbackTimerInMinutesPortbase", delegate
			{
				return new IntRegistryItem(
					"NLFallbackTimerInMinutesPortbase",
					Categories.Customs_Netherlands_Fallback_FallbackTimerInMinutes,
					ResString.GetMultilingualString("C6F0EDFB-1639-4D64-B087-D88EE2CFECBD", "Portbase"),
					ResString.GetMultilingualString("99C31E72-7DBC-4CAF-873C-F19166A482C1", "Fallback timer in minutes for Portbase"),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.Default,
					30);
			});
		}
	}

	public FallbackConfigurationRegistryItem FallbackConfiguration_DMSExport
	{
		get
		{
			return GetItem("NLFallbackConfigurationDMSExport", delegate
			{
				var result = new FallbackConfigurationRegistryItem(
					"NLFallbackConfigurationDMSExport",
					Categories.Customs_Netherlands_Fallback_FallbackConfiguration,
					ResString.GetMultilingualString("733E992B-5ABB-4CF6-8401-0AD98F232AE4", "DMS Export"),
					ResString.GetMultilingualString("C54FC6A2-AC20-47EA-B1A8-91AB781C3CE2", "Fallback configuration for DMS Export"),
					RegistryStorageFlags.System | RegistryStorageFlags.Company);
				result.DataType = new FallbackConfigurationRegistryItem.FallbackConfigurationRegistryDataType();
				return result;
			});
		}
	}

	public IRegistryItem FallbackConfiguration_DMSImport
	{
		get
		{
			return GetItem("NLFallbackConfigurationDMSImport", delegate
			{
				var result = new FallbackConfigurationRegistryItem(
					"NLFallbackConfigurationDMSImport",
					Categories.Customs_Netherlands_Fallback_FallbackConfiguration,
					ResString.GetMultilingualString("44DEF7A6-9DDB-4B63-8076-73BF5CE8712A", "DMS Import"),
					ResString.GetMultilingualString("ED008FB6-8AF7-4D1B-A8B7-13487B8948CE", "Fallback configuration for DMS Import"),
					RegistryStorageFlags.System | RegistryStorageFlags.Company);
				result.DataType = new FallbackConfigurationRegistryItem.FallbackConfigurationRegistryDataType();
				return result;
			});
		}
	}

	public IRegistryItem FallbackConfiguration_DVA
	{
		get
		{
			return GetItem("NLFallbackConfigurationDVA", delegate
			{
				var result = new FallbackConfigurationRegistryItem(
					"NLFallbackConfigurationDVA",
					Categories.Customs_Netherlands_Fallback_FallbackConfiguration,
					ResString.GetMultilingualString("1503EAE9-A8D2-4644-B2D2-59DA2D4DBF6A", "DVA"),
					ResString.GetMultilingualString("0BAA5A1B-65C8-41F3-995F-F734A2AEEBCC", "Fallback configuration for DVA"),
					RegistryStorageFlags.System | RegistryStorageFlags.Company);
				result.DataType = new FallbackConfigurationRegistryItem.FallbackConfigurationRegistryDataType();
				return result;
			});
		}
	}

	public IRegistryItem FallbackConfiguration_ECS
	{
		get
		{
			return GetItem("NLFallbackConfigurationECS", delegate
			{
				var result = new FallbackConfigurationRegistryItem(
					"NLFallbackConfigurationECS",
					Categories.Customs_Netherlands_Fallback_FallbackConfiguration,
					ResString.GetMultilingualString("933F7FEB-7BC8-4BF8-90D5-65CB00924351", "ECS"),
					ResString.GetMultilingualString("30985550-1DF7-4F38-911B-7302BC7A0CBC", "Fallback configuration for ECS"),
					RegistryStorageFlags.System | RegistryStorageFlags.Company);
				result.DataType = new FallbackConfigurationRegistryItem.FallbackConfigurationRegistryDataType();
				return result;
			});
		}
	}

	public IRegistryItem FallbackConfiguration_Portbase
	{
		get
		{
			return GetItem("NLFallbackConfigurationPortbase", delegate
			{
				var result = new FallbackConfigurationRegistryItem(
					"NLFallbackConfigurationPortbase",
					Categories.Customs_Netherlands_Fallback_FallbackConfiguration,
					ResString.GetMultilingualString("FE05F304-1E36-43F3-B9F3-68FB75FC4E4B", "Portbase"),
					ResString.GetMultilingualString("8E34BFD2-EF31-452F-8F3A-FBFC92D88952", "Fallback configuration for Portbase"),
					RegistryStorageFlags.System | RegistryStorageFlags.Company);
				result.DataType = new FallbackConfigurationRegistryItem.FallbackConfigurationRegistryDataType();
				return result;
			});
		}
	}

	public IntRegistryItem FallbackRegularizationTimerInMinutes
	{
		get
		{
			return GetItem("NLFallbackRegularizationTimerInMinutes", delegate
			{
				return new IntRegistryItem(
					"NLFallbackRegularizationTimerInMinutes",
					Categories.Customs_Netherlands_Fallback,
					ResString.GetMultilingualString("6189F03A-1273-4DA6-B61B-BDBE021075AC", "Regularization timer in minutes"),
					ResString.GetMultilingualString("048C468D-10E6-4586-9D38-6E5B9DF5488A", "Regularization timer in minutes"),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.Default,
					5);
			});
		}
	}

	public CodePairRegistryItem FallbackEmail
	{
		get
		{
			return GetItem(nameof(FallbackEmail), delegate
			{
				return new CodePairRegistryItem(
					nameof(FallbackEmail),
					Categories.Customs_Netherlands_Fallback,
					ResString.GetMultilingualString("A5D83435-DD0D-45E4-AC4B-C6D6C9C227B0", "Send email to Customs during fallback"),
					ResString.GetMultilingualString("2023694D-73AE-4720-A7FB-B2D4CA393EE7", "The field below defines how the emails to Customs are sent to Customs during Fallback procedure"),
					new CodeDescriptionPairListProvider(() => new EmailFallbackList()),
					RegistryStorageFlags.Company,
					EmailFallbackList.Codes.MNL);
			});
		}
	}
}
