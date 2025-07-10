using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using ShipmentTypes = Enterprise.Core.Constants.ShipmentTypes;
using TransportModes = Enterprise.Core.Constants.TransportModes;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class USeManifestConverterTest : CustomsRelatedBusinessObjectConverterBaseTest<USeManifestConverter>
	{
		public void TestConvertShipmentToUSeManifest()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();

				shipment.JS_TransportMode = TransportModes.Road;
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

				var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment1.Items.AddNew();

				var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment2.Items.AddNew();

				Factory.Save();

				var converter = new USeManifestConverter(new USRoadEManifestCommand(shipment));
				var success = converter.TryConvert(out var errorMessage);
				Assert("Convert successfully", success);
				AssertNullOrEmpty("No error occured", errorMessage);

				var eManifest = converter.CustomsRelatedBusinessCollection.Single() as Trip;
				AssertNotNull("New trip should be created", eManifest);
				Assert("New trip should not be in database", !eManifest.IsInDatabase);
				AssertEquals("2 shipments should be created from consignments", 2, eManifest.Shipments.Count);
				Assert("Transferred log is not added to shipment when trip is not saved", !shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode).Any());
			}
		}

		public void TestClickCreateHVLVeManifestMenuItem_ShouldAddLog_ToShipment()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();

				shipment.JS_TransportMode = TransportModes.Road;
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.Items.AddNew();

				Factory.Save();

				var eManifests = ConvertAndSave(shipment).Cast<Trip>();

				shipment.Logs.GetAllLogs().Reload(true);

				var trfLogs = shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode);
				AssertHVLVeManifestLogs(trfLogs, eManifests);
			}
		}

		public void TestClickCreateHVLVeManifestMenuItem_ShouldAddLog_ToEManifest()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();

				shipment.JS_TransportMode = TransportModes.Road;
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.Items.AddNew();

				Factory.Save();

				var eManifests = ConvertAndSave(shipment).Cast<Trip>();

				var eManifest = eManifests.Single();
				var trfLog = eManifest.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode).Single();

				trfLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var type);
				trfLog.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber, out var jobNumber);

				CombineAssertions("eManifest should have a TRF log", () =>
				{
					AssertEquals("Type should be HVL", "HVL", type);
					AssertEquals("Job number should be equal to the shipment number", shipment.JobNumber, jobNumber);
				});
			}
		}

		public void TestConvertShipmentToUSeManifest_WhenSaveAndCloseEManifestForms_ShouldSaveParties()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();

				shipment.JS_TransportMode = TransportModes.Road;
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_ConsigneeName = "ConsigneeName";
				consignment.HVC_ConsigneeAddress1 = "address1";
				consignment.HVC_ConsigneeCity = "LA";
				consignment.HVC_ConsigneePostcode = "123456";
				consignment.HVC_ShipperName = "ShipperName";
				consignment.HVC_ShipperAddress1 = "address1";
				consignment.HVC_ShipperCity = "SH";
				consignment.Items.AddNew();

				Factory.Save();

				var converter = new USeManifestConverter(new USRoadEManifestCommand(shipment));
				var success = converter.TryConvert(out var errorMessage);
				Assert("Convert successfully", success);

				var eManifest = converter.CustomsRelatedBusinessCollection.Single() as Trip;
				AssertNotNull("New trip should be created", eManifest);

				eManifest.Factory.Save();

				var cusInBondBill = eManifest.Shipments.Single();

				CombineAssertions("parties has been saved",
				() =>
				{
					Assert(cusInBondBill.Consignee.IsInDatabase);
					AssertEquals("ConsigneeName", cusInBondBill.Consignee.CompanyName);
					Assert(cusInBondBill.Shipper.IsInDatabase);
					AssertEquals("ShipperName", cusInBondBill.Shipper.CompanyName);
				});
			}
		}

		public void TestConvertShipmentToUSeManifest_WhenSaveAndCloseEManifestForms_ShouldUseBulkCopySave()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();

				shipment.JS_TransportMode = TransportModes.Road;
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

				for (var i = 0; i < Factory.DefaultBulkCopyThreshold() + 1; i++)
				{
					var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
					consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
					consignment.Items.AddNew();
				}

				Factory.Save();

				var converter = new USeManifestConverter(new USRoadEManifestCommand(shipment));
				var success = converter.TryConvert(out var errorMessage);
				Assert("Convert successfully", success);

				var eManifest = converter.CustomsRelatedBusinessCollection.Single() as Trip;
				AssertNotNull("New trip should be created", eManifest);

				using (var bulkCopyEventTracker = new SqlBulkCopyEventTracker())
				{
					AssertNoExceptionThrown(eManifest.Factory.Save);
					Assert(bulkCopyEventTracker.HasBulkCopyEvent(CusInBondBillSchema.Constants.TableName, ["CheckConstraints", "FireTriggers"]));
					Assert(bulkCopyEventTracker.HasBulkCopyEvent(JobDocAddressSchema.Constants.TableName, ["CheckConstraints"]));
				}
			}
		}

		public void TestUSeManifest_ShouldPopulateGenPivot()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				var header = shipment.GetOrCreateHVLVConsignmentHeader();

				shipment.JS_TransportMode = TransportModes.Road;
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.Items.AddNew();

				Factory.Save();

				var bizo = ConvertAndSave(shipment).Cast<Trip>().Single();

				var genPivot = Factory.LoadTop1<GenPivot>(new ZQuery());
				AssertNotNull(genPivot);
				AssertEquals(genPivot.XX_RelationType, GenPivotTypes.HighVolumeLowValue);
				AssertEquals(genPivot.XX_Relation1ID, header.PK);
				AssertEquals(genPivot.XX_Relation2ID, bizo.PK);
				AssertEquals(genPivot.XX_Relation1TableCode, HVLVConsignmentHeaderSchema.Constants.Prefix);
				AssertEquals(genPivot.XX_Relation2TableCode, CusInBondHeaderSchema.Constants.Prefix);
			}
		}

		public new void TestNonWesternEuropeanCharactersRemovalService()
		{
			var shouldStripNonWesternEuropeanCharactersProperty = typeof(USeManifestConverter).GetProperty("ShouldStripNonWesternEuropeanCharacters", BindingFlags.Instance | BindingFlags.NonPublic);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var converter = new USeManifestConverter(new USRoadEManifestCommand(shipment));

			using (HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersUSeManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("ShouldStripNonWesternEuropeanCharacters should be true when registry is set", true, shouldStripNonWesternEuropeanCharactersProperty.GetValue(converter, null));
			}

			using (HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersUSeManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
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
			var converter = new USeManifestConverter(new USRoadEManifestCommand(shipment));
			converter.TryConvert(out _);

			var factory = converter.CustomsRelatedBusinessCollection.First().Factory;
			factory.Save();

			return converter.CustomsRelatedBusinessCollection;
		}

		void AssertHVLVeManifestLogs(IEnumerable<StmALog> logs, IEnumerable<Trip> eManifests)
		{
			AssertEquals(eManifests.Count(), logs.Count());

			foreach (var log in logs)
			{
				log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, out var type);
				AssertEquals("MAN", type);
			}

			AssertContainsExactElementsInAnyOrder(eManifests.Select(manifest => manifest.BH_JobReference), logs.Select(log =>
			{
				log.Parameters.TryGetValue(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, out var referenceNumber);
				return referenceNumber;
			}));
		}

		protected override ZString LoginCountry => CountryCodes.UnitedStates;

		protected override string[] SupportedTransportModes => new[] { TransportModes.Road };

		protected override RecipientRoleType GetExpectedPickupOrDeliveryRole(Directions shipmentDirection) => RecipientRoleType.DCA;

		protected override string GetExpectedTRFEventType(USeManifestConverter converter) => "MAN";

		protected override bool ShouldGenerateHLREventOnConversionFactorySaving => false;

		protected override string GetBillHumanReadableName(string billNumber) => $"Shipment: {billNumber}";

		protected override BaseHVLVRelatedJobCommand GetRelatedJobCommand(ForwardingShipment shipment) => new USRoadEManifestCommand(shipment);

		protected override BusinessObject SetupExistingRelatedCustomsJob(ForwardingShipment shipment) => Factory.NewWithValidTestData<Trip>();

		protected override IEnumerable<UniversalDataBuss.DataObjects.Universal.Shipment> GetDataObjectsContainingDataTarget(UniversalDataBuss.DataObjects.Universal.Shipment topLevelDataObject)
		{
			if (topLevelDataObject.SubShipmentCollection != null)
			{
				foreach (var subShipment in topLevelDataObject.SubShipmentCollection)
				{
					var shipmentType = subShipment.ShipmentType.GetCodeAsUpperCase();
					if (shipmentType == ShipmentTypes.HighVolumeLowValue)
					{
						yield return subShipment;
					}
				}
			}
		}

		protected override ZString GetExistingRelatedCustomsJobReference(BusinessObject existingJob) => ((Trip)existingJob).BH_JobReference;
	}
}
