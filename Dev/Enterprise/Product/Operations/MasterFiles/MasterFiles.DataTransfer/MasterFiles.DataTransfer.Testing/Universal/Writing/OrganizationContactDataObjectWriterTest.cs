using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class OrganizationContactDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestWrite()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_Mobile = "0011 10 0000 0000";

			var contactBO = orgHeader.Contacts.AddNew();
			contactBO.OC_ContactName = "ABCDE AABBCCDDEE";
			contactBO.OC_Email = "email@email.com";

			var writer = new OrganizationContactDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, contactBO)));
			var contactData = writer.GetDataObject(contactBO);

			AssertEquals("contactData.FullName", "ABCDE AABBCCDDEE", contactData.FullName);
			AssertEquals("contactData.Phone", "0011 10 0000 0000", contactData.Phone);
			AssertEquals("contactData.Email", "email@email.com", contactData.Email);

			orgAddress.OA_Phone = "0022 20 0000 0000";
			contactData = writer.GetDataObject(contactBO);
			AssertEquals("contactData.Phone", "0022 20 0000 0000", contactData.Phone);

			contactBO.OC_Mobile = "0033 30 0000 0000";
			contactData = writer.GetDataObject(contactBO);
			AssertEquals("contactData.Phone", "0033 30 0000 0000", contactData.Phone);

			contactBO.OC_Phone = "0044 40 0000 0000";
			contactData = writer.GetDataObject(contactBO);
			AssertEquals("contactData.Phone", "0044 40 0000 0000", contactData.Phone);
		}
	}
}
