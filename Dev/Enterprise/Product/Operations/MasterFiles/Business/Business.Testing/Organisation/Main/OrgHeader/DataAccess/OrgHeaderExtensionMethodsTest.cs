using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgHeaderExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestUSLocalCustomsCarrierCode()
		{
			var header = Factory.New<OrgHeader>();
			var code1 = header.CustomsCodes.AddNew();
			code1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			code1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			code1.OK_CustomsRegNo = "4321";

			var code2 = header.CustomsCodes.AddNew();
			code2.OK_CodeType = OrgCusCode.CodeTypes.TruckCarrierCode;
			code2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			code2.OK_CustomsRegNo = "432A";

			var code3 = header.CustomsCodes.AddNew();
			code3.OK_CodeType = OrgCusCode.USACodeTypes.CarrierPrefixCode;
			code3.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			code3.OK_CustomsRegNo = "432B";

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.PuertoRico);
			AssertEquals("4321", header.USLocalCustomsCarrierCode());
			AssertEquals("432A", header.USLocalCustomsCarrierCode(true));
			AssertContainsExactElementsInAnyOrder(new List<ZString> { "4321", "432B" }, header.USLocalCustomsCarrierCodes());
			AssertContainsExactElementsInAnyOrder(new List<ZString> { "432A", "4321", "432B" }, header.USLocalCustomsCarrierCodes(true));
		}
	}
}
