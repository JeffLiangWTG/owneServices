using System.Windows.Forms;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.CFS.GUI
{
	public abstract class ConstantsAndReusablesGUI
	{
		protected ConstantsAndReusablesGUI()
		{
		}

		#region Changing Job IDs

		public static string ChangingJobNumberWarning
		{
			get
			{
				return Res.GetString("8ad420cf-91e0-4c7e-b7b7-d8c6b3a68a9f", "Warning: The current CFS job number is about to permanently change to a forwarding number.\r\nThis is happening because you have changed the client to your Organization Proxy.\r\nIf you do not want this to happen, undo your change.\r\n\r\nContinue saving?");
			}
		}

		public static string ChangingJobNumberWarningCaption
		{
			get { return Res.GetString("1e8c9e0d-33f4-47b8-83e3-d77476e7f13c", "Allow change of Job Number?"); }
		}

		public static DialogResult AskInGUIAboutChangingJobID(CFSShipment shipment)
		{
			DialogResult result = DialogResult.Yes;

			if (shipment.OnSaveWillChangeFromCFSJobNumberToForwardingJobNumber)
			{
				result = Globals.Message.Show(ChangingJobNumberWarning, ChangingJobNumberWarningCaption,
					MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			}

			return result;
		}

		public static DialogResult AskInGUIAboutChangingJobID(CFSLoadListConsol loadList)
		{
			DialogResult result = DialogResult.Yes;

			if (loadList.OnSaveWillChangeFromCFSJobNumberToForwardingJobNumber) // grrr, stupid no multiple inheritance in C#!!
			{
				result = Globals.Message.Show(ChangingJobNumberWarning, ChangingJobNumberWarningCaption,
					MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			}

			return result;
		}

		#endregion
	}
}
