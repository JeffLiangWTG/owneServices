using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WDFEDIMessage : EDIMessage
	{
		public WDFEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		protected override void AfterNewObjectIsLinked(BusinessObject newBizObj)
		{
			base.AfterNewObjectIsLinked(newBizObj);
			WhsDocket docket = newBizObj as WhsDocket;
			this.EM_MessageType = GetMessageType(docket.WD_DocketType);
		}

		protected ZString GetMessageType(ZString docketTypeValue)
		{
			ZString result = ZString.Empty;
			switch (docketTypeValue)
			{
				case DocketType.Codes.Receive:
					result = Enterprise.Warehouse.DataTransfer.CodeLists.NotificationTypes.Codes.WhsASN;
					break;
				case DocketType.Codes.Order:
					result = Enterprise.Warehouse.DataTransfer.CodeLists.NotificationTypes.Codes.WhsOrder;
					break;
			}
			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			this.EM_ApplicationCode = EDIMessage.ApplicationCodes.WarehouseDocket;
			this.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			this.EM_Status = EDIMessage.Status.Received;
			this.EM_IsTestMessage = ZBool.False;
		}

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			//Do Nothing
		}

		#endregion
	}
}
