using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingModuleOrgAgentRelationshipCollection : OrgAgentRelationshipCollection, IFilterModuleExtraNotificationProvider
	{
		public ForwardingModuleOrgAgentRelationshipCollection(BusinessObjectFactory factory) : base(factory)
		{
			AddDefaultFilters();
		}

		public ForwardingModuleOrgAgentRelationshipCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
			AddDefaultFilters();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filters")]
		void AddDefaultFilters()
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Agency Office", "Property", (ZString)GlbCompany.CurrentCompany.GC_OH_OrgProxy.ToString()));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Profit Share Type", "Property", (ZString)OrgAgentRelationship.ProfitShareTypes.AgencyProfile));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Job Type", "Property", (ZString)JobInvoicingConsumerTypes.GatewayConsolCode));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Gateway Profit Apportionment Method", "ComparisonOperator", new ZString("is not blank")));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Start Date", "PropertySearch", new ZString("Date Range")));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("End Date", "PropertySearch", new ZString("Date Range")));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Sending Location", "Property", ZString.Empty));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Receiving Location", "Property", ZString.Empty));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Freight Mode", "Property", ZString.Empty));
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((OrgAgentRelationship)child).O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.AgencyProfile;
		}

		public INotification GetExtraNotification(BusinessObject businessObject)
		{
			var profitShareAgreement = (OrgAgentRelationship)businessObject;

			var error = ZString.Empty;
			if (profitShareAgreement.O3_ProfitShareType != OrgAgentRelationship.ProfitShareTypes.AgencyProfile)
			{
				error = Res.GetString("A91B0492-3E4A-488B-B840-17CEE7538C45", "Only Agency Profit Share Type can be selected.");
			}
			else if (!(profitShareAgreement.SendingAgent?.IsProxyOrg(GlbCompany.CurrentCompany) ?? false))
			{
				error = Res.GetString("72B79E7F-1F8C-47BA-B8F4-A90B5C32C98E", "The Agency Office of Profit Share Agreement must be an Organization Proxy of the Current Company.");
			}
			else
			{
				var matchJobTypeAndApportionmentMethod = profitShareAgreement
					.ProfitShareDetails
					.OfType<OrgProfitShareDetails>()
					.Any(x =>
						x.O4_JobType == JobTypesList.Codes.GCN &&
						!x.O4_GatewayProfitApportionmentMethod.IsEmpty);
				if (!matchJobTypeAndApportionmentMethod)
				{
					error = Res.GetString("0c0d6c95-102b-467a-8cc7-944b3128beb0", "Profit Share Setup (Rules) must have Job Type GCN and non-blank GW Profit Apportionment Method.");
				}
			}

			if (error.IsEmpty)
			{
				return null;
			}

			return new Notification(CargoWise.EntityFramework.NotificationType.Error, error);
		}
	}
}
