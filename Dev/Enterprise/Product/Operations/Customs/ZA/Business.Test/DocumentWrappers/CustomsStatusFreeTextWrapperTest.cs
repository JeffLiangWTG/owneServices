using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(CustomsStatusFreeTextWrapper))]
	sealed class CustomsStatusFreeTextWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var test = new CustomsStatusFreeTextWrapper("line", "code", "free text");
			AssertEquals("line", test.Line);
			AssertEquals("code", test.Code);
			AssertEquals("free text", test.FreeText);
		}

		protected override BusinessObject GetNewBusinessObject() => new CustomsStatusFreeTextWrapper("", "", "");
	}
}
