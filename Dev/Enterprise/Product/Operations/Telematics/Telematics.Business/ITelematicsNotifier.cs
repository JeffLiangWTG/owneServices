using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.MobileServices.Common.Messages;

namespace Enterprise.Telematics.Business
{
	public interface ITelematicsNotifier
	{
		void NotifyMobileServicesOfNewDeviceParent(BusinessObjectFactory factory, string deviceFriendlyIdentifier, string parentCode, string parentDescription, string parentType);
		void NotifyMobileServicesOfDeviceDetails(BusinessObjectFactory factory, string mobileServicesClientId, string deviceFriendlyIdentifier, bool isBYOD, string manufacturer, string description, DeviceKind deviceKind, string deviceDrivenIdentifier, string clientLicenceCode);
		void NotifyTelematicsServicesOfDeviceDetails(BusinessObjectFactory factory, IEnumerable<string> telematicsClientIds, string humanReadableIdentifier, string model, string hardwareIdentifier, string deviceWasAssignedTo, string deviceAssignedTo);
		void NotifyMobileServicesOfBYODRegistration(BusinessObjectFactory factory, string deviceClientIdentifier, string deviceModel, DeviceKind deviceKind, string deviceIdentifier);
		void NotifyMobileServicesOfBYODDeregistration(BusinessObjectFactory factory, string deviceClientIdentifier);
		bool ShouldNotifySynchronously { get; }
	}
}
