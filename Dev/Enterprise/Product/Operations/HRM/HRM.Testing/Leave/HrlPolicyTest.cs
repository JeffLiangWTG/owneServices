using CargoWise.EntityFramework;
using Enterprise.HRM.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.HRM.Testing
{
	[TestedType(typeof(HrlPolicy))]
	class HrlPolicyTest : EnterpriseBusinessObjectTestCase
	{
		public void TestStateDefaultConstraintIsPerCountry()
		{
			var p = Factory.NewWithValidTestData<HrlPolicy>();
			p.LLP_Name = "Some policy";
			p.LLP_RN_NKCountry = "AU";
			p.LLP_RW_NKState = "SA";
			p.LLP_IsStateDefault = true;

			Factory.Save();

			var p2 = Factory.NewWithValidTestData<HrlPolicy>();
			p2.LLP_Name = "Another policy";
			p2.LLP_RN_NKCountry = "BQ";
			p2.LLP_RW_NKState = "SA";
			p2.LLP_IsStateDefault = true;

			AssertNoExceptionThrown("States with the same code, but from different countries, should not violate the 'only one default' constraint", () => Factory.Save());
		}

		public void TestStateDefaultConstraintRejectsTwoDefaultsForSameState()
		{
			var p = Factory.NewWithValidTestData<HrlPolicy>();
			p.LLP_Name = "Some policy";
			p.LLP_RN_NKCountry = "AU";
			p.LLP_RW_NKState = "SA";
			p.LLP_IsStateDefault = true;

			Factory.Save();

			var p2 = Factory.NewWithValidTestData<HrlPolicy>();
			p2.LLP_Name = "Another policy";
			p2.LLP_RN_NKCountry = "AU";
			p2.LLP_RW_NKState = "SA";
			p2.LLP_IsStateDefault = true;

			AssertExceptionThrown<ZSaveException>("You cannot have two defaults policies for the same state.", () => Factory.Save());
		}
	}
}
