using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupOrgContactLink : AutoGlbGroupOrgContactLink
	{
		public GlbGroupOrgContactLink(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
