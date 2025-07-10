using System.Collections.Generic;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class N5110MessageDocumentSupporter : TWEDIMessageDocumentSupporter
	{
		public N5110MessageDocumentSupporter(TWMessage message) : base(message) { }
		public const string N5110MessagePair = ".N5110MessagePair";
		protected new N5110EDIMessage EdiMessage => (N5110EDIMessage)base.EdiMessage;

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(N5110MessagePair));
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			IBODocDataProvider wrapper;
			switch (dataContextValue.FullDataContext)
			{
				case N5110MessagePair:
					{
						wrapper = new N5110EDIMessageDocumentWrapper(EdiMessage);
						break;
					}
				default:
					return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
			}
			return wrapper != null ? new[] { wrapper } : null;
		}
	}
}
