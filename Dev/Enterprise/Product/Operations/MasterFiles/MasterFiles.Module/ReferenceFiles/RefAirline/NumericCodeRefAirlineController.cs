using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class NumericCodeRefAirlineController : RefAirlineController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(NumericCodeRefAirline);

		public override ControllerID ID => ControllerIDs.NumericCodeRefAirline;

		public override ModuleIdentifier ModuleID => ModuleIDs.NumericCodeRefAirline;
	}
}
