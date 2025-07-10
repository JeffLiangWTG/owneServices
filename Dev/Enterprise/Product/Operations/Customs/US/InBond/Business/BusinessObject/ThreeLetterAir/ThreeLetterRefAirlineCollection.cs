using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.InBond.Business
{
	[ZArchitecture.ComponentModel.ModuleID(ModuleId.ThreeLetterRefAirline)]
	public class ThreeLetterRefAirlineCollection : ActiveBusinessObjectCollection<ThreeLetterRefAirline>
	{
		public ThreeLetterRefAirlineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ThreeLetterRefAirlineCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
