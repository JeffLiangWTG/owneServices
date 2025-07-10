using System;
using CargoWise.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.Integration
{
	public interface ITransportRegistry
	{
		IRegistryItem TransportDriversGroup { get; }
		StringRegistryItem EquipmentTruckSafe { get; }
		IRegistryItem LocalTransportMobileSettingsPassword { get; }
		BooleanRegistryItem EnableBookingWithCarrierMessagingBuss { get; }
		BooleanRegistryItem EnableLandTransport { get; }
		BooleanRegistryItem AuthorityToLeave { get; }
		StringRegistryItem RemotePrintServerURL { get; }
		IRegistryItem OrganisationRTUSOptions { get; }
		IOrganisationRTUSOption GetRTUSOption(Guid carrierBookingAgentPK);
		StringRegistryItem OrganisationRTUSWebProxy { get; }
		ServerUsernamePassword OrganisationRTUSWebProxyCredentials { get; }
		BooleanRegistryItem UseStandardRemotePrintingForRTUS { get; }
		BooleanRegistryItem EnableRTUSBatchProcessing { get; }
		StringRegistryItem TestCbaId { get; }
		BooleanRegistryItem EnableEquipmentCombination { get; }
	}

	public interface IConsignmentListLoader
	{
		ICodeDescriptionPairList GetTransportJobServices();
		ICodeDescriptionPairList GetTransportBookingJobServices();
	}
}
