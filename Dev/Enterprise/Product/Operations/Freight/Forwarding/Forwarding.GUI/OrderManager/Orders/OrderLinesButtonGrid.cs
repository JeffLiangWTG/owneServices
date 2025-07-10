using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class OrderLinesButtonGrid : ZModuleButtonGridWithoutColumnStylesSerialisation
	{
		public OrderLinesButtonGrid()
		{
			InitializeComponent();
			CreateQuantityBookedAndQuantityOpenColumns();
			InnerGrid.ColourDeciding += new EventHandler<ColourDecidingEventArgs>(InnerGrid_ColourDeciding);
			ModuleID = ModuleIDs.OrderLine;
			new UNDGDataItemFormManager(InnerGrid).Initialize();

			OrderTotals.AllowOverlap(mainLayoutPanel);
		}

		void SetupHSCodeEffectiveDate()
		{
			var tariffColumnStyleInfo = this.InnerGrid.GetColumnStyle(OrderLine.Schema.JO_HSCode) as TariffColumnStyleInfo;
			TariffFindHelper.AddDefaultPropertyToTariffControlWithEffectiveDate(tariffColumnStyleInfo, order.JD_SystemCreateTimeUtc);
		}

		void CreateQuantityBookedAndQuantityOpenColumns()
		{
			if (DesignModeFinder.IsDesigning || AdvOrmFeatureHelper.IsEnabled)
			{
				var zQuantityBookedCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
				zQuantityBookedCalcEditColumnStyleInfo.CaptionResourceString = Res.GetData("OrderLinesButtonGrid|c60a39e8-a299-4b68-b1c2-bf420013642b", "Qty Booked");
				zQuantityBookedCalcEditColumnStyleInfo.ColumnName = "JO_QtyBooked";
				zQuantityBookedCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
				zQuantityBookedCalcEditColumnStyleInfo.Decimals = 5;
				zQuantityBookedCalcEditColumnStyleInfo.IsReadOnly = true;
				ColumnStyles.Add(zQuantityBookedCalcEditColumnStyleInfo);

				var zQuantityOpenCalcEditColumnStyleInfo = new ZCalcEditColumnStyleInfo();
				zQuantityOpenCalcEditColumnStyleInfo.CaptionResourceString = Res.GetData("OrderLinesButtonGrid|206dafed-e775-4b6f-ba91-f500cf46bf71", "Qty Open");
				zQuantityOpenCalcEditColumnStyleInfo.ColumnName = "JO_OpenQuantity";
				zQuantityOpenCalcEditColumnStyleInfo.BindToDecimalPlaces = null;
				zQuantityOpenCalcEditColumnStyleInfo.Decimals = 5;
				zQuantityOpenCalcEditColumnStyleInfo.IsReadOnly = true;
				ColumnStyles.Add(zQuantityOpenCalcEditColumnStyleInfo);
			}
		}

		#region Color Deciding

		void InnerGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			if (((OrderLine)e.ObjectAtRow).IsLineStatusCancelled)
			{
				e.Colour = Color.Orange;
			}
		}

		#endregion

		#region Binding

		protected override ZController GetNewControllerCore(BusinessObject selected)
		{
			return ZControllerFactory.Create(ControllerIDs.OrderLineFromOrder);
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "It's okay, just checking to see if it's empty.")]
		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (order != null)
			{
				order.OrderLines.ExporterOrderLineDeleteAttempt -= OrderLines_ExporterOrderLineDeleteAttempt;
			}

			if (fGridLayoutPersister != null)
			{
				fGridLayoutPersister.Dispose();
			}

			order = dataSource as Order;
			AddWorkflowCustomFields(order);

			base.SetDataBinding(dataSource, dataMember);

			if (order != null)
			{
				order.OrderLines.ExporterOrderLineDeleteAttempt += OrderLines_ExporterOrderLineDeleteAttempt;
				fGridLayoutPersister = new CustomLabelsGridLayoutPersister(InnerGrid, OrderLine.NewCustomLabelsProvider(order));

				if (order.Buyer != null)
				{
					if (!order.Buyer.MiscServ.OM_IMPartAttrib1Name.IsEmpty)
					{
						InnerGrid.SetColumnCaption(JobOrderLineSchema.JO_PartAttrib1.Name, order.Buyer.PartAttributeManager.PartAttributeName1);
					}

					if (!order.Buyer.MiscServ.OM_IMPartAttrib2Name.IsEmpty)
					{
						InnerGrid.SetColumnCaption(JobOrderLineSchema.JO_PartAttrib2.Name, order.Buyer.PartAttributeManager.PartAttributeName2);
					}

					if (!order.Buyer.MiscServ.OM_IMPartAttrib3Name.IsEmpty)
					{
						InnerGrid.SetColumnCaption(JobOrderLineSchema.JO_PartAttrib3.Name, order.Buyer.PartAttributeManager.PartAttributeName3);
					}
				}

				this.BindingSource.DataSource = order;

				if (!DesignModeFinder.IsDesigning)
				{
					SetupHSCodeEffectiveDate();
				}
			}
		}

		void AddWorkflowCustomFields(Order order)
		{
			if (order != null && !hasAddedWorkflowCustomFields)
			{
				hasAddedWorkflowCustomFields = true;
				WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(InnerGrid, order.OrderLines, WorkflowDescriptors.OrderLineWorkflowDescriptorCode);
			}
		}
		bool hasAddedWorkflowCustomFields;

		Order order;

		#endregion

		#region Order Split

		void OrderLines_ExporterOrderLineDeleteAttempt(object sender, EventArgs e)
		{
			DialogResult result = Globals.Message.Show(Res.GetString("5b9f58c6-cef2-47fe-99c5-43e84bf86164", @"This Order Line has been exported to a customer system and therefore cannot be deleted. 

If this order was incorrectly attached to this pre-advice, you can choose 'Yes' which will create a new unallocated Order Split that can then be attached to the correct shipment pre-advice.

You can turn off this behavior and allow deletes by modifying the Registry value located at Orders --> Allow Exported Order Lines To Be Deleted."), Res.GetString("dd18711d-ff24-4689-b3c6-f134c9b9ee81", "Delete"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				if (order.IsNextOrderSplitNumberValid())
				{
					LastShownSplitForm = OrdersModule.ShowFormForSplit(order, CreateOrderType.Split);
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("579d5170-b03d-4051-9963-572b4f68ba5b", "The order has been split the maximum number of times. This order will not be split."));
				}
			}
		}

		public IZForm LastShownSplitForm;

		IOrdersModule OrdersModule
		{
			get { return fOrdersModule ?? (fOrdersModule = ((IOrdersModule)ZModuleFactory.Instance.Create(ModuleIDs.Orders))); }
		}

		IOrdersModule fOrdersModule;

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (fGridLayoutPersister != null)
				{
					fGridLayoutPersister.Dispose();
				}

				if (fOrdersModule != null)
				{
					((ZModule)fOrdersModule).Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
