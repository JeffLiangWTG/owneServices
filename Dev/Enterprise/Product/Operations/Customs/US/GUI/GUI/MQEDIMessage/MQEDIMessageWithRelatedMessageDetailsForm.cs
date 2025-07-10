using Enterprise.Customs.US.Business;
using Enterprise.Messaging.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class MQEDIMessageWithRelatedMessageDetailsForm : MQEDIMessageForm
	{
		public MQEDIMessageWithRelatedMessageDetailsForm(MQEDIMessage message)
			: base(message)
		{
			InitializeComponent();
		}

		protected override EDIMessageStandAloneUserControl GetNewMessageDetailUserControl()
		{
			return new MQEDIMessageWithRelatedMessageDetailsUserControl();
		}
	}
}
