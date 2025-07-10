using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(HarmonizedNumberCollection))]
	sealed class HarmonizedNumberCollectionTest : CusCodeDataCollectionTest<HarmonizedNumber>
	{
		protected override CusCodeDataCollection<HarmonizedNumber> GetCusCodeDataCollection() => new HarmonizedNumberCollection(Factory.New<Commodity>());

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<HarmonizedNumber>();
			var commodity = Factory.New<Commodity>();
			result.CY_ParentID = commodity.PK;
			result.CY_ParentTableCode = commodity.TablePrefix;
			return result;
		}
	}
}
