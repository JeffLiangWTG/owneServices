namespace Enterprise.MasterFiles.GUI
{
	using System;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.GUI;

	public partial class VehicleDetailsUserControl : ZUserControl
	{
		public VehicleDetailsUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			equipment = CurrentDataItem as RefEquipment;
			if (equipment != null)
			{
				equipment.RQ_RN_NKRegistrationCountryInfo.ValueChanged += RQ_RN_NKRegistrationCountryInfo_ValueChanged;

				SetAUFieldsVisibility();
			}
		}

		void RQ_RN_NKRegistrationCountryInfo_ValueChanged(object sender, EventArgs e)
		{
			SetAUFieldsVisibility();
		}

		void SetAUFieldsVisibility()
		{
			if (equipment != null)
			{
				var showAUFields = equipment.RQ_RN_NKRegistrationCountry == Core.Constants.CountryCodes.Australia;
				NHVAStextbox.Visible = showAUFields;
				Truckcheckbox.Visible = showAUFields;
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				if (equipment != null)
				{
					equipment.RQ_RN_NKRegistrationCountryInfo.ValueChanged -= RQ_RN_NKRegistrationCountryInfo_ValueChanged;
				}
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		RefEquipment equipment;
	}
}
