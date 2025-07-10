using CargoWise.EntityFramework;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.InBond.Module
{
	public class ThreeLetterRefAirlineModule : RefAirlineModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.US.ThreeLetterRefAirline;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.US.ThreeLetterRefAirline);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ThreeLetterRefAirlineCollection(Factory);
		}
	}
}
