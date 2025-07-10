namespace Enterprise.Customs.US.Business.MessageProcessors
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Schema;

	public static class CusEntryHeaderMessageProcessingExtensionMethods
	{
		public static ZString[] GetEmailRecipients(this IEntryHeaderParentBusinessObject declaration, IEnumerable<EDIMessage> messages)
		{
			var result = new List<ZString>();

			foreach (var message in messages.OrderByDescending(x => x.EM_SystemCreateTimeUtc))
			{
				var staff = message.UserWhoQueuedThisRecord;
				if (staff != null && !staff.GS_EmailAddress.IsEmpty)
				{
					result.Add(staff.GS_EmailAddress);
					break;
				}
			}

			if (result.Count == 0)
			{
				if (declaration != null && declaration.CusAgent != null && !declaration.CusAgent.GS_EmailAddress.IsEmpty)
				{
					result.Add(declaration.CusAgent.GS_EmailAddress);
				}
			}

			return result.ToArray();
		}

		public static (ZString, bool) GetSuppresseMessageLogs(this IEntryHeaderParentBusinessObject declaration)
		{
			var reference = ZString.Empty;
			var redAndBold = true;
			var ssmlog = declaration.TopLevelBusinessObjectLogs.MostRecentLogByEventTime(Events.SuppressSendingMessage, new ZQuery(StmALogSchema.SL_IsCancelled, false));
			if (ssmlog != null)
			{
				reference += ssmlog.SL_Reference;
				if (reference.Contains("STU message suppressed because STU message is pending and the new statement date") && reference.Contains("is not different."))
				{
					redAndBold = false;
				}
			}
			return (reference, redAndBold);
		}
	}
}
