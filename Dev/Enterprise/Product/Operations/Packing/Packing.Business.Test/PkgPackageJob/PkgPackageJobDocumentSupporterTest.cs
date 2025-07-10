using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Packing.Business.Testing
{
	public class PkgPackageJobDocumentSupporterTest : PackingTestCaseWithFactory
	{
		#region TestBusinessContext

		public void TestBusinessContext()
		{
			Data.CreatePackingData();
			AssertEquals(BusinessContext.Packing, Data.PackageJobDocumentSupporter.BusinessContext);
		}

		#endregion

		#region TestCustomisationSecurityCheckpoint

		public void TestCustomisationSecurityCheckpoint()
		{
			Data.CreatePackingData();
			AssertEquals(Env.Security.PkgPackageJobCustomizeDocuments, Data.PackageJobDocumentSupporter.CustomisationSecurityCheckpoint);
		}

		#endregion

		#region TestGetDocumentWrappers_ForGenericFreightWrapperContext

		public void TestGetDocumentWrappers_ForGenericFreightWrapperContext()
		{
			Data.CreatePackingData();
			var wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericFreightJob, null);
			AssertEquals("FreightWrapperFromPkgPackageJob", wrappers[0].GetType().Name);
		}

		#endregion

		#region TestGetDocumentWrappers_ProductLabelAll

		public void TestGetDocumentWrappers_ProductLabelAll()
		{
			Data.CreatePackingData();
			Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);

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

			var wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("All packages with Products and Ids", 5, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
			wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("Selected 1, so selected package and it's children, but it's not printed from BusinessConext.GenericPackage, so return all again", 5, wrappers.Length);
		}

		#endregion

		#region TestGetDocumentWrappers_ProductLabel

		public void TestGetDocumentWrappers_ProductLabel()
		{
			Data.CreatePackingData();
			Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);

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

			Helper.CreatePackage(order1InnerPackWith3, 1, Constants.PkgUnit.Box, "B5");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);

			var wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductLabel, Factory.New<StmMenuItem>());
			AssertEquals("Only top level with Products and IDs", 2, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
			wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductLabel, Factory.New<StmMenuItem>());
			AssertEquals("Selected 1, but not context package, so do all", 2, wrappers.Length);
		}

		#endregion

		#region TestGetDocumentWrappers_BasicLabelAll

		public void TestGetDocumentWrappers_BasicLabelAll()
		{
			// packSelection = isPackageSelected ? PackSelection.Selected : PackSelection.All;
			// packLevel = PackLevel.All;

			Data.CreatePackingData();
			Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);

			var order1OuterPackWith1 = Helper.CreatePackage(Data.PackageJob, "B2", 1, Constants.PkgUnit.Box);
			order1OuterPackWith1.Pack(Data.DummyLine1, 1m);

			var order1OuterPackWith2 = Helper.CreatePackage(Data.PackageJob, "B3", 1, Constants.PkgUnit.Box);
			order1OuterPackWith2.Pack(Data.DummyLine2, 1);
			order1OuterPackWith2.Pack(Data.DummyLine3, 1);

			var order1InnerPackWith3 = Helper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);
			order1InnerPackWith3.Pack(Data.DummyLine2, 1);
			order1InnerPackWith3.Pack(Data.DummyLine3, 1);

			Helper.CreatePackage(order1InnerPackWith3, 1, Constants.PkgUnit.Box, "B5");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);

			var wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericBasicLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("No Selected, so all packs on all levels", 5, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
			wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericBasicLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("Selected 1, but not context package, so do all", 5, wrappers.Length);
		}

		#endregion

		#region TestGetDocumentWrappers_BasicLabel

		public void TestGetDocumentWrappers_BasicLabel()
		{
			// packSelection = PackSelection.All;
			// packLevel = isPackageSelected ? PackLevel.First : PackLevel.Second;

			Data.CreatePackingData();
			Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);

			var order1OuterPackWith1 = Helper.CreatePackage(Data.PackageJob, "B2", 1, Constants.PkgUnit.Box);
			order1OuterPackWith1.Pack(Data.DummyLine1, 1m);

			var order1OuterPackWith2 = Helper.CreatePackage(Data.PackageJob, "B3", 1, Constants.PkgUnit.Box);
			order1OuterPackWith2.Pack(Data.DummyLine2, 1);
			order1OuterPackWith2.Pack(Data.DummyLine3, 1);

			var order1InnerPackWith3 = Helper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);
			order1InnerPackWith3.Pack(Data.DummyLine2, 1);
			order1InnerPackWith3.Pack(Data.DummyLine3, 1);

			Helper.CreatePackage(order1InnerPackWith3, 1, Constants.PkgUnit.Box, "B4");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);

			var wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericBasicLabel, Factory.New<StmMenuItem>());
			AssertEquals("No Selected, so just first 2 levels", 4, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
			wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericBasicLabel, Factory.New<StmMenuItem>());
			AssertEquals("Selected 1, but not context package, so do all", 4, wrappers.Length);
		}

		#endregion

		#region TestGetDocumentWrappers_DeliveryLabelAll

		public void TestGetDocumentWrappers_DeliveryLabelAll()
		{
			Data.CreatePackingData();
			Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);
			var order1OuterPackWith1 = Helper.CreatePackage(Data.PackageJob, "B2", 1, Constants.PkgUnit.Box);
			Helper.CreatePackage(Data.PackageJob, "B3", 1, Constants.PkgUnit.Box);
			var order1InnerPackWith3 = Helper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			Helper.CreatePackage(order1InnerPackWith3, 1, Constants.PkgUnit.Box, "B5");

			var wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericDeliveryLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("All packages with Products and Ids", 5, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
			wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericDeliveryLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("Selected 1, so selected package and it's children, but it's not printed from BusinessConext.GenericPackage, so return all again", 5, wrappers.Length);
		}

		#endregion

		#region TestGetDocumentWrappers_DeliveryLabel

		public void TestGetDocumentWrappers_DeliveryLabel()
		{
			Data.CreatePackingData();
			Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);
			var order1OuterPackWith1 = Helper.CreatePackage(Data.PackageJob, "B2", 1, Constants.PkgUnit.Box);
			Helper.CreatePackage(Data.PackageJob, "B3", 1, Constants.PkgUnit.Box);
			var order1InnerPackWith3 = Helper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			Helper.CreatePackage(order1InnerPackWith3, 1, Constants.PkgUnit.Box, "B5");

			var wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericDeliveryLabel, Factory.New<StmMenuItem>());
			AssertEquals("Only top level with Products and IDs", 3, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
			wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericDeliveryLabel, Factory.New<StmMenuItem>());
			AssertEquals("Selected 1, but not context package, so do all", 3, wrappers.Length);
		}

		#endregion

		#region TestGetDocumentWrappers_ProductDeliveryLabelAll

		public void TestGetDocumentWrappers_ProductDeliveryLabelAll()
		{
			Data.CreatePackingData();
			Helper.CreatePackage(Data.PackageJob, "", 1, Constants.PkgUnit.Box);

			Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);

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
			order1InnerInnerPackWith1.Pack(Data.DummyLine1, 1);

			var wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductDeliveryLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("All packages with Ids", 5, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
			wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductDeliveryLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("Selected 1, so selected package and its children, but it's not printed from BusinessConext.GenericPackage, so return all again", 5, wrappers.Length);
		}

		#endregion

		#region TestGetDocumentWrappers_ProductDeliveryLabel

		public void TestGetDocumentWrappers_ProductDeliveryLabel()
		{
			Data.CreatePackingData();
			Helper.CreatePackage(Data.PackageJob, "", 1, Constants.PkgUnit.Box);

			Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);

			var order1OuterPackWith1 = Helper.CreatePackage(Data.PackageJob, "B2", 1, Constants.PkgUnit.Box);
			order1OuterPackWith1.Pack(Data.DummyLine1, 1m);

			var order1OuterPackWith2 = Helper.CreatePackage(Data.PackageJob, "B3", 1, Constants.PkgUnit.Box);
			order1OuterPackWith2.Pack(Data.DummyLine2, 1);
			order1OuterPackWith2.Pack(Data.DummyLine3, 1);

			var order1InnerPackWith3 = Helper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);
			order1InnerPackWith3.Pack(Data.DummyLine2, 1);
			order1InnerPackWith3.Pack(Data.DummyLine3, 1);

			Helper.CreatePackage(order1InnerPackWith3, 1, Constants.PkgUnit.Box, "B5");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);

			var wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductDeliveryLabel, Factory.New<StmMenuItem>());
			AssertEquals("Only top level with IDs", 3, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
			wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericProductDeliveryLabel, Factory.New<StmMenuItem>());
			AssertEquals("Selected 1, but not context package, so do all", 3, wrappers.Length);
		}

		#endregion

		#region TestGetDocumentWrappers_RetailersLabelAll

		public void TestGetDocumentWrappers_RetailersLabelAll()
		{
			// packSelection = isPackageSelected ? PackSelection.Selected : PackSelection.All;
			// packLevel = PackLevel.All;

			Data.CreatePackingData();
			Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);

			var order1OuterPackWith1 = Helper.CreatePackage(Data.PackageJob, "B2", 1, Constants.PkgUnit.Box);
			order1OuterPackWith1.Pack(Data.DummyLine1, 1m);

			var order1OuterPackWith2 = Helper.CreatePackage(Data.PackageJob, "B3", 1, Constants.PkgUnit.Box);
			order1OuterPackWith2.Pack(Data.DummyLine2, 1);
			order1OuterPackWith2.Pack(Data.DummyLine3, 1);

			var order1InnerPackWith3 = Helper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);
			order1InnerPackWith3.Pack(Data.DummyLine2, 1);
			order1InnerPackWith3.Pack(Data.DummyLine3, 1);

			Helper.CreatePackage(order1InnerPackWith3, 1, Constants.PkgUnit.Box, "B5");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);

			var wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericRetailersLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("No Selected, so all packs on all levels", 5, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
			wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericRetailersLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("Selected 1, but not context package, so do all", 5, wrappers.Length);
		}

		#endregion

		#region TestGetDocumentWrappers_RetailersLabel

		public void TestGetDocumentWrappers_RetailersLabel()
		{
			// packSelection = PackSelection.All;
			// packLevel = isPackageSelected ? PackLevel.First : PackLevel.Second;

			Data.CreatePackingData();
			Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);

			var order1OuterPackWith1 = Helper.CreatePackage(Data.PackageJob, "B2", 1, Constants.PkgUnit.Box);
			order1OuterPackWith1.Pack(Data.DummyLine1, 1m);

			var order1OuterPackWith2 = Helper.CreatePackage(Data.PackageJob, "B3", 1, Constants.PkgUnit.Box);
			order1OuterPackWith2.Pack(Data.DummyLine2, 1);
			order1OuterPackWith2.Pack(Data.DummyLine3, 1);

			var order1InnerPackWith3 = Helper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);
			order1InnerPackWith3.Pack(Data.DummyLine2, 1);
			order1InnerPackWith3.Pack(Data.DummyLine3, 1);

			Helper.CreatePackage(order1InnerPackWith3, 1, Constants.PkgUnit.Box, "B4");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);

			var wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericRetailersLabel, Factory.New<StmMenuItem>());
			AssertEquals("Only top level with IDs", 3, wrappers.Length);

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
			wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericRetailersLabel, Factory.New<StmMenuItem>());
			AssertEquals("Selected 1, but not context package, so do all", 3, wrappers.Length);
		}

		#region TestGetDocumentWrappers_CarrierLabel

		public void TestGetDocumentWrappers_CarrierLabel()
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

			var order1InnerInnerPackWith1 = Helper.CreatePackage(order1InnerPackWith3, 1, Constants.PkgUnit.Box, "B4");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);

			var order1InnerPackWith3DS = ((IDocumentSupportable)order1InnerPackWith3).DocumentSupporter;
			var contact = order1InnerPackWith3DS.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY);
			if (contact != null && contact.OrgHeader != null)
			{
				var serviceLevel = ((OrgHeader)contact.OrgHeader).MiscServ.CarrierServiceLevels.Select(s => s.PL_Code).First();
				var dummyParent = PkgPackageJob.LoadParent<DummyWithPacking>(Data.PackageJob);
				dummyParent.SetCarrierServiceLevelCode(serviceLevel);
				dummyParent.CarrierForTest = contact.OrgHeader as OrgHeader;

				var wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericCarrierLabel, Factory.New<StmMenuItem>());
				AssertEquals("Only top level with IDs", 3, wrappers.Length);

				Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
				wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericCarrierLabel, Factory.New<StmMenuItem>());
				AssertEquals("Selected 1, but not context package, so do all", 3, wrappers.Length);
			}
		}

		#endregion

		#region TestGetDocumentWrappers_CarrierLabel

		public void TestGetDocumentWrappers_CarrierLabelAll()
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

			var order1InnerInnerPackWith1 = Helper.CreatePackage(order1InnerPackWith3, 1, Constants.PkgUnit.Box, "B4");
			order1InnerPackWith3.Pack(Data.DummyLine1, 1);

			var order1InnerPackWith3DS = ((IDocumentSupportable)order1InnerPackWith3).DocumentSupporter;
			var contact = order1InnerPackWith3DS.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY);
			if (contact != null && contact.OrgHeader != null)
			{
				var serviceLevel = ((OrgHeader)contact.OrgHeader).MiscServ.CarrierServiceLevels.Select(s => s.PL_Code).First();
				var dummyParent = PkgPackageJob.LoadParent<DummyWithPacking>(Data.PackageJob);
				dummyParent.SetCarrierServiceLevelCode(serviceLevel);
				dummyParent.CarrierForTest = contact.OrgHeader as OrgHeader;

				var wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericRetailersLabelAll, Factory.New<StmMenuItem>());
				AssertEquals("No Selected, so all packs on all levels", 5, wrappers.Length);

				Data.PackageJob.Selected.UpdateSelectedPackages(new[] { order1InnerPackWith3 });
				wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericRetailersLabelAll, Factory.New<StmMenuItem>());
				AssertEquals("Selected 1, but not context package, so do all", 5, wrappers.Length);
			}
		}

		#endregion

		#region TestGetDocumentWrappers_GenericDeliveryIDLabelAll

		public void TestGetDocumentWrappers_GenericDeliveryIDLabelAll()
		{
			Data.CreatePackingData();

			var pallet1 = Data.PackageJob.Packages.AddNew("PLT");
			var pallet2 = Data.PackageJob.Packages.AddNew("PLT");
			var box1 = pallet1.Packages.AddNew("BOX");
			var box2 = pallet2.Packages.AddNew("BOX");

			pallet1.KP_PackageID = "PLT-1";
			pallet2.KP_PackageID = "PLT-2";
			box1.KP_PackageID = "BOX-1";
			box2.KP_PackageID = "BOX-2";

			var newPackageID1 = Data.PackageJob.LoosePackageIDs.AddNew();
			newPackageID1.KPH_PackageID = "ABC";
			var newPackageID2 = Data.PackageJob.LoosePackageIDs.AddNew();
			newPackageID2.KPH_PackageID = "DEF";

			Factory.Save();

			var packageJobDocumentSupporter = ((IDocumentSupportable)Data.PackageJob).DocumentSupporter;
			var wrappers = packageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericDeliveryIDLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("All outer package and package headers will be selected", 4, wrappers.Length);
		}

		#endregion

		#endregion

		#region TestGetDocumentWrappers_ForGetContactOrganisation

		public void TestGetDocumentWrappers_ForGetContactOrganisation()
		{
			Data.CreatePackingData();
			var contact = Data.PackageJobDocumentSupporter.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY);
			AssertEquals("OC_ContactName", "Fred", ((OrgHeader)contact.OrgHeader).Contacts[0].OC_ContactName);
			AssertEquals("OC_Mobile", "112233", ((OrgHeader)contact.OrgHeader).Contacts[0].OC_Mobile);
		}

		public void TestGetDocumentWrappers_ForGetContactOrganisationWithNoParent()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var contact = ((IDocumentSupportable)packageJob).DocumentSupporter.GetContactOrganisation("", ContactType.Consignee, DocumentDirection.ANY);
			AssertNull("Cannot get a contact because there is no parent", contact);
		}

		#endregion

		#region TestGetDocumentWrappers_ModuleSpecificLabel

		public void TestGetDocumentWrappers_ModuleSpecificLabel()
		{
			Data.CreatePackingData();
			var package1 = Helper.CreatePackage(Data.PackageJob, "FilterMe1", 1, Constants.PkgUnit.Box);
			package1.Pack(Data.DummyLine1, 1m);

			var childOfTheFilteredPack = Helper.CreatePackage(package1, 1, Constants.PkgUnit.Box, "TheSon");
			childOfTheFilteredPack.Pack(Data.DummyLine1, 1m);

			var package2 = Helper.CreatePackage(Data.PackageJob, "PkLvl1", 1, Constants.PkgUnit.Box);
			package2.Pack(Data.DummyLine2, 1m);

			var package3 = Helper.CreatePackage(package2, 1, Constants.PkgUnit.Box, "PkLvl2");
			package3.Pack(Data.DummyLine3, 1m);

			var wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericAuditVarianceLabel, Factory.New<StmMenuItem>());
			AssertEquals("Current functionality for all data contexts: No Selected packages, so should return first package without 'FilterMe' at current level(1).", 1, wrappers.Length);
			AssertContainsExactElementsInAnyOrder("No selected packages, so return first package without 'FilterMe' at current level(1).", package2.PackageJob, wrappers.Select(w => w.WrappedObject));

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { package3 });
			wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericAuditVarianceLabel, Factory.New<StmMenuItem>());
			AssertEquals("Current functionality for all data contexts: Selected 1 package, however, since it's called from PackageJob it should return all packages without 'FilterMe'.", 1, wrappers.Length);
			AssertContainsExactElementsInAnyOrder("Selected 1 package, however, since it's called from PackageJob it should return all packages without 'FilterMe'.", package2.PackageJob, wrappers.Select(w => w.WrappedObject));
		}

		#endregion

		#region TestGetDocumentWrappers_ModuleSpecificLabelAll

		public void TestGetDocumentWrappers_ModuleSpecificLabelAll()
		{
			Data.CreatePackingData();
			var package1 = Helper.CreatePackage(Data.PackageJob, "FilterMe1", 1, Constants.PkgUnit.Box);
			package1.Pack(Data.DummyLine1, 1m);

			var childOfTheFilteredPack = Helper.CreatePackage(package1, 1, Constants.PkgUnit.Box, "TheSon");
			childOfTheFilteredPack.Pack(Data.DummyLine1, 1m);

			var package2 = Helper.CreatePackage(Data.PackageJob, "Pack2", 1, Constants.PkgUnit.Box);
			package2.Pack(Data.DummyLine2, 1);

			var package3 = Helper.CreatePackage(package2, 1, Constants.PkgUnit.Box, "Pack3");
			package3.Pack(Data.DummyLine3, 1);

			var expectedPacks = new PkgPackageJob[] { package2.PackageJob, package3.PackageJob, childOfTheFilteredPack.PackageJob };
			var wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericAuditVarianceLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("Current functionality for all data contexts: No Selected packages, so should return all packages without 'FilterMe'.", 3, wrappers.Length);
			AssertContainsExactElementsInAnyOrder("No selected packages, so return packages without 'FilterMe'.", expectedPacks, wrappers.Select(w => w.WrappedObject));

			Data.PackageJob.Selected.UpdateSelectedPackages(new[] { package3 });
			wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericAuditVarianceLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("Current functionality for all data contexts: Selected 1 package, however, since it's called from PackageJob it should return all packages without 'FilterMe'.", 3, wrappers.Length);
			AssertContainsExactElementsInAnyOrder("Selected 1 package, however, since it's called from PackageJob it should return all packages without 'FilterMe'.", expectedPacks, wrappers.Select(w => w.WrappedObject));
		}

		#endregion

		#region TestGetDocumentWrappers_GetBODocDataProvidersNotFoundMessage

		public void TestGetDocumentWrappers_GetBODocDataProvidersNotFoundMessage()
		{
			Data.CreatePackingData();
			var message = Data.PackageJobDocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValue("GenericAuditVarianceLabel"), Factory.New<StmMenuItem>());
			AssertEquals("Must show a Not Found message", "Not Found!", message);
		}

		#endregion

		#region TestDocumentNumberAndTotalInWrappers

		public void TestDocumentNumberAndTotalInWrappers()
		{
			Data.CreatePackingData();
			Helper.CreatePackage(Data.PackageJob, "B1", 1, Constants.PkgUnit.Box);
			var outerPack2 = Helper.CreatePackage(Data.PackageJob, "B2", 1, Constants.PkgUnit.Box);
			Helper.CreatePackage(Data.PackageJob, "B3", 1, Constants.PkgUnit.Box);
			var innerPack = Helper.CreatePackage(outerPack2, 1, Constants.PkgUnit.Box, "B4");
			Helper.CreatePackage(innerPack, 1, Constants.PkgUnit.Box, "B5");

			var wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericRetailersLabel, Factory.New<StmMenuItem>());
			AssertEquals("Wrapper length is incorrect, should not include inners.", 3, wrappers.Length);
			AssertPackageNumberAndTotal((IPackageOverrider)wrappers[0], 1, 3);
			AssertPackageNumberAndTotal((IPackageOverrider)wrappers[1], 2, 3);
			AssertPackageNumberAndTotal((IPackageOverrider)wrappers[2], 3, 3);

			wrappers = Data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericRetailersLabelAll, Factory.New<StmMenuItem>());
			AssertEquals("Wrapper length is incorrect, should not include inners.", 5, wrappers.Length);
			AssertPackageNumberAndTotal((IPackageOverrider)wrappers[0], 1, 3);
			AssertPackageNumberAndTotal((IPackageOverrider)wrappers[1], 2, 3);
			AssertPackageNumberAndTotal((IPackageOverrider)wrappers[2], 0, 0);
			AssertPackageNumberAndTotal((IPackageOverrider)wrappers[3], 0, 0);
			AssertPackageNumberAndTotal((IPackageOverrider)wrappers[4], 3, 3);
		}

		void AssertPackageNumberAndTotal(IPackageOverrider wrapper, ZInt documentNumber, ZInt documentTotal)
		{
			AssertEquals("Document number is incorrect", documentNumber, wrapper.DocumentNumber);
			AssertEquals("Document total is incorrect", documentTotal, wrapper.DocumentTotal);
		}

		#endregion
	}
}
