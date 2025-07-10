using System;
using System.Windows.Forms;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class DA63UserControl : ZUserControl
	{
		ZArchitecture.ZCalcEdit importVATPaidCalcEdit1;
		ZArchitecture.ZCalcEdit importDutyPaidCalcEdit1;
		ZArchitecture.ZCalcEdit importDutyPaidCalcEdit;
		ZArchitecture.ZCalcEdit importCustomsValueCalcEditCalcEdit1;
		ZArchitecture.ZCalcEdit importVATPaidCalcEdit;
		ZGroupBox additionalQuantityGroupBox;
		ZCalcDropEdit firstAddUnitCalcDropEdit;
		ZCalcDropEdit secondAddUnitCalcDropEdit;
		ZGroupBox additionalDutiesGroupBox;
		ZArchitecture.ZGrid additionalDutiesGrid;
		ZGroupBox bottomGroupBox;
		ZGroupBox topGroupBox;
		ZArchitecture.ZCalcEdit previousConversionFactorCalcEdit;
		ZArchitecture.ZCalcEdit conversionFactorCalcEdit;
		ZArchitecture.ZCalcEdit importCustomsValueCalcEdit;

		JobComInvoiceLine InvoiceLine => GetInvoiceLine();

		protected virtual JobComInvoiceLine GetInvoiceLine() => this.CurrentDataItem as JobComInvoiceLine;

		internal static string ReCalculationConfirmation => Res.GetString("02D66F62-FDB7-45C9-B294-CC23EC4A156D", "Some value on this invoice line has been changed that may affect the DA63 have been made, do you want to recalculate DA63 values?");

		internal void ReCalculateDA63ValuesIfNeeded()
		{
			var invoiceLine = InvoiceLine;
			if (invoiceLine != null && !invoiceLine.IsDeleted && invoiceLine.IsDA63 && invoiceLine.DA63NeedsRecalculation)
			{
				if (invoiceLine.IsInDatabase && Globals.Message.Show(ReCalculationConfirmation, Res.GetString("05B2C99F-87C9-4003-AB8D-9BA314B1C155", "Recalculate DA63 values"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes)
				{
					invoiceLine.RecalculateDA63Values();
				}
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (InvoiceLine != null)
			{
				InvoiceLine.JI_ImportTariffInfo.ValueChanged -= UZ_ImportTariff_ValueChanged;
				InvoiceLine.JI_ImportTariffInfo.ValueChanged += UZ_ImportTariff_ValueChanged;
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (InvoiceLine != null)
			{
				InvoiceLine.JI_ImportTariffInfo.ValueChanged -= UZ_ImportTariff_ValueChanged;
			}
		}

		void UZ_ImportTariff_ValueChanged(object sender, EventArgs e)
		{
			#region Workaround for ZDropEdit and ZCodeFindBox CodeBox not updating correctly on RefreshBinding.

			BindingSource.SetBindingMember(ImportTariffCodeFindBox, CargoWise.Types.ZString.Empty);
			BindingSource.SetBindingMember(ImportTariffCodeFindBox, JobComInvoiceLine.Schema.JI_ImportTariff);

			#endregion
		}

		ZCalcDropEdit importCustomsQtyCalcDropEdit1;
		ZCalcDropEdit importCustomsQtyCalcDropEdit;
		public Universal.GUI.TariffFindBox ImportTariffCodeFindBox1;
		public Universal.GUI.TariffFindBox ImportTariffCodeFindBox;
		ZArchitecture.ZLabel zLabel3;
		ZArchitecture.ZLabel zLabel1;
		ZArchitecture.ZLabel zLabel2;

		public DA63UserControl()
		{
			InitializeComponent();
			ImportTariffCodeFindBox1.GetCountryCode = () => Core.Constants.CountryCodes.SouthAfrica;
			ImportTariffCodeFindBox1.GetDataGrouping = () => Core.Constants.CountryCodes.SouthAfrica;
		}
	}
}

