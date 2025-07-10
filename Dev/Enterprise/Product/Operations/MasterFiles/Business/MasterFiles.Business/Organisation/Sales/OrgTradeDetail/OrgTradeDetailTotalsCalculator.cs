using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.Business
{
	public class OrgTradeDetailTotalsCalculator
	{
		#region TotalAnnualCount

		public ZDecimal GetTotalAnnualCount(IEnumerable<OrgTradeDetail> tradeDetails)
		{
			return tradeDetails.Sum(x => x.PA_Calc_AnnualCount);
		}

		#endregion

		#region TotalAnnualWeight

		public ZDecimal GetTotalAnnualWeight(IEnumerable<OrgTradeDetail> tradeDetails)
		{
			var targetUnit = GetTotalAnnualWeightUQ(tradeDetails);
			ZDecimal total = 0;

			foreach (var tradeDetail in tradeDetails)
			{
				total += Constants.Weight.ConvertSafe(tradeDetail.PA_Calc_AnnualWeight, tradeDetail.CurrentProspectPeriod.PAS_WeightUQ, targetUnit);
			}

			return total;
		}

		public ZString GetTotalAnnualWeightUQ(IEnumerable<OrgTradeDetail> tradeDetails)
		{
			var useMetricUnit = !tradeDetails.Any() || tradeDetails.Any(x => x.CurrentProspectPeriod.WeightUQIsMetric);
			return useMetricUnit ? Constants.Weight.Kilograms : Constants.Weight.Pounds;
		}

		#endregion

		#region TotalAnnualVolume

		public ZDecimal GetTotalAnnualVolume(IEnumerable<OrgTradeDetail> tradeDetails)
		{
			var targetUnit = GetTotalAnnualVolumeUQ(tradeDetails);
			ZDecimal total = 0;

			foreach (var tradeDetail in tradeDetails)
			{
				total += Constants.Volume.ConvertSafe(tradeDetail.PA_Calc_AnnualVolume, tradeDetail.CurrentProspectPeriod.PAS_VolumeUQ, targetUnit);
			}

			return total;
		}

		public ZString GetTotalAnnualVolumeUQ(IEnumerable<OrgTradeDetail> tradeDetails)
		{
			var useMetricUnit = !tradeDetails.Any() || tradeDetails.Any(x => x.CurrentProspectPeriod.VolumeUQIsMetric);
			return useMetricUnit ? Constants.Volume.CubicMetres : Constants.Volume.CubicFeet;
		}

		#endregion

		#region TotalAnnualMetricVolume

		public ZDecimal GetTotalAnnualMetricVolume(IEnumerable<OrgTradeDetail> tradeDetails)
		{
			var targetUnit = GetTotalAnnualMetricVolumeUQ();
			ZDecimal total = 0;

			foreach (var tradeDetail in tradeDetails)
			{
				total += Constants.Volume.ConvertSafe(tradeDetail.PA_Calc_AnnualVolume, tradeDetail.CurrentProspectPeriod.PAS_VolumeUQ, targetUnit);
			}

			return total;
		}

		public ZString GetTotalAnnualMetricVolumeUQ()
		{
			return Constants.Volume.CubicMetres;
		}

		#endregion

		#region TotalAnnualChargeable

		public ZDecimal GetTotalAnnualChargeable(bool isDomestic, IEnumerable<OrgTradeDetail> tradeDetails)
		{
			var unit = GetTotalAnnualChargeableUQ(tradeDetails);
			ZDecimal total = 0;

			foreach (var tradeDetail in tradeDetails)
			{
				var chargeableUQ = tradeDetail.CurrentProspectPeriod.PAS_Calc_ChargeableUQ;
				if (Constants.Weight.ContainsCode(chargeableUQ))
				{
					total += Constants.Weight.Convert(tradeDetail.PA_Calc_AnnualChargeable, chargeableUQ, unit);
				}
				else if (Constants.Volume.ContainsCode(chargeableUQ))
				{
					var chargeableFactor = ChargeableFactor.GetDefault(isDomestic ? ChargeableFactorSource.Domestic : ChargeableFactorSource.International, tradeDetail.PA_TradeMode);
					if (chargeableFactor == null)
					{
						total += tradeDetail.PA_Calc_AnnualChargeable;
					}
					else
					{
						var chargeableUQIsMetric = tradeDetail.ChargeableUQIsMetric;
						var equation = chargeableUQIsMetric ? chargeableFactor.MetricFactor : chargeableFactor.ImperialFactor;

						total += equation.Convert(new ZVolume(tradeDetail.PA_Calc_AnnualChargeable, chargeableUQ)).Amount;
					}
				}
				else
				{
					total += tradeDetail.PA_Calc_AnnualChargeable;
				}
			}

			return total;
		}

		public ZString GetTotalAnnualChargeableUQ(IEnumerable<OrgTradeDetail> tradeDetails)
		{
			var useMetricUnit = !tradeDetails.Any() || tradeDetails.Any(x => x.ChargeableUQIsMetric);
			return useMetricUnit ? Constants.Weight.Kilograms : Constants.Weight.Pounds;
		}

		#endregion

		#region TotalAnnualTEU

		public ZDecimal GetTotalAnnualTEU(IEnumerable<OrgTradeDetail> tradeDetails)
		{
			return tradeDetails.Sum(x => x.PA_Calc_AnnualTEU);
		}

		#endregion

		#region TotalAnnualPalletCount

		public ZDecimal GetTotalAnnualPalletCount(IEnumerable<OrgTradeDetail> tradeDetails)
		{
			return tradeDetails.Sum(x => x.PA_Calc_AnnualPalletCount);
		}

		#endregion
	}
}
