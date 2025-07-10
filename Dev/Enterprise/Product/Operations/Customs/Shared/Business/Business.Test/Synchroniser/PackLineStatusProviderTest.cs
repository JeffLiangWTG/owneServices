using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Integration;

namespace Enterprise.Customs.Business.Testing
{
	sealed class PackLineStatusProviderTest : TestCaseWithFactory
	{
		public void TestPackLineStatusProvider()
		{
			PackLineStatusProvider packLineStatusProvider = new PackLineStatusProvider();
			IPackLineStatus packLineStatus = packLineStatusProvider.GetPackLineStatus(Factory, Core.Constants.CountryCodes.Australia);
			AssertNotNull(packLineStatus);
			packLineStatus = packLineStatusProvider.GetPackLineStatus(Factory, Core.Constants.CountryCodes.NewZealand);
			AssertNotNull(packLineStatus);
			packLineStatus = packLineStatusProvider.GetPackLineStatus(Factory, Core.Constants.CountryCodes.UnitedStates);
			AssertNull(packLineStatus);
		}
	}
}
