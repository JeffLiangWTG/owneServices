using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Rating
{
	public class RatingDocumentsChargeGroupingOrRollupLookups : AutoRatingDocumentsChargeGroupingOrRollupLookups
	{
		public RatingDocumentsChargeGroupingOrRollupLookups(AutoRatingDocumentsChargeGroupingOrRollup parent)
			: base(parent)
		{
		}

		new RatingDocumentsChargeGroupingOrRollup Parent => (RatingDocumentsChargeGroupingOrRollup)base.Parent;

		public CodeDescriptionPairList ModuleList => Parent.Helper.ModuleList;

		public CodeDescriptionPairList JobTypeList => Parent.Helper.JobTypeList;

		public CodeDescriptionPairList TransportModeList => Parent.Helper.TransportModeList;

		public CodeDescriptionPairList DisplayList => Parent.Helper.DisplayList;

		public CodeDescriptionPairList StyleList => Parent.Helper.StyleList;
	}
}
