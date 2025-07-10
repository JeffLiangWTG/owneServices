using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	class PickFacesFilterControlTest : TestCaseWithFactory
	{
		#region TestColourDeciding

		public void TestColourDeciding()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var whs = helper.CreateWarehouse("WH1", "A", 3, 1);
			var client = helper.CreateClient("C1");
			var product = helper.CreateProduct(client, "P1");
			var locationTypeFix = helper.CreateLocationType("LT1", "LT1 Test", false, 1, LocationClasses.Codes.FIX);
			Factory.Save();

			var location1 = whs.FindLocation("A-1");
			location1.WLV_WLT_LocationType = locationTypeFix.PK;
			var pickFace = helper.CreateProductPickFace(product, client, location1);

			var location2 = whs.FindLocation("A-2");
			location2.WLV_WLT_LocationType = locationTypeFix.PK;

			var location3 = whs.FindLocation("A-3");
			var receivePK = iHelper.CreateWhsReceive(client.PK, whs.PK, "R1", helper.Notify);
			var receiveLinePK = iHelper.CreateWhsReceiveInventoryLine(receivePK, product.PK, 10m, location3.PK);
			location3.WLV_WLT_LocationType = locationTypeFix.PK;

			Factory.Save();

			var pickViewCollection = new WhsPickFaceViewCollection(Factory);
			var filter = new PickFacesFilterBusinessObject();
			AssertEquals("Expected 3 rows in pick face view.", 3, pickViewCollection.Count);

			using (PickFacesFilterControl control = new PickFacesFilterControl(pickViewCollection, filter))
			{
				var args1 = new ColourDecidingEventArgs(pickViewCollection.Single(pf => pf.WPV_WL == location1.PK));
				control.FilteredPickFaces_ColorDeciding(this, args1);
				AssertEquals(Color.LightGreen, args1.Colour);

				var args2 = new ColourDecidingEventArgs(pickViewCollection.Single(pf => pf.WPV_WL == location2.PK));
				control.FilteredPickFaces_ColorDeciding(this, args2);
				AssertEquals(Color.DeepSkyBlue, args2.Colour);

				var args3 = new ColourDecidingEventArgs(pickViewCollection.Single(pf => pf.WPV_WL == location3.PK));
				control.FilteredPickFaces_ColorDeciding(this, args3);
				AssertEquals(Color.Orange, args3.Colour);
			}
		}

		#endregion
	}

	class PickFacesFilterControlDbHitsTest : WhsEnvFilterControlDBHitsTestCase<WhsPickFaceViewCollection, PickFacesFilterBusinessObject>
	{
		protected override Dictionary<string, int> GetBaseHits()
		{
			var dict = new Dictionary<string, int>();
			dict.Add(WhsPickFaceViewSchema.Constants.TableName, 1);
			return dict;
		}

		protected override bool ShouldCheckForUnusedFetchHints(string columnName) => false;

		protected override Dictionary<string, Dictionary<string, int>> GetExpectedHitsDictionary()
		{
			var dict = new Dictionary<string, Dictionary<string, int>>();

			AddHits(dict, WhsPickFaceViewSchema.Constants.WPV_WW_Whs, WhsWarehouseSchema.Constants.TableName);
			AddHits(dict, WhsPickFaceViewSchema.Constants.WPV_OH, OrgHeaderSchema.Constants.TableName);
			AddHits(dict, WhsPickFaceViewSchema.Constants.WPV_OP, OrgSupplierPartSchema.Constants.TableName);
			AddHits(dict, WhsPickFaceViewSchema.Constants.WPV_WL, WhsLocationViewSchema.Constants.TableName);
			AddHits(dict, "SupplierPart+" + OrgSupplierPartSchema.Constants.OP_Desc, OrgSupplierPartSchema.Constants.TableName);

			return dict;
		}

		void AddHits(Dictionary<string, Dictionary<string, int>> dict, string column, string tableName)
		{
			var result = new Dictionary<string, int>(GetBaseHits());
			result.Add(tableName, 1);
			dict.Add(column, result);
		}

		protected override WhsPickFaceViewCollection GetNewCollection(BusinessObjectFactory factory) => new WhsPickFaceViewCollection(factory);
		protected override PickFacesFilterBusinessObject GetNewFilterBusinessObject() => new PickFacesFilterBusinessObject();
		protected override ZFilterStripControl GetNewFilterControl(WhsPickFaceViewCollection collection, PickFacesFilterBusinessObject filterBizO) => new PickFacesFilterControl(collection, filterBizO);

		protected override void SetupData()
		{
			const int warehouses = 3;
			const int locationsPerWhs = 3;
			const int unassignedLocationsPerWhs = 1;
			const int clients = 10;
			Assert("Precondition", unassignedLocationsPerWhs < locationsPerWhs);

			var data = new PickFaceViewTestData(Factory, warehouses, clients, locationsPerWhs);

			var i = 1;
			foreach (var whs in data.Warehouses)
			{
				foreach (var loc in data.Locations[whs].Skip(unassignedLocationsPerWhs))
				{
					foreach (var client in data.Clients)
					{
						foreach (var part in data.Parts)
						{
							Helper.CreateProductPickFace(part, client, loc, i++);
						}
					}
				}
			}

			Factory.Save();
		}
	}
}
