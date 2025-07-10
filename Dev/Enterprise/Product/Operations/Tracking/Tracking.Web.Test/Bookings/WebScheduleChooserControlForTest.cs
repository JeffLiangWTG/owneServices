using System;
using System.Web.UI.HtmlControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Bookings.Testing
{
	sealed class WebScheduleChooserControlForTest : WebScheduleChooserControl
	{
		public WebScheduleChooserControlForTest()
		{
			VoyageNumber = new ZTextBox();
			Journey = new ZTextBox();
			LCLCutOff = new ZDateTimeLabel();
			FCLCutOff = new ZDateEdit();
			ETD = new ZDateEdit();
			ETA = new ZDateEdit();
			TotalWeight = new ZNumericTextBox();
			UnitsOfWeight = new ZDropDownList();
			TotalVolume = new ZNumericTextBox();
			UnitsOfVolume = new ZDropDownList();
			CFSRef = new ZTextBox();
			Direct = new ZCheckBox();
			ServiceLevel = new ZDropDownList();
			MAWBSeaNumberTextBox = new ZTextBox();
			MAWBPrefixTextBox = new ZTextBox();
			MAWBNumberTextBox = new ZTextBox();
			IsNeutral = new ZCheckBox();
			Carrier = new ZGuidFindBox();
			SelectScheduleBtn = new ZGuidFindBox();

			JourneyLabel = new ZTextLabel();
			FCLCutOffLabel = new ZTextLabel();
			ETALabel = new ZTextLabel();
			ETDLabel = new ZTextLabel();
			TotalWeightLabel = new ZTextLabel();
			TotalVolumeLabel = new ZTextLabel();
			CarrierLabel = new ZTextLabel();
			CFSRefLabel = new ZTextLabel();
			Direct = new ZCheckBox();
			ServiceLevelLabel = new ZTextLabel();
			IsNeutral = new ZCheckBox();
			MAWBLabel = new ZTextLabel();
			MAWBHyphenLabel = new ZTextLabel();
			HeaderLabel = new ZTextLabel();
			VoyageNumberLabel = new ZTextLabel();
			LCLCutOffLabel = new ZTextLabel();

			SailingDiv = new HtmlGenericControl();
			VisibleDiv = new HtmlGenericControl();

			VisibleDiv.Controls.Add(Carrier);
			VisibleDiv.Controls.Add(CarrierLabel);
			VisibleDiv.Controls.Add(CFSRefLabel);
			VisibleDiv.Controls.Add(ServiceLevelLabel);
			VisibleDiv.Controls.Add(Direct);
			VisibleDiv.Controls.Add(IsNeutral);
			VisibleDiv.Controls.Add(MAWBLabel);
			VisibleDiv.Controls.Add(MAWBHyphenLabel);
			VisibleDiv.Controls.Add(CFSRef);
			VisibleDiv.Controls.Add(ServiceLevel);
			VisibleDiv.Controls.Add(MAWBSeaNumberTextBox);
			VisibleDiv.Controls.Add(MAWBPrefixTextBox);
			VisibleDiv.Controls.Add(MAWBNumberTextBox);
			VisibleDiv.Controls.Add(IsNeutral);
			VisibleDiv.Controls.Add(Direct);

			SailingDiv.Controls.Add(VoyageNumber);
			SailingDiv.Controls.Add(Journey);
			SailingDiv.Controls.Add(LCLCutOff);
			SailingDiv.Controls.Add(FCLCutOff);
			SailingDiv.Controls.Add(ETD);
			SailingDiv.Controls.Add(ETA);
			SailingDiv.Controls.Add(TotalWeight);
			SailingDiv.Controls.Add(UnitsOfWeight);
			SailingDiv.Controls.Add(TotalVolume);
			SailingDiv.Controls.Add(UnitsOfVolume);
			SailingDiv.Controls.Add(JourneyLabel);
			SailingDiv.Controls.Add(FCLCutOffLabel);
			SailingDiv.Controls.Add(ETALabel);
			SailingDiv.Controls.Add(ETDLabel);
			SailingDiv.Controls.Add(TotalWeightLabel);
			SailingDiv.Controls.Add(TotalVolumeLabel);
			SailingDiv.Controls.Add(VoyageNumberLabel);
			SailingDiv.Controls.Add(LCLCutOffLabel);
		}

		public ZGuidFindBox CarrierGetter
		{
			get { return Carrier; }
		}

		public ZTextBox CFSRefGetter
		{
			get { return CFSRef; }
		}

		public HtmlGenericControl SailingDivGetter
		{
			get { return SailingDiv; }
		}

		public ZGuidFindBox SelectScheduleBtnGetter
		{
			get { return SelectScheduleBtn; }
		}

		public ZTextLabel HeaderLabelGetter
		{
			get { return HeaderLabel; }
		}

		public ZTextBox MAWBNumberTextBoxForTesting => MAWBNumberTextBox;

		public ZTextBox MAWBSeaNumberTextBoxForTesting => MAWBSeaNumberTextBox;

		public void CallOnLoad()
		{
			OnLoad(EventArgs.Empty);
		}

		public void CallOnInit()
		{
			OnInit(EventArgs.Empty);
		}

		public void CallOnPreRender() => OnPreRender(EventArgs.Empty);

		public void SetDirectForTesting(bool value)
		{
			Direct.Checked = value;
			Direct.HasChanges = true;
		}
	}
}
