using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(OrderLinesSelectionForm))]
	sealed class OrderLinesSelectionFormTest : ZFormBasherTest
	{
		public void TestImportSelectedOrderLines_WhenSelectButtonClicked()
		{
			var whsWarehouse = Helper.GetNewWhsWarehouse(Helper.Warehouse.MainAddress.PK, true, "N10");
			var whsReceive = Helper.GetNewWhsReceive(whsWarehouse.PK, Helper.Importer.PK);
			var whsInventory1 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 900m, 900m, bondedEntryKey: "EN00123-1");
			var whsInventory2 = Helper.GetNewReceiveInventory(whsReceive, Helper.Part, ZString.Empty, 50m, 900m, 900m, bondedEntryKey: "EN00123-2");

			var orderLinePicked = CreateNewPickedOrderAndOrderLines("ORD1", "W0000111", 1, 10m, (whsInventory1, qtyToPick: 10m));
			var orderLinePickedMultiInventory = CreateNewPickedOrderAndOrderLines("ORD2", "W0000222", 2, 15m, (whsInventory1, qtyToPick: 6m), (whsInventory2, qtyToPick: 9m));
			var orderLineUnpicked = CreateNewPickedOrderAndOrderLines("ORD3", "W0000333", 3, 8m);

			Factory.Save();

			IWhsDocketLine CreateNewPickedOrderAndOrderLines(ZString orderRefText, ZString orderID, ZShort orderLineNo, ZDecimal totalOrderQty, params (IWhsInventoryView inventory, ZDecimal qtyToPick)[] inventoryQtyPairs)
			{
				var order = Helper.GetNewWhsOrder(Helper.Importer.PK, whsWarehouse, orderRefText);
				order.WD_DocketID = orderID;
				var orderLine = Helper.GetNewWhsOrderLine(order, Helper.Part, totalOrderQty);
				orderLine.WE_LineNo = orderLineNo;
				Helper.GetNewWhsPick(new[] { order });
				foreach (var pair in inventoryQtyPairs)
				{
					Helper.GetNewWhsPickLine(orderLine, pair.inventory, pair.qtyToPick);
				}

				return orderLine;
			}

			var declaration = Factory.New<BaseJobDeclaration>();

			using (var form = new OrderLinesSelectionForm_ForTesting(declaration))
			{
				form.Show();
				AssertEquals("Prerequisite: No invoice lines in declaration before the order lines imported.", 0, declaration.InvoiceLines.Count);

				var filterStripControl = form.FindSingle<ZFilterStripControl>("OrderLineFilterStripControl");
				filterStripControl.FirePerformSearch();
				var selectButton = form.FindSingle<ZButton>("SelectButton");

				var loadedOrderLineCollection = filterStripControl.GridCollection;
				AssertContainsExactElementsInAnyOrder(new ZString[] { "W0000111", "W0000222", "W0000333" }, loadedOrderLineCollection.Cast<IWhsDocketLine>().Select(line => line.WE_WD).Select(wd => Factory.Load<IWhsDocket>(wd)).Select(order => order.WD_DocketID));

				CombineAssertions("When no lines are selected: ", () =>
				{
					selectButton.PerformClick();
					AssertEquals("No Order Lines are selected.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(0, declaration.InvoiceLines.Count);
				});

				CombineAssertions("When one line unpicked is selected: ", () =>
				{
					filterStripControl.FilteredGrid.Select(loadedOrderLineCollection.IndexOf(orderLineUnpicked));
					selectButton.PerformClick();

					AssertEquals("No related Inventories can be loaded for the selected Order Lines, please ensure that at least one of the selected Order Lines is attached to a Pick and allocated to one specific inventory.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(0, declaration.InvoiceLines.Count);
				});
				UnitTestUserNotification.Instance.ClearMessages();

				CombineAssertions("When one line picked from single inventory is selected: ", () =>
				{
					filterStripControl.FilteredGrid.UnSelectAll();
					filterStripControl.FilteredGrid.Select(loadedOrderLineCollection.IndexOf(orderLinePicked));
					selectButton.PerformClick();

					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContainsExactElementsInAnyOrder(new[] { "W0000111-1-10" }, declaration.InvoiceLines.Select(line => $"{line.JI_BondedWHSOrderNumber}-{line.JI_BondedWHSOrderLineNumber}-{line.JI_BondedWhsQuantity}"));
				});
				declaration.InvoiceLines.RemoveAndDeleteAll();

				CombineAssertions("When one line picked from multiple inventories is selected: ", () =>
				{
					filterStripControl.FilteredGrid.UnSelectAll();
					filterStripControl.FilteredGrid.Select(loadedOrderLineCollection.IndexOf(orderLinePickedMultiInventory));
					selectButton.PerformClick();

					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContainsExactElementsInAnyOrder(new[] { "W0000222-2-6", "W0000222-2-9" }, declaration.InvoiceLines.Select(line => $"{line.JI_BondedWHSOrderNumber}-{line.JI_BondedWHSOrderLineNumber}-{line.JI_BondedWhsQuantity}"));
				});
				declaration.InvoiceLines.RemoveAndDeleteAll();

				CombineAssertions("When multiple lines are selected, both are picked: ", () =>
				{
					filterStripControl.FilteredGrid.UnSelectAll();
					filterStripControl.FilteredGrid.Select(loadedOrderLineCollection.IndexOf(orderLinePicked));
					filterStripControl.FilteredGrid.Select(loadedOrderLineCollection.IndexOf(orderLinePickedMultiInventory));
					selectButton.PerformClick();

					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContainsExactElementsInAnyOrder(new[] { "W0000111-1-10", "W0000222-2-6", "W0000222-2-9" }, declaration.InvoiceLines.Select(line => $"{line.JI_BondedWHSOrderNumber}-{line.JI_BondedWHSOrderLineNumber}-{line.JI_BondedWhsQuantity}"));
				});
				declaration.InvoiceLines.RemoveAndDeleteAll();

				CombineAssertions("When multiple lines are selected, partially picked", () =>
				{
					filterStripControl.FilteredGrid.UnSelectAll();
					filterStripControl.FilteredGrid.Select(loadedOrderLineCollection.IndexOf(orderLinePicked));
					filterStripControl.FilteredGrid.Select(loadedOrderLineCollection.IndexOf(orderLinePickedMultiInventory));
					filterStripControl.FilteredGrid.Select(loadedOrderLineCollection.IndexOf(orderLineUnpicked));

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					selectButton.PerformClick();

					AssertEquals("Only part of Order Lines out of all the selected ones are picked and allocated to typical inventories\r\n, do you want to import the qualified ones while do nothing to the unqualified rest?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(0, declaration.InvoiceLines.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					selectButton.PerformClick();

					AssertEquals("Only part of Order Lines out of all the selected ones are picked and allocated to typical inventories\r\n, do you want to import the qualified ones while do nothing to the unqualified rest?", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					AssertContainsExactElementsInAnyOrder(new[] { "W0000111-1-10", "W0000222-2-6", "W0000222-2-9" }, declaration.InvoiceLines.Select(line => $"{line.JI_BondedWHSOrderNumber}-{line.JI_BondedWHSOrderLineNumber}-{line.JI_BondedWhsQuantity}"));
				});
			}
		}

		public void TestFilterDefaults()
		{
			CombineAssertions("Warehouse and Client filters are defaulted if applicable.", () =>
			{
				using (var form = GetFormToBashCore() as OrderLinesSelectionForm_ForTesting)
				{
					AssertClientFilterActive(form, true);
					AssertWarehouseFilterActive(form, true);
				}
			});

			CombineAssertions("Client filter not defaulted if not provided.", () =>
			{
				var supporterWithoutClientPK = new Mock<IWarehouseIntegrationSupporter>();
				supporterWithoutClientPK.Setup(x => x.ClientPK).Returns(ZGuid.Empty);
				supporterWithoutClientPK.Setup(x => x.WarehouseAddress).Returns(address);
				supporterWithoutClientPK.Setup(x => x.Factory).Returns(Factory);
				using (var form = new OrderLinesSelectionForm_ForTesting(supporterWithoutClientPK.Object))
				{
					AssertClientFilterActive(form, false);
					AssertWarehouseFilterActive(form, true);
				}
			});

			CombineAssertions("Client filter not defaulted if not provided.", () =>
			{
				var supporterWithoutWarehousePK = new Mock<IWarehouseIntegrationSupporter>();
				supporterWithoutWarehousePK.Setup(x => x.ClientPK).Returns(supplier.PK);
				supporterWithoutWarehousePK.Setup(x => x.WarehouseAddress).Returns((OrgAddress)null);
				supporterWithoutWarehousePK.Setup(x => x.Factory).Returns(Factory);

				using (var form = new OrderLinesSelectionForm_ForTesting(supporterWithoutWarehousePK.Object))
				{
					AssertClientFilterActive(form, true);
					AssertWarehouseFilterActive(form, false);
				}
			});

			void AssertWarehouseFilterActive(OrderLinesSelectionForm_ForTesting form, bool isActive)
			{
				var filter = (ModuleGuidFilter)form.Module_Exposed.FilterBusinessObject.ModuleFilters["Warehouse"];
				if (isActive)
				{
					AssertEquals("Warehouse filter defaulted to warehouse.PK.", warehouse.PK, filter.Property);
				}
				else
				{
					AssertEquals("Warehouse filter not defaulted.", ZGuid.Empty, filter.Property);
				}
			}

			void AssertClientFilterActive(OrderLinesSelectionForm_ForTesting form, bool isActive)
			{
				var filter = (ModuleGuidFilter)form.Module_Exposed.FilterBusinessObject.ModuleFilters["Client"];
				if (isActive)
				{
					AssertEquals("Client filter defaulted to supplier.PK.", supplier.PK, filter.Property);
				}
				else
				{
					AssertEquals("Client filter not defaulted.", ZGuid.Empty, filter.Property);
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			supplier = Factory.NewWithValidTestData<OrgHeader>();
			address = Factory.New<OrgAddress>();
			warehouse = Factory.New<IWhsWarehouse>();
			warehouse.WW_OA_WarehouseAddress = address.PK;

			var supporter = new Mock<IWarehouseIntegrationSupporter>();
			supporter.Setup(x => x.ClientPK).Returns(supplier.PK);
			supporter.Setup(x => x.WarehouseAddress).Returns(address);
			supporter.Setup(x => x.Factory).Returns(Factory);
			return new OrderLinesSelectionForm_ForTesting(supporter.Object);
		}
		OrgHeader supplier;
		IWhsWarehouse warehouse;
		OrgAddress address;

		WhsDataTestHelper Helper => helper ?? (helper = new WhsDataTestHelper(Factory));
		WhsDataTestHelper helper;

		sealed class OrderLinesSelectionForm_ForTesting : OrderLinesSelectionForm
		{
			public OrderLinesSelectionForm_ForTesting(IWarehouseIntegrationSupporter parent) : base(parent)
			{
			}

			public ZFilterModule Module_Exposed => module;
		}
	}
}
