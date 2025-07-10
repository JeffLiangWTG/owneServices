using CargoWise.EntityFramework;
using Enterprise.Security;

namespace Enterprise.Customs.Business.MessagingProcess
{
	public class CustomsSupervisorOverrides : SupervisorOverrides
	{
		public static class CustomsSupervisorOverridesContext
		{
			public const string SendMessagesWithErrors = "SendMessagesWithErrors";
		}

		public CustomsSupervisorOverrides(IBusiness businessEntity, SecurityCheckpoint securityContext, string context) : base(businessEntity, context)
		{
			this.securityContext = securityContext;
		}
		readonly SecurityCheckpoint securityContext;

		protected override void CreateMessagesCore()
		{
			switch (context)
			{
				case CustomsSupervisorOverridesContext.SendMessagesWithErrors:
					CheckSendMessagesWithErrors();
					break;
			}
		}

		internal void CheckSendMessagesWithErrors()
		{
			if (CheckSecurityRightForCheckpointRequired(securityContext))
			{
				AddMessageLog(securityContext, SendMessagesWithErrorsMessage);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message string")]
		const string SendMessagesWithErrorsMessage = "Sending with message errors";
	}
}
