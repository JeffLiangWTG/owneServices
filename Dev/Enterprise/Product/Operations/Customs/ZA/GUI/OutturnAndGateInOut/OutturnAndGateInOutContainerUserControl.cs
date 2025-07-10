using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class OutturnAndGateInOutContainerUserControl : ZUserControl
	{
		ZArchitecture.ZGrid zGridContainer;
		ZDropEditWithFixedWidth zDropEditWithFixedWidthSealingPartyType;
		ZArchitecture.ZTextBox zTextBoxSeal1;
		ZDateEdit zDateEditUnpackTime;
		ZGuidFindBox zGuidFindBoxContainerType;
		ZDropEditWithFixedWidth zDropEditWithFixedWidthEmptyFullIndicator;
		ZArchitecture.ZTextBox zTextBoxContainerNumber;
		ZGroupBox zGroupBoxContainerDetail;
		ZDateEdit zDateEditGateInOutTime;

		public OutturnAndGateInOutContainerUserControl()
		{
			InitializeComponent();
		}
	}
}

