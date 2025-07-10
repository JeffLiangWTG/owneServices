//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobVoyDestinationLookups
//
//    This class should be used for overriding collections in AutoJobVoyDestinationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class BaseJobVoyDestinationLookups : JobVoyDestinationLookups
	{
		public BaseJobVoyDestinationLookups(AutoJobVoyDestination parent) : base(parent)
		{
			Destination = (VoyageDestination)parent;
		}

		readonly VoyageDestination Destination;

		public OrgHeaderCollection CTO_List
		{
			get
			{
				OrgHeaderCollection result = null;
				if (Destination.Voyage != null && Destination.Voyage.JV_AirSeaRoad == Core.Constants.TransportModes.Air)
				{
					result = new AirCTOCollection(Factory);
				}
				else if (Destination.Voyage != null && Destination.Voyage.JV_AirSeaRoad == Core.Constants.TransportModes.Sea)
				{
					result = new SeaCTOCollection(Factory);
				}
				else
				{
					result = new CTOCollection(Factory);
				}
				return result;
			}
		}

		public OrgAddressCollection JB_ArrivalCTOAddress_List
		{
			get
			{
				OrgAddressCollection result = new OrgAddressCollection(Factory);
				if (Destination.JB_Calc_ArrivalCTOAddressOrg.IsValid)
				{
					var arrivalCTO = Factory.Load<OrgHeader>(Destination.JB_Calc_ArrivalCTOAddressOrg);
					if (arrivalCTO != null)
					{
						result = new OrgAddressCollection(Factory, new ZQuery(OrgAddressSchema.OA_OH, arrivalCTO.PK));
						result.Load();
					}
				}
				return result;
			}
		}
	}
}
