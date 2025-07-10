using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ExternalRequestTypesFilterBusinessObject))]
	internal class ExternalRequestTypesFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ExternalRequestTypesFilterBusinessObject();

		public void TestCode()
		{
			PrepareTestData();

			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip["Code #"];
			filter.Property = ExternalRequestTypeJobTypes.Codes.ORD;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.IsActive = true;

			var collection = new ExternalRequestTypeCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("1 external request type", 1, collection.Count);
			AssertEquals(ExternalRequestTypeJobTypes.Codes.ORD, collection[0].RQT_Code);
		}

		public void TestDescription()
		{
			PrepareTestData();

			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip["Description"];
			filter.Property = ExternalRequestTypeJobTypes.Descriptions.SPL;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.IsActive = true;

			var collection = new ExternalRequestTypeCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("1 external request type", 1, collection.Count);
			AssertEquals(ExternalRequestTypeJobTypes.Codes.SPL, collection[0].RQT_Code);
		}

		public void TestJobType()
		{
			PrepareTestData();

			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip["Job Type"];
			filter.Property = ExternalRequestTypeJobTypes.Codes.ORD;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.IsActive = true;

			var collection = new ExternalRequestTypeCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("1 external request type", 1, collection.Count);
			AssertEquals(ExternalRequestTypeJobTypes.Codes.ORD, collection[0].RQT_Code);
		}

		void PrepareTestData()
		{
			new ExternalRequestTypeJobTypes().Cast<CodeDescriptionPair>().ForEach(pair =>
			{
				var obj = Factory.New<ExternalRequestType>();
				obj.RQT_Code = pair.Code;
				obj.RQT_Description = pair.Description;
				obj.RQT_JobType = pair.Code;
				obj.RQT_IsActive = true;
			});

			Factory.Save();
		}
	}
}
