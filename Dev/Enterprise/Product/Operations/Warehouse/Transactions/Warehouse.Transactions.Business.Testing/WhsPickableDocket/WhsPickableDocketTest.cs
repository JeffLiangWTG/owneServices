using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsPickableDocketTest<TPickableDocket> : WhsDocketTestCase<TPickableDocket>
		where TPickableDocket : WhsPickableDocket
	{
		#region TestSetDefaultValues

		protected override void TestSetDefaultValuesCore(TPickableDocket docket)
		{
			base.TestSetDefaultValuesCore(docket);

			AssertEquals(WhsPickOption.Codes.Auto, docket.WD_PickOption);
		}

		#endregion

		#region Clone

		protected override void AdditionalSetupForTestClone(TPickableDocket docket)
		{
			base.AdditionalSetupForTestClone(docket);
			docket.ConsigneePK = Factory.New<OrgHeader>().PK;

			var transportOrg = Factory.NewWithValidTestData<OrgHeader>();
			var whs = Helper.CreateWarehouse("Whs2", "W2", "A");
			var load = Helper.CreateWhsLoad(transportOrg, whs.DefaultOutboundDockDoorLocation);
			docket.WD_WLO_PlannedLoad = load.PK;
		}

		#endregion

		#region Delete

		protected override void AdditionalSetupForDelete_DeletesAllRelatedJobDocAddress(TPickableDocket docket, TestDataSimpleEnvironment data)
		{
			base.AdditionalSetupForDelete_DeletesAllRelatedJobDocAddress(docket, data);
			docket.ConsigneeDocAddress.OrganisationPK = data.Org1.PK;
		}

		#endregion

		#region Related Entities

		#region TestPick

		public void TestPick()
		{
			WhsPick pick = Factory.New<WhsPick>();
			Docket.WD_WP = pick.PK;
			AssertEquals(pick, Docket.Pick);
		}

		#endregion

		#region TestConsignee

		public void TestConsignee()
		{
			var docket = GetNewBusinessObject();
			AssertNull(docket.Consignee);

			var consignee = Helper.CreateClient("CONSIGNEE");
			docket.ConsigneeAddressPK = consignee.MainAddress.PK;
			AssertEquals(consignee, docket.Consignee);

			docket.ConsigneeDocAddress.E2_AddressOverride = true;
			docket.ConsigneeDocAddress.E2_Address1 = "Test";
			AssertNull(docket.Consignee);
		}

		public void TestConsignee_DbHits()
		{
			var docket = SetupForTestFinaliseDocket();
			new JobHeader.Loader(docket).TryLoadOrCreateWithoutMutexForTestOnly();
			docket.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(docket);
			AssertIsFinalisedPrecondition(docket.Pick);

			docket.Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var docketInOtherFactory = otherFactory.Load<TPickableDocket>(docket.PK);
			AssertEquals(docket.Consignee.PK, docketInOtherFactory.Consignee.PK);

			var expectedHits = new Dictionary<string, int>(ExpectedDbHitsForConsigneeCore)
			{
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
			};

			AssertDbHits(expectedHits, otherFactory);
		}

		protected abstract Dictionary<string, int> ExpectedDbHitsForConsigneeCore { get; }

		#endregion

		#region TestConsigneeAddress

		public void TestConsigneeAddress()
		{
			var docket = GetNewBusinessObject();
			AssertNull(docket.ConsigneeAddress);

			var consignee = Helper.CreateClient("CONSIGNEE");
			docket.ConsigneeAddressPK = consignee.MainAddress.PK;
			AssertEquals(consignee.MainAddress, docket.ConsigneeAddress);

			docket.ConsigneeDocAddress.E2_AddressOverride = true;
			docket.ConsigneeDocAddress.E2_Address1 = "Test";
			AssertNull(docket.ConsigneeAddress);
		}

		#endregion

		#region TestConsigneeDocAddress

		public void TestConsigneeDocAddress()
		{
			var docket = GetNewBusinessObject();
			AssertNotNull("Consignee DocAddress should be set.", docket.ConsigneeDocAddress);
			AssertNull("Consignee should be null", docket.Consignee);
			AssertEquals("ConsigneePK should be empty", ZGuid.Empty, docket.ConsigneePK);
			AssertNull("ConsigneeAddress should be null", docket.ConsigneeAddress);
			AssertEquals("ConsigneeAddressPK should be empty", ZGuid.Empty, docket.ConsigneeAddressPK);

			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.MainAddress;

			docket.ConsigneePK = orgHeader.PK;
			AssertEquals("Consignee DocAddress should indirectly point to new Org.", orgHeader.PK, docket.ConsigneeDocAddress.OrganisationPK);
			AssertEquals("Consignee PK should should indirectly point to new Org.", orgHeader.PK, docket.ConsigneePK);
			AssertEquals("Consignee DocAddress should indirectly point to new Org Address.", address.PK, docket.ConsigneeDocAddress.E2_OA_Address);
			AssertEquals("Consignee AddressPK should indirectly point to new Org Address.", address.PK, docket.ConsigneeAddressPK);

			docket.ConsigneeDocAddress.E2_AddressOverride = true;
			AssertNull("Consignee should be null", docket.Consignee);
			AssertEquals("ConsigneePK should be empty", ZGuid.Empty, docket.ConsigneePK);
			AssertNull("ConsigneeAddress should be null", docket.ConsigneeAddress);
			AssertEquals("ConsigneeAddressPK should be empty", ZGuid.Empty, docket.ConsigneeAddressPK);

			TestConsigneeDocAddressCore();
		}

		protected virtual void TestConsigneeDocAddressCore()
		{
		}

		#endregion

		#region TestConsigneeDocAddressDefaultTypes

		public void TestConsigneeDocAddressDefaultTypes()
		{
			var docket = GetNewBusinessObject();
			AssertDocAddressDefaultTypes(docket, DocAddressType.ConsigneeAddress, docket.ConsigneeDocAddressRequirement, ContactType.Consignee);
		}

		#endregion

		#region	TestCrossDockLocation

		public void TestCrossDockLocation()
		{
			var location = Factory.New<WhsLocation>();
			Docket.WD_WL_CrossDock = location.PK;
			AssertEquals(location, Docket.CrossDockLocation);
		}

		#endregion

		#region TestRelatedJobs_WhenDocketHasNoWorkOrders

		public void TestRelatedJobs_WhenDocketHasWorkOrders()
		{
			Factory.NewWithValidTestData<WhsWorkOrder>();
			Factory.NewWithValidTestData<WhsWorkOrder>();
			Factory.Save();

			AssertEquals("Nothing should be loaded as the Docket as no Work Orders (ZQuery.NoResultsQuery should be used).", 0, Docket.RelatedJobs.Count);
		}

		#endregion

		#region TestWorkOrders

		public void TestWorkOrders()
		{
			TestDataSimpleEnvironment data = new TestDataSimpleEnvironment(Factory);

			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			AssertEquals(0, Docket.CurrentWorkOrders.Count());

			workOrder.WD_WD_ParentDocket = Docket.PK;
			AssertEquals(1, Docket.CurrentWorkOrders.Count());
		}

		#endregion

		#region TestLoadPickLinesForAllDocketLines

		public void TestLoadPickLinesForAllDocketLines()
		{
			TestLoadPickLinesForAllDocketLinesCore();
		}

		protected virtual void TestLoadPickLinesForAllDocketLinesCore()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			for (var i = 0; i < 2; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.BOM.BikeWheel, 1m); // make sure these are on sep inv lines
			}

			Helper.CreateWhsReceiveInventoryLine(receive, data.BOM.BikeEngine, 1m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.BOM.Polish, 1m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			// order some bikes
			var docket = GetPickableDocket_ForLoadPickLinesForAllDocketLinesTest(data);
			var pick = Helper.CreatePickNew(docket);
			AssertEquals("Precondition - Pick failed.", true, docket.IsAttachedToPickButNotFinalised);
			Factory.Save();

			// load everything in a new factory to test load performance
			var otherFactory = new BusinessObjectFactory();
			var docketInOtherFactory = otherFactory.Load<WhsPickableDocket>(docket.PK);
			var line1 = FindPickableDocketLine(docketInOtherFactory, data.BOM.BikeEngine);
			var line2 = FindPickableDocketLine(docketInOtherFactory, data.BOM.BikeWheel);

			var initialDBHits = otherFactory.DatabaseLoadCount;
			docketInOtherFactory.AddFetchHintsForPickLines();
			AssertEquals("Should have used only 1 DB hit to load all PickLines", 1, otherFactory.DatabaseLoadCount - initialDBHits);
			//
			AssertEquals("Should be 1 PickLine (1 Engine).", 1, line1.PickLines.Count);
			AssertEquals("Should be 2 PickLines (1 Wheel per PickLine).", 2, line2.PickLines.Count);
			//
			AssertEquals("Should have used only 1 DB hit to load all PickLines", 1, otherFactory.DatabaseLoadCount - initialDBHits);
		}

		protected abstract WhsPickableDocket GetPickableDocket_ForLoadPickLinesForAllDocketLinesTest(TestDataForBOM data);

		protected WhsPickableDocketLine FindPickableDocketLine(WhsPickableDocket order, OrgSupplierPart part)
		{
			foreach (WhsPickableDocketLine line in order.AllLines)
			{
				if (line.WE_OP == part.PK)
				{
					return line;
				}
			}
			throw new ArgumentException("No DocketLine on Order was found for product with part number " + part.OP_PartNum + ".");
		}

		#endregion

		#region TestIsAttachedToPickButNotFinalised

		public void TestIsAttachedToPickButNotFinalised()
		{
			var pick = Factory.New<WhsPick>();
			var docket = GetNewBusinessObject();
			AssertEquals(false, docket.IsAttachedToPickButNotFinalised);

			docket.WD_WP = pick.PK;
			AssertEquals(true, docket.IsAttachedToPickButNotFinalised);

			docket.WD_FinalisedDate = ZDateTimeOffset.Today;
			AssertEquals(false, docket.IsAttachedToPickButNotFinalised);
		}

		#endregion

		#region TestIsAttachedToPick

		public void TestIsAttachedToPick()
		{
			var docket = GetNewBusinessObject();
			var pick = Factory.New<WhsPick>();
			AssertEquals(false, docket.IsAttachedToPick);

			docket.WD_WP = pick.PK;
			AssertEquals(true, docket.IsAttachedToPick);
		}

		#endregion

		#region TestIsPickFinalised

		public void TestIsPickFinalised()
		{
			var pick = Factory.New<WhsPick>();
			var docket = GetNewBusinessObject();
			docket.WD_WP = pick.PK;
			AssertEquals(false, docket.IsPickFinalised);

			pick.WP_PickStatus = PickStatus.Codes.Finalised;
			AssertEquals(true, docket.IsPickFinalised);
		}

		#endregion

		#region TestIsRecalculateOrderPricing

		public void TestIsRecalculateOrderPricing()
		{
			var client = Helper.CreateClient();
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = client.PK;

			client.MiscServ.OM_WhsIsRecalculateOrderPricing = false;
			AssertEquals(false, docket.IsRecalculateOrderPricing);

			client.MiscServ.OM_WhsIsRecalculateOrderPricing = true;
			AssertEquals(SupportsRecalculateOrderPricing, docket.IsRecalculateOrderPricing);
		}

		protected virtual bool SupportsRecalculateOrderPricing => true;

		#endregion

		#region TestIsFinalisedOrCancelledOrNotPicked

		public void TestIsFinalisedOrCancelledOrNotPicked()
		{
			var docket = GetNewBusinessObject();
			AssertEquals(true, docket.IsFinalisedOrCancelledOrNotPicked);

			var pick = Factory.New<WhsPick>();
			docket.WD_WP = pick.PK;
			AssertEquals(false, docket.IsFinalisedOrCancelledOrNotPicked);

			docket.WD_FinalisedDate = ZDateTimeOffset.UtcNow;
			AssertEquals(true, docket.IsFinalisedOrCancelledOrNotPicked);

			docket.WD_WP = ZGuid.Empty;
			AssertEquals(true, docket.IsFinalisedOrCancelledOrNotPicked);

			docket.WD_FinalisedDate = ZDateTimeOffset.Empty;
			docket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertEquals(true, docket.IsFinalisedOrCancelledOrNotPicked);

			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals(true, docket.IsFinalisedOrCancelledOrNotPicked);
		}

		#endregion

		#region TestFinaliseDocket_ShowNotificationMessage

		protected override bool ShowsConfirmationMessage
		{
			get { return false; }
		}

		#endregion

		#region TestWD_IsOrderSelectedForFinalisation

		public void TestWD_IsOrderSelectedForFinalisation()
		{
			var docket = GetNewBusinessObject();
			docket.WD_IsOrderSelectedForFinalisation = true;
			AssertEquals(true, docket.WD_IsOrderSelectedForFinalisation);

			docket.WD_IsOrderSelectedForFinalisation = false;
			AssertEquals(false, docket.WD_IsOrderSelectedForFinalisation);

			TestNonStandardReadOnly1(d => d.WD_IsOrderSelectedForFinalisationInfo);
		}

		#endregion

		#region TestWD_HandlingInstructions

		#region TestWD_HandlingInstructions_Get

		public void TestWD_HandlingInstructions_Get()
		{
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader transportCo = Factory.NewWithValidTestData<OrgHeader>();

			Docket.WD_OH_Client = client.PK;
			Docket.ConsigneePK = consignee.PK;
			Docket.TransportCoDocAddress.OrganisationPK = transportCo.PK;

			client.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "some (obsolete) special instructions on the Client");
			client.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "some handling instructions on the Client");

			consignee.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "some (obsolete) special instructions on the Consignee");
			consignee.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "some handling instructions on the Consignee");

			transportCo.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "some (obsolete) special instructions on the TransportCo");
			transportCo.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "some handling instructions on the TransportCo");

			StmNote note1 = Docket.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "some (obsolete) special instructions on the WhsOrder");
			StmNote note2 = Docket.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "some handling instructions on the WhsOrder");

			string expectedNoteText =
