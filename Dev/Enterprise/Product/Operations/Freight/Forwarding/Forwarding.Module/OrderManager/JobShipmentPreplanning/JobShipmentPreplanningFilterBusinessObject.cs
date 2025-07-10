using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Module
{
	public class JobShipmentPreplanningFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();

			AddNumberFilters(result);
			AddOrganisationFilters(result);
			AddLocationFilters(result);
			AddGuidFilters(result);

			return result;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			helpers.Add(new WorkflowFilterStripsHelperWithRoutingSupport(typeof(JobShipmentPreplanning), WorkflowDescriptors.JobShipmentPreplanningWorkflowDescriptorCode, Factory));

			return helpers;
		}

		#region Filters

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter("Masterbill", JobShipmentPreplanningSchema.EF_MasterBill).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentPreplanningFilter|Masterbill", "Master Bill");
			filters.AddNumberFilter("Housebill", JobShipmentPreplanningSchema.EF_HouseBill).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentPreplanningFilter|Housebill", "House Bill");
		}

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			ModuleLocationFilter loadDischargeFilter = filters.AddLocationFilter("Load / Discharge", JobShipmentPreplanningSchema.EF_RL_NKPortLoad, PortList, JobShipmentPreplanningSchema.EF_RL_NKPortDisch, PortList);
			loadDischargeFilter.SetItemDescriptions(Res.GetData("Forwarding|JobShipmentPreplanningFilter|Load", "Load"), Res.GetData("Forwarding|JobShipmentPreplanningFilter|Discharge", "Discharge"));
			loadDischargeFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentPreplanningFilter|LoadDischarge", "Load / Discharge");
		}

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			ModuleGuidsFilter sendReceiveAgentsFilter = filters.AddGuidFilter("Send / Receive Agents", ModuleIDs.Organisation, JobShipmentPreplanningSchema.EF_OH_SendingAgent, OrgList, JobShipmentPreplanningSchema.EF_OH_ReceivingAgent, OrgList);
			sendReceiveAgentsFilter.SetItemDescriptions(Res.GetData("Forwarding|JobShipmentPreplanningFilter|SendAgent", "Send Agent"), Res.GetData("Forwarding|JobShipmentPreplanningFilter|RecAgent", "Rec. Agent"));
			sendReceiveAgentsFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentPreplanningFilter|SendReceiveAgents", "Send / Receive Agents");

			filters.AddGuidFilter("Buyer", ModuleIDs.Organisation, GetBuyerQuery, BuyerList).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentPreplanningFilter|Buyer", "Buyer");
		}

		void AddGuidFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter("Shipment", ModuleIDs.JobShipment, JobShipmentPreplanningSchema.EF_JS, ShipmentList).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentPreplanningFilter|Shipment", "Shipment");
			filters.AddGuidFilter("Declaration", ModuleIDs.Customs.JobDeclaration, JobShipmentPreplanningSchema.EF_JE, DeclarationList).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentPreplanningFilter|Declaration", "Declaration");
		}

		ZQuery GetBuyerQuery(ZGuid buyerPK)
		{
			return GetOrgAddressQuery(buyerPK, JobShipmentPreplanningSchema.EF_OA_BuyerAddress);
		}

		ZQuery GetOrgAddressQuery(ZGuid orgPK, SchemaColumn foreignKeyColumn)
		{
			var result = new ZDBOnlyQuery(typeof(JobShipmentPreplanning));
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), foreignKeyColumn);
			orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, orgPK);
			result.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
			return result;
		}

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			ModuleNumberFilter preAdviceIDFilter = new ModuleNumberFilter("Pre Advice ID", JobShipmentPreplanningSchema.EF_PreshipID);
			preAdviceIDFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobShipmentPreplanningFilter|PreAdviceID", "Pre Advice ID");
			return preAdviceIDFilter;
		}

		#endregion

		#region Lookups

		public LocationCollection PortList
		{
			get
			{
				if (fPortList == null)
				{
					fPortList = new LocationCollection(Factory);
				}
				return fPortList;
			}
		}

		LocationCollection fPortList;

		public OrganisationsFindBoxCollection OrgList
		{
			get
			{
				if (fOrgList == null)
				{
					fOrgList = new OrganisationsFindBoxCollection(Factory);
				}
				return fOrgList;
			}
		}

		OrganisationsFindBoxCollection fOrgList;

		public ConsigneeCollection BuyerList
		{
			get
			{
				if (fBuyerList == null)
				{
					fBuyerList = new ConsigneeCollection(Factory);
				}
				return fBuyerList;
			}
		}

		ConsigneeCollection fBuyerList;

		public ShipmentCollection ShipmentList
		{
			get
			{
				if (fShipmentList == null)
				{
					fShipmentList = new ShipmentCollection(Factory);
				}
				return fShipmentList;
			}
		}

		ShipmentCollection fShipmentList;

		public BusinessObjectCollection DeclarationList
		{
			get
			{
				if (fDeclarationList == null)
				{
					fDeclarationList = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclarationCollection>(), Factory);
				}

				return fDeclarationList;
			}
		}

		BusinessObjectCollection fDeclarationList;

		#endregion
	}
}
