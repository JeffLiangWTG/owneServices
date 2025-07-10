//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefFacilityLookups
//
//    This class should be used for overriding collections in AutoRefFacilityLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RefFacilityLookups : AutoRefFacilityLookups
	{
		public RefFacilityLookups(AutoRefFacility parent) : base(parent)
		{
		}

		internal RefFacility ParentFacility => Parent as RefFacility;

		#region FacilityType

		public CodeDescriptionPairList FacilityTypes
		{
			get
			{
				return new CodeDescriptionPairList(OLookUpEditType.FacilityTypes);
			}
		}

		#endregion

		#region RFT_Codes

		public RefFacilityCollection RefFacilities
		{
			get
			{
				if (refFacilities == null)
				{
					refFacilities = new RefFacilityCollection(Factory);
				}
				return refFacilities;
			}
		}
		RefFacilityCollection refFacilities;

		#endregion

		#region RefFacilityLocalCodes

		public RefFacilityLocalCodeCollection RefFacilityLocalCodeList
		{
			get => new RefFacilityLocalCodeCollection(ParentFacility);
		}

		#endregion
	}
}
