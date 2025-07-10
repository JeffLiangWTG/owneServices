using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefDocSource)]
	public class RefDocSourceCollection : ActiveBusinessObjectCollection<RefDocSource>
	{
		public RefDocSourceCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefDocSourceCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}
