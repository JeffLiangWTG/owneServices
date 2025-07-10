using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.ProcessTemplates)]
	public class ActiveProcessTaskTemplateCollection : ActiveBusinessObjectCollection<ProcessTaskTemplate>
	{
		public ActiveProcessTaskTemplateCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public ActiveProcessTaskTemplateCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
