using System.Collections.Generic;
using CargoWise.Definitions;
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

namespace Enterprise.Customs.US.Business.Protest
{
	class ProtestDocumentSupporter : DocumentSupporter
	{
		public ProtestDocumentSupporter(Protest protest)
			: base(protest)
		{
		}

		protected Protest Protest
		{
			get { return (Protest)BusinessObject; }
		}

		internal const string ProtestMenuName = "Protest";

		#region Overrides

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			List<DataContextValue> result = GetSupportedBODataSourcesFor(typeof(Protest));
			result.Add(new DataContextValue(".Protest"));
			return result;
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			IDocumentDeliveryContact result = null;
			if (contact == ContactType.Consignee && !Protest.Protestant.E2_AddressOverride)
			{
				result = new OrgHeaderContact(Protest.Protestant.Organisation, Protest.Protestant.Organisation, null);
			}

			return result;
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Protest; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;

			if (dataContext == DataContext.UsProtest)
			{
				result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Protest);
			}
			else if (dataContext == DataContext.GenericFreightJobInvoice)
			{
				result = JobDeclarationDocumentSupporterHelper.GetWrappersForInvoice(commandBeingRun, true, Protest.Declaration);
			}

			return result;
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[]
			{
				DataContext.GenericFreightJob,
				DataContext.UsProtest
			};
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			switch (filterName)
			{
				case DocumentFilters.CTY:
					return Core.Constants.CountryCodes.UnitedStates;

				default:
					return base.GetFilterValue(filterName);
			}
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.CustomsDeclarationCustomiseDocument; }
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.DataContext == DataContext.GenericFreightJob
				|| dataContextValue.DataContext == DataContext.GenericFreightJobInvoice
				|| dataContextValue.DataContext == DataContext.UsProtest)
			{
				return Res.GetString("F1304BCE-770C-49A3-9921-4E27A4841D14", "Protest declaration cannot be found.");
			}
			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return (dataContext == DataContext.GenericFreightJob
				|| dataContext == DataContext.GenericFreightJobInvoice
				|| dataContext == DataContext.UsProtest) &&
				base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}
		#endregion
	}
}
