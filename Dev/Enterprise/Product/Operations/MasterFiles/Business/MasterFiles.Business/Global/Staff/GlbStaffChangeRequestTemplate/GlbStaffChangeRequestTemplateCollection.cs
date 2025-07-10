using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffChangeRequestTemplateCollection : ActiveBusinessObjectCollection<GlbStaffChangeRequestTemplate>
	{
		public GlbStaffChangeRequestTemplateCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
