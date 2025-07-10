using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupRole : AutoGlbGroupRole
	{
		public GlbGroupRole(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
