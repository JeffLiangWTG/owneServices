using System;
using System.Collections.Generic;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.GUI;
using Enterprise.DocumentEngine.GUI.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	[HttpContextEnabledTest]
	sealed class LinerAndAgencyBillOfLadingDocMenuHelperTest : DocumentsMenuHelperTest
	{
		#region TestConstructor

		public void TestConstructor()
		{
			TestHelper testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);

			AssertConstructorByPK(typeof(LinerAndAgencyBillOfLadingDocMenuHelper));
		}

		#endregion

		#region TestIDocumentsMenuHelper

		public void TestIDocumentsMenuHelper()
		{
			AssertEquals("PKForBizOCreation should be equal to BillOfLading.PK", BillOfLading.PK, Helper.PKForBizOCreation);

			var testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);

			using (AgencyRegistry.Instance.UseNewFormBuilderBillOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var menuItems = Helper.GetAvailableDocuments();
				var menuItem = FindDocumentsMenuItem(menuItems, "Bill of Lading");

				AssertEquals(1, menuItems.Count);
				AssertNotNull("Bill Of Lading document is expected", menuItem);
				AssertEquals(Core.Constants.DocumentEngine.MenuPaths.LegacyDocuments + "/Export", menuItem.DocumentCommand.SU_MenuPath);
			}

			using (AgencyRegistry.Instance.UseNewFormBuilderBillOfLading.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var menuItems = Helper.GetAvailableDocuments();
				var menuItem = FindDocumentsMenuItem(menuItems, "Bill of Lading");

				AssertEquals(1, menuItems.Count);
				AssertNotNull("Bill Of Lading document is expected", menuItem);
				AssertEquals("Export", menuItem.DocumentCommand.SU_MenuPath);
			}
		}

		public void TestSetupDocumentPack()
		{
			DocumentPack pack = null;
			DocumentCommand command = DocumentCommand.GetDocumentCommand(Factory, BillOfLading, "Bill of Lading");
			if (command != null)
			{
				pack = new DocumentPack(command, BillOfLading, new DocumentEngine.RuntimeOptions.UserControlProviderList(), null);
			}

			AssertNotNull("Document Pack should be created", pack);
			Assert("Should be at least two pages", pack.Count >= 2);
			AssertEquals("First should be ORIGINAL", "ORIGINAL", pack[0].Name);
			AssertEquals("Second should be COPY", "COPY", pack[1].Name);

			Helper.SetupDocumentPack(pack);

			AssertEquals("Should contain 1 page", 1, pack.Count);
			AssertEquals("It should be COPY", "COPY", pack[0].Name);
			AssertEquals("DeliveryInstructions.IsDraft should be True", true, pack.DeliveryInstructions.IsDraft);

			pack = new DocumentPack(command, BillOfLading, new DocumentEngine.RuntimeOptions.UserControlProviderList(), null);
			AssertNotNull("Document Pack should be created", pack);
			Assert("Should be at least two pages", pack.Count >= 2);
			AssertEquals("First should be ORIGINAL", "ORIGINAL", pack[0].Name);
			IDeliverable original = pack[0];
			pack.RemoveAll();
			pack.Add(original);

			Helper.SetupDocumentPack(pack);

			AssertEquals("Should be no pages", 0, pack.Count);
			AssertEquals("DeliveryInstructions.IsDraft should be True", true, pack.DeliveryInstructions.IsDraft);
		}

		#endregion

		#region Implementation

		DocumentsMenuItem FindDocumentsMenuItem(List<DocumentsMenuItem> menuItems, string name)
		{
			return menuItems.Find(delegate(DocumentsMenuItem match)
			{
				return match.DocumentCommand.SU_MenuName == name;
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			BillOfLading = Factory.NewWithValidTestData<BillOfLading>();
			Helper = new LinerAndAgencyBillOfLadingDocMenuHelper(BillOfLading);
		}

		BillOfLading BillOfLading;
		DocumentsMenuHelper Helper;

		#endregion
	}
}
