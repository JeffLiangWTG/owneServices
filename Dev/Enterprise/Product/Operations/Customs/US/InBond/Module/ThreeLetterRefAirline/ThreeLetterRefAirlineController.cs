using System;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.InBond.Module
{
	public class ThreeLetterRefAirlineController : RefAirlineController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(ThreeLetterRefAirline);

		public override ControllerID ID => ControllerIDs.Customs.US.ThreeLetterRefAirline;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.ThreeLetterRefAirline;
	}
}
