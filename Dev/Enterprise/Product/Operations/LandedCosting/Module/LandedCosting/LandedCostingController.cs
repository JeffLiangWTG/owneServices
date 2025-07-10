using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.LandedCosting.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.LandedCosting.Module
{
	public class LandedCostingController : LandedCostingControllerBase
	{
		public LandedCostingController()
		{
		}

		public override ControllerID ID => ControllerIDs.LandedCosting;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override ResourceStringData PluginTabPageCaption => Res.GetData("PlugInTabPage|LandedCosting", "Landed Costing");

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new LandedCostingPlugIn(((ILandedCostHeaderProvider)businessEntity).LCHeaderHost);
	}
}
