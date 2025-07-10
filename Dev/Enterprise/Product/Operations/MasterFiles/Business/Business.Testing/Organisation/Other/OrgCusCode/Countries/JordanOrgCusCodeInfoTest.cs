using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JordanOrgCusCodeInfo))]
	sealed class JordanOrgCusCodeInfoTest : TestCaseWithFactory
	{
		public void TestOrgCusCodeInfoListContainsCodes()
		{
			var list = new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.Jordan);
			CombineAssertions(() =>
			{
				foreach (var codeAndDescription in ExpectedCustomsCodeAndDescription())
				{
					var code = codeAndDescription.code;
					var description = codeAndDescription.description;
					Assert($"Code: {code} should be present", list.ContainsCode(code));
					AssertEquals($"Description of {code} should be same as expectation", description, list.GetDescriptionFromCode(code));
				}
			});
		}

		IEnumerable<(string code, string description)> ExpectedCustomsCodeAndDescription()
		{
			yield return ("GST", Country.GetDefaultTaxCodeDescription("GST"));
			yield return ("IDN", "Identity Card Number");
			yield return ("BAN", "Business Activity Number");
		}
	}
}
