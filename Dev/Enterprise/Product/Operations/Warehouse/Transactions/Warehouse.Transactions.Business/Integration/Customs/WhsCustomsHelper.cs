using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class WhsCustomsHelper
	{
		public static bool CanFinaliseWhsOrderWithoutCustomsClearance(WhsOrder order)
		{
			return CheckCustomsForClearance(order?.Warehouse?.WarehouseAddress, Env.CurrentCompany?.Country?.Code, order?.Client, order?.ConsigneeDocAddress);
		}

		static bool CheckCustomsForClearance(OrgAddress warehouseAddress, string countryCode, OrgHeader client, JobDocAddress consignee)
		{
			// To use properties remove later when Customs link is added
			if (warehouseAddress == null || countryCode == null || client == null || consignee == null)
			{ }

			// Replace with Customs link later
			return WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.Value;
		}

		public static void SplitCustomsValuesAndQuantities(WhsDocketLine docketLine, decimal originalQty)
		{
			if (!docketLine.IsCustomsTransaction)
			{
				throw new InvalidOperationException("You cannot split customs values if the docket is not a customs transaction.");
			}

			if (originalQty == 0)
			{
				throw new InvalidOperationException("Original quantity cannot be 0.");
			}

			if (docketLine.WE_TransactionQuantity > originalQty)
			{
				throw new InvalidOperationException("New quantity cannot be larger than the original quantity after splitting.");
			}

			var customsData = docketLine.CustomsData;
			customsData.WB_BondedWhsQty = customsData.WB_BondedWhsQty * docketLine.WE_TransactionQuantity / originalQty;
			customsData.WB_CustomsQty = customsData.WB_CustomsQty * docketLine.WE_TransactionQuantity / originalQty;
			customsData.WB_TILV = customsData.WB_TILV * docketLine.WE_TransactionQuantity / originalQty;
			customsData.WB_ValueForDuty = customsData.WB_ValueForDuty * docketLine.WE_TransactionQuantity / originalQty;
			customsData.WB_CustomsSecondQuantity = customsData.WB_CustomsSecondQuantity * docketLine.WE_TransactionQuantity / originalQty;
			customsData.WB_CustomsThirdQuantity = customsData.WB_CustomsThirdQuantity * docketLine.WE_TransactionQuantity / originalQty;
		}

		public static ZString SafelyRemoveSuffixValue(ZString input)
		{
			ZString result = input;
			int charDashIndex = input.LastIndexOf("-", StringComparison.Ordinal);
			if (charDashIndex > 0)
			{
				result = input.SubstringSafe(0, charDashIndex);
			}
			return result;
		}
	}
}
