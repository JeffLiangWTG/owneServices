using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.AMS.Business
{
	public class StowPlanShipmentDataValidation : AutoStowPlanShipmentDataValidation
	{
		public StowPlanShipmentDataValidation(AutoStowPlanShipmentData parent)
			: base(parent)
		{ }

		public new StowPlanShipmentData Parent
		{
			get { return (StowPlanShipmentData)base.Parent; }
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			if (Parent.Containers.Count == 0)
			{
				Parent.AddRowNotification(new StowPlanNotification(Parent, NotificationType.MessageError, NoContainersMsg, NoContainersDetail));
			}
		}
		internal const string NoContainersMsg = "A containerized Bill of Lading must have at least one container.";
		const string NoContainersDetail = "Containers can be captured on Containers Tab of a Bill of Lading (Operation -> Shipping -> Bills of Lading).";

		protected override void CheckPortOfDischarge()
		{
			base.CheckPortOfDischarge();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.PortOfDischargeInfo);
		}

		protected override void CheckPortOfLading()
		{
			base.CheckPortOfLading();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.PortOfLadingInfo);
		}
	}
}
