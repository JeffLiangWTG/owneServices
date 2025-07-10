using System;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public sealed class AssetManagementPortalModule : ZSimpleUrlLauncherModule
	{
		public override ModuleIdentifier ID => ModuleIDs.AssetManagementPortal;

		public override Uri Url => new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("AST");

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.AssetManagementPortal;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Accountant; // can be changed later
	}
}
