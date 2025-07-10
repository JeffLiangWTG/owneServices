using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	abstract class WhsTransitConsignmentDataObjectReaderTest<TBizO, TReader> : TransitUniversalTestCase
		where TBizO : BusinessObject, IDocAddresses, IUniversalXMLNoteParent, IHaveServices, IHaveCusEntryNumReferences
		where TReader : WhsTransitConsignmentDataObjectReader<TBizO>
	{
		#region TestAttemptToGetConsignmentWithMatchingAddress

		public void TestMatchOnJobLink_MultipleLinks_MatchesLatestConsignment()
		{
			SetupForBaseTests();
			Data.ShipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });
			AdditionalSetupForImport(Data.ShipmentDataObject);

			var matchingHelper = GetMatchingHelper(Factory, GlbCompany.CurrentCompany.OrgProxy, WarehouseMatchingHelper.GetWarehouse(Data.ShipmentDataObject, Factory, Logger), Logger);
			var createdConsignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			DeleteJobLinks(Factory);
			AssertEquals(createdConsignment, matchingHelper.MatchConsignment(TestDataForUniversal.DefaultConsignmentID, "", Data.ShipmentDataObject));

			Data.ShipmentDataObject.OrganizationAddressCollection.Add(Data.Orgs.Consignee_INTHEMSYD);
			AdditionalSetupForImport(Data.ShipmentDataObject);
			var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			DeleteJobLinks(Factory);

			// test real address
			AssertEquals(consignment, matchingHelper.MatchConsignment(TestDataForUniversal.DefaultConsignmentID, "", Data.ShipmentDataObject));
			AssertNull("Should not match if there are no Consignments matching the Consignment ID.", matchingHelper.MatchConsignment("RANDOM", "", Data.ShipmentDataObject));

			// test overriden address
			var localCartageExporter = consignment.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageExporter);
			localCartageExporter.E2_AddressOverride = true;
			AssertEquals(consignment, matchingHelper.MatchConsignment(TestDataForUniversal.DefaultConsignmentID, "", Data.ShipmentDataObject));

			var consignor = Data.ShipmentDataObject.OrganizationAddressCollection.FirstOrDefault((ZString)nameof(DocAddressType.ConsignorDocumentaryAddress));
			ChangeFieldOnAddressDOAndAssertStillMatching(matchingHelper, f => consignor.Address1 = f, () => consignor.Address1, consignment);
			ChangeFieldOnAddressDOAndAssertStillMatching(matchingHelper, f => consignor.Address2 = f, () => consignor.Address2, consignment);
			ChangeFieldOnAddressDOAndAssertStillMatching(matchingHelper, f => consignor.CompanyName = f, () => consignor.CompanyName, consignment);
			ChangeFieldOnAddressDOAndAssertStillMatching(matchingHelper, f => consignor.City = f, () => consignor.City, consignment);
			ChangeFieldOnAddressDOAndAssertStillMatching(matchingHelper, f => consignor.State = f, () => consignor.State, consignment);

			AssertEquals(consignment, matchingHelper.MatchConsignment(TestDataForUniversal.DefaultConsignmentID, "", Data.ShipmentDataObject));
		}

		public void TestAttemptToGetConsignmentWithMatchingAddress_Instructions()
		{
			SetupForBaseTests();
			AdditionalSetupForImport(Data.ShipmentDataObject);
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DTW } } });

			var matchingHelper = GetMatchingHelper(Factory, GlbCompany.CurrentCompany.OrgProxy, WarehouseMatchingHelper.GetWarehouse(Data.ShipmentDataObject, Factory, Logger), Logger);
			var createdConsignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			DeleteJobLinks(Factory);

			// change shipment data to match with xml coming from runsheets
			Data.ShipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Warehouse_WUFSHIJNB });
			var exporter = Data.Orgs.Consignor_CRAHOLSYD;
			exporter.AddressType = nameof(DocAddressType.LocalCartageExporter);

			var importer = Data.Orgs.Consignee_INTHEMSYD;
			importer.AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);

			var cfs = Data.Orgs.Warehouse_WUFSHIJNB;
			cfs.AddressType = nameof(DocAddressType.LocalCartageCFS);

			Data.ShipmentDataObject.SetInstructionCollection(() =>
			{
				var instructions = new DataObjectList<Instruction>();
				instructions.Add(new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { Address = exporter });
				instructions.Add(new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { Address = cfs });

				return instructions;
			});

			AssertEquals(createdConsignment, matchingHelper.MatchConsignment(TestDataForUniversal.DefaultConsignmentID, "", Data.ShipmentDataObject));

			Data.ShipmentDataObject.InstructionCollection.Add(new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { Address = importer });
			AdditionalSetupForImport(Data.ShipmentDataObject);
			var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			DeleteJobLinks(Factory);

			// test real address
			AssertEquals(consignment, matchingHelper.MatchConsignment(TestDataForUniversal.DefaultConsignmentID, "", Data.ShipmentDataObject));
			AssertNull("Should not match if there are no Consignments matching the Consignment ID.", matchingHelper.MatchConsignment("RANDOM", "", Data.ShipmentDataObject));

			// test overriden address
			var localCartageExporter = consignment.DocAddresses.FindByDocAddressType(DocAddressType.LocalCartageExporter);
			localCartageExporter.E2_AddressOverride = true;
			AssertEquals(consignment, matchingHelper.MatchConsignment(TestDataForUniversal.DefaultConsignmentID, "", Data.ShipmentDataObject));

			var consignor = Data.ShipmentDataObject.InstructionCollection.Select(i => i.Address).FirstOrDefault((ZString)nameof(DocAddressType.LocalCartageExporter));
			ChangeFieldOnAddressDOAndAssertStillMatching(matchingHelper, f => consignor.Address1 = f, () => consignor.Address1, consignment);
			ChangeFieldOnAddressDOAndAssertStillMatching(matchingHelper, f => consignor.Address2 = f, () => consignor.Address2, consignment);
			ChangeFieldOnAddressDOAndAssertStillMatching(matchingHelper, f => consignor.CompanyName = f, () => consignor.CompanyName, consignment);
			ChangeFieldOnAddressDOAndAssertStillMatching(matchingHelper, f => consignor.City = f, () => consignor.City, consignment);
			ChangeFieldOnAddressDOAndAssertStillMatching(matchingHelper, f => consignor.State = f, () => consignor.State, consignment);
		}

		public void TestConsignmentMatchingHelper_Logs_MultipleDataSources()
		{
			SetupForBaseTests();
			AdditionalSetupForImport(Data.ShipmentDataObject);
			Data.SetupNewDataContextWithDataSource(Data.ShipmentDataObject, consolNumber: "C1000000", shipmentNumber: "S1000000");

			var matchingHelper = GetMatchingHelper(Factory, GlbCompany.CurrentCompany.OrgProxy, Data.Warehouse.Row(), Logger);
			matchingHelper.MatchConsignment(string.Empty, string.Empty, Data.ShipmentDataObject);

			AssertContains("Should add a warning if there is more than one data source", "Warning - Consignment matching may not function correctly as multiple Data Sources were provided.", Logger.Logs);
		}

		void ChangeFieldOnAddressDOAndAssertStillMatching(WhsTransitConsignmentMatchingHelper<TBizO> matchingHelper, Action<ZString?> setField, Func<ZString?> getField, TBizO consignment)
		{
			var originalField = getField();
			setField("CHANGED");
			AssertEquals(consignment, matchingHelper.MatchConsignment(TestDataForUniversal.DefaultConsignmentID, "", Data.ShipmentDataObject));

			setField(originalField);
			AssertEquals(consignment, matchingHelper.MatchConsignment(TestDataForUniversal.DefaultConsignmentID, "", Data.ShipmentDataObject));
		}

		WhsTransitConsignmentMatchingHelper<TBizO> GetMatchingHelper(UniversalObjectFactory factory, IOrgHeader bookingParty, IColumnIndexer warehouse, IXmlImportLogger logger)
		{
			return new TestMatchingHelper(factory, bookingParty, warehouse, JobIDColumn, ConsignmentIDColumn, HouseBillNumberColumn, CreateTimeColumn, WarehouseColumn, PKColumn, CompleteTimeColumn, "", logger);
		}

		class TestMatchingHelper : WhsTransitConsignmentMatchingHelper<TBizO>
		{
			public TestMatchingHelper(UniversalObjectFactory factory, IOrgHeader bookingParty, IColumnIndexer warehouse, SchemaStringColumn consignmentJobIDColumn, SchemaStringColumn houseBillNumberColumn, SchemaStringColumn idColumn, SchemaDateTimeColumn createdColumn, SchemaGuidColumn warehouseColumn, SchemaGuidColumn pkColumn, SchemaDateTimeOffsetColumn completeTimeColumn, string jobDescription, IXmlImportLogger logger)
				: base(factory, bookingParty, warehouse, logger)
			{
				ConsignmentJobIDColumn = consignmentJobIDColumn;
				IDColumn = idColumn;
				CreatedColumn = createdColumn;
				WhsColumn = warehouseColumn;
				PKColumn = pkColumn;
				CompletedColumn = completeTimeColumn;
				JobDesc = jobDescription;
				HSBNumberColumn = houseBillNumberColumn;
			}

			readonly SchemaStringColumn ConsignmentJobIDColumn;
			readonly SchemaStringColumn HSBNumberColumn;
			readonly SchemaStringColumn IDColumn;
			readonly SchemaDateTimeColumn CreatedColumn;
			readonly SchemaGuidColumn WhsColumn;
			readonly SchemaGuidColumn PKColumn;
			readonly SchemaDateTimeOffsetColumn CompletedColumn;
			readonly string JobDesc;

			protected override SchemaStringColumn JobIDColumn => ConsignmentJobIDColumn;
			protected override SchemaStringColumn ConsignmentIDColumn => IDColumn;
			protected override SchemaStringColumn HouseBillNumberColumn => HSBNumberColumn;
			protected override SchemaDateTimeColumn CreatedTimeColumn => CreatedColumn;
			protected override SchemaGuidColumn WarehouseColumn => WhsColumn;
			protected override SchemaGuidColumn PKSchemaColumn => PKColumn;
			protected override SchemaDateTimeOffsetColumn CompleteTimeColumn => CompletedColumn;
			protected override ZString JobDescription => JobDesc;
		}

		#endregion

		#region TestJobID

		public void TestJobID()
		{
			SetupForBaseTests();
			AdditionalSetupForImport(Data.ShipmentDataObject);

			var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject().Row();
			AssertEquals("The Consignment Reference should be taken from the waybill.", "Waybill123", consignment.GetValue(ConsignmentIDColumn));
			AssertEquals("The Job ID should be taken from the number fountain.", $"{JobIDPrefix}00000001", consignment.GetValue(JobIDColumn));

			var consignment2 = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject().Row();
			AssertEquals("The Consignment should have matched and updated the first.", consignment.GetValue(PKColumn), consignment2.GetValue(PKColumn));
			AssertEquals("The Consignment Reference should still be taken from the waybill.", "Waybill123", consignment2.GetValue(ConsignmentIDColumn));
			AssertEquals("The Job ID should still be the same.", $"{JobIDPrefix}00000001", consignment2.GetValue(JobIDColumn));
		}

		protected abstract SchemaStringColumn JobIDColumn { get; }
		protected abstract ZString JobIDPrefix { get; }
		protected abstract SchemaGuidColumn PKColumn { get; }

		#endregion

		#region TestConsignmentIDDefaulting

		public void TestConsignmentIDDefaulting()
		{
			SetupForBaseTests();
			AdditionalSetupForImport(Data.ShipmentDataObject);

			var wayBillConsignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject().Row();
			AssertEquals("The ID should be taken from the waybill.", "Waybill123", wayBillConsignment.GetValue(ConsignmentIDColumn));

			Data.ShipmentDataObject.WayBillNumber = "";
			AdditionalSetupForImport(Data.CreateShipmentWithPackages("S1000000", "ID0"));

			Logger.TopLevelDataObject = Data.ShipmentDataObject;
			var noWayBillConsignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject().Row();
			AssertEquals("The ID should be taken from the S Number if no waybill exists.", "S1000000", noWayBillConsignment.GetValue(ConsignmentIDColumn));
		}

		protected abstract SchemaStringColumn ConsignmentIDColumn { get; }

		#endregion

		#region TestHouseBillNumber

		public void TestHouseBillNumber()
		{
			SetupForBaseTests();
			AdditionalSetupForImport(Data.ShipmentDataObject);

			var wayBillConsignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject().Row();

			AssertEquals("The House Bill Number should be taken from the waybill.", "Waybill123", wayBillConsignment.GetValue(HouseBillNumberColumn));
		}

		protected abstract SchemaStringColumn HouseBillNumberColumn { get; }

		#endregion

		#region TestServiceLevel

		public void TestServiceLevel()
		{
			SetupForBaseTests();
			Data.HeaderDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			AdditionalSetupForImport(Data.ShipmentDataObject);

			Data.ShipmentDataObject.ServiceLevel = new ServiceLevel { Code = "TST", Description = "TEST" };
			var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject().Row();
			AssertEquals("The service level should be taken from data object.", "TST", consignment.GetValue(ServiceLevelColumn));
		}

		protected abstract SchemaStringColumn ServiceLevelColumn { get; }

		#endregion

		#region TestWarehouse

		public void TestWarehouse()
		{
			SetupForBaseTests();
			AdditionalSetupForImport(Data.ShipmentDataObject);

			var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject().Row();
			AssertEquals("The warehouse should be taken from data object.", Data.Warehouse.PK, consignment.GetValue(WarehouseColumn));
		}

		public void TestGetWarehouseFromPremise_Arrival() => TestGetWarehouseFromPremise(true);

		public void TestGetWarehouseFromPremise_Departure() => TestGetWarehouseFromPremise(false);

		void TestGetWarehouseFromPremise(bool arrival)
		{
			SetupForBaseTests();
			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.WarehouseCRAHOLSYD, "9922W", CountryCodes.Australia);
			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.WarehouseINTHEMSYD, "9914N", CountryCodes.Australia);

			var recipientRoleType = arrival ? RecipientRoleType.ATW : RecipientRoleType.DTW;
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = recipientRoleType } } });

			Data.ShipmentDataObject.SetOrganizationAddressCollection(() => null);

			var types = new[] { (WarehouseAdditionalReferenceTypes.Codes.DestinationPremiseID, "9914N", "CN"), (WarehouseAdditionalReferenceTypes.Codes.OriginPremiseID, "9922W", "CN") };
			Helper.SetShipmentAdditionalReference(Data.ShipmentDataObject, types);

			AdditionalSetupForImport(Data.ShipmentDataObject);
			var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();

			if (arrival)
			{
				AssertEquals("The warehouse should be taken from premise id 9914N.", Data.WarehouseINTHEMSYD.PK, consignment.GetValue(WarehouseColumn));
			}
			else
			{
				AssertEquals("The warehouse should be taken from premise id 9922W.", Data.WarehouseCRAHOLSYD.PK, consignment.GetValue(WarehouseColumn));
			}
		}

		protected abstract SchemaGuidColumn WarehouseColumn { get; }

		protected abstract SchemaStringColumn DestinationColumn { get; }

		#endregion

		#region TestCompleteTime

		public void TestCompleteTime()
		{
			SetupForBaseTests();
			AdditionalSetupForImport(Data.ShipmentDataObject);

			var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject().Row();
			AssertEquals("The complete time should be taken from data object.", ZDateTimeOffset.Empty, consignment.GetValue(CompleteTimeColumn));
		}

		protected abstract SchemaDateTimeOffsetColumn CompleteTimeColumn { get; }

		#endregion

		#region TestGetExistingBusinessObjectUsingModuleSpecificBusinessRules

		public void TestGetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			SetupForBaseTests();
			Data.ShipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Consignor_CRAHOLSYD, Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });

			AdditionalSetupForImport(Data.ShipmentDataObject);

			var originalConsignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			DeleteJobLinks(Factory);
			Factory.SaveForTesting();

			var consignmentFound = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			DeleteJobLinks(Factory);
			AssertEquals("The consignments should match.", originalConsignment.PK, consignmentFound.PK);

			var otherConsignor = OrganizationAddressTestHelper.GetNewAddressData_WUFSHIJNB(DocAddressType.ConsignorDocumentaryAddress);
			Data.ShipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Warehouse_WUFSHIJNB, otherConsignor, Data.Orgs.Warehouse_INTHEMSYD });

			AdditionalSetupForImport(Data.ShipmentDataObject);

			var consignmentForOtherConsignor = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			AssertEquals("Should have updated existing consignment.", originalConsignment.PK, consignmentFound.PK);
			DeleteJobLinks(Factory);
			Factory.SaveForTesting();

			var matchedConsignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			AssertEquals("Should have matched existing consignment.", originalConsignment.PK, matchedConsignment.PK);
			DeleteJobLinks(Factory);
			Factory.SaveForTesting();

			// create a duplicate consignment (matching addresses and ID)
			Data.ShipmentDataObject.WayBillNumber = "DUPLICATE"; // temporarily set the wrong ID to create a new consignment
			AdditionalSetupForImport(Data.ShipmentDataObject);

			var duplicateConsignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			DeleteJobLinks(Factory);
			AssertNotEquals("Precondition: Should create a new Consignment", matchedConsignment, duplicateConsignment);

			duplicateConsignment[ConsignmentIDColumn] = TestDataForUniversal.DefaultConsignmentID; // set consignment ID back to correct value
			duplicateConsignment[HouseBillNumberColumn] = TestDataForUniversal.DefaultConsignmentID; // set house bill number back to correct value
			duplicateConsignment[CreateTimeColumn] = ZDateTime.Now.AddDays(2); // make this consignment the latest
			Data.ShipmentDataObject.WayBillNumber = TestDataForUniversal.DefaultConsignmentID; // set consignment ID back to correct value
			Factory.SaveForTesting();

			AssertEquals("Should grab Latest Consignment if there are duplicates.", duplicateConsignment, GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject());
		}

		protected abstract SchemaDateTimeColumn CreateTimeColumn { get; }

		#endregion

		#region TestPopulateAddresses

		public void TestPopulateAddresses()
		{
			SetupForBaseTests();
			AdditionalSetupForImport(Data.ShipmentDataObject);

			var reader = GetDataObjectReader(Factory, Data.ShipmentDataObject);
			var consignment = reader.ReadIntoBusinessObject();
			var addresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, consignment.PK));
			var expectedAddresses = 3 + (reader is WhsTransitReceiveConsignmentDataObjectReader ? 1 : 0);

			AssertEquals("Should have imported all supported Addresses.", expectedAddresses, addresses.Length);

			var cneJDA = addresses.Single(a => a.E2_OA_Address == Data.Orgs.INTHEMSYD.MainAddress.PK);
			var cnrJDA = addresses.Single(a => a.E2_OA_Address == Data.Orgs.CRAHOLSYD.MainAddress.PK);
			var bkpJDA = addresses.Single(a => a.E2_OA_Address == GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK);
			AssertEquals("Should have imported Consignee Address.", DocAddressTypes.Codes.ConsigneeDocumentaryAddress, cneJDA.E2_AddressType);
			AssertEquals("Should have imported Consignor Address.", DocAddressTypes.Codes.LocalCartageExporter, cnrJDA.E2_AddressType);
			AssertEquals("Should have imported Booking party Address.", DocAddressTypes.Codes.BookingPartyDocumentaryAddress, bkpJDA.E2_AddressType);
			AssertAddressContentMatches_CRAHOLSYD(cnrJDA.Address);
			AssertAddressContentMatches_INTHEMSYD(cneJDA.Address);
		}

		public void TestPopulateCTOAddress()
		{
			SetupForBaseTests();

			var arrivalCTO = Data.Orgs.ArrivalCTOAddress_CRAHOLSYD;
			var departureCTO = Data.Orgs.DepartureCTOAddress_INTHEMSYD;
			Data.HeaderDataObject.OrganizationAddressCollection.Add(arrivalCTO);
			Data.HeaderDataObject.OrganizationAddressCollection.Add(departureCTO);

			var reader = GetDataObjectReader(Factory, Data.HeaderDataObject);
			var consignment = reader.ReadIntoBusinessObject();
			var addresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, consignment.PK));
			var expectedAddresses = 4 + (reader is WhsTransitReceiveConsignmentDataObjectReader ? 1 : 0);

			AssertEquals("Should have imported all supported Addresses.", expectedAddresses, addresses.Length);
			if (consignment is WhsItemReceiveConsignment)
			{
				var ctoJDA = addresses.Where(a => a.E2_AddressType == DocAddressTypes.Codes.ArrivalCTOAddress && a.E2_OA_Address == Data.Orgs.CRAHOLSYD.MainAddress.PK);
				AssertEquals("Should have imported Arrival CTO Address.", 1, ctoJDA.Count());
			}
			else
			{
				var ctoJDA = addresses.Where(a => a.E2_AddressType == DocAddressTypes.Codes.DepartureCTOAddress && a.E2_OA_Address == Data.Orgs.INTHEMSYD.MainAddress.PK);
				AssertEquals("Should have imported Departure CTO Address.", 1, ctoJDA.Count());
			}
		}

		public void TestPopulateCRBAddressWhenRegistryValueIsLCN() => TestPopulateCRBAddressCore(Data.Orgs.SendersLocalCLient_CRAHOLSYD, 4, DefaultBilltoPartyForTWConsignmentCodeList.Codes.LCN);

		public void TestPopulateCRBAddressWhenRegistryValueIsCED() => TestPopulateCRBAddressCore(Data.Orgs.ConsigneePickupDeliveryAddress_CRAHOLSYD, 4, DefaultBilltoPartyForTWConsignmentCodeList.Codes.CED);

		public void TestPopulateCRBAddressWhenRegistryValueIsSFA() => TestPopulateCRBAddressCore(Data.Orgs.SendingForwarderAddress_CRAHOLSYD, 4, DefaultBilltoPartyForTWConsignmentCodeList.Codes.SFA);

		public void TestPopulateCRBAddressWhenRegistryValueIsRFA() => TestPopulateCRBAddressCore(Data.Orgs.ReceivingForwarderAddress_CRAHOLSYD, 4, DefaultBilltoPartyForTWConsignmentCodeList.Codes.RFA);

		public void TestPopulateCRBAddressWhenRegistryValueIsBKD() => TestPopulateCRBAddressCore(Data.Orgs.BookingPartyAddress_CRAHOLSYD, 4, DefaultBilltoPartyForTWConsignmentCodeList.Codes.BKD);

		void TestPopulateCRBAddressCore(OrganizationAddress orgAddress, int expectedAddressesCount, string addressType)
		{
			SetupForBaseTests();

			var addressToSet = orgAddress;

			if (addressType == DefaultBilltoPartyForTWConsignmentCodeList.Codes.LCN || addressType == DefaultBilltoPartyForTWConsignmentCodeList.Codes.CED || addressType == DefaultBilltoPartyForTWConsignmentCodeList.Codes.BKD)
			{
				Data.ShipmentDataObject.OrganizationAddressCollection.Add(addressToSet);
			}

			if (addressType == DefaultBilltoPartyForTWConsignmentCodeList.Codes.SFA || addressType == DefaultBilltoPartyForTWConsignmentCodeList.Codes.RFA)
			{
				Data.HeaderDataObject.OrganizationAddressCollection.Add(addressToSet);
			}

			WarehouseDataRegistry.Instance.DefaultBilltoPartyForTWConsignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, addressType);

			var reader = GetDataObjectReader(Factory, Data.HeaderDataObject);
			var consignment = reader.ReadIntoBusinessObject();
			var addresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, consignment.PK));

			var expectedAddressesTotal = expectedAddressesCount
				+ (reader is WhsTransitReceiveConsignmentDataObjectReader ? 1 : 0)
				+ (addressType == DefaultBilltoPartyForTWConsignmentCodeList.Codes.CED && consignment is WhsItemDispatchConsignment ? 1 : 0);

			AssertEquals("Should have imported all supported Addresses.", expectedAddressesTotal, addresses.Length);

			if (addressType == DefaultBilltoPartyForTWConsignmentCodeList.Codes.BKD)
			{
				var clientRequestedBillingAddress = addresses.FirstOrDefault(a => a.E2_AddressType == DocAddressTypes.Codes.ClientRequestedBillingParty);
				var bookingPartyAddress = addresses.FirstOrDefault(a => a.E2_AddressType == DocAddressTypes.Codes.BookingPartyDocumentaryAddress);

				bool isCRBPKMatchingBKDPK = bookingPartyAddress?.E2_OA_Address == clientRequestedBillingAddress?.E2_OA_Address;

				AssertEquals("Should have imported correct Address as Bill To Party Address.", true, clientRequestedBillingAddress != null && isCRBPKMatchingBKDPK);
			}
			else
			{
				var billToPartyAddress = addresses.Where(a => a.E2_AddressType == DocAddressTypes.Codes.ClientRequestedBillingParty && a.E2_OA_Address == Data.Orgs.CRAHOLSYD.MainAddress.PK);

				AssertEquals("Should have imported correct Address as Bill To Party Address.", 1, billToPartyAddress.Count());
			}
		}

		#endregion

		#region TestPopulateBusinessObject_BookingParty

		public void TestPopulateBusinessObject_BookingParty()
		{
			SetupForBaseTests();
			AdditionalSetupForImport(Data.ShipmentDataObject);
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(UniversalHelper.CreateOrganizationAddress("AUSAHI", DocAddressType.BookingPartyDocumentaryAddress));
			var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			DeleteJobLinks(Factory);
			var query = new ZQuery(JobDocAddressSchema.E2_ParentID, consignment.PK);
			query.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.BookingPartyDocumentaryAddress);
			var addresses1 = Factory.Load<JobDocAddress>(query);
			AssertEquals("Should have imported a Booking Party.", 1, addresses1.Length);

			var bookingParty1 = addresses1.Single();
			AssertEquals("Booking Party should be the sending system.",
				GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, bookingParty1.E2_OA_Address);
			Factory.SaveForTesting();

			AssertEquals("Address should have saved correctly.",
				GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK,
				new BusinessObjectFactory().Load<JobDocAddress>(query).Single().E2_OA_Address);

			var tempCompany = Factory.New<GlbCompany>();
			tempCompany.GC_Code = "FWD";
			Data.ShipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(tempCompany);

			var licenceCode = Data.ShipmentDataObject.DataContext.DataProviderForCodeMapping;

			// map to sender
			var sender = Factory.New<OrgHeader>();
			sender.OH_Code = "SENDR";
			var orgMatch = Factory.New<OrgPatternMatchOverride>();
			orgMatch.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;
			orgMatch.OO_OH = Env.CurrentCompany.OrganisationPK;
			orgMatch.OO_LocalGuid = sender.PK;
			orgMatch.OO_ForeignCode = licenceCode;
			Factory.SaveForTesting();

			var updatedConsignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			AssertEquals("Should have matched existing consignment.", consignment, updatedConsignment);

			var addresses2 = Factory.Load<JobDocAddress>(query);
			AssertEquals("Should have imported a Booking Party.", 1, addresses2.Length);

			var bookingParty2 = addresses2.Single();
			AssertEquals("Should have updated existing Job Doc Address.", bookingParty1, bookingParty2);
			AssertEquals("Booking Party should be the mapped organisation.", sender.MainAddress.PK, bookingParty2.E2_OA_Address);

			Factory.SaveForTesting();
			AssertEquals("Address should have saved correctly.",
				sender.MainAddress.PK, new BusinessObjectFactory().Load<JobDocAddress>(query).Single().E2_OA_Address);
		}

		public void TestPopulateBusinessObject_BookingParty_SeaCargoOutturn()
		{
			SetupForBaseTests();

			Data.SetupNewDataContextWithDataSource(Data.ShipmentDataObject, shipmentNumber: null);
			Data.ShipmentDataObject.DataContext.AddDataSource(DataContextType.Outturn, "");
			Data.SetupNewDataContextWithDataSource(Data.HeaderDataObject, consolNumber: "C1000000", shipmentNumber: null);
			Data.HeaderDataObject.DataContext.AddDataSource(DataContextType.SeaCargoOutturn, "S1000000");

			AdditionalSetupForImport(Data.ShipmentDataObject);
			Logger.OutboundSessionTracker = null;
			var reader = GetDataObjectReader(Factory, Data.HeaderDataObject);
			var consignment = reader.ReadIntoBusinessObject();
			consignment.Row().SetValue(ConsignmentIDColumn, "UnmatchedID", Logger);

			Factory.SaveForTesting();

			var jobDocAddresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, consignment.PK));
			var bookingParty = jobDocAddresses.Where(a => a.E2_AddressType == nameof(DocAddressType.BookingPartyDocumentaryAddress)).FirstOrDefault();
			AssertNull("Receive/Dispatch consignment created from Sea Cargo Outturn should not have booking party.", bookingParty);
		}

		public void TestPopulateBusinessObject_BookingParty_ArrivalCFS() => TestPopulateBusinessObject_BookingParty(true);

		public void TestPopulateBusinessObject_BookingParty_DepartureCFS() => TestPopulateBusinessObject_BookingParty(false);

		void TestPopulateBusinessObject_BookingParty(bool arrival)
		{
			SetupForBaseTests();

			var recipientRoleType = arrival ? RecipientRoleType.ATW : RecipientRoleType.DTW;
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = recipientRoleType } } });

			Data.ShipmentDataObject.OrganizationAddressCollection.Add(UniversalHelper.CreateOrganizationAddress("AUSAHI", DocAddressType.BookingPartyDocumentaryAddress));

			var sendingForwarderAddress = UniversalHelper.CreateOrganizationAddress("OCEAND", DocAddressType.SendingForwarderAddress);
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(sendingForwarderAddress);

			var receivingForwarderAddress = UniversalHelper.CreateOrganizationAddress("ISSEXP", DocAddressType.ReceivingForwarderAddress);
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(receivingForwarderAddress);

			AdditionalSetupForImport(Data.ShipmentDataObject);
			var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();

			var query = new ZQuery(JobDocAddressSchema.E2_ParentID, consignment.PK);
			query.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.BookingPartyDocumentaryAddress);
			var addresses1 = Factory.Load<JobDocAddress>(query);
			AssertEquals("Should have imported a Booking Party.", 1, addresses1.Length);
			var bookingParty = addresses1.Single();

			if (arrival)
			{
				AssertEquals("Booking Party should be the Receiving Agent.", receivingForwarderAddress.OrganizationCode.ToString(), bookingParty.Organisation.OH_Code);
				AssertContains("Should add a log when retrieving booking party.", "Information - Matching 'Booking Party':- Matched to 'ISSEXP' by code from 'ReceivingForwarderAddress' for Arrival Transit Warehouse.", Logger.Logs);
			}
			else
			{
				AssertEquals("Booking Party should be the Sending Agent.", sendingForwarderAddress.OrganizationCode.ToString(), bookingParty.Organisation.OH_Code);
				AssertContains("Should add a log when retrieving booking party.", "Information - Matching 'Booking Party':- Matched to 'OCEAND' by code from 'SendingForwarderAddress' for Departure Transit Warehouse.", Logger.Logs);
			}
		}

		public void TestPopulateBusinessObject_BookingParty_ArrivalOrgNotFound() => TestPopulateBusinessObject_BookingParty_OrgNotFound(true);

		public void TestPopulateBusinessObject_BookingParty_DepartureOrgNotFound() => TestPopulateBusinessObject_BookingParty_OrgNotFound(false);

		void TestPopulateBusinessObject_BookingParty_OrgNotFound(bool arrival)
		{
			SetupForBaseTests();

			var recipientRoleType = arrival ? RecipientRoleType.ATW : RecipientRoleType.DTW;
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = recipientRoleType } } });

			var sendingForwarderAddress = UniversalHelper.CreateOrganizationAddress("NOTEX1", DocAddressType.SendingForwarderAddress);
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(sendingForwarderAddress);

			var receivingForwarderAddress = UniversalHelper.CreateOrganizationAddress("NOTEX2", DocAddressType.ReceivingForwarderAddress);
			Data.ShipmentDataObject.OrganizationAddressCollection.Add(receivingForwarderAddress);

			AdditionalSetupForImport(Data.ShipmentDataObject);

			var tempCompany = Factory.New<GlbCompany>();
			tempCompany.GC_Code = "";
			Data.ShipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(tempCompany);

			GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();

			if (arrival)
			{
				AssertContains("Should add a log saying booking party is not found for organization NOTEX2.", "Error - Could not find 'Booking Party':- Organization Code 'NOTEX2' does not exist for this Arrival Transit Warehouse, and UXML does not originate from this system.", Logger.Logs);
			}
			else
			{
				AssertContains("Should add a log saying booking party is not found for organization NOTEX1.", "Error - Could not find 'Booking Party':- Organization Code 'NOTEX1' does not exist for this Departure Transit Warehouse, and UXML does not originate from this system.", Logger.Logs);
			}
		}

		public void TestPopulateBusinessObject_BookingParty_WrongRecipient_OrgNotFound()
		{
			SetupForBaseTests();
			var cfsAddress = UniversalHelper.CreateOrganizationAddress("INTHEMSYD", DocAddressType.LocalCartageCFS);

			Data.ShipmentDataObject.OrganizationAddressCollection.Add(cfsAddress);
			Data.ShipmentDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.DCT } } });

			AdditionalSetupForImport(Data.ShipmentDataObject);

			var tempCompany = Factory.New<GlbCompany>();
			tempCompany.GC_Code = "";
			Data.ShipmentDataObject.DataContext.SetCompanyAndDataProviderDetails(tempCompany);

			GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();

			AssertContains("Should add a matching party not found error log.", "Error - Could not find 'Booking Party':- Role type in the UXML is incorrect for Departure Transit Warehouse and Arrival Transit Warehouse, and UXML does not originate from this system.", Logger.Logs);
		}

		#endregion

		#region TestPopulateAddresses_Instructions

		public void TestPopulateAddresses_Instructions()
		{
			SetupForBaseTests();
			AdditionalSetupForImport(Data.ShipmentDataObject);

			// change shipment data to match with xml coming from runsheets
			Data.ShipmentDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress> { Data.Orgs.Warehouse_WUFSHIJNB, Data.Orgs.Warehouse_INTHEMSYD });
			var exporter = Data.Orgs.Consignor_CRAHOLSYD;
			exporter.AddressType = nameof(DocAddressType.LocalCartageExporter);

			var importer = Data.Orgs.Consignee_INTHEMSYD;
			importer.AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);

			var cfs = Data.Orgs.Warehouse_WUFSHIJNB;
			cfs.AddressType = nameof(DocAddressType.LocalCartageCFS);

			var differentCFS = Data.Orgs.Warehouse_INTHEMSYD;
			differentCFS.AddressType = nameof(DocAddressType.LocalCartageCFS);

			var instructions = new DataObjectList<Instruction>();
			instructions.Add(new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { Address = exporter });
			instructions.Add(new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { Address = importer });
			instructions.Add(new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { Address = cfs });
			instructions.Add(new Instruction(DefaultDataObjectWriterStrategy.TestInstance) { Address = differentCFS });

			Data.ShipmentDataObject.SetInstructionCollection(() => instructions);

			var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject();
			var addresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, consignment.PK));
			AssertEquals("Should have imported four Addresses.", 3, addresses.Length);

			var cneJDA = addresses.Single(a => a.E2_OA_Address == Data.Orgs.INTHEMSYD.MainAddress.PK);
			var cnrJDA = addresses.Single(a => a.E2_OA_Address == Data.Orgs.CRAHOLSYD.MainAddress.PK);
			var bkpJDA = addresses.Single(a => a.E2_OA_Address == GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK);
			AssertEquals("Should have imported Consignee Address.", DocAddressTypes.Codes.ConsigneeDocumentaryAddress, cneJDA.E2_AddressType);
			AssertEquals("Should have imported Consignor Address.", DocAddressTypes.Codes.LocalCartageExporter, cnrJDA.E2_AddressType);
			AssertEquals("Should have imported Booking party Address.", DocAddressTypes.Codes.BookingPartyDocumentaryAddress, bkpJDA.E2_AddressType);

			AssertAddressContentMatches_CRAHOLSYD(cnrJDA.Address);
			AssertAddressContentMatches_INTHEMSYD(cneJDA.Address);
		}

		#endregion

		#region TestPopulateAdditionalReferences

		protected abstract int AdditionalReferencesRows { get; }
		protected abstract int InitialReferencesRowCount { get; }

		public void TestAdditionalReferencesBase()
		{
			SetupForBaseTests();
			AdditionalSetupForImport(Data.ShipmentDataObject);

			var header = Data.HeaderDataObject;
			Data.Warehouse.RelatedCompanyBranch.GB_RL_NKHomePort = "ZAJNB";

			var reader = GetDataObjectReader(Factory, header);
			var consignment = reader.ReadIntoBusinessObject();
			DeleteJobLinks(Factory);
			var cusEntryNumbers1 = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, consignment.PK));
			AssertEquals($"We should populate {InitialReferencesRowCount} rows in additional references.", InitialReferencesRowCount, cusEntryNumbers1.Length);

			Helper.AssertAdditionalReferences(cusEntryNumbers1, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber, "S1000000", "ForwardingShipmentNumber");
			Helper.AssertAdditionalReferences(cusEntryNumbers1, WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentDescription, "shipment desc", "ForwardingShipmentDescription");

			var referenceHelper = new WhsTransitAdditionalReferencesHelper(Logger, Factory);
			var additionalReferences = new List<TransitAdditionalReferenceInfo>();
			referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.MasterBill, "MB0000001");
			new TransitWarehouseCusEntryNumReferenceCollectionReader(additionalReferences.ToArray(), consignment, Logger, Factory).ReadIntoCollectionRetainingUnmatchedElements();

			reader = GetDataObjectReader(Factory, header);
			var updatedConsignment = reader.ReadIntoBusinessObject();
			AssertEquals("Should have updated the same consignment.", consignment, updatedConsignment);

			var cusEntryNumbers2 = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, consignment.PK));
			AssertEquals(string.Format("We should not delete original references and populate new additional references."),
				AdditionalReferencesRows, cusEntryNumbers2.Length);
		}

		#endregion

		#region TestPopulateDestination

		public void TestPopulateDestination()
		{
			SetupForBaseTests();
			AdditionalSetupForImport(Data.ShipmentDataObject);

			var consignment = GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject().Row();
			AssertEquals("The destination should be taken from data object.", "AUSYD", consignment.GetValue(DestinationColumn));
		}

		#endregion

		#region TestDataObject_NoSubshipments

		public void TestDataObject_NoSubshipments_AssemblyMaster()
		{
			AssertDataObject_NoSubshipments(Constants.ShipmentTypes.AssemblyMaster);
		}

		public void TestDataObject_NoSubshipments_CoLoadMaster()
		{
			AssertDataObject_NoSubshipments(Constants.ShipmentTypes.CoLoadMaster);
		}

		public void TestDataObject_NoSubshipments_BlindCoLoadMaster()
		{
			AssertDataObject_NoSubshipments(Constants.ShipmentTypes.BlindCoLoadMaster);
		}

		public void TestDataObject_NoSubshipments_BuyersConsolLeadMaster()
		{
			Data.SetupForForwardingImport();
			AdditionalSetupForImport(Data.ShipmentDataObject);
			Data.ShipmentDataObject.ShipmentType = new CodeDescriptionPair() { Code = Constants.ShipmentTypes.BuyersConsolLead };

			AssertNoExceptionThrown("Should not have given errors since buyer consol lead return itself.",
				() => GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject());
		}

		void AssertDataObject_NoSubshipments(string shipmentType)
		{
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.ShipmentType = new CodeDescriptionPair() { Code = shipmentType };

			AssertExceptionThrown<DataObjectReadFailureException>("Should have given errors since there are not subshipments to import.",
				"Sub Shipments are mandatory for Master Shipments but none were specified for Master Shipment 'S1000000'.", () => GetDataObjectReader(Factory, Data.ShipmentDataObject).ReadIntoBusinessObject());
		}

		#endregion

		#region TestUniversalLinks

		public void TestUniversalLinks()
		{
			SetupForBaseTests();
			AdditionalSetupForImport(Data.ShipmentDataObject);
			var header = Data.HeaderDataObject;
			var reader = GetDataObjectReader(Factory, header);
			var consignment = reader.ReadIntoBusinessObject();
			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, GlbBranch.CurrentBranch.OrgProxy, consignment.PK, consignment.TablePrefix, DataContextType.ForwardingShipment, "S1000000", "EDI", "DAT", "EDI");
			Factory.SaveForTesting();

			var newUniversalFactory = new UniversalObjectFactory();
			var readerAgain = GetDataObjectReader(newUniversalFactory, header);
			var updatedConsignment = readerAgain.ReadIntoBusinessObject();
			newUniversalFactory.SaveForTesting();
			var linksInNewFactory = new BusinessObjectFactory().Load<StmUniversalJobLink>(new ZQuery());
			AssertEquals("Precondition: Should have updated the same consignment.", consignment.PK, updatedConsignment.PK);
			AssertUniversalJobLink(linksInNewFactory, GlbBranch.CurrentBranch.OrgProxy, updatedConsignment.PK, updatedConsignment.TablePrefix, DataContextType.ForwardingShipment, "S1000000", "EDI", "DAT", "EDI");
		}

		public void TestUniversalLinks_LinkExistsWithDifferentDataContextType()
		{
			SetupForBaseTests();

			AdditionalSetupForImport(Data.ShipmentDataObject);
			var header = Data.HeaderDataObject;
			var reader = GetDataObjectReader(Factory, header);
			var consignment = reader.ReadIntoBusinessObject();
			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			links.Single(l => l.UCL_ParentID == consignment.PK).UCL_SourceType = nameof(DataContextType.AccEInvoicingBatch);
			Factory.SaveForTesting();

			var newUniversalFactory = new UniversalObjectFactory();
			var readerAgain = GetDataObjectReader(newUniversalFactory, header);
			var updatedConsignment = readerAgain.ReadIntoBusinessObject();
			newUniversalFactory.SaveForTesting();
			AssertEquals("Precondition: Should have updated the same consignment.", consignment.PK, updatedConsignment.PK);
			var linksInNewFactory = new BusinessObjectFactory().Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(linksInNewFactory, GlbBranch.CurrentBranch.OrgProxy, updatedConsignment.PK, updatedConsignment.TablePrefix, DataContextType.AccEInvoicingBatch, "S1000000", "EDI", "DAT", "EDI");
			AssertUniversalJobLink(linksInNewFactory, GlbBranch.CurrentBranch.OrgProxy, updatedConsignment.PK, updatedConsignment.TablePrefix, DataContextType.ForwardingShipment, "S1000000", "EDI", "DAT", "EDI");
		}

		public void TestUniversalLinks_UpdatedSourceKey()
		{
			SetupForBaseTests();

			AdditionalSetupForImport(Data.ShipmentDataObject);
			var header = Data.HeaderDataObject;
			var reader = GetDataObjectReader(Factory, header);
			var consignment = reader.ReadIntoBusinessObject();
			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			links.Single(l => l.UCL_ParentID == consignment.PK).UCL_SourceKey = "Key Updated";
			Factory.SaveForTesting();

			var newUniversalFactory = new UniversalObjectFactory();
			var readerAgain = GetDataObjectReader(newUniversalFactory, header);
			var updatedConsignment = readerAgain.ReadIntoBusinessObject();
			newUniversalFactory.SaveForTesting();
			var linksInNewFactory = new BusinessObjectFactory().Load<StmUniversalJobLink>(new ZQuery());
			AssertEquals("Precondition: Should have updated the same consignment.", consignment.PK, updatedConsignment.PK);
			AssertUniversalJobLink(linksInNewFactory, GlbBranch.CurrentBranch.OrgProxy, updatedConsignment.PK, updatedConsignment.TablePrefix, DataContextType.ForwardingShipment, "S1000000", "EDI", "DAT", "EDI");
		}

		public void TestMatchOnJobLink_InternalImport_MatchesExistingConsignment() => TestMatchOnJobLink_MatchesExistingConsignment(isInternal: true);

		public void TestMatchOnJobLink_ExternalImport_MatchesExistingConsignment() => TestMatchOnJobLink_MatchesExistingConsignment(isInternal: false);

		void TestMatchOnJobLink_MatchesExistingConsignment(bool isInternal)
		{
			var consignment = SetupImportedShipmentAndChangeConsignmentID(Factory, isInternal);

			Factory.SaveForTesting();

			AdditionalSetupForImport(Data.ShipmentDataObject);
			var consignmentFromReimport = ReimportShipmentInNewFactory();

			AssertEquals("Should match the same consignment.", consignment.PK, consignmentFromReimport.PK);
		}

		public void TestMatchOnJobLink_NewSourceKey_DoesNotMatch()
		{
			var consignment = SetupImportedShipmentAndChangeConsignmentID(Factory);
			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			links.Single(l => l.UCL_ParentID == consignment.PK).UCL_SourceKey = "Key Updated";

			Factory.SaveForTesting();

			AdditionalSetupForImport(Data.ShipmentDataObject);
			var consignmentFromReimport = ReimportShipmentInNewFactory();

			AssertNotEquals("Should not match the consignment by job link if the source key is changed.", consignment.PK, consignmentFromReimport.PK);
		}

		public void TestMatchOnJobLink_FallbackToOldMatching()
		{
			SetupForBaseTests();

			Data.SetupNewDataContextWithDataSource(Data.ShipmentDataObject);

			AdditionalSetupForImport(Data.ShipmentDataObject);

			var reader = GetDataObjectReader(Factory, Data.HeaderDataObject);
			var consignment = reader.ReadIntoBusinessObject();

			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertUniversalJobLink(links, GlbBranch.CurrentBranch.OrgProxy, consignment.PK, consignment.TablePrefix, DataContextType.ForwardingShipment, "S1000000", "EDI", "DAT", "EDI");
			links.Single(l => l.UCL_ParentID == consignment.PK).UCL_SourceKey = "Key Updated";

			Factory.SaveForTesting();

			AdditionalSetupForImport(Data.ShipmentDataObject);
			var consignmentFromReimport = ReimportShipmentInNewFactory();

			AssertEquals("Should match the consignment by fallback rules if job link matching fails.", consignment.PK, consignmentFromReimport.PK);
		}

		public void TestMatchOnJobLink_MultipleLinks_MatchesMostRecentConsignment()
		{
			var consignment1 = SetupImportedShipmentAndChangeConsignmentID(Factory);
			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			// Change the Consignment job link number to prevent matching
			links.Single(l => l.UCL_ParentID == consignment1.PK).UCL_SourceKey = "S2";
			Factory.SaveForTesting();

			// Import a second Consignment
			var newUniversalFactory = new UniversalObjectFactory();
			Data.ShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { Content = CollectionContent.Complete });
			var reader = GetDataObjectReader(newUniversalFactory, Data.HeaderDataObject);
			var consignment2 = reader.ReadIntoBusinessObject();
			AssertNotEquals("Precondition: Consignments do not match", consignment1.PK, consignment2.PK);

			// Make both job links match the shipment number
			links = newUniversalFactory.Load<StmUniversalJobLink>(new ZQuery());
			links.Single(l => l.UCL_ParentID == consignment1.PK).UCL_SourceKey = "S1000000";
			links.Single(l => l.UCL_ParentID == consignment2.PK).UCL_SourceKey = "S1000000";

			// Make consignment1 the most recent
			var now = ZDateTime.Now;
			var consignments = newUniversalFactory.Load<TBizO>(new ZQuery());
			consignments.Single(c => c.PK == consignment1.PK).SetValue(CreateTimeColumn, now.AddDays(1));
			consignments.Single(c => c.PK == consignment2.PK).SetValue(CreateTimeColumn, now);

			newUniversalFactory.SaveForTesting();

			// Reimport the Consignment
			var newUniversalFactory2 = new UniversalObjectFactory();
			Data.ShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { Content = CollectionContent.Complete });
			var reader2 = GetDataObjectReader(newUniversalFactory2, Data.HeaderDataObject);
			var consignmentFromReimport = reader2.ReadIntoBusinessObject();

			AssertEquals("Should match the job link with the most recent consignment.", consignment1.PK, consignmentFromReimport.PK);
		}

		public void TestMatchOnJobLink_MultipleLinks_MatchesOpenConsignment()
		{
			var latestClosedConsignment = SetupImportedShipmentAndChangeConsignmentID(Factory);
			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			// Change the Consignment job link number to prevent matching
			links.Single(l => l.UCL_ParentID == latestClosedConsignment.PK).UCL_SourceKey = "S2";
			Factory.SaveForTesting();

			// Import a second Consignment
			var newUniversalFactory = new UniversalObjectFactory();
			Data.ShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { Content = CollectionContent.Complete });
			var reader = GetDataObjectReader(newUniversalFactory, Data.HeaderDataObject);
			var oldOpenConsignment = reader.ReadIntoBusinessObject();
			AssertNotEquals("Precondition: Consignments do not match", latestClosedConsignment.PK, oldOpenConsignment.PK);

			// Make both job links match the shipment number
			links = newUniversalFactory.Load<StmUniversalJobLink>(new ZQuery());
			links.Single(l => l.UCL_ParentID == latestClosedConsignment.PK).UCL_SourceKey = "S1000000";
			links.Single(l => l.UCL_ParentID == oldOpenConsignment.PK).UCL_SourceKey = "S1000000";

			// Make consignment1 the most recent
			var now = ZDateTime.Now;
			var consignments = newUniversalFactory.Load<TBizO>(new ZQuery());
			var latestConsignment = consignments.Single(c => c.PK == latestClosedConsignment.PK);
			latestConsignment.SetValue(CreateTimeColumn, now.AddDays(1));
			latestConsignment.SetValue(CompleteTimeColumn, now.AddDays(1));
			consignments.Single(c => c.PK == oldOpenConsignment.PK).SetValue(CreateTimeColumn, now);

			newUniversalFactory.SaveForTesting();

			// Reimport the Consignment
			var newUniversalFactory2 = new UniversalObjectFactory();
			Data.ShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { Content = CollectionContent.Complete });
			var reader2 = GetDataObjectReader(newUniversalFactory2, Data.HeaderDataObject);
			var consignmentFromReimport = reader2.ReadIntoBusinessObject();
			AssertEquals("Should match the job link with the latest consignment although its closed.",
				latestClosedConsignment.PK, consignmentFromReimport.PK);
		}

		public void TestMatchOnJobLink_ForwardingShipment_MatchesExistingConsignment()
			=> TestMatchOnJobLink_DataContextType_MatchesExistingConsignment(DataContextType.ForwardingShipment, shouldMatch: true, shouldCreateJobLink: true);

		public void TestMatchOnJobLink_LandTransportConsignment_MatchesExistingConsignment()
			=> TestMatchOnJobLink_DataContextType_MatchesExistingConsignment(DataContextType.LandTransportConsignment, shouldMatch: true, shouldCreateJobLink: true);

		public void TestMatchOnJobLink_SeaCargoOutturn_DoesNotMatchExistingConsignment()
			=> TestMatchOnJobLink_DataContextType_MatchesExistingConsignment(DataContextType.SeaCargoOutturn, shouldMatch: false, shouldCreateJobLink: false);

		void TestMatchOnJobLink_DataContextType_MatchesExistingConsignment(DataContextType dataContextType, bool shouldMatch, bool shouldCreateJobLink)
		{
			var consignment = SetupImportedShipmentAndChangeConsignmentID(Factory, shouldCreateJobLink: shouldCreateJobLink, dataContextType: dataContextType);

			Factory.SaveForTesting();

			AdditionalSetupForImport(Data.ShipmentDataObject);
			var consignmentFromReimport = ReimportShipmentInNewFactory();

			if (shouldMatch)
			{
				AssertEquals(consignment.PK, consignmentFromReimport.PK);
			}
			else
			{
				AssertNotEquals(consignment.PK, consignmentFromReimport.PK);
			}
		}

		public void TestMatchOnJobLink_DifferentWarehouse_DoesNotMatchConsignment()
		{
			var consignment = SetupImportedShipmentAndChangeConsignmentID(Factory);

			var consignmentFromReimport = SetupImportedShipmentAndChangeConsignmentID(Factory, isReimport: true);

			Factory.SaveForTesting();

			AssertNotEquals("Should not match using job links if the warehouse does not match.", consignment.PK, consignmentFromReimport.PK);
		}

		public void TestMatchOnJobLink_NewSourceType_DoesNotMatch()
		{
			var consignment = SetupImportedShipmentAndChangeConsignmentID(Factory);
			var links = Factory.Load<StmUniversalJobLink>(new ZQuery());
			links.Single(l => l.UCL_ParentID == consignment.PK).UCL_SourceType = nameof(DataContextType.LandTransportConsignment);

			Factory.SaveForTesting();

			AdditionalSetupForImport(Data.ShipmentDataObject);
			var consignmentFromReimport = ReimportShipmentInNewFactory();

			AssertNotEquals("Should not match using job links for other source types.", consignment.PK, consignmentFromReimport.PK);
		}

		TBizO SetupImportedShipmentAndChangeConsignmentID(UniversalObjectFactory factory, bool isInternal = false, bool shouldCreateJobLink = true, DataContextType dataContextType = DataContextType.ForwardingShipment, bool isReimport = false)
		{
			if (!isReimport)
			{
				SetupForBaseTests();

				Data.SetupNewDataContextWithDataSource(Data.ShipmentDataObject, shipmentNumber: null);
				Data.ShipmentDataObject.DataContext.AddDataSource(dataContextType, "S1000000");
				Data.SetupNewDataContextWithDataSource(Data.HeaderDataObject, consolNumber: "C1000000", shipmentNumber: null);
				Data.HeaderDataObject.DataContext.AddDataSource(dataContextType, "S1000000");

				AdditionalSetupForImport(Data.ShipmentDataObject);
			}
			else
			{
				Data.ShipmentDataObjectwithNewWarehouse.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { Content = CollectionContent.Complete });
				Data.SetupNewDataContextWithDataSource(Data.ShipmentDataObjectwithNewWarehouse, shipmentNumber: null);
				Data.ShipmentDataObjectwithNewWarehouse.DataContext.AddDataSource(dataContextType, "S1000000");
				Data.SetupNewDataContextWithDataSource(Data.HeaderDataObjectwithNewWarehouse, consolNumber: "C1000000", shipmentNumber: null);
				Data.HeaderDataObjectwithNewWarehouse.DataContext.AddDataSource(dataContextType, "S1000000");

				AdditionalSetupForImport(Data.ShipmentDataObjectwithNewWarehouse);
			}

			if (isInternal)
			{
				Logger.OutboundSessionTracker = new DataWritingManager(new DummyActionInfo());
			}
			else
			{
				Logger.OutboundSessionTracker = null;
			}

			var reader = GetDataObjectReader(factory, isReimport ? Data.HeaderDataObjectwithNewWarehouse : Data.HeaderDataObject);
			// Import the shipment but change its ID prevent fallback matching
			var consignment = reader.ReadIntoBusinessObject();

			if (shouldCreateJobLink)
			{
				var links = factory.Load<StmUniversalJobLink>(new ZQuery());
				AssertUniversalJobLink(links, isInternal ? null : GlbBranch.CurrentBranch.OrgProxy, consignment.PK, consignment.TablePrefix, dataContextType, "S1000000", "EDI", "DAT", "EDI");
			}

			consignment.Row().SetValue(ConsignmentIDColumn, "UnmatchedID", Logger);
			consignment.Row().SetValue(HouseBillNumberColumn, "UnmatchedID", Logger);
			return consignment;
		}

		TBizO ReimportShipmentInNewFactory()
		{
			var newUniversalFactory = new UniversalObjectFactory();
			var readerInNewFactory = GetDataObjectReader(newUniversalFactory, Data.HeaderDataObject);

			return readerInNewFactory.ReadIntoBusinessObject();
		}

		#endregion

		#region TestStmALog

		protected void AssertLogParentLogs(string eventCode, IStmALogParent parent, ZDateTime eventTime, params string[] expectedReferences)
		{
			var logs = parent.Logs.Find(l => !l.SL_IsCancelled && l.SL_SE_NKEvent == eventCode);
			AssertEquals("Should create log", expectedReferences.Length, logs.Count());
			AssertContainsExactElementsInAnyOrder(expectedReferences, logs.Select(t => t.SL_Reference));
			foreach (var log in logs)
			{
				AssertEquals(eventTime, log.SL_EventTime);
			}
		}

		protected void AssertLog(StmALog log, string eventCode, ZDateTime eventTime, string reference)
		{
			AssertEquals(eventCode, log.SL_SE_NKEvent);
			AssertEquals(eventTime, log.SL_EventTime);
			AssertEquals(reference, log.SL_Reference);
		}

		#endregion

		#region Implementation

		protected abstract TReader GetDataObjectReader(UniversalObjectFactory factory, UniversalShipment shipmentDataObject);

		void SetupForBaseTests()
		{
			Data.SetupForForwardingImport();
			Data.ShipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { Content = CollectionContent.Complete });
		}

		protected static void DeleteJobLinks(UniversalObjectFactory factory) => factory.BOFactory.Load<StmUniversalJobLink>(new ZQuery()).DeleteAll();

		protected virtual void AdditionalSetupForImport(UniversalShipment shipmentDataObject)
		{
		}

		TestHelperForUniversal UniversalHelper => new TestHelperForUniversal(Factory.BOFactory);

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;

		#endregion
	}
}
