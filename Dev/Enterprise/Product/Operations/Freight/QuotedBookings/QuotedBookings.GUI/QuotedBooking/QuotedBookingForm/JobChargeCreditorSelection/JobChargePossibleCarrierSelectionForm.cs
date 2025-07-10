using System;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public partial class JobChargePossibleCarrierSelectionForm : ZChildForm
	{
		public JobChargePossibleCarrierSelectionForm(JobChargePossibleCarrierSelection jobChargeCreditorsSelection) : base(jobChargeCreditorsSelection) { }

		public override string FormVerb => string.Empty;

		public override string FormCaption => Res.GetString("03edb4e4-ce34-4243-bc4a-f3c06f2fe873", "Assign Creditor");

		public new JobChargePossibleCarrierSelection DataSource
		{
			get { return (JobChargePossibleCarrierSelection)base.DataSource; }
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			SelectionMade();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void SelectionMade()
		{
			DataSource.RunPreSaveValidation();
			if (DataSource.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				var oneOffCarrier = (RateOneOffCarrier)PotentialCarriersGrid.ListManager.GetCurrent();
				if (oneOffCarrier == null)
				{
					return;
				}

				DataSource.SetPossibleCarrierSelection(oneOffCarrier.TTC_OH_Carrier, oneOffCarrier.TTC_OH_Creditor);
				Close();
			}
		}

		void PotentialCarriersGrid_DoubleClick(object sender, EventArgs e)
		{
			SelectionMade();
		}
	}
}

