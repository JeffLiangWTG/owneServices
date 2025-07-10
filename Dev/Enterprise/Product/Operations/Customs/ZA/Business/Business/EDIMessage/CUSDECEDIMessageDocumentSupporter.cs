using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class CUSDECEDIMessageDocumentSupporter : Customs.Business.EDIMessageDocumentSupporter
	{
		public CUSDECEDIMessageDocumentSupporter(CUSDECEDIMessage message) : base(message) { }

		protected new CUSDECEDIMessage EdiMessage => (CUSDECEDIMessage)base.EdiMessage;

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(CusEntryHeaderDocumentSupporter.SADDocumentPack));
			result.Add(new DataContextValue(CusEntryHeaderDocumentSupporter.VOCDocumentPack));
			result.Add(new DataContextValue(CusEntryHeaderDocumentSupporter.VOCRefundDocument));
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var fullDataContext = dataContextValue.FullDataContext;
			switch (fullDataContext)
			{
				case CusEntryHeaderDocumentSupporter.SADDocumentPack:
					return new[] { BODocDataProvider.Get(SADDocumentPackDocumentWrapper.NewForMessage(EdiMessage)) };
				case CusEntryHeaderDocumentSupporter.VOCDocumentPack:
					return new[] { BODocDataProvider.Get(VOCDocumentPackDocumentWrapper.NewForMessage(EdiMessage)) };
				case CusEntryHeaderDocumentSupporter.VOCRefundDocument:
					return new[] { BODocDataProvider.Get(VOCRefundDocumentWrapper.NewForMessage(EdiMessage)) };
				default:
					return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
			}
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var fullDataContext = dataContextValue.FullDataContext;
			switch (fullDataContext)
			{
				case CusEntryHeaderDocumentSupporter.SADDocumentPack:
				case CusEntryHeaderDocumentSupporter.VOCDocumentPack:
				case CusEntryHeaderDocumentSupporter.VOCRefundDocument:
					return Res.GetString("80D1C0B3-873E-4215-8057-908D089B75D8", "The selected message is not linked to a proper Entry Header or the selected message body is not valid CUSDEC message.");
				default:
					return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
			}
		}

		public override MultilingualString GetCustomWatermarkText(IDocumentCommand documentCommand, IBODocDataProvider docDataProvider)
		{
			return DocumentWatermarkHelper.GetWatermarkText(documentCommand.SU_MenuName, docDataProvider);
		}
	}
}
