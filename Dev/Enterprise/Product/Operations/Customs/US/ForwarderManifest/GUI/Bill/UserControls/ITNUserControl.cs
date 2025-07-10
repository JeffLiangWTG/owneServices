namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	public class ITNUserControl : CusEntryNumbersUserControl
	{
		protected override string CaptionString => "AES ITN";

		protected override bool IsITN => true;

		protected override void InitControl()
		{
			MoreNumbersButton.Tag = CaptionString;
			MoreNumbersButton.Name = CaptionString;

			MoreNumbersTextBox.CaptionResourceString = Res.GetData("68CBAB52-3215-44A4-B22A-BA40F1F8CE19", "AES ITN");

			BindingSource.SetBindingMember(MoreNumbersTextBox, "AESITNNumbers");

			MoreNumbersTextBox.Name = "AESITNNumbers";
		}
	}
}
