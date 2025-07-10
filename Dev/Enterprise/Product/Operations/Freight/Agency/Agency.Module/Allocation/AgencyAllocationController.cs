using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Agency.Module
{
	public class AgencyAllocationController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public AgencyAllocationController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.AgencyAllocation; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		protected override ZArchitecture.GUI.IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("Not supported");
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Freight.Agency.Module.Res.GetData("PlugInTabPage|AgencyAllocation", "Allocations", "The Allocations tab."); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			ZPlugIn result = null;
			JobVoyage voyage = businessEntity as JobVoyage;

			if (voyage != null && !voyage.IsDeleted && voyage.JV_AirSeaRoad == Constants.TransportModes.Sea)
			{
				result = new AgencyAllocationPlugin(voyage);
			}

			return result;
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		#region Security checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.SailingScheduleAllocationView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.SailingScheduleAllocationEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.SailingScheduleAllocationEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}


