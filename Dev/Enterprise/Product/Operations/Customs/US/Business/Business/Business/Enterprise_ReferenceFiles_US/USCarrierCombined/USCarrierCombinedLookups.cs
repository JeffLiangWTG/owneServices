//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSCarrierCombinedLookups
//
//    This class should be used for overriding collections in AutoUSCarrierCombinedLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Customs.US.Messaging.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USCarrierCombinedLookups : AutoUSCarrierCombinedLookups
	{
		public USCarrierCombinedLookups(AutoUSCarrierCombined parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList TransportList
		{
			get
			{
				return Factory.GetCachedValue("USCCarrierTransportList", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(TransportModeCodes.Codes.VesselNonContainer, Res.GetString("52784EA0-8205-407A-88E4-8F4DC84B92A9", "Vessel"));
					result.AddPair(TransportModeCodes.Codes.RailNonContainer, Res.GetString("61F58655-B828-46E0-9C07-C3913812AE71", "Rail"));
					result.AddPair(TransportModeCodes.Codes.TruckNonContainer, Res.GetString("19019DBC-85E8-4DDA-B2B9-0A41B5BCE1F0", "Truck"));
					result.AddPair(TransportModeCodes.Codes.AirNonContainer, Res.GetString("909C57E3-AAF3-46FA-820A-C0B48D5A7C7B", "Air"));
					return result;
				});
			}
		}
	}
}
