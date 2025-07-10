using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.NZ.Business
{
	public class ConsolidatedDeclarationDocumentSupporter : Customs.Business.BaseConsolidatedDeclarationDocumentSupporter
	{
		public ConsolidatedDeclarationDocumentSupporter(ConsolidatedDeclaration consolidatedDeclaration) : base(consolidatedDeclaration)
		{
		}

		protected new JobDeclaration Declaration => (JobDeclaration)ConsolidatedDeclaration.BuildAggregateJobDeclaration();

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (dataContextValue.DataContext == DataContext.Declaration)
			{
				return Res.GetString("Enterprise.Customs.NZ.Business.ConsolidatedDeclarationDocumentSupporter|NotFoundDeclarationProviders", "Consolidated Declaration cannot be found.");
			}
			return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return base.GetSupportedDataContexts().Concat(new[] { Core.Constants.DataContext.Declaration }).ToArray();
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result;
			if (dataContext == DataContext.Declaration)
			{
				result = Declaration.DocumentSupporter.GetDocumentWrappers(dataContext, commandBeingRun);
			}
			else
			{
				result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, BusinessObject);
			}
			return result;
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			string result;
			if (filterName == DocumentFilters.MSGBKRCTY)
			{
				var declaration = Declaration;
				var countryCode = declaration.CountryCode;
				result = declaration.MessageTypeForDocumentFilter + Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);
			}
			else
			{
				result = base.GetFilterValue(filterName);
			}
			return result;
		}
	}
}
