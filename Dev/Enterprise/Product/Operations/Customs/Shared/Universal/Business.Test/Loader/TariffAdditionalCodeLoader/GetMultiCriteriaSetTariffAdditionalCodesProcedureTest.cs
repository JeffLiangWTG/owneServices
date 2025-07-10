using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing.Loader.TariffAdditionalCodeLoader
{
	class GetMultiCriteriaSetTariffAdditionalCodesProcedureTest : TestCase
	{
		public void TestProperties()
		{
			AssertEquals("dbo.GetMultiCriteriaSetTariffAdditionalCodes", GetMultiCriteriaSetTariffAdditionalCodesProcedure.QualifiedName);
			AssertEquals("@TariffAdditionalCodeTvp", GetMultiCriteriaSetTariffAdditionalCodesProcedure.Parameters.CriteriaTvp);
			AssertEquals("CriteriaId", GetMultiCriteriaSetTariffAdditionalCodesProcedure.Columns.CriteriaId);
			AssertEquals("TariffAdditionalCodePk", GetMultiCriteriaSetTariffAdditionalCodesProcedure.Columns.DataPk);
		}
	}
}
