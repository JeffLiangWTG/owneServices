using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.Warehouse;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportBookings.Shared.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using GlowRegistry = Enterprise.Registry.Business.GlowRegistry;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	#region WhsReceiveTest

	[TestedType(typeof(WhsReceive))]
	public class WhsReceiveTest : WhsDocketTestCase<WhsReceive>
	{
		#region ICustomizableNumberFountainConsumer

		public void TestICustomizableNumberFountainConsumer_SavingSetsReceiveID()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Factory.Save();

			var docket = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			docket.WD_ExternalReference = "";

			AssertEquals("Precondition: Docket ID is empty.", "", docket.WD_DocketID);

			Factory.Save();
			AssertEquals("Docket ID was set correctly.", "W00000001", docket.WD_DocketID);
			AssertEquals("External Reference was set correctly.", "W00000001", docket.WD_ExternalReference);

			var customisations = new BillOfLadingNumberCustomisationsByServiceLevel();
			customisations.Categories |= NumberCustomisationElementCategories.WarehouseJob | NumberCustomisationElementCategories.WarehouseReceive;
			var allCustomisation = customisations.BillOfLadingNumberCustomisations[OrgCarrierServiceLevel.AllCode];
			foreach (BillOfLadingNumberCustomisationElement element in allCustomisation.Elements)
			{
				switch (element.Key)
				{
					case BillOfLadingNumberCustomisationElement.Keys.SequenceNumber:
						element.Include = true;
						element.Detail = "6";
						element.Order = 50;
						break;
					case BillOfLadingNumberCustomisationElement.Keys.BranchCode:
						element.Include = true;
						element.Order = 2;
						break;
					case BillOfLadingNumberCustomisationElement.Keys.CompanyCode:
						element.Include = true;
						element.Order = 1;
						break;
					case BillOfLadingNumberCustomisationElement.Keys.WarehouseReceiveCategoryCode:
						element.Include = true;
						element.Order = 3;
						break;
					default:
						element.Include = false;
						break;
				}
			}
			WarehouseDataRegistry.Instance.WarehouseNumberCustomisation_Receive.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisations);

			var docket2 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			docket2.WD_ReceiveCategory = "RC1";
			docket2.WD_ExternalReference = "";

			Factory.Save();
			AssertEquals("Docket ID was set correctly.", "WEDIBNERC1000002", docket2.WD_DocketID);
			AssertEquals("External Reference was set correctly.", "WEDIBNERC1000002", docket2.WD_ExternalReference);
		}

		#endregion

		#region Constructor

		public void TestConstructor_SetConcurrencyPolicy()
		{
			var receive = Factory.New<WhsReceive>();
			AssertEquals("Concurrency Policy should be strict for WD_FinalisedDate.", ConcurrencyPolicy.Strict, receive.WD_FinalisedDateInfo.ConcurrencyPolicy);
		}

		#endregion

		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.WhsReceive);
			}
		}

		#endregion

		#region Performance Tests

		#region TestPerformanceOfRunPreSaveValidation_WithLocationSOHValidation

		[StressTest]
		public void TestPerformanceOfRunPreSaveValidation_WithLocationSOHValidation()
		{
			const int NumberOfInventoriesToCreate = 300; // Inventory Lines to create x2

			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);

			var location = data.Whs1.DefaultLocation;
			for (int i = 0; i < NumberOfInventoriesToCreate; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, location);
				Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, location);
			}

			Factory.Save();

			WarehouseDataRegistry.Instance.SOHLocationWarning.SetValue(Env.CurrentCompany.PK, System.Guid.Empty, System.Guid.Empty, true);

			receive1.RunPreSaveValidation();

			var warning = receive1.Lines[0].WE_WLInfo.GetWarnings().Single().Message;
			AssertMultilineASCIIEquals("Should have warning message.", @"Stock On Hand exists.
Client: 111, Product: P1", warning);
		}

		#endregion

		#region TestPerformanceOfFinaliseWithSerialNoAttributes

		[StressTest]
		public void TestPerformanceOfFinaliseWithSerialNoAttributes()
		{
			const int NumberOfInventoriesToCreate = 100; // Inventory Lines to create

			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.MiscServ.OM_IMUseSerialNumber = true;
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			for (int i = 0; i < NumberOfInventoriesToCreate; i++)
			{
				var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");
				inventory.WI_SerialNumber = "SN1_" + i;
			}
			receive.AllocateLocationsWithMock();

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 2 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 2 },
				{ JobServiceSchema.Constants.TableName, 1 },
				{ JobHeaderSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ ProcessTaskNotificationSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ ProcessWorkflowExceptionTypeSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketContainerSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsDocketPalletSchema.Constants.TableName, 1 },
				{ WhsDocketReferenceSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 3 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 2 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsClientParameterByWarehouseSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 1 },
			};

			using (RowFactory.SetCachedTables())
			{
				receiveInOtherFactory.FinaliseDocketWithoutUserConfirmation();
			}

			AssertEquals("Receive Finalise didn't work.", true, receiveInOtherFactory.IsFinalised);
			AssertDbHits(expectedDbHits, otherFactory);
		}

		#endregion

		#region TestPerformanceOfRunPreSaveValidation_WhenUsingCrossDocking

		[StressTest]
		public void TestPerformanceOfRunPreSaveValidation_WhenUsingCrossDocking()
		{
			const int numberOfInventoriesToCreate = 100;

			var data = new TestDataSimpleEnvironment(Factory, 10, 10);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			for (int i = 0; i < numberOfInventoriesToCreate; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			}

			receive.AllocateLocationsWithMock();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			for (int i = 0; i < numberOfInventoriesToCreate / 2; i++)
			{
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
				AssertNotNull("Precondition - divot was created.", orderLine.ReserveStockIfAbleTo(receive.Inventory[i * 2]));
				AssertNotNull("Precondition - divot was created.", orderLine.ReserveStockIfAbleTo(receive.Inventory[(i * 2) + 1]));
			}

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			using (RowFactory.SetCachedTables())
			{
				receiveInOtherFactory.RunPreSaveValidation();
				receiveInOtherFactory.Validation.ValidateAll();
			}

			var expectedDBHitsForValidation = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsInventoryViewSchema.Constants.TableName, 2 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 2 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 3 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
			};

			AssertDbHits(expectedDBHitsForValidation, otherFactory);

			var expectedDBHitsForFinalisation = new Dictionary<string, int>(expectedDBHitsForValidation);
			expectedDBHitsForFinalisation[WhsDocketSchema.Constants.TableName] += 1;
			expectedDBHitsForFinalisation.Add(WhsDocketReferenceSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(WhsDocketPalletSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(ProcessTasksSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(JobServiceSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(ProcessTaskNotificationSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(WhsDocketContainerSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(WhsClientParameterByWarehouseSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(ProcessWorkflowExceptionTypeSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(JobDocAddressSchema.Constants.TableName, 2);
			expectedDBHitsForFinalisation.Add(JobHeaderSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(ProcessTaskTemplateSchema.Constants.TableName, 1);

			receiveInOtherFactory.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receiveInOtherFactory);
			AssertDbHits(expectedDBHitsForFinalisation, otherFactory);
		}

		#endregion

		#region TestPerformanceOfRunPreSaveValidation_WhenUsingPalletID_DBHits

		[StressTest]
		public void TestPerformanceOfRunPreSaveValidation_WhenUsingPalletID_DBHits_Parameterized()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				const int NumberOfInventoriesToCreate = 100; // Inventory Lines to create
				const int ExpectedDBHitsCount = 8;
				var data = new TestDataSimpleEnvironment(Factory, NumberOfInventoriesToCreate, 1);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);

				var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
				for (int i = 0; i < NumberOfInventoriesToCreate; i++)
				{
					Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[i], "PLT-" + i);
				}

				Factory.Save();
				Factory.ResetDatabaseLoadCount();

				using (AssertDbHitsWithUsefulQueryInformation(new Dictionary<string, int>
				{
					{ OrgCustomLabelsSchema.Constants.TableName, 1 },
					{ StmEventSchema.Constants.TableName, 1 },
					{ WhsDocketLineSchema.Constants.TableName, 5 },
				}, Factory))
				{
					receive.RunPreSaveValidationWithFetchHints();
				}

				AssertEquals("Too many DB hits.", ExpectedDBHitsCount, Factory.DatabaseLoadCount);
			}
		}

		#endregion

		#region TestPerformanceOfJulianBatchNumbersAndValidation_DBHits

		public void TestPerformanceOfJulianBatchNumbersAndValidation_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			const int NumberOfProductsToCreate = 10;
			const int NumberOfInventoriesToCreatePerProduct = 5; // Inventory Lines to create
			var products = new OrgSupplierPart[NumberOfProductsToCreate];
			for (int i = 0; i < NumberOfProductsToCreate; i++)
			{
				var product = Helper.CreateProduct(data.Org1, "PR" + i);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
				Helper.SetProductAttributeUse(data.Org1, product, AttributeNumber.One, true);
				product.RelatedOrganisations.Cast<OrgPartRelation>().Single(l => l.OU_OH == data.Org1.PK).OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.BatchNumber_YDDD;
				Helper.CreateProductParamsByWhsAndClient(product, data.Org1, data.Whs1, maximumShelfLife: 2);
				products[i] = product;
			}

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			for (int i = 0; i < NumberOfInventoriesToCreatePerProduct; i++)
			{
				for (int j = 0; j < NumberOfProductsToCreate; j++)
				{
					Helper.CreateWhsReceiveInventoryLine(receive, products[j], 1m, ZDate.Empty, ZDate.Empty, "ABC3001", "", "", "");
				}
			}

			receive.AllocateLocationsWithMock();
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			using (RowFactory.SetCachedTables())
			{
				receiveInOtherFactory.RunPreSaveValidationWithFetchHints();
			}

			var expectedDBHitsForValidation = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 }, // this is the fetch hint we are testing
				{ RefPacksSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
			};

			AssertDbHits(expectedDBHitsForValidation, otherFactory);
		}

		#endregion

		#region TestPerformanceOfRunPreSaveValidation_CustomsReceive_DBHits

		[StressTest]
		public void TestPerformanceOfRunPreSaveValidation_CustomsReceive_DBHits()
		{
			const int NumberOfInventoriesToCreate = 100; // Inventory Lines to create
			var data = new TestDataSimpleEnvironment(Factory, NumberOfInventoriesToCreate, 1);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			foreach (var location in locations)
			{
				location.WLV_WA_PutawayArea = bondedArea.PK;
			}
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			for (int i = 0; i < NumberOfInventoriesToCreate; i++)
			{
				var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locations[i], "");
				receiveLine.CustomsData.WB_EntryKey = "E01";
			}

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			using (AssertDbHitsWithUsefulQueryInformation(new Dictionary<string, int>
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ StmALogSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
				{ WhsBondedWarehouseAttributeSchema.Constants.TableName, 2 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 2 },
				{ WhsLocationViewSchema.Constants.TableName, 2 },
				{ WhsPickLineSchema.Constants.TableName, 2 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			}, otherFactory))
			{
				receiveInOtherFactory.RunPreSaveValidationWithFetchHints();
			}
		}

		#endregion

		#endregion

		#region Customs Stuff

		protected override void TestIsCustomsTransactionCore()
		{
			var receive = GetNewBusinessObject();
			AssertEquals(false, receive.IsCustomsTransaction);
			AssertEquals(false, receive.IsCustomsDataVisible);

			receive.WD_DocketSubType = CodeLists.ReceiveType.Codes.Customs;
			AssertEquals(true, receive.IsCustomsTransaction);
			AssertEquals(true, receive.IsCustomsDataVisible);
		}

		public override void TestDefaultDocketSubTypeForCustomsTransaction()
		{
			Docket = GetNewBusinessObject();
			AssertEquals(CodeLists.ReceiveType.Codes.Customs, Docket.DefaultDocketSubTypeForCustomsTransaction);
		}

		#endregion

		#region EventLogs

		public void TestReceiveLogSequence()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.WD_DocketStatus = DocketStatus.Codes.Entered;
			Factory.Save();

			receive.WD_DocketStatus = DocketStatus.Codes.Putaway;
			Factory.Save();

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);
			var enteredTime = receiveInOtherFactory.Logs.Find(Helper.GetLogFilter(Events.WarehouseJobEntered.Code))[0].SL_EventTime;
			var puttingAwayTime = receiveInOtherFactory.Logs.Find(Helper.GetLogFilter(Events.WarehouseReceiptPuttingAway.Code))[0].SL_EventTime;
			var finalisedTime = receiveInOtherFactory.Logs.Find(Helper.GetLogFilter(Events.ItemDocumentJobFinalised.Code))[0].SL_EventTime;

			Assert("Putaway time should be after entered time", puttingAwayTime > enteredTime);
			Assert("Finalised time should be after putting away time", finalisedTime > puttingAwayTime);
		}

		public void TestFinalisedEvent_OnlyAddedOnce()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoor = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoor = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventoryLine = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoor, "PLT1", 10m);

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = putawayTransfer.Lines.AddNew();
			putawayTransferLine.SetDocketLineFromInventory(inventoryLine.InDocketLine, ExcludeFromCopy.None);
			putawayTransferLine.WE_WL_TransferFrom = dockDoor.PK;
			putawayTransferLine.WE_WL = nonDockDoor.PK;
			putawayTransfer.RunPreSaveValidation();

			AssertNotNull("Precondition: Putaway transfer has been created.", putawayTransferLine);
			AssertEquals("Precondition: Receive line's inventory status is Received.", InventoryStatus.Codes.Received, inventoryLine.WI_InventoryStatus);

			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;
			putawayTransfer.FinaliseDocketWithoutUserConfirmation();

			AssertIsFinalisedPrecondition(putawayTransfer);
			receive.FinaliseDocketWithoutUserConfirmation();

			AssertEquals("Putaway transfer line's current inventory status is Available.", InventoryStatus.Codes.Available, putawayTransferLine.WE_CurrentInventoryStatus);
			putawayTransferLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Putaway;

			Helper.AssertZCannotSaveExceptionThrown("Cannot save as putaway transfer lines have invalid inventory status.", Factory.Save);

			putawayTransferLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			AssertNoExceptionThrown("Should be able to save when transfer line status is Available.", Factory.Save);

			var finalisedLogs = receive.Logs.Find(Helper.GetLogFilter(Events.ItemDocumentJobFinalised.Code));
			AssertEquals("Finalised Log should only get added once.", 1, finalisedLogs.Length);
		}

		#endregion

		#region TestCreateOperationalStatusChangeEvents

		[TestDate(2013, 9, 26, 5, 10, 0)]
		public void TestCreateOperationalStatusChangeEvents()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			var receive = GetNewBusinessObject();
			receive.WD_OH_Client = org.PK;
			receive.WD_WW_Whs = whs.PK;
			receive.WD_DocketStatus = DocketStatus.Codes.Putaway;
			Factory.Save();
			AssertEquals("Putaway event should be created", 1, receive.Logs.Find(Helper.GetLogFilter(Events.WarehouseReceiptPuttingAway.Code)).Length);
			AssertEquals("ETA event should not be created", 0, receive.Logs.Find(Helper.GetLogFilter(Events.WarehouseReceiptETANotification.Code)).Length);
			AssertEquals("Arrival event should not be created", 0, receive.Logs.Find(Helper.GetLogFilter(Events.WarehouseReceiptArrived.Code)).Length);

			receive.WD_ETA = ZDateTimeOffset.Now.AddDays(-1);
			receive.WD_ArrivalDate = ZDateTimeOffset.Now.AddHours(-2);
			Factory.Save();
			var expectedETALog = ZDateTime.Now;
			var expectedArriveLog = ZDateTime.Now;
			var actualETALog = receive.Logs.Find(Helper.GetLogFilter(Events.WarehouseReceiptETANotification.Code))[0].SL_EventTime;
			var actualArriveLog = receive.Logs.Find(Helper.GetLogFilter(Events.WarehouseReceiptArrived.Code))[0].SL_EventTime;

			AssertNotEquals("ETA Log should not use WD_ETA", receive.WD_ETA, actualETALog);
			AssertNotEquals("ArrivalDate should not use ArrivalDate", receive.WD_ArrivalDate, actualArriveLog);
			AssertEquals("ETA event date incorrect", expectedETALog, actualETALog);
			AssertEquals("Arrival evnt date incorrect", expectedArriveLog, actualArriveLog);
			AssertEquals("Only one Putaway event should be created", 1, receive.Logs.Find(Helper.GetLogFilter(Events.WarehouseReceiptPuttingAway.Code)).Length);
			receive.WD_PalletsSent = 10;
			Factory.Save();
			AssertEquals("Only one ETA event should be created", 1, receive.Logs.Find(Helper.GetLogFilter(Events.WarehouseReceiptETANotification.Code)).Length);
			AssertEquals("Only one Arrival event should be created", 1, receive.Logs.Find(Helper.GetLogFilter(Events.WarehouseReceiptArrived.Code)).Length);
		}

		#endregion

		#region Business Object Overrides

		#region TestDataRefreshOnDeletedInventory

		public void TestDataRefreshOnDeletedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part11 = Helper.CreateProduct(data.Org1, "P11");
			var part12 = Helper.CreateProduct(data.Org1, "P12");

			Helper.CreateProductBOM(data.Part1, part11, 1m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(data.Part1, part12, 1m, Constants.PkgUnit.Unit);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 20m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, part11, 20m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, part12, 20m, data.Whs1.DefaultLocation);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "WO1", WorkOrderType.Codes.Assemble);
			var line1 = Helper.CreateWhsWorkOrderLine(workOrder, data.Part1, 20m, Constants.PkgUnit.Pallet);
			var pick = Helper.CreatePickNew(workOrder);
			AssertEquals("Precondition.", true, workOrder.IsAttachedToPickButNotFinalised);

			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertNotNull("Precondition", workOrder.Receive);
			var receiveLine = workOrder.Receive.Lines[0];
			var inventory = workOrder.Receive.Inventory[0];
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var receiveInAnotherFactory = newFactory.Load<WhsReceive>(workOrder.Receive.PK);
			var receiveLineInAnotherFactory = receiveInAnotherFactory.Lines[0];
			var inventoryInAnotherFactory = receiveInAnotherFactory.Inventory[0];
			receiveInAnotherFactory.Inventory.DeleteAll();
			AssertEquals(true, receiveLineInAnotherFactory.IsDeleted);
			AssertEquals(true, inventoryInAnotherFactory.IsDeleted);
			newFactory.Save();

			AssertNoExceptionThrown(() => Factory.Save());
			AssertEquals(true, receiveLine.IsDeleted);
			AssertEquals(true, inventory.IsDeleted);
			AssertEquals(0, workOrder.Receive.Lines.Count);
			AssertEquals(0, workOrder.Receive.Inventory.Count);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			Receive.WD_DocketID = "V00001001";
			AssertEquals("Warehouse Receipt V00001001", Receive.HumanReadableName);
		}

		#endregion

		#region TestSetDefaultValues

		protected override void TestSetDefaultValuesCore(WhsReceive docket)
		{
			base.TestSetDefaultValuesCore(docket);
			AssertEquals("Docket type must be 'INW'", CodeLists.DocketType.Codes.Receive, docket.WD_DocketType);
			AssertEquals("Docket sub type must be 'REC'", CodeLists.ReceiveType.Codes.Receipt, docket.WD_DocketSubType);
			AssertEquals("Arrival Date must be Empty", ZDateTimeOffset.Empty, docket.WD_ArrivalDate);
		}

		#endregion

		#region TestOnFactorySavingDoesnotAutoGenerateReceiveReferenceWhenItShouldnot

		public void TestOnFactorySavingDoesnotAutoGenerateReceiveReferenceWhenItShouldnot()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("1");
			OrgHeader org = Helper.CreateClient();
			WhsReceive receive = Helper.CreateWhsReceive(org, whs, "I1");

			Factory.Save();
			AssertEquals("I1", receive.WD_ExternalReference);
		}

		#endregion

		#region TestCanDelete

		protected override bool CanDeleteOverride
		{
			get { return false; }
		}

		protected override MultilingualString CantDeleteReasonMsg
		{
			get { return (NoResString)"Receives cannot be deleted."; }
		}

		#endregion

		#region TestDelete

		public override void TestDelete()
		{
			base.TestDelete();

			var order = Factory.New<WhsOrder>();
			order.Delete();

			var receive = Factory.New<WhsReceive>();
			var i1 = receive.Lines.AddNew().Inventory[0];
			var i2 = receive.Lines.AddNew().Inventory[0];
			receive.Delete();
			AssertEquals(0, receive.Inventory.Count);
			AssertEquals(true, i1.IsDeleted);
			AssertEquals(true, i2.IsDeleted);
		}

		#endregion

		#region TestDelete_UnlinksChildSplits

		public void TestDelete_UnlinksChildSplits()
		{
			var parent = GetNewBusinessObject();
			var parentOfParent = GetNewBusinessObject();
			var parentOfParentOfParent = GetNewBusinessObject();
			var split1 = GetNewBusinessObject();
			var split2 = GetNewBusinessObject();
			parent.FillWithValidTestData();
			parentOfParent.FillWithValidTestData();
			parentOfParentOfParent.FillWithValidTestData();
			split1.FillWithValidTestData();
			split2.FillWithValidTestData();

			parentOfParent.WD_WD_Split = parentOfParentOfParent.PK;
			parent.WD_WD_Split = parentOfParent.PK;
			split1.WD_WD_Split = parent.PK;
			split2.WD_WD_Split = parent.PK;
			Factory.Save();

			parent.Delete();
			Assert("Precondition.", parent.IsDeleted);
			AssertNoExceptionThrown(Factory.Save);

			AssertEquals("Split dockets should no longer have a link.", ZGuid.Empty, split1.WD_WD_Split);
			AssertEquals("Split dockets should no longer have a link.", ZGuid.Empty, split2.WD_WD_Split);
			AssertEquals("Should not have removed links for the deleted receives parent.", parentOfParentOfParent.PK, parentOfParent.WD_WD_Split);
		}

		#endregion

		#region Notes

		protected override void TestNoteContextsForRelatedNotesSetup(WhsReceive docket)
		{
			base.TestNoteContextsForRelatedNotesSetup(docket);

			var whsReceive = docket;
			var transportCo = Helper.CreateClient();
			var supplier = Helper.CreateClient();
			CreateTestNoteCollection(transportCo);
			CreateTestNoteCollection(supplier);

			whsReceive.TransportCoDocAddress.OrganisationPK = transportCo.PK;
			whsReceive.SupplierDocAddress.OrganisationPK = supplier.PK;
		}

		protected override void TestNoteContextsForRelatedNotesAssertions(WhsReceive whsReceive)
		{
			Assert("Should always be 'Warehouse' module", (whsReceive.GetNoteContextsForRelatedNotes().Module & StmNoteContextModule.W) != 0);
			Assert("Should always be 'In' direction", (whsReceive.GetNoteContextsForRelatedNotes().Direction & StmNoteContextDirection.R) != 0);
			Assert("Should always be 'Receive' freight mode", (whsReceive.GetNoteContextsForRelatedNotes().FreightMode & StmNoteContextFreightMode.R) != 0);

			AssertEquals("Visible Notes Count", 16, whsReceive.Notes.VisibleNotes.Count);
		}

		#endregion

		#endregion

		#region Fetch Strategy

		protected override Type FetchStrategyType
		{
			get { return typeof(WhsReceiveFetchStrategy); }
		}

		#endregion

		#region TestOnFactorySavingDoesnotHitDatabaseIfThereAreNoChanges

		public void TestOnFactorySavingDoesnotHitDatabaseIfThereAreNoChanges()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			Factory.Save();

			var connection = (Factory as IDbConnected).Connection;
			var initialCount = connection.ExecutedCommandCount;
			AssertEquals("", false, receive.HasChanges);
			Factory.Save();
			AssertEquals("No queries should be run in on Factory Saving event without any receive changes", initialCount, connection.ExecutedCommandCount);
		}

		#endregion

		#region Related Business Objects

		#region TestAsnLineCollection

		public void TestAsnLineCollection()
		{
			TestDataForInventory data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			data.Receive11.AsnLines.AddNew();
			data.Receive11.AsnLines.AddNew();
			data.Receive11.AsnLines.AddNew();
			data.Receive11.AsnLines.AddNew();
			data.Receive11.AsnLines.AddNew();
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			WhsReceive receive = newFactory.Load<WhsReceive>(data.Receive11.PK);
			AssertNotNull(receive);
			AssertEquals(5, receive.AsnLines.Count);
		}

		#endregion

		#region TestLines

		protected override Type ExpectedLineCollectionType => typeof(WhsReceiveLineCollection);

		public void TestLines_IsOriginalInventory()
		{
			var receive = Factory.New<WhsReceive>();
			var receiveLine1 = receive.Lines.AddNew();
			var receiveLine2 = receive.Lines.AddNew();
			AssertEquals("Both lines should be in the collection", 2, receive.Lines.Count);

			receiveLine2.WE_IsOriginalInventory = false;
			AssertEquals("Only original inventory lines should be in the collection.", 1, receive.Lines.Count);
			receive.Lines.Single(l => l.PK == receiveLine1.PK);
		}

		#endregion

		#region ExpectedAdditionalNoteTypesCore

		protected override IEnumerable<PredefinedNoteType> ExpectedAdditionalNoteTypesCore
		{
			get
			{
				yield return PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks;
				yield return PredefinedNoteTypes.Instance.ReceiveConfirmationInstructionsNote;
			}
		}

		#endregion

		#region TestInventory

		public void TestInventory()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory(false);
			var inv1 = Helper.CreateWhsReceiveInventoryLine(data.Receive21, data.Part1, 10m);
			AssertEquals("Pre-condition:", 8, data.Receive21.Inventory.Count);

			var inv2 = Helper.CreateWhsReceiveInventoryLine(data.Receive21, data.Part1, 10m);
			inv2.WI_InDocketLineUnits = 0;
			inv2.WI_IsOriginalReceiptLine = false;
			var inv2DocketLine = inv2.InDocketLine;
			inv2DocketLine.WE_WE_ParentDocketLine = inv1.InDocketLine.PK;
			inv2DocketLine.WE_WE_OriginalDocketLineForRating = inv1.InDocketLine.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var receive = newFactory.Load<WhsReceive>(data.Receive21.PK);
			AssertEquals(8, receive.Inventory.Count);
		}

		#endregion

		#region BusinessObjectsWithRelatedEvents

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines();
			var forwardingOrder = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IOrder>();
			var pivot = Factory.New<WhsDocketJobPivot>();
			pivot.WV_ParentId = forwardingOrder.PK;
			pivot.WV_ParentTableCode = forwardingOrder.TablePrefix;
			pivot.WV_DocketType = data.Receive11.WD_DocketType;
			pivot.WV_WD_Docket = data.Receive11.PK;

			AssertContainsExactElementsInAnyOrder(data.Receive11.Lines.Concat(new[] { forwardingOrder }), data.Receive11.BusinessObjectsWithRelatedEvents);
		}

		public void TestBusinessObjectsWithRelatedEvents_Shipment()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines();

			var shipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var pivot = Factory.New<WhsDocketJobPivot>();
			pivot.WV_ParentId = shipment.PK;
			pivot.WV_ParentTableCode = shipment.TablePrefix;
			pivot.WV_DocketType = Docket.WD_DocketType;
			pivot.WV_WD_Docket = Docket.PK;

			AssertCollectionContains(shipment, Docket.BusinessObjectsWithRelatedEvents);
		}

		#endregion

		#region TestCheckSerialNumberIsUnique

		public void TestCheckSerialNumberIsUnique()
		{
			TestCheckSerialNumberIsUniqueCore(useBothRelationshipType: false);
		}

		public void TestCheckSerialNumberIsUnique_BothRelationshipType()
		{
			TestCheckSerialNumberIsUniqueCore(useBothRelationshipType: true);
		}

		public void TestCheckSerialNumberIsUnique_RF()
		{
			TestCheckSerialNumberIsUnique_RF_Core(useBothRelationshipType: false);
		}

		public void TestCheckSerialNumberIsUnique_RF_BothRelationshipType()
		{
			TestCheckSerialNumberIsUnique_RF_Core(useBothRelationshipType: true);
		}

		void TestCheckSerialNumberIsUnique_RF_Core(bool useBothRelationshipType)
		{
			var isWeb = Globals.IsWeb;
			var isUserInteractive = Globals.IsUserInteractive;

			try
			{
				Globals.IsWeb = true;
				Globals.IsUserInteractive = false;
				AssertEquals("Precondition: Is RF.", true, WhsEnvironment.IsRF);

				TestCheckSerialNumberIsUniqueCore(useBothRelationshipType);
			}
			finally
			{
				Globals.IsWeb = isWeb;
				Globals.IsUserInteractive = isUserInteractive;
			}
		}

		void TestCheckSerialNumberIsUniqueCore(bool useBothRelationshipType)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true, "Serial Number 1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.BatchNumber, "Batch Number");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Three, true);

			if (useBothRelationshipType)
			{
				var part1Relation = data.Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
				part1Relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			}

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "BN1", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "BN1", "");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "SN1", ""); // Batch = SN from another inventory on this job
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "BN1", "");
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "BN1", "");
			var inventory6 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "BN1", "");
			inventory1.WI_SerialNumber = "SN1"; // SN not duplicated
			inventory2.WI_SerialNumber = "SN2"; // SN is duplicated on same job
			inventory3.WI_SerialNumber = "SN4"; // SN not duplicated
			inventory4.WI_SerialNumber = "SN2"; // SN is duplicated on same job
			inventory5.WI_SerialNumber = "SN3"; // SN duplicated on another job
			inventory6.WI_SerialNumber = "SN5"; // SN not duplicated

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var inventory7 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "BN1", "");
			var inventory8 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "BN1", "");
			var inventory9 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "BN3", "");
			var inventory10 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "BN3", "");
			var inventory11 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 1m, ZDate.Empty, ZDate.Empty, "", "", "BN3", "");
			var inventory12 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 1m, ZDate.Empty, ZDate.Empty, "", "", "BN3", "");

			inventory7.WI_SerialNumber = "SN3"; // SN is duplicated on another job
			inventory8.WI_SerialNumber = "SN6"; // SN not duplicated
			inventory9.WI_SerialNumber = "SN8"; // SN not duplicated for Part 1
			inventory10.WI_SerialNumber = "SN9"; // SN not duplicated for Part 1
			inventory11.WI_SerialNumber = "SN8"; // SN not duplicated for Part 2
			inventory12.WI_SerialNumber = "SN9"; // SN not duplicated for Part 2

			Factory.Save();
			Factory.ClearQueryCache(WhsInventoryViewSchema.Constants.TableName);

			// Single inventory validation

			AssertEquals(true, receive1.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory1, true)); // SN1 is not duplicated
			AssertEquals(false, receive1.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory2, true)); // SN2 is duplicated on the same job
			AssertEquals(false, receive1.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory4, true)); // SN2 is duplicated on the same job
			AssertEquals(false, receive1.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory5, true)); // SN3 is duplicated on another job
			AssertEquals(true, receive1.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory3, true)); // SN4 is not duplicated
			AssertEquals(true, receive1.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory6, true)); // SN5 is not duplicated

			AssertEquals(false, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory7, true)); // SN3 is duplicated on another job
			AssertEquals(true, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory8, true)); // SN6 is not duplicated
			using (WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "CLI")) // unique per client
			{
				// SN8 & SN9 duplicated across client
				AssertEquals(false, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory9, true));
				AssertEquals(false, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory10, true));
				AssertEquals(false, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory11, true));
				AssertEquals(false, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory12, true));
			}
			using (WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "PRO")) // unique per product
			{
				// SN8 & SN9 not duplicated across product
				AssertEquals(true, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory9, true));
				AssertEquals(true, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory10, true));
				AssertEquals(true, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory11, true));
				AssertEquals(true, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory12, true));
			}

			//  Receive validation

			var validationsRunCount = 0;
			receive1.WD_DocketTypeInfo.AdditionalValidation += () =>
			{
				AssertEquals(true, receive1.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory1, true)); // SN1 is not duplicated
				AssertEquals(false, receive1.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory2, true)); // SN2 is duplicated on the same job
				AssertEquals(false, receive1.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory4, true)); // SN2 is duplicated on the same job
				AssertEquals(false, receive1.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory5, true)); // SN3 is duplicated on another job
				AssertEquals(true, receive1.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory3, true)); // SN4 is not duplicated
				AssertEquals(true, receive1.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory6, true)); // SN5 is not duplicated
				validationsRunCount++;
			};
			receive1.RunPreSaveValidation();

			RunValidationInvoker action = () =>
			{
				using (WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "CLI")) // unique per client
				{
					AssertEquals(false, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory7, true)); // SN3 is duplicated on another job
					AssertEquals(true, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory8, true)); // SN6 is not duplicated

					// SN8 & SN9 duplicated across client
					AssertEquals(false, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory9, true));
					AssertEquals(false, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory10, true));
					AssertEquals(false, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory11, true));
					AssertEquals(false, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory12, true));
					validationsRunCount++;
				}
			};
			receive2.WD_DocketTypeInfo.AdditionalValidation += action;
			receive2.RunPreSaveValidation();
			receive2.WD_DocketTypeInfo.AdditionalValidation -= action; // clean up

			receive2.WD_DocketTypeInfo.AdditionalValidation += () =>
			{
				using (WarehouseDataRegistry.Instance.EnforceSerialUniqueness.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "PRO")) // unique per product
				{
					AssertEquals(false, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory7, true)); // SN3 is duplicated on another job
					AssertEquals(true, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory8, true)); // SN6 is not duplicated

					// SN8 & SN9 not duplicated across product
					AssertEquals(true, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory9, true));
					AssertEquals(true, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory10, true));
					AssertEquals(true, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory11, true));
					AssertEquals(true, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory12, true));
					validationsRunCount++;
				}
			};
			receive2.RunPreSaveValidation();
			AssertEquals("Ensure validation was run 3 times.", 3, validationsRunCount);
		}

		public void TestCheckSerialNumberIsUnique_EmptySN()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.BatchNumber, "Batch Number");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Three, true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");

			Factory.Save();
			AssertNoExceptionThrown("Should not throw SQL exception", () => receive1.ReceiveValidationStrategy.CheckSerialNumberIsUnique(receive1.Client, inventory1, true));
		}

		public void TestCheckSerialNumberIsUnique_WithInTransitLines()
		{
			TestCheckSerialNumberIsUnique_WithInTransitLines_Core();
		}

		public void TestCheckSerialNumberIsUnique_WithInTransitLines_RF()
		{
			var isWeb = Globals.IsWeb;
			var isUserInteractive = Globals.IsUserInteractive;

			try
			{
				Globals.IsWeb = true;
				Globals.IsUserInteractive = false;
				AssertEquals("When Web Tracker.", true, WhsEnvironment.IsRF);

				TestCheckSerialNumberIsUnique_WithInTransitLines_Core();
			}
			finally
			{
				Globals.IsWeb = isWeb;
				Globals.IsUserInteractive = isUserInteractive;
			}
		}

		void TestCheckSerialNumberIsUnique_WithInTransitLines_Core()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Serial, true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN1";
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			AssertEquals("Precondition: Stock On Hand.", 1m, inventory1.WI_TotalUnits);
			Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Today);
			AssertEquals("Precondition: No Stock On Hand.", 0m, inventory1.WI_TotalUnits);
			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory2.WI_SerialNumber = "SN1";
			AssertEquals(false, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory2, true));

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			Factory.Save();

			AssertEquals(true, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory2, true));
		}

		public void TestCheckSerialNumberIsUnique_InMemoryPickLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true, "Serial Number 1");
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			inventory1.WI_SerialNumber = "SN1"; // SN not duplicated

			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			AssertEquals(true, receive1.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory1, true));

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Now;

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "", "", "BN1", "");
			inventory2.WI_SerialNumber = "SN1";

			AssertEquals(true, receive2.ReceiveValidationStrategy.CheckSerialNumberIsUnique(data.Org1, inventory1, true));
		}

		public void TestCheckSerialNumberIsUnique_ReceiveHasError()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation);
				var pivot1 = receiveLine.SerialNumbers.AddNew();
				pivot1.SerialNumberValue = "SN1";
				var pivot2 = receiveLine.SerialNumbers.AddNew();
				pivot2.SerialNumberValue = "SN1";

				AssertHasError("WSN_SerialNumberInfo has an error.", pivot2.SerialNumberValueInfo, "Serial # already used.");
			}
		}

		public void TestCheckSerialNumberIsUnique_ReceiveSaveDbHits()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var n = 100;
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, n, data.Whs1.DefaultLocation);
				for (var i = 0; i < n; i++)
				{
					receiveLine.SerialNumbers.AddNew().SerialNumberValue = $"SN{i}";
				}
				Factory.ResetDatabaseLoadCount();

				var expectedDbHits = new Dictionary<string, int>
				{
					{ DtbBookingConsolidationSchema.Constants.TableName, 1 },
					{ GlbBranchSchema.Constants.TableName, 1 },
					{ GlbStaffSchema.Constants.TableName, 1 },
					{ JobDocAddressSchema.Constants.TableName, 2 },
					{ JobHeaderSchema.Constants.TableName, 1 },
					{ OrgCustomLabelsSchema.Constants.TableName, 1 },
					{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
					{ ProcessTaskNotificationSchema.Constants.TableName, 1 },
					{ ProcessTasksSchema.Constants.TableName, 1 },
					{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
					{ WhsDocketJobPivotSchema.Constants.TableName, 1 },
					{ WhsSerialNumberSchema.Constants.TableName, 1 },
				};

				using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, Factory))
				using (RowFactory.SetCachedTables())
				{
					receive.RunPreSaveValidation();
					Factory.Save();
				}
			}
		}

		#endregion

		#region TestReceiveDeviceStrategy

		public void TestReceiveDeviceStrategy()
		{
			var receive1 = Factory.New<WhsReceive>();
			AssertEquals("When run by Enterprise", typeof(WhsReceiveValidationStrategy), receive1.ReceiveValidationStrategy.GetType());

			try
			{
				Globals.IsWeb = true;
				var receive2 = Factory.New<WhsReceive>();
				AssertEquals("When run by WebTracker", typeof(WhsReceiveValidationStrategy), receive2.ReceiveValidationStrategy.GetType());

				Globals.IsUserInteractive = false;
				var receive3 = Factory.New<WhsReceive>();
				AssertEquals("When run by RF web service", typeof(WhsReceiveValidationRFStrategy), receive3.ReceiveValidationStrategy.GetType());

				Globals.IsWeb = false;
				var receive4 = Factory.New<WhsReceive>();
				AssertEquals("When run by Service Tasks", typeof(WhsReceiveValidationStrategy), receive4.ReceiveValidationStrategy.GetType());
			}
			finally // clean up
			{
				Globals.IsWeb = false;
				Globals.IsUserInteractive = true;
			}
		}

		#endregion

		#region TestRelatedOrgPartyScreeningStatusCollection

		public void TestRelatedOrgPartyScreeningStatusCollection_AllScreenPartyLogsInclude()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();
			Assert(receive.RelatedOrgPartyScreeningStatusCollection.Count == 0);

			var warehouseAddress = receive.Warehouse.DocAddresses.AddNew();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			warehouseAddress.E2_OA_Address = orgAddress.PK;
			var warehouseAddressScreenPartyLog = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			warehouseAddressScreenPartyLog.PJ_ParentID = warehouseAddress.Organisation.PK;
			warehouseAddressScreenPartyLog.PJ_ParentTableCode = warehouseAddress.Organisation.TablePrefix;
			Factory.Save();
			Assert(receive.RelatedOrgPartyScreeningStatusCollection.Count == 1);
			AssertCollectionContains(warehouseAddress.Organisation.PK,
				receive.RelatedOrgPartyScreeningStatusCollection.Cast<IRelatedOrgPartyScreeningStatus>()
					.Select(o => o.PJ_ParentID));

			var localChargeScreenPartyLog = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			var jobLoader = new JobHeader.Loader(receive);
			var jobHeader = jobLoader.TryCreateWithoutMutexForTestOnly();
			jobHeader.JH_ParentID = receive.PK;
			jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;
			localChargeScreenPartyLog.PJ_ParentID = receive.JobHeader.LocalCharges.PK;
			localChargeScreenPartyLog.PJ_ParentTableCode = receive.JobHeader.LocalCharges.TablePrefix;
			Factory.Save();
			Assert(receive.RelatedOrgPartyScreeningStatusCollection.Count == 2);
			AssertCollectionContains(receive.JobHeader.LocalCharges.PK,
				receive.RelatedOrgPartyScreeningStatusCollection.Cast<IRelatedOrgPartyScreeningStatus>()
					.Select(o => o.PJ_ParentID));
		}

		public void TestRelatedOrgPartyScreeningStatusCollection_SelfAndAllRelatedChildJobsInclude()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();
			Assert(receive.RelatedOrgPartyScreeningStatusCollection.Count == 0);

			var selfJobLog = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			selfJobLog.PJ_ParentID = receive.PK;
			selfJobLog.PJ_ParentTableCode = receive.TablePrefix;
			Factory.Save();
			Assert(receive.RelatedOrgPartyScreeningStatusCollection.Count == 1);
			AssertCollectionContains(receive.PK,
				receive.RelatedOrgPartyScreeningStatusCollection.Cast<IRelatedOrgPartyScreeningStatus>()
					.Select(o => o.PJ_ParentID));
		}

		#endregion

		#region TestTransportCoDocAddress

		protected override JobDocAddressRequirement GetJobDocAddressRequirement(IJobWithTransportCompany docket, DocAddressType addressType)
		{
			return ((WhsReceive)docket).GetTransportCoRequirement(addressType);
		}

		public void TestTransportCoDocAddress_ReadOnlyIsLazyTriggered()
		{
			var receive = Factory.New<WhsReceive>();
			AssertNotNull("Poke & Precondition", receive.TransportCoDocAddress);

			AssertPersistentPropertiesHitCount("Getting TransportCoDocAddress should not trigger property hits.", 0, () => _ = receive.TransportCoDocAddress);

			var hits = GetPersistentPropertiesHitCount(() => _ = receive.TransportCoDocAddress.ReadOnly);
			AssertEquals("Poking at ReadOnly property should trigger state calculation.", true, hits > 0);
		}

		#endregion

		#endregion

		#region RelatedSplits

		public void TestRelatedSplits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var parent = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var split1 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var split2 = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			receive.WD_WD_Split = parent.PK;
			split1.WD_WD_Split = receive.PK;
			split2.WD_WD_Split = receive.PK;

			// cannot use AssertContains because it fails in tracking solution
			Assert(receive.RelatedSplits.Contains(parent));
			Assert(receive.RelatedSplits.Contains(split1));
			Assert(receive.RelatedSplits.Contains(split2));
			Assert(parent.RelatedSplits.Contains(receive));
			Assert(split1.RelatedSplits.Contains(receive));
			Assert(split2.RelatedSplits.Contains(receive));

			AssertEquals("PARENT", parent.SplitRelationshipTypeDescription);
			AssertEquals("PARENT", receive.SplitRelationshipTypeDescription);
			AssertEquals("CHILD", split1.SplitRelationshipTypeDescription);
			AssertEquals("CHILD", split2.SplitRelationshipTypeDescription);
		}

		public void TestRelatedSplits_AutoRefreshChildren()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var parent = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var split1 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var split2 = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			AssertEquals(0, parent.RelatedSplits.Count);
			AssertEquals(0, receive.RelatedSplits.Count);
			AssertEquals(0, split1.RelatedSplits.Count);
			AssertEquals(0, split2.RelatedSplits.Count);

			receive.WD_WD_Split = parent.PK;
			split1.WD_WD_Split = receive.PK;
			split2.WD_WD_Split = receive.PK;

			parent.HasChanges = false;
			split1.HasChanges = false;
			split2.HasChanges = false;

			AssertContainsExactElementsInAnyOrder("The parent should contain the child receive", receive, parent.RelatedSplits);
			AssertContainsExactElementsInAnyOrder("The receive should contain the two child receives", new[] { split1, split2 }, receive.RelatedSplits);

			AssertEquals("CHILD", split1.SplitRelationshipTypeDescription);
			AssertEquals("CHILD", split2.SplitRelationshipTypeDescription);

			AssertEquals(false, parent.HasChanges);
			AssertEquals(false, split1.HasChanges);
			AssertEquals(false, split2.HasChanges);
		}

		#endregion

		#region UpdateTotalLineUnits, UpdateTotalWeightAndVolume

		#region TestUpdateTotalLineUnitsWhenInventoryLineRemoved

		public void TestUpdateTotalLineUnitsWhenInventoryLineRemoved()
		{
			TestDataForInventory data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(20m, 30m, 40m, 0m, 0m, false);

			AssertEquals("Precondition", 90m, data.Receive11.WD_TotalUnitsFromLines);

			data.Receive11.Inventory.RemoveAndDelete(data.Line113);
			AssertEquals(50m, data.Receive11.WD_TotalUnitsFromLines);

			data.Receive11.Inventory.RemoveAndDelete(data.Line112);
			AssertEquals(20m, data.Receive11.WD_TotalUnitsFromLines);
		}

		protected override bool IsWD_TotalUnitsFromLinesUpdatedOnLineRemove
		{
			get { return false; } // Receive instead does this on removal of Inventory.
		}

		#endregion

		#endregion

		#region Status Change

		#region TestUpdateDocketStatus

		public void TestUpdateDocketStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			AssertEquals("Precondition:", DocketStatus.Codes.New, receive.WD_DocketStatus);

			receive.UpdateDocketStatus();
			AssertEquals(DocketStatus.Codes.New, receive.WD_DocketStatus);

			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.UpdateDocketStatus();
			AssertEquals(DocketStatus.Codes.New, receive.WD_DocketStatus);

			inventory.WI_WL = data.Whs1.DefaultLocation.PK;
			receive.WD_DocketStatus = DocketStatus.Codes.New;
			receive.UpdateDocketStatus();
			AssertEquals(DocketStatus.Codes.Putaway, receive.WD_DocketStatus);

			inventory.WI_WL = ZGuid.Empty;
			receive.WD_DocketStatus = DocketStatus.Codes.Putaway;
			receive.UpdateDocketStatus();
			AssertEquals(DocketStatus.Codes.New, receive.WD_DocketStatus);

			Factory.Save();
			AssertEquals("Precondition:", DocketStatus.Codes.Entered, receive.WD_DocketStatus);

			receive.UpdateDocketStatus();
			AssertEquals(DocketStatus.Codes.Entered, receive.WD_DocketStatus);

			inventory.WI_WL = data.Whs1.DefaultLocation.PK;
			receive.WD_DocketStatus = DocketStatus.Codes.Entered;
			receive.UpdateDocketStatus();
			AssertEquals(DocketStatus.Codes.Putaway, receive.WD_DocketStatus);

			inventory.WI_WL = ZGuid.Empty;
			receive.WD_DocketStatus = DocketStatus.Codes.Putaway;
			receive.UpdateDocketStatus();
			AssertEquals(DocketStatus.Codes.Entered, receive.WD_DocketStatus);

			inventory.WI_WL = data.Whs1.DefaultLocation.PK;
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(DocketStatus.Codes.Finalised, receive.WD_DocketStatus);

			receive.UpdateDocketStatus();
			AssertEquals(DocketStatus.Codes.Finalised, receive.WD_DocketStatus);
		}

		public void TestUpdateDocketStatus_PutawayReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			AssertEquals("Precondition:", DocketStatus.Codes.New, receive.WD_DocketStatus);

			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			inventory.WI_WL = data.Whs1.DefaultLocation.PK;
			receive.WD_DocketStatus = DocketStatus.Codes.Entered;
			receive.UpdateDocketStatus();
			AssertEquals("Docket status is putaway.", DocketStatus.Codes.Putaway, receive.WD_DocketStatus);
		}

		public void TestUpdateDocketStatus_DockdoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			AssertEquals("Precondition:", DocketStatus.Codes.New, receive.WD_DocketStatus);

			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			inventory.WI_WL = data.Whs1.DefaultOutboundDockDoorLocation.PK;
			receive.WD_DocketStatus = DocketStatus.Codes.Putaway;
			receive.UpdateDocketStatus();
			AssertEquals("Docket status is not Putaway.", DocketStatus.Codes.New, receive.WD_DocketStatus);
		}

		public void TestUpdateDocketStatus_FinalisedPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var dockDoor = data.Whs1.DefaultOutboundDockDoorLocation;
			var location = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoor, "A");
			AssertEquals("Precondition: WD_DocketStatus is New", DocketStatus.Codes.New, receive.WD_DocketStatus);
			Factory.Save();

			AssertEquals("Precondition: WD_DocketStatus is Entered", DocketStatus.Codes.Entered, receive.WD_DocketStatus);
			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoor, location, "A", 10m);
			putawayTransfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(putawayTransfer);
			AssertEquals("WD_DocketStatus is Putaway", DocketStatus.Codes.Putaway, receive.WD_DocketStatus);

			receive.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals("Precondition: WD_DocketStatus is Entered", DocketStatus.Codes.Entered, receive.WD_DocketStatus);
			receive.UpdateDocketStatus();
			AssertEquals("WD_DocketStatus is Putaway", DocketStatus.Codes.Putaway, receive.WD_DocketStatus);
		}

		#endregion

		#region TestReceiveRevertsStatusWhenAllLinesDeleted

		public void TestReceiveRevertsStatusWhenAllLinesDeleted()
		{
			var year = ZDateTime.Now.Year;

			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ArrivalDate = new ZDateTimeOffset(year, 1, 1);
			var inventoryLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			AssertEquals("Precondition:", DocketStatus.Codes.New, receive.WD_DocketStatus);

			inventoryLine1.WI_WL = data.Whs1.DefaultLocation.PK;
			AssertEquals(DocketStatus.Codes.Putaway, receive.WD_DocketStatus);

			receive.Inventory.RemoveAndDeleteAll();
			AssertEquals(DocketStatus.Codes.New, receive.WD_DocketStatus);
		}

		#endregion

		#endregion

		#region RunPreSaveValidation

		public virtual void TestRunPreSaveValidationWhenJobInErrorAndNotFixed()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			var docket = Helper.CreateWhsReceive(org, whs, "1");

			Factory.Save();

			docket.WD_WW_Whs = ZGuid.Empty;
			docket.RunPreSaveValidation();
			docket.WD_DocketStatus = DocketStatus.Codes.Error;

			AssertEquals("Precondition", DocketStatus.Codes.Error, docket.WD_DocketStatus);
			AssertEquals("Precondition", true, docket.HasErrors);
			AssertEquals("Precondition", false, docket.HasMessageErrors);

			docket.RunPreSaveValidation();

			AssertEquals("WD_DocketStatus must remain 'ERR'", DocketStatus.Codes.Error, docket.WD_DocketStatus);
			AssertEquals("Docket should still have errors", true, docket.HasErrors);
		}

		public virtual void TestRunPreSaveValidationWhenJobInErrorAndFixed()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			var docket = Helper.CreateWhsReceive(org, whs, "1");

			Factory.Save();

			docket.RunPreSaveValidation();
			docket.WD_DocketStatus = DocketStatus.Codes.Error;

			AssertEquals("Precondition", DocketStatus.Codes.Error, docket.WD_DocketStatus);
			AssertEquals("Precondition", false, docket.HasErrors);
			AssertEquals("Precondition", false, docket.HasMessageErrors);

			docket.RunPreSaveValidation();

			AssertEquals("WD_DocketStatus must be set to 'ENT'", DocketStatus.Codes.Entered, docket.WD_DocketStatus);
			AssertEquals("Docket Should not have any errors", false, docket.HasErrors);
		}

		public void TestRunPreSaveValidationDoesNotUpdateDocketTotals()
		{
			OrgHeader client = Helper.CreateClient();
			WhsWarehouse warehouse = Helper.CreateWarehouse("1");
			WhsReceive receive = Helper.CreateWhsReceive(client, warehouse, "1");

			WhsInventoryView inventory = receive.Lines.AddNew().Inventory[0];
			OrgSupplierPart part = Helper.CreateProduct(client, "P1");

			inventory.WI_InDocketLineUnits = 10M;
			inventory.WI_OP = part.PK;

			receive.RunPreSaveValidation();

			AssertEquals("The weight total was increased on saving", 0M, receive.WD_TotalWeight);
			AssertEquals("The volume total was increased on saving", 0M, receive.WD_TotalCubic);
		}

		public void TestRunPreSaveValidationWhenRelatedSplitsHasError()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var parent = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REC1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REC2");
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REC3");

			receive.WD_WD_Split = parent.PK;
			receive2.WD_WD_Split = parent.PK;
			Assert(parent.RelatedSplits.Contains(receive));
			Assert(parent.RelatedSplits.Contains(receive2));

			receive.AddRowError("Some Errors - 1");
			receive.AddRowError("Some Errors - 2");
			receive2.AddRowError("Some Errors");
			AssertEquals(true, parent.RelatedSplits.HasErrors());

			var expectedErrorMsg = @"Reference: REC2-0:
Error - Warehouse Receipt: Some Errors - 1
Error - Warehouse Receipt: Some Errors - 2
Reference: REC3-0:
Error - Warehouse Receipt: Some Errors";
			parent.RunPreSaveValidation();
			AssertHasRowError(parent, expectedErrorMsg);
		}

		public void TestRunPreSaveValidation_ReceivedPalletsMoreThanTotalPallets_RegistryControlEnabled()
		{
			RunPreSaveValidation_ReceivedPalletsMoreThanTotalPalletsCore(true);
		}

		public void TestRunPreSaveValidation_ReceivedPalletsMoreThanTotalPallets_RegistryControlDisabled()
		{
			RunPreSaveValidation_ReceivedPalletsMoreThanTotalPalletsCore(false);
		}

		void RunPreSaveValidation_ReceivedPalletsMoreThanTotalPalletsCore(bool isRegistryControlEnabled)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, data.Whs1.DefaultLocation, "PLT2");
			receive.WD_TotalPallets = 1;

			var expectedErrorMessage = "Total Pallets 1 does not equal the total of received Pallets 2.";

			using (WarehouseDataRegistry.Instance.TotalPalletsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, isRegistryControlEnabled))
			{
				receive.RunPreSaveValidation();
			}

			if (isRegistryControlEnabled)
			{
				AssertHasError("WD_PalletsInfo has an error.", receive.WD_TotalPalletsInfo, expectedErrorMessage);
			}
			else
			{
				AssertNoError("WD_TotalPalletsInfo should not have an error.", receive.WD_TotalPalletsInfo, expectedErrorMessage);
				AssertHasWarning("WD_PalletsInfo should have a warning only.", receive.WD_TotalPalletsInfo, expectedErrorMessage);
			}
		}

		public void TestRunPreSaveValidation_Pallets_MemoryLoad()
		{
			// 1k inventory to putaway across 100 pallets, 25 on another receive, 25 already putaway on this receive, 50 to go to engine
			var client = Helper.CreateClient("C1");
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 10, 10, 10);

			var products = new List<OrgSupplierPart>();
			for (var i = 0; i < 10; i++)
			{
				products.Add(Helper.CreateProduct("P" + i, client));
			}
			Factory.Save();

			var locations = row.Locations.ToArray();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, locations.Select(l => l.PK));

			var receive = Helper.CreateWhsReceive(client, whs, "R2");

			// Some pallets on this receive
			for (var i = 0; i < 100; i++)
			{
				var line1 = Helper.CreateWhsReceiveLine(receive, products[i % products.Count], 1m);
				var line2 = Helper.CreateWhsReceiveLine(receive, products[i % products.Count], 1m, locations[i]);
				line1.WE_PalletID = $"PLT-{i % 10}";
				line2.WE_PalletID = $"PLT-{i % 10}";
			}

			// Some pallets on another receive
			var oldReceive = Helper.CreateWhsReceive(client, whs, "R1");
			for (var i = 100; i < 400; i++)
			{
				var line1 = Helper.CreateWhsReceiveLine(receive, products[i % products.Count], 1m);
				var line2 = Helper.CreateWhsReceiveLine(oldReceive, products[i % products.Count], 1m, locations[i]);
				line1.WE_PalletID = $"PLT-{25 + (i % 10)}";
				line2.WE_PalletID = $"PLT-{25 + (i % 10)}";
			}

			oldReceive.FinaliseDocket();

			// Rest of the pallets go to the engine
			for (var i = 200; i < 300; i++)
			{
				var number = 50 + (i % 10);
				var line = Helper.CreateWhsReceiveLine(receive, products[i % products.Count], 1m);
				line.WE_PalletID = $"PLT-{number}";
			}
			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			BusinessObjectFactory.StartLogging();
			receiveInOtherFactory.RunPreSaveValidation();
			var loadLog = BusinessObjectFactory.DebugLog;
			BusinessObjectFactory.StopLogging();

			var logCount = Regex.Matches(loadLog, Regex.Escape("WE_StockOnHand > 0 and WE_PalletID <> '' and WE_PalletID")).Count;
			AssertEquals("Should load minimum Pallets.", 10, logCount);
		}

		public void TestRunPreSaveValidation_BlindReceiveWithPreventReceivingOversEnabled()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = true;
			partRelation.OU_ReceiveOverageTolerancePercent = 500;
			var receive = Helper.CreateWhsReceive(client, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, product, 1m);
			receiveLine1.WE_ClientOrderedUnits = 0m;
			receiveLine1.WE_TransactionQuantity = 7m;
			Factory.Save();

			receive.RunPreSaveValidation();
			AssertEquals(false, receive.HasErrors);
		}

		public void TestRunPreSaveValidation_BlindReceiveTemporaryProducts()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var receive = Helper.CreateWhsReceive(client, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, ZGuid.Invalid, 20m);
			receiveLine1.ProductCode = "TemporaryTestProd1";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, ZGuid.Invalid, 40m);
			receiveLine2.ProductCode = "TemporaryTestProd2";
			receiveLine2.ProductDesc = "Temporary test prod2 description";
			receiveLine1.WE_TransactionQuantity = 21m;
			receiveLine2.WE_TransactionQuantity = 100m;

			receive.RunPreSaveValidation();
			AssertEquals(true, receive.HasErrors);

			var product1Summary = receive.ReceiveProductSummaryCollection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == "TemporaryTestProd1");
			AssertEquals(false, product1Summary.HasErrors);
			AssertEquals("", product1Summary.ProductDescription);
			AssertEquals(20m, product1Summary.ExpectedQuantity);
			AssertEquals(21m, product1Summary.ReceivedQuantity);

			var product2Summary = receive.ReceiveProductSummaryCollection.Cast<WhsReceiveProductSummary>()
			   .FirstOrDefault(x => x.ProductCode == "TemporaryTestProd2");
			AssertEquals(false, product2Summary.HasErrors);
			AssertEquals("Temporary test prod2 description", product2Summary.ProductDescription);
			AssertEquals(40m, product2Summary.ExpectedQuantity);
			AssertEquals(100m, product2Summary.ReceivedQuantity);
		}

		public void TestRunPreSaveValidation_ReceiveWithPreventReceivingOversEnabledButNoOverReceiving()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = true;
			partRelation.OU_ReceiveOverageTolerancePercent = 500;
			var receive = Helper.CreateWhsReceive(client, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, product, 1m);
			receiveLine1.WE_TransactionQuantity = 6m;
			Factory.Save();

			receive.PopulateASNLines();

			receive.RunPreSaveValidation();
			AssertEquals(false, receive.HasErrors);
		}

		public void TestRunPreSaveValidation_ReceiveWithPreventReceivingOversEnabledAndOverReceiving()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = true;
			partRelation.OU_ReceiveOverageTolerancePercent = 500;
			var receive = Helper.CreateWhsReceive(client, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, product, 1m);
			receiveLine1.WE_TransactionQuantity = 7m;
			Factory.Save();

			receive.PopulateASNLines();

			receive.RunPreSaveValidation();
			AssertEquals(true, receive.HasErrors);
			var productSummary = (WhsReceiveProductSummary)receive.ReceiveProductSummaryCollection.FirstOrDefault();
			var error = productSummary?.ReceivedQuantityInfo.GetErrors().Single().Message;
			AssertEquals("Received quantity of product 'P1' exceeds the allowed quantity.", error);
		}

		public void TestRunPreSaveValidation_ReceiveMultipleLinesOfSameProductWithPreventReceivingOversEnabledAndOverReceiving()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = true;
			partRelation.OU_ReceiveOverageTolerancePercent = 100;
			var receive = Helper.CreateWhsReceive(client, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, product, 10m);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, product, 20m);
			receiveLine1.WE_TransactionQuantity = 5m;
			receiveLine2.WE_TransactionQuantity = 56m;
			Factory.Save();

			receive.PopulateASNLines();

			receive.RunPreSaveValidation();
			AssertEquals(true, receive.HasErrors);
			var productSummary = (WhsReceiveProductSummary)receive.ReceiveProductSummaryCollection.FirstOrDefault();
			var error = productSummary?.ReceivedQuantityInfo.GetErrors().Single().Message;
			AssertEquals("Received quantity of product 'P1' exceeds the allowed quantity.", error);
		}

		public void TestRunPreSaveValidation_ReceiveMultipleProductsWithPreventReceivingOversEnabledAndOneProductIsOverReceiving()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product1 = Helper.CreateProduct(client, "P1");
			var product2 = Helper.CreateProduct(client, "P2");
			var partRelation1 = Helper.CreateProductClientRelationShip(client, product1, OrgPartRelation.RelationshipTypes.Owner);
			partRelation1.OU_PreventReceivingOvers = true;
			partRelation1.OU_ReceiveOverageTolerancePercent = 100;
			var partRelation2 = Helper.CreateProductClientRelationShip(client, product2, OrgPartRelation.RelationshipTypes.Owner);
			partRelation2.OU_PreventReceivingOvers = true;
			partRelation2.OU_ReceiveOverageTolerancePercent = 0;
			var receive = Helper.CreateWhsReceive(client, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, product1, 10m);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, product2, 20m);
			receiveLine1.WE_TransactionQuantity = 20m;
			receiveLine2.WE_TransactionQuantity = 21m;
			Factory.Save();

			receive.PopulateASNLines();

			receive.RunPreSaveValidation();
			AssertEquals(true, receive.HasErrors);

			var product1Summary = receive.ReceiveProductSummaryCollection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == "P1");
			AssertEquals(false, product1Summary.HasErrors);

			var product2Summary = receive.ReceiveProductSummaryCollection.Cast<WhsReceiveProductSummary>()
				.FirstOrDefault(x => x.ProductCode == "P2");
			var error = product2Summary?.ReceivedQuantityInfo.GetErrors().Single().Message;
			AssertEquals("Received quantity of product 'P2' exceeds the allowed quantity.", error);
		}

		public void TestRunPreSaveValidation_PreventOverReceive_ProductAndReceiveParamsBothEnabled()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = true;
			partRelation.OU_ReceiveOverageTolerancePercent = 200;

			var whsClientParameterByWarehouse = Helper.CreateWhsClientParameterByWarehouse(client, warehouse);
			whsClientParameterByWarehouse.WY_PreventReceivingOvers = true;
			whsClientParameterByWarehouse.WY_ReceiveOverageTolerancePercent = 500;

			var receive = Helper.CreateWhsReceive(client, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, product, 1m);
			receiveLine1.WE_TransactionQuantity = 4m;
			Factory.Save();

			receive.PopulateASNLines();
			receive.RunPreSaveValidation();

			AssertEquals(true, receive.HasErrors);
			var productSummary = (WhsReceiveProductSummary)receive.ReceiveProductSummaryCollection.FirstOrDefault();
			var error = productSummary?.ReceivedQuantityInfo.GetErrors().Single().Message;
			AssertEquals("Received quantity of product 'P1' exceeds the allowed quantity.", error);
		}

		public void TestRunPreSaveValidation_PreventOverReceive_ProductDisabledAndReceiveParamsEnabled()
		{
			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("RC1", (NoResString)"Receive Category 1");
			WarehouseDataRegistry.Instance.ReceiveCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);

			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = false;

			var whsClientParameterByWarehouse1 = Helper.CreateWhsClientParameterByWarehouse(client, warehouse);
			whsClientParameterByWarehouse1.WY_ReceiveCategory = "RC1";
			whsClientParameterByWarehouse1.WY_PreventReceivingOvers = true;
			whsClientParameterByWarehouse1.WY_ReceiveOverageTolerancePercent = 300;

			var whsClientParameterByWarehouse2 = Helper.CreateWhsClientParameterByWarehouse(client, warehouse);
			whsClientParameterByWarehouse2.WY_ReceiveCategory = string.Empty;
			whsClientParameterByWarehouse2.WY_PreventReceivingOvers = true;
			whsClientParameterByWarehouse2.WY_ReceiveOverageTolerancePercent = 500;

			var receive = Helper.CreateWhsReceive(client, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, product, 1m);
			receiveLine1.WE_TransactionQuantity = 4m;
			Factory.Save();

			receive.PopulateASNLines();
			receive.RunPreSaveValidation();

			AssertEquals(false, receive.HasErrors);
		}

		public void TestRunPreSaveValidation_PreventOverReceive_ProductDisabledAndReceiveParamsEnabled_MatchingByReceiveCategory()
		{
			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("RC1", (NoResString)"Receive Category 1");
			WarehouseDataRegistry.Instance.ReceiveCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);

			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = false;

			var whsClientParameterByWarehouse1 = Helper.CreateWhsClientParameterByWarehouse(client, warehouse);
			whsClientParameterByWarehouse1.WY_ReceiveCategory = "RC1";
			whsClientParameterByWarehouse1.WY_PreventReceivingOvers = true;
			whsClientParameterByWarehouse1.WY_ReceiveOverageTolerancePercent = 100;

			var whsClientParameterByWarehouse2 = Helper.CreateWhsClientParameterByWarehouse(client, warehouse);
			whsClientParameterByWarehouse2.WY_ReceiveCategory = string.Empty;
			whsClientParameterByWarehouse2.WY_PreventReceivingOvers = true;
			whsClientParameterByWarehouse2.WY_ReceiveOverageTolerancePercent = 500;

			var receive = Helper.CreateWhsReceive(client, warehouse);
			receive.WD_ReceiveCategory = "RC1";
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, product, 1m);
			receiveLine1.WE_TransactionQuantity = 4m;
			Factory.Save();

			receive.PopulateASNLines();
			receive.RunPreSaveValidation();

			AssertEquals(true, receive.HasErrors);
			var productSummary = (WhsReceiveProductSummary)receive.ReceiveProductSummaryCollection.FirstOrDefault();
			var error = productSummary?.ReceivedQuantityInfo.GetErrors().Single().Message;
			AssertEquals("Received quantity of product 'P1' exceeds the allowed quantity.", error);
		}

		public void TestRunPreSaveValidation_PreventOverReceive_ProductDisabledAndReceiveParamsEnabled_Exceed()
		{
			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("RC1", (NoResString)"Receive Category 1");
			WarehouseDataRegistry.Instance.ReceiveCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);

			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = false;

			var whsClientParameterByWarehouse1 = Helper.CreateWhsClientParameterByWarehouse(client, warehouse);
			whsClientParameterByWarehouse1.WY_ReceiveCategory = "RC1";
			whsClientParameterByWarehouse1.WY_PreventReceivingOvers = true;
			whsClientParameterByWarehouse1.WY_ReceiveOverageTolerancePercent = 500;

			var whsClientParameterByWarehouse2 = Helper.CreateWhsClientParameterByWarehouse(client, warehouse);
			whsClientParameterByWarehouse2.WY_ReceiveCategory = string.Empty;
			whsClientParameterByWarehouse2.WY_PreventReceivingOvers = true;
			whsClientParameterByWarehouse2.WY_ReceiveOverageTolerancePercent = 100;

			var receive = Helper.CreateWhsReceive(client, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, product, 1m);
			receiveLine1.WE_TransactionQuantity = 4m;
			Factory.Save();

			receive.PopulateASNLines();
			receive.RunPreSaveValidation();

			AssertEquals(true, receive.HasErrors);
			var productSummary = (WhsReceiveProductSummary)receive.ReceiveProductSummaryCollection.FirstOrDefault();
			var error = productSummary?.ReceivedQuantityInfo.GetErrors().Single().Message;
			AssertEquals("Received quantity of product 'P1' exceeds the allowed quantity.", error);
		}

		public void TestRunPreSaveValidation_PreventOverReceive_ProductEnabledAndReceiveParamsDisabled()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = true;
			partRelation.OU_ReceiveOverageTolerancePercent = 500;

			var whsClientParameterByWarehouse = Helper.CreateWhsClientParameterByWarehouse(client, warehouse);
			whsClientParameterByWarehouse.WY_PreventReceivingOvers = false;

			var receive = Helper.CreateWhsReceive(client, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, product, 1m);
			receiveLine1.WE_TransactionQuantity = 3m;
			Factory.Save();

			receive.PopulateASNLines();
			receive.RunPreSaveValidation();

			AssertEquals(false, receive.HasErrors);
		}

		public void TestRunPreSaveValidation_PreventOverReceive_ProductEnabledAndReceiveParamsDisabled_Exceed()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = true;
			partRelation.OU_ReceiveOverageTolerancePercent = 500;

			var whsClientParameterByWarehouse = Helper.CreateWhsClientParameterByWarehouse(client, warehouse);
			whsClientParameterByWarehouse.WY_PreventReceivingOvers = false;

			var receive = Helper.CreateWhsReceive(client, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, product, 1m);
			receiveLine1.WE_TransactionQuantity = 7m;
			Factory.Save();

			receive.PopulateASNLines();
			receive.RunPreSaveValidation();

			AssertEquals(true, receive.HasErrors);
			var productSummary = (WhsReceiveProductSummary)receive.ReceiveProductSummaryCollection.FirstOrDefault();
			var error = productSummary?.ReceivedQuantityInfo.GetErrors().Single().Message;
			AssertEquals("Received quantity of product 'P1' exceeds the allowed quantity.", error);
		}

		public void TestRunPreSaveValidation_PreventOverReceive_ProductAndReceiveParamsBothDisabled()
		{
			var receiveCategories = new SystemDefinableCodeDescriptionBoolCollection();
			receiveCategories.Add("RC1", (NoResString)"Receive Category 1");
			WarehouseDataRegistry.Instance.ReceiveCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, receiveCategories);

			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var product = Helper.CreateProduct(client, "P1");
			var partRelation = Helper.CreateProductClientRelationShip(client, product, OrgPartRelation.RelationshipTypes.Owner);
			partRelation.OU_PreventReceivingOvers = false;

			var whsClientParameterByWarehouse = Helper.CreateWhsClientParameterByWarehouse(client, warehouse);
			whsClientParameterByWarehouse.WY_ReceiveCategory = "RC1";
			whsClientParameterByWarehouse.WY_PreventReceivingOvers = false;

			var receive = Helper.CreateWhsReceive(client, warehouse);
			receive.WD_ReceiveCategory = "RC1";
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, product, 1m);
			receiveLine1.WE_TransactionQuantity = 7m;
			Factory.Save();

			receive.PopulateASNLines();
			receive.RunPreSaveValidation();

			AssertEquals(false, receive.HasErrors);
		}

		public void TestRunPreSaveValidationCore_SynchronisesOffset_ETD()
		{
			TestRunPreSaveValidationCore_SynchronisesOffset(
				(receive, value) => receive.WD_ETD = value,
				(receive) => receive.WD_ETD);
		}

		public void TestRunPreSaveValidationCore_SynchronisesOffset_ETD_FailsIfNoWarehouse()
		{
			TestRunPreSaveValidationCore_SynchronisesOffset_FailsIfNoWarehouse(
				(receive, value) => receive.WD_ETD = value,
				(receive) => receive.WD_ETD);
		}

		public void TestRunPreSaveValidationCore_SynchronisesOffset_ETD_FailsIfDocketInError()
		{
			TestRunPreSaveValidationCore_SynchronisesOffset_FailsIfDocketInError(
				(receive, value) => receive.WD_ETD = value,
				(receive) => receive.WD_ETDInfo);
		}

		public void TestRunPreSaveValidationCore_SynchronisesOffset_ETA()
		{
			TestRunPreSaveValidationCore_SynchronisesOffset(
				(receive, value) => receive.WD_ETA = value,
				(receive) => receive.WD_ETA);
		}

		public void TestRunPreSaveValidationCore_SynchronisesOffset_ETA_FailsIfNoWarehouse()
		{
			TestRunPreSaveValidationCore_SynchronisesOffset_FailsIfNoWarehouse(
				(receive, value) => receive.WD_ETA = value,
				(receive) => receive.WD_ETA);
		}

		public void TestRunPreSaveValidationCore_SynchronisesOffset_ETA_FailsIfDocketInError()
		{
			TestRunPreSaveValidationCore_SynchronisesOffset_FailsIfDocketInError(
				(receive, value) => receive.WD_ETA = value,
				(receive) => receive.WD_ETAInfo);
		}

		public void TestRunPreSaveValidationCore_SynchronisesOffset_ArrivalDate()
		{
			TestRunPreSaveValidationCore_SynchronisesOffset(
				(receive, value) => receive.WD_ArrivalDate = value,
				(receive) => receive.WD_ArrivalDate);
		}

		public void TestRunPreSaveValidationCore_SynchronisesOffset_ArrivalDate_FailsIfNoWarehouse()
		{
			TestRunPreSaveValidationCore_SynchronisesOffset_FailsIfNoWarehouse(
				(receive, value) => receive.WD_ArrivalDate = value,
				(receive) => receive.WD_ArrivalDate);
		}

		public void TestRunPreSaveValidationCore_SynchronisesOffset_ArrivalDate_FailsIfDocketInError()
		{
			TestRunPreSaveValidationCore_SynchronisesOffset_FailsIfDocketInError(
				(receive, value) => receive.WD_ArrivalDate = value,
				(receive) => receive.WD_ArrivalDateInfo);
		}

		void TestRunPreSaveValidationCore_SynchronisesOffset(
			Action<WhsDocket, ZDateTimeOffset> setTestedField,
			Func<WhsDocket, ZDateTimeOffset> getTestedField)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var receive = Helper.CreateWhsReceive(client, warehouse);
			var dateTime = new ZDateTime(2024, 06, 12, 12, 30, 00);

			setTestedField(receive, new ZDateTimeOffset(dateTime, TimeSpan.FromHours(0)));
			receive.RunPreSaveValidation();

			var expectedOffset = warehouse.GetWarehouseBranchDateTimeOffset(dateTime);
			Assert("Docket should not be in error", !receive.HasErrors);
			AssertEquals(
				"Offsets should have same value, including offset component",
				expectedOffset.ToString("dd-MMM-yyyy hh:mm:ss zzz"),
				getTestedField(receive).ToString("dd-MMM-yyyy hh:mm:ss zzz"));
		}

		void TestRunPreSaveValidationCore_SynchronisesOffset_FailsIfNoWarehouse(
			Action<WhsDocket, ZDateTimeOffset> setTestedField,
			Func<WhsDocket, ZDateTimeOffset> getTestedField)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var receive = Helper.CreateWhsReceive(client, warehouse);
			var dateTime = new ZDateTime(2024, 06, 12, 12, 30, 00);

			receive.WD_WW_Whs = ZGuid.Empty;
			setTestedField(receive, new ZDateTimeOffset(dateTime, TimeSpan.FromHours(0)));
			receive.RunPreSaveValidation();

			Assert("Docket should be in error", receive.HasErrors);
			AssertEquals("Do not sychronise Offset if Warehouse is unpopulated.",
				dateTime.ToString("12-Jun-2024 12:30:00 +00:00"),
				getTestedField(receive).ToString("dd-MMM-yyyy hh:mm:ss zzz"));
		}

		void TestRunPreSaveValidationCore_SynchronisesOffset_FailsIfDocketInError(
			Action<WhsDocket, ZDateTimeOffset> setTestedField,
			Func<WhsDocket, ZPropertyInfo> getTestedField)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var client = Helper.CreateClient("C1");
			var receive = Helper.CreateWhsReceive(client, warehouse);
			var dateTime = new ZDateTime(0024, 06, 12, 12, 30, 00);// Wrong datatime

			setTestedField(receive, new ZDateTimeOffset(dateTime, TimeSpan.FromHours(0)));

			receive.RunPreSaveValidation();
			var dateTimeOffSetInfo = getTestedField(receive);
			var dateTimeOffSet = (ZDateTimeOffset)dateTimeOffSetInfo.Value;
			Assert("Docket should be in error", dateTimeOffSetInfo.HasErrors());
			AssertEquals("Do not sychronise Offset if this field is in Error.",
				"12-Jun-0024 12:30 +00:00",
				dateTimeOffSet.ToString("dd-MMM-yyyy hh:mm zzz"));
		}

		public override void TestRunPreSaveValidationCore_SynchronisesOffset_UpdatesDocketLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);

			var dateTime = new ZDateTime(2024, 06, 12, 12, 30, 00);

			receiveLine.WE_RequiredByDate = new ZDateTimeOffset(dateTime, TimeSpan.FromHours(0));
			receiveLine.WE_AdjustmentArrivalDate = new ZDateTimeOffset(dateTime, TimeSpan.FromHours(0));

			receive.RunPreSaveValidation();

			Assert("Docket should not be in error", !receive.HasErrors);

			var expectedOffset = data.Whs1.GetWarehouseBranchDateTimeOffset(dateTime);
			AssertEquals(
				"Offsets should have same value, including offset component",
				expectedOffset.ToString("dd-MMM-yyyy hh:mm:ss zzz"),
				receiveLine.WE_RequiredByDate.ToString("dd-MMM-yyyy hh:mm:ss zzz"));

			AssertEquals(
				"Offsets should have same value, including offset component",
				expectedOffset.ToString("dd-MMM-yyyy hh:mm:ss zzz"),
				receiveLine.WE_AdjustmentArrivalDate.ToString("dd-MMM-yyyy hh:mm:ss zzz"));
		}

		public void TestRunPreSaveValidation_ReturnReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "");
			receive.WD_DocketSubType = ReceiveType.Codes.Returns;
			receive.WD_ExternalReference = "Order123";

			AssertEquals(0, receive.Lines.Count);
			AssertEquals(receive.WD_WD_ParentDocket, ZGuid.Empty);

			receive.RunPreSaveValidation();

			AssertEquals(2, receive.Lines.Count);
			AssertEquals(receive.WD_WD_ParentDocket, order.PK);

			AssertReturnReceiveLine(receive.Lines.Single(line => line.WE_OP == data.Part1.PK), data.Part1.PK, 10m);
			AssertReturnReceiveLine(receive.Lines.Single(line => line.WE_OP == data.Part2.PK), data.Part2.PK, 20m);
		}

		public void TestRunPreSaveValidation_ReturnReceive_WithLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "");
			receive.WD_DocketSubType = ReceiveType.Codes.Returns;
			receive.WD_ExternalReference = "Order123";
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);

			AssertEquals(1, receive.Lines.Count);
			AssertEquals(receive.WD_WD_ParentDocket, ZGuid.Empty);

			receive.RunPreSaveValidation();

			AssertEquals(1, receive.Lines.Count);
			AssertEquals(receive.WD_WD_ParentDocket, order.PK);
		}

		public void TestRunPreSaveValidation_ReturnReceive_RF()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			WhsEnvironment.IsRF = true;
			try
			{
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "");
				receive.WD_DocketSubType = ReceiveType.Codes.Returns;
				receive.WD_ExternalReference = "Order123";

				AssertEquals(0, receive.Lines.Count);
				AssertEquals(receive.WD_WD_ParentDocket, ZGuid.Empty);

				receive.RunPreSaveValidation();

				AssertEquals(0, receive.Lines.Count);
				AssertEquals(receive.WD_WD_ParentDocket, order.PK);
			}
			finally
			{
				WhsEnvironment.IsRF = false;
			}
		}

		public void TestRunPreSaveValidation_ReturnReceive_WithAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Factory.Save();

			var inventory = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var expiryDate = ZDate.Today.AddDays(1);
			var packingDate = ZDate.Today;
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(inventory, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "PLT-1", expiryDate, packingDate, "A", "B", "C", "S1", "");
			inventory.AllocateLocationsWithMock();
			inventory.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "");
			receive.WD_DocketSubType = ReceiveType.Codes.Returns;
			receive.WD_ExternalReference = "Order123";

			AssertEquals(0, receive.Lines.Count);
			AssertEquals(receive.WD_WD_ParentDocket, ZGuid.Empty);

			receive.RunPreSaveValidation();

			AssertEquals(1, receive.Lines.Count);
			AssertEquals(receive.WD_WD_ParentDocket, order.PK);

			AssertReturnReceiveLine(receive.Lines.Single(), data.Part1.PK, 1m, packingDate, expiryDate, "A", "B", "C", "S1");
		}

		public void TestRunPreSaveValidation_ReturnReceive_OrderPartiallyReturned()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "");
			returnReceive1.WD_DocketSubType = ReceiveType.Codes.Returns;
			returnReceive1.WD_ExternalReference = "Order123";
			Helper.CreateWhsReceiveLine(returnReceive1, data.Part1, 5m);
			returnReceive1.WD_WD_ParentDocket = order.PK;
			returnReceive1.RunPreSaveValidation();
			Factory.Save();

			AssertEquals(returnReceive1.WD_WD_ParentDocket, order.PK);
			AssertEquals(1, returnReceive1.Lines.Count);

			var returnReceive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "");
			returnReceive2.WD_DocketSubType = ReceiveType.Codes.Returns;
			returnReceive2.WD_ExternalReference = "Order123";
			returnReceive2.RunPreSaveValidation();

			AssertEquals(returnReceive2.WD_WD_ParentDocket, order.PK);
			AssertEquals(2, returnReceive2.Lines.Count);

			AssertReturnReceiveLine(returnReceive2.Lines.Single(line => line.WE_OP == data.Part1.PK), data.Part1.PK, 5m);
			AssertReturnReceiveLine(returnReceive2.Lines.Single(line => line.WE_OP == data.Part2.PK), data.Part2.PK, 20m);
		}

		public void TestRunPreSaveValidation_ReturnReceive_OrderPartiallyReturned_OneProductFullyReturned()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "");
			returnReceive1.WD_DocketSubType = ReceiveType.Codes.Returns;
			returnReceive1.WD_ExternalReference = "Order123";
			Helper.CreateWhsReceiveLine(returnReceive1, data.Part1, 10m);
			returnReceive1.WD_WD_ParentDocket = order.PK;
			returnReceive1.RunPreSaveValidation();
			Factory.Save();

			AssertEquals(returnReceive1.WD_WD_ParentDocket, order.PK);
			AssertEquals(1, returnReceive1.Lines.Count);

			var returnReceive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "");
			returnReceive2.WD_DocketSubType = ReceiveType.Codes.Returns;
			returnReceive2.WD_ExternalReference = "Order123";
			returnReceive2.RunPreSaveValidation();

			AssertEquals(returnReceive2.WD_WD_ParentDocket, order.PK);
			AssertEquals(1, returnReceive2.Lines.Count);

			AssertReturnReceiveLine(returnReceive2.Lines.Single(), data.Part2.PK, 20m);
		}

		public void TestRunPreSaveValidation_ReturnReceive_OrderFullyReturned()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "");
			returnReceive1.WD_DocketSubType = ReceiveType.Codes.Returns;
			returnReceive1.WD_ExternalReference = "Order123";
			Helper.CreateWhsReceiveLine(returnReceive1, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(returnReceive1, data.Part2, 20m);
			returnReceive1.WD_WD_ParentDocket = order.PK;
			returnReceive1.RunPreSaveValidation();
			Factory.Save();

			AssertEquals(returnReceive1.WD_WD_ParentDocket, order.PK);
			AssertEquals(2, returnReceive1.Lines.Count);

			var returnReceive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "");
			returnReceive2.WD_DocketSubType = ReceiveType.Codes.Returns;
			returnReceive2.WD_ExternalReference = "Order123";
			returnReceive2.RunPreSaveValidation();

			AssertEquals(returnReceive2.WD_WD_ParentDocket, ZGuid.Empty);
			AssertEquals(0, returnReceive2.Lines.Count);
			AssertHasError(returnReceive2.WD_ExternalReferenceInfo, "This reference points to a departed order that has already been fully returned or is in the process of being fully returned.");
		}

		public void TestRunPreSaveValidation_ReturnReceive_NoOrderToReturn()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "");
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			returnReceive.WD_ExternalReference = "SomeRandomNumber";

			AssertEquals("Precondition", true, returnReceive.WD_WD_ParentDocket.IsEmpty);
			returnReceive.RunPreSaveValidation();

			AssertEquals("Precondition", true, returnReceive.WD_WD_ParentDocket.IsEmpty);
			AssertEquals(0, returnReceive.Lines.Count);
		}

		void AssertReturnReceiveLine(WhsDocketLine line, ZGuid productPK, ZDecimal qty)
			=> AssertReturnReceiveLine(line, productPK, qty, ZDate.Empty, ZDate.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

		void AssertReturnReceiveLine(WhsDocketLine line, ZGuid productPK, ZDecimal qty,
			ZDate packingDate, ZDate expiryDate,
			ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber)
		{
			CombineAssertions(() =>
			{
				AssertEquals(productPK, line.WE_OP);
				AssertEquals(qty, line.WE_ClientOrderedUnits);
				AssertEquals(packingDate, line.WE_PackingDate);
				AssertEquals(expiryDate, line.WE_ExpiryDate);
				AssertEquals(partAttrib1, line.WE_PartAttrib1);
				AssertEquals(partAttrib2, line.WE_PartAttrib2);
				AssertEquals(partAttrib3, line.WE_PartAttrib3);
				AssertEquals(serialNumber, line.WE_SerialNumber);
			});
		}

		public void TestRunPreSaveValidation_ReturnReceive_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Factory.Save();

			var products = new List<OrgSupplierPart>();
			for (var i = 0; i < 10; i++)
			{
				var product = Helper.CreateProduct(data.Org1, $"OP{i}");
				products.Add(product);
			}
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			foreach(var product in products)
			{
				Helper.CreateWhsReceiveLine(receive, product, 10m);
			}
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			foreach (var product in products)
			{
				Helper.CreateWhsOrderLine(order, product, 10m);
			}

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "");
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			Factory.Save();

			AssertEquals("Precondition", ZGuid.Empty, returnReceive.WD_WD_ParentDocket);
			AssertEquals("Precondition", 0, returnReceive.Lines.Count);

			var expectedDBHits = new Dictionary<string, int>
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
				{ WhsBOMInventoryPivotSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 4 },
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
			};

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var returnReceiveInNewFactory = newFactory.Load<WhsReceive>(returnReceive.PK);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, newFactory))
			{
				returnReceiveInNewFactory.WD_ExternalReference = "O1";
				returnReceiveInNewFactory.RunPreSaveValidation();
				AssertEquals(10, returnReceiveInNewFactory.Lines.Count);
				AssertEquals(order.PK, returnReceiveInNewFactory.WD_WD_ParentDocket);
			}

			AssertNoExceptionThrown(newFactory.Save);
		}

		public void TestRunPreSaveValidation_SerialNumberPivot()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var today = ZDateTimeOffset.Today;
				var numberOfDocketLines = 3;
				var numberOfSerialNumberPerLine = 5;
				var numberOfAllSerialNumbers = numberOfDocketLines * numberOfSerialNumberPerLine;
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, numberOfAllSerialNumbers, true, false);
				for (var n = 0; n < numberOfDocketLines; n++)
				{
					var receiveLine = receive.Lines[0];
					for (var i = 1; i <= numberOfSerialNumberPerLine; i++)
					{
						var pivot = receiveLine.SerialNumbers.AddNew();
						pivot.SerialNumberValue = $"SN{n}{i}";
					}
				}

				var expectedDbHits = new Dictionary<string, int>
				{
					{ WhsSerialNumberSchema.Constants.TableName, 1 },
					{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				};
				Factory.ResetDatabaseLoadCount();

				var serialNumberUniquenessChecker = new SerialNumberUniquenessChecker(receive);
				using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, Factory))
				using (RowFactory.SetCachedTables())
				{
					foreach (var snp in receive.Lines.Cast<WhsReceiveLine>().SelectMany(l => l.SerialNumbers.Cast<WhsSerialNumberPivot>()))
					{
						receive.RunPreSaveValidation();
					}
				}
			}
		}

		#endregion

		#region RunPreFinaliseValidationCore

		#region TestCheckLocationString_AllLinesHaveSameAreaType

		public void TestCheckLocationString_AllLinesHaveSameAreaType()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var exciseArea = Helper.CreateArea(data.Whs1, "AREA2", AreaTypes.Codes.Excise);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			location1.PutawayArea.WA_AreaType = AreaTypes.Codes.Bonded;
			location2.WLV_WA_PutawayArea = exciseArea.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1).InDocketLine;
			receiveLine1.CustomsData.WB_EntryKey = "ABC";
			Factory.Save();

			// ensuring it will work for records in memory too
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1).InDocketLine;
			receiveLine2.CustomsData.WB_EntryKey = "ABC";

			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				AssertNoRowError(receive, WhsReceive.CannotPutawayIntoDifferentAreaTypesErrorMsg);

				receiveLine2.LocationString = "A-2";
				receive.FinaliseDocketWithoutUserConfirmation();
				AssertHasRowError("Receive should get error during finalise if inventories are in different area types.", receive, WhsReceive.CannotPutawayIntoDifferentAreaTypesErrorMsg);
				AssertEquals("Receive should not be finalised.", false, receive.IsFinalised);

				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
				var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1).InDocketLine;

				receiveInNewFactory.FinaliseDocketWithoutUserConfirmation();
				AssertNoRowError(receiveInNewFactory, WhsReceive.CannotPutawayIntoDifferentAreaTypesErrorMsg);
				AssertEquals("Receive should be finalised.", true, receiveInNewFactory.IsFinalised);
			}
		}

		#endregion

		#endregion

		#region Validation

		protected override Type GetExpectedValidationType()
		{
			return typeof(WhsReceiveValidation);
		}

		#endregion

		#region Lookups

		protected override Type GetExpectedLookupsType()
		{
			return typeof(WhsReceiveLookups);
		}

		#endregion

		#region Properties

		#region Bool

		#region ReadOnly

		protected override void TestStandardReadOnlyCore(Func<WhsReceive, bool> getReadOnly, string name, WhsReceive docket, bool readOnlyWhenDocketHasLines)
		{
			docket.WD_DocketStatus = DocketStatus.Codes.Putaway;
			AssertEquals($"{name} should be readonly if docket is Putaway", true, getReadOnly(docket));
		}

		#endregion

		#region IsPuttingAway

		public virtual void TestIsPuttingAway()
		{
			var receive = GetNewBusinessObject();
			receive.WD_DocketStatus = DocketStatus.Codes.New;
			AssertEquals(false, receive.IsPuttingAway);
			receive.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals(false, receive.IsPuttingAway);
			receive.WD_DocketStatus = DocketStatus.Codes.Putaway;
			AssertEquals(true, receive.IsPuttingAway);
			receive.WD_DocketStatus = DocketStatus.Codes.Finalised;
			AssertEquals(false, receive.IsPuttingAway);
			receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertEquals(false, receive.IsPuttingAway);
			receive.WD_DocketStatus = ZString.Empty;
			AssertEquals(false, receive.IsPuttingAway);
		}

		#endregion

		#region IsPuttingAwayOrFinalised

		public virtual void TestIsPuttingAwayOrFinalised()
		{
			var receive = GetNewBusinessObject();
			receive.WD_DocketStatus = DocketStatus.Codes.New;
			AssertEquals(false, receive.IsPuttingAwayOrFinalised);
			receive.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals(false, receive.IsPuttingAwayOrFinalised);
			receive.WD_DocketStatus = DocketStatus.Codes.Putaway;
			AssertEquals(true, receive.IsPuttingAwayOrFinalised);
			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals(true, receive.IsPuttingAwayOrFinalised);
			receive.WD_FinalisedDate = ZDateTimeOffset.Empty;

			receive.WD_DocketStatus = DocketStatus.Codes.Entered;
			receive.CancelReactivateDocket();
			AssertEquals(false, receive.IsPuttingAwayOrFinalised);
			receive.WD_DocketStatus = ZString.Empty;
			AssertEquals(false, receive.IsPuttingAwayOrFinalised);
		}

		#endregion

		#region IsPostFinalizeEditAllowed

		public override void TestIsPostFinalizeEditAllowed()
		{
			Env.Security.WhsReceivePostFinaliseEdit.IsAllowed = false;
			AssertEquals(false, Receive.IsPostFinalizeEditAllowed);

			Env.Security.WhsReceivePostFinaliseEdit.IsAllowed = true;
			AssertEquals(true, Receive.IsPostFinalizeEditAllowed);

			Helper.AddOnePostedChargeLine(Receive);
			AssertEquals(false, Receive.IsPostFinalizeEditAllowed);
		}

		#endregion

		#region IsCreatedFromWorkOrder

		public void TestIsCreatedFromWorkOrder()
		{
			AssertEquals(false, Receive.IsCreatedFromWorkOrder);

			Receive.WD_WD_ParentDocket = Factory.New<WhsWorkOrder>().PK;
			AssertEquals(true, Receive.IsCreatedFromWorkOrder);
		}

		#endregion

		#region TestIsCreatedFromPickByBOM

		public void TestIsCreatedFromPickByBOM_InDatabase_True()
		{
			TestIsCreatedFromPickByBOM_InDatabase_Core(isInDatabase: true);
		}

		public void TestIsCreatedFromPickByBOM_InDatabase_False()
		{
			TestIsCreatedFromPickByBOM_InDatabase_Core(isInDatabase: false);
		}

		void TestIsCreatedFromPickByBOM_InDatabase_Core(bool isInDatabase)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 5m);
			var pick = Helper.CreatePickNew(order);
			var wheelOrderLine = orderLine1.ChildComponentLines.Single();
			var wheelPickLine = wheelOrderLine.PickLines.Single();
			var kitPickLine = orderLine1.PickLines.Single();

			if (isInDatabase)
			{
				Factory.Save();
			}

			var createdReceiveLine = kitPickLine.InventoryLine;
			var createdReceive = createdReceiveLine.Docket;
			AssertEquals(true, createdReceive.IsCreatedFromPickByBOM);
		}

		#endregion

		#region TestIsFinalisedOrCancelledCore

		protected override void TestIsFinalisedOrCancelledCore(WhsReceive receive)
		{
			receive.WD_FinalisedDate = ZDateTimeOffset.Empty;
			receive.WD_DocketStatus = DocketStatus.Codes.Entered;
			AssertEquals(false, receive.IsFinalisedOrCancelled);
			receive.WD_WP_ParentPickForReceive = ZGuid.NewZGuid();
			AssertEquals(true, receive.IsFinalisedOrCancelled);
		}

		#endregion

		#region IsLocationSetOnAllInventoryLines

		public void TestIsLocationSetOnAllInventoryLines()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 0, 20, 40 }, false);
			foreach (WhsInventoryView inventory in data.Receive11.Inventory)
			{
				inventory.WI_WL = data.Whs1.DefaultLocation.PK;
			}
			AssertEquals(true, data.Receive11.IsLocationSetOnAllInventoryLines);

			data.Receive11.Inventory[0].LocationString = ZString.Empty;
			data.Receive11.Inventory[0].WI_ExpectedReceiptQuantity = 1;
			data.Receive11.Inventory[0].WI_InDocketLineUnits = 0;
			AssertEquals(true, data.Receive11.IsLocationSetOnAllInventoryLines);
			AssertEquals("AllInventoryLinesHaveLocationSet should set the location for under lines with zero quantity to the warehouse default location", data.Whs1.DefaultLocation.PK, data.Receive11.Inventory[0].WI_WL);

			data.Receive11.Inventory[1].LocationString = ZString.Empty;
			AssertEquals(false, data.Receive11.IsLocationSetOnAllInventoryLines);
		}

		#endregion

		#region TestHasInventoryWithTemporaryProducts

		public void TestHasInventoryWithTemporaryProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			WhsReceive receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			AssertEquals(false, receive.HasInventoryWithTemporaryProducts);

			WhsInventoryView inventory = receive.Lines.AddNew().Inventory[0];
			AssertEquals(false, receive.HasInventoryWithTemporaryProducts);

			inventory.WI_OP = data.Part1.PK;
			AssertEquals(false, receive.HasInventoryWithTemporaryProducts);

			inventory.WI_OP = ZGuid.Invalid;
			inventory.WI_OP_PartNum = "NewProduct";
			AssertEquals(true, receive.HasInventoryWithTemporaryProducts);
		}

		#endregion

		#region TestHasInventoryWithStockAndTemporaryProducts

		public void TestHasInventoryWithStockAndTemporaryProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			WhsReceive receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			AssertEquals("Precondition", false, receive.HasInventoryWithStockAndTemporaryProducts);

			WhsInventoryView inventory1 = receive.Lines.AddNew().Inventory[0]; // inventory without products
			AssertEquals(false, receive.HasInventoryWithStockAndTemporaryProducts);

			WhsInventoryView inventory2 = receive.Lines.AddNew().Inventory[0]; // inventory with existing product
			inventory2.WI_OP = data.Part1.PK;
			AssertEquals(false, receive.HasInventoryWithStockAndTemporaryProducts);

			WhsInventoryView inventory3 = receive.Lines.AddNew().Inventory[0]; // inventory with temporary product but without stock
			inventory3.WI_OP = ZGuid.Invalid;
			inventory3.WI_OP_PartNum = "NewProduct";
			AssertEquals(false, receive.HasInventoryWithStockAndTemporaryProducts);

			WhsInventoryView inventory4 = receive.Lines.AddNew().Inventory[0]; // inventory with temporary product with stock but without UQ
			inventory4.WI_OP = ZGuid.Invalid;
			inventory4.WI_OP_PartNum = "NewProduct";
			inventory4.WI_InDocketLineUnits = 5m;
			inventory4.WI_UnitsUQ = "";
			AssertEquals(false, receive.HasInventoryWithStockAndTemporaryProducts);

			WhsInventoryView inventory5 = receive.Lines.AddNew().Inventory[0]; // inventory with temporary product with stock
			inventory5.WI_OP = ZGuid.Invalid;
			inventory5.WI_OP_PartNum = "NewProduct";
			inventory5.WI_InDocketLineUnits = 5m;
			inventory5.WI_UnitsUQ = "UNT";
			AssertEquals(true, receive.HasInventoryWithStockAndTemporaryProducts);
		}

		#endregion

		#region TestStartedReceiving

		public void TestStartedReceiving()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			AssertEquals("StartedReceiving should be false if WD_StartedReceivingTimeUtc is null", false, receive.StartedReceiving);

			receive.WD_StartedReceivingTimeUtc = ZDateTime.Now;
			AssertEquals("StartedReceiving should be true if WD_StartedReceivingTimeUtc is not null", true, receive.StartedReceiving);
		}

		#endregion

		#region TestIsGeneratingSericalNumbers

		public void TestIsGeneratingSericalNumbers()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			AssertEquals(false, receive.IsGeneratingSerialNumbers);

			using (new SemaphoreManager(receive.GenerateSerialNumbersSemaphore))
			{
				AssertEquals(true, receive.IsGeneratingSerialNumbers);
			}

			AssertEquals(false, receive.IsGeneratingSerialNumbers);
		}

		#endregion

		#region TestStartedReceiving

		#region TestStartedReceiving_PopulateASNLine_NoReceiveLines

		public void TestStartedReceiving_PopulateASNLine_NoReceiveLines()
		{
			var client = Helper.CreateClient("Client");
			var whs = Helper.CreateWarehouse("Whs");
			var receive = Helper.CreateWhsReceive(client, whs);
			Factory.Save();

			AssertEquals("Precondition - receive.StartedReceiving", false, receive.StartedReceiving);
			receive.PopulateASNLines();
			AssertEquals("Calling PopulateASNLines should mark receive as Started Receiving.", true, receive.StartedReceiving);
			AssertEquals("Calling PopulateASNLines on a receive with no receivelines should produce no ASN lines.", 0, receive.AsnLines.Count);
		}

		#endregion

		#region TestStartedReceiving_PopulateASNLine_WithReceiveLines

		public void TestStartedReceiving_PopulateASNLine_WithReceiveLines()
		{
			var client = Helper.CreateClient("Client");
			var whs = Helper.CreateWarehouse("Whs");
			var product = Helper.CreateProduct("Product", client);
			var receive = Helper.CreateWhsReceive(client, whs);

			Helper.CreateWhsReceiveLine(receive, product, 10m);
			Helper.CreateWhsReceiveLine(receive, product, 15m);
			Helper.CreateWhsReceiveLine(receive, product, 20m);
			Factory.Save();
			AssertEquals("Precondition - receive.WD_StartedReceivingTimeUtc", false, receive.StartedReceiving);
			receive.PopulateASNLines();

			AssertEquals("Calling PopulateASNLines should mark receive as Started Receiving.", true, receive.StartedReceiving);
			AssertEquals("Calling PopulateASNLines on a receive with receivelines should produce matching ASN lines.", 3, receive.AsnLines.Count);
		}

		#endregion

		#region TestStartedReceiving_PopulateASNLine_UnsavedChanges

		public void TestStartedReceiving_PopulateASNLine_UnsavedChanges()
		{
			var client = Helper.CreateClient("Client");
			var whs = Helper.CreateWarehouse("Whs");
			var product = Helper.CreateProduct("Product", client);
			var receive = Helper.CreateWhsReceive(client, whs);
			Helper.CreateWhsReceiveLine(receive, product, 10m);

			AssertEquals("Precondition - receive.WD_StartedReceivingTimeUtc", false, receive.StartedReceiving);
			receive.PopulateASNLines();

			AssertEquals("Calling PopulateASNLines without saving changes should not mark receive as Started Receiving.", false, receive.StartedReceiving);
			AssertEquals("Calling PopulateASNLines on a receive with unsaved changes should produce no ASN lines.", 0, receive.AsnLines.Count);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(ReceiveErrorTypes.CreateASNLineRequiresReceiptToBeSaved));
		}

		#endregion

		#region TestStartedReceiving_PopulateASNLine_FinalisedOrCancelledDocket

		public void TestStartedReceiving_PopulateASNLine_FinalisedOrCancelledDocket()
		{
			var client = Helper.CreateClient("Client");
			var whs = Helper.CreateWarehouse("Whs", "A", 4, 4);
			var product = Helper.CreateProduct("Product", client);
			var receive1 = Helper.CreateWhsReceive(client, whs, "receive1");
			var receive2 = Helper.CreateWhsReceive(client, whs, "receive2");
			Helper.CreateWhsReceiveLine(receive1, product, 10m);
			Helper.CreateWhsReceiveLine(receive2, product, 10m);
			receive1.AllocateLocationsWithMock();
			Factory.Save();

			receive1.FinaliseDocket();
			receive2.CancelReactivateDocket();
			Factory.Save();

			AssertEquals("Precondition - receive.WD_StartedReceivingTimeUtc", false, receive1.StartedReceiving);
			AssertEquals("Precondition - receive.WD_StartedReceivingTimeUtc", false, receive2.StartedReceiving);
			AssertEquals("Precondition - receive.WD_DocketStatus", DocketStatus.Codes.Finalised, receive1.WD_DocketStatus);
			AssertEquals("Precondition - receive.WD_DocketStatus", DocketStatus.Codes.Cancelled, receive2.WD_DocketStatus);

			receive1.PopulateASNLines();
			AssertEquals("Calling PopulateASNLines on a finalised docket should not mark receive as Started Receiving.", false, receive1.StartedReceiving);
			AssertEquals(true, ((NotificationBuffer)receive1.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));

			receive2.PopulateASNLines();
			AssertEquals("Calling PopulateASNLines on a cancelled docket should not mark receive as Started Receiving.", false, receive2.StartedReceiving);
			AssertEquals(true, ((NotificationBuffer)receive2.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		#endregion

		#region TestStartedReceiving_PopulateASNLine_ReceiveCreatedFromWorkOrder

		public void TestStartedReceiving_PopulateASNLine_ReceiveCreatedFromWorkOrder()
		{
			var client = Helper.CreateClient("Client");
			var whs = Helper.CreateWarehouse("Whs", "A", 4, 4);
			var product = Helper.CreateProduct("Product", client);
			var receive = Helper.CreateWhsReceive(client, whs, "receive1");
			Helper.CreateWhsReceiveLine(receive, product, 10m);
			Factory.Save();

			var workOrder = Factory.New<WhsWorkOrder>();
			receive.WD_WD_ParentDocket = workOrder.PK;
			receive.HasChanges = false;
			receive.PopulateASNLines();
			AssertEquals("Calling PopulateASNLines on a finalised docket should not mark receive as Started Receiving.", false, receive.StartedReceiving);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(ReceiveErrorTypes.CannotPerformThisOperationBecauseReceiveWasCreatedFromWorkOrder));
		}

		#endregion

		#region TestStartedReceiving_PopulateASNLine_ReceiveCreatedFromPickByBOM

		public void TestStartedReceiving_PopulateASNLine_ReceiveCreatedFromPickByBOM()
		{
			var client = Helper.CreateClient("Client");
			var whs = Helper.CreateWarehouse("Whs", "A", 4, 4);
			var product = Helper.CreateProduct("Product", client);
			var receive = Helper.CreateWhsReceive(client, whs, "receive1");
			Helper.CreateWhsReceiveLine(receive, product, 10m);
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			receive.WD_WP_ParentPickForReceive = pick.PK;
			receive.HasChanges = false;
			receive.PopulateASNLines();
			AssertEquals("Calling PopulateASNLines on a finalised docket should not mark receive as Started Receiving.", false, receive.StartedReceiving);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(ReceiveErrorTypes.CannotPerformThisOperationBecauseReceiveWasCreatedFromPickOrder));
		}

		#endregion

		#endregion

		#endregion

		#region SubTypeDesc

		public override void TestSubTypeDesc()
		{
			Docket.WD_DocketSubType = CodeLists.ReceiveType.Codes.Returns;
			AssertEquals(CodeLists.ReceiveType.Descriptions.Returns, Docket.SubTypeDesc);
		}

		#endregion

		#region WD_ArrivalDate

		public void TestWD_ArrivalDate()
		{
			var arrivalDate = ZDateTimeOffset.Today.AddMonths(-1);
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Factory.New<WhsReceive>();
			receive.WD_OH_Client = data.Org1.PK;
			receive.WD_WW_Whs = data.Whs1.PK;

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1);

			Factory.Save();

			AssertEquals("Precondition", 3, receive.Inventory.Count);
			foreach (WhsInventoryView inv in receive.Inventory)
			{
				AssertEquals(ZDateTimeOffset.Empty, inv.WI_ArrivalDate);
				AssertEquals(ZDateTimeOffset.Empty, inv.InDocketLine.WE_AdjustmentArrivalDate);
				AssertEquals(InventoryStatus.Codes.Pending, inv.WI_InventoryStatus);
			}

			receive.WD_ArrivalDate = arrivalDate;
			foreach (WhsInventoryView inv in receive.Inventory)
			{
				AssertEquals("WD_ArrivalDate setter should set inventory arrival date", arrivalDate, inv.WI_ArrivalDate);
				AssertEquals("WD_ArrivalDate setter should set docket line arrival date", arrivalDate, inv.InDocketLine.WE_AdjustmentArrivalDate);
				AssertEquals("WD_ArrivalDate setter should set inventory status", InventoryStatus.Codes.Arrived, inv.WI_InventoryStatus);
			}

			receive.Inventory[0].WI_InventoryStatus = InventoryStatus.Codes.Held;
			receive.Inventory[0].OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			receive.Inventory[1].WI_InventoryStatus = InventoryStatus.Codes.Held;
			receive.Inventory[1].OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;

			receive.WD_DocketStatus = DocketStatus.Codes.Finalised;
			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			receive.WD_ArrivalDate = ZDateTimeOffset.Empty;
			AssertEquals(ZDateTimeOffset.Empty, receive.Inventory[0].WI_ArrivalDate);
			AssertEquals(ZDateTimeOffset.Empty, receive.Inventory[1].WI_ArrivalDate);
			AssertEquals(ZDateTimeOffset.Empty, receive.Inventory[2].WI_ArrivalDate);
			AssertEquals(ZDateTimeOffset.Empty, receive.Inventory[0].InDocketLine.WE_AdjustmentArrivalDate);
			AssertEquals(ZDateTimeOffset.Empty, receive.Inventory[1].InDocketLine.WE_AdjustmentArrivalDate);
			AssertEquals(ZDateTimeOffset.Empty, receive.Inventory[2].InDocketLine.WE_AdjustmentArrivalDate);
			AssertEquals("Should not change status if finalised", InventoryStatus.Codes.Held, receive.Inventory[0].WI_InventoryStatus);
			AssertEquals("Should not change HeldCode if finalised", InventoryHoldCodes.Codes.Damaged, receive.Inventory[0].WI_HeldCode);
			AssertEquals("Should not change status if finalised", InventoryStatus.Codes.Held, receive.Inventory[1].WI_InventoryStatus);
			AssertEquals("Should not change HeldCode if finalised", InventoryHoldCodes.Codes.Held, receive.Inventory[1].WI_HeldCode);
			AssertEquals("Should not change status if finalised", InventoryStatus.Codes.Arrived, receive.Inventory[2].WI_InventoryStatus);
			AssertEquals("Should not change HeldCode if finalised", ZString.Empty, receive.Inventory[2].WI_HeldCode);
		}

		public void TestWD_ArrivalDate_PopulatedFromDocketOnSave()
		{
			var year = ZDateTime.Now.Year;

			var arrivalDate = new ZDateTimeOffset(year, 01, 12);
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Factory.New<WhsReceive>();
			receive.WD_OH_Client = data.Org1.PK;
			receive.WD_WW_Whs = data.Whs1.PK;
			receive.WD_ArrivalDate = arrivalDate;

			var inventory = receive.Lines.AddNew().Inventory[0];
			inventory.WI_OH_Client = receive.WD_OH_Client;
			inventory.WI_OP = data.Part1.PK;
			inventory.WI_InDocketLineUnits = 10m;
			AssertEquals(1, receive.Lines.Count);
			AssertEquals(arrivalDate, receive.WD_ArrivalDate);
			AssertEquals(receive.Inventory[0].WI_ArrivalDate, receive.Lines[0].WE_AdjustmentArrivalDate);
		}

		public void TestWD_ArrivalDate_DockDoorLocations()
		{
			var arrivalDate = ZDateTimeOffset.Today.AddMonths(-1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var nonDockDoorlocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var receive = Factory.New<WhsReceive>();
			receive.WD_OH_Client = data.Org1.PK;
			receive.WD_WW_Whs = data.Whs1.PK;
			var inventoryWithDockDoorLocation = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1);
			inventoryWithDockDoorLocation.InDocketLine.WE_WL = dockDoorLocation.PK;
			inventoryWithDockDoorLocation.InDocketLine.WE_AdjustmentArrivalDate = arrivalDate;  // Since we need to keep empty date in docket.

			var inventoryWithPutawayLocation = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1);
			inventoryWithPutawayLocation.InDocketLine.WE_WL = nonDockDoorlocation.PK;
			inventoryWithPutawayLocation.InDocketLine.WE_AdjustmentArrivalDate = arrivalDate;

			var inventoryWithoutDockDoorLocation = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1);

			Factory.Save();

			AssertEquals("Precondition", 3, receive.Inventory.Count);
			AssertEquals(InventoryStatus.Codes.Received, inventoryWithDockDoorLocation.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Putaway, inventoryWithPutawayLocation.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Pending, inventoryWithoutDockDoorLocation.WI_InventoryStatus);

			receive.WD_ArrivalDate = arrivalDate;
			AssertEquals(InventoryStatus.Codes.Received, inventoryWithDockDoorLocation.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Putaway, inventoryWithPutawayLocation.WI_InventoryStatus);
			AssertEquals("Eventhough the inventory doesnot have a dock door location yet it may get a one later, therefore leave that inventory as pending.", InventoryStatus.Codes.Arrived, inventoryWithoutDockDoorLocation.WI_InventoryStatus);

			receive.WD_ArrivalDate = ZDateTimeOffset.Empty;
			AssertEquals(InventoryStatus.Codes.Received, inventoryWithDockDoorLocation.WI_InventoryStatus);
			AssertEquals("Inventory gets dock door location and putaway location from putaway transfers therefore status should not be changed.", InventoryStatus.Codes.Putaway, inventoryWithPutawayLocation.WI_InventoryStatus);
			AssertEquals(InventoryStatus.Codes.Pending, inventoryWithoutDockDoorLocation.WI_InventoryStatus);
		}

		public void TestWD_ArrivalDate_ChangingOffset()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			receive.WD_ArrivalDate = new ZDateTimeOffset(new ZDateTime(2024, 06, 12, 12, 30, 00), TimeSpan.FromHours(10));
			AssertEquals("Correct Offset is set.", TimeSpan.FromHours(10), receive.WD_ArrivalDate.Offset);

			receive.WD_ArrivalDate = new ZDateTimeOffset(new ZDateTime(2024, 06, 12, 08, 30, 00), TimeSpan.FromHours(6));
			AssertEquals("Correct Offset is set.", TimeSpan.FromHours(6), receive.WD_ArrivalDate.Offset);

			receive.WD_ArrivalDate = new ZDateTimeOffset(new ZDateTime(2024, 06, 12, 02, 30, 00), TimeSpan.FromHours(0));
			AssertEquals("Correct Offset is set.", TimeSpan.FromHours(0), receive.WD_ArrivalDate.Offset);
		}

		#endregion

		#region TestWD_UnloadCompletedTime_ReadOnly

		public void TestWD_UnloadCompletedTime_ReadOnly()
		{
			TestStandardReadOnly(r => r.WD_UnloadCompletedTimeInfo);
		}

		#endregion

		#region TestWD_HoldPalletIDPutaway_ReadOnly

		public void TestWD_HoldPalletIDPutaway_ReadOnly()
		{
			TestStandardReadOnly(r => r.WD_HoldPalletIDPutawayInfo);
		}

		#endregion

		#region TestWD_OH_Client_ReadOnly

		public void TestWD_OH_Client_ReadOnlyForTaskPlanningStatus()
		{
			TestDocketPlanningStatusReadOnly(d => d.WD_OH_ClientInfo);
		}

		public void TestWD_OH_Client_ReadOnlyOnceReceiveHasStarted()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			AssertEquals("Editable by default.", false, receive.WD_OH_ClientInfo.ReadOnly);

			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			AssertEquals("Client cannot be changed once Receiving has started.", true, receive.WD_OH_ClientInfo.ReadOnly);
		}

		#endregion

		#region TestWD_OHClient_SynchronisesClientOnInventory

		public void TestWD_OH_Client_SynchronisesClientOnInventory()
		{
			var receive = Factory.New<WhsReceive>();
			receive.Lines.AddNew();
			receive.Lines.AddNew();
			receive.Lines.AddNew();

			AssertEquals("Pre-condition", 3, receive.Inventory.Count);
			AssertClientOnInventory(receive.Inventory, ZGuid.Empty);

			var client = Factory.New<OrgHeader>();
			receive.WD_OH_Client = client.PK;
			AssertClientOnInventory(receive.Inventory, client.PK);
		}

		void AssertClientOnInventory(WhsInventoryViewCollection inventories, ZGuid clientPK)
		{
			foreach (WhsInventoryView inv in inventories)
			{
				AssertEquals(clientPK, inv.WI_OH_Client);
			}
		}

		#endregion

		#region TestWD_WW_Whs_SynchronisesWarehouseOnInventory

		public void TestWD_WW_Whs_SynchronisesWarehouseOnInventory()
		{
			var receive = Factory.New<WhsReceive>();
			receive.Lines.AddNew();
			receive.Lines.AddNew();
			receive.Lines.AddNew();

			AssertEquals("Pre-condition", 3, receive.Inventory.Count);
			AssertWarehouseOnInventory(receive.Inventory, ZGuid.Empty);

			var warehouse = Helper.CreateWarehouse("TSTWarehouse");
			receive.WD_WW_Whs = warehouse.PK;
			AssertWarehouseOnInventory(receive.Inventory, warehouse.PK);
		}

		void AssertWarehouseOnInventory(WhsInventoryViewCollection inventories, ZGuid warehousePK)
		{
			foreach (WhsInventoryView inv in inventories)
			{
				AssertEquals(warehousePK, inv.WI_WW_Whs);
			}
		}

		#endregion

		#region TestWD_TotalUnitsFromLines

		public override void TestWD_TotalUnitsFromLines()
		{
			var receive = SetupForTestFinaliseDocket();
			AssertEquals(45m, receive.WD_TotalUnitsFromLines);
		}

		#endregion

		#region TestWD_TotalExpectedUnits

		public void TestWD_TotalExpectedQuantity()
		{
			var client = Helper.CreateClient("client");
			var whs = Helper.CreateWarehouse("whs");
			var product = Helper.CreateProduct(client, "product");
			var receive = Helper.CreateWhsReceive(client, whs, "receive1");
			Factory.Save();

			AssertEquals("Precondition: Total Expected Quantity should initally be zero.", 0m, receive.WD_TotalExpectedQuantity);

			Helper.CreateWhsReceiveInventoryLine(receive, product, 1m);
			receive.Lines[0].WE_ClientOrderedUnits = 10m;
			AssertEquals("Total Expected Quantity should be correct.", 10m, receive.WD_TotalExpectedQuantity);

			Helper.CreateWhsReceiveInventoryLine(receive, product, 2m);
			receive.Lines[1].WE_ClientOrderedUnits = 20m;
			AssertEquals("Total Expected Quantity should be correct.", 30m, receive.WD_TotalExpectedQuantity);

			Helper.CreateWhsReceiveInventoryLine(receive, product, 3m);
			receive.Lines[2].WE_ClientOrderedUnits = 30m;
			AssertEquals("Total Expected Quantity should be correct.", 60m, receive.WD_TotalExpectedQuantity);
		}

		#endregion

		#region TestWD_SplitRelationshipType

		public void TestSplitRelationshipTypeDescription()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receiveChild = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REC-Child", null);
			AssertEquals(ZString.Empty, receiveChild.SplitRelationshipTypeDescription);

			var receiveParent = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REC-Parent", null);
			AssertEquals(ZString.Empty, receiveParent.SplitRelationshipTypeDescription);

			receiveChild.WD_WD_Split = receiveParent.PK;
			Assert("Precondition: Child receive should in Parent.RelatedSplits", receiveParent.RelatedSplits.Contains(receiveChild));
			Assert("Precondition: Parent receive should in Child.RelatedSplits", receiveChild.RelatedSplits.Contains(receiveParent));
			AssertEquals("CHILD", receiveChild.SplitRelationshipTypeDescription);
			AssertEquals("PARENT", receiveParent.SplitRelationshipTypeDescription);
		}

		public void TestSplitRelationshipTypeDescriptionInfo()
		{
			TestReadOnly(r => r.SplitRelationshipTypeDescriptionInfo, true, true, true, true);
		}

		#endregion

		#region TestWD_F3_NKTotalPackType_ReadOnly

		public void TestWD_F3_NKTotalPackType_ReadOnly()
		{
			TestNonStandardReadOnly1(d => d.WD_F3_NKTotalPackTypeInfo);
		}

		#endregion

		#region TestWD_PackagesSent_ReadOnly

		public void TestWD_PackagesSent_ReadOnly()
		{
			TestNonStandardReadOnly1(d => d.WD_PackagesSentInfo);
		}

		#endregion

		#region TestWD_WW_Whs_ReadOnly

		public void TestWD_WW_Whs_ReadOnlyForTaskPlanningStatus()
		{
			TestDocketPlanningStatusReadOnly(d => d.WD_WW_WhsInfo);
		}

		#endregion

		#region TestWD_TotalPallets_ReadOnly

		public void TestWD_TotalPallets_ReadOnly()
		{
			TestNonStandardReadOnly1(d => d.WD_TotalPalletsInfo);
		}

		#endregion

		#region TestWD_TaskPlanningStatus

		public void TestWD_TaskPlanningStatus_InitialSave_TaskManagementEnabled()
		{
			TestWD_TaskPlanningStatus_InitialSaveCore(true, TaskPlanningStatus.Codes.NotReady);
		}

		public void TestWD_TaskPlanningStatus_InitialSave_TaskManagementNotEnabled()
		{
			TestWD_TaskPlanningStatus_InitialSaveCore(false, string.Empty);
		}

		void TestWD_TaskPlanningStatus_InitialSaveCore(bool isTaskManagementEnabled, string expectedTaskPlanningStatus)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			if (isTaskManagementEnabled)
			{
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			}
			Factory.Save();

			AssertEquals(expectedTaskPlanningStatus, receive.WD_TaskPlanningStatus);
		}

		public void TestWD_TaskPlanningStatus_WarehouseChanged_TaskManagementEnabled_StatusIsEmpty()
		{
			TestWD_TaskPlanningStatus_WarehouseChangedCore(string.Empty, true, TaskPlanningStatus.Codes.NotReady);
		}

		public void TestWD_TaskPlanningStatus_WarehouseChanged_TaskManagementEnabled_StatusIsNotReady()
		{
			TestWD_TaskPlanningStatus_WarehouseChangedCore(TaskPlanningStatus.Codes.NotReady, true, TaskPlanningStatus.Codes.NotReady);
		}

		public void TestWD_TaskPlanningStatus_WarehouseChanged_TaskManagementEnabled_StatusIsReady()
		{
			TestWD_TaskPlanningStatus_WarehouseChangedCore(TaskPlanningStatus.Codes.Ready, true, TaskPlanningStatus.Codes.Ready);
		}

		public void TestWD_TaskPlanningStatus_WarehouseChanged_TaskManagementEnabled_StatusIsPlanned()
		{
			TestWD_TaskPlanningStatus_WarehouseChangedCore(TaskPlanningStatus.Codes.Planned, true, TaskPlanningStatus.Codes.Planned);
		}

		public void TestWD_TaskPlanningStatus_WarehouseChanged_TaskManagementEnabled_StatusIsError()
		{
			TestWD_TaskPlanningStatus_WarehouseChangedCore(TaskPlanningStatus.Codes.Error, true, TaskPlanningStatus.Codes.Error);
		}

		public void TestWD_TaskPlanningStatus_WarehouseChanged_TaskManagementNotEnabled_StatusIsEmpty()
		{
			TestWD_TaskPlanningStatus_WarehouseChangedCore(string.Empty, false, string.Empty);
		}

		public void TestWD_TaskPlanningStatus_WarehouseChanged_TaskManagementNotEnabled_StatusIsNotReady()
		{
			TestWD_TaskPlanningStatus_WarehouseChangedCore(TaskPlanningStatus.Codes.NotReady, false, string.Empty);
		}

		public void TestWD_TaskPlanningStatus_WarehouseChanged_TaskManagementNotEnabled_StatusIsReady()
		{
			TestWD_TaskPlanningStatus_WarehouseChangedCore(TaskPlanningStatus.Codes.Ready, false, TaskPlanningStatus.Codes.Ready);
		}

		public void TestWD_TaskPlanningStatus_WarehouseChanged_TaskManagementNotEnabled_StatusIsPlanned()
		{
			TestWD_TaskPlanningStatus_WarehouseChangedCore(TaskPlanningStatus.Codes.Planned, false, TaskPlanningStatus.Codes.Planned);
		}

		public void TestWD_TaskPlanningStatus_WarehouseChanged_TaskManagementNotEnabled_StatusIsError()
		{
			TestWD_TaskPlanningStatus_WarehouseChangedCore(TaskPlanningStatus.Codes.Error, false, TaskPlanningStatus.Codes.Error);
		}

		void TestWD_TaskPlanningStatus_WarehouseChangedCore(string initialStatus, bool isTaskManagementEnabled, string expectedTaskPlanningStatus)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Factory.Save();
			AssertNullOrEmptyOrWhitespace(receive.WD_TaskPlanningStatus);

			receive.WD_TaskPlanningStatus = initialStatus;
			Factory.Save();

			var warehouse = Helper.CreateWarehouse("TST", "A", 2, 2);
			if (isTaskManagementEnabled)
			{
				var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
				warehouse.WW_GG_ReleaseGroup = releaseGroup.PK;
			}
			receive.WD_WW_Whs = warehouse.PK;
			Factory.Save();

			AssertEquals(expectedTaskPlanningStatus, receive.WD_TaskPlanningStatus);
		}

		public void TestWD_TaskPlanningStatus_ShouldBeClearAfterFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.WD_DocketStatus = DocketStatus.Codes.Entered;
			Factory.Save();
			AssertNullOrEmptyOrWhitespace(receive.WD_TaskPlanningStatus);

			receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			AssertEquals(TaskPlanningStatus.Codes.Ready, receive.WD_TaskPlanningStatus);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();
			AssertNullOrEmptyOrWhitespace(receive.WD_TaskPlanningStatus);
		}

		public void TestWD_TaskPlanningStatus_StartedReceiving_StatusIsEmpty()
		{
			TestWD_TaskPlanningStatus_StartedReceiving(string.Empty, TaskPlanningStatus.Codes.Ready);
		}

		public void TestWD_TaskPlanningStatus_StartedReceiving_StatusIsNotReady()
		{
			TestWD_TaskPlanningStatus_StartedReceiving(TaskPlanningStatus.Codes.NotReady, TaskPlanningStatus.Codes.Ready);
		}

		public void TestWD_TaskPlanningStatus_StartedReceiving_StatusIsReady()
		{
			TestWD_TaskPlanningStatus_StartedReceiving(TaskPlanningStatus.Codes.Ready, TaskPlanningStatus.Codes.Ready);
		}

		public void TestWD_TaskPlanningStatus_StartedReceiving_StatusIsPlanned()
		{
			TestWD_TaskPlanningStatus_StartedReceiving(TaskPlanningStatus.Codes.Planned, TaskPlanningStatus.Codes.Planned);
		}

		public void TestWD_TaskPlanningStatus_StartedReceiving_StatusIsError()
		{
			TestWD_TaskPlanningStatus_StartedReceiving(TaskPlanningStatus.Codes.Error, TaskPlanningStatus.Codes.Error);
		}

		public void TestWD_TaskPlanningStatus_StartedReceiving_InitialSave_StatusIsEmpty()
		{
			TestWD_TaskPlanningStatus_StartedReceiving(string.Empty, TaskPlanningStatus.Codes.Ready, receiveInDb: false);
		}

		public void TestWD_TaskPlanningStatus_StartedReceiving_InitialSave_StatusIsNotReady()
		{
			TestWD_TaskPlanningStatus_StartedReceiving(TaskPlanningStatus.Codes.NotReady, TaskPlanningStatus.Codes.Ready, receiveInDb: false);
		}

		public void TestWD_TaskPlanningStatus_StartedReceiving_InitialSave_StatusIsReady()
		{
			TestWD_TaskPlanningStatus_StartedReceiving(TaskPlanningStatus.Codes.Ready, TaskPlanningStatus.Codes.Ready, receiveInDb: false);
		}

		public void TestWD_TaskPlanningStatus_StartedReceiving_InitialSave_StatusIsPlanned()
		{
			TestWD_TaskPlanningStatus_StartedReceiving(TaskPlanningStatus.Codes.Planned, TaskPlanningStatus.Codes.Planned, receiveInDb: false);
		}

		public void TestWD_TaskPlanningStatus_StartedReceiving_InitialSave_StatusIsError()
		{
			TestWD_TaskPlanningStatus_StartedReceiving(TaskPlanningStatus.Codes.Error, TaskPlanningStatus.Codes.Error, receiveInDb: false);
		}

		public void TestWD_TaskPlanningStatus_StartedReceiving(string initialStatus, string expectedTaskPlanningStatus, bool receiveInDb = true)
		{
			var client = Helper.CreateClient("Client");
			var whs = Helper.CreateWarehouse("Whs");
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			whs.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(client, whs);

			if (receiveInDb)
			{
				Factory.Save();
			}

			AssertEquals("Precondition - receive.StartedReceiving", false, receive.StartedReceiving);
			using (WarehouseDataRegistry.Instance.AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, whs.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, true))
			{
				receive.WD_TaskPlanningStatus = initialStatus;
				receive.WD_StartedReceivingTimeUtc = DateTime.UtcNow;
				Factory.Save();
				AssertEquals(true, receive.StartedReceiving);
				AssertEquals(expectedTaskPlanningStatus, receive.WD_TaskPlanningStatus);
			}
		}

		public void TestWD_TaskPlanningStatus_StartedReceiving_NotReadyForPlanningStatusSet()
		{
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Factory.Save();
			AssertEquals("Precondition.", TaskPlanningStatus.Codes.NotReady, receive.WD_TaskPlanningStatus);

			using (WarehouseDataRegistry.Instance.AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, true))
			{
				receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
				Factory.Save();
				AssertEquals("Precondition.", TaskPlanningStatus.Codes.Ready, receive.WD_TaskPlanningStatus);

				receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
				Factory.Save();
				AssertEquals("Task planning status should not get clobbered on saving.", TaskPlanningStatus.Codes.NotReady, receive.WD_TaskPlanningStatus);
			}
		}

		public void TestWD_TaskPlanningStatus_NotStartedReceiving()
		{
			var client = Helper.CreateClient("Client");
			var whs = Helper.CreateWarehouse("Whs");
			var receive = Helper.CreateWhsReceive(client, whs);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			whs.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			AssertEquals("Precondition - receive.StartedReceiving", false, receive.StartedReceiving);
			AssertEquals("Precondition - task planning status is NotReady", TaskPlanningStatus.Codes.NotReady, receive.WD_TaskPlanningStatus);

			using (WarehouseDataRegistry.Instance.AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, whs.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, true))
			{
				receive.WD_BookingDate = DateTime.UtcNow;
				Factory.Save();
				AssertEquals(false, receive.StartedReceiving);
				AssertEquals(true, receive.Warehouse.WW_GG_ReleaseGroup.IsValid);
				AssertEquals("when receive has not started, save will not change task planning status", TaskPlanningStatus.Codes.NotReady, receive.WD_TaskPlanningStatus);
			}
		}

		public void TestWD_TaskPlanningStatus_StartedReceiving_TaskManagementNotEnabled()
		{
			var client = Helper.CreateClient("Client");
			var whs = Helper.CreateWarehouse("Whs");
			var receive = Helper.CreateWhsReceive(client, whs);
			Factory.Save();

			AssertEquals("Precondition - receive.StartedReceiving", false, receive.StartedReceiving);
			AssertEquals("Precondition - task planning status is empty", string.Empty, receive.WD_TaskPlanningStatus);

			using (WarehouseDataRegistry.Instance.AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, whs.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, true))
			{
				receive.WD_StartedReceivingTimeUtc = DateTime.UtcNow;
				Factory.Save();
				AssertEquals(true, receive.StartedReceiving);
				AssertEquals(false, receive.Warehouse.WW_GG_ReleaseGroup.IsValid);
				AssertEquals("when task management is not enabled, save will not change task planning status", string.Empty, receive.WD_TaskPlanningStatus);
			}
		}

		public void TestWD_TaskPlanningStatus_TaskManagementForReceiveNotEnabled()
		{
			var client = Helper.CreateClient("Client");
			var whs = Helper.CreateWarehouse("Whs");
			var receive = Helper.CreateWhsReceive(client, whs);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			whs.WW_GG_ReleaseGroup = releaseGroup.PK;
			Factory.Save();

			AssertEquals("Precondition - receive.StartedReceiving", false, receive.StartedReceiving);
			AssertEquals("Precondition - task planning status is NotReady", TaskPlanningStatus.Codes.NotReady, receive.WD_TaskPlanningStatus);

			using (WarehouseDataRegistry.Instance.AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning.SetTemporaryValue(Guid.Empty, whs.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, false))
			{
				receive.WD_StartedReceivingTimeUtc = DateTime.UtcNow;
				Factory.Save();
				AssertEquals(true, receive.StartedReceiving);
				AssertEquals(true, receive.Warehouse.WW_GG_ReleaseGroup.IsValid);
				AssertEquals("when task management for receive is not enabled in registry, save will not change task planning status", TaskPlanningStatus.Codes.NotReady, receive.WD_TaskPlanningStatus);
			}
		}

		public void TestWD_TaskPlanningStatus_NoWarehouseSet() => TestWD_TaskPlanningStatus_NoWarehouseSet(startedReceiving: false);

		public void TestWD_TaskPlanningStatus_NoWarehouseSet_StartedReceiving() => TestWD_TaskPlanningStatus_NoWarehouseSet(startedReceiving: true);

		void TestWD_TaskPlanningStatus_NoWarehouseSet(bool startedReceiving)
		{
			var client = Helper.CreateClient("Client");
			var whs = Helper.CreateWarehouse("Whs");
			var receive = Helper.CreateWhsReceive(client, whs);
			Factory.Save();

			receive.WD_StartedReceivingTimeUtc = startedReceiving ? ZDateTime.UtcNow : ZDateTime.Empty;
			receive.WD_WW_Whs = ZGuid.Empty;
			AssertNoExceptionThrown(receive.OnSaving);
			AssertEquals("Task planning status should not be set", string.Empty, receive.WD_TaskPlanningStatus);
		}

		#endregion

		#region TestWD_TotalUnits_ReadOnly

		public void TestWD_TotalUnits_ReadOnly()
		{
			TestNonStandardReadOnly1(d => d.WD_TotalUnitsInfo);
		}

		#endregion

		#region	TestInboundDockDoor

		public void TestInboundDockDoor()
		{
			var location = Factory.New<WhsLocation>();
			var receive = Factory.New<WhsReceive>();
			AssertNull(receive.InboundDockDoor);

			receive.WD_WL_InboundDockDoor = location.PK;

			AssertEquals(location, receive.InboundDockDoor);
		}

		#endregion

		#region TestTotalPalletsRecevied

		#region TestTotalPalletsReceived

		public void TestTotalPalletsReceived()
		{
			var refreshBindingCalled = false;
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.TotalPalletsReceivedInfo.ValueChanged += (s, e) => refreshBindingCalled = true;

			AssertEquals("Precondition:", 0, receive.TotalPalletsReceived);

			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			AssertEquals("Should have 1 PalletID as there is one unique PalletID", 1, receive.TotalPalletsReceived);
			AssertEquals("Should trigger RefreshBinding", true, refreshBindingCalled);

			refreshBindingCalled = false;
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			AssertEquals("Should have 1 PalletID as there is one unique PalletID", 1, receive.TotalPalletsReceived);
			AssertEquals("Should trigger RefreshBinding", true, refreshBindingCalled);

			refreshBindingCalled = false;
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT2");
			AssertEquals("Should have 2 PalletIDs as there are two unique PalletIDs", 2, receive.TotalPalletsReceived);
			AssertEquals("Should trigger RefreshBinding", true, refreshBindingCalled);

			refreshBindingCalled = false;
			var receiveLine4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT3");
			AssertEquals("Should have 3 PalletIDs as there are three unique PalletIDs", 3, receive.TotalPalletsReceived);
			AssertEquals("Should trigger RefreshBinding", true, refreshBindingCalled);

			refreshBindingCalled = false;
			var receiveLine5 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			AssertEquals("Should not increase pallet count as the new line has empty PalletID", 3, receive.TotalPalletsReceived);
			AssertEquals("No new PalletID entered, but new Line added, should trigger RefreshBinding", true, refreshBindingCalled);
		}

		#endregion

		#region TestTotalPalletsRecevied_WhenLineRemoved

		public void TestTotalPalletsRecevied_WhenLineRemoved()
		{
			var refreshBindingCalled = false;
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT1");
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT2");
			var receiveLine4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT3");
			var receiveLine5 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "");
			receive.TotalPalletsReceivedInfo.ValueChanged += (s, e) => refreshBindingCalled = true;

			AssertEquals("Precondition:", 3, receive.TotalPalletsReceived);
			AssertEquals("Precondition:", false, refreshBindingCalled);

			receive.Lines.Delete(receiveLine1.InDocketLine);
			AssertEquals("Removed line with non-unique PalletID, should still have 3 PalletIDs", 3, receive.TotalPalletsReceived);
			AssertEquals("Should trigger RefreshBinding", true, refreshBindingCalled);

			refreshBindingCalled = false;
			receive.Lines.Delete(receiveLine3.InDocketLine);
			AssertEquals("Removed line with unique PalletID, should have 2 PalletIDs", 2, receive.TotalPalletsReceived);
			AssertEquals("Should trigger RefreshBinding", true, refreshBindingCalled);

			refreshBindingCalled = false;
			receive.Lines.Delete(receiveLine5.InDocketLine);
			AssertEquals("Removed line with empty PalletID, should still have 2 PalletIDs", 2, receive.TotalPalletsReceived);
			AssertEquals("Should trigger RefreshBinding", true, refreshBindingCalled);
		}

		#endregion

		#region TestUpdateTotalPalletsReceived

		public void TestUpdateTotalPalletsReceived()
		{
			var refreshBindingCalled = false;
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.TotalPalletsReceivedInfo.ValueChanged += (s, e) => refreshBindingCalled = true;

			AssertEquals("Precondition:", false, refreshBindingCalled);

			receive.UpdateTotalPalletsReceived();
			AssertEquals("Should trigger TotalPalletsReceived.RefreshBinding", true, refreshBindingCalled);
		}

		#endregion

		#region TestDeferUpdateTotalPalletsReceived

		public void TestDeferUpdateTotalPalletsReceived()
		{
			var refreshBindingHitCount = 0;
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.TotalPalletsReceivedInfo.ValueChanged += (s, e) => refreshBindingHitCount++;

			AssertEquals("Precondition:", 0, refreshBindingHitCount);

			using (receive.DeferUpdateTotalPalletsReceived())
			{
				AssertEquals("Defer should not call UpdateTotalPalletsReceived.", 0, refreshBindingHitCount);

				receive.UpdateTotalPalletsReceived();
				AssertEquals("While deferred, UpdateTotalPalletsReceived should do nothing.", 0, refreshBindingHitCount);

				using (receive.DeferUpdateTotalPalletsReceived())
				{
					receive.UpdateTotalPalletsReceived();
					AssertEquals("While deferred, UpdateTotalPalletsReceived should do nothing.", 0, refreshBindingHitCount);
				}

				AssertEquals("While deferred, UpdateTotalPalletsReceived should do nothing.", 0, refreshBindingHitCount);
			}

			AssertEquals("Disposing should trigger UpdateTotalPalletsReceived.", 1, refreshBindingHitCount);
		}

		#endregion

		#region TestReceiveTaskPlanningStatusPrompt

		protected override bool SupportsPlanningStatusPrompt => true;

		#endregion

		#endregion

		#region TotalASNLineUnits

		public void TestTotalASNLineUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.RunPreSaveValidation();
			Factory.Save();

			AssertEquals("Precondition:", 0m, Receive.TotalASNLineUnits);
			AssertEquals("Precondition: ASN lines weren't popupated yet, so no ASNLines should exist", 0m, receive.TotalASNLineUnits);
			receive.PopulateASNLines();
			AssertEquals("ALL Inventory lines should be populated and Total calculated", 15m, receive.TotalASNLineUnits);

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m);
			AssertEquals("Change to Inventory colletion doesn't affect ASN Lines", 15m, receive.TotalASNLineUnits);
		}

		#endregion

		#region TestIsFinaliseAllowed

		protected override bool CheckIsHeldByCustomsWhenFinalising
		{
			get { return true; }
		}

		#endregion

		#region TestNeedToAutoGenerateLineNumbersOnCreation

		public void TestNeedToAutoGenerateLineNumbersOnCreation()
		{
			var line = Receive.Lines.AddNew();

			AssertEquals("Line number is auto-filled, because we don't have ASNLines yet", (ZShort)1, line.WE_LineNo);

			Receive.AsnLines.AddNew();
			var line2 = Receive.Lines.AddNew();

			AssertEquals("Line number remains zero because we have ASN lines", (ZShort)0, line2.WE_LineNo);
		}

		#endregion

		#region TestCanCreateInventory

		public void TestCanCreateInventory()
		{
			AssertEquals("Receive always creates new Inventroy.", true, Docket.CanCreateInventory);
		}

		#endregion

		#region TestReceiveGoodsHandlingInstructions

		public void TestReceiveGoodsHandlingInstructions()
		{
			var receive = base.SetupForTestFinaliseDocket();
			Factory.Save();

			var notes = receive.ReceiveGoodsHandlingInstructions;
			AssertEquals("Precondition: No Goods Handling Instructions Notes should exist", string.Empty, notes);

			receive.Notes.AddNew(false, "Goods Handling Instructions", "Be very very careful with this rabbit.");

			notes = receive.ReceiveGoodsHandlingInstructions;
			AssertEquals("ReceiveGoodsHandlingInstructions should be correct.", "Be very very careful with this rabbit.", notes);
		}

		public void TestReceiveGoodsHandlingInstructions_Multiple()
		{
			var receive = base.SetupForTestFinaliseDocket();
			var note1 = receive.Notes.AddNew(false, "Goods Handling Instructions", "Be very very careful with this rabbit.");
			var note2 = receive.Notes.AddNew(false, "Goods Handling Instructions", "Second note.");
			note2.ST_NoteContextDirection = nameof(StmNoteContextDirection.I);
			var note3 = receive.Notes.AddNew(false, "Goods Handling Instructions", "Third note.");
			note3.ST_NoteContextDirection = nameof(StmNoteContextDirection.F);
			Factory.Save();

			var notes = receive.ReceiveGoodsHandlingInstructions;
			AssertEquals("ReceiveGoodsHandlingInstructions should be correct.", "Multiple Goods Handling Instructions found.", notes);
		}

		public void TestReceiveGoodsHandlingInstructions_NoteOnRelatedJob()
		{
			var receive = base.SetupForTestFinaliseDocket();
			var order = Factory.New<WhsOrder>();
			Docket.WD_WD_ParentDocket = order.PK;
			order.Notes.AddNew(false, "Goods Handling Instructions", "Be very very careful with this rabbit.");

			var notes = receive.ReceiveGoodsHandlingInstructions;
			AssertEquals("ReceiveGoodsHandlingInstructions should be empty as the note is on a related job.", string.Empty, notes);
		}

		public void TestReceiveGoodsHandlingInstructions_MultipleNotes_RelatedJob()
		{
			var receive = base.SetupForTestFinaliseDocket();
			var order = Factory.New<WhsOrder>();
			Docket.WD_WD_ParentDocket = order.PK;
			order.Notes.AddNew(false, "Goods Handling Instructions", "Be very very careful with this rabbit.");
			receive.Notes.AddNew(false, "Goods Handling Instructions", "Receive Note.");

			var notes = receive.ReceiveGoodsHandlingInstructions;
			AssertEquals("ReceiveGoodsHandlingInstructions should be note from receive.", "Receive Note.", notes);
		}

		public void TestReceiveGoodsHandlingInstructions_NoteOnClient()
		{
			var receive = base.SetupForTestFinaliseDocket();
			receive.Client.Notes.AddNew(false, "Goods Handling Instructions", "Organisational Instructions");
			Factory.Save();

			var notes = receive.ReceiveGoodsHandlingInstructions;
			AssertEquals("ReceiveGoodsHandlingInstructions should be correct.", "Organisational Instructions", notes);
		}

		#endregion

		#region WD_TotalPallets

		public void TestWD_TotalPallets_TotalPalletsValidation_RegistryControlEnabled_NoReceiveLines()
		{
			using (WarehouseDataRegistry.Instance.TotalPalletsValidation.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				receive.WD_TotalPallets = 1;
				AssertEquals("Precondition: Receive has no receive lines.", false, receive.Lines.Count > 0);
				var expectedNotification = "Total Pallets 1 does not equal the total of received Pallets 0.";

				AssertNoError("WD_TotalPalletsInfo should not have an error.", receive.WD_TotalPalletsInfo, expectedNotification);
				AssertHasWarning("WD_TotalPalletsInfo should only have a warning.", receive.WD_TotalPalletsInfo, expectedNotification);
			}
		}

		#endregion

		#region TestHasBeenLoadedInRF

		public void TestHasBeenLoadedInRF()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			AssertEquals("IsReceiptLoadedInRF is false.", false, receive.HasBeenLoadedInRF());

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			receive.Logs.AddNew(Events.EditedARecord, "RF", ZDateTimeOffset.Now);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			AssertEquals("IsReceiptLoadedInRF is true.", true, receive.HasBeenLoadedInRF());
		}

		#endregion

		#region TestProductCount

		protected override void TestProductCountCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1");
			Helper.CreateWhsReceiveLine(receive1, data.Part1, 7m, location);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive2");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 15m, location);
			Helper.CreateWhsReceiveLine(receive2, data.Part2, 3m, location);

			AssertEquals("Receive1 in db returns correct product count.", 1, receive1.ProductCount);
			AssertEquals("Receive2 in db returns correct product count.", 2, receive2.ProductCount);

			Factory.Save();
			AssertEquals("Receive1 not in db returns correct product count.", 1, receive1.ProductCount);
			AssertEquals("Receive2 not in db returns correct product count.", 2, receive2.ProductCount);
		}

		public void TestReceiveProductCount_DuplicateProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 15m, location);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 3m, location);
			AssertEquals("Receive in db returns correct product count.", 1, receive.ProductCount);

			Factory.Save();
			AssertEquals("Receive not in db returns correct product count.", 1, receive.ProductCount);
		}

		public void TestReceiveProductCount_InvalidProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Order1");
			var receiveLine = receive.Lines.AddNew();
			AssertEquals("Product is empty", false, receiveLine.WE_OP.IsValid);
			AssertEquals("Receive in db returns correct product count.", 0, receive.ProductCount);
		}

		public void TestReceiveProductCount_MultipleDifferentProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location = data.Whs1.FindLocation("A-1");
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var part4 = Helper.CreateProduct(data.Org1, "P4");
			var part5 = Helper.CreateProduct(data.Org1, "P5");
			var part6 = Helper.CreateProduct(data.Org1, "P6");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 15m, location);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 13m, location);
			Helper.CreateWhsReceiveLine(receive, part3, 9m, location);
			Helper.CreateWhsReceiveLine(receive, part4, 5m, location);
			Helper.CreateWhsReceiveLine(receive, part5, 7m, location);
			Helper.CreateWhsReceiveLine(receive, part6, 16m, location);
			AssertEquals("Receive in db returns correct product count.", 6, receive.ProductCount);

			Factory.Save();
			AssertEquals("Receive not in db returns correct product count.", 6, receive.ProductCount);
		}

		public void TestReceiveProductCount_DeletingDocketLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 15m, location);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 3m, location);
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 5m, location);
			Factory.Save();

			AssertEquals("Receive has 3 lines.", 3, receive.Lines.Count);
			AssertEquals("Receive returns correct product count.", 2, receive.ProductCount);

			receiveLine3.Delete();
			AssertEquals("Receive has 2 lines.", 2, receive.Lines.Count);
			AssertEquals("Receive returns correct product count after receiveline is deleted.", 1, receive.ProductCount);
		}

		public void TestReceiveProductCount_DBHits()
		{
			var numberOfReceiveLines = 25;
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location = data.Whs1.FindLocation("A-1");

			var testReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1");
			Helper.CreateWhsReceiveLine(testReceive, data.Part1, 15m, location);

			for (var i = 0; i < numberOfReceiveLines; i++)
			{
				Helper.CreateWhsReceiveLine(testReceive, data.Part2, 5m, location);
			}
			Factory.Save();

			var expectedDBHits = new Dictionary<string, int>()
			{
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
			};

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, newFactory))
			{
				var receiveInNewFactory = newFactory.Load<WhsReceive>(testReceive.PK);
				AssertEquals("receiveInNewFactory returns correct product count.", 2, receiveInNewFactory.ProductCount);
			}
		}

		#endregion

		#region TestReceiveProductSummaryCollection

		public void TestReceiveProductSummaryCollection_CreatesCollectionAsExpected()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory, "Colour");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m);
			receiveLine.WE_PartAttrib1 = "Blue";
			receiveLine.WE_ExpiryDate = ZDate.Today.AddDays(4);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 12m);
			Factory.Save();

			receive.PopulateASNLines();
			receiveLine.WE_TransactionQuantity = 0m;
			Factory.Save();

			var collection = receive.ReceiveProductSummaryCollection;
			AssertEquals("Collection has two summaries", 2, collection.Count);
			var part1Summary = collection.Cast<WhsReceiveProductSummary>().FirstOrDefault(x => x.ProductCode == data.Part1.OP_PartNum);
			AssertEquals("Collection has correct Part1 summary - ExpectedQuantity", 30m, part1Summary.ExpectedQuantity);
			AssertEquals("Collection has correct Part1 summary - ReceivedQuantity", 0m, part1Summary.ReceivedQuantity);

			var part2Summary = collection.Cast<WhsReceiveProductSummary>().FirstOrDefault(x => x.ProductCode == data.Part2.OP_PartNum);
			AssertEquals("Collection has correct Part2 summary - ExpectedQuantity", 12m, part2Summary.ExpectedQuantity);
			AssertEquals("Collection has correct Part2 summary - ReceivedQuantity", 12m, part2Summary.ReceivedQuantity);
		}

		public void TestReceiveProductSummaryCollection_UpdatesSummaryCollection_OnAddingReceiveLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			var collection = receive.ReceiveProductSummaryCollection;
			AssertEquals("Collection has no summaries", 0, collection.Count);

			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m);
			collection = receive.ReceiveProductSummaryCollection;
			AssertEquals("Collection has one summary", 1, collection.Count);
			var summary = collection.Cast<WhsReceiveProductSummary>().FirstOrDefault(x => x.ProductCode == data.Part1.OP_PartNum);
			AssertEquals("Collection has correct summary - ExpectedQuantity", 30m, summary.ExpectedQuantity);
			AssertEquals("Collection has correct summary - ReceivedQuantity", 30m, summary.ReceivedQuantity);

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 12m);
			collection = receive.ReceiveProductSummaryCollection;
			AssertEquals("Collection has two summaries", 2, collection.Count);
			var part1Summary = collection.Cast<WhsReceiveProductSummary>().FirstOrDefault(x => x.ProductCode == data.Part1.OP_PartNum);
			AssertEquals("Collection has correct Part1 summary - ExpectedQuantity", 30m, part1Summary.ExpectedQuantity);
			AssertEquals("Collection has correct Part1 summary - ReceivedQuantity", 30m, part1Summary.ReceivedQuantity);

			var part2Summary = collection.Cast<WhsReceiveProductSummary>().FirstOrDefault(x => x.ProductCode == data.Part2.OP_PartNum);
			AssertEquals("Collection has correct Part2 summary - ExpectedQuantity", 12m, part2Summary.ExpectedQuantity);
			AssertEquals("Collection has correct Part2 summary - ReceivedQuantity", 12m, part2Summary.ReceivedQuantity);
		}

		public void TestReceiveProductSummaryCollection_ChangeWE_ClientOrderedUnitsOfReceiveLineAfterASNLineCreation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var collection = receive.ReceiveProductSummaryCollection;
			AssertEquals("Collection has no summaries", 0, collection.Count);

			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 12m);
			Factory.Save();

			receive.PopulateASNLines();
			receiveLine1.WE_TransactionQuantity = 15m;
			collection = receive.ReceiveProductSummaryCollection;
			AssertEquals("Collection has two summaries", 2, collection.Count);

			var part1Summary = collection.Cast<WhsReceiveProductSummary>().FirstOrDefault(x => x.ProductCode == data.Part1.OP_PartNum);
			AssertEquals("Collection has correct Part1 summary - ExpectedQuantity", 30m, part1Summary.ExpectedQuantity);
			AssertEquals("Collection has correct Part1 summary - ReceivedQuantity", 15m, part1Summary.ReceivedQuantity);

			var part2Summary = collection.Cast<WhsReceiveProductSummary>().FirstOrDefault(x => x.ProductCode == data.Part2.OP_PartNum);
			AssertEquals("Collection has correct Part2 summary - ExpectedQuantity", 12m, part2Summary.ExpectedQuantity);
			AssertEquals("Collection has correct Part2 summary - ReceivedQuantity", 12m, part2Summary.ReceivedQuantity);

			receiveLine2.WE_ClientOrderedUnits = 20m;
			collection = receive.ReceiveProductSummaryCollection;
			AssertEquals("Collection has two summaries", 2, collection.Count);

			part1Summary = collection.Cast<WhsReceiveProductSummary>().FirstOrDefault(x => x.ProductCode == data.Part1.OP_PartNum);
			AssertEquals("Collection has correct Part1 summary - ExpectedQuantity", 30m, part1Summary.ExpectedQuantity);
			AssertEquals("Collection has correct Part1 summary - ReceivedQuantity", 15m, part1Summary.ReceivedQuantity);

			part2Summary = collection.Cast<WhsReceiveProductSummary>().FirstOrDefault(x => x.ProductCode == data.Part2.OP_PartNum);
			AssertEquals("ExpectedQuantity of Part2 doesn't change", 12m, part2Summary.ExpectedQuantity);
			AssertEquals("Collection has correct Part2 summary - ReceivedQuantity", 12m, part2Summary.ReceivedQuantity);
		}

		public void TestReceiveProductSummaryCollection_AddReceiveLineAfterASNLineCreation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 30m);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 12m);
			Factory.Save();

			receive.PopulateASNLines();
			receiveLine1.WE_TransactionQuantity = 15m;
			receiveLine2.WE_ClientOrderedUnits = 20m;
			Factory.Save();

			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 16m);
			receiveLine3.WE_ClientOrderedUnits = 0m;
			var collection = receive.ReceiveProductSummaryCollection;
			AssertEquals("Collection has two summaries", 2, collection.Count);

			var part1Summary = collection.Cast<WhsReceiveProductSummary>().FirstOrDefault(x => x.ProductCode == data.Part1.OP_PartNum);
			AssertEquals("Collection has correct Part1 summary - ExpectedQuantity", 30m, part1Summary.ExpectedQuantity);
			AssertEquals("Collection has correct Part1 summary - ReceivedQuantity", 15m, part1Summary.ReceivedQuantity);

			var part2Summary = collection.Cast<WhsReceiveProductSummary>().FirstOrDefault(x => x.ProductCode == data.Part2.OP_PartNum);
			AssertEquals("ExpectedQuantity of Part2 doesn't change", 12m, part2Summary.ExpectedQuantity);
			AssertEquals("ReceivedQuantity of Part2 is updated", 28m, part2Summary.ReceivedQuantity);
		}

		#endregion

		#region TestVehicleNo

		public void TestVehicleNo()
		{
			var receive = GetNewBusinessObject();

			AssertEquals(ZString.Empty, receive.VehicleNo);

			receive.References.AddNew();
			receive.References[0].WX_RefType = "VHN";
			receive.References[0].WX_Reference = "123";
			AssertEquals("123", receive.VehicleNo);

			receive.References[0].WX_RefType = "HSB";
			AssertEquals(ZString.Empty, receive.VehicleNo);

			receive.References[0].WX_RefType = "VHN";
			receive.VehicleNo = "345";
			AssertEquals("345", receive.References[0].WX_Reference);

			receive.References.AddNew();
			receive.References[1].WX_RefType = "XXX";
			receive.VehicleNo = ZString.Empty;
			AssertEquals(1, receive.References.Count);
			AssertEquals("XXX", receive.References[0].WX_RefType);
			AssertEquals(ZString.Empty, receive.VehicleNo);
		}

		public void TestVehicleNo_OperationalActions()
		{
			var actionFieldAttribute = ActionFieldAttribute.Get(typeof(WhsReceive).GetProperty(WhsReceive.Schema.VehicleNo));
			AssertNotNull($"Action field attribute of {WhsReceive.Schema.VehicleNo} should exist", actionFieldAttribute);
		}

		#endregion

		#region TestVehicleNoInfo

		public void TestVehicleNoInfo()
		{
			var receive = GetNewBusinessObject();
			AssertEquals(25, receive.VehicleNoInfo.MaxLength);
			TestNonStandardReadOnly1(r => r.VehicleNoInfo);
		}

		#endregion

		#region TestWD_FinalisedDate

		public void TestWD_FinalisedDate_UpdateVersionIdPolicyWhenSet()
		{
			var receive = Factory.New<WhsReceive>();
			AssertEquals("Concurrency Policy should be Ignore when WD_FinalisedDate is not set.", ConcurrencyPolicy.Ignore, receive.WD_CriticalChangesVersionIDInfo.ConcurrencyPolicy);

			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals("Concurrency Policy should be Strict when WD_FinalisedDate is set.", ConcurrencyPolicy.Strict, receive.WD_CriticalChangesVersionIDInfo.ConcurrencyPolicy);

			receive.WD_FinalisedDate = ZDateTimeOffset.Empty;
			AssertEquals("Concurrency Policy should be Ignore when WD_FinalisedDate is not set.", ConcurrencyPolicy.Ignore, receive.WD_CriticalChangesVersionIDInfo.ConcurrencyPolicy);
		}

		#endregion

		#region TestWD_FirstScannedInboundDockDoorUtc

		public void TestWD_FirstScannedInboundDockDoorUtc()
		{
			var receive = Factory.New<WhsReceive>();
			AssertEquals("Precondition", ZDateTime.Empty, receive.WD_FirstScannedInboundDockDoorUtc);
			AssertEquals("Precondition", ConcurrencyPolicy.Default, receive.WD_WL_InboundDockDoorInfo.ConcurrencyPolicy);

			var now = ZDateTime.UtcNow;
			receive.WD_FirstScannedInboundDockDoorUtc = now;
			AssertEquals(now, receive.WD_FirstScannedInboundDockDoorUtc);
			AssertEquals(ConcurrencyPolicy.Strict, receive.WD_WL_InboundDockDoorInfo.ConcurrencyPolicy);

			receive.WD_FirstScannedInboundDockDoorUtc = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, receive.WD_FirstScannedInboundDockDoorUtc);
			AssertEquals(ConcurrencyPolicy.Default, receive.WD_WL_InboundDockDoorInfo.ConcurrencyPolicy);
		}

		#endregion

		#region TestReadonly_WhenCreatedFromPickByBOM

		public void TestReadonly_WhenCreatedFromPickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 70m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 2m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);

			var createdReceive = Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertEquals(true, createdReceive.WD_OH_ClientInfo.ReadOnly);
			AssertEquals(true, createdReceive.WD_WW_WhsInfo.ReadOnly);
			AssertEquals(true, createdReceive.WD_DocketSubTypeInfo.ReadOnly);
			AssertEquals(true, createdReceive.TransportCoDocAddress.ReadOnly);
			AssertEquals(true, createdReceive.SupplierDocAddress.ReadOnly);
			AssertEquals(true, createdReceive.WD_ReceiveCategoryInfo.ReadOnly);
			AssertEquals(true, createdReceive.WD_ExternalReferenceInfo.ReadOnly);
			AssertEquals(true, createdReceive.WD_CustomerReferenceInfo.ReadOnly);
			AssertEquals(true, createdReceive.WD_RS_NKServiceLevelInfo.ReadOnly);
			AssertEquals(true, createdReceive.WD_TransportReferenceInfo.ReadOnly);
			AssertEquals(true, createdReceive.VehicleNoInfo.ReadOnly);
			AssertEquals(true, createdReceive.WD_PL_NKCarrierServiceLevelInfo.ReadOnly);
			AssertEquals(true, createdReceive.WD_DropModeInfo.ReadOnly);
			AssertEquals(true, createdReceive.WD_BookingDateInfo.ReadOnly);
			AssertEquals(true, createdReceive.WD_ETDInfo.ReadOnly);
			AssertEquals(true, createdReceive.WD_ETAInfo.ReadOnly);
			AssertEquals(true, createdReceive.WD_ArrivalDateInfo.ReadOnly);
			AssertEquals(true, createdReceive.WD_TotalUnitsInfo.ReadOnly);
			AssertEquals(true, createdReceive.WD_TotalPalletsInfo.ReadOnly);
			AssertEquals(true, createdReceive.WD_PackagesSentInfo.ReadOnly);
			AssertEquals(true, createdReceive.WD_F3_NKTotalPackTypeInfo.ReadOnly);
			AssertEquals(true, createdReceive.WD_TotalWeightInfo.ReadOnly);
			AssertEquals(true, createdReceive.WD_TotalWeightUnitInfo.ReadOnly);
			AssertEquals(true, createdReceive.WD_TotalCubicInfo.ReadOnly);
			AssertEquals(true, createdReceive.WD_TotalCubicUnitInfo.ReadOnly);

			var createdReceiveLine = createdReceive.Lines[0];
			AssertEquals(true, createdReceiveLine.WE_OPInfo.ReadOnly);
			AssertEquals(true, createdReceiveLine.WE_PackQuantityInfo.ReadOnly);
			AssertEquals(true, createdReceiveLine.WE_F3_NKPackTypeInfo.ReadOnly);
			AssertEquals(true, createdReceiveLine.WE_ClientOrderedUnitsInfo.ReadOnly);
			AssertEquals(true, createdReceiveLine.WE_WLInfo.ReadOnly);
			AssertEquals(true, createdReceiveLine.WE_ReceiveCrossDockOrderNoInfo.ReadOnly);
			AssertEquals(true, createdReceiveLine.WE_PalletIDInfo.ReadOnly);
			AssertEquals(true, createdReceiveLine.ConsigneeNameOrPKInfo.ReadOnly);
			AssertEquals(true, createdReceiveLine.SplitQuantityInfo.ReadOnly);
			AssertEquals(true, createdReceiveLine.WE_WHC_NKOriginalInventoryHeldCodeInfo.ReadOnly);
			AssertEquals(true, createdReceiveLine.WE_RequiredByDateInfo.ReadOnly);
			AssertEquals(true, createdReceiveLine.WE_PartAttrib1Info.ReadOnly);
			AssertEquals(true, createdReceiveLine.WE_PartAttrib2Info.ReadOnly);
			AssertEquals(true, createdReceiveLine.WE_PartAttrib3Info.ReadOnly);
			AssertEquals(true, createdReceiveLine.WE_SerialNumberInfo.ReadOnly);
			AssertEquals(true, createdReceiveLine.WE_ExpiryDateInfo.ReadOnly);
			AssertEquals(true, createdReceiveLine.WE_PackingDateInfo.ReadOnly);
			AssertEquals(true, createdReceiveLine.OriginalHoldReasonInfo.ReadOnly);
			AssertEquals(true, createdReceiveLine.WE_LineNoInfo.ReadOnly);
			AssertEquals(true, createdReceiveLine.WE_SubLineNoInfo.ReadOnly);

			AssertEquals(true, createdReceive.References.ReadOnly);
			AssertEquals(true, createdReceive.Containers.ReadOnly);
		}

		#endregion

		public void TestWD_DocketSubType_ReturnReceiveValidatesWD_ExternalReference()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "");
			AssertNoWarnings("Precondition", receive.WD_ExternalReferenceInfo);

			receive.WD_DocketSubType = ReceiveType.Codes.Returns;
			AssertHasWarning(receive.WD_ExternalReferenceInfo, "Return receives require a reference with the same order number of a departed order to be considered a valid return receive.");

			receive.WD_DocketSubType = ReceiveType.Codes.Receipt;
			AssertNoWarnings(receive.WD_ExternalReferenceInfo);
		}

		#endregion

		#region Action Menu Functionality

		#region Cancel Receive

		public void TestCancelReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			AssertEquals("Receive should not be cancelled by default.", false, receive.IsCancelled);

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			Factory.Save();
			AssertEquals("Should have 8 Inventory attached to Receive.", 8, receive.Inventory.Count);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 8m);

			foreach (WhsInventoryView inventory in receive.Inventory)
			{
				AssertEquals(1m, inventory.WI_TotalUnits);
				AssertEquals(1m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);

				var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
				AssertEquals("Precondition: Stock is Reserved.", 1m, reservedPickLine.ReservedQuantity);
			}

			AssertEquals("Precondition: Stock is reserved.", 8, orderLine.ReservedPickLines.Count);

			receive.CancelReactivateDocket();
			AssertEquals("Receive should be successfully cancelled.", true, receive.IsCancelled);
			AssertEquals("No Inventory should be deleted when Receive is cancelled.", 8, receive.Inventory.Count);

			foreach (WhsInventoryView inventory in receive.Inventory)
			{
				AssertEquals("Inventory's Total Units should be set to Zero when Receive is Cancelled.", 0m, inventory.WI_TotalUnits);
				AssertEquals("Inventory's Total Units should be set to Zero when Receive is Cancelled.", 0m, inventory.CommittedQuantityIncludingUnfinalisedReceipt);
				AssertEquals("All reservations should be deleted when cancelling a Receive.", 0, inventory.ReservedPickLines.Count);
			}

			AssertEquals("All reservations should be deleted when cancelling a Receive.", 0, orderLine.ReservedPickLines.Count);
		}

		public void TestCancelReceive_ReceiveHasPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 4);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "A", 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "A", 10m);
			transfer.RunPreSaveValidation();
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.PuttingAway, transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", false, receive.IsCancelled);

			var canCancel = receive.CanCancel();
			AssertEquals("Receive should not be cancellable.", "You cannot cancel receives that have an active putaway transfer.", canCancel);
		}

		public void TestCancelReceive_ReceiveCreatedFromWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Factory.Save();

			var workOrder = Factory.New<WhsWorkOrder>();
			receive.WD_WD_ParentDocket = workOrder.PK;
			var canCancel = receive.CanCancel();
			AssertEquals("Receive should not be cancellable.", "You cannot cancel receives created from Work Orders.", canCancel);
		}

		#endregion

		#region TestCancelReactivateReceive

		public void TestCancelReactivateReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Factory.Save();
			AssertEquals("Precondition.", false, receive.IsCancelled);
			AssertEquals("Precondition.", 10m, receiveLine.WE_StockOnHand);

			receive.CancelReactivateDocket();
			AssertEquals("Should be cancel.", true, receive.IsCancelled);
			AssertEquals("Cancel line has zero units.", 0m, receiveLine.WE_StockOnHand);

			receive.CancelReactivateDocket();
			AssertEquals("Should Reactivate the recive.", false, receive.IsCancelled);
			AssertEquals("After Reactivating should back to original value.", 10m, receiveLine.WE_StockOnHand);
		}

		#endregion

		#region TestCancelReactivateReceive_PackType

		public void TestCancelReactivateReceive_PackType()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, "UNT", "PLT", 25m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 4m);
			receiveLine.Inventory[0].WI_F3_NKPackType = "PLT";
			Factory.Save();
			AssertEquals("Precondition", false, receive.IsCancelled);
			AssertEquals("Precondition", 100m, receiveLine.WE_StockOnHand);

			receive.CancelReactivateDocket();
			AssertEquals("Should be cancel.", true, receive.IsCancelled);
			AssertEquals("Cancel line has zero units.", 0m, receiveLine.WE_StockOnHand);

			receive.CancelReactivateDocket();
			AssertEquals("Should Reactivate the recive.", false, receive.IsCancelled);
			AssertEquals("After Reactivating should back to original value.", 100m, receiveLine.WE_StockOnHand);
		}

		#endregion

		#region TestReactivateReceive_ReceiveHasPickedPutAwayTransfer

		public void TestReactivateReceive_ReceiveHasPickedPutAwayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 4);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "A", 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "A", 10m);
			transfer.RunPreSaveValidation();
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();
			transferLine.FinaliseDocketLine();

			AssertEquals("Precondition.", false, receive.IsCancelled);
			AssertEquals("Precondition.", 0m, receive.Lines.Single().WE_StockOnHand);

			// Cancel receive; have to do this through row, as cancellation validation rules will prevent receive from being cancelled due to picked putaway transfer
			((IBusinessObjectInternals)receive).Row[WhsDocketSchema.Constants.WD_DocketStatus] = DocketStatus.Codes.Cancelled;
			receive.WD_GS_NKCanceledBy = "~BP";
			receive.WD_CanceledTimeUtc = ZDateTime.UtcNow;
			receive.HasChanges = true;
			receiveLine.InDocketLine.WE_DocketLineStatus = DocketLineStatus.Codes.Cancelled;
			Factory.Save();
			AssertEquals("Should be cancelled.", true, receive.IsCancelled);
			AssertEquals("Stock on hand still 0 after cancellation.", 0m, receive.Lines.Single().WE_StockOnHand);

			receive.CancelReactivateDocket();
			Factory.Save();
			AssertEquals("Receive should be reactivated.", false, receive.IsCancelled);
			AssertEquals("Stock on hand should remain 0, as receive line has a picked putaway transfer line.", 0m, receive.Lines.Single().WE_StockOnHand);
			AssertEquals("Receive line status should be set to PFU.", DocketLineStatus.Codes.PickedForUnload, receive.Lines.Single().WE_DocketLineStatus);
		}

		#endregion

		#region Receipt Splitting

		#region TestAutoFillSplitQuantities

		#region TestAutoFillSplitQuantities

		public void TestAutoFillSplitQuantities()
		{
			var inventory = new List<WhsInventoryView>();
			inventory.Add(Factory.New<WhsInventoryView>());
			inventory.Add(Factory.New<WhsInventoryView>());
			inventory.Add(Factory.New<WhsInventoryView>());

			inventory[0].WI_InDocketLineUnits = 0.01m;
			inventory[1].WI_InDocketLineUnits = 10m;
			inventory[1].WI_SplitQuantity = 6m;
			inventory[2].WI_InDocketLineUnits = 1000.23m;

			Receive.AutoFillSplitQuantities(inventory);

			AssertEquals(0.01m, inventory[0].WI_SplitQuantity);
			AssertEquals(6m, inventory[1].WI_SplitQuantity);
			AssertEquals(1000.23m, inventory[2].WI_SplitQuantity);
		}

		#endregion

		#region TestAutoFillSplitQuantities_DoesNotExecuteIfFinalisedOrCancelled

		public void TestAutoFillSplitQuantities_DoesNotExecuteIfFinalisedOrCancelled()
		{
			var inventory = new List<WhsInventoryView>();
			inventory.Add(Factory.New<WhsInventoryView>());
			inventory[0].WI_InDocketLineUnits = 0.01m;

			var receive = GetNewBusinessObject();
			receive.WD_FinalisedDate = ZDateTimeOffset.Today;
			receive.AutoFillSplitQuantities(inventory);
			AssertEquals(0m, inventory[0].WI_SplitQuantity);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));

			((NotificationBuffer)receive.NotificationManager.Peek).Clear();
			receive.WD_FinalisedDate = ZDateTimeOffset.Empty;
			receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			receive.AutoFillSplitQuantities(inventory);
			AssertEquals(0m, inventory[0].WI_SplitQuantity);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		#endregion

		#region TestAutoFillSplitQuantities_ChecksArgument

		[ExpectException(typeof(ArgumentNullException))]
		public void TestAutoFillSplitQuantities_ChecksArgument()
		{
			Receive.AutoFillSplitQuantities(null);
		}

		#endregion

		#region TestAutoFillSplitQuantities_PutawayTransfers

		public void TestAutoFillSplitQuantities_PutawayTransfers()
		{
			AssertPerformActionOnSelectedInventory_PutawayTransfers((receive, inventory) => receive.AutoFillSplitQuantities(inventory));
		}

		#endregion

		#endregion

		#region TestSplitReceiptByQuantity

		#region TestSplitReceiptByQuantity

		public void TestSplitReceiptByQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REF1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m);
			var inventory5 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			var inventory6 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m);

			receive.SplitReceiptByQuantity();
			var notify = (NotificationBuffer)receive.NotificationManager.Peek;
			AssertEquals("Should have error if no lines have split quantity", true, notify.ContainsNotificationType(ReceiveErrorTypes.SplitByQuantityRequiresLinesWithSplitQuantities));

			receive.AutoFillSplitQuantities(receive.Inventory.Cast<WhsInventoryView>());
			receive.SplitReceiptByQuantity();
			notify = (NotificationBuffer)receive.NotificationManager.Peek;
			AssertEquals("Should have error if all lines have full split quantities", true, notify.ContainsNotificationType(ReceiveErrorTypes.SplitByQuantitiesFullySet));

			inventory1.WI_SplitQuantity = 1m;
			inventory2.WI_SplitQuantity = 0m;
			inventory3.WI_SplitQuantity = 3m;
			inventory4.WI_SplitQuantity = 4m;
			inventory5.WI_SplitQuantity = 2m;
			inventory6.WI_SplitQuantity = 5m;

			receive.SplitReceiptByQuantity();
			AssertEquals("Should be saved", true, notify.ContainsNotificationType(ReceiveErrorTypes.SplitByQuantityRequiresReceiptToBeSaved));
			Factory.Save();

			notify.Clear();
			receive.SplitReceiptByQuantity();
			AssertEquals("Should have no error", false, notify.HasErrors);
			AssertEquals("Original Receive should only have 3 lines after split", 3, receive.Lines.Count);

			var query = new ZQuery(WhsDocketSchema.WD_ExternalReference, SQLComparisonOperator.StartsWith, "REF1");
			AssertEquals("Should now be 2 receive (1 new split)", 2, Factory.Load<WhsReceive>(query).Length);
		}

		#endregion

		#region TestSplitReceiptByQuantity_WithLargerTotalWeight

		public void TestSplitReceiptByQuantity_WithLargerTotalWeight()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 999999.99m, Constants.Weight.Kilograms, 999999.2m, Constants.Volume.CubicMetres);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REF1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m);
			inventory.WI_SplitQuantity = 5m;

			receive.SplitReceiptByQuantity();
			var lastEvent = ((TestNotificationBuffer)(receive.NotificationManager.Peek)).LastEvent;
			AssertEquals("Should have error", "Error: This Receipt cannot be split because it has some errors. Please correct the errors and try again.", lastEvent.Message);

			receive.WD_TotalWeight = 1m; // Hack to save receive to DB so that it won't exceed the DB limit
			receive.WD_TotalCubic = 1m;
			AssertNoExceptionThrown(() => Factory.Save());

			receive.SplitReceiptByQuantity();
			AssertEquals("Split Quantity should be 0", 0m, receive.Lines[0].SplitQuantity);
			AssertEquals("After Split the Quantity should be 15", 15m, receive.Inventory[0].WI_InDocketLineUnits);
			AssertEquals("After Split the Expected Quantity should be 15", 15m, receive.Inventory[0].WI_ExpectedReceiptQuantity);

			var relatedSplits = receive.RelatedSplits;
			AssertEquals(1, relatedSplits.Count);
			AssertEquals(true, relatedSplits.HasErrors());

			var expectedErrorMsg = @"Error - WD_TotalCubic: The number 4,999,997.0 is too large, the maximum value allowed for Volume is 999,999.999.
Error - WD_TotalWeight: The number 5,000,000.95 is too large, the maximum value allowed for Weight is 999,999.999.";
			var errorMsg = string.Join("\r\n", relatedSplits.GetErrors().GetUniqueMessageList());
			AssertEquals(expectedErrorMsg, errorMsg);
		}

		#endregion

		#region TestSplitReceiptByQuantity_DoesNotExecuteIfFinalisedOrCancelled

		public void TestSplitReceiptByQuantity_DoesNotExecuteIfFinalisedOrCancelled()
		{
			var inventory = new List<WhsInventoryView>();
			inventory.Add(Factory.New<WhsInventoryView>());
			inventory[0].WI_InDocketLineUnits = 10m;
			inventory[0].WI_SplitQuantity = 4m;

			var receive = GetNewBusinessObject();
			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			receive.AutoFillSplitQuantities(inventory);
			AssertEquals(10m, inventory[0].WI_InDocketLineUnits);
			AssertEquals(1, Factory.Load<WhsReceive>(new ZQuery()).Length);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
			receive.WD_FinalisedDate = ZDateTimeOffset.Empty;

			((NotificationBuffer)receive.NotificationManager.Peek).Clear();
			receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			receive.AutoFillSplitQuantities(inventory);
			AssertEquals(10m, inventory[0].WI_InDocketLineUnits);
			AssertEquals(1, Factory.Load<WhsReceive>(new ZQuery()).Length);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		#endregion

		#region TestSplitReceiptByQuantity_DoesNotExecuteIfCreatedFromWorkOrder

		public void TestSplitReceiptByQuantity_DoesNotExecuteIfCreatedFromWorkOrder()
		{
			var inventory = new List<WhsInventoryView>() { Factory.New<WhsInventoryView>() };
			inventory[0].WI_InDocketLineUnits = 10m;
			inventory[0].WI_SplitQuantity = 4m;

			var receive = GetNewBusinessObject();
			var workOrder = Factory.New<WhsWorkOrder>();
			receive.WD_WD_ParentDocket = workOrder.PK;
			receive.AutoFillSplitQuantities(inventory);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(ReceiveErrorTypes.CannotPerformThisOperationBecauseReceiveWasCreatedFromWorkOrder));

			((NotificationBuffer)receive.NotificationManager.Peek).Clear();
			receive.SplitReceiptByQuantity();
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(ReceiveErrorTypes.CannotPerformThisOperationBecauseReceiveWasCreatedFromWorkOrder));
		}

		#endregion

		#region TestSplitReceiptByQuantity_DoesNotExecuteIfCreatedFromPickByBOM

		public void TestSplitReceiptByQuantity_DoesNotExecuteIfCreatedFromPickByBOM()
		{
			var inventory = new List<WhsInventoryView>() { Factory.New<WhsInventoryView>() };
			inventory[0].WI_InDocketLineUnits = 10m;
			inventory[0].WI_SplitQuantity = 4m;

			var receive = GetNewBusinessObject();
			var pick = Factory.New<WhsPick>();
			receive.WD_WP_ParentPickForReceive = pick.PK;
			receive.AutoFillSplitQuantities(inventory);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(ReceiveErrorTypes.CannotPerformThisOperationBecauseReceiveWasCreatedFromPickOrder));

			((NotificationBuffer)receive.NotificationManager.Peek).Clear();
			receive.SplitReceiptByQuantity();
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(ReceiveErrorTypes.CannotPerformThisOperationBecauseReceiveWasCreatedFromPickOrder));
		}

		#endregion

		#region TestSplitReceiptByQuantity_ChecksPackageGroupIDAndPerPackageQty

		public void TestSplitReceiptByQuantity_ChecksPackageGroupIDAndPerPackageQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.RemoveAreasOfTypeFromWarehouse(data.Whs1, "FRE");
			Factory.Save();

			var location = data.Whs1.FindLocation("RR1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "123-1", "ABC", 2m);
			inventory1.WI_WL = location.PK;

			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "123-1", "ABC", 2m);
			inventory2.WI_WL = location.PK;

			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "123-1", "", 2m);
			inventory3.WI_WL = location.PK;

			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "123-1");
			inventory4.WI_WL = location.PK;

			inventory1.WI_SplitQuantity = 8m; // split some of ABC on one line

			var notDivisibleErrorMessage = "Error: This Receipt cannot be split because one or more inventory's Split Quantity is not divisible by its Per Package Quantity.";
			var packageGroupPartiallySplitErrorMessage = "Error: This Receipt cannot be split because one or more Package Groups are not fully split.";
			var notify = (NotificationBuffer)receive.NotificationManager.Peek;
			AssertSplit(receive, notify, packageGroupPartiallySplitErrorMessage);

			// splitting full package amount on one line, but not all of ABC
			inventory1.WI_SplitQuantity = 10m;
			AssertSplit(receive, notify, packageGroupPartiallySplitErrorMessage);

			// make sure splitting nothing on a package is ok
			inventory1.WI_SplitQuantity = 0m;
			inventory4.WI_SplitQuantity = 10m; // to prevent no split quantity set error
			AssertSplit(receive, notify);

			// split both ABC lines, but one line is not fully split
			inventory1.WI_SplitQuantity = 8m;
			inventory2.WI_SplitQuantity = 10m;
			AssertSplit(receive, notify, packageGroupPartiallySplitErrorMessage);

			// both ABC lines are fully split, should be no error
			inventory1.WI_SplitQuantity = 10m;
			AssertSplit(receive, notify);

			// split quantity is not divisible by per package qty
			inventory3.WI_SplitQuantity = 5m;
			AssertSplit(receive, notify, notDivisibleErrorMessage);

			// splitting a multiple of per package qty
			inventory3.WI_SplitQuantity = 6m;
			AssertSplit(receive, notify);
		}

		void AssertSplit(WhsReceive receive, NotificationBuffer notify, string expectedErrorMessage = "")
		{
			Factory.Save();
			receive.SplitReceiptByQuantity();

			bool expectErrors = !string.IsNullOrEmpty(expectedErrorMessage);
			AssertEquals(expectErrors, notify.HasErrors);

			if (expectErrors)
			{
				AssertEquals(expectedErrorMessage, notify.AsString.Trim());
			}

			notify.Clear();
		}

		#endregion

		#region TestSplitReceiptByQuantity_CopiesCustomsDataAndPackageGroupIdAndPerPackageQty

		public void TestSplitReceiptByQuantity_CopiesCustomsDataAndPackageGroupIdAndPerPackageQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.RemoveAreasOfTypeFromWarehouse(data.Whs1, "FRE");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation.PK, "123-1", "ABC", 2m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation.PK, "123-1", "XYZ", 2m);
			inventory1.WI_SplitQuantity = 10m;
			Factory.Save();
			var customsDataPK = inventory1.CustomsData.PK;
			receive.SplitReceiptByQuantity();

			var splitReceipt = (WhsReceive)receive.RelatedSplits.Single();
			var newInventory = (WhsInventoryView)splitReceipt.Inventory.Single();
			AssertEquals("123", newInventory.CustomsData.WB_EntryKey);
			AssertEquals((ZShort)1, newInventory.CustomsData.WB_EntryLineNo);

			var newCustomsDataPK = newInventory.CustomsData.PK;
			AssertNotEquals(customsDataPK, newCustomsDataPK);
			AssertEquals("ABC", newInventory.PackageGroupId);
			AssertEquals(2m, newInventory.PerPackageQty);
			AssertEquals("ABC", newInventory.InDocketLine.WE_PackageGroupId);
			AssertEquals(2m, newInventory.InDocketLine.WE_PerPackageQty);
			AssertEquals(newCustomsDataPK, newInventory.InDocketLine.CustomsData.PK);
		}

		#endregion

		#region TestSplitReceiptByQuantity_PutawayTransfers

		public void TestSplitReceiptByQuantity_PutawayTransfers()
		{
			AssertPerformActionOnReceive((receive) => receive.SplitReceiptByQuantity());
		}

		#endregion

		#region TestSplitReceiptByQuantity_DocketStatus

		public void TestSplitReceiptByQuantity_DocketStatus_SplittingMultipleLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "REF1", ZDateTimeOffset.Today);
			var putawayInventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			var arrivalInventoryToSplit = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m);

			AssertEquals("Pre-condition: Docket Status", DocketStatus.Codes.Putaway, receive.WD_DocketStatus);
			AssertEquals("Pre-condition: Docket Line Status", InventoryStatus.Codes.Putaway, putawayInventory.InDocketLine.WE_OriginalInventoryStatus);
			AssertEquals("Pre-condition: Docket Line Status", InventoryStatus.Codes.Arrived, arrivalInventoryToSplit.InDocketLine.WE_OriginalInventoryStatus);
			putawayInventory.WI_SplitQuantity = 5m;
			arrivalInventoryToSplit.WI_SplitQuantity = 15m;

			Factory.Save();

			receive.SplitReceiptByQuantity();
			Factory.Save();

			var query = new ZQuery(WhsDocketSchema.WD_ExternalReference, SQLComparisonOperator.StartsWith, "REF1");
			var receives = Factory.Load<WhsReceive>(query);
			AssertEquals("Should have 2 receives (1 new split).", 2, receives.Length);

			var splitReceive = receives.Where(r => r.PK != receive.PK).Single();
			AssertEquals("ArrivalDate should be empty.", ZDateTimeOffset.Empty, splitReceive.WD_ArrivalDate);
			AssertEquals("The docket status of new split receive must be Entered.", DocketStatus.Codes.Entered, splitReceive.WD_DocketStatus);

			var splitReceiveLines = splitReceive.Lines;
			var line1 = splitReceiveLines.Single(l => l.WE_OP == data.Part1.PK);
			var line2 = splitReceiveLines.Single(l => l.WE_OP == data.Part2.PK);
			AssertEquals("WE_TransactionQuantity", 5m, line1.WE_TransactionQuantity);
			AssertEquals("WE_TransactionQuantity", 15m, line2.WE_TransactionQuantity);
			AssertEquals("WE_OriginalInventoryStatus must be Pending.", InventoryStatus.Codes.Pending, line1.WE_OriginalInventoryStatus);
			AssertEquals("WE_OriginalInventoryStatus must be Pending.", InventoryStatus.Codes.Pending, line2.WE_OriginalInventoryStatus);
		}

		#endregion

		#region TestSplitReceiptByQuantity_SplitsCustomsQuantitiesAndValues

		public void TestSplitReceiptByQuantity_SplitsCustomsQuantitiesAndValues()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REF1");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine.WE_BondedEntryKey = "KEY-1";
			receiveLine.WE_TransactionQuantity = 10m;

			var customsData = receiveLine.CustomsData;
			customsData.WB_EntryKey = "KEY";
			customsData.WB_EntryLineNo = (ZShort)1;
			customsData.WB_CustomsQty = 10m;
			customsData.WB_BondedWhsQty = 10m;
			customsData.WB_ValueForDuty = 100m;
			customsData.WB_TILV = 100m;
			customsData.WB_CustomsSecondQuantity = 20m;
			customsData.WB_CustomsThirdQuantity = 30m;

			receiveLine.SplitQuantity = 4m;
			receiveLine.RunPreSaveValidation();
			Factory.Save();

			receive.SplitReceiptByQuantity();
			Factory.Save();

			var query = new ZQuery(WhsDocketSchema.WD_ExternalReference, SQLComparisonOperator.StartsWith, "REF1");
			var receives = Factory.Load<WhsReceive>(query);
			AssertEquals("Should have 2 receives (1 new split).", 2, receives.Length);

			AssertEquals("WE_TransactionQuantity", 6m, receiveLine.WE_TransactionQuantity);
			AssertEquals("WB_CustomsQty", 6m, customsData.WB_CustomsQty);
			AssertEquals("WB_BondedWhsQty", 6m, customsData.WB_BondedWhsQty);
			AssertEquals("WB_ValueForDuty", 60m, customsData.WB_ValueForDuty);
			AssertEquals("WB_TILV", 60m, customsData.WB_TILV);
			AssertEquals("WB_CustomsSecondQuantity", 12m, customsData.WB_CustomsSecondQuantity);
			AssertEquals("WB_CustomsThirdQuantity", 18m, customsData.WB_CustomsThirdQuantity);

			var splitReceive = receives.Where(r => r.PK != receive.PK).Single();
			var newReceiveLine = splitReceive.Lines.Single();

			AssertEquals("WE_TransactionQuantity", 4m, newReceiveLine.WE_TransactionQuantity);
			var customsDataNewLine = newReceiveLine.CustomsData;
			AssertEquals("WB_CustomsQty", 4m, customsDataNewLine.WB_CustomsQty);
			AssertEquals("WB_BondedWhsQty", 4m, customsDataNewLine.WB_BondedWhsQty);
			AssertEquals("WB_ValueForDuty", 40m, customsDataNewLine.WB_ValueForDuty);
			AssertEquals("WB_TILV", 40m, customsDataNewLine.WB_TILV);
			AssertEquals("WB_CustomsSecondQuantity", 8m, customsDataNewLine.WB_CustomsSecondQuantity);
			AssertEquals("WB_CustomsThirdQuantity", 12m, customsDataNewLine.WB_CustomsThirdQuantity);
		}

		#endregion

		#endregion

		#region TestSplitReceiptByAreaType

		#region TestSplitReceiptByAreaType

		public void TestSplitReceiptByAreaType()
		{
			var data = new TestDataForInventory(Factory);

			var whs = Helper.CreateWarehouse("1", "A", 6, 1);
			var locations = whs.Rows.Single(r => r.WR_Name == "A").Locations;
			var area1 = whs.Areas.AddNew();
			var area2 = whs.Areas.AddNew();
			var area3 = whs.Areas.AddNew();
			area1.WA_Name = "area1";
			area2.WA_Name = "area2";
			area3.WA_Name = "area3";
			area1.WA_AreaType = AreaTypes.Codes.Bonded;
			area2.WA_AreaType = AreaTypes.Codes.Bonded;
			area3.WA_AreaType = AreaTypes.Codes.Excise;
			locations[0].WLV_WA_PutawayArea = area1.PK;
			locations[1].WLV_WA_PutawayArea = area2.PK;
			locations[2].WLV_WA_PutawayArea = area2.PK;
			locations[3].WLV_WA_PutawayArea = area3.PK;
			locations[4].WLV_WA_PutawayArea = area3.PK;
			locations[5].WLV_WA_PutawayArea = area3.PK;
			Factory.Save();

			var org = Helper.CreateClient();
			var product = Helper.CreateProduct(org, "P1");
			data.CreateSimpleInventoryManyLines(whs, org, product, new ZDecimal[6] { 1m, 2m, 3m, 4m, 5m, 6m }, "REF1", false);

			var receive = data.Receive11;
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			receive.Inventory[0].LocationString = "";

			receive.SplitReceiptByAreaType();
			var notify = (NotificationBuffer)receive.NotificationManager.Peek;
			AssertEquals("Should have error if not every line has a location", true, notify.ContainsNotificationType(ReceiveErrorTypes.NotEveryLineHasLocationSetForSplit));
			for (int i = 0; i < 6; i++)
			{
				receive.Inventory[i].LocationString = "A-1";
			}

			receive.SplitReceiptByAreaType();
			notify = (NotificationBuffer)receive.NotificationManager.Peek;
			AssertEquals("Should have error if only one area type used", true, notify.ContainsNotificationType(ReceiveErrorTypes.SplitByAreaTypeRequiresDifferentAreaTypes));

			for (int i = 0; i < 6; i++)
			{
				receive.Inventory[i].WI_WL = locations[i].PK;
			}

			receive.SplitReceiptByAreaType();
			AssertEquals("Should be saved", true, notify.ContainsNotificationType(ReceiveErrorTypes.SplitByAreaTypeRequiresReceiptToBeSaved));

			Factory.Save();
			receive.SplitReceiptByAreaType();
			AssertEquals("Should be saved", true, notify.ContainsNotificationType(ReceiveErrorTypes.SplitByAreaTypeRequiresReceiptToBeSaved));
			AssertEquals("Original receive should only have 3 lines after split", 3, receive.Inventory.Count);

			var query = new ZQuery(WhsDocketSchema.WD_ExternalReference, SQLComparisonOperator.StartsWith, "REF1");
			var receives = Factory.Load<WhsReceive>(query);
			AssertEquals("Should now be 2 receives (1 new split)", 2, receives.Length);
			AssertEquals("Receives should be split.", true, receives.All(r => r.Lines.Cast<WhsReceiveLine>().Select(l => l.PutawayLocationAreaType).Distinct().Count() == 1));
		}

		#endregion

		#region TestSplitReceiptByAreaType_WithLargerTotalWeight

		public void TestSplitReceiptByAreaType_WithLargerTotalWeight()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var area2 = Helper.CreateArea(data.Whs1, "A2", AreaTypes.Codes.Bonded);
			var area3 = Helper.CreateArea(data.Whs1, "A3", AreaTypes.Codes.Excise);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[0].WLV_WA_PutawayArea = area2.PK;
			locations[1].WLV_WA_PutawayArea = area2.PK;
			locations[2].WLV_WA_PutawayArea = area3.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REF1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, locations[0]);
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, locations[1]);
			var line3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40m, locations[2]);
			line1.CustomsData.WB_EntryKey = "ABC";
			line2.CustomsData.WB_EntryKey = "ABC";
			line3.CustomsData.WB_EntryKey = "ABC";

			Helper.SetProductWeightAndVolume(data.Part1, 999999.99m, Constants.Weight.Kilograms, 999999.2m, Constants.Volume.CubicMetres);

			receive.SplitReceiptByAreaType();
			var notify = (NotificationBuffer)receive.NotificationManager.Peek;
			AssertEquals("Should have error if only one area type used", true, notify.ContainsNotificationType(ReceiveErrorTypes.SplitByAreaTypeRequiresReceiptToBeSaved));

			receive.WD_TotalWeight = 1m; // Hack to save receive to DB so that it won't exceed the DB limit
			receive.WD_TotalCubic = 1m;
			AssertNoExceptionThrown(() => Factory.Save());

			receive.SplitReceiptByAreaType();
			AssertEquals("Original receive should only have 2 lines after split", 2, receive.Inventory.Count);

			var query = new ZQuery(WhsDocketSchema.WD_ExternalReference, SQLComparisonOperator.StartsWith, "REF1");
			var receives = Factory.Load<WhsReceive>(query);
			AssertEquals("Should now be 2 receives (1 new splits)", 2, receives.Length);
			AssertEquals("Receives should be split.", true, receives.All(r => r.Lines.Cast<WhsReceiveLine>().Select(l => l.PutawayLocationAreaType).Distinct().Count() == 1));

			var relatedSplits = receive.RelatedSplits;
			AssertEquals(1, relatedSplits.Count);
			AssertEquals(true, relatedSplits.HasErrors());

			var expectedErrorMsg = @"Error - WD_TotalCubic: The number 39,999,969.0 is too large, the maximum value allowed for Volume is 999,999.999.
Error - WD_TotalWeight: The number 40,000,000.60 is too large, the maximum value allowed for Weight is 999,999.999.";
			var errorMsg = string.Join("\r\n", relatedSplits.GetErrors().GetUniqueMessageList());
			AssertEquals(expectedErrorMsg, errorMsg);
		}

		#endregion

		#region TestSplitReceiptByAreaType_DoesNotExecuteIfFinalisedOrCancelled

		public void TestSplitReceiptByAreaType_DoesNotExecuteIfFinalisedOrCancelled()
		{
			var receive = GetNewBusinessObject();
			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			receive.SplitReceiptByAreaType();
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
			receive.WD_FinalisedDate = ZDateTimeOffset.Empty;

			((NotificationBuffer)receive.NotificationManager.Peek).Clear();
			receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			receive.SplitReceiptByAreaType();
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		#endregion

		#region TestSplitReceiptByAreaType_DoesNotExecuteIfCreatedFromWorkOrder

		public void TestSplitReceiptByAreaType_DoesNotExecuteIfCreatedFromWorkOrder()
		{
			var receive = GetNewBusinessObject();
			var workOrder = Factory.New<WhsWorkOrder>();
			receive.WD_WD_ParentDocket = workOrder.PK;
			receive.SplitReceiptByAreaType();
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(ReceiveErrorTypes.CannotPerformThisOperationBecauseReceiveWasCreatedFromWorkOrder));
		}

		#endregion

		#region TestSplitReceiptByAreaType_DoesNotExecuteIfCreatedFromPickByBOM

		public void TestSplitReceiptByAreaType_DoesNotExecuteIfCreatedFromPickByBOM()
		{
			var receive = GetNewBusinessObject();
			var pick = Factory.New<WhsPick>();
			receive.WD_WP_ParentPickForReceive = pick.PK;
			receive.SplitReceiptByAreaType();
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(ReceiveErrorTypes.CannotPerformThisOperationBecauseReceiveWasCreatedFromPickOrder));
		}

		#endregion

		#region TestSplitReceiptByAreaType_ChecksPackageGroupIsSplitFully

		public void TestSplitReceiptByAreaType_ChecksPackageGroupIsSplitFully()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			data.Whs1.Areas.Single(a => a.WA_AreaType == "FRE").WA_AreaType = "EXC";
			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");
			location1.WLV_WA_PutawayArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "BON").PK;
			location2.WLV_WA_PutawayArea = data.Whs1.Areas.Single(a => a.WA_AreaType == "EXC").PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;

			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1.PK, "123-1", "ABC", 2m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, location2.PK, "123-1", "ABC", 2m);
			Factory.Save();

			var notify = (NotificationBuffer)receive.NotificationManager.Peek;
			receive.SplitReceiptByAreaType();
			AssertEquals(true, notify.HasErrors);
			AssertEquals("Error: This Receipt cannot be split by area because one or more Package Groups will become partially split.", notify.AsString.Trim());
			notify.Clear(); // clean up

			inventory2.InDocketLine.WE_PackageGroupId = "";
			Factory.Save();
			receive.SplitReceiptByAreaType();
			AssertEquals(false, notify.HasErrors);
		}

		#endregion

		#region TestSplitReceiptByAreaType_PutawayTransfers

		public void TestSplitReceiptByAreaType_PutawayTransfers()
		{
			AssertPerformActionOnReceive((receive) => receive.SplitReceiptByAreaType());
		}

		#endregion

		#endregion

		#endregion

		#region Palletize Lines

		#region TestPalletizeLines

		public void TestPalletizeLines()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(50, 20, 40, 21, 0, false);
			Helper.CreateProductUnit(data.Part1, "PLT", 20);

			var receive = data.Receive11;
			var selected = new List<WhsReceiveLine>();
			selected.Add(receive.Lines[0]);
			receive.PalletizeLines(selected);
			AssertEquals("Should of generated 2 new lines", 6, receive.Lines.Count);
			AssertEquals("50 units should now be 20", 20m, receive.Lines[0].WE_TransactionQuantity);
			AssertEquals("Should have new line of 20 units", 20m, receive.Lines[4].WE_TransactionQuantity);
			AssertEquals("Should have new line of 10 units", 10m, receive.Lines[5].WE_TransactionQuantity);
		}

		#endregion

		#region TestPalletizeLines_PutawayTransfers

		public void TestPalletizeLines_PutawayTransfers()
		{
			AssertPerformActionOnSelectedInventory_PutawayTransfersReceive((receive, inventory) => receive.PalletizeLines(inventory));
		}

		#endregion

		#region TestPalletizeLinesChecksArgument

		[ExpectException(typeof(ArgumentNullException))]
		public void TestPalletizeLinesChecksArgument()
		{
			Receive.PalletizeLines(null);
		}

		#endregion

		#region TestPalletizeLines_DoesNotExecuteIfFinalisedOrCancelled

		public void TestPalletizeLines_DoesNotExecuteIfFinalisedOrCancelled()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(50, 0, 0, 0, 0);
			Helper.CreateProductUnit(data.Part1, "PLT", 20);

			var receive = data.Receive11;
			var selected = new List<WhsReceiveLine>();
			selected.AddRange(receive.Lines.ToList<WhsReceiveLine>());
			receive.PalletizeLines(selected);
			AssertEquals("Should not have generated new lines because the docket is finalised", 1, receive.Lines.Count);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));

			((NotificationBuffer)receive.NotificationManager.Peek).Clear();
			receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			receive.PalletizeLines(selected);
			AssertEquals("Should not have generated new lines because the docket is cancelled", 1, receive.Lines.Count);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		#endregion

		#region TestPalletizeLines_CreatedFromWorkOrder_SplitLinks

		public void TestPalletizeLines_CreatedFromWorkOrder_SplitLinks()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var component2 = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateProductBOM(data.Part2, data.Part1, 1.5m, "UNT");
			Helper.CreateProductBOM(data.Part2, component2, 1m, "UNT");
			Helper.CreateProductUnit(data.Part2, "PLT", 7);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 15m, data.Whs1.DefaultLocation);
			receiveLine1.WE_PartAttrib1 = "RED";
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 6m, data.Whs1.DefaultLocation);
			receiveLine2.WE_PartAttrib1 = "BLUE";
			receive2.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive2);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", component2, 14m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 14m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var receive = workOrder.Receive;
			AssertEquals("Should have created 2 product.", 2, receive.Lines.Count);

			var inventory1 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 10m);
			var inventory2 = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 4m);

			var workOrderLine1 = (WhsWorkOrderLine)workOrder.Lines.Single(l => l.WE_TransactionQuantity == 10m);
			var componentLine1_1 = workOrderLine1.ChildComponentLines.Single(c => c.WE_OP == data.Part1.PK);
			var componentLine1_2 = workOrderLine1.ChildComponentLines.Single(c => c.WE_OP == component2.PK);

			var workOrderLine2 = (WhsWorkOrderLine)workOrder.Lines.Single(l => l.WE_TransactionQuantity == 4m);
			var componentLine2_1 = workOrderLine2.ChildComponentLines.Single(c => c.WE_OP == data.Part1.PK);
			var componentLine2_2 = workOrderLine2.ChildComponentLines.Single(c => c.WE_OP == component2.PK);

			var links1 = inventory1.BOMComponentLinks;
			AssertEquals("Should have two Links.", 2, links1.Count());
			var link1_1 = links1.Single(l => l.WIP_WE_ComponentLine == componentLine1_1.PK);
			AssertEquals("Linked correct Component Qty.", 15m, link1_1.WIP_ComponentQuantity);
			var link1_2 = links1.Single(l => l.WIP_WE_ComponentLine == componentLine1_2.PK);
			AssertEquals("Linked correct Component Qty.", 10m, link1_2.WIP_ComponentQuantity);

			var links2 = inventory2.BOMComponentLinks;
			AssertEquals("Should have two Links.", 2, links2.Count());
			var link2_1 = links2.Single(l => l.WIP_WE_ComponentLine == componentLine2_1.PK);
			AssertEquals("Linked correct Component Qty.", 6m, link2_1.WIP_ComponentQuantity);
			var link2_2 = links2.Single(l => l.WIP_WE_ComponentLine == componentLine2_2.PK);
			AssertEquals("Linked correct Component Qty.", 4m, link2_2.WIP_ComponentQuantity);

			AssertNoExceptionThrown(() => Factory.Save());

			var selected = new List<WhsReceiveLine>();
			selected.AddRange(receive.Lines.ToList<WhsReceiveLine>());
			receive.PalletizeLines(selected);

			AssertEquals("Should split into 3 lines.", 3, receive.Lines.Count);
			var splitInventory1 = receive.Lines.Single(l => l.WE_TransactionQuantity == 7m);
			var splitInventory2 = receive.Lines.First(l => l.WE_TransactionQuantity == 3m);
			var splitInventory3 = receive.Lines.Single(l => l.WE_TransactionQuantity == 4m);

			var splitLinks1 = splitInventory1.BOMComponentLinks;
			AssertEquals("Should have two Links.", 2, splitLinks1.Count());
			var splitLinks1_1 = splitLinks1.Single(l => l.WIP_WE_ComponentLine == componentLine1_1.PK);
			AssertEquals("Split link correctly.", 10.5m, splitLinks1_1.WIP_ComponentQuantity);
			var splitLinks1_2 = splitLinks1.Single(l => l.WIP_WE_ComponentLine == componentLine1_2.PK);
			AssertEquals("Split link correctly.", 7m, splitLinks1_2.WIP_ComponentQuantity);

			var splitLinks2 = splitInventory2.BOMComponentLinks;
			AssertEquals("Should have two Links.", 2, splitLinks2.Count());
			var splitLinks2_1 = splitLinks2.Single(l => l.WIP_WE_ComponentLine == componentLine1_1.PK);
			AssertEquals("Split link correctly.", 4.5m, splitLinks2_1.WIP_ComponentQuantity);
			var splitLinks2_2 = splitLinks2.Single(l => l.WIP_WE_ComponentLine == componentLine1_2.PK);
			AssertEquals("Split link correctly.", 3m, splitLinks2_2.WIP_ComponentQuantity);

			var splitLinks3 = splitInventory3.BOMComponentLinks;
			AssertEquals("Should have two Links.", 2, splitLinks3.Count());
			var splitLinks3_1 = splitLinks3.Single(l => l.WIP_WE_ComponentLine == componentLine2_1.PK);
			AssertEquals("This link is not changed.", 6m, splitLinks3_1.WIP_ComponentQuantity);
			var splitLinks3_2 = splitLinks3.Single(l => l.WIP_WE_ComponentLine == componentLine2_2.PK);
			AssertEquals("This link is not changed.", 4m, splitLinks3_2.WIP_ComponentQuantity);
		}

		#endregion

		#region TestPalletizeLines_CreatedFromWorkOrder_OldDataWithoutLinks

		public void TestPalletizeLines_CreatedFromWorkOrder_OldDataWithoutLinks()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part2, data.Part1, 1m, "UNT");
			Helper.CreateProductUnit(data.Part2, "PLT", 10);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, "ExtRef", data.Part2, 15m);
			workOrder.WD_DocketSubType = WorkOrderType.Codes.Assemble;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;

			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			var receive = workOrder.Receive;
			AssertEquals("Should have created 1 product.", 1, receive.Lines.Count);

			var inventory = (WhsReceiveLine)receive.Lines.Single(l => l.WE_TransactionQuantity == 15m);
			inventory.BOMComponentLinks.DeleteAll();
			AssertNoExceptionThrown(() => Factory.Save());

			var selected = new List<WhsReceiveLine>();
			selected.AddRange(receive.Lines.ToList<WhsReceiveLine>());
			receive.PalletizeLines(selected);

			AssertEquals("Should split into 2 lines.", 2, receive.Lines.Count);
			var splitInventory1 = receive.Lines.Single(l => l.WE_TransactionQuantity == 10m);
			var splitInventory2 = receive.Lines.First(l => l.WE_TransactionQuantity == 5m);

			AssertEquals("No Links to split.", 0, splitInventory1.BOMComponentLinks.Count());
			AssertEquals("No Links to split.", 0, splitInventory2.BOMComponentLinks.Count());
		}

		#endregion

		#region TestPalletizeLines_RecalculateTotalLinesUnits

		public void TestPalletizeLines_RecalculateTotalLinesUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, "UNT", "PLT", 25m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m);
			receive.Inventory[0].WI_F3_NKPackType = "PLT";

			AssertEquals("Precondition - TotalLinesUnits was calculated correctly", 100m, receive.WD_TotalUnitsFromLines);

			var linesToSplit = new List<WhsReceiveLine>();
			linesToSplit.AddRange(receive.Lines.ToList<WhsReceiveLine>());
			receive.PalletizeLines(linesToSplit);
			AssertEquals("TotalLineUnits was calculated correctly", 100m, receive.WD_TotalUnitsFromLines);
		}

		#endregion

		#region TestPalletizeLines_TotalWeightAndVolumeIsNotModified

		public void TestPalletizeLines_TotalWeightAndVolumeIsNotModified()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 20m, Constants.Weight.Kilograms, 0.002m, Constants.Volume.CubicMetres);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 25m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m);
			inventory.WI_F3_NKPackType = Constants.PkgUnit.Pallet;
			AssertEquals("Precondition - TotalWeight was calculated correctly", 2000m, receive.WD_TotalWeight);
			AssertEquals("Precondition - TotalVolume was calculated correctly", 0.2m, receive.WD_TotalCubic);

			receive.WD_TotalWeight = 500m;
			receive.WD_TotalCubic = 0.5m;
			AssertEquals("Precondition - TotalWeight was updated.", 500m, receive.WD_TotalWeight);
			AssertEquals("Precondition - TotalVolume was updated.", 0.5m, receive.WD_TotalCubic);

			var linesToSplit = new List<WhsReceiveLine>();
			linesToSplit.AddRange(receive.Lines.ToList<WhsReceiveLine>());
			receive.PalletizeLines(linesToSplit);
			AssertEquals("TotalWeight should not be modified.", 500m, receive.WD_TotalWeight);
			AssertEquals("TotalVolume should not be modified.", 0.5m, receive.WD_TotalCubic);
		}

		#endregion

		#region TestPalletizeLines_SplitsCustomsQuantitiesAndValues

		public void TestPalletizeLines_SplitsCustomsQuantitiesAndValues()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.CreateProductUnit(data.Part1, "PLT", 6);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine.WE_BondedEntryKey = "KEY-1";
			receiveLine.WE_TransactionQuantity = 10m;

			var customsData = receiveLine.CustomsData;
			customsData.WB_EntryKey = "KEY";
			customsData.WB_EntryLineNo = (ZShort)1;
			customsData.WB_CustomsQty = 10m;
			customsData.WB_BondedWhsQty = 10m;
			customsData.WB_ValueForDuty = 100m;
			customsData.WB_TILV = 100m;
			customsData.WB_CustomsSecondQuantity = 20m;
			customsData.WB_CustomsThirdQuantity = 30m;
			receiveLine.RunPreSaveValidation();
			Factory.Save();

			var selected = new List<WhsReceiveLine>();
			selected.Add(receive.Lines[0]);
			receive.PalletizeLines(selected);
			AssertEquals("Should have generated 1 new line", 2, receive.Lines.Count);

			AssertEquals("WE_TransactionQuantity", 6m, receiveLine.WE_TransactionQuantity);
			AssertEquals("WB_CustomsQty", 6m, customsData.WB_CustomsQty);
			AssertEquals("WB_BondedWhsQty", 6m, customsData.WB_BondedWhsQty);
			AssertEquals("WB_ValueForDuty", 60m, customsData.WB_ValueForDuty);
			AssertEquals("WB_TILV", 60m, customsData.WB_TILV);
			AssertEquals("WB_CustomsSecondQuantity", 12m, customsData.WB_CustomsSecondQuantity);
			AssertEquals("WB_CustomsThirdQuantity", 18m, customsData.WB_CustomsThirdQuantity);

			var newReceiveLine = receive.Lines.Single(l => l.WE_TransactionQuantity == 4m);
			var customsDataNewLine = newReceiveLine.CustomsData;
			AssertEquals("WB_CustomsQty", 4m, customsDataNewLine.WB_CustomsQty);
			AssertEquals("WB_BondedWhsQty", 4m, customsDataNewLine.WB_BondedWhsQty);
			AssertEquals("WB_ValueForDuty", 40m, customsDataNewLine.WB_ValueForDuty);
			AssertEquals("WB_TILV", 40m, customsDataNewLine.WB_TILV);
			AssertEquals("WB_CustomsSecondQuantity", 8m, customsDataNewLine.WB_CustomsSecondQuantity);
			AssertEquals("WB_CustomsThirdQuantity", 12m, customsDataNewLine.WB_CustomsThirdQuantity);
		}

		public void TestPalletizeLines_SplitsCustomsQuantitiesAndValues_SplitToMultipleLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.CreateProductUnit(data.Part1, "PLT", 4);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine.WE_BondedEntryKey = "KEY-1";
			receiveLine.WE_TransactionQuantity = 10m;

			var customsData = receiveLine.CustomsData;
			customsData.WB_EntryKey = "KEY";
			customsData.WB_EntryLineNo = (ZShort)1;
			customsData.WB_CustomsQty = 10m;
			customsData.WB_BondedWhsQty = 10m;
			customsData.WB_ValueForDuty = 100m;
			customsData.WB_TILV = 100m;
			customsData.WB_CustomsSecondQuantity = 20m;
			customsData.WB_CustomsThirdQuantity = 30m;
			receiveLine.RunPreSaveValidation();
			Factory.Save();

			var selected = new List<WhsReceiveLine>();
			selected.Add(receive.Lines[0]);
			receive.PalletizeLines(selected);
			AssertEquals("Should have generated 2 new lines", 3, receive.Lines.Count);

			AssertEquals("WE_TransactionQuantity", 4m, receiveLine.WE_TransactionQuantity);
			AssertEquals("WB_CustomsQty", 4m, customsData.WB_CustomsQty);
			AssertEquals("WB_BondedWhsQty", 4m, customsData.WB_BondedWhsQty);
			AssertEquals("WB_ValueForDuty", 40m, customsData.WB_ValueForDuty);
			AssertEquals("WB_TILV", 40m, customsData.WB_TILV);
			AssertEquals("WB_CustomsSecondQuantity", 8m, customsData.WB_CustomsSecondQuantity);
			AssertEquals("WB_CustomsThirdQuantity", 12m, customsData.WB_CustomsThirdQuantity);

			var newReceiveLine1 = receive.Lines.Single(l => l.WE_TransactionQuantity == 4m && l.PK != receiveLine.PK);
			var customsDataNewLine1 = newReceiveLine1.CustomsData;
			AssertEquals("WB_CustomsQty", 4m, customsDataNewLine1.WB_CustomsQty);
			AssertEquals("WB_BondedWhsQty", 4m, customsDataNewLine1.WB_BondedWhsQty);
			AssertEquals("WB_ValueForDuty", 40m, customsDataNewLine1.WB_ValueForDuty);
			AssertEquals("WB_TILV", 40m, customsDataNewLine1.WB_TILV);
			AssertEquals("WB_CustomsSecondQuantity", 8m, customsDataNewLine1.WB_CustomsSecondQuantity);
			AssertEquals("WB_CustomsThirdQuantity", 12m, customsDataNewLine1.WB_CustomsThirdQuantity);

			var newReceiveLine2 = receive.Lines.Single(l => l.WE_TransactionQuantity == 2m);
			var customsDataNewLine2 = newReceiveLine2.CustomsData;
			AssertEquals("WB_CustomsQty", 2m, customsDataNewLine2.WB_CustomsQty);
			AssertEquals("WB_BondedWhsQty", 2m, customsDataNewLine2.WB_BondedWhsQty);
			AssertEquals("WB_ValueForDuty", 20m, customsDataNewLine2.WB_ValueForDuty);
			AssertEquals("WB_TILV", 20m, customsDataNewLine2.WB_TILV);
			AssertEquals("WB_CustomsSecondQuantity", 4m, customsDataNewLine2.WB_CustomsSecondQuantity);
			AssertEquals("WB_CustomsThirdQuantity", 6m, customsDataNewLine2.WB_CustomsThirdQuantity);
		}

		#endregion

		#endregion

		#region Generate Pallet IDs

		#region TestGenerateSequentialPalletIDs

		public void TestGenerateSequentialPalletIDs_EmptySelectedDocketLines_DoesNothing()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 50, 20, 40 }, false);
			Factory.Save();

			var receive = data.Receive11;

			var mockGenerator = new Mock<IPalletIDGenerator>();

			using (ObjectFactory.Substitute(mockGenerator.Object))
			{
				receive.GenerateSequentialPalletIDs(new List<WhsReceiveLine>());
				mockGenerator.Verify(g => g.GenerateIDs(It.IsAny<WhsReceive>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Never);
			}

			for (var i = 0; i < 3; i++)
			{
				AssertEquals(true, receive.Lines[i].WE_PalletID.IsEmpty);
				AssertNoWarnings(receive.Lines[i].WE_PalletIDInfo);
			}
		}

		public void TestGenerateSequentialPalletIDs_AppliesIdsToSelectedSelectedDocketLines()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 50, 20, 40 }, false);
			Factory.Save();

			var receive = data.Receive11;

			var selectedLines = receive.Lines.Cast<WhsReceiveLine>().Where(l => l.WE_TransactionQuantity > 20m);

			var ids = new List<GeneratedID> { new GeneratedID("0001", 1), new GeneratedID("0002", 2) };

			var mockGenerator = new Mock<IPalletIDGenerator>();
			mockGenerator.Setup(g => g.GenerateIDs(It.IsAny<WhsReceive>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>())).Returns(ids);

			using (ObjectFactory.Substitute(mockGenerator.Object))
			{
				receive.GenerateSequentialPalletIDs(selectedLines);
				mockGenerator.Verify(g => g.GenerateIDs(receive, 2, true, 1), Times.Once);
			}

			AssertEquals("0001", receive.Lines[0].WE_PalletID);
			AssertEquals(true, receive.Lines[1].WE_PalletID.IsEmpty);
			AssertEquals("0002", receive.Lines[2].WE_PalletID);

			AssertNoWarnings(receive.Lines[0].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[1].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[2].WE_PalletIDInfo);
		}

		public void TestGenerateSequentialPalletIDs_DoesNotOverwriteExistingPalletIDs()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 50, 20, 40 }, false);
			Factory.Save();

			var receive = data.Receive11;
			receive.Lines[0].WE_PalletID = "0002";
			receive.Lines[2].WE_PalletID = "BLAH";

			var selectedLines = receive.Lines.Cast<WhsReceiveLine>();

			var ids = new List<GeneratedID> { new GeneratedID("0001", 1) };

			var mockGenerator = new Mock<IPalletIDGenerator>();
			mockGenerator.Setup(g => g.GenerateIDs(It.IsAny<WhsReceive>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>())).Returns(ids);

			using (ObjectFactory.Substitute(mockGenerator.Object))
			{
				receive.GenerateSequentialPalletIDs(selectedLines);
				mockGenerator.Verify(g => g.GenerateIDs(receive, 1, true, 1), Times.Once);
			}

			AssertEquals("0002", receive.Lines[0].WE_PalletID);
			AssertEquals("0001", receive.Lines[1].WE_PalletID);
			AssertEquals("BLAH", receive.Lines[2].WE_PalletID);

			AssertNoWarnings(receive.Lines[0].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[1].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[2].WE_PalletIDInfo);
		}

		public void TestGenerateSequentialPalletIDs_DoesNotExecuteIfFinalisedOrCancelled()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(50, 0, 0, 0, 0, true);
			Factory.Save();
			var receive = data.Receive11;

			var selectedLines = receive.Lines.Cast<WhsReceiveLine>();
			receive.GenerateSequentialPalletIDs(selectedLines);
			AssertEquals("Should not generate pallet id because the docket is finalised", true, receive.Lines[0].WE_PalletID.IsEmpty);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));

			((NotificationBuffer)receive.NotificationManager.Peek).Clear();
			receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			receive.GenerateSequentialPalletIDs(selectedLines);
			AssertEquals("Should not generate pallet id because the docket is cancelled", true, receive.Lines[0].WE_PalletID.IsEmpty);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		[TestDate(2015, 5, 10)]
		public void TestGenerateSequentialPalletIDs_DoesNotProcessInvalidLines()
		{
			// Invalid Lines are Lines with
			// 1. No product
			// 2. Error Notifications
			// 3. Finalised Status

			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(50, 40, 30, 25, 21, false);
			Factory.Save();

			var receive = data.Receive11;

			receive.Lines[0].WE_OP = ZGuid.Empty;   // line 0 - invalid because has no product
			receive.Lines[1].WE_OP = ZGuid.Invalid; // line 1 - invalid because has errors
			receive.Lines[2].WE_CurrentInventoryStatus = InventoryStatus.Codes.Available; // line 2 - invalid because its finalised

			var selectedLines = receive.Lines.Cast<WhsReceiveLine>();

			var ids = new List<GeneratedID> { new GeneratedID("0001", 1), new GeneratedID("0002", 2) };

			var mockGenerator = new Mock<IPalletIDGenerator>();
			mockGenerator.Setup(g => g.GenerateIDs(It.IsAny<WhsReceive>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>())).Returns(ids);

			using (ObjectFactory.Substitute(mockGenerator.Object))
			{
				receive.GenerateSequentialPalletIDs(selectedLines);
				mockGenerator.Verify(g => g.GenerateIDs(receive, 2, true, 1), Times.Once);
			}

			AssertEquals("Line 0 should not have no palletid", true, receive.Lines[0].WE_PalletID.IsEmpty);
			AssertEquals("Line 1 should not have no palletid", true, receive.Lines[1].WE_PalletID.IsEmpty);
			AssertEquals("Line 2 should not have no palletid", true, receive.Lines[2].WE_PalletID.IsEmpty);
			AssertEquals("Line 3 should have palletid", "0001", receive.Lines[3].WE_PalletID);
			AssertEquals("Line 4 should have palletid", "0002", receive.Lines[4].WE_PalletID);

			for (var i = 0; i < 3; i++)
			{
				AssertEquals(1, receive.Lines[i].RowWarnings.Count());
				AssertEquals("The system has not generated a Pallet ID for this line because it does not have a product, " +
					"or it has errors or it is finalized", receive.Lines[i].RowWarnings.First().Message);
			}
			AssertNoWarnings(receive.Lines[3].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[4].WE_PalletIDInfo);
		}

		[TestDate(2015, 5, 10)]
		public void TestGenerateSequentialPalletIDs_WarnsIfNoMorePalletIDsAvailable()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 50, 20, 30, 40 }, false);
			Factory.Save();

			var receive = data.Receive11;

			var selectedLines = receive.Lines.Cast<WhsReceiveLine>();

			var ids = new List<GeneratedID> { new GeneratedID("9998", 9998), new GeneratedID("9999", 9999) };

			var mockGenerator = new Mock<IPalletIDGenerator>();
			mockGenerator.Setup(g => g.GenerateIDs(It.IsAny<WhsReceive>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>())).Returns(ids);

			using (ObjectFactory.Substitute(mockGenerator.Object))
			{
				receive.GenerateSequentialPalletIDs(selectedLines);
				mockGenerator.Verify(g => g.GenerateIDs(receive, 4, true, 1), Times.Once);
			}

			AssertEquals("9998", receive.Lines[0].WE_PalletID);
			AssertEquals("9999", receive.Lines[1].WE_PalletID);
			AssertEquals(true, receive.Lines[2].WE_PalletID.IsEmpty);
			AssertEquals(true, receive.Lines[3].WE_PalletID.IsEmpty);

			AssertNoWarnings(receive.Lines[0].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[1].WE_PalletIDInfo);
			for (var i = 2; i < 4; i++)
			{
				AssertEquals(1, receive.Lines[i].RowWarnings.Count());
				AssertEquals("The system has not generated a Pallet ID for this line because all Pallet ID's have been allocated. " +
					"You must split this job to access more Pallet ID's.", receive.Lines[i].RowWarnings.First().Message);
			}
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestGenerateSequentialPalletIDs_ChecksArgument()
		{
			Receive.GenerateSequentialPalletIDs(null);
		}

		public void TestGenerateSerialNumbers_PutawayTransfers()
		{
			AssertPerformActionOnSelectedInventory_PutawayTransfers((receive, inventory) => receive.GenerateSerialNumbers(inventory.Select(i => (WhsReceiveLine)i.InDocketLine)));
		}

		[UseSnapshotProtection]
		public void TestGenerateSequentialPalletIDsCaching_EndToEnd()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 50, 20, 40 }, finalise: false);
			data.Receive11.Client.MiscServ.OM_WhsGenerateSSCCOnInbound = true;

			var warehouseAddressHeader = Factory.Load<OrgHeader>(data.Receive11.Warehouse.WarehouseAddress.OA_OH);
			var cusCode = warehouseAddressHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GS1, "1111111");
			cusCode.OK_OA_PremisesAddress = data.Receive11.Warehouse.WarehouseAddress.PK;

			Factory.Save();

			var receive = data.Receive11;

			var selectedLines = receive.Lines.Cast<WhsReceiveLine>().Where(l => l.WE_TransactionQuantity > 20m);

			var notificationBuffer = new TestNotificationBuffer(true);
			receive.NotificationManager.Push(notificationBuffer);

			receive.GenerateSequentialPalletIDs(selectedLines);

			AssertEquals("011111110000000014", receive.Lines[0].WE_PalletID);
			AssertEquals(true, receive.Lines[1].WE_PalletID.IsEmpty);
			AssertEquals("011111110000000021", receive.Lines[2].WE_PalletID);

			AssertNoWarnings(receive.Lines[0].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[1].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[2].WE_PalletIDInfo);

			AssertNotNull(notificationBuffer.LastQueryUserEventArgs);
			Assert(notificationBuffer.LastQueryUserEventArgs is QueryUserYesNoEventArgs);
			AssertEquals(
				"Client 1 does not have a GS1 company prefix, would you like to use the warehouse GS1 company prefix?",
				((QueryUserYesNoEventArgs)notificationBuffer.LastQueryUserEventArgs).Message);
			AssertEquals(
				"Generate SSCC Number",
				((QueryUserYesNoEventArgs)notificationBuffer.LastQueryUserEventArgs).Caption);

			notificationBuffer.LastQueryUserEventArgs = null;

			selectedLines = receive.Lines.Cast<WhsReceiveLine>().Where(l => l.WE_TransactionQuantity == 20m);

			receive.GenerateSequentialPalletIDs(selectedLines);

			AssertEquals("011111110000000014", receive.Lines[0].WE_PalletID);
			AssertEquals("011111110000000038", receive.Lines[1].WE_PalletID);
			AssertEquals("011111110000000021", receive.Lines[2].WE_PalletID);

			AssertNoWarnings(receive.Lines[0].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[1].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[2].WE_PalletIDInfo);

			AssertNull(notificationBuffer.LastQueryUserEventArgs);
		}

		#endregion

		#region TestGenerateIdenticalPalletIDs

		public void TestGenerateIdenticalPalletIDs_EmptySelectedDocketLines_DoesNothing()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 50, 20, 40 }, false);
			Factory.Save();

			var receive = data.Receive11;

			var mockGenerator = new Mock<IPalletIDGenerator>();

			using (ObjectFactory.Substitute(mockGenerator.Object))
			{
				receive.GenerateIdenticalPalletIDs(new List<WhsReceiveLine>());
				mockGenerator.Verify(g => g.GenerateIDs(It.IsAny<WhsReceive>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Never);
			}

			for (var i = 0; i < 3; i++)
			{
				AssertEquals(true, receive.Lines[i].WE_PalletID.IsEmpty);
				AssertNoWarnings(receive.Lines[i].WE_PalletIDInfo);
			}
		}

		[TestDate(2015, 5, 10)]
		public void TestGenerateIdenticalPalletIDs_AppliesSingleIdToSelected()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 50, 20, 40 }, false);
			Factory.Save();

			var receive = data.Receive11;

			var selectedLines = receive.Lines.Cast<WhsReceiveLine>().Where(l => l.WE_TransactionQuantity > 20m);

			var ids = new List<GeneratedID> { new GeneratedID("0001", 1) };

			var mockGenerator = new Mock<IPalletIDGenerator>();
			mockGenerator.Setup(g => g.GenerateIDs(It.IsAny<WhsReceive>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>())).Returns(ids);

			using (ObjectFactory.Substitute(mockGenerator.Object))
			{
				receive.GenerateIdenticalPalletIDs(selectedLines);
				mockGenerator.Verify(g => g.GenerateIDs(receive, 1, true, 1), Times.Once);
			}

			AssertEquals("0001", receive.Lines[0].WE_PalletID);
			AssertEquals(true, receive.Lines[1].WE_PalletID.IsEmpty);
			AssertEquals("0001", receive.Lines[2].WE_PalletID);

			AssertNoWarnings(receive.Lines[0].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[1].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[2].WE_PalletIDInfo);
		}

		public void TestGenerateIdenticalPalletIDs_UsesFirstPalletIDFound()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 50, 20, 40 }, false);
			Factory.Save();

			var receive = data.Receive11;
			receive.Lines[1].WE_PalletID = "0002";

			var selectedLines = receive.Lines.Cast<WhsReceiveLine>();

			receive.NotificationManager.Push(new NotificationBufferWithDefaultResponse(false));
			receive.GenerateIdenticalPalletIDs(selectedLines);
			AssertEquals(ZString.Empty, receive.Lines[0].WE_PalletID);
			AssertEquals("0002", receive.Lines[1].WE_PalletID);
			AssertEquals(ZString.Empty, receive.Lines[2].WE_PalletID);

			AssertNoWarnings(receive.Lines[0].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[1].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[2].WE_PalletIDInfo);

			receive.NotificationManager.Push(new NotificationBufferWithDefaultResponse(true));
			receive.GenerateIdenticalPalletIDs(selectedLines);
			AssertEquals("0002", receive.Lines[0].WE_PalletID);
			AssertEquals("0002", receive.Lines[1].WE_PalletID);
			AssertEquals("0002", receive.Lines[2].WE_PalletID);

			AssertNoWarnings(receive.Lines[0].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[1].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[2].WE_PalletIDInfo);
		}

		public void TestGenerateIdenticalPalletIDs_DoesOverwriteExistingPalletIDs()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 50, 20, 40 }, false);
			Factory.Save();

			var receive = data.Receive11;
			receive.WD_DocketID = "W00001234";
			receive.Lines[0].WE_PalletID = "BLAH";
			receive.Lines[2].WE_PalletID = "SHEEP";

			var selectedLines = receive.Lines.Cast<WhsReceiveLine>();
			receive.GenerateIdenticalPalletIDs(selectedLines);
			AssertEquals("BLAH", receive.Lines[0].WE_PalletID);
			AssertEquals("BLAH", receive.Lines[1].WE_PalletID);
			AssertEquals("BLAH", receive.Lines[2].WE_PalletID);

			AssertNoWarnings(receive.Lines[0].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[1].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[2].WE_PalletIDInfo);
		}

		public void TestGenerateIdenticalPalletIDs_DoesNotExecuteIfFinalisedOrCancelled()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(50, 0, 0, 0, 0, true);
			Factory.Save();
			var receive = data.Receive11;

			var selectedLines = receive.Lines.Cast<WhsReceiveLine>();

			receive.GenerateIdenticalPalletIDs(selectedLines);
			AssertEquals("Should not generate pallet id because the docket is finalised", true, receive.Lines[0].WE_PalletID.IsEmpty);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));

			((NotificationBuffer)receive.NotificationManager.Peek).Clear();
			receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			receive.GenerateIdenticalPalletIDs(selectedLines);

			AssertEquals("Should not generate pallet id because the docket is cancelled", true, receive.Lines[0].WE_PalletID.IsEmpty);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		[TestDate(2007, 3, 20)]
		public void TestGenerateIdenticalPalletIDs_DoesNotProcessInvalidLines()
		{
			// Invalid Lines are Lines with
			// 1. No product
			// 2. Error Notifications
			// 3. Finalised Status

			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(50, 40, 30, 25, 21, false);
			Factory.Save();

			var receive = data.Receive11;

			receive.Lines[0].WE_OP = ZGuid.Empty;   // line 0 - invalid because has no product
			receive.Lines[1].WE_OP = ZGuid.Invalid; // line 1 - invalid because has errors
			receive.Lines[2].WE_CurrentInventoryStatus = CodeLists.InventoryStatus.Codes.Available; // line 2 - invalid because its finalised

			var selectedLines = receive.Lines.Cast<WhsReceiveLine>();

			var ids = new List<GeneratedID> { new GeneratedID("0001", 1) };

			var mockGenerator = new Mock<IPalletIDGenerator>();
			mockGenerator.Setup(g => g.GenerateIDs(It.IsAny<WhsReceive>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>())).Returns(ids);

			using (ObjectFactory.Substitute(mockGenerator.Object))
			{
				receive.GenerateIdenticalPalletIDs(selectedLines);
				mockGenerator.Verify(g => g.GenerateIDs(receive, 1, true, 1), Times.Once);
			}

			AssertEquals("Line 0 should not have no palletid", true, receive.Lines[0].WE_PalletID.IsEmpty);
			AssertEquals("Line 1 should not have no palletid", true, receive.Lines[1].WE_PalletID.IsEmpty);
			AssertEquals("Line 2 should not have no palletid", true, receive.Lines[2].WE_PalletID.IsEmpty);
			AssertEquals("Line 3 should have palletid", "0001", receive.Lines[3].WE_PalletID);
			AssertEquals("Line 4 should have palletid", "0001", receive.Lines[4].WE_PalletID);

			for (var i = 0; i < 3; i++)
			{
				AssertEquals(1, receive.Lines[i].RowWarnings.Count());
				AssertEquals("The system has not generated a Pallet ID for this line because it does not have a product, " +
					"or it has errors or it is finalized", receive.Lines[i].RowWarnings.First().Message);
			}
			AssertNoWarnings(receive.Lines[3].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[4].WE_PalletIDInfo);
		}

		public void TestGenerateIdenticalPalletIDs_WarnsIfNoMorePalletIDsAvailable()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 50, 20, 30, 40 }, false);
			Factory.Save();

			var receive = data.Receive11;

			var selectedLines = receive.Lines.Cast<WhsReceiveLine>();

			var ids = new List<GeneratedID>();

			var mockGenerator = new Mock<IPalletIDGenerator>();
			mockGenerator.Setup(g => g.GenerateIDs(It.IsAny<WhsReceive>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>())).Returns(ids);

			using (ObjectFactory.Substitute(mockGenerator.Object))
			{
				receive.GenerateIdenticalPalletIDs(selectedLines);
				mockGenerator.Verify(g => g.GenerateIDs(receive, 1, true, 1), Times.Once);
			}

			for (var i = 0; i < 4; i++)
			{
				AssertEquals(true, receive.Lines[i].WE_PalletID.IsEmpty);
				AssertEquals(1, receive.Lines[i].RowWarnings.Count());
				AssertEquals("The system has not generated a Pallet ID for this line because all Pallet ID's have been allocated. " +
					"You must split this job to access more Pallet ID's.", receive.Lines[i].RowWarnings.First().Message);
			}
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestGenerateIdenticalPalletIDChecksArgumentWithWhsDocketLine()
		{
			Receive.GenerateIdenticalPalletIDs(null);
		}

		[UseSnapshotProtection]
		public void TestGenerateIdenticalPalletIDsCaching_EndToEnd()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 50, 20, 40 }, finalise: false);
			data.Receive11.Client.MiscServ.OM_WhsGenerateSSCCOnInbound = true;

			var warehouseAddressHeader = Factory.Load<OrgHeader>(data.Receive11.Warehouse.WarehouseAddress.OA_OH);
			var cusCode = warehouseAddressHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GS1, "1111111");
			cusCode.OK_OA_PremisesAddress = data.Receive11.Warehouse.WarehouseAddress.PK;

			Factory.Save();

			var receive = data.Receive11;

			var selectedLines = receive.Lines.Cast<WhsReceiveLine>().Where(l => l.WE_TransactionQuantity > 20m);

			var notificationBuffer = new TestNotificationBuffer(true);
			receive.NotificationManager.Push(notificationBuffer);

			receive.GenerateIdenticalPalletIDs(selectedLines);

			AssertEquals("011111110000000014", receive.Lines[0].WE_PalletID);
			AssertEquals(true, receive.Lines[1].WE_PalletID.IsEmpty);
			AssertEquals("011111110000000014", receive.Lines[2].WE_PalletID);

			AssertNoWarnings(receive.Lines[0].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[1].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[2].WE_PalletIDInfo);

			AssertNotNull(notificationBuffer.LastQueryUserEventArgs);
			Assert(notificationBuffer.LastQueryUserEventArgs is QueryUserYesNoEventArgs);
			AssertEquals(
				"Client 1 does not have a GS1 company prefix, would you like to use the warehouse GS1 company prefix?",
				((QueryUserYesNoEventArgs)notificationBuffer.LastQueryUserEventArgs).Message);
			AssertEquals(
				"Generate SSCC Number",
				((QueryUserYesNoEventArgs)notificationBuffer.LastQueryUserEventArgs).Caption);

			notificationBuffer.LastQueryUserEventArgs = null;

			selectedLines = receive.Lines.Cast<WhsReceiveLine>().Where(l => l.WE_TransactionQuantity == 20m);

			receive.GenerateIdenticalPalletIDs(selectedLines);

			AssertEquals("011111110000000014", receive.Lines[0].WE_PalletID);
			AssertEquals("011111110000000021", receive.Lines[1].WE_PalletID);
			AssertEquals("011111110000000014", receive.Lines[2].WE_PalletID);

			AssertNoWarnings(receive.Lines[0].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[1].WE_PalletIDInfo);
			AssertNoWarnings(receive.Lines[2].WE_PalletIDInfo);

			AssertNull(notificationBuffer.LastQueryUserEventArgs);
		}

		public void TestGenerateIdenticalPalletIDs_GivesPromptWithDefaultableQueryEventArgs()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 50, 20, 40 }, false);
			Factory.Save();

			var receive = data.Receive11;
			receive.Lines[1].WE_PalletID = "0002";
			var selectedLines = receive.Lines.Cast<WhsReceiveLine>();

			AssertEquals("Precondition: line 0 has no palletID", ZString.Empty, receive.Lines[0].WE_PalletID);
			AssertEquals("Precondition: line 1 has correct palletID", "0002", receive.Lines[1].WE_PalletID);
			AssertEquals("Precondition: line 2 has no palletID", ZString.Empty, receive.Lines[2].WE_PalletID);

			receive.NotificationManager.Push(Notify);
			Notify.PreQueryUser += (sender, e) =>
			{
				if (e is DefaultableQueryUserEventArgs args)
				{
					AssertEquals("Precondition: default response correct", true, args.Response);
				}
			};
			receive.GenerateIdenticalPalletIDs(selectedLines);

			var lastQueryEventArgs = (DefaultableQueryUserEventArgs)Notify.LastQueryUserEventArgs;
			var expectedMessage = $"The system will use the first Pallet ID you have selected: {receive.Lines[1].WE_PalletID}. Do you want to proceed?";
			AssertEquals("Postcondition: correct notification message", expectedMessage, lastQueryEventArgs.Message);
			AssertEquals("Postcondition: correct response", true, lastQueryEventArgs.Response);
			AssertEquals("User query Buttons should be correct.", ZMessageBoxButtons.YesNo, lastQueryEventArgs.Context.Buttons);
			AssertContainsExactElementsInAnyOrder("User query Results Not To Save should be correct.", new[] { ZDialogResult.No }, lastQueryEventArgs.Context.DialogResultsToNotSave);
			AssertEquals("Postcondition: line 0 has correct palletID", "0002", receive.Lines[0].WE_PalletID);
			AssertEquals("Postcondition: line 1 has correct palletID", "0002", receive.Lines[1].WE_PalletID);
			AssertEquals("Postcondition: line 2 has correct palletID", "0002", receive.Lines[2].WE_PalletID);
		}

		#endregion

		#region Clear Pallet ID

		public void TestClearPalletIDWithWhsDocketLine()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(50, 40, 12, 0, 0, false);
			Factory.Save();
			var receive = data.Receive11;

			receive.Lines[0].WE_PalletID = "testpalletid1";
			receive.Lines[1].WE_PalletID = "testpalletid2";
			receive.Lines[2].WE_PalletID = "testpalletid3";

			var selectedLines = receive.Lines.Cast<WhsReceiveLine>().Where(l => l.WE_TransactionQuantity > 20m);

			receive.ClearPalletIDs(selectedLines);
			AssertEquals("Should clear the pallet id", ZString.Empty, receive.Lines[0].WE_PalletID);
			AssertEquals("Should clear the pallet id", ZString.Empty, receive.Lines[1].WE_PalletID);
			AssertEquals("Should not clear the pallet ids because the line was not selected", "testpalletid3", receive.Lines[2].WE_PalletID);
		}

		public void TestClearPalletIDDoesNotClearIfFinalisedOrCancelledWithWhsDocketLine()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(50, 0, 0, 0, 0);
			Factory.Save();

			var receive = data.Receive11;
			receive.Lines[0].WE_PalletID = "testpalletid1";

			var selectedLines = receive.Lines.Cast<WhsReceiveLine>().Where(l => l.WE_TransactionQuantity > 20m);

			receive.ClearPalletIDs(selectedLines);
			AssertEquals("Should not clear the pallet ids because the docket is finalised", "testpalletid1", receive.Lines[0].WE_PalletID);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));

			((NotificationBuffer)receive.NotificationManager.Peek).Clear();
			receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			receive.ClearPalletIDs(selectedLines);
			AssertEquals("Should not clear the pallet ids because the docket is cancelled", "testpalletid1", receive.Lines[0].WE_PalletID);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		public void TestClearPalletIDs_PutawayTransfers()
		{
			AssertPerformActionOnSelectedInventory_PutawayTransfers((receive, inventory) => receive.ClearPalletIDs(inventory));
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestClearPalletIDChecksArgumentWithWhsDocketLine()
		{
			IEnumerable<WhsReceiveLine> lines = null;
			Receive.ClearPalletIDs(lines);
		}

		#endregion

		#endregion

		#region Palletize Lines And Generate Pallet IDs

		public void TestPalletizeLinesAndGeneratePalletIDs()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			Helper.CreateProductUnit(data.Part1, "PLT", 100);

			var receive = data.Receive11;
			receive.WD_DocketID = "W00000001";
			var selected = new List<WhsReceiveLine>
			{
				receive.Lines[0]
			};
			receive.PalletizeLinesAndGeneratePalletIDs(selected);
			AssertEquals("Should not split new line", 1, receive.Lines.Count);
			AssertEquals("unit of the line should not change", 100m, receive.Lines[0].WE_TransactionQuantity);

			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0001", receive.Lines[0].WE_PalletID);
		}

		public void TestPalletizeLinesAndGeneratePalletIDs_LessThanPalletQty()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			Helper.CreateProductUnit(data.Part1, "PLT", 101);

			var receive = data.Receive11;
			receive.WD_DocketID = "W00000001";
			var selected = new List<WhsReceiveLine>
			{
				receive.Lines[0]
			};
			receive.PalletizeLinesAndGeneratePalletIDs(selected);
			AssertEquals("Should not split new line", 1, receive.Lines.Count);
			AssertEquals("unit of the line should not change", 100m, receive.Lines[0].WE_TransactionQuantity);

			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0001", receive.Lines[0].WE_PalletID);
		}

		public void TestPalletizeLinesAndGeneratePalletIDs_MoreThanPalletQty()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			Helper.CreateProductUnit(data.Part1, "PLT", 45);

			var receive = data.Receive11;
			receive.WD_DocketID = "W00000001";
			var selected = new List<WhsReceiveLine>
			{
				receive.Lines[0]
			};
			receive.PalletizeLinesAndGeneratePalletIDs(selected);
			AssertEquals("Should of generated 3 new lines", 3, receive.Lines.Count);
			AssertEquals("100 units should now be 45", 45m, receive.Lines[0].WE_TransactionQuantity);
			AssertEquals("Should have new line of 45 units", 45m, receive.Lines[1].WE_TransactionQuantity);
			AssertEquals("Should have new line of 10 units", 10m, receive.Lines[2].WE_TransactionQuantity);

			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0001", receive.Lines[0].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0002", receive.Lines[1].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0003", receive.Lines[2].WE_PalletID);
		}

		public void TestPalletizeLinesAndGeneratePalletIDs_MultipleLines()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 10, 10, 10, 10, 10 }, false);
			Helper.CreateProductUnit(data.Part1, "PLT", 10);

			var receive = data.Receive11;
			receive.WD_DocketID = "W00000001";
			var selected = new List<WhsReceiveLine>
			{
				receive.Lines[0],
				receive.Lines[1],
				receive.Lines[2],
				receive.Lines[3],
				receive.Lines[4]
			};
			receive.PalletizeLinesAndGeneratePalletIDs(selected);
			AssertEquals("Should not split new line", 5, receive.Lines.Count);
			AssertEquals("unit of the receive line should not change", 10m, receive.Lines[0].WE_TransactionQuantity);
			AssertEquals("unit of the receive line should not change", 10m, receive.Lines[1].WE_TransactionQuantity);
			AssertEquals("unit of the receive line should not change", 10m, receive.Lines[2].WE_TransactionQuantity);
			AssertEquals("unit of the receive line should not change", 10m, receive.Lines[3].WE_TransactionQuantity);
			AssertEquals("unit of the receive line should not change", 10m, receive.Lines[4].WE_TransactionQuantity);

			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0001", receive.Lines[0].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0002", receive.Lines[1].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0003", receive.Lines[2].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0004", receive.Lines[3].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0005", receive.Lines[4].WE_PalletID);

			AssertArrayEqualsByElements("Palletized Line from select should have correct Pallet ID",
				new[] { "W00000001-0001", "W00000001-0002", "W00000001-0003", "W00000001-0004", "W00000001-0005" },
				new String[] { receive.Lines[0].WE_PalletID, receive.Lines[1].WE_PalletID, receive.Lines[2].WE_PalletID, receive.Lines[3].WE_PalletID, receive.Lines[4].WE_PalletID });
		}

		public void TestPalletizeLinesAndGeneratePalletIDs_MultipleLines_LessThanPalletQty()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 12, 11, 10, 9, 7 }, false);
			Helper.CreateProductUnit(data.Part1, "PLT", 15);

			var receive = data.Receive11;
			receive.WD_DocketID = "W00000001";
			var selected = new List<WhsReceiveLine>
			{
				receive.Lines[0],
				receive.Lines[1],
				receive.Lines[2],
				receive.Lines[3],
				receive.Lines[4]
			};
			receive.PalletizeLinesAndGeneratePalletIDs(selected);
			AssertEquals("Should of generated 2 new lines", 7, receive.Lines.Count);
			AssertEquals("unit of the receive line should not change", 12m, receive.Lines[0].WE_TransactionQuantity);
			AssertEquals("unit of the receive line should not change", 11m, receive.Lines[1].WE_TransactionQuantity);
			AssertEquals("unit of the receive line should not change", 10m, receive.Lines[2].WE_TransactionQuantity);
			AssertEquals("10 units should now be 3", 5m, receive.Lines[3].WE_TransactionQuantity);
			AssertEquals("8 units should now be 5", 3m, receive.Lines[4].WE_TransactionQuantity);
			AssertEquals("Should have new line of 5 units", 4m, receive.Lines[5].WE_TransactionQuantity);
			AssertEquals("Should have new line of 5 units", 4m, receive.Lines[6].WE_TransactionQuantity);

			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0001", receive.Lines[0].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0002", receive.Lines[1].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0003", receive.Lines[2].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0003", receive.Lines[3].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0001", receive.Lines[4].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0002", receive.Lines[5].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0004", receive.Lines[6].WE_PalletID);
		}

		public void TestPalletizeLinesAndGeneratePalletIDs_MultipleLines_MoreThanPalletQty()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 15, 14, 13, 12, 11 }, false);
			Helper.CreateProductUnit(data.Part1, "PLT", 10);

			var receive = data.Receive11;
			receive.WD_DocketID = "W00000001";
			var selected = new List<WhsReceiveLine>
			{
				receive.Lines[0],
				receive.Lines[1],
				receive.Lines[2],
				receive.Lines[3],
				receive.Lines[4]
			};
			receive.PalletizeLinesAndGeneratePalletIDs(selected);
			AssertEquals("Should of generated 10 new lines", 11, receive.Lines.Count);
			AssertEquals("15 units should now be 10", 10m, receive.Lines[0].WE_TransactionQuantity);
			AssertEquals("14 units should now be 10", 10m, receive.Lines[1].WE_TransactionQuantity);
			AssertEquals("13 units should now be 10", 10m, receive.Lines[2].WE_TransactionQuantity);
			AssertEquals("12 units should now be 10", 10m, receive.Lines[3].WE_TransactionQuantity);
			AssertEquals("11 units should now be 10", 10m, receive.Lines[4].WE_TransactionQuantity);
			AssertEquals("Should have new line of 5 units", 5m, receive.Lines[5].WE_TransactionQuantity);
			AssertEquals("Should have new line of 4 units", 4m, receive.Lines[6].WE_TransactionQuantity);
			AssertEquals("Should have new line of 2 units", 2m, receive.Lines[7].WE_TransactionQuantity);
			AssertEquals("Should have new line of 2 units", 2m, receive.Lines[8].WE_TransactionQuantity);
			AssertEquals("Should have new line of 1 units", 1m, receive.Lines[9].WE_TransactionQuantity);
			AssertEquals("Should have new line of 1 units", 1m, receive.Lines[10].WE_TransactionQuantity);

			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0001", receive.Lines[0].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0002", receive.Lines[1].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0003", receive.Lines[2].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0004", receive.Lines[3].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0005", receive.Lines[4].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0006", receive.Lines[5].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0007", receive.Lines[6].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0006", receive.Lines[7].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0006", receive.Lines[8].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0006", receive.Lines[9].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0007", receive.Lines[10].WE_PalletID);
		}

		public void TestPalletizeLinesAndGeneratePalletIDs_MultipleLines_MixTransactionQty()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 12, 11, 10, 9, 8 }, false);
			Helper.CreateProductUnit(data.Part1, "PLT", 10);

			var receive = data.Receive11;
			receive.WD_DocketID = "W00000001";
			var selected = new List<WhsReceiveLine>
			{
				receive.Lines[0],
				receive.Lines[1],
				receive.Lines[2],
				receive.Lines[3],
				receive.Lines[4]
			};
			receive.PalletizeLinesAndGeneratePalletIDs(selected);
			AssertEquals("Should of generated 7 new lines", 7, receive.Lines.Count);
			AssertEquals("12 units should now be 10", 10m, receive.Lines[0].WE_TransactionQuantity);
			AssertEquals("11 units should now be 10", 10m, receive.Lines[1].WE_TransactionQuantity);
			AssertEquals("unit of the receive line should not change", 10m, receive.Lines[2].WE_TransactionQuantity);
			AssertEquals("unit of the receive line should not change", 9m, receive.Lines[3].WE_TransactionQuantity);
			AssertEquals("unit of the receive line should not change", 8m, receive.Lines[4].WE_TransactionQuantity);
			AssertEquals("Should have new line of 2 units", 2m, receive.Lines[5].WE_TransactionQuantity);
			AssertEquals("Should have new line of 1 units", 1m, receive.Lines[6].WE_TransactionQuantity);

			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0001", receive.Lines[0].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0002", receive.Lines[1].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0003", receive.Lines[2].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0004", receive.Lines[3].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0005", receive.Lines[4].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0005", receive.Lines[5].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0004", receive.Lines[6].WE_PalletID);
		}

		public void TestPalletizeLinesAndGeneratePalletIDs_MultipleProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15);

			Helper.CreateProductUnit(data.Part1, "PLT", 10);
			Helper.CreateProductUnit(data.Part2, "PLT", 15);

			receive.WD_DocketID = "W00000001";
			var selected = new List<WhsReceiveLine>
			{
				receive.Lines[0],
				receive.Lines[1],
				receive.Lines[2],
				receive.Lines[3],
				receive.Lines[4],
				receive.Lines[5]
			};
			receive.PalletizeLinesAndGeneratePalletIDs(selected);
			AssertEquals("Should not split new line", 6, receive.Lines.Count);
			AssertEquals("unit of the receive line should not change", 10m, receive.Lines[0].WE_TransactionQuantity);
			AssertEquals("unit of the receive line should not change", 10m, receive.Lines[1].WE_TransactionQuantity);
			AssertEquals("unit of the receive line should not change", 10m, receive.Lines[2].WE_TransactionQuantity);
			AssertEquals("unit of the receive line should not change", 15m, receive.Lines[3].WE_TransactionQuantity);
			AssertEquals("unit of the receive line should not change", 15m, receive.Lines[4].WE_TransactionQuantity);
			AssertEquals("unit of the receive line should not change", 15m, receive.Lines[5].WE_TransactionQuantity);

			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0001", receive.Lines[0].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0002", receive.Lines[1].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0003", receive.Lines[2].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0004", receive.Lines[3].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0005", receive.Lines[4].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0006", receive.Lines[5].WE_PalletID);
		}

		public void TestPalletizeLinesAndGeneratePalletIDs_MultipleProduct_LessThanPalletQty()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 14);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 13);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 12);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 24);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 23);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 22);

			Helper.CreateProductUnit(data.Part1, "PLT", 15);
			Helper.CreateProductUnit(data.Part2, "PLT", 30);

			receive.WD_DocketID = "W00000001";
			var selected = new List<WhsReceiveLine>
			{
				receive.Lines[0],
				receive.Lines[1],
				receive.Lines[2],
				receive.Lines[3],
				receive.Lines[4],
				receive.Lines[5]
			};
			receive.PalletizeLinesAndGeneratePalletIDs(selected);
			AssertEquals("Should of generated 10 new lines", 10, receive.Lines.Count);
			AssertEquals("unit of the receive line should not change", 14m, receive.Lines[0].WE_TransactionQuantity);
			AssertEquals("unit of the receive line should not change", 13m, receive.Lines[1].WE_TransactionQuantity);
			AssertEquals("12 units should now be 1", 1m, receive.Lines[2].WE_TransactionQuantity);
			AssertEquals("unit of the receive line should not change", 24m, receive.Lines[3].WE_TransactionQuantity);
			AssertEquals("unit of the receive line should not change", 23m, receive.Lines[4].WE_TransactionQuantity);
			AssertEquals("22 units should now be 6", 6m, receive.Lines[5].WE_TransactionQuantity);
			AssertEquals("Should have new line of 2 units", 2m, receive.Lines[6].WE_TransactionQuantity);
			AssertEquals("Should have new line of 9 units", 9m, receive.Lines[7].WE_TransactionQuantity);
			AssertEquals("Should have new line of 7 units", 7m, receive.Lines[8].WE_TransactionQuantity);
			AssertEquals("Should have new line of 9 units", 9m, receive.Lines[9].WE_TransactionQuantity);

			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0001", receive.Lines[0].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0002", receive.Lines[1].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0001", receive.Lines[2].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0004", receive.Lines[3].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0005", receive.Lines[4].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0004", receive.Lines[5].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0002", receive.Lines[6].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0003", receive.Lines[7].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0005", receive.Lines[8].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0006", receive.Lines[9].WE_PalletID);
		}

		public void TestPalletizeLinesAndGeneratePalletIDs_MultipleProduct_MoreThanPalletQty()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 40);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 24);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 55);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 35);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 40);

			Helper.CreateProductUnit(data.Part1, "PLT", 15);
			Helper.CreateProductUnit(data.Part2, "PLT", 25);

			receive.WD_DocketID = "W00000001";
			var selected = new List<WhsReceiveLine>
			{
				receive.Lines[0],
				receive.Lines[1],
				receive.Lines[2],
				receive.Lines[3],
				receive.Lines[4],
				receive.Lines[5]
			};
			receive.PalletizeLinesAndGeneratePalletIDs(selected);
			AssertEquals("Should of generated 14 new lines", 15, receive.Lines.Count);
			AssertEquals("40 units should now be 15", 15m, receive.Lines[0].WE_TransactionQuantity);
			AssertEquals("25 units should now be 15", 15m, receive.Lines[1].WE_TransactionQuantity);
			AssertEquals("20 units should now be 15", 15m, receive.Lines[2].WE_TransactionQuantity);
			AssertEquals("55 units should now be 25", 25m, receive.Lines[3].WE_TransactionQuantity);
			AssertEquals("35 units should now be 25", 25m, receive.Lines[4].WE_TransactionQuantity);
			AssertEquals("40 units should now be 25", 25m, receive.Lines[5].WE_TransactionQuantity);
			AssertEquals("Should have new line of 15 units", 15m, receive.Lines[6].WE_TransactionQuantity);
			AssertEquals("Should have new line of 10 units", 10m, receive.Lines[7].WE_TransactionQuantity);
			AssertEquals("Should have new line of 9 units", 9m, receive.Lines[8].WE_TransactionQuantity);
			AssertEquals("Should have new line of 5 units", 5m, receive.Lines[9].WE_TransactionQuantity);
			AssertEquals("Should have new line of 25 units", 25m, receive.Lines[10].WE_TransactionQuantity);
			AssertEquals("Should have new line of 5 units", 5m, receive.Lines[11].WE_TransactionQuantity);
			AssertEquals("Should have new line of 5 units", 5m, receive.Lines[12].WE_TransactionQuantity);
			AssertEquals("Should have new line of 15 units", 15m, receive.Lines[13].WE_TransactionQuantity);
			AssertEquals("Should have new line of 5 units", 5m, receive.Lines[14].WE_TransactionQuantity);

			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0001", receive.Lines[0].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0002", receive.Lines[1].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0003", receive.Lines[2].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0007", receive.Lines[3].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0008", receive.Lines[4].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0009", receive.Lines[5].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0004", receive.Lines[6].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0005", receive.Lines[7].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0006", receive.Lines[8].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0005", receive.Lines[9].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0010", receive.Lines[10].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0011", receive.Lines[11].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0011", receive.Lines[12].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0011", receive.Lines[13].WE_PalletID);
			AssertEquals("Palletized Line from select should have correct Pallet ID", "W00000001-0012", receive.Lines[14].WE_PalletID);
		}

		public void TestPalletizeLinesAndGeneratePalletIDs_NoPalletToStockKeepingUnitConversion()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketID = "W00000001";
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 20);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 30);

			AssertEquals("Precondition", 0m, data.Part1.OP_StockKeepingUnitPerPallet);

			AssertNoExceptionThrown(() => receive.PalletizeLinesAndGeneratePalletIDs(receive.Lines.ToArray<WhsReceiveLine>()));
			AssertEquals("Should not split the lines", 3, receive.Lines.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "W00000001-0001", "W00000001-0002", "W00000001-0003" }, receive.Lines.Select(line => line.WE_PalletID));
		}

		public void TestPalletizeLinesAndGeneratePalletIDs_NoProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketID = "W00000001";
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10);
			receiveLine.WE_OP = ZGuid.Empty;

			AssertEquals("Precondition", null, receiveLine.Product);

			AssertNoExceptionThrown(() => receive.PalletizeLinesAndGeneratePalletIDs(receive.Lines.ToArray<WhsReceiveLine>()));
			AssertEquals("Should not split the lines", 1, receive.Lines.Count);
			AssertEquals("Should not generate pallet id.", string.Empty, receiveLine.WE_PalletID);
			AssertEquals("The system has not generated a Pallet ID for this line because it does not have a product, or it has errors or it is finalized", receiveLine.RowWarnings.First().Message);
		}

		#endregion

		#region Delete All Test in this region after implementing WhsDocketLine instead of WhsInventoryView

		#region Clear Pallet ID

		public void TestClearPalletID()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(50, 10, 12, 0, 0, false);
			var receive = data.Receive11;

			receive.Inventory[0].WI_PalletID = "testpalletid1";
			receive.Inventory[1].WI_PalletID = "testpalletid2";
			receive.Inventory[2].WI_PalletID = "testpalletid3";

			var selected = new List<WhsInventoryView>();
			selected.Add(receive.Inventory[0]);
			selected.Add(receive.Inventory[1]);

			receive.ClearPalletIDs(selected);
			AssertEquals("Should clear the pallet id", ZString.Empty, receive.Inventory[0].WI_PalletID);
			AssertEquals("Should clear the pallet id", ZString.Empty, receive.Inventory[1].WI_PalletID);
			AssertEquals("Should not clear the pallet ids because the line was not selected", "testpalletid3", receive.Inventory[2].WI_PalletID);
		}

		public void TestClearPalletIDDoesNotClearIfFinalisedOrCancelled()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(50, 0, 0, 0, 0);

			var receive = data.Receive11;
			receive.Inventory[0].WI_PalletID = "testpalletid1";
			var selected = new List<WhsInventoryView>();
			selected.Add(receive.Inventory[0]);

			receive.ClearPalletIDs(selected);
			AssertEquals("Should not clear the pallet ids because the docket is finalised", "testpalletid1", receive.Inventory[0].WI_PalletID);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));

			((NotificationBuffer)receive.NotificationManager.Peek).Clear();
			receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			receive.ClearPalletIDs(selected);
			AssertEquals("Should not clear the pallet ids because the docket is cancelled", "testpalletid1", receive.Inventory[0].WI_PalletID);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestClearPalletIDChecksArgument()
		{
			List<WhsInventoryView> inventories = null;
			Receive.ClearPalletIDs(inventories);
		}

		#endregion

		#endregion Delete upto this line

		#region Change Hold Code Using WhsDocketLine

		#region Change Hold Code For Product

		public void TestChangeHoldCodeForProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-2"), "PLT2");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A-3"), "PLT3");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A-4"), "PLT4");

			Factory.Save();

			AssertEquals("Precondition: ReceiveLine1's Hold Code is empty.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
			AssertEquals("Precondition: ReceiveLine2's Hold Code is empty.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
			AssertEquals("Precondition: ReceiveLine3's Hold Code is empty.", true, receiveLine3.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
			AssertEquals("Precondition: ReceiveLine4's Hold Code is empty.", true, receiveLine4.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);

			var selectedLines = receive.Lines.Cast<WhsReceiveLine>();
			receive.ChangeHoldCodeForProduct(selectedLines);

			var notify = (NotificationBuffer)receive.NotificationManager.Peek;
			AssertEquals(true, notify.ContainsNotificationType(ReceiveErrorTypes.NoLinesWithHoldCode));
			notify.Clear();

			receiveLine1.WE_WHC_NKOriginalInventoryHeldCode = "HEL";
			receiveLine3.WE_WHC_NKOriginalInventoryHeldCode = "DAM";
			receive.ChangeHoldCodeForProduct(selectedLines);

			AssertEquals("Should not have error", false, notify.HasErrors);
			AssertEquals("ReceiveLine1's Hold Code is HEL.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			AssertEquals("ReceiveLine2's Hold Code is HEL.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			AssertEquals("ReceiveLine3's Hold Code is DAM.", true, receiveLine3.WE_WHC_NKOriginalInventoryHeldCode == "DAM");
			AssertEquals("ReceiveLine4's Hold Code is DAM.", true, receiveLine4.WE_WHC_NKOriginalInventoryHeldCode == "DAM");
		}

		public void TestChangeHoldCodeForProduct_UsesFirstHoldCodeBeFound()
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-2"), "PLT2", "", "HEL");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A-3"), "PLT3");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A-4"), "PLT4", "", "DAM");

			Factory.Save();

			AssertEquals("Precondition: ReceiveLine1's Hold Code is empty.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
			AssertEquals("Precondition: ReceiveLine2's Hold Code is HEL.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			AssertEquals("Precondition: ReceiveLine3's Hold Code is empty.", true, receiveLine3.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
			AssertEquals("Precondition: ReceiveLine4's Hold Code is DAM.", true, receiveLine4.WE_WHC_NKOriginalInventoryHeldCode == "DAM");

			var selectedLines = receive.Lines.Cast<WhsReceiveLine>();
			receive.ChangeHoldCodeForProduct(selectedLines);

			AssertEquals("Should not have error", false, ((NotificationBuffer)receive.NotificationManager.Peek).HasErrors);
			AssertEquals("ReceiveLine1's Hold Code is HEL.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			AssertEquals("ReceiveLine2's Hold Code is HEL.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			AssertEquals("ReceiveLine3's Hold Code is DAM.", true, receiveLine3.WE_WHC_NKOriginalInventoryHeldCode == "DAM");
			AssertEquals("ReceiveLine4's Hold Code is DAM.", true, receiveLine4.WE_WHC_NKOriginalInventoryHeldCode == "DAM");
		}

		public void TestChangeHoldCodeForProduct_OverwritesExistingHoldCodes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1", "", "HEL");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-2"), "PLT2", "", "DAM");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A-3"), "PLT3", "", "DAM");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A-4"), "PLT4", "", "HEL");

			Factory.Save();

			AssertEquals("Precondition: ReceiveLine1's Hold Code is HEL.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			AssertEquals("Precondition: ReceiveLine2's Hold Code is DAM.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode == "DAM");
			AssertEquals("Precondition: ReceiveLine3's Hold Code is DAM.", true, receiveLine3.WE_WHC_NKOriginalInventoryHeldCode == "DAM");
			AssertEquals("Precondition: ReceiveLine4's Hold Code is HEL.", true, receiveLine4.WE_WHC_NKOriginalInventoryHeldCode == "HEL");

			var selectedLines = receive.Lines.Cast<WhsReceiveLine>();
			receive.ChangeHoldCodeForProduct(selectedLines);

			AssertEquals("Should not have error", false, ((NotificationBuffer)receive.NotificationManager.Peek).HasErrors);
			AssertEquals("ReceiveLine1's Hold Code is HEL.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			AssertEquals("ReceiveLine2's Hold Code is HEL.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			AssertEquals("ReceiveLine3's Hold Code is DAM.", true, receiveLine3.WE_WHC_NKOriginalInventoryHeldCode == "DAM");
			AssertEquals("ReceiveLine4's Hold Code is DAM.", true, receiveLine4.WE_WHC_NKOriginalInventoryHeldCode == "DAM");
		}

		public void TestChangeHoldCodeForProduct_DoesNotExecuteIfFinalisedOrCancelled()
		{
			TestChangeHoldCode_DoesNotClearIfFinalisedOrCancelled((receive, selectedLines) => receive.ChangeHoldCodeForProduct(selectedLines));
		}

		public void TestChangeHoldCodeForProduct_DoesNotProcessInvalidLines_WithNoProduct()
		{
			TestChangeHoldCode_DoesNotProcessInvalidLinesCore(
				receive => receive.Lines[0].WE_OP = ZGuid.Empty,
				"has no product",
				(receive, selectedLines) => receive.ChangeHoldCodeForProduct(selectedLines));
		}

		public void TestChangeHoldCodeForProduct_DoesNotProcessInvalidLines_WithErrorNotifications()
		{
			TestChangeHoldCode_DoesNotProcessInvalidLinesCore(
				receive => receive.Lines[0].WE_OP = ZGuid.Invalid,
				"has errors",
				(receive, selectedLines) => receive.ChangeHoldCodeForProduct(selectedLines));
		}

		public void TestChangeHoldCodeForProduct_DoesNotProcessInvalidLines_WithReadOnly()
		{
			TestChangeHoldCode_DoesNotProcessInvalidLinesCore(
				receive => receive.Lines[0].WE_OriginalInventoryStatus = InventoryStatus.Codes.PuttingAway,
				"Hold Code is readonly",
				(receive, selectedLines) => receive.ChangeHoldCodeForProduct(selectedLines));
		}

		public void TestChangeHoldCodeForProduct_ChecksArgument()
		{
			AssertExceptionThrown("Since selected Lines are null, null exception should be thrown.", typeof(ArgumentNullException), () => Receive.ChangeHoldCodeForProduct(null));
		}

		#endregion

		#region Change Hold Code For Receipt

		public void TestChangeHoldCodeForReceipt()
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-2"), "PLT2");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A-3"), "PLT3");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A-4"), "PLT4");

			Factory.Save();

			AssertEquals("Precondition: ReceiveLine1's Hold Code is empty.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
			AssertEquals("Precondition: ReceiveLine2's Hold Code is empty.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
			AssertEquals("Precondition: ReceiveLine3's Hold Code is empty.", true, receiveLine3.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
			AssertEquals("Precondition: ReceiveLine4's Hold Code is empty.", true, receiveLine4.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);

			receive.ChangeHoldCodeForReceipt();

			var notify = (NotificationBuffer)receive.NotificationManager.Peek;
			AssertEquals(true, notify.ContainsNotificationType(ReceiveErrorTypes.NoLinesWithHoldCode));
			notify.Clear();

			receiveLine1.WE_WHC_NKOriginalInventoryHeldCode = "HEL";
			receive.ChangeHoldCodeForReceipt();

			AssertEquals("Should not have error", false, notify.HasErrors);
			AssertEquals("ReceiveLine1's Hold Code is HEL.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			AssertEquals("ReceiveLine2's Hold Code is HEL.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			AssertEquals("ReceiveLine3's Hold Code is HEL.", true, receiveLine3.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			AssertEquals("ReceiveLine4's Hold Code is HEL.", true, receiveLine4.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
		}

		public void TestChangeHoldCodeForReceipt_UsesFirstHoldCodeBeFound()
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-2"), "PLT2", "", "HEL");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A-3"), "PLT3");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A-4"), "PLT4", "", "DAM");

			Factory.Save();

			AssertEquals("Precondition: ReceiveLine1's Hold Code is empty.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
			AssertEquals("Precondition: ReceiveLine2's Hold Code is HEL.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			AssertEquals("Precondition: ReceiveLine3's Hold Code is empty.", true, receiveLine3.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
			AssertEquals("Precondition: ReceiveLine4's Hold Code is DAM.", true, receiveLine4.WE_WHC_NKOriginalInventoryHeldCode == "DAM");

			receive.ChangeHoldCodeForReceipt();

			AssertEquals("Should not have error", false, ((NotificationBuffer)receive.NotificationManager.Peek).HasErrors);
			AssertEquals("ReceiveLine1's Hold Code is HEL.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			AssertEquals("ReceiveLine2's Hold Code is HEL.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			AssertEquals("ReceiveLine3's Hold Code is HEL.", true, receiveLine3.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			AssertEquals("ReceiveLine4's Hold Code is HEL.", true, receiveLine4.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
		}

		public void TestChangeHoldCodeForReceipt_OverwritesExistingHoldCodes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1", "", "HEL");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-2"), "PLT2", "", "DAM");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A-3"), "PLT3", "", "DAM");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A-4"), "PLT4", "", "HEL");

			Factory.Save();

			AssertEquals("Precondition: ReceiveLine1's Hold Code is HEL.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			AssertEquals("Precondition: ReceiveLine2's Hold Code is DAM.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode == "DAM");
			AssertEquals("Precondition: ReceiveLine3's Hold Code is DAM.", true, receiveLine3.WE_WHC_NKOriginalInventoryHeldCode == "DAM");
			AssertEquals("Precondition: ReceiveLine4's Hold Code is HEL.", true, receiveLine4.WE_WHC_NKOriginalInventoryHeldCode == "HEL");

			receive.ChangeHoldCodeForReceipt();

			AssertEquals("Should not have error", false, ((NotificationBuffer)receive.NotificationManager.Peek).HasErrors);
			AssertEquals("ReceiveLine1's Hold Code is HEL.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			AssertEquals("ReceiveLine2's Hold Code is HEL.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			AssertEquals("ReceiveLine3's Hold Code is HEL.", true, receiveLine3.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			AssertEquals("ReceiveLine4's Hold Code is HEL.", true, receiveLine4.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
		}

		public void TestChangeHoldCodeForReceipt_DoesNotExecuteIfFinalisedOrCancelled()
		{
			TestChangeHoldCode_DoesNotClearIfFinalisedOrCancelled((receive, selectedLines) => receive.ChangeHoldCodeForReceipt());
		}

		public void TestChangeHoldCodeForReceipt_DoesNotProcessInvalidLines_WithNoProduct()
		{
			TestChangeHoldCode_DoesNotProcessInvalidLinesCore(
				receive => receive.Lines[0].WE_OP = ZGuid.Empty,
				"has no product",
				(receive, selectedLines) => receive.ChangeHoldCodeForReceipt());
		}

		public void TestChangeHoldCodeForReceipt_DoesNotProcessInvalidLines_WithErrorNotifications()
		{
			TestChangeHoldCode_DoesNotProcessInvalidLinesCore(
				receive => receive.Lines[0].WE_OP = ZGuid.Invalid,
				"has errors",
				(receive, selectedLines) => receive.ChangeHoldCodeForReceipt());
		}

		public void TestChangeHoldCodeForReceipt_DoesNotProcessInvalidLines_WithReadOnly()
		{
			TestChangeHoldCode_DoesNotProcessInvalidLinesCore(
				receive => receive.Lines[0].WE_OriginalInventoryStatus = InventoryStatus.Codes.PuttingAway,
				"Hold Code is readonly",
				(receive, selectedLines) => receive.ChangeHoldCodeForReceipt());
		}

		#endregion

		#region Clear Hold Codes

		public void TestClearHoldCodes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1", "", "HEL");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-2"), "PLT2", "", "DAM");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A-3"), "PLT3", "", "DAM");

			Factory.Save();

			AssertEquals("Precondition: ReceiveLine1's Hold Code is HEL.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			AssertEquals("Precondition: ReceiveLine2's Hold Code is DAM.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode == "DAM");
			AssertEquals("Precondition: ReceiveLine3's Hold Code is DAM.", true, receiveLine3.WE_WHC_NKOriginalInventoryHeldCode == "DAM");

			var selectedLines = receive.Lines.Cast<WhsReceiveLine>().Where(t => t.WE_OP == data.Part1.PK);
			receive.ClearHoldCodes(selectedLines);

			AssertEquals("Should not have error", false, ((NotificationBuffer)receive.NotificationManager.Peek).HasErrors);
			AssertEquals("Should clear the Hold Code", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
			AssertEquals("Should clear the Hold Code", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
			AssertEquals("Should not clear the Hold Codes because the line was not selected", true, receiveLine3.WE_WHC_NKOriginalInventoryHeldCode == "DAM");
		}

		public void TestClearHoldCodes_DoesNotClearIfFinalisedOrCancelled()
		{
			TestChangeHoldCode_DoesNotClearIfFinalisedOrCancelled((receive, selectedLines) => receive.ClearHoldCodes(selectedLines));
		}

		public void TestClearHoldCodes_DoesNotProcessInvalidLines_WithNoProduct()
		{
			TestChangeHoldCode_DoesNotProcessInvalidLinesCore(
				receive => receive.Lines[0].WE_OP = ZGuid.Empty,
				"has no product",
				(receive, selectedLines) => receive.ClearHoldCodes(selectedLines));
		}

		public void TestClearHoldCodes_DoesNotProcessInvalidLines_WithErrorNotifications()
		{
			TestChangeHoldCode_DoesNotProcessInvalidLinesCore(
				receive => receive.Lines[0].WE_OP = ZGuid.Invalid,
				"has errors",
				(receive, selectedLines) => receive.ClearHoldCodes(selectedLines));
		}

		public void TestClearHoldCodes_DoesNotProcessInvalidLines_WithReadOnly()
		{
			TestChangeHoldCode_DoesNotProcessInvalidLinesCore(
				receive => receive.Lines[0].WE_OriginalInventoryStatus = InventoryStatus.Codes.PuttingAway,
				"Hold Code is readonly",
				(receive, selectedLines) => receive.ClearHoldCodes(selectedLines));
		}

		public void TestClearHoldCodes_ChecksArgument()
		{
			AssertExceptionThrown("Since selected Lines are null, null exception should be thrown.", typeof(ArgumentNullException), () => Receive.ClearHoldCodes(null));
		}

		#endregion

		void TestChangeHoldCode_DoesNotClearIfFinalisedOrCancelled(Action<WhsReceive, IEnumerable<WhsReceiveLine>> actionToChangeHoldCode)
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-2"), "PLT2", "", "HEL");
			receive.FinaliseDocket();

			Factory.Save();

			AssertEquals("Precondition: ReceiveLine1's Hold Code is empty.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
			AssertEquals("Precondition: ReceiveLine2's Hold Code is HEL.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode == "HEL");

			var lines = receive.Lines.Cast<WhsReceiveLine>();
			actionToChangeHoldCode(receive, lines);

			var notify = (NotificationBuffer)receive.NotificationManager.Peek;
			AssertEquals("Should not change Hold Code because the docket is finalised", 1, receive.Lines.Count(l => l.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty));
			AssertEquals(true, notify.ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));

			notify.Clear();
			receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			actionToChangeHoldCode(receive, lines);
			AssertEquals("Should not change Hold Code because the docket is cancelled", 1, receive.Lines.Count(l => l.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty));
			AssertEquals(true, notify.ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		void TestChangeHoldCode_DoesNotProcessInvalidLinesCore(Action<WhsReceive> actionToChangePrecondition, string preconditionMessage, Action<WhsReceive, IEnumerable<WhsReceiveLine>> actionToChangeHoldCode)
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1", "", "HEL");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, data.Whs1.FindLocation("A-2"), "PLT2", "", "DAM");

			Factory.Save();

			actionToChangePrecondition(receive);

			AssertEquals($"Precondition: ReceiveLine1's Hold Code is HEL, but {preconditionMessage}.", true, receiveLine1.WE_WHC_NKOriginalInventoryHeldCode == "HEL");
			AssertEquals($"Precondition: ReceiveLine2's Hold Code is DAM, but {preconditionMessage}.", true, receiveLine2.WE_WHC_NKOriginalInventoryHeldCode == "DAM");

			var selectedLines = receive.Lines.Cast<WhsReceiveLine>();
			actionToChangeHoldCode(receive, selectedLines);

			AssertEquals("Should have error", true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(ReceiveErrorTypes.NoValidLines));
		}

		#endregion

		#region Create Product Files

		#region TestCreateProductFiles

		public void TestCreateProductFiles_IsImportingAndSecurityRightGranted()
		{
			TestCreateProductFiles_CheckPermissionsCore(true, true);
		}

		public void TestCreateProductFiles_IsImportingAndSecurityRightNotGranted()
		{
			TestCreateProductFiles_CheckPermissionsCore(true, false);
		}

		public void TestCreateProductFiles_IsNotImportingAndSecurityRightNotGranted()
		{
			TestCreateProductFiles_CheckPermissionsCore(false, false);
		}

		public void TestCreateProductFiles_IsNotImportingAndSecurityRightGranted()
		{
			TestCreateProductFiles_CheckPermissionsCore(false, true);
		}

		void TestCreateProductFiles_CheckPermissionsCore(bool isImporting, bool isSecurityRightGranted)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var commodityCode = Factory.NewWithValidTestData<RefCommodityCode>();
			commodityCode.RH_Code = "CODE";
			Factory.Save();
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = CreateWhsReceiveInventoryLine(receive, "NEWP1", "New Product 1", "", "UNT");

			receive.IsImportingData = isImporting;
			Env.Security.WhsReceiveCreateProductFiles.IsAllowed = isSecurityRightGranted;

			receive.CreateProductFiles();
			var collection = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "NEWP1"));

			if (isImporting || isSecurityRightGranted)
			{
				AssertEquals("Product's PK should be bound to Inventory.", collection[0].PK, inventory.WI_OP);
			}
			else
			{
				AssertEquals("No product should be created since WhsReceiveCreateProductFiles is not granted and !IsImportingData.", 0, collection.Length);
				AssertEquals("Since Product was not created, Inventory Product PK should be still invalid.", ZGuid.Invalid, inventory.WI_OP);
			}
		}

		public void TestCreateProductFiles()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var commodityCode = Factory.NewWithValidTestData<RefCommodityCode>();
			commodityCode.RH_Code = "CODE";

			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Env.Security.WhsReceiveCreateProductFiles.IsAllowed = true;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m); // check that use of existing product will not damage the process
			var inventory1_1 = CreateWhsReceiveInventoryLine(receive, "NEWP1", "NewDesc1", "", "UNT"); // 2 same products will not mean creating of product duplicate
			var inventory1_2 = CreateWhsReceiveInventoryLine(receive, "NEWP1", "NewDesc1", "", "UNT");
			var inventory2_1 = CreateWhsReceiveInventoryLine(receive, "NEWP2", "NewDesc2_1", "", "UNT"); // 2 same products with different properties will not be proceeded until corrected
			var inventory2_2 = CreateWhsReceiveInventoryLine(receive, "NEWP2", "NewDesc2_2", "", "UNT");
			var inventory3 = CreateWhsReceiveInventoryLine(receive, "NEWP3", "NewDesc3", "CODE", "BAG"); // all properties are passed correctly
			var inventory4 = CreateWhsReceiveInventoryLine(receive, "NEWP4", "NewDesc4", "", ""); // not all mandatory properties set
			var inventory5 = CreateWhsReceiveInventoryLine(receive, "NEWP5", "NewDesc5", "", "UNT"); // If Part was created while inventory has been in process of setup.
			var inventory6 = CreateWhsReceiveInventoryLine(receive, "NEWP6", "NewDesc6", "", "111"); // Units UQ value is incorrect
			var inventory7 = CreateWhsReceiveInventoryLine(receive, "NEWP7", "NewDesc7", "111", "UNT"); // Commodity Code is incorrect

			var part5 = Helper.CreateProduct(data.Org1, "NEWP5"); // created product from other line or just in system while this receive wasn't created yet.
			part5.OP_Desc = "Different desc";
			part5.OP_RH_NKCommodityCode = "CODE";
			part5.OP_StockKeepingUnit = "BAG";

			receive.CreateProductFiles();

			var collection1 = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "NEWP1"));
			AssertEquals("Only 1 product should be created from 2 identical lines", 1, collection1.Length);
			AssertEquals("Since Product was created, its PK should be now bound to Inventory.", collection1[0].PK, inventory1_1.WI_OP);
			AssertEquals("Since Product was created, its PK should be now bound to Inventory.", collection1[0].PK, inventory1_2.WI_OP);

			var collection2 = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "NEWP2"));
			AssertEquals("No product should be created since product properties are different", 0, collection2.Length);
			AssertEquals("Since Product was not created, Inventory Product PK should be still invalid.", ZGuid.Invalid, inventory2_1.WI_OP);
			AssertEquals("Since Product was not created, Inventory Product PK should be still invalid.", ZGuid.Invalid, inventory2_2.WI_OP);

			// Check all required Properties were set
			var collection3 = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "NEWP3"));
			AssertEquals("NEWP3", collection3[0].OP_PartNum);
			AssertEquals("NewDesc3", collection3[0].OP_Desc);
			AssertEquals("CODE", collection3[0].OP_RH_NKCommodityCode);
			AssertEquals("BAG", collection3[0].OP_StockKeepingUnit);
			AssertNotNull("Client should be owner", collection3[0].RelatedOrganisations.FindByOrganisationPKAndExactRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner));
			AssertEquals("Since Product was created, its PK should be now bound to Inventory.", collection3[0].PK, inventory3.WI_OP);

			var collection4 = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "NEWP4"));
			AssertEquals("No product should be created since not all mandatory new product properties set.", 0, collection4.Length);
			AssertEquals("Since Product was not created, Inventory Product PK should be still invalid.", ZGuid.Invalid, inventory4.WI_OP);

			// if product was created independantly, then inventory should update it's values to match that product
			var collection5 = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "NEWP5"));
			AssertEquals("Only one product should be in DB, the one created independantly.", 1, collection5.Length);
			AssertEquals("inventory should receive link to the independantly created product", collection5[0].PK, inventory5.WI_OP);
			AssertEquals("NEWP5", inventory5.WI_OP_PartNum);
			AssertEquals("Different desc", inventory5.WI_OP_Desc);
			AssertEquals("CODE", inventory5.CommodityCode);
			AssertEquals("BAG", inventory5.WI_UnitsUQ);
			AssertNotNull("Client should be owner", inventory5.SupplierPart.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner));

			var collection6 = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "NEWP6"));
			AssertEquals("No product should be created since Stock Keeping Units is incorrect.", 0, collection6.Length);
			AssertEquals("Since Product was not created, Inventory Product PK should be still invalid.", ZGuid.Invalid, inventory6.WI_OP);

			var collection7 = Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "NEWP7"));
			AssertEquals("No product should be created since Commodity Code is incorrect.", 0, collection7.Length);
			AssertEquals("Since Product was not created, Inventory Product PK should be still invalid.", ZGuid.Invalid, inventory7.WI_OP);
		}

		#endregion

		#region TestCreateProductFiles_SetAttributeUse

		public void TestCreateProductFiles_SetAttributeUse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, false);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Env.Security.WhsReceiveCreateProductFiles.IsAllowed = true;
			CreateWhsReceiveInventoryLine(receive, "NP1", "NEW PRODUCT 1", "", "UNT", "PA1", "", "", ZDate.Empty, ZDate.Empty); // All Attributes
			CreateWhsReceiveInventoryLine(receive, "NP1", "NEW PRODUCT 1", "", "UNT", "", "PA2", "", ZDate.Empty, ZDate.Today);
			CreateWhsReceiveInventoryLine(receive, "NP1", "NEW PRODUCT 1", "", "UNT", "", "", "PA3", ZDate.Today, ZDate.Empty);
			CreateWhsReceiveInventoryLine(receive, "NP1", "NEW PRODUCT 1", "", "UNT", "", "", "", ZDate.Empty, ZDate.Empty);
			var inventory = CreateWhsReceiveInventoryLine(receive, "NP1", "NEW PRODUCT 1", "", "UNT", "", "", "", ZDate.Empty, ZDate.Empty);
			inventory.WI_SerialNumber = "SN1";
			inventory.WI_InDocketLineUnits = 1m;

			CreateWhsReceiveInventoryLine(receive, "NP2", "NEW PRODUCT 2", "", "UNT", "PA1", "", "", ZDate.Empty, ZDate.Empty); // Only 1 Part Attrib
			CreateWhsReceiveInventoryLine(receive, "NP3", "NEW PRODUCT 3", "", "UNT", "", "", "", ZDate.Today, ZDate.Empty); // Only 1 DateTime Attrib
			CreateWhsReceiveInventoryLine(receive, "NP4", "NEW PRODUCT 4", "", "UNT", "", "", "", ZDate.Empty, ZDate.Empty); // No Attrib

			receive.CreateProductFiles();
			AssertProductPartAttribUse("NP1", isExpectedPartAttrib1Use: true, isExpectedPartAttrib2Use: true, isExpectedPartAttrib3Use: false, isExpectedSerialUse: true, isExpectedExpiryDateUse: true, isExpectedPackingDateUse: true); // PA3 is not set on Organization, so shouldn't be selected as used ever if value entered.
			AssertProductPartAttribUse("NP2", isExpectedPartAttrib1Use: true, isExpectedPartAttrib2Use: false, isExpectedPartAttrib3Use: false, isExpectedSerialUse: false, isExpectedExpiryDateUse: false, isExpectedPackingDateUse: false); // PA1 is the only not empty attribute.
			AssertProductPartAttribUse("NP3", isExpectedPartAttrib1Use: false, isExpectedPartAttrib2Use: false, isExpectedPartAttrib3Use: false, isExpectedSerialUse: false, isExpectedExpiryDateUse: true, isExpectedPackingDateUse: false); // ExpiryDate is the only not empty attribute.
			AssertProductPartAttribUse("NP4", isExpectedPartAttrib1Use: false, isExpectedPartAttrib2Use: false, isExpectedPartAttrib3Use: false, isExpectedSerialUse: false, isExpectedExpiryDateUse: false, isExpectedPackingDateUse: false); // All Attributes are empty, none should be set for usage.
		}

		void AssertProductPartAttribUse(string partNum, bool isExpectedPartAttrib1Use, bool isExpectedPartAttrib2Use, bool isExpectedPartAttrib3Use, bool isExpectedSerialUse, bool isExpectedExpiryDateUse, bool isExpectedPackingDateUse)
		{
			var part = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, partNum));
			AssertNotNull("Product should be created.", part);
			AssertEquals(isExpectedPartAttrib1Use, part.RelatedOrganisations[0].OU_UsePartAttrib1);
			AssertEquals(isExpectedPartAttrib2Use, part.RelatedOrganisations[0].OU_UsePartAttrib2);
			AssertEquals(isExpectedPartAttrib3Use, part.RelatedOrganisations[0].OU_UsePartAttrib3);
			AssertEquals(isExpectedSerialUse, part.RelatedOrganisations[0].OU_UseSerialNumber);
			AssertEquals(isExpectedExpiryDateUse, part.RelatedOrganisations[0].OU_UseExpiryDate);
			AssertEquals(isExpectedPackingDateUse, part.RelatedOrganisations[0].OU_UsePackingDate);
		}

		#endregion

		#region TestCreateProduct_ShouldSetDefaultForAttributeNeutral

		public void TestCreateProduct_ShouldSetDefaultForAttributeNeutral()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			var partCode = "Code1";
			Helper.SetClientAttributeType(client, AttributeNumber.Serial, true);
			client.MiscServ.OM_WhsDefaultWarehousePickMode = WhsPickMode.Codes.AttributeNeutral;

			var receive = Helper.CreateWhsReceive(client, data.Whs1, "R1", Notify);
			Env.Security.WhsReceiveCreateProductFiles.IsAllowed = true;
			var inventory = CreateWhsReceiveInventoryLine(receive, partCode, "Product 1", string.Empty, "UNT", string.Empty, string.Empty, string.Empty, ZDate.Empty, ZDate.Empty);
			inventory.WI_SerialNumber = "Serial1";

			receive.CreateProductFiles();

			var part = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, partCode));
			var relation = part.RelatedOrganisations.FindFirstByOrganisationPK(client.PK);

			AssertEquals(false, relation.OU_UsePartAttrib1);
			AssertEquals(false, relation.OU_UsePartAttrib2);
			AssertEquals(false, relation.OU_UsePartAttrib3);
			AssertEquals(true, relation.OU_UseSerialNumber);
			AssertEquals(relation.OU_PickMode, WhsPickMode.Codes.AttributeNeutral);
			AssertEquals(relation.OU_RFAttributeConfirm, RFAttributeConfirmCode.Codes.SerialNumber);
		}

		#endregion

		#region CreateWhsReceiveInventoryLine

		WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, ZString partCode, ZString partDesc, ZString commodityCode, ZString stockKeepingUnit,
				 ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZDate expiryDate, ZDate packingDate)
		{
			WhsInventoryView inventory = inventory = receive.Lines.AddNew().Inventory[0];
			inventory.WI_OP = ZGuid.Invalid;
			inventory.WI_OP_PartNum = partCode;
			inventory.WI_OP_Desc = partDesc;
			inventory.CommodityCode = commodityCode;
			inventory.WI_UnitsUQ = stockKeepingUnit;
			inventory.WI_InDocketLineUnits = 10m;

			inventory.WI_PartAttrib1 = partAttrib1;
			inventory.WI_PartAttrib2 = partAttrib2;
			inventory.WI_PartAttrib3 = partAttrib3;
			inventory.WI_ExpiryDate = expiryDate;
			inventory.WI_PackingDate = packingDate;
			return inventory;
		}

		WhsInventoryView CreateWhsReceiveInventoryLine(WhsReceive receive, ZString partCode, ZString partDesc, ZString commodityCode, ZString stockKeepingUnit)
		{
			return CreateWhsReceiveInventoryLine(receive, partCode, partDesc, commodityCode, stockKeepingUnit, "", "", "", ZDate.Empty, ZDate.Empty);
		}

		#endregion

		#endregion

		#region Reconcile ASN

		#region TestReconcileASN

		public void TestReconcileASN()
		{
			var year = ZDateTime.Now.Year;

			WhsWarehouse warehouse = Helper.CreateWarehouse("TST", "A", 2, 2);
			OrgHeader client = Helper.CreateClient();
			client.MiscServ.OM_IMPartAttrib1Name = "Attr1";
			client.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.NonMandatory;
			client.MiscServ.OM_IMPartAttrib2Name = "Attr2";
			client.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.NonMandatory;
			client.MiscServ.OM_IMPartAttrib3Name = "Attr3";
			client.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.NonMandatory;

			OrgSupplierPart part1 = Helper.CreateProduct(client, "Part1");
			part1.OP_StockKeepingUnit = "UNT";
			OrgPartRelation relation1 = part1.RelatedOrganisations.FindFirstByOrganisationPK(client.PK);
			relation1.OU_UsePartAttrib1 = true;
			relation1.OU_UsePartAttrib2 = true;
			relation1.OU_UsePartAttrib3 = true;
			OrgSupplierPart part2 = Helper.CreateProduct(client, "Part2");
			part2.OP_StockKeepingUnit = "KG";
			OrgPartRelation relation2 = part2.RelatedOrganisations.FindFirstByOrganisationPK(client.PK);
			relation2.OU_UsePartAttrib1 = true;
			relation2.OU_UsePartAttrib2 = true;
			relation2.OU_UsePartAttrib3 = true;

			OrgSupplierPart part3 = Helper.CreateProduct(client, "Part3");
			part1.OP_StockKeepingUnit = "UNT";

			WhsReceive receive = Helper.CreateWhsReceive(client, warehouse);
			AssertEquals(0, receive.Inventory.Count);
			AssertEquals(0, receive.AsnLines.Count);

			AddInventoryLine(receive, part1, "", "", "", ZDate.Empty, ZDate.Empty, 10m);
			AddInventoryLine(receive, part1, "PA1", "", "", ZDate.Empty, ZDate.Empty, 15m);
			AddInventoryLine(receive, part1, "PA11", "", "", ZDate.Empty, ZDate.Empty, 15m);
			AddInventoryLine(receive, part1, "", "PA2", "", ZDate.Empty, ZDate.Empty, 25m);
			AddInventoryLine(receive, part1, "", "PA21", "", ZDate.Empty, ZDate.Empty, 25m);
			AddInventoryLine(receive, part1, "", "", "PA3", ZDate.Empty, ZDate.Empty, 30m);
			AddInventoryLine(receive, part1, "", "", "PA31", ZDate.Empty, ZDate.Empty, 30m);
			AddInventoryLine(receive, part1, "", "", "", new ZDate(year, 05, 12), ZDate.Empty, 35m);
			AddInventoryLine(receive, part1, "", "", "", new ZDate(year, 05, 13), ZDate.Empty, 35m);
			AddInventoryLine(receive, part1, "", "", "", ZDate.Empty, new ZDate(year, 05, 13), 40m);
			AddInventoryLine(receive, part1, "", "", "", ZDate.Empty, new ZDate(year, 05, 14), 40m);
			AddInventoryLine(receive, part1, "PA1", "PA2", "PA3", ZDate.Empty, ZDate.Empty, 45m);
			AddInventoryLine(receive, part1, "PA1", "PA2", "PA3", new ZDate(year, 05, 12), new ZDate(year, 05, 12), 50m);

			AddInventoryLine(receive, part2, "", "", "", ZDate.Empty, ZDate.Empty, 10m);
			AddInventoryLine(receive, part2, "PA1", "", "", ZDate.Empty, ZDate.Empty, 15m);
			AddInventoryLine(receive, part2, "PA11", "", "", ZDate.Empty, ZDate.Empty, 15m);
			AddInventoryLine(receive, part2, "", "PA2", "", ZDate.Empty, ZDate.Empty, 25m);
			AddInventoryLine(receive, part2, "", "PA21", "", ZDate.Empty, ZDate.Empty, 25m);
			AddInventoryLine(receive, part2, "", "", "PA3", ZDate.Empty, ZDate.Empty, 30m);
			AddInventoryLine(receive, part2, "", "", "PA31", ZDate.Empty, ZDate.Empty, 30m);
			AddInventoryLine(receive, part2, "", "", "", new ZDate(year, 05, 12), ZDate.Empty, 35m);
			AddInventoryLine(receive, part2, "", "", "", new ZDate(year, 05, 13), ZDate.Empty, 35m);
			AddInventoryLine(receive, part2, "", "", "", ZDate.Empty, new ZDate(year, 05, 13), 40m);
			AddInventoryLine(receive, part2, "", "", "", ZDate.Empty, new ZDate(year, 05, 14), 40m);
			AddInventoryLine(receive, part2, "PA1", "PA2", "PA3", ZDate.Empty, ZDate.Empty, 45m);
			AddInventoryLine(receive, part2, "PA1", "PA2", "PA3", new ZDate(year, 05, 12), new ZDate(year, 05, 12), 50m);
			receive.Inventory.Sort("WI_InDocketLineUnits", System.ComponentModel.ListSortDirection.Descending);

			AssertEquals(26, receive.Inventory.Count);
			AssertPartInventoryAfterASNReconcilationExpectedQuantity(receive, part1, "UNT", 10m, 15m, 15m, 25m, 25m, 30m, 30m, 35m, 35m, 40m, 40m, 45m, 50m);
			AssertPartInventoryAfterASNReconcilationExpectedQuantity(receive, part2, "KG", 10m, 15m, 15m, 25m, 25m, 30m, 30m, 35m, 35m, 40m, 40m, 45m, 50m);

			var originalInventory = receive.Inventory.ToArray();
			var receive2 = Helper.CreateWhsReceive(client, warehouse);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive2, part3, 395m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			receive2.RunPreSaveValidation();
			var part3AsnLine = receive.AsnLines.Add(inventory.InDocketLine);
			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			receive.ReconcileASN();
			AssertEquals(27, receive.Inventory.Count);
			AssertPartInventoryAfterASNReconcilationExpectedQuantity(receive, part1, "UNT", 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m);
			AssertPartInventoryAfterASNReconcilationExpectedQuantity(receive, part2, "KG", 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m);

			// if there no Inventory that match AsnLine attributes, then should be created new Inventory with WN_Units as Expected quantity and WI_TotalUnits == 0
			AssertInventoryExpectedQty(receive, part3, "", "", "", ZDate.Empty, ZDate.Empty, 395m, "UNT", 1);
			AssertInventoryTotalUnits(receive, part3, "", "", "", ZDate.Empty, ZDate.Empty, 0m, "UNT", 1);
			AssertEquals(true, receive.Inventory.Except(originalInventory).Cast<WhsInventoryView>().All(i => i.WI_InventoryStatus == InventoryStatus.Codes.Arrived));

			inventory.InDocketLine.WE_OP = part1.PK;
			var part1AsnLine = receive.AsnLines.Add(inventory.InDocketLine);

			inventory.InDocketLine.WE_OP = part2.PK;
			var part2AsnLine = receive.AsnLines.Add(inventory.InDocketLine);

			AssertEquals(3, receive.AsnLines.Count);
			receive.ReconcileASN();
			AssertEquals(27, receive.Inventory.Count);
			AssertPartInventoryAfterASNReconcilationExpectedQuantity(receive, part1, "UNT", 10m, 15m, 15m, 25m, 25m, 30m, 30m, 35m, 35m, 40m, 40m, 45m, 50m);
			AssertPartInventoryAfterASNReconcilationExpectedQuantity(receive, part2, "KG", 10m, 15m, 15m, 25m, 25m, 30m, 30m, 35m, 35m, 40m, 40m, 45m, 50m);
			AssertEquals(true, receive.Inventory.Except(originalInventory).Cast<WhsInventoryView>().All(i => i.WI_InventoryStatus == InventoryStatus.Codes.Arrived));

			part1AsnLine.WN_Quantity = 5m;
			part2AsnLine.WN_Quantity = 305m;
			part3AsnLine.WN_Quantity = 100m;
			receive.ReconcileASN();
			AssertEquals(27, receive.Inventory.Count);
			AssertPartInventoryAfterASNReconcilationExpectedQuantity(receive, part1, "UNT", 5m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m);
			AssertPartInventoryAfterASNReconcilationExpectedQuantity(receive, part2, "KG", 10m, 15m, 15m, 25m, 25m, 30m, 30m, 35m, 35m, 40m, 40m, 5m, 0m);

			AssertInventoryExpectedQty(receive, part3, "", "", "", ZDate.Empty, ZDate.Empty, 100m, "UNT", 1);
			AssertInventoryTotalUnits(receive, part3, "", "", "", ZDate.Empty, ZDate.Empty, 0m, "UNT", 1);
			AssertEquals(true, receive.Inventory.Except(originalInventory).Cast<WhsInventoryView>().All(i => i.WI_InventoryStatus == InventoryStatus.Codes.Arrived));

			part1AsnLine.WN_Quantity = 405m;
			part2AsnLine.WN_Quantity = 120m;
			part2AsnLine.WN_PartAttrib1 = "PA1";
			receive.ReconcileASN();
			AssertEquals(27, receive.Inventory.Count);
			AssertPartInventoryAfterASNReconcilationExpectedQuantity(receive, part1, "UNT", 20m, 15m, 15m, 25m, 25m, 30m, 30m, 35m, 35m, 40m, 40m, 45m, 50m);
			AssertPartInventoryAfterASNReconcilationExpectedQuantity(receive, part2, "KG", 0m, 25m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 45m, 50m);
			AssertEquals(true, receive.Inventory.Except(originalInventory).Cast<WhsInventoryView>().All(i => i.WI_InventoryStatus == InventoryStatus.Codes.Arrived));

			part1AsnLine.WN_Quantity = 90m;
			part1AsnLine.WN_PartAttrib1 = "PA1";
			part1AsnLine.WN_PartAttrib2 = "PA2";
			part2AsnLine.WN_Quantity = 100m;
			part2AsnLine.WN_PartAttrib1 = "PA1";
			part2AsnLine.WN_PartAttrib2 = "PA2";
			part2AsnLine.WN_PartAttrib3 = "PA3";
			receive.ReconcileASN();
			AssertEquals(27, receive.Inventory.Count);
			AssertPartInventoryAfterASNReconcilationExpectedQuantity(receive, part1, "UNT", 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 45m, 45m);
			AssertPartInventoryAfterASNReconcilationExpectedQuantity(receive, part2, "KG", 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 50m, 50m);
			AssertEquals(true, receive.Inventory.Except(originalInventory).Cast<WhsInventoryView>().All(i => i.WI_InventoryStatus == InventoryStatus.Codes.Arrived));

			part1AsnLine.WN_Quantity = 120m;
			part1AsnLine.WN_PartAttrib1 = "";
			part1AsnLine.WN_PartAttrib2 = "PA2";
			part2AsnLine.WN_Quantity = 60m;
			part2AsnLine.WN_PartAttrib1 = "PA1";
			part2AsnLine.WN_PartAttrib2 = "PA2";
			part2AsnLine.WN_PartAttrib3 = "PA3";
			part2AsnLine.WN_PackingDate = new ZDate(year, 05, 12);
			part2AsnLine.WN_ExpiryDate = new ZDate(year, 05, 12);
			receive.ReconcileASN();
			AssertEquals(27, receive.Inventory.Count);
			AssertPartInventoryAfterASNReconcilationExpectedQuantity(receive, part1, "UNT", 0m, 0m, 0m, 25m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 45m, 50m);
			AssertPartInventoryAfterASNReconcilationExpectedQuantity(receive, part2, "KG", 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 60m);
			AssertEquals(true, receive.Inventory.Except(originalInventory).Cast<WhsInventoryView>().All(i => i.WI_InventoryStatus == InventoryStatus.Codes.Arrived));

			part1AsnLine.WN_Quantity = 35m;
			part1AsnLine.WN_PartAttrib1 = "";
			part1AsnLine.WN_PartAttrib2 = "";
			part1AsnLine.WN_PartAttrib3 = "PA31";
			part2AsnLine.WN_Quantity = 70m;
			part2AsnLine.WN_PartAttrib1 = "";
			part2AsnLine.WN_PartAttrib2 = "";
			part2AsnLine.WN_PartAttrib3 = "";
			part2AsnLine.WN_PackingDate = new ZDate(year, 05, 12);
			part2AsnLine.WN_ExpiryDate = ZDate.Empty;
			receive.ReconcileASN();
			AssertEquals(27, receive.Inventory.Count);
			AssertPartInventoryAfterASNReconcilationExpectedQuantity(receive, part1, "UNT", 0m, 0m, 0m, 0m, 0m, 0m, 35m, 0m, 0m, 0m, 0m, 0m, 0m);
			AssertPartInventoryAfterASNReconcilationExpectedQuantity(receive, part2, "KG", 0m, 0m, 0m, 0m, 0m, 0m, 0m, 35m, 0m, 0m, 0m, 0m, 35m);
			AssertEquals(true, receive.Inventory.Except(originalInventory).Cast<WhsInventoryView>().All(i => i.WI_InventoryStatus == InventoryStatus.Codes.Arrived));

			part1AsnLine.WN_Quantity = 35m;
			part1AsnLine.WN_PartAttrib1 = "";
			part1AsnLine.WN_PartAttrib2 = "";
			part1AsnLine.WN_PartAttrib3 = "PA31";
			part1AsnLine.WN_PackingDate = new ZDate(year, 05, 12);
			part2AsnLine.WN_Quantity = 50m;
			part2AsnLine.WN_PartAttrib1 = "";
			part2AsnLine.WN_PartAttrib2 = "";
			part2AsnLine.WN_PartAttrib3 = "";
			part2AsnLine.WN_PackingDate = ZDate.Empty;
			part2AsnLine.WN_ExpiryDate = new ZDate(year, 05, 13);
			receive.ReconcileASN();
			AssertEquals(28, receive.Inventory.Count);
			AssertPartInventoryAfterASNReconcilationExpectedQuantity(receive, part1, "UNT", 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m);
			AssertPartInventoryAfterASNReconcilationExpectedQuantity(receive, part2, "KG", 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 50m, 0m, 0m, 0m);

			// there isn't Inventory with such attributes, so system create new one with TotalUnits == 0 and ExperctedQuantity == WN_Quantity with specific attributes.
			AssertInventoryExpectedQty(receive, part1, "", "", "PA31", new ZDate(year, 05, 12), ZDate.Empty, 35m, "UNT", 1);
			AssertInventoryTotalUnits(receive, part1, "", "", "PA31", new ZDate(year, 05, 12), ZDate.Empty, 0m, "UNT", 1);
			AssertEquals(true, receive.Inventory.Except(originalInventory).Cast<WhsInventoryView>().All(i => i.WI_InventoryStatus == InventoryStatus.Codes.Arrived));
		}

		#endregion

		#region TestReconcileASNConsidersPalletID

		public void TestReconcileASNConsidersPalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);
			var year = ZDateTime.Today.Year;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10, 1, 0);
			inventory1.WI_PalletID = "PLT-1";
			inventory1.WI_PackingDate = new ZDate(year, 3, 2);
			inventory1.WI_ExpiryDate = new ZDate(year, 3, 2);
			inventory1.WI_PartAttrib1 = "PA1";
			inventory1.WI_PartAttrib2 = "PA2";
			inventory1.WI_PartAttrib3 = "PA3";

			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15, 2, 0);
			inventory2.WI_PalletID = "PLT-2";
			inventory2.WI_PackingDate = new ZDate(year, 3, 2);
			inventory2.WI_ExpiryDate = new ZDate(year, 3, 2);
			inventory2.WI_PartAttrib1 = "PA1";
			inventory2.WI_PartAttrib2 = "PA2";
			inventory2.WI_PartAttrib3 = "PA3";

			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 8, 2, 0);
			inventory3.WI_PalletID = "PLT-2";
			inventory3.WI_PackingDate = new ZDate(year, 3, 2);
			inventory3.WI_ExpiryDate = new ZDate(year, 3, 2);
			inventory3.WI_PartAttrib1 = "PA1";
			inventory3.WI_PartAttrib2 = "PA2";
			inventory3.WI_PartAttrib3 = "PA3";

			Factory.Save();

			receive.PopulateASNLines();
			receive.Lines.DeleteAll();
			receive.Inventory.RemoveAndDeleteAll();

			AssertEquals(2, receive.AsnLines.Count);
			AssertEquals("Precondition: 10 for PLT-1", 10m, receive.AsnLines.Cast<WhsAsnLine>().Single(a => a.WN_PalletId == "PLT-1").WN_Quantity);
			AssertEquals("Precondition: 23 for PLT-2", 23m, receive.AsnLines.Cast<WhsAsnLine>().Single(a => a.WN_PalletId == "PLT-2").WN_Quantity);

			// now create a receive line with swapped quantities for the pallets
			var receiveLineUnloaded1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 23, null, "PLT-1", new ZDate(year, 3, 2), new ZDate(year, 3, 2), "PA1", "PA2", "PA3", "");
			var receiveLineUnloaded2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10, null, "PLT-2", new ZDate(year, 3, 2), new ZDate(year, 3, 2), "PA1", "PA2", "PA3", "");
			receive.ReconcileASN();

			AssertEquals(3, receive.Lines.Count);
			AssertEquals(10m, receiveLineUnloaded1.WI_ExpectedReceiptQuantity);
			AssertEquals(10m, receiveLineUnloaded1.WI_TotalUnits);
			AssertEquals((ZShort)1, receiveLineUnloaded1.WI_LineNo);
			AssertEquals(23m, receiveLineUnloaded2.WI_ExpectedReceiptQuantity);
			AssertEquals(10m, receiveLineUnloaded2.WI_TotalUnits);
			AssertEquals((ZShort)2, receiveLineUnloaded2.WI_LineNo);

			var newInventory = receive.Inventory.Cast<WhsInventoryView>().Single(line => line.WI_LineNo == 0);
			AssertEquals(0m, newInventory.WI_ExpectedReceiptQuantity);
			AssertEquals(13m, newInventory.WI_InDocketLineUnits);
		}

		#endregion

		#region TestReconcileASNDoesNotExecuteIfFinalisedOrCancelled

		public void TestReconcileASNDoesNotExecuteIfFinalisedOrCancelled()
		{
			var receive = GetNewBusinessObject();
			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			receive.ReconcileASN();
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
			receive.WD_FinalisedDate = ZDateTimeOffset.Empty;

			((NotificationBuffer)receive.NotificationManager.Peek).Clear();
			receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			receive.ReconcileASN();
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		#endregion

		#region TestReconcileASN_SerialNumberProducts

		public void TestReconcileASN_SerialNumberProducts()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsTestHelperFunctions(factory);
			var data = new TestDataSimpleEnvironment(factory);
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);

			data.Org1.MiscServ.OM_IMUseSerialNumber = true;
			data.Org1.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.NonMandatory;

			var orgPartRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			orgPartRelation.OU_UseSerialNumber = true;
			orgPartRelation.OU_UsePartAttrib1 = true;

			var inventoryWithAttribute = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, 1, 0);
			inventoryWithAttribute.WI_SerialNumber = "";
			inventoryWithAttribute.WI_PartAttrib1 = "Red";

			var inventoryWithoutAttribute = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, 2, 0);
			inventoryWithoutAttribute.WI_SerialNumber = "";
			inventoryWithoutAttribute.WI_PartAttrib1 = "";

			var inventoryWithSerialNumber = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, 3, 0);
			inventoryWithSerialNumber.WI_SerialNumber = "S3";
			inventoryWithSerialNumber.WI_PartAttrib1 = "Red";
			factory.Save();

			AssertEquals("Precondition - No ASN lines were created.", 0, receive.AsnLines.Count);
			receive.PopulateASNLines();
			AssertEquals("Precondition - 3 ASN lines must be created.", 3, receive.AsnLines.Count);
			AssertEquals("Precondition - Inventory lines should not be removed.", 3, receive.Inventory.Count);

			inventoryWithAttribute.WI_SerialNumber = "S1";
			inventoryWithoutAttribute.WI_SerialNumber = "S2";
			receive.ReconcileASN();
			AssertEquals("Since serial number attribute was blank, Inventory lines should not be created for overs and unders.", 3, receive.Inventory.Count);
			receive.Inventory.Cast<WhsInventoryView>().Single(i => i.WI_SerialNumber == "S1" && i.WI_PartAttrib1 == "Red");
			receive.Inventory.Cast<WhsInventoryView>().Single(i => i.WI_SerialNumber == "S2" && i.WI_PartAttrib1 == "");
			receive.Inventory.Cast<WhsInventoryView>().Single(i => i.WI_SerialNumber == "S3" && i.WI_PartAttrib1 == "Red");
		}

		#endregion

		#region TestReconcileASN_SerialNumbersWereNotKnownBeforeUnloading

		public void TestReconcileASN_SerialNumbersWereNotKnownBeforeUnloading()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			data.Org1.MiscServ.OM_IMUseSerialNumber = true;
			data.Org1.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.NonMandatory;

			var orgPartRelation = data.Part1.RelatedOrganisations.FindFirstByOrganisationPK(data.Org1.PK);
			orgPartRelation.OU_UseSerialNumber = true;
			orgPartRelation.OU_UsePartAttrib1 = true;

			var inventoryWithAttribute = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, 1, 0);
			inventoryWithAttribute.WI_SerialNumber = "";
			inventoryWithAttribute.WI_PartAttrib1 = "Red";

			var inventoryWithoutAttribute = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, 2, 0);
			inventoryWithoutAttribute.WI_SerialNumber = "";
			inventoryWithoutAttribute.WI_PartAttrib1 = "";

			var inventoryWithSerialNumber = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, 3, 0);
			inventoryWithSerialNumber.WI_SerialNumber = "S3";
			inventoryWithSerialNumber.WI_PartAttrib1 = "Red";
			Factory.Save();

			AssertEquals("Precondition - No ASN lines were created.", 0, receive.AsnLines.Count);
			receive.PopulateASNLines();
			AssertEquals("Precondition - 3 ASN lines must be created.", 3, receive.AsnLines.Count);
			AssertEquals("Precondition - Inventory lines should not be removed.", 3, receive.Inventory.Count);

			inventoryWithAttribute.WI_SerialNumber = "S1";
			inventoryWithoutAttribute.WI_SerialNumber = "S2";
			receive.ReconcileASN();
			AssertEquals("Since serial number attribute was blank, Inventory lines should not be created for overs and unders.", 3, receive.Inventory.Count);
			receive.Inventory.Cast<WhsInventoryView>().Single(i => i.WI_SerialNumber == "S1" && i.WI_PartAttrib1 == "Red");
			receive.Inventory.Cast<WhsInventoryView>().Single(i => i.WI_SerialNumber == "S2" && i.WI_PartAttrib1 == "");
			receive.Inventory.Cast<WhsInventoryView>().Single(i => i.WI_SerialNumber == "S3" && i.WI_PartAttrib1 == "Red");
		}

		#endregion

		#region TestReconcileASN_ReceiveLineWithPutAwayTransferNotSplit_MultipleASN

		public void TestReconcileASN_ReceiveLineWithPutAwayTransferNotSplit_MultipleASN()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m);
			Factory.Save();

			receive.PopulateASNLines();
			receive.Lines.DeleteAll();
			Factory.Save();
			AssertEquals("Precondition", 0, receive.Lines.Count);

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 25m, dockDoorLocation, "P0001");
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "P0001", 25m);
			putawayTransfer.RunPreSaveValidation();
			Factory.Save();

			AssertEquals("Precondition", 1, receive.Lines.Count);
			AssertEquals("Precondition", 2, receive.AsnLines.Count);
			AssertEquals("Precondition", 1, receive.Lines[0].Inventory.Count);
			AssertEquals("Precondition", true, receive.Lines[0].Inventory[0].HasPutawayTransfer);

			receive.ReconcileASN();
			AssertEquals("No additional receive line should have been created from ASN lines.", 1, receive.Lines.Count);

			var inventory = receive.Inventory.Cast<WhsInventoryView>().Single();
			AssertEquals("Inventory sum should still be 25.", 25m, inventory.WI_TotalUnits);
			AssertEquals("Inventory expected quantity must be 25.", 25m, inventory.WI_ExpectedReceiptQuantity);
			AssertEquals("Receive lines should have no errors.", false, receive.Lines.HasErrors());

			Factory.Save();
			AssertEquals("Receive lines should have no errors after saving.", false, receive.Lines.HasErrors());
		}

		#endregion

		#region TestReconcileASNDoesNotExecuteIfReceiveHasErrors

		public void TestReconcileASNDoesNotExecuteIfReceiveHasErrors()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Factory.Save();

			receive.PopulateASNLines();
			AssertEquals("Precondition: Receive has ASN Lines.", true, receive.AsnLines.Count > 0);
			AssertEquals("Precondition: Receive has no errors.", false, receive.HasErrors);

			receiveLine.WE_OP = ZGuid.Empty;
			AssertEquals("Precondition: Receive has errors.", true, receive.HasErrors);
			AssertNoExceptionThrown("No exception is thrown when ReconcileASN is ran.", receive.ReconcileASN);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(ReceiveErrorTypes.CalculateOversAndUndersRequiresReceiptToHaveNoErrors));
		}

		#endregion

		#region TestReconcileASNDoesNotThrowExceptionIfProductIsNull

		public void TestReconcileASNDoesNotThrowExceptionIfProductIsNull()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Factory.Save();

			receive.PopulateASNLines();
			AssertEquals("Precondition: Receive has ASN Lines.", true, receive.AsnLines.Count > 0);
			AssertEquals("Precondition: Receive has no errors.", false, receive.HasErrors);

			using (receive.GetValidationSuspender()) // to allow ReconcileASN to run without product
			{
				receiveLine.WE_OP = ZGuid.Empty;
				AssertEquals("Precondition: Receive has no errors.", false, receive.HasErrors);
				AssertNull("Precondition: product is null.", receiveLine.Product);
				AssertNoExceptionThrown("No exception is thrown when ReconcileASN is ran.", receive.ReconcileASN);
			}
		}

		#endregion

		#region TestReconcileASNRemapsUnfulfilledReservedStock

		public void TestReconcileASNRemapsUnfulfilledReservedStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLineWithNoTxnQty = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLineWithNoTxnQty.WE_TransactionQuantity = 0m;
			var receiveLineWithTxnQty = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateReservePickLine(orderLine, receiveLineWithNoTxnQty.Inventory[0], 10m);
			Factory.Save();

			receive.PopulateASNLines();

			AssertEquals("Precondition: Receive has ASN Lines.", true, receive.AsnLines.Count > 0);
			AssertEquals("Precondition: Receive has no errors.", false, receive.HasErrors);
			AssertEquals("Precondition", 10m, receiveLineWithNoTxnQty.ReservedQuantity);
			AssertEquals("Precondition", 0m, receiveLineWithTxnQty.ReservedQuantity);
			AssertEquals("Precondition", 10m, orderLine.WE_CrossDockQuantity);
			AssertEquals("Precondition", 2, receive.Lines.Count);
			AssertEquals("Precondition", 2, receive.AsnLines.Count);
			receive.ReconcileASN();

			AssertEquals("No new receive line is created.", 2, receive.Lines.Count);
			AssertEquals("Expected Quantity", 10m, receiveLineWithTxnQty.WE_ClientOrderedUnits);
			AssertEquals("Transaction Quantity", 10m, receiveLineWithTxnQty.WE_TransactionQuantity);
			AssertEquals("Reserved stock is remapped to receive line with transaction quantity.", 10m, receiveLineWithTxnQty.ReservedQuantity);

			AssertEquals("Expected Quantity", 10m, receiveLineWithNoTxnQty.WE_ClientOrderedUnits);
			AssertEquals("Transaction Quantity", 0m, receiveLineWithNoTxnQty.WE_TransactionQuantity);
			AssertEquals("Reserved Stock", 0m, receiveLineWithNoTxnQty.ReservedQuantity);
		}

		#endregion

		void AssertPartInventoryAfterASNReconcilationExpectedQuantity(WhsReceive receive, OrgSupplierPart part, ZString expUQ, ZDecimal expQty1, ZDecimal expQty2, ZDecimal expQty3
					 , ZDecimal expQty4, ZDecimal expQty5, ZDecimal expQty6, ZDecimal expQty7, ZDecimal expQty8, ZDecimal expQty9
					 , ZDecimal expQty10, ZDecimal expQty11, ZDecimal expQty12, ZDecimal expQty13)
		{
			var year = ZDateTime.Now.Year;

			AssertInventoryExpectedQty(receive, part, "", "", "", ZDate.Empty, ZDate.Empty, expQty1, expUQ, 1);
			AssertInventoryExpectedQty(receive, part, "PA1", "", "", ZDate.Empty, ZDate.Empty, expQty2, expUQ, 1);
			AssertInventoryExpectedQty(receive, part, "PA11", "", "", ZDate.Empty, ZDate.Empty, expQty3, expUQ, 1);
			AssertInventoryExpectedQty(receive, part, "", "PA2", "", ZDate.Empty, ZDate.Empty, expQty4, expUQ, 1);
			AssertInventoryExpectedQty(receive, part, "", "PA21", "", ZDate.Empty, ZDate.Empty, expQty5, expUQ, 1);
			AssertInventoryExpectedQty(receive, part, "", "", "PA3", ZDate.Empty, ZDate.Empty, expQty6, expUQ, 1);
			AssertInventoryExpectedQty(receive, part, "", "", "PA31", ZDate.Empty, ZDate.Empty, expQty7, expUQ, 1);
			AssertInventoryExpectedQty(receive, part, "", "", "", new ZDate(year, 05, 12), ZDate.Empty, expQty8, expUQ, 1);
			AssertInventoryExpectedQty(receive, part, "", "", "", new ZDate(year, 05, 13), ZDate.Empty, expQty9, expUQ, 1);
			AssertInventoryExpectedQty(receive, part, "", "", "", ZDate.Empty, new ZDate(year, 05, 13), expQty10, expUQ, 1);
			AssertInventoryExpectedQty(receive, part, "", "", "", ZDate.Empty, new ZDate(year, 05, 14), expQty11, expUQ, 1);
			AssertInventoryExpectedQty(receive, part, "PA1", "PA2", "PA3", ZDate.Empty, ZDate.Empty, expQty12, expUQ, 1);
			AssertInventoryExpectedQty(receive, part, "PA1", "PA2", "PA3", new ZDate(year, 05, 12), new ZDate(year, 05, 12), expQty13, expUQ, 1);
		}

		#endregion

		#region Populate ASN Lines

		public void TestPopulateASNLines()
		{
			var year = ZDateTime.Now.Year;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_StockKeepingUnit = "UNT";
			data.Part2.OP_StockKeepingUnit = "KG";
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);

			AssertEquals(0, receive.Inventory.Count);

			AddInventoryLine(receive, data.Part1, "", "", "", ZDate.Empty, ZDate.Empty, 10m, 1, 0);
			AddInventoryLine(receive, data.Part1, "", "", "", ZDate.Empty, ZDate.Empty, 10m, 1, 0);

			AddInventoryLine(receive, data.Part2, "", "", "", ZDate.Empty, ZDate.Empty, 15m, 2, 0);
			AddInventoryLine(receive, data.Part2, "", "", "", ZDate.Empty, ZDate.Empty, 15m, 2, 0);

			AddInventoryLine(receive, data.Part1, "PA1", "", "", ZDate.Empty, ZDate.Empty, 13m, 3, 0);
			AddInventoryLine(receive, data.Part1, "PA1", "", "", ZDate.Empty, ZDate.Empty, 13m, 3, 0);

			AddInventoryLine(receive, data.Part2, "PA1", "", "", ZDate.Empty, ZDate.Empty, 14m, 4, 0);
			AddInventoryLine(receive, data.Part2, "PA1", "", "", ZDate.Empty, ZDate.Empty, 14m, 4, 0);

			AddInventoryLine(receive, data.Part1, "PA11", "", "", ZDate.Empty, ZDate.Empty, 16m, 5, 0);
			AddInventoryLine(receive, data.Part1, "PA11", "", "", ZDate.Empty, ZDate.Empty, 16m, 5, 0);

			AddInventoryLine(receive, data.Part2, "PA11", "", "", ZDate.Empty, ZDate.Empty, 17m, 6, 0);
			AddInventoryLine(receive, data.Part2, "PA11", "", "", ZDate.Empty, ZDate.Empty, 17m, 6, 0);

			AddInventoryLine(receive, data.Part1, "", "PA2", "", ZDate.Empty, ZDate.Empty, 1m, 7, 0);
			AddInventoryLine(receive, data.Part1, "", "PA2", "", ZDate.Empty, ZDate.Empty, 1m, 7, 0);

			AddInventoryLine(receive, data.Part2, "", "PA2", "", ZDate.Empty, ZDate.Empty, 2m, 8, 0);
			AddInventoryLine(receive, data.Part2, "", "PA2", "", ZDate.Empty, ZDate.Empty, 2m, 8, 0);

			AddInventoryLine(receive, data.Part1, "", "PA21", "", ZDate.Empty, ZDate.Empty, 3m, 9, 0);
			AddInventoryLine(receive, data.Part1, "", "PA21", "", ZDate.Empty, ZDate.Empty, 3m, 9, 0);

			AddInventoryLine(receive, data.Part2, "", "PA21", "", ZDate.Empty, ZDate.Empty, 4m, 10, 0);
			AddInventoryLine(receive, data.Part2, "", "PA21", "", ZDate.Empty, ZDate.Empty, 4m, 10, 0);

			AddInventoryLine(receive, data.Part1, "", "", "PA3", ZDate.Empty, ZDate.Empty, 22m, 11, 0);
			AddInventoryLine(receive, data.Part1, "", "", "PA3", ZDate.Empty, ZDate.Empty, 22m, 11, 0);

			AddInventoryLine(receive, data.Part2, "", "", "PA3", ZDate.Empty, ZDate.Empty, 23m, 12, 0);
			AddInventoryLine(receive, data.Part2, "", "", "PA3", ZDate.Empty, ZDate.Empty, 23m, 12, 0);

			AddInventoryLine(receive, data.Part1, "", "", "PA31", ZDate.Empty, ZDate.Empty, 24m, 13, 0);
			AddInventoryLine(receive, data.Part1, "", "", "PA31", ZDate.Empty, ZDate.Empty, 24m, 13, 0);

			AddInventoryLine(receive, data.Part2, "", "", "PA31", ZDate.Empty, ZDate.Empty, 25m, 14, 0);
			AddInventoryLine(receive, data.Part2, "", "", "PA31", ZDate.Empty, ZDate.Empty, 25m, 14, 0);

			AddInventoryLine(receive, data.Part1, "", "", "", new ZDate(year, 05, 09), ZDate.Empty, 32m, 15, 0);
			AddInventoryLine(receive, data.Part1, "", "", "", new ZDate(year, 05, 09), ZDate.Empty, 32m, 15, 0);

			AddInventoryLine(receive, data.Part2, "", "", "", new ZDate(year, 05, 09), ZDate.Empty, 33m, 16, 0);
			AddInventoryLine(receive, data.Part2, "", "", "", new ZDate(year, 05, 09), ZDate.Empty, 33m, 16, 0);

			AddInventoryLine(receive, data.Part1, "", "", "", new ZDate(year, 05, 10), ZDate.Empty, 34m, 17, 0);
			AddInventoryLine(receive, data.Part1, "", "", "", new ZDate(year, 05, 10), ZDate.Empty, 34m, 17, 0);

			AddInventoryLine(receive, data.Part2, "", "", "", new ZDate(year, 05, 10), ZDate.Empty, 35m, 18, 0);
			AddInventoryLine(receive, data.Part2, "", "", "", new ZDate(year, 05, 10), ZDate.Empty, 35m, 18, 0);

			AddInventoryLine(receive, data.Part1, "", "", "", ZDate.Empty, new ZDate(year, 05, 09), 42m, 19, 0);
			AddInventoryLine(receive, data.Part1, "", "", "", ZDate.Empty, new ZDate(year, 05, 09), 42m, 19, 0);

			AddInventoryLine(receive, data.Part2, "", "", "", ZDate.Empty, new ZDate(year, 05, 09), 43m, 20, 0);
			AddInventoryLine(receive, data.Part2, "", "", "", ZDate.Empty, new ZDate(year, 05, 09), 43m, 20, 0);

			AddInventoryLine(receive, data.Part1, "", "", "", ZDate.Empty, new ZDate(year, 05, 10), 44m, 21, 0);
			AddInventoryLine(receive, data.Part1, "", "", "", ZDate.Empty, new ZDate(year, 05, 10), 44m, 21, 0);

			AddInventoryLine(receive, data.Part2, "", "", "", ZDate.Empty, new ZDate(year, 05, 10), 45m, 22, 0);
			AddInventoryLine(receive, data.Part2, "", "", "", ZDate.Empty, new ZDate(year, 05, 10), 45m, 22, 0);

			AddInventoryLine(receive, data.Part1, "PA1", "PA2", "PA3", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 52m, 23, 0);
			AddInventoryLine(receive, data.Part1, "PA1", "PA2", "PA3", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 52m, 23, 0);

			AddInventoryLine(receive, data.Part2, "PA1", "PA2", "PA3", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 62m, 24, 0);
			AddInventoryLine(receive, data.Part2, "PA1", "PA2", "PA3", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 62m, 24, 0);

			AddInventoryLine(receive, data.Part1, "", "", "", ZDate.Empty, ZDate.Empty, 10m, 25, 1);
			AddInventoryLine(receive, data.Part1, "", "", "", ZDate.Empty, ZDate.Empty, 10m, 25, 1);

			AddInventoryLine(receive, data.Part2, "", "", "", ZDate.Empty, ZDate.Empty, 15m, 26, 1);
			AddInventoryLine(receive, data.Part2, "", "", "", ZDate.Empty, ZDate.Empty, 15m, 26, 1);

			AddInventoryLine(receive, data.Part1, "PA1", "", "", ZDate.Empty, ZDate.Empty, 13m, 27, 2);
			AddInventoryLine(receive, data.Part1, "PA1", "", "", ZDate.Empty, ZDate.Empty, 13m, 27, 2);
			Factory.Save();

			AssertEquals(54, receive.Inventory.Count);
			AssertEquals(0, receive.AsnLines.Count);

			receive.PopulateASNLines();

			AssertEquals(27, receive.AsnLines.Count);
			AssertUniqueASNLine(receive.AsnLines, data.Part1, "", "", "", ZDate.Empty, ZDate.Empty, 20m, "UNT", 1, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part2, "", "", "", ZDate.Empty, ZDate.Empty, 30m, "KG", 2, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part1, "PA1", "", "", ZDate.Empty, ZDate.Empty, 26m, "UNT", 3, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part2, "PA1", "", "", ZDate.Empty, ZDate.Empty, 28m, "KG", 4, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part1, "PA11", "", "", ZDate.Empty, ZDate.Empty, 32m, "UNT", 5, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part2, "PA11", "", "", ZDate.Empty, ZDate.Empty, 34m, "KG", 6, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part1, "", "PA2", "", ZDate.Empty, ZDate.Empty, 2m, "UNT", 7, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part2, "", "PA2", "", ZDate.Empty, ZDate.Empty, 4m, "KG", 8, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part1, "", "PA21", "", ZDate.Empty, ZDate.Empty, 6m, "UNT", 9, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part2, "", "PA21", "", ZDate.Empty, ZDate.Empty, 8m, "KG", 10, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part1, "", "", "PA3", ZDate.Empty, ZDate.Empty, 44m, "UNT", 11, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part2, "", "", "PA3", ZDate.Empty, ZDate.Empty, 46m, "KG", 12, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part1, "", "", "PA31", ZDate.Empty, ZDate.Empty, 48m, "UNT", 13, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part2, "", "", "PA31", ZDate.Empty, ZDate.Empty, 50m, "KG", 14, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part1, "", "", "", new ZDate(year, 05, 09), ZDate.Empty, 64m, "UNT", 15, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part2, "", "", "", new ZDate(year, 05, 09), ZDate.Empty, 66m, "KG", 16, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part1, "", "", "", new ZDate(year, 05, 10), ZDate.Empty, 68m, "UNT", 17, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part2, "", "", "", new ZDate(year, 05, 10), ZDate.Empty, 70m, "KG", 18, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part1, "", "", "", ZDate.Empty, new ZDate(year, 05, 09), 84m, "UNT", 19, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part2, "", "", "", ZDate.Empty, new ZDate(year, 05, 09), 86m, "KG", 20, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part1, "", "", "", ZDate.Empty, new ZDate(year, 05, 10), 88m, "UNT", 21, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part2, "", "", "", ZDate.Empty, new ZDate(year, 05, 10), 90m, "KG", 22, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part1, "PA1", "PA2", "PA3", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 104m, "UNT", 23, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part2, "PA1", "PA2", "PA3", new ZDate(year, 05, 09), new ZDate(year, 05, 10), 124m, "KG", 24, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part1, "", "", "", ZDate.Empty, ZDate.Empty, 20m, "UNT", 25, 1);
			AssertUniqueASNLine(receive.AsnLines, data.Part2, "", "", "", ZDate.Empty, ZDate.Empty, 30m, "KG", 26, 1);
			AssertUniqueASNLine(receive.AsnLines, data.Part1, "PA1", "", "", ZDate.Empty, ZDate.Empty, 26m, "UNT", 27, 2);
		}

		public void TestPopulateASNLinesDoesNotExecuteIfReceiveIsMarkedAsHeld()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = "CUS";
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);

			receive.Logs.AddNew(Events.HoldTheWarehouseOrder);
			Factory.Save();

			receive.PopulateASNLines();
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(ReceiveErrorTypes.CannotPerformThisOperationBecauseDocketIsHeldByCustoms));
			AssertEquals("Asn lines are not created.", false, receive.AsnLines.Count > 0);

			((NotificationBuffer)receive.NotificationManager.Peek).Clear();

			receive.Logs.AddNew(Events.WarehouseJobCanNowBeFinalised);
			Factory.Save();

			receive.PopulateASNLines();
			AssertEquals(false, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(ReceiveErrorTypes.CannotPerformThisOperationBecauseDocketIsHeldByCustoms));
			AssertEquals("Asn lines are created.", true, receive.AsnLines.Count > 0);
		}

		public void TestPopulateASNLinesDoesNotExecuteIfFinalisedOrCancelled()
		{
			var receive = GetNewBusinessObject();
			receive.WD_FinalisedDate = ZDateTimeOffset.Now;
			receive.PopulateASNLines();
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
			receive.WD_FinalisedDate = ZDateTimeOffset.Empty;

			((NotificationBuffer)receive.NotificationManager.Peek).Clear();
			receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			receive.PopulateASNLines();
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		public void TestPopulateAsnLinesDoesNotClearTransactionQuantityOnReceiveLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Factory.Save();

			receive.PopulateASNLines();
			AssertEquals("Asn lines are created.", true, receive.AsnLines.Count > 0);
			AssertEquals("Receive line's transaction quantity is retained.", 10m, receiveLine.WE_TransactionQuantity);
		}

		protected void AddInventoryLine(WhsReceive receive, OrgSupplierPart part, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZDate packingDate, ZDate expiryDate, ZDecimal units)
		{
			AddInventoryLine(receive, part, partAttrib1, partAttrib2, partAttrib3, packingDate, expiryDate, units, 0, 0);
		}

		protected void AddInventoryLine(WhsReceive receive, OrgSupplierPart part, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZDate packingDate, ZDate expiryDate, ZDecimal units, ZShort lineNo, ZShort subLineNo)
		{
			var inventory = receive.Lines.AddNew().Inventory[0];
			inventory.WI_OP = part.PK;
			inventory.WI_PartAttrib1 = partAttrib1;
			inventory.WI_PartAttrib2 = partAttrib2;
			inventory.WI_PartAttrib3 = partAttrib3;
			inventory.WI_PackingDate = packingDate;
			inventory.WI_ExpiryDate = expiryDate;
			inventory.WI_ExpectedReceiptQuantity = units;
			if (lineNo > 0)
			{
				inventory.WI_LineNo = lineNo;
			}
			if (subLineNo > 0)
			{
				inventory.WI_SubLineNo = subLineNo;
			}
		}

		protected void AssertInventoryExpectedQty(WhsReceive receive, OrgSupplierPart part, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZDate packingDate, ZDate expiryDate, ZDecimal expectedUnits, ZString expectedUnitsUQ, int expectedMatchCount)
		{
			int matchCount = 0;
			foreach (WhsInventoryView inventoryLine in receive.Inventory)
			{
				if (inventoryLine.WI_OP == part.PK)
				{
					if (inventoryLine.WI_PartAttrib1 == partAttrib1 && inventoryLine.WI_PartAttrib2 == partAttrib2 && inventoryLine.WI_PartAttrib3 == partAttrib3)
					{
						if (inventoryLine.WI_PackingDate == packingDate && inventoryLine.WI_ExpiryDate == expiryDate)
						{
							if (inventoryLine.WI_ExpectedReceiptQuantity == expectedUnits && inventoryLine.WI_UnitsUQ == expectedUnitsUQ)
							{
								matchCount++;
							}
						}
					}
				}
			}
			AssertEquals(expectedMatchCount, matchCount);
		}

		protected void AssertInventoryTotalUnits(WhsReceive receive, OrgSupplierPart part, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZDate packingDate, ZDate expiryDate, ZDecimal totalUnits, ZString expectedUnitsUQ, int expectedMatchCount)
		{
			int matchCount = 0;
			foreach (WhsInventoryView inventoryLine in receive.Inventory)
			{
				if (inventoryLine.WI_OP == part.PK)
				{
					if (inventoryLine.WI_PartAttrib1 == partAttrib1 && inventoryLine.WI_PartAttrib2 == partAttrib2 && inventoryLine.WI_PartAttrib3 == partAttrib3)
					{
						if (inventoryLine.WI_PackingDate == packingDate && inventoryLine.WI_ExpiryDate == expiryDate)
						{
							if (inventoryLine.WI_TotalUnits == totalUnits && inventoryLine.WI_UnitsUQ == expectedUnitsUQ)
							{
								matchCount++;
							}
						}
					}
				}
			}
			AssertEquals(expectedMatchCount, matchCount);
		}

		protected void AssertUniqueASNLine(WhsAsnLineCollection asnLines, OrgSupplierPart part, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZDate packingDate, ZDate expiryDate, ZDecimal units, ZString unitsUQ)
		{
			AssertUniqueASNLine(asnLines, part, partAttrib1, partAttrib2, partAttrib3, packingDate, expiryDate, units, unitsUQ, 0, 0);
		}

		protected void AssertUniqueASNLine(WhsAsnLineCollection asnLines, OrgSupplierPart part, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZDate packingDate, ZDate expiryDate, ZDecimal units, ZString unitsUQ, ZShort lineNo, ZShort subLineNo)
		{
			int matchCount = 0;

			foreach (WhsAsnLine line in asnLines)
			{
				if (line.WN_OP == part.PK)
				{
					if (line.WN_PartAttrib1 == partAttrib1 && line.WN_PartAttrib2 == partAttrib2 && line.WN_PartAttrib3 == partAttrib3)
					{
						if (line.WN_PackingDate == packingDate && line.WN_ExpiryDate == expiryDate)
						{
							if (line.WN_LineNo == lineNo && line.WN_SubLineNo == subLineNo)
							{
								AssertEquals(units, line.WN_Quantity);
								AssertEquals(unitsUQ, line.WN_QuantityUQ);
								matchCount++;
							}
						}
					}
				}
			}

			AssertEquals(1, matchCount);
		}

		#endregion

		#region TestCreateAsnLineWithInvalidProducts

		public void TestCreateAsnLineWithInvalidProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var client = data.Org1;

			var receive = Helper.CreateWhsReceive(client, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m);
			Factory.Save();

			receiveLine2.WI_OP = ZGuid.Missing;
			receive.RunPreSaveValidation();
			receiveLine2.HasChanges = false;
			receiveLine2.InDocketLine.HasChanges = false;
			receive.ReceiveProductSummaryCollection.HasChanges = false;
			receive.HasChanges = false;

			AssertNoExceptionThrown(() => receive.PopulateASNLines());
			AssertEquals(1, receive.AsnLines.Count);
		}

		#endregion

		#region Generate Serial Numbers

		#region TestGenerateSerialNumbers_NoReceiveLineValidationError

		public void TestGenerateSerialNumbers_NoReceiveLineValidationError()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			line1.WE_SerialNumber = "SN001";

			receive.AllocateLocationsWithMock();

			receive.GenerateSerialNumbers(new[] { line1 });
			AssertEquals("Should be 10 lines", 10, receive.Lines.Count);
			AssertEquals("None of lines should have WI_WD validation error.", false, receive.Inventory.Cast<WhsInventoryView>().Any(i => i.WI_WDInfo.HasErrors()));
		}

		#endregion

		#region TestGenerateSerialNumbers_NoUniqueSerialNumberChecksDuringGeneration

		public void TestGenerateSerialNumbers_NoUniqueSerialNumberChecksDuringGeneration()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			line1.WE_SerialNumber = "SN001";

			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			line2.WE_SerialNumber = "SN005";

			receive.AllocateLocationsWithMock();

			receive.GenerateSerialNumbers(new[] { line1 });
			AssertEquals("Should be 11 lines", 11, receive.Lines.Count);
			AssertEquals("SerialNumber should not have any errors", false, line2.WE_SerialNumberInfo.HasErrors());
			foreach (var line in receive.Lines)
			{
				AssertEquals("SerialNumber has no errors.", false, line.WE_SerialNumberInfo.HasErrors());
			}

			receive.RunPreSaveValidation();
			var sn005Inventories = receive.Inventory.Cast<WhsInventoryView>().Where(i => i.WI_SerialNumber == "SN005");
			foreach (var sn005inv in sn005Inventories)
			{
				Assert("SerialNumber has errors.", sn005inv.InDocketLine.WE_SerialNumberInfo.HasErrors());
			}
		}

		#endregion

		public void TestGenerateSerialNumbers()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine.WE_SerialNumber = "SN001";
			AssertEquals("Should be 1 line", 1, receive.Lines.Count);

			receive.AllocateLocationsWithMock();

			receive.GenerateSerialNumbers(new[] { receiveLine });
			AssertEquals("Should be 5 lines", 5, receive.Lines.Count);
			var receiveLines = receive.Lines;
			Assert(receiveLines.Any(line => line.WE_SerialNumber == "SN001"));
			Assert(receiveLines.Any(line => line.WE_SerialNumber == "SN002"));
			Assert(receiveLines.Any(line => line.WE_SerialNumber == "SN003"));
			Assert(receiveLines.Any(line => line.WE_SerialNumber == "SN004"));
			Assert(receiveLines.Any(line => line.WE_SerialNumber == "SN005"));
		}

		public void TestGenerateSerialNumbers_LongSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine.WE_SerialNumber = "SN1000000000000001";
			AssertEquals("Should be 1 line", 1, receive.Lines.Count);

			receive.AllocateLocationsWithMock();

			receive.GenerateSerialNumbers(new[] { receiveLine });
			AssertEquals("Should be 10 lines", 10, receive.Lines.Count);
			var receiveLines = receive.Lines;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"SN1000000000000001",
					"SN1000000000000002",
					"SN1000000000000003",
					"SN1000000000000004",
					"SN1000000000000005",
					"SN1000000000000006",
					"SN1000000000000007",
					"SN1000000000000008",
					"SN1000000000000009",
					"SN1000000000000010",
				},
				receiveLines.Select(rl => rl.WE_SerialNumber));
		}

		public void TestGenerateSerialNumbers_VeryLongSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine.WE_SerialNumber = "SN1234567891234567891234567891234567891234567891";
			AssertEquals("Should be 1 line", 1, receive.Lines.Count);

			receive.AllocateLocationsWithMock();

			receive.GenerateSerialNumbers(new[] { receiveLine });
			AssertEquals("Should be 10 lines", 10, receive.Lines.Count);
			var receiveLines = receive.Lines;

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"SN1234567891234567891234567891234567891234567891",
					"SN1234567891234567891234567891234567891234567892",
					"SN1234567891234567891234567891234567891234567893",
					"SN1234567891234567891234567891234567891234567894",
					"SN1234567891234567891234567891234567891234567895",
					"SN1234567891234567891234567891234567891234567896",
					"SN1234567891234567891234567891234567891234567897",
					"SN1234567891234567891234567891234567891234567898",
					"SN1234567891234567891234567891234567891234567899",
					"SN1234567891234567891234567891234567891234567900",
				},
				receiveLines.Select(rl => rl.WE_SerialNumber));
		}

		public void TestGenerateSerialNumbers_VeryLongSerialNumber_IncrementingByNumberLargerThanALong()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine.WE_SerialNumber = "SN1234567891234567890999999999999999999999999999";
			AssertEquals("Should be 1 line", 1, receive.Lines.Count);

			receive.AllocateLocationsWithMock();

			receive.GenerateSerialNumbers(new[] { receiveLine });
			AssertEquals("Should be 10 lines", 10, receive.Lines.Count);
			var receiveLines = receive.Lines;
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"SN1234567891234567890999999999999999999999999999",
					"SN1234567891234567891000000000000000000000000000",
					"SN1234567891234567891000000000000000000000000001",
					"SN1234567891234567891000000000000000000000000002",
					"SN1234567891234567891000000000000000000000000003",
					"SN1234567891234567891000000000000000000000000004",
					"SN1234567891234567891000000000000000000000000005",
					"SN1234567891234567891000000000000000000000000006",
					"SN1234567891234567891000000000000000000000000007",
					"SN1234567891234567891000000000000000000000000008",
				},
				receiveLines.Select(rl => rl.WE_SerialNumber));
		}

		public void TestGenerateSerialNumbers_SerialNumberNotUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine.WE_SerialNumber = "SN001";
			AssertEquals("Should be 1 line", 1, receive.Lines.Count);

			receive.AllocateLocationsWithMock();

			receive.GenerateSerialNumbers(new[] { receiveLine });
			AssertEquals("Should have not generated new lines since serial number attribute is not used.", 1, receive.Lines.Count);
		}

		public void TestGenerateSerialNumbersDoesNotExecuteIfFinalisedOrCancelled()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(2, 0, 0, 0, 0);

			var receive = data.Receive11;
			receive.Lines[0].WE_PartAttrib1 = "S123";
			receive.Client.PartAttributeManager.SetProductToUseAttribute(data.Part1, 6, true);
			receive.Client.MiscServ.OM_IMUseSerialNumber = true;

			var selected = new List<WhsReceiveLine>();
			selected.AddRange(receive.Lines.Cast<WhsReceiveLine>());
			receive.GenerateSerialNumbers(selected);
			AssertEquals("Should not of generated new lines because the docket is finalised", 1, receive.Lines.Count);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));

			((NotificationBuffer)receive.NotificationManager.Peek).Clear();
			receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			receive.GenerateSerialNumbers(selected);
			AssertEquals("Should not of generated new lines because the docket is cancelled", 1, receive.Lines.Count);
			AssertEquals(true, ((NotificationBuffer)receive.NotificationManager.Peek).ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestGenerateSerialNumbersChecksArgument()
		{
			Receive.GenerateSerialNumbers(null);
		}

		public void TestGenerateSerialNumbers_RecalculatesPackQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 5m);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, finalise: false);
			var line = receive.Lines[0];
			line.WE_F3_NKPackType = Constants.PkgUnit.Pallet;
			line.WE_SerialNumber = "1";
			AssertEquals("Precondition: Qty.", 25m, line.WE_TransactionQuantity);
			AssertEquals("Precondition: Pack Qty.", 5m, line.WE_PackQuantity);

			receive.GenerateSerialNumbers(receive.Lines.Cast<WhsReceiveLine>().ToArray());
			AssertEquals("Should have generated new lines.", 25, receive.Lines.Count);
			AssertEquals("Each line should be 0.2 of a pallet.", true, receive.Lines.All(l => l.WE_PackQuantity == 0.20m && l.WE_F3_NKPackType == Constants.PkgUnit.Pallet));
		}

		#endregion

		#region TestClearLocations

		public void TestClearLocations()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(10m, 20m, 30m, 40m, 50m, false);
			AssertEquals("Precondition - location should be allocated", true, data.Line111.WI_WL.IsValid);

			var selected = new List<WhsReceiveLine>();
			selected.AddRange(new[] { (WhsReceiveLine)data.Line112.InDocketLine, (WhsReceiveLine)data.Line113.InDocketLine, (WhsReceiveLine)data.Line114.InDocketLine });

			data.Receive11.ClearLocations(selected);
			AssertEquals("Should not be cleared", true, data.Line111.WI_WL.IsValid);
			AssertEquals("Should be cleared", true, data.Line112.WI_WL.IsEmpty);
			AssertEquals("Should be cleared", true, data.Line113.WI_WL.IsEmpty);
			AssertEquals("Should be cleared", true, data.Line114.WI_WL.IsEmpty);
			AssertEquals("Should not be cleared", true, data.Line115.WI_WL.IsValid);
		}

		public void TestClearLocations_PutawayTransfers()
		{
			AssertPerformActionOnSelectedInventory_PutawayTransfersReceive((receive, inventory) => receive.ClearLocations(inventory));
		}

		#endregion

		#region AssertPerformActionOnSelectedInventory_PutawayTransfers

		void AssertPerformActionOnSelectedInventory_PutawayTransfers(Action<WhsReceive, WhsInventoryView[]> actionToPerform)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			Helper.CreateProductUnit(data.Part1, "PLT", 5);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
			var receivedInventoryLine = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "A", 10m);
			var puttingAwayInventoryLine = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "B", 10m);
			var putawayInventoryLine = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "C", 10m);
			Factory.Save();

			var transferForPalletB = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transferForPalletB.WD_IsPutawayTransfer = true;
			var transferLineForPalletB = Helper.SetupTransferLineForDockDoorLocation(transferForPalletB, data.Part1, dockDoorLocation, nonDockDoorLocation, "B", 10m);
			transferForPalletB.RunPreSaveValidation();
			transferLineForPalletB.PickedTime = ZDateTimeOffset.Now;

			var transferForPalletC = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transferForPalletC.WD_IsPutawayTransfer = true;
			var transferLineForPalletC = Helper.SetupTransferLineForDockDoorLocation(transferForPalletC, data.Part1, dockDoorLocation, nonDockDoorLocation, "C", 10m);

			transferForPalletC.RunPreSaveValidation();
			transferForPalletC.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, transferForPalletC.IsFinalised);

			AssertEquals("Precondition", InventoryStatus.Codes.Received, receivedInventoryLine.OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.PuttingAway, transferLineForPalletB.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Putaway, transferLineForPalletC.WE_OriginalInventoryStatus);

			actionToPerform(receive, (new[] { receivedInventoryLine, puttingAwayInventoryLine, putawayInventoryLine }));
			Assert(Notify.ContainsNotificationType(ReceiveErrorTypes.ContainsReceiveLinesWithPutawayTransfers));
			Notify.Clear();

			actionToPerform(receive, new[] { puttingAwayInventoryLine, putawayInventoryLine });
			Assert(Notify.ContainsNotificationType(ReceiveErrorTypes.ContainsReceiveLinesWithPutawayTransfers));
			Notify.Clear();

			actionToPerform(receive, new[] { putawayInventoryLine });
			Assert(Notify.ContainsNotificationType(ReceiveErrorTypes.ContainsReceiveLinesWithPutawayTransfers));
			Notify.Clear();

			actionToPerform(receive, new[] { receivedInventoryLine });
			Assert(!Notify.ContainsNotificationType(ReceiveErrorTypes.ContainsReceiveLinesWithPutawayTransfers));
		}

		void AssertPerformActionOnSelectedInventory_PutawayTransfersReceive(Action<WhsReceive, WhsReceiveLine[]> actionToPerform)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			Helper.CreateProductUnit(data.Part1, "PLT", 5);
			var ddlLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = ddlLocationType.PK; // dock door location

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
			var receivedInventoryLine = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "A", 10m).InDocketLine as WhsReceiveLine;
			var puttingAwayInventoryLine = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "B", 10m).InDocketLine as WhsReceiveLine;
			var putawayInventoryLine = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "C", 10m).InDocketLine as WhsReceiveLine;
			Factory.Save();

			var transferForPalletB = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transferForPalletB.WD_IsPutawayTransfer = true;
			var transferLineForPalletB = Helper.SetupTransferLineForDockDoorLocation(transferForPalletB, data.Part1, dockDoorLocation, nonDockDoorLocation, "B", 10m);
			transferForPalletB.RunPreSaveValidation();
			transferLineForPalletB.PickedTime = ZDateTimeOffset.Now;

			var transferForPalletC = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transferForPalletC.WD_IsPutawayTransfer = true;
			var transferLineForPalletC = Helper.SetupTransferLineForDockDoorLocation(transferForPalletC, data.Part1, dockDoorLocation, nonDockDoorLocation, "C", 10m);

			transferForPalletC.RunPreSaveValidation();
			transferForPalletC.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, transferForPalletC.IsFinalised);

			actionToPerform(receive, (new[] { receivedInventoryLine, puttingAwayInventoryLine, putawayInventoryLine }));
			Assert(Notify.ContainsNotificationType(ReceiveErrorTypes.ContainsReceiveLinesWithPutawayTransfers));
			Notify.Clear();

			actionToPerform(receive, new[] { puttingAwayInventoryLine, putawayInventoryLine });
			Assert(Notify.ContainsNotificationType(ReceiveErrorTypes.ContainsReceiveLinesWithPutawayTransfers));
			Notify.Clear();

			actionToPerform(receive, new[] { putawayInventoryLine });
			Assert(Notify.ContainsNotificationType(ReceiveErrorTypes.ContainsReceiveLinesWithPutawayTransfers));
			Notify.Clear();

			actionToPerform(receive, new[] { receivedInventoryLine });
			Assert(!Notify.ContainsNotificationType(ReceiveErrorTypes.ContainsReceiveLinesWithPutawayTransfers));
		}

		void AssertPerformActionOnReceive(Action<WhsReceive> actionToPerform)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateProductUnit(data.Part1, "PLT", 5);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
			var receivedInventoryLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "A");
			var puttingAwayInventoryLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "B");
			var putawayInventoryLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "C");
			Factory.Save();

			var transferForPalletB = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transferForPalletB.WD_IsPutawayTransfer = true;
			var transferLineForPalletB = Helper.SetupTransferLineForDockDoorLocation(transferForPalletB, data.Part1, dockDoorLocation, nonDockDoorLocation, "B", 10m);
			transferForPalletB.RunPreSaveValidation();
			transferLineForPalletB.PickedTime = ZDateTimeOffset.Now;

			var transferForPalletC = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transferForPalletC.WD_IsPutawayTransfer = true;
			var transferLineForPalletC = Helper.SetupTransferLineForDockDoorLocation(transferForPalletC, data.Part1, dockDoorLocation, nonDockDoorLocation, "C", 10m);
			transferForPalletC.RunPreSaveValidation();
			transferForPalletC.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, transferLineForPalletC.IsFinalised);

			AssertEquals("Precondition", InventoryStatus.Codes.Received, receivedInventoryLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.PuttingAway, transferLineForPalletB.WE_OriginalInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Putaway, transferLineForPalletC.WE_OriginalInventoryStatus);

			actionToPerform(receive);
			Assert(Notify.ContainsNotificationType(ReceiveErrorTypes.ContainsReceiveLinesWithPutawayTransfers));
			Notify.Clear();
		}

		#endregion

		#region TestSetUnloadCompleteTimeAndCloseOrCancelActiveUnloadTasksIfPossible

		public void TestSetUnloadCompleteTimeAndCloseOrCancelActiveUnloadTasksIfPossible()
			=> TestSetUnloadCompleteTimeAndCloseOrCancelActiveUnloadTasksIfPossibleCore(isPlannedReceive: false);

		public void TestSetUnloadCompleteTimeAndCloseOrCancelActiveUnloadTasksIfPossible_PlannedReceive()
			=> TestSetUnloadCompleteTimeAndCloseOrCancelActiveUnloadTasksIfPossibleCore(isPlannedReceive: true);

		void TestSetUnloadCompleteTimeAndCloseOrCancelActiveUnloadTasksIfPossibleCore(bool isPlannedReceive)
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			if (isPlannedReceive)
			{
				receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			}
			Factory.Save();

			var task1 = Helper.CreateProcessTaskForReceive(receive, staff);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			var task2 = Helper.CreateProcessTaskForReceive(receive, staff);
			var task3 = Helper.CreateProcessTaskForReceive(receive);
			var task4 = Helper.CreateProcessTaskForReceive(receive, staff);
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var task5 = Helper.CreateProcessTaskForReceive(receive, staff);
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			var task6 = Helper.CreateProcessTaskForReceive(receive, staff);
			task6.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();

			AssertEquals("Precondition", isPlannedReceive, receive.WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned));
			AssertEquals("Precondition:", false, receive.WD_UnloadCompletedTime.IsValid);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Working, task1.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task2.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Open, task3.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Closed, task4.P9_Status);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Cancelled, task5.P9_Status);

			receive.SetUnloadCompleteTimeAndCloseOrCancelActiveUnloadTasksIfPossible();

			AssertEquals(true, receive.WD_UnloadCompletedTime.IsValid);
			AssertEquals(isPlannedReceive ? ProcessTaskStatusCodeList.Codes.Closed : ProcessTaskStatusCodeList.Codes.Working, task1.P9_Status);
			AssertEquals(isPlannedReceive ? ProcessTaskStatusCodeList.Codes.Closed : ProcessTaskStatusCodeList.Codes.Suspended, task6.P9_Status);
			AssertEquals(isPlannedReceive ? ProcessTaskStatusCodeList.Codes.Cancelled : ProcessTaskStatusCodeList.Codes.Assigned, task2.P9_Status);
			AssertEquals(isPlannedReceive ? ProcessTaskStatusCodeList.Codes.Cancelled : ProcessTaskStatusCodeList.Codes.Open, task3.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task4.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task5.P9_Status);
		}

		public void TestSetUnloadCompleteTimeAndCloseOrCancelActiveUnloadTasksIfPossible_PlannedReceiveWithWorkingTask_UserCancels()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			AssertEquals("Precondition", true, receive.WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned));
			AssertEquals("Precondition:", false, receive.WD_UnloadCompletedTime.IsValid);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);

			var closeUnloadTasksConfirmationMessage = "This Receipt has active unload job tasks set to working. The tasks need to be completed to complete the unload for this Receipt.\r\n\r\nDo you wish to close these active tasks set to working to complete the unload for this Receipt?";
			Notify.PreQueryUser += (sender, e) =>
			{
				if (e is DefaultableQueryUserEventArgs args)
				{
					args.Response = !args.Message.Equals(closeUnloadTasksConfirmationMessage);
				}
			};

			receive.SetUnloadCompleteTimeAndCloseOrCancelActiveUnloadTasksIfPossible();

			AssertEquals(false, receive.WD_UnloadCompletedTime.IsValid);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
		}

		#endregion

		#endregion

		#region Update Actual Quantity with Expected Quantity

		#region TestUpdateActualQuantityWithExpectedQuantity

		public void TestUpdateActualQuantityWithExpectedQuantity()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(50m, 25m, 10m, 0m, 0m, false);
			Factory.Save(); // required to ensure WI_InDocketLineUnits update will not affect WI_ExpectedReceiptQuantity.

			data.Line111.WI_InDocketLineUnits = 0m;
			data.Line112.WI_InDocketLineUnits = 1m;
			data.Line113.WI_InDocketLineUnits = 0m;
			AssertEquals("Precondition", 50m, data.Line111.WI_ExpectedReceiptQuantity);
			AssertEquals("Precondition", 25m, data.Line112.WI_ExpectedReceiptQuantity);
			AssertEquals("Precondition", 10m, data.Line113.WI_ExpectedReceiptQuantity);

			var selectedReceiveLines = new List<WhsReceiveLine>();
			selectedReceiveLines.Add((WhsReceiveLine)data.Line111.InDocketLine);
			selectedReceiveLines.Add((WhsReceiveLine)data.Line112.InDocketLine);

			data.Receive11.UpdateActualQuantityWithExpectedQuantity(selectedReceiveLines);
			AssertEquals("Actual Quantity should be updated from Expected Quantity.", 50m, data.Line111.WI_InDocketLineUnits);
			AssertEquals("Actual Quantity should not be updated from Expected Quantity, because it was not 0.", 1m, data.Line112.WI_InDocketLineUnits);
			AssertEquals("Actual Quantity should not be updated from Expected Quantity.", 0m, data.Line113.WI_InDocketLineUnits);
		}

		#endregion

		#region TestUpdateActualQuantityWithExpectedQuantity_DoesNotUpdateIfFinalisedOrCancelled

		public void TestUpdateActualQuantityWithExpectedQuantity_DoesNotUpdateIfFinalisedOrCancelled()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			Factory.Save(); // required to ensure WI_InDocketLineUnits update will not affect WI_ExpectedReceiptQuantity.

			data.Line111.WI_InDocketLineUnits = 0m;
			AssertEquals("Precondition", 100m, data.Line111.WI_ExpectedReceiptQuantity);

			data.Receive11.FinaliseDocket();
			AssertIsFinalisedPrecondition(data.Receive11);

			var notificationBuffer = (NotificationBuffer)data.Receive11.NotificationManager.Peek;

			data.Receive11.UpdateActualQuantityWithExpectedQuantity(data.Receive11.Lines.Cast<WhsReceiveLine>());
			AssertEquals("Should not update actual quantity with expected quantity because the receive is finalised.", 0m, data.Line111.WI_InDocketLineUnits);
			AssertEquals(true, notificationBuffer.ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));

			notificationBuffer.Clear();
			data.Receive11.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			data.Receive11.UpdateActualQuantityWithExpectedQuantity(data.Receive11.Lines.Cast<WhsReceiveLine>());
			AssertEquals("Should not update actual quantity with expected quantity because the receive is canceled.", 0m, data.Line111.WI_InDocketLineUnits);
			AssertEquals(true, notificationBuffer.ContainsNotificationType(WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
		}

		#endregion

		#region TestUpdateActualQuantityWithExpectedQuantity_ChecksArgument

		[ExpectException(typeof(ArgumentNullException))]
		public void TestUpdateActualQuantityWithExpectedQuantity_ChecksArgument()
		{
			Receive.UpdateActualQuantityWithExpectedQuantity(null);
		}

		#endregion

		#region TestUpdateActualQuantityWithExpectedQuantity_PutawayTransfers

		public void TestUpdateActualQuantityWithExpectedQuantity_PutawayTransfers()
		{
			AssertPerformActionOnSelectedInventory_PutawayTransfersReceive((receive, receiveLines) => receive.UpdateActualQuantityWithExpectedQuantity(receiveLines));
		}

		#endregion

		#endregion

		#region Finalisation

		public void TestFinaliseDocket_ShouldFailWithEmptySerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation, ZDate.Empty, ZDate.Empty, "", "", "", "").InDocketLine;
			receive.FinaliseDocket();
			AssertEquals("Receive should NOT be finalised without specifying a serial number.", false, receive.IsFinalised);
			AssertHasError(receiveLine.WE_SerialNumberInfo, "Please enter a Serial Number.");
		}

		public void TestFinaliseDocket_ReconcileASNIsCalled()
		{
			var receive = SetupForTestFinaliseDocket();
			AssertEquals(false, receive.ReconcileASNHasBeenCalled_TestsOnly);

			receive.FinaliseDocket();

			AssertEquals(true, receive.IsFinalised);
			AssertEquals(true, receive.ReconcileASNHasBeenCalled_TestsOnly);
		}

		public void TestFinaliseDocket_ReconcileASN_PalletIDsEnforced()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);

			var whsClientParams1 = Factory.New<WhsClientParameterByWarehouse>();
			whsClientParams1.WY_OH_Client = data.Org1.PK;
			whsClientParams1.WY_WW_Whs = data.Whs1.PK;
			whsClientParams1.WY_ReceiveCategory = ZString.Empty;
			whsClientParams1.WY_EnforcePalletIDEntry = true;

			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			Factory.Save();

			receive.PopulateASNLines();
			AssertEquals("Precondition: Has ASNs.", 1, receive.AsnLines.Count);
			AssertEquals("Precondition: Has ASN with empty pallet id.", string.Empty, receive.AsnLines[0].WN_PalletId);

			inventory.WI_OP = data.Part2.PK;
			inventory.WI_PalletID = "ABC";

			AssertEquals("Precondition.", false, receive.ReconcileASNHasBeenCalled_TestsOnly);
			receive.FinaliseDocket();

			AssertEquals("Precondition: Reconciled ASNs.", true, receive.ReconcileASNHasBeenCalled_TestsOnly);
			AssertEquals("Precondition: Finalized.", true, receive.IsFinalised);

			var underLine = receive.Lines.Single(l => l.WE_OP == data.Part1.PK && l.WE_TransactionQuantity == 0m);
			AssertNotNull("Precondition: Created under line.", underLine);
			AssertNoErrors("Should not show an error when pallet ID is not empty.", inventory.InDocketLine.WE_PalletIDInfo);
			AssertNoErrors("Should not show an error when pallet ID is empty and EnforcePalletIDs property is true, but quantity is 0.", underLine.WE_PalletIDInfo);
		}

		public void TestFinaliseDocket_ValidatesAllInventoryLinesHaveLocationSet()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(1, 2, 3, 4, 5, false);

			data.Line113.LocationString = "";
			Docket = data.Receive11;

			Docket.FinaliseDocket();
			AssertEquals("Should not be finalised because validation should of picked up an inventory line with no location", false, Docket.IsFinalised);
			AssertHasError(data.Line113.InDocketLine.WE_WLInfo, "Please enter a valid location.");

			// check that RowNotification has been cleared from the previous Run
			data.Line113.WI_WL = data.Whs1.DefaultLocation.PK;
			Docket.FinaliseDocket();
			AssertEquals("The Receive should be Finalised after error is corrected", true, Docket.IsFinalised);
			AssertNoErrors(data.Line113.InDocketLine.WE_WLInfo);
		}

		public void TestFinaliseDocket_ValidatesDifferentAreaTypes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var area1 = Helper.CreateArea(data.Whs1, "AREA 1", AreaTypes.Codes.Bonded);
			var area2 = Helper.CreateArea(data.Whs1, "AREA 2", AreaTypes.Codes.Excise);
			location1.WLV_WA_PutawayArea = area1.PK;
			location2.WLV_WA_PutawayArea = area2.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1).InDocketLine;
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location2).InDocketLine;
			receiveLine1.CustomsData.WB_EntryKey = "ABC";
			receiveLine2.CustomsData.WB_EntryKey = "ABC";
			Factory.Save();

			receive.FinaliseDocket();
			AssertEquals("Should not be finalised because validation should of picked up mulitple areas", false, receive.IsFinalised);
			AssertHasRowError(receive, WhsReceive.CannotPutawayIntoDifferentAreaTypesErrorMsg);

			location2.WLV_WA_PutawayArea = area1.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);

			receiveInNewFactory.FinaliseDocketWithoutUserConfirmation();
			AssertNoRowErrors(receiveInNewFactory);
			AssertEquals(true, receiveInNewFactory.IsFinalised);
		}

		public void TestFinaliseDocket_ValidatesDifferentAreaTypes_EmptyLocationsNotIncluded()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 0m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			Factory.Save();

			AssertEquals("Precondition", false, receive.HasErrors);
			receive.FinaliseDocketWithoutUserConfirmation();

			AssertEquals("Receive is finalised.", true, receive.IsFinalised);
			AssertEquals("Receive has no errors.", false, receive.HasErrors);
		}

		public void TestFinaliseDocket_ValidatesDifferentAreaTypes_AllLinesHaveNoLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 0m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 0m);
			Factory.Save();

			AssertEquals("Precondition", false, receive.HasErrors);
			receive.FinaliseDocketWithoutUserConfirmation();

			AssertEquals("Receive is finalised.", true, receive.IsFinalised);
			AssertEquals("Receive has no errors.", false, receive.HasErrors);
		}

		public void TestFinaliseDocket_InventoryStatuses()
		{
			var receive = SetupForTestFinaliseDocket();
			receive.Inventory[0].OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			receive.Inventory[1].OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			receive.Inventory[2].OriginalInventoryStatus = CodeLists.InventoryStatus.Codes.Putaway;

			AssertEquals("Precondition - Doesnt become held until finalised.", true, receive.Inventory[0].OriginalInventoryStatus != InventoryStatus.Codes.Held);
			AssertEquals("Precondition - Doesnt become held until finalised.", true, receive.Inventory[1].OriginalInventoryStatus != InventoryStatus.Codes.Held);
			receive.FinaliseDocket();

			AssertEquals(true, receive.IsFinalised);

			AssertEquals(CodeLists.InventoryStatus.Codes.Held, receive.Inventory[0].WI_InventoryStatus);
			AssertEquals(CodeLists.InventoryStatus.Codes.Held, receive.Inventory[0].InDocketLine.WE_OriginalInventoryStatus);
			AssertEquals(InventoryHoldCodes.Codes.Damaged, receive.Inventory[0].InDocketLine.WE_WHC_NKOriginalInventoryHeldCode);

			AssertEquals(CodeLists.InventoryStatus.Codes.Held, receive.Inventory[1].WI_InventoryStatus);
			AssertEquals(CodeLists.InventoryStatus.Codes.Held, receive.Inventory[1].InDocketLine.WE_OriginalInventoryStatus);
			AssertEquals(InventoryHoldCodes.Codes.Held, receive.Inventory[1].InDocketLine.WE_WHC_NKOriginalInventoryHeldCode);

			AssertEquals(CodeLists.InventoryStatus.Codes.Available, receive.Inventory[2].WI_InventoryStatus);
			AssertEquals(CodeLists.InventoryStatus.Codes.Available, receive.Inventory[2].InDocketLine.WE_OriginalInventoryStatus);
			AssertEquals(ZString.Empty, receive.Inventory[2].InDocketLine.WE_WHC_NKOriginalInventoryHeldCode);
		}

		public void TestFinaliseDocket_WithCorrectAttributesValidation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true); // part attrib 1
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true); // expiry date
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Today, ZDate.Empty, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			Factory.Save();

			receive.PopulateASNLines();
			receive.Inventory.RemoveAndDeleteAll();
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.RunPreSaveValidation();

			AssertEquals("Precondition - correct number of AsnLines were created", 2, receive.AsnLines.Count);
			AssertEquals("Precondition - correct number of Inventory Lines left", 1, receive.Inventory.Count);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Receive should have correct number of Inventory Lines", 3, receive.Inventory.Count);
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
		}

		public void TestFinaliseDocket_WithCurrentHoldReason()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receiveLine.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			receiveLine.OriginalHoldReason = "Whatever";
			Factory.Save();

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			AssertEquals("Original Hold Reason should be set.", "Whatever", receiveLine.OriginalHoldReason);
			AssertEquals("Current Hold Reason should be set.", "Whatever", receiveLine.WE_CurrentHoldReason);
			Factory.Save();

			receiveLine.HeldCodeChangeQuantity = receiveLine.WE_StockOnHand;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			AssertEquals("Original Hold Reason should not change.", "Whatever", receiveLine.OriginalHoldReason);
			AssertEquals("Current Hold Reason should be empty.", "", receiveLine.WE_CurrentHoldReason);
		}

		public void TestFinaliseDocket_ReceiveLinesWithPutawayTransferLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoor = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoor = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoor, "PLT1");
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoor, nonDockDoor, "PLT1", 10m);
			putawayTransfer.RunPreSaveValidation();

			AssertNotNull("Precondition: Putaway transfer has been created.", putawayTransferLine);
			AssertEquals("Precondition: Receive line's inventory status is Received.", InventoryStatus.Codes.Received, receiveLine.WE_OriginalInventoryStatus);

			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;
			putawayTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertIsFinalisedPrecondition(putawayTransfer);
			AssertEquals("Precondition: Received receive line's inventory status is still Received.", InventoryStatus.Codes.Received, receiveLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Putaway transfer line's original inventory status is Putaway.", InventoryStatus.Codes.Putaway, putawayTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Putaway transfer line's current inventory status is Putaway.", InventoryStatus.Codes.Putaway, putawayTransferLine.WE_CurrentInventoryStatus);

			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertEquals("Receive is finalised.", true, receive.IsFinalised);
			AssertEquals("Receive line's inventory status is still received.", InventoryStatus.Codes.Received, receiveLine.WE_OriginalInventoryStatus);
			AssertEquals("Putaway transfer line's original inventory status is putaway.", InventoryStatus.Codes.Putaway, putawayTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Putaway transfer line's current inventory status is available.", InventoryStatus.Codes.Available, putawayTransferLine.WE_CurrentInventoryStatus);
		}

		public void TestFinaliseDocket_ReceiveLinesWithPutawayTransferLines_WithHeldCode()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoor = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoor = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventoryLine = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoor, "PLT1", 10m);
			inventoryLine.OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = putawayTransfer.Lines.AddNew();
			putawayTransferLine.SetDocketLineFromInventory(inventoryLine.InDocketLine, ExcludeFromCopy.None);
			putawayTransferLine.WE_WL_TransferFrom = dockDoor.PK;
			putawayTransferLine.WE_WL = nonDockDoor.PK;
			putawayTransfer.RunPreSaveValidation();

			AssertNotNull("Precondition: Putaway transfer has been created.", putawayTransferLine);
			AssertEquals("Precondition: Receive line's inventory status is Received.", InventoryStatus.Codes.Received, inventoryLine.WI_InventoryStatus);

			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;
			putawayTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertIsFinalisedPrecondition(putawayTransfer);
			AssertEquals("Precondition: Received receive line's inventory status is still Received.", InventoryStatus.Codes.Received, inventoryLine.WI_InventoryStatus);
			AssertEquals("Precondition: Putaway transfer line's original inventory status is Putaway.", InventoryStatus.Codes.Putaway, putawayTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Putaway transfer line's current inventory status is Putaway.", InventoryStatus.Codes.Putaway, putawayTransferLine.WE_CurrentInventoryStatus);

			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertEquals("Receive is finalised.", true, receive.IsFinalised);
			AssertEquals("Receive line's inventory status is still received.", InventoryStatus.Codes.Received, inventoryLine.WI_InventoryStatus);
			AssertEquals("Putaway transfer line's original inventory status is putaway.", InventoryStatus.Codes.Putaway, putawayTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Putaway transfer line's current inventory status is available.", InventoryStatus.Codes.Held, putawayTransferLine.WE_CurrentInventoryStatus);
		}

		public void TestFinaliseReceiveWithInvalidPutawayTransferLines_AVL()
		{
			TestFinaliseReceiveWithInvalidPutawayTransferLines_Core(originalInventoryIsDamaged: false);
		}

		public void TestFinaliseReceiveWithInvalidPutawayTransferLines_HeldCode()
		{
			TestFinaliseReceiveWithInvalidPutawayTransferLines_Core(originalInventoryIsDamaged: true);
		}

		void TestFinaliseReceiveWithInvalidPutawayTransferLines_Core(bool originalInventoryIsDamaged)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoor = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoor = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventoryLine = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoor, "PLT1", 10m);
			if (originalInventoryIsDamaged)
			{
				inventoryLine.OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;
			}
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = putawayTransfer.Lines.AddNew();
			putawayTransferLine.SetDocketLineFromInventory(inventoryLine.InDocketLine, ExcludeFromCopy.None);
			putawayTransferLine.WE_WL_TransferFrom = dockDoor.PK;
			putawayTransferLine.WE_WL = nonDockDoor.PK;
			putawayTransfer.RunPreSaveValidation();

			AssertNotNull("Precondition: Putaway transfer has been created.", putawayTransferLine);
			AssertEquals("Precondition: Receive line's inventory status is Received.", InventoryStatus.Codes.Received, inventoryLine.WI_InventoryStatus);

			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;
			putawayTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertIsFinalisedPrecondition(putawayTransfer);
			AssertEquals("Precondition: Received receive line's inventory status is still Received.", InventoryStatus.Codes.Received, inventoryLine.WI_InventoryStatus);
			AssertEquals("Precondition: Putaway transfer line's original inventory status is Putaway.", InventoryStatus.Codes.Putaway, putawayTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Putaway transfer line's current inventory status is Putaway.", InventoryStatus.Codes.Putaway, putawayTransferLine.WE_CurrentInventoryStatus);

			receive.FinaliseDocketWithoutUserConfirmation();
			var validStatus = new List<string>() { "AVL", "HEL" };
			foreach (var statusCode in new InventoryStatus().ToArray().Select(l => l.Code).Except(validStatus).ToArray())
			{
				putawayTransferLine.WE_CurrentInventoryStatus = statusCode;
				Helper.AssertZCannotSaveExceptionThrown("Cannot save as putaway transfer lines have invalid inventory status.", Factory.Save);
			}
		}

		public void TestFinaliseDocket_CheckAllPutawayTransferLinesHasValidInventoryStatus_MatchingLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoor = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoor = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoor, "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, dockDoor, "PLT1");
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoor, nonDockDoor, "PLT1", 15m);
			putawayTransfer.RunPreSaveValidation();

			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;
			putawayTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var matchingLine = putawayTransferLine.MatchingLines.Single();
			AssertIsFinalisedPrecondition(putawayTransfer);
			AssertEquals("Precondition: Received receive line's inventory status is still Received.", InventoryStatus.Codes.Received, receiveLine1.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Received receive line's inventory status is still Received.", InventoryStatus.Codes.Received, receiveLine2.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Putaway transfer matching line's original inventory status is Putaway.", InventoryStatus.Codes.Putaway, putawayTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Putaway transfer matching line's current inventory status is Putaway.", InventoryStatus.Codes.Putaway, putawayTransferLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: Putaway transfer matching line's original inventory status is Putaway.", InventoryStatus.Codes.Putaway, matchingLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Putaway transfer matching line's current inventory status is Putaway.", InventoryStatus.Codes.Putaway, matchingLine.WE_CurrentInventoryStatus);

			receive.FinaliseDocketWithoutUserConfirmation();
			var validFinaliseStatus = new List<string>() { "AVL", "HEL" };
			var originalValue = matchingLine.WE_CurrentInventoryStatusInfo.OriginalValue.ToString();
			AssertCollectionContains("Precondition: Expect error in next assertion if value is not valid.", originalValue, new InventoryStatus().ToArray().Select(l => l.Code).Except(validFinaliseStatus).ToArray());
			matchingLine.WE_CurrentInventoryStatus = originalValue; // Undo status, change simulate not change to valid status
			Helper.AssertZCannotSaveExceptionThrown("Cannot save as putaway transfer lines have invalid inventory status.", Factory.Save);
		}

		public void TestFinaliseReceiveWithInvalidPutawayTransferLines_NoStatusChange()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoor = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoor = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventoryLine = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoor, "PLT1", 10m);
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = putawayTransfer.Lines.AddNew();
			putawayTransferLine.SetDocketLineFromInventory(inventoryLine.InDocketLine, ExcludeFromCopy.None);
			putawayTransferLine.WE_WL_TransferFrom = dockDoor.PK;
			putawayTransferLine.WE_WL = nonDockDoor.PK;
			putawayTransfer.RunPreSaveValidation();

			AssertNotNull("Precondition: Putaway transfer has been created.", putawayTransferLine);
			AssertEquals("Precondition: Receive line's inventory status is Received.", InventoryStatus.Codes.Received, inventoryLine.WI_InventoryStatus);

			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;
			putawayTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertIsFinalisedPrecondition(putawayTransfer);
			AssertEquals("Precondition: Received receive line's inventory status is still Received.", InventoryStatus.Codes.Received, inventoryLine.WI_InventoryStatus);
			AssertEquals("Precondition: Putaway transfer line's original inventory status is Putaway.", InventoryStatus.Codes.Putaway, putawayTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Putaway transfer line's current inventory status is Putaway.", InventoryStatus.Codes.Putaway, putawayTransferLine.WE_CurrentInventoryStatus);

			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertEquals("Putaway transfer line's current inventory status is Available.", InventoryStatus.Codes.Available, putawayTransferLine.WE_CurrentInventoryStatus);
			Db.Connection.ExecuteNonQuery(string.Format("update dbo.WhsDocketline set WE_CurrentInventoryStatus = '{0}', WE_SystemLastEditTimeUtc = SYSUTCDATETIME(), WE_SystemLastEditUser = '~BP' where WE_PK = '{1}'", InventoryStatus.Codes.Putaway, putawayTransferLine.PK));

			var receiveInNewFactory = NewFactory().Load<WhsReceive>(receive.PK);
			receiveInNewFactory.WD_ExternalReference = "Just to make a change";
			AssertNoExceptionThrown($"Should not check related transfer line's inventory status when we do not change receive's status", receiveInNewFactory.Factory.Save);
		}

		public void TestFinaliseReceiveWithInvalidPutawayTransferLines_ReceiveNotInDB()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoor = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoor = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventoryLine = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoor, "PLT1", 10m);

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = putawayTransfer.Lines.AddNew();
			putawayTransferLine.SetDocketLineFromInventory(inventoryLine.InDocketLine, ExcludeFromCopy.None);
			putawayTransferLine.WE_WL_TransferFrom = dockDoor.PK;
			putawayTransferLine.WE_WL = nonDockDoor.PK;
			putawayTransfer.RunPreSaveValidation();

			AssertNotNull("Precondition: Putaway transfer has been created.", putawayTransferLine);
			AssertEquals("Precondition: Receive line's inventory status is Received.", InventoryStatus.Codes.Received, inventoryLine.WI_InventoryStatus);

			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;
			putawayTransfer.FinaliseDocketWithoutUserConfirmation();

			AssertIsFinalisedPrecondition(putawayTransfer);
			AssertEquals("Precondition: Received receive line's inventory status is still Received.", InventoryStatus.Codes.Received, inventoryLine.WI_InventoryStatus);
			AssertEquals("Precondition: Putaway transfer line's original inventory status is Putaway.", InventoryStatus.Codes.Putaway, putawayTransferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition: Putaway transfer line's current inventory status is Putaway.", InventoryStatus.Codes.Putaway, putawayTransferLine.WE_CurrentInventoryStatus);

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Putaway transfer line's current inventory status is Available.", InventoryStatus.Codes.Available, putawayTransferLine.WE_CurrentInventoryStatus);
			putawayTransferLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Putaway;
			Helper.AssertZCannotSaveExceptionThrown("Cannot save as putaway transfer lines have invalid inventory status.", Factory.Save);
		}

		protected override WhsReceive SetupForTestFinaliseDocket()
		{
			var receive = base.SetupForTestFinaliseDocket();
			Factory.Save();
			//changing the quantities will break tests
			receive.WD_ExternalReference = "1";
			receive.WD_ArrivalDate = ZDateTimeOffset.Now;
			var part = Helper.CreateProduct(receive.Client, "P1");

			receive.Lines.AddNew();
			AddNewInventory(receive.Inventory[0], receive.Client, part, 10);

			receive.Lines.AddNew();
			AddNewInventory(receive.Inventory[1], receive.Client, part, 15);

			receive.Lines.AddNew();
			AddNewInventory(receive.Inventory[2], receive.Client, part, 20);

			// putaway the receive so it can be finalised
			receive.AllocateLocationsWithMock();
			AssertEquals("Receive should be putaway", true, receive.IsPuttingAway);

			return receive;
		}

		protected void AddNewInventory(WhsInventoryView inventory, OrgHeader client, OrgSupplierPart part, ZDecimal units)
		{
			inventory.WI_OH_Client = client.PK;
			inventory.WI_OP = part.PK;
			inventory.WI_InDocketLineUnits = units;
			inventory.WI_InDocketLineType = CodeLists.DocketType.Codes.Receive;
		}

		[TestDate(2005, 1, 20)]
		public void TestFinalisedDateOverride()
		{
			var receive = SetupForTestFinaliseDocket();
			receive.Warehouse.WW_UseArrivalDateForInwardsFinalisedDate = true;
			receive.WD_ArrivalDate = new ZDateTimeOffset(2004, 2, 15);
			receive.FinaliseDocket();
			AssertEquals(2004, receive.WD_FinalisedDate.Year);
			AssertEquals(2, receive.WD_FinalisedDate.Month);
			AssertEquals(15, receive.WD_FinalisedDate.Day);
		}

		public override void TestFinaliseDocket_AbortsAndNotifiesUserIfRunRreFinaliseValidationFails()
		{
			OrgHeader client = Helper.CreateClient();
			WhsWarehouse whs = Helper.CreateWarehouse("1");
			Docket = Helper.CreateWhsReceive(client, whs);
			AssertEquals("Precondition", false, Docket.IsFinalised);

			Docket.FinaliseDocket();

			AssertEquals(false, Docket.IsFinalised);
			NotificationBuffer notify = (NotificationBuffer)Docket.NotificationManager.Peek;
			AssertEquals(true, notify.ContainsNotificationType(WhsErrorTypes.NoLinesEntered));
		}

		#region TestCheckAllPutawayTransferLinesAreFinalised

		public void TestCheckAllPutawayTransferLinesAreFinalised_NeitherPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoor = data.Whs1.FindLocation("A-1");
			var nonDockDoor = data.Whs1.FindLocation("A-2");
			dockDoor.WLV_WLT_LocationType = dockDoorType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoor, "A", 2m);
			var receiveLine = receive.Lines[0];
			receiveLine.LocationString = "A-2";

			Factory.Save();

			AssertNull("The receive line should not have putaway transfer line.", receiveLine.PutawayTransferLine);

			receive.FinaliseDocket();

			AssertEquals("The receive should be successfully finalised since there is no putaway transfer lines.", true, receive.IsFinalised);
			AssertNoRowErrors("The receive line should not have row errors.", receiveLine);
		}

		public void TestCheckAllPutawayTransferLinesAreFinalised_NeitherFinalised()
		{
			TestCheckAllPutawayTransferLinesAreFinalised(false, false, expectedReceiveFinalised: false);
		}

		public void TestCheckAllPutawayTransferLinesAreFinalised_OneFinalisedAndTheOtherNot()
		{
			TestCheckAllPutawayTransferLinesAreFinalised(true, false, expectedReceiveFinalised: false);
		}

		public void TestCheckAllPutawayTransferLinesAreFinalised_BothFinalised()
		{
			TestCheckAllPutawayTransferLinesAreFinalised(true, true, expectedReceiveFinalised: true);
		}

		void TestCheckAllPutawayTransferLinesAreFinalised(bool transferLine1Finalised, bool transferLine2Finalised, bool expectedReceiveFinalised)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoor = data.Whs1.FindLocation("A-1");
			var nonDockDoor = data.Whs1.FindLocation("A-2");
			dockDoor.WLV_WLT_LocationType = dockDoorType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoor, "A", 2m);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoor, "B", 4m);
			Factory.Save();

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer1.WD_IsPutawayTransfer = true;
			var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(transfer1, data.Part1, dockDoor, nonDockDoor, "A", 2m);
			transfer1.RunPreSaveValidation();

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer2.WD_IsPutawayTransfer = true;
			var transferLine2 = Helper.SetupTransferLineForDockDoorLocation(transfer2, data.Part1, dockDoor, nonDockDoor, "B", 4m);
			transfer2.RunPreSaveValidation();

			var receiveLine1 = receive.Lines[0];
			var receiveLine2 = receive.Lines[1];

			if (transferLine1Finalised)
			{
				transfer1.FinaliseDocketWithoutUserConfirmation();
			}

			if (transferLine2Finalised)
			{
				transfer2.FinaliseDocketWithoutUserConfirmation();
			}

			Factory.Save();
			receive.FinaliseDocket();

			AssertEquals("The receive should be successfully finalised since there will be no un-finalised putaway transfer lines.", true, receive.IsFinalised);
			Assert("The 1st transfer should be finalised", transfer1.IsFinalised);
			Assert("The 1st transfer line should be finalised", transferLine1.IsFinalised);
			Assert("The 2nd transfer should be finalised", transfer2.IsFinalised);
			Assert("The 2nd transfer line should be finalised", transferLine2.IsFinalised);
			AssertNoRowErrors("The 1st receive line should not have row errors.", receiveLine1);
			AssertNoRowErrors("The 2nd receive line should not have row errors.", receiveLine2);
		}

		public void TestCheckAllPutawayTransferLinesAreFinalised_TransferFinalised_RowErrorIsClearedAndReceiveIsFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoor = data.Whs1.FindLocation("A-1");
			var nonDockDoor = data.Whs1.FindLocation("A-2");
			dockDoor.WLV_WLT_LocationType = dockDoorType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			var receiveLine = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoor, "A", 2m).InDocketLine as WhsReceiveLine;

			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoor, nonDockDoor, "A", 2m);
			transfer.RunPreSaveValidation();
			Factory.Save();

			transfer.AddRowError("Error here so I cannot finalise.");
			receive.FinaliseDocket();
			AssertHasRowError("Precondition: The receive line should contain error message since transfer is not finalised.", receiveLine, WhsReceive.ContainsUnfinalizedPutawayTransfersErrorMessage);

			transfer.RemoveRowError("Error here so I cannot finalise.");
			transfer.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(transfer);

			receive.FinaliseDocket();
			AssertEquals("Receive is finalised.", true, receive.IsFinalised);
			AssertNoRowError(receiveLine, WhsReceive.ContainsUnfinalizedPutawayTransfersErrorMessage);
		}

		#endregion

		#region TestFinalise_WithMixedReceiveLines

		public void TestFinalise_WithMixedReceiveLines_FRE_DDA()
		{
			TestFinalise_WithMixedReceiveLines(AreaTypes.Codes.FreeStore, AreaTypes.Codes.DockDoor, isCustoms: false, expectedFinalised: true);
		}

		public void TestFinalise_WithMixedReceiveLines_DDA_FRE()
		{
			TestFinalise_WithMixedReceiveLines(AreaTypes.Codes.DockDoor, AreaTypes.Codes.FreeStore, isCustoms: false, expectedFinalised: true);
		}

		public void TestFinalise_WithMixedReceiveLines_EXC_BON()
		{
			TestFinalise_WithMixedReceiveLines(AreaTypes.Codes.Excise, AreaTypes.Codes.Bonded, isCustoms: true, expectedFinalised: false);
		}

		public void TestFinalise_WithMixedReceiveLines_BON_EXC()
		{
			TestFinalise_WithMixedReceiveLines(AreaTypes.Codes.Bonded, AreaTypes.Codes.Excise, isCustoms: true, expectedFinalised: false);
		}

		void TestFinalise_WithMixedReceiveLines(string areaType1, string areaType2, bool isCustoms, bool expectedFinalised)
		{
			// DPF is excluded as it's invalid for PutawayArea type.
			// Bonded/Excise can only be used with customs receipts and vice versa, other combinations will be caught by WhsReceiveLineValidation.LocationInBondAreaValidation.
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var area1 = Helper.CreateArea(data.Whs1, "AREA 1", areaType1);
			area1.WA_IsPickingArea = false;
			area1.WA_IsPutawayArea = true;
			location1.WLV_WA_PutawayArea = area1.PK;

			var area2 = Helper.CreateArea(data.Whs1, "AREA 2", areaType2);
			area2.WA_IsPickingArea = false;
			area2.WA_IsPutawayArea = true;
			location2.WLV_WA_PutawayArea = area2.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1).InDocketLine;
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1).InDocketLine;

			if (isCustoms)
			{
				receive.WD_DocketSubType = ReceiveType.Codes.Customs;
				receiveLine1.CustomsData.WB_EntryKey = "ABC";
				receiveLine2.CustomsData.WB_EntryKey = "ABC";
			}

			Factory.Save();

			using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
			{
				AssertNoRowError(receive, WhsReceive.CannotPutawayIntoDifferentAreaTypesErrorMsg);

				receiveLine2.LocationString = "A-2";
				receive.FinaliseDocketWithoutUserConfirmation();

				if (expectedFinalised)
				{
					AssertNoRowError(receive, WhsReceive.CannotPutawayIntoDifferentAreaTypesErrorMsg);
					AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
				}
				else
				{
					AssertHasRowError("Receive should get error during finalise if inventories are in different area types.", receive, WhsReceive.CannotPutawayIntoDifferentAreaTypesErrorMsg);
					AssertEquals("Receive should not be finalised.", false, receive.IsFinalised);
				}
			}
		}

		#endregion

		#region TestFinaliseDocket_DBHits

		public void TestFinaliseDocket_DBHits()
		{
			var receive = SetupForTestFinaliseDocket();
			receive.Lines[0].WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			otherFactory.RefreshEnabled = false;
			var otherHelper = new WhsTestHelperFunctions(otherFactory);
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			using (RowFactory.SetCachedTables())
			{
				receiveInOtherFactory.RunPreSaveValidation();
				receiveInOtherFactory.Validation.ValidateAll();
			}

			var expectedDBHitsForValidation = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ RefPacksSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsInventoryHeldCodeSchema.Constants.TableName, 1 }, // Due to inventory hold code list validation in docketlines
				{ WhsAsnLineSchema.Constants.TableName, 1 },
			};

			AssertDbHits(expectedDBHitsForValidation, otherFactory);

			var factoryForFinalise = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInFinaliseFactory = factoryForFinalise.Load<WhsReceive>(receive.PK);

			var expectedDBHitsForFinalisation = new Dictionary<string, int>(expectedDBHitsForValidation);
			expectedDBHitsForFinalisation[WhsDocketSchema.Constants.TableName] += 1;
			expectedDBHitsForFinalisation[GlbBranchSchema.Constants.TableName] += 1;

			expectedDBHitsForFinalisation.Add(WhsDocketReferenceSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(WhsDocketPalletSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(ProcessTasksSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(JobServiceSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(ProcessTaskNotificationSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(WhsDocketContainerSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(GlbCompanySchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(WhsClientParameterByWarehouseSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(GlbStaffSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(ProcessWorkflowExceptionTypeSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(JobDocAddressSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(JobHeaderSchema.Constants.TableName, 1);
			expectedDBHitsForFinalisation.Add(ProcessTaskTemplateSchema.Constants.TableName, 1);

			using (RowFactory.SetCachedTables())
			{
				receiveInFinaliseFactory.FinaliseDocketWithoutUserConfirmation();
			}

			AssertEquals(true, receiveInFinaliseFactory.IsFinalised);
			AssertDbHits(expectedDBHitsForFinalisation, factoryForFinalise);
		}

		[TestDate(2025, 06, 02)]
		public void TestFinaliseDocket_DBHits_PlannedReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;

			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "PLT1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "PLT2");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "PLT3");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "PLT4");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoorLocation, "PLT5");
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine1 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "PLT1", 10m);
			var putawayTransferLine2 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "PLT2", 10m);
			var putawayTransferLine3 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "PLT3", 10m);
			var putawayTransferLine4 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "PLT4", 10m);
			var putawayTransferLine5 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "PLT5", 10m);
			putawayTransfer.RunPreSaveValidation();

			AssertNotNull("Precondition: Putaway transfer has been created.", putawayTransferLine1);

			var putawayTask1 = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff);
			putawayTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			var putawayTask2 = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff);
			var putawayTask3 = Helper.CreateProcessTaskForTransfer(putawayTransfer);
			var putawayTask4 = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff);
			putawayTask4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var putawayTask5 = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff);
			putawayTask5.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			putawayTransferLine1.PickedTime = ZDateTimeOffset.Now;
			putawayTransferLine2.PickedTime = ZDateTimeOffset.Now;
			putawayTransferLine3.PickedTime = ZDateTimeOffset.Now;
			putawayTransferLine4.PickedTime = ZDateTimeOffset.Now;
			putawayTransferLine5.PickedTime = ZDateTimeOffset.Now;
			putawayTransfer.FinaliseDocketWithoutUserConfirmation();

			var task1 = Helper.CreateProcessTaskForReceive(receive, staff);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			var task2 = Helper.CreateProcessTaskForReceive(receive, staff);
			var task3 = Helper.CreateProcessTaskForReceive(receive);
			var task4 = Helper.CreateProcessTaskForReceive(receive, staff);
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var task5 = Helper.CreateProcessTaskForReceive(receive, staff);
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			AssertIsFinalisedPrecondition(putawayTransfer);

			var otherFactory = new BusinessObjectFactory();
			otherFactory.RefreshEnabled = false;
			var otherHelper = new WhsTestHelperFunctions(otherFactory);
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			var expectedDBHitsForValidation = new Dictionary<string, int>()
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHitsForValidation, otherFactory))
			{
				receiveInOtherFactory.RunPreSaveValidation();
				receiveInOtherFactory.Validation.ValidateAll();
			}

			var factoryForFinalise = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInFinaliseFactory = factoryForFinalise.Load<WhsReceive>(receive.PK);

			var expectedDBHitsForFinalisation = new Dictionary<string, int>()
			{
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ JobHeaderSchema.Constants.TableName, 1 },
				{ JobServiceSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ ProcessTaskExtraResourceSchema.Constants.TableName, 1 },
				{ ProcessTaskNotificationSchema.Constants.TableName, 1 },
				{ ProcessTaskRequiredSkillSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 5 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ ProcessWorkflowExceptionTypeSchema.Constants.TableName, 1 },
				{ StmALogSchema.Constants.TableName, 2 },
				{ WhsAsnLineSchema.Constants.TableName, 1 },
				{ WhsDocketContainerSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 7 },
				{ WhsDocketPalletSchema.Constants.TableName, 1 },
				{ WhsDocketReferenceSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 3 },
				{ WhsInventoryViewSchema.Constants.TableName, 7 },
				{ WhsLocationViewSchema.Constants.TableName, 3 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsPutawayJobSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHitsForFinalisation, factoryForFinalise))
			{
				receiveInFinaliseFactory.NotificationManager.Push(Notify);
				receiveInFinaliseFactory.FinaliseDocket();
				AssertEquals(true, receiveInFinaliseFactory.IsFinalised);
			}
		}

		#endregion

		#region TestValidateEachLocationOnlyHitDatabaseOnce

		public void TestValidateEachLocationOnlyHitDatabaseOnce_Weight()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			Helper.SetLocationMaxWeightAndVolume(location1, 1m, "KG", 1m, "M3");
			Helper.SetLocationMaxWeightAndVolume(location2, 2m, "KG", 2m, "M3");
			Helper.SetProductWeightAndVolume(data.Part1, 1m, "KG", 0, "M3");
			Factory.Save();

			ValidateEachLocationOnlyHitDatabaseOnceCore(data
					, "Total required Weight (10.00 KG) exceeds the maximum available Weight (1.00 KG) for this location."
					, "Total required Weight (10.00 KG) exceeds the maximum available Weight (2.00 KG) for this location."
					, false);
		}

		public void TestValidateEachLocationOnlyHitDatabaseOnce_Volume()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			Helper.SetLocationMaxWeightAndVolume(location1, 1m, "KG", 1m, "M3");
			Helper.SetLocationMaxWeightAndVolume(location2, 2m, "KG", 2m, "M3");
			Helper.SetProductWeightAndVolume(data.Part1, 0, "KG", 1m, "M3");
			Factory.Save();

			ValidateEachLocationOnlyHitDatabaseOnceCore(data
					, "Total required Volume (10.000 M3) exceeds the maximum available Volume (1.000 M3) for this location."
					, "Total required Volume (10.000 M3) exceeds the maximum available Volume (2.000 M3) for this location."
					, false);
		}

		public void TestValidateEachLocationOnlyHitDatabaseOnce_Quantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.FindLocation("A-1").WLV_MaxQuantity = 5;
			data.Whs1.FindLocation("A-2").WLV_MaxQuantity = 7;
			Factory.Save();

			ValidateEachLocationOnlyHitDatabaseOnceCore(data
					, "Total required Quantity (10) exceeds the maximum available Quantity (5.000) for this location."
					, "Total required Quantity (10) exceeds the maximum available Quantity (7.000) for this location."
					, true);
		}

		void ValidateEachLocationOnlyHitDatabaseOnceCore(TestDataSimpleEnvironment data, string messageLocation1, string messageLocation2, bool isError)
		{
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var org = data.Org1;
			var part = data.Part1;

			var receive = Helper.CreateWhsReceive(org, data.Whs1, "R1");
			for (var i = 0; i < 10; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, part, 1m, location1);
				Helper.CreateWhsReceiveInventoryLine(receive, part, 1m, location2);
			}

			receive.LocationCapacityValidationManager.ClearLocationAvailableCapacityForTest();
			Factory.ResetDatabaseLoadCount();

			foreach (var line in receive.Lines)
			{
				using (line.SuspendValidationTesting())
				{
					line.WE_WLInfo.ClearAllNotifications();
					line.Validation.ValidateWE_WL();

					if (isError)
					{
						AssertHasError(line.WE_WLInfo, line.WE_WL == location1.PK ? messageLocation1 : messageLocation2);
					}
					else
					{
						AssertHasWarning(line.WE_WLInfo, line.WE_WL == location1.PK ? messageLocation1 : messageLocation2);
					}
				}
			}

			// one call per each location
			AssertEquals(1 + 1, Factory.DatabaseLoadCount);
		}

		#endregion

		#region TestFinaliseDocket_FinalisationIsNotAllowed

		public void TestFinaliseDocket_FinalisationIsNotAllowed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var expectedErrorMessage = "Cannot be finalized until Customs send an ACCEPT event.";
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var location = data.Whs1.FindLocation("A-1");
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			// Normal Receive
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, true, false);
			receive.Logs.AddNew(Events.HoldTheWarehouseOrder);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			AssertNoRowError(receive, expectedErrorMessage);

			// Bonded Receive
			var bondedReceive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, false, false);
			bondedReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			bondedReceive.Inventory[0].CustomsData.WB_EntryKey = "123";
			bondedReceive.Logs.AddNew(Events.HoldTheWarehouseOrder);
			bondedReceive.AllocateLocationsWithMock();
			bondedReceive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(false, bondedReceive.IsFinalised);
			AssertHasRowError(bondedReceive, expectedErrorMessage);

			// we want to set the utc date to be later then the 'hold' event's utc date
			var allowFinaliseLog = bondedReceive.Logs.AddNew(Events.WarehouseJobCanNowBeFinalised);
			Helper.SetLogUTCTimeOnFactorySave(TestConnection, allowFinaliseLog, ZDateTime.Now.AddDays(1));
			Factory.Save(); // to set Posted Time

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var bondedReceiveInNewFactory = newFactory.Load<WhsReceive>(bondedReceive.PK);

			bondedReceiveInNewFactory.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, bondedReceiveInNewFactory.IsFinalised);
			AssertNoRowError(bondedReceiveInNewFactory, expectedErrorMessage);
		}

		#endregion

		#region TestFinaliseReceive_BookingDateModifiedOnlyInCurrentUser

		public void TestFinaliseReceive_BookingDateModifiedOnlyInCurrentUser()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RCV", data.Part1, 10m, true, false);
			Factory.Save();

			receive.WD_BookingDate = ZDateTimeOffset.Now;
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#region TestFinaliseReceive_UnfinalisedPutawayTransfersAreFinalised

		public void TestFinaliseReceive_UnfinalisedPutawayTransfersAreFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "A", 10m);
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "A", 10m);
			putawayTransfer.RunPreSaveValidation();
			putawayTransferLine.FinaliseDocketLine();
			Factory.Save();

			var receiveLine = receive.Lines[0];
			Assert("Precondition: Receive line has putaway transfer.", receiveLine.HasPutawayTransfer);
			Assert("Precondition: Putaway transfer is not finalised.", !putawayTransfer.IsFinalised);

			receive.FinaliseDocket();
			Assert("Receive is finalised.", receive.IsFinalised);
			Assert("Putaway transfer is finalised.", putawayTransfer.IsFinalised);
			Assert("Receive has no errors.", !receive.HasErrors);
		}

		public void TestFinaliseReceive_UnfinalisedPutawayTransfersAreFinalised_NoPutawayTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-2"));
			Factory.Save();

			Assert("Precondition: Receive line has no putaway transfer.", !receiveLine.HasPutawayTransfer);

			AssertNoExceptionThrown("Finalise does not throw exception.", () => receive.FinaliseDocket());
			Assert("Receive is finalised.", receive.IsFinalised);
			Assert("Receive has no errors.", !receive.HasErrors);
		}

		public void TestFinaliseReceive_UnfinalisedPutawayTransfersAreFinalised_PutawayTransferHasError()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "A", 10m);
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoorLocation, nonDockDoorLocation, "A", 10m);
			putawayTransfer.RunPreSaveValidation();
			Factory.Save();

			var receiveLine = receive.Lines[0];
			Assert("Precondition: Receive line has putaway transfer.", receiveLine.HasPutawayTransfer);
			Assert("Precondition: Putaway transfer is not finalised.", !putawayTransfer.IsFinalised);

			putawayTransfer.AddRowError("Error");
			Assert("Precondition: Putaway transfer has an error.", putawayTransfer.HasErrors);

			AssertNoExceptionThrown("Finalise does not throw exception.", () => receive.FinaliseDocket());
			Assert("Receive is not finalised.", !receive.IsFinalised);
			Assert("Putaway transfer is not finalised.", !putawayTransfer.IsFinalised);
			Assert("Receive has errors.", receive.HasErrors);
			AssertHasRowError("The receive line should contain error message.", receiveLine, WhsReceive.ContainsUnfinalizedPutawayTransfersErrorMessage);
		}

		#endregion

		#region TestFinaliseReceive_UseArrivalDateAsFinaliseDate

		public void TestFinaliseReceive_UseArrivalDateAsFinaliseDate()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = false;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);

			receive.WD_ArrivalDate = ZDateTimeOffset.Today.AddDays(10);

			receive.FinaliseDocket();
			AssertHasError("Error should exist", receive.WD_ArrivalDateInfo, "Arrival date is a future date.");
			AssertNoErrors("Errors should NOT exist", receive.WD_DocketStatusDescriptionInfo);
			AssertEquals("Receive must NOT be finalised.", false, receive.IsFinalised);
		}

		#endregion

		#region TestFinaliseReceive_Override_WE_AdjustmentArrivalDate

		public void TestFinaliseReceive_Override_WE_AdjustmentArrivalDate()
		{
			var date = ZDateTimeOffset.Today.AddDays(-1);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = false;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, finalise: false);

			receive.WD_ArrivalDate = date;

			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);
			AssertEquals("Should override line's WE_AdjustmentArrivalDate.", date, receive.Lines[0].WE_AdjustmentArrivalDate);
		}

		#endregion

		#region TestFinaliseReceive_Override_WE_AdjustmentArrivalDate_PickByBOMReceive_IsBOMProductPickedOnSalesOrder

		public void TestFinaliseReceive_Override_WE_AdjustmentArrivalDate_PickByBOMReceive_IsBOMProductPickedOnSalesOrder()
		{
			var date = ZDateTimeOffset.Today.AddDays(-1);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = false;
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", bike, 10m, finalise: false);
			Helper.CreateWhsReceiveLine(receive, wheel, 20m, data.Whs1.DefaultLocation);
			receive.WD_ArrivalDate = date;
			Factory.Save();

			receive.WD_WP_ParentPickForReceive = ZGuid.NewZGuid();

			var date2 = ZDateTimeOffset.Today.AddDays(-10);
			var line1 = receive.Lines.Single(l => l.WE_OP == bike.PK);
			var line2 = receive.Lines.Single(l => l.WE_OP == wheel.PK);
			line1.WE_AdjustmentArrivalDate = date2;
			line2.WE_AdjustmentArrivalDate = date2;

			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);
			AssertEquals("Should override since IsBOMProductPickedOnSalesOrder is true for bike.", date, line1.WE_AdjustmentArrivalDate);
			AssertEquals("Should NOT override since IsBOMProductPickedOnSalesOrder is false for wheel.", date2, line2.WE_AdjustmentArrivalDate);
		}

		#endregion

		#region TestFinaliseReceive_AssociatedWhsPutawayJobs

		public void TestFinaliseReceive_AssociatedWhsPutawayJobs()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff1 = helper.CreateGlbStaff("S1", "S1");
			var staff2 = helper.CreateGlbStaff("S2", "S2");

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			helper.CreateWhsReceiveLine(receive, data.Part1, 25m, data.Whs1.DefaultInboundDockDoorLocation, "PLT5");
			helper.CreateWhsReceiveLine(receive, data.Part2, 25m, data.Whs1.DefaultInboundDockDoorLocation, "PLT85");
			helper.CreateWhsReceiveLine(receive, data.Part1, 5m, data.Whs1.DefaultInboundDockDoorLocation, "PLT59");
			Factory.Save();

			var putawayTransfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine1 = helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT5", 25m);
			transferLine1.RunPreSaveValidation();
			transferLine1.WE_GS_NKPutawayBy = staff1.GS_Code;

			var transferLine2 = helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part2, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT85", 25m);
			transferLine2.RunPreSaveValidation();
			transferLine2.WE_GS_NKPutawayBy = staff1.GS_Code;

			var transferLine3 = helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT59", 5m);
			transferLine3.RunPreSaveValidation();
			transferLine3.WE_GS_NKPutawayBy = staff2.GS_Code;

			var putawayJob1 = helper.CreateWhsPutawayJob(data.Whs1, staff1);
			helper.CreateWhsPutawayLine(putawayJob1, "PLT5", false);
			helper.CreateWhsPutawayLine(putawayJob1, "PLT85", false);

			var putawayJob2 = helper.CreateWhsPutawayJob(data.Whs1, staff2);
			helper.CreateWhsPutawayLine(putawayJob2, "PLT59", false);

			transferLine1.FinaliseDocketLine();
			transferLine2.FinaliseDocketLine();
			transferLine3.FinaliseDocketLine();
			Factory.Save();

			// Act
			receive.FinaliseDocket();
			Factory.Save();

			// Assert
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertIsFinalisedPrecondition(newFactory.Load<WhsReceive>(receive.PK));
			AssertIsFinalisedPrecondition(newFactory.Load<WhsTransfer>(putawayTransfer.PK));
			AssertIsFinalisedPrecondition(newFactory.Load<WhsTransferLine>(transferLine1.PK));
			AssertIsFinalisedPrecondition(newFactory.Load<WhsTransferLine>(transferLine2.PK));
			AssertIsFinalisedPrecondition(newFactory.Load<WhsTransferLine>(transferLine3.PK));
			AssertIsFinalised(newFactory.Load<WhsPutawayJob>(putawayJob1.PK));
			AssertIsFinalised(newFactory.Load<WhsPutawayJob>(putawayJob2.PK));
		}

		public void TestFinaliseReceive_AssociatedWhsPutawayJobs_TransferLineFinalisedInFactoryOnly()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff1 = helper.CreateGlbStaff("S1", "S1");

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			helper.CreateWhsReceiveLine(receive, data.Part1, 25m, data.Whs1.DefaultInboundDockDoorLocation, "PLT5");
			helper.CreateWhsReceiveLine(receive, data.Part1, 15m, data.Whs1.DefaultInboundDockDoorLocation, "PLT75");
			Factory.Save();

			var putawayTransfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine1 = helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT5", 25m);
			transferLine1.RunPreSaveValidation();
			transferLine1.WE_GS_NKPutawayBy = staff1.GS_Code;

			var transferLine2 = helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT75", 15m);
			transferLine2.RunPreSaveValidation();
			transferLine2.WE_GS_NKPutawayBy = staff1.GS_Code;

			var putawayJob = helper.CreateWhsPutawayJob(data.Whs1, staff1);
			helper.CreateWhsPutawayLine(putawayJob, "PLT5", false);
			helper.CreateWhsPutawayLine(putawayJob, "PLT75", false);

			// Act
			receive.FinaliseDocket();
			Factory.Save();

			// Assert
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertIsFinalisedPrecondition(newFactory.Load<WhsReceive>(receive.PK));
			AssertIsFinalisedPrecondition(newFactory.Load<WhsTransfer>(putawayTransfer.PK));
			AssertIsFinalisedPrecondition(newFactory.Load<WhsTransferLine>(transferLine1.PK));
			AssertIsFinalisedPrecondition(newFactory.Load<WhsTransferLine>(transferLine2.PK));
			AssertIsFinalised(newFactory.Load<WhsPutawayJob>(putawayJob.PK));
		}

		public void TestFinaliseReceive_AssociatedWhsPutawayJobs_TransferLineFinalisedInFactoryOnly_TwoReceives()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff1 = helper.CreateGlbStaff("S1", "S1");

			var receive1 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			helper.CreateWhsReceiveLine(receive1, data.Part1, 25m, data.Whs1.DefaultInboundDockDoorLocation, "PLT5");

			var receive2 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			helper.CreateWhsReceiveLine(receive2, data.Part1, 15m, data.Whs1.DefaultInboundDockDoorLocation, "PLT75");
			Factory.Save();

			var putawayTransfer1 = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR440");
			putawayTransfer1.WD_IsPutawayTransfer = true;
			var transferLine1 = helper.SetupTransferLineForDockDoorLocation(putawayTransfer1, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT5", 25m);
			transferLine1.RunPreSaveValidation();
			transferLine1.WE_GS_NKPutawayBy = staff1.GS_Code;

			var putawayTransfer2 = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR144");
			putawayTransfer2.WD_IsPutawayTransfer = true;
			var transferLine2 = helper.SetupTransferLineForDockDoorLocation(putawayTransfer2, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT75", 15m);
			transferLine2.RunPreSaveValidation();
			transferLine2.WE_GS_NKPutawayBy = staff1.GS_Code;

			var putawayJob = helper.CreateWhsPutawayJob(data.Whs1, staff1);
			helper.CreateWhsPutawayLine(putawayJob, "PLT5", false);
			helper.CreateWhsPutawayLine(putawayJob, "PLT75", false);

			// Act
			receive1.FinaliseDocket();

			// Assert
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(putawayTransfer1);
			AssertIsFinalisedPrecondition(transferLine1);
			AssertEquals("Receive2 is not finalised", false, receive2.IsFinalised);
			AssertEquals("Putaway Transfer2 is not finalised", false, putawayTransfer2.IsFinalised);
			AssertEquals("Putaway Transfer Line2 is not finalised", false, transferLine2.IsFinalised);
			AssertEquals("Putaway Job is not finalised", false, putawayJob.IsFinalised);

			receive2.FinaliseDocket();
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertIsFinalisedPrecondition(newFactory.Load<WhsReceive>(receive1.PK));
			AssertIsFinalisedPrecondition(newFactory.Load<WhsTransfer>(putawayTransfer1.PK));
			AssertIsFinalisedPrecondition(newFactory.Load<WhsTransferLine>(transferLine1.PK));
			AssertIsFinalisedPrecondition(newFactory.Load<WhsReceive>(receive2.PK));
			AssertIsFinalisedPrecondition(newFactory.Load<WhsTransfer>(putawayTransfer2.PK));
			AssertIsFinalisedPrecondition(newFactory.Load<WhsTransferLine>(transferLine2.PK));
			AssertIsFinalised(newFactory.Load<WhsPutawayJob>(putawayJob.PK));
		}

		public void TestFinaliseReceive_AssociatedWhsPutawayJobs_IgnoresDuplicatePalletIDInOtherWhs()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = helper.CreateWarehouse("WH1", "D", 2, 1);
			var staff1 = helper.CreateGlbStaff("S1", "S1");

			var receive1 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			helper.CreateWhsReceiveLine(receive1, data.Part1, 25m, data.Whs1.DefaultInboundDockDoorLocation, "PLT5");
			helper.CreateWhsReceiveLine(receive1, data.Part1, 25m, data.Whs1.DefaultInboundDockDoorLocation, "PLT85");

			var receive2 = helper.CreateWhsReceive(data.Org1, whs2, "R2", Notify);
			helper.CreateWhsReceiveLine(receive2, data.Part2, 85m, whs2.DefaultInboundDockDoorLocation, "PLT85");
			Factory.Save();

			var putawayTransfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR000");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine1 = helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT5", 25m);
			transferLine1.RunPreSaveValidation();
			transferLine1.WE_GS_NKPutawayBy = staff1.GS_Code;

			var transferLine2 = helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT85", 25m);
			transferLine2.RunPreSaveValidation();
			transferLine2.WE_GS_NKPutawayBy = staff1.GS_Code;

			var putawayTransfer2 = helper.CreateWhsTransfer(data.Org1, whs2, "TR111");
			putawayTransfer2.WD_IsPutawayTransfer = true;
			var transferLine3 = helper.SetupTransferLineForDockDoorLocation(putawayTransfer2, data.Part2, whs2.DefaultInboundDockDoorLocation, whs2.FindLocation("D-2"), "PLT85", 85m);
			transferLine3.RunPreSaveValidation();
			transferLine3.WE_GS_NKPutawayBy = staff1.GS_Code;

			var putawayJob = helper.CreateWhsPutawayJob(data.Whs1, staff1);
			helper.CreateWhsPutawayLine(putawayJob, "PLT5", false);
			helper.CreateWhsPutawayLine(putawayJob, "PLT85", false);

			transferLine1.FinaliseDocketLine();
			transferLine2.FinaliseDocketLine();
			Factory.Save();

			// Act
			receive1.FinaliseDocket();
			Factory.Save();

			// Assert
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertIsFinalisedPrecondition(newFactory.Load<WhsReceive>(receive1.PK));
			AssertIsFinalisedPrecondition(newFactory.Load<WhsTransfer>(putawayTransfer.PK));
			AssertIsFinalisedPrecondition(newFactory.Load<WhsTransferLine>(transferLine1.PK));
			AssertIsFinalisedPrecondition(newFactory.Load<WhsTransferLine>(transferLine2.PK));

			AssertEquals("Receive2 not finalised", false, receive2.IsFinalised);
			AssertEquals("PutawayTransfer2 not finalised", false, putawayTransfer2.IsFinalised);
			AssertEquals("TransferLine3 not finalised", false, transferLine3.IsFinalised);

			AssertIsFinalised(newFactory.Load<WhsPutawayJob>(putawayJob.PK));
		}

		public void TestFinaliseReceive_AssociatedWhsPutawayJobs_IgnoresPuttingAwayWhsPutawayJob()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff1 = helper.CreateGlbStaff("S1", "S1");
			var staff2 = helper.CreateGlbStaff("S2", "S2");

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			helper.CreateWhsReceiveLine(receive, data.Part1, 25m, data.Whs1.DefaultInboundDockDoorLocation, "PLT5");
			helper.CreateWhsReceiveLine(receive, data.Part2, 25m, data.Whs1.DefaultInboundDockDoorLocation, "PLT85");
			helper.CreateWhsReceiveLine(receive, data.Part1, 5m, data.Whs1.DefaultInboundDockDoorLocation, "PLT59");
			Factory.Save();

			var putawayTransfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR00");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine1 = helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT5", 25m);
			transferLine1.RunPreSaveValidation();
			transferLine1.WE_GS_NKPutawayBy = staff1.GS_Code;

			var transferLine2 = helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part2, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT85", 25m);
			transferLine2.RunPreSaveValidation();
			transferLine2.WE_GS_NKPutawayBy = staff1.GS_Code;

			var transferLine3 = helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT59", 5m);
			transferLine3.RunPreSaveValidation();
			transferLine3.WE_GS_NKPutawayBy = staff2.GS_Code;

			var putawayJob1 = helper.CreateWhsPutawayJob(data.Whs1, staff1);
			helper.CreateWhsPutawayLine(putawayJob1, "PLT5", false);
			helper.CreateWhsPutawayLine(putawayJob1, "PLT85", false);

			var putawayJob2 = helper.CreateWhsPutawayJob(data.Whs1, staff2);
			helper.CreateWhsPutawayLine(putawayJob2, "PLT59", true);

			transferLine1.FinaliseDocketLine();
			transferLine2.FinaliseDocketLine();
			transferLine3.FinaliseDocketLine();
			Factory.Save();

			// Act
			receive.FinaliseDocket();
			Factory.Save();

			// Assert
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertEquals("PutawayJob2 not finalised", false, newFactory.LoadTop1<WhsPutawayJob>(new ZQuery(WhsPutawayJobSchema.PK, putawayJob2.PK)).IsFinalised);

			AssertIsFinalisedPrecondition(newFactory.Load<WhsReceive>(receive.PK));
			AssertIsFinalisedPrecondition(newFactory.Load<WhsTransfer>(putawayTransfer.PK));
			AssertIsFinalisedPrecondition(newFactory.Load<WhsTransferLine>(transferLine1.PK));
			AssertIsFinalisedPrecondition(newFactory.Load<WhsTransferLine>(transferLine2.PK));
			AssertIsFinalisedPrecondition(newFactory.Load<WhsTransferLine>(transferLine3.PK));
			AssertIsFinalised(newFactory.Load<WhsPutawayJob>(putawayJob1.PK));
		}

		public void TestFinaliseReceive_AssociatedWhsPutawayJobs_PalletOnTwoReceives()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receive1 = helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation, "PLT5", 25m);

			var receive2 = helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R2", ZDateTimeOffset.Now);
			helper.CreateInventoryForDockDoorLocation(receive2, data.Part1, dockDoorLocation, "PLT59", 5m);
			Factory.Save();

			var putawayTransfer1 = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer1.WD_IsPutawayTransfer = true;
			var transferLine1 = helper.SetupTransferLineForDockDoorLocation(putawayTransfer1, data.Part1, dockDoorLocation, nonDockDoorLocation, "PLT5", 25m);
			putawayTransfer1.RunPreSaveValidation();

			var putawayTransfer2 = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			putawayTransfer2.WD_IsPutawayTransfer = true;
			var transferLine2 = helper.SetupTransferLineForDockDoorLocation(putawayTransfer2, data.Part1, dockDoorLocation, nonDockDoorLocation, "PLT59", 5m);
			putawayTransfer2.RunPreSaveValidation();

			var putawayJob = helper.CreateWhsPutawayJob(data.Whs1, staff);
			helper.CreateWhsPutawayLine(putawayJob, "PLT5", false);
			helper.CreateWhsPutawayLine(putawayJob, "PLT59", false);

			transferLine1.FinaliseDocketLine();
			Factory.Save();

			// Act
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(transferLine1);
			AssertIsFinalisedPrecondition(putawayTransfer1);
			Factory.Save();

			// Assert
			AssertEquals("Putaway Job is not finalised", false, putawayJob.IsFinalised);

			// Act
			transferLine2.FinaliseDocketLine();
			Factory.Save();

			receive2.FinaliseDocket();

			// Assert
			AssertIsFinalisedPrecondition(receive2);
			AssertIsFinalisedPrecondition(transferLine2);
			AssertIsFinalisedPrecondition(putawayTransfer2);
			AssertIsFinalised(putawayJob);
		}

		public void TestFinaliseReceive_AssociatedWhsPutawayJobs_DuplicateFinalisedPalletID()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			// Set up finalised duplicate Pallet
			var receiveDup = helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "RD", ZDateTimeOffset.Now);
			helper.CreateInventoryForDockDoorLocation(receiveDup, data.Part1, dockDoorLocation, "PLT5", 25m);
			Factory.Save();

			var putawayTransferDup = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TRD");
			putawayTransferDup.WD_IsPutawayTransfer = true;
			var transferLineDup = helper.SetupTransferLineForDockDoorLocation(putawayTransferDup, data.Part1, dockDoorLocation, nonDockDoorLocation, "PLT5", 25m);
			transferLineDup.RunPreSaveValidation();
			transferLineDup.FinaliseDocketLine();
			Factory.Save();

			receiveDup.FinaliseDocket();
			AssertIsFinalisedPrecondition(receiveDup);
			AssertIsFinalisedPrecondition(transferLineDup);
			AssertIsFinalisedPrecondition(putawayTransferDup);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 25m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			AssertEquals("Putaway transfer line should be 0 SOH", 0m, transferLineDup.WE_StockOnHand);

			// Complete test
			var receive1 = helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation, "PLT5", 25m);

			var receive2 = helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R2", ZDateTimeOffset.Now);
			helper.CreateInventoryForDockDoorLocation(receive2, data.Part1, dockDoorLocation, "PLT59", 5m);
			Factory.Save();

			var putawayTransfer1 = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer1.WD_IsPutawayTransfer = true;
			var transferLine1 = helper.SetupTransferLineForDockDoorLocation(putawayTransfer1, data.Part1, dockDoorLocation, nonDockDoorLocation, "PLT5", 25m);
			putawayTransfer1.RunPreSaveValidation();

			var putawayTransfer2 = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			putawayTransfer2.WD_IsPutawayTransfer = true;
			var transferLine2 = helper.SetupTransferLineForDockDoorLocation(putawayTransfer2, data.Part1, dockDoorLocation, nonDockDoorLocation, "PLT59", 5m);
			putawayTransfer2.RunPreSaveValidation();

			var putawayJob = helper.CreateWhsPutawayJob(data.Whs1, staff);
			helper.CreateWhsPutawayLine(putawayJob, "PLT5", false);
			helper.CreateWhsPutawayLine(putawayJob, "PLT59", false);

			transferLine1.FinaliseDocketLine();
			Factory.Save();

			// Act
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(transferLine1);
			AssertIsFinalisedPrecondition(putawayTransfer1);
			Factory.Save();

			// Assert
			AssertEquals("Putaway Job is not finalised", false, putawayJob.IsFinalised);

			// Act
			transferLine2.FinaliseDocketLine();
			Factory.Save();

			receive2.FinaliseDocket();

			// Assert
			AssertIsFinalisedPrecondition(receive2);
			AssertIsFinalisedPrecondition(transferLine2);
			AssertIsFinalisedPrecondition(putawayTransfer2);
			AssertIsFinalised(putawayJob);
		}

		public void TestFinaliseReceive_AssociatedWhsPutawayJobs_ConsolidatedPallet()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = helper.CreateGlbStaff("S1", "S1");

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			helper.CreateWhsReceiveLine(receive, data.Part1, 25m, data.Whs1.DefaultInboundDockDoorLocation, "PLT5");
			Factory.Save();

			var putawayJob = helper.CreateWhsPutawayJob(data.Whs1, staff);
			helper.CreateWhsPutawayLine(putawayJob, "PLT5", false);
			Factory.Save();

			var putawayTransfer = helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR0");
			putawayTransfer.WD_IsPutawayTransfer = true;
			var transferLine1 = helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, data.Whs1.FindLocation("A-2"), "PLT5", 25m);
			transferLine1.RunPreSaveValidation();
			transferLine1.WE_PalletID = "PLT88";
			transferLine1.WE_GS_NKPutawayBy = staff.GS_Code;
			transferLine1.PickedTime = ZDateTimeOffset.Now;

			// Act
			receive.FinaliseDocket();
			Factory.Save();

			// Assert
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertIsFinalisedPrecondition(newFactory.Load<WhsReceive>(receive.PK));
			AssertIsFinalisedPrecondition(newFactory.Load<WhsTransfer>(putawayTransfer.PK));
			var transferLineInOtherFactory = newFactory.Load<WhsTransferLine>(transferLine1.PK);
			AssertIsFinalisedPrecondition(transferLineInOtherFactory);
			AssertEquals("TransferLine WE_PalletID", "PLT88", transferLineInOtherFactory.WE_PalletID);
			AssertIsFinalised(newFactory.Load<WhsPutawayJob>(putawayJob.PK));
		}

		#endregion

		#region TestFinaliseReceive_GivesPromptWithDefaultableQueryEventArgs

		public void TestFinaliseReceive_GivesPromptWithDefaultableQueryEventArgs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 0m);
			Factory.Save();

			AssertEquals("Precondition: receive has no errors", false, receive.HasErrors);
			AssertEquals("Precondition: receive is unfinalized", false, receive.IsFinalised);

			Notify.PreQueryUser += (sender, e) =>
			{
				if (e is DefaultableQueryUserEventArgs args)
				{
					AssertEquals("Precondition: default response correct", false, args.Response);
					args.Response = true;
				}
			};

			receive.FinaliseDocket();

			var lastQueryEventArgs = (DefaultableQueryUserEventArgs)Notify.LastQueryUserEventArgs;
			AssertEquals("Postcondition: correct eventArgs type", true, lastQueryEventArgs is DefaultableQueryUserEventArgs);
			var expectedMessage = "Finalizing this Receipt will update the inventory, making it available for Picking / Release.\r\nOn finalization, this Receipt will become read-only so that it cannot be modified.\r\nThis finalization process cannot be undone once saved.\r\n\r\nNote: There are warnings. Click Cancel to review these warnings before finalizing.\r\n\r\nDo you wish to Finalize this Receipt?";
			AssertEquals("Postcondition: correct notification message", expectedMessage, lastQueryEventArgs.Message);
			AssertEquals("Postcondition: correct response", true, lastQueryEventArgs.Response);
			AssertEquals("Postcondition: receive is finalized.", true, receive.IsFinalised);
			AssertEquals("Postcondition: receive has no errors.", false, receive.HasErrors);
			AssertEquals("User query Buttons should be correct.", ZMessageBoxButtons.YesNoCancel, lastQueryEventArgs.Context.Buttons);
			AssertContainsExactElementsInAnyOrder("User query Results Not To Save should be correct.", new[] { ZDialogResult.No, ZDialogResult.Cancel }, lastQueryEventArgs.Context.DialogResultsToNotSave);
		}

		public void TestFinalise_PlannedReceive_WithWorkingUnloadTask_PromptsToCloseActiveUnloadTasks()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 0m);
			receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			AssertEquals("Precondition: receive has no errors", false, receive.HasErrors);
			AssertEquals("Precondition: receive is unfinalized", false, receive.IsFinalised);
			AssertEquals("Precondition", TaskPlanningStatus.Codes.Planned, receive.WD_TaskPlanningStatus);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);

			var finaliseReceiveQueryIsDisplayed = false;
			var closeActiveUnloadTasksQueryIsDisplayed = false;
			var finaliseReceiveConfirmationMessage = "Finalizing this Receipt will update the inventory, making it available for Picking / Release.\r\nOn finalization, this Receipt will become read-only so that it cannot be modified.\r\nThis finalization process cannot be undone once saved.\r\n\r\nNote: There are warnings. Click Cancel to review these warnings before finalizing.\r\n\r\nDo you wish to Finalize this Receipt?";
			var closeActiveUnloadTasksConfirmationMessage = "This Receipt has active unload job tasks set to working. The tasks need to be completed to complete the unload for this Receipt.\r\n\r\nDo you wish to close these active tasks set to working to complete the unload for this Receipt?";
			Notify.PreQueryUser += (sender, e) =>
			{
				if (e is DefaultableQueryUserEventArgs args)
				{
					AssertEquals("Precondition: default response correct", false, args.Response);
					args.Response = true;
					if (args.Message.Equals(finaliseReceiveConfirmationMessage))
					{
						finaliseReceiveQueryIsDisplayed = true;
					}
					else if (args.Message.Equals(closeActiveUnloadTasksConfirmationMessage))
					{
						closeActiveUnloadTasksQueryIsDisplayed = true;
						AssertEquals("Correct response", true, args.Response);
						AssertEquals("Correct caption", "Completing unload with active unload tasks set to working confirmation", args.Caption);
						AssertContainsExactElementsInAnyOrder("User query Results Not To Save should be correct.", new[] { ZDialogResult.No, ZDialogResult.Cancel }, args.Context.DialogResultsToNotSave);
						AssertEquals("User query Buttons should be correct.", ZMessageBoxButtons.YesNoCancel, args.Context.Buttons);
					}
				}
			};

			receive.FinaliseDocket();

			Assert("User query to finalise receive was displayed.", finaliseReceiveQueryIsDisplayed);
			Assert("User query to close active unload tasks was displayed.", closeActiveUnloadTasksQueryIsDisplayed);

			var lastQueryEventArgs = (DefaultableQueryUserEventArgs)Notify.LastQueryUserEventArgs;
			AssertEquals("Confirmation to finalise is the last confirmation notification.", finaliseReceiveConfirmationMessage, lastQueryEventArgs.Message);
		}

		public void TestFinalise_NotPlannedReceive_WithWorkingTask_DoesNotPromptToCloseActiveUnloadTasks()
			=> TestFinalise_DoesNotPromptToCloseActiveUnloadTasks(isWorkingTask: true, isPlannedReceive: false, isTaskForCurrentUser: false);

		public void TestFinalise_PlannedReceive_WithNotWorkingTask_DoesNotPromptToCloseActiveUnloadTasks()
			=> TestFinalise_DoesNotPromptToCloseActiveUnloadTasks(isWorkingTask: false, isPlannedReceive: true, isTaskForCurrentUser: false);

		public void TestFinalise_PlannedReceive_WithWorkingTask_TaskForCurrentUser_DoesNotPromptToCloseActiveUnloadTasks()
			=> TestFinalise_DoesNotPromptToCloseActiveUnloadTasks(isWorkingTask: false, isPlannedReceive: false, isTaskForCurrentUser: true);

		void TestFinalise_DoesNotPromptToCloseActiveUnloadTasks(bool isWorkingTask, bool isPlannedReceive, bool isTaskForCurrentUser)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			if (isPlannedReceive)
			{
				receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			}
			Helper.CreateWhsReceiveLine(receive, data.Part1, 0m);
			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, isTaskForCurrentUser ? (GlbStaff)Env.CurrentUser : staff);
			if (isWorkingTask)
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			}
			Factory.Save();

			AssertEquals("Precondition: receive has no errors", false, receive.HasErrors);
			AssertEquals("Precondition: receive is unfinalized", false, receive.IsFinalised);
			AssertEquals("Precondition", isPlannedReceive, receive.WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned));
			AssertEquals("Precondition", isWorkingTask, task.P9_Status.EqualsIgnoringCase(ProcessTaskStatusCodeList.Codes.Working));

			var finaliseReceiveConfirmationMessage = "Finalizing this Receipt will update the inventory, making it available for Picking / Release.\r\nOn finalization, this Receipt will become read-only so that it cannot be modified.\r\nThis finalization process cannot be undone once saved.\r\n\r\nNote: There are warnings. Click Cancel to review these warnings before finalizing.\r\n\r\nDo you wish to Finalize this Receipt?";
			var closeActiveUnloadTasksConfirmationMessage = "This Receipt has active unload job tasks set to working. The tasks need to be completed to complete the unload for this Receipt.\r\n\r\nDo you wish to close these active tasks set to working to complete the unload for this Receipt?";
			var finaliseReceiveQueryIsDisplayed = false;
			var closeActiveUnloadTasksQueryIsDisplayed = false;
			Notify.PreQueryUser += (sender, e) =>
			{
				if (e is DefaultableQueryUserEventArgs args)
				{
					AssertEquals("Precondition: default response correct", false, args.Response);
					args.Response = true;
					if (args.Message.Equals(finaliseReceiveConfirmationMessage))
					{
						finaliseReceiveQueryIsDisplayed = true;
					}
					else if (args.Message.Equals(closeActiveUnloadTasksConfirmationMessage))
					{
						closeActiveUnloadTasksQueryIsDisplayed = true;
					}
				}
			};

			receive.FinaliseDocket();

			Assert("User query to finalise receive was displayed.", finaliseReceiveQueryIsDisplayed);
			Assert("User query to close active tasks was not displayed.", !closeActiveUnloadTasksQueryIsDisplayed);
		}

		public void TestFinalise_PlannedReceive_WithWorkingUnloadTask_PromptsToCloseActivePutawayTasks()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var dockDoor = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoor = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoor, "PLT1");
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoor, nonDockDoor, "PLT1", 10m);
			putawayTransfer.RunPreSaveValidation();

			AssertNotNull("Precondition: Putaway transfer has been created.", putawayTransferLine);
			AssertEquals("Precondition: Receive line's inventory status is Received.", InventoryStatus.Codes.Received, receiveLine.WE_OriginalInventoryStatus);

			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;
			putawayTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertIsFinalisedPrecondition(putawayTransfer);
			AssertEquals("Precondition: receive has no errors", false, receive.HasErrors);
			AssertEquals("Precondition: receive is unfinalized", false, receive.IsFinalised);
			AssertEquals("Precondition", TaskPlanningStatus.Codes.Planned, receive.WD_TaskPlanningStatus);
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);

			var finaliseReceiveQueryIsDisplayed = false;
			var closeActivePutawayTasksQueryIsDisplayed = false;
			var finaliseReceiveConfirmationMessage = "Finalizing this Receipt will update the inventory, making it available for Picking / Release.\r\nOn finalization, this Receipt will become read-only so that it cannot be modified.\r\nThis finalization process cannot be undone once saved.\r\n\r\nNote: There are warnings. Click Cancel to review these warnings before finalizing.\r\n\r\nDo you wish to Finalize this Receipt?";
			var closeActivePutawayTasksConfirmationMessage = "This Receipt has related active putaway job tasks set to working. The tasks need to be completed to finalize this Receipt.\r\n\r\nDo you wish to close these active tasks set to working to finalize this Receipt?";
			Notify.PreQueryUser += (sender, e) =>
			{
				if (e is DefaultableQueryUserEventArgs args)
				{
					AssertEquals("Precondition: default response correct", false, args.Response);
					args.Response = true;
					if (args.Message.Equals(finaliseReceiveConfirmationMessage))
					{
						finaliseReceiveQueryIsDisplayed = true;
					}
					else if (args.Message.Equals(closeActivePutawayTasksConfirmationMessage))
					{
						closeActivePutawayTasksQueryIsDisplayed = true;
						AssertEquals("Correct response", true, args.Response);
						AssertEquals("Correct caption", "Finalizing receive with active putaway tasks set to working confirmation", args.Caption);
						AssertContainsExactElementsInAnyOrder("User query Results Not To Save should be correct.", new[] { ZDialogResult.No, ZDialogResult.Cancel }, args.Context.DialogResultsToNotSave);
						AssertEquals("User query Buttons should be correct.", ZMessageBoxButtons.YesNoCancel, args.Context.Buttons);
					}
				}
			};

			receive.FinaliseDocket();

			Assert("User query to finalise receive was displayed.", finaliseReceiveQueryIsDisplayed);
			Assert("User query to close active putaway tasks was displayed.", closeActivePutawayTasksQueryIsDisplayed);

			var lastQueryEventArgs = (DefaultableQueryUserEventArgs)Notify.LastQueryUserEventArgs;
			AssertEquals("Confirmation to finalise is the last confirmation notification.", finaliseReceiveConfirmationMessage, lastQueryEventArgs.Message);
		}

		public void TestFinalise_NotPlannedReceive_WithWorkingTask_DoesNotPromptToCloseActivePutawayTasks()
			=> TestFinalise_DoesNotPromptToCloseActivePutawayTasks(isWorkingTask: true, isPlannedReceive: false, isTaskForCurrentUser: false);

		public void TestFinalise_PlannedReceive_WithNotWorkingTask_DoesNotPromptToCloseActivePutawayTasks()
			=> TestFinalise_DoesNotPromptToCloseActivePutawayTasks(isWorkingTask: false, isPlannedReceive: true, isTaskForCurrentUser: false);

		public void TestFinalise_PlannedReceive_WithWorkingTask_TaskForCurrentUser_DoesNotPromptToCloseActivePutawayTasks()
			=> TestFinalise_DoesNotPromptToCloseActivePutawayTasks(isWorkingTask: false, isPlannedReceive: false, isTaskForCurrentUser: true);

		void TestFinalise_DoesNotPromptToCloseActivePutawayTasks(bool isWorkingTask, bool isPlannedReceive, bool isTaskForCurrentUser)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var dockDoor = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoor = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			if (isPlannedReceive)
			{
				receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			}
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoor, "PLT1");
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoor, nonDockDoor, "PLT1", 10m);
			putawayTransfer.RunPreSaveValidation();

			AssertNotNull("Precondition: Putaway transfer has been created.", putawayTransferLine);
			AssertEquals("Precondition: Receive line's inventory status is Received.", InventoryStatus.Codes.Received, receiveLine.WE_OriginalInventoryStatus);

			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, isTaskForCurrentUser ? (GlbStaff)Env.CurrentUser : staff);
			if (isWorkingTask)
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			}
			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;
			putawayTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertIsFinalisedPrecondition(putawayTransfer);
			AssertEquals("Precondition: receive has no errors", false, receive.HasErrors);
			AssertEquals("Precondition: receive is unfinalized", false, receive.IsFinalised);
			AssertEquals("Precondition", isPlannedReceive, receive.WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned));
			AssertEquals("Precondition", isWorkingTask, task.P9_Status.EqualsIgnoringCase(ProcessTaskStatusCodeList.Codes.Working));

			var finaliseReceiveQueryIsDisplayed = false;
			var closeActivePutawayTasksQueryIsDisplayed = false;
			var finaliseReceiveConfirmationMessage = "Finalizing this Receipt will update the inventory, making it available for Picking / Release.\r\nOn finalization, this Receipt will become read-only so that it cannot be modified.\r\nThis finalization process cannot be undone once saved.\r\n\r\nNote: There are warnings. Click Cancel to review these warnings before finalizing.\r\n\r\nDo you wish to Finalize this Receipt?";
			var closeActivePutawayTasksConfirmationMessage = "This Receipt has related active putaway job tasks set to working. The tasks need to be completed to finalize this Receipt.\r\n\r\nDo you wish to close these active tasks set to working to finalize this Receipt?";
			Notify.PreQueryUser += (sender, e) =>
			{
				if (e is DefaultableQueryUserEventArgs args)
				{
					AssertEquals("Precondition: default response correct", false, args.Response);
					args.Response = true;
					if (args.Message.Equals(finaliseReceiveConfirmationMessage))
					{
						finaliseReceiveQueryIsDisplayed = true;
					}
					else if (args.Message.Equals(closeActivePutawayTasksConfirmationMessage))
					{
						closeActivePutawayTasksQueryIsDisplayed = true;
					}
				}
			};

			receive.FinaliseDocket();

			Assert("User query to finalise receive was displayed.", finaliseReceiveQueryIsDisplayed);
			Assert("User query to close active tasks was not displayed.", !closeActivePutawayTasksQueryIsDisplayed);
		}

		public void TestFinaliseDocket_PlannedReceive_WithWorkingTask()
			=> TestFinaliseDocketWithUnloadTaskCore(
					isPlannedReceive: true,
					isUnloadTask: true,
					withUserConfirmation: true,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Working,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Closed);

		public void TestFinaliseDocketWithoutUserConfirmation_PlannedReceive_WithWorkingTask()
			=> TestFinaliseDocketWithUnloadTaskCore(
					isPlannedReceive: true,
					isUnloadTask: true,
					withUserConfirmation: false,
					expectedIsFinalised: false,
					taskStatus: ProcessTaskStatusCodeList.Codes.Working,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Working);

		public void TestFinaliseDocket_PlannedReceive_WithSuspendedTask()
			=> TestFinaliseDocketWithUnloadTaskCore(
					isPlannedReceive: true,
					isUnloadTask: true,
					withUserConfirmation: true,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Suspended,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Closed);

		public void TestFinaliseDocketWithoutUserConfirmation_PlannedReceive_WithSuspendedTask()
			=> TestFinaliseDocketWithUnloadTaskCore(
					isPlannedReceive: true,
					isUnloadTask: true,
					withUserConfirmation: false,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Suspended,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Closed);

		public void TestFinaliseDocket_PlannedReceive_WithOpenTask()
			=> TestFinaliseDocketWithUnloadTaskCore(
					isPlannedReceive: true,
					isUnloadTask: true,
					withUserConfirmation: true,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Open,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Cancelled);

		public void TestFinaliseDocketWithoutUserConfirmation_PlannedReceive_WithOpenTask()
			=> TestFinaliseDocketWithUnloadTaskCore(
					isPlannedReceive: true,
					isUnloadTask: true,
					withUserConfirmation: false,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Open,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Cancelled);

		public void TestFinaliseDocket_PlannedReceive_WithAssignedTask()
			=> TestFinaliseDocketWithUnloadTaskCore(
					isPlannedReceive: true,
					isUnloadTask: true,
					withUserConfirmation: true,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Assigned,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Cancelled);

		public void TestFinaliseDocketWithoutUserConfirmation_PlannedReceive_WithAssignedTask()
			=> TestFinaliseDocketWithUnloadTaskCore(
					isPlannedReceive: true,
					isUnloadTask: true,
					withUserConfirmation: false,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Assigned,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Cancelled);

		public void TestFinaliseDocket_PlannedReceive_WithCancelledTask()
			=> TestFinaliseDocketWithUnloadTaskCore(
					isPlannedReceive: true,
					isUnloadTask: true,
					withUserConfirmation: true,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Cancelled);

		public void TestFinaliseDocketWithoutUserConfirmation_PlannedReceive_WithCancelledTask()
			=> TestFinaliseDocketWithUnloadTaskCore(
					isPlannedReceive: true,
					isUnloadTask: true,
					withUserConfirmation: false,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Cancelled);

		public void TestFinaliseDocket_PlannedReceive_WithClosedTask()
			=> TestFinaliseDocketWithUnloadTaskCore(
					isPlannedReceive: true,
					isUnloadTask: true,
					withUserConfirmation: true,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Closed,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Closed);

		public void TestFinaliseDocketWithoutUserConfirmation_PlannedReceive_WithClosedTask()
			=> TestFinaliseDocketWithUnloadTaskCore(
					isPlannedReceive: true,
					isUnloadTask: true,
					withUserConfirmation: false,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Closed,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Closed);

		public void TestFinaliseDocket_PlannedReceive_WithNotUnloadTask()
			=> TestFinaliseDocketWithUnloadTaskCore(
					isPlannedReceive: true,
					isUnloadTask: false,
					withUserConfirmation: true,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Open,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Open);

		public void TestFinaliseDocketWithoutUserConfirmation_PlannedReceive_WithNotUnloadTask()
			=> TestFinaliseDocketWithUnloadTaskCore(
					isPlannedReceive: true,
					isUnloadTask: false,
					withUserConfirmation: false,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Open,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Open);

		public void TestFinaliseDocket_NotPlannedReceive_WithTask()
			=> TestFinaliseDocketWithUnloadTaskCore(
					isPlannedReceive: false,
					isUnloadTask: true,
					withUserConfirmation: true,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Assigned,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Assigned);

		public void TestFinaliseDocketWithoutUserConfirmation_NotPlannedReceive_WithTask()
			=> TestFinaliseDocketWithUnloadTaskCore(
					isPlannedReceive: false,
					isUnloadTask: true,
					withUserConfirmation: false,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Assigned,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Assigned);

		void TestFinaliseDocketWithUnloadTaskCore(bool isPlannedReceive, bool isUnloadTask, bool withUserConfirmation, bool expectedIsFinalised, string taskStatus, string expectedTaskStatusAfterFinalize)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA2 = data.Whs1.FindLocation("A-2");
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, locationA2);
			if (isPlannedReceive)
			{
				receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			}
			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			if (!isUnloadTask)
			{
				task.P9_FormFlowType = string.Empty;
			}
			task.P9_Status = taskStatus;
			Factory.Save();

			AssertEquals("Precondition: receive has no errors", false, receive.HasErrors);
			AssertEquals("Precondition: receive is unfinalized", false, receive.IsFinalised);
			AssertEquals("Precondition", isPlannedReceive, receive.WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned));
			AssertEquals("Precondition", taskStatus, task.P9_Status);

			if (withUserConfirmation)
			{
				Assert("Precondition", Notify.DefaultResponse);
				receive.FinaliseDocket();
			}
			else
			{
				receive.FinaliseDocketWithoutUserConfirmation();
			}
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
			AssertEquals(expectedIsFinalised, receiveInNewFactory.IsFinalised);

			var taskInNewFactory = newFactory.Load<ProcessTask>(task.PK);
			AssertEquals(expectedTaskStatusAfterFinalize, taskInNewFactory.P9_Status);
		}

		public void TestFinaliseDocketWithUnloadTask_WithUserConfirmation_UserCancels()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA2 = data.Whs1.FindLocation("A-2");
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, locationA2);
			receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			AssertEquals("Precondition: receive has no errors", false, receive.HasErrors);
			AssertEquals("Precondition: receive is unfinalized", false, receive.IsFinalised);
			AssertEquals("Precondition", true, receive.WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned));
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);

			var closeUnloadTasksConfirmationMessage = "This Receipt has active unload job tasks set to working. The tasks need to be completed to complete the unload for this Receipt.\r\n\r\nDo you wish to close these active tasks set to working to complete the unload for this Receipt?";
			Notify.PreQueryUser += (sender, e) =>
			{
				if (e is DefaultableQueryUserEventArgs args)
				{
					args.Response = !args.Message.Equals(closeUnloadTasksConfirmationMessage);
				}
			};

			receive.FinaliseDocket();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
			AssertEquals(false, receiveInNewFactory.IsFinalised);

			var taskInNewFactory = newFactory.Load<ProcessTask>(task.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, taskInNewFactory.P9_Status);
		}

		public void TestFinaliseDocket_PlannedReceive_WithWorkingPutawayTask()
			=> TestFinaliseDocketWithPutawayTaskCore(
					isPlannedReceive: true,
					isPutawayTask: true,
					withUserConfirmation: true,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Working,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Closed);

		public void TestFinaliseDocketWithoutUserConfirmation_PlannedReceive_WithWorkingPutawayTask()
			=> TestFinaliseDocketWithPutawayTaskCore(
					isPlannedReceive: true,
					isPutawayTask: true,
					withUserConfirmation: false,
					expectedIsFinalised: false,
					taskStatus: ProcessTaskStatusCodeList.Codes.Working,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Working);

		public void TestFinaliseDocket_PlannedReceive_WithSuspendedPutawayTask()
			=> TestFinaliseDocketWithPutawayTaskCore(
					isPlannedReceive: true,
					isPutawayTask: true,
					withUserConfirmation: true,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Suspended,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Closed);

		public void TestFinaliseDocketWithoutUserConfirmation_PlannedReceive_WithSuspendedPutawayTask()
			=> TestFinaliseDocketWithPutawayTaskCore(
					isPlannedReceive: true,
					isPutawayTask: true,
					withUserConfirmation: false,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Suspended,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Closed);

		public void TestFinaliseDocket_PlannedReceive_WithOpenPutawayTask()
			=> TestFinaliseDocketWithPutawayTaskCore(
					isPlannedReceive: true,
					isPutawayTask: true,
					withUserConfirmation: true,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Open,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Cancelled);

		public void TestFinaliseDocketWithoutUserConfirmation_PlannedReceive_WithOpenPutawayTask()
			=> TestFinaliseDocketWithPutawayTaskCore(
					isPlannedReceive: true,
					isPutawayTask: true,
					withUserConfirmation: false,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Open,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Cancelled);

		public void TestFinaliseDocket_PlannedReceive_WithAssignedPutawayTask()
			=> TestFinaliseDocketWithPutawayTaskCore(
					isPlannedReceive: true,
					isPutawayTask: true,
					withUserConfirmation: true,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Assigned,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Cancelled);

		public void TestFinaliseDocketWithoutUserConfirmation_PlannedReceive_WithAssignedPutawayTask()
			=> TestFinaliseDocketWithPutawayTaskCore(
					isPlannedReceive: true,
					isPutawayTask: true,
					withUserConfirmation: false,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Assigned,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Cancelled);

		public void TestFinaliseDocket_PlannedReceive_WithCancelledPutawayTask()
			=> TestFinaliseDocketWithPutawayTaskCore(
					isPlannedReceive: true,
					isPutawayTask: true,
					withUserConfirmation: true,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Cancelled);

		public void TestFinaliseDocketWithoutUserConfirmation_PlannedReceive_WithCancelledPutawayTask()
			=> TestFinaliseDocketWithPutawayTaskCore(
					isPlannedReceive: true,
					isPutawayTask: true,
					withUserConfirmation: false,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Cancelled);

		public void TestFinaliseDocket_PlannedReceive_WithClosedPutawayTask()
			=> TestFinaliseDocketWithPutawayTaskCore(
					isPlannedReceive: true,
					isPutawayTask: true,
					withUserConfirmation: true,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Closed,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Closed);

		public void TestFinaliseDocketWithoutUserConfirmation_PlannedReceive_WithClosedPutawayTask()
			=> TestFinaliseDocketWithPutawayTaskCore(
					isPlannedReceive: true,
					isPutawayTask: true,
					withUserConfirmation: false,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Closed,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Closed);

		public void TestFinaliseDocket_PlannedReceive_WithNotUnloadPutawayTask()
			=> TestFinaliseDocketWithPutawayTaskCore(
					isPlannedReceive: true,
					isPutawayTask: false,
					withUserConfirmation: true,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Open,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Open);

		public void TestFinaliseDocketWithoutUserConfirmation_PlannedReceive_WithNotUnloadPutawayTask()
			=> TestFinaliseDocketWithPutawayTaskCore(
					isPlannedReceive: true,
					isPutawayTask: false,
					withUserConfirmation: false,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Open,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Open);

		public void TestFinaliseDocket_NotPlannedReceive_WithPutawayTask()
			=> TestFinaliseDocketWithPutawayTaskCore(
					isPlannedReceive: false,
					isPutawayTask: true,
					withUserConfirmation: true,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Assigned,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Assigned);

		public void TestFinaliseDocketWithoutUserConfirmation_NotPlannedReceive_WithPutawayTask()
			=> TestFinaliseDocketWithPutawayTaskCore(
					isPlannedReceive: false,
					isPutawayTask: true,
					withUserConfirmation: false,
					expectedIsFinalised: true,
					taskStatus: ProcessTaskStatusCodeList.Codes.Assigned,
					expectedTaskStatusAfterFinalize: ProcessTaskStatusCodeList.Codes.Assigned);

		void TestFinaliseDocketWithPutawayTaskCore(bool isPlannedReceive, bool isPutawayTask, bool withUserConfirmation, bool expectedIsFinalised, string taskStatus, string expectedTaskStatusAfterFinalize)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var dockDoor = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoor = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			if (isPlannedReceive)
			{
				receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			}
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoor, "PLT1");
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoor, nonDockDoor, "PLT1", 10m);
			putawayTransfer.RunPreSaveValidation();

			AssertNotNull("Precondition: Putaway transfer has been created.", putawayTransferLine);
			AssertEquals("Precondition: Receive line's inventory status is Received.", InventoryStatus.Codes.Received, receiveLine.WE_OriginalInventoryStatus);

			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff);
			if (!isPutawayTask)
			{
				task.P9_FormFlowType = string.Empty;
			}
			task.P9_Status = taskStatus;

			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;
			putawayTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertIsFinalisedPrecondition(putawayTransfer);
			AssertEquals("Precondition: receive has no errors", false, receive.HasErrors);
			AssertEquals("Precondition: receive is unfinalized", false, receive.IsFinalised);
			AssertEquals("Precondition", isPlannedReceive, receive.WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned));
			AssertEquals("Precondition", taskStatus, task.P9_Status);

			if (withUserConfirmation)
			{
				Assert("Precondition", Notify.DefaultResponse);
				receive.FinaliseDocket();
			}
			else
			{
				receive.FinaliseDocketWithoutUserConfirmation();
			}
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
			AssertEquals(expectedIsFinalised, receiveInNewFactory.IsFinalised);

			var taskInNewFactory = newFactory.Load<ProcessTask>(task.PK);
			AssertEquals(expectedTaskStatusAfterFinalize, taskInNewFactory.P9_Status);
		}

		public void TestFinaliseDocketWithPutawayTask_WithUserConfirmation_UserCancels()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var dockDoor = data.Whs1.DefaultOutboundDockDoorLocation;
			var nonDockDoor = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoor, "PLT1");
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoor, nonDockDoor, "PLT1", 10m);
			putawayTransfer.RunPreSaveValidation();

			AssertNotNull("Precondition: Putaway transfer has been created.", putawayTransferLine);
			AssertEquals("Precondition: Receive line's inventory status is Received.", InventoryStatus.Codes.Received, receiveLine.WE_OriginalInventoryStatus);

			var task = Helper.CreateProcessTaskForTransfer(putawayTransfer, staff);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			putawayTransferLine.PickedTime = ZDateTimeOffset.Now;
			putawayTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertIsFinalisedPrecondition(putawayTransfer);
			AssertEquals("Precondition: receive has no errors", false, receive.HasErrors);
			AssertEquals("Precondition: receive is unfinalized", false, receive.IsFinalised);
			AssertEquals("Precondition", true, receive.WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned));
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);

			var closePutawayTasksConfirmationMessage = "This Receipt has related active putaway job tasks set to working. The tasks need to be completed to finalize this Receipt.\r\n\r\nDo you wish to close these active tasks set to working to finalize this Receipt?";
			Notify.PreQueryUser += (sender, e) =>
			{
				if (e is DefaultableQueryUserEventArgs args)
				{
					args.Response = !args.Message.Equals(closePutawayTasksConfirmationMessage);
				}
			};

			receive.FinaliseDocket();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
			AssertEquals(false, receiveInNewFactory.IsFinalised);

			var taskInNewFactory = newFactory.Load<ProcessTask>(task.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, taskInNewFactory.P9_Status);
		}

		#endregion

		#endregion

		#region TestFinalisedDateProvider

		[TestDate(2017, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestOverrideFinaliseDate()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var kitReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			kitReceive.WD_WP_ParentPickForReceive = Factory.New<WhsPick>().PK;
			var kitReceiveLine = WhsPickByBOMHelper.NewKitReceiveLine(Factory, kitReceive.PK, data.Part1.PK, 10m);
			WhsPickByBOMHelper.UpdateKitReceiveLineToPUT(kitReceiveLine, location1.PK, GlbStaff.CurrentUser.GS_Code, ZDateTimeOffset.Now);

			var provider = new Provider { FinaliseTime = new ZDateTimeOffset(2017, 2, 2) };
			using (kitReceive.SetFinalisedDateProvider(provider))
			{
				kitReceive.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(kitReceive);
				AssertEquals("Finalised Date is Overriden with the Correct value.",
					new ZDateTimeOffset(2017, 2, 2, 0, 0, 0, kitReceive.WD_FinalisedDate.Offset),
					kitReceive.WD_FinalisedDate);
			}
		}

		public void TestGetFinalisedDateProvider()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			AssertNull(receive.GetFinalisedDateProvider());

			var provider = new Provider { FinaliseTime = ZDateTimeOffset.Now };
			receive.WD_WP_ParentPickForReceive = Factory.New<WhsPick>().PK;
			using (receive.SetFinalisedDateProvider(provider))
			{
				AssertExceptionThrown(typeof(InvalidOperationException), "Attempt to call GetFinalisedDateProvider() without finalising the Receive.", () => receive.GetFinalisedDateProvider());

				using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
				{
					AssertEquals("Provider should be correct.", provider, receive.GetFinalisedDateProvider());
				}
			}
		}

		public void TestSetFinalisedDateProvider()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			AssertExceptionThrown<ArgumentNullException>(() => receive.SetFinalisedDateProvider(null));

			var provider = new Provider { FinaliseTime = ZDateTimeOffset.Now };
			AssertExceptionThrown(typeof(InvalidOperationException), "Can only use a IFinalisedDateProvider for Receives created from Pick by BOM.", () => receive.SetFinalisedDateProvider(provider));

			receive.WD_WP_ParentPickForReceive = Factory.New<WhsPick>().PK;

			using (receive.SetFinalisedDateProvider(provider))
			{
				using (new SemaphoreManager(receive.FinaliseDocketSemaphore))
				{
					AssertEquals("Provider should be correct.", provider, receive.GetFinalisedDateProvider());
				}
			}

			AssertNull("Once the Disposable is Disposed, the Provider should be cleared out.", receive.GetFinalisedDateProvider());
		}

		class Provider : IFinalisedDateProvider
		{
			ZDateTimeOffset IFinalisedDateProvider.GetFinalisationTimeOffset() => FinaliseTime;

			public ZDateTimeOffset FinaliseTime { get; set; }
		}

		#endregion

		#region TestWD_ReceiveCategory

		public void TestWD_ReceiveCategory_Readonly_Finalised()
		{
			var receive = Factory.New<WhsReceive>();

			receive.WD_FinalisedDate = DateTime.Now;
			AssertEquals(true, receive.WD_ReceiveCategoryInfo.ReadOnly);
		}

		public void TestWD_ReceiveCategory_Readonly_Cancelled()
		{
			var receive = Factory.New<WhsReceive>();

			receive.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			AssertEquals(true, receive.WD_ReceiveCategoryInfo.ReadOnly);
		}

		public void TestWD_ReceiveCategory_Readonly_Started_Receiving()
		{
			var receive = Factory.New<WhsReceive>();

			receive.WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
			AssertEquals(true, receive.WD_ReceiveCategoryInfo.ReadOnly);
		}

		public void TestWD_ReceiveCategory_Readonly_Default()
		{
			var receive1 = Factory.New<WhsReceive>();
			AssertEquals(false, receive1.WD_ReceiveCategoryInfo.ReadOnly);
		}

		public void TestWD_ReceiveCategory_Lookups()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(WhsReceive), WhsDocketSchema.Constants.WD_ReceiveCategory, false,
				la => la.ListDataSourceMember == "Lookups.ReceiveCategories");
		}

		#endregion

		#region TestFinalise_AllocatesStockToOrderOnWorkOrder

		public void TestFinalise_AllocatesStockToOrderOnWorkOrder()
		{
			var data = new TestDataForBOM(Factory);

			var order = data.CreateOrderWithBOMShortfall();

			// create work order
			order.BOM.AutoCreateWorkOrders(Notify);
			var workOrder = order.CurrentWorkOrders.ElementAt(0);

			// pick and finalise work order (this will auto-create the receive)
			var pick = Factory.New<WhsPick>();
			pick.PickOrdersWithAllocationMock(new[] { workOrder });
			workOrder.FinaliseDocket();

			// precondition - ensure original *order* was correctly picked
			AssertEquals("Precondition", true, order.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition", 100m, order.Lines[0].PickLineQuantity);
			AssertEquals("Precondition", 10m, order.Lines[0].QuantityNotPicked);
			AssertEquals("Precondition", 110m, order.Lines[0].WE_TransactionQuantity);

			// We have to save it before finalisation, to make sure stock is in DB ready for picking
			Factory.Save();

			// finalise receive
			workOrder.Receive.NotificationManager.Push(Notify); // use test notify object rather than the one on the receive

			var allocationEngineMock = new Mock<IAllocationEngineManager>(MockBehavior.Strict);
			allocationEngineMock.
				Setup(ae =>
					ae.Allocate(
						It.IsAny<WhsPick>(),
						It.Is<INotifications>(n => n == Notify),
						It.IsNotNull<IPickStrategy>(),
						It.IsAny<IEnumerable<WhsPickOrderedInventory>>()))
				.Returns<WhsPick, INotifications, IPickStrategy, IEnumerable<WhsPickOrderedInventory>>((p, n, ps, ordInv) =>
				{
					new AllocateFIFOLegacyMock().Allocate(p, n, ps, ordInv);
					return AllocationResult.AllocatedStock;
				})
				.Verifiable();

			using (ObjectFactory.Substitute(allocationEngineMock.Object))
			{
				workOrder.Receive.FinaliseDocket();
			}

			AssertIsFinalisedPrecondition(workOrder.Receive);

			// finalise receive should automatically allocate received stock to the original order
			AssertEquals(100 + 10m, order.Lines[0].PickLineQuantity);
			AssertEquals(0m, order.Lines[0].QuantityNotPicked);
			AssertEquals(100 + 10m, order.Lines[0].WE_TransactionQuantity);

			workOrder.Receive.Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var pickInOtherFactory = otherFactory.Load<WhsPick>(order.Pick.PK);
			var lineInOtherFactory = pickInOtherFactory.Orders[0].Lines[0];
			AssertEquals(110m, lineInOtherFactory.PickLineQuantity);
			AssertEquals(0m, lineInOtherFactory.QuantityNotPicked);
			AssertEquals(110m, lineInOtherFactory.WE_TransactionQuantity);
			AssertEquals(string.Format("Stock received was automatically allocated to Order {0}.", order.WD_ExternalReference), Notify.LastEvent.Message);
			allocationEngineMock.VerifyAll();
		}

		#endregion

		#region TestReadOnly_WhenReceiveCreatedFromWorkOrder

		public void TestReadOnly_WhenReceiveCreatedFromWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var docket = GetNewBusinessObject();
			docket.WD_WD_ParentDocket = workOrder.PK;

			AssertEquals(false, docket.WD_ArrivalDateInfo.ReadOnly);
			AssertEquals(true, docket.WD_OH_ClientInfo.ReadOnly);
			AssertEquals(true, docket.WD_WW_WhsInfo.ReadOnly);
			AssertEquals(true, docket.TransportCoDocAddress.ReadOnly);
			AssertEquals(true, docket.SupplierDocAddress.ReadOnly);

			AssertEquals(true, docket.WD_DocketSubTypeInfo.ReadOnly);
			AssertEquals(true, docket.WD_ExternalReferenceInfo.ReadOnly);
			AssertEquals(true, docket.WD_ExternalReferenceSplitInfo.ReadOnly);
			AssertEquals(true, docket.WD_CustomerReferenceInfo.ReadOnly);
			AssertEquals(true, docket.WD_DocketStatusInfo.ReadOnly);

			AssertEquals(false, docket.WD_TransportReferenceInfo.ReadOnly);
			AssertEquals(true, docket.VehicleNoInfo.ReadOnly);
			AssertEquals(true, docket.WD_PL_NKCarrierServiceLevelInfo.ReadOnly);
			AssertEquals(true, docket.WD_RS_NKServiceLevelInfo.ReadOnly);
			AssertEquals(true, docket.WD_DropModeInfo.ReadOnly);

			AssertEquals(true, docket.WD_BookingDateInfo.ReadOnly);
			AssertEquals(true, docket.WD_ETDInfo.ReadOnly);
			AssertEquals(true, docket.WD_ETAInfo.ReadOnly);
			AssertEquals(false, docket.WD_ArrivalDateInfo.ReadOnly);

			AssertEquals(true, docket.WD_TotalUnitsInfo.ReadOnly);
			AssertEquals(true, docket.WD_PackagesSentInfo.ReadOnly);
			AssertEquals(true, docket.WD_F3_NKTotalPackTypeInfo.ReadOnly);
			AssertEquals(true, docket.WD_TotalPalletsInfo.ReadOnly);

			AssertEquals(true, docket.DGContactInfo.ReadOnly);

			AssertEquals(false, docket.WD_TotalWeightInfo.ReadOnly);
			AssertEquals(false, docket.WD_TotalWeightUnitInfo.ReadOnly);
			AssertEquals(false, docket.WD_TotalCubicInfo.ReadOnly);
			AssertEquals(false, docket.WD_TotalCubicUnitInfo.ReadOnly);
			AssertEquals(true, docket.WD_TotalUnitsFromLinesInfo.ReadOnly);
		}

		#endregion

		#region TestHasOversAndUnders

		public void TestHasOversAndUnders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			var line = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			AssertEquals("HasOversAndUnders should return false", false, receive.HasOversAndUnders);

			line.InDocketLine.WE_TransactionQuantity = 10m;
			AssertEquals("HasOversAndUnders should return true", true, receive.HasOversAndUnders);

			line.InDocketLine.WE_TransactionQuantity = 5m;
			AssertEquals("HasOversAndUnders should return false", false, receive.HasOversAndUnders);
		}

		#endregion

		#region CanFinaliseDPS

		// More thorough tests at Enterprise.Warehouse.Transactions.GUI.Testing.DPSSecurityProviderTest
		public void TestCannotFinaliseWhenDPSMatched()
		{
			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("WH1", "A");

			var receive = Helper.CreateWhsReceive(client, whs, "R1", Notify);
			receive.WD_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Receive should not be finalised.", false, receive.IsFinalised);
		}

		public void TestCannotFinaliseWhenDPSMatched_CannotShowDialog()
		{
			Env.Security.DpsAllowUpdateToMatched.IsAllowed = false;
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALL"))
			{
				try
				{
					Globals.IsUserInteractive = false;

					var client = Helper.CreateClient();
					var whs = Helper.CreateWarehouse("WH1", "A");

					var receive = Helper.CreateWhsReceive(client, whs, "R1", Notify);
					receive.WD_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
					receive.AllocateLocationsWithMock();

					Assert("Precondition", !Globals.CanShowDialogs);
					Assert("Precondition", receive.IsDPSMovementRestricted());
					receive.FinaliseDocket();
					AssertEquals("Receive should not be finalised.", false, receive.IsFinalised);
					Assert("There should be no error message.", UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
				finally
				{
					Globals.IsUserInteractive = true;
				}
			}
		}

		#endregion

		#region IDocManagerSupport Members

		public override void TestDocManagerInfo()
		{
			var receive = Factory.New<WhsReceive>();
			var docManagerInfo = receive.DocManagerInfo;
			AssertEquals(receive, docManagerInfo.BusinessEntity);
			AssertEquals("WID", docManagerInfo.DocManagerCode);
			AssertEquals(typeof(WhsReceiveDocManagerInfo), docManagerInfo.GetType());
		}

		#endregion

		#region IDocumentSupportable Members

		public void TestDocumentSupporter()
		{
			AssertEquals(typeof(WhsReceiveDocumentSupporter), Receive.DocumentSupporter.GetType());
		}

		#endregion

		#region IDtbBookingParent Members

		public void TestIDtbBookingParent()
		{
			var receive = Factory.New<WhsReceive>();
			var dtbBookingParent = receive as IDtbBookingParent;

			receive.WD_DocketID = "W00000123";
			receive.WD_BOLNo = "456789";
			receive.WD_GoodsDescription = "Pallet of Books";
			AssertEquals("Warehouse Receipt", dtbBookingParent.JobTypeDescription);
			AssertEquals("W00000123", dtbBookingParent.JobNumber);
			AssertEquals(WorkflowDescriptors.WhsReceiveWorkflowDescriptorCode, dtbBookingParent.JobType);

			AssertContainsExactElementsInAnyOrder(new DtbBookingDirection[] { DtbBookingDirection.PIC }, dtbBookingParent.GetSupportedDirections());
		}

		public void TestIDtbBookingParent_ControllerID()
		{
			var receive = Factory.New<WhsReceive>();
			var dtbBookingParent = receive as IDtbBookingParent;
			AssertEquals(ControllerIDs.WhsReceive, dtbBookingParent.ControllerID);
		}

		public void TestCanCreateTransportBooking()
		{
			var receive = Factory.New<WhsReceive>();
			IDtbBookingParent dtbBookingParent = receive;
			AssertEquals("CanCreateTransportBooking should always return true", true, dtbBookingParent.CanCreateTransportBooking);
		}

		public void TestBookingParentPK()
		{
			var receive = Factory.New<WhsReceive>();
			IDtbBookingParent dtbBookingParent = receive;
			AssertEquals("BookingParentPK should be the Receive PK.", receive.PK, dtbBookingParent.BookingParentPK);
		}

		public void TestBookingParentTablePrefix()
		{
			var receive = Factory.New<WhsReceive>();
			IDtbBookingParent dtbBookingParent = receive;
			AssertEquals("BookingParentTablePrefix should be the Receive table prefix.", receive.TablePrefix, dtbBookingParent.BookingParentTablePrefix);
		}

		public void TestGetExtendingConfirmMessageBeforeCreateTransportBooking()
		{
			var receive = Factory.New<WhsReceive>();
			IDtbBookingParent dtbBookingParent = receive;

			var (isShouldShow, caption, message, confirmation) = dtbBookingParent.GetExtendingConfirmMessageBeforeCreateTransportBooking();
			AssertEquals("IsShouldShow should return false.", false, isShouldShow);
			AssertNullOrEmpty("Caption should be null.", caption);
			AssertNullOrEmpty("Message should be null.", message);
			AssertNullOrEmpty("Confirmation should be null.", confirmation);
		}

		#endregion

		#region ITemplateCopyable Members

		public void TestITemplateCopyableTemplateCopy()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1"); // just used to cause duplicate ref error on WD_ExternalReference

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1", Notify);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Three, true);
			receiveLine1.WI_PalletID = "P1";
			receiveLine1.WI_PartAttrib1 = "A11";
			receiveLine1.WI_SerialNumber = "SN24";
			receiveLine2.WI_PartAttrib2 = "A22";
			receiveLine2.WI_PartAttrib3 = "A23";

			receive.WD_DocketID = "W0000001";
			receive.WD_TransportReference = "TR1";
			receive.WD_WD_Split = Factory.New<WhsReceive>().PK;
			receive.WD_ExternalReferenceSplit = 2;
			receive.Containers.AddNew().WC_ContainerNum = "C1";
			receive.Pallets.AddNew();
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			var invoiceType = ObjectFactory.GetType<IWhsInvoice>();
			Factory.NewWithValidTestData(invoiceType);

			var copy = (WhsReceive)((ITemplateCopyable)receive).TemplateCopy();
			AssertEquals("", copy.WD_DocketID);
			AssertEquals("INW1", copy.WD_ExternalReference);
			AssertHasErrors(copy.WD_ExternalReferenceInfo);
			AssertEquals(DocketStatus.Codes.New, copy.WD_DocketStatus);
			AssertEquals("", copy.WD_TransportReference);
			AssertEquals(ZDateTimeOffset.Empty, copy.WD_FinalisedDate);
			AssertEquals(ZDateTimeOffset.Empty, copy.WD_ArrivalDate);
			AssertEquals(ZGuid.Empty, copy.WD_WD_Split);
			AssertEquals((ZByte)0, copy.WD_ExternalReferenceSplit);
			AssertEquals(2, copy.Lines.Count);
			AssertEquals(2, copy.Inventory.Count);
			AssertEquals(0, copy.Containers.Count);
			AssertEquals(0, copy.Pallets.Count);
			AssertEquals(0.04m, copy.WD_TotalCubic);
			AssertEquals(4m, copy.WD_TotalWeight);

			AssertEquals(InventoryStatus.Codes.Pending, copy.Inventory[0].WI_InventoryStatus);
			AssertEquals(ZDateTimeOffset.Empty, copy.Inventory[0].WI_ArrivalDate);
			AssertEquals(ZGuid.Empty, copy.Inventory[0].WI_WL);
			AssertNotEquals(ZGuid.Empty, copy.Inventory[0].WI_WE_InDocketLine);
			AssertEquals(copy.PK, copy.Inventory[0].WI_WD);

			AssertEquals("Don't copy pallet id's", ZString.Empty, copy.Inventory[0].WI_PalletID);
			AssertEquals("Should copy part attribs.", "A11", copy.Inventory[0].WI_PartAttrib1);
			AssertEquals("Don't copy serial numbers", ZString.Empty, copy.Inventory[0].WI_SerialNumber);
			AssertEquals("Should copy part attribs.", "A22", copy.Inventory[1].WI_PartAttrib2);
			AssertEquals("Should copy part attribs.", "A23", copy.Inventory[1].WI_PartAttrib3);

			AssertEquals("Total cubic on source receive should not be changed", 0.04m, receive.WD_TotalCubic);
			AssertEquals("Total weight on source receive should not be updated", 4m, receive.WD_TotalWeight);
		}

		public void TestITemplateCopyable_CopyPickedInventory_DoesNotBreakConstraintOnSave()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			var receiveLine = receive.Lines.Single();
			AssertEquals("Precondition: Receiveline has reduced WE_StockOnHand", 5m, receiveLine.WE_StockOnHand);
			AssertEquals("Precondition: ReceiveLine WE_StockOnHand != WE_TransactionQuantity", true, receiveLine.WE_StockOnHand != receiveLine.WE_TransactionQuantity);

			var copiedReceive = (WhsReceive)((ITemplateCopyable)receive).TemplateCopy();
			copiedReceive.WD_ExternalReference = "Rec2";
			copiedReceive.WD_FinalisedDate = ZDateTimeOffset.Empty;
			AssertNoExceptionThrown("No constraint should fail", () => Factory.Save());
		}

		public void TestITemplateCopyable_CopyPickedInvetory_SOHPackedQtyClientOrdereUnits_SetToTransactionQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			var receiveLine = receive.Lines.Single();
			AssertEquals("Precondition: Receiveline has reduced WE_StockOnHand", 5m, receiveLine.WE_StockOnHand);
			AssertEquals("Precondition: ReceiveLine WE_StockOnHand != WE_TransactionQuantity", true, receiveLine.WE_StockOnHand != receiveLine.WE_TransactionQuantity);

			var copiedReceive = (WhsReceive)((ITemplateCopyable)receive).TemplateCopy();
			var copiedReceiveLine = copiedReceive.Lines.Cast<WhsReceiveLine>().Single();
			CombineAssertions(() =>
			{
				AssertEquals("Cloned ReceiveLine has correct WE_TransactionQuantity", 10m, copiedReceiveLine.WE_TransactionQuantity);
				AssertEquals("Cloned ReceiveLine has correct WE_StockOnHand", 10m, copiedReceiveLine.WE_StockOnHand);
				AssertEquals("Cloned ReceiveLine has correct WE_PackQuantity", 10m, copiedReceiveLine.WE_PackQuantity);
				AssertEquals("Cloned ReceiveLine has correct WE_ClientOrderedUnits", 10m, copiedReceiveLine.WE_ClientOrderedUnits);
			});
		}

		#endregion

		#region ILineToPutawayParentMembers

		public void TestILineToPutawayParentMembers()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			AssertEquals(nameof(ILineToPutawayParent.ClientPK), receive.WD_OH_Client, ((ILineToPutawayParent)receive).ClientPK);
			AssertEquals(nameof(ILineToPutawayParent.WarehousePK), receive.WD_WW_Whs, ((ILineToPutawayParent)receive).WarehousePK);
			AssertEquals(nameof(ILineToPutawayParent.Client), receive.Client, ((ILineToPutawayParent)receive).Client);
			AssertEquals(nameof(ILineToPutawayParent.Warehouse), receive.Warehouse, ((ILineToPutawayParent)receive).Warehouse);
		}

		#endregion

		#region TestRelatedJobs

		protected override List<IRelatedJob> GetValidRelatedJobs(WhsReceive docket)
		{
			var workOrder = Factory.New<WhsWorkOrder>();
			docket.WD_WD_ParentDocket = workOrder.PK;

			var forwardingOrder = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IOrder>();
			var pivot = Factory.New<WhsDocketJobPivot>();
			pivot.WV_ParentId = forwardingOrder.PK;
			pivot.WV_ParentTableCode = forwardingOrder.TablePrefix;
			pivot.WV_DocketType = docket.WD_DocketType;
			pivot.WV_WD_Docket = docket.PK;

			var consol = (IDtbBookingConsolidation)Factory.New(ObjectFactory.GetType<IDtbBookingConsolidation>());
			consol.KB_ParentID = docket.PK;
			consol.KB_ParentTableCode = WhsDocketSchema.Constants.Prefix;

			var booking = (IDtbBooking)Factory.New(ObjectFactory.GetType<IDtbBooking>());
			booking.KM_KB_Booking = consol.PK;
			booking.KM_JobID = "Booking 1";

			return new List<IRelatedJob>(new[] { workOrder, (IRelatedJob)forwardingOrder, (IRelatedJob)booking });
		}

		protected override List<IRelatedJob> GetInvalidRelatedJobs(WhsReceive docket)
			=> new List<IRelatedJob>(new IRelatedJob[] { Factory.New<WhsWorkOrder>(), Factory.New<WhsOrder>() });

		public void TestRelatedJobs_OrderToReturn()
		{
			var order = Factory.New<WhsOrder>();

			Docket.WD_WD_ParentDocket = order.PK;

			AssertCollectionContains(order, Docket.RelatedJobs);
		}

		public void TestRelatedJobs_Shipment()
		{
			var shipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var pivot = Factory.New<WhsDocketJobPivot>();
			pivot.WV_ParentId = shipment.PK;
			pivot.WV_ParentTableCode = shipment.TablePrefix;
			pivot.WV_DocketType = Docket.WD_DocketType;
			pivot.WV_WD_Docket = Docket.PK;

			AssertCollectionContains(shipment, Docket.RelatedJobs);
		}

		#endregion

		#region ISendEmailSource Members

		protected override ZInt ExpectedRecipientCount
		{
			get { return base.ExpectedRecipientCount + 2; }
		}

		protected override void CheckForRecipients(OrgHeader client, OrgHeader consignee, OrgHeader goodsBillTo, OrgHeader supplier, OrgHeader transportCo, OrgHeader transportBillTo, AddressBookSelection selection)
		{
			base.CheckForRecipients(client, consignee, goodsBillTo, supplier, transportCo, transportBillTo, selection);

			AssertRecipient(supplier, selection);
			AssertRecipient(transportCo, selection);
		}

		protected override string ExpectedTemplateCategory
		{
			get { return MailTemplateCategoryList.Codes.WarehouseReceive; }
		}

		#endregion

		#region TestCreateASNLinesAndExpectedQuantity

		public void TestCreateASNLinesAndCalculatingExpectedQuantityInDifferentFactories()
		{
			// create receive and populate ASN lines in one factory
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Factory.Save();
			receive.PopulateASNLines();
			AssertUniqueASNLine(receive.AsnLines, data.Part1, "", "", "", ZDate.Empty, ZDate.Empty, 10m, "UNT", 1, 0);
			receive.Lines.DeleteAll();
			Factory.Save();
			AssertEquals("Precondition", 0, receive.Lines.Count);

			// Finalise the receive in the other factory
			var factoryToFinaliseReceive = new BusinessObjectFactory();
			var receiveLoadedInFactoryToFinaliseReceive = factoryToFinaliseReceive.Load<WhsReceive>(receive.PK);
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receiveLoadedInFactoryToFinaliseReceive, data.Part1, 5m, data.Whs1.DefaultLocation,
					ZDate.Empty, ZDate.Empty, "AT1", "", "", "");
			receiveLoadedInFactoryToFinaliseReceive.FinaliseDocketWithoutUserConfirmation();
			var receiveLine1 = receiveLoadedInFactoryToFinaliseReceive.Lines.Single(line => line.WE_TransactionQuantity == 5m);
			AssertIsFinalisedPrecondition(receiveLoadedInFactoryToFinaliseReceive);
			AssertEquals("Expected Receipt quantity should be populated from ASN line.", 5m, inventoryLine.WI_ExpectedReceiptQuantity);
			AssertEquals("Client ordered units should be same as Expected receipt quantity.", 5m, receiveLine1.WE_ClientOrderedUnits);

			var receiveLine2 = receiveLoadedInFactoryToFinaliseReceive.Lines.Single(line => line.WE_TransactionQuantity == 0m);
			AssertEquals("ASN reconcilliation created a new receiveline with no transaction quantity.", 5m, receiveLine2.WE_ClientOrderedUnits);
			AssertEquals("ASN reconcilliation created a new receiveline with no transaction quantity.", 0m, receiveLine2.WE_TransactionQuantity);

			factoryToFinaliseReceive.Save();
			AssertEquals("Expected Receipt quantity should be populated from ASN line.", 5m, inventoryLine.WI_ExpectedReceiptQuantity);
			AssertEquals("Client ordered units should be same as Expected receipt quantity.", 5m, receiveLine1.WE_ClientOrderedUnits);

			var loadReceiveFactory = new BusinessObjectFactory();
			var receiveLoadedInReceiveFactory = loadReceiveFactory.Load<WhsReceive>(receive.PK);
			AssertEquals("Receive loaded in the receive factory has 2 lines.", 2, receiveLoadedInReceiveFactory.Lines.Count);
			AssertEquals("Expected Receipt quantity should not be reverted back to the receive line quantity.", 5m, receiveLoadedInReceiveFactory.Inventory[0].WI_ExpectedReceiptQuantity);
			AssertEquals("Client ordered units should be same as Expected receipt quantity.", 5m, receiveLoadedInReceiveFactory.Lines.Single(line => line.WE_TransactionQuantity == 5m).WE_ClientOrderedUnits);
			AssertEquals("Line added from asn reconcilliation with expected quantity only.", 5m, receiveLoadedInReceiveFactory.Lines.Single(line => line.WE_TransactionQuantity == 0m).WE_ClientOrderedUnits);
		}

		#endregion

		#region TestNewReceiveLineCreatedFromInventoryDuringFinalisationCorrectly

		public void TestNewReceiveLineCreatedFromInventoryDuringFinalisationCorrectly()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m);
			Factory.Save();
			receive.PopulateASNLines();
			receive.Lines.DeleteAll();
			Factory.Save();
			AssertEquals("Precondition", 0, receive.Lines.Count);

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 25m, data.Whs1.DefaultLocation);
			Factory.Save();
			AssertEquals("Precondition", 1, receive.Lines.Count);

			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertEquals("Invenory sum should still be 25", 25m, receive.Inventory.Cast<WhsInventoryView>().Sum(i => i.WI_TotalUnits));
			AssertEquals("Additional receive line should have been created from ASN lines", 2, receive.Lines.Count);
			AssertEquals("Sum of lines should still be valid", 25m, receive.Lines.Sum(l => l.WE_TransactionQuantity));
			Assert(receive.Lines.All(l => l.WE_DocketLineStatus == DocketLineStatus.Codes.Finalised));
		}

		#endregion

		#region TestASNLines_AreCreated_ForReceiveLinesWithPutawayTransfers

		public void TestASNLines_AreCreated_ForReceiveLinesWithPutawayTransfers()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var dockDoor = data.Whs1.DefaultOutboundDockDoorLocation;
			var location = data.Whs1.DefaultLocation;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Now);
			var receiveLineA = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockDoor, "A");
			var receiveLineB = Helper.CreateWhsReceiveLine(receive, data.Part2, 7m, dockDoor, "B");
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, dockDoor, location, "A", 10m);
			putawayTransfer.RunPreSaveValidation();

			receive.PopulateASNLines();
			AssertEquals("Only 1 ASN line is created.", 2, receive.AsnLines.Count);
			AssertUniqueASNLine(receive.AsnLines, data.Part1, "", "", "", ZDate.Empty, ZDate.Empty, 10m, "UNT", 1, 0);
			AssertUniqueASNLine(receive.AsnLines, data.Part2, "", "", "", ZDate.Empty, ZDate.Empty, 7m, "UNT", 2, 0);
		}

		#endregion

		#region TestDisappearingMileStones

		[TestDate(2013, 5, 6)]
		public void TestDisappearingMileStones()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Factory.Save();
			AssertEquals(6, receive.WorkflowItems.Count);

			//cancel docket
			receive.CancelReactivateDocket();
			Factory.Save();
			AssertEquals("Precondition", true, receive.IsCancelled);
			AssertEquals(2, receive.WorkflowItems.Count);

			// reactivate docket
			receive.CancelReactivateDocket();
			Factory.Save();
			AssertEquals("Precondition", false, receive.IsCancelled);

			// check finalise milestone
			var finaliseMilestone = receive.WorkflowItems.Cast<WhsReceiveProcessTasks>().FirstOrDefault(m => m.P9_SE_NKMilestoneEvent == Events.ItemDocumentJobFinalisedCode);
			AssertNotNull("Finalise Milestone should exist after Receive is cancelled then re-activated.", finaliseMilestone);

			// finalise docket
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			// check workflow items
			AssertEquals(2013, finaliseMilestone.P9_ActualDate.Year);
			AssertEquals(5, finaliseMilestone.P9_ActualDate.Month);
			AssertEquals(6, finaliseMilestone.P9_ActualDate.Day);
			AssertEquals(6, receive.WorkflowItems.Count);
		}

		#endregion

		#region TestFinaliseReceive_WithEmptyInventory

		[UseSnapshotProtection]
		public void TestFinaliseReceive_WithEmptyInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "RCV", data.Part1, 10m, true, false);
			Factory.Save();

			receive.Inventory.RemoveAll();
			AssertNoExceptionThrown(() => receive.FinaliseDocketWithoutUserConfirmation());
		}

		#endregion

		#region TestCreateUnloadTime

		[TestDate(2014, 9, 2, 5, 5, 0)]
		public void TestCreateUnloadTime()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);

			receive.CreateUnloadTime();
			var logsEvents = Helper.FindLogs(receive.Logs, Events.WarehouseReceiptUnloaded);
			AssertEquals("Unloaded event added successfully", 1, logsEvents.Length);
			AssertEquals("Check event time is added correctly", new DateTime(2014, 9, 2, 5, 5, 0), logsEvents[0].SL_EventTime);
		}

		public void TestCreateUnloadTime_WithGivenDateTime()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			var dateTime = new ZDateTimeOffset(2014, 9, 2, 5, 5, 0);

			receive.CreateUnloadTime(dateTime);
			var logsEvents = Helper.FindLogs(receive.Logs, Events.WarehouseReceiptUnloaded);
			AssertEquals("Unloaded event added successfully", 1, logsEvents.Length);
			AssertEquals("Check event time is added correctly", new DateTime(2014, 9, 2, 5, 5, 0), logsEvents[0].SL_EventTime);
		}

		#endregion

		#region IWhsLogEventParent

		protected override string ExpectedEventReferenceParameterType => Constants.EventReferenceParameterTypes.Receive;

		#endregion

		#region IScreeningPartyProvider

		public void TestIScreeningPartyProvider_ScreeningParties_WhenAllOrganisationsConfigured()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transport = Factory.New<OrgHeader>();
			var supplier = Factory.New<OrgHeader>();
			var goodsBillTo = Factory.New<OrgHeader>();
			var pickup = Factory.New<OrgHeader>();
			var dropoff = Factory.New<OrgHeader>();
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var job = new Job.Loader(receive).TryCreate();
			receive.TransportCoPK = transport.PK;
			receive.GoodsBillToPK = goodsBillTo.PK;
			receive.SupplierDocAddress.OrganisationPK = supplier.PK;
			receive.PickUpPK = pickup.PK;
			receive.DropOffPK = dropoff.PK;

			var screeningParties = ((IScreeningPartyProvider)receive).ScreeningParties;

			AssertEquals(8, screeningParties.Length);
			AssertContainsDeniedCandidate("Client", data.Org1, screeningParties);
			AssertContainsDeniedCandidate("Warehouse", data.Whs1.WarehouseAddress.Header, screeningParties);
			AssertContainsDeniedCandidate("Local Client", job.LocalCharges, screeningParties);
			AssertContainsDeniedCandidateAsJobAddress("Transport Company", transport, screeningParties);
			AssertContainsDeniedCandidateAsJobAddress("Goods Billed To Address", goodsBillTo, screeningParties);
			AssertContainsDeniedCandidateAsJobAddress("Supplier Documentary Address", supplier, screeningParties);
			AssertContainsDeniedCandidateAsJobAddress("Pick Up Address", pickup, screeningParties);
			AssertContainsDeniedCandidateAsJobAddress("Drop Off Address", dropoff, screeningParties);
		}

		public void TestIScreeningPartyProvider_ScreeningParties_WhenMinimumConfigured()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			var screeningParties = ((IScreeningPartyProvider)receive).ScreeningParties;

			AssertContainsDeniedCandidate("Client", data.Org1, screeningParties);
			AssertContainsDeniedCandidate("Warehouse", data.Whs1.WarehouseAddress.Header, screeningParties);
		}

		public void TestIScreeningPartyProvider_GetWorstScreeningStatus_WhenOneHasMatched()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			data.Org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			data.Whs1.WarehouseAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

			var worst = receive.GetWorstScreeningStatus();

			AssertEquals(ScreeningStatusesList.Codes.Matched, worst);
		}

		public void TestIScreeningPartyProvider_GetWorstScreeningStatus_WhenUnknownIsWorstInTheList()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			data.Org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			data.Whs1.WarehouseAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;

			var worst = receive.GetWorstScreeningStatus();

			AssertEquals(ScreeningStatusesList.Codes.Unknown, worst);
		}

		public void TestIScreeningPartyProvider_GetWorstScreeningStatus_WhenAllIsPermanentClear()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			data.Org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			data.Whs1.WarehouseAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;

			var worst = receive.GetWorstScreeningStatus();

			AssertEquals(ScreeningStatusesList.Codes.PermanentClear, worst);
		}

		public void TestIScreeningPartyProvider_GetWorstScreeningStatusUnlessManuallyCleared_WhenCurrentStatusIsJobClearedButContainsMatched()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			data.Org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			data.Whs1.WarehouseAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			receive.WD_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;

			var worst = receive.GetWorstScreeningStatusUnlessManuallyCleared();

			AssertEquals(ScreeningStatusesList.Codes.JobCleared, worst);
		}

		#endregion

		#region TestUpdateScreeningStatus_FromScreeningParties

		public void TestUpdateScreeningStatus_FromScreeningParties_OnFactorySave_WhenAllIsPermanentClear()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			data.Whs1.WarehouseAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			AssertEquals("Precondition", ScreeningStatusesList.Codes.NotScreened, receive.WD_ScreeningStatus);
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.Clear, receive.WD_ScreeningStatus);
		}

		public void TestUpdateScreeningStatus_FromScreeningParties_OnFactorySave_WhenOneHasMatched()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			data.Whs1.WarehouseAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			AssertEquals("Precondition", ScreeningStatusesList.Codes.NotScreened, receive.WD_ScreeningStatus);
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.Matched, receive.WD_ScreeningStatus);
		}

		public void TestUpdateScreeningStatus_FromScreeningParties_OnFactorySave_WhenUnknownIsWorstInTheList()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			data.Whs1.WarehouseAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			AssertEquals("Precondition", ScreeningStatusesList.Codes.NotScreened, receive.WD_ScreeningStatus);
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.Unknown, receive.WD_ScreeningStatus);
		}

		public void TestUpdateScreeningStatus_FromScreeningParties_OnFinalise()
		{
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				data.Org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				data.Whs1.WarehouseAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
				receive.AllocateLocationsWithMock();

				AssertEquals("Precondition", false, receive.IsInDatabase);
				AssertEquals("Precondition", ScreeningStatusesList.Codes.NotScreened, receive.WD_ScreeningStatus);
				AssertNotEquals("Precondition", DocketStatus.Codes.Finalised, receive.WD_DocketStatus);
				receive.FinaliseDocketWithoutUserConfirmation();

				AssertEquals("Receive is finalised.", DocketStatus.Codes.Finalised, receive.WD_DocketStatus);
				AssertEquals("Screening status is Clear.", ScreeningStatusesList.Codes.Clear, receive.WD_ScreeningStatus);
				AssertEquals("Receive has no errors.", false, receive.HasErrors);
			}
		}

		public void TestUpdateScreeningStatus_FromScreeningParties_OnFinalise_InitializedWithScreeningStatus()
		{
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				data.Org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				data.Whs1.WarehouseAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				receive.WD_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
				receive.AllocateLocationsWithMock();

				AssertEquals("Precondition", false, receive.IsInDatabase);
				AssertEquals("Precondition", ScreeningStatusesList.Codes.Matched, receive.WD_ScreeningStatus);
				AssertNotEquals("Precondition", DocketStatus.Codes.Finalised, receive.WD_DocketStatus);
				receive.FinaliseDocketWithoutUserConfirmation();

				AssertEquals("Receive is finalised.", DocketStatus.Codes.Finalised, receive.WD_DocketStatus);
				AssertEquals("Screening status is updated from the client screening party.", ScreeningStatusesList.Codes.Clear, receive.WD_ScreeningStatus);
				AssertEquals("Receive has no errors.", false, receive.HasErrors);
			}
		}

		public void TestUpdateScreeningStatus_FromScreeningParties_OnFinalise_ReceiveInDatabase()
		{
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				data.Org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				data.Whs1.WarehouseAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
				receive.AllocateLocationsWithMock();
				Factory.Save();

				AssertEquals("Precondition", true, receive.IsInDatabase);
				AssertEquals("Precondition", ScreeningStatusesList.Codes.Matched, receive.WD_ScreeningStatus);
				AssertNotEquals("Precondition", DocketStatus.Codes.Finalised, receive.WD_DocketStatus);

				receive.WD_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				receive.FinaliseDocketWithoutUserConfirmation();

				AssertEquals("Receive is finalised.", DocketStatus.Codes.Finalised, receive.WD_DocketStatus);
				AssertEquals("Screening status is not updated/retrieved from the client screening party.", ScreeningStatusesList.Codes.Clear, receive.WD_ScreeningStatus);
				AssertEquals("Receive has no errors.", false, receive.HasErrors);
			}
		}

		#endregion

		#region ICreditControlledDocumentDelivery

		public void TestICreditControlledDocumentDelivery_GetScreeningParties_ReturnsSameAsScreeningParties()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			var screeningParties = ((ICreditControlledDocumentDelivery)receive).GetScreeningParties();

			AssertContainsDeniedCandidate("Client", data.Org1, screeningParties);
			AssertContainsDeniedCandidate("Warehouse", data.Whs1.WarehouseAddress.Header, screeningParties);
		}

		protected override void TestIsDPSFreightMovementRestrictedCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var testCases = new[]
			{
				(ScreeningStatusesList.Codes.PermanentClear, false),
				(ScreeningStatusesList.Codes.Clear, false),
				(ScreeningStatusesList.Codes.JobCleared, false),
				(ScreeningStatusesList.Codes.Unknown, true),
				(ScreeningStatusesList.Codes.NotScreened, true),
				(ScreeningStatusesList.Codes.Matched, true),
			};

			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			{
				CombineAssertions(() =>
				{
					foreach (var (screeningStatus, expected) in testCases)
					{
						receive.WD_ScreeningStatus = screeningStatus;

						var restricted = ((ICreditControlledDocumentDelivery)receive).IsDPSFreightMovementRestricted;

						AssertEquals($"Restriction for status {screeningStatus} should be {expected}", expected, restricted);
					}
				});
			}
		}

		protected override bool TestIsDPSFreightMovementRestricted_All => true;

		#endregion

		#region ITaskPlanningJob

		protected override ZString HumanReadableNameWithoutID => "Warehouse Receipt";

		protected override bool SupportsPlanningStatus(WhsDocket docket) => true;

		#endregion

		#region TestReceiveSavingError

		public void TestReceiveSavingError()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);
			var line = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newHelper = new WhsTestHelperFunctions(newFactory);
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
			receiveInNewFactory.Lines.Single().Delete();
			var newLineInNewFactory = newHelper.CreateWhsReceiveInventoryLine(receiveInNewFactory, data.Part1.PK, 10);
			newLineInNewFactory.LocationString = "A";
			receiveInNewFactory.FinaliseDocketWithoutUserConfirmation();
			Assert(receiveInNewFactory.IsFinalised);

			AssertNoExceptionThrown(() => newFactory.Save());
		}

		#endregion

		#region WhsReceiveLineWrapperStrategy

		public void TestBuildWrapperStrategyIsWeb()
		{
			// Arrange
			var originalIsWeb = Globals.IsWeb;
			try
			{
				Globals.IsWeb = true;
				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

				// Action
				var wrapper = receive.WrapperStrategy;

				// Assert
				Assert("Should have created the wrapper.", wrapper != null && !(wrapper is WhsReceive));
			}
			finally
			{
				Globals.IsWeb = originalIsWeb;
			}
		}

		public void TestBuildWrapperStrategyDefault()
		{
			// Arrange
			var originalIsWeb = Globals.IsWeb;
			try
			{
				Globals.IsWeb = false;
				var data = new TestDataSimpleEnvironment(Factory);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

				// Action
				var wrapper = receive.WrapperStrategy;

				// Assert
				Assert("Should have created the wrapper.", wrapper != null && wrapper is WhsReceive);
			}
			finally
			{
				Globals.IsWeb = originalIsWeb;
			}
		}

		#endregion

		#region TestWhsReceive_AllocateLocationsWithMock

		public void TestWhsReceive_AllocateLocationsWithMock()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Factory.Save();

			receive.AllocateLocationsWithMock();

			Assert("Ensure operation worked.", !receiveLine.WE_WL.IsEmpty);
		}

		public void TestWhsReceive_AllocateLocationsWithMock_SelectedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 2);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);

			receive.AllocateLocationsWithMock(new[] { receiveLine });

			Assert("Ensure operation worked.", !receiveLine.WE_WL.IsEmpty);
		}

		#endregion

		#region TestLocationStockOnHandCache

		public void TestLocationStockOnHandCache()
		{
			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(
					   Enterprise.Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);
				var location = data.Whs1.FindLocation("A-1");

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "PLT1");
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, location, "PLT2");
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, location);
				receive2.RunPreSaveValidation();

				var key = GetStockOnHandLocationCacheKey(location.PK, receive2.PK);
				AssertNotNull("Cache should exist on factory",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(key, () => null));

				Factory.Save();
				AssertNull("Cache should not exist on factory after factory save",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(key, () => null));
			}
		}

		public void TestLocationStockOnHandCache_MultipleLocations()
		{
			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(
					   Enterprise.Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);
				var locationA1 = data.Whs1.FindLocation("A-1");
				var locationA2 = data.Whs1.FindLocation("A-2");
				var locationA3 = data.Whs1.FindLocation("A-3");

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, locationA1);
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, locationA1);
				Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 10m, locationA2);
				Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, locationA3);
				receive2.RunPreSaveValidation();

				var key1 = GetStockOnHandLocationCacheKey(locationA1.PK, receive2.PK);
				AssertNotNull("Cache should exist on factory",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(key1, () => null));

				var key2 = GetStockOnHandLocationCacheKey(locationA2.PK, receive2.PK);
				AssertNotNull("Cache should exist on factory",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(key2, () => null));

				var key3 = GetStockOnHandLocationCacheKey(locationA3.PK, receive2.PK);
				AssertNotNull("Cache should exist on factory",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(key3, () => null));

				Factory.Save();
				AssertNull("Cache should not exist on factory after factory save",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(key1, () => null));
				AssertNull("Cache should not exist on factory after factory save",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(key2, () => null));
				AssertNull("Cache should not exist on factory after factory save",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(key3, () => null));
			}
		}

		public void TestLocationStockOnHandCache_MultiplePallets()
		{
			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(
					   Enterprise.Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);
				var location = data.Whs1.FindLocation("A-1");

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "PLT1");
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, location, "PLT2");
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "PLT3");
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, location);
				receive2.RunPreSaveValidation();

				var key = GetStockOnHandLocationCacheKey(location.PK, receive2.PK);
				AssertNotNull("Cache should exist on factory",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(key, () => null));

				Factory.Save();
				AssertNull("Cache should not exist on factory after factory save",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(key, () => null));
			}
		}

		public void TestLocationStockOnHandCache_SOHLocationWarningRegistryDisabled()
		{
			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(
					   Enterprise.Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);
				var location = data.Whs1.FindLocation("A-1");

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location);
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, location, "PLT1");

				AssertEquals("Precondition", false, WarehouseDataRegistry.Instance.SOHLocationWarning.Value);
				receive2.RunPreSaveValidation();

				AssertNull("Cache should not exist on factory",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(
						GetStockOnHandLocationCacheKey(location.PK, receive2.PK), () => null));
			}
		}

		public void TestLocationStockOnHandCache_FinalisedReceive()
		{
			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(
					   Enterprise.Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var data = new TestDataSimpleEnvironment(Factory, 3, 1);
				var location = data.Whs1.FindLocation("A-1");

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location);
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				receive.RunPreSaveValidation();

				AssertNull("Cache should not exist on factory",
					Factory.GetCachedValue<IEnumerable<LocationStockOnHandInfo>>(
						GetStockOnHandLocationCacheKey(location.PK, receive.PK), () => null));
			}
		}

		public void TestDBHits_StockOnHandLocationCache()
		{
			using (WarehouseDataRegistry.Instance.SOHLocationWarning.SetTemporaryValue(
					   Enterprise.Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				const int NumberOfUniqueRecords = 10;
				var data = new TestDataSimpleEnvironment(Factory, 10, 1);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				for (int i = 0; i < NumberOfUniqueRecords; i++)
				{
					Helper.CreateWhsReceiveLine(receive, data.Part1, new ZDecimal(i + 1),
						data.Whs1.FindLocation("A-" + (i + 1)));
				}

				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);

				var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
				for (int i = 0; i < NumberOfUniqueRecords; i++)
				{
					Helper.CreateWhsReceiveLine(receive2, data.Part1, new ZDecimal(i + 1),
						data.Whs1.FindLocation("A-" + (i + 1)));
				}

				Factory.Save();

				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var receiveInOtherFactory = newFactory.Load<WhsReceive>(receive2.PK);
				var expectedDbHits = new Dictionary<string, int>()
				{
					{ JobDocAddressSchema.Constants.TableName, 1 },
					{ JobHeaderSchema.Constants.TableName, 1 },
					{ JobServiceSchema.Constants.TableName, 1 },
					{ ProcessTaskNotificationSchema.Constants.TableName, 1 },
					{ ProcessTasksSchema.Constants.TableName, 1 },
					{ OrgAddressSchema.Constants.TableName, 1 },
					{ OrgCompanyDataSchema.Constants.TableName, 1 },
					{ OrgCustomLabelsSchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgPartRelationSchema.Constants.TableName, 1 },
					{ OrgPartUnitSchema.Constants.TableName, 1 },
					{ OrgSupplierPartSchema.Constants.TableName, 1 },
					{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
					{ ProcessWorkflowExceptionTypeSchema.Constants.TableName, 1 },
					{ RefUNLOCOSchema.Constants.TableName, 1 },
					{ WhsAreaSchema.Constants.TableName, 1 },
					{ WhsAsnLineSchema.Constants.TableName, 1 },
					{ WhsDocketContainerSchema.Constants.TableName, 1 },
					{ WhsDocketPalletSchema.Constants.TableName, 1 },
					{ WhsDocketReferenceSchema.Constants.TableName, 1 },
					{ WhsDocketSchema.Constants.TableName, 2 },
					{ WhsDocketLineSchema.Constants.TableName, 1 },
					{ WhsInventoryViewSchema.Constants.TableName, 1 },
					{ WhsLocationViewSchema.Constants.TableName, 1 },
					{ WhsPickLineSchema.Constants.TableName, 1 },
					{ WhsRowSchema.Constants.TableName, 1 },
					{ WhsWarehouseSchema.Constants.TableName, 1 }
				};

				using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
				{
					receiveInOtherFactory.RunPreSaveValidationWithFetchHints();
					receiveInOtherFactory.FinaliseDocketWithoutUserConfirmation();
				}

				Assert(receiveInOtherFactory.IsFinalised);
			}
		}

		static string GetStockOnHandLocationCacheKey(ZGuid locationPK, ZGuid docketPK)
		{
			return "LocationStockOnHandInfoCache|" + docketPK + "|" + locationPK;
		}

		#endregion

		#region ICriticalChangesVersionID

		protected override void UpdatedDocketVersionAndSubscribeToFactoryCore(BusinessObjectFactory factory, WhsDocket docket)
			=> UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsReceive>.UpdateBusinessObjectVersionAndSubscribeToFactory(factory, docket.PK);

		protected override WhsDocketLine PrepareValidDocketLineForICriticalChangesTestCore(BusinessObjectFactory factory, WhsDocket docket, OrgSupplierPart product, WhsLocation location)
		{
			var receiveLine = Helper.CreateWhsReceiveLine((WhsReceive)docket, product, 10m);
			docket.WD_ArrivalDate = ZDateTimeOffset.Now.AddDays(-7);
			return receiveLine;
		}

		public void TestChangesToCriticalFieldsUpdatesCriticalVersionID()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "AAA");

			whs.WW_UseArrivalDateForInwardsFinalisedDate = ZBool.False;
			var receive = Helper.CreateWhsReceive(org, whs, "1", Notify);
			receive.WD_ArrivalDate = ZDateTimeOffset.Now.AddDays(-7);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, part, 10m);
			receiveLine1.WI_InventoryStatus = CodeLists.InventoryStatus.Codes.Held;

			receive.AllocateLocationsWithMock();
			Factory.Save();

			var changed = 0;
			receive.WD_CriticalChangesVersionIDInfo.ValueChanged += (object sender, EventArgs e) =>
			{
				changed++;
			};

			receive.WD_BookingDate = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Should increment calls to update.", 1, changed);
			Assert("Should update to valid Guid.", receive.WD_CriticalChangesVersionID.IsValid);

			receive.FinaliseDocket();
			var today = ZDateTimeOffset.Today;
			AssertEquals(true, receive.WD_FinalisedDate.Date == today.Date);

			Factory.Save();
			AssertEquals("Should increment calls to update.", 2, changed);
			Assert("Should update to empty Guid.", receive.WD_CriticalChangesVersionID.IsEmpty);
		}

		public void TestSaveOrDeleteOfAttachedWhsReceiveLineUpdatesCriticalVersionID()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "AAA");

			var receive = Helper.CreateWhsReceive(org, whs, "1", Notify);
			receive.WD_ArrivalDate = ZDateTimeOffset.Now.AddDays(-7);

			Factory.Save();

			var changed = 0;
			receive.WD_CriticalChangesVersionIDInfo.ValueChanged += (object sender, EventArgs e) =>
			{
				changed++;
			};

			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, part, 10m);
			receiveLine1.WI_InventoryStatus = CodeLists.InventoryStatus.Codes.Held;

			receive.AllocateLocationsWithMock();
			Factory.Save();

			AssertEquals("Should increment calls to update.", 1, changed);
			Assert("Should update to valid Guid.", receive.WD_CriticalChangesVersionID.IsValid);

			receiveLine1.Delete();

			Factory.Save();
			AssertEquals("Should increment calls to update.", 2, changed);
			Assert("Should update to valid Guid.", receive.WD_CriticalChangesVersionID.IsValid);
		}

		#endregion

		#region TestCheckWB_EntryKeyCached

		public void TestCheckWB_EntryKeyCached()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Factory.Save();

			var location = data.Whs1.DefaultLocation;
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location);
			receiveLine1.CustomsData.WB_EntryKey = "E01";

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location);
			receiveLine2.CustomsData.WB_EntryKey = "E01";

			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location);
			receiveLine3.CustomsData.WB_EntryKey = "E01";

			receive.RunPreSaveValidation();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);

			using (TestConnection.TrackExecutedCommands())
			using (RowFactory.SetCachedTables())
			{
				receiveInNewFactory.RunPreSaveValidation();
				AssertEquals("Should have only hit the DB once during validation.", 1, TestConnection.ExecutedCommands.Count(c => c.Contains("WB_ParentID IN (SELECT Value FROM @DocketLineParentPKs)")));
				AssertEquals("There should be 1 table hit count for loading all customs data.", 1, newFactory.TableSelects.Single(t => t.TableName == WhsBondedWarehouseAttributeSchema.Constants.TableName).Value);
			}
		}

		#endregion

		#region TestReceiveUpdateByDataRefresh

		protected override void TestDocketUpdatedByDataRefresh_FinalisedInMemoryCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			receiveInNewFactory.WD_ExternalReference = "NEWR1";
			AssertEquals(false, receiveInNewFactory.IsFinalised);
			newFactory.Save();

			AssertEquals("Receive is still finalised after data refresh.", true, receive.IsFinalised);
			AssertEquals("External reference is updated after data refresh.", "NEWR1", receive.WD_ExternalReference);
			Helper.AssertZCannotSaveExceptionThrown("The Receive has been updated by another job. Please reload the Receive.", Factory.Save);
		}

		protected override void TestDocketUpdatedByDataRefresh_CriticalChangesVersionIDUpdateCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receive.WD_CriticalChangesVersionID = ZGuid.NewZGuid();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);

			receive.AllocateLocationsWithMock();

			var newCriticalChangesVersionID = ZGuid.NewZGuid();
			AssertNotEquals(receive.WD_CriticalChangesVersionID, newCriticalChangesVersionID);

			receiveInNewFactory.WD_CriticalChangesVersionID = newCriticalChangesVersionID;
			newFactory.Save();

			AssertEquals("WD_CriticalChangesVersionID is updated after data refresh.", newCriticalChangesVersionID, receive.WD_CriticalChangesVersionID);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should have the 'need to reload' notification", true, Notify.ContainsNotificationType(WhsErrorTypes.CannotFinaliseWithoutReload));
		}

		#endregion

		#region TestWD_HoldPalletIDPutaway

		public void TestWD_HoldPalletIDPutaway_DefaultValue()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Factory.Save();
			AssertEquals("The default value should be false", false, receive.WD_HoldPalletIDPutaway);
		}

		public void TestWD_HoldPalletIDPutaway_SetValue()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_HoldPalletIDPutaway = true;
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Factory.Save();
			AssertEquals("Now the value of the property should be true", true, receive.WD_HoldPalletIDPutaway);
		}

		public void TestOnFinaliseSucceeded_HoldPalletIDPutaway()
		{
			var client = Helper.CreateClient("Client");
			var whs = Helper.CreateWarehouse("Whs", "A", 4, 4);
			var product = Helper.CreateProduct("Product", client);
			var receive1 = Helper.CreateWhsReceive(client, whs, "receive1");
			receive1.WD_HoldPalletIDPutaway = true;
			Helper.CreateWhsReceiveLine(receive1, product, 10m);
			receive1.AllocateLocationsWithMock();
			Factory.Save();

			receive1.FinaliseDocket();
			AssertEquals("receive.WD_DocketStatus", DocketStatus.Codes.Finalised, receive1.WD_DocketStatus);
			AssertEquals("receive.HoldPalletIDPutaway should be set to false", false, receive1.WD_HoldPalletIDPutaway);
		}

		#endregion

		#region TestWD_UnloadCompletedTime

		public void TestWD_UnloadCompletedTime_DefaultValue()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Factory.Save();
			AssertEquals("The default value should be empty", true, receive.WD_UnloadCompletedTime.IsEmpty);
		}

		public void TestWD_UnloadCompletedTime_OnFinaliseSucceeded()
		{
			var client = Helper.CreateClient("Client");
			var whs = Helper.CreateWarehouse("Whs", "A", 4, 4);
			var product = Helper.CreateProduct("Product", client);

			var receive = Helper.CreateWhsReceive(client, whs, "R1");
			Helper.CreateWhsReceiveLine(receive, product, 10m);
			receive.AllocateLocationsWithMock();
			Factory.Save();

			receive.FinaliseDocket();
			AssertEquals("receive.WD_DocketStatus", DocketStatus.Codes.Finalised, receive.WD_DocketStatus);
			AssertEquals("receive.WD_UnloadCompletedTime should be set to WD_FinalisedDate", receive.WD_FinalisedDate, receive.WD_UnloadCompletedTime);
		}

		public void TestWD_UnloadCompletedTime_OnFinaliseSucceeded_AlreadySet()
		{
			var testTime = DateTimeOffset.Now.AddDays(-1);
			var client = Helper.CreateClient("Client");
			var whs = Helper.CreateWarehouse("Whs", "A", 4, 4);
			var product = Helper.CreateProduct("Product", client);

			var receive = Helper.CreateWhsReceive(client, whs, "R1");
			Helper.CreateWhsReceiveLine(receive, product, 10m);
			receive.AllocateLocationsWithMock();
			receive.WD_UnloadCompletedTime = testTime;
			Factory.Save();

			receive.FinaliseDocket();
			AssertEquals("receive.WD_DocketStatus", DocketStatus.Codes.Finalised, receive.WD_DocketStatus);
			AssertEquals("receive.WD_UnloadCompletedTime should be set to correct previous value", testTime, receive.WD_UnloadCompletedTime);
			AssertNotEquals("receive.WD_FinalisedDate should NOT be the same value", testTime, receive.WD_FinalisedDate);
		}

		#endregion

		#region TestLockReceiveAfterUnloadCompletedAndPutawayHoldCleared

		public void TestLockReceiveAfterUnloadCompletedAndPutawayHoldCleared()
		{
			var client = Helper.CreateClient("Client");
			var whs = Helper.CreateWarehouse("Whs", "A", 4, 4);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			Factory.Save();

			var receive = Helper.CreateWhsReceive(client, whs, "R1");
			AssertEquals("Precondition", false, receive.WD_HoldPalletIDPutaway);
			AssertEquals("Precondition", false, receive.WD_UnloadCompletedTime.IsValid);
			AssertEquals("Precondition", false, receive.LockReceiveAfterUnloadCompletedAndPutawayHoldCleared);

			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			AssertEquals("Should be true if WD_UnloadCompletedTime set", true, receive.LockReceiveAfterUnloadCompletedAndPutawayHoldCleared);

			receive.WD_HoldPalletIDPutaway = true;
			AssertEquals("Should be false if WD_HoldPalletIDPutaway set", false, receive.LockReceiveAfterUnloadCompletedAndPutawayHoldCleared);
			receive.WD_HoldPalletIDPutaway = false;
			AssertEquals("Should be set to true when WD_UnloadCompletedTime should and WD_HoldPalletIDPutaway unset", true, receive.LockReceiveAfterUnloadCompletedAndPutawayHoldCleared);

			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Empty;
			AssertEquals("Should be false if WD_UnloadCompletedTime clear", false, receive.LockReceiveAfterUnloadCompletedAndPutawayHoldCleared);
		}

		#endregion

		#region TestSave_PreCreatePutawayTransfer

		#region TestSave_PreCreatePutawayTransfer_UnloadCompletedTime

		public void TestSave_PreCreatePutawayTransfer_UnloadCompletedTimeIsInvalid()
		{
			TestSave_PreCreatePutawayTransfer_UnloadCompletedTimeCore(false);
		}

		public void TestSave_PreCreatePutawayTransfer_UnloadCompletedTimeIsValid()
		{
			TestSave_PreCreatePutawayTransfer_UnloadCompletedTimeCore(true);
		}

		void TestSave_PreCreatePutawayTransfer_UnloadCompletedTimeCore(bool unloadCompletedTimeIsValid)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "";
			receive.WD_UnloadCompletedTime = unloadCompletedTimeIsValid ? ZDateTimeOffset.Now : ZDateTimeOffset.Empty;
			receive.WD_HoldPalletIDPutaway = false;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-2");
			Factory.Save();

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, data.Org1.PK);
			var transfers = Factory.Load<WhsTransfer>(query);

			AssertEquals(unloadCompletedTimeIsValid ? 1 : 0, transfers.Length);
		}

		public void TestSave_PreCreatePutawayTransfer_ChangeUnloadCompletedTime()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "";
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-2");
			Factory.Save();

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, data.Org1.PK);
			var transfers = Factory.Load<WhsTransfer>(query);
			AssertEquals(0, transfers.Length);

			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			Factory.Save();

			transfers = Factory.Load<WhsTransfer>(query);
			AssertEquals(1, transfers.Length);
		}

		#endregion

		#region TestSave_PreCreatePutawayTransfer_HoldPalletIDPutaway

		public void TestSave_PreCreatePutawayTransfer_HoldPalletIDPutawayIsTrue()
		{
			TestSave_PreCreatePutawayTransfer_HoldPalletIDPutawayCore(true);
		}

		public void TestSave_PreCreatePutawayTransfer_HoldPalletIDPutawayIsFalse()
		{
			TestSave_PreCreatePutawayTransfer_HoldPalletIDPutawayCore(false);
		}

		void TestSave_PreCreatePutawayTransfer_HoldPalletIDPutawayCore(bool holdPalletIDPutaway)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "";
			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			receive.WD_HoldPalletIDPutaway = holdPalletIDPutaway;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-2");
			Factory.Save();

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, data.Org1.PK);
			var transfers = Factory.Load<WhsTransfer>(query);

			AssertEquals(holdPalletIDPutaway ? 0 : 1, transfers.Length);
		}

		public void TestSave_PreCreatePutawayTransfer_HoldPalletIDPutawayChange()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "";
			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			receive.WD_HoldPalletIDPutaway = true;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-2");
			Factory.Save();

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, data.Org1.PK);
			var transfers = Factory.Load<WhsTransfer>(query);
			AssertEquals(0, transfers.Length);

			receive.WD_HoldPalletIDPutaway = false;
			Factory.Save();
			transfers = Factory.Load<WhsTransfer>(query);
			AssertEquals(1, transfers.Length);
		}

		#endregion

		#region TestSave_PreCreatePutawayTransfer_IsFinalised

		public void TestSave_PreCreatePutawayTransfer_Finalised()
		{
			TestSave_PreCreatePutawayTransfer_IsFinalisedCore(true);
		}

		public void TestSave_PreCreatePutawayTransfer_NotFinalised()
		{
			TestSave_PreCreatePutawayTransfer_IsFinalisedCore(false);
		}

		void TestSave_PreCreatePutawayTransfer_IsFinalisedCore(bool isFinalised)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "";
			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Empty;
			receive.WD_HoldPalletIDPutaway = false;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 4m);
			receiveLine.WE_PalletID = "PLT-1";
			Factory.Save();

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, data.Org1.PK);
			var transfers = Factory.Load<WhsTransfer>(query);
			AssertEquals(0, transfers.Length);

			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			if (isFinalised)
			{
				receiveLine.WE_WL = data.Whs1.DefaultLocation.PK;
				receive.FinaliseDocketWithoutUserConfirmation();
			}
			else
			{
				receiveLine.WE_WL = data.Whs1.DefaultInboundDockDoorLocation.PK;
			}
			Factory.Save();

			AssertEquals(isFinalised, receive.IsFinalisedOrCancelled);
			transfers = Factory.Load<WhsTransfer>(query);
			AssertEquals(isFinalised ? 0 : 1, transfers.Length);
		}

		#endregion

		#region TestSave_PreCreatePutawayTransfer_IsCancelled

		public void TestSave_PreCreatePutawayTransfer_Cancelled()
		{
			TestSave_PreCreatePutawayTransfer_IsCancelledCore(true);
		}

		public void TestSave_PreCreatePutawayTransfer_NotCancelled()
		{
			TestSave_PreCreatePutawayTransfer_IsCancelledCore(false);
		}

		void TestSave_PreCreatePutawayTransfer_IsCancelledCore(bool isCancelled)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "";
			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Empty;
			receive.WD_HoldPalletIDPutaway = false;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 4m);
			receiveLine.WE_PalletID = "PLT-1";
			receiveLine.WE_WL = data.Whs1.DefaultInboundDockDoorLocation.PK;
			Factory.Save();

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, data.Org1.PK);
			var transfers = Factory.Load<WhsTransfer>(query);
			AssertEquals(0, transfers.Length);

			if (isCancelled)
			{
				receive.CancelReactivateDocket();
			}

			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals(isCancelled, receive.IsFinalisedOrCancelled);
			transfers = Factory.Load<WhsTransfer>(query);
			AssertEquals(isCancelled ? 0 : 1, transfers.Length);
		}

		#endregion

		#region TestSave_PreCreatePutawayTransfer_TaskManagement

		public void TestSave_PreCreatePutawayTransfer_TaskManagementEnabled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "";
			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			receive.WD_HoldPalletIDPutaway = false;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-2");
			Factory.Save();

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, data.Org1.PK);
			var transfers = Factory.Load<WhsTransfer>(query);

			AssertEquals(1, transfers.Length);
			var transfer = transfers[0];
			AssertEquals(TaskPlanningStatus.Codes.Planned, transfer.WD_TaskPlanningStatus);

			var transferLines = transfer.Lines;
			var transferLine1 = transferLines.Single(x => x.WE_OP == data.Part1.PK);
			CombineAssertions(() =>
			{
				AssertEquals(10m, transferLine1.WE_TransactionQuantity);
				AssertEquals(data.Whs1.WW_DefaultInboundDockDoor, transferLine1.WE_WL_TransferFrom);
				AssertEquals("PLT-1", transferLine1.WE_TransferFromPalletId);
			});

			var transferLine2 = transferLines.Single(x => x.WE_OP == data.Part2.PK);
			CombineAssertions(() =>
			{
				AssertEquals(20m, transferLine2.WE_TransactionQuantity);
				AssertEquals(data.Whs1.WW_DefaultInboundDockDoor, transferLine2.WE_WL_TransferFrom);
				AssertEquals("PLT-2", transferLine2.WE_TransferFromPalletId);
			});
		}

		public void TestSave_PreCreatePutawayTransfer_TaskManagementDisabled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_GG_ReleaseGroup = Guid.Empty;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "";
			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			receive.WD_HoldPalletIDPutaway = false;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-2");
			Factory.Save();

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, data.Org1.PK);
			var transfers = Factory.Load<WhsTransfer>(query);

			AssertEquals(0, transfers.Length);
		}

		#endregion

		#region TestSave_PreCreatePutawayTransfer_PalletID

		public void TestSave_PreCreatePutawayTransfer_ValidPalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "";
			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			receive.WD_HoldPalletIDPutaway = false;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-2");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 30m, data.Whs1.DefaultInboundDockDoorLocation, "");
			Factory.Save();

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, data.Org1.PK);
			var transfers = Factory.Load<WhsTransfer>(query);

			AssertEquals(1, transfers.Length);
			var transfer = transfers[0];

			AssertEquals(2, transfer.Lines.Count);
			AssertArrayEqualsByElements(["PLT-1", "PLT-2"], transfer.Lines.Select(s => s.WE_PalletID).OrderBy(s => s).ToArray());
		}

		public void TestSave_PreCreatePutawayTransfer_EmptyPalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "";
			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			receive.WD_HoldPalletIDPutaway = false;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, data.Whs1.DefaultInboundDockDoorLocation, "");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 30m, data.Whs1.DefaultInboundDockDoorLocation, "");
			Factory.Save();

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, data.Org1.PK);
			var transfers = Factory.Load<WhsTransfer>(query);

			AssertEquals(0, transfers.Length);
		}

		public void TestSave_PreCreatePutawayTransfer_PalletIDDifferentCase()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "";
			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			receive.WD_HoldPalletIDPutaway = false;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, data.Whs1.DefaultInboundDockDoorLocation, "plt-1");
			Factory.Save();

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, data.Org1.PK);
			var transfers = Factory.Load<WhsTransfer>(query);
			AssertEquals(1, transfers.Length);

			var transferLines = transfers.Single().Lines;
			CombineAssertions(() =>
			{
				AssertEquals(2, transfers.Single().Lines.Count);
				AssertArrayEqualsByElements([10, 20], transferLines.Where(w => w.WE_PalletID.ToUpper() == "PLT-1").Select(s => s.WE_TransactionQuantity).OrderBy(s => s).ToArray());
			});
		}

		#endregion

		#region TestSave_PreCreatePutawayTransfer_LinkReceiveLines

		public void TestSave_PreCreatePutawayTransfer_LinkReceiveLines_SinglePallet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive1.WD_ExternalReference = "";
			var inventory11 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 11m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive2.WD_ExternalReference = "";
			var inventory21 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 21m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-2");
			Factory.Save();

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive3.WD_ExternalReference = "";
			receive3.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			receive3.WD_HoldPalletIDPutaway = false;
			var inventory31 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 31m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Factory.Save();

			AssertEquals("PFU", inventory11.InDocketLine.WE_DocketLineStatus);
			AssertEquals("", inventory21.InDocketLine.WE_DocketLineStatus);
			AssertEquals("PFU", inventory31.InDocketLine.WE_DocketLineStatus);

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, data.Org1.PK);
			var transfers = Factory.Load<WhsTransfer>(query);

			AssertEquals(1, transfers.Length);
			var transfer = transfers[0];
			var transferLines = transfer.Lines;
			AssertEquals(2, transferLines.Count);
			AssertEquals(true, transferLines.All(s => s.WE_DocketLineStatus == "HFT"));
			AssertArrayEqualsByElements([11, 31], transferLines.Where(w => w.WE_OP == data.Part1.PK).Select(s => s.WE_TransactionQuantity).ToArray());
		}

		public void TestSave_PreCreatePutawayTransfer_LinkReceiveLines_MultiplePallet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive1.WD_ExternalReference = "";
			var inventory11 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 11m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			var inventory12 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 12m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-2");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive2.WD_ExternalReference = "";
			var inventory21 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 21m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			var inventory22 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 22m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-2");

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive3.WD_ExternalReference = "";
			var inventory31 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 31m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-3");
			var inventory32 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part2, 32m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-4");
			Factory.Save();

			var receive4 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive4.WD_ExternalReference = "";
			receive4.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			receive4.WD_HoldPalletIDPutaway = false;
			var inventory41 = Helper.CreateWhsReceiveInventoryLine(receive4, data.Part1, 41m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			var inventory42 = Helper.CreateWhsReceiveInventoryLine(receive4, data.Part2, 42m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-2");
			Factory.Save();

			AssertEquals("PFU", inventory11.InDocketLine.WE_DocketLineStatus);
			AssertEquals("PFU", inventory12.InDocketLine.WE_DocketLineStatus);
			AssertEquals("PFU", inventory21.InDocketLine.WE_DocketLineStatus);
			AssertEquals("PFU", inventory22.InDocketLine.WE_DocketLineStatus);
			AssertEquals("", inventory31.InDocketLine.WE_DocketLineStatus);
			AssertEquals("", inventory32.InDocketLine.WE_DocketLineStatus);
			AssertEquals("PFU", inventory41.InDocketLine.WE_DocketLineStatus);
			AssertEquals("PFU", inventory42.InDocketLine.WE_DocketLineStatus);

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, data.Org1.PK);
			var transfers = Factory.Load<WhsTransfer>(query);

			AssertEquals(1, transfers.Length);
			var transfer = transfers[0];
			var transferLines = transfer.Lines;
			AssertEquals(6, transferLines.Count);
			AssertEquals(true, transferLines.All(s => s.WE_DocketLineStatus == "HFT"));
			AssertArrayEqualsByElements([11, 21, 41], transferLines.Where(w => w.WE_OP == data.Part1.PK).Select(s => s.WE_TransactionQuantity).OrderBy(s => s).ToArray());
			AssertArrayEqualsByElements([12, 22, 42], transferLines.Where(w => w.WE_OP == data.Part2.PK).Select(s => s.WE_TransactionQuantity).OrderBy(s => s).ToArray());
		}

		public void TestSave_PreCreatePutawayTransfer_LinkReceiveLines_PalletIDIgnoreCase()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive1.WD_ExternalReference = "";
			var inventory11 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 11m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive2.WD_ExternalReference = "";
			var inventory21 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 21m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-2");
			Factory.Save();

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive3.WD_ExternalReference = "";
			receive3.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			receive3.WD_HoldPalletIDPutaway = false;
			var inventory31 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 31m, data.Whs1.DefaultInboundDockDoorLocation, "plt-1");
			Factory.Save();

			AssertEquals("PFU", inventory11.InDocketLine.WE_DocketLineStatus);
			AssertEquals("", inventory21.InDocketLine.WE_DocketLineStatus);
			AssertEquals("PFU", inventory31.InDocketLine.WE_DocketLineStatus);

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, data.Org1.PK);
			var transfers = Factory.Load<WhsTransfer>(query);

			AssertEquals(1, transfers.Length);
			var transfer = transfers[0];
			var transferLines = transfer.Lines;
			AssertEquals(2, transferLines.Count);
			AssertEquals(true, transferLines.All(s => s.WE_DocketLineStatus == "HFT"));
			AssertArrayEqualsByElements([11, 31], transferLines.Where(w => w.WE_OP == data.Part1.PK).Select(s => s.WE_TransactionQuantity).ToArray());
			AssertArrayEqualsByElements(["PLT-1", "plt-1"], transferLines.Where(w => w.WE_OP == data.Part1.PK).Select(s => s.WE_PalletID).ToArray());
		}

		#endregion

		#region TestSave_PreCreatePutawayTransfer_ReceiveToLocation

		public void TestSave_PreCreatePutawayTransfer_ReceiveToLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "";
			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			receive.WD_HoldPalletIDPutaway = false;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-2");
			Factory.Save();

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, data.Org1.PK);
			var transfers = Factory.Load<WhsTransfer>(query);

			AssertEquals(1, transfers.Length);
		}

		#endregion

		#region TestSave_PreCreatePutawayTransfer_TransferCreationFailed

		public void TestSave_PreCreatePutawayTransfer_TransferCreationFailed_UNDG()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var location = data.Whs1.FindLocation("A-1");
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var undgCode = "AAA";
			var undgClass = "1";
			var substance = Helper.CreateUNDGSubstance("1234", "1234", undgCode);
			substance.DG_Standard = "IMO";
			Helper.CreateUNDGDataItem(data.Part1, substance, 1m, "KG", 1m, "M3", substance.DG_Class);
			Helper.CreateUNDGSubstancePivot(data.Part1.PK, "DI", substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var reference = Helper.CreateCountryReference(referenceCode: "AU");
			Helper.CreateUNDGCountryReferencePivot(reference.PK, substance.DG_UNNO, substance.DG_Variant, substance.DG_Standard);

			var warehouse = data.Whs1;
			warehouse.WW_DGThresholdPercentage = 50;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;
			var undgLimit1 = Helper.CreateWhsUNDGLimit(warehouse, undgCode, totalWeightLimit: 10m, totalVolumeLimit: 10m);
			warehouse.UNDGLimits.Add(undgLimit1);

			var undgLimit2 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, string.Empty, reference, totalWeightLimit: 10m, totalVolumeLimit: 10m);
			warehouse.UNDGLimits.Add(undgLimit2);

			var undgLimit3 = Helper.CreateWhsUNDGLimit(warehouse, (UNDGSubstance)null, undgClass, null, totalWeightLimit: 10m, totalVolumeLimit: 10m);
			warehouse.UNDGLimits.Add(undgLimit3);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_ArrivalDate = ZDateTimeOffset.Now;
			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			receive.WD_HoldPalletIDPutaway = false;
			Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, "Pallet-1", 15m);
			Helper.AssertZCannotSaveExceptionThrown(@"The following UNDG Limits will exceed 100% of warehouse capacity limit by putting away this job:
Substance Code 'AAA'
Country Reference 'AU'
Class Code '1'
", Factory.Save);
			Helper.AssertZCannotSaveExceptionThrown(@"Cannot save as an error occurred during Saving. Please reload the Receive.", Factory.Save);
		}

		#endregion

		#region TestSave_PreCreatePutawayTransfer_DirectUnload

		public void TestSave_PreCreatePutawayTransfer_DirectUnload()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_ExternalReference = "";
			receive.WD_UnloadCompletedTime = ZDateTimeOffset.Now;
			receive.WD_HoldPalletIDPutaway = false;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, data.Whs1.DefaultLocation, "PLT-2");
			Factory.Save();

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, data.Org1.PK);
			var transfers = Factory.Load<WhsTransfer>(query);

			CombineAssertions(() =>
			{
				AssertEquals("putaway transfer is pre created", 1, transfers.Length);
				AssertNotNull("putaway transfer line is created for unload to dockdoor", inventory1.PutawayTransferLine);
				AssertNull("putaway transfer line is not created for unload to location", inventory2.PutawayTransferLine);
			});
		}

		#endregion

		#endregion

		public void TestIsReturnReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);

			AssertNotEquals(ReceiveType.Codes.Returns, receive.WD_DocketSubType);
			AssertEquals(false, receive.IsReturnReceive);

			receive.WD_DocketSubType = ReceiveType.Codes.Returns;
			Assert(receive.IsReturnReceive);
		}

		public void TestIsLinkedReturnReceiveReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, Notify);

			AssertNotEquals(ReceiveType.Codes.Returns, receive.WD_DocketSubType);
			AssertEquals(ZGuid.Empty, receive.WD_WD_ParentDocket);
			AssertEquals(false, receive.WD_ExternalReferenceInfo.ReadOnly);

			receive.WD_DocketSubType = ReceiveType.Codes.Returns;
			AssertEquals(false, receive.WD_ExternalReferenceInfo.ReadOnly);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			receive.WD_WD_ParentDocket = order.PK;
			AssertEquals(true, receive.WD_ExternalReferenceInfo.ReadOnly);
		}

		public void TestGetExistingReturnReceivesForParentOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "O1");
			receive1.WD_DocketSubType = ReceiveType.Codes.Returns;
			receive1.WD_ExternalReferenceSplit = 1;
			receive1.WD_WD_ParentDocket = order.PK;

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "O1");
			receive2.WD_DocketSubType = ReceiveType.Codes.Returns;
			receive2.WD_ExternalReferenceSplit = 2;
			receive2.WD_WD_ParentDocket = order.PK;

			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive3.WD_DocketSubType = ReceiveType.Codes.Returns;
			receive3.WD_WD_ParentDocket = order.PK;
			Factory.Save();

			var returnReceivePKsForParentOrderWithOrderReference = WhsReceive.GetExistingReturnReceivesForParentOrderWithOrderReference(order).Select(r => r.PK);
			AssertEquals(2, returnReceivePKsForParentOrderWithOrderReference.Count());
			AssertContainsExactElementsInAnyOrder(new[] { receive1.PK, receive2.PK }, returnReceivePKsForParentOrderWithOrderReference);
		}

		public void TestGetNextMaxExternalReferenceSplitFromReceiveCollection()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "O1");
			receive1.WD_DocketSubType = ReceiveType.Codes.Returns;
			receive1.WD_ExternalReferenceSplit = 1;
			receive1.WD_WD_ParentDocket = order.PK;

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "O1");
			receive2.WD_ExternalReferenceSplit = 7;
			Factory.Save();

			AssertEquals((ZByte)8, WhsReceive.GetNextMaxExternalReferenceSplitFromReceiveCollection(new[] { receive1, receive2 }));
		}

		public void TestReturnReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order123");
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "");
			receive.WD_DocketSubType = ReceiveType.Codes.Returns;
			receive.WD_ExternalReference = "Order123";
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part2, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			Assert(receive.IsInDatabase);
			Assert(receive.IsFinalised);
			AssertEquals(order.PK, receive.WD_WD_ParentDocket);
		}

		#region Implementation

		protected WhsReceive Receive
		{
			get => Docket;
			set => Docket = value;
		}

		protected override ControllerID ExpectedControllerId
		{
			get { return ControllerIDs.WhsReceive; }
		}

		protected override ZString ExpectedDescription
		{
			get { return "Receive"; }
		}

		protected override DataContextType? ExpectedDataContextType => DataContextType.WarehouseReceive;

		protected override string WorkflowDescriptorCode
		{
			get { return WorkflowDescriptors.WhsReceiveWorkflowDescriptorCode; }
		}

		protected override WhsReceive GetDocketForRating()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			return Helper.CreateWhsReceive(data.Org1, data.Whs1);
		}

		#endregion
	}

	#endregion

	#region WhsReceiveProcessTasksProviderTest

	[TestedType(typeof(WhsReceive))]
	public class WhsReceiveProcessTasksProviderTest : WhsDocketProcessTasksProviderTest<WhsReceive>
	{
		public void TestGetTemplateFilterCriteria_ReceiveCategory()
		{
			var categories = new SystemDefinableCodeDescriptionBoolCollection();
			categories.Add("IMA", (NoResString)"In", true);
			categories.Add("DAN", (NoResString)"The", true);
			categories.Add("CIN", (NoResString)"Moonlight", true);
			WarehouseDataRegistry.Instance.ReceiveCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, categories);
			var receive = BusinessObject;
			receive.WD_ReceiveCategory = "IMA";
			receive.Factory.Save();

			AssertGetTemplateFilterCriteria<ZString>(receive.WD_ReceiveCategoryInfo, ProcessTaskTemplate.P0_SubType1Info, "DAN", "CIN", ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ColumnRanksCorrect()
		{
			var categories = new SystemDefinableCodeDescriptionBoolCollection();
			categories.Add("IMA", (NoResString)"In", true);
			categories.Add("DAN", (NoResString)"The", true);
			categories.Add("CIN", (NoResString)"Moonlight", true);
			WarehouseDataRegistry.Instance.ReceiveCategories.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, categories);
			var receive = BusinessObject;
			receive.WD_ReceiveCategory = "IMA";
			receive.Factory.Save();

			var ranker = ((IWorkflowProvider)receive).GetTemplateSelectionCriteria();
			var columnValues = ((IColumnValueRankerInternals)ranker).ColumnValues.ToArray();
			AssertArrayEqualsByElements(
			new[] {
					$"P0_OH_Client - {receive.WD_OH_Client}, 00000000-0000-0000-0000-000000000000",
					$"P0_WW - {receive.WD_WW_Whs}, 00000000-0000-0000-0000-000000000000",
					"P0_SubType1 - IMA, "
				},
				columnValues.Select(cv => $"{cv.ColumnName} - {string.Join(", ", cv.Values)}").ToArray());
		}

		protected override ZString GetExpectedWorkflowType()
		{
			return JobInvoicingConsumerTypes.WarehouseInwards.Code;
		}

		protected override WhsReceive GetNewDocket()
		{
			NewRefNumber++;
			return Helper.CreateWhsReceive(Client, Warehouse, "Ref" + NewRefNumber.ToString());
		}
	}

	#endregion

	#region WhsReceiveRelatableActivityTest

	[TestedType(typeof(WhsReceive))]
	public class WhsReceiveRelatableActivityTest : RelatableActivityTestCase<WhsReceive>
	{
		protected override WhsReceive GetNewActivity()
		{
			return Factory.NewWithValidTestData<WhsReceive>();
		}
	}

	#endregion

	#region DocumentSupporter Tests

	public class WhsReceiveDocumentSupporterCartageAdviceHandlerTest : DocumentSupporterCartageAdviceHandlerTest
	{
		protected override IDocumentSupportable GetDocumentSupporterParent()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var org = Helper.CreateClient();
			var receive = Helper.CreateWhsReceive(org, whs);
			return receive;
		}

		protected override IDocumentSupportable[] GetBookingsFromBookingsProperty(DocumentSupporter documentSupporter)
		{
			return ((WhsReceiveDocumentSupporter)documentSupporter).Bookings;
		}

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		protected override bool ChecksChildMenuItem => true;

		protected override bool DocumentSupportablesUseDifferentFactory => true;
	}

	#endregion

	#region DocketSplitter Tests

	#region DocketSplitter Tests

	public abstract class ReceiveSplitterBaseTestCase : WhsTestCaseWithFactory
	{
		#region TestSplit

		public void TestSplit()
		{
			TestSplitCore();
		}

		protected abstract void TestSplitCore();

		#endregion

		#region TestGetNextSplitNumber

		public void TestGetNextSplitNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var area2 = Helper.CreateArea(data.Whs1, "B2", AreaTypes.Codes.Bonded);
			var area3 = Helper.CreateArea(data.Whs1, "B3", AreaTypes.Codes.Excise);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[1].WLV_WA_PutawayArea = area2.PK;
			locations[2].WLV_WA_PutawayArea = area3.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "REF", Notify);

			ZByte[] result = CreateTwoSplitsAndReturnSplitNumbers(data, receive);
			Splitter.NewDocketSplits.Sort();
			AssertEquals((ZByte)1, result[0]);
			AssertEquals((ZByte)2, result[1]);
			Splitter = null;

			result = CreateTwoSplitsAndReturnSplitNumbers(data, receive);
			Splitter.NewDocketSplits.Sort();
			AssertEquals((ZByte)3, result[0]);
			AssertEquals((ZByte)4, result[1]);
			Splitter = null;

			WhsDocket[] dockets = Factory.Load<WhsDocket>(new ZQuery());

			// remove one to test removed numbers are not reused
			foreach (WhsDocket docket in dockets)
			{
				if (docket.WD_ExternalReferenceSplit == 1)
				{
					docket.Delete();
					break;
				}
			}

			result = CreateTwoSplitsAndReturnSplitNumbers(data, receive);
			Splitter.NewDocketSplits.Sort();
			AssertEquals((ZByte)5, result[0]);
			AssertEquals((ZByte)6, result[1]);
		}

		public void TestGetNextSplitNumber_MaxSplitNumberExceeded()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Factory.Save();

			var receive = SetupReceiveWithMaxExternalReferenceSplit(data);
			AssertNoExceptionThrown("No exception is thrown when trying to split.", () => Splitter.Split(receive));
			AssertEquals("No new docket splits.", 0, Splitter.NewDocketSplits.Count);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestSplitterChecksArgument()
		{
			Splitter.Split(null);
		}

		#endregion

		#region Implementation

		protected abstract ZByte[] CreateTwoSplitsAndReturnSplitNumbers(TestDataSimpleEnvironment data, WhsReceive org);

		protected abstract WhsReceive SetupReceiveWithMaxExternalReferenceSplit(TestDataSimpleEnvironment data);

		protected void AssertRelatedSplits(WhsReceive receive, int expectedCount)
		{
			AssertNotNull(receive);
			AssertNotNull(receive.RelatedSplits);
			AssertEquals(expectedCount, receive.RelatedSplits.Count);
		}

		protected ReceiveSplitter Splitter
		{
			get { return splitter ?? (splitter = GetNewSplitter()); }
			set { splitter = value; }
		}

		ReceiveSplitter splitter;

		protected abstract ReceiveSplitter GetNewSplitter();

		#endregion
	}

	#endregion

	public class ReceiveSplitterByQuantityTest : ReceiveSplitterBaseTestCase
	{
		#region TestSplitCore

		protected override void TestSplitCore()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[6] { 5m, 12m, 35m, 400m, 55m, 0.6m }, "REF1", false);

			data.Receive11.Inventory[0].WI_SplitQuantity = 5m;      // Full split
			data.Receive11.Inventory[1].WI_SplitQuantity = 0m;      // No split
			data.Receive11.Inventory[2].WI_SplitQuantity = 35m;     // Full split
			data.Receive11.Inventory[3].WI_SplitQuantity = 400m;    // Full split
			data.Receive11.Inventory[4].WI_SplitQuantity = 30m;     // Partial split
			data.Receive11.Inventory[5].WI_SplitQuantity = 0.5m;    // Partial split

			Factory.Save();

			Splitter.Split(data.Receive11);

			Factory.Save();

			AssertEquals("Original Receive should only have 3 inventories after split", 3, data.Receive11.Inventory.Count);
			AssertEquals("Original Receive should only have 3 receive lines after split", 3, data.Receive11.Lines.Count);
			data.Receive11.Inventory.Sort(WhsInventoryViewSchema.WI_InDocketLineUnits.Name);
			AssertEquals(0.1m, data.Receive11.Inventory[0].WI_InDocketLineUnits);
			AssertEquals(12m, data.Receive11.Inventory[1].WI_InDocketLineUnits);
			AssertEquals(25m, data.Receive11.Inventory[2].WI_InDocketLineUnits);
			AssertEquals(0m, data.Receive11.Inventory[0].WI_SplitQuantity);
			AssertEquals(0m, data.Receive11.Inventory[1].WI_SplitQuantity);
			AssertEquals(0m, data.Receive11.Inventory[2].WI_SplitQuantity);

			AssertRelatedSplits(data.Receive11, 1);

			var query1 = new ZQuery(WhsDocketSchema.WD_ExternalReference, "REF1");
			query1.AddToFilter(WhsDocketSchema.WD_ExternalReferenceSplit, (ZByte)1);
			WhsReceive[] receive1 = Factory.Load<WhsReceive>(query1);

			AssertEquals(1, receive1.Length);
			AssertEquals("2nd Receive should have 5 inventories", 5, receive1[0].Inventory.Count);
			AssertEquals("2nd Receive should have 5 receive lines", 5, receive1[0].Lines.Count);
			receive1[0].Inventory.Sort(WhsInventoryViewSchema.WI_InDocketLineUnits.Name);
			AssertEquals(0.5m, receive1[0].Inventory[0].WI_InDocketLineUnits);
			AssertEquals(5m, receive1[0].Inventory[1].WI_InDocketLineUnits);
			AssertEquals(30m, receive1[0].Inventory[2].WI_InDocketLineUnits);
			AssertEquals(35m, receive1[0].Inventory[3].WI_InDocketLineUnits);
			AssertEquals(400m, receive1[0].Inventory[4].WI_InDocketLineUnits);
			AssertEquals(0.5m, receive1[0].Inventory[0].WI_ExpectedReceiptQuantity);
			AssertEquals(5m, receive1[0].Inventory[1].WI_ExpectedReceiptQuantity);
			AssertEquals(30m, receive1[0].Inventory[2].WI_ExpectedReceiptQuantity);
			AssertEquals(35m, receive1[0].Inventory[3].WI_ExpectedReceiptQuantity);
			AssertEquals(400m, receive1[0].Inventory[4].WI_ExpectedReceiptQuantity);

			AssertEquals(receive1[0].Inventory[0].WI_WD, receive1[0].PK);
			AssertEquals(receive1[0].Inventory[1].WI_WD, receive1[0].PK);
			AssertEquals(receive1[0].Inventory[2].WI_WD, receive1[0].PK);
			AssertEquals(receive1[0].Inventory[3].WI_WD, receive1[0].PK);
			AssertEquals(receive1[0].Inventory[4].WI_WD, receive1[0].PK);
			AssertEquals(receive1[0].Inventory[0].InDocketLine.WE_WD, receive1[0].PK);
			AssertEquals(receive1[0].Inventory[1].InDocketLine.WE_WD, receive1[0].PK);
			AssertEquals(receive1[0].Inventory[2].InDocketLine.WE_WD, receive1[0].PK);
			AssertEquals(receive1[0].Inventory[3].InDocketLine.WE_WD, receive1[0].PK);
			AssertEquals(receive1[0].Inventory[4].InDocketLine.WE_WD, receive1[0].PK);
		}

		#endregion

		#region TestSplit_DoesNotAccessDeletedBizo

		[ExpectNoExceptions]
		public void TestSplit_DoesNotAccessDeletedBizo()
		{
			TestDataForInventory data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[2] { 10m, 20m }, "REF1", false);

			data.Receive11.Inventory[0].WI_SplitQuantity = 5m;
			data.Receive11.Inventory[1].WI_SplitQuantity = 20m;
			Factory.Save();
			int toucher = data.Receive11.Inventory[0].InDocketLine.Inventory.Count;
			toucher = data.Receive11.Inventory[1].InDocketLine.Inventory.Count;

			Splitter.Split(data.Receive11);

			ZQuery query = new ZQuery(WhsDocketSchema.WD_ExternalReference, "REF1");
			query.AddToFilter(WhsDocketSchema.WD_ExternalReferenceSplit, Splitter.NewDocketSplits[0]);
			WhsDocket split = Factory.LoadTop1<WhsDocket>(query);
			split.Delete();
			Factory.Save();
		}

		#endregion

		#region TestSplit_DoesNotCopyAllInformation

		public void TestSplit_DoesNotCopyAllInformation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_ArrivalDate = ZDateTimeOffset.Today;
			receive.WD_ETA = ZDateTimeOffset.Today;
			receive.WD_TotalUnits = 10m;
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT-1");
			inventory.WI_SplitQuantity = 3m;
			Factory.Save();

			receive.SplitReceiptByQuantity();
			// Check original receipt is correct
			AssertNotEquals("Original receipt should keep its arrival date.", ZDateTime.Empty, receive.WD_ArrivalDate);
			AssertNotEquals("Original receipt should keep its ETA date.", ZDateTime.Empty, receive.WD_ETA);
			AssertEquals("Original receipt should keep its Total Units.", 10m, receive.WD_TotalUnits);
			AssertEquals("Original inventory should have its qty reduced.", 7m, inventory.WI_InDocketLineUnits);
			AssertEquals("Original inventory should keep its location", data.Whs1.DefaultLocation.PK, inventory.WI_WL);
			AssertEquals("Original inventory should keep its pallet ID", "PLT-1", inventory.WI_PalletID);

			// Check split receipt is correct
			var splitReceipt = receive.RelatedSplits.Cast<WhsReceive>().Single();
			var splitInventory = splitReceipt.Inventory.Cast<WhsInventoryView>().Single();
			AssertEquals("Split receipt should clear original arrival date.", ZDateTimeOffset.Empty, splitReceipt.WD_ArrivalDate);
			AssertNotEquals("Split receipt should keep original ETA date.", ZDateTime.Empty, splitReceipt.WD_ETA);
			AssertEquals("Split receipt should clear Total Units.", 0m, splitReceipt.WD_TotalUnits);
			AssertEquals("Split inventory should have correct qty.", 3m, splitInventory.WI_InDocketLineUnits);
			AssertEquals("Split inventory should have correct expected qty.", 3m, splitInventory.WI_ExpectedReceiptQuantity);
			AssertEquals("Split inventory should clear original location", ZGuid.Empty, splitInventory.WI_WL);
			AssertEquals("Split inventory should clear original pallet ID", "", splitInventory.WI_PalletID);
		}

		#endregion

		#region TestSplit_SplitPallet_SplitPartiallyWithNoSplit

		public void TestSplit_SplitPallet_SplitPartiallyWithNoSplit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.DefaultLocation, "PLT-1").WI_SplitQuantity = 0;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m, data.Whs1.DefaultLocation, "PLT-1").WI_SplitQuantity = 40;
			Factory.Save();

			receive.SplitReceiptByQuantity();
			Factory.Save();

			// Check original receipt is correct
			var receiptInventories = new BusinessObjectFactory().Load<WhsReceive>(receive.PK).Lines.Cast<WhsReceiveLine>().SelectMany(l => l.Inventory).Cast<WhsInventoryView>();
			AssertEquals("Should have 2 lines.", 2, receiptInventories.Count());
			AssertEquals("Line should exists.", 1, receiptInventories.Count(i => i.WI_OP.Equals(data.Part1.PK) && i.WI_InDocketLineUnits == 50m && i.WI_PalletID == "PLT-1"));
			AssertEquals("Line should exists.", 1, receiptInventories.Count(i => i.WI_OP.Equals(data.Part2.PK) && i.WI_InDocketLineUnits == 10m && i.WI_PalletID == "PLT-1"));

			// Check split receipt is correct
			var splitReceipt = receive.RelatedSplits.Cast<WhsReceive>().Single();
			var splitReceiptInventories = splitReceipt.Lines.Cast<WhsReceiveLine>().SelectMany(l => l.Inventory).Cast<WhsInventoryView>();
			AssertEquals("Should have 1 line.", 1, splitReceiptInventories.Count());
			AssertEquals("Line should exists.", 1, splitReceiptInventories.Count(i => i.WI_OP.Equals(data.Part2.PK) && i.WI_InDocketLineUnits == 40m && i.WI_PalletID.IsEmpty));
		}

		#endregion

		#region TestSplit_SplitPallet_FullySplit

		public void TestSplit_SplitPallet_FullySplit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.DefaultLocation, "PLT-1").WI_SplitQuantity = 50;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m, data.Whs1.DefaultLocation, "PLT-1").WI_SplitQuantity = 50;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.DefaultLocation, "PLT-2").WI_SplitQuantity = 0;
			Factory.Save();

			receive.SplitReceiptByQuantity();
			Factory.Save();

			// Check original receipt is correct
			var receiptInventories = new BusinessObjectFactory().Load<WhsReceive>(receive.PK).Lines.Cast<WhsReceiveLine>().SelectMany(l => l.Inventory).Cast<WhsInventoryView>();
			AssertEquals("Should have 1 line.", 1, receiptInventories.Count());
			AssertEquals("Line should exists.", 1, receiptInventories.Count(i => i.WI_OP.Equals(data.Part1.PK) && i.WI_InDocketLineUnits == 50m && i.WI_PalletID == "PLT-2"));

			// Check split receipt is correct
			var splitReceipt = receive.RelatedSplits.Cast<WhsReceive>().Single();
			var splitReceiptInventories = splitReceipt.Lines.Cast<WhsReceiveLine>().SelectMany(l => l.Inventory).Cast<WhsInventoryView>();
			AssertEquals("Should have 2 lines.", 2, splitReceiptInventories.Count());
			AssertEquals("Line should exists.", 1, splitReceiptInventories.Count(i => i.WI_OP.Equals(data.Part1.PK) && i.WI_InDocketLineUnits == 50m && i.WI_PalletID == "PLT-1"));
			AssertEquals("Line should exists.", 1, splitReceiptInventories.Count(i => i.WI_OP.Equals(data.Part2.PK) && i.WI_InDocketLineUnits == 50m && i.WI_PalletID == "PLT-1"));
		}

		#endregion

		#region TestSplit_SplitPallet_SplitPartiallyWithFullySplit

		public void TestSplit_SplitPallet_SplitPartiallyWithFullySplit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product3 = Helper.CreateProduct(data.Org1, "P3");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.DefaultLocation, "PLT-2").WI_SplitQuantity = 0;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m, data.Whs1.DefaultLocation, "PLT-1").WI_SplitQuantity = 20;
			Helper.CreateWhsReceiveInventoryLine(receive, product3, 50m, data.Whs1.DefaultLocation, "PLT-1").WI_SplitQuantity = 50;
			Factory.Save();

			receive.SplitReceiptByQuantity();
			Factory.Save();

			// Check original receipt is correct
			var receiptInventories = new BusinessObjectFactory().Load<WhsReceive>(receive.PK).Lines.Cast<WhsReceiveLine>().SelectMany(l => l.Inventory).Cast<WhsInventoryView>();
			AssertEquals("Should have 2 line.", 2, receiptInventories.Count());
			AssertEquals("Line should exists.", 1, receiptInventories.Count(i => i.WI_OP.Equals(data.Part1.PK) && i.WI_InDocketLineUnits == 50m && i.WI_PalletID == "PLT-2"));
			AssertEquals("Line should exists.", 1, receiptInventories.Count(i => i.WI_OP.Equals(data.Part2.PK) && i.WI_InDocketLineUnits == 30m && i.WI_PalletID == "PLT-1"));

			// Check split receipt is correct
			var splitReceipt = receive.RelatedSplits.Cast<WhsReceive>().Single();
			var splitReceiptInventories = splitReceipt.Lines.Cast<WhsReceiveLine>().SelectMany(l => l.Inventory).Cast<WhsInventoryView>();
			AssertEquals("Should have 2 lines.", 2, splitReceiptInventories.Count());
			AssertEquals("Line should exists.", 1, splitReceiptInventories.Count(i => i.WI_OP.Equals(data.Part2.PK) && i.WI_InDocketLineUnits == 20m && i.WI_PalletID.IsEmpty));
			AssertEquals("Line should exists.", 1, splitReceiptInventories.Count(i => i.WI_OP.Equals(product3.PK) && i.WI_InDocketLineUnits == 50m && i.WI_PalletID.IsEmpty));
		}

		#endregion

		#region TestSplit_SplitPallet_ManySplitPartiallyForOnePallet

		public void TestSplit_SplitPallet_ManySplitPartiallyForOnePallet()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product3 = Helper.CreateProduct(data.Org1, "P3");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.DefaultLocation, "PLT-1").WI_SplitQuantity = 10;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m, data.Whs1.DefaultLocation, "PLT-1").WI_SplitQuantity = 20;
			Helper.CreateWhsReceiveInventoryLine(receive, product3, 50m, data.Whs1.DefaultLocation, "PLT-1").WI_SplitQuantity = 50;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, data.Whs1.DefaultLocation, "PLT-2").WI_SplitQuantity = 50;
			Factory.Save();

			receive.SplitReceiptByQuantity();
			Factory.Save();
			// Check original receipt is correct
			var receiptInventories = new BusinessObjectFactory().Load<WhsReceive>(receive.PK).Lines.Cast<WhsReceiveLine>().SelectMany(l => l.Inventory).Cast<WhsInventoryView>();
			AssertEquals("Should have 3 lines.", 2, receiptInventories.Count());
			AssertEquals("Line should exists.", 1, receiptInventories.Count(i => i.WI_OP.Equals(data.Part1.PK) && i.WI_InDocketLineUnits == 40m && i.WI_PalletID == "PLT-1"));
			AssertEquals("Line should exists.", 1, receiptInventories.Count(i => i.WI_OP.Equals(data.Part2.PK) && i.WI_InDocketLineUnits == 30m && i.WI_PalletID == "PLT-1"));

			// Check split receipt is correct
			var splitReceipt = receive.RelatedSplits.Cast<WhsReceive>().Single();
			var splitReceiptInventories = splitReceipt.Lines.Cast<WhsReceiveLine>().SelectMany(l => l.Inventory).Cast<WhsInventoryView>();
			AssertEquals("Should have 3 lines.", 4, splitReceiptInventories.Count());
			AssertEquals("Line should exists.", 1, splitReceiptInventories.Count(i => i.WI_OP.Equals(data.Part1.PK) && i.WI_InDocketLineUnits == 10m && i.WI_PalletID.IsEmpty));
			AssertEquals("Line should exists.", 1, splitReceiptInventories.Count(i => i.WI_OP.Equals(data.Part2.PK) && i.WI_InDocketLineUnits == 20m && i.WI_PalletID.IsEmpty));
			AssertEquals("Line should exists.", 1, splitReceiptInventories.Count(i => i.WI_OP.Equals(product3.PK) && i.WI_InDocketLineUnits == 50m && i.WI_PalletID.IsEmpty));
			AssertEquals("Line should exists.", 1, splitReceiptInventories.Count(i => i.WI_OP.Equals(data.Part1.PK) && i.WI_InDocketLineUnits == 50m && i.WI_PalletID == "PLT-2"));
		}

		#endregion

		#region TestSplit_SplitPallet_FullySplit

		public void TestSplit_SplitPallet_FullySplit_NotSamePallet()
		{
			TestSplit_SplitPallet_FullySplitCore(line1PalletID: "PLT-1", line2PalletID: "PLT-2", expectedPalletIDInSplitLine: "PLT-2");
		}

		public void TestSplit_SplitPallet_FullySplit_SamePallet()
		{
			TestSplit_SplitPallet_FullySplitCore(line1PalletID: "PLT-1", line2PalletID: "PLT-1", expectedPalletIDInSplitLine: "");
		}

		void TestSplit_SplitPallet_FullySplitCore(string line1PalletID, string line2PalletID, string expectedPalletIDInSplitLine)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.DefaultLocation, line1PalletID);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 100m, data.Whs1.DefaultLocation, line2PalletID);
			inventory1.WI_SplitQuantity = 0m;
			inventory2.WI_SplitQuantity = 100m;
			Factory.Save();

			receive.SplitReceiptByQuantity();
			Factory.Save();
			var receiptInventory = new BusinessObjectFactory().Load<WhsReceive>(receive.PK).Lines[0].Inventory.Cast<WhsInventoryView>().Single();
			// Check original receipt is correct
			AssertEquals("Keep Inventory", inventory1.PK, receiptInventory.PK);
			AssertEquals("Line should exists.", line1PalletID, receiptInventory.WI_PalletID);

			// Check split receipt is correct
			var splitReceipt = receive.RelatedSplits.Cast<WhsReceive>().Single();
			var splitReceiptInventory = splitReceipt.Lines.Cast<WhsReceiveLine>().SelectMany(l => l.Inventory).Cast<WhsInventoryView>();
			AssertNotEquals("It will create new inventory (just to show current behavior).", inventory2.PK, splitReceiptInventory.Single().PK);
			AssertEquals("Keep pallet only if is not split.", 1, splitReceiptInventory.Count(i => i.WI_OP.Equals(data.Part2.PK) && i.WI_InDocketLineUnits == 100m && i.WI_PalletID.Equals(expectedPalletIDInSplitLine)));
		}

		#endregion

		#region Implementation

		protected override ZByte[] CreateTwoSplitsAndReturnSplitNumbers(TestDataSimpleEnvironment data, WhsReceive receive)
		{
			var result = new ZByte[2];

			var receive1 = Factory.New<WhsReceive>();
			receive1.WD_ExternalReference = "REF";
			receive1.Inventory.AddNew();

			receive1.Inventory[0].WI_InDocketLineUnits = 5m;
			receive1.Inventory[0].WI_SplitQuantity = 3m;
			Splitter.Split(receive);
			result[0] = Splitter.NewDocketSplits[0];

			receive1.Inventory[0].WI_InDocketLineUnits = 5m;
			receive1.Inventory[0].WI_SplitQuantity = 3m;
			Splitter.Split(receive);
			result[1] = Splitter.NewDocketSplits[0];

			return result;
		}

		protected override WhsReceive SetupReceiveWithMaxExternalReferenceSplit(TestDataSimpleEnvironment data)
		{
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_ExternalReferenceSplit = 255;

			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			inventory.WI_SplitQuantity = 3m;
			Factory.Save();

			return receive;
		}

		protected override ReceiveSplitter GetNewSplitter()
		{
			return new ReceiveSplitterByQuantity();
		}

		#endregion
	}

	public class ReceiveSplitterByAreaTypeTest : ReceiveSplitterBaseTestCase
	{
		#region TestSplitCore

		protected override void TestSplitCore()
		{
			var data = new TestDataForInventory(Factory);

			var whs = Helper.CreateWarehouse("11", "AA", 6, 1);
			var locations = whs.Rows.Single(r => r.WR_Name == "AA").Locations;
			var area2 = whs.Areas.AddNew();
			var area3 = whs.Areas.AddNew();
			area2.WA_Name = "area2_0";
			area3.WA_Name = "area3_0";
			area2.WA_AreaType = AreaTypes.Codes.Bonded;
			area3.WA_AreaType = AreaTypes.Codes.Excise;
			locations[1].WLV_WA_PutawayArea = area2.PK;
			locations[2].WLV_WA_PutawayArea = area2.PK;
			locations[3].WLV_WA_PutawayArea = area3.PK;
			locations[4].WLV_WA_PutawayArea = area3.PK;
			locations[5].WLV_WA_PutawayArea = area3.PK;

			var org = Helper.CreateClient();
			var product = Helper.CreateProduct(org, "P1");
			data.CreateSimpleInventoryManyLines(whs, org, product, new ZDecimal[6] { 1m, 2m, 3m, 4m, 5m, 6m }, "REF1", false);

			Factory.Save();
			AssertEquals("Should saved before splitting", false, data.Receive11.HasChanges);

			Splitter.Split(data.Receive11);
			AssertEquals("Original Receive should have 3 lines after split", 3, data.Receive11.Inventory.Count);

			AssertRelatedSplits(data.Receive11, 1);

			var query = new ZQuery(WhsDocketSchema.WD_ExternalReference, SQLComparisonOperator.StartsWith, "REF1");
			var receives = Factory.Load<WhsReceive>(query);
			AssertEquals("Should now be 2 receives (1 new split)", 2, receives.Length);
			AssertEquals("Receives should be split.", true, receives.All(r => r.Lines.Cast<WhsReceiveLine>().Select(l => l.PutawayLocationAreaType).Distinct().Count() == 1));
		}

		#endregion

		#region Implementation

		protected override ZByte[] CreateTwoSplitsAndReturnSplitNumbers(TestDataSimpleEnvironment data, WhsReceive receive)
		{
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m);

			inventory1.LocationString = "A-1";
			inventory2.LocationString = "A-2";
			inventory3.LocationString = "A-3";

			AssertNoExceptionThrown(() => Factory.Save());

			Splitter.Split(receive);

			return Splitter.NewDocketSplits.ToArray();
		}

		protected override WhsReceive SetupReceiveWithMaxExternalReferenceSplit(TestDataSimpleEnvironment data)
		{
			var area2 = Helper.CreateArea(data.Whs1, "B2", AreaTypes.Codes.Bonded);
			var area3 = Helper.CreateArea(data.Whs1, "B3", AreaTypes.Codes.Excise);
			var locations = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			locations[1].WLV_WA_PutawayArea = area2.PK;
			locations[2].WLV_WA_PutawayArea = area3.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_ExternalReferenceSplit = 255;

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.FindLocation("A-2"));
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, data.Whs1.FindLocation("A-3"));
			Factory.Save();

			return receive;
		}

		protected override ReceiveSplitter GetNewSplitter()
		{
			return new ReceiveSplitterByAreaType();
		}

		#endregion
	}

	#endregion

	#region ReceiveSplitter Tests

	internal abstract class ReceiveSplitterTestCase : WhsTestCaseWithFactory
	{
		public abstract void TestSplitLines();

		public abstract void TestSplitLines_UpdatesCrossDockLinks();

		[TestDate(2007, 10, 22)]
		public virtual void TestSplitLineProperties()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			var today = ZDateTimeOffset.Today;
			Helper.CreateProductUnit(data.Part1, "PLT", 1m);
			var i = data.Receive11.Lines[0];
			i.WE_TransactionQuantity = 2;
			i.WE_ClientOrderedUnits = 2;
			i.WE_AdjustmentArrivalDate = today.AddDays(1);
			i.WE_BondedEntryKey = "BEK-1";
			i.Docket.WD_OH_Client = data.Receive11.WD_OH_Client;
			i.WE_PalletID = "PI11";
			i.WE_LineNo = 5;
			i.WE_SubLineNo = 4;
			AssertSplitLinePropertiesSetup(data.Receive11);

			var selectedLines = new List<WhsReceiveLine>();
			selectedLines.AddRange(data.Receive11.Lines.ToList<WhsReceiveLine>());

			Splitter.SplitLines(data.Receive11, selectedLines);
			AssertEquals(2, data.Receive11.Lines.Count);
			AssertSplitLineProperties(data.Receive11.Lines[0], data.Receive11.Lines[1]);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestSplitterChecksArgument1()
		{
			Splitter.SplitLines(null, new List<WhsReceiveLine>());
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestSplitterChecksArgument2()
		{
			Splitter.SplitLines(Factory.New<WhsReceive>(), null);
		}

		protected void AssertSplitLines(ZDecimal[] lineQtys, ZDecimal palletSize, ZDecimal[] splitQtys)
		{
			AssertSplitLines(lineQtys, lineQtys, palletSize, splitQtys, splitQtys);
		}

		protected virtual void AssertSplitLines(ZDecimal[] lineQtys, ZDecimal[] expectedQtys, ZDecimal palletSize, ZDecimal[] splitQtys, ZDecimal[] splitExpectedQtys)
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(lineQtys, false);
			data.SetExpectedQuantities(data.Receive11, expectedQtys);
			Helper.CreateProductUnit(data.Part1, "PLT", palletSize);
			AssertSplitLinesSetup(data.Receive11);

			var selectedLines = new List<WhsReceiveLine>();
			selectedLines.AddRange(data.Receive11.Lines.ToList<WhsReceiveLine>());

			var totalWeight = 0m;
			var totalVolume = 0m;
			foreach (var qty in lineQtys)
			{
				totalWeight += qty * data.Part1.OP_Weight;
				totalVolume += qty * data.Part1.OP_Cubic;
			}

			AssertEquals("Precondition", totalWeight, data.Receive11.WD_TotalWeight);
			AssertEquals("Precondition", totalVolume, data.Receive11.WD_TotalCubic);

			var weightRefreshCount = 0;
			var cubicRefreshCount = 0;
			var receiveLineRefreshCount = 0;
			data.Receive11.WD_TotalWeightInfo.ValueChanged += (s, e) => weightRefreshCount++;
			data.Receive11.WD_TotalCubicInfo.ValueChanged += (s, e) => cubicRefreshCount++;
			data.Receive11.Lines.CountChanged += (s, e) => receiveLineRefreshCount++;

			Splitter.SplitLines(data.Receive11, selectedLines);

			AssertEquals("Total Weight should not refresh for split", 0, weightRefreshCount);
			AssertEquals("Total Volume should not refresh for split", 0, cubicRefreshCount);
			AssertLessThanOrEqualTo("Lines of receive should refresh at most once for each line for split", receiveLineRefreshCount, lineQtys.Length);

			var lineCount = System.Math.Max(splitQtys.Length, splitExpectedQtys.Length);
			AssertEquals(lineCount, data.Receive11.Lines.Count);

			AssertEquals("Total Weight of the Docket should same with before split", totalWeight, data.Receive11.WD_TotalWeight);
			AssertEquals("Total Volume of the Docket should same with before split", totalVolume, data.Receive11.WD_TotalCubic);

			for (var i = 0; i < lineCount; i++)
			{
				if (splitQtys.Length > i)
				{
					AssertEquals(splitQtys[i], data.Receive11.Lines[i].WE_TransactionQuantity);
				}

				if (splitExpectedQtys.Length > i)
				{
					AssertEquals(splitExpectedQtys[i], data.Receive11.Lines[i].WE_ClientOrderedUnits);
				}
			}
		}

		protected virtual void AssertSplitLinesSetup(WhsReceive receive)
		{
		}

		protected virtual void AssertSplitLinePropertiesSetup(WhsReceive receive)
		{
		}

		void AssertSplitLineProperties(WhsDocketLine original, WhsDocketLine newInventory)
		{
			AssertEquals("Incorrect arrival date", original.WE_AdjustmentArrivalDate, newInventory.WE_AdjustmentArrivalDate);
			AssertEquals("Incorrect bonded entry key", original.WE_BondedEntryKey, newInventory.WE_BondedEntryKey);
			AssertEquals("Incorrect status", InventoryStatus.Codes.Arrived, newInventory.WE_CurrentInventoryStatus);
			AssertEquals("Incorrect held code", original.OriginalHeldCode, newInventory.OriginalHeldCode);
			AssertEquals("Incorrect client", original.Docket.WD_OH_Client, newInventory.Docket.WD_OH_Client);
			AssertEquals("Incorrect product", original.WE_OP, newInventory.WE_OP);
			AssertEquals("Incorrect docket line type", original.Docket.WD_DocketType, newInventory.Docket.WD_DocketType);
			AssertEquals("Incorrect location", ZGuid.Empty, newInventory.WE_WL);
			AssertEquals("Incorrect pallet id", "", newInventory.WE_PalletID);
			AssertEquals("Incorrect Line No", original.WE_LineNo, newInventory.WE_LineNo);
			AssertEquals("Incorrect Sub Line No", original.WE_SubLineNo + 1, newInventory.WE_SubLineNo);
			AssertEquals("Incorrect attributes", true, AttributeComparer.Compare(newInventory, original));
		}

		#region Implementation

		protected ReceiveSplitterBase Splitter
		{
			get { return splitter ?? (splitter = GetNewSplitter()); }
			set { splitter = value; }
		}

		protected abstract ReceiveSplitterBase GetNewSplitter();

		ReceiveSplitterBase splitter;

		#endregion
	}

	class PalletReceiveSplitterTest : ReceiveSplitterTestCase
	{
		public override void TestSplitLines()
		{
			AssertSplitLines(new ZDecimal[] { 100m }, 40m, new ZDecimal[3] { 40m, 40m, 20m });
		}

		public void TestSplitLines2()
		{
			AssertSplitLines(new ZDecimal[] { 50m }, 20m, new ZDecimal[3] { 20m, 20m, 10m });
		}

		public void TestSplitLines3()
		{
			AssertSplitLines(new ZDecimal[] { 20m }, 20m, new ZDecimal[1] { 20m });
		}
		public void TestSplitLines4()
		{
			AssertSplitLines(new ZDecimal[] { 15m }, 20m, new ZDecimal[1] { 15m });
		}

		public void TestSplitLines5()
		{
			AssertSplitLines(new ZDecimal[] { 21m }, 20m, new ZDecimal[2] { 20m, 1m });
		}

		public void TestSplitLines6()
		{
			AssertSplitLines(new ZDecimal[] { 0m }, 10m, new ZDecimal[1] { 0m });
		}

		public void TestSplitLines7()
		{
			AssertSplitLines(new ZDecimal[] { 30m, 50m }, 20m, new ZDecimal[5] { 20m, 20m, 10m, 20m, 10m });
		}

		// test when expected quantity is different
		public void TestSplitLines8()
		{
			AssertSplitLines(new ZDecimal[] { 100m }, new ZDecimal[1] { 140m }, 40m, new ZDecimal[4] { 40m, 40m, 20m, 0m }, new ZDecimal[4] { 40m, 40m, 40m, 20m });
		}

		public void TestSplitLines9()
		{
			AssertSplitLines(new ZDecimal[] { 30m }, new ZDecimal[1] { 25m }, 20m, new ZDecimal[2] { 20m, 10m }, new ZDecimal[2] { 20m, 5m });
		}

		public void TestSplitLines10()
		{
			AssertSplitLines(new ZDecimal[] { 25m }, new ZDecimal[1] { 30m }, 20m, new ZDecimal[2] { 20m, 5m }, new ZDecimal[2] { 20m, 10m });
		}

		public void TestSplitLines11()
		{
			AssertSplitLines(new ZDecimal[] { 25m }, new ZDecimal[1] { 0m }, 20m, new ZDecimal[2] { 20m, 5m }, new ZDecimal[2] { 0m, 0m });
		}

		public void TestSplitLines12()
		{
			AssertSplitLines(new ZDecimal[] { 0m }, new ZDecimal[1] { 25m }, 20m, new ZDecimal[2] { 0m, 0m }, new ZDecimal[2] { 20m, 5m });
		}

		public void TestSplitLines13()
		{
			AssertSplitLines(new ZDecimal[] { 30m, 50m }, new ZDecimal[2] { 35m, 35m }, 20m, new ZDecimal[5] { 20m, 20m, 10m, 20m, 10m }, new ZDecimal[5] { 20m, 20m, 15m, 15m, 0m });
		}

		public void TestSplitLines_CustomiseMaxThreshold()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 50m, 51m }, false);
			Helper.CreateProductUnit(data.Part1, "PLT", 1m);
			AssertSplitLinesSetup(data.Receive11);

			var selectedLines = new List<WhsReceiveLine>();
			selectedLines.AddRange(data.Receive11.Lines.ToList<WhsReceiveLine>());

			var notify = (NotificationBuffer)data.Receive11.NotificationManager.Peek;

			using (WarehouseDataRegistry.Instance.MaxSplitCountForPalletizeLines.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 100))
			{
				Splitter.SplitLines(data.Receive11, selectedLines);
				AssertEquals(true, notify.HasErrors);
				AssertEquals("Error: Palletize lines will result in a quantity exceeding the max counts of lines.\r\nYou can increase the threshold in Registry -> Warehouse -> Receive -> Max Split Count For Palletize Lines.", notify.AsString.Trim());
			}

			notify.Clear();

			Splitter.SplitLines(data.Receive11, selectedLines);
			AssertEquals(false, notify.HasErrors);
			AssertEquals("Original line should successfully split with correct number.", 101, data.Receive11.Lines.Count);
		}

		public void TestSplitLines_ExceedingMaxThreshold()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(new ZDecimal[] { 1001m }, false);
			Helper.CreateProductUnit(data.Part1, "PLT", 1m);
			AssertSplitLinesSetup(data.Receive11);

			var selectedLines = new List<WhsReceiveLine>();
			selectedLines.AddRange(data.Receive11.Lines.ToList<WhsReceiveLine>());
			var notify = (NotificationBuffer)data.Receive11.NotificationManager.Peek;

			Splitter.SplitLines(data.Receive11, selectedLines);
			AssertEquals(true, notify.HasErrors);
			AssertEquals("Error: Palletize lines will result in a quantity exceeding the max counts of lines.\r\nYou can increase the threshold in Registry -> Warehouse -> Receive -> Max Split Count For Palletize Lines.", notify.AsString.Trim());

			notify.Clear();

			using (WarehouseDataRegistry.Instance.MaxSplitCountForPalletizeLines.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1010))
			{
				Splitter.SplitLines(data.Receive11, selectedLines);
				AssertEquals(false, notify.HasErrors);
				AssertEquals("Original line should successfully split with correct number.", 1001, data.Receive11.Lines.Count);
			}
		}

		#region TestSplitLines_UpdatesCrossDockLinks

		public override void TestSplitLines_UpdatesCrossDockLinks()
		{
			TestPalletiseLines_UpdatesCrossDockLinks();
		}

		void TestPalletiseLines_UpdatesCrossDockLinks()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			Helper.CreateProductUnit(data.Part1, "PLT", 40);
			data.Receive11.RunPreSaveValidation();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var order1Line = Helper.CreateWhsOrderLine(order1, data.Part1, 50);
			Helper.CreateReservePickLine(order1Line, data.Line111, 50);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var order2Line = Helper.CreateWhsOrderLine(order2, data.Part1, 40);
			Helper.CreateReservePickLine(order2Line, data.Line111, 40);

			var receive = data.Receive11;
			AssertEquals("Pre-condition", 1, receive.Lines.Count);
			AssertEquals("Pre-condition", 90m, receive.Lines[0].ReservedQuantity);
			AssertEquals("Pre-condition", 50m, order1Line.WE_CrossDockQuantity);
			AssertEquals("Pre-condition", 40m, order2Line.WE_CrossDockQuantity);

			var inventoryToPalletize = new List<WhsReceiveLine>();
			inventoryToPalletize.Add(data.Line111.InDocketLine as WhsReceiveLine);
			receive.PalletizeLines(inventoryToPalletize);

			AssertEquals(3, receive.Lines.Count);
			AssertEquals(40m, receive.Lines[0].ReservedQuantity);
			AssertEquals(30m, receive.Lines[1].ReservedQuantity);
			AssertEquals(20m, receive.Lines[2].ReservedQuantity);

			AssertEquals(40m, receive.Lines[0].WE_TransactionQuantity);
			AssertEquals(40m, receive.Lines[1].WE_TransactionQuantity);
			AssertEquals(20m, receive.Lines[2].WE_TransactionQuantity);

			AssertEquals(50m, order1Line.WE_CrossDockQuantity);
			AssertEquals(40m, order2Line.WE_CrossDockQuantity);

			var pickLine1 = receive.Lines[0].ReservedPickLines.Single();
			var pickLine2 = receive.Lines[1].ReservedPickLines.Single();
			var pickLine3 = receive.Lines[2].ReservedPickLines.Single();
			AssertEquals("All lines should have a Inventory Line set.", receive.Lines[0].PK, pickLine1.WZ_WE_InventoryLine);
			AssertEquals("All lines should have a Inventory Line set.", receive.Lines[1].PK, pickLine2.WZ_WE_InventoryLine);
			AssertEquals("All lines should have a Inventory Line set.", receive.Lines[2].PK, pickLine3.WZ_WE_InventoryLine);
		}

		public void TestPalletiseLines_UpdatesCrossDockLinks_ClientOrderedUnitsGreaterThanTransactionQuantity()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			Helper.CreateProductUnit(data.Part1, "PLT", 40);
			var receiveLineToSplit = data.Line111.InDocketLine as WhsReceiveLine;
			receiveLineToSplit.WE_ClientOrderedUnits = 100m;
			receiveLineToSplit.WE_TransactionQuantity = 50m;
			data.Receive11.RunPreSaveValidation();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var order1Line = Helper.CreateWhsOrderLine(order1, data.Part1, 50);
			Helper.CreateReservePickLine(order1Line, data.Line111, 50);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var order2Line = Helper.CreateWhsOrderLine(order2, data.Part1, 40);
			Helper.CreateReservePickLine(order2Line, data.Line111, 40);

			var receive = data.Receive11;
			AssertEquals("Pre-condition", 1, receive.Lines.Count);
			AssertEquals("Pre-condition", 90m, receive.Lines[0].ReservedQuantity);
			AssertEquals("Pre-condition", 50m, order1Line.WE_CrossDockQuantity);
			AssertEquals("Pre-condition", 40m, order2Line.WE_CrossDockQuantity);

			var inventoryToPalletize = new List<WhsReceiveLine>();
			inventoryToPalletize.Add(receiveLineToSplit);
			receive.PalletizeLines(inventoryToPalletize);

			AssertEquals(3, receive.Lines.Count);
			AssertEquals(40m, receive.Lines[0].ReservedQuantity);
			AssertEquals(30m, receive.Lines[1].ReservedQuantity);
			AssertEquals(20m, receive.Lines[2].ReservedQuantity);

			AssertEquals(40m, receive.Lines[0].WE_TransactionQuantity);
			AssertEquals(10m, receive.Lines[1].WE_TransactionQuantity);
			AssertEquals(0m, receive.Lines[2].WE_TransactionQuantity);

			AssertEquals(40m, receive.Lines[0].WE_ClientOrderedUnits);
			AssertEquals(40m, receive.Lines[1].WE_ClientOrderedUnits);
			AssertEquals(20m, receive.Lines[2].WE_ClientOrderedUnits);

			AssertEquals(50m, order1Line.WE_CrossDockQuantity);
			AssertEquals(40m, order2Line.WE_CrossDockQuantity);

			var pickLine1 = receive.Lines[0].ReservedPickLines.Single();
			var pickLine2 = receive.Lines[1].ReservedPickLines.Single();
			var pickLine3 = receive.Lines[2].ReservedPickLines.Single();
			AssertEquals("All lines should have a Inventory Line set.", receive.Lines[0].PK, pickLine1.WZ_WE_InventoryLine);
			AssertEquals("All lines should have a Inventory Line set.", receive.Lines[1].PK, pickLine2.WZ_WE_InventoryLine);
			AssertEquals("All lines should have a Inventory Line set.", receive.Lines[2].PK, pickLine3.WZ_WE_InventoryLine);
		}

		public void TestPalletisedLines_NoUpdateOfCrossDockLinksRequired()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(false);
			Helper.CreateProductUnit(data.Part1, "PLT", 40);
			data.Receive11.RunPreSaveValidation();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20);

			var pivot = Helper.CreateReservePickLine(orderLine, data.Line111, 20);

			var receive = data.Receive11;
			AssertEquals("Pre-condition", 1, receive.Lines.Count);
			AssertEquals("Pre-condition", 20m, receive.Lines[0].ReservedQuantity);
			AssertEquals("Pre-condition", 20m, orderLine.WE_CrossDockQuantity);

			var inventoryToPalletize = new List<WhsReceiveLine>();
			inventoryToPalletize.Add(data.Line111.InDocketLine as WhsReceiveLine);
			receive.PalletizeLines(inventoryToPalletize);

			AssertEquals(3, receive.Lines.Count);
			AssertEquals(20m, receive.Lines[0].ReservedQuantity);
			AssertEquals(0m, receive.Lines[1].ReservedQuantity);
			AssertEquals(0m, receive.Lines[2].ReservedQuantity);

			AssertEquals(40m, receive.Lines[0].WE_TransactionQuantity);
			AssertEquals(40m, receive.Lines[1].WE_TransactionQuantity);
			AssertEquals(20m, receive.Lines[2].WE_TransactionQuantity);

			AssertEquals(20m, orderLine.WE_CrossDockQuantity);
		}

		#endregion

		public virtual void TestDoesNotSplitInvalidLines()
		{
			// Invalid Lines are Lines with
			// 1. No product
			// 2. Error Notifications
			// 3. No PLT definition
			// 4. Finalised Status

			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(50, 40, 30, 25, 21, false);
			var product2 = Helper.CreateProduct(data.Org1, "P2");
			Helper.CreateProductUnit(data.Part1, "PLT", 20);
			var receive = data.Receive11;

			receive.Lines[0].WE_OP = ZGuid.Empty;               // line 0 - invalid because has no product
			receive.Lines[1].WE_OP = ZGuid.Invalid;             // line 1 - invalid because has errors
			receive.Lines[2].WE_OP = product2.PK;               // line 2 - invalid because has no PLT definition
			receive.Lines[3].WE_CurrentInventoryStatus = CodeLists.InventoryStatus.Codes.Available; // line 3 - invalid because its finalised

			var selected = new List<WhsReceiveLine>();
			selected.AddRange(receive.Lines.ToList<WhsReceiveLine>());
			Splitter.SplitLines(data.Receive11, selected);

			AssertEquals("Should of only generated 1 new line", 6, receive.Lines.Count);
			AssertEquals("Line 0 should not of been split", 50m, receive.Lines[0].WE_TransactionQuantity);
			AssertEquals("Line 1 should not of been split", 40m, receive.Lines[1].WE_TransactionQuantity);
			AssertEquals("Line 2 should not of been split", 30m, receive.Lines[2].WE_TransactionQuantity);
			AssertEquals("Line 3 should not of been split", 25m, receive.Lines[3].WE_TransactionQuantity);
			AssertEquals("Line 4 should be split. 21 units should now be 20", 20m, receive.Lines[4].WE_TransactionQuantity);
			AssertEquals("Should have new line of 1 unit", 1m, receive.Lines[5].WE_TransactionQuantity);

			AssertHasRowWarning(receive.Lines[0], Splitter.ErrorMessage_InvalidLine);
			AssertHasRowWarning(receive.Lines[1], Splitter.ErrorMessage_InvalidLine);
			AssertHasRowWarning(receive.Lines[2], Splitter.ErrorMessage_NoQuantityDefined);
			AssertHasRowWarning(receive.Lines[3], Splitter.ErrorMessage_InvalidLine);
			AssertNoRowWarnings(receive.Lines[4]);
			AssertNoRowWarnings(receive.Lines[5]);
		}

		#region Implementation

		protected override ReceiveSplitterBase GetNewSplitter()
		{
			return new PalletReceiveSplitter();
		}

		#endregion
	}

	#endregion

	#region ID Generator Tests

	class SerialNumberGeneratorTest : WhsTestCaseWithFactory
	{
		#region TestInvalidArguments

		public void TestInvalidArguments()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(2, 2, 2, 2, 2, false);
			Helper.CreateProductUnit(data.Part1, "PLT", 1);
			var receive = data.Receive11;
			AssertNotNull("Should have a valid Receive object", receive);

			var selected = new List<WhsReceiveLine>();
			var generator = new SerialNumberGenerator();

			try
			{
				generator.Generate(null, null);
				Assert("Should have generated an ArgumentNull Exception", false);
			}
			catch (ArgumentNullException)
			{
			}
			catch (Exception e1)
			{
				Assert("Should have generated an ArgumentNull Exception, got:" + e1.StackTrace, false);
			}

			try
			{
				generator.Generate(null, selected);
				Assert("Should have generated an ArgumentNull Exception", false);
			}
			catch (ArgumentNullException)
			{
			}
			catch (Exception e2)
			{
				Assert("Should have generated an ArgumentNull Exception, got:" + e2.StackTrace, false);
			}

			try
			{
				generator.Generate(receive, null);
				Assert("Should have generated an ArgumentNull Exception", false);
			}
			catch (ArgumentNullException)
			{
			}
			catch (Exception e3)
			{
				Assert("Should have generated an ArgumentNull Exception, got:" + e3.StackTrace, false);
			}

			try
			{
				generator.Generate(receive, selected);
			}
			catch (ArgumentNullException)
			{
				Assert("Should not have generated an ArgumentNull Exception", false);
			}
			catch (Exception e4)
			{
				Assert("Should not have generated an Exception, got:" + e4.StackTrace, false);
			}
		}

		#endregion

		#region TestDoesNotSplitInvalidLines

		public void TestDoesNotSplitInvalidLines_NoProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine.WE_OP = ZGuid.Empty;
			AssertEquals("Precondition: No product is assigned on the line.", ZGuid.Empty, receiveLine.WE_OP);

			var generator = new SerialNumberGenerator();
			generator.Generate(receive, new[] { receiveLine });

			AssertEquals("Line should not have been split", 5m, receiveLine.WE_TransactionQuantity);
			AssertHasWarning(receive.Lines[0].WE_OPInfo, "The Generate Serial Numbers action cannot process lines with no product");
		}

		public void TestDoesNotSplitInvalidLines_InvalidProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine.WE_OP = ZGuid.Invalid;

			var generator = new SerialNumberGenerator();
			generator.Generate(receive, new[] { receiveLine });

			AssertEquals("Line should not have been split", 5m, receiveLine.WE_TransactionQuantity);
		}

		public void TestDoesNotSplitInvalidLines_ProductDoesNotUseSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			AssertEquals("Precondition: Serial number is not used on the product.", false, receiveLine.Product.IsSerialNumberUsed(data.Org1));

			var generator = new SerialNumberGenerator();
			generator.Generate(receive, new[] { receiveLine });

			AssertEquals("Line should not have been split", 5m, receiveLine.WE_TransactionQuantity);
		}

		public void TestDoesNotSplitInvalidLines_FinalisedLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			AssertEquals("Precondition: inventory is available.", InventoryStatus.Codes.Available, receiveLine.WE_CurrentInventoryStatus);

			var generator = new SerialNumberGenerator();
			generator.Generate(receive, new[] { receiveLine });

			AssertEquals("Line should not have been split", 5m, receiveLine.WE_TransactionQuantity);
		}

		public void TestDoesNotSplitInvalidLines_SerialNumberIsReleaseCaptured()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			Assert("Precondition: Serial number is used on the product.", receiveLine.Product.IsSerialNumberUsed(data.Org1));
			Assert("Precondition: Serial number is release captured.", receiveLine.Product.IsSerialNumberReleaseCaptured(data.Org1));

			var generator = new SerialNumberGenerator();
			generator.Generate(receive, new[] { receiveLine });

			AssertEquals("Line should not have been split", 5m, receiveLine.WE_TransactionQuantity);
		}

		public void TestSerialNumberErrorMessages_NoQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 0m);

			var generator = new SerialNumberGenerator();
			generator.Generate(receive, receive.Lines.Cast<WhsReceiveLine>().ToArray());

			receive.RunPreSaveValidation();

			AssertHasWarning(receiveLine.WE_SerialNumberInfo, "The Generate Serial Number action will only work for lines that have more than one unit. For lines with one unit (like this one), just enter the serial number directly into this field");
		}

		public void TestSerialNumberErrorMessages_NoQtyWithInvalidStartingSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 0m);
			receiveLine.WE_SerialNumber = "SN";

			var generator = new SerialNumberGenerator();
			generator.Generate(receive, receive.Lines.Cast<WhsReceiveLine>().ToArray());

			receive.RunPreSaveValidation();

			AssertHasWarning(receiveLine.WE_SerialNumberInfo, "The Generate Serial Number action will only work for lines that have more than one unit. For lines with one unit (like this one), just enter the serial number directly into this field. The serial number you have entered does not have a numeric suffix, which is unusual. Please check it is entered correctly");
		}

		public void TestSerialNumberErrorMessages_SingleQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m);

			var generator = new SerialNumberGenerator();
			generator.Generate(receive, receive.Lines.Cast<WhsReceiveLine>().ToArray());

			receive.RunPreSaveValidation();

			AssertHasWarning(receiveLine.WE_SerialNumberInfo, "The Generate Serial Number action will only work for lines that have more than one unit. For lines with one unit (like this one), just enter the serial number directly into this field");
		}

		public void TestSerialNumberErrorMessages_NoInitialSerialNumberEntered()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);

			var generator = new SerialNumberGenerator();
			generator.Generate(receive, receive.Lines.Cast<WhsReceiveLine>().ToArray());

			receive.RunPreSaveValidation();

			AssertHasWarning(receiveLine.WE_SerialNumberInfo, "The Generate Serial Numbers action requires you to enter the first serial number in the sequence. This will allow the system to calculate the remaining serial numbers");
		}

		public void TestSerialNumberErrorMessages_SerialNumberWithNoNumberSuffix()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receiveLine.WE_SerialNumber = "SN";

			var generator = new SerialNumberGenerator();
			generator.Generate(receive, receive.Lines.Cast<WhsReceiveLine>().ToArray());

			receive.RunPreSaveValidation();

			AssertHasWarning(receiveLine.WE_SerialNumberInfo, "The Generate Serial Numbers action requires you to enter the first serial number in the sequence and this serial number must have a valid numeric suffix otherwise the system cannot calculate the remaining numbers. e.g. ABC100, SER-10, PS4232323");
		}

		#endregion
	}

	#endregion

	#region Old

	[TestedType(typeof(WhsReceive))]
	internal class WhsReceive_OldTest : WhsBusinessObjectTestCase
	{
		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.WhsReceive);
			}
		}

		#endregion

		#region General

		public void TestCreateAndSaveState()
		{
			SetupPutaway();

			AssertEquals("Docket status must be 'New'", DocketStatus.Codes.New, Docket.WD_DocketStatus);
			AssertEquals("Docket type must be 'Receive'", DocketType.Codes.Receive, Docket.WD_DocketType);
			AssertEquals("Docket sub type must be 'Receipt'", ReceiveType.Codes.Receipt, Docket.WD_DocketSubType);
			Assert("Booking Date is not null", Docket.WD_BookingDate != ZDateTimeOffset.Empty);
			Assert("Arrival Date is not null", Docket.WD_ArrivalDate != ZDateTimeOffset.Empty);

			Factory.Save();
			Assert("Auto Log must be created", Docket.Logs.GetAllLogs().Count > 0);
		}

		public void TestClientReadOnly()
		{
			Docket = Factory.New<WhsReceive>();
			AssertEquals("WD_OH_Client - New", false, Docket.WD_OH_ClientInfo.ReadOnly);

			Docket.WD_DocketStatus = DocketStatus.Codes.Putaway;
			AssertEquals("WD_OH_Client - Putaway", true, Docket.WD_OH_ClientInfo.ReadOnly);

			Docket.WD_FinalisedDate = ZDateTimeOffset.Now;
			Docket.WD_DocketStatus = DocketStatus.Codes.Finalised;
			AssertEquals("WD_OH_Client - Finalised", true, Docket.WD_OH_ClientInfo.ReadOnly);
		}

		public void TestWarehouseReadOnly()
		{
			Docket = Factory.New<WhsReceive>();
			AssertEquals("WD_WW_Whs - New", false, Docket.WD_WW_WhsInfo.ReadOnly);

			Docket.WD_DocketStatus = DocketStatus.Codes.Putaway;
			AssertEquals("WD_WW_Whs - Putaway", true, Docket.WD_WW_WhsInfo.ReadOnly);

			Docket.WD_FinalisedDate = ZDateTimeOffset.Now;
			Docket.WD_DocketStatus = DocketStatus.Codes.Finalised;
			AssertEquals("WD_WW_Whs - Finalised", true, Docket.WD_WW_WhsInfo.ReadOnly);
		}

		#endregion

		#region Allocate Locations

		public void TestPutawayLocationWhenUsingSamePalletIDAndLocationsAreEmpty()
		{
			SetupPutaway();
			var row1A = Helper.CreateRowAndGenerateLocations(Whs1, "A", 4, 4);
			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 20m);

			Docket.AllocateLocationsWithMock();
			AssertEquals("Inventory count", 2, Docket.Inventory.Count);
			AssertEquals("Inventory into first location", row1A.Locations[0].PK, Docket.Inventory[0].WI_WL);
			AssertEquals("Inventory into first location", row1A.Locations[1].PK, Docket.Inventory[1].WI_WL);

			Docket.Inventory[0].WI_WL = ZGuid.Empty;
			Docket.Inventory[1].WI_WL = ZGuid.Empty;

			Docket.Inventory[0].WI_PalletID = "A";
			Docket.Inventory[1].WI_PalletID = "A";

			Docket.AllocateLocationsWithMock();
			AssertEquals("Inventory into first location", row1A.Locations[0].PK, Docket.Inventory[0].WI_WL);
			AssertEquals("Inventory into first location", row1A.Locations[0].PK, Docket.Inventory[1].WI_WL);
		}

		public void TestPutawayLocationWhenUsingSamePalletIDAndLocationIsNotEmpty()
		{
			SetupPutaway();
			var row1A = Helper.CreateRowAndGenerateLocations(Whs1, "A", 4, 4);
			var docket = Docket;
			Helper.CreateWhsReceiveInventoryLine(docket, Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(docket, Part1, 20m);

			docket.AllocateLocationsWithMock();
			AssertEquals("Inventory count", 2, docket.Inventory.Count);
			AssertEquals("Inventory into first location", row1A.Locations[0].PK, docket.Inventory[0].WI_WL);
			AssertEquals("Inventory into second location", row1A.Locations[1].PK, docket.Inventory[1].WI_WL);

			docket.Inventory[0].WI_WL = ZGuid.Empty;

			docket.Inventory[0].WI_PalletID = "A";
			docket.Inventory[1].WI_PalletID = "A";

			AssertNoErrors(docket.Inventory[0]);
			AssertNoErrors(docket.Inventory[1]);
			Factory.Save();

			docket.AllocateLocationsWithMock();
			AssertEquals("Inventory into first location", row1A.Locations[0].PK, docket.Inventory[0].WI_WL);
			AssertEquals("Inventory into second location", row1A.Locations[1].PK, docket.Inventory[1].WI_WL);
			docket.RunPreSaveValidation();
			AssertHasErrors("There should be an error as same pallet exists in 2 locations", docket.Inventory[0].InDocketLine.WE_PalletIDInfo);
			AssertHasErrors("There should be an error as same pallet exists in 2 locations", docket.Inventory[1].InDocketLine.WE_PalletIDInfo);
		}

		public void TestPutawayLocationWhenUsingDifferentPalletIDAndLocationIsNotEmpty()
		{
			SetupPutaway();
			var row1A = Helper.CreateRowAndGenerateLocations(Whs1, "A", 4, 4);
			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 20m);

			Docket.AllocateLocationsWithMock();
			AssertEquals("Inventory count", 2, Docket.Inventory.Count);
			AssertEquals("Inventory into first location", row1A.Locations[0].PK, Docket.Inventory[0].WI_WL);
			AssertEquals("Inventory into second location", row1A.Locations[1].PK, Docket.Inventory[1].WI_WL);

			Docket.Inventory[0].WI_WL = ZGuid.Empty;

			Docket.Inventory[0].WI_PalletID = "A1";
			Docket.Inventory[1].WI_PalletID = "A2";

			Docket.AllocateLocationsWithMock();
			AssertEquals("Inventory into first location", row1A.Locations[0].PK, Docket.Inventory[0].WI_WL);
			AssertEquals("Inventory into second location", row1A.Locations[1].PK, Docket.Inventory[1].WI_WL);
		}

		public void TestPutawayLocationWhenUsingDifferentPalletIDAndLocationsAreEmpty()
		{
			SetupPutaway();
			var row1A = Helper.CreateRowAndGenerateLocations(Whs1, "A", 4, 4);
			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 20m);

			Docket.AllocateLocationsWithMock();
			AssertEquals("Inventory count", 2, Docket.Inventory.Count);
			AssertEquals("Inventory into first location", row1A.Locations[0].PK, Docket.Inventory[0].WI_WL);
			AssertEquals("Inventory into second location", row1A.Locations[1].PK, Docket.Inventory[1].WI_WL);

			Docket.Inventory[0].WI_WL = ZGuid.Empty;
			Docket.Inventory[1].WI_WL = ZGuid.Empty;

			Docket.Inventory[0].WI_PalletID = "A1";
			Docket.Inventory[1].WI_PalletID = "A2";

			Docket.AllocateLocationsWithMock();
			AssertEquals("Inventory into first location", row1A.Locations[0].PK, Docket.Inventory[0].WI_WL);
			AssertEquals("Inventory into second location", row1A.Locations[1].PK, Docket.Inventory[1].WI_WL);
		}

		public void TestPutawaySimpleWithNoEmptyLocations()
		{
			// put some stock into a one location warehouse (so we have no empty locations).
			SetupPutaway();
			var row1A = Helper.CreateRowAndGenerateLocations(Whs1, "A", 1, 1);
			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 10m);

			Docket.AllocateLocationsWithMock();
			Docket.FinaliseDocket();

			// now do another putaway to test a putaway into a full warehouse
			Docket = Helper.CreateWhsReceive(Org, Whs1, "1");
			Docket.WD_BookingDate = ZDateTimeOffset.Now;
			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 10m);

			Docket.AllocateLocationsWithMock();
			AssertEquals("Inventory count", 1, Docket.Inventory.Count);
			AssertEquals("Inventory into first location", row1A.Locations[0].PK, Docket.Inventory[0].WI_WL);
		}

		public void TestPutawaySimpleWithManyLocations()
		{
			SetupPutaway();
			var row1A = Helper.CreateRowAndGenerateLocations(Whs1, "A", 4, 4);
			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 10m);

			// test most basic putaway possible, stock should go into first location for warehouse
			Docket.AllocateLocationsWithMock();
			AssertEquals("Inventory count", 1, Docket.Inventory.Count);
			AssertEquals("Inventory into first location", row1A.Locations[0].PK, Docket.Inventory[0].WI_WL);
			AssertEquals("Inventory should be committed", Docket.Inventory[0].CommittedQuantityIncludingUnfinalisedReceipt, Docket.Inventory[0].WI_TotalUnits);
			AssertEquals("Inventory status should be available", InventoryStatus.Codes.Putaway, Docket.Inventory[0].WI_InventoryStatus);
		}

		public void TestPutawaySimpleWithAttributesManyProducts()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var org = Helper.CreateClient();
			org.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			org.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.Mandatory;
			org.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;

			var part = Helper.CreateProduct(org, "NORMAL");
			var partA1 = Helper.CreateProduct(org, "ATTRIB1");
			var partA2 = Helper.CreateProduct(org, "ATTRIB2");
			var partA3 = Helper.CreateProduct(org, "ATTRIB3");
			var partA13 = Helper.CreateProduct(org, "ATTRIB13");
			var partEX = Helper.CreateProduct(org, "EXPIRY");
			var partPA = Helper.CreateProduct(org, "PACKING");

			Helper.SetProductAttributeUse(org, partA1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(org, partA2, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(org, partA3, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(org, partEX, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(org, partPA, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(org, partA13, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(org, partA13, AttributeNumber.Three, true);

			NotifyBuffer = new TestNotificationBuffer();
			var docket = Helper.CreateWhsReceive(org, whs, NotifyBuffer);
			Helper.CreateWhsReceiveInventoryLine(docket, part, 10m);
			Helper.CreateWhsReceiveInventoryLine(docket, partA1, 10m);
			Helper.CreateWhsReceiveInventoryLine(docket, partA2, 10m);
			Helper.CreateWhsReceiveInventoryLine(docket, partA3, 10m);
			Helper.CreateWhsReceiveInventoryLine(docket, partEX, 10m);
			Helper.CreateWhsReceiveInventoryLine(docket, partPA, 10m);
			Helper.CreateWhsReceiveInventoryLine(docket, partA13, 10m);

			docket.Inventory[1].WI_PartAttrib1 = "A1";
			docket.Inventory[2].WI_PartAttrib2 = "A2";
			docket.Inventory[3].WI_PartAttrib3 = "A3";
			docket.Inventory[4].WI_ExpiryDate = ZDate.Today.AddMonths(1);
			docket.Inventory[5].WI_PackingDate = ZDate.Today.AddMonths(-1);
			docket.Inventory[6].WI_PartAttrib1 = "A131";
			docket.Inventory[6].WI_PartAttrib3 = "A133";

			docket.AllocateLocationsWithMock();
			AssertEquals("Putaway could not be created", true, docket.IsPuttingAway);

			AssertEquals("PartAttrib1 not set on Inventory", "A1", docket.Inventory[1].WI_PartAttrib1);
			AssertEquals("PartAttrib2 not set on Inventory", "A2", docket.Inventory[2].WI_PartAttrib2);
			AssertEquals("PartAttrib3 not set on Inventory", "A3", docket.Inventory[3].WI_PartAttrib3);
			AssertEquals("ExpiryDate  not set on Inventory", ZDateTime.Today.AddMonths(1), docket.Inventory[4].WI_ExpiryDate);
			AssertEquals("PackingDate not set on Inventory", ZDateTime.Today.AddMonths(-1), docket.Inventory[5].WI_PackingDate);
			AssertEquals("PartAttrib1 not set on Inventory", "A131", docket.Inventory[6].WI_PartAttrib1);
			AssertEquals("PartAttrib3 not set on Inventory", "A133", docket.Inventory[6].WI_PartAttrib3);
		}

		public void TestPutawaySimpleWithAttributesOneProduct()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P");
			org.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.Mandatory;
			org.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.Mandatory;
			org.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;

			Helper.SetProductAttributeUse(org, part, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(org, part, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(org, part, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(org, part, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(org, part, AttributeNumber.PackingDate, true);

			NotifyBuffer = new TestNotificationBuffer();
			var docket = Helper.CreateWhsReceive(org, whs, NotifyBuffer);
			Helper.CreateWhsReceiveInventoryLine(docket, part, 10m);

			docket.Inventory[0].WI_ExpiryDate = ZDate.Today.AddMonths(1);
			docket.Inventory[0].WI_PackingDate = ZDate.Today.AddMonths(-1);
			docket.Inventory[0].WI_PartAttrib1 = "PA1";
			docket.Inventory[0].WI_PartAttrib2 = "PA2";
			docket.Inventory[0].WI_PartAttrib3 = "PA3";

			docket.AllocateLocationsWithMock();
			AssertEquals("Putaway could not be created", true, docket.IsPuttingAway);

			AssertEquals("ExpiryDate  not set on Inventory", ZDateTime.Today.AddMonths(1), docket.Inventory[0].WI_ExpiryDate);
			AssertEquals("PackingDate not set on Inventory", ZDateTime.Today.AddMonths(-1), docket.Inventory[0].WI_PackingDate);
			AssertEquals("PartAttrib1 not set on Inventory", "PA1", docket.Inventory[0].WI_PartAttrib1);
			AssertEquals("PartAttrib2 not set on Inventory", "PA2", docket.Inventory[0].WI_PartAttrib2);
			AssertEquals("PartAttrib3 not set on Inventory", "PA3", docket.Inventory[0].WI_PartAttrib3);
		}

		public void TestPutawaySimpleWithBondedEntryKey()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "NORMAL");

			NotifyBuffer = new TestNotificationBuffer();
			var docket = Helper.CreateWhsReceive(org, whs, NotifyBuffer);
			Helper.CreateWhsReceiveInventoryLine(docket, part, 10m, "E1-1");

			docket.AllocateLocationsWithMock();
			AssertEquals("Putaway could not be created", true, docket.IsPuttingAway);
			AssertEquals("EntryKey not set on Inventory", "E1-1", docket.Inventory[0].WI_BondedEntryKey);
		}

		public void TestPutawayWithUserDefinedLocation()
		{
			SetupPutaway();
			var row1A = Helper.CreateRowAndGenerateLocations(Whs1, "A", 2, 2);
			Factory.Save();

			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 10m);

			Docket.Inventory[0].LocationString = "A-2-1";

			// test most basic putaway possible, stock should go into first location for warehouse
			Docket.AllocateLocationsWithMock();
			AssertEquals("Inventory into user specified location", row1A.Locations[2].PK, Docket.Inventory[0].WI_WL);
		}

		public void TestPutawayWithPickface()
		{
			SetupPutaway();
			var product1 = WhsProduct.GetWhsProduct(Part1);
			var row1A = Helper.CreateRowAndGenerateLocations(Whs1, "A", 4, 4);
			var row2A = Helper.CreateRowAndGenerateLocations(Whs2, "A", 4, 4);

			int locationCount1 = row1A.WR_Columns * row1A.WR_Levels;
			int locationCount2 = row2A.WR_Columns * row2A.WR_Levels;

			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 10m);

			Helper.CreateProductPickFace(product1, Docket.Client, row1A.Locations[locationCount1 - 1]);
			Helper.CreateProductPickFace(product1, Docket.Client, row2A.Locations[locationCount2 - 1]);

			Docket.AllocateLocationsWithMock();
			AssertEquals("Inventory count", 2, Docket.Inventory.Count);
			AssertEquals("First inventory to first location (ignore pickfaces)", row1A.Locations[0].PK, Docket.Inventory[0].WI_WL);
			AssertEquals("Second inventory to secode location (ignore pickfaces)", row1A.Locations[1].PK, Docket.Inventory[1].WI_WL);
		}

		public void TestPutawayFallsBackOnDefaultLocation()
		{
			SetupPutaway();
			var row1A = Helper.CreateRowAndGenerateLocations(Whs1, "A", 2, 1);

			int locnCount = row1A.WR_Columns * row1A.WR_Levels;

			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 10m);

			Docket.AllocateLocationsWithMock();
			AssertEquals("Inventory count", 4, Docket.Inventory.Count);

			int defLocnCount = 0;
			foreach (WhsInventoryView inv in Docket.Inventory)
			{
				// def location is Row.Locations[0]
				if (inv.WI_WL == row1A.Locations[0].PK)
				{
					++defLocnCount;
				}
			}
			AssertEquals("Inventory into default location", 3, defLocnCount);
		}

		public void TestPutawayIgnoresIncorrectLocationStatus()
		{
			SetupPutaway();
			var part2 = Helper.CreateProduct(Org, "AAB");
			var part3 = Helper.CreateProduct(Org, "AAC");
			var row1A = Helper.CreateRowAndGenerateLocations(Whs1, "A", 4, 1);

			// putaway should ignore void, held and damaged locations, locations[2] is the valid one
			row1A.Locations[0].WLV_LocationStatus = Enterprise.Warehouse.Environment.CodeLists.LocationStatus.Codes.Void;
			row1A.Locations[1].WLV_LocationStatus = Enterprise.Warehouse.Environment.CodeLists.LocationStatus.Codes.Held;
			row1A.Locations[3].WLV_LocationStatus = Enterprise.Warehouse.Environment.CodeLists.LocationStatus.Codes.Damaged;

			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(Docket, part2, 10m);
			Helper.CreateWhsReceiveInventoryLine(Docket, part3, 10m);

			// test all inventory went into location[2]
			Docket.AllocateLocationsWithMock();
			AssertEquals("Inventory count", 3, Docket.Inventory.Count);
			AssertEquals("Inventory1 into bad location", row1A.Locations[2].PK, Docket.Inventory[0].WI_WL);
			AssertEquals("Inventory2 into bad location", row1A.Locations[2].PK, Docket.Inventory[1].WI_WL);
			AssertEquals("Inventory3 into bad location", row1A.Locations[2].PK, Docket.Inventory[2].WI_WL);
		}

		public void TestEndToEndSimple()
		{
			SetupPutaway();
			Helper.CreateRowAndGenerateLocations(Whs1, "A", 4, 4);
			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 10m);

			Docket.AllocateLocationsWithMock();
			Docket.FinaliseDocket();
			Factory.Save();

			var order = Helper.CreateWhsOrder(Org, Whs1, "1", NotifyBuffer);
			Helper.CreateWhsOrderLine(order, Part1, 10);
			Helper.CreateWhsOrderLine(order, Part2, 20);

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseOrder(order);
			pick.FinalisePick();
			AssertEquals(true, pick.IsFinalised);
		}

		#endregion

		#region Finalisation

		#region Tests for conditions which stop finalise

		public void TestFinaliseAbortsIfNoPutaway()
		{
			SetupPutaway();
			Docket.FinaliseDocket();

			AssertEquals("Docket should not be putaway", false, Docket.IsPuttingAway);
			AssertEquals("Docket should have 'no lines created' notification", true, NotifyBuffer.ContainsNotificationType(WhsErrorTypes.NoLinesEntered));
		}

		public void TestFinaliseConfirmationCancelAbortsFinalise()
		{
			SetupPutaway();
			WhsRow row1A = Helper.CreateRowAndGenerateLocations(Whs1, "A", 1, 1);
			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 10m);

			Docket.AllocateLocationsWithMock();
			AssertEquals("Docket should be putaway", true, Docket.IsPuttingAway);

			NotifyBuffer.DefaultResponse = false;  // similate user clicking cancel
			Docket.FinaliseDocket();
			AssertEquals("Docket should not be finalised", false, Docket.IsFinalised);
			AssertEquals("Finalization Confirmation", ((QueryUserMsgBoxEventArgs)NotifyBuffer.LastQueryUserEventArgs).Caption);
		}
		#endregion

		#region Finalise

		public void TestFinaliseUpdate()
		{
			SetupPutaway();
			WhsRow row1A = Helper.CreateRowAndGenerateLocations(Whs1, "A", 1, 1);
			WhsInventoryView receiveLine1 = Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 10m);
			WhsInventoryView receiveLine2 = Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 10m);
			receiveLine1.WI_BondedEntryKey = "BK";

			Factory.Save();

			Docket.AllocateLocationsWithMock();
			AssertEquals("Docket should be putaway", true, Docket.IsPuttingAway);
			AssertEquals("Inventory 1 should be committed", receiveLine1.WI_InDocketLineUnits, receiveLine1.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Inventory 2 should be committed", receiveLine2.WI_InDocketLineUnits, receiveLine2.CommittedQuantityIncludingUnfinalisedReceipt);
			AssertEquals("Inventory 1 status should be putaway", InventoryStatus.Codes.Putaway, receiveLine1.WI_InventoryStatus);
			AssertEquals("Inventory 2 status should be putaway", InventoryStatus.Codes.Putaway, receiveLine2.WI_InventoryStatus);
			AssertEquals("No attribute should exist", 0, ((WhsBondedWarehouseAttribute[])Factory.Load(typeof(WhsBondedWarehouseAttribute), new ZQuery(WhsBondedWarehouseAttributeSchema.WB_ParentID, receiveLine1.WI_WE_InDocketLine))).Length);
		}

		public void TestFinalisedDate()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "AAA");

			// test default behaviour
			whs.WW_UseArrivalDateForInwardsFinalisedDate = ZBool.False;
			var receive1 = Helper.CreateWhsReceive(org, whs, "1", Notify);
			receive1.WD_ArrivalDate = ZDateTimeOffset.Now.AddDays(-7);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, part, 10m);
			receiveLine1.WI_InventoryStatus = CodeLists.InventoryStatus.Codes.Held;

			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();

			var today = ZDateTimeOffset.Today;
			AssertEquals(true, receive1.WD_FinalisedDate.Date == today.Date);

			// test arrival date override
			whs.WW_UseArrivalDateForInwardsFinalisedDate = ZBool.True;
			var receive2 = Helper.CreateWhsReceive(org, whs, "2", Notify);
			receive2.WD_ArrivalDate = ZDateTimeOffset.Today.AddDays(-7);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive2, part, 10m);
			receiveLine2.WI_InventoryStatus = CodeLists.InventoryStatus.Codes.Held;

			receive2.AllocateLocationsWithMock();
			receive2.FinaliseDocket();
			AssertEquals(ZDateTimeOffset.Today.AddDays(-7), receive2.WD_ArrivalDate);
			AssertEquals(receive2.WD_ArrivalDate, receive2.WD_FinalisedDate);
		}

		#endregion

		#endregion

		#region Properties

		public void TestWD_ArrivalDate()
		{
			SetupPutaway();
			var row1A = Helper.CreateRowAndGenerateLocations(Whs1, "A", 1, 1);

			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 20m);
			Helper.CreateWhsReceiveInventoryLine(Docket, Part1, 30m);

			Docket.WD_ArrivalDate = ZDateTimeOffset.Now;
			Docket.AllocateLocationsWithMock();

			foreach (WhsInventoryView inventory in Docket.Inventory)
			{
				AssertEquals("Precondition: Arrival Date already set to WD_ArrivalDate", Docket.WD_ArrivalDate, inventory.WI_ArrivalDate);
			}

			Docket.WD_ArrivalDate = ZDateTimeOffset.Now.AddDays(1);

			foreach (WhsInventoryView inventory in Docket.Inventory)
			{
				AssertEquals("Inventory arrival date must be Synced", Docket.WD_ArrivalDate, inventory.WI_ArrivalDate);
			}

			var arbitraryDateTime = ZDateTimeOffset.Now;
			Docket.Inventory[0].WI_ArrivalDate = arbitraryDateTime;
			Docket.WD_ArrivalDate = Docket.WD_ArrivalDate; // set it to itself
			AssertEquals("Not Synced - Retained Old Value", arbitraryDateTime, Docket.Inventory[0].WI_ArrivalDate);
			Assert("Not Synced - Inventory Diff from Docket", Docket.WD_ArrivalDate != Docket.Inventory[0].WI_ArrivalDate);
		}

		#endregion

		#region Implementation

		protected void AddNewInventory(WhsReceive receive, OrgHeader client, OrgSupplierPart part, ZDecimal units)
		{
			WhsInventoryView inv = receive.Inventory.AddNew();
			inv.WI_OH_Client = client.PK;
			inv.WI_OP = part.PK;
			inv.WI_InDocketLineUnits = units;
			inv.WI_InDocketLineType = CodeLists.DocketType.Codes.Receive;
		}

		protected void SetupPutaway()
		{
			// create environment
			NotifyBuffer = new TestNotificationBuffer();
			Whs1 = Helper.CreateWarehouse("1");
			Whs2 = Helper.CreateWarehouse("2");

			// create product
			Org = Helper.CreateClient();
			Part1 = Helper.CreateProduct(Org, "P1");
			Part2 = Helper.CreateProduct(Org, "P2");

			// create receive docket
			Docket = Helper.CreateWhsReceive(Org, Whs1, "TEST", NotifyBuffer);
			Docket.WD_BookingDate = ZDateTimeOffset.Now;
		}

		WhsWarehouse Whs1;
		WhsWarehouse Whs2;
		OrgHeader Org;
		WhsReceive Docket;
		OrgSupplierPart Part1;
		OrgSupplierPart Part2;
		TestNotificationBuffer NotifyBuffer;

		#endregion
	}

	#endregion

	#region WhsReceiveIDtbBookingParentTestCase

	[TestedType(typeof(WhsReceive))]
	public class WhsReceiveIDtbBookingParentTestCase : IDtbBookingParentTestCase<WhsReceive>
	{
		protected override WhsReceive GetNewParent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			return new WhsTestHelperFunctions(Factory).CreateWhsReceive(data.Org1, data.Whs1);
		}

		protected override bool IsWorkflowDescriptorSupportsCreateTransportBooking()
		{
			return true;
		}

		// WhsReceive does not allow direct JobCartage child, see code in CommonCartage.CartageParent (for WD forces WhsOrder parent)
		protected override bool CanHaveDirectCartageChild => false;
	}

	#endregion

	#region WhsReceiveTriggerTest

	public class WhsReceiveTriggerTest : WhsDocketTriggerTest<WhsReceive>
	{
		protected override WhsReceive GetNewDocket(OrgHeader client, WhsWarehouse warehouse)
			=> Helper.CreateWhsReceive(client, warehouse);

		protected override WhsDocketLine GetNewDocketLine(WhsReceive receive, OrgSupplierPart part, WhsLocation location)
		{
			var line = receive.Lines.AddNew();
			line.WE_OP = part.PK;
			line.WE_TransactionQuantity = 10;
			line.WE_WL = location.PK;
			return line;
		}

		protected override void SetupDocketLine_CancelledDocketWithUnCancelledLine(WhsDocketLine line)
		{
			line.WE_WL = ZGuid.Empty; // DocketStatus PUT -> ENT
		}

		protected override string GetDocketSubTypeNotDefault() => ReceiveType.Codes.Customs;
	}

	#endregion

	#region Unique Index Failure Handler

	public class DocketIDFountainUniqueIndexFailureHandlerReceive : DocketIDFountainUniqueIndexFailureHandler<WhsReceive>
	{
		protected override string GetDocketType() => DocketType.Codes.Receive;
		protected override string GetDocketSubType() => ReceiveType.Codes.Receipt;
	}

	#endregion

	#region WhsReceiveConversationProviderTest

	public class WhsReceiveConversationProviderTest : TestCaseWithFactory
	{
		public void TestConversation()
		{
			var provider = (IConversationProvider)receive;
			Factory.Save();

			var conversation = provider.eConversation;
			var expectedConversation = JobConversation.GetConversation(receive);

			AssertEquals(expectedConversation.PK, conversation.PK);
		}

		public void TestConversation_NotSaved()
		{
			var provider = (IConversationProvider)receive;

			AssertNull(provider.eConversation);
		}

		public void TestConversation_AlreadyExists()
		{
			var provider = (IConversationProvider)receive;
			Factory.Save();

			var expectedConversation = JobConversation.GetOrCreate(receive);

			var conversation = provider.eConversation;

			AssertEquals(expectedConversation.PK, conversation.PK);
		}

		public void TestParentModule()
		{
			var provider = (IConversationProvider)receive;

			AssertEquals(ModuleIDs.WhsReceive, provider.ParentModule);
		}

		public void TestParentController()
		{
			var provider = (IConversationProvider)receive;

			AssertEquals(ControllerIDs.WhsReceive, provider.ParentController);
		}

		public void TestAdditionalParticipants()
		{
			var provider = (IConversationProvider)receive;

			AssertSequencesEqual(Enumerable.Empty<EConversation.Business.RelatedParty>(), provider.AdditionalParticipants);
		}

		public void TestSendEmailNotificationsOnSave()
		{
			var provider = (IConversationProvider)receive;

			Assert(provider.SendEmailNotificationsOnSave);
		}

		public void TestEmailSubjectContentOverride()
		{
			var provider = (IConversationProvider)receive;

			AssertNull(provider.EmailSubjectContentOverride);
		}

		public void TestFromAddressOverride()
		{
			var provider = (IConversationProvider)receive;
			AssertNull(provider.FromAddressOverride);

			using (GlowRegistry.Instance.NeoEnableConversations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlowRegistry.Instance.NeoConversationsEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "conversations@neo.cargowise.com"))
			{
				AssertEquals("conversations@neo.cargowise.com", provider.FromAddressOverride);
			}
		}

		public void TestGetAdditionalParticipants_NoStaffSubscribers()
		{
			using (GlowRegistry.Instance.NeoEnableConversations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var provider = (IConversationAdditionalParticipantProvider)receive;

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				var additionalParticipants = new List<IConversationParticipant> { staff };
				var mockParticipantProvider = new Mock<IWarehouseConversationParticipantProvider>();
				ObjectFactory.Substitute(mockParticipantProvider.Object);

				var contact = Factory.NewWithValidTestData<OrgContact>();
				var subscribedParticipants = new ReadOnlyCollection<IConversationParticipant>(new List<IConversationParticipant> { contact });
				var participant = Factory.NewWithValidTestData<JobConversationParticipant>();
				participant.JCP_ParticipantTableCode = contact.TablePrefix;
				participant.ParentKey = contact.PK.ToString();

				mockParticipantProvider.Setup(x => x.GetAdditionalParticipants(receive, participant)).Returns(additionalParticipants);

				AssertSequencesEqual(additionalParticipants, provider.GetAdditionalParticipants(subscribedParticipants, participant));
			}
		}

		public void TestGetAdditionalParticipants_NeoConversationsDisabled()
		{
			using (GlowRegistry.Instance.NeoEnableConversations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var provider = (IConversationAdditionalParticipantProvider)receive;

				var mockParticipantProvider = new Mock<IWarehouseConversationParticipantProvider>(MockBehavior.Strict);
				ObjectFactory.Substitute(mockParticipantProvider.Object);

				var contact = Factory.NewWithValidTestData<OrgContact>();
				var subscribedParticipants = new ReadOnlyCollection<IConversationParticipant>(new List<IConversationParticipant> { contact });

				AssertSequencesEqual(Enumerable.Empty<IConversationParticipant>(), provider.GetAdditionalParticipants(subscribedParticipants, Factory.NewWithValidTestData<JobConversationParticipant>()));
			}
		}

		public void TestGetAdditionalParticipants_StaffSubscriber()
		{
			using (GlowRegistry.Instance.NeoEnableConversations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var provider = (IConversationAdditionalParticipantProvider)receive;

				var additionalParticipants = new List<IConversationParticipant>();

				var mockParticipantProvider = new Mock<IWarehouseConversationParticipantProvider>();
				ObjectFactory.Substitute(mockParticipantProvider.Object);

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				var subscribedParticipants = new ReadOnlyCollection<IConversationParticipant>(new List<IConversationParticipant> { staff });

				var contact = Factory.NewWithValidTestData<OrgContact>();
				var participant = Factory.NewWithValidTestData<JobConversationParticipant>();
				participant.JCP_ParticipantTableCode = contact.TablePrefix;
				participant.ParentKey = contact.PK.ToString();

				mockParticipantProvider.Setup(x => x.GetAdditionalParticipants(receive, participant)).Returns(additionalParticipants);

				AssertSequencesEqual(Enumerable.Empty<IConversationParticipant>(), provider.GetAdditionalParticipants(subscribedParticipants, participant));
			}
		}

		public void TestShouldUseThisProviderForHyperlink()
		{
			var provider = (IConversationParentHyperlinkProvider)receive;

			Assert(provider.ShouldUseThisProviderForHyperlink(null));
			Assert(provider.ShouldUseThisProviderForHyperlink(Factory.New<OrgContact>()));
			Assert(!provider.ShouldUseThisProviderForHyperlink(Factory.New<GlbStaff>()));
		}

		public void TestGetHyperlinkToConversationParent()
		{
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glowdev/Portals/"))
			{
				var provider = (IConversationParentHyperlinkProvider)receive;

				AssertEquals($"https://glowdev/Portals/NEO/Desktop#/formFlow/default/IWhsReceive/{receive.PK}", provider.GetHyperlinkToConversationParent());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			receive = Factory.NewWithValidTestData<WhsReceive>();
		}
		WhsReceive receive;
	}

	#endregion
}

