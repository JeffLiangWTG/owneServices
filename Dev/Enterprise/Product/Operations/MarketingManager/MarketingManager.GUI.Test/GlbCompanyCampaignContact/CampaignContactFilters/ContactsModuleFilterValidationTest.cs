using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MarketingManager.GUI.GlbCompanyCampaignContactFilterBusinessObject;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class ContactsModuleFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckSelectedFiltersDescription()
		{
			Env.Security.OrgContactView.IsAllowed = true;
			var filter = new ContactsModuleFilter(FilterDescription.Contacts, OrgContactSchema.PK, ViewCampaignContactSchema.PK, new OrgContactCollection(Factory), typeof(CampaignContact));
			filter.Validation.ValidateSelectedFiltersDescription();
			AssertNoErrors(filter.SelectedFiltersDescriptionInfo);

			Env.Security.OrgContactView.IsAllowed = false;
			filter.Validation.ValidateSelectedFiltersDescription();
			AssertHasError(filter.SelectedFiltersDescriptionInfo, @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Master Data -> Organization Contacts -> View");
		}
	}
}
