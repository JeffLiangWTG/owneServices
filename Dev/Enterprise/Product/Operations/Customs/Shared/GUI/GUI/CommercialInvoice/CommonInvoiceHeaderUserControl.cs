using System;
using System.ComponentModel;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GUI
{
	public partial class CommonInvoiceHeaderUserControl : ZUserControl
	{
		public CommonInvoiceHeaderUserControl()
		{
			InitializeComponent();
			var zCheckBoxColumnStyleInfo3 = (ZCheckBoxColumnStyleInfo)InvoiceChargesGrid.GetColumnStyle(BaseInvoiceCharge.Schema.J7_IsIncludedInITOT);
			zCheckBoxColumnStyleInfo3.ToolTip = Res.GetString("InvoiceHeaderUserControl|C321A0A4-2E61-4949-B126-0846D5D3CD0A", "If you tick this flag, it indicates that this charge is included in Lines. It won\'t reduce the ITOT you have to enter");
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public BaseJobComInvoiceHeader Invoice
		{
			get
			{
				if (fInvoice == null)
				{
					ErrorReporter.ReportOnce("CommInvoiceNull", "Invoice property has not been set on BaseInvoiceHeaderUserControl. Please set this property in your form constructor.");
				}

				return fInvoice;
			}
			set { fInvoice = value; }
		}
		BaseJobComInvoiceHeader fInvoice;

		void JZ_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			MessageTypeChanged();
		}

		void MessageTypeChanged()
		{
			ChangeControlsVisibilityWhenMessageTypeChanges();
			ChangeInvoiceChargesGridTitlesWhenMessageTypeChanges();
		}

		protected virtual void ChangeControlsVisibilityWhenMessageTypeChanges()
		{
		}

		protected virtual void ChangeInvoiceChargesGridTitlesWhenMessageTypeChanges()
		{
			if (InvoiceChargesGrid.GetColumnStyle(JobComInvHeaderChargeSchema.J7_IsDutiable.Name) != null)
			{
				if (Invoice.IsImport)
				{
					InvoiceChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsDutiable.Name, ColumnTitleWhenImportForDutiable);
				}
				else
				{
					InvoiceChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsDutiable.Name, ColumnTitleWhenExportForDutiable);
				}
			}
			if (InvoiceChargesGrid.GetColumnStyle(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name) != null)
			{
				InvoiceChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name, ColumnTitleForGSTApplies);
			}
			InvoiceChargesGrid.RefreshTableStyles();
		}

		protected virtual string ColumnTitleWhenImportForDutiable
		{
			get { return Res.GetString("d4449dcb-805f-4a98-8714-d455336483c8", "Dutiable"); }
		}

		protected virtual string ColumnTitleWhenExportForDutiable
		{
			get { return Res.GetString("171f29cc-a4ae-400f-887a-a425037d51ec", "Add to FOB?"); }
		}

		protected virtual string ColumnTitleForGSTApplies => Invoice.IsImport ? BaseCustomsSupplierHeaderUserControl.IsGSTApplicableCaption : Res.GetString("98b702c1-d582-4e1e-b88b-62c0d98e44dc", "Add to CIF?");

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (!DesignModeFinder.IsDesigning)
			{
				MessageTypeChanged();
				if (Invoice != null)
				{
					Invoice.JZ_MessageTypeInfo.ValueChanged += new EventHandler(JZ_MessageTypeInfo_ValueChanged);
				}
			}
		}

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (Invoice != null)
			{
				Invoice.JZ_MessageTypeInfo.ValueChanged -= new EventHandler(JZ_MessageTypeInfo_ValueChanged);
			}
		}

		#endregion

	}
}
