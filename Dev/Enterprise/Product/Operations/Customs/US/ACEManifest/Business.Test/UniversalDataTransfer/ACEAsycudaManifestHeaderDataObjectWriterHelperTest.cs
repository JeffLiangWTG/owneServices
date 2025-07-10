using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.US.ACEManifest.Business.UniversalDataTransfer.Testing
{
	class ACEAsycudaManifestHeaderDataObjectWriterHelperTest : TestCaseWithUniversalObjectFactory
	{
		public void TestExportFDAIndicator()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.SaveForTesting();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "IAM";
			var bill = header.Bills.AddNew();
			bill.FDAIndicator = ZBool.True;
			var headerData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, header);
			var billData = headerData.SubShipmentCollection[0];
			var billEntryInstructionData = billData.EntryInstructionCollection[0];
			var fdaIndicator = billEntryInstructionData.AddInfoCollection.First(x => x.Key.GetValueOrDefault() == AddInfoConstants.BillCountry.FDAIndicator);
			AssertEquals("Y", fdaIndicator.Value);
		}

		[TestDate(2020, 07, 20)]
		public void TestExportEstDateAtFirstArrival()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.SaveForTesting();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = "IAM";
			var bill = header.Bills.AddNew();
			var testDatetime = ZDateTime.Now;
			header.EstDateAtFirstArrival = testDatetime;
			var headerData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, header);
			AssertEquals(testDatetime, headerData.DateCollection.First(x => x.Type == DateType.FirstArrivalInCountry).Value);
		}
	}
}
