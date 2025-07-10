//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobVoyOriginLookups
//
//    This class should be used for overriding collections in AutoJobVoyOriginLookups
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
	public class BaseJobVoyOriginLookups : JobVoyOriginLookups
	{
		public BaseJobVoyOriginLookups(AutoJobVoyOrigin parent) : base(parent)
		{
			Origin = (VoyageOrigin)parent;
		}

		readonly VoyageOrigin Origin;

		public OrgHeaderCollection CTO_List
		{
			get
			{
				OrgHeaderCollection result = null;
				if (Origin.Voyage != null && Origin.Voyage.JV_AirSeaRoad == Core.Constants.TransportModes.Air)
				{
					result = new AirCTOCollection(Factory);
				}
				else if (Origin.Voyage != null && Origin.Voyage.JV_AirSeaRoad == Core.Constants.TransportModes.Sea)
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

		public OrgAddressCollection JA_DepartureCTOAddress_List
		{
			get
			{
				OrgAddressCollection result = new OrgAddressCollection(Factory);
				if (Origin.JA_Calc_DepartureCTOAddressOrg.IsValid)
				{
					var departureCTO = Factory.Load<OrgHeader>(Origin.JA_Calc_DepartureCTOAddressOrg);
					if (departureCTO != null)
					{
						result = new OrgAddressCollection(Factory, new ZQuery(OrgAddressSchema.OA_OH, departureCTO.PK));
						result.Load();
					}
				}
				return result;
			}
		}
	}
}
