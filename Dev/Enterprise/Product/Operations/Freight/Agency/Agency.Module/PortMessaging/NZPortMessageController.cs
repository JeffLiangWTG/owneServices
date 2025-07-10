using CargoWiseOne.ResourceStrings;

namespace Enterprise.Freight.Agency.Module
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.Core;
	using Enterprise.Environment;
	using Enterprise.Freight.Business;
	using Enterprise.Security;
	using Enterprise.ZArchitecture.GUI;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.PlugIn;

	public class NZPortMessageController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region ZController

		public override ControllerID ID
		{
			get { return ControllerIDs.AgencyNZPortMessaging; }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			ZPlugIn result = null;

			var voyage = businessEntity as JobVoyage;
			if (voyage != null && !voyage.IsDeleted && voyage.JV_AirSeaRoad == Constants.TransportModes.Sea)
			{
				result = new NZPortMessagePlugin(voyage);
			}

			return result;
		}

		public override ResourceStringData PluginTabPageCaption
		{
			get
			{
				return Res.GetData("82400846-b32d-11e4-9cbe-902b34dc814a", "Port Messages", "The Port Messages tab.");
			}
		}

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
