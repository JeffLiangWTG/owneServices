using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.Business
{
	public class JobDeclarationDocumentSupporter : Customs.Business.BaseJobDeclarationDocumentSupporter
	{
		public const string ExportNonCondensedDeclarationDocument = ".ExportNonCondensedDeclaration";
		internal const string InvoicePackingWeightListDocument = JobComInvoiceHeaderDocumentSupporter.InvoicePackingWeightListDocument;
		internal const string GenericCommercialInvoice = JobComInvoiceHeaderDocumentSupporter.GenericCommercialInvoice;
		internal const string CusPackingList = "CusPackingList";
		internal const string LetterofAuthorizationPersonal = ".LetterofAuthorizationPersonal";

		public JobDeclarationDocumentSupporter(JobDeclaration declaration) : base(declaration) { }

		protected JobDeclaration Declaration => (JobDeclaration)BusinessObject;
		protected CusEntryHeader EntryHeader => Declaration?.CustomsEntryHeaders[0];

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(CusEntryHeaderDocumentSupporter.N5167));
			result.Add(new DataContextValue(N5110MessageDocumentSupporter.N5110MessagePair));
			result.Add(new DataContextValue(N5111MessageDocumentSupporter.N5111MessagePair));
			result.Add(new DataContextValue(LetterofAuthorizationPersonal));
			result.Add(new DataContextValue(CusEntryHeaderDocumentSupporter.ExportCustomsDeclarationDocument));
			result.Add(new DataContextValue(CusEntryHeaderDocumentSupporter.ImportCustomsDeclarationDocument));
			result.Add(new DataContextValue(ExportNonCondensedDeclarationDocument));
			result.Add(new DataContextValue(InvoicePackingWeightListDocument));
			result.Add(new DataContextValue(CusEntryHeaderDocumentSupporter.ImportCustomsNonCondensedDeclarationDocument));
			result.Add(new DataContextValue(CusEntryHeaderDocumentSupporter.ApplicationAndCertificateDocument));
			result.Add(new DataContextValue(GenericCommercialInvoice));
			result.Add(new DataContextValue(CusPackingList));
			result.Add(new DataContextValue(N5116MessageDocumentSupporter.N5116MessagePair));
			result.Add(new DataContextValue(N5204MessageDocumentSupporter.N5204MessagePair));
			return result;
		}

		IBODocDataProvider[] GetCustomsDeclarationDocumentBODocDataProviders(CusEntryHeader header, Func<CusEntryHeader, DocumentWrapper> selector)
		{
			var result = Array.Empty<IBODocDataProvider>();
			if (header != null)
			{
				var entryHeaders = header.Declaration?.DocumentSupporter?.JobDeclarationDocumentAddressConfig.Declarations.Select(x => x.EntryHeader) ?? Enumerable.Empty<CusEntryHeader>();
				if (!entryHeaders.Any())
				{
					entryHeaders = entryHeaders.Concat(new[] { header });
				}
				return entryHeaders.Select(x => BODocDataProvider.Get(selector(x))).ToArray();
			}
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			IBODocDataProvider[] getBODocDataProviderArray(BusinessObject a, DocumentWrapper b) => a == null ? Array.Empty<IBODocDataProvider>() : new IBODocDataProvider[] { BODocDataProvider.Get(b) };

			var entryHeader = EntryHeader;
			var declaration = Declaration;
			switch (dataContextValue.FullDataContext)
			{
				case CusEntryHeaderDocumentSupporter.N5167:
					return new IBODocDataProvider[] { BODocDataProvider.Get(new N5167MessageSendingDocumentWrapper(entryHeader, Factory)) };
				case N5110MessageDocumentSupporter.N5110MessagePair:
					var n5110message = entryHeader?.Messages.GetLastMessage(ApplicationCodeList.Codes.TWCustoms, MessageTypeList.Codes.TPC) as N5110EDIMessage;
					return n5110message == null ? Array.Empty<IBODocDataProvider>() : new IBODocDataProvider[] { BODocDataProvider.Get(new N5110EDIMessageDocumentWrapper(n5110message)) };
				case N5111MessageDocumentSupporter.N5111MessagePair:
					var n5111message = entryHeader?.Messages.GetLastMessage(ApplicationCodeList.Codes.TWCustoms, MessageTypeList.Codes.TAD) as N5111EDIMessage;
					return n5111message == null ? Array.Empty<IBODocDataProvider>() : new IBODocDataProvider[] { BODocDataProvider.Get(new N5111EDIMessageDocumentWrapper(n5111message)) };
				case LetterofAuthorizationPersonal:
					return getBODocDataProviderArray(declaration, new LetterofAuthorizationPersonalDocumentWrapper(declaration, Factory));
				case CusEntryHeaderDocumentSupporter.ExportCustomsDeclarationDocument:
					return GetCustomsDeclarationDocumentBODocDataProviders(entryHeader, x => new ExportCustomsDeclarationDocumentWrapper(x, Factory));
				case ExportNonCondensedDeclarationDocument:
					return getBODocDataProviderArray(entryHeader, new ExportNonCondensedDeclarationDocumentWrapper(entryHeader, Factory));
				case CusEntryHeaderDocumentSupporter.ImportCustomsDeclarationDocument:
					return GetCustomsDeclarationDocumentBODocDataProviders(entryHeader, x => new ImportCustomsDeclarationDocumentWrapper(x, Factory));
				case InvoicePackingWeightListDocument:
					return declaration?.Invoices.Cast<JobComInvoiceHeader>().Where(x => IsLinkedToPackage(x.JobComInvoiceLines.Cast<JobComInvoiceLine>())).Select(x => BODocDataProvider.Get(new InvoicePackingWeightListDocumentWrapper(x, Factory))).ToArray() ?? Array.Empty<IBODocDataProvider>();
				case CusEntryHeaderDocumentSupporter.ImportCustomsNonCondensedDeclarationDocument:
					return getBODocDataProviderArray(entryHeader, new ImportCustomsDeclarationNonCondensedDocumentWrapper(entryHeader, Factory));
				case GenericCommercialInvoice:
					return declaration?.Invoices.Cast<JobComInvoiceHeader>().Select(x => CommercialInvoiceWrapper.New(x, x.Factory)).ToArray() ?? Array.Empty<IBODocDataProvider>();
				case CusEntryHeaderDocumentSupporter.ApplicationAndCertificateDocument:
					return entryHeader?.ApplicationAndCertificateDocumentWrappers.Select(BODocDataProvider.Get).ToArray() ?? Array.Empty<IBODocDataProvider>();
				case N5116MessageDocumentSupporter.N5116MessagePair:
					var n5116message = (N5116EDIMessage)entryHeader?.Messages.GetLastMessage(ApplicationCodeList.Codes.TWCustoms, MessageTypeList.Codes.IRM);
					return n5116message == null ? Array.Empty<IBODocDataProvider>() : [BODocDataProvider.Get(new N5116EDIMessageDocumentWrapper(n5116message))];
				case N5204MessageDocumentSupporter.N5204MessagePair:
					var n5204message = (N5204EDIMessage)entryHeader?.Messages.GetLastMessage(ApplicationCodeList.Codes.TWCustoms, MessageTypeList.Codes.ERM);
					return n5204message == null ? Array.Empty<IBODocDataProvider>() : [BODocDataProvider.Get(new N5204EDIMessageDocumentWrapper(n5204message))];
				default:
					return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
			}
		}

		bool IsLinkedToPackage(IEnumerable<JobComInvoiceLine> lines) => lines?.Any(x => x.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().Any(linkPackage => linkPackage.IsLinked)) ?? false;

		public override string GetFilterValue(DocumentFilters filterName)
		{
			string result;
			var declaration = Declaration;
			switch (filterName)
			{
				case DocumentFilters.IsNonCondensedDeclaration:
					result = declaration.JE_MessageType + declaration.CountryCode + declaration.JE_MergeBy;
					break;
				case DocumentFilters.MSGGDSCTRY:
					var entryHeader = EntryHeader;
					if (entryHeader != null)
					{
						result = declaration.MessageTypeForDocumentFilter + entryHeader.GoodsTypeForDocumentFilter + entryHeader.CountryCode;
					}
					else
					{
						result = ZString.Empty;
					}
					break;
				default:
					result = base.GetFilterValue(filterName);
					break;
			}
			return result;
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var result = ZString.Empty;
			if (dataContextValue.FullDataContext == CusEntryHeaderDocumentSupporter.ImportCustomsNonCondensedDeclarationDocument
				|| dataContextValue.FullDataContext == CusEntryHeaderDocumentSupporter.ApplicationAndCertificateDocument)
			{
				if (EntryHeader == null)
				{
					result = Res.GetString("66ce36ad-7617-434b-a86f-af8e2a2473d8", "Entry Header cannot be found. Please select Brokerage > Generate Entries (Merge) to merge the declaration.");
				}
			}
			else if (dataContextValue.FullDataContext == InvoicePackingWeightListDocument)
			{
				result = Res.GetString("D95FACFB-35FD-4629-91D9-FB388517156E", "No Packing Weight List information.");
			}
			else
			{
				result = base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
			}
			return result;
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == DataContext.CusPackingList)
			{
				var packingList = Declaration.LoadCusPackingList(Factory) as CusPackingList;
				return packingList == null ? Array.Empty<DocumentWrapper>() : new DocumentWrapper[] { DocCusPackingList.New(packingList, packingList.Factory) };
			}
			else
			{
				var result = base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
				if (EntryHeader != null && commandBeingRun != null)
				{
					DocumentWrapper newWrapper = null;
					switch (commandBeingRun.SU_MenuName)
					{
						case DocumentName.ImportCustomsDeclarationEnglish:
						case DocumentName.ImportCustomsDeclarationFormal:
						case DocumentName.ImportCustomsDeclarationInformal:
						case DocumentName.ImportCustomsDeclarationProof:
							newWrapper = ImportCustomsDeclarationDocumentWrapper.New(EntryHeader, Factory);
							break;
						case DocumentName.ImportCustomsDeclarationNCDFormal:
						case DocumentName.ImportCustomsDeclarationNCDInformal:
							newWrapper = ImportCustomsDeclarationNonCondensedDocumentWrapper.New(EntryHeader, Factory);
							break;
						case DocumentName.ExportCustomsDeclarationEnglish:
						case DocumentName.ExportCustomsDeclarationFormal:
						case DocumentName.ExportCustomsDeclarationInformal:
						case DocumentName.ExportCustomsDeclarationProof:
							newWrapper = ExportCustomsDeclarationDocumentWrapper.New(EntryHeader, Factory);
							break;
						case DocumentName.ExportCustomsDeclarationNCDFormal:
						case DocumentName.ExportCustomsDeclarationNCDInformal:
							newWrapper = ExportNonCondensedDeclarationDocumentWrapper.New(EntryHeader, Factory);
							break;
					}

					if (newWrapper != null)
					{
						result = result == null ? [newWrapper] : result.Append(newWrapper).ToArray();
					}
				}
				return result;
			}
		}

		protected override DocumentSupporterDataState GetDataStateBeforeRunCore(IStmMenuItem commandAboutToBeRun)
		{
			CurrentCommand = commandAboutToBeRun;

			using (SuspendSettingCurrentCommand())
			{
				var documentSupporterDataState = base.GetDataStateBeforeRunCore(commandAboutToBeRun);
				if (documentSupporterDataState.IsValid && commandAboutToBeRun != null)
				{
					var documentOptionsToPrint = ObjectFactory.Get<Integration.Customs.TW.IDocumentOptionsToPrint>();
					documentSupporterDataState.IsValid = documentOptionsToPrint.GetDocumentOptionsToPrintCustomsDeclaration(Declaration, commandAboutToBeRun.Documents);
				}
				return documentSupporterDataState;
			}
		}

		#region CurrentCommand

		public IDisposable SuspendSettingCurrentCommand()
		{
			return new DisposableAction(() => suspendSettingCurrentCommand++, () => suspendSettingCurrentCommand--);
		}

		public ZBool IsSettingCurrentCommandSuspended => suspendSettingCurrentCommand > 0;

		public IStmMenuItem CurrentCommand
		{
			get
			{
				return currentCommand;
			}
			set
			{
				if (!IsSettingCurrentCommandSuspended)
				{
					currentCommand = value;
				}
			}
		}

		int suspendSettingCurrentCommand;

		IStmMenuItem currentCommand;

		#endregion

		public JobDeclarationDocumentAddressConfig JobDeclarationDocumentAddressConfig => jobDeclarationDocumentAddressConfig ??= new JobDeclarationDocumentAddressConfig(Declaration);
		JobDeclarationDocumentAddressConfig jobDeclarationDocumentAddressConfig;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Document names")]
		public static class DocumentName
		{
			public const string ImportCustomsDeclarationEnglish = "Import Customs Declaration (English)";
			public const string ImportCustomsDeclarationFormal = "Import Customs Declaration (Formal)";
			public const string ImportCustomsDeclarationInformal = "Import Customs Declaration (Informal)";
			public const string ImportCustomsDeclarationProof = "Import Customs Declaration (Proof)";
			public const string ImportCustomsDeclarationNCDFormal = "Import Customs Declaration (NCD Formal)";
			public const string ImportCustomsDeclarationNCDInformal = "Import Customs Declaration (NCD Informal)";
			public const string ExportCustomsDeclarationEnglish = "Export Customs Declaration (English)";
			public const string ExportCustomsDeclarationFormal = "Export Customs Declaration (Formal)";
			public const string ExportCustomsDeclarationInformal = "Export Customs Declaration (Informal)";
			public const string ExportCustomsDeclarationProof = "Export Customs Declaration (Proof)";
			public const string ExportCustomsDeclarationNCDFormal = "Export Customs Declaration (NCD Formal)";
			public const string ExportCustomsDeclarationNCDInformal = "Export Customs Declaration (NCD Informal)";
		}
	}
}
