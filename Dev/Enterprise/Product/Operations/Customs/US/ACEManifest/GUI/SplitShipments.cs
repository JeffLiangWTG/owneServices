using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.US.ACEManifest.Business;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

namespace Enterprise.Customs.US.ACEManifest.GUI
{
	public partial class SplitShipments : FlightSelectionDialog
	{
		[Obsolete("This constructor is just for the designer")]
		protected SplitShipments()
			: base()
		{
		}

		public SplitShipments(AdditionalMessageInformation additionalMessageInformation, string messageSubType)
				: base(additionalMessageInformation, "Send")
		{
			this.messageSubType = messageSubType;
			SetFieldsVisibility();
		}
		readonly string messageSubType;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected void SetFieldsVisibility()
		{
			var fieldsAreVisible = messageSubType != AIMMessageSubTypes.FRX && messageSubType != AIMMessageSubTypes.FXX;
			IsSplitShipmentCheckBox.Visible = fieldsAreVisible;
			BoardedQtyCalcEdit.Visible = fieldsAreVisible;
			BoardedWeightCalcEdit.Visible = fieldsAreVisible;
			BoardedWeightUQ.Visible = fieldsAreVisible;
		}

		protected override DialogResult OnOKButtonClicked()
		{
			var result = DialogResult.None;

			var selectedFlight = additionalMessageInformation.FlightArrivalDetails.GetSelectedFlights().FirstOrDefault();
			if (selectedFlight != null)
			{
				additionalMessageInformation.AM_FlightNo = selectedFlight.FlightNo;
				additionalMessageInformation.AM_FlightArrivalDate = selectedFlight.FlightArrivalDate;
				additionalMessageInformation.AM_FlightReference = selectedFlight.FlightReference;
				result = DialogResult.OK;
			}

			return result;
		}
	}
}
