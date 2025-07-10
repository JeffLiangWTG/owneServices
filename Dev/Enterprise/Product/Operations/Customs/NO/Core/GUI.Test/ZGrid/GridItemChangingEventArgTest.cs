using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(GridItemChangingEventArg<>))]
sealed class GridItemChangingEventArgTest : TestCase
{
	public void TestProperties() => CombineAssertions(() =>
	{
		var dummyItem1 = new DummyClass();
		var dummyItem2 = new DummyClass();
		var args = new GridItemChangingEventArg<DummyClass>(dummyItem1, dummyItem2);

		AssertSame("OldItem", dummyItem1, args.OldItem);
		AssertSame("NewItem", dummyItem2, args.NewItem);
	});

	class DummyClass { }
}
