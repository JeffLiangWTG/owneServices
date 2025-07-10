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
	public class HVLVOuterPackageDocumentSupporter : DocumentSupporter
	{
		public HVLVOuterPackageDocumentSupporter(HVLVOuterPackage parentBusinessObject)
			: base(parentBusinessObject)
		{
		}

		HVLVOuterPackage OuterPackage => (HVLVOuterPackage)BusinessObject;

		public override BusinessContext BusinessContext => BusinessContext.HVLVOuterPackage;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.HVLVBookingHeaderCustomiseDocuments;

		protected override List<DataContextValue> GetSupportedBODataSources() => Enumerable.Empty<DataContextValue>().ToList();

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, OuterPackage);
		}

		protected override DataContext[] GetSupportedDataContexts() => new[] { DataContext.GenericFreightJob };
	}
}
