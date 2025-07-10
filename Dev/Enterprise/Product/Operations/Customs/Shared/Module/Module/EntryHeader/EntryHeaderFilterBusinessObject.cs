using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class EntryHeaderFilterBusinessObject : FilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter types")]
		public static class Constants
		{
			public const string EntryNumber = ModuleEntryHeaderCollection.FilterConstants.EntryNumber;
			public const string JobNumber = ModuleEntryHeaderCollection.FilterConstants.JobNumber;
			public const string ReferenceNumber = ModuleEntryHeaderCollection.FilterConstants.ReferenceNumber;
			public const string SubmissionDate = ModuleEntryHeaderCollection.FilterConstants.SubmissionDate;
			public const string ReleaseDate = ModuleEntryHeaderCollection.FilterConstants.ReleaseDate;
			public const string EntryStatus = ModuleEntryHeaderCollection.FilterConstants.EntryStatus;
			public const string MessageStatus = ModuleEntryHeaderCollection.FilterConstants.MessageStatus;
			public const string WarehouseTransactionStatus = ModuleEntryHeaderCollection.FilterConstants.WarehouseTransactionStatus;
			public const string MessageType = ModuleEntryHeaderCollection.FilterConstants.MessageType;

			public const string OwnerReference = "Owner Reference";
			public const string OriginETD = "Origin ETD";
			public const string FinalDestinationETA = "Final Destination ETA";
			public const string ArrivalDate = "Arrival Date";
			public const string ExportDate = "Export Date";
			public const string Declarant = "Declarant";
			public const string DeclarationType = "Declaration Type";
			public const string OriginDestination = "Origin/Destination";
			public const string LoadingDischarge = "Loading/Discharge";
			public const string Loading = "Loading";
			public const string Discharge = "Discharge";
			public const string CustomsAgent = "Customs Agent";
			public const string Branch = "Branch";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();

			var entryNumberFilter = result.AddNumberFilter(Constants.EntryNumber, GetEntryNumberQuery);
			entryNumberFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
			entryNumberFilter.MultilingualDescription = GetEntryNumberFilterMultilingualDescription();

			var jobNumberFilter = result.AddNumberFilter(Constants.JobNumber, JobDeclarationSchema.JE_DeclarationReference);
			jobNumberFilter.SubGroup = JobDeclarationSubGroup;
			jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|JobNumber", Constants.JobNumber);

			result.AddNumberFilter(Constants.ReferenceNumber, CusEntryHeaderSchema.CH_BGMReference).MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|ReferenceNumber", Constants.ReferenceNumber);
			result.AddDateFilter(Constants.SubmissionDate, CusEntryHeaderSchema.CH_EntrySubmittedDate).MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|SubmissionDate", Constants.SubmissionDate);
			result.AddDateFilter(Constants.ReleaseDate, CusEntryHeaderSchema.CH_EntryReleaseDate).MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|ReleaseDate", Constants.ReleaseDate);

			var entryStatusFilter = result.AddTextFilter(Constants.EntryStatus, CusEntryHeaderSchema.CH_EntryStatus, GetEntryStatusList);
			entryStatusFilter.Category = FilterCategories.StatusAndFlags;
			entryStatusFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|EntryStatus", Constants.EntryStatus);
			entryStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			entryStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			entryStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			entryStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);

			var messageStatusFilter = result.AddTextFilter(Constants.MessageStatus, GetMessageStatusQuery, Lookups.MessageStatusList);
			messageStatusFilter.Category = FilterCategories.StatusAndFlags;
			messageStatusFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|MessageStatus", Constants.MessageStatus);

			var messageTypeFilter = result.AddTextFilter(Constants.MessageType, GetMessageTypeQuery, Lookups.MessageTypeList);
			messageTypeFilter.Category = FilterCategories.StatusAndFlags;
			messageTypeFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|MessageType", Constants.MessageType);

			var warehouseTransactionStatus = result.AddTextFilter(Constants.WarehouseTransactionStatus, CusEntryHeaderSchema.CH_WarehouseTransactionStatus, Lookups.WarehouseTransactionStatusList);
			warehouseTransactionStatus.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|WarehouseTransactionStatus", Constants.WarehouseTransactionStatus);
			warehouseTransactionStatus.Category = FilterCategories.StatusAndFlags;

			var declarationTypeFilter = result.AddTextFilter(Constants.DeclarationType, CusEntryInstructionSchema.CEI_Style, Lookups.EntryInstructionStyleList);
			declarationTypeFilter.SubGroup = EntryInstructionSubGroup;
			declarationTypeFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|DeclarationType", Constants.DeclarationType);
			declarationTypeFilter.Category = FilterCategories.ModesAndTypes;

			AddOrganisationFilters(result);
			AddJobDeclarationSubGroupFilters(result);

			return result;
		}

		protected void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var importerSupplierFilter = filters.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier, ModuleIDs.Organisation, GetImporterSupplierQuery, Lookups.Consignees, Lookups.Consignors);
			importerSupplierFilter.SubGroup = JobDeclarationSubGroup;
			importerSupplierFilter.SetItemDescriptions(Res.GetData("EntryHeaderFilter|Importer", "Importer"), Res.GetData("EntryHeaderFilter|Supplier", "Supplier"));
			importerSupplierFilter.MultilingualDescription = GetImporterSupplierFilterMultilingualDescription();

			var controllingAgentFilter = filters.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.ControllingAgent, ModuleIDs.Organisation, JobDeclarationSchema.JE_OH_ControllingAgent, Lookups.ControllingAgents);
			controllingAgentFilter.SubGroup = JobDeclarationSubGroup;
			controllingAgentFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|ControllingAgent", DeclarationFilterConstants.OrgFilterTypes.ControllingAgent);
			controllingAgentFilter.Category = FilterCategories.Organisations;

			var controllingCustomerFilter = filters.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.ControllingCustomer, ModuleIDs.Organisation, JobDeclarationSchema.JE_OH_ControllingCustomer, Lookups.ControllingCustomers);
			controllingCustomerFilter.SubGroup = JobDeclarationSubGroup;
			controllingCustomerFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|ControllingCustomer", DeclarationFilterConstants.OrgFilterTypes.ControllingCustomer);
			controllingCustomerFilter.Category = FilterCategories.Organisations;

			var declarantFilter = filters.AddGuidFilter(Constants.Declarant, ModuleIDs.OrgAddresses, JobDeclarationSchema.JE_OA_DeclarantAddress, Lookups.DeclarantList);
			declarantFilter.SubGroup = JobDeclarationSubGroup;
			declarantFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|Declarant", Constants.Declarant);
			declarantFilter.Category = FilterCategories.Organisations;

			var brokerFilter = filters.AddNkFilter(Constants.CustomsAgent, JobDeclarationSchema.JE_GS_NKCusAgent, ModuleIDs.GlbStaff, Lookups.StaffList);
			brokerFilter.Category = FilterCategories.Organisations;
			brokerFilter.SubGroup = JobDeclarationSubGroup;
			brokerFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|CustomsAgent", Constants.CustomsAgent);

			var branchFilter = filters.AddGuidFilter(Constants.Branch, ModuleIDs.GlbBranch, JobDeclarationSchema.JE_GB, Lookups.BranchList);
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.SubGroup = JobDeclarationSubGroup;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|Branch", Constants.Branch);
		}

		ZQuery GetImporterSupplierQuery(ZGuid importer, ZGuid supplier)
		{
			var result = new ZQuery();

			if (!importer.IsEmpty)
			{
				result.AddToFilter(JobDeclarationSchema.JE_OH_Importer, importer);
			}

			if (!supplier.IsEmpty)
			{
				result.AddToFilter(JobDeclarationSchema.JE_OH_Supplier, supplier);
			}

			return result;
		}

		protected void AddJobDeclarationSubGroupFilters(ModuleFilterCollection filters)
		{
			var jobDeclarationFilterGenerator = new JobDeclarationFilterGenerator(this);
			var flightVoyageVesselFilter = jobDeclarationFilterGenerator.AddFlightVoyageVesselFilter(filters, DeclarationFilterConstants.FlightVoyageVessel);
			flightVoyageVesselFilter.SubGroup = JobDeclarationSubGroup;
			flightVoyageVesselFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|FlightVoyageVessel", DeclarationFilterConstants.FlightVoyageVessel);

			var originDestinationFilter = jobDeclarationFilterGenerator.AddOriginDestinationFilter(filters, Constants.OriginDestination);
			originDestinationFilter.SubGroup = JobDeclarationSubGroup;
			originDestinationFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|OriginDestination", Constants.OriginDestination);

			var loadDischargeFilter = jobDeclarationFilterGenerator.AddLoadDischargeFilter(filters, Constants.LoadingDischarge);
			loadDischargeFilter.SubGroup = JobDeclarationSubGroup;
			loadDischargeFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|LoadingDischarge", Constants.LoadingDischarge);
			loadDischargeFilter.SetItemDescriptions(Res.GetData("EntryHeaderFilter|Loading", Constants.Loading), Res.GetData("EntryHeaderFilter|Discharge", Constants.Discharge));

			var agentsReferenceFilter = jobDeclarationFilterGenerator.AddAgentsReferenceFilter(filters, DeclarationFilterConstants.NumberFilterTypes.AgentsReference);
			agentsReferenceFilter.SubGroup = JobDeclarationSubGroup;
			agentsReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|AgentsReference", DeclarationFilterConstants.NumberFilterTypes.AgentsReference);

			var ownersRefFilter = filters.AddNumberFilter(Constants.OwnerReference, JobDeclarationSchema.JE_OwnerRef);
			ownersRefFilter.SubGroup = JobDeclarationSubGroup;
			ownersRefFilter.Category = FilterCategories.NumbersAndReferences;
			ownersRefFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|OwnerReference", Constants.OwnerReference);

			var referenceNumberFilter = jobDeclarationFilterGenerator.AddAdditionalReferenceNumberFilter(filters, Factory);
			referenceNumberFilter.SubGroup = JobDeclarationSubGroup;

			var masterBillFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.MasterBill, JobDeclarationSchema.JE_MasterBill);
			masterBillFilter.SubGroup = JobDeclarationSubGroup;
			masterBillFilter.Category = FilterCategories.NumbersAndReferences;
			masterBillFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|MasterBill", DeclarationFilterConstants.NumberFilterTypes.MasterBill);

			var houseBillFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.HouseBill, JobDeclarationSchema.JE_HouseBill);
			houseBillFilter.SubGroup = JobDeclarationSubGroup;
			houseBillFilter.Category = FilterCategories.NumbersAndReferences;
			houseBillFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|HouseBill", DeclarationFilterConstants.NumberFilterTypes.HouseBill);

			var shipTypeFilter = filters.AddTextFilter(DeclarationFilterConstants.ShipmentType, GetShipmentTypeQuery, Lookups.ShipmentTypeList);
			shipTypeFilter.SubGroup = JobDeclarationSubGroup;
			shipTypeFilter.Category = FilterCategories.ModesAndTypes;
			shipTypeFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|ShipmentType", DeclarationFilterConstants.ShipmentType);

			var transportModeFilter = filters.AddTextFilter(DeclarationFilterConstants.TransportMode, GetTransportModeQuery, Lookups.TransportTypeList);
			transportModeFilter.SubGroup = JobDeclarationSubGroup;
			transportModeFilter.Category = FilterCategories.ModesAndTypes;
			transportModeFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|TransportMode", DeclarationFilterConstants.TransportMode);

			var originETDFilter = filters.AddDateFilter(Constants.OriginETD, JobDeclarationSchema.JE_DateAtOrigin);
			originETDFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|OriginETD", Constants.OriginETD);
			originETDFilter.SubGroup = JobDeclarationSubGroup;
			originETDFilter.Category = FilterCategories.Dates;

			var finalDestinationETAFilter = filters.AddDateFilter(Constants.FinalDestinationETA, JobDeclarationSchema.JE_DateAtFinalDestination);
			finalDestinationETAFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|FinalDestinationETA", Constants.FinalDestinationETA);
			finalDestinationETAFilter.SubGroup = JobDeclarationSubGroup;
			finalDestinationETAFilter.Category = FilterCategories.Dates;

			var arrivalDateFilter = filters.AddDateFilter(Constants.ArrivalDate, JobDeclarationSchema.JE_DateOfArrival);
			arrivalDateFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|ArrivalDate", Constants.ArrivalDate);
			arrivalDateFilter.SubGroup = JobDeclarationSubGroup;
			arrivalDateFilter.Category = FilterCategories.Dates;

			var exportDateFilter = filters.AddDateFilter(Constants.ExportDate, JobDeclarationSchema.JE_ExportDate);
			exportDateFilter.MultilingualDescription = ResString.GetMultilingualString("EntryHeaderFilter|ExportDate", Constants.ExportDate);
			exportDateFilter.SubGroup = JobDeclarationSubGroup;
			exportDateFilter.Category = FilterCategories.Dates;
		}

		ZQuery GetTransportModeQuery(ZString value) => new ZQuery(JobDeclarationSchema.JE_TransportMode, SQLComparisonOperator.Equal, value);

		ZQuery GetShipmentTypeQuery(ZString value) => new ZQuery(JobDeclarationSchema.JE_MessageType, SQLComparisonOperator.Equal, value);

		protected ZQuery GetMessageTypeQuery(ZString value) => new ZQuery(CusEntryHeaderSchema.CH_MessageType, value);

		public EntryHeaderFilterLookups Lookups => lookups ?? (lookups = GetNewLookups());
		EntryHeaderFilterLookups lookups;

		protected virtual EntryHeaderFilterLookups GetNewLookups() => new EntryHeaderFilterLookups(this);

		protected ModuleFilterSubGroup JobDeclarationSubGroup => jobDeclarationSubGroup ?? (jobDeclarationSubGroup = new BaseJobDeclarationSubGroup());
		ModuleFilterSubGroup jobDeclarationSubGroup;

		protected virtual CodeDescriptionPairList GetEntryStatusList() => Lookups.EntryStatusList(GlbCompany.CurrentCompany.Country.RN_Code);

		protected ZQuery GetEntryStatusQuery(ZString value)
		{
			return new ZQuery(CusEntryHeaderSchema.CH_EntryStatus, value);
		}

		protected virtual ZQuery GetMessageStatusQuery(ZString value)
		{
			if (value == DeclarationFilterConstants.EntryStatus.NotSentForFilter)
			{
				return new ZQuery(CusEntryHeaderSchema.CH_Status, new ZString[] { ZString.Empty, value });
			}
			else
			{
				return new ZQuery(CusEntryHeaderSchema.CH_Status, value);
			}
		}

		protected virtual ZQuery GetEntryNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var entryNumberQuery = GetEntryNumberQueryOfParentTableAndEntryTypeForEntryNum(comparisonOperator, value, CusEntryHeaderSchema.Constants.TableName, CusEntryNumberTypes.Standard.MovementReferenceNumber, ZString.Empty, false);

			var result = new ZDBOnlyQuery(typeof(CusEntryHeader));
			result.AddSubQuery(entryNumberQuery, JoinCondition.And);

			return result;
		}

		protected virtual ZQuery GetEntryTypeFilter(string entryType, bool useOriginalEntryType = true)
		{
			return new ZQuery(CusEntryNumSchema.CE_EntryType, entryType);
		}

		protected ZDBOnlySubQuery GetEntryNumberQueryOfParentTableAndEntryType(SQLComparisonOperator comparisonOperator, SchemaStringColumn column, ZString value, string parentTable, string entryType, ZString countryCode, bool useOriginalEntryType = true)
		{
			var isNegative = comparisonOperator.IsNegativeSQLOperator();
			if (comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				isNegative = comparisonOperator == SpecialComparisonOperator.IsBlank;
			}

			var entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, isNegative);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, parentTable);
			entryNumberQuery.AddToFilter(GetEntryTypeFilter(entryType, useOriginalEntryType));

			if (!countryCode.IsEmpty)
			{
				entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, countryCode);
			}

			var resultComparisonOperator = comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery();
			if (resultComparisonOperator == SpecialComparisonOperator.IsBlank || resultComparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				entryNumberQuery.AddToFilter(column, SQLComparisonOperator.NotEqual, string.Empty);
			}
			else
			{
				entryNumberQuery.AddToFilter_PossiblyCommaSeparated(column, resultComparisonOperator, value);
			}
			return entryNumberQuery;
		}

		protected ZDBOnlySubQuery GetEntryNumberQueryOfParentTableAndEntryTypeForEntryNum(SQLComparisonOperator comparisonOperator, ZString value, string parentTable, string entryType, ZString countryCode, bool useOriginalEntryType = true)
		{
			return GetEntryNumberQueryOfParentTableAndEntryType(comparisonOperator, CusEntryNumSchema.CE_EntryNum, value, parentTable, entryType, countryCode, useOriginalEntryType);
		}

		protected ZDBOnlySubQuery GetEntryNumberQueryOfParentTableAndEntryTypeForEntryStatus(SQLComparisonOperator comparisonOperator, ZString value, string parentTable, string entryType, ZString countryCode, bool useOriginalEntryType = true)
		{
			return GetEntryNumberQueryOfParentTableAndEntryType(comparisonOperator, CusEntryNumSchema.CE_EntryStatus, value, parentTable, entryType, countryCode, useOriginalEntryType);
		}

		protected virtual ResourceString GetImporterSupplierFilterMultilingualDescription()
		{
			return ResString.GetMultilingualString("EntryHeaderFilter|ImporterSupplier", DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier);
		}

		protected virtual ResourceString GetEntryNumberFilterMultilingualDescription()
		{
			return ResString.GetMultilingualString("EntryHeaderFilter|EntryNumber", Constants.EntryNumber);
		}

		protected class EntryNumberSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(CusEntryHeader));

				var entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeaderSchema.Constants.TableName);
				entryNumberQuery.AddToFilter(filter);

				result.AddSubQuery(entryNumberQuery, JoinCondition.And);

				return result;
			}
		}

		class BaseJobDeclarationSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(CusEntryHeader));

				var declarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), CusEntryHeaderSchema.CH_JE);
				declarationQuery.AddToFilter(filter);
				result.AddSubQuery(declarationQuery, JoinCondition.And);

				return result;
			}
		}

		protected CusEntryInstructionSubGroup EntryInstructionSubGroup
		{
			get { return cusEntryInstructionSubGroup ?? (cusEntryInstructionSubGroup = new CusEntryInstructionSubGroup()); }
		}
		CusEntryInstructionSubGroup cusEntryInstructionSubGroup;

		protected class CusEntryInstructionSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(CusEntryHeader));

				var instructionQuery = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryHeaderSchema.CH_CEI_Instruction);
				instructionQuery.AddToFilter(filter);
				result.AddSubQuery(instructionQuery, JoinCondition.And);

				return result;
			}
		}
	}
}
