namespace Enterprise.Customs.US.GUI
{
	public partial class StatementMessagesUserControl : Customs.GUI.MessageUserControl
	{
		public StatementMessagesUserControl()
		{
			InitializeComponent();
			MessagesTabControl.SelectedTab = MessageDetailsTabPage;
		}
	}
}
