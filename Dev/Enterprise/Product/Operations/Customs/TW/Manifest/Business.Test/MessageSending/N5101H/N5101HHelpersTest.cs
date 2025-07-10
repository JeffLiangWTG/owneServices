using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class N5101HHelpersTest : TestCaseWithFactory
	{
		public void TestRegNoTypeToTypeCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("VAT", TW.Business.PartyIdentifierCodeList.Codes._58, N5101HHelpers.RegNoTypeToTypeCode(OrgCusCode.CodeTypes.VATCode));
				AssertEquals("PAS", TW.Business.PartyIdentifierCodeList.Codes._53, N5101HHelpers.RegNoTypeToTypeCode(OrgCusCode.CodeTypes.PassportID));
				AssertEquals("PID", TW.Business.PartyIdentifierCodeList.Codes._174, N5101HHelpers.RegNoTypeToTypeCode(OrgCusCode.TaiwanCodeTypes.PID));
				AssertEquals("Other type", ZString.Empty, N5101HHelpers.RegNoTypeToTypeCode("Oth"));
			});
		}

		public void TestGetSeals()
		{
			var container = Factory.New<AsycudaContainer>();
			container.ACN_Seal1 = "Seal1";
			container.ACN_Seal2 = "Seal2";
			container.ACN_Seal3 = "Seal3";
			var actual = container.GetSeals().ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("Seal1", actual[0]);
				AssertEquals("Seal2", actual[1]);
				AssertEquals("Seal3", actual[2]);
			});
		}
	}
}
