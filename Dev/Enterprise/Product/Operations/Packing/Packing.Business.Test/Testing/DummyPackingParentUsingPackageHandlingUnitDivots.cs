using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Packing;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Packing.Business.Testing
{
	[UniversalDataContext(DataContextType.DummyBusinessObject)]
	public class DummyPackingParentUsingPackageHandlingUnitDivots : DummyBusinessObject, IPackingParent
	{
		public DummyPackingParentUsingPackageHandlingUnitDivots(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		bool IShouldPackTrackedPackagesViaDivot.ShouldPackTrackedPackagesViaDivot
		{
			get { return true; }
		}

		ZString IPackingParent.GetSSCCPrefix(INotifications notify, SSCCGenerationContext context)
		{
			return ZString.Empty;
		}

		IPackageActionStrategy IPackingParent.GetPackageActionStrategy(PkgPackage package)
		{
			return new PackageActionStrategy(package);
		}

		ControllerID IPackingParent.ControllerID
		{
			get { return null; }
		}

		DocumentOptions IPackingParent.DocumentOptions
		{
			get { return DocumentOptions.ShowBasicLabelOnly; }
		}

		bool IPackingParent.IsAutoPrintAllowed
		{
			get { return false; }
		}

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
			get { return ""; }
		}

		ZString IPackingParent.ConnoteNo
		{
			get { return ZString.Empty; }
		}

		ZString IPackingParent.JobNo
		{
			get { return "HU"; } // This is the hard code job no of handling unit.
		}

		bool IPackingParent.IsPackingJobReadOnly => false;

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

		OrgHeader IPackingParent.CarrierBookingAgent => null;

		OrgHeader IPackingParent.GetCarrier(PkgPackage package)
		{
			return null;
		}

		ZString IPackingParent.TransportReference { get => ZString.Empty; set { } }

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

		public ControllerID ControllerID => throw new NotImplementedException();

		public ZGuid LogsParentPK => throw new NotImplementedException();

		public string LogsParentTableName => throw new NotImplementedException();

		public BusinessObject[] BusinessObjectsWithRelatedEvents => throw new NotImplementedException();

		public bool DeferFiringWorkflow => throw new NotImplementedException();

		public Logs Logs => throw new NotImplementedException();

		public BusinessObjectFactory LogsFactory => throw new NotImplementedException();

		PackageSequenceType IPackingParent.PackageSequenceType => PackageSequenceType.Outer;

		bool IPackingParent.CanReleasePackage(PkgPackage package) => true;

		ZString IPackingParent.GetCannotReleasePackageMessage(PkgPackage package) => ZString.Empty;

		void IPackingParent.OnPackageBookedViaRTUS(ZDateTime sentDateTime)
		{
		}

		public NotificationTypes NotificationTypeForInvalidContainerNumber { get; set; }

		public void ProcessLog(IStmALog log)
		{
			throw new NotImplementedException();
		}
	}
}
