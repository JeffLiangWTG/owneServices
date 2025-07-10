using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.EquipmentManagement.Module
{
	public sealed class EquipmentManagementPortalModule : ZSimpleUrlLauncherModule
	{
		public override ModuleIdentifier ID => ModuleIDs.EquipmentManagementPortal;

		public override Uri Url => new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("EQM");

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.LinerAndAgency;  // Same with OCS, can be changed later should specific is required for EQM 

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ShippingManager; // Same with OCS, can be changed later should specific is required for EQM 
	}
}
