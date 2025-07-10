using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Module.Testing;

[TestedType(typeof(EntryHeaderFilterBusinessObject))]
sealed class EntryHeaderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	public void TestLookups()
	{
		var filterBizObj = new EntryHeaderFilterBusinessObject();
		AssertType<EntryHeaderFilterLookups>("Lookups of correct type", filterBizObj.Lookups);
	}

	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryHeaderFilterBusinessObject();
}
