using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CustomFieldInfoTest : TestCaseWithFactory
	{
		public void TestNew_SchemaColumn()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => CustomFieldInfo.New(true, "caption", "hint", null));

			var customFieldInfo = CustomFieldInfo.New(true, "caption", "hint", DummyBizoSchema.Z0_Number);

			AssertEquals(true, customFieldInfo.IsActive);
			AssertEquals("caption", customFieldInfo.Caption);
			AssertEquals("hint", customFieldInfo.Hint);
			AssertEquals("Z0_Number", customFieldInfo.SchemaColumnName);
			AssertEquals(typeof(ZInt), customFieldInfo.ZDataType);
		}

		public void TestNew_SchemaColumnName()
		{
			AssertExceptionThrown(typeof(ArgumentException), () => CustomFieldInfo.New<ZString>(true, "caption", "hint", ""));

			var customFieldInfo = CustomFieldInfo.New<ZString>(true, "caption", "hint", "Z0_Description");

			AssertEquals(true, customFieldInfo.IsActive);
			AssertEquals("caption", customFieldInfo.Caption);
			AssertEquals("hint", customFieldInfo.Hint);
			AssertEquals("Z0_Description", customFieldInfo.SchemaColumnName);
			AssertEquals(typeof(ZString), customFieldInfo.ZDataType);
		}
	}
}
