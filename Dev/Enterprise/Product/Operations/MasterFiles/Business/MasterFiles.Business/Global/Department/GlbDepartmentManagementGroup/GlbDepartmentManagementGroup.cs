using System.Data;
using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("New table for cost centre.")]
	public class GlbDepartmentManagementGroup : AutoGlbDepartmentManagementGroup
	{
		public GlbDepartmentManagementGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
