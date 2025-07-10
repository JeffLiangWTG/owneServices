//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsCartonSizeLookups
//
//    This class should be used for overriding collections in AutoWhsCartonSizeLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsCartonSizeLookups : AutoWhsCartonSizeLookups
	{
		public WhsCartonSizeLookups(AutoWhsCartonSize parent) : base(parent)
		{
		}

		#region DimensionUQs

		public CodeDescriptionPairList DimensionUQs
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Length); }
		}

		#endregion

		#region WeightUQs

		public CodeDescriptionPairList WeightUQs
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		#endregion

		#region VolumeUQs

		public CodeDescriptionPairList VolumeUQs
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		#endregion
	}
}