@"some handling instructions on the WhsOrder
some handling instructions on the Client
some (obsolete) special instructions on the Client
some handling instructions on the Consignee
some (obsolete) special instructions on the Consignee
some handling instructions on the TransportCo
some (obsolete) special instructions on the TransportCo";

			AssertEquals(expectedNoteText, Docket.WD_HandlingInstructions);
		}

		#endregion

		#region TestWD_HandlingInstructions_NotDuplicatedWhenClientConsigneeOrTransportCoAreTheSame

		public void TestWD_HandlingInstructions_NotDuplicatedWhenClientConsigneeOrTransportCoAreTheSame()
		{
			var org = Helper.CreateClient("CLIENT");
			org.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "Special Instruction from Org.");
			org.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handling Instruction from Org.");

			Docket.WD_OH_Client = org.PK;
			Docket.ConsigneePK = org.PK;
			Docket.TransportCoDocAddress.OrganisationPK = org.PK;

			string expectedNoteText =
@"Handling Instruction from Org.
Special Instruction from Org.";
			AssertEquals("Notes that came from related organisations should not be dupplicated if same org used multiple times.", expectedNoteText, Docket.WD_HandlingInstructions);
		}

		#endregion

		#region TestWD_HandlingInstructions_OnlyAppendInstructionIfModuleMatches

		public void TestWD_HandlingInstructions_OnlyAppendInstructionIfModuleMatches()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var transportCo = Factory.NewWithValidTestData<OrgHeader>();

			Docket.WD_OH_Client = client.PK;
			Docket.ConsigneePK = consignee.PK;
			Docket.TransportCoDocAddress.OrganisationPK = transportCo.PK;

			var invalidNote = client.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "This should not show up at all");
			invalidNote.ST_NoteContextModule = "Z";

			var noteWithModule = client.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Note for Warehouse Module");
			noteWithModule.ST_NoteContextModule = "W"; // this will work because this is for the warehouse module

			// Adds a default note for all types
			client.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Note for Default");

			var expectedNoteText =
