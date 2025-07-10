using Enterprise.Customs.US.eManifest.Business;

namespace Enterprise.Customs.US.eManifest.GUI
{
	public partial class MessagesUserControl : Customs.GUI.MessagesUserControl
	{
		public MessagesUserControl()
			: base()
		{
			this.Layout += AddMessageTabPagessInSpecificOrderSoHtmlInterpretationBoxCanDisplayCorrectly;
		}

		public void ReorderTabPages()
		{
			MessageTabControl.TabPages.Remove(this.MessageTextTabPage);
			MessageTabControl.TabPages.Insert(this.MessageTextTabPage, 0);

			MessageTabControl.TabPages.Remove(this.MessageDetailsTabPage);
			MessageTabControl.TabPages.Insert(this.MessageDetailsTabPage, 1);
		}

		void AddMessageTabPagessInSpecificOrderSoHtmlInterpretationBoxCanDisplayCorrectly(object sender, System.Windows.Forms.LayoutEventArgs e)
		{
			var trip = CurrentDataItem as Trip;
			if (trip != null && trip.IsFromHVLV)
			{
				ReorderTabPages();
			}
		}
	}
}
