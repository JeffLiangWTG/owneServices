using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(Schema.GSG_Code)]
	[DescriptionProperty(Schema.GSG_TemplateName)]
	public class GlbStaffChangeRequestTemplate : AutoGlbStaffChangeRequestTemplate
	{
		public GlbStaffChangeRequestTemplate(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
