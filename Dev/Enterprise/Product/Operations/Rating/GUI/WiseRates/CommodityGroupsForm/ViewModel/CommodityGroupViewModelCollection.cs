using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.GUI
{
	public class CommodityGroupViewModelCollection : NonPersistentBusinessObjectCollection<CommodityGroupViewModel>
	{
		public void Add(ZString universalGroup, ZString universalGroupDescription, ZString commodityCode, ZString commodityDescription)
		{
			var commodityGroup = AddNew();
			commodityGroup.UniversalGroup = universalGroup;
			commodityGroup.UniversalGroupDescription = universalGroupDescription;
			commodityGroup.CommodityCode = commodityCode;
			commodityGroup.CommodityDescription = commodityDescription;
		}
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CommodityGroupViewModel();
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
	}
}
