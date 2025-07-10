using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.AlternateGLAccounts)]
	public class AccAlternateGLAccountCollection : BusinessObjectCollection<AccAlternateGLAccount>
	{
		public AccAlternateGLAccountCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AccAlternateGLAccountCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
		: base(factory, additionalFilter)
		{
		}
	}
}
