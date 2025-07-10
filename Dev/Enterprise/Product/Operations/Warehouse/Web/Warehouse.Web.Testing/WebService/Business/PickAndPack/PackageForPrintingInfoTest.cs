using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(PackageForPrintingInfo))]
	class PackageForPrintingInfoTest : DataObjectInfoTestCase<PackageForPrintingInfo>
	{
		#region TestSupportsCarrierLabelIntegration

		public void TestSupportsCarrierLabelIntegration()
		{
			var info = new PackageForPrintingInfo();
			AssertEquals(false, info.SupportsCarrierLabelIntegration);

			info.SupportsCarrierLabelIntegration = true;
			AssertEquals(true, info.SupportsCarrierLabelIntegration);
		}

		#endregion

		#region Implementation

		protected override DataObjectInfo GetNewObjectInfo() => new PackageForPrintingInfo();

		#endregion
	}
}
