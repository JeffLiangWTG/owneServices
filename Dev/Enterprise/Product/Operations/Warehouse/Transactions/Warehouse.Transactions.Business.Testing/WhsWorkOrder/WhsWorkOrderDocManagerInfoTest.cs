using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsWorkOrderDocManagerInfo))]
	sealed class WhsWorkOrderDocManagerInfoTest : DocManagerInfoTestCase
	{
		public void TestGetEDocsView()
		{
			DocumentsDataRegistry.Instance.EDocsToBeAutoAttachedForDelivery.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DeliverEDocsOptionList.Codes.SendAll);

			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			data.BOM.BikeEngine.OP_AutoPrintAssemblyInstructions = true;
			data.BOM.BikeWheel.OP_AutoPrintAssemblyInstructions = true;

			// order the building of 1 Engine and 2 Wheels.
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var engineLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.BikeEngine, 1m);
			var wheelLine = Helper.CreateWhsWorkOrderLine(workOrder, data.BOM.BikeWheel, 2m);

			var docManagerInfo = new WhsWorkOrderDocManagerInfo(AssemblyWorkOrder, "WWO");
			var typeBOA = Factory.New<RefDocType>();
			typeBOA.RT_DocType = "BOA";

			var typeBOD = Factory.New<RefDocType>();
			typeBOD.RT_DocType = "BOD";

			var command = Factory.New<DocumentCommand>();
			command.SU_MenuName = "TestMenu";
			command.Parent = workOrder;
			command.AddEDoc(typeBOA);
			command.AddEDoc(typeBOD);

			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var assemblyPngPath = resourceRetriever.SaveResourceToFile("Enterprise.Warehouse.Transactions.Business.Testing.WhsWorkOrder.TestFiles.Assembly.png", "Assembly.png");
				var assemblyPdfPath = resourceRetriever.SaveResourceToFile("Enterprise.Warehouse.Transactions.Business.Testing.WhsWorkOrder.TestFiles.Assembly.pdf", "Assembly.pdf");
				// Engine: 1 unrelated eDoc, Wheel: 1 valid eDoc
				var engine_eDoc_BAD = engineLine.SupplierPartDocManagerInfo.AddFileOrDocument(assemblyPngPath, "XXX");
				var wheel_eDoc = wheelLine.SupplierPartDocManagerInfo.AddFileOrDocument(assemblyPdfPath, "BOA");
				AsserWorkOrderContainsEDocsOnly(workOrder, wheel_eDoc);
				AssertDocumentPackCount(command, 1);

				// Engine: 1 valid eDoc + 1 unrelated eDoc, Wheel: 1 valid eDoc
				var engine_eDoc = engineLine.SupplierPartDocManagerInfo.AddFileOrDocument(assemblyPdfPath, "BOA");
				AsserWorkOrderContainsEDocsOnly(workOrder, wheel_eDoc, engine_eDoc);
				AssertDocumentPackCount(command, 2);

				// ensure child item eDocs are not included on the WorkOrder's eDocs
				var pistonLine = engineLine.BOM.ChildComponentLines.Single(line => line.WE_OP == data.BOM.EnginePiston.PK);
				pistonLine.SupplierPartDocManagerInfo.AddFileOrDocument(assemblyPdfPath, "BOA");
				AsserWorkOrderContainsEDocsOnly(workOrder, wheel_eDoc, engine_eDoc);
				AssertDocumentPackCount(command, 2);

				// test Disassembly ("BOD" eDocs)
				workOrder.WD_DocketSubType = CodeLists.WorkOrderType.Codes.Disassemble;
				AsserWorkOrderContainsEDocsOnly(workOrder);
				var engine_DisassemblyInstructions_eDoc = engineLine.SupplierPartDocManagerInfo.AddFileOrDocument(assemblyPdfPath, "BOD");
				AsserWorkOrderContainsEDocsOnly(workOrder, engine_DisassemblyInstructions_eDoc);
				AssertDocumentPackCount(command, 1);
			}

			// disable printing of the Engine eDocs
			data.BOM.BikeEngine.OP_AutoPrintAssemblyInstructions = false;
			AsserWorkOrderContainsEDocsOnly(workOrder);
			AssertDocumentPackCount(command, 0);
		}

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<WhsWorkOrder>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			return AssemblyWorkOrder;
		}

		void AsserWorkOrderContainsEDocsOnly(WhsWorkOrder workOrder, params IeDoc[] expected_eDocs)
		{
			AssertEquals(expected_eDocs.Length, workOrder.DocManagerInfo.EDocsView.Count);

			foreach (IeDoc eDoc in expected_eDocs)
			{
				AssertCollectionContains(eDoc, workOrder.DocManagerInfo.EDocsView);
			}
		}

		void AssertDocumentPackCount(DocumentCommand command, int count)
		{
			using (var pack = new DocumentPack())
			{
				pack.AddEDocsToPack(command);
				AssertEquals(count, pack.Count);
			}
		}

		TestDataSimpleEnvironment WhsData
		{
			get
			{
				if (whsData == null)
				{
					whsData = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
					var part11 = Helper.CreateProduct(whsData.Org1, "P11");
					var part12 = Helper.CreateProduct(whsData.Org1, "P12");
					var part111 = Helper.CreateProduct(whsData.Org1, "P111");

					Helper.CreateProductBOM(whsData.Part1, part11, 2m, "UNT");
					Helper.CreateProductBOM(whsData.Part1, part12, 3m, "BAG");
					Helper.CreateProductBOM(part11, part111, 5m, "PLT");
				}
				return whsData;
			}
		}
		TestDataSimpleEnvironment whsData;

		WhsWorkOrder AssemblyWorkOrder
		{
			get
			{
				if (assemblyWorkOrder == null)
				{
					assemblyWorkOrder = Helper.CreateWhsWorkOrder(WhsData.Org1, WhsData.Whs1);
					Helper.CreateWhsWorkOrderLine(assemblyWorkOrder, WhsData.Part1, 1m);
					Helper.CreateWhsWorkOrderLine(assemblyWorkOrder, WhsData.Part2, 2m);
				}
				return assemblyWorkOrder;
			}
		}
		WhsWorkOrder assemblyWorkOrder;

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;
	}
}
