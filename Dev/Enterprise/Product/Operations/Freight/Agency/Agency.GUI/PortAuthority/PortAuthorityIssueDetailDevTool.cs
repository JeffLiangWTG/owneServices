using System.Diagnostics;
using System.Windows.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.GUI
{
	internal sealed class PortAuthorityIssueDetailDevTool : IDevTool
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Diagnostics Tool")]
		public const string Name = "Show Issue Detail";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Diagnostics Tool")]
		public void Show(Form form)
		{
			PortAuthorityFilterDialog dialog = form as PortAuthorityFilterDialog;

			string caption;
			string message;

			if (dialog == null)
			{
				caption = (NoResString)"Cannot Show Issue Details";
				message = (NoResString)"Invalid Form";
			}
			else
			{
				PortMessageIssue issue = dialog.CurrentIssue;

				if (issue == null)
				{
					caption = "Cannot Show Issue Details";
					message = "No issue selected";
				}
				else
				{
					caption = "Issue Details - " + issue.Text;
					message = issue.Detail;
				}
			}

			Globals.Message.Show(message, caption, MessageBoxButtons.OK, DialogResult.OK);
		}

		#region IDevTool Members

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		bool IDevTool.AddAsButton
		{
			[DebuggerStepThrough]
			get { return false; }
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		string IDevTool.Name
		{
			[DebuggerStepThrough]
			get { return Name; }
		}

		#endregion
	}
}


