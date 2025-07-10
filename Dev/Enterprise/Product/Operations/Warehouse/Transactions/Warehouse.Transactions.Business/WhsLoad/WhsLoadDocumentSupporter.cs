using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsLoadDocumentSupporter : DocumentSupporter
	{
		public WhsLoadDocumentSupporter(WhsLoad whsLoad)
			: base(whsLoad)
		{
		}

		protected WhsLoad Load => (WhsLoad)BusinessObject;

		public override BusinessContext BusinessContext => BusinessContext.WhsLoad;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.WhsLoadCustomizeDocuments;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			=> DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Load);

		protected override Constants.DataContext[] GetSupportedDataContexts()
			=> new Constants.DataContext[] { Constants.DataContext.GenericFreightJob };

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
			=> new OrgHeaderContact(Load.TransportCompany, null);

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun) => false;
	}
}
