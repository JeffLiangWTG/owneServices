using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Manifest.Business.UniversalDataTransfer.Testing
{
	class ZAAsycudaManifestDataObjectReaderHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestImportAsycudaManifestHeaderFields()
		{
			ASYCUDA.Business.Testing.ZZDataTestHelper.SetupZZ(new BusinessObjectFactory(), Core.Constants.CountryCodes.SouthAfrica);
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var usAirLocalPort1 = help.GetAirLocalPort1("US");
			var zaAirLocalPort1 = help.GetAirLocalPort1("ZA");
			var usFirstArrival = new UNLOCO { Code = usAirLocalPort1.RL_Code };
			var zaFirstArrival = new UNLOCO { Code = zaAirLocalPort1.RL_Code };
			var headerDataObject = help.SetupManifestHeader("MAST0006", usFirstArrival, zaFirstArrival, new ZDateTime(2018, 2, 10), new ZDateTime(2018, 2, 1), "", ZaManifestTypes.Codes.HAB);
			var headerEntryHeader = help.SetupCountryHeaderEntryHeader(Core.Constants.CountryCodes.SouthAfrica, 1);
			var headerEntryInstruction = help.SetupCountryHeaderEntryInstruction(1, usFirstArrival, "OTT1", Core.Constants.CountryCodes.SouthAfrica, nameof(ManifestDocumentType.ALH), "NT1");
			headerEntryInstruction.AddInfoCollection.Add(new AddInfo { Key = AsycudaManifestHeader.Schema.PlaceOfEntry, Value = "PEN" });
			headerEntryInstruction.AddInfoCollection.Add(new AddInfo { Key = GenAddOnHelper.PlaceOfExitCode, Value = "PEX" });
			headerDataObject.SetEntryHeaderCollection(() => new List<UniversalDataBuss.DataObjects.Universal.Customs.EntryHeader>());
			headerDataObject.EntryHeaderCollection.Add(headerEntryHeader);
			headerDataObject.SetEntryInstructionCollection(() => new List<UniversalDataBuss.DataObjects.Universal.Customs.EntryInstruction>());
			headerDataObject.EntryInstructionCollection.Add(headerEntryInstruction);
			headerDataObject.DateCollection.Add(new Date { Type = DateType.LoadingDate, Value = ZDateTime.MaxSmallDateTimeValue, IsEstimate = ZBool.True });
			var message = GetQueuedUniversalShipmentMessage(headerDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();
			var query = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			subQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);
			subQuery.AddToFilter(AsycudaBillSchema.ABL_BillNumber, "MAST0006");
			query.AddSubQuery(subQuery, JoinCondition.And);
			var headerBO = Factory.LoadTop1<AsycudaManifestHeader>(query);
			AssertEquals("headerBO.AMA_ManifestType", nameof(ManifestDocumentType.ALH), headerBO.AMA_ManifestType);
			AssertEquals("headerBO.AMA_Nature", "NT1", headerBO.AMA_Nature);
			AssertEquals("headerBO.AMA_DateAtCustomsOffice", ZDateTime.Today, headerBO.AMA_DateAtCustomsOffice);
			AssertEquals("headerBO.AMA_CarrierCode", "OTT1", headerBO.AMA_CarrierCode);
			AssertEquals("headerBO.PlaceOfEntry", "PEN", headerBO.PlaceOfEntry);
			AssertEquals("headerBO.PlaceOfExit", "PEX", headerBO.PlaceOfExit);
			AssertEquals("headerBO.EstimatedTimeOfLoading", ZDateTime.MaxSmallDateTimeValue, headerBO.EstimatedTimeOfLoading);
		}
	}
}
