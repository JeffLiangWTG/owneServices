using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.Packing;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
	public class PkgHandlingUnit : AutoPkgHandlingUnit,
		IPackingParent,
		IDocumentSupportable
	{
		public PkgHandlingUnit(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Delete

		public override void Delete()
		{
			PackageJob?.Delete();
			base.Delete();
		}

		#endregion

		#region IShouldPackTrackedPackagesViaDivot

		bool IShouldPackTrackedPackagesViaDivot.ShouldPackTrackedPackagesViaDivot
		{
			get { return false; }
		}

		#endregion

		#region IPackingParent

		IPackageActionStrategy IPackingParent.GetPackageActionStrategy(PkgPackage package)
		{
			return new PackageActionStrategy(package);
		}

		ControllerID IPackingParent.ControllerID
		{
			get { return null; }
		}

		ZString IPackingParent.GetSSCCPrefix(INotifications notify, SSCCGenerationContext context)
		{
			return ZString.Empty;
		}

		DocumentOptions IPackingParent.DocumentOptions
		{
			get { return DocumentOptions.ShowBasicLabelOnly; }
		}

		bool IPackingParent.IsAutoPrintAllowed
		{
			get { return false; }
		}

		bool IPackingParent.IsPackingJobReadOnly => true;

		bool IPackingParent.IsParentJobFinalised
		{
			get { return false; }
		}

		bool IPackingParent.IsScanEventsVisible
		{
			get { return false; }
		}

		ZString IPackingParent.JobDescription
		{
			get { return Res.GetString("83511c6d-532f-4acd-9e10-44e7e0d627b8", "Handling Unit"); }
		}

		ZString IPackingParent.ConnoteNo
		{
			get { return ZString.Empty; }
		}

		ZString IPackingParent.JobNo
		{
			get { return "HU"; } // This is the hard code job no of handling unit.
		}

		void IPackingParent.OnContainerIDChanged(PkgPackage container)
		{
		}

		void IPackingParent.OnPackageJobReleased()
		{
		}

		void IPackingParent.OnPackageJobCreatedOrLoaded(PkgPackageJob pkgJob)
		{
		}

		void IPackingParent.OnPackageDelete(PkgPackage package)
		{
		}

		ZString IPackingParent.CarrierServiceLevelCode(PkgPackage package)
		{
			return "";
		}

		// Tested in Enterprise.Warehouse.Web.WebService.Testing.PrintCarrierLabelTest
		OrgHeader IPackingParent.CarrierBookingAgent
			=> InnerPackageParentJobs.SameOrDefault(p => p.CarrierBookingAgent);

		IReadOnlyCollection<IPackingParent> InnerPackageParentJobs => innerPackageParentJobs ?? InnerPackageParentJobsForProductWarehouse();
		readonly IReadOnlyCollection<IPackingParent> innerPackageParentJobs;

		List<IPackingParent> InnerPackageParentJobsForProductWarehouse()
		{
			var innerpackagesForProductWarehouse = new List<IPackingParent>();

			if (KPU_JobContext == PkgHandlingUnitConstants.BusinessContexts.ProductWarehouse)
			{
				var handlingUnitPackage = PackageJob.Packages.FirstOrDefault();

				var innerPackagesQuery = new ZQuery(PkgPackageSchema.KP_KP_TopHandlingUnitPackage, handlingUnitPackage.PK);
				var innerPackageRows = ((IBusinessObjectFactoryInternals)Factory).RowFactory.Load(PkgPackageSchema.Constants.TableName, innerPackagesQuery); // avoid triggering type decider.
				var packageJobPKs = innerPackageRows.Select(row => (Guid)row[nameof(PkgPackageSchema.KP_KJ_ParentPackageJob)]).Distinct();
				var packageJobs = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.PK, packageJobPKs));

				innerpackagesForProductWarehouse = packageJobs.Select(p => p.ParentJob).ToList();
			}

			return innerpackagesForProductWarehouse;
		}

		OrgHeader IPackingParent.GetCarrier(PkgPackage package)
		{
			return null;
		}

		// Tested in Enterprise.Warehouse.Web.WebService.Testing.PrintCarrierLabelTest
		ZString IPackingParent.TransportReference
		{
			get => InnerPackageParentJobs.SameOrDefault(p => p.TransportReference);
			set => InnerPackageParentJobs.ForEach(p => p.TransportReference = value);
		}

		bool IPackingParent.IsLoosePackageIDsSupported
		{
			get { return false; }
		}

		void IPackingParent.BeforeUnpackingPackages(IReadOnlyList<PkgPackage> packages)
		{
		}

		bool IPackingParent.IsUXMLEventParent(IXmlEventValueObject xmlEvent) => false;

		IEnumerable<KeyValuePair<TypeWithDescription, IZType>> IPackingParent.GetAdditionalEventContextValuesFromParent() => Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();

		ParentJobType IPackingParent.ParentJobType => ParentJobType.None;

		PackageSequenceType IPackingParent.PackageSequenceType => PackageSequenceType.Outer;

		bool IPackingParent.CanReleasePackage(PkgPackage package) => true;

		ZString IPackingParent.GetCannotReleasePackageMessage(PkgPackage package) => ZString.Empty;

		void IPackingParent.OnPackageBookedViaRTUS(ZDateTime sentDateTime)
		{
		}

		NotificationTypes IPackingParent.NotificationTypeForInvalidContainerNumber => NotificationTypes.None;

		#endregion

		#region PackageJob

		public PkgPackageJob PackageJob
		{
			get { return packageJob ?? (packageJob = GetPackageJob()); }
		}

		PkgPackageJob GetPackageJob()
		{
			var packageJobToReturn = PkgPackageJob.LoadOrCreatePackageJobWithNoChanges(this);
			return packageJobToReturn;
		}

		PkgPackageJob packageJob;

		#endregion

		#region HandlingUnitID

		public ZString HandlingUnitID => PackageJob.Packages.FirstOrDefault()?.PackageID?.KPH_PackageID ?? ZString.Empty;

		public ZPropertyInfo HandlingUnitIDInfo => GetZPropertyInfo(nameof(HandlingUnitID));

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore => Res.GetString("a0b41654-ca39-43ca-b3e4-42022fcb8114", "Package Handling Unit");

		#endregion

		#region DocumentSupporter

		DocumentSupporter IDocumentSupportable.DocumentSupporter => documentSupporter ?? (documentSupporter = new PkgHandlingUnitDocumentSupporter(this));

		DocumentSupporter documentSupporter;

		#endregion

		#region FillWithValidTestDataCoreForTest
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			KPU_JobContext = PkgHandlingUnitConstants.BusinessContexts.TransitWarehouse; // set the test default to Transit Warehouse.
			KPU_GB_Branch = GlbBranch.CurrentBranch.PK;
		}
#endif
		#endregion
	}
}

