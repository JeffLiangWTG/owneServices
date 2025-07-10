using Enterprise.Customs.US.AMS.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.GUI
{
	public partial class USAMSMainUserControl : ZUserControl
	{
		public USAMSMainUserControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(System.EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				ChangeVisibility();
			}
		}

		void ChangeVisibility()
		{
			var header = this.CurrentDataItem as CusInBondHeader;
			var isNVOCCVisible = header != null && header.IsNVOCCHeader;
			OceanBillDetailsGroupBox.Visible = isNVOCCVisible;
			BH_FIRMSCodeFindBox.Visible = !isNVOCCVisible;
			ArrivalDateGroupBox.Visible = !isNVOCCVisible;
			ActualArrivalDateDateEdit.Visible = isNVOCCVisible;

			if (header.IsNVOCCHeader)
			{
				BH_CarrierSCACCodeFindBox.CaptionResourceString = Res.GetData("4d836872-030f-41fc-a35c-23ea8fe1b2c7", "NVO Carrier Code");
			}
		}
	}
}
