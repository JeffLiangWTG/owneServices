using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class WhsTransitLoadListEventParentFinderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestMatchLoadList_TransportModeIsNotAir()
		{
			var dll = CreateDispatchLoadList();
			dll.WDL_TransportMode = "SEA";

			var mawbNumber = dll.AdditionalReferenceNumbers.AddNew();
			mawbNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			mawbNumber.CE_EntryNum = "831-12345678";
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(ShipmentEventXMLBuilder());
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var dlls = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should not find non-Air Load List", 0, dlls.Length);
			AssertEquals("Should have log errors", true, logger.HasErrors());
			AssertContains($@"Information - Found warehouse 'AUS' matching UNLOCO 'AUSYD'.
Error - No matching load lists found by consol number '' or master bill number '831-12345678'.", logger.Logs);
		}

		public void TestMatchLoadList_MAWBIsNotMatched()
		{
			var dll = CreateDispatchLoadList();
			dll.WDL_TransportMode = "AIR";

			var mawbNumber = dll.AdditionalReferenceNumbers.AddNew();
			mawbNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			mawbNumber.CE_EntryNum = "831-00000000";
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(ShipmentEventXMLBuilder());
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var dlls = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should not find Load List when MAWB is not matched", 0, dlls.Length);
			AssertEquals("Should have log errors", true, logger.HasErrors());
			AssertContains($@"Information - Found warehouse 'AUS' matching UNLOCO 'AUSYD'.
Error - No matching load lists found by consol number '' or master bill number '831-12345678'.", logger.Logs);
		}

		public void TestMatchLoadList_MatchConsolNumber()
		{
			var dll = CreateDispatchLoadList();
			dll.WDL_TransportMode = "AIR";
			var consolNumber = dll.AdditionalReferenceNumbers.AddNew();
			consolNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber;
			consolNumber.CE_EntryNum = "C00001566";
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(ShipmentEventXMLBuilder(consolNumber: "C00001566"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var dlls = finder.GetLogParentsForEvent(xmlEvent);
			AssertContainsExactElementsInAnyOrder(new[] { dll }, dlls);
			AssertEquals("Should not have log errors", false, logger.HasErrors());
			AssertContains($@"Information - Found warehouse 'AUS' matching UNLOCO 'AUSYD'.
Information - The following load list(s) matching consol number 'C00001566'.
Load List
(DLL1)", logger.Logs);
		}

		public void TestMatchLoadList_MatchReferenceNumber()
		{
			var dll = CreateDispatchLoadList(referenceNumber: "C0001");
			dll.WDL_TransportMode = "AIR";
			var consolNumber = dll.AdditionalReferenceNumbers.AddNew();
			consolNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber;
			consolNumber.CE_EntryNum = "C00002";
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(StopLoadEventXMLBuilder(consolNumber: "C0001"));
			
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);
			var dlls = finder.GetLogParentsForEvent(xmlEvent);

			var query = new ZQuery(StmALogSchema.SL_Parent, dll.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdatedCode);
			var stmLog = Factory.RowFactory.Load(StmALogSchema.Constants.TableName, query);
			var reference = stmLog.FirstOrDefault()[StmALogSchema.Constants.SL_Reference];

			AssertContainsExactElementsInAnyOrder(new[] { dll }, dlls);
			AssertEquals("reference should contain reference", "|TYP=STOP LOAD|WHS=AUS|JOB=DLL1|RFN=C0001", reference);
			AssertEquals("Should not have log errors", false, logger.HasErrors());
			AssertContains($@"Information - Found warehouse 'AUS' matching UNLOCO 'AUSYD'.
Information - The following load list(s) matching consol number 'C0001'.
Load List
(DLL1)", logger.Logs);
		}

		public void TestMatchLoadList_RecipientRoleIsNotDTW()
		{
			var dll = CreateDispatchLoadList();
			dll.WDL_TransportMode = "AIR";

			var mawbNumber = dll.AdditionalReferenceNumbers.AddNew();
			mawbNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			mawbNumber.CE_EntryNum = "831-00000000";
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(ShipmentEventXMLBuilder("ATW"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var dlls = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should not find Load List when Recipient Role is not DTW", 0, dlls.Length);
		}

		public void TestMatchLoadList_CannotFindWarehouseByUNLOCO()
		{
			var dll = CreateDispatchLoadList(warehouseUnloco: "CNNJG");
			dll.WDL_TransportMode = "AIR";

			var mawbNumber = dll.AdditionalReferenceNumbers.AddNew();
			mawbNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			mawbNumber.CE_EntryNum = "831-12345678";
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(ShipmentEventXMLBuilder());
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var dlls = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should not find Load List when warehouse is not matched", 0, dlls.Length);
			AssertEquals("Should have log errors", true, logger.HasErrors());
			AssertContains($@"Error - No matching Transit Warehouse by UNLOCO 'AUSYD'.", logger.Logs);
		}

		public void TestMatchLoadList_MatchWarehouseByCountryCode()
		{
			var dll = CreateDispatchLoadList(warehouseUnloco: "AUMEL");
			dll.WDL_TransportMode = "AIR";

			var branch2 = Helper.CreateGlbBranch("B2");
			var warehouse2 = CreateWarehouse("AUBNE", branch2);

			var mawbNumber = dll.AdditionalReferenceNumbers.AddNew();
			mawbNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			mawbNumber.CE_EntryNum = "831-12345678";
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(ShipmentEventXMLBuilder());
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var dlls = finder.GetLogParentsForEvent(xmlEvent);
			AssertContainsExactElementsInAnyOrder(new[] { dll }, dlls);
			AssertEquals("Should not have log errors", false, logger.HasErrors());
			AssertContains($@"Information - Found 2 warehouses matching UNLOCO 'AUSYD': AUB, AUM.
Information - The following load list(s) matching master bill number '831-12345678'.
Load List
831-12345678 (DLL1)", logger.Logs);
		}

		public void TestMatchLoadList()
		{
			var dll = CreateDispatchLoadList();
			dll.WDL_TransportMode = "AIR";

			var mawbNumber = dll.AdditionalReferenceNumbers.AddNew();
			mawbNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			mawbNumber.CE_EntryNum = "831-12345678";
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(ShipmentEventXMLBuilder());
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var dlls = finder.GetLogParentsForEvent(xmlEvent);
			AssertContainsExactElementsInAnyOrder(new[] { dll }, dlls);
			AssertEquals("Should not have log errors", false, logger.HasErrors());
			AssertContains($@"Information - Found warehouse 'AUS' matching UNLOCO 'AUSYD'.
Information - The following load list(s) matching master bill number '831-12345678'.
Load List
831-12345678 (DLL1)", logger.Logs);
		}

		public void TestMatchLoadList_MAWBNumberWithoutHyphen()
		{
			var dll = CreateDispatchLoadList();
			dll.WDL_TransportMode = "AIR";

			var mawbNumber = dll.AdditionalReferenceNumbers.AddNew();
			mawbNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			mawbNumber.CE_EntryNum = "83112345678";
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(ShipmentEventXMLBuilder());
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var dlls = finder.GetLogParentsForEvent(xmlEvent);
			AssertContainsExactElementsInAnyOrder(new[] { dll }, dlls);
			AssertEquals("Should not have log errors", false, logger.HasErrors());
			AssertContains($@"Information - Found warehouse 'AUS' matching UNLOCO 'AUSYD'.
Information - The following load list(s) matching master bill number '831-12345678'.
Load List
83112345678 (DLL1)", logger.Logs);
		}

		public void TestMatchLoadList_MultipleDLLs()
		{
			var dll = CreateDispatchLoadList();
			dll.WDL_TransportMode = "AIR";

			var mawbNumber = dll.AdditionalReferenceNumbers.AddNew();
			mawbNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			mawbNumber.CE_EntryNum = "831-12345678";

			var dll2 = Helper.CreateDispatchLoadList("DLL2", dll.WDL_WW_Warehouse);
			dll2.WDL_TransportMode = "AIR";

			var mawbNumber2 = dll2.AdditionalReferenceNumbers.AddNew();
			mawbNumber2.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			mawbNumber2.CE_EntryNum = "831-12345678";
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(ShipmentEventXMLBuilder());
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var dlls = finder.GetLogParentsForEvent(xmlEvent);
			AssertContainsExactElementsInAnyOrder(new[] { dll, dll2 }, dlls);
			AssertEquals("Should not have log errors", false, logger.HasErrors());
			AssertContains($@"Information - Found warehouse 'AUS' matching UNLOCO 'AUSYD'.
Information - The following load list(s) matching master bill number '831-12345678'.
Load List
831-12345678 (DLL1)
831-12345678 (DLL2)", logger.Logs);
		}

		public void TestMatchLoadList_MultipleDLLs_OneInAnotherWarehouse()
		{
			var branch = Helper.CreateGlbBranch("BBB");
			var warehouse2 = Helper.CreateWarehouse("WH2", "WH2", "A");
			warehouse2.WW_GB_RelatedCompanyBranch = branch.PK;
			warehouse2.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			warehouse2.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUMEL";
			Factory.SaveForTesting();

			var dll = CreateDispatchLoadList();
			dll.WDL_TransportMode = "AIR";

			var mawbNumber = dll.AdditionalReferenceNumbers.AddNew();
			mawbNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			mawbNumber.CE_EntryNum = "831-12345678";

			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse2.PK);
			dll2.WDL_TransportMode = "AIR";

			var mawbNumber2 = dll2.AdditionalReferenceNumbers.AddNew();
			mawbNumber2.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			mawbNumber2.CE_EntryNum = "831-12345678";
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(ShipmentEventXMLBuilder());
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var dlls = finder.GetLogParentsForEvent(xmlEvent);
			AssertContainsExactElementsInAnyOrder(new[] { dll }, dlls);
			AssertEquals("Should not have log errors", false, logger.HasErrors());
			AssertContains($@"Information - Found warehouse 'AUS' matching UNLOCO 'AUSYD'.
Information - The following load list(s) matching master bill number '831-12345678'.
Load List
831-12345678 (DLL1)", logger.Logs);
		}

		public void TestMatchLoadList_UseStopLoadXml_DepartureWarehouse()
		{
			var dll = CreateDispatchLoadList();

			var mawbNumber = dll.AdditionalReferenceNumbers.AddNew();
			mawbNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			mawbNumber.CE_EntryNum = "831-12345678";
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(StopLoadEventXMLBuilder());
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var dlls = finder.GetLogParentsForEvent(xmlEvent);
			AssertContainsExactElementsInAnyOrder(new[] { dll }, dlls);

			var dllFound = Factory.Load<WhsItemDispatchLoadList>(dlls.First().PK);
			AssertEquals("Is Awaiting Forwarding Changes shall be true", true, dllFound.WDL_IsAwaitingForwardingChanges);
			AssertEquals("Is Ready To Stage shall be false", false, dllFound.WDL_IsReadyToStage);
			AssertEquals("Complete time shall be empty", ZDateTimeOffset.Empty, dllFound.WDL_CompleteTime);
			AssertEquals("Should not have log errors", false, logger.HasErrors());
			AssertContains($@"Information - Found warehouse 'AUS' matching UNLOCO 'AUSYD'.
Information - The following load list(s) matching master bill number '831-12345678'.
Load List
831-12345678 (DLL1)", logger.Logs);
			Factory.SaveForTesting();

			logger.ClearLogs();

			var newDlls = finder.GetLogParentsForEvent(xmlEvent);
			AssertContainsExactElementsInAnyOrder(new[] { dll }, newDlls);

			var dllFound2 = Factory.Load<WhsItemDispatchLoadList>(newDlls.First().PK);
			AssertEquals("Is Awaiting Forwarding Changes shall be true", true, dllFound2.WDL_IsAwaitingForwardingChanges);
			AssertEquals("Is Ready To Stage shall be false", false, dllFound2.WDL_IsReadyToStage);
			AssertEquals("Complete time shall be empty", ZDateTimeOffset.Empty, dllFound2.WDL_CompleteTime);
			AssertEquals("Should not have log errors", false, logger.HasErrors());
			AssertContains($@"Information - Found warehouse 'AUS' matching UNLOCO 'AUSYD'.
Information - The following load list(s) matching master bill number '831-12345678'.
Load List
831-12345678 (DLL1)
Information - Load List DLL1 already Stopped.", logger.Logs);
		}

		public void TestMatchLoadList_UseStopLoadXml_ArrivalWarehouse()
		{
			var dll = CreateDispatchLoadList();

			var mawbNumber = dll.AdditionalReferenceNumbers.AddNew();
			mawbNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			mawbNumber.CE_EntryNum = "831-12345678";
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(StopLoadEventXMLBuilder("ATW"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var dlls = finder.GetLogParentsForEvent(xmlEvent);
			AssertContainsExactElementsInAnyOrder(new[] { dll }, dlls);

			var dllFound = (WhsItemDispatchLoadList)dlls.First();
			AssertEquals("Is Awaiting Forwarding Changes shall be true", true, dllFound.WDL_IsAwaitingForwardingChanges);
			AssertEquals("Is Ready To Stage shall be false", false, dllFound.WDL_IsReadyToStage);
			AssertEquals("Complete time shall be empty", ZDateTimeOffset.Empty, dllFound.WDL_CompleteTime);
			AssertEquals("Should not have log errors", false, logger.HasErrors());
			AssertContains($@"Information - Found warehouse 'AUS' matching UNLOCO 'AUSYD'.
Information - The following load list(s) matching master bill number '831-12345678'.
Load List
831-12345678 (DLL1)", logger.Logs);
			Factory.SaveForTesting();

			logger.ClearLogs();

			var newDlls = finder.GetLogParentsForEvent(xmlEvent);
			AssertContainsExactElementsInAnyOrder(new[] { dll }, newDlls);

			var dllFound2 = Factory.Load<WhsItemDispatchLoadList>(newDlls.First().PK);
			AssertEquals("Is Awaiting Forwarding Changes shall be true", true, dllFound2.WDL_IsAwaitingForwardingChanges);
			AssertEquals("Is Ready To Stage shall be false", false, dllFound2.WDL_IsReadyToStage);
			AssertEquals("Complete time shall be empty", ZDateTimeOffset.Empty, dllFound2.WDL_CompleteTime);
			AssertEquals("Should not have log errors", false, logger.HasErrors());
			AssertContains($@"Information - Found warehouse 'AUS' matching UNLOCO 'AUSYD'.
Information - The following load list(s) matching master bill number '831-12345678'.
Load List
831-12345678 (DLL1)
Information - Load List DLL1 already Stopped.", logger.Logs);
		}

		public void TestMatchLoadList_UseStopLoadXml_DifferentMasterBillType()
		{
			var dll = CreateDispatchLoadList();

			var mawbNumber = dll.AdditionalReferenceNumbers.AddNew();
			mawbNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			mawbNumber.CE_EntryNum = "831-12345678";
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(StopLoadEventXMLBuilder(masterBillType: "MBOLNumber"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var dlls = finder.GetLogParentsForEvent(xmlEvent);
			AssertContainsExactElementsInAnyOrder(new[] { dll }, dlls);

			var dllFound = (WhsItemDispatchLoadList)dlls.First();
			AssertEquals("Is Awaiting Forwarding Changes shall be true", true, dllFound.WDL_IsAwaitingForwardingChanges);
			AssertEquals("Is Ready To Stage shall be false", false, dllFound.WDL_IsReadyToStage);
			AssertEquals("Complete time shall be empty", ZDateTimeOffset.Empty, dllFound.WDL_CompleteTime);
			AssertEquals("Should not have log errors", false, logger.HasErrors());
			AssertContains($@"Information - Found warehouse 'AUS' matching UNLOCO 'AUSYD'.
Information - The following load list(s) matching master bill number '831-12345678'.
Load List
831-12345678 (DLL1)", logger.Logs);
		}

		public void TestMatchLoadList_UseStopLoadXml_CancelStopLoad()
		{
			var dll = CreateDispatchLoadList();
			dll.WDL_IsReadyToStage = false;
			dll.WDL_IsAwaitingForwardingChanges = true;

			var mawbNumber = dll.AdditionalReferenceNumbers.AddNew();
			mawbNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			mawbNumber.CE_EntryNum = "831-12345678";
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(StopLoadEventXMLBuilder(eventType: "SVR"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var dlls = finder.GetLogParentsForEvent(xmlEvent);
			AssertContainsExactElementsInAnyOrder(new[] { dll }, dlls);

			var dllFound = (WhsItemDispatchLoadList)dlls.First();
			AssertEquals("Is Awaiting Forwarding Changes shall be false", false, dllFound.WDL_IsAwaitingForwardingChanges);
			AssertEquals("Should not have log errors", false, logger.HasErrors());
			AssertContains($@"Information - Found warehouse 'AUS' matching UNLOCO 'AUSYD'.
Information - The following load list(s) matching master bill number '831-12345678'.
Load List
831-12345678 (DLL1)", logger.Logs);
			Factory.SaveForTesting();

			logger.ClearLogs();
			var newDlls = finder.GetLogParentsForEvent(xmlEvent);
			AssertContainsExactElementsInAnyOrder(new[] { dll }, newDlls);

			var dllFound2 = (WhsItemDispatchLoadList)dlls.First();
			AssertEquals("Is Awaiting Forwarding Changes shall be false", false, dllFound2.WDL_IsAwaitingForwardingChanges);
			AssertEquals("Should not have log errors", false, logger.HasErrors());
			AssertContains($@"Information - Found warehouse 'AUS' matching UNLOCO 'AUSYD'.
Information - The following load list(s) matching master bill number '831-12345678'.
Load List
831-12345678 (DLL1)
Information - Load List DLL1 already Cancelled.", logger.Logs);
		}

		public void TestMatchLoadList_UseStopLoadXml_WrongRecipientRole()
		{
			var dll = CreateDispatchLoadList();

			var mawbNumber = dll.AdditionalReferenceNumbers.AddNew();
			mawbNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			mawbNumber.CE_EntryNum = "831-12345678";
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(StopLoadEventXMLBuilder("TWD"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var dlls = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should contains 0 DLL", 0, dlls.Length);
		}

		public void TestMatchLoadList_UseStopLoadXml_WrongEventType()
		{
			var dll = CreateDispatchLoadList();

			var mawbNumber = dll.AdditionalReferenceNumbers.AddNew();
			mawbNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			mawbNumber.CE_EntryNum = "831-12345678";
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(StopLoadEventXMLBuilder(eventType: "CNC"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var dlls = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Should contains 0 DLL", 0, dlls.Length);
		}

		public void TestMatchLoadList_UseStopLoadXml_Concurrency()
		{
			var dll = CreateDispatchLoadList();
			dll.WDL_IsReadyToStage = false;
			dll.WDL_IsAwaitingForwardingChanges = true;
			var mawbNumber = dll.AdditionalReferenceNumbers.AddNew();
			mawbNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			mawbNumber.CE_EntryNum = "831-12345678";
			Factory.SaveForTesting();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var dllInNewFactory = factory2.Load<WhsItemDispatchLoadList>(dll.PK);
			dllInNewFactory.WDL_IsAwaitingForwardingChanges = false;
			factory2.Save();

			var xmlEvent = EventDeserializer.Parse(StopLoadEventXMLBuilder());
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var dlls = finder.GetLogParentsForEvent(xmlEvent);
			AssertContainsExactElementsInAnyOrder(new[] { dll }, dlls);

			var dlla = (WhsItemDispatchLoadList)dlls.First();
			var dllFound = Factory.Load<WhsItemDispatchLoadList>(dlla.PK);
			AssertEquals("Is Awaiting Forwarding Changes shall be true", true, dllFound.WDL_IsAwaitingForwardingChanges);
			AssertEquals("Is Ready To Stage shall be false", false, dllFound.WDL_IsReadyToStage);
		}

		public void TestMatchLoadList_UseStopLoadXml_DepartureWarehouse_AutoEventsNotMatched()
		{
			var dll = CreateDispatchLoadList();

			var mawbNumber = dll.AdditionalReferenceNumbers.AddNew();
			mawbNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			mawbNumber.CE_EntryNum = "831-12345678";
			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(StopLoadEventXMLBuilder(eventType: "CEN"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var dlls = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals("no entity found", 0, dlls.Length);
		}

		#region Univsersal Event XML

		string ShipmentEventXMLBuilder(string recipientRole = "DTW", string consolNumber = "") => $@"
<UniversalEvent>
	<Event>
		<EventTime>2016-12-25T01:02:03.001</EventTime>
		<EventType>DEP</EventType>
		<EventReference>FAC=TWS|LOC=AUSYD</EventReference>
		<DataContext>
			<DataSourceCollection>
				<DataSource>
					<Type>ForwardingConsol</Type>
					<Key>{consolNumber}</Key>
				</DataSource>
			</DataSourceCollection>
			<Company>
				<Code>TMS</Code>
			</Company>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>EVP</ServerID>
			<RecipientRoleCollection>
				<RecipientRole>
					<Code>{recipientRole}</Code>
				</RecipientRole>
			</RecipientRoleCollection>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>MAWBNumber</Type>
				<Value>831-12345678</Value>
			</Context>
			<Context>
				<Type>MAWBOriginIATAAirportCode</Type>
				<Value>SYD</Value>
			</Context>
			<Context>
				<Type>MBOLOriginUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>MBOLDestinationUNLOCO</Type>
				<Value>CNNJG</Value>
			</Context>
			<Context>
				<Type>HAWBNumber</Type>
				<Value>C00001566</Value>
			</Context>
			<Context>
				<Type>HAWBOriginIATAAirportCode</Type>
				<Value>SYD</Value>
			</Context>
			<Context>
				<Type>HBOLOriginUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		string StopLoadEventXMLBuilder(string recipientRole = "DTW", string eventType = "SVS", string masterBillType = "MAWBNumber", string consolNumber = "") => $@"
<UniversalEvent>
	<Event>
		<EventTime>2022-04-25T01:02:03.001</EventTime>
		<EventType>{eventType}</EventType>
		<EventReference>FAC=TWS|LOC=AUSYD|TYP=Load</EventReference>
		<DataContext>
			<DataSourceCollection>
				<DataSource>
					<Type>ForwardingConsol</Type>
					<Key>{consolNumber}</Key>
				</DataSource>
			</DataSourceCollection>
			<Company>
				<Code>TMS</Code>
			</Company>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>EVP</ServerID>
			<RecipientRoleCollection>
				<RecipientRole>
					<Code>{recipientRole}</Code>
				</RecipientRole>
			</RecipientRoleCollection>
		</DataContext>
		<ContextCollection>
			<Context>
				<Type>{masterBillType}</Type>
				<Value>831-12345678</Value>
			</Context>
			<Context>
				<Type>MAWBOriginIATAAirportCode</Type>
				<Value>SYD</Value>
			</Context>
			<Context>
				<Type>MBOLOriginUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>MBOLDestinationUNLOCO</Type>
				<Value>CNNJG</Value>
			</Context>
			<Context>
				<Type>HAWBNumber</Type>
				<Value>C00001566</Value>
			</Context>
			<Context>
				<Type>HAWBOriginIATAAirportCode</Type>
				<Value>SYD</Value>
			</Context>
			<Context>
				<Type>HBOLOriginUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		#endregion

		#region Implementation

		WhsItemDispatchLoadList CreateDispatchLoadList(string warehouseUnloco = "AUSYD", string referenceNumber = null)
		{
			var warehouse = CreateWarehouse(warehouseUnloco);
			var stageLocation = warehouse.DefaultLocation;
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK, stageLocation, true);
			dll.WDL_ReferenceNumber = referenceNumber ?? dll.WDL_JobID;
			Factory.SaveForTesting();

			return dll;
		}

		WhsWarehouse CreateWarehouse(string addressUnloco = "AUSYD", GlbBranch branch = null)
		{
			var warehoueCode = addressUnloco.Substring(0, 3);
			var warehouse = Helper.CreateWarehouse(addressUnloco, warehoueCode, "A");
			warehouse.WW_GB_RelatedCompanyBranch = branch != null ? branch.PK : GlbBranch.CurrentBranch.PK;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = addressUnloco;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = addressUnloco.Substring(0, 2);

			Factory.SaveForTesting();

			return warehouse;
		}

		public EventParentFinder GetFinder(IXmlImportLogger logger = null)
		{
			return new WhsTransitLoadListEventParentFinder(Factory.BOFactory, new WhsTransitDispatchLoadListDataContextManager(), logger ?? new TestErrorLogger());
		}

		XmlEventDeserializer EventDeserializer => eventDeserializer ?? (eventDeserializer = new XmlEventDeserializer());
		XmlEventDeserializer eventDeserializer;

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;

		#endregion
	}
}
