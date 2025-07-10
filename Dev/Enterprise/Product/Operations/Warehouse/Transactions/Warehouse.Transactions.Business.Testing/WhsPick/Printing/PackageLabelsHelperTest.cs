using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	sealed class PackageLabelsHelperTest : TestCaseWithFactory
	{
		public void TestPrintAllDocumentWithSeparatorLabels()
		{
			AssertPrintAllDocument(true);
		}

		public void TestPrintAllDocumentWithoutSeparatorLabels()
		{
			AssertPrintAllDocument(false);
		}

		public void TestGetDocumentPackWithSeparatorLabels()
		{
			var pick = SetupCartonisedWhsPick();

			using (var docPack = PackageLabelsHelper.GetDocumentPack(pick, Factory.New<DocumentCommand>()))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Expected documents: 4 for pallets + 1 end of pallet + 3 for cases + 1 end of cases + 2 for split cases + 1 end of splitcase + 1 end of area.", 13, docPack.Count);
					AssertEquals("Product/Delivery", docPack[0].Name);
					AssertEquals("Product/Delivery", docPack[1].Name);
					AssertEquals("Product/Delivery", docPack[2].Name);
					AssertEquals("Product/Delivery", docPack[3].Name);
					AssertEquals("End of Pallet", docPack[4].Name);
					AssertEquals("Product/Delivery", docPack[5].Name);
					AssertEquals("Product/Delivery", docPack[6].Name);
					AssertEquals("Product/Delivery", docPack[7].Name);
					AssertEquals("End of Case", docPack[8].Name);
					AssertEquals("Product/Delivery", docPack[9].Name);
					AssertEquals("Product/Delivery", docPack[10].Name);
					AssertEquals("End of Split Case", docPack[11].Name);
					AssertEquals("End of Area", docPack[12].Name);
				});
			}
		}

		public void TestGetDocumentPackWithoutSeparatorLabels()
		{
			var pick = SetupCartonisedWhsPick();
			using (pick.EnablePrintingWithoutSeparatorLabels())
			using (var docPack = PackageLabelsHelper.GetDocumentPack(pick, Factory.New<DocumentCommand>()))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Expected documents: 4 for pallets 3 for cases + 2 for split cases.", 9, docPack.Count);
					AssertEquals("Product/Delivery", docPack[0].Name);
					AssertEquals("Product/Delivery", docPack[1].Name);
					AssertEquals("Product/Delivery", docPack[2].Name);
					AssertEquals("Product/Delivery", docPack[3].Name);
					AssertEquals("Product/Delivery", docPack[4].Name);
					AssertEquals("Product/Delivery", docPack[5].Name);
					AssertEquals("Product/Delivery", docPack[6].Name);
					AssertEquals("Product/Delivery", docPack[7].Name);
					AssertEquals("Product/Delivery", docPack[8].Name);
				});
			}
		}

		public void TestGetDocumentPack_ChildDocuments_DocPack_WithSeparatorLabels()
		{
			AssertGetDocumentPack_ChildDocuments(true, true);
		}

		public void TestGetDocumentPack_ChildDocuments_DocPack_WithoutSeparatorLabels()
		{
			AssertGetDocumentPack_ChildDocuments(true, false);
		}

		public void TestGetDocumentPack_ChildDocuments_NotDocPack_WithSeparatorLabels()
		{
			AssertGetDocumentPack_ChildDocuments(false, true);
		}

		public void TestGetDocumentPack_ChildDocuments_NotDocPack_WithoutSeparatorLabels()
		{
			AssertGetDocumentPack_ChildDocuments(false, false);
		}

		public void TestAddDelimeterDocumentsIfNeeded_EndOfArea()
		{
			AssertAddDelimeterDocumentsIfNeeded(PackageComparerForLabelPrinting.DocDelimeterType.EndOfArea, "End of Area");
		}

		public void TestAddDelimeterDocumentsIfNeeded_EndOfCase()
		{
			AssertAddDelimeterDocumentsIfNeeded(PackageComparerForLabelPrinting.DocDelimeterType.EndOfCase, "End of Case");
		}

		public void TestAddDelimeterDocumentsIfNeeded_EndOfPallet()
		{
			AssertAddDelimeterDocumentsIfNeeded(PackageComparerForLabelPrinting.DocDelimeterType.EndOfPallet, "End of Pallet");
		}

		public void TestAddDelimeterDocumentsIfNeeded_EndOfSplitCase()
		{
			AssertAddDelimeterDocumentsIfNeeded(PackageComparerForLabelPrinting.DocDelimeterType.EndOfSplitCase, "End of Split Case");
		}

		public void TestAddDelimeterDocumentsIfNeeded_EndOfPickGroup()
		{
			AssertAddDelimeterDocumentsIfNeeded(PackageComparerForLabelPrinting.DocDelimeterType.EndOfPickGroup, "End of Pick Group");
		}

		public void TestAddDelimeterDocumentsIfNeeded_None()
		{
			var package = Factory.New<PkgPackage>();
			var docList = new DocumentCommandCollection(package);

			PackageLabelsHelper.AddDelimeterDocumentsIfNeeded(PackageComparerForLabelPrinting.DocDelimeterType.None, docList);
			AssertEquals("No delimiter document is added.", false, docList.Count > 0);
		}

		public void TestGetDocCommandInNewReadOnlyFactory()
		{
			var command = PackageLabelsHelper.GetDocCommandInNewReadOnlyFactory();
			AssertType<ReadOnlyBusinessObjectFactory>("Should have been created in a new read only factory.", command.Factory);
			AssertEquals("Should be a DocPack.", true, command.SU_IsDocPack);
			AssertEquals("Should have correct MenuName.", "All Package Labels for a Pick.", command.SU_MenuName);
		}

		public void TestGetDocumentPack_PackageWithNoPackType()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 28m);
			var pick = helper.CreatePickNew(order);
			order.PackageJob.Packages.AddNew("KG"); // Not a pack type, should not blow up
			AssertNoExceptionThrown(() => { using (PackageLabelsHelper.GetDocumentPack(pick, Factory.New<DocumentCommand>())) { } });
		}

		public void TestExceptionForNullPick()
		{
			AssertExceptionThrown<ArgumentNullException>(() => { using (PackageLabelsHelper.GetDocumentPack(null, Factory.New<DocumentCommand>())) { } });
		}

		public void TestEndOfAreaLabelExistsInDb()
		{
			AssertDocExistsInDB("End of Area Label", PackageLabelsHelper.EndOfAreaLabelPK);
		}

		public void TestEndOfPalletLabelExistsInDb()
		{
			AssertDocExistsInDB("End of Pallet Label", PackageLabelsHelper.EndOfPalletLabelPK);
		}

		public void TestEndOfCaseLabelExistsInDb()
		{
			AssertDocExistsInDB("End of Case Label", PackageLabelsHelper.EndOfCaseLabelPK);
		}

		public void TestEndOfSplitCaseLabelExistsInDb()
		{
			AssertDocExistsInDB("End of Split Case Label", PackageLabelsHelper.EndOfSplitCaseLabelPK);
		}

		public void TestEndOfPickGroupLabelExistsInDb()
		{
			AssertDocExistsInDB("End of Pick Group Label", PackageLabelsHelper.EndOfPickGroupLabelPK);
		}

		void AssertPrintAllDocument(bool isPrintingWithoutSeparatorLabels)
		{
			var pick = SetupCartonisedWhsPick();

			using (isPrintingWithoutSeparatorLabels ? pick.EnablePrintingWithoutSeparatorLabels() : null)
			{
				var packageLabelsHelper = new PackageLabelsHelper(pick);
				packageLabelsHelper.PrintAllDocument();

				foreach (var package in pick.OuterPackages)
				{
					AssertEquals(true, package.IsLabelPrinted);
				}
			}
		}

		void AssertGetDocumentPack_ChildDocuments(bool isDocPack, bool isPrintingWithoutSeparatorLabels)
		{
			var pick = SetupCartonisedWhsPick(1, 1, 1);

			using (isPrintingWithoutSeparatorLabels ? pick.EnablePrintingWithoutSeparatorLabels() : null)
			{
				var parentCommand = Factory.Load<DocumentCommand>(PackingRegistry.Instance.DefaultDocumentToPrintOnClosePackage.Value);
				parentCommand.SU_IsDocPack = isDocPack;

				var template = CreateTemplate("Test");
				var childCommand1 = CreateDocCommand("ChildCommand1", "Package");
				CreateMenuMenuPivot(parentCommand, childCommand1);
				CreateMenuTemplatePivot("Child1", template, childCommand1);

				var childCommand2 = CreateDocCommand("ChildCommand2", "Package");
				CreateMenuMenuPivot(parentCommand, childCommand2);
				CreateMenuTemplatePivot("Child2", template, childCommand2);

				using (var docPack = PackageLabelsHelper.GetDocumentPack(pick, Factory.New<DocumentCommand>()))
				{
					if (isPrintingWithoutSeparatorLabels)
					{
						CombineAssertions(() =>
						{
							if (isDocPack)
							{
								AssertEquals("Expected documents: 2 for pallets children + 2 for cases children + 2 for split cases child.", 6, docPack.Count);
								AssertEquals("Child1", docPack[0].Name);
								AssertEquals("Child2", docPack[1].Name);
								AssertEquals("Child1", docPack[2].Name);
								AssertEquals("Child2", docPack[3].Name);
								AssertEquals("Child1", docPack[4].Name);
								AssertEquals("Child2", docPack[5].Name);
							}
							else
							{
								AssertEquals("Expected documents: 1 for pallets + 2 for children + 1 for cases + 2 for child  + 1 for split cases + 2 for child.", 9, docPack.Count);
								AssertEquals("Product/Delivery", docPack[0].Name);
								AssertEquals("Child1", docPack[1].Name);
								AssertEquals("Child2", docPack[2].Name);
								AssertEquals("Product/Delivery", docPack[3].Name);
								AssertEquals("Child1", docPack[4].Name);
								AssertEquals("Child2", docPack[5].Name);
								AssertEquals("Product/Delivery", docPack[6].Name);
								AssertEquals("Child1", docPack[7].Name);
								AssertEquals("Child2", docPack[8].Name);
							}
						});
					}
					else
					{
						CombineAssertions(() =>
						{
							if (isDocPack)
							{
								AssertEquals("Expected documents: 2 for pallets children + 1 end of pallet + 2 for cases children + 1 end of cases + 2 for split cases child + 1 end of splitcase + 1 end of area.", 10, docPack.Count);
								AssertEquals("Child1", docPack[0].Name);
								AssertEquals("Child2", docPack[1].Name);
								AssertEquals("End of Pallet", docPack[2].Name);
								AssertEquals("Child1", docPack[3].Name);
								AssertEquals("Child2", docPack[4].Name);
								AssertEquals("End of Case", docPack[5].Name);
								AssertEquals("Child1", docPack[6].Name);
								AssertEquals("Child2", docPack[7].Name);
								AssertEquals("End of Split Case", docPack[8].Name);
								AssertEquals("End of Area", docPack[9].Name);
							}
							else
							{
								AssertEquals("Expected documents: 1 for pallets + 2 for children + 1 end of pallet + 1 for cases + 2 for child  + 1 end of cases + 1 for split cases + 2 for child + 1 end of splitcase + 1 end of area.", 13, docPack.Count);
								AssertEquals("Product/Delivery", docPack[0].Name);
								AssertEquals("Child1", docPack[1].Name);
								AssertEquals("Child2", docPack[2].Name);
								AssertEquals("End of Pallet", docPack[3].Name);
								AssertEquals("Product/Delivery", docPack[4].Name);
								AssertEquals("Child1", docPack[5].Name);
								AssertEquals("Child2", docPack[6].Name);
								AssertEquals("End of Case", docPack[7].Name);
								AssertEquals("Product/Delivery", docPack[8].Name);
								AssertEquals("Child1", docPack[9].Name);
								AssertEquals("Child2", docPack[10].Name);
								AssertEquals("End of Split Case", docPack[11].Name);
								AssertEquals("End of Area", docPack[12].Name);
							}
						});
					}
				}
			}
		}

		DocumentCommand CreateDocCommand(string name, string context)
		{
			var result = Factory.New<DocumentCommand>();
			result.SU_IsPublished = true;
			result.SU_IsSystemDefined = true;
			result.SU_MenuName = name;
			result.SU_IsDocPack = false;
			result.SU_BusinessContext = context;

			return result;
		}

		StmMenuMenuPivot CreateMenuMenuPivot(DocumentCommand inward, DocumentCommand outward)
		{
			StmMenuMenuPivot result = inward.ChildMenus.AddNew();
			result.SF_SU_Inward = inward.PK;
			result.SF_SU_Outward = outward.PK;
			result.SF_IsSystemDefined = true;
			return result;
		}

		StmTemplateBase CreateTemplate(string name)
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(typeof(DocumentEngine.Testing.PrintTaskTest).Assembly))
			{
				var result = Factory.New<StmTemplateBase>();
				result.SO_DataContext = "UnitTest";
				result.SO_IsSystemDefined = true;
				result.SO_Name = name;
				result.SO_Template = resourceRetriever.GetBytes("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.UDF with defaults.xls");
				return result;
			}
		}

		StmMenuTemplatePivotBase CreateMenuTemplatePivot(string title, StmTemplateBase template, DocumentCommand command)
		{
			var result = command.Documents.AddNew();
			result.SI_SO = template.PK;
			result.SI_SU = command.PK;
			result.SI_DocumentTitle = title;
			return result;
		}

		void AssertAddDelimeterDocumentsIfNeeded(PackageComparerForLabelPrinting.DocDelimeterType delimeterType, string expectedDocName)
		{
			var package = Factory.New<PkgPackage>();
			var docList = new DocumentCommandCollection(package);

			PackageLabelsHelper.AddDelimeterDocumentsIfNeeded(delimeterType, docList);
			AssertEquals("Expected delimiter document is added.", 1, docList.Count);
			AssertEquals("Expected delimiter document is added.", expectedDocName, docList[0].SU_MenuName);
		}

		WhsPick SetupCartonisedWhsPick(int splitCaseNumberOfLabels = 2, int caseNumberOfLabels = 3, int palletNumberOfLabels = 4)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			var helper = new WhsTestHelperFunctions(Factory);
			var packingHelper = new PackingTestHelper(Factory);
			helper.CreateProductUnit(data.Part1, "CAS", 5);
			helper.CreateProductUnit(data.Part1, "PLT", 20);

			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("UNT")).F3_UOMType = UOMPackTypesList.Codes.SplitCase;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("CAS")).F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT")).F3_UOMType = UOMPackTypesList.Codes.Pallet;

			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 28m);
			Factory.Save();

			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 28m);
			var pick = helper.CreatePickNew(order);
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();

			pick.AllocatePackageLabels();
			var packageForSplitCase = packingHelper.CreatePackage(order.PackageJob, "P-000001", 1, "UNT");
			var pickLineForSplitCase = pick.GetAllPickLines().Single(pl => pl.WZ_F3_NKAllocatedPackType == "UNT");
			packingHelper.CreatePackageDivot(packageForSplitCase, pickLineForSplitCase);
			AssertEquals("Precondition: should be 3 packages: for pallet, for case and for split cases.", 3, order.PackageJob.GetAllPackagesOnJob().Length);
			order.PackageJob.Packages.AddNew(); // Should be filtered out
			AssertEquals("Precondition", 4, order.PackageJob.GetAllPackagesOnJob().Length);

			var uomTypeSettings = WarehouseDataRegistry.Instance.UOMPackType.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			uomTypeSettings.Cast<UOMPackType>().Single(u => u.Code == UOMPackTypesList.Codes.SplitCase).NumberOfLabels = splitCaseNumberOfLabels;
			uomTypeSettings.Cast<UOMPackType>().Single(u => u.Code == UOMPackTypesList.Codes.Case).NumberOfLabels = caseNumberOfLabels;
			uomTypeSettings.Cast<UOMPackType>().Single(u => u.Code == UOMPackTypesList.Codes.Pallet).NumberOfLabels = palletNumberOfLabels;
			WarehouseDataRegistry.Instance.UOMPackType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, uomTypeSettings);
			return pick;
		}

		void AssertDocExistsInDB(ZString docName, ZGuid docPK)
		{
			var docList = new DocumentCommandCollection(Factory.New<PkgPackage>());
			docList.AddFromDatabase(docPK);
			AssertEquals(string.Format("System document {0} with SU_PK = {1} must be in database.", docName, docPK), 1, docList.Count);
		}
	}
}
