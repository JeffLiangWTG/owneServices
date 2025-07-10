using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(PackageDimensionsInfo))]
	class PackageDimensionsInfoTestCase : DataObjectInfoTestCase<PackageDimensionsInfo>
	{
		#region TestPackageDimensionsInfo

		public void TestPackageDimensionsInfo()
		{
			var dimensionsInfo = new PackageDimensionsInfo();
			AssertEquals("Default dimensions are empty.", string.Empty, dimensionsInfo.PackType);
			AssertEquals("Default dimensions are empty.", 0m, dimensionsInfo.EmptyWeight);
			AssertEquals("Default dimensions are empty.", 0m, dimensionsInfo.Weight);
			AssertEquals("Default dimensions are empty.", string.Empty, dimensionsInfo.WeightUQ);
			AssertEquals("Default dimensions are empty.", 0m, dimensionsInfo.Length);
			AssertEquals("Default dimensions are empty.", 0m, dimensionsInfo.Width);
			AssertEquals("Default dimensions are empty.", 0m, dimensionsInfo.Height);
			AssertEquals("Default dimensions are empty.", string.Empty, dimensionsInfo.DimensionUQ);

			dimensionsInfo.PackType = "PLT";
			dimensionsInfo.EmptyWeight = 1m;
			dimensionsInfo.Weight = 2m;
			dimensionsInfo.WeightUQ = "KG";
			dimensionsInfo.Length = 4m;
			dimensionsInfo.Width = 6m;
			dimensionsInfo.Height = 8m;
			dimensionsInfo.DimensionUQ = "M";

			AssertEquals("Value is set.", "PLT", dimensionsInfo.PackType);
			AssertEquals("Value is set.", 1m, dimensionsInfo.EmptyWeight);
			AssertEquals("Value is set.", 2m, dimensionsInfo.Weight);
			AssertEquals("Value is set.", "KG", dimensionsInfo.WeightUQ);
			AssertEquals("Value is set.", 4m, dimensionsInfo.Length);
			AssertEquals("Value is set.", 6m, dimensionsInfo.Width);
			AssertEquals("Value is set.", 8m, dimensionsInfo.Height);
			AssertEquals("Value is set.", "M", dimensionsInfo.DimensionUQ);
		}

		#endregion

		#region Implementation

		protected new PackageDimensionsInfo Parent
		{
			get
			{
				return (PackageDimensionsInfo)base.Parent;
			}
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new PackageDimensionsInfo();
		}

		#endregion
	}
}
