using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.NZ;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class NZAirCargoReportConverterTest : CustomsRelatedBusinessObjectConverterBaseTest<NZAirCargoReportConverter>
	{
		public void TestConvertShipmentToCargoReport_EndToEnd_NewZealand()
		{
			AssertConvertShipmentToCargoReport_EndToEnd(TransportModes.Air, CusHAWBSchema.Constants.Prefix);
		}

		public void TestConvertShipmentToCargoReport_Cancel_NewZealand()
		{
			AssertConvertShipmentToCargoReport_Cancel(TransportModes.Air, CusMAWBSchema.Constants.Prefix, CusHAWBSchema.Constants.Prefix);
		}

		public void TestMultipleCustomsRelatedBusinessCollectionCreatedWhenConsignmentsMoreThanRegistryLimit()
		{
			var registryItem = (ObjectFactory.
				Get<INZCustomsDataRegistry>()
				as RegistryItemSet).
				FindByName("MaxNumberOfECIManifestLinesAccepted");
			var oldRegistryValue = registryItem.Value;
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			var registryItemCRE = (ObjectFactory.
				Get<INZCustomsDataRegistry>()
				as RegistryItemSet).
				FindByName("MaxNumberOfECIManifestLinesAcceptedCRE");
			var oldRegistryValueCRE = registryItem.Value;
			registryItemCRE.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);

			try
			{
				var shipment = SetupTestShipment(TransportModes.Air);
				for (var i = 3; i < 7; i++)
				{
					var item = Factory.NewWithValidTestData<HVLVItem>();
					item.HVI_JS_LoadedOnShipment = shipment.PK;
					var consignment1 = item.Consignment;
					consignment1.HVC_WaybillNumber = "HVC0000" + i;
					consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
				}

				Factory.Save();

				var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
				{
					var success = converter.TryConvert(out var errorMsg);

					var converterFactory = converter.CustomsRelatedBusinessCollection[0].Factory;
					converterFactory.Save();

					var masterBillType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix("CM");
					var masterBills = Factory.Load(masterBillType, new ZQuery());

					CombineAssertions(() =>
					{
						Assert("Convert successfully", success);
						AssertNullOrEmpty("No error occured", errorMsg);
						AssertNotNull("New masterBills should be created", masterBills);
						AssertEquals("2 masterBills should be created", 2, masterBills.Length);
						AssertEquals("2 masterBills should be created", 2, converter.CustomsRelatedBusinessCollection.Count);
						Assert("New masterBills should be saved", masterBills.All(m => m.IsInDatabase));
						AssertContainsExactElementsInAnyOrder("Masterbill PK", masterBills.Select(m => m.PK), converter.CustomsRelatedBusinessCollection.Select(m => m.PK));
					});
				}
			}
			finally
			{
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldRegistryValue);
				registryItemCRE.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldRegistryValueCRE);
			}
		}

		public void TestConvertShipmentsLinkedToSameAirConsolToCargoReport_EndToEnd_NewZealand()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_MasterBillNum = "08138374491";

				var shipment = CreateTestShipment(consol, "M97N872947", 2);
				shipment.JS_HouseBill = "TESTSHIPMENT1";
				Factory.Save();

				var masterBill = ConvertToRelatedJob<ICusMAWB>(shipment);
				Assert("New masterBill should not be saved", !masterBill.IsInDatabase);

				var converterFactory = masterBill.Factory;
				var houseBillType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(CusHAWBSchema.Constants.Prefix);
				var houseBills = converterFactory.Load(houseBillType, new ZQuery());
				AssertEquals("2 houseBills should be created", 2, houseBills.Length);
				Assert("New houseBill should not be saved", houseBills.All(h => !h.IsInDatabase));
				Assert("HouseBills from shipment 1 should only be added to MasterBill", houseBills.Length == 2);
				Assert("HouseBill related to shipment 1 should be added to MasterBill", houseBills.FirstOrDefault(o => ((CusHAWB)o).CS_HAWB.Equals("M97N872947-0")) != null);
				Assert("HouseBill related to shipment 1 should be added to MasterBill", houseBills.FirstOrDefault(o => ((CusHAWB)o).CS_HAWB.Equals("M97N872947-1")) != null);

				converterFactory.Save();

				var hlrLogs = shipment.Logs.GetAllLogs().OfType<StmALog>().Where(l => l.SL_SE_NKEvent == "HLR");
				AssertEquals("1 HLR event should be added to shipment", 1, hlrLogs.Count());
				AssertEquals("New HLR event should have reason Cargo Report Created", "|RES=Cargo Report Created", hlrLogs.First().SL_Reference);

				var shipment2 = CreateTestShipment(consol, "M97N872948", 2);
				shipment2.JS_HouseBill = "TESTSHIPMENT2";
				Factory.Save();

				masterBill = ConvertToRelatedJob<ICusMAWB>(shipment2);
				Assert("Existing masterBill should be updated", masterBill.IsInDatabase);

				converterFactory = masterBill.Factory;
				houseBillType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(CusHAWBSchema.Constants.Prefix);
				houseBills = converterFactory.Load(houseBillType, new ZQuery());
				Assert("HouseBills from both shipments should be added to MasterBill", houseBills.Length == 4);
				CombineAssertions("HAWB MasterHouseBill should be populated with shipment house bill", () =>
				{
					AssertEquals(2, houseBills.Where(hbl => ((CusHAWB)hbl).CS_MasterHouseBill == "TESTSHIPMENT1").Count());
					AssertEquals(2, houseBills.Where(hbl => ((CusHAWB)hbl).CS_MasterHouseBill == "TESTSHIPMENT2").Count());
				});

				converterFactory.Save();

				hlrLogs = shipment2.Logs.GetAllLogs().OfType<StmALog>().Where(l => l.SL_SE_NKEvent == "HLR");
				AssertEquals("1 HLR event should be added to shipment", 1, hlrLogs.Count());
				AssertEquals("New HLR event should have reason Cargo Report Created", "|RES=Cargo Report Created", hlrLogs.First().SL_Reference);
			}
		}

		public void TestConvertShipmentsLinkedToSameConsolToCargoReport_ExceedLimit_NewZealand()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var nzRegistry = ObjectFactory.Get<INZCustomsDataRegistry>();
				nzRegistry.MaxNumberOfECIManifestLinesAcceptedCRE.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 6);

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_MasterBillNum = "08138374491";

				var shipment = CreateTestShipment(consol, "M97N872947", 4);
				Factory.Save();

				var masterBill = ConvertToRelatedJob<ICusMAWB>(shipment);
				masterBill.Factory.Save();

				var shipment2 = CreateTestShipment(consol, "M97N872948", 4);
				Factory.Save();

				var newMasterBill = ConvertToRelatedJob<ICusMAWB>(shipment2);

				AssertNotNull("MasterBill should be created, consolidating to existing will exceed limit", newMasterBill);
				AssertNotEquals("The MasterBill should be different from previous one", masterBill.PK, newMasterBill.PK);
				Assert("New master bill should not be in database", !newMasterBill.IsInDatabase);
				AssertEquals("New master bill should have 4 house bill lines", 4, ((ICusMAWB)newMasterBill).ConsignmentCount);
			}
		}

		public void TestConvertShipmentsLinkedToSameConsolToCargoReport_NotConsolidateWhenLatestBillIsOneOfTwinsBillOfParentShipment_NewZeland()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var nzRegistry = ObjectFactory.Get<INZCustomsDataRegistry>();
				nzRegistry.MaxNumberOfECIManifestLinesAcceptedCRE.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 6);

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_MasterBillNum = "08138374491";

				var shipment = CreateTestShipment(consol, "M97N872947", 8);
				Factory.Save();

				var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

				var success = converter.TryConvert(out var errorMsg);
				Assert("Convert successfully", success);
				AssertNullOrEmpty("No error occured", errorMsg);

				var converterFactory = converter.CustomsRelatedBusinessCollection[0].Factory;

				var masterBills = converter.CustomsRelatedBusinessCollection;
				AssertEquals("2 masterBills should be created because the limit is exceeded", 2, masterBills.Count);

				converterFactory.Save();

				var shipment2 = CreateTestShipment(consol, "M97N872948", 1);
				Factory.Save();

				var converter2 = CreateCustomsRelatedBusinessObjectConverter(shipment2);

				success = converter2.TryConvert(out var errorMsg1);
				Assert("Convert successfully", success);
				AssertNullOrEmpty("No error occured", errorMsg1);

				var newMasterBill = converter2.CustomsRelatedBusinessCollection.Single();
				AssertNotNull("New MasterBill should be created, no consolidate to twins bills", newMasterBill);
				Assert("The MasterBill should be different from previous ones", !masterBills.Any(b => b.PK == newMasterBill.PK));
				Assert("New master bill should not be in database", !newMasterBill.IsInDatabase);
				AssertEquals("New master bill should have 1 house bill line", 1, ((CusMAWB)newMasterBill).ChildBills.Count);
			}
		}

		[TestDate(2023, 5, 24)]
		public void TestConvertShipmentsLinkedToSameConsolToCargoReport_MultipleShipmentsToMultipleBills_NewZeland()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			using (HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var nzRegistry = ObjectFactory.Get<INZCustomsDataRegistry>();
				nzRegistry.MaxNumberOfECIManifestLinesAcceptedCRE.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 6);

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_MasterBillNum = "08138374491";

				var shipment1 = CreateTestShipment(consol, "testShipment1", 2);
				var shipment2 = CreateTestShipment(consol, "testShipment2", 2);
				Factory.Save();

				var masterBill1 = ConvertToRelatedJob<ICusMAWB>(shipment1);
				masterBill1.Factory.Save();

				var updatedMasterBill1 = ConvertToRelatedJob<ICusMAWB>(shipment2);
				AssertEquals("Consolidate into bill 1", masterBill1.PK, updatedMasterBill1.PK);
				updatedMasterBill1.Factory.Save();

				TestDateAttribute.AddMinutes(1);

				var shipment3 = CreateTestShipment(consol, "testShipment3", 3);
				Factory.Save();

				var masterBill2 = ConvertToRelatedJob<ICusMAWB>(shipment3);
				AssertNotEquals("Should created new bill", masterBill1.PK, masterBill2.PK);
				masterBill2.Factory.Save();

				var shipment4 = CreateTestShipment(consol, "testShipment4", 1);
				Factory.Save();

				var updatedMasterBill2 = ConvertToRelatedJob<ICusMAWB>(shipment4);
				AssertEquals("Consolidate into bill 2", masterBill2.PK, updatedMasterBill2.PK);
			}
		}

		public void TestConvertShipmentToCargoReport_Exceptions_MultipleRelatedBusinesses()
		{
			var nzRegistry = ObjectFactory.Get<INZCustomsDataRegistry>();
			nzRegistry.MaxNumberOfECIManifestLinesAccepted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5);

			var shipment = SetupTestShipment(TransportModes.Air);
			shipment.ArrivalConsol.JK_MasterBillNum = string.Empty;

			for (var i = 3; i < 7; i++)
			{
				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_JS_LoadedOnShipment = shipment.PK;
				var consignment1 = item.Consignment;
				consignment1.HVC_WaybillNumber = "HVC0000" + i;
				consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			}

			Factory.Save();

			var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				var success = converter.TryConvert(out var errorMsg);

				Assert(!success);
				AssertEquals(@"Failed to read into multiple AirManifest from Shipment [EBM22Q33TU475BXH3P60]
Cannot populate CusMAWB because:
WayBillNumber must not be empty.
Cannot populate CusMAWB because:
WayBillNumber must not be empty.", errorMsg);
			}
		}

		ForwardingShipment CreateTestShipment(ForwardingConsol consol, string houseBillNum, int totalConsignmentsToCreate)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = consol.TransportMode;
			shipment.JS_HouseBill = houseBillNum;

			var destination = Factory.NewWithValidTestData<RefUNLOCO>();
			destination.RL_RN_NKCountryCode = CountryCodes.NewZealand;
			shipment.JS_RL_NKDestination = destination.Code;
			shipment.JS_RL_NKOrigin = "USMEM";
			shipment.JS_RL_NKDestination = "NZABY";

			consol.Shipments.Add(shipment);

			for (var i = 0; i < totalConsignmentsToCreate; i++)
			{
				var item = Factory.NewWithValidTestData<HVLVItem>();
				item.HVI_JS_LoadedOnShipment = shipment.PK;
				var consignment = item.Consignment;
				consignment.HVC_WaybillNumber = string.Concat(shipment.JS_HouseBill, "-", i);
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			}

			return shipment;
		}

		protected override ZString LoginCountry => CountryCodes.NewZealand;

		protected override string[] SupportedTransportModes => new[] { TransportModes.Air };

		protected override string GetBillHumanReadableName(string billNumber) => $"Consignment: (HAWB: {billNumber})";

		protected override BaseHVLVRelatedJobCommand GetRelatedJobCommand(ForwardingShipment shipment) => new NZAirCargoReportCommand(shipment);

		protected override BusinessObject SetupExistingRelatedCustomsJob(ForwardingShipment shipment)
		{
			var result = Factory.New<ICusMAWB>();
			result.CM_MessageReference = "TestReference";
			return (BusinessObject)result;
		}

		protected override IEnumerable<Shipment> GetDataObjectsContainingDataTarget(Shipment topLevelDataObject) => Enumerable.Empty<Shipment>();

		protected override ZString GetExistingRelatedCustomsJobReference(BusinessObject existingJob) => ((ICusMAWB)existingJob).CM_MessageReference;
	}
}
