using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration.Customs.PermitService;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class FTZOrderPermitManager
	{
		public static ActionResult IsPermitAvailable(WhsOrder orderToCheck, IEnumerable<WhsOrderLine> ftzOrderLines)
		{
			return CheckPermitsForOrderLines(new[] { orderToCheck }, ftzOrderLines,
				r => ObjectFactory.Get<IPermitService>().IsPermitAvailable(r), "IsPermitAvailable()", shouldSetCustomsDataOnSuccess: false);  // Invalid error messages are not being translated.
		}

		public static ActionResult TryAquireFTZPermit(IEnumerable<WhsOrder> ordersToCheck, IEnumerable<WhsOrderLine> ftzOrderLines)
		{
			return CheckPermitsForOrderLines(ordersToCheck, ftzOrderLines,
				r => ObjectFactory.Get<IPermitService>().TryGetPermits(r), "TryGetPermits()", shouldSetCustomsDataOnSuccess: true); // Invalid error messages are not being translated.
		}

		static ActionResult CheckPermitsForOrderLines(IEnumerable<WhsOrder> ordersToCheck, IEnumerable<WhsOrderLine> ftzOrderLines,
			Func<IEnumerable<IPermitWithdrawRequest>, IPermitWithdrawRequestResponseResult> getPermitRequestResponses, string invalidErrorMessage, bool shouldSetCustomsDataOnSuccess)
		{
			var result = ActionResult.Success();

			if (ftzOrderLines.Any())
			{
				// remove permit row warning from previous save attempt (user might try to save multiple times until it succeeds, for example)
				var permitRowWarningMessageForOrder = Res.GetString("422299F8-9DBC-4DCA-8E7C-544A5C215356", "Not all Order Lines could be granted a weekly estimate for this Order.");
				foreach (var order in ordersToCheck)
				{
					order.RemoveRowWarning(permitRowWarningMessageForOrder);
				}

				var permitRequests = ftzOrderLines.Select(l => new { Request = new WhsPermitWithdrawRequest(l), Line = l }).ToArray();
				var permitRequestResponses = getPermitRequestResponses(permitRequests.Select(r => r.Request));

				var permitResponsesByNumber = permitRequestResponses.Responses.ToDictionary(r => r.Request.PermitTransactionRefNumber);
				var ordersWithWarnings = new HashSet<ZGuid>();
				var hasPermitError = false;
				foreach (var request in permitRequests)
				{
					var refNumber = request.Request.PermitTransactionRefNumber;
					if (permitResponsesByNumber.TryGetValue(refNumber, out var response))
					{
						var isSuccess = response.SuccessOrFailure == SuccessOrFailure.Success;
						hasPermitError = hasPermitError || !isSuccess;

						var orderLine = request.Line;
						orderLine.ApplyPermitResponse(response);

						if (isSuccess && shouldSetCustomsDataOnSuccess && response.OutwardEntryNumber.HasValue)
						{
							var customsData = orderLine.CustomsData;
							customsData.WB_EntryKey = response.OutwardEntryNumber.GetValueOrDefault();
							customsData.WB_EntryLineNo = ZShort.Zero;
						}

						// multiple lines may have a failed permit, but just add the warning to the Order once
						if (!isSuccess && ordersWithWarnings.Add(orderLine.WE_WD)) // will only be true if was not present in the hashset
						{
							var order = orderLine.Order;
							order.AddRowWarning(permitRowWarningMessageForOrder);
						}
					}
					else
					{
						throw new InvalidOperationException(FormattableString.Invariant($"Customs' {invalidErrorMessage} did not provide a response for every Permit Request."));
					}
				}

				if (hasPermitError)
				{
					result = ActionResult.Failure(ResString.GetMultilingualString("CB82CB2A-DC34-49DA-B784-985BA2F64966",
						"Not every Order Line on this Pick could be matched to a weekly estimate. Errors below:\r\n{0}",
						ftzOrderLines.SelectMany(o => o.RowWarnings).ToUniqueMessageListString()));
				}
			}

			return result;
		}
	}
}
