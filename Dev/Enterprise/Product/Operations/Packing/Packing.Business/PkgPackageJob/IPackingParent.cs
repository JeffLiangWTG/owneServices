using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Packing;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Packing.Business
{
	// eg WhsOrder or JobCartage..
	public interface IPackingParent : IStmALogParent, IShouldPackTrackedPackagesViaDivot
	{
		// these exist on BusinessObject thus consumers will not have to reimplement..
		ZGuid PK { get; }
		string TablePrefix { get; }

		ZString JobDescription { get; }
		ZString JobNo { get; }
		ZString ConnoteNo { get; }
		ZString GetSSCCPrefix(INotifications notify, SSCCGenerationContext context);

		/// <summary>
		/// Packing will automatically delete any finalised package jobs that are empty.
		/// This property is only used in such a case, to determine whether or not the new auto-created packagejob (for binding) is ReadOnly.
		/// </summary>
		bool IsParentJobFinalised { get; }

		bool IsAutoPrintAllowed { get; }

		bool IsPackingJobReadOnly { get; }

		bool IsLoosePackageIDsSupported { get; }

		/// <summary>
		/// Attached GUI components should honour this in determining whether or not to show Scan Events (StmALogs) for Packages.
		/// </summary>
		bool IsScanEventsVisible { get; }

		ControllerID ControllerID { get; }
		IPackageActionStrategy GetPackageActionStrategy(PkgPackage package);

		void OnContainerIDChanged(PkgPackage container);
		void OnPackageJobReleased();

		void OnPackageDelete(PkgPackage package);

		void OnPackageJobCreatedOrLoaded(PkgPackageJob packageJob);

		void BeforeUnpackingPackages(IReadOnlyList<PkgPackage> packages);

		/// <summary>
		/// Used by the Document Engine to determine which documents to show.
		/// </summary>
		DocumentOptions DocumentOptions { get; }

		/// <summary>
		/// Carriers Information
		/// </summary>
		ZString CarrierServiceLevelCode(PkgPackage package);

		OrgHeader CarrierBookingAgent { get; }
		OrgHeader GetCarrier(PkgPackage package);

		ZString TransportReference { get; set; }

		bool IsUXMLEventParent(IXmlEventValueObject xmlEvent);

		IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetAdditionalEventContextValuesFromParent();

		ParentJobType ParentJobType { get; }

		PackageSequenceType PackageSequenceType { get; }

		bool CanReleasePackage(PkgPackage package);
		ZString GetCannotReleasePackageMessage(PkgPackage package);
		void OnPackageBookedViaRTUS(ZDateTime sentDateTime);
		NotificationTypes NotificationTypeForInvalidContainerNumber { get; }
	}
}
