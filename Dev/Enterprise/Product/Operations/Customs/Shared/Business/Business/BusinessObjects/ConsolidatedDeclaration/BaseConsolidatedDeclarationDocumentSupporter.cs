using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.Business
{
	public class BaseConsolidatedDeclarationDocumentSupporter : DocumentSupporter
	{
		public BaseConsolidatedDeclarationDocumentSupporter(ConsolidatedDeclaration consolidatedDeclaration)
			: base(consolidatedDeclaration)
		{
		}

		protected ConsolidatedDeclaration ConsolidatedDeclaration => (ConsolidatedDeclaration)BusinessObject;

		protected BaseJobDeclaration Declaration => ConsolidatedDeclaration.BuildAggregateJobDeclaration();

		public override BusinessContext BusinessContext => BusinessContext.ConsolidatedEntry;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.CustomsDeclarationCustomiseDocument;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return new List<DocumentWrapper>().ToArray();
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[]
				{
					DataContext.CusEntryHeader
				};
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			switch (dataContextValue.DataContext)
			{
				case DataContext.CusEntryHeader:
					return Res.GetString("3C3438B7-7C88-4DC6-97BA-B53117508B83", "Entry Header cannot be found.");
			}
			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			return BaseJobDeclarationDocumentSupporterHelper.GetContactOrganisation(Declaration, contact);
		}
	}
}
