using CargoWise.EntityFramework;

using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.UNDGSubstance)]
	public class UNDGSubstanceCollection : ActiveBusinessObjectCollection<UNDGSubstance>
	{
		public UNDGSubstanceCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public UNDGSubstanceCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
