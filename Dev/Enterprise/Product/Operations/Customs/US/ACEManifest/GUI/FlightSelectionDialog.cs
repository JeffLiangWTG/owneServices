using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.ACEManifest.GUI
{
	public partial class FlightSelectionDialog : ZArchitecture.GUI.ZChildForm
	{
		[Obsolete("This constructor is just for the designer")]
		protected FlightSelectionDialog()
			: base()
		{
		}

		public FlightSelectionDialog(AdditionalMessageInformation additionalMessageInformation, ZString label)
				: base(additionalMessageInformation)
		{
			this.additionalMessageInformation = additionalMessageInformation;
			Text = label;
		}
		readonly protected AdditionalMessageInformation additionalMessageInformation;

		public override string FormVerb => "";

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.None;
			additionalMessageInformation.RunPreSaveValidation();

			if (additionalMessageInformation.HasErrors())
			{
				ShowErrorsDialog();
			}
			else if (!additionalMessageInformation.FlightArrivalDetails.GetSelectedFlights().Any())
			{
				Globals.Message.ShowInformation("Please select the required Flight information from the Flight Details grid first, in order to proceed.", "Select Flight Information");
			}
			else
			{
				DialogResult = OnOKButtonClicked();
			}
		}

		protected virtual DialogResult OnOKButtonClicked()
		{
			return DialogResult.OK;
		}

		void Cancel_Button_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
	}
}
