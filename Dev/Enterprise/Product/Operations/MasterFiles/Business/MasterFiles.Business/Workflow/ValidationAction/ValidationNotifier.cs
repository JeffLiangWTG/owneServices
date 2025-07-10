using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Workflow.ValidationAction
{
	public class ValidationNotifier
	{
		internal ValidationNotifier(ZString recipientEmailAddress, BusinessObject entityValidated, BusinessObjectFactory factory)
		{
			this.recipientEmailAddress = recipientEmailAddress;
			this.entityValidated = Argument.NotNull(entityValidated, "entityValidated");
			this.Factory = factory;
		}

		readonly ZString recipientEmailAddress;
		readonly BusinessObject entityValidated;
		readonly BusinessObjectFactory Factory;

		internal void SendValidationFailureNotification(string[] errors, string url = "")
		{
			var email = new HtmlNotificationEmailSender().CreateEmail(Subject, GetFormattedBody(url, errors));
			email.AddRecipientForUserCommunication(recipientEmailAddress);
			Env.OutgoingMailManager.Create(Factory, email);
		}

		ZString Subject
		{
			get { return Res.GetString("df1dc229-22d8-4f29-ba87-103e7d9d9dd0", "Validation Failure on {0}.", entityValidated.HumanReadableName); }
		}

		ZString GetFormattedBody(string url, string[] errors)
		{
			var hrefJobNumber = !string.IsNullOrEmpty(url) ? ((NoResString)"<a href=\"" + url + (NoResString)"\">" + entityValidated.HumanReadableName + (NoResString)"</a>") : entityValidated.HumanReadableName.ToString();
			return Res.GetString("ec6c3aba-fc62-4f6a-a082-74c801524cd0", "Validation was run on {0}, and the following failures occurred:-<br><br>\r\n{1}", hrefJobNumber, string.Join("<br>\r\n", errors));
		}
	}
}
