using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingSupplierPart))]
	sealed class TrackingSupplierPartBizOChangesEmailNotificationTest : BizOChangesEmailNotificationTest<TrackingSupplierPart>
	{
		protected override TrackingSupplierPart GetNewBizOForNotification()
		{
			var supplierPart = new TrackingSupplierPart(Factory, Factory.NewWithValidTestData<OrgSupplierPart>());
			supplierPart.HasChanges = true;
			return supplierPart;
		}
	}
}
