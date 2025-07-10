using System.Collections.Generic;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class CusEntryHeaderDocumentSupporter : Customs.Business.CusEntryHeaderDocumentSupporter
	{
		public CusEntryHeaderDocumentSupporter(CusEntryHeader header) : base(header) { }

		internal const string ApplicationAndCertificateDocument = ".ApplicationAndCertificateDocument";
		internal const string N5167 = ".N5167";
		public const string ExportCustomsDeclarationDocument = ".ExportCustomsDeclarationDocument";
		internal const string ImportCustomsDeclarationDocument = ".ImportCustomsDeclarationDocument";
		public const string ImportCustomsNonCondensedDeclarationDocument = ".ImportNonCondensedDeclaration";
		protected new CusEntryHeader EntryHeader => (CusEntryHeader)BusinessObject;

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(ApplicationAndCertificateDocument));
			result.Add(new DataContextValue(N5167));
			result.Add(new DataContextValue(ImportCustomsDeclarationDocument));
			result.Add(new DataContextValue(ExportCustomsDeclarationDocument));
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var result = new List<IBODocDataProvider>();
			if (dataContextValue.FullDataContext == ApplicationAndCertificateDocument)
			{
				foreach (var wrapper in EntryHeader.ApplicationAndCertificateDocumentWrappers)
				{
					result.Add(BODocDataProvider.Get(wrapper));
				}
			}
			else if (dataContextValue.FullDataContext == N5167)
			{
				result.Add(BODocDataProvider.Get(new N5167MessageSendingDocumentWrapper(EntryHeader, Factory)));
			}
			else if (dataContextValue.FullDataContext == ImportCustomsDeclarationDocument)
			{
				if (EntryHeader == null)
				{
					result.AddRange(System.Array.Empty<IBODocDataProvider>());
				}
				else
				{
					result.Add(BODocDataProvider.Get(new ImportCustomsDeclarationDocumentWrapper(EntryHeader, Factory)));
				}
			}
			else if (dataContextValue.FullDataContext == ExportCustomsDeclarationDocument)
			{
				result.Add(BODocDataProvider.Get(new ExportCustomsDeclarationDocumentWrapper(EntryHeader, Factory)));
			}
			return result.ToArray();
		}

		public override string GetFilterValue(DocumentFilters filterName)
		{
			switch (filterName)
			{
				case DocumentFilters.MSGBKRCTYMOD:
					return EntryHeader.Declaration.MessageTypeForDocumentFilter + Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(EntryHeader.CountryCode) + EntryHeader.Declaration.JE_TransportMode;

				case DocumentFilters.MSGBKRCTY:
					return EntryHeader.Declaration.MessageTypeForDocumentFilter + Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(EntryHeader.CountryCode);

				default:
					return base.GetFilterValue(filterName);
			}
		}
	}
}
