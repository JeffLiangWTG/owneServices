using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DeliveryOrderContainerCollection))]
	sealed class DeliveryOrderContainerCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCopyContainersFromBill()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill bill1 = declaration.Bills.AddNew();
			bill1.CU_BillNum = "HB23432223";
			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillNum = "HB92342345";
			CusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "TURE2352467";
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			CusContainer container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "KEHD2352467";
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.BreakBulk;
			CusContainer container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "LEGE2352467";
			container3.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			bill1.Containers.Add(container2);
			bill2.Containers.Add(container3);
			bill2.Containers.Add(container1);
			var packingGroup1 = container2.PackingGroups.AddNew();
			var package1 = packingGroup1.Packages.AddNew();
			package1.CW_ContainerNoOrEquipmentNo = "KEHD2352467";
			package1.CW_PackQty = 12;
			package1.CW_PackType = ShippingOrPackingingUnitList.Codes.Bag;
			var package2 = packingGroup1.Packages.AddNew();
			package2.CW_ContainerNoOrEquipmentNo = "KEHD2352467";
			package2.CW_PackQty = 15;
			package2.CW_PackType = ShippingOrPackingingUnitList.Codes.Bag;
			var packingGroup2 = container3.PackingGroups.AddNew();
			var package3 = packingGroup2.Packages.AddNew();
			package3.CW_ContainerNoOrEquipmentNo = "LEGE2352467";
			package3.CW_PackQty = 12;
			package3.CW_PackType = ShippingOrPackingingUnitList.Codes.Bag;
			var package4 = packingGroup2.Packages.AddNew();
			package4.CW_ContainerNoOrEquipmentNo = "LEGE2352467";
			package4.CW_PackQty = 12;
			package4.CW_PackType = ShippingOrPackingingUnitList.Codes.Carton;
			var packingGroup3 = container1.PackingGroups.AddNew();
			var package5 = packingGroup3.Packages.AddNew();
			package5.CW_ContainerNoOrEquipmentNo = "TURE2352467";
			package5.CW_PackQty = 10;
			package5.CW_PackType = ShippingOrPackingingUnitList.Codes.Carton;
			DeliveryOrderHeader order = declaration.DeliveryOrderHeaders.AddNew();
			order.DeliveryOrderContainers.RemoveAndDeleteAll();
			order.DeliveryOrderContainers.CopyContainersIfMissing(bill1.Containers.ToArray<CusContainer>());
			AssertEquals(1, order.DeliveryOrderContainers.Count);
			DeliveryOrderContainer orderContainer = order.DeliveryOrderContainers[0];
			AssertDeliveryOrderContainer(orderContainer, "KEHD2352467", Core.Constants.ContainerModes.BreakBulk, ShippingOrPackingingUnitList.Codes.Bag, 27);
			orderContainer.US_ContainerNumber = "LEGE2352467";
			orderContainer.US_ContainerMode = Core.Constants.ContainerModes.Bulk;
			order.DeliveryOrderContainers.CopyContainersIfMissing(bill2.Containers.ToArray<CusContainer>());
			AssertEquals(2, order.DeliveryOrderContainers.Count);
			orderContainer = order.DeliveryOrderContainers[0];
			AssertDeliveryOrderContainer(orderContainer, "LEGE2352467", Core.Constants.ContainerModes.Bulk, ZString.Empty, 0);
			orderContainer = order.DeliveryOrderContainers[1];
			AssertDeliveryOrderContainer(orderContainer, "TURE2352467", Core.Constants.ContainerModes.FCL, ShippingOrPackingingUnitList.Codes.Carton, 10);
		}

		void AssertDeliveryOrderContainer(DeliveryOrderContainer orderContainer, ZString containerNumber, ZString containerMode, ZString packageType, ZInt noOfPacks)
		{
			AssertEquals(containerNumber, orderContainer.US_ContainerNumber);
			AssertEquals(containerMode, orderContainer.US_ContainerMode);
			AssertEquals(packageType, orderContainer.US_PackageType);
			AssertEquals(noOfPacks, orderContainer.US_NoOfPackages);
		}

		public void TestIndexer_ContainerNumber()
		{
			DeliveryOrderContainer container1 = DeliveryOrderHeader.DeliveryOrderContainers.AddNew();
			container1.US_ContainerNumber = "TURE1232122";
			DeliveryOrderContainer container2 = DeliveryOrderHeader.DeliveryOrderContainers.AddNew();
			container2.US_ContainerNumber = "ABDD1232122";
			DeliveryOrderContainer container3 = DeliveryOrderHeader.DeliveryOrderContainers.AddNew();
			container3.US_ContainerNumber = "KHJD1232122";
			AssertEquals(container3, DeliveryOrderHeader.DeliveryOrderContainers["KHJD1232122"]);
			AssertEquals(container1, DeliveryOrderHeader.DeliveryOrderContainers["TURE1232122"]);
			AssertEquals(container2, DeliveryOrderHeader.DeliveryOrderContainers["ABDD1232122"]);
			AssertNull(DeliveryOrderHeader.DeliveryOrderContainers["LJDS1232122"]);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new DeliveryOrderContainerCollection(DeliveryOrderHeader);

		DeliveryOrderHeader deliveryOrderHeader;
		DeliveryOrderHeader DeliveryOrderHeader => deliveryOrderHeader ?? (deliveryOrderHeader = Factory.New<DeliveryOrderHeader>());
	}
}
