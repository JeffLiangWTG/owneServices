//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoLandedCostHistoryLookups
//
//    This class should be used for overriding collections in AutoLandedCostHistoryLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.LandedCosting.Business
{
	public class LandedCostHistoryLookups : AutoLandedCostHistoryLookups
	{
		public LandedCostHistoryLookups(AutoLandedCostHistory parent) : base(parent)
		{
		}

		public CodeDescriptionPairList LineTypeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(LandedCostType.Actual, Constants.LineTypeDescriptions.Actual);
				if (Parent.SupportsNoCostApportionmentItem)
				{
					result.AddPair(LandedCostType.NoCostApportionmentItem, Constants.LineTypeDescriptions.NoCostApportionmentItem);
				}
				else
				{
					result.AddPair(LandedCostType.Estimated, Constants.LineTypeDescriptions.Estimated);
				}
				return result;
			}
		}

		protected new LandedCostHistory Parent => (LandedCostHistory)base.Parent;

		public static class Constants
		{
			public static class LineTypeDescriptions
			{
				public static MultilingualString Actual => ResString.GetMultilingualString("LineTypeDescription|Actual", "Actual");
				public static MultilingualString NoCostApportionmentItem => ResString.GetMultilingualString("LineTypeDescription|NoCostApportionmentItem", "No Cost Apportionment Item");
				public static MultilingualString Estimated => ResString.GetMultilingualString("LineTypeDescription|Estimated", "Estimated");
			}
		}
	}
}
