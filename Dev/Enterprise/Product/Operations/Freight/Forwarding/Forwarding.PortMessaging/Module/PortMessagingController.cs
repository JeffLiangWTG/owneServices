using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.PortMessaging.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Forwarding.PortMessaging.Module
{
	public class PortMessagingController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.PortMessaging; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.MaintainConsol; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.PortMessaging; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.MaintainConsol; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.PortMessaging; }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new PortMessagingPlugIn(businessEntity);
		}

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Res.GetData("PlugInTabPage|PortMessagingManager", "Port Messaging", "The Port Messaging tab."); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("This is a plugin - no Form.");
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("Non persistent top level business object (ConsolPortMessagingManager)"); }
		}
	}
}
