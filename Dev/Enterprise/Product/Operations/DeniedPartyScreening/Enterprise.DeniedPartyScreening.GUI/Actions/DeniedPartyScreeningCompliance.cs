using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DeniedPartyScreening.GUI
{
	static class DeniedPartyScreeningCompliance
	{
		internal static bool HasAcknowledgedRisk(string message)
		{
			var result = Globals.Message.ShowConfirmation(message,
				Res.GetString("A4664A60-6B91-4D6B-9621-671700DAEBC0", "CRITICAL WARNING"),
				Res.GetString("C4D9F7CB-FB50-4A0D-9412-56CB1E59F6F7", "I understand the impact"), MessageBoxIcon.Warning);

			return result == DialogResult.OK;
		}
	}
}
