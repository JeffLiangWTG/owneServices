using System.Windows.Forms;

namespace CargoWise.eHub.BizTalkAdapters.Transferrer.UI
{
	public class AdapterManagementUI : IAdapterManagementUI
	{
		public static IAdapterManagementUI Instance { get; } = new AdapterManagementUI();

		public string GetPasswordPrompt(string prompt)
		{
			using (var passwordPrompt = new PasswordPrompt())
			{
				passwordPrompt.Prompt = prompt;
				return passwordPrompt.ShowDialog() == DialogResult.OK ? passwordPrompt.Password : null;
			}
		}
	}
}
