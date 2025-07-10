using System.Collections.Generic;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class N5204MessageDocumentSupporter : TWEDIMessageDocumentSupporter
	{
		public N5204MessageDocumentSupporter(TWMessage message) : base(message)
		{
		}

		public const string N5204MessagePair = ".N5204MessagePair";
		protected new N5204EDIMessage EdiMessage => (N5204EDIMessage)base.EdiMessage;

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(N5204MessagePair));
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			IBODocDataProvider wrapper;
			switch (dataContextValue.FullDataContext)
			{
				case N5204MessagePair:
					{
						wrapper = new N5204EDIMessageDocumentWrapper(EdiMessage);
						break;
					}
				default:
					return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
			}
			return wrapper != null ? new[] { wrapper } : null;
		}
	}
}
