using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbEmploymentTeam : AutoGlbEmploymentTeam
	{
		public GlbEmploymentTeam(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(GET_AutoEffectiveEndDate), ConcurrencyPolicy.Ignore);
		}
	}
}
