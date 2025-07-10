using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.Access.Business
{
	public class AsycudaPackedItemLookups : ASYCUDA.Business.AsycudaPackedItemLookups
	{
		public AsycudaPackedItemLookups(AsycudaPackedItem parent)
			: base(parent)
		{
		}

		protected new AsycudaPackedItem Parent => (AsycudaPackedItem)base.Parent;

		public CodeDescriptionPairList GoodsTypeList
		{
			get
			{
				var isExport = Parent.Header?.IsExport ?? false;
				return Factory.GetCachedValue("SGAccessGoodsTypeList" + isExport, () =>
				{
					var list = new UntranslatableCodeDescriptionPairList(
						(NoResString)"GetGoodsTypeListForCountry fetches strings from the database and does not need translation");

					list.AddRange(GetGoodsTypeListForCountry(Factory, Core.Constants.CountryCodes.Singapore));

					if (isExport)
					{
						list.RemoveCode(Constants.GoodsType.MajorExporter);
						list.RemoveCode(Constants.GoodsType.DutiableGoods);
					}
					else
					{
						list.AddPairIfNotExist(Constants.GoodsType.MajorExporter, "Major Exporter");
						list.AddPairIfNotExist(Constants.GoodsType.DutiableGoods, "Dutiable Goods");
					}

					return list;
				});
			}
		}

		public CodeDescriptionPairList YesNoList => Factory.GetCachedValue<YesNoList>();
	}
}

