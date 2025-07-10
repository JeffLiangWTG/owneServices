using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonBookedCtgMoveLookups : JobBookedCtgMoveLookups
	{
		public CommonBookedCtgMoveLookups(CommonBookedCtgMove parent)
			: base(parent) { }

		public CodeDescriptionPairList DropModes
		{
			get { return CartageBindToLists.DropModes(Move.IsContainerised); }
		}

		public CommonCartageCollection Cartages
		{
			get { return new CommonCartageCollection(Factory); }
		}

		public CodeDescriptionPairList CartageAddressList
		{
			get { return CartageBindToLists.CartageAddressList(Move.Cartage); }
		}

		public CartageBindToLists.AddressSelectionElement[] GetCartageAddressElements()
		{
			return CartageBindToLists.CartageAddressElements(Move.Cartage);
		}

		CommonBookedCtgMove Move
		{
			get { return (CommonBookedCtgMove)Parent; }
		}

		public CartageBindToLists CartageBindToLists
		{
			get { return Factory.GetCachedValue("CartageBindToLists", () => new CartageBindToLists(Factory)); }
		}
	}
}
