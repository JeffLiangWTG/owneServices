using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.Common;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document
{
	sealed public class CIN750CorNotificationBuilder : CIN750NotificationBuilder<WhsItemReceiveConsignment, CIN750CorNotification>
	{
		public CIN750CorNotificationBuilder(WhsItemReceiveConsignment rcn) : base(rcn)
		{
		}

		#region Details

		bool HasMasterBill(WhsItemReceiveConsignment rcn) => !((IConsignment)rcn).MasterBillNumber.IsEmpty;

		protected override (ZString RefType, ZString RefCode) GetRefTypeAndRefCode()
		{
			var refType = ZString.Empty;
			var refCode = ZString.Empty;

			if (HasMasterBill(sourceBO))
			{
				refType = CIN750RefTypes.Codes.MasterAirWaybill;
				refCode = ((IConsignment)sourceBO).MasterBillNumber;
			}
			else
			{
				refType = CIN750RefTypes.Codes.Reference;
				refCode = TransitDocumentHelper.GetEnterpiseAndServerCode() + sourceBO.WRC_ConsignmentID;
			}
			return (refType, refCode);
		}

		protected override CIN750CorNotification GetDocDataObject() =>
			new CIN750CorNotification(
				nameof(WhsItemReceiveConsignment),
				sourceBO.WRC_ConsignmentID);

		protected override OrgAddress GetDeclaredInWarehouse() => sourceBO.Warehouse?.WarehouseAddress;

		protected override void PopulatePackingLines(CIN750CorNotification cinNotification)
		{
			var packline = new CIN750CorNotificationPackingLineBuilder(cinNotification).Build();
			cinNotification.Goods = new List<DocPackingLine>() { packline };
		}

		#endregion
	}
}
