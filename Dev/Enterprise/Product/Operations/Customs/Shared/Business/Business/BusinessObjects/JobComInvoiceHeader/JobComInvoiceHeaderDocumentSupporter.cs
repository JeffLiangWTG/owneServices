using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.Business
{
	public class JobComInvoiceHeaderDocumentSupporter : DocumentSupporter
	{
		public JobComInvoiceHeaderDocumentSupporter(BaseJobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		public BaseJobComInvoiceHeader InvoiceHeader
		{
			get { return (BaseJobComInvoiceHeader)BusinessObject; }
		}

		#region Overrides

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[]
				{
					Constants.DataContext.JobComInvoiceHeader,
					Constants.DataContext.ComInvoiceHeader,
					Constants.DataContext.CommercialInvoice,
					Constants.DataContext.GenericCommercialInvoice
				};
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CommercialInvoice; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);

			if (result == null)
			{
				switch (dataContext)
				{
					case Constants.DataContext.JobComInvoiceHeader:
					case Constants.DataContext.ComInvoiceHeader:
					case Constants.DataContext.CommercialInvoice:
						result = new DocumentWrapper[] { DocumentWrapperFactory.CreateCustomsWrapper(Constants.DataContext.JobComInvoiceHeader, InvoiceHeader, InvoiceHeader.CountryCode) };
						break;
				}
			}

			return result;
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			switch (filterName)
			{
				case DocumentFilters.CTY:
					return InvoiceHeader.CountryCode;

				case DocumentFilters.BKRCTY:
					return Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(InvoiceHeader.CountryCode);

				case DocumentFilters.MSGBKRCTYAPP:
					BaseJobDeclaration declaration = InvoiceHeader.JobDeclaration;
					if (declaration != null)
					{
						return declaration.MessageTypeForDocumentFilter
							+ Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(InvoiceHeader.CountryCode)
							+ declaration.JE_ApplicationCode;
					}
					break;

				default:
					return base.GetFilterValue(filterName);
			}
			return ZString.Empty;
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.CustomsDeclarationCustomiseDocument; }
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.DataContext == Constants.DataContext.JobComInvoiceHeader ||
				dataContextValue.DataContext == Constants.DataContext.ComInvoiceHeader ||
				dataContextValue.DataContext == Constants.DataContext.CommercialInvoice ||
				dataContextValue.DataContext == Constants.DataContext.GenericCommercialInvoice)
			{
				return Res.GetString("874E844D-010A-41D3-A8AE-D6BA77F134A9", "Commercial invoice cannot be found.");
			}
			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		#endregion

	}
}
