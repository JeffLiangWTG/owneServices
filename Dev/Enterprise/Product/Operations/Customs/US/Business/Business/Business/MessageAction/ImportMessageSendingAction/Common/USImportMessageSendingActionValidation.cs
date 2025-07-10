using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.US.Business
{
	public class USImportMessageSendingActionValidation : AutoUSImportMessageSendingActionValidation
	{
		public USImportMessageSendingActionValidation(AutoUSImportMessageSendingAction parent)
			: base(parent)
		{
		}

		protected new ImportMessageSendingAction Parent
		{
			get { return (ImportMessageSendingAction)base.Parent; }
		}

		protected override void CheckUS_SE_ContactPhone()
		{
			base.CheckUS_SE_ContactPhone();

			if (!Parent.US_SE_ContactPhone.IsEmpty && Parent.IsSE13DataRelevant && !Parent.US_SE_ContactPhone.IsNumbersOnlyOrEmpty)
			{
				Parent.US_SE_ContactPhoneInfo.AddMessageError(InValidContactPhoneMessage);
			}
		}
		internal const string InValidContactPhoneMessage = "Invalid Contact Phone Number format: should be only numerics.";

		protected override void CheckUS_SendMessage()
		{
			base.CheckUS_SendMessage();

			JobDeclaration declaration = Parent.Declaration;

			if (Parent.US_SendMessage && declaration != null)
			{
				if (Parent.actions.messageSendingMessageType == ImportMessageSendingMessageType.Deletion)
				{
					if (Parent.IsACECargoRelease)
					{
						List<CusPermitHeader> relatedPermits = declaration.FindRelatedPermits();
						bool hasOrders = relatedPermits.Any(p => p.HasNonZeroOrderBalance());
						if (hasOrders)
						{
							Parent.US_SendMessageInfo.AddError(WeeklyEstimatesUsedForOrders);
						}
					}
				}
				else if (Parent.actions.messageSendingMessageType == ImportMessageSendingMessageType.Original)
				{
					List<CusPermitHeader> relatedPermits = declaration.FindRelatedPermits();

					if (Parent.IsEntrySummary && relatedPermits.Any(p => p.HasPendingTransaction()))
					{
						Parent.US_SendMessageInfo.AddError(WeeklyEstimatesEntrySummaryMustWaitForAllFinalizedOrders);
					}
				}
				else if (Parent.actions.messageSendingMessageType == ImportMessageSendingMessageType.Replacement)
				{
					List<CusPermitHeader> relatedPermits = declaration.FindRelatedPermits();

					if (Parent.IsEntrySummary && relatedPermits.Any(p => p.HasPendingTransaction()))
					{
						Parent.US_SendMessageInfo.AddError(WeeklyEstimatesEntrySummaryMustWaitForAllFinalizedOrders);
					}
					else if (Parent.IsACECargoRelease && relatedPermits.Count > 0)
					{
						Parent.US_SendMessageInfo.AddError(WeeklyEstimatesCannotBeUpdated);
					}
				}
			}
		}

		internal const string WeeklyEstimatesUsedForOrders = "This weekly estimate has warehouse orders against it and cannot be deleted.";
		internal const string WeeklyEstimatesCannotBeUpdated = "Weekly Estimates cannot be updated or replaced. You must file a supplemental weekly estimate instead.";
		internal static string WeeklyEstimatesEntrySummaryMustWaitForAllFinalizedOrders => "All orders should be finalized before sending.";
	}
}
