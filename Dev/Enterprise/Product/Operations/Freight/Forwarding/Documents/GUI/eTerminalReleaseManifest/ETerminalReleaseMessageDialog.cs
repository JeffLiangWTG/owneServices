using System;
using System.Windows.Forms;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Documents.GUI
{
	public partial class ETerminalReleaseMessageDialog : ZChildForm
	{
		public ETerminalReleaseMessageDialog(JobVoyage voyage)
			: base(new ETerminalReleaseMessage(voyage))
		{
			InitializeComponent();
		}

		ETerminalReleaseMessage Message => (ETerminalReleaseMessage)BusinessEntity;

		void MessagingConsolsDoubleClick(object sender, MouseEventArgs e)
		{
			if (messageConsolsGrid?.ListManager?.GetCurrent() is ETerminalReleaseMessageConsol message
				&& message.Consol != null)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.JobConsol);

				controller.SetFormsModalTo(this);
				controller.ShowEditForm(message.Consol);
			}
		}

		public override string FormHeading => Res.GetString("f9dc5703-d4f5-486f-a04a-57b483713534", "eTerminal Release Message");

		#region Message Sending

		void SendButtonClicked(object sender, EventArgs e)
		{
			if (CheckAllowSendMessage())
			{
				Message.SendMessage(ProgressLog);
			}
		}

		bool CheckAllowSendMessage()
		{
			var eTerminalReleaseManifestMenu = Message?.Factory.Load<IStmMenuItem>(ConsolSystemFormMenuItems.DocumentMenuETerminalReleaseManifestCNPK);

			if (eTerminalReleaseManifestMenu != null)
			{
				var securityService = new DocumentSecurityService(eTerminalReleaseManifestMenu, ModuleIDs.JobConsol);

				if (!securityService.CanSendMessage)
				{
					securityService.ShowSendMessageError();

					return false;
				}
			}

			return true;
		}

		#endregion
	}
}
