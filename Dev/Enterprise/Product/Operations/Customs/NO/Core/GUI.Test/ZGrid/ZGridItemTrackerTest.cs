using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(ZGridItemTracker<>))]
sealed class ZGridItemTrackerTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When grid is null", () => new ZGridItemTracker<DummyChildBusinessObject>(null));
	}

	public void TestItemEventsOnItemChange()
	{
		int itemChangedCount = 0, itemChangingCount = 0;
		using var form = new ZForm(dummyBusinessObject);
		using var grid = CreateGridForTest();
		using var tracker = new ZGridItemTracker<DummyChildBusinessObject>(grid);
		form.Controls.Add(grid);
		GridItemChangingEventArg<DummyChildBusinessObject> changingArgs = null;
		tracker.OnCurrentItemChanged += (_, _) => itemChangedCount++;
		tracker.OnCurrentItemChanging += (_ , args) =>
		{
			itemChangingCount++;
			changingArgs = args;
		};

		grid.SetDataBinding(dummyBusinessObject, nameof(DummyBusinessObject.Collection));
		form.Show();

		CombineAssertions("When First Row is selected", () =>
		{
			AssertEquals("Item Changing Count", 1, itemChangingCount);
			AssertEquals("Item Changed Count", 1, itemChangedCount);
			AssertNull("Old Item", changingArgs.OldItem);
			AssertSame("New Item", child1, changingArgs.NewItem);
		});

		grid.CurrentRowIndex = 1;
		CombineAssertions("When Second Row is selected", () =>
		{
			AssertEquals("Item Changing Count", 2, itemChangingCount);
			AssertEquals("Item Changed Count", 2, itemChangedCount);
			AssertSame("OLd Item", child1, changingArgs.OldItem);
			AssertSame("New Item", child2, changingArgs.NewItem);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		dummyBusinessObject = Factory.New<DummyBusinessObject>();
		child1 = dummyBusinessObject.Collection.AddNew();
		child2 = dummyBusinessObject.Collection.AddNew();
	}

	DummyBusinessObject dummyBusinessObject;
	DummyChildBusinessObject child1, child2;

	ZGrid CreateGridForTest()
	{
		var grid = new ZGrid();
		grid.AllowNavigation = false;
		var textBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo(DummyChildBusinessObject.Schema.Z0_Code, 100);
		grid.ColumnStyles.Add(textBoxColumnStyleInfo);
		return grid;
	}
}
