using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	[TestedType(typeof(AgencyShipmentContainerDataContextManager))]
	sealed class AgencyShipmentContainerDataContextManagerTest : ShipmentDataContextManagerTestCase<AgencyShipmentContainerDataContextManager, AgencyShipmentContainer>
	{
		public void TestDataContextType()
		{
			AssertEquals(DataContextType.AgencyShipmentContainer, GetNewDataContextManager().DataContextType);
		}

		public void TestDataContextKey()
		{
			var billOfLading = Factory.New<BillOfLading>();
			var container = billOfLading.ShippingContainers.AddNew();
			container.JC_ContainerJobID = "D00001000";
			var manager = GetNewDataContextManager();
			((IDataContextManager)manager).Init(container);
			AssertEquals("D00001000", manager.DataContextKey);
		}

		public void TestGetDataContextKeyMatchingQuery_BillOfLadingContainer()
		{
			var billOfLading = Factory.New<BillOfLading>();
			var container = billOfLading.ShippingContainers.AddNew();
			container.JC_ContainerJobID = "D00001000";
			Factory.SaveForTesting();
			AssertGetDataContextKeyMatchingQuery(container);
		}

		public void TestGetDataContextKeyMatchingQuery_BillOfLadingContainer_ImportRow()
		{
			var factory = new BusinessObjectFactory();
			var billOfLading = factory.New<BillOfLading>();
			var container = billOfLading.ShippingContainers.AddNew();
			container.JC_ContainerJobID = "D00001000";
			factory.Save();
			AssertGetDataContextKeyMatchingQuery(container);
		}

		public void TestGetDataContextKeyMatchingQuery_BillOfLadingContainer_MultipleContainersAroundRow()
		{
			var billOfLading = Factory.New<BillOfLading>();
			var container = billOfLading.ShippingContainers.AddNew();
			container.JC_ContainerJobID = "D00001000";
			Factory.SaveForTesting();
			var agencyContainer = Factory.BOFactory.Load<AgencyShipmentContainer>(container.PK);
			AssertContainsExactElementsInAnyOrder("prerequisite", new[] { container, agencyContainer }, Factory.BOFactory.GetBizOsForPK(container.PK.ToGuid()));
			AssertGetDataContextKeyMatchingQuery(container, false);
		}

		public void TestGetDataContextKeyMatchingQuery_BillOfLadingContainer_MultipleContainersAroundRow_AgencyContainerLoaded()
		{
			var factory = new BusinessObjectFactory();
			var billOfLading = factory.New<BillOfLading>();
			var container = billOfLading.ShippingContainers.AddNew();
			container.JC_ContainerJobID = "D00001000";
			factory.Save();
			var agencyContainer = Factory.BOFactory.Load<AgencyShipmentContainer>(container.PK);
			AssertContainsExactElementsInAnyOrder("prerequisite", new[] { agencyContainer }, Factory.BOFactory.GetBizOsForPK(container.PK.ToGuid()));
			AssertGetDataContextKeyMatchingQuery(container, false);
		}

		public void TestGetDataContextKeyMatchingQuery_AgencyBookingContainer()
		{
			var booking = Factory.New<AgencyBooking>();
			var container = booking.ShippingContainers.AddNew();
			container.JC_ContainerJobID = "D00001000";
			Factory.SaveForTesting();
			AssertGetDataContextKeyMatchingQuery(container);
		}

		public void TestGetDataContextKeyMatchingQuery_AgencyBookingContainerDescendant()
		{
			var booking = Factory.New<AgencyBooking>();
			var container = Factory.New<AgencyBookingContainerForTest>();
			container.JC_ContainerJobID = "D00001000";
			booking.ShippingContainers.Add(container);
			Factory.SaveForTesting();
			AssertGetDataContextKeyMatchingQuery(container);
		}

		public void TestGetDataContextKeyMatchingQuery_AgencyBookingContainer_ImportRow()
		{
			var factory = new BusinessObjectFactory();
			var booking = factory.New<AgencyBooking>();
			var container = booking.ShippingContainers.AddNew();
			container.JC_ContainerJobID = "D00001000";
			factory.Save();
			AssertGetDataContextKeyMatchingQuery(container);
		}

		public void TestGetDataContextKeyMatchingQuery_AgencyBookingContainer_MultipleContainersAroundRow()
		{
			var booking = Factory.New<AgencyBooking>();
			var container = booking.ShippingContainers.AddNew();
			container.JC_ContainerJobID = "D00001000";
			Factory.SaveForTesting();
			var agencyContainer = Factory.BOFactory.Load<AgencyShipmentContainer>(container.PK);
			AssertContainsExactElementsInAnyOrder("prerequisite", new[] { container, agencyContainer }, Factory.BOFactory.GetBizOsForPK(container.PK.ToGuid()));
			AssertGetDataContextKeyMatchingQuery(container, false);
		}

		public void TestGetDataContextKeyMatchingQuery_AgencyBookingContainer_MultipleContainersAroundRow_AgencyContainerLoaded()
		{
			var factory = new BusinessObjectFactory();
			var booking = factory.New<AgencyBooking>();
			var container = booking.ShippingContainers.AddNew();
			container.JC_ContainerJobID = "D00001000";
			factory.Save();
			var agencyContainer = Factory.BOFactory.Load<AgencyShipmentContainer>(container.PK);
			AssertContainsExactElementsInAnyOrder("prerequisite", new[] { agencyContainer }, Factory.BOFactory.GetBizOsForPK(container.PK.ToGuid()));
			AssertGetDataContextKeyMatchingQuery(container, false);
		}

		void AssertGetDataContextKeyMatchingQuery(AgencyShipmentContainer container, bool checkForExtraBizObjAroundRow = true)
		{
			var manager = GetNewDataContextManager();
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.CodesMappedToTarget = true;
			dataObject.DataContext.AddDataTarget(DataContextType.AgencyShipmentContainer, container.JC_ContainerJobID);
			var dataTarget = dataObject.DataContext.DataTargetCollection.FirstOrDefault();
			var loadedContainer = ((IDataContextManager)manager).LoadBusinessObjectFromDataTarget(dataObject, dataTarget, Factory.BOFactory, new DummyLogger());
			AssertEquals("containers PK match", container.PK, loadedContainer.PK);
			AssertEquals("containers type match", container.GetType(), loadedContainer.GetType());
			if (checkForExtraBizObjAroundRow)
			{
				AssertContainsExactElementsInAnyOrder("no extra biz object around row", new[] { loadedContainer }, Factory.BOFactory.GetBizOsForPK(loadedContainer.PK.ToGuid()));
			}
		}

		public void TestManagesShipmentsManagesEvents()
		{
			var manager = GetNewDataContextManager();
			AssertEquals(true, manager.ManagesShipments);
			AssertEquals(true, manager.ManagesEvents);
		}

		public void TestDefaultOutputDirectory()
		{
			const string testDirectory = @"c:\Test"; // constant for test
			SystemDataRegistry.Instance.ShipmentExportDirectory.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testDirectory);
			AssertEquals(testDirectory, GetNewDataContextManager().DefaultOutputDirectory);
		}

		public void TestPopulateUniversalEventContextReferences()
		{
			var container = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory.BOFactory, "TEST4100013", "", "AUSYD", "USSFO", "USS ESSES", "001", "12345", "HBL001", "MCLAREN", "S001");
			container.JC_ReleaseNum = "RN2323";
			Factory.SaveForTesting();
			var manager = GetNewDataContextManager();
			((IDataContextManager)manager).Init(container);
			AssertContainsExactElementsInAnyOrder(new[] { "CarriersBookingReference|MCLAREN", "MBOLNumber|HBL001", "LloydsNumber|12345", "VesselName|USS ESSES", "VoyageNumber|001", "LegOriginUNLOCO|AUSYD", "LegDestinationUNLOCO|USSFO", "ContainerNumber|TEST4100013", "ContainerISOCode|22G0", "ContainerReleaseNumber|RN2323" }, Format(manager.EventContextValues));
		}

		public void TestPopulateUniversalEventContextReferences_TopLevelPack()
		{
			var container = AgencyShipmentContainerUniversalTestHelper.CreateTopLevelPackForAgencyShipment(Factory.BOFactory, "TEST4100013", "AUSYD", "USSFO", "USS ESSES", "001", "12345", "HBL001", "MCLAREN", "001", Constants.ContainerModes.RollOnRollOff);
			Factory.SaveForTesting();
			var manager = GetNewDataContextManager();
			((IDataContextManager)manager).Init(container);
			AssertContainsExactElementsInAnyOrder(new[] { "CarriersBookingReference|MCLAREN", "MBOLNumber|HBL001", "LloydsNumber|12345", "VesselName|USS ESSES", "VoyageNumber|001", "LegOriginUNLOCO|AUSYD", "LegDestinationUNLOCO|USSFO", "GoodsItemID|TEST4100013" }, Format(manager.EventContextValues));
		}

		protected override AgencyShipmentContainer GetNewBusinessObjectForTesting()
		{
			var booking = Factory.New<AgencyBooking>();
			return booking.BookedContainers.AddNew();
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				using (var retriever = new EmbeddedResourceRetriever())
				{
					return retriever.GetString("Enterprise.Freight.Agency.DataTransfer.Test.Universal.AgencyShipmentContainer.TestFiles.FullyPopulatedBillOfLadingContainer_UniversalShipment.xml");
				}
			}
		}

		IEnumerable<string> Format(IEnumerable<KeyValuePair<TypeWithDescription, IZType>> contextValues) => contextValues.Select(Format);

		string Format(KeyValuePair<TypeWithDescription, IZType> contextValue) => string.Format("{0}|{1}", contextValue.Key.Type, contextValue.Value);

		AgencyShipmentContainerDataContextManager GetNewDataContextManager() => new AgencyShipmentContainerDataContextManager();

		sealed class AgencyBookingContainerForTest : AgencyBookingContainer
		{
			public AgencyBookingContainerForTest(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
			{
			}
		}
	}
}
