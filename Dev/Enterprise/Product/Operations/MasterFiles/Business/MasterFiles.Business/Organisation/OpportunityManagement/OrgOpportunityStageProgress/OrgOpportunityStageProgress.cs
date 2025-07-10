using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgOpportunityStageProgress : AutoOrgOpportunityStageProgress
	{
		public OrgOpportunityStageProgress(BusinessObjectFactory factory, DataRow row)
					: base(factory, row)
		{
		}

		public abstract new class Schema : AutoOrgOpportunityStageProgress.Schema
		{
		}

		public ZInt DaysInStage
		{
			get
			{
				if (OSP_DateStarted.IsEmpty)
				{
					return 0;
				}
				var toDateTimeOffset = OSP_DateCompleted == ZDateTimeOffset.Empty ? ZDateTimeOffset.Now : OSP_DateCompleted;
				return toDateTimeOffset.ToDateTime().Subtract(OSP_DateStarted.ToDateTime()).Days;
			}
		}
	}
}
