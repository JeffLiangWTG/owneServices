//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusVehicleLookups
//
//    This class should be used for overriding collections in AutoCusVehicleLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class CusVehicleLookups : AutoCusVehicleLookups
	{
		public CusVehicleLookups(AutoCusVehicle parent) : base(parent)
		{
		}

		public CodeDescriptionPairList MileageUQList
		{
			get
			{
				return Factory.GetCachedValue("CusVehicleLookups.MileageUQList", () =>
				{
					var codeDescriptionPairList = new CodeDescriptionPairList();
					codeDescriptionPairList.AddPair("KM", Res.GetString("98B5460D-49F7-4204-ABA5-2CC6ED50BD81", "Kilometers"));
					codeDescriptionPairList.AddPair("MI", Res.GetString("AD6BE059-7501-4B1B-A46B-A9B5ED43AE97", "Miles"));
					return codeDescriptionPairList;
				});
			}
		}
	}
}
