using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class GPSClientActivityTestDataEntryForm : ZChildForm
	{
		public GPSClientActivityTestDataEntryForm(GPSSupporterActivityTestDataCollection activities)
			: base(activities)
		{
			Factory = activities.Factory;
		}

		public static void ShowDialog(CommonWorkSheet workSheet)
		{
			ZFormModaliser.ShowDialogAndDispose(new GPSClientActivityTestDataEntryForm(new GPSSupporterActivityTestDataCollection(workSheet)));
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			GPSClientActivityGrid.BeginEdit(GPSClientActivityGrid.TableStyles[0].GridColumnStyles[GPSSupporterActivityTestData.Schema.EN_ActivityType], 0);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void OnSaveButton_Click(object sender, EventArgs e)
		{
			SaveButton.DialogResult = DialogResult.None;

			var activities = (GPSSupporterActivityTestDataCollection)BusinessEntity;
			activities.RunPreSaveValidation();

			if (activities.HasErrors())
			{
				ShowErrorsDialog();
			}
			else
			{
				Factory.Save();
				this.SaveButton.DialogResult = DialogResult.OK;
				Close();
			}
		}

		void OnCloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
