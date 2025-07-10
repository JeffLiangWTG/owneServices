using System.Linq;
using CargoWise.Definitions;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Packing.Business.Testing
{
	public class PkgPackageDocumentSupporterTest : PackingTestCaseWithFactory
	{
		#region TestBusinessContext

		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.Package, PackageDocumentSupporter.BusinessContext);
		}

		#endregion

		#region TestCustomisationSecurityCheckpoint

		public void TestCustomisationSecurityCheckpoint()
		{
			AssertEquals(Env.Security.None, PackageDocumentSupporter.CustomisationSecurityCheckpoint);
		}

		#endregion

		#region TestGetDocumentWrappers_ProductLabelAll

		public void TestGetDocumentWrappers_ProductLabelAll()
		{
			Data.CreatePackingData();
			var order1OuterPackWith0 = Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);

			var order1OuterPackWith1 = Helper.CreatePackage(Data.PackageJob, "B2", 1, Constants.PkgUnit.Box);
			order1OuterPackWith1.Pack(Data.DummyLine1, 1m);
			order1OuterPackWith1.Pack(Data.DummyLine1, 1m); // pack twice to create two divots to the same item.

			var order1OuterPackWith2 = Helper.CreatePackage(Data.PackageJob, "B3", 1, Constants.PkgUnit.Box);
			order1OuterPackWith2.Pack(Data.DummyLine2, 1);
			order1OuterPackWith2.Pack(Data.DummyLine3, 1);
			order1OuterPackWith2.Pack(Data.DummyLine3, 1); // pack twice to create two divots to the same item.

			var order1InnerPackWith3 = Helper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);
			order1InnerPackWith3.Pack(Data.DummyLine2, 1);
			order1InnerPackWith3.Pack(Data.DummyLine3, 1);

			var order1InnerInnerPackWith1 = Helper.CreatePackage(order1InnerPackWith3, 1, Constants.PkgUnit.Box, "B5");
			order1InnerInnerPackWith1.Pack(Data.DummyLine1, 1);

			var order1OuterPackWith0DocumentSupporter = ((IDocumentSupportable)order1OuterPackWith0).DocumentSupporter;
			var wrappers = order1OuterPackWith0DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("All packages with Products and Ids cause none are selected", 5, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
			wrappers = order1OuterPackWith0DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("Selected 1, so selected package and it's children", 3, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3, order1OuterPackWith2 });
			wrappers = order1OuterPackWith0DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("Selected multiple, so selected package and it's children", 4, wrappers.Length);
		}

		#endregion

		#region TestGetDocumentWrappers_ProductLabel

		public void TestGetDocumentWrappers_ProductLabel()
		{
			Data.CreatePackingData();
			var order1OuterPackWith0 = Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);

			var order1OuterPackWith1 = Helper.CreatePackage(Data.PackageJob, "B2", 1, Constants.PkgUnit.Box);
			order1OuterPackWith1.Pack(Data.DummyLine1, 1m);
			order1OuterPackWith1.Pack(Data.DummyLine1, 1m); // pack twice to create two divots to the same item.

			var order1OuterPackWith2 = Helper.CreatePackage(Data.PackageJob, "B3", 1, Constants.PkgUnit.Box);
			order1OuterPackWith2.Pack(Data.DummyLine2, 1);
			order1OuterPackWith2.Pack(Data.DummyLine3, 1);
			order1OuterPackWith2.Pack(Data.DummyLine3, 1); // pack twice to create two divots to the same item.

			var order1InnerPackWith3 = Helper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);
			order1InnerPackWith3.Pack(Data.DummyLine2, 1);
			order1InnerPackWith3.Pack(Data.DummyLine3, 1);

			var order1InnerInnerPackWith1 = Helper.CreatePackage(order1InnerPackWith3, 1, Constants.PkgUnit.Box, "B5");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);

			var order1InnerPackWith3DS = ((IDocumentSupportable)order1InnerPackWith3).DocumentSupporter;
			var wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericProductLabel, Factory.New<StmMenuItem>());
			AssertEquals("Only top level with Products and IDs", 2, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
			wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericProductLabel, Factory.New<StmMenuItem>());
			AssertEquals("Selected 1, so just selected package, has 3 products, so split across 2 labels", 2, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1OuterPackWith1, order1InnerPackWith3 });
			wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericProductLabel, Factory.New<StmMenuItem>());
			AssertEquals("Selected multiple, so just selected package", 3, wrappers.Length);
		}

		#endregion

		#region TestGetDocumentWrappers_BasicLabelAll

		public void TestGetDocumentWrappers_BasicLabelAll()
		{
			// packSelection = isPackageSelected ? PackSelection.Selected : PackSelection.All;
			// packLevel = PackLevel.All;

			Data.CreatePackingData();
			var order1OuterPackWith0 = Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);

			var order1OuterPackWith1 = Helper.CreatePackage(Data.PackageJob, "B2", 1, Constants.PkgUnit.Box);
			order1OuterPackWith1.Pack(Data.DummyLine1, 1m);

			var order1OuterPackWith2 = Helper.CreatePackage(Data.PackageJob, "B3", 1, Constants.PkgUnit.Box);
			order1OuterPackWith2.Pack(Data.DummyLine2, 1);
			order1OuterPackWith2.Pack(Data.DummyLine3, 1);

			var order1InnerPackWith3 = Helper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);
			order1InnerPackWith3.Pack(Data.DummyLine2, 1);
			order1InnerPackWith3.Pack(Data.DummyLine3, 1);

			var order1InnerInnerPackWith1 = Helper.CreatePackage(order1InnerPackWith3, 1, Constants.PkgUnit.Box, "B5");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);

			var order1InnerPackWith3DS = ((IDocumentSupportable)order1InnerPackWith3).DocumentSupporter;
			var wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericBasicLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("No Selected, so all packs on all levels", 5, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
			wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericBasicLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("Selected 1, so selected package and it's children", 2, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1OuterPackWith0, order1InnerPackWith3 });
			wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericBasicLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("Selected multiple, so selected package and it's children", 3, wrappers.Length);
		}

		#endregion

		#region TestGetDocumentWrappers_BasicLabel

		public void TestGetDocumentWrappers_BasicLabel()
		{
			// packSelection = PackSelection.All;
			// packLevel = isPackageSelected ? PackLevel.First : PackLevel.Second;

			Data.CreatePackingData();
			var order1OuterPackWith0 = Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);

			var order1OuterPackWith1 = Helper.CreatePackage(Data.PackageJob, "B2", 1, Constants.PkgUnit.Box);
			order1OuterPackWith1.Pack(Data.DummyLine1, 1m);

			var order1OuterPackWith2 = Helper.CreatePackage(Data.PackageJob, "B3", 1, Constants.PkgUnit.Box);
			order1OuterPackWith2.Pack(Data.DummyLine2, 1);
			order1OuterPackWith2.Pack(Data.DummyLine3, 1);

			var order1InnerPackWith3 = Helper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);
			order1InnerPackWith3.Pack(Data.DummyLine2, 1);
			order1InnerPackWith3.Pack(Data.DummyLine3, 1);

			var order1InnerInnerPackWith1 = Helper.CreatePackage(order1InnerPackWith3, 1, Constants.PkgUnit.Box, "B4");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);

			var order1InnerPackWith3DS = ((IDocumentSupportable)order1InnerPackWith3).DocumentSupporter;
			var wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericBasicLabel, Factory.New<StmMenuItem>());
			AssertEquals("No Selected, so just first 2 levels", 4, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
			wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericBasicLabel, Factory.New<StmMenuItem>());
			AssertEquals("Selected 1, so just selected package", 1, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1OuterPackWith0, order1InnerPackWith3 });
			wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericBasicLabel, Factory.New<StmMenuItem>());
			AssertEquals("Selected multiple, so just selected", 2, wrappers.Length);
		}

		#endregion

		#region TestGetDocumentWrappers_BasicLabel_CallFromWebService

		public void TestGetDocumentWrappers_BasicLabel_CallFromWebService_FromWeb()
		{
			var initialValue = Globals.IsWeb;
			Globals.IsWeb = true;
			try
			{
				TestGetDocumentWrappers_BasicLabel_CallFromWebServiceCore();
			}
			finally
			{
				Globals.IsWeb = initialValue;
			}
		}

		public void TestGetDocumentWrappers_BasicLabel_CallFromWebServiceCore_FromWebService()
		{
			var initialValue = Globals.IsWebService;
			Globals.IsWebService = true;
			try
			{
				TestGetDocumentWrappers_BasicLabel_CallFromWebServiceCore();
			}
			finally
			{
				Globals.IsWebService = initialValue;
			}
		}

		void TestGetDocumentWrappers_BasicLabel_CallFromWebServiceCore()
		{
			Data.CreatePackingData();
			var order1OuterPackWith0 = Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);

			var order1OuterPackWith1 = Helper.CreatePackage(Data.PackageJob, "B2", 1, Constants.PkgUnit.Box);
			order1OuterPackWith1.Pack(Data.DummyLine1, 1m);

			var order1OuterPackWith2 = Helper.CreatePackage(Data.PackageJob, "B3", 1, Constants.PkgUnit.Box);
			order1OuterPackWith2.Pack(Data.DummyLine2, 1);
			order1OuterPackWith2.Pack(Data.DummyLine3, 1);

			var order1InnerPackWith3 = Helper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);
			order1InnerPackWith3.Pack(Data.DummyLine2, 1);
			order1InnerPackWith3.Pack(Data.DummyLine3, 1);

			var order1InnerPackWith3DS = ((IDocumentSupportable)order1InnerPackWith3).DocumentSupporter;
			var wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericBasicLabel, Factory.New<StmMenuItem>());
			AssertEquals("Should just get 1 document wrapper for current package.", 1, wrappers.Length);
		}

		#endregion

		#region TestGetDocumentWrappers_DeliveryLabelAll

		public void TestGetDocumentWrappers_DeliveryLabelAll()
		{
			Data.CreatePackingData();
			var order1OuterPackWith0 = Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);
			var order1OuterPackWith1 = Helper.CreatePackage(Data.PackageJob, "B2", 1, Constants.PkgUnit.Box);
			var order1OuterPackWith2 = Helper.CreatePackage(Data.PackageJob, "B3", 1, Constants.PkgUnit.Box);
			var order1InnerPackWith3 = Helper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			var order1InnerInnerPackWith1 = Helper.CreatePackage(order1InnerPackWith3, 1, Constants.PkgUnit.Box, "B5");

			var order1OuterPackWith0DocumentSupporter = ((IDocumentSupportable)order1OuterPackWith0).DocumentSupporter;
			var wrappers = order1OuterPackWith0DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericDeliveryLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("All packages with Products and Ids cause none are selected", 5, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
			wrappers = order1OuterPackWith0DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericDeliveryLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("Selected 1, so selected package and it's children", 2, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3, order1OuterPackWith2 });
			wrappers = order1OuterPackWith0DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericDeliveryLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("Selected multiple, so selected package and it's children", 3, wrappers.Length);
		}

		#endregion

		#region TestGetDocumentWrappers_DeliveryLabel

		public void TestGetDocumentWrappers_DeliveryLabel()
		{
			Data.CreatePackingData();
			var order1OuterPackWith0 = Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);
			var order1OuterPackWith1 = Helper.CreatePackage(Data.PackageJob, "B2", 1, Constants.PkgUnit.Box);
			var order1OuterPackWith2 = Helper.CreatePackage(Data.PackageJob, "B3", 1, Constants.PkgUnit.Box);
			var order1InnerPackWith3 = Helper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			var order1InnerInnerPackWith1 = Helper.CreatePackage(order1InnerPackWith3, 1, Constants.PkgUnit.Box, "B5");

			var order1InnerPackWith3DS = ((IDocumentSupportable)order1InnerPackWith3).DocumentSupporter;
			var wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericDeliveryLabel, Factory.New<StmMenuItem>());
			AssertEquals("None seleted, Only top level with Products and IDs", 3, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
			wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericDeliveryLabel, Factory.New<StmMenuItem>());
			AssertEquals("Selected 1, so just selected package, so 1 label", 1, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1OuterPackWith1, order1InnerPackWith3 });
			wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericDeliveryLabel, Factory.New<StmMenuItem>());
			AssertEquals("Selected multiple, so just selected package", 2, wrappers.Length);
		}

		#endregion

		#region TestGetDocumentWrappers_ProductDeliveryLabelAll

		public void TestGetDocumentWrappers_ProductDeliveryLabelAll()
		{
			Data.CreatePackingData();
			var order1OuterPackWithNoID = Helper.CreatePackage(Data.PackageJob, "", 1, Constants.PkgUnit.Box);

			var order1OuterPackWith0 = Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);

			var order1OuterPackWith1 = Helper.CreatePackage(Data.PackageJob, "B2", 1, Constants.PkgUnit.Box);
			order1OuterPackWith1.Pack(Data.DummyLine1, 1m);

			var order1OuterPackWith2 = Helper.CreatePackage(Data.PackageJob, "B3", 1, Constants.PkgUnit.Box);
			order1OuterPackWith2.Pack(Data.DummyLine2, 1);
			order1OuterPackWith2.Pack(Data.DummyLine3, 1);

			var order1InnerPackWith0 = Helper.CreatePackage(order1OuterPackWith2, 1, Constants.PkgUnit.Box, "B6");

			var order1InnerPackWith3 = Helper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);
			order1InnerPackWith3.Pack(Data.DummyLine2, 1);
			order1InnerPackWith3.Pack(Data.DummyLine3, 1);

			var order1InnerInnerPackWith1 = Helper.CreatePackage(order1InnerPackWith3, 1, Constants.PkgUnit.Box, "B5");
			order1InnerInnerPackWith1.Pack(Data.DummyLine1, 1);

			var order1OuterPackWith0DocumentSupporter = ((IDocumentSupportable)order1OuterPackWith0).DocumentSupporter;
			var wrappers = order1OuterPackWith0DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductDeliveryLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("All packages with Ids cause none are selected", 6, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
			wrappers = order1OuterPackWith0DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductDeliveryLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("Selected 1, so selected package and its children", 2, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3, order1OuterPackWith2 });
			wrappers = order1OuterPackWith0DocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductDeliveryLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("Selected multiple, so selected package and its children", 4, wrappers.Length);
		}

		#endregion

		#region TestGetDocumentWrappers_ProductDeliveryLabel

		public void TestGetDocumentWrappers_ProductDeliveryLabel()
		{
			Data.CreatePackingData();
			var order1OuterPackWithNoID = Helper.CreatePackage(Data.PackageJob, "", 1, Constants.PkgUnit.Box);

			var order1OuterPackWith0 = Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);

			var order1OuterPackWith1 = Helper.CreatePackage(Data.PackageJob, "B2", 1, Constants.PkgUnit.Box);
			order1OuterPackWith1.Pack(Data.DummyLine1, 1m);

			var order1OuterPackWith2 = Helper.CreatePackage(Data.PackageJob, "B3", 1, Constants.PkgUnit.Box);
			order1OuterPackWith2.Pack(Data.DummyLine2, 1);
			order1OuterPackWith2.Pack(Data.DummyLine3, 1);

			var order1InnerPackWith3 = Helper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);
			order1InnerPackWith3.Pack(Data.DummyLine2, 1);
			order1InnerPackWith3.Pack(Data.DummyLine3, 1);

			var order1InnerInnerPackWith1 = Helper.CreatePackage(order1InnerPackWith3, 1, Constants.PkgUnit.Box, "B5");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);

			var order1InnerPackWith3DS = ((IDocumentSupportable)order1InnerPackWith3).DocumentSupporter;
			var wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericProductDeliveryLabel, Factory.New<StmMenuItem>());
			AssertEquals("Only top level with IDs", 3, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
			wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericProductDeliveryLabel, Factory.New<StmMenuItem>());
			AssertEquals("Selected 1, so just selected package", 1, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1OuterPackWith1, order1InnerPackWith3 });
			wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericProductDeliveryLabel, Factory.New<StmMenuItem>());
			AssertEquals("Selected multiple, so just selected packages", 2, wrappers.Length);
		}

		#endregion

		#region TestGetDocumentWrappers_ForGetContactOrganisation

		public void TestGetDocumentWrappers_ForGetContactOrganisation()
		{
			var contact = PackageDocumentSupporter.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY);
			AssertEquals("OC_ContactName", "Fred", ((OrgHeader)contact.OrgHeader).Contacts[0].OC_ContactName);
			AssertEquals("OC_Mobile", "112233", ((OrgHeader)contact.OrgHeader).Contacts[0].OC_Mobile);
		}

		public void TestGetDocumentWrappers_ForGetContactOrganisationWithNoParent()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var package = packageJob.Packages.AddNew("BOX");
			var contact = ((IDocumentSupportable)package).DocumentSupporter.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY);
			AssertNull("Cannot get a contact because there is no parent", contact);
		}

		#endregion

		#region TestGetDocumentWrappers_RetailersLabelAll

		public void TestGetDocumentWrappers_RetailersLabelAll()
		{
			// packSelection = isPackageSelected ? PackSelection.Selected : PackSelection.All;
			// packLevel = PackLevel.All;

			Data.CreatePackingData();
			var order1OuterPackWith0 = Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);

			var order1OuterPackWith1 = Helper.CreatePackage(Data.PackageJob, "B2", 1, Constants.PkgUnit.Box);
			order1OuterPackWith1.Pack(Data.DummyLine1, 1m);

			var order1OuterPackWith2 = Helper.CreatePackage(Data.PackageJob, "B3", 1, Constants.PkgUnit.Box);
			order1OuterPackWith2.Pack(Data.DummyLine2, 1);
			order1OuterPackWith2.Pack(Data.DummyLine3, 1);

			var order1InnerPackWith3 = Helper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);
			order1InnerPackWith3.Pack(Data.DummyLine2, 1);
			order1InnerPackWith3.Pack(Data.DummyLine3, 1);

			var order1InnerInnerPackWith1 = Helper.CreatePackage(order1InnerPackWith3, 1, Constants.PkgUnit.Box, "B5");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);

			var order1InnerPackWith3DS = ((IDocumentSupportable)order1InnerPackWith3).DocumentSupporter;
			var wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericRetailersLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("No Selected, so all packs on all levels", 5, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
			wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericRetailersLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("Selected 1, so selected package and it's children", 2, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1OuterPackWith0, order1InnerPackWith3 });
			wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericRetailersLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("Selected multiple, so selected package and it's children", 3, wrappers.Length);
		}

		#endregion

		#region TestGetDocumentWrappers_RetailersLabel

		public void TestGetDocumentWrappers_RetailersLabel()
		{
			// packSelection = PackSelection.All;
			// packLevel = isPackageSelected ? PackLevel.First : PackLevel.Second;

			Data.CreatePackingData();
			var order1OuterPackWith0 = Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);

			var order1OuterPackWith1 = Helper.CreatePackage(Data.PackageJob, "B2", 1, Constants.PkgUnit.Box);
			order1OuterPackWith1.Pack(Data.DummyLine1, 1m);

			var order1OuterPackWith2 = Helper.CreatePackage(Data.PackageJob, "B3", 1, Constants.PkgUnit.Box);
			order1OuterPackWith2.Pack(Data.DummyLine2, 1);
			order1OuterPackWith2.Pack(Data.DummyLine3, 1);

			var order1InnerPackWith3 = Helper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);
			order1InnerPackWith3.Pack(Data.DummyLine2, 1);
			order1InnerPackWith3.Pack(Data.DummyLine3, 1);

			var order1InnerInnerPackWith1 = Helper.CreatePackage(order1InnerPackWith3, 1, Constants.PkgUnit.Box, "B4");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);

			var order1InnerPackWith3DS = ((IDocumentSupportable)order1InnerPackWith3).DocumentSupporter;
			var wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericRetailersLabel, Factory.New<StmMenuItem>());
			AssertEquals("Only top level with IDs", 3, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
			wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericRetailersLabel, Factory.New<StmMenuItem>());
			AssertEquals("Selected 1, so just selected package", 1, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1OuterPackWith1, order1InnerPackWith3 });
			wrappers = order1InnerPackWith3DS.GetDocumentWrappers(Constants.DataContext.GenericRetailersLabel, Factory.New<StmMenuItem>());
			AssertEquals("Selected multiple, so selected package and it's children", 2, wrappers.Length);
		}

		#endregion

		#region TestGetDocumentWrappers_ModuleSpecificLabel

		public void TestGetDocumentWrappers_ModuleSpecificLabel()
		{
			Data.CreatePackingData();
			var package1 = Helper.CreatePackage(Data.PackageJob, "FilterMe", 1, Constants.PkgUnit.Box);
			package1.Pack(Data.DummyLine1, 1);

			var package2 = Helper.CreatePackage(Data.PackageJob, "NoFilter", 1, Constants.PkgUnit.Box);
			package2.Pack(Data.DummyLine2, 1);

			var package3 = Helper.CreatePackage(Data.PackageJob, "Other", 1, Constants.PkgUnit.Box);
			package3.Pack(Data.DummyLine3, 1);

			var packDocSupporter = ((IDocumentSupportable)package1).DocumentSupporter;
			var wrappers = packDocSupporter.GetDocumentWrappers(Constants.DataContext.GenericAuditVarianceLabel, Factory.New<StmMenuItem>());
			AssertEquals("Current functionality for all data contexts: When no package is selected, it should return all packages without 'FilterMe'.", 2, wrappers.Length);
			AssertContainsExactElementsInAnyOrder("Since no packages were selected, all packages without 'FilterMe' were expected.", new PkgPackageJob[] { package2.PackageJob, package3.PackageJob }, wrappers.Select(w => w.WrappedObject));

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { package2 });
			packDocSupporter = ((IDocumentSupportable)package2).DocumentSupporter;
			wrappers = packDocSupporter.GetDocumentWrappers(Constants.DataContext.GenericAuditVarianceLabel, Factory.New<StmMenuItem>());
			AssertEquals("The package is returned since it is not a 'FilterMe' package.", 1, wrappers.Length);
			AssertContainsExactElementsInAnyOrder("Selected package2, so return just that one package without 'FilterMe'.", package2.PackageJob, wrappers.Select(w => w.WrappedObject));
		}

		#endregion

		#region TestGetDocumentWrappers_ModuleSpecificLabelAll

		public void TestGetDocumentWrappers_ModuleSpecificLabelAll()
		{
			Data.CreatePackingData();
			var package1 = Helper.CreatePackage(Data.PackageJob, "FilterMe1", 1, Constants.PkgUnit.Box);
			package1.Pack(Data.DummyLine1, 1);

			var package2 = Helper.CreatePackage(Data.PackageJob, "NoFilter", 1, Constants.PkgUnit.Box);
			package2.Pack(Data.DummyLine2, 1);

			var package3 = Helper.CreatePackage(Data.PackageJob, "Other", 1, Constants.PkgUnit.Box);
			package3.Pack(Data.DummyLine3, 1);

			var packDocSupporter = ((IDocumentSupportable)package1).DocumentSupporter;
			var wrappers = packDocSupporter.GetDocumentWrappers(Constants.DataContext.GenericAuditVarianceLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("Current functionality for all data contexts: When no package was selected, it should return all packages without 'FilterMe'.", 2, wrappers.Length);
			AssertContainsExactElementsInAnyOrder("Since no packages were selected, all packages without 'FilterMe' were expected.", new PkgPackageJob[] { package2.PackageJob, package3.PackageJob }, wrappers.Select(w => w.WrappedObject));

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { package2 });
			packDocSupporter = ((IDocumentSupportable)package2).DocumentSupporter;
			wrappers = packDocSupporter.GetDocumentWrappers(Constants.DataContext.GenericAuditVarianceLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("Current functionality for all data contexts: The selected package is returned since it is not a 'FilterMe' package.", 1, wrappers.Length);
			AssertContainsExactElementsInAnyOrder("Selected package2, so return just that one package without 'FilterMe'.", package2.PackageJob, wrappers.Select(w => w.WrappedObject));
		}

		#endregion

		#region TestGetDocumentWrappers_GetBODocDataProvidersNotFoundMessage

		public void TestGetDocumentWrappers_GetBODocDataProvidersNotFoundMessage()
		{
			Data.CreatePackingData();
			var message = Data.PackageJobDocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue("GenericAuditVarianceLabel"), Factory.New<StmMenuItem>());
			AssertEquals("Must show a Not Found message", "Not Found!", message);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_NoPackageID_BasicLabel_ParentIsWhsItemReceiveConsignment()
		{
			Data.CreatePackingData();
			var package = Helper.CreatePackage(Data.PackageJob, 1, Constants.PkgUnit.Box);
			package.PackageJob.KJ_ParentTableCode = "WRC";
			var supporter = new PkgPackageDocumentSupporter(package);

			var message = supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue("GenericBasicLabel"), Factory.New<StmMenuItem>());
			AssertEquals("Package ID is Required.", message);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_NoPackageID_BasicLabel_ParentIsNotWhsItemReceiveConsignment()
		{
			Data.CreatePackingData();
			var package = Helper.CreatePackage(Data.PackageJob, 1, Constants.PkgUnit.Box);
			package.PackageJob.KJ_ParentTableCode = "WDC";
			var supporter = new PkgPackageDocumentSupporter(package);

			var message = supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue("GenericBasicLabel"), Factory.New<StmMenuItem>());
			AssertEquals(string.Empty, message);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_NoPackageID_NotBasicLabel_ParentIsWhsItemReceiveConsignment()
		{
			Data.CreatePackingData();
			var package = Helper.CreatePackage(Data.PackageJob, 1, Constants.PkgUnit.Box);
			package.PackageJob.KJ_ParentTableCode = "WRC";
			var supporter = new PkgPackageDocumentSupporter(package);

			var message = supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue("GenericAuditVarianceLabel"), Factory.New<StmMenuItem>());
			AssertEquals(string.Empty, message);
		}

		public void TestGetBODocDataProvidersNotFoundMessage_HasPackageID_BasicLabel_ParentIsWhsItemReceiveConsignment()
		{
			Data.CreatePackingData();
			var package = Helper.CreatePackage(Data.PackageJob, "P1", 1, Constants.PkgUnit.Box);
			package.PackageJob.KJ_ParentTableCode = "WRC";
			var supporter = new PkgPackageDocumentSupporter(package);

			var message = supporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue("GenericBasicLabel"), Factory.New<StmMenuItem>());
			AssertEquals(string.Empty, message);
		}

		#endregion

		#region TestDocumentPrinted_IsLabelPrinted

		public void TestDocumentPrinted_IsLabelPrinted()
		{
			Data.CreatePackingData();
			var package1 = Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);
			var package2 = Helper.CreatePackage(Data.PackageJob, "B2", 1, Constants.PkgUnit.Box);

			var supporter1 = new PkgPackageDocumentSupporter(package1);
			var supporter2 = new PkgPackageDocumentSupporter(package2);
			var eventSource = new MockDocumentEventSource();
			supporter1.Initialise(eventSource);

			AssertEquals("Precondition", false, package1.IsLabelPrinted);
			var queryDeliveryLabel = new DocumentZQuery(BusinessContext.Packing, "Delivery Label (Top Level)");
			var deliveryLabelCommand = Factory.LoadTop1<DocumentCommand>(queryDeliveryLabel);
			var argsDeliveryLabel = new DocumentPrintedEventArgs(DeliveryInstructionDestination.None, deliveryLabelCommand);
			eventSource.FireDocumentPrinted(argsDeliveryLabel);
			AssertEquals(true, package1.IsLabelPrinted);

			supporter2.Initialise(eventSource);

			AssertEquals("Precondition", false, package2.IsLabelPrinted);
			var queryBasicLabel = new DocumentZQuery(BusinessContext.Packing, "Basic Label (All Packages)");
			var basicLabelCommand = Factory.LoadTop1<DocumentCommand>(queryBasicLabel);
			var argsBasicLabel = new DocumentPrintedEventArgs(DeliveryInstructionDestination.None, basicLabelCommand);
			eventSource.FireDocumentPrinted(argsBasicLabel);
			AssertEquals(true, package2.IsLabelPrinted);
		}

		#region class MockDocumentEventSource

		class MockDocumentEventSource : IDocumentEvents
		{
			public event DocumentCancelEventHandler DocumentPrintRequested;
			public event DocumentPrintedEventHandler DocumentPrePreviewed;
			public event DocumentPrintedEventHandler DocumentPrePrinted;
			public event DocumentPrintedEventHandler DocumentPrinted;

			public void FireDocumentPrintRequested(DocumentCancelEventArgs e)
			{
				DocumentPrintRequested(this, e);
			}

			public void FireDocumentPrePreviewed(DocumentPrintedEventArgs e)
			{
				DocumentPrePreviewed(this, e);
			}

			public void FireDocumentPrePrinted(DocumentPrintedEventArgs e)
			{
				DocumentPrePrinted(this, e);
			}

			public void FireDocumentPrinted(DocumentPrintedEventArgs e)
			{
				DocumentPrinted(this, e);
			}
		}

		#endregion

		#endregion

		#region TestGetDataStateBeforeRunForPkgPackageWithoutID

		public void TestGetDataStateBeforeRunForPkgPackageWithoutID_BasicLabel()
			=> TestGetDataStateBeforeRunForPkgPackageWithoutID("Basic Label");

		public void TestGetDataStateBeforeRunForPkgPackageWithoutID_DetailedLabel()
			=> TestGetDataStateBeforeRunForPkgPackageWithoutID("Detailed Label");

		public void TestGetDataStateBeforeRunForPkgPackageWithoutID_PackageManifest()
			=> TestGetDataStateBeforeRunForPkgPackageWithoutID("Package Manifest");

		void TestGetDataStateBeforeRunForPkgPackageWithoutID(string menuName)
		{
			var commandFilter = new DocumentZQuery("Package", menuName);
			var command = Factory.LoadTop1<DocumentCommand>(commandFilter);

			var result = PackageDocumentSupporter.GetDataStateBeforeRun(command);
			if (menuName == "Basic Label" || menuName == "Detailed Label")
			{
				AssertEquals("Not valid as the package has no id", false, result.IsValid);
				AssertEquals("Should pop proper message", "Package ID is Required.", result.ErrorMessage);
			}
			else
			{
				AssertEquals("Valid as the menu item doesn't need to check package id", true, result.IsValid);
			}

			var packageWithID = Data.PackageJob.Packages.AddNew("PKG", "P1");
			var documentSupporterOfPackageWithID = (PkgPackageDocumentSupporter)((IDocumentSupportable)packageWithID).DocumentSupporter;
			var result2 = documentSupporterOfPackageWithID.GetDataStateBeforeRun(command);
			AssertEquals("Valid as the package has id", true, result2.IsValid);
		}

		#endregion

		#region PackageDocumentSupporter

		PkgPackageDocumentSupporter PackageDocumentSupporter
		{
			get
			{
				if (packageDocumentSupporter == null)
				{
					Data.CreatePackingData();
					var package = Data.PackageJob.Packages.AddNew("BOX");
					package.Pack(Data.DummyLine1, 1m);
					package.Pack(Data.DummyLine2, 2m);
					packageDocumentSupporter = (PkgPackageDocumentSupporter)((IDocumentSupportable)package).DocumentSupporter;
				}
				return packageDocumentSupporter;
			}
		}
		PkgPackageDocumentSupporter packageDocumentSupporter;

		#endregion
	}
}
