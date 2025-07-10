using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(C4CodeCollection))]
	sealed class C4CodeCollectionTest : CusCodeDataCollectionTest<C4Code>
	{
		protected override CusCodeDataCollection<C4Code> GetCusCodeDataCollection() => new C4CodeCollection(Factory.New<Commodity>());

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<C4Code>();
			var commodity = Factory.New<Commodity>();
			result.CY_ParentID = commodity.PK;
			result.CY_ParentTableCode = commodity.TablePrefix;
			return result;
		}
	}
}
