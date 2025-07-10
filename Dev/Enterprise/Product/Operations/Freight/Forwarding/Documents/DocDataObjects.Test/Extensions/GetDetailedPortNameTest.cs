using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class GetDetailedPortNameTest : TestCaseWithFactory
	{
		public void TestName()
		{
			var context = new CommonContext(Factory);

			var unloco = new Unloco(Factory, context.Unlocos, context.Countries).WithCustomNameProvider(UnlocoExtensions.GetDetailedPortName);
			unloco.Code = "PLWRO";
			AssertEquals("Name", "Wroclaw, Poland", unloco.Name);

			unloco.Name = "test";
			AssertEquals("Name", "test", unloco.Name);
		}

		public void TestNameMaxLengthOverload()
		{
			var context = new CommonContext(Factory);

			var refUnloco = Factory.New<RefUNLOCO>();
			refUnloco.RL_Code = "ABCDE";
			refUnloco.RL_PortName = "12345678901234567890123456789012345";

			var refCountry = Factory.New<RefCountry>();
			refCountry.RN_Code = "37";

			var refCountryStates = Factory.New<RefCountryStates>();
			refCountryStates.RW_RN_NKCountryCode = refCountry.RN_Code;
			refCountryStates.RW_Code = "31";
			refCountryStates.RW_Description = "DESC";

			refUnloco.RL_RW = refCountryStates.PK;

			var unlocoWithDetailedPortName = new Unloco(Factory, context.Unlocos, context.Countries).WithCustomNameProvider(UnlocoExtensions.GetDetailedPortName);
			AssertNoExceptionThrown(() => unlocoWithDetailedPortName.Code = refUnloco.RL_Code);
			AssertLessThanOrEqualTo("Check max length", unlocoWithDetailedPortName.Name.Length, AutoRefUNLOCO.Schema.RL_PortNameMaxLength);
		}

		public void TestUSNoCountryStates()
		{
			var context = new CommonContext(Factory);

			var unloco = new Unloco(Factory, context.Unlocos, context.Countries).WithCustomNameProvider(UnlocoExtensions.GetDetailedPortName);
			AssertNoExceptionThrown(() => unloco.Code = "USWFO");
			AssertEquals("Name", "West Foreland, United States", unloco.Name);
		}
	}
}
