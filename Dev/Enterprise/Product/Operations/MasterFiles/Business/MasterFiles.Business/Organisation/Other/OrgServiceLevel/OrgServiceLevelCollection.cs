using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgServiceLevelCollection : ActiveBusinessObjectCollection<OrgServiceLevel>
	{
		public OrgServiceLevelCollection(BusinessObjectFactory factory, ZGuid orgHeaderPK) : base(factory, new ZQuery(Enterprise.ZArchitecture.Schema.OrgServiceLevelSchema.PM_OH, orgHeaderPK)) { }

		protected override bool AllowNew
		{
			get
			{
				return false;
			}
		}

		public OrgServiceLevel FindByRefServiceLevelCode(ZString code)
		{
			foreach (OrgServiceLevel level in this)
			{
				if (level.PM_RS_NKSrvLvl == code)
				{
					return level;
				}
			}
			return null;
		}
	}
}
