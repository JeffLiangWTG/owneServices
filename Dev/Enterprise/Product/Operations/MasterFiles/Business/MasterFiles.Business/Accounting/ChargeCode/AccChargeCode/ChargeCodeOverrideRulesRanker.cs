using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public interface IChargeCodeOverrideRulesRanker
	{
		ZString GetBestSellComplianceDescriptionOverride(BusinessObjectFactory factory, IBusinessObjectCollection sellComplianceDescriptionOverrideRules, ZString? jobType, ZString? transportMode, ZString supplyType);
	}

	class ChargeCodeOverrideRulesRanker : IChargeCodeOverrideRulesRanker
	{
		ZString IChargeCodeOverrideRulesRanker.GetBestSellComplianceDescriptionOverride(BusinessObjectFactory factory, IBusinessObjectCollection sellComplianceDescriptionOverrideRules, ZString? jobType, ZString? transportMode, ZString supplyType)
		{
			if (!jobType.HasValue || !transportMode.HasValue || supplyType.IsEmpty)
			{
				return ZString.Empty;
			}

			var ranker = new ColumnValueRanker();
			ranker.Add(AccChargeComplianceDescriptionSchema.ADE_JobType, jobType.Value, (ZString)new AllJobsConsumerType().Code);
			ranker.Add(AccChargeComplianceDescriptionSchema.ADE_TransportMode, transportMode.Value, (ZString)Core.Constants.FreightShipmentDirection.Code.All, ZString.Empty);
			ranker.Add(AccChargeComplianceDescriptionSchema.ADE_SupplyType, supplyType);
			var complianceDescription = ranker.GetBestMatch<AccChargeComplianceDescription>(factory, sellComplianceDescriptionOverrideRules.CompleteFilter);

			return complianceDescription?.ADE_Description ?? ZString.Empty;
		}
	}
}
