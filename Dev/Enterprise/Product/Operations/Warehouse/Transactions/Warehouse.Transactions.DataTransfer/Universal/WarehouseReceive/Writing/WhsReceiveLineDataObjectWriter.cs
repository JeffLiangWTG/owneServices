using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsReceiveLineDataObjectWriter : WhsDocketLineDataObjectWriter<WhsReceiveLine>
	{
		internal WhsReceiveLineDataObjectWriter(IDataWritingManager manager) : base(manager) { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		protected override void PopulateDataObject(OrderLine receiveLineDataObject, WhsReceiveLine receiveLineBO)
		{
			receiveLineDataObject.Consignee = new JobDocAddressDataObjectWriter(writeManager).GetDataObject(receiveLineBO.ConsigneeDocAddress);
			receiveLineDataObject.ExpectedQuantity = receiveLineBO.WE_ClientOrderedUnits;
			receiveLineDataObject.CrossDockOrderNumber = receiveLineBO.WE_ReceiveCrossDockOrderNo;
			receiveLineDataObject.PalletID = receiveLineBO.WE_PalletID;

			receiveLineDataObject.RequiredBy = receiveLineBO.WE_RequiredByDate;
			receiveLineDataObject.Status = ListHelper.GetWithDescription<CodeDescriptionPair>(receiveLineBO.WE_CurrentInventoryStatus, new InventoryStatus());

			var holdCode = receiveLineBO.WE_WHC_NKOriginalInventoryHeldCode;
			receiveLineDataObject.OriginalHoldCode = ListHelper.GetWithDescription<CodeDescriptionPair9Char>(holdCode, receiveLineBO.Lookups.InventoryHeldCodeCollection);
			receiveLineDataObject.CurrentHoldReason = receiveLineBO.WE_CurrentHoldReason;
		}
	}
}
