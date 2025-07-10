using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusMapType))]
	class RefCusMapTypeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZZP_Direction()
		{
			CombineAssertions("Valid Direction Code must be used", () =>
			{
				var testMapType = Factory.NewWithValidTestData<RefCusMapType>();
				testMapType.ZZP_MapType = "ABC";
				testMapType.ZZP_Direction = "ABC";
				var ex = AssertExceptionThrown<ZSaveException>(() => Factory.Save());
				AssertContains("The INSERT statement conflicted with the CHECK constraint \"CK_RefCusMapType_ZZP_Direction\".", ex.Message);

				testMapType.ZZP_Direction = MapDirectionList.Codes.BTH;
				AssertNoExceptionThrown(() => Factory.Save());
			});
		}

		public void TestDefaultValues()
		{
			var testMapType = Factory.New<RefCusMapType>();
			AssertEquals(ZBool.False, testMapType.ZZP_IsReadonly);
		}
	}
}
