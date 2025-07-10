using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business
{
	public class StowPlanSailingDataValidation : AutoStowPlanSailingDataValidation
	{
		public StowPlanSailingDataValidation(AutoStowPlanSailingData parent)
			: base(parent)
		{
		}

		public new StowPlanSailingData Parent
		{
			get { return (StowPlanSailingData)base.Parent; }
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			if (Parent.Shipments.Count == 0)
			{
				var provider = (IStowPlanNotificationProvider)Parent;
				Parent.AddRowNotification(new StowPlanNotification(JobShipmentSchema.Constants.Prefix, provider.TargetSubject, NotificationType.MessageError,
					string.Format(NoBillsMsg, Parent.Arrival), NoBillsDetail));
			}
		}
		public const string NoBillsMsg = "There are no Bills of Lading on the vessel at the time it arrives at {0}.";
		const string NoBillsDetail = "The Stow Plan reports all Bills of Lading on a vessel at the time of arrival in the selected port. Bills of Lading can be entered under Operations -> Shipping -> Bills of Lading.";

		protected override void CheckArrival()
		{
			base.CheckArrival();
			MandatoryValidation.CheckEntered(Parent.ArrivalInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ArrivalInfo);
		}

		protected override void CheckArrivalTime()
		{
			base.CheckArrivalTime();
			MandatoryValidation.CheckEntered(Parent.ArrivalTimeInfo);
		}

		protected override void CheckDeparture()
		{
			base.CheckDeparture();
			ListValidation.ErrorIfInvalidCode(Parent.DepartureInfo);
		}

		protected override void CheckIssueFilter()
		{
			base.CheckIssueFilter();
			MandatoryValidation.CheckEntered(Parent.IssueFilterInfo);
			ListValidation.ErrorIfInvalidCode(Parent.IssueFilterInfo);
		}
	}
}
