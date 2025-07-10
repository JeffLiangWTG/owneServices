using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.Common;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document
{
	public sealed class CIN750DeconsNotificationBuilder : CIN750NotificationBuilder<WhsItemDispatchConsignment, CIN750DeconsNotification>
	{
		public CIN750DeconsNotificationBuilder(WhsItemDispatchConsignment dcn) : base(dcn) { }

		protected override OrgAddress GetDeclaredInWarehouse() => sourceBO.Warehouse.WarehouseAddress;

		protected override CIN750DeconsNotification GetDocDataObject() => new CIN750DeconsNotification(nameof(DataContextType.TransitDispatch), sourceBO.WDC_ConsignmentID);

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

		protected override void PopulatePackingLines(CIN750DeconsNotification cinNotification)
		{
			cinNotification.GoodsPairs = new CIN750DeconsNotificationPackingLineBuilder(cinNotification).BuildDocPackingLines().ToList();
		}
	}
}
