using CargoWise.Integration;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CanadaProvinceTerritoryCodesTest : NUnit.Framework.TestCase
	{
		public void TestGetCodeFromNormalStateCode()
		{
			AssertEquals(CanadaProvinceTerritoryCodes.GetCodeFromNormalStateCode(CanadaStatesList.Codes.AB), CanadaProvinceTerritoryCodes.Codes.XA);
			AssertEquals(CanadaProvinceTerritoryCodes.GetCodeFromNormalStateCode(CanadaStatesList.Codes.NB), CanadaProvinceTerritoryCodes.Codes.XB);
			AssertEquals(CanadaProvinceTerritoryCodes.GetCodeFromNormalStateCode(CanadaStatesList.Codes.BC), CanadaProvinceTerritoryCodes.Codes.XC);
			AssertEquals(CanadaProvinceTerritoryCodes.GetCodeFromNormalStateCode(CanadaStatesList.Codes.MB), CanadaProvinceTerritoryCodes.Codes.XM);
			AssertEquals(CanadaProvinceTerritoryCodes.GetCodeFromNormalStateCode(CanadaStatesList.Codes.NS), CanadaProvinceTerritoryCodes.Codes.XN);
			AssertEquals(CanadaProvinceTerritoryCodes.GetCodeFromNormalStateCode(CanadaStatesList.Codes.ON), CanadaProvinceTerritoryCodes.Codes.XO);
			AssertEquals(CanadaProvinceTerritoryCodes.GetCodeFromNormalStateCode(CanadaStatesList.Codes.PE), CanadaProvinceTerritoryCodes.Codes.XP);
			AssertEquals(CanadaProvinceTerritoryCodes.GetCodeFromNormalStateCode(CanadaStatesList.Codes.QC), CanadaProvinceTerritoryCodes.Codes.XQ);
			AssertEquals(CanadaProvinceTerritoryCodes.GetCodeFromNormalStateCode(CanadaStatesList.Codes.SK), CanadaProvinceTerritoryCodes.Codes.XS);
			AssertEquals(CanadaProvinceTerritoryCodes.GetCodeFromNormalStateCode(CanadaStatesList.Codes.NT), CanadaProvinceTerritoryCodes.Codes.XT);
			AssertEquals(CanadaProvinceTerritoryCodes.GetCodeFromNormalStateCode(CanadaStatesList.Codes.NU), CanadaProvinceTerritoryCodes.Codes.XV);
			AssertEquals(CanadaProvinceTerritoryCodes.GetCodeFromNormalStateCode(CanadaStatesList.Codes.NL), CanadaProvinceTerritoryCodes.Codes.XW);
			AssertEquals(CanadaProvinceTerritoryCodes.GetCodeFromNormalStateCode(CanadaStatesList.Codes.YT), CanadaProvinceTerritoryCodes.Codes.XY);
		}

		public void TestIsCanadianSoftwoodLumberRegion()
		{
			string[] canadianCanadianSoftwoodLumberProvinces = new string[]
			{
				CanadaProvinceTerritoryCodes.Codes.XA,
				CanadaProvinceTerritoryCodes.Codes.XC,
				CanadaProvinceTerritoryCodes.Codes.XD,
				CanadaProvinceTerritoryCodes.Codes.XE,
				CanadaProvinceTerritoryCodes.Codes.XM,
				CanadaProvinceTerritoryCodes.Codes.XO,
				CanadaProvinceTerritoryCodes.Codes.XS,
				CanadaProvinceTerritoryCodes.Codes.XQ
			};
			CanadaProvinceTerritoryCodes list = new CanadaProvinceTerritoryCodes();

			foreach (string code in canadianCanadianSoftwoodLumberProvinces)
			{
				AssertEquals(true, CanadaProvinceTerritoryCodes.IsCanadianSoftwoodLumberRegion(code));
				list.RemoveCode(code);
			}

			foreach (ICodeDescription code in list)
			{
				AssertEquals(code.Code, false, CanadaProvinceTerritoryCodes.IsCanadianSoftwoodLumberRegion(code.Code));
			}
		}
	}
}
