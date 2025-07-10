using System;
using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.Business
{
	public class CusPackingListDocumentSupporter : DocumentSupporter
	{
		public CusPackingListDocumentSupporter(CusPackingList cusPackingList) : base(cusPackingList)
		{
		}

		protected CusPackingList PackingList => (CusPackingList)BusinessObject;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.CustomsDeclarationCustomiseDocument;

		protected override DataContext[] GetSupportedDataContexts() => new DataContext[] { DataContext.CusPackingList };

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun) => GetWrappers(PackingList);

		public override BusinessContext BusinessContext => BusinessContext.CusPackingList;

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun) => GetWrappers(PackingList);

		DocumentWrapper[] GetWrappers(CusPackingList packingList)
		{
			return Common.Shared.CustomsDocumentWrapperFinder.GetCustomsDocumentWrapperProvider(packingList.CountryCode)?.GetDocumentWrapper(packingList) is DocumentWrapper wrapper ? [wrapper] : Array.Empty<DocumentWrapper>();
		}
	}
}
