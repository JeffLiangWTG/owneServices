using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Moq;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class BondedHelperTest : TestCaseWithFactory
	{
		#region TestIsCountrySupportedForBonded

		public void TestIsCountrySupportedForBonded()
		{
			var supportedForBonded = new Mock<Enterprise.Integration.Customs.ISupportedForBonded>();
			supportedForBonded.Setup(m => m.IsSupportsBondedWarehousing(
				It.IsAny<BusinessObjectFactory>(), It.IsAny<ZString>())).Returns(true);
			ObjectFactory.Substitute(supportedForBonded.Object);
			AssertEquals(true, BondedHelper.IsCountrySupportedForBonded(Factory, Constants.CountryCodes.Singapore));
			AssertEquals(true, BondedHelper.IsCountrySupportedForBonded(Factory, Constants.CountryCodes.UnitedKingdom));
			AssertEquals(true, BondedHelper.IsCountrySupportedForBonded(Factory, Constants.CountryCodes.Australia));
			AssertEquals(true, BondedHelper.IsCountrySupportedForBonded(Factory, Constants.CountryCodes.NewZealand));
			supportedForBonded.VerifyAll();
		}

		public void TestIsCountrySupportedForBondedFalse()
		{
			var supportedForBonded = new Mock<Enterprise.Integration.Customs.ISupportedForBonded>();
			supportedForBonded.Setup(m => m.IsSupportsBondedWarehousing(
				It.IsAny<BusinessObjectFactory>(), It.IsAny<ZString>())).Returns(false);
			ObjectFactory.Substitute(supportedForBonded.Object);
			AssertEquals(false, BondedHelper.IsCountrySupportedForBonded(Factory, Constants.CountryCodes.Singapore));
			AssertEquals(false, BondedHelper.IsCountrySupportedForBonded(Factory, Constants.CountryCodes.UnitedKingdom));
			AssertEquals(false, BondedHelper.IsCountrySupportedForBonded(Factory, Constants.CountryCodes.Australia));
			AssertEquals(false, BondedHelper.IsCountrySupportedForBonded(Factory, Constants.CountryCodes.NewZealand));
			supportedForBonded.VerifyAll();
		}

		#endregion

		#region TestIsCountrySupportedForFTZPermits

		public void TestIsCountrySupportedForFTZPermits()
		{
			AssertEquals(false, BondedHelper.IsCountrySupportedForFTZPermits(""));
			AssertEquals(false, BondedHelper.IsCountrySupportedForFTZPermits(Constants.CountryCodes.Singapore));

			foreach (var country in BondedHelper.SupportedCountriesForFTZPermits)
			{
				AssertEquals(true, BondedHelper.IsCountrySupportedForFTZPermits(country));
			}
		}

		#endregion

		#region TestSupportedCountriesForFTZPermits

		public void TestSupportedCountriesForFTZPermits()
		{
			AssertContainsExactElementsInAnyOrder(new[]
			{
				Constants.CountryCodes.UnitedStates,
				Constants.CountryCodes.PuertoRico,
			}, BondedHelper.SupportedCountriesForFTZPermits);
		}

		#endregion

		#region Helper

		protected WhsTestHelperFunctionsEnv Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new WhsTestHelperFunctionsEnv(Factory);
				}
				return helper;
			}
		}
		WhsTestHelperFunctionsEnv helper;

		#endregion
	}
}
