using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class QuickPackItemLookups : ZLookups
	{
		public QuickPackItemLookups(QuickPackItem parent) : base(parent)
		{
		}

		public new QuickPackItem Parent => (QuickPackItem)base.Parent;

		public CodeDescriptionPairList QuickPackSeqList
		{
			get
			{
				var list = Parent.PackableItem?.PackingList?.PackageJob?.Packages?.QuickPackSeqList;
				return list ?? Factory.GetCachedValue("QuickPackSeqListEmpty", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair("0", ResString.GetMultilingualString("5C0CBA26-9541-4B57-9127-F52CA51E54AA", "0 - New Pack #"));
					return result;
				});
			}
		}
	}
}
