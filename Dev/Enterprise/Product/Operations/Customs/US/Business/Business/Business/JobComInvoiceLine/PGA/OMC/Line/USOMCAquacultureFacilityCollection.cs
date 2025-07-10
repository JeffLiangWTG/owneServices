using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class USOMCAquacultureFacilityCollection : DependentCusAddInfoCollection<USOMCAquacultureFacility, BusinessObject>
	{
		public USOMCAquacultureFacilityCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USOMCDetails)
		{
		}

		protected override bool AllowNewCore
		{
			get
			{
				var master = Master as OMCHeader;
				return master != null && master.IsAquacultureFacilityRequired;
			}
		}
	}
}
