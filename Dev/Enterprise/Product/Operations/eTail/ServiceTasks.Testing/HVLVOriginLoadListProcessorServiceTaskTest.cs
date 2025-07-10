using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.eTail.ServiceTasks.Testing
{
	[TestedType(typeof(HVLVOriginLoadListProcessorServiceTask))]
	class HVLVOriginLoadListProcessorServiceTaskTest : ServiceTaskTestCase<HVLVOriginLoadListProcessorServiceTask>
	{
		public void TestCreateShipment_UseLodgedUserBranchNumberFountainToGenerateShipmentHouseBill()
		{
			var destinationAddress = Factory.NewWithValidTestData<OrgAddress>();
			destinationAddress.OA_RL_NKRelatedPortCode = "USORD";

			var defaultBranchCustomisations = new ForwardingShipmentTest().CreateCustomisations("AA");
			var originLoadListWithoutLog = Factory.NewWithValidTestData<HVLVOriginLoadList>();

			using (FreightDataRegistry.Instance.HouseBillNumberCustomisation.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, defaultBranchCustomisations))
			{
				AssertUseLodgedUserBranchNumberFountainToGenerateShipmentHouseBill("MBN20181221", "AAALX000001", originLoadListWithoutLog, Env.CurrentBranchPK);
			}

			var lodgedUserBranch = Factory.NewWithValidTestData<GlbBranch>();
			var lodgedUserBranchPK = lodgedUserBranch.PK.ToGuid();
			lodgedUserBranch.SetCountry(CountryCodes.Australia);

			Factory.Save();

			var lodgedUserBranchCustomisations = new ForwardingShipmentTest().CreateCustomisations("BB");
			var originLoadListHasLodgedLog = Factory.NewWithValidTestData<HVLVOriginLoadList>();

			using (FreightDataRegistry.Instance.HouseBillNumberCustomisation.SetTemporaryValue(Guid.Empty, lodgedUserBranch.PK.ToGuid(), Guid.Empty, lodgedUserBranchCustomisations))
			{
				AssertUseLodgedUserBranchNumberFountainToGenerateShipmentHouseBill("MBN20181222", "BBALX000001", originLoadListHasLodgedLog, lodgedUserBranchPK);
			}

			void AssertUseLodgedUserBranchNumberFountainToGenerateShipmentHouseBill(string masterBillNum, string expectHouseBillNumber, HVLVOriginLoadList loadList, Guid branchPK)
			{
				var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
				forwardingConsol.JK_MasterBillNum = masterBillNum;
				forwardingConsol.JK_IsCancelled = false;

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branchPK, Env.CurrentDepartmentPK))
				{
					loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Lodged;
					loadList.HVL_MasterBillNumber = masterBillNum;
					loadList.HVL_OA_DestinationDepot = destinationAddress.PK;

					var item = Factory.NewWithValidTestData<HVLVItem>();
					item.HVI_HVL_LoadList = loadList.PK;

					Factory.Save();
				}

				CreateAndRunTask();

				forwardingConsol.Shipments.Reload(true);
				AssertEquals(1, forwardingConsol.Shipments.Count);
				AssertEquals(expectHouseBillNumber, forwardingConsol.Shipments.OfType<ForwardingShipment>().FirstOrDefault().JS_HouseBill);
			}
		}

		public void TestServiceTaskUsesCreatingBranchOfLoadListAsFallbackWhenLogBranchCannotBeFound()
		{
			var creatingUser = Factory.NewWithValidTestData<GlbStaff>();
			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Lodged;
			originLoadList.HVL_MasterBillNumber = "MAWB";
			originLoadList.HVL_SystemCreateUser = creatingUser.GS_Code;

			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_HVL_LoadList = originLoadList.PK;

			Factory.Save();

			var latestLodgedLog = originLoadList.Logs.MostRecentLogByEventTime(
				AutoEvents.StatusUpdated,
				x => x.Parameters.TryGetValue(EventReferenceParameters.Codes.New, out var logTypeValue)
					&& logTypeValue == HVLVOriginLoadListStatus.Codes.Lodged);

			AssertNotNull("Precondition: log is not null", latestLodgedLog);
			latestLodgedLog.SL_GB_NKBranch = "ABC";

			Factory.Save();

			AssertNull("Precondition: branch in log does not exist", Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, latestLodgedLog.SL_GB_NKBranch)));

			var task = CreateAndRunTask();
			var log = task.ServiceLogger.Print();

			var loadListInNewFactory = new BusinessObjectFactory().Load<HVLVOriginLoadList>(new ZQuery(HVLVOriginLoadListSchema.HVL_MasterBillNumber, "MAWB"))[0];
			var hvlShipments = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_ShipmentType, "HVL"));

			CombineAssertions(() =>
			{
				AssertContains("Warning|Could not find matching branch from latest lodge log. Falling back to creating branch of load list...", log);
				AssertEquals("Load list is still processed despite branch in log not existing", HVLVOriginLoadListStatus.Codes.Consolidated, loadListInNewFactory.HVL_Status);
				AssertEquals("Shipment is still created despite branch in log not existing", 1, hvlShipments.Length);
			});
		}

		public void TestServiceTaskLogsErrorWhenBothLogBranchAndCreatingBranchCannotBeFound()
		{
			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Lodged;
			originLoadList.HVL_MasterBillNumber = "MAWB";
			originLoadList.HVL_SystemCreateUser = "ZZZ";

			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_HVL_LoadList = originLoadList.PK;
			item.HVI_Status = "LDG";

			Factory.Save();

			var latestLodgedLog = originLoadList.Logs.MostRecentLogByEventTime(
				AutoEvents.StatusUpdated,
				x => x.Parameters.TryGetValue(EventReferenceParameters.Codes.New, out var logTypeValue)
					&& logTypeValue == HVLVOriginLoadListStatus.Codes.Lodged);

			AssertNotNull("Precondition: log is not null", latestLodgedLog);
			latestLodgedLog.SL_GB_NKBranch = "ABC";

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNull("Precondition: branch in log does not exist", Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, latestLodgedLog.SL_GB_NKBranch)));
				AssertNull("Precondition: creating user does not exist", Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, originLoadList.HVL_SystemCreateUser)));
			});

			var task = CreateAndRunTask();
			var log = task.ServiceLogger.Print();

			var loadListInNewFactory = new BusinessObjectFactory().Load<HVLVOriginLoadList>(new ZQuery(HVLVOriginLoadListSchema.HVL_MasterBillNumber, "MAWB"))[0];
			var mostRecentUpdateLog = loadListInNewFactory.Logs.MostRecentLogByEventTime(AutoEvents.StatusUpdated);
			var hvlShipments = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_ShipmentType, "HVL"));

			CombineAssertions(() =>
			{
				AssertContains("Warning|Could not find matching branch from latest lodge log. Falling back to creating branch of load list...", log);
				AssertContains($"Error|Could not find the creating branch for HVLV Origin Load List {loadListInNewFactory.HVL_UniqueReference}. Load list status has been set to 'FAL - Failed'.", log);
				AssertEquals("Load list is set to FAL status", HVLVOriginLoadListStatus.Codes.Failed, loadListInNewFactory.HVL_Status);
				Assert("Items are set to LLA status", loadListInNewFactory.Items.Cast<HVLVItem>().All(item => item.HVI_Status == HVLVItemStatus.Codes.LoadListAllocated));
				AssertEquals("No shipments are created created despite branch in log not existing", 0, hvlShipments.Length);
				AssertEquals($"|OLD=LDG|NEW=FAL|RES=Could not find the creating branch for HVLV Origin Load List {loadListInNewFactory.HVL_UniqueReference}. Load list status has been set to 'FAL - Failed'.", mostRecentUpdateLog.SL_Reference);
			});
		}

		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Precondition: expected single attribute", 1, hostedServiceAttributes.Length);

			var hostedServiceAttribute = hostedServiceAttributes.Single();
			CombineAssertions(() =>
			{
				Assert("CanRunInAnyBranch", hostedServiceAttribute.CanRunInAnyBranch);
				AssertEquals("1minute", hostedServiceAttribute.MinimumPeriod);
			});
		}

		public void TestAttachToConsol_MasterHouse_ExistingConsol_ExistingHVM_ExistingHVL()
		{
			var loadList = GetLoadList(MasterBillNumber);
			loadList.HVL_IsMasterHouse = true;

			var stdBookingHeader = GetBookingHeader();
			var expBookingHeader = GetBookingHeader();
			Factory.Save();

			var consol = CreateMatchingConsol();
			var hvmShipment = CreateMatchingHVMShipment(consol);

			CreateMatchingHVLShipment(consol, hvmShipment.PK);

			expBookingHeader.HVH_RS_NKBookingServiceLevel = "EXP";

			var stdConsignment = stdBookingHeader.Consignments.AddNew();
			stdConsignment.HVC_RN_NKConsigneeCountryCode = "IT";
			stdConsignment.HVC_WaybillNumber = "HVC001";
			var stdItem = stdConsignment.Items.AddNew();
			var boxedSTDItem = stdConsignment.Items.AddNew();

			var expConsignment = expBookingHeader.Consignments.AddNew();
			expConsignment.HVC_RN_NKConsigneeCountryCode = "FR";
			expConsignment.HVC_WaybillNumber = "HVC002";
			var expItem = expConsignment.Items.AddNew();
			expItem.HVI_CurrentBarcode = "Express is good";
			var boxedEXPItem = expConsignment.Items.AddNew();
			boxedEXPItem.HVI_CurrentBarcode = "Express and boxed is even better";

			stdItem.HVI_HVL_LoadList = loadList.PK;
			expItem.HVI_HVL_LoadList = loadList.PK;
			boxedEXPItem.HVI_HVL_LoadList = loadList.PK;
			boxedSTDItem.HVI_HVL_LoadList = loadList.PK;

			var outerPackage = loadList.OuterPackages[0];

			boxedSTDItem.HVI_HVO_OuterPackage = outerPackage.PK;
			boxedEXPItem.HVI_HVO_OuterPackage = outerPackage.PK;

			Factory.Save();

			CreateAndRunTask();

			var factory = new BusinessObjectFactory();

			var consols = factory.Load<ForwardingConsol>(new ZQuery());
			AssertEquals(1, consols.Length);
			consol = consols[0];
			var shipments = consol.Shipments;
			AssertEquals("1 HVL & 1 HVM exist, 1 HVL matched and updated, 1 new HVL was created", 3, shipments.Count);

			var lastShipment = factory.LoadTop1<ForwardingShipment>(new ZQuery() { OrderBy = "JS_UniqueConsignRef DESC" });

			AssertContainsExactElementsInAnyOrder(new[] { "Express is good", "Express and boxed is even better" }, lastShipment.HVLVItems.Select(i => i.HVI_CurrentBarcode));
		}

		public void TestAttachToExistingConsol()
		{
			var masterBillNumber = "MBN20181221";

			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_Status = "LDG";
			originLoadList.HVL_MasterBillNumber = masterBillNumber;

			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_HVL_LoadList = originLoadList.PK;

			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			forwardingConsol.JK_MasterBillNum = masterBillNumber;
			forwardingConsol.JK_IsCancelled = false;

			Factory.Save();

			CreateAndRunTask();

			forwardingConsol = new BusinessObjectFactory().Load<ForwardingConsol>(forwardingConsol.PK);
			AssertEquals(1, forwardingConsol.Shipments.Count);
		}

		public void TestDoNotAttachToDeactivatedConsol()
		{
			var masterBillNumber = "MBN20181221";

			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_Status = "LDG";
			originLoadList.HVL_MasterBillNumber = masterBillNumber;

			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_HVL_LoadList = originLoadList.PK;

			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			forwardingConsol.JK_MasterBillNum = masterBillNumber;
			forwardingConsol.JK_IsCancelled = true;

			Factory.Save();

			CreateAndRunTask();

			forwardingConsol.Shipments.Reload(true);
			AssertEquals("A load list should not attach to a deactivated consol", 0, forwardingConsol.Shipments.Count);
		}

		public void TestCreateConsol()
		{
			using (ForwardingConfigurationRegistry.Instance.ConsolTypeDefaultForConsol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AgentType.CoLoad))
			{
				var masterBillNumber = "MBN20181227";

				var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
				originLoadList.HVL_Status = "LDG";
				originLoadList.HVL_MasterBillNumber = masterBillNumber;

				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_HVL_LoadList = originLoadList.PK;

				Factory.Save();

				CreateAndRunTask();

				var query = new ZQuery();
				query.AddToFilter(JobConsolSchema.JK_MasterBillNum, masterBillNumber);
				var forwardingConsol = Factory.LoadTop1<ForwardingConsol>(query);
				forwardingConsol.JK_MasterBillNum = masterBillNumber;
				AssertNotNull(forwardingConsol);
				AssertNotNull(masterBillNumber, forwardingConsol.JK_MasterBillNum);
				AssertEquals(1, forwardingConsol.Shipments.Count);

				AssertEquals("Consol Agent Type should be hard coded to 'AGT'", "AGT", forwardingConsol.JK_AgentType);
			}
		}

		public void TestAttachToExistingConsol_SetsLoadedOnConsolForOuterPackages()
		{
			const string masterBillNumber = "MBN20181221";

			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_Status = "LDG";
			originLoadList.HVL_MasterBillNumber = masterBillNumber;

			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_HVL_LoadList = originLoadList.PK;

			var outerPackage1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var outerPackage2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage1.HVO_HVL_LoadList = originLoadList.PK;
			outerPackage2.HVO_HVL_LoadList = originLoadList.PK;

			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			forwardingConsol.JK_MasterBillNum = masterBillNumber;
			forwardingConsol.JK_IsCancelled = false;

			Factory.Save();
			CreateAndRunTask();

			forwardingConsol.Shipments.Reload(true);
			originLoadList.OuterPackages.Reload(true);
			CombineAssertions(() =>
			{
				AssertEquals(forwardingConsol.PK, outerPackage1.HVO_JK_LoadedOnConsol);
				AssertEquals(forwardingConsol.PK, outerPackage2.HVO_JK_LoadedOnConsol);
			});
		}

		public void TestCreateConsol_SetsLoadedOnConsolForOuterPackages()
		{
			const string masterBillNumber = "MBN20181227";

			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_Status = "LDG";
			originLoadList.HVL_MasterBillNumber = masterBillNumber;

			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_HVL_LoadList = originLoadList.PK;

			var outerPackage1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var outerPackage2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage1.HVO_HVL_LoadList = originLoadList.PK;
			outerPackage2.HVO_HVL_LoadList = originLoadList.PK;

			Factory.Save();

			CreateAndRunTask();

			var query = new ZQuery();
			query.AddToFilter(JobConsolSchema.JK_MasterBillNum, masterBillNumber);
			var forwardingConsol = Factory.LoadTop1<ForwardingConsol>(query);
			forwardingConsol.JK_MasterBillNum = masterBillNumber;

			originLoadList.OuterPackages.Reload(true);
			CombineAssertions(() =>
			{
				AssertEquals(forwardingConsol.PK, outerPackage1.HVO_JK_LoadedOnConsol);
				AssertEquals(forwardingConsol.PK, outerPackage2.HVO_JK_LoadedOnConsol);
			});
		}

		public void TestCreateConsol_WhenLoadListPKIsLocked_ShouldStopProcessing()
		{
			var masterBillNumber = "MBN20181227";

			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_Status = "LDG";
			originLoadList.HVL_MasterBillNumber = masterBillNumber;

			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_HVL_LoadList = originLoadList.PK;

			Factory.Save();

			var task = new HVLVOriginLoadListProcessorServiceTask();

			var connection = Db.NewExtraConnectionToMainDb();
			Assert(connection.TryGetLock(("HVLVOriginLoadListProcessingQueue," + originLoadList.PK.ToString()).ToUpperInvariant(), out var appLock));

			using (appLock)
			{
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);
			}

			AssertContains("HLP should contains warning message", "Warning|Failed to acquire lock for rows in table HVLVOriginLoadList, data being processed by other user.", task.ServiceLogger.ToString().Trim());

			var query = new ZQuery();
			query.AddToFilter(JobConsolSchema.JK_MasterBillNum, masterBillNumber);
			var forwardingConsols = Factory.Load<ForwardingConsol>(query);

			AssertEquals("No consol should be created from loadlist", 0, forwardingConsols.Length);
		}

		public void TestAttachToExistingConsol_WhenCreateMultiLoadLists_SetsLoadedOnConsolForOuterPackages()
		{
			const string masterBillNumber = "MBN20181221";

			var originLoadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList1.HVL_Status = "LDG";
			originLoadList1.HVL_MasterBillNumber = masterBillNumber;

			var item1 = Factory.NewWithValidTestData<HVLVItem>();
			item1.HVI_HVL_LoadList = originLoadList1.PK;

			var outerPackage1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var outerPackage2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage1.HVO_HVL_LoadList = originLoadList1.PK;
			outerPackage2.HVO_HVL_LoadList = originLoadList1.PK;

			var originLoadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList2.HVL_Status = "LDG";
			originLoadList2.HVL_MasterBillNumber = masterBillNumber;

			var item2 = Factory.NewWithValidTestData<HVLVItem>();
			item2.HVI_HVL_LoadList = originLoadList2.PK;

			var outerPackage3 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var outerPackage4 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage3.HVO_HVL_LoadList = originLoadList2.PK;
			outerPackage4.HVO_HVL_LoadList = originLoadList2.PK;

			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			forwardingConsol.JK_MasterBillNum = masterBillNumber;
			forwardingConsol.JK_IsCancelled = false;

			var forwardingConsol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			var success = new HVLVOriginLoadListHelper(new DummyLogger()).TryAttachToConsol(forwardingConsol2, new List<IHVLVOriginLoadList> { originLoadList2 }, out _);

			Factory.Save();
			CreateAndRunTask();

			forwardingConsol.Shipments.Reload(true);
			originLoadList1.OuterPackages.Reload(true);
			originLoadList2.OuterPackages.Reload(true);
			CombineAssertions(() =>
			{
				AssertEquals(forwardingConsol.PK, outerPackage1.HVO_JK_LoadedOnConsol);
				AssertEquals(forwardingConsol.PK, outerPackage2.HVO_JK_LoadedOnConsol);
				AssertEquals(forwardingConsol2.PK, outerPackage3.HVO_JK_LoadedOnConsol);
				AssertEquals(forwardingConsol2.PK, outerPackage4.HVO_JK_LoadedOnConsol);
			});
		}

		public void TestRunTask_MultipleMasterHouseLoadList()
		{
			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_Status = "LDG";
			loadList1.HVL_IsMasterHouse = true;
			loadList1.HVL_RS_NKServiceLevel = "EXP";

			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_Status = "LDG";
			loadList2.HVL_IsMasterHouse = true;
			loadList2.HVL_RS_NKServiceLevel = "STD";

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader1.HVH_BookingReference = "TestHeader1";
			bookingHeader1.HVH_RS_NKBookingServiceLevel = "EXP";

			CreateConsignmentWithItem(bookingHeader1, "Waybill1", loadList1.PK);
			CreateConsignmentWithItem(bookingHeader1, "Waybill2", loadList2.PK);

			Factory.Save();
			CreateAndRunTask();

			var masterShipments = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_ShipmentType, "HVM"));
			CombineAssertions(() =>
			{
				AssertEquals("Should create 2 master shipments as loadlists service level are different", 2, masterShipments.Length);
				AssertContainsExactElementsInAnyOrder("Expect 2 master shipments have correct service levels populated from loadlists", new[] { "STD", "EXP" }, masterShipments.Select(shipment => shipment.JS_RS_NKServiceLevel));
			});
		}

		public void TestRunTask_HVLShipmentAttachedToHVMShipmentConsols()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol1.JK_MasterBillNum = "Master1";

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_MasterBillNum = "Master2";

			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_Status = "LDG";
			loadList1.HVL_IsMasterHouse = true;
			loadList1.HVL_RS_NKServiceLevel = "EXP";
			loadList1.HVL_MasterBillNumber = "Master1";

			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_Status = "LDG";
			loadList2.HVL_IsMasterHouse = true;
			loadList2.HVL_RS_NKServiceLevel = "STD";
			loadList2.HVL_MasterBillNumber = "Master2";

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader1.HVH_BookingReference = "TestHeader1";
			bookingHeader1.HVH_RS_NKBookingServiceLevel = "EXP";

			CreateConsignmentWithItem(bookingHeader1, "Waybill1", loadList1.PK);
			CreateConsignmentWithItem(bookingHeader1, "Waybill2", loadList2.PK);

			Factory.Save();
			CreateAndRunTask();

			var hvmShipments = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_ShipmentType, "HVM"));
			var hvlShipments = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_ShipmentType, "HVL"));
			CombineAssertions(() =>
			{
				AssertEquals("Should create 2 master shipments as loadlists service level are different", 2, hvmShipments.Length);
				AssertEquals("Should create 2 hvl shipments", 2, hvlShipments.Length);
				AssertContainsExactElementsInAnyOrder(new[] { consol1.PK, consol2.PK }, hvlShipments.SelectMany(x => x.Consols.Select(y => y.PK)).Distinct());
			});
		}

		public void TestWhenRunHLPServiceTask_FireELCEventTriggerInLWK()
		{
			const string masterBillNumber = "MBN20221129";
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_TransportMode = TransportModes.Air;
			loadList.HVL_IsMasterHouse = true;
			loadList.HVL_Status = "LDG";
			loadList.HVL_MasterBillNumber = masterBillNumber;

			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_HVL_LoadList = loadList.PK;

			Factory.Save();
			CreateAndRunTask();

			var consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_MasterBillNum, masterBillNumber));
			var jobConShipLink = loadList.Factory.LoadTop1<JobConShipLink>(new ZQuery(JobConShipLinkSchema.JN_JK, consol.PK));
			var shipment = loadList.Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.PK, jobConShipLink.JN_JS));

			var shipmentWorkflowItem = shipment.WorkflowItems.AddNew();
			shipmentWorkflowItem.P9_Type = Workflow.WorkflowTriggerType;
			shipmentWorkflowItem.TriggerConditions.TriggerEventCode = "ELC";
			shipmentWorkflowItem.P9_RespondToCascadedEvents = true;
			shipmentWorkflowItem.P9_Description = "trigger1";

			var notification = shipmentWorkflowItem.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = "XML";
			notification.PQ_TriggerParty = "EML";
			notification.PQ_EmailAddr = "xxx@yyy.zzz";

			Factory.Save();
			AssertEquals("Precondition: Trigger binding to ELC is not fired", (ZShort)100, shipmentWorkflowItem.P9_TriggerFiredCountdown);

			MasterFilesTestHelper.RunLogWalker();
			var query = new ZQuery();
			query.AddToFilter(ProcessTasksSchema.P9_ParentID, shipment.PK);
			query.AddToFilter(ProcessTasksSchema.P9_Description, "trigger1");
			var trigger = new BusinessObjectFactory().LoadTop1<ProcessTask>(query);

			AssertEquals("ELC events with SL_FireWorkflow = true should fire in LWK", (ZShort)99, trigger.P9_TriggerFiredCountdown);
		}

		public void TestAttachToConsol_WhenLoadListIsMasterHouse_DuplicateHVLVItemUNDGToHVMShipmentPackingLine()
		{
			const string masterBillNumber = "MBN20181221";

			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_Status = "LDG";
			originLoadList.HVL_MasterBillNumber = masterBillNumber;
			originLoadList.HVL_IsMasterHouse = true;

			var outerPackage1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			var outerPackage2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage1.HVO_HVL_LoadList = originLoadList.PK;
			outerPackage1.HVO_PackageReference = "OuterPackage1";
			outerPackage2.HVO_HVL_LoadList = originLoadList.PK;
			outerPackage2.HVO_PackageReference = "OuterPackage2";

			var item1 = Factory.NewWithValidTestData<HVLVItem>();
			var item2 = Factory.NewWithValidTestData<HVLVItem>();
			var item3 = Factory.NewWithValidTestData<HVLVItem>();
			item1.HVI_HVO_OuterPackage = outerPackage1.PK;
			item1.HVI_HVL_LoadList = originLoadList.PK;
			item2.HVI_HVO_OuterPackage = outerPackage1.PK;
			item2.HVI_HVL_LoadList = originLoadList.PK;
			item3.HVI_HVO_OuterPackage = outerPackage2.PK;
			item3.HVI_HVL_LoadList = originLoadList.PK;

			var undgSubstance = Factory.New<UNDGSubstance>();
			var undgLine = item1.UNDGs.AddNew();
			undgSubstance.DG_UNNO = "4321";
			undgLine.DI_DG = undgSubstance.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = masterBillNumber;
			consol.JK_IsCancelled = false;

			Factory.Save();

			var task = new HVLVOriginLoadListProcessorServiceTask();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);

			var consolInNewFactory = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			var loadedShipment = consolInNewFactory.Shipments.OfType<ForwardingShipment>().Single(s => s.JS_ShipmentType == ShipmentTypes.HighVolumeLowValueMaster);

			var undgInShipment = loadedShipment.OuterPackLines.SelectMany(packLine => ((ForwardingPackLine)packLine).UNDGs).FirstOrDefault();

			CombineAssertions(() =>
			{
				AssertNotNull("A new UNDG created under shipment pack line", undgInShipment);
				AssertEquals("UNDG was created under pack line", JobPackLinesSchema.Constants.Prefix, undgInShipment.DI_ParentTableCode);
				AssertEquals("New UNDG data was copied from HVLVItem UNDG", undgSubstance.PK, undgInShipment.DI_DG);
			});
		}

		public void TestAttachConsol_SkipProcessEmptyLoadList()
		{
			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_Status = "LDG";
			originLoadList.HVL_UniqueReference = "HVL000000000000001";

			Factory.Save();

			var task = CreateAndRunTask();

			var loadListInNewFactory = new BusinessObjectFactory().Load<HVLVOriginLoadList>(new ZQuery())[0];
			CombineAssertions("Process load list has no Items", () =>
			{
				AssertContains("Error message", "Error|HVLV Origin Load List 'HVL000000000000001' does not have any items attached", task.ServiceLogger.ToString());
				AssertEquals("Status change to FAL", "FAL", loadListInNewFactory.HVL_Status);
				Assert("New log added", loadListInNewFactory.Logs.GetAllLogs().Cast<StmALog>().Any(log => log.SL_Reference == "|OLD=LDG|NEW=FAL|RES=HVLV Origin Load List 'HVL000000000000001' does not have any items attached"));
			});
		}

		public void TestGivenOuterPackageStatusIsLodged_WhenLoadListIsSuccessfullyCreated_ThenStatusUpdatedToConsolidated()
		{
			const string masterBillNumber = "MBN20181221";

			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_Status = "LDG";
			originLoadList.HVL_MasterBillNumber = masterBillNumber;

			var item = Factory.NewWithValidTestData<HVLVItem>();
			item.HVI_HVL_LoadList = originLoadList.PK;

			var outerPackage1 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage1.HVO_Status = HVLVOuterPackageStatus.Codes.Lodged;
			outerPackage1.HVO_HVL_LoadList = originLoadList.PK;

			var outerPackage2 = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage2.HVO_Status = HVLVOuterPackageStatus.Codes.Lodged;
			outerPackage2.HVO_HVL_LoadList = originLoadList.PK;

			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			forwardingConsol.JK_MasterBillNum = masterBillNumber;
			forwardingConsol.JK_IsCancelled = false;

			Factory.Save();
			CreateAndRunTask();

			forwardingConsol.Shipments.Reload(true);
			originLoadList.OuterPackages.Reload(true);
			CombineAssertions(() =>
			{
				var packages = originLoadList.OuterPackages;

				var package1 = packages[0];
				package1.Reload();
				AssertEquals(HVLVOuterPackageStatus.Codes.Consolidated, package1.HVO_Status);

				var package2 = packages[1];
				package2.Reload();
				AssertEquals(HVLVOuterPackageStatus.Codes.Consolidated, package2.HVO_Status);
			});
		}

		[UseSnapshotProtection]
		public void TestRunTask_WhenAttachToConsolFailsDueToRepeatedWaybill_ShouldReportErrorsAndSetLoadStatusFail()
		{
			// Need a connection not in a transaction since we need to mock the rollback when factory saving failure
			using (var disposable = new DisposableAction(
				() => { Db.ConnectionOverrideForTest = Db.NewExtraConnectionToMainDb(); },
				() =>
				{
					Db.ConnectionOverrideForTest.Dispose();
					Db.ConnectionOverrideForTest = null;
				}))
			{
				var factory = new BusinessObjectFactory();
				var masterBillNumber = "MBN20181227";

				var originLoadList = factory.NewWithValidTestData<HVLVOriginLoadList>();
				originLoadList.HVL_Status = "LDG";
				originLoadList.HVL_MasterBillNumber = masterBillNumber;

				var bookingHeader1 = factory.NewWithValidTestData<HVLVBookingHeader>();
				var bookingHeader2 = factory.NewWithValidTestData<HVLVBookingHeader>();
				bookingHeader1.HVH_BookingReference = "TestHeader1";
				bookingHeader2.HVH_BookingReference = "TestHeader2";
				bookingHeader2.HVH_OA_BillToParty = bookingHeader1.HVH_OA_BillToParty;
				bookingHeader1.HVH_RS_NKBookingServiceLevel = "EXP";
				bookingHeader2.HVH_RS_NKBookingServiceLevel = "EXP";

				CreateConsignmentWithItem(bookingHeader1, "Waybill1", originLoadList.PK);
				CreateConsignmentWithItem(bookingHeader2, "Waybill1", originLoadList.PK);

				var bookingHeader3 = factory.NewWithValidTestData<HVLVBookingHeader>();
				var bookingHeader4 = factory.NewWithValidTestData<HVLVBookingHeader>();
				bookingHeader3.HVH_BookingReference = "TestHeader3";
				bookingHeader4.HVH_BookingReference = "TestHeader4";
				bookingHeader4.HVH_OA_BillToParty = bookingHeader3.HVH_OA_BillToParty;
				bookingHeader3.HVH_RS_NKBookingServiceLevel = "STD";
				bookingHeader4.HVH_RS_NKBookingServiceLevel = "STD";

				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_HVL_LoadList = originLoadList.PK;
				item.HVI_Status = "LDG";

				CreateConsignmentWithItem(bookingHeader3, "Waybill2", originLoadList.PK);
				CreateConsignmentWithItem(bookingHeader4, "Waybill2", originLoadList.PK);

				factory.Save();

				var task = CreateAndRunTask();

				originLoadList.Reload();

				AssertContains("Error Message", $"Error|An error occurred when processing the HVLV Origin Load List '{originLoadList.HVL_UniqueReference}'", task.ServiceLogger.ToString());
				AssertEquals("Load List status should be set", HVLVOriginLoadListStatus.Codes.Failed, originLoadList.HVL_Status);
				Assert("Items are set to LLA status", originLoadList.Items.Cast<HVLVItem>().All(item => item.HVI_Status == HVLVItemStatus.Codes.LoadListAllocated));

				var query = new ZDBOnlyQuery(typeof(ForwardingConsol));

				var forwardingConsols = new BusinessObjectFactory().Load<ForwardingConsol>(query);

				AssertEquals("No consol should be created from loadlist", 0, forwardingConsols.Length);
			}
		}

		public void TestRunTask_MultipleMasterHouseLoadListWithEmptyHouseBill()
		{
			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_Status = "LDG";
			loadList1.HVL_IsMasterHouse = true;
			loadList1.HVL_RS_NKServiceLevel = "EXP";
			loadList1.HVL_HouseBillNumber = ZString.Empty;

			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_Status = "LDG";
			loadList2.HVL_IsMasterHouse = true;
			loadList2.HVL_RS_NKServiceLevel = "STD";
			loadList2.HVL_HouseBillNumber = ZString.Empty;

			var bookingHeader1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader1.HVH_BookingReference = "TestHeader1";
			bookingHeader1.HVH_RS_NKBookingServiceLevel = "D2D";

			CreateConsignmentWithItem(bookingHeader1, "Waybill1", loadList1.PK);
			CreateConsignmentWithItem(bookingHeader1, "Waybill2", loadList2.PK);

			Factory.Save();
			CreateAndRunTask();

			var masterShipments = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_ShipmentType, ShipmentTypes.HighVolumeLowValueMaster));

			CombineAssertions(() =>
			{
				AssertEquals("Should create 2 master shipments as loadlists service level are different", 2, masterShipments.Length);
				AssertContainsExactElementsInAnyOrder("Expect 2 master shipments have correct service levels populated from loadlists", new[] { "STD", "EXP" }, masterShipments.Select(shipment => shipment.JS_RS_NKServiceLevel));
			});
		}

		HVLVConsignment CreateConsignmentWithItem(HVLVBookingHeader header, string waybillNumber, ZGuid loadListPK)
		{
			var consignment = header.Consignments.AddNew();
			consignment.HVC_WaybillNumber = waybillNumber;
			var item = consignment.Items.AddNew();
			item.HVI_HVL_LoadList = loadListPK;
			return consignment;
		}

		public void TestProcessLoadList_ShipmentConsignorConsigneeAddressArePopulated()
		{
			var billToOrg = Factory.NewWithValidTestData<OrgHeader>();
			billToOrg.OH_Code = "ORG";
			var billToAddress = billToOrg.Addresses.AddNew();
			billToAddress.Address1 = "Addr1";

			var originLoadList = GetLoadList("00001111");
			originLoadList.HVL_IsMasterHouse = true;
			var bookingHeader = GetBookingHeader();
			bookingHeader.HVH_OA_BillToParty = billToAddress.PK;
			var consignment = bookingHeader.Consignments.AddNew();
			var item = consignment.Items.AddNew();
			item.HVI_HVL_LoadList = originLoadList.PK;

			Factory.Save();

			CreateAndRunTask();

			var consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery());
			var hvmShipment = consol.Shipments.OfType<ForwardingShipment>().Single(s => s.JS_ShipmentType == ShipmentTypes.HighVolumeLowValueMaster);
			var hvlShipment = consol.Shipments.OfType<ForwardingShipment>().Single(s => s.JS_ShipmentType == ShipmentTypes.HighVolumeLowValue);

			CombineAssertions("Consignor and consignee addresses were populated", () =>
			{
				AssertEquals(OriginDepot.Address1, hvmShipment.ConsignorDocumentaryAddress.Address1);
				AssertEquals(DestinationDepot.Address1, hvmShipment.ConsigneeDocumentaryAddress.Address1);

				AssertEquals(billToAddress.Address1, hvlShipment.ConsignorDocumentaryAddress.Address1);
				AssertEquals(DestinationDepot.Address1, hvlShipment.ConsigneeDocumentaryAddress.Address1);
			});
		}

		public void TestProcessLoadList_UseNewFactoryToProcessEachLoadlist()
		{
			var loadList1 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList1.HVL_Status = "LDG";
			loadList1.HVL_MasterBillNumber = "00001111";

			var item1 = Factory.NewWithValidTestData<HVLVItem>();
			item1.HVI_HVL_LoadList = loadList1.PK;

			var loadList2 = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList2.HVL_Status = "LDG";
			loadList2.HVL_MasterBillNumber = "00001112";

			var item2 = Factory.NewWithValidTestData<HVLVItem>();
			item2.HVI_HVL_LoadList = loadList2.PK;

			Factory.Save();

			var factoryInstanceCollection = new List<long>();
			BusinessObjectFactory.SetOnFactorySaveHookForTest(
			(factory) =>
			{
				if (factory.NameForDebugging == "Load List Processor" && !factoryInstanceCollection.Any(x => x == factory._Instance))
				{
					factoryInstanceCollection.Add(factory._Instance);
				}
			});

			CreateAndRunTask();
			AssertEquals(2, factoryInstanceCollection.Count);
		}

		public void TestProcessLoadList_PopulateHVLShipmentBasicRegistration()
		{
			HVLVTestHelper.SetExchangeRate(Factory, "USD", 0.5m);

			var billToOrg = Factory.NewWithValidTestData<OrgHeader>();
			billToOrg.OH_Code = "ORG";
			var billToAddress = billToOrg.Addresses.AddNew();
			billToAddress.Address1 = "Addr1";
			billToAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var bookingHeader = GetBookingHeader();
			bookingHeader.HVH_OA_BillToParty = billToAddress.PK;

			var destination = Factory.NewWithValidTestData<OrgHeader>();
			var destinationAddress = destination.Addresses.AddNew();
			destinationAddress.Address1 = "Address1";
			destinationAddress.OA_RL_NKRelatedPortCode = "USLAX";

			var loadList = GetLoadList("00001111");
			loadList.HVL_RL_NKDestination = string.Empty;
			loadList.HVL_OA_DestinationDepot = destinationAddress.PK;

			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_RX_NKGoodsValueCurrency = "AUD";
			consignment1.HVC_GoodsValue = 10;
			var item1 = consignment1.Items.AddNew();
			item1.HVI_HVL_LoadList = loadList.PK;

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_RX_NKGoodsValueCurrency = "USD";
			consignment2.HVC_GoodsValue = 10;
			var item2 = consignment2.Items.AddNew();
			item2.HVI_HVL_LoadList = loadList.PK;

			Factory.Save();
			CreateAndRunTask();

			var hvlShipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_ShipmentType, ShipmentTypes.HighVolumeLowValue));

			CombineAssertions("Shipment basic registration details", () =>
			{
				AssertEquals("Transport mode", TransportModes.Air, hvlShipment.JS_TransportMode);
				AssertEquals("Packing mode", ContainerModes.ULD, hvlShipment.JS_PackingMode);
				AssertEquals("Shipment type", ShipmentTypes.HighVolumeLowValue, hvlShipment.JS_ShipmentType);
				AssertEquals("Origin", "AUSYD", hvlShipment.JS_RL_NKOrigin);
				AssertEquals("Destination", "USLAX", hvlShipment.JS_RL_NKDestination);
				AssertEquals("Goods description", "Various Cargo", hvlShipment.JS_GoodsDescription);
				AssertEquals("Service level", "STD", hvlShipment.JS_RS_NKServiceLevel);
				AssertEquals("Unit of weight", Env.Registry.FreightWeightUnit, hvlShipment.JS_UnitOfWeight);
				AssertEquals("Unit of volume", Env.Registry.FreightVolumeUnit, hvlShipment.JS_UnitOfVolume);
				AssertEquals("Goods value currency", 15m, hvlShipment.JS_GoodsValue);
				AssertEquals("Insurance value currency", "USD", hvlShipment.JS_RX_NKInsuranceCurrency);
			});
		}

		public void TestProcessLoadList_SetShipmentIncoTerms_WhenConsolIsDomestic()
		{
			DestinationDepot.Header.MiscServ.OM_IMDefaultINCOTerm = "DDP";

			var loadList = GetLoadList("12345678901");
			loadList.HVL_RL_NKOrigin = "AUSYD";
			loadList.HVL_RL_NKDestination = "AUSYD";

			var billToOrg = Factory.NewWithValidTestData<OrgHeader>();
			billToOrg.MiscServ.OM_EXDefaultIncoTerm = string.Empty;
			billToOrg.OH_Code = "ORG";
			var billToAddress = billToOrg.Addresses.AddNew();
			billToAddress.Address1 = "Addr1";

			var bookingHeader = GetBookingHeader();
			bookingHeader.HVH_OA_BillToParty = billToAddress.PK;
			var consignment = bookingHeader.Consignments.AddNew();
			var item = consignment.Items.AddNew();
			item.HVI_HVL_LoadList = loadList.PK;

			Factory.Save();

			using (FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CreateAndRunTask();
			}

			var hvlShipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_ShipmentType, ShipmentTypes.HighVolumeLowValue));
			AssertEquals("DDP", hvlShipment.JS_INCO);
		}

		public void TestProcessLoadList_PopulateMasterBillNumberWhenLoadListIsNeutralMaster()
		{
			using var disposable =
				FreightDataRegistry.Instance.AllocateMAWBNumberFromMAWBStockUponXMLImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var mawb = Factory.NewWithValidTestData<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "027";
			mawb.JM_MAWB = "10000001";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_ServiceLevel = "STD";
			mawb.JM_SystemCreateTimeUtc = ZDateTime.Now;

			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "Org1";
			var originDepot = Factory.NewWithValidTestData<OrgAddress>();
			originDepot.OA_OH = orgHeader1.PK;
			originDepot.Header.OH_RL_NKClosestPort = "AUSYD";

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "Org2";
			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepot.OA_OH = orgHeader2.PK;
			destinationDepot.Header.OH_RL_NKClosestPort = "USLAX";

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_OA_OriginDepot = originDepot.PK;
			loadList.HVL_OA_DestinationDepot = destinationDepot.PK;
			loadList.HVL_RL_NKOrigin = "AUSYD";
			loadList.HVL_RL_NKDestination = "USLAX";
			loadList.HVL_TransportMode = TransportModes.Air;
			loadList.HVL_RS_NKServiceLevel = "STD";
			loadList.HVL_VoyageFlight = "AS111";
			loadList.HVL_Status = ELoadListStatuses.Lodged;
			loadList.HVL_UniqueReference = "HVL00000001";
			loadList.HVL_IsNeutralMaster = true;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item = consignment.Items.AddNew();
			item.HVI_ItemId = "HVI0000001";
			item.HVI_HVL_LoadList = loadList.PK;

			Factory.Save();
			CreateAndRunTask();

			loadList = new BusinessObjectFactory().Load<HVLVOriginLoadList>(loadList.PK);
			AssertEquals("02710000001", loadList.HVL_MasterBillNumber);
		}

		public void TestProcessLoadList_UpdateShipmentFromOuterPackLines()
		{
			var loadList = GetLoadList("12345678901");
			var bookingHeader = GetBookingHeader();

			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_WeightUQ = "KG";
			consignment.HVC_VolumeUQ = "M3";

			var item = consignment.Items.AddNew();
			item.HVI_ActualVolume = 3;
			item.HVI_ActualWeight = 3;
			item.HVI_HVL_LoadList = loadList.PK;

			Factory.Save();
			CreateAndRunTask();

			var hvlShipment = Factory.LoadTop1<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_ShipmentType, ShipmentTypes.HighVolumeLowValue));
			AssertEquals(3m, hvlShipment.JS_ActualWeight);
			AssertEquals(3m, hvlShipment.JS_ActualVolume);
			AssertEquals(1, hvlShipment.JS_OuterPacks);
		}

		#region Save Exception Handling

		public void TestHandleSaveConcurrencyException_ShowWarningAndMakeLogEvent()
		{
			MakeAndSaveLodgedLoadListWithItem("AMD20221122");

			var task = RunHLPTaskWithConcurrency(0);

			var loadListInNewFactory = new BusinessObjectFactory().LoadTop1<HVLVOriginLoadList>(new ZQuery(HVLVOriginLoadListSchema.HVL_MasterBillNumber, "AMD20221122"));
			var logs = task.ServiceLogger.ToString().Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
			var hlpFailureLogsCount = GetHLPFailureLogsCount(loadListInNewFactory);

			CombineAssertions("Handling save failure with re-try on next schdule.", () =>
			{
				AssertEquals("Load list should stay 'Lodged'.", HVLVOriginLoadListStatus.Codes.Lodged, loadListInNewFactory.HVL_Status);
				AssertEquals("ERR event should be added to load list", 1, hlpFailureLogsCount);
				Assert("Should output warning", logs.Any(log => log.Contains($"Warning|Another process has made changes related to Load List '{loadListInNewFactory.HVL_UniqueReference}', an attempt to merge the changes will be made in the next schedule.")));
			});
		}

		public void TestHandleSaveConcurrencyException_StopRetryAndMarkLoadListFailedAtSixthFailure()
		{
			MakeAndSaveLodgedLoadListWithItem("AMD20221122");

			for (var i = 0; i < 5; i++)
			{
				RunHLPTaskWithConcurrency(i);
			}

			var sixthRunTask = RunHLPTaskWithConcurrency(5);

			var loadListInNewFactory = new BusinessObjectFactory().LoadTop1<HVLVOriginLoadList>(new ZQuery(HVLVOriginLoadListSchema.HVL_MasterBillNumber, "AMD20221122"));
			var logs = sixthRunTask.ServiceLogger.ToString().Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
			var hlpFailureLogsCount = GetHLPFailureLogsCount(loadListInNewFactory);

			CombineAssertions("Handling save failure with re-try on next schdule.", () =>
			{
				AssertEquals("Load list should be marked as 'Failed'.", HVLVOriginLoadListStatus.Codes.Failed, loadListInNewFactory.HVL_Status);
				AssertEquals("5 ERR events should be added to load list", 5, hlpFailureLogsCount);
				Assert("Should output error if the load list cannot be processed after 5 attempts", logs.Any(log => log.Contains($"Error|An error occurred when processing the HVLV Origin Load List '{loadListInNewFactory.HVL_UniqueReference}'")));
				Assert("Error detail", logs.Any(log => log.Contains($"Load list saving failed due to concurrency error, retry attempts exceeded the threshold of 5.")));
			});
			ExceptionReporterTestListener.Instance.Clear();
		}

		HVLVOriginLoadListProcessorServiceTask RunHLPTaskWithConcurrency(int sequence)
		{
			var task = new HVLVOriginLoadListProcessorServiceTask();
			InitialiseTaskSchedule(task);

			BusinessObjectFactory.SetOnFactorySaveHookForTest((f) =>
			{
				var loadListCauseConcurrencyException = new BusinessObjectFactory().LoadTop1<HVLVOriginLoadList>(new ZQuery(HVLVOriginLoadListSchema.HVL_MasterBillNumber, "AMD20221122"));
				loadListCauseConcurrencyException.HVL_ContainerNumber = $"TestContainerNumber{sequence}";
				BusinessObjectFactory.SetOnFactorySaveHookForTest(null);
				loadListCauseConcurrencyException.Factory.Save();
			});

			RunTaskSchedule(task);

			return task;
		}

		static int GetHLPFailureLogsCount(HVLVOriginLoadList loadList)
		{
			return loadList.Logs.GetAllLogs().Where(l => l.SL_SE_NKEvent == AutoEvents.ErrorReportCode && l.Parameters.ContainsSameElementsInAnyOrder(
				new[]
				{
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.ServiceTask, "HLP"),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Reason, "Concurrency")
				})).Count();
		}

		[UseSnapshotProtection]
		public void TestServiceTaskHandlesClusterKeyClashes()
		{
			Db.Connection.RollbackTransaction();

			MakeAndSaveLodgedLoadListWithItem("AMD20221122");

			CreateAndRunTask();

			var firstConsignmentHeader = GetCreatedConsignmentHeaderByMasterBillNumber("AMD20221122");
			AssertNotNull(firstConsignmentHeader);

			MakeAndSaveLodgedLoadListWithItem("NVD20221122");

			var sqlText = $@"UPDATE dbo.StmNumberCache SET SG_IsUsed = 0 WHERE SG_SN IN (SELECT SN_ID from dbo.StmNums WHERE SN_Name = 'HVLVConsignmentClusterKey') AND SG_Value = '{firstConsignmentHeader.HCH_ClusterKey}';";
			TestConnection.ExecuteNonQuery(sqlText);

			CreateAndRunTask();

			var fixedConsignmentHeader = GetCreatedConsignmentHeaderByMasterBillNumber("NVD20221122");

			CombineAssertions(() =>
			{
				AssertNotNull("Consignment header will exist because the cluster key clash has been fixed", fixedConsignmentHeader);
				AssertEquals("New consignment header should have its cluster key fixed to the next available value", firstConsignmentHeader.HCH_ClusterKey + 2, fixedConsignmentHeader.HCH_ClusterKey);
			});

			Db.Connection.BeginTransaction();
		}

		public void TestServiceTaskHandlesConcurrencyIssueWhenXUSDeletesItemLine()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_Status = "LDG";
			loadList.HVL_UniqueReference = "HVL00000001";
			loadList.HVL_MasterBillNumber = "MBN20230822";

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item = consignment.Items.AddNew();
			item.HVI_ItemId = "HVI0000001";
			item.HVI_HVL_LoadList = loadList.PK;

			var itemLine = item.Lines.AddNew();
			itemLine.HVS_Quantity = 1;

			Factory.Save();

			var task = new HVLVOriginLoadListProcessorServiceTask();
			InitialiseTaskSchedule(task);

			BusinessObjectFactory.SetOnFactorySaveHookForTest((f) =>
			{
				item = f.Load<HVLVItem>(HVLVItemSchema.Constants.Prefix, item.PK);
				item.Lines.Load();

				var itemWithItemLines = new BusinessObjectFactory().LoadTop1<HVLVItem>(new ZQuery(HVLVItemSchema.HVI_ItemId, "HVI0000001"));
				itemWithItemLines.Lines.DeleteAll();
				BusinessObjectFactory.SetOnFactorySaveHookForTest(null);
				itemWithItemLines.Factory.Save();
			});

			AssertNoExceptionThrown(() => RunTaskSchedule(task));

			var loadListInNewFactory = new BusinessObjectFactory().LoadTop1<HVLVOriginLoadList>(new ZQuery(HVLVOriginLoadListSchema.HVL_MasterBillNumber, "MBN20230822"));
			var logs = task.ServiceLogger.ToString().Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();

			Assert("Should output warning", logs.Any(log => log.Contains($"Warning|Another process has made changes related to Load List 'HVL00000001', an attempt to merge the changes will be made in the next schedule.")));
		}

		public void TestCreateEDTLogWithReasonWhenServiceTaskFails()
		{
			const string masterBillNumber = "MBN20181227";

			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_Status = "LDG";
			originLoadList.HVL_MasterBillNumber = masterBillNumber;
			originLoadList.HVL_UniqueReference = "HVL00000001";

			Factory.Save();

			var task = new HVLVOriginLoadListProcessorServiceTask();
			InitialiseTaskSchedule(task);

			RunTaskSchedule(task);

			var reloadedOriginLoadList = Factory.Load<HVLVOriginLoadList>(originLoadList.PK);
			var logRecord = reloadedOriginLoadList.Logs.GetAllLogs().First(log => ((StmALog)log).SL_SE_NKEvent == AutoEvents.StatusUpdated.Code) as StmALog;

			AssertEquals("Assert created Edit log is correct", "|OLD=LDG|NEW=FAL|RES=HVLV Origin Load List 'HVL00000001' does not have any items attached", logRecord.SL_Reference);
		}

		public void TestServiceTaskCanAllocateMAWBWhenMAWBIsAvailable()
		{
			var mawb = Factory.NewWithValidTestData<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "027";
			mawb.JM_MAWB = "10000001";
			mawb.JM_GB = GlbBranch.CurrentBranch.PK;
			mawb.JM_ServiceLevel = "STD";
			mawb.JM_SystemCreateTimeUtc = ZDateTime.Now;

			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "Org1";
			var originDepot = Factory.NewWithValidTestData<OrgAddress>();
			originDepot.OA_OH = orgHeader1.PK;
			originDepot.Header.OH_RL_NKClosestPort = "AUSYD";

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "Org2";
			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepot.OA_OH = orgHeader2.PK;
			destinationDepot.Header.OH_RL_NKClosestPort = "USLAX";

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_OA_OriginDepot = originDepot.PK;
			loadList.HVL_OA_DestinationDepot = destinationDepot.PK;
			loadList.HVL_RL_NKOrigin = "AUSYD";
			loadList.HVL_RL_NKDestination = "USLAX";
			loadList.HVL_TransportMode = TransportModes.Air;
			loadList.HVL_RS_NKServiceLevel = "STD";
			loadList.HVL_VoyageFlight = "AS111";
			loadList.HVL_Status = ELoadListStatuses.Lodged;
			loadList.HVL_UniqueReference = "HVL00000001";
			loadList.HVL_IsNeutralMaster = true;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item = consignment.Items.AddNew();
			item.HVI_ItemId = "HVI0000001";
			item.HVI_HVL_LoadList = loadList.PK;

			var itemLine = item.Lines.AddNew();
			itemLine.HVS_Quantity = 1;

			Factory.Save();

			var task = new HVLVOriginLoadListProcessorServiceTask();
			InitialiseTaskSchedule(task);

			RunTaskSchedule(task);

			var logs = task.ServiceLogger.ToString().Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();

			Assert("There should be no logs with no MAWB available.", !logs.Any(log => log.Contains($"No Master Bill Numbers in stock to allocate to this job. MAWB stock can be added via Maintain > Reference Files > MAWB Stock.")));

			loadList = new BusinessObjectFactory().Load<HVLVOriginLoadList>(loadList.PK);

			AssertEquals("The HVL Status of loadList should be equal to CON.", "CON", loadList.HVL_Status);
		}

		public virtual void TestServiceTaskHandleMAWBAllocationExceptionWhenNoMAWBIsAvailable()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "Org1";
			var originDepot = Factory.NewWithValidTestData<OrgAddress>();
			originDepot.OA_OH = orgHeader1.PK;
			originDepot.Header.OH_RL_NKClosestPort = "AUSYD";

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "Org2";
			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepot.OA_OH = orgHeader2.PK;
			destinationDepot.Header.OH_RL_NKClosestPort = "USLAX";

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_OA_OriginDepot = originDepot.PK;
			loadList.HVL_OA_DestinationDepot = destinationDepot.PK;
			loadList.HVL_RL_NKOrigin = "AUSYD";
			loadList.HVL_RL_NKDestination = "USLAX";
			loadList.HVL_TransportMode = TransportModes.Air;
			loadList.HVL_RS_NKServiceLevel = "STD";
			loadList.HVL_VoyageFlight = "AS111";
			loadList.HVL_Status = ELoadListStatuses.Lodged;
			loadList.HVL_UniqueReference = "HVL00000001";
			loadList.HVL_IsNeutralMaster = true;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item = consignment.Items.AddNew();
			item.HVI_ItemId = "HVI0000001";
			item.HVI_HVL_LoadList = loadList.PK;

			var itemLine = item.Lines.AddNew();
			itemLine.HVS_Quantity = 1;

			Factory.Save();

			var task = new HVLVOriginLoadListProcessorServiceTask();
			InitialiseTaskSchedule(task);

			RunTaskSchedule(task);

			var logs = task.ServiceLogger.ToString().Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();

			AssertEquals(0, ErrorReporter.TotalErrorCount);

			Assert("Should be no MAWB available for output.", logs.Any(log => log.Contains($"No Master Bill Numbers in stock to allocate to this job. MAWB stock can be added via Maintain > Reference Files > MAWB Stock.")));

			loadList = new BusinessObjectFactory().Load<HVLVOriginLoadList>(loadList.PK);
			AssertEquals("The HVL Status of loadList should be equal to CON.", "CON", loadList.HVL_Status);
		}

		public void TestServiceTaskReportsErrorAndHandlesFriendlyMessageWhenSaveHandlerFails()
		{
			MakeAndSaveLodgedLoadListWithItem("AMD20221122", "HVL00000001");

			CreateAndRunTask();

			var firstConsignmentHeader = GetCreatedConsignmentHeaderByMasterBillNumber("AMD20221122");
			AssertNotNull(firstConsignmentHeader);

			var loadlist = MakeAndSaveLodgedLoadListWithItem("NVD20221122", "HVL00000002");

			List<string> logs = null;

			BusinessObjectFactory.SetOnFactorySaveHookForTest((factory) =>
			{
				var query = new ZQuery();
				query.FetchOnlyFromLocalCache = true;
				var consolsInFactory = factory.Load<ForwardingConsol>(query);

				// If there is no consols in cache then the factory is not running save for HLP, so we should not hook the exception throwing
				if (consolsInFactory.Length > 0)
				{
					factory.Saving += thisFactory =>
					{
						throw new ZSaveException(new FriendlyMessageExceptionForTest("Test save exception"), thisFactory);
					};
				}
			});

			AssertNoExceptionThrown(() =>
			{
				var task = CreateAndRunTask();
				logs = task.ServiceLogger.ToString().Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
			});

			CombineAssertions("Save exception should be output to log", () =>
			{
				Assert("Should output ** Error Saving Record **", logs.Any(log => log.Contains("** Error Saving Record **")));
				Assert("Should output Friendly Message", logs.Any(log => log.Contains("Test save exception")));
				Assert("Should output Inner Message", logs.Any(log => log.Contains("Inner Message = An exception with friendly message was thrown.")));
			});

			AssertEquals("An error occurred when processing the HVLV Origin Load List 'HVL00000002'", ErrorReporter.LastMessageReported);
			Assert("Exception should be thrown to ErrorReporter", ErrorReporter.ExceptionsThrown.Any(error => error.Contains("An exception with friendly message was thrown.")));

			var eventReferenceWithErrorDetail = "|OLD=LDG|NEW=FAL|RES=Test save exception";
			var reloadedOriginLoadList = Factory.Load<HVLVOriginLoadList>(loadlist.PK);
			var stuEvents = reloadedOriginLoadList.Logs.GetAllLogs().OfType<StmALog>().Where(log => log.SL_SE_NKEvent == AutoEvents.StatusUpdated.Code);

			Assert("Friendly Message of ZSaveException should be write into STU event", stuEvents.Any(e => e.SL_Reference.Contains(eventReferenceWithErrorDetail)));

			ErrorReporter.Clear();
		}

		public void TestChangeLoadListStatusToFALWhenSaveHandlerFails()
		{
			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_IsMasterHouse = true;
			originLoadList.HVL_Status = "LDG";
			originLoadList.HVL_MasterBillNumber = "MAWB";
			originLoadList.HVL_TransportMode = "AIR";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			var outerPackage = originLoadList.OuterPackages.AddNew();
			outerPackage.HVO_VolumeUQ = "M3";
			outerPackage.HVO_Volume = 6000;

			var item = consignment.Items.AddNew();
			item.HVI_HVL_LoadList = originLoadList.PK;
			Factory.Save();

			var column = JobShipmentSchema.JS_ActualChargeable;
			var columnMaxValue = (decimal)Math.Pow(10, column.Precision - column.Scale) - 1;
			var actualChargeable = ChargeableAmountCalculator.CalculateChargeable(new ChargeableParameters
			{
				Volume = new ZVolume(outerPackage.HVO_Volume, outerPackage.HVO_VolumeUQ),
				TargetUnit = Weight.Kilograms,
				ConversionFactors = ChargeableAmountCalculator.GetDefaultConversionFactors(false, TransportModes.Air, Weight.Kilograms)
			}).Chargeable.Amount;

			Assert("Pre-condition: Actual Chargeable amount exceeds max value of JS_ActualChargeable", actualChargeable > columnMaxValue);

			AssertNoExceptionThrown(() => CreateAndRunTask());

			var loadListInNewFactory = new BusinessObjectFactory().Load<HVLVOriginLoadList>(new ZQuery(HVLVOriginLoadListSchema.HVL_MasterBillNumber, "MAWB"))[0];
			AssertEquals("FAL", loadListInNewFactory.HVL_Status);

			ErrorReporter.Clear();
		}

		public void TestServiceTaskReportsValidityErrorsWhenSaveHandlerFails()
		{
			var originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			originLoadList.HVL_Status = ELoadListStatuses.Lodged;
			originLoadList.HVL_MasterBillNumber = "MAWB";
			originLoadList.HVL_UniqueReference = "HVL00000001";
			originLoadList.HVL_IsMasterHouse = true;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			bookingHeader.HVH_BookingReference = "TestHeader";
			bookingHeader.HVH_RS_NKBookingServiceLevel = "STD";
			var consignment1 = CreateConsignmentWithItem(bookingHeader, "Waybill1", originLoadList.PK);
			var consignment2 = CreateConsignmentWithItem(bookingHeader, "Waybill2", originLoadList.PK);

			var outerPackage1 = originLoadList.OuterPackages.AddNew();
			outerPackage1.HVO_Volume = 888777.555;
			var outerPackage2 = originLoadList.OuterPackages.AddNew();
			outerPackage2.HVO_Volume = 888777.555;
			consignment1.Items.Cast<HVLVItem>().First().HVI_HVO_OuterPackage = outerPackage1.PK;
			consignment2.Items.Cast<HVLVItem>().First().HVI_HVO_OuterPackage = outerPackage2.PK;

			Factory.Save();

			var task = CreateAndRunTask();
			var log = task.ServiceLogger.Print();
			var loadListInNewFactory = new BusinessObjectFactory().Load<HVLVOriginLoadList>(new ZQuery(HVLVOriginLoadListSchema.HVL_MasterBillNumber, "MAWB"))[0];

			CombineAssertions(() =>
			{
				AssertContains("Error|An error occurred when processing the HVLV Origin Load List 'HVL00000001'. Load List status has been set to 'FAL - Failed'", log);
				AssertContains("Error - JS_ActualChargeable: The number 1,777,555 is too large, the maximum value allowed for Chargeable Weight/Volume is 999,999.999.", log);
				AssertContains("Error - JS_DocumentedChargeable: The number 1,777,555 is too large, the maximum value allowed for Client Declared Shipment Chargeable is 999,999.999.", log);
				AssertContains("Error - JS_ManifestedChargeable: The number 1,777,555 is too large, the maximum value allowed for Carrier / Manifested Shipment Chargeable is 999,999.999.", log);
				AssertEquals(ELoadListStatuses.Failed, loadListInNewFactory.HVL_Status);
			});

			ErrorReporter.Clear();
		}

		[Serializable]
		class FriendlyMessageExceptionForTest : ZDataException
		{
			public FriendlyMessageExceptionForTest(string friendlyMessage)
				: base(new Exception("An exception with friendly message was thrown."), friendlyMessage, debugMessage: string.Empty, row: null, connection: null)
			{
			}

#if NETFRAMEWORK
			protected FriendlyMessageExceptionForTest(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		#endregion

		#region Helper functions

		ForwardingConsol CreateMatchingConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_MasterBillNum = MasterBillNumber;

			return consol;
		}

		ForwardingShipment CreateMatchingHVLShipment(ForwardingConsol consol, ZGuid? masterShipmentPK)
		{
			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorPK = eTailer.PK;
			shipment.JS_RS_NKServiceLevel = "STD";
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKDestination = "IT2C8";
			shipment.JS_JS_ColoadMasterShipment = masterShipmentPK ?? Guid.Empty;
			return shipment;
		}

		ForwardingShipment CreateMatchingHVMShipment(ForwardingConsol consol)
		{
			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorPK = eTailer.PK;
			shipment.JS_RS_NKServiceLevel = "STD";
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValueMaster;
			shipment.JS_RL_NKDestination = "IT2C8";
			shipment.JS_HouseBill = HouseBillNumber;
			return shipment;
		}

		protected HVLVBookingHeader GetBookingHeader()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			bookingHeader.HVH_IsBookingConfirmed = true;
			bookingHeader.HVH_IsBookingReceived = true;
			bookingHeader.HVH_OA_BillToParty = eTailer.MainAddress.PK;
			bookingHeader.HVH_OA_OriginDepot = OriginDepot.PK;
			bookingHeader.HVH_RS_NKBookingServiceLevel = "STD";

			return bookingHeader;
		}

		protected HVLVOriginLoadList GetLoadList(string masterBillNumber)
		{
			var originLoadList = Factory.New<HVLVOriginLoadList>();
			originLoadList.HVL_Status = "LDG";
			originLoadList.HVL_ContainerNumber = "CTNR939230";
			originLoadList.HVL_E_Arv = new ZDateTime(2023, 10, 28);
			originLoadList.HVL_E_Dep = new ZDateTime(2023, 10, 20);
			originLoadList.HVL_HouseBillNumber = HouseBillNumber;
			originLoadList.HVL_INCO = IncoTerms.FreeOnBoard;
			originLoadList.HVL_OA_OriginDepot = OriginDepot.PK;
			originLoadList.HVL_OA_DestinationDepot = DestinationDepot.PK;
			originLoadList.HVL_OH_Carrier = Carrier.PK;
			originLoadList.HVL_OH_Owner = eTailer.PK;
			originLoadList.HVL_RC_ContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			originLoadList.HVL_RL_NKDestination = "DEWVN";
			originLoadList.HVL_RL_NKOrigin = "CHSHA";
			originLoadList.HVL_RS_NKServiceLevel = "STD";
			originLoadList.HVL_TransportMode = TransportModes.Air;
			originLoadList.HVL_VesselName = "Faster pls";
			originLoadList.HVL_VoyageFlight = "QF993";
			originLoadList.HVL_MasterBillNumber = masterBillNumber;

			HVLVTestHelper.SetRefZoneHeader(DestinationDepot.Header.PK, Factory);

			CreateOuterPackage(originLoadList);

			return originLoadList;
		}

		void CreateOuterPackage(HVLVOriginLoadList loadList)
		{
			var outerPackage = loadList.OuterPackages.AddNew();
			outerPackage.HVO_ContainerNumber = loadList.HVL_ContainerNumber;
		}

		HVLVOriginLoadList MakeAndSaveLodgedLoadListWithItem(string masterBillNumber, string uniqueReference = "")
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_Status = "LDG";
			loadList.HVL_UniqueReference = uniqueReference;
			loadList.HVL_MasterBillNumber = masterBillNumber;

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item = consignment.Items.AddNew();
			item.HVI_HVL_LoadList = loadList.PK;

			Factory.Save();

			return loadList;
		}

		HVLVConsignmentHeader GetCreatedConsignmentHeaderByMasterBillNumber(string masterBillNumber)
		{
			var query = new ZQuery();
			query.AddToFilter(JobConsolSchema.JK_MasterBillNum, masterBillNumber);

			var consol = Factory.LoadTop1<ForwardingConsol>(query);
			AssertNotNull("A consol should be created by HLP service task", consol);

			var shipment = consol.Shipments.Single() as ForwardingShipment;
			var header = shipment.GetHVLVConsignmentHeader();

			return header;
		}

		protected HVLVOriginLoadListProcessorServiceTask CreateAndRunTask()
		{
			var task = new HVLVOriginLoadListProcessorServiceTask();
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task);

			return task;
		}

		#endregion

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						HVLVOriginLoadListSchema.Constants.TableName,
						"HVLV Origin Load List Processor",
						HVLVOriginLoadListSchema.Constants.HVL_Status + "=" + HVLVOriginLoadListStatus.Codes.Lodged),
				};
			}
		}

		OrgAddress OriginDepot;
		OrgAddress DestinationDepot;
		OrgHeader Carrier;
		OrgHeader eTailer;
		const string MasterBillNumber = "MBN20231017";
		const string HouseBillNumber = "HBN20231017";

		protected override void SetUpCore()
		{
			OriginDepot = Factory.Load<OrgAddress>(new ZGuid("09B244ED-69D5-44DD-BC13-33DFDA36B41D"));
			DestinationDepot = Factory.Load<OrgAddress>(new ZGuid("2C8DDB59-D769-42E4-BF90-0431A6F29759"));
			Carrier = Factory.Load<OrgHeader>(new ZGuid("9ECE8510-9E97-4310-A0C6-0487877D5FA2"));
			eTailer = Factory.Load<OrgHeader>(new ZGuid("49299E23-8242-49DA-A18D-035077BD7715"));
		}
	}

	class HVLVOriginLoadListProcessorServiceTaskTestForXUS : HVLVOriginLoadListProcessorServiceTaskTest
	{
		public void TestAttachToConsol_WillFailedWhenProcessingXUSAndFallbackToOldSolution()
		{
			const string masterBillNumberExceedsLength11AndWillCausingXUSProcessingFailure = "112233445566";

			var loadList = GetLoadList(masterBillNumberExceedsLength11AndWillCausingXUSProcessingFailure);

			var stdBookingHeader = GetBookingHeader();
			var stdConsignment = stdBookingHeader.Consignments.AddNew();
			stdConsignment.HVC_RN_NKConsigneeCountryCode = "IT";
			var stdItem = stdConsignment.Items.AddNew();
			stdItem.HVI_HVL_LoadList = loadList.PK;

			Factory.Save();

			CreateAndRunTask();

			CombineAssertions("XUS processing should failed and the error should be reported", () =>
			{
				AssertEquals(1, ErrorReporter.TotalErrorCount);
				AssertEquals("Error detected while trying to use Universal XML for load list processing, completed using direct data access instead.", ErrorReporter.LastMessageReported);
			});

			ErrorReporter.Clear();

			var loadListInNewFactory = new BusinessObjectFactory().Load<HVLVOriginLoadList>(new ZQuery(HVLVOriginLoadListSchema.HVL_MasterBillNumber, masterBillNumberExceedsLength11AndWillCausingXUSProcessingFailure))[0];
			AssertEquals("LoadList is processed by old solution", "CON", loadListInNewFactory.HVL_Status);
		}

		bool isEnableXUSForHVLVOriginLoadListProcessingRegistryValue;

		protected override void SetUpCore()
		{
			base.SetUpCore();

			isEnableXUSForHVLVOriginLoadListProcessingRegistryValue = HVLVDataRegistry.Instance.ProcessLoadListUsingUniversalXML.Value;
			HVLVDataRegistry.Instance.ProcessLoadListUsingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDownCore()
		{
			HVLVDataRegistry.Instance.ProcessLoadListUsingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isEnableXUSForHVLVOriginLoadListProcessingRegistryValue);
		}
	}
}
