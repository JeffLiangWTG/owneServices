using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DataTransfer.Integration;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.Packing;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportConsignment;
using Enterprise.Integration.Warehouse;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.PermitService;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportBookings.Shared.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Common.Testing;
using Enterprise.Warehouse.Transactions.Business.Printing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.PickByLabel;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;
using GlowRegistry = Enterprise.Registry.Business.GlowRegistry;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	#region WhsOrderTest

	[TestedType(typeof(WhsOrder))]
	public class WhsOrderTest : WhsPickableDocketTest<WhsOrder>
	{
		#region ICustomizableNumberFountainConsumer

		public void TestICustomizableNumberFountainConsumer_SavingSetsOrderID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var docket = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			docket.WD_ExternalReference = "";

			AssertEquals("Precondition: Docket ID is empty.", "", docket.WD_DocketID);

			Factory.Save();
			AssertEquals("Docket ID was set correctly.", "W00000001", docket.WD_DocketID);

			var customisations = new BillOfLadingNumberCustomisationsByServiceLevel();
			customisations.Categories |= NumberCustomisationElementCategories.WarehouseJob | NumberCustomisationElementCategories.WarehouseOrder;
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
					case BillOfLadingNumberCustomisationElement.Keys.WarehouseCode:
						element.Include = true;
						element.Order = 3;
						break;
					default:
						element.Include = false;
						break;
				}
			}
			WarehouseDataRegistry.Instance.WarehouseNumberCustomisation_Order.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisations);

			var docket2 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			docket2.WD_ExternalReference = "";

			Factory.Save();
			AssertEquals("Docket ID was set correctly.", "WEDIBNE1000002", docket2.WD_DocketID);
		}

		#endregion

		#region TestConstructorSetsConcurrencyPolicy

		public void TestConstructorSetsConcurrencyPolicy_WD_WLO_PlannedLoad()
		{
			AssertEquals(ConcurrencyPolicy.Strict, Docket.WD_WLO_PlannedLoadInfo.ConcurrencyPolicy);
		}

		#endregion

		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.WhsOrder);
			}
		}

		#endregion

		#region TestFinaliseDocket_DBHits_WithOrders

		public void TestFinaliseDocket_DBHits_WithOrders()
		{
			var numberOfOrders = 25;
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			for (var i = 0; i < numberOfOrders * 2; i++)
			{
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R" + i, data.Part1, 1m);
			}

			var orders = new WhsPickableDocket[numberOfOrders];
			for (var i = 0; i < numberOfOrders; i++)
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Ord" + i);
				Helper.CreateWhsOrderLine(order, data.Part1, 2m);
				orders[i] = order;
			}

			Factory.Save();

			Helper.CreatePickByAttachingOrders(orders);
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var order4Finalise = newFactory.Load<WhsOrder>(orders[0].PK);
			order4Finalise.Pick.IsAlterPick = true;

			using (RowFactory.SetCachedTables())
			{
				order4Finalise.RunPreSaveValidation();
			}

			AssertDbHits(TestFinaliseDocket_DBHits_WithOrders_ExpectedDBHitsForValidation, newFactory, false);
		}

		protected virtual Dictionary<string, int> TestFinaliseDocket_DBHits_WithOrders_ExpectedDBHitsForValidation
		{
			get
			{
				return new Dictionary<string, int>()
				{
					{ GlbBranchSchema.Constants.TableName, 1 },
					{ JobDocAddressSchema.Constants.TableName, 1 },
					{ OrgAddressSchema.Constants.TableName, 3 },
					{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
					{ OrgContactSchema.Constants.TableName, 1 },
					{ OrgCusCodeSchema.Constants.TableName, 1 },
					{ OrgCustomLabelsSchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgPartUnitSchema.Constants.TableName, 1 },
					{ OrgPartRelationSchema.Constants.TableName, 1 },
					{ OrgSupplierPartSchema.Constants.TableName, 1 },
					{ OrgCompanyDataSchema.Constants.TableName, 1 },
					{ RefCountrySchema.Constants.TableName, 1 },
					{ RefPacksSchema.Constants.TableName, 1 },
					{ RefTimeZoneSchema.Constants.TableName, 1 },
					{ RefTimeZoneSetSchema.Constants.TableName, 1 },
					{ RefUNLOCOSchema.Constants.TableName, 1 },
					{ WhsAreaSchema.Constants.TableName, 1 },
					{ WhsPickSchema.Constants.TableName, 1 },
					{ WhsPickLineSchema.Constants.TableName, 1 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
					{ WhsDocketContainerSchema.Constants.TableName, 1 },
					{ PkgPackageSchema.Constants.TableName, 1 },
					{ PkgPackageJobSchema.Constants.TableName, 1 },

					// Important for this test
					{ WhsDocketSchema.Constants.TableName, 2 },
					{ WhsDocketLineSchema.Constants.TableName, 3 },
				};
			}
		}

		#endregion

		#region Fetch Strategy

		protected override Type FetchStrategyType
		{
			get { return typeof(WhsOrderFetchStrategy); }
		}

		#endregion

		#region BuyerSupplierRelationshipConsumer

		#region TestPromptToSaveBuyerSupplier

		public void TestPromptToSaveBuyerSupplier()
		{
			Env.Registry.UseBuyerSupplierRelationships = true;
			Env.Registry.PromptToSaveBuyerSupplier = true;

			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.ConsigneePK = data.Org1.PK;
			AssertEquals(true, order.ShouldPromptToSaveSupplierBuyerRelationship);

			Env.Registry.UseBuyerSupplierRelationships = false;
			AssertEquals(false, order.ShouldPromptToSaveSupplierBuyerRelationship);

			Env.Registry.UseBuyerSupplierRelationships = true;
			Env.Registry.PromptToSaveBuyerSupplier = false;
			AssertEquals(false, order.ShouldPromptToSaveSupplierBuyerRelationship);

			Env.Registry.PromptToSaveBuyerSupplier = true;
			var link = data.Org1.SupplierLinks.AddNew(data.Org1);
			AssertEquals("Buyer/Supplier link already exists, should not prompt.", false,
				order.ShouldPromptToSaveSupplierBuyerRelationship);

			var org2 = Helper.CreateClient("CONSIGNEE2");
			order.ConsigneePK = org2.PK;
			AssertEquals(true, order.ShouldPromptToSaveSupplierBuyerRelationship);

			order.ConsigneeDocAddress.E2_AddressOverride = true;
			AssertEquals(false, order.ShouldPromptToSaveSupplierBuyerRelationship);

			order.ConsigneePK = data.Org1.PK;
			AssertEquals("Buyer/Supplier link already exists, should not prompt.", false,
				order.ShouldPromptToSaveSupplierBuyerRelationship);

			order.WD_OH_Client = Helper.CreateClient().PK;
			AssertEquals(true, order.ShouldPromptToSaveSupplierBuyerRelationship);
		}

		#endregion

		#region TestAddNewBuyerSupplierLink

		public void TestAddNewBuyerSupplierLink()
		{
			var order = Factory.New<WhsOrder>();
			AssertNull(order.Client);
			AssertNull(order.Consignee);
			AssertNull(order.SupplierBuyerLink);
			order.AddNewBuyerSupplierLink();
			AssertNull(order.SupplierBuyerLink);

			var client = Helper.CreateClient();
			var consignee = Helper.CreateClient();
			consignee.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			order.WD_OH_Client = client.PK;
			order.AddNewBuyerSupplierLink();
			AssertNull(order.SupplierBuyerLink);

			order.ConsigneePK = consignee.PK;
			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.AddNewBuyerSupplierLink();
			AssertNull(order.SupplierBuyerLink);

			order.ConsigneeDocAddress.E2_AddressOverride = false;
			order.AddNewBuyerSupplierLink();
			AssertNotNull(order.SupplierBuyerLink);
		}

		#endregion

		#region TestClearingTransportModeDoesNotFireBuyerSupplierRelationshipUpdate

		public void TestClearingTransportModeDoesNotFireBuyerSupplierRelationshipUpdate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			order.ConsigneePK = consignee.PK;
			order.WD_TransportMode = "SEA";

			order.AddNewBuyerSupplierLink();
			AssertNotNull("Precondition: Supplier/Buyer Link created.", order.SupplierBuyerLink);

			order.SupplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_TransportMode = "AIR";
			order.WD_TransportMode = "";
			AssertEquals("Transport Mode should stay cleared out", "", order.WD_TransportMode);
		}

		#endregion

		#endregion

		#region Customs Stuff

		protected override bool ExpectedIsBondedEntryKeyVisibleForCustomsTransactions => true;

		protected override void TestIsCustomsTransactionCore()
		{
			var docket = GetNewBusinessObject();
			AssertEquals(false, docket.IsCustomsTransaction);
			AssertEquals(false, docket.IsCustomsDataVisible);

			docket.WD_DocketSubType = CodeLists.OrderType.Codes.Customs;
			AssertEquals(true, docket.IsCustomsTransaction);
			AssertEquals(true, docket.IsCustomsDataVisible);

			docket.WD_DocketSubType = CodeLists.OrderType.Codes.CustomsReleaseWithPermit;
			AssertEquals(true, docket.IsCustomsTransaction);
			AssertEquals(true, docket.IsCustomsDataVisible);
		}

		public void TestDefaultDocketSubTypeForCustomsTransaction_FTZ()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouse();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs, data.Part1, 5m);
			AssertEquals(CodeLists.OrderType.Codes.Customs, order.DefaultDocketSubTypeForCustomsTransaction);
		}

		public void TestDefaultDocketSubTypeForCustomsTransaction_USFTZ()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs, data.Part1, 5m);
			AssertEquals(CodeLists.OrderType.Codes.CustomsReleaseWithPermit,
				order.DefaultDocketSubTypeForCustomsTransaction);
		}

		public void TestDefaultDocketSubTypeForCustomsTransaction_PuertoRicoFTZ()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouse(countrycode: Constants.CountryCodes.PuertoRico);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs, data.Part1, 5m);
			AssertEquals(CodeLists.OrderType.Codes.CustomsReleaseWithPermit,
				order.DefaultDocketSubTypeForCustomsTransaction);
		}

		public override void TestDefaultDocketSubTypeForCustomsTransaction()
		{
			var docket = GetNewBusinessObject();
			AssertEquals(CodeLists.OrderType.Codes.Customs, docket.DefaultDocketSubTypeForCustomsTransaction);
		}

		public void TestIsPickingFTZCustomsOrderWithPermit_Customs()
		{
			TestIsPickingFTZCustomsOrderWithPermitCore(OrderType.Codes.Customs, expectedResult: false);
		}

		public void TestIsPickingFTZCustomsOrderWithPermit_CustomsReleaseWithPermit()
		{
			TestIsPickingFTZCustomsOrderWithPermitCore(OrderType.Codes.CustomsReleaseWithPermit, expectedResult: true);
		}

		void TestIsPickingFTZCustomsOrderWithPermitCore(string subType, bool expectedResult)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs, data.Part1, 5m);
			order.WD_DocketSubType = subType;
			Helper.SetOutwardsEntryKeyForOrderLine(order.Lines[0], "ABC");
			using (PermitServiceTestHelper.MockServiceToGetPermits())
			{
				var pick = Helper.CreatePickNew(order);
				AssertEquals("Precondition", true, order.IsAttachedToPickButNotFinalised);
				AssertEquals(expectedResult, order.IsPickingFTZCustomsOrderWithPermit);
			}
		}

		public void TestIsPickingFTZCustomsOrderWithPermit_CustomsReleaseWithPermit_WarehouseInPuertoRico()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouse(countrycode: Constants.CountryCodes.PuertoRico);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs, data.Part1, 5m);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;

			using (PermitServiceTestHelper.MockServiceToGetPermits())
			{
				var pick = Helper.CreatePickNew(order);
				AssertEquals("Precondition", true, order.IsAttachedToPickButNotFinalised);
				AssertEquals(true, order.IsPickingFTZCustomsOrderWithPermit);
			}
		}

		public void TestIsPickingFTZCustomsOrderWithPermit_NotFTZWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			Helper.SetOutwardsEntryKeyForOrderLine(order.Lines[0], "ABC");
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", false, data.Whs1.IsFTZWarehouseInCountryThatUsesPermits);
			AssertEquals("Precondition", true, order.IsAttachedToPickButNotFinalised);
			AssertEquals(false, order.IsPickingFTZCustomsOrderWithPermit);
		}

		public void TestIsPickingFTZCustomsOrderWithPermit_NoPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs, data.Part1, 5m);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			AssertEquals("Precondition", false, order.IsAttachedToPickButNotFinalised);
			AssertEquals(false, order.IsPickingFTZCustomsOrderWithPermit);
		}

		public void TestIsPickingFTZCustomsOrderWithPermit_NotCustomsOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", true, order.IsAttachedToPickButNotFinalised);
			AssertEquals(false, order.IsPickingFTZCustomsOrderWithPermit);
		}

		#region TestIsFTZCustomsOrderWithPermit

		public void TestIsFTZCustomsOrderWithPermit_OrderSubTypeORD_CountryUS()
		{
			TestIsFTZCustomsOrderWithPermitCore(OrderType.Codes.Order, Core.Constants.CountryCodes.UnitedStates,
				expectedResult: false);
		}

		public void TestIsFTZCustomsOrderWithPermit_OrderSubTypeORD_CountryPR()
		{
			TestIsFTZCustomsOrderWithPermitCore(OrderType.Codes.Order, Core.Constants.CountryCodes.PuertoRico,
				expectedResult: false);
		}

		public void TestIsFTZCustomsOrderWithPermit_OrderSubTypeORD_CountryAU()
		{
			TestIsFTZCustomsOrderWithPermitCore(OrderType.Codes.Order, Core.Constants.CountryCodes.Australia,
				expectedResult: false);
		}

		public void TestIsFTZCustomsOrderWithPermit_OrderSubTypeCUS_CountryUS()
		{
			TestIsFTZCustomsOrderWithPermitCore(OrderType.Codes.Customs, Core.Constants.CountryCodes.UnitedStates,
				expectedResult: false);
		}

		public void TestIsFTZCustomsOrderWithPermit_OrderSubTypeCUS_CountryPR()
		{
			TestIsFTZCustomsOrderWithPermitCore(OrderType.Codes.Customs, Core.Constants.CountryCodes.PuertoRico,
				expectedResult: false);
		}

		public void TestIsFTZCustomsOrderWithPermit_OrderSubTypeCUS_CountryAU()
		{
			TestIsFTZCustomsOrderWithPermitCore(OrderType.Codes.Customs, Core.Constants.CountryCodes.Australia,
				expectedResult: false);
		}

		public void TestIsFTZCustomsOrderWithPermit_OrderSubTypeCPS_CountryUS()
		{
			TestIsFTZCustomsOrderWithPermitCore(OrderType.Codes.CustomsReleaseWithPermit,
				Core.Constants.CountryCodes.UnitedStates, expectedResult: true);
		}

		public void TestIsFTZCustomsOrderWithPermit_OrderSubTypeCPS_CountryPR()
		{
			TestIsFTZCustomsOrderWithPermitCore(OrderType.Codes.CustomsReleaseWithPermit,
				Core.Constants.CountryCodes.PuertoRico, expectedResult: true);
		}

		public void TestIsFTZCustomsOrderWithPermit_OrderSubTypeCPS_CountryAU()
		{
			TestIsFTZCustomsOrderWithPermitCore(OrderType.Codes.CustomsReleaseWithPermit,
				Core.Constants.CountryCodes.Australia, expectedResult: false);
		}

		void TestIsFTZCustomsOrderWithPermitCore(string orderSubType, string warehouseCountryCode, bool expectedResult)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = data.Whs1;
			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(warehouseCountryCode).RL_Code;
			whs.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;

			Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 100m);
			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = orderSubType;

			AssertEquals(expectedResult, order.IsFTZCustomsOrderWithPermit);
		}

		#endregion

		#endregion

		#region Documents

		#region TestBusinessContext

		public void TestBusinessContext()
		{
			AssertEquals("BusinessContext should be WhsOrder", BusinessContext.WhsOrder,
				GetNewBusinessObject().DocumentSupporter.BusinessContext);
		}

		#endregion

		#region TestSupportedDataContext

		public void TestSupportedDataContext()
		{
			AssertEquals("Core.Constants.DataContext.WhsOrder is Supported", true,
				GetNewBusinessObject().DocumentSupporter.IsDataContextSupported(
					new DataContextValueForTesting(Core.Constants.DataContext.WhsOrder)));
		}

		#endregion

		#region TestGetDocBusinessObject

		public void TestGetDocBusinessObject()
		{
			var wrappers =
				GetNewBusinessObject().DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.WhsOrder, null);
			AssertEquals("Should return 1 wrapper", 1, wrappers.Length);
			AssertEquals("DocWrapper should wrap a WhsOrder type bizo", ExpectedBusinessObjectType,
				wrappers[0].WrappedObject.GetType());
		}

		#endregion

		#endregion

		#region TestTransportCoValidation

		public void TestTransportCoValidation()
		{
			Globals.IsWeb = true;
			var testOrder = Factory.NewWithValidTestData<WhsOrder>();
			testOrder.TransportCoDocAddress.OrganisationNameOrPK = string.Empty;
			testOrder.RunPreSaveValidation();
			Assert(!testOrder.TransportCoDocAddress.OrganisationNameOrPKInfo.HasNotifications());

			Globals.IsWeb = false;
			testOrder = Factory.NewWithValidTestData<WhsOrder>();
			testOrder.TransportCoDocAddress.OrganisationNameOrPK = string.Empty;
			testOrder.RunPreSaveValidation();
			Assert(testOrder.TransportCoDocAddress.OrganisationNameOrPKInfo.HasNotifications());
		}

		#endregion

		#region Business Object Overrides

		#region BusinessObjectsWithRelatedEvents

		public void TestBusinessObjectsWithRelatedEvents()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);

			var shipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var pivot = Factory.New<WhsDocketJobPivot>();
			pivot.WV_ParentId = shipment.PK;
			pivot.WV_ParentTableCode = shipment.TablePrefix;
			pivot.WV_DocketType = order.WD_DocketType;
			pivot.WV_WD_Docket = order.PK;

			var cartageJob = (BusinessObject)Factory.New<ICommonCartage>();
			cartageJob[JobCartageSchema.JJ_ParentID] = order.PK;
			cartageJob[JobCartageSchema.JJ_ConsignmentID] = order.WD_DocketID;

			Helper.CreatePickNew(order); // create package job
			var package1 = order.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "ABC";

			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { shipment, cartageJob, package1 },
				order.BusinessObjectsWithRelatedEvents);
		}

		#endregion

		#region Clone

		protected override void AdditionalSetupForTestClone(WhsOrder docket)
		{
			base.AdditionalSetupForTestClone(docket);
			docket.WD_GS_NKAssignedPacker = "ABC";

			var task = Factory.New<ProcessTask>();
			task.P9_ParentID = docket.PK;
			task.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task.P9_FormFlowType = "WDP";

			docket.WD_P9_PackingTask = task.PK;
		}

		protected override void TestCloneCore(WhsOrder originalDocket, WhsOrder clonedDocket)
		{
			base.TestCloneCore(originalDocket, clonedDocket);
			var order = clonedDocket;
			AssertEquals("ConsigneePK", originalDocket.ConsigneePK, order.ConsigneePK);
			AssertEquals("WD_WLO_PlannedLoad not cloned", ZGuid.Empty, order.WD_WLO_PlannedLoad);
			AssertEquals("WD_GS_NKAssignedPacker for original", "ABC", originalDocket.WD_GS_NKAssignedPacker);
			AssertEquals("WD_GS_NKAssignedPacker for clone", string.Empty, clonedDocket.WD_GS_NKAssignedPacker);
			AssertEquals("WD_P9_PackingTask not cloned.", ZGuid.Empty, clonedDocket.WD_P9_PackingTask);
		}

		public void TestClone_ExcludesOrderClassification()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_OrderClassification = OrderClassification.Codes.Bulk;

			var clone = (WhsOrder)order.Clone();
			Assert(clone.WD_OrderClassification.IsEmpty);
		}

		#endregion

		#region TestCreateOperationalStatusChangeEvents

		protected override void TestCreateOperationalStatusChangeEvents(WhsOrder docket)
		{
			var pick = Helper.CreatePickNew();
			pick.WP_WW_Whs = docket.Warehouse.PK;
			docket.WD_WP = pick.PK;
			docket.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			Factory.Save();
			AssertEquals("Order Picking event should be created", 1,
				docket.Logs.Find(Helper.GetLogFilter(Events.WarehouseOrderPicking.Code)).Length);
			AssertNotEquals("Precondition - ensure at least 1 log exist.", ZDateTime.Empty,
				docket.Logs.MostRecentLog.SL_EventTime);

			docket.WD_PalletsSent = 10;
			Factory.Save();
			AssertEquals("Only one Order Picking event should be created", 1,
				docket.Logs.Find(Helper.GetLogFilter(Events.WarehouseOrderPicking.Code)).Length);

			_ = docket.Logs.MostRecentLog.SL_EventTime;
			docket.WD_WP = ZGuid.Empty;
			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			Factory.Save();
			AssertEquals("Order Picking Cancelled event should be created", 1,
				docket.Logs.Find(Helper.GetLogFilter(Events.WarehousePickCancelled.Code)).Length);

			docket.WD_PalletsSent = 11;
			Factory.Save();
			AssertEquals("Only one Order Picking Cancelled event should be created", 1,
				docket.Logs.Find(Helper.GetLogFilter(Events.WarehousePickCancelled.Code)).Length);
		}

		#endregion

		#region TestCalculateTotalsEnabledCore

		protected override void TestCalculateTotalsEnabledCore()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A");
			var client = Helper.CreateClient();
			Factory.Save();

			var order = GetNewBusinessObject();

			order.WD_OH_Client = client.PK;
			order.WD_WW_Whs = warehouse.PK;
			order.WD_RequiredDate = ZDateTimeOffset.Today;
			order.ConsigneeNameOrPK = Helper.CreateClient("CNE").PK.ToString();

			var part = Helper.CreateProduct(client, "PRODUCT1");
			part.OP_Cubic = 1.0m;
			part.OP_CubicUQ = Core.Constants.Volume.CubicMetres;
			part.OP_Weight = 1.0m;
			part.OP_WeightUQ = Core.Constants.Weight.Kilograms;

			Helper.CreateWhsReceiveWithInventory(client, order.Warehouse, "R1", part, 10m);
			Factory.Save();

			Helper.CreateWhsOrderLine(order, part, 10);
			Helper.CreatePickNew(order);

			order.WD_TotalWeightUnit = Core.Constants.Weight.Grams;
			order.WD_TotalCubicUnit = Core.Constants.Volume.CubicDecimetres;

			AssertEquals(10m, order.WD_UnitsSent);
			AssertEquals(10000m, order.WD_CubicSent);
			AssertEquals(10000m, order.WD_WeightSentUserEntered);

			order.CalculateTotalsEnabled = false;
			AssertEquals(false, order.CalculateTotalsEnabled);

			order.WD_TotalCubicUnit = Core.Constants.Volume.CubicMetres;
			order.WD_TotalWeightUnit = Core.Constants.Weight.Kilograms;

			order.WD_UnitsSent = 20m;
			order.WD_CubicSent = 200m;
			order.WD_WeightSentUserEntered = 100m;

			AssertEquals(20m, order.WD_UnitsSent);
			AssertEquals(200m, order.WD_CubicSent);
			AssertEquals(100m, order.WD_WeightSentUserEntered);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var order = GetNewBusinessObject();
			order.WD_DocketID = "OD00001001";
			AssertEquals("Warehouse Order OD00001001", order.HumanReadableName);
		}

		#endregion

		#region TestSetDefaultValues

		public void TestSetDefaultValues_WD_AddPalletWeightToOrder()
		{
			using (WarehouseDataRegistry.Instance.AddPalletWeightToOrder.SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, false))
			{
				var order1 = Factory.New<WhsOrder>();
				AssertEquals("Should have defaulted to false from Registry.", false, order1.WD_AddPalletWeightToOrder);
			}

			using (WarehouseDataRegistry.Instance.AddPalletWeightToOrder.SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, true))
			{
				var order2 = Factory.New<WhsOrder>();
				AssertEquals("Should have defaulted to true from Registry.", true, order2.WD_AddPalletWeightToOrder);
			}
		}

		#endregion

		#region Notes

		protected override void TestNoteContextsForRelatedNotesSetup(WhsOrder docket)
		{
			base.TestNoteContextsForRelatedNotesSetup(docket);

			var transportCo = Helper.CreateClient();
			var consignee = Helper.CreateClient();
			CreateTestNoteCollection(transportCo);
			CreateTestNoteCollection(consignee);

			var order = docket;
			order.TransportCoDocAddress.OrganisationPK = transportCo.PK;
			order.ConsigneeDocAddress.OrganisationPK = consignee.PK;
		}

		protected override void TestNoteContextsForRelatedNotesAssertions(WhsOrder docket)
		{
			var order = docket;
			Assert("Should always be 'Warehouse' module",
				(order.GetNoteContextsForRelatedNotes().Module & StmNoteContextModule.W) != 0);
			Assert("Should always be 'Out' direction",
				(order.GetNoteContextsForRelatedNotes().Direction & StmNoteContextDirection.O) != 0);
			Assert("Should always be 'Order' freight mode",
				(order.GetNoteContextsForRelatedNotes().FreightMode & StmNoteContextFreightMode.O) != 0);

			AssertEquals("Visible Notes Count", 16, order.Notes.VisibleNotes.Count);
		}

		#endregion

		#region TestExternalReferenceIsSetToDocketIDIfEmptyOnSaving

		public void TestExternalReferenceIsSetToDocketIDIfEmptyOnSaving()
		{
			var whs = Helper.CreateWarehouse("A");
			var org = Helper.CreateClient();
			var order = GetNewBusinessObject();
			order.WD_OH_Client = org.PK;
			order.WD_WW_Whs = whs.PK;
			order.WD_ExternalReference = "";
			Factory.Save();
			Assert(order.WD_ExternalReference == order.WD_DocketID);
		}

		#endregion

		#region TestCreateAndSaveState

		public void TestCreateAndSaveState()
		{
			var whs = Helper.CreateWarehouse("AAAA");
			var org = Helper.CreateClient();
			Factory.Save();

			var order = GetNewBusinessObject();
			AssertEquals("Order status must be 'New'", CodeLists.DocketStatus.Codes.New, order.WD_DocketStatus);
			AssertEquals("Order type must be 'Order'", CodeLists.DocketType.Codes.Order, order.WD_DocketType);
			AssertEquals("Order sub type must be 'Order'", CodeLists.OrderType.Codes.Order, order.WD_DocketSubType);
			Assert("Booking Date is null by default.", order.WD_BookingDate == ZDateTimeOffset.Empty);

			order.WD_OH_Client = org.PK;
			order.WD_WW_Whs = whs.PK;
			order.WD_ExternalReference = "1";
			Factory.Save();

			Assert("Booking Date is set after warehouse is been set.", order.WD_BookingDate != ZDateTimeOffset.Empty);
			Assert("Auto Log must be created", order.Logs.GetAllLogs().Count > 0);
		}

		#endregion

		#region TestOnFactorySaving_RequiredDate

		public void TestOnFactorySaving_RequiredDate()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "A");
			var receive = Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(org, whs, "1", part, 10m);
			var pick = Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order);
			order.WD_RequiredDate = ZDateTimeOffset.Empty;
			order.WD_FinalisedDate = ZDateTimeOffset.Now;
			AssertEquals(true, order.WD_RequiredDate.IsEmpty);
			AssertEquals(true, order.RequiredDate.IsEmpty);

			Factory.Save();
			AssertEquals(order.WD_FinalisedDate.EndOfDay().AddSeconds(-59), order.WD_RequiredDate);
			AssertEquals(order.WD_FinalisedDate.EndOfDay().ToDateTime().AddSeconds(-59), order.RequiredDate);
		}

		#endregion

		#endregion

		#region Related Entities

		#region TestAddNewBuyerSupplierLinkWithTransportAndContainerModeAndINCO

		public void TestAddNewBuyerSupplierLinkWithTransportAndContainerModeAndINCO()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var consignee = Helper.CreateClient();
			consignee.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			order.ConsigneePK = consignee.PK;
			order.AddNewBuyerSupplierLink();
			AssertNotNull("Precondition", order.SupplierBuyerLink);
			AssertEquals("Precondition", "", order.WD_TransportMode);
			AssertEquals("Precondition", "", order.WD_ContainerMode);
			AssertEquals("Precondition", "", order.WD_INCO);
			AssertEquals("Precondition", 1, order.SupplierBuyerLink.OrgSupBuyLinkTrnModes.Count);
			var supBuyLinkTrnMode = order.SupplierBuyerLink.OrgSupBuyLinkTrnModes[0];
			AssertEquals("Default Transport Mode.", "ROA", supBuyLinkTrnMode.PF_TransportMode);
			AssertEquals("Default Container Mode.", "LTL", supBuyLinkTrnMode.PF_ContainerMode);
			AssertEquals("Default INCO Term.", "FOB", supBuyLinkTrnMode.PF_IncoTerm);

			var newConsignee = Helper.CreateClient();
			newConsignee.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			order.ConsigneeDocAddress.OrganisationPK = newConsignee.PK;
			order.WD_TransportMode = "AIR";
			order.WD_ContainerMode = "LSE";
			order.WD_INCO = "FCA";

			order.AddNewBuyerSupplierLink();
			AssertNotNull("Precondition", order.SupplierBuyerLink);
			AssertEquals(1, order.SupplierBuyerLink.OrgSupBuyLinkTrnModes.Count);
			var newSupBuyLinkTrnMode = order.SupplierBuyerLink.OrgSupBuyLinkTrnModes[0];
			AssertEquals("AIR", newSupBuyLinkTrnMode.PF_TransportMode);
			AssertEquals("LSE", newSupBuyLinkTrnMode.PF_ContainerMode);
			AssertEquals("FCA", newSupBuyLinkTrnMode.PF_IncoTerm);
		}

		#endregion

		#region TestCartonGroup

		public void TestCartonGroup()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Factory.New<WhsOrder>();
			order.WD_WW_Whs = data.Whs1.PK;
			Assert(!order.CartonGroup.OrgCartonGroupPK.IsValid);

			var carrier = Helper.CreateClient("CAR");
			var consignee = Helper.CreateClient("CNE");

			var carrierGroup = Helper.CreateWhsCartonGroup("1", "Carrier");
			var cneGroup = Helper.CreateWhsCartonGroup("2", "Consignee");
			var clientGroup = Helper.CreateWhsCartonGroup("3", "WhsClient");
			var whsGroup = Helper.CreateWhsCartonGroup("4", "WhsOrg");

			carrier.MiscServ.OM_WCG_CartonGroup = carrierGroup.PK;
			consignee.MiscServ.OM_WCG_CartonGroup = cneGroup.PK;
			data.Org1.MiscServ.OM_WCG_CartonGroup = clientGroup.PK;
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			// Test Property Handles Organisation not existing + fallbacks
			order.WD_WW_Whs = data.Whs1.PK;
			AssertEquals(whsGroup.PK, order.CartonGroup.OrgCartonGroupPK);

			order.WD_OH_Client = data.Org1.PK;
			AssertEquals(clientGroup.PK, order.CartonGroup.OrgCartonGroupPK);

			order.ConsigneeNameOrPK = consignee.PK.ToString();
			AssertEquals(cneGroup.PK, order.CartonGroup.OrgCartonGroupPK);

			order.TransportCoNameOrPK = carrier.PK.ToString();
			AssertEquals(carrierGroup.PK, order.CartonGroup.OrgCartonGroupPK);

			// Test empty FK to carton group + fallbacks again
			carrier.MiscServ.OM_WCG_CartonGroup = ZGuid.Empty;
			AssertEquals(cneGroup.PK, order.CartonGroup.OrgCartonGroupPK);

			consignee.MiscServ.OM_WCG_CartonGroup = ZGuid.Empty;
			AssertEquals(clientGroup.PK, order.CartonGroup.OrgCartonGroupPK);

			data.Org1.MiscServ.OM_WCG_CartonGroup = ZGuid.Empty;
			AssertEquals(whsGroup.PK, order.CartonGroup.OrgCartonGroupPK);

			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = ZGuid.Empty;
			Assert(!order.CartonGroup.OrgCartonGroupPK.IsValid);
		}

		public void TestCartonGroup_UseOrderWarehouseBranchCartonGroupSequence()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("2", "B", 1, 1);
			var order = Factory.New<WhsOrder>();
			order.WD_WW_Whs = data.Whs1.PK;
			Assert(order.CartonGroup.OrgCartonGroupPK.IsEmpty);

			var carrier = Helper.CreateClient("CAR");
			var consignee = Helper.CreateClient("CNE");

			var carrierGroup = Helper.CreateWhsCartonGroup("1", "Carrier");
			var cneGroup = Helper.CreateWhsCartonGroup("2", "Consignee");
			var clientGroup = Helper.CreateWhsCartonGroup("3", "WhsClient");
			var whsGroup = Helper.CreateWhsCartonGroup("4", "WhsOrg");
			var productGroup = Helper.CreateWhsCartonGroup("5", "WhsProduct");

			carrier.MiscServ.OM_WCG_CartonGroup = carrierGroup.PK;
			consignee.MiscServ.OM_WCG_CartonGroup = cneGroup.PK;
			data.Org1.MiscServ.OM_WCG_CartonGroup = clientGroup.PK;
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			order.WD_WW_Whs = data.Whs1.PK;
			order.WD_OH_Client = data.Org1.PK;
			order.ConsigneeNameOrPK = consignee.PK.ToString();
			order.TransportCoNameOrPK = carrier.PK.ToString();
			data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner).OU_WCG_CartonGroup = productGroup.PK;

			var whs1BranchPk = data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid();
			var whs2BranchPk = whs2.WW_GB_RelatedCompanyBranch.ToGuid();
			AssertNotEquals("Precondition: ", whs1BranchPk, whs2BranchPk);

			using (WarehouseDataRegistry.Instance.CartonGroupSequence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new CartonGroupSequence()
				{
					Product = 5,
					Carrier = 3,
					Consignee = 2,
					Client = 4,
					Warehouse = 1
				}))
			using (WarehouseDataRegistry.Instance.CartonGroupSequence.SetTemporaryValue(Guid.Empty, whs1BranchPk, Guid.Empty,
				new CartonGroupSequence()
				{
					Product = 5,
					Carrier = 3,
					Consignee = 2,
					Client = 1,
					Warehouse = 4
				}))
			using (WarehouseDataRegistry.Instance.CartonGroupSequence.SetTemporaryValue(Guid.Empty, whs2BranchPk, Guid.Empty,
				new CartonGroupSequence()
				{
					Product = 1,
					Carrier = 2,
					Consignee = 5,
					Client = 3,
					Warehouse = 4
				}))
			{
				AssertEquals(clientGroup.PK, order.CartonGroup.OrgCartonGroupPK);
				AssertEquals(false, order.CartonGroup.ProductCartonGroupTakesPrecedence);
			}
		}

		public void TestCartonGroup_WorkWithPriority()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Factory.New<WhsOrder>();
			order.WD_WW_Whs = data.Whs1.PK;
			Assert(order.CartonGroup.OrgCartonGroupPK.IsEmpty);

			var carrier = Helper.CreateClient("CAR");
			var consignee = Helper.CreateClient("CNE");

			var carrierGroup = Helper.CreateWhsCartonGroup("1", "Carrier");
			var cneGroup = Helper.CreateWhsCartonGroup("2", "Consignee");
			var clientGroup = Helper.CreateWhsCartonGroup("3", "WhsClient");
			var whsGroup = Helper.CreateWhsCartonGroup("4", "WhsOrg");
			var productGroup = Helper.CreateWhsCartonGroup("5", "WhsProduct");

			carrier.MiscServ.OM_WCG_CartonGroup = carrierGroup.PK;
			consignee.MiscServ.OM_WCG_CartonGroup = cneGroup.PK;
			data.Org1.MiscServ.OM_WCG_CartonGroup = clientGroup.PK;
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			order.WD_WW_Whs = data.Whs1.PK;
			order.WD_OH_Client = data.Org1.PK;
			order.ConsigneeNameOrPK = consignee.PK.ToString();
			order.TransportCoNameOrPK = carrier.PK.ToString();
			data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner).OU_WCG_CartonGroup = productGroup.PK;
			// Client has the highest Priority
			using (WarehouseDataRegistry.Instance.CartonGroupSequence.SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty,
				new CartonGroupSequence()
				{
					Product = 5,
					Carrier = 3,
					Consignee = 2,
					Client = 1,
					Warehouse = 4
				}))
			{
				AssertEquals(clientGroup.PK, order.CartonGroup.OrgCartonGroupPK);
				AssertEquals(false, order.CartonGroup.ProductCartonGroupTakesPrecedence);
			}

			// Carrier has the highest Priority
			using (WarehouseDataRegistry.Instance.CartonGroupSequence.SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty,
				new CartonGroupSequence()
				{
					Product = 5,
					Carrier = 1,
					Consignee = 2,
					Client = 3,
					Warehouse = 4
				}))
			{
				AssertEquals(carrierGroup.PK, order.CartonGroup.OrgCartonGroupPK);
				AssertEquals(false, order.CartonGroup.ProductCartonGroupTakesPrecedence);
			}

			// Warehouse has the highest Priority
			using (WarehouseDataRegistry.Instance.CartonGroupSequence.SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty,
				new CartonGroupSequence()
				{
					Product = 5,
					Carrier = 4,
					Consignee = 2,
					Client = 3,
					Warehouse = 1
				}))
			{
				AssertEquals(whsGroup.PK, order.CartonGroup.OrgCartonGroupPK);
				AssertEquals(false, order.CartonGroup.ProductCartonGroupTakesPrecedence);
			}

			// Consignee has the highest Priority
			using (WarehouseDataRegistry.Instance.CartonGroupSequence.SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty,
				new CartonGroupSequence()
				{
					Product = 2,
					Carrier = 3,
					Consignee = 1,
					Client = 5,
					Warehouse = 4
				}))
			{
				AssertEquals(cneGroup.PK, order.CartonGroup.OrgCartonGroupPK);
				AssertEquals(false, order.CartonGroup.ProductCartonGroupTakesPrecedence);
			}

			// Product has the highest Priority, but Client has the highest Priority at Order level
			using (WarehouseDataRegistry.Instance.CartonGroupSequence.SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty,
				new CartonGroupSequence()
				{
					Product = 1,
					Carrier = 3,
					Consignee = 5,
					Client = 2,
					Warehouse = 4
				}))
			{
				AssertEquals(clientGroup.PK, order.CartonGroup.OrgCartonGroupPK);
				AssertEquals(true, order.CartonGroup.ProductCartonGroupTakesPrecedence);
			}
		}

		public void TestCartonGroup_ProductCartonGroupTakesPrecedence_HigherPriorityOrgsHaveNoCartonGroup()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Factory.New<WhsOrder>();
			order.WD_WW_Whs = data.Whs1.PK;
			Assert(order.CartonGroup.OrgCartonGroupPK.IsEmpty);

			using (WarehouseDataRegistry.Instance.CartonGroupSequence.SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty,
				new CartonGroupSequence()
				{
					Product = 5,
					Carrier = 3,
					Consignee = 1,
					Client = 2,
					Warehouse = 4
				}))
			{
				Assert(!order.CartonGroup.OrgCartonGroupPK.IsValid);
				AssertEquals(true, order.CartonGroup.ProductCartonGroupTakesPrecedence);
			}

			var carrier = Helper.CreateClient("CAR");

			var carrierGroup = Helper.CreateWhsCartonGroup("1", "Carrier");
			var clientGroup = Helper.CreateWhsCartonGroup("2", "WhsClient");
			var whsGroup = Helper.CreateWhsCartonGroup("3", "WhsOrg");
			var productGroup = Helper.CreateWhsCartonGroup("4", "WhsProduct");

			carrier.MiscServ.OM_WCG_CartonGroup = carrierGroup.PK;
			data.Org1.MiscServ.OM_WCG_CartonGroup = clientGroup.PK;
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			order.WD_WW_Whs = data.Whs1.PK;
			order.WD_OH_Client = data.Org1.PK;
			order.TransportCoNameOrPK = carrier.PK.ToString();
			data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner).OU_WCG_CartonGroup = productGroup.PK;

			using (WarehouseDataRegistry.Instance.CartonGroupSequence.SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty,
				new CartonGroupSequence()
				{
					Product = 2,
					Carrier = 3,
					Consignee = 1,
					Client = 5,
					Warehouse = 4
				}))
			{
				AssertEquals(carrierGroup.PK, order.CartonGroup.OrgCartonGroupPK);
				AssertEquals(true, order.CartonGroup.ProductCartonGroupTakesPrecedence);
			}
		}

		public void TestCartonGroup_SkipUnusedSources()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Factory.New<WhsOrder>();
			order.WD_WW_Whs = data.Whs1.PK;
			Assert(order.CartonGroup.OrgCartonGroupPK.IsEmpty);

			var carrier = Helper.CreateClient("CAR");
			var consignee = Helper.CreateClient("CNE");

			var carrierGroup = Helper.CreateWhsCartonGroup("1", "Carrier");
			var cneGroup = Helper.CreateWhsCartonGroup("2", "Consignee");
			var clientGroup = Helper.CreateWhsCartonGroup("3", "WhsClient");
			var whsGroup = Helper.CreateWhsCartonGroup("4", "WhsOrg");
			var productGroup = Helper.CreateWhsCartonGroup("5", "WhsProduct");

			carrier.MiscServ.OM_WCG_CartonGroup = carrierGroup.PK;
			consignee.MiscServ.OM_WCG_CartonGroup = cneGroup.PK;
			data.Org1.MiscServ.OM_WCG_CartonGroup = clientGroup.PK;
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			order.WD_WW_Whs = data.Whs1.PK;
			order.WD_OH_Client = data.Org1.PK;
			order.ConsigneeNameOrPK = consignee.PK.ToString();
			order.TransportCoNameOrPK = carrier.PK.ToString();
			data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner).OU_WCG_CartonGroup = productGroup.PK;

			// Warehouse has the highest Priority at Order level
			using (WarehouseDataRegistry.Instance.CartonGroupSequence.SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty,
				new CartonGroupSequence()
				{
					Product = 1,
					Carrier = 0,
					Consignee = 3,
					Client = 0,
					Warehouse = 2
				}))
			{
				AssertEquals(whsGroup.PK, order.CartonGroup.OrgCartonGroupPK);
				AssertEquals(true, order.CartonGroup.ProductCartonGroupTakesPrecedence);
			}
		}

		public void TestCartonGroup_AllSourcesFromOrderAreUnused()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Factory.New<WhsOrder>();
			order.WD_WW_Whs = data.Whs1.PK;
			Assert(order.CartonGroup.OrgCartonGroupPK.IsEmpty);

			var carrier = Helper.CreateClient("CAR");
			var consignee = Helper.CreateClient("CNE");

			var carrierGroup = Helper.CreateWhsCartonGroup("1", "Carrier");
			var cneGroup = Helper.CreateWhsCartonGroup("2", "Consignee");
			var clientGroup = Helper.CreateWhsCartonGroup("3", "WhsClient");
			var whsGroup = Helper.CreateWhsCartonGroup("4", "WhsOrg");
			var productGroup = Helper.CreateWhsCartonGroup("5", "WhsProduct");

			carrier.MiscServ.OM_WCG_CartonGroup = carrierGroup.PK;
			consignee.MiscServ.OM_WCG_CartonGroup = cneGroup.PK;
			data.Org1.MiscServ.OM_WCG_CartonGroup = clientGroup.PK;
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			order.WD_WW_Whs = data.Whs1.PK;
			order.WD_OH_Client = data.Org1.PK;
			order.ConsigneeNameOrPK = consignee.PK.ToString();
			order.TransportCoNameOrPK = carrier.PK.ToString();
			data.Part1.RelatedOrganisations.AddOrganisationIfNotExist(data.Org1.PK, OrgPartRelation.RelationshipTypes.Owner).OU_WCG_CartonGroup = productGroup.PK;

			using (WarehouseDataRegistry.Instance.CartonGroupSequence.SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty,
				new CartonGroupSequence()
				{
					Product = 1,
					Carrier = 0,
					Consignee = 0,
					Client = 0,
					Warehouse = 0
				}))
			{
				Assert(order.CartonGroup.OrgCartonGroupPK.IsEmpty);
				AssertEquals(true, order.CartonGroup.ProductCartonGroupTakesPrecedence);
			}
		}

		#endregion

		#region TestConsignee_DbHits

		protected override Dictionary<string, int> ExpectedDbHitsForConsigneeCore => new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 2 },
			{ OrgHeaderSchema.Constants.TableName, 1 },
			{ WhsWarehouseSchema.Constants.TableName, 1 }
		};

		#endregion

		#region Containers

		public void TestGenerateDataFromContainersNotifiesUserWhenDone()
		{
			var order = GetNewBusinessObject();
			order.NotificationManager.Push(Notify);
			order.GenerateDataFromContainers();
			Assert("Notify user", Notify.LastEvent is InfoNotification);
		}

		public void TestGenerateDataFromContainers()
		{
			var today = ZDate.Today;
			// create receive entries
			var notifyBuffer = new TestNotificationBuffer();
			var whs1 = Helper.CreateWarehouse("1", "A");
			var whs2 = Helper.CreateWarehouse("2", "A");
			var org = Helper.CreateClient();
			var part1 = Helper.CreateProduct(org, "AAA");
			var part2 = Helper.CreateProduct(org, "BBB");

			Helper.SetClientAttributeType(org, AttributeNumber.One, true);
			Helper.SetClientAttributeType(org, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(org, AttributeNumber.Three, true);
			Helper.SetClientAttributeType(org, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(org, AttributeNumber.PackingDate, true);
			Helper.SetProductAllAttributeUse(org, part1, true, useSerialNumber: false);
			Helper.SetProductAllAttributeUse(org, part2, true, useSerialNumber: false);

			// make sure order can be saved
			var order = GetNewBusinessObject();
			order.NotificationManager.Push(notifyBuffer);
			order.WD_OH_Client = org.PK;
			order.WD_WW_Whs = whs1.PK;
			order.WD_BookingDate = ZDateTimeOffset.Now;
			order.WD_ArrivalDate = ZDateTimeOffset.Now;
			order.WD_ExternalReference = "TEST";
			var ordCon1 = order.Containers.AddNew();
			ordCon1.WC_ContainerNum = "1";

			// create receive docket with 2 lines for whs1
			var inDoc1 = Helper.CreateWhsReceive(org.PK, whs1.PK, "1", notifyBuffer);
			var con1 = inDoc1.Containers.AddNew();
			con1.WC_ContainerNum = "1";
			var inDocLine1_1 = Helper.CreateWhsReceiveInventoryLine(inDoc1, part1.PK, 100);
			var inDocLine1_2 = Helper.CreateWhsReceiveInventoryLine(inDoc1, part2.PK, 50);
			Helper.SetInventoryAttributes(inDocLine1_1, today.AddMonths(2), today.AddMonths(-2), "PA1", "PA2", "PA3",
				"");
			Helper.SetInventoryAttributes(inDocLine1_2, today.AddMonths(1), today.AddMonths(-1), "PA1", "PA2", "PA3",
				"");

			inDoc1.AllocateLocationsWithMock();
			inDoc1.FinaliseDocket();
			AssertIsFinalisedPrecondition(inDoc1);

			// create receive docket with 2 lines for whs2 (multi warehouse testing)
			var inDoc2 = Helper.CreateWhsReceive(org.PK, whs2.PK, "2", notifyBuffer);
			var con2 = inDoc2.Containers.AddNew();
			con2.WC_ContainerNum = "1";
			var inDocLine2_1 = Helper.CreateWhsReceiveInventoryLine(inDoc2, part1.PK, 10, "E1-1");
			var inDocLine2_2 = Helper.CreateWhsReceiveInventoryLine(inDoc2, part1.PK, 5, "E1-1");
			Helper.SetInventoryAttributes(inDocLine2_1, today.AddMonths(2), today.AddMonths(-2), "PA1", "PA2", "PA3",
				"");
			Helper.SetInventoryAttributes(inDocLine2_2, today.AddMonths(1), today.AddMonths(-1), "PA1", "PA2", "PA3",
				"");
			inDoc2.AllocateLocationsWithMock();
			inDoc2.FinaliseDocket();
			AssertIsFinalisedPrecondition(inDoc2);

			// generate new lines from old entry
			Factory.Save(); // DBOnly query is used
			ordCon1.WC_ContainerNum = "2";
			order.GenerateDataFromContainers();
			AssertEquals("line count", 0, order.Lines.Count);

			ordCon1.WC_ContainerNum = "1";
			order.GenerateDataFromContainers();
			AssertEquals("line count", 2, order.Lines.Count);

			// if not sorted intermittent failures can occur as the order is not guaranteed
			order.Lines.ApplySort(WhsDocketLine.Schema.ProductDesc, ListSortDirection.Ascending);

			var orderLine1 = order.Lines[0];
			AssertEquals("line 1 product", inDocLine1_1.WI_OP, orderLine1.WE_OP);
			AssertEquals("line 1 qty", inDocLine1_1.InDocketLine.WE_TransactionQuantity,
				orderLine1.WE_TransactionQuantity);

			var orderLine2 = order.Lines[1];
			AssertEquals("line 2 product", inDocLine1_2.WI_OP, orderLine2.WE_OP);
			AssertEquals("line 2 qty", inDocLine1_2.InDocketLine.WE_TransactionQuantity,
				orderLine2.WE_TransactionQuantity);
			AssertEquals("line 2 expiry", inDocLine1_2.WI_ExpiryDate, orderLine2.WE_ExpiryDate);
			AssertEquals("line 2 packing", inDocLine1_2.WI_PackingDate, orderLine2.WE_PackingDate);
			AssertEquals("line 2 partattrib1", inDocLine1_2.WI_PartAttrib1, orderLine2.WE_PartAttrib1);
			AssertEquals("line 2 partattrib2", inDocLine1_2.WI_PartAttrib2, orderLine2.WE_PartAttrib2);
			AssertEquals("line 2 partattrib3", inDocLine1_2.WI_PartAttrib3, orderLine2.WE_PartAttrib3);

			Assert("Notify user", notifyBuffer.LastEvent is InfoNotification);
		}

		#endregion

		#region TestCurrentWorkOrders

		public void TestCurrentWorkOrders()
		{
			// current
			WhsWorkOrder newWorkOrder = Factory.New<WhsWorkOrder>();
			WhsWorkOrder enteredWorkOrder = Factory.New<WhsWorkOrder>();
			WhsWorkOrder heldWorkOrder = Factory.New<WhsWorkOrder>();

			// not current
			WhsWorkOrder cancelledWorkOrder = Factory.New<WhsWorkOrder>();
			WhsWorkOrder finalisedWorkOrder = Factory.New<WhsWorkOrder>();
			WhsWorkOrder childWorkOrder = Factory.New<WhsWorkOrder>();

			newWorkOrder.WD_WD_ParentDocket = Order.PK;
			enteredWorkOrder.WD_WD_ParentDocket = Order.PK;
			heldWorkOrder.WD_WD_ParentDocket = Order.PK;
			cancelledWorkOrder.WD_WD_ParentDocket = Order.PK;
			finalisedWorkOrder.WD_WD_ParentDocket = Order.PK;
			childWorkOrder.WD_WD_ParentDocket = newWorkOrder.PK;

			newWorkOrder.WD_DocketStatus = DocketStatus.Codes.New;
			enteredWorkOrder.WD_DocketStatus = DocketStatus.Codes.Entered;
			heldWorkOrder.WD_DocketStatus = DocketStatus.Codes.Held;
			cancelledWorkOrder.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			finalisedWorkOrder.WD_DocketStatus = DocketStatus.Codes.Finalised;
			childWorkOrder.WD_DocketStatus = DocketStatus.Codes.New;

			AssertCollectionContains(newWorkOrder, Order.CurrentWorkOrders);
			AssertCollectionContains(enteredWorkOrder, Order.CurrentWorkOrders);
			AssertCollectionContains(heldWorkOrder, Order.CurrentWorkOrders);

			AssertCollectionNotContains(cancelledWorkOrder, Order.CurrentWorkOrders);
			AssertCollectionNotContains(finalisedWorkOrder, Order.CurrentWorkOrders);
			AssertCollectionNotContains(childWorkOrder, Order.CurrentWorkOrders);
		}

		#endregion

		#region TestAllocatedReceipts

		public void TestAllocatedReceipts()
		{
			// create 3 receipts, but only allocate 2 of them
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "2", Notify);
			var receive3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "3", Notify);

			var receiveLine21 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);
			var receiveLine31 = Helper.CreateWhsReceiveInventoryLine(receive3, data.Part1, 10m);

			receive2.AllocateLocationsWithMock();
			receive3.AllocateLocationsWithMock();
			receive2.FinaliseDocket();
			receive3.FinaliseDocket();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreateReservePickLine(orderLine1, data.Line111, 7m);
			Helper.CreateReservePickLine(orderLine2, receiveLine31, 5m);

			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { data.Receive11, receive3 }, order.AllocatedReceipts);
			AssertEquals(typeof(WhsDocketCollectionAdHoc), order.AllocatedReceipts.GetType());
			AssertEquals("Receives should be Read Only.", true, data.Receive11.ReadOnly);
			AssertEquals("Receives should be Read Only.", true, receive3.ReadOnly);
		}

		#endregion

		#region TestAllocatedReceipts_PickedOrder

		public void TestAllocatedReceipts_PickedOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var inventory = receive.Inventory[0];

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 10m, reservedPickLine.ReservedQuantity);

			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { receive }, order.AllocatedReceipts);

			var otherFactory = new BusinessObjectFactory();
			var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);
			var pick = otherFactory.New<WhsPick>();
			pick.PickOrdersWithAllocationMock(orderInOtherFactory);
			AssertEquals("Picked Orders have no allocated Receipts.", 0, orderInOtherFactory.AllocatedReceipts.Count);
		}

		#endregion

		#region TestPackageJob

		public void TestPackageJob()
		{
			var order = GetNewBusinessObject();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			AssertEquals(packageJob, order.PackageJob);
		}

		#endregion

		#region TestPickWhenWD_WPIsEmpty

		public void TestPickWhenWD_WPIsEmpty()
		{
			var order = GetNewBusinessObject();
			var pick = Factory.New<WhsPick>();
			AssertNull(order.Pick);
			order.WD_WP = pick.PK;
			AssertEquals(pick, order.Pick);
		}

		#endregion

		#region TestLines

		protected override Type ExpectedLineCollectionType => typeof(WhsOrderLineCollection);

		#endregion

		#region TestNoteTypes

		protected override IEnumerable<PredefinedNoteType> ExpectedAdditionalNoteTypesCore
		{
			get
			{
				yield return PredefinedNoteTypes.Instance.DeliveryInstructionsNote;
				yield return PredefinedNoteTypes.Instance.PickingInstructions;
				yield return PredefinedNoteTypes.Instance.RTUSRequestLog;
			}
		}

		#endregion

		#region TestSelectedOrderLines

		public void TestSelectedOrderLines()
		{
			var testForm = new TestIPickForm();
			var pick = Factory.NewWithValidTestData<WhsPick>();
			AssertNull(pick.ParentForm);
			pick.ParentForm = testForm;
			Factory.Save();

			var order = GetNewBusinessObject();
			order.WD_OH_Client = Helper.CreateClient().PK;
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			order.ConsigneePK = order.Client.PK;

			var orderLine1 = order.Lines.AddNew();
			orderLine1.WE_TransactionQuantity = 1;
			var orderLine2 = order.Lines.AddNew();
			orderLine2.WE_TransactionQuantity = 1;
			var orderLine3 = order.Lines.AddNew();
			orderLine3.WE_TransactionQuantity = 1;
			AssertEquals("Precondition: ", 3, order.SelectedOrderLines.Count);

			var form = new TestWhsOrderIPickForm(order);
			AssertEquals(3, order.SelectedOrderLines.Count);

			order.WD_RequiredDate = ZDateTimeOffset.Now.AddDays(2);
			pick = Helper.CreatePickNew(order);
			pick.ParentForm = form;

			order = GetNewBusinessObject();
			var orderLine4 = order.Lines.AddNew();
			form.OrderLinesSelection = new List<WhsPickableDocketLine>(order.Lines.ToArray<WhsPickableDocketLine>());

			AssertEquals(1, order.SelectedOrderLines.Count);
			AssertEquals(orderLine4, order.SelectedOrderLines[0]);
		}

		#endregion

		#region TestSupplierBuyerLink

		public void TestSupplierBuyerLink()
		{
			var client = Helper.CreateClient();
			var consignee = Helper.CreateClient();
			consignee.OH_RL_NKClosestPort = "AUSYD";

			var order = GetNewBusinessObject();
			AssertNull(order.Client);
			AssertNull(order.Consignee);
			AssertNull(order.SupplierBuyerLink);

			order.WD_OH_Client = client.PK;
			AssertNull(order.SupplierBuyerLink);

			order.ConsigneePK = consignee.PK;
			AssertNull(order.SupplierBuyerLink);

			var link1 = consignee.SupplierLinks.AddNew(client);
			link1.OL_RN_NKImporterCountry = "US";
			var link2 = consignee.SupplierLinks.AddNew(client);
			link2.OL_RN_NKImporterCountry = "AU";
			AssertNotNull(order.SupplierBuyerLink);
			AssertEquals(link2, order.SupplierBuyerLink);

			order.ConsigneeDocAddress.E2_AddressOverride = true;
			AssertNull(order.SupplierBuyerLink);
		}

		#endregion

		#region TestLoadPickLinesForAllDocketLines

		protected override WhsPickableDocket GetPickableDocket_ForLoadPickLinesForAllDocketLinesTest(
			TestDataForBOM data)
		{
			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = data.Org1.PK;
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_RequiredDate = ZDateTimeOffset.Now;
			docket.ConsigneeAddressPK = data.Org1.MainAddress.PK;

			Helper.CreateWhsPickableDocketLine(docket, data.BOM.BikeEngine, 1m);
			Helper.CreateWhsPickableDocketLine(docket, data.BOM.BikeWheel, 2m);

			return docket;
		}

		#endregion

		#region TestGetRelatedJobs

		public void TestGetRelatedJobsFromPertinentJobs()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "A");
			var receive = Helper.CreateWhsReceiveWithInventory(org, whs, "R1", part, 30m);

			// first order with no parent
			var order = Helper.CreateWhsOrderWithOrderLine(org, whs, "1", part, 10m);
			order.WD_DocketSubType = "ORD";
			order.WD_DocketID = "W00045076";
			order.WD_ExternalReference = "W00045076";
			order.WD_ExternalReferenceSplit = 0;
			order.WD_FinalisedDate = DateTime.Today.AddDays(-3);
			var pick1 = Helper.CreatePickNew();
			pick1.WP_WW_Whs = order.Warehouse.PK;
			order.WD_WP = pick1.PK;
			order.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			Factory.Save();

			// order is parent for backorder
			var backorder = Helper.CreateWhsOrderWithOrderLine(org, whs, "1", part, 10m);
			backorder.WD_DocketID = "W00045234";
			backorder.WD_DocketSubType = "BAK";
			backorder.WD_ExternalReference = "W00045076";
			backorder.WD_ExternalReferenceSplit = 1;
			backorder.WD_FinalisedDate = DateTime.Today.AddDays(-1);
			backorder.WD_WD_ParentDocket = order.PK;
			var pick2 = Helper.CreatePickNew();
			pick2.WP_WW_Whs = backorder.Warehouse.PK;
			backorder.WD_WP = pick2.PK;
			backorder.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			Factory.Save();

			// backorder is parent for backBackOrder
			var backBackOrder = Helper.CreateWhsOrderWithOrderLine(org, whs, "1", part, 10m);
			backBackOrder.WD_DocketID = "W00045322";
			backBackOrder.WD_ExternalReference = "W00045076";
			backBackOrder.WD_DocketSubType = "BAK";
			backBackOrder.WD_ExternalReferenceSplit = 2;
			backBackOrder.WD_DocketStatus = "PIC";
			backBackOrder.WD_WD_ParentDocket = backorder.PK;
			var pick3 = Helper.CreatePickNew();
			pick3.WP_WW_Whs = backBackOrder.Warehouse.PK;
			backBackOrder.WD_WP = pick3.PK;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(
				"Ensure there is no stack overflow getting back orders related orders related orders.",
				new[] { backorder, backBackOrder }, order.RelatedJobs);
			AssertNoExceptionThrown(
				"Ensure there is no stack overflow getting back orders related orders related orders.", () =>
				{
					var poke = order.RelatedJobs;
				});
		}

		#endregion

		#region TestRelatedJobs

		protected override List<IRelatedJob> GetValidRelatedJobs(WhsOrder docket)
		{
			docket.FillWithValidTestData();

			var shipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var pivot = Factory.New<WhsDocketJobPivot>();
			pivot.WV_ParentId = shipment.PK;
			pivot.WV_ParentTableCode = shipment.TablePrefix;
			pivot.WV_DocketType = DocketType.Codes.Order;
			pivot.WV_WD_Docket = docket.PK;

			var cartageJob = (BusinessObject)Factory.New<ICommonCartage>();
			cartageJob.FillWithValidTestData();
			cartageJob[JobCartageSchema.JJ_ParentID] = docket.PK;
			cartageJob[JobCartageSchema.JJ_ParentTableCode] = docket.TablePrefix;
			cartageJob[JobCartageSchema.JJ_ConsignmentID] = docket.WD_DocketID;

			var childWorkOrder1 = Factory.NewWithValidTestData<WhsWorkOrder>();
			var childWorkOrder2a = Factory.NewWithValidTestData<WhsWorkOrder>();
			var childWorkOrder2b = Factory.NewWithValidTestData<WhsWorkOrder>();
			childWorkOrder1.WD_WD_ParentDocket = docket.PK;
			childWorkOrder2a.WD_WD_ParentDocket = childWorkOrder1.PK;
			childWorkOrder2b.WD_WD_ParentDocket = childWorkOrder1.PK;

			var receive1 = Factory.NewWithValidTestData<WhsReceive>();
			var receive2 = Factory.NewWithValidTestData<WhsReceive>();
			receive1.WD_WD_ParentDocket = docket.PK;
			receive2.WD_WD_ParentDocket = docket.PK;

			var transfer = Factory.NewWithValidTestData<WhsTransfer>();
			transfer.WD_WD_ParentDocket = docket.PK;

			var consol = (IDtbBookingConsolidation)Factory.New(ObjectFactory.GetType<IDtbBookingConsolidation>());
			consol.KB_ParentID = docket.PK;
			consol.KB_ParentTableCode = WhsDocketSchema.Constants.Prefix;

			var booking = (IDtbBooking)Factory.New(ObjectFactory.GetType<IDtbBooking>());
			booking.KM_KB_Booking = consol.PK;
			booking.KM_JobID = "Booking 1";

			var portTransportJob =
				(ICommonCartage)Factory.New(
					ObjectFactory.GetType<ICommonCartage>());
			portTransportJob.JJ_ParentID = booking.PK;
			portTransportJob.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			Factory.Save();

			return new List<IRelatedJob>()
			{
				childWorkOrder1,
				childWorkOrder2a,
				childWorkOrder2b,
				receive1,
				receive2,
				transfer,
				(IRelatedJob)cartageJob,
				(IRelatedJob)shipment,
				(IRelatedJob)booking,
				(IRelatedJob)portTransportJob
			};
		}

		protected override List<IRelatedJob> GetInvalidRelatedJobs(WhsOrder docket)
		{
			docket.FillWithValidTestData();
			var unrelatedOrder = Factory.NewWithValidTestData<WhsOrder>();
			var unrelatedWorkOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			var unrelatedReceive = Factory.NewWithValidTestData<WhsReceive>();
			var unrelatedShipment =
				(BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var pivot = Factory.New<WhsDocketJobPivot>();
			pivot.WV_ParentId = unrelatedShipment.PK;
			pivot.WV_ParentTableCode = unrelatedShipment.TablePrefix;
			pivot.WV_DocketType = DocketType.Codes.Receive;
			pivot.WV_WD_Docket = docket.PK;

			Factory.Save();

			return new List<IRelatedJob>()
			{
				unrelatedOrder, unrelatedWorkOrder, unrelatedReceive, (IRelatedJob)unrelatedShipment
			};
		}

		#endregion

		#region TestConsignee

		protected override void TestConsigneeNameOrPKReadOnlyCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			AssertEquals("Precondition", false, order.ConsigneeNameOrPKInfo.ReadOnly);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10);
			var docketLine = Helper.CreateWhsPickableDocketLine(order, data.Part1, 10);
			order.Lines.Add(docketLine);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertEquals("Should not be read only if there are no Julian products.", false,
				order.ConsigneeDocAddress.ReadOnly);
			AssertEquals("Should not be read only if there are no Julian products.", false,
				order.ConsigneeNameOrPKInfo.ReadOnly);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One,
				PartAttributeTypeList.Codes.JulianBatchNumber);
			AssertEquals(
				"Should not be read only if there are no Julian products, even if client has julian batch number attribute.",
				false, order.ConsigneeDocAddress.OrganisationPKInfo.ReadOnly);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			var productParam = Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);

			order.ConsigneePK = Helper.CreateClient("CNE").PK;
			AssertEquals("The control should be unaffected and remain not read only.", false,
				order.ConsigneeDocAddress.ReadOnly);
			AssertEquals("Should be read only if there are Julian products.", true,
				order.ConsigneeDocAddress.OrganisationPKInfo.ReadOnly);
			AssertEquals("Should be read only if there are Julian products.", true,
				order.ConsigneeNameOrPKInfo.ReadOnly);
			AssertEquals("Consignee address should be changeable.", false,
				order.ConsigneeDocAddress.E2_OA_AddressInfo.ReadOnly);

			Factory.Save();
			var orgPkInfo =
				order.ConsigneeDocAddress
					.OrganisationPKInfo; // prevent poking ConsigneeDocAddress to test readonly is updated by DataRefresh
			AssertEquals("Precondition", true, orgPkInfo.ReadOnly);

			if (!Globals.IsWeb)
			{
				// DataRefreshManager is not Enabled when Globals.IsWeb = true
				var otherFactory = new BusinessObjectFactory();
				var pickInOtherFactory = otherFactory.Load<WhsPick>(pick.PK);
				pickInOtherFactory.CancelPick();
				otherFactory.Save();
				AssertEquals("Precondition", true, pickInOtherFactory.IsCancelled);
				AssertEquals("Precondition", DocketStatus.Codes.Entered, order.WD_DocketStatus);
				AssertEquals("Should no longer be read only.", false, orgPkInfo.ReadOnly);
				AssertEquals("Should no longer be read only.", false, order.ConsigneeNameOrPKInfo.ReadOnly);
			}
		}

		public void TestConsigneeNameOrPKReadOnly_ExpiryDatePartAttributeUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			AssertEquals("Precondition", false, order.ConsigneeNameOrPKInfo.ReadOnly);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10);
			var docketLine = Helper.CreateWhsPickableDocketLine(order, data.Part1, 10);
			order.Lines.Add(docketLine);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertEquals("Should not be read only if there are no products with expiry date.", false,
				order.ConsigneeDocAddress.ReadOnly);
			AssertEquals("Should not be read only if there are no products with expiry date.", false,
				order.ConsigneeNameOrPKInfo.ReadOnly);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);

			order.ConsigneePK = Helper.CreateClient("CNE").PK;
			AssertEquals("The control should be unaffected and remain not read only.", false,
				order.ConsigneeDocAddress.ReadOnly);
			AssertEquals("Should be read only if there are products with expiry dates.", true,
				order.ConsigneeDocAddress.OrganisationPKInfo.ReadOnly);
			AssertEquals("Should be read only if there are products with expiry dates.", true,
				order.ConsigneeNameOrPKInfo.ReadOnly);
			AssertEquals("Consignee address should be changeable.", false,
				order.ConsigneeDocAddress.E2_OA_AddressInfo.ReadOnly);

			Factory.Save();
			var orgPkInfo =
				order.ConsigneeDocAddress
					.OrganisationPKInfo; // prevent poking ConsigneeDocAddress to test readonly is updated by DataRefresh
			AssertEquals("Precondition", true, orgPkInfo.ReadOnly);

			if (!Globals.IsWeb)
			{
				// DataRefreshManager is not Enabled when Globals.IsWeb = true
				var otherFactory = new BusinessObjectFactory();
				var pickInOtherFactory = otherFactory.Load<WhsPick>(pick.PK);
				pickInOtherFactory.CancelPick();
				otherFactory.Save();
				AssertEquals("Precondition", true, pickInOtherFactory.IsCancelled);
				AssertEquals("Precondition", DocketStatus.Codes.Entered, order.WD_DocketStatus);
				AssertEquals("Should no longer be read only.", false, orgPkInfo.ReadOnly);
				AssertEquals("Should no longer be read only.", false, order.ConsigneeNameOrPKInfo.ReadOnly);
			}
		}

		public void TestConsigneeDocAddress_ReadOnlyIsLazyTriggered()
		{
			var pickableDocket = GetNewBusinessObject();
			AssertNotNull("Poke & Precondition", pickableDocket.ConsigneeDocAddress);

			AssertPersistentPropertiesHitCount("Getting ConsigneeDocAddress should not trigger property hits.", 0,
				() => _ = pickableDocket.ConsigneeDocAddress);

			var hits = GetPersistentPropertiesHitCount(() => _ = pickableDocket.ConsigneeDocAddress.ReadOnly);
			AssertEquals("Poking at ReadOnly property should trigger state calculation.", true, hits > 0);
		}

		#endregion

		#region TestRelatedBackOrder

		public void TestRelatedBackOrder()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			data.Whs1.WW_IsBondedWarehouse = true;
			Helper.CreateArea(data.Whs1, "BONDED").WA_AreaType = "BON";

			var client2 = Helper.CreateClient("Client2");
			var part2 = Helper.CreateProduct(client2, "P2");
			var part3 = Helper.CreateProduct(client2, "P3");
			var part4 = Helper.CreateProduct(client2, "P4");

			var receive2 = Helper.CreateWhsReceive(client2, data.Whs1, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive2, part2, 100m);
			Helper.CreateWhsReceiveInventoryLine(receive2, part3, 100m);
			Helper.CreateWhsReceiveInventoryLine(receive2, part4, 100m);
			receive2.AllocateLocationsWithMock();
			receive2.FinaliseDocket();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			Helper.CreateWhsOrderLine(order1, data.Part1, 50m);
			var order2 = Helper.CreateWhsOrder(client2, data.Whs1, "O2", Notify);
			Helper.CreateWhsOrderLine(order2, part2, 50m);
			var order3 = Helper.CreateWhsOrder(client2, data.Whs1, "O3", Notify);
			Helper.CreateWhsOrderLine(order3, part3, 200m);
			var order4 = Helper.CreateWhsOrder(client2, data.Whs1, "O4", Notify);
			order4.WD_DocketSubType = OrderType.Codes.Customs;
			Helper.CreateWhsOrderLine(order4, part4, 200m, "123-1", "DummyOutward-1", "");
			Factory.Save();

			data.Org1.MiscServ.OM_WhsGenerateBackOrdersOnShortfalls = false;
			client2.MiscServ.OM_WhsGenerateBackOrdersOnShortfalls = true;

			var pick = Helper.CreatePickNew(new[] { order1, order2, order3, order4 });
			pick.FinaliseAllOrders();
			pick.FinalisePick();

			AssertIsFinalisedPrecondition(order1);
			AssertIsFinalisedPrecondition(order2);
			AssertIsFinalisedPrecondition(order3);
			AssertIsFinalisedPrecondition(order4);
			AssertIsFinalisedPrecondition(pick);

			AssertEquals("Precondition", false, order1.Client.MiscServ.OM_WhsGenerateBackOrdersOnShortfalls);
			AssertEquals("Precondition", true, order2.Client.MiscServ.OM_WhsGenerateBackOrdersOnShortfalls);
			AssertEquals("Precondition", true, order3.Client.MiscServ.OM_WhsGenerateBackOrdersOnShortfalls);
			AssertEquals("Precondition", true, order4.Client.MiscServ.OM_WhsGenerateBackOrdersOnShortfalls);

			AssertEquals("Precondition", false, order1.GetShortfallExistsStatus());
			AssertEquals("Precondition", false, order2.GetShortfallExistsStatus());
			AssertEquals("Precondition", true, order3.GetShortfallExistsStatus());
			AssertEquals("Precondition", true, order4.GetShortfallExistsStatus());

			AssertNull(
				"No Back Order should exist as there are no shortfalls and Generate Back Orders On Shortfall checkbox is turned OFF",
				order1.RelatedBackOrder);
			AssertNull(
				"No Back Order should exist ever through Generate Back Orders On Shortfall checkbox is turned ON, because there are no shortfalls",
				order2.RelatedBackOrder);
			AssertNotNull("Back Order should have been created", order3.RelatedBackOrder);
			AssertNull("Back Order should *not* be created for CUS jobs.", order4.RelatedBackOrder);
		}

		#endregion

		#region TestCarrierBookingAgent

		public void TestCarrierBookingAgent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			Factory.Save();

			AssertNotNull(order.CarrierBookingAgentDocAddress);
			AssertEquals(order.CarrierBookingAgentDocAddress.DocAddressType, DocAddressType.CarrierBookingAgent);
			AssertNull(order.CarrierBookingAgent);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.MainAddress;

			order.CarrierBookingAgentDocAddress.E2_OA_Address = address.PK;
			AssertEquals(org.PK, order.CarrierBookingAgentDocAddress.OrganisationPK);
			AssertEquals(org.PK, order.CarrierBookingAgentPK);
		}

		#region TestCarrierBookingAgentDocAddressDefaultTypes

		public void TestCarrierBookingAgentDocAddressDefaultTypes()
		{
			var order = GetNewBusinessObject();
			AssertDocAddressDefaultTypes(order, DocAddressType.CarrierBookingAgent,
				order.CarrierBookingAgentDocAddressRequirement, ContactType.NoContactType);
			Assert(!order.CarrierBookingAgentDocAddressRequirement.CanOverride);
		}

		#endregion

		#region TestReturnAddress

		public void TestReturnAddress()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			Factory.Save();

			AssertNotNull(order.ReturnDocAddress);
			AssertEquals(order.ReturnDocAddress.DocAddressType, DocAddressType.ReturnAddress);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.MainAddress;
			order.ReturnDocAddress.E2_OA_Address = address.PK;

			AssertEquals(org.PK, order.ReturnDocAddress.OrganisationPK);
		}

		#endregion

		#region TestTestReturnAddressDefaultTypes

		public void TestTestReturnAddressDefaultTypes()
		{
			var order = GetNewBusinessObject();

			AssertDocAddressDefaultTypes(order, DocAddressType.ReturnAddress, order.ReturnDocAddressRequirement, ContactType.NoContactType);
			Assert(order.ReturnDocAddressRequirement.CanOverride);
		}

		#endregion

		#endregion

		#region TestPlannedLoad

		public void TestPlannedLoad()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			Factory.Save();

			AssertNull(order.PlannedLoad);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation);

			order.WD_WLO_PlannedLoad = load.PK;
			AssertNotNull(order.PlannedLoad);
			AssertEquals(load.PK, order.WD_WLO_PlannedLoad);
			AssertEquals(load, order.PlannedLoad);
		}

		#endregion

		#endregion

		#region Validation

		protected override Type GetExpectedValidationType()
		{
			return typeof(WhsOrderValidation);
		}

		#region TestDocAddressValidation

		public virtual void TestDocAddressValidation()
		{
			var docket = GetNewBusinessObject();
			AssertNotNull(docket.TransportCoDocAddress.Validation);
			Assert(docket.TransportCoDocAddress.Validation.ContainsPiggybackedValidation(
				GetExpectedDocAddressValidationType()));
		}

		#endregion

		#region TestDocAddressValidationUS

		public virtual void TestDocAddressValidationUS()
		{
			var docket = GetNewBusinessObject();
			US.Testing.WhsTestHelperFunctionsUS helper = new US.Testing.WhsTestHelperFunctionsUS(Factory);
			var whs = helper.CreateWarehouse("WHSUS");
			docket.WD_WW_Whs = whs.PK;
			AssertNotNull(docket.TransportCoDocAddress.Validation);
			Assert(docket.TransportCoDocAddress.Validation.ContainsPiggybackedValidation(
				GetExpectedUSDocAddressValidationType()));
		}

		#endregion

		#region RunPreSaveValidation

		#region TestRunPreSaveValidation_PickLineWithNullInventory

		public void TestRunPreSaveValidation_PickLineWithNullInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			AssertEquals("Precondition: In database", true, orderLine.IsInDatabase);
			AssertEquals("Order has no reserved stock", false, order.HasReservedStock);

			orderLine.ReserveStockIfAbleTo(receive.Inventory[0], 10m);
			AssertEquals("Order has reserved stock", true, order.HasReservedStock);

			orderLine.PickLines[0].WZ_WE_InventoryLine = receive.PK;
			AssertEquals(false, orderLine.PickLines[0].WZ_WE_InventoryLine.IsEmpty);
			AssertNoExceptionThrown(() => order.RunPreSaveValidation());
		}

		#endregion

		#region TestRunPreSaveValidation_BuildsReleaseLinesDuringFinalisation

		public void TestRunPreSaveValidation_BuildsReleaseLinesDuringFinalisation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var otherFactory = new BusinessObjectFactory();
			var orderLineInOtherFactory = otherFactory.Load<WhsOrderLine>(orderLine.PK);
			orderLineInOtherFactory.WE_TransactionQuantity = 5m; // make the pick lines over released.
			AssertEquals("Precondition: Release Lines not built.", false,
				orderLineInOtherFactory.IsReleaseLineCollectionBuilt);

			orderLineInOtherFactory.RunPreSaveValidation();
			AssertEquals("Release Lines should only be built during Finalisation.", false,
				orderLineInOtherFactory.IsReleaseLineCollectionBuilt);

			var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);
			orderInOtherFactory.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Order should not have finalised.", false, orderInOtherFactory.IsFinalised);
			AssertEquals("Release Lines should be built during Finalisation.", true,
				orderLineInOtherFactory.IsReleaseLineCollectionBuilt);
			AssertHasError(orderLineInOtherFactory.ReleaseLines[0].QuantityInfo,
				"Quantity Met cannot be greater than Quantity Ordered");

			orderLineInOtherFactory.ClearReleaseLines();
			AssertEquals("Precondition: Release Lines are cleared.", false,
				orderLineInOtherFactory.IsReleaseLineCollectionBuilt);

			orderInOtherFactory.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Order should not have finalised.", false, orderInOtherFactory.IsFinalised);
			AssertEquals("Release Lines should be built during Finalisation.", true,
				orderLineInOtherFactory.IsReleaseLineCollectionBuilt);
			AssertHasError(orderLineInOtherFactory.ReleaseLines[0].QuantityInfo,
				"Quantity Met cannot be greater than Quantity Ordered");
		}

		#endregion

		#region TestRunPreSaveValidation_WhenJobInErrorAndNotFixed

		public virtual void TestRunPreSaveValidation_WhenJobInErrorAndNotFixed()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			var docket = Helper.CreateWhsOrder(org, whs, "1");

			Factory.Save();

			docket.WD_WW_Whs = ZGuid.Empty;
			docket.RunPreSaveValidation();
			docket.WD_DocketStatus = DocketStatus.Codes.Error;

			AssertEquals("Precondition", DocketStatus.Codes.Error, docket.WD_DocketStatus);
			AssertEquals("Precondition", true, docket.HasErrors);

			docket.RunPreSaveValidation();

			AssertEquals("WD_DocketStatus must remain 'ERR'", DocketStatus.Codes.Error,
				docket.WD_DocketStatus);
			AssertEquals("Docket should still have errors", true, docket.HasErrors);
		}

		#endregion

		#region TestRunPreSaveValidation_WhenJobInErrorAndFixed

		public virtual void TestRunPreSaveValidation_WhenJobInErrorAndFixed()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			var docket = Helper.CreateWhsOrder(org, whs, "1");

			Factory.Save();

			docket.RunPreSaveValidation();
			docket.WD_DocketStatus = DocketStatus.Codes.Error;

			AssertEquals("Precondition", DocketStatus.Codes.Error, docket.WD_DocketStatus);
			AssertEquals("Precondition", false, docket.HasErrors);

			docket.RunPreSaveValidation();

			AssertEquals("WD_DocketStatus must be set to 'ENT'", DocketStatus.Codes.Entered,
				docket.WD_DocketStatus);
			AssertEquals("Docket Should not have any errors", false, docket.HasErrors);
		}

		#endregion

		#region TestRunPreSaveValidation_FetchHintsUseTVP

		public void TestRunPreSaveValidation_FetchHintsUseTVP()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			for (var index = 0; index < 500; index++)
			{
				Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			}

			Factory.ExecuteAllFetchHints();
			order.RunPreSaveValidation();

			using (TestConnection.TrackExecutedCommands())
			{
				Factory.ExecuteAllFetchHints();

				var locationQueryCommand = TestConnection.ExecutedCommands.SingleOrDefault(c =>
					c.Contains("SELECT WE_WL FROM dbo.WhsDocketLine WHERE WE_PK IN"));
				AssertContains("Allocation query should use TVP.", "(WZ_WE_TransactionLine in (SELECT Value FROM",
					locationQueryCommand);
			}
		}

		public override void TestRunPreSaveValidationCore_SynchronisesOffset_UpdatesDocketLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var dateTime = new ZDateTime(2024, 06, 12, 12, 30, 00);

			orderLine.WE_AdjustmentArrivalDate = new ZDateTimeOffset(dateTime, TimeSpan.FromHours(0));

			order.RunPreSaveValidation();

			Assert("Docket should not be in error", !order.HasErrors);

			var expectedOffset = data.Whs1.GetWarehouseBranchDateTimeOffset(dateTime);
			AssertEquals(
				"Offsets should have same value, including offset component",
				expectedOffset.ToString("dd-MMM-yyyy hh:mm:ss zzz"),
				orderLine.WE_AdjustmentArrivalDate.ToString("dd-MMM-yyyy hh:mm:ss zzz"));
		}

		#endregion

		#endregion

		#endregion

		#region Lookups

		protected override Type GetExpectedLookupsType()
		{
			return typeof(WhsOrderLookups);
		}

		#endregion

		#region Printing

		#region TestGetRFPickPackPrinter

		public void TestGetRFPickPackPrinter()
		{
			var printer1 = Helper.CreatePrintQueue("PRINTER1");
			var printer2 = Helper.CreatePrintQueue("PRINTER2");
			var printer3 = Helper.CreatePrintQueue("PRINTER3");
			var printer4 = Helper.CreatePrintQueue("PRINTER4");

			var document1 = Factory.New<StmMenuItem>();
			document1.SU_BusinessContext = nameof(BusinessContext.WhsOrder);
			document1.SU_MenuName = "Pick Pack Document 1";

			var document2 = Factory.New<StmMenuItem>();
			document2.SU_BusinessContext = nameof(BusinessContext.WhsOrder);
			document2.SU_MenuName = "Pick Pack Document 2";

			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "GHE";
			newStaff.GS_EmailAddress = "staff1@edi.com.au";

			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Factory.Save();
			AssertNull("No Printer set.", order.GetRFPickPackPrinter(GlbStaff.CurrentUser, document1));

			Helper.CreateDefaultPrinter(document2.PK, GlbStaff.CurrentUser, printer1.PK, 1);
			Factory.Save();
			AssertNull("No Printer set for this document.",
				order.GetRFPickPackPrinter(GlbStaff.CurrentUser, document1));

			Helper.CreateDefaultPrinter(document1.PK, newStaff, printer1.PK, 1);
			Factory.Save();
			AssertNull("No Printer set for this staff.", order.GetRFPickPackPrinter(GlbStaff.CurrentUser, document1));

			Helper.CreateDefaultPrinter(document1.PK, GlbStaff.CurrentUser, printer1.PK, 1);
			Factory.Save();
			AssertEquals("The final fallback should be the Printer set against the Document.", printer1,
				order.GetRFPickPackPrinter(GlbStaff.CurrentUser, document1));

			data.Whs1.RFPickPackPrinterPK = printer2.PK;
			AssertEquals("The next fallback should be the Printer set against the Warehouse.", printer2,
				order.GetRFPickPackPrinter(GlbStaff.CurrentUser, document1));

			var ddlArea = Helper.CreateArea(data.Whs1, "DockDoor");
			ddlArea.RFPickPackPrinterPK = printer3.PK;
			var ddlRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "DDL", 1, 1);
			var dockDoorLocation = ddlRow.Locations.Single();
			dockDoorLocation.WLV_WA_PickingArea = ddlArea.PK;
			AssertEquals(
				"Dock Door Area has a Printer but the Order does not have the Pick with a DDL location from the area.",
				printer2, order.GetRFPickPackPrinter(GlbStaff.CurrentUser, document1));

			var pick = Helper.CreatePickNew(order);
			pick.WP_WL_DockDoor = dockDoorLocation.PK;
			AssertEquals("The next fallback should be the Printer set against the DDL of the Pick.", printer3,
				order.GetRFPickPackPrinter(GlbStaff.CurrentUser, document1));

			ddlArea.RFPickPackPrinterPK = ZGuid.Empty;
			AssertEquals("Even though the DDL is set on the Pick, the Dock Door Area has no Printer.", printer2,
				order.GetRFPickPackPrinter(GlbStaff.CurrentUser, document1));
		}

		public void TestGetRFPickPackPrinter_WhenMenuItemIsNull()
		{
			var printer = Helper.CreatePrintQueue("PRINTER1");
			var document = Factory.New<StmMenuItem>();
			document.SU_BusinessContext = nameof(BusinessContext.WhsOrder);
			document.SU_MenuName = "Pick Pack Document 1";

			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "GHE";
			newStaff.GS_EmailAddress = "staff1@edi.com.au";

			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);

			Helper.CreateDefaultPrinter(ZGuid.Empty, GlbStaff.CurrentUser, printer.PK, 1);
			Helper.CreateDefaultPrinter(document.PK, GlbStaff.CurrentUser, printer.PK, 1);

			AssertNull("If passing in null, should never fallback to the default Printer for the User.",
				order.GetRFPickPackPrinter(GlbStaff.CurrentUser, null));
		}

		#endregion

		#region Packing Slip

		public void TestPrintPackingSlip()
		{
			var mockPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			mockPrintTaskUIProvider
				.Setup(m => m.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ISecurityCheckpoint>()))
				.Returns(true);

			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskUIProvider.Object))
			{
				WhsDocumentPrinter.LastPrintedDocumentName = "";
				AssertEquals("Precondition", "", WhsDocumentPrinter.LastPrintedDocumentName);

				var client = NewClientContact();
				var order = GetNewBusinessObject();
				order.WD_OH_Client = client.PK;

				order.PrintPackingSlip();
				var printer = new WhsPackingSlipDocumentPrinter(order, new TestNotificationBuffer());
				AssertEquals(printer.DocumentMenuName, WhsDocumentPrinter.LastPrintedDocumentName);
			}
		}

		public void TestPrintPackingSlip_WhenDataImporting()
		{
			WhsDocumentPrinter.LastPrintedDocumentName = "";
			AssertEquals("Precondition", "", WhsDocumentPrinter.LastPrintedDocumentName);

			var order = GetNewBusinessObject();
			((ISupportDataImporting)order).IsImportingData = true;
			order.PrintPackingSlip();
			var printer = new WhsPackingSlipDocumentPrinter(order, new TestNotificationBuffer());
			AssertEquals("Nothing should print during DataImport.", "", WhsDocumentPrinter.LastPrintedDocumentName);
		}

		OrgHeader NewClientContact()
		{
			var deliveryMethod = "EML";

			var result = Factory.NewWithValidTestData<OrgHeader>();
			result.OH_FullName = "Client" + deliveryMethod;

			var contact = result.Contacts.AddNew();
			contact.OC_ContactName = deliveryMethod;
			contact.OC_Fax = "131314";
			contact.OC_Email = "unit.test@cw1.com";

			var document = contact.Documents.AddNew();
			document.OD_DeliverBy = deliveryMethod;
			document.OD_DocumentGroup = ContactType.All.Code;

			return result;
		}

		#region OnOrderToPrint

		public void TestOnWhsOrderToPrint()
		{
			WhsOrder order = Factory.New<WhsOrder>();
			MockForm mockForm = new MockForm(order);
			AssertEquals("Nothing printed", false, mockForm.Printed);

			order.CallOnWhsOrderToPrint(this, null);
			AssertEquals("Nothing printed", false, mockForm.Printed);

			try
			{
				mockForm.BindEvent();
				order.CallOnWhsOrderToPrint(this, null);
				AssertEquals("Printed", true, mockForm.Printed);
			}
			finally
			{
				mockForm.UnbindEvent();
			}
		}

		class MockForm
		{
			internal MockForm(WhsOrder order)
			{
				this.order = order;
				Printed = false;
			}

			internal void BindEvent()
			{
				order.OnWhsOrderToPrint += Order_OnWhsOrderToPrint;
			}

			internal void UnbindEvent()
			{
				order.OnWhsOrderToPrint -= Order_OnWhsOrderToPrint;
			}

			void Order_OnWhsOrderToPrint(object sender, WhsOrderToPrintEventArgs e)
			{
				Printed = true;
			}

			internal bool Printed { get; set; }

			readonly WhsOrder order;
		}

		#endregion

		#endregion

		#endregion

		#region Properties

		#region TestClientPickingParams

		public void TestClientPickingParams()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			AssertNull("Precondition", order.ClientPickingParams);

			var pickParams1 = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			AssertNull("Should not find PickingParams if no Warehouse is specified.", order.ClientPickingParams);

			pickParams1.WPP_WW_Warehouse = data.Whs1.PK;
			AssertEquals("Should find PickingParams.", pickParams1, order.ClientPickingParams);

			pickParams1.WPP_WW_Warehouse = Helper.CreateWarehouse("TEST").PK;
			AssertNull("Should not find PickingParams if they are assigned to another warehouse.",
				order.ClientPickingParams);

			var pickParams2 = WhsClientPickingParams.GetClientPickingParams(Helper.CreateClient("2"))
				.WarehousePickPackParams.AddNew();
			pickParams2.WPP_WW_Warehouse = data.Whs1.PK;
			AssertNull("Should not find PickingParams if they are assigned to another client.",
				order.ClientPickingParams);
		}

		public void TestClientPickingParams_SalesChannel()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			AssertNull("Precondition", order.ClientPickingParams);

			var pickParams1 = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams1.WPP_WW_Warehouse = data.Whs1.PK;
			AssertEquals("Should find PickingParams.", pickParams1, order.ClientPickingParams);

			var salesChannel = Helper.CreateWhsSalesChannel("ECO", "eCommerce");
			order.WD_WSH_SalesChannel = salesChannel.PK;
			AssertEquals("Should find PickingParams. As it defaults to the Parameter with empty Sales Channel.", pickParams1, order.ClientPickingParams);

			var otherSalesChannel = Helper.CreateWhsSalesChannel("DBT", "Distribution");
			pickParams1.WPP_WSH_SalesChannel = otherSalesChannel.PK;
			AssertNull("Should not find PickingParams as nothing matches the sales channel and there is no empty fallback.", order.ClientPickingParams);

			order.WD_WSH_SalesChannel = otherSalesChannel.PK;
			AssertEquals("Should find PickingParams. As it finds the Parameter with matching Sales Channel.", pickParams1, order.ClientPickingParams);

			var pickParams2 = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams2.WPP_WW_Warehouse = data.Whs1.PK;
			AssertEquals("Should find PickingParams. As it finds the Parameter with matching Sales Channel.", pickParams1, order.ClientPickingParams);

			pickParams1.WPP_WW_Warehouse = Helper.CreateWarehouse("TEST").PK;
			AssertEquals("Should find PickingParams. As it finds the Parameter with empty Sales Channel.", pickParams2, order.ClientPickingParams);

			pickParams2.Delete();
			AssertNull("Should not find PickingParams if they are assigned to another warehouse.", order.ClientPickingParams);
		}

		#endregion

		#region TestIsFinaliseAllowed

		protected override bool CheckIsHeldByCustomsWhenFinalising
		{
			get { return true; }
		}

		#endregion

		#region TestFinalisedDate

		public void TestFinalisedDate()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = ZBool.False;
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "AAA");

			var receive = Helper.CreateWhsReceive(org, whs, "1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, part, 10);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			// test default behaviour
			var order1 = Helper.CreateWhsOrder(org, whs, "1", Notify);
			Helper.CreateWhsOrderLine(order1, part, 5);

			Helper.CreatePickNew(order1);
			order1.FinaliseDocket();

			var today = ZDateTimeOffset.Today;
			AssertEquals(true, order1.WD_FinalisedDate.Date == today.Date);

			using (WarehouseDataRegistry.Instance.AllowWarehouseOrderFinalisationDateUpTo30DaysInTheFuture
					   .SetTemporaryValue(Guid.Empty, whs.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, true))
			{
				// test override behaviour
				whs.WW_UseRequiredDateForOutwardsFinalisedDate = ZBool.True;
				var order2 = Helper.CreateWhsOrder(org, whs, "2", Notify);
				order2.WD_RequiredDate = ZDateTimeOffset.Today.AddDays(7);
				Helper.CreateWhsOrderLine(order2, part, 5);

				Helper.CreatePickNew(order2);
				order2.FinaliseDocket();
				AssertEquals(ZDateTimeOffset.Today.AddDays(7), order2.WD_RequiredDate);
				AssertEquals(order2.WD_RequiredDate, order2.WD_FinalisedDate);
			}
		}

		#endregion

		#region TestGoodsDescriptionWithFallback

		public void TestGoodsDescriptionWithFallback()
		{
			var order = Factory.New<WhsOrder>();
			order.WD_GoodsDescription = "Guitars";
			AssertEquals("Guitars", order.GoodsDescriptionWithFallback);

			order.WD_GoodsDescription = "";
			AssertEquals("0 Box", order.GoodsDescriptionWithFallback);

			order.WD_UnitsSent = 5;
			AssertEquals("5 Boxes", order.GoodsDescriptionWithFallback);

			order.WD_PalletsSent = 5;
			AssertEquals("5 Pallets", order.GoodsDescriptionWithFallback);

			order.WD_PalletsSent = 1;
			AssertEquals("1 Pallet", order.GoodsDescriptionWithFallback);

			order.WD_PalletsSent = 0;
			order.WD_PackagesSent = 5;
			AssertEquals("5 Parcels", order.GoodsDescriptionWithFallback);

			order.WD_PackagesSent = 1;
			AssertEquals("1 Parcel", order.GoodsDescriptionWithFallback);
		}

		#endregion

		#region TestPortOfOrigin

		public void TestPortOfOrigin()
		{
			var branch = Helper.CreateGlbBranch("BR1");
			var client = Helper.CreateClient("CLIENT");
			var address = client.Addresses.AddNew(OrgAddressType.Delivery, false);
			var warehouse = Helper.CreateWarehouse("WHS", address, branch);
			client.OH_RL_NKClosestPort = "AUSYD";
			address.OA_RL_NKRelatedPortCode = "NZAKL";

			var order = Factory.New<WhsOrder>();
			AssertEquals("Precondition", "", order.PortOfOrigin);

			order.WD_WW_Whs = warehouse.PK;
			AssertEquals("NZAKL", order.PortOfOrigin);

			address.OA_RL_NKRelatedPortCode = "";
			AssertEquals("AUSYD", order.PortOfOrigin);
		}

		#endregion

		#region TestLoadID

		public void TestLoadID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "WL10000001", startTime: DateTimeOffset.Now);
			order1.WD_WLO_PlannedLoad = load1.PK;
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD02", data.Part1, 10m);

			var load2 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL20000001", "CDS", startTime: DateTimeOffset.Now);
			var load3 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL20000002", "CDS", startTime: DateTimeOffset.Now);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD03", data.Part1, 10m);
			order3.WD_DocketStatus = "ENT";
			order3.WD_TotalWeight = 2m;
			order3.WD_TotalWeightUnit = "G";
			order3.WD_TotalCubic = 1.5m;
			order3.WD_TotalCubicUnit = "WW";
			var pick3 = Helper.CreatePickNew(order3);
			var packageJob3 = order3.PackageJob;
			var package1 = packageJob3.Packages.AddNew("CTN", 1);
			var package2 = packageJob3.Packages.AddNew("BOX", 1);
			var pivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load2);
			var pivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load3);

			Factory.Save();
			AssertEquals("WL10000001", order1.LoadID);
			AssertEquals("", order2.LoadID);
			AssertContains("Order3 should contains 2 loads.", "Many", order3.LoadID);
		}

		#endregion

		#region TestTrolleyNumber

		public void TestTrolleyNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m); // assigned to T1
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m); // assigned to T2
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 5m); // assigned to T2
			var pick1 = Helper.CreatePickNew(order1, order2);
			var pick2 = Helper.CreatePickNew(order3);

			var trolley1 = Helper.CreateTrolley("T1");
			var trolley2 = Helper.CreateTrolley("T2");
			var trolley3 = Helper.CreateTrolley("T3");
			var trolley4 = Helper.CreateTrolley("T4");

			var package1 = order1.PackageJob.Packages.AddNew();
			var package2 = order1.PackageJob.Packages.AddNew();
			var package3 = order1.PackageJob.Packages.AddNew();
			var package4 = order2.PackageJob.Packages.AddNew();
			var package5 = order2.PackageJob.Packages.AddNew();
			var package6 = order3.PackageJob.Packages.AddNew();
			var package7 = order1.PackageJob.Packages.AddNew();

			//load trolley slots
			var trolleyJob1 = Helper.CreateWhsPickTrolleyJob(trolley1.PK, "PIC");

			Helper.CreateWhsPickTrolleySlot(trolleyJob1.PK, package1.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob1.PK, package4.PK, 2);

			var trolleyJob2 = Helper.CreateWhsPickTrolleyJob(trolley2.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob2.PK, package2.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob2.PK, package5.PK, 2);

			var trolleyJob3 = Helper.CreateWhsPickTrolleyJob(trolley3.PK, "FIN");
			Helper.CreateWhsPickTrolleySlot(trolleyJob3.PK, package3.PK, 1);
			Helper.CreateWhsPickTrolleySlot(trolleyJob3.PK, package6.PK, 2);

			var trolleyJob4 = Helper.CreateWhsPickTrolleyJob(trolley4.PK, "PIC");
			Helper.CreateWhsPickTrolleySlot(trolleyJob4.PK, package7.PK, 1);
			Factory.Save();

			var expectedOrder1 = new[] { "T2", "T1", "T4" };
			var expectedOrder2 = new[] { "T2", "T1" };
			var expectedOrder3 = new[] { ZString.Empty };

			var order1Array = order1.TrolleyNumber.Split(", ");
			var order2Array = order2.TrolleyNumber.Split(", ");
			var order3Array = order3.TrolleyNumber.Split(", ");

			AssertContainsExactElementsInAnyOrder(expectedOrder1, order1Array);
			AssertContainsExactElementsInAnyOrder(expectedOrder2, order2Array);
			AssertContainsExactElementsInAnyOrder(expectedOrder3, order3Array);
		}

		#endregion

		#region TestPortOfDestination

		public void TestPortOfDestination()
		{
			var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.Header.OH_RL_NKClosestPort = "AUSYD";
			consigneeAddress.OA_RL_NKRelatedPortCode = "NZAKL";

			var order = Factory.New<WhsOrder>();
			AssertEquals("Precondition:", "", order.PortOfDestination);

			order.ConsigneeAddressPK = consigneeAddress.PK;
			AssertEquals("NZAKL", order.PortOfDestination);

			consigneeAddress.OA_RL_NKRelatedPortCode = "";
			AssertEquals("AUSYD", order.PortOfDestination);

			consigneeAddress.Header.OH_RL_NKClosestPort = "";
			order.ConsigneeAddressPK = ZGuid.Empty;
			order.ConsigneeDocAddress.E2_RN_NKCountryCode = "NZ";
			order.ConsigneeDocAddress.E2_City = "Wellingto"; // Spelt incorrectly
			AssertEquals("", order.PortOfDestination);

			order.ConsigneeDocAddress.E2_City = "Wellington";
			AssertEquals("NZWLG", order.PortOfDestination);
		}

		#endregion

		#region TestWD_CalcCrossDockLocationVolumesIncludingCrossDockAllocations

		public void TestWD_CalcCrossDockLocationVolumesIncludingCrossDockAllocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			var location = data.Whs1.FindLocation(WhsWarehouse.DefaultDockDoorRowName);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 2);
			AssertEquals("Should return zero if no cross dock location", 0m,
				order.WD_CalcCrossDockAvailableVolumeIncludingCrossDockAllocations);
			AssertEquals("Should return zero if no cross dock location", 0m,
				order.WD_CalcCrossDockCurrentVolumeIncludingCrossDockAllocations);

			order.WD_WL_CrossDock = location.PK;
			location.WLV_MaxCubic = 2.0f;
			location.WLV_MaxCubicUnit = Constants.Volume.CubicMetres;
			data.Part1.OP_Cubic = 1.2f;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "2");
			var inv = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2);
			inv.WI_WL = ZGuid.Empty; // crossdocked stock usually won't have a location
			receive.RunPreSaveValidation(); // create inventory docket lines
			var reservedPickLine = Helper.CreateReservePickLine(line1, inv, 2m);

			// create some random unrelated inventory / orders / links to assert it is not included
			var randomReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "3");
			var randomInv = Helper.CreateWhsReceiveInventoryLine(randomReceive, data.Part1, 2);
			var randomOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "3");
			var randomOrderLine = Helper.CreateWhsOrderLine(randomOrder, data.Part1, 2);
			randomReceive.RunPreSaveValidation(); // create inventory docket lines
			Helper.CreateReservePickLine(randomOrderLine, randomInv, 2);
			randomOrder.WD_WL_CrossDock = data.Whs1.FindLocation("A-1").PK;

			Factory.Save();

			AssertEquals(-0.4m, order.WD_CalcCrossDockAvailableVolumeIncludingCrossDockAllocations);
			AssertEquals(2.4m, order.WD_CalcCrossDockCurrentVolumeIncludingCrossDockAllocations);

			reservedPickLine.ReservedQuantity = 1m;
			order.Validation.ValidateWD_WL_CrossDock();
			AssertEquals(0.8m, order.WD_CalcCrossDockAvailableVolumeIncludingCrossDockAllocations);
			AssertEquals(1.2m, order.WD_CalcCrossDockCurrentVolumeIncludingCrossDockAllocations);

			// make sure an inventories volume is not counted twice if it is located in the cross dock location
			// and there is also a crossdock link. ie. don't count the inventory + link quantity - it is the same stock
			location.WLV_MaxCubic = 3.0f;
			inv.WI_WL = location.PK;

			Factory.Save();
			AssertEquals(0.6m, order.WD_CalcCrossDockAvailableVolumeIncludingCrossDockAllocations);
			AssertEquals(2.4m, order.WD_CalcCrossDockCurrentVolumeIncludingCrossDockAllocations);
		}

		#endregion

		#region TestVolumeAndWeightUnit

		public void TestVolumeAndWeightUnit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part1 = Helper.CreateProduct(data.Org1, "PRODUCT1");
			part1.PartUnits[0].OF_ParentPackType = "CTN";
			part1.OP_Cubic = 1.0m;
			part1.OP_CubicUQ = Core.Constants.Volume.CubicMetres;
			part1.OP_Weight = 1.0m;
			part1.OP_WeightUQ = Core.Constants.Weight.Kilograms;

			var part2 = Helper.CreateProduct(data.Org1, "PRODUCT2");
			part2.PartUnits[0].OF_ParentPackType = "CTN";
			part2.OP_Cubic = 1.0m;
			part2.OP_CubicUQ = Core.Constants.Volume.CubicDecimetres;
			part2.OP_Weight = 0.001m;
			part2.OP_WeightUQ = Core.Constants.Weight.Tonnes;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", part1, 2m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", part2, 1m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, part1, 10m);
			var line1 = order.Lines[0];
			var line2 = Helper.CreateWhsOrderLine(order, part2, 10m);
			order.WD_TotalWeightUnit = Core.Constants.Weight.Grams;
			order.WD_TotalCubicUnit = Core.Constants.Volume.Litre;

			Helper.CreatePickNew(order);
			AssertEquals("Total weight should have changed:", 3000m, order.WD_WeightSentUserEntered);
			AssertEquals("Total volume should have changed:", 2001.0m, order.WD_CubicSent);

			line1.ReleaseLines[0].Quantity += 1m;
			AssertEquals("Total weight should have changed:", 4000m, order.WD_WeightSentUserEntered);
			AssertEquals("Total volume should have changed:", 3001.0m, order.WD_CubicSent);

			line2.ReleaseLines[0].Quantity += 1m;
			AssertEquals("Total weight should have changed:", 5000m, order.WD_WeightSentUserEntered);
			AssertEquals("Total volume should have changed:", 3002.0m, order.WD_CubicSent);
		}

		public void TestVolumeAndWeightUnit_RecalculatesWeightSentAndVolumeSentWhenChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0.5m, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("CTN");
			package.KP_Weight = 3m;
			package.KP_Volume = 8m;
			AssertEquals(nameof(WhsOrder.GrossWeightSent), 23m, order.GrossWeightSent);
			AssertEquals(nameof(WhsOrder.WD_WeightSentUserEntered), 20m, order.WD_WeightSentUserEntered);
			AssertEquals(nameof(WhsOrder.WD_WeightSent), 23m, order.WD_WeightSent);
			AssertEquals(nameof(WhsOrder.GrossWeightSent), 8m, order.WD_CubicSent);

			order.WD_TotalWeightUnit = "G";
			AssertEquals(nameof(WhsOrder.GrossWeightSent), 23000m, order.GrossWeightSent);
			AssertEquals(nameof(WhsOrder.WD_WeightSentUserEntered), 20000m, order.WD_WeightSentUserEntered);
			AssertEquals(nameof(WhsOrder.WD_WeightSent), 23000m, order.WD_WeightSent);
			AssertEquals(nameof(WhsOrder.GrossWeightSent), 8m, order.WD_CubicSent);

			order.WD_TotalCubicUnit = "L";
			AssertEquals(nameof(WhsOrder.GrossWeightSent), 23000m, order.GrossWeightSent);
			AssertEquals(nameof(WhsOrder.WD_WeightSentUserEntered), 20000m, order.WD_WeightSentUserEntered);
			AssertEquals(nameof(WhsOrder.WD_WeightSent), 23000m, order.WD_WeightSent);
			AssertEquals(nameof(WhsOrder.GrossWeightSent), 8000m, order.WD_CubicSent);

			order.UsePackingWeightAndVolume = false;
			AssertEquals(nameof(WhsOrder.GrossWeightSent), 23000m, order.GrossWeightSent);
			AssertEquals(nameof(WhsOrder.WD_WeightSentUserEntered), 20000m, order.WD_WeightSentUserEntered);
			AssertEquals(nameof(WhsOrder.WD_WeightSent), 20000m, order.WD_WeightSent);
			AssertEquals(nameof(WhsOrder.GrossWeightSent), 5000m, order.WD_CubicSent);

			order.WD_TotalWeightUnit = "KG";
			AssertEquals(nameof(WhsOrder.GrossWeightSent), 23m, order.GrossWeightSent);
			AssertEquals(nameof(WhsOrder.WD_WeightSentUserEntered), 20m, order.WD_WeightSentUserEntered);
			AssertEquals(nameof(WhsOrder.WD_WeightSent), 20m, order.WD_WeightSent);
			AssertEquals(nameof(WhsOrder.GrossWeightSent), 5000m, order.WD_CubicSent);

			order.WD_TotalCubicUnit = "M3";
			AssertEquals(nameof(WhsOrder.GrossWeightSent), 23m, order.GrossWeightSent);
			AssertEquals(nameof(WhsOrder.WD_WeightSentUserEntered), 20m, order.WD_WeightSentUserEntered);
			AssertEquals(nameof(WhsOrder.WD_WeightSent), 20m, order.WD_WeightSent);
			AssertEquals(nameof(WhsOrder.GrossWeightSent), 5m, order.WD_CubicSent);
		}

		public void TestVolumeUnit_RecalculatesVolumeSentWhenChanged_NetVolumeAndNoPackagesUseCases()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0.5m, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("CTN");
			package.KP_Weight = 3m;
			package.KP_Volume = 4m;
			AssertEquals("Should use Net Volume as Packing Volume is less.", 5m, order.WD_CubicSent);

			order.WD_TotalCubicUnit = "L";
			AssertEquals("Should use Net Volume as Packing Volume is less.", 5000m, order.WD_CubicSent);

			package.Delete();
			order.WD_AddPalletWeightToOrder = true;
			order.WD_TotalCubicUnit = "M3";
			AssertEquals("Should use Net Volume.", 5m, order.WD_CubicSent);
		}

		#endregion

		#region TestWD_OH_Client

		#region TestWD_OH_Client_INCOTerm

		public void TestWD_OH_Client_INCOTerm()
		{
			var order = Factory.New<WhsOrder>();
			var client1 = Factory.NewWithValidTestData<OrgHeader>();
			client1.MiscServ.OM_IMDefaultINCOTerm = order.Lookups.INCOTerms[0].Code;

			var client2 = Factory.NewWithValidTestData<OrgHeader>();
			client2.MiscServ.OM_IMDefaultINCOTerm = "XXX";

			var client3 = Factory.New<OrgHeader>();
			client3.MiscServ.OM_IMDefaultINCOTerm = "MMM";

			order.WD_OH_Client = client1.PK;
			AssertEquals(order.Lookups.INCOTerms[0].Code, order.WD_INCO);

			order.WD_OH_Client = client2.PK;
			AssertEquals(order.Lookups.INCOTerms[0].Code, order.WD_INCO);

			order.ConsigneeDocAddress.OrganisationPK = client3.PK;
			AssertEquals(order.Lookups.INCOTerms[0].Code, order.WD_INCO);
			client3.MiscServ.OM_IMDefaultINCOTerm = order.Lookups.INCOTerms[1].Code;
			order.ConsigneeDocAddress.OrganisationPK = client2.PK;
			AssertEquals(order.Lookups.INCOTerms[0].Code, order.WD_INCO);
			order.ConsigneeDocAddress.OrganisationPK = client3.PK;
			AssertEquals("Should not change INCO term if not empty and valid.", order.Lookups.INCOTerms[0].Code,
				order.WD_INCO);
		}

		#endregion

		#region TestWD_OH_Client_PickPriority

		public void TestWD_OH_Client_PickPriority()
		{
			var order = Factory.New<WhsOrder>();
			var client1 = Helper.CreateClient("client1");
			client1.MiscServ.OM_WhsOrderDefaultPickPriority = 0;

			var client2 = Helper.CreateClient("client2");
			client2.MiscServ.OM_WhsOrderDefaultPickPriority = 1;

			var invalidPK = new ZGuid();

			AssertEquals("Precondition: Pick Priority is preset to 0.", (ZByte)0, order.WD_PickPriority);

			order.WD_OH_Client = client1.PK;
			AssertEquals("Pick Priority should match the Organisations default Pick Priority.", (ZByte)0,
				order.WD_PickPriority);
			order.WD_OH_Client = invalidPK;
			AssertEquals("Selecting an invalid Warehouse Client should leave the pick priority unchanged.", (ZByte)0,
				order.WD_PickPriority);

			order.WD_OH_Client = client2.PK;
			AssertEquals("Pick Priority should match the Organisations default Pick Priority.", (ZByte)1,
				order.WD_PickPriority);
			order.WD_OH_Client = invalidPK;
			AssertEquals("Selecting an invalid Warehouse Client should leave the pick priority unchanged.", (ZByte)1,
				order.WD_PickPriority);
		}

		#endregion

		#region TestWD_OH_Client_PickPriority_ClientNotChanged

		public void TestWD_OH_Client_PickPriority_ClientNotChanged()
		{
			var order = Factory.New<WhsOrder>();

			var client1 = Helper.CreateClient("client1");
			client1.MiscServ.OM_WhsOrderDefaultPickPriority = 0;

			order.WD_OH_Client = client1.PK;

			AssertEquals("Precondition: Order pick priority should be '0'.", (ZByte)0,
				client1.MiscServ.OM_WhsOrderDefaultPickPriority);

			client1.MiscServ.OM_WhsOrderDefaultPickPriority = 1;
			order.WD_OH_Client = client1.PK;

			AssertEquals("The client should not have changed.", client1.PK, order.WD_OH_Client);
			AssertEquals("The client default pick priority should have updated.", (ZByte)1,
				client1.MiscServ.OM_WhsOrderDefaultPickPriority);
			AssertEquals("The order pick priority should not change after the default client pick priority changes.",
				(ZByte)0, order.WD_PickPriority);
		}

		#endregion

		#region TestWD_OH_Client_FulfillmentRule

		public void TestWD_OH_Client_FulfillmentRule()
		{
			var order = Factory.New<WhsOrder>();
			var client1 = Helper.CreateClient("client1");
			client1.MiscServ.OM_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;

			var client2 = Helper.CreateClient("client2");
			client2.MiscServ.OM_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.None;

			var invalidPK = new ZGuid();

			AssertEquals("Precondition: Fulfillment Rule field is empty.", "", order.WD_WhsOrderFulfillmentRule);

			order.WD_OH_Client = client1.PK;
			AssertEquals("Fulfillment Rule should match the Organisations default fulfillment rule.",
				WhsOrderFulfillmentRuleList.Codes.All, order.WD_WhsOrderFulfillmentRule);
			order.WD_OH_Client = invalidPK;
			AssertEquals("Selecting an invalid Warehouse Client should leave the fulfillment rule unchanged.",
				WhsOrderFulfillmentRuleList.Codes.All, order.WD_WhsOrderFulfillmentRule);

			order.WD_OH_Client = client2.PK;
			AssertEquals("Fulfillment Rule should match the Organisations default fulfillment rule.",
				WhsOrderFulfillmentRuleList.Codes.None, order.WD_WhsOrderFulfillmentRule);
			order.WD_OH_Client = invalidPK;
			AssertEquals("Selecting an invalid Warehouse Client should leave the fulfillment rule unchanged.",
				WhsOrderFulfillmentRuleList.Codes.None, order.WD_WhsOrderFulfillmentRule);
		}

		#endregion

		#region TestWD_OH_Client_FulfillmentRule_ClientNotChanged

		public void TestWD_OH_Client_FulfillmentRule_ClientNotChanged()
		{
			var order = Factory.New<WhsOrder>();

			var client1 = Helper.CreateClient("client1");
			client1.MiscServ.OM_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.None;

			order.WD_OH_Client = client1.PK;

			AssertEquals("Precondition: Order fulfillment rule should be 'None'.",
				WhsOrderFulfillmentRuleList.Codes.None, client1.MiscServ.OM_WhsOrderFulfillmentRule);

			client1.MiscServ.OM_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			order.WD_OH_Client = client1.PK;

			AssertEquals("The client should not have changed.", client1.PK, order.WD_OH_Client);
			AssertEquals("The client default fulfillment rule should have updated.",
				WhsOrderFulfillmentRuleList.Codes.All, client1.MiscServ.OM_WhsOrderFulfillmentRule);
			AssertEquals(
				"The order fulfillment rule should not change after the client default fulfillment rule changes.",
				WhsOrderFulfillmentRuleList.Codes.None, order.WD_WhsOrderFulfillmentRule);
		}

		#endregion

		#endregion

		#region TestHandlingInstructionsPropertyAndNotes

		public void TestHandlingInstructionsPropertyAndNotes()
		{
			var order = GetNewBusinessObject();
			order.WD_OH_Client = Helper.CreateClient().PK;
			order.TransportCoDocAddress.OrganisationPK = Helper.CreateClient().PK;
			order.ConsigneeDocAddress.OrganisationPK = Helper.CreateClient().PK;

			var orderSINotes =
				order.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
			AssertEquals("Order should have no Handling Instructions notes", 0, orderSINotes.Length);
			AssertEquals("Handling Instructions property should be empty", ZString.Empty,
				order.WD_HandlingInstructions);
		}

		public void TestHandlingInstructionsPropertyAndNotes_HandlingInstructions()
		{
			var order = GetNewBusinessObject();
			order.WD_OH_Client = Helper.CreateClient().PK;
			order.TransportCoDocAddress.OrganisationPK = Helper.CreateClient().PK;
			order.ConsigneeDocAddress.OrganisationPK = Helper.CreateClient().PK;
			order.WD_HandlingInstructions = "Test";

			var orderSINotes = order.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
			AssertEquals("Order should have one Handling Instructions note", 1, orderSINotes.Length);
			AssertEquals("Order should have one Handling Instructions note 'Test'", "Test",
				orderSINotes[0].ST_NoteText.ToString());
			AssertEquals("Handling Instructions property text must be the same as Handling Instructions note",
				orderSINotes[0].ST_NoteText.ToString(), order.WD_HandlingInstructions);

			order.WD_HandlingInstructions = "Test2";

			orderSINotes = order.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
			AssertEquals("Order should have only one Handling Instructions note", 1, orderSINotes.Length);
			AssertEquals("Order should have only one Handling Instructions note 'Test2'", "Test2",
				orderSINotes[0].ST_NoteText.ToString());
			AssertEquals("Handling Instructions property text must be the same as Handling Instructions note",
				orderSINotes[0].ST_NoteText.ToString(), order.WD_HandlingInstructions);
		}

		public void TestHandlingInstructionsPropertyAndNotes_AddNewNotes()
		{
			var order = GetNewBusinessObject();
			order.WD_OH_Client = Helper.CreateClient().PK;
			order.TransportCoDocAddress.OrganisationPK = Helper.CreateClient().PK;
			order.ConsigneeDocAddress.OrganisationPK = Helper.CreateClient().PK;

			order.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "OrderSI");
			var orderSINotes = order.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
			AssertEquals("Order should have one Handling Instructions note", 1, orderSINotes.Length);
			AssertContains("WD_HandlingInstructions should contain WhsOrder Handling Instructions", "OrderSI",
				order.WD_HandlingInstructions);

			order.Client.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "ClientSI");
			orderSINotes = order.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
			AssertEquals("Order should have only one Handling Instructions note", 1, orderSINotes.Length);
			AssertContains("WD_HandlingInstructions should contain WhsOrder Handling Instructions", "OrderSI",
				order.WD_HandlingInstructions);
			AssertContains("WD_HandlingInstructions should contain Client Handling Instructions", "ClientSI",
				order.WD_HandlingInstructions);

			order.ConsigneeDocAddress.Organisation.Notes.AddNew(false,
				PredefinedNoteTypes.Instance.HandlingInstructions.Description, "ConsigneeSI");
			orderSINotes = order.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
			AssertEquals("Order should have only one Handling Instructions note", 1, orderSINotes.Length);
			AssertContains("WD_HandlingInstructions should contain WhsOrder Handling Instructions", "OrderSI",
				order.WD_HandlingInstructions);
			AssertContains("WD_HandlingInstructions should contain Client Handling Instructions", "ClientSI",
				order.WD_HandlingInstructions);
			AssertContains("WD_HandlingInstructions should contain Consignee Handling Instructions", "ConsigneeSI",
				order.WD_HandlingInstructions);

			order.TransportCoDocAddress.Organisation.Notes.AddNew(false,
				PredefinedNoteTypes.Instance.HandlingInstructions.Description, "TransportCoSI");
			orderSINotes = order.Notes.FindByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description);
			AssertEquals("Order should have only one Handling Instructions note", 1, orderSINotes.Length);
			AssertContains("WD_HandlingInstructions should contain WhsOrder Handling Instructions", "OrderSI",
				order.WD_HandlingInstructions);
			AssertContains("WD_HandlingInstructions should contain Client Handling Instructions", "ClientSI",
				order.WD_HandlingInstructions);
			AssertContains("WD_HandlingInstructions should contain Consignee Handling Instructions", "ConsigneeSI",
				order.WD_HandlingInstructions);
			AssertContains("WD_HandlingInstructions should contain TransportCo Handling Instructions", "TransportCoSI",
				order.WD_HandlingInstructions);
		}

		#endregion

		#region TestSubTypeDesc

		public override void TestSubTypeDesc()
		{
			var docket = GetNewBusinessObject();
			docket.WD_DocketSubType = OrderType.Codes.BackOrder;
			AssertEquals(OrderType.Descriptions.BackOrder, docket.SubTypeDesc);
		}

		#endregion

		#region TestConsigneeDocAddress

		protected override void TestConsigneeDocAddressCore()
		{
			base.TestConsigneeDocAddressCore();

			var order = Factory.New<WhsOrder>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;

			order.ConsigneePK = orgHeader.PK;
			AssertEquals("Consignee address should be main address", orgHeader.MainAddress.PK,
				order.ConsigneeAddressPK);

			order.ConsigneePK = ZGuid.Empty;
			var pickupAddress = orgHeader.Addresses.AddNew(OrgAddressType.Pickup, true);
			order.ConsigneePK = orgHeader.PK;
			AssertEquals("Consignee address should be main address", orgHeader.MainAddress.PK,
				order.ConsigneeAddressPK);

			order.ConsigneePK = ZGuid.Empty;
			var pickupAndDeliveryAddress = orgHeader.Addresses.AddNew(OrgAddressType.PickupAndDelivery, true);
			order.ConsigneePK = orgHeader.PK;
			AssertEquals("Consignee address should be PickupAndDelivery address", pickupAndDeliveryAddress.PK,
				order.ConsigneeAddressPK);

			var client1 = Factory.NewWithValidTestData<OrgHeader>();
			client1.MiscServ.OM_IMDefaultINCOTerm = order.Lookups.INCOTerms[0].Code;
			var client2 = Factory.NewWithValidTestData<OrgHeader>();
			client2.MiscServ.OM_IMDefaultINCOTerm = "XXX";

			order.WD_OH_Client = client1.PK;
			AssertEquals(order.Lookups.INCOTerms[0].Code, order.WD_INCO);
			order.WD_OH_Client = client2.PK;
			AssertEquals(order.Lookups.INCOTerms[0].Code, order.WD_INCO);

			var client3 = Factory.NewWithValidTestData<OrgHeader>();
			client3.MiscServ.OM_IMDefaultINCOTerm = "MMM";
			order.ConsigneeDocAddress.OrganisationPK = client3.PK;
			AssertEquals(order.Lookups.INCOTerms[0].Code, order.WD_INCO);
			client3.MiscServ.OM_IMDefaultINCOTerm = order.Lookups.INCOTerms[1].Code;
			order.ConsigneeDocAddress.OrganisationPK = client2.PK;
			AssertEquals(order.Lookups.INCOTerms[0].Code, order.WD_INCO);
			order.ConsigneeDocAddress.OrganisationPK = client3.PK;
			AssertEquals("Should not change INCO term if not empty and valid.", order.Lookups.INCOTerms[0].Code,
				order.WD_INCO);
		}

		#endregion

		#region TestChangeTransportCoPK

		public void TestChangeTransportCoPK_NoErrorForNotChangeTransportCo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();
			var transportCo1 = Helper.CreateClient("X1", "X1");
			var transportCo2 = Helper.CreateClient("Y1", "Y1");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.TransportCoPK = transportCo1.PK;
			AssertEquals("Precondition - has NO package on a HU.", false, order.IsAnyPackageInConsolidationLocationOrOnConsolidationHU());
			Assert("Precondition - has NO errors.", !order.TransportCoDocAddress.OrganisationPKInfo.HasErrors());

			Helper.CreatePickNew(order);
			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			package.KP_GS_NKClosedBy = "LTS";

			var handlingUnit = Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = PackingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);
			handlingUnitPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage.KP_GS_NKClosedBy = "LTS";

			PackingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);
			orderInOtherFactory.RunPreSaveValidation();
			AssertNoErrors("No Error for TransportCo when don't change TransportCo", orderInOtherFactory.TransportCoDocAddress.OrganisationPKInfo);
		}

		public void TestChangeTransportCoPK_WhenOrderIsPickingAndAnyOrderPackagesIsAtCONLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			var transportCo1 = Helper.CreateClient("X1", "X1");
			var transportCo2 = Helper.CreateClient("Y1", "Y1");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 25m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 25m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.TransportCoPK = transportCo1.PK;
			AssertNotEquals("Precondition - order is NOT Picking.", DocketStatus.Codes.Picking, order.WarehouseOrderStatus);
			AssertEquals("Precondition - has NO package in consolidation location.", false, order.IsAnyPackageInConsolidationLocationOrOnConsolidationHU());
			Assert("Precondition - has NO errors.", !order.TransportCoDocAddress.OrganisationPKInfo.HasErrors());

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("2 Pick Lines", 2, orderLine.PickLines.Count);
			var pickLine1 = orderLine.PickLines[0];

			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(orderLine.ReleaseLines[0], 10m);
			Factory.Save();

			AssertEquals("order is Picking.", DocketStatus.Codes.Picking, order.WarehouseOrderStatus);
			AssertEquals("has package in a consolidation location.", true, order.IsAnyPackageInConsolidationLocationOrOnConsolidationHU());
			order.TransportCoPK = transportCo2.PK;
			Assert(order.TransportCoDocAddress.OrganisationPKInfo.HasErrors());
			AssertHasError(order.TransportCoDocAddress.OrganisationPKInfo, "Cannot change the Transport Company when any packages are in a Packing Consolidation Location or on a Handling Unit.");
		}

		public void TestChangeTransportCoPK_WhenAnyOrderPackagesIsOnHU()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();
			var transportCo1 = Helper.CreateClient("X1", "X1");
			var transportCo2 = Helper.CreateClient("Y1", "Y1");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.TransportCoPK = transportCo1.PK;
			AssertEquals("Precondition - has NO package on a HU.", false, order.IsAnyPackageInConsolidationLocationOrOnConsolidationHU());
			Assert("Precondition - has NO errors.", !order.TransportCoDocAddress.OrganisationPKInfo.HasErrors());

			Helper.CreatePickNew(order);
			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			package.KP_GS_NKClosedBy = "LTS";

			var handlingUnit = Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = PackingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);
			handlingUnitPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage.KP_GS_NKClosedBy = "LTS";

			PackingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);
			Factory.Save();

			AssertEquals("has package on a HU.", true, order.IsAnyPackageInConsolidationLocationOrOnConsolidationHU());
			order.TransportCoPK = transportCo2.PK;
			Assert(order.TransportCoDocAddress.OrganisationPKInfo.HasErrors());
			AssertHasError(order.TransportCoDocAddress.OrganisationPKInfo, "Cannot change the Transport Company when any packages are in a Packing Consolidation Location or on a Handling Unit.");
		}

		public void TestChangeTransportCoPK_WhenOrderIsPickingAndAnyOrderPackagesIsAtCONLocation_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			var transportCo1 = Helper.CreateClient("X1", "X1");
			var transportCo2 = Helper.CreateClient("Y1", "Y1");
			var transportCo3 = Helper.CreateClient("Z1", "Z1");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 25m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 25m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.TransportCoPK = transportCo1.PK;
			AssertNotEquals("Precondition - order is NOT Picking.", DocketStatus.Codes.Picking, order.WarehouseOrderStatus);
			AssertEquals("Precondition - has NO package in consolidation location.", false, order.IsAnyPackageInConsolidationLocationOrOnConsolidationHU());
			Assert("Precondition - has NO errors.", !order.TransportCoDocAddress.OrganisationPKInfo.HasErrors());

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("2 Pick Lines", 2, orderLine.PickLines.Count);
			var pickLine1 = orderLine.PickLines[0];

			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(orderLine.ReleaseLines[0], 10m);
			Factory.Save();

			AssertEquals("order is Picking.", DocketStatus.Codes.Picking, order.WarehouseOrderStatus);
			AssertEquals("has package in a consolidation location.", true, order.IsAnyPackageInConsolidationLocationOrOnConsolidationHU());
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 7 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 3 },
				{ OrgContactSchema.Constants.TableName, 3 },
				{ OrgHeaderSchema.Constants.TableName, 3 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ RateTransportProviderSchema.Constants.TableName, 2 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsLoadOrderSchema.Constants.TableName, 1 },
				{ WhsOrderStatusViewSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsPackageLocationViewSchema.Constants.TableName, 1 }, // should hit WhsPackageLocationView once
			};
			var newFactory = new BusinessObjectFactory();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				var orderInNewDb = newFactory.Load<WhsOrder>(order.PK);
				for (int i = 0; i < 20; i++)
				{
					if (i % 2 == 0)
					{
						orderInNewDb.TransportCoPK = transportCo2.PK;
					}
					else
					{
						orderInNewDb.TransportCoPK = transportCo3.PK;
					}
				}
			}
		}

		public void TestChangeTransportCoPK_WhenAnyOrderPackagesIsOnHU_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();
			var transportCo1 = Helper.CreateClient("X1", "X1");
			var transportCo2 = Helper.CreateClient("Y1", "Y1");
			var transportCo3 = Helper.CreateClient("Z1", "Z1");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.TransportCoPK = transportCo1.PK;
			AssertEquals("Precondition - has NO package on a HU.", false, order.IsAnyPackageInConsolidationLocationOrOnConsolidationHU());
			Assert("Precondition - has NO errors.", !order.TransportCoDocAddress.OrganisationPKInfo.HasErrors());

			Helper.CreatePickNew(order);
			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			package.KP_GS_NKClosedBy = "LTS";

			var handlingUnit = Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = PackingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);
			handlingUnitPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage.KP_GS_NKClosedBy = "LTS";

			PackingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);
			Factory.Save();

			AssertEquals("has package on a HU.", true, order.IsAnyPackageInConsolidationLocationOrOnConsolidationHU());
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 7 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 3 },
				{ OrgContactSchema.Constants.TableName, 3 },
				{ OrgHeaderSchema.Constants.TableName, 3 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ RateTransportProviderSchema.Constants.TableName, 2 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsLoadOrderSchema.Constants.TableName, 1 },
				{ WhsOrderStatusViewSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },// should NOT hit WhsPackageLocationView
			};
			var newFactory = new BusinessObjectFactory();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				var orderInNewDb = newFactory.Load<WhsOrder>(order.PK);
				for (int i = 0; i < 20; i++)
				{
					if (i % 2 == 0)
					{
						orderInNewDb.TransportCoPK = transportCo2.PK;
					}
					else
					{
						orderInNewDb.TransportCoPK = transportCo3.PK;
					}
				}
			}
		}

		#endregion

		#region TestChangeCarrierServiceLevel

		public void TestChangeCarrierServiceLevel_WhenOrderIsPickingAndAnyOrderPackagesIsAtCONLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			var transportCo1 = Helper.CreateClient("X1", "X1");
			var service1 = transportCo1.MiscServ.CarrierServiceLevels.AddNew();
			service1.PL_Code = "SL1";

			var transportCo2 = Helper.CreateClient("Y1", "Y1");
			var service2 = transportCo2.MiscServ.CarrierServiceLevels.AddNew();
			service2.PL_Code = "SL2";

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 25m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 25m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.TransportCoPK = transportCo1.PK;
			order.WD_PL_NKCarrierServiceLevel = "SL1";
			AssertNotEquals("Precondition - order is NOT Picking.", DocketStatus.Codes.Picking, order.WarehouseOrderStatus);
			AssertEquals("Precondition - has NO package in consolidation location.", false, order.IsAnyPackageInConsolidationLocationOrOnConsolidationHU());
			Assert("Precondition - has NO errors.", !order.WD_PL_NKCarrierServiceLevelInfo.HasErrors());

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("2 Pick Lines", 2, orderLine.PickLines.Count);
			var pickLine1 = orderLine.PickLines[0];

			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(orderLine.ReleaseLines[0], 10m);
			Factory.Save();

			AssertEquals("order is Picking.", DocketStatus.Codes.Picking, order.WarehouseOrderStatus);
			AssertEquals("has package in a consolidation location.", true, order.IsAnyPackageInConsolidationLocationOrOnConsolidationHU());
			order.WD_PL_NKCarrierServiceLevel = "SL2";
			Assert(order.WD_PL_NKCarrierServiceLevelInfo.HasErrors());
			AssertHasError(order.WD_PL_NKCarrierServiceLevelInfo, "Cannot change the Carrier Service Level when any packages are in a Packing Consolidation Location or on a Handling Unit.");
		}

		public void TestChangeCarrierServiceLevel_WhenAnyOrderPackagesIsOnHU()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();
			var transportCo1 = Helper.CreateClient("X1", "X1");
			var service1 = transportCo1.MiscServ.CarrierServiceLevels.AddNew();
			service1.PL_Code = "SL1";

			var transportCo2 = Helper.CreateClient("Y1", "Y1");
			var service2 = transportCo2.MiscServ.CarrierServiceLevels.AddNew();
			service2.PL_Code = "SL2";
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.TransportCoPK = transportCo1.PK;
			order.WD_PL_NKCarrierServiceLevel = "SL1";
			AssertEquals("Precondition - has NO package on a HU.", false, order.IsAnyPackageInConsolidationLocationOrOnConsolidationHU());
			Assert("Precondition - has NO errors.", !order.WD_PL_NKCarrierServiceLevelInfo.HasErrors());

			Helper.CreatePickNew(order);
			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			package.KP_GS_NKClosedBy = "LTS";

			var handlingUnit = Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = PackingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);
			handlingUnitPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage.KP_GS_NKClosedBy = "LTS";

			PackingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);
			Factory.Save();

			AssertEquals("has package on a HU.", true, order.IsAnyPackageInConsolidationLocationOrOnConsolidationHU());
			order.WD_PL_NKCarrierServiceLevel = "SL2";
			Assert(order.WD_PL_NKCarrierServiceLevelInfo.HasErrors());
			AssertHasError(order.WD_PL_NKCarrierServiceLevelInfo, "Cannot change the Carrier Service Level when any packages are in a Packing Consolidation Location or on a Handling Unit.");
		}

		public void TestChangeCarrierServiceLevel_WhenOrderIsPickingAndAnyOrderPackagesIsAtCONLocation_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			var transportCo1 = Helper.CreateClient("X1", "X1");
			var service1 = transportCo1.MiscServ.CarrierServiceLevels.AddNew();
			service1.PL_Code = "SL1";

			var transportCo2 = Helper.CreateClient("Y1", "Y1");
			var service2 = transportCo2.MiscServ.CarrierServiceLevels.AddNew();
			service2.PL_Code = "SL2";

			var transportCo3 = Helper.CreateClient("Z1", "Z1");
			var service3 = transportCo3.MiscServ.CarrierServiceLevels.AddNew();
			service3.PL_Code = "SL3";

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 25m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 25m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.TransportCoPK = transportCo1.PK;
			order.WD_PL_NKCarrierServiceLevel = "SL1";
			AssertNotEquals("Precondition - order is NOT Picking.", DocketStatus.Codes.Picking, order.WarehouseOrderStatus);
			AssertEquals("Precondition - has NO package in consolidation location.", false, order.IsAnyPackageInConsolidationLocationOrOnConsolidationHU());
			Assert("Precondition - has NO errors.", !order.WD_PL_NKCarrierServiceLevelInfo.HasErrors());

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("2 Pick Lines", 2, orderLine.PickLines.Count);
			var pickLine1 = orderLine.PickLines[0];

			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(orderLine.ReleaseLines[0], 10m);
			Factory.Save();

			AssertEquals("order is Picking.", DocketStatus.Codes.Picking, order.WarehouseOrderStatus);
			AssertEquals("has package in a consolidation location.", true, order.IsAnyPackageInConsolidationLocationOrOnConsolidationHU());
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgCarrierServiceLevelSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsLoadOrderSchema.Constants.TableName, 1 },
				{ WhsOrderStatusViewSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsPackageLocationViewSchema.Constants.TableName, 1 }, // should hit WhsPackageLocationView once
			};
			var newFactory = new BusinessObjectFactory();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				var orderInNewDb = newFactory.Load<WhsOrder>(order.PK);
				for (int i = 0; i < 20; i++)
				{
					if (i % 2 == 0)
					{
						orderInNewDb.WD_PL_NKCarrierServiceLevel = "SL2";
					}
					else
					{
						orderInNewDb.WD_PL_NKCarrierServiceLevel = "SL3";
					}
				}
			}
		}

		public void TestChangeCarrierServiceLevel_WhenAnyOrderPackagesIsOnHU_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();
			var transportCo1 = Helper.CreateClient("X1", "X1");
			var service1 = transportCo1.MiscServ.CarrierServiceLevels.AddNew();
			service1.PL_Code = "SL1";

			var transportCo2 = Helper.CreateClient("Y1", "Y1");
			var service2 = transportCo2.MiscServ.CarrierServiceLevels.AddNew();
			service2.PL_Code = "SL2";

			var transportCo3 = Helper.CreateClient("Z1", "Z1");
			var service3 = transportCo3.MiscServ.CarrierServiceLevels.AddNew();
			service3.PL_Code = "SL3";

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.TransportCoPK = transportCo1.PK;
			order.WD_PL_NKCarrierServiceLevel = "SL1";
			AssertEquals("Precondition - has NO package on a HU.", false, order.IsAnyPackageInConsolidationLocationOrOnConsolidationHU());
			Assert("Precondition - has NO errors.", !order.WD_PL_NKCarrierServiceLevelInfo.HasErrors());

			Helper.CreatePickNew(order);
			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG", 1, PkgUnit.Box);
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			package.KP_GS_NKClosedBy = "LTS";

			var handlingUnit = Factory.New<PkgHandlingUnit>();
			handlingUnit.KPU_GB_Branch = data.Whs1.WW_GB_RelatedCompanyBranch;
			handlingUnit.KPU_JobContext = "3PL";

			var handlingUnitPackageJob = PkgPackageJob.LoadOrCreatePackageJob(handlingUnit);
			var handlingUnitPackage = PackingHelper.CreatePackage(handlingUnitPackageJob, "HU", 1, PkgUnit.Package);
			handlingUnitPackage.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnitPackage.KP_GS_NKClosedBy = "LTS";

			PackingHelper.PackHandlingUnit(handlingUnitPackage, package, handlingUnitPackage);
			Factory.Save();

			AssertEquals("has package on a HU.", true, order.IsAnyPackageInConsolidationLocationOrOnConsolidationHU());
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgCarrierServiceLevelSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsLoadOrderSchema.Constants.TableName, 1 },
				{ WhsOrderStatusViewSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },// should NOT hit WhsPackageLocationView
			};
			var newFactory = new BusinessObjectFactory();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				var orderInNewDb = newFactory.Load<WhsOrder>(order.PK);
				for (int i = 0; i < 20; i++)
				{
					if (i % 2 == 0)
					{
						orderInNewDb.WD_PL_NKCarrierServiceLevel = "SL2";
					}
					else
					{
						orderInNewDb.WD_PL_NKCarrierServiceLevel = "SL3";
					}
				}
			}
		}

		#endregion

		#region TestTransportCoDocAddress

		protected override int TransportCoDocAddressReadOnlyPropHit => 2;

		#endregion

		#region TestTransportCoDocAddress_ReadOnly

		public void TestTransportCoDocAddress_ReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			AssertEquals("Precondition", false, order.WD_WLO_PlannedLoad.IsValid);
			AssertEquals(false, order.TransportCoDocAddress.ReadOnly);

			order.WD_WLO_PlannedLoad = ZGuid.NewZGuid();
			AssertEquals(true, order.TransportCoDocAddress.ReadOnly);
		}

		public void TestTransportCoDocAddress_ReadOnly_OrderLinkedToLoadThruPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			Factory.Save();

			AssertEquals("Precondition", false, order.WD_WLO_PlannedLoad.IsValid);
			AssertEquals("Precondition", false, order.TransportCoDocAddress.ReadOnly);

			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01", "CDS", startTime: DateTimeOffset.Now);
			Helper.CreatePickNew(order);
			var packageJob = order.PackageJob;
			var package = packageJob.Packages.AddNew("BOX", 1);
			Helper.CreateLoadPkgPackagePivot(package.PK, load);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			AssertEquals("Order has no planned load.", false, orderInNewFactory.WD_WLO_PlannedLoad.IsValid);
			AssertEquals(true, orderInNewFactory.TransportCoDocAddress.ReadOnly);
		}

		#endregion

		#region TestWarehouseReadOnly

		public void TestWarehouseReadOnly_WhenAttachedToLoad()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			Factory.Save();

			AssertEquals("Order has no planned load.", false, order.WD_WLO_PlannedLoad.IsValid);
			AssertEquals(false, order.WD_WW_WhsInfo.ReadOnly);

			var pick = Helper.CreatePickNew(order);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation);
			order.WD_WLO_PlannedLoad = load.PK;

			load.WLO_StartTime = ZDateTimeOffset.Today;
			load.WLO_TransportationUnitNumber = "4543";
			load.WLO_GateOutTime = ZDateTimeOffset.Today;
			load.WLO_CompleteTime = ZDateTimeOffset.Today;
			Factory.Save();

			AssertEquals("Order has a planned load.", true, order.WD_WLO_PlannedLoad.IsValid);
			AssertEquals(true, order.WD_WW_WhsInfo.ReadOnly);
		}

		#endregion

		#region TestPickUpDocAddress

		public override void TestPickUpDocAddress()
		{
			var docket = GetNewBusinessObject();
			base.TestPickUpDocAddress();
			docket.PickUpDocAddress.E2_AddressOverride = false;

			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress; //Factory.New<OrgAddress>();

			docket.PickUpPK = org.PK;
			AssertEquals("PickUp address should be main address", org.MainAddress.PK, docket.PickUpAddressPK);

			docket.PickUpPK = ZGuid.Empty;
			var deliveryAddress = org.Addresses.AddNew(OrgAddressType.Delivery, true);
			docket.PickUpPK = org.PK;
			AssertEquals("PickUp address should be main address", org.MainAddress.PK, docket.PickUpAddressPK);

			docket.PickUpPK = ZGuid.Empty;
			var pickupAndDeliveryAddress = org.Addresses.AddNew(OrgAddressType.PickupAndDelivery, true);
			docket.PickUpPK = org.PK;
			AssertEquals("PickUp address should be delivery address", pickupAndDeliveryAddress.PK,
				docket.PickUpAddressPK);
		}

		#endregion

		#region TestDropOffDocAddress

		public override void TestDropOffDocAddress()
		{
			var docket = GetNewBusinessObject();
			base.TestDropOffDocAddress();
			docket.DropOffDocAddress.E2_AddressOverride = false;

			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress; //Factory.New<OrgAddress>();

			docket.DropOffPK = org.PK;
			AssertEquals("DropOff address should be main address", org.MainAddress.PK, docket.DropOffAddressPK);

			docket.DropOffPK = ZGuid.Empty;
			var pickupAddress = org.Addresses.AddNew(OrgAddressType.Pickup, true);
			docket.DropOffPK = org.PK;
			AssertEquals("DropOff address should be main address", org.MainAddress.PK, docket.DropOffAddressPK);

			docket.DropOffPK = ZGuid.Empty;
			var pickupAndDeliveryAddress = org.Addresses.AddNew(OrgAddressType.PickupAndDelivery, true);
			docket.DropOffPK = org.PK;
			AssertEquals("DropOff address should be delivery address", pickupAndDeliveryAddress.PK,
				docket.DropOffAddressPK);
		}

		#endregion

		#region TestWD_WW_Whs

		public override void TestWD_WW_Whs()
		{
			base.TestWD_WW_Whs();

			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.MiscServ.OM_IMDefaultINCOTerm = "FCD";
			client.MainAddress.OA_RL_NKRelatedPortCode = "USORD";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.MiscServ.OM_IMDefaultINCOTerm = "FOB";
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var whs = Helper.CreateWarehouse("1");
			whs.WW_OA_WarehouseAddress = consignee.MainAddress.PK;

			var order = Factory.New<WhsOrder>();
			order.WD_OH_Client = client.PK;
			AssertEquals("FCD", order.WD_INCO);

			order.WD_WW_Whs = whs.PK;
			AssertEquals(ZString.Empty, order.WD_INCO);

			order.ConsigneePK = consignee.PK;
			AssertEquals("FCD", order.WD_INCO);

			order.WD_WW_Whs = ZGuid.Empty;
			AssertEquals("FCD", order.WD_INCO);
		}

		public override void TestWD_WW_Whs_AccessibilityToOperationalActions()
		{
			var actionFieldAttribute =
				ActionFieldAttribute.Get(typeof(WhsOrder).GetProperty(WhsDocketSchema.WD_WW_Whs.Name));
			AssertEquals(
				$"Action field attribute of {WhsDocketSchema.WD_WW_Whs.Name} should exist as this property is exposed to Operation Actions on WhsOrder.",
				actionFieldAttribute.CollectionType, typeof(WhsWarehouseCollectionWithSecurityCheck));
		}

		#endregion

		#region TestIsOrderHeld

		public void TestIsOrderHeld()
		{
			var whs = Helper.CreateWarehouse("1");
			var client = Helper.CreateClient();
			var order = GetNewBusinessObject();
			order.WD_WW_Whs = whs.PK;
			order.WD_OH_Client = client.PK;

			var refreshBindingCount = 0;
			order.IsOrderHeldInfo.ValueChanged += (s, e) => refreshBindingCount++;

			order.IsOrderHeld = true;
			AssertEquals(true, order.IsOrderHeld);
			AssertEquals(DocketStatus.Codes.Held, order.WD_DocketStatus);
			AssertEquals(1, refreshBindingCount);

			// Check that re-setting the status works
			order.IsOrderHeld = true;
			AssertEquals(true, order.IsOrderHeld);
			AssertEquals(DocketStatus.Codes.Held, order.WD_DocketStatus);
			AssertEquals(2, refreshBindingCount);

			order.IsOrderHeld = false;
			AssertEquals(false, order.IsOrderHeld);
			AssertEquals(DocketStatus.Codes.New, order.WD_DocketStatus);
			AssertEquals(3, refreshBindingCount);

			// Check that re-setting the status works
			order.IsOrderHeld = false;
			AssertEquals(false, order.IsOrderHeld);
			AssertEquals(DocketStatus.Codes.New, order.WD_DocketStatus);
			AssertEquals(4, refreshBindingCount);

			Factory.Save();

			order.IsOrderHeld = true;
			AssertEquals(true, order.IsOrderHeld);
			AssertEquals(DocketStatus.Codes.Held, order.WD_DocketStatus);
			AssertEquals(5, refreshBindingCount);

			order.IsOrderHeld = false;
			AssertEquals(false, order.IsOrderHeld);
			AssertEquals(DocketStatus.Codes.Entered, order.WD_DocketStatus);
			AssertEquals(6, refreshBindingCount);
		}

		public void TestIsOrderHeld_CaseInsensitive()
		{
			var whs = Helper.CreateWarehouse("1");
			var client = Helper.CreateClient();
			var order = GetNewBusinessObject();
			order.WD_WW_Whs = whs.PK;
			order.WD_OH_Client = client.PK;

			var refreshBindingCount = 0;
			order.IsOrderHeldInfo.ValueChanged += (s, e) => refreshBindingCount++;

			order.WD_DocketStatus = "nEw";
			AssertEquals(false, order.IsOrderHeld);

			order.IsOrderHeld = true;
			AssertEquals(true, order.IsOrderHeld);
			AssertEquals(DocketStatus.Codes.Held, order.WD_DocketStatus);
			AssertEquals(1, refreshBindingCount);

			order.WD_DocketStatus = "hEl";
			AssertEquals(true, order.IsOrderHeld);

			order.IsOrderHeld = false;
			AssertEquals(false, order.IsOrderHeld);
			AssertEquals(DocketStatus.Codes.New, order.WD_DocketStatus);
			AssertEquals(2, refreshBindingCount);

			Factory.Save();

			order.WD_DocketStatus = "eNt";
			AssertEquals(false, order.IsOrderHeld);

			order.IsOrderHeld = true;
			AssertEquals(true, order.IsOrderHeld);
			AssertEquals(DocketStatus.Codes.Held, order.WD_DocketStatus);
			AssertEquals(3, refreshBindingCount);

			order.IsOrderHeld = false;
			AssertEquals(false, order.IsOrderHeld);
			AssertEquals(DocketStatus.Codes.Entered, order.WD_DocketStatus);
			AssertEquals(4, refreshBindingCount);
		}

		public void TestIsOrderHeld_Canceled()
		{
			var whs = Helper.CreateWarehouse("1");
			var client = Helper.CreateClient();
			var order = GetNewBusinessObject();
			order.WD_WW_Whs = whs.PK;
			order.WD_OH_Client = client.PK;

			var refreshBindingCount = 0;
			order.IsOrderHeldInfo.ValueChanged += (s, e) => refreshBindingCount++;

			order.CancelReactivateDocket();
			AssertEquals("Precondition.", DocketStatus.Codes.Cancelled, order.WD_DocketStatus);

			AssertExceptionThrown<InvalidOperationException>(() => order.IsOrderHeld = true);
			AssertEquals("Should *not* change the status.", false, order.IsOrderHeld);
			AssertEquals("Should *not* change the status.", DocketStatus.Codes.Cancelled, order.WD_DocketStatus);
			AssertEquals(0, refreshBindingCount);

			AssertExceptionThrown<InvalidOperationException>(() => order.IsOrderHeld = false);
			AssertEquals("Should *not* change the status.", false, order.IsOrderHeld);
			AssertEquals("Should *not* change the status.", DocketStatus.Codes.Cancelled, order.WD_DocketStatus);
			AssertEquals(0, refreshBindingCount);
		}

		public void TestIsOrderHeld_AttachedToPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition.", DocketStatus.Codes.AttachedToPick, order.WD_DocketStatus);

			var refreshBindingCount = 0;
			order.IsOrderHeldInfo.ValueChanged += (s, e) => refreshBindingCount++;

			AssertExceptionThrown<InvalidOperationException>(() => order.IsOrderHeld = false);
			AssertEquals("Should *not* change the status.", false, order.IsOrderHeld);
			AssertEquals("Should *not* change the status.", DocketStatus.Codes.AttachedToPick, order.WD_DocketStatus);
			AssertEquals(0, refreshBindingCount);

			AssertExceptionThrown<InvalidOperationException>(() => order.IsOrderHeld = true);
			AssertEquals("Should *not* change the status.", false, order.IsOrderHeld);
			AssertEquals("Should *not* change the status.", DocketStatus.Codes.AttachedToPick, order.WD_DocketStatus);
			AssertEquals(0, refreshBindingCount);
		}

		public void TestIsOrderHeld_Departed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertEquals("Precondition.", WhsOrderStatus.Codes.Departed, order.WD_DocketStatus);

			var refreshBindingCount = 0;
			order.IsOrderHeldInfo.ValueChanged += (s, e) => refreshBindingCount++;

			AssertExceptionThrown<InvalidOperationException>(() => order.IsOrderHeld = true);
			AssertEquals("Should *not* change the status.", false, order.IsOrderHeld);
			AssertEquals("Should *not* change the status.", WhsOrderStatus.Codes.Departed, order.WD_DocketStatus);
			AssertEquals(0, refreshBindingCount);

			AssertExceptionThrown<InvalidOperationException>(() => order.IsOrderHeld = false);
			AssertEquals("Should *not* change the status.", false, order.IsOrderHeld);
			AssertEquals("Should *not* change the status.", WhsOrderStatus.Codes.Departed, order.WD_DocketStatus);
			AssertEquals(0, refreshBindingCount);
		}

		#endregion

		#region TestFieldUpdaterUpdate_WhenIsOrderHeldIsReadOnly_ShouldFail

		public void TestFieldUpdaterUpdate_WhenIsOrderHeldIsReadOnly_ShouldFail()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);

			var propertyInfo = order.GetType().GetProperty(nameof(order.IsOrderHeld));

			var outMessage = string.Empty;
			var checkResult = ((IPropertyChecker)order).IsPropertyUpdatableViaXueAdditionalFields(propertyInfo, true, out outMessage);

			AssertEquals("Can't set IsOrderHeld when it's read-only", false, checkResult);
			AssertEquals("Should return error message", "Is Order Held can only be set for Entered, New or Held Orders.", outMessage);
		}

		#endregion

		#region TestProductCount

		protected override void TestProductCountCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 7m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 15m);
			Helper.CreateWhsOrderLine(order2, data.Part2, 3m);

			AssertEquals("Order1 in db returns correct product count.", 1, order1.ProductCount);
			AssertEquals("Order2 in db returns correct product count.", 2, order2.ProductCount);

			Factory.Save();
			AssertEquals("Order1 not in db returns correct product count.", 1, order1.ProductCount);
			AssertEquals("Order2 not in db returns correct product count.", 2, order2.ProductCount);
		}

		public void TestOrderProductCount_DuplicateProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			Helper.CreateWhsOrderLine(order, data.Part1, 3m);
			AssertEquals("Order in db returns correct product count.", 1, order.ProductCount);

			Factory.Save();
			AssertEquals("Order not in db returns correct product count.", 1, order.ProductCount);
		}

		public void TestOrderProductCount_InvalidProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			var orderline = order.Lines.AddNew();
			AssertEquals("Product is empty", false, orderline.WE_OP.IsValid);
			AssertEquals("Order in db returns correct product count.", 0, order.ProductCount);
		}

		public void TestOrderProductCount_MultipleDifferentProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			var part4 = Helper.CreateProduct(data.Org1, "P4");
			var part5 = Helper.CreateProduct(data.Org1, "P5");
			var part6 = Helper.CreateProduct(data.Org1, "P6");

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			Helper.CreateWhsOrderLine(order, data.Part2, 13m);
			Helper.CreateWhsOrderLine(order, part3, 9m);
			Helper.CreateWhsOrderLine(order, part4, 5m);
			Helper.CreateWhsOrderLine(order, part5, 7m);
			Helper.CreateWhsOrderLine(order, part6, 16m);
			AssertEquals("Order in db returns correct product count.", 6, order.ProductCount);

			Factory.Save();
			AssertEquals("Order not in db returns correct product count.", 6, order.ProductCount);
		}

		public void TestOrderProductCount_DeletingDocketLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			Helper.CreateWhsOrderLine(order, data.Part1, 3m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			Factory.Save();

			AssertEquals("Order has 3 lines.", 3, order.Lines.Count);
			AssertEquals("Order returns correct product count.", 2, order.ProductCount);

			orderLine3.Delete();
			AssertEquals("Order has 2 lines.", 2, order.Lines.Count);
			AssertEquals("Order returns correct product count after orderline is deleted.", 1, order.ProductCount);
		}

		public void TestOrderProductCount_DBHits()
		{
			var numberOfOrderLines = 25;
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var testOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			Helper.CreateWhsOrderLine(testOrder, data.Part1, 15m);

			for (var i = 0; i < numberOfOrderLines; i++)
			{
				Helper.CreateWhsOrderLine(testOrder, data.Part2, 5m);
			}

			Factory.Save();

			var expectedDBHits = new Dictionary<string, int>()
			{
				{ WhsDocketSchema.Constants.TableName, 1 }, { WhsDocketLineSchema.Constants.TableName, 1 }
			};

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, newFactory))
			{
				var orderInNewFactory = newFactory.Load<WhsOrder>(testOrder.PK);
				AssertEquals("orderInNewFactory returns correct product count.", 2, orderInNewFactory.ProductCount);
			}
		}

		#endregion

		#region TestDistributionCentreNameOrPK

		public void TestDistributionCentreNameOrPK()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");

			var distributionCentre = Factory.New<OrgHeader>();
			distributionCentre.OH_Code = "TEST";
			distributionCentre.OH_FullName = "FULL NAME";

			order.DistributionCentreDocAddress.E2_AddressOverride = true;
			order.DistributionCentreDocAddress.E2_CompanyName = "TEST COMPANY";
			AssertEquals("TEST COMPANY", order.DistributionCentreNameOrPK);

			order.DistributionCentreDocAddress.E2_AddressOverride = false;
			order.DistributionCentreDocAddress.OrganisationPK = distributionCentre.PK;
			AssertEquals(distributionCentre.PK.ToString(), order.DistributionCentreNameOrPK);

			order.DistributionCentreDocAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty.ToString(), order.DistributionCentreNameOrPK);
		}

		#endregion

		#region TestDistributionCentreAddressPK

		public void TestDistributionCentreAddressPK()
		{
			var org1 = Helper.CreateClient("111", "111");
			var whs1 = Helper.CreateWarehouse("1", "A", 2, 2);
			var org2 = Helper.CreateClient("222", "222");
			var product = Helper.CreateProduct(org1, "P1");
			var order = Helper.CreateWhsOrderWithOrderLine(org1, whs1, "ORD1", product, 10m);

			Factory.Save();

			order.DistributionCentreAddressPK = org2.MainAddress.PK;
			AssertEquals(org2.MainAddress.PK, order.DistributionCentreDocAddress.E2_OA_Address);

			order.DistributionCentreDocAddress.E2_AddressOverride = false;
			AssertEquals(order.DistributionCentreDocAddress.E2_OA_Address, order.DistributionCentreAddressPK);

			order.DistributionCentreDocAddress.E2_AddressOverride = true;
			AssertEquals(ZGuid.Empty, order.DistributionCentreAddressPK);
		}

		#endregion

		#region TestDistributionCentreFieldType

		public void TestDistributionCentreFieldType()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");

			order.DistributionCentreDocAddress.E2_AddressOverride = true;
			AssertEquals(nameof(FieldType.Text), order.DistributionCentreFieldType);

			order.DistributionCentreDocAddress.E2_AddressOverride = false;
			AssertEquals(nameof(FieldType.Guid), order.DistributionCentreFieldType);
		}

		#endregion

		#region SetDefaults

		#region TestSetDefaultFreightForwarder

		public void TestSetDefaultFreightForwarder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			var consignee = Helper.CreateClient("Consignee");
			consignee.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var order = Factory.New<WhsOrder>();
			var forwarder = Factory.New<OrgHeader>();
			var clientForwarder = Factory.New<OrgHeader>();
			var consigneeForwarder = Factory.New<OrgHeader>();

			consignee.AddRelatedParty(consigneeForwarder.PK, RelatedPartyTypeList.Codes.WarehouseForwarder,
				RelatedPartyDirectionList.Codes.Forwarder, "", "", null);
			client.AddRelatedParty(clientForwarder.PK, RelatedPartyTypeList.Codes.WarehouseForwarder,
				RelatedPartyDirectionList.Codes.Forwarder, "", "", null);

			order.WD_OH_Client = client.PK;
			AssertNull("Should not default without the Consignee set.", order.Forwarder);

			order.WD_OH_Client = ZGuid.Empty;
			order.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			AssertNull("Should not default without the Client set.", order.Forwarder);

			order.WD_OH_Forwarder = forwarder.PK;
			order.WD_OH_Client = client.PK;
			AssertEquals("Should not default when Forwarder is filled in.", forwarder, order.Forwarder);

			order.WD_OH_Forwarder = ZGuid.Empty;
			order.WD_OH_Client = ZGuid.Empty;
			order.WD_OH_Client = client.PK;
			AssertEquals("Consignee Warehouse Forwarder takes precedence when defaulting.", consigneeForwarder,
				order.Forwarder);

			consignee.AllRelatedParties.RemoveAndDeleteAll();
			order.WD_OH_Forwarder = ZGuid.Empty;
			order.WD_OH_Client = ZGuid.Empty;
			order.WD_OH_Client = client.PK;
			AssertEquals("Defaulting falls back to Client Warehouse Forwarder.", clientForwarder, order.Forwarder);

			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.WD_OH_Forwarder = ZGuid.Empty;
			order.WD_OH_Client = ZGuid.Empty;
			order.WD_OH_Client = client.PK;
			AssertEquals("Defaulting falls back to Client Warehouse Forwarder.", clientForwarder, order.Forwarder);

			client.AllRelatedParties.RemoveAndDeleteAll();
			order.WD_OH_Forwarder = ZGuid.Empty;
			order.WD_OH_Client = ZGuid.Empty;
			order.WD_OH_Client = client.PK;
			AssertNull(order.Forwarder);
		}

		#endregion

		#region TestSetDefaultINCOTerm

		public void TestSetDefaultINCOTerm()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			var consignee = Helper.CreateClient("Consignee");
			consignee.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			var supplierBuyerLink = consignee.SupplierLinks.AddNew(client);
			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_IncoTerm = "PPD";

			// This is to prevent an INCO term being Defaulted from the relationship
			consignee.SupplierLinks.AddNew(Factory.NewWithValidTestData<OrgHeader>());
			client.BuyerLinks.AddNew(Factory.NewWithValidTestData<OrgHeader>());

			var order = Factory.New<WhsOrder>();
			order.WD_OH_Client = client.PK;
			AssertEquals("Should not default without the Consignee set.", "", order.WD_INCO);

			order.WD_OH_Client = ZGuid.Empty;
			order.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			AssertEquals("Should not default without the Client set.", "", order.WD_INCO);

			order.WD_OH_Client = client.PK;
			AssertEquals("PPD", order.WD_INCO);

			order.WD_INCO = ZString.Empty;
			order.ConsigneeDocAddress.OrganisationPK = ZGuid.Empty;
			order.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			AssertEquals("PPD", order.WD_INCO);

			order.WD_OH_Client = ZGuid.Empty;
			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_IncoTerm = "XXX";
			order.WD_OH_Client = client.PK;
			AssertEquals("XXX is not a valid INCO Term.", "PPD", order.WD_INCO);

			order.WD_OH_Client = ZGuid.Empty;
			order.WD_INCO = ZString.Empty;
			client.MiscServ.OM_IMDefaultINCOTerm = "CLT";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_IncoTerm = "PPD";
			order.WD_OH_Client = client.PK;
			AssertEquals("Relationship should take precedence.", "PPD", order.WD_INCO);

			order.WD_OH_Client = ZGuid.Empty;
			order.WD_INCO = ZString.Empty;
			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_IncoTerm = "XXX";
			order.WD_OH_Client = client.PK;
			AssertEquals("Should take from Client if relationship doesn't contain valid INCO Term.", "CLT",
				order.WD_INCO);
		}

		#endregion

		#region TestSetDefaultContainerMode

		public void TestSetDefaultContainerMode()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			var consignee = Helper.CreateClient("Consignee");
			consignee.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			var supplierBuyerLink = consignee.SupplierLinks.AddNew(client);
			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_ContainerMode = "LCL";

			// This is to prevent a Consignee or Client being Defaulted from the relationship
			consignee.SupplierLinks.AddNew(Factory.NewWithValidTestData<OrgHeader>());
			client.BuyerLinks.AddNew(Factory.NewWithValidTestData<OrgHeader>());

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			AssertEquals("Should not default without the Consignee set.", "", order.WD_ContainerMode);

			order.WD_OH_Client = ZGuid.Empty;
			order.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			AssertEquals("Should not default without the Client set.", "", order.WD_ContainerMode);

			order.WD_OH_Client = client.PK;
			AssertEquals("LCL", order.WD_ContainerMode);

			order.ConsigneeDocAddress.OrganisationPK = ZGuid.Empty;
			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_ContainerMode = "FCL";
			order.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			AssertEquals("Should not change because was not empty.", "LCL", order.WD_ContainerMode);

			order.WD_OH_Client = ZGuid.Empty;
			order.WD_TransportMode = "ROA";
			order.WD_ContainerMode = "";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_ContainerMode = "BLK";
			order.WD_OH_Client = client.PK;
			AssertEquals("ROA", order.WD_TransportMode);
			AssertEquals("BLK is *not* a Road Transport Container Mode Type.", "", order.WD_ContainerMode);

			order.WD_OH_Client = ZGuid.Empty;
			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_ContainerMode = "FTL";
			order.WD_OH_Client = client.PK;
			AssertEquals("Should be changed because FTL is a valid Road Transport Container Mode.", "FTL",
				order.WD_ContainerMode);
		}

		#endregion

		#region TestGetCarrierServiceLevelCode

		public void TestGetCarrierServiceLevelCode()
		{
			var order = Factory.New<WhsOrder>();
			var serviceLevelCode = "SL1";
			order.WD_PL_NKCarrierServiceLevel = serviceLevelCode;
			AssertEquals(serviceLevelCode, ((IPackingParent)order).CarrierServiceLevelCode(null));
		}

		#endregion

		#region TestGetCarrier

		public void TestGetCarrier()
		{
			var order = Factory.New<WhsOrder>();
			OrgHeader transportCo = Helper.CreateClient();

			order.TransportCoDocAddress.OrganisationPK = transportCo.PK;
			AssertEquals(transportCo, ((IPackingParent)order).GetCarrier(null));
		}

		#endregion

		#region TestTransportCompanyOrCarrierServiceLevel_ReadOnly

		public void TestTransportCompanyOrCarrierServiceLevel_ReadOnly_OrderIsReadyToPack()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);

			AssertNotEquals("Precondition - WhsOrderStatus should NOT be Ready To Pack.", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);
			AssertEquals("Precondition - TransportCoDocAddress should NOT be readonly.", false, order.TransportCoDocAddress.ReadOnly);
			AssertEquals("Precondition - WD_PL_NKCarrierServiceLevel should NOT be readonly.", false, order.WD_PL_NKCarrierServiceLevelInfo.ReadOnly);

			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("WhsOrderStatus should be Ready To Pack.", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);
			AssertEquals("TransportCoDocAddress should be readonly.", true, order.TransportCoDocAddress.ReadOnly);
			AssertEquals("WD_PL_NKCarrierServiceLevel should be readonly.", true, order.WD_PL_NKCarrierServiceLevelInfo.ReadOnly);
		}

		#endregion

		#region TestSetDefaultCarrierServiceLevel

		public void TestSetDefaultCarrierServiceLevel()
		{
			var client = Helper.CreateClient("Client");
			var consignee = Helper.CreateClient("Consignee");
			consignee.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			var order = Factory.New<WhsOrder>();

			var serviceLevel1 = Factory.New<RefServiceLevel>();
			var serviceLevel2 = Factory.New<RefServiceLevel>();
			var serviceLevel3 = Factory.New<RefServiceLevel>();

			serviceLevel1.RS_Code = "SL1";
			serviceLevel2.RS_Code = "SL2";
			serviceLevel3.RS_Code = "SL3";

			client.MiscServ.OM_RS_NKEXDefaultServiceLevel = "SL1";
			consignee.MiscServ.OM_RS_NKIMDefaultServiceLevel = "SL2";

			order.WD_OH_Client = client.PK;
			AssertEquals("Service Level should be defaulted from Client Service Level",
				client.MiscServ.EXDefaultServiceLevel.RS_Code, order.WD_PL_NKCarrierServiceLevel);

			order.WD_OH_Client = ZGuid.Empty;
			order.WD_PL_NKCarrierServiceLevel = ZString.Empty;
			order.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			AssertEquals("Service Level should be defaulted from Consignee Service Level",
				consignee.MiscServ.IMDefaultServiceLevel.RS_Code, order.WD_PL_NKCarrierServiceLevel);

			order.WD_PL_NKCarrierServiceLevel = serviceLevel3.RS_Code;

			order.WD_OH_Client = client.PK;
			AssertEquals("Service Level shouldn't change, as it's not blank", serviceLevel3.RS_Code,
				order.WD_PL_NKCarrierServiceLevel);

			order.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			AssertEquals("Service Level shouldn't change, as it's not blank", serviceLevel3.RS_Code,
				order.WD_PL_NKCarrierServiceLevel);

			var supplierBuyerLink = consignee.SupplierLinks.AddNew(client);
			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_RS_NKDefaultServiceLevel = "SMP";
			order.WD_OH_Client = ZGuid.Empty;
			order.WD_OH_Client = client.PK;
			AssertEquals("Relationship should take precedence", "SMP", order.WD_PL_NKCarrierServiceLevel);

			order.WD_OH_Client = ZGuid.Empty;
			order.WD_PL_NKCarrierServiceLevel = "";
			order.WD_OH_Client = client.PK;
			AssertEquals("Relationship should take precedence", "SMP", order.WD_PL_NKCarrierServiceLevel);

			order.WD_PL_NKCarrierServiceLevel = "";
			supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_RS_NKDefaultServiceLevel = "";
			order.WD_OH_Client = ZGuid.Empty;
			order.WD_OH_Client = client.PK;
			AssertEquals("No Service level on Relationship so should use Consignee", serviceLevel2.RS_Code,
				order.WD_PL_NKCarrierServiceLevel);
		}

		#endregion

		#region TestCarrierServiceLevel_ReadOnly

		public void TestCarrierServiceLevel_ReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			AssertEquals("Precondition", false, order.WD_WLO_PlannedLoad.IsValid);
			AssertEquals(false, order.WD_PL_NKCarrierServiceLevelInfo.ReadOnly);

			order.WD_WLO_PlannedLoad = ZGuid.NewZGuid();
			AssertEquals(true, order.WD_PL_NKCarrierServiceLevelInfo.ReadOnly);
		}

		public void TestCarrierServiceLevel_ReadOnly_OrderLinkedToLoadThruPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			Factory.Save();

			AssertEquals("Precondition", false, order.WD_WLO_PlannedLoad.IsValid);
			AssertEquals("Precondition", false, order.WD_PL_NKCarrierServiceLevelInfo.ReadOnly);

			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01", "CDS", startTime: DateTimeOffset.Now);
			Helper.CreatePickNew(order);
			var packageJob = order.PackageJob;
			var package = packageJob.Packages.AddNew("BOX", 1);
			Helper.CreateLoadPkgPackagePivot(package.PK, load);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			AssertEquals("Order has no planned load.", false, orderInNewFactory.WD_WLO_PlannedLoad.IsValid);
			AssertEquals(true, orderInNewFactory.WD_PL_NKCarrierServiceLevelInfo.ReadOnly);
		}

		#endregion

		#region TestSetDefaultTransportCo

		public void TestSetDefaultTransportCo()
		{
			var client = Helper.CreateClient("CLT");
			var clientTransportCo = Factory.New<OrgHeader>();
			client.AllRelatedParties.SetRelatedParty(clientTransportCo, RelatedPartyTypeList.Codes.LocalTransport,
				RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);

			var consignee = Factory.New<OrgHeader>();
			var consigneeAddress1 = consignee.Addresses.AddNew();
			var consigneeAddress2 = consignee.Addresses.AddNew();
			var consigneeAddress1TransportCo = Factory.New<OrgHeader>();
			var consigneeAddress2TransportCo = Factory.New<OrgHeader>();
			consignee.AllRelatedParties.SetRelatedParty(consigneeAddress1.PK, consigneeAddress1TransportCo,
				RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery,
				Constants.TransportModes.All, ZString.Empty);
			consignee.AllRelatedParties.SetRelatedParty(consigneeAddress2.PK, consigneeAddress2TransportCo,
				RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery,
				Constants.TransportModes.All, ZString.Empty);

			var warehouseOrgProxy = Factory.New<OrgHeader>();
			var warehouse = Helper.CreateWarehouse("WHS1");
			warehouse.WarehouseAddress.OA_OH = warehouseOrgProxy.PK;
			var warehouseTransportCo = Factory.New<OrgHeader>();
			warehouseOrgProxy.AllRelatedParties.SetRelatedParty(warehouseTransportCo,
				RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup,
				Constants.TransportModes.All, ZString.Empty);

			var order = Factory.New<WhsOrder>();

			WarehouseDataRegistry.Instance.TransportCoDefaultingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new TransportCoDefaultingRules() { Warehouse = 1, Client = 2, Consignee = 3 });
			order.TransportCoDocAddress.OrganisationPK = consigneeAddress1TransportCo.PK;
			order.WD_WW_Whs = warehouse.PK;
			order.WD_OH_Client = client.PK;
			order.ConsigneePK = consignee.PK;
			AssertEquals("Should not override an entered transport co.", consigneeAddress1TransportCo.PK,
				order.GetTransportCo().PK);

			order.TransportCoDocAddress.E2_AddressOverride = true;
			AssertEquals("Precondition", true, order.TransportCoDocAddress.E2_AddressOverride);
			order.WD_WW_Whs = warehouse.PK; // poke
			AssertEquals("Should not override an overriden transport co.", null, order.GetTransportCo());

			order.TransportCoDocAddress.E2_AddressOverride = false; // set back
			order.TransportCoDocAddress.OrganisationPK = ZGuid.Empty;
			order.WD_WW_Whs = warehouse.PK; // poke
			AssertEquals(warehouseTransportCo.PK, order.GetTransportCo().PK);

			WarehouseDataRegistry.Instance.TransportCoDefaultingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new TransportCoDefaultingRules() { Warehouse = 2, Client = 1, Consignee = 3 });
			order.TransportCoDocAddress.OrganisationPK = consigneeAddress1TransportCo.PK;
			order.WD_WW_Whs = warehouse.PK;
			order.WD_OH_Client = client.PK;
			order.ConsigneePK = consignee.PK;
			AssertEquals("Should not override an entered transport co.", consigneeAddress1TransportCo.PK,
				order.GetTransportCo().PK);

			order.TransportCoDocAddress.E2_AddressOverride = true;
			AssertEquals("Precondition", true, order.TransportCoDocAddress.E2_AddressOverride);
			order.WD_OH_Client = client.PK; // poke
			AssertEquals("Should not override an overriden transport co.", null, order.GetTransportCo());

			order.TransportCoDocAddress.E2_AddressOverride = false; // set back
			order.TransportCoDocAddress.OrganisationPK = ZGuid.Empty;
			order.WD_OH_Client = client.PK; // poke
			AssertEquals(clientTransportCo.PK, order.GetTransportCo().PK);

			WarehouseDataRegistry.Instance.TransportCoDefaultingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new TransportCoDefaultingRules() { Warehouse = 3, Client = 2, Consignee = 1 });
			order.TransportCoDocAddress.OrganisationPK = warehouseTransportCo.PK;
			order.WD_WW_Whs = warehouse.PK;
			order.WD_OH_Client = client.PK;
			order.ConsigneeAddressPK = consigneeAddress1.PK;
			AssertEquals("Should not override an entered transport co.", warehouseTransportCo.PK,
				order.GetTransportCo().PK);

			order.TransportCoDocAddress.E2_AddressOverride = true;
			AssertEquals("Precondition", true, order.TransportCoDocAddress.E2_AddressOverride);
			order.ConsigneePK = consignee.PK; // poke
			AssertEquals("Should not override an overriden transport co.", null, order.GetTransportCo());

			order.ConsigneeAddressPK = ZGuid.Empty;
			order.TransportCoDocAddress.E2_AddressOverride = false; // set back
			order.TransportCoDocAddress.OrganisationPK = ZGuid.Empty;
			order.ConsigneeAddressPK = consigneeAddress1.PK; // poke
			AssertEquals(consigneeAddress1TransportCo.PK, order.GetTransportCo().PK);

			order.ConsigneeAddressPK = consigneeAddress2.PK;
			AssertEquals("Should change as TransportCo was the previous consignee's default.",
				consigneeAddress2TransportCo.PK, order.GetTransportCo().PK);
		}

		public void TestSetDefaultTransportCo_Client_WithFactorySave()
		{
			var client1 = Helper.CreateClient("CLT");
			var client1TransportCo = Helper.CreateClient("CTC");
			client1.AllRelatedParties.SetRelatedParty(client1TransportCo, RelatedPartyTypeList.Codes.LocalTransport,
				RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);

			var client2 = Helper.CreateClient("CL2");
			var client2TransportCo = Helper.CreateClient("CT2");
			client2.AllRelatedParties.SetRelatedParty(client2TransportCo, RelatedPartyTypeList.Codes.LocalTransport,
				RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);

			var warehouseOrgProxy = Helper.CreateClient("WOP");
			var warehouse = Helper.CreateWarehouse("WHS1");
			warehouse.WarehouseAddress.OA_Code = "ADDRESS";
			warehouse.WarehouseAddress.OA_OH = warehouseOrgProxy.PK;
			var warehouseTransportCo = Helper.CreateClient("WTC");
			warehouseOrgProxy.AllRelatedParties.SetRelatedParty(warehouseTransportCo,
				RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup,
				Constants.TransportModes.All, ZString.Empty);

			var order = Factory.New<WhsOrder>();

			WarehouseDataRegistry.Instance.TransportCoDefaultingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new TransportCoDefaultingRules() { Warehouse = 2, Client = 1, Consignee = 3 });
			order.WD_OH_Client = client1.PK;
			order.WD_WW_Whs = warehouse.PK;
			AssertEquals(client1TransportCo.PK, order.GetTransportCo().PK);
			Factory.Save();

			var orderOnOtherFactory = (new BusinessObjectFactory()).Load<WhsOrder>(order.PK);
			orderOnOtherFactory.WD_OH_Client = client2.PK;
			AssertEquals("Should change as old TransportCo was the previous default.", client2TransportCo.PK,
				orderOnOtherFactory.GetTransportCo().PK);
		}

		public void TestSetDefaultTransportCo_Consignee_WithFactorySave()
		{
			var consignee1 = Helper.CreateClient("Consignee1");
			var consigneeAddress1 = consignee1.Addresses.AddNew();
			consigneeAddress1.FillWithValidTestData();
			var consigneeAddress2 = consignee1.Addresses.AddNew();
			consigneeAddress2.FillWithValidTestData();
			var consigneeAddress1TransportCo = Helper.CreateClient("CneTCO1");
			var consigneeAddress2TransportCo = Helper.CreateClient("CneTCO2");
			consignee1.AllRelatedParties.SetRelatedParty(consigneeAddress1.PK, consigneeAddress1TransportCo,
				RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery,
				Constants.TransportModes.All, ZString.Empty);
			consignee1.AllRelatedParties.SetRelatedParty(consigneeAddress2.PK, consigneeAddress2TransportCo,
				RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery,
				Constants.TransportModes.All, ZString.Empty);

			var consignee2 = Helper.CreateClient("Consignee2");
			var consignee2Address = consignee2.Addresses.AddNew();
			consignee2Address.FillWithValidTestData();
			var consignee2AddressTransportCo = Helper.CreateClient("Cn2TCO1");
			consignee2.AllRelatedParties.SetRelatedParty(consignee2Address.PK, consignee2AddressTransportCo,
				RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Delivery,
				Constants.TransportModes.All, ZString.Empty);

			var client = Helper.CreateClient("CLT");
			var warehouse = Helper.CreateWarehouse("WHS1");
			var order = Helper.CreateWhsOrder(client, warehouse);

			WarehouseDataRegistry.Instance.TransportCoDefaultingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new TransportCoDefaultingRules() { Warehouse = 3, Client = 2, Consignee = 1 });
			order.ConsigneePK = consignee1.PK;
			order.ConsigneeAddressPK = consigneeAddress1.PK;
			AssertEquals(consigneeAddress1TransportCo.PK, order.GetTransportCo().PK);

			Factory.Save();
			var orderOnOtherFactory1 = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			orderOnOtherFactory1.ConsigneeAddressPK = consigneeAddress2.PK;
			AssertEquals("Should change as old TransportCo was the previous consignee's default.",
				consigneeAddress2TransportCo.PK, orderOnOtherFactory1.GetTransportCo().PK);

			var orderOnOtherFactory2 = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			AssertEquals(consignee1.PK, orderOnOtherFactory2.ConsigneePK);
			orderOnOtherFactory2.ConsigneeAddressPK = consignee2Address.PK;
			AssertEquals("Should change as old TransportCo was the previous consignee's default.",
				consignee2AddressTransportCo.PK, orderOnOtherFactory2.GetTransportCo().PK);
		}

		public void TestSetDefaultTransportCo_Whs_WithFactorySave()
		{
			var client = Helper.CreateClient("CLT");
			var clientTransportCo = Helper.CreateClient("CTC");
			client.AllRelatedParties.SetRelatedParty(clientTransportCo, RelatedPartyTypeList.Codes.LocalTransport,
				RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);

			var warehouse1OrgProxy = Helper.CreateClient("WOP");
			var warehouse1 = Helper.CreateWarehouse("WHS1");
			warehouse1.WarehouseAddress.OA_Code = "ADDRESS";
			warehouse1.WarehouseAddress.OA_OH = warehouse1OrgProxy.PK;
			var warehouse1TransportCo = Helper.CreateClient("WTC");
			warehouse1OrgProxy.AllRelatedParties.SetRelatedParty(warehouse1TransportCo,
				RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup,
				Constants.TransportModes.All, ZString.Empty);

			var warehouse2OrgProxy = Helper.CreateClient("WP2");
			var warehouse2 = Helper.CreateWarehouse("WHS2");
			warehouse2.WarehouseAddress.OA_Code = "ADDRESS2";
			warehouse2.WarehouseAddress.OA_OH = warehouse2OrgProxy.PK;
			var warehouse2TransportCo = Helper.CreateClient("WB2");
			warehouse2OrgProxy.AllRelatedParties.SetRelatedParty(warehouse2TransportCo,
				RelatedPartyTypeList.Codes.LocalTransport, RelatedPartyDirectionList.Codes.Pickup,
				Constants.TransportModes.All, ZString.Empty);

			var order = Factory.New<WhsOrder>();

			WarehouseDataRegistry.Instance.TransportCoDefaultingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new TransportCoDefaultingRules() { Warehouse = 1, Client = 2, Consignee = 3 });
			order.WD_WW_Whs = warehouse1.PK;
			order.WD_OH_Client = client.PK;
			AssertEquals(warehouse1TransportCo.PK, order.GetTransportCo().PK);
			Factory.Save();

			var orderOnOtherFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			orderOnOtherFactory.WD_WW_Whs = warehouse2.PK;
			AssertEquals("Should change as old TransportCo was the previous default.", warehouse2TransportCo.PK,
				orderOnOtherFactory.GetTransportCo().PK);
		}

		public void TestSetDefaultTransportCo_DoesntRunOnImporting()
		{
			var client1 = Helper.CreateClient("CLT");
			var client1TransportCo = Helper.CreateClient("CTC");
			client1.AllRelatedParties.SetRelatedParty(client1TransportCo, RelatedPartyTypeList.Codes.LocalTransport,
				RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);

			var order = Factory.New<WhsOrder>();
			((ISupportDataImporting)order).IsImportingData = true;
			WarehouseDataRegistry.Instance.TransportCoDefaultingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new TransportCoDefaultingRules() { Warehouse = 2, Client = 1, Consignee = 3 });
			order.WD_OH_Client = client1.PK;
			AssertEquals(null, order.GetTransportCo());
		}

		#endregion

		#region TestSetDefaultTransportBillTo

		public void TestSetDefaultTransportBillTo()
		{
			var client = Helper.CreateClient();
			var clientBillTo = Factory.New<OrgHeader>();
			client.AllRelatedParties.SetRelatedParty(clientBillTo, RelatedPartyTypeList.Codes.LocalTransportBillTo,
				RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);

			var consignee = Factory.New<OrgHeader>();
			var consigneeAddress1 = consignee.Addresses.AddNew();
			var consigneeAddress2 = consignee.Addresses.AddNew();
			var consigneeAddress1BillTo = Factory.New<OrgHeader>();
			var consigneeAddress2BillTo = Factory.New<OrgHeader>();
			consignee.AllRelatedParties.SetRelatedParty(consigneeAddress1.PK, consigneeAddress1BillTo,
				RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Delivery,
				Constants.TransportModes.All, ZString.Empty);
			consignee.AllRelatedParties.SetRelatedParty(consigneeAddress2.PK, consigneeAddress2BillTo,
				RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Delivery,
				Constants.TransportModes.All, ZString.Empty);

			var warehouseOrgProxy = Factory.New<OrgHeader>();
			var warehouse = Helper.CreateWarehouse("WHS1");
			warehouse.WarehouseAddress.OA_OH = warehouseOrgProxy.PK;
			var warehouseBillTo = Factory.New<OrgHeader>();
			warehouseOrgProxy.AllRelatedParties.SetRelatedParty(warehouseBillTo,
				RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Pickup,
				Constants.TransportModes.All, ZString.Empty);

			var order = Factory.New<WhsOrder>();

			WarehouseDataRegistry.Instance.TransportCoDefaultingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new TransportCoDefaultingRules() { Warehouse = 1, Client = 2, Consignee = 3 });
			order.TransportBillToDocAddress.OrganisationPK = consigneeAddress1BillTo.PK;
			order.WD_WW_Whs = warehouse.PK;
			order.WD_OH_Client = client.PK;
			order.ConsigneePK = consignee.PK;
			AssertEquals("Should not override an entered transport bill to co.", consigneeAddress1BillTo.PK,
				order.TransportBillTo.PK);

			order.TransportBillToDocAddress.E2_AddressOverride = true;
			AssertEquals("Precondition", true, order.TransportBillToDocAddress.E2_AddressOverride);
			order.WD_WW_Whs = warehouse.PK; // poke
			AssertEquals("Should not override an overriden transport bill to co.", null, order.TransportBillTo);

			order.TransportBillToDocAddress.E2_AddressOverride = false; // set back
			order.TransportBillToDocAddress.OrganisationPK = ZGuid.Empty;
			order.WD_WW_Whs = warehouse.PK; // poke
			AssertEquals(warehouseBillTo.PK, order.TransportBillTo.PK);

			WarehouseDataRegistry.Instance.TransportCoDefaultingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new TransportCoDefaultingRules() { Warehouse = 2, Client = 1, Consignee = 3 });
			order.TransportBillToDocAddress.OrganisationPK = consigneeAddress1BillTo.PK;
			order.WD_WW_Whs = warehouse.PK;
			order.WD_OH_Client = client.PK;
			order.ConsigneePK = consignee.PK;
			AssertEquals("Should not override an entered transport bill to co.", consigneeAddress1BillTo.PK,
				order.TransportBillTo.PK);

			order.TransportBillToDocAddress.E2_AddressOverride = true;
			AssertEquals("Precondition", true, order.TransportBillToDocAddress.E2_AddressOverride);
			order.WD_OH_Client = client.PK; // poke
			AssertEquals("Should not override an overriden transport bill to co.", null, order.TransportBillTo);

			order.TransportBillToDocAddress.E2_AddressOverride = false; // set back
			order.TransportBillToDocAddress.OrganisationPK = ZGuid.Empty;
			order.WD_OH_Client = client.PK; // poke
			AssertEquals(clientBillTo.PK, order.TransportBillTo.PK);

			WarehouseDataRegistry.Instance.TransportCoDefaultingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new TransportCoDefaultingRules() { Warehouse = 3, Client = 2, Consignee = 1 });
			order.TransportBillToDocAddress.OrganisationPK = warehouseBillTo.PK;
			order.WD_WW_Whs = warehouse.PK;
			order.WD_OH_Client = client.PK;
			order.ConsigneeAddressPK = consigneeAddress1.PK;
			AssertEquals("Should not override an entered transport bill to co.", warehouseBillTo.PK,
				order.TransportBillTo.PK);

			order.TransportBillToDocAddress.E2_AddressOverride = true;
			AssertEquals("Precondition", true, order.TransportBillToDocAddress.E2_AddressOverride);
			order.ConsigneeAddressPK = consignee.PK; // poke
			AssertEquals("Should not override an overriden transport bill to co.", null, order.TransportBillTo);

			order.ConsigneeAddressPK = ZGuid.Empty;
			order.TransportBillToDocAddress.E2_AddressOverride = false; // set back
			order.TransportBillToDocAddress.OrganisationPK = ZGuid.Empty;
			order.ConsigneeAddressPK = consigneeAddress1.PK; // poke
			AssertEquals(consigneeAddress1BillTo.PK, order.TransportBillTo.PK);

			order.ConsigneeAddressPK = consigneeAddress2.PK;
			AssertEquals("Should change as TransportBillToCo was the previous consignee's default.",
				consigneeAddress2BillTo.PK, order.TransportBillTo.PK);
		}

		public void TestSetDefaultTransportBillToCo_Client_WithFactorySave()
		{
			var client1 = Helper.CreateClient("CLT");
			var client1TransportBillToCo = Helper.CreateClient("CBT");
			client1.AllRelatedParties.SetRelatedParty(client1TransportBillToCo,
				RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Pickup,
				Constants.TransportModes.All, ZString.Empty);

			var client2 = Helper.CreateClient("CL2");
			var client2TransportBillToCo = Helper.CreateClient("CB2");
			client2.AllRelatedParties.SetRelatedParty(client2TransportBillToCo,
				RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Pickup,
				Constants.TransportModes.All, ZString.Empty);

			var warehouseOrgProxy = Helper.CreateClient("WOP");
			var warehouse = Helper.CreateWarehouse("WHS1");
			warehouse.WarehouseAddress.OA_Code = "ADDRESS";
			warehouse.WarehouseAddress.OA_OH = warehouseOrgProxy.PK;
			var warehouseTransportBillToCo = Helper.CreateClient("WBT");
			warehouseOrgProxy.AllRelatedParties.SetRelatedParty(warehouseTransportBillToCo,
				RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Pickup,
				Constants.TransportModes.All, ZString.Empty);

			var order = Factory.New<WhsOrder>();

			WarehouseDataRegistry.Instance.TransportCoDefaultingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new TransportCoDefaultingRules() { Warehouse = 2, Client = 1, Consignee = 3 });
			order.WD_OH_Client = client1.PK;
			order.WD_WW_Whs = warehouse.PK;
			AssertEquals(client1TransportBillToCo.PK, order.TransportBillTo.PK);
			Factory.Save();

			var orderOnOtherFactory = (new BusinessObjectFactory()).Load<WhsOrder>(order.PK);
			orderOnOtherFactory.WD_OH_Client = client2.PK;
			AssertEquals("Should change as old TransportCo was the previous default.", client2TransportBillToCo.PK,
				orderOnOtherFactory.TransportBillTo.PK);
		}

		public void TestSetDefaultTransportBillTo_Consignee_WithFactorySave()
		{
			var consignee1 = Helper.CreateClient("Consignee1");
			var consigneeAddress1 = consignee1.Addresses.AddNew();
			consigneeAddress1.FillWithValidTestData();
			var consigneeAddress2 = consignee1.Addresses.AddNew();
			consigneeAddress2.FillWithValidTestData();
			var consigneeAddress1TransportBillToCo = Helper.CreateClient("CneTCO1");
			var consigneeAddress2TransportBillToCo = Helper.CreateClient("CneTCO2");
			consignee1.AllRelatedParties.SetRelatedParty(consigneeAddress1.PK, consigneeAddress1TransportBillToCo,
				RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Delivery,
				Constants.TransportModes.All, ZString.Empty);
			consignee1.AllRelatedParties.SetRelatedParty(consigneeAddress2.PK, consigneeAddress2TransportBillToCo,
				RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Delivery,
				Constants.TransportModes.All, ZString.Empty);

			var consignee2 = Helper.CreateClient("Consignee2");
			var consignee2Address = consignee2.Addresses.AddNew();
			consignee2Address.FillWithValidTestData();
			var consignee2AddressTransportBillTo = Helper.CreateClient("Cn2TCO1");
			consignee2.AllRelatedParties.SetRelatedParty(consignee2Address.PK, consignee2AddressTransportBillTo,
				RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Delivery,
				Constants.TransportModes.All, ZString.Empty);

			var client = Helper.CreateClient("CLT");
			var warehouse = Helper.CreateWarehouse("WHS1");
			var order = Helper.CreateWhsOrder(client, warehouse);

			WarehouseDataRegistry.Instance.TransportCoDefaultingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new TransportCoDefaultingRules() { Warehouse = 3, Client = 2, Consignee = 1 });
			order.ConsigneePK = consignee1.PK;
			order.ConsigneeAddressPK = consigneeAddress1.PK;
			AssertEquals(consigneeAddress1TransportBillToCo.PK, order.TransportBillTo.PK);

			Factory.Save();
			var orderOnOtherFactory1 = (new BusinessObjectFactory()).Load<WhsOrder>(order.PK);
			orderOnOtherFactory1.ConsigneeAddressPK = consigneeAddress2.PK;
			AssertEquals("Should change as old TransportBillToCo was the previous consignee's default.",
				consigneeAddress2TransportBillToCo.PK, orderOnOtherFactory1.TransportBillTo.PK);

			var orderOnOtherFactory2 = (new BusinessObjectFactory()).Load<WhsOrder>(order.PK);
			AssertEquals(consignee1.PK, orderOnOtherFactory2.ConsigneePK);
			orderOnOtherFactory2.ConsigneeAddressPK = consignee2Address.PK;
			AssertEquals("Should change as old TransportBillToCo was the previous consignee's default.",
				consignee2AddressTransportBillTo.PK, orderOnOtherFactory2.TransportBillTo.PK);
		}

		public void TestSetDefaultTransportBillToCo_Whs_WithFactorySave()
		{
			var client = Helper.CreateClient("CLT");
			var clientTransportBillToCo = Helper.CreateClient("CBT");
			client.AllRelatedParties.SetRelatedParty(clientTransportBillToCo,
				RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Pickup,
				Constants.TransportModes.All, ZString.Empty);

			var warehouse1OrgProxy = Helper.CreateClient("WOP");
			var warehouse1 = Helper.CreateWarehouse("WHS1");
			warehouse1.WarehouseAddress.OA_Code = "ADDRESS";
			warehouse1.WarehouseAddress.OA_OH = warehouse1OrgProxy.PK;
			var warehouse1TransportBillToCo = Helper.CreateClient("WBT");
			warehouse1OrgProxy.AllRelatedParties.SetRelatedParty(warehouse1TransportBillToCo,
				RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Pickup,
				Constants.TransportModes.All, ZString.Empty);

			var warehouse2OrgProxy = Helper.CreateClient("WP2");
			var warehouse2 = Helper.CreateWarehouse("WHS2");
			warehouse2.WarehouseAddress.OA_Code = "ADDRESS2";
			warehouse2.WarehouseAddress.OA_OH = warehouse2OrgProxy.PK;
			var warehouse2TransportBillToCo = Helper.CreateClient("WB2");
			warehouse2OrgProxy.AllRelatedParties.SetRelatedParty(warehouse2TransportBillToCo,
				RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Pickup,
				Constants.TransportModes.All, ZString.Empty);

			var order = Factory.New<WhsOrder>();

			WarehouseDataRegistry.Instance.TransportCoDefaultingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new TransportCoDefaultingRules() { Warehouse = 1, Client = 2, Consignee = 3 });
			order.WD_WW_Whs = warehouse1.PK;
			order.WD_OH_Client = client.PK;
			AssertEquals(warehouse1TransportBillToCo.PK, order.TransportBillTo.PK);
			Factory.Save();

			var orderOnOtherFactory = (new BusinessObjectFactory()).Load<WhsOrder>(order.PK);
			orderOnOtherFactory.WD_WW_Whs = warehouse2.PK;
			AssertEquals("Should change as old TransportBillToCo was the previous default.",
				warehouse2TransportBillToCo.PK, orderOnOtherFactory.TransportBillTo.PK);
		}

		public void TestSetDefaultTransportBillTo_DoesntRunOnImporting()
		{
			var client1 = Helper.CreateClient("CLT");
			var client1TransportBillToCo = Helper.CreateClient("CBT");
			client1.AllRelatedParties.SetRelatedParty(client1TransportBillToCo,
				RelatedPartyTypeList.Codes.LocalTransportBillTo, RelatedPartyDirectionList.Codes.Pickup,
				Constants.TransportModes.All, ZString.Empty);

			var order = Factory.New<WhsOrder>();
			((ISupportDataImporting)order).IsImportingData = true;
			WarehouseDataRegistry.Instance.TransportCoDefaultingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new TransportCoDefaultingRules() { Warehouse = 2, Client = 1, Consignee = 3 });
			order.WD_OH_Client = client1.PK;
			AssertEquals(null, order.TransportBillTo);
		}

		#endregion

		#region TestSetDefaultDistributionCentre

		public void TestSetDefaultDistributionCentre()
		{
			var consignee = Helper.CreateClient();
			consignee.OH_IsMiscFreightServices = true;
			consignee.OH_IsConsignee = true;
			var consigneeAddress1 = consignee.Addresses.AddNew();
			var consigneeAddress2 = consignee.Addresses.AddNew();
			var consigneeAddress3 = consignee.Addresses.AddNew();

			var distributionCenter1 = CreateDistributionCentre();
			var distributionCenter2 = CreateDistributionCentre();
			consignee.AllRelatedParties.SetRelatedParty(consigneeAddress1.PK, distributionCenter1,
				RelatedPartyTypeList.Codes.NationalDistributionCentre, RelatedPartyDirectionList.Codes.Delivery, "",
				"");
			consignee.AllRelatedParties.SetRelatedParty(consigneeAddress2.PK, distributionCenter2,
				RelatedPartyTypeList.Codes.NationalDistributionCentre, RelatedPartyDirectionList.Codes.Delivery, "",
				"");

			var order = Factory.New<WhsOrder>();
			order.ConsigneeAddressPK = consigneeAddress1.PK;
			AssertEquals("It should update distribution centre.", distributionCenter1.PK,
				order.DistributionCentreDocAddress.Organisation.PK);

			order.ConsigneeAddressPK = consigneeAddress2.PK;
			AssertEquals("It should change Distribution centre.", distributionCenter2.PK,
				order.DistributionCentreDocAddress.Organisation.PK);

			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeAddressPK = consigneeAddress1.PK;
			AssertEquals("Since address is overridden it should not changes distribution centre.",
				distributionCenter2.PK, order.DistributionCentreDocAddress.Organisation.PK);

			order.ConsigneeDocAddress.E2_AddressOverride = false;
			order.ConsigneeAddressPK = consigneeAddress3.PK;
			AssertEquals("Since there is no matching distribution centre it should not change.", distributionCenter2.PK,
				order.DistributionCentreDocAddress.Organisation.PK);

			order.ConsigneeAddressPK = consigneeAddress1.PK;
			AssertEquals(
				"Since previous address doesn't have a distribution centre it should not default distribution centre.",
				distributionCenter2.PK, order.DistributionCentreDocAddress.Organisation.PK);

			order.DistributionCentreDocAddress.OrganisationPK = ZGuid.Empty;
			AssertNull("Precondition", order.DistributionCentreDocAddress.Organisation);
			order.ConsigneeAddressPK = consigneeAddress2.PK;
			AssertEquals("Since there is no distribution centre it should update distribution centre.",
				distributionCenter2.PK, order.DistributionCentreDocAddress.Organisation.PK);
		}

		#region TestSetDefaultDistributionCentre_DoesnotRunOnImporting

		public void TestSetDefaultDistributionCentre_DoesnotRunOnImporting()
		{
			var consignee = Helper.CreateClient("C1");
			var distributionCenter = CreateDistributionCentre();
			consignee.AllRelatedParties.SetRelatedParty(consignee.MainAddress.PK, distributionCenter,
				RelatedPartyTypeList.Codes.NationalDistributionCentre, "", "", "");

			var order = Factory.New<WhsOrder>();
			((ISupportDataImporting)order).IsImportingData = true;
			order.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			AssertNull(order.DistributionCentreDocAddress.Organisation);
		}

		#endregion

		OrgHeader CreateDistributionCentre()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_IsMiscFreightServices = true;
			org.OH_IsDistributionCentre = true;
			return org;
		}

		#endregion

		#region TestSetDefaultDropMode

		public void TestSetDefaultDropMode()
		{
			OrgHeader client = Helper.CreateClient();
			OrgHeader clientTransportCo = Factory.New<OrgHeader>();
			client.AllRelatedParties.SetRelatedParty(clientTransportCo, RelatedPartyTypeList.Codes.LocalTransport,
				RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);

			OrgHeader consignee = Factory.New<OrgHeader>();
			OrgAddress consigneeAddress1 = consignee.Addresses.AddNew();
			OrgAddress consigneeAddress2 = consignee.Addresses.AddNew();

			consigneeAddress1.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandHaulier;
			consigneeAddress2.OA_LCLEquipmentNeeded = Constants.LCLAIREquipmentNeeded.HandUnloadLoad;

			consigneeAddress1.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.LiftOffOn;
			consigneeAddress2.OA_FCLEquipmentNeeded = Constants.FCLEquipmentNeeded.SideLoader;

			WhsOrder order = Factory.New<WhsOrder>();

			// if the order has no container
			order.ConsigneeAddressPK = consigneeAddress1.PK;
			AssertEquals("The drop mode should be 'LCL HandHaulier'", Constants.LCLAIREquipmentNeeded.HandHaulier,
				order.WD_DropMode);

			order.ConsigneeAddressPK = consigneeAddress2.PK;
			AssertEquals("The drop mode should be 'LCL HandUnloadLoad'", Constants.LCLAIREquipmentNeeded.HandUnloadLoad,
				order.WD_DropMode);

			// if the order has container(s)
			order.Containers.AddNew().WC_ContainerNum = "C0001";

			order.ConsigneeAddressPK = consigneeAddress1.PK;
			AssertEquals("The drop mode should be 'FCL LiftOffOn'", Constants.FCLEquipmentNeeded.LiftOffOn,
				order.WD_DropMode);

			order.ConsigneeAddressPK = consigneeAddress2.PK;
			AssertEquals("The drop mode should be 'FCL HandUnloadLoad'", Constants.FCLEquipmentNeeded.SideLoader,
				order.WD_DropMode);
		}

		#endregion

		#endregion

		#region TestPickingInstructions

		public void TestPickingInstructions()
		{
			StmNote pickingInstructionNote = Order.Notes.AddNew();

			pickingInstructionNote.ST_Description = PredefinedNoteTypes.Instance.PickingInstructions.Description;
			pickingInstructionNote.ST_NoteText = "Order note";
			AssertEquals("Picking Instruction is incorrect", "Order note", Order.PickingInstructions);

			pickingInstructionNote.ST_Description = PredefinedNoteTypes.Instance.AgentNotes.Description;
			AssertEquals("Picking Instruction should be empty", "", Order.PickingInstructions);
		}

		#endregion

		#region TestOrderGoodsHandlingInstructions

		public void TestOrderGoodsHandlingInstructions()
		{
			var order = SetupForTestFinaliseDocket();
			Factory.Save();

			var notes = order.OrderGoodsHandlingInstructions;
			AssertEquals("Precondition: No GHI Notes should exist", string.Empty, notes);

			order.Notes.AddNew(false, "Goods Handling Instructions", "Be very very careful with this rabbit.");

			notes = order.OrderGoodsHandlingInstructions;
			AssertEquals("OrderGoodsHandlingInstructions should be correct.", "Be very very careful with this rabbit.",
				notes);
		}

		public void TestOrderGoodsHandlingInstructions_Multiple()
		{
			var order = SetupForTestFinaliseDocket();
			var note1 = order.Notes.AddNew(false, "Goods Handling Instructions",
				"Be very very careful with this rabbit.");
			var note2 = order.Notes.AddNew(false, "Goods Handling Instructions", "Second note.");
			note2.ST_NoteContextDirection = nameof(StmNoteContextDirection.I);
			var note3 = order.Notes.AddNew(false, "Goods Handling Instructions", "Third note.");
			note3.ST_NoteContextDirection = nameof(StmNoteContextDirection.F);
			Factory.Save();

			var notes = order.OrderGoodsHandlingInstructions;
			AssertEquals("OrderGoodsHandlingInstructions should be correct.",
				"Multiple Goods Handling Instructions found.", notes);
		}

		public void TestOrderGoodsHandlingInstructions_NoteOnRelatedJob()
		{
			var order = SetupForTestFinaliseDocket();
			var receive = Factory.New<WhsReceive>();
			order.WD_WD_ParentDocket = receive.PK;
			receive.Notes.AddNew(false, "Goods Handling Instructions", "Be very very careful with this rabbit.");

			var notes = order.OrderGoodsHandlingInstructions;
			AssertEquals("OrderGoodsHandlingInstructions should be empty as the note is on a related job.",
				string.Empty, notes);
		}

		public void TestOrderGoodsHandlingInstructions_NoteOnClient()
		{
			var order = SetupForTestFinaliseDocket();
			order.Client.Notes.AddNew(false, "Goods Handling Instructions", "Organisational Instructions");
			Factory.Save();

			var notes = order.OrderGoodsHandlingInstructions;
			AssertEquals("OrderGoodsHandlingInstructions should be correct.", "Organisational Instructions", notes);
		}

		#endregion

		#region TestGrossWeightSent

		public void TestGrossWeightSent_UnallocatedOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0m, "");

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			AssertEquals(0m, order.GrossWeightSent);
		}

		public void TestGrossWeightSent_AllocatedOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0m, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals(20m, order.GrossWeightSent);

			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 8m; // reduce by 2
			AssertEquals(16m, order.GrossWeightSent);

			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);
			AssertEquals(16m, orderInOtherFactory.GrossWeightSent);
		}

		public void TestGrossWeightSent_PartiallyPackedOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0m, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("CTN");
			package.KP_Weight = 2m;
			AssertEquals("Gross Weight should be unpacked Weight + Package Weight.", 22m, order.GrossWeightSent);

			package.Pack(order.Lines[0].ReleaseLines[0], 6m);
			AssertEquals("Precondition: Package is packed.", 6m, package.GetPackedQty(order.Lines[0].ReleaseLines[0]));
			AssertEquals("Package Weight is correct.", 14m, package.KP_Weight);
			AssertEquals("Gross Weight should be unpacked Weight + Package Weight.", 22m, order.GrossWeightSent);

			package.KP_Weight += 2m;
			AssertEquals("Gross Weight should be unpacked Weight + Package Weight.", 24m, order.GrossWeightSent);

			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);
			AssertEquals(24m, orderInOtherFactory.GrossWeightSent);
		}

		public void TestGrossWeightSent_FullyPackedOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0m, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("CTN");
			package.KP_Weight = 2m;
			AssertEquals("Gross Weight should be unpacked Weight + Package Weight.", 22m, order.GrossWeightSent);

			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			AssertEquals("Precondition: Package is packed.", 10m, package.GetPackedQty(order.Lines[0].ReleaseLines[0]));
			AssertEquals("Package Weight is correct.", 22m, package.KP_Weight);
			AssertEquals("Gross Weight should be unpacked Weight + Package Weight.", 22m, order.GrossWeightSent);

			package.KP_Weight = 14m;
			AssertEquals("Gross Weight should be unpacked Weight + Package Weight.", 14m, order.GrossWeightSent);

			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);
			AssertEquals(14m, orderInOtherFactory.GrossWeightSent);
		}

		public void TestGrossWeightSent_MultipleProductsPackagesAndWeightUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "LB", 0m, "");
			Helper.SetProductWeightAndVolume(data.Part2, 1200m, "G", 0m, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			Helper.CreatePickNew(order);

			var package1 = order.PackageJob.Packages.AddNew("CTN");
			var package2 = order.PackageJob.Packages.AddNew("CTN");
			package1.KP_Weight = 2m;
			package1.KP_WeightUQ = "LB";
			package2.KP_Weight = 3000m;
			package2.KP_WeightUQ = "G";
			AssertEquals("Gross Weight should be unpacked Weight + Package Weight (KG).", 24.979m,
				order.GrossWeightSent);

			package1.Pack(orderLine1.ReleaseLines[0], 3m);
			package2.Pack(orderLine2.ReleaseLines[0], 7m);
			AssertEquals("Precondition: Package is packed.", 3m, package1.GetPackedQty(orderLine1.ReleaseLines[0]));
			AssertEquals("Precondition: Package is packed.", 7m, package2.GetPackedQty(orderLine2.ReleaseLines[0]));
			AssertEquals("Package Weight is correct.", 8m, package1.KP_Weight); // is in pounds
			AssertEquals("Package Weight is correct.", 11400m, package2.KP_Weight); // is in grams
			AssertEquals("Gross Weight should be unpacked Weight + Package Weight (KG).", 24.979m,
				order.GrossWeightSent);

			package1.KP_Weight += 2m;
			AssertEquals("Gross Weight should be unpacked Weight + Package Weight (KG).", 25.886m,
				order.GrossWeightSent);

			package2.KP_Weight += 500m;
			AssertEquals("Gross Weight should be unpacked Weight + Package Weight (KG).", 26.386m,
				order.GrossWeightSent);

			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);
			AssertEquals(26.386m, orderInOtherFactory.GrossWeightSent);
		}

		public void TestGrossWeightSent_InvalidWD_TotalWeightUnit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, Constants.Weight.Kilograms, 0m, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("CTN");
			package.KP_Weight = 2m;
			AssertEquals("Gross Weight should be unpacked Weight + Package Weight.", 22m, order.GrossWeightSent);

			AssertNoExceptionThrown(() =>
			{
				order.WD_TotalWeightUnit = "42";
				package.Pack(order.Lines[0].ReleaseLines[0], 6m);
			});
			AssertEquals("Order GrossWeightSent should be 0.", 0m, order.GrossWeightSent);

			AssertNoExceptionThrown(() =>
			{
				order.WD_TotalWeightUnit = Constants.Weight.Kilograms;
				package.Pack(order.Lines[0].ReleaseLines[0], 6m);
			});
			AssertEquals("Order GrossWeightSent should be 22.", 22m, order.GrossWeightSent);
		}

		#endregion

		#region TestGrossWeightSentInfo

		public void TestGrossWeightSentInfo()
		{
			AssertEquals(true, GetNewBusinessObject().GrossWeightSentInfo.ReadOnly);
		}

		#endregion

		#region TestTareWeight

		public void TestTareWeight()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0m, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("CTN");
			package.KP_Weight = 2m;
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			AssertEquals("Precondition: Package is packed.", 10m, package.GetPackedQty(order.Lines[0].ReleaseLines[0]));
			AssertEquals("Package Weight is correct.", 22m, package.KP_Weight);
			AssertEquals("Gross Weight should be unpacked Weight + Package Weight.", 22m, order.GrossWeightSent);
			AssertEquals("Net Weight should be based on Product Weight only.", 20m, order.WD_WeightSentUserEntered);
			AssertEquals("Tare Weight should be Gross minus Net Weight.", 2m, order.TareWeight);

			package.KP_Weight = 25m;
			AssertEquals("Gross Weight should be unpacked Weight + Package Weight.", 25m, order.GrossWeightSent);
			AssertEquals("Net Weight should be based on Product Weight only.", 20m, order.WD_WeightSentUserEntered);
			AssertEquals("Tare Weight should be Gross minus Net Weight.", 5m, order.TareWeight);

			package.KP_Weight = 14m;
			AssertEquals("Gross Weight should be unpacked Weight + Package Weight.", 14m, order.GrossWeightSent);
			AssertEquals("Net Weight should be based on Product Weight only.", 20m, order.WD_WeightSentUserEntered);
			AssertEquals("Tare Weight should be never go below 0.", 0m, order.TareWeight);
		}

		#endregion

		#region TestPackages_CollectionCountChange_NullPackageJob

		public void TestPackages_CollectionCountChange_NullPackageJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Box)).F3_UOMType =
				UOMPackTypesList.Codes.Case;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Unit)).F3_UOMType =
				UOMPackTypesList.Codes.SplitCase;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Box, 10m);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 50m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.WP_CartoniseSplitCases = true;
			pick.WP_PickCasesByLabel = true;
			pick.WP_PickPalletsByLabel = true;
			Factory.Save();

			var allocateLabelsFactory = new BusinessObjectFactory();
			var allocateLabelPick = allocateLabelsFactory.Load<WhsPick>(pick.PK);

			var removeOrderFactory = new BusinessObjectFactory();
			var removeOrderPick = removeOrderFactory.Load<WhsPick>(pick.PK);
			var removeOrder = removeOrderFactory.Load<WhsOrder>(order.PK);
			removeOrderPick.RemoveOrders(new[] { removeOrder });

			AssertNoExceptionThrown("Null PackageJob should be handled.",
				() => allocateLabelPick.AllocatePackageLabels());
		}

		#endregion

		#region TestUsePackingWeightAndVolume

		public void TestUsePackingWeightAndVolume()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0.5m, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("CTN");
			package.KP_Weight = 2m;
			package.KP_Volume = 6m;
			order.UsePackingWeightAndVolume = true;
			AssertEquals("If UsePackingWeightAndVolume is true, then Weight Sent should be Gross Weight.", 22m,
				order.WD_WeightSent);
			AssertEquals(
				"If UsePackingWeightAndVolume is true, then Volume Sent should be the bigger of Packing Volume or Product Volume.",
				6m, order.WD_CubicSent);

			order.UsePackingWeightAndVolume = false;
			AssertEquals("If UsePackingWeightAndVolume is false, then Weight Sent should be Net Weight.", 20m,
				order.WD_WeightSent);
			AssertEquals("If UsePackingWeightAndVolume is false, then Volume Sent should be Product Volume.", 5m,
				order.WD_CubicSent);

			package.KP_Volume = 4m;
			order.UsePackingWeightAndVolume = true;
			AssertEquals("If UsePackingWeightAndVolume is true, then Weight Sent should be Gross Weight.", 22m,
				order.WD_WeightSent);
			AssertEquals(
				"If UsePackingWeightAndVolume is true, then Volume Sent should be the bigger of Packing Volume or Product Volume.",
				5m, order.WD_CubicSent);
		}

		public void TestUsePackingWeightAndVolume_IsDefaultedToTrueWhenFirstAddingAPackage_AddPalletWeightRegistryOn()
		{
			AssertUsePackingWeightAndVolume_IsDefaultedToTrueWhenFirstAddingAPackage(addPalletWeightRegistryOn: true);
		}

		public void TestUsePackingWeightAndVolume_IsDefaultedToTrueWhenFirstAddingAPackage_AddPalletWeightRegistryOff()
		{
			AssertUsePackingWeightAndVolume_IsDefaultedToTrueWhenFirstAddingAPackage(addPalletWeightRegistryOn: false);
		}

		void AssertUsePackingWeightAndVolume_IsDefaultedToTrueWhenFirstAddingAPackage(bool addPalletWeightRegistryOn)
		{
			using (WarehouseDataRegistry.Instance.AddPalletWeightToOrder.SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, addPalletWeightRegistryOn))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
				Helper.CreatePickNew(order);
				AssertEquals("Should be registry by default.", addPalletWeightRegistryOn,
					order.UsePackingWeightAndVolume);

				var package = order.PackageJob.Packages.AddNew("CTN");
				AssertEquals("Should default to true when Adding a Package.", true, order.UsePackingWeightAndVolume);

				package.Delete();
				AssertEquals("Should default to Registry setting for Adding Pallet Weight on Last Package removal.",
					addPalletWeightRegistryOn, order.UsePackingWeightAndVolume);
				Factory.Save();

				var otherFactory1 = new BusinessObjectFactory();
				var orderInOtherFactory1 = otherFactory1.Load<WhsOrder>(order.PK);

				var newPackage = orderInOtherFactory1.PackageJob.Packages.AddNew("CTN");
				AssertEquals("Should default to true when Adding a Package.", true,
					orderInOtherFactory1.UsePackingWeightAndVolume);
				otherFactory1.Save();

				var otherFactory2 = new BusinessObjectFactory();
				var orderInOtherFactory2 = otherFactory2.Load<WhsOrder>(order.PK);
				_ = orderInOtherFactory2.PackageJob; // hook packagejob events

				var newPackageInOtherFactory = otherFactory2.Load<PkgPackage>(newPackage.PK);
				newPackageInOtherFactory.Delete();
				AssertEquals("Should default to Registry setting for Adding Pallet Weight on Last Package removal.",
					addPalletWeightRegistryOn, orderInOtherFactory2.UsePackingWeightAndVolume);
			}
		}

		public void TestUsePackingWeightAndVolume_IsDefaultedToTrueWhenFirstAddingAPackage_IncludingMassPackageProcess()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);
			AssertEquals("Should be false without any Packages.", false, order.UsePackingWeightAndVolume);

			using (order.PackageJob.InitiateMassPackageProcess())
			{
				order.PackageJob.Packages.AddNew("CTN");
				order.PackageJob.Packages.AddNew("CTN");
				order.PackageJob.Packages.AddNew("CTN");
				AssertEquals("Should not yet be defaulted as Package Process is not finished.", false,
					order.UsePackingWeightAndVolume);
			}

			AssertEquals("Should default to true when after adding Packages after Package Process is finished.", true,
				order.UsePackingWeightAndVolume);
		}

		public void
			TestUsePackingWeightAndVolume_IsDefaultedToRegistryWhenRemovingLastPackage_AddPalletWeightRegistryOn()
		{
			AssertUsePackingWeightAndVolume_IsDefaultedToRegistryWhenRemovingLastPackage(
				addPalletWeightRegistryOn: true);
		}

		public void
			TestUsePackingWeightAndVolume_IsDefaultedToRegistryWhenRemovingLastPackage_AddPalletWeightRegistryOff()
		{
			AssertUsePackingWeightAndVolume_IsDefaultedToRegistryWhenRemovingLastPackage(
				addPalletWeightRegistryOn: false);
		}

		void AssertUsePackingWeightAndVolume_IsDefaultedToRegistryWhenRemovingLastPackage(
			bool addPalletWeightRegistryOn)
		{
			using (WarehouseDataRegistry.Instance.AddPalletWeightToOrder.SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, addPalletWeightRegistryOn))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
				Helper.CreatePickNew(order);
				AssertEquals("Should be registry by default.", addPalletWeightRegistryOn,
					order.UsePackingWeightAndVolume);

				var package1 = order.PackageJob.Packages.AddNew("CTN");
				var package2 = order.PackageJob.Packages.AddNew("CTN");
				AssertEquals("Should default to true when Adding a Package.", true, order.UsePackingWeightAndVolume);

				package2.Delete();
				AssertEquals("Have not removed last package, nothing should change.", true,
					order.UsePackingWeightAndVolume);

				package1.Delete();
				AssertEquals("Should default to Registry Setting for Add Pallet Weight after removing last Package.",
					addPalletWeightRegistryOn, order.UsePackingWeightAndVolume);

				var package3 = order.PackageJob.Packages.AddNew("CTN");
				AssertEquals("Should default to true when Adding a Package.", true, order.UsePackingWeightAndVolume);

				order.UsePackingWeightAndVolume = false;
				package3.Delete();
				AssertEquals("Should default to Registry Setting for Add Pallet Weight after removing last Package.",
					addPalletWeightRegistryOn, order.UsePackingWeightAndVolume);

				var package4 = order.PackageJob.Packages.AddNew("CTN");
				AssertEquals("Should default to true when Adding a Package.", true, order.UsePackingWeightAndVolume);
				Factory.Save();

				var otherFactory = new BusinessObjectFactory();
				var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);
				var packageInOtherFactory = otherFactory.Load<PkgPackage>(package4.PK);

				package4.Delete();
				AssertEquals("Should default to Registry Setting for Add Pallet Weight after removing last Package.",
					addPalletWeightRegistryOn, order.UsePackingWeightAndVolume);
			}
		}

		public void
			TestUsePackingWeightAndVolume_IsDefaultedToRegistryWhenRemovingLastPackage_IncludingMassPackageProcess_AddPalletWeightRegistryOn()
		{
			AssertUsePackingWeightAndVolume_IsDefaultedToRegistryWhenRemovingLastPackage_IncludingMassPackageProcess(
				addPalletWeightRegistryOn: true);
		}

		public void
			TestUsePackingWeightAndVolume_IsDefaultedToRegistryWhenRemovingLastPackage_IncludingMassPackageProcess_AddPalletWeightRegistryOff()
		{
			AssertUsePackingWeightAndVolume_IsDefaultedToRegistryWhenRemovingLastPackage_IncludingMassPackageProcess(
				addPalletWeightRegistryOn: false);
		}

		void AssertUsePackingWeightAndVolume_IsDefaultedToRegistryWhenRemovingLastPackage_IncludingMassPackageProcess(
			bool addPalletWeightRegistryOn)
		{
			using (WarehouseDataRegistry.Instance.AddPalletWeightToOrder.SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, addPalletWeightRegistryOn))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
				Helper.CreatePickNew(order);
				AssertEquals("Should be registry by default.", addPalletWeightRegistryOn,
					order.UsePackingWeightAndVolume);

				order.PackageJob.Packages.AddNew("CTN");
				order.PackageJob.Packages.AddNew("CTN");
				order.PackageJob.Packages.AddNew("CTN");
				AssertEquals("Should default to true when Adding a Package.", true, order.UsePackingWeightAndVolume);

				using (order.PackageJob.InitiateMassPackageProcess())
				{
					order.PackageJob.Packages.DeleteAll();
					AssertEquals("Should not yet be changed as Package Process is not finished.", true,
						order.UsePackingWeightAndVolume);
				}

				AssertEquals(
					"Should default to Registry Setting for Add Pallet Weight after removing Packages after Package Process is finished.",
					addPalletWeightRegistryOn, order.UsePackingWeightAndVolume);
			}
		}

		public void TestUsePackingWeightAndVolume_DefaultingDoesNotOccurWhenAddingAdditionalPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0.5m, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);
			AssertEquals("Should be false without any Packages.", false, order.UsePackingWeightAndVolume);

			order.PackageJob.Packages.AddNew("CTN");
			AssertEquals("Should default to true when Adding a Package.", true, order.UsePackingWeightAndVolume);

			order.UsePackingWeightAndVolume = false;
			order.PackageJob.Packages.AddNew("CTN");
			AssertEquals("No defaulting should occur.", false, order.UsePackingWeightAndVolume);

			Factory.Save();
			var otherFactory = new BusinessObjectFactory();
			var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);
			orderInOtherFactory.PackageJob.Packages.AddNew("CTN");
			AssertEquals("No defaulting should occur.", false, order.UsePackingWeightAndVolume);
		}

		public void TestUsePackingWeightAndVolume_IsNotChangedWhenPackageDetailsAreChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0.5m, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);
			AssertEquals("Should be false without any Packages.", false, order.UsePackingWeightAndVolume);

			var package = order.PackageJob.Packages.AddNew("CTN");
			AssertEquals("Should default to true when Adding a Package.", true, order.UsePackingWeightAndVolume);

			order.UsePackingWeightAndVolume = false;
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);
			var packageInOtherFactory = otherFactory.Load<PkgPackage>(package.PK);
			packageInOtherFactory.KP_PackageQty = 4;
			AssertEquals("Should be no defaulting.", false, orderInOtherFactory.UsePackingWeightAndVolume);
		}

		#endregion

		#region TestUsePackingWeightAndVolumeInfo

		public void TestUsePackingWeightAndVolumeInfo()
		{
			TestNonStandardReadOnly1(d => d.UsePackingWeightAndVolumeInfo);
		}

		#endregion

		#region TestWD_WeightSentUserEntered_WD_WeightSent

		public void TestWD_WeightSentUserEntered_WD_WeightSent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			AssertEquals("Precondition", 0m, order.WD_WeightSentUserEntered);
			AssertEquals("Precondition", 0m, order.WD_WeightSent);
			AssertEquals("Precondition", (short)0, order.WD_PalletsSent);
			AssertEquals("Precondition", false, order.WD_AddPalletWeightToOrder);
			AssertEquals("Precondition", "KG", order.WD_TotalWeightUnit);

			var packtype = Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT"));
			packtype.F3_Weight = 15m;
			packtype.F3_UnitOfWeight = "KG";
			order.WD_PalletsSent = 2;
			order.WD_AddPalletWeightToOrder = true;

			// 2 pallets with each 15KG
			order.WD_WeightSentUserEntered = 100m;
			AssertEquals(130m, order.WD_WeightSent);

			order.WD_WeightSentUserEntered = 20m;
			AssertEquals(50m, order.WD_WeightSent);

			using (order.SetIsDetachingToPick())
			{
				order.WD_WeightSent = 20m;
				order.WD_WeightSentUserEntered = 20m;
				AssertEquals(20m, order.WD_WeightSent);
			}
		}

		public void TestWD_WeightSentUserEntered_UpdateWeightSentWhenHasPackagesIfUsePackingWeightIsFalse()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0m, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("CTN");
			AssertEquals(20m, order.WD_WeightSentUserEntered);
			AssertEquals(20m, order.WD_WeightSent);

			order.WD_WeightSentUserEntered = 10m;
			AssertEquals(10m, order.WD_WeightSentUserEntered);
			AssertEquals("Weight Sent should be unchanged.", 20m, order.WD_WeightSent);

			order.UsePackingWeightAndVolume = false;
			AssertEquals(10m, order.WD_WeightSentUserEntered);
			AssertEquals("Weight Sent should be updated to Net Weight.", 10m, order.WD_WeightSent);

			order.WD_WeightSentUserEntered = 15m;
			AssertEquals(15m, order.WD_WeightSentUserEntered);
			AssertEquals("Weight Sent should get updated now.", 15m, order.WD_WeightSent);
		}

		#endregion

		#region TestWD_WeightSentUserEnteredInfo

		public void TestWD_WeightSentUserEnteredInfo()
		{
			TestNonStandardReadOnly1(d => d.WD_WeightSentUserEnteredInfo);
		}

		#endregion

		#region TestWD_AddPalletWeightToOrder_WD_WeightSent

		public void TestWD_AddPalletWeightToOrder_WD_WeightSent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			AssertEquals("Precondition", 0m, order.WD_WeightSentUserEntered);
			AssertEquals("Precondition", 0m, order.WD_WeightSent);
			AssertEquals("Precondition", (short)0, order.WD_PalletsSent);
			AssertEquals("Precondition", false, order.WD_AddPalletWeightToOrder);

			var packtype = Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT"));
			packtype.F3_Weight = 15m;
			packtype.F3_UnitOfWeight = "KG";
			order.WD_PalletsSent = 2;
			order.WD_WeightSentUserEntered = 100m;
			AssertEquals(100m, order.WD_WeightSent);

			//CheckBox Checked Vs Unchecked (2 pallets with each 15KG, WD_WeightSentUserEntered=100m)
			order.WD_AddPalletWeightToOrder = true;
			AssertEquals(100m, order.WD_WeightSentUserEntered);
			AssertEquals(130m, order.WD_WeightSent);

			order.WD_AddPalletWeightToOrder = false;
			AssertEquals(100m, order.WD_WeightSentUserEntered);
			AssertEquals(100m, order.WD_WeightSent);

			using (order.SetIsDetachingToPick())
			{
				order.WD_AddPalletWeightToOrder = true;
				AssertEquals(100m, order.WD_WeightSent);
			}
		}

		#endregion

		#region TestWD_AddPalletWeightToOrder_WithPackagesDoesNotUpdateWeightSent

		public void TestWD_AddPalletWeightToOrder_WithPackagesDoesNotUpdateWeightSent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Weight = 0m;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			AssertEquals("Precondition", 0m, order.WD_WeightSentUserEntered);
			AssertEquals("Precondition", 0m, order.WD_WeightSent);
			AssertEquals("Precondition", (short)0, order.WD_PalletsSent);
			AssertEquals("Precondition", false, order.WD_AddPalletWeightToOrder);

			order.PackageJob.Packages.AddNew("CTN");
			var packtype = Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT"));
			packtype.F3_Weight = 15m;
			packtype.F3_UnitOfWeight = "KG";
			AssertEquals("Precondition: Add Pallet Weight Flag should be true.", true, order.WD_AddPalletWeightToOrder);
			AssertEquals("Precondition", 0m, order.WD_WeightSentUserEntered);
			AssertEquals("Precondition", 0m, order.WD_WeightSent);

			order.WD_AddPalletWeightToOrder = false;
			AssertEquals(0m, order.WD_WeightSentUserEntered);
			AssertEquals(0m, order.WD_WeightSent);

			order.WD_AddPalletWeightToOrder = true;
			AssertEquals(
				"Updating AddPalletWeightToOrder should not have updated Weight Sent as the Order has packages.", 0m,
				order.WD_WeightSentUserEntered);
			AssertEquals(
				"Updating AddPalletWeightToOrder should not have updated Weight Sent as the Order has packages.", 0m,
				order.WD_WeightSent);
		}

		#endregion

		#region TestWD_AddPalletWeightToOrderInfo

		public void TestWD_AddPalletWeightToOrderInfo()
		{
			TestNonStandardReadOnly1(d => d.WD_AddPalletWeightToOrderInfo);
		}

		#endregion

		#region TestWD_CubicSent

		public void TestWD_CubicSent_UnallocatedOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 0m, "", 0.5m, "M3");

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			AssertEquals(0m, order.WD_CubicSent);
		}

		public void TestWD_CubicSent_AllocatedOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 0m, "", 0.5m, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals(5m, order.WD_CubicSent);

			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 8m; // reduce by 2
			AssertEquals(4m, order.WD_CubicSent);
		}

		public void TestWD_CubicSent_WithPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 0m, "", 0.5m, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("CTN");
			package.KP_Volume = 2m;
			AssertEquals("Volume Sent should be greater of Package Volume or Product Volume.", 5m, order.WD_CubicSent);

			package.KP_Volume = 6m;
			AssertEquals("Volume Sent should be greater of Package Volume or Product Volume.", 6m, order.WD_CubicSent);

			Factory.Save();
			var otherFactory1 = new BusinessObjectFactory();
			var orderInOtherFactory1 = otherFactory1.Load<WhsOrder>(order.PK);
			orderInOtherFactory1.UsePackingWeightAndVolume = false;
			AssertEquals("Volume Sent should be Product Volume.", 5m, orderInOtherFactory1.WD_CubicSent);

			Factory.Save();
			var otherFactory2 = new BusinessObjectFactory();
			var orderInOtherFactory2 = otherFactory2.Load<WhsOrder>(order.PK);
			orderInOtherFactory2.UsePackingWeightAndVolume = true;
			AssertEquals("Volume Sent should be greater of Package Volume or Product Volume.", 6m,
				orderInOtherFactory2.WD_CubicSent);
		}

		public void TestWD_CubicSent_MultipleProductsPackagesAndVolumeUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 0m, "", 4m, "CF");
			Helper.SetProductWeightAndVolume(data.Part2, 0m, "", 12000m, "CC");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			Helper.CreatePickNew(order);

			var package1 = order.PackageJob.Packages.AddNew("CTN");
			var package2 = order.PackageJob.Packages.AddNew("CTN");
			package1.KP_Volume = 49m;
			package1.KP_VolumeUQ = "CF";
			package2.KP_Volume = 150000m;
			package2.KP_VolumeUQ = "CC";
			AssertEquals("Volume Sent should be greater of Package Volume or Product Volume (M3).", 1.538m,
				order.WD_CubicSent);

			order.UsePackingWeightAndVolume = false;
			AssertEquals("Volume Sent should be Product Volume (M3).", 1.253m, order.WD_CubicSent);

			package2.KP_Volume = 250000m;
			AssertEquals("Volume Sent should be Product Volume (M3).", 1.253m, order.WD_CubicSent);

			order.UsePackingWeightAndVolume = true;
			AssertEquals("Volume Sent should be greater of Package Volume or Product Volume (M3).", 1.638m,
				order.WD_CubicSent);
		}

		public void TestWD_CubicSent_MultiplePackagesAndVolumeUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			Helper.CreatePickNew(order);

			var package1 = order.PackageJob.Packages.AddNew("CTN");
			var package2 = order.PackageJob.Packages.AddNew("CTN");
			package1.KP_Volume = 2m;
			package1.KP_VolumeUQ = "CF";
			package2.KP_Volume = 3000m;
			package2.KP_VolumeUQ = "D3";
			AssertEquals("Volume Sent should be Package Volume (M3).", 3.057m, order.WD_CubicSent);
		}

		public void TestWD_CubicSent_InvalidWD_TotalCubicUnit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, Constants.Weight.Kilograms, 2m,
				Constants.Volume.CubicMetres);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew("CTN");
			package.KP_Volume = 2m;
			AssertEquals("WD_CubicSent should be correct", 20m, order.WD_CubicSent);

			AssertNoExceptionThrown(() =>
			{
				order.WD_TotalCubicUnit = "42";
				package.Pack(order.Lines[0].ReleaseLines[0], 6m);
			});
			AssertEquals("Order GrossWeightSent should be 0.", 0m, order.WD_CubicSent);

			AssertNoExceptionThrown(() =>
			{
				order.WD_TotalCubicUnit = Constants.Volume.CubicMetres;
				package.Pack(order.Lines[0].ReleaseLines[0], 6m);
			});
			AssertEquals("Order GrossWeightSent should be 20.", 20m, order.WD_CubicSent);
		}

		#endregion

		#region TestWD_CubicSentInfo

		public void TestWD_CubicSentInfo()
		{
			TestNonStandardReadOnly1(d => d.WD_CubicSentInfo);
		}

		public void TestWD_CubicSentInfo_WithPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);
			AssertEquals(false, order.WD_CubicSentInfo.ReadOnly);

			order.PackageJob.Packages.AddNew();
			AssertEquals(true, order.WD_CubicSentInfo.ReadOnly);
		}

		#endregion

		#region TestWD_PalletSent_WD_WeightSent

		public void TestWD_PalletSent_WD_WeightSent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			AssertEquals("Precondition", 0m, order.WD_WeightSentUserEntered);
			AssertEquals("Precondition", 0m, order.WD_WeightSent);
			AssertEquals("Precondition", (short)0, order.WD_PalletsSent);
			AssertEquals("Precondition", false, order.WD_AddPalletWeightToOrder);
			var packtype = Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT"));
			packtype.F3_Weight = 15m;
			packtype.F3_UnitOfWeight = "KG";
			order.WD_AddPalletWeightToOrder = true;
			order.WD_WeightSentUserEntered = 50m;

			//Change Pallet quantity (each pallet is 15KG, WD_WeightSentUserEntered=50m)
			order.WD_PalletsSent = 2;
			AssertEquals(80m, order.WD_WeightSent);

			order.WD_PalletsSent = 0;
			AssertEquals(50m, order.WD_WeightSentUserEntered);
			AssertEquals(50m, order.WD_WeightSent);

			using (order.SetIsDetachingToPick())
			{
				order.WD_PalletsSent = 2;
				AssertEquals(50m, order.WD_WeightSent);
			}
		}

		#endregion

		#region TestWD_PalletSent_WithPackagesDoesNotUpdateWeightSent

		public void TestWD_PalletSent_WithPackagesDoesNotUpdateWeightSent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Weight = 0m;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			AssertEquals("Precondition", 0m, order.WD_WeightSentUserEntered);
			AssertEquals("Precondition", 0m, order.WD_WeightSent);
			AssertEquals("Precondition", (short)0, order.WD_PalletsSent);
			AssertEquals("Precondition", false, order.WD_AddPalletWeightToOrder);

			order.PackageJob.Packages.AddNew("CTN");
			var packtype = Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT"));
			packtype.F3_Weight = 15m;
			packtype.F3_UnitOfWeight = "KG";
			AssertEquals("Precondition: Add Pallet Weight Flag should be true.", true, order.WD_AddPalletWeightToOrder);
			AssertEquals("Precondition", 0m, order.WD_WeightSentUserEntered);
			AssertEquals("Precondition", 0m, order.WD_WeightSent);

			order.WD_PalletsSent = 2;
			AssertEquals("Updating Pallets Sent should not have updated Weight Sent as the Order has packages.", 0m,
				order.WD_WeightSentUserEntered);
			AssertEquals("Updating Pallets Sent should not have updated Weight Sent as the Order has packages.", 0m,
				order.WD_WeightSent);
		}

		#endregion

		#region TestWD_PalletsSentInfo

		public void TestWD_PalletsSentInfo()
		{
			TestNonStandardReadOnly1(d => d.WD_PalletsSentInfo);
		}

		#endregion

		#region TestRefPackType_WD_WeightSent

		public void TestRefPackType_WD_WeightSent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			AssertEquals("Precondition", 0m, order.WD_WeightSentUserEntered);
			AssertEquals("Precondition", 0m, order.WD_WeightSent);
			AssertEquals("Precondition", (short)0, order.WD_PalletsSent);
			AssertEquals("Precondition", false, order.WD_AddPalletWeightToOrder);

			var packtype = Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT"));
			packtype.F3_UnitOfWeight = "KG";
			order.WD_AddPalletWeightToOrder = true;
			order.WD_PalletsSent = 2;

			//Change each Pallet weight (2 pallets, WD_WeightSentUserEntered=80m)
			packtype.F3_Weight = 15m;
			order.WD_WeightSentUserEntered = 80m;
			AssertEquals(110m, order.WD_WeightSent);

			packtype.F3_Weight = 2000m;
			order.WD_WeightSentUserEntered = 80m;
			AssertEquals(4080m, order.WD_WeightSent);

			//Change pallet weight Unit
			packtype.F3_UnitOfWeight = "G";
			order.WD_WeightSentUserEntered = 80m;
			AssertEquals(84m, order.WD_WeightSent);

			packtype.F3_UnitOfWeight = "LB";
			order.WD_WeightSentUserEntered = 80m;
			AssertEquals(1894.37m, ZArchitecture.Core.Utilities.Round(order.WD_WeightSent, 2));
		}

		#endregion

		#region TestRequiredDate

		protected override void TestRequiredDateCore()
		{
			var year = ZDateTime.Now.Year;

			var date1 = new ZDateTime(year, 3, 19, 7, 30, 0);
			var date1Offset = new ZDateTimeOffset(year, 3, 19, 7, 30, 0, TimeSpan.FromHours(10));
			var date2 = new ZDateTime(year, 5, 6, 9, 5, 0);
			var date2Offset = new ZDateTimeOffset(year, 5, 6, 9, 5, 0, TimeSpan.FromHours(10));

			var branch = Helper.CreateGlbBranch("222");
			branch.GB_RL_NKHomePort = "AUBNE";
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_GB_RelatedCompanyBranch = branch.PK;
			Factory.Save();

			// the DB does not save seconds, ensure we do not use seconds when pushing the req by date to the end of the day
			var date1_EndOfDay = new ZDateTime(year, 3, 19, 23, 59, 0);
			var date1_EndOfDayOffset = new ZDateTimeOffset(year, 3, 19, 23, 59, 0, TimeSpan.FromHours(10));

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			order.RequiredDate = ZDateTime.Empty;
			AssertEquals("Precondition - RequiredDate should be empty.", true, order.RequiredDate.IsEmpty);
			AssertEquals("Precondition - WD_RequiredDate should be empty.", true, order.WD_RequiredDate.IsEmpty);

			order.RequiredDate = date1;
			AssertEquals("RequiredDate should be end of day.", date1_EndOfDay, order.RequiredDate);
			AssertEquals("WD_RequiredDate should be end of day.", date1_EndOfDayOffset, order.WD_RequiredDate);

			order.RequiredDate = ZDateTime.Empty;
			AssertEquals("Clearing the RequiredDate should *not* set time to the end of the day.", ZDateTime.Empty,
				order.RequiredDate);
			AssertEquals("Clearing the RequiredDate should *not* set time to the end of the day.", ZDateTimeOffset.Empty,
				order.WD_RequiredDate);

			order.RequiredDate = date1;
			order.RequiredDate = date2;
			AssertEquals("Overwriting a non-empty date should *not* set the time to the end of the day.", date2,
				order.RequiredDate);
			AssertEquals("Overwriting a non-empty date should *not* set the time to the end of the day but should at offset for WD_RequiredDate.", date2Offset,
				order.WD_RequiredDate);

			order.RequiredDate = ZDateTime.Empty;
			((ISupportDataImporting)order).IsImportingData = true;
			order.RequiredDate = date1;
			AssertEquals("Date should not be set to end of day during a data import.", date1, order.RequiredDate);
			AssertEquals("Date should not be set to end of day during a data import.", date1Offset, order.WD_RequiredDate);
		}

		#endregion

		#region TestWD_WeightSentInfo

		public void TestWD_WeightSentInfo()
		{
			AssertEquals(true, GetNewBusinessObject().WD_WeightSentInfo.ReadOnly);
		}

		#endregion

		#region TestWD_WP_CreatesOrDeletesPackageJob

		public void TestWD_WP_CreatesOrDeletesPackageJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// receive stock
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			// create an order
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			AssertNull("Precondition - PackageJob should not exist.",
				Factory.LoadFromUniqueKey<PkgPackageJob>(PkgPackageJobSchema.KJ_ParentID, order.PK));

			// pick the order - ensure package job is created/saved
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - Pick should be created.", true, order.IsAttachedToPickButNotFinalised);

			Factory.Save(); // prev. empty PackageJobs had HasChanges = false. Load in a new factory to ensure this behaviour has been removed.
			AssertNotNull(
				"The PackageJob should have been created on Picking to make it available to the Packing module.",
				new BusinessObjectFactory().LoadFromUniqueKey<PkgPackageJob>(PkgPackageJobSchema.KJ_ParentID,
					order.PK));

			// pack the picked items
			order.PackageJob.Packages.AddNew("PLT").Pack(order.Lines[0].ReleaseLines[0], 5m);

			// cancel the pick - ensure package job (and therefore the packed contents/divots) is deleted
			pick.CancelPick();
			AssertEquals("Precondition - Order should *not* be Picked.", false, order.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition - Pick should be Cancelled.", true, pick.IsCancelled);
			AssertNull("The PackageJob should have been deleted on Cancel Pick.", order.PackageJob);
		}

		#endregion

		#region TestAuditStatus

		public void TestAuditStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			AssertEquals("Since the QualityAuditRequired is false it should be NotRequired.",
				OrderAuditStatus.Descriptions.NotRequired, order.AuditStatus);

			order.WD_QualityAuditRequired = true;
			AssertEquals("Since there are no packages but audit expected, status must be Pending.",
				OrderAuditStatus.Descriptions.Pending, order.AuditStatus);

			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TestPackage1";
			order.WD_QualityAuditRequired = false;
			AssertEquals(
				"Since the QualityAuditRequired is false, and there are not packages with failures it should be NotRequired.",
				OrderAuditStatus.Descriptions.NotRequired, order.AuditStatus);

			order.WD_QualityAuditRequired = true;
			AssertEquals(
				"Since the QualityAuditRequired is true, and there is a package without audit it should be PendingAQualityAudit.",
				OrderAuditStatus.Descriptions.Pending, order.AuditStatus);

			var audit = WhsPackageAuditManager.AuditPackage(package);
			AssertEquals(
				"Since the QualityAuditRequired is true, and there is a package with passed audit it should be AuditPassed.",
				OrderAuditStatus.Descriptions.Passed, order.AuditStatus);

			var failure = Factory.New<WhsPackageAuditLineFailure>();
			audit.PackageAuditFailureLines.Add(failure);
			AssertEquals(
				"Since the QualityAuditRequired is true, and there is a package with failing audit it should be AuditFailed.",
				OrderAuditStatus.Descriptions.Failed, order.AuditStatus);

			order.PackageJob.Packages.AddNew().KP_PackageID = "TestPackage2";
			AssertEquals(
				"Since the QualityAuditRequired is true, and there is a package with failing audit but another without audit it should be PendingAQualityAudit.",
				OrderAuditStatus.Descriptions.Pending, order.AuditStatus);
		}

		public void TestAuditStatus_NoAuditRequiredAndPackageFailed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "TestPackage1";
			order.WD_QualityAuditRequired = false;

			var audit = WhsPackageAuditManager.AuditPackage(package);
			var failure = Factory.New<WhsPackageAuditLineFailure>();
			audit.PackageAuditFailureLines.Add(failure);

			AssertEquals("For Quality Audit not required and package with failures, expected status is Failed.",
				OrderAuditStatus.Descriptions.Failed, order.AuditStatus);

			order.PackageJob.Packages.AddNew().KP_PackageID = "TestPackage2";
			AssertEquals(
				"For Quality Audit not required one package with failure and other without failure, expected status is Failed.",
				OrderAuditStatus.Descriptions.Failed, order.AuditStatus);
		}

		#endregion

		#region TestWD_IsAuthorisedToLeave

		public void TestWD_IsAuthorisedToLeave()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			AssertEquals("WD_IsAuthorisedToLeave: default to be false.", false, order.WD_IsAuthorisedToLeave);

			var consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			consignee1.MainAddress.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;

			var consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			consignee2.MainAddress.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.NO;

			order.ConsigneePK = consignee1.PK;
			AssertNotNull(order.ConsigneeDocAddress);
			AssertEquals("Precondition: JobDocAddress.E2_AddressOverride.", false,
				order.ConsigneeDocAddress.E2_AddressOverride);
			AssertEquals("WD_IsAuthorisedToLeave", true, order.WD_IsAuthorisedToLeave);

			order.ConsigneeDocAddress.E2_AddressOverride = true;
			AssertEquals("WD_IsAuthorisedToLeave", false, order.WD_IsAuthorisedToLeave);

			order.ConsigneeDocAddress.E2_AddressOverride = false;
			AssertEquals("WD_IsAuthorisedToLeave", true, order.WD_IsAuthorisedToLeave);

			order.ConsigneePK = consignee2.PK;
			AssertNotNull(order.ConsigneeDocAddress);
			AssertEquals("Precondition: JobDocAddress.E2_AddressOverride.", false,
				order.ConsigneeDocAddress.E2_AddressOverride);
			AssertEquals("WD_IsAuthorisedToLeave", false, order.WD_IsAuthorisedToLeave);

			order.WD_IsAuthorisedToLeave = true;
			AssertEquals("WD_IsAuthorisedToLeave", true, order.WD_IsAuthorisedToLeave);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var oderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			AssertEquals("WD_IsAuthorisedToLeave", true, oderInNewFactory.WD_IsAuthorisedToLeave);
		}

		public void TestWD_IsAuthorisedToLeave_WhenClientChanges()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var client2 = Helper.CreateClient("2");
			AssertEquals("WD_IsAuthorisedToLeave: default is false.", false, order.WD_IsAuthorisedToLeave);

			var consignee = Helper.CreateClient("CONSIGNEE");
			var supplierLink = consignee.SupplierLinks.AddNew();
			supplierLink.OL_OH_Supplier = client2.PK;
			supplierLink.OL_AuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;
			consignee.MainAddress.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.DEF;

			order.ConsigneePK = consignee.PK;
			AssertNotNull(order.ConsigneeDocAddress);
			AssertEquals("Precondition: JobDocAddress.E2_AddressOverride.", false,
				order.ConsigneeDocAddress.E2_AddressOverride);
			AssertEquals("ATL should be false as the client does not have ATL set up.", false,
				order.WD_IsAuthorisedToLeave);

			order.WD_OH_Client = client2.PK;
			AssertEquals("ATL should be true as the client does have ATL set up.", true, order.WD_IsAuthorisedToLeave);

			order.WD_OH_Client = data.Org1.PK;
			AssertEquals("ATL should be false as the client does not have ATL set up.", false,
				order.WD_IsAuthorisedToLeave);
		}

		public void TestWD_IsAuthorisedToLeave_DoesNotChangeIfOverriddenAddressIsEdited()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			AssertEquals("Precondition: ATL is false.", false, order.WD_IsAuthorisedToLeave);

			order.WD_IsAuthorisedToLeave = true;
			AssertEquals("Precondition: Consignee is not overridden.", false,
				order.ConsigneeDocAddress.E2_AddressOverride);

			order.ConsigneeDocAddress.E2_AddressOverride = true;
			AssertEquals("ATL should become false when address is overridden.", false, order.WD_IsAuthorisedToLeave);

			order.WD_IsAuthorisedToLeave = true;
			order.ConsigneeDocAddress.E2_Address1 = "123 Street";
			AssertEquals("Editing an already overridden address should not untick ATL.", true,
				order.WD_IsAuthorisedToLeave);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			AssertEquals("Precondition: ATL is true.", true, orderInNewFactory.WD_IsAuthorisedToLeave);

			orderInNewFactory.ConsigneeDocAddress.E2_Address1 = "456 Street";
			AssertEquals("Editing an already overridden address should not untick ATL.", true,
				orderInNewFactory.WD_IsAuthorisedToLeave);

			orderInNewFactory.ConsigneeDocAddress.E2_AddressOverride = false;
			AssertEquals("Address is no longer overridden, ATL should become false.", false,
				orderInNewFactory.WD_IsAuthorisedToLeave);
		}

		public void TestWD_IsAuthorisedToLeave_DoesChangeWhenAddressChanges()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			AssertEquals("Precondition: ATL is false.", false, order.WD_IsAuthorisedToLeave);

			order.WD_IsAuthorisedToLeave = true;
			AssertEquals("Precondition: Consignee is not overridden.", false,
				order.ConsigneeDocAddress.E2_AddressOverride);

			order.ConsigneeDocAddress.OrganisationPK = ZGuid.Empty;
			AssertEquals("ATL should become false when address is changed.", false, order.WD_IsAuthorisedToLeave);

			order.WD_IsAuthorisedToLeave = true;
			order.ConsigneeDocAddress.E2_OA_Address = data.Org1.MainAddress.PK;
			AssertEquals("ATL should become false when address is changed.", false, order.WD_IsAuthorisedToLeave);

			order.WD_IsAuthorisedToLeave = true;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			AssertEquals("Precondition: ATL is true.", true, orderInNewFactory.WD_IsAuthorisedToLeave);

			// emulate Form opening
			orderInNewFactory.ConsigneeDocAddress.ValidationStatus = AddressValidationStatus.NotRequired;
			AssertEquals("ATL should remain true.", true, orderInNewFactory.WD_IsAuthorisedToLeave);
		}

		#endregion

		#region TestWD_IsAuthorisedToLeave_ReadOnly

		public void TestWD_IsAuthorisedToLeave_ReadOnly()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			AssertEquals("ReadOnly should be false when pick is not finalised.", false,
				order.WD_IsAuthorisedToLeaveInfo.ReadOnly);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Pre-condition: Pick is not finalised.", false, pick.IsFinalised);
			AssertEquals("ReadOnly should be false when pick is not finalised.", false,
				order.WD_IsAuthorisedToLeaveInfo.ReadOnly);

			pick.FinaliseAllOrders();
			pick.FinalisePick();

			AssertIsFinalisedPrecondition(pick);
			AssertEquals("ReadOnly should be true when pick is finalised.", true,
				order.WD_IsAuthorisedToLeaveInfo.ReadOnly);
		}

		#endregion

		#region TestWD_PackingAfterPickingRequired

		public void TestWD_PackingAfterPickingRequired_ReadOnly()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Pre-condition: Pick is not finalised.", false, pick.IsFinalised);
			AssertEquals("ReadOnly should be false when pick is not finalised.", false,
				order.WD_PackingAfterPickingRequiredInfo.ReadOnly);

			pick.FinaliseAllOrders();
			pick.FinalisePick();

			AssertIsFinalisedPrecondition(pick);
			AssertEquals("ReadOnly should be true when pick is finalised.", true,
				order.WD_PackingAfterPickingRequiredInfo.ReadOnly);
		}

		#endregion

		#region TestTransportZone

		public override void TestTransportZone()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var rateTransportProvider = Helper.SetUpRateTransportProvider(true, "OPS", "AU", "ALL", data.Org1);

			SetupMainOrgAddress(data.Org1, "20 Maxwell Street", "", "PERTH", "6162", "WA", "AUBYW");
			var address2 = Helper.SetUpOrgAddress("12 Alexandria Street", "Alexandria", "2015", "SYDNEY", "NSW",
				"AUALX", data.Org1);
			var rateTransportZonePerth = Helper.SetUpRateTransportZone(rateTransportProvider, true, "Perth");
			Helper.SetUpRateTransportZoneItem(rateTransportZonePerth, "AU", "6162");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			order.TransportCoPK = data.Org1.PK;
			AssertEquals("WD_TZ_TransportZone", rateTransportZonePerth.PK, order.WD_TZ_TransportZone);
			AssertEquals("TransportZone", rateTransportZonePerth, order.TransportZone);

			var rateTransportZoneSydney = Helper.SetUpRateTransportZone(rateTransportProvider, true, "Sydney");
			Helper.SetUpRateTransportZoneItem(rateTransportZoneSydney, "AU", "2015");
			Factory.Save();

			order.ConsigneeAddressPK = address2.PK;
			AssertEquals("WD_TZ_TransportZone", order.WD_TZ_TransportZone, rateTransportZoneSydney.PK);
			AssertEquals("TransportZone", rateTransportZoneSydney, order.TransportZone);
		}

		public void TestTransportZone_ZoneUpdatedWhenTransportCoChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var org2 = OrgHeader.New(Factory);
			org2.OH_Code = "ORG2";
			org2.OH_Category = "NGO";
			org2.OH_ScreeningStatus = "CLR";
			var rateTransportProvider1 = Helper.SetUpRateTransportProvider(true, "OPS", "AU", "ALL", data.Org1);
			var rateTransportProvider2 = Helper.SetUpRateTransportProvider(true, "OPS", "AU", "ALL", org2);

			SetupMainOrgAddress(data.Org1, "20 Maxwell Street", "", "PERTH", "6162", "WA", "AUBYW");
			var rateTransportZoneDianella = Helper.SetUpRateTransportZone(rateTransportProvider1, true, "Dianella");
			Helper.SetUpRateTransportZoneItem(rateTransportZoneDianella, "AU", "6162");
			var rateTransportZoneMalaga = Helper.SetUpRateTransportZone(rateTransportProvider2, true, "Malaga");
			Helper.SetUpRateTransportZoneItem(rateTransportZoneMalaga, "AU", "6162");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			order.TransportCoPK = data.Org1.PK;
			AssertEquals("WD_TZ_TransportZone", rateTransportZoneDianella.PK, order.WD_TZ_TransportZone);

			order.TransportCoPK = org2.PK;
			AssertEquals("WD_TZ_TransportZone should change when TransportCo is changed", rateTransportZoneMalaga.PK,
				order.WD_TZ_TransportZone);
		}

		public void TestTransportZone_ZoneRemovedWhenTransportCoCleared()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var rateTransportProvider = Helper.SetUpRateTransportProvider(true, "OPS", "AU", "ALL", data.Org1);
			SetupMainOrgAddress(data.Org1, "20 Maxwell Street", "", "PERTH", "6162", "WA", "AUBYW");
			var rateTransportZonePerth = Helper.SetUpRateTransportZone(rateTransportProvider, true, "Perth");
			Helper.SetUpRateTransportZoneItem(rateTransportZonePerth, "AU", "6162");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			order.TransportCoPK = data.Org1.PK;
			AssertEquals("WD_TZ_TransportZone", rateTransportZonePerth.PK, order.WD_TZ_TransportZone);

			order.TransportCoPK = ZGuid.Empty;
			AssertEquals("WD_TZ_TransportZone should be empty", ZGuid.Empty, order.WD_TZ_TransportZone);
		}

		public void TestTransportZone_ZoneRemovedWhenConsigneeCleared()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var rateTransportProvider = Helper.SetUpRateTransportProvider(true, "OPS", "AU", "ALL", data.Org1);
			SetupMainOrgAddress(data.Org1, "20 Maxwell Street", "", "PERTH", "6162", "WA", "AUBYW");
			var rateTransportZonePerth = Helper.SetUpRateTransportZone(rateTransportProvider, true, "Perth");
			Helper.SetUpRateTransportZoneItem(rateTransportZonePerth, "AU", "6162");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			order.TransportCoPK = data.Org1.PK;
			AssertEquals("WD_TZ_TransportZone", rateTransportZonePerth.PK, order.WD_TZ_TransportZone);

			order.ConsigneePK = ZGuid.Empty;
			AssertEquals("WD_TZ_TransportZone should be empty", ZGuid.Empty, order.WD_TZ_TransportZone);
		}

		public override void TestTransportZoneName()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var rateTransportProvider = Helper.SetUpRateTransportProvider(true, "OPS", "AU", "ALL", data.Org1);

			SetupMainOrgAddress(data.Org1, "20 Maxwell Street", "", "PERTH", "6162", "WA", "AUBYW");
			var address2 = Helper.SetUpOrgAddress("12 Alexandria Street", "Alexandria", "2015", "SYDNEY", "NSW",
				"AUALX", data.Org1);
			var rateTransportZonePerth = Helper.SetUpRateTransportZone(rateTransportProvider, true, "Perth");
			Helper.SetUpRateTransportZoneItem(rateTransportZonePerth, "AU", "6162");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			order.TransportCoPK = data.Org1.PK;
			AssertEquals("TransportZoneName", "Perth", order.TransportZoneName);

			var rateTransportZoneSydney = Helper.SetUpRateTransportZone(rateTransportProvider, true, "Sydney");
			Helper.SetUpRateTransportZoneItem(rateTransportZoneSydney, "AU", "2015");
			Factory.Save();

			order.ConsigneeAddressPK = address2.PK;
			AssertEquals("TransportZoneName", "Sydney", order.TransportZoneName);

			// Address is not save in order, so still someone can to delete the address
			var newFactory = new BusinessObjectFactory();
			newFactory.Load<JobDocAddress>(order.ConsigneeDocAddress.PK).Delete();
			newFactory.Save();

			AssertNoExceptionThrown(() => order.TransportCoPK = ZGuid.Empty);
		}

		public void TestTransportZone_ZoneUpdatedWithTransportCo_ConsigneeDocAddressNotInitialised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var rateTransportProvider = Helper.SetUpRateTransportProvider(true, "OPS", "AU", "ALL", data.Org1);

			SetupMainOrgAddress(data.Org1, "20 Maxwell Street", "", "PERTH", "6162", "WA", "AUBYW");
			Helper.SetUpOrgAddress("12 Alexandria Street", "Alexandria", "2015", "SYDNEY", "NSW", "AUALX", data.Org1);
			var rateTransportZonePerth = Helper.SetUpRateTransportZone(rateTransportProvider, true, "Perth");
			Helper.SetUpRateTransportZoneItem(rateTransportZonePerth, "AU", "6162");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			AssertEquals("Precondition: Order has no transport zone.", ZGuid.Empty, order.WD_TZ_TransportZone);
			AssertEquals("Precondition: Order has no transport zone.", ZString.Empty, order.TransportZoneName);
			AssertNotNull("Order has consignee doc address.", order.ConsigneeDocAddress);
			AssertEquals("Order has consignee doc address.", false, order.ConsigneeDocAddress.IsEmpty);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			AssertEquals("Precondition: Order has no transport zone.", ZGuid.Empty,
				orderInNewFactory.WD_TZ_TransportZone);
			AssertNull("Precondition: Order has no transport zone.", orderInNewFactory.TransportZone);

			orderInNewFactory.TransportCoPK = data.Org1.PK;
			AssertEquals("Order has TransportZone", rateTransportZonePerth.PK, orderInNewFactory.WD_TZ_TransportZone);
			AssertEquals("Order has TransportZone", "Perth", orderInNewFactory.TransportZoneName);
			AssertEquals("Order has TransportZone", "Perth", ((IWhsOrder)orderInNewFactory).TransportZoneName);
		}

		#endregion

		#region Flags

		#region TestIsDomesticFreight

		public override void TestIsDomesticFreight()
		{
			var docket = GetNewBusinessObject();
			AssertEquals(true, docket.IsDomesticFreight);

			var org = Helper.CreateClient("XZA");
			docket.ConsigneePK = org.PK;
			org.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";
			AssertEquals(true, docket.IsDomesticFreight);

			var warehouse = Helper.CreateWarehouse("XXX");
			docket.WD_WW_Whs = warehouse.PK;
			AssertEquals(false, docket.IsDomesticFreight);

			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "USCHI";
			AssertEquals(true, docket.IsDomesticFreight);
		}

		#endregion

		#region TestIsSavedFromOrderForm

		public void TestIsSavedFromOrderForm()
		{
			var order = GetNewBusinessObject();
			AssertEquals(false, order.IsSavedFromOrderForm);

			order.IsSavedFromOrderForm = true;
			AssertEquals(true, order.IsSavedFromOrderForm);
		}

		#endregion

		#region TestHasReservedStock

		public void TestHasReservedStock()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Factory.Save();

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			AssertEquals("Precondition: Not in database", false, orderLine.IsInDatabase);
			AssertEquals("Order has no reserved stock", false, order.HasReservedStock);

			orderLine.ReserveStockIfAbleTo(receive.Inventory[0], 0m);
			AssertEquals("Order has no reserved stock", false, order.HasReservedStock);

			Factory.Save();

			AssertEquals("Precondition: In database", true, orderLine.IsInDatabase);
			AssertEquals("Order has no reserved stock", false, order.HasReservedStock);

			orderLine.ReserveStockIfAbleTo(receive.Inventory[0], 10m);
			AssertEquals("Order has reserved stock", true, order.HasReservedStock);

			orderLine.ReservedPickLines.DeleteAll();
			AssertEquals("Order has no reserved stock", false, order.HasReservedStock);
		}

		#endregion

		#region TestAreLinesUpdateAllowedAfterPick

		public void TestAreLinesUpdateAllowedAfterPick_RegistryEnabled()
		{
			TestAreLinesUpdateAllowedAfterPick_Core(true);
		}

		public void TestAreLinesUpdateAllowedAfterPick_RegistryDisabled()
		{
			TestAreLinesUpdateAllowedAfterPick_Core(false);
		}

		void TestAreLinesUpdateAllowedAfterPick_Core(bool isRegistryEnabled)
		{
			using (WarehouseDataRegistry.Instance.PreventOrderLinesUpdateWhenOrderIsInPicking.SetTemporaryValue(
					   Guid.Empty, Guid.Empty, Guid.Empty, isRegistryEnabled))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				Helper.CreatePickNew(order);

				AssertEquals("Precondition: order is picking.", true, order.IsAttachedToPickButNotFinalised);
				AssertEquals("AreLinesUpdateDisabledAfterPick is true when picking and registry is enabled.",
					isRegistryEnabled, order.AreLinesUpdateDisabledAfterPick);
			}
		}

		public void TestAreLinesUpdateAllowedAfterPick_NotPicking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			AssertEquals("Precondition: order is not picking.", false, order.IsAttachedToPickButNotFinalised);
			AssertEquals("AreLinesUpdateDisabledAfterPick is false.", false, order.AreLinesUpdateDisabledAfterPick);
		}

		#endregion

		#region TestIsSalesChannelAllowed

		protected override bool ExpectedIsSalesChannelAllowed => true;

		#endregion

		#region TestIsCancelled

		public void TestIsCancelled_CustomsOrder_ImportingForChangeOfInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bondedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			bondedOrder.WD_DocketSubType = OrderType.Codes.Customs;
			AssertEquals("Precondition", false, bondedOrder.IsCancelled);

			bondedOrder.Logs.AddNew(Events.Cancelled);
			AssertEquals("Order is not cancelled.", false, bondedOrder.IsCancelled);

			using (bondedOrder.MarkAsImportingForChangeOfInventory())
			{
				AssertEquals("Order is cancelled.", true, bondedOrder.IsCancelled);
			}
		}

		#endregion

		#region IsLoadingOrLoadedOrDeparted

		public void TestIsLoadingOrLoadedOrDeparted_Loading()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = order.PackageJob.Packages.AddNew();
			var package2 = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			Factory.Save();

			AssertEquals("Order Status is correct.", WhsOrderStatus.Codes.Loading, order.WarehouseOrderStatus);
			AssertEquals("IsLoadingOrLoadedOrDeparted is correct.", true, order.IsLoadingOrLoadedOrDeparted);
		}

		public void TestIsLoadingOrLoadedOrDeparted_Loaded()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Order Status is correct.", WhsOrderStatus.Codes.Loaded, order.WarehouseOrderStatus);
			AssertEquals("IsLoadingOrLoadedOrDeparted is correct.", true, order.IsLoadingOrLoadedOrDeparted);
		}

		public void TestIsLoadingOrLoadedOrDeparted_Departed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			Helper.DepartPackageNow(loadPkgPackagePivot1);
			Factory.Save();

			AssertEquals("Order Status is correct.", WhsOrderStatus.Codes.Departed, order.WarehouseOrderStatus);
			AssertEquals("IsLoadingOrLoadedOrDeparted is correct.", true, order.IsLoadingOrLoadedOrDeparted);
		}

		public void TestIsLoadingOrLoadedOrDeparted_Entered()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			AssertEquals("Order Status is correct.", DocketStatus.Codes.Entered, order.WarehouseOrderStatus);
			AssertEquals("IsLoadingOrLoadedOrDeparted is correct.", false, order.IsLoadingOrLoadedOrDeparted);
		}

		public void TestIsLoadingOrLoadedOrDeparted_AttachedToPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();
			var pick = Helper.CreatePickNew(order);

			AssertEquals("Order Status is correct.", DocketStatus.Codes.AttachedToPick, order.WarehouseOrderStatus);
			AssertEquals("IsLoadingOrLoadedOrDeparted is correct.", false, order.IsLoadingOrLoadedOrDeparted);
		}

		#endregion

		#endregion

		#region Property Infos

		public void TestIsOrderHeldInfo()
		{
			TestStandardReadOnly(d => d.IsOrderHeldInfo);
		}

		public void TestWD_IsOrderSelectedForFinalisationInfo()
		{
			var order = GetNewBusinessObject();
			AssertEquals(WhsOrder.Schema.WD_IsOrderSelectedForFinalisation,
				order.WD_IsOrderSelectedForFinalisationInfo.Name);
			TestNonStandardReadOnly1(d => d.WD_IsOrderSelectedForFinalisationInfo);
		}

		public void TestWD_CalcCrossDockCurrentVolumeIncludingCrossDockAllocationsInfo()
			=> AssertEquals(WhsOrder.Schema.WD_CalcCrossDockCurrentVolumeIncludingCrossDockAllocations,
					GetNewBusinessObject().WD_CalcCrossDockCurrentVolumeIncludingCrossDockAllocationsInfo.Name);

		public void TestWD_CalcCrossDockAvailableVolumeIncludingCrossDockAllocationsInfo()
			=> AssertEquals(WhsOrder.Schema.WD_CalcCrossDockAvailableVolumeIncludingCrossDockAllocations,
					GetNewBusinessObject().WD_CalcCrossDockAvailableVolumeIncludingCrossDockAllocationsInfo.Name);

		#region TestWD_DocketSubType_ReadOnly

		public void TestWD_DocketSubType_ReadOnly()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.MiscServ.OM_WhsGenerateBackOrdersOnShortfalls = true;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 300m);
			AssertEquals("DocketSubType should be readonly if it's an Order with any lines.", true,
				order.WD_DocketSubTypeInfo.ReadOnly);

			var backorder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "BO1", data.Part1, 200m);
			backorder.WD_DocketSubType = OrderType.Codes.BackOrder;
			backorder.WD_WD_ParentDocket = order.PK;
			AssertEquals("DocketSubType should be readonly if it's a Back Order.", true,
				backorder.WD_DocketSubTypeInfo.ReadOnly);

			order.Lines.DeleteAll();
			backorder.Lines.DeleteAll();
			AssertEquals("DocketSubType should not be readonly if it's an Order with empty lines.", false,
				order.WD_DocketSubTypeInfo.ReadOnly);
			AssertEquals("DocketSubType should be readonly if it's a Back Order.", true,
				backorder.WD_DocketSubTypeInfo.ReadOnly);
		}

		#endregion

		#region TestWD_WW_WhsInfoCore

		protected override void TestWD_WW_WhsInfoCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			AssertEquals("Warehouse is editable when Order has no Pick", false, order.WD_WW_WhsInfo.ReadOnly);

			Helper.CreatePickNew(order);
			AssertEquals("Warehouse is not editable when Order has Pick", true, order.WD_WW_WhsInfo.ReadOnly);
		}

		#endregion

		#endregion

		#region TestWD_WSH_SalesChannelInfo

		public void TestWD_WSH_SalesChannelInfo()
		{
			TestNonStandardReadOnly1(d => d.WD_WSH_SalesChannelInfo);
		}

		#endregion

		#region TestSalesChannelCode

		protected override void TestSalesChannelCodeCore()
		{
			var salesChannel = Factory.New<WhsSalesChannel>();
			salesChannel.WSH_Code = "TER";
			var order = GetNewBusinessObject();
			order.WD_WSH_SalesChannel = salesChannel.PK;
			AssertEquals("TER", order.SalesChannelCode);
		}

		#endregion

		#region TestWD_TotalUnitsFromLines_WhenContainsPickedOnSale

		public void TestWD_TotalUnitsFromLines_WhenContainsPickedOnSale()
		{
			var data = new TestDataSimpleEnvironment(Factory, 10, 10);
			var mainPart = data.Part1;
			var subPart1 = Helper.CreateProduct(data.Org1, "SubPart1");
			var subPart2 = Helper.CreateProduct(data.Org1, "SubPart2");
			Helper.CreateProductBOM(mainPart, subPart1, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(mainPart, subPart2, 1m, Constants.PkgUnit.Unit);
			mainPart.OP_IsComponentPickedOnSalesOrder = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", subPart1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", subPart2, 10m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, mainPart, 1m);
			Helper.CreateWhsOrderLine(order, subPart1, 1m);
			Factory.Save();

			AssertEquals("Precondition: WD_TotalUnitsFromLines should be 2, 1 for each order line", 2m,
				order.WD_TotalUnitsFromLines);
			AssertEquals("Precondition: main part should have 0 component lines before Pick is created", 0,
				orderLine1.ChildComponentLines.Count);

			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItemsWithMock();

			AssertEquals("Component lines created after pick", 2, orderLine1.ChildComponentLines.Count);
			AssertEquals("WD_TotalUnitsFromLines should still be 2, component lines not counted", 2m,
				order.WD_TotalUnitsFromLines);
		}

		#endregion

		#region TestWarehouseOrderStatus

		public void TestWarehouseOrderStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Factory.Save();
			AssertEquals("WarehouseOrderStatus is ENT", DocketStatus.Codes.Entered, order.WarehouseOrderStatus);
			AssertEquals("WarehouseOrderStatusDescription is correct", DocketStatus.Descriptions.Entered, order.WarehouseOrderStatusDescription);
			AssertEquals("WD_DocketStatusDescription is correct", DocketStatus.Descriptions.Entered, order.WD_DocketStatusDescription);

			Helper.CreatePickNew(order);
			var orderATP = new BusinessObjectFactory() { RefreshEnabled = false }.LoadTop1<WhsOrder>(new ZQuery(WhsDocketSchema.PK, order.PK));
			AssertEquals("WarehouseOrderStatus is ATP", DocketStatus.Codes.AttachedToPick, orderATP.WarehouseOrderStatus);
			AssertEquals("WarehouseOrderStatusDescription is correct", DocketStatus.Descriptions.AttachedToPick, orderATP.WarehouseOrderStatusDescription);
			AssertEquals("WD_DocketStatusDescription is correct", DocketStatus.Descriptions.AttachedToPick, orderATP.WD_DocketStatusDescription);

			var pickLine = order.Lines.Single().PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var orderSTA = new BusinessObjectFactory() { RefreshEnabled = false }.LoadTop1<WhsOrder>(new ZQuery(WhsDocketSchema.PK, order.PK));
			AssertEquals("WarehouseOrderStatus is STA", WhsOrderStatus.Codes.Staged, orderSTA.WarehouseOrderStatus);
			AssertEquals("WarehouseOrderStatusDescription is correct", WhsOrderStatus.Descriptions.Staged, orderSTA.WarehouseOrderStatusDescription);
			AssertEquals("WD_DocketStatusDescription is correct", WhsOrderStatus.Descriptions.Staged, orderSTA.WD_DocketStatusDescription);

			var package = order.PackageJob.Packages.AddNew("CTN");
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			var loadPkgPackagePivot = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			loadPkgPackagePivot.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot.WLP_GS_NKLoadingUser = "E";

			Factory.Save();
			var orderLOA = new BusinessObjectFactory() { RefreshEnabled = false }.LoadTop1<WhsOrder>(new ZQuery(WhsDocketSchema.PK, order.PK));
			AssertEquals("WarehouseOrderStatus is LOA", WhsOrderStatus.Codes.Loaded, orderLOA.WarehouseOrderStatus);
			AssertEquals("WarehouseOrderStatusDescription is correct", WhsOrderStatus.Descriptions.Loaded, orderLOA.WarehouseOrderStatusDescription);
			AssertEquals("WD_DocketStatusDescription is correct", WhsOrderStatus.Descriptions.Loaded, orderLOA.WD_DocketStatusDescription);

			Helper.DepartPackageNow(loadPkgPackagePivot);
			Factory.Save();
			var orderDEP = new BusinessObjectFactory() { RefreshEnabled = false }.LoadTop1<WhsOrder>(new ZQuery(WhsDocketSchema.PK, order.PK));
			AssertEquals("WarehouseOrderStatus is DEP", WhsOrderStatus.Codes.Departed, orderDEP.WarehouseOrderStatus);
			AssertEquals("WarehouseOrderStatusDescription is correct", WhsOrderStatus.Descriptions.Departed, orderDEP.WarehouseOrderStatusDescription);
			AssertEquals("WD_DocketStatusDescription is correct", WhsOrderStatus.Descriptions.Departed, orderDEP.WD_DocketStatusDescription);
		}

		public void TestWarehouseOrderStatus_IsCached()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			AssertEquals("WarehouseOrderStatus is NEW", DocketStatus.Codes.New, order.WarehouseOrderStatus);

			Helper.CreatePickNew(order);
			var pickLine = order.Lines.Single().PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("WarehouseOrderStatus is STA", WhsOrderStatus.Codes.Staged, order.WarehouseOrderStatus);
			AssertEquals("WarehouseOrderStatusDescription is correct", WhsOrderStatus.Descriptions.Staged, order.WarehouseOrderStatusDescription);
			AssertEquals("WD_DocketStatusDescription is correct", WhsOrderStatus.Descriptions.Staged, order.WD_DocketStatusDescription);

			var package = order.PackageJob.Packages.AddNew("CTN");
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			var loadPkgPackagePivot = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			loadPkgPackagePivot.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot.WLP_GS_NKLoadingUser = "E";

			Factory.Save();
			AssertEquals("WarehouseOrderStatus is STA", WhsOrderStatus.Codes.Staged, order.WarehouseOrderStatus);
			AssertEquals("WarehouseOrderStatusDescription is correct", WhsOrderStatus.Descriptions.Staged, order.WarehouseOrderStatusDescription);
			AssertEquals("WD_DocketStatusDescription is correct", WhsOrderStatus.Descriptions.Staged, order.WD_DocketStatusDescription);

			var orderATP = new BusinessObjectFactory() { RefreshEnabled = false }.LoadTop1<WhsOrder>(new ZQuery(WhsDocketSchema.PK, order.PK));
			AssertEquals("WarehouseOrderStatus is LOA", WhsOrderStatus.Codes.Loaded, orderATP.WarehouseOrderStatus);
			AssertEquals("WarehouseOrderStatusDescription is correct", WhsOrderStatus.Descriptions.Loaded, orderATP.WarehouseOrderStatusDescription);
			AssertEquals("WD_DocketStatusDescription is correct", WhsOrderStatus.Descriptions.Loaded, orderATP.WD_DocketStatusDescription);
		}

		public void TestWarehouseOrderStatus_ReturnedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Factory.Save();

			Helper.CreatePickNew(order);
			var pickLine = order.Lines.Single().PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var orderSTA = new BusinessObjectFactory() { RefreshEnabled = false }.LoadTop1<WhsOrder>(new ZQuery(WhsDocketSchema.PK, order.PK));
			AssertEquals("WarehouseOrderStatus is STA", WhsOrderStatus.Codes.Staged, orderSTA.WarehouseOrderStatus);
			AssertEquals("WarehouseOrderStatusDescription is correct", WhsOrderStatus.Descriptions.Staged, orderSTA.WarehouseOrderStatusDescription);
			AssertEquals("WD_DocketStatusDescription is correct", WhsOrderStatus.Descriptions.Staged, orderSTA.WD_DocketStatusDescription);

			var orderLine = (WhsOrderLine)orderSTA.Lines.Single();
			var releaseLine = (WhsReleaseLine)orderLine.ReleaseLines.Single();
			ReleaseLineReductionManager.ReduceStock(releaseLine, 1m,
				ObjectFactory.Get<IPickedStockAdjustersFactory>(), ReduceStockReason.Returned);

			Factory.Save();

			var orderSTA2 = new BusinessObjectFactory() { RefreshEnabled = false }.LoadTop1<WhsOrder>(new ZQuery(WhsDocketSchema.PK, order.PK));
			AssertEquals("WarehouseOrderStatus is still STA", WhsOrderStatus.Codes.Staged, orderSTA2.WarehouseOrderStatus);
			AssertEquals("WarehouseOrderStatusDescription is correct", WhsOrderStatus.Descriptions.Staged, orderSTA2.WarehouseOrderStatusDescription);
			AssertEquals("WD_DocketStatusDescription is correct", WhsOrderStatus.Descriptions.Staged, orderSTA2.WD_DocketStatusDescription);
		}

		#endregion

		#region TestWD_DocketStatusDescription

		protected override void TestWD_DocketStatusDescriptionCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			Factory.Save();

			Helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines.Single().PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;

			var pickLine2 = order2.Lines.Single().PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			var package1 = order1.PackageJob.Packages.AddNew("CTN");
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";

			var package2 = order2.PackageJob.Packages.AddNew("CTN");
			package2.Pack(order2.Lines[0].ReleaseLines[0], 2m);
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			loadPkgPackagePivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot2.WLP_GS_NKLoadingUser = "E";

			var package3 = order2.PackageJob.Packages.AddNew("CTN");
			package3.Pack(order2.Lines[0].ReleaseLines[0], 3m);
			Factory.Save();

			AssertEquals(WhsOrderStatus.Descriptions.Loaded, order1.WD_DocketStatusDescription);
			AssertEquals(WhsOrderStatus.Descriptions.Loading, order2.WD_DocketStatusDescription);
		}

		#endregion

		#region TestWD_ExcludeFromTotePicking

		public void TestWD_ExcludeFromTotePicking_ReadOnly()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			Factory.Save();

			Assert("Pre-condition: order is not picked.", order.WD_WP.IsEmpty);
			AssertEquals("ReadOnly should be false when order is not picked.", false,
				order.WD_ExcludeFromTotePickingInfo.ReadOnly);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Pre-condition: Pick is not finalised.", false, pick.IsFinalised);
			AssertEquals("ReadOnly should be false when pick is not finalised.", false,
				order.WD_ExcludeFromTotePickingInfo.ReadOnly);

			pick.FinaliseAllOrders();
			pick.FinalisePick();

			AssertIsFinalisedPrecondition(pick);
			AssertEquals("ReadOnly should be true when pick is finalised.", true,
				order.WD_ExcludeFromTotePickingInfo.ReadOnly);
		}

		#endregion

		#region TestWD_UseDirectedPackingConsolidation

		public void TestWD_UseDirectedPackingConsolidationReadOnly()
		{
			TestReadOnly(d => d.WD_UseDirectedPackingConsolidationInfo, false, false, true, true);
		}

		public void TestWD_UseDirectedPackingConsolidationReadOnly_PickTaskPlanningStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertNullOrEmpty("Default task planning status of pick should be empty.", pick.WP_TaskPlanningStatus);
			AssertEquals("WD_UseDirectedPackingConsolidation should not be readonly for default.", false, order.WD_UseDirectedPackingConsolidationInfo.ReadOnly);

			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			AssertEquals("Task planning status of pick should be changed to ready for planning", TaskPlanningStatus.Codes.Ready, pick.WP_TaskPlanningStatus);
			AssertEquals("WD_UseDirectedPackingConsolidation should be readonly when pick is ready for planning.", true, order.WD_UseDirectedPackingConsolidationInfo.ReadOnly);

			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.NotReady;
			AssertEquals("Task planning status of pick should be changed to not ready for planning", TaskPlanningStatus.Codes.NotReady, pick.WP_TaskPlanningStatus);
			AssertEquals("WD_UseDirectedPackingConsolidation should not be readonly when pick is not ready for planning.", false, order.WD_UseDirectedPackingConsolidationInfo.ReadOnly);

			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			AssertEquals("Task planning status of pick should be changed to planned", TaskPlanningStatus.Codes.Planned, pick.WP_TaskPlanningStatus);
			AssertEquals("WD_UseDirectedPackingConsolidation should be readonly when pick is planned.", true, order.WD_UseDirectedPackingConsolidationInfo.ReadOnly);
		}

		#endregion

		#endregion

		#region TestAutoPackAndPrintAllLabels

		public void TestAutoPackAndPrintAllLabels()
		{
			AssertAutoPackAndPrintAllLabels(isPrinting: true);
		}

		public void TestAutoPackAndPrintAllLabels_WithoutPrinting()
		{
			AssertAutoPackAndPrintAllLabels(isPrinting: false);
		}

		void AssertAutoPackAndPrintAllLabels(bool isPrinting)
		{
			if (Globals.IsWeb)
			{
				Assert(true);
			}
			else
			{
				// setup data
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "x", data.Part1, 500);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "y", data.Part2, 500);
				Helper.CreateProductUnit(data.Part1, "PLT", 4);
				Helper.CreateProductUnit(data.Part2, "PLT", 10);

				// setup the printer
				Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocumentForTesting, GlbStaff.CurrentUser,
					Factory.New<IStmPrintQueue>().PK, 1);
				Factory.Save();

				// setup orders and pick
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
				Helper.CreateWhsOrderLine(order, data.Part1, 2m).WE_F3_NKPackType = "PLT";
				Helper.CreateWhsOrderLine(order, data.Part2, 2m).WE_F3_NKPackType = "PLT";
				var pick = Helper.CreatePickNew(order);
				pick.AutoAllocateItems();

				// test
				AssertEquals("Precondition", 0, order.PackageJob.Packages.Count);
				AssertEquals("Precondition", 0, Factory.Load<IStmPrintJob>(new ZQuery()).Length);

				var autoPackSuccess = order.AutoPackAndPrintAllLabels(Notify, disablePrinting: !isPrinting);
				AssertEquals("Auto Pack should have succeeded.", true, autoPackSuccess);

				var printJobs = Factory.Load<IStmPrintJob>(new ZQuery());
				AssertAutoPackedItems(order.PackageJob, data);

				if (isPrinting)
				{
					AssertEquals("There should be a print job for all 4 packages.", 4, printJobs.Length);
					foreach (StmPrintJob job in printJobs)
					{
						AssertEquals("The job's parent should match the packagejob.", order.PackageJob.PK,
							job.SP_ParentGuid);
					}

					AssertEquals("Packages should be saved to the DB.", true, order.PackageJob.Packages.All(p => p.IsInDatabase));
				}
				else
				{
					AssertEquals("There should be no print jobs when printing is disabled.", 0, printJobs.Length);
					AssertEquals("Packages should not be saved to the DB as printing is disabled, auto-saving is not necessary.", true, order.PackageJob.Packages.All(p => !p.IsInDatabase));
				}
			}
		}

		public void TestAutoPackAndPrintAllLabels_WithNoPackageJob()
		{
			// setup data
			var data = new TestDataSimpleEnvironment(Factory);

			// setup order and pick
			var orderWithNoPackageJob = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var line = orderWithNoPackageJob.Lines.AddNew();
			line.WE_TransactionQuantity = 1;
			var pick = Helper.CreatePickNew(orderWithNoPackageJob);
			orderWithNoPackageJob.WD_WP = pick.PK;
			orderWithNoPackageJob.WD_DocketID = "BLAH";
			orderWithNoPackageJob.PackageJob.Delete(); // this should not happen in production

			// test
			var autoPackSuccess = orderWithNoPackageJob.AutoPackAndPrintAllLabels(Notify, disablePrinting: false);
			AssertEquals("Auto Pack should have failed.", false, autoPackSuccess);
			AssertEquals(@"Order BLAH does not have a Packing Job.
To rectify, open the Release, click on the Packing tab and then click Save.
This will create the necessary Packing information.
", Notify.AsString);
		}

		public void TestAutoPackAndPrintAllLabels_WithNoPick()
		{
			// setup data
			var data = new TestDataSimpleEnvironment(Factory);

			// setup order
			var orderWithNoPick = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			orderWithNoPick.WD_DocketID = "BLAH";

			var autoPackSuccess = orderWithNoPick.AutoPackAndPrintAllLabels(Notify, disablePrinting: false);
			AssertEquals("Auto Pack should have failed.", false, autoPackSuccess);
			AssertEquals("Order BLAH cannot be Auto-Packed as it has not been Picked.\r\n", Notify.AsString);
		}

		public void TestAutoPackAndPrintAllLabels_AlreadyPacked()
		{
			// setup order
			var data = new TestDataSimpleEnvironment(Factory);
			var orderWithPreExistingPacks = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var line = orderWithPreExistingPacks.Lines.AddNew();
			line.WE_TransactionQuantity = 1;

			var pick = Helper.CreatePickNew(orderWithPreExistingPacks);
			orderWithPreExistingPacks.WD_WP = pick.PK;
			orderWithPreExistingPacks.WD_DocketID = "BLAH";
			orderWithPreExistingPacks.PackageJob.Packages.AddNew();
			AssertEquals("Precondition", 1, orderWithPreExistingPacks.PackageJob.Packages.Count);

			var autoPackSuccess = orderWithPreExistingPacks.AutoPackAndPrintAllLabels(Notify, disablePrinting: false);
			AssertEquals("Auto Pack should have failed.", false, autoPackSuccess);
			AssertEquals("Order BLAH cannot be Auto-Packed as it already has Packages.\r\n", Notify.AsString);
		}

		public void TestAutoPackAndPrintAllLabels_PickIsAlreadyFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			order.WD_DocketID = "BLAH";

			var pick = Helper.CreatePickNew(order);
			AssertNotNull("Precondition", order.PackageJob);

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);

			var autoPackSuccess = order.AutoPackAndPrintAllLabels(Notify, disablePrinting: false);
			AssertEquals("Auto Pack should have failed.", false, autoPackSuccess);
			AssertEquals("Order BLAH cannot be Auto-Packed as it has a read only Packing Job.\r\n", Notify.AsString);
		}

		public void TestAutoPackAndPrintAllLabels_ConsigneeDisabledAutoPack()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var orderWithAutoPackDisabled =
				Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			orderWithAutoPackDisabled.Consignee.MiscServ.OM_IsAutoPackAllowed = false;
			Helper.CreatePickNew(orderWithAutoPackDisabled);

			var autoPackSuccess = orderWithAutoPackDisabled.AutoPackAndPrintAllLabels(Notify, disablePrinting: false);
			AssertEquals("Auto Pack should have failed.", false, autoPackSuccess);
			AssertEquals("Order W00000002 cannot be Auto-Packed as it is not enabled on Consignee 111.\r\n",
				Notify.AsString);
		}

		public void TestAutoPackAndPrintAllLabels_PickIsReadyForPlanningDisabledAutoPack()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			var autoPackSuccess = order.AutoPackAndPrintAllLabels(Notify, disablePrinting: false);
			AssertEquals("Auto Pack should have failed.", false, autoPackSuccess);
			AssertEquals("Order W00000002 cannot be Auto-Packed as it has a read only Packing Job.\r\n",
				Notify.AsString);
		}

		public void TestAutoPackAndPrintAllLabels_PickIsPlannedDisabledAutoPack()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;

			var autoPackSuccess = order.AutoPackAndPrintAllLabels(Notify, disablePrinting: false);
			AssertEquals("Auto Pack should have failed.", false, autoPackSuccess);
			AssertEquals("Order W00000002 cannot be Auto-Packed as it has a read only Packing Job.\r\n",
				Notify.AsString);
		}

		public void TestAutoPackAndPrintAllLabels_NoPackableItems()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			Helper.CreateWhsOrderLine(order, data.Part2, 100m);
			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItems();
			Factory.Save();

			var autoPackSuccess = order.AutoPackAndPrintAllLabels(Notify, disablePrinting: false);
			AssertEquals("Auto Pack should have failed.", false, autoPackSuccess);
			AssertEquals(
				string.Format("Order {0} cannot be Auto-Packed as it has no Packable Items caused by a shortfall.\r\n",
					order.WD_DocketID), Notify.AsString);
		}

		public void TestAutoPackAndPrintAllLabels_UnitConversionFailure()
		{
			// setup data
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "x", data.Part1, 500);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "y", data.Part2, 500);
			Helper.CreateProductUnit(data.Part1, "PLT", 4);
			Helper.CreateProductUnit(data.Part2, "PLT", 10);

			// setup the printer
			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocumentForTesting, GlbStaff.CurrentUser,
				Factory.New<IStmPrintQueue>().PK, 1);
			Factory.Save();

			// setup orders and pick
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			Helper.CreateWhsOrderLine(order, data.Part1, 2m).WE_F3_NKPackType = "ROL";
			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItems();

			// test
			order.AutoPackAndPrintAllLabels(Notify, disablePrinting: false);
			AssertEquals(
				"\r\nThe following items could not be Auto-Packed because on the Warehouse Order, the Qty per Package is 0, or no Package Unit is defined:\r\n\r\n\t2 x P1\r\n",
				Notify.AsString);
		}

		public void TestAutoPackAndPrintAllLabels_AvoidFactorySaveOnValidationError()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			data.Part1.OP_Depth = 20m;
			data.Part1.OP_Width = 20m;
			data.Part1.OP_Height = 20m;
			data.Part1.OP_MeasureUQ = "M";

			data.Part1.OP_WeightUQ = Constants.Weight.Kilograms;
			data.Part1.OP_CubicUQ = Constants.Volume.CubicMetres;

			Helper.CreateProductUnit(data.Part1, "PLT", 10m);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			// setup orders
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order.Lines[0].WE_F3_NKPackType = "PLT";

			// setup picks
			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItems();

			Factory.Save();

			var packtype = Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT"));
			packtype.F3_Length = 1m;
			packtype.F3_Width = 1m;
			packtype.F3_Height = 1m;

			packtype.F3_Weight = 15m;
			packtype.F3_UnitOfWeight = Core.Constants.Weight.Kilograms;
			packtype.F3_UnitOfDimension = Core.Constants.Length.Metres;

			order.WD_AddPalletWeightToOrder = true;

			Factory.Save();

			using (PackingRegistry.Instance.VolumeUnit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.Volume.CubicCentimeters))
			{
				var isSuccess = order.AutoPackAndPrintAllLabels(Notify, disablePrinting: false);
				AssertEquals(
					@"Order W00000002 cannot be saved due to the following validation errors:
Error - KP_Volume: The number 1,000,000 is too large, the maximum value allowed for Volume is 999,999.999.",
					Notify.AsString.Trim());

				AssertEquals(false, isSuccess);

				var newFactory = new BusinessObjectFactory();
				var order2InNewFactory = newFactory.Load<WhsOrder>(order.PK);
				AssertEquals("Packages should not have been created and saved for second order.", 0, order2InNewFactory.PackageJob.Packages.Count);
			}
		}

		public void TestAutoPackAndPrintAllLabels_SerialNumberedProducts()
		{
			if (Globals.IsWeb)
			{
				Assert(true);
			}
			else
			{
				// setup data
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				Helper.CreateProductUnit(data.Part1, "PLT", 2);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation).WE_SerialNumber =
					"SER1";
				Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation).WE_SerialNumber =
					"SER2";
				Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation).WE_SerialNumber =
					"SER3";
				Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation).WE_SerialNumber =
					"SER4";
				receive.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				// setup the printer
				Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocumentForTesting, GlbStaff.CurrentUser,
					Factory.New<IStmPrintQueue>().PK, 1);
				Factory.Save();

				// setup orders and pick
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m).WE_F3_NKPackType = "PLT";

				var pick = Helper.CreatePickNew(order);
				pick.AutoAllocateItems();

				// test
				AssertEquals("Precondition", 0, order.PackageJob.Packages.Count);
				AssertEquals("Precondition", 0, Factory.Load<IStmPrintJob>(new ZQuery()).Length);

				var autoPackSuccess = order.AutoPackAndPrintAllLabels(Notify, disablePrinting: false);
				AssertEquals("Auto Pack should have succeeded.", true, autoPackSuccess);
				AssertEquals("There should be only 2 packages.", 2, order.PackageJob.Packages.Count);
				foreach (var package in order.PackageJob.Packages)
				{
					AssertEquals("There should be 2 packed items.", 2, package.PackedItemDivots.Count);
					AssertEquals("The total qty of packed items should be 2.", 2m,
						package.PackedItemDivots.Sum(divot => divot.KI_PackedQty));
				}

				var printJobs = Factory.Load<IStmPrintJob>(new ZQuery());
				AssertEquals("There should be 2 print jobs.", 2, printJobs.Length);
				foreach (StmPrintJob job in printJobs)
				{
					AssertEquals("The job's parent should match the packagejob.", order.PackageJob.PK,
						job.SP_ParentGuid);
				}
			}
		}

		void AssertAutoPackedItems(PkgPackageJob packageJob, TestDataSimpleEnvironment data)
		{
			// expect 4x PLT, 2 with 4 items each, 2 with with 10 items each.
			var part1Packages = packageJob.Packages.Where(p =>
				p.KP_F3_NKPackType == "PLT" && p.PackedItemDivots[0].KI_PackedQty == 4);
			var part2Packages = packageJob.Packages.Where(p =>
				p.KP_F3_NKPackType == "PLT" && p.PackedItemDivots[0].KI_PackedQty == 10);

			AssertEquals(4, packageJob.Packages.Count);
			AssertEquals(2, part1Packages.Count());
			AssertEquals(2, part2Packages.Count());

			foreach (var package in part1Packages)
			{
				var divot = package.PackedItemDivots.Single();
				AssertEquals(4m, divot.KI_PackedQty);
			}

			foreach (var package in part2Packages)
			{
				var divot = package.PackedItemDivots.Single();
				AssertEquals(10m, divot.KI_PackedQty);
			}
		}

		#endregion

		#region TestAutoPackAndPrintAllLabelsWithNoFactorySave

		public void TestAutoPackAndPrintAllLabelsWithNoFactorySave()
		{
			AssertAutoPackAndPrintAllLabelsWithNoFactorySave(isPrinting: true);
		}

		public void TestAutoPackAndPrintAllLabelsWithNoFactorySave_WithoutPrinting()
		{
			AssertAutoPackAndPrintAllLabelsWithNoFactorySave(isPrinting: false);
		}

		void AssertAutoPackAndPrintAllLabelsWithNoFactorySave(bool isPrinting)
		{
			if (Globals.IsWeb)
			{
				Assert(true);
			}
			else
			{
				// setup data
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "x", data.Part1, 500);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "y", data.Part2, 500);
				Helper.CreateProductUnit(data.Part1, "PLT", 4);
				Helper.CreateProductUnit(data.Part2, "PLT", 10);

				// setup the printer
				Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocumentForTesting, GlbStaff.CurrentUser,
					Factory.New<IStmPrintQueue>().PK, 1);
				Factory.Save();

				// setup orders and pick
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
				Helper.CreateWhsOrderLine(order, data.Part1, 2m).WE_F3_NKPackType = "PLT";
				Helper.CreateWhsOrderLine(order, data.Part2, 2m).WE_F3_NKPackType = "PLT";
				var pick = Helper.CreatePickNew(order);
				pick.AutoAllocateItems();

				// test
				AssertEquals("Precondition", 0, order.PackageJob.Packages.Count);
				AssertEquals("Precondition", 0, Factory.Load<IStmPrintJob>(new ZQuery()).Length);

				var autoPackSuccess = order.AutoPackAndPrintAllLabelsWithNoFactorySave(Notify, disablePrinting: !isPrinting);
				AssertEquals("Auto Pack should have succeeded.", true, autoPackSuccess);

				AssertEquals("No Package should be in PackageJob.", 0,
					new BusinessObjectFactory().Load<WhsOrder>(order.PK).PackageJob.Packages.Count);
				AssertEquals("No PrintJob should be created.", 0,
					new BusinessObjectFactory().Load<IStmPrintJob>(new ZQuery()).Length);

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
				AssertAutoPackedItems(orderInNewFactory.PackageJob, data);

				var printJobs = newFactory.Load<IStmPrintJob>(new ZQuery());
				AssertAutoPackedItems(orderInNewFactory.PackageJob, data);

				if (isPrinting)
				{
					AssertEquals("There should be a print job for all 4 packages.", 4, printJobs.Length);
					foreach (StmPrintJob job in printJobs)
					{
						AssertEquals("The job's parent should match the packagejob.", orderInNewFactory.PackageJob.PK,
							job.SP_ParentGuid);
					}

					//delete print jobs
					foreach (StmPrintJob job in printJobs)
					{
						job.Delete();
					}

					newFactory.Save();

					AssertEquals("No PrintJobs after the delete.", 0,
						new BusinessObjectFactory().Load<IStmPrintJob>(new ZQuery()).Length);
					order.WD_ExternalReference = "ABC1";
					Factory.Save();
					AssertEquals("No PrintJob should be created after the save.", 0,
						new BusinessObjectFactory().Load<IStmPrintJob>(new ZQuery()).Length);
				}
				else
				{
					AssertEquals("There should be no print jobs when printing is disabled.", 0, printJobs.Length);
				}
			}
		}

		public void TestAutoPackAndPrintAllLabelsWithNoFactorySave_WithNoPackageJob()
		{
			// setup data
			var data = new TestDataSimpleEnvironment(Factory);

			// setup order and pick
			var orderWithNoPackageJob = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var line = orderWithNoPackageJob.Lines.AddNew();
			line.WE_TransactionQuantity = 1;
			var pick = Helper.CreatePickNew(orderWithNoPackageJob);
			orderWithNoPackageJob.WD_WP = pick.PK;
			orderWithNoPackageJob.WD_DocketID = "BLAH";
			orderWithNoPackageJob.PackageJob.Delete(); // this should not happen in production

			// test
			var autoPackSuccess = orderWithNoPackageJob.AutoPackAndPrintAllLabelsWithNoFactorySave(Notify, disablePrinting: false);
			AssertEquals("Auto Pack should have failed.", false, autoPackSuccess);
			AssertEquals(@"Order BLAH does not have a Packing Job.
To rectify, open the Release, click on the Packing tab and then click Save.
This will create the necessary Packing information.
", Notify.AsString);
		}

		public void TestAutoPackAndPrintAllLabelsWithNoFactorySave_WithNoPick()
		{
			// setup data
			var data = new TestDataSimpleEnvironment(Factory);

			// setup order
			var orderWithNoPick = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			orderWithNoPick.WD_DocketID = "BLAH";

			var autoPackSuccess = orderWithNoPick.AutoPackAndPrintAllLabelsWithNoFactorySave(Notify, disablePrinting: false);
			AssertEquals("Auto Pack should have failed.", false, autoPackSuccess);
			AssertEquals("Order BLAH cannot be Auto-Packed as it has not been Picked.\r\n", Notify.AsString);
		}

		public void TestAutoPackAndPrintAllLabelsWithNoFactorySave_AlreadyPacked()
		{
			// setup order
			var data = new TestDataSimpleEnvironment(Factory);
			var orderWithPreExistingPacks = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var line = orderWithPreExistingPacks.Lines.AddNew();
			line.WE_TransactionQuantity = 1;

			var pick = Helper.CreatePickNew(orderWithPreExistingPacks);
			orderWithPreExistingPacks.WD_WP = pick.PK;
			orderWithPreExistingPacks.WD_DocketID = "BLAH";
			orderWithPreExistingPacks.PackageJob.Packages.AddNew();
			AssertEquals("Precondition", 1, orderWithPreExistingPacks.PackageJob.Packages.Count);

			var autoPackSuccess = orderWithPreExistingPacks.AutoPackAndPrintAllLabelsWithNoFactorySave(Notify, disablePrinting: false);
			AssertEquals("Auto Pack should have failed.", false, autoPackSuccess);
			AssertEquals("Order BLAH cannot be Auto-Packed as it already has Packages.\r\n", Notify.AsString);
		}

		public void TestAutoPackAndPrintAllLabelsWithNoFactorySave_PickIsAlreadyFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			order.WD_DocketID = "BLAH";

			var pick = Helper.CreatePickNew(order);
			AssertNotNull("Precondition", order.PackageJob);

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);

			var autoPackSuccess = order.AutoPackAndPrintAllLabelsWithNoFactorySave(Notify, disablePrinting: false);
			AssertEquals("Auto Pack should have failed.", false, autoPackSuccess);
			AssertEquals("Order BLAH cannot be Auto-Packed as it has a read only Packing Job.\r\n", Notify.AsString);
		}

		public void TestAutoPackAndPrintAllLabelsWithNoFactorySave_ConsigneeDisabledAutoPack()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var orderWithAutoPackDisabled =
				Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			orderWithAutoPackDisabled.Consignee.MiscServ.OM_IsAutoPackAllowed = false;
			Helper.CreatePickNew(orderWithAutoPackDisabled);

			var autoPackSuccess = orderWithAutoPackDisabled.AutoPackAndPrintAllLabelsWithNoFactorySave(Notify, disablePrinting: false);
			AssertEquals("Auto Pack should have failed.", false, autoPackSuccess);
			AssertEquals("Order W00000002 cannot be Auto-Packed as it is not enabled on Consignee 111.\r\n",
				Notify.AsString);
		}

		public void TestAutoPackAndPrintAllLabelsWithNoFactorySave_PickIsReadyForPlanningDisabledAutoPack()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;

			var autoPackSuccess = order.AutoPackAndPrintAllLabelsWithNoFactorySave(Notify, disablePrinting: false);
			AssertEquals("Auto Pack should have failed.", false, autoPackSuccess);
			AssertEquals("Order W00000002 cannot be Auto-Packed as it has a read only Packing Job.\r\n",
				Notify.AsString);
		}

		public void TestAutoPackAndPrintAllLabelsWithNoFactorySave_PickIsPlannedDisabledAutoPack()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;

			var autoPackSuccess = order.AutoPackAndPrintAllLabelsWithNoFactorySave(Notify, disablePrinting: false);
			AssertEquals("Auto Pack should have failed.", false, autoPackSuccess);
			AssertEquals("Order W00000002 cannot be Auto-Packed as it has a read only Packing Job.\r\n",
				Notify.AsString);
		}

		public void TestAutoPackAndPrintAllLabelsWithNoFactorySave_NoPackableItems()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			Helper.CreateWhsOrderLine(order, data.Part2, 100m);
			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItems();
			Factory.Save();

			var autoPackSuccess = order.AutoPackAndPrintAllLabelsWithNoFactorySave(Notify, disablePrinting: false);
			AssertEquals("Auto Pack should have failed.", false, autoPackSuccess);
			AssertEquals(
				string.Format("Order {0} cannot be Auto-Packed as it has no Packable Items caused by a shortfall.\r\n",
					order.WD_DocketID), Notify.AsString);
		}

		public void TestAutoPackAndPrintAllLabelsWithNoFactorySave_UnitConversionFailure()
		{
			// setup data
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "x", data.Part1, 500);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "y", data.Part2, 500);
			Helper.CreateProductUnit(data.Part1, "PLT", 4);
			Helper.CreateProductUnit(data.Part2, "PLT", 10);

			// setup the printer
			Helper.CreateDefaultPrinter(PackingRegistry.DefaultTargetLabelDocumentForTesting, GlbStaff.CurrentUser,
				Factory.New<IStmPrintQueue>().PK, 1);
			Factory.Save();

			// setup orders and pick
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			Helper.CreateWhsOrderLine(order, data.Part1, 2m).WE_F3_NKPackType = "ROL";
			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItems();

			// test
			order.AutoPackAndPrintAllLabelsWithNoFactorySave(Notify, disablePrinting: false);
			AssertEquals(
				"\r\nThe following items could not be Auto-Packed because on the Warehouse Order, the Qty per Package is 0, or no Package Unit is defined:\r\n\r\n\t2 x P1\r\n",
				Notify.AsString);
		}

		#endregion

		#region Packing

		#region TestPackableItemParents_ReleaseLinesUpdatedWhenOrderLinesChanged

		public void TestPackableItemParents_ReleaseLinesUpdatedWhenOrderLinesChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", part3, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true; // need to emulate being on the Release form.

			var releaseLine1 = orderLine1.ReleaseLines[0];
			var releaseLine2 = orderLine2.ReleaseLines[0];
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2 },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(p => p.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2 },
				order.PackableItemParents.Typed);

			int packableItemParentsCountChangedHitCount = 0;
			IPackingParentWithPackableItems packingParent = order;
			packingParent.PackableItemParentsCountChanged += (sender, e) => packableItemParentsCountChangedHitCount++;
			var orderLine3 = Helper.CreateWhsOrderLine(order, part3, 10m);

			pick.ClearOrderedInventoriesCache();
			pick.ClearAllInventoriesCache();
			pick.AutoAllocateItemsWithMock();
			AssertEquals("Rebuilding the Collection should have only one hit at the end.", 1,
				packableItemParentsCountChangedHitCount);

			var releaseLine3 = orderLine3.ReleaseLines[0];
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2, releaseLine3 },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(p => p.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2, releaseLine3 },
				order.PackableItemParents.Typed);

			orderLine1.WE_WD = ZGuid.Empty;
			AssertEquals("Rebuilding the Collection should have only one hit at the end.", 2,
				packableItemParentsCountChangedHitCount);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine2, releaseLine3 },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(p => p.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine2, releaseLine3 },
				order.PackableItemParents.Typed);

			orderLine1.WE_WD = order.PK;
			AssertEquals("Rebuilding the Collection should have only one hit at the end.", 3,
				packableItemParentsCountChangedHitCount);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2, releaseLine3 },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(p => p.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2, releaseLine3 },
				order.PackableItemParents.Typed);

			orderLine2.Delete();
			AssertEquals("Rebuilding the Collection should have only one hit at the end.", 4,
				packableItemParentsCountChangedHitCount);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine3 },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(p => p.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine3 },
				order.PackableItemParents.Typed);
		}

		public void
			TestPackableItemParents_ReleaseLinesUpdatedWhenOrderLinesChanged_OrderLineDeleted_DataRefreshDoesNotThrowException()
		{
			if (Globals.IsWeb)
			{
				// DataRefreshManager is not Enabled when Globals.IsWeb = true
				Assert(true);
			}
			else
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m);
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				order.WD_RequiredDate = ZDateTimeOffset.Now;
				order.ConsigneePK = data.Org1.PK;

				var orderLine1 = Helper.CreateWhsPickableDocketLine(order, data.Part1, 10m);
				var orderLine2 = Helper.CreateWhsPickableDocketLine(order, data.Part2, 20m);

				Helper.CreatePickNew(order);
				AssertEquals("Order line has pick line.", true, orderLine1.PickLines.Count > 0);
				AssertEquals("Order line has pick line.", true, orderLine2.PickLines.Count > 0);
				Factory.Save();

				var newFactory = new BusinessObjectFactory { RefreshEnabled = true };
				var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
				var orderLine1InNewFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
				var releaseLine1 = orderLine1InNewFactory.ReleaseLines[0];
				AssertEquals("Release line collection has been poked.", true,
					orderLine1InNewFactory.IsReleaseLineCollectionBuilt);

				var orderLine2InNewFactory = newFactory.Load<WhsOrderLine>(orderLine2.PK);
				var releaseLine2 = orderLine2InNewFactory.ReleaseLines[0];
				AssertEquals("Release line collection has been poked.", true,
					orderLine2InNewFactory.IsReleaseLineCollectionBuilt);

				AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2 },
					orderInNewFactory.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
						.Select(p => p.PackableItemParent));

				orderLine1.WE_TransactionQuantity = 0m;
				order.IsSavedFromOrderForm = true;
				order.RunPreSaveValidation();
				AssertNoExceptionThrown("No exception thrown on data refresh.", Factory.Save);

				AssertEquals("Order line is deleted.", true, orderLine1.IsDeleted);
				AssertEquals("Order line is deleted.", true, orderLine1InNewFactory.IsDeleted);
				AssertContainsExactElementsInAnyOrder(new[] { releaseLine2 },
					orderInNewFactory.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
						.Select(p => p.PackableItemParent));
			}
		}

		#endregion

		#region TestPackableItemParents_UpdatePackableItemParentsFromOtherFactory

		public void TestPackableItemParents_UpdatePackableItemParentsFromOtherFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true; // need to emulate being on the Release form.

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.Quantity = 6m;
			releaseLine1.PartAttribute1 = "RED";
			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.Quantity = 4m;
			releaseLine2.PartAttribute1 = "BLUE";
			var package = order.PackageJob.Packages.AddNew();
			package.Pack(releaseLine1, 6m);
			package.Pack(releaseLine2, 4m);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			newFactory.Load<WhsOrderLine>(orderLine.PK).Delete();

			AssertNoExceptionThrown("Should be able to save data without error.", newFactory.Save);
		}

		#endregion

		#region TestPackableItemParents_ReleaseLineWithNoStockAllocated

		public void TestPackableItemParents_ReleaseLineWithNoStockAllocated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true; // need to emulate being on the Release form.

			var releaseLine = orderLine.ReleaseLines[0];
			using (releaseLine.GetValidationSuspender()) // test that release lines are validated when built
			{
				releaseLine.PartAttribute2 = "RED";
			}

			AssertNoErrors("Precondition:", releaseLine);
			AssertEquals("No valid Release Lines for Packing to use.", 0, order.PackageJob.PackableItemParents.Count);
			AssertEquals("No valid Release Lines for Packing to use.", 0, order.PackableItemParents.Count);

			releaseLine.PartAttribute2 = "";
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(w => w.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);
		}

		#endregion

		#region TestPackableItemParents_RevertedQuantityWhenPackedError

		public void TestPackableItemParents_RevertedQuantityWhenPackedError()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true; // need to emulate being on the Release form.

			var releaseLine = orderLine.ReleaseLines[0];
			var package = order.PackageJob.Packages.AddNew();
			package.Pack(releaseLine, 10m);
			AssertEquals("Precondition: Release Line is packed.", true, releaseLine.IsPacked);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(p => p.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine },
				package.PackedItems.Typed.Select(p => p.PackableItemParent));
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var orderLineInOtherFactory = otherFactory.Load<WhsOrderLine>(orderLine.PK);
			var orderInOtherFactory = orderLineInOtherFactory.Order;
			orderInOtherFactory.Pick.IsAlterPick = true; // need to emulate being on the Release form.

			var releaseLineInOtherFactory = orderLineInOtherFactory.ReleaseLines[0];
			AssertEquals("Precondition: Release Line is packed.", true, releaseLineInOtherFactory.IsPacked);
			releaseLineInOtherFactory.Quantity = 9m;
			AssertHasError(releaseLineInOtherFactory.QuantityInfo,
				"This item is packed.\r\n\r\nQuantity released (9) cannot be less than quantity packed (10.000). Reduce the quantity packed first.");

			var packageInOtherFactory = otherFactory.Load<PkgPackage>(package.PK);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLineInOtherFactory },
				orderInOtherFactory.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(p => p.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(new[] { releaseLineInOtherFactory },
				orderInOtherFactory.PackableItemParents.Typed);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLineInOtherFactory },
				packageInOtherFactory.PackedItems.Typed.Select(p => p.PackableItemParent));
		}

		#endregion

		#region TestLoadAllPackableItemParentsInOneHit

		public void TestLoadAllPackableItemParentsInOneHit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 250m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 100m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 150m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			Factory.Save();

			var releaseLine1 = orderLine1.ReleaseLines[0];
			var releaseLine2 = orderLine2.ReleaseLines[0];

			var package1 = order.PackageJob.Packages.AddNew();
			var package2 = order.PackageJob.Packages.AddNew();
			var package3 = order.PackageJob.Packages.AddNew();
			var package4 = order.PackageJob.Packages.AddNew();

			package1.Pack(releaseLine1, 50m);
			package2.Pack(releaseLine1, 50m);
			package3.Pack(releaseLine2, 50m);
			package4.Pack(releaseLine2, 100m);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedOrder = newFactory.Load<WhsOrder>(order.PK);
			var loadedPick = newFactory.Load<WhsPick>(pick.PK);
			loadedPick.IsAlterPick = true;

			((IPackingParentWithPackableItems)loadedOrder).LoadAllPackableItemParentsInOneHit(loadedOrder.PackageJob);
			foreach (var package in loadedOrder.PackageJob.Packages)
			{
				var poke = package.PackedItemDivots.Select(d => d.PackedItem).ToArray();
			}

			AssertTableHitCount(1, WhsPickLineSchema.Constants.TableName, newFactory);
		}

		#endregion

		#region TestRegisterOrderLineWithClearedReleaseLines

		public void TestRegisterOrderLineWithClearedReleaseLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 3m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 3m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			Helper.CreatePickNew(order);

			var releaseLine1 = orderLine1.ReleaseLines[0];
			releaseLine1.Quantity = 2m;
			releaseLine1.PartAttribute1 = "RED";

			var releaseLine2 = orderLine1.ReleaseLines.AddNew();
			releaseLine2.Quantity = 1m;
			releaseLine2.PartAttribute1 = "BLUE";

			var releaseLine3 = orderLine2.ReleaseLines[0];
			releaseLine3.Quantity = 1m;
			releaseLine3.PartAttribute1 = "YELLOW";

			var releaseLine4 = orderLine2.ReleaseLines.AddNew();
			releaseLine4.Quantity = 2m;
			releaseLine4.PartAttribute1 = "GREEN";

			var releaseLine5 = orderLine3.ReleaseLines[0];

			AssertContainsExactElementsInAnyOrder(
				new[] { releaseLine1, releaseLine2, releaseLine3, releaseLine4, releaseLine5 },
				order.PackableItemParents.Typed);

			var packableItemsChangedHitCount = 0;
			order.PackableItemParents.CountChanged += (sender, e) => packableItemsChangedHitCount++;

			// ClearCollection calls RegisterOrderLineWithClearedReleaseLines
			orderLine1.ReleaseLines.ClearCollection();
			orderLine2.ReleaseLines.ClearCollection();
			AssertEquals("Count changed should have been fired once per Cleared Release Line Collection.", 2,
				packableItemsChangedHitCount);

			orderLine1.BuildReleaseLines();
			orderLine2.BuildReleaseLines();
			AssertEquals("Count changed should not have been fired when rebuilding Release Lines.", 2,
				packableItemsChangedHitCount);

			AssertEquals("Release Lines should have been rebuilt correctly.", 2, orderLine1.ReleaseLines.Count);
			AssertEquals("Release Lines should have been rebuilt correctly.", 2, orderLine2.ReleaseLines.Count);
			AssertCollectionNotContains(releaseLine1, orderLine1.ReleaseLines);
			AssertCollectionNotContains(releaseLine2, orderLine1.ReleaseLines);
			AssertCollectionNotContains(releaseLine3, orderLine2.ReleaseLines);
			AssertCollectionNotContains(releaseLine4, orderLine2.ReleaseLines);
			AssertEquals("Accessing Count shold give us correct amount of records.", 5,
				order.PackableItemParents.Count);
			AssertContainsExactElementsInAnyOrder(
				orderLine1.ReleaseLines.Cast<WhsReleaseLine>().Concat(orderLine2.ReleaseLines.Cast<WhsReleaseLine>())
					.Append(releaseLine5), order.PackableItemParents.Typed);
			AssertEquals("Count changed should only have been fired once for rebuilding Packable Item Parents.", 3,
				packableItemsChangedHitCount);
		}

		public void TestRegisterOrderLineWithClearedReleaseLines_DefensiveException()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			AssertExceptionThrown(typeof(InvalidOperationException), "Only register OrderLines on this Order.",
				() => order.RegisterOrderLineWithClearedReleaseLines(Factory.New<WhsOrderLine>()));

			AssertNoExceptionThrown(() => order.RegisterOrderLineWithClearedReleaseLines(order.Lines[0]));
		}

		#endregion

		#region TestPackingPickByBOMKitsBuiltFromComponents

		public void TestPackingPickByBOMKitsBuiltFromComponents()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct1, 1m, Constants.PkgUnit.Unit);

			var bomComponentProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct2, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 6m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct1, 20m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct2, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			Factory.Save();

			Helper.CreatePickNew(order);
			AssertEquals("Precondition: Should be 1 Release Line representing Built Kits + Already made Kits.", 1,
				orderLine.ReleaseLines.Count);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertEquals("Precondition: Should be 1 Release Line representing Built Kits + Already made Kits.", 10m,
				releaseLine.Quantity);

			IPackableItemParent iPackableItemParent = releaseLine;
			AssertEquals("We are able to pack assembled Kits now.", 10m, iPackableItemParent.TotalQty);

			var package = order.PackageJob.Packages.AddNew();
			var packedItem = package.Pack(iPackableItemParent, 10m).Single();
			AssertEquals("Packed Item should have the Key for the Kit Product.",
				true, orderLine.PickLines.All(pl => InventoryGroupingKey.New(pl) == packedItem.Key));
			AssertEquals("Packed Item should have the PackableItemParent for the Kit Product.", releaseLine,
				packedItem.PackableItemParent);
			AssertContainsExactElementsInAnyOrder("Packed Item should have the Pick Line for the Kit Product.", orderLine.PickLines.ToArray(), packedItem.PackedItems);
			AssertEquals("We are able to pack assembled Kits now.", 10m, packedItem.PackedQty);
		}

		#endregion

		#region TestPackingWhenReleaseCapturedAttribsAreReducedWhenEmptyReleaseCapturedLineIsPacked

		public void TestPackingWhenReleaseCapturedAttribsAreReducedWhenEmptyReleaseCapturedLineIsPacked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true; // need to emulate being on the Release form.

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.Quantity = 6m;
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(w => w.PackableItemParent));
			AssertEquals("Nothing should be packed.", 0m, order.PackageJob.PackableItemParents[0].PackedQty);
			AssertEquals("Everything should be unpacked.", 6m, order.PackageJob.PackableItemParents[0].UnpackedQty);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 }, order.PackableItemParents.Typed);

			releaseLine1.PartAttribute1 = "RED";
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(w => w.PackableItemParent));
			AssertEquals("Nothing should be packed.", 0m, order.PackageJob.PackableItemParents[0].PackedQty);
			AssertEquals("Everything should be unpacked.", 6m, order.PackageJob.PackableItemParents[0].UnpackedQty);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 }, order.PackableItemParents.Typed);

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(w => w.PackableItemParent));
			AssertEquals("Nothing should be packed.", 0m, order.PackageJob.PackableItemParents[0].PackedQty);
			AssertEquals("Everything should be unpacked.", 6m, order.PackageJob.PackableItemParents[0].UnpackedQty);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 }, order.PackableItemParents.Typed);

			releaseLine2.Quantity = 4m;
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2 },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(w => w.PackableItemParent));
			AssertEquals("Nothing should be packed.", 0m, order.PackageJob.PackableItemParents[0].PackedQty);
			AssertEquals("Nothing should be packed.", 0m, order.PackageJob.PackableItemParents[1].PackedQty);
			AssertEquals("Everything should be unpacked.", 6m, order.PackageJob.PackableItemParents[0].UnpackedQty);
			AssertEquals("Everything should be unpacked.", 4m, order.PackageJob.PackableItemParents[1].UnpackedQty);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2 },
				order.PackableItemParents.Typed);

			var package = order.PackageJob.Packages.AddNew();
			var packedItem = package.Pack(releaseLine2, 4m).Single();
			AssertEquals("Precondition: Release Line is Packed.", true, releaseLine2.IsPacked);

			var wrapperForReleaseLine2 = order.PackageJob.PackableItemParents
				.Cast<PackableItemParentWrapper>()
				.Single(w => w.PackableItemParent == releaseLine2);
			AssertEquals("Everything should be packed.", 4m, wrapperForReleaseLine2.PackedQty);
			AssertEquals("Nothing should be unpacked.", 0m, wrapperForReleaseLine2.UnpackedQty);
			AssertEquals("Everything should be packed.", 4m, packedItem.PackedQty);
			AssertEquals("Packed Item should have correct Quantity.", 4m, packedItem.PackedItems.Single().Quantity);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var orderInOtherFactory =
				(WhsOrder)otherFactory.Load(ExpectedBusinessObjectType,
					order.PK); // use ExpectedBusinessObjectType for tracking test
			orderInOtherFactory.Pick.IsAlterPick = true; // need to emulate being on the Release form.
			var releaseLine1InOtherFactory = orderInOtherFactory.Lines[0].ReleaseLines.Cast<WhsReleaseLine>()
				.Single(r => r.PartAttribute1 == "RED");
			var releaseLine2InOtherFactory = orderInOtherFactory.Lines[0].ReleaseLines.Cast<WhsReleaseLine>()
				.Single(r => r.PartAttribute1 == "");
			releaseLine1InOtherFactory.Quantity = 5m;
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1InOtherFactory, releaseLine2InOtherFactory },
				orderInOtherFactory.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(w => w.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1InOtherFactory, releaseLine2InOtherFactory },
				orderInOtherFactory.PackableItemParents.Typed);

			var wrapperForReleaseLine2InOtherFactory = orderInOtherFactory.PackageJob.PackableItemParents
				.Cast<PackableItemParentWrapper>()
				.Single(w => w.PackableItemParent == releaseLine2InOtherFactory);
			AssertEquals("Everything should be packed.", 4m, wrapperForReleaseLine2InOtherFactory.PackedQty);
			AssertEquals("Nothing should be unpacked.", 0m, wrapperForReleaseLine2InOtherFactory.UnpackedQty);

			var packageInOtherFactory = otherFactory.Load<PkgPackage>(package.PK);
			var packedItemInOtherFactory = packageInOtherFactory.PackedItems[0];
			AssertEquals("Correct Release Line should be packed.", releaseLine2InOtherFactory,
				packedItemInOtherFactory.PackableItemParent);
			AssertEquals("Everything should be packed.", 4m, packedItemInOtherFactory.PackedQty);
			AssertEquals("Packed Item should have correct Quantity.", 4m,
				packedItemInOtherFactory.PackedItems.Single().Quantity);

			var releaseLine3 = orderInOtherFactory.Lines[0].ReleaseLines.AddNew();
			releaseLine3.PartAttribute1 = "BLUE";
			releaseLine3.Quantity = 1m;
			AssertContainsExactElementsInAnyOrder(
				new[] { releaseLine1InOtherFactory, releaseLine2InOtherFactory, releaseLine3 },
				orderInOtherFactory.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(w => w.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(
				new[] { releaseLine1InOtherFactory, releaseLine2InOtherFactory, releaseLine3 },
				orderInOtherFactory.PackableItemParents.Typed);
			AssertEquals("Everything should be packed.", 4m, packedItemInOtherFactory.PackedQty);
			AssertEquals("Packed Item should have correct Quantity.", 4m,
				packedItemInOtherFactory.PackedItems.Single().Quantity);
			otherFactory.Save(); // Ensure everything was created correctly.
		}

		#endregion

		#region TestPackingWhenReleaseCapturedAttribsAreReducedWhenEmptyReleaseCapturedLineIsPacked_InMemory

		public void TestPackingWhenReleaseCapturedAttribsAreReducedWhenEmptyReleaseCapturedLineIsPacked_InMemory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true; // need to emulate being on the Release form.

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.Quantity = 6m;
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(w => w.PackableItemParent));
			AssertEquals("Nothing should be packed.", 0m, order.PackageJob.PackableItemParents[0].PackedQty);
			AssertEquals("Everything should be unpacked.", 6m, order.PackageJob.PackableItemParents[0].UnpackedQty);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 }, order.PackableItemParents.Typed);

			releaseLine1.PartAttribute1 = "RED";
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(w => w.PackableItemParent));
			AssertEquals("Nothing should be packed.", 0m, order.PackageJob.PackableItemParents[0].PackedQty);
			AssertEquals("Everything should be unpacked.", 6m, order.PackageJob.PackableItemParents[0].UnpackedQty);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 }, order.PackableItemParents.Typed);

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(w => w.PackableItemParent));
			AssertEquals("Nothing should be packed.", 0m, order.PackageJob.PackableItemParents[0].PackedQty);
			AssertEquals("Everything should be unpacked.", 6m, order.PackageJob.PackableItemParents[0].UnpackedQty);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1 }, order.PackableItemParents.Typed);

			releaseLine2.Quantity = 4m;
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2 },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(w => w.PackableItemParent));
			AssertEquals("Nothing should be packed.", 0m, order.PackageJob.PackableItemParents[0].PackedQty);
			AssertEquals("Nothing should be packed.", 0m, order.PackageJob.PackableItemParents[1].PackedQty);
			AssertEquals("Everything should be unpacked.", 6m, order.PackageJob.PackableItemParents[0].UnpackedQty);
			AssertEquals("Everything should be unpacked.", 4m, order.PackageJob.PackableItemParents[1].UnpackedQty);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2 },
				order.PackableItemParents.Typed);

			var package = order.PackageJob.Packages.AddNew();
			var packedItem = package.Pack(releaseLine2, 4m).Single();
			AssertEquals("Precondition: Release Line is Packed.", true, releaseLine2.IsPacked);

			var wrapperForReleaseLine2 = order.PackageJob.PackableItemParents
				.Cast<PackableItemParentWrapper>()
				.Single(w => w.PackableItemParent == releaseLine2);
			AssertEquals("Everything should be packed.", 4m, wrapperForReleaseLine2.PackedQty);
			AssertEquals("Nothing should be unpacked.", 0m, wrapperForReleaseLine2.UnpackedQty);
			AssertEquals("Everything should be packed.", 4m, packedItem.PackedQty);
			AssertEquals("Packed Item should have correct Quantity.", 4m, packedItem.PackedItems.Single().Quantity);

			releaseLine1.Quantity = 5m;
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2 },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(w => w.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2 },
				order.PackableItemParents.Typed);
			AssertEquals("Everything should be packed.", 4m, wrapperForReleaseLine2.PackedQty);
			AssertEquals("Nothing should be unpacked.", 0m, wrapperForReleaseLine2.UnpackedQty);

			var packedItemFromPackageCollection = package.PackedItems[0];
			AssertEquals("Correct Release Line should be packed.", releaseLine2,
				packedItemFromPackageCollection.PackableItemParent);
			AssertEquals("Everything should be packed.", 4m, packedItemFromPackageCollection.PackedQty);
			AssertEquals("Packed Item should have correct Quantity.", 4m,
				packedItemFromPackageCollection.PackedItems.Single().Quantity);

			releaseLine2.Quantity = 5m;
			AssertEquals("Everything should be packed.", 4m, wrapperForReleaseLine2.PackedQty);
			AssertEquals("Nothing should be unpacked.", 1m, wrapperForReleaseLine2.UnpackedQty);
			AssertEquals("Correct Release Line should be packed.", releaseLine2,
				packedItemFromPackageCollection.PackableItemParent);
			AssertEquals("Everything should be packed.", 4m, packedItemFromPackageCollection.PackedQty);
			AssertEquals("Packed Item should have correct Quantity.", 4m,
				packedItemFromPackageCollection.PackedItems.Single().Quantity);
			Factory.Save();

			releaseLine2.Quantity = 4m;
			AssertEquals("Everything should be packed.", 4m, wrapperForReleaseLine2.PackedQty);
			AssertEquals("Nothing should be unpacked.", 0m, wrapperForReleaseLine2.UnpackedQty);
			AssertEquals("Correct Release Line should be packed.", releaseLine2,
				packedItemFromPackageCollection.PackableItemParent);
			AssertEquals("Everything should be packed.", 4m, packedItemFromPackageCollection.PackedQty);
			AssertEquals("Packed Item should have correct Quantity.", 4m,
				packedItemFromPackageCollection.PackedItems.Single().Quantity);

			releaseLine1.Quantity = 6m;
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2 },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(w => w.PackableItemParent));
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine1, releaseLine2 },
				order.PackableItemParents.Typed);
			AssertEquals("Everything should be packed.", 4m, packedItemFromPackageCollection.PackedQty);
			AssertEquals("Packed Item should have correct Quantity.", 4m,
				packedItemFromPackageCollection.PackedItems.Single().Quantity);
			Factory.Save(); // Ensure everything was created correctly.
		}

		#endregion

		#region TestPackingWhenReleaseCapturedAttribsAreTemporarilyReducedToZero

		public void TestPackingWhenReleaseCapturedAttribsAreTemporarilyReducedToZero()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true; // need to emulate being on the Release form.

			var releaseLine = orderLine.ReleaseLines[0];
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(w => w.PackableItemParent));
			AssertEquals("Nothing should be packed.", 0m, order.PackageJob.PackableItemParents[0].PackedQty);
			AssertEquals("Everything should be unpacked.", 10m, order.PackageJob.PackableItemParents[0].UnpackedQty);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);

			releaseLine.PartAttribute1 = "RED";
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(w => w.PackableItemParent));
			AssertEquals("Nothing should be packed.", 0m, order.PackageJob.PackableItemParents[0].PackedQty);
			AssertEquals("Everything should be unpacked.", 10m, order.PackageJob.PackableItemParents[0].UnpackedQty);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);
			Factory.Save();

			releaseLine.Quantity = 0m;
			AssertEquals("No valid Release Lines, should empty collection.", 0,
				order.PackageJob.PackableItemParents.Count);
			AssertEquals("No valid Release Lines, should empty collection.", 0, order.PackableItemParents.Count);

			var otherFactory = new BusinessObjectFactory();
			var orderInOtherFactory =
				(WhsOrder)otherFactory.Load(ExpectedBusinessObjectType,
					order.PK); // use ExpectedBusinessObjectType for tracking test
			orderInOtherFactory.Pick.IsAlterPick = true; // need to emulate being on the Release form.

			var releaseLineInOtherFactory = orderInOtherFactory.Lines[0].ReleaseLines[0];
			releaseLineInOtherFactory.Quantity = 0m;
			AssertEquals("No valid Release Lines, should empty collection.", 0,
				orderInOtherFactory.PackageJob.PackableItemParents.Count);
			AssertEquals("No valid Release Lines, should empty collection.", 0,
				orderInOtherFactory.PackableItemParents.Count);

			releaseLineInOtherFactory.Quantity = 10m;
			AssertEquals("Release Line is Valid again, Collection should be updated.", 1,
				orderInOtherFactory.PackageJob.PackableItemParents.Count);
			AssertEquals("Release Line is Valid again, Collection should be updated.", 1,
				orderInOtherFactory.PackableItemParents.Count);
		}

		#endregion

		#region TestPackingWhenNonReleaseCapturedAttribsAreTemporarilyReducedToZero

		public void TestPackingWhenNonReleaseCapturedAttribsAreTemporarilyReducedToZero()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true; // need to emulate being on the Release form.

			var releaseLine = orderLine.ReleaseLines[0];
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine },
				order.PackageJob.PackableItemParents.Cast<PackableItemParentWrapper>()
					.Select(w => w.PackableItemParent));
			AssertEquals("Nothing should be packed.", 0m, order.PackageJob.PackableItemParents[0].PackedQty);
			AssertEquals("Everything should be unpacked.", 10m, order.PackageJob.PackableItemParents[0].UnpackedQty);
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);
			Factory.Save();

			releaseLine.Quantity = 0m;
			AssertEquals("No valid Release Lines, should empty collection.", 0,
				order.PackageJob.PackableItemParents.Count);
			AssertEquals("No valid Release Lines, should empty collection.", 0, order.PackableItemParents.Count);

			var otherFactory = new BusinessObjectFactory();
			var orderInOtherFactory =
				(WhsOrder)otherFactory.Load(ExpectedBusinessObjectType,
					order.PK); // use ExpectedBusinessObjectType for tracking test
			orderInOtherFactory.Pick.IsAlterPick = true; // need to emulate being on the Release form.

			var releaseLineInOtherFactory = orderInOtherFactory.Lines[0].ReleaseLines[0];
			releaseLineInOtherFactory.Quantity = 0m;
			AssertEquals("No valid Release Lines, should empty collection.", 0,
				orderInOtherFactory.PackageJob.PackableItemParents.Count);
			AssertEquals("No valid Release Lines, should empty collection.", 0,
				orderInOtherFactory.PackableItemParents.Count);

			releaseLineInOtherFactory.Quantity = 10m;
			AssertEquals("Release Line is Valid again, Collection should be updated.", 1,
				orderInOtherFactory.PackageJob.PackableItemParents.Count);
			AssertEquals("Release Line is Valid again, Collection should be updated.", 1,
				orderInOtherFactory.PackableItemParents.Count);
		}

		#endregion

		#region TestUnPackingWhenPakcedItemsWithSameKey

		public void TestUnPackingWhenPakcedItemsWithSameKey_ClearAllocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			AssertEquals("PreCondition", 1, pick.OrderedInventories.Count);
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 10m;

			var package = order.PackageJob.Packages.AddNew();
			package.Pack(orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(), 10m);
			pick.ClearAllocatedItems();
			pick.OrderedInventories[0].AvailableInventories[1].PickLineQuantity = 10m;

			package.Pack(orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(), 10m);
			pick.ClearAllocatedItems();
			pick.OrderedInventories[0].AvailableInventories[2].PickLineQuantity = 10m;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var otherPackage = newFactory.Load<PkgPackage>(package.PK);

			AssertNoExceptionThrown(() => otherPackage.Unpack(otherPackage.PackedItems[0], 10m));
		}

		public void TestUnPackingWhenPakcedItemsWithSameKey_ChangeAllocationsByAnotherInstance()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 3);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			AssertEquals("PreCondition", 1, pick.OrderedInventories.Count);
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 10m;
			Factory.Save();

			// Instance1 -- Pack
			var package = order.PackageJob.Packages.AddNew();
			package.Pack(orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(), 10m);

			// Instance2 -- Change allocations
			var factoryForPick = new BusinessObjectFactory() { RefreshEnabled = false };
			var newPick = factoryForPick.Load<WhsPick>(pick.PK);
			newPick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 0m;
			newPick.OrderedInventories[0].AvailableInventories[1].PickLineQuantity = 10m;
			factoryForPick.Save();
			Factory.Save();

			// Instance1 -- Pack
			var newFactory1 = new BusinessObjectFactory();
			var orderLine1 = newFactory1.Load<WhsOrderLine>(orderLine.PK);
			var package1 = newFactory1.Load<PkgPackage>(package.PK);
			package1.Pack(orderLine1.ReleaseLines.Cast<WhsReleaseLine>().Single(), 10m);

			// Instance2 -- Change allocations
			newPick.OrderedInventories[0].AvailableInventories[1].PickLineQuantity = 0m;
			newPick.OrderedInventories[0].AvailableInventories[2].PickLineQuantity = 10m;
			factoryForPick.Save();
			newFactory1.Save();

			// Instance1 -- Reload package and unpack
			var newFactory2 = new BusinessObjectFactory();
			var package2 = newFactory2.Load<PkgPackage>(package.PK);

			AssertNoExceptionThrown(() => package2.Unpack(package2.PackedItems[0], 10m));
		}

		#endregion

		#region TestDbHitsDeletePackagesWhenFinaliseOrder

		public void TestDbHitsDeletePackagesWhenFinaliseOrder()
		{
			const int numberOfPackages = 100;
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsCartonised = true;

			for (int i = 0; i < numberOfPackages; i++)
			{
				order.PackageJob.Packages.AddNew("PLT"); // it is empty package
			}

			AssertEquals("Precondition", true, pick.WP_IsCartonised);
			AssertEquals("Precondition", numberOfPackages, order.PackageJob.Packages.Count);
			Factory.Save();

			var expectedHitCounts = new Dictionary<string, int>
			{
				{ GenCustomAddOnRuleAckSchema.Constants.TableName, 1 },
				{ GenAddOnColumnSchema.Constants.TableName, 2 },
				{ GlbBranchSchema.Constants.TableName, 2 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ JobDocumentDeliverySchema.Constants.TableName, 1 },
				{ JobDocumentExclusionSchema.Constants.TableName, 1 },
				{ JobHeaderSchema.Constants.TableName, 1 },
				{ JobServiceSchema.Constants.TableName, 1 },
				{ OrgAddressSchema.Constants.TableName, 3 },
				{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
				{ OrgCompanyDataSchema.Constants.TableName, 1 },
				{ OrgContactSchema.Constants.TableName, 1 },
				{ OrgCusCodeSchema.Constants.TableName, 1 },
				{ OrgCustomLabelsSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ PkgPackageSchema.Constants.TableName, 3 },
				{ PkgPackageBookedDetailSchema.Constants.TableName, 2 },
				{ PkgPackageItemDivotSchema.Constants.TableName, 2 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ PkgPackageScreeningSchema.Constants.TableName, 2 },
				{ ProcessTaskNotificationSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ RefCountrySchema.Constants.TableName, 1 },
				{ RefPackTypeSchema.Constants.TableName, 1 },
				{ RefTimeZoneSchema.Constants.TableName, 1 },
				{ RefTimeZoneSetSchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ StmUniversalCopySchema.Constants.TableName, 1 },
				{ StmDocDataOverrideSchema.Constants.TableName, 2 },
				{ UNDGDataItemSchema.Constants.TableName, 2 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 3 },
				{ WhsDocketContainerSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 3 },
				{ WhsDocketPalletSchema.Constants.TableName, 1 },
				{ WhsDocketReferenceSchema.Constants.TableName, 1 },
				{ WhsLoadPkgPackagePivotSchema.Constants.TableName, 2 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsPickTrolleySlotSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsPickByLabelLabelSchema.Constants.TableName, 1 },
				{ StmDefaultPrinterSchema.Constants.TableName, 2 },
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ ProcessWorkflowExceptionTypeSchema.Constants.TableName, 1 },
				{ JobServiceLinkSchema.Constants.TableName, 2 }
			};

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			using (AssertDbHitsWithUsefulQueryInformation(expectedHitCounts, newFactory))
			using (RowFactory.SetCachedTables())
			{
				var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
				AssertEquals("Precondition", false, orderInNewFactory.IsFinalised);
				orderInNewFactory.FinaliseDocket();
				AssertEquals(true, orderInNewFactory.IsFinalised);
			}
		}

		#endregion

		#region TestFinaliseOrderShouldDeleteEmptyPackages

		public void TestFinaliseOrderShouldDeleteEmptyPackages_EmptyPackage()
		{
			Action<WhsOrder> addPackageToOrder = (order) => order.PackageJob.Packages.AddNew("PLT");

			FinaliseOrderShouldDeleteEmptyPackagesCore("Should delete empty packege", addPackageToOrder,
				expectedCountBeforeFinalise: 1, expectedCountAfterFinalise: 0);
		}

		public void TestFinaliseOrderShouldDeleteEmptyPackages_PackageIsNotEmpty()
		{
			Action<WhsOrder> addPackageToOrder = (order) =>
				order.PackageJob.Packages.AddNew("PLT").Pack(order.Lines[0].ReleaseLines[0], 5m);

			FinaliseOrderShouldDeleteEmptyPackagesCore("Should not delete packege when is not empty", addPackageToOrder,
				expectedCountBeforeFinalise: 1, expectedCountAfterFinalise: 1);
		}

		public void TestFinaliseOrderShouldDeleteEmptyPackages_OnlyEmptyPackage()
		{
			void addPackageToOrder(WhsOrder order)
			{
				order.PackageJob.Packages.AddNew("PLT");
				order.PackageJob.Packages.AddNew("PLT").Pack(order.Lines[0].ReleaseLines[0], 5m);
			}

			FinaliseOrderShouldDeleteEmptyPackagesCore("Should delete one empty packege and keep the other one ",
				addPackageToOrder, expectedCountBeforeFinalise: 2, expectedCountAfterFinalise: 1);
		}

		public void TestFinaliseOrderShouldDeleteEmptyPackages_ParentAndChildWhenAreNotEmpty()
		{
			Action<WhsOrder> addPackageToOrder = (order) =>
			{
				order.PackageJob.Packages.AddNew("PLT").Packages.AddNew("BOX").Pack(order.Lines[0].ReleaseLines[0], 5m);
			};

			FinaliseOrderShouldDeleteEmptyPackagesCore(
				"Should not delete parent and child packages when child is not empty", addPackageToOrder,
				expectedCountBeforeFinalise: 2, expectedCountAfterFinalise: 2);
		}

		public void TestFinaliseOrderShouldDeleteEmptyPackages_OnlyDeleteOuterPackage()
		{
			Action<WhsOrder> addPackageToOrder = (order) =>
			{
				order.PackageJob.Packages.AddNew("PLT").Packages.AddNew("BOX");
			};

			FinaliseOrderShouldDeleteEmptyPackagesCore("Should not delete parent when has child packages",
				addPackageToOrder, expectedCountBeforeFinalise: 2, expectedCountAfterFinalise: 2);
		}

		public void TestFinaliseOrderShouldDeleteEmptyPackages_IsNotCartonised()
		{
			Action<WhsOrder> addPackageToOrder = (order) =>
			{
				order.PackageJob.Packages.AddNew("PLT");
			};

			FinaliseOrderShouldDeleteEmptyPackagesCore("Should not delete empty packages when is not cartonised",
				addPackageToOrder, expectedCountBeforeFinalise: 1, expectedCountAfterFinalise: 1, isCartonised: false);
		}

		void FinaliseOrderShouldDeleteEmptyPackagesCore(string expectedDeltePackageNote,
			Action<WhsOrder> addPackageToOrder, int expectedCountBeforeFinalise, int expectedCountAfterFinalise,
			bool isCartonised = true)
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsCartonised = isCartonised;

			addPackageToOrder(order);

			AssertEquals("Precondition", isCartonised, pick.WP_IsCartonised);
			AssertEquals("Precondition", false, order.IsFinalised);

			AssertEquals("Precondition", expectedCountBeforeFinalise, PackageCount(order.PackageJob.Packages));

			order.FinaliseDocketAlwaysFinalisingPick();
			AssertEquals("should finalize without error", true, order.IsFinalised);
			AssertEquals("no change", isCartonised, pick.WP_IsCartonised);

			AssertEquals(expectedDeltePackageNote, expectedCountAfterFinalise, PackageCount(order.PackageJob.Packages));
		}

		int PackageCount(PkgPackageCollection packages)
		{
			return (packages?.Count ?? 0) + (packages?.Sum(c => PackageCount(c.Packages)) ?? 0);
		}

		#endregion

		#region TestFinaliseOrderShouldDeleteEmptyPackagesInCurrentOrder

		public void TestFinaliseOrderShouldDeleteEmptyPackagesInCurrentOrder()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 1m);
			var pick = Helper.CreatePickNew(new WhsOrder[] { order1, order2 });
			pick.WP_IsCartonised = true;

			order1.PackageJob.Packages.AddNew("PLT");
			order2.PackageJob.Packages.AddNew("PLT");

			AssertEquals("Precondition", true, pick.WP_IsCartonised);
			AssertEquals("Precondition", false, order1.IsFinalised);
			AssertEquals("Precondition", false, order2.IsFinalised);

			AssertEquals("Precondition", 1, PackageCount(order1.PackageJob.Packages));
			AssertEquals("Precondition", 1, PackageCount(order2.PackageJob.Packages));

			order1.FinaliseDocket();
			AssertEquals("should finalize without error", true, order1.IsFinalised);
			AssertEquals(false, order2.IsFinalised);
			AssertEquals("no change", true, pick.WP_IsCartonised);

			AssertEquals("finalize order should delete packages", 0, order1.PackageJob.Packages.Count);
			AssertEquals("finalize is not called to delete packages", 1, order2.PackageJob.Packages.Count);
		}

		#endregion

		#region TestFinaliseOrderShouldDeleteEmptyPackages_FinalisedJobWithNoPackages

		public void TestFinaliseOrderShouldDeleteEmptyPackages_FinalisedJobWithNoPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsCartonised = true;
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInFactory2 = factory2.Load<WhsPick>(pick.PK);

			var factory3 = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInFactory3 = factory3.Load<WhsPick>(pick.PK);

			// Finalise in factory 2
			pickInFactory2.FinaliseAllOrders();
			pickInFactory2.FinalisePick();
			AssertIsFinalisedPrecondition(pickInFactory2);
			// Empty package jobs are deleted on saving after finalisation
			factory2.Save();
			AssertNull("Precondition.", ((WhsOrder)pickInFactory2.Orders[0]).PackageJob);

			// Try finalising in factory 3
			AssertNoExceptionThrown(() => pickInFactory3.FinaliseAllOrders());
			pickInFactory3.FinalisePick();
			AssertIsFinalisedPrecondition(pickInFactory3);
		}

		#endregion

		#region TestIPackingParent_OnPackageBookedViaRTUS

		public void TestIPackingParent_OnPackageBookedViaRTUS()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Assert("Precondition.", order.WD_BookedWithCBADateTimeUtc.IsEmpty);

			var now = ZDateTime.Now;
			((IPackingParent)order).OnPackageBookedViaRTUS(now);

			AssertEquals("Should set value when is empty.", now, order.WD_BookedWithCBADateTimeUtc);
			((IPackingParent)order).OnPackageBookedViaRTUS(now.AddDays(1));

			AssertEquals("Should not change if not empty.", now, order.WD_BookedWithCBADateTimeUtc);
		}

		#endregion

		#endregion

		#region Unpacking

		public void TestLoadedPackageAreNotAllowedToUnpack()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Factory.Save();

			Helper.CreatePickNew(order);
			var pickLine = order.Lines.Single().PickLines.Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
			{
				Factory.Save();
			}

			var package = order.PackageJob.Packages.AddNew("CTN");
			package.Pack(order.Lines[0].ReleaseLines[0], 10m);
			var pivot = Helper.CreateLoadPkgPackagePivot(package.PK, load);
			pivot.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot.WLP_GS_NKLoadingUser = "E";

			Factory.Save();

			var packageInNewFactory = new BusinessObjectFactory().Load<PkgPackage>(package.PK);
			AssertEquals("Package is Loaded", true, pivot.WLP_LoadedTime != ZDateTimeOffset.Empty && pivot.WLP_UnloadedTime == ZDateTimeOffset.Empty);
			AssertEquals("Cannot delete or unpack a loaded package.", false, packageInNewFactory.IsAvailableForUnpacking(out var errorMessage));
			AssertEquals("Cannot delete or unpack a loaded package.", errorMessage);

			var pivotInAnotherFactory = new BusinessObjectFactory().Load<WhsLoadPkgPackagePivot>(pivot.PK);
			pivotInAnotherFactory.WLP_UnloadedTime = ZDateTimeOffset.Now;
			pivotInAnotherFactory.WLP_GS_NKLoadingUser = "E";
			var packageInAnotherFactory = pivotInAnotherFactory.Factory.Load<PkgPackage>(package.PK);
			AssertEquals("Package is Unloaded", true, pivotInAnotherFactory.WLP_UnloadedTime != ZDateTimeOffset.Empty);
			AssertEquals("Unloaded Packages are allowed to unpack.", true, packageInAnotherFactory.IsAvailableForUnpacking(out errorMessage));
			AssertEquals("No error message.", string.Empty, errorMessage);
		}

		#endregion

		#region Picking

		#region TestGetPickabilityCore

		protected override void TestGetPickabilityCore(WhsPick pick, WhsPickableDocket order,
			WhsPickableDocketLine line, WhsPick.DocketPickabilityEventArgs args)
		{
			OrgHeader consignee = order.Consignee;

			base.TestGetPickabilityCore(pick, order, line, args);

			// order has no consignee (can happen when generating orders via op actions where the user needs to enter the consignee after generation)
			order.ConsigneeAddressPK = ZGuid.Empty;
			args = order.GetPickability(pick);
			AssertGetPickabilityResult(string.Format("This {0} has no Consignee.", order.Description),
				NotificationTypes.Error, false, false, args);
			order.ConsigneeAddressPK = consignee.MainAddress.PK; // clean up

			// If overridden Consignee Address and no company name - Error.
			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeDocAddress.E2_CompanyName = "";
			args = order.GetPickability(pick);
			AssertGetPickabilityResult(string.Format("This {0} has no Consignee.", order.Description),
				NotificationTypes.Error, false, false, args);

			// overridden Consignee Addresses should be allowed.
			order.ConsigneeDocAddress.E2_CompanyName = "SOME NAME";
			order.ConsigneeDocAddress.E2_Address1 = "#1";
			order.ConsigneeDocAddress.E2_City = "SYDNEY";
			order.ConsigneeDocAddress.E2_RN_NKCountryCode = ZString.Empty;
			args = order.GetPickability(pick);
			AssertGetPickabilityResult("", NotificationTypes.None, true, false, args);
			order.ConsigneeAddressPK = consignee.MainAddress.PK; // clean up

			// consignee address without city
			order.ConsigneeDocAddress.E2_AddressOverride = false;
			order.ConsigneeDocAddress.E2_City = "";
			args = order.GetPickability(pick);
			AssertGetPickabilityResult("", NotificationTypes.None, true, false, args);

			// consignee address with city
			order.ConsigneeDocAddress.E2_City = "SYDNEY";
			args = order.GetPickability(pick);
			AssertGetPickabilityResult("", NotificationTypes.None, true, false, args);

			// overridden consignee address without city
			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeDocAddress.E2_CompanyName = "ABCD";
			order.ConsigneeDocAddress.E2_City = "";
			args = order.GetPickability(pick);
			AssertGetPickabilityResult(string.Format("Consignee on this {0} has validation errors.", order.Description),
				NotificationTypes.Error, false, false, args);

			// overridden consignee address with city
			order.ConsigneeDocAddress.E2_City = "SYDNEY";
			order.ConsigneeDocAddress.E2_Address1 = "#1";
			order.ConsigneeDocAddress.E2_City = "SYDNEY";
			order.ConsigneeDocAddress.E2_RN_NKCountryCode = ZString.Empty;
			args = order.GetPickability(pick);
			AssertGetPickabilityResult("", NotificationTypes.None, true, false, args);

			// order line has PalletID and pick has DynamicPickAreaOverride set
			line.WE_PalletID = "ABC";
			var newArea = Factory.New<WhsArea>();
			newArea.WA_WW_Whs = order.WD_WW_Whs;
			pick.WP_WA_DynamicPickAreaOverride = newArea.PK;
			args = order.GetPickability(pick);
			AssertGetPickabilityResult("Pick is using Dynamic Pick Face Replenishment and cannot attach Order with Ordered Pallet ID.", NotificationTypes.Error, false, false, args);
			pick.WP_WA_DynamicPickAreaOverride = ZGuid.Empty;// clean up
		}

		protected override void TestGetPickabilityCore(WhsPickableDocket order,
			WhsPickableDocketLine line, WhsPick.DocketPickabilityEventArgs args)
		{
			var consignee = order.Consignee;

			base.TestGetPickabilityCore(order, line, args);

			// order has no consignee (can happen when generating orders via op actions where the user needs to enter the consignee after generation)
			order.ConsigneeAddressPK = ZGuid.Empty;
			args = order.GetPickabilityWithoutPick();
			AssertGetPickabilityResult(string.Format("This {0} has no Consignee.", order.Description),
				NotificationTypes.Error, false, false, args);
			order.ConsigneeAddressPK = consignee.MainAddress.PK; // clean up

			// If overridden Consignee Address and no company name - Error.
			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeDocAddress.E2_CompanyName = "";
			args = order.GetPickabilityWithoutPick();
			AssertGetPickabilityResult(string.Format("This {0} has no Consignee.", order.Description),
				NotificationTypes.Error, false, false, args);

			// overridden Consignee Addresses should be allowed.
			order.ConsigneeDocAddress.E2_CompanyName = "SOME NAME";
			order.ConsigneeDocAddress.E2_Address1 = "#1";
			order.ConsigneeDocAddress.E2_City = "SYDNEY";
			order.ConsigneeDocAddress.E2_RN_NKCountryCode = ZString.Empty;
			args = order.GetPickabilityWithoutPick();
			AssertGetPickabilityResult("", NotificationTypes.None, true, false, args);
			order.ConsigneeAddressPK = consignee.MainAddress.PK; // clean up

			// consignee address without city
			order.ConsigneeDocAddress.E2_AddressOverride = false;
			order.ConsigneeDocAddress.E2_City = "";
			args = order.GetPickabilityWithoutPick();
			AssertGetPickabilityResult("", NotificationTypes.None, true, false, args);

			// consignee address with city
			order.ConsigneeDocAddress.E2_City = "SYDNEY";
			args = order.GetPickabilityWithoutPick();
			AssertGetPickabilityResult("", NotificationTypes.None, true, false, args);

			// overridden consignee address without city
			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeDocAddress.E2_CompanyName = "ABCD";
			order.ConsigneeDocAddress.E2_City = "";
			args = order.GetPickabilityWithoutPick();
			AssertGetPickabilityResult(string.Format("Consignee on this {0} has validation errors.", order.Description),
				NotificationTypes.Error, false, false, args);

			// overridden consignee address with city
			order.ConsigneeDocAddress.E2_City = "SYDNEY";
			order.ConsigneeDocAddress.E2_Address1 = "#1";
			order.ConsigneeDocAddress.E2_City = "SYDNEY";
			order.ConsigneeDocAddress.E2_RN_NKCountryCode = ZString.Empty;
			args = order.GetPickabilityWithoutPick();
			AssertGetPickabilityResult("", NotificationTypes.None, true, false, args);
		}

		[TestDate(2017, 9, 4)]
		public void TestGetPickability_IsPermitAvailable()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 5m, whs.DefaultLocation, "");

			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			var orderLineWithPermitErrors = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			orderLineWithPermitErrors.CustomsData.WB_CustomsQty = 50m;
			orderLineWithPermitErrors.CustomsData.WB_Tariff = "1020304050";
			Factory.Save();

			var permitService = new Mock<IPermitService>();
			var responseForIsAvailablePermits =
				new WhsPermitWithdrawRequestResponseForTest(orderLineWithPermitErrors, SuccessOrFailure.Failure, 0m,
					null, null);
			permitService.Setup(m => m.IsPermitAvailable(It.IsAny<IEnumerable<IPermitWithdrawRequest>>())).Returns(
				new WhsPermitWithdrawRequestResponseResultForTest
				{
					Responses = new[] { responseForIsAvailablePermits }
				});

			using (ObjectFactory.Substitute(permitService.Object))
			{
				var pick = Factory.New<WhsPick>();
				var args = order.GetPickability(pick);
				var expectedError =
					"Not every Order Line on this Pick could be matched to a weekly estimate. Errors below:\r\nNo weekly estimate found for Order Line 1 (5 UNT of Product P1) (Date: 04-Sep-17, ).";
				AssertGetPickabilityResult(expectedError, NotificationTypes.Error, false, false, args);

				permitService.Verify(m => m.TryGetPermits(It.IsAny<IEnumerable<IPermitWithdrawRequest>>()), Times.Never);
				permitService.Verify(m => m.RelinquishPermitTransactions(It.IsAny<IEnumerable<IPermitTransactionDetail>>()), Times.Never);
			}

			permitService.VerifyAll();
			AssertHasRowWarning("Order Has Warning.", order,
				"Not all Order Lines could be granted a weekly estimate for this Order.");
			AssertHasRowWarning("OrderLine Has Warning.", orderLineWithPermitErrors,
				"No weekly estimate found for Order Line 1 (5 UNT of Product P1) (Date: 04-Sep-17, ).");
		}

		public void TestGetPickability_IsPermitAvailable_OrderLineWithoutPermitIssues()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 5m, whs.DefaultLocation, "");

			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			orderLine.CustomsData.WB_CustomsQty = 50m;
			orderLine.CustomsData.WB_Tariff = "1020304050";
			Factory.Save();

			var permitService = new Mock<IPermitService>();
			var responseForIsAvailablePermits =
				new WhsPermitWithdrawRequestResponseForTest(orderLine, SuccessOrFailure.Success, 0m, "001");
			permitService.Setup(m => m.IsPermitAvailable(It.IsAny<IEnumerable<IPermitWithdrawRequest>>())).Returns(
				new WhsPermitWithdrawRequestResponseResultForTest
				{
					Responses = new[] { responseForIsAvailablePermits }
				});

			using (ObjectFactory.Substitute(permitService.Object))
			{
				var pick = Factory.New<WhsPick>();
				var args = order.GetPickability(pick);
				AssertGetPickabilityResult("", NotificationTypes.None, true, true, args);
				AssertEquals("OutwardEntryNumber must not be updated.", "", orderLine.CustomsData.WB_EntryKey);
				AssertEquals("OutwardEntryLineNumber must not be updated.", ZShort.Zero,
					orderLine.CustomsData.WB_EntryLineNo);
				AssertNoRowWarnings("OrderLine must not have any warnings.", orderLine);

				permitService.Verify(m => m.TryGetPermits(It.IsAny<IEnumerable<IPermitWithdrawRequest>>()), Times.Never);
				permitService.Verify(m => m.RelinquishPermitTransactions(It.IsAny<IEnumerable<IPermitTransactionDetail>>()),
					Times.Never);
			}

			permitService.VerifyAll();
		}

		public void TestGetPickability_IsCustomsTransaction_Repick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouse();
			Helper.CreateProductBOM(data.Part1, data.Part2);

			var order = GetNewBusinessObject();
			order.WD_OH_Client = data.Org1.PK;
			order.WD_WW_Whs = whs.PK;
			order.WD_DocketSubType = OrderType.Codes.Customs;
			order.WD_RequiredDate = ZDateTimeOffset.Now;
			order.ConsigneeAddressPK = data.Org1.MainAddress.PK;

			var line = order.Lines.AddNew();
			line.WE_TransactionQuantity = 5;
			line.WE_OP = data.Part1.PK;
			line.SetShortfallForTest(0);

			var customsData = line.CustomsData;
			customsData.WB_EntryKey = "TBD";
			customsData.WB_EntryLineNo = 0;
			Factory.Save();

			var firstPick = Helper.CreatePickNew(order);
			Factory.Save();

			firstPick.CancelPick();
			Factory.Save();

			var secondPick = Factory.New<WhsPick>();
			var args = order.GetPickability(secondPick);
			AssertGetPickabilityResult(expectedMsg: "This Order has Customs validation errors on its Lines.", expectedMsgType: NotificationTypes.Error, expectedPickability: false, expectedPickExists: false, args);
		}

		#endregion

		#region TestWhsOrderPickabiltyRelatedToDefaultODDL

		public void TestWhsOrderPickabiltyRelatedToDefaultODDL()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1);
			var pick = Factory.New<WhsPick>();
			var args = order.GetPickability(pick);
			AssertGetPickabilityResult("", NotificationTypes.None, true, false, args);

			var oddl = order.Warehouse.WW_DefaultOutboundDockDoor;
			order.Warehouse.WW_DefaultOutboundDockDoor = ZGuid.Empty;
			args = order.GetPickability(pick);
			AssertGetPickabilityResult(
				"Cannot create Pick. Default outbound dock door location is not specified for this warehouse.",
				NotificationTypes.Error, false, false, args);

			order.Warehouse.WW_IsActive = false;
			args = order.GetPickability(pick);
			AssertGetPickabilityResult("", NotificationTypes.None, true, false, args);
			order.Warehouse.WW_IsActive = true;

			order.Warehouse.WW_IsVirtualWarehouse = true;
			args = order.GetPickability(pick);
			AssertGetPickabilityResult("", NotificationTypes.None, true, false, args);
			order.Warehouse.WW_IsVirtualWarehouse = false;

			order.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			args = order.GetPickability(pick);
			AssertGetPickabilityResult("", NotificationTypes.None, true, false, args);

			order.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			args = order.GetPickability(pick);
			AssertGetPickabilityResult(
				"Cannot create Pick. Default outbound dock door location is not specified for this warehouse.",
				NotificationTypes.Error, false, false, args);

			order.Warehouse.WW_DefaultOutboundDockDoor = oddl;
			args = order.GetPickability(pick);
			AssertGetPickabilityResult("", NotificationTypes.None, true, false, args);
		}

		#endregion

		#region TestWhsOrderPickabilty_WhenPalletIDEnteredOnOrderLine

		public void TestWhsOrderPickabilty_WhenPalletIDEnteredOnOrderAndPickHasDynamicPickAreaOverrideSet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bulkLocation = data.Whs1.FindLocation("A-1");
			var dynamicPickFaceLocation = data.Whs1.FindLocation("A-2");
			var dynamicArea = Helper.CreateArea(data.Whs1, "DYNAMIC");
			dynamicPickFaceLocation.WLV_WA_PickingArea = dynamicArea.PK;
			var dynamicLocationType = Helper.CreateLocationType("DLC", LocationClasses.Codes.DPF);
			dynamicPickFaceLocation.WLV_WLT_LocationType = dynamicLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 50m, bulkLocation, "ABC");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 50m, "ABC");

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WA_DynamicPickAreaOverride = dynamicArea.PK;

			var args = order.GetPickability(pick);

			AssertGetPickabilityResult("Pick is using Dynamic Pick Face Replenishment and cannot attach Order with Ordered Pallet ID.", NotificationTypes.Error, false, false, args);
			AssertEquals("Precondition: Should not have attached order.", 0, pick.Orders.Count);
		}

		#endregion

		#region TestResetOrderAfterDetachingFromPick

		public void TestResetOrderAfterDetachingFromPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var orderLine = order.Lines[0];
			orderLine.CustomsData.WB_EntryKey = "ENTRY";
			orderLine.CustomsData.WB_EntryLineNo = 2;
			order.WD_UnitsSent = 4m;
			order.WD_CubicSent = 10m;
			order.WD_PalletsSent = 5;
			order.WD_WeightSentUserEntered = 20m;
			order.WD_WeightSent = 30m;
			order.WD_GS_NKAssignedPacker = "ABC";

			order.ResetOrderAfterDetachingFromPick();
			AssertEquals("Units Sent should be reset.", 0m, order.WD_UnitsSent);
			AssertEquals("Volume Sent should be reset.", 0m, order.WD_CubicSent);
			AssertEquals("Pallets Sent should be reset.", (short)0, order.WD_PalletsSent);
			AssertEquals("Weight Sent - User Entered should be reset.", 0m, order.WD_WeightSentUserEntered);
			AssertEquals("Weight Sent should be reset.", 0m, order.WD_WeightSent);
			AssertEquals("Assigned Packer should be reset.", string.Empty, order.WD_GS_NKAssignedPacker);

			AssertEquals("Outwards Entry Key should be reset.", "", orderLine.CustomsData.WB_EntryKey);
			AssertEquals("Outwards Entry Key should be reset.", (short)0, orderLine.CustomsData.WB_EntryLineNo);
		}

		#endregion

		public override void TestGetLinesToPick()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			WhsOrderLine line = Helper.CreateWhsOrderLine(Order, part, 1m);

			WhsPickableDocketLineCollection linesToPick = Order.GetLinesToPick();
			AssertEquals("Should just pick the standard Order.Lines collection.", Order.Lines, linesToPick);
			AssertCollectionContains(line, linesToPick);
		}

		#endregion

		#region TestCheckIsPalletPackType

		public void TestCheckIsPalletPackType()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");

			AssertEquals("Package Type is a Pallet.", true, order.CheckIsPalletPackType(Constants.PkgUnit.Pallet));

			AssertEquals("Package Type is not a Pallet.", false, order.CheckIsPalletPackType(Constants.PkgUnit.Package));
			AssertEquals("Package Type is not a Pallet.", false, order.CheckIsPalletPackType(Constants.PkgUnit.Carton));
			AssertEquals("Package Type is not a Pallet.", false, order.CheckIsPalletPackType(Constants.PkgUnit.Tote));
			AssertEquals("Package Type is not a Pallet.", false, order.CheckIsPalletPackType(Constants.PkgUnit.Bag));
			AssertEquals("Package Type is not a Pallet.", false, order.CheckIsPalletPackType(Constants.PkgUnit.Box));
			AssertEquals("Package Type is not a Pallet.", false, order.CheckIsPalletPackType(Constants.PkgUnit.Container));

			var palletRefType1 = PackingHelper.CreateRefPackType("PL1", "PL1", 1m, 2m, 3m, Constants.Length.Feet, 7,
				Constants.Weight.Pounds, uomType: UOMPackTypesList.Codes.Pallet);

			AssertEquals("Package Type is a Pallet.", true, order.CheckIsPalletPackType(palletRefType1.F3_Code));
		}

		#endregion

		#region TransportPayerAccount

		public void TestGetTransportPayerAccountWithTransportCo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			AssertEquals(ZString.Empty, order.GetTransportBillToAccountCodeWithTransportCo());

			var tranportCo = Factory.New<OrgHeader>();
			var transportBillTo = Factory.New<OrgHeader>();

			var transportBillToCodeMap = tranportCo.CreatePatternMatchOverrideForTest();
			transportBillToCodeMap.OO_ForeignCode = "T100";
			transportBillToCodeMap.OO_Relationship =
				Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			transportBillToCodeMap.OO_LocalGuid = transportBillTo.PK;

			order.TransportCoPK = tranportCo.PK;
			order.TransportBillToDocAddress.OrganisationPK = transportBillTo.PK;

			AssertEquals("T100", order.GetTransportBillToAccountCodeWithTransportCo());
		}

		#endregion

		#region Fulfillment Rules

		#region WD_WhsOrderFulfillmentRule

		protected override bool SupportsFullfillmentRule
		{
			get { return true; }
		}

		#endregion

		#region OverrideFulfillmentRuleAndLog

		public void TestOverrideFulfillmentRuleAndCreateLog_WithNoPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			order.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			order.OverrideFulfillmentRuleAndCreateLog("123456789", Notify);
			AssertEquals(WhsOrderFulfillmentRuleList.Codes.None, order.WD_WhsOrderFulfillmentRule);
			AssertEquals(1,
				order.Logs.Find(Helper.GetLogFilter(Events.EditedARecord.Code,
					"Fulfillment Rule Overridden - Ref: 123456789")).Length);
		}

		public void TestOverrideFulfillmentRuleAndLog_WithPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			order.WD_RequiredDate = DateTime.Now;
			Helper.CreateWhsPickableDocketLine(order, data.Part1, 7m);
			order.ConsigneePK = Helper.CreateClient().PK;
			Helper.CreatePickNew(order);
			order.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			order.Pick.WP_PickStatus = CodeLists.PickStatus.Codes.Building;

			order.OverrideFulfillmentRuleAndCreateLog(Notify);
			AssertEquals(CodeLists.PickStatus.Codes.Created, order.Pick.WP_PickStatus);
		}

		public void TestOverrideFulfillmentRuleAndLog_RuleAlreadyNone()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketID = "WD1234";
			order.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.None;

			order.OverrideFulfillmentRuleAndCreateLog(Notify);
			AssertEquals(WhsOrderFulfillmentRuleList.Codes.None, order.WD_WhsOrderFulfillmentRule);
			AssertEquals("The Fulfillment Rule is already set to 'None' for this Order: WD1234",
				Notify.LastEvent.Message);
		}

		public void TestOverrideFulfillmentRuleAndLog_NoReferenceProvided()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			order.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			UnitTestUserNotification.Instance.AddUserResponse("987654321");
			order.OverrideFulfillmentRuleAndCreateLog(Notify);
			AssertEquals(WhsOrderFulfillmentRuleList.Codes.None, order.WD_WhsOrderFulfillmentRule);
			AssertEquals(1,
				order.Logs.Find(Helper.GetLogFilter(Events.EditedARecord.Code,
					"Fulfillment Rule Overridden - Ref: 987654321")).Length);
		}

		#endregion

		#region GetOverrideFulfillmentRuleReference

		public void TestGetOverrideFulfillmentRuleReference()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			UnitTestUserNotification.Instance.AddUserResponse("");
			var reference = order.GetOverrideFulfillmentRuleReference(Notify);
			AssertEquals(ZString.Empty, reference);

			UnitTestUserNotification.Instance.AddUserResponse(
				"This reference is longer than 80 characters and should prevent the fulfillment rule from being overridden");
			reference = order.GetOverrideFulfillmentRuleReference(Notify);
			AssertEquals(ZString.Empty, reference);

			UnitTestUserNotification.Instance.AddUserResponse("This reference is less than 80 characters");
			reference = order.GetOverrideFulfillmentRuleReference(Notify);
			AssertEquals("This reference is less than 80 characters", reference);
		}

		#endregion

		#endregion

		#region TestTransportBillToDocAddressDefaultTypes

		public void TestTransportBillToDocAddressDefaultTypes()
		{
			var order = GetNewBusinessObject();
			AssertDocAddressDefaultTypes(order, DocAddressType.TransportBillToAddress,
				order.TransportBillToDocAddressRequirement, ContactType.TransportServices);
			AssertDocAddressDefaultTypes(order, DocAddressType.DistributionCentreAddress,
				order.DistributionCentreDocAddressRequirement, ContactType.NoContactType);
		}

		#endregion

		#region TestDestinationWarehouseDocAddress

		public void TestDestinationWarehouseDocAddress()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			Factory.Save();

			AssertNotNull(order.DestinationWarehouseDocAddress);
			AssertEquals(order.DestinationWarehouseDocAddress.DocAddressType, DocAddressType.DestinationWarehouse);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.MainAddress;

			order.DestinationWarehouseDocAddress.E2_OA_Address = address.PK;
			AssertEquals(org.PK, order.DestinationWarehouseDocAddress.OrganisationPK);
		}

		public void TestDestinationWarehouseDocAddressType()
		{
			var order = GetNewBusinessObject();
			AssertDocAddressDefaultTypes(order, DocAddressType.DestinationWarehouse,
				order.DestinationWarehouseDocAddressRequirement, ContactType.NoContactType);
		}

		#endregion

		#region Finalisation

		#region TestFinaliseDocket_DBHits

		public void TestFinaliseDocket_DBHits()
		{
			const int numberOfInventoriesToCreate = 100;

			var data = new TestDataSimpleEnvironment(Factory, 10, 10);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			for (var i = 0; i < numberOfInventoriesToCreate; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			}

			receive.AllocateLocationsWithMock();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			for (var i = 0; i < numberOfInventoriesToCreate / 2; i++)
			{
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
				AssertNotNull("Precondition - divot was created.",
					orderLine.ReserveStockIfAbleTo(receive.Inventory[i * 2]));
				AssertNotNull("Precondition - divot was created.",
					orderLine.ReserveStockIfAbleTo(receive.Inventory[(i * 2) + 1]));
			}

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();

			using (AssertDbHitsWithUsefulQueryInformation(FinaliseDocket_DBHits_ExpectedDBHitsForValidation,
						otherFactory))
			{
				otherFactory.RefreshEnabled = false;
				var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);

				using (RowFactory.SetCachedTables())
				{
					orderInOtherFactory.RunPreSaveValidation();
					orderInOtherFactory.Validation.ValidateAll();
				}
			}

			var newFactoryForPick = new BusinessObjectFactory();
			newFactoryForPick.RefreshEnabled = false;
			var expectedDBHits = FinaliseDocket_DBHits_ExpectedDBHitsForFinalisation;
			expectedDBHits.Add(GlbStaffSchema.Constants.TableName, 1);
			expectedDBHits.Add(RefPackTypeSchema.Constants.TableName, 1);
			expectedDBHits.Add(JobHeaderSchema.Constants.TableName, 1);
			expectedDBHits.Add(ProcessTaskTemplateSchema.Constants.TableName, 1);
			expectedDBHits.Add(StmEventSchema.Constants.TableName, 1);
			expectedDBHits[GlbBranchSchema.Constants.TableName] += 1;
			expectedDBHits[WhsLocationTypeSchema.Constants.TableName] = 1;

			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, newFactoryForPick))
			{
				var otherHelper2 = new WhsTestHelperFunctions(newFactoryForPick);
				var orderInOtherFactory2 = newFactoryForPick.Load<WhsOrder>(order.PK);

				using (RowFactory.SetCachedTables())
				{
					var pick = otherHelper2.CreatePickNew(orderInOtherFactory2);
					pick.FinaliseAllOrders();
				}

				AssertEquals(true, orderInOtherFactory2.IsFinalised);
			}
		}

		Dictionary<string, int> FinaliseDocket_DBHits_ExpectedDBHitsForValidation
		{
			get
			{
				var expectedDBHits = TestFinaliseDocket_DBHits_ExpectedDBHitsForValidation;
				expectedDBHits[PkgPackageSchema.Constants.TableName] -= 1;
				expectedDBHits.Add(RefCountrySchema.Constants.TableName, 1);
				return expectedDBHits;
			}
		}

		Dictionary<string, int> FinaliseDocket_DBHits_ExpectedDBHitsForFinalisation
		{
			get
			{
				var expectedDBHits = TestFinaliseDocket_DBHits_ExpectedDBHitsForValidation;
				expectedDBHits[WhsDocketSchema.Constants.TableName] += 1;
				expectedDBHits[WhsDocketLineSchema.Constants.TableName] += 1;
				expectedDBHits[WhsPickLineSchema.Constants.TableName] -= 2;
				expectedDBHits[RefPacksSchema.Constants.TableName] -= 1;
				expectedDBHits[WhsLocationViewSchema.Constants.TableName] -= 1;
				expectedDBHits[WhsInventoryViewSchema.Constants.TableName] +=
					1; // 1 more DBHits during CreatePickNew, not from Finalisation
				expectedDBHits[WhsRowSchema.Constants.TableName] -= 1;
				expectedDBHits.Add(WhsDocketReferenceSchema.Constants.TableName, 1);
				expectedDBHits.Add(WhsDocketPalletSchema.Constants.TableName, 1);
				expectedDBHits.Add(WhsLocationTypeSchema.Constants.TableName, 2);
				expectedDBHits.Add(WhsPickFaceSchema.Constants.TableName, 1);
				expectedDBHits.Add(ProcessTasksSchema.Constants.TableName, 1);
				expectedDBHits.Add(JobServiceSchema.Constants.TableName, 1);
				expectedDBHits.Add(ProcessTaskNotificationSchema.Constants.TableName, 1);
				expectedDBHits.Add(GenCustomAddOnRuleAckSchema.Constants.TableName, 1);
				expectedDBHits.Add(WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1);
				expectedDBHits.Add(GlbCompanySchema.Constants.TableName, 1);
				expectedDBHits.Add(RefCountrySchema.Constants.TableName, 1);
				expectedDBHits.Add(ProcessWorkflowExceptionTypeSchema.Constants.TableName, 1);

				return expectedDBHits;
			}
		}

		protected virtual Dictionary<string, int> TestFinaliseDocket_DBHits_ExpectedDBHitsForValidation
		{
			get
			{
				return new Dictionary<string, int>()
				{
					{ GlbBranchSchema.Constants.TableName, 1 },
					{ JobDocAddressSchema.Constants.TableName, 1 },
					{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
					{ OrgAddressSchema.Constants.TableName, 3 },
					{ OrgCompanyDataSchema.Constants.TableName, 1 },
					{ OrgContactSchema.Constants.TableName, 1 },
					{ OrgCusCodeSchema.Constants.TableName, 1 },
					{ OrgCustomLabelsSchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ PkgPackageJobSchema.Constants.TableName, 1 },
					{ PkgPackageSchema.Constants.TableName, 1 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgPartRelationSchema.Constants.TableName, 1 },
					{ OrgPartUnitSchema.Constants.TableName, 1 },
					{ OrgSupplierPartSchema.Constants.TableName, 1 },
					{ RefPacksSchema.Constants.TableName, 1 },
					{ RefTimeZoneSchema.Constants.TableName, 1 },
					{ RefTimeZoneSetSchema.Constants.TableName, 1 },
					{ RefUNLOCOSchema.Constants.TableName, 1 },
					{ WhsAreaSchema.Constants.TableName, 1 },
					{ WhsDocketContainerSchema.Constants.TableName, 1 },
					{ WhsDocketSchema.Constants.TableName, 1 },
					{ WhsDocketLineSchema.Constants.TableName, 3 },
					{ WhsInventoryViewSchema.Constants.TableName, 2 },
					{ WhsLocationViewSchema.Constants.TableName, 2 },
					{ WhsPickLineSchema.Constants.TableName, 3 },
					{ WhsRowSchema.Constants.TableName, 1 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
				};
			}
		}

		protected virtual Dictionary<string, int> TestFinaliseDocket_DBHits_ExpectedDBHitsForFinalisation
		{
			get
			{
				var expectedDBHits = TestFinaliseDocket_DBHits_ExpectedDBHitsForValidation;
				expectedDBHits[WhsDocketSchema.Constants.TableName] += 1;
				expectedDBHits[WhsDocketLineSchema.Constants.TableName] += 1;
				expectedDBHits[JobDocAddressSchema.Constants.TableName] += 1;
				expectedDBHits[WhsPickLineSchema.Constants.TableName] -= 2;
				expectedDBHits[RefPacksSchema.Constants.TableName] -= 1;
				expectedDBHits[WhsLocationViewSchema.Constants.TableName] -= 1;
				expectedDBHits[WhsInventoryViewSchema.Constants.TableName] +=
					1; // 1 more DBHits during CreatePickNew, not from Finalisation
				expectedDBHits[WhsRowSchema.Constants.TableName] -= 1;
				expectedDBHits.Add(PkgPackageJobSchema.Constants.TableName, 1);
				expectedDBHits.Add(PkgPackageSchema.Constants.TableName, 1);
				expectedDBHits.Add(WhsDocketReferenceSchema.Constants.TableName, 1);
				expectedDBHits.Add(WhsDocketPalletSchema.Constants.TableName, 1);
				expectedDBHits.Add(WhsLocationTypeSchema.Constants.TableName, 2);
				expectedDBHits.Add(WhsPickFaceSchema.Constants.TableName, 1);
				expectedDBHits.Add(ProcessTasksSchema.Constants.TableName, 1);
				expectedDBHits.Add(JobServiceSchema.Constants.TableName, 1);
				expectedDBHits.Add(ProcessTaskNotificationSchema.Constants.TableName, 1);
				expectedDBHits.Add(GenCustomAddOnRuleAckSchema.Constants.TableName, 1);
				expectedDBHits.Add(WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1);
				return expectedDBHits;
			}
		}

		#endregion

		#region TestFinaliseDocket_DBHits_WithPackageAudit

		public void TestFinaliseDocket_DBHits_WithPackageAudit()
		{
			const int numberOfInventoriesToCreate = 100;
			const int numberOfPackagesToCreate = 100;

			var data = new TestDataSimpleEnvironment(Factory, 10, 10);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			for (int i = 0; i < numberOfInventoriesToCreate; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			}

			receive.AllocateLocationsWithMock();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_QualityAuditRequired = true;
			for (int i = 0; i < numberOfInventoriesToCreate / 2; i++)
			{
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
				AssertNotNull("Precondition - divot was created.",
					orderLine.ReserveStockIfAbleTo(receive.Inventory[i * 2]));
				AssertNotNull("Precondition - divot was created.",
					orderLine.ReserveStockIfAbleTo(receive.Inventory[(i * 2) + 1]));
			}

			Factory.Save();

			Helper.CreatePickNew(order);
			for (var i = 0; i < numberOfPackagesToCreate; i++)
			{
				var package = order.PackageJob.Packages.AddNew();
				package.KP_PackageID = $"P{i}";
				WhsPackageAuditManager.AuditPackage(package);
			}

			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			using (AssertDbHitsWithUsefulQueryInformation(
					   TestFinaliseDocket_DBHits_ExpectedDBHitsForValidation_WithPackageAudit, otherFactory))
			using (RowFactory.SetCachedTables())
			{
				var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);

				orderInOtherFactory.RunPreSaveValidation();
				orderInOtherFactory.Validation.ValidateAll();
			}

			var newFactoryForPick = new BusinessObjectFactory { RefreshEnabled = false };
			var expectedDBHits = TestFinaliseDocket_DBHits_ExpectedDBHitsForFinalisation_WithPackageAudit;
			expectedDBHits.Remove(OrgPartUnitSchema.Constants.TableName);
			using (AssertDbHitsWithUsefulQueryInformation(
					   expectedDBHits, newFactoryForPick))
			using (RowFactory.SetCachedTables())
			{
				var orderInOtherFactory2 = newFactoryForPick.Load<WhsOrder>(order.PK);

				orderInOtherFactory2.Pick.FinaliseAllOrders();
				AssertEquals(true, orderInOtherFactory2.IsFinalised);
			}
		}

		protected virtual Dictionary<string, int> TestFinaliseDocket_DBHits_ExpectedDBHitsForValidation_WithPackageAudit
		{
			get
			{
				return new Dictionary<string, int>()
				{
					{ GlbBranchSchema.Constants.TableName, 1 },
					{ JobDocAddressSchema.Constants.TableName, 1 },
					{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
					{ OrgAddressSchema.Constants.TableName, 3 },
					{ OrgCompanyDataSchema.Constants.TableName, 1 },
					{ OrgContactSchema.Constants.TableName, 1 },
					{ OrgCusCodeSchema.Constants.TableName, 1 },
					{ OrgCustomLabelsSchema.Constants.TableName, 1 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgPartRelationSchema.Constants.TableName, 1 },
					{ OrgPartUnitSchema.Constants.TableName, 1 },
					{ OrgSupplierPartSchema.Constants.TableName, 1 },
					{ RefPacksSchema.Constants.TableName, 1 },
					{ WhsAreaSchema.Constants.TableName, 1 },
					{ WhsDocketContainerSchema.Constants.TableName, 1 },
					{ WhsDocketSchema.Constants.TableName, 2 },
					{ WhsDocketLineSchema.Constants.TableName, 3 },
					{ WhsInventoryViewSchema.Constants.TableName, 2 },
					{ WhsPickSchema.Constants.TableName, 1 },
					{ WhsPickLineSchema.Constants.TableName, 1 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
					{ PkgPackageSchema.Constants.TableName, 1 },
					{ PkgPackageJobSchema.Constants.TableName, 1 },
					{ RefCountrySchema.Constants.TableName, 1 },
					{ RefTimeZoneSchema.Constants.TableName, 1 },
					{ RefTimeZoneSetSchema.Constants.TableName, 1 },
					{ RefUNLOCOSchema.Constants.TableName, 1 },
				};
			}
		}

		protected virtual Dictionary<string, int>
			TestFinaliseDocket_DBHits_ExpectedDBHitsForFinalisation_WithPackageAudit
		{
			get
			{
				var expectedDBHits = TestFinaliseDocket_DBHits_ExpectedDBHitsForValidation;
				expectedDBHits[RefPacksSchema.Constants.TableName] -= 1;
				expectedDBHits[WhsDocketSchema.Constants.TableName] += 1;
				expectedDBHits[WhsDocketLineSchema.Constants.TableName] += 1;
				expectedDBHits[WhsLocationViewSchema.Constants.TableName] -= 2;
				expectedDBHits[PkgPackageSchema.Constants.TableName] += 2;
				expectedDBHits[WhsRowSchema.Constants.TableName] -= 1;
				expectedDBHits[WhsPickLineSchema.Constants.TableName] -= 2;
				expectedDBHits[GlbBranchSchema.Constants.TableName] += 1;

				expectedDBHits.Add(WhsDocketReferenceSchema.Constants.TableName, 1);
				expectedDBHits.Add(WhsDocketPalletSchema.Constants.TableName, 1);
				expectedDBHits.Add(ProcessTasksSchema.Constants.TableName, 1);
				expectedDBHits.Add(JobServiceSchema.Constants.TableName, 1);
				expectedDBHits.Add(ProcessTaskNotificationSchema.Constants.TableName, 1);
				expectedDBHits.Add(GenCustomAddOnRuleAckSchema.Constants.TableName, 1);
				expectedDBHits.Add(PkgPackageHeaderSchema.Constants.TableName, 2);
				expectedDBHits.Add(PkgPackageItemDivotSchema.Constants.TableName, 2);
				expectedDBHits.Add(PkgPackageJobPackageHeaderPivotSchema.Constants.TableName, 1);
				expectedDBHits.Add(UNDGDataItemSchema.Constants.TableName, 2);
				expectedDBHits.Add(WhsPackageAuditSchema.Constants.TableName, 1);
				expectedDBHits.Add(WhsPickSchema.Constants.TableName, 1);
				expectedDBHits.Add(GenAddOnColumnSchema.Constants.TableName, 3);
				expectedDBHits.Add(WhsLoadPkgPackagePivotSchema.Constants.TableName, 2);
				expectedDBHits.Add(WhsPickTrolleySlotSchema.Constants.TableName, 2);
				expectedDBHits.Add(WhsPickByLabelLabelSchema.Constants.TableName, 2);
				expectedDBHits.Add(StmDefaultPrinterSchema.Constants.TableName, 2);
				expectedDBHits.Add(GlbCompanySchema.Constants.TableName, 1);
				expectedDBHits.Add(RefCountrySchema.Constants.TableName, 1);
				expectedDBHits.Add(RefPackTypeSchema.Constants.TableName, 1);
				expectedDBHits.Add(GlbStaffSchema.Constants.TableName, 1);
				expectedDBHits.Add(ProcessWorkflowExceptionTypeSchema.Constants.TableName, 1);
				expectedDBHits.Add(JobHeaderSchema.Constants.TableName, 1);
				expectedDBHits.Add(ProcessTaskTemplateSchema.Constants.TableName, 1);
				expectedDBHits.Add(StmEventSchema.Constants.TableName, 1);

				return expectedDBHits;
			}
		}

		#endregion

		#region TestFinalisedDateOverride

		[TestDate(2005, 1, 20)]
		public void TestFinalisedDateOverride()
		{
			var order = SetupForTestFinaliseDocket();
			order.Warehouse.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			order.WD_RequiredDate = new ZDateTimeOffset(2004, 2, 15);
			order.FinaliseDocket();
			AssertEquals(2004, order.WD_FinalisedDate.Year);
			AssertEquals(2, order.WD_FinalisedDate.Month);
			AssertEquals(15, order.WD_FinalisedDate.Day);
		}

		#endregion

		#region TestFinaliseDocket_AbortsAndNotifiesUserIfRunRreFinaliseValidationFails

		public override void TestFinaliseDocket_AbortsAndNotifiesUserIfRunRreFinaliseValidationFails()
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			var order = Helper.CreateWhsOrder(org, whs);
			order.ConsigneePK = org.PK;
			order.ConsigneeAddressPK = org.MainAddress.PK;
			AssertEquals("Precondition", false, order.IsFinalised);

			order.Lines.AddNew(); // need a line to pick
			WhsPick pick = Factory.New<WhsPick>();
			pick.PickOrders(new WhsPickableDocket[] { order });

			order.FinaliseDocket();

			AssertEquals(false, order.IsFinalised);
			AssertEquals(true, ((NotificationBuffer)order.NotificationManager.Peek).HasErrors);
		}

		#endregion

		#region TestFinaliseDocket_FinalisationIsNotAllowed

		public void TestFinaliseDocket_FinalisationIsNotAllowed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			var expectedErrorMessage = "Cannot be finalized until Customs send an ACCEPT event.";

			// Normal Orders
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order.Logs.AddNew(Events.HoldTheWarehouseOrder);
			var pick = Helper.CreatePickNew(order);
			pick.FinaliseAllOrders();
			AssertEquals(true, order.IsFinalised);
			AssertNoRowError(order, expectedErrorMessage);

			// Bonded Order
			Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			var bondedOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 10m);
			bondedOrder.WD_DocketSubType = OrderType.Codes.Customs;
			bondedOrder.Lines[0].WE_BondedEntryKey = "123-1";
			bondedOrder.Lines[0].CustomsData.WB_EntryKey = "DummyOutward";
			bondedOrder.Lines[0].CustomsData.WB_EntryLineNo = 1;
			bondedOrder.Logs.AddNew(Events.HoldTheWarehouseOrder);
			var bondedPick = Helper.CreatePickNew(bondedOrder);
			bondedPick.FinaliseAllOrders();
			AssertEquals(false, bondedOrder.IsFinalised);
			AssertHasRowError(bondedOrder, expectedErrorMessage);

			// we want to set the utc date to be later then the 'hold' event's utc date
			var allowFinaliseLog = bondedOrder.Logs.AddNew(Events.WarehouseJobCanNowBeFinalised);
			Helper.SetLogUTCTimeOnFactorySave(TestConnection, allowFinaliseLog, ZDateTime.Now.AddDays(1));

			Factory.Save(); // to set Posted Time

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var bondedOrderInNewFactory = newFactory.Load<WhsOrder>(bondedOrder.PK);
			var bondedPickInNewFactory = newFactory.Load<WhsPick>(bondedPick.PK);

			bondedPickInNewFactory.FinaliseAllOrders();
			AssertEquals(true, bondedOrderInNewFactory.IsFinalised);
			AssertNoRowError(bondedOrderInNewFactory, expectedErrorMessage);
		}

		#endregion

		#region TestFinaliseDocket_PrintsPackingSlip

		public void TestFinaliseDocket_PrintsPackingSlip()
		{
			bool isUserInteractive = true;
			AssertFinaliseDocket_PrintsPackingSlip(isUserInteractive);
		}

		public void TestFinaliseDocket_PrintsNoPackingSlipIfNotInteractive()
		{
			bool isUserInteractive = false;
			AssertFinaliseDocket_PrintsPackingSlip(isUserInteractive);
		}

		void AssertFinaliseDocket_PrintsPackingSlip(bool testRunningAsUserInteractive)
		{
			bool userInteractiveBeforeTest = Globals.IsUserInteractive;
			try
			{
				Globals.IsUserInteractive = testRunningAsUserInteractive && !Globals.IsWeb;

				var order = SetupForTestFinaliseDocket();
				order.Warehouse.WW_AutoPrintPackingSlip = true;

				WhsDocumentPrinter.LastPrintedDocumentName = "";

				order.FinaliseDocket();
				var printer = new WhsPackingSlipDocumentPrinter(order, new TestNotificationBuffer());

				if (testRunningAsUserInteractive && !Globals.IsWeb)
				{
					AssertNotEquals("", WhsDocumentPrinter.LastPrintedDocumentName);
					AssertEquals(printer.DocumentMenuName, WhsDocumentPrinter.LastPrintedDocumentName);
				}
				else
				{
					AssertEquals("", WhsDocumentPrinter.LastPrintedDocumentName);
				}
			}
			finally
			{
				Globals.IsUserInteractive = userInteractiveBeforeTest;
			}
		}

		#endregion

		#region TestFinaliseDocket_DoesNotFinalisePick_WhenAutoFinaliseIsFalse

		public void TestFinaliseDocket_DoesNotFinalisePick_WhenAutoFinaliseIsFalse()
		{
			WarehouseDataRegistry.Instance.AutoFinalizePick.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var order = SetupForTestFinaliseDocket();
			AssertEquals("Precondition", false, order.IsFinalised);
			AssertEquals("Precondition", false, order.Pick.IsFinalised);

			order.FinaliseDocket();
			AssertEquals(true, order.IsFinalised);
			AssertEquals(false, order.Pick.IsFinalised);
		}

		#endregion

		#region TestFinaliseDocket_FinalisesPick_WhenAutoFinaliseIsTrue

		public void TestFinaliseDocket_FinalisesPick_WhenAutoFinaliseIsTrue()
		{
			WarehouseDataRegistry.Instance.AutoFinalizePick.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var order = SetupForTestFinaliseDocket();
			AssertEquals("Precondition", false, order.IsFinalised);
			AssertEquals("Precondition", false, order.Pick.IsFinalised);

			order.FinaliseDocket();
			AssertEquals(true, order.IsFinalised);
			AssertEquals(true, order.Pick.IsFinalised);
		}

		#endregion

		#region TestFinaliseDocket_RunPreFinaliseValidation_WithFulfillmentRuleMet

		public void TestFinaliseDocket_RunPreFinaliseValidation_WithFulfillmentRuleMet()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(); // 100 units received

			WhsOrder order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 150m);
			order.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			Factory.Save();

			WhsPick pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - Pick failed", true, order.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition - No error should exist.", false,
				order.RowErrors.Contains("Order cannot be finalized until Fulfillment Rule is met"));

			order.FinaliseDocket();
			AssertEquals(true, order.RowErrors.Contains("Order cannot be finalized until Fulfillment Rule is met"));
			AssertEquals(false, order.IsFinalised);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);

			// check that RowNotification has been cleared from the previous Run
			orderInNewFactory.Lines[0].WE_TransactionQuantity = 100m;
			orderInNewFactory.Lines[0].ClearWE_ShortfallQuantityCached();
			orderInNewFactory.FinaliseDocket();
			AssertEquals(false,
				orderInNewFactory.RowErrors.Contains("Order cannot be finalized until Fulfillment Rule is met"));
			AssertEquals(true, orderInNewFactory.IsFinalised);
		}

		#endregion

		#region TestFinaliseDocket_NotAllowed_PackagesAreHeld

		public void TestFinaliseDocket_NotAllowed_PackagesAreHeld()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_IsHeld = true;
			var subPackage = package.Packages.AddNew();
			subPackage.KP_IsHeld = true;

			Factory.Save();

			order.FinaliseDocket();
			AssertEquals("The order should not be finalized because there should be an error", false,
				order.IsFinalised);
			AssertHasRowError(order,
				$"There is a failed audit for Order {order.WD_DocketID}, or one or more Packages are Held. Remove the Hold Status on Held Packages to finalize the Order.");

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			var packageInNewFactory = newFactory.Load<PkgPackage>(package.PK);

			packageInNewFactory.KP_IsHeld = false;
			orderInNewFactory.FinaliseDocket();
			AssertEquals("The order should not be finalized because there should be an error", false,
				orderInNewFactory.IsFinalised);
			AssertHasRowError(orderInNewFactory,
				$"There is a failed audit for Order {orderInNewFactory.WD_DocketID}, or one or more Packages are Held. Remove the Hold Status on Held Packages to finalize the Order.");
		}

		#endregion

		#region TestFinaliseDocket_WithQualityAuditRequired_NotAllowed

		#region PackageHasNotBeenAudited

		public void TestFinaliseDocket_WithQualityAuditRequired_NotAllowed_PackageHasNotBeenAudited()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			order.WD_QualityAuditRequired = true;
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "P1";

			Factory.Save();

			order.FinaliseDocket();
			AssertEquals(false, order.IsFinalised);
			AssertHasRowError(order,
				$"Order {order.WD_DocketID} requires a Package audit and not all Packages have been audited or it has audit failures.");

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			var packageInNewFactory = newFactory.Load<PkgPackage>(package.PK);

			WhsPackageAuditManager.AuditPackage(packageInNewFactory);
			orderInNewFactory.FinaliseDocket();
			AssertEquals(true, orderInNewFactory.IsFinalised);
			AssertNoRowErrors(orderInNewFactory);
		}

		#endregion

		#region PackageHasFailingAudits

		public void TestFinaliseDocket_WithQualityAuditRequired_NotAllowed_PackageHasFailingAudits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			order.WD_QualityAuditRequired = true;
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "P1";
			package.KP_IsHeld = true;
			var completeTimeoffset = new ZDateTimeOffset(ZDateTime.BrettsBirthday);

			var audit = WhsPackageAuditManager.AuditPackage(package, completeTimeoffset, GlbStaff.CurrentUser);
			var packageAuditFailureLine = audit.PackageAuditFailureLines.AddNew();
			packageAuditFailureLine.WPF_OP = data.Part1.PK;
			packageAuditFailureLine.WPF_ExpectedQty = 5m;
			packageAuditFailureLine.WPF_AuditedQty = 10m;

			Factory.Save();

			order.FinaliseDocket();
			AssertEquals(false, order.IsFinalised);
			AssertHasRowError(order,
				$"There is a failed audit for Order {order.WD_DocketID}, or one or more Packages are Held. Remove the Hold Status on Held Packages to finalize the Order.");

			var newFactory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var packageInNewFactory1 = newFactory1.Load<PkgPackage>(package.PK);
			var orderInNewFactory1 = newFactory1.Load<WhsOrder>(order.PK);

			WhsPackageAuditManager.AuditPackage(packageInNewFactory1);
			newFactory1.Save();

			orderInNewFactory1.FinaliseDocket();
			AssertEquals(
				"A successful audit should not clear the hold on the packages, therefore, the order can't be finalized.",
				false, orderInNewFactory1.IsFinalised);
			AssertHasRowError(orderInNewFactory1,
				$"There is a failed audit for Order {orderInNewFactory1.WD_DocketID}, or one or more Packages are Held. Remove the Hold Status on Held Packages to finalize the Order.");

			var newFactory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var orderInNewFactory2 = newFactory2.Load<WhsOrder>(order.PK);

			orderInNewFactory2.PackageJob.RemoveHoldOfAllPackages();
			orderInNewFactory2.FinaliseDocket();
			AssertEquals("After removing the hold, we should be able to finalize the order.", true,
				orderInNewFactory2.IsFinalised);
			AssertNoRowErrors(orderInNewFactory2);
		}

		#endregion

		#region PackageHasFailingAuditsHoldRemoved

		public void TestFinaliseDocket_WithQualityAuditRequired_Allowed_HoldRemovedAfterFailingAudit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			order.WD_QualityAuditRequired = true;
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "P1";
			package.KP_IsHeld = true;

			var completeTimeoffset = new ZDateTimeOffset(ZDateTime.BrettsBirthday);
			var audit = WhsPackageAuditManager.AuditPackage(package, completeTimeoffset, GlbStaff.CurrentUser);
			var packageAuditFailureLine = audit.PackageAuditFailureLines.AddNew();
			packageAuditFailureLine.WPF_OP = data.Part1.PK;
			packageAuditFailureLine.WPF_ExpectedQty = 5m;
			packageAuditFailureLine.WPF_AuditedQty = 10m;

			Factory.Save();

			order.FinaliseDocket();
			AssertEquals("Finalization should not be allowed since we have a hold on package.", false,
				order.IsFinalised);
			AssertHasRowError(order,
				$"There is a failed audit for Order {order.WD_DocketID}, or one or more Packages are Held. Remove the Hold Status on Held Packages to finalize the Order.");

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);

			orderInNewFactory.PackageJob.RemoveHoldOfAllPackages();
			orderInNewFactory.FinaliseDocket();
			AssertEquals(
				"After removing the hold, we should be able to finalize the order even when failures in audit persist.",
				true, orderInNewFactory.IsFinalised);
			AssertNoRowErrors(orderInNewFactory);
		}

		#endregion

		#region NonContainerOutersAndFirstLevelPackagesOnContainersWithoutPackageID

		public void
			TestFinaliseDocket_WithQualityAuditRequired_NotAllowed_NonContainerOutersAndFirstLevelPackagesOnContainersWithoutPackageID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			order.WD_QualityAuditRequired = true;
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			var container = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Container);
			var containerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container.Container.K0_RC_ContainerType = containerType.PK;
			var packageInsideContainer = container.Packages.AddNew();

			Factory.Save();

			order.FinaliseDocket();
			AssertEquals("The order should not be finalized because there should be an error", false,
				order.IsFinalised);
			AssertHasRowError(order,
				$"The Order {order.WD_DocketID} requires Quality Audit but not all the outer packages have an ID. All the outer packages must have an ID to complete the audits.");

			var newFactory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var orderInNewFactory1 = newFactory1.Load<WhsOrder>(order.PK);
			var packageInNewFactory1 = newFactory1.Load<PkgPackage>(package.PK);

			packageInNewFactory1.KP_PackageID = "P001";
			newFactory1.Save();

			orderInNewFactory1.FinaliseDocket();
			AssertEquals(
				"The order should not be finalized because there should be an error, the other package has ID but not the first level package of the container",
				false, orderInNewFactory1.IsFinalised);
			AssertHasRowError(orderInNewFactory1,
				$"The Order {orderInNewFactory1.WD_DocketID} requires Quality Audit but not all the outer packages have an ID. All the outer packages must have an ID to complete the audits.");

			var newFactory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var orderInNewFactory2 = newFactory2.Load<WhsOrder>(order.PK);
			var packageInsideContainerInNewFactory2 = newFactory2.Load<PkgPackage>(packageInsideContainer.PK);

			packageInsideContainerInNewFactory2.KP_PackageID = "P002";
			orderInNewFactory2.FinaliseDocket();
			AssertEquals(
				"The order should not be finalized because it has not been audited yet but no it has packages with",
				false, orderInNewFactory2.IsFinalised);
			AssertNoRowErrorContaining(orderInNewFactory2,
				$"The Order {orderInNewFactory2.WD_DocketID} requires Quality Audit but not all the outer packages have an ID. All the outer packages must have an ID to complete the audits.");
		}

		#endregion

		#endregion

		#region TestFinaliseDocket_WithIsLoadingRequired

		public void TestFinaliseDocket_WithIsLoadingRequired()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			order.WD_IsLoadingRequired = true;
			Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "P1";

			Factory.Save();

			order.FinaliseDocket();
			AssertEquals(false, order.IsFinalised);
			AssertHasRowError(order, $@"Order {order.WD_DocketID} requires Loading and not all Packages have been Loaded:
Package 'P1' for Load 'Unassigned'");

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var helperInNewFactory = new WhsTestHelperFunctions(newFactory);
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);

			// Load all Packages
			var transportCo = helperInNewFactory.CreateClient("CARRIER");
			var load = helperInNewFactory.CreateWhsLoad(transportCo, orderInNewFactory.Warehouse.DefaultOutboundDockDoorLocation);
			load.WLO_TransportationUnitNumber = "1234";
			var pivot = helperInNewFactory.CreateLoadPkgPackagePivot(package.PK, load);
			pivot.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot.WLP_GS_NKLoadingUser = "E";
			load.WLO_StartTime = ZDateTimeOffset.Now;

			newFactory.Save();

			orderInNewFactory.FinaliseDocket();
			AssertEquals(true, orderInNewFactory.IsFinalised);
			AssertNoRowErrors(orderInNewFactory);
		}

		public void TestFinaliseDocket_WithIsLoadingRequired_FailsIfAnyPackageIsNotLoaded()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			order.WD_IsLoadingRequired = true;
			Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew();
			var package2 = order.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P1";
			package2.KP_PackageID = "P2";

			Factory.Save();

			var transportCo = Helper.CreateClient("CARRIER");
			var load = Helper.CreateWhsLoad(transportCo, data.Whs1.DefaultOutboundDockDoorLocation);
			load.WLO_TransportationUnitNumber = "1234";
			var pivot = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			pivot.WLP_LoadedTime = ZDateTimeOffset.Now;
			pivot.WLP_GS_NKLoadingUser = "E";
			load.WLO_StartTime = ZDateTimeOffset.Now;

			Factory.Save();

			order.FinaliseDocket();
			AssertEquals(false, order.IsFinalised);
			AssertHasRowError(order, $@"Order {order.WD_DocketID} requires Loading and not all Packages have been Loaded:
Package 'P2' for Load 'Unassigned'");
		}

		public void TestFinaliseDocket_WithIsLoadingRequired_FailsIfNoPackagesOnOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			order.WD_IsLoadingRequired = true;
			Helper.CreatePickNew(order);

			Factory.Save();

			order.FinaliseDocket();
			AssertEquals(false, order.IsFinalised);
			AssertHasRowError(order,
				$"Order {order.WD_DocketID} requires Loading and Order has no Packages.");
		}

		public void TestFinaliseDocket_WithIsLoadingRequired_FailsIfAnyPackageIsNotLoaded_MultipleLoads()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			order.WD_IsLoadingRequired = true;
			Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew();
			var package2 = order.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P1";
			package2.KP_PackageID = "P2";

			Factory.Save();

			var transportCo = Helper.CreateClient("CARRIER");
			var load1 = Helper.CreateWhsLoad(transportCo, data.Whs1.DefaultOutboundDockDoorLocation);
			var load2 = Helper.CreateWhsLoad(transportCo, data.Whs1.DefaultOutboundDockDoorLocation);
			load1.WLO_TransportationUnitNumber = "1234";
			load2.WLO_TransportationUnitNumber = "5678";

			Helper.CreateLoadPkgPackagePivot(package1.PK, load1);
			load1.WLO_StartTime = ZDateTimeOffset.Now;
			order.WD_WLO_PlannedLoad = load2.PK;

			Factory.Save();

			order.FinaliseDocket();
			AssertEquals(false, order.IsFinalised);
			AssertHasRowError(order, $@"Order {order.WD_DocketID} requires Loading and not all Packages have been Loaded:
Package 'P1' for Load '{load1.WLO_JobID}'
Package 'P2' for Load '{load2.WLO_JobID}'");
		}

		#endregion

		#region TestFinaliseDocket_HoldTheWarehouseOrderEvent_IsAllowed

		public void TestFinaliseDocket_HoldTheWarehouseOrderEvent_IsAllowed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			order.Logs.AddNew(Events.HoldTheWarehouseOrder);
			Factory.Save();

			using (WarehouseDataRegistry.Instance.AllowOrderToBeFinalisedWithoutCustomsClearance.SetTemporaryValue(
					   Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Even though the Hold event exists, registry should allow finalisation.", true,
					order.IsFinaliseAllowed);
			}

			AssertEquals("If registry is false Hold event should prevent finalisation.", false,
				order.IsFinaliseAllowed);
		}

		#endregion

		#region TestPropagatesFinalisedStatusChangeToLines

		[TestDate(2024, 11, 21)]
		public void TestPropagatesFinalisedStatusChangeToLines_Departed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part2, 3m);

			AssertEquals(DocketStatus.Codes.New, order1.WD_DocketStatus);
			AssertEquals("", orderLine1.WE_DocketLineStatus);
			AssertEquals("", orderLine2.WE_DocketLineStatus);
			order1.WD_DocketStatus = WhsOrderStatus.Codes.Departed;
			order1.WD_FinalisedDate = DateTime.Now;

			AssertEquals("Should propagate departed status to order lines", DocketLineStatus.Codes.Departed, orderLine1.WE_DocketLineStatus);
			AssertEquals("Should propagate departed status to order lines", DocketLineStatus.Codes.Departed, orderLine2.WE_DocketLineStatus);
		}

		[TestDate(2024, 11, 21)]
		public void TestPropagatesFinalisedStatusChangeToLines_Finalised()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part2, 3m);

			AssertEquals(DocketStatus.Codes.New, order1.WD_DocketStatus);
			AssertEquals("", orderLine1.WE_DocketLineStatus);
			AssertEquals("", orderLine2.WE_DocketLineStatus);
			order1.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			order1.WD_FinalisedDate = DateTime.Now;

			AssertEquals("Should propagate finalised status to order lines", DocketLineStatus.Codes.Finalised, orderLine1.WE_DocketLineStatus);
			AssertEquals("Should propagate finalised status to order lines", DocketLineStatus.Codes.Finalised, orderLine2.WE_DocketLineStatus);
		}

		#endregion

		#region TestFinaliseDocket_OrderLinesFinaliseDataAndStatus

		[TestDate(2022, 11, 16)]
		public void TestFinaliseDocket_OrderLinesFinaliseDataAndStatus_OnSet()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order1, data.Part2, 3m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine3 = Helper.CreateWhsOrderLine(order2, data.Part1, 2m);
			var orderLine4 = Helper.CreateWhsOrderLine(order2, data.Part2, 4m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order1);
			AssertEquals("Order not picked", true, order1.IsAttachedToPickButNotFinalised);
			Factory.Save();

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order1);
			AssertEquals("OrderLine1 correct WE_FinalisedDate", ZDateTimeOffset.Now, orderLine1.WE_FinalisedDate);
			AssertEquals("OrderLine1 correct WE_DocketLineStatus", DocketLineStatus.Codes.Finalised, orderLine1.WE_DocketLineStatus);
			AssertEquals("OrderLine2 correct WE_FinalisedDate", ZDateTimeOffset.Now, orderLine2.WE_FinalisedDate);
			AssertEquals("OrderLine2 correct WE_DocketLineStatus", DocketLineStatus.Codes.Finalised, orderLine2.WE_DocketLineStatus);

			AssertEquals("Order2 not finalised", false, order2.IsFinalised);
			AssertEquals("OrderLine3 correct WE_FinalisedDate", ZDateTimeOffset.Empty, orderLine3.WE_FinalisedDate);
			AssertEquals("OrderLine3 correct WE_DocketLineStatus", ZString.Empty, orderLine3.WE_DocketLineStatus);
			AssertEquals("OrderLine4 correct WE_FinalisedDate", ZDateTimeOffset.Empty, orderLine4.WE_FinalisedDate);
			AssertEquals("OrderLine4 correct WE_DocketLineStatus", ZString.Empty, orderLine4.WE_DocketLineStatus);
		}

		[TestDate(2022, 11, 16)]
		public void TestFinaliseDocket_OrderLinesFinaliseDataAndStatus_OnUnSet()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 3m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Order not picked", true, order.IsAttachedToPickButNotFinalised);
			Factory.Save();

			pick.FinaliseAllOrders();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("OrderLine1 correct WE_FinalisedDate", ZDateTimeOffset.Now, orderLine1.WE_FinalisedDate);
			AssertEquals("OrderLine1 correct WE_DocketLineStatus", DocketLineStatus.Codes.Finalised, orderLine1.WE_DocketLineStatus);
			AssertEquals("OrderLine2 correct WE_FinalisedDate", ZDateTimeOffset.Now, orderLine2.WE_FinalisedDate);
			AssertEquals("OrderLine2 correct WE_DocketLineStatus", DocketLineStatus.Codes.Finalised, orderLine2.WE_DocketLineStatus);

			order.WD_FinalisedDate = ZDateTimeOffset.Empty;
			AssertEquals("Order not finalised", false, order.IsFinalised);
			AssertEquals("OrderLine1 correct WE_FinalisedDate", ZDateTimeOffset.Empty, orderLine1.WE_FinalisedDate);
			AssertEquals("OrderLine1 correct WE_DocketLineStatus", ZString.Empty, orderLine1.WE_DocketLineStatus);
			AssertEquals("OrderLine2 correct WE_FinalisedDate", ZDateTimeOffset.Empty, orderLine2.WE_FinalisedDate);
			AssertEquals("OrderLine2 correct WE_DocketLineStatus", ZString.Empty, orderLine2.WE_DocketLineStatus);
		}

		#endregion

		#region TestFinaliseDocket_CustomsOrderWithCancelEvent

		public void TestFinaliseDocket_CustomsOrderWithCancelEvent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			var bondedOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 10m);
			bondedOrder.WD_DocketSubType = OrderType.Codes.Customs;
			var bondedOrderLine = bondedOrder.Lines[0];
			bondedOrderLine.WE_BondedEntryKey = "123-1";
			bondedOrderLine.CustomsData.WB_EntryKey = "DummyOutward";
			bondedOrderLine.CustomsData.WB_EntryLineNo = 1;
			var bondedPick = Helper.CreatePickNew(bondedOrder);
			bondedOrder.Logs.AddNew(Events.WarehouseJobCanNowBeFinalised);
			Factory.Save();

			bondedOrder.Logs.AddNew(Events.Cancelled);
			Factory.Save();

			AssertEquals("Precondition", DocketStatus.Codes.AttachedToPick, bondedOrder.WD_DocketStatus);
			AssertEquals("Precondition", ZDateTimeOffset.Empty, bondedOrder.WD_FinalisedDate);
			AssertEquals("Precondition", string.Empty, bondedOrderLine.WE_DocketLineStatus);
			AssertEquals("Precondition", ZDateTimeOffset.Empty, bondedOrderLine.WE_FinalisedDate);

			bondedPick.FinaliseAllOrders();
			AssertEquals("Order is finalised.", true, bondedOrder.IsFinalised);
			AssertEquals("Order line is finalised.", DocketLineStatus.Codes.Finalised, bondedOrderLine.WE_DocketLineStatus);
			AssertNoExceptionThrown("No save exceptions.", () => Factory.Save());

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(bondedOrder.PK);
			AssertEquals("orderInNewFactory is finalised.", true, orderInNewFactory.IsFinalised);
		}

		#endregion

		#region Setup

		protected override WhsOrder SetupForTestFinaliseDocket()
		{
			var order = base.SetupForTestFinaliseDocket();
			Factory.Save();

			order.WD_ExternalReference = "ORD1";
			order.ConsigneePK = order.Client.PK;
			order.ConsigneeAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			order.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			order.WD_RequiredDate = ZDateTimeOffset.Today;

			// setup some inventory to pick
			var part = Helper.CreateProduct(order.Client, "P1");
			Helper.CreateStock(order.Warehouse, order.Client, part, 100m, "A");

			var docketLine = (WhsDocketLine)order.Lines.AddNew();
			docketLine.WE_OP = part.PK;
			docketLine.WE_TransactionQuantity = 10;

			// pick the order so it can be finalised
			Helper.CreatePickNew(order);
			AssertEquals("Order not picked", true, order.IsAttachedToPickButNotFinalised);

			return order;
		}

		#endregion

		#endregion

		#region CrossDocking

		[ExpectNoExceptions]
		public void TestAgainstCrossDockLinkCollectionModifiedException()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			BusinessObjectFactory linkFactory = new BusinessObjectFactory();
			WhsTestHelperFunctions linkHelper = new WhsTestHelperFunctions(linkFactory);

			linkHelper.CreateReservePickLine(line1, data.Line111, 10m);
			linkFactory.Save();

			decimal d = order.WD_CalcCrossDockAvailableVolumeIncludingCrossDockAllocations;
			d = order.WD_CalcCrossDockCurrentVolumeIncludingCrossDockAllocations;

			AssertEquals("Just touching", 1, order.AllocatedReceipts.Count);
			AssertEquals("Just touching", 1, line1.ReservedPickLines.Count);

			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			linkHelper.CreateReservePickLine(line2, data.Line111, 10m);
			linkFactory.Save();

			AssertEquals("Just touching", 1, line2.ReservedPickLines.Count);

			line2.Delete();
			line2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			d = order.WD_CalcCrossDockAvailableVolumeIncludingCrossDockAllocations;
			d = order.WD_CalcCrossDockCurrentVolumeIncludingCrossDockAllocations;

			Factory.Save();
		}

		#endregion

		#region Workflow

		#region TestClearEvents

		public void TestClearActualDatesOnMilestones()
		{
			ClearEvents(withMilestones: true);
		}

		public void TestClearEvents()
		{
			ClearEvents(withMilestones: false);
		}

		void ClearEvents(bool withMilestones)
		{
			var org = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("1");
			Factory.Save();

			var docket = GetNewBusinessObject();
			docket.WD_OH_Client = org.PK;
			docket.WD_WW_Whs = whs.PK;
			var pick = Helper.CreatePickNew();
			pick.WP_WW_Whs = whs.PK;
			if (withMilestones)
			{
				var task = docket.WorkflowItems.AddNew();
				task.TriggerConditions.TriggerEventCode = Events.WarehouseOrderPicking.Code;
			}
			else
			{
				var templates =
					Factory.Load<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, "WOU"));
				foreach (var template in templates)
				{
					template.WorkflowItems.DeleteAll();
				}

				docket.WorkflowItems.DeleteAll();
			}

			Factory.Save();
			if (withMilestones)
			{
				AssertEquals("Picking milestone should not have date", false,
					CompletedMilestonesExistForThisEvent(docket, Events.WarehouseOrderPicking));
			}

			AssertEquals("There should be no picking event", 0, CountLogsForThisEvent(docket, Events.WarehouseOrderPicking));

			docket.WD_WP = pick.PK;
			docket.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			Factory.Save();
			if (withMilestones)
			{
				AssertEquals("Picking milestone should have date", true,
					CompletedMilestonesExistForThisEvent(docket, Events.WarehouseOrderPicking));
			}

			AssertEquals("Event should be here.", 1, CountLogsForThisEvent(docket, Events.WarehouseOrderPicking));
			AssertEquals("Event should not be cancelled.", 1,
				CountLogsForThisEvent(docket, Events.WarehouseOrderPicking, isCancelled: false));

			docket.WD_WP = ZGuid.Empty;
			docket.WD_DocketStatus = DocketStatus.Codes.Entered;
			Factory.Save();
			if (withMilestones)
			{
				AssertEquals("Once a fired Milestone has been saved, it should not unfire.", true,
					CompletedMilestonesExistForThisEvent(docket, Events.WarehouseOrderPicking));
			}

			AssertEquals("Event should be here.", 1, CountLogsForThisEvent(docket, Events.WarehouseOrderPicking));
			AssertEquals("Event should be cancelled.", 1,
				CountLogsForThisEvent(docket, Events.WarehouseOrderPicking, isCancelled: true));
		}

		#endregion

		#region TestDisappearingMileStones

		[TestDate(2013, 5, 6)]
		public void TestDisappearingMileStones()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Factory.Save();

			AssertEquals(4, order.WorkflowItems.Count);

			// cancel order
			order.CancelReactivateDocket();
			Factory.Save();
			AssertEquals("Precondition", true, order.IsCancelled);
			AssertEquals(1, order.WorkflowItems.Count);

			// reactivate order
			order.CancelReactivateDocket();
			Factory.Save();
			AssertEquals("Precondition", false, order.IsCancelled);

			// pick the order so it can be finalised
			Helper.CreatePickNew(order);
			AssertEquals("Order not picked", true, order.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition", false, order.IsFinalised);

			// check finalise milestone
			var finaliseMilestone = order.WorkflowItems.Cast<WhsOrderProcessTasks>()
				.FirstOrDefault(m => m.P9_SE_NKMilestoneEvent == Events.ItemDocumentJobFinalisedCode);
			AssertNotNull("Finalise Milestone should exist after Order is cancelled then re-activated.",
				finaliseMilestone);

			// ensure milstone works
			order.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(order);

			AssertEquals(2013, finaliseMilestone.P9_ActualDate.Year);
			AssertEquals(5, finaliseMilestone.P9_ActualDate.Month);
			AssertEquals(6, finaliseMilestone.P9_ActualDate.Day);

			//check Workflow Items
			AssertEquals(4, order.WorkflowItems.Count);
		}

		#endregion

		#endregion

		#region TestNonStandardReadOnly1

		protected override void TestNonStandardReadOnly1Core(Func<WhsOrder, bool> getReadOnly, string name, WhsOrder docket)
		{
			Helper.AddOnePostedChargeLine(docket);
			AssertEquals(
				$"{name} should be readonly if docket is finalised and pick is finalised and at least one charge line is posted",
				true, getReadOnly(docket));
		}

		#endregion

		#region TestIsDepartedFromPickFinalisation

		public void TestIsDepartedFromPickFinalisation()
		{
			var docket = GetNewBusinessObject();
			AssertEquals(false, docket.IsDepartedFromPickFinalisation);
			docket.WD_DocketStatus = WhsOrderStatus.Codes.Departed;
			AssertEquals(true, docket.IsDepartedFromPickFinalisation);
		}

		#endregion

		#region TestIsPostFinalizeEditAllowed

		public override void TestIsPostFinalizeEditAllowed()
		{
			base.TestIsPostFinalizeEditAllowed();

			var docket = GetNewBusinessObject();
			Helper.AddOnePostedChargeLine(docket);
			AssertEquals(false, docket.IsPostFinalizeEditAllowed);
		}

		#endregion

		#region TestFinalisedOrderIsReadOnlyAfterRevenueWasPosted

		public void TestFinalisedOrderIsReadOnlyAfterRevenueWasPosted()
		{
			//create order and finalise
			WarehouseDataRegistry.Instance.AutoFinalizePick.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var order = SetupForTestFinaliseDocket();

			order.FinaliseDocket();
			AssertEquals(true, order.IsFinalised);
			AssertEquals(true, order.Pick.IsFinalised);

			AssertEquals("Precondition:", true, order.IsPostFinalizeEditAllowed);
			AssertEquals("Precondition:", false, order.WD_CustomerReferenceInfo.ReadOnly);

			bool wasRefreshBindingInvoked = false;
			((IBindingList)order).ListChanged += delegate
			{
				wasRefreshBindingInvoked = true;
			};

			// create Non-Posted charge line
			Helper.CreateAccountingDataWithWIPCharge(order);
			AssertEquals("Order has a Charge.", 1, order.AccTranLines.Count);
			AssertEquals("Refresh Binding should not have been called yet.", false, wasRefreshBindingInvoked);
			AssertEquals("Order has a Charge, but it is not posted, so the Order should still be editable.", true,
				order.IsPostFinalizeEditAllowed);
			AssertEquals(
				"Order has a Charge, but it is not posted, so the Order Customer Ref should still be editable.", false,
				order.WD_CustomerReferenceInfo.ReadOnly);

			// create Posted charge line
			Helper.AddChargeLine(order);
			AssertEquals("Order has a Posted REV Charge, but the cache has not been refreshed.", 1,
				order.AccTranLines.Count);
			AssertEquals("Refresh Binding should not have been called yet.", false, wasRefreshBindingInvoked);
			AssertEquals(
				"Order has a posted Charge, but but the cache has not been refreshed, so the Order should still be editable.",
				true, order.IsPostFinalizeEditAllowed);
			AssertEquals(
				"Order has a Posted REV Charge, but the cache has not been refreshed, so the Order Customer Ref should still be editable.",
				false, order.WD_CustomerReferenceInfo.ReadOnly);

			// mimic Accounting Posting Functionality
			order.InvoicingSupporter.PostedStateChanged();
			AssertEquals(
				"Order has two lines and the cache should have been refreshed, so the number of lines should be two.",
				2, order.AccTranLines.Count);
			AssertEquals("Refresh Binding should have been called in PostedStateChanged so the UI can refresh.", true,
				wasRefreshBindingInvoked);
			AssertEquals(
				"Order has a posted Charge, and the cache has been refreshed, so the Order should not be editable.",
				false, order.IsPostFinalizeEditAllowed);
			AssertEquals(
				"Order now has a Charge Line with REV (means it's Posted), so the Order Customer Ref should now be readonly.",
				true, order.WD_CustomerReferenceInfo.ReadOnly);
		}

		#endregion

		#region TestCustomerRefFieldOfOrderWithPostedRevenueBecomeReadOnlyWhenPickWasFinalisedAndSaved

		public void TestCustomerRefFieldOfOrderWithPostedRevenueBecomeReadOnlyWhenPickWasFinalisedAndSaved()
		{
			var order = SetupForTestFinaliseDocket();
			order.FinaliseDocket();
			AssertEquals(true, order.IsFinalised);
			AssertEquals(false, order.Pick.IsFinalised);
			Factory.Save();

			Helper.AddOnePostedChargeLine(order);
			AssertEquals(
				"Order has a Posted Charge, but pick has not been finalised yet, so the Order Customer Ref should still be editable.",
				false, order.WD_CustomerReferenceInfo.ReadOnly);

			bool wasRefreshBindingInvoked = false;
			((IBindingList)order).ListChanged += delegate
			{
				wasRefreshBindingInvoked = true;
			};

			// Create another factory, load the order and Finalise Pick in new factory, and save
			var anotherFactory = new BusinessObjectFactory();
			anotherFactory.RefreshEnabled = true;
			var orderInAnotherFactory = anotherFactory.Load<WhsOrder>(order.PK);
			AssertEquals(false, orderInAnotherFactory.Pick.IsFinalised);
			orderInAnotherFactory.Pick.FinalisePick();
			AssertEquals(true, orderInAnotherFactory.Pick.IsFinalised);
			anotherFactory.Save();

			AssertEquals("Original Order should have been updated via data refresh bus.", true, order.Pick.IsFinalised);
			AssertEquals("After pick was finalised, RefreshBinding should have been invoked.", true,
				wasRefreshBindingInvoked);
			AssertEquals(
				"Order has a Posted Charge, and pick has been finalised, so the Order Customer Ref should be read only.",
				true, order.WD_CustomerReferenceInfo.ReadOnly);
		}

		#endregion

		#region TestAutoCreateWorkOrders

		#region TestAutoCreateWorkOrders

		public void TestAutoCreateWorkOrders()
		{
			var data = new TestDataForBOM(Factory);
			var order = data.CreateOrderWithBOMShortfall();
			Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			AssertEquals("Precondition - WD_ExternalReference cannot be empty.", false,
				order.WD_ExternalReference.IsEmpty);

			order.BOM.AutoCreateWorkOrders(Notify);
			AssertEquals("One WorkOrder should be created per OrderLine.", 2, order.CurrentWorkOrders.Count());
			AssertNotEquals("Each WorkOrder requires a unique Ext Ref. Split",
				order.CurrentWorkOrders.ElementAt(0).WD_ExternalReferenceSplit,
				order.CurrentWorkOrders.ElementAt(1).WD_ExternalReferenceSplit);

			Factory.Save(); // to generate a DocketID.

			var getPropertiesToExcludeFromCloningMethod =
				typeof(WhsOrderLine).GetMethod("GetPropertiesToExcludeFromCloning",
					BindingFlags.Instance | BindingFlags.NonPublic);
			var nonCloneableFields =
				(IEnumerable<string>)getPropertiesToExcludeFromCloningMethod.Invoke(order.Lines[0], Array.Empty<object>());

			for (int i = 0; i < order.CurrentWorkOrders.Count(); i++)
			{
				var workOrder = order.CurrentWorkOrders.ElementAt(i);

				var orderLine = order.Lines[i];
				var workOrderLine = workOrder.Lines[0];

				AssertEquals(order.WD_ExternalReference, workOrder.WD_ExternalReference);
				AssertEquals("One WorkOrderLine should be created per OrderLine.", 1, workOrder.Lines.Count);

				foreach (SchemaColumn column in WhsDocketLineSchema.All)
				{
					if (column != WhsDocketLineSchema.PK &&
						column != WhsDocketLineSchema.WE_WD &&
						column != WhsDocketLineSchema.WE_DocketLineType &&
						column != WhsDocketLineSchema.WE_IsValid &&
						column != WhsDocketLineSchema.WE_TransactionQuantity &&
						column != WhsDocketLineSchema.WE_LineNo &&
						!nonCloneableFields.Contains(column.Name))
					{
						AssertEquals(
							string.Format("OrderLine.{0} was '{1}' but WorkOrderLine.{0} was '{2}'.", column.Name,
								orderLine[column], workOrderLine[column]),
							orderLine[column], workOrderLine[column]);
					}
				}

				// ensure we build enough units to meet the shortfall
				string msg = string.Format("Order Shortfall is {0} units but the WorkOrder is building {1} units.",
					orderLine.WE_ShortfallQuantityCached, workOrderLine.WE_TransactionQuantity);
				AssertEquals(msg, workOrderLine.WE_TransactionQuantity, orderLine.WE_ShortfallQuantityCached);
			}
		}

		#endregion

		#region TestAutoCreateWorkOrders_WorkOrdersAreRegisteredEditable

		public void TestAutoCreateWorkOrders_WorkOrdersAreRegisteredEditable()
		{
			var data = new TestDataForBOM(Factory);
			WhsOrder order = data.CreateOrderWithBOMShortfall();

			order.BOM.AutoCreateWorkOrders(Notify);
			AssertEquals(true, order.IsRegisteredEditableChildObject(order.CurrentWorkOrders.ElementAt(0)));
		}

		#endregion

		#region TestAutoCreateWorkOrders_ReloadsRelatedJobs

		/// <summary>
		///     This is so that after creating Work Orders, the user can click on the Order.RelatedJobs tab to view them.
		/// </summary>
		public void TestAutoCreateWorkOrders_ReloadsRelatedJobs()
		{
			var data = new TestDataForBOM(Factory);
			var order = data.CreateOrderWithBOMShortfall();
			AssertEquals(0, order.RelatedJobs.Count);

			var relatedJobs = order.RelatedJobs;
			order.BOM.AutoCreateWorkOrders(Notify);
			Factory.Save(); // RelatedJobs uses a SQL Common Table Expression which is a DB-Only read
			AssertEquals("Changing the RelatedJobs reference will break binding.", relatedJobs, order.RelatedJobs);
			AssertEquals(1, order.RelatedJobs.Count);
			AssertEquals(order.CurrentWorkOrders.ElementAt(0), order.RelatedJobs[0]);
		}

		public void TestAutoCreateWorkOrders_DoesNotReloadJobsIfRelatedJobsWasNotLoaded()
		{
			var data = new TestDataForBOM(Factory);
			var order = data.CreateOrderWithBOMShortfall();
			order.BOM.AutoCreateWorkOrders(Notify);
			var docketHits1 = Factory.TableSelects.Single(t => t.TableName == WhsDocketSchema.Constants.TableName);
			Factory.Save(); // RelatedJobs uses a SQL Common Table Expression which is a DB-Only read

			var docketHits2 = Factory.TableSelects.Single(t => t.TableName == WhsDocketSchema.Constants.TableName);
			AssertEquals(
				"Docket hits should not have changed on Factory.Save() as it should not have attempted to load Related Jobs.",
				docketHits1.Value, docketHits2.Value);
			AssertEquals(1, order.RelatedJobs.Count);

			var docketHits3 = Factory.TableSelects.Single(t => t.TableName == WhsDocketSchema.Constants.TableName);
			Assert($"Docket hits should have increased when accessing Related Jobs as it loaded the jobs.",
				docketHits3.Value > docketHits2.Value);
			AssertEquals(order.CurrentWorkOrders.ElementAt(0), order.RelatedJobs[0]);
		}

		#endregion

		#region TestAutoCreateWorkOrders_OverridesExistingWorkOrders

		public void TestAutoCreateWorkOrders_OverridesExistingWorkOrders()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.BOM.Bike, 10m); // no stock.
			Helper.CreatePickNew(order);
			AssertEquals("Precondition - Pick failed.", true, order.IsAttachedToPickButNotFinalised);

			// test that work orders are generated correctly the first time
			Notify.DefaultResponse = true;
			order.BOM.AutoCreateWorkOrders(Notify);
			AssertEquals(BOMAutoCreateHelper.WorkOrderCreationSuccessMsg, Notify.LastEvent.Message);
			// level 1
			var bikesWO = order.CurrentWorkOrders.Single();
			AssertEquals(2, bikesWO.CurrentWorkOrders.Count());
			// level 2
			var enginesWO =
				bikesWO.CurrentWorkOrders.Single(wo => wo.Lines.Single().SupplierPart == data.BOM.BikeEngine);
			var wheelsWO =
				bikesWO.CurrentWorkOrders.Single(wo => wo.Lines.Single().SupplierPart == data.BOM.BikeWheel);
			// level 3
			var pistonsWO = enginesWO.CurrentWorkOrders.Single();
			AssertEquals("WorkOrder Bike build qty should match Order Shortfall qty.",
				bikesWO.Lines.Single().WE_TransactionQuantity, 10m);
			AssertEquals("WorkOrder Engines build qty should match Order Shortfall qty.",
				enginesWO.Lines.Single().WE_TransactionQuantity, 10m);
			AssertEquals("WorkOrder Wheels build qty should match Order Shortfall qty.",
				wheelsWO.Lines.Single().WE_TransactionQuantity, 20m);

			// user decides not to overwrite existing WO's
			Notify.DefaultResponse = false;
			order.BOM.AutoCreateWorkOrders(Notify);
			AssertEquals(order.CurrentWorkOrders.Single().PK, bikesWO.PK);
			AssertEquals(false, bikesWO.IsDeleted);
			AssertEquals(false, enginesWO.IsDeleted);
			AssertEquals(false, wheelsWO.IsDeleted);
			AssertEquals(false, pistonsWO.IsDeleted);

			// user decides to overwrite but one WO is in progress
			Notify.DefaultResponse = true;
			data.CreateProductInInventory("piston crank", data.BOM.PistonCrank, 1);
			data.CreateProductInInventory("piston head", data.BOM.PistonHead, 1);
			data.CreateProductInInventory("piston rings", data.BOM.PistonRing, 4);
			Factory.Save();
			Helper.CreatePickNew(pistonsWO);
			AssertEquals("Precondition - Pick failed.", true, pistonsWO.IsAttachedToPickButNotFinalised);

			order.BOM.AutoCreateWorkOrders(Notify);
			AssertEquals("New Work Order should NOT have been generated because ", order.CurrentWorkOrders.Single().PK,
				bikesWO.PK);
			AssertEquals(false, bikesWO.IsDeleted);
			AssertEquals(false, enginesWO.IsDeleted);
			AssertEquals(false, wheelsWO.IsDeleted);
			AssertEquals(false, pistonsWO.IsDeleted);

			// user decides to overwrite existing WO's
			pistonsWO.Pick.CancelPick();
			order.BOM.AutoCreateWorkOrders(Notify);
			AssertNotEquals(order.CurrentWorkOrders.Single().PK, bikesWO.PK);
			AssertEquals(true, bikesWO.IsDeleted);
			AssertEquals(true, enginesWO.IsDeleted);
			AssertEquals(true, wheelsWO.IsDeleted);
			AssertEquals(true, pistonsWO.IsDeleted);

			// no longer any shortfalls
			order.Lines[0].WE_TransactionQuantity = 0;
			Notify.DefaultResponse = true;
			order.BOM.AutoCreateWorkOrders(Notify);
			AssertEquals("Shortfalls no longer exist, the WorkOrder should have been deleted.", 0,
				order.CurrentWorkOrders.Count());
		}

		#endregion

		#region TestAutoCreateWorkOrders_CreatesChildItems

		public void TestAutoCreateWorkOrders_CreatesChildItems()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();

			// create an order for 2 bikes
			WhsOrder order =
				Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.BOM.Bike,
					2m); // no inventory created, shortfall of 2.

			// pick the order, should have 2 bikes missing.
			WhsPick pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - Pick failed.", true, order.IsAttachedToPickButNotFinalised);

			order.BOM.AutoCreateWorkOrders(Notify);

			// bike work order
			WhsWorkOrder bikeWorkOrder = order.CurrentWorkOrders.ElementAt(0);
			AssertEquals("Should have created one top level WorkOrder for the bike.", 1,
				order.CurrentWorkOrders.Count());
			AssertEquals("Should have created one line for the bike.", 1, bikeWorkOrder.Lines.Count);
			AssertEquals("Should be building 2 bikes.", 2m, bikeWorkOrder.Lines[0].WE_TransactionQuantity);

			// level 1 work orders (engine WO & wheels WO)
			WhsWorkOrder[] engineAndWheelsworkOrders =
				Factory.Load<WhsWorkOrder>(new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, bikeWorkOrder.PK));
			AssertEquals("Should have one work Order for the Engine, and one for the Wheels.", 2,
				engineAndWheelsworkOrders.Length);

			WhsWorkOrder engineWorkOrder = FindWorkOrder(data.BOM.BikeEngine, engineAndWheelsworkOrders);
			AssertEquals("Should have created one line for the engine.", 1, engineWorkOrder.Lines.Count);
			AssertEquals("Should be building 2 engines (1 per bike).", 2m,
				engineWorkOrder.Lines[0].WE_TransactionQuantity);

			WhsWorkOrder wheelsWorkOrder = FindWorkOrder(data.BOM.BikeWheel, engineAndWheelsworkOrders);
			AssertEquals("Should have created one line for the wheels.", 1, wheelsWorkOrder.Lines.Count);
			AssertEquals("Should be building 4 wheels (2 per bike).", 4m,
				wheelsWorkOrder.Lines[0].WE_TransactionQuantity);

			// level 2 work orders (piston WO)
			WhsWorkOrder[] pistonsWorkOrders =
				Factory.Load<WhsWorkOrder>(new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, engineWorkOrder.PK));
			AssertEquals("Should have one work Order for the pistons.", 1, pistonsWorkOrders.Length);

			WhsWorkOrder pistonsWorkOrder = pistonsWorkOrders[0];
			AssertEquals("Should have created one line for the pistons.", 1, pistonsWorkOrder.Lines.Count);
			AssertEquals("Should be building 8 pistons (4 per engine).", 8m,
				pistonsWorkOrder.Lines[0].WE_TransactionQuantity);

			// test that saving does not fail, and confirm that the results are available in the RelatedJobs
			Factory.Save();
			AssertEquals(4, order.RelatedJobs.Count);
			AssertCollectionContains(bikeWorkOrder, order.RelatedJobs);
			AssertCollectionContains(engineWorkOrder, order.RelatedJobs);
			AssertCollectionContains(pistonsWorkOrder, order.RelatedJobs);
			AssertCollectionContains(wheelsWorkOrder, order.RelatedJobs);
		}

		#endregion

		#region TestAutoCreateWorkOrders_NoWorkOrderCreatedIfNoBOMShortfallExists

		public void TestAutoCreateWorkOrders_NoWorkOrderCreatedIfNoBOMShortfallExists()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryWithSaveFactoryForWarehouse();
			Factory.Save(); // otherwise attempting to reload inventory not in db will blow up

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m); // not a BOM line

			var pick1 = Factory.New<WhsPick>();
			pick1.PickOrdersWithAllocationMock(new WhsOrder[] { order });
			order.FinaliseDocket();

			// no BOM lines exists
			order.BOM.AutoCreateWorkOrders(Notify);
			AssertEquals("No BOM lines on the Order.", 0, order.CurrentWorkOrders.Count());
			pick1.CancelPick();

			// BOM lines exist, but no shortfall exists
			data.Part1.BillOfMaterials.AddNew().FillWithValidTestData();
			WhsPick pick2 = Factory.New<WhsPick>();
			pick2.PickOrdersWithAllocationMock(new WhsOrder[] { order });
			order.FinaliseDocket();

			order.BOM.AutoCreateWorkOrders(Notify);
			AssertEquals("BOM lines exist on the Order however there is no Shortfall.", 0,
				order.CurrentWorkOrders.Count());
		}

		#endregion

		#region TestAutoCreateWorkOrders_NoWorkOrderCreatedIfOrderFinalizedOrCanceled

		public void TestAutoCreateWorkOrders_NoWorkOrderCreatedIfOrderFinalizedOrCanceled()
		{
			var data = new TestDataForBOM(Factory);
			WhsOrder orderWithShortfall = data.CreateOrderWithBOMShortfall();

			orderWithShortfall.WD_FinalisedDate = ZDateTimeOffset.Today;
			orderWithShortfall.BOM.AutoCreateWorkOrders(Notify);
			AssertEquals("No Work Orders were created because the Order is already Finalized.",
				Notify.LastEvent.Message);
			AssertEquals(0, Order.CurrentWorkOrders.Count());

			orderWithShortfall.WD_FinalisedDate = ZDateTimeOffset.Empty;
			orderWithShortfall.WD_DocketStatus = DocketStatus.Codes.Entered;
			orderWithShortfall.WD_DocketStatus = DocketStatus.Codes.Cancelled;
			orderWithShortfall.BOM.AutoCreateWorkOrders(Notify);
			AssertEquals("No Work Orders were created because the Order is Canceled.", Notify.LastEvent.Message);
			AssertEquals(0, Order.CurrentWorkOrders.Count());
		}

		#endregion

		#region TestAutoCreateWorkOrders_SetsDefaultsFromOrder

		public void TestAutoCreateWorkOrders_SetsDefaultsFromOrder()
		{
			var data = new TestDataForBOM(Factory);

			// setup an order with a BOM shortfall (such that work orders are required to fulfill the shortfall)
			WhsOrder order = data.CreateOrderWithBOMShortfall();
			order.WD_RequiredDate = ZDateTimeOffset.Now;

			// auto-create the work orders
			order.BOM.AutoCreateWorkOrders(Notify);
			var workOrder = order.CurrentWorkOrders.ElementAt(0);

			// test fields that were copied across
			AssertNull("Product has no BOM Staging Location, Work Order should also have no Location.",
				workOrder.Lines[0].Location);
			AssertEquals(order.WD_RequiredDate, workOrder.WD_RequiredDate);
		}

		#endregion

		#region TestAutoCreateWorkOrders_GetsStagingLocationFromProductParams

		public void TestAutoCreateWorkOrders_GetsStagingLocationFromProductParams()
		{
			var data = new TestDataForBOM(Factory);
			var order = data.CreateOrderWithBOMShortfall();

			order.BOM.AutoCreateWorkOrders(Notify);
			AssertNull("Product has no BOM Staging Location, WorkOrderLine should have no BOM Staging Location.",
				order.CurrentWorkOrders.ElementAt(0).Lines[0].StagingLocationBOM);

			var productParams = data.Product1.ParamsByWhsAndClient.AddNew();
			var warehouse = Helper.CreateWarehouse("W1", "A");
			var stagingArea = warehouse.Areas[0];
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "R");
			var stagingLocationBOM = row.Locations[0];
			productParams.W3_OP = data.Part1.PK;
			productParams.W3_OH = order.Client.PK;
			productParams.W3_WW = order.Warehouse.PK;
			productParams.W3_WL_StagingLocationBOM = stagingLocationBOM.PK;

			AssertNotNull("Precondition -- ensure we have a valid BOM Staging Location.",
				productParams.StagingLocationBOM);

			order.CurrentWorkOrders.ElementAt(0).Delete();
			order.BOM.AutoCreateWorkOrders(Notify);
			AssertEquals(productParams.StagingLocationBOM,
				order.CurrentWorkOrders.ElementAt(0).Lines[0].StagingLocationBOM);
		}

		#endregion

		#region TestAutoCreateWorkOrders_NotifiesOfSuccessOrFailure

		public void TestAutoCreateWorkOrders_NotifiesOfSuccessOrFailure()
		{
			var data = new TestDataForBOM(Factory);
			var orderWithShortfall = data.CreateOrderWithBOMShortfall();
			orderWithShortfall.BOM.AutoCreateWorkOrders(Notify);
			AssertEquals(BOMAutoCreateHelper.WorkOrderCreationSuccessMsg, Notify.LastEvent.Message);

			var orderWithNoShortfall = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			orderWithNoShortfall.BOM.AutoCreateWorkOrders(Notify);
			AssertEquals("Order has no BOM Shortfalls and does not require a Work Order.", Notify.LastEvent.Message);
			AssertEquals(0, orderWithNoShortfall.CurrentWorkOrders.Count());
		}

		#endregion

		#endregion

		#region TestCartageType

		public void TestCartageType()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketID = "test DocketID";
			order.WD_ExternalReference = "test ext ref";
			order.Containers.AddNew();
			order.Lines.AddNew();
			var cartageType = new WhsOrder.WhsCartageType(order);

			AssertEquals(1, cartageType.CartageContainers.Count);
			AssertEquals(order.Containers[0], cartageType.CartageContainers.First());
			AssertEquals(Core.Constants.CartageJobType.NEW_WarehouseContainerizedDelivery, cartageType.CartageJobType);
			AssertEquals(order, cartageType.CartageLooseCargo.First());
			AssertEquals(order.TransportCoDocAddress.Address, cartageType.LocalTransportProviderAddress);
			AssertEquals(order.TransportBillToDocAddress.E2_OA_AddressInfo, cartageType.CartageAddressInfo);
			AssertEquals(((ICartageParent)order).GoodsDescription, cartageType.Description);
			AssertEquals(order.WD_DropMode, cartageType.DropMode);
			AssertEquals(ZDateTime.Empty, cartageType.E_ARV);
			AssertEquals(ZDateTime.Empty, cartageType.E_DEP);
			AssertEquals(order.WD_RequiredDate.ToZDateTime(), cartageType.EstimatedCartageDelivery);
			AssertEquals(order.WD_FinalisedDate.ToZDateTime(), cartageType.EstimatedCartagePickup);
			AssertEquals(ZDateTime.Empty, cartageType.FCLAvailabilityDate);
			AssertEquals(ZDateTime.Empty, cartageType.FCLStorageDate);
			AssertEquals(ZDateTime.Empty, cartageType.LCLAvailabilityDate);
			AssertEquals(ZDateTime.Empty, cartageType.LCLStorageDate);

			var whsAddy = cartageType.GetCartageAddress(LocalCartageJobOrgTypeList.Codes.WHS);
			AssertEquals(
				JobDocAddress.GetOrCreateNonPersistantDocAddress(order.Warehouse, DocAddressType.PickUpAddress,
					order.Warehouse.WarehouseAddress.PK), whsAddy);
			AssertEquals(order.ConsigneeDocAddress,
				cartageType.GetCartageAddress(LocalCartageJobOrgTypeList.Codes.CNE));
			AssertNull(cartageType.GetCartageAddress("junk"));

			AssertEquals("", cartageType.PortOfDischarge);
			AssertEquals("", cartageType.PortOfLoading);
			AssertEquals("", cartageType.Vessel);
			AssertEquals("", cartageType.VoyageFlight);
			AssertEquals(order.WD_ExternalReference + ": " + cartageType.Description, cartageType.DescriptionForMenu);

			var cartageJob = (BusinessObject)Factory.New<ICommonCartage>();
			cartageJob[JobCartageSchema.JJ_ParentID] = order.PK;
			cartageType.CartageAdvised(Factory); // fire event
			AssertEquals(cartageJob[JobCartageSchema.JJ_ConsignmentID], order.WD_TransportReference);

			AssertArrayEqualsByElements("GetMatchingDirectionCodes() returned the wrong values",
				new ZString[] { "EXP", "IMP", "ORG", "DST", "LOC", "LIN" },
				cartageType.GetMatchingDirectionCodes().ToArray());
		}

		#endregion

		#region TestCartageAdvised_NoResultsFound

		public void TestCartageAdvised_NoResultsFound()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var cartageType = new WhsOrder.WhsCartageType(Order);
			AssertNoExceptionThrown(() => cartageType.CartageAdvised(Factory));
		}

		#endregion

		#region TestCartageTypeWhenOrderIsLooseCargo

		public void TestCartageTypeWhenOrderIsLooseCargo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketID = "test DocketID";
			order.WD_ExternalReference = "test ext ref";
			order.Lines.AddNew();
			var cartageType = new WhsOrder.WhsCartageType(order);

			AssertEquals(0, cartageType.CartageContainers.Count);
			AssertEquals(Core.Constants.CartageJobType.NEW_WarehouseLooseDelivery, cartageType.CartageJobType);
			//AssertEquals(Order.Lines[0], cartageType.CartageLooseCargo[0]);
		}

		#endregion

		#region TestCopyTemplate

		protected override WhsOrder GetNewDocketForTemplateCopy()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsPickableDocketLine(order, data.Part1, 10m);

			var consignee = order.Consignee;
			consignee.MiscServ.OM_IMDefaultINCOTerm = "FOB";
			consignee.MainAddress.OA_RN_NKCountryCode = "AU";
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			data.Part1.OP_IsComponentPickedOnSalesOrder = true;
			var component = Helper.CreateProduct(data.Org1, "COM1");
			Helper.CreateProductBOM(data.Part1, component, 4, "UNT");

			var childOrderLine1 = order.Lines.AddNew();
			childOrderLine1.WE_WE_ParentDocketLine = orderLine1.PK;
			childOrderLine1.WE_OP = component.PK;
			childOrderLine1.WE_TransactionQuantity = 40m;
			childOrderLine1.WE_F3_NKPackType = component.OP_StockKeepingUnit;

			var orderLine2 = Helper.CreateWhsPickableDocketLine(order, data.Part2, 20m);

			var invoiceType = ObjectFactory.GetType<IWhsInvoice>();
			var invoice = Factory.NewWithValidTestData(invoiceType);
			return order;
		}

		protected override void AssertTemplateCopy(WhsOrder source, WhsOrder copy, bool shouldCopyLines)
		{
			base.AssertTemplateCopy(source, copy, shouldCopyLines);

			AssertEquals(60m, copy.WD_TotalWeight);
			AssertEquals(0.6m, copy.WD_TotalCubic);
			AssertHasError(copy.WD_ExternalReferenceInfo,
				"This Order Reference is already used by another Order for this client. " +
				"To save this Order you must enter a reference that is not already used");

			AssertEquals("Total weight on source should not be changed by copy", 60m, source.WD_TotalWeight);
			AssertEquals("Total cubic on source should not be changed by copy", 0.6m, source.WD_TotalCubic);

			AssertEquals(shouldCopyLines ? 2 : 0, copy.Lines.Count);
			Assert("Child lines should not be copied", copy.Lines.All(line => line.WE_WE_ParentDocketLine.IsEmpty));
		}

		#endregion

		#region TestDelayUpdatingReleaseTotals

		public void TestDelayUpdatingReleaseTotals()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewBusinessObject();
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_OH_Client = data.Org1.PK;
			Helper.SetProductWeightAndVolume(data.Part1, 500m, "G", 100m, "D3");
			AssertEquals("Precondition: WD_UnitsSent is empty", 0m, docket.WD_UnitsSent);
			AssertEquals("Precondition: WD_CubicSent is empty", 0m, docket.WD_CubicSent);
			AssertEquals("Precondition: WD_WeightSent is empty", 0m, docket.WD_WeightSent);
			AssertEquals("Precondition: WD_WeightSentUserEntered is empty", 0m, docket.WD_WeightSentUserEntered);

			using (docket.DelayUpdatingReleaseTotals())
			{
				using (docket.DelayUpdatingReleaseTotals())
				{
					// null product would mean that no quantity is added to the sum of changes.
					docket.UpdateReleaseTotals(null, 100m);
					AssertEquals("WD_UnitsSent", 0m, docket.WD_UnitsSent);
					AssertEquals("WD_CubicSent", 0m, docket.WD_CubicSent);
					AssertEquals("WD_WeightSent", 0m, docket.WD_WeightSent);
					AssertEquals("WD_WeightSentUserEntered", 0m, docket.WD_WeightSentUserEntered);

					// should not update cache while Updating Totals is still delayed.
					docket.UpdateReleaseTotals(data.Part1, 100m);
					AssertEquals("WD_UnitsSent", 0m, docket.WD_UnitsSent);
					AssertEquals("WD_CubicSent", 0m, docket.WD_CubicSent);
					AssertEquals("WD_WeightSent", 0m, docket.WD_WeightSent);
					AssertEquals("WD_WeightSentUserEntered", 0m, docket.WD_WeightSentUserEntered);

					// should not update cache while Updating Totals is still delayed.
					docket.UpdateReleaseTotals(data.Part1, -50m);
					AssertEquals("WD_UnitsSent", 0m, docket.WD_UnitsSent);
					AssertEquals("WD_CubicSent", 0m, docket.WD_CubicSent);
					AssertEquals("WD_WeightSent", 0m, docket.WD_WeightSent);
					AssertEquals("WD_WeightSentUserEntered", 0m, docket.WD_WeightSentUserEntered);
				}

				// should not update cache while Updating Totals is still delayed.
				AssertEquals("WD_UnitsSent", 0m, docket.WD_UnitsSent);
				AssertEquals("WD_CubicSent", 0m, docket.WD_CubicSent);
				AssertEquals("WD_WeightSent", 0m, docket.WD_WeightSent);
				AssertEquals("WD_WeightSentUserEntered", 0m, docket.WD_WeightSentUserEntered);
			}

			// When Caculation delay is removed, the sum of the changes should be applied.
			AssertEquals("WD_UnitsSent", 50m, docket.WD_UnitsSent);
			AssertEquals("WD_CubicSent", 5m, docket.WD_CubicSent);
			AssertEquals("WD_WeightSent", 25m, docket.WD_WeightSent);
			AssertEquals("WD_WeightSentUserEntered", 25m, docket.WD_WeightSentUserEntered);
		}

		public void TestDelayUpdatingReleaseTotals_WithPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0.5m, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();
			package.KP_Weight = 3m;
			package.KP_Volume = 7m;
			AssertEquals(nameof(order.WD_CubicSent), 7m, order.WD_CubicSent);
			AssertEquals(nameof(order.WD_WeightSent), 23m, order.WD_WeightSent);
			AssertEquals(nameof(order.WD_WeightSentUserEntered), 20m, order.WD_WeightSentUserEntered);
			AssertEquals(nameof(order.GrossWeightSent), 23m, order.GrossWeightSent);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var orderInOtherFactory = otherFactory.Load<WhsOrder>(order.PK);

			using (orderInOtherFactory.DelayUpdatingReleaseTotals())
			{
				order.UpdateReleaseTotals(data.Part1, 8m);
			}

			AssertEquals("Net Volume is still greather than Gross Volume, use Net Volume.", 9m, order.WD_CubicSent);
			AssertEquals("Weight Sent is untouched when there are Packages and Use Packing Weight is true.", 23m,
				order.WD_WeightSent);
			AssertEquals(nameof(order.WD_WeightSentUserEntered), 36m, order.WD_WeightSentUserEntered);
			AssertEquals("Gross Weight is untouched when there are Packages.", 23m, order.GrossWeightSent);
		}

		#endregion

		#region TestUpdateReleaseTotals

		public void TestUpdateReleaseTotals()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewBusinessObject();
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_OH_Client = data.Org1.PK;
			Helper.SetProductWeightAndVolume(data.Part1, 500m, "G", 100m, "D3");
			AssertEquals("Precondition: WD_UnitsSent is empty", 0m, docket.WD_UnitsSent);
			AssertEquals("Precondition: WD_CubicSent is empty", 0m, docket.WD_CubicSent);
			AssertEquals("Precondition: WD_WeightSent is empty", 0m, docket.WD_WeightSent);
			AssertEquals("Precondition: WD_WeightSentUserEntered is empty", 0m, docket.WD_WeightSentUserEntered);

			docket.UpdateReleaseTotals(null, 100m);
			AssertEquals(nameof(docket.WD_UnitsSent), 0m, docket.WD_UnitsSent);
			AssertEquals(nameof(docket.WD_CubicSent), 0m, docket.WD_CubicSent);
			AssertEquals(nameof(docket.WD_WeightSent), 0m, docket.WD_WeightSent);
			AssertEquals(nameof(docket.WD_WeightSentUserEntered), 0m, docket.WD_WeightSentUserEntered);
			AssertEquals(nameof(docket.GrossWeightSent), 0m, docket.GrossWeightSent);

			docket.UpdateReleaseTotals(data.Part1, 100m);
			AssertEquals(nameof(docket.WD_UnitsSent), 100m, docket.WD_UnitsSent);
			AssertEquals(nameof(docket.WD_CubicSent), 10m, docket.WD_CubicSent);
			AssertEquals(nameof(docket.WD_WeightSent), 50m, docket.WD_WeightSent);
			AssertEquals(nameof(docket.WD_WeightSentUserEntered), 50m, docket.WD_WeightSentUserEntered);
			AssertEquals(nameof(docket.GrossWeightSent), 50m, docket.GrossWeightSent);

			docket.UpdateReleaseTotals(data.Part1, -50m);
			AssertEquals(nameof(docket.WD_UnitsSent), 50m, docket.WD_UnitsSent);
			AssertEquals(nameof(docket.WD_CubicSent), 5m, docket.WD_CubicSent);
			AssertEquals(nameof(docket.WD_WeightSent), 25m, docket.WD_WeightSent);
			AssertEquals(nameof(docket.WD_WeightSentUserEntered), 25m, docket.WD_WeightSentUserEntered);
			AssertEquals(nameof(docket.GrossWeightSent), 25m, docket.GrossWeightSent);
		}

		public void TestUpdateReleaseTotals_WithPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0.5m, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();
			package.KP_Weight = 3m;
			package.KP_Volume = 7m;
			AssertEquals(nameof(order.WD_CubicSent), 7m, order.WD_CubicSent);
			AssertEquals(nameof(order.WD_WeightSent), 23m, order.WD_WeightSent);
			AssertEquals(nameof(order.WD_WeightSentUserEntered), 20m, order.WD_WeightSentUserEntered);
			AssertEquals(nameof(order.GrossWeightSent), 23m, order.GrossWeightSent);

			order.UpdateReleaseTotals(data.Part1, 2m);
			AssertEquals("Net Volume is still less than Gross Volume, use Gross Volume.", 7m, order.WD_CubicSent);
			AssertEquals("Weight Sent is untouched when there are Packages and Use Packing Weight is true.", 23m,
				order.WD_WeightSent);
			AssertEquals(nameof(order.WD_WeightSentUserEntered), 24m, order.WD_WeightSentUserEntered);
			AssertEquals("Gross Weight is untouched when there are Packages.", 23m, order.GrossWeightSent);

			order.UpdateReleaseTotals(data.Part1, 6m);
			AssertEquals("Net Volume is still greather than Gross Volume, use Net Volume.", 9m, order.WD_CubicSent);
			AssertEquals("Weight Sent is untouched when there are Packages and Use Packing Weight is true.", 23m,
				order.WD_WeightSent);
			AssertEquals(nameof(order.WD_WeightSentUserEntered), 36m, order.WD_WeightSentUserEntered);
			AssertEquals("Gross Weight is untouched when there are Packages.", 23m, order.GrossWeightSent);
		}

		#endregion

		#region TestUpdateReleaseTotals_WithAddPalletWeightToOrder

		public void TestUpdateReleaseTotals_WithAddPalletWeightToOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewBusinessObject();
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_OH_Client = data.Org1.PK;
			Helper.SetProductWeightAndVolume(data.Part1, 500m, "G", 100m, "D3");

			var packType = Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT"));
			packType.F3_Weight = 10m;
			docket.WD_AddPalletWeightToOrder = true;
			docket.WD_PalletsSent = 2;
			AssertEquals("Precondition: WD_UnitsSent is empty", 0m, docket.WD_UnitsSent);
			AssertEquals("Precondition: WD_CubicSent is empty", 0m, docket.WD_CubicSent);
			AssertEquals("Precondition: WD_WeightSent was set based on pallet weight if necessary.", 20m,
				docket.WD_WeightSent);
			AssertEquals("Precondition: WD_WeightSentUserEntered is empty", 0m, docket.WD_WeightSentUserEntered);

			docket.UpdateReleaseTotals(null, 100m);
			AssertEquals("WD_UnitsSent", 0m, docket.WD_UnitsSent);
			AssertEquals("WD_CubicSent", 0m, docket.WD_CubicSent);
			AssertEquals("WD_WeightSent", 20m, docket.WD_WeightSent);
			AssertEquals("WD_WeightSentUserEntered", 0m, docket.WD_WeightSentUserEntered);

			docket.UpdateReleaseTotals(data.Part1, 100m);
			AssertEquals("WD_UnitsSent", 100m, docket.WD_UnitsSent);
			AssertEquals("WD_CubicSent", 10m, docket.WD_CubicSent);
			AssertEquals("WD_WeightSent", 70m, docket.WD_WeightSent);
			AssertEquals("WD_WeightSentUserEntered", 50m, docket.WD_WeightSentUserEntered);

			docket.UpdateReleaseTotals(data.Part1, -50m);
			AssertEquals("WD_UnitsSent", 50m, docket.WD_UnitsSent);
			AssertEquals("WD_CubicSent", 5m, docket.WD_CubicSent);
			AssertEquals("WD_WeightSent", 45m, docket.WD_WeightSent);
			AssertEquals("WD_WeightSentUserEntered", 25m, docket.WD_WeightSentUserEntered);
		}

		#endregion

		#region TestSuspendUpdatingReleaseTotals

		public void TestSuspendUpdatingReleaseTotals()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewBusinessObject();
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_OH_Client = data.Org1.PK;
			Helper.SetProductWeightAndVolume(data.Part1, 500m, "G", 100m, "D3");
			AssertEquals("Precondition: WD_UnitsSent is empty", 0m, docket.WD_UnitsSent);
			AssertEquals("Precondition: WD_CubicSent is empty", 0m, docket.WD_CubicSent);
			AssertEquals("Precondition: WD_WeightSent is empty", 0m, docket.WD_WeightSent);
			AssertEquals("Precondition: WD_WeightSentUserEntered is empty", 0m, docket.WD_WeightSentUserEntered);

			docket.UpdateReleaseTotals(data.Part1, 100m);
			AssertEquals("WD_UnitsSent", 100m, docket.WD_UnitsSent);
			AssertEquals("WD_CubicSent", 10m, docket.WD_CubicSent);
			AssertEquals("WD_WeightSent", 50m, docket.WD_WeightSent);
			AssertEquals("WD_WeightSentUserEntered", 50m, docket.WD_WeightSentUserEntered);

			using (docket.SuspendUpdatingReleaseTotals_DoNotUse())
			{
				using (docket.SuspendUpdatingReleaseTotals_DoNotUse())
				{
					docket.UpdateReleaseTotals(data.Part1, 50m);

					AssertEquals("WD_UnitsSent", 100m, docket.WD_UnitsSent);
					AssertEquals("WD_CubicSent", 10m, docket.WD_CubicSent);
					AssertEquals("WD_WeightSent", 50m, docket.WD_WeightSent);
					AssertEquals("WD_WeightSentUserEntered", 50m, docket.WD_WeightSentUserEntered);
				}

				docket.UpdateReleaseTotals(data.Part1, 50m);
			}

			AssertEquals("WD_UnitsSent", 100m, docket.WD_UnitsSent);
			AssertEquals("WD_CubicSent", 10m, docket.WD_CubicSent);
			AssertEquals("WD_WeightSent", 50m, docket.WD_WeightSent);
			AssertEquals("WD_WeightSentUserEntered", 50m, docket.WD_WeightSentUserEntered);
		}

		#endregion

		#region TestSuspendUpdatingReleaseTotals_InCombinationWithDelayUpdatingReleaseTotals

		public void TestSuspendUpdatingReleaseTotals_InCombinationWithDelayUpdatingReleaseTotals()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = GetNewBusinessObject();
			docket.WD_WW_Whs = data.Whs1.PK;
			docket.WD_OH_Client = data.Org1.PK;
			Helper.SetProductWeightAndVolume(data.Part1, 500m, "G", 100m, "D3");
			AssertEquals("Precondition: WD_UnitsSent is empty", 0m, docket.WD_UnitsSent);
			AssertEquals("Precondition: WD_CubicSent is empty", 0m, docket.WD_CubicSent);
			AssertEquals("Precondition: WD_WeightSent is empty", 0m, docket.WD_WeightSent);
			AssertEquals("Precondition: WD_WeightSentUserEntered is empty", 0m, docket.WD_WeightSentUserEntered);

			docket.UpdateReleaseTotals(data.Part1, 100m);
			AssertEquals("WD_UnitsSent", 100m, docket.WD_UnitsSent);
			AssertEquals("WD_CubicSent", 10m, docket.WD_CubicSent);
			AssertEquals("WD_WeightSent", 50m, docket.WD_WeightSent);
			AssertEquals("WD_WeightSentUserEntered", 50m, docket.WD_WeightSentUserEntered);

			using (docket.DelayUpdatingReleaseTotals())
			{
				using (docket.SuspendUpdatingReleaseTotals_DoNotUse())
				{
					docket.UpdateReleaseTotals(data.Part1, 50m);
				}
			}

			AssertEquals("WD_UnitsSent", 100m, docket.WD_UnitsSent);
			AssertEquals("WD_CubicSent", 10m, docket.WD_CubicSent);
			AssertEquals("WD_WeightSent", 50m, docket.WD_WeightSent);
			AssertEquals("WD_WeightSentUserEntered", 50m, docket.WD_WeightSentUserEntered);
		}

		#endregion

		#region TestSyncPickWithOrder

		#region TestSyncPickWithOrder

		public void TestSyncPickWithOrder()
		{
			// setup a picked order.

			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			WhsReceive receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 50m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			WhsOrderLine orderLine_Part1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			WhsOrderLine orderLine_Part2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			Factory.Save();

			WhsPick pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - Pick failed.", true, order.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition - ensure 10 units were picked.", 10m, orderLine_Part1.PickLineQuantity);
			AssertEquals("Precondition - ensure 10 attribs were met.", 10m, orderLine_Part1.SumOfUnitsMet);
			AssertEquals("Precondition - ensure 10 units were picked.", 10m, orderLine_Part2.PickLineQuantity);
			AssertEquals("Precondition - ensure 10 attibs were met.", 10m, orderLine_Part2.SumOfUnitsMet);

			// simulate user changes to the order after picking

			WhsOrderLine
				orderLine_Part1_AddedAfterPick =
					Helper.CreateWhsOrderLine(order, data.Part1, 10m); // adding an additional order line for 10 pieces
			AssertEquals("Precondition - no units are picked.", 0m, orderLine_Part1_AddedAfterPick.PickLineQuantity);
			AssertEquals("Precondition - no units are met.", 0m, orderLine_Part1_AddedAfterPick.SumOfUnitsMet);

			orderLine_Part1.WE_TransactionQuantity = 15m; // increase order from 10 to 15
			order.SyncPickWithOrder();
			AssertEquals("PickLineQuantity was changed -- we should never automatically allocate more stock. ", 10m,
				orderLine_Part1.PickLineQuantity);
			AssertEquals("SumOfUnitsMet was changed -- we should never automatically allocate more stock", 10m,
				orderLine_Part1.SumOfUnitsMet);
			AssertEquals("Expected no change.", 10m, orderLine_Part2.PickLineQuantity);
			AssertEquals("Expected no change.", 10m, orderLine_Part2.SumOfUnitsMet);
			AssertEquals("Expected no change.", 0m, orderLine_Part1_AddedAfterPick.PickLineQuantity);
			AssertEquals("Expected no change.", 0m, orderLine_Part1_AddedAfterPick.SumOfUnitsMet);

			orderLine_Part1.WE_TransactionQuantity = 8m; // decrease order from 15 to 8, 10 were previously allocated...
			orderLine_Part2.WE_TransactionQuantity = 0m; // decrease order from 10 to 0, 10 were previously allocated...
			order.SyncPickWithOrder();
			AssertEquals("PickLineQuantity should have been adjusted down to match the decreased Order Qty.", 8m,
				orderLine_Part1.PickLineQuantity);
			AssertEquals("SumOfUnitsMet should have been adjusted down to match the decreased Order Qty", 8m,
				orderLine_Part1.SumOfUnitsMet);
			AssertEquals("PickLineQuantity should have been adjusted down to match the decreased Order Qty", 2m,
				orderLine_Part1_AddedAfterPick.PickLineQuantity);
			AssertEquals("SumOfUnitsMet should have been adjusted down to match the decreased Order Qty", 2m,
				orderLine_Part1_AddedAfterPick.SumOfUnitsMet);

			AssertEquals("PickLineQuantity should have been adjusted down to match the decreased Order Qty", 0m,
				orderLine_Part2.PickLineQuantity);
			AssertEquals("Should have 0 elements.", 0, orderLine_Part2.ReleaseLines.Count);
			AssertEquals("Should have 0 elements.", 0, orderLine_Part2.PickLines.Count);
			AssertEquals("Lines with 0 units should be deleted.", true, orderLine_Part2.IsDeleted);
		}

		#endregion

		#region TestSyncPickWithOrder_WhenUnitsOrderedLessThanAttributesMet

		/// <summary>
		///     This test is to ensure the correct Sync behaviour when we have the following OrderLine state:
		///     Ordered   Picked   AttributesMet
		///     Part1     2        0           [2]
		///     Part1     2       [2]           0
		///     ..and the user then changes the Order *after* picking like so:
		///     Ordered
		///     Part1    [0]
		///     Part1     2
		/// </summary>
		public void TestSyncPickWithOrder_WhenUnitsOrderedLessThanAttributesMet()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			// receive some stock into the whs
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);

			// order the stock
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - Pick failed.", true, order.IsAttachedToPickButNotFinalised);

			// simulate the pick engine assigning the picklines to line1, and the attributesMet to line2 (which may happen with continuous allocation/deallocation of stock)
			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			Factory.Save();
			AssertEquals("If line2 is not in the DB, the test will still pass but for the wrong reason!", true,
				line2.IsInDatabase);

			line1.PickLines[0].WZ_WE_TransactionLine = line2.PK;
			pick.ClearInventoryCache();
			pick.ClearOrderedInventoriesCache();
			pick.ClearOrdersCache();

			// simulate user lowering qty for line 1
			line1.WE_TransactionQuantity = 0;

			order.SyncPickWithOrder();
			AssertEquals(true, line1.IsDeleted);
			AssertEquals(2m, line2.PickLineQuantity);
			AssertEquals(2m, line2.SumOfUnitsMet);
		}

		#endregion

		#region TestSyncPickWithOrder_DeletesEmptyLinesEvenIfNoSyncRequired

		public void TestSyncPickWithOrder_DeletesEmptyLinesEvenIfNoSyncRequired()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			data.Part2 = Factory.New<OrgSupplierPart>();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var lineWithStock = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var lineWithNoStock = Helper.CreateWhsOrderLine(order, data.Part2, 1m); // no stock

			WhsPick pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - Pick failed.", true, order.IsAttachedToPickButNotFinalised);
			AssertEquals(2, order.Lines.Count);

			lineWithNoStock.WE_TransactionQuantity =
				0m; // nothing was allocated thus nothing to sync, but we need to be sure the cleanup occured anyway
			order.SyncPickWithOrder();
			AssertEquals(1, order.Lines.Count);
			AssertEquals(true, lineWithNoStock.IsDeleted);
		}

		#endregion

		#region TestSyncPickWithOrder_DeletedOrderLineGetDeletedInPickOrderedInventory

		public void TestSyncPickWithOrder_DeletedOrderLineGetDeletedInPickOrderedInventory()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var line2 = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var line3 = Helper.CreateWhsOrderLine(order, data.Part1, 3m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - Pick failed.", true, order.IsAttachedToPickButNotFinalised);
			AssertEquals(1, pick.OrderedInventories.Count);
			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals(3, order.Lines.Count);
			AssertEquals(3, orderedInventory.Owners.Count);
			AssertEquals(6m, orderedInventory.QuantityOrdered);

			line1.WE_TransactionQuantity = 0m;
			order.SyncPickWithOrder();

			AssertEquals(1, pick.OrderedInventories.Count);
			AssertEquals(2, order.Lines.Count);
			AssertEquals(2, orderedInventory.Owners.Count);
			AssertEquals(5m, orderedInventory.QuantityOrdered);
		}

		#endregion

		#region TestSyncPickWithOrder_IsDisabledForMultiOrderPick

		[ExpectExceptionMessage(typeof(NotSupportedException),
			"Order.SyncPickWithOrder() is not supported on a Multi-Order Pick.")]
		public void TestSyncPickWithOrder_IsDisabledForMultiOrderPick()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2");
			var line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var line2 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);

			WhsPick pick = Helper.CreatePickNew(order1, order2);
			AssertEquals("Precondition - Pick failed.", true, order1.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition - Pick failed.", true, order2.IsAttachedToPickButNotFinalised);

			order1.SyncPickWithOrder();
		}

		#endregion

		#region TestSyncPickWithOrder_IsDisabledForFinalisedOrder

		public void TestSyncPickWithOrder_IsDisabledForFinalisedOrder()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			var line = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - Pick failed.", true, order.IsAttachedToPickButNotFinalised);

			pick.FinaliseAllOrders();
			AssertEquals(true, order.IsFinalised);

			line.WE_TransactionQuantity = 5;
			order.SyncPickWithOrder();
			AssertEquals("Order was finalised, should not have reduced stock.", 10m, line.PickLineQuantity);
		}

		#endregion

		#region TestSyncPickWithOrder_IsNotCalledWhenValidatingMultiOrderPick()

		[ExpectNoExceptions]
		public void TestSyncPickWithOrder_IsNotCalledWhenValidatingMultiOrderPick()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1");
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order2");
			var line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 1m);
			var line2 = Helper.CreateWhsOrderLine(order2, data.Part1, 1m);

			WhsPick pick = Helper.CreatePickNew(order1, order2);
			AssertEquals("Precondition - Pick failed.", true, order1.IsAttachedToPickButNotFinalised);
			AssertEquals("Precondition - Pick failed.", true, order2.IsAttachedToPickButNotFinalised);

			order1.IsSavedFromOrderForm = true;
			AssertEquals("Precondition - the Order must have no errors.", false, order1.HasErrors);
			AssertEquals("Precondition - the Order cannot be finalised or cancelled.", false,
				order1.IsFinalisedOrCancelled);
			AssertEquals("Precondition - the Order must be 'saved from the OrderForm'.", true,
				order1.IsSavedFromOrderForm);

			order1.RunPreSaveValidation(); // should not throw a NotSupported exception
		}

		#endregion

		#region TestSyncPickWithOrder_DeallocatesOvers_WhenUsingMultipleFactories

		public void TestSyncPickWithOrder_DeallocatesOvers_WhenUsingMultipleFactories()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition", 0m, orderLine.PickLineQuantity); // poke property to cache (simulate binding)
			AssertEquals("Precondition", 0m, orderLine.SumOfUnitsMet); // poke property to cache (simulate binding)

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var pickInOtherFactory = otherFactory.Load<WhsPick>(pick.PK);
			pickInOtherFactory.AutoAllocateItemsWithMock();
			AssertEquals("Precondition", 1, pickInOtherFactory.GetAllPickLines().Count());
			otherFactory.Save();
			AssertEquals("Precondition", 10m, pickInOtherFactory.OrderedInventories[0].PickLineQuantity);

			orderLine.WE_TransactionQuantity = 5m;
			order.SyncPickWithOrder();
			AssertEquals("Precondition", 1, orderLine.PickLines.Count);
			AssertEquals("Sync should have reduced PickLineQuantity to match ordered qty.", 5m,
				orderLine.PickLineQuantity);
			AssertEquals("Sync should have reduced QuantityMet to match ordered qty.", 5m, orderLine.SumOfUnitsMet);
		}

		#endregion

		#region TestSyncPickWithOrder_IsDisabledForUSCustomsOrders

		[ExpectExceptionMessage(typeof(NotSupportedException),
			"Order.SyncPickWithOrder() is not supported for US Customs Orders.")]
		public void TestSyncPickWithOrder_IsDisabledForUSCustomsOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m, "", "ABC");
			var pick = Helper.CreatePickNew(order);

			order.SyncPickWithOrder();
		}

		#endregion

		#endregion

		#region TestRequiredDate_ReadOnly

		protected override void TestRequiredDate_ReadOnlyCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			using (WarehouseDataRegistry.Instance.AllowWarehouseOrderFinalisationDateUpTo30DaysInTheFuture
					   .SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, true))
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				Helper.CreateWhsOrderLine(order, data.Part1, 5);
				Helper.CreatePickNew(order);
				data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
				AssertEquals("Precondition", false, order.RequiredDateInfo.ReadOnly);

				order.FinaliseDocket();
				AssertEquals(true, order.IsFinalised);
				AssertEquals(true, order.RequiredDateInfo.ReadOnly);
			}
		}

		#endregion

		#region TestWD_IsLoadingRequired_ReadOnly

		public void TestWD_IsLoadingRequired_ReadOnly()
		{
			TestReadOnly(d => d.WD_IsLoadingRequiredInfo, false, false, true, true);
		}

		#endregion

		#region TestWD_QualityAuditRequired

		public void TestWD_QualityAuditRequired_ReadOnly()
		{
			TestReadOnly(d => d.WD_QualityAuditRequiredInfo, false, false, true, true);
		}

		public void TestWD_QualityAuditRequired_DefaultValue()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			AssertEquals("The default value should be false", false, order.WD_QualityAuditRequired);
		}

		public void TestWD_QualityAuditRequired_SetValue()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_QualityAuditRequired = true;
			AssertEquals("Now the value of the property should be true", true, order.WD_QualityAuditRequired);
		}

		#endregion

		#region TestDistributionCentreForOrder

		public void TestDistributionCentreForOrder()
		{
			var order = Factory.New<WhsOrder>();
			AssertDistributuionCentreSecurity(order, DocketStatus.Codes.Entered, false, false);

			var pick = Factory.New<WhsPick>();
			pick.WP_PickStatus = PickStatus.Codes.Finalised;
			order.WD_WP = pick.PK;
			AssertDistributuionCentreSecurity(order, DocketStatus.Codes.Finalised, false, true);
			AssertDistributuionCentreSecurity(order, DocketStatus.Codes.Cancelled, false, true);
		}

		static void AssertDistributuionCentreSecurity(WhsOrder order, ZString status,
			bool expectedReadOnlyPostFinaliseEditEnabled, bool expectedReadOnlyPostFinaliseEditDisabled)
		{
			order.WD_DocketStatus = status;
			Env.Security.WhsReleasePostFinaliseEdit.IsAllowed = true;
			AssertEquals(expectedReadOnlyPostFinaliseEditEnabled, order.DistributionCentreDocAddress.ReadOnly);

			Env.Security.WhsReleasePostFinaliseEdit.IsAllowed = false;
			AssertEquals(expectedReadOnlyPostFinaliseEditDisabled, order.DistributionCentreDocAddress.ReadOnly);
		}

		public void TestDistributionCentreDocAddress_ReadOnlyIsLazyTriggered()
		{
			var order = Factory.New<WhsOrder>();
			AssertNotNull("Poke & Precondition", order.DistributionCentreDocAddress);

			AssertPersistentPropertiesHitCount("Getting DistricutionCentreDocAddress should not trigger property hits.",
				0, () => _ = order.DistributionCentreDocAddress);

			var hits = GetPersistentPropertiesHitCount(() => _ = order.DistributionCentreDocAddress.ReadOnly);
			AssertEquals("Poking at ReadOnly property should trigger state calculation.", true, hits > 0);
		}

		#endregion

		#region TestOnDocketSubTypeChanged

		public void TestOnDocketSubTypeChanged_CUS()
		{
			TestOnDocketSubTypeChangedCore(OrderType.Codes.Customs,
				expectedValue: WhsBondedWarehouseAttributeOutwardType.Codes.CNN);
		}

		public void TestOnDocketSubTypeChanged_CPS()
		{
			TestOnDocketSubTypeChangedCore(OrderType.Codes.CustomsReleaseWithPermit,
				expectedValue: WhsBondedWarehouseAttributeOutwardType.Codes.CNN);
		}

		public void TestOnDocketSubTypeChanged_NormalWarehouse()
		{
			TestOnDocketSubTypeChangedCore(OrderType.Codes.Order, expectedValue: string.Empty);
		}

		void TestOnDocketSubTypeChangedCore(string docketSubType, string expectedValue)
		{
			var whs = Helper.CreateWarehouse("whs1");
			var order = Factory.New<WhsOrder>();
			var orderline = order.Lines.AddNew();
			order.WD_WW_Whs = whs.PK;
			order.WD_DocketSubType = docketSubType;
			AssertEquals("Should set default for Customs jobs.", expectedValue, orderline.CustomsData.WB_OutwardType);
		}

		public void TestOnDocketSubTypeChanged_NotchangeIfAlreadySet()
		{
			var whs = Helper.CreateWarehouse("whs1");
			var order = Factory.New<WhsOrder>();
			var orderline = order.Lines.AddNew();
			order.WD_WW_Whs = whs.PK;
			orderline.CustomsData.WB_OutwardType = WhsBondedWarehouseAttributeOutwardType.Codes.TOF;
			order.WD_DocketSubType = OrderType.Codes.Customs;
			AssertEquals("Should change existing value.", WhsBondedWarehouseAttributeOutwardType.Codes.TOF,
				orderline.CustomsData.WB_OutwardType);
		}

		#endregion

		#region Relinquish Permit

		#region TestDoesNotRelinquishPermitWhenOrderDetachedIfWarehouseTypeIsNotFTZ

		public void TestDoesNotRelinquishPermitWhenOrderDetachedIfWarehouseTypeIsNotFTZ()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "US";
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m, "", "ABC");

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			pick.Orders.Remove(order);

			var permitService = new Mock<IPermitService>();

			using (ObjectFactory.Substitute(permitService.Object))
			{
				AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);
				permitService.Verify(m => m.RelinquishPermitTransactions(It.IsAny<IEnumerable<IPermitTransactionDetail>>()),
					Times.Never);
			}

			permitService.VerifyAll();
		}

		#endregion

		#region TestDoesNotRelinquishPermitWhenOrderDetachedIfWarehouseCountryIsNotFTZSupported

		public void TestDoesNotRelinquishPermitWhenOrderDetachedIfWarehouseCountryIsNotFTZSupported()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "AU";
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m, "", "ABC");

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			pick.Orders.Remove(order);

			var permitService = new Mock<IPermitService>();

			using (ObjectFactory.Substitute(permitService.Object))
			{
				AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);
				permitService.Verify(m => m.RelinquishPermitTransactions(It.IsAny<IEnumerable<IPermitTransactionDetail>>()),
					Times.Never);
			}

			permitService.VerifyAll();
		}

		#endregion

		#region TestDoesNotRelinquishPermitWhenOrderDetachedIfOrderSubTypeIsNotCUS

		public void TestDoesNotRelinquishPermitWhenOrderDetachedIfOrderSubTypeIsNotCUS()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.Order;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			pick.Orders.Remove(order);

			var permitService = new Mock<IPermitService>();

			using (ObjectFactory.Substitute(permitService.Object))
			{
				AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);
				permitService.Verify(m => m.RelinquishPermitTransactions(It.IsAny<IEnumerable<IPermitTransactionDetail>>()),
					Times.Never);
			}

			permitService.VerifyAll();
		}

		#endregion

		#region TestDoesNotRelinquishPermitWhenOrderDetachedAndIsNotInTheDatabase

		public void TestDoesNotRelinquishPermitWhenOrderDetachedAndIsNotInTheDatabase()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();
			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var permitService = new Mock<IPermitService>();
			permitService.Setup(m => m.IsPermitAvailable(It.IsAny<IEnumerable<IPermitWithdrawRequest>>())).Returns(
				new WhsPermitWithdrawRequestResponseResultForTest
				{
					Responses = new[]
					{
						new WhsPermitWithdrawRequestResponseForTest(orderLine, SuccessOrFailure.Success, 10,
							"OUT")
					}
				}
			);

			using (ObjectFactory.Substitute(permitService.Object))
			{
				var pick = Helper.CreatePickNew(order);
				pick.Orders.Remove(order);

				AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);

				permitService.Verify(m => m.RelinquishPermitTransactions(It.IsAny<IEnumerable<IPermitTransactionDetail>>()),
					Times.Never);
			}

			permitService.VerifyAll();
		}

		#endregion

		#region TestRelinquishPermitWhenOrderDetached_Success

		public void TestRelinquishPermitWhenOrderDetached_Success()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();

			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketID = "W00000001";
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			orderLine.CustomsData.WB_CustomsQty = 50m;
			orderLine.CustomsData.WB_Tariff = "1020304050";

			WhsPick pick;
			using (PermitServiceTestHelper.MockServiceToGetPermits())
			{
				pick = Helper.CreatePickNew(order);
				AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);
			}

			AssertEquals("PreCondition:EntryKey", "W00000001_1", orderLine.CustomsData.WB_EntryKey);
			AssertEquals("PreCondition:EntryLineNo", ZShort.Zero, orderLine.CustomsData.WB_EntryLineNo);

			ZString[] permitNumbers = null;

			var permitService = new Mock<IPermitService>();

			SuccessOrFailure method(IEnumerable<IPermitTransactionDetail> args)
			{
				permitNumbers = args.ToArray().Select(x => x.PermitTransactionRefNumber).ToArray();
				return SuccessOrFailure.Success;
			}

			permitService.Setup(m => m.RelinquishPermitTransactions(It.IsAny<IEnumerable<IPermitTransactionDetail>>()))
				.Returns<IEnumerable<IPermitTransactionDetail>>(method);

			pick.Orders.Remove(order);

			using (ObjectFactory.Substitute(permitService.Object))
			{
				AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);
				AssertContainsExactElementsInAnyOrder(
					new[] { WhsPermitWithdrawalRequestDetail.GetPermitTransactionRefNumber(orderLine) }, permitNumbers);
				AssertEquals("OutwardEntryNumber", "", orderLine.CustomsData.WB_EntryKey);
				AssertEquals("OutwardEntryLineNumber", ZShort.Zero, orderLine.CustomsData.WB_EntryLineNo);
			}

			var relinquishedOrderLine = new BusinessObjectFactory().Load<WhsOrderLine>(orderLine.PK);
			AssertEquals("EntryKey has been cleared.", ZString.Empty, relinquishedOrderLine.CustomsData.WB_EntryKey);
			AssertEquals("EntryLineNo has been cleared.", ZShort.Zero,
				relinquishedOrderLine.CustomsData.WB_EntryLineNo);
		}

		#endregion

		#region TestRelinquishPermitWhenOrderDetached_Fail

		public void TestRelinquishPermitWhenOrderDetached_Fail()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();

			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			order.WD_DocketID = "W00000001";

			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			orderLine.CustomsData.WB_CustomsQty = 50m;
			orderLine.CustomsData.WB_Tariff = "1020304050";

			WhsPick pick;
			using (PermitServiceTestHelper.MockServiceToGetPermits())
			{
				pick = Helper.CreatePickNew(order);
				AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);
			}

			AssertEquals("PreCondition:EntryKey", "W00000001_1", orderLine.CustomsData.WB_EntryKey);
			AssertEquals("PreCondition:EntryLineNo", ZShort.Zero, orderLine.CustomsData.WB_EntryLineNo);

			ZString[] permitNumbers = null;
			SuccessOrFailure method(IEnumerable<IPermitTransactionDetail> args)
			{
				permitNumbers = args.ToArray().Select(x => x.PermitTransactionRefNumber).ToArray();
				return SuccessOrFailure.Failure;
			}
			var permitService = new Mock<IPermitService>();

			permitService.Setup(m => m.RelinquishPermitTransactions(It.IsAny<IEnumerable<IPermitTransactionDetail>>()))
				.Returns<IEnumerable<IPermitTransactionDetail>>(method);

			pick.Orders.Remove(order);

			using (ObjectFactory.Substitute(permitService.Object))
			{
				AssertEquals("OutwardEntryNumber", "", orderLine.CustomsData.WB_EntryKey);
				AssertEquals("OutwardEntryLineNumber", ZShort.Zero, orderLine.CustomsData.WB_EntryLineNo);
				Helper.AssertZCannotSaveExceptionThrown("The Order(s) you detached could not relinquish its Permits. Close and Re-open the form and try again.", Factory.Save);
				AssertContainsExactElementsInAnyOrder(
					new[] { WhsPermitWithdrawalRequestDetail.GetPermitTransactionRefNumber(orderLine) }, permitNumbers);
			}

			var relinquishedOrderLine = new BusinessObjectFactory().Load<WhsOrderLine>(orderLine.PK);
			AssertEquals("EntryKey is not changed.", "W00000001_1", relinquishedOrderLine.CustomsData.WB_EntryKey);
			AssertEquals("EntryLineNo is not changed.", ZShort.Zero, relinquishedOrderLine.CustomsData.WB_EntryLineNo);
		}

		#endregion

		#region TestRelinquishPermitWhenOrderDetached_Fail_Redone

		public void TestRelinquishPermitWhenOrderDetached_Fail_Redone()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();

			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			orderLine.CustomsData.WB_CustomsQty = 50m;
			orderLine.CustomsData.WB_Tariff = "1020304050";

			WhsPick pick;
			using (PermitServiceTestHelper.MockServiceToGetPermits())
			{
				pick = Helper.CreatePickNew(order);
				AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);
			}

			var permitService = new Mock<IPermitService>();
			permitService
				.Setup(m => m.RelinquishPermitTransactions(It.IsAny<IEnumerable<IPermitTransactionDetail>>()))
				.Returns(SuccessOrFailure.Failure);
			pick.Orders.Remove(order);

			using (ObjectFactory.Substitute(permitService.Object))
			{
				Helper.AssertZCannotSaveExceptionThrown("The Order(s) you detached could not relinquish its Permits. Close and Re-open the form and try again.", Factory.Save);
			}

			permitService.Setup(m => m.RelinquishPermitTransactions(It.IsAny<IEnumerable<IPermitTransactionDetail>>()))
				.Returns(SuccessOrFailure.Success);
			using (ObjectFactory.Substitute(permitService.Object))
			{
				AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);
			}
		}

		#endregion

		#region TestRelinquishPermitWhenOrderDetached_WithExceptionDuringOnSaving

		public void TestRelinquishPermitWhenOrderDetached_WithExceptionDuringOnSaving()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();

			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			orderLine.CustomsData.WB_CustomsQty = 50m;
			orderLine.CustomsData.WB_Tariff = "1020304050";

			WhsPick pick;
			using (PermitServiceTestHelper.MockServiceToGetPermits())
			{
				pick = Helper.CreatePickNew(order);
				AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);
			}

			var permitService = new Mock<IPermitService>();

			using (ObjectFactory.Substitute(permitService.Object))
			{
				pick.Orders.Remove(order);
				BusinessObjectFactory.SavingEventHandler handler = f =>
				{
					f.ServiceContainer.AddAfterOnSavingService(new ServiceThatThrowsException(f));
				};
				Factory.Saving += handler;

				Helper.AssertZCannotSaveExceptionThrown("Test", Factory.Save);
				Factory.Saving -= handler;

				pick.Orders.Add(order); // test that the subscribed order list is cleared on factory save failure
				AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);

				permitService.Verify(m => m.RelinquishPermitTransactions(It.IsAny<IEnumerable<IPermitTransactionDetail>>()),
					Times.Never);
				permitService.Verify(m => m.TryGetPermits(It.IsAny<IEnumerable<IPermitWithdrawRequest>>()), Times.Never);
			}

			permitService.VerifyAll();
		}

		// throw exception before committing transaction
		class ServiceThatThrowsException : IAfterOnSavingBOProcessingService
		{
			public ServiceThatThrowsException(BusinessObjectFactory factory)
			{
				Factory = factory;
			}

			readonly BusinessObjectFactory Factory;

			void IAfterOnSavingBOProcessingService.ProcessBusinesObjects(
				IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				Factory.ServiceContainer.RemoveAfterOnSavingService<ServiceThatThrowsException>();
				throw new ZCannotSaveException("Test", "Test", ExceptionType.BusinessFailure);
			}
		}

		#endregion

		#region TestRelinquishPermitWhenOrderDetached_IsRolledBackIfExceptionDuringSave

		public void TestRelinquishPermitWhenOrderDetached_IsRolledBackIfExceptionDuringSave()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();

			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			orderLine.CustomsData.WB_CustomsQty = 50m;
			orderLine.CustomsData.WB_Tariff = "1020304050";

			WhsPick pick;
			using (PermitServiceTestHelper.MockServiceToGetPermits())
			{
				pick = Helper.CreatePickNew(order);
				AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);
			}

			var permitService = new Mock<IPermitService>();
			permitService
				.Setup(m => m.RelinquishPermitTransactions(It.IsAny<IEnumerable<IPermitTransactionDetail>>()))
				.Returns(SuccessOrFailure.Success);

			IPermitWithdrawRequest[] permitRequests = null;

			permitService.Setup(m => m.TryGetPermits(It.IsAny<IEnumerable<IPermitWithdrawRequest>>()))
				.Returns<IEnumerable<IPermitWithdrawRequest>>(args =>
				{
					permitRequests = args.ToArray();
					return new WhsPermitWithdrawRequestResponseResultForTest()
					{
						Responses = new IPermitWithdrawRequestResponse[]
						{
							new WhsPermitWithdrawRequestResponseForTest(orderLine, SuccessOrFailure.Success, 1000m,
								"111")
						}
					};
				});

			using (ObjectFactory.Substitute(permitService.Object))
			{
				pick.Orders.Remove(order);
				BusinessObjectFactory.SavingEventHandler handler = f =>
				{
					f.ServiceContainer.AddAfterSaveInTransactionService(
						new ServiceThatThrowsExceptionBeforeCommit(f));
				};
				Factory.Saving += handler;

				Helper.AssertZCannotSaveExceptionThrown("Test", Factory.Save); // should fail with test exception
				Factory.Saving -= handler;

				// should rollback the relinquishing of permits as the save failed.
				permitService.VerifyAll();
				AssertContainsExactElementsInAnyOrder(
					new[] { WhsPermitWithdrawalRequestDetail.GetPermitTransactionRefNumber(orderLine) },
					permitRequests.Select(r => r.PermitTransactionRefNumber));
			}
		}

		// throw exception before committing transaction
		class ServiceThatThrowsExceptionBeforeCommit : IAfterSaveInTransactionService
		{
			public ServiceThatThrowsExceptionBeforeCommit(BusinessObjectFactory factory)
			{
				Factory = factory;
			}

			readonly BusinessObjectFactory Factory;

			void IAfterSaveInTransactionService.DoFinalCheckBeforeCommit(
				IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				Factory.ServiceContainer.RemoveAfterSaveInTransactionService<ServiceThatThrowsExceptionBeforeCommit>();
				throw new ZCannotSaveException("Test", "Test", ExceptionType.BusinessFailure);
			}
		}

		#endregion

		#region TestRelinquishPermitWhenOrderDetached_IsRolledBackIfExceptionDuringSave_RetryIfRollbackFailed

		public void
			TestRelinquishPermitWhenOrderDetached_IsRolledBackIfExceptionDuringSave_RetryIfRollbackFailed_RetryOnce()
		{
			RelinquishPermitWhenOrderDetached_IsRolledBackIfExceptionDuringSave_RetryIfRollbackFailed(1);
		}

		public void
			TestRelinquishPermitWhenOrderDetached_IsRolledBackIfExceptionDuringSave_RetryIfRollbackFailed_RetryTwice()
		{
			RelinquishPermitWhenOrderDetached_IsRolledBackIfExceptionDuringSave_RetryIfRollbackFailed(2);
		}

		public void
			TestRelinquishPermitWhenOrderDetached_IsRolledBackIfExceptionDuringSave_RetryIfRollbackFailed_RetryThreeTimes()
		{
			RelinquishPermitWhenOrderDetached_IsRolledBackIfExceptionDuringSave_RetryIfRollbackFailed(3);
		}

		void RelinquishPermitWhenOrderDetached_IsRolledBackIfExceptionDuringSave_RetryIfRollbackFailed(int retryTimes)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs = Helper.CreateFTZWarehouseInUS();

			var order = Helper.CreateWhsOrder(data.Org1, whs);
			order.WD_DocketSubType = OrderType.Codes.CustomsReleaseWithPermit;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			orderLine.CustomsData.WB_CustomsQty = 50m;
			orderLine.CustomsData.WB_Tariff = "1020304050";

			WhsPick pick;
			using (PermitServiceTestHelper.MockServiceToGetPermits())
			{
				pick = Helper.CreatePickNew(order);
				AssertNoExceptionThrown("Save Pick With No Exception.", Factory.Save);
			}

			var permitService = new Mock<IPermitService>();
			permitService
				.Setup(m => m.RelinquishPermitTransactions(It.IsAny<IEnumerable<IPermitTransactionDetail>>()))
				.Returns(SuccessOrFailure.Success);

			IPermitWithdrawRequest[] permitRequests = null;
			int times = 0;

			for (int i = 0; i <= times; i++)
			{
				if (i <= retryTimes)
				{
					permitService.Setup(m => m.TryGetPermits(It.IsAny<IEnumerable<IPermitWithdrawRequest>>()))
						.Returns<IEnumerable<IPermitWithdrawRequest>>(args =>
						{
							permitRequests = args.ToArray();
							times++;
							return times <= retryTimes
								? new WhsPermitWithdrawRequestResponseResultForTest()
								{
									Responses = new IPermitWithdrawRequestResponse[]
									{
										new WhsPermitWithdrawRequestResponseForTest(orderLine, SuccessOrFailure.Failure,
											1000m, "111")
									}
								}
								: new WhsPermitWithdrawRequestResponseResultForTest()
								{
									Responses = new IPermitWithdrawRequestResponse[]
									{
										new WhsPermitWithdrawRequestResponseForTest(orderLine, SuccessOrFailure.Success,
											1000m, "111")
									}
								};
						});
				}
			}

			using (ObjectFactory.Substitute(permitService.Object))
			{
				pick.Orders.Remove(order);
				BusinessObjectFactory.SavingEventHandler handler = f =>
				{
					f.ServiceContainer.AddAfterSaveInTransactionService(
						new ServiceThatThrowsExceptionBeforeCommit(f));
				};
				Factory.Saving += handler;
				Helper.AssertZCannotSaveExceptionThrown("Test", Factory.Save); // should fail with test exception
				Factory.Saving -= handler;

				permitService.VerifyAll();

				AssertContainsExactElementsInAnyOrder(
					new[] { WhsPermitWithdrawalRequestDetail.GetPermitTransactionRefNumber(orderLine) },
					permitRequests.Select(r => r.PermitTransactionRefNumber));

				permitService.Verify(m => m.TryGetPermits(It.IsAny<IEnumerable<IPermitWithdrawRequest>>()),
					Times.Exactly(retryTimes < 3 ? retryTimes + 1 : 3));
			}
		}

		#endregion

		#endregion

		#region TestRelatedOrgPartyScreeningStatusCollection

		public void TestRelatedOrgPartyScreeningStatusCollection_AllScreenPartyLogsInclude()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();
			Assert(order.RelatedOrgPartyScreeningStatusCollection.Count == 0);

			var orgAddressPks = order.DocAddresses.Cast<JobDocAddress>().Select(o => o.E2_OA_Address);
			AssertCollectionContains(order.ConsigneeAddress.PK, orgAddressPks);
			var addressScreenPartyLog = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			addressScreenPartyLog.PJ_ParentID = order.Consignee.PK;
			addressScreenPartyLog.PJ_ParentTableCode = "OH";
			Factory.Save();
			Assert(order.RelatedOrgPartyScreeningStatusCollection.Count == 1);
			AssertCollectionContains(order.Consignee.PK,
				order.RelatedOrgPartyScreeningStatusCollection.Cast<IRelatedOrgPartyScreeningStatus>()
					.Select(o => o.PJ_ParentID));

			var warehouseAddress = order.Warehouse.DocAddresses.AddNew();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			warehouseAddress.E2_OA_Address = orgAddress.PK;
			var warehouseAddressScreenPartyLog = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			warehouseAddressScreenPartyLog.PJ_ParentID = warehouseAddress.Organisation.PK;
			warehouseAddressScreenPartyLog.PJ_ParentTableCode = "OH";
			Factory.Save();
			Assert(order.RelatedOrgPartyScreeningStatusCollection.Count == 2);
			AssertCollectionContains(warehouseAddress.Organisation.PK,
				order.RelatedOrgPartyScreeningStatusCollection.Cast<IRelatedOrgPartyScreeningStatus>()
					.Select(o => o.PJ_ParentID));

			var localChargeScreenPartyLog = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			var jobLoader = new JobHeader.Loader(order);
			var jobHeader = jobLoader.TryCreateWithoutMutexForTestOnly();
			jobHeader.JH_ParentID = order.PK;
			jobHeader.JH_GC = GlbCompany.CurrentCompany.PK;
			localChargeScreenPartyLog.PJ_ParentID = order.JobHeader.LocalCharges.PK;
			localChargeScreenPartyLog.PJ_ParentTableCode = "OH";
			Factory.Save();
			Assert(order.RelatedOrgPartyScreeningStatusCollection.Count == 3);
			AssertCollectionContains(order.JobHeader.LocalCharges.PK,
				order.RelatedOrgPartyScreeningStatusCollection.Cast<IRelatedOrgPartyScreeningStatus>()
					.Select(o => o.PJ_ParentID));

			var forwarderScreenPartyLog = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			order.WD_OH_Forwarder = forwarder.PK;
			forwarderScreenPartyLog.PJ_ParentID = order.Forwarder.PK;
			forwarderScreenPartyLog.PJ_ParentTableCode = "OH";
			Factory.Save();
			Assert(order.RelatedOrgPartyScreeningStatusCollection.Count == 4);
			AssertCollectionContains(order.Forwarder.PK,
				order.RelatedOrgPartyScreeningStatusCollection.Cast<IRelatedOrgPartyScreeningStatus>()
					.Select(o => o.PJ_ParentID));
		}

		public void TestRelatedOrgPartyScreeningStatusCollection_SelfAndAllRelatedChildJobsInclude()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();
			Assert(order.RelatedOrgPartyScreeningStatusCollection.Count == 0);

			var selfJobLog = Factory.NewWithValidTestData<StmEntityScreeningLog>();
			selfJobLog.PJ_ParentID = order.PK;
			selfJobLog.PJ_ParentTableCode = "JS";
			Factory.Save();
			Assert(order.RelatedOrgPartyScreeningStatusCollection.Count == 1);
			AssertCollectionContains(order.PK,
				order.RelatedOrgPartyScreeningStatusCollection.Cast<IRelatedOrgPartyScreeningStatus>()
					.Select(o => o.PJ_ParentID));
		}

		#endregion

		#region TestAddOuterNonPalletPackage

		public void TestAddOuterNonPalletPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);

			Helper.CreatePickNew(order);
			AssertEquals("Precondition", ZShort.Zero, order.WD_PalletsSent);
			AssertEquals("Precondition", 0, order.WD_PackagesSent);
			AssertEquals("Precondition", "", order.WD_F3_NKTotalPackType);
			var outer = order.PackageJob.Packages.AddNew();
			outer.KP_PackageID = "A1";
			outer.KP_PackageQty = 5;
			outer.KP_F3_NKPackType = "BAG";

			AssertEquals("No pallets must be in outer.", ZShort.Zero, order.WD_PalletsSent);
			AssertEquals("Five Package must be sent.", 5, order.WD_PackagesSent);
			AssertEquals("One pack type is in outer.", "BAG", order.WD_F3_NKTotalPackType);

			var outerWithSamePackType = order.PackageJob.Packages.AddNew();
			outerWithSamePackType.KP_PackageID = "A2";
			outerWithSamePackType.KP_F3_NKPackType = "CAS";
			AssertEquals("Mutiple outer non pallets must not update pallets sent.", ZShort.Zero, order.WD_PalletsSent);
			AssertEquals("Mutiple outer non pallets must update WD_PackagesSent to six.", 6, order.WD_PackagesSent);
			AssertEquals("Mutiple outer non pallets must update WD_F3_NKTotalPackType to ''.", "",
				order.WD_F3_NKTotalPackType);

			var outerDifferentPackType = order.PackageJob.Packages.AddNew();
			outerDifferentPackType.KP_PackageID = "A3";
			outerDifferentPackType.KP_F3_NKPackType = "CAS";
			AssertEquals("Mutiple outer non pallets must not update pallets sent.", ZShort.Zero, order.WD_PalletsSent);
			AssertEquals("Mutiple outer non pallets must update WD_PackagesSent to seven.", 7, order.WD_PackagesSent);
			AssertEquals("There are 2 CAS and 5 BAG, therefore WD_F3_NKTotalPackType must be ''.", "",
				order.WD_F3_NKTotalPackType);
		}

		#endregion

		#region TestAddOuterPalletPackage

		public void TestAddOuterPalletPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);

			Helper.CreatePickNew(order);
			AssertEquals("Precondition", ZShort.Zero, order.WD_PalletsSent);
			AssertEquals("Precondition", 0, order.WD_PackagesSent);
			AssertEquals("Precondition", "", order.WD_F3_NKTotalPackType);
			var outer = order.PackageJob.Packages.AddNew();
			outer.KP_PackageID = "A1";
			outer.KP_PackageQty = 4;
			outer.KP_F3_NKPackType = "PLT";

			AssertEquals("One outer pallet must be sent.", (ZShort)4, order.WD_PalletsSent);
			AssertEquals("One outer pallet must be sent.", 0, order.WD_PackagesSent);
			AssertEquals("Sent packages are PLT type.", "", order.WD_F3_NKTotalPackType);

			var outerWithSamePackType = order.PackageJob.Packages.AddNew();
			outerWithSamePackType.KP_PackageID = "A2";
			outerWithSamePackType.KP_F3_NKPackType = "PLT";
			AssertEquals("Multiple outer pallets must update WD_PalletsSent to five.", (ZShort)5, order.WD_PalletsSent);
			AssertEquals("Multiple outer packages must update WD_PackagesSent to ''.", 0, order.WD_PackagesSent);
			AssertEquals("All outer WD_F3_NKTotalPackType are PLT so must stay as empty.", "",
				order.WD_F3_NKTotalPackType);

			var outerDifferentPackType = order.PackageJob.Packages.AddNew();
			outerDifferentPackType.KP_PackageID = "A3";
			outerDifferentPackType.KP_F3_NKPackType = "CAS";
			AssertEquals("There must be five pallets sending out.", (ZShort)5, order.WD_PalletsSent);
			AssertEquals("5 PLTs and 1 CAS must set package sent to 1.", 1, order.WD_PackagesSent);
			AssertEquals("WD_F3_NKTotalPackType must be 'CAS' because there is only one non PLT pack in outer.", "CAS",
				order.WD_F3_NKTotalPackType);
		}

		#endregion

		#region TestAddInnerPalletPackage

		public void TestAddInnerPalletPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);

			Helper.CreatePickNew(order);
			AssertEquals("Precondition", ZShort.Zero, order.WD_PalletsSent);
			AssertEquals("Precondition", 0, order.WD_PackagesSent);
			AssertEquals("Precondition", "", order.WD_F3_NKTotalPackType);
			var outer = order.PackageJob.Packages.AddNew();
			outer.KP_PackageID = "A1";
			outer.KP_F3_NKPackType = "PLT";

			AssertEquals("One outer pallet must be sent.", (ZShort)1, order.WD_PalletsSent);
			AssertEquals("Zero outer pallet must be sent.", 0, order.WD_PackagesSent);
			AssertEquals("Sent packages are empty type.", "", order.WD_F3_NKTotalPackType);

			var innerPLT = outer.Packages.AddNew();
			innerPLT.KP_PackageID = "A2";
			innerPLT.KP_F3_NKPackType = "PLT";
			AssertEquals("Inner pallet must not change pallet sent.", (ZShort)1, order.WD_PalletsSent);
			AssertEquals("Inner pallet must not change packages sent.", 0, order.WD_PackagesSent);
			AssertEquals("Inner pallet must not change total pack type.", "", order.WD_F3_NKTotalPackType);

			var innerNonPallet = outer.Packages.AddNew();
			innerNonPallet.KP_PackageID = "A3";
			innerNonPallet.KP_F3_NKPackType = "CAS";
			AssertEquals("Inner non pallet must not change pallet sent.", (ZShort)1, order.WD_PalletsSent);
			AssertEquals("Inner non pallet must not change packages sent.", 0, order.WD_PackagesSent);
			AssertEquals("Inner non pallet must not change total pack type.", "", order.WD_F3_NKTotalPackType);
		}

		#endregion

		#region TestMoveOuterPalletToInnerPackage

		public void TestMoveOuterPalletToInnerPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);

			Helper.CreatePickNew(order);
			var outer = order.PackageJob.Packages.AddNew();
			outer.KP_PackageID = "A1";
			outer.KP_F3_NKPackType = "PLT";

			var outerPLT = order.PackageJob.Packages.AddNew();
			outerPLT.KP_PackageID = "A2";
			outerPLT.KP_F3_NKPackType = "PLT";

			AssertEquals("Precondition", (ZShort)2, order.WD_PalletsSent);
			AssertEquals("Precondition", 0, order.WD_PackagesSent);
			AssertEquals("Precondition", "", order.WD_F3_NKTotalPackType);

			outerPLT.KP_KP_ParentPackage = outer.PK;
			AssertEquals("Moving outer pallet to inner must reduce pallets sent.", (ZShort)1, order.WD_PalletsSent);
			AssertEquals("Moving outer pallet to inner must reduce packages sent.", 0, order.WD_PackagesSent);
			AssertEquals("Total pack type must be empty since there is only one pallet.", "",
				order.WD_F3_NKTotalPackType);
		}

		#endregion

		#region TestMoveOuterNonPalletToInnerPackage

		public void TestMoveOuterNonPalletToInnerPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);

			Helper.CreatePickNew(order);
			var outer = order.PackageJob.Packages.AddNew();
			outer.KP_PackageID = "A1";
			outer.KP_F3_NKPackType = "BAS";

			var outerNonPLT = order.PackageJob.Packages.AddNew();
			outerNonPLT.KP_PackageID = "A2";
			outerNonPLT.KP_F3_NKPackType = "CAS";

			AssertEquals("Precondition", (ZShort)0, order.WD_PalletsSent);
			AssertEquals("Precondition", 2, order.WD_PackagesSent);
			AssertEquals("Precondition", "", order.WD_F3_NKTotalPackType);

			outerNonPLT.KP_KP_ParentPackage = outer.PK;
			AssertEquals("There are no outer pallets therefore pallets sent must be zero.", ZShort.Zero,
				order.WD_PalletsSent);
			AssertEquals("Moving outer pallet to inner must reduce packages sent.", 1, order.WD_PackagesSent);
			AssertEquals("Total pack type must be BAS since there is only one outer package.", "BAS",
				order.WD_F3_NKTotalPackType);
		}

		#endregion

		#region TestMoveInnerPalletToOuterPackage

		public void TestMoveInnerPalletToOuterPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);

			Helper.CreatePickNew(order);
			var outer = order.PackageJob.Packages.AddNew();
			outer.KP_PackageID = "A1";
			outer.KP_F3_NKPackType = "PLT";

			var innerPLT = outer.Packages.AddNew();
			innerPLT.KP_PackageID = "A2";
			innerPLT.KP_F3_NKPackType = "PLT";

			AssertEquals("Precondition", (ZShort)1, order.WD_PalletsSent);
			AssertEquals("Precondition", 0, order.WD_PackagesSent);
			AssertEquals("Precondition", "", order.WD_F3_NKTotalPackType);

			innerPLT.KP_KP_ParentPackage = ZGuid.Empty;
			innerPLT.KP_KJ_ParentPackageJob = order.PackageJob.PK;
			AssertEquals("Inner pallet moved to outer, therefore pallets sent is two.", (ZShort)2,
				order.WD_PalletsSent);
			AssertEquals("Moving inner pallet to outer increase packages sent to two.", 0, order.WD_PackagesSent);
			AssertEquals("All outer packages are PLT therefore, TotalPackType must be empty.", "",
				order.WD_F3_NKTotalPackType);
		}

		#endregion

		#region TestMoveInnerNonPalletToOuterPackage

		public void TestMoveInnerNonPalletToOuterPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);

			Helper.CreatePickNew(order);
			var outer = order.PackageJob.Packages.AddNew();
			outer.KP_PackageID = "A1";
			outer.KP_F3_NKPackType = "CAS";

			var innerNonPLT = outer.Packages.AddNew();
			innerNonPLT.KP_PackageID = "A2";
			innerNonPLT.KP_F3_NKPackType = "BAG";

			AssertEquals("Precondition", ZShort.Zero, order.WD_PalletsSent);
			AssertEquals("Precondition", 1, order.WD_PackagesSent);
			AssertEquals("Precondition", "CAS", order.WD_F3_NKTotalPackType);

			innerNonPLT.KP_KP_ParentPackage = ZGuid.Empty;
			innerNonPLT.KP_KJ_ParentPackageJob = order.PackageJob.PK;
			AssertEquals("Although Inner non pallet moved to outer, pallets sent must remain zero.", ZShort.Zero,
				order.WD_PalletsSent);
			AssertEquals("Moving inner non pallet to outer increase packages sent to two.", 2, order.WD_PackagesSent);
			AssertEquals("All outer packages are BAG and CAS therefore, TotalPackType must be ''.", "",
				order.WD_F3_NKTotalPackType);
		}

		#endregion

		#region TestDeleteInnerPackages

		public void TestDeleteInnerPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);

			Helper.CreatePickNew(order);
			var outerNonPLT = order.PackageJob.Packages.AddNew();
			outerNonPLT.KP_PackageID = "A1";
			outerNonPLT.KP_F3_NKPackType = "CAS";

			var outerPLT = order.PackageJob.Packages.AddNew();
			outerPLT.KP_PackageID = "B1";
			outerPLT.KP_F3_NKPackType = "PLT";

			var innerNonPLT = outerNonPLT.Packages.AddNew();
			innerNonPLT.KP_PackageID = "A2";
			innerNonPLT.KP_F3_NKPackType = "BAG";

			var innerPLT = outerNonPLT.Packages.AddNew();
			innerPLT.KP_PackageID = "A3";
			innerPLT.KP_F3_NKPackType = "PLT";

			AssertEquals("Precondition", (ZShort)1, order.WD_PalletsSent);
			AssertEquals("Precondition", 1, order.WD_PackagesSent);
			AssertEquals("Precondition", "CAS", order.WD_F3_NKTotalPackType);

			innerNonPLT.Delete();
			AssertEquals("Removing non pallet must not change palelts sent.", (ZShort)1, order.WD_PalletsSent);
			AssertEquals("Deleting inner non pallet or pallet must not change packages sent.", 1,
				order.WD_PackagesSent);
			AssertEquals("Deleting inner non pallet or pallet must not change total pack type.", "CAS",
				order.WD_F3_NKTotalPackType);

			innerPLT.Delete();
			AssertEquals("Removing non pallet must not change palelts sent.", (ZShort)1, order.WD_PalletsSent);
			AssertEquals("Deleting inner non pallet or pallet must not change packages sent.", 1,
				order.WD_PackagesSent);
			AssertEquals("Deleting inner non pallet or pallet must not change total pack type.", "CAS",
				order.WD_F3_NKTotalPackType);
		}

		#endregion

		#region TestDeleteOuterPackages

		public void TestDeleteOuterPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);

			Helper.CreatePickNew(order);
			var outerNonPLT = order.PackageJob.Packages.AddNew();
			outerNonPLT.KP_PackageID = "A1";
			outerNonPLT.KP_F3_NKPackType = "CAS";

			var outerPLT = order.PackageJob.Packages.AddNew();
			outerPLT.KP_PackageID = "B1";
			outerPLT.KP_F3_NKPackType = "PLT";

			AssertEquals("Precondition", (ZShort)1, order.WD_PalletsSent);
			AssertEquals("Precondition", 1, order.WD_PackagesSent);
			AssertEquals("Precondition", "CAS", order.WD_F3_NKTotalPackType);

			outerNonPLT.Delete();
			AssertEquals("Removing non pallet must not change pallets sent to.", (ZShort)1, order.WD_PalletsSent);
			AssertEquals("Deleting non pallet must change packages sent.", 0, order.WD_PackagesSent);
			AssertEquals("Deleting non pallet must change total pack type to ''.", "", order.WD_F3_NKTotalPackType);

			outerPLT.Delete();
			AssertEquals("Removing pallet must change pallets sent to Zero.", ZShort.Zero, order.WD_PalletsSent);
			AssertEquals("Deleting pallet must not change packages sent.", 0, order.WD_PackagesSent);
			AssertEquals("Deleting pallet must not change total pack type.", "", order.WD_F3_NKTotalPackType);
		}

		#endregion

		#region TestUpdateInnerPackages

		public void TestUpdateInnerPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);

			Helper.CreatePickNew(order);
			var outerNonPLT = order.PackageJob.Packages.AddNew();
			outerNonPLT.KP_PackageID = "A1";
			outerNonPLT.KP_F3_NKPackType = "CAS";

			var outerPLT = order.PackageJob.Packages.AddNew();
			outerPLT.KP_PackageID = "B1";
			outerPLT.KP_F3_NKPackType = "PLT";

			var innerNonPLT = outerNonPLT.Packages.AddNew();
			innerNonPLT.KP_PackageID = "A2";
			innerNonPLT.KP_F3_NKPackType = "BAG";

			var innerPLT = outerNonPLT.Packages.AddNew();
			innerPLT.KP_PackageID = "A3";
			innerPLT.KP_F3_NKPackType = "PLT";

			AssertEquals("Precondition", (ZShort)1, order.WD_PalletsSent);
			AssertEquals("Precondition", 1, order.WD_PackagesSent);
			AssertEquals("Precondition", "CAS", order.WD_F3_NKTotalPackType);

			innerPLT.KP_F3_NKPackType = "BAG";
			AssertEquals("Updating inner pallet type to BAG must not change pallet sent.", (ZShort)1,
				order.WD_PalletsSent);
			AssertEquals("Updating inner pallet type to BAG must not change packages sent.", 1, order.WD_PackagesSent);
			AssertEquals("Updating inner pallet type to BAG must not change total pack type.", "CAS",
				order.WD_F3_NKTotalPackType);

			innerNonPLT.KP_F3_NKPackType = "PLT";
			innerNonPLT.KP_PackageQty = 100;
			AssertEquals("Updating inner non pallet type to PLT must not change pallet sent.", (ZShort)1,
				order.WD_PalletsSent);
			AssertEquals("Deleting inner non pallet to PLT must not change packages sent.", 1, order.WD_PackagesSent);
			AssertEquals("Deleting inner non pallet to PLT must not change total pack type.", "CAS",
				order.WD_F3_NKTotalPackType);
		}

		#endregion

		#region TestUpdateOuterPackages

		public void TestUpdateOuterPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);

			Helper.CreatePickNew(order);
			var outerNonPLT = order.PackageJob.Packages.AddNew();
			outerNonPLT.KP_PackageID = "A1";
			outerNonPLT.KP_F3_NKPackType = "CAS";

			var outerPLT = order.PackageJob.Packages.AddNew();
			outerPLT.KP_PackageID = "B1";
			outerPLT.KP_F3_NKPackType = "PLT";

			AssertEquals("Precondition", (ZShort)1, order.WD_PalletsSent);
			AssertEquals("Precondition", 1, order.WD_PackagesSent);
			AssertEquals("Precondition", "CAS", order.WD_F3_NKTotalPackType);

			outerNonPLT.KP_F3_NKPackType = "PLT";
			outerNonPLT.KP_PackageQty = 5;
			AssertEquals("Updating outer non pallet package to pallet must change pallet sent.", (ZShort)6,
				order.WD_PalletsSent);
			AssertEquals("Updating outer non pallet package to pallet and qty must change package sent.", 0,
				order.WD_PackagesSent);
			AssertEquals("Updating outer non pallet package to pallet must change total pack type to ''.", "",
				order.WD_F3_NKTotalPackType);

			outerPLT.KP_F3_NKPackType = "CAS";
			outerPLT.KP_PackageQty = 4;
			AssertEquals("Updating outer pallet package to non pallet must change pallet sent.", (ZShort)5,
				order.WD_PalletsSent);
			AssertEquals("Updating outer pallet package to non pallet and qty is 4 must change package sent.", 4,
				order.WD_PackagesSent);
			AssertEquals("Updating outer pallet package to non pallet must change total pack type to 'CAS'.", "CAS",
				order.WD_F3_NKTotalPackType);
		}

		#endregion

		#region TestChangingPackageWeightOrVolumeUpdatesWeightSentAndVolumeSent

		public void TestChangingPackageWeightOrVolumeUpdatesWeightSentAndVolumeSent_WithPackageType()
		{
			var packtype = Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT"));
			packtype.F3_Weight = 15m;
			packtype.F3_Height = 2m;
			packtype.F3_Length = 2m;
			packtype.F3_Width = 2m;
			packtype.F3_UnitOfDimension = "M3"; // 8M3 Volume
			packtype.F3_UnitOfWeight = "KG";

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0.5m, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pallet = order.PackageJob.Packages.AddNew();
			AssertEquals("Precondition: Package is Pallet.", "PLT", pallet.KP_F3_NKPackType);
			AssertEquals("Weight is correct.", 35m, order.WD_WeightSent);
			AssertEquals("Weight is correct.", 35m, order.GrossWeightSent);
			AssertEquals("Volume is correct.", 8m, order.WD_CubicSent);
		}

		public void TestChangingPackageWeightOrVolumeUpdatesWeightSentAndVolumeSent_UsePackingWeightAndVolumeOn()
		{
			AssertChangingPackageWeightOrVolumeUpdatesWeightSentAndVolumeSent(usePackingWeightAndVolume: true);
		}

		public void TestChangingPackageWeightOrVolumeUpdatesWeightSentAndVolumeSent_UsePackingWeightAndVolumeOff()
		{
			AssertChangingPackageWeightOrVolumeUpdatesWeightSentAndVolumeSent(usePackingWeightAndVolume: false);
		}

		void AssertChangingPackageWeightOrVolumeUpdatesWeightSentAndVolumeSent(bool usePackingWeightAndVolume)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0.5m, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew("CTN");
			order.UsePackingWeightAndVolume = usePackingWeightAndVolume;
			package.KP_Weight = 4m;
			package.KP_Volume = 7m;

			if (usePackingWeightAndVolume)
			{
				AssertEquals("Should use Packing Weight.", 24m, order.WD_WeightSent);
				AssertEquals("Should use Packing Weight.", 24m, order.GrossWeightSent);
				AssertEquals("Should use Packing Volume.", 7m, order.WD_CubicSent);
			}
			else
			{
				AssertEquals("Should use Net Weight.", 20m, order.WD_WeightSent);
				AssertEquals("Should use Net Weight.", 24m, order.GrossWeightSent);
				AssertEquals("Should use Net Volume.", 5m, order.WD_CubicSent);
			}

			order.UsePackingWeightAndVolume = !usePackingWeightAndVolume;

			if (!usePackingWeightAndVolume)
			{
				AssertEquals("Should use Packing Weight.", 24m, order.WD_WeightSent);
				AssertEquals("Gross Weight should use Packing Weight.", 24m, order.GrossWeightSent);
				AssertEquals("Should use Packing Volume.", 7m, order.WD_CubicSent);
			}
			else
			{
				AssertEquals("Should use Net Weight.", 20m, order.WD_WeightSent);
				AssertEquals("Gross Weight should use Packing Weight.", 24m, order.GrossWeightSent);
				AssertEquals("Should use Net Volume.", 5m, order.WD_CubicSent);
			}

			package.KP_Volume = 3m;

			if (!usePackingWeightAndVolume)
			{
				AssertEquals("Should use Packing Weight.", 24m, order.WD_WeightSent);
				AssertEquals("Gross Weight should use Packing Weight.", 24m, order.GrossWeightSent);
				AssertEquals("Should use Net Volume as Packing Volume is smaller.", 5m, order.WD_CubicSent);
			}
			else
			{
				AssertEquals("Should use Net Weight.", 20m, order.WD_WeightSent);
				AssertEquals("Gross Weight should use Packing Weight.", 24m, order.GrossWeightSent);
				AssertEquals("Should use Net Volume.", 5m, order.WD_CubicSent);
			}
		}

		#endregion

		#region TestRemovingPackagesUpdatesWeightAndVolume

		public void TestRemovingPackagesUpdatesWeightAndVolume()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0.5m, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew("CTN");
			var package2 = order.PackageJob.Packages.AddNew("CTN");
			var package3 = order.PackageJob.Packages.AddNew("CTN");
			package1.KP_Weight = 2m;
			package2.KP_Weight = 1.5m;
			package3.KP_Weight = 3m;
			package1.KP_Volume = 6m;
			package2.KP_Volume = 4m;
			package3.KP_Volume = 4m;
			AssertEquals("Should use Packing Weight.", 26.5m, order.WD_WeightSent);
			AssertEquals("Gross Weight should use Packing Weight.", 26.5m, order.GrossWeightSent);
			AssertEquals("Should use Packing Volume.", 14m, order.WD_CubicSent);

			package3.Delete();
			AssertEquals("Weight should be updated after removing a Package.", 23.5m, order.WD_WeightSent);
			AssertEquals("Weight should be updated after removing a Package.", 23.5m, order.GrossWeightSent);
			AssertEquals("Volume should be updated after removing a Package.", 10m, order.WD_CubicSent);

			package2.Delete();
			AssertEquals("Weight should be updated after removing a Package.", 22m, order.WD_WeightSent);
			AssertEquals("Weight should be updated after removing a Package.", 22m, order.GrossWeightSent);
			AssertEquals("Volume should be updated after removing a Package.", 6m, order.WD_CubicSent);

			package1.Delete();
			AssertEquals("Weight should be updated to Net Weight after removing last Package.", 20m,
				order.WD_WeightSent);
			AssertEquals("Weight should be updated to Net Weight after removing last Package.", 20m,
				order.GrossWeightSent);
			AssertEquals("Volume should be updated to Net Volume after removing last Package.", 5m, order.WD_CubicSent);
		}

		public void TestRemovingPackagesUpdatesWeightAndVolume_WithMassPackageProcess()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0.5m, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew("CTN");
			var package2 = order.PackageJob.Packages.AddNew("CTN");
			var package3 = order.PackageJob.Packages.AddNew("CTN");
			package1.KP_Weight = 2m;
			package2.KP_Weight = 1.5m;
			package3.KP_Weight = 3m;
			package1.KP_Volume = 6m;
			package2.KP_Volume = 4m;
			package3.KP_Volume = 4m;
			AssertEquals("Should use Packing Weight.", 26.5m, order.WD_WeightSent);
			AssertEquals("Gross Weight should use Packing Weight.", 26.5m, order.GrossWeightSent);
			AssertEquals("Should use Packing Volume.", 14m, order.WD_CubicSent);

			using (order.PackageJob.InitiateMassPackageProcess())
			{
				order.PackageJob.Packages.DeleteAll();
				AssertEquals("Should be no change.", 26.5m, order.WD_WeightSent);
				AssertEquals("Should be no change.", 26.5m, order.GrossWeightSent);
				AssertEquals("Should be no change.", 14m, order.WD_CubicSent);
			}

			AssertEquals("Weight should be updated to Net Weight after removing all Packages after Package Process.",
				20m, order.WD_WeightSent);
			AssertEquals("Weight should be updated to Net Weight after removing all Packages after Package Process.",
				20m, order.GrossWeightSent);
			AssertEquals("Volume should be updated to Net Volume after removing all Packages after Package Process.",
				5m, order.WD_CubicSent);
		}

		#endregion

		#region TestUpdatePackageDataAndWeightAndVolume

		public void TestUpdatePackageDataAndWeightAndVolume()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);

			Helper.CreatePickNew(order);
			var outerNonPLT = order.PackageJob.Packages.AddNew();
			outerNonPLT.KP_PackageID = "A1";
			outerNonPLT.KP_F3_NKPackType = "CAS";

			var outerPLT = order.PackageJob.Packages.AddNew();
			outerPLT.KP_PackageID = "B1";
			outerPLT.KP_F3_NKPackType = "PLT";

			order.WD_PalletsSent = 100;
			order.WD_PackagesSent = 500;
			order.WD_F3_NKTotalPackType = "XYZ";
			AssertEquals("Precondition", (ZShort)100, order.WD_PalletsSent);
			AssertEquals("Precondition", 500, order.WD_PackagesSent);
			AssertEquals("Precondition", "XYZ", order.WD_F3_NKTotalPackType);

			order.UpdatePackageDataAndWeightAndVolume();
			AssertEquals("WD_PalletsSent must be changed.", (ZShort)1, order.WD_PalletsSent);
			AssertEquals("WD_PackagesSent must be changed.", 1, order.WD_PackagesSent);
			AssertEquals("WD_F3_NKTotalPackType must be changed.", "CAS", order.WD_F3_NKTotalPackType);
		}

		public void TestUpdatePackageDataAndWeightAndVolume_WhenMassPackageProcessRunning()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);

			Helper.CreatePickNew(order);
			var outerNonPLT = order.PackageJob.Packages.AddNew();
			outerNonPLT.KP_PackageID = "A1";
			outerNonPLT.KP_F3_NKPackType = "CAS";

			var outerPLT = order.PackageJob.Packages.AddNew();
			outerPLT.KP_PackageID = "B1";
			outerPLT.KP_F3_NKPackType = "PLT";

			order.WD_PalletsSent = 100;
			order.WD_PackagesSent = 500;
			order.WD_F3_NKTotalPackType = "XYZ";
			AssertEquals("Precondition", (ZShort)100, order.WD_PalletsSent);
			AssertEquals("Precondition", 500, order.WD_PackagesSent);
			AssertEquals("Precondition", "XYZ", order.WD_F3_NKTotalPackType);

			using (order.PackageJob.InitiateMassPackageProcess())
			{
				order.UpdatePackageDataAndWeightAndVolume();
				AssertEquals("Package values should not have updated.", (ZShort)100, order.WD_PalletsSent);
				AssertEquals("Package values should not have updated.", 500, order.WD_PackagesSent);
				AssertEquals("Package values should not have updated.", "XYZ", order.WD_F3_NKTotalPackType);
			}

			// After Mass Package Process is finished, Package Data should update.
			AssertEquals("WD_PalletsSent must be changed.", (ZShort)1, order.WD_PalletsSent);
			AssertEquals("WD_PackagesSent must be changed.", 1, order.WD_PackagesSent);
			AssertEquals("WD_F3_NKTotalPackType must be changed.", "CAS", order.WD_F3_NKTotalPackType);
		}

		public void TestUpdatePackageDataAndWeightAndVolume_UpdatesWeightAndVolume()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0.5m, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew("CAS");
			package1.KP_Weight = 4m;
			package1.KP_Volume = 10m;

			var package2 = order.PackageJob.Packages.AddNew("PLT");
			package2.KP_Weight = 3m;
			package2.KP_Volume = 7m;

			order.WD_CubicSent = 400m;
			order.WD_WeightSent = 500m;
			order.UpdatePackageDataAndWeightAndVolume();
			AssertEquals("WD_CubicSent should be changed.", 17m, order.WD_CubicSent);
			AssertEquals("WD_WeightSent should be changed.", 27m, order.WD_WeightSent);
		}

		public void TestUpdatePackageDataAndWeightAndVolume_UpdatesWeightAndVolume_WhenMassPackageProcessRunning()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0.5m, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew("CAS");
			package1.KP_Weight = 4m;
			package1.KP_Volume = 10m;

			var package2 = order.PackageJob.Packages.AddNew("PLT");
			package2.KP_Weight = 3m;
			package2.KP_Volume = 7m;

			order.WD_CubicSent = 400m;
			order.WD_WeightSent = 500m;

			using (order.PackageJob.InitiateMassPackageProcess())
			{
				order.UpdatePackageDataAndWeightAndVolume();
				AssertEquals("WD_CubicSent should not be changed.", 400m, order.WD_CubicSent);
				AssertEquals("WD_WeightSent should not be changed.", 500m, order.WD_WeightSent);
			}

			// After Mass Package Process is finished, Package Data should update.
			AssertEquals("WD_CubicSent should be changed.", 17m, order.WD_CubicSent);
			AssertEquals("WD_WeightSent should be changed.", 27m, order.WD_WeightSent);
		}

		public void TestUpdatePackageDataAndWeightAndVolume_UpdatesWeightAndVolume_WhenPackingUpdateSuspended()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetProductWeightAndVolume(data.Part1, 2m, "KG", 0.5m, "M3");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Helper.CreatePickNew(order);
			var package1 = order.PackageJob.Packages.AddNew("CAS");
			package1.KP_Weight = 4m;
			package1.KP_Volume = 10m;

			var package2 = order.PackageJob.Packages.AddNew("PLT");
			package2.KP_Weight = 3m;
			package2.KP_Volume = 7m;

			order.WD_CubicSent = 400m;
			order.WD_WeightSent = 500m;

			using (new SemaphoreManager(order.SuspendPackingUpdateSemaphore))
			{
				order.UpdatePackageDataAndWeightAndVolume();
				AssertEquals("WD_CubicSent should not be changed.", 400m, order.WD_CubicSent);
				AssertEquals("WD_WeightSent should not be changed.", 500m, order.WD_WeightSent);
			}

			// Semaphore disposal does not trigger Weight & Volume update
			AssertEquals("WD_CubicSent should not be changed.", 400m, order.WD_CubicSent);
			AssertEquals("WD_WeightSent should not be changed.", 500m, order.WD_WeightSent);
		}

		public void TestUpdatePackageDataAndWeightAndVolume_PalletHasOtherUOMType_Empty() =>
			TestUpdatePackageDataAndWeightAndVolume_PalletHasOtherUOMType(string.Empty);

		public void TestUpdatePackageDataAndWeightAndVolume_PalletHasOtherUOMType_SplitCase() =>
			TestUpdatePackageDataAndWeightAndVolume_PalletHasOtherUOMType(UOMPackTypesList.Codes
				.SplitCase); // Weird case, but "PLT" pack type still means pallet in some places

		public void TestUpdatePackageDataAndWeightAndVolume_PalletHasOtherUOMType_Case() =>
			TestUpdatePackageDataAndWeightAndVolume_PalletHasOtherUOMType(UOMPackTypesList.Codes
				.Case); // Weird case, but "PLT" pack type still means pallet in some places

		void TestUpdatePackageDataAndWeightAndVolume_PalletHasOtherUOMType(string uomType)
		{
			var palletType = Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "PLT");
			palletType.F3_UOMType = uomType;

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);

			Helper.CreatePickNew(order);
			var outerNonPLT = order.PackageJob.Packages.AddNew();
			outerNonPLT.KP_PackageQty = 3;
			outerNonPLT.KP_F3_NKPackType = "CAS";

			var outerPLT = order.PackageJob.Packages.AddNew();
			outerPLT.KP_PackageQty = 5;
			outerPLT.KP_F3_NKPackType = "PLT";

			order.WD_PalletsSent = 100;
			order.WD_PackagesSent = 500;
			order.WD_F3_NKTotalPackType = "XYZ";
			AssertEquals("Precondition", (ZShort)100, order.WD_PalletsSent);
			AssertEquals("Precondition", 500, order.WD_PackagesSent);
			AssertEquals("Precondition", "XYZ", order.WD_F3_NKTotalPackType);

			order.UpdatePackageDataAndWeightAndVolume();
			AssertEquals("WD_PalletsSent must be changed.", (ZShort)5, order.WD_PalletsSent);
			AssertEquals("WD_PackagesSent must be changed.", 3, order.WD_PackagesSent);
			AssertEquals("WD_F3_NKTotalPackType must be changed.", "CAS", order.WD_F3_NKTotalPackType);
		}

		public void TestUpdatePackageDataAndWeightAndVolume_OtherPackTypes()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var palletRefType1 = PackingHelper.CreateRefPackType("PL1", "PL1", 1m, 2m, 3m, Constants.Length.Feet, 7,
				Constants.Weight.Pounds, uomType: UOMPackTypesList.Codes.Pallet);
			var palletRefType2 = PackingHelper.CreateRefPackType("PL2", "PL2", 2m, 4m, 8m, Constants.Length.Feet, 14,
				Constants.Weight.Pounds, uomType: UOMPackTypesList.Codes.Pallet);
			var casRefType = PackingHelper.CreateRefPackType("C1S", "C1S", 2m, 4m, 8m, Constants.Length.Feet, 14,
				Constants.Weight.Pounds, uomType: UOMPackTypesList.Codes.Case);
			var spcRefType = PackingHelper.CreateRefPackType("S1C", "S1C", 2m, 4m, 8m, Constants.Length.Feet, 14,
				Constants.Weight.Pounds, uomType: UOMPackTypesList.Codes.SplitCase);
			var othRefType = PackingHelper.CreateRefPackType("OTH", "OTH", 2m, 4m, 8m, Constants.Length.Feet, 14,
				Constants.Weight.Pounds, uomType: string.Empty);

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);

			Helper.CreatePickNew(order);
			var casePackage = order.PackageJob.Packages.AddNew();
			casePackage.KP_PackageQty = 3;
			casePackage.KP_F3_NKPackType = "C1S";

			var outerPLT1 = order.PackageJob.Packages.AddNew();
			outerPLT1.KP_PackageQty = 5;
			outerPLT1.KP_F3_NKPackType = "PLT";

			var outerPLT2 = order.PackageJob.Packages.AddNew();
			outerPLT2.KP_PackageQty = 7;
			outerPLT2.KP_F3_NKPackType = "PL1";

			var outerPLT3 = order.PackageJob.Packages.AddNew();
			outerPLT3.KP_PackageQty = 9;
			outerPLT3.KP_F3_NKPackType = "PL2";

			var splitCasePackage = order.PackageJob.Packages.AddNew();
			splitCasePackage.KP_PackageQty = 11;
			splitCasePackage.KP_F3_NKPackType = "S1C";

			var otherPackage = order.PackageJob.Packages.AddNew();
			otherPackage.KP_PackageQty = 13;
			otherPackage.KP_F3_NKPackType = "OTH";

			order.WD_PalletsSent = 100;
			order.WD_PackagesSent = 500;
			order.WD_F3_NKTotalPackType = "XYZ";
			AssertEquals("Precondition", (ZShort)100, order.WD_PalletsSent);
			AssertEquals("Precondition", 500, order.WD_PackagesSent);
			AssertEquals("Precondition", "XYZ", order.WD_F3_NKTotalPackType);

			order.UpdatePackageDataAndWeightAndVolume();
			AssertEquals("WD_PalletsSent must be changed.", (ZShort)21, order.WD_PalletsSent);
			AssertEquals("WD_PackagesSent must be changed.", 27, order.WD_PackagesSent);
			AssertEquals("WD_F3_NKTotalPackType must be changed.", string.Empty, order.WD_F3_NKTotalPackType);
		}

		public void TestUpdatePackageDataAndWeightAndVolume_OtherPackTypes_CaseInsensitive()
		{
			var packingHelper = new PackingTestHelper(Factory);
			var palletRefType1 = PackingHelper.CreateRefPackType("PL1", "PL1", 1m, 2m, 3m, Constants.Length.Feet, 7,
				Constants.Weight.Pounds, uomType: UOMPackTypesList.Codes.Pallet);
			var palletRefType2 = PackingHelper.CreateRefPackType("PL2", "PL2", 2m, 4m, 8m, Constants.Length.Feet, 14,
				Constants.Weight.Pounds, uomType: UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);

			Helper.CreatePickNew(order);
			var outerNonPLT = order.PackageJob.Packages.AddNew();
			outerNonPLT.KP_PackageQty = 3;
			outerNonPLT.KP_F3_NKPackType = "CaS";

			var outerPLT1 = order.PackageJob.Packages.AddNew();
			outerPLT1.KP_PackageQty = 5;
			outerPLT1.KP_F3_NKPackType = "PlT";

			var outerPLT2 = order.PackageJob.Packages.AddNew();
			outerPLT2.KP_PackageQty = 7;
			outerPLT2.KP_F3_NKPackType = "Pl1";

			var outerPLT3 = order.PackageJob.Packages.AddNew();
			outerPLT3.KP_PackageQty = 9;
			outerPLT3.KP_F3_NKPackType = "pL2";

			order.WD_PalletsSent = 100;
			order.WD_PackagesSent = 500;
			order.WD_F3_NKTotalPackType = "XYZ";
			AssertEquals("Precondition", (ZShort)100, order.WD_PalletsSent);
			AssertEquals("Precondition", 500, order.WD_PackagesSent);
			AssertEquals("Precondition", "XYZ", order.WD_F3_NKTotalPackType);

			order.UpdatePackageDataAndWeightAndVolume();
			AssertEquals("WD_PalletsSent must be changed.", (ZShort)21, order.WD_PalletsSent);
			AssertEquals("WD_PackagesSent must be changed.", 3, order.WD_PackagesSent);
			AssertEquals("WD_F3_NKTotalPackType must be changed.", "CaS", order.WD_F3_NKTotalPackType);
		}

		public void TestUpdatePackageDataAndWeightAndVolume_NonExistentRefPacktype()
		{
			AssertNull("Precondition.", Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, "BAD")));

			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 10m);

			Helper.CreatePickNew(order);
			var outerNonPLT = order.PackageJob.Packages.AddNew();
			outerNonPLT.KP_PackageID = "A1";
			outerNonPLT.KP_F3_NKPackType = "BAD";

			var outerPLT = order.PackageJob.Packages.AddNew();
			outerPLT.KP_PackageID = "B1";
			outerPLT.KP_F3_NKPackType = "PLT";

			order.WD_PalletsSent = 100;
			order.WD_PackagesSent = 500;
			order.WD_F3_NKTotalPackType = "XYZ";
			AssertEquals("Precondition", (ZShort)100, order.WD_PalletsSent);
			AssertEquals("Precondition", 500, order.WD_PackagesSent);
			AssertEquals("Precondition", "XYZ", order.WD_F3_NKTotalPackType);

			order.UpdatePackageDataAndWeightAndVolume();
			AssertEquals("WD_PalletsSent must be changed.", (ZShort)1, order.WD_PalletsSent);
			AssertEquals("WD_PackagesSent must be changed.", 1, order.WD_PackagesSent);
			AssertEquals("WD_F3_NKTotalPackType must be changed.", "BAD", order.WD_F3_NKTotalPackType);
		}

		public void TestUpdatePackageDataAndWeightAndVolume_PackageJobNull()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsCartonised = true;
			Factory.Save();

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInFactory2 = factory2.Load<WhsPick>(pick.PK);

			var factory3 = new BusinessObjectFactory { RefreshEnabled = false };
			var pickInFactory3 = factory3.Load<WhsPick>(pick.PK);

			// Finalise in factory 2
			pickInFactory2.FinaliseAllOrders();
			pickInFactory2.FinalisePick();
			AssertIsFinalisedPrecondition(pickInFactory2);
			// Empty package jobs are deleted on saving after finalisation
			factory2.Save();
			AssertNull("Precondition.", ((WhsOrder)pickInFactory2.Orders[0]).PackageJob);

			// Try UpdatePackageData in factory 3
			AssertNoExceptionThrown(() => ((WhsOrder)pickInFactory3.Orders[0]).UpdatePackageDataAndWeightAndVolume());
		}

		public void TestUpdatePackageDataAndWeightAndVolume_NoPackages()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsCartonised = true;
			Factory.Save();

			order.UpdatePackageDataAndWeightAndVolume();
			AssertEquals("With no Packages, all values should be empty.", (ZShort)0, order.WD_PalletsSent);
			AssertEquals("With no Packages, all values should be empty.", 0, order.WD_PackagesSent);
			AssertEquals("With no Packages, all values should be empty.", "", order.WD_F3_NKTotalPackType);

			order.WD_PalletsSent = 1;
			order.WD_PackagesSent = 2;
			order.WD_F3_NKTotalPackType = "CTN";
			order.UpdatePackageDataAndWeightAndVolume();
			AssertEquals("With no Packages, values should not get overidden.", (ZShort)1, order.WD_PalletsSent);
			AssertEquals("With no Packages, values should not get overidden.", 2, order.WD_PackagesSent);
			AssertEquals("With no Packages, values should not get overidden.", "CTN", order.WD_F3_NKTotalPackType);
		}

		#endregion

		#region CanFinaliseDPS

		// More thorough tests at Enterprise.Warehouse.Transactions.GUI.Testing.DPSSecurityProviderTest
		public void TestCannotFinaliseWhenDPSMatched()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "R1");
			order.WD_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			order.FinaliseDocket();
			AssertEquals("Order should not be finalised.", false, order.IsFinalised);
		}

		public void TestCannotFinaliseWhenDPSMatched_CannotShowDialog()
		{
			Env.Security.DpsAllowUpdateToMatched.IsAllowed = false;
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty, "ALL"))
			{
				try
				{
					Globals.IsUserInteractive = false;

					var data = new TestDataSimpleEnvironment(Factory, 2, 1);
					var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
					order.WD_ScreeningStatus = ScreeningStatusesList.Codes.Matched;

					Assert("Precondition", !Globals.CanShowDialogs);
					Assert("Precondition", order.IsDPSMovementRestricted());
					order.FinaliseDocket();
					AssertEquals("Order should not be finalised.", false, order.IsFinalised);
					Assert("There should be no error message.", UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
				finally
				{
					Globals.IsUserInteractive = true;
				}
			}
		}

		#endregion

		#region TestFinaliseOrder_UseRequiredDateAsFinaliseDate

		public void TestFinaliseOrder_UseRequiredDateForOutwardsFinalisedDate_RequiredDateOptionOff()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = false;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertEquals("Receive must be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			order.WD_RequiredDate = ZDateTimeOffset.Today.AddDays(10);

			var pick = Helper.CreatePickNew(order);
			pick.FinaliseOrder(order);
			AssertNoErrors("Error should NOT exist", order.WD_DocketStatusDescriptionInfo);
			AssertEquals("Order must be finalised.", true, order.IsFinalised);
		}

		const string ExpectedFutureFinalisedDateError = @"Finalized Date is currently an incorrect future date.
Finalized Date is being set by Required Date since the Warehouse option 'Use Required Date For Outwards Finalized Date' is enabled for the current Warehouse.
For valid future finalization dates less than 30 days in the future the registry setting 'Allow Finalization Date Up To 30 Days In The Future' can be enabled for this warehouse's branch.";

		public void TestFinaliseOrder_UseRequiredDateForOutwardsFinalisedDate_RegistryOnDifferentBranchToWhs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			AssertEquals("Receive must be finalised.", true, receive.IsFinalised);
			var branch = Helper.CreateGlbBranch("RFT");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);

			using (WarehouseDataRegistry.Instance.AllowWarehouseOrderFinalisationDateUpTo30DaysInTheFuture
					   .SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, true))
			{
				AssertNotEquals("Precondition: Whs branch different from current and registry branch", branch.PK,
					data.Whs1.WW_GB_RelatedCompanyBranch);
				order.WD_RequiredDate = ZDateTimeOffset.Today.AddDays(10);

				var pick = Helper.CreatePickNew(order);
				pick.FinaliseOrder(order);
				AssertHasError("Error should exist", order.WD_DocketStatusDescriptionInfo,
					ExpectedFutureFinalisedDateError);
				AssertEquals("Order must NOT be finalised.", false, order.IsFinalised);
			}
		}

		public void TestFinaliseOrder_UseRequiredDateForOutwardsFinalisedDate_RegistryOn_ThirtyOneDaysInFuture()
		{
			TestFinaliseOrder_UseRequiredDateForOutwardsFinalisedDate_RegistryOn_DateGreaterThan30DaysInFutureCore(31);
		}

		public void TestFinaliseOrder_UseRequiredDateForOutwardsFinalisedDate_RegistryOn_FortyDaysInFuture()
		{
			TestFinaliseOrder_UseRequiredDateForOutwardsFinalisedDate_RegistryOn_DateGreaterThan30DaysInFutureCore(41);
		}

		void TestFinaliseOrder_UseRequiredDateForOutwardsFinalisedDate_RegistryOn_DateGreaterThan30DaysInFutureCore(
			int daysInFuture)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			using (WarehouseDataRegistry.Instance.AllowWarehouseOrderFinalisationDateUpTo30DaysInTheFuture
					   .SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, true))
			{
				data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				AssertEquals("Receive must be finalised.", true, receive.IsFinalised);
				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
				order.WD_RequiredDate = ZDateTimeOffset.Today.AddDays(daysInFuture);

				var pick = Helper.CreatePickNew(order);
				pick.FinaliseOrder(order);
				AssertHasError("Error should exist", order.WD_DocketStatusDescriptionInfo,
					ExpectedFutureFinalisedDateError);
				AssertEquals("Order must NOT be finalised.", false, order.IsFinalised);
			}
		}

		public void TestFinaliseOrder_UseRequiredDateForOutwardsFinalisedDate_RegistryOn_DateToday()
		{
			TestFinaliseOrder_UseRequiredDateForOutwardsFinalisedDate_RegistryOn_DateLessThan30DaysInFutureCore(0);
		}

		public void TestFinaliseOrder_UseRequiredDateForOutwardsFinalisedDate_RegistryOn_FiveDaysInFuture()
		{
			TestFinaliseOrder_UseRequiredDateForOutwardsFinalisedDate_RegistryOn_DateLessThan30DaysInFutureCore(5);
		}

		public void TestFinaliseOrder_UseRequiredDateForOutwardsFinalisedDate_RegistryOn_TwentyNineDaysInFuture()
		{
			TestFinaliseOrder_UseRequiredDateForOutwardsFinalisedDate_RegistryOn_DateLessThan30DaysInFutureCore(29);
		}

		public void TestFinaliseOrder_UseRequiredDateForOutwardsFinalisedDate_RegistryOn_ThirtyDaysInFuture()
		{
			TestFinaliseOrder_UseRequiredDateForOutwardsFinalisedDate_RegistryOn_DateLessThan30DaysInFutureCore(30);
		}

		void TestFinaliseOrder_UseRequiredDateForOutwardsFinalisedDate_RegistryOn_DateLessThan30DaysInFutureCore(
			int daysInFuture)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			using (WarehouseDataRegistry.Instance.AllowWarehouseOrderFinalisationDateUpTo30DaysInTheFuture
					   .SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, true))
			{
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				AssertEquals("Receive must be finalised.", true, receive.IsFinalised);
				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
				order.WD_RequiredDate = ZDateTimeOffset.Today.AddDays(daysInFuture);

				var pick = Helper.CreatePickNew(order);
				pick.FinaliseOrder(order);
				AssertNoErrors("Error should NOT exist", order.WD_DocketStatusDescriptionInfo);
				AssertEquals("Order must be finalised.", true, order.IsFinalised);
			}
		}

		#endregion

		#region TestFinaliseOrderShouldDeleteEmptyPackages_EmptyPackageHasWhsPickByLabelJob

		public void TestFinaliseOrderShouldDeleteEmptyPackages_EmptyPackageHasWhsPickByLabelJob()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsCartonised = true;

			var package = order.PackageJob.Packages.AddNew("PLT");
			WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK,
				order.Pick.DockDoorLocation.PK, "BP", package.PK);

			AssertEquals("Precondition", true, pick.WP_IsCartonised);
			AssertEquals("Precondition", false, order.IsFinalised);

			AssertEquals("Precondition", 1, PackageCount(order.PackageJob.Packages));

			order.FinaliseDocketAlwaysFinalisingPick();
			AssertEquals("should finalize without error", true, order.IsFinalised);
			AssertEquals("no change", true, pick.WP_IsCartonised);

			AssertEquals("Should delete empty package even if it has a WhsPickByLabelJob", 0,
				PackageCount(order.PackageJob.Packages));
		}

		#endregion

		#region TestCannotUnpackPickByLabelJob

		public void TestCannotUnpackPickByLabelJob_WithPackedItems()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew();
			package.Pack(order.Lines[0].ReleaseLines[0], 5m);
			package.KP_F3_NKPackType = "UNT";
			Factory.Save();

			AssertNotNull("Precondition: packageJob.ParentJob is NOT null.", packageJob.ParentJob);
			AssertEquals("Precondition: No PickByLabelJob exists.", 0,
				Factory.Load<WhsPickByLabelJob>(new ZQuery()).Length);
			AssertEquals("Precondition: Non-PickByLabelJob packages are able to be unpacked.", true,
				package.IsAvailableForUnpacking(out ZString errorMessage));
			AssertEquals(true, errorMessage.IsEmpty);

			var pickByLabelJob = WhsPickByLabelHelper.AddPackageToListOfPickByLabelForUser(Factory, data.Whs1.PK,
				order.Pick.DockDoorLocation.PK, "BP", package.PK);
			AssertEquals("Package has a PickByLabelJob.", true, pickByLabelJob.Labels[0].Package.PK.Equals(package.PK));
			AssertEquals("PickByLabelJob packages are NOT able to be unpacked.", false,
				package.IsAvailableForUnpacking(out errorMessage));
			AssertEquals("Expect cannot edit PickByLabelJob error message",
				"Cannot modify the selected UNT because it has Pick By Label Job and packed items that are not all picked.",
				errorMessage);

			AssertEquals("PickByLabelJob packages are NOT able to be packed.", false,
				package.IsAvailableForPacking(out errorMessage));
			AssertEquals("Expect cannot edit PickByLabelJob error message",
				"Cannot modify the selected UNT because it has Pick By Label Job and packed items that are not all picked.",
				errorMessage);
		}

		#endregion

		#region CalculateOrderClassification

		public void TestCalculateOrderClassification_CubicSent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "2", data.Part1,
				MinBulkProductQtyBoundary + 1m); // +BulkScore
			order.WD_TotalWeightUnit = "KG";
			order.WD_TotalCubicUnit = "M3";
			order.WD_WeightSent = MinBulkWeightBoundary + 1m; // +BulkScore

			var consignee = Helper.CreateClient("CONSIGNEE"); // +ECommerceScore
			order.ConsigneeAddressPK = consignee.MainAddress.PK;

			Assert(order.Lines.Select(line => line.WE_OP).Distinct().Count() <
				   MaxECommerceProductCountBoundary); // +ECommerceScore
			Factory.Save();

			order.WD_WP = Factory.New<WhsPick>().PK;

			order.WD_CubicSent = MinBulkVolumeBoundary + 0.1m;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Bulk, order.WD_OrderClassification);

			order.WD_CubicSent = MaxECommerceVolumeBoundary - 0.1m;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.ECommerce, order.WD_OrderClassification);

			order.WD_CubicSent = MaxECommerceVolumeBoundary;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);

			order.WD_CubicSent = MaxECommerceVolumeBoundary + 0.1m;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);

			order.WD_CubicSent = MinBulkVolumeBoundary;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);

			order.WD_CubicSent = MinBulkVolumeBoundary - 0.1m;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);

			order.WD_CubicSent = MinECommerceMeasurementBoundary;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);
		}

		public void TestCalculateOrderClassification_CubicSent_UnitConversion()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "2", data.Part1,
				MinBulkProductQtyBoundary + 1m); // +BulkScore
			order.WD_TotalWeightUnit = "KG";
			order.WD_TotalCubicUnit = "L";
			order.WD_WeightSent = MinBulkWeightBoundary + 1m; // +BulkScore

			var consignee = Helper.CreateClient("CONSIGNEE"); // +ECommerceScore
			order.ConsigneeAddressPK = consignee.MainAddress.PK;

			Assert(order.Lines.Select(line => line.WE_OP).Distinct().Count() <
				   MaxECommerceProductCountBoundary); // +ECommerceScore
			Factory.Save();

			order.WD_WP = Factory.New<WhsPick>().PK;

			order.WD_CubicSent = 1100m;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Bulk, order.WD_OrderClassification);

			order.WD_CubicSent = 400m;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.ECommerce, order.WD_OrderClassification);

			order.WD_CubicSent = 700m;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);
		}

		public void TestCalculateOrderClassification_ProductWithZeroVolume()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product = Helper.CreateProduct("P3", data.Org1);
			product.OP_Cubic = 0m;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", product, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "2", product,
				MinBulkProductQtyBoundary + 1m); // +BulkScore
			order.WD_TotalWeightUnit = "KG";
			order.WD_TotalCubicUnit = "M3";
			order.WD_WeightSent = MinBulkWeightBoundary + 1m; // +BulkScore
			order.WD_CubicSent = MaxECommerceVolumeBoundary - 0.1m; // +ECommerceScore

			var consignee = Helper.CreateClient("CONSIGNEE"); // +ECommerceScore
			order.ConsigneeAddressPK = consignee.MainAddress.PK;

			Assert(order.Lines.Select(line => line.WE_OP).Distinct().Count() <
				   MaxECommerceProductCountBoundary); // +ECommerceScore
			Factory.Save();

			order.WD_WP = Factory.New<WhsPick>().PK;

			order.CalculateAndSetOrderClassification();
			AssertEquals("Zero product volume overrides order volume in order classification calculation.",
				OrderClassification.Codes.Bulk, order.WD_OrderClassification);
		}

		public void TestCalculateOrderClassification_WeightSent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "2", data.Part1,
				MinBulkProductQtyBoundary + 1m); // +BulkScore
			order.WD_TotalWeightUnit = "KG";
			order.WD_TotalCubicUnit = "M3";
			order.WD_CubicSent = MinBulkWeightBoundary + 1m; // +BulkScore

			var consignee = Helper.CreateClient("CONSIGNEE"); // +ECommerceScore
			order.ConsigneeAddressPK = consignee.MainAddress.PK;

			Assert(order.Lines.Select(line => line.WE_OP).Distinct().Count() <
				   MaxECommerceProductCountBoundary); // +ECommerceScore
			Factory.Save();

			order.WD_WP = Factory.New<WhsPick>().PK;

			order.WD_WeightSent = MinBulkWeightBoundary + 1m;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Bulk, order.WD_OrderClassification);

			order.WD_WeightSent = MaxECommerceWeightBoundary - 1m;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.ECommerce, order.WD_OrderClassification);

			order.WD_WeightSent = MaxECommerceWeightBoundary + 1m;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);

			order.WD_WeightSent = MaxECommerceWeightBoundary;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);

			order.WD_WeightSent = MinBulkWeightBoundary - 1m;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);

			order.WD_WeightSent = MinBulkWeightBoundary;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);

			order.WD_WeightSent = MinECommerceMeasurementBoundary;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);
		}

		public void TestCalculateOrderClassification_WeightSent_UnitConversion()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "2", data.Part1,
				MinBulkProductQtyBoundary + 1m); // +BulkScore
			order.WD_TotalWeightUnit = "G";
			order.WD_TotalCubicUnit = "M3";
			order.WD_CubicSent = MinBulkVolumeBoundary + 1m; // +BulkScore

			var consignee = Helper.CreateClient("CONSIGNEE"); // +ECommerceScore
			order.ConsigneeAddressPK = consignee.MainAddress.PK;

			Assert(order.Lines.Select(line => line.WE_OP).Distinct().Count() <
				   MaxECommerceProductCountBoundary); // +ECommerceScore
			Factory.Save();

			order.WD_WP = Factory.New<WhsPick>().PK;

			order.WD_WeightSent = 21000m;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Bulk, order.WD_OrderClassification);

			order.WD_WeightSent = 1000m;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.ECommerce, order.WD_OrderClassification);

			order.WD_WeightSent = 11000m;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);
		}

		public void TestCalculateOrderClassification_ProductWithZeroWeight()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var product = Helper.CreateProduct("P3", data.Org1);
			product.OP_Weight = 0m;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", product, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "2", product,
				MinBulkProductQtyBoundary + 1m); // +BulkScore
			order.WD_TotalWeightUnit = "KG";
			order.WD_TotalCubicUnit = "M3";
			order.WD_WeightSent = MaxECommerceWeightBoundary - 1m; // +ECommerceScore
			order.WD_CubicSent = MinBulkVolumeBoundary + 1m; // +BulkScore

			var consignee = Helper.CreateClient("CONSIGNEE"); // +ECommerceScore
			order.ConsigneeAddressPK = consignee.MainAddress.PK;

			Assert(order.Lines.Select(line => line.WE_OP).Distinct().Count() <
				   MaxECommerceProductCountBoundary); // +ECommerceScore
			Factory.Save();

			order.WD_WP = Factory.New<WhsPick>().PK;

			order.CalculateAndSetOrderClassification();
			AssertEquals("Zero product weight overrides order weight in order classification calculation.",
				OrderClassification.Codes.Bulk, order.WD_OrderClassification);
		}

		public void TestCalculateOrderClassification_MaxQuantityByProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "2");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, MinBulkProductQtyBoundary + 1m); // +BulkScore
			order.WD_TotalWeightUnit = "KG";
			order.WD_TotalCubicUnit = "M3";
			order.WD_CubicSent = MinBulkVolumeBoundary + 1m; // +BulkScore
			order.WD_WeightSent = MinBulkWeightBoundary + 1m; // +BulkScore

			var consignee = Helper.CreateClient("CONSIGNEE"); // +ECommerceScore
			order.ConsigneeAddressPK = consignee.MainAddress.PK;

			Assert(order.Lines.Select(line => line.WE_OP).Distinct().Count() <
				   MaxECommerceProductCountBoundary); // +ECommerceScore
			Factory.Save();

			order.WD_WP = Factory.New<WhsPick>().PK;

			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Bulk, order.WD_OrderClassification);

			orderLine.WE_TransactionQuantity = MaxECommerceProductQtyBoundary - 1;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.ECommerce, order.WD_OrderClassification);

			orderLine.WE_TransactionQuantity = MaxECommerceProductQtyBoundary;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);

			orderLine.WE_TransactionQuantity = MinBulkProductQtyBoundary;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);
		}

		public void TestCalculateOrderClassification_MaxQuantityByProduct_MultipleLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "2");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, MinBulkProductQtyBoundary);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			order.WD_TotalWeightUnit = "KG";
			order.WD_TotalCubicUnit = "M3";
			order.WD_CubicSent = MinBulkVolumeBoundary + 1m; // +BulkScore
			order.WD_WeightSent = MinBulkWeightBoundary + 1m; // +BulkScore

			var consignee = Helper.CreateClient("CONSIGNEE"); // +ECommerceScore
			order.ConsigneeAddressPK = consignee.MainAddress.PK;

			Assert(order.Lines.Select(line => line.WE_OP).Distinct().Count() <
				   MaxECommerceProductCountBoundary); // +ECommerceScore
			Factory.Save();

			order.WD_WP = Factory.New<WhsPick>().PK;

			Assert(GetMaxQtyByProduct(order) > MinBulkProductQtyBoundary);
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Bulk, order.WD_OrderClassification);

			orderLine1.WE_TransactionQuantity = 1m;
			Assert(GetMaxQtyByProduct(order) < MaxECommerceProductQtyBoundary);
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.ECommerce, order.WD_OrderClassification);

			orderLine1.WE_TransactionQuantity = 2m;
			Assert(GetMaxQtyByProduct(order) == MaxECommerceProductQtyBoundary);
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);

			orderLine1.WE_TransactionQuantity = 3m;
			Assert(GetMaxQtyByProduct(order) == MinBulkProductQtyBoundary);
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);
		}

		public void TestCalculateOrderClassification_MaxQuantityByProduct_MultipleLines_MultipleProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = Helper.CreateProduct("P3", data.Org1);
			part3.OP_Cubic = 1m;
			part3.OP_Weight = 1m;
			var part4 = Helper.CreateProduct("P4", data.Org1);
			part4.OP_Cubic = 1m;
			part4.OP_Weight = 1m;
			var part5 = Helper.CreateProduct("P5", data.Org1);
			part5.OP_Cubic = 1m;
			part5.OP_Weight = 1m;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "2", data.Part2, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "3", part3, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "4", part4, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "5", part5, 10m);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "6");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreateWhsOrderLine(order, data.Part2, 1m);
			Helper.CreateWhsOrderLine(order, data.Part2, 1m);
			Helper.CreateWhsOrderLine(order, part3, 1m);
			Helper.CreateWhsOrderLine(order, part3, 1m);
			Helper.CreateWhsOrderLine(order, part4, 1m);
			Helper.CreateWhsOrderLine(order, part4, 1m);
			Helper.CreateWhsOrderLine(order, part5, 1m);
			Helper.CreateWhsOrderLine(order, part5, 1m);
			order.WD_TotalWeightUnit = "KG";
			order.WD_TotalCubicUnit = "M3";
			order.WD_CubicSent = MaxECommerceVolumeBoundary - 0.1m; // +ECommerceScore
			order.WD_WeightSent = MinBulkWeightBoundary + 1m; // +BulkScore

			var consignee = Helper.CreateClient("CONSIGNEE"); // +ECommerceScore
			order.ConsigneeAddressPK = consignee.MainAddress.PK;

			Assert(order.Lines.Select(line => line.WE_OP).Distinct().Count() >
				   MinBulkProductCountBoundary); // +BulkScore
			Factory.Save();

			order.WD_WP = Factory.New<WhsPick>().PK;

			Assert(GetMaxQtyByProduct(order) < MaxECommerceProductQtyBoundary);
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.ECommerce, order.WD_OrderClassification);

			orderLine1.WE_TransactionQuantity = 4m;
			Assert(GetMaxQtyByProduct(order) > MinBulkProductCountBoundary);
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Bulk, order.WD_OrderClassification);

			orderLine1.WE_TransactionQuantity = 2m;
			Assert(GetMaxQtyByProduct(order) == MaxECommerceProductQtyBoundary);
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);

			orderLine1.WE_TransactionQuantity = 3m;
			Assert(GetMaxQtyByProduct(order) == MinBulkProductCountBoundary);
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);
		}

		public void TestCalculateOrderClassification_ProductCount()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var prod3 = Helper.CreateProduct("P3", data.Org1);
			var prod4 = Helper.CreateProduct("P4", data.Org1);
			var prod5 = Helper.CreateProduct("P5", data.Org1);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "2");
			Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreateWhsOrderLine(order, data.Part2, 1m);
			order.WD_TotalWeightUnit = "KG";
			order.WD_TotalCubicUnit = "M3";
			order.WD_CubicSent = MinBulkVolumeBoundary + 1m; // +BulkScore
			order.WD_WeightSent = MinBulkWeightBoundary + 1m; // +BulkScore

			var consignee = Helper.CreateClient("CONSIGNEE"); // +ECommerceScore
			order.ConsigneeAddressPK = consignee.MainAddress.PK;

			Assert(order.Lines.Select(line => line.WE_OP).Distinct().Count() <
				   MaxECommerceProductCountBoundary); // +ECommerceScore
			Assert(GetMaxQtyByProduct(order) < MaxECommerceProductQtyBoundary); // +ECommerceScore
			Factory.Save();

			order.WD_WP = Factory.New<WhsPick>().PK;

			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.ECommerce, order.WD_OrderClassification);

			Helper.CreateWhsOrderLine(order, prod3, 1m);
			Assert(order.Lines.Select(line => line.WE_OP).Distinct().Count() == MaxECommerceProductCountBoundary);
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);

			Helper.CreateWhsOrderLine(order, prod4, 1m);
			Assert(order.Lines.Select(line => line.WE_OP).Distinct().Count() == MinBulkProductCountBoundary);
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);

			Helper.CreateWhsOrderLine(order, prod5, 1m);
			Assert(order.Lines.Select(line => line.WE_OP).Distinct().Count() > MinBulkProductCountBoundary);
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Bulk, order.WD_OrderClassification);
		}

		decimal GetMaxQtyByProduct(WhsOrder order)
		{
			return order
				.Lines
				.GroupBy(line => line.WE_OP,
					(key, group) => group.Sum(orderLineByProd => orderLineByProd.WE_TransactionQuantity))
				.Max();
		}

		public void TestCalculateOrderClassification_Consignee()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			Helper.SetProductWeightAndVolume(data.Part1, 0m, "", 0m, ""); // +2 BulkScore
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "2", data.Part1,
				MaxECommerceProductQtyBoundary - 1); // +ECommerceScore
			order.WD_TotalWeightUnit = "KG";
			order.WD_TotalCubicUnit = "M3";
			order.WD_CubicSent = MinBulkVolumeBoundary + 1m; // +BulkScore
			order.WD_WeightSent = MinBulkWeightBoundary + 1m; // +BulkScore

			var consignee = Helper.CreateClient("CONSIGNEE"); // +ECommerceScore
			order.ConsigneeAddressPK = consignee.MainAddress.PK;

			Assert(order.Lines.Select(line => line.WE_OP).Distinct().Count() <
				   MaxECommerceProductCountBoundary); // +ECommerceScore
			Factory.Save();
			Helper.CreatePickNew(order);

			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.ECommerce, order.WD_OrderClassification);

			var otherOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "3", data.Part1, 1m);
			otherOrder.ConsigneeAddressPK = consignee.MainAddress.PK; // +BulkScore
			Factory.Save();

			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Bulk, order.WD_OrderClassification);
		}

		public void TestCalculateOrderClassification_OveriddenConsigneeAddress()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "2", data.Part1,
				MaxECommerceProductQtyBoundary - 1); // +ECommerceScore
			order.WD_TotalWeightUnit = "KG";
			order.WD_TotalCubicUnit = "M3";
			order.WD_CubicSent = MinBulkVolumeBoundary + 1m; // +BulkScore
			order.WD_WeightSent = MinBulkWeightBoundary + 1m; // +BulkScore

			order.ConsigneeDocAddress.E2_AddressOverride = true; // +ECommerceScore
			order.ConsigneeDocAddress.E2_CompanyName = "Random Consignee";
			order.ConsigneeDocAddress.E2_Address1 = "Test";
			AssertNull(order.Consignee);

			Assert(order.Lines.Select(line => line.WE_OP).Distinct().Count() <
				   MaxECommerceProductCountBoundary); // +ECommerceScore
			Factory.Save();

			order.WD_WP = Factory.New<WhsPick>().PK;

			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.ECommerce, order.WD_OrderClassification);
		}

		public void TestCalculateOrderClassification_MultipleSameOveriddenConsigneeAddress()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "2", data.Part1,
				MaxECommerceProductQtyBoundary - 1); // +ECommerceScore
			order.WD_TotalWeightUnit = "KG";
			order.WD_TotalCubicUnit = "M3";
			order.WD_CubicSent = MinBulkVolumeBoundary + 1m; // +BulkScore
			order.WD_WeightSent = MinBulkWeightBoundary + 1m; // +BulkScore

			Assert(order.Lines.Select(line => line.WE_OP).Distinct().Count() <
				   MaxECommerceProductCountBoundary); // +ECommerceScore

			order.ConsigneeDocAddress.E2_AddressOverride = true; // +ECommerceScore
			order.ConsigneeDocAddress.E2_CompanyName = "Random Consignee";
			order.ConsigneeDocAddress.E2_Address1 = "Test";
			AssertNull(order.Consignee);

			var otherOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "3", data.Part1, 1m);
			otherOrder.ConsigneeDocAddress.E2_AddressOverride = true;
			otherOrder.ConsigneeDocAddress.E2_CompanyName = "Random Consignee";
			otherOrder.ConsigneeDocAddress.E2_Address1 = "Test";
			AssertNull(otherOrder.Consignee);
			Factory.Save();

			order.WD_WP = Factory.New<WhsPick>().PK;

			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.ECommerce, order.WD_OrderClassification);
		}

		public void TestCalculateOrderClassification_ConsigneeAddressUsedAsAnotherAddress()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "2", data.Part1,
				MaxECommerceProductQtyBoundary - 1); // +ECommerceScore
			order.WD_TotalWeightUnit = "KG";
			order.WD_TotalCubicUnit = "M3";
			order.WD_CubicSent = MinBulkVolumeBoundary + 1m; // +BulkScore
			order.WD_WeightSent = MinBulkWeightBoundary + 1m; // +BulkScore

			var consignee = Helper.CreateClient("CONSIGNEE"); // +ECommerceScore
			order.ConsigneeAddressPK = consignee.MainAddress.PK;
			order.SupplierDocAddress.OrganisationPK = consignee.PK;

			Assert(order.Lines.Select(line => line.WE_OP).Distinct().Count() <
				   MaxECommerceProductCountBoundary); // +ECommerceScore
			Factory.Save();

			order.WD_WP = Factory.New<WhsPick>().PK;

			var addressQuery = new ZQuery(JobDocAddressSchema.E2_OA_Address, consignee.MainAddress.PK);
			addressQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, "WD");
			Assert(Factory.Load<JobDocAddress>(addressQuery).Length > 1);

			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.ECommerce, order.WD_OrderClassification);
		}

		public void TestCalculateOrderClassification_SameConsigneeDifferentAddress()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "2", data.Part1,
				MaxECommerceProductQtyBoundary - 1); // +ECommerceScore
			order.WD_TotalWeightUnit = "KG";
			order.WD_TotalCubicUnit = "M3";
			order.WD_CubicSent = MinBulkVolumeBoundary + 1m; // +BulkScore
			order.WD_WeightSent = MinBulkWeightBoundary + 1m; // +BulkScore

			var consignee = Helper.CreateClient("CONSIGNEE");
			var newAddress = consignee.Addresses.AddNew();
			newAddress.FillWithValidTestData();
			order.ConsigneeAddressPK = consignee.MainAddress.PK;

			var otherOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "3", data.Part1, 1m);
			otherOrder.ConsigneeAddressPK = newAddress.PK;

			Assert(order.Lines.Select(line => line.WE_OP).Distinct().Count() <
				   MaxECommerceProductCountBoundary); // +ECommerceScore
			Factory.Save();

			order.WD_WP = Factory.New<WhsPick>().PK;

			AssertEquals(order.Consignee.PK, otherOrder.Consignee.PK);
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.ECommerce, order.WD_OrderClassification);
		}

		#region TestCalculateOrderClassification_RTUS_AllECO

		public void TestCalculateOrderClassification_RTUS_AllECO()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var package1 = PackingHelper.CreatePackage(order, "BOX2", 1, Core.Constants.PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(order, "BOX2", 1, Core.Constants.PkgUnit.Box);
			package1.RTUSBookedType = "ECO";
			package2.RTUSBookedType = "ECO";
			Factory.Save();

			order.WD_WP = Factory.New<WhsPick>().PK;

			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.ECommerce, order.WD_OrderClassification);
		}

		public void TestCalculateOrderClassification_RTUS_HasBLK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var package1 = PackingHelper.CreatePackage(order, "BOX2", 1, Core.Constants.PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(order, "BOX2", 1, Core.Constants.PkgUnit.Box);
			package1.RTUSBookedType = "BLK";
			package2.RTUSBookedType = "ECO";
			Factory.Save();

			order.WD_WP = Factory.New<WhsPick>().PK;

			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Bulk, order.WD_OrderClassification);
		}

		public void TestCalculateOrderClassification_RTUS_HasUNK()
		{
			var order = CreateOrderClassificationRTUSFallbackData();
			order.WD_WP = Factory.New<WhsPick>().PK;

			var package1 = PackingHelper.CreatePackage(order, "BOX2", 1, Constants.PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(order, "BOX2", 1, Constants.PkgUnit.Box);
			var package3 = PackingHelper.CreatePackage(order, "BOX2", 1, Constants.PkgUnit.Box);
			package1.RTUSBookedType = "BLK";
			package2.RTUSBookedType = "UNK";
			package3.RTUSBookedType = "ECO";
			order.WD_WeightSent = MinBulkWeightBoundary + 1m; // +BulkScore

			order.WD_CubicSent = MinBulkVolumeBoundary + 0.1m;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Bulk, order.WD_OrderClassification);

			order.WD_CubicSent = MaxECommerceVolumeBoundary - 0.1m;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.ECommerce, order.WD_OrderClassification);

			order.WD_CubicSent = MaxECommerceVolumeBoundary;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);
		}

		public void TestCalculateOrderClassification_RTUS_HasEmpty()
		{
			var order = CreateOrderClassificationRTUSFallbackData();
			order.WD_WP = Factory.New<WhsPick>().PK;

			var package1 = PackingHelper.CreatePackage(order, "BOX2", 1, Constants.PkgUnit.Box);
			var package2 = PackingHelper.CreatePackage(order, "BOX2", 1, Constants.PkgUnit.Box);
			var package3 = PackingHelper.CreatePackage(order, "BOX2", 1, Constants.PkgUnit.Box);
			package1.RTUSBookedType = "BLK";
			package2.RTUSBookedType = "";
			package3.RTUSBookedType = "ECO";
			order.WD_WeightSent = MinBulkWeightBoundary + 1m; // +BulkScore

			order.WD_CubicSent = MinBulkVolumeBoundary + 0.1m;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Bulk, order.WD_OrderClassification);

			order.WD_CubicSent = MaxECommerceVolumeBoundary - 0.1m;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.ECommerce, order.WD_OrderClassification);

			order.WD_CubicSent = MaxECommerceVolumeBoundary;
			order.CalculateAndSetOrderClassification();
			AssertEquals(OrderClassification.Codes.Unknown, order.WD_OrderClassification);
		}

		WhsOrder CreateOrderClassificationRTUSFallbackData()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "2", data.Part1,
				MinBulkProductQtyBoundary + 1m); // +BulkScore
			order.WD_TotalWeightUnit = "KG";
			order.WD_TotalCubicUnit = "M3";

			var consignee = Helper.CreateClient("CONSIGNEE"); // +ECommerceScore
			order.ConsigneeAddressPK = consignee.MainAddress.PK;

			Assert(order.Lines.Select(line => line.WE_OP).Distinct().Count() <
				   MaxECommerceProductCountBoundary); // +ECommerceScore
			Factory.Save();

			return order;
		}

		#endregion

		ZDecimal MinBulkWeightBoundary => 20m;
		ZDecimal MinBulkVolumeBoundary => 1m;
		ZDecimal MinBulkProductQtyBoundary => 4m;
		ZDecimal MinBulkProductCountBoundary => 4m;
		ZDecimal MaxECommerceWeightBoundary => 10m;
		ZDecimal MaxECommerceVolumeBoundary => 0.5m;
		ZDecimal MinECommerceMeasurementBoundary => 0m;
		ZDecimal MaxECommerceProductQtyBoundary => 3m;
		ZDecimal MaxECommerceProductCountBoundary => 3m;

		#endregion

		#region TestOnFactorySaving_CreateOperationalStatusChangeEvents

		protected override void SetFinsalisedDocketValuesForTest(WhsOrder docket)
		{
			docket.WD_FinalisedDate = ZDateTimeOffset.Now;
		}

		#endregion

		// interfaces

		#region IAutoCreateAddressOnUnmatch Members

		public void TestCreateOrgAddressOnUnmatch()
		{
			IAutoCreateAddressOnUnmatch order = Factory.New<WhsOrder>();

			using (WarehouseDataRegistry.Instance.CreateNewAddressOnUnmatchedAddressForOrders.SetTemporaryValue(
					   Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(false, order.CreateOrgAddressOnUnmatch);
			}

			using (WarehouseDataRegistry.Instance.CreateNewAddressOnUnmatchedAddressForOrders.SetTemporaryValue(
					   Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(true, order.CreateOrgAddressOnUnmatch);
			}
		}

		#endregion

		#region IAttachedOrder Members

		public void TestIAttachedOrder()
		{
			var year = ZDateTime.Now.Year;

			var order = Factory.New<WhsOrder>();
			IAttachedOrder attachedOrder = order;

			order.WD_GoodsDescription = "Cakes";
			AssertEquals("Cakes", attachedOrder.GoodsDescription);

			order.WD_RequiredDate = new ZDateTimeOffset(year, 1, 1);
			AssertEquals(new ZDateTime(year, 1, 1).Date, attachedOrder.JobDate.Date);

			order.WD_ExternalReference = "12345";
			AssertEquals("12345", attachedOrder.JobNo);
			AssertEquals("NEW", attachedOrder.JobStatus);

			AssertEquals(WhsOrder.AttachedOrderType, attachedOrder.JobType);
			AssertEquals("Warehouse Order", attachedOrder.JobDescription);
			AssertEquals(ModuleIDs.WhsOrder, attachedOrder.ModuleID);
			AssertEquals(ControllerIDs.WhsOrder, attachedOrder.ControllerID);
		}

		#endregion

		#region IBuyerSupplierRelationshipConsumer Members

		public void TestIBuyerSupplierRelationshipConsumer()
		{
			var order = Factory.New<WhsOrder>();
			var iBuyerSupplierRelationshipConsumer = (IBuyerSupplierRelationshipConsumer)order;
			var consigneeChanged = false;
			var modeChangedCount = 0;
			iBuyerSupplierRelationshipConsumer.ModesChanged += delegate
			{
				modeChangedCount++;
			};
			iBuyerSupplierRelationshipConsumer.ConsigneeChanged += delegate
			{
				consigneeChanged = true;
			};

			var consignee = Factory.NewWithValidTestData<OrgAddress>();
			order.ConsigneeAddressPK = consignee.PK;
			AssertEquals(consignee.Header, iBuyerSupplierRelationshipConsumer.Consignee);
			AssertEquals(true, consigneeChanged);

			var newConsignee = Helper.CreateClient("Consignee");
			iBuyerSupplierRelationshipConsumer.Consignee = newConsignee;
			AssertEquals(newConsignee, order.Consignee);

			var client = Helper.CreateClient("Client");
			var consignorChanged = false;
			iBuyerSupplierRelationshipConsumer.ConsignorChanged += delegate
			{
				consignorChanged = true;
			};
			order.WD_OH_Client = client.PK;
			AssertEquals(client, iBuyerSupplierRelationshipConsumer.Consignor);
			AssertEquals(true, consignorChanged);

			var newClient = Helper.CreateClient("New Client");
			iBuyerSupplierRelationshipConsumer.Consignor = newClient;
			AssertEquals(newClient, order.Client);

			order.WD_TransportMode = TransportModes.Road;
			AssertEquals(TransportModes.Road, order.WD_TransportMode);
			AssertEquals(1, modeChangedCount);

			iBuyerSupplierRelationshipConsumer.TransportMode = TransportModes.Sea;
			AssertEquals(TransportModes.Sea, iBuyerSupplierRelationshipConsumer.TransportMode);
			AssertEquals(2, modeChangedCount);

			order.WD_ContainerMode = ContainerModes.LCL;
			AssertEquals(ContainerModes.LCL, iBuyerSupplierRelationshipConsumer.ContainerMode);
			AssertEquals(3, modeChangedCount);

			iBuyerSupplierRelationshipConsumer.ContainerMode = ContainerModes.FCL;
			AssertEquals(ContainerModes.FCL, order.WD_ContainerMode);
			AssertEquals(4, modeChangedCount);

			iBuyerSupplierRelationshipConsumer.ContainerMode = ContainerModes.Containerised;
			AssertEquals("CNT is non-applicable container mode", ContainerModes.FCL, iBuyerSupplierRelationshipConsumer.ContainerMode);
			AssertEquals(4, modeChangedCount);

			order.WD_GoodsDescription = "Drums";
			AssertEquals("Drums", iBuyerSupplierRelationshipConsumer.GoodsDescription);

			iBuyerSupplierRelationshipConsumer.GoodsDescription = "Pizza hats";
			AssertEquals("Pizza hats", order.WD_GoodsDescription);

			order.WD_INCO = "PPD";
			AssertEquals("PPD", iBuyerSupplierRelationshipConsumer.PaymentTerms);

			iBuyerSupplierRelationshipConsumer.PaymentTerms = "XXX";
			AssertEquals("PPD", order.WD_INCO);
			AssertEquals("PPD", iBuyerSupplierRelationshipConsumer.PaymentTerms);

			iBuyerSupplierRelationshipConsumer.PaymentTerms = "CLT";
			AssertEquals("CLT", order.WD_INCO);

			order.WD_PL_NKCarrierServiceLevel = "";
			order.Client.MiscServ.OM_RS_NKEXDefaultServiceLevel = "STD";
			iBuyerSupplierRelationshipConsumer.RestoreServiceLevelFallback();
			AssertEquals("STD", order.WD_PL_NKCarrierServiceLevel);

			order.WD_PL_NKCarrierServiceLevel = "SMP";
			AssertEquals("SMP", iBuyerSupplierRelationshipConsumer.ServiceLevel);

			iBuyerSupplierRelationshipConsumer.ServiceLevel = "STD";
			AssertEquals("STD", order.WD_PL_NKCarrierServiceLevel);

			AssertEquals(true, iBuyerSupplierRelationshipConsumer.ShouldRestorePaymentTerm);

			Env.Registry.UseBuyerSupplierRelationships = false;
			AssertEquals(false, iBuyerSupplierRelationshipConsumer.ShouldPromptToSaveBuyerSupplierRelationship);

			Env.Registry.UseBuyerSupplierRelationships = true;
			AssertEquals(true, iBuyerSupplierRelationshipConsumer.ShouldPromptToSaveBuyerSupplierRelationship);

			// Unused interface implementations
			AssertNull(iBuyerSupplierRelationshipConsumer.ConsigneeDeliveryAddress);
			AssertNull(iBuyerSupplierRelationshipConsumer.ConsignorPickupAddress);
			AssertEquals(ZGuid.Empty, iBuyerSupplierRelationshipConsumer.DeliveryCartageCoPK);
			AssertEquals("", iBuyerSupplierRelationshipConsumer.Destination);
			AssertEquals("", iBuyerSupplierRelationshipConsumer.DischargePort);
			AssertEquals("", iBuyerSupplierRelationshipConsumer.GoodsCurrency);
			AssertEquals(ZGuid.Empty, iBuyerSupplierRelationshipConsumer.ImportBrokerPK);
			AssertEquals("", iBuyerSupplierRelationshipConsumer.LoadPort);
			AssertEquals(ZByte.Zero, iBuyerSupplierRelationshipConsumer.NoCopyBills);
			AssertEquals(ZByte.Zero, iBuyerSupplierRelationshipConsumer.NoOriginalBills);
			AssertNull(iBuyerSupplierRelationshipConsumer.NotifyPartyDocumentaryAddress);
			AssertEquals("", iBuyerSupplierRelationshipConsumer.Origin);
			AssertEquals(ZGuid.Empty, iBuyerSupplierRelationshipConsumer.PickupCartageCoPK);
			AssertEquals(ZGuid.Empty, iBuyerSupplierRelationshipConsumer.ReceivingAgentPK);
			AssertEquals("", iBuyerSupplierRelationshipConsumer.ReleaseType);
			AssertEquals(ZGuid.Empty, iBuyerSupplierRelationshipConsumer.SendingAgentPK);
			AssertEquals(ZGuid.Empty, iBuyerSupplierRelationshipConsumer.ShippingLinePK);
		}

		#endregion

		#region IOrdersDocumentSupport Members

		public void TestGetPackageLabelDocumentWrappers_Orders()
		{
			WhsOrder order = Factory.NewWithValidTestData<WhsOrder>();
			IOrdersDocumentSupport documentSupport = order;
			AssertEquals("One document", 1, documentSupport.GetPackageLabelDocumentWrappers().Length);
		}

		public void TestGetDeliveryLabelDocumentWrappers_Orders()
		{
			WhsOrder order = Factory.NewWithValidTestData<WhsOrder>();
			IOrdersDocumentSupport documentSupport = order;
			AssertEquals("One document", 0,
				documentSupport.GetDeliveryLabelDocumentWrappers(order.CallOnWhsOrderToPrint).Length);

			order.WD_PackagesSent = 5;
			AssertEquals("One document", 1,
				documentSupport.GetDeliveryLabelDocumentWrappers(order.CallOnWhsOrderToPrint).Length);
		}

		#endregion

		#region ISendEmailSource Members

		protected override ZInt ExpectedRecipientCount
		{
			get { return base.ExpectedRecipientCount + 4; }
		}

		protected override void CheckForRecipients(OrgHeader client, OrgHeader consignee, OrgHeader goodsBillTo,
			OrgHeader supplier, OrgHeader transportCo, OrgHeader transportBillTo, AddressBookSelection selection)
		{
			base.CheckForRecipients(client, consignee, goodsBillTo, supplier, transportCo, transportBillTo, selection);
			AssertRecipient(consignee, selection);
			AssertRecipient(goodsBillTo, selection);
			AssertRecipient(transportCo, selection);
			AssertRecipient(transportBillTo, selection);
		}

		protected override string ExpectedTemplateCategory
		{
			get { return MailTemplateCategoryList.Codes.WarehouseOrder; }
		}

		#endregion

		#region IJobInvoicingAdditionalData Members

		protected override void TestGetAdditionalProperties_Core(WhsOrder docket, JobCharge charge,
			CustomPropertyContainer<JobCharge> properties)
		{
			var consignee = Helper.CreateClient("CONSIGNEE");
			SetupMainOrgAddress(consignee, "Addr1", "Addr2", "City", "12345", "State", "UAIEV");

			AssertEquals("",
				properties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeCode].GetValue(charge));
			AssertEquals("",
				properties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeAddress1].GetValue(charge));
			AssertEquals("",
				properties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeAddress2].GetValue(charge));
			AssertEquals("",
				properties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeCity].GetValue(charge));
			AssertEquals("",
				properties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneePostCode].GetValue(charge));
			AssertEquals("",
				properties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeState].GetValue(charge));
			AssertEquals("",
				properties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeUNLOCO].GetValue(charge));

			docket.ConsigneeDocAddress.OrganisationPK = consignee.PK;
			AssertEquals("CONSIGIEV",
				properties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeCode].GetValue(charge));
			AssertEquals("Addr1",
				properties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeAddress1].GetValue(charge));
			AssertEquals("Addr2",
				properties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeAddress2].GetValue(charge));
			AssertEquals("City",
				properties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeCity].GetValue(charge));
			AssertEquals("12345",
				properties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneePostCode].GetValue(charge));
			AssertEquals("State",
				properties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeState].GetValue(charge));
			AssertEquals("UAIEV",
				properties[WhsJobInvoicingAdditionalDataPropertyProvider.Schema.ConsigneeUNLOCO].GetValue(charge));
		}

		void SetupMainOrgAddress(OrgHeader org, ZString address1, ZString address2, ZString city, ZString postCode,
			ZString state, ZString relatedPortCode)
		{
			var address = org.MainAddress;
			address.OA_Address1 = address1;
			address.OA_Address2 = address2;
			address.OA_City = city;
			address.OA_PostCode = postCode;
			address.OA_State = state;
			address.OA_RL_NKRelatedPortCode = relatedPortCode;
		}

		#endregion

		#region IDocManagerSupport Members

		public override void TestDocManagerInfo()
		{
			var docket = GetNewBusinessObject();
			var info = docket.DocManagerInfo;
			AssertEquals(docket, info.BusinessEntity);
			AssertEquals("WOD", info.DocManagerCode);
		}

		#endregion

		#region ICustomFieldProvider Members

		protected override string WorkflowDescriptorCode
		{
			get { return WorkflowDescriptors.WhsOrderWorkflowDescriptorCode; }
		}

		#endregion

		#region TestIPackingParent_IsLoosePackageIDsSupported

		public void TestIPackingParent_IsLoosePackageIDsSupported()
		{
			var order = Factory.New<WhsOrder>();
			IPackingParent packingParent = order;
			AssertEquals(false, packingParent.IsLoosePackageIDsSupported);
		}

		#endregion

		#region TestIPackingParent_ShouldPackTrackedPackagesViaDivot

		public void TestIPackingParent_ShouldPackTrackedPackagesViaDivot()
		{
			var order = Factory.New<WhsOrder>();
			IPackingParent packingParent = order;
			AssertEquals(false, packingParent.ShouldPackTrackedPackagesViaDivot);
		}

		#endregion

		#region IPackingParentDefaultPackageType Members

		public void TestIPackingParentDefaultPackageType()
		{
			var order = Factory.New<WhsOrder>();
			IPackingParentDefaultPackageType iPackingParentDefaultPackageType = order;
			AssertEquals("Order without a Client should not have a Default Outer Pack Type.", "",
				iPackingParentDefaultPackageType.DefaultOuterPackType);

			var client = Helper.CreateClient();
			order.WD_OH_Client = client.PK;
			AssertEquals("Client without a Pick Pack Parameter should not have a Default Outer Pack Type.", "",
				iPackingParentDefaultPackageType.DefaultOuterPackType);

			var warehouse = Helper.CreateWarehouse("WHS");
			var pickPackParameter = Helper.CreatePickPackParameter(client, warehouse);
			pickPackParameter.WPP_F3_NKPackType = "KEG";
			AssertEquals("Order without a Warehouse should not have a Default Outer Pack Type.", "",
				iPackingParentDefaultPackageType.DefaultOuterPackType);

			order.WD_WW_Whs = warehouse.PK;
			AssertEquals("Order details match the Pick Pack Parameter so it should have a Default Outer Pack Type.",
				"KEG", iPackingParentDefaultPackageType.DefaultOuterPackType);

			pickPackParameter.WPP_F3_NKPackType = "";
			AssertEquals("Pick Pack Parameter without Pack Type should not have a Default Outer Pack Type.", "",
				iPackingParentDefaultPackageType.DefaultOuterPackType);
		}

		#endregion

		#region IPackingParentWithPackableItems Members

		public void TestIPackingParentWithPackableItems()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory(TestDataForInventory.StockType.WithoutBondEntryKeys);

			// create an order
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var line2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var packingParent = (IPackingParentWithPackableItems)order;
			order.WD_DocketID = "W00123";
			order.WD_ExternalReference = "abc";

			// pick the order
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;
			AssertEquals("Precondition - Pick failed.", true, order.IsAttachedToPickButNotFinalised);

			// test IPackingParent members
			AssertEquals(order.Factory, packingParent.Factory);
			AssertEquals(order.PK, packingParent.PK);
			AssertEquals(ControllerIDs.WhsOrder, packingParent.ControllerID);
			AssertEquals("Warehouse Order", packingParent.JobDescription);
			AssertEquals("abc", packingParent.JobNo);
			AssertEquals(ZString.Empty, packingParent.ConnoteNo);
			AssertEquals(false, packingParent.IsScanEventsVisible);
			AssertEquals(DocumentOptions.None, packingParent.DocumentOptions);

			// test IPackingParent.PackableItems
			AssertContainsExactElementsInAnyOrder(new[] { line1.ReleaseLines[0], line2.ReleaseLines[0] },
				packingParent.PackableItemParents);
			AssertEquals("PackableItems should be cached.", order.PackableItemParents, order.PackableItemParents);

			// test IPackingParent.OnPackageJobReleased
			var pupEventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.PickedUp.Code);
			AssertEquals("Precondition - should not have a PUP event.", 0, order.Logs.Find(pupEventQuery).Length);

			packingParent.OnPackageJobReleased();
			AssertEquals(1, order.Logs.Find(pupEventQuery).Length);

			// test IPackingParent.IsScanQtyAllowed
			order.Consignee.MiscServ.OM_IsScanPackQtyAllowed = false;
			AssertEquals(false, packingParent.IsScanQtyAllowed);
			AssertEquals("Scan Qty Mode is not enabled for the current Consignee.",
				packingParent.IsScanQtyAllowed.ReasonForNotAllowed);

			order.Consignee.MiscServ.OM_IsScanPackQtyAllowed = true;
			AssertEquals(true, packingParent.IsScanQtyAllowed);
			AssertEquals("", packingParent.IsScanQtyAllowed.ReasonForNotAllowed);

			order.ConsigneeDocAddress.E2_AddressOverride = true;
			AssertEquals("Overriden Addresses should act like new orgs and default to true.", true,
				packingParent.IsScanQtyAllowed);
			AssertEquals("", packingParent.IsScanQtyAllowed.ReasonForNotAllowed);

			order.ConsigneePK = data.Org1.PK; // cleanup
			order.ConsigneePK = ZGuid.Empty;
			AssertEquals(true, packingParent.IsScanQtyAllowed);
			order.ConsigneePK = data.Org1.PK; // cleanup

			// test IPackingParent.IsAutoPrintAllowed
			order.Consignee.MiscServ.OM_IsLabelPrintedOnClosePackage = false;
			AssertEquals(false, packingParent.IsAutoPrintAllowed);

			order.Consignee.MiscServ.OM_IsLabelPrintedOnClosePackage = true;
			AssertEquals(true, packingParent.IsAutoPrintAllowed);

			order.ConsigneePK = ZGuid.Empty;
			AssertEquals(false, packingParent.IsAutoPrintAllowed);
			order.ConsigneePK = data.Org1.PK; // cleanup

			// test IsAutoPackAllowed
			order.Consignee.MiscServ.OM_IsAutoPackAllowed = false;
			AssertEquals(false, packingParent.IsAutoPackAllowed);
			AssertEquals("Auto-Pack is not enabled for the current Consignee.",
				packingParent.IsAutoPackAllowed.ReasonForNotAllowed);

			order.Consignee.MiscServ.OM_IsAutoPackAllowed = true;
			AssertEquals(true, packingParent.IsAutoPackAllowed);

			order.ConsigneeDocAddress.E2_AddressOverride = true;
			AssertEquals("Overriden Addresses should act like new orgs and default to true.", true,
				packingParent.IsAutoPackAllowed);

			order.ConsigneePK = data.Org1.PK; // cleanup
			order.ConsigneePK = ZGuid.Empty;
			AssertEquals(true, packingParent.IsAutoPackAllowed);
			order.ConsigneePK = data.Org1.PK; // cleanup

			// simulate the user modifying the pick (via the rls screen) and ensure IPackableItem.PackableItems is updated
			var line3 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			pick.ClearOrderedInventoriesCache();
			pick.AutoAllocateItemsWithMock();
			AssertContainsExactElementsInAnyOrder(
				new[] { line1.ReleaseLines[0], line2.ReleaseLines[0], line3.ReleaseLines[0] },
				packingParent.PackableItemParents);
		}

		public void TestIPackingParentWithPackableItems_PackableItemParentsCountChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);

			int countChangedHitCount = 0;
			IPackingParentWithPackableItems packingParent = order;
			packingParent.PackableItemParentsCountChanged += (sender, e) => countChangedHitCount++;

			releaseLine.Quantity = 0m;
			AssertEquals("Release Lines should have removed invalid Release Line.", 0, order.PackableItemParents.Count);
			AssertEquals("When Release Lines changes, Count Changed should fire.", 1, countChangedHitCount);

			releaseLine.Quantity = 10m;
			AssertContainsExactElementsInAnyOrder(new[] { releaseLine }, order.PackableItemParents.Typed);
			AssertEquals("When Release Lines changes, Count Changed should fire.", 2, countChangedHitCount);

			releaseLine.Delete();
			AssertEquals("Release Lines should have removed deleted Release Line.", 0, order.PackableItemParents.Count);
			AssertEquals("When Release Lines changes, Count Changed should fire.", 3, countChangedHitCount);
		}

		public void TestIPackingParent_IsParentJobFinalised()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory();

			// create an order
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var line1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var line2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var packingParent = (IPackingParent)order;
			order.WD_DocketID = "W00123";
			order.WD_ExternalReference = "abc";

			// pick the order
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - Order should be Picking.", true, order.IsAttachedToPickButNotFinalised);
			AssertEquals("Order is Picked, Packing should be editable.", false, packingParent.IsParentJobFinalised);

			// finalise the order
			order.FinaliseDocket();
			AssertIsFinalisedPrecondition(order);
			AssertEquals("Order is Finalised but Pick is not yet Finalised, Packing should be editable.", false,
				packingParent.IsParentJobFinalised);

			// finalise the pick
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(pick);
			AssertEquals("Pick is Finalised, Packing should be ReadOnly.", true, packingParent.IsParentJobFinalised);
		}

		public void TestIPackingParent_GetPackageActionStrategy_AllStrategiesReturnNull()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var package = Factory.New<PkgPackage>();

			var strategyMock1 = new Mock<IWhsPackageStrategy>();
			var strategyMock2 = new Mock<IWhsPackageStrategy>();
			strategyMock1.Setup(s => s.GetPackageActionStrategy(order, package))
				.Returns<IWhsOrder, IPkgPackage>((o, p) => null).Verifiable();
			strategyMock2.Setup(s => s.GetPackageActionStrategy(order, package))
				.Returns<IWhsOrder, IPkgPackage>((o, p) => null).Verifiable();
			var strategyMockList = new ListObject();
			strategyMockList.Add(strategyMock1.Object);
			strategyMockList.Add(strategyMock2.Object);

			using (ObjectFactory.Substitute("WhsPackageStrategyList", strategyMockList))
			{
				order.PackageJob.Packages.Add(package);
				var packageActionStrategy = ((IPackingParent)order).GetPackageActionStrategy(package);
				strategyMock1.VerifyAll();
				strategyMock2.VerifyAll();
				AssertPackageActionStrategiesAreEqual(new PackageActionStrategy(package), packageActionStrategy);
			}
		}

		void AssertPackageActionStrategiesAreEqual(IPackageActionStrategy expectedStrategy,
			IPackageActionStrategy actualStrategy)
		{
			AssertEquals(expectedStrategy.Package, actualStrategy.Package);
			AssertEquals(expectedStrategy.ReasonForNotAllowingAction, actualStrategy.ReasonForNotAllowingAction);
			foreach (PackageAction action in Enum.GetValues(typeof(PackageAction)))
			{
				AssertEquals(expectedStrategy.IsActionAllowed(action), actualStrategy.IsActionAllowed(action));
			}
		}

		public void TestIPackingParent_GetPackageActionStrategy_OneStrategyReturnsPackageActionStrategy()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var package = Factory.New<PkgPackage>();

			var packageActionStrategyReturn = new PackageActionStrategy(null, PackageAction.Delete, "Test");
			var strategyMock1 = new Mock<IWhsPackageStrategy>();
			strategyMock1.Setup(s => s.GetPackageActionStrategy(order, package)).Returns(packageActionStrategyReturn)
				.Verifiable();

			using (ObjectFactory.Substitute("WhsPackageStrategyList",
					   new IWhsPackageStrategy[] { strategyMock1.Object }))
			{
				order.PackageJob.Packages.Add(package);
				var packageActionStrategy = ((IPackingParent)order).GetPackageActionStrategy(package);
				strategyMock1.VerifyAll();
				AssertEquals(packageActionStrategy, packageActionStrategyReturn);
			}
		}

		public void TestIPackingParent_OnPackageDelete()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var package = Factory.New<PkgPackage>();

			var strategyMock1 = new Mock<IWhsPackageStrategy>();
			var strategyMock2 = new Mock<IWhsPackageStrategy>();
			strategyMock1.Setup(s => s.OnPackageDelete(order, package)).Verifiable();
			strategyMock2.Setup(s => s.OnPackageDelete(order, package)).Verifiable();

			using (ObjectFactory.Substitute("WhsPackageStrategyList",
					   new IWhsPackageStrategy[] { strategyMock1.Object, strategyMock2.Object }))
			{
				order.PackageJob.Packages.Add(package);
				((IPackingParent)order).OnPackageDelete(package);
				strategyMock1.VerifyAll();
				strategyMock2.VerifyAll();
			}
		}

		public void TestIPackingParent_IsReadOnly_PickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;

			var splitCaseRefType = PackingHelper.CreateRefPackType("1", "1", 3m, 6m, 12m, Constants.Length.Feet, 7,
				Constants.Weight.Pounds, UOMPackTypesList.Codes.SplitCase);

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, splitCaseRefType.F3_Code);
			Helper.CreateProductBOM(bike, frame, 1m, splitCaseRefType.F3_Code);

			var pc = Helper.CreateProduct(data.Org1, "PC");
			pc.OP_IsComponentPickedOnSalesOrder = true;
			var keyboard = Helper.CreateProduct(data.Org1, "BOARD");
			var mouse = Helper.CreateProduct(data.Org1, "MOUSE");
			Helper.CreateProductBOM(pc, keyboard, 1m, splitCaseRefType.F3_Code);
			Helper.CreateProductBOM(pc, mouse, 1m, splitCaseRefType.F3_Code);

			bike.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			wheel.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			frame.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			pc.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			keyboard.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			mouse.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			data.Part2.OP_StockKeepingUnit = splitCaseRefType.F3_Code;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var location = data.Whs1.FindLocation("A");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 50m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 25m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, keyboard, 50m, location);
			Helper.CreateWhsReceiveInventoryLine(receive, mouse, 50m, location);
			receive.FinaliseDocket();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT", bike, 10m);
			var kitOrderLine = order.Lines[0];
			var wheelLine = Helper.CreateWhsOrderLine(order, wheel, 1m);
			var frameLine = Helper.CreateWhsOrderLine(order, frame, 1m);
			var pcLine = Helper.CreateWhsOrderLine(order, pc, 25m);
			var pcLine2 = Helper.CreateWhsOrderLine(order, pc, 14m);

			var pick = Helper.CreatePickNew(order);

			AssertEquals(false, pick.IsFinalised);
			AssertEquals(false, ((IPackingParent)order).IsReadOnly);
		}

		#region TestIPackingParent_IsPackingJobReadOnly

		public void TestIPackingParent_IsPackingJobReadOnly_PickTaskPlanningStatusIsIsReadyForPlanning()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			Factory.Save();

			AssertEquals(true, ((IPackingParent)order).IsPackingJobReadOnly);
		}

		public void TestIPackingParent_IsPackingJobReadOnly_PickTaskPlanningStatusIsPlanned()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			pick.WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			Factory.Save();

			AssertEquals(true, ((IPackingParent)order).IsPackingJobReadOnly);
		}

		public void TestIPackingParent_IsPackingJobReadOnly_PickIsFinalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "WL10000001", startTime: DateTimeOffset.Now);
			order.WD_WLO_PlannedLoad = load.PK;

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickStatus = PickStatus.Codes.Finalised;
			pick.WP_FinalizedDateUtc = ZDateTime.UtcNow;
			order.WD_DocketStatus = WhsOrderStatus.Codes.Departed;
			order.WD_FinalisedDate = ZDateTimeOffset.UtcNow;
			Factory.Save();

			AssertEquals("WL10000001", order.LoadID);
			AssertEquals(false, data.Whs1.WW_GG_ReleaseGroup.IsValid);
			AssertEquals(true, order.IsPickFinalised);
			CombineAssertions(() =>
			{
				TestIPackingParent_IsPackingJobReadOnlyCore(order, load, string.Empty, true);
				TestIPackingParent_IsPackingJobReadOnlyCore(order, load, TaskPlanningStatus.Codes.NotReady, true);
				TestIPackingParent_IsPackingJobReadOnlyCore(order, load, TaskPlanningStatus.Codes.Ready, true);
				TestIPackingParent_IsPackingJobReadOnlyCore(order, load, TaskPlanningStatus.Codes.Planned, true);
			});
		}

		public void TestIPackingParent_IsPackingJobReadOnly_OneLoad()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "WL10000001", startTime: DateTimeOffset.Now);
			order.WD_WLO_PlannedLoad = load.PK;
			Factory.Save();

			AssertEquals("WL10000001", order.LoadID);
			AssertEquals(false, order.IsPickFinalised);
			AssertEquals(true, data.Whs1.WW_GG_ReleaseGroup.IsValid);
			CombineAssertions(() =>
			{
				TestIPackingParent_IsPackingJobReadOnlyCore(order, load, string.Empty, false);
				TestIPackingParent_IsPackingJobReadOnlyCore(order, load, TaskPlanningStatus.Codes.NotReady, false);
				TestIPackingParent_IsPackingJobReadOnlyCore(order, load, TaskPlanningStatus.Codes.Ready, true);
				TestIPackingParent_IsPackingJobReadOnlyCore(order, load, TaskPlanningStatus.Codes.Planned, true);
			});
		}

		public void TestIPackingParent_IsPackingJobReadOnly_MultipleLoad()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL20000001", "CDS", startTime: DateTimeOffset.Now);
			var load2 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL20000002", "CDS", startTime: DateTimeOffset.Now);
			var pick = Helper.CreatePickNew(order);
			var packageJob = order.PackageJob;
			var package1 = packageJob.Packages.AddNew("CTN", 1);
			var package2 = packageJob.Packages.AddNew("BOX", 1);
			var pivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load1);
			var pivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load2);
			Factory.Save();

			AssertEquals(ZString.Empty, load1.WLO_TaskPlanningStatus);
			AssertEquals(ZString.Empty, load2.WLO_TaskPlanningStatus);
			AssertEquals(false, order.IsPickFinalised);
			AssertEquals(true, data.Whs1.WW_GG_ReleaseGroup.IsValid);
			CombineAssertions(() =>
			{
				TestIPackingParent_IsPackingJobReadOnlyCore(order, load1, string.Empty, false);
				TestIPackingParent_IsPackingJobReadOnlyCore(order, load1, TaskPlanningStatus.Codes.NotReady, false);
				TestIPackingParent_IsPackingJobReadOnlyCore(order, load1, TaskPlanningStatus.Codes.Ready, true);
				TestIPackingParent_IsPackingJobReadOnlyCore(order, load1, TaskPlanningStatus.Codes.Planned, true);
			});

			load1.WLO_TaskPlanningStatus = string.Empty;
			CombineAssertions(() =>
			{
				TestIPackingParent_IsPackingJobReadOnlyCore(order, load2, string.Empty, false);
				TestIPackingParent_IsPackingJobReadOnlyCore(order, load2, TaskPlanningStatus.Codes.NotReady, false);
				TestIPackingParent_IsPackingJobReadOnlyCore(order, load2, TaskPlanningStatus.Codes.Ready, true);
				TestIPackingParent_IsPackingJobReadOnlyCore(order, load2, TaskPlanningStatus.Codes.Planned, true);
			});
		}

		public void TestIPackingParent_IsPackingJobReadOnly_ReleaseGroupIsNotValid()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "WL10000001", startTime: DateTimeOffset.Now);
			order.WD_WLO_PlannedLoad = load.PK;
			Factory.Save();

			AssertEquals("WL10000001", order.LoadID);
			AssertEquals(false, order.IsPickFinalised);
			AssertEquals(false, data.Whs1.WW_GG_ReleaseGroup.IsValid);
			CombineAssertions(() =>
			{
				TestIPackingParent_IsPackingJobReadOnlyCore(order, load, string.Empty, false);
				TestIPackingParent_IsPackingJobReadOnlyCore(order, load, TaskPlanningStatus.Codes.NotReady, false);
				TestIPackingParent_IsPackingJobReadOnlyCore(order, load, TaskPlanningStatus.Codes.Ready, false);
				TestIPackingParent_IsPackingJobReadOnlyCore(order, load, TaskPlanningStatus.Codes.Planned, false);
			});
		}

		void TestIPackingParent_IsPackingJobReadOnlyCore(WhsOrder order, WhsLoad load, string loadPlanningStatus, bool expectedPackingJobReadOnly)
		{
			load.WLO_TaskPlanningStatus = loadPlanningStatus;
			Factory.Save();

			AssertEquals(expectedPackingJobReadOnly, ((IPackingParent)order).IsPackingJobReadOnly);
		}

		#endregion

		public void TestIPackingParent_ConsigneeAddressOverridden_IsAutoPrintAllowedFallsBackToRegistry()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var packingParent = (IPackingParentWithPackableItems)order;
			order.WD_DocketID = "W00123";
			order.WD_ExternalReference = "abc";

			AssertEquals("Precondition: registry fallback setting is disabled.", false,
				PackingRegistry.Instance.AutoPrintLabelOnPackageCloseForOverriddenConsigneeAddresses.Value);
			AssertEquals("Precondition: OM_IsLabelPrintedOnClosePackage is enabled.", true,
				order.Consignee.MiscServ.OM_IsLabelPrintedOnClosePackage);
			AssertEquals("Precondition: consignee address is not overriden.", false,
				order.ConsigneeDocAddress.E2_AddressOverride);
			AssertEquals("Precondition: IsAutoPrintAllowed is true.", true, packingParent.IsAutoPrintAllowed);

			order.ConsigneeDocAddress.E2_AddressOverride = true;
			PackingRegistry.Instance.AutoPrintLabelOnPackageCloseForOverriddenConsigneeAddresses.SetTemporaryValue(
				Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Consignee address is overriden, IsAutoPackAllowed should fallback to registry setting.", true,
				packingParent.IsAutoPrintAllowed);

			PackingRegistry.Instance.AutoPrintLabelOnPackageCloseForOverriddenConsigneeAddresses.SetTemporaryValue(
				Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Consignee address is overriden, IsAutoPackAllowed should fallback to registry setting.",
				false, packingParent.IsAutoPrintAllowed);
		}

		public void TestIPackingParent_ConsigneeAddressNotOverridden_IsAutoPrintAllowedShouldNotFallBackToRegistry()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var packingParent = (IPackingParentWithPackableItems)order;
			order.WD_DocketID = "W00123";
			order.WD_ExternalReference = "abc";

			AssertEquals("Precondition: registry fallback setting is disabled.", false,
				PackingRegistry.Instance.AutoPrintLabelOnPackageCloseForOverriddenConsigneeAddresses.Value);
			AssertEquals("Precondition: OM_IsLabelPrintedOnClosePackage is enabled.", true,
				order.Consignee.MiscServ.OM_IsLabelPrintedOnClosePackage);
			AssertEquals("Precondition: consignee address is not overriden.", false,
				order.ConsigneeDocAddress.E2_AddressOverride);
			AssertEquals("Precondition: IsAutoPrintAllowed is true.", true, packingParent.IsAutoPrintAllowed);

			order.Consignee.MiscServ.OM_IsLabelPrintedOnClosePackage = false;
			PackingRegistry.Instance.AutoPrintLabelOnPackageCloseForOverriddenConsigneeAddresses.SetTemporaryValue(
				Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(
				"Consignee address is not overriden. IsAutoPackAllowed should not fallback to registry setting.", false,
				packingParent.IsAutoPrintAllowed);
		}

		public void TestIPackingParent_IsEventParent_DocketIdMatch()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var clientABC = Helper.CreateClient("ABC");
			var bike = Helper.CreateProduct(clientABC, "BIKE");
			var order = Helper.CreateWhsOrderWithOrderLine(clientABC, data.Whs1, "Order1", bike, 10m);
			order.WD_DocketID = "O1";
			Factory.Save();

			var deserializer = ObjectFactory.Get<IXmlEventDeserializer>();
			var xmlEvent = deserializer.Parse(dummyEvent);

			AssertEquals(true, ((IPackingParent)order).IsUXMLEventParent(xmlEvent));
		}

		public void TestIPackingParent_IsEventParent_ExternalReferenceAndClientCodeMatch()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var clientABC = Helper.CreateClient("ABC");
			var bike = Helper.CreateProduct(clientABC, "BIKE");
			var order = Helper.CreateWhsOrderWithOrderLine(clientABC, data.Whs1, "Order1", bike, 10m);
			order.WD_DocketID = "Order1";
			Factory.Save();

			var deserializer = ObjectFactory.Get<IXmlEventDeserializer>();
			var xmlEvent = deserializer.Parse(dummyEvent);

			AssertEquals(true, ((IPackingParent)order).IsUXMLEventParent(xmlEvent));
		}

		public void TestIPackingParent_IsEventParent_ExternalReferenceMatch_ClientCodeNotMatch()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var clientXYZ = Helper.CreateClient("XYZ");
			var bike = Helper.CreateProduct(clientXYZ, "BIKE");
			var order = Helper.CreateWhsOrderWithOrderLine(clientXYZ, data.Whs1, "Order1", bike, 10m);
			order.WD_DocketID = "Order1";
			Factory.Save();

			var deserializer = ObjectFactory.Get<IXmlEventDeserializer>();
			var xmlEvent = deserializer.Parse(dummyEvent);

			AssertEquals(false, ((IPackingParent)order).IsUXMLEventParent(xmlEvent));
		}

		public void TestIPackingParent_IsEventParent_ClientCodeMatch_ExternalReferenceNotMatch()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var clientABC = Helper.CreateClient("ABC");
			var bike = Helper.CreateProduct(clientABC, "BIKE");
			var order = Helper.CreateWhsOrderWithOrderLine(clientABC, data.Whs1, "O1", bike, 10m);
			order.WD_DocketID = "Order1";
			Factory.Save();

			var deserializer = ObjectFactory.Get<IXmlEventDeserializer>();
			var xmlEvent = deserializer.Parse(dummyEvent);

			AssertEquals(false, ((IPackingParent)order).IsUXMLEventParent(xmlEvent));
		}

		const string dummyEvent = @"<UniversalEvent>
	<Event>
		<EventTime>2018-04-01T01:02:03.001</EventTime>
		<EventType>STU</EventType>
		<EventReference>TYP=CBN|RFN=132132132|RES=Carrier Label was generated</EventReference>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>PkgPackage</Type>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>TMS</Code>
			</Company>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>EVP</ServerID>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>ClientReference</Type>
				<Value>Order1</Value>
			</Context>
			<Context>
				<Type>OrderNumber</Type>
				<Value>O1</Value>
			</Context>
			<Context>
				<Type>TransportBookingPackageID</Type>
				<Value>P0001</Value>
			</Context>
			<Context>
				<Type>ClientCode</Type>
				<Value>ABC</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		public void TestExportUniversalEvent_WhenPackagePackingCompleted()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew("BOX");

			Factory.Save();

			var trigger = order.WorkflowItems.Triggers.AddNew();
			trigger.P9_LineTriggerType = TriggerLineTypes.Codes.PkgPackage;
			trigger.TriggerConditions.TriggerEventCode = Events.PackingCompletedCode;
			AssertNoErrors(trigger.P9_LineTriggerTypeInfo);

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Event;

			var communicationsMode = Factory.New<EDICommunicationsMode>();
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
			communicationsMode.EK_CommunicationsTransport =
				EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			communicationsMode.EK_Destination = "UNVRSLVNT";

			var workFlowTriggerEvents = trigger.GetLogs().Find(l => l.SL_SE_NKEvent == Events.WorkflowTriggerEventCode);
			AssertEquals("Pre-condition:", 0, workFlowTriggerEvents.Count());

			var logBO = package.Logs.AddNew(Events.PackingCompleted, "Packing Completed");
			AssertEquals(1, workFlowTriggerEvents.Count());
		}

		public void TestGetAdditionalEventContextValues_OrderWithPick()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD2", data.Part1, 10m);
			order.WD_DocketID = "W0001";
			var package = PackingHelper.CreatePackage(order, "BOX2", 1, Core.Constants.PkgUnit.Box);
			var pick = Helper.CreatePickNew(order);
			pick.WP_PickNo = "P0001";
			var packingParent = (IPackingParentWithPackableItems)order;
			var eventContext = packingParent.GetAdditionalEventContextValuesFromParent();
			var eventContextString = string.Join("\r\n", eventContext.Select(e => e.Key + " - " + e.Value));
			AssertMultilineASCIIEquals("GetAdditionalEventContextValues()", @"
OrderNumber - W0001
PickNumber - P0001
".Trim(), eventContextString);
		}

		public void TestIPackingParent_TransportReference()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 10m);
			order.WD_DocketID = "Order1";
			order.WD_TransportReference = "Initial Ref Value";
			Factory.Save();

			AssertEquals("Precondition: WD_TransportReference is initial value", "Initial Ref Value",
				order.WD_TransportReference);

			((IPackingParent)order).TransportReference = "TransportRefence Changed";
			AssertEquals("WD_TransportReference updated to new value", "TransportRefence Changed",
				order.WD_TransportReference);
		}

		public void TestIPackingParent_PackageSequenceType()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 10m);
			Factory.Save();

			IPackingParent packingParent = order;
			AssertEquals(PackageSequenceType.Outer, packingParent.PackageSequenceType);
		}

		public void TestIPackingParent_PackageSequenceType_CartonisedPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Order1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.WP_IsCartonised = true;
			Factory.Save();

			IPackingParent packingParent = order;
			AssertEquals(PackageSequenceType.Consolidated, packingParent.PackageSequenceType);
		}

		#endregion

		#region IProcessHandlingInfoProvider Members

		public void TestIProcessHandlingInfoProvider()
		{
			IProcessHandlingInfoProvider orderHandlingInfoProvider = Factory.New<WhsOrder>();
			AssertEquals(typeof(WhsOrderProcessHandlingInfo), orderHandlingInfoProvider.ProcessHandlingInfo.GetType());
		}

		#endregion

		#region IModuleToModule Members

		#region TestCanExportShipment

		public void TestCanExportShipment()
		{
			ZString errorMessage;

			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = "NZAKL";
			order.ConsigneeAddressPK = consignee.MainAddress.PK;
			order.WD_OH_Forwarder = forwarder.PK;

			order.WD_TransportMode = "SEA";
			order.WD_ContainerMode = "";

			var iModuleToModule = (IModuleToModule)order;
			AssertEquals(false, iModuleToModule.CanExportData(out errorMessage));
			AssertEquals(
				"Cannot create a Shipment as the Container Mode is empty and the entered Transport Mode 'Sea Freight' does not support Container Mode 'Less Truck Load'",
				errorMessage);

			order.WD_TransportMode = "";
			order.WD_ContainerMode = "LQD";
			AssertEquals(false, iModuleToModule.CanExportData(out errorMessage));
			AssertEquals(
				"Cannot create a Shipment as the Transport Mode is empty and Transport Mode 'Road Freight' does not support the entered Container Mode 'Liquid'",
				errorMessage);

			order.WD_ContainerMode = "";
			AssertEquals(true, iModuleToModule.CanExportData(out errorMessage));
			AssertEquals("", errorMessage);

			order.WD_OH_Forwarder = ZGuid.Empty;
			AssertEquals(false, iModuleToModule.CanExportData(out errorMessage));
			AssertEquals("Cannot create a Shipment as no Freight Forwarder is specified.", errorMessage);

			order.ConsigneeAddressPK = ZGuid.Empty;
			order.WD_OH_Forwarder = forwarder.PK;
			AssertEquals(false, iModuleToModule.CanExportData(out errorMessage));
			AssertEquals(
				"Cannot create a Shipment as a Consignee Location Port/UNLOCO could not be determined. Check the City, State and Country/Region for a valid combination.",
				errorMessage);

			order.ConsigneeAddressPK = consignee.MainAddress.PK;
			AssertEquals(true, iModuleToModule.CanExportData(out errorMessage));
			AssertEquals("", errorMessage);

			order.WD_TransportMode = "SEA";
			order.WD_ContainerMode = "LCL";
			AssertEquals(true, iModuleToModule.CanExportData(out errorMessage));
			AssertEquals("", errorMessage);
		}

		#endregion

		#region TestGetRelatedShipment

		public void TestGetRelatedShipment()
		{
			var order = Factory.New<WhsOrder>();

			var iModuleToModule = (IModuleToModule)order;
			AssertEquals(null, iModuleToModule.GetRelatedObject());

			var cartageJob = (BusinessObject)Factory.New<ICommonCartage>();
			cartageJob.FillWithValidTestData();
			cartageJob[JobCartageSchema.JJ_ParentID] = order.PK;
			cartageJob[JobCartageSchema.JJ_ConsignmentID] = order.WD_DocketID;

			AssertEquals(null, iModuleToModule.GetRelatedObject());

			var shipment = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var pivot = Factory.New<WhsDocketJobPivot>();
			pivot.WV_WD_Docket = order.PK;
			pivot.WV_DocketType = DocketType.Codes.Order;
			pivot.WV_ParentId = shipment.PK;
			pivot.WV_ParentTableCode = shipment.TablePrefix;

			AssertEquals(shipment, iModuleToModule.GetRelatedObject());
		}

		#endregion

		#region TestChildLinesDoNotUpdateWeightAndVolumeOfDocket

		public void TestChildLinesDoNotUpdateWeightAndVolumeOfDocket()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			//OP_Weight = 2.0m && OP_Cubic = 0.02m for all product
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct1, 1m, Constants.PkgUnit.Unit);

			var bomComponentProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct2, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct1, 20m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct2, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("3 Order Line should be there, 1 ParentLine with 2 child lines", 3, order.AllLines.Count);
			AssertEquals("Total Weight of order should be 20, it should be taken only from parent line", 20m,
				order.WD_TotalWeight);
			AssertEquals("Total Cubic of order should be 0.2, it should be taken only from parent line", 0.2m,
				order.WD_TotalCubic);
		}

		#endregion

		#region TestParentLines

		public void TestParentLines()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct1 =
				Helper.CreateProductBOM(mainProduct, bomComponentProduct1, 1m, Constants.PkgUnit.Unit);

			var bomComponentProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			var bomPartForMainProduct2 =
				Helper.CreateProductBOM(mainProduct, bomComponentProduct2, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 =
				Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct1, 20m, inventoryLocation);
			var inventory3 =
				Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct2, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 8m);
			Factory.Save();

			var pick = Helper.CreatePickNew(new WhsPickableDocket[1] { order });
			AssertEquals("3 Order Line should be there, 1 ParentLine with 2 child lines", 3, order.AllLines.Count);
			AssertEquals("2 child lines should be added to orderLine", 2, orderLine.ChildComponentLines.Count);
			AssertEquals("1 Order Line should be return, consider only parent lines", 1, order.ParentLines.Count);
		}

		#endregion

		#region TestPickingBOMProductWithOrderWhenFulfillmentRuleAllIsSpecified

		public void TestPickingBOMProductWithOrderWhenFulfillmentRuleAllIsSpecified()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct1, 1m, Constants.PkgUnit.Unit);

			var bomComponentProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct2, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct1, 20m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct2, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 2m);
			Factory.Save();

			WhsPick.AutoPickEventArgs args = null;
			var pick = Factory.New<WhsPick>();
			pick.AutoPickAttempt += (sender, e) => args = e;
			pick.PickOrdersWithAllocationMock(order);
			Factory.Save();
			AssertEquals($"Pick {pick.WP_PickNo} has been created.", args.Message);
			AssertEquals(true, args.FulfillmentRulesMet);
			AssertEquals(PickStatus.Codes.Created, pick.WP_PickStatus);

			pick.CancelPick();
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestPickingBOMProductWithOrderWhenFulfillmentRuleAllIsSpecified_CancelPick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct1 = Helper.CreateProduct(data.Org1, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct1, 1m, Constants.PkgUnit.Unit);

			var bomComponentProduct2 = Helper.CreateProduct(data.Org1, "BOM2");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct2, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct1, 20m, inventoryLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct2, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 2m);
			Factory.Save();

			WhsPick.AutoPickEventArgs args = null;
			var pick = Factory.New<WhsPick>();
			pick.AutoPickAttempt += (sender, e) => args = e;
			pick.PickOrdersWithAllocationMock(order);
			Factory.Save();

			pick.CancelPick();
			AssertNoExceptionThrown(() => Factory.Save());
		}

		#endregion

		#endregion

		#region IWhsOrder Members

		public void TestIncludeInRelatedShipmentCharges()
		{
			var order = Factory.New<WhsOrder>();
			IWhsOrder iWhsOrder = order;
			AssertEquals(false, iWhsOrder.IncludeInRelatedShipmentCharges);

			order.WD_OH_Client = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals(true, iWhsOrder.IncludeInRelatedShipmentCharges); // Defaults to true

			order.Client.CompanyData.OB_WhsIncludeReleaseChargesOnShipment = false;
			AssertEquals(false, iWhsOrder.IncludeInRelatedShipmentCharges);
		}

		public void TestTotalOrderLines()
		{
			var order = Factory.New<WhsOrder>();
			IWhsOrder iWhsOrder = order;
			AssertEquals(0, iWhsOrder.TotalOrderLines);

			var line = order.Lines.AddNew();
			AssertEquals(1, iWhsOrder.TotalOrderLines);

			order.Lines.Delete(line);
			AssertEquals(0, iWhsOrder.TotalOrderLines);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2022, 10, 20, 1, 00, 00)]
		public void TestCreateDate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			IWhsOrder iWhsOrder = order;

			AssertEquals(ZDate.Empty, iWhsOrder.CreateDate);

			Factory.Save();

			AssertEquals(new ZDate(2022, 10, 20), iWhsOrder.CreateDate);

			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_Name = "US Test Company";
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			usCompany.GC_Code = "USC";

			var usBranch = usCompany.Branches.AddNew();
			usBranch.GB_Code = "USA";
			usBranch.GB_RL_NKHomePort = "USLAX";
			order.Warehouse.WW_GB_RelatedCompanyBranch = usBranch.PK;
			Factory.Save();

			AssertEquals(new ZDate(2022, 10, 19), iWhsOrder.CreateDate);
		}

		#endregion

		#region IWhsLogEventParent

		protected override string ExpectedEventReferenceParameterType => Constants.EventReferenceParameterTypes.Order;

		#endregion

		#region TestUpdateScreeningStatus_FromScreeningParties

		public void TestUpdateScreeningStatus_FromScreeningParties_OnFactorySave_WhenAllIsClear()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var consignee = Helper.CreateClient("ZZZ");
			Factory.Save();

			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			data.Org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			data.Whs1.WarehouseAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "TEST");
			AssertEquals("Precondition", ScreeningStatusesList.Codes.NotScreened, order.WD_ScreeningStatus);
			foreach (var jobDoc in order.DocAddresses.Cast<JobDocAddress>())
			{
				jobDoc.E2_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			}
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.Clear, order.WD_ScreeningStatus);
		}

		public void TestUpdateScreeningStatus_FromScreeningParties_OnFactorySave_WhenOneHasMatched()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			data.Whs1.WarehouseAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			var consignee = Helper.CreateClient("ZZZ");
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "TEST");
			AssertEquals("Precondition", ScreeningStatusesList.Codes.NotScreened, order.WD_ScreeningStatus);
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.Matched, order.WD_ScreeningStatus);
		}

		public void TestUpdateScreeningStatus_FromScreeningParties_OnFactorySave_WhenUnknownIsWorstInTheList()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			data.Whs1.WarehouseAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			var consignee = Helper.CreateClient("ZZZ");
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "TEST");
			AssertEquals("Precondition", ScreeningStatusesList.Codes.NotScreened, order.WD_ScreeningStatus);
			Factory.Save();

			AssertEquals(ScreeningStatusesList.Codes.Unknown, order.WD_ScreeningStatus);
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
				Factory.Save();

				receive.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				Helper.CreatePickByAttachingOrders(order);

				AssertEquals("Precondition", false, order.IsInDatabase);
				AssertEquals("Precondition", ScreeningStatusesList.Codes.NotScreened, order.WD_ScreeningStatus);
				AssertEquals("Precondition: order.WD_FinalisedDate.IsEmpty", true, order.WD_FinalisedDate.IsEmpty);
				order.FinaliseDocketWithoutUserConfirmation();

				AssertEquals("Order is finalised.", true, order.IsFinalised);
				AssertEquals("Screening status is Clear.", ScreeningStatusesList.Codes.Clear, order.WD_ScreeningStatus);
				AssertEquals("Order has no errors.", false, order.HasErrors);
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
				Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
				receive.AllocateLocationsWithMock();
				Factory.Save();

				receive.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				order.WD_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				Helper.CreatePickByAttachingOrders(order);

				AssertEquals("Precondition", false, order.IsInDatabase);
				AssertEquals("Precondition", ScreeningStatusesList.Codes.Matched, order.WD_ScreeningStatus);
				AssertEquals("Precondition: order.WD_FinalisedDate.IsEmpty", true, order.WD_FinalisedDate.IsEmpty);
				order.FinaliseDocketWithoutUserConfirmation();

				AssertEquals("Order is finalised.", true, order.IsFinalised);
				AssertEquals("Screening status is Clear.", ScreeningStatusesList.Codes.Clear, order.WD_ScreeningStatus);
				AssertEquals("Order has no errors.", false, order.HasErrors);
			}
		}

		public void TestUpdateScreeningStatus_FromScreeningParties_OnFinalise_OrderInDatabase()
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
				Factory.Save();

				receive.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				data.Org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				Helper.CreatePickByAttachingOrders(order);
				Factory.Save();

				AssertEquals("Precondition", true, order.IsInDatabase);
				AssertEquals("Precondition", ScreeningStatusesList.Codes.Matched, order.WD_ScreeningStatus);
				AssertEquals("Precondition: order.WD_FinalisedDate.IsEmpty", true, order.WD_FinalisedDate.IsEmpty);

				order.WD_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				order.FinaliseDocketWithoutUserConfirmation();

				AssertEquals("Order is finalised.", true, order.IsFinalised);
				AssertEquals("Screening status is not updated/retrieved from the client screening party.", ScreeningStatusesList.Codes.Clear, order.WD_ScreeningStatus);
				AssertEquals("Order has no errors.", false, order.HasErrors);
			}
		}

		#endregion

		#region IScreeningPartyProvider

		public void TestIScreeningPartyProvider_ScreeningParties_WhenAllOrganisationsConfigured()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transport = Factory.New<OrgHeader>();
			var transportBillTo = Factory.New<OrgHeader>();
			var forwarder = Factory.New<OrgHeader>();
			var consignee = Factory.New<OrgHeader>();
			var supplier = Factory.New<OrgHeader>();
			var goodsBillTo = Factory.New<OrgHeader>();
			var distribution = Factory.New<OrgHeader>();
			var bookingAgent = Factory.New<OrgHeader>();
			var pickup = Factory.New<OrgHeader>();
			var dropoff = Factory.New<OrgHeader>();
			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "TEST");
			var job = new Job.Loader(order).TryCreate();
			order.TransportCoPK = transport.PK;
			order.TransportBillToDocAddress.OrganisationPK = transportBillTo.PK;
			order.WD_OH_Forwarder = forwarder.PK;
			order.GoodsBillToPK = goodsBillTo.PK;
			order.DistributionCentreDocAddress.OrganisationPK = distribution.PK;
			order.CarrierBookingAgentDocAddress.OrganisationPK = bookingAgent.PK;
			order.SupplierDocAddress.OrganisationPK = supplier.PK;
			order.PickUpPK = pickup.PK;
			order.DropOffPK = dropoff.PK;

			var screeningParties = ((IScreeningPartyProvider)order).ScreeningParties;

			AssertEquals(13, screeningParties.Length);
			AssertContainsDeniedCandidate("Client", data.Org1, screeningParties);
			AssertContainsDeniedCandidate("Warehouse", data.Whs1.WarehouseAddress.Header, screeningParties);
			AssertContainsDeniedCandidate("Local Client", job.LocalCharges, screeningParties);
			AssertContainsDeniedCandidateAsJobAddress("Transport Company", transport, screeningParties);
			AssertContainsDeniedCandidateAsJobAddress("Transport Bill To Address", transportBillTo, screeningParties);
			AssertContainsDeniedCandidateAsJobAddress("Supplier Documentary Address", supplier, screeningParties);
			AssertContainsDeniedCandidate("Forwarder", forwarder, screeningParties);
			AssertContainsDeniedCandidateAsJobAddress("Consignee Address", consignee, screeningParties);
			AssertContainsDeniedCandidateAsJobAddress("Goods Billed To Address", goodsBillTo, screeningParties);
			AssertContainsDeniedCandidateAsJobAddress("Distribution Center Address", distribution, screeningParties);
			AssertContainsDeniedCandidateAsJobAddress("Carrier Booking Agent", bookingAgent, screeningParties);
			AssertContainsDeniedCandidateAsJobAddress("Pick Up Address", pickup, screeningParties);
			AssertContainsDeniedCandidateAsJobAddress("Drop Off Address", dropoff, screeningParties);
		}

		public void TestIScreeningPartyProvider_ScreeningParties_WhenMinimumConfigured()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			var screeningParties = ((IScreeningPartyProvider)order).ScreeningParties;

			AssertContainsDeniedCandidate("Client", data.Org1, screeningParties);
			AssertContainsDeniedCandidate("Warehouse", data.Whs1.WarehouseAddress.Header, screeningParties);
		}

		public void TestIScreeningPartyProvider_GetWorstScreeningStatus_WhenOneHasMatched()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var consignee = Factory.New<OrgHeader>();
			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "TEST");
			data.Org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			data.Whs1.WarehouseAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

			var worst = order.GetWorstScreeningStatus();

			AssertEquals(ScreeningStatusesList.Codes.Matched, worst);
		}

		public void TestIScreeningPartyProvider_GetWorstScreeningStatus_WhenUnknownIsWorstInTheList()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var consignee = Factory.New<OrgHeader>();
			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "TEST");
			data.Org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			data.Whs1.WarehouseAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;

			var worst = order.GetWorstScreeningStatus();

			AssertEquals(ScreeningStatusesList.Codes.Unknown, worst);
		}

		public void TestIScreeningPartyProvider_GetWorstScreeningStatus_WhenAllIsPermanentClear()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var consignee = Factory.New<OrgHeader>();
			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "TEST");
			data.Org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			data.Whs1.WarehouseAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			foreach (var jobDoc in order.DocAddresses.Cast<JobDocAddress>())
			{
				jobDoc.E2_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			}

			var worst = order.GetWorstScreeningStatus();

			AssertEquals(ScreeningStatusesList.Codes.PermanentClear, worst);
		}

		public void
			TestIScreeningPartyProvider_GetWorstScreeningStatusUnlessManuallyCleared_WhenCurrentStatusIsJobClearedButContainsMatched()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var consignee = Factory.New<OrgHeader>();
			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "TEST");
			data.Org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			data.Whs1.WarehouseAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			order.WD_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;

			var worst = order.GetWorstScreeningStatusUnlessManuallyCleared();

			AssertEquals(ScreeningStatusesList.Codes.JobCleared, worst);
		}

		#endregion

		#region ICreditControlledDocumentDelivery

		public void TestICreditControlledDocumentDelivery_GetScreeningParties_ReturnsSameAsScreeningParties()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			var screeningParties = ((ICreditControlledDocumentDelivery)order).GetScreeningParties();

			AssertContainsDeniedCandidate("Client", data.Org1, screeningParties);
			AssertContainsDeniedCandidate("Warehouse", data.Whs1.WarehouseAddress.Header, screeningParties);
		}

		protected override void TestIsDPSFreightMovementRestrictedCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var testCases = new[]
			{
				(ScreeningStatusesList.Codes.PermanentClear, false), (ScreeningStatusesList.Codes.Clear, false),
				(ScreeningStatusesList.Codes.JobCleared, false), (ScreeningStatusesList.Codes.Unknown, true),
				(ScreeningStatusesList.Codes.NotScreened, true), (ScreeningStatusesList.Codes.Matched, true)
			};

			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(
					   GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
					   DPSFreightMovementRestrictionsOptions.Codes.All))
			{
				CombineAssertions(() =>
				{
					foreach (var (screeningStatus, expected) in testCases)
					{
						order.WD_ScreeningStatus = screeningStatus;

						var restricted = ((ICreditControlledDocumentDelivery)order).IsDPSFreightMovementRestricted;

						AssertEquals($"Restriction for status {screeningStatus} should be {expected}", expected,
							restricted);
					}
				});
			}
		}

		protected override bool TestIsDPSFreightMovementRestricted_All => true;

		#endregion

		#region WhsOrderWrapperStrategy

		public void TestBuildWrapperStrategyIsWeb()
		{
			// Arrange
			var originalIsWeb = Globals.IsWeb;
			try
			{
				Globals.IsWeb = true;

				// Action
				var wrapper = GetNewBusinessObject().WrapperStrategy;

				// Assert
				Assert("Should have created the wrapper.", wrapper != null && !(wrapper is WhsOrder));
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

				// Action
				var wrapper = GetNewBusinessObject().WrapperStrategy;

				// Assert
				Assert("Should have created the wrapper.", wrapper != null && wrapper is WhsOrder);
			}
			finally
			{
				Globals.IsWeb = originalIsWeb;
			}
		}

		#endregion

		#region IPackingParentDocumentSupporter

		public void TestIPackingParentDocumentSupporter_GetDataContexts()
		{
			var expectedDataContexts = new[]
			{
				Constants.DataContext.GenericAuditVarianceLabel, Constants.DataContext.GenericAuditVarianceLabelAll
			};
			var docSupporter = Order.DocumentSupporter as IPackingParentDocumentSupporter;
			var actualDataContexts = docSupporter?.GetModuleSpecificPackageSupportedDataContexts();
			AssertContainsExactElementsInAnyOrder(expectedDataContexts, actualDataContexts);

			GlbStaff.CurrentUser.GS_LoginName = "test_notsupport";
			actualDataContexts = docSupporter?.GetModuleSpecificPackageSupportedDataContexts();
			AssertContainsExactElementsInAnyOrder("Audit Labels should be for non-CWSupport",
				Array.Empty<DataContext>(), actualDataContexts);
		}

		public void TestIPackingParentDocumentSupporter_PackageFiltering()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var inventory = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "Inv1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 50m);
			var packWithoutAudit = PackingHelper.CreatePackage(order, "B1", 1, Constants.PkgUnit.Box);
			var docSupporter = order.DocumentSupporter as IPackingParentDocumentSupporter;

			var completeTimeoffset = new ZDateTimeOffset(ZDateTime.BrettsBirthday);
			var packWithPassAudit = PackingHelper.CreatePackage(order, "B2", 1, Constants.PkgUnit.Box);
			var auditB2_Fail01 = Helper.CreateWhsPackageAuditWithLineFailure(packWithPassAudit, data.Part1, 10m, 9m,
				completeTimeoffset.AddDays(1));
			var auditB2_Pass02 = Helper.CreateWhsPackageAudit(packWithPassAudit, completeTimeoffset.AddDays(2));

			var subPackWithFailures = PackingHelper.CreatePackage(packWithPassAudit, 1, Constants.PkgUnit.Box, "B2.01");
			var auditB201_Fail01 = Helper.CreateWhsPackageAuditWithLineFailure(subPackWithFailures, data.Part1, 10m, 9m,
				completeTimeoffset.AddDays(1));
			var auditB201_Pass02 =
				Helper.CreateWhsPackageAudit(subPackWithFailures, completeTimeoffset.AddDays(2));
			var auditB201_Fail03 = Helper.CreateWhsPackageAuditWithLineFailure(subPackWithFailures, data.Part1, 11m,
				10m, completeTimeoffset.AddDays(3));

			var packWithFailAudit = PackingHelper.CreatePackage(order, "B3", 1, Constants.PkgUnit.Box);
			var auditB3_Fail01 = Helper.CreateWhsPackageAuditWithLineFailure(packWithFailAudit, data.Part1, 10m, 9m);
			Factory.Save();

			AssertEquals("Package B1 should be filtered.", false,
				docSupporter.IsPrintablePackageForSpecificModuleDataContext(
					Constants.DataContext.GenericAuditVarianceLabel, packWithoutAudit));
			AssertEquals("Package B1 should be filtered.", false,
				docSupporter.IsPrintablePackageForSpecificModuleDataContext(
					Constants.DataContext.GenericAuditVarianceLabelAll, packWithoutAudit));
			AssertEquals("Package B2 should be filtered.", false,
				docSupporter.IsPrintablePackageForSpecificModuleDataContext(
					Constants.DataContext.GenericAuditVarianceLabel, packWithPassAudit));
			AssertEquals("Package B2 should be filtered.", false,
				docSupporter.IsPrintablePackageForSpecificModuleDataContext(
					Constants.DataContext.GenericAuditVarianceLabelAll, packWithPassAudit));
			AssertEquals("Package B2.01 should be included.", true,
				docSupporter.IsPrintablePackageForSpecificModuleDataContext(
					Constants.DataContext.GenericAuditVarianceLabel, subPackWithFailures));
			AssertEquals("Package B2.01 should be included.", true,
				docSupporter.IsPrintablePackageForSpecificModuleDataContext(
					Constants.DataContext.GenericAuditVarianceLabelAll, subPackWithFailures));
			AssertEquals("Package B3 should be included.", true,
				docSupporter.IsPrintablePackageForSpecificModuleDataContext(
					Constants.DataContext.GenericAuditVarianceLabel, packWithFailAudit));
			AssertEquals("Package B3 should be included.", true,
				docSupporter.IsPrintablePackageForSpecificModuleDataContext(
					Constants.DataContext.GenericAuditVarianceLabelAll, packWithFailAudit));
		}

		public void TestIPackingParentDocumentSupporter_GetModuleSpecificNotFoundMessage()
		{
			var docSupporter = Order.DocumentSupporter as IPackingParentDocumentSupporter;
			AssertEquals("No variance was found for selected package(s)",
				docSupporter.GetModuleSpecificNotFoundMessage(Constants.DataContext.GenericAuditVarianceLabel));
			AssertEquals("No variance was found for any packages in this job.",
				docSupporter.GetModuleSpecificNotFoundMessage(Constants.DataContext.GenericAuditVarianceLabelAll));
			AssertEquals("", docSupporter.GetModuleSpecificNotFoundMessage(Constants.DataContext.AccountingJournal));
		}

		public void TestIPackingParentDocumentSupporter_PackageDepthLevels()
		{
			var docSupporter = Order.DocumentSupporter as IPackingParentDocumentSupporter;
			AssertEquals("Should not allow all package levels for GenericAuditVarianceLabel", false,
				docSupporter.IsAllPackLevelsEnabled(Constants.DataContext.GenericAuditVarianceLabel));
			AssertEquals("Should allow all package levels for GenericAuditVarianceLabelAll", true,
				docSupporter.IsAllPackLevelsEnabled(Constants.DataContext.GenericAuditVarianceLabelAll));
		}

		#endregion

		#region IDocketInternals Members

		protected override void TestIDocketInternalsFlagDocketAsFinalisedCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var docket = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			AssertEquals(DocketStatus.Codes.New, docket.WD_DocketStatus);
			AssertEquals(ZDateTimeOffset.Empty, docket.WD_FinalisedDate);

			var docketLine = Helper.CreateWhsOrderLine(docket, data.Part1, 10m);
			AssertEquals(ZString.Empty, docketLine.WE_DocketLineStatus);
			AssertEquals(ZDateTimeOffset.Empty, docketLine.WE_FinalisedDate);

			((IDocketInternals)docket).FlagDocketAsFinalised();

			AssertEquals(DocketStatus.Codes.New, docket.WD_DocketStatus); // Order.WD_DocketStatus doesn't get set to FIN by finalisation.
			AssertEquals(DocketStatus.Codes.Finalised, docketLine.WE_DocketLineStatus);

			var now = data.Whs1.GetWarehouseBranchDateTimeOffset(ZDateTime.Now);
			now = new ZDateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 34, now.Offset); // ensure that seconds != 0.
			AssertNotEquals(now, docket.WD_FinalisedDate);
			AssertNotEquals(now, docketLine.WE_FinalisedDate);

			now = new ZDateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, now.Offset); // ensure that seconds = 0
			AssertEquals(now, docket.WD_FinalisedDate);
			AssertEquals(now, docketLine.WE_FinalisedDate);
		}

		#endregion

		#region IPropertyChecker Members

		public void TestIPropertyChecker_IsPropertyUpdatableViaXueAdditionalFields()
		{
			var whs = Helper.CreateWarehouse("1");
			var client = Helper.CreateClient();
			var order = GetNewBusinessObject();
			order.WD_WW_Whs = whs.PK;
			order.WD_OH_Client = client.PK;

			var propertyChecker = order as IPropertyChecker;
			AssertNotNull(propertyChecker);

			// Just test a single property that's not IsOrderHeld for now
			var property = order.GetType().GetProperty(nameof(order.WD_ExternalReference));

			var result = propertyChecker.IsPropertyUpdatableViaXueAdditionalFields(property, "TEST", out var message1);
			AssertEquals("Property should be updateable.", true, result);
			AssertNull("Property should be updateable.", message1);
		}

		public void TestIPropertyChecker_IsPropertyUpdatableViaXueAdditionalFields_IsOrderHeld()
		{
			var whs = Helper.CreateWarehouse("1");
			var client = Helper.CreateClient();
			var order = GetNewBusinessObject();
			order.WD_WW_Whs = whs.PK;
			order.WD_OH_Client = client.PK;

			var propertyChecker = order as IPropertyChecker;
			AssertNotNull(propertyChecker);

			var property = order.GetType().GetProperty(nameof(order.IsOrderHeld));

			foreach (var status in new[] { DocketStatus.Codes.Entered, "eNt", DocketStatus.Codes.Entered, "nEw", DocketStatus.Codes.Held, "hEl" })
			{
				order.WD_DocketStatus = status;
				var result = propertyChecker.IsPropertyUpdatableViaXueAdditionalFields(property, true, out var message1);
				AssertEquals("Property should be updateable.", true, result);
				AssertNull("Property should be updateable.", message1);
			}
		}

		public void TestIPropertyChecker_IsPropertyUpdatableViaXueAdditionalFields_IsOrderHeld_AttachedToPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition.", DocketStatus.Codes.AttachedToPick, order.WD_DocketStatus);

			var propertyChecker = order as IPropertyChecker;
			AssertNotNull(propertyChecker);

			var property = order.GetType().GetProperty(nameof(order.IsOrderHeld));

			var result = propertyChecker.IsPropertyUpdatableViaXueAdditionalFields(property, true, out var message1);
			AssertEquals("Property should *not* be updateable.", false, result);
			AssertEquals("Property should *not* be updateable.", "Is Order Held can only be set for Entered, New or Held Orders.", message1);
		}

		#endregion

		#region ITaskPlanningJob

		protected override ZString HumanReadableNameWithoutID => "Warehouse Order";

		#endregion

		#region TestLines_CountChanged

		#region TestLines_CountChanged_OrderLinesAdded_UpdatesPickOrderedInventories

		public void TestLines_CountChanged_OrderLinesAdded_UpdatesPickOrderedInventories()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "1", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 6m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine2.WE_PartAttrib1 = "1";
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertEquals(2, pick.OrderedInventories.Count);
			var orderedInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
				.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 6);
			AssertNotNull("Precondition", orderedInventory1);
			AssertEquals("Precondition: ordered inventory has only 1 owner.", 1, orderedInventory1.Owners.Count);
			AssertNotNull("Precondition",
				pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
					.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 10));

			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 8m);
			order.IsSavedFromOrderForm = true;
			order.RunPreSaveValidation();
			Factory.Save();

			AssertEquals("pick's OrderedInventories count should still be 2.", 2, pick.OrderedInventories.Count);
			orderedInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
				.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 14);
			AssertNotNull(orderedInventory1);
			AssertEquals("ordered inventory now has 2 owners.", 2, orderedInventory1.Owners.Count);
			AssertNotNull(pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
				.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 10));
		}

		#endregion

		#region TestLines_CountChanged_SuspendUpdatingPickOrderedInventories

		public void TestLines_CountChanged_SuspendUpdatingPickOrderedInventories()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "1", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 6m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine2.WE_PartAttrib1 = "1";
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertEquals(2, pick.OrderedInventories.Count);
			var orderedInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
				.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 6);
			AssertNotNull("Precondition", orderedInventory1);
			AssertEquals("Precondition: ordered inventory has only 1 owner.", 1, orderedInventory1.Owners.Count);
			AssertNotNull("Precondition",
				pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
					.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 10));

			using (order.SuspendUpdatingPickOrderedInventories())
			{
				var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 8m);
			}

			AssertEquals("pick's OrderedInventories count should still be 2.", 2, pick.OrderedInventories.Count);
			orderedInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
				.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 6);
			AssertNotNull(orderedInventory1);
			AssertEquals("ordered inventory still has 1 owner.", 1, orderedInventory1.Owners.Count);
			AssertNotNull(pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
				.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 10));

			var orderLine4 = Helper.CreateWhsOrderLine(order, data.Part1, 4m);

			orderedInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
				.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 18);
			AssertNotNull(orderedInventory1);
			AssertEquals("ordered inventory now has 3 owners.", 3, orderedInventory1.Owners.Count);
			AssertNotNull(pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
				.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 10));
		}

		#endregion

		#region TestLines_CountChanged_OrderLineRemoved_UpdatesPickOrderedInventories

		public void TestLines_CountChanged_OrderLineRemoved_UpdatesPickOrderedInventories()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "1", "", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 6m);
			var orderLine1 = order.Lines[0];
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine2.WE_PartAttrib1 = "1";
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertEquals(2, pick.OrderedInventories.Count);
			AssertNotNull("Precondition",
				pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
					.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 6));
			AssertNotNull("Precondition",
				pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
					.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 10));

			orderLine1.WE_TransactionQuantity = 0m;
			order.IsSavedFromOrderForm = true;
			order.RunPreSaveValidation();
			Factory.Save();

			AssertEquals("pick's OrderedInventories count should be reduced to 1.", 1, pick.OrderedInventories.Count);
			AssertNull("Precondition",
				pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
					.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 6));
			AssertNotNull("Precondition",
				pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
					.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 10));
		}

		#endregion

		#region TestLines_OrderLinesDeletedAndRecreatedOnAnotherFactory_UpdatesPickOrderedInventoriesOnBothFactories

		public void
			TestLines_OrderLinesDeletedAndRecreatedOnAnotherFactory_UpdatesPickOrderedInventoriesOnBothFactories()
		{
			if (!Globals.IsWeb)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAllAttributeType(data.Org1, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "1", "", "",
					"");
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 6m);
				var orderLine1 = order.Lines[0];
				var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				orderLine2.WE_PartAttrib1 = "1";
				var pick = Helper.CreatePickNew(order);
				Factory.Save();

				AssertNotNull(pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
					.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 6));
				AssertNotNull(pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
					.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 10));
				orderLine2.ReleaseLines[0].Quantity = 8m;

				var newFactory = new BusinessObjectFactory();
				var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
				var orderLine1InNewFactory = orderInNewFactory.Lines.Single(line => line.WE_TransactionQuantity == 6m);
				orderLine1InNewFactory.WE_TransactionQuantity = 0m;
				var orderLine3InNewFactory = orderInNewFactory.Lines.AddNew();
				orderLine3InNewFactory.WE_TransactionQuantity = 4m;
				orderLine3InNewFactory.WE_OP = data.Part1.PK;
				orderInNewFactory.IsSavedFromOrderForm = true;
				orderInNewFactory.RunPreSaveValidation();
				newFactory.Save();

				Assert("orderLine1 has been deleted.", orderLine1.IsDeleted);
				var orderLine3InOrigFactory = Factory.Load<WhsOrderLine>(orderLine3InNewFactory.PK);
				AssertEquals("New order line has release lines in the original factory.", true,
					orderLine3InOrigFactory.ReleaseLines.Count > 0);

				AssertEquals("pick's OrderedInventories count should still be 2.", 2, pick.OrderedInventories.Count);
				AssertNotNull(pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
					.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 10));
				AssertNotNull(pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
					.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 4));
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region TestLines_OrderLinesAddedOnAnotherFactory_UpdatesPickOrderedInventoriesOnBothFactories

		public void TestLines_OrderLinesAddedOnAnotherFactory_UpdatesPickOrderedInventoriesOnBothFactories()
		{
			if (!Globals.IsWeb)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAllAttributeType(data.Org1, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "1", "", "",
					"");
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 6m);
				var orderLine1 = order.Lines[0];
				var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				orderLine2.WE_PartAttrib1 = "1";
				var pick = Helper.CreatePickNew(order);
				Factory.Save();

				AssertEquals(2, pick.OrderedInventories.Count);
				var orderedInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
					.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 6);
				AssertNotNull("Precondition", orderedInventory1);
				AssertEquals("Precondition: ordered inventory has only 1 owner.", 1, orderedInventory1.Owners.Count);
				AssertNotNull("Precondition",
					pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
						.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 10));

				var newFactory = new BusinessObjectFactory();
				var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
				var orderLine3InNewFactory = orderInNewFactory.Lines.AddNew();
				orderLine3InNewFactory.WE_TransactionQuantity = 8m;
				orderLine3InNewFactory.WE_OP = data.Part1.PK;
				orderInNewFactory.IsSavedFromOrderForm = true;
				orderInNewFactory.RunPreSaveValidation();
				newFactory.Save();

				AssertEquals("pick's OrderedInventories count should still be 2.", 2, pick.OrderedInventories.Count);
				orderedInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
					.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 14);
				AssertNotNull(orderedInventory1);
				AssertEquals("First ordered inventory's owers count is incremented.", 2,
					orderedInventory1.Owners.Count);
				AssertNotNull(pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
					.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 10));
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region TestLines_OrderLinesRemovedOnAnotherFactory_UpdatesPickOrderedInventoriesOnBothFactories

		public void TestLines_OrderLinesRemovedOnAnotherFactory_UpdatesPickOrderedInventoriesOnBothFactories()
		{
			if (!Globals.IsWeb)
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAllAttributeType(data.Org1, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, ZDate.Empty, ZDate.Empty, "1", "", "",
					"");
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 6m);
				var orderLine1 = order.Lines[0];
				var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
				orderLine2.WE_PartAttrib1 = "1";
				var pick = Helper.CreatePickNew(order);
				Factory.Save();

				AssertEquals(2, pick.OrderedInventories.Count);
				AssertNotNull("Precondition",
					pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
						.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 6));
				AssertNotNull("Precondition",
					pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
						.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 10));

				var newFactory = new BusinessObjectFactory();
				var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
				var orderLine1InNewFactory = orderInNewFactory.Lines.Single(line => line.WE_TransactionQuantity == 6m);
				orderLine1InNewFactory.WE_TransactionQuantity = 0m;
				orderInNewFactory.IsSavedFromOrderForm = true;
				orderInNewFactory.RunPreSaveValidation();
				newFactory.Save();

				AssertEquals("pick's OrderedInventories count should be reduced to 1.", 1,
					pick.OrderedInventories.Count);
				AssertNull(pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
					.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 6));
				AssertNotNull(pick.OrderedInventories.Cast<WhsPickOrderedInventory>()
					.FirstOrDefault(orderedInventory => orderedInventory.QuantityOrdered == 10));
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#endregion

		#region TestClearPickAndReleaseLines

		public void TestClearPickAndReleaseLines()
		{
			if (Globals.IsWeb)
			{
				Assert(true);
			}
			else
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAllAttributeType(data.Org1, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);
				Factory.Save();

				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
				var pick = Helper.CreatePickNew(order);
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var pickInNewFactory = newFactory.Load<WhsPick>(pick.PK);
				var orderLineInNewFactory = newFactory.Load<WhsOrderLine>(orderLine.PK);
				var releaseLine = (WhsReleaseLine)orderLineInNewFactory.ReleaseLines.Single();
				AssertEquals("Precondition", 5m, releaseLine.Quantity);
				releaseLine.PartAttribute1 = "AAA";

				pick.Orders.Remove(order);
				Factory.Save();
				AssertEquals("Order should be removed from Pick in new Factory by DataRefreshBus", 0,
					pickInNewFactory.Orders.Count);
				AssertEquals("Release Line should be removed in new Factory by DataRefreshBus", 0,
					orderLineInNewFactory.ReleaseLines.Count);
			}
		}

		#endregion

		#region TestTG_WhsOrder_EnsureDDLIsEnteredOnPick_ShouldDeferTrigger

		public void TestTG_WhsOrder_EnsureDDLIsEnteredOnPick_ShouldDeferTrigger_insert()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var strategy =
				ObjectFactory.Get<IWhsCheckOrderDockDoorLocationIsMatchWithPick_DeferTriggerStrategy>() as
					IDeferTriggerConditionStrategy;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			AssertEquals("Trigger should be deferred. when adding new order trigger works as normal.", true,
				strategy.ShouldDeferTrigger(order));
			Factory.Save();

			var pick = Helper.CreatePickNew();
			pick.Orders.Add(order);
			AssertEquals("Trigger should be deferred.", true, strategy.ShouldDeferTrigger(order));

			Factory.Save();
			AssertEquals("Trigger should not be deferred.", false, strategy.ShouldDeferTrigger(order));
		}

		public void TestTG_WhsOrder_EnsureDDLIsEnteredOnPick_ShouldDeferTrigger_Update()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var strategy =
				ObjectFactory.Get<IWhsCheckOrderDockDoorLocationIsMatchWithPick_DeferTriggerStrategy>() as
					IDeferTriggerConditionStrategy;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew();
			Factory.Save();

			order.WD_GoodsDescription = "Something";
			AssertEquals("Trigger should not be deferred for irrelevant column.", false,
				strategy.ShouldDeferTrigger(order));

			order.WD_WP = pick.PK;
			AssertEquals("Trigger should be deferred.", true, strategy.ShouldDeferTrigger(order));

			order.WD_WP = Guid.Empty;
			AssertEquals("Trigger should not be deferred.", false, strategy.ShouldDeferTrigger(order));
		}

		#endregion

		#region TestCanNotCancelOrderWhenAttachToLoad

		public void TestCanNotCancelOrderWhenAttachToLoad()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			AssertEquals("Precondition", string.Empty, order.CanCancel());

			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation);
			order.WD_WLO_PlannedLoad = load.PK;
			AssertEquals("The order cannot be canceled while it is assigned to a Load.", order.CanCancel());

			order.WD_WLO_PlannedLoad = ZGuid.Empty;
			AssertEquals("After detach user should be able to cancel.", string.Empty, order.CanCancel());
		}

		public void TestCanNotCancelOrderWhenAttachToLoad_OrderLinkedToLoadThruPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			Factory.Save();

			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01", "CDS", startTime: DateTimeOffset.Now);
			Helper.CreatePickNew(order);
			var packageJob = order.PackageJob;
			var package = packageJob.Packages.AddNew("BOX", 1);
			Helper.CreateLoadPkgPackagePivot(package.PK, load);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			orderInNewFactory.WD_DocketStatus = DocketStatus.Codes.Entered; // for testing so it can be a "cancellable" status
			AssertEquals("Order has no planned load.", false, orderInNewFactory.WD_WLO_PlannedLoad.IsValid);
			AssertEquals("The order cannot be canceled while it is assigned to a Load.", orderInNewFactory.CanCancel());
		}

		#endregion

		public void TestPackages_CollectionCountChange_PickLineDeletedOnDataRefresh()
		{
			if (Globals.IsWeb)
			{
				// DataRefreshManager is not Enabled when Globals.IsWeb = true
				Assert(true);
			}
			else
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 30m);
				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				order.WD_RequiredDate = ZDateTimeOffset.Now;
				order.ConsigneePK = data.Org1.PK;

				var orderLine1 = Helper.CreateWhsPickableDocketLine(order, data.Part1, 10m);
				var orderLine2 = Helper.CreateWhsPickableDocketLine(order, data.Part2, 20m);

				var pick = Helper.CreatePickNew(order);
				var pickLine1 = orderLine1.PickLines.Single();
				var pickLine2 = orderLine2.PickLines.Single();

				var package1 = order.PackageJob.Packages.AddNew();
				package1.Pack(orderLine1.ReleaseLines[0], 10m);

				var package2 = order.PackageJob.Packages.AddNew();
				package2.Pack(orderLine2.ReleaseLines[0], 20m);
				Factory.Save();

				var newFactory = new BusinessObjectFactory { RefreshEnabled = true };
				var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
				var orderLine1InNewFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
				var releaseLine1 = orderLine1InNewFactory.ReleaseLines[0];
				AssertEquals("Release line collection has been poked.", true,
					orderLine1InNewFactory.IsReleaseLineCollectionBuilt);
				var orderLine2InNewFactory = newFactory.Load<WhsOrderLine>(orderLine2.PK);
				var releaseLine2 = orderLine2InNewFactory.ReleaseLines[0];
				AssertEquals("Release line collection has been poked.", true,
					orderLine2InNewFactory.IsReleaseLineCollectionBuilt);
				AssertEquals(2, orderInNewFactory.PackageJob.Packages.Count);

				pickLine2.Delete();
				Factory.Save();

				order.PackageJob.Packages.Delete(package2);
				AssertNoExceptionThrown(() => Factory.Save());
				AssertEquals("Packages", 1, orderInNewFactory.PackageJob.Packages.Count);
			}
		}

		public void TestTemplateCopy_CopyOrderFromTemplate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "Some reference", data.Part1, 10m);
			order.WD_GS_NKAssignedPacker = "123";

			var orderCopy = (WhsOrder)((ITemplateCopyable)order).TemplateCopy();

			AssertEquals("Assigned packer should not be copied when clicking copy in CW1", string.Empty, orderCopy.WD_GS_NKAssignedPacker);
		}

		#region TestDefaultUseDirectedPackingConsolidation

		public void TestDefaultUseDirectedPackingConsolidation_OnClientChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var client2 = Helper.CreateClient("2");

			var pickParams = WhsClientPickingParams.GetClientPickingParams(client2).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_UseDirectedPackingConsolidation = true;
			Factory.Save();

			AssertEquals("Precondition: WD_UseDirectedPackingConsolidation is not set.", false, order.WD_UseDirectedPackingConsolidation);
			AssertEquals("Precondition: ClientPickingParams is null.", null, order.ClientPickingParams);

			order.WD_OH_Client = client2.PK;
			AssertEquals("ClientPickingParams is not null.", pickParams, order.ClientPickingParams);
			AssertEquals("WD_UseDirectedPackingConsolidation is defaulted from client picking params.", true, order.WD_UseDirectedPackingConsolidation);

			order.WD_UseDirectedPackingConsolidation = false;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			AssertEquals(false, orderInNewFactory.WD_UseDirectedPackingConsolidation);
		}

		public void TestDefaultUseDirectedPackingConsolidation_OnWarehouseChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var warehouse = Helper.CreateWarehouse("2");

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = warehouse.PK;
			pickParams.WPP_UseDirectedPackingConsolidation = true;
			Factory.Save();

			AssertEquals("Precondition: WD_UseDirectedPackingConsolidation is not set.", false, order.WD_UseDirectedPackingConsolidation);
			AssertEquals("Precondition: ClientPickingParams is null.", null, order.ClientPickingParams);

			order.WD_WW_Whs = warehouse.PK;
			AssertEquals("ClientPickingParams is not null.", pickParams, order.ClientPickingParams);
			AssertEquals("WD_UseDirectedPackingConsolidation is defaulted from client picking params.", true, order.WD_UseDirectedPackingConsolidation);

			order.WD_UseDirectedPackingConsolidation = false;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			AssertEquals(false, orderInNewFactory.WD_UseDirectedPackingConsolidation);
		}

		public void TestDefaultUseDirectedPackingConsolidation_OnSalesChannelChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");

			var salesChannel = Helper.CreateWhsSalesChannel("ECO", "eCommerce");
			var pickParams1 = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams1.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams1.WPP_WSH_SalesChannel = salesChannel.PK;
			pickParams1.WPP_UseDirectedPackingConsolidation = true;

			var pickParams2 = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams2.WPP_WW_Warehouse = data.Whs1.PK;
			Factory.Save();

			order.WD_WSH_SalesChannel = salesChannel.PK;
			AssertEquals("ClientPickingParams is not null.", pickParams1, order.ClientPickingParams);
			AssertEquals("WD_UseDirectedPackingConsolidation is defaulted from client picking params.", true, order.WD_UseDirectedPackingConsolidation);

			order.WD_WSH_SalesChannel = ZGuid.Empty;
			AssertEquals("ClientPickingParams is not null.", pickParams2, order.ClientPickingParams);
			AssertEquals("WD_UseDirectedPackingConsolidation is defaulted from client picking params.", false, order.WD_UseDirectedPackingConsolidation);

			order.WD_UseDirectedPackingConsolidation = true;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderInNewFactory = newFactory.Load<WhsOrder>(order.PK);
			AssertEquals(true, orderInNewFactory.WD_UseDirectedPackingConsolidation);
		}

		#endregion

		#region TestUpdateWP_CriticalChangesVersionID

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_WD_IsInwardsProcessingJob()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) => docket.WD_IsInwardsProcessingJob = true, true, () =>
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.EnableWarehouseForBond(data.Whs1, true);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
				receive.WD_DocketSubType = "CUS";
				var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, "BEK-1");
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var docket = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				docket.WD_DocketSubType = "CUS";
				docket.WD_OH_Client = data.Org1.PK;
				docket.WD_WW_Whs = data.Whs1.PK;
				docket.WD_RequiredDate = ZDateTimeOffset.Now;
				docket.ConsigneeAddressPK = data.Org1.MainAddress.PK;
				var line = Helper.CreateWhsPickableDocketLine(docket, data.Part1, 2m);
				line.CustomsData.WB_EntryKey = "OUTWARDS";
				line.CustomsData.WB_EntryLineNo = 1;

				var pick = Helper.CreatePickNew(docket);
				Factory.Save();
				AssertEquals("Precondition - Pick is not picking.", true, docket.IsAttachedToPickButNotFinalised);

				return (docket, pick);
			});
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_WD_ExcludeFromTotePicking()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) => docket.WD_ExcludeFromTotePicking = true, true);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_WD_GS_NKAssignedPacker()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) => docket.WD_GS_NKAssignedPacker = "AAA", true);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_WD_IsLoadingRequired()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) => docket.WD_IsLoadingRequired = true, true);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_WD_OrderClassification()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) => docket.WD_OrderClassification = "ECO", true);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_WD_P9_PackingTask()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) => docket.WD_P9_PackingTask = Helper.CreateProcessTaskForDirectedPackingJob(docket.Pick, GlbStaff.CurrentUser).PK, true);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_WD_PackingAfterPickingRequired()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) => docket.WD_PackingAfterPickingRequired = true, true);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_WD_ScreeningStatus()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) => docket.WD_ScreeningStatus = "REQ", true);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_WD_UseDirectedPackingConsolidation()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) => docket.WD_UseDirectedPackingConsolidation = true, true);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_WD_WhsOrderFulfillmentRule()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) => docket.WD_WhsOrderFulfillmentRule = "AAA", true);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_WD_WLO_PlannedLoad()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) => docket.WD_WLO_PlannedLoad = Helper.CreateWhsLoad(Helper.CreateClient("TRANSPORT"), docket.Warehouse.DefaultOutboundDockDoorLocation).PK, true);
		}

		public void TestUpdateWP_CriticalChangesVersionID_ExceptionFields_WD_WSH_SalesChannel()
		{
			AssertUpdateWP_CriticalChangesVersionID_ExceptionFieldsCore((docket) => docket.WD_WSH_SalesChannel = Helper.CreateWhsSalesChannel("SAL", "Sales").PK, true);
		}

		#endregion

		#region TestDocketUpdatedByDataRefresh

		protected override void TestDocketUpdatedByDataRefresh_FinalisedInMemoryCore()
		{
			Assert(true);
		}

		protected override void TestDocketUpdatedByDataRefresh_CriticalChangesVersionIDUpdateCore()
		{
			Assert(true);
		}

		#endregion

		#region Implementation

		protected override void AssertDocketLineEqualsInventory(WhsInventoryView inventory, WhsDocketLine line)
		{
			base.AssertDocketLineEqualsInventory(inventory, line);

			AssertEquals("WE_TransactionQuantity", inventory.WI_AvailableToPickQuantity, line.WE_TransactionQuantity);
		}

		protected override void AssertDocketLineEqualsInventory_CustomsData(
			WhsBondedWarehouseAttribute inventoryCustomsData, WhsBondedWarehouseAttribute docketlineCustomsData)
		{
			AssertEquals("CustomsData.WB_EntryLineNo, Should always copied if docket support customs type.",
				inventoryCustomsData.WB_EntryLineNo, docketlineCustomsData.WB_EntryLineNo);
			AssertEquals("CustomsData.WB_EntryKey, Should not be copied for dockets of type Order.", "",
				docketlineCustomsData.WB_EntryKey);
			AssertEquals("CustomsData.WB_AddInfo", inventoryCustomsData.WB_AddInfo, docketlineCustomsData.WB_AddInfo);

			// We need to Copy the Transaction Qty into WB_BondedWhsQty as it will be used by customs to calculate the VFD (i.e. Ordered Qty / Transaction Qty * Total VFD)
			AssertEquals("CustomsData.WB_BondedWhsQty", inventoryCustomsData.Parent.WE_TransactionQuantity,
				docketlineCustomsData.WB_BondedWhsQty);
			// Do not copy the WB_BondedWhsQty from the Inventory, it's currently never set except in Old Bonded code.
			AssertNotEquals("CustomsData.WB_BondedWhsQty", inventoryCustomsData.WB_BondedWhsQty,
				docketlineCustomsData.WB_BondedWhsQty);
			AssertEquals("CustomsData.WB_BondedWhsUnitOfQty", inventoryCustomsData.WB_BondedWhsUnitOfQty,
				docketlineCustomsData.WB_BondedWhsUnitOfQty);
			AssertEquals("CustomsData.WB_CustomsQty", inventoryCustomsData.WB_CustomsQty,
				docketlineCustomsData.WB_CustomsQty);
			AssertEquals("CustomsData.WB_CustomsUnitOfQty", inventoryCustomsData.WB_CustomsUnitOfQty,
				docketlineCustomsData.WB_CustomsUnitOfQty);
			AssertEquals("CustomsData.WB_DeclarationReference", inventoryCustomsData.WB_DeclarationReference,
				docketlineCustomsData.WB_DeclarationReference);
			AssertEquals("CustomsData.WB_CustomsDeadline", inventoryCustomsData.WB_CustomsDeadline,
				docketlineCustomsData.WB_CustomsDeadline);
			AssertEquals("CustomsData.WB_InwardStyle", inventoryCustomsData.WB_InwardStyle,
				docketlineCustomsData.WB_InwardStyle);
			AssertEquals("CustomsData.WB_InwardProcedure", inventoryCustomsData.WB_InwardProcedure,
				docketlineCustomsData.WB_InwardProcedure);
			AssertEquals("CustomsData.WB_EntryDate", inventoryCustomsData.WB_EntryDate,
				docketlineCustomsData.WB_EntryDate);
			AssertEquals("CustomsData.WB_IsActive", inventoryCustomsData.WB_IsActive,
				docketlineCustomsData.WB_IsActive);
			AssertEquals("CustomsData.WB_ParentTableCode", "WE", docketlineCustomsData.WB_ParentTableCode);
			AssertEquals("CustomsData.WB_RN_NKCountryOfOrigin", inventoryCustomsData.WB_RN_NKCountryOfOrigin,
				docketlineCustomsData.WB_RN_NKCountryOfOrigin);
			AssertEquals("CustomsData.WB_RX_NKTILVCurrency", inventoryCustomsData.WB_RX_NKTILVCurrency,
				docketlineCustomsData.WB_RX_NKTILVCurrency);
			AssertEquals("CustomsData.WB_TILV", inventoryCustomsData.WB_TILV, docketlineCustomsData.WB_TILV);
			AssertEquals("CustomsData.WB_ValueForDuty", inventoryCustomsData.WB_ValueForDuty,
				docketlineCustomsData.WB_ValueForDuty);
			AssertEquals("CustomsData.WB_WB_InwardsEntry", inventoryCustomsData.WB_WB_InwardsEntry,
				docketlineCustomsData.WB_WB_InwardsEntry);
			AssertEquals("CustomsData.WB_PrimaryPreference", inventoryCustomsData.WB_PrimaryPreference,
				docketlineCustomsData.WB_PrimaryPreference);
			AssertEquals("CustomsData.WB_CustomsSecondQuantity", inventoryCustomsData.WB_CustomsSecondQuantity,
				docketlineCustomsData.WB_CustomsSecondQuantity);
			AssertEquals("CustomsData.WB_CustomsSecondUnitQty", inventoryCustomsData.WB_CustomsSecondUnitQty,
				docketlineCustomsData.WB_CustomsSecondUnitQty);
			AssertEquals("CustomsData.WB_CustomsThirdQuantity", inventoryCustomsData.WB_CustomsThirdQuantity,
				docketlineCustomsData.WB_CustomsThirdQuantity);
			AssertEquals("CustomsData.WB_CustomsThirdUnitQty", inventoryCustomsData.WB_CustomsThirdUnitQty,
				docketlineCustomsData.WB_CustomsThirdUnitQty);
			AssertEquals("CustomsData.WB_Tariff", inventoryCustomsData.WB_Tariff, docketlineCustomsData.WB_Tariff);
			AssertEquals("CustomsData.WB_OA_ManufacturerAddress", inventoryCustomsData.WB_OA_ManufacturerAddress,
				docketlineCustomsData.WB_OA_ManufacturerAddress);
		}

		protected virtual Type GetExpectedDocAddressValidationType()
		{
			return typeof(WhsOrderDocAddressValidation);
		}

		protected virtual Type GetExpectedUSDocAddressValidationType()
		{
			return typeof(US.WhsOrderDocAddressValidation);
		}

		protected override DataContextType? ExpectedDataContextType => DataContextType.WarehouseOrder;

		protected override ControllerID ExpectedControllerId
		{
			get { return ControllerIDs.WhsOrder; }
		}

		protected override ZString ExpectedDescription
		{
			get { return "Order"; }
		}

		protected WhsOrder Order
		{
			get { return Docket; }
			set { base.Docket = value; }
		}

		#region TestWhsOrderIPickForm

		class TestWhsOrderIPickForm : IPickForm
		{
			public TestWhsOrderIPickForm()
			{
			}

			public TestWhsOrderIPickForm(WhsOrder order)
			{
				this.order = order;
			}

			public WhsOrder CurrentOrder
			{
				get { return order; }
			}

			public List<WhsPickableDocketLine> SelectedOrderLines
			{
				get
				{
					return OrderLinesSelection ?? new List<WhsPickableDocketLine>(order.Lines.ToArray<WhsPickableDocketLine>());
				}
			}

			public List<WhsPickableDocketLine> OrderLinesSelection
			{
				get { return selectedOrderLines; }
				set { selectedOrderLines = value; }
			}

			List<WhsPickableDocketLine> selectedOrderLines;
			readonly WhsOrder order;
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			UnitTestUserNotification.Instance.ClearUserResponses();
		}

		protected override bool CanDeleteOverride
		{
			get { return false; }
		}

		protected override MultilingualString CantDeleteReasonMsg
		{
			get
			{
				return (NoResString)@"Orders cannot be deleted.";
			}
		}

		protected override WhsOrder GetDocketForRating()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			return Helper.CreateWhsOrder(data.Org1, data.Whs1);
		}

		#endregion

		#region OutboundLocationTest
		public void TestOutboundLocation_JustEnteredOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			AssertEquals("Order Status is Entered.", DocketStatus.Codes.Entered, order.WarehouseOrderStatus);
			AssertEquals("Just entered order should not have outbound location", string.Empty, order.OutboundLocation);
		}

		public void TestOutboundLocation_AttachedToPickOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertEquals("Order status is AttachedToPick.", DocketStatus.Codes.AttachedToPick, order.WarehouseOrderStatus);
			AssertEquals("AttachedToPick order should not have outbound location", string.Empty, order.OutboundLocation);
		}

		public void TestOutboundLocation_StagedOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var pickLine = order.Lines.Single().PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Order status is staged", WhsOrderStatus.Codes.Staged, order.WarehouseOrderStatus);
			AssertEquals("Pickline's outbound location should be dockdoor", data.Whs1.DefaultOutboundDockDoorLocation.ToLocationString(), pickLine.InventoryLine.Location.WLV_LocationString_UserFriendly);
			AssertEquals("Order's outbound location should be dockdoor", data.Whs1.DefaultOutboundDockDoorLocation.ToLocationString(), order.OutboundLocation);
		}

		public void TestOutboundLocation_ReadyToPackOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = order.Lines[0].PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Order should be Ready To Pack.", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);
			AssertEquals("Pickline's outbound location should be packing station", packingLocation.ToLocationString(), pickLine.InventoryLine.Location.WLV_LocationString_UserFriendly);
			AssertEquals("Order's outbound location should be the packing station", packingLocation.ToLocationString(), order.OutboundLocation);
		}

		public void TestOutboundLocation_LoadingOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);

			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			var pickLine = order.Lines[0].PickLines.Single();

			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = order.PackageJob.Packages.AddNew();
			var package2 = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);

			Factory.Save();

			AssertEquals("Order Status is Loading.", WhsOrderStatus.Codes.Loading, order.WarehouseOrderStatus);
			AssertEquals("Pickline's outbound location should be dockdoor", data.Whs1.DefaultOutboundDockDoorLocation.ToLocationString(), pickLine.InventoryLine.Location.WLV_LocationString_UserFriendly);
			AssertEquals("Order's outbound location should be dockdoor", data.Whs1.DefaultOutboundDockDoorLocation.ToLocationString(), order.OutboundLocation);
		}

		public void TestOutboundLocation_LoadedOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			var pickLine = order.Lines[0].PickLines.Single();
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Order Status is Loaded.", WhsOrderStatus.Codes.Loaded, order.WarehouseOrderStatus);
			AssertEquals("Pickline's outbound location should be dockdoor", data.Whs1.DefaultOutboundDockDoorLocation.ToLocationString(), pickLine.InventoryLine.Location.WLV_LocationString_UserFriendly);
			AssertEquals("Order's outbound location should be dockdoor", data.Whs1.DefaultOutboundDockDoorLocation.ToLocationString(), order.OutboundLocation);
		}

		public void TestOutboundLocation_DepartedOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 10m);

			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			orderLine.PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			var package1 = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			Helper.DepartPackageNow(loadPkgPackagePivot1);

			Factory.Save();

			AssertEquals("Order Status is Departed.", WhsOrderStatus.Codes.Departed, order.WarehouseOrderStatus);
			AssertEquals("Departed order should not have outbound location", string.Empty, order.OutboundLocation);
		}

		public void TestOutboundLocation_OrderInMultipleOutboundLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var packingStationLocationType = Helper.CreateLocationType("PST", "Packing", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "PST");
			var packingStationLocation = newRow.Locations[0];
			packingStationLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var consolidationLocationType = Helper.CreateLocationType("CON", "Consolidate", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "CON", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);

			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingStationLocation.PK;

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = consolidationLocation.PK;

			transferLine1.FinaliseDocketLine();
			transferLine2.FinaliseDocketLine();

			Factory.Save();

			AssertEquals("Order's outbound location should be consolidation and packing station", "CON, PST", order.OutboundLocation);
		}
		#endregion
	}

	#endregion

	#region WhsOrderProcessTasksProviderTest

	[TestedType(typeof(WhsOrder))]
	public class WhsOrderProcessTasksProviderTest : WhsDocketProcessTasksProviderTest<WhsOrder>
	{
		public void TestGetTemplateFilterCriteria_SalesChannel()
		{
			var warehouse2 = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse2.RelatedCompanyBranch.GB_GC = Env.CurrentCompanyPK;

			var salesChannel = Factory.New<WhsSalesChannel>();
			salesChannel.WSH_Code = "SCH";
			salesChannel.WSH_Description = "SCH";

			var order = BusinessObject;
			order.WD_WSH_SalesChannel = salesChannel.PK;
			Factory.Save();

			AssertGetTemplateFilterCriteria<ZString>(order.SalesChannel.WSH_CodeInfo, ProcessTaskTemplate.P0_SubType1Info, "ERT", "DFG", ZString.Empty);
		}

		public void TestGetTemplateFilterCriteria_ColumnRanksCorrect()
		{
			var warehouse2 = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse2.RelatedCompanyBranch.GB_GC = Env.CurrentCompanyPK;

			var salesChannel = Factory.New<WhsSalesChannel>();
			salesChannel.WSH_Code = "SCH";
			salesChannel.WSH_Description = "SCH";

			var order = BusinessObject;
			order.WD_WSH_SalesChannel = salesChannel.PK;
			Factory.Save();

			var ranker = ((IWorkflowProvider)order).GetTemplateSelectionCriteria();
			var columnValues = ((IColumnValueRankerInternals)ranker).ColumnValues.ToArray();
			AssertArrayEqualsByElements(
			new[] {
					$"P0_OH_Client - {order.WD_OH_Client}, 00000000-0000-0000-0000-000000000000",
					$"P0_WW - {order.WD_WW_Whs}, 00000000-0000-0000-0000-000000000000",
					"P0_SubType1 - SCH, "
				},
				columnValues.Select(cv => $"{cv.ColumnName} - {string.Join(", ", cv.Values)}").ToArray());
		}

		protected override ZString GetExpectedWorkflowType() => JobInvoicingConsumerTypes.WarehouseOutwards.Code;

		protected override WhsOrder GetNewDocket()
		{
			NewRefNumber++;
			return Helper.CreateWhsOrder(Client, Warehouse, "Ref1" + NewRefNumber.ToString());
		}
	}

	#endregion

	#region WhsOrderRelatableActivityTest

	[TestedType(typeof(WhsOrder))]
	public class WhsOrderRelatableActivityTest : RelatableActivityTestCase<WhsOrder>
	{
		protected override WhsOrder GetNewActivity()
		{
			return Factory.NewWithValidTestData<WhsOrder>();
		}
	}

	#endregion

	public class WhsOrderDocumentSupporterCartageAdviceHandlerTest : DocumentSupporterCartageAdviceHandlerTest
	{
		protected override IDocumentSupportable GetDocumentSupporterParent()
		{
			var whs = Helper.CreateWarehouse("1", "A");
			var org = Helper.CreateClient();
			var order = Helper.CreateWhsOrder(org, whs);
			return order;
		}

		protected override IDocumentSupportable[] GetBookingsFromBookingsProperty(DocumentSupporter documentSupporter)
		{
			return ((WhsOrderDocumentSupporter)documentSupporter).Bookings;
		}

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		protected override bool ChecksChildMenuItem => true;

		protected override bool DocumentSupportablesUseDifferentFactory => true;
	}

	#region WhsOrderICartageTest

	/// <summary>
	///     Tests the WhsOrder.ICartageParent + WhsOrder.ICartageLooseCargo + ITransportJobLinkProvider implementation,
	///     and is a separate testcase so that WhsPick can reuse it.
	/// </summary>
	public class WhsOrderICartageTest : WhsTestCaseWithFactory
	{
		#region TestICartageParent

		public void TestICartageParent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_PackagesSent = 7;
			order.WD_PalletsSent = 5;
			order.WD_CubicSent = 30;
			order.WD_TotalCubicUnit = Constants.Volume.Litre;
			order.WD_WeightSent = 3;
			order.WD_TotalWeightUnit = Constants.Weight.Pounds;

			ICartageParent cartageParent = order;
			AssertEquals(ZGuid.Empty, cartageParent.BranchPK);
			AssertEquals(order.PK, cartageParent.CartageParentID);
			AssertEquals(WhsDocketSchema.Constants.Prefix, cartageParent.CartageParentTableCode);
			AssertEquals(ControllerIDs.WhsOrder, cartageParent.ControllerID);

			var cartageTypes = cartageParent.CartageTypes;
			AssertEquals(1, cartageTypes.Count);
			AssertEquals(typeof(WhsOrder.WhsCartageType), cartageTypes.First().GetType());
			AssertNotEquals("Cartage Types should not be cached.", cartageTypes.First(),
				cartageParent.CartageTypes.First());

			var localCartageType = cartageParent.GetLocalCartageType;
			AssertEquals(typeof(WhsOrder.WhsCartageType), localCartageType.GetType());
			AssertNotEquals("Local Cartage Type should not be cached.", localCartageType,
				cartageParent.GetLocalCartageType);

			order.WD_GoodsDescription = "Guitars";
			AssertEquals("Guitars", cartageParent.GoodsDescription);

			order.WD_GoodsDescription = "";
			AssertEquals("5 Pallets", cartageParent.GoodsDescription);

			// JobHeaderPK
			AssertEquals(ZGuid.Empty, cartageParent.JobHeaderPK);
			var jobHeader = new Job.Loader(order).TryCreate();
			AssertEquals(jobHeader.PK, cartageParent.JobHeaderPK);

			AssertEquals(order.Client.MainAddress.PK, cartageParent.LocalClientAddressPK);
			AssertEquals(order.WD_ExternalReference, cartageParent.OrderReferenceNumber);
			AssertEquals(order.WD_PL_NKCarrierServiceLevel, cartageParent.ServiceLevel);

			AssertEquals(Constants.PkgUnit.Pallet, cartageParent.TotalPackType);
			AssertEquals(order.WD_PalletsSent, cartageParent.TotalPackages);
			AssertEquals(order.WD_CubicSent, cartageParent.TotalVolume);
			AssertEquals(order.WD_TotalCubicUnit, cartageParent.TotalVolumeUnit);
			AssertEquals(order.WD_WeightSent, cartageParent.TotalWeight);
			AssertEquals(order.WD_TotalWeightUnit, cartageParent.TotalWeightUnit);

			AssertEquals(order.WD_DocketID, cartageParent.UniqueConsignmentID);
			AssertEquals(order.WD_BOLNo, cartageParent.WayBillNumber);
			AssertEquals(true, cartageParent.RebuildLocalCartageMenuOnClick);
		}

		#endregion

		#region TestITransportJobLinkProvider_Event_PortTransport

		public void TestITransportJobLinkProvider_Event_PortTransport()
		{
			bool cartageJobCreatedAndSavedFired = false;

			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);

			Helper.CreateCartageJob(order);
			var pick = Factory.New<WhsPick>();
			pick.PickOrdersWithAllocationMock(order);
			var transportLinkProvider = GetTransportLinkProvider(order); // Order or Pick

			transportLinkProvider.TransportJobCreatedAndSaved += delegate
			{
				cartageJobCreatedAndSavedFired = true;
			};

			AssertNotNull(transportLinkProvider.TransportJobResult);
			AssertEquals(order.CartageJob, transportLinkProvider.TransportJobResult.TransportJob);

			((ICartageParent)transportLinkProvider)
				.CartageCreatedAndSaved(); // in production internal cartage manager calls this method on first CartageJob save
			AssertEquals(true, cartageJobCreatedAndSavedFired);
		}

		#endregion

		#region TestITransportJobLinkProvider_Event_TransportBooking

		public void TestITransportJobLinkProvider_Event_TransportBooking()
		{
			// TBs created on release screen use the Order's interface, event hooked on form must propagate to CurrentOrder
			bool transportJobCreatedOrUpdated = false;
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);

			var pick = Factory.New<WhsPick>();
			pick.PickOrdersWithAllocationMock(order);
			var transportLinkProvider = GetTransportLinkProvider(order); // Order or Pick

			transportLinkProvider.TransportJobCreatedAndSaved += delegate
			{
				transportJobCreatedOrUpdated = true;
			};
			((IDtbBookingParent)order)
				.TransportBookingCreatedOrUpdated(); // in production TB Delivery manager calls this method after import
			AssertEquals(true, transportJobCreatedOrUpdated);
		}

		#endregion

		#region TestITransportJobLinkProvider_Parent

		public void TestITransportJobLinkProvider_Parent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var transportLinkProvider = ((ITransportJobLinkProvider)order); // Order or Pick

			AssertNull(transportLinkProvider.TransportJobResult.TransportJob);
			AssertNull(transportLinkProvider.TransportJobResult.ReasonForEmptyOverride);

			// First Standalone TB
			var consol1 = Factory.New<IDtbBookingConsolidation>();
			consol1.KB_ParentID = order.PK;
			consol1.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;
			consol1.KB_ParentTableCode = WhsDocketSchema.Constants.Prefix;

			var booking1 = Factory.New<IDtbBooking>();
			booking1.KM_KB_Booking = consol1.PK;
			booking1.KM_JobID = "Booking 1";
			Factory.Save();

			AssertEquals("Should be linked to TB", booking1.PK,
				transportLinkProvider.TransportJobResult.TransportJob.BusinessObjectPK);
			AssertNull("No overriden reason", transportLinkProvider.TransportJobResult.ReasonForEmptyOverride);
			AssertEquals("Booking 1", order.TransportJobNumber);

			// Second Standalone TB
			var consol2 = Factory.New<IDtbBookingConsolidation>();
			consol2.KB_ParentID = order.PK;
			consol2.KB_ParentTableCode = WhsDocketSchema.Constants.Prefix;

			var booking2 = Factory.New<IDtbBooking>();
			booking2.KM_KB_Booking = consol2.PK;
			booking2.KM_JobID = "Booking 2";
			Factory.Save();

			AssertNull("Should not be linked to a job as there are multiple jobs at the same fallback level",
				transportLinkProvider.TransportJobResult.TransportJob);
			AssertEquals("Multiple Jobs", transportLinkProvider.TransportJobResult.ReasonForEmptyOverride);
			AssertEquals("Multiple Jobs", order.TransportJobNumber);

			// Land Transport (Consignment)
			var consignmentConsol1 = Factory.New<IDtbConsignmentConsolidation>();
			consignmentConsol1.KB_ParentID = booking1.PK;
			consignmentConsol1.KB_ParentTableCode = DtbBookingSchema.Constants.Prefix;

			var consignment1 = Factory.New<IDtbBookingConsignment>();
			consignment1.KM_KB_Booking = consignmentConsol1.PK;
			consignment1.KM_JobID = "Consignment 1";
			Factory.Save();

			AssertNull("There should still be no linked TransportJob.", transportLinkProvider.TransportJobResult.TransportJob);
			AssertEquals("Multiple Jobs", transportLinkProvider.TransportJobResult.ReasonForEmptyOverride);
			AssertEquals("Multiple Jobs", order.TransportJobNumber);

			// Create Port Transport Job via TB
			var portTransportViaTB1 = Factory.New<ICommonCartage>();
			portTransportViaTB1.JJ_ParentID = booking1.PK;
			portTransportViaTB1.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			Factory.Save();

			AssertEquals("Should now be linked to the Port Transport Job", portTransportViaTB1.PK,
				transportLinkProvider.TransportJobResult.TransportJob.BusinessObjectPK);
			AssertNull("No overriden reason", transportLinkProvider.TransportJobResult.ReasonForEmptyOverride);
			AssertEquals(portTransportViaTB1.JJ_ConsignmentID, order.TransportJobNumber);

			// Create Another Port Transport Job via TB
			var portTransportViaTB2 = Factory.New<ICommonCartage>();
			portTransportViaTB2.JJ_ParentID = booking2.PK;
			portTransportViaTB2.JJ_ParentTableCode = DtbBookingSchema.Constants.Prefix;
			Factory.Save();

			AssertNull("Should not be linked to a job as there are multiple jobs at the same fallback level",
				transportLinkProvider.TransportJobResult.TransportJob);
			AssertEquals("Multiple Jobs", transportLinkProvider.TransportJobResult.ReasonForEmptyOverride);

			// Create a Port Transport Directly
			var portTransportDirect1 = Helper.CreateCartageJob(order);
			Factory.Save();

			AssertEquals("Should now be linked to the Port Transport Job", portTransportDirect1.PK,
				transportLinkProvider.TransportJobResult.TransportJob.BusinessObjectPK);
			AssertNull("No overriden reason", transportLinkProvider.TransportJobResult.ReasonForEmptyOverride);
			AssertEquals(portTransportDirect1.JJ_ConsignmentID, order.TransportJobNumber);

			// Create Another Port Transport Directly
			var portTransportDirect2 = Helper.CreateCartageJob(order);
			portTransportDirect2.JJ_ConsignmentID = "PT2";
			Factory.Save();

			AssertNull("Should not be linked to a job as there are multiple jobs at the same fallback level",
				transportLinkProvider.TransportJobResult.TransportJob);
			AssertEquals("Multiple Jobs", transportLinkProvider.TransportJobResult.ReasonForEmptyOverride);
		}

		#endregion

		#region TestICartageLooseCargo

		public void TestICartageLooseCargo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			order.WD_PalletsSent = 7;
			order.WD_TotalCubicUnit = Core.Constants.Volume.Litre;
			order.WD_TotalWeightUnit = Core.Constants.Weight.Pounds;
			order.WD_CubicSent = 20.7m;
			order.WD_WeightSent = 13.8m;
			var cartageLooseCargo = (ICartageLooseCargo)order;

			AssertEquals("M", cartageLooseCargo.BookedDimensionUnit);
			AssertEquals(0m, cartageLooseCargo.BookedHeight);
			AssertEquals(0m, cartageLooseCargo.BookedLength);
			AssertEquals(0m, cartageLooseCargo.BookedWidth);

			AssertEquals(7, cartageLooseCargo.BookedPackages);
			AssertEquals(Core.Constants.PkgUnit.Pallet, cartageLooseCargo.BookedPackType);

			AssertEquals(20.7m, cartageLooseCargo.BookedVolume);
			AssertEquals(Core.Constants.Volume.Litre, cartageLooseCargo.BookedVolumeUnit);

			AssertEquals(13.8m, cartageLooseCargo.BookedWeight);
			AssertEquals(Core.Constants.Weight.Pounds, cartageLooseCargo.BookedWeightUnit);

			AssertEquals(0, cartageLooseCargo.DangerousGoods.Count);
		}

		#endregion

		#region IDtbBookingParent Members

		public void TestIDtbBookingParent()
		{
			var order = Factory.New<WhsOrder>();
			var dtbBookingParent = order as IDtbBookingParent;

			order.WD_DocketID = "W00000123";
			order.WD_BOLNo = "456789";
			order.WD_GoodsDescription = "Pallet of Books";
			AssertEquals("Warehouse Order", dtbBookingParent.JobTypeDescription);
			AssertEquals("W00000123", dtbBookingParent.JobNumber);
			AssertEquals(WorkflowDescriptors.WhsOrderWorkflowDescriptorCode, dtbBookingParent.JobType);

			AssertContainsExactElementsInAnyOrder(new DtbBookingDirection[] { DtbBookingDirection.DLV },
				dtbBookingParent.GetSupportedDirections());
		}

		public void TestIDtbBookingParent_ControllerID()
		{
			var order = Factory.New<WhsOrder>();
			var dtbBookingParent = order as IDtbBookingParent;
			AssertEquals(ControllerIDs.WhsOrder, dtbBookingParent.ControllerID);
		}

		public void TestCanCreateTransportBooking()
		{
			var order = Factory.New<WhsOrder>();
			IDtbBookingParent dtbBookingParent = order;
			AssertEquals("CanCreateTransportBooking should always return true", true, dtbBookingParent.CanCreateTransportBooking);
		}

		public void TestBookingParentPK()
		{
			var order = Factory.New<WhsOrder>();
			IDtbBookingParent dtbBookingParent = order;
			AssertEquals("BookingParentPK should be the Order PK.", order.PK, dtbBookingParent.BookingParentPK);
		}

		public void TestBookingParentTablePrefix()
		{
			var order = Factory.New<WhsOrder>();
			IDtbBookingParent dtbBookingParent = order;
			AssertEquals("BookingParentTablePrefix should be the Order table prefix.", order.TablePrefix, dtbBookingParent.BookingParentTablePrefix);
		}

		public void TestGetExtendingConfirmMessageBeforeCreateTransportBooking()
		{
			var order = Factory.New<WhsOrder>();
			IDtbBookingParent dtbBookingParent = order;

			var (isShouldShow, caption, message, confirmation) = dtbBookingParent.GetExtendingConfirmMessageBeforeCreateTransportBooking();
			AssertEquals("IsShouldShow should return false.", false, isShouldShow);
			AssertNullOrEmpty("Caption should be null.", caption);
			AssertNullOrEmpty("Message should be null.", message);
			AssertNullOrEmpty("Confirmation should be null.", confirmation);
		}

		#endregion

		#region Implementation

		protected virtual ITransportJobLinkProvider GetTransportLinkProvider(WhsOrder order)
		{
			return order;
		}

		#endregion
	}

	#endregion

	#region WhsOrderICartageParentTestCase

	[TestedType(typeof(WhsOrder))]
	public class WhsOrderICartageParentTestCase : ICartageParentTestCase
	{
		#region GetNewParent

		protected override ICartageParent GetNewParent()
		{
			return Factory.New<WhsOrder>();
		}

		#endregion

		#region SupportsMultipleCartages

		protected override ZBool SupportsMultipleCartages
		{
			get { return false; }
		}

		#endregion
	}

	#endregion

	#region WhsOrderIDtbBookingParentTestCase

	[TestedType(typeof(WhsOrder))]
	public class WhsOrderIDtbBookingParentTestCase : IDtbBookingParentTestCase<WhsOrder>
	{
		#region TestITransportJobLinkProvider_Event

		public void TestITransportJobLinkProvider_Event()
		{
			bool transportJobCreatedAndSavedFired = false;

			var order = GetNewParent();
			var transportLinkProvider = ((ITransportJobLinkProvider)order); // Order or Pick
			transportLinkProvider.TransportJobCreatedAndSaved += delegate
			{
				transportJobCreatedAndSavedFired = true;
			};

			((IDtbBookingParent)order).TransportBookingCreatedOrUpdated(); // in production called by DtbDeliveryManager
			AssertEquals(true, transportJobCreatedAndSavedFired);
		}

		#endregion

		protected override WhsOrder GetNewParent()
		{
			return Factory.NewWithValidTestData<WhsOrder>();
		}

		protected override bool IsWorkflowDescriptorSupportsCreateTransportBooking()
		{
			return true;
		}

		protected override bool CanHaveDirectCartageChild => true;
	}

	#endregion

	#region WhsOrderTriggerTest

	public class WhsOrderTriggerTest : WhsPickableDocketTriggerTest<WhsOrder>
	{
		#region TestTG_WhsDocket_PreventWarehouseChange

		protected override void TestTG_WhsDocket_PreventWarehouseChangeCore()
		{
			var otherWhs = Helper.CreateWarehouse("WH2", "A", 1, 1);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var order = GetNewDocket(data.Org1, data.Whs1);
			Factory.Save();

			var originalWarehousePK = order.WD_WW_Whs;
			order.WD_WW_Whs = otherWhs.PK;
			AssertNoExceptionThrown("Should allow changing warehouse when there are no lines.", Factory.Save);

			var orderLine = (WhsOrderLine)GetNewDocketLine(order, data.Part1, otherWhs.DefaultLocation);
			AssertNoExceptionThrown("Should allow changing warehouse when there are lines.", Factory.Save);

			order.WD_WL_CrossDock = otherWhs.WW_DefaultOutboundDockDoor;
			AssertNoExceptionThrown("Allow add Cross Dock Location", Factory.Save);

			order.WD_WW_Whs = data.Whs1.PK;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsOrder.PreventChangeOfWarehouse), "Should prevent changing Warehouse on order that has cross dock location.");
		}

		[ExpectNoExceptions]
		public void TestTG_WhsDocket_PreventWarehouseChange_WithReservedStock()
		{
			var otherWhs = Helper.CreateWarehouse("WH2", "A", 1, 1);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var order = GetNewDocket(data.Org1, data.Whs1);
			var orderLine = (WhsOrderLine)GetNewDocketLine(order, data.Part1, otherWhs.DefaultLocation);
			orderLine.ReserveStockIfAbleTo(receive.Inventory[0], 10m);
			Factory.Save();

			AssertEquals("Precondition: Reserved Stock", true, order.HasReservedStock);

			order.WD_WW_Whs = otherWhs.PK;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsOrder.PreventChangeOfWarehouse), "Should prevent changing Warehouse on order that has Reserved Stock.");
		}

		[ExpectNoExceptions]
		public void TestTG_WhsDocket_PreventWarehouseChange_HasPick()
		{
			var otherWhs = Helper.CreateWarehouse("WH2", "A", 1, 1);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var order = GetNewDocket(data.Org1, data.Whs1);
			var orderLine = (WhsOrderLine)GetNewDocketLine(order, data.Part1, otherWhs.DefaultLocation);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			AssertEquals("Precondition: Status is Attached To Pick", DocketStatus.Codes.AttachedToPick, order.WD_DocketStatus);

			order.WD_WW_Whs = otherWhs.PK;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsOrder.PreventChangeOfWarehouse), "Should prevent changing Warehouse on order when it has Pick");
		}

		#endregion

		protected override WhsOrder GetNewDocket(OrgHeader client, WhsWarehouse warehouse) => Helper.CreateWhsOrder(client, warehouse);

		protected override WhsDocketLine GetNewDocketLine(WhsOrder docket, OrgSupplierPart part, WhsLocation location)
			=> Helper.CreateWhsOrderLine(docket, part, 10);

		protected override WhsDocketLineCollection GetAllLines(WhsOrder docket) => docket.AllLines;

		protected override string StatusForFinalisedDocket => DocketStatus.Codes.Picking;

		protected override string GetDocketSubTypeNotDefault() => OrderType.Codes.RepeatOrder;
	}

	#endregion

	#region WhsOrderDocketFetchStrategyTest

	class WhsOrderDocketFetchStrategyTest : WhsPickableDocketFetchStrategyTest<WhsOrder>
	{
		protected override WhsOrder CreateDocketAndLine(OrgHeader org, WhsWarehouse warehouse, string docketId,
			OrgSupplierPart part, decimal numberOfUnits)
		{
			return Helper.CreateWhsOrderWithOrderLine(org, warehouse, docketId, part, numberOfUnits);
		}
	}

	#endregion

	#region TestConcurrencyProblemsForOrder

	public class TestConcurrencyProblemsForOrder : TestCase
	{
		#region TestTG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder

		#region Globals.IsWeb = false

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder_ProductCode()
		{
			AssertExceptionThrownWhenFactorySaving((data, orderLine) =>
			{
				orderLine.WE_OP = data.Part2.PK;
			}, false);
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder_ExpiryDate()
		{
			AssertExceptionThrownWhenFactorySaving((data, orderLine) =>
			{
				orderLine.WE_ExpiryDate = ZDate.Today;
			}, false);
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder_PackingDate()
		{
			AssertExceptionThrownWhenFactorySaving((data, orderLine) =>
			{
				orderLine.WE_PackingDate = ZDate.Today;
			}, false);
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder_Attribut1()
		{
			AssertExceptionThrownWhenFactorySaving((data, orderLine) =>
			{
				orderLine.WE_PartAttrib1 = "Attr1";
			}, false);
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder_Attribut2()
		{
			AssertExceptionThrownWhenFactorySaving((data, orderLine) =>
			{
				orderLine.WE_PartAttrib2 = "Attr2";
			}, false);
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder_Attribut3()
		{
			AssertExceptionThrownWhenFactorySaving((data, orderLine) =>
			{
				orderLine.WE_PartAttrib3 = "Attr3";
			}, false);
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder_SerialNumber()
		{
			AssertExceptionThrownWhenFactorySaving((data, orderLine) =>
			{
				orderLine.WE_SerialNumber = "SN1";
			}, false);
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder_PalletID()
		{
			AssertExceptionThrownWhenFactorySaving((data, orderLine) =>
			{
				orderLine.WE_PalletID = "ABC";
			}, isWeb: false);
		}

		#endregion

		#region Globals.IsWeb = true

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder_ProductCode_IsWeb()
		{
			AssertExceptionThrownWhenFactorySaving((data, orderLine) =>
			{
				orderLine.WE_OP = data.Part2.PK;
			}, true);
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder_ExpiryDate_IsWeb()
		{
			AssertExceptionThrownWhenFactorySaving((data, orderLine) =>
			{
				orderLine.WE_ExpiryDate = ZDate.Today;
			}, true);
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder_PackingDate_IsWeb()
		{
			AssertExceptionThrownWhenFactorySaving((data, orderLine) =>
			{
				orderLine.WE_PackingDate = ZDate.Today;
			}, true);
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder_Attribut1_IsWeb()
		{
			AssertExceptionThrownWhenFactorySaving((data, orderLine) =>
			{
				orderLine.WE_PartAttrib1 = "Attr1";
			}, true);
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder_Attribut2_IsWeb()
		{
			AssertExceptionThrownWhenFactorySaving((data, orderLine) =>
			{
				orderLine.WE_PartAttrib2 = "Attr2";
			}, true);
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder_Attribut3_IsWeb()
		{
			AssertExceptionThrownWhenFactorySaving((data, orderLine) =>
			{
				orderLine.WE_PartAttrib3 = "Attr3";
			}, true);
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder_SerialNumber_IsWeb()
		{
			AssertExceptionThrownWhenFactorySaving((data, orderLine) =>
			{
				orderLine.WE_SerialNumber = "SN1";
			}, true);
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestTG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder_PalletID_IsWeb()
		{
			AssertExceptionThrownWhenFactorySaving((data, orderLine) =>
			{
				orderLine.WE_PalletID = "123";
			}, isWeb: true);
		}

		#endregion

		#region TestTG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder

		[UseSnapshotProtection]
		public void TestTG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder()
		{
			// ensure that saving some unrelated changes does not take lock.
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Factory.Save();

			WhsTestHelperFunctions.DisablePageLocks(Db.Connection);

			Exception exception = null;
			var secondOperatorThread = new Thread(() =>
			{
				try
				{
					using (Db.DisposableActionForDbConnection())
					using (var connectionForUser2 = Db.NewExtraConnectionToMainDb())
					{
						var otherFactory2 = new BusinessObjectFactory(connectionForUser2) { RefreshEnabled = false };
						var orderInOtherFactory2 = otherFactory2.Load<WhsOrder>(order.PK);
						orderInOtherFactory2.WD_ExternalReference = "BLABLA"; // do some changes to order / order line

						try
						{
							otherFactory2.Save();
						}
						catch (Exception ex)
						{
							exception = ex;
						}
					}
				}
				catch
				{
					// to prevent CW1 from crashing
				}
			});

			var secondThreadWasBlocked = true;
			using (Db.DisposableActionForDbConnection())
			using (var connectionForUser1 = Db.NewExtraConnectionToMainDb())
			{
				var otherFactory1 = new BusinessObjectFactory(connectionForUser1) { RefreshEnabled = false };
				var orderInOtherFactory1 = otherFactory1.Load<WhsOrder>(order.PK);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				orderInOtherFactory1.Logs.AddNew(Events.EditedARecord); // add some unrelated changes to order
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

				EventHandler runSecondThread = (s, e) =>
				{
					secondOperatorThread.Start();
					secondThreadWasBlocked =
						!secondOperatorThread
							.Join(5000); // wait and ensure that second thread is not blocked (true if ended within 5 seconds, i.e. not blocked).
				};

				otherFactory1.ServiceContainer.AddAfterOnSavingService(
					new DummyAfterOnSavingBOProcessingService(otherFactory1,
						(bisOs) => runSecondThread(this, EventArgs.Empty)));

				try
				{
					otherFactory1.Save(); // pass the critical part and run second thread
					otherFactory1.ServiceContainer.RemoveAfterOnSavingService<DummyAfterOnSavingBOProcessingService>();
				}
				catch (Exception ex)
				{
					exception = ex;
				}
			}

			AssertNull("Both threads should save with no issues and no blocking.", exception);
			AssertEquals("First thread should not take any locks, as it only saves logs.", false,
				secondThreadWasBlocked);
		}

		#endregion

		delegate void ChangeCriticalField(TestDataSimpleEnvironment data, WhsOrderLine orderLine);

		void AssertExceptionThrownWhenFactorySaving(ChangeCriticalField changeCriticalField, bool isWeb)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();
			AssertEquals("OrderLine should be in Database", true, orderLine.IsInDatabase);

			using (Db.DisposableActionForDbConnection())
			using (var connectionForSecondUser = Db.NewExtraConnectionToMainDb())
			{
				var secondFactory = new BusinessObjectFactory(connectionForSecondUser) { RefreshEnabled = false };
				var orderInSecondFactory = secondFactory.Load<WhsOrder>(order.PK);
				var pick = secondFactory.New<WhsPick>();
				pick.PickOrdersWithAllocationMock(orderInSecondFactory);
				secondFactory.Save();

				var originalIsWeb = Globals.IsWeb;
				try
				{
					Globals.IsWeb = isWeb;
					var newFactory = new BusinessObjectFactory();
					if (isWeb)
					{
						var trackingOrder = newFactory.Load<WhsOrder>(order.PK);
						var trackingOrderLine = trackingOrder.Lines[0];
						changeCriticalField(data, trackingOrderLine);

						NUnit.Framework.Assert.That(newFactory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to change critical fields on picked order.", true), "Should not be able to update line if order is picked.");
					}
					else
					{
						changeCriticalField(data, orderLine);

						NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to change critical fields on picked order.", true), "Should not be able to update line if order is picked.");
					}
				}
				finally
				{
					Globals.IsWeb = originalIsWeb;
				}
			}
		}

		#endregion

		#region TestLockIsWorkingWithOrderLineProductChange

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestLockIsWorkingWithOrderLineProductChange()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var anotherProduct = Helper.CreateProduct(data.Org1, "PR2");
			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();
			AssertEquals("OrderLine should be in Database", true, orderLine.IsInDatabase);

			((IDbConnected)Factory).Connection.BeginTransaction();
			var pick = Factory.New<WhsPick>();
			pick.PickOrdersWithAllocationMock(order);
			Factory.Save();

			bool isSecondUserLocked = false;
			bool threwException = false;
			var task = Task.Factory.StartNew(() =>
			{
				using (Db.DisposableActionForDbConnection())
				using (var connectionForSecondUser = Db.NewExtraConnectionToMainDb())
				{
					var secondFactory = new BusinessObjectFactory(connectionForSecondUser) { RefreshEnabled = false };
					var orderInSecondFactory = secondFactory.Load<WhsOrder>(order.PK);

					connectionForSecondUser.BeginTransaction();
					isSecondUserLocked = true;
					try
					{
						orderInSecondFactory.Lines[0].WE_OP = data.Part2.PK;
						NUnit.Framework.Assert.That(secondFactory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to change critical fields on picked order.", true), "Should not be able to update line if order is picked.");
						isSecondUserLocked = false;
					}
					catch (Exception)
					{
						threwException = true;
					}
					finally
					{
						connectionForSecondUser.RollbackTransaction();
					}
				}
			});

			Thread.Sleep(5000);
			AssertEquals(
				"Should not have thrown exception, should have been waiting for first transaction to complete.", false,
				threwException);
			AssertEquals("Second user should be currently locked.", true, isSecondUserLocked);
			AssertEquals(true, (Factory as IDbConnected).Connection.IsInTransaction);

			((IDbConnected)Factory).Connection.CommitTransaction();
			task.Wait();
			AssertEquals(
				"Should not have thrown exception, should have been waiting for first transaction to complete.", false,
				threwException);
			AssertEquals("Second user should not be locked anymore.", false, isSecondUserLocked);
			AssertEquals(false, (Factory as IDbConnected).Connection.IsInTransaction);
		}

		#endregion

		#region Implementation

		#region Helpers

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion

		#region Factory

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory(testCaseDbConnection)); }
		}

		BusinessObjectFactory factory;

		DbConnection testCaseDbConnection;

		protected override void SetUp()
		{
			testCaseDbConnection = Db.NewExtraConnectionToMainDb();
		}

		protected override void TearDown()
		{
			testCaseDbConnection.Dispose();
			testCaseDbConnection = null;
		}

		#endregion

		#endregion
	}

	#endregion

	#region Unique Index Failure Handler

	public class DocketIDFountainUniqueIndexFailureHandlerOrder : DocketIDFountainUniqueIndexFailureHandler<WhsOrder>
	{
		protected override string GetDocketType() => DocketType.Codes.Order;
		protected override string GetDocketSubType() => OrderType.Codes.Order;
	}

	#endregion

	#region ReleaseLinesOnOrderCollectionTest

	[TestedType(typeof(WhsOrder.ReleaseLinesOnOrderCollection))]
	class ReleaseLinesOnOrderCollectionTest : NonPersistentBusinessObjectCollectionTestCase<WhsOrder.ReleaseLinesOnOrderCollection>
	{
		protected override WhsOrder.ReleaseLinesOnOrderCollection GetCollectionToTest()
		{
			return new WhsOrder.ReleaseLinesOnOrderCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var lineFactory = new BusinessObjectFactory();
			var docket = lineFactory.NewWithValidTestData<WhsPickableDocket>();
			var docketLine = docket.Lines.AddNew();
			var collection = WhsReleaseLineCollection.GetNewReleaseLinesCollection(docketLine);

			var orderLine = lineFactory.NewWithValidTestData<WhsOrderLine>();
			var releaseLine = new WhsReleaseLine(orderLine);
			collection.Add(releaseLine);

			return releaseLine;
		}

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
		}
	}

	#endregion

	#region WhsOrderLineFromInventoryTestCase

	public class WhsOrderLineFromInventoryTestCase : DocketLineFromInventoryHelperTest<WhsOrder, WhsOrderLine>
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
				new OrderLineFromInventoryHelper(new TestNotificationBuffer(), null));
		}

		#endregion

		#region TestHasProductUnitsOrAttribsChangedIsSet

		public void TestHasProductUnitsOrAttribsChangedIsSet()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");

			var inventory = receive.Lines[0].Inventory[0];

			var docketLineFromInventoryHelper = GetInventoryHelper(order);
			var hasProductUnitsOrAttribsBeenSet = false;
			var orderLine1 = order.Lines.AddNew();
			orderLine1.WE_OPInfo.ValueChanged += (sender, e) =>
				hasProductUnitsOrAttribsBeenSet |= orderLine1.Shortfall.HasProductUnitsOrAttribsChanged;
			orderLine1.WE_TransactionQuantityInfo.ValueChanged += (sender, e) =>
				hasProductUnitsOrAttribsBeenSet |= orderLine1.Shortfall.HasProductUnitsOrAttribsChanged;
			docketLineFromInventoryHelper.SetDocketLineFromInventory(orderLine1, inventory.InDocketLine,
				ExcludeFromCopy.None);
			AssertEquals("While setting orderline fields, shortfall property should have been false.", false,
				hasProductUnitsOrAttribsBeenSet);
			AssertEquals("After setting, all fields, it should be set to true.", true,
				orderLine1.Shortfall.HasProductUnitsOrAttribsChanged);

			WhsPickableDocketLine orderLine2 = null;

			using (order.ShortfallManager.DeferMarkingLinesAsShortfallPropertiesChanged())
			{
				orderLine2 =
					(WhsPickableDocketLine)docketLineFromInventoryHelper.CreateDocketLineFromInventory(order.Lines,
						inventory);
				AssertEquals(false, orderLine2.Shortfall.HasProductUnitsOrAttribsChanged);
			}

			AssertEquals(true, orderLine2.Shortfall.HasProductUnitsOrAttribsChanged);
		}

		#endregion

		#region TestShortfallManagerIsSuspendedDuringDocketLineCreation

		public void TestShortfallManagerIsSuspendedDuringDocketLineCreation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");

			var inventory = receive.Lines[0].Inventory[0];

			var assertionWasRun = false;
			var docketLineFromInventoryHelper = new DummyPickableDocketLineFromInventoryHelper(order);
			docketLineFromInventoryHelper.AssertLineDuringCreation = (dl) =>
			{
				assertionWasRun = true;
				AssertEquals("Shortfall Manager should be suspended during Docket Line creation.", true,
					order.ShortfallManager.IsMarkingLinesAsShortfallPropertiesChangedSuspended);
			};

			docketLineFromInventoryHelper.AcceptInventoryLinesFromSearchGrid(order.Lines, new[] { inventory });
			AssertEquals(true, assertionWasRun);
		}

		class DummyPickableDocketLineFromInventoryHelper : PickableDocketLineFromInventoryHelper<WhsPickableDocketLine>
		{
			public DummyPickableDocketLineFromInventoryHelper(WhsPickableDocket docket)
				: base(new TestNotificationBuffer(), docket)
			{
			}

			public Action<WhsPickableDocketLine> AssertLineDuringCreation;

			protected override void SetDocketLineFromInventoryCore(WhsPickableDocketLine transactionLine,
				WhsDocketLine inventoryLine, ExcludeFromCopy exclude)
			{
				AssertLineDuringCreation(transactionLine);

				base.SetDocketLineFromInventoryCore(transactionLine, inventoryLine, exclude);
			}
		}

		#endregion

		#region TestSetDefaultOutwardTypeIfEmpty

		public void TestSetDefaultOutwardTypeIfEmpty_CUS()
		{
			TestSetDefaultOutwardTypeIfEmpty(OrderType.Codes.Customs);
		}

		public void TestSetDefaultOutwardTypeIfEmpty_CPS()
		{
			TestSetDefaultOutwardTypeIfEmpty(OrderType.Codes.CustomsReleaseWithPermit);
		}

		public void TestSetDefaultOutwardTypeIfEmpty_ORD()
		{
			TestSetDefaultOutwardTypeIfEmpty(OrderType.Codes.Order);
		}

		void TestSetDefaultOutwardTypeIfEmpty(string docketSubType)
		{
			var whs = Helper.CreateWarehouse("Q1", "A");
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, whs, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrder(data.Org1, whs, "O1");
			order.WD_DocketSubType = docketSubType;

			var inventory = receive.Lines[0].Inventory[0];
			var orderLine = order.Lines.AddNew();
			var docketLineFromInventoryHelper = GetInventoryHelper(order);
			docketLineFromInventoryHelper.SetDocketLineFromInventory(orderLine, inventory.InDocketLine,
				ExcludeFromCopy.None);
			orderLine.CustomsData.WB_OutwardType = string.Empty;
			AssertEquals("Precondition", string.Empty, inventory.CustomsData.WB_OutwardType);
			AssertEquals("Precondition", string.Empty, orderLine.CustomsData.WB_OutwardType);

			docketLineFromInventoryHelper.SetDocketLineFromInventory(orderLine, inventory.InDocketLine,
				ExcludeFromCopy.None);
			AssertEquals("When is empty it should set defult value",
				order.IsCustomsTransaction ? WhsBondedWarehouseAttributeOutwardType.Codes.CNN : string.Empty,
				orderLine.CustomsData.WB_OutwardType);

			inventory.CustomsData.WB_OutwardType = WhsBondedWarehouseAttributeOutwardType.Codes.EXS;
			docketLineFromInventoryHelper.SetDocketLineFromInventory(orderLine, inventory.InDocketLine,
				ExcludeFromCopy.None);
			AssertEquals("Should not change if already have value",
				order.IsCustomsTransaction ? WhsBondedWarehouseAttributeOutwardType.Codes.EXS : string.Empty,
				orderLine.CustomsData.WB_OutwardType);
		}

		#endregion

		#region TestCheckOrderLineWB_EntryKeyIsNotClonedFromInventory

		public void TestCheckOrderLineWB_EntryKeyAndWB_EntryLineNoIsNotClonedFromInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			receive.Lines[0].CustomsData.WB_EntryKey = "123";
			receive.Lines[0].CustomsData.WB_EntryLineNo = 3;

			var inventory = receive.Lines[0].Inventory[0];

			var docketLineFromInventoryHelper = GetInventoryHelper(order);
			var orderLine = docketLineFromInventoryHelper.CreateDocketLineFromInventory(order.Lines, inventory);
			AssertEquals("WB_EntryKey should not be copied when setting from Inventory.", "", orderLine.CustomsData.WB_EntryKey);
			AssertEquals("WB_EntryLineNo should not be copied when setting from Inventory.", (short)0, orderLine.CustomsData.WB_EntryLineNo);
		}

		#endregion

		protected override WhsOrder GetNewDocket(OrgHeader client, WhsWarehouse warehouse)
		{
			var order = Factory.New<WhsOrder>();
			order.WD_WW_Whs = warehouse.PK;
			order.WD_OH_Client = client.PK;

			return order;
		}

		protected override WhsOrderLine GetNewDocketLine(WhsOrder docket, OrgSupplierPart part, WhsLocation location)
		{
			var orderLine = Helper.CreateWhsOrderLine(docket, part, 10m);
			orderLine.RunPreSaveValidation(); // to commit inventory

			return orderLine;
		}

		protected override void AssertLocations(WhsDocketLine line, WhsInventoryView inventory)
		{
			// Locations not used by Order
		}

		protected override void AssertDocketLineEqualsInventory_CustomsData(
			WhsBondedWarehouseAttribute inventoryCustomsData, WhsBondedWarehouseAttribute docketlineCustomsData)
		{
			AssertEquals("CustomsData.WB_BondedWhsQty", inventoryCustomsData.WB_BondedWhsQty,
				docketlineCustomsData.WB_BondedWhsQty);

			AssertEquals("CustomsData.WB_EntryLineNo, Should not be copied.", (short)0, docketlineCustomsData.WB_EntryLineNo);
			AssertEquals("CustomsData.WB_EntryKey, Should not be copied.", "", docketlineCustomsData.WB_EntryKey);
			AssertEquals("CustomsData.WB_AddInfo", inventoryCustomsData.WB_AddInfo, docketlineCustomsData.WB_AddInfo);
			AssertEquals("CustomsData.WB_BondedWhsUnitOfQty", inventoryCustomsData.WB_BondedWhsUnitOfQty, docketlineCustomsData.WB_BondedWhsUnitOfQty);
			AssertEquals("CustomsData.WB_CustomsQty", inventoryCustomsData.WB_CustomsQty, docketlineCustomsData.WB_CustomsQty);
			AssertEquals("CustomsData.WB_CustomsUnitOfQty", inventoryCustomsData.WB_CustomsUnitOfQty, docketlineCustomsData.WB_CustomsUnitOfQty);
			AssertEquals("CustomsData.WB_DeclarationReference", inventoryCustomsData.WB_DeclarationReference, docketlineCustomsData.WB_DeclarationReference);
			AssertEquals("CustomsData.WB_EntryDate", inventoryCustomsData.WB_EntryDate, docketlineCustomsData.WB_EntryDate);
			AssertEquals("CustomsData.WB_IsActive", inventoryCustomsData.WB_IsActive, docketlineCustomsData.WB_IsActive);
			AssertEquals("CustomsData.WB_ParentTableCode, Should not copied.", "WE", docketlineCustomsData.WB_ParentTableCode);
			AssertEquals("CustomsData.WB_RN_NKCountryOfOrigin", inventoryCustomsData.WB_RN_NKCountryOfOrigin, docketlineCustomsData.WB_RN_NKCountryOfOrigin);
			AssertEquals("CustomsData.WB_RX_NKTILVCurrency", inventoryCustomsData.WB_RX_NKTILVCurrency, docketlineCustomsData.WB_RX_NKTILVCurrency);
			AssertEquals("CustomsData.WB_TILV", inventoryCustomsData.WB_TILV, docketlineCustomsData.WB_TILV);
			AssertEquals("CustomsData.WB_ValueForDuty", inventoryCustomsData.WB_ValueForDuty, docketlineCustomsData.WB_ValueForDuty);
			AssertEquals("CustomsData.WB_WB_InwardsEntry", ZGuid.Empty, docketlineCustomsData.WB_WB_InwardsEntry);
			AssertEquals("CustomsData.WB_PrimaryPreference", inventoryCustomsData.WB_PrimaryPreference, docketlineCustomsData.WB_PrimaryPreference);
			AssertEquals("CustomsData.WB_CustomsSecondQuantity", inventoryCustomsData.WB_CustomsSecondQuantity, docketlineCustomsData.WB_CustomsSecondQuantity);
			AssertEquals("CustomsData.WB_CustomsSecondUnitQty", inventoryCustomsData.WB_CustomsSecondUnitQty, docketlineCustomsData.WB_CustomsSecondUnitQty);
			AssertEquals("CustomsData.WB_CustomsThirdQuantity", inventoryCustomsData.WB_CustomsThirdQuantity, docketlineCustomsData.WB_CustomsThirdQuantity);
			AssertEquals("CustomsData.WB_CustomsThirdUnitQty", inventoryCustomsData.WB_CustomsThirdUnitQty, docketlineCustomsData.WB_CustomsThirdUnitQty);
			AssertEquals("CustomsData.WB_Tariff", inventoryCustomsData.WB_Tariff, docketlineCustomsData.WB_Tariff);
			AssertEquals("CustomsData.WB_OA_ManufacturerAddress", inventoryCustomsData.WB_OA_ManufacturerAddress, docketlineCustomsData.WB_OA_ManufacturerAddress);
			AssertEquals("CustomsData.WB_IsMainInwardsProcessedItem", inventoryCustomsData.WB_IsMainInwardsProcessedItem, docketlineCustomsData.WB_IsMainInwardsProcessedItem);
			AssertEquals("CustomsData.WB_IsSecondaryInwardsProcessedItem", inventoryCustomsData.WB_IsSecondaryInwardsProcessedItem, docketlineCustomsData.WB_IsSecondaryInwardsProcessedItem);
		}

		protected override DocketLineFromInventoryHelper<WhsOrderLine> GetInventoryHelper(WhsDocket docket)
		{
			return new OrderLineFromInventoryHelper(docket?.NotificationSubscriber, (WhsOrder)docket);
		}
	}

	#endregion

	#region DeferrableTriggers_WhsOrderTest class

	[TestedType(typeof(WhsOrder))]
	class DeferrableTriggers_WhsOrderTest : DeferrableTriggerTestCase<WhsOrder>
	{
	}

	#endregion

	#region WhsOrderConversationProviderTest

	public class WhsOrderConversationProviderTest : TestCaseWithFactory
	{
		public void TestConversation()
		{
			var provider = (IConversationProvider)order;
			Factory.Save();

			var conversation = provider.eConversation;
			var expectedConversation = JobConversation.GetConversation(order);

			AssertEquals(expectedConversation.PK, conversation.PK);
		}

		public void TestConversation_NotSaved()
		{
			var provider = (IConversationProvider)order;

			AssertNull(provider.eConversation);
		}

		public void TestConversation_AlreadyExists()
		{
			var provider = (IConversationProvider)order;
			Factory.Save();

			var expectedConversation = JobConversation.GetOrCreate(order);

			var conversation = provider.eConversation;

			AssertEquals(expectedConversation.PK, conversation.PK);
		}

		public void TestParentModule()
		{
			var provider = (IConversationProvider)order;

			AssertEquals(ModuleIDs.WhsOrder, provider.ParentModule);
		}

		public void TestParentController()
		{
			var provider = (IConversationProvider)order;

			AssertEquals(ControllerIDs.WhsOrder, provider.ParentController);
		}

		public void TestAdditionalParticipants()
		{
			var provider = (IConversationProvider)order;

			AssertSequencesEqual(Enumerable.Empty<EConversation.Business.RelatedParty>(), provider.AdditionalParticipants);
		}

		public void TestSendEmailNotificationsOnSave()
		{
			var provider = (IConversationProvider)order;

			Assert(provider.SendEmailNotificationsOnSave);
		}

		public void TestEmailSubjectContentOverride()
		{
			var provider = (IConversationProvider)order;

			AssertNull(provider.EmailSubjectContentOverride);
		}

		public void TestFromAddressOverride()
		{
			var provider = (IConversationProvider)order;
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
				var provider = (IConversationAdditionalParticipantProvider)order;

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				var additionalParticipants = new List<IConversationParticipant> { staff };
				var mockParticipantProvider = new Mock<IWarehouseConversationParticipantProvider>();
				ObjectFactory.Substitute(mockParticipantProvider.Object);

				var contact = Factory.NewWithValidTestData<OrgContact>();
				var subscribedParticipants = new ReadOnlyCollection<IConversationParticipant>(new List<IConversationParticipant> { contact });
				var participant = Factory.NewWithValidTestData<JobConversationParticipant>();
				participant.JCP_ParticipantTableCode = contact.TablePrefix;
				participant.ParentKey = contact.PK.ToString();

				mockParticipantProvider.Setup(x => x.GetAdditionalParticipants(order, participant)).Returns(additionalParticipants);

				AssertSequencesEqual(additionalParticipants, provider.GetAdditionalParticipants(subscribedParticipants, participant));
			}
		}

		public void TestGetAdditionalParticipants_NeoConversationsDisabled()
		{
			using (GlowRegistry.Instance.NeoEnableConversations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var provider = (IConversationAdditionalParticipantProvider)order;

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
				var provider = (IConversationAdditionalParticipantProvider)order;

				var additionalParticipants = new List<IConversationParticipant>();

				var mockParticipantProvider = new Mock<IWarehouseConversationParticipantProvider>();
				ObjectFactory.Substitute(mockParticipantProvider.Object);

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				var subscribedParticipants = new ReadOnlyCollection<IConversationParticipant>(new List<IConversationParticipant> { staff });

				var contact = Factory.NewWithValidTestData<OrgContact>();
				var participant = Factory.NewWithValidTestData<JobConversationParticipant>();
				participant.JCP_ParticipantTableCode = contact.TablePrefix;
				participant.ParentKey = contact.PK.ToString();

				mockParticipantProvider.Setup(x => x.GetAdditionalParticipants(order, participant)).Returns(additionalParticipants);

				AssertSequencesEqual(Enumerable.Empty<IConversationParticipant>(), provider.GetAdditionalParticipants(subscribedParticipants, participant));
			}
		}

		public void TestShouldUseThisProviderForHyperlink()
		{
			var provider = (IConversationParentHyperlinkProvider)order;

			Assert(provider.ShouldUseThisProviderForHyperlink(null));
			Assert(provider.ShouldUseThisProviderForHyperlink(Factory.New<OrgContact>()));
			Assert(!provider.ShouldUseThisProviderForHyperlink(Factory.New<GlbStaff>()));
		}

		public void TestGetHyperlinkToConversationParent()
		{
			using (GlowRegistry.Instance.GlowPortalsUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glowdev/Portals/"))
			{
				var provider = (IConversationParentHyperlinkProvider)order;

				AssertEquals($"https://glowdev/Portals/NEO/Desktop#/formFlow/default/IWhsOrder/{order.PK}", provider.GetHyperlinkToConversationParent());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			order = Factory.NewWithValidTestData<WhsOrder>();
		}
		WhsOrder order;
	}

	#endregion
}
