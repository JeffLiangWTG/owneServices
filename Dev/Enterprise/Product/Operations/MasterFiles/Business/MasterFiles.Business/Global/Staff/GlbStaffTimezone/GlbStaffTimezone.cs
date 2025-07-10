using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffTimezone : AutoGlbStaffTimezone
	{
		public GlbStaffTimezone(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(GSZ_AutoEffectiveEndDate), ConcurrencyPolicy.Ignore);
		}
	}
}
