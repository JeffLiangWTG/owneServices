using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.eTail.Business.Testing
{
	public class HVLVBookingHeaderTestLoadlistCreatorTest : TestCaseWithFactory
	{
		public void TestCreateLoadList_ShouldCreateLoadListWithHVLVOuterPackage()
		{
			var factory = new BusinessObjectFactory();
			var bookingHeader = factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.Items.AddNew();
			factory.Save();

			var loadList = HVLVBookingHeaderTestLoadlistCreator.CreateLoadList(bookingHeader, true);
			AssertNotNull(loadList);

			var factory1 = new BusinessObjectFactory();
			var outerPackages = factory1.Load<HVLVOuterPackage>(new ZQuery());
			AssertEquals("1 outerPackage has been saved", 1, outerPackages.Length);

			var items = factory1.Load<HVLVItem>(new ZQuery());
			AssertEquals("1 item has been saved", 1, items.Length);
			AssertEquals("the item has been linked to the outerpackage", items.First().HVI_HVO_OuterPackage, outerPackages.First().PK);
			AssertEquals("the item has been linked to the loadlist", items.First().HVI_HVL_LoadList, loadList.PK);
		}

		public void TestCreateLoadList_ShouldCreateLoadListWithoutHVLVOuterPackage()
		{
			var factory = new BusinessObjectFactory();
			var bookingHeader = factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.Items.AddNew();
			factory.Save();

			var loadList = HVLVBookingHeaderTestLoadlistCreator.CreateLoadList(bookingHeader, false);
			AssertNotNull(loadList);

			var factory1 = new BusinessObjectFactory();

			var items = factory1.Load<HVLVItem>(new ZQuery());
			AssertEquals("1 item has been saved", 1, items.Length);
			AssertEquals("the item has not been linked to the outerpackage", ZGuid.Empty, items.First().HVI_HVO_OuterPackage);
			AssertEquals("the item has been linked to the loadlist", items.First().HVI_HVL_LoadList, loadList.PK);
		}
	}
}
