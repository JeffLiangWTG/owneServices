using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	class OrgContactWrapperTest : TestCaseWithFactory
	{
		public void TestOrgContactWrapperWithNullContact()
		{
			AssertNull(OrgContactWrapper.New(null));
		}

		public void TestOrgContactWrapper()
		{
			var contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "TEST CONTACT";
			contact.OC_Email = "Why@WasThisNotTestedTo.Start.With";
			contact.OC_Phone = "90018765";
			contact.OC_Fax = "90018766";
			contact.OC_Mobile = "0434436090";
			IContact orgContactWrapper = OrgContactWrapper.New(contact);
			AssertEquals(4, orgContactWrapper.Communications.Count());
			AssertEquals("Why@WasThisNotTestedTo.Start.With", orgContactWrapper.Communications.First(x => x.ContactType == CommunicationTypeList.Codes.EM).ContactDetail);
			AssertEquals("90018765", orgContactWrapper.Communications.First(x => x.ContactType == CommunicationTypeList.Codes.TE).ContactDetail);
			AssertEquals("0434436090", orgContactWrapper.Communications.First(x => x.ContactType == CommunicationTypeList.Codes.AL).ContactDetail);
			AssertEquals("90018766", orgContactWrapper.Communications.First(x => x.ContactType == CommunicationTypeList.Codes.FX).ContactDetail);
		}
	}
}
