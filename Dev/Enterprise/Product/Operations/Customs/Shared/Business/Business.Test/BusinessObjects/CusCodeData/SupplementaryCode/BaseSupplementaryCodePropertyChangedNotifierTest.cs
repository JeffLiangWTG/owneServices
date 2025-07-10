using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class BaseSupplementaryCodePropertyChangedNotifierTest : TestCaseWithFactory
	{
		public abstract void TestDefaultUOMsBySupplementaryCode1();

		public abstract void TestDefaultUOMsBySupplementaryCode2();

		public abstract void TestDefaultUOMsByAdditionalSupplementaryCodes();

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Should throw exception when parent is null", () => new BaseSupplementaryCodePropertyChangedNotifier(null));

			var supplementaryCode = Factory.New<BaseSupplementaryCode>();
			var supplementaryCodeSupporterForTest = new SupplementaryCodeSupporterForTest(Factory, "EUN");
			AssertNoExceptionThrown("No exception expected", () => new BaseSupplementaryCodePropertyChangedNotifier(supplementaryCode));
		}

		public void TestNotifyChange()
		{
			var supplementaryCodeSupporterForTest = new SupplementaryCodeSupporterForTest(Factory, "EUN");
			var supplementaryCode = Factory.New<BaseSupplementaryCode>();
			var propertyChangedNotifier = new SupplementaryCodeNotifierPropertyChangedForTest(supplementaryCode);

			propertyChangedNotifier.NotifyChange(BaseSupplementaryCode.Schema.CY_Code, "", "Test");
			AssertEquals("CY_CodeForTest", "Test", propertyChangedNotifier.CY_CodeForTest);

			AssertNoExceptionThrown("No exception expected when property is unknown", () => propertyChangedNotifier.NotifyChange("XXX", "", ""));
		}

		class SupplementaryCodeNotifierPropertyChangedForTest : BaseSupplementaryCodePropertyChangedNotifier
		{
			public SupplementaryCodeNotifierPropertyChangedForTest(BaseSupplementaryCode parent) : base(parent)
			{
			}

			public ZString CY_CodeForTest { get; private set; }

			protected override void NotifyPropertyCY_CodeChanged(ZString oldValue, ZString newValue)
			{
				CY_CodeForTest = newValue;
			}
		}
	}
}
