using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsAdjustmentLinePartAttributeValidationAdjustmentOutTest : PartAttributeValidationTest
	{
		protected override bool CheckAttributeWhenNotEmpty(ZPropertyInfo info) => true;

		protected override void AssertNoPartAttributeNotifications(string message, ZPropertyInfo info) => AssertNoWarnings(message, info);
		protected override void AssertHasPartAttributeNotification(string message, ZPropertyInfo info, string notificationExpectedToBeFound) => AssertHasWarning(message, info, notificationExpectedToBeFound);
		protected override void AssertHasPartAttributeNotifications(string message, ZPropertyInfo info) => AssertHasWarnings(message, info);

		protected override MasterFiles.Business.PartAttributeValidation GetNewValidationCore() => new WhsAdjustmentLinePartAttributeValidationAdjustmentOut();
	}
}
