using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business
{
	public class StowPlanContainerDataValidation : AutoStowPlanContainerDataValidation
	{
		public StowPlanContainerDataValidation(AutoStowPlanContainerData parent)
			: base(parent)
		{ }

		public new StowPlanContainerData Parent
		{
			get { return (StowPlanContainerData)base.Parent; }
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			var provider = (IStowPlanNotificationProvider)Parent;
			if (!Parent.EquipmentNumber.IsEmpty && Parent.StockData == null)
			{
				Parent.AddRowNotification(new StowPlanNotification(RefContainerStockSchema.Constants.Prefix, provider.TargetSubject,
					NotificationType.MessageError, string.Format(NoContainerReferenceMsg, Parent.EquipmentNumber), NoContainerReferenceMsgDetail));
			}
			if (!Parent.HazardCodes.Any())
			{
				Parent.AddRowNotification(new StowPlanNotification(Parent, NotificationType.Warning, NoHazardCodes, NoHazardCodesDetail));
			}
		}
		public const string NoContainerReferenceMsg = "Container {0} does not exist in Container Manager.";
		const string NoContainerReferenceMsgDetail = "Container Owner that is required on the Stow Plan can be captured in Container Manager (Operations->Shipping->Container Manager).";
		public const string NoHazardCodes = "You may want to enter harzardous details.";
		const string NoHazardCodesDetail = "Hazardous details can be entered on Container -> Pack Lines -> DBSubtance.";

		protected override void CheckEquipmentNumber()
		{
			base.CheckEquipmentNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.EquipmentNumberInfo);
		}

		protected override void CheckStowPosition()
		{
			base.CheckStowPosition();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.StowPositionInfo);
		}

		protected override void CheckGrossWeightInKG()
		{
			base.CheckGrossWeightInKG();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.GrossWeightInKGInfo);
		}
	}
}
