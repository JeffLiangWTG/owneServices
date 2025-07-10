using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAdjustmentLinePartAttributeValidationAdjustmentOut : PartAttributeValidation
	{
		public WhsAdjustmentLinePartAttributeValidationAdjustmentOut()
			: base()
		{
		}

		public WhsAdjustmentLinePartAttributeValidationAdjustmentOut(IPartAttributeValidationConsumer iPartAttributeValidationConsumer)
			: base(iPartAttributeValidationConsumer)
		{
		}

		protected override void AddNotification(ZPropertyInfo info, string errorMessage) => info.AddWarning(errorMessage);
	}
}
