using System.Collections.Generic;
using System.Linq;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document
{
	public class CIN750OutNotificationLogsCreator : CIN750NotificationLogsCreator<CIN750OutNotification>
	{
		protected override IEnumerable<KeyValuePair<string, string>> GetNotificationParametersForEvent(CIN750OutNotification notification)
		{
			var dcn = notification.SourceBusinessObject as WhsItemDispatchConsignment;
			yield return new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber,
				dcn.WDC_ConsignmentID);
			yield return new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MasterBill,
				FillWithHyphenIfEmpty(dcn.MasterBillNumber));
			yield return new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.HouseBill,
				FillWithHyphenIfEmpty(dcn.WDC_HouseBillNumber));
			yield return new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ParentType,
				notification.RefType.Code);
			yield return new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber,
				TransitDocumentHelper.GetEnterpiseAndServerCode() + dcn.WDC_ConsignmentID);
			yield return new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.OuterPackQuantity,
				notification.Goods == null ? "0" : notification.Goods.Sum(g => g.AmountQuantity).ToString());
			yield return new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Weight,
				notification.Goods == null ? "0" : notification.Goods.Sum(g => g.AmountWeight).ToString());
		}

		public override string GetHistoryMessageIdPairAddOnValueType() => CIN750NotificationConstants.HistoryMessageIdPairAddOnValueType.Out;
	}
}
