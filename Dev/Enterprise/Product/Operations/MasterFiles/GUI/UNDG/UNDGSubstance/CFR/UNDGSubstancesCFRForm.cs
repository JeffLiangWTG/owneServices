using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class UNDGSubstanceCFRForm : ZForm
	{
		public UNDGSubstanceCFRForm(UNDGSubstanceCFR substance)
			: base(substance)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);

			SetIconImage();

#if DEBUG
			ZArchitecture.GUI.Testing.MissingResourceStringChecker.ExcludeFromTest(CFR_ReportableKiloQuantityCalcEdit);
#endif
		}

		void SetIconImage()
		{
			IconPictureBox.Image = Properties.Resources.cfr_freight;
		}

		UNDGSubstanceCFR Substance
		{
			get { return (UNDGSubstanceCFR)BusinessEntity; }
		}

		protected override bool AllowNew => false;

		public override string FormCaption
		{
			get
			{
				var formName = Res.GetString("UNDGSubstanceCFRForm|FormCaptionPrefix", "Dangerous Goods Substance - CFR");
				return Substance.IsInDatabase ? formName + " - " + Substance.CFR_UNNO + " - " + Substance.CFR_PSN : formName;
			}
		}
	}
}
