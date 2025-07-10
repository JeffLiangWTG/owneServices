using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// This collection contains all the related tariff rules for a tariff number
	/// </summary>
	class USCTariffRuleAdhocCollection : ActiveBusinessObjectCollection<USCTariffRule>
	{
		public USCTariffRuleAdhocCollection(BusinessObjectFactory factory)
			: base(factory, new AdhocCollectionRelationship(typeof(USCTariffRule)))
		{
		}

		public bool Applies(ZString ruleCode, ZString tariffNumber, ZDateTime effectiveDate)
		{
			return GetTariffRuleIfApplies(ruleCode, tariffNumber, effectiveDate) != null;
		}

		public USCTariffRule GetTariffRuleIfApplies(ZString ruleCode, ZString tariffNumber, ZDateTime effectiveDate)
		{
			foreach (var tariffRule in this)
			{
				if (tariffRule.Applies(ruleCode, tariffNumber, effectiveDate))
				{
					return tariffRule;
				}
			}
			return null;
		}

		public void Load(ZString tariffNumber)
		{
			((IList)this).Clear();

			var query = USCTariffRule.Loader.GetTariffRange(tariffNumber);

			AddRange(Factory.Load<USCTariffRule>(query));
		}
	}
}
