using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCACCaseLiqSuspensionCollection))]
	sealed class USCACCaseLiqSuspensionCollectionTest : ActiveBusinessObjectCollectionTestCase<USCACCaseLiqSuspensionCollection>
	{
		protected override USCACCaseLiqSuspensionCollection GetCollectionToTest()
		{
			return new USCACCaseLiqSuspensionCollection(ParentCase);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = (USCACCaseLiqSuspension)base.GetNewElementToAddToTheCollection();
			result.UN_CaseNumber = "A1";
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
