using System;
using Enterprise.Core;
using Enterprise.Customs.Business.SADH;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.SADH
{
	public partial class SADHEntryForm : ZChildForm
	{
		public SADHEntryForm()
		{
		}

		public SADHEntryForm(ISADHFormDataManager formDataManager)
			: base(formDataManager.FormData)
		{
			FormDataManager = formDataManager;
			FormDataManager.FormData.D1_ModeOfTransportAtTheBorderInfo.ValueChanged += D1_ModeTransportAtBorderInfo_ValueChanged;
			D1_ModeTransportAtBorderInfo_ValueChanged(null, null);

			FormBottomBorderLabel.AllowOverlap(Section54Panel);
			FormBottomBorderLabel.AllowOverlap(SectionJPanel);
			FormRightBorderLabel.AllowOverlap(Section13Panel);
			FormRightBorderLabel.AllowOverlap(Section17bPanel);
			FormRightBorderLabel.AllowOverlap(Section17Panel);
			FormRightBorderLabel.AllowOverlap(Section20Panel);
			FormRightBorderLabel.AllowOverlap(Section24Panel);
			FormRightBorderLabel.AllowOverlap(Section28Panel);
			FormRightBorderLabel.AllowOverlap(Section36Panel);
			FormRightBorderLabel.AllowOverlap(Section39Panel);
			FormRightBorderLabel.AllowOverlap(Section40Panel);
			FormRightBorderLabel.AllowOverlap(Section43Panel);
			FormRightBorderLabel.AllowOverlap(Section45Panel);
			FormRightBorderLabel.AllowOverlap(Section46Panel);
			FormRightBorderLabel.AllowOverlap(Section49Panel);
			FormRightBorderLabel.AllowOverlap(Section51fPanel);
			FormRightBorderLabel.AllowOverlap(Section53Panel);
			FormRightBorderLabel.AllowOverlap(Section54Panel);
			FormRightBorderLabel.AllowOverlap(Section7Panel);
			FormRightBorderLabel.AllowOverlap(Section9Panel);
			FormRightBorderLabel.AllowOverlap(SectionAPanel);
			FormRightBorderLabel.AllowOverlap(SectionBPanel);
			FormRightBorderLabel.AllowOverlap(SectionCPanel);
			Section10Panel.AllowOverlap(Section8Panel);
			Section10Panel.AllowOverlap(Section9Panel);
			Section11Panel.AllowOverlap(Section10Panel);
			Section11Panel.AllowOverlap(Section9Panel);
			Section12Panel.AllowOverlap(Section11Panel);
			Section12Panel.AllowOverlap(Section9Panel);
			Section13Panel.AllowOverlap(Section12Panel);
			Section13Panel.AllowOverlap(Section9Panel);
			Section14Panel.AllowOverlap(Section10Panel);
			Section14Panel.AllowOverlap(Section15Panel);
			Section14Panel.AllowOverlap(Section16Panel);
			Section14Panel.AllowOverlap(Section8Panel);
			Section15bPanel.AllowOverlap(Section12Panel);
			Section15bPanel.AllowOverlap(Section15Panel);
			Section15bPanel.AllowOverlap(Section16Panel);
			Section15bPanel.AllowOverlap(Section17Panel);
			Section15Panel.AllowOverlap(Section10Panel);
			Section15Panel.AllowOverlap(Section11Panel);
			Section15Panel.AllowOverlap(Section12Panel);
			Section16Panel.AllowOverlap(Section15Panel);
			Section17bPanel.AllowOverlap(Section12Panel);
			Section17bPanel.AllowOverlap(Section13Panel);
			Section17bPanel.AllowOverlap(Section15bPanel);
			Section17bPanel.AllowOverlap(Section17Panel);
			Section17Panel.AllowOverlap(Section15Panel);
			Section17Panel.AllowOverlap(Section16Panel);
			Section18Panel.AllowOverlap(Section14Panel);
			Section19Panel.AllowOverlap(Section14Panel);
			Section19Panel.AllowOverlap(Section16Panel);
			Section19Panel.AllowOverlap(Section18Panel);
			Section1Panel.AllowOverlap(SectionAPanel);
			Section20Panel.AllowOverlap(Section14Panel);
			Section20Panel.AllowOverlap(Section16Panel);
			Section20Panel.AllowOverlap(Section17Panel);
			Section20Panel.AllowOverlap(Section19Panel);
			Section21Panel.AllowOverlap(Section18Panel);
			Section21Panel.AllowOverlap(Section19Panel);
			Section21Panel.AllowOverlap(Section20Panel);
			Section22Panel.AllowOverlap(Section19Panel);
			Section22Panel.AllowOverlap(Section20Panel);
			Section22Panel.AllowOverlap(Section21Panel);
			Section23Panel.AllowOverlap(Section20Panel);
			Section23Panel.AllowOverlap(Section22Panel);
			Section24Panel.AllowOverlap(Section20Panel);
			Section24Panel.AllowOverlap(Section23Panel);
			Section25DropEdit.AllowOverlap(Section25Description2Label);
			Section25Panel.AllowOverlap(Section21Panel);
			Section26Panel.AllowOverlap(Section21Panel);
			Section26Panel.AllowOverlap(Section25Panel);
			Section26Panel.AllowOverlap(Section27Panel);
			Section27Panel.AllowOverlap(Section21Panel);
			Section27Panel.AllowOverlap(Section22Panel);
			Section28Panel.AllowOverlap(Section21Panel);
			Section28Panel.AllowOverlap(Section22Panel);
			Section28Panel.AllowOverlap(Section23Panel);
			Section28Panel.AllowOverlap(Section24Panel);
			Section28Panel.AllowOverlap(Section27Panel);
			Section2Panel.AllowOverlap(Section1Panel);
			Section30Panel.AllowOverlap(Section26Panel);
			Section30Panel.AllowOverlap(Section27Panel);
			Section30Panel.AllowOverlap(Section28Panel);
			Section31bPanel.AllowOverlap(Section28Panel);
			Section31bPanel.AllowOverlap(Section30Panel);
			Section31Panel.AllowOverlap(Section31bPanel);
			Section33Panel.AllowOverlap(Section28Panel);
			Section33Panel.AllowOverlap(Section31bPanel);
			Section34Panel.AllowOverlap(Section31bPanel);
			Section34Panel.AllowOverlap(Section33Panel);
			Section35Panel.AllowOverlap(Section33Panel);
			Section35Panel.AllowOverlap(Section34Panel);
			Section35Panel.AllowOverlap(Section36Panel);
			Section37Panel.AllowOverlap(Section31bPanel);
			Section37Panel.AllowOverlap(Section34Panel);
			Section37Panel.AllowOverlap(Section35Panel);
			Section38Panel.AllowOverlap(Section34Panel);
			Section38Panel.AllowOverlap(Section35Panel);
			Section38Panel.AllowOverlap(Section36Panel);
			Section38Panel.AllowOverlap(Section37Panel);
			Section39Panel.AllowOverlap(Section35Panel);
			Section39Panel.AllowOverlap(Section36Panel);
			Section39Panel.AllowOverlap(Section38Panel);
			Section3Panel.AllowOverlap(Section1Panel);
			Section3Panel.AllowOverlap(Section2Panel);
			Section40Panel.AllowOverlap(Section31bPanel);
			Section40Panel.AllowOverlap(Section37Panel);
			Section40Panel.AllowOverlap(Section38Panel);
			Section40Panel.AllowOverlap(Section39Panel);
			Section41Panel.AllowOverlap(Section31bPanel);
			Section41Panel.AllowOverlap(Section40Panel);
			Section43Panel.AllowOverlap(Section40Panel);
			Section44Panel.AllowOverlap(Section31bPanel);
			Section44Panel.AllowOverlap(Section31Panel);
			Section45Panel.AllowOverlap(Section43Panel);
			Section45Panel.AllowOverlap(Setion42Panel);
			Section46Panel.AllowOverlap(Section45Panel);
			Section46Panel.AllowOverlap(SectionA1Panel);
			Section47bPanel.AllowOverlap(Section44Panel);
			Section47bPanel.AllowOverlap(Section47Panel);
			Section47Panel.AllowOverlap(Section44Panel);
			Section48Panel.AllowOverlap(Section47bPanel);
			Section49Panel.AllowOverlap(Section46Panel);
			Section49Panel.AllowOverlap(Section48Panel);
			Section4Panel.AllowOverlap(Section1Panel);
			Section4Panel.AllowOverlap(Section3Panel);
			Section4Panel.AllowOverlap(SectionAPanel);
			Section50Panel.AllowOverlap(Section47bPanel);
			Section50Panel.AllowOverlap(Section47Panel);
			Section50Panel.AllowOverlap(SectionBPanel);
			Section51aPanel.AllowOverlap(Section50Panel);
			Section51aPanel.AllowOverlap(Section51Panel);
			Section51bPanel.AllowOverlap(Section50Panel);
			Section51bPanel.AllowOverlap(Section51aPanel);
			Section51cPanel.AllowOverlap(Section50Panel);
			Section51cPanel.AllowOverlap(Section51bPanel);
			Section51dPanel.AllowOverlap(Section50Panel);
			Section51dPanel.AllowOverlap(Section51cPanel);
			Section51dPanel.AllowOverlap(SectionCPanel);
			Section51ePanel.AllowOverlap(Section50Panel);
			Section51ePanel.AllowOverlap(Section51dPanel);
			Section51ePanel.AllowOverlap(SectionCPanel);
			Section51fPanel.AllowOverlap(Section51ePanel);
			Section51fPanel.AllowOverlap(SectionCPanel);
			Section51Panel.AllowOverlap(Section50Panel);
			Section52Panel.AllowOverlap(Section51aPanel);
			Section52Panel.AllowOverlap(Section51bPanel);
			Section52Panel.AllowOverlap(Section51cPanel);
			Section52Panel.AllowOverlap(Section51dPanel);
			Section52Panel.AllowOverlap(Section51ePanel);
			Section52Panel.AllowOverlap(Section51Panel);
			Section53Panel.AllowOverlap(Section51dPanel);
			Section53Panel.AllowOverlap(Section51ePanel);
			Section53Panel.AllowOverlap(Section51fPanel);
			Section53Panel.AllowOverlap(Section52Panel);
			Section54Panel.AllowOverlap(Section52Panel);
			Section54Panel.AllowOverlap(Section53Panel);
			Section54Panel.AllowOverlap(SectionJPanel);
			Section5Panel.AllowOverlap(Section2Panel);
			Section5Panel.AllowOverlap(Section3Panel);
			Section5Panel.AllowOverlap(Section4Panel);
			Section6Panel.AllowOverlap(Section3Panel);
			Section6Panel.AllowOverlap(Section4Panel);
			Section6Panel.AllowOverlap(Section5Panel);
			Section6Panel.AllowOverlap(SectionAPanel);
			Section7Panel.AllowOverlap(Section6Panel);
			Section7Panel.AllowOverlap(SectionAPanel);
			Section8Panel.AllowOverlap(Section2Panel);
			Section8Panel.AllowOverlap(Section5Panel);
			Section9Panel.AllowOverlap(Section2Panel);
			Section9Panel.AllowOverlap(Section5Panel);
			Section9Panel.AllowOverlap(Section6Panel);
			Section9Panel.AllowOverlap(Section7Panel);
			Section9Panel.AllowOverlap(Section8Panel);
			SectionA1Panel.AllowOverlap(Section41Panel);
			SectionA1Panel.AllowOverlap(Section45Panel);
			SectionA1Panel.AllowOverlap(Setion42Panel);
			SectionBPanel.AllowOverlap(Section47bPanel);
			SectionBPanel.AllowOverlap(Section48Panel);
			SectionBPanel.AllowOverlap(Section49Panel);
			SectionCPanel.AllowOverlap(Section50Panel);
			SectionCPanel.AllowOverlap(SectionBPanel);
			SectionJPanel.AllowOverlap(Section52Panel);
			Setion42Panel.AllowOverlap(Section40Panel);
			Setion42Panel.AllowOverlap(Section41Panel);
			Setion42Panel.AllowOverlap(Section43Panel);
			CopyCountryDestinationVerticalLabel.AllowOverlap(CopyCountryDestinationBorderLabel);

			Section24DescriptionLabel.AllowOutsideOfParent();
			Section25Description2Label.AllowOutsideOfParent();
			Section32Panel.AllowOutsideOfParent();
			Section52CodePanel.AllowOutsideOfParent();
			Section52Panel.AllowOutsideOfParent();
			SectionALeftBorder.AllowOutsideOfParent();
		}

		void D1_ModeTransportAtBorderInfo_ValueChanged(object sender, EventArgs e)
		{
			if (FormDataManager.FormData.D1_ModeOfTransportAtTheBorder == Constants.TransportModes.Air)
			{
				TransportAirFieldsVisible(true);
				TransportSeaFieldsVisible(false);
				TransportOtherFieldsVisible(false);
			}
			else if (FormDataManager.FormData.D1_ModeOfTransportAtTheBorder == Constants.TransportModes.Sea)
			{
				TransportAirFieldsVisible(false);
				TransportSeaFieldsVisible(true);
				TransportOtherFieldsVisible(false);
			}
			else
			{
				TransportAirFieldsVisible(false);
				TransportSeaFieldsVisible(false);
				TransportOtherFieldsVisible(true);
			}
		}

		void TransportAirFieldsVisible(bool visible)
		{
			Section21FlightNoLabel.Visible = visible;
			Section21FlightNoTextBox.Visible = visible;
			Section21FlightDateLabel.Visible = visible;
			Section21FlightDateDateEdit.Visible = visible;
		}

		void TransportSeaFieldsVisible(bool visible)
		{
			Section21VesselLabel.Visible = visible;
			Section21VesselCodeFindBox.Visible = visible;
		}

		void TransportOtherFieldsVisible(bool visible)
		{
			DepartureTransportIDLabel.Visible = visible;
			DepartureTransportIDBox.Visible = visible;
		}

		public readonly ISADHFormDataManager FormDataManager;

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			FormDataManager.WriteData();
			Close();
		}

		public override string FormHeading
		{
			get { return Res.GetString("49d173a5-63e3-493b-a67c-4ec5668b4c55", "SAD/H Data Entry"); }
		}
	}
}
