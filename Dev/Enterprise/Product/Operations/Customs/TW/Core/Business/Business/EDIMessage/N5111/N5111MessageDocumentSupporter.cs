using System.Collections.Generic;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class N5111MessageDocumentSupporter : TWEDIMessageDocumentSupporter
	{
		public N5111MessageDocumentSupporter(TWMessage message) : base(message) { }

		public const string N5111MessagePair = ".N5111MessagePair";
		protected new N5111EDIMessage EdiMessage => (N5111EDIMessage)base.EdiMessage;

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(N5111MessagePair));
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			IBODocDataProvider wrapper;
			switch (dataContextValue.FullDataContext)
			{
				case N5111MessagePair:
					{
						wrapper = new N5111EDIMessageDocumentWrapper(EdiMessage);
						break;
					}
				default:
					return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
			}
			return wrapper != null ? new[] { wrapper } : null;
		}
	}
}
