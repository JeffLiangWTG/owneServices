using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class GenericOrdersModuleButtonGrid : ZModuleButtonGrid
	{
		public GenericOrdersModuleButtonGrid()
		{
			InitializeComponent();
			ShowAttachButton = false;
			ShowDetachButton = false;
			ShowEditButton = false;
			ShowNewButton = false;
		}

		#region SetupToolStripItems

		protected override void InitializeButtons()
		{
			this.DetachOrderButton = new ZToolStripButton();
			this.NewOrderSplitButton = new ZToolStripSplitButton();
			this.AttachOrderSplitButton = new ZToolStripSplitButton();
			this.EditOrderButton = new ZToolStripButton();

			toolStrip?.Dispose();

			toolStrip = new ZToolStrip
			{
				Name = "toolStrip",
				GripStyle = ToolStripGripStyle.Hidden,
				Anchor = AnchorStyles.Left,
				TabStop = false,
				BackColor = Color.Transparent
			};
			mainLayoutPanel.Controls.Add(toolStrip, 0, 1);
			toolStrip.Items.AddRange(new ToolStripItem[]
			{
				NewOrderSplitButton,
				EditOrderButton,
				AttachOrderSplitButton,
				DetachOrderButton
			});
		}

		#endregion

		#region Resize

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (Visible)
			{
				ResizeGrid();
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			ResizeGrid();
		}

		void ResizeGrid()
		{
			if (toolStrip != null)
			{
				ControlDpiScalingHelper.SetHeight(InnerGrid, Height - this.toolStrip.Height, false);
			}
		}

		#endregion

		#region AttachedOrders

		GenericOrderCollection AttachedOrders
		{
			get { return ((IAttachGenericOrders)DataSource).GenericOrders; }
		}

		protected override ZController GetNewControllerCore(BusinessObject selected)
		{
			return IsDetaching ? null : selected == null ? base.GetNewControllerCore(selected) : ZControllerFactory.Create(((IAttachedOrder)selected).ControllerID);
		}

		#endregion

		#region New

		protected override void NewButton_Click(object sender, EventArgs e)
		{
			if (DataSource != null)
			{
				var attachedOrders = AttachedOrders;
				attachedOrders.CurrentModuleID = ModuleId.Orders;
				ModuleID = ModuleIDs.Orders;
				base.NewButton_Click(sender, e);
			}
		}

		protected override bool ShouldAddObjectToCollection(BusinessObject objectToAdd)
		{
			if (objectToAdd is Order order
				&& DataSource is ForwardingShipment shipment
				&& shipment.IsInDatabase
				&& order.JD_JS != shipment.PK)
			{
				return false;
			}

			return base.ShouldAddObjectToCollection(objectToAdd);
		}

		void NewWarehouseOrderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var shipment = DataSource as ForwardingShipment;
			if (shipment != null)
			{
				var publishResult = ForwardingShipmentToWarehouseOrderSender.CreateWarehouseOrderFromForwardingShipment(shipment);
				switch (publishResult.ResultType)
				{
					case UniversalResult.HadErrors:
						var caption = Res.GetString("1a475ec8-332f-49ec-8e46-c6264d3784e5", "Create Order");
						Globals.Message.ShowError(publishResult.ErrorMessage, caption);
						break;
					case UniversalResult.Internal:
						var whsOrder = publishResult.FindJobIfExists();
						if (whsOrder != null)
						{
							var whsOrderInShipmentFactory = LoadOrderAndRelatedPivotInShipmentFactory(whsOrder, shipment.Factory);

							AttachedOrders.Add(whsOrderInShipmentFactory);
							ShowWarehouseOrderForm(whsOrderInShipmentFactory);
						}
						break;
					case UniversalResult.External:
						throw new InvalidOperationException("This should never happen");
					default:
						throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Result type {0} is not supported.", publishResult.ResultType));
				}
			}
		}

		[SuppressMessage("Microsoft.Performance", "CA1804:Remove unused locals")]
		BusinessObject LoadOrderAndRelatedPivotInShipmentFactory(BusinessObject order, BusinessObjectFactory shipmentFactory)
		{
			var orderInShipmentFactory = shipmentFactory.Load(ObjectFactory.GetType<IWhsOrder>(), order.PK);
			var forceReloadPivotInShipmentFactory = shipmentFactory.Load(ObjectFactory.GetType<IWhsDocketJobPivot>(),
				new ZQuery(WhsDocketJobPivotSchema.WV_WD_Docket, orderInShipmentFactory.PK));

			return orderInShipmentFactory;
		}

		void ShowWarehouseOrderForm(BusinessObject businessObject)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.WhsOrder);
			SetupNewController(controller);

			var form = controller.ShowEditForm(businessObject);
			if (form != null)
			{
				LastShownZForm = form;
			}
		}

		#endregion

		#region Edit

		protected override void EditButton_Click(object sender, EventArgs e)
		{
			if (DataSource != null)
			{
				base.EditButton_Click(sender, e);
			}
		}

		protected override void Edit(BusinessObject selected, object sender)
		{
			var attachedOrders = AttachedOrders;
			var attachedJob = (IAttachedOrder)selected;
			attachedOrders.CurrentModuleID = (ModuleId)attachedJob.ModuleID.ID;
			base.Edit(selected, sender);
		}

		protected override bool AllowDoubleClick
		{
			get { return EditOrderButton.Visible && EditOrderButton.Enabled; }
		}

		#endregion

		#region Attach

		protected override void AttachButton_Click(object sender, EventArgs e)
		{
			if (DataSource != null)
			{
				AttachedOrders.CurrentModuleID = ModuleId.Orders;
				ModuleID = ModuleIDs.Orders;
				base.AttachButton_Click(sender, e);
			}
		}

		protected override bool LogErrorIfFindBoxListNull => false;

		void AttachWarehouseOrderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (DataSource != null)
			{
				AttachedOrders.CurrentModuleID = ModuleId.WhsOrder;
				ModuleID = ModuleIDs.WhsOrder;
				base.AttachButton_Click(sender, e);
			}
		}

		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return base.GetNewRecordAttacher(destinationCollection, AttachedOrders.CurrentBindingList, moduleID);
		}

		#endregion

		#region Detach

		protected override void DetachButton_Click(object sender, EventArgs e)
		{
			if (DataSource != null)
			{
				using (new DisposableAction(() => IsDetaching = true, () => IsDetaching = false))
				{
					base.DetachButton_Click(sender, e);
				}
			}
		}

		bool IsDetaching;

		#endregion
	}
}
