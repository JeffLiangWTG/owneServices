using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business
{
	[ModuleID(ModuleId.USCarrierCombined)]
	public class USCarrierCombinedCollection : ActiveBusinessObjectCollection<USCarrierCombined>
	{
		public USCarrierCombinedCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			SetFilterBusinessObjectDefaults(TransportModeCodes.Codes.AirNonContainer);
		}

		public USCarrierCombinedCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
			SetFilterBusinessObjectDefaults(TransportModeCodes.Codes.AirNonContainer);
		}

		public USCarrierCombinedCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
			SetFilterBusinessObjectDefaults(TransportModeCodes.Codes.AirNonContainer);
		}

		public USCarrierCombinedCollection(BusinessObjectFactory factory, ZString defaultTransportMode)
			: base(factory)
		{
			SetFilterBusinessObjectDefaults(defaultTransportMode);
		}

		void SetFilterBusinessObjectDefaults(ZString defaultTransportMode)
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Mode Of Transportation", "Property", defaultTransportMode));
		}
	}
}
