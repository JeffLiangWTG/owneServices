using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(EntryLineFilterStripBusinessObject))]
	public class EntryLineFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EntryLineFilterStripBusinessObject();
		}

		public void TestFilterMaxLength_ForEntryLine()
		{
			var filterObject = GetNewFilterStripBusinessObject();
			var entryLineFilter = filterObject.ModuleFilters["Entry Line Number"];
			AssertEquals("EntryLine filter should contain MaxLength for EntryKey.", ModuleNumberFilter.MultiplyMaxLength(WhsInventoryViewSchema.WI_BondedEntryKey.MaxLength - 1), entryLineFilter.MaxLength);
		}
	}
}
