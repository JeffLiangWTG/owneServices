using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Business
{
	public class CGNConsolReportSendingProvider : DocDataObjectReportSendingProvider
	{
		public CGNConsolReportSendingProvider(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override string DataContext => "CGNExportNotification";
		protected override ZGuid MenuItemPK => ConsolSystemFormMenuItems.DocumentMenuExportNotificationCargonautNLPK;
		public override ModuleIdentifier ModuleIdentifier => ModuleIDs.JobConsol;
	}
}
