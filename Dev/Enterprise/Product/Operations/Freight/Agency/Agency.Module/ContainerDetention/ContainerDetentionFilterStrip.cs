using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Module
{
	public class ContainerDetentionFilterStrip : FilterStripBusinessObject, IAccountingFilterStripHolder
	{
		#region Descriptions

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filter related")]
		public static class Descriptions
		{
			// Numbers & Refs
			public const string BillNumber = "Bill Of Lading";
			public const string ContainerNumber = "Container #";
			public const string JobNumber = "Job #";
			public const string ShipmentNumber = "Shipment #";

			// Text
			public const string VoyageVessel = "Voyage / Vessel";

			// Organisations / Staff
			public const string Client = "Client";
			public const string Principal = "Principal";
			public const string OwningCompany = "Owning Company";

			// Locations
			public const string LoadDischarge = "Load / Discharge";
			public const string OriginDestination = "Origin / Destination";

			// Status and Flags
			public const string DetentionStatus = "Detention Status";
			public const string ContainersAttached = "Containers Attached";

			// Modes And Types
			public const string DetentionType = "Detention Type";
		}

		#endregion

		#region SubGroups

		ModuleFilterSubGroup MovementFilterProcessor => movementFilterProcessor ?? (movementFilterProcessor = new MovementFilterSubGroup());
		MovementFilterSubGroup movementFilterProcessor;

		ModuleFilterSubGroup ShipmentFilterProcessor => shipmentFilterProcessor ?? (shipmentFilterProcessor = new ShipmentFilterSubGroup(MovementFilterProcessor));
		ShipmentFilterSubGroup shipmentFilterProcessor;

		ModuleFilterSubGroup StockFilterProcessor => stockFilterProcessor ?? (stockFilterProcessor = new StockFilterSubGroup(MovementFilterProcessor));
		StockFilterSubGroup stockFilterProcessor;

		ModuleFilterSubGroup VoyageFilterProcessor => voyageFilterProcessor ?? (voyageFilterProcessor = new VoyageFilterSubGroup(MovementFilterProcessor));
		VoyageFilterSubGroup voyageFilterProcessor;

		class MovementFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var movement = new ZDBOnlySubQuery(typeof(ContainerMovement), JobContainerMoveSchema.E9_NC);
				movement.AddToFilter(filter);

				var detention = new ZDBOnlyQuery(typeof(ContainerDetention));
				detention.AddSubQuery(JobContainerDetentionSchema.PK, movement, JoinCondition.And);

				return detention;
			}
		}

		class ShipmentFilterSubGroup : ModuleFilterSubGroup
		{
			public ShipmentFilterSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var stock = new ZDBOnlySubQuery(typeof(RefContainerStock), RefContainerStockSchema.R6_ContainerNum);
				stock.AddToFilter(RefContainerStockSchema.PK, SQLComparisonOperator.Equal, JobContainerMoveSchema.E9_R6);

				var container = new ZDBOnlySubQuery(typeof(AgencyShipmentContainer), JobContainerSchema.JC_JS_FCLBookingOnlyLink);
				container.AddToFilter(JobContainerSchema.JC_Purpose, ContainerBookedStatus.Codes.Real);
				container.AddSubQuery(JobContainerSchema.JC_ContainerNum, stock, JoinCondition.And);

				var shipment = new ZDBOnlySubQuery(typeof(AgencyShipment), JobShipmentSchema.JS_JX);
				shipment.AddToFilter(JobShipmentSchema.JS_IsShipping, true);
				shipment.AddToFilter(filter);
				shipment.AddSubQuery(container, JoinCondition.And);

				var sailing = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.JX_JA);
				sailing.AddSubQuery(shipment, JoinCondition.And);

				var origin = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobVoyOriginSchema.JA_JV);
				origin.AddSubQuery(sailing, JoinCondition.And);

				var movement = new ZDBOnlyQuery(typeof(ContainerMovement));
				movement.AddSubQuery(JobContainerMoveSchema.E9_JV, origin, JoinCondition.And);

				return movement;
			}
		}

		class StockFilterSubGroup : ModuleFilterSubGroup
		{
			public StockFilterSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var stock = new ZDBOnlySubQuery(typeof(RefContainerStock), JobContainerMoveSchema.E9_R6);
				stock.AddToFilter(filter);

				var movement = new ZDBOnlyQuery(typeof(ContainerMovement));
				movement.AddSubQuery(stock, JoinCondition.And);

				return movement;
			}
		}

		class VoyageFilterSubGroup : ModuleFilterSubGroup
		{
			public VoyageFilterSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var voyage = new ZDBOnlySubQuery(typeof(JobVoyage), JobContainerMoveSchema.E9_JV);
				voyage.AddToFilter(filter);

				var movement = new ZDBOnlyQuery(typeof(ContainerMovement));
				movement.AddSubQuery(voyage, JoinCondition.And);

				return movement;
			}
		}

		#endregion

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			ModuleFilter result = new ModuleFountainFilter(Descriptions.JobNumber, JobContainerDetentionSchema.NC_JobNumber, "DI");
			result.MultilingualDescription = ResString.GetMultilingualString("Shipping|ContainerDetentionFilter|JobNumber", "Job #");

			return result;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection collection = new ModuleFilterCollection();

			AddNumbersAndRefsFilters(collection);
			AddVoyageVesselFilters(collection);
			AddOrganisationsAndStaffFilters(collection);
			AddLocationsFilters(collection);
			AddStatusAndFlagsFilters(collection);
			AddModesAndTypesFilters(collection);

			AccountingFilterStrip.AddBillingFilters(collection);
			AccountingFilterStrip.AddJobManagementFilters(collection, Env.Security.AgencyContainerDetentionJobInvoicing);

			return collection;
		}

		public override ZQuery Filter
		{
			get
			{
				if (Env.Security.AgencyContainerDetentionViewInterCompany.IsAllowed)
				{
					return base.Filter;
				}
				else
				{
					ZQuery result = new ZQuery();
					result.AddToFilter(JobContainerDetentionSchema.NC_GC, GlbCompany.CurrentCompany.PK);
					result.AddToFilter(base.Filter);

					return result;
				}
			}
		}

		#region AddNumbersAndRefsFilters

		void AddNumbersAndRefsFilters(ModuleFilterCollection collection)
		{
			var billNumberFilter = collection.AddNumberFilter(Descriptions.BillNumber, JobShipmentSchema.JS_HouseBill);
			billNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|ContainerDetentionFilter|BillNumber", "Bill Of Lading");
			billNumberFilter.SubGroup = ShipmentFilterProcessor;

			var containerNumberFilter = collection.AddNumberFilter(Descriptions.ContainerNumber, RefContainerStockSchema.R6_ContainerNum);
			containerNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|ContainerDetentionFilter|ContainerNumber", "Container #");
			containerNumberFilter.SubGroup = StockFilterProcessor;

			var shipmentNumberFilter = collection.AddFountainFilter(Descriptions.ShipmentNumber, JobShipmentSchema.JS_UniqueConsignRef, "V");
			shipmentNumberFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|ContainerDetentionFilter|ShipmentNumber", "Shipment #");
			shipmentNumberFilter.SubGroup = ShipmentFilterProcessor;
		}

		#endregion

		#region AddVoyageVesselFilters

		void AddVoyageVesselFilters(ModuleFilterCollection collection)
		{
			var filter = new VoyageVesselModuleFilter(Descriptions.VoyageVessel, GetVoyageVesselFilter, new RefVesselCollection(Factory))
				.WithMaxLengthOf(JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
			filter.MultilingualDescription = ResString.GetMultilingualString("Shipping|ContainerDetentionFilter|VoyageVessel", "Voyage / Vessel");
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.SubGroup = VoyageFilterProcessor;
			collection.AddCustomFilter(filter);
		}

		ZQuery GetVoyageVesselFilter(SQLComparisonOperator opp, ZString voyage, ZString vessel, ZBool includeArchived)
		{
			return VoyageVesselModuleFilterHelper.GetBasicVoyageVesselQuery(opp, voyage, vessel, includeArchived, JobVoyageSchema.JV_VoyageFlight, JobVoyageSchema.JV_RV_NKVessel);
		}

		#endregion

		#region AddOrganisationsAndStaffFilters

		void AddOrganisationsAndStaffFilters(ModuleFilterCollection collection)
		{
			ModuleGuidFilter clientFilter = collection.AddGuidFilter(Descriptions.Client, ModuleIDs.Organisation, JobContainerDetentionSchema.NC_OH_Client, new OrganisationsFindBoxCollection(Factory));
			clientFilter.Category = FilterCategories.Organisations;
			clientFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|ContainerDetentionFilter|Client", "Client");

			ModuleGuidFilter principalFilter = collection.AddGuidFilter(Descriptions.Principal, ModuleIDs.Organisation, JobContainerDetentionSchema.NC_OH_Principal, new ShipsAgencyPrincipalCollectionWithSecurityCheck(Factory));
			principalFilter.Category = FilterCategories.Organisations;
			principalFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|ContainerDetentionFilter|Principal", "Principal");

			ModuleGuidFilter companyFilter = collection.AddGuidFilter(Descriptions.OwningCompany, ModuleIDs.GlbCompany, OwningCompanyFilter, new GlbCompanyCollection(Factory));
			companyFilter.Category = FilterCategories.Organisations;
			companyFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|ContainerDetentionFilter|OwningCompany", "Owning Company");
			companyFilter.Visibility = FilterVisibility.AlwaysApplied;
			companyFilter.DefaultProperty = GlbCompany.CurrentCompany.PK;

			if (!Env.Security.AgencyContainerDetentionViewInterCompany.IsAllowed)
			{
				companyFilter.PropertyValidation = delegate(ZPropertyInfo target)
				{
					if (!target.Value.Equals(GlbCompany.CurrentCompany.PK))
					{
						target.AddError(Res.GetString("b021449a-5d77-47da-b1c2-8ec7a509a7e5", "You are not authorized to view container detention jobs from other companies."));
					}
				};
			}
		}

		ZQuery OwningCompanyFilter(ZGuid companyPK)
		{
			return companyPK.IsEmpty ? new ZQuery() : new ZQuery(JobContainerDetentionSchema.NC_GC, companyPK);
		}

		#endregion

		#region AddLocationsFilters

		void AddLocationsFilters(ModuleFilterCollection collection)
		{
			LocationCollection locations = new LocationCollection(Factory);
			ModuleLocationFilter loadDischargeFilter = collection.AddLocationFilter(Descriptions.LoadDischarge, GetLoadDischargeFilter, locations, locations);
			loadDischargeFilter.SetItemDescriptions(Res.GetData("f0375956-34ff-426c-af2f-3c9bad4b25ea", "Load"), Res.GetData("d331eb53-e85e-4dc2-8597-a8cc816e3f73", "Discharge"));
			loadDischargeFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|ContainerDetentionFilter|LoadDischarge", "Load / Discharge");
			loadDischargeFilter.SubGroup = ShipmentFilterProcessor;

			ModuleLocationFilter originDestinationFilter = collection.AddLocationFilter(Descriptions.OriginDestination, GetOriginDestinationFilter, locations, locations);
			originDestinationFilter.SetItemDescriptions(Res.GetData("03ac1919-7d63-4bb2-951c-98aadb907981", "Origin"), Res.GetData("e446932e-862e-46f0-97e8-c061df287ed6", "Destination"));
			originDestinationFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|ContainerDetentionFilter|OriginDestination", "Origin / Destination");
			originDestinationFilter.SubGroup = ShipmentFilterProcessor;
		}

		ZQuery GetLoadDischargeFilter(ZString load, ZString discharge)
		{
			SailingFilterBuilder builder = new SailingFilterBuilder(Factory);
			builder.LoadPort = load;
			builder.DischargePort = discharge;
			return builder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.Direct);
		}

		ZQuery GetOriginDestinationFilter(ZString origin, ZString destination)
		{
			ZQuery shipmentFilter = new ZQuery();

			if (!origin.IsEmpty)
			{
				shipmentFilter.AddToFilter(JobShipmentSchema.JS_RL_NKOrigin, origin);
			}

			if (!destination.IsEmpty)
			{
				shipmentFilter.AddToFilter(JobShipmentSchema.JS_RL_NKDestination, destination);
			}

			return shipmentFilter;
		}

		#endregion

		#region AddModesAndTypesFilters

		public void AddModesAndTypesFilters(ModuleFilterCollection collection)
		{
			collection.AddTextFilter(Descriptions.DetentionType, JobContainerDetentionSchema.NC_DetentionType, new DetentionInvoiceType()).MultilingualDescription = ResString.GetMultilingualString("Shipping|ContainerDetentionFilter|DetentionType", "Detention Type");
		}

		#endregion

		#region AddStatusAndFlagsFilters

		void AddStatusAndFlagsFilters(ModuleFilterCollection collection)
		{
			collection.AddTextFilter(Descriptions.DetentionStatus, GetDetentionStatusFilter, new DetentionInvoiceStatus()).MultilingualDescription = ResString.GetMultilingualString("Shipping|ContainerDetentionFilter|DetentionStatus", "Detention Status");

			ModuleFlagsFilter containersAttachedFilter = collection.AddFlagsFilter(
				Descriptions.ContainersAttached,
				new string[] { Res.GetString("335cd39d-c47a-47a4-a71e-f2cf6670ce86", "Containers Attached") },
				new GetFlagsQuery[] { ContainersAttached }
			);
			containersAttachedFilter.MultilingualDescription = ResString.GetMultilingualString("Shipping|ContainerDetentionFilter|ContainersAttached", "Containers Attached");
		}

		ZQuery GetDetentionStatusFilter(ZString status)
		{
			switch (status)
			{
				case DetentionInvoiceStatus.Codes.NotPosted:
					return FromJobHeaderFilter(GetHeaderChargesFilter(true, true), true);

				case DetentionInvoiceStatus.Codes.PartiallyPosted:
					{
						ZQuery result = new ZQuery();
						result.AddToFilter(GetHeaderChargesFilter(true, true));
						result.AddToFilter(GetHeaderChargesFilter(false, true));
						return FromJobHeaderFilter(result, false);
					}

				case DetentionInvoiceStatus.Codes.Posted:
					{
						ZQuery result = new ZQuery();
						result.AddToFilter(GetHeaderChargesFilter(true, true));
						result.AddToFilter(GetHeaderChargesFilter(false, false));
						return FromJobHeaderFilter(result, false);
					}

				default:
					return new ZQuery();
			}
		}
		ZQuery GetHeaderChargesFilter(bool posted, bool exists)
		{
			ZQuery result;

			string sqlFormat = exists ? " EXISTS " : (NoResString)"NOT EXISTS ";  // filter related
			sqlFormat += "(SELECT 1 " +
						 "FROM {0} " +
						 "WHERE ( ";                                 // filter related

			sqlFormat += posted ? "EXISTS " : (NoResString)"NOT EXISTS ";         // filter related
			sqlFormat += "(SELECT 1 " +
						 "FROM {1} " +
						 "WHERE {2} = '{3}' AND {4} = {5}) ";        // filter related

			sqlFormat += posted ? string.Empty : (NoResString)"OR {4} IS NULL ";  // filter related
			sqlFormat += ") AND {6} = {7} ) ";                       // filter related

			string sql = string.Format(CultureInfo.InvariantCulture, sqlFormat, HeaderChargesFilterQueryObject());

			result = new ZDBOnlyQuery(typeof(JobHeader));
			result.AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection(), JoinCondition.And);

			return result;
		}

		static object[] HeaderChargesFilterQueryObject()
		{
			return new object[]
			{
				JobChargeSchema.Constants.TableName,
				AccTransactionLinesSchema.Constants.TableName,
				AccTransactionLinesSchema.Constants.AL_LineType,
				TransactionLineTypes.Revenue,
				JobChargeSchema.Constants.JR_AL_ARLine,
				AccTransactionLinesSchema.Constants.PK,
				JobHeaderSchema.Constants.PK,
				JobChargeSchema.Constants.JR_JH,
			};
		}

		ZQuery ContainersAttached(ZBool value)
		{
			ZQuery result;

			string sqlFormat = (value ? (NoResString)" exists " : (NoResString)" not exists ") + // SQL keywords
				"( " +
					"select {0} " +
					"from {1}  " +
					"where {0} = {2} " +
				") ";

			string sql = string.Format(CultureInfo.InvariantCulture, sqlFormat,
				JobContainerMoveSchema.Constants.E9_NC,     // 0
				JobContainerMoveSchema.Constants.TableName, // 1
				JobContainerDetentionSchema.Constants.PK    // 2
			);

			result = new ZDBOnlyQuery(typeof(ContainerDetention));
			result.AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection());

			return result;
		}

		#endregion

		#region IAccountingFilterStripHolder Members

		internal IAccountingFilterStrip AccountingFilterStrip
		{
			get
			{
				if (AccountingFilterStrip_innerValue == null)
				{
					AccountingFilterStrip_innerValue = (IAccountingFilterStrip)Activator.CreateInstance(ObjectFactory.GetType<IAccountingFilterStrip>(), this);
					AccountingFilterStrip_innerValue.Initialize(addProfitLossReasonFilters: true);
				}

				return AccountingFilterStrip_innerValue;
			}
		}
		IAccountingFilterStrip AccountingFilterStrip_innerValue;

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => new Dictionary<string, object>()
		{
			{ AccountingFilterStripConfigurationKeys.BusinessObjectType, typeof(ContainerDetention) },
			{ AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride, ResString.GetMultilingualString("Shipping|ContainerDetentionFilter|InvoiceStatus", "Invoice Status") }
		};

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ContainerDetention));
			result.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return result;
		}

		MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride
		{
			get { return null; }
		}

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride
		{
			get { return null; }
		}

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories
		{
			get { return null; }
		}

		#endregion
		#region Implementation

		#region From*Filter

		ZQuery FromJobHeaderFilter(ZQuery jobHeaderFilter, bool notIn)
		{
			ZDBOnlySubQuery header = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID, notIn);
			header.AddToFilter(jobHeaderFilter);
			header.AddToFilter(JobHeaderSchema.JH_ParentTableCode, JobContainerDetentionSchema.Constants.Prefix);

			ZDBOnlyQuery detention = new ZDBOnlyQuery(typeof(ContainerDetention));
			detention.AddSubQuery(JobContainerDetentionSchema.PK, header, JoinCondition.And);

			return detention;
		}

		#endregion

		#region CodeLists

		public class FinalProformaStatusList : ReadOnlyCodeDescriptionPairList
		{
			public const string Final = "FIN";
			public const string Proforma = "PRO";

			public FinalProformaStatusList()
			{
				Elements.Add(new CodeDescriptionPair(Final, Res.GetString("84e55256-ab62-425b-ba4f-049936827fef", "Final")));
				Elements.Add(new CodeDescriptionPair(Proforma, Res.GetString("c74ed8b5-bbb9-43a2-87d3-048740164b01", "Proforma")));
			}
		}

		#endregion

		#endregion
	}
}



