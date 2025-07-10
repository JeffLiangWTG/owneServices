using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbEmploymentHistory : AutoGlbEmploymentHistory
	{
		public GlbEmploymentHistory(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(GEH_AutoEffectiveEndDate), ConcurrencyPolicy.Ignore);
		}
	}
}
