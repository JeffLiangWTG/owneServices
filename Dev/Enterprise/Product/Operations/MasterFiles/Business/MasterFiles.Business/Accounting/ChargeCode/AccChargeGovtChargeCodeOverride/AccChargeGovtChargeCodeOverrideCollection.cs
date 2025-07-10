using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccChargeGovtChargeCodeOverrideCollection : DependentBusinessObjectCollection<AccChargeGovtChargeCodeOverride, AccChargeCode>
	{
		public AccChargeGovtChargeCodeOverrideCollection(AccChargeCode chargeCode) : base(chargeCode)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var config = (AccChargeGovtChargeCodeOverride)child;
			config.ACG_AC = Master.PK;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(AccChargeGovtChargeCodeOverrideSchema.ACG_AC, Master.PK);
			return query;
		}

		public AccChargeGovtChargeCodeOverride GetGovtChargeCode(BusinessObjectFactory factory, ZGuid chargeCodePK, ConfigurationMatcherHelper.ConfigurationMatcherParameters parameters)
		{
			parameters.Direction = ChargeCode.RecalculateDirectionBasedOnOriginAndDestination(parameters);
			var directionValue = GetDirectionCode(parameters.Direction);
			var costSellValue = GetCostSell(parameters.CostOrSell);
			var ranker = new ColumnValueRanker();
			ranker.Add(AccChargeGovtChargeCodeOverrideSchema.ACG_CostSellAll, costSellValue, new ZString(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All));
			ranker.Add(AccChargeGovtChargeCodeOverrideSchema.ACG_JobType, parameters.JobType, new ZString(JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All));
			ranker.Add(AccChargeGovtChargeCodeOverrideSchema.ACG_TransportMode, parameters.TransportMode, new ZString(JobConfigurationSelectorLookups.ModeAdditionalCodes.All));
			ranker.Add(AccChargeGovtChargeCodeOverrideSchema.ACG_Direction, ConfigurationMatcherHelper.GetDirectionFallBackCodes(directionValue, parameters));
			return ranker.GetBestMatch<AccChargeGovtChargeCodeOverride>(factory, CreateRelationshipFilter());
		}

		public ZString GetCostSell(CostSell costSellEnum)
		{
			switch (costSellEnum)
			{
				case CostSell.Cost:
					return AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.Cost;
				case CostSell.Revenue:
					return AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.Revenue;
				default:
					return AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All;
			}
		}

		public ZString GetDirectionCode(Directions? directionEnum)
		{
			if (directionEnum == null || directionEnum == Directions.Unknown)
			{
				return ZString.Empty;
			}

			switch (directionEnum)
			{
				case Directions.Import:
					return Core.Constants.FreightShipmentDirection.Code.Import;
				case Directions.Export:
					return Core.Constants.FreightShipmentDirection.Code.Export;
				case Directions.Domestic:
					return Core.Constants.FreightShipmentDirection.Code.Domestic;
				default:
					return Core.Constants.FreightShipmentDirection.Code.Other;
			}
		}

		AccChargeCode ChargeCode
		{
			get { return Master; }
		}
	}
}
