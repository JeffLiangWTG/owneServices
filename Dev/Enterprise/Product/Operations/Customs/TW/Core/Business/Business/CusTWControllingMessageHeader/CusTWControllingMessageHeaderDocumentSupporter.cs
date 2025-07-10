using System.Collections.Generic;
using CargoWise.Definitions;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.TW.Business
{
	public class CusTWControllingMessageHeaderDocumentSupporter : DocumentSupporter
	{
		internal const string NX101ControllingMessageHeaderPair = ".NX101ControllingMessageHeaderPair";

		public CusTWControllingMessageHeaderDocumentSupporter(CusTWControllingMessageHeader header) : base(header) { }

		protected CusTWControllingMessageHeader ControllingMessageHeader => (CusTWControllingMessageHeader)BusinessObject;

		public override BusinessContext BusinessContext => BusinessContext.TWControllingMessage;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.CustomsDeclarationCustomiseDocument;

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(NX101ControllingMessageHeaderPair));
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
			=> dataContextValue.FullDataContext == NX101ControllingMessageHeaderPair ? new[] { new NX101ControllingMessageHeaderDocumentWrapper(ControllingMessageHeader) }
			: base.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun) => System.Array.Empty<DocumentWrapper>();

		protected override Core.Constants.DataContext[] GetSupportedDataContexts() => System.Array.Empty<Core.Constants.DataContext>();
	}
}
