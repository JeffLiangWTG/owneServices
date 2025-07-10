using System.Drawing;
using System.Windows.Forms;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public partial class GenralMessageForm : ZTemplateForm, IPreviousNextControlProvider
	{
		public GenralMessageForm(GENRALMessage businessEntity) : base(businessEntity)
		{
			ControllerID = ZAControllerIDs.GenralMessage;
			UserIdleWorker.QueueWorkItem(this, 0, new MethodInvoker(RefreshWebBrowser), null);
		}

		protected GENRALMessage GenralMessage => (GENRALMessage)BusinessEntity;

		protected override bool SupportsEDocs => false;

		public override Size MinimumSize
		{
			get { return CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 342); }
			set { base.MinimumSize = value; }
		}

		void RefreshWebBrowser()
		{
			zWebBrowser1.DocumentText = GenralMessage.EM_MessageInterpretation;
		}
	}
}
