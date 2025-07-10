using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DutyFeeInformationDocWrapper))]
	sealed class DutyFeeInformationDocWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstruction()
		{
			var tester = new DutyFeeInformationDocWrapper("123", "123");
			AssertEquals("123", tester.Code);
			AssertEquals(123m, tester.Value);
			tester = new DutyFeeInformationDocWrapper("123", "xxx");
			AssertEquals("123", tester.Code);
			AssertEquals(0m, tester.Value);
		}

		protected override BusinessObject GetNewBusinessObject() => new DutyFeeInformationDocWrapper("", "");
	}
}
