using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Packing.Business
{
	public class PkgPackageJobPackageHeaderPivotDocumentSupporter : DocumentSupporter
	{
		public PkgPackageJobPackageHeaderPivotDocumentSupporter(PkgPackageJobPackageHeaderPivot packageJobPackageHeaderPivot)
			: base(packageJobPackageHeaderPivot)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.PackageHeader; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result;

			switch (dataContext)
			{
				case Constants.DataContext.GenericFreightJob:
				case Constants.DataContext.GenericBasicLabel:
					PackageHeaderPivot.PackageJob.CheckAndFixSequence();
					var packageHeader = PackageHeaderPivot.PackageHeader;
					packageHeader.CurrentPackageJob = PackageHeaderPivot.PackageJob;
					result = DocumentWrapperFactory.GenerateGenericWrappers(Constants.DataContext.GenericFreightJob, PackageHeaderPivot.PackageJob, packageHeader);
					break;

				default:
					result = System.Array.Empty<DocumentWrapper>();
					break;
			}

			return result;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[] { Constants.DataContext.GenericFreightJob, Constants.DataContext.GenericBasicLabel };
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		#region PackageHeaderPivot

		PkgPackageJobPackageHeaderPivot PackageHeaderPivot
		{
			get { return (PkgPackageJobPackageHeaderPivot)BusinessObject; }
		}

		#endregion
	}
}
