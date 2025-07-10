using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Change Inventory Hold Code")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse ChangeInventoryHoldCode(Guid inventoryPK, string holdCode, string holdChangeReason, decimal qty)
		{
			return HandleWebServiceRequest<WebServiceResponse>(response => ChangeInventoryHoldCodeCore(response, inventoryPK, holdCode, holdChangeReason, qty));
		}

		void ChangeInventoryHoldCodeCore(WebServiceResponse response, Guid inventoryPK, string holdCode, string holdChangeReason, decimal qty)
		{
			var inventory = Factory.Load<WhsInventoryView>(inventoryPK);
			var docketLine = inventory?.InDocketLine;
			if (IsValidHoldCodeChange(response, inventory, docketLine, holdChangeReason, qty))
			{
				docketLine.HeldCodeToChangeTo = holdCode;
				docketLine.HoldReasonToChangeTo = holdChangeReason;
				docketLine.HeldCodeChangeQuantity = qty;

				if (docketLine.ChangeInventoryHeldCode(true))
				{
					Factory.Save();
				}
				else
				{
					response.LogBusinessValidationError(Res.GetString("31e1cea8-e80b-4eda-b947-4a67e6186e74", "Failed to change inventory hold code."));
				}
			}
		}

		bool IsValidHoldCodeChange(WebServiceResponse response, WhsInventoryView inventory, WhsDocketLine docketLine, string holdChangeReason, decimal qty)
		{
			var isValid = false;

			if (docketLine == null)
			{
				response.LogBusinessValidationError(Res.GetString("edd5fa43-0f33-4912-b500-ed569122d902", "This inventory does not exist in the warehouse."));
			}
			else if (holdChangeReason.Length > docketLine.HoldReasonToChangeToInfo.MaxLength)
			{
				response.LogBusinessValidationError(Res.GetString("83031f65-72ba-46ee-85ff-57f2361c42b0", "Hold Change Reason exceeds maximum length of {0} characters.", docketLine.HoldReasonToChangeToInfo.MaxLength));
			}
			else if (qty <= 0m)
			{
				response.LogBusinessValidationError(Res.GetString("8cc431be-491e-47aa-b171-9fce7c86e168", "Quantity cannot be negative or zero."));
			}
			else if (qty > inventory.WI_AvailableToTransferQuantity)
			{
				response.LogBusinessValidationError(Res.GetString("c4ba84d3-a782-4699-aaa1-81bcd8901981", "Quantity cannot be greater than Available To Transfer Quantity."));
			}
			else
			{
				isValid = true;
			}

			return isValid;
		}
	}
}
