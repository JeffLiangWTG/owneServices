using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Packing.Business
{
	public class PkgHandlingUnitDocumentSupporter : DocumentSupporter
	{
		public PkgHandlingUnitDocumentSupporter(PkgHandlingUnit handlingUnit)
			: base(handlingUnit)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.PackageHandlingUnit; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;

			if (dataContext == Constants.DataContext.GenericNewPackageID && commandBeingRun != null)
			{
				result = TransportPackageLabelHelper.CreateDocumentWrapperForNewPackageID(HandlingUnit);
			}

			return result;
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return false;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Constants.DataContext.GenericFreightJob };
		}

		#region SecurityCheckpoint

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.PkgHandlingUnitCustomizeDocuments;

		#endregion

		#region HandlingUnit

		protected PkgHandlingUnit HandlingUnit
		{
			get { return (PkgHandlingUnit)BusinessObject; }
		}

		#endregion
	}
}
