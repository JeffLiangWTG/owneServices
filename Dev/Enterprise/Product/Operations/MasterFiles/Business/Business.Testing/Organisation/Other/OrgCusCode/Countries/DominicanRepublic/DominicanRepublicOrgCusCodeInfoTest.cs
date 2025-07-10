using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DominicanRepublicOrgCusCodeInfoTest : TestCaseWithFactory
	{
		public void TestGetCodesCannotCoexistInParentCountry()
		{
			var conflictingCodes = new List<ZString> { DominicanRepublicOrgCusCodeInfo.OrgCusCodes.REG, DominicanRepublicOrgCusCodeInfo.OrgCusCodes.RCS };
			var notConflictingCode = ArgentinaOrgCusCodeInfo.OrgCusCodes.CUIT;

			var addCusCode1 = Factory.NewWithValidTestData<OrgCusCode>();
			var addCusCode2 = Factory.NewWithValidTestData<OrgCusCode>();

			addCusCode1.OK_RN_NKCodeCountry = Constants.CountryCodes.DominicanRepublic;
			addCusCode2.OK_RN_NKCodeCountry = Constants.CountryCodes.Argentina;

			foreach (var code1 in conflictingCodes)
			{
				addCusCode1.OK_CodeType = code1;
				AssertNoErrors(addCusCode1.OK_CodeTypeInfo);

				foreach (var code2 in conflictingCodes)
				{
					addCusCode2.OK_CodeType = code2;
					AssertHasErrors("Expected Error message", addCusCode2.OK_CodeTypeInfo);

					addCusCode2.OK_CodeType = notConflictingCode;
					AssertNoErrors(addCusCode2.OK_CodeTypeInfo);
				}
			}
		}
	}
}
