using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region UnloadPalletInfo

		[WebMethod(Description = "Unload Pallet Inventory")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		[SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "http://stackoverflow.com/questions/41980285/exception-filter-causes-ca2000-despite-a-using-statement")]
		[SuppressMessage("Microsoft.Usage", "CA2202:DoNotDisposeObjectsMultipleTimes", Justification = "http://stackoverflow.com/questions/35203767/exception-filter-triggers-ca2202")]
		public WhsInventoryWebServiceResponse UnloadPalletInfo(Guid receivePK, Guid dockDoorLocationPK, string palletID)
		{
			return HandleWebServiceRequest<WhsInventoryWebServiceResponse>(result => UnloadPalletInfoCore(result, receivePK, dockDoorLocationPK, palletID));
		}

		void UnloadPalletInfoCore(WhsInventoryWebServiceResponse response, Guid receivePK, Guid dockDoorLocationPK, string palletID)
		{
			var receive = WhsGroupedUnloadHelper.LoadValidNotFinalizedReceive(response, Factory, receivePK);
			if (response.Error == ErrorTypes.None)
			{
				WhsGroupedUnloadHelper.CheckASNLinesPalletInfo(response, receive, palletID, isPerformingUnload: true);
			}

			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			if (warehouse == null)
			{
				response.LogBusinessValidationError(Res.GetString("73D3FFA7-9956-471A-96B5-ECC557D0112D", "Warehouse should not be null."));
			}
			else if (response.Error == ErrorTypes.None)
			{
				var messageHCCs = new HashSet<string>();
				foreach (var line in receive.AsnLines.Cast<WhsAsnLine>().Where(al => al.WN_PalletId.EqualsIgnoringCase(palletID)))
				{
					CreateOrUpdateReceiveLines(response, warehouse, messageHCCs, dockDoorLocationPK, receive, line);
					if (!response.NoError())
					{
						break;
					}
				}

				if (messageHCCs.Any())
				{
					response.Error = ErrorTypes.Information;
					response.ErrorMessage = Res.GetString("4c0487b5-07ee-4eac-a575-0d7b8f8b5a26", "This pallet contains the following Hold Code(s): '{0}'.", string.Join(", ", messageHCCs));
				}

				if (response.NoError())
				{
					receive.RunPreSaveValidation();
					InvalidReceiveDataForTest(receive);
				}

				if (receive.HasErrors)
				{
					response.Error = ErrorTypes.BusinessValidationError;
					response.ErrorMessage = GetReceiveErrors(receive);
				}
				else
				{
					WebServiceHelper.SaveFactoryWithExceptionHandling(
						receive.Factory,
						response,
						(concurrencyException) => Res.GetString("fbf74ff5-66e0-4872-aac4-d64b55ae56ed", "Another user has made changes to the unloading job while you have been working on it. Please restart the operation and try again."));
				}
			}
		}

		string GetReceiveErrors(WhsReceive receive)
		{
			var errorMessageBuilder = new StringBuilder();
			errorMessageBuilder.AppendLine(Res.GetString("28709ad6-bb19-4a02-97fd-16a02ec2b0b0", "Errors occurred when trying to unload full pallet, please correct or use Product Unload instead."));

			var errors = new List<string>();
			var receiveNotifications = receive.NotificationsIncludingChildren;

			if (receive.HasErrors)
			{
				errors.AddRange(receiveNotifications.GetErrors().GetUniqueMessageList());
			}

			if (receive.HasMessageErrors)
			{
				errors.AddRange(receiveNotifications.GetMessageErrors().GetUniqueMessageList());
			}

			if (errors.Count > MaxErrorCount)
			{
				errorMessageBuilder.AppendLine(Res.GetString("0e5663de-9be5-44d8-84b7-11fcaacacd58", "Displaying the first 10 of {0} errors.", errors.Count));
				errorMessageBuilder.AppendLine(string.Empty);
				errors.Take(MaxErrorCount).ForEach(e => errorMessageBuilder.AppendLine(e));
			}
			else
			{
				errorMessageBuilder.AppendLine(string.Empty);
				errors.ForEach(e => errorMessageBuilder.AppendLine(e));
			}

			return errorMessageBuilder.ToString().Trim();
		}

		void CreateOrUpdateReceiveLines(WhsInventoryWebServiceResponse response, WhsWarehouse warehouse, HashSet<string> hccsForMessage, Guid dockDoorLocationPK, WhsReceive receive, WhsAsnLine asnLine)
		{
			var matchedLineHCCs = receive.Lines.Where(
					dl => dl.WE_PalletID.EqualsIgnoringCase(asnLine.WN_PalletId)
					&& dl.WE_OP == asnLine.WN_OP
					&& dl.ProductUQ == asnLine.WN_QuantityUQ
					&& dl.WE_PartAttrib1.EqualsIgnoringCase(asnLine.WN_PartAttrib1)
					&& dl.WE_PartAttrib2.EqualsIgnoringCase(asnLine.WN_PartAttrib2)
					&& dl.WE_PartAttrib3.EqualsIgnoringCase(asnLine.WN_PartAttrib3)
					&& dl.WE_LineNo == asnLine.WN_LineNo
					&& dl.WE_SubLineNo == asnLine.WN_SubLineNo
					&& dl.WE_ExpiryDate == asnLine.WN_ExpiryDate
					&& dl.WE_PackingDate == asnLine.WN_PackingDate)
				.GroupBy(l => l.WE_WHC_NKCurrentInventoryHeldCode);

			var currentQuantity = asnLine.WN_Quantity;
			foreach (var linesWithSameHCC in matchedLineHCCs)
			{
				var linesQtySum = linesWithSameHCC.Sum(l => l.WE_ClientOrderedUnits);
				var qty = Math.Min(currentQuantity, linesQtySum);
				var hcc = linesWithSameHCC.Key;
				CreateOrUpdateWhsReceiveLine(response, warehouse, dockDoorLocationPK, receive, asnLine, qty, hcc);

				if (!response.NoError())
				{
					break;
				}
				currentQuantity -= qty;
				if (!string.IsNullOrEmpty(hcc))
				{
					hccsForMessage.Add(hcc);
				}

				if (currentQuantity == 0)
				{
					break;
				}
			}

			if (response.NoError() && currentQuantity > 0)
			{
				CreateOrUpdateWhsReceiveLine(response, warehouse, dockDoorLocationPK, receive, asnLine, currentQuantity, string.Empty);
			}
		}

		void CreateOrUpdateWhsReceiveLine(WhsInventoryWebServiceResponse response, WhsWarehouse warehouse, Guid dockDoorLocationPK, WhsReceive receive, WhsAsnLine asnLine, decimal qty, string hcc)
		{
			var receiveLine = CreateWhsReceiveLine(warehouse, receive, asnLine.SupplierPart.OP_PartNum, qty, asnLine.WN_QuantityUQ,
				asnLine.WN_PartAttrib1.ToUpper(), asnLine.WN_PartAttrib2.ToUpper(), asnLine.WN_PartAttrib3.ToUpper(), asnLine.WN_SerialNumber.ToUpper(),
				asnLine.WN_ExpiryDate.IsEmpty ? new DateTime() : asnLine.WN_ExpiryDate.ToDateTime(),
				asnLine.WN_PackingDate.IsEmpty ? new DateTime() : asnLine.WN_PackingDate.ToDateTime(),
				hcc, asnLine.WN_PalletId.ToUpper(), asnLine.WN_LineNo, asnLine.WN_SubLineNo, string.Empty, dockDoorLocationPK,
				productsWhichMayFulfillAsnLinesWithStockUnit: Array.Empty<Guid>(),
				isEmptyAsnPalletIdMatchingEnabled: false, asnLine.SupplierPart, null, out var validationError);

			if (string.IsNullOrEmpty(validationError))
			{
				response.InventoryLinePK = receiveLine.PK.ToGuid();
			}
			else
			{
				response.LogBusinessValidationError(validationError);
			}
		}

		partial void InvalidReceiveDataForTest(WhsReceive receive);

		const int MaxErrorCount = 10;

		#endregion
	}
#if DEBUG
	public partial class WhsSecureService
	{
		partial void InvalidReceiveDataForTest(WhsReceive receive)
		{
			CreateInvalidReceiveDataForTest?.Invoke(receive);
		}
		public Action<WhsReceive> CreateInvalidReceiveDataForTest;
	}
#endif
}
