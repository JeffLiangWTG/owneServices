using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.AMS.Business
{
	public class StowPlanMessageIssueCollectionView : BusinessObjectCollectionView<StowPlanMessageIssue>
	{
		public StowPlanMessageIssueCollectionView(StowPlanMessageIssueCollection issuesCollection, StowPlanSailingData sailingData)
			: base(issuesCollection)
		{
			this.sailingData = sailingData;
		}
		readonly StowPlanSailingData sailingData;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var issue = (StowPlanMessageIssue)element;
			return sailingData != null && (sailingData.IssueFilter == STWIssueFilterList.Codes.ShowAll ||
				(sailingData.IssueFilter == STWIssueFilterList.Codes.WarningOnly && issue.notificationType == NotificationType.Warning) ||
				(sailingData.IssueFilter == STWIssueFilterList.Codes.MessageErrorOnly && issue.notificationType == NotificationType.MessageError));
		}
	}
}
