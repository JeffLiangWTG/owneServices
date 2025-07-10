using System.Collections.Generic;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class N5116MessageDocumentSupporter : TWEDIMessageDocumentSupporter
	{
		public N5116MessageDocumentSupporter(TWMessage message) : base(message) { }
		public const string N5116MessagePair = ".N5116MessagePair";
		protected new N5116EDIMessage EdiMessage => (N5116EDIMessage)base.EdiMessage;

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(N5116MessagePair));
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			IBODocDataProvider wrapper;
			switch (dataContextValue.FullDataContext)
			{
				case N5116MessagePair:
					{
						wrapper = new N5116EDIMessageDocumentWrapper(EdiMessage);
						break;
					}
				default:
					return base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
			}
			return wrapper != null ? new[] { wrapper } : null;
		}
	}
}
