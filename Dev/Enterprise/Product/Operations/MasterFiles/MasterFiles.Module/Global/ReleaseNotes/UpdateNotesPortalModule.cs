using System;
using CargoWise.Application;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class UpdateNotesPortalModule : ZSimpleUrlLauncherModule
	{
		public override ModuleIdentifier ID => ModuleIDs.UpdateNotesPortal;

		public override Uri Url => null;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		public override void Show()
		{
			var helpProvider = ObjectFactory.Get<IHelpMenuProvider>();
			helpProvider.ShowWiseTechAcademy(ZHelpMenu.WiseTechAcademyContentAndSupportPath, ZHelpMenu.WiseTechAcademyUpdateNotesTarget);
		}
	}
}
