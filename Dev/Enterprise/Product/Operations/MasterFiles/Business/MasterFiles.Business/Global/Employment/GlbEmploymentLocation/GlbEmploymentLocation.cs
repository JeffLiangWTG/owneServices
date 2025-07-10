using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbEmploymentLocation : AutoGlbEmploymentLocation
	{
		public GlbEmploymentLocation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(GEL_AutoEffectiveEndDate), ConcurrencyPolicy.Ignore);
		}
	}
}
