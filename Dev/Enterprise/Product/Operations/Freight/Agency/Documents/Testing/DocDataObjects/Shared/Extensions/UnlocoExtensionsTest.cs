using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	class UnlocoExtensionsTest : TestCaseWithFactory
	{
		public void TestGetDetailedPortName()
		{
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			AssertEquals("Sydney, Australia", UnlocoExtensions.GetDetailedPortName(ausyd));
		}

		public void TestCreateFromUNLOCO()
		{
			var context = new CommonContext(Factory.GetCachedReadOnlyFactory());
			var universalUNLOCO = new UNLOCO()
			{
				Code = "AUSYD",
				Name = "Australia Sydney Port"
			};

			var unloco = UnlocoExtensions.CreateFromUNLOCO(context, universalUNLOCO);
			AssertEquals(universalUNLOCO.Code.Value, unloco.Code);
			AssertEquals(universalUNLOCO.Name.Value, unloco.Name);
			AssertEquals(universalUNLOCO.Code.Value.Substring(0, 2), unloco.Country.Code);

			unloco = UnlocoExtensions.CreateFromUNLOCO(context, universalUNLOCO, true);
			AssertEquals(universalUNLOCO.Code.Value, unloco.Code);
			AssertEquals("Sydney, Australia", unloco.Name);
			AssertEquals(universalUNLOCO.Code.Value.Substring(0, 2), unloco.Country.Code);
		}
	}
}
