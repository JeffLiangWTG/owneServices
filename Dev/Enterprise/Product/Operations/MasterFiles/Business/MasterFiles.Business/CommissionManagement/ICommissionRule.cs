using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public interface ICommissionRule : ICommissionRuleRatesProvider
	{
		BusinessObjectFactory Factory { get; }

		ZGuid GroupPk { get; }
		GlbGroup Group { get; }
		ZGuid CompanyPk { get; }

		ZString Product { get; }
		ZString Service { get; }
		ZString SubModule { get; }

		ZString Mode { get; }
		ZString Origin { get; }
		ZString Destination { get; }

		ZDate StartDate { get; }
		ZDate EndDate { get; }
	}

	public interface ICommissionRuleRatesProvider
	{
		ZString CommissionBasis { get; set; }
		ZString CommissionTriggerType { get; set; }
		AccCommissionRuleRateCollection Rates { get; }
	}

	public interface ICommissionRateOverridable
	{
		BusinessObjectFactory Factory { get; }

		GlbCompany Company { get; }
		ZString CommissionType { get; set; }
		ZDecimal CommissionPercentage { get; set; }
		ZDecimal CommissionAmount { get; set; }
		ZString CommissionCurrency { get; set; }
		ZString CommissionPeriod { get; set; }
	}

	public static class CommissionRuleExtensions
	{
		public static bool HasOverlappingDateRange(this ICommissionRule commissionRule, ICommissionRule otherCommissionRule)
		{
			return ZDateTime.Overlaps(commissionRule.StartDate, commissionRule.EndDate, otherCommissionRule.StartDate, otherCommissionRule.EndDate);
		}
	}

	public static class CommissionRateOverridableExtensions
	{
		public static bool IsPercentageCommissionType(this ICommissionRateOverridable commissionRate)
		{
			return commissionRate.CommissionType == CommissionTypes.Codes.PCT;
		}

		public static bool IsAmountCommissionType(this ICommissionRateOverridable commissionRate)
		{
			return commissionRate.CommissionType == CommissionTypes.Codes.FIX;
		}

		public static bool IsCurrencyCommissionType(this ICommissionRateOverridable commissionRate)
		{
			return IsAmountCommissionType(commissionRate);
		}

		public static ZString GetCommissionPeriodDescription(this ICommissionRateOverridable commissionRate)
		{
			return CommissionLookups.New(commissionRate.Factory).CommissionPeriods.GetDescriptionFromCode(commissionRate.CommissionPeriod);
		}

		public static bool HasOverlappingCommissionPeriods(this ICommissionRateOverridable commissionRate, ICommissionRateOverridable otherCommissionRate)
		{
			var commissionPeriodList = OrganisationsDataRegistry.Instance.CommissionPeriodList.Value;
			var commissionPeriod = commissionPeriodList.FindByCode(commissionRate.CommissionPeriod);
			var otherCommissionPeriod = commissionPeriodList.FindByCode(otherCommissionRate.CommissionPeriod);
			if (commissionPeriod == null || otherCommissionPeriod == null)
			{
				return false;
			}

			var commissionPeriodStartsBeforeOtherCommissionPeriodEnds = commissionPeriod.Start < otherCommissionPeriod.End || otherCommissionPeriod.End == 0;
			var otherCommissionPeriodStartsBeforeCommissionPeriodEnds = otherCommissionPeriod.Start < commissionPeriod.End || commissionPeriod.End == 0;

			return commissionPeriodStartsBeforeOtherCommissionPeriodEnds && otherCommissionPeriodStartsBeforeCommissionPeriodEnds;
		}
	}
}
