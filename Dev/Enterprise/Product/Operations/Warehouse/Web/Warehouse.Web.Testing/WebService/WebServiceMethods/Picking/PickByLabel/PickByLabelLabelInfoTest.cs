using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(PickByLabelLabelInfo))]
	public class PickByLableLableInfoTestCase : DataObjectInfoTestCase<PickByLabelLabelInfo>
	{
		#region TestPickByLableLableInfo_PackageID

		public void TestPickByLableLableInfo_PackageID()
		{
			var pickByLabelLabelInfo = new PickByLabelLabelInfo("PACKAGE-1");
			AssertEquals("PACKAGE-1", pickByLabelLabelInfo.PackageID);
		}

		#endregion

		#region Implementation

		protected override DataObjectInfo GetNewObjectInfo() => new PickByLabelLabelInfo();

		#endregion
	}
}
