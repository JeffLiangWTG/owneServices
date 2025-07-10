using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	class DtbBookingDataObjectWriterTest : DtbBookingTestCaseWithFactory
	{
		public void TestNamespace2012PutsParentConsolidationInParentShipmentCollection()
		{
			var shipmentBO = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S00001234";
			shipmentBO[JobShipmentSchema.JS_HouseBill] = "HB31278903";

			var consolBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			consolBO[JobConsolSchema.JK_UniqueConsignRef] = "C00001432";
			consolBO[JobConsolSchema.JK_AgentType] = "AGT";
			consolBO[JobConsolSchema.JK_TransportMode] = "SEA";
			consolBO[JobConsolSchema.JK_MasterBillNum] = "MB234890232";

			var consolShipmentLinkBO = Factory.New(ObjectFactory.GetType<Freight.Integration.IJobConShipLink>());
			consolShipmentLinkBO[JobConShipLinkSchema.JN_JK] = consolBO.PK;
			consolShipmentLinkBO[JobConShipLinkSchema.JN_JS] = shipmentBO.PK;

			var bookingConsolidationBO = Factory.New<DtbBookingConsolidation>();
			bookingConsolidationBO.KB_ParentID = shipmentBO.PK;
			bookingConsolidationBO.KB_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			bookingConsolidationBO.KB_JobDirection = nameof(DtbBookingDirection.PIC);

			AssertEquals("Precondition: bookingConsolidationBO.Parent should be the Forwarding Shipment.", shipmentBO, bookingConsolidationBO.Parent.ParentWithWorkflow);

			var bookingBO = bookingConsolidationBO.Bookings.AddNew();
			bookingBO.KM_KT_NKBookingTemplate = "ABC";
			bookingBO.KM_Description = "DESC123";
			bookingBO.KM_Direction = "IMP";
			bookingBO.KM_TransportReference = "TRANS123";

			Factory.Save();

			UniversalShipment topLevelDataObject;
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				topLevelDataObject = new DtbBookingDataObjectWriter(null, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, bookingBO)), true).GetDataObject(bookingBO);
			}

			CombineAssertions(() =>
			{
				AssertEquals("topLevelDataObject.DataContext.GetDataSources()", string.Format("TransportBooking [{0}]", bookingBO.KM_JobID), topLevelDataObject.DataContext.GetDataSources());
				AssertNotNull("topLevelDataObject.ParentShipmentCollection", topLevelDataObject.ParentShipmentCollection);
				AssertEquals("topLevelDataObject.ParentShipmentCollection.Count", 1, topLevelDataObject.ParentShipmentCollection.Count);
			});

			var transportConsolidationDataObject = topLevelDataObject.ParentShipmentCollection[0];
			CombineAssertions(() =>
			{
				AssertEquals("transportConsolidationDataObject.DataContext.GetDataSources()", string.Format("TransportBookingConsolidation [{0}]", bookingConsolidationBO.KB_JobID), transportConsolidationDataObject.DataContext.GetDataSources());
				AssertNull("transportConsolidationDataObject.PreCarriageShipmentCollection", transportConsolidationDataObject.PreCarriageShipmentCollection);
				AssertNotNull("transportConsolidationDataObject.PostCarriageShipmentCollection", transportConsolidationDataObject.PostCarriageShipmentCollection);
				AssertEquals("transportConsolidationDataObject.PostCarriageShipmentCollection.Count", 1, transportConsolidationDataObject.PostCarriageShipmentCollection.Count);
			});

			var shipmentDataObject = transportConsolidationDataObject.PostCarriageShipmentCollection[0];
			AssertEquals("transportConsolidationDataObject.DataContext.GetDataSources()", "ForwardingShipment [S00001234]", shipmentDataObject.DataContext.GetDataSources());
		}

		public void TestGetDataObject()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_GoodsDescription = "GOODS DESCRIPTION";

			var carrier = Helper.CreateOrganisation("C1");
			var carrierAccount = Factory.New<OrgCarrierAccount>();
			carrierAccount.OAN_OH_Carrier = carrier.PK;
			carrierAccount.OAN_AccountNumber = "123";
			carrierAccount.OAN_DepotID = "456";
			carrierAccount.OAN_MerchantNumber = "789";

			var controllingBranch = Factory.New<GlbBranch>();
			controllingBranch.GB_Code = "HII";
			controllingBranch.GB_BranchName = "I'm a branch";
			controllingBranch.GB_GC = Environment.Env.CurrentCompanyPK;

			var booking = Helper.CreateBooking(consolidation);
			booking.KM_GB_Branch = controllingBranch.PK;
			booking.KM_KT_NKBookingTemplate = "IFCY";
			booking.KM_Description = "DESC123";
			booking.KM_Direction = "IMP";
			booking.KM_JobID = "BM00000001";
			booking.KM_TransportReference = "TRANS123";
			booking.KM_TransportMode = "RAI";
			booking.KM_RS_NKServiceLevel = "D2D";
			booking.KM_PL_NKCarrierServiceLevel = "STD";
			booking.KM_IsHazardous = true;
			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Loose;
			booking.KM_RequiresRefrigeration = true;
			booking.KM_OAN_CarrierAccount = carrierAccount.PK;
			booking.Instructions.DeleteAll();
			Factory.Save();

			var bookingDataObject1 = new DtbBookingDataObjectWriter(null, new DataWritingManager(new DummyActionInfo()), false).GetDataObject(booking);

			CombineAssertions(() =>
			{
				AssertEquals("TransportBooking [BM00000001]", bookingDataObject1.DataContext.GetDataSources());
				AssertEquals("bookingDataObject1.Branch", "HII", bookingDataObject1.Branch.Code);
				AssertEquals("bookingDataObject1.Branch", "I'm a branch", bookingDataObject1.Branch.Name);
				AssertEquals("bookingDataObject1.TransportBookingDirection.Code", "IMP", bookingDataObject1.TransportBookingDirection.Code);
				AssertEquals("bookingDataObject1.TransportBookingDirection.Description", "Import", bookingDataObject1.TransportBookingDirection.Description);
				AssertEquals("bookingDataObject1.LocalTransportJobType.Code", "IFCY", bookingDataObject1.LocalTransportJobType.Code);
				AssertEquals("bookingDataObject1.LocalTransportJobType.Description", "Import FCL, CNR to CFS", bookingDataObject1.LocalTransportJobType.Description);
				AssertEquals("bookingDataObject1.LocalProcessing.ArrivalCartageRef", "TRANS123", bookingDataObject1.LocalProcessing.ArrivalCartageRef);
				AssertEquals("bookingDataObject1.ServiceLevel.Code", "D2D", bookingDataObject1.ServiceLevel.Code);
				AssertEquals("bookingDataObject1.ServiceLevel.Description", "Door to Door", bookingDataObject1.ServiceLevel.Description);
				AssertEquals("bookingDataObject1.CarrierServiceLevel.Code", "STD", bookingDataObject1.CarrierServiceLevel.Code);
				AssertEquals("bookingDataObject1.CarrierServiceLevel.Description", "Standard", bookingDataObject1.CarrierServiceLevel.Description);
				AssertEquals("bookingDataObject1.IsHazardous", true, bookingDataObject1.IsHazardous);
				AssertEquals("bookingDataObject1.RequiresRefrigeration", true, bookingDataObject1.RequiresRefrigeration);
				AssertEquals("bookingDataObject1.RatingTransportMode.Code", RatingFreightModes.Codes.Loose, bookingDataObject1.RatingTransportMode.Code);
				AssertEquals("bookingDataObject1.RatingTransportMode.Description", RatingFreightModes.Descriptions.Loose, bookingDataObject1.RatingTransportMode.Description);
				AssertEquals("bookingDataObject1.BookingTransportMode.Code", "RAI", bookingDataObject1.BookingTransportMode.Code);
				AssertEquals("bookingDataObject1.BookingTransportMode.Description", "Rail Transport", bookingDataObject1.BookingTransportMode.Description);
				AssertEquals("bookingDataObject1.ShipmentStatus.Code", TransportStatuses.Codes.Available, bookingDataObject1.ShipmentStatus.Code);
				AssertEquals("bookingDataObject1.ShipmentStatus.Description", TransportStatuses.Descriptions.Available, bookingDataObject1.ShipmentStatus.Description);
				AssertEquals("bookingDataObject1.GoodsDescription", "GOODS DESCRIPTION", bookingDataObject1.GoodsDescription);
				AssertNotNull("bookingDataObject1.CarrierAccount", bookingDataObject1.CarrierAccount.AccountNumber);
				AssertEquals("bookingDataObject1.CarrierAccount.AccountNumber", "123", bookingDataObject1.CarrierAccount.AccountNumber);
				AssertEquals("bookingDataObject1.CarrierAccount.DepotID", "456", bookingDataObject1.CarrierAccount.DepotID);
				AssertEquals("bookingDataObject1.CarrierAccount.MerchantNumber", "789", bookingDataObject1.CarrierAccount.MerchantNumber);
			});

			AssertNull("No Doc Addresses were set, so no OrganizationAddresses should be created.", bookingDataObject1.OrganizationAddressCollection);
			AssertNull("No BookingInstructions were set, so no Instructions should be created.", bookingDataObject1.InstructionCollection);

			var transportCompany = Factory.New<OrgHeader>();
			booking.Address.OrganisationPK = transportCompany.PK;
			booking.Instructions.AddNew();
			booking.Instructions.AddNew();

			var bookingDataObject2 = new DtbBookingDataObjectWriter(null, new DataWritingManager(new DummyActionInfo()), false).GetDataObject(booking);
			AssertEquals("Transport Doc Addresses was set, so 1 OrganizationAddresses should be created.", 1, bookingDataObject2.OrganizationAddressCollection.Count);
			AssertEquals("2 BookingInstructions were set, so 2 Instructions should be created.", 2, bookingDataObject2.InstructionCollection.Count);
		}

		public void TestGetDataObject_CustomizedFields()
		{
			var today = ZDateTime.Today;
			var booking = Helper.CreateBooking();

			var emptyBookingInstructionDataObject = new DtbBookingDataObjectWriter(null, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, booking)), true).GetDataObject(booking);
			AssertNull("Precondition:", emptyBookingInstructionDataObject.CustomizedFieldCollection);

			booking.SetUserDefinedValue("DATE", today);
			booking.SetUserDefinedValue("BOOL", ZBool.True);
			booking.SetUserDefinedValue("DECIMAL", new ZDecimal(123.456));
			booking.SetUserDefinedValue("INT", new ZInt(123));
			booking.SetUserDefinedValue("BYTE", new ZByte(234));
			booking.SetUserDefinedValue("STRING", new ZString("HELLO"));
			booking.SetUserDefinedValue("SHORT", new ZShort(345));

			var bookingConsolidationDataObject = new DtbBookingDataObjectWriter(null, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, booking)), true).GetDataObject(booking);
			var bookingDataObject = bookingConsolidationDataObject.SubShipmentCollection[0];
			var customFieldCollection = bookingDataObject.CustomizedFieldCollection;

			AssertEquals("Not all Custom Fields were exported.", 7, customFieldCollection.Count);
			customFieldCollection.AssertCustomFieldWasExported(DataType.DateTime, "DATE", today.ToISO8601String());
			customFieldCollection.AssertCustomFieldWasExported(DataType.Boolean, "BOOL", "true");
			customFieldCollection.AssertCustomFieldWasExported(DataType.Decimal, "DECIMAL", "123.456");
			customFieldCollection.AssertCustomFieldWasExported(DataType.Integer, "INT", "123");
			customFieldCollection.AssertCustomFieldWasExported(DataType.Byte, "BYTE", "234");
			customFieldCollection.AssertCustomFieldWasExported(DataType.String, "STRING", "HELLO");
			customFieldCollection.AssertCustomFieldWasExported(DataType.Short, "SHORT", "345");
		}

		public void TestGetDataObject_LinksDictionary()
		{
			// create a consol with 1 booking 
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);

			// create and assign a container and a box to the booking instruction
			var container = consolidation.PackageJob.Packages.AddNew("CNT");
			var box = container.Packages.AddNew("BOX");
			Helper.CreatePackageDivot(instruction, container, 1);
			Helper.CreatePackageDivot(instruction, box, 1);

			var linksDictionary = new Dictionary<ZGuid, ZInt>();
			const int ContainerIndex = 0; // index in List<Container>
			const int PackageIndex = 0; // index in List<PackingLine>
			linksDictionary.Add(container.PK, ContainerIndex);
			linksDictionary.Add(box.PK, PackageIndex);

			const bool IncludeParentConsolidation = true;
			var bookingDataObjectExportedFromConsolidation = new DtbBookingDataObjectWriter(linksDictionary, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, booking)), !IncludeParentConsolidation).GetDataObject(booking);
			AssertEquals("Container Divot should be linked to a correct Container.", ContainerIndex, bookingDataObjectExportedFromConsolidation.InstructionCollection[0].InstructionContainerLinkCollection[0].ContainerLink);
			AssertEquals("Package Divot should be linked to a correct PackingLine.", PackageIndex, bookingDataObjectExportedFromConsolidation.InstructionCollection[0].InstructionPackingLineLinkCollection[0].PackingLineLink);

			var bookingDataObjectExportedFromBooking = new DtbBookingDataObjectWriter(null, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, booking)), IncludeParentConsolidation).GetDataObject(booking);
			AssertEquals("Container should have correct link.", 0, bookingDataObjectExportedFromBooking.ContainerCollection[0].Link);
			AssertEquals("PackingLine should have correct link.", 0, bookingDataObjectExportedFromBooking.PackingLineCollection[0].Link);
			AssertEquals("Container Divot should be linked to a correct Container.", 0, bookingDataObjectExportedFromBooking.SubShipmentCollection[0].InstructionCollection[0].InstructionContainerLinkCollection[0].ContainerLink);
			AssertEquals("Package Divot should be linked to a correct PackingLine.", 0, bookingDataObjectExportedFromBooking.SubShipmentCollection[0].InstructionCollection[0].InstructionPackingLineLinkCollection[0].PackingLineLink);
		}

		public void TestGetDataObject_UnrelatedContainer()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var container1 = consolidation.PackageJob.Packages.AddNew("CNT");
			var container2 = consolidation.PackageJob.Packages.AddNew("CNT");
			var container1Box = container1.Packages.AddNew("BOX");
			var container2Box = container2.Packages.AddNew("BOX");
			var container1Carton = container1Box.Packages.AddNew("CTN");
			var container2Carton = container2Box.Packages.AddNew("CTN");
			var outerPackage = consolidation.PackageJob.Packages.AddNew("PLT");
			container1.KP_PackageID = "container1";
			container2.KP_PackageID = "container2";
			container1Box.KP_PackageID = "container1Box";
			container2Box.KP_PackageID = "container2Box";
			container1Carton.KP_PackageID = "container1Carton";
			container2Carton.KP_PackageID = "container2Carton";
			outerPackage.KP_PackageID = "outerPackage";

			Helper.CreatePackageDivot(instruction, container1, 1);
			Helper.CreatePackageDivot(instruction, container2Carton, 1);

			const bool IncludeParentConsolidation = true;

			var bookingDataObjectExportedFromBooking = new DtbBookingDataObjectWriter(null, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, booking)), IncludeParentConsolidation).GetDataObject(booking);
			var containerCollection = bookingDataObjectExportedFromBooking.ContainerCollection;
			AssertEquals("Should not export 2nd container", 1, containerCollection.Count);
			AssertEquals("Should be container1", "container1", containerCollection[0].ContainerNumber);

			var packingCollection = bookingDataObjectExportedFromBooking.PackingLineCollection;
			AssertEquals("Should have outer from container1 (container1Box) and carton from container 2 (container2Carton)", 2, packingCollection.Count);
			AssertNotNull("Should be container1Box", packingCollection.First(p => p.ReferenceNumber.Value == "container1Box"));
			AssertNotNull("Should be container2Carton", packingCollection.First(p => p.ReferenceNumber.Value == "container2Carton"));
		}

		public void TestGetDataObject_TransportMode_BookingWithoutParentJob()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var orgHeader = Helper.CreateOrgHeader(Factory, "ABC");
			var orgAddress = Helper.CreateOrgAddress(Factory, orgHeader, "ABC");

			var requirement = (((IDocAddresses)booking).GetDocAddressRequirement(DocAddressType.ClientRequestedBillingParty));
			var billingPartyAddress = consolidation.DocAddresses.FindOrCreateWithRequirement(requirement);
			billingPartyAddress.E2_OA_Address = orgAddress.PK;
			billingPartyAddress.E2_Address1 = "1 Orange Street";
			Assert(!billingPartyAddress.IsEmpty);
			consolidation.DocAddresses.Add(billingPartyAddress);

			AssertEquals("Precondition:", null, booking.ParentJob);

			var bookingDataObjectExportedFromBooking = new DtbBookingDataObjectWriter(null, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, booking)), true).GetDataObject(booking);
			AssertNull("Not Routing info in Consolidation, TransportMode Should be null", bookingDataObjectExportedFromBooking.TransportMode);
			Assert("Should set the ClientRequestedBillingParty on the data object", bookingDataObjectExportedFromBooking.OrganizationAddressCollection?.Where(oa => oa.AddressType.Value == nameof(DocAddressType.ClientRequestedBillingParty)).Count() > 0);

			var transport1 = Factory.NewWithValidTestData<Transport>();
			transport1.ParentType = typeof(DtbBookingConsolidation);
			transport1.JW_ParentGUID = consolidation.PK;
			transport1.JW_LegOrder = 1;

			var transport2 = Factory.NewWithValidTestData<Transport>();
			transport2.ParentType = typeof(DtbBookingConsolidation);
			transport2.JW_ParentGUID = consolidation.PK;
			transport2.JW_LegOrder = 2;

			var bookingDataObjectExportedFromBooking2 = new DtbBookingDataObjectWriter(null, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, booking)), true).GetDataObject(booking);
			AssertNull("Consolidation has Routing infos without TransportMode, TransportMode Should be null", bookingDataObjectExportedFromBooking2.TransportMode);

			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_TransportMode = Constants.TransportModes.Air;

			var bookingDataObjectExportedFromBooking3 = new DtbBookingDataObjectWriter(null, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, booking)), true).GetDataObject(booking);
			AssertNotNull("Consolidation has Routing info with TransportMode, TransportMode should not be null", bookingDataObjectExportedFromBooking3.TransportMode);
			AssertEquals("Should get TransportMode from first Routing info", "SEA", bookingDataObjectExportedFromBooking3.TransportMode.Code);
		}

		public void TestGetDataObject_CheckActualChargeableValue()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);

			var bookingDataObject = new DtbBookingDataObjectWriter(null, new DataWritingManager(new DummyActionInfo()), false).GetDataObject(booking);
			AssertNull("Actual Chargeable should have a null value", bookingDataObject.ActualChargeable);

			booking.KM_OverrideChargeable = true;
			booking.KM_Chargeable = 550m;

			bookingDataObject = new DtbBookingDataObjectWriter(null, new DataWritingManager(new DummyActionInfo()), false).GetDataObject(booking);
			AssertEquals("Actual Chargeable should be 550.00", 550m, bookingDataObject.ActualChargeable);
		}

		public void TestGetDataObject_CheckTotalWeightUnitHasNullValue()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);

			var bookingDataObject = new DtbBookingDataObjectWriter(null, new DataWritingManager(new DummyActionInfo()), false).GetDataObject(booking);

			AssertNull("Override Chargeable is not true for transport booking. TotalWeightUnit should have null value.", bookingDataObject.TotalWeightUnit);
		}

		public void TestGetDataObject_CheckTotalWeightUnitHasValue()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			booking.KM_OverrideChargeable = true;

			using (PackingRegistry.Instance.WeightUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.Weight.Tonnes))
			{
				var bookingDataObject = new DtbBookingDataObjectWriter(null, new DataWritingManager(new DummyActionInfo()), false).GetDataObject(booking);

				AssertEquals("Chargeable Weight Unit should be in Tonne(s)", Constants.Weight.Tonnes, bookingDataObject.TotalWeightUnit.Code);
			}
		}

		[TestDate(2024, 7, 16)]
		public void TestGetDataObject_CheckDateCollection()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var writer = new DtbBookingDataObjectWriter(null, new DataWritingManager(new DummyActionInfo()), false);
			Factory.Save();

			var bookingDataObject = writer.GetDataObject(booking);
			AssertEquals(new DateTime(2024, 7, 16), bookingDataObject.DateCollection.FirstOrDefault(x => x.Type == DateType.JobCreated).Value);
		}

		public void TestPopulateDataObject_WhenBookingDataObjectIsNull()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var writer = new DtbBookingDataObjectWriter(null, new DataWritingManager(new DummyActionInfo()), false);

			booking.KM_GB_Branch = ZGuid.Empty;

			var bookingDataObject = writer.GetDataObject(booking);

			AssertEquals(null, bookingDataObject.Branch.Code);
			AssertEquals(null, bookingDataObject.Branch.Name);
		}

		public void TestDataObjectSubShipmentCollectionType()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var dataObject = new DtbBookingDataObjectWriter(null, new DataWritingManager(new ActionInfo(RecipientRoleType.CTG, booking)), true).GetDataObject(booking);
			AssertType<DataObjectList<UniversalShipment>>(dataObject.SubShipmentCollection);
		}

		public void TestAdditionalReferences()
		{
			var bookingBO = Helper.CreateBooking();
			bookingBO.ConsolidationSingleJob.AdditionalReferenceNumbers.Add(AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(Factory, "TRF", "CON-TRF"));
			bookingBO.ConsolidationSingleJob.AdditionalReferenceNumbers.Add(AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(Factory, "HSB", "CON-HSB"));

			bookingBO.AdditionalReferenceNumbers.Add(AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(Factory, "TRF", "BK-TRF"));
			bookingBO.AdditionalReferenceNumbers.Add(AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(Factory, "CIN"));
			bookingBO.AdditionalReferenceNumbers.Add(AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(Factory, "UCR"));
			bookingBO.AdditionalReferenceNumbers.Add(AdditionalReferenceDataObjectWriterTest.SetupAdditionalReference(Factory, "CLR"));

			var writer = new DtbBookingDataObjectWriter(new Dictionary<ZGuid, ZInt>(), new DataWritingManager(new DummyActionInfo()), false);
			var bookingDataObject = writer.GetDataObject(bookingBO);
			AssertNotNull("Precondition", bookingDataObject);
			AssertNotNull("Precondition", bookingDataObject.AdditionalReferenceCollection);

			CombineAssertions(delegate
			{
				AssertEquals(4, bookingDataObject.AdditionalReferenceCollection.Count);
				var sortedAdditionalReferences = bookingDataObject.AdditionalReferenceCollection.Cast<AdditionalReference>().OrderBy(e => e.Type.Code).ToArray();

				AdditionalReferenceDataObjectWriterTest.AssertContents(sortedAdditionalReferences[0], "CIN", "Commercial Invoice Number");
				AdditionalReferenceDataObjectWriterTest.AssertContents(sortedAdditionalReferences[1], "CLR", "Customer Reference Number");
				AdditionalReferenceDataObjectWriterTest.AssertContents(sortedAdditionalReferences[2], "TRF", "Transport Reference Number", "BK-TRF");
				AdditionalReferenceDataObjectWriterTest.AssertContents(sortedAdditionalReferences[3], "UCR", "External (3rd Party) Unique Consignment Reference");
			});
		}

		public void TestAdditionalReferences_PopulatesWayBillNumbersIntoAppropriateUXMLFields()
		{
			var consolidation = Helper.CreateConsolidation();
			var bookingBO = consolidation.Bookings.AddNew();

			var houseBill = bookingBO.AdditionalReferenceNumbers.AddNew();
			var masterBill1 = bookingBO.AdditionalReferenceNumbers.AddNew();
			var masterBill2 = bookingBO.AdditionalReferenceNumbers.AddNew();

			houseBill.CE_EntryType = "HSB";
			masterBill1.CE_EntryType = "MAB";
			masterBill2.CE_EntryType = "MAB";
			houseBill.CE_EntryNum = "1";
			masterBill1.CE_EntryNum = "2";
			masterBill2.CE_EntryNum = "3";

			var writer1 = new DtbBookingDataObjectWriter(new Dictionary<ZGuid, ZInt>(), new DataWritingManager(new DummyActionInfo()), false);
			var bookingDataObject1 = writer1.GetDataObject(bookingBO);
			AssertNotNull("Precondition", bookingDataObject1);
			AssertEquals("Should remain populated in additional references.", 3, bookingDataObject1.AdditionalReferenceCollection.Count);
			AssertEquals("Should populate HouseBill in UXML WayBill field.", "1", bookingDataObject1.WayBillNumber);
			AssertEquals("Should populate HouseBill in UXML WayBill field.", WayBillTypeList.Codes.House, bookingDataObject1.WayBillType.Code);
			AssertEquals("Should populate HouseBill in UXML WayBill field.", WayBillTypeList.Descriptions.House, bookingDataObject1.WayBillType.Description);

			bookingBO.AdditionalReferenceNumbers.RemoveAndDelete(masterBill2);

			// Empty House Bill - should ignore
			houseBill.CE_EntryNum = "";
			var writer2 = new DtbBookingDataObjectWriter(new Dictionary<ZGuid, ZInt>(), new DataWritingManager(new DummyActionInfo()), false);
			var bookingDataObject2 = writer2.GetDataObject(bookingBO);

			AssertNotNull("Precondition", bookingDataObject2);
			AssertEquals("Should remain populated in additional references.", 2, bookingDataObject2.AdditionalReferenceCollection.Count);
			AssertEquals("Should populate MasterBill in UXML WayBill field.", "2", bookingDataObject2.WayBillNumber);
			AssertEquals("Should populate MasterBill in UXML WayBill field.", WayBillTypeList.Codes.Master, bookingDataObject2.WayBillType.Code);
			AssertEquals("Should populate MasterBill in UXML WayBill field.", WayBillTypeList.Descriptions.Master, bookingDataObject2.WayBillType.Description);

			// No House Bill
			bookingBO.AdditionalReferenceNumbers.RemoveAndDelete(houseBill);
			var writer3 = new DtbBookingDataObjectWriter(new Dictionary<ZGuid, ZInt>(), new DataWritingManager(new DummyActionInfo()), false);
			var bookingDataObject3 = writer3.GetDataObject(bookingBO);

			AssertNotNull("Precondition", bookingDataObject3);
			AssertEquals("Should remain populated in additional references.", 1, bookingDataObject3.AdditionalReferenceCollection.Count);
			AssertEquals("Should populate MasterBill in UXML WayBill field.", "2", bookingDataObject3.WayBillNumber);
			AssertEquals("Should populate MasterBill in UXML WayBill field.", WayBillTypeList.Codes.Master, bookingDataObject3.WayBillType.Code);
			AssertEquals("Should populate MasterBill in UXML WayBill field.", WayBillTypeList.Descriptions.Master, bookingDataObject3.WayBillType.Description);
		}

		public void TestAdditionalReferences_PopulatesParentIDIntoAdditionalReferences()
		{
			var parent = Factory.New<DummyWithDtbBooking>();
			parent.Z0_Description = "S00000123";

			var consolidation = Helper.CreateConsolidation(parent);
			var bookingBO = consolidation.Bookings.AddNew();

			var writer1 = new DtbBookingDataObjectWriter(new Dictionary<ZGuid, ZInt>(), new DataWritingManager(new DummyActionInfo()), false);
			var bookingDataObject1 = writer1.GetDataObject(bookingBO);
			AssertNotNull("Precondition.", bookingDataObject1);
			AssertNotNull("Precondition.", bookingDataObject1.AdditionalReferenceCollection);
			AssertEquals("Precondition.", 1, bookingDataObject1.AdditionalReferenceCollection.Count);
			AssertEquals("Should populate Sender Booking in AdditionalReferenceCollection.", "BPR", bookingDataObject1.AdditionalReferenceCollection[0].Type.Code);
			AssertEquals("Should populate Sender Booking in AdditionalReferenceCollection.", "Booking Party Reference", bookingDataObject1.AdditionalReferenceCollection[0].Type.Description);
			AssertEquals("Should populate MasterBill in AdditionalReferenceCollection.", "S00000123", bookingDataObject1.AdditionalReferenceCollection[0].ReferenceNumber);
		}

		public void TestLocalClient()
		{
			var localClient = OrganizationAddressTestHelper.GetOrganizationBO_CRAHOLSYD(Factory);
			var booking = Helper.CreateBooking();
			var job = new JobHeader.Loader(booking).TryCreate();
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;

			var writer = new DtbBookingDataObjectWriter(new Dictionary<ZGuid, ZInt>(), new DataWritingManager(new DummyActionInfo()), false);
			var bookingDataObject = writer.GetDataObject(booking);
			var localClientDataObject = bookingDataObject.OrganizationAddressCollection.FirstOrDefault(AddressTypes.SendersLocalClient);
			OrganizationAddressTestHelper.AssertOrganizationBO_CRAHOLSYD(AddressTypes.SendersLocalClient, localClientDataObject, AddressTypes.SendersLocalClient);
		}

		public void TestNotes()
		{
			var bookingBO = Helper.CreateBooking();
			var noteBO1 = bookingBO.Notes.AddNew(false, "Goods Handling Instructions", "Highly explosive");
			var noteBO2 = bookingBO.Notes.AddNew(true, "Test Description", "Test note text");
			noteBO2.ST_NoteType = nameof(CargoWise.Definitions.StmNoteVisibility.PUB);
			var bookingDataObject = new DtbBookingDataObjectWriter(new Dictionary<ZGuid, ZInt>(), new DataWritingManager(new DummyActionInfo()), false).GetDataObject(bookingBO);
			AssertNotNull("bookingDataObject", bookingDataObject);
			AssertNotNull("bookingDataObject.NoteCollection", bookingDataObject.NoteCollection);
			AssertEquals("bookingDataObject.NoteCollection.Count", 2, bookingDataObject.NoteCollection.Count);

			var note1 = bookingDataObject.NoteCollection[0];

			CombineAssertions(delegate
			{
				AssertEquals("note1.Description", "Goods Handling Instructions", note1.Description);
				AssertEquals("note1.IsCustomDescription", false, note1.IsCustomDescription);
				AssertEquals("note1.NoteText", "Highly explosive", note1.NoteText);
				AssertEquals("note1.NoteContext.Description", "Module: A - All, Direction: A - All, Freight: A - All", note1.NoteContext.Description);
				AssertEquals("note1.Visibility.Code", "PUB", note1.Visibility.Code);
				AssertEquals("note1.Visibility.Description", "CLIENT-VISIBLE", note1.Visibility.Description);
			});

			var note2 = bookingDataObject.NoteCollection[1];

			CombineAssertions(delegate
			{
				AssertEquals("note2.Description", "Test Description", note2.Description);
				AssertEquals("note2.IsCustomDescription", true, note2.IsCustomDescription);
				AssertEquals("note2.NoteText", "Test note text", note2.NoteText);
				AssertEquals("note2.NoteContext.Description", "Module: A - All, Direction: A - All, Freight: A - All", note2.NoteContext.Description);
				AssertEquals("note2.Visibility.Code", "PUB", note2.Visibility.Code);
				AssertEquals("note2.Visibility.Description", "CLIENT-VISIBLE", note2.Visibility.Description);
			});
		}

		public void TestJobCostingInformationIsExportedIfRecipientRoleIsTransportCompany()
		{
			var branch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			branch.FillWithValidTestData();
			branch.GB_Code = "AST";
			var booking = Helper.CreateBooking();
			var job = new JobHeader.Loader(booking).TryCreate();
			job.JH_GB = branch.PK;
			Factory.Save();

			var writer1 = new DtbBookingDataObjectWriter(new Dictionary<ZGuid, ZInt>(), new DataWritingManager(new DummyActionInfo()), false);
			var bookingDataObject1 = writer1.GetDataObject(booking);
			AssertNull(bookingDataObject1.JobCosting);

			var writer2 = new DtbBookingDataObjectWriter(new Dictionary<ZGuid, ZInt>(), new DataWritingManager(new ActionInfo(RecipientRoleType.TPC, booking)), false);
			var bookingDataObject2 = writer2.GetDataObject(booking);
			AssertNull(bookingDataObject2.JobCosting);

			booking.Address.OrganisationPK = GlbCompany.CurrentCompany.OrgProxy.PK;
			Factory.Save();
			var writer3 = new DtbBookingDataObjectWriter(new Dictionary<ZGuid, ZInt>(), new DataWritingManager(new ActionInfo(RecipientRoleType.TPC, booking)), false);
			var bookingDataObject3 = writer3.GetDataObject(booking);
			AssertNotNull(bookingDataObject3.JobCosting);
			AssertEquals("AST", bookingDataObject3.JobCosting.Branch.GetCodeAsUpperCase());
		}

		public void TestImportMetaDataIsIncludedInJobCostingForOneDeliveryInstruction()
		{
			var branch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			branch.FillWithValidTestData();
			branch.GB_Code = "AST";
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);

			var code = Factory.New<AccChargeCode>();
			code.AC_Code = "TST";
			code.AC_Desc = "test charege code";
			code.AC_GC = GlbCompany.CurrentCompany.PK;

			var job = new JobHeader.Loader(booking).TryCreate();
			job.JH_GB = branch.PK;

			var charge = Factory.New<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_GE = job.JH_GE;
			charge.JR_GB = job.JH_GB;
			charge.JR_AC = code.PK;
			charge.JR_LocalSellAmt = 1;
			charge.JR_OSSellAmt = 1;
			charge.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.TransportBooking;

			var pickupOrganisation = Helper.CreateOrganisation("PORG");
			var deliveryOrganisation1 = Helper.CreateOrganisation("DORG");
			var deliveryOrganisation2 = Helper.CreateOrganisation("KOPL");

			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, pickupOrganisation.MainAddress);
			Helper.CreateConfirmation(picInstruction, ConfirmationTypes.Codes.PickUp);

			var dlvInstruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, deliveryOrganisation1.MainAddress);
			Helper.CreateConfirmation(dlvInstruction1, ConfirmationTypes.Codes.Delivery);

			booking.Address.OrganisationPK = GlbCompany.CurrentCompany.OrgProxy.PK;

			Factory.Save();

			var writer = new DtbBookingDataObjectWriter(new Dictionary<ZGuid, ZInt>(), new DataWritingManager(new ActionInfo(RecipientRoleType.TPC, booking)), false);
			var bookingDataObject = writer.GetDataObject(booking);
			AssertNotNull("bookingDataObject", bookingDataObject);
			AssertNotNull("bookingDataObject.ImportMetaData", bookingDataObject.JobCosting.ChargeLineCollection[0].ImportMetaData);
			AssertEquals("bookingDataObject.ImportMetaData.Instruction", InstructionType.Insert, bookingDataObject.JobCosting.ChargeLineCollection[0].ImportMetaData.Instruction);

			var dlvInstruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, deliveryOrganisation2.MainAddress);
			Helper.CreateConfirmation(dlvInstruction2, ConfirmationTypes.Codes.Delivery);

			Factory.Save();

			writer = new DtbBookingDataObjectWriter(new Dictionary<ZGuid, ZInt>(), new DataWritingManager(new ActionInfo(RecipientRoleType.TPC, booking)), false);
			bookingDataObject = writer.GetDataObject(booking);
			AssertNotNull("bookingDataObject", bookingDataObject);
			AssertNull("bookingDataObject.ImportMetaData", bookingDataObject.JobCosting.ChargeLineCollection[0].ImportMetaData);
		}

		public void TestImportMetaDataIsNotIncludedInJobCostingForOneDeliveryInstructionWithParent()
		{
			var branch = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew();
			branch.FillWithValidTestData();
			branch.GB_Code = "AST";

			var dummyBO = Factory.New<DummyWithDtbBooking>();
			var consolidation = Helper.CreateConsolidation(dummyBO);
			var booking = Helper.CreateBooking(consolidation);

			var code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_Code = "TST";
			code.AC_Desc = "test charege code";
			code.AC_GC = GlbCompany.CurrentCompany.PK;

			var job = new JobHeader.Loader(dummyBO).TryCreate();
			job.JH_GB = branch.PK;

			Factory.Save();

			var charge = Factory.New<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_GE = job.JH_GE;
			charge.JR_GB = job.JH_GB;
			charge.JR_AC = code.PK;
			charge.JR_LocalSellAmt = 1;
			charge.JR_OSSellAmt = 1;
			charge.ChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.TransportBooking;

			var pickupOrganisation = Helper.CreateOrganisation("PORG");
			var deliveryOrganisation1 = Helper.CreateOrganisation("DORG");
			var deliveryOrganisation2 = Helper.CreateOrganisation("KOPL");

			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNR, pickupOrganisation.MainAddress);
			Helper.CreateConfirmation(picInstruction, ConfirmationTypes.Codes.PickUp);

			var dlvInstruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, deliveryOrganisation1.MainAddress);
			Helper.CreateConfirmation(dlvInstruction1, ConfirmationTypes.Codes.Delivery);

			booking.Address.OrganisationPK = GlbCompany.CurrentCompany.OrgProxy.PK;

			var dlvInstruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, deliveryOrganisation2.MainAddress);
			Helper.CreateConfirmation(dlvInstruction2, ConfirmationTypes.Codes.Delivery);

			Factory.Save();

			var writer = new DtbBookingDataObjectWriter(new Dictionary<ZGuid, ZInt>(), new DataWritingManager(new ActionInfo(RecipientRoleType.TPC, booking)), false);
			var bookingDataObject = writer.GetDataObject(booking);
			AssertNotNull("bookingDataObject", bookingDataObject);
			AssertNull("bookingDataObject.ImportMetaData", bookingDataObject.JobCosting.ChargeLineCollection[0].ImportMetaData);
		}

		public void TestValidateForSendingToCTO_WhenValidationPasses()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			EnsureBookingHasCorrectDetailsToBeExportedToCTO(booking);
			booking.KM_Status = TransportStatuses.Codes.Available;

			Factory.Save();

			var factoryForUniversalDataHook = new BusinessObjectFactory();
			var actionInfoWithCBARecipientRole = new ActionInfo(RecipientRoleType.CBA.ToRecipientRoleDetails(), booking.ConsolidationSingleJob, factoryForUniversalDataHook);
			var writingManager = new DataWritingManager(actionInfoWithCBARecipientRole);
			var writer = new DtbBookingDataObjectWriter(new Dictionary<ZGuid, ZInt>(), writingManager, false);

			var bookingInFactoryForUniversalDataHook = writingManager.Action.FactoryForProcessing.Load<DtbBooking>(booking.PK);
			UniversalShipment bookingDataObject = null;

			AssertNoExceptionThrown("Should not throw exception when validating sending XUS to CTO", () =>
			{
				bookingDataObject = writer.GetDataObject(booking);
			});

			CombineAssertions(() =>
			{
				AssertNotNull("Data object should have been created.", bookingDataObject);
				AssertEquals("Booking should have no errors in NotificationBufferForSendingXUSToCTO", false, booking.NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors() || booking.NotificationBufferForSendingXUSToCTO.Events.HasErrors());
				AssertEquals("Booking in Factory for universal data hook should NOT have errors in NotificationBufferForSendingXUSToCTO", false, bookingInFactoryForUniversalDataHook.NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors() || bookingInFactoryForUniversalDataHook.NotificationBufferForSendingXUSToCTO.Events.HasErrors());
				AssertEquals("IsCurrentlyValidatingSendingXUSToCTO should have been reset to false.", false, booking.IsValidatingSendingXUSToCTO);
			});
		}

		public void TestValidateForSendingToCTO_WhenValidationFailsWithBothErrorsAndMessageErrors()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			EnsureBookingHasCorrectDetailsToBeExportedToCTO(booking);
			booking.KM_Status = TransportStatuses.Codes.Available;

			Factory.Save();

			var instructionWithError = Helper.CreateInstructionForBookingThatWillPassValidationForSendingXUSToCTO(booking, instructionType: ZString.Empty, OrganisationTypesList.Codes.CNR, "CONT4444441");
			booking.AdditionalReferenceNumbers.Delete();

			var factoryForUniversalDataHook = new BusinessObjectFactory();
			var actionInfoWithCBARecipientRole = new ActionInfo(RecipientRoleType.CBA.ToRecipientRoleDetails(), booking.ConsolidationSingleJob, factoryForUniversalDataHook);
			var writingManager = new DataWritingManager(actionInfoWithCBARecipientRole);
			var writer = new DtbBookingDataObjectWriter(new Dictionary<ZGuid, ZInt>(), writingManager, false);

			var bookingInFactoryForUniversalDataHook = writingManager.Action.FactoryForProcessing.Load<DtbBooking>(booking.PK);
			UniversalShipment bookingDataObject = null;

			var validationExceptionMessage = string.Empty;
			AssertExceptionThrown("Because the Booking does not have a Carrier Booking Reference and an instruction has no type, validation should have failed.", typeof(DataObjectValidationException), () =>
			{
				try
				{
					bookingDataObject = writer.GetDataObject(booking);
				}
				catch (Exception ex) when (ex is DataObjectValidationException)
				{
					validationExceptionMessage = ex.Message;
					throw;
				}
			});

			CombineAssertions(() =>
			{
				AssertContains("Expection message should contain message error message.", booking.NotificationBufferForSendingXUSToCTO.Events.GetMessageErrors().First().Message, validationExceptionMessage);
				AssertContains("Expection message should contain error message.", booking.NotificationBufferForSendingXUSToCTO.Events.GetErrors().First().Message, validationExceptionMessage);
				AssertNull("Data object should not have been created.", bookingDataObject);
				AssertEquals("Booking should have errors in NotificationBufferForSendingXUSToCTO", true, booking.NotificationBufferForSendingXUSToCTO.Events.HasErrors());
				AssertEquals("Booking should have message errors in NotificationBufferForSendingXUSToCTO", true, booking.NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors());
				AssertEquals("Booking in Factory for universal data hook should have errors in NotificationBufferForSendingXUSToCTO", true, bookingInFactoryForUniversalDataHook.NotificationBufferForSendingXUSToCTO.Events.HasErrors());
				AssertEquals("Booking in Factory for universal data hook should have message errors in NotificationBufferForSendingXUSToCTO", true, bookingInFactoryForUniversalDataHook.NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors());
				AssertEquals("IsCurrentlyValidatingSendingXUSToCTO should have been reset to false.", false, booking.IsValidatingSendingXUSToCTO);
			});
		}

		public void TestValidateForSendingToCTO_WhenValidationFailsWithOnlyMessageErrors()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			EnsureBookingHasCorrectDetailsToBeExportedToCTO(booking);
			booking.KM_Status = TransportStatuses.Codes.Available;

			Factory.Save();

			booking.AdditionalReferenceNumbers.Delete();

			var factoryForUniversalDataHook = new BusinessObjectFactory();
			var actionInfoWithCBARecipientRole = new ActionInfo(RecipientRoleType.CBA.ToRecipientRoleDetails(), booking.ConsolidationSingleJob, factoryForUniversalDataHook);
			var writingManager = new DataWritingManager(actionInfoWithCBARecipientRole);
			var writer = new DtbBookingDataObjectWriter(new Dictionary<ZGuid, ZInt>(), writingManager, false);

			var bookingInFactoryForUniversalDataHook = writingManager.Action.FactoryForProcessing.Load<DtbBooking>(booking.PK);
			UniversalShipment bookingDataObject = null;

			var validationExceptionMessage = string.Empty;
			AssertExceptionThrown("Because the Booking does not have a Carrier Booking Reference, validation should have failed.", typeof(DataObjectValidationException), () =>
			{
				try
				{
					bookingDataObject = writer.GetDataObject(booking);
				}
				catch (Exception ex) when (ex is DataObjectValidationException)
				{
					validationExceptionMessage = ex.Message;
					throw;
				}
			});

			CombineAssertions(() =>
			{
				AssertContains("Expection message should contain message error message.", booking.NotificationBufferForSendingXUSToCTO.Events.GetMessageErrors().First().Message, validationExceptionMessage);
				AssertNull("Data object should not have been created.", bookingDataObject);
				AssertEquals("Booking should have NO errors in NotificationBufferForSendingXUSToCTO", false, booking.NotificationBufferForSendingXUSToCTO.Events.HasErrors());
				AssertEquals("Booking should have message errors in NotificationBufferForSendingXUSToCTO", true, booking.NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors());
				AssertEquals("Booking in Factory for universal data hook should have NO errors in NotificationBufferForSendingXUSToCTO", false, bookingInFactoryForUniversalDataHook.NotificationBufferForSendingXUSToCTO.Events.HasErrors());
				AssertEquals("Booking in Factory for universal data hook should have message errors in NotificationBufferForSendingXUSToCTO", true, bookingInFactoryForUniversalDataHook.NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors());
				AssertEquals("IsCurrentlyValidatingSendingXUSToCTO should have been reset to false.", false, booking.IsValidatingSendingXUSToCTO);
			});
		}

		public void TestValidateForSendingToCTO_WhenValidationFailsWithOnlyErrors()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			EnsureBookingHasCorrectDetailsToBeExportedToCTO(booking);
			booking.KM_Status = TransportStatuses.Codes.Available;

			Factory.Save();

			var instructionWithError = Helper.CreateInstructionForBookingThatWillPassValidationForSendingXUSToCTO(booking, instructionType: ZString.Empty, OrganisationTypesList.Codes.CNR, "CONT4444441");

			var factoryForUniversalDataHook = new BusinessObjectFactory();
			var actionInfoWithCBARecipientRole = new ActionInfo(RecipientRoleType.CBA.ToRecipientRoleDetails(), booking.ConsolidationSingleJob, factoryForUniversalDataHook);
			var writingManager = new DataWritingManager(actionInfoWithCBARecipientRole);
			var writer = new DtbBookingDataObjectWriter(new Dictionary<ZGuid, ZInt>(), writingManager, false);

			var bookingInFactoryForUniversalDataHook = writingManager.Action.FactoryForProcessing.Load<DtbBooking>(booking.PK);
			UniversalShipment bookingDataObject = null;

			var validationExceptionMessage = string.Empty;
			AssertExceptionThrown("Because an instruction on the Booking has no type, validation should have failed.", typeof(DataObjectValidationException), () =>
			{
				try
				{
					bookingDataObject = writer.GetDataObject(booking);
				}
				catch (Exception ex) when (ex is DataObjectValidationException)
				{
					validationExceptionMessage = ex.Message;
					throw;
				}
			});

			CombineAssertions(() =>
			{
				AssertContains("Expection message should contain error message.", booking.NotificationBufferForSendingXUSToCTO.Events.GetErrors().First().Message, validationExceptionMessage);
				AssertNull("Data object should not have been created.", bookingDataObject);
				AssertEquals("Booking should have errors in NotificationBufferForSendingXUSToCTO", true, booking.NotificationBufferForSendingXUSToCTO.Events.HasErrors());
				AssertEquals("Booking should have NO message errors in NotificationBufferForSendingXUSToCTO", false, booking.NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors());
				AssertEquals("Booking in Factory for universal data hook should have errors in NotificationBufferForSendingXUSToCTO", true, bookingInFactoryForUniversalDataHook.NotificationBufferForSendingXUSToCTO.Events.HasErrors());
				AssertEquals("Booking in Factory for universal data hook should have NO message errors in NotificationBufferForSendingXUSToCTO", false, bookingInFactoryForUniversalDataHook.NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors());
				AssertEquals("IsCurrentlyValidatingSendingXUSToCTO should have been reset to false.", false, booking.IsValidatingSendingXUSToCTO);
			});
		}

		public void TestValidateForSendingToCTO_WhenBookingIsNotAvailable()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			EnsureBookingHasCorrectDetailsToBeExportedToCTO(booking);
			booking.KM_Status = TransportStatuses.Codes.Held;

			Factory.Save();

			var factoryForUniversalDataHook = new BusinessObjectFactory();
			var actionInfoWithCBARecipientRole = new ActionInfo(RecipientRoleType.CBA.ToRecipientRoleDetails(), booking.ConsolidationSingleJob, factoryForUniversalDataHook);
			var writingManager = new DataWritingManager(actionInfoWithCBARecipientRole);
			var writer = new DtbBookingDataObjectWriter(new Dictionary<ZGuid, ZInt>(), writingManager, false);

			var bookingInFactoryForUniversalDataHook = writingManager.Action.FactoryForProcessing.Load<DtbBooking>(booking.PK);
			UniversalShipment bookingDataObject = null;

			AssertExceptionThrown(typeof(DataObjectValidationException), "Only available bookings can be sent to Container Transport Optimization", () =>
			{
				bookingDataObject = writer.GetDataObject(booking);
			});

			CombineAssertions(() =>
			{
				AssertNull("Data object should NOT have been created.", bookingDataObject);
				AssertEquals("Booking should have no errors in NotificationBufferForSendingXUSToCTO", false, booking.NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors() || booking.NotificationBufferForSendingXUSToCTO.Events.HasErrors());
				AssertEquals("Booking in Factory for universal data hook should NOT have errors in NotificationBufferForSendingXUSToCTO", false, bookingInFactoryForUniversalDataHook.NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors() || bookingInFactoryForUniversalDataHook.NotificationBufferForSendingXUSToCTO.Events.HasErrors());
				AssertEquals("IsCurrentlyValidatingSendingXUSToCTO should still be false.", false, booking.IsValidatingSendingXUSToCTO);
			});
		}

		public void TestValidateForSendingToCTOIfRequired_WhenNotSendingToCarrierBookingAgent()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			EnsureBookingHasCorrectDetailsToBeExportedToCTO(booking);
			booking.KM_Status = TransportStatuses.Codes.Held;

			Factory.Save();

			var factoryForUniversalDataHook = new BusinessObjectFactory();
			var actionInfoWithNoCBARecipientRole = new ActionInfo(RecipientRoleType.CAP.ToRecipientRoleDetails(), booking.ConsolidationSingleJob, factoryForUniversalDataHook);
			var writingManager = new DataWritingManager(actionInfoWithNoCBARecipientRole);
			var writer = new DtbBookingDataObjectWriter(new Dictionary<ZGuid, ZInt>(), writingManager, false);

			AssertEquals("Precondition: There should be no CBA recipient role in the Action Info.", false, actionInfoWithNoCBARecipientRole.RecipientRoleDetails.Any(r => r.Type == RecipientRoleType.CBA));

			var bookingInFactoryForUniversalDataHook = writingManager.Action.FactoryForProcessing.Load<DtbBooking>(booking.PK);
			UniversalShipment bookingDataObject = null;

			AssertNoExceptionThrown("Despite the KM_Status of the booking guaranteeing an XUS to CTO validation failure if that validation were run, no exception should be thrown (meaning the XUS to CTO validation should not have run).", () =>
			{
				bookingDataObject = writer.GetDataObject(booking);
			});

			CombineAssertions(() =>
			{
				AssertNotNull("Data object should have been created.", bookingDataObject);
				AssertEquals("Booking should have no errors in NotificationBufferForSendingXUSToCTO", false, booking.NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors() || booking.NotificationBufferForSendingXUSToCTO.Events.HasErrors());
				AssertEquals("Booking in Factory for universal data hook should NOT have errors in NotificationBufferForSendingXUSToCTO", false, bookingInFactoryForUniversalDataHook.NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors() || bookingInFactoryForUniversalDataHook.NotificationBufferForSendingXUSToCTO.Events.HasErrors());
				AssertEquals("IsCurrentlyValidatingSendingXUSToCTO should still be false.", false, booking.IsValidatingSendingXUSToCTO);
			});
		}

		public void TestValidateForSendingToCTOIfRequired_WhenNotSendingToContainerTransportOptimization()
		{
			var booking = Helper.CreateBookingThatWillPassValidationForSendingXUSToCTO();
			EnsureBookingHasCorrectDetailsToBeExportedToCTO(booking);

			var ctoCommMode = (EDICommunicationsMode)booking.CarrierBookingAgent.EDICommunicationsModes.Single();
			ctoCommMode.EK_Destination = "NOT_CTO";

			booking.KM_Status = TransportStatuses.Codes.Held;

			Factory.Save();

			var factoryForUniversalDataHook = new BusinessObjectFactory();
			var actionInfoWithCBARecipientRole = new ActionInfo(RecipientRoleType.CBA.ToRecipientRoleDetails(), booking.ConsolidationSingleJob, factoryForUniversalDataHook);
			var writingManager = new DataWritingManager(actionInfoWithCBARecipientRole);
			var writer = new DtbBookingDataObjectWriter(new Dictionary<ZGuid, ZInt>(), writingManager, false);

			var bookingInFactoryForUniversalDataHook = writingManager.Action.FactoryForProcessing.Load<DtbBooking>(booking.PK);
			UniversalShipment bookingDataObject = null;

			AssertNoExceptionThrown("Despite the KM_Status of the booking guaranteeing an XUS to CTO validation failure if that validation were run, no exception should be thrown (meaning the XUS to CTO validation should not have run).", () =>
			{
				bookingDataObject = writer.GetDataObject(booking);
			});

			CombineAssertions(() =>
			{
				AssertNotNull("Data object should have been created.", bookingDataObject);
				AssertEquals("Booking should have no errors in NotificationBufferForSendingXUSToCTO", false, booking.NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors() || booking.NotificationBufferForSendingXUSToCTO.Events.HasErrors());
				AssertEquals("Booking in Factory for universal data hook should NOT have errors in NotificationBufferForSendingXUSToCTO", false, bookingInFactoryForUniversalDataHook.NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors() || bookingInFactoryForUniversalDataHook.NotificationBufferForSendingXUSToCTO.Events.HasErrors());
				AssertEquals("IsCurrentlyValidatingSendingXUSToCTO should still be false.", false, booking.IsValidatingSendingXUSToCTO);
			});
		}

		void EnsureBookingHasCorrectDetailsToBeExportedToCTO(DtbBooking booking)
		{
			var carrierBookingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var ctoCommMode = carrierBookingAgent.EDICommunicationsModes.AddNew();

			ctoCommMode.EK_Module = WorkflowDescriptors.DtbBookingWorkflowDescriptorCode;
			ctoCommMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			ctoCommMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			ctoCommMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			ctoCommMode.EK_Destination = DtbAgentBooking.ContainerTransportOptimizationCBA;
			booking.CarrierBookingAgentDocAddress.E2_OA_Address = carrierBookingAgent.MainAddress.PK;
		}
	}
}
