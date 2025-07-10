using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ACEManifest.Business.UniversalDataTransfer.Testing
{
	sealed class ACEAsycudaManifestDataObjectReaderHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetBillDataObjectReader_WhenHeaderInDatabase()
		{
			var readerHelper = new ACEAsycudaManifestDataObjectReaderHelper(Factory.BOFactory);
			var header = Factory.BOFactory.New<AsycudaManifestHeader>();
			var helper = new AsycudaManifestDataObjectReaderTestHelper();
			var usAirLocalPort1 = helper.GetAirLocalPort1("US");
			var usAirLocalPort2 = helper.GetAirLocalPort2(usAirLocalPort1.PK);
			var portOfLoading = new UNLOCO { Code = usAirLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO { Code = usAirLocalPort2.RL_Code };
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "BIL00001";
			var billDataObject = helper.SetupBill("BIL00001", portOfLoading, portOfDischarge, 300m, "Goods Desc", 3m, "Carrier Reference", "STD", "PRE");
			var billCountryEntryHeader = helper.SetupCountryBillEntryHeader("US", 1, "CLR", "BIL00001");
			billDataObject.SetEntryHeaderCollection(() => new List<EntryHeader> { billCountryEntryHeader });
			var codeDescription = new CodeDescriptionPair();
			codeDescription.Code = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			billDataObject.ShipmentType = codeDescription;
			Factory.SaveForTesting();
			Assert("Precondition - Header in Database", header.IsInDatabase);
			var billDataObjectReader = readerHelper.GetBillDataObjectReader(billDataObject, new DummyLogger(), Factory, header, readerHelper, false);
			CombineAssertions(() =>
			{
				AssertType<HVLVACEAsycudaBillDataObjectReader>(billDataObjectReader);
				var dynMethod = billDataObjectReader.GetType().GetMethod("GetExistingBusinessObjectUsingModuleSpecificBusinessRules", BindingFlags.NonPublic | BindingFlags.Instance);
				AssertNotNull(dynMethod);
				var result = dynMethod.Invoke(billDataObjectReader, System.Array.Empty<object>());
				AssertType<AsycudaBill>(result);
			});
		}

		public void TestGetBillDataObjectReader_WhenHeaderNotInDatabase()
		{
			var readerHelper = new ACEAsycudaManifestDataObjectReaderHelper(Factory.BOFactory);
			var header = Factory.BOFactory.New<AsycudaManifestHeader>();
			var helper = new AsycudaManifestDataObjectReaderTestHelper();
			var usAirLocalPort1 = helper.GetAirLocalPort1("US");
			var usAirLocalPort2 = helper.GetAirLocalPort2(usAirLocalPort1.PK);
			var portOfLoading = new UNLOCO { Code = usAirLocalPort1.RL_Code };
			var portOfDischarge = new UNLOCO { Code = usAirLocalPort2.RL_Code };
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "BIL00001";
			var billDataObject = helper.SetupBill("BIL00001", portOfLoading, portOfDischarge, 300m, "Goods Desc", 3m, "Carrier Reference", "STD", "PRE");
			var billCountryEntryHeader = helper.SetupCountryBillEntryHeader("US", 1, "CLR", "BIL00001");
			billDataObject.SetEntryHeaderCollection(() => new List<EntryHeader> { billCountryEntryHeader });
			Assert("Precondition - Header not in Database", !header.IsInDatabase);
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var codeDescription = new CodeDescriptionPair();
			codeDescription.Code = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			shipment.ShipmentType = codeDescription;
			var billDataObjectReader = readerHelper.GetBillDataObjectReader(shipment, new DummyLogger(), Factory, header, readerHelper, false);
			CombineAssertions(() =>
			{
				AssertType<HVLVACEAsycudaBillDataObjectReader>(billDataObjectReader);
				var dynMethod = billDataObjectReader.GetType().GetMethod("GetExistingBusinessObjectUsingModuleSpecificBusinessRules", BindingFlags.NonPublic | BindingFlags.Instance);
				var result = dynMethod.Invoke(billDataObjectReader, System.Array.Empty<object>());
				AssertNull(result);
			});
		}

		public void TestImportFDAIndicator()
		{
			var helper = new AsycudaManifestDataObjectReaderTestHelper();
			var headerDataObject = helper.SetupManifestHeader("MAST0006", new UNLOCO()
			{ Code = "AUSYD" }, new UNLOCO()
			{ Code = "USLAX" }, ZDateTime.Today, ZDateTime.Today.AddDays(1), "", "IAM");
			var headerEntryHeader = helper.SetupCountryHeaderEntryHeader("US", 1);
			var headerEntryInstruction = helper.SetupCountryHeaderEntryInstruction(1, new UNLOCO()
			{ Code = "USLAX" }, "OTT1", "US", "IAM", "NT1");
			headerDataObject.SetEntryHeaderCollection(() => new List<EntryHeader>());
			headerDataObject.EntryHeaderCollection.Add(headerEntryHeader);
			headerDataObject.SetEntryInstructionCollection(() => new List<EntryInstruction>());
			headerDataObject.EntryInstructionCollection.Add(headerEntryInstruction);
			var billDataObject = helper.SetupBill("BIL00001", new UNLOCO()
			{ Code = "AUMEL" }, new UNLOCO()
			{ Code = "USCHI" }, 300m, "Goods Desc", 3m, "Carrier Reference", "STD", "PRE");
			var billCountryEntryHeader = helper.SetupCountryBillEntryHeader("US", 1, "CLR", "Sender Reference1");
			var billCountryEntryInstruction = helper.SetupCountryBillEntryInstruction(1, "Goods Location1", "Location Information1", "IMP", "US", "OTT1", 200.01m, 300.01m, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty);
			billCountryEntryInstruction.AddInfoCollection.Add(new AddInfo()
			{ Key = AddInfoConstants.BillCountry.FDAIndicator, Value = "Y" });
			billDataObject.SetEntryHeaderCollection(() => new List<EntryHeader>());
			billDataObject.EntryHeaderCollection.Add(billCountryEntryHeader);
			billDataObject.SetEntryInstructionCollection(() => new List<EntryInstruction>());
			billDataObject.EntryInstructionCollection.Add(billCountryEntryInstruction);
			headerDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			headerDataObject.SubShipmentCollection.Add(billDataObject);
			var message = GetQueuedUniversalShipmentMessage(headerDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();
			var query = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			subQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, "BOL");
			subQuery.AddToFilter(AsycudaBillSchema.ABL_BillNumber, "MAST0006");
			query.AddSubQuery(subQuery, JoinCondition.And);
			var headerBO = Factory.LoadTop1<AsycudaManifestHeader>(query);
			AssertEquals("headerBO.Bills.Count", 1, headerBO.Bills.Count);
			var billBO = headerBO.Bills[0];
			AssertEquals("billCountryBO.FDAIndicator", ZBool.True, billBO.FDAIndicator);
		}

		public void TestImportEstDateAtFirstArrival()
		{
			var helper = new AsycudaManifestDataObjectReaderTestHelper();
			var headerDataObject = helper.SetupManifestHeader("MAST0006", new UNLOCO()
			{ Code = "AUSYD" }, new UNLOCO()
			{ Code = "USLAX" }, ZDateTime.Today, ZDateTime.Today.AddDays(1), "", "IAM");
			var headerEntryHeader = helper.SetupCountryHeaderEntryHeader("US", 1);
			var headerEntryInstruction = helper.SetupCountryHeaderEntryInstruction(1, new UNLOCO()
			{ Code = "USLAX" }, "OTT1", "US", "IAM", "NT1");
			headerDataObject.SetEntryHeaderCollection(() => new List<EntryHeader>());
			headerDataObject.EntryHeaderCollection.Add(headerEntryHeader);
			headerDataObject.SetEntryInstructionCollection(() => new List<EntryInstruction>());
			headerDataObject.EntryInstructionCollection.Add(headerEntryInstruction);
			headerDataObject.DateCollection.Add(new Date()
			{ Type = DateType.FirstArrivalInCountry, Value = ZDateTime.Today });
			var billDataObject = helper.SetupBill("BIL00001", new UNLOCO()
			{ Code = "AUMEL" }, new UNLOCO()
			{ Code = "USCHI" }, 300m, "Goods Desc", 3m, "Carrier Reference", "STD", "PRE");
			var billCountryEntryHeader = helper.SetupCountryBillEntryHeader("US", 1, "CLR", "Sender Reference1");
			var billCountryEntryInstruction = helper.SetupCountryBillEntryInstruction(1, "Goods Location1", "Location Information1", "IMP", "US", "OTT1", 200.01m, 300.01m, ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty);
			billCountryEntryInstruction.AddInfoCollection.Add(new AddInfo()
			{ Key = AddInfoConstants.BillCountry.FDAIndicator, Value = "Y" });
			billDataObject.SetEntryHeaderCollection(() => new List<EntryHeader>());
			billDataObject.EntryHeaderCollection.Add(billCountryEntryHeader);
			billDataObject.SetEntryInstructionCollection(() => new List<EntryInstruction>());
			billDataObject.EntryInstructionCollection.Add(billCountryEntryInstruction);
			headerDataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			headerDataObject.SubShipmentCollection.Add(billDataObject);
			var message = GetQueuedUniversalShipmentMessage(headerDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();
			var query = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			subQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, "BOL");
			subQuery.AddToFilter(AsycudaBillSchema.ABL_BillNumber, "MAST0006");
			query.AddSubQuery(subQuery, JoinCondition.And);
			var headerBO = Factory.LoadTop1<AsycudaManifestHeader>(query);
			AssertEquals("headerBO.Bills.Count", 1, headerBO.Bills.Count);
			AssertEquals("EstDateAtFirstArrival", ZDateTime.Today, headerBO.EstDateAtFirstArrival);
		}

		public void TestFiltersContainerMode()
		{
			var setColumns = new List<SchemaColumn>();
			var helper = new ACEAsycudaManifestDataObjectReaderHelper(Factory.BOFactory);
			var shipment = new AsycudaManifestDataObjectReaderTestHelper().SetupManifestHeader("MAN00001", null, null, ZDateTime.UtcNow, ZDateTime.UtcNow, "", "IAM");
			CombineAssertions(() =>
			{
				shipment.TransportMode = new CodeDescriptionPair()
				{ Code = TransportTypeList.Codes.Air, Description = TransportTypeList.Descriptions.Air };
				foreach (var col in AsycudaManifestHeaderSchema.All)
				{
					helper.FilterAndSet(col, shipment, (_) => setColumns.Add(_));
				}

				setColumns.Add(AsycudaManifestHeaderSchema.AMA_ContainerMode);
				AssertContainsExactElementsInAnyOrder("Every column should be set except AMA_ContainerMode with Air", AsycudaManifestHeaderSchema.All.Select(_ => _.Name), setColumns.Select(_ => _.Name));
				setColumns.Clear();
				shipment.TransportMode = new CodeDescriptionPair()
				{ Code = TransportTypeList.Codes.Sea, Description = TransportTypeList.Descriptions.Sea };
				foreach (var col in AsycudaManifestHeaderSchema.All)
				{
					helper.FilterAndSet(col, shipment, (_) => setColumns.Add(_));
				}

				AssertContainsExactElementsInAnyOrder("Every column should be set including AMA_ContainerMode with Sea", AsycudaManifestHeaderSchema.All.Select(_ => _.Name), setColumns.Select(_ => _.Name));
			});
		}
	}
}
