using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.US.GUI
{
	public partial class MessagesStatusErrorsUserControl : ZUserControl
	{
		public MessagesStatusErrorsUserControl()
		{
			InitializeComponent();
			blockTextBox.Font = new System.Drawing.Font("Courier New", 8F);
		}
	}
}
