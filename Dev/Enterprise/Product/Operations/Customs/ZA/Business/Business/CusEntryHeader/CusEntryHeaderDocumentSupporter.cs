using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ZA.Business
{
	public class CusEntryHeaderDocumentSupporter : Customs.Business.CusEntryHeaderDocumentSupporter
	{
		public CusEntryHeaderDocumentSupporter(CusEntryHeader header) : base(header) { }

		#region Const

		internal const string CUSDECCUSRESMessagePair = ".CUSDECCUSRESMessagePair";
		internal const string SADDocumentPack = ".SADDocumentPack";
		internal const string VOCDocumentPack = ".VOCDocumentPack";
		internal const string DA63Document = ".DA63Document";
		internal const string DA74Document = ".DA74Document";
		internal const string VOCRefundDocument = ".VOCRefundDocument";
		internal const string RefundWorkSheet = ".RefundWorkSheet";
		internal const string RefundControlSheetDocument = ".RefundControlSheetDocument";

		#endregion

		protected new CusEntryHeader EntryHeader => (CusEntryHeader)BusinessObject;

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(CUSDECCUSRESMessagePair));
			result.Add(new DataContextValue(SADDocumentPack));
			result.Add(new DataContextValue(VOCDocumentPack));
			result.Add(new DataContextValue(DA63Document));
			result.Add(new DataContextValue(DA74Document));
			result.Add(new DataContextValue(VOCRefundDocument));
			result.Add(new DataContextValue(RefundWorkSheet));
			result.Add(new DataContextValue(RefundControlSheetDocument));
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			IBODocDataProvider[] result = null;
			if (dataContextValue.FullDataContext == CUSDECCUSRESMessagePair)
			{
				result = new IBODocDataProvider[] { BODocDataProvider.Get(new CUSDECCUSRESMessagePairDocumentWrapper(EntryHeader)) };
			}
			else if (dataContextValue.FullDataContext == SADDocumentPack)
			{
				result = new IBODocDataProvider[] { BODocDataProvider.Get(new SADDocumentPackDocumentWrapper(EntryHeader)) };
			}
			else if (dataContextValue.FullDataContext == VOCDocumentPack)
			{
				result = new IBODocDataProvider[] { BODocDataProvider.Get(new VOCDocumentPackDocumentWrapper(EntryHeader)) };
			}
			else if (dataContextValue.FullDataContext == DA63Document)
			{
				var docwrapper = new DA63DocumentWrapper(EntryHeader);
				if (docwrapper.HasDA63EntryLines)
				{
					result = new IBODocDataProvider[] { BODocDataProvider.Get(docwrapper) };
				}
			}
			else if (dataContextValue.FullDataContext == DA74Document)
			{
				result = new IBODocDataProvider[] { BODocDataProvider.Get(new DA74DocumentWrapper(EntryHeader)) };
			}
			else if (dataContextValue.FullDataContext == VOCRefundDocument)
			{
				result = new IBODocDataProvider[] { BODocDataProvider.Get(new VOCRefundDocumentWrapper(EntryHeader)) };
			}
			else if (dataContextValue.FullDataContext == RefundWorkSheet)
			{
				result = new IBODocDataProvider[] { BODocDataProvider.Get(new RefundWorkSheetDocumentWrapper(EntryHeader)) };
			}
			else if (dataContextValue.FullDataContext == RefundControlSheetDocument)
			{
				var docwrapper = new RefundControlSheetDocumentWrapper(EntryHeader);
				if (docwrapper.HasRefundLines)
				{
					result = new IBODocDataProvider[] { BODocDataProvider.Get(docwrapper) };
				}
			}
			else
			{
				result = base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
			}
			return result ?? System.Array.Empty<IBODocDataProvider>();
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var result = ZString.Empty;
			if (dataContextValue.FullDataContext == DA63Document)
			{
				result = Res.GetString("70E0F588-C328-4B9C-9D97-B7AA22B38DA6", "Entry Header {0} doesn't contain any DA63 Entry Line", EntryHeader.CH_BGMReference);
			}
			else if (dataContextValue.FullDataContext == RefundControlSheetDocument)
			{
				result = Res.GetString("B3BC7E78-1658-4A3E-AE24-52D4265856E3", "Entry Header {0} doesn't contain any Refund Entry Lines", EntryHeader.CH_BGMReference);
			}
			else if (dataContextValue.FullDataContext == CUSDECCUSRESMessagePair
				|| dataContextValue.FullDataContext == SADDocumentPack
				|| dataContextValue.FullDataContext == VOCDocumentPack
				|| dataContextValue.FullDataContext == DA74Document
				|| dataContextValue.FullDataContext == VOCRefundDocument
				|| dataContextValue.FullDataContext == RefundWorkSheet)
			{
				result = Res.GetString("716626ED-5F61-4E03-8F01-BF2F78B124F2", "Entry Header cannot be found.", EntryHeader.CH_BGMReference);
			}
			else
			{
				result = base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
			}
			return result;
		}

		public override MultilingualString GetCustomWatermarkText(IDocumentCommand documentCommand, IBODocDataProvider docDataProvider)
		{
			return DocumentWatermarkHelper.GetWatermarkText(documentCommand.SU_MenuName, docDataProvider);
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return [DocCusEntryHeader.New(EntryHeader, Factory)];
		}
	}
}
