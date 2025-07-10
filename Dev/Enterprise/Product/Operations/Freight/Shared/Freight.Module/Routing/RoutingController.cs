using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Module
{
	public class RoutingController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.Routing; }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Res.GetData("PlugInTabPage|Routing", "Routing", "The Routing tab."); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			var routingSupport = businessEntity as IRoutingSupport;

			RoutingPlugin plugin = null;
			if (routingSupport != null)
			{
				plugin = new RoutingPlugin(routingSupport);
			}
			else
			{
				var coreSupport = businessEntity as ITransportParentCore;
				if (coreSupport != null)
				{
					plugin = new RoutingPlugin(coreSupport);
				}
			}

			return plugin;
		}

		#region Not supported

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("Not supported");
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		#endregion

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
