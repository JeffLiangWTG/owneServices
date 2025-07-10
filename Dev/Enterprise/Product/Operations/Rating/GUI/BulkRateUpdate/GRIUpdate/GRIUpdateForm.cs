using System;
using Enterprise.DocumentEngine;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class GRIUpdateForm : ZChildForm
	{
		public GRIUpdateForm()
			: base(new RateUpdater())
		{
			InitializeComponent();
		}

		public override ODisplayMode DisplayMode
		{
			get { return ODisplayMode.Undefined; }
			set { }
		}

		public override string FormHeading => FormCaption;

		public void PerformUpdateClick()
		{
			updateButton.PerformClick();
		}

		public void PerformCancelClick()
		{
			cancelButtonX.PerformClick();
		}

		void UpdateButton_Click(object sender, EventArgs e)
		{
			Updater.RunPreSaveValidation();

			if (Updater.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				if (Updater.SendNotifications())
				{
					Updater.UpdateRegistryLastRunDate();
				}

				Close();
			}
		}

		void CancelButtonX_Click(object sender, EventArgs e)
		{
			Close();
		}

		RateUpdater Updater => (RateUpdater)BusinessEntity;

		void previewButton_Click(object sender, EventArgs e)
		{
			UpdateRate selectedRate = null;
			if (bodyControl.clientGrid.ListManager.Position >= 0)
			{
				selectedRate = bodyControl.clientGrid.ListManager.GetCurrent() as UpdateRate;

				if (selectedRate != null)
				{
					selectedRate.Validation.ValidateAll();
				}
			}

			if (selectedRate != null && !selectedRate.HasErrors)
			{
				DocumentPack documentPack;
				DeliveryInstructions instructions;

				using (var printTask = Updater.GetPrintTask(selectedRate, out documentPack, out instructions))
				{
					if (documentPack != null && documentPack.Count > 0 && instructions != null)
					{
						printTask.Preview(instructions);
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("a84698d0-1227-4fdf-8a1a-ce02d16777bb", "No documents will be delivered for the selected client rate update"));
					}
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("fa425eed-8ae8-4121-b95d-7c4cdbcca0e4", "Select a valid client rate update document to preview"));
			}
		}
	}
}

