using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USCACCaseEventCollection))]
	sealed class USCACCaseEventCollectionTest : ActiveBusinessObjectCollectionTestCase<USCACCaseEventCollection>
	{
		protected override USCACCaseEventCollection GetCollectionToTest()
		{
			return new USCACCaseEventCollection(ParentCase);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = (USCACCaseEvent)base.GetNewElementToAddToTheCollection();
			result.U7_CaseNumber = "A1";
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
