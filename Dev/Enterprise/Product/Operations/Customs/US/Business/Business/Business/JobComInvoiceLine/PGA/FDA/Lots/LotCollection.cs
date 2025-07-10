using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class LotCollection : DependentCusAddInfoCollection<Lot, BusinessObject>
	{
		public LotCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USLot)
		{
		}

		protected override bool AllowNewCore
		{
			get
			{
				var header = Master as CPSCHeader;
				return header == null || !header.IsREF;
			}
		}
	}
}
