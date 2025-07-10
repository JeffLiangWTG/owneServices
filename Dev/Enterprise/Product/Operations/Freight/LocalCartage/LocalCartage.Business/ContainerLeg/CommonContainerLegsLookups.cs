using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonCartageLegLookups : JobContainerLegsLookups
	{
		public CommonCartageLegLookups(CommonCartageLeg parent)
			: base(parent) { }

		public CodeDescriptionPairList CartageAddressList
		{
			get { return CartageBindToLists.CartageAddressList(CartageLeg.Cartage); }
		}

		public CartageBindToLists.AddressSelectionElement[] GetCartageAddressElements()
		{
			return CartageBindToLists.CartageAddressElements(CartageLeg.Cartage);
		}

		public ModuleCartageRunSheetCollection WorkSheets
		{
			get { return new ModuleCartageRunSheetCollection(Factory); }
		}

		CommonCartageLeg CartageLeg
		{
			get { return (CommonCartageLeg)Parent; }
		}

		public CartageBindToLists CartageBindToLists
		{
			get { return Factory.GetCachedValue("CartageBindToLists", () => new CartageBindToLists(Factory)); }
		}
	}
}
