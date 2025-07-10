using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusDisposition))]
	sealed class CusDispositionTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var bizObj = base.GetNewBusinessObjectForDeleteTest(factory) as CusDisposition;
			bizObj.CDI_ParentID = Declaration.PK;
			bizObj.CDI_ParentTableCode = "JE";
			return bizObj;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			if (GetType() == typeof(CusDispositionTest))
			{
				Assert($"Covered by {nameof(FetchStrategies.Testing.CusDispositionFetchStrategyTest)}.", true);
				return;
			}

			base.TestCalcPropertiesWithDbHitsUseFetchHints();
		}

		BaseJobDeclaration Declaration
		{
			get
			{
				return declaration ?? (declaration = Factory.New<BaseJobDeclaration>());
			}
		}
		BaseJobDeclaration declaration;
	}
}
