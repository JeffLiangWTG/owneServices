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
	public class CIN750ConsNotificationLogsCreator : CIN750NotificationLogsCreator<CIN750ConsNotification>
	{
		protected override bool CreateSentEvents(object logParent, Event @event, IDynamicData data, string documentName, string recipient, string reasonForSending = null)
		{
			if (logParent is IStmALogProvider logProvider && data?.Value is CIN750ConsNotification notification)
			{
				var now = ZDateTimeOffset.Now;
				var fromGoods = notification.FromGoods;
				var logIDs = new List<ZGuid>();
				var dcn = notification.SourceBusinessObject as WhsItemDispatchConsignment;
				foreach (var fromGood in fromGoods)
				{
					var fromLog = fromGood.SourceID.StartsWith("RC") ?
					logProvider?.Logs.CreateRecreateOrUpdateEventLog(@event, EstimateActual.Actual, now, ZString.Empty, GetParametersFromRCN(fromGood, fromGood.SourceReceiveConsignment, documentName + NotificationHistoryInfo.FromHistorySuffix, reasonForSending).ToArray()) :
					logProvider?.Logs.CreateRecreateOrUpdateEventLog(@event, EstimateActual.Actual, now, ZString.Empty, GetParametersFromDCN(fromGood, dcn, documentName + NotificationHistoryInfo.FromHistorySuffix, reasonForSending).ToArray());
					logIDs.Add(fromLog.PK);
				}

				var toGoods = notification.ToGoods;
				var toLog = logProvider?.Logs.CreateRecreateOrUpdateEventLog(@event, EstimateActual.Actual, now, ZString.Empty, GetParametersFromDCN(toGoods, dcn, documentName + NotificationHistoryInfo.ToHistorySuffix, reasonForSending).ToArray());
				logIDs.Add(toLog.PK);

				var currentNumber = GetCurrentHistoryMessageIdPairNumber(dcn);
				foreach (var logID in logIDs)
				{
					dcn.PopulateAddOnValue($"{GetHistoryMessageIdPairAddOnValueType()}{currentNumber}", "STR", $"{notification.MessageID}|{logID}");
					currentNumber++;
				}
				dcn.Factory.Save();

				return true;
			}
			return false;
		}

		IEnumerable<KeyValuePair<string, string>> GetParametersFromDCN(DocPackingLine packingLine, WhsItemDispatchConsignment dcn, string documentName, string reasonForSending)
		{
			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName);

			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ParentType, packingLine.RefType.Code);
			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, TransitDocumentHelper.GetEnterpiseAndServerCode() + dcn.WDC_ConsignmentID);
			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber, dcn.WDC_JobID);
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

		IEnumerable<KeyValuePair<string, string>> GetParametersFromRCN(DocPackingLine packingLine, WhsItemReceiveConsignment rcn, string documentName, string reasonForSending)
		{
			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, documentName);

			yield return new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ParentType, packingLine.RefType.Code);
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

		protected override IEnumerable<KeyValuePair<string, string>> GetNotificationParametersForEvent(CIN750ConsNotification notification)
		{
			yield break;
		}

		public override string GetHistoryMessageIdPairAddOnValueType() => CIN750NotificationConstants.HistoryMessageIdPairAddOnValueType.Cons;
	}
}
