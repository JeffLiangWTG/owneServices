using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class N5101HFunctionalReferenceIdGeneratorTest : TestCaseWithFactory
	{
		[TestDate(2022, 06, 27)]
		public void TestGetFunctionalReferenceId()
		{
			var todayAsFormattedString = "22JUN27";
			var vat = "52889317";
			var orgCusCode = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
			orgCusCode.FillWithValidTestData();
			orgCusCode.OK_CustomsRegNo = vat;
			orgCusCode.OK_CodeType = "VAT";
			orgCusCode.OK_RN_NKCodeCountry = "TW";
			AssertEquals(vat + todayAsFormattedString + "00001", N5101HFunctionalReferenceIdGenerator.GetFunctionalReferenceId(Factory));
		}
	}
}
