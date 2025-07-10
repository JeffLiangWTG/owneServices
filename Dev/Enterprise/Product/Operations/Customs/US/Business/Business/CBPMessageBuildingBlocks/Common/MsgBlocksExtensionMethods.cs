using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Output;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.US.Business
{
	static class MessageBlocksExtensionMethods
	{
		public static ZDecimal GetAmount(this IEnumerable<ENS89> ens89s, ZString feeType)
		{
			ZDecimal result = ZDecimal.Zero;

			foreach (ENS89 ens89 in ens89s)
			{
				if (ens89.ClassCode == feeType)
				{
					result = ens89.TotalAmount;
					break;
				}
				else if (ens89.ClassCode1 == feeType)
				{
					result = ens89.TotalAmount1;
					break;
				}
				else if (ens89.ClassCode2 == feeType)
				{
					result = ens89.TotalAmount2;
					break;
				}
				else if (ens89.ClassCode3 == feeType)
				{
					result = ens89.TotalAmount3;
					break;
				}
				else if (ens89.ClassCode4 == feeType)
				{
					result = ens89.TotalAmount4;
					break;
				}
			}

			return result;
		}

		public static ZBool IsFailed(this ENQJ9 enq9)
		{
			return enq9.NarrativeMessage != ACSABIProcessor.Constants.EntrySummaryQuery.BillDataStatus &&
					enq9.NarrativeMessage != ACSABIProcessor.Constants.EntrySummaryQuery.CollectionDataStatus;
		}

		public static ZString GetTeamNumber(this AENSE0 e0)
		{
			return e0.ReferenceDataTypeCode == EntrySummaryReferenceDataList.Codes.SUMMRY ? e0.ReferenceDataText.SubstringSafe(26, 3) : ZString.Empty;
		}

		public static void ProcessBillDispositionDetails(this ASESSO50Base so50, HtmlTableCreator creator)
		{
			var dispDateTime = so50.GetDispositionDateTime();
			var dispositionActionDate = so50.DispositionCode + " " + so50.NarrativeMessage + "/" + dispDateTime;
			creator.WriteRow("Disposition Action/Date", dispositionActionDate);
		}

		internal static ZDateTime GetDispositionDateTime(this ASESSO50Base so50)
		{
			return DateTimeParser.GetDateTimeFromZDateAndStringTime(so50.DispositionDate, so50.DispositionTime);
		}
	}
}
