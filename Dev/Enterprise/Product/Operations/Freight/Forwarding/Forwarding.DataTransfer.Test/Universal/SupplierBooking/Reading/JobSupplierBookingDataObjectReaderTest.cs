using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using SupplierBookingStatus = Enterprise.Core.Constants.SupplierBookingStatus;
using UniversalCodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;
using UniversalContainerType = Enterprise.UniversalDataBuss.DataObjects.Universal.ContainerType;
using UniversalCustomizedField = Enterprise.UniversalDataBuss.DataObjects.Universal.CustomizedField;
using UniversalDate = Enterprise.UniversalDataBuss.DataObjects.Universal.Date;
using UniversalDateType = Enterprise.UniversalDataBuss.DataObjects.Universal.DateType;
using UniversalLoadMode = Enterprise.UniversalDataBuss.DataObjects.Universal.LoadMode;
using UniversalOrganizationAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalUNLOCO = Enterprise.UniversalDataBuss.DataObjects.Universal.UNLOCO;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class JobSupplierBookingDataObjectReaderTest : OrganizationAddressTestHelper
	{
		public void TestBasicFieldMappingsForNoExisted()
		{
			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			bookingParty.OH_Code = "BKSIN";

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "SPSIN";
			supplier.Addresses.AddNew().FillWithValidTestData();
			supplier.Contacts.AddNew().FillWithValidTestData();

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CCSZX";
			controllingCustomer.Addresses.AddNew().FillWithValidTestData();
			controllingCustomer.Contacts.AddNew().FillWithValidTestData();

			var localCartageCFS = Factory.NewWithValidTestData<OrgHeader>();
			localCartageCFS.OH_Code = "LCCFS";
			localCartageCFS.Addresses.AddNew().FillWithValidTestData();
			localCartageCFS.Contacts.AddNew().FillWithValidTestData();

			var cfsOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			cfsOrgHeader.OH_Code = "CBCcC";
			cfsOrgHeader.Addresses.AddNew().FillWithValidTestData();

			Factory.SaveForTesting();

			var uShipment = BuildUniversalShipmentForSupplierBooking();
			uShipment.LoadMode = new UniversalLoadMode { Code = Core.Constants.SupplierBookingLoadMode.ContainerYard };
			var supplierBooking = new JobSupplierBookingDataObjectReader(uShipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertNotNull(supplierBooking);
			Assert(!supplierBooking.JSB_BookingId.IsEmpty);
			AssertEquals("PLC", supplierBooking.JSB_Status);
			AssertEquals("SGSIN", supplierBooking.JSB_RL_NKDischargePort);
			AssertEquals("AUSYD", supplierBooking.JSB_RL_NKLoadPort);
			AssertEquals("AUMEL", supplierBooking.JSB_RL_NKOrigin);
			AssertEquals("CNCAN", supplierBooking.JSB_RL_NKDestination);
			AssertEquals("EXW", supplierBooking.JSB_IncoTerm);
			AssertEquals("SEA", supplierBooking.JSB_TransportMode);
			AssertEquals(Core.Constants.SupplierBookingLoadMode.ContainerYard, supplierBooking.JSB_LoadMode);
			AssertEquals(new ZDateTime(2022, 4, 3), supplierBooking.JSB_BookedOnDate);
			AssertEquals(new ZDateTime(2022, 4, 6), supplierBooking.JSB_CargoAvailableDate);
			AssertNotNull(supplierBooking.BookingParty);
			AssertEquals("BKSIN", supplierBooking.BookingParty.OH_Code);
			AssertNotNull(supplierBooking.SupplierAddress);
			AssertEquals("SPSIN", supplierBooking.SupplierAddress.Organisation.OH_Code);
			AssertNotNull(supplierBooking.ControllingCustomerAddress);
			AssertEquals("CCSZX", supplierBooking.ControllingCustomerAddress.Organisation.OH_Code);
			AssertNotNull(supplierBooking.CFSAddress);
			AssertEquals(Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_OH, cfsOrgHeader.PK)).OA_Code, supplierBooking.CFSAddress.OA_Code);
			AssertNotNull(supplierBooking.LocalCartageCFSAddress);
			AssertEquals("LCCFS", supplierBooking.LocalCartageCFSAddress.Organisation.OH_Code);
			AssertEquals("DetailedGoodsDescription Test", supplierBooking.JSB_DetailedGoodsDescription);
			AssertEquals(2, supplierBooking.PlannedContainers.Count);
			Assert(supplierBooking.PlannedContainers.Cast<JobSupplierBookingPlannedContainer>().Any(container => container.J1_ContainerCount == 2 && container.Container.RC_Code == "20FR"));
			Assert(supplierBooking.PlannedContainers.Cast<JobSupplierBookingPlannedContainer>().Any(container => container.J1_ContainerCount == 3 && container.J1_RC.IsEmpty));

			var customFields = supplierBooking.GetUserDefinedValues();
			var customFieldsString = customFields.Select(f => f.PropertyName + " - " + f.Value).ToList();
			Assert("Custom Field 1 not found", customFieldsString.Contains("STR1 - ME"));
			Assert("Custom Field 2 not found", customFieldsString.Contains("DAT1 - 27-Jan-22 00:00:00"));
			Assert("Custom Field 3 not found", customFieldsString.Contains("DEC1 - 12.35"));
			Assert("Custom Field 4 not found", customFieldsString.Contains("INT1 - 32"));
		}

		public void TestEmptyNote()
		{
			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			bookingParty.OH_Code = "BKSIN";

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "SPSIN";
			supplier.Addresses.AddNew().FillWithValidTestData();
			supplier.Contacts.AddNew().FillWithValidTestData();

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CCSZX";
			controllingCustomer.Addresses.AddNew().FillWithValidTestData();
			controllingCustomer.Contacts.AddNew().FillWithValidTestData();

			Factory.SaveForTesting();

			var uShipment = BuildUniversalShipmentForSupplierBooking();

			uShipment.SetNoteCollection(() => new DataObjectList<UniversalDataBuss.DataObjects.Universal.Note>());

			var supplierBooking = new JobSupplierBookingDataObjectReader(uShipment, Logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var filter = new ZQuery(StmNoteSchema.ST_ParentID, supplierBooking.PK);
			filter.AddToFilter(StmNoteSchema.ST_Description, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Code);
			filter.AddToFilter(StmNoteSchema.ST_Table, supplierBooking.TableName);

			var note = Factory.LoadTop1<StmNote>(filter);
			AssertNotNull(note);
			AssertEquals("", note.ST_NoteText);
		}

		static UniversalShipment BuildUniversalShipmentForSupplierBooking()
		{
			var uShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			uShipment.DataContext = DataContextFactory.New();
			uShipment.PortOfDischarge = new UniversalUNLOCO { Code = "SGSIN" };
			uShipment.PortOfLoading = new UniversalUNLOCO { Code = "AUSYD" };
			uShipment.PortOfOrigin = new UniversalUNLOCO { Code = "AUMEL" };
			uShipment.PortOfDestination = new UniversalUNLOCO { Code = "CNCAN" };
			uShipment.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm { Code = "EXW" };
			uShipment.ShipmentStatus = new UniversalCodeDescriptionPair { Code = "PLC" };
			uShipment.TransportMode = new UniversalCodeDescriptionPair { Code = "SEA" };
			uShipment.LoadMode = new UniversalLoadMode { Code = "CFS" };
			uShipment.SetCustomizedFieldCollection(() => new List<UniversalCustomizedField>());
			uShipment.CustomizedFieldCollection.Add(UniversalCustomizedField.New("STR1", new ZString("ME")));
			uShipment.CustomizedFieldCollection.Add(UniversalCustomizedField.New("DAT1", new ZDateTime(2022, 1, 27)));
			uShipment.CustomizedFieldCollection.Add(UniversalCustomizedField.New("DEC1", new ZDecimal(12.35m)));
			uShipment.CustomizedFieldCollection.Add(UniversalCustomizedField.New("INT1", new ZInt(32)));
			uShipment.SetContainerCollection(() => new DataObjectList<UniversalContainer>());
			uShipment.SetNoteCollection(() => new DataObjectList<UniversalDataBuss.DataObjects.Universal.Note>());
			uShipment.NoteCollection.Add(new UniversalDataBuss.DataObjects.Universal.Note
			{
				Description = "Detailed Goods Description",
				NoteContext = new UniversalDataBuss.DataObjects.Universal.NoteContext { Code = "AAA", Description = "Module: A - All, Direction: A - All, Freight: A - All" },
				Visibility = new UniversalCodeDescriptionPair { Code = "PUB", Description = "CLIENT-VISIBLE" },
				NoteText = "DetailedGoodsDescription Test",
				IsCustomDescription = false
			});
			uShipment.ContainerCollection.Add(new UniversalContainer
			{
				ContainerCount = 2,
				ContainerType = new UniversalContainerType { Code = "20FR" }
			});
			uShipment.ContainerCollection.Add(new UniversalContainer
			{
				ContainerCount = 3,
				ContainerType = new UniversalContainerType { Code = "XXX" }
			});

			uShipment.SetDateCollection(() => new List<UniversalDate>());
			uShipment.DateCollection.Add(new UniversalDate { Type = UniversalDateType.BookedOnDate, Value = new ZDateTime(2022, 4, 3) });
			uShipment.DateCollection.Add(new UniversalDate { Type = UniversalDateType.CargoAvailableDate, Value = new ZDateTime(2022, 4, 6) });

			uShipment.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress>());
			uShipment.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.BookingPartyDocumentaryAddress), "BK", "SGSIN"));
			uShipment.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.Supplier), "SP", "SGSIN"));
			uShipment.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.ControllingCustomer), "CC", "CNSZX"));
			uShipment.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.LocalCartageCFS), "LC", "LCCFS"));
			uShipment.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.ArrivalCFSAddress), "CB", "CCCcC"));

			return uShipment;
		}

		public void TestBasicFieldMappingsForExisted()
		{
			var oldSupplierBooking = JobSupplierBookingDataObjectHelperTest.BuildJobSupplierBookingForTest(Factory);

			Factory.SaveForTesting();

			var uShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			uShipment.DataContext = DataContextFactory.New();
			uShipment.DataContext.AddDataTarget(DataContextType.JobSupplierBooking, "SBK001");
			uShipment.PortOfDischarge = new UniversalUNLOCO { Code = "SGSIN" };
			uShipment.PortOfLoading = new UniversalUNLOCO { Code = "AUSYD" };
			uShipment.PortOfOrigin = new UniversalUNLOCO { Code = "AUMEL" };
			uShipment.PortOfDestination = new UniversalUNLOCO { Code = "CNCAN" };
			uShipment.LoadMode = new UniversalLoadMode { Code = "CFS" };
			uShipment.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm { Code = "EXW" };
			uShipment.ShipmentStatus = new UniversalCodeDescriptionPair { Code = "PLC" };
			uShipment.TransportMode = new UniversalCodeDescriptionPair { Code = "SEA" };
			uShipment.GoodsDescription = "Good Desc";
			uShipment.MarksAndNumbers = "MN Test";

			uShipment.SetDateCollection(() => new List<UniversalDate>());
			uShipment.DateCollection.Add(new UniversalDate { Type = UniversalDateType.BookedOnDate, Value = new ZDateTime(2022, 4, 3) });
			uShipment.DateCollection.Add(new UniversalDate { Type = UniversalDateType.CargoAvailableDate, Value = new ZDateTime(2022, 4, 6) });

			uShipment.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress>());
			uShipment.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.BookingPartyDocumentaryAddress), "BK", "SGSIN"));
			uShipment.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.Supplier), "SP", "SGSIN"));
			uShipment.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.ControllingCustomer), "CC", "CNSZX"));
			uShipment.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.LocalCartageCFS), "LC", "LCCFS"));
			uShipment.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.ConsigneeDocumentaryAddress), "CD", "CNSHA"));
			uShipment.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.ArrivalCFSAddress), "CB", "CCCcC"));

			var supplierBooking = new JobSupplierBookingDataObjectReader(uShipment, Logger, Factory).ReadIntoBusinessObject();
			AssertNotNull(supplierBooking);
			AssertEquals(oldSupplierBooking.PK, supplierBooking.PK);
			AssertEquals("SBK001", supplierBooking.JSB_BookingId);
			AssertEquals("Good Desc", supplierBooking.JSB_GoodsDescription);
			AssertEquals("MN Test", supplierBooking.JSB_MarksAndNumbers);
			AssertEquals("FCL", supplierBooking.JSB_ContainerMode);
			AssertEquals("PLC", supplierBooking.JSB_Status);
			AssertEquals("SGSIN", supplierBooking.JSB_RL_NKDischargePort);
			AssertEquals("AUSYD", supplierBooking.JSB_RL_NKLoadPort);
			AssertEquals("EXW", supplierBooking.JSB_IncoTerm);
			AssertEquals("SEA", supplierBooking.JSB_TransportMode);
			AssertEquals(new ZDateTime(2022, 4, 3), supplierBooking.JSB_BookedOnDate);
			AssertEquals(new ZDateTime(2022, 4, 6), supplierBooking.JSB_CargoAvailableDate);
			AssertNotNull(supplierBooking.BookingParty);
			AssertEquals("BKSIN", supplierBooking.BookingParty.OH_Code);
			AssertNotNull(supplierBooking.SupplierAddress);
			AssertEquals("SPSIN", supplierBooking.SupplierAddress.Organisation.OH_Code);
			AssertNotNull(supplierBooking.ControllingCustomerAddress);
			AssertEquals("CCSZX", supplierBooking.ControllingCustomerAddress.Organisation.OH_Code);
			AssertNotNull(supplierBooking.LocalCartageCFSAddress);
			AssertEquals("LCCFS", supplierBooking.LocalCartageCFSAddress.Organisation.OH_Code);
			AssertNotNull(supplierBooking.ConsigneeDocumentaryAddress);
			AssertEquals("CNSHA", supplierBooking.ConsigneeDocumentaryAddress.Organisation.OH_Code);
			AssertNotNull(supplierBooking.CFSAddress);
			AssertEquals(oldSupplierBooking.CFSAddress.OA_Code, supplierBooking.CFSAddress.OA_Code);
			AssertEquals("AUMEL", supplierBooking.JSB_RL_NKOrigin);
			AssertEquals("CNCAN", supplierBooking.JSB_RL_NKDestination);
		}

		public void TestCheck()
		{
			JobSupplierBookingDataObjectHelperTest.BuildJobSupplierBookingForTest(Factory);
			Factory.SaveForTesting();

			Logger.ClearLogs();
			var uShipment = BuildUniversalShipment();
			uShipment.DataContext = DataContextFactory.New();
			uShipment.DataContext.AddDataTarget(DataContextType.JobSupplierBooking, "SBK001");
			uShipment.PortOfDischarge = null;
			new JobSupplierBookingDataObjectReader(uShipment, Logger, Factory).ReadIntoBusinessObject();
			AssertContains("Discharge Port should be not empty", Logger.GetErrors());

			Logger.ClearLogs();
			uShipment = BuildUniversalShipment();
			uShipment.DataContext = DataContextFactory.New();
			uShipment.DataContext.AddDataTarget(DataContextType.JobSupplierBooking, "SBK001");
			uShipment.PortOfLoading = null;
			new JobSupplierBookingDataObjectReader(uShipment, Logger, Factory).ReadIntoBusinessObject();
			AssertContains("Loading Port should be not empty", Logger.GetErrors());

			Logger.ClearLogs();
			uShipment = BuildUniversalShipment();
			uShipment.DataContext = DataContextFactory.New();
			uShipment.DataContext.AddDataTarget(DataContextType.JobSupplierBooking, "SBK001");
			uShipment.OrganizationAddressCollection.Clear();
			new JobSupplierBookingDataObjectReader(uShipment, Logger, Factory).ReadIntoBusinessObject();
			AssertContains("There must be one organization at least", Logger.GetErrors());

			Logger.ClearLogs();
			uShipment = BuildUniversalShipment();
			uShipment.DataContext = DataContextFactory.New();
			uShipment.DataContext.AddDataTarget(DataContextType.JobSupplierBooking, "SBK001");
			uShipment.OrganizationAddressCollection.RemoveAt(0);
			new JobSupplierBookingDataObjectReader(uShipment, Logger, Factory).ReadIntoBusinessObject();
			AssertContains("There is no matched Booking Party", Logger.GetErrors());

			Logger.ClearLogs();
			uShipment = BuildUniversalShipment();
			uShipment.LoadMode = null;
			uShipment.DataContext = DataContextFactory.New();
			uShipment.DataContext.AddDataTarget(DataContextType.JobSupplierBooking, "SBK001");
			new JobSupplierBookingDataObjectReader(uShipment, Logger, Factory).ReadIntoBusinessObject();
			AssertContains("Load Mode should not empty", Logger.GetErrors());

			Logger.ClearLogs();
			uShipment = BuildUniversalShipment();
			uShipment.LoadMode = new UniversalLoadMode { Code = "XXX" };
			uShipment.DataContext = DataContextFactory.New();
			uShipment.DataContext.AddDataTarget(DataContextType.JobSupplierBooking, "SBK001");
			new JobSupplierBookingDataObjectReader(uShipment, Logger, Factory).ReadIntoBusinessObject();
			AssertContains("Load Mode only accepts CY or CFS", Logger.GetErrors());
		}

		public void TestStatusCheck()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			supplierBooking.JSB_BookingId = "SBK001";

			Factory.SaveForTesting();

			foreach (var status in typeof(SupplierBookingStatus).GetFields().Select(field => field.GetValue(null) as string))
			{
				var uShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
				uShipment.DataContext = DataContextFactory.New();
				uShipment.DataContext.AddDataTarget(DataContextType.JobSupplierBooking, "SBK001");
				supplierBooking.JSB_Status = status;

				if (status == SupplierBookingStatus.Cancelled || status == SupplierBookingStatus.Shipped || status == SupplierBookingStatus.Converted || status == SupplierBookingStatus.Received)
				{
					AssertExceptionThrown<DataObjectReadFailureException>($"should throw exception while status is {status}", $"Supplier booking in {status} state cannot be updated.", () => new JobSupplierBookingDataObjectReader(uShipment, Logger, Factory).ReadIntoBusinessObject());
				}
				else
				{
					AssertNoExceptionThrown(() => new JobSupplierBookingDataObjectReader(uShipment, Logger, Factory).ReadIntoBusinessObject());
				}
			}
		}

		public void TestPlannedContainers()
		{
			var originalSupplierBooking = JobSupplierBookingDataObjectHelperTest.BuildJobSupplierBookingForTest(Factory);
			originalSupplierBooking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.ContainerYard;
			originalSupplierBooking.JSB_BookingId = "SBK001";
			originalSupplierBooking.JSB_Status = SupplierBookingStatus.Incomplete;
			Factory.SaveForTesting();
			AssertEquals(1, originalSupplierBooking.PlannedContainers.Count);

			var uShipment = BuildUniversalShipmentForSupplierBooking();
			uShipment.LoadMode = new UniversalLoadMode { Code = "CY" };
			uShipment.DataContext.AddDataTarget(DataContextType.JobSupplierBooking, "SBK001");
			uShipment.SetContainerCollection(() => new DataObjectList<UniversalContainer> {
				new UniversalContainer { ContainerType = new UniversalContainerType { Code = "20GP" }, ContainerCount = 2 },
				new UniversalContainer { ContainerType = new UniversalContainerType { Code = "40GP" }, ContainerCount = 3 } });

			var supplierBooking = new JobSupplierBookingDataObjectReader(uShipment, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("SBK001", supplierBooking.JSB_BookingId);
			AssertEquals(2, supplierBooking.PlannedContainers.Count);
			AssertEquals("20GP", supplierBooking.PlannedContainers[0].Container.RC_Code);
			AssertEquals((byte)2, supplierBooking.PlannedContainers[0].J1_ContainerCount);
			AssertEquals("40GP", supplierBooking.PlannedContainers[1].Container.RC_Code);
			AssertEquals((byte)3, supplierBooking.PlannedContainers[1].J1_ContainerCount);
		}

		public void TestAllocatedContainers()
		{
			var originalSupplierBooking = JobSupplierBookingDataObjectHelperTest.BuildJobSupplierBookingForTest(Factory);
			originalSupplierBooking.JSB_LoadMode = Core.Constants.SupplierBookingLoadMode.ContainerYard;
			originalSupplierBooking.JSB_BookingId = "SBK001";
			originalSupplierBooking.JSB_Status = SupplierBookingStatus.Incomplete;

			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "JK001";
			var containe1 = consol1.Containers.AddNew();
			containe1.JC_ContainerNum = "CON001";
			containe1.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			var containe2 = consol1.Containers.AddNew();
			containe2.JC_ContainerNum = "CON002";
			containe2.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;
			var containe3 = consol2.Containers.AddNew();
			containe3.JC_ContainerNum = "CON003";
			containe3.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;
			var containe4 = consol2.Containers.AddNew();
			containe4.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;
			containe4.JC_ContainerCount = 3;

			Factory.SaveForTesting();
			AssertEquals(0, originalSupplierBooking.Containers.Count);

			var uShipment = BuildUniversalShipmentForSupplierBooking();
			uShipment.LoadMode = new UniversalLoadMode { Code = "CY" };
			uShipment.ShipmentStatus = new UniversalCodeDescriptionPair { Code = "INC" };
			uShipment.DataContext.AddDataTarget(DataContextType.JobSupplierBooking, "SBK001");
			var containerWriter = new SupplierBookingAllocatedContainerDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			uShipment.SetRelatedShipmentCollection(() => new List<UniversalShipment> { containerWriter.GetDataObject(containe1), containerWriter.GetDataObject(containe4) });

			new JobSupplierBookingDataObjectReader(uShipment, Logger, Factory).ReadIntoBusinessObject();
			AssertContains("Consol containers can only be allocated to approved CY supplier bookings.", Logger.GetWarnings());

			uShipment.ShipmentStatus = new UniversalCodeDescriptionPair { Code = "APP" };

			var supplierBooking = new JobSupplierBookingDataObjectReader(uShipment, Logger, Factory).ReadIntoBusinessObject();
			AssertEquals("SBK001", supplierBooking.JSB_BookingId);
			AssertEquals(2, supplierBooking.Containers.Count);
			AssertEquals(consol1.JK_UniqueConsignRef, supplierBooking.Containers[0].Consol.JK_UniqueConsignRef);
			AssertEquals("20GP", supplierBooking.Containers[0].Container.RC_Code);
			AssertEquals("CON001", supplierBooking.Containers[0].JC_ContainerNum);
			AssertEquals((byte)1, supplierBooking.Containers[0].JC_ContainerCount);
			AssertEquals(consol2.JK_UniqueConsignRef, supplierBooking.Containers[1].Consol.JK_UniqueConsignRef);
			AssertEquals("40GP", supplierBooking.Containers[1].Container.RC_Code);
			AssertEquals(ZString.Empty, supplierBooking.Containers[1].JC_ContainerNum);
			AssertEquals((byte)3, supplierBooking.Containers[1].JC_ContainerCount);
		}

		static UniversalShipment BuildUniversalShipment(int bookingLineNumber = 1)
		{
			var uShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			uShipment.DataContext = DataContextFactory.New();
			uShipment.PortOfDischarge = new UniversalUNLOCO { Code = "SGSIN" };
			uShipment.PortOfLoading = new UniversalUNLOCO { Code = "AUSYD" };
			uShipment.ShipmentIncoTerm = new UniversalDataBuss.DataObjects.Universal.IncoTerm { Code = "EXW" };
			uShipment.ShipmentStatus = new UniversalCodeDescriptionPair { Code = "PLC" };
			uShipment.TransportMode = new UniversalCodeDescriptionPair { Code = "SEA" };
			uShipment.LoadMode = new UniversalLoadMode() { Code = "CY" };

			uShipment.SetDateCollection(() => new List<UniversalDate>());
			uShipment.DateCollection.Add(new UniversalDate { Type = UniversalDateType.BookedOnDate, Value = new ZDateTime(2022, 4, 3) });
			uShipment.DateCollection.Add(new UniversalDate { Type = UniversalDateType.CargoAvailableDate, Value = new ZDateTime(2022, 4, 6) });

			uShipment.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress>());
			uShipment.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.BookingPartyDocumentaryAddress), "BK", "SGSIN"));
			uShipment.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.Supplier), "SP", "SGSIN"));
			uShipment.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.ControllingCustomer), "CC", "CNSZX"));
			uShipment.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.ArrivalCFSAddress), "CB", "CCCcC"));

			uShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());
			for (var i = 0; i < bookingLineNumber; i++)
			{
				uShipment.SubShipmentCollection.Add(new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance));
				var uSubShipment = uShipment.SubShipmentCollection[i];
				uSubShipment.TotalNoOfPacksDecimal = 8.2;
				uSubShipment.Order = new UniversalDataBuss.DataObjects.Universal.Order(DefaultDataObjectWriterStrategy.TestInstance) { OrderNumber = "ORD0001", OrderNumberSplit = 1 };
				uSubShipment.Order.SetOrderLineCollection(() => new DataObjectList<UniversalDataBuss.DataObjects.Universal.OrderLine>());
				uSubShipment.Order.OrderLineCollection.Add(new UniversalDataBuss.DataObjects.Universal.OrderLine { LineNumber = i + 1, SubLineNumber = i + 2 });
				uSubShipment.SetPackingLineCollection(() => new DataObjectList<UniversalDataBuss.DataObjects.Universal.PackingLine>());
				uSubShipment.PackingLineCollection.Add(new UniversalDataBuss.DataObjects.Universal.PackingLine
				{
					PackQty = 2,
					MarksAndNos = "Line Marks&Nos",
					PackType = new UniversalDataBuss.DataObjects.Universal.PackageType { Code = "BAG" },
					Weight = 3,
					WeightUnit = new UniversalDataBuss.DataObjects.Universal.UnitOfWeight { Code = "KT" },
					Volume = 5,
					VolumeUnit = new UniversalDataBuss.DataObjects.Universal.UnitOfVolume { Code = "CF" }
				});
				uSubShipment.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress>());
				uSubShipment.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.Manufacturer), "MF", "NKAKL"));
			}

			return uShipment;
		}

		#region Shipment Window Log

		public void TestExceptionRaisedEventForMismatchedShipmentWindow()
		{
			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			bookingParty.OH_Code = "BKSIN";
			Factory.SaveForTesting();
			foreach (var orderWindowDate in new ZDate[] { ZDate.Empty, new ZDate(2022, 9, 1) })
			{
				foreach (var orderLineWindowDate in new ZDate[] { ZDate.Empty, new ZDate(2022, 9, 1), new ZDate(2022, 9, 2) })
				{
					foreach (var bookingLineWindowDate in new ZDate[] { ZDate.Empty, new ZDate(2022, 9, 1), new ZDate(2022, 9, 2), new ZDate(2022, 9, 3) })
					{
						var shipmentWindowDate = (orderLineWindowDate.IsEmpty ? orderWindowDate : orderLineWindowDate);

						var hasEvent = !shipmentWindowDate.IsEmpty && bookingLineWindowDate != shipmentWindowDate;

						CheckExceptionRaisedEvent(orderWindowDate, ZDate.Empty, orderLineWindowDate, ZDate.Empty, bookingLineWindowDate, ZDate.Empty, true, hasEvent, false);
						CheckExceptionRaisedEvent(orderWindowDate, ZDate.Empty, orderLineWindowDate, ZDate.Empty, bookingLineWindowDate, ZDate.Empty, false, hasEvent, false);
						CheckExceptionRaisedEvent(ZDate.Empty, orderWindowDate, ZDate.Empty, orderLineWindowDate, ZDate.Empty, bookingLineWindowDate, true, false, hasEvent);
						CheckExceptionRaisedEvent(ZDate.Empty, orderWindowDate, ZDate.Empty, orderLineWindowDate, ZDate.Empty, bookingLineWindowDate, false, false, hasEvent);
					}
				}
			}
		}

		public void TestExceptionRaisedEventForMismatchedShipmentWindow_ValidStatus()
		{
			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			bookingParty.OH_Code = "BKSIN";
			Factory.SaveForTesting();
			foreach (var bookingStatus in typeof(SupplierBookingStatus).GetFields().Select(field => field.GetValue(null) as string))
			{
				var hasEvent = bookingStatus == SupplierBookingStatus.Placed;
				if (bookingStatus == Core.Constants.SupplierBookingStatus.Shipped ||
					bookingStatus == Core.Constants.SupplierBookingStatus.Converted ||
					bookingStatus == Core.Constants.SupplierBookingStatus.Received ||
					 bookingStatus == Core.Constants.SupplierBookingStatus.Cancelled)
				{
					AssertExceptionThrown<DataObjectReadFailureException>(() =>
					{
						CheckExceptionRaisedEvent(ZDate.Empty, ZDate.Empty, new ZDate(2022, 9, 1), new ZDate(2022, 9, 2), new ZDate(2022, 10, 1), new ZDate(2022, 10, 2), false, hasEvent, hasEvent, bookingStatus);
						CheckExceptionRaisedEvent(ZDate.Empty, ZDate.Empty, new ZDate(2022, 9, 1), new ZDate(2022, 9, 2), new ZDate(2022, 10, 1), new ZDate(2022, 10, 2), true, hasEvent, hasEvent, bookingStatus);
					});
				}
				else
				{
					CheckExceptionRaisedEvent(ZDate.Empty, ZDate.Empty, new ZDate(2022, 9, 1), new ZDate(2022, 9, 2), new ZDate(2022, 10, 1), new ZDate(2022, 10, 2), false, hasEvent, hasEvent, bookingStatus);
					CheckExceptionRaisedEvent(ZDate.Empty, ZDate.Empty, new ZDate(2022, 9, 1), new ZDate(2022, 9, 2), new ZDate(2022, 10, 1), new ZDate(2022, 10, 2), true, hasEvent, hasEvent, bookingStatus);
				}
			}
		}

		public void TestExceptionRaisedEventForMismatchedShipmentWindow_ChangeStatus()
		{
			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			bookingParty.OH_Code = "BKSIN";
			Factory.SaveForTesting();

			var orderLineDate = new ZDate(2022, 9, 1);
			var bookingLineDate = new ZDate(2022, 10, 1);
			CreateOrderLineAndSupplierBooingWithShipmentWindow(ZDate.Empty, ZDate.Empty, orderLineDate, orderLineDate, SupplierBookingStatus.Incomplete, false, out var orderLine1, out var orderLine2, out var existingBooking);
			AssertOrderLineShipmentWindowLogs(existingBooking, 0, 0, orderLine1, orderLine2);

			GetSupplierBookingObjectWithShipmentWindow(SupplierBookingStatus.Placed, bookingLineDate, bookingLineDate, existingBooking);
			Factory.SaveForTesting();
			AssertOrderLineShipmentWindowLogs(existingBooking, 1, 1, orderLine1, orderLine2);

			existingBooking.JSB_Status = SupplierBookingStatus.Rejected;
			Factory.SaveForTesting();
			GetSupplierBookingObjectWithShipmentWindow(SupplierBookingStatus.Placed, bookingLineDate, bookingLineDate, existingBooking);
			Factory.SaveForTesting();
			AssertOrderLineShipmentWindowLogs(existingBooking, 2, 2, orderLine1, orderLine2);

			existingBooking.JSB_Status = SupplierBookingStatus.Incomplete;
			Factory.SaveForTesting();
			GetSupplierBookingObjectWithShipmentWindow(SupplierBookingStatus.Placed, bookingLineDate, bookingLineDate, existingBooking);
			Factory.SaveForTesting();
			AssertOrderLineShipmentWindowLogs(existingBooking, 3, 3, orderLine1, orderLine2);
		}

		void CheckExceptionRaisedEvent(ZDate orderShipmentWindowStart, ZDate orderShipmentWindowEnd, ZDate orderLineShipmentWindowStart, ZDate orderLineShipmentWindowEnd, ZDate bookingLineShipmentWindowStart, ZDate bookingLineShipmentWindowEnd, bool testInsert, bool hasStartEvent, bool hasEndEvent, string bookingStatus = SupplierBookingStatus.Placed)
		{
			CreateOrderLineAndSupplierBooingWithShipmentWindow(orderShipmentWindowStart, orderShipmentWindowEnd, orderLineShipmentWindowStart, orderLineShipmentWindowEnd, bookingStatus, testInsert, out var orderLine1, out var orderLine2, out var existingBooking);

			var createdBooking = GetSupplierBookingObjectWithShipmentWindow(bookingStatus, bookingLineShipmentWindowStart, bookingLineShipmentWindowEnd, existingBooking);

			AssertOrderLineShipmentWindowLogs(createdBooking, hasStartEvent ? 1 : 0, hasEndEvent ? 1 : 0, orderLine1, orderLine2);
		}

		void CreateOrderLineAndSupplierBooingWithShipmentWindow(ZDate orderShipmentWindowStart, ZDate orderShipmentWindowEnd, ZDate orderLineShipmentWindowStart, ZDate orderLineShipmentWindowEnd, string bookingStatus, bool testInsert, out OrderLine orderLine1, out OrderLine orderLine2, out JobSupplierBooking supplierBooking)
		{
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_ShipmentWindowStart = orderShipmentWindowStart;
			order.JD_ShipmentWindowEnd = orderShipmentWindowEnd;
			order.JD_OrderNumber = "ORD0001";
			order.JD_OrderNumberSplit = 1;
			orderLine1 = Factory.NewWithValidTestData<OrderLine>();
			orderLine1.JO_JD = order.PK;
			orderLine1.JO_ShipmentWindowStart = orderLineShipmentWindowStart;
			orderLine1.JO_ShipmentWindowEnd = orderLineShipmentWindowEnd;
			orderLine1.JO_LineNo = 1;
			orderLine1.JO_SubLineNo = 2;
			orderLine2 = Factory.NewWithValidTestData<OrderLine>();
			orderLine2.JO_JD = order.PK;
			orderLine2.JO_ShipmentWindowStart = orderLineShipmentWindowStart;
			orderLine2.JO_ShipmentWindowEnd = orderLineShipmentWindowEnd;
			orderLine2.JO_LineNo = 2;
			orderLine2.JO_SubLineNo = 3;
			Factory.SaveForTesting();

			supplierBooking = null;
			if (!testInsert)
			{
				supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
				supplierBooking.JSB_Status = bookingStatus;
				var supplierBookingLine1 = Factory.NewWithValidTestData<JobSupplierBookingLine>();
				supplierBookingLine1.JSL_JSB_Booking = supplierBooking.PK;
				supplierBookingLine1.JSL_JO_OrderLine = orderLine1.PK;
				supplierBookingLine1.JSL_ShipmentWindowStart = new ZDate(1999, 9, 1);
				supplierBookingLine1.JSL_ShipmentWindowEnd = new ZDate(1999, 9, 2);
				var supplierBookingLine2 = Factory.NewWithValidTestData<JobSupplierBookingLine>();
				supplierBookingLine2.JSL_JSB_Booking = supplierBooking.PK;
				supplierBookingLine2.JSL_JO_OrderLine = orderLine2.PK;
				supplierBookingLine2.JSL_ShipmentWindowStart = new ZDate(1999, 9, 1);
				supplierBookingLine2.JSL_ShipmentWindowEnd = new ZDate(1999, 9, 2);
				Factory.SaveForTesting();
			}
		}

		JobSupplierBooking GetSupplierBookingObjectWithShipmentWindow(string bookingStatus, ZDate? bookingLineShipmentWindowStart, ZDate? bookingLineShipmentWindowEnd, JobSupplierBooking existingBooking)
		{
			var uShipment = BuildUniversalShipment(2);
			uShipment.ShipmentStatus = new UniversalCodeDescriptionPair { Code = bookingStatus };
			if (existingBooking != null)
			{
				uShipment.DataContext.AddDataTarget(DataContextType.JobSupplierBooking, existingBooking.JSB_BookingId);
			}

			for (var i = 0; i < uShipment.SubShipmentCollection.Count; i++)
			{
				var uSubShipment = uShipment.SubShipmentCollection[i];
				if (existingBooking != null)
				{
					uSubShipment.PackingLineCollection[0].PackingLineID = existingBooking.SupplierBookingLines[i].JSL_BookingLineId;
				}
				if (bookingLineShipmentWindowStart != null)
				{
					uSubShipment.SetDateCollection(() => new List<UniversalDate>()
					{
						new () { Type = UniversalDateType.ShipmentWindowStart, Value = bookingLineShipmentWindowStart },
					});
				}
				if (bookingLineShipmentWindowEnd != null)
				{
					if (uSubShipment.DateCollection == null)
					{
						uSubShipment.SetDateCollection(() => new List<UniversalDate>());
					}
					uSubShipment.DateCollection.Add(new UniversalDate { Type = UniversalDateType.ShipmentWindowEnd, Value = bookingLineShipmentWindowEnd });
				}
			}
			var supplierBookingObject = new JobSupplierBookingDataObjectReader(uShipment, Logger, Factory).ReadIntoBusinessObject();
			return supplierBookingObject;
		}

		void AssertOrderLineShipmentWindowLogs(JobSupplierBooking supplierBooking, int startEventCount, int endEventCount, OrderLine orderLine1, OrderLine orderLine2)
		{
			AssertEquals(2, supplierBooking.SupplierBookingLines.Count);

			var orderLine1Logs = orderLine1.Logs.GetAllLogs();
			AssertEquals(startEventCount, orderLine1Logs.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.Equals($"|RES=This Order Line has a Ship Window Start mismatch with its linked Supplier Booking. Please check and review the linked Supplier Booking ({supplierBooking.JSB_BookingId}).|TYP=Ship Window Start")).Count());
			AssertEquals(endEventCount, orderLine1Logs.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.Equals($"|RES=This Order Line has a Ship Window End mismatch with its linked Supplier Booking. Please check and review the linked Supplier Booking ({supplierBooking.JSB_BookingId}).|TYP=Ship Window End")).Count());

			var orderLine2Logs = orderLine2.Logs.GetAllLogs();
			AssertEquals(startEventCount, orderLine2Logs.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code  && log.SL_Reference.Equals($"|RES=This Order Line has a Ship Window Start mismatch with its linked Supplier Booking. Please check and review the linked Supplier Booking ({supplierBooking.JSB_BookingId}).|TYP=Ship Window Start")).Count());
			AssertEquals(endEventCount, orderLine2Logs.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.Equals($"|RES=This Order Line has a Ship Window End mismatch with its linked Supplier Booking. Please check and review the linked Supplier Booking ({supplierBooking.JSB_BookingId}).|TYP=Ship Window End")).Count());

			var bookingLogs = supplierBooking.Logs.GetAllLogs();
			AssertEquals(startEventCount, bookingLogs.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.Equals("|RES=This Supplier Booking has booking lines whose Ship Window Start dates do not match with their order line dates.|TYP=Ship Window Start")).Count());
			AssertEquals(endEventCount, bookingLogs.Where(log => log.SL_SE_NKEvent == Events.ExceptionRaised.Code && log.SL_Reference.Equals("|RES=This Supplier Booking has booking lines whose Ship Window End dates do not match with their order line dates.|TYP=Ship Window End")).Count());
		}

		#endregion
	}
}
