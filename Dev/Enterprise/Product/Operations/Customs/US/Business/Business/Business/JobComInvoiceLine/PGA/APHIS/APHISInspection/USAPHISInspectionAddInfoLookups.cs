//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSAPHISInspectionAddInfoLookups
//
//    This class should be used for overriding collections in AutoUSAPHISInspectionAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class USAPHISInspectionAddInfoLookups : AutoUSAPHISInspectionAddInfoLookups
	{
		public USAPHISInspectionAddInfoLookups(AutoUSAPHISInspectionAddInfo parent)
			: base(parent)
		{
		}

		public ICodeDescriptionPairList InspectionStatusList
		{
			get { return Enterprise.Customs.US.Business.InspectionStatusList.GetListForAPHIS(Factory); }
		}

		public IBusinessObjectCollection PortCodes
		{
			get
			{
				if (Inspection.IsUNLOCOApplicable)
				{
					return new RefUNLOCOCollection(Factory);
				}
				else
				{
					return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
				}
			}
		}

		protected new USAPHISInspectionAddInfo Parent
		{
			get { return (USAPHISInspectionAddInfo)base.Parent; }
		}

		protected APHISInspection Inspection
		{
			get { return Parent.Parent; }
		}
	}
}
