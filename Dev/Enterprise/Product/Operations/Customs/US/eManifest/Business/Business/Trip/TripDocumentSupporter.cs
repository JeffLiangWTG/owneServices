using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.US.eManifest.Business
{
	class TripDocumentSupporter : DocumentSupporter
	{
		public TripDocumentSupporter(BusinessObject parentBusinessObject)
			: base(parentBusinessObject)
		{
		}

		#region Overrides of DocumentSupporter

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(eManifestDocuments));
			return result;
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			return new[] { BODocDataProvider.Get(new eManifestDocumentWrapper((Trip)BusinessObject)) };
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return null;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return System.Array.Empty<Constants.DataContext>();
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.eManifest; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.USeManifestCustomiseDocuments; }
		}

		const string eManifestDocuments = ".e-Manifest";

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		#endregion
	}
}
