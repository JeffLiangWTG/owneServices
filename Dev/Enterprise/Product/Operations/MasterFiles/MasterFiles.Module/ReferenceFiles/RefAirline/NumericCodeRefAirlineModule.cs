using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class NumericCodeRefAirlineModule : RefAirlineModule
	{
		public override ModuleIdentifier ID => ModuleIDs.NumericCodeRefAirline;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.NumericCodeRefAirline);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new NumericCodeRefAirlineCollection(Factory);
		}
	}
}
