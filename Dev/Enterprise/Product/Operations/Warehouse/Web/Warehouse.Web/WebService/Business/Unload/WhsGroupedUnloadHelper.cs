using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	public static class WhsGroupedUnloadHelper
	{
		#region LoadValidNotFinalizedReceive

		public static WhsReceive LoadValidNotFinalizedReceive(WebServiceResponse response, BusinessObjectFactory factory, Guid receivePK)
		{
			var receive = factory.Load<WhsReceive>(new ZGuid(receivePK));
			if (receive == null)
			{
				response.LogBusinessValidationError(Res.GetString("b0cac925-ae8b-474b-af8c-83c8dc79df5e", "Receive record could not be found."));
			}
			else if (receive.IsFinalised)
			{
				response.LogBusinessValidationError(Res.GetString("fe934266-ff11-4143-b409-c049e64dadc6", "Receive record has been finalized."));
			}
			return receive;
		}

		#endregion

		#region CheckASNLinesPalletInfo

		public static void CheckASNLinesPalletInfo(WebServiceResponse response, WhsReceive receive, string palletID, bool isPerformingUnload)
		{
			var validAsnLines = receive.AsnLines.Cast<WhsAsnLine>().Where(al => al.WN_PalletId.EqualsIgnoringCase(palletID)).ToArray();
			if (validAsnLines.Length == 0)
			{
				response.ErrorMessage = Res.GetString("7fd52422-5f79-4d03-a496-29dcc541d527", "Pallet Id '{0}' not expected for this receive. Use Product Unload to receive it.", palletID);
				response.Error = ErrorTypes.BusinessValidationError;
			}
			else if (response.Error != ErrorTypes.BusinessValidationError && receive.Inventory.Cast<WhsInventoryView>().Any(i => i.WI_PalletID.ToUpper() == palletID && i.WI_ExpectedReceiptQuantity <= i.WI_InDocketLineUnits))
			{
				if (isPerformingUnload)
				{
					response.ErrorMessage = Res.GetString("fc5dff7c-d9ee-4d05-aa0c-c3ae4a2fbdaf", "Pallet Id '{0}' was already unloaded.", palletID);
					response.Error = ErrorTypes.BusinessValidationError;
				}
				else
				{
					response.ErrorMessage = Res.GetString("aea65570-684d-4ebc-8151-099792454df8", "Pallet Id '{0}' was already unloaded. Do you want to check it?", palletID);
					response.Error = ErrorTypes.YesNoEnquiry;
				}
			}
			else
			{
				var factory = receive.Factory;
				var client = receive.Client;
				var partAttributeManager = client.PartAttributeManager;
				var isMandatoryAttribute1 = partAttributeManager.IsPartAttributeMandatory(1);
				var isMandatoryAttribute2 = partAttributeManager.IsPartAttributeMandatory(2);
				var isMandatoryAttribute3 = partAttributeManager.IsPartAttributeMandatory(3);
				var isExpiryDateUsed = partAttributeManager.IsExpiryDateUsedByOrganisation;
				var isPackingDateUsed = partAttributeManager.IsPackingDateUsedByOrganisation;

				var preventedProducts = client.MiscServ.OM_WhsCheckPartWeightOrDimsOnReceive != ProductReceiveWeightOrDimsCheckTypeList.Codes.DoNotCheckWeightOrDims
					? validAsnLines.DistinctBy(x => x.WN_OP).Where(x => x.SupplierPart.ShouldPreventReceiveOfPartWithNoWeightOrDims(client)).Select(x => x.SupplierPart.OP_PartNum).ToArray()
					: Array.Empty<ZString>();

				if (preventedProducts.Length > 0)
				{
					var allProducts = string.Join("\r\n", preventedProducts);

					response.ErrorMessage = Res.GetString("d1da01f7-5481-443f-b1de-537428d94fd6", "The following product(s) on this Pallet Id '{0}' cannot be received as the Product Master is missing weight or dimensions: \r\n{1}", palletID, allProducts);
					response.Error = ErrorTypes.BusinessValidationError;
				}
				else
				{
					foreach (var asnLine in validAsnLines)
					{
						var product = WhsProduct.GetWhsProduct(factory, asnLine.WN_OP);

						var hasInvalidSerialNumber = CheckSerialNumberValid(client, asnLine, product);
						if (hasInvalidSerialNumber)
						{
							response.ErrorMessage = Res.GetString("9471025e-326d-495a-bfff-bb390f304db4", "Product on this Pallet Id '{0}' contains serial numbers which must be entered. Use Product Unload mode to unload it.", palletID);
							response.Error = ErrorTypes.PalletAsnLineMissingMandatoryAttributes;
							break;
						}

						var hasInvalidMandatoryAttr = CheckMandatoryAttributeValid(client, asnLine, product, isMandatoryAttribute1, isMandatoryAttribute2, isMandatoryAttribute3, isExpiryDateUsed, isPackingDateUsed);
						if (hasInvalidMandatoryAttr)
						{
							response.ErrorMessage = Res.GetString("fcefb8c0-4c4b-4968-af22-b9cbbc8d2d3d", "Product on this Pallet Id '{0}' contains mandatory attribute which must be entered. Use Product Unload mode to unload it.", palletID);
							response.Error = ErrorTypes.PalletAsnLineMissingMandatoryAttributes;
							break;
						}
					}
				}
			}
		}

		static bool CheckSerialNumberValid(OrgHeader client, WhsAsnLine asnLine, WhsProduct product)
		{
			var hasInvalidSerialNumber = false;
			if (product.IsSerialNumberUsedAndNotReleaseCaptured(client))
			{
				hasInvalidSerialNumber = string.IsNullOrEmpty(asnLine.WN_SerialNumber) || (!string.IsNullOrEmpty(asnLine.WN_SerialNumber) && asnLine.WN_Quantity > 1);
			}

			return hasInvalidSerialNumber;
		}

		static bool CheckMandatoryAttributeValid(OrgHeader client, WhsAsnLine asnLine, WhsProduct product, bool isMandatoryAttribute1, bool isMandatoryAttribute2, bool isMandatoryAttribute3,
			bool isExpiryDateUsed, bool isPackingDateUsed)
		{
			var hasInvalidMandatoryAttr = false;
			if (isMandatoryAttribute1 && product.IsPartAttributeUsed(client, 1) && !product.IsPartAttribReleaseCaptured(client, 1))
			{
				hasInvalidMandatoryAttr = string.IsNullOrEmpty(asnLine.WN_PartAttrib1);
			}
			if (!hasInvalidMandatoryAttr && isMandatoryAttribute2 && product.IsPartAttributeUsed(client, 2) && !product.IsPartAttribReleaseCaptured(client, 2))
			{
				hasInvalidMandatoryAttr = string.IsNullOrEmpty(asnLine.WN_PartAttrib2);
			}
			if (!hasInvalidMandatoryAttr && isMandatoryAttribute3 && product.IsPartAttributeUsed(client, 3) && !product.IsPartAttribReleaseCaptured(client, 3))
			{
				hasInvalidMandatoryAttr = string.IsNullOrEmpty(asnLine.WN_PartAttrib3);
			}
			if (!hasInvalidMandatoryAttr && isExpiryDateUsed && product.IsExpiryDateUsed(client))
			{
				hasInvalidMandatoryAttr = !asnLine.WN_ExpiryDate.IsValid && asnLine.WN_ExpiryDate.IsEmpty;
			}
			if (!hasInvalidMandatoryAttr && isPackingDateUsed && product.IsPackingDateUsed(client))
			{
				hasInvalidMandatoryAttr = !asnLine.WN_PackingDate.IsValid && asnLine.WN_PackingDate.IsEmpty;
			}

			return hasInvalidMandatoryAttr;
		}

		#endregion
	}
}
