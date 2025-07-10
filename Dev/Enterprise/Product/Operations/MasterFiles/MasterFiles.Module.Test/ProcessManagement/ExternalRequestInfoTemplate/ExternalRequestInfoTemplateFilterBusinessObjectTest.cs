using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ExternalRequestInfoTemplateFilterBusinessObject))]
	internal class ExternalRequestInfoTemplateFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ExternalRequestInfoTemplateFilterBusinessObject();

		public void TestCode()
		{
			PrepareTestData();

			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip["Code #"];
			filter.Property = ExternalRequestTypeJobTypes.Codes.ORD;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.IsActive = true;

			var collection = new ExternalRequestInfoTemplateCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("1 external request info template", 1, collection.Count);
			AssertEquals(ExternalRequestTypeJobTypes.Codes.ORD, collection[0].RIT_Code);
		}

		public void TestDescription()
		{
			PrepareTestData();

			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip["Description"];
			filter.Property = ExternalRequestTypeJobTypes.Descriptions.SPL;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.IsActive = true;

			var collection = new ExternalRequestInfoTemplateCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("1 external request info template", 1, collection.Count);
			AssertEquals(ExternalRequestTypeJobTypes.Codes.SPL, collection[0].RIT_Code);
		}

		public void TestJobType()
		{
			PrepareTestData();

			var filterStrip = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterStrip["Job Type"];
			filter.Property = ExternalRequestTypeJobTypes.Codes.ORD;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.IsActive = true;

			var collection = new ExternalRequestInfoTemplateCollection(Factory);
			collection.AdditionalFilter = filterStrip.Filter;

			AssertEquals("1 external request info template", 1, collection.Count);
			AssertEquals(ExternalRequestTypeJobTypes.Codes.ORD, collection[0].RIT_Code);
		}

		void PrepareTestData()
		{
			new ExternalRequestTypeJobTypes().Cast<CodeDescriptionPair>().ForEach(pair =>
			{
				var obj = Factory.New<ExternalRequestInfoTemplate>();
				obj.RIT_Code = pair.Code;
				obj.RIT_Description = pair.Description;
				obj.RIT_JobType = pair.Code;
				obj.RIT_IsActive = true;
			});

			Factory.Save();
		}
	}
}
