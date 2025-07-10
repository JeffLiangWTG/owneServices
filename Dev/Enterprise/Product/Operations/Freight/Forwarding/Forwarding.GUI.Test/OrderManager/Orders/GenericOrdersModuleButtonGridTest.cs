using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.GUI;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class GenericOrdersModuleButtonGridTest : TestCaseWithFactory
	{
		public void TestEditOrderLicenceCheckpoint()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var order = Factory.New<Order>();
			order.BuyerPK = org.PK;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.GenericOrders.Add(order);
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			Factory.Save();

			using (var module = ModuleTree.Tree.FindByID(ModuleIDs.JobShipment.Name).CreateZModule())
			using (var form = (ZForm)((IFilterModuleInternalsForTesting)module).ShowViewForm(shipment))
			{
				AssertEditLicenceCheckpoint(form, Env.Licence.OrderManager);
			}
		}

		public void TestEditWarehouseOrderLicenceCheckpoint()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = helper.CreateWarehouse("WHS", "A");
			warehouse[WhsWarehouseSchema.WW_OA_WarehouseAddress] = org.MainAddress.PK;
			warehouse[WhsWarehouseSchema.WW_GB_RelatedCompanyBranch] = GlbBranch.CurrentBranch.PK;
			var warhouseOrder = (BusinessObject)Factory.New<IWhsOrder>();
			warhouseOrder[WhsDocketSchema.WD_OH_Client] = org.PK;
			warhouseOrder[WhsDocketSchema.WD_WW_Whs] = warehouse.PK;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.GenericOrders.Add(warhouseOrder);
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			Factory.Save();

			using (var module = ModuleTree.Tree.FindByID(ModuleIDs.JobShipment.Name).CreateZModule())
			using (var form = (ZForm)((IFilterModuleInternalsForTesting)module).ShowViewForm(shipment))
			{
				AssertEditLicenceCheckpoint(form, Env.Licence.WarehouseManagerOperationsAnd3PL);
			}
		}

		void AssertEditLicenceCheckpoint(ZForm parentForm, LicenceCheckpoint expected, params LicenceCheckpoint[] notExpectedCheckpoints)
		{
			using (var userControl = new GenericOrdersModuleButtonGridControl())
			{
				parentForm.Controls.Add(userControl);
				AssertEquals("Precondition: Grid.Form", parentForm, userControl.GenericOrdersModuleButtonGrid.Form_Exposed);

				var moduleButtonGrid = userControl.GenericOrdersModuleButtonGrid;
				moduleButtonGrid.SelectFirstRowIfOnlyRowInGrid();
				moduleButtonGrid.EditButton.PerformClick();

				AssertNotNull("LastShownZForm", moduleButtonGrid.LastShownZForm);
				using (var shownForm = (ZForm)moduleButtonGrid.LastShownZForm)
				{
					CombineAssertions(() =>
						{
							AssertEquals("ContainsCheckpoint(" + expected.Name + ")", true, shownForm.LicensedComponentManager.ContainsCheckpoint(expected));
							foreach (var notExpectedCheckpoint in notExpectedCheckpoints)
							{
								AssertEquals("ContainsCheckpoint(" + notExpectedCheckpoint.Name + ")", false, shownForm.LicensedComponentManager.ContainsCheckpoint(notExpectedCheckpoint));
							}
						});
				}
			}
		}

		public void TestButtons()
		{
			IAttachGenericOrders shipment = Factory.New<ForwardingShipment>();
			using (var form = new ZForm(shipment))
			using (var genericOrdersModuleButtonGrid = new GenericOrdersModuleButtonGridForTest())
			{
				form.Controls.Add(genericOrdersModuleButtonGrid);

				genericOrdersModuleButtonGrid.BindToGridList = "GenericOrders";
				genericOrdersModuleButtonGrid.InnerGrid.DataSource = shipment;

				var dummyColumn = new ZTextBoxColumnStyleInfo();
				dummyColumn.CaptionResourceString = Res.GetData("1f1e43e2-37ac-404d-8ca8-1f05baa2de20", "Job Number");
				dummyColumn.ColumnName = "JobNo";
				genericOrdersModuleButtonGrid.ColumnStyles.Add(dummyColumn);

				genericOrdersModuleButtonGrid.InnerGrid.SetDataBinding(shipment, "GenericOrders");

				form.Show();

				AssertEquals(false, genericOrdersModuleButtonGrid.ShowAttachButton);
				AssertEquals(false, genericOrdersModuleButtonGrid.ShowDetachButton);
				AssertEquals(false, genericOrdersModuleButtonGrid.ShowEditButton);
				AssertEquals(false, genericOrdersModuleButtonGrid.ShowNewButton);

#pragma warning disable IDE0200 // Cannot replace by grouping here, this is not the same
				using (new DisposableAction(() => genericOrdersModuleButtonGrid.LastShownZForm.Dispose()))
#pragma warning restore IDE0200
				{
					genericOrdersModuleButtonGrid.NewButton.PerformButtonClick();
					AssertEquals(typeof(OrdersForm), genericOrdersModuleButtonGrid.LastShownZForm.GetType());
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				genericOrdersModuleButtonGrid.NewWarehouseOrderButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Create Order", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertMultilineASCIIEquals("Should try to create Warehouse Order", @"Failed to create Warehouse Order:
Error - Cannot Import Order
No Client Address was provided.
No Warehouse was provided.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				genericOrdersModuleButtonGrid.EditButton.PerformClick();
				AssertEquals("Please select an item in the grid by first clicking in the left-hand gutter to highlight the whole row.", UnitTestUserNotification.Instance.LastMessage.Text);

				int attachHitCount = 0;
				genericOrdersModuleButtonGrid.Attaching += (sender, e) => attachHitCount++;

#pragma warning disable IDE0200 // Cannot replace by grouping here, this is not the same
				using (new DisposableAction(() => genericOrdersModuleButtonGrid.LastShownAttachPopupForTesting.Dispose()))
#pragma warning restore IDE0200 
				{
					genericOrdersModuleButtonGrid.AttachButton.PerformButtonClick();
					AssertEquals(1, attachHitCount);
				}

#pragma warning disable IDE0200 // Cannot replace by grouping here, this is not the same
				using (new DisposableAction(() => genericOrdersModuleButtonGrid.LastShownAttachPopupForTesting.Dispose()))
#pragma warning restore IDE0200
				{
					genericOrdersModuleButtonGrid.AttachedWarehouseOrderButton.PerformClick();
					AssertEquals(2, attachHitCount);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				genericOrdersModuleButtonGrid.DetachButton.PerformClick();
				AssertEquals("Please select an item in the grid by first clicking in the left-hand gutter to highlight the whole row.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			ErrorReporter.Clear();
		}

		[NUnit.Framework.ExpectNoExceptions]
		public void TestButtonClicksThrowNoExceptionOnEmptyDataSource()
		{
			using (var genericOrdersModuleButtonGrid = new GenericOrdersModuleButtonGridForTest())
			{
				genericOrdersModuleButtonGrid.NewButton.PerformButtonClick();
				genericOrdersModuleButtonGrid.EditButton.PerformClick();
				genericOrdersModuleButtonGrid.AttachButton.PerformButtonClick();
				genericOrdersModuleButtonGrid.DetachButton.PerformClick();
			}
		}

		public void TestNewWarehouseOrder_NoUnexpectedConcurrencyWarningsWhenDetachWithoutSave()
		{
			var whsClient = Factory.NewWithValidTestData<OrgHeader>();
			whsClient.OH_IsWarehouseClient = true;
			whsClient.OH_IsConsignor = true;

			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = helper.CreateWarehouse("WHS", "A");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsignorPK = whsClient.PK;
			shipment.DocsAndCartage.JP_OrderItemsAsString = "Holmes";

			Factory.Save();

			using (var form = new ZForm(shipment))
			using (var genericOrdersModuleButtonGrid = new GenericOrdersModuleButtonGridForTest())
			{
				var dummyColumn = new ZTextBoxColumnStyleInfo();
				dummyColumn.CaptionResourceString = Res.GetData("1f1e43e2-37ac-404d-8ca8-1f05baa2de20", "Job Number");
				dummyColumn.ColumnName = "JobNo";

				genericOrdersModuleButtonGrid.BindToGridList = "GenericOrders";
				genericOrdersModuleButtonGrid.ColumnStyles.Add(dummyColumn);
				genericOrdersModuleButtonGrid.InnerGrid.DataSource = shipment;
				genericOrdersModuleButtonGrid.InnerGrid.SetDataBinding(shipment, "GenericOrders");

				form.Controls.Add(genericOrdersModuleButtonGrid);

				AssertEquals("Precondition: Shipment should not have orders yet", 0, shipment.AttachedWarehouseOrders.Count);

#pragma warning disable IDE0200 // Cannot replace by grouping here, this is not the same
				using (new DisposableAction(() => genericOrdersModuleButtonGrid.LastShownZForm.Dispose()))
#pragma warning restore IDE0200
				{
					genericOrdersModuleButtonGrid.NewWarehouseOrderButton.PerformClick();
					AssertEquals("Precondition: Shipment should now have one warehouse order", 1, shipment.AttachedWarehouseOrders.Count);

					genericOrdersModuleButtonGrid.SelectFirstRowIfOnlyRowInGrid();
					genericOrdersModuleButtonGrid.DetachButton.PerformClick();
					AssertStartsWith("Precondition: Detach message is shown as last message",
						"Are you sure you want to detach the selected records?", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}

				using (var saveAndCloseButton = new ZButton())
#pragma warning disable IDE0200 // Cannot replace by grouping here, this is not the same
				using (new DisposableAction(() => genericOrdersModuleButtonGrid.LastShownZForm.Dispose()))
#pragma warning restore IDE0200
				{
					ZFormPostingButtonsStrategy.SetupPosting(form, saveAndCloseButton, null);
					form.DisplayMode = ODisplayMode.Delete;
					form.Show();

					saveAndCloseButton.PerformClick();
					Application.DoEvents();

					Assert("There should be no redundant concurrency warning", UnitTestUserNotification.Instance.LastMessage.WasNone);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		public void TestNewWarehouseOrder_LoadsOrderAndRelatedPivotToShipmentFactory_NoDuplicates()
		{
			var whsClient = Factory.NewWithValidTestData<OrgHeader>();
			whsClient.OH_IsWarehouseClient = true;

			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = helper.CreateWarehouse("WHS", "A");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsignorPK = whsClient.PK;

			Factory.Save();

			using (var form = new ZForm(shipment))
			using (var genericOrdersModuleButtonGrid = new GenericOrdersModuleButtonGridForTest())
			{
				var dummyColumn = new ZTextBoxColumnStyleInfo();
				dummyColumn.CaptionResourceString = Res.GetData("1f1e43e2-37ac-404d-8ca8-1f05baa2de20", "Job Number");
				dummyColumn.ColumnName = "JobNo";

				genericOrdersModuleButtonGrid.BindToGridList = "GenericOrders";
				genericOrdersModuleButtonGrid.ColumnStyles.Add(dummyColumn);
				genericOrdersModuleButtonGrid.InnerGrid.DataSource = shipment;
				genericOrdersModuleButtonGrid.InnerGrid.SetDataBinding(shipment, "GenericOrders");

				form.Controls.Add(genericOrdersModuleButtonGrid);

				AssertEquals("Precondition: Shipment should not have orders yet", 0, shipment.GenericOrders.Count);
				AssertEquals("Precondition: Shipment should not have orders yet", 0, shipment.AttachedWarehouseOrders.Count);

#pragma warning disable IDE0200 // Cannot replace by grouping here, this is not the same
				using (new DisposableAction(() => genericOrdersModuleButtonGrid.LastShownZForm.Dispose()))
#pragma warning restore IDE0200
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					genericOrdersModuleButtonGrid.NewWarehouseOrderButton.PerformClick();
				}

				AssertEquals("Shipment should now have one warehouse order", 1, shipment.GenericOrders.Count);
				AssertEquals("Shipment should now have one warehouse order", 1, shipment.AttachedWarehouseOrders.Count);

				var whsOrder = shipment.AttachedWarehouseOrders[0] as BusinessObject;
				var query = new ZQuery(WhsDocketJobPivotSchema.WV_WD_Docket, whsOrder.PK);
				query.FetchOnlyFromLocalCache = true;

				var pivotInLocalFactoryCache = (BusinessObject)Factory.Load<IWhsDocketJobPivot>(query).Single();

				Assert("The single pivot found in cache should also be in database, not a locally created pivot", pivotInLocalFactoryCache.IsInDatabase);
			}
		}

		public void TestNewWarehouseOrder_NotCreatedIfOrderLimitExceededOnShipment()
		{
			const string errorMessage = @"Failed to create Warehouse Order:
The number of Orders on a Shipment is limited for performance and database management reasons to 2 Orders. Above 1 Orders you will receive this message for every additional Order added. For XML imports the system will fail the import if this limit is exceeded.

If your company needs larger numbers of Shipments WiseTech Global provides an alternative method of operation that allows for a very large number of Shipments on a Master House Shipment (we call this the HVLV system or High Volume Low Value Shipment system). If you need these higher volumes (as much as 20,000 Shipments on a Manifest) contact your account manager to discuss.";

			using (FreightDataRegistry.Instance.OrdersPerShipmentLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			using (FreightDataRegistry.Instance.OrdersPerShipmentLimitIntroductionTimeUTC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.Now.AddDays(-1)))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "BACON PANCAKES";

				var order1 = shipment.AttachedOrders.AddNew();
				var order2 = shipment.AttachedOrders.AddNew();
				order1.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
				order2.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

				var whsOrder = (BusinessObject)Factory.New<IWhsOrder>();
				whsOrder.FillWithValidTestData();
				whsOrder["WD_BOLNo"] = "BACON PANCAKES";
				Factory.Save();

				IAttachGenericOrders iAttachGenericOrders = shipment;
				using (var form = new ZForm(shipment))
				using (var genericOrdersModuleButtonGrid = new GenericOrdersModuleButtonGridForTest())
				{
					form.Controls.Add(genericOrdersModuleButtonGrid);

					genericOrdersModuleButtonGrid.BindToGridList = "GenericOrders";
					genericOrdersModuleButtonGrid.InnerGrid.DataSource = iAttachGenericOrders;

					var dummyColumn = new ZTextBoxColumnStyleInfo();
					dummyColumn.CaptionResourceString = Res.GetData("1f1e43e2-37ac-404d-8ca8-1f05baa2de20", "Job Number");
					dummyColumn.ColumnName = "JobNo";
					genericOrdersModuleButtonGrid.ColumnStyles.Add(dummyColumn);
					genericOrdersModuleButtonGrid.InnerGrid.SetDataBinding(iAttachGenericOrders, "GenericOrders");

					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					genericOrdersModuleButtonGrid.NewWarehouseOrderButton.PerformClick();
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("Create Order", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("Should not try to create Warehouse Order as it will exceed the Order Limit.", errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestOrderLinkage_ShipmentIsInDatabase()
		{
			var shipment = Factory.New<ForwardingShipment>();

			Factory.Save();

			using (var form = new ZForm(shipment))
			using (var genericOrdersModuleButtonGrid = new GenericOrdersModuleButtonGridForTest())
			{
				form.Controls.Add(genericOrdersModuleButtonGrid);

				genericOrdersModuleButtonGrid.BindToGridList = "GenericOrders";
				genericOrdersModuleButtonGrid.InnerGrid.DataSource = shipment;

				var dummyColumn = new ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("1f1e43e2-37ac-404d-8ca8-1f05baa2de20", "Job Number"),
					ColumnName = "JobNo"
				};

				genericOrdersModuleButtonGrid.ColumnStyles.Add(dummyColumn);
				genericOrdersModuleButtonGrid.InnerGrid.SetDataBinding(shipment, "GenericOrders");

				form.Show();

#pragma warning disable IDE0200 // Cannot replace by grouping here, this is not the same
				using (new DisposableAction(() => genericOrdersModuleButtonGrid.LastShownZForm.Dispose()))
#pragma warning restore IDE0200
				{
					genericOrdersModuleButtonGrid.NewButton.PerformButtonClick();
					var orderForm = (ZForm)genericOrdersModuleButtonGrid.LastShownZForm;
					AssertEquals(shipment.PK, ((Order)orderForm.DataSource).JD_JS);
				}
			}
		}

		[RequiresSTA]
		public void TestOrderLinkage_ShipmentNotInDatabase()
		{
			var shipment = Factory.New<ForwardingShipment>();

			using (var form = new ZForm(shipment))
			using (var genericOrdersModuleButtonGrid = new GenericOrdersModuleButtonGridForTest())
			{
				form.Controls.Add(genericOrdersModuleButtonGrid);

				genericOrdersModuleButtonGrid.BindToGridList = "GenericOrders";
				genericOrdersModuleButtonGrid.InnerGrid.DataSource = shipment;

				var dummyColumn = new ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("1f1e43e2-37ac-404d-8ca8-1f05baa2de20", "Job Number"),
					ColumnName = "JobNo"
				};

				genericOrdersModuleButtonGrid.ColumnStyles.Add(dummyColumn);
				genericOrdersModuleButtonGrid.InnerGrid.SetDataBinding(shipment, "GenericOrders");

				form.Show();

#pragma warning disable IDE0200 // Cannot replace by grouping here, this is not the same
				using (new DisposableAction(() => genericOrdersModuleButtonGrid.LastShownZForm.Dispose()))
#pragma warning restore IDE0200
				{
					genericOrdersModuleButtonGrid.NewButton.PerformButtonClick();
					var orderForm = (ZForm)genericOrdersModuleButtonGrid.LastShownZForm;
					AssertEquals(ZGuid.Empty, ((Order)orderForm.DataSource).JD_JS);
				}
			}
		}

		public void TestOrderLinkage_LinkRemovedAffectsShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();

			Factory.Save();

			using (var form = new ZForm(shipment))
			using (var genericOrdersModuleButtonGrid = new GenericOrdersModuleButtonGridForTest())
			{
				form.Controls.Add(genericOrdersModuleButtonGrid);

				genericOrdersModuleButtonGrid.BindToGridList = "GenericOrders";
				genericOrdersModuleButtonGrid.InnerGrid.DataSource = shipment;

				var dummyColumn = new ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("1f1e43e2-37ac-404d-8ca8-1f05baa2de20", "Job Number"),
					ColumnName = "JobNo"
				};

				genericOrdersModuleButtonGrid.ColumnStyles.Add(dummyColumn);
				genericOrdersModuleButtonGrid.InnerGrid.SetDataBinding(shipment, "GenericOrders");

				form.Show();
#pragma warning disable IDE0200 // Cannot replace by grouping here, this is not the same
				using (new DisposableAction(() => genericOrdersModuleButtonGrid.LastShownZForm.Dispose()))
#pragma warning restore IDE0200
				{
					genericOrdersModuleButtonGrid.NewButton.PerformButtonClick();
					var orderForm = (ZForm)genericOrdersModuleButtonGrid.LastShownZForm;
					var order = (Order)orderForm.DataSource;

					AssertEquals("Order has shipment set", shipment.PK, order.JD_JS);

					order.FillWithValidTestData();
					order.JD_JS = ZGuid.Empty;
					order.Factory.Save();

					typeof(ZForm).InvokeMember("FireSaved", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, orderForm, null);

					AssertEquals("Shipment has order removed", 0, shipment.GenericOrders.Count);
					Assert("Shipment should not have any changes", !shipment.HasChanges);
				}
			}
		}

		#region Implementation

		class GenericOrdersModuleButtonGridControl : ZUserControl
		{
			public GenericOrdersModuleButtonGridControl()
			{
				InitializeComponent();
			}

			void InitializeComponent()
			{
				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
				this.GenericOrdersModuleButtonGrid = new GenericOrdersModuleButtonGridForTest();
				// 
				// BindingSource
				// 
				this.BindingSource.DataSourceType = typeof(ForwardingShipment);
				// 
				// GenericOrdersModuleButtonGrid
				// 
				this.BindingSource.SetBindingMember(this.GenericOrdersModuleButtonGrid, "GenericOrders");
				zTextBoxColumnStyleInfo1.ColumnName = "JobNo";
				this.GenericOrdersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
				//
				// GenericOrdersModuleButtonGridControl
				//
				this.Controls.Add(GenericOrdersModuleButtonGrid);
			}

			internal GenericOrdersModuleButtonGridForTest GenericOrdersModuleButtonGrid;
		}

		class GenericOrdersModuleButtonGridForTest : GenericOrdersModuleButtonGrid
		{
			public ZForm Form_Exposed
			{
				get { return Form; }
			}

			public ToolStripSplitButton NewButton
			{
				get { return (ToolStripSplitButton)typeof(GenericOrdersModuleButtonGrid).GetField("NewOrderSplitButton", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this); }
			}

			public ZToolStripMenuItem NewWarehouseOrderButton
			{
				get { return (ZToolStripMenuItem)typeof(GenericOrdersModuleButtonGrid).GetField("NewWarehouseOrderMenuItem", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this); }
			}

			public ToolStripButton EditButton
			{
				get { return (ToolStripButton)typeof(GenericOrdersModuleButtonGrid).GetField("EditOrderButton", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this); }
			}

			public ToolStripSplitButton AttachButton
			{
				get { return (ToolStripSplitButton)typeof(GenericOrdersModuleButtonGrid).GetField("AttachOrderSplitButton", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this); }
			}

			public ZToolStripMenuItem AttachedWarehouseOrderButton
			{
				get { return (ZToolStripMenuItem)typeof(GenericOrdersModuleButtonGrid).GetField("AttachWarehouseOrderMenuItem", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this); }
			}

			public ToolStripButton DetachButton
			{
				get { return (ToolStripButton)typeof(GenericOrdersModuleButtonGrid).GetField("DetachOrderButton", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this); }
			}
		}

		#endregion
	}
}
