using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(VehicleIdentificationNumberCollection))]
	sealed class VehicleIdentificationNumberCollectionTest : CusCodeDataCollectionTest<VehicleIdentificationNumber>
	{
		protected override CusCodeDataCollection<VehicleIdentificationNumber> GetCusCodeDataCollection()
		{
			return new VehicleIdentificationNumberCollection(Factory.New<Commodity>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<VehicleIdentificationNumber>();
			var commodity = Factory.New<Commodity>();
			result.CY_ParentID = commodity.PK;
			result.CY_ParentTableCode = commodity.TablePrefix;
			return result;
		}
	}
}
