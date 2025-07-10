using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	sealed class JobMawbUniqueIndexFailureHandler : IUniqueIndexFailureHandler
	{
		public JobMawbUniqueIndexFailureHandler(JobMawb mawb)
		{
			Argument.NotNull(mawb, "mawb");
			this.mawb = mawb;
		}

		readonly JobMawb mawb;

		BusinessObjectFactory Factory
		{
			get { return mawb.Factory; }
		}

		public IEnumerable<string> HandledUniqueIndexNames
		{
			get { yield return JobMawbSchema.Constants.Indexes.NR_UX__JM_ParentID; }
		}

		public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			ZQuery otherMawbQuery = new ZQuery(JobMawbSchema.JM_ParentID, mawb.JM_ParentID);
			otherMawbQuery.AddToFilter(JobMawbSchema.JM_ParentTableCode, mawb.JM_ParentTableCode);
			otherMawbQuery.AddToFilter(JobMawbSchema.PK, SQLComparisonOperator.NotEqual, mawb.PK);
			otherMawbQuery.FetchOnlyFromLocalCache = true;

			JobMawb otherMawb = Factory.LoadTop1<JobMawb>(otherMawbQuery);

			if (otherMawb != null)
			{
				otherMawb.Reload();
			}
			else
			{
				ZDBOnlyQuery otherMawbQueryDBQuery = new ZDBOnlyQuery(mawb.GetType());
				otherMawbQueryDBQuery.AddToFilter(otherMawbQuery);
				otherMawb = Factory.LoadTop1<JobMawb>(otherMawbQueryDBQuery);
			}

			IMAWBParent mawbParent = mawb.Parent;

			this.mawb.JM_ParentID = ZGuid.Empty;
			this.mawb.JM_ParentTableCode = ZString.Empty;
			mawbParent.MAWBAllocationParent.MAWBAllocation.SetAllocatedMAWBToParent(otherMawb);

			string message = Res.GetString("68c5f16c-0239-44af-b135-da1538cebdf0", @"The MAWB has been replaced due to changes made by another user. Master Bill {0}{1} has returned to unallocated MAWB stock.

Please review the changes to this {2}.", mawb.JM_Airline3DigitPrefix, mawb.JM_MAWB, ((BusinessObject)mawbParent).HumanReadableName);

			notifier.ReportInformation(message, Res.GetString("d1bfa6fe-a192-41bc-b4b9-fb00fdddcc9f", "Information"));
		}
	}
}
