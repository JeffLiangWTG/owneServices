using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.LVS.Business
{
	public static class MessageSendingHelper
	{
		public static bool IsConsignmentNotWaitingForResponse(CusUSLVConsignment consignment)
		{
			return !consignment.Lookups.ULB_MessageStatusList.IsWaitingForResponse(consignment.ULB_MessageStatus);
		}

		public static bool IsConsignmentPendingForSendOriginalMessage(CusUSLVConsignment consignment)
		{
			return consignment.ULB_MessageStatus == ImportMessageStatusList.Codes.OriginalRequestPending;
		}

		public static bool HasValidStatusOnConsignment(CusUSLVConsignment consignment)
		{
			var result = false;
			if (consignment.Action != null)
			{
				switch (consignment.Action)
				{
					case UpdateActionCode.Add:
						result = consignment.CE_EntryStatus.IsEmpty && consignment.ULB_MessageStatus != ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
						break;
					case UpdateActionCode.Replace:
						result = !consignment.CE_EntryStatus.IsEmpty
							&& consignment.CE_EntryStatus != CRLReleaseStatusList.Codes.CAN
							&& consignment.CE_EntryStatus != CRLReleaseStatusList.Codes.DEL
							&& consignment.CE_EntryStatus != CRLReleaseStatusList.Codes.REL;
						break;
					case UpdateActionCode.Update:
						result = !consignment.CE_EntryStatus.IsEmpty
							&& consignment.CE_EntryStatus != CRLReleaseStatusList.Codes.CAN
							&& consignment.CE_EntryStatus != CRLReleaseStatusList.Codes.DEL;
						break;
					case UpdateActionCode.Delete:
						result = !consignment.CE_EntryStatus.IsEmpty
							&& consignment.CE_EntryStatus != CRLReleaseStatusList.Codes.CAN
							&& consignment.CE_EntryStatus != CRLReleaseStatusList.Codes.DEL;
						break;
				}
			}
			return result;
		}

		public static bool CheckHasMessageErrorsOnClearanceOrConsignment(CusUSLVClearance clearance, IEnumerable<CusUSLVConsignment> consignments)
		{
			var result = true;
			clearance.Validation.ValidateAll();
			ValidateConsignmentsIncludingChildren(consignments);
			result = clearance.HasMessageErrorsNotIncludingChildren || consignments.FirstOrDefault(x => x.HasMessageErrors) != null;
			return result;
		}

		public static IEnumerable<CusUSLVConsignmentForMessaging> GetConsignmentsWithoutMessageErrors(IEnumerable<CusUSLVConsignmentForMessaging> consignments)
		{
			return consignments.Where(x => !new USCustomsNotificationCollector(x.Consignment, true, false).HasMessageErrors());
		}

		public static CusUSLVConsignmentForMessaging GetFirstConsignmentHasMessageErrors(IEnumerable<CusUSLVConsignmentForMessaging> consignments)
		{
			return consignments.FirstOrDefault(x => new USCustomsNotificationCollector(x.Consignment, true, false).HasMessageErrors());
		}

		public static void ValidateConsignmentsIncludingChildren(IEnumerable<CusUSLVConsignment> consignments)
		{
			consignments.ForEach(x =>
			{
				x.MarkAsNeedingValidationIncludingChildren();
				x.RunPreSaveValidation();
			});
		}
	}
}
