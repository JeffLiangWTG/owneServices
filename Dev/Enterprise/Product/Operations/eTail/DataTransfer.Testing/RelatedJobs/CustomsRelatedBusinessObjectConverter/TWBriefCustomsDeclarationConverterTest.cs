using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using BriefCustomsDeclaration = Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaManifestHeader;
using ForwarderManifest = Enterprise.Customs.TW.Manifest.Business.AsycudaManifestHeader;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class TWBriefCustomsDeclarationConverterTest : BaseAsycudaManifestConverterTest<TWBriefCustomsDeclarationConverter>
	{
		public void TestConvertShipmentToBriefCustomsDeclaration_WhenForwarderManifestExists_ShouldConvertSuccessfully()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_MasterBillNum = "TESTCONSOL";
				consol.JK_RL_NKLoadPort = "TWAPG";

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = consol.TransportMode;
				shipment.JS_HouseBill = "TESTSHIPMENT";

				var destination = Factory.NewWithValidTestData<RefUNLOCO>();
				destination.RL_RN_NKCountryCode = CountryCodes.NewZealand;
				shipment.JS_RL_NKDestination = destination.Code;
				shipment.JS_RL_NKOrigin = "TWAPG";
				shipment.JS_RL_NKDestination = "NZABY";

				consol.Shipments.Add(shipment);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_WaybillNumber = "TESTCONSIGNMENT1";
				consignment.HVC_GoodsDescription = "ConsignmentDesc1";
				consignment.HVC_ConsignmentId = "CONSIGNMENT1";

				var item = consignment.Items.AddNew();
				item.HVI_JS_LoadedOnShipment = shipment.PK;

				Factory.Save();

				var forwarderManifestConverter = new TWForwarderManifestConverter(new TWForwarderManifestCommand(shipment));
				var success = forwarderManifestConverter.TryConvert(out _);
				Assert("Pre-condition: Convert to Forwarder Manifest succeed.", success);

				var forwarderManifest = (ForwarderManifest)forwarderManifestConverter.CustomsRelatedBusinessCollection.Single();
				AssertNotNull("Pre-condition: Forwarder Manifest is created", forwarderManifest);

				forwarderManifest.Factory.Save();
				Factory.Save();

				var converter = new TWBriefCustomsDeclarationConverter(new TWBriefCustomsDeclarationCommand(shipment));
				success = converter.TryConvert(out _);
				Assert("Convert to Brief Customs Declaration should succeed", success);

				var bcd = converter.CustomsRelatedBusinessCollection.Single() as BriefCustomsDeclaration;
				AssertNotNull("Brief Customs Declaration should be created.", bcd);
			}
		}

		protected override ZString LoginCountry => CountryCodes.Taiwan;

		protected override string[] SupportedTransportModes => new[] { TransportModes.Air, TransportModes.Sea };

		protected override string GetBillHumanReadableName(string billNumber) => $"Manifest Bill {billNumber}";

		protected override BaseHVLVRelatedJobCommand GetRelatedJobCommand(ForwardingShipment shipment) => new TWBriefCustomsDeclarationCommand(shipment);

		protected override string GetExpectedMessageTypeCode(ForwardingShipment shipment)
		{
			var result = base.GetExpectedMessageTypeCode(shipment);
			if (shipment.IsImport())
			{
				result = TWManifestTypes.Codes.ImportLowValueDutyFreeGoods;
			}
			else if (shipment.IsExport())
			{
				result = TWManifestTypes.Codes.ExportLowValueGoods;
			}

			return result;
		}

		protected override BusinessObject SetupExistingRelatedCustomsJob(ForwardingShipment shipment)
		{
			var result = Factory.NewWithValidTestData<BriefCustomsDeclaration>();
			result.AMA_JobReference = "TestReference" + shipment.JS_TransportMode;
			return result;
		}

		protected override ZString GetExistingRelatedCustomsJobReference(BusinessObject existingJob) => ((BriefCustomsDeclaration)existingJob).AMA_JobReference;
	}
}
