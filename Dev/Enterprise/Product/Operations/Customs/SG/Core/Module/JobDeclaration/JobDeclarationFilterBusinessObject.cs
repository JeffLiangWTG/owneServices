using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Module;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Module
{
	public class JobDeclarationFilterBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject, Integration.Customs.SG.IJobDeclarationFilterBusinessObject
	{
		#region SuppressResourceStringsCheckRegion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SG Customs Filter strip terminology")]
		public static class Descriptions
		{
			public const string OutwardHousebill = "Outward House Bill";
			public const string OutwardMasterbill = "Outward Master Bill";
			public const string InwardHouseBill = "Inward House Bill";
			public const string InwardMasterBill = "Inward Master Bill";
			public const string MessageURN = "Message URN#";
			public const string OutwardTransportMode = "Outward Transport Mode";
			public const string PackingType = "Packing Type";
			public const string DeclarationBranch = "Declaration Branch";
			public const string OutwardFlightVoyageVessel = "Outward Flight/Voyage # and Vessel";
			public const string OutwardShippingLineForwarder = "Outward Shipping Line/Forwarder";
			public const string InwardTransportMode = "Inward Transport Mode";
		}

		protected override void AddModeFilters(ModuleFilterCollection filters)
		{
			var containerModeFilter41 = filters.AddTextFilter(Descriptions.PackingType, JobDeclarationSchema.JE_ContainerMode, Lookups.ContainerModeList_TN41);
			containerModeFilter41.MultilingualDescription = ResString.GetMultilingualString("7248B6E1-549E-48E5-871C-B19E465EC327", Descriptions.PackingType);
			containerModeFilter41.Category = FilterCategories.ModesAndTypes;

			var inwardFlightVoyageVesselFilter = filters.AddTextAndNkFilter("Inward Flight/Voyage # and Vessel", FilterGenerator.GetDeclarationFlightVoyageAndVesselQuery, ModuleIDs.RefVessel, Lookups.VesselList);
			inwardFlightVoyageVesselFilter.MultilingualDescription = ResString.GetMultilingualString("3D68998F-8BEA-4FE2-A786-14302C01DA54", "Inward Flight/Voyage # and Vessel");
			inwardFlightVoyageVesselFilter.Category = FilterCategories.ModesAndTypes;

			var outwardFlightVoyageVesselFilter = filters.AddTextAndNkFilter(Descriptions.OutwardFlightVoyageVessel, GetOuterwardFlightVoyageVesselQuery, ModuleIDs.RefVessel, Lookups.VesselList)
				.WithMaxLengthOf<ModuleTextAndNkFilter>(GenAddOnColumnSchema.XA_Data);
			outwardFlightVoyageVesselFilter.MultilingualDescription = ResString.GetMultilingualString("9B8A6653-0957-40B6-B1DC-37FA65BD65C8", Descriptions.OutwardFlightVoyageVessel);
			outwardFlightVoyageVesselFilter.Category = FilterCategories.ModesAndTypes;

			var shipTypeFilter = filters.AddTextFilter("Type", JobDeclarationSchema.JE_MessageType, Lookups.MessageTypeList);
			shipTypeFilter.MultilingualDescription = ResString.GetMultilingualString("D6328862-8A1D-4699-AA86-6115FA2606C0", "Type");
			shipTypeFilter.Category = FilterCategories.ModesAndTypes;

			var shipSubTypeFilter = filters.AddTextFilter("Dec. Type", JobDeclarationSchema.JE_MessageSubType, Lookups.MessageSubTypeList);
			shipSubTypeFilter.MultilingualDescription = ResString.GetMultilingualString("F213CD0B-D23F-4208-9EE2-B1330EC5B118", "Dec. Type");
			shipSubTypeFilter.Category = FilterCategories.ModesAndTypes;

			var inwardTransportModeFilter = filters.AddTextFilter(Descriptions.InwardTransportMode, JobDeclarationSchema.JE_TransportMode, Lookups.TransportTypeList);
			inwardTransportModeFilter.MultilingualDescription = ResString.GetMultilingualString("33871601-B247-4C57-A1B7-D01312D2F31E", Descriptions.InwardTransportMode);
			inwardTransportModeFilter.Category = FilterCategories.ModesAndTypes;

			AddServiceLevelFilter(filters);
			AddServiceTypeFilter(filters);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
		protected override void AddMessageStatusFilter(ModuleFilterCollection filters)
		{
			return;
		}

		protected override void AddNumberFilters(ModuleFilterCollection filters)
		{
			base.AddNumberFilters(filters);

			var houseBillFilter = filters.AddNumberFilter(Descriptions.InwardHouseBill, CusDecHouseBillSchema.CU_BillNum);
			houseBillFilter.MultilingualDescription = ResString.GetMultilingualString("E69667C7-7AF4-4B3F-B2BB-F932DFAE08EC", Descriptions.InwardHouseBill);
			houseBillFilter.IsCommon = true;
			houseBillFilter.SubGroup = new HouseBillSubGroup();

			var inwardMAWBFilter = filters.AddNumberFilter(Descriptions.InwardMasterBill, CusDecHouseBillSchema.CU_BillNum);
			inwardMAWBFilter.MultilingualDescription = ResString.GetMultilingualString("1F7A081E-1ECA-46C0-AAAD-1C8EE8E91DCB", Descriptions.InwardMasterBill);
			inwardMAWBFilter.SubGroup = new MasterBillSubGroup();

			var urnFilter = filters.AddNumberFilter(Descriptions.MessageURN, GetMessageURNFilter).WithMaxLengthOf<ModuleNumberFilter>(EDIMessageSchema.EM_ApplicationReference);
			urnFilter.MultilingualDescription = ResString.GetMultilingualString("C34D375D-F62F-4C2E-B63A-7167C6F5427B", Descriptions.InwardMasterBill);

			var outwardHAWBFilter = filters.AddTextFilter(Descriptions.OutwardHousebill, GetOutwardHAWBQuery).WithMaxLengthOf<ModuleTextFilter>(SGAddInfoSchema.SG_OutwardHAWB);
			outwardHAWBFilter.MultilingualDescription = ResString.GetMultilingualString("510C1616-5F4C-4433-8309-74DA74072E5D", Descriptions.OutwardHousebill);
			outwardHAWBFilter.Category = FilterCategories.NumbersAndReferences;

			var outwardMAWBFilter = filters.AddTextFilter(Descriptions.OutwardMasterbill, GetOutwardMAWBQuery).WithMaxLengthOf<ModuleTextFilter>(SGAddInfoSchema.SG_OutwardMAWB);
			outwardMAWBFilter.MultilingualDescription = ResString.GetMultilingualString("151264F6-3D1D-42C9-8E46-90E6336D6957", Descriptions.OutwardMasterbill);
			outwardMAWBFilter.Category = FilterCategories.NumbersAndReferences;

			var outwardTransportModeFilter = filters.AddTextFilter(Descriptions.OutwardTransportMode, GetOutwardTransportMode, Lookups.TransportTypeList).WithMaxLengthOf<ModuleTextFilter>(SGAddInfoSchema.SG_OutwardTransportMode);
			outwardTransportModeFilter.MultilingualDescription = ResString.GetMultilingualString("B9E8275C-346D-421A-9ED8-26F091C70D15", Descriptions.OutwardTransportMode);
			outwardTransportModeFilter.Category = FilterCategories.NumbersAndReferences;
		}

		protected override void AddServiceLevelFilter(ModuleFilterCollection filters)
		{
			var serviceLevelModeFilter = filters.AddNkFilter(DeclarationFilterConstants.ServiceLevel, GetServiceLevelQuery, ModuleIDs.ServiceLevel, Lookups.ServiceLevelList);
			serviceLevelModeFilter.Category = FilterCategories.ModesAndTypes;
			serviceLevelModeFilter.MultilingualDescription = ResString.GetMultilingualString("1453CF11-1C7E-43DE-BDA3-CF691C48B081", DeclarationFilterConstants.ServiceLevel);
		}

		protected ZQuery GetServiceLevelQuery(ZDBOnlySubQuery filtersMatchQuery, SQLComparisonOperator sqlOperator, ZString serviceLevel)
		{
			var jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			var jobShipmentSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			if (filtersMatchQuery != null)
			{
				jobDeclarationQuery.AddSubQuery(JobDeclarationSchema.JE_RS_NKServiceLevel, RefServiceLevelSchema.RS_Code, filtersMatchQuery, JoinCondition.And);
				jobShipmentSubQuery.AddSubQuery(JobShipmentSchema.JS_RS_NKServiceLevel, RefServiceLevelSchema.RS_Code, filtersMatchQuery, JoinCondition.And);
			}
			else
			{
				jobDeclarationQuery.AddToFilter(JobDeclarationSchema.JE_RS_NKServiceLevel, sqlOperator, serviceLevel);
				jobShipmentSubQuery.AddToFilter(JobShipmentSchema.JS_RS_NKServiceLevel, sqlOperator, serviceLevel);
			}

			if (sqlOperator == SQLComparisonOperator.NotEqual || sqlOperator == SQLComparisonOperator.IsBlank)
			{
				var jobDeclarationSubQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				jobDeclarationSubQuery.AddSubQuery(JobDeclarationSchema.JE_JS, jobShipmentSubQuery, JoinCondition.And);
				jobDeclarationSubQuery.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_JS, null);
				jobDeclarationQuery.AddToFilter(jobDeclarationSubQuery, JoinCondition.And);
			}
			else
			{
				jobDeclarationQuery.AddSubQuery(JobDeclarationSchema.JE_JS, jobShipmentSubQuery, JoinCondition.Or);
			}

			return jobDeclarationQuery;
		}

		protected override bool IsCommonForEntryNumberFilter => true;

		protected override void AddMasterBillAndHouseBillFilters(ModuleFilterCollection filters)
		{
		}

		ZQuery GetOutwardHAWBQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GenAddOnColumnHelper.SimpleQueryHelper.GetQueryHandlingBlanks(SGAddInfoSchema.Constants.SG_OutwardHAWB, comparisonOperator, value);
		}

		ZQuery GetOutwardMAWBQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GenAddOnColumnHelper.SimpleQueryHelper.GetQueryHandlingBlanks(SGAddInfoSchema.Constants.SG_OutwardMAWB, comparisonOperator, value);
		}

		ZQuery GetOutwardTransportMode(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GenAddOnColumnHelper.SimpleQueryHelper.GetQueryHandlingBlanks(SGAddInfoSchema.Constants.SG_OutwardTransportMode, comparisonOperator, value);
		}

		protected ZQuery GetOuterwardFlightVoyageVesselQuery(SQLComparisonOperator flightOrVoyageNoComparisonOperator, ZString flightOrVoyageNo, ZString nKVessel)
		{
			var query = new ZQuery();

			if (!flightOrVoyageNo.IsEmpty || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsBlank || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				query.AddToFilter(GenAddOnColumnHelper.SimpleQueryHelper.GetQueryHandlingBlanks(SGAddInfoSchema.Constants.SG_OutwardVoyageFlightNo, flightOrVoyageNoComparisonOperator, flightOrVoyageNo));
			}

			if (!nKVessel.IsEmpty || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsBlank || flightOrVoyageNoComparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				query.AddToFilter(GenAddOnColumnHelper.SimpleQueryHelper.GetQueryHandlingBlanks(SGAddInfoSchema.Constants.SG_OutwardVesselName, flightOrVoyageNoComparisonOperator, nKVessel));
			}

			return query;
		}

		ZQuery GetOutwardShippingLineForwarderQuery(ZGuid orgPK)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			var docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

			orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, orgPK);
			docAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, DocAddressType.OutwardCarrierAgent));
			docAddressSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

			result.AddSubQuery(docAddressSubQuery, JoinCondition.And);

			return result;
		}

		protected ZDBOnlyQuery GetMessageURNFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobDeclaration));
			ZDBOnlySubQuery cusEntryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
			ZDBOnlySubQuery ediMessageQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_LinkUniqueID);
			ediMessageQuery.AddToFilter_PossiblyCommaSeparated(EDIMessageSchema.EM_ApplicationReference, comparisonOperator, value);
			cusEntryHeaderQuery.AddSubQuery(ediMessageQuery, JoinCondition.And);
			result.AddSubQuery(cusEntryHeaderQuery, JoinCondition.And);
			return result;
		}

		protected override void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			ModuleFilter branchFilter = filters.AddGuidFilter(Descriptions.DeclarationBranch, ModuleIDs.GlbBranch, JobDeclarationSchema.JE_GB, Lookups.BranchList);
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("3E51DDE6-015D-491D-88FE-49EFD5CC2AD5", Descriptions.DeclarationBranch);
			branchFilter.Category = FilterCategories.Organisations;

			ModuleFilter brokerFilter = filters.AddNkFilter("Customs Broker", JobDeclarationSchema.JE_GS_NKCusAgent, ModuleIDs.GlbStaff, Lookups.StaffList);
			brokerFilter.MultilingualDescription = ResString.GetMultilingualString("4BA209DE-EFD6-42F5-AEE3-B77ED219FD2C", "Customs Broker");
			brokerFilter.Category = FilterCategories.Organisations;

			ModuleGuidsFilter importerSupplierFilter = filters.AddGuidFilter(Customs.Module.DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier, ModuleIDs.Organisation, GetImporterSupplierQuery, Lookups.Consignees, Lookups.Consignors);
			importerSupplierFilter.SetItemDescriptions(new ResourceStringData("", Customs.Module.DeclarationFilterConstants.OrgFilterTypes.Importer), new ResourceStringData("", Customs.Module.DeclarationFilterConstants.OrgFilterTypes.Supplier));
			importerSupplierFilter.MultilingualDescription = ResString.GetMultilingualString("4B0536F0-FE54-42DC-BADD-D4E1D953FCE2", "Importer/Supplier");

			ModuleGuidsFilter shipplineLineForwarderFilter = filters.AddGuidFilter("Inward Shipping Line/Forwarder", ModuleIDs.Organisation, JobDeclarationSchema.JE_OH_ShippingLine, Lookups.Forwarders, JobDeclarationSchema.JE_OH_Forwarder, Lookups.Forwarders);
			shipplineLineForwarderFilter.SetItemDescriptions(new ResourceStringData("", Customs.Module.DeclarationFilterConstants.OrgFilterTypes.ShippingLine), new ResourceStringData("", Customs.Module.DeclarationFilterConstants.OrgFilterTypes.Forwarder));
			shipplineLineForwarderFilter.MultilingualDescription = ResString.GetMultilingualString("4FD9C641-833B-4302-B0FD-B45E6F9F6F0B", "Inward Shipping Line/Forwarder");

			ModuleFilter cartageCofilter = filters.AddGuidFilter(Customs.Module.DeclarationFilterConstants.OrgFilterTypes.CartageCoordinator, ModuleIDs.GlbStaff, GetCartageCoordinatorQuery, Lookups.StaffList);
			cartageCofilter.MultilingualDescription = ResString.GetMultilingualString("0F4622A9-5E21-4713-8D4B-9B53DF5176FB", DeclarationFilterConstants.OrgFilterTypes.CartageCoordinator);
			cartageCofilter.Category = FilterCategories.Organisations;

			var outwardShipplineLineForwarderFilter = filters.AddGuidFilter(Descriptions.OutwardShippingLineForwarder, ModuleIDs.Organisation, GetOutwardShippingLineForwarderQuery, Lookups.Organisations);
			outwardShipplineLineForwarderFilter.MultilingualDescription = ResString.GetMultilingualString("684E2BD0-C977-4159-9776-3C7B00B0C79A", Descriptions.OutwardShippingLineForwarder);
			outwardShipplineLineForwarderFilter.Category = FilterCategories.Organisations;

			AddBillingOrganisationFilter(filters);

			AddControllingAgentFilter(filters);
			AddControllingCustomerFilter(filters);
			AddExternalBrokerFilter(filters);
		}

		protected override void AddEntryStatusFilter(ModuleFilterCollection filters)
		{
			base.AddEntryStatusFilter(filters);
			if (SGCustomsDataRegistry.Instance.ACCESSEnable.Value)
			{
				var relatedManifestFilterFilter = filters.AddTextFilter("Related Manifest Bill Customs Status", GetLinkedGlobalManifestBill, Lookups.GlobalManifestStatusList);
				relatedManifestFilterFilter.MultilingualDescription = ResString.GetMultilingualString("EA33CFF6-D74C-4D86-8EEA-2A9D4593C521", "Related Manifest Bill Customs Status");
				relatedManifestFilterFilter.Category = FilterCategories.StatusAndFlags;
			}
		}

		protected ZQuery GetLinkedGlobalManifestBill(ZString value)
		{
			value = (value == Common.SG.GlobalManifestStatusList.Codes.NoStatus) ? ZString.Empty : value;

			var sqlParameters = new ZSqlParameterCollection();
			sqlParameters.Add("@BillStatusCondition", value, AsycudaBillSchema.ABL_BillStatus);
			sqlParameters.Add("@BolType", JobDeclaration.ChildBolCode, AsycudaBillSchema.ABL_BolType);
			sqlParameters.Add("@ImportShipmentType", ShipmentTypeList.Codes.Import23, AsycudaBillSchema.ABL_ShipmentType);
			sqlParameters.Add("@ExportShipmentType", ShipmentTypeList.Codes.Export22, AsycudaBillSchema.ABL_ShipmentType);
			sqlParameters.Add("@Country", Core.Constants.CountryCodes.Singapore, AsycudaManifestHeaderSchema.AMA_RN_NKCountry);

			var queryString =
			@"
			(
				JE_PK IN
				(
					SELECT JE_PK FROM dbo.JobDeclaration
					INNER JOIN dbo.AsycudaBill AS mbl ON mbl.ABL_BillNumber = JE_MasterBill
						AND mbl.ABL_BolType = @BolType
					INNER JOIN dbo.AsycudaManifestHeader ON AMA_PK = mbl.ABL_AMA
						AND AMA_RN_NKCountry = @Country
					INNER JOIN dbo.AsycudaBill AS hbl ON hbl.ABL_AMA = AMA_PK
						AND hbl.ABL_ShipmentType = @ImportShipmentType
						AND hbl.ABL_BillNumber = JE_HouseBill
						AND hbl.ABL_BolType <> @BolType
						AND hbl.ABL_BillStatus = @BillStatusCondition
				)
				OR JE_PK IN
				(
					SELECT jemb.XA_ParentID FROM dbo.GenAddOnColumn AS jemb
					INNER JOIN dbo.GenAddOnColumn AS jehb ON jemb.XA_ParentID=jehb.XA_ParentID AND jemb.XA_Name='SG_OutwardMAWB' AND jehb.XA_Name='SG_OutwardHAWB'
					INNER JOIN dbo.AsycudaBill AS mbl ON jemb.XA_Name='SG_OutwardMAWB' AND mbl.ABL_BillNumber = jemb.XA_Data AND mbl.ABL_BolType = 'BOL'
					INNER JOIN dbo.AsycudaManifestHeader ON AMA_PK = mbl.ABL_AMA AND AMA_RN_NKCountry = 'SG'
					INNER JOIN dbo.AsycudaBill AS hbl ON hbl.ABL_AMA = AMA_PK
						AND hbl.ABL_ShipmentType = @ExportShipmentType
						AND hbl.ABL_BillNumber = jehb.XA_Data
						AND hbl.ABL_BolType <> @BolType
						AND hbl.ABL_BillStatus = @BillStatusCondition
				)
			)";

			var mawbRecyclePeriod = FreightDataRegistry.Instance.MAWBRecyclePeriod.Value;
			if (mawbRecyclePeriod > 0)
			{
				sqlParameters.Add("@SystemCreateTimeUtc", ZDateTime.UtcNow.AddMonths(-mawbRecyclePeriod), JobDeclarationSchema.JE_SystemCreateTimeUtc);
				queryString += @" AND JE_SystemCreateTimeUtc >= @SystemCreateTimeUtc";
			}

			var result = new ZQuery();
			result.AddFilterAndZSQLParameterCollection(queryString, sqlParameters);
			return result;
		}

		#region Lookups

		public new JobDeclarationFilterLookups Lookups
		{
			get { return (JobDeclarationFilterLookups)base.Lookups; }
		}

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups()
		{
			return new JobDeclarationFilterLookups(this);
		}

		#endregion

		protected override ZString GetModuleFilterName(FilterBusinessObjectDefault filterDefault)
		{
			switch (filterDefault.FilterName)
			{
				case DeclarationDefaultFilterProvider.Constants.TransportMode:
					return Descriptions.InwardTransportMode;
				case DeclarationDefaultFilterProvider.Constants.ContainerMode:
					return Descriptions.PackingType;
				default:
					return base.GetModuleFilterName(filterDefault);
			}
		}

		protected GenAddOnColumnHelper GenAddOnColumnHelper => genAddOnColumnHelper ?? (genAddOnColumnHelper = new GenAddOnColumnHelper());
		GenAddOnColumnHelper genAddOnColumnHelper;

		public override MultilingualString ApplicationCodeFilterCaption => ResString.GetMultilingualString("SGCustoms|DeclarationFilter|SubmitType", "Message Type");

		#endregion
	}
}
