using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(GenerateRMAByOrdersActionMethodApplicator))]
	public class GenerateRMAByOrdersActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestCopyBOMComponentLinks

		#region TestCopyBOMComponentLinks_HaveMultipleGroupReleaseLinesFromWorkOrderInventory

		public void TestCopyBOMComponentLinks_HaveMultipleGroupReleaseLinesFromWorkOrderInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receiveForCompentPart = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receiveForCompentPart, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receiveForCompentPart, data.Part2, 10m);
			receiveForCompentPart.AllocateLocationsWithMock();
			receiveForCompentPart.FinaliseDocketWithoutUserConfirmation();

			var mainProduct1 = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateProductBOM(mainProduct1, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(mainProduct1, data.Part2, 1m, "UNT");
			mainProduct1.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			mainProduct1.RelatedOrganisations[0].OU_UsePartAttrib2 = true;

			var mainProduct2 = Helper.CreateProduct(data.Org1, "P4");
			Helper.CreateProductBOM(mainProduct2, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(mainProduct2, data.Part2, 1m, "UNT");
			mainProduct2.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			mainProduct2.RelatedOrganisations[0].OU_UsePartAttrib2 = true;

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder, mainProduct1, 1m);
			Helper.SetDocketLineAttributes(workOrderLine1, ZDate.Empty, ZDate.Empty, "color 1", "size 1", "", "");
			var workOrderLine2 = Helper.CreateWhsWorkOrderLine(workOrder, mainProduct1, 2m);
			Helper.SetDocketLineAttributes(workOrderLine2, ZDate.Empty, ZDate.Empty, "color 2", "size 2", "", "");
			var workOrderLine3 = Helper.CreateWhsWorkOrderLine(workOrder, mainProduct2, 3m);
			Helper.SetDocketLineAttributes(workOrderLine3, ZDate.Empty, ZDate.Empty, "color 3", "size 3", "", "");
			var workOrderLine4 = Helper.CreateWhsWorkOrderLine(workOrder, mainProduct2, 4m);
			Helper.SetDocketLineAttributes(workOrderLine4, ZDate.Empty, ZDate.Empty, "color 4", "size 4", "", "");

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1");
			Helper.CreateWhsOrderLine(order, mainProduct1, 3m);
			Helper.CreateWhsOrderLine(order, mainProduct2, 7m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var expectedErrorLogText =
				"<-- Summary -->\r\n" +
				"INFO: Receive [HL W00000005] has been generated.\r\n";
			ApplyApplicator(new WhsOrder[] { order }, expectedErrorLogText, true);

			var query = new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, order.PK);
			var returnReceives = Factory.Load<WhsReceive>(query);

			AssertEquals("Should be 1 receive", 1, returnReceives.Length);
			AssertEquals("Should contain 4 receive lines", 4, returnReceives[0].Lines.Count);

			var expectedComponentsInventoryPKs = receiveForCompentPart.Lines.Select(l => l.PK).ToList();
			AssertReturnReceiveLines(returnReceives[0], 1, 1, mainProduct1.PK, expectedComponentsInventoryPKs);
			AssertReturnReceiveLines(returnReceives[0], 2, 1, mainProduct1.PK, expectedComponentsInventoryPKs);
			AssertReturnReceiveLines(returnReceives[0], 3, 1, mainProduct2.PK, expectedComponentsInventoryPKs);
			AssertReturnReceiveLines(returnReceives[0], 4, 1, mainProduct2.PK, expectedComponentsInventoryPKs);
		}

		#endregion

		#region TestCopyBOMComponentLinks_OneReleaseLineFromInventoryOfMultipleTypes

		public void TestCopyBOMComponentLinks_OneReleaseLineFromInventoryOfMultipleTypes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receiveForComponents = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receiveForComponents, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receiveForComponents, data.Part2, 20m);
			receiveForComponents.AllocateLocationsWithMock();
			receiveForComponents.FinaliseDocketWithoutUserConfirmation();

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			part3.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(part3, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(part3, data.Part2, 1m, "UNT");

			var receiveForPart3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			Helper.CreateWhsReceiveInventoryLine(receiveForPart3, part3, 1m);
			receiveForPart3.AllocateLocationsWithMock();
			receiveForPart3.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var workOrder1 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "work order 1");
			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder1, part3, 1m);
			var workOrder2 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "work order 2");
			var workOrderLine2 = Helper.CreateWhsWorkOrderLine(workOrder2, part3, 1m);

			Helper.CreatePickNew(workOrder1);
			workOrder1.FinaliseDocketAlwaysFinalisingPick();

			Helper.CreatePickNew(workOrder2);
			workOrder2.FinaliseDocketAlwaysFinalisingPick();

			workOrder1.Receive.FinaliseDocketWithoutUserConfirmation();
			workOrder2.Receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, part3, 4m);
			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			Factory.Save();

			var receiveLineForPBB = Factory.LoadTop1<WhsDocket>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));

			var expectedErrorLogText =
				"<-- Summary -->\r\n" +
				"INFO: Receive [HL W00000009] has been generated.\r\n";
			ApplyApplicator(new WhsOrder[] { order }, expectedErrorLogText, true);

			var query = new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, order.PK);
			var returnReceives = Factory.Load<WhsReceive>(query);

			AssertEquals("Should be 1 receive", 1, returnReceives.Length);
			AssertEquals("Should contain 3 receive lines", 3, returnReceives[0].Lines.Count);

			var receiveLinesFromPBBAndNormalReceiveLine = returnReceives[0].Lines.Where(l => l.WE_TransactionQuantity == 2 && l.WE_OP == part3.PK && !l.BOMComponentLinks.Any()).ToArray();
			AssertEquals(1, receiveLinesFromPBBAndNormalReceiveLine.Length);

			var expectedComponentsInventoryPKs = receiveForComponents.Lines.Select(l => l.PK).ToList();
			AssertReturnReceiveLines(returnReceives[0], 1, 2, part3.PK, expectedComponentsInventoryPKs);
		}

		#endregion

		#region TestCopyBOMComponentLinks_MultipleOrderLinesFromSameInventory

		public void TestCopyBOMComponentLinks_MultipleOrderLinesFromSameInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receiveForComponentsPart = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");

			Helper.CreateWhsReceiveInventoryLine(receiveForComponentsPart, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receiveForComponentsPart, data.Part2, 10m);
			receiveForComponentsPart.AllocateLocationsWithMock();
			receiveForComponentsPart.FinaliseDocketWithoutUserConfirmation();

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateProductBOM(part3, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(part3, data.Part2, 1m, "UNT");
			part3.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			part3.RelatedOrganisations[0].OU_UsePartAttrib2 = true;

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, part3, 2m);
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1");
			Helper.CreateWhsOrderLine(order, part3, 1m);
			Helper.CreateWhsOrderLine(order, part3, 1m);
			var pick = Helper.CreatePickNew(order);
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var expectedErrorLogText =
				"<-- Summary -->\r\n" +
				"INFO: Receive [HL W00000005] has been generated.\r\n";
			ApplyApplicator(new WhsOrder[] { order }, expectedErrorLogText, true);

			var query = new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, order.PK);
			var receives = Factory.Load<WhsReceive>(query);

			AssertEquals("Should be 1 receive", 1, receives.Length);
			AssertEquals("Should contain 1 receive lines", 1, receives[0].Lines.Count);
			AssertReturnReceiveLines(receives[0], 2, 1, part3.PK, receiveForComponentsPart.Lines.Select(l => l.PK).ToList());
		}

		#endregion

		#region TestCopyBOMComponentLinks_MultipleOrdersFromSameInventory

		public void TestCopyBOMComponentLinks_MultipleOrdersFromSameInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receiveForComponent = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");

			Helper.CreateWhsReceiveInventoryLine(receiveForComponent, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receiveForComponent, data.Part2, 10m);
			receiveForComponent.AllocateLocationsWithMock();
			receiveForComponent.FinaliseDocketWithoutUserConfirmation();

			var mainProduct = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateProductBOM(mainProduct, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(mainProduct, data.Part2, 1m, "UNT");
			mainProduct.RelatedOrganisations[0].OU_UsePartAttrib1 = true;
			mainProduct.RelatedOrganisations[0].OU_UsePartAttrib2 = true;

			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var workOrderLine = Helper.CreateWhsWorkOrderLine(workOrder, mainProduct, 2m);
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			workOrder.Receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order 1");
			Helper.CreateWhsOrderLine(order1, mainProduct, 1m);
			var pick1 = Helper.CreatePickNew(order1);
			order1.FinaliseDocketWithoutUserConfirmation();
			pick1.FinalisePick();

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "order 2");
			Helper.CreateWhsOrderLine(order2, mainProduct, 1m);
			var pick2 = Helper.CreatePickNew(order2);
			order2.FinaliseDocketWithoutUserConfirmation();
			pick2.FinalisePick();

			var expectedErrorLogText =
				"<-- Summary -->\r\n" +
				"INFO: Receive [HL W00000006] has been generated.\r\n" +
				"INFO: Receive [HL W00000007] has been generated.\r\n";
			ApplyApplicator(new WhsOrder[] { order1, order2 }, expectedErrorLogText, true);

			var query = new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, new ZGuid[] { order1.PK, order2.PK });
			var receives = Factory.Load<WhsReceive>(query);

			AssertEquals("Should be 2 receive", 2, receives.Length);
			AssertEquals("Should contain 1 receive lines", 1, receives[0].Lines.Count);
			AssertEquals("Should contain 1 receive lines", 1, receives[1].Lines.Count);

			AssertReturnReceiveLines(receives[0], 1, 1, mainProduct.PK, receiveForComponent.Lines.Select(l => l.PK).ToList());
			AssertReturnReceiveLines(receives[1], 1, 1, mainProduct.PK, receiveForComponent.Lines.Select(l => l.PK).ToList());
		}

		#endregion

		#region TestDbHitsForWhsBOMInventoryPivot

		public void TestDbHitsForWhsBOMInventoryPivot()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receiveForComponents = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receiveForComponents, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receiveForComponents, data.Part2, 20m);
			receiveForComponents.AllocateLocationsWithMock();
			receiveForComponents.FinaliseDocketWithoutUserConfirmation();

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			part3.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(part3, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(part3, data.Part2, 1m, "UNT");

			var receiveForPart3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");

			for (int i = 0; i < 20; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receiveForPart3, part3, 1m);
			}
			receiveForPart3.AllocateLocationsWithMock();
			receiveForPart3.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			for (int i = 0; i < 2; i++)
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, $"O{i}");
				Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				var pick = Helper.CreatePickNew(order);
				order.FinaliseDocketWithoutUserConfirmation();
				pick.FinalisePick();
			}

			Factory.Save();

			var newfactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ DtbBookingConsolidationSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 2 },
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobHeaderSchema.Constants.TableName, 2 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },     //Implementation of denied party resynchronization screening status
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessTaskNotificationSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 5 }, //CheckExternalReferenceForDuplicates when a new docket is setting property WD_ExternalReference
				{ WhsDocketJobPivotSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsBOMInventoryPivotSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 2 },       //Implementation of denied party resynchronization screening status
			};

			var orderInNewFactory = newfactory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newfactory))
			using (RowFactory.SetCachedTables())
			{
				SimulateRun(orderInNewFactory, true);
			}
		}

		#endregion

		[TestDate(2021, 8, 20)]
		public void TestCopyBOMComponentLinks_AllAttributesBeenRetained()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var expiryDate = ZDate.Today;
			var packingDate = ZDate.Today;
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "PLT-1", expiryDate, packingDate, "A", "B", "C", "S1", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order");
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories[0].Allocate = true;

			Factory.Save();

			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Factory.Save();

			var expectedErrorLogText =
				"<-- Summary -->\r\n" +
				"INFO: Receive [HL W00000003] has been generated.\r\n";
			ApplyApplicator(new WhsOrder[] { order }, expectedErrorLogText, true);

			var query = new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, order.PK);
			var newReceive = Factory.LoadTop1<WhsReceive>(query);

			Assert("All attributes are retained for new inventory", newReceive.Lines.All(l => l.WE_PartAttrib1 == "A" && l.WE_PartAttrib2 == "B" && l.WE_PartAttrib3 == "C" && l.WE_SerialNumber == "S1" && l.WE_ExpiryDate == expiryDate && l.WE_PackingDate == packingDate));
		}

		void AssertReturnReceiveLines(WhsDocket returnReceive, int expectedQuantity, int expectedLineCount, ZGuid originalPartPK, List<ZGuid> componentLinePKs)
		{
			var returnReceiveLine = returnReceive.Lines.Where(line => line.WE_TransactionQuantity == expectedQuantity && line.WE_OP == originalPartPK && line.BOMComponentLinks.Any()).ToList();
			AssertEquals($"Should be {expectedLineCount} receive line", expectedLineCount, returnReceiveLine.Count);
			for (int i = 0; i < expectedLineCount; i++)
			{
				var receiveLine = returnReceiveLine[i];
				AssertEquals(2, receiveLine.ComponentInventoryLines.Count);
				Assert(receiveLine.BOMComponentLinks.All(link => link.WIP_ComponentQuantity == expectedQuantity));
				Assert(receiveLine.ComponentInventoryLines.All(l => componentLinePKs.Contains(l.PK)));
			}
		}

		#endregion

		#region TestAction

		public void TestAction()
		{
			Applicator.RMAReference = "TEST";

			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var pickFinalizedOrderWithReleaseLines1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1");
			Helper.CreateWhsOrderLine(pickFinalizedOrderWithReleaseLines1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(pickFinalizedOrderWithReleaseLines1);
			pickFinalizedOrderWithReleaseLines1.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Factory.Save();

			var pickFinalizedOrderWithReleaseLines2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized2");
			Helper.CreateWhsOrderLine(pickFinalizedOrderWithReleaseLines2, data.Part1, 10m);
			Helper.CreateWhsOrderLine(pickFinalizedOrderWithReleaseLines2, data.Part2, 10m);
			var pick2 = Helper.CreatePickNew(pickFinalizedOrderWithReleaseLines2);
			pickFinalizedOrderWithReleaseLines2.FinaliseDocketWithoutUserConfirmation();
			pick2.FinalisePick();
			Factory.Save();

			var pickFinalizedOrderWithReleaseLines3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized3");
			Helper.CreateWhsOrderLine(pickFinalizedOrderWithReleaseLines3, data.Part1, 10m);
			Helper.CreateWhsOrderLine(pickFinalizedOrderWithReleaseLines3, data.Part2, 10m);
			var pick3 = Helper.CreatePickNew(pickFinalizedOrderWithReleaseLines3);
			pickFinalizedOrderWithReleaseLines3.FinaliseDocketWithoutUserConfirmation();
			pick3.FinalisePick();
			Factory.Save();

			var pickFinalizedOrderWithOutReleaseLine = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized4");
			Helper.CreateWhsOrderLine(pickFinalizedOrderWithOutReleaseLine, data.Part1, 10m);
			Helper.CreateWhsOrderLine(pickFinalizedOrderWithOutReleaseLine, data.Part2, 10m);
			var pick4 = Helper.CreatePickNew(pickFinalizedOrderWithOutReleaseLine);
			pickFinalizedOrderWithOutReleaseLine.FinaliseDocketWithoutUserConfirmation();
			pick4.FinalisePick();
			Factory.Save();

			var pickUnFinalizedOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "PickUnFinalized", data.Part1, 10m);
			Helper.CreatePickNew(pickUnFinalizedOrder);

			Factory.Save();

			AssertEquals("Precondition", 1, pickFinalizedOrderWithReleaseLines1.Lines.Cast<WhsPickableDocketLine>().Count(l => l.ReleaseLines.Count > 0));
			AssertEquals("Precondition", 2, pickFinalizedOrderWithReleaseLines2.Lines.Cast<WhsPickableDocketLine>().Count(l => l.ReleaseLines.Count > 0));
			AssertEquals("Precondition", 1, pickFinalizedOrderWithReleaseLines3.Lines.Cast<WhsPickableDocketLine>().Count(l => l.ReleaseLines.Count > 0));
			AssertEquals("Precondition", 0, pickFinalizedOrderWithOutReleaseLine.Lines.Cast<WhsPickableDocketLine>().Count(l => l.ReleaseLines.Count > 0));
			AssertEquals("Precondition", 0, pickUnFinalizedOrder.Lines.Cast<WhsPickableDocketLine>().Count(l => l.ReleaseLines.Count > 0));

			var orders = new WhsOrder[] { pickFinalizedOrderWithReleaseLines1, pickFinalizedOrderWithReleaseLines2, pickFinalizedOrderWithReleaseLines3, pickFinalizedOrderWithOutReleaseLine, pickUnFinalizedOrder };

			var saveOnSuccess = true;
			var expectedErrorLogText =
				"<-- Summary -->\r\n" +
				"INFO: Receive [HL W00000007] has been generated.\r\n" +
				"INFO: Receive [HL W00000008] has been generated.\r\n" +
				"INFO: Receive [HL W00000009] has been generated.\r\n" +
				"ERROR: Could not generate a Receive from Order [HL W00000005], because no Release Lines in this Order.\r\n" +
				"ERROR: Could not generate a Receive from Order [HL W00000006], because its Pick is not finalized.\r\n";

			ApplyApplicator(orders, expectedErrorLogText, saveOnSuccess);

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, CodeLists.DocketType.Codes.Receive);
			query.AddToFilter(WhsDocketSchema.PK, SQLComparisonOperator.NotEqual, receive.PK);
			query.OrderBy = "WD_DocketID";
			var receives = Factory.Load<WhsReceive>(query);
			AssertEquals("Should be 3 receive", 3, receives.Length);
			AssertEquals("Should be 1 receive lines", 1, receives[0].Lines.Count);
			AssertEquals("Should be 2 receive lines", 2, receives[1].Lines.Count);
			AssertEquals("Should be 1 receive lines", 1, receives[2].Lines.Count);
			foreach (var receiveItem in receives)
			{
				AssertEquals("RMA Reference should be 'TEST'", "TEST", receiveItem.WD_CustomerReference);
				AssertEquals("Should link to Order", 1, receiveItem.RelatedJobs.Count);
				AssertEquals("Warehouse should match Order", data.Whs1, receiveItem.Warehouse);
			}
			AssertEquals("Should link to RMA Receive", 1, pickFinalizedOrderWithReleaseLines1.RelatedJobs.Count);
			AssertEquals("Should link to RMA Receive", 1, pickFinalizedOrderWithReleaseLines2.RelatedJobs.Count);
			AssertEquals("Should link to RMA Receive", 1, pickFinalizedOrderWithReleaseLines3.RelatedJobs.Count);

			AssertContainsExactElementsInAnyOrder(new[] { pickFinalizedOrderWithReleaseLines1 }, receives[0].RelatedJobs);
			AssertContainsExactElementsInAnyOrder(new[] { pickFinalizedOrderWithReleaseLines2 }, receives[1].RelatedJobs);
			AssertContainsExactElementsInAnyOrder(new[] { pickFinalizedOrderWithReleaseLines3 }, receives[2].RelatedJobs);
			AssertContainsExactElementsInAnyOrder(new[] { receives[0] }, pickFinalizedOrderWithReleaseLines1.RelatedJobs);
			AssertContainsExactElementsInAnyOrder(new[] { receives[1] }, pickFinalizedOrderWithReleaseLines2.RelatedJobs);
			AssertContainsExactElementsInAnyOrder(new[] { receives[2] }, pickFinalizedOrderWithReleaseLines3.RelatedJobs);
		}

		#endregion

		#region TestAction_NoRMAReferenceEntered

		public void TestAction_NoRMAReferenceEntered()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var pickFinalizedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(pickFinalizedOrder, data.Part1, 10m);
			var pick = Helper.CreatePickNew(pickFinalizedOrder);
			pickFinalizedOrder.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Factory.Save();

			var orders = new WhsOrder[] { pickFinalizedOrder };

			var saveOnSuccess = true;
			var expectedErrorLogText =
				"<-- Summary -->\r\n" +
				"INFO: Receive [HL W00000003] has been generated.\r\n";

			ApplyApplicator(orders, expectedErrorLogText, saveOnSuccess);

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, CodeLists.DocketType.Codes.Receive);
			query.AddToFilter(WhsDocketSchema.PK, SQLComparisonOperator.NotEqual, receive.PK);
			query.OrderBy = "WD_DocketID";
			var receives = Factory.Load<WhsReceive>(query);
			AssertEquals("Should be 1 receive", 1, receives.Length);

			var loadedReceive = receives[0];
			AssertEquals("Should be 1 receive lines", 1, loadedReceive.Lines.Count);
			AssertEquals("RMA Reference should be empty", ZString.Empty, loadedReceive.WD_CustomerReference);
			AssertEquals("Should link to PickFinalizedOrder", 1, loadedReceive.RelatedJobs.Count);
			AssertEquals("Should link to RMA Receive", 1, pickFinalizedOrder.RelatedJobs.Count);
			AssertContainsExactElementsInAnyOrder(new[] { pickFinalizedOrder }, loadedReceive.RelatedJobs);
			AssertContainsExactElementsInAnyOrder(new[] { loadedReceive }, pickFinalizedOrder.RelatedJobs);
		}

		#endregion

		#region TestWhsOverrideSingleOrder_NoRMAReferenceEntered

		public void TestWhsOverrideSingleOrder_NoRMAReferenceEntered()
		{
			var newWarehouse = Helper.CreateWarehouse("W2");
			Applicator.WhsOverride = newWarehouse.PK;
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var pickFinalizedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(pickFinalizedOrder, data.Part1, 10m);
			Helper.CreateWhsOrderLine(pickFinalizedOrder, data.Part2, 30m);
			var pick = Helper.CreatePickNew(pickFinalizedOrder);
			pickFinalizedOrder.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Factory.Save();

			var orders = new WhsOrder[] { pickFinalizedOrder };

			var saveOnSuccess = true;
			var expectedErrorLogText =
				"<-- Summary -->\r\n" +
				"INFO: Receive [HL W00000003] has been generated.\r\n";

			ApplyApplicator(orders, expectedErrorLogText, saveOnSuccess);

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, CodeLists.DocketType.Codes.Receive);
			query.AddToFilter(WhsDocketSchema.PK, SQLComparisonOperator.NotEqual, receive.PK);
			query.OrderBy = "WD_DocketID";
			var receives = Factory.Load<WhsReceive>(query);
			AssertEquals("Should be 1 receive", 1, receives.Length);

			var loadedReceive = receives[0];
			AssertEquals("Should be 2 receive lines", 2, loadedReceive.Lines.Count);
			AssertEquals("RMA Reference should be empty", ZString.Empty, loadedReceive.WD_CustomerReference);
			AssertEquals("Should link to PickFinalizedOrder", 1, loadedReceive.RelatedJobs.Count);
			AssertEquals("Should link to RMA Receive", 1, pickFinalizedOrder.RelatedJobs.Count);
			AssertContainsExactElementsInAnyOrder(new[] { pickFinalizedOrder }, loadedReceive.RelatedJobs);
			AssertContainsExactElementsInAnyOrder(new[] { loadedReceive }, pickFinalizedOrder.RelatedJobs);
			AssertEquals("Warehouse should be W2", newWarehouse.WW_WarehouseCode, loadedReceive.Warehouse.WW_WarehouseCode);
		}

		#endregion

		#region TestWhsOverrideMultipleOrders_RMAReferenceEntered

		public void TestWhsOverrideMultipleOrders_RMAReferenceEntered()
		{
			Applicator.RMAReference = "TEST";
			var newWarehouse = Helper.CreateWarehouse("W2");
			Applicator.WhsOverride = newWarehouse.PK;

			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var pickFinalizedOrderWithReleaseLines1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1");
			Helper.CreateWhsOrderLine(pickFinalizedOrderWithReleaseLines1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(pickFinalizedOrderWithReleaseLines1);
			pickFinalizedOrderWithReleaseLines1.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Factory.Save();

			var pickFinalizedOrderWithReleaseLines2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized2");
			Helper.CreateWhsOrderLine(pickFinalizedOrderWithReleaseLines2, data.Part1, 10m);
			Helper.CreateWhsOrderLine(pickFinalizedOrderWithReleaseLines2, data.Part2, 10m);
			var pick2 = Helper.CreatePickNew(pickFinalizedOrderWithReleaseLines2);
			pickFinalizedOrderWithReleaseLines2.FinaliseDocketWithoutUserConfirmation();
			pick2.FinalisePick();
			Factory.Save();

			var pickFinalizedOrderWithReleaseLines3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized3");
			Helper.CreateWhsOrderLine(pickFinalizedOrderWithReleaseLines3, data.Part1, 10m);
			Helper.CreateWhsOrderLine(pickFinalizedOrderWithReleaseLines3, data.Part2, 10m);
			var pick3 = Helper.CreatePickNew(pickFinalizedOrderWithReleaseLines3);
			pickFinalizedOrderWithReleaseLines3.FinaliseDocketWithoutUserConfirmation();
			pick3.FinalisePick();
			Factory.Save();

			var pickFinalizedOrderWithOutReleaseLine = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized4");
			Helper.CreateWhsOrderLine(pickFinalizedOrderWithOutReleaseLine, data.Part1, 10m);
			Helper.CreateWhsOrderLine(pickFinalizedOrderWithOutReleaseLine, data.Part2, 10m);
			var pick4 = Helper.CreatePickNew(pickFinalizedOrderWithOutReleaseLine);
			pickFinalizedOrderWithOutReleaseLine.FinaliseDocketWithoutUserConfirmation();
			pick4.FinalisePick();
			Factory.Save();

			var pickUnFinalizedOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "PickUnFinalized", data.Part1, 10m);
			Helper.CreatePickNew(pickUnFinalizedOrder);

			Factory.Save();

			AssertEquals("Precondition", 1, pickFinalizedOrderWithReleaseLines1.Lines.Cast<WhsPickableDocketLine>().Count(l => l.ReleaseLines.Count > 0));
			AssertEquals("Precondition", 2, pickFinalizedOrderWithReleaseLines2.Lines.Cast<WhsPickableDocketLine>().Count(l => l.ReleaseLines.Count > 0));
			AssertEquals("Precondition", 1, pickFinalizedOrderWithReleaseLines3.Lines.Cast<WhsPickableDocketLine>().Count(l => l.ReleaseLines.Count > 0));
			AssertEquals("Precondition", 0, pickFinalizedOrderWithOutReleaseLine.Lines.Cast<WhsPickableDocketLine>().Count(l => l.ReleaseLines.Count > 0));
			AssertEquals("Precondition", 0, pickUnFinalizedOrder.Lines.Cast<WhsPickableDocketLine>().Count(l => l.ReleaseLines.Count > 0));

			var orders = new WhsOrder[] { pickFinalizedOrderWithReleaseLines1, pickFinalizedOrderWithReleaseLines2, pickFinalizedOrderWithReleaseLines3, pickFinalizedOrderWithOutReleaseLine, pickUnFinalizedOrder };

			var saveOnSuccess = true;
			var expectedErrorLogText =
				"<-- Summary -->\r\n" +
				"INFO: Receive [HL W00000007] has been generated.\r\n" +
				"INFO: Receive [HL W00000008] has been generated.\r\n" +
				"INFO: Receive [HL W00000009] has been generated.\r\n" +
				"ERROR: Could not generate a Receive from Order [HL W00000005], because no Release Lines in this Order.\r\n" +
				"ERROR: Could not generate a Receive from Order [HL W00000006], because its Pick is not finalized.\r\n";

			ApplyApplicator(orders, expectedErrorLogText, saveOnSuccess);

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, CodeLists.DocketType.Codes.Receive);
			query.AddToFilter(WhsDocketSchema.PK, SQLComparisonOperator.NotEqual, receive.PK);
			query.OrderBy = "WD_DocketID";
			var receives = Factory.Load<WhsReceive>(query);
			AssertEquals("Should be 3 receive", 3, receives.Length);
			AssertEquals("Should be 1 receive lines", 1, receives[0].Lines.Count);
			AssertEquals("Should be 2 receive lines", 2, receives[1].Lines.Count);
			AssertEquals("Should be 1 receive lines", 1, receives[2].Lines.Count);
			foreach (var receiveItem in receives)
			{
				AssertEquals("RMA Reference should be 'TEST'", "TEST", receiveItem.WD_CustomerReference);
				AssertEquals("Should link to Order", 1, receiveItem.RelatedJobs.Count);
				AssertEquals("Warehouse should be W2", newWarehouse.WW_WarehouseCode, receiveItem.Warehouse.WW_WarehouseCode);
			}
			AssertEquals("Should link to RMA Receive", 1, pickFinalizedOrderWithReleaseLines1.RelatedJobs.Count);
			AssertEquals("Should link to RMA Receive", 1, pickFinalizedOrderWithReleaseLines2.RelatedJobs.Count);
			AssertEquals("Should link to RMA Receive", 1, pickFinalizedOrderWithReleaseLines3.RelatedJobs.Count);

			AssertContainsExactElementsInAnyOrder(new[] { pickFinalizedOrderWithReleaseLines1 }, receives[0].RelatedJobs);
			AssertContainsExactElementsInAnyOrder(new[] { pickFinalizedOrderWithReleaseLines2 }, receives[1].RelatedJobs);
			AssertContainsExactElementsInAnyOrder(new[] { pickFinalizedOrderWithReleaseLines3 }, receives[2].RelatedJobs);
			AssertContainsExactElementsInAnyOrder(new[] { receives[0] }, pickFinalizedOrderWithReleaseLines1.RelatedJobs);
			AssertContainsExactElementsInAnyOrder(new[] { receives[1] }, pickFinalizedOrderWithReleaseLines2.RelatedJobs);
			AssertContainsExactElementsInAnyOrder(new[] { receives[2] }, pickFinalizedOrderWithReleaseLines3.RelatedJobs);
		}

		#endregion

		#region TestAction_BuildLine

		public void TestAction_BuildLine()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, today.AddDays(1), today, "PA1", "PA2", "PA3", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m, today.AddDays(2), today, "PA1", "PA2", "PA3", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var pickFinalizedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(pickFinalizedOrder, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(pickFinalizedOrder, data.Part1, 20m);
			var orderLine3 = Helper.CreateWhsOrderLine(pickFinalizedOrder, data.Part1, 10m);
			var orderLine4 = Helper.CreateWhsOrderLine(pickFinalizedOrder, data.Part1, 30m);
			var pick = Helper.CreatePickNew(pickFinalizedOrder);
			pickFinalizedOrder.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Factory.Save();

			AssertEquals("Precondition: orderLine has own release line", true, orderLine1.ReleaseLines.Count > 0);
			AssertEquals("Precondition: orderLine has own release line", true, orderLine2.ReleaseLines.Count > 0);
			AssertEquals("Precondition: orderLine has own release line", true, orderLine3.ReleaseLines.Count > 0);
			AssertEquals("Precondition: orderLine has own release line", true, orderLine4.ReleaseLines.Count > 0);

			var orders = new WhsOrder[] { pickFinalizedOrder };

			var saveOnSuccess = true;
			var expectedErrorLogText =
				"<-- Summary -->\r\n" +
				"INFO: Receive [HL W00000003] has been generated.\r\n";

			ApplyApplicator(orders, expectedErrorLogText, saveOnSuccess);

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, CodeLists.DocketType.Codes.Receive);
			query.AddToFilter(WhsDocketSchema.PK, SQLComparisonOperator.NotEqual, receive.PK);
			query.OrderBy = "WD_DocketID";
			var receives = Factory.Load<WhsReceive>(query);
			AssertEquals("Should be 1 receive", 1, receives.Length);
			AssertEquals("Should be 2 receive lines", 2, receives[0].Lines.Count);
			AssertEquals("Receive line 1 grouped by orderLine1 and orderLine2", 30m, receives[0].Lines[0].WE_PackQuantity);
			AssertEquals("Receive line 2 grouped by orderLine3 and orderLine4", 40m, receives[0].Lines[1].WE_PackQuantity);
		}

		#endregion

		#region TestAction_GetLineData

		public void TestAction_GetLineData()
		{
			var today = ZDate.Today;
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, today.AddDays(1), today, "PA1", "PA2", "PA3", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var pickFinalizedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(pickFinalizedOrder, data.Part1, 11m);
			var pick = Helper.CreatePickNew(pickFinalizedOrder);
			pickFinalizedOrder.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Factory.Save();

			AssertEquals("Precondition: only 10 units released.", 10m, orderLine.ReleaseLines[0].Quantity);

			var orders = new WhsOrder[] { pickFinalizedOrder };
			var saveOnSuccess = true;
			var expectedErrorLogText =
				"<-- Summary -->\r\n" +
				"INFO: Receive [HL W00000003] has been generated.\r\n";

			ApplyApplicator(orders, expectedErrorLogText, saveOnSuccess);

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive);
			query.AddToFilter(WhsDocketSchema.PK, SQLComparisonOperator.NotEqual, receive.PK);
			query.OrderBy = "WD_DocketID";
			var receives = Factory.Load<WhsReceive>(query);
			AssertEquals("Should be 1 receive", 1, receives.Length);

			var loadedReceive = receives[0];
			AssertEquals(data.Org1.PK, loadedReceive.WD_OH_Client);
			AssertEquals(data.Whs1.PK, loadedReceive.WD_WW_Whs);
			AssertEquals(CodeLists.ReceiveType.Codes.Returns, loadedReceive.WD_DocketSubType);
			AssertEquals("O1 W00000003", loadedReceive.WD_ExternalReference);

			AssertEquals("Should be 1 receive lines", 1, loadedReceive.Lines.Count);
			AssertEquals(data.Part1.PK, loadedReceive.Lines[0].WE_OP);
			AssertEquals(10m, loadedReceive.Lines[0].WE_TransactionQuantity);
			AssertEquals(today.AddDays(1), loadedReceive.Lines[0].WE_ExpiryDate);
			AssertEquals(today, loadedReceive.Lines[0].WE_PackingDate);

			AssertEquals("PA1", loadedReceive.Lines[0].WE_PartAttrib1);
			AssertEquals("PA2", loadedReceive.Lines[0].WE_PartAttrib2);
			AssertEquals("PA3", loadedReceive.Lines[0].WE_PartAttrib3);
		}

		public void TestAction_GetLineData_WithSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			inventory.WI_SerialNumber = "SN1";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var pickFinalizedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(pickFinalizedOrder, data.Part1, 1m);
			orderLine.WE_SerialNumber = "SN1";
			var pick = Helper.CreatePickNew(pickFinalizedOrder);
			pickFinalizedOrder.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Factory.Save();

			AssertEquals("Precondition: only 1 units released.", 1m, orderLine.ReleaseLines[0].Quantity);

			var orders = new WhsOrder[] { pickFinalizedOrder };
			var saveOnSuccess = true;
			var expectedErrorLogText =
				"<-- Summary -->\r\n" +
				"INFO: Receive [HL W00000003] has been generated.\r\n";

			ApplyApplicator(orders, expectedErrorLogText, saveOnSuccess);

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, CodeLists.DocketType.Codes.Receive);
			query.AddToFilter(WhsDocketSchema.PK, SQLComparisonOperator.NotEqual, receive.PK);
			query.OrderBy = "WD_DocketID";
			var receives = Factory.Load<WhsReceive>(query);
			AssertEquals("Should be 1 receive", 1, receives.Length);

			var loadedReceive = receives[0];
			AssertEquals(data.Org1.PK, loadedReceive.WD_OH_Client);
			AssertEquals(data.Whs1.PK, loadedReceive.WD_WW_Whs);
			AssertEquals(ReceiveType.Codes.Returns, loadedReceive.WD_DocketSubType);
			AssertEquals("O1 W00000003", loadedReceive.WD_ExternalReference);

			AssertEquals("Should be 1 receive lines", 1, loadedReceive.Lines.Count);
			AssertEquals(data.Part1.PK, loadedReceive.Lines[0].WE_OP);
			AssertEquals(1m, loadedReceive.Lines[0].WE_TransactionQuantity);
			AssertEquals("SN1", loadedReceive.Lines[0].WE_SerialNumber);
		}

		#endregion

		#region TestAction_GetLineData_AttributeReleaseCaptured

		public void TestAction_GetLineData_AttributeReleaseCaptured()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.NonMandatory);

			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: true, useSerialNumber: false);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var pickFinalizedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(pickFinalizedOrder, data.Part1, 11m);
			var pick = Helper.CreatePickNew(pickFinalizedOrder);
			pickFinalizedOrder.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Factory.Save();

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("Precondition: only 10 units released.", 10m, releaseLine.Quantity);
			releaseLine.PartAttribute1 = "P1";
			releaseLine.PartAttribute2 = "P2";
			releaseLine.PartAttribute3 = "P3";

			var orders = new WhsOrder[] { pickFinalizedOrder };
			var saveOnSuccess = true;
			var expectedErrorLogText =
				"<-- Summary -->\r\n" +
				"INFO: Receive [HL W00000003] has been generated.\r\n";

			ApplyApplicator(orders, expectedErrorLogText, saveOnSuccess);

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, CodeLists.DocketType.Codes.Receive);
			query.AddToFilter(WhsDocketSchema.PK, SQLComparisonOperator.NotEqual, receive.PK);
			query.OrderBy = "WD_DocketID";
			var receives = Factory.Load<WhsReceive>(query);
			AssertEquals("Should be 1 receive", 1, receives.Length);

			var loadedReceive = receives[0];

			AssertEquals(string.Empty, loadedReceive.Lines[0].WE_PartAttrib1);
			AssertEquals(string.Empty, loadedReceive.Lines[0].WE_PartAttrib2);
			AssertEquals(string.Empty, loadedReceive.Lines[0].WE_PartAttrib3);
		}

		public void TestAction_GetLineData_AttributeReleaseCaptured_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var pickFinalizedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(pickFinalizedOrder, data.Part1, 1m);
			var pick = Helper.CreatePickNew(pickFinalizedOrder);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("Precondition: only 1 units released.", 1m, releaseLine.Quantity);
			releaseLine.SerialNumber = "SN1";

			pickFinalizedOrder.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();
			Factory.Save();

			var orders = new WhsOrder[] { pickFinalizedOrder };
			var saveOnSuccess = true;
			var expectedErrorLogText =
				"<-- Summary -->\r\n" +
				"INFO: Receive [HL W00000003] has been generated.\r\n";

			ApplyApplicator(orders, expectedErrorLogText, saveOnSuccess);

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive);
			query.AddToFilter(WhsDocketSchema.PK, SQLComparisonOperator.NotEqual, receive.PK);
			query.OrderBy = "WD_DocketID";
			var receives = Factory.Load<WhsReceive>(query);
			AssertEquals("Should be 1 receive", 1, receives.Length);

			var loadedReceive = receives[0];

			AssertEquals(string.Empty, loadedReceive.Lines[0].WE_SerialNumber);
		}

		#endregion

		#region TestFetchHint

		public void TestDbHitsWhenAction()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 300m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			for (var i = 0; i < 20; i++)
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, $"O{i}");
				Helper.CreateWhsOrderLine(order, data.Part1, 1m);
				Helper.CreateWhsOrderLine(order, data.Part1, 2m);
				var pick = Helper.CreatePickNew(order);
				order.FinaliseDocketWithoutUserConfirmation();
				pick.FinalisePick();
			}
			Factory.Save();

			var newfactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ DtbBookingConsolidationSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 2 },
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobHeaderSchema.Constants.TableName, 20 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },     //Implementation of denied party resynchronization screening status
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessTaskNotificationSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 23 }, //CheckExternalReferenceForDuplicates when a new docket is setting property WD_ExternalReference
				{ WhsDocketJobPivotSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsBOMInventoryPivotSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 2 },       //Implementation of denied party resynchronization screening status
			};

			var orderInNewFactory = newfactory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newfactory))
			using (RowFactory.SetCachedTables())
			{
				SimulateRun(orderInNewFactory, true);
			}
		}

		public void TestDbHitsWhenAction_NotFinalizedOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 300m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			for (var i = 0; i < 20; i++)
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, $"O{i}");
				Helper.CreateWhsOrderLine(order, data.Part1, 1m);
				Helper.CreateWhsOrderLine(order, data.Part1, 2m);
				Helper.CreatePickNew(order);
			}
			Factory.Save();

			var newfactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ WhsDocketSchema.Constants.TableName, 3 }, //CheckExternalReferenceForDuplicates when a new docket is setting property WD_ExternalReference
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			var orderInNewFactory = newfactory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order));

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newfactory))
			using (RowFactory.SetCachedTables())
			{
				SimulateRun(orderInNewFactory, true);
			}
		}

		#endregion

		public void TestAction_OrderAlreadyLinkedToReturnReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Order123");
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			Helper.CreateWhsReceiveLine(returnReceive, data.Part1, 5m);
			returnReceive.WD_WD_ParentDocket = order.PK;
			Factory.Save();

			var saveOnSuccess = true;
			var expectedErrorLogText =
				"<-- Summary -->\r\n" +
				"ERROR: Could not generate a Receive from Order [HL W00000003], because a return receive is already linked to this Order.\r\n";

			ApplyApplicator(new[] { order }, expectedErrorLogText, saveOnSuccess);

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive);
			query.AddToFilter(WhsDocketSchema.WD_DocketSubType, SQLComparisonOperator.Equal, ReceiveType.Codes.Returns);
			var receives = Factory.Load<WhsReceive>(query);
			AssertEquals("Should return only 1 return receive", 1, receives.Length);
		}

		#region Implementation

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion

		new GenerateRMAByOrdersActionMethodApplicator Applicator => (GenerateRMAByOrdersActionMethodApplicator)base.Applicator;
	}
}
