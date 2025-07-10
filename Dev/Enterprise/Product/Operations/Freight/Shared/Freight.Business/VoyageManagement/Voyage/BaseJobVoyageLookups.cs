//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobVoyageLookups
//
//    This class should be used for overriding collections in AutoJobVoyageLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class BaseJobVoyageLookups : JobVoyageLookups
	{
		public BaseJobVoyageLookups(AutoJobVoyage parent) : base(parent)
		{
			voyage = (JobVoyage)parent;
		}

		readonly JobVoyage voyage;

		public override OrgHeaderCollection Lines
		{
			get { return GetCarrierLookup(Factory, voyage.JV_AirSeaRoad); }
		}

		public static OrgHeaderCollection GetCarrierLookup(BusinessObjectFactory factory, string transportMode)
		{
			switch (transportMode)
			{
				case Core.Constants.TransportModes.Air:
					return new AirShippingProviderCollection(factory);

				case Core.Constants.TransportModes.Sea:
					return new SeaShippingProviderCollection(factory);

				case Core.Constants.TransportModes.Rail:
					return new TransportScheduleRailShippingProviderCollection(factory);

				case Core.Constants.TransportModes.Road:
					return new TransportScheduleLineHaulShippingProviderCollection(factory);

				default:
					return new ShippingProviderCollection(factory);
			}
		}
	}
}
