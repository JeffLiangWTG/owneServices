namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	public class InBondUserControl : CusEntryNumbersUserControl
	{
		protected override string CaptionString => "In-Bond Number";

		protected override void InitControl()
		{
			MoreNumbersButton.Tag = CaptionString;
			MoreNumbersButton.Name = CaptionString;

			MoreNumbersTextBox.CaptionResourceString = Res.GetData("9C490FAB-F8E9-4516-878C-98DBBFC73317", "In-Bond Number");

			BindingSource.SetBindingMember(MoreNumbersTextBox, "InBondNumbers");

			MoreNumbersTextBox.Name = "InBondNumbers";
		}
	}
}
