using System;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.GUI
{
	public partial class MQEDIMessageWithRelatedMessageDetailsUserControl : MQEDIMessageUserControl
	{
		public MQEDIMessageWithRelatedMessageDetailsUserControl()
		{
			InitializeComponent();
		}

		MQEDIMessage Message
		{
			get { return (MQEDIMessage)CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Message != null && Message.RelatedMessage != null)
			{
				MessageDetailsTabControl.SelectTab(ReceivedMessageDetailsTabPage);
			}
		}
	}
}
