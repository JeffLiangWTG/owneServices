using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Agency.Module
{
	public class DangerousGoodsManifestController : ZController
	{
		public override ControllerID ID => ControllerIDs.AgencyDangerousGoodsManifest;

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			ZPlugIn result = null;

			var voyage = businessEntity as JobVoyage;
			if (voyage != null && !voyage.IsDeleted && voyage.JV_AirSeaRoad == Constants.TransportModes.Sea)
			{
				result = new DangerousGoodsManifestPlugin(voyage);
			}

			return result;
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.SailingSchedulePortMessaging;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.SailingSchedulePortMessaging;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.SailingSchedulePortMessaging;

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
