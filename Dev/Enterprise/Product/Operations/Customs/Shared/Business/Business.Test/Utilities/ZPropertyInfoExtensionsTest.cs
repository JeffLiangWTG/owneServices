using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ZPropertyInfoExtensionsTest : TestCaseWithFactory
	{
		public void TestSetValueSafe()
		{
			var biz = Factory.New<DummyBusinessObject>();
			((ZPropertyInfo<ZString>)biz.Z0_CodeInfo).SetValueSafe("A long string.");
			AssertEquals("A lon", biz.Z0_Code);
		}

		public void TestGetAttribute()
		{
			var bizObj = Factory.New<TestClass>();
			AssertEquals(typeof(IsNAddInfoFieldAttribute), bizObj.TC_String2Info.GetAttribute<IsNAddInfoFieldAttribute>().GetType());
		}
	}
}
