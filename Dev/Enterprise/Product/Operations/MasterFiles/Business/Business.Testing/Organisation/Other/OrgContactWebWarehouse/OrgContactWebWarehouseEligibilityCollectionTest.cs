using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgContactWebWarehouseEligibilityCollection))]
	sealed class OrgContactWebWarehouseEligibilityCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OrgContactWebWarehouseEligibilityCollection>
	{
		#region TestConstructor_NoPivotsMeansGrantedAccessToAll

		public void TestConstructor_NoPivotsMeansGrantedAccessToAll()
		{
			var whsHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs1 = whsHelper.CreateWarehouse("WHS-1", "R");
			var whs2 = whsHelper.CreateWarehouse("WHS-2", "R");
			var contact = Factory.New<OrgContact>();

			var collection = new OrgContactWebWarehouseEligibilityCollection(Factory, contact);

			AssertEquals(2, collection.Count);
			AssertNotNull(collection.Cast<OrgContactWebWarehouseEligibility>().SingleOrDefault(x => x.WarehousePK == whs1.PK));
			AssertNotNull(collection.Cast<OrgContactWebWarehouseEligibility>().SingleOrDefault(x => x.WarehousePK == whs2.PK));
			Assert("All access is granted by default", collection.Cast<OrgContactWebWarehouseEligibility>().All(x => x.IsGranted));
		}

		#endregion

		#region TestConstructor_AccessIsDeniedForWarehousesWithPivots

		public void TestConstructor_AccessIsDeniedForWarehousesWithPivots()
		{
			var whsHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs1 = whsHelper.CreateWarehouse("WHS-1", "R");
			var whs2 = whsHelper.CreateWarehouse("WHS-2", "R");
			var contact = Factory.New<OrgContact>();

			CreatePivot(whs1.PK, contact.PK);

			var collection = new OrgContactWebWarehouseEligibilityCollection(Factory, contact);

			AssertEquals(2, collection.Count);
			var whs1Eligibility = collection.Cast<OrgContactWebWarehouseEligibility>().SingleOrDefault(x => x.WarehousePK == whs1.PK);
			AssertNotNull(whs1Eligibility);
			AssertEquals("As there are pivots for that contact, access should be denied for warehouse with the pivot.", false, whs1Eligibility.IsGranted);

			var whs2Eligibility = collection.Cast<OrgContactWebWarehouseEligibility>().SingleOrDefault(x => x.WarehousePK == whs2.PK);
			AssertNotNull(whs2Eligibility);
			AssertEquals("There is no pivot for this warehouse, so access shold be granted.", true, whs2Eligibility.IsGranted);
		}

		#endregion

		#region TestConstructor_ContactMustNotBeNull

		public void TestConstructor_ContactMustNotBeNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new OrgContactWebWarehouseEligibilityCollection(Factory, null));
		}

		#endregion

		#region TestInactiveWarehousesAreIgnored

		public void TestInactiveWarehousesAreIgnored()
		{
			var whsHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs1 = whsHelper.CreateWarehouse("WHS-1", "R");
			var whs2 = whsHelper.CreateWarehouse("WHS-2", "R");
			whs2[WhsWarehouseSchema.WW_IsActive] = false;
			var contact = Factory.New<OrgContact>();

			var collection = new OrgContactWebWarehouseEligibilityCollection(Factory, contact);

			AssertEquals("Only active warehouses must be added to collection.", 1, collection.Count);
			AssertEquals(whs1.PK, collection[0].WarehousePK);
		}

		#endregion

		#region TestPivotsCreatedOrDeletedWhenAccessChanges

		public void TestPivotsCreatedOrDeletedWhenAccessChanges()
		{
			var whsHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs1 = whsHelper.CreateWarehouse("WHS-1", "R");
			var whs2 = whsHelper.CreateWarehouse("WHS-2", "R");
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			Assert(!contact.HasChanges);

			var collection = new OrgContactWebWarehouseEligibilityCollection(Factory, contact);
			AssertEquals(2, collection.Count);
			var whs1Eligibility = collection.Cast<OrgContactWebWarehouseEligibility>().Single(x => x.WarehousePK == whs1.PK);
			var whs2Eligibility = collection.Cast<OrgContactWebWarehouseEligibility>().Single(x => x.WarehousePK == whs2.PK);
			Assert("All access is granted by default", collection.Cast<OrgContactWebWarehouseEligibility>().All(x => x.IsGranted));

			whs1Eligibility.IsGranted = false;
			Assert(contact.HasChanges);

			var genPivotQuery = new ZQuery();
			genPivotQuery.AddToFilter(GenPivotSchema.XX_RelationType, Constants.GenPivotTypes.OrgContactDeniedWarehouse);
			genPivotQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, OrgContactSchema.Constants.Prefix);
			genPivotQuery.AddToFilter(GenPivotSchema.XX_Relation1ID, contact.PK);
			genPivotQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, WhsWarehouseSchema.Constants.Prefix);
			var pivots = Factory.Load<GenPivot>(genPivotQuery);
			AssertEquals(1, pivots.Length);
			AssertContainsExactElementsInAnyOrder(new[] { whs1.PK }, pivots.Select(p => p.XX_Relation2ID));

			whs2Eligibility.IsGranted = false;
			pivots = Factory.Load<GenPivot>(genPivotQuery);
			AssertEquals(2, pivots.Length);
			AssertContainsExactElementsInAnyOrder(new[] { whs1.PK, whs2.PK }, pivots.Select(p => p.XX_Relation2ID));

			whs2Eligibility.IsGranted = true;
			pivots = Factory.Load<GenPivot>(genPivotQuery);
			AssertEquals(1, pivots.Length);
			AssertContainsExactElementsInAnyOrder(new[] { whs1.PK }, pivots.Select(p => p.XX_Relation2ID));

			whs1Eligibility.IsGranted = true;
			pivots = Factory.Load<GenPivot>(genPivotQuery);
			AssertEquals(0, pivots.Length);
		}

		#endregion

		#region Helpers

		void CreatePivot(ZGuid warehousePK, ZGuid orgContactPK)
		{
			var pivot = Factory.New<GenPivot>();
			pivot.XX_Relation1ID = orgContactPK;
			pivot.XX_Relation2ID = warehousePK;
			pivot.XX_Relation1TableCode = OrgContactSchema.Constants.Prefix;
			pivot.XX_Relation2TableCode = WhsWarehouseSchema.Constants.Prefix;
			pivot.XX_RelationType = Constants.GenPivotTypes.OrgContactDeniedWarehouse;
		}

		#endregion

		#region Implementation

		protected override OrgContactWebWarehouseEligibilityCollection GetCollectionToTest()
		{
			var contact = Factory.New<OrgContact>();
			return new OrgContactWebWarehouseEligibilityCollection(Factory, contact);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var warehouse = Factory.New<IWhsWarehouse>();
			var contact = Factory.New<OrgContact>();
			return new OrgContactWebWarehouseEligibility(Factory, warehouse, contact, null);
		}

		#endregion
	}
}
