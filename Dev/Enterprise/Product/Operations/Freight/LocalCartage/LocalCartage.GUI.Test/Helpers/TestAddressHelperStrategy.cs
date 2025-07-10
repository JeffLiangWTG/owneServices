using CargoWise.EntityFramework;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class TestAddressHelperStrategy : QuickAddressHelperStrategy
	{
		public TestAddressHelperStrategy(CartageBindToLists.AddressSelectionElement[] addressElements, IDocAddresses parent, ZPropertyInfo info, DocAddressType defaultAddressType, BusinessObjectFactory factory) : base(factory)
		{
			this.addressElements = addressElements;
			this.info = info;
			this.parent = parent;
			this.defaultAddressType = defaultAddressType;
		}

		readonly CartageBindToLists.AddressSelectionElement[] addressElements;
		readonly ZPropertyInfo info;
		readonly IDocAddresses parent;
		readonly DocAddressType defaultAddressType;
		public override CartageBindToLists.AddressSelectionElement[] GetAddressElements()
		{
			return addressElements;
		}

		public override ZPropertyInfo Info
		{
			get
			{
				return info;
			}
		}

		public override IDocAddresses Parent
		{
			get
			{
				return parent;
			}
		}

		public override DocAddressType DefaultAddressType
		{
			get
			{
				return defaultAddressType;
			}
		}
	}
}
