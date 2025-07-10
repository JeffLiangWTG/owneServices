using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.Access.Business.UniversalDataTransfer.Testing
{
	sealed class SGAsycudaManifestDataObjectReaderHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestImportAsycudaManifestHeaderFields()
		{
			var sgRegistry = ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ASYCUDA.Business.Testing.ZZDataTestHelper.SetupZZ(new BusinessObjectFactory(), Core.Constants.CountryCodes.Singapore);
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var usAirLocalPort1 = help.GetAirLocalPort1("US");
			var sgAirLocalPort1 = help.GetAirLocalPort1(Core.Constants.CountryCodes.Singapore);
			var usFirstArrival = new UNLOCO()
			{ Code = usAirLocalPort1.RL_Code };
			var sgFirstArrival = new UNLOCO()
			{ Code = sgAirLocalPort1.RL_Code };
			var headerDataObject = help.SetupManifestHeader("MAST0006", usFirstArrival, sgFirstArrival, new ZDateTime(2018, 2, 10), new ZDateTime(2018, 2, 1), "", SGManifestTypes.Codes.MGI);
			var headerEntryHeader = help.SetupCountryHeaderEntryHeader(Core.Constants.CountryCodes.Singapore, 1);
			var headerEntryInstruction = help.SetupCountryHeaderEntryInstruction(1, usFirstArrival, "OTT1", Core.Constants.CountryCodes.Singapore, Constants.ManifestType.Import, "NT1");
			if (headerDataObject.SetEntryHeaderCollection(() => new List<UniversalDataBuss.DataObjects.Universal.Customs.EntryHeader>()))
			{
				headerDataObject.EntryHeaderCollection.Add(headerEntryHeader);
			}

			if (headerDataObject.SetEntryInstructionCollection(() => new List<UniversalDataBuss.DataObjects.Universal.Customs.EntryInstruction>()))
			{
				headerDataObject.EntryInstructionCollection.Add(headerEntryInstruction);
			}

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
			AssertEquals("headerBO.AMA_ManifestType", Constants.ManifestType.Import, headerBO.AMA_ManifestType);
			AssertEquals("headerBO.AMA_Nature", "NT1", headerBO.AMA_Nature);
			AssertEquals("headerBO.AMA_DateAtCustomsOffice", ZDate.Today, headerBO.AMA_DateAtCustomsOffice);
			AssertEquals("headerBO.AMA_CarrierCode", "OTT1", headerBO.AMA_CarrierCode);
			AssertEquals("countryBO.ValuationDate", ZDate.Today, headerBO.ValuationDate);
		}

		public void TestGetAdditionalInfoColumnList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var helper = new SGAsycudaManifestDataObjectReaderHelper(Factory.BOFactory);
			var detail = helper.GetAdditionalInfoColumnList(bill).Single();
			CombineAssertions(() =>
			{
				AssertEquals("STR", detail.TypeCode);
				AssertEquals("GSTNReferenceNo", detail.AddInfoKey);
				AssertEquals("GSTNReferenceNo", detail.GenAddOnColumnName);
				AssertEquals("GSTNReferenceNo", detail.PropertyName);
			});
		}
	}
}
