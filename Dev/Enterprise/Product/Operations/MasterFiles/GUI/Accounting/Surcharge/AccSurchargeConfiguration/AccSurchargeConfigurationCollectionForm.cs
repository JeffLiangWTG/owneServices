using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccSurchargeConfigurationCollectionForm : ZChildForm
	{
		public AccSurchargeConfigurationCollectionForm(AccSurchargeConfigurationCollection surchargeConfigurations)
			: base(surchargeConfigurations)
		{
			this.fOldItems = surchargeConfigurations.ApplicableSurchargesAsString;
			this.SurchargeConfigurations = surchargeConfigurations;
		}

		public static DialogResult ShowDialog(AccSurchargeConfigurationCollection surchargeConfigurations)
		{
			return ZFormModaliser.ShowDialogAndDispose(new AccSurchargeConfigurationCollectionForm(surchargeConfigurations));
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		readonly string fOldItems;

		readonly AccSurchargeConfigurationCollection SurchargeConfigurations;

		#region Form Caption

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion

		#region Closing

		protected override void OnClosing(CancelEventArgs e)
		{
			CloseButton.Focus();

			if (DialogResult == DialogResult.Cancel)
			{
				SurchargeConfigurations.ApplicableSurchargesAsString = fOldItems;
			}

			base.OnClosing(e);
		}

		#endregion

		#region Buttons

		void OnOKButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void OnCloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion
	}
}
