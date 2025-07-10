using System;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing
{
	using Enterprise.ZArchitecture.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(NZCGroup))]
	class NZCGroupTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFromCountry()
		{
			NZCGroup[] groups = NZCGroup.FromCountry(Enterprise.Core.Constants.CountryCodes.Thailand, new DateTime(2009, 11, 21), Factory);

			AssertEquals("groups.Length", 2, groups.Length);
			AssertEquals("groups[0].Q4_Code", "TH", groups[0].Q4_Code);
			AssertEquals("groups[1].Q4_Code", "LDC", groups[1].Q4_Code);
		}
	}
}
