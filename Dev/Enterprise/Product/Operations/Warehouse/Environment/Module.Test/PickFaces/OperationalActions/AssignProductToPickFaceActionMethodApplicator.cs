using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(AssignProductToPickFaceActionMethodApplicator))]
	public class AssignProductToPickFaceActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestApplyApplicator

		public void TestApplyApplicator_Success()
		{
			TestApplyApplicator_Success(p => { }, shouldSetClient: true);
		}

		public void TestApplyApplicator_Success_BothRelationship()
		{
			TestApplyApplicator_Success(
				p =>
				{
					var relationship = (OrgPartRelation)p.RelatedOrganisations.Single();
					relationship.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
				}, shouldSetClient: true);
		}

		public void TestApplyApplicator_Success_WithSupplier()
		{
			TestApplyApplicator_Success(
				p =>
				{
					var supplier = new WhsTestHelperFunctionsEnv(Factory).CreateClient("SUP");
					var otherRelationship = p.RelatedOrganisations.AddSupplier(supplier);
				}, shouldSetClient: true);
		}

		public void TestApplyApplicator_Success_WithWarehouseConsignee()
		{
			TestApplyApplicator_Success(
				p =>
				{
					var otherOrg = new WhsTestHelperFunctionsEnv(Factory).CreateClient("WCN");
					var otherRelationship = p.RelatedOrganisations.AddSupplier(otherOrg);
					otherRelationship.OU_Relationship = OrgPartRelation.RelationshipTypes.WarehouseConsignee;
				}, shouldSetClient: true);
		}

		public void TestApplyApplicator_Success_MultipleOwners()
		{
			TestApplyApplicator_Success(
				p =>
				{
					var otherClient = new WhsTestHelperFunctionsEnv(Factory).CreateClient("CLT02");
					var otherRelationship = p.RelatedOrganisations.AddOwner(otherClient);
				}, shouldSetClient: false); // We can't know which one is correct to set
		}

		public void TestApplyApplicator_Success_MultipleOwners_BothRelationship()
		{
			TestApplyApplicator_Success(
				p =>
				{
					var otherClient = new WhsTestHelperFunctionsEnv(Factory).CreateClient("CLT02");
					var otherRelationship = p.RelatedOrganisations.AddOrganisationIfNotExist(otherClient.PK, OrgPartRelation.RelationshipTypes.Both);
				}, shouldSetClient: false); // We can't know which one is correct to set
		}

		void TestApplyApplicator_Success(Action<OrgSupplierPart> setupPart, bool shouldSetClient)
		{
			var data = new DataSetup(Factory);
			setupPart(data.Part);

			var pickFaceView = new BusinessObjectFactory().New<WhsPickFaceView>(); // we just need the location
			pickFaceView.WPV_WL = data.FirstAlphaLocation.PK;

			Applicator.ProductPK = data.Part.PK;

			var expectedLogText = new ZStringBuilder();
			expectedLogText.Append("INFO: Product form will open and populate pick faces. Please modify and then save the form\r\n");

			if (!shouldSetClient)
			{
				expectedLogText.Append("WARNING: Client was not defaulted as there are multiple owner relationships for this product");
			}

			ApplyApplicator(new BusinessObject[] { pickFaceView }, expectedLogText.ToString());

			var pickFace = new WhsPickFaceCollection(data.Part, Factory).FirstOrDefault();
			AssertNotNull("Should have created a pick face.", pickFace);
			AssertEquals("Should have defaulted the location.", data.FirstAlphaLocation.PK, pickFace.WF_WL);
			AssertEquals("Should have defaulted the product.", data.Part.PK, pickFace.WF_OP);
			AssertEquals("Should have defaulted the client if possible.", shouldSetClient ? data.Client.PK : ZGuid.Empty, pickFace.WF_OH_Client);

			// Simulate the user saving the form
			if (!shouldSetClient)
			{
				var zgrid1 = Applicator.ProductForm.SelectAndReturnPickFaceGrid();

				((WhsPickFace)zgrid1.List[0]).WF_OH_Client = data.Client.PK;
				((ICancelAddNew)zgrid1.List).EndNew(0);

				Application.DoEvents();
			}

			var saveResult = Applicator.ProductForm.FireSaveButton();
			Applicator.ProductForm.Dispose();

			AssertEquals(ContinueWithSave.Yes, saveResult);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var pickFaceCollection = new WhsPickFaceCollection(data.Part, factory2);
			var pickFaceExists = pickFaceCollection.ContainsPickFace(data.Client, data.FirstAlphaLocation);

			AssertEquals(true, pickFaceExists);
		}

		public void TestApplyApplicator_CloseForm()
		{
			var data = new DataSetup(Factory);

			var pickFaceView = new BusinessObjectFactory().New<WhsPickFaceView>(); // we just need the location
			pickFaceView.WPV_WL = data.FirstAlphaLocation.PK;

			Applicator.ProductPK = data.Part.PK;

			ApplyApplicator(new BusinessObject[] { pickFaceView }, "INFO: Product form will open and populate pick faces. Please modify and then save the form");

			var zgrid1 = Applicator.ProductForm.SelectAndReturnPickFaceGrid();

			Applicator.ProductForm.Close();
			Application.DoEvents();
			Applicator.ProductForm.Dispose();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var pickFaceCollection = new WhsPickFaceCollection(data.Part, factory2);
			bool pickFaceExists = pickFaceCollection.ContainsPickFace(data.Client, data.FirstAlphaLocation);

			AssertEquals(false, pickFaceExists);
		}

		public void TestApplyApplicator_InActiveProduct()
		{
			var data = new DataSetup(Factory);
			data.Part.OP_IsActive = false;
			Factory.Save();

			var pickFaceView = new BusinessObjectFactory().New<WhsPickFaceView>(); // we just need the location
			pickFaceView.WPV_WL = data.FirstAlphaLocation.PK;

			Applicator.ProductPK = data.Part.PK;

			var expectedLogText = new ZStringBuilder();
			expectedLogText.Append("INFO: Product form will open and populate pick faces. Please modify and then save the form\r\n");
			expectedLogText.Append("ERROR: The selected product is inactive");
			ApplyApplicator(new BusinessObject[] { pickFaceView }, expectedLogText.ToString());
		}

		#region TestApplyApplicator_Assigned

		public void TestApplyApplicator_Assigned_WithStock_ShouldPreventAssignment()
		{
			var data = new DataSetup(Factory);
			var pickFace = data.Helper.CreateProductPickFace(data.Part, data.Client, data.FirstAlphaLocation, 1);
			CreateInventory(data, "R1", data.Whs.PK, data.Client.PK, data.FirstAlphaLocation.PK, data.Part.PK, 10m);
			Factory.Save();

			var pickFaceView = Factory.LoadTop1<WhsPickFaceView>(new ZQuery());
			AssertEquals("Expected location to be assigned.", pickFace.PK, pickFaceView.WPV_WF);
			AssertEquals("Expected inventory in location.", 10m, pickFaceView.WPV_TotalQuantity);

			using (Applicator)
			{
				var expectedLogText = new ZStringBuilder();
				expectedLogText.Append("INFO: Product form will open and populate pick faces. Please modify and then save the form\r\n");
				expectedLogText.Append($"ERROR: A pick face was already assigned for that product in Location {data.FirstAlphaLocation.WLV_LocationString}.");

				Applicator.ProductPK = data.Part.PK;
				ApplyApplicator("Expected error when trying to assign pickface.", new BusinessObject[] { pickFaceView }, expectedLogText.ToString());
			}
		}

		public void TestApplyApplicator_Assigned_WithoutStock_ShouldPreventAssignment()
		{
			var data = new DataSetup(Factory);
			var pickFace = data.Helper.CreateProductPickFace(data.Part, data.Client, data.FirstAlphaLocation, 1);
			Factory.Save();

			var pickFaceView = Factory.LoadTop1<WhsPickFaceView>(new ZQuery());
			AssertEquals("Expected location to be assigned.", pickFace.PK, pickFaceView.WPV_WF);

			using (Applicator)
			{
				var expectedLogText = new ZStringBuilder();
				expectedLogText.Append("INFO: Product form will open and populate pick faces. Please modify and then save the form\r\n");
				expectedLogText.Append($"ERROR: A pick face was already assigned for that product in Location {data.FirstAlphaLocation.WLV_LocationString}.");

				Applicator.ProductPK = data.Part.PK;
				ApplyApplicator("Expected error when trying to assign pickface.", new BusinessObject[] { pickFaceView }, expectedLogText.ToString());
			}
		}

		#endregion

		#region TestApplyApplicator_UnassignedPickFace

		public void TestApplyApplicator_UnassignedPickFace_WithStock_ShouldPreventAssignment()
		{
			var data = new DataSetup(Factory);
			CreateInventory(data, "R1", data.Whs.PK, data.Client.PK, data.FirstAlphaLocation.PK, data.Part.PK, 10m);
			Factory.Save();

			var pickFaceView = Factory.LoadTop1<WhsPickFaceView>(new ZQuery());
			AssertEquals("Expected location to be un-assigned.", ZGuid.Empty, pickFaceView.WPV_WF);
			AssertEquals("Expected inventory in location.", 10m, pickFaceView.WPV_TotalQuantity);

			using (Applicator)
			{
				var expectedLogText = new ZStringBuilder();
				expectedLogText.Append("INFO: Product form will open and populate pick faces. Please modify and then save the form\r\n");
				expectedLogText.Append($"ERROR: Location {data.FirstAlphaLocation.WLV_LocationString} is not empty for that product and client, please clear the location before assigning it.");

				Applicator.ProductPK = data.Part.PK;
				ApplyApplicator("Expected error when trying to assign pickface.", new BusinessObject[] { pickFaceView }, expectedLogText.ToString());
			}
		}

		public void TestApplyApplicator_UnassignedPickFace_WithStock_TwoProducts()
		{
			var data = new DataSetup(Factory);
			var product1 = data.Part;
			var product2 = data.Helper.CreateProduct(data.Client, "P2");
			var location = data.FirstAlphaLocation;
			CreateInventory(data, "R1", data.Whs.PK, data.Client.PK, location.PK, product1.PK, 10m);
			CreateInventory(data, "R2", data.Whs.PK, data.Client.PK, location.PK, product2.PK, 20m);
			Factory.Save();

			var pickFaceView = Factory.Load<WhsPickFaceView>(new ZQuery());
			AssertEquals("Expected two lines to be shown: one for each product.", 2, pickFaceView.Length);
			AssertEquals("Expected location to be un-assigned.", 2, pickFaceView.Where(pf => pf.WPV_WF == ZGuid.Empty).Count());
			AssertEquals("Expected inventory in location for product 1.", 10m, pickFaceView.Single(pf => pf.WPV_OP == product1.PK).WPV_TotalQuantity);
			AssertEquals("Expected inventory in location for product 2.", 20m, pickFaceView.Single(pf => pf.WPV_OP == product2.PK).WPV_TotalQuantity);

			using (Applicator)
			{
				var expectedLogText = new ZStringBuilder();
				expectedLogText.Append("INFO: Product form will open and populate pick faces. Please modify and then save the form\r\n");
				expectedLogText.Append($"ERROR: Location {location.WLV_LocationString} is not empty for that product and client, please clear the location before assigning it.");

				Applicator.ProductPK = product2.PK;
				ApplyApplicator("Expected error when trying to assign pickface for product 2 from selected product 1.", new BusinessObject[] { pickFaceView.Single(pf => pf.WPV_OP == product1.PK) }, expectedLogText.ToString());
			}
		}

		public void TestApplyApplicator_UnassignedPickFace_WithStock_TwoLocations()
		{
			var data = new DataSetup(Factory);
			var product2 = data.Helper.CreateProduct(data.Client, "P2");
			var location2 = data.RowA.Locations[1];

			CreateInventory(data, "R1", data.Whs.PK, data.Client.PK, data.FirstAlphaLocation.PK, product2.PK, 10m);
			CreateInventory(data, "R2", data.Whs.PK, data.Client.PK, location2.PK, data.Part.PK, 5m);

			location2.WLV_WLT_LocationType = data.FixedLocationType.PK;
			Factory.Save();

			var pickFaceView = Factory.LoadTop1<WhsPickFaceView>(new ZQuery(WhsPickFaceViewSchema.WPV_OP, data.Part.PK));
			AssertEquals("Expected location.", location2.PK, pickFaceView.WPV_WL);
			AssertEquals("Expected location to be un-assigned.", ZGuid.Empty, pickFaceView.WPV_WF);
			AssertEquals("Expected inventory in selected location.", 5m, pickFaceView.WPV_TotalQuantity);

			using (Applicator)
			{
				var expectedLogText = new ZStringBuilder();
				expectedLogText.Append("INFO: Product form will open and populate pick faces. Please modify and then save the form\r\n");

				Applicator.ProductPK = product2.PK;
				ApplyApplicator("Expected no error when trying to assign pickface.", new BusinessObject[] { pickFaceView }, expectedLogText.ToString());
			}
		}

		public void TestApplyApplicator_UnassignedPickFace_WithStock_AddNewProductToLocation()
		{
			var data = new DataSetup(Factory);
			var product1 = data.Part;
			var product2 = data.Helper.CreateProduct(data.Client, "P2");
			var location = data.RowA.Locations[1];
			CreateInventory(data, "R1", data.Whs.PK, data.Client.PK, location.PK, product1.PK, 10m);
			location.WLV_WLT_LocationType = data.FixedLocationType.PK;
			Factory.Save();

			var pickFaceView = Factory.Load<WhsPickFaceView>(new ZQuery(WhsPickFaceViewSchema.WPV_OP, product1.PK)).Single();
			AssertEquals("Expected location.", location.PK, pickFaceView.WPV_WL);
			AssertEquals("Expected location to be un-assigned.", ZGuid.Empty, pickFaceView.WPV_WF);
			AssertEquals("Expected inventory in selected location.", 10m, pickFaceView.WPV_TotalQuantity);

			using (Applicator)
			{
				var expectedLogText = new ZStringBuilder();
				expectedLogText.Append("INFO: Product form will open and populate pick faces. Please modify and then save the form\r\n");

				Applicator.ProductPK = product2.PK;
				ApplyApplicator("Expected no errors when trying to assign pickface.", new BusinessObject[] { pickFaceView }, expectedLogText.ToString());
			}
		}

		public void TestApplyApplicator_UnassignedPickFace_WithoutStock_ShouldAllowAssignment()
		{
			var data = new DataSetup(Factory);
			Factory.Save();

			var pickFaceView = Factory.LoadTop1<WhsPickFaceView>(new ZQuery());
			AssertEquals("Expected location to be un-assigned.", ZGuid.Empty, pickFaceView.WPV_WF);
			AssertEquals("Expected no inventory in location.", 0m, pickFaceView.WPV_TotalQuantity);

			using (Applicator)
			{
				var expectedLogText = new ZStringBuilder();
				expectedLogText.Append("INFO: Product form will open and populate pick faces. Please modify and then save the form\r\n");

				Applicator.ProductPK = data.Part.PK;
				ApplyApplicator("Expected no errors when trying to assign pickface.", new BusinessObject[] { pickFaceView }, expectedLogText.ToString());
			}
		}

		#endregion

		#region TestApplyApplicator_MultipleSelection

		public void TestApplyApplicator_MultipleSelection_ShouldReportErrors()
		{
			var data = new DataSetup(Factory);
			var location1 = data.FirstAlphaLocation;
			var location2 = data.RowA.Locations[1];
			var location3 = data.RowA.Locations[2];

			var pickFace = data.Helper.CreateProductPickFace(data.Part, data.Client, location1, 1);
			CreateInventory(data, "R1", data.Whs.PK, data.Client.PK, location2.PK, data.Part.PK, 10m);
			location2.WLV_WLT_LocationType = data.FixedLocationType.PK;
			location3.WLV_WLT_LocationType = data.FixedLocationType.PK;
			Factory.Save();

			var pickFaceViews = Factory.Load<WhsPickFaceView>(new ZQuery());
			var pickFaceView1 = pickFaceViews.Where(pfv => pfv.WPV_WL == location1.PK).Single();
			var pickFaceView2 = pickFaceViews.Where(pfv => pfv.WPV_WL == location2.PK).Single();
			var pickFaceView3 = pickFaceViews.Where(pfv => pfv.WPV_WL == location3.PK).Single();
			AssertEquals("Expected three rows in pick face view.", 3, pickFaceViews.Length);
			AssertEquals("Expected location1 to be assigned.", pickFace.PK, pickFaceView1.WPV_WF);
			AssertEquals("Expected location2 to be un-assigned.", ZGuid.Empty, pickFaceView2.WPV_WF);
			AssertEquals("Expected location2 to have inventory.", 10m, pickFaceView2.WPV_TotalQuantity);
			AssertEquals("Expected location3 to be un-assigned.", ZGuid.Empty, pickFaceView3.WPV_WF);

			using (Applicator)
			{
				var expectedLogText = new ZStringBuilder();
				expectedLogText.Append("INFO: Product form will open and populate pick faces. Please modify and then save the form\r\n");
				expectedLogText.Append($"ERROR: A pick face was already assigned for that product in Location {location1.WLV_LocationString}.\r\n");
				expectedLogText.Append($"ERROR: Location {location2.WLV_LocationString} is not empty for that product and client, please clear the location before assigning it.");

				Applicator.ProductPK = data.Part.PK;
				ApplyApplicator("Expected errors to be reported when trying to assign pickface.", new BusinessObject[] { pickFaceView1, pickFaceView2, pickFaceView3 }, expectedLogText.ToString());
			}
		}

		#endregion

		#endregion

		#region TestHelpers

		void CreateInventory(DataSetup data, ZString docketID, ZGuid whs, ZGuid client, ZGuid location, ZGuid product, ZDecimal quantity)
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var notify = new TestNotificationBuffer();
			var receive = iHelper.CreateWhsReceive(client, whs, docketID, notify);
			iHelper.CreateWhsReceiveInventoryLine(receive, product, quantity, location);
		}

		#endregion

		#region DataSetup

		class DataSetup
		{
			public DataSetup(BusinessObjectFactory factory)
			{
				Helper = new WhsTestHelperFunctionsEnv(factory);

				Whs = Helper.CreateWarehouse("WHS");
				RowA = Helper.CreateRowAndGenerateLocations(Whs, "A", 5, 2);
				FixedLocationType = Helper.CreateLocationType("XYZ", "Test", false, 1, LocationClasses.Codes.FIX);

				Client = Helper.CreateClient("CLT01");
				Part = Helper.CreateProduct(Client, "P1");

				FirstAlphaLocation = RowA.Locations[0];
				FirstAlphaLocation.WLV_WLT_LocationType = FixedLocationType.PK; // fixed location

				factory.Save();
			}

			public WhsWarehouse Whs { get; }
			public WhsRow RowA { get; }
			public WhsLocationType FixedLocationType { get; }
			public OrgHeader Client { get; }
			public OrgSupplierPart Part { get; }
			public WhsLocation FirstAlphaLocation { get; }
			public WhsTestHelperFunctionsEnv Helper { get; }
		}

		#endregion

		new AssignProductToPickFaceActionMethodApplicator Applicator => (AssignProductToPickFaceActionMethodApplicator)base.Applicator;
	}
}
