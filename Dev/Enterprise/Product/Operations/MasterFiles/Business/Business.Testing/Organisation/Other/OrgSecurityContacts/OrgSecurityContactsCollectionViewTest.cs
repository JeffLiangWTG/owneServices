using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSecurityContactsCollectionView))]
	class OrgSecurityContactsCollectionViewTest : BusinessObjectCollectionViewTestCase<OrgSecurityContactsCollectionView>
	{
		protected new OrgSecurityContactsCollectionView Collection
		{
			get { return base.Collection; }
		}

		protected override OrgSecurityContactsCollectionView GetCollectionToTest()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var securityCollection = new OrgSecurityContactsCollection(org.SecurityRights[0]);
			var view = new OrgSecurityContactsCollectionView(securityCollection);
			return view;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgSecurityContacts security = org.SecurityRights[0].ContactSecurityRights.AddNew();
			return security;
		}

		public void TestIsThisPartOfTheCollection()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var report = Factory.New<StmMenuItem>();
			report.SU_BusinessContext = "RepTest";
			report.SU_MenuName = "Test Report";
			report.SU_MenuType = "WEB";

			OrgSecurity security1 = org.SecurityRights[0];

			var security2 = Factory.New<OrgSecurity>();
			security2.OX_SecurityItemName = "Dummy Security";
			security2.OX_OH = org.PK;
			security2.OX_Granted = true;

			var security3 = Factory.New<OrgSecurity>();
			security3.OX_SecurityItemName = string.Empty;
			security3.OX_OH = org.PK;
			security3.OX_Granted = true;
			security3.OX_SU = report.PK;

			OrgContact contact = org.Contacts.AddNew();

			var contactSecurity1 = Factory.New<OrgSecurityContacts>();
			contactSecurity1.OZ_OX = security1.PK;
			contactSecurity1.OZ_OC = contact.PK;
			contactSecurity1.OZ_Granted = true;

			var contactSecurity2 = Factory.New<OrgSecurityContacts>();
			contactSecurity2.OZ_OX = security2.PK;
			contactSecurity2.OZ_OC = contact.PK;
			contactSecurity2.OZ_Granted = true;

			var contactSecurity3 = Factory.New<OrgSecurityContacts>();
			contactSecurity3.OZ_OX = security3.PK;
			contactSecurity3.OZ_OC = contact.PK;
			contactSecurity3.OZ_Granted = true;

			var collection = new OrgSecurityContactsCollection(contact);
			collection.Add(contactSecurity1);
			collection.Add(contactSecurity2);
			collection.Add(contactSecurity3);

			var collectionView = new OrgSecurityContactsCollectionView(collection);
			AssertCollectionContains(contactSecurity1, collectionView);
			AssertCollectionNotContains(contactSecurity2, collectionView);
			AssertCollectionContains(contactSecurity3, collectionView);
		}

		public void TestIsThisPartOfTheCollection_WebWarehouse()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			OrgSecurity nonWarehouseSecurity = org.SecurityRights.FirstOrDefault(s => !((OrgSecurity)s).IsWebWarehouseSecurity) as OrgSecurity;
			AssertNotNull(nonWarehouseSecurity);
			OrgSecurity warehouseSecurity = org.SecurityRights.FirstOrDefault(s => ((OrgSecurity)s).IsWebWarehouseSecurity) as OrgSecurity;
			AssertNotNull(warehouseSecurity);

			OrgContact contact = org.Contacts.AddNew();

			var contactSecurity1 = Factory.New<OrgSecurityContacts>();
			contactSecurity1.OZ_OX = nonWarehouseSecurity.PK;
			contactSecurity1.OZ_OC = contact.PK;
			contactSecurity1.OZ_Granted = true;

			var contactSecurity2 = Factory.New<OrgSecurityContacts>();
			contactSecurity2.OZ_OX = warehouseSecurity.PK;
			contactSecurity2.OZ_OC = contact.PK;
			contactSecurity2.OZ_Granted = true;

			var collection = new OrgSecurityContactsCollection(contact);
			collection.Add(contactSecurity1);
			collection.Add(contactSecurity2);

			org.OH_IsWarehouseClient = true;
			var collectionView = new OrgSecurityContactsCollectionView(collection);
			AssertCollectionContains("Non-warehouse should be in", contactSecurity1, collectionView);
			AssertCollectionContains("Warehouse should be in", contactSecurity2, collectionView);

			org.OH_IsWarehouseClient = false;
			collectionView = new OrgSecurityContactsCollectionView(collection);
			AssertCollectionContains("Non-warehouse should be in", contactSecurity1, collectionView);
			AssertCollectionNotContains("Warehouse should not be in", contactSecurity2, collectionView);
		}
	}
}
