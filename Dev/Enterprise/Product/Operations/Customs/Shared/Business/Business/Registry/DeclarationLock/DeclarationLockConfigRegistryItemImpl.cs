using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DataRegistry.Business
{
	sealed class DeclarationLockConfigRegistryItemImpl : RegistryItemImpl
	{
		public DeclarationLockConfigRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(name, category, caption, hint, new DeclarationLockConfigRegistryItemDataType(), storage, options)
		{
		}

		protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var company = companyPK == EnvProxy.Instance.CurrentCompany.PK
									? EnvProxy.Instance.CurrentCompany
									: (ICompany)((IFactoryProvider)Env.CurrentCompany).Factory.Load<IGlbCompany>(companyPK);

			var countryCode = company?.Country?.Code ?? string.Empty;
			var fallbackLevel = new FallbackLevel(companyPK, branchPK, departmentPK);

			switch (countryCode)
			{
				case CountryCodes.Spain:
					return GetDefaultLockConfigForES(fallbackLevel);
				case CountryCodes.Switzerland:
					return GetDefaultLockConfigForCH(fallbackLevel);
				case CountryCodes.Italy:
					return GetDefaultLockConfigForIT(fallbackLevel);
				default:
					return base.GetDefaultValueCore(companyPK, branchPK, departmentPK);
			}
		}

		internal DeclarationLockConfigCollection GetDefaultLockConfigForES(FallbackLevel fallbackLevel)
		{
			const string eventCLR_Reference = "CLR";
			const string eventCDA_Reference = "CDA";

			var configCollection = new DeclarationLockConfigCollection(fallbackLevel);

			var impConfig = AddDeclarationLockConfig(configCollection, "IMP", Core.Constants.Customs.DeclarationLockModes.Codes.All);
			AddEventInfo(impConfig, Events.CustomsEntryStatusCode, Core.Constants.Customs.EventLockSourceTypes.Codes.EntryHeader, eventCLR_Reference);
			AddEventInfo(impConfig, Events.CustomsEntryStatusCode, Core.Constants.Customs.EventLockSourceTypes.Codes.EntryHeader, eventCDA_Reference);
			AddTabInfo(impConfig);

			var expConfig = AddDeclarationLockConfig(configCollection, "EXP", Core.Constants.Customs.DeclarationLockModes.Codes.All);
			AddEventInfo(expConfig, Events.CustomsEntryStatusCode, Core.Constants.Customs.EventLockSourceTypes.Codes.EntryHeader, eventCLR_Reference);
			AddEventInfo(expConfig, Events.CustomsEntryStatusCode, Core.Constants.Customs.EventLockSourceTypes.Codes.EntryHeader, eventCDA_Reference);
			AddTabInfo(expConfig);

			return configCollection;
		}

		internal DeclarationLockConfigCollection GetDefaultLockConfigForCH(FallbackLevel fallbackLevel)
		{
			const string arrivalDeclarationType = "ARN";
			const string unloadingDeclarationType = "ULR";
			const string impDeclarationType = "IMP";
			const string expDeclarationType = "EXP";
			const string edaDeclarationType = "EDA";

			const string eventACC_Reference = "ACC";
			const string eventACC_ReferenceWithWildCard = "*NEW=ACC*";
			const string eventCL1_Reference = "CL1";
			const string eventCL3_Reference = "CL3";
			const string eventALL_Reference = "*";

			var configCollection = new DeclarationLockConfigCollection(fallbackLevel);

			var depConfig = AddDeclarationLockConfig(configCollection, DepartureDeclarationType, Core.Constants.Customs.DeclarationLockModes.Codes.All);
			AddEventInfo(depConfig, Events.MessageStatusChangeCode, Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader, eventACC_Reference);
			AddEventInfo(depConfig, Events.CustomsEntryStatusCode, Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader, eventALL_Reference);
			AddTabInfo(depConfig);

			var arnConfig = AddDeclarationLockConfig(configCollection, arrivalDeclarationType, Core.Constants.Customs.DeclarationLockModes.Codes.All);
			AddEventInfo(arnConfig, Events.MessageStatusChangeCode, Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader, eventACC_Reference);
			AddEventInfo(arnConfig, Events.CustomsEntryStatusCode, Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader, eventALL_Reference);
			AddTabInfo(arnConfig);

			var ulrConfig = AddDeclarationLockConfig(configCollection, unloadingDeclarationType, Core.Constants.Customs.DeclarationLockModes.Codes.All);
			AddEventInfo(ulrConfig, Events.CustomsEntryStatusCode, Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader, eventCL1_Reference);
			AddEventInfo(ulrConfig, Events.CustomsEntryStatusCode, Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader, eventCL3_Reference);
			AddTabInfo(ulrConfig, tabPage: Core.Constants.Customs.DeclarationTabPages.Codes.NctsArrivalUnloadingRemarks);

			var impConfig = AddDeclarationLockConfig(configCollection, impDeclarationType, Core.Constants.Customs.DeclarationLockModes.Codes.All);
			AddEventInfo(impConfig, Events.MessageStatusChangeCode, Core.Constants.Customs.EventLockSourceTypes.Codes.EntryHeader, eventACC_ReferenceWithWildCard);
			AddEventInfo(impConfig, Events.CustomsEntryStatusCode, Core.Constants.Customs.EventLockSourceTypes.Codes.EntryHeader, eventALL_Reference);
			AddTabInfo(impConfig);

			var expConfig = AddDeclarationLockConfig(configCollection, expDeclarationType, Core.Constants.Customs.DeclarationLockModes.Codes.All);
			AddEventInfo(expConfig, Events.MessageStatusChangeCode, Core.Constants.Customs.EventLockSourceTypes.Codes.EntryHeader, eventACC_ReferenceWithWildCard);
			AddEventInfo(expConfig, Events.CustomsEntryStatusCode, Core.Constants.Customs.EventLockSourceTypes.Codes.EntryHeader, eventALL_Reference);
			AddTabInfo(expConfig);

			var edaConfig = AddDeclarationLockConfig(configCollection, edaDeclarationType, Core.Constants.Customs.DeclarationLockModes.Codes.All);
			AddEventInfo(edaConfig, Events.CustomsEntryStatusCode, Core.Constants.Customs.EventLockSourceTypes.Codes.EntryHeader, eventALL_Reference);
			AddTabInfo(edaConfig);

			return configCollection;
		}

		internal DeclarationLockConfigCollection GetDefaultLockConfigForIT(FallbackLevel fallbackLevel)
		{
			const string eventMRN_Reference = "MRN";
			const string eventREL_Reference = "REL";
			const string eventCO3_Reference = "CO3";
			const string eventACS_Reference = "ACS";
			const string eventSNT_Reference = "SNT";
			const string eventACK_Reference = "ACK";
			const string eventTSA_Reference = "TSA";

			var configCollection = new DeclarationLockConfigCollection(fallbackLevel);

			var depConfig = AddDeclarationLockConfig(configCollection, DepartureDeclarationType, Core.Constants.Customs.DeclarationLockModes.Codes.All);
			AddEventInfo(depConfig, Events.CustomsEntryStatusCode, Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader, eventMRN_Reference);
			AddEventInfo(depConfig, Events.CustomsEntryStatusCode, Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader, eventREL_Reference);
			AddEventInfo(depConfig, Events.CustomsEntryStatusCode, Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader, eventCO3_Reference);
			AddEventInfo(depConfig, Events.CustomsEntryStatusCode, Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader, eventACS_Reference);
			AddEventInfo(depConfig, Events.MessageStatusChangeCode, Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader, eventSNT_Reference);
			AddTabInfo(depConfig);

			var tstConfig = AddDeclarationLockConfig(configCollection, Common.IT.ITJobMessageTypeList.Codes.TemporaryStorage, Core.Constants.Customs.DeclarationLockModes.Codes.All);
			AddEventInfo(tstConfig, Events.CustomsEntryStatusCode, Core.Constants.Customs.EventLockSourceTypes.Codes.TemporaryStorage, eventTSA_Reference);
			AddEventInfo(tstConfig, Events.MessageStatusChangeCode, Core.Constants.Customs.EventLockSourceTypes.Codes.TemporaryStorage, eventACK_Reference);
			AddEventInfo(tstConfig, Events.MessageStatusChangeCode, Core.Constants.Customs.EventLockSourceTypes.Codes.TemporaryStorage, eventACS_Reference);
			AddEventInfo(tstConfig, Events.MessageStatusChangeCode, Core.Constants.Customs.EventLockSourceTypes.Codes.TemporaryStorage, eventSNT_Reference);
			AddTabInfo(tstConfig);

			return configCollection;
		}

		DeclarationLockConfig AddDeclarationLockConfig(DeclarationLockConfigCollection collection, ZString declarationType, ZString lockMode)
		{
			var declarationLockConfig = collection.AddNew();
			using (declarationLockConfig.GetValidationSuspender())
			{
				declarationLockConfig.DeclarationType = declarationType;
				declarationLockConfig.LockMode = lockMode;
			}
			return declarationLockConfig;
		}

		void AddEventInfo(DeclarationLockConfig config, ZString eventType, ZString eventSource, ZString eventReference)
		{
			var configEvent = config.EventInfos.AddNew();
			using (configEvent.GetValidationSuspender())
			{
				configEvent.EventType = eventType;
				configEvent.EventSource = eventSource;
				configEvent.EntryType = Core.Constants.Customs.EntryHeaderTypes.Codes.All;
				configEvent.EventReference = eventReference;
			}
		}

		void AddTabInfo(DeclarationLockConfig config, ZString? tabPage = null)
		{
			var configTab = config.TabInfos.AddNew();
			using (configTab.GetValidationSuspender())
			{
				configTab.TabPage = tabPage ?? Core.Constants.Customs.DeclarationTabPages.Codes.All;
			}
		}

		const string DepartureDeclarationType = "DEP";
	}
}
