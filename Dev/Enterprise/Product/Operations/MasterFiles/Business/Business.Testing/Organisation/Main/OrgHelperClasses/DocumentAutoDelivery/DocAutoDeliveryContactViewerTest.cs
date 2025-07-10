using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DocAutoDeliveryContactViewer))]
	sealed class DocAutoDeliveryContactViewerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			return new DocAutoDeliveryContactViewer(org);
		}

		public void TestGenerateAutoDeliveryContacts()
		{
			BusinessObjectFactory factoryToSave = new BusinessObjectFactory();
			StmMenuItem menuItem = factoryToSave.New<StmMenuItem>();
			menuItem.SU_ContactType = ContactType.TransportServices.Code;
			factoryToSave.Save();

			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Test contact";
			contact.Documents.AddNew();
			contact.Documents[0].OD_DocumentGroup = ContactType.TransportServices.Code;

			DocAutoDeliveryContactViewer contactViewer = new DocAutoDeliveryContactViewer(org);
			contactViewer.MenuItemGuid = menuItem.PK;
			contactViewer.TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			contactViewer.GenerateAutoDeliveryContacts();

			AssertEquals("DocContacts.Count", 1, contactViewer.DocContacts.Count);
			AssertEquals("Contact name", contact.OC_ContactName, contactViewer.DocContacts[0].Name);
		}
	}
}