@"Note for Warehouse Module
Note for Default";

			AssertEquals(expectedNoteText, Docket.WD_HandlingInstructions);
		}

		#endregion

		#region TestWD_HandlingInstructions_HasChanges

		public void TestWD_HandlingInstructions_HasChanges()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 25m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 25m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();
			AssertEquals("Pick was just saved, it shouldn't have changes.", false, pick.HasChanges);

			pick.Orders[0].WD_HandlingInstructions = "111";
			AssertEquals("Handling Note was just created, so Pick should have changes.", true, pick.HasChanges);
			Factory.Save();
			AssertEquals("Pick was just saved, it shouldn't have changes.", false, pick.HasChanges);

			var newFactory = new BusinessObjectFactory();
			var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
			pickInNewFactory.Orders[0].WD_HandlingInstructions = "555"; // bound in UI
			AssertEquals("Order Handling Note changed, so Pick should have changes.", true, pickInNewFactory.HasChanges);
		}

		#endregion

		#endregion

		#region TestWD_ParentOrderNo

		public void TestWD_ParentOrderNo()
		{
			AssertEquals(true, Docket.WD_ParentOrderNo.IsEmpty);

			var parent = GetNewBusinessObject();
			parent.WD_ExternalReference = "W123";
			Docket.WD_WD_ParentDocket = parent.PK;
			AssertEquals("W123", Docket.WD_ParentOrderNo);
		}

		#endregion

		#region TestTransportCoDocAddress

		protected override JobDocAddressRequirement GetJobDocAddressRequirement(IJobWithTransportCompany docket, DocAddressType addressType)
		{
			return ((WhsPickableDocket)docket).GetTransportCoRequirement(addressType);
		}

		public void TestTransportCoDocAddress_ReadOnlyIsLazyTriggered()
		{
			var pickableDocket = GetNewBusinessObject();
			AssertNotNull("Poke & Precondition", pickableDocket.TransportCoDocAddress);

			JobDocAddress transportCoDocAddress = null;
			var hits = GetPersistentPropertiesHitCount(() => transportCoDocAddress = pickableDocket.TransportCoDocAddress);
			AssertEquals("Getting TransportCoDocAddress should only generate minimal hits.", true, hits <= 1);

			AssertPersistentPropertiesHitCount("Expected hit for transportCoDocAddress ReadOnly is not match.", TransportCoDocAddressReadOnlyPropHit, () => _ = transportCoDocAddress.ReadOnly);
		}

		// Poking ReadOnly property does not do anything,ReadOnlyStrategy is not set for work order.
		protected virtual int TransportCoDocAddressReadOnlyPropHit => 0;

		#endregion

		#endregion

		#region TestCancel_DeleteCrossDocksAndPickLinesIfCancelled

		public void TestCancel_DeleteCrossDocksAndPickLinesIfCancelled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];

			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			var docketLine = Helper.CreateWhsPickableDocketLine(docket, data.Part1, 10m);
			var reservedPickLine = Helper.CreateReservePickLine(docketLine, inventory, 5m);
			var doggyPickLine = Helper.CreateWhsPickLine(docketLine, inventory, 2m);
			AssertEquals("Precondition", false, docket.IsCancelled);
			AssertEquals("Precondition", false, reservedPickLine.IsDeleted);
			AssertEquals("Precondition", false, doggyPickLine.IsDeleted);

			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals("Should *only* delete reserved pick lines on Cancel.", false, reservedPickLine.IsDeleted);
			AssertEquals("Should *only* delete wrong pick lines on Cancel.", false, doggyPickLine.IsDeleted);

			docket.CancelReactivateDocket();
			AssertEquals("docket.IsCancelled", true, docket.IsCancelled);
			AssertEquals("Should delete reserved pick lines on Cancel.", true, reservedPickLine.IsDeleted);
			AssertEquals("Should delete wrong pick lines on Cancel.", true, doggyPickLine.IsDeleted);
		}

		#endregion

		#region Properties

		#region TestConsigneeFieldType

		public void TestConsigneeFieldType()
		{
			Docket.ConsigneeDocAddress.E2_AddressOverride = true;
			AssertEquals(nameof(FieldType.Text), Docket.ConsigneeFieldType);

			Docket.ConsigneeDocAddress.E2_AddressOverride = false;
			AssertEquals(nameof(FieldType.Guid), Docket.ConsigneeFieldType);
		}

		#endregion

		#region TestConsigneeNameOrPK

		public void TestConsigneeNameOrPK()
		{
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "TEST";

			Docket.ConsigneeDocAddress.E2_AddressOverride = true;
			Docket.ConsigneeDocAddress.E2_CompanyName = "TEST COMPANY";
			AssertEquals("TEST COMPANY", Docket.ConsigneeNameOrPK);

			Docket.ConsigneeDocAddress.E2_AddressOverride = false;
			Docket.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			AssertEquals(consignee.PK.ToString(), Docket.ConsigneeNameOrPK);

			Docket.ConsigneeDocAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty.ToString(), Docket.ConsigneeNameOrPK);

			Docket.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			ZString code = RelatedBusinessObjectAttribute.GetCodeForGuid(Docket.ConsigneeNameOrPKInfo);
			AssertEquals("Docket.ConsigneeNameOrPK should be Consignee.PK", consignee.PK, new ZGuid(Docket.ConsigneeNameOrPKInfo.Value));
			AssertEquals("Ensure that the RelatedBusinessObjectAttribute is correct.", "TEST", code);
		}

		#endregion

		#region TestConsigneeNameOrPKMaxLength

		public void TestConsigneeNameOrPKMaxLength()
		{
			Docket.ConsigneeDocAddress.E2_AddressOverride = true;
			AssertEquals(JobDocAddressSchema.E2_CompanyName.MaxLength, Docket.ConsigneeNameOrPKInfo.MaxLength);

			Docket.ConsigneeDocAddress.E2_AddressOverride = false;
			AssertEquals(ZGuid.Empty.ToString().Length, Docket.ConsigneeNameOrPKInfo.MaxLength);
		}

		#endregion

		#region TestShortfallExists

		public void TestShortfallExists()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A", 1, 1);
			var client = Helper.CreateClient();
			var product = Helper.CreateProduct(client, "BOWLHAT");
			var receive = Helper.CreateWhsReceiveWithInventory(client, warehouse, "REC1", product, 10m);

			Factory.Save();

			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = client.PK;
			docket.WD_WW_Whs = warehouse.PK;
			var docketLine = Helper.CreateWhsPickableDocketLine(docket, product, 5m);

			AssertEquals(false, docket.ShortfallExists);

			receive.Inventory[0].WI_TotalUnits = 2m;
			AssertEquals(false, docket.ShortfallExists);
		}

		#endregion

		#region TestShortfallManager

		public void TestShortfallManager()
		{
			var docket = GetNewBusinessObject();
			AssertNotNull(docket.ShortfallManager);
			AssertEquals(docket.ShortfallManager, docket.ShortfallManager);
			AssertType<ShortfallManager>(docket.ShortfallManager);
		}

		#endregion

		#region TestCanCreateInventory

		public void TestCanCreateInventory()
		{
			AssertEquals("Pickable dockets should not directly create inventories, but they can create other jobs to create inventory.", false, Docket.CanCreateInventory);
		}

		#endregion

		#region TestWD_OH_Client

		public void TestWD_OH_Client_PickOption()
		{
			var docket = GetNewBusinessObject();

			var client1 = Helper.CreateClient("client1");
			client1.MiscServ.OM_IMDefaultWarehousePickOption = WhsPickOption.Codes.Manual;

			var client2 = Helper.CreateClient("client2");
			client2.MiscServ.OM_IMDefaultWarehousePickOption = WhsPickOption.Codes.ManualWithAutoAllocate;

			var client3 = Helper.CreateClient("client3");
			client3.MiscServ.OM_IMDefaultWarehousePickOption = WhsPickOption.Codes.Auto;

			var invalidPK = ZGuid.Empty;

			AssertEquals("Precondition: Pick Option defaulted to AUT", WhsPickOption.Codes.Auto, docket.WD_PickOption);
			Assert("Precondition: Test client is Invalid", !invalidPK.IsValid);

			docket.WD_OH_Client = client1.PK;
			AssertEquals("Warehouse Client with Manual pick option selected should cause the pick option to default to MAN", WhsPickOption.Codes.Manual, docket.WD_PickOption);
			docket.WD_OH_Client = invalidPK;
			AssertEquals("Selecting an invalid Warehouse Client should leave the pick option unchanged", WhsPickOption.Codes.Manual, docket.WD_PickOption);

			docket.WD_OH_Client = client2.PK;
			AssertEquals("Warehouse Client with Manual with Auto Allocate pick option selected should cause the pick option to default to MAT", WhsPickOption.Codes.ManualWithAutoAllocate, docket.WD_PickOption);
			docket.WD_OH_Client = invalidPK;
			AssertEquals("Selecting an invalid Warehouse Client should leave the pick option unchanged", WhsPickOption.Codes.ManualWithAutoAllocate, docket.WD_PickOption);

			docket.WD_OH_Client = client3.PK;
			AssertEquals("Warehouse Client with Automatic pick option selected should cause the pick option to default to AUT", WhsPickOption.Codes.Auto, docket.WD_PickOption);
			docket.WD_OH_Client = invalidPK;
			AssertEquals("Selecting an invalid Warehouse Client should leave the pick option unchanged", WhsPickOption.Codes.Auto, docket.WD_PickOption);
		}

		#endregion

		#region TestWD_OH_Client_PickOption_ClientNotChanged

		public void TestWD_OH_Client_PickOption_ClientNotChanged()
		{
			var docket = GetNewBusinessObject();

			var client1 = Helper.CreateClient("client1");
			client1.MiscServ.OM_IMDefaultWarehousePickOption = WhsPickOption.Codes.Manual;

			docket.WD_OH_Client = client1.PK;

			AssertEquals("Precondition: Order pick option should be set to 'Manual'.", WhsPickOption.Codes.Manual, client1.MiscServ.OM_IMDefaultWarehousePickOption);

			client1.MiscServ.OM_IMDefaultWarehousePickOption = WhsPickOption.Codes.Auto;
			docket.WD_OH_Client = client1.PK;

			AssertEquals("The client should not have changed.", client1.PK, docket.WD_OH_Client);
			AssertEquals("The client default pick option should have updated.", WhsPickOption.Codes.Auto, client1.MiscServ.OM_IMDefaultWarehousePickOption);
			AssertEquals("The pick option of order should not change after the default client pick option changes.", WhsPickOption.Codes.Manual, docket.WD_PickOption);
		}

		#endregion

		#region TestWD_WP

		public void TestWD_WP()
		{
			var docket = GetNewBusinessObject();
			Assert("Precondition", !docket.IsValidationSuspended);

			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			docket.WD_WP = Factory.New<WhsPick>().PK;
			AssertHasError(docket.WD_WPInfo, "Un-finalized Docket without status of ATTACHED TO PICK or PICKING must not be attached to any pick.");
			AssertHasError(docket.WD_DocketStatusInfo, "Un-finalized Docket has a pick so docket status must set to either ATTACHED TO PICK or PICKING.");

			docket.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			AssertNoErrors(docket.WD_WPInfo);
			AssertNoErrors(docket.WD_DocketStatusInfo);
		}

		#endregion

		#region TestWD_WP_RefreshesReservedPickLines

		public void TestWD_WP_RefreshesReservedPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedPickLine.ReservedQuantity);
			AssertContainsExactElementsInAnyOrder(new[] { reservedPickLine }, orderLine.ReservedPickLines);
			AssertContainsExactElementsInAnyOrder(new[] { reservedPickLine }, inventory.ReservedPickLines);

			order.WD_WP = Factory.New<WhsPick>().PK;
			AssertEquals("When Pick is set, Reserved Pick Lines should be refreshed.", 0, orderLine.ReservedPickLines.Count);
			AssertEquals("When Pick is set, Reserved Pick Lines should be refreshed.", 0, inventory.ReservedPickLines.Count);
		}

		#endregion

		#region TestWD_DocketStatus

		public override void TestWD_DocketStatus()
		{
			var docket = GetNewBusinessObject();
			Assert("Precondition", !docket.IsValidationSuspended);

			docket.WD_WP = ZGuid.Empty;
			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertNoErrors(docket.WD_WPInfo);
			AssertNoErrors(docket.WD_DocketStatusInfo);

			docket.WD_WP = Factory.New<WhsPick>().PK;
			AssertHasError(docket.WD_DocketStatusInfo, "Un-finalized Docket has a pick so docket status must set to either ATTACHED TO PICK or PICKING.");
			AssertHasError(docket.WD_WPInfo, "Un-finalized Docket without status of ATTACHED TO PICK or PICKING must not be attached to any pick.");
		}

		#endregion

		#region TestLogEventWithCorrectEventTime

		[TestDate(2016, 3, 9, 10, 10, 0)]
		public void TestLogEventWithCorrectEventTime()
		{
			var now = ZDateTimeOffset.Now;
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");

			// docket, has EstimatedDateChanged added
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = org.PK;
			docket.WD_WW_Whs = whs.PK;
			Factory.Save();

			var estimateDateChangeEvent = docket.Logs.AddNew(Events.EstimatedDateChanged, ZString.Empty, now.AddDays(2));
			Factory.Save();

			CreateNewPick(docket, whs);
			SetFinsalisedDocketValuesForTest(docket);
			Factory.Save();

			AssertEquals(ZDateTime.Now.AddDays(2), estimateDateChangeEvent.SL_EventTime);

			// docket2, has an IsEstimated = true event added
			var docket2 = GetNewBusinessObject();
			docket2.WD_OH_Client = org.PK;
			docket2.WD_WW_Whs = whs.PK;
			Factory.Save();

			var estimateEvent = docket2.Logs.AddNew(Events.PickedUp, now.AddDays(3), true);
			Factory.Save();

			CreateNewPick(docket2, whs);
			if (docket2 is WhsComponentOrder)
			{
				docket2.WD_DocketStatus = DocketStatus.Codes.Finalised;
			}
			docket2.WD_FinalisedDate = now;
			Factory.Save();

			AssertEquals(ZDateTime.Now.AddDays(3), estimateEvent.SL_EventTime);
		}

		#endregion

		#region TestUpdateTotalWeightAndVolumeWhenLineRemoved

		public void TestUpdateTotalWeightAndVolumeWhenLineRemoved()
		{
			TestUpdateTotalWeightAndVolumeWhenLineRemovedCore();
		}

		protected virtual void TestUpdateTotalWeightAndVolumeWhenLineRemovedCore()
		{
			var org = Helper.CreateClient();

			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = org.PK;
			docket.WD_TotalWeightUnit = Constants.Weight.Grams;
			docket.WD_TotalCubicUnit = Constants.Volume.Litre;

			var part1 = Helper.CreateProduct(org, "PRODUCT1");
			part1.OP_Cubic = 0.2m;
			part1.OP_CubicUQ = Constants.Volume.CubicMetres;
			part1.OP_Weight = 3.0m;
			part1.OP_WeightUQ = Constants.Weight.Kilograms;

			var line1 = docket.Lines.AddNew();
			var line2 = docket.Lines.AddNew();
			var line3 = docket.Lines.AddNew();
			line1.WE_OP = part1.PK;
			line2.WE_OP = part1.PK;
			line3.WE_OP = part1.PK;
			line1.WE_TransactionQuantity = 2m;
			line2.WE_TransactionQuantity = 3m;
			line3.WE_TransactionQuantity = 4m;

			AssertEquals("Precondition", 27000m, docket.WD_TotalWeight);
			AssertEquals("Precondition", 1800m, docket.WD_TotalCubic);

			docket.Lines.Delete(line2);
			AssertEquals(18000m, docket.WD_TotalWeight);
			AssertEquals(1200m, docket.WD_TotalCubic);

			docket.Lines.RemoveFromRelationship(line1);
			AssertEquals(12000m, docket.WD_TotalWeight);
			AssertEquals(800m, docket.WD_TotalCubic);
		}

		#endregion

		#region Flags

		#region TestIsPickFinalising

		public void TestIsPickFinalising()
		{
			var docket = GetNewBusinessObject();
			AssertEquals("There is no Pick, IsPickFinalising should be false.", false, docket.IsPickFinalising);

			var pick = Factory.New<WhsPick>();
			docket.WD_WP = pick.PK;
			AssertEquals("Pick is not finalising, IsPickFinalising should be false.", false, docket.IsPickFinalising);

			using (new SemaphoreManager(pick.FinalisePickSemaphore))
			{
				AssertEquals("Pick is finalising, IsPickFinalising should be true.", true, docket.IsPickFinalising);
			}
		}

		#endregion

		#region TestIsPartiallyOrFullyPickedFromPutawayLocation

		public void TestIsPartiallyOrFullyPickedFromPutawayLocation()
		{
			TestIsPartiallyOrFullyPickedFromPutawayLocationCore();
		}

		protected virtual void TestIsPartiallyOrFullyPickedFromPutawayLocationCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 100m);
			var pickableDocket = GetNewBusinessObject();
			pickableDocket.WD_OH_Client = data.Org1.PK;
			pickableDocket.WD_WW_Whs = data.Whs1.PK;
			pickableDocket.WD_PickOption = WhsPickOption.Codes.Manual;
			pickableDocket.WD_RequiredDate = ZDateTimeOffset.Today;
			pickableDocket.ConsigneePK = Helper.CreateClient("CONSIGNEE").PK;
			var orderLine1 = Helper.CreateWhsPickableDocketLine(pickableDocket, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsPickableDocketLine(pickableDocket, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(pickableDocket);
			AssertEquals("When pickableDocket have no pick lines attached to its lines it is not considered picked.", false, pickableDocket.IsPartiallyOrFullyPickedFromPutawayLocation);

			var pickLine1 = Helper.CreateWhsPickLine(orderLine1, receive1.Inventory[0], 10m);
			var pickLine2 = Helper.CreateWhsPickLine(orderLine2, receive2.Inventory[0], 10m);
			AssertEquals("When pickableDocket have not picked pick line attached to its lines, then it is still not considered picked.", false, pickableDocket.IsPartiallyOrFullyPickedFromPutawayLocation);

			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("When pickableDocket have picked pick line attached to its lines, then it is considered picked.", true, pickableDocket.IsPartiallyOrFullyPickedFromPutawayLocation);

			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Empty;
			AssertEquals("Precondition.", false, pickableDocket.IsPartiallyOrFullyPickedFromPutawayLocation);

			pickLine1.WZ_WE_OriginalPickedInventoryLine = receive2.Lines[0].PK;
			AssertEquals("When pickableDocket have In-Transit pick line attached to its lines, then it is considered picked from Putaway Location.", true, pickableDocket.IsPartiallyOrFullyPickedFromPutawayLocation);
		}

		#endregion

		#region TestIsCurrentlyBeingPickedFromPutawayLocation

		public void TestIsCurrentlyBeingPickedFromPutawayLocation()
		{
			TestIsCurrentlyBeingPickedFromPutawayLocationCore();
		}

		protected virtual void TestIsCurrentlyBeingPickedFromPutawayLocationCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 100m);
			var pickableDocket = GetNewBusinessObject();
			pickableDocket.WD_OH_Client = data.Org1.PK;
			pickableDocket.WD_WW_Whs = data.Whs1.PK;
			pickableDocket.WD_PickOption = WhsPickOption.Codes.Manual;
			pickableDocket.WD_RequiredDate = ZDateTimeOffset.Today;
			pickableDocket.ConsigneePK = Helper.CreateClient("CONSIGNEE").PK;
			var orderLine1 = Helper.CreateWhsPickableDocketLine(pickableDocket, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsPickableDocketLine(pickableDocket, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(pickableDocket);
			AssertEquals("When pickableDocket has no pick lines attached to its order lines, it is not considered as currently being picked.", false, pickableDocket.IsCurrentlyBeingPickedFromPutawayLocation);

			var pickLine1 = Helper.CreateWhsPickLine(orderLine1, receive1.Inventory[0], 10m);
			var pickLine2 = Helper.CreateWhsPickLine(orderLine2, receive2.Inventory[0], 10m);
			AssertEquals("When pickableDocket's pick lines are not currently being picked, then it is still not considered as currently being picked.", false, pickableDocket.IsCurrentlyBeingPickedFromPutawayLocation);

			pickLine1.WZ_GS_NKAssignedTo = "AAA";
			pickLine1.WZ_IsPicking = true;
			AssertEquals("When any of pickableDocket's pick lines are marked IsPicking, it is considered as currently being picked.", true, pickableDocket.IsCurrentlyBeingPickedFromPutawayLocation);
		}

		#endregion

		#endregion

		#region TestWD_F3_NKTotalPackType

		public void TestWD_F3_NKTotalPackType_ReadOnly()
		{
			TestNonStandardReadOnly1(d => d.WD_F3_NKTotalPackTypeInfo);
		}

		#endregion

		#region TestWD_PackagesSent

		public void TestWD_PackagesSent_ReadOnly()
		{
			TestNonStandardReadOnly1(d => d.WD_PackagesSentInfo);
		}

		#endregion

		#region TestWD_TotalPallets

		public void TestWD_TotalPallets_ReadOnly()
		{
			TestNonStandardReadOnly1(d => d.WD_TotalPalletsInfo);
		}

		#endregion

		#region TestWD_TotalUnits

		public void TestWD_TotalUnits_ReadOnly()
		{
			TestNonStandardReadOnly1(d => d.WD_TotalUnitsInfo);
		}

		#endregion

		#region TestRequiredDate

		public void TestRequiredDate_ReadOnly()
		{
			TestRequiredDate_ReadOnlyCore();
		}

		protected virtual void TestRequiredDate_ReadOnlyCore()
		{
			TestNonStandardReadOnly1(d => d.WD_RequiredDateInfo);
		}

		public void TestWhsPickableDocket_RequiredDate_ReadOnly()
		{
			var pickableDocket = Factory.New<PickableDocketForTest>();
			AssertEquals("Precondition: readonly should be off.", false, pickableDocket.RequiredDateInfo.ReadOnly);

			pickableDocket.WD_DocketID = "Testing";
			AssertEquals("Readonly should be on", true, pickableDocket.RequiredDateInfo.ReadOnly);
		}

		public void TestRequiredDate()
		{
			TestRequiredDateCore();
		}

		protected virtual void TestRequiredDateCore()
		{
			var branch = Helper.CreateGlbBranch("BN1");
			branch.GB_RL_NKHomePort = "AUBNE";
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_GB_RelatedCompanyBranch = branch.PK;
			Factory.Save();

			var pickableDocket = GetNewBusinessObject();
			pickableDocket.WD_WW_Whs = data.Whs1.PK;
			AssertEquals("Empty on creation", ZDateTime.Empty, pickableDocket.RequiredDate);

			pickableDocket.RequiredDate = new ZDateTime(2024, 07, 05, 12, 12, 12);
			AssertEquals("WD_RequiredDate correct", new ZDateTimeOffset(2024, 07, 05, 12, 12, 12, TimeSpan.FromHours(10)), pickableDocket.WD_RequiredDate);

			AssertEquals("RequiredDate correct", new ZDateTime(2024, 07, 05, 12, 12, 12), pickableDocket.RequiredDate);
		}

		#endregion

		#region TestWD_RequiredDate_ReadOnly

		public void TestWD_RequiredDate_ReadOnly()
		{
			TestWD_RequiredDate_ReadOnlyCore();
		}

		protected virtual void TestWD_RequiredDate_ReadOnlyCore()
		{
			TestNonStandardReadOnly1(d => d.WD_RequiredDateInfo);
		}

		public void TestWhsPickableDocket_WD_RequiredDate_ReadOnly()
		{
			var pickableDocket = Factory.New<PickableDocketForTest>();
			AssertEquals("Precondition: readonly should be off.", false, pickableDocket.WD_RequiredDateInfo.ReadOnly);

			pickableDocket.WD_DocketID = "Testing";
			AssertEquals("Readonly should be on", true, pickableDocket.WD_RequiredDateInfo.ReadOnly);
		}

		class PickableDocketForTest : WhsWorkOrder
		{
			public PickableDocketForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override bool RequiredDateReadOnly => WD_DocketID == "Testing";
			protected override bool NonStandardReadOnly1 => WD_DocketType != DocketType.Codes.WorkOrder;
		}

		#endregion

		#region TestVehicleNo

		public void TestVehicleNo()
		{
			var pickableDocket = Factory.New<PickableDocketForTest>();

			AssertEquals(ZString.Empty, pickableDocket.VehicleNo);

			pickableDocket.References.AddNew();
			pickableDocket.References[0].WX_RefType = "VHN";
			pickableDocket.References[0].WX_Reference = "123";
			AssertEquals("123", pickableDocket.VehicleNo);

			pickableDocket.References[0].WX_RefType = "HSB";
			AssertEquals(ZString.Empty, pickableDocket.VehicleNo);

			pickableDocket.References[0].WX_RefType = "VHN";
			pickableDocket.VehicleNo = "345";
			AssertEquals("345", pickableDocket.References[0].WX_Reference);

			pickableDocket.References.AddNew();
			pickableDocket.References[1].WX_RefType = "XXX";
			pickableDocket.VehicleNo = ZString.Empty;
			AssertEquals(1, pickableDocket.References.Count);
			AssertEquals("XXX", pickableDocket.References[0].WX_RefType);
			AssertEquals(ZString.Empty, pickableDocket.VehicleNo);
		}

		public void TestVehicleNo_OperationalActions()
		{
			var actionFieldAttribute = ActionFieldAttribute.Get(typeof(WhsPickableDocket).GetProperty(WhsPickableDocket.Schema.VehicleNo));
			AssertNotNull($"Action field attribute of {WhsPickableDocket.Schema.VehicleNo} should exist", actionFieldAttribute);
		}

		#endregion

		#region TestVehicleNoInfo

		public void TestVehicleNoInfo()
		{
			var docket = GetNewBusinessObject();
			AssertEquals(25, docket.VehicleNoInfo.MaxLength);
			TestNonStandardReadOnly1(d => d.VehicleNoInfo);
		}

		#endregion

		#region TestSalesChannelCode

		public void TestSalesChannelCode()
		{
			TestSalesChannelCodeCore();
		}

		protected virtual void TestSalesChannelCodeCore()
		{
			AssertEquals(string.Empty, GetNewBusinessObject().SalesChannelCode);
		}

		#endregion

		public void TestIsHeldInventoryOrder()
		{
			var docket = GetNewBusinessObject();
			AssertEquals(false, docket.IsHeldInventoryOrder);

			var line = docket.Lines.AddNew();
			AssertEquals(false, docket.IsHeldInventoryOrder);

			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // Only possible with EnableHeldGoodsForOrders
			{
				line.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;
				AssertEquals(true, docket.IsHeldInventoryOrder);

				line.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Damaged;
				AssertEquals(true, docket.IsHeldInventoryOrder);
			}

			docket.Lines.Delete(line);
			AssertEquals(false, docket.IsHeldInventoryOrder);
		}

		#endregion

		#region TestGetShortfallExistsStatus

		public void TestGetShortfallExistsStatus_WhenNoShortfall()
		{
			var docket = GetNewBusinessObject();
			AssertEquals("Precondition", 0, docket.Lines.Count);
			AssertEquals("No lines, no shortfall expected.", false, docket.GetShortfallExistsStatus());

			// create order with no shortfalls
			var data = new TestDataForBOM(Factory);
			var dummyClient = Factory.NewWithValidTestData<OrgHeader>();
			var dummyWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			docket.WD_OH_Client = dummyClient.PK;
			docket.WD_WW_Whs = dummyWarehouse.PK;

			data.CreateBOMProductsInInventory(saveFactoryForWarehouse: true); // for order test
			data.CreateBOMComponentsInInventory(); // for workorder test

			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_RequiredDate = ZDateTimeOffset.Now;
			docket.ConsigneeAddressPK = data.Org1.MainAddress.PK;
			Factory.Save(); // for inventory

			Helper.CreateWhsPickableDocketLine(docket, data.BOM.Bike, 5m);

			Helper.CreatePickNew(docket);
			AssertEquals("Precondition", true, docket.IsAttachedToPickButNotFinalised);
			AssertEquals(false, docket.GetShortfallExistsStatus());
		}

		public void TestGetShortfallExistsStatus_WhenShortfallExists()
		{
			var docket = GetNewBusinessObject();
			AssertEquals("Precondition", 0, docket.Lines.Count);
			AssertEquals("No lines, no shortfall expected.", false, docket.GetShortfallExistsStatus());

			// create order with shortfalls
			var data = new TestDataForBOM(Factory);
			var dummyClient = Helper.CreateClient("DY");
			var dummyWarehouse = Helper.CreateWarehouse("DYW", "D2", "A");
			docket.WD_OH_Client = dummyClient.PK;
			docket.WD_WW_Whs = dummyWarehouse.PK;

			data.CreateBOMProductsInInventory(saveFactoryForWarehouse: true); // for order test
			data.CreateBOMComponentsInInventory(); // for workorder test

			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_RequiredDate = ZDateTimeOffset.Now;
			docket.ConsigneeAddressPK = data.Org1.MainAddress.PK;
			Factory.Save(); // for inventory

			Helper.CreateWhsPickableDocketLine(docket, data.BOM.Bike, 110m);

			Helper.CreatePickNew(docket);
			AssertEquals("Precondition", true, docket.IsAttachedToPickButNotFinalised);
			AssertEquals(UsesShortfall, docket.GetShortfallExistsStatus());
		}

		protected virtual bool UsesShortfall => true;

		#endregion

		#region Picking

		#region TestGetLinesToPick

		public abstract void TestGetLinesToPick();

		#endregion

		#region TestGetPickability

		public void TestGetPickability()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);

			var pick = Factory.New<WhsPick>();

			var order = GetNewBusinessObject();
			order.WD_OH_Client = data.Org1.PK;
			order.WD_WW_Whs = data.Whs1.PK;
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			order.ConsigneeAddressPK = data.Org1.MainAddress.PK;

			var line = order.Lines.AddNew();
			line.WE_TransactionQuantity = 5;
			line.WE_OP = data.Part1.PK;
			line.SetShortfallForTest(0);

			WhsPick.DocketPickabilityEventArgs args;

			// pick is not in a valid state (WP_PickStatus is not CREATED)
			pick.WP_PickNo = "XYZ";
			pick.WP_PickStatus = PickStatus.Codes.PickSlip;

			args = order.GetPickability(pick);
			AssertGetPickabilityResult("Pick XYZ is Finalized, Canceled or has had it's Pick Slip printed.", NotificationTypes.Error, false, true, args);
			pick.WP_PickStatus = PickStatus.Codes.Created; // clean up

			// pick is cancelled
			pick.WP_PickNo = "XYZ";
			pick.WP_PickStatus = PickStatus.Codes.Cancelled;

			args = order.GetPickability(pick);
			AssertGetPickabilityResult("Pick XYZ is Finalized, Canceled or has had it's Pick Slip printed.", NotificationTypes.Error, false, false, args);
			pick.WP_PickNo = "";
			pick.WP_PickStatus = PickStatus.Codes.Created; // clean up

			// no security to auto-allocate w/manual + auto-allocation pick
			Env.Security.WhsPicking.IsAllowed = false;
			order.WD_PickOption = WhsPickOption.Codes.ManualWithAutoAllocate;

			args = order.GetPickability(pick);
			AssertGetPickabilityResult(WhsErrorTypes.NoSecurityRights.Message, NotificationTypes.Error, false, false, args);
			Env.Security.WhsPicking.IsAllowed = true; // clean up

			// no security to auto-allocate w/manual pick
			Env.Security.WhsPicking.IsAllowed = false;
			order.WD_PickOption = WhsPickOption.Codes.Manual;

			args = order.GetPickability(pick);
			AssertGetPickabilityResult(WhsErrorTypes.NoSecurityRights.Message, NotificationTypes.Error, false, false, args);
			Env.Security.WhsPicking.IsAllowed = true; // clean up

			// order has a DocketStatus other than Entered, New or Picking (Held)
			order.WD_DocketStatus = DocketStatus.Codes.Held;

			args = order.GetPickability(pick);
			AssertGetPickabilityResult(string.Format("This {0} is Held. Only Entered (Saved) {0}s can be attached to a Pick.", order.Description), NotificationTypes.Error, false, false, args);

			// order has a DocketStatus other than Entered (Finalised)
			order.WD_DocketStatus = DocketStatus.Codes.Picking;
			order.WD_FinalisedDate = ZDateTimeOffset.Now;

			args = order.GetPickability(pick);
			AssertGetPickabilityResult(string.Format("This {0} is Picking. Only Entered (Saved) {0}s can be attached to a Pick.", order.Description), NotificationTypes.Error, false, false, args);
			order.WD_DocketStatus = DocketStatus.Codes.New; // clean up
			order.WD_FinalisedDate = ZDateTimeOffset.Empty;

			// order has a different Warehouse to the Pick's Warehouse
			pick.WP_WW_Whs = Factory.New<WhsWarehouse>().PK;

			args = order.GetPickability(pick);
			AssertGetPickabilityResult(string.Format("This {0} is for a different Warehouse.", order.Description), NotificationTypes.Error, false, false, args);
			pick.WP_WW_Whs = order.Warehouse.PK; // clean up (and ensures that same warehouse is ok)

			// order has no lines
			Array.ForEach(order.Lines.ToArray<WhsPickableDocketLine>(), oLine => order.Lines.RemoveFromRelationship(oLine));

			args = order.GetPickability(pick);
			AssertGetPickabilityResult(string.Format("This {0} has no Lines.", order.Description), NotificationTypes.Error, false, false, args);
			order.Lines.Add(line); // clean up

			// order has no units
			line.WE_TransactionQuantity = 0m;

			args = order.GetPickability(pick);
			AssertGetPickabilityResult(string.Format("This {0} has no Units.", order.Description), NotificationTypes.Error, false, false, args);
			line.WE_TransactionQuantity = 5;           //
			line.SetShortfallForTest(0); // cleanup

			// order has no req'd by date (can happen when generating orders via op actions where the user needs to enter the req by date manually)
			order.WD_RequiredDate = ZDateTimeOffset.Empty;
			args = order.GetPickability(pick);
			AssertGetPickabilityResult(string.Format("This {0} has no Required By Date.", order.Description), NotificationTypes.Error, false, false, args);
			order.WD_RequiredDate = ZDateTimeOffset.Now; // clean up

			// order exists on another pick
			WhsPick otherPick = Factory.New<WhsPick>();
			otherPick.Orders.Add(order);

			args = order.GetPickability(pick);
			AssertGetPickabilityResult(string.Format("This {0} is attached to another Pick.", order.Description), NotificationTypes.Error, false, false, args);
			otherPick.CancelPick(); // clean up

			// allow sub-classes to test their specific validation
			TestGetPickabilityCore(pick, order, line, args);

			// order has no errors
			order.WD_DocketStatus = DocketStatus.Codes.New;
			args = order.GetPickability(pick);
			AssertGetPickabilityResult("", NotificationTypes.None, true, false, args);

			order.WD_DocketStatus = DocketStatus.Codes.Entered;
			args = order.GetPickability(pick);
			AssertGetPickabilityResult("", NotificationTypes.None, true, false, args);

			// test that multiple errors are concatenated
			pick.WP_WW_Whs = Factory.New<WhsWarehouse>().PK;
			Array.ForEach(order.Lines.ToArray<WhsPickableDocketLine>(), oLine => order.Lines.RemoveFromRelationship(oLine));

			args = order.GetPickability(pick);
			AssertGetPickabilityResult(string.Format(@"
This {0} is for a different Warehouse.
This {0} has no Lines.".Trim(), order.Description), NotificationTypes.Error, false, false, args);
		}

		public void TestGetPickability_WaitingReplenishment()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);

			var pick = Factory.New<WhsPick>();

			var order = GetNewBusinessObject();
			order.WD_OH_Client = data.Org1.PK;
			order.WD_WW_Whs = data.Whs1.PK;
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			order.ConsigneeAddressPK = data.Org1.MainAddress.PK;

			var line = order.Lines.AddNew();
			line.WE_TransactionQuantity = 5;
			line.WE_OP = data.Part1.PK;
			line.SetShortfallForTest(0);

			pick.WP_IsAwaitingReplenishment = true;
			var args = order.GetPickability(pick);
			AssertGetPickabilityResult("", NotificationTypes.None, true, false, args);
		}

		public void TestGetPickability_WithoutPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);

			var order = GetNewBusinessObject();
			order.WD_OH_Client = data.Org1.PK;
			order.WD_WW_Whs = data.Whs1.PK;
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			order.ConsigneeAddressPK = data.Org1.MainAddress.PK;

			var line = order.Lines.AddNew();
			line.WE_TransactionQuantity = 5;
			line.WE_OP = data.Part1.PK;
			line.SetShortfallForTest(0);

			WhsPick.DocketPickabilityEventArgs args;

			// no security to auto-allocate w/manual + auto-allocation pick
			Env.Security.WhsPicking.IsAllowed = false;
			order.WD_PickOption = WhsPickOption.Codes.ManualWithAutoAllocate;

			args = order.GetPickabilityWithoutPick();
			AssertGetPickabilityResult(WhsErrorTypes.NoSecurityRights.Message, NotificationTypes.Error, false, false, args);
			Env.Security.WhsPicking.IsAllowed = true; // clean up

			// no security to auto-allocate w/manual pick
			Env.Security.WhsPicking.IsAllowed = false;
			order.WD_PickOption = WhsPickOption.Codes.Manual;

			args = order.GetPickabilityWithoutPick();
			AssertGetPickabilityResult(WhsErrorTypes.NoSecurityRights.Message, NotificationTypes.Error, false, false, args);
			Env.Security.WhsPicking.IsAllowed = true; // clean up

			// order has a DocketStatus other than Entered, New or Picking (Held)
			order.WD_DocketStatus = DocketStatus.Codes.Held;
			args = order.GetPickabilityWithoutPick();
			AssertGetPickabilityResult(string.Format("This {0} is Held. Only Entered (Saved) {0}s can be attached to a Pick.", order.Description), NotificationTypes.Error, false, false, args);

			// order has a DocketStatus other than Entered (Finalised)
			order.WD_DocketStatus = DocketStatus.Codes.Picking;
			order.WD_FinalisedDate = ZDateTimeOffset.Now;

			args = order.GetPickabilityWithoutPick();
			AssertGetPickabilityResult(string.Format("This {0} is Picking. Only Entered (Saved) {0}s can be attached to a Pick.", order.Description), NotificationTypes.Error, false, false, args);
			order.WD_FinalisedDate = ZDateTimeOffset.Empty;
			order.WD_DocketStatus = DocketStatus.Codes.New; // clean up

			// order has no lines
			Array.ForEach(order.Lines.ToArray<WhsPickableDocketLine>(), oLine => order.Lines.RemoveFromRelationship(oLine));
			args = order.GetPickabilityWithoutPick();
			AssertGetPickabilityResult(string.Format("This {0} has no Lines.", order.Description), NotificationTypes.Error, false, false, args);
			order.Lines.Add(line); // clean up

			// order has no units
			line.WE_TransactionQuantity = 0m;
			args = order.GetPickabilityWithoutPick();
			AssertGetPickabilityResult(string.Format("This {0} has no Units.", order.Description), NotificationTypes.Error, false, false, args);
			line.WE_TransactionQuantity = 5;           //
			line.SetShortfallForTest(0); // cleanup

			// order has no req'd by date (can happen when generating orders via op actions where the user needs to enter the req by date manually)
			order.WD_RequiredDate = ZDateTimeOffset.Empty;
			args = order.GetPickabilityWithoutPick();
			AssertGetPickabilityResult(string.Format("This {0} has no Required By Date.", order.Description), NotificationTypes.Error, false, false, args);
			order.WD_RequiredDate = ZDateTimeOffset.Now; // clean up

			// allow sub-classes to test their specific validation
			TestGetPickabilityCore(order, line, args);

			// order has no errors
			order.WD_DocketStatus = DocketStatus.Codes.New;
			args = order.GetPickabilityWithoutPick();
			AssertGetPickabilityResult("", NotificationTypes.None, true, false, args);

			order.WD_DocketStatus = DocketStatus.Codes.Entered;
			args = order.GetPickabilityWithoutPick();
			AssertGetPickabilityResult("", NotificationTypes.None, true, false, args);

			// test that multiple errors are concatenated
			Array.ForEach(order.Lines.ToArray<WhsPickableDocketLine>(), oLine => order.Lines.RemoveFromRelationship(oLine));
			order.WD_RequiredDate = ZDateTimeOffset.Empty;
			args = order.GetPickabilityWithoutPick();
			AssertGetPickabilityResult(string.Format(@"
This {0} has no Required By Date.
This {0} has no Lines.".Trim(), order.Description), NotificationTypes.Error, false, false, args);
		}

		/// <summary>
		/// Subclasses should override this method to add class-specific assertions
		/// </summary>
		protected virtual void TestGetPickabilityCore(WhsPickableDocket order, WhsPickableDocketLine line, WhsPick.DocketPickabilityEventArgs args)
		{
		}

		/// <summary>
		/// Subclasses should override this method to add class-specific assertions
		/// </summary>
		protected virtual void TestGetPickabilityCore(WhsPick pick, WhsPickableDocket order, WhsPickableDocketLine line, WhsPick.DocketPickabilityEventArgs args)
		{
		}

		protected void AssertGetPickabilityResult(string expectedMsg, NotificationTypes expectedMsgType, bool expectedPickability, bool expectedPickExists,
			WhsPick.DocketPickabilityEventArgs args)
		{
			AssertEquals("expectedMsg: ", expectedMsg, args.Message);
			AssertEquals("expectedMsgType: ", expectedMsgType, args.MessageType);
			AssertEquals("expectedPickability: ", expectedPickability, args.IsDocketPickable);
			AssertEquals("expectedPickExists: ", expectedPickExists, args.PickAlreadyExists);
		}

		#endregion

		#endregion

		#region Fetch Strategy

		protected override Type FetchStrategyType
		{
			get { return typeof(WhsPickableDocketFetchStrategy); }
		}

		#endregion

		#region ReadOnly

		#region TestConsigneeDocAddress_ReadOnly

		#region TestConsigneeDocAddress

		public void TestConsigneeDocAddress_ReadOnly_WhsReleasePostFinaliseEdit_NotAllowed()
		{
			TestConsigneeDocAddress_ReadOnlyCore(isPostFinaliseEditAllowed: false);
		}

		public void TestConsigneeDocAddress_ReadOnly_WhsReleasePostFinaliseEdit_Allowed()
		{
			TestConsigneeDocAddress_ReadOnlyCore(isPostFinaliseEditAllowed: true);
		}

		protected virtual void TestConsigneeDocAddress_ReadOnlyCore(bool isPostFinaliseEditAllowed)
		{
			var pickableDocket = GetNewBusinessObject();
			_ = pickableDocket.ConsigneeDocAddress; // Testing that changing docket status refreshes the read only

			pickableDocket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertEquals(true, pickableDocket.ConsigneeDocAddress.ReadOnly);

			pickableDocket.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals(false, pickableDocket.ConsigneeDocAddress.ReadOnly);

			pickableDocket.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			AssertEquals(false, pickableDocket.ConsigneeDocAddress.ReadOnly);

			pickableDocket.WD_DocketStatus = DocketStatus.Codes.Picking;
			AssertEquals(false, pickableDocket.ConsigneeDocAddress.ReadOnly);

			var pick = Helper.CreatePickNew(false, false, false, true, pickableDocket);
			pickableDocket.WD_WP = pick.PK;
			pick.WP_PickStatus = PickStatus.Codes.Finalised;
			AssertIsFinalisedPrecondition(pickableDocket.Pick);

			Env.Security.WhsReleasePostFinaliseEdit.IsAllowed = isPostFinaliseEditAllowed;
			pickableDocket.WD_DocketStatus = DocketStatus.Codes.Finalised;
			AssertEquals(!isPostFinaliseEditAllowed, pickableDocket.ConsigneeDocAddress.ReadOnly);
		}

		public void TestConsigneeDocAddress_DoesNotGetLoadedOnDocketStatusChange()
		{
			var pickableDocket = GetNewBusinessObject();
			AssertNull("Precondition: Should *not* have loaded the ConsigneeDocAddress.", pickableDocket.DocAddresses.FindByDocAddressType(pickableDocket.ConsigneeDocAddressRequirement.DefaultDocAddressType));

			pickableDocket.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertNull("Should *not* have loaded the ConsigneeDocAddress.", pickableDocket.DocAddresses.FindByDocAddressType(pickableDocket.ConsigneeDocAddressRequirement.DefaultDocAddressType));

			AssertEquals(true, pickableDocket.ConsigneeDocAddress.ReadOnly);
			AssertNotNull("Post-Precondition?: Should have loaded the ConsigneeDocAddress when accessing it above.", pickableDocket.DocAddresses.FindByDocAddressType(pickableDocket.ConsigneeDocAddressRequirement.DefaultDocAddressType));
		}

		#endregion

		#region TestConsigneeNameOrPKReadOnly

		public void TestConsigneeNameOrPKReadOnly()
		{
			TestConsigneeNameOrPKReadOnlyCore();
		}

		protected abstract void TestConsigneeNameOrPKReadOnlyCore();

		#endregion

		#endregion

		#region TestStandardReadOnly

		protected override void TestStandardReadOnlyCore(Func<TPickableDocket, bool> getReadOnly, string name, TPickableDocket docket, bool readOnlyWhenDocketHasLines)
		{
			docket.WD_WP = Factory.New<WhsPick>().PK;
			AssertEquals($"{name} should be readonly if docket is Picking", true, getReadOnly(docket));
		}

		#endregion

		#region TestNonStandardReadOnly

		protected override sealed void TestNonStandardReadOnly1(Func<TPickableDocket, bool> getReadOnly, Func<TPickableDocket, string> getName)
		{
			void additionalAssertions(string name, TPickableDocket docket)
			{
				docket.WD_DocketStatus = DocketStatus.Codes.Entered;
				var pick = Factory.New<WhsPick>();
				docket.Lines.AddNew();
				pick.Orders.Add(docket);

				docket.WD_DocketStatus = DocketStatus.Codes.Finalised;
				AssertEquals($"{name} should not be readonly if docket is finalised and pick is unfinalised", false, getReadOnly(docket));

				pick.WP_PickStatus = PickStatus.Codes.Finalised;
				Env.Security.WhsReleasePostFinaliseEdit.IsAllowed = false;
				AssertEquals($"{name} should be readonly if docket is finalised and pick is finalised", true, getReadOnly(docket));

				Env.Security.WhsReleasePostFinaliseEdit.IsAllowed = true;
				AssertEquals($"{name} should not be readonly if docket is finalised and pick is finalised but user has security for edit", false, getReadOnly(docket));

				TestNonStandardReadOnly1Core(getReadOnly, name, docket);
			}

			TestReadOnly(getReadOnly, getName, false, false, false, true, (_, n, d) => additionalAssertions(n, d));
		}

		protected virtual void TestNonStandardReadOnly1Core(Func<TPickableDocket, bool> getReadOnly, string name, TPickableDocket docket)
		{
		}

		#endregion

		#endregion

		#region Finalisation

		#region TestFinaliseDocketAlwaysFinalisingPick_CleansUpPickOnFailure

		public void TestFinaliseDocketAlwaysFinalisingPick_CleansUpPickOnFailure()
		{
			var docket = GetNewBusinessObject();
			AssertEquals("Precondition", false, docket.IsFinalised);
			AssertNull("Precondition", docket.Pick);

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				docket.FinaliseDocketAlwaysFinalisingPick();
			}
			AssertEquals("Precondition", false, docket.IsFinalised);
			AssertNull("Precondition", docket.Pick);
		}

		#endregion

		#region TestFinaliseDocketAlwaysFinalisingPick_UsesDocketsNotificationSubscriber

		public void TestFinaliseDocketAlwaysFinalisingPick_UsesDocketsNotificationSubscriber()
		{
			var docket = SetupForTestFinaliseDocket();
			docket.Pick.Orders.RemoveAll();
			AssertEquals("Precondition", false, docket.IsFinalised);
			Factory.Save();

			var allocationEngineMock = new Mock<IAllocationEngineManager>(MockBehavior.Strict);
			allocationEngineMock.
				Setup(ae =>
					ae.Allocate(
						It.IsNotNull<WhsPick>(),
						It.Is<INotifications>(notification => notification == docket.NotificationSubscriber),
						It.IsNotNull<IPickStrategy>(),
						It.IsNotNull<IEnumerable<WhsPickOrderedInventory>>()))
				.Returns<WhsPick, INotifications, IPickStrategy, IEnumerable<WhsPickOrderedInventory>>((p, n, ps, ordInv) =>
				{
					new AllocateFIFOLegacyMock().Allocate(p, n, ps, ordInv);
					return AllocationResult.AllocatedStock;
				}).Verifiable();

			using (ObjectFactory.Substitute(allocationEngineMock.Object))
			{
				docket.FinaliseDocketAlwaysFinalisingPick();
			}

			AssertEquals("Should have finalised the docket.", true, docket.IsFinalised);
			allocationEngineMock.VerifyAll();
		}

		#endregion

		public void TestFinaliseDocket_DoNotAddInventoryFetchHint()
		{
			var docket = SetupForTestFinaliseDocket();
			AssertEquals("Precondition", false, docket.IsFinalised);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			otherFactory.RefreshEnabled = false;

			var docketInOtherFactory = otherFactory.Load<WhsDocket>(docket.PK);
			int fetchHintOnInventoryBeforeFinalise = otherFactory.GetLoadedFetchHintCountForTable(WhsInventoryViewSchema.Constants.TableName);
			docketInOtherFactory.FinaliseDocket();

			int fetchHintOnInventoryAfterFinalise = otherFactory.GetLoadedFetchHintCountForTable(WhsInventoryViewSchema.Constants.TableName);
			AssertEquals(fetchHintOnInventoryAfterFinalise, fetchHintOnInventoryBeforeFinalise);
		}

		protected override TPickableDocket SetupForTestFinaliseDocket()
		{
			var result = base.SetupForTestFinaliseDocket();
			result.ConsigneeAddressPK = result.Client.MainAddress.PK;
			return result;
		}

		#endregion

		#region Security

		#region TestIsPostFinalizeEditAllowed

		public override void TestIsPostFinalizeEditAllowed()
		{
			var docket = GetNewBusinessObject();
			Env.Security.WhsReleasePostFinaliseEdit.IsAllowed = false;
			AssertEquals(false, docket.IsPostFinalizeEditAllowed);

			Env.Security.WhsReleasePostFinaliseEdit.IsAllowed = true;
			AssertEquals(true, docket.IsPostFinalizeEditAllowed);
		}

		#endregion

		#endregion

		#region TestIsFulfillmentRuleMet

		public void TestIsFulfillmentRuleMet()
		{
			if (SupportsFullfillmentRule)
			{
				var data = new TestDataForInventory(Factory);
				data.CreateSimpleInventory();
				Docket.WD_OH_Client = data.Org1.PK;
				Docket.WD_WW_Whs = data.Whs1.PK;
				Docket.WD_RequiredDate = ZDateTimeOffset.Now;
				Docket.ConsigneeAddressPK = data.Org1.MainAddress.PK;

				AssertEquals("No rule has been set", true, Docket.IsFulfillmentRuleMet);

				Docket.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
				Helper.CreateWhsPickableDocketLine(Docket, data.Part1, 10);
				Factory.Save();

				Helper.CreatePickNew(Docket);
				int count = Factory.DatabaseLoadCount;
				AssertEquals(true, Docket.IsFulfillmentRuleMet);
				AssertEquals(count, Factory.DatabaseLoadCount);

				Docket.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.None;
				AssertEquals(true, Docket.IsFulfillmentRuleMet);
				AssertEquals(count, Factory.DatabaseLoadCount);
				Docket.Pick.CancelPick();
				Docket.Lines.DeleteAll();

				Docket.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
				Helper.CreateWhsPickableDocketLine(Docket, data.Part1, 101); //only 100 units in inventory
				Helper.CreatePickNew(Docket);

				int count1 = Factory.DatabaseLoadCount;
				AssertEquals(false, Docket.IsFulfillmentRuleMet);
				AssertEquals(count1, Factory.DatabaseLoadCount);

				Docket.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.None;
				AssertEquals(true, Docket.IsFulfillmentRuleMet);
				AssertEquals(count1, Factory.DatabaseLoadCount);
			}
			else
			{
				AssertEquals(true, Docket.IsFulfillmentRuleMet);
			}
		}

		#endregion

		#region TestOnFactorySaving_CreatesUniqueReferenceIfRequiredCore

		protected override void TestOnFactorySaving_CreatesUniqueReferenceIfRequiredCore(OrgHeader client, WhsWarehouse whs)
		{
			// for Empty External Reference
			WhsPickableDocket docket2 = CreateWhsPickableDocket(client, whs, "", true);
			Factory.Save();
			AssertEquals("Docket not saved", true, docket2.IsInDatabase);
			AssertEquals("Reference incorrect", "W00000002", docket2.WD_ExternalReference);

			// for not empty but External Reference already used by client on same type of docket
			WhsPickableDocket docket3 = CreateWhsPickableDocket(client, whs, "R1", true);
			Factory.Save();
			AssertEquals("Docket not saved", true, docket3.IsInDatabase);
			AssertEquals("Reference incorrect", "R1 W00000003", docket3.WD_ExternalReference);

			// for not empty but External Reference already used by client on another type of docket
			WhsReceive receive = Helper.CreateWhsReceive(client, whs, "REC1");
			Factory.Save();

			WhsPickableDocket docket4 = CreateWhsPickableDocket(client, whs, "REC1", true);
			Factory.Save();
			AssertEquals("Docket not saved", true, docket4.IsInDatabase);
			AssertEquals("Reference incorrect", "REC1", docket4.WD_ExternalReference);

			// for not empty and not used External Reference
			WhsPickableDocket docket5 = CreateWhsPickableDocket(client, whs, "R2", true);
			Factory.Save();
			AssertEquals("Docket not saved", true, docket5.IsInDatabase);
			AssertEquals("Reference incorrect", "R2", docket5.WD_ExternalReference);

			// for not empty with External Reference used by another client
			OrgHeader client1 = Helper.CreateClient("Client1");
			WhsPickableDocket docketForAnotherClient = CreateWhsPickableDocket(client1, whs, "A123", false);
			Factory.Save();
			AssertEquals("Pre-Condition", "A123", docketForAnotherClient.WD_ExternalReference);
			AssertEquals("Pre-Condition", true, docketForAnotherClient.IsInDatabase);

			WhsPickableDocket docket6 = CreateWhsPickableDocket(client, whs, "A123", true);
			Factory.Save();
			AssertEquals("Docket not saved", true, docket6.IsInDatabase);
			AssertEquals("Reference incorrect", "A123", docket6.WD_ExternalReference);
		}

		WhsPickableDocket CreateWhsPickableDocket(OrgHeader client, WhsWarehouse warehouse, ZString externalReference, bool isRefCreatedOnSave)
		{
			return CreateWhsDocket(client, warehouse, externalReference, isRefCreatedOnSave);
		}

		#endregion

		#region TestIsAttachingToPick

		public void TestIsAttachingToPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewBusinessObject();
			AssertEquals("semaphore is null attaching to pick should be false", false, docket.IsAttachingToPick);
			using (docket.SetIsAttachingToPick())
			{
				AssertEquals("semaphore is suspended, attaching to pick should be true", true, docket.IsAttachingToPick);
			}
			AssertEquals("semaphore is not suspended, attaching to pick should be false", false, docket.IsAttachingToPick);
		}

		#endregion

		#region TestAddFetchHintsForPickLinesFactoryCachedValue

		public void TestAddFetchHintsForPickLinesFactoryCachedValue()
		{
			var docket = GetNewBusinessObject();
			var cacheKey = $"WhsPickableDocket|AddFetchHintsForPickLines|{docket.PK}";

			Factory.ClearCachedValue<bool>(cacheKey);
			docket.AddFetchHintsForPickLines();
			AssertEquals("this key has been already added to factory (when we call add fetch hint action) and it should return true", true, Factory.GetCachedValue(cacheKey, () => false));
		}

		#endregion

		#region TestUpdateWP_CriticalChangesVersionID

		public void TestUpdateWP_CriticalChangesVersionID_Attach()
		{
			var (pickableDocket, pick) = CreatePickableDocketAndPick(attachOrderToPick: false);

			var currentPickVersionInDB = NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID;

			pick.WP_WW_Whs = pickableDocket.WD_WW_Whs;
			pickableDocket.WD_WP = pick.PK;
			pickableDocket.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			Factory.Save();

			AssertPickVersionUpdatedInDB(pick, currentPickVersionInDB);
		}

		public void TestUpdateWP_CriticalChangesVersionID_Detach()
		{
			var (pickableDocket, pick) = CreatePickableDocketAndPick(attachOrderToPick: true);

			var currentPickVersionInDB = NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID;
			pick.Orders.Remove(pickableDocket);
			pickableDocket.Factory.Save();

			var pickInNewFactory = NewFactory().Load<WhsPick>(pick.PK);
			AssertNotEquals("Should update pick version.", currentPickVersionInDB, pickInNewFactory.WP_CriticalChangesVersionID);
			AssertEquals("Pick version should be valid.", true, pickInNewFactory.WP_CriticalChangesVersionID.IsValid);
		}

		public void TestUpdateWP_CriticalChangesVersionID_AttachAndDetach()
		{
			var (pickableDocket, oldPick) = CreatePickableDocketAndPick(attachOrderToPick: true);
			var newPick = Helper.CreatePickNew();
			newPick.WP_WW_Whs = pickableDocket.WD_WW_Whs;
			Factory.Save();

			var oldPickInDB = NewFactory().Load<WhsPick>(oldPick.PK).WP_CriticalChangesVersionID;
			var newPickInDB = NewFactory().Load<WhsPick>(newPick.PK).WP_CriticalChangesVersionID;
			pickableDocket.WD_WP = newPick.PK;
			pickableDocket.Factory.Save();

			AssertNotEquals("Should update pick version (order detached).", oldPickInDB, NewFactory().Load<WhsPick>(oldPick.PK).WP_CriticalChangesVersionID);
			AssertNotEquals("Should update pick version.(order attached).", newPickInDB, NewFactory().Load<WhsPick>(newPick.PK).WP_CriticalChangesVersionID);
		}

		public void TestUpdateWP_CriticalChangesVersionID_PickableDocketChildChange()
		{
			var (pickableDocket, pick) = CreatePickableDocketAndPick(attachOrderToPick: true);
			var line = pickableDocket.Lines.AddNew();
			line.WE_CustomAttrib1 = "1";
			var product = Helper.CreateProduct(pickableDocket.Client, "Product");
			line.WE_OP = product.PK;
			Factory.Save();

			var currentPickVersionInDB = NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID;
			line.WE_CustomAttrib1 = "2";
			Factory.Save();

			AssertPickVersionUpdatedInDB(pick, currentPickVersionInDB);
		}

		#region TestUpdateWP_CriticalChangesVersionID_ExceptionFields

		public void TestUpdateWP_CriticalChangesVersionID_AllFieldsHandled()
		{
			var ignoredColumns = new SchemaColumn[]
			{
				WhsDocketSchema.WD_AddPalletWeightToOrder,
				WhsDocketSchema.WD_BookedWithCBADateTimeUtc,
				WhsDocketSchema.WD_BookingDate,
				WhsDocketSchema.WD_CODPayMethod,
				WhsDocketSchema.WD_ContainerMode,
				WhsDocketSchema.WD_CubicSent,
				WhsDocketSchema.WD_CustomAttrib1,
				WhsDocketSchema.WD_CustomAttrib2,
				WhsDocketSchema.WD_CustomAttrib3,
				WhsDocketSchema.WD_CustomAttrib4,
				WhsDocketSchema.WD_CustomAttrib5,
				WhsDocketSchema.WD_CustomDate1,
				WhsDocketSchema.WD_CustomDate2,
				WhsDocketSchema.WD_CustomDecimal1,
				WhsDocketSchema.WD_CustomDecimal2,
				WhsDocketSchema.WD_CustomDecimal3,
				WhsDocketSchema.WD_CustomDecimal4,
				WhsDocketSchema.WD_CustomDecimal5,
				WhsDocketSchema.WD_CustomerReference,
				WhsDocketSchema.WD_CustomFlag1,
				WhsDocketSchema.WD_CustomFlag2,
				WhsDocketSchema.WD_CustomFlag3,
				WhsDocketSchema.WD_CustomFlag4,
				WhsDocketSchema.WD_CustomFlag5,
				WhsDocketSchema.WD_DropMode,
				WhsDocketSchema.WD_ExternalReference,
				WhsDocketSchema.WD_ExternalReferenceSplit,
				WhsDocketSchema.WD_ExWhsJobGuid,
				WhsDocketSchema.WD_F3_NKTotalPackType,
				WhsDocketSchema.WD_GoodsDescription,
				WhsDocketSchema.WD_INCO,
				WhsDocketSchema.WD_IsAuthorisedToLeave,
				WhsDocketSchema.WD_LocalCartInsuranceCost,
				WhsDocketSchema.WD_OH_Forwarder,
				WhsDocketSchema.WD_PackagesSent,
				WhsDocketSchema.WD_PalletsSent,
				WhsDocketSchema.WD_PickPriority,
				WhsDocketSchema.WD_PL_NKCarrierServiceLevel,
				WhsDocketSchema.WD_RequiredDate,
				WhsDocketSchema.WD_RS_NKServiceLevel,
				WhsDocketSchema.WD_RX_NKTotalOrderCurrency,
				WhsDocketSchema.WD_ShipperCODAmount,
				WhsDocketSchema.WD_SystemCreateBranch,
				WhsDocketSchema.WD_SystemCreateDepartment,
				WhsDocketSchema.WD_SystemCreateTimeUtc,
				WhsDocketSchema.WD_SystemCreateUser,
				WhsDocketSchema.WD_SystemLastEditTimeUtc,
				WhsDocketSchema.WD_SystemLastEditUser,
				WhsDocketSchema.WD_TotalCubic,
				WhsDocketSchema.WD_TotalCubicUnit,
				WhsDocketSchema.WD_TotalOrderValue,
				WhsDocketSchema.WD_TotalPallets,
				WhsDocketSchema.WD_TotalUnits,
				WhsDocketSchema.WD_TotalWeight,
				WhsDocketSchema.WD_TotalWeightUnit,
				WhsDocketSchema.WD_TransportMode,
				WhsDocketSchema.WD_TransportReference,
				WhsDocketSchema.WD_TZ_TransportZone,
				WhsDocketSchema.WD_UnitsSent,
				WhsDocketSchema.WD_WD_ParentDocket,
				WhsDocketSchema.WD_WeightSent,
				WhsDocketSchema.WD_WeightSentUserEntered,
				WhsDocketSchema.WD_WeightVolSetFromImport,
			};

			var notUsedByPickableDocket = new SchemaColumn[]
			{
				WhsDocketSchema.WD_ArrivalDate,
				WhsDocketSchema.WD_CriticalChangesVersionID,
				WhsDocketSchema.WD_ETA,
				WhsDocketSchema.WD_ETD,
				WhsDocketSchema.WD_FirstScannedInboundDockDoorUtc,
				WhsDocketSchema.WD_HoldPalletIDPutaway,
				WhsDocketSchema.WD_IsPickFaceReplenishment,
				WhsDocketSchema.WD_IsPutawayTransfer,
				WhsDocketSchema.WD_ReceiveCategory,
				WhsDocketSchema.WD_StartedReceivingTimeUtc,
				WhsDocketSchema.WD_TaskPlanningStatus,
				WhsDocketSchema.WD_UnloadCompletedTime,
				WhsDocketSchema.WD_WD_Split,
				WhsDocketSchema.WD_WL_InboundDockDoor,
				WhsDocketSchema.WD_WP_ParentPickForReceive,
				WhsDocketSchema.WD_WP_ParentPickForTransfer,
				WhsDocketSchema.WD_WP_PickBeingReplenished,
			};

			var doNotChangeInPractice = new SchemaColumn[]
			{
				WhsDocketSchema.WD_DocketID,
				WhsDocketSchema.WD_DocketType,
				WhsDocketSchema.PK,
			};

			var columnsThatNeedToBeChecked = new SchemaColumn[]
			{
				WhsDocketSchema.WD_AutoFinaliseBOMIntoInventory,
				WhsDocketSchema.WD_CanceledTimeUtc,
				WhsDocketSchema.WD_CustomsParentReference,
				WhsDocketSchema.WD_DocketStatus,
				WhsDocketSchema.WD_DocketSubType,
				WhsDocketSchema.WD_ExcludeFromTotePicking,
				WhsDocketSchema.WD_FinalisedDate,
				WhsDocketSchema.WD_GS_NKAssignedPacker,
				WhsDocketSchema.WD_GS_NKCanceledBy,
				WhsDocketSchema.WD_GS_NKFinalizedBy,
				WhsDocketSchema.WD_IsInwardsProcessingJob,
				WhsDocketSchema.WD_IsLoadingRequired,
				WhsDocketSchema.WD_OH_Client,
				WhsDocketSchema.WD_OrderClassification,
				WhsDocketSchema.WD_PackingAfterPickingRequired,
				WhsDocketSchema.WD_PickOption,
				WhsDocketSchema.WD_P9_PackingTask,
				WhsDocketSchema.WD_QualityAuditRequired,
				WhsDocketSchema.WD_ScreeningStatus,
				WhsDocketSchema.WD_UseDirectedPackingConsolidation,
				WhsDocketSchema.WD_WP,
				WhsDocketSchema.WD_WhsOrderFulfillmentRule,
				WhsDocketSchema.WD_WLO_PlannedLoad,
				WhsDocketSchema.WD_WL_CrossDock,
				WhsDocketSchema.WD_WSH_SalesChannel,
				WhsDocketSchema.WD_WW_Whs,
			};

			AssertContainsExactElementsInAnyOrder("Make sure all columns for Pickable Docket are considered on whether or not they should update their corresponding Pick's Critical Changes Version ID (Check OnSaving Logic).",
				column => column.Name,
				WhsDocketSchema.All,
				ignoredColumns.Concat(notUsedByPickableDocket).Concat(doNotChangeInPractice).Concat(columnsThatNeedToBeChecked));
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_AllFieldsExceptOnesWeCheckAndThatCauseConstraintFailures()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) =>
			{
				var columnsThatBumpCriticalChangesVersionID = new SchemaColumn[]
				{
					WhsDocketSchema.WD_AutoFinaliseBOMIntoInventory,
					WhsDocketSchema.WD_CustomsParentReference,
					WhsDocketSchema.WD_DocketStatus,
					WhsDocketSchema.WD_WP,
					WhsDocketSchema.WD_ExcludeFromTotePicking,
					WhsDocketSchema.WD_GS_NKAssignedPacker,
					WhsDocketSchema.WD_IsInwardsProcessingJob,
					WhsDocketSchema.WD_IsLoadingRequired,
					WhsDocketSchema.WD_FinalisedDate,
					WhsDocketSchema.WD_OrderClassification,
					WhsDocketSchema.WD_PickOption,
					WhsDocketSchema.WD_PackingAfterPickingRequired,
					WhsDocketSchema.WD_P9_PackingTask,
					WhsDocketSchema.WD_ScreeningStatus,
					WhsDocketSchema.WD_QualityAuditRequired,
					WhsDocketSchema.WD_UseDirectedPackingConsolidation,
					WhsDocketSchema.WD_WL_CrossDock,
					WhsDocketSchema.WD_WL_InboundDockDoor,
					WhsDocketSchema.WD_WhsOrderFulfillmentRule,
					WhsDocketSchema.WD_WLO_PlannedLoad,
					WhsDocketSchema.WD_WSH_SalesChannel,
				};

				var columnsThatCannotBeSetDueToTriggersOrConstraints = new List<SchemaColumn>
				{
					WhsDocketSchema.WD_DocketType,
					WhsDocketSchema.WD_WD_ParentDocket,
					WhsDocketSchema.PK,
					WhsDocketSchema.WD_WD_Split,
					WhsDocketSchema.WD_WP_ParentPickForReceive,
					WhsDocketSchema.WD_WP_ParentPickForTransfer,
					WhsDocketSchema.WD_WP_PickBeingReplenished,
					WhsDocketSchema.WD_HoldPalletIDPutaway,
					WhsDocketSchema.WD_IsPickFaceReplenishment,
					WhsDocketSchema.WD_IsPutawayTransfer,
					WhsDocketSchema.WD_ReceiveCategory,
					WhsDocketSchema.WD_StartedReceivingTimeUtc,
					WhsDocketSchema.WD_CanceledTimeUtc,
					WhsDocketSchema.WD_DocketSubType,
					WhsDocketSchema.WD_GS_NKCanceledBy,
					WhsDocketSchema.WD_GS_NKFinalizedBy,
					WhsDocketSchema.WD_OH_Client,
					WhsDocketSchema.WD_TaskPlanningStatus,
					WhsDocketSchema.WD_WW_Whs,
					WhsDocketSchema.WD_UnloadCompletedTime,
					WhsDocketSchema.WD_FirstScannedInboundDockDoorUtc,
				};

				if (docket is WhsComponentOrder)
				{
					columnsThatCannotBeSetDueToTriggersOrConstraints.Add(WhsDocketSchema.WD_BookedWithCBADateTimeUtc);
				}

				var allExcludedColumns = new HashSet<SchemaColumn>(columnsThatBumpCriticalChangesVersionID.Concat(columnsThatCannotBeSetDueToTriggersOrConstraints));

				var typeToValueDictionary = new Dictionary<Type, IZType>()
				{
					{ typeof(ZString), (ZString)"AAA" },
					{ typeof(ZDecimal), (ZDecimal)10.4m },
					{ typeof(ZByte), (ZByte)2 },
					{ typeof(ZShort), (ZShort)9 },
					{ typeof(ZInt), (ZInt)11 },
					{ typeof(ZDateTime), ZDateTime.Now },
					{ typeof(ZDateTimeOffset), ZDateTimeOffset.Now },
					{ typeof(ZBool), ZBool.True },
					{ typeof(ZGuid), ZGuid.NewZGuid() },
				};

				foreach (var column in WhsDocketSchema.All.Where(s => !allExcludedColumns.Contains(s)))
				{
					if (column.Name == WhsDocketSchema.Constants.WD_OH_Forwarder)
					{
						docket.WD_OH_Forwarder = Helper.CreateClient("Forwarder").PK;
					}
					else if (column.Name == WhsDocketSchema.Constants.WD_TotalCubicUnit)
					{
						docket.WD_TotalCubicUnit = Constants.Volume.TeaChest;
					}
					else if (column.Name == WhsDocketSchema.Constants.WD_TotalWeightUnit)
					{
						docket.WD_TotalWeightUnit = Constants.Weight.Ounces;
					}
					else if (column.Name == WhsDocketSchema.Constants.WD_TZ_TransportZone)
					{
						var transportProvider = Helper.SetUpRateTransportProvider(true, "ALL", "AU", "ALL", docket.Client);
						docket.WD_TZ_TransportZone = Helper.SetUpRateTransportZone(transportProvider, true, "ZONE").PK;
					}
					else if (typeToValueDictionary.TryGetValue(column.GetEquivalentZType(), out var value))
					{
						docket[column] = value;
					}
					else
					{
						throw new NotImplementedException($"Unhandled column: {column.Name}.");
					}
				}
			},
			false);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_WD_AutoFinaliseBOMIntoInventory()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) => docket.WD_AutoFinaliseBOMIntoInventory = docket is not WhsDynamicWorkOrder, true);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_WD_CustomsParentReference()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) => docket.WD_CustomsParentReference = "AAA", true);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_WD_DocketStatus()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) => docket.WD_DocketStatus = "PIC", true);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_WD_FinalisedDate()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) =>
			{
				docket.WD_FinalisedDate = ZDateTimeOffset.Now;
				docket.WD_GS_NKFinalizedBy = "AAA";

				if (docket is WhsComponentOrder)
				{
					docket.WD_DocketStatus = "FIN";
				}
			}, true);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_WD_PickOption()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) => docket.WD_PickOption = "MAN", true);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_WD_QualityAuditRequired()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) => docket.WD_QualityAuditRequired = true, true);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_WD_WL_CrossDock()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) => docket.WD_WL_CrossDock = docket.Warehouse.WW_DefaultOutboundDockDoor, true);
		}

		protected void AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore(Action<WhsPickableDocket> editOrder, bool expectedToUpdateVersion, Func<(WhsPickableDocket, WhsPick)> getDocketOverride = null)
		{
			var (pickableDocket, pick) = getDocketOverride?.Invoke() ?? CreatePickableDocketAndPick(attachOrderToPick: true);

			var currentPickVersionInDB = NewFactory().Load<WhsPick>(pick.PK).WP_CriticalChangesVersionID;
			editOrder(pickableDocket);
			pickableDocket.Factory.Save();

			var pickInNewFactory = NewFactory().Load<WhsPick>(pick.PK);
			if (expectedToUpdateVersion)
			{
				AssertNotEquals("Should update pick version.", currentPickVersionInDB, pickInNewFactory.WP_CriticalChangesVersionID);
				AssertEquals("Pick version should be valid.", true, pickInNewFactory.WP_CriticalChangesVersionID.IsValid);
			}
			else
			{
				AssertEquals("Should not update pick version.", currentPickVersionInDB, pickInNewFactory.WP_CriticalChangesVersionID);
			}
		}

		#endregion

		(WhsPickableDocket pickableDocket, WhsPick pick) CreatePickableDocketAndPick(bool attachOrderToPick)
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.BOM.BikeWheel, 2m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.BOM.BikeEngine, 1m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.BOM.Polish, 1m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			// order some bikes
			var docket = GetPickableDocket_ForLoadPickLinesForAllDocketLinesTest(data);
			WhsPick pick;
			if (attachOrderToPick)
			{
				pick = Helper.CreatePickNew(docket);
				AssertEquals("Precondition - Pick is not picking.", true, docket.IsAttachedToPickButNotFinalised);
			}
			else
			{
				pick = Factory.New<WhsPick>();
			}
			Factory.Save();

			return (docket, pick);
		}

		void AssertPickVersionUpdatedInDB(WhsPick pick, ZGuid currentPickVersionInDB)
		{
			var pickInNewFactory = NewFactory().Load<WhsPick>(pick.PK);
			AssertNotEquals("Should update pick version.", currentPickVersionInDB, pickInNewFactory.WP_CriticalChangesVersionID);
			AssertEquals("Pick version should be valid.", true, pickInNewFactory.WP_CriticalChangesVersionID.IsValid);
		}

		#endregion

		#region TestDataRefresh

		#region TestDataRefresh_CancelledPick

		public void TestDataRefresh_CancelledPick()
		{
			if (!Globals.IsWeb)
			{
				// create unfinalised order and pick
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
				var pick = Helper.CreatePickNew(order);
				Factory.Save();

				// cancel pick in another factory - do *NOT* save that factory
				var otherFactoryForPick = new BusinessObjectFactory();
				var pickInOtherFactory = otherFactoryForPick.Load<WhsPick>(pick.PK);
				pickInOtherFactory.CancelPick();

				// modify order in other Factory and Save.
				var otherFactoryForOrder = new BusinessObjectFactory();
				var orderInOtherFactory = otherFactoryForPick.Load<WhsOrder>(order.PK);
				orderInOtherFactory.WD_SystemLastEditTimeUtc = ZDateTime.UtcNow;
				otherFactoryForOrder.Save(); // data refresh bus is overwriting the CAN status on the canceled order in "otherFactory". bad.
											 // ensure the order is still in picking status, since cancelation in another factory was not saved yet.
				var assertFactory = new BusinessObjectFactory();
				var orderInAssertFactory = assertFactory.Load<WhsPickableDocket>(order.PK);
				AssertEquals("Order should have Atached to Pick status since cancellation was not saved yet.", DocketStatus.Codes.AttachedToPick, orderInAssertFactory.WD_DocketStatus);
				AssertEquals("Order should be attached to the pick since cancellation was not saved yet.", pick.PK, orderInAssertFactory.WD_WP);

				// save canceled pick in other factory
				otherFactoryForPick.Save();
				// ensure the order is canceled in the DB
				var assertFactory2 = new BusinessObjectFactory();
				var orderInAssertFactory2 = assertFactory2.Load<WhsPickableDocket>(order.PK);
				AssertEquals("Order status should be retained from the factory where the pick was cancelled.", DocketStatus.Codes.Entered, orderInAssertFactory2.WD_DocketStatus);
				AssertEquals("Order should not be connected to the pick since it was cancelled.", ZGuid.Empty, orderInAssertFactory2.WD_WP);
			}
			else
			{
				// DataRefreshManager is not Enabled when Globals.IsWeb = true
				Assert(true);
			}
		}

		#endregion

		#region TestCancelledPickNo

		public void TestCancelledPickNo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("CancelledPickNo should be Empty string.", ZString.Empty, pick.Orders[0].CancelledPickNo);

			pick.CancelPick();
			AssertEquals("CancelledPickNo should contain the PickNO.", pick.WP_PickNo, order.CancelledPickNo);
		}

		#endregion

		#region TestDataRefresh_FinalisedPick

		public void TestDataRefresh_FinalisedPick()
		{
			if (!Globals.IsWeb)
			{
				// create unfinalised order and pick
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
				var pick = Helper.CreatePickNew(order);
				Factory.Save();

				// finalised order and pick in another factory - do *NOT* save that factory
				var otherFactory = new BusinessObjectFactory();
				var orderInOtherFactory = otherFactory.Load<WhsPickableDocket>(order.PK);
				var pickInOtherFactory = otherFactory.Load<WhsPick>(pick.PK);
				pickInOtherFactory.FinaliseAllOrders();
				AssertIsFinalisedPrecondition(orderInOtherFactory);

				// modify order in original Factory and Save.
				// will bump Pick version because of Cartonised check on the Pick
				order.WD_ExternalReference = "NEW_REF";

				// data refresh bus is overwriting the FIN status on the finalised order in "otherFactory". bad.
				Factory.Save();

				// ensure the order is still in picking status, since finalisation in another factory was not saved yet.
				var assertFactory = new BusinessObjectFactory();
				var orderInAssertFactory = assertFactory.Load<WhsPickableDocket>(order.PK);
				AssertEquals("Order should not be finalised, since finalisation was not saved yet.", false, orderInAssertFactory.IsFinalised);
				AssertEquals("Pick should not be finalised, since finalisation was not saved yet.", false, orderInAssertFactory.Pick.IsFinalised);

				// save finalised order in other factory
				otherFactory.Save();
				// ensure the order is finalised in the DB
				var assertFactory2 = new BusinessObjectFactory();
				var orderInAssertFactory2 = assertFactory2.Load<WhsPickableDocket>(order.PK);
				AssertEquals("Order should be retained from the factory where the pick was finalised.", true, orderInAssertFactory2.IsFinalised);

				// save original factory second time, ensure that it will not override status again.
				Factory.Save();
				var assertFactory3 = new BusinessObjectFactory();
				var orderInAssertFactory3 = assertFactory3.Load<WhsPickableDocket>(order.PK);
				AssertEquals("Order should be retained from the factory where the pick was finalised.", true, orderInAssertFactory3.IsFinalised);

				// try to finalise pick to make sure its status is retained as well
				pickInOtherFactory.FinalisePick();
				AssertIsFinalisedPrecondition(pickInOtherFactory);

				var finalisedTime = pickInOtherFactory.WP_FinalizedDateUtc;
				var finalisedBy = pickInOtherFactory.WP_GS_NKFinalizedBy;
				// make sure critical changes is update to date.
				pick.WP_CriticalChangesVersionID = (ZGuid)pickInOtherFactory.WP_CriticalChangesVersionIDInfo.OriginalValue;
				pick.WP_SystemLastEditTimeUtc = ZDateTime.UtcNow.AddMinutes(1); // make the pick have changes to propagate data refresh

				// data refresh bus is overwriting the FIN status on the finalised pick in "otherFactory". bad.
				Factory.Save();

				// ensure the pick is still not finalised, since finalisation in another factory was not saved yet.
				var assertFactory4 = new BusinessObjectFactory();
				var pickInAssertFactory4 = assertFactory4.Load<WhsPick>(pick.PK);
				AssertEquals("Pick should not be finalised, since finalisation was not saved yet.", false, pickInAssertFactory4.IsFinalised);

				// save the pick that was finalised.
				otherFactory.Save();

				var assertFactory5 = new BusinessObjectFactory();
				var pickInAssertFactory5 = assertFactory5.Load<WhsPick>(pick.PK);
				AssertEquals("Pick should be finalised, since finalisation was saved.", true, pickInAssertFactory5.IsFinalised);
				AssertEquals("Pick finalised time should be correct.", finalisedTime.ToSmallDateTimeFloor(), pickInAssertFactory5.WP_FinalizedDateUtc);
				AssertEquals("Pick finalised by should be correct.", finalisedBy, pickInAssertFactory5.WP_GS_NKFinalizedBy);
				AssertEquals("Pick Critical Changes Version ID should be empty.", ZGuid.Empty, pickInAssertFactory5.WP_CriticalChangesVersionID);
			}
			else
			{
				// DataRefreshManager is not Enabled when Globals.IsWeb = true
				Assert(true);
			}
		}

		#endregion

		#endregion

		#region CreateNewPick

		protected override WhsPick CreateNewPick(TPickableDocket docket, WhsWarehouse warehouse)
		{
			var pick = Helper.CreatePickNew();
			pick.WP_WW_Whs = warehouse.PK;
			docket.WD_WP = pick.PK;
			docket.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			return pick;
		}

		#endregion

		#region TestAcceptInventoryLinesFromSearchGrid

		protected override void AssertDocketLineEqualsInventory(WhsInventoryView inventory, WhsDocketLine line)
		{
			base.AssertDocketLineEqualsInventory(inventory, line);
			AssertEquals("WE_TransactionQuantity", inventory.WI_AvailableToPickQuantity, line.WE_TransactionQuantity);
		}

		#endregion

		#region TestHasDangerousGoodsFlag

		public void TestHasDangerousGoodsFlag_HasDangerousGoods()
		{
			TestHasDangerousGoodsFlag_Core(true);
		}

		public void TestHasDangerousGoodsFlag_HasNoDangerousGoods()
		{
			TestHasDangerousGoodsFlag_Core(false);
		}

		public void TestHasDangerousGoodsFlag_Core(bool hasDangerousGoods)
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var part1 = Helper.CreateProduct(data.Org1, "PRODUCT1");
			var part2 = Helper.CreateProduct(data.Org1, "PRODUCT2");
			Helper.CreateWhsOrderLine(order2, part1, 10m);
			Helper.CreateWhsOrderLine(order2, part2, 10m);

			if (hasDangerousGoods)
			{
				part1.UNDGs.AddNew();
			}

			AssertEquals(false, order1.HasDangerousGoods);
			AssertEquals(hasDangerousGoods, order2.HasDangerousGoods);
		}

		#endregion

		#region ISendEmailSource Members

		protected override void SetupConsigneeForAddressBookSelection(TPickableDocket docket, OrgHeader consignee)
		{
			base.SetupConsigneeForAddressBookSelection(docket, consignee);
			docket.ConsigneePK = consignee.PK;
		}

		#endregion

		#region	IWhsJobTemplateCopyable Members

		public void TestTemplateCopy_Base()
		{
			TestTemplateCopyCore(docket => (TPickableDocket)((ITemplateCopyable)docket).TemplateCopy(), shouldHaveLines: true);
		}

		public void TestTemplateCopy_WithoutLines()
		{
			TestTemplateCopyCore(docket => (TPickableDocket)((IWhsJobTemplateCopyable)docket).TemplateCopyWithoutLines(), shouldHaveLines: false);
		}

		void TestTemplateCopyCore(Func<TPickableDocket, TPickableDocket> copyAction, bool shouldHaveLines)
		{
			var docket = GetNewDocketForTemplateCopy();

			var client = docket.Client;
			client.MiscServ.OM_IMDefaultINCOTerm = "FOB";
			client.MainAddress.OA_RN_NKCountryCode = "AU";
			client.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			docket.WD_INCO = "C3P";

			docket.WD_DocketID = "W0000001";
			docket.WD_DocketStatus = DocketStatus.Codes.Finalised;
			docket.WD_ExternalReference = "Ext Ref.";
			docket.WD_TransportReference = "TR1";
			docket.WD_FinalisedDate = ZDateTimeOffset.Today.AddDays(1);
			docket.WD_WL_CrossDock = Factory.New<WhsLocation>().PK;
			docket.Containers.AddNew().WC_ContainerNum = "C1";
			docket.Pallets.AddNew();

			docket.WD_UnitsSent = 1;
			docket.WD_PackagesSent = 2;
			docket.WD_PalletsSent = 3;
			docket.WD_WeightSent = 4;
			docket.WD_CubicSent = 5;
			docket.WD_WeightSentUserEntered = 6;
			docket.WD_OrderClassification = OrderClassification.Codes.Bulk;

			// the actual copy
			var copy = copyAction(docket);

			AssertEquals("", copy.WD_DocketID);
			AssertEquals("Ext Ref.", copy.WD_ExternalReference);
			AssertEquals(DocketStatus.Codes.New, copy.WD_DocketStatus);
			AssertEquals("", copy.WD_TransportReference);
			AssertEquals(ZDateTimeOffset.Empty, copy.WD_FinalisedDate);
			AssertEquals(ZGuid.Empty, copy.WD_WP);
			AssertEquals(ZGuid.Empty, copy.WD_WL_CrossDock);
			AssertEquals(0, copy.Containers.Count);
			AssertEquals(0, copy.Pallets.Count);

			AssertEquals(docket.WD_OH_Client, copy.WD_OH_Client);
			AssertEquals(docket.WD_WW_Whs, copy.WD_WW_Whs);
			AssertEquals(docket.ConsigneePK, copy.ConsigneePK);
			Assert(copy.Lookups.INCOTerms.ContainsCode(copy.WD_INCO));
			AssertEquals("C3P", copy.WD_INCO);

			// fields that should not be copied
			AssertNotEquals("Precondition", 0, docket.WD_UnitsSent);
			AssertEquals(0m, copy.WD_UnitsSent);

			AssertNotEquals("Precondition", 0, docket.WD_PackagesSent);
			AssertEquals(0, copy.WD_PackagesSent);

			AssertNotEquals("Precondition", (ZShort)0, docket.WD_PalletsSent);
			AssertEquals((ZShort)0, copy.WD_PalletsSent);

			AssertNotEquals("Precondition", 0, docket.WD_WeightSent);
			AssertEquals(0m, copy.WD_WeightSent);

			AssertNotEquals("Precondition", 0, docket.WD_CubicSent);
			AssertEquals(0m, copy.WD_CubicSent);

			AssertNotEquals("Precondition", 0, docket.WD_WeightSentUserEntered);
			AssertEquals(0m, copy.WD_WeightSentUserEntered);

			AssertEquals("Precondition", OrderClassification.Codes.Bulk, docket.WD_OrderClassification);
			AssertEquals(string.Empty, copy.WD_OrderClassification);

			// anything sub-classes wish to assert...
			AssertTemplateCopy(docket, copy, shouldHaveLines);
		}

		protected virtual void AssertTemplateCopy(TPickableDocket source, TPickableDocket copy, bool shouldHaveLines)
		{
		}

		protected abstract TPickableDocket GetNewDocketForTemplateCopy();

		#endregion

		#region Implementation

		protected override void AssertLocations(WhsDocketLine line, WhsInventoryView inventory)
		{
			// locations are not used by pickable dockets
		}

		protected WhsWorkOrder FindWorkOrder(OrgSupplierPart product, WhsWorkOrder[] workOrders)
		{
			foreach (WhsWorkOrder workOrder in workOrders)
			{
				if (workOrder.Lines[0].SupplierPart == product)
				{
					return workOrder;
				}
			}
			return null;
		}

		#endregion
	}
}
