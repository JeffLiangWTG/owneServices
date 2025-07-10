using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class TranCircumstancePropertyChangedNotifierTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Should throw exception when parent is null", () => new TranCircumstancePropertyChangedNotifier(null));

		var supplementaryCode = Factory.New<TranCircumstance>();
		AssertNoExceptionThrown("No exception expected", () => new TranCircumstancePropertyChangedNotifier(supplementaryCode));
	}

	public void TestNotifyChange()
	{
		var supplementaryCode = Factory.New<TranCircumstance>();
		var propertyChangedNotifier = new SupplementaryCodeNotifierPropertyChangedForTest(supplementaryCode);

		propertyChangedNotifier.NotifyChange(TranCircumstance.Schema.CY_Code, "", "Test");
		AssertEquals("CY_CodeForTest", "Test", propertyChangedNotifier.CY_CodeForTest);

		AssertNoExceptionThrown("No exception expected when property is unknown", () => propertyChangedNotifier.NotifyChange("XXX", "", ""));
	}

	class SupplementaryCodeNotifierPropertyChangedForTest : TranCircumstancePropertyChangedNotifier
	{
		public SupplementaryCodeNotifierPropertyChangedForTest(TranCircumstance parent) : base(parent)
		{
		}

		public ZString CY_CodeForTest { get; private set; }

		protected override void NotifyPropertyCY_CodeChanged(ZString oldValue, ZString newValue)
		{
			CY_CodeForTest = newValue;
		}
	}
}
