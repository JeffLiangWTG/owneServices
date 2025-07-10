using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class WarehouseCustomsAddInfoWrapper
	{
		internal WarehouseCustomsAddInfoWrapper(Warehouse.Integration.IWarehouseCustomsAddInfo warehouseCustomsAddInfo)
		{
			var addInfos = AddInfoParser.CreateDictionaryWithAddInfoString(warehouseCustomsAddInfo.B7_AddInfoData);
			GatherData(addInfos, AddInfoKeys.InvoiceQuantity, ref invoiceQuantity);
			GatherData(addInfos, AddInfoKeys.InvoiceQuantityUnit, ref invoiceQuantityUnit, JobComInvoiceLine.Schema.JI_InvoiceUQMaxLength);
			GatherData(addInfos, AddInfoKeys.CustomsQuantity, ref customsQuantity);
			GatherData(addInfos, AddInfoKeys.CustomsQuantityUnit, ref customsQuantityUnit, JobComInvoiceLine.Schema.JI_CustomsUnitQtyMaxLength);
			GatherData(addInfos, AddInfoKeys.SecondCustomsQuantityUnit, ref secondCustomsQuantityUnit, AddInfoKeys.QuantityUnitMaxLength);
			GatherData(addInfos, AddInfoKeys.ThirdCustomsQuantityUnit, ref thirdCustomsQuantityUnit, AddInfoKeys.QuantityUnitMaxLength);
			GatherData(addInfos, AddInfoKeys.LinePrice, ref linePrice);
			GatherData(addInfos, AddInfoKeys.InvoiceQuantityUnit, ref invoiceQuantityUnit, JobComInvoiceLine.Schema.JI_InvoiceUQMaxLength);
			GatherData(addInfos, AddInfoKeys.LineNo, ref lineNo);
			GatherData(addInfos, AddInfoKeys.Tariff, ref tariff, JobComInvoiceLine.Schema.JI_TariffMaxLength);
			GatherData(addInfos, AddInfoKeys.SecondQty, ref secondQty);
			GatherData(addInfos, AddInfoKeys.ThirdQty, ref thirdQty);
			var parentProductLineNo = ZShort.Zero;
			GatherData(addInfos, AddInfoKeys.ParentProductLineNo, ref parentProductLineNo);
			isProductRelatedLine = parentProductLineNo > ZShort.Zero;
			addInfo = AddInfoParser.Serialise(addInfos);
		}

		void GatherData(Dictionary<ZString, ZString> addInfos, ZString addInfoKey, ref ZShort dataToSet)
		{
			ZString data;
			if (addInfos.TryGetValue(addInfoKey, out data))
			{
				dataToSet = ZShort.ParseSafe(data, ZShort.Zero);
				addInfos.Remove(addInfoKey);
			}
		}

		void GatherData(Dictionary<ZString, ZString> addInfos, ZString addInfoKey, ref ZDecimal dataToSet)
		{
			ZString data;
			if (addInfos.TryGetValue(addInfoKey, out data))
			{
				dataToSet = ZDecimal.ParseSafe(data, ZDecimal.Zero);
				addInfos.Remove(addInfoKey);
			}
		}

		void GatherData(Dictionary<ZString, ZString> addInfos, ZString addInfoKey, ref ZString dataToSet, int maxLength)
		{
			ZString data;
			if (addInfos.TryGetValue(addInfoKey, out data))
			{
				dataToSet = data.Left(maxLength);
				addInfos.Remove(addInfoKey);
			}
		}

		public ZBool IsProductRelatedLine
		{
			get { return isProductRelatedLine; }
		}
		readonly ZBool isProductRelatedLine;

		public ZDecimal InvoiceQuantity
		{
			get { return invoiceQuantity; }
		}
		readonly ZDecimal invoiceQuantity;

		public ZString InvoiceQuantityUnit
		{
			get { return invoiceQuantityUnit; }
		}
		readonly ZString invoiceQuantityUnit;

		public ZDecimal CustomsQuantity
		{
			get { return customsQuantity; }
		}
		readonly ZDecimal customsQuantity;

		public ZString CustomsQuantityUnit
		{
			get { return customsQuantityUnit; }
		}
		readonly ZString customsQuantityUnit;

		public ZString SecondCustomsQuantityUnit
		{
			get { return secondCustomsQuantityUnit; }
		}
		readonly ZString secondCustomsQuantityUnit;

		public ZString ThirdCustomsQuantityUnit
		{
			get { return thirdCustomsQuantityUnit; }
		}
		readonly ZString thirdCustomsQuantityUnit;

		public ZDecimal LinePrice
		{
			get { return linePrice; }
		}
		readonly ZDecimal linePrice;

		public ZShort LineNo
		{
			get { return lineNo; }
		}
		readonly ZShort lineNo;

		public ZString AddInfo
		{
			get { return addInfo; }
		}
		readonly ZString addInfo;

		public ZString Tariff
		{
			get { return tariff; }
		}
		readonly ZString tariff;

		public ZDecimal SecondQty
		{
			get { return secondQty; }
		}
		readonly ZDecimal secondQty;

		public ZDecimal ThirdQty
		{
			get { return thirdQty; }
		}
		readonly ZDecimal thirdQty;

		public static class AddInfoKeys
		{
			public const string ParentProductLineNo = "ParentProductLineNo";
			public const string Tariff = "Tariff";
			public const string InvoiceQuantity = "InvoiceQuantity";
			public const string InvoiceQuantityUnit = "InvoiceQuantityUnit";
			public const string CustomsQuantity = "CustomsQuantity";
			public const string CustomsQuantityUnit = "CustomsQuantityUnit";
			public const string LinePrice = "LinePrice";
			public const string LineNo = "LineNo";
			public const string SecondCustomsQuantityUnit = "SecondUQ";
			public const string ThirdCustomsQuantityUnit = "ThirdUQ";
			public const string SecondQty = "SecondQty";
			public const string ThirdQty = "ThirdQty";
			public const int QuantityUnitMaxLength = 3;
		}

		public static Dictionary<ZString, ZString> GetAddInfosDetails(JobComInvoiceLine invoiceLine, Dictionary<ZString, ZString> addInfos)
		{
			RemoveIrrelevantPGAIndicatorsFromInventory(invoiceLine, addInfos);
			return addInfos;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		public static Dictionary<ZString, ZString> GetAddInfosDetailsAndThirdQtyAndUQ(JobComInvoiceLine invoiceLine, Dictionary<ZString, ZString> addInfos, ref ZString thirdUQ, ref ZDecimal thirdQty)
		{
			RemoveIrrelevantPGAIndicatorsFromInventory(invoiceLine, addInfos);

			if (addInfos.ContainsKey(BondedWarehousingHelper.Constants.CustomsThirdQuantityUnit))
			{
				thirdUQ = addInfos[BondedWarehousingHelper.Constants.CustomsThirdQuantityUnit];
				addInfos.Remove(BondedWarehousingHelper.Constants.CustomsThirdQuantityUnit);
			}

			if (addInfos.ContainsKey(BondedWarehousingHelper.Constants.CustomsThirdQuantity))
			{
				thirdQty = ZDecimal.ParseSafe(addInfos[BondedWarehousingHelper.Constants.CustomsThirdQuantity], ZDecimal.Zero);
				addInfos.Remove(BondedWarehousingHelper.Constants.CustomsThirdQuantity);
			}

			return addInfos;
		}

		static void RemoveIrrelevantPGAIndicatorsFromInventory(JobComInvoiceLine invoiceLine, Dictionary<ZString, ZString> addInfoDict)
		{
			var indicatorCalculator = new PGAIndicatorsCalculator((pgaCode) => invoiceLine.IsPGAIndicatorAllowedToBeDefaulted(pgaCode));
			var clonedAddInfoDict = addInfoDict.ToArray();

			foreach (var clonedAddInfo in clonedAddInfoDict)
			{
				var shouldRemoveIndicator = indicatorCalculator.ShouldRemoveIrrelevantIndicator(clonedAddInfo.Key);
				if (shouldRemoveIndicator && addInfoDict.ContainsKey(clonedAddInfo.Key))
				{
					addInfoDict.Remove(clonedAddInfo.Key);
				}
			}
		}

		public static void SetupQty(ZDecimal ratio, ZPropertyInfo qtyInfo, ZString qtyUQ, ZString qtyUQFromInventory, ZDecimal qty)
		{
			if (qtyInfo.ReadOnly || qtyUQFromInventory.IsEmpty || qtyUQFromInventory != qtyUQ)
			{
				if (!qty.IsEmpty)
				{
					qtyInfo.Value = ZDecimal.Zero;
				}
			}
			else if (!qty.IsEmpty)
			{
				qtyInfo.Value = ((ZDecimal)(ratio * qty)).Round(5);
			}
		}
	}
}
