using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.Common;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document
{
	sealed public class CIN750InNotificationBuilder : CIN750NotificationBuilder<WhsItemReceiveConsignment, CIN750InNotification>
	{
		public CIN750InNotificationBuilder(WhsItemReceiveConsignment rcn) : base(rcn)
		{
		}

		#region Details

		protected override (ZString RefType, ZString RefCode) GetRefTypeAndRefCode()
		{
			var refType = ZString.Empty;
			var refCode = ZString.Empty;

			if (!sourceBO.MasterBillNumber.IsEmpty)
			{
				refType = CIN750RefTypes.Codes.MasterAirWaybill;
				refCode = sourceBO.MasterBillNumber;
			}
			else if (!sourceBO.HouseBillNumber.IsEmpty)
			{
				refType = CIN750RefTypes.Codes.HouseAirWaybill;
				refCode = sourceBO.HouseBillNumber;
			}
			else
			{
				refType = CIN750RefTypes.Codes.Reference;
				refCode = TransitDocumentHelper.GetEnterpiseAndServerCode() + sourceBO.WRC_ConsignmentID;
			}
			return (refType, refCode);
		}

		protected override CIN750InNotification GetDocDataObject() =>
			new CIN750InNotification(
				nameof(DataContextType.TransitReceive),
				sourceBO.WRC_ConsignmentID);

		protected override OrgAddress GetDeclaredInWarehouse() => sourceBO.Warehouse?.WarehouseAddress;

		protected override void PopulateProperties(CIN750InNotification cinNotification)
		{
			var fromCTOWarehouse = GetFromCTOWarehouse(sourceBO);
			cinNotification.FromCTO = AddressBuilder.Create(context, fromCTOWarehouse);
			cinNotification.FromCTOCINNumber = fromCTOWarehouse?.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CIN);
			cinNotification.FromCTOCIN = cinNotification.FromCTOCINNumber?.Value ?? ZString.Empty;
			if (cinNotification.FromCTOCIN.IsEmpty)
			{
				cinNotification.FromCTOCIN = TransitDocDataConstants.NOTCIN;
			}

			cinNotification.CustomsStatus = TransitDocumentHelper.GetCustomsStatus(sourceBO);
		}

		protected override void PopulatePackingLines(CIN750InNotification cinNotification)
		{
			var packline = new CIN750InNotificationPackingLineBuilder(cinNotification).Build();
			cinNotification.Goods = new List<DocPackingLine>() { packline };
		}

		OrgAddress GetFromCTOWarehouse(WhsItemReceiveConsignment rcn) => rcn.CTODocAddress?.Address;

		protected override void AddExtraValidations(CIN750InNotification cinNotification)
		{
			cinNotification.FromCTOCINInfo.AddMessageErrorIfEmpty(Res.GetString("dd5a48eb-8145-4f13-a53e-263712a41d23", "From CTO CIN is required."));
		}

		#endregion
	}
}
