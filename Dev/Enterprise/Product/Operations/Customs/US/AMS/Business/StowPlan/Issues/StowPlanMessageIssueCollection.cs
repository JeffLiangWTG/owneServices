using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Business
{
	public class StowPlanMessageIssueCollection : NonPersistentBusinessObjectCollection<StowPlanMessageIssue>
	{
		public StowPlanMessageIssue AddNew(ZGuid targetPK, ZString targetCode, ZString text, ZString detail, ZString moreDetail, CargoWise.ComponentModel.INotificationType notificationType)
		{
			var issue = new StowPlanMessageIssue(targetPK, targetCode, text, detail, moreDetail, notificationType);
			Add(issue);
			return issue;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}
	}
}
