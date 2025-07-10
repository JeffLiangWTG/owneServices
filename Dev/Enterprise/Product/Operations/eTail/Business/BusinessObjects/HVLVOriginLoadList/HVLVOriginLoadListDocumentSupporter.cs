using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.Business
{
	public class HVLVOriginLoadListDocumentSupporter : DocumentSupporter
	{
		public HVLVOriginLoadListDocumentSupporter(HVLVOriginLoadList parentBusinessObject)
			: base(parentBusinessObject)
		{
		}

		public override BusinessContext BusinessContext => BusinessContext.HVLVOriginLoadList;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.HVLVBookingHeaderCustomiseDocuments;

		protected override List<DataContextValue> GetSupportedBODataSources() => Enumerable.Empty<DataContextValue>().ToList();

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun) => null;

		protected override DataContext[] GetSupportedDataContexts() => new[] { DataContext.GenericFreightJob };
	}
}
