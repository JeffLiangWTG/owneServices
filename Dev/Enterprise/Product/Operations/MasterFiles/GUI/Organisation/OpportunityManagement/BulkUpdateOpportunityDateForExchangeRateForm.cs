using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class BulkUpdateOpportunityDateForExchangeRateForm : ZChildForm
	{
		public BulkUpdateOpportunityDateForExchangeRateForm(BulkUpdateP8_DateForExchangeRateAction updateAction)
			: base(updateAction)
		{
			InitializeComponent();
			SetupInstructionsLabel();
		}

		public new BulkUpdateP8_DateForExchangeRateAction BusinessEntity
		{
			get { return (BulkUpdateP8_DateForExchangeRateAction)base.BusinessEntity; }
		}

		#region InstructionsLabel

		protected internal ZLabel InstructionsLabel
		{
			get { return instructionsLabel; }
		}

		void SetupInstructionsLabel()
		{
			instructionsLabel.Text = Res.GetString("F5CA9D8C-70F5-4FA4-A750-D15814609AC4",
@"Updating the Exchange Rate Date for the {0} selected opportunity(s).
The total opportunity estimate values will be converted into the new currency as per date set.",
				BusinessEntity.Count);
		}

		#endregion

		#region Buttons

		#region UpdateDateButton

		protected internal ZButton UpdateDateButton
		{
			get { return updateDateButton; }
		}

		void UpdateDateButton_Click(object sender, EventArgs e)
		{
			BusinessEntityForValidation.RunPreSaveValidation();
			if (!BusinessEntityForValidation.HasErrors())
			{
				DoUpdate();
				Close();
			}
			else
			{
				ShowErrorsDialog();
			}
		}

		void DoUpdate()
		{
			try
			{
				using (var progressForm = new ProgressForm())
				{
					progressForm.Cancelled += (sender, e) => BusinessEntity.Cancel();
					var progressHandler = new Progress(progressForm.SetStatusAndPercentComplete);

					BusinessEntity.Execute(progressHandler);
				}

				if (!BusinessEntity.Cancelled)
				{
					Globals.Message.Show(Res.GetString("C913D0C1-76BA-4037-BF8A-39711B9E1D2F", "Bulk update Exchange Rate Date completed."));
				}
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		#endregion

		#endregion

		#region Form Caption

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion
	}
}
