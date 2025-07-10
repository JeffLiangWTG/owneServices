using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class ProofOfPaymentAutoDeliveryJobTest : AutoDocumentDeliveryJobTest
	{
		public new void TestDeliver_WithIncompleteDeliveryInstructions()
		{
			OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
			Recipient.OC_Email = ""; // invalid delivery instructions
			Factory.Save();
			DocumentDelivery.Deliver(Notifications);
			AssertDocumentNotDelivered("clinton@edi.com.au", Core.Constants.ContactNotifyModes.Email);
		}

		public new void TestDeliver_ByEmail()
		{
			OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
			Recipient.OC_Email = "clinton@edi.com.au";
			Factory.Save();
			(BusinessObjectToDeliver as VAT404Document).DeliveryContacts.RemoveAll();
			var newContact = (BusinessObjectToDeliver as VAT404Document).DeliveryContacts.AddNew();
			newContact.Email = "clinton2@edi.com.au";
			newContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			DocumentDelivery.Deliver(Notifications);
			AssertDocumentDelivered("clinton2@edi.com.au", Core.Constants.ContactNotifyModes.Email);
		}

		public new void TestDeliver_ByFax()
		{
			OrgDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Fax;
			Recipient.OC_Fax = "+61280012200";
			Factory.Save();
			(BusinessObjectToDeliver as VAT404Document).DeliveryContacts.RemoveAll();
			var newContact = (BusinessObjectToDeliver as VAT404Document).DeliveryContacts.AddNew();
			newContact.Fax = "+61280012201";
			newContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Fax;
			DocumentDelivery.Deliver(Notifications);
			AssertDocumentDelivered("+61280012201", Core.Constants.ContactNotifyModes.Fax);
		}

		public new void TestDeliverInNonUserInteractiveEnvironmentDoesNotShowAnyForms()
		{
			var newContact = (BusinessObjectToDeliver as VAT404Document).DeliveryContacts.AddNew();
			newContact.Email = "jimmy.luong@cargowise.com";
			newContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			base.TestDeliverInNonUserInteractiveEnvironmentDoesNotShowAnyForms();
		}

		public new void TestDeliveryGroupUsesOverridenEmailSubjectInMenuItem()
		{
			var newContact = (BusinessObjectToDeliver as VAT404Document).DeliveryContacts.AddNew();
			newContact.Email = "unit.test@cargowise.com";
			newContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			base.TestDeliveryGroupUsesOverridenEmailSubjectInMenuItem();
		}

		public new void TestDeliver_WhenJobNotApplicable()
		{
			Assert("Not Supported", true);
		}

		public new void TestDeliver_WithInvalidFaxDetails()
		{
			Assert("Not Supported", true);
		}

		protected override DocumentCommand DocumentCommand
		{
			get
			{
				var filter = new DocumentZQuery(CargoWise.Definitions.BusinessContext.ZAProofOfPayment, "VAT 404 Proof Of Payment");
				return Factory.LoadTop1<DocumentCommand>(filter);
			}
		}

		protected override IDocumentSupportable NewBusinessObjectToDeliver()
		{
			return VAT404TestHelper.GetVAT404Document(Factory);
		}

		protected override AutoDocumentDeliveryJob NewDocumentDeliveryJob(bool sendToDocManager)
		{
			return new ProofOfPaymentAutoDeliveryJob(BusinessObjectToDeliver as VAT404Document, DocumentCommand.PK);
		}
	}
}
