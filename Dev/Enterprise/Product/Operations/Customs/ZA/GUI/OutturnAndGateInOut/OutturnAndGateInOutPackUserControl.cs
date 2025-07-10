using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
#if DEBUG
using Enterprise.ZArchitecture.GUI.Testing;
#endif

namespace Enterprise.Customs.ZA.GUI
{
	public partial class OutturnAndGateInOutPackUserControl : ZUserControl
	{
		ZGroupBox zGroupBoxPackDetail;
		ZArchitecture.ZTextBox zTextBoxMarksAndNumbers;
		ZDropEditWithFixedWidth zDropEditWithFixedWidthVolumeOutturnedUQ;
		ZDropEditWithFixedWidth zDropEditWithFixedWidthWeightOutturnedUQ;
		ZDropEditWithFixedWidth zDropEditWithFixedWidthVolumeUQ;
		ZArchitecture.ZCalcEdit zCalcEditVolumeOutturned;
		ZArchitecture.ZCalcEdit zCalcEditVolume;
		ZArchitecture.ZCalcEdit zCalcEditWeightOutturned;
		ZDropEditWithFixedWidth zDropEditWithFixedWidthWeightUQ;
		ZArchitecture.ZCalcEdit zCalcEditWeight;
		ZArchitecture.ZTextBox zTextBoxGoodsDescription;
		ZArchitecture.ZTextBox zTextBoxPackCondDesc;
		ZDropEditWithFixedWidth zDropEditWithFixedWidthExcessShortInd;
		ZArchitecture.ZTextBox zTextBoxAPA_GoodsDescription;
		ZArchitecture.ZTextBox zTextBoxContShouldBe;
		ZDropEdit zDropEditPackageCondition;
		ZArchitecture.ZCalcEdit zCalcEditPackagesOutturned;
		ZDropEditWithFixedWidth zDropEditWithFixedWidthCargoType;
		ZDropEditWithFixedWidth zDropEditWithFixedWidthPackUQ;
		ZArchitecture.ZCalcEdit zCalcEditPackQty;
		ZGuidDropEditWithFixedWidth zGuidDropEditWithFixedWidthContainerPK;
		ZCheckBox zCheckBoxSealIntactIndicator;
		ZArchitecture.ZGrid zGridPacks;
		ZPanel zPanelPackDetail;
		KSplitContainer splitContainer;
		ZArchitecture.ZLabel zLabelManifestedShouldBe;
		ZArchitecture.ZLabel zLabelActuallyFoundToBe;
		ZArchitecture.ZLabel zLabelDiscrepancyReport;

		public OutturnAndGateInOutPackUserControl()
		{
			InitializeComponent();
#if DEBUG
			MissingResourceStringChecker.ExcludeFromTest(this.zDropEditPackageCondition);
			MissingResourceStringChecker.ExcludeFromTest(this.zDropEditWithFixedWidthCargoType);
			MissingResourceStringChecker.ExcludeFromTest(this.zDropEditWithFixedWidthExcessShortInd);
#endif
		}
	}
}

