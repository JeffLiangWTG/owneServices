using System.Globalization;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class EventReferenceHelperTest : TestCaseWithFactory
	{
		[HttpContextEnabledTest]
		public void TestGetEventReference()
		{
			var eventHelper = new EventReferenceHelper(Helper.TestSiteUser);
			var eventReference = eventHelper.GetEventReferences(WebParties);
			var expectedReference = string.Format(CultureInfo.CurrentCulture, "{0},{1}", WebPartyType.ExportBroker, WebPartyType.ImportBroker);

			AssertEquals("Event reference identified logged in organization", expectedReference, eventReference);
		}

		WebPartyTypeOrgPairCollection WebParties
		{
			get
			{
				if (webParties == null)
				{
					webParties = new WebPartyTypeOrgPairCollection();
					webParties.Add(WebPartyType.ExportBroker, Helper.TestSiteUser.LoggedInOrganisation);
					webParties.Add(WebPartyType.Consignee, Factory.New<OrgHeader>());
					webParties.Add(WebPartyType.ImportBroker, Helper.TestSiteUser.LoggedInOrganisation);
				}

				return webParties;
			}
		}
		WebPartyTypeOrgPairCollection webParties;

		TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}
		TestHelper helper;
	}
}
