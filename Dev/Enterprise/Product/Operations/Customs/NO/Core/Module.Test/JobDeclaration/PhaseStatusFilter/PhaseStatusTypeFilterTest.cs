using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Module.Testing;

[TestedType(typeof(PhaseStatusTypeFilter))]
sealed class PhaseStatusTypeFilterTest : NonPersistentBusinessObjectTestCase
{
	public void TestPhaseStatusFilter_ThrowsArgumentException()
	{
		_ = AssertExceptionThrown<NullReferenceException>(() => new PhaseStatusTypeFilter("Phase Status", null, () => new CodeDescriptionPairList()));
		_ = AssertExceptionThrown<ArgumentException>(() => new PhaseStatusTypeFilter("Phase Status", delegate { return new ZQuery(); }, null));
	}

	public void TestTypeList()
	{
		AssertNotNull("TypeList should not be null after first access.", Filter.TypeList);
		AssertEquals("TypeList should return the same instance on multiple accesses.", Filter.TypeList, Filter.TypeList);
	}

	public void TestDefaultValues()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Filter Type should be - Any", PhaseStatusFilterTypeList.Codes.Any, Filter.FilterType);
			AssertEquals("Property value is Empty", ZString.Empty, Filter.Property);
		});
	}

	public void TestClear()
	{
		Filter.Property = CustomsEntryPhaseStatusList.Codes.Reminder;
		Filter.FilterType = PhaseStatusFilterTypeList.Codes.All;

		CombineAssertions(() =>
		{
			AssertEquals("Before Clear : Filter Type should be - All", PhaseStatusFilterTypeList.Codes.All, Filter.FilterType);
			AssertEquals("Before Clear : Property value is Empty", CustomsEntryPhaseStatusList.Codes.Reminder, Filter.Property);
			Filter.Clear();
			AssertEquals("After Clear : Filter Type should be - Any", PhaseStatusFilterTypeList.Codes.Any, Filter.FilterType);
			AssertEquals("After Clear : Property value is Empty", ZString.Empty, Filter.Property);
		});
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return new PhaseStatusTypeFilter("Phase Status",
			delegate { return new ZQuery(); },
			() => new CodeDescriptionPairList());
	}

	PhaseStatusTypeFilter Filter => filter ??= (PhaseStatusTypeFilter)GetNewBusinessObject();
	PhaseStatusTypeFilter filter;
}
