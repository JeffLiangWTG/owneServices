using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Business
{
	public class CCTConsolReportSendingProvider : DocDataObjectReportSendingProvider
	{
		public CCTConsolReportSendingProvider(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override string DataContext => "CCTHouseManifest";

		protected override ZGuid MenuItemPK => ConsolSystemFormMenuItems.CCTHouseManifestPK;

		public override ModuleIdentifier ModuleIdentifier => ModuleIDs.JobConsol;
	}
}
