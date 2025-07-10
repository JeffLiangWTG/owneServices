using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.ZA.Manifest.Business.UniversalDataTransfer.Testing
{
	class ZAAsycudaManifestHeaderDataObjectWriterHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestExportAsycudaManifestHeaderFields()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_ManifestType = nameof(ManifestDocumentType.ALM);
			header.PlaceOfEntry = "JHB";
			header.PlaceOfExit = "JSA";
			header.EstimatedTimeOfLoading = ZDateTime.MaxSmallDateTimeValue;
			var headerData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, header);
			var headerEntryInstructionData = headerData.EntryInstructionCollection[0];
			var placeOfEntry = headerEntryInstructionData.AddInfoCollection.FirstOrDefault(x => x.Key.Value == AsycudaManifestHeader.Schema.PlaceOfEntry).Value;
			var placeOfExitCode = headerEntryInstructionData.AddInfoCollection.FirstOrDefault(x => x.Key.Value == GenAddOnHelper.PlaceOfExitCode).Value;
			var estimateLoadTime = headerData.DateCollection.FirstOrDefault(x => x.Type == DateType.LoadingDate && x.IsEstimate == ZBool.True).Value;
			AssertEquals("PlaceOfEntry", "JHB", placeOfEntry);
			AssertEquals("PlaceOfExitCode", "JSA", placeOfExitCode);
			AssertEquals("Estimated Load Time", ZDateTime.MaxSmallDateTimeValue, estimateLoadTime);
		}
	}
}
