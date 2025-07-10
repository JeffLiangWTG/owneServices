using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCACCaseTariffCollection))]
	sealed class USCACCaseTariffCollectionTest : ActiveBusinessObjectCollectionTestCase<USCACCaseTariffCollection>
	{
		protected override USCACCaseTariffCollection GetCollectionToTest()
		{
			return new USCACCaseTariffCollection(ParentCase);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = (USCACCaseTariff)base.GetNewElementToAddToTheCollection();
			result.U9_CaseNumber = "A1";
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
