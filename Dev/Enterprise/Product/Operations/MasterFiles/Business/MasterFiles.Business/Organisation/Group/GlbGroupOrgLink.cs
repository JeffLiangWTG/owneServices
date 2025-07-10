using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupOrgLink : AutoGlbGroupOrgLink
	{
		public GlbGroupOrgLink(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
