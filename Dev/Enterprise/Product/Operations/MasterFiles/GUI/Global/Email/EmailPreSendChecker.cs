using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public interface IEmailPreSendChecker
	{
		bool PromptUserIfSendingToNdrRecipients(IEnumerable<ZString> emailAddresses);
	}

	public class EmailPreSendChecker : IEmailPreSendChecker
	{
		public bool PromptUserIfSendingToNdrRecipients(IEnumerable<ZString> emailAddresses)
		{
			var factory = new BusinessObjectFactory();
			var nonDeliveryFilter = new ZQuery(GlbEmailAddressSchema.GI_DeliveryStatus, EmailDeliveryReportStatus.Codes.NonDeliveryReport);
			var ndrEmailAddresses = GlbEmailAddress.Load(factory, emailAddresses, nonDeliveryFilter);
			if (ndrEmailAddresses.Length == 0)
			{
				return true;
			}

			var shouldContinue = Globals.Message.Show(
				Res.GetString("d171a8df-1d76-4c19-a0f9-a7e6d0a15150", "At least one recipient has received a Non-Delivery Receipt. Do you want to continue to send to these recipients?"),
				Res.GetString("7d450b70-63d5-4c9e-b303-c240a5ce7742", "Delivering to Non-Delivery recipients"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				DialogResult.No) == DialogResult.Yes;

			if (shouldContinue)
			{
				foreach (var glbEmailAddress in ndrEmailAddresses)
				{
					glbEmailAddress.GI_DeliveryStatus = ZString.Empty;
				}

				try
				{
					factory.Save();
				}
				catch (ZSaveException)
				{
					// Ignore. Don't stop sending of email
				}
			}

			return shouldContinue;
		}
	}
}
