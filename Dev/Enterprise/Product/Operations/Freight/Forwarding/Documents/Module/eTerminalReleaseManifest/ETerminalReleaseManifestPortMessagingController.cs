using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Forwarding.Documents.Module
{
	public class ETerminalReleaseManifestPortMessagingController : ZController
	{
		public override ControllerID ID => ControllerIDs.ETerminalReleaseManifestPortMessaging;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => throw new ModuleGuiNotSupportedException("Not supported");

		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("Not supported");

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.SailingSchedule;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.SailingSchedule;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.SailingSchedule;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.SailingSchedule;

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			ZPlugIn result = null;

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.China)
			{
				var voyage = businessEntity as JobVoyage;
				if (voyage != null && !voyage.IsDeleted && voyage.JV_AirSeaRoad == Constants.TransportModes.Sea)
				{
					result = new ETerminalReleaseManifestPortMessagingPlugin(voyage);
				}
			}

			return result;
		}
	}
}
