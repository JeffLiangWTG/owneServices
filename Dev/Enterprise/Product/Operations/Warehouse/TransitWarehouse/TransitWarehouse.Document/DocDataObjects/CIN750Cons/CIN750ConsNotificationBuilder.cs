using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.Common;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document
{
	sealed public class CIN750ConsNotificationBuilder : CIN750NotificationBuilder<WhsItemDispatchConsignment, CIN750ConsNotification>
	{
		public CIN750ConsNotificationBuilder(WhsItemDispatchConsignment dcn, ConsNotificationAdditionalData additionalData) : base(dcn)
		{
			if (additionalData != null)
			{
				AdditionalData = additionalData;
			}
			else
			{
				var dcnHistoryManager = new DispatchConsignmentCIN750NotificationHistoryManager(dcn);
				dcnHistoryManager.GetNextMessageTypeCore();
				AdditionalData = dcnHistoryManager.AdditionalDataForCons;
			}
		}

		protected override OrgAddress GetDeclaredInWarehouse() => sourceBO.Warehouse.WarehouseAddress;

		protected override CIN750ConsNotification GetDocDataObject() => new CIN750ConsNotification(nameof(WhsItemDispatchConsignment), sourceBO.WDC_ConsignmentID);

		protected override (ZString RefType, ZString RefCode) GetRefTypeAndRefCode()
		{
			var refType = ZString.Empty;
			var refCode = ZString.Empty;
			var houseBill = sourceBO.HouseBillNumber;
			var masterBill = sourceBO.MasterBillNumber;

			if (!masterBill.IsEmpty)
			{
				refType = CIN750RefTypes.Codes.MasterAirWaybill;
				refCode = masterBill;
			}
			else if (!houseBill.IsEmpty)
			{
				refType = CIN750RefTypes.Codes.HouseAirWaybill;
				refCode = houseBill;
			}

			return (refType, refCode);
		}

		protected override void PopulatePackingLines(CIN750ConsNotification cinNotification)
		{
			if (AdditionalData != null)
			{
				var fromToGoods = new CIN750ConsNotificationPackingLineBuilder(cinNotification, AdditionalData).BuildDocPackingLines();
				cinNotification.FromGoods = fromToGoods.FromPackingLine.ToList();
				cinNotification.ToGoods = fromToGoods.ToPackingLine;
			}
			else
			{
				cinNotification.FromGoods = new List<DocPackingLine>();
				cinNotification.ToGoods = new DocPackingLine(ZGuid.NewZGuid());
			}
		}

		ConsNotificationAdditionalData AdditionalData { get; }
	}
}
