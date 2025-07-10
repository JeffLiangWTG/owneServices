using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(WarningAcknowledgementFilterBusinessObject))]
	sealed class WarningAcknowledgementFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestParentTableFilter()
		{
			var warning = Factory.NewWithValidTestData<GenCustomAddOnRuleAck>();
			warning.XK_ParentTableCode = "GS";
			Factory.Save();

			var filter = new WarningAcknowledgementFilterBusinessObjectForTest();
			var parentTableFilter = (ModuleTextFilter)filter["Parent Table Code"];
			parentTableFilter.IsActive = true;
			parentTableFilter.Property = "GS";

			var collection = Factory.Load<GenCustomAddOnRuleAck>(filter.Filter);
			AssertEquals("Too many records inside the collection", 1, collection.Length);
			AssertEquals("Returned value is incorrect", "GS", collection[0].XK_ParentTableCode);

			parentTableFilter.Property = "AA";
			collection = Factory.Load<GenCustomAddOnRuleAck>(filter.Filter);
			AssertEquals("Collection should be empty", 0, collection.Length);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new WarningAcknowledgementFilterBusinessObject();
		}

		sealed class WarningAcknowledgementFilterBusinessObjectForTest : WarningAcknowledgementFilterBusinessObject
		{
			public override CargoWise.EntityFramework.ZQuery Filter
			{
				get
				{
					var result = base.Filter;
					result.AddToFilter(GenCustomAddOnRuleAckSchema.XK_ParentTableCode, WarningForTest);
					return result;
				}
			}

			internal static string WarningForTest = "GS";
		}
	}
}
