using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public static class CustomsEntryInfoExtensions
	{
		public static ZString GetFormattedCustomsEntryKeyWithLineNo(this CustomsEntryInfo customsData)
		{
			ZString result = "";

			if (customsData != null)
			{
				result = WhsBondedWarehouseAttribute.BuildKey(customsData.EntryKey.GetValueOrDefault(), customsData.EntryLineNumber.GetValueOrDefault());
			}

			return result;
		}

		public static ZString GetFormattedInwardsCustomsEntryKeyWithLineNoForOrderLine(this CustomsEntryInfo customsData)
		{
			ZString result = "";

			if (customsData != null)
			{
				result = WhsBondedWarehouseAttribute.BuildKey(customsData.InwardsEntryKey.GetValueOrDefault(), customsData.InwardsEntryLineNumber.GetValueOrDefault());
			}

			return result;
		}
	}
}