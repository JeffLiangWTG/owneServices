using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class JobSupplierBookingDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestBasicFieldMappings()
		{
			var supplierBooking = JobSupplierBookingDataObjectHelperTest.BuildJobSupplierBookingForTest(Factory);
			Factory.SaveForTesting();

			var manager = new JobSupplierBookingDataContextManager();
			var writer = (manager as IShipmentDataContextManager).GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, supplierBooking))) as JobSupplierBookingDataObjectWriter;
			var dataObject = writer.GetDataObject(supplierBooking);

			AssertNotNull(dataObject);
			AssertEquals(supplierBooking.JSB_BookingId, dataObject.DataContext?.DataSourceCollection?.FirstOrDefault(source => source.Type.GetValueOrDefault() == nameof(DataContextType.JobSupplierBooking)).Key);
			AssertEquals("Good Desc", dataObject.GoodsDescription);
			AssertEquals("Marks & Numbers", dataObject.MarksAndNumbers);
			AssertEquals("PLC", dataObject.ShipmentStatus.Code);
			AssertEquals("CNCAN", dataObject.PortOfDischarge.Code);
			AssertEquals("AUSYD", dataObject.PortOfLoading.Code);
			AssertEquals("FCL", dataObject.ContainerMode.Code);
			AssertEquals((ZDecimal)15, dataObject.TotalVolume);
			AssertEquals("M3", dataObject.TotalVolumeUnit.Code);
			AssertEquals((ZDecimal)12, dataObject.TotalWeight);
			AssertEquals("KG", dataObject.TotalWeightUnit.Code);
			AssertEquals("AUMEL", dataObject.PortOfOrigin.Code);
			AssertEquals("SGSIN", dataObject.PortOfDestination.Code);
			AssertEquals("EXW", dataObject.ShipmentIncoTerm.Code);
			AssertEquals("SEA", dataObject.TransportMode.Code);
			AssertEquals("CFS", dataObject.LoadMode.Code);
			AssertEquals("BKSIN", JobSupplierBookingDataObjectHelper.FindByDocAddressType(dataObject, DocAddressType.BookingPartyDocumentaryAddress).OrganizationCode);
			AssertEquals("CCSZX", JobSupplierBookingDataObjectHelper.FindByDocAddressType(dataObject, DocAddressType.ControllingCustomer).OrganizationCode);
			AssertEquals("SPSIN", JobSupplierBookingDataObjectHelper.FindByDocAddressType(dataObject, DocAddressType.SupplierDocumentaryAddress).OrganizationCode);
			AssertEquals("LCCFS", JobSupplierBookingDataObjectHelper.FindByDocAddressType(dataObject, DocAddressType.LocalCartageCFS).OrganizationCode);
			AssertEquals("CNSHA", JobSupplierBookingDataObjectHelper.FindByDocAddressType(dataObject, DocAddressType.ConsigneeDocumentaryAddress).OrganizationCode);
			AssertEquals("CCCcC", JobSupplierBookingDataObjectHelper.FindByDocAddressType(dataObject, DocAddressType.ArrivalCFSAddress).OrganizationCode);
			AssertNull(dataObject.ContainerCollection);

			AssertEquals("Detailed Good Desc", dataObject.NoteCollection.First(note => note.Description.GetValueOrDefault() == PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description).NoteText);

			var customFieldsString = dataObject.CustomizedFieldCollection.Select(f => f.Key + " - " + f.Value).ToList();
			Assert("Custom Field 1 not found", customFieldsString.Contains("STR1 - ME TOO"));
			Assert("Custom Field 2 not found", customFieldsString.Contains("DAT1 - 2022-02-27T00:00:00"));
			Assert("Custom Field 3 not found", customFieldsString.Contains("DEC1 - 12.34"));
			Assert("Custom Field 4 not found", customFieldsString.Contains("INT1 - 12"));
		}

		public void TestCYSpecificMappings()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.ContainerYard;

			var plannedContainer1 = supplierBooking.PlannedContainers.AddNew();
			plannedContainer1.J1_ContainerCount = 1;
			plannedContainer1.J1_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			var plannedContainer2 = supplierBooking.PlannedContainers.AddNew();
			plannedContainer2.J1_ContainerCount = 2;
			plannedContainer2.J1_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "JK001";
			var containe1 = consol.Containers.AddNew();
			containe1.JC_ContainerNum = "CON001";
			containe1.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			var containe2 = consol.Containers.AddNew();
			containe2.JC_ContainerNum = "CON002";
			containe2.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;
			var containe3 = consol.Containers.AddNew();
			containe3.JC_ContainerNum = "CON003";
			containe3.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;
			var containe4 = consol.Containers.AddNew();
			containe4.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;
			containe4.JC_ContainerCount = 3;
			supplierBooking.Containers.Add(containe1);
			supplierBooking.Containers.Add(containe2);
			supplierBooking.Containers.Add(containe3);
			supplierBooking.Containers.Add(containe4);

			Factory.SaveForTesting();

			var manager = new JobSupplierBookingDataContextManager();
			var writer = (manager as IShipmentDataContextManager).GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, supplierBooking))) as JobSupplierBookingDataObjectWriter;
			var dataObject = writer.GetDataObject(supplierBooking);

			AssertEquals(2, dataObject.ContainerCollection.Count);
			AssertEquals("20GP", dataObject.ContainerCollection[0].ContainerType.Code);
			AssertEquals(1, dataObject.ContainerCollection[0].ContainerCount);
			AssertEquals("40GP", dataObject.ContainerCollection[1].ContainerType.Code);
			AssertEquals(2, dataObject.ContainerCollection[1].ContainerCount);

			var assertJobContainer = (Shipment shipmentObject, ZString contaienrTypeCode, ZString countainerNumber, int containerCount) =>
			{
				AssertEquals(contaienrTypeCode, shipmentObject.ContainerCollection[0].ContainerType.Code);
				AssertEquals(countainerNumber, shipmentObject.ContainerCollection[0].ContainerNumber);
				AssertEquals(containerCount, shipmentObject.ContainerCollection[0].ContainerCount);
			};

			AssertEquals(4, dataObject.RelatedShipmentCollection.Count);
			assertJobContainer(dataObject.RelatedShipmentCollection[0], "20GP", "CON001", 1);
			assertJobContainer(dataObject.RelatedShipmentCollection[1], "40GP", "CON002", 1);
			assertJobContainer(dataObject.RelatedShipmentCollection[2], "40GP", "CON003", 1);
			assertJobContainer(dataObject.RelatedShipmentCollection[3], "40GP", ZString.Empty, 3);
		}
	}
}
