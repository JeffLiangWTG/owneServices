using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class EntryLineAdditionalDataUserControl : ZUserControl
	{
		public EntryLineAdditionalDataUserControl()
		{
			InitializeComponent();
		}

		internal void ChangeControlsVisibility(bool isImport)
		{
			tradeAgreementLabel.Visible = isImport;

			if (isImport)
			{
				provisionalPaymentGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("44e59746-dc66-4348-b9f9-cab2073bec47", "Provisional Payments");
			}
			else
			{
				provisionalPaymentGroupBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("6FE5040C-22F5-4944-A529-F39C4A5E82CF", "Diamond Levy");
			}
		}
	}
}
