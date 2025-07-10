using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using DataContext = Enterprise.Core.Constants.DataContext;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	class ReconDeclarationDocumentSupporter : DocumentSupporter
	{
		public ReconDeclarationDocumentSupporter(ReconDeclaration reconDeclaration)
			: base(reconDeclaration)
		{
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			IDocumentDeliveryContact result = base.GetContactOrganisation(menuName, contact, direction);
			if (contact == ContactType.Consignee)
			{
				result = new OrgHeaderContact(ReconDeclaration.ReconWrappedJobDeclaration.Importer, null);
			}

			return result;
		}

		ReconDeclaration ReconDeclaration
		{
			get { return (ReconDeclaration)BusinessObject; }
		}

		#region Overrides

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			List<DataContextValue> result = GetSupportedBODataSourcesFor(BusinessObject.GetType());
			result.Add(new DataContextValue(".ReconDeclaration"));
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			string menuName = commandBeingRun.SU_MenuName;

			switch (menuName)
			{
				case AggregateReconMenuName:
					ReconDeclaration.RefreshAggregatedEntries();
					break;

				case LineSummaryMenuItem:
					new ReconChangedLinesMerger(ReconDeclaration, ReconMergeContext.Documents).DoMerge(needCountDecreaseLine: true);
					break;
			}

			return new IBODocDataProvider[] { BODocDataProvider.Get(ReconDeclaration) };
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[]
				{
					DataContext.GenericFreightJob
				};
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.ReconDeclaration; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;

			if (dataContext == DataContext.GenericFreightJob)
			{
				result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);
			}
			else if (dataContext == DataContext.GenericFreightJobInvoice)
			{
				result = JobDeclarationDocumentSupporterHelper.GetWrappersForInvoice(commandBeingRun, true, ReconDeclaration.ReconWrappedJobDeclaration);
			}

			return result;
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.DataContext == DataContext.GenericFreightJob || dataContextValue.DataContext == DataContext.GenericFreightJobInvoice)
			{
				return Res.GetString("C44BF210-AA6A-49C8-AF45-0295B7DDD909", "Recon job cannot be found.");
			}
			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return (dataContext == DataContext.GenericFreightJob || dataContext == DataContext.GenericFreightJobInvoice) &&
				base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			string result = string.Empty;
			switch (filterName)
			{
				case DocumentFilters.CTY:
					result = Core.Constants.CountryCodes.UnitedStates;
					break;
				default:
					result = base.GetFilterValue(filterName);
					break;
			}

			return result;
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.CustomsDeclarationCustomiseDocument; }
		}

		internal const string AggregateReconMenuName = "Aggregate Recon";
		internal const string EntryByEntryReconMenuName = "Entry-By-Entry Recon Association File";
		internal const string LineSummaryMenuItem = "Line Summary Recon";

		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			DocumentSupporterDataState result = base.GetDataStateBeforeRun(commandAboutToBeRun);

			if (result.IsValid)
			{
				string menuName = commandAboutToBeRun.SU_MenuName;

				switch (menuName)
				{
					case AggregateReconMenuName:
						if (!ReconDeclaration.US_IsAggregate)
						{
							result = new DocumentSupporterDataState(false, IsNotAggregate);
						}
						break;

					case EntryByEntryReconMenuName:
						if (ReconDeclaration.IsNoChangeAggregate)
						{
							result = new DocumentSupporterDataState(false, NoChangeAggregate);
						}
						break;

					case LineSummaryMenuItem:

						string errors = null;
						if (ReconDeclaration.IsNoChangeAggregate)
						{
							errors = NoChangeAggregate;
						}
						else
						{
							errors = new ReconChangedLinesMerger(ReconDeclaration).GetErrors();
						}

						if (!string.IsNullOrEmpty(errors))
						{
							result = new DocumentSupporterDataState(false, errors);
						}
						break;
				}
			}

			return result;
		}

		internal const string IsNotAggregate = "The current recon job is not an aggregate reconciliation and therefore there is nothing to print for this document.";
		internal const string NoChangeAggregate = "The current recon job is a 'No-Change' aggregate reconciliation. You cannot print this document.";
		#endregion
	}
}
