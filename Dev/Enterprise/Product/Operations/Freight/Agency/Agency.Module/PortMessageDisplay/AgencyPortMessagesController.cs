using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Agency.Module
{
	public class AgencyPortMessagesController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.AgencyPortMessaging; }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Freight.Agency.Module.Res.GetData("PlugInTabPage|AgencyPortMessaging", "Port Messages", "The Port Messages tab."); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			ZPlugIn result = null;
			JobVoyage voyage = businessEntity as JobVoyage;

			if (voyage != null && !voyage.IsDeleted && voyage.JV_AirSeaRoad == Constants.TransportModes.Sea)
			{
				result = new AgencyPortMessagesPlugin(voyage);
			}

			return result;
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.SailingSchedulePortMessaging; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.SailingSchedulePortMessaging; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.SailingSchedulePortMessaging; }
		}

		#endregion

		#region Not Supported

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("Not supported");
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		#endregion
	}
}
