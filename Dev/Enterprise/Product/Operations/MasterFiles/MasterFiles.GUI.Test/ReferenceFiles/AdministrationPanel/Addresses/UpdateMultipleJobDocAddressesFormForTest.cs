using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class UpdateMultipleJobDocAddressesFormForTest : UpdateMultipleJobDocAddressesForm
	{
		public UpdateMultipleJobDocAddressesFormForTest(string title, bool showYesButton = true) : base(title, showYesButton)
		{
		}

		public ZButton UpdateThisRecordButtonForTest => UpdateThisRecordButton;
		public ZButton UpdateAllRecordsButtonForTest => UpdateAllRecordsButton;
		public ZButton CancelSelectButtonTest => CancelSelectButton;
		public ZButton YesButtonTest => YesButton;
	}
}
