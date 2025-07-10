using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transit.Document
{
	public class CIN750DeconsNotificationLogsCreator : CIN750NotificationLogsCreator<CIN750DeconsNotification>
	{
		protected override bool CreateSentEvents(object logParent, Event @event, IDynamicData data, string documentName, string recipient, string reasonForSending = null)
		{
			if (logParent is IStmALogProvider logProvider && data?.Value is CIN750DeconsNotification notification)
			{
				var dcn = notification.SourceBusinessObject as WhsItemDispatchConsignment;
				var now = ZDateTimeOffset.Now;
				var logIDs = new List<string>();
				var currentNumber = GetCurrentHistoryMessageIdPairNumber(dcn);

				foreach (var fromToGoods in notification.GoodsPairs)
				{
					var fromGoods = fromToGoods.Item1;
					var fromLog = logProvider?.Logs.CreateRecreateOrUpdateEventLog(@event, EstimateActual.Actual, now, ZString.Empty, GetParametersFromRCN(fromGoods.SourceReceiveConsignment, fromGoods, documentName + NotificationHistoryInfo.FromHistorySuffix, fromGoods.RefType.Code, reasonForSending).ToArray());
					logIDs.Add(fromLog.PK.ToString());

					var toGoods = fromToGoods.Item2;
					var toLog = logProvider?.Logs.CreateRecreateOrUpdateEventLog(@event, EstimateActual.Actual, now, ZString.Empty, GetParametersFromDCN(dcn, toGoods, documentName + NotificationHistoryInfo.ToHistorySuffix, toGoods.RefType.Code, reasonForSending).ToArray());
					logIDs.Add(toLog.PK.ToString());

					dcn.PopulateAddOnValue($"{GetHistoryMessageIdPairAddOnValueType()}{currentNumber}", "STR", $"{fromGoods.DeconsMessageID}|{fromLog.PK}");
					currentNumber++;
					dcn.PopulateAddOnValue($"{GetHistoryMessageIdPairAddOnValueType()}{currentNumber}", "STR", $"{fromGoods.DeconsMessageID}|{toLog.PK}");
					currentNumber++;
				}
				dcn.Factory.Save();

				return true;
			}
			return false;
		}

		IEnumerable<KeyValuePair<string, string>> GetParametersFromDCN(WhsItemDispatchConsignment dcn, DocPackingLine packingLine, string documentName, string refType, string reasonForSending)
		{
			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName);

			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ParentType, refType);
			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, TransitDocumentHelper.GetEnterpiseAndServerCode() + dcn.WDC_ConsignmentID);
			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber, packingLine.SourceID);
			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MasterBill, FillWithHyphenIfEmpty(dcn.MasterBillNumber));
			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.HouseBill, FillWithHyphenIfEmpty(dcn.WDC_HouseBillNumber));
			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.OuterPackQuantity, packingLine.AmountQuantity.ToString());
			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Weight, packingLine.AmountWeight.ToString("F3"));
			if (!string.IsNullOrWhiteSpace(reasonForSending))
			{
				yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, reasonForSending);
			}

			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Argument, ZDateTime.Now.ToString("yyMMddHHmmssfff"));
		}

		IEnumerable<KeyValuePair<string, string>> GetParametersFromRCN(WhsItemReceiveConsignment rcn, DocPackingLine packingLine, string documentName, string refType, string reasonForSending)
		{
			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName);

			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ParentType, refType);
			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, TransitDocumentHelper.GetEnterpiseAndServerCode() + rcn.WRC_ConsignmentID);
			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber, rcn.WRC_JobID);
			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MasterBill, FillWithHyphenIfEmpty(rcn.MasterBillNumber));
			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.HouseBill, FillWithHyphenIfEmpty(rcn.WRC_HouseBillNumber));
			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.OuterPackQuantity, packingLine.AmountQuantity.ToString());
			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Weight, packingLine.AmountWeight.ToString("F3"));
			if (!string.IsNullOrWhiteSpace(reasonForSending))
			{
				yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, reasonForSending);
			}

			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Argument, ZDateTime.Now.ToString("yyMMddHHmmssfff"));
		}

		protected override IEnumerable<KeyValuePair<string, string>> GetNotificationParametersForEvent(CIN750DeconsNotification notification)
		{
			yield break;
		}

		public override string GetHistoryMessageIdPairAddOnValueType() => CIN750NotificationConstants.HistoryMessageIdPairAddOnValueType.Decons;
	}
}
