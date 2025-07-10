using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSecurityCollectionView))]
	class OrgSecurityCollectionViewTest : BusinessObjectCollectionViewTestCase<OrgSecurityCollectionView>
	{
		protected new OrgSecurityCollectionView Collection
		{
			get { return base.Collection; }
		}

		protected override OrgSecurityCollectionView GetCollectionToTest()
		{
			var securityCollection = new OrgSecurityCollection(Factory);
			var view = new OrgSecurityCollectionView(securityCollection);
			return view;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var security = (OrgSecurity)org.SecurityRights.Find(new ZQuery(OrgSecuritySchema.OX_SecurityItemName, WebSecurityRightsList.WebQuotes.Code))[0];
			return security;
		}

		public void TestIsThisPartOfTheCollection()
		{
			var report = Factory.New<StmMenuItem>();
			report.SU_BusinessContext = "RepTest";
			report.SU_MenuName = "Test Report";
			report.SU_MenuType = "WEB";

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var security1 = org.SecurityRights[0];
			var security2 = Factory.New<OrgSecurity>();
			security2.OX_SecurityItemName = "Dummy Security";
			security2.OX_OH = org.PK;
			security2.OX_Granted = true;

			var security3 = Factory.New<OrgSecurity>();
			security3.OX_SecurityItemName = string.Empty;
			security3.OX_OH = org.PK;
			security3.OX_SU = report.PK;
			security3.OX_Granted = true;

			var collection = new OrgSecurityCollection(Factory);
			collection.Add(security1);
			collection.Add(security2);
			collection.Add(security3);

			var collectionView = new OrgSecurityCollectionView(collection);
			AssertCollectionContains(security1, collectionView);
			AssertCollectionNotContains(security2, collectionView);
			AssertCollectionContains(security3, collectionView);
		}

		public void TestIsThisPartOfTheCollection_WebWarehouse()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			OrgSecurity nonWarehouseSecurity = org.SecurityRights.FirstOrDefault(s => !((OrgSecurity)s).IsWebWarehouseSecurity) as OrgSecurity;
			AssertNotNull(nonWarehouseSecurity);
			OrgSecurity warehouseSecurity = org.SecurityRights.FirstOrDefault(s => ((OrgSecurity)s).IsWebWarehouseSecurity) as OrgSecurity;
			AssertNotNull(warehouseSecurity);

			var collection = new OrgSecurityCollection(Factory);
			collection.Add(nonWarehouseSecurity);
			collection.Add(warehouseSecurity);

			org.OH_IsWarehouseClient = true;
			var collectionView = new OrgSecurityCollectionView(collection);
			AssertCollectionContains("Non-warehouse should be in", nonWarehouseSecurity, collectionView);
			AssertCollectionContains("Warehouse should be in", warehouseSecurity, collectionView);

			org.OH_IsWarehouseClient = false;
			collectionView = new OrgSecurityCollectionView(collection);
			AssertCollectionContains("Non-warehouse should be in", nonWarehouseSecurity, collectionView);
			AssertCollectionNotContains("Warehouse should not be in", warehouseSecurity, collectionView);
		}
	}
}
