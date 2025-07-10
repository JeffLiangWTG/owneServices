using System;
using CargoWise.ComponentModel;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Confirmations.GUI
{
	public partial class ConfirmationDetailsControl : ZUserControl
	{
		public ConfirmationDetailsControl()
		{
			InitializeComponent();

			ChangeConfirmAddressCheckBox.AllowOverlap(ConfirmAddressControl);
			PickupDeliveryConfirmPanel.AllowOutsideOfParent();
			PostcodeDistanceUnitLabel.AllowOutsideOfParent();
			zTextBox1.AllowOutsideOfParent();
			zTextBox2.AllowOutsideOfParent();
			DistanceTextBox.AllowOutsideOfParent();
			TransportCoNameTextBox.AllowOutsideOfParent();
			DriversNameTextBox.AllowOutsideOfParent();
			TruckRegistrationTextBox.AllowOutsideOfParent();
			DriversLicenseTextBox.AllowOutsideOfParent();
			ChangeConfirmAddressCheckBox.AllowOutsideOfParent();
			CalculateDistanceButton.AllowOutsideOfParent();
			DistanceUDropEdit.AllowOutsideOfParent();
			TransportCoAddressControl.AllowOutsideOfParent();
			ConfirmAddressControl.AllowOutsideOfParent();
			CalculateDistanceButton.AllowOverlap(DistanceUDropEdit);
		}

		void CalculateDistanceButton_Click(object sender, EventArgs e)
		{
			if (Confirm != null)
			{
				Confirm.SetCalculatedDistance((INotifications)ParentForm);
			}
		}

		CommonPickupDeliveryConfirm Confirm
		{
			get { return (CommonPickupDeliveryConfirm)CurrentDataItem; }
		}
	}
}
