using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USDeliveryOrderContainerAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestWeightUQList()
		{
			AssertEquals("WeightUQList", OLookUpEditType.Weight, Container.AddInfoLookups.WeightUQList.LookupEditType);
		}

		public void TestPackageTypeList()
		{
			AssertEquals("PackageTypeList", Factory.GetCachedValue<ShippingOrPackingingUnitList>(), Container.AddInfoLookups.PackageTypeList);
		}

		public void TestContainerList()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "TURE2352467";
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			CusContainer container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "KEHD2352467";
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.BreakBulk;
			CusContainer container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "LEGE2352467";
			container3.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			DeliveryOrderHeader order1 = declaration.DeliveryOrderHeaders.AddNew();
			order1.DeliveryOrderContainers.DeleteAll();
			DeliveryOrderContainer orderContainer1 = order1.DeliveryOrderContainers.AddNew();
			orderContainer1.US_ContainerNumber = "KEHD2352467";
			DeliveryOrderContainer orderContainer2 = order1.DeliveryOrderContainers.AddNew();
			DeliveryOrderHeader order2 = declaration.DeliveryOrderHeaders.AddNew();
			DeliveryOrderContainer orderContainer3 = order2.DeliveryOrderContainers.AddNew();
			CodeDescriptionPairList list = orderContainer1.AddInfoLookups.ContainerList;
			AssertEquals(3, list.Count);
			AssertEquals(true, list.ContainsCode("TURE2352467"));
			AssertEquals(true, list.ContainsCode("KEHD2352467"));
			AssertEquals(true, list.ContainsCode("LEGE2352467"));
			list = orderContainer2.AddInfoLookups.ContainerList;
			AssertEquals(2, list.Count);
			AssertEquals(true, list.ContainsCode("TURE2352467"));
			AssertEquals(true, list.ContainsCode("LEGE2352467"));
			list = orderContainer3.AddInfoLookups.ContainerList;
			AssertEquals(2, list.Count);
			AssertEquals(true, list.ContainsCode("TURE2352467"));
			AssertEquals(true, list.ContainsCode("LEGE2352467"));
			orderContainer3.US_ContainerNumber = "LEGE2352467";
			list = orderContainer1.AddInfoLookups.ContainerList;
			AssertEquals(2, list.Count);
			AssertEquals(true, list.ContainsCode("TURE2352467"));
			AssertEquals(true, list.ContainsCode("KEHD2352467"));
			list = orderContainer2.AddInfoLookups.ContainerList;
			AssertEquals(1, list.Count);
			AssertEquals(true, list.ContainsCode("TURE2352467"));
			list = orderContainer3.AddInfoLookups.ContainerList;
			AssertEquals(2, list.Count);
			AssertEquals(true, list.ContainsCode("TURE2352467"));
			AssertEquals(true, list.ContainsCode("LEGE2352467"));
		}

		public void TestContainerModeList()
		{
			CodeDescriptionPairList list = Container.AddInfoLookups.ContainerModeList;
			CodeDescriptionPairList expectedList = Declaration.CusContainers.AddNew().Lookups.CO_FCL_LCL_NCT_List;
			AssertEquals(expectedList.Count, list.Count);
			foreach (CodeDescriptionPair pair in expectedList)
			{
				AssertEquals(pair.Description, list.GetDescriptionFromCode(pair.Code));
			}
		}

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());

		DeliveryOrderContainer container;
		DeliveryOrderContainer Container
		{
			get
			{
				if (container == null)
				{
					var header = Declaration.DeliveryOrderHeaders.AddNew();
					container = header.DeliveryOrderContainers.AddNew();
				}
				return container;
			}
		}
	}
}
