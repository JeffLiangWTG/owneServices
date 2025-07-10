using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module
{
	public class ContainerLoadPlanFilterBusinessObject : FilterStripBusinessObject
	{
		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var helper = new WorkflowFilterStripsHelperWithRoutingSupport(typeof(CFSContainerLoadList), WorkflowDescriptors.ContainerLoadPlanWorkflowDescriptorCode, Factory);
			helpers.Add(helper);

			return helpers;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			filters.AddTextFilter("Load List #", ContainerLoadListHeaderSchema.CLH_LoadListId).With(
				filter =>
				{
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadPlanFilter|LoadPlan", "Load Plan #");
					filter.Visibility = FilterVisibility.AlwaysVisible;
					filter.MaxLength = ContainerLoadListHeaderSchema.CLH_LoadListId.MaxLength;
				});

			filters.AddTextFilter("Load List Status", ContainerLoadListHeaderSchema.CLH_Status, StatusList).With(
				filter =>
				{
					filter.Category = FilterCategories.StatusAndFlags;
					filter.Visibility = FilterVisibility.AlwaysVisible;
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadPlanFilter|CLH_Status", "Load Plan Status");
					filter.MaxLength = ContainerLoadListHeaderSchema.CLH_Status.MaxLength;
				});

			filters.AddTextFilter("Planned Transport Mode", ContainerLoadListHeaderSchema.CLH_PlannedTransportMode, TransportMode_List).With(
				filter =>
				{
					filter.Category = FilterCategories.ModesAndTypes;
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadPlanFilter|PlannedTransportMode", "Planned Transport Mode");
					filter.MaxLength = ContainerLoadListHeaderSchema.CLH_PlannedTransportMode.MaxLength;
				});

			filters.AddTextFilter("CFS", GetOrgFullNameFilter).With(
				filter =>
				{
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadPlanFilter|CFS", "CFS");
					filter.Visibility = FilterVisibility.AlwaysVisible;
					filter.MaxLength = ContainerLoadListHeaderSchema.CLH_OA_CFSAddress.MaxLength;
				});

			filters.AddGuidFilter("CFS Address", ModuleIDs.OrgAddresses, GetDocAddressQueryWithOperatorDelegate(DocAddressType.LocalCartageCFS), BindingLists.OrgHeader_List).With(
				filter =>
				{
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadPlanFilter|CLH_OA_CFSAddress", "CFS Address");
					filter.Visibility = FilterVisibility.AlwaysVisible;
					filter.SupportsBlankComparisonOperators = true;
					filter.MaxLength = ContainerLoadListHeaderSchema.CLH_OA_CFSAddress.MaxLength;
				});

			filters.AddLocationFilter("Planned Load / Planned Discharge Port", GetLoadDischargePortsForClpQuery, BindingLists.RefLocation_List, BindingLists.RefLocation_List).With(
				filter =>
				{
					filter.SetItemDescriptions(Res.GetData("Forwarding|ContainerLoadPlanFilter|PlannedLoadPort", "Load"), Res.GetData("Forwarding|ContainerLoadPlanFilter|PlannedDischargePort", "Discharge"));
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadPlanFilter|PlannedLoadDischarge", "Planned Load / Discharge Port");
					filter.Visibility = FilterVisibility.AlwaysVisible;
					filter.MaxLength = ContainerLoadListHeaderSchema.CLH_RL_NKPlannedLoadPort.MaxLength;
				});

			filters.AddGuidFilter("Controlling Customer", ModuleIDs.Organisation, GetDocAddressQueryWithOperatorDelegate(DocAddressType.ControllingCustomer), BindingLists.OrgHeader_List).With(
				filter =>
				{
					filter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|ContainerLoadListFilter|ControllingCustomer", "Controlling Customer");
					filter.Visibility = FilterVisibility.AlwaysVisible;
					filter.MaxLength = ContainerLoadListHeaderSchema.CLH_OA_CFSAddress.MaxLength;
					filter.SupportsFiltersMatchComparisonOperator = false;
				});
			return filters;
		}

		ZQuery GetLoadDischargePortsForClpQuery(ZString origin, ZString destination)
		{
			var  containerLoadPlanQuery = new ZDBOnlyQuery(typeof(CFSContainerLoadList));

			if (!origin.IsEmpty)
			{
				containerLoadPlanQuery.AddToFilter(LocationHelper.GetLocationFilter(Factory, origin, ContainerLoadListHeaderSchema.CLH_RL_NKPlannedLoadPort, typeof(CFSContainerLoadList)));
			}
			if (!destination.IsEmpty)
			{
				containerLoadPlanQuery.AddToFilter(LocationHelper.GetLocationFilter(Factory, destination, ContainerLoadListHeaderSchema.CLH_RL_NKPlannedDischargePort, typeof(CFSContainerLoadList)));
			}

			return containerLoadPlanQuery;
		}

		ZQuery GetOrgFullNameFilter(SQLComparisonOperator comparisonOperator, ZString name)
		{
			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, name);

			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressQuery.AddSubQuery(OrgAddressSchema.OA_OH, orgHeaderSubQuery, JoinCondition.And);

			var containerLoadListQuery = new ZDBOnlyQuery(typeof(CFSContainerLoadList));
			containerLoadListQuery.AddSubQuery(ContainerLoadListHeaderSchema.CLH_OA_CFSAddress, orgAddressQuery, JoinCondition.And);

			return containerLoadListQuery;
		}

		GetGuidQueryWithOperator GetDocAddressQueryWithOperatorDelegate(DocAddressType addressType)
		{
			return (SQLComparisonOperator comparisonOperator, object pK) => GetDocAddressFilter(pK, addressType, comparisonOperator);
		}

		ZQuery GetDocAddressFilter(object pK, DocAddressType docAddressType, SQLComparisonOperator comparisonOperator)
		{
			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.PK, pK);

			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressQuery.AddSubQuery(OrgAddressSchema.OA_OH, orgHeaderSubQuery, JoinCondition.And);

			var docAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_OA_Address);
			docAddressQuery.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(CFSContainerLoadList));
			result.AddSubQuery(ContainerLoadListHeaderSchema.CLH_OA_CFSAddress, docAddressQuery, JoinCondition.And);

			if (comparisonOperator == SQLComparisonOperator.IsBlank || comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			}
			else
			{
				ZGuid orgPK = (ZGuid)pK;
				if (!orgPK.IsValid)
				{
					return new ZQuery();
				}
				orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, comparisonOperator, orgPK);
			}

			return result;
		}

		CodeDescriptionPairList StatusList => statusList ?? (statusList = new CommonContainerLoadListStatusList());

		CodeDescriptionPairList statusList;

		BindToLists BindingLists => BindToLists.GetCachedLists(Factory);

		CodeDescriptionPairList TransportMode_List
		{
			get { return FreightCodePairLists.LinkableTransportModeList(); }
		}
	}
}
