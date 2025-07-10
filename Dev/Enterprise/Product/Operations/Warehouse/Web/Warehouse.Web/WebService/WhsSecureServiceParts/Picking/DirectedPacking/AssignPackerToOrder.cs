using System;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Assign packer to the order in the packing station.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public AssignPackerToOrderWebServiceResponse AssignPackerToOrder(Guid orderPK)
		{
			return HandleWebServiceRequest<AssignPackerToOrderWebServiceResponse>(r => AssignPackerToOrderCore(r, orderPK));
		}

		void AssignPackerToOrderCore(AssignPackerToOrderWebServiceResponse response, Guid orderPK)
		{
			var docket = Factory.Load<WhsDocket>(orderPK);
			if (docket != null && docket is WhsOrder order)
			{
				ValidateAndAssignPackerToOrder(response, order);
			}
			else
			{
				response.LogBusinessValidationError(OrderDoesNotExistError);
			}
		}

		void ValidateAndAssignPackerToOrder(AssignPackerToOrderWebServiceResponse response, WhsOrder order)
		{
			var user = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			ValidateWhsOrder(response, order, user);
			if (response.NoError() && order.WD_GS_NKAssignedPacker.IsEmpty)
			{
				order.WD_GS_NKAssignedPacker = user.GS_Code;
				WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) =>
				{
					response.IsAssignedToAnotherPacker = true;
					return Res.GetString("b30f0f1f-c5c4-4544-bb57-6544e7da19f6", "Another user has taken the order.");
				});
			}

			if (response.NoError())
			{
				response.AssignedPackerCode = order.WD_GS_NKAssignedPacker;
			}
		}

		static void ValidateWhsOrder(AssignPackerToOrderWebServiceResponse response, WhsOrder order, GlbStaff user)
		{
			if (!order.WD_WP.IsValid)
			{
				response.LogBusinessValidationError(Res.GetString("ff6b47f6-51c2-4047-a260-1ecd7a23a74d", "Order '{0}' is not picked.", order.WD_ExternalReference));
			}
			else if (!order.WD_GS_NKAssignedPacker.IsEmpty && !order.WD_GS_NKAssignedPacker.EqualsIgnoringCase(user.GS_Code))
			{
				response.LogBusinessValidationError(Res.GetString("dc32a518-d93f-4016-95f7-0a5bdbc11f38", "Order '{0}' is already assigned to '{1}'.", order.WD_ExternalReference, order.WD_GS_NKAssignedPacker));
				response.IsAssignedToAnotherPacker = true;
			}
			else if (!order.WarehouseOrderStatus.Equals(WhsOrderStatus.Codes.ReadyToPack))
			{
				response.LogBusinessValidationError(GetOrderNotReadyToPackError(order.WD_ExternalReference));
			}
			else if (!DoesOrderHaveUnpackedItems(order))
			{
				response.LogBusinessValidationError(GetOrderNothingToPackError(order.WD_ExternalReference));
			}
		}

		static bool DoesOrderHaveUnpackedItems(WhsOrder order)
		{
			var packageJob = order.PackageJob;
			var hasUnpackedItems = true;
			if (packageJob != null)
			{
				var pickLines = order.Lines.SelectMany(line => line.PickLines).ToArray();
				hasUnpackedItems = pickLines.Any(pl => !packageJob.IsPacked(pl));
			}

			return hasUnpackedItems;
		}
	}
}
