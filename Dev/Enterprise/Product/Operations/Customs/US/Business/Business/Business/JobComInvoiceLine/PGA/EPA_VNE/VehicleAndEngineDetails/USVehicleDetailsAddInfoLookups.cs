//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSVehicleDetailsAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSVehicleDetailsAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USVehicleDetailsAddInfoLookups : AutoUSVehicleDetailsAddInfoLookups
	{
		public USVehicleDetailsAddInfoLookups(AutoUSVehicleDetailsAddInfo parent) : base(parent)
		{
		}

		public CodeDescriptionPairList IdentityQualifier
		{
			get
			{
				return Factory.GetCachedValue("IdentityQualifier",
					delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(ItemIdentityNumberQualifierList.Codes.SerialNumber, ItemIdentityNumberQualifierList.Descriptions.SerialNumber);
						result.AddPair(ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN, ItemIdentityNumberQualifierList.Descriptions.VehicleIdentificationNumberVIN);
						return result;
					});
			}
		}

		public MonthList MonthList
		{
			get { return Factory.GetCachedValue<MonthList>(); }
		}

		public ManufactureDateTypeList DateTypes
		{
			get { return Factory.GetCachedValue<ManufactureDateTypeList>(); }
		}
	}
}
