using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.ZA.Business
{
	public class CUSRESEDIMessageDocumentSupporter : Customs.Business.EDIMessageDocumentSupporter
	{
		public CUSRESEDIMessageDocumentSupporter(CUSRESEDIMessage message) : base(message) { }

		protected new CUSRESEDIMessage EdiMessage => (CUSRESEDIMessage)base.EdiMessage;

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(CusEntryHeaderDocumentSupporter.CUSDECCUSRESMessagePair));
			result.Add(new DataContextValue(CUSCAR_CUSRES));
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			IBODocDataProvider wrapper = null;
			switch (dataContextValue.FullDataContext)
			{
				case CusEntryHeaderDocumentSupporter.CUSDECCUSRESMessagePair:
					{
						wrapper = CUSDECCUSRESMessagePairDocumentWrapper.NewForMessage(EdiMessage);
						break;
					}
				case CUSCAR_CUSRES:
					{
						var header = EdiMessage?.EM_LinkedObject as ManifestBase.AsycudaManifestHeader ?? (EdiMessage?.EM_LinkedObject as ManifestBase.AsycudaBill)?.Header;
						if (header != null)
						{
							wrapper = DocumentWrapperFactory.CreateWrapperWithoutException("Enterprise.Customs.ZA.Manifest.Business.CUSCAR_CUSRESEDIMessageDocumentWrapper, Enterprise.Customs.ZA.Manifest.Business", EdiMessage);
						}
						break;
					}
				default:
					return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
			}
			return wrapper != null ? new[] { wrapper } : null;
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var fullDataContext = dataContextValue.FullDataContext;
			switch (fullDataContext)
			{
				case CusEntryHeaderDocumentSupporter.CUSDECCUSRESMessagePair:
					return Res.GetString("C2998B97-435E-4777-B612-71FA622DC4BF", "The selected message is not linked to a proper Entry Header.");
				default:
					return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
			}
		}

		internal const string CUSCAR_CUSRES = ".CUSCAR_CUSRES";
	}
}
