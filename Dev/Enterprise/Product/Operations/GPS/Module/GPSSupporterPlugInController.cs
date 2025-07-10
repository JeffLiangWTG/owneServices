using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.GPS.Module
{
	public class GPSSupporterPlugInController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public GPSSupporterPlugInController()
		{
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AllowVehicleMonitoringAndManagement; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AllowVehicleMonitoringAndManagement; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AllowVehicleMonitoringAndManagement; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AllowVehicleMonitoringAndManagement; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.GPSSupporter; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefEquipment); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("Not implemented"); //if you ever implement this don't forget to add corresponding security checkpoints
		}

		public override ResourceStringData PluginTabPageCaption { get { return Res.GetData("PlugInTabPage|GPSSupporter", "GPS", "The GPS tab."); } }

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new GPSSupporterPlugIn(businessEntity);
		}
	}
}
