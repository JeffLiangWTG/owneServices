using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradeLineForTest : TradeLine
	{
		public TradeLineForTest() : base(null, null) { }
		public TradeLineForTest(
			ZString mode, ZString type, ZGuid supplierPartPk, ZByte status,
			ZDate period,
			ZGuid mainOrg, ZGuid org2, ZGuid org3,
			ZDateTime periodLastTrade, int jobCount, int palletCount, int lineCount, decimal weightVolume, decimal weightAmount, decimal volumeM3, decimal chargeableAmount, string chargeableUnits, decimal teu,
			string currency, ZGuid chargeCompanyPk,
			decimal jobRevenue, decimal jobCost, decimal mainOrgRevenue, decimal mainOrgCost,
			ZGuid relatedRateEntryPk, OrgSalesProduct product = null, ZGuid? originPK = null, ZGuid? destinationPK = null) : base(null, new Dictionary<ZString, OrgSalesProduct>())
		{
			MainOrg = mainOrg;
			Buyer = org2;
			Supplier = org3;

			StatusCode = ConvertTradeStatusToCode(status);
			RelatedRateEntryPk = relatedRateEntryPk;

			TradeMode = mode;
			TradeType = type;
			SupplierPartPk = supplierPartPk;

			PeriodStart = period;

			Currency = currency;
			ChargeCompanyPk = chargeCompanyPk;

			JobRevenue = jobRevenue;
			JobCost = jobCost;
			MainOrgRevenue = mainOrgRevenue;
			MainOrgCost = mainOrgCost;

			PeriodLastTrade = periodLastTrade;
			JobCount = jobCount;
			PalletCount = palletCount;
			LineCount = lineCount;
			WeightVolume = weightVolume;
			WeightAmount = weightAmount;
			VolumeM3 = volumeM3;
			ChargeableAmount = chargeableAmount;
			ChargeableUnits = chargeableUnits;
			TEU = teu;

			if (product != null)
			{
				ProductCode = product.MP_Code;
				products.Add(ProductCode, product);
			}

			if (originPK != null)
			{
				OriginPk = originPK.Value;
			}

			if (destinationPK != null)
			{
				DestinationPk = destinationPK.Value;
			}
		}
	}
}
