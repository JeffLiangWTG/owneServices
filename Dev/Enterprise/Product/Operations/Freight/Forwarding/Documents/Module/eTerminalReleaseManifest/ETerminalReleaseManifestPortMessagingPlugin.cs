using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Documents.GUI;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Forwarding.Documents.Module
{
	public class ETerminalReleaseManifestPortMessagingPlugin : ZPlugIn
	{
		public ETerminalReleaseManifestPortMessagingPlugin(JobVoyage voyage)
			: base(voyage)
		{
			this.voyage = voyage;
		}

		readonly JobVoyage voyage;

		public override string Name => Res.GetString("334244de-7d7c-4b6c-ba77-a30d9bbd544b", "Port Messages");

		protected override ZBool HasUserControl => false;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		protected override MenuItem GetNewTopLevelMenu()
		{
			var topLevelMenu = new ZMenuItem(ResString.GetMultilingualString("377be2fc-dddc-413e-a3f7-606db2a37390", "eTerminal Release Manifest"), (sender, args) =>
			{
				ZFormModaliser.ShowDialogWithoutDispose(new ETerminalReleaseMessageDialog(voyage));
			});

			return topLevelMenu;
		}
	}
}
