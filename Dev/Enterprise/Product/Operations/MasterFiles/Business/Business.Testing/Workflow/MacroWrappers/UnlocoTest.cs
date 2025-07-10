using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Macros.Testing
{
	sealed class UnlocoTest : TestCaseWithFactory
	{
		public void TestUnloco()
		{
			var refUnloco = new RefUNLOCO.Loader(Factory).Load("AUSYD");
			var unloco = new Unloco(refUnloco);

			CombineAssertions(() =>
			{
				AssertEquals("Code", "AUSYD", unloco.Code);
				AssertEquals("Name", "Sydney", unloco.Name);
				AssertEquals("Country.Code", "AU", unloco.Country.Code);
			});
		}

		public void TestNullUnloco()
		{
			var unloco = new Unloco(null);

			CombineAssertions(() =>
			{
				AssertNullOrEmpty("Code", unloco.Code);
				AssertNullOrEmpty("Name", unloco.Name);
				AssertNotNull("Country", unloco.Country);
			});
		}
	}
}
