using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class BaseSupplementaryCodeLookups : CusCodeDataLookups
	{
		public BaseSupplementaryCodeLookups(BaseSupplementaryCode parent)
				: base(parent)
		{
		}

		protected new BaseSupplementaryCode Parent => (BaseSupplementaryCode)base.Parent;

		public override CodeDescriptionPairList CY_CodeList
		{
			get
			{
				CodeDescriptionPairList result;
				if (Parent.Parent is ISupplementaryCodeSupporter supporter)
				{
					result = UniversalReferenceDataHelper.GetDynamicRateApplicabilityCodeList(supporter.Tariff, supporter.CachedListOfAdditionalCodeDescriptions, supporter.RateSelectionCriteria);
				}
				else
				{
					result = new CodeDescriptionPairList();
				}
				return result;
			}
		}
	}
}
