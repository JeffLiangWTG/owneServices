using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Business;
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
	class USSeaAMSConverterTest : CustomsRelatedBusinessObjectConverterBaseTest<USSeaAMSConverter>
	{
		public void TestConvertShipmentToUSSeaAMS()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
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

				var converter = new USSeaAMSConverter(new USSeaAMSCommand(shipment));
				var success = converter.TryConvert(out var errorMsg);
				Assert("Convert successfully", success);
				AssertNullOrEmpty("No error occured", errorMsg);

				var amsHeader = converter.CustomsRelatedBusinessCollection.Single() as CusInBondHeader;
				AssertNotNull("New US Sea AMS header should be created", amsHeader);
				Assert("New US Sea AMS header should not be in database", !amsHeader.IsInDatabase);
				AssertEquals("2 bills should be created from consignments", 2, amsHeader.Bills.Count);
				Assert("Transferred log is not added to shipment when header is not saved", !shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode).Any());
			}
		}

		[TestDate(2023, 1, 19)]
		public void TestConvertShipmentToUSSeaAMS_PopulateSecurityFilingFirstUsageTimeUtc()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S002";
				shipment.JS_RL_NKDestination = "USLAX";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				var item1 = consignment.Items.AddNew();
				item1.HVI_SecurityFilingFirstUsageTimeUtc = new ZDateTime(2022, 2, 2);
				var item2 = Factory.NewWithValidTestData<HVLVItem>();
				item2.HVI_JS_LoadedOnShipment = shipment.PK;

				Factory.Save();

				AssertEquals("pre condition", new ZDateTime(2022, 2, 2), item1.HVI_SecurityFilingFirstUsageTimeUtc);
				AssertEquals("pre condition", ZDateTime.Empty, item2.HVI_SecurityFilingFirstUsageTimeUtc);

				ConvertAndSave(shipment);

				item1.Reload();
				item2.Reload();

				CombineAssertions(() =>
				{
					AssertEquals("SecurityFilingFirstUsageTimeUtc should not be changed if not empty", new ZDateTime(2022, 2, 2), item1.HVI_SecurityFilingFirstUsageTimeUtc);
					AssertEquals("SecurityFilingFirstUsageTimeUtc should be populated", new ZDateTime(2023, 1, 19), item2.HVI_SecurityFilingFirstUsageTimeUtc);
				});
			}
		}

		public void TestConvertShipmentToUSSeaAMS_ShouldAddLogToShipment()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S002";
				shipment.JS_RL_NKDestination = "USLAX";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_IsActive = true;
				consignment.Items.AddNew();

				Factory.Save();

				var trfLog = shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode).SingleOrDefault();
				AssertNull("pre condition", trfLog);

				var amsHeader = ConvertAndSave(shipment).Single() as CusInBondHeader;

				trfLog = shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode).SingleOrDefault();
				AssertNotNull(trfLog);

				trfLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var logType);
				trfLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Mode, out var transportMode);
				trfLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, out var referenceNumber);

				CombineAssertions(() =>
				{
					AssertEquals("AMS", logType);
					AssertEquals(TransportModes.Sea, transportMode);
					AssertEquals(amsHeader.BH_JobReference, referenceNumber);
				});
			}
		}

		public void TestConvertShipmentToUSSeaAMS_ShouldAddLogToHeader()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_UniqueConsignRef = "S002";
				shipment.JS_RL_NKDestination = "USLAX";

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_IsActive = true;
				consignment.Items.AddNew();

				Factory.Save();

				var amsHeader = ConvertAndSave(shipment).Single() as CusInBondHeader;
				var trfLog = amsHeader.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode).SingleOrDefault();
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

		public void TestUSSeaAMS_ShouldPopulateGenPivot()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.Items.AddNew();

				Factory.Save();

				var amsHeader = ConvertAndSave(shipment).Single() as CusInBondHeader;

				var genPivot = Factory.LoadTop1<GenPivot>(new ZQuery());

				CombineAssertions("Should populate GenPivot", () =>
				{
					AssertNotNull(genPivot);
					AssertEquals(GenPivotTypes.HighVolumeLowValue, genPivot.XX_RelationType);
					AssertEquals(consignmentHeader.PK, genPivot.XX_Relation1ID);
					AssertEquals(amsHeader.PK, genPivot.XX_Relation2ID);
					AssertEquals(HVLVConsignmentHeaderSchema.Constants.Prefix, genPivot.XX_Relation1TableCode);
					AssertEquals(CusInBondHeaderSchema.Constants.Prefix, genPivot.XX_Relation2TableCode);
				});
			}
		}

		public new void TestNonWesternEuropeanCharactersRemovalService()
		{
			var shouldStripNonWesternEuropeanCharactersProperty = typeof(USSeaAMSConverter).GetProperty("ShouldStripNonWesternEuropeanCharacters", BindingFlags.Instance | BindingFlags.NonPublic);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var converter = new USSeaAMSConverter(new USSeaAMSCommand(shipment));

			using (HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersUSSeaAMS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("ShouldStripNonWesternEuropeanCharacters should be true when registry is set", true, shouldStripNonWesternEuropeanCharactersProperty.GetValue(converter, null));
			}

			using (HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersUSSeaAMS.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
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
			var converter = new USSeaAMSConverter(new USSeaAMSCommand(shipment));
			converter.TryConvert(out _);

			var factory = converter.CustomsRelatedBusinessCollection.Single().Factory;
			factory.Save();

			return converter.CustomsRelatedBusinessCollection;
		}

		protected override ZString LoginCountry => CountryCodes.UnitedStates;

		protected override string[] SupportedTransportModes => new[] { TransportModes.Sea };

		protected override RecipientRoleType GetExpectedPickupOrDeliveryRole(Directions shipmentDirection) => RecipientRoleType.DCA;

		protected override string GetExpectedTRFEventType(USSeaAMSConverter converter) => "AMS";

		protected override BaseHVLVRelatedJobCommand GetRelatedJobCommand(ForwardingShipment shipment) => new USSeaAMSCommand(shipment);

		protected override bool ShouldGenerateHLREventOnConversionFactorySaving => false;

		protected override string GetExpectedMessageTypeCode(ForwardingShipment shipment) => DirectionTypeList.Codes.NVOCC;

		protected override BusinessObject SetupExistingRelatedCustomsJob(ForwardingShipment shipment) => Factory.NewWithValidTestData<CusInBondHeader>();

		protected override ZString GetExistingRelatedCustomsJobReference(BusinessObject existingJob) => ((CusInBondHeader)existingJob).BH_JobReference;

		protected override bool ShouldAddTransferredLogToRelatedJobOnEveryConversion => false;
	}
}
