using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Business
{
	public class CusEntryHeaderDocumentSupporter : DocumentSupporter
	{
		public CusEntryHeaderDocumentSupporter(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		protected CusEntryHeader EntryHeader
		{
			get { return (CusEntryHeader)BusinessObject; }
		}

		#region Overrides

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[]
				{
					Enterprise.Core.Constants.DataContext.CusEntryHeader
				};
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CusEntryHeader; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return new DocumentWrapper[] { DocumentWrapperFactory.CreateCustomsWrapper(Core.Constants.DataContext.CusEntryHeader, EntryHeader, EntryHeader.CountryCode) };
		}

		public override string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider docWrapperForCurrentPivot)
		{
			switch (filterType)
			{
				case MenuTemplateFilterType.BOE:
					ZString filterValue = EntryHeader.CountryCode;
					if (EntryHeader.Declaration != null)
					{
						filterValue += EntryHeader.Declaration.JE_MessageType;
					}
					return filterValue;

				case MenuTemplateFilterType.MULTISUPPLIER:
					if (EntryHeader.Declaration != null &&
						EntryHeader.Declaration.JE_MessageType == JobMessageTypeList.Codes.Import)
					{
						if (EntryHeader.Suppliers.Count > 1)
						{
							return (NoResString)"Yes";
						}
					}
					return (NoResString)"No";
			}
			return ZString.Empty;
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			switch (filterName)
			{
				case DocumentFilters.CTY:
					return EntryHeader.CountryCode;

				case DocumentFilters.BKRCTY:
					return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(EntryHeader.CountryCode);

				case DocumentFilters.MSGBKRCTYAPP:
					return EntryHeader.Declaration.MessageTypeForDocumentFilter + Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(EntryHeader.CountryCode) + EntryHeader.Declaration.JE_ApplicationCode;

				case DocumentFilters.MSGENTCTRYAPP:
					return EntryHeader.MessageTypeForDocumentFilter + Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(EntryHeader.CountryCode) + EntryHeader.Declaration.JE_ApplicationCode;

				case DocumentFilters.MSGGDSCTRY:
					return EntryHeader.Declaration.MessageTypeForDocumentFilter + EntryHeader.GoodsTypeForDocumentFilter + EntryHeader.CountryCode;

				case DocumentFilters.MSGBKRCTY:
					return EntryHeader.Declaration.MessageTypeForDocumentFilter + Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(EntryHeader.CountryCode);

				case DocumentFilters.ASYCUDA:
					return ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry(EntryHeader.CountryCode) ? "Y" : "N";

				default:
					return base.GetFilterValue(filterName);
			}
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.CustomsDeclarationCustomiseDocument; }
		}

		public override bool StorageDocsAreEditableIfInRelated
		{
			get { return true; }
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.DataContext == Enterprise.Core.Constants.DataContext.CusEntryHeader)
			{
				return Res.GetString("5E6A5281-DEC8-4337-ADF5-53853F1295A2", "Entry Header cannot be found.");
			}
			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			return BaseJobDeclarationDocumentSupporterHelper.GetContactOrganisation(EntryHeader.Declaration, contact);
		}

		#endregion
	}
}
