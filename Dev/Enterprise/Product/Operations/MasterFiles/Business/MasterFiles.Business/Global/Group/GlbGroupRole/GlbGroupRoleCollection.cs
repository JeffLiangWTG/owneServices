using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupRoleCollection : ActiveBusinessObjectCollection<GlbGroupRole>
	{
		public GlbGroupRoleCollection(GlbGroup master)
			: base(master.Factory, master, null, GlbGroupRoleSchema.GGR_GG_Group)
		{
		}
	}
}
