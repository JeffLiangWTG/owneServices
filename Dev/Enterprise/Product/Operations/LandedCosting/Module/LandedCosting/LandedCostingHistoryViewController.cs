using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.LandedCosting.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.LandedCosting.Module
{
	public class LandedCostingHistoryViewController : LandedCostingControllerBase
	{
		public LandedCostingHistoryViewController()
		{
		}

		public override ControllerID ID => ControllerIDs.LandedCostingHistoryView;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("Only for PlugIn");

		public override ResourceStringData PluginTabPageCaption => Res.GetData("PlugInTabPage|LandedCostingHistoryView", "Landed Cost History", "The Landed Cost History tab.");

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new LandedCostingHistoryViewPlugIn((ILandedCostHistoryMaster)businessEntity);
	}
}
