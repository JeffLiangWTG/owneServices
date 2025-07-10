using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business
{
	public interface IContainerTrackingProvider
	{
		ZString TransportMode { get; }

		bool TransportModeHasChanges { get; }

		bool CarrierCodeHasChanges { get; }

		bool CoLoadHasChanges { get; }

		ZString CoLoadWithMasterBillNumber { get; }

		ZString CoLoadWithCarrierBookingReference { get; }

		OrgHeader CoLoadWith { get; }

		ZString MasterBillNumber { get; }

		ZString ContainerMode { get; }

		bool MasterBillNumberHasChanges { get; }

		bool ContainerModeHasChanges { get; }

		ZString CarrierBookingReference { get; }

		bool CarrierBookingReferenceHasChanges { get; }

		ZDateTime GetLastArrivalDate();

		ZDateTime GetFirstDepartureDate();

		bool SubscribeToContainersOnly { get; }

		bool SubscribeToContainersOnlyHasChanges { get; }

		IEnumerable<ITrackableContainer> Containers { get; }

		OrgHeader ShippingLine { get; }

		Logs Logs { get; }

		bool RoutingLegsHaveChanges { get; }
	}
}
