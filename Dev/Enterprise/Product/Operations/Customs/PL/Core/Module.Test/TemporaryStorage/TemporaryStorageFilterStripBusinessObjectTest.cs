using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.TemporaryStorage.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Module.Testing;

[TestedType(typeof(TemporaryStorageFilterStripBusinessObject))]
class TemporaryStorageFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new TemporaryStorageFilterStripBusinessObject();

	public void TestJobNumber()
	{
		AssertModuleTextFilter(CusTempStorageJobHeaderSchema.Constants.SJH_JobReference, "Job #");
	}

	public void TestGetFilterInflators()
	{
		var filterStrip = new TemporaryStorageFilterStripBusinessObjectForTest();
		var filterInflators = filterStrip.GetFilterInflators_Exposed();
		AssertContainsExactElementsInAnyOrder(ExpectedFilterInflatorTypes, filterInflators.Select(inf => inf.GetType()));
	}

	void AssertModuleTextFilter(string propertyName, string filterName)
	{
		var header1 = GetNewTempStorageJobHeader();
		header1[propertyName] = "ABC";

		var header2 = GetNewTempStorageJobHeader();
		header2[propertyName] = "DEF";

		Factory.Save();

		var filter = new TemporaryStorageFilterStripBusinessObject();
		var customsOfficeFilter = (ModuleTextFilter)filter[filterName];

		customsOfficeFilter.Property = ZString.Empty;
		customsOfficeFilter.IsActive = true;

		CombineAssertions(() =>
		{
			var coll = new CusTempStorageJobHeaderCollection<CusTempStorageJobHeader>(Factory, GlbBranch.CurrentBranch);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(2, coll.Count);
			AssertEquals(header1.PK, coll[0].PK);
			AssertEquals(header2.PK, coll[1].PK);

			customsOfficeFilter.Property = "DEF";
			coll.AdditionalFilter = filter.Filter;
			coll.RefreshFromDb();
			AssertEquals(1, coll.Count);
			AssertEquals(header2.PK, coll[0].PK);
		});
	}

	CusTempStorageJobHeader GetNewTempStorageJobHeader()
	{
		var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
		header.SJH_GB = GlbBranch.CurrentBranch.PK;
		header.SJH_AppCode = "IST";
		return header;
	}

	Type[] ExpectedFilterInflatorTypes =>
	[
		typeof(JobNumberFilterInflator)
	];

	sealed class TemporaryStorageFilterStripBusinessObjectForTest : TemporaryStorageFilterStripBusinessObject
	{
		public List<IFilterInflator> GetFilterInflators_Exposed() => GetFilterInflators();
	}
}
