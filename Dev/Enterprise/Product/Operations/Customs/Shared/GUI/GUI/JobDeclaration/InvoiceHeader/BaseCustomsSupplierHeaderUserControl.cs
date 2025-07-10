using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GUI
{
	public partial class BaseCustomsSupplierHeaderUserControl : ZUserControl
	{
		public static string IsGSTApplicableCaption
		{
			get { return Res.GetString("4cb156c6-1f30-46f3-b3bd-3fcecb7d47ff", "{0} Apply", GlbCompany.CurrentCompany.Country.ConsumptionTaxDescription); }
		}

		public CargoWise.Windows.UI.KPanel BottomPanel;

		public BaseCustomsSupplierHeaderUserControl()
		{
			InitializeComponent();
			BindingSource.SetBindingMember(InvCustomFieldsDisplayControl, nameof(BaseJobDeclaration.Invoices));

			JobComInvoiceHeadersBoundGrid.InnerGrid.CheckDatabaseAfterFirstBinding = true;
			JobComInvoiceHeadersBoundGrid.InnerGrid.OnRowsChangedInDatabase += (o, e) =>
			{
				Globals.Message.ShowError(Res.GetString("30f2276e-eb1c-4335-8132-bab18153a58d",
"While you were working, another user or process has updated this job.  Any changes made will not be able to be saved.  Please exit this job before continuing."));
			};
			if (!DesignModeFinder.IsDesigning)
			{
				InvoiceChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name, ColumnTitleForGSTApplies);
				ApportionedChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name, ColumnTitleForGSTApplies);
				BaseGroupChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name, ColumnTitleForGSTApplies);
			}

			JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = "GridLayoutUU5vFvxYnT4ITmuJkyIQgg==";

			ApportionmentPendingLabel.AllowOverlap(JZ_Calc_TNIBoundInvoiceCurrencyControl);
			ApportionmentPendingLabel.AllowOverlap(JZ_CIFAmountBoundCurrencyControl);
			ApportionmentPendingLabel.AllowOverlap(JZ_FOBAmountBoundCurrencyControl);
			ApportionmentPendingLabel.AllowOverlap(RightBottomPanel);
		}

		protected override void InitLayout()
		{
			base.InitLayout();
			ReorderCustomFieldsTab();
		}

		protected virtual string ColumnTitleForGSTApplies => IsGSTApplicableCaption;

		protected virtual string ColumnTitleWhenImportForDutiable => Res.GetString("d4449dcb-805f-4a98-8714-d455336483c8", "Dutiable");

		protected virtual string ColumnTitleWhenExportForDutiable => Res.GetString("12345678-1f30-46f3-b3bd-3fcecb7d47ff", "Add to FOB?");

		#region Business Entity

		public new BaseJobDeclaration CurrentDataItem
		{
			get { return (BaseJobDeclaration)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (CurrentDataItem != null)
			{
				CurrentDataItem.OnApportionmentDirtyChanged -= new BaseJobDeclaration.ApportionmentDirtyChangedEventHandler(CurrentDataItem_OnApportionmentDirtyChanged);
				CurrentDataItem_OnApportionmentDirtyChanged();
				CurrentDataItem.JE_MessageTypeInfo.ValueChanged -= CurrentDataItem_MessageTypeInfo_ValueChanged;
			}
		}

		void CurrentDataItem_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			var declaration = CurrentDataItem;
			var columnTitleForDutiable = declaration?.IsExport ?? false ? ColumnTitleWhenExportForDutiable : ColumnTitleWhenImportForDutiable;
			if (InvoiceChargesGrid.GetColumnStyle(JobComInvHeaderChargeSchema.J7_IsDutiable.Name) != null)
			{
				InvoiceChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsDutiable.Name, columnTitleForDutiable);
			}
			if (BaseGroupChargesGrid.GetColumnStyle(JobComInvHeaderChargeSchema.J7_IsDutiable.Name) != null)
			{
				BaseGroupChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsDutiable.Name, columnTitleForDutiable);
			}
			if (ApportionedChargesGrid.GetColumnStyle(JobComInvHeaderChargeSchema.J7_IsDutiable.Name) != null)
			{
				ApportionedChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsDutiable.Name, columnTitleForDutiable);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				CurrentDataItem.OnApportionmentDirtyChanged += new BaseJobDeclaration.ApportionmentDirtyChangedEventHandler(CurrentDataItem_OnApportionmentDirtyChanged);
				CurrentDataItem_OnApportionmentDirtyChanged();
				CurrentDataItem.JE_MessageTypeInfo.ValueChanged += CurrentDataItem_MessageTypeInfo_ValueChanged;
				CurrentDataItem_MessageTypeInfo_ValueChanged(this, e);
			}
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (CurrentDataItem != null)
			{
				CurrentDataItem_OnApportionmentDirtyChanged();
			}
		}

		#endregion

		#region Exposing Controls for unit tests

		public BaseInvoiceArrayBoundGrid InvoiceHeadersBoundGrid
		{
			get { return JobComInvoiceHeadersBoundGrid.InvoiceInnerGrid; }
		}

		#endregion

		#region Implementation

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				JobComInvoiceHeadersBoundGrid.InvoiceInnerGrid.AddColumnToSkip(BaseJobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice);
			}
		}

		protected void CurrentDataItem_OnApportionmentDirtyChanged()
		{
			if (CurrentDataItem != null)
			{
				ApportionmentPendingLabel.Visible = CurrentDataItem.ApportionmentDirty;
				if (CurrentDataItem.ApportionmentDirty)
				{
					ApportionmentPendingLabel.BringToFront();
					ApportionmentPendingLabel.Visible = true;
				}
				else
				{
					ApportionmentPendingLabel.SendToBack();
					ApportionmentPendingLabel.Visible = false;
				}

				if (CurrentDataItem is BaseJobDeclaration declaration)
				{
					var incoTermAndChargeFactory = declaration.IncoTermAndChargeFactory;
					var invoiceCharges = incoTermAndChargeFactory.GetChargeList(Common.ChargeParentTypes.Invoice);
					InvoiceChargesTabPage.TabVisible = invoiceCharges.Any();
				}
			}
		}

		#endregion

		#region Custom Fields Tab

		void ReorderCustomFieldsTab()
		{
			if (CustomFieldsTabPage != null)
			{
				InvoiceTabControl.Controls.Remove(CustomFieldsTabPage);
				InvoiceTabControl.Controls.Add(CustomFieldsTabPage);
			}
		}

		#endregion
	}
}
