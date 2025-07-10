using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// A RateLine along with a RatingCriteria for fast calculation of properties specific to the criteria.
	/// Critieria may be null for certain calculations.
	/// It's the callers responsiblity not to call methods/properties that require a critieria.
	/// </summary>
	public class FastLine
	{
		public FastLine(IRateLine line, RatingCriteria criteria = null)
		{
			Line = line;
			Criteria = criteria;
			ParentRateEntry = line.ParentRateEntry;
			isCostRate = line.IsCostRate();
		}
		readonly bool isCostRate;

		public IRateLine Line { get; }
		public RatingCriteria Criteria { get; }
		public IRateEntry ParentRateEntry { get; }

		public bool IsCostRate() => isCostRate;
		public bool IsIntercompanyTariff() => Line.IsIntercompanyTariff();

		public Calculator Calculator => Line.Calculator;
		public AccChargeCode ChargeCode => Line.ChargeCode;

		public ZString TL_FeeChargeType => Line.TL_FeeChargeType;
		public BusinessObjectFactory Factory => Line.Factory;

		public ZString DisplayInfo() => Line.DisplayInfo();

		public List<OrgHeader> GetChargedOrgs(bool forCompanyTariff)
			=> chargedOrgs ??= Criteria.GetChargedOrgs(Line, forCompanyTariff);
		List<OrgHeader> chargedOrgs;

		public int GetFreightLeg()
			=> (freightLeg ?? (freightLeg = Criteria.GetFreightLeg(ParentRateEntry))).Value;
		int? freightLeg;

		/// <summary>
		/// A number which represents which org priority index in the registry got
		/// matched. Or IncotermsRanker.NotApplicable if none was found.
		/// </summary>
		public int GetOrgImportance() => GetIncotermsRanker().OrgImportance;

		public IncotermsRanker GetIncotermsRanker()
			=> incotermsRanker ?? (incotermsRanker = new IncotermsRanker().Rank(this));
		IncotermsRanker incotermsRanker;

		public bool? IsCollect()
		{
			if (!isCollectIsCalculated)
			{
				isCollectIsCalculated = true;
				isCollect = Criteria.Cache.IsCollect(IsCostRate(), Line.ChargeCode.AC_ChargeGroup);
			}
			return isCollect;
		}
		bool? isCollect;
		bool isCollectIsCalculated;
	}
}
