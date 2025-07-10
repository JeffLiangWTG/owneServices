using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NonDependentNZCConcessionCollection : BusinessObjectCollection<NZCConcession>
	{
		public NonDependentNZCConcessionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public NonDependentNZCConcessionCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public void DefaultModuleFilterFields(ZString tariffNumber)
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Tariff Heading", "Property", tariffNumber.Left(2)));
		}

		public static ZQuery ConcessionLinkQuery(ZString tariffNumber)
		{
			ZDBOnlySubQuery linkSubQuery = new ZDBOnlySubQuery(typeof(NZCConcessionClassificationLink), NZCConcessionClassificationLinkSchema.U3_U2_Concession);
			linkSubQuery.AddToFilter(JoinCondition.Or, NZCConcessionClassificationLinkSchema.U3_TariffPortion, SQLComparisonOperator.Equal, tariffNumber.Left(2));
			linkSubQuery.AddToFilter(JoinCondition.Or, NZCConcessionClassificationLinkSchema.U3_TariffPortion, SQLComparisonOperator.Equal, tariffNumber.Left(4));
			linkSubQuery.AddToFilter(JoinCondition.Or, NZCConcessionClassificationLinkSchema.U3_TariffPortion, SQLComparisonOperator.Equal, tariffNumber.Left(7));
			linkSubQuery.AddToFilter(JoinCondition.Or, NZCConcessionClassificationLinkSchema.U3_TariffPortion, SQLComparisonOperator.Equal, tariffNumber.Left(10));

			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(NZCConcession));
			result.AddSubQuery(linkSubQuery, JoinCondition.And);
			return result;
		}
	}
}
