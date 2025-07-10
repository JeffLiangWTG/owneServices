using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefCommodityRatingCodeMapValidation : AutoRefCommodityRatingCodeMapValidation
	{
		public RefCommodityRatingCodeMapValidation(AutoRefCommodityRatingCodeMap parent) : base(parent)
		{
		}

		protected override void CheckRI_RH_NKCommodityChild()
		{
			base.CheckRI_RH_NKCommodityChild();

			MandatoryValidation.CheckEntered(Parent.RI_RH_NKCommodityChildInfo);
			ListValidation.ErrorIfInvalidCode(Parent.RI_RH_NKCommodityChildInfo);

			if (Parent.CommodityParent != null)
			{
				if (Parent.RI_RH_NKCommodityParent.Equals(Parent.RI_RH_NKCommodityChild))
				{
					Parent.RI_RH_NKCommodityChildInfo.AddError(Res.GetString("e95c6225-2c12-45ab-ae35-0aa6e0d2dfe9", "Selected commodity code cannot be the same code as parent."));
				}

				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.RI_RH_NKCommodityChildInfo, Parent.CommodityParent.RefCommodityRatingCodeMaps);
			}
		}
	}
}
