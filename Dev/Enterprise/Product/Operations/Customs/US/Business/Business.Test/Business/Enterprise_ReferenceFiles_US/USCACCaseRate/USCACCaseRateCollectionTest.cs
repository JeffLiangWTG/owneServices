using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCACCaseRateCollection))]
	sealed class USCACCaseRateCollectionTest : ActiveBusinessObjectCollectionTestCase<USCACCaseRateCollection>
	{
		public void TestGetRateWithMultipleRatesWithTheSameEffectiveDate()
		{
			var aCase = Factory.New<USCACCase>();
			aCase.U5_CaseNumber = "A520804002";

			var rate1 = aCase.CaseRates.AddNew();
			rate1.U6_AddedDate = new ZDateTime(2011, 4, 11);
			rate1.U6_EffectiveDate = new ZDateTime(2011, 3, 11);
			rate1.U6_AdValoremRate = 0.1923m;

			var rate2 = aCase.CaseRates.AddNew();
			rate2.U6_AddedDate = new ZDateTime(2012, 4, 11);
			rate2.U6_EffectiveDate = new ZDateTime(2012, 3, 23);
			rate2.U6_AdValoremRate = 0.028m;

			var rate3 = aCase.CaseRates.AddNew();
			rate3.U6_AddedDate = new ZDateTime(2012, 5, 10);
			rate3.U6_EffectiveDate = new ZDateTime(2012, 3, 23);
			rate3.U6_AdValoremRate = 0.0251m;

			AssertEquals(rate3, aCase.CaseRates.GetDepositRate(new ZDate(2012, 3, 23)));
			AssertEquals(rate1, aCase.CaseRates.GetDepositRate(new ZDate(2011, 3, 11)));
		}

		public void TestGetDepositRate()
		{
			var parentCase = ParentCase;
			var caseRate0 = parentCase.CaseRates.AddNew();
			caseRate0.U6_EffectiveDate = new ZDateTime(2007, 1, 1);

			var caseRate1 = parentCase.CaseRates.AddNew();
			caseRate1.U6_EffectiveDate = new ZDateTime(2009, 1, 1);

			var caseRate2 = parentCase.CaseRates.AddNew();
			caseRate2.U6_EffectiveDate = new ZDateTime(2009, 3, 1);
			caseRate2.U6_InactivatedDate = new ZDateTime(2009, 3, 31);

			var caseRate3 = parentCase.CaseRates.AddNew();
			caseRate3.U6_EffectiveDate = new ZDateTime(2008, 1, 1);

			AssertEquals(caseRate1, parentCase.CaseRates.GetDepositRate(ZDate.Today));
		}

		protected override USCACCaseRateCollection GetCollectionToTest()
		{
			return new USCACCaseRateCollection(ParentCase);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = (USCACCaseRate)base.GetNewElementToAddToTheCollection();
			result.U6_CaseNumber = "A1";
			return result;
		}

		USCACCase ParentCase
		{
			get
			{
				if (parentCase == null)
				{
					parentCase = Factory.New<USCACCase>();
					parentCase.U5_CaseNumber = "A1";
				}
				return parentCase;
			}
		}
		USCACCase parentCase;
	}
}
