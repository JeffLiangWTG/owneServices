
namespace Enterprise.Customs.US.AMS.Business
{
	public class StowPlanContainerStockDataValidation : AutoStowPlanContainerStockDataValidation
	{
		public StowPlanContainerStockDataValidation(AutoStowPlanContainerStockData parent)
			: base(parent)
		{ }

		protected override void CheckContainerOperator()
		{
			base.CheckContainerOperator();
			if (Parent.ContainerOperator.IsEmpty)
			{
				Parent.ContainerOperatorInfo.AddMessageError(MissingSCACMsg);
			}
		}
		public const string MissingSCACMsg = "Container Owner does not have Standard Carrier Alpha Code (SCAC).";
	}
}
