using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.GUI
{
	public partial class ImportSupplierHeaderUserControl : BaseCustomsSupplierHeaderUserControl
	{
		public ImportSupplierHeaderUserControl()
		{
			InitializeComponent();
			InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
			JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = "GridLayoutmzSsmLeeORtZAZNEEQiqsA=="; // DataImportWizard GridID.
			InvoiceChargesGrid.GridId = "8D35E6B2-5825-4054-9346-C8DEB3167B45";
			BaseGroupChargesGrid.GridId = "A6E65C1C-B197-44B4-BCE1-B2DCBB8B307A";
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();

			var captionForIsStatisticalValueApplicable = Res.GetData("e0616ce1-898c-4dac-be90-8951ba823092", "Incl. in FOB", "It indicates whether the charge is included in the FOB price. The Incoterm and charge code determine whether the charge is included by default.");
			using (InvoiceChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				InvoiceChargesGrid.SetColumnWidth(BaseJobComInvHeaderCharge.Schema.J7_IsDutiable, 80);
				InvoiceChargesGrid.RemoveFromAvailableColumns(BaseJobComInvHeaderCharge.Schema.J7_IsGSTApplicable);
				InvoiceChargesGrid.ColumnStyles.Add(new ZArchitecture.ZCheckBoxColumnStyleInfo
				{
					ColumnName = BaseJobComInvHeaderCharge.Schema.J7_IsStatisticalValueApplicable,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					CaptionResourceString = captionForIsStatisticalValueApplicable
				});
			}

			using (BaseGroupChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				BaseGroupChargesGrid.SetColumnWidth(BaseJobComInvHeaderCharge.Schema.J7_IsDutiable, 80);
				BaseGroupChargesGrid.RemoveFromAvailableColumns(BaseJobComInvHeaderCharge.Schema.J7_IsGSTApplicable);
				BaseGroupChargesGrid.ColumnStyles.Add(new ZArchitecture.ZCheckBoxColumnStyleInfo
				{
					ColumnName = BaseJobComInvHeaderCharge.Schema.J7_IsStatisticalValueApplicable,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					CaptionResourceString = captionForIsStatisticalValueApplicable
				});
			}

			using (ApportionedChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				ApportionedChargesGrid.SetColumnWidth(BaseJobComInvHeaderCharge.Schema.J7_IsDutiable, 80);
				ApportionedChargesGrid.RemoveFromAvailableColumns(BaseJobComInvHeaderCharge.Schema.J7_IsGSTApplicable);
				ApportionedChargesGrid.ColumnStyles.Add(new ZArchitecture.ZCheckBoxColumnStyleInfo
				{
					ColumnName = BaseJobComInvHeaderCharge.Schema.J7_IsStatisticalValueApplicable,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					CaptionResourceString = captionForIsStatisticalValueApplicable
				});
			}

			ResetInvoiceChargesGridColumnOrder();
		}

		#region The Columns of InvoiceChargesGrid (Order, DisplayHidden)

		void ResetInvoiceChargesGridColumnOrder()
		{
			InvoiceChargesGrid.ReOrderColumns(ColumnNamesInSortOrder);
			InvoiceChargesGrid.SetAllColumnsVisible(false);
			InvoiceChargesGrid.SetColumnVisible(true, defaultColumnsForGrid);
		}

		readonly string[] defaultColumnsForGrid = new string[]
		{
			BaseJobComInvHeaderCharge.Schema.J7_ChargeType,
			BaseJobComInvHeaderCharge.Schema.J7_ChargeDescription,
			BaseJobComInvHeaderCharge.Schema.J7_Amount,
			BaseJobComInvHeaderCharge.Schema.J7_RX_NKCurrency,
			BaseJobComInvHeaderCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
			BaseJobComInvHeaderCharge.Schema.J7_IsIncludedInITOT,
			BaseJobComInvHeaderCharge.Schema.J7_IsStatisticalValueApplicable,
			BaseJobComInvHeaderCharge.Schema.J7_IsDutiable,
			BaseJobComInvHeaderCharge.Schema.J7_DistributeBy
		};

		string[] fColumnNamesInSortOrder;
		string[] ColumnNamesInSortOrder
		{
			get
			{
				if (fColumnNamesInSortOrder == null)
				{
					var result = new List<string>(defaultColumnsForGrid);
					result.Add(BaseJobComInvHeaderCharge.Schema.IsJ7_ExchangeRateUserEnterable);
					result.Add(BaseJobComInvHeaderCharge.Schema.J7_ExchangeRate);
					result.Add(BaseJobComInvHeaderCharge.Schema.J7_PrepaidCollect);
					result.Add(BaseJobComInvHeaderCharge.Schema.J7_Percentage);
					fColumnNamesInSortOrder = result.ToArray();
				}
				return fColumnNamesInSortOrder;
			}
		}

		#endregion

		protected override string ColumnTitleWhenImportForDutiable => Res.GetString("65016a2d-f0b5-469c-b68b-bb398c0d000b", "Incl. in CIF");

		protected override void SetJobComInvoiceHeadersBoundGridColumns()
		{
			base.SetJobComInvoiceHeadersBoundGridColumns();

			JobComInvoiceHeadersBoundGrid.ColumnStyles.Add(new ZArchitecture.GUI.ZDropEditColumnStyleInfo
			{
				ColumnName = JobComInvoiceHeaderSchema.Constants.JZ_RelatedIndicator,
				BindToList = "Lookups.RelatedIndicatorList",
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
				IsVisible = false
			});
		}
	}
}
