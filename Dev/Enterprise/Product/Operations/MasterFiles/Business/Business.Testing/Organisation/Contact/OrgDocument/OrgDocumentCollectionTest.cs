using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgDocumentCollection))]
	sealed class OrgDocumentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestContainsDocGroup()
		{
			OrgDocument doc1 = Documents.AddNew();
			doc1.OD_DocumentGroup = ContactType.Consignee.Code;

			Assert("Should contain doc group", Documents.ContainsDocGroup(ContactType.Consignee.Code));
			Assert("Should not contain doc group", !Documents.ContainsDocGroup(ContactType.NotifyParty.Code));
		}

		public void TestContainsDefault()
		{
			OrgDocument doc1 = Documents.AddNew();
			doc1.OD_DocumentGroup = ContactType.Consignee.Code;
			doc1.OD_DefaultContact = true;

			OrgDocument doc2 = Documents.AddNew();
			doc2.OD_DocumentGroup = ContactType.NotifyParty.Code;
			doc2.OD_DefaultContact = false;

			Assert("Default should be found", Documents.ContainsDefault(ContactType.Consignee.Code));
			Assert("Default should not be found", !Documents.ContainsDefault(ContactType.NotifyParty.Code));
			Assert("Default should not be found", !Documents.ContainsDefault(ContactType.Consignor.Code));
		}

		public void TestContainsDocumentWithDeliveryMode()
		{
			OrgDocument doc1 = Documents.AddNew();
			doc1.OD_DeliverBy = Constants.ContactNotifyModes.Email;

			OrgDocument doc2 = Documents.AddNew();
			doc2.OD_DeliverBy = Constants.ContactNotifyModes.Fax;

			Assert("The Document Collection should contain a document with delivery mode fax", Documents.ContainsDocumentWithDeliveryMode(Constants.ContactNotifyModes.Fax));
			Assert("The Document Collection should contain a document with delivery mode email", Documents.ContainsDocumentWithDeliveryMode(Constants.ContactNotifyModes.Email));
			Assert("The Document Collection should not contain a document with delivery mode print", !Documents.ContainsDocumentWithDeliveryMode(Constants.ContactNotifyModes.Print));
		}

		public void TestDefaultsForNewChild()
		{
			Contact.OC_NotifyMode = Constants.ContactNotifyModes.Email;
			Contact.OC_AttachmentType = OrgConstants.AttachmentType.TIF;
			OrgDocument doc1 = Contact.Documents.AddNew();
			AssertEquals("OD_DeliverBy", Constants.ContactNotifyModes.Email, doc1.OD_DeliverBy);
			AssertEquals("OD_AttachmentType", OrgConstants.AttachmentType.TIF, doc1.OD_AttachmentType);

			Contact.OC_NotifyMode = Constants.ContactNotifyModes.Print;
			Contact.OC_AttachmentType = "";
			OrgDocument doc2 = Contact.Documents.AddNew();
			AssertEquals("OD_DeliverBy", Constants.ContactNotifyModes.Print, doc2.OD_DeliverBy);
			AssertEquals("OD_AttachmentType", "", doc2.OD_AttachmentType);

			//Doc1 still the same
			AssertEquals("OD_DeliverBy", Constants.ContactNotifyModes.Email, doc1.OD_DeliverBy);
			AssertEquals("OD_AttachmentType", OrgConstants.AttachmentType.TIF, doc1.OD_AttachmentType);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgDocumentCollection(Contact, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			OrgHeader org = OrgHeader.New(Factory);
			org.FillWithValidTestData();
			Contact = org.Contacts.AddNew();
			Documents = Contact.Documents;
		}

		OrgContact Contact;
		OrgDocumentCollection Documents;

		#endregion
	}
}
