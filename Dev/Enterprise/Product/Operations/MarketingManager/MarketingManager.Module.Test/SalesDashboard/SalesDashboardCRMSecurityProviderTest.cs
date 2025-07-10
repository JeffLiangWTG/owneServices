using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.MarketingManager.Business.SalesDashboardActivityTypeCodeList;

namespace Enterprise.MarketingManager.Module.Testing
{
	public class SalesDashboardCRMSecurityProviderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestDashboardCRMSecurity()
		{
			SetCheckpoints(true);
			var salesDashboardCRMSecurityProvider = new SalesDashboardCRMSecurityProvider();
			var filters = new ModuleFilterCollection();
			salesDashboardCRMSecurityProvider.AddCRMSecurityFilterStrips(Factory, filters);
			AssertEquals(0, filters.Count());

			SetCheckpoints(false);
			salesDashboardCRMSecurityProvider.AddCRMSecurityFilterStrips(Factory, filters);
			AssertEquals(1, filters.Count());

			var filter = filters["SalesDashboardCRMSecurityFilter"] as ModuleFlagsFilter;
			AssertNotNull("SalesDashboardCRMSecurityFilter", filter);
			AssertEquals(FilterOrCategory.MandatoryFilterOrCategory, filter.OrCategory);

			var bizObj = Factory.Load<SalesDashboardActivity>(filter.Query);

			var filterTypes = new string[]
			{
				Descriptions.Campaign,
				Descriptions.Communication,
				Descriptions.Inquiry,
				Descriptions.OneOffQuote,
				Descriptions.Opportunity,
				Descriptions.Project,
				Descriptions.Quotation
			};

			foreach (var type in filterTypes)
			{
				Assert("Enforced CRM Security for " + type, filter.DefaultProperties[type]);
			}
		}

		void SetCheckpoints(bool isAllowed)
		{
			Env.Security.InquiryManagerCRMSecurity.ViewByStaffNotAssigned.IsAllowed = isAllowed;
			Env.Security.InquiryManagerCRMSecurity.IgnoreOSMG.IsAllowed = isAllowed;
			Env.Security.InquiryManagerCRMSecurity.IgnoreTaskAssignment.IsAllowed = isAllowed;

			Env.Security.OpportunityManagementCRMSecurity.ViewByStaffNotAssigned.IsAllowed = isAllowed;
			Env.Security.OpportunityManagementCRMSecurity.IgnoreOSMG.IsAllowed = isAllowed;
			Env.Security.OpportunityManagementCRMSecurity.IgnoreTaskAssignment.IsAllowed = isAllowed;

			Env.Security.CommunicationManagerCRMSecurity.ViewByStaffNotAssigned.IsAllowed = isAllowed;
			Env.Security.CommunicationManagerCRMSecurity.IgnoreOSMG.IsAllowed = isAllowed;
			Env.Security.CommunicationManagerCRMSecurity.IgnoreTaskAssignment.IsAllowed = isAllowed;

			Env.Security.CampaignManagementCRMSecurity.ViewByStaffNotAssigned.IsAllowed = isAllowed;
			Env.Security.CampaignManagementCRMSecurity.IgnoreTaskAssignment.IsAllowed = isAllowed;

			Env.Security.OneOffQuoteCRMSecurity.ViewByStaffNotAssigned.IsAllowed = isAllowed;
			Env.Security.OneOffQuoteCRMSecurity.IgnoreOSMG.IsAllowed = isAllowed;
			Env.Security.OneOffQuoteCRMSecurity.IgnoreTaskAssignment.IsAllowed = isAllowed;

			Env.Security.QuotationCRMSecurity.ViewByStaffNotAssigned.IsAllowed = isAllowed;
			Env.Security.QuotationCRMSecurity.IgnoreOSMG.IsAllowed = isAllowed;
			Env.Security.QuotationCRMSecurity.IgnoreTaskAssignment.IsAllowed = isAllowed;

			Env.Security.ProjectCRMSecurity.ViewByStaffNotAssigned.IsAllowed = isAllowed;
			Env.Security.ProjectCRMSecurity.IgnoreOSMG.IsAllowed = isAllowed;
			Env.Security.ProjectCRMSecurity.IgnoreTaskAssignment.IsAllowed = isAllowed;
		}
	}
}
