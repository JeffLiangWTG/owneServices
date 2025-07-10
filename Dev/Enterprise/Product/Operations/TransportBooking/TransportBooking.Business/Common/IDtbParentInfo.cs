using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Packing;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.Business
{
	public interface IDtbParentInfoLoader
	{
		IDtbParentInfo Load(DtbBookingConsolidation consolidation);
#if DEBUG
		IDisposable SetDummyWriterDecider(bool isFCL = false, bool populateActualDates = false);
#endif
	}

	public interface IDtbParentInfo
	{
		// direct bo - business

		ZGuid PK { get; }
		string TablePrefix { get; }
		bool HasChanges { get; }
		ZString HumanReadableName { get; }
		bool IsInDatabase { get; }
		BusinessObjectFactory Factory { get; }
		ControllerID ControllerID { get; }
		BusinessObject ParentWithWorkflow { get; }
		bool SupportsDirectSailing { get; }

		// direct bo - job specific

		ZString JobNumber { get; }
		ZString JobDescription { get; }
		ZString JobType { get; }
		ZString ClientServiceLevel { get; }

		// direct bo - job header

		IJobInvoicingPlugIn InvoicingJob { get; }

		// bus - direction specific

		ZString? TransportMode { get; }
		ZString? ContainerMode { get; }
		ZString? CarrierServiceLevel { get; }
		ZString? TransportReference { get; }
		ZString? DropMode { get; }

		void UpdateAddress(JobDocAddress docAddressToUpdate, bool allowOverride, IEnumerable<ZInt> containerLinksFilter, IEnumerable<ZString> containerNumbersFilter);

		DatesAndReference GetDatesAndReferences(ZString confirmationTypeCode, ZString addressType, IPkgPackage package, IReadOnlyDictionary<PkgPackage, ZString> releaseNumbersByPackage);

		DataObjectList<TransportLeg> GetParentRoutingTransportLegs();
	}
}

