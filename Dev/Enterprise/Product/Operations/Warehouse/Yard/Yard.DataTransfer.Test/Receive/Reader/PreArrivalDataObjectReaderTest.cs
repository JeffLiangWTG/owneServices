using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal.Test
{
	[TestedType(typeof(PreArrivalDataObjectReader))]
	public class PreArrivalDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		UniversalTestData Data;

		protected override void SetUp()
		{
			base.SetUp();
			Data = new UniversalTestData(Factory, new TestErrorLogger());
			Data.SetupDataForTesting();
		}

		public void TestReadFromDataObject()
		{
			#region Setup UXML
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			var fromDate = ZDateTime.UtcNow.Date.AddDays(-2);
			var toDate = ZDateTime.UtcNow.Date.AddDays(2);

			var refContainerType = SetupRefContainerData();
			var clientAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.BookingPartyDocumentaryAddress);
			var address = new OrganisationDataObjectReader(clientAddress, logger, Factory).GetMatchedOrNewForTesting();
			var shipment = CreateNewShipmentWithTestData(fromDate, toDate);
			shipment.OrganizationAddressCollection.Add(clientAddress);
			#endregion Setup UXML

			#region Read the UXML

			var receiveAdvice = new PreArrivalDataObjectReader(shipment, new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			#endregion Read the UXML

			var newFactory = new BusinessObjectFactory();
			var receiveAdvices = newFactory.Load<CYDReceiveAdvice>(new ZQuery());
			var yardUnitStates = newFactory.Load<CYDYardUnitState>(new ZQuery());
			var unitLineItem = newFactory.LoadTop1<CYDUnitLineItem>(new ZQuery(CYDUnitLineItemSchema.YLI_SealNumber, "seal number"));

			AssertEquals("1 ReceiveAdviceLine should be populated.", 1, receiveAdvices.Length);
			AssertEquals("1 YardUnitState should be populated.", 1, yardUnitStates.Length);
			Assert("1 YardUnitState should be populated.", yardUnitStates.Any(u => u.YUS_UnitID.Equals("CON1")));
			AssertNotNull("1 unitLineItem should be populated", unitLineItem);
			AssertEquals(3, (int)unitLineItem.YLI_Quantity);
			AssertEquals(true, unitLineItem.YLI_IsEmpty);
			AssertEquals("seal number", unitLineItem.YLI_SealNumber);
			AssertEquals(refContainerType.PK, unitLineItem.YLI_RC_ContainerType);
			AssertEquals("ACC1", receiveAdvices.Single().YRA_AcceptanceNumber);
			AssertEquals(fromDate, receiveAdvices.Single().YRA_FromDate);
			AssertEquals(toDate, receiveAdvices.Single().YRA_ToDate);
		}

		public void TestContainerNumberValidation(string containerNumber)
		{
			#region Setup UXML

			var fromDate = ZDateTime.UtcNow.Date.AddDays(-2);
			var toDate = ZDateTime.UtcNow.Date.AddDays(2);

			var refContainerType = SetupRefContainerData();
			var shipment = CreateNewShipmentWithTestData(fromDate, toDate, "SP!@123");

			#endregion Setup UXML

			AssertExceptionThrown<DataObjectReadFailureException>(
				@"No containers with valid container numbers found in the incoming UXML.",
				() => new PreArrivalDataObjectReader(shipment, new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code), Factory).ReadIntoBusinessObject());
		}

		public void TestFiltersContainersWithWrongNumber()
		{
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			#region Setup UXML

			var fromDate = ZDateTime.UtcNow.Date.AddDays(-2);
			var toDate = ZDateTime.UtcNow.Date.AddDays(2);

			var refContainerType = SetupRefContainerData();
			var shipment = CreateNewShipmentWithTestData(fromDate, toDate, "SP123");
			var clientAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.BookingPartyDocumentaryAddress);
			var address = new OrganisationDataObjectReader(clientAddress, logger, Factory).GetMatchedOrNewForTesting();

			shipment.OrganizationAddressCollection.Add(clientAddress);

			var container = UniversalTestHelper.CreateContainer("C111", "LD-3", true, 3, "seal number");
			var containerWrongNumber = UniversalTestHelper.CreateContainer("C!01", "LD-3", true, 3, "seal number");

			shipment.SubShipmentCollection[0].RelatedShipmentCollection[0].ContainerCollection.Add(container);
			shipment.SubShipmentCollection[0].RelatedShipmentCollection[0].ContainerCollection.Add(containerWrongNumber);

			#endregion Setup UXML

			#region Read the UXML

			var receiveAdvice = new PreArrivalDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			#endregion Read the UXML

			var newFactory = new BusinessObjectFactory();
			var receiveAdvices = newFactory.Load<CYDReceiveAdvice>(new ZQuery());
			var yardUnitStates = newFactory.Load<CYDYardUnitState>(new ZQuery());
			var unitLineItem = newFactory.Load<CYDUnitLineItem>(new ZQuery(CYDUnitLineItemSchema.YLI_SealNumber, "seal number"));

			// results also include the container from the setup function
			AssertEquals("1 ReceiveAdviceLine should be populated.", 1, receiveAdvices.Length);
			AssertEquals("2 YardUnitState should be populated.", 2, yardUnitStates.Length);
			Assert("YardUnitState should be populated.", yardUnitStates.Any(u => u.YUS_UnitID.Equals("SP123")));
			Assert("YardUnitState should be populated.", yardUnitStates.Any(u => u.YUS_UnitID.Equals("C111")));
			AssertNotNull("2 unitLineItems should be populated", unitLineItem.Length);
		}

		public void TestUpdateExistingDataObjectWithDataTarget()
		{
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			#region Setup UXML

			var clientAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.BookingPartyDocumentaryAddress);

			var address = new OrganisationDataObjectReader(clientAddress, logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			var fromDate = ZDateTime.UtcNow.Date.AddDays(-2);
			var toDate = ZDateTime.UtcNow.Date.AddDays(2);

			var refContainerType = SetupRefContainerData();
			var shipment = CreateNewShipmentWithTestData(fromDate, toDate);

			shipment.OrganizationAddressCollection.Add(clientAddress);

			#endregion Setup UXML

			#region Save the first UXML

			var receiveAdvice = new PreArrivalDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			#endregion Save the first UXML

			var newFactory = new BusinessObjectFactory();
			var receiveAdvices = newFactory.Load<CYDReceiveAdvice>(new ZQuery());
			var unitLineItem = newFactory.LoadTop1<CYDUnitLineItem>(new ZQuery(CYDUnitLineItemSchema.YLI_SealNumber, "seal number"));
			AssertEquals("1 ReceiveAdviceLine should be populated.", 1, receiveAdvices.Length);
			AssertEquals("ACC1", receiveAdvices.Single().YRA_AcceptanceNumber);
			AssertEquals(refContainerType.PK, unitLineItem.YLI_RC_ContainerType);
			AssertEquals(fromDate, receiveAdvices.Single().YRA_FromDate);
			AssertEquals(toDate, receiveAdvices.Single().YRA_ToDate);
			AssertEquals("ReceiveAdvice should have a client address", address.PK, receiveAdvices.Single().Client.E2_OA_Address);
			AssertEquals("ReceiveAdvice should have the empty lessee address", ZGuid.Empty, receiveAdvices.Single().Lessee.E2_OA_Address);

			#region Create second UXML to update the PreArrivalDataObject

			// Update from and to dates
			var updatedFromDate = ZDateTime.UtcNow.Date.AddDays(-1);
			var updatedToDate = ZDateTime.UtcNow.Date.AddDays(3);

			var updatedContainerNumber = "CON2";
			var updatedContainerType = "LD-2";
			var updatedRefContainerType = SetupRefContainerData(updatedContainerType);
			var updatedShipment = CreateNewShipmentWithTestData(updatedFromDate, updatedToDate, updatedContainerNumber, updatedContainerType);

			// Set the data target of the updated shipment to be the previously saved CYDReceiveAdvice
			updatedShipment.DataContext.AddDataTarget(DataContextType.CYDReceiveAdvice, receiveAdvices[0].YRA_JobNumber);

			// Update shipment client address
			var newClientAddress = OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.BookingPartyDocumentaryAddress);

			var newAddress = new OrganisationDataObjectReader(newClientAddress, logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			updatedShipment.OrganizationAddressCollection.Add(newClientAddress);

			// Update shipment lessee address
			var newLesseeOrganizationAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.ControllingCustomer);

			var newLesseeAddress = new OrganisationDataObjectReader(newLesseeOrganizationAddress, logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			updatedShipment.OrganizationAddressCollection.Add(newLesseeOrganizationAddress);

			var updatedReceiveAdvice = new PreArrivalDataObjectReader(updatedShipment, new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			#endregion Create second UXML to update the PreArrivalDataObject

			newFactory = new BusinessObjectFactory();
			var updatedReceiveAdvices = newFactory.Load<CYDReceiveAdvice>(new ZQuery());
			var yardUnitStates = newFactory.Load<CYDYardUnitState>(new ZQuery());
			var updatedUnitLineItem = newFactory.LoadTop1<CYDUnitLineItem>(new ZQuery(CYDUnitLineItemSchema.YLI_SealNumber, "seal number"));

			AssertEquals("1 ReceiveAdviceLine should be populated.", 1, updatedReceiveAdvices.Length);
			AssertEquals("1 YardUnitState should be populated.", 1, yardUnitStates.Length);
			Assert("1 YardUnitState should be populated.", yardUnitStates.Any(u => u.YUS_UnitID.Equals(updatedContainerNumber)));
			AssertNotNull("1 unitLineItem should be populated", updatedUnitLineItem);
			AssertEquals(3, (int)updatedUnitLineItem.YLI_Quantity);
			AssertEquals(true, updatedUnitLineItem.YLI_IsEmpty);
			AssertEquals("seal number", updatedUnitLineItem.YLI_SealNumber);
			AssertEquals(updatedRefContainerType.PK, updatedUnitLineItem.YLI_RC_ContainerType);
			AssertEquals("ACC1", updatedReceiveAdvices.Single().YRA_AcceptanceNumber);
			AssertEquals(updatedFromDate, updatedReceiveAdvices.Single().YRA_FromDate);
			AssertEquals(updatedToDate, updatedReceiveAdvices.Single().YRA_ToDate);
			AssertEquals("Updated ReceiveAdvice should keep the original client address", address.PK, updatedReceiveAdvices.Single().Client.E2_OA_Address);
			AssertEquals("Updated ReceiveAdvice should have the new lessee address", newLesseeAddress.PK, updatedReceiveAdvices.Single().Lessee.E2_OA_Address);
		}

		public void TestUpdateExistingDataObjectWithDataTargetMatchingClientAndRefNumberToOverlapingTime()
		{
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			#region Setup UXML

			var clientAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.BookingPartyDocumentaryAddress);

			var address = new OrganisationDataObjectReader(clientAddress, logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			var fromDate = ZDateTime.UtcNow.Date.AddDays(-2);
			var toDate = ZDateTime.UtcNow.Date.AddDays(2);

			var refContainerType = SetupRefContainerData();
			var shipment = CreateNewShipmentWithTestData(fromDate, toDate);

			shipment.OrganizationAddressCollection.Add(clientAddress);

			#endregion Setup UXML

			#region Save the first UXML

			var receiveAdvice = new PreArrivalDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			#endregion Save the first UXML

			var newFactory = new BusinessObjectFactory();
			var receiveAdvices = newFactory.Load<CYDReceiveAdvice>(new ZQuery());
			var unitLineItem = newFactory.LoadTop1<CYDUnitLineItem>(new ZQuery(CYDUnitLineItemSchema.YLI_SealNumber, "seal number"));
			AssertEquals("1 ReceiveAdviceLine should be populated.", 1, receiveAdvices.Length);
			AssertEquals("ACC1", receiveAdvices.Single().YRA_AcceptanceNumber);
			AssertEquals(refContainerType.PK, unitLineItem.YLI_RC_ContainerType);
			AssertEquals(fromDate, receiveAdvices.Single().YRA_FromDate);
			AssertEquals(toDate, receiveAdvices.Single().YRA_ToDate);
			AssertEquals("Updated ReceiveAdvice should have the new client address", address.PK, receiveAdvices.Single().Client.E2_OA_Address);
			AssertEquals("Updated ReceiveAdvice should have the empty lessee address", ZGuid.Empty, receiveAdvices.Single().Lessee.E2_OA_Address);

			#region Create second UXML to update the PreArrivalDataObject

			// Update from and to dates
			var updatedFromDate = ZDateTime.UtcNow.Date.AddDays(-1);
			var updatedToDate = ZDateTime.UtcNow.Date.AddDays(3);

			var updatedContainerNumber = "CON2";
			var updatedContainerType = "LD-2";
			var updatedRefContainerType = SetupRefContainerData(updatedContainerType);
			var updatedShipment = CreateNewShipmentWithTestData(updatedFromDate, updatedToDate, updatedContainerNumber, updatedContainerType);

			// Set the data target of the updated shipment to be the previously saved CYDReceiveAdvice
			updatedShipment.DataContext.AddDataTarget(DataContextType.CYDReceiveAdvice, receiveAdvices[0].YRA_JobNumber);

			updatedShipment.OrganizationAddressCollection.Add(clientAddress);

			// Update shipment lessee address
			var newLesseeOrganizationAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.ControllingCustomer);

			var newLesseeAddress = new OrganisationDataObjectReader(newLesseeOrganizationAddress, logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			updatedShipment.OrganizationAddressCollection.Add(newLesseeOrganizationAddress);

			var updatedReceiveAdvice = new PreArrivalDataObjectReader(updatedShipment, new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			#endregion Create second UXML to update the PreArrivalDataObject

			newFactory = new BusinessObjectFactory();
			var updatedReceiveAdvices = newFactory.Load<CYDReceiveAdvice>(new ZQuery());
			var yardUnitStates = newFactory.Load<CYDYardUnitState>(new ZQuery());
			var updatedUnitLineItem = newFactory.LoadTop1<CYDUnitLineItem>(new ZQuery(CYDUnitLineItemSchema.YLI_SealNumber, "seal number"));

			AssertEquals("1 ReceiveAdviceLine should be populated.", 1, updatedReceiveAdvices.Length);
			AssertEquals("1 YardUnitState should be populated.", 1, yardUnitStates.Length);
			Assert("1 YardUnitState should be populated.", yardUnitStates.Any(u => u.YUS_UnitID.Equals(updatedContainerNumber)));
			AssertNotNull("1 unitLineItem should be populated", updatedUnitLineItem);
			AssertEquals(3, (int)updatedUnitLineItem.YLI_Quantity);
			AssertEquals(true, updatedUnitLineItem.YLI_IsEmpty);
			AssertEquals("seal number", updatedUnitLineItem.YLI_SealNumber);
			AssertEquals(updatedRefContainerType.PK, updatedUnitLineItem.YLI_RC_ContainerType);
			AssertEquals("ACC1", updatedReceiveAdvices.Single().YRA_AcceptanceNumber);
			AssertEquals(updatedFromDate, updatedReceiveAdvices.Single().YRA_FromDate);
			AssertEquals(updatedToDate, updatedReceiveAdvices.Single().YRA_ToDate);
			AssertEquals("Updated ReceiveAdvice should have the new client address", address.PK, updatedReceiveAdvices.Single().Client.E2_OA_Address);
			AssertEquals("Updated ReceiveAdvice should have the new lessee address", newLesseeAddress.PK, updatedReceiveAdvices.Single().Lessee.E2_OA_Address);
		}

		public void TestUpdateAllDataOfExistingDataObjectWithDataTarget()
		{
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			#region Setup UXML

			var clientAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.BookingPartyDocumentaryAddress);

			var address = new OrganisationDataObjectReader(clientAddress, logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			var lesseeOrganizationAddress = OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.ControllingCustomer);

			var lesseeAddress = new OrganisationDataObjectReader(lesseeOrganizationAddress, logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			var fromDate = ZDateTime.UtcNow.Date.AddDays(-2);
			var toDate = ZDateTime.UtcNow.Date.AddDays(2);

			var refContainerType = SetupRefContainerData();
			var shipment = CreateNewShipmentWithTestData(fromDate, toDate);

			shipment.OrganizationAddressCollection.Add(clientAddress);
			shipment.OrganizationAddressCollection.Add(lesseeOrganizationAddress);

			#endregion Setup UXML

			#region Save the first UXML

			var receiveAdvice = new PreArrivalDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			#endregion Save the first UXML

			var newFactory = new BusinessObjectFactory();
			var receiveAdvices = newFactory.Load<CYDReceiveAdvice>(new ZQuery());
			var unitLineItem = newFactory.LoadTop1<CYDUnitLineItem>(new ZQuery(CYDUnitLineItemSchema.YLI_SealNumber, "seal number"));
			AssertEquals("1 ReceiveAdviceLine should be populated.", 1, receiveAdvices.Length);
			AssertEquals("ACC1", receiveAdvices.Single().YRA_AcceptanceNumber);
			AssertEquals(refContainerType.PK, unitLineItem.YLI_RC_ContainerType);
			AssertEquals(fromDate, receiveAdvices.Single().YRA_FromDate);
			AssertEquals(toDate, receiveAdvices.Single().YRA_ToDate);
			AssertEquals("ReceiveAdvice should have a client address", address.PK, receiveAdvices.Single().Client.E2_OA_Address);
			AssertEquals("ReceiveAdvice should have a lessee address", lesseeAddress.PK, receiveAdvices.Single().Lessee.E2_OA_Address);

			#region Create second UXML to update the PreArrivalDataObject

			// Update from and to dates
			var updatedFromDate = ZDateTime.UtcNow.Date.AddDays(-1);
			var updatedToDate = ZDateTime.UtcNow.Date.AddDays(3);

			var updatedContainerNumber = "CON2";
			var updatedContainerType = "LD-2";
			var updatedContainerIsEmpty = false;
			var updatedContainerCount = 5;
			var updatedContainerSealNumber = "2seal number";
			var updatedRefContainerType = SetupRefContainerData(updatedContainerType);
			var updatedShipment = CreateNewShipmentWithTestData(updatedFromDate, updatedToDate, updatedContainerNumber, updatedContainerType, updatedContainerIsEmpty, updatedContainerCount, updatedContainerSealNumber);

			// Set the data target of the updated shipment to be the previously saved CYDReceiveAdvice
			updatedShipment.DataContext.AddDataTarget(DataContextType.CYDReceiveAdvice, receiveAdvices[0].YRA_JobNumber);

			// Update shipment client address
			var newClientAddress = OrganizationAddressTestHelper.GetNewAddressData_CRAHOLSYD(DocAddressType.BookingPartyDocumentaryAddress);

			var newAddress = new OrganisationDataObjectReader(newClientAddress, logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			updatedShipment.OrganizationAddressCollection.Add(newClientAddress);

			// Update shipment lessee address
			var newLesseeOrganizationAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.ControllingCustomer);

			var newLesseeAddress = new OrganisationDataObjectReader(newLesseeOrganizationAddress, logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			updatedShipment.OrganizationAddressCollection.Add(newLesseeOrganizationAddress);

			var updatedReceiveAdvice = new PreArrivalDataObjectReader(updatedShipment, new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			#endregion Create second UXML to update the PreArrivalDataObject

			newFactory = new BusinessObjectFactory();
			var updatedReceiveAdvices = newFactory.Load<CYDReceiveAdvice>(new ZQuery());
			var yardUnitStates = newFactory.Load<CYDYardUnitState>(new ZQuery());
			var updatedUnitLineItem = newFactory.LoadTop1<CYDUnitLineItem>(new ZQuery(CYDUnitLineItemSchema.YLI_SealNumber, updatedContainerSealNumber));

			AssertEquals("1 ReceiveAdviceLine should be populated.", 1, updatedReceiveAdvices.Length);
			AssertEquals("1 YardUnitState should be populated.", 1, yardUnitStates.Length);
			Assert("1 YardUnitState should be populated.", yardUnitStates.Any(u => u.YUS_UnitID.Equals(updatedContainerNumber)));
			AssertNotNull("1 unitLineItem should be populated", updatedUnitLineItem);
			AssertEquals(updatedContainerCount, (int)updatedUnitLineItem.YLI_Quantity);
			AssertEquals(updatedContainerIsEmpty, updatedUnitLineItem.YLI_IsEmpty);
			AssertEquals(updatedContainerSealNumber, updatedUnitLineItem.YLI_SealNumber);
			AssertEquals(updatedRefContainerType.PK, updatedUnitLineItem.YLI_RC_ContainerType);
			AssertEquals("ACC1", updatedReceiveAdvices.Single().YRA_AcceptanceNumber);
			AssertEquals(updatedFromDate, updatedReceiveAdvices.Single().YRA_FromDate);
			AssertEquals(updatedToDate, updatedReceiveAdvices.Single().YRA_ToDate);
			AssertEquals("Updated ReceiveAdvice should keep the original client address", address.PK, updatedReceiveAdvices.Single().Client.E2_OA_Address);
			AssertEquals("Updated ReceiveAdvice should have the new lessee address", newLesseeAddress.PK, updatedReceiveAdvices.Single().Lessee.E2_OA_Address);
		}

		public void TestCreateSecondDataObjectWithRepeatedClientAndRefNumber()
		{
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			#region Setup UXML

			var clientAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.BookingPartyDocumentaryAddress);

			var address = new OrganisationDataObjectReader(clientAddress, logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			var fromDate = ZDateTime.UtcNow.Date.AddDays(-2);
			var toDate = ZDateTime.UtcNow.Date.AddDays(2);

			var refContainerType = SetupRefContainerData();
			var shipment = CreateNewShipmentWithTestData(fromDate, toDate);

			shipment.OrganizationAddressCollection.Add(clientAddress);

			#endregion Setup UXML

			#region Save the first UXML

			var receiveAdvice = new PreArrivalDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			#endregion Save the first UXML

			var newFactory = new BusinessObjectFactory();
			var receiveAdvices = newFactory.Load<CYDReceiveAdvice>(new ZQuery());
			var unitLineItem = newFactory.LoadTop1<CYDUnitLineItem>(new ZQuery(CYDUnitLineItemSchema.YLI_SealNumber, "seal number"));
			AssertEquals("1 ReceiveAdviceLine should be populated.", 1, receiveAdvices.Length);
			AssertEquals("ACC1", receiveAdvices.Single().YRA_AcceptanceNumber);
			AssertEquals(refContainerType.PK, unitLineItem.YLI_RC_ContainerType);
			AssertEquals(fromDate, receiveAdvices.Single().YRA_FromDate);
			AssertEquals(toDate, receiveAdvices.Single().YRA_ToDate);
			AssertEquals("Updated ReceiveAdvice should have the new client address", address.PK, receiveAdvices.Single().Client.E2_OA_Address);
			AssertEquals("Updated ReceiveAdvice should have the empty lessee address", ZGuid.Empty, receiveAdvices.Single().Lessee.E2_OA_Address);

			(int, int)[] newFromAndToOffsetPairs =
			{
				(-5, -3), // Both incoming from and to dates are earlier than the saved PRA from date
				(3, 5), // Both incoming from and to dates are later than the saved PRA to date
			};

			int expectedNumberOfEntities = receiveAdvices.Length;

			foreach (var newFromAndToOffsetPair in newFromAndToOffsetPairs)
			{
				var incomingFromDateOffset = newFromAndToOffsetPair.Item1;
				var incomingToDateOffset = newFromAndToOffsetPair.Item2;

				#region Create second UXML to create

				// Update from and to dates
				var updatedFromDate = ZDateTime.UtcNow.Date.AddDays(incomingFromDateOffset);
				var updatedToDate = ZDateTime.UtcNow.Date.AddDays(incomingToDateOffset);

				var updatedShipment = CreateNewShipmentWithTestData(updatedFromDate, updatedToDate);

				updatedShipment.OrganizationAddressCollection.Add(clientAddress);

				var secondReceiveAdvice = new PreArrivalDataObjectReader(updatedShipment,
					new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code), Factory).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				expectedNumberOfEntities++;

				#endregion Create second UXML to create

				newFactory = new BusinessObjectFactory();
				var updatedReceiveAdvices = newFactory.Load<CYDReceiveAdvice>(new ZQuery());
				var yardUnitStates = newFactory.Load<CYDYardUnitState>(new ZQuery());
				var updatedUnitLineItem =
					newFactory.Load<CYDUnitLineItem>(new ZQuery(CYDUnitLineItemSchema.YLI_SealNumber, "seal number"));

				AssertEquals("2 ReceiveAdviceLine should be populated.", expectedNumberOfEntities, updatedReceiveAdvices.Length);
				AssertEquals("2 YardUnitState should be populated.", expectedNumberOfEntities, yardUnitStates.Length);
				AssertEquals("2 unitLineItem should be populated", expectedNumberOfEntities, updatedUnitLineItem.Length);

				for (int i = 0; i < expectedNumberOfEntities; i++)
				{
					AssertEquals(3, (int)updatedUnitLineItem[i].YLI_Quantity);
					AssertEquals(true, updatedUnitLineItem[i].YLI_IsEmpty);
					AssertEquals("seal number", updatedUnitLineItem[i].YLI_SealNumber);
					AssertEquals(refContainerType.PK, updatedUnitLineItem[i].YLI_RC_ContainerType);
					AssertEquals("ACC1", updatedReceiveAdvices[i].YRA_AcceptanceNumber);
					if (updatedFromDate == updatedReceiveAdvices[i].YRA_FromDate)
					{
						AssertEquals(updatedToDate, updatedReceiveAdvices[i].YRA_ToDate);
					}

					if (fromDate == updatedReceiveAdvices[i].YRA_FromDate)
					{
						AssertEquals(toDate, updatedReceiveAdvices[i].YRA_ToDate);
					}

					AssertEquals("Updated ReceiveAdvice should have the new client address", address.PK,
						updatedReceiveAdvices[i].Client.E2_OA_Address);
					AssertEquals("Updated ReceiveAdvice should have the empty lessee address", ZGuid.Empty,
						updatedReceiveAdvices[i].Lessee.E2_OA_Address);
				}
			}
		}

		public void TestUpdateExistingDataObjectMatchedUsingClientAndRefNumber()
		{
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			#region Setup UXML

			var clientAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.BookingPartyDocumentaryAddress);

			var address = new OrganisationDataObjectReader(clientAddress, logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			var fromDate = ZDateTime.UtcNow.Date.AddDays(-2);
			var toDate = ZDateTime.UtcNow.Date.AddDays(2);

			var refContainerType = SetupRefContainerData();
			var shipment = CreateNewShipmentWithTestData(fromDate, toDate);

			shipment.OrganizationAddressCollection.Add(clientAddress);

			#endregion Setup UXML

			#region Save the first UXML

			var receiveAdvice = new PreArrivalDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			#endregion Save the first UXML

			var newFactory = new BusinessObjectFactory();
			var receiveAdvices = newFactory.Load<CYDReceiveAdvice>(new ZQuery());
			var unitLineItem = newFactory.LoadTop1<CYDUnitLineItem>(new ZQuery(CYDUnitLineItemSchema.YLI_SealNumber, "seal number"));
			AssertEquals("1 ReceiveAdviceLine should be populated.", 1, receiveAdvices.Length);
			AssertEquals("ACC1", receiveAdvices.Single().YRA_AcceptanceNumber);
			AssertEquals(refContainerType.PK, unitLineItem.YLI_RC_ContainerType);
			AssertEquals(fromDate, receiveAdvices.Single().YRA_FromDate);
			AssertEquals(toDate, receiveAdvices.Single().YRA_ToDate);
			AssertEquals("Updated ReceiveAdvice should have the new client address", address.PK, receiveAdvices.Single().Client.E2_OA_Address);
			AssertEquals("Updated ReceiveAdvice should have the empty lessee address", ZGuid.Empty, receiveAdvices.Single().Lessee.E2_OA_Address);

			(int, int)[] newFromAndToOffsetPairs =
			{
				(-2, 2), // New dates match exactly to existing dates
				(-1, 1), // New dates are withing existing dates
			};

			var updatedContainerNumber = "CON2";
			var updatedContainerType = "LD-2";
			var updatedRefContainerType = SetupRefContainerData(updatedContainerType);

			foreach (var newFromAndToOffsetPair in newFromAndToOffsetPairs)
			{
				var incomingFromDateOffset = newFromAndToOffsetPair.Item1;
				var incomingToDateOffset = newFromAndToOffsetPair.Item2;

				#region Create second UXML to create

				// Update from and to dates
				var updatedFromDate = ZDateTime.UtcNow.Date.AddDays(incomingFromDateOffset);
				var updatedToDate = ZDateTime.UtcNow.Date.AddDays(incomingToDateOffset);

				var updatedShipment = CreateNewShipmentWithTestData(updatedFromDate, updatedToDate,
					updatedContainerNumber, updatedContainerType);

				updatedShipment.OrganizationAddressCollection.Add(clientAddress);

				// Update shipment lessee address
				var newLesseeOrganizationAddress =
					OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.ControllingCustomer);

				var newLesseeAddress = new OrganisationDataObjectReader(newLesseeOrganizationAddress, logger, Factory)
					.GetMatchedOrNewForTesting();
				Factory.SaveForTesting();

				updatedShipment.OrganizationAddressCollection.Add(newLesseeOrganizationAddress);

				var updatedReceiveAdvice = new PreArrivalDataObjectReader(updatedShipment, new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code), Factory).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				#endregion Create second UXML to update the PreArrivalDataObject

				newFactory = new BusinessObjectFactory();
				var updatedReceiveAdvices = newFactory.Load<CYDReceiveAdvice>(new ZQuery());
				var yardUnitStates = newFactory.Load<CYDYardUnitState>(new ZQuery());
				var updatedUnitLineItem =
					newFactory.LoadTop1<CYDUnitLineItem>(
						new ZQuery(CYDUnitLineItemSchema.YLI_SealNumber, "seal number"));

				AssertEquals("1 ReceiveAdviceLine should be populated.", 1, updatedReceiveAdvices.Length);
				AssertEquals("1 YardUnitState should be populated.", 1, yardUnitStates.Length);
				Assert("1 YardUnitState should be populated.",
					yardUnitStates.Any(u => u.YUS_UnitID.Equals(updatedContainerNumber)));
				AssertNotNull("1 unitLineItem should be populated", updatedUnitLineItem);
				AssertEquals(3, (int)updatedUnitLineItem.YLI_Quantity);
				AssertEquals(true, updatedUnitLineItem.YLI_IsEmpty);
				AssertEquals("seal number", updatedUnitLineItem.YLI_SealNumber);
				AssertEquals(updatedRefContainerType.PK, updatedUnitLineItem.YLI_RC_ContainerType);
				AssertEquals("ACC1", updatedReceiveAdvices.Single().YRA_AcceptanceNumber);
				AssertEquals(updatedFromDate, updatedReceiveAdvices.Single().YRA_FromDate);
				AssertEquals(updatedToDate, updatedReceiveAdvices.Single().YRA_ToDate);
				AssertEquals("Updated ReceiveAdvice should have the new client address", address.PK,
					updatedReceiveAdvices.Single().Client.E2_OA_Address);
				AssertEquals("Updated ReceiveAdvice should have the new lessee address", newLesseeAddress.PK,
					updatedReceiveAdvices.Single().Lessee.E2_OA_Address);
			}
		}

		public void TestTryCreateOverlappingDataObjectsWithRepeatedClientAndRefNumber()
		{
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			#region Setup UXML

			var clientAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.BookingPartyDocumentaryAddress);

			var address = new OrganisationDataObjectReader(clientAddress, logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			var fromDate = ZDateTime.UtcNow.Date.AddDays(-2);
			var toDate = ZDateTime.UtcNow.Date.AddDays(2);

			var refContainerType = SetupRefContainerData();
			var shipment = CreateNewShipmentWithTestData(fromDate, toDate);

			shipment.OrganizationAddressCollection.Add(clientAddress);

			#endregion Setup UXML

			#region Save the first UXML

			var receiveAdvice = new PreArrivalDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			#endregion Save the first UXML

			var newFactory = new BusinessObjectFactory();
			var receiveAdvices = newFactory.Load<CYDReceiveAdvice>(new ZQuery());
			var unitLineItem = newFactory.LoadTop1<CYDUnitLineItem>(new ZQuery(CYDUnitLineItemSchema.YLI_SealNumber, "seal number"));
			AssertEquals("1 ReceiveAdviceLine should be populated.", 1, receiveAdvices.Length);
			AssertEquals("ACC1", receiveAdvices.Single().YRA_AcceptanceNumber);
			AssertEquals(refContainerType.PK, unitLineItem.YLI_RC_ContainerType);
			AssertEquals(fromDate, receiveAdvices.Single().YRA_FromDate);
			AssertEquals(toDate, receiveAdvices.Single().YRA_ToDate);
			AssertEquals("Updated ReceiveAdvice should have the new client address", address.PK, receiveAdvices.Single().Client.E2_OA_Address);
			AssertEquals("Updated ReceiveAdvice should have the empty lessee address", ZGuid.Empty, receiveAdvices.Single().Lessee.E2_OA_Address);

			#region Create second UXML to create

			// Update from and to dates
			var updatedFromDate = ZDateTime.UtcNow.Date.AddDays(3);
			var updatedToDate = ZDateTime.UtcNow.Date.AddDays(5);

			var updatedShipment = CreateNewShipmentWithTestData(updatedFromDate, updatedToDate);

			updatedShipment.OrganizationAddressCollection.Add(clientAddress);

			var secondReceiveAdvice = new PreArrivalDataObjectReader(updatedShipment, new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			#endregion Create second UXML to create

			newFactory = new BusinessObjectFactory();
			var updatedReceiveAdvices = newFactory.Load<CYDReceiveAdvice>(new ZQuery());
			var yardUnitStates = newFactory.Load<CYDYardUnitState>(new ZQuery());
			var updatedUnitLineItem = newFactory.Load<CYDUnitLineItem>(new ZQuery(CYDUnitLineItemSchema.YLI_SealNumber, "seal number"));

			AssertEquals("2 ReceiveAdviceLine should be populated.", 2, updatedReceiveAdvices.Length);
			AssertEquals("2 YardUnitState should be populated.", 2, yardUnitStates.Length);
			Assert("2 YardUnitState should be populated.", yardUnitStates.Any(u => u.YUS_UnitID.Equals("CON1")));
			AssertEquals("2 unitLineItem should be populated", 2, updatedUnitLineItem.Length);

			(int, int)[] overlappingFromAndToOffsetPairs =
			{
				(3, 5), // Incoming date pair matches exactly to the saved PRA's respective values.
				(3, 4), // Incoming date pair is within the saved PRA's date pair.
				(2, 3), // Incoming to date is same as the from date of saved PRA
				(5, 6), // Incoming from date is same as the to date of saved PRA
				(2, 4), // Incoming to date within existing PRA from and to dates, and incoming from date earlier than the from date of saved PRA
				(4, 6), // Incoming from date within existing PRA from and to dates, and incoming to date later than the to date of saved PRA
				(2, 6), // Incoming from date is earlier than and incoming to date is later than the saved PRA's respective values.
			};

			foreach (var overlappingFromAndToOffsetPair in overlappingFromAndToOffsetPairs)
			{
				var incomingFromDateOffset = overlappingFromAndToOffsetPair.Item1;
				var incomingToDateOffset = overlappingFromAndToOffsetPair.Item2;

				#region Create third UXML to update the first PreArrivalDataObject

				// Update from and to dates
				var thirdUpdatedFromDate = ZDateTime.UtcNow.Date.AddDays(incomingFromDateOffset);
				var thirdUpdatedToDate = ZDateTime.UtcNow.Date.AddDays(incomingToDateOffset);

				var thirdShipment = CreateNewShipmentWithTestData(thirdUpdatedFromDate, thirdUpdatedToDate);

				// Set the data target of the updated shipment to be the previously saved CYDReceiveAdvice
				thirdShipment.DataContext.AddDataTarget(DataContextType.CYDReceiveAdvice,
					receiveAdvices[0].YRA_JobNumber);

				// Update shipment client address
				thirdShipment.OrganizationAddressCollection.Add(clientAddress);

				AssertExceptionThrown<DataObjectReadFailureException>("Data Object Read Failure",
					"The from and to dates of incoming PRA is overlapping with existing saved PRA data.",
					() =>
					{
						var thirdReceiveAdvice = new PreArrivalDataObjectReader(thirdShipment,
								new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code), Factory)
							.ReadIntoBusinessObject();
					});

				#endregion Create third UXML to update the first PreArrivalDataObject
			}
		}

		public void TestTryWithTimeOverlapUpdateExistingDataObjectMatchedUsingClientAndRefNumber()
		{
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			#region Setup UXML

			var clientAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.BookingPartyDocumentaryAddress);

			var address = new OrganisationDataObjectReader(clientAddress, logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			var fromDate = ZDateTime.UtcNow.Date.AddDays(-2);
			var toDate = ZDateTime.UtcNow.Date.AddDays(2);

			var refContainerType = SetupRefContainerData();
			var shipment = CreateNewShipmentWithTestData(fromDate, toDate);

			shipment.OrganizationAddressCollection.Add(clientAddress);

			#endregion Setup UXML

			#region Save the first UXML

			var receiveAdvice = new PreArrivalDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			#endregion Save the first UXML

			var newFactory = new BusinessObjectFactory();
			var receiveAdvices = newFactory.Load<CYDReceiveAdvice>(new ZQuery());
			var unitLineItem = newFactory.LoadTop1<CYDUnitLineItem>(new ZQuery(CYDUnitLineItemSchema.YLI_SealNumber, "seal number"));
			AssertEquals("1 ReceiveAdviceLine should be populated.", 1, receiveAdvices.Length);
			AssertEquals("ACC1", receiveAdvices.Single().YRA_AcceptanceNumber);
			AssertEquals(refContainerType.PK, unitLineItem.YLI_RC_ContainerType);
			AssertEquals(fromDate, receiveAdvices.Single().YRA_FromDate);
			AssertEquals(toDate, receiveAdvices.Single().YRA_ToDate);
			AssertEquals("Updated ReceiveAdvice should have the new client address", address.PK, receiveAdvices.Single().Client.E2_OA_Address);
			AssertEquals("Updated ReceiveAdvice should have the empty lessee address", ZGuid.Empty, receiveAdvices.Single().Lessee.E2_OA_Address);

			(int, int)[] overlappingFromAndToOffsetPairs =
			{
				(-3, -2), // Incoming to date is same as the from date of saved PRA
				(2, 3), // Incoming from date is same as the to date of saved PRA
				(-1, 3), // Incoming from date within existing PRA from and to dates, and incoming to date later than the to date of saved PRA
				(-3, 1), // Incoming to date within existing PRA from and to dates, and incoming from date earlier than the from date of saved PRA
				(-3, 3), // Incoming from date is earlier than and incoming to date is later than the saved PRA's respective values.
			};

			foreach (var overlappingFromAndToOffsetPair in overlappingFromAndToOffsetPairs)
			{
				var incomingFromDateOffset = overlappingFromAndToOffsetPair.Item1;
				var incomingToDateOffset = overlappingFromAndToOffsetPair.Item2;
				#region Create second UXML to update the PreArrivalDataObject

				// Update from and to dates
				var updatedFromDate = ZDateTime.UtcNow.Date.AddDays(incomingFromDateOffset);
				var updatedToDate = ZDateTime.UtcNow.Date.AddDays(incomingToDateOffset);

				var updatedContainerNumber = "CON2";
				var updatedContainerType = "LD-2";
				var updatedRefContainerType = SetupRefContainerData(updatedContainerType);
				var updatedShipment = CreateNewShipmentWithTestData(updatedFromDate, updatedToDate, updatedContainerNumber, updatedContainerType);

				updatedShipment.OrganizationAddressCollection.Add(clientAddress);

				// Update shipment lessee address
				var newLesseeOrganizationAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.ControllingCustomer);

				var newLesseeAddress = new OrganisationDataObjectReader(newLesseeOrganizationAddress, logger, Factory).GetMatchedOrNewForTesting();
				Factory.SaveForTesting();

				updatedShipment.OrganizationAddressCollection.Add(newLesseeOrganizationAddress);

				#endregion Create second UXML to update the PreArrivalDataObject

				AssertExceptionThrown<DataObjectReadFailureException>("Data Object Read Failure",
					"The from and to dates of incoming PRA is overlapping with existing saved PRA data.",
					() =>
					{
						var updatedReceiveAdvice = new PreArrivalDataObjectReader(updatedShipment, new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code), Factory).ReadIntoBusinessObject();
					});
			}
		}

		public void TestTryUpdateNotExistingDataObject()
		{
			#region Setup UXML

			var nonExistentReference = "PAI00000023";

			var fromDate = ZDateTime.UtcNow.Date.AddDays(-2);
			var toDate = ZDateTime.UtcNow.Date.AddDays(2);

			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var clientAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.BookingPartyDocumentaryAddress);
			var address = new OrganisationDataObjectReader(clientAddress, logger, Factory).GetMatchedOrNewForTesting();
			var refContainer = SetupRefContainerData();
			var shipment = CreateNewShipmentWithTestData(fromDate, toDate);
			shipment.DataContext.AddDataTarget(DataContextType.CYDReceiveAdvice, nonExistentReference);
			shipment.OrganizationAddressCollection.Add(clientAddress);

			#endregion Setup UXML

			AssertExceptionThrown<DataObjectReadFailureException>("Data Object Read Failure", $"Match couldn't be found for CYDReceiveAdvice with Key {nonExistentReference}", () =>
			{
				var nonExistentAdvice = new PreArrivalDataObjectReader(shipment, new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code), Factory).ReadIntoBusinessObject();
			});
		}

		public void TestValidateUpdatedReceiveAdviceWithDuplicateUnitNumbersAcrossRelatedShipments()
		{
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			#region Setup UXML

			var clientAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.BookingPartyDocumentaryAddress);
			var address = new OrganisationDataObjectReader(clientAddress, logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			var fromDate = ZDateTime.UtcNow.Date.AddDays(-2);
			var toDate = ZDateTime.UtcNow.Date.AddDays(2);

			var refContainerType = SetupRefContainerData();
			var shipment = CreateShipmentWithDuplicateUnitNumbersTestData(fromDate, toDate);

			shipment.OrganizationAddressCollection.Add(clientAddress);

			var receiveAdvice = new PreArrivalDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			#endregion Setup UXML
			var unitItems = Factory.Load<CYDUnitLineItem>(new ZQuery());
			AssertEquals("6 containers should be populated", 6, unitItems.Length);
		}

		public void TestAssignJobNumberWhenAcceptNumberIsEmpty()
		{
			#region Setup UXML

			var fromDate = ZDateTime.UtcNow.Date.AddDays(-2);
			var toDate = ZDateTime.UtcNow.Date.AddDays(2);

			var refContainerType = SetupRefContainerData();
			var shipment = CreateNewShipmentWithTestData(fromDate, toDate, "CNIT");

			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var clientAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.BookingPartyDocumentaryAddress);
			var address = new OrganisationDataObjectReader(clientAddress, logger, Factory).GetMatchedOrNewForTesting();
			shipment.OrganizationAddressCollection.Add(clientAddress);
			Factory.SaveForTesting();

			#endregion Setup UXML

			#region Read the UXML with AcceptanceNumber as null

			shipment.BookingConfirmationReference = null;
			var receiveAdviceWithNullAcceptanceNumber = new PreArrivalDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			#endregion Read the UXML with AcceptanceNumber as null

			var newFactory = new BusinessObjectFactory();
			var receiveAdvicesWithNullAcceptanceNumber = newFactory.Load<CYDReceiveAdvice>(new ZQuery());

			AssertEquals("ReceiveAdvice should exist.", 1, receiveAdvicesWithNullAcceptanceNumber.Length);
			AssertEquals("JobNumber should be used as AcceptanceNumber when AcceptanceNumber is null.",
				receiveAdvicesWithNullAcceptanceNumber.Single().JobNumber,
				receiveAdvicesWithNullAcceptanceNumber.Single().YRA_AcceptanceNumber);

			#region Setup another UXML with AcceptanceNumber
			var shipment1 = CreateNewShipmentWithTestData(fromDate, toDate, "CNIT1");
			var logger1 = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var clientAddress1 = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.BookingPartyDocumentaryAddress);
			var address1 = new OrganisationDataObjectReader(clientAddress1, logger1, Factory).GetMatchedOrNewForTesting();
			shipment1.OrganizationAddressCollection.Add(clientAddress1);
			var acceptanceNumber = "ACC2";
			shipment1.BookingConfirmationReference = acceptanceNumber;
			var receiveAdviceWithAcceptanceNumber = new PreArrivalDataObjectReader(shipment1, logger1, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			#endregion Setup another UXML with AcceptanceNumber

			var receiveAdvicesWithAcceptanceNumber = newFactory.Load<CYDReceiveAdvice>(new ZQuery());

			var receiveAdvice = receiveAdvicesWithAcceptanceNumber.FirstOrDefault(r => r.YRA_AcceptanceNumber == acceptanceNumber);
			AssertEquals("ReceiveAdvice should exist.", 2, receiveAdvicesWithAcceptanceNumber.Length);
			AssertNotNull("AcceptanceNumber should be used when it is provided.", receiveAdvice);
		}

		public void TestImportSameUnitnumberForPRA_WhenDateNotOverlapping()
		{
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			#region Setup UXML

			var clientAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.BookingPartyDocumentaryAddress);
			var address = new OrganisationDataObjectReader(clientAddress, logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			var fromDate = ZDateTime.UtcNow.Date.AddDays(-2);
			var toDate = ZDateTime.UtcNow.Date.AddDays(2);

			var refContainerType = SetupRefContainerData();
			var shipment = CreateNewShipmentWithTestData(fromDate, toDate);

			shipment.OrganizationAddressCollection.Add(clientAddress);

			#endregion Setup UXML

			var receiveAdvice = new PreArrivalDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			#region Create second UXML to create

			var newFromDate = ZDateTime.UtcNow.Date.AddDays(3);
			var newToDate = ZDateTime.UtcNow.Date.AddDays(4);

			var newShipment = CreateNewShipmentWithTestData(newFromDate, newToDate, accNumber: "ACC2");

			newShipment.OrganizationAddressCollection.Add(clientAddress);

			Factory.SaveForTesting();

			var newReceiveAdvice = new PreArrivalDataObjectReader(newShipment, new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			#endregion Create second UXML to update the PreArrivalDataObject

			var newFactory = new BusinessObjectFactory();
			var receiveAdvices = newFactory.Load<CYDReceiveAdvice>(new ZQuery());
			var yardUnitStates = newFactory.Load<CYDYardUnitState>(new ZQuery());
			var newUnitLineItem = newFactory.LoadTop1<CYDUnitLineItem>(
				new ZQuery(CYDUnitLineItemSchema.YLI_RC_ContainerType, refContainerType.PK));

			AssertEquals("2 ReceiveAdvice should be populated.", 2, receiveAdvices.Length);
			AssertEquals("2 YardUnitState should be populated.", 2, yardUnitStates.Length);
			AssertEquals(refContainerType.PK, newUnitLineItem.YLI_RC_ContainerType);
		}

		public void TestImportSameUnitnumberForPRA_WhenDateIsOverlapping()
		{
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			#region Setup UXML

			var clientAddress = OrganizationAddressTestHelper.GetNewAddressData_INTHEMSYD(DocAddressType.BookingPartyDocumentaryAddress);
			var address = new OrganisationDataObjectReader(clientAddress, logger, Factory).GetMatchedOrNewForTesting();
			Factory.SaveForTesting();

			var fromDate = ZDateTime.UtcNow.Date.AddDays(-2);
			var toDate = ZDateTime.UtcNow.Date.AddDays(2);

			var refContainerType = SetupRefContainerData();
			var shipment = CreateNewShipmentWithTestData(fromDate, toDate);

			shipment.OrganizationAddressCollection.Add(clientAddress);

			#endregion Setup UXML

			var receiveAdvice = new PreArrivalDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var newFromDate = ZDateTime.UtcNow.Date.AddDays(-2);
			var newToDate = ZDateTime.UtcNow.Date.AddDays(4);

			var newShipment = CreateNewShipmentWithTestData(newFromDate, newToDate, accNumber: "ACC2");

			newShipment.OrganizationAddressCollection.Add(clientAddress);

			Factory.SaveForTesting();

			var reader = new PreArrivalDataObjectReader(newShipment, new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code), Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Same unit number already exists in Pre-Arrival Instruction ID", () => reader.ReadIntoBusinessObject());
		}

		public void TestGetClientOrgAddress_FallbackToDataProvider_WhenMissBookingPartyDocumentaryAddress()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var logger = new TestLogger(orgHeader.OH_Code);

			#region Setup UXML

			var fromDate = ZDateTime.UtcNow.Date.AddDays(-2);
			var toDate = ZDateTime.UtcNow.Date.AddDays(2);

			var shipment = CreateNewShipmentWithTestData(fromDate, toDate);
			shipment.DataContext.DataProviderForCodeMapping = orgHeader.OH_Code;

			#endregion Setup UXML

			var receiveAdvice = new PreArrivalDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertEquals(orgHeader.MainAddress.PK, receiveAdvice.Client.E2_OA_Address);
		}

		public void TestGetClientOrgAddress_FallbackToOrgProxy_WhenMissBookingPartyDocumentaryAddress()
		{
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			#region Setup UXML

			var fromDate = ZDateTime.UtcNow.Date.AddDays(-2);
			var toDate = ZDateTime.UtcNow.Date.AddDays(2);

			var shipment = CreateNewShipmentWithTestData(fromDate, toDate);
			shipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			#endregion Setup UXML

			var receiveAdvice = new PreArrivalDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			AssertEquals(GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK, receiveAdvice.Client.E2_OA_Address);
		}

		static Shipment CreateNewShipmentWithTestData(ZDate fromDate, ZDate toDate, string containerNumber = "CON1", string containerType = "LD-3", bool isEmpty = true, int containerCount = 3, string sealNumber = "seal number", string accNumber = "ACC1")
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress {
					AddressType = nameof(DocAddressType.LocalCartageYard),
					OrganizationCode = "WUFSHIJNB"
				},
			});

			var subShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var relatedShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			relatedShipment.SetContainerCollection(() => new DataObjectList<Container> { UniversalTestHelper.CreateContainer(containerNumber, containerType, isEmpty, containerCount, sealNumber) });
			subShipment.SetRelatedShipmentCollection(() => new List<Shipment> { relatedShipment });
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { subShipment });
			shipment.BookingConfirmationReference = accNumber;

			var dateCollection = new List<Date>
			{
				{ DateType.Start, ZBool.True, fromDate },
				{ DateType.End, ZBool.True, toDate }
			};
			shipment.SetDateCollection(() => dateCollection);

			return shipment;
		}

		static Shipment CreateShipmentWithDuplicateUnitNumbersTestData(ZDate fromDate, ZDate toDate, string containerNumber = "MARS6", string containerType = "LD-3", bool isEmpty = true, int containerCount = 3, string sealNumber = "seal number")
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress {
					AddressType = nameof(DocAddressType.LocalCartageYard),
					OrganizationCode = "WUFSHIJNB"
				},
			});

			var subShipment1 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var subShipment2 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var relatedShipment1 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var relatedShipment2 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var relatedShipment3 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var relatedShipment4 = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			relatedShipment1.SetContainerCollection(() => new DataObjectList<Container>
			{
				UniversalTestHelper.CreateContainer("MARS5", containerType, isEmpty, containerCount, sealNumber),
				UniversalTestHelper.CreateContainer("MARS6", containerType, isEmpty, containerCount, sealNumber)
			});
			relatedShipment2.SetContainerCollection(() => new DataObjectList<Container>
			{
				UniversalTestHelper.CreateContainer("MARS5", containerType, isEmpty, containerCount, sealNumber),
				UniversalTestHelper.CreateContainer("MARS3", containerType, isEmpty, containerCount, sealNumber)
			});

			relatedShipment3.SetContainerCollection(() => new DataObjectList<Container>
			{
				UniversalTestHelper.CreateContainer("MARS7", containerType, isEmpty, containerCount, sealNumber),
				UniversalTestHelper.CreateContainer("MARS6", containerType, isEmpty, containerCount, sealNumber)
			});

			relatedShipment4.SetContainerCollection(() => new DataObjectList<Container>
			{
				UniversalTestHelper.CreateContainer("CON2", containerType, isEmpty, containerCount, sealNumber),
				UniversalTestHelper.CreateContainer("CON1", containerType, isEmpty, containerCount, sealNumber)
			});

			subShipment1.SetRelatedShipmentCollection(() => new List<Shipment> { relatedShipment1 , relatedShipment2 });
			subShipment2.SetRelatedShipmentCollection(() => new List<Shipment> { relatedShipment3, relatedShipment4 });
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { subShipment1 , subShipment2 });
			shipment.BookingConfirmationReference = "ACC1";

			var dateCollection = new List<Date>
			{
				{ DateType.Start, ZBool.True, fromDate },
				{ DateType.End, ZBool.True, toDate }
			};
			shipment.SetDateCollection(() => dateCollection);

			return shipment;
		}

		RefContainer SetupRefContainerData(string containerType = "LD-3")
		{
			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_Code = containerType;
			Factory.SaveForTesting();

			return refContainer;
		}
	}
}
