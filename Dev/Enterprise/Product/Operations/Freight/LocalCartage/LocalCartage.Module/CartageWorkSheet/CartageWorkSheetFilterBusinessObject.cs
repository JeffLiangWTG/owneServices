using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Module
{
	public class CartageWorkSheetFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			AddNumbersAndReferencesFilters(filters);
			AddDatesFilters(filters);
			AddOrganisationsAndStaffFilters(filters);

			return filters;
		}

		void AddNumbersAndReferencesFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddFountainFilter("Run Sheet #", JobCartageRunSheetSchema.EY_RunSheetNumber, "RS");
			filter.MultilingualDescription = ResString.GetMultilingualString("f926fa02-34b6-4af6-b995-108ba850ecee", "Run Sheet #");
			filter.Category = FilterCategories.NumbersAndReferences;

			filter = filters.AddNumberFilter(NumberFilterTypes.ContainerNumber, GetContainerNumberQuery);
			filter.MultilingualDescription = ResString.GetMultilingualString("46cf32ce-d9fd-4c47-99c5-0c83e589f5a9", "Container #");
			filter.MaxLength = JobContainerSchema.JC_ContainerNum.MaxLength;
			filter.IsCommon = true;
		}

		ZQuery GetContainerNumberQuery(SQLComparisonOperator @operator, ZString containerNo)
		{
			return GetContainerQuery(JobContainerSchema.JC_ContainerNum, @operator, containerNo);
		}

		void AddDatesFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddDateFilter("Start Time", JobCartageRunSheetSchema.EY_StartTime);
			filter.MultilingualDescription = ResString.GetMultilingualString("898fc184-efb8-46d3-bdb5-647385148262", "Start Time");
			filter.Category = FilterCategories.Dates;

			filter = filters.AddDateFilter("End Time", JobCartageRunSheetSchema.EY_EndTime);
			filter.MultilingualDescription = ResString.GetMultilingualString("0ab68d61-9f10-4e4a-bf63-e9dfb25282c6", "End Time");
			filter.Category = FilterCategories.Dates;
		}

		void AddOrganisationsAndStaffFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddNkFilter("Staff Driver", JobCartageRunSheetSchema.EY_GS_NKTruckDriver, ModuleIDs.GlbStaff, BindingLists.StaffDrivers);
			filter.MultilingualDescription = ResString.GetMultilingualString("024425d6-0dab-4b09-b354-052d174c4bef", "Staff Driver");
			filter.Category = FilterCategories.Organisations;

			filter = filters.AddGuidFilter("Transport Company", ModuleIDs.Organisation, JobCartageRunSheetSchema.EY_OH_TransportCo, BindingLists.TransportProviders);
			filter.MultilingualDescription = ResString.GetMultilingualString("feb39d2c-c77f-435a-a4fc-8dd2325e3082", "Transport Company");
			filter.Category = FilterCategories.Organisations;

			filter = filters.AddTextFilter("Drivers Name", JobCartageRunSheetSchema.EY_DriversName);
			filter.MultilingualDescription = ResString.GetMultilingualString("1ab47cd8-2d70-4f4f-8cb8-d97f1310043e", "Drivers Name");
			filter.Category = FilterCategories.Organisations;

			filter = filters.AddTextFilter("Transport Company Name", GetTransportCompanyNameQuery);
			filter.MultilingualDescription = ResString.GetMultilingualString("7dffb4c6-c2d7-4447-a19c-4bafb0eef628", "Transport Company Name");
			filter.MaxLength = Math.Min(JobCartageRunSheetSchema.EY_TransportCoName.MaxLength, OrgHeaderSchema.OH_FullName.MaxLength);
			filter.Category = FilterCategories.Organisations;

			filter = filters.AddTextFilter("Drivers License", JobCartageRunSheetSchema.EY_DriversLicence);
			filter.MultilingualDescription = ResString.GetMultilingualString("7dafd586-a963-439a-8a43-fcc1ae1445d9", "Drivers License");
			filter.Category = FilterCategories.Organisations;

			filter = filters.AddTextFilter("Vehicle Registration", JobCartageRunSheetSchema.EY_TruckRegistration);
			filter.MultilingualDescription = ResString.GetMultilingualString("d4795111-2547-4afd-a207-4036bfe9e213", "Vehicle Registration");
			filter.Category = FilterCategories.Organisations;

			filter = filters.AddGuidFilter("Vehicle", ModuleIDs.RefEquipment, JobCartageRunSheetSchema.EY_RQ_Truck, BindingLists.Vehicles);
			filter.MultilingualDescription = ResString.GetMultilingualString("583fb4d8-3fd9-43ff-b67d-5fd6a64ede0c", "Vehicle");
			filter.Category = FilterCategories.Organisations;
		}

		ZQuery GetTransportCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery(JobCartageRunSheetSchema.EY_TransportCoName, comparisonOperator, value);
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CommonWorkSheet));
			result.AddToFilter(query);

			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				result.AddToFilter(JobCartageRunSheetSchema.EY_OH_TransportCo, SQLComparisonOperator.Equal, null);
			}
			else if (comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				result.AddToFilter(JoinCondition.Or, JobCartageRunSheetSchema.EY_OH_TransportCo, SQLComparisonOperator.NotEqual, null);
			}
			else
			{
				ZDBOnlySubQuery orgHeaderQuery = new ZDBOnlySubQuery(typeof(OrgHeader), JobCartageRunSheetSchema.EY_OH_TransportCo);
				orgHeaderQuery.AddToFilter(new ZQuery(OrgHeaderSchema.OH_FullName, comparisonOperator, value));
				result.AddSubQuery(orgHeaderQuery, JoinCondition.Or);
			}

			return result;
		}

		BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		protected ZDBOnlyQuery GetContainerQuery(SchemaColumn containerSchemaColumn, SQLComparisonOperator @operator, IZType value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CommonWorkSheet));

			ZDBOnlySubQuery legSubQuery = new ZDBOnlySubQuery(typeof(CommonCartageLeg), JobContainerLegsSchema.JU_EY_RunSheet);
			ZDBOnlySubQuery moveSubQuery = new ZDBOnlySubQuery(typeof(CommonBookedCtgMove), JobContainerLegsSchema.JU_EW);
			ZDBOnlySubQuery containerSubQuery = new ZDBOnlySubQuery(typeof(CommonContainer), JobBookedCtgMoveSchema.EW_JC_Container);
			containerSubQuery.AddToFilter_PossiblyCommaSeparated(containerSchemaColumn, @operator, value);
			moveSubQuery.AddSubQuery(containerSubQuery, JoinCondition.And);
			legSubQuery.AddSubQuery(moveSubQuery, JoinCondition.And);
			result.AddSubQuery(legSubQuery, JoinCondition.And);

			return result;
		}
	}
}
