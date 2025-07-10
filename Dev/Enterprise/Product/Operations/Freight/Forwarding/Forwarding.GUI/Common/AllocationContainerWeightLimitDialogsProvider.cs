using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core.Environment;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public sealed class AllocationContainerWeightLimitDialogsProvider : IAllocationContainerWeightLimitDialogsProvider
	{
		public static void Register(BusinessObjectFactory factory)
		{
			factory?.SetValue<IAllocationContainerWeightLimitDialogsProvider, AllocationContainerWeightLimitDialogsProvider>();
		}

		#region Allow Override Dialog

		bool IAllocationContainerWeightLimitDialogsProvider.ConfirmContainerWeightLimitOverride(string message)
		{
			var result = UserNotification.Instance.Show(message, Res.GetString("fd8c1317-5b60-46ae-a077-41f35e9c4ce6", "Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
			return result == DialogResult.Yes;
		}

		#endregion

		#region Escalate User Dialog

		bool IAllocationContainerWeightLimitDialogsProvider.AllowContainerWeightLimitOverrideBySecurityRole(string message)
		{
			while (true)
			{
				using var logForm = new UserEscalationLoginForm();

				logForm.Message = message;
				var dlgResult = ZFormModaliser.ShowDialogWithoutDispose(logForm);

				if (dlgResult != DialogResult.OK)
				{
					return false;
				}

				if (logForm.Credentials?.UserSecurity?.FindCheckPoint(Env.Security.ApproveAllocationExceedContainerWeightLimit.LookupKey).IsAllowed ?? false)
				{
					return true;
				}

				Globals.Message.Show(
					Res.GetString("58eba8ed-ba31-43dd-afe7-96d939dc787d", "Login failed. Please check your username and password, and try again."),
					Res.GetString("1153359a-e793-4b31-a29b-b2893a173df9", "Invalid Credentials"),
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}

		#endregion
	}
}
