using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(SealNumberCollection))]
	sealed class SealNumberCollectionTest : CusCodeDataCollectionTest<SealNumber>
	{
		protected override CusCodeDataCollection<SealNumber> GetCusCodeDataCollection() => new SealNumberCollection(Factory.New<Equipment>());

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<SealNumber>();
			var equipment = Factory.New<Equipment>();
			result.CY_ParentID = equipment.PK;
			result.CY_ParentTableCode = equipment.TablePrefix;
			return result;
		}
	}
}
