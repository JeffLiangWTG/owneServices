using System.Linq;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class ContainerLoadListDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestBasicFieldMappingsForCY()
		{
			var containerLoadList = ContainerLoadListDataObjectHelper.BuildDataForTest(Factory, Core.Constants.SupplierBookingLoadMode.ContainerYard) as CYContainerLoadList;
			Factory.SaveForTesting();

			var manager = new ContainerLoadListDataContextManager();
			var writer = (manager as IShipmentDataContextManager).GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, containerLoadList)));
			var dataObject = writer.GetDataObject(containerLoadList) as UniversalShipment;

			AssertNotNull(dataObject);
			AssertEquals("Goods Desc", dataObject.GoodsDescription);
			AssertEquals("Marks&Nos", dataObject.MarksAndNumbers);
			AssertEquals("CLL0001", dataObject.GetMatchingDataSource(DataContextType.ContainerLoadList).Key);
			AssertEquals(SupplierBookingLoadModeList.Codes.CY, dataObject.LoadMode.Code);
			AssertEquals(Core.Constants.ContainerLoadListHeaderStatus.Incomplete, dataObject.ShipmentStatus.Code);
			AssertEquals("BKT001", JobSupplierBookingDataObjectHelper.FindByDocAddressType(dataObject, DocAddressType.BookingPartyDocumentaryAddress).OrganizationCode);

			AssertEquals(1, containerLoadList.Booking.Containers.Count);
			AssertEquals("SBK001", dataObject.RelatedShipmentCollection[0].GetMatchingDataSource(DataContextType.JobSupplierBooking).Key);

			AssertEquals("Detailed Good Desc", dataObject.NoteCollection.First(note => note.Description.GetValueOrDefault() == PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description).NoteText);

			AssertContainers(dataObject, "CON001", "MWLF9771112", "CY/CY", "LCL");
		}

		static void AssertContainers(UniversalShipment dataObject, string consolKey, string containerNumber, string deliveryMode, string containerMode)
		{
			AssertEquals(1, dataObject.ParentShipmentCollection.Count);
			AssertEquals(consolKey, dataObject.ParentShipmentCollection.Single().GetMatchingDataSource(DataContextType.ForwardingConsol).Key);
			var containerDataObject = dataObject.ParentShipmentCollection.Single().ContainerCollection.Single();
			AssertEquals(1, containerDataObject.Link);
			AssertEquals(1, containerDataObject.ContainerCount);
			AssertEquals(containerNumber, containerDataObject.ContainerNumber);
			AssertEquals("20GP", containerDataObject.ContainerType.Code);
			AssertEquals(deliveryMode, containerDataObject.DeliveryMode);
			AssertEquals(containerMode, containerDataObject.FCL_LCL_AIR.Code);
		}
	}
}
