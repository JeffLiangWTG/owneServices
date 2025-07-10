using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class USAirAMSConverterTest : BaseAsycudaManifestConverterTest<USAirAMSConverter>
	{
		protected override string GetBillHumanReadableName(string billNumber)
		{
			return $"US Air AMS  - {billNumber}";
		}

		public void TestConvertShipmentToUSAirAMS()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_UniqueConsignRef = "S002";
				shipment.JS_RL_NKDestination = "USLAX";

				var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment1.HVC_IsActive = true;
				consignment1.Items.AddNew();

				var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment2.HVC_IsActive = true;
				consignment2.Items.AddNew();

				Factory.Save();

				var converter = new USAirAMSConverter(new USAirAMSCommand(shipment));
				var success = converter.TryConvert(out var errorMsg);
				Assert("Convert successfully", success);
				AssertNullOrEmpty("No error occured", errorMsg);

				var manifestHeader = converter.CustomsRelatedBusinessCollection.Single() as AsycudaManifestHeader;
				AssertNotNull("New manifest header should be created", manifestHeader);
				Assert("New manifest header should not be in database", !manifestHeader.IsInDatabase);
				AssertEquals("2 bills should be created from consignments", 2, manifestHeader.Bills.Count);
				Assert("Transferred log is not added to shipment when header is not saved", !shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode).Any());
			}
		}

		public void TestConvertShipmentToUSAirAMS_WhenCurrentCompanyIsNotUS()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_UniqueConsignRef = "S002";
				shipment.JS_RL_NKDestination = "USLAX";

				var header = shipment.GetOrCreateHVLVConsignmentHeader();
				var consignment = header.Consignments.AddNew();
				consignment.Items.AddNew();

				Factory.Save();

				var converter = new USAirAMSConverter(new USAirAMSCommand(shipment));
				var success = converter.TryConvert(out var errorMsg);

				Assert("Convert successfully", success);
				AssertNullOrEmpty("No error occured", errorMsg);

				var manifestHeader = converter.CustomsRelatedBusinessCollection.Single() as AsycudaManifestHeader;
				AssertNotNull("New manifest header should be created for non US company", manifestHeader);
				AssertEquals("Manifest header country should be set to US", CountryCodes.UnitedStates, manifestHeader.AMA_RN_NKCountry);
				AssertEquals("1 bill should be created from consignment", 1, manifestHeader.Bills.Count);
				Assert("Transferred log is not added to shipment when header is not saved", !shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode).Any());
			}
		}

		public void TestConvertShipmentToUSAirAMS_ShouldUseGoodsValueFallback()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_UniqueConsignRef = "S002";
				shipment.JS_RL_NKDestination = "USLAX";

				var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment1.HVC_IsActive = true;
				consignment1.HVC_GoodsValue = 0m;

				var item1 = consignment1.Items.AddNew();
				item1.HVI_HVC_Consignment = consignment1.PK;
				item1.HVI_JS_LoadedOnShipment = consignment1.HVC_JS_ManifestedOnShipment;

				var itemLine1 = item1.Lines.AddNew();
				itemLine1.HVS_Quantity = 1;
				itemLine1.HVS_CustomsValue = 25m;

				var itemLine2 = item1.Lines.AddNew();
				itemLine2.HVS_Quantity = 1;
				itemLine2.HVS_CustomsValue = 5m;

				Factory.Save();

				var converter = new USAirAMSConverter(new USAirAMSCommand(shipment));
				var success = converter.TryConvert(out var errorMsg);
				Assert("Precondition: Converted to US Air AMS successfully", success);
				AssertNullOrEmpty("Precondition: No error occurred", errorMsg);

				var manifestHeader = converter.CustomsRelatedBusinessCollection.Single() as AsycudaManifestHeader;
				AssertNotNull("Precondition: New manifest header should be created", manifestHeader);

				var bills = manifestHeader.Bills;
				var asycudaBill = bills.AsEnumerable().Single();
				AssertNotNull(asycudaBill);

				AssertEquals("When the consignment 'Goods Value' (HVC_GoodsValue) is zero, then fallback logic should be used.", itemLine1.HVS_CustomsValue + itemLine2.HVS_CustomsValue, asycudaBill.ABL_GoodsValue);
			}
		}

		[TestDate(2021, 4, 8)]
		public void TestConvertShipmentToUSAirAMS_PopulateSecurityFilingFirstUsageTimeUtc()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_UniqueConsignRef = "S002";
				shipment.JS_RL_NKDestination = "USLAX";

				var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
				var item1 = consignment.Items.AddNew();
				item1.HVI_SecurityFilingFirstUsageTimeUtc = new ZDateTime(2021, 2, 2);
				var item2 = Factory.NewWithValidTestData<HVLVItem>();
				item2.HVI_JS_LoadedOnShipment = shipment.PK;

				Factory.Save();

				AssertEquals("pre condition", new ZDateTime(2021, 2, 2), item1.HVI_SecurityFilingFirstUsageTimeUtc);
				AssertEquals("pre condition", ZDateTime.Empty, item2.HVI_SecurityFilingFirstUsageTimeUtc);

				ConvertAndSave(shipment);

				item1.Reload();
				item2.Reload();

				CombineAssertions(() =>
				{
					AssertEquals("SecurityFilingFirstUsageTimeUtc should not be changed if not empty", new ZDateTime(2021, 2, 2), item1.HVI_SecurityFilingFirstUsageTimeUtc);
					AssertEquals("SecurityFilingFirstUsageTimeUtc should be populated", new ZDateTime(2021, 4, 8), item2.HVI_SecurityFilingFirstUsageTimeUtc);
				});
			}
		}

		public void TestConvertShipmentToUSAirAMS_ShouldAddLogToShipment()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_UniqueConsignRef = "S002";
				shipment.JS_RL_NKDestination = "USLAX";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_IsActive = true;
				consignment.Items.AddNew();

				Factory.Save();

				var trfLog = shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode).SingleOrDefault();
				AssertNull("pre condition", trfLog);

				var header = ConvertAndSave(shipment).Single() as AsycudaManifestHeader;

				trfLog = shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode).SingleOrDefault();
				AssertNotNull(trfLog);

				trfLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var logType);
				trfLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Mode, out var transportMode);
				trfLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, out var referenceNumber);

				CombineAssertions(() =>
				{
					AssertEquals("AMS", logType);
					AssertEquals(TransportModes.Air, transportMode);
					AssertEquals(header.AMA_JobReference, referenceNumber);
				});
			}
		}

		public void TestConvertShipmentToUSAirAMS_ShouldAddLogToHeader()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_UniqueConsignRef = "S002";
				shipment.JS_RL_NKDestination = "USLAX";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_IsActive = true;
				consignment.Items.AddNew();

				Factory.Save();

				var header = ConvertAndSave(shipment).Single() as AsycudaManifestHeader;
				var trfLog = header.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode).SingleOrDefault();
				AssertNotNull(trfLog);

				trfLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var logType);
				trfLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber, out var jobNumber);

				CombineAssertions(() =>
				{
					AssertEquals("HVL", logType);
					AssertEquals(shipment.JobNumber, jobNumber);
				});
			}
		}

		public void TestUSAirAMS_ShouldPopulateGenPivot()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				var header = shipment.GetOrCreateHVLVConsignmentHeader();

				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.Items.AddNew();

				Factory.Save();

				var bizo = ConvertAndSave(shipment).Single() as AsycudaManifestHeader;

				var genPivot = Factory.LoadTop1<GenPivot>(new ZQuery());
				AssertNotNull(genPivot);
				AssertEquals(genPivot.XX_RelationType, GenPivotTypes.HighVolumeLowValue);
				AssertEquals(genPivot.XX_Relation1ID, header.PK);
				AssertEquals(genPivot.XX_Relation2ID, bizo.PK);
				AssertEquals(genPivot.XX_Relation1TableCode, HVLVConsignmentHeaderSchema.Constants.Prefix);
				AssertEquals(genPivot.XX_Relation2TableCode, AsycudaManifestHeaderSchema.Constants.Prefix);
			}
		}

		public new void TestNonWesternEuropeanCharactersRemovalService()
		{
			var shouldStripNonWesternEuropeanCharactersProperty = typeof(USAirAMSConverter).GetProperty("ShouldStripNonWesternEuropeanCharacters", BindingFlags.Instance | BindingFlags.NonPublic);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var converter = new USAirAMSConverter(new USAirAMSCommand(shipment));

			using (HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersUSAirAMS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("ShouldStripNonWesternEuropeanCharacters should be true when registry is set", true, shouldStripNonWesternEuropeanCharactersProperty.GetValue(converter, null));
			}

			using (HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersUSAirAMS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("ShouldStripNonWesternEuropeanCharacters should be false when registry is not set", false, shouldStripNonWesternEuropeanCharactersProperty.GetValue(converter, null));
			}
		}

		public void TestExportedUniversalShipment_TopLevelDataObjectIsRelevantConsol_WhenNonUSCompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(ForeignCountryForTestData))
			{
				AssertExportedUniversalShipment_TopLevelDataObjectIsRelevantConsol(isDestinationSameAsLoginCountry: true, GetExpectedPickupOrDeliveryRole(GetShipmentDirectionRelativeToForeignCountry(true)));
				AssertExportedUniversalShipment_TopLevelDataObjectIsRelevantConsol(isDestinationSameAsLoginCountry: false, GetExpectedPickupOrDeliveryRole(GetShipmentDirectionRelativeToForeignCountry(false)));
			}

			static Directions GetShipmentDirectionRelativeToForeignCountry(bool isDestinationSameAsLoginCountry) => isDestinationSameAsLoginCountry ? Directions.Export : Directions.Import;
		}

		IReadOnlyCollection<BusinessObject> ConvertAndSave(ForwardingShipment shipment)
		{
			var converter = new USAirAMSConverter(new USAirAMSCommand(shipment));
			converter.TryConvert(out _);

			var factory = converter.CustomsRelatedBusinessCollection.Single().Factory;
			factory.Save();

			return converter.CustomsRelatedBusinessCollection;
		}

		protected override ZString LoginCountry => CountryCodes.UnitedStates;

		protected override string[] SupportedTransportModes => new[] { TransportModes.Air };

		protected override RecipientRoleType GetExpectedPickupOrDeliveryRole(Directions shipmentDirection) => RecipientRoleType.DCA;

		protected override string GetExpectedTRFEventType(USAirAMSConverter converter) => "AMS";

		protected override BaseHVLVRelatedJobCommand GetRelatedJobCommand(ForwardingShipment shipment) => new USAirAMSCommand(shipment);

		protected override bool ShouldGenerateHLREventOnConversionFactorySaving => false;

		protected override bool ShouldEntryHeaderContainDestinationCountryInsteadOfCurrentLoginCountry => true;

		protected override string GetExpectedMessageTypeCode(ForwardingShipment shipment) => ACEManifestTypes.Codes.IAM;

		protected override BusinessObject SetupExistingRelatedCustomsJob(ForwardingShipment shipment) => Factory.NewWithValidTestData<AsycudaManifestHeader>();

		protected override ZString GetExistingRelatedCustomsJobReference(BusinessObject existingJob) => ((AsycudaManifestHeader)existingJob).AMA_JobReference;

		protected override bool ShouldAddTransferredLogToRelatedJobOnEveryConversion => false;
	}
}
