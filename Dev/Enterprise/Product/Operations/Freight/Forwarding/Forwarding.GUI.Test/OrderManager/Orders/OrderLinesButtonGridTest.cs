using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Modules;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.GUI.Testing
{
	public class OrderLinesButtonGridTest : TestCaseWithFactory
	{
		#region TestEditOrderLineLicenceCheckpoint

		public void TestEditOrderLineLicenceCheckpoint()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var order = Factory.New<Order>();
			order.BuyerPK = org.PK;
			var orderLine = order.OrderLines.AddNew();
			Factory.Save();

			using (var module = ModuleTree.Tree.FindByID(ModuleIDs.Orders.Name).CreateZModule())
			using (var form = (ZForm)((IFilterModuleInternalsForTesting)module).ShowViewForm(order))
			using (var userControl = new OrderLinesButtonGridControl())
			{
				form.Controls.Add(userControl);

				userControl.OrderLinesButtonGrid.EditButton.PerformClick();

				using (var shownForm = (ZForm)userControl.OrderLinesButtonGrid.LastShownZForm)
				{
					AssertEquals("ContainsCheckpoint(Env.Licence.OrderManager)", true, shownForm.LicensedComponentManager.ContainsCheckpoint(Env.Licence.OrderManager));
				}
			}
		}

		#endregion

		#region TestSerialNumberColumn

		public void TestSerialNumberColumn()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var order = Factory.New<Order>();
			order.BuyerPK = org.PK;
			using (var form = new ZForm(order))
			using (var grid = new OrderLinesButtonGrid())
			{
				grid.BindToGridList = "OrderLines";
				form.Controls.Add(grid);
				form.Show();
				grid.InnerGrid.SetDataBinding(order, "OrderLines");
				var columnStyle = grid.InnerGrid.GetColumnStyle(JobOrderLineSchema.JO_SerialNumber.Name);
				AssertNotNull("Serial Number column should exist", columnStyle);
				AssertEquals("Visibility", false, columnStyle.IsVisible);
			}
		}

		#endregion

		#region TestWorkflowCustomFields

		public void TestWorkflowCustomFields()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.OrderLineWorkflowDescriptorCode;
			template.P0_Name = "WF Order Line Test Workflow";

			var column = template.GenCustomColumnDefinitions.AddNew();
			column.XC_Type = MasterFiles.Business.CustomValues.AddOnColumnDataType.Codes.Integer;
			column.XC_Name = "WF Custom Field";

			var order = Factory.NewWithValidTestData<Order>();
			Factory.Save();

			using (var grid = new OrderLinesButtonGrid())
			{
				grid.BindToGridList = "OrderLines";
				grid.Show();
				grid.SetDataBinding(order, "OrderLines");
				var columnStyle = grid.InnerGrid.GetColumnStyle("__WF CUSTOM FIELD__prop__ZInt");
				AssertNotNull(columnStyle);
				AssertEquals("Caption", "WF Custom Field", columnStyle.Caption);
				AssertEquals("Visibility", false, columnStyle.IsVisible);
				AssertEquals("ReadOnly", true, columnStyle.IsReadOnly);
			}
		}

		#endregion

		#region TestHSCodeColumn

		public void TestHSCodeColumn()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var order = Factory.New<Order>();
			order.JD_OrderDate = new ZDateTime(2023, 1, 9);
			order.JD_SystemCreateTimeUtc = new ZDateTime(2023, 1, 5, 1, 1, 1);
			order.BuyerPK = org.PK;
			using (var form = new ZForm(order))
			using (var grid = new OrderLinesButtonGrid())
			{
				grid.BindToGridList = "OrderLines";
				form.Controls.Add(grid);
				form.Show();
				grid.InnerGrid.SetDataBinding(order, "OrderLines");
				var columnStyle = grid.InnerGrid.GetColumnStyle(JobOrderLineSchema.JO_HSCode.Name) as TariffColumnStyleInfo;
				AssertNotNull("H.S. Code column should exist", columnStyle);
				AssertEquals("Visibility", false, columnStyle.IsVisible);
				AssertEquals("EffectiveDate", order.JD_SystemCreateTimeUtc, columnStyle.GetEffectiveDate());
			}
		}

		#endregion

		#region TestCommodityCodeColumn

		[RequiresSTA]
		public void TestCommodityCodeColumn()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var order = Factory.New<Order>();
			order.JD_OrderDate = new ZDateTime(2023, 1, 9);
			order.BuyerPK = org.PK;
			using (var form = new ZForm(order))
			using (var grid = new OrderLinesButtonGrid())
			{
				grid.BindToGridList = "OrderLines";
				form.Controls.Add(grid);
				form.Show();
				grid.InnerGrid.SetDataBinding(order, "OrderLines");
				var columnStyle = grid.InnerGrid.GetColumnStyle(JobOrderLineSchema.JO_RH_NKCommodityCode.Name) as ZCodeFindBoxColumnStyleInfo;
				AssertNotNull("Commonity Code column should exist", columnStyle);
				AssertEquals("Visibility", true, columnStyle.IsVisible);
			}
		}

		#endregion

		[RequiresSTA]
		public void TestCustomFieldCaptions()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.OM_IMPartAttrib1Name = "Zubs";
			org.MiscServ.OM_IMPartAttrib2Name = "Rakhsh";
			org.MiscServ.OM_IMPartAttrib3Name = "Zayd";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var order = Factory.New<Order>();
			var line = order.OrderLines.AddNew();

			order.BuyerPK = org.PK;
			using (var form = new ZForm(order))
			using (var grid = new OrderLinesButtonGrid())
			{
				grid.BindToGridList = "OrderLines";
				form.Controls.Add(grid);
				form.Show();
				grid.InnerGrid.SetDataBinding(order, "OrderLines");

				AssertEquals("Zubs", grid.InnerGrid.GetColumnCaption(JobOrderLineSchema.JO_PartAttrib1.Name));
				AssertEquals("Rakhsh", grid.InnerGrid.GetColumnCaption(JobOrderLineSchema.JO_PartAttrib2.Name));
				AssertEquals("Zayd", grid.InnerGrid.GetColumnCaption(JobOrderLineSchema.JO_PartAttrib3.Name));
			}

			order.BuyerPK = org2.PK;
			using (var form = new ZForm(order))
			using (var grid = new OrderLinesButtonGrid())
			{
				grid.BindToGridList = "OrderLines";
				form.Controls.Add(grid);
				form.Show();
				grid.InnerGrid.SetDataBinding(order, "OrderLines");

				AssertEquals("Part Attrib. 1", grid.InnerGrid.GetColumnCaption(JobOrderLineSchema.JO_PartAttrib1.Name));
				AssertEquals("Part Attrib. 2", grid.InnerGrid.GetColumnCaption(JobOrderLineSchema.JO_PartAttrib2.Name));
				AssertEquals("Part Attrib. 3", grid.InnerGrid.GetColumnCaption(JobOrderLineSchema.JO_PartAttrib3.Name));
			}

			var proxyOrg = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			proxyOrg.MiscServ.OM_IMPartAttrib1Name = "HA";
			proxyOrg.MiscServ.OM_IMPartAttrib2Name = "HO";
			proxyOrg.MiscServ.OM_IMPartAttrib3Name = "WOW";
			Factory.Save();

			order.BuyerPK = org2.PK;
			using (var form = new ZForm(order))
			using (var grid = new OrderLinesButtonGrid())
			{
				grid.BindToGridList = "OrderLines";
				form.Controls.Add(grid);
				form.Show();
				grid.InnerGrid.SetDataBinding(order, "OrderLines");

				AssertEquals("HA", grid.InnerGrid.GetColumnCaption(JobOrderLineSchema.JO_PartAttrib1.Name));
				AssertEquals("HO", grid.InnerGrid.GetColumnCaption(JobOrderLineSchema.JO_PartAttrib2.Name));
				AssertEquals("WOW", grid.InnerGrid.GetColumnCaption(JobOrderLineSchema.JO_PartAttrib3.Name));
			}

			org2.MiscServ.OM_IMPartAttrib1Name = "AA";
			org2.MiscServ.OM_IMPartAttrib2Name = "BB";
			org2.MiscServ.OM_IMPartAttrib3Name = "CC";
			order.BuyerPK = org2.PK;
			using (var form = new ZForm(order))
			using (var grid = new OrderLinesButtonGrid())
			{
				grid.BindToGridList = "OrderLines";
				form.Controls.Add(grid);
				form.Show();
				grid.InnerGrid.SetDataBinding(order, "OrderLines");

				AssertEquals("AA", grid.InnerGrid.GetColumnCaption(JobOrderLineSchema.JO_PartAttrib1.Name));
				AssertEquals("BB", grid.InnerGrid.GetColumnCaption(JobOrderLineSchema.JO_PartAttrib2.Name));
				AssertEquals("CC", grid.InnerGrid.GetColumnCaption(JobOrderLineSchema.JO_PartAttrib3.Name));
			}
		}

		public void TestColourDeciding()
		{
			Order order = Factory.New<Order>();
			OrderLine line1 = order.OrderLines.AddNew();
			OrderLine line2 = order.OrderLines.AddNew();
			line2.JO_LineStatus = Core.Constants.OrderStatus.Cancelled;

			using (ZForm form = new ZForm(order))
			{
				using (OrderLinesButtonGrid grid = new OrderLinesButtonGrid())
				{
					grid.BindToGridList = "OrderLines";
					form.Controls.Add(grid);
					form.Show();
					grid.InnerGrid.SetDataBinding(order, "OrderLines");

					AssertEquals("GetCustomRowBackgroundColour(0)", Color.Empty, grid.InnerGrid.GetType().GetMethod("GetCustomRowBackgroundColour", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(int) }, null).Invoke(grid.InnerGrid, new object[] { 0 }));
					AssertEquals("GetCustomRowBackgroundColour(0)", Color.Orange, grid.InnerGrid.GetType().GetMethod("GetCustomRowBackgroundColour", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(int) }, null).Invoke(grid.InnerGrid, new object[] { 1 }));
				}
			}
		}

		public void TestControllerIsOrderLinesFromOrderController()
		{
			using (DummyOrderLinesButtonGrid grid = new DummyOrderLinesButtonGrid())
			{
				Order order = Factory.New<Order>();
				OrderLine line = order.OrderLines.AddNew();

				ZController controller = grid.GetNewControllerCore(line);
				AssertEquals(ControllerIDs.OrderLineFromOrder, controller.ID);
			}
		}

		public void TestDeleteOrderLineEvent()
		{
			OrdersDataRegistry.Instance.AllowExportedOrderLinesToBeDeleted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			Order order = Factory.NewWithValidTestData<Order>();
			order.Logs.AddNew(Events.DataExport);
			OrderLine line1 = order.OrderLines.AddNew();
			Factory.Save();

			using (ZForm form = new ZForm(order))
			{
				using (OrderLinesButtonGrid grid = new OrderLinesButtonGrid())
				{
					grid.BindToGridList = "OrderLines";
					form.Controls.Add(grid);
					form.Show();
					grid.SetDataBinding(order, "OrderLines");
					grid.InnerGrid.SetDataBinding(order, "OrderLines");

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					order.OrderLines.Delete(line1);
					AssertEquals("This Order Line has been exported to a customer system and therefore cannot be deleted. \r\n\r\nIf this order was incorrectly attached to this pre-advice, you can choose 'Yes' which will create a new unallocated Order Split that can then be attached to the correct shipment pre-advice.\r\n\r\nYou can turn off this behavior and allow deletes by modifying the Registry value located at Orders --> Allow Exported Order Lines To Be Deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull(grid.LastShownSplitForm);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					order.OrderLines.Delete(line1);
					AssertEquals("This Order Line has been exported to a customer system and therefore cannot be deleted. \r\n\r\nIf this order was incorrectly attached to this pre-advice, you can choose 'Yes' which will create a new unallocated Order Split that can then be attached to the correct shipment pre-advice.\r\n\r\nYou can turn off this behavior and allow deletes by modifying the Registry value located at Orders --> Allow Exported Order Lines To Be Deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(typeof(OrdersForm), grid.LastShownSplitForm.GetType());
					grid.LastShownSplitForm.Dispose();
				}
			}
		}

		public void TestOrderSplitting_MaxJD_OrderNumberSplit()
		{
			using (OrdersDataRegistry.Instance.AllowExportedOrderLinesToBeDeleted.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var order = Factory.NewWithValidTestData<Order>();
				order.JD_OrderNumberSplit = Byte.MaxValue;
				order.Logs.AddNew(Events.DataExport);

				var line = order.OrderLines.AddNew();
				Factory.Save();

				using (var form = new ZForm(order))
				{
					using (var grid = new OrderLinesButtonGrid())
					{
						grid.BindToGridList = "OrderLines";
						form.Controls.Add(grid);
						form.Show();
						grid.SetDataBinding(order, "OrderLines");
						grid.InnerGrid.SetDataBinding(order, "OrderLines");

						UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
						order.OrderLines.Delete(line);
						AssertEquals("This Order Line has been exported to a customer system and therefore cannot be deleted. \r\n\r\nIf this order was incorrectly attached to this pre-advice, you can choose 'Yes' which will create a new unallocated Order Split that can then be attached to the correct shipment pre-advice.\r\n\r\nYou can turn off this behavior and allow deletes by modifying the Registry value located at Orders --> Allow Exported Order Lines To Be Deleted.", UnitTestUserNotification.Instance.LastMessage.Text);

						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						order.OrderLines.Delete(line);
						AssertEquals("The order has been split the maximum number of times. This order will not be split.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestQuantityBookedAndOpenQuantity_InDesignerWithRegistrySettingOn_ColumnsExist()
		{
			AssertQuantityBookedAndOpenQuantity_PresentOnCondition(isSupplierBookingEnabled: true, isDesigning: true, shouldBePresent: true);
		}

		public void TestQuantityBookedAndOpenQuantity_InDesignerWithRegistrySettingOff_ColumnsExist()
		{
			AssertQuantityBookedAndOpenQuantity_PresentOnCondition(isSupplierBookingEnabled: false, isDesigning: true, shouldBePresent: true);
		}

		public void TestQuantityBookedAndOpenQuantity_WhenEnableAdvOrmFeatureIsOn_ColumnsExist()
		{
			AssertQuantityBookedAndOpenQuantity_PresentOnCondition(isSupplierBookingEnabled: true, isDesigning: false, shouldBePresent: true);
		}

		public void TestQuantityBookedAndOpenQuantity_WhenEnableAdvOrmFeatureIsOff_ColumnsDoNotExist()
		{
			AssertQuantityBookedAndOpenQuantity_PresentOnCondition(isSupplierBookingEnabled: false, isDesigning: false, shouldBePresent: false);
		}

		#region Implementation

		void AssertQuantityBookedAndOpenQuantity_PresentOnCondition(bool isSupplierBookingEnabled, bool isDesigning, bool shouldBePresent)
		{
			try
			{
				AdvOrmFeatureHelper.RunTestWith(isSupplierBookingEnabled, action: () =>
				{
					DesignModeFinder.SetIsDesigningForTest(isDesigning);
					using (var grid = new OrderLinesButtonGrid())
					{
						var qtyBookedColumnStyle = grid.InnerGrid.GetColumnStyle("JO_QtyBooked");
						var qtyOpenColumnStyle = grid.InnerGrid.GetColumnStyle("JO_OpenQuantity");

						CombineAssertions(() =>
						{
							AssertEquals(qtyBookedColumnStyle != null, shouldBePresent);
							AssertEquals(qtyOpenColumnStyle != null, shouldBePresent);
						});
					}
				});
			}
			finally
			{
				DesignModeFinder.SetIsDesigningForTest(false);
			}
		}

		class OrderLinesButtonGridForTest : OrderLinesButtonGrid
		{
			public ZToolStripButton EditButton
			{
				get
				{
					var toolStrip = Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
					return toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Edit, true).OfType<ZToolStripButton>().First();
				}
			}
		}

		class OrderLinesButtonGridControl : ZUserControl
		{
			public OrderLinesButtonGridControl()
			{
				InitializeComponent();
			}

			void InitializeComponent()
			{
				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo() { ColumnName = "zTextBoxColumnStyleInfo1" };
				this.OrderLinesButtonGrid = new OrderLinesButtonGridForTest();

				this.BindingSource.DataSourceType = typeof(Order);

				OrderLinesButtonGrid.BindToGridList = "OrderLines";
				OrderLinesButtonGrid.BindToFindBoxList = "OrderLines";
				OrderLinesButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

				this.Controls.Add(OrderLinesButtonGrid);
			}

			internal OrderLinesButtonGridForTest OrderLinesButtonGrid;
		}

		class DummyOrderLinesButtonGrid : OrderLinesButtonGrid
		{
			public new ZController GetNewControllerCore(BusinessObject selectedOrderLine)
			{
				return base.GetNewControllerCore(selectedOrderLine);
			}
		}

		[TestedType(typeof(OrderLinesButtonGrid))]
		class OrderLinesButtonGridModuleButtonGridTest : ZArchitecture.GUI.Testing.ZModuleButtonGridTestBase
		{
		}

		#endregion
	}
}
