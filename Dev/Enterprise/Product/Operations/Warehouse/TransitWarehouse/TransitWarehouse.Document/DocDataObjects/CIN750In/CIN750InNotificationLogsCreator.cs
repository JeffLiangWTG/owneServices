using System.Collections.Generic;
using System.Linq;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document
{
	public class CIN750InNotificationLogsCreator : CIN750NotificationLogsCreator<CIN750InNotification>
	{
		protected override IEnumerable<KeyValuePair<string, string>> GetNotificationParametersForEvent(CIN750InNotification notification)
		{
			var rcn = notification.SourceBusinessObject as WhsItemReceiveConsignment;
			yield return new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber,
				rcn.WRC_JobID);
			yield return new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MasterBill,
				FillWithHyphenIfEmpty(rcn.MasterBillNumber));
			yield return new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.HouseBill,
				FillWithHyphenIfEmpty(rcn.WRC_HouseBillNumber));
			yield return new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ParentType,
				notification.RefType.Code);
			yield return new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber,
				TransitDocumentHelper.GetEnterpiseAndServerCode() + rcn.WRC_ConsignmentID);
			yield return new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.OuterPackQuantity,
				notification.Goods == null ? "0" : notification.Goods.Sum(g => g.AmountQuantity).ToString());
			yield return new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Weight,
				notification.Goods == null ? "0.000" : notification.Goods.Sum(g => g.AmountWeight).ToString("F3"));
		}

		public override string GetHistoryMessageIdPairAddOnValueType() => CIN750NotificationConstants.HistoryMessageIdPairAddOnValueType.In;
	}
}
