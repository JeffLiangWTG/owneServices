using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	[HttpContextEnabledTest]
	public abstract class LinerAndAgencyBaseWebUserVisibleNotesSupportTest : IWebUserVisibleNotesSupportTest
	{
		protected override void SetAgentNotesVisibility(IWebUserVisibleNotesSupport notesSupport, bool visibility)
		{ }

		protected override BusinessObject GetRelatedBusinessObject(IWebUserVisibleNotesSupport parent)
		{
			return null;
		}

		protected abstract AgencyShipment GetBusinessObjectForHelper();
		//{
		//    return Factory.NewWithValidTestData<AgencyShipment>();
		//}

		protected AgencyShipment Shipment;

		protected override void SetUp()
		{
			Shipment = GetBusinessObjectForHelper();
			base.SetUp();
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
		}
	}
}
