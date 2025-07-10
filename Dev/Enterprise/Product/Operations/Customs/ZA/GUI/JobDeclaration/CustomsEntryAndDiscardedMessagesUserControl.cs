namespace Enterprise.Customs.ZA.GUI
{
	public partial class CustomsEntryAndDiscardedMessagesUserControl : Customs.GUI.CustomsEntryAndDiscardedMessagesUserControl
	{
		System.ComponentModel.Container components;

		public CustomsEntryAndDiscardedMessagesUserControl()
		{
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		protected override Customs.GUI.BaseCustomsEntryUserControl GetMessageUserControl()
		{
			return new MessageUserControl();
		}
	}
}

