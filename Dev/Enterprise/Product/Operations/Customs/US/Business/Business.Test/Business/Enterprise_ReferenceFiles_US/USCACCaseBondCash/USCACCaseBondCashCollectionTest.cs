using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCACCaseBondCashCollection))]
	sealed class USCACCaseBondCashCollectionTest : ActiveBusinessObjectCollectionTestCase<USCACCaseBondCashCollection>
	{
		public void TestIsCashRequired()
		{
			var collection = GetCollectionToTest();
			Assert(!collection.IsCashRequired(ZDate.BrettsBirthday));

			var element = collection.AddNew();
			element.U8_EffectiveDate = ZDateTime.BrettsBirthday;
			element.U8_Indicator = BondCashIndicatorList.Codes.Cash;
			Assert(collection.IsCashRequired(ZDate.BrettsBirthday));

			var element2 = collection.AddNew();
			element2.U8_EffectiveDate = ZDateTime.BrettsBirthday.AddDays(2);
			element2.U8_Indicator = BondCashIndicatorList.Codes.BOC;
			Assert(!collection.IsCashRequired(ZDate.BrettsBirthday.AddDays(2)));

			element2.U8_InactivatedDate = ZDateTime.BrettsBirthday.AddDays(10);
			Assert(collection.IsCashRequired(ZDate.BrettsBirthday));
		}

		protected override USCACCaseBondCashCollection GetCollectionToTest()
		{
			return new USCACCaseBondCashCollection(ParentCase);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = (USCACCaseBondCash)base.GetNewElementToAddToTheCollection();
			result.U8_CaseNumber = "A1";
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
