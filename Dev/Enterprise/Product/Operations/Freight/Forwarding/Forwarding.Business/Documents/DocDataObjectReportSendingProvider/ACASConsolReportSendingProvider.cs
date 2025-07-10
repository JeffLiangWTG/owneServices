using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ACASConsolReportSendingProvider : DocDataObjectReportSendingProvider
	{
		public ACASConsolReportSendingProvider(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override string DataContext => "ACASHouseChecklist";

		protected override ZGuid MenuItemPK => ConsolSystemFormMenuItems.DocumentMenuACASHouseChecklistUSPK;

		public override ModuleIdentifier ModuleIdentifier => ModuleIDs.JobConsol;
	}
}
