using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.Messaging.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class MQEDIMessageForm : EDIMessageForm
	{
		public MQEDIMessageForm(MQEDIMessage message)
			: base(message)
		{
			InitializeComponent();
		}

		protected override EDIMessageStandAloneUserControl GetNewMessageDetailUserControl()
		{
			return new MQEDIMessageUserControl();
		}

#if DEBUG
		internal TabControl MessageDetailsTabControlExposedForTest => ((MQEDIMessageUserControl)MessageControlExposedForTest).MessageDetailsTabControl;
#endif

	}
}
