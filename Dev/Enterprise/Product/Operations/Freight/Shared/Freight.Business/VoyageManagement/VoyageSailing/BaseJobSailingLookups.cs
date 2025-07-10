//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobSailingLookups
//
//    This class should be used for overriding collections in AutoJobSailingLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	public class BaseJobSailingLookups : JobSailingLookups
	{
		public BaseJobSailingLookups(AutoJobSailing parent)
			: base(parent)
		{
			Sailing = (BaseJobSailing)parent;
		}

		protected BaseJobSailing Sailing;

		public RefUNLOCOCollection Ports
		{
			get
			{
				if (fPorts == null)
				{
					fPorts = new RefUNLOCOCollection(Factory);
				}
				return fPorts;
			}
		}
		protected RefUNLOCOCollection fPorts;

		protected OrgHeaderCollection fCTO_List;
		public OrgHeaderCollection CTO_List
		{
			get
			{
				if (fCTO_List == null)
				{
					if (Sailing.JX_TransportMode == Constants.TransportModes.Air)
					{
						fCTO_List = new AirCTOCollection(Factory);
					}
					else if (Sailing.JX_TransportMode == Constants.TransportModes.Sea)
					{
						fCTO_List = new SeaCTOCollection(Factory);
					}
					else
					{
						fCTO_List = new CTOCollection(Factory);
					}
				}
				return fCTO_List;
			}
		}

		protected OrgHeaderCollection fCarrier_List;
		public OrgHeaderCollection Carrier_List
		{
			get
			{
				if (fCarrier_List == null)
				{
					if (Sailing.JX_TransportMode == Constants.TransportModes.Air)
					{
						fCarrier_List = new AirShippingProviderCollection(Factory);
					}
					else if (Sailing.JX_TransportMode == Constants.TransportModes.Sea)
					{
						fCarrier_List = new SeaShippingProviderCollection(Factory);
					}
					else if (Sailing.JX_TransportMode == Constants.TransportModes.Road)
					{
						fCarrier_List = new TransportScheduleLineHaulShippingProviderCollection(Factory);
					}
					else if (Sailing.JX_TransportMode == Constants.TransportModes.Rail)
					{
						fCarrier_List = new TransportScheduleRailShippingProviderCollection(Factory);
					}
					else
					{
						fCarrier_List = new ShippingProviderCollection(Factory);
					}
				}
				return fCarrier_List;
			}
		}
	}
}
