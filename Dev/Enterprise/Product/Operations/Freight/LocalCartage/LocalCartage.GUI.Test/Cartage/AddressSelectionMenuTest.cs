using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	class AddressSelectionMenuTest : TestCaseWithFactory
	{
		public void TestAddressSelectionMenu()
		{
			CartageBindToLists.AddressSelectionElement element = new CartageBindToLists.AddressSelectionElement(ZGuid.NewZGuid(), DocAddressType.LocalCartageImporter, "ORGCODE", "ADDRESSSHORTCODE", "Address as single line");
			AddressSelectionMenu menu = new AddressSelectionMenu(element);
			AssertEquals("ORGCODE :: ADDRESSSHORTCODE :: Address as single line", menu.Text);
		}
	}
}
