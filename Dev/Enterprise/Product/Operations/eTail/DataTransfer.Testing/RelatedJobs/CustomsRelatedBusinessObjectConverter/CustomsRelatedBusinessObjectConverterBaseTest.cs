using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.CustomsReferenceNumberType;

namespace Enterprise.eTail.DataTransfer.Testing
{
	abstract class CustomsRelatedBusinessObjectConverterBaseTest<TConverter> : TestCaseWithFactory
		where TConverter : CustomsRelatedBusinessObjectConverter
	{
		public void TestConstructor_InstantiateByRelatedJobCommand()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

			CombineAssertions("Converter instantiated correctly", () =>
			{
				AssertEquals("Converter shipment should be populated from command", shipment.PK, converter.Shipment.PK);

				var factoryGetter = typeof(CustomsRelatedBusinessObjectConverter).GetProperty("Factory", BindingFlags.NonPublic | BindingFlags.Instance);
				var converterFactory = factoryGetter.GetValue(converter) as BusinessObjectFactory;
				AssertNotEquals("Converter should create a new factory", Factory._Instance, converterFactory._Instance);

				AssertEquals("Converter and converter shipment should use the same factory", converter.Shipment.Factory._Instance, converterFactory._Instance);
			});
		}

		public void TestConstructor_InstantiateByRelatedJobCommandAndShipment()
		{
			var converterCtor = typeof(TConverter).GetConstructor(new[] { typeof(BaseHVLVRelatedJobCommand), typeof(ForwardingShipment) });
			if (converterCtor != null)
			{
				var newFactory = new BusinessObjectFactory();
				var shipment = newFactory.New<ForwardingShipment>();
				var command = GetRelatedJobCommand(shipment);
				var converter = converterCtor.Invoke(new object[] { command, shipment }) as TConverter;

				CombineAssertions("Converter instantiated correctly", () =>
				{
					AssertEquals("Converter shipment should be populated", shipment.PK, converter.Shipment.PK);

					var factoryGetter = typeof(CustomsRelatedBusinessObjectConverter).GetProperty("Factory", BindingFlags.NonPublic | BindingFlags.Instance);
					var converterFactory = factoryGetter.GetValue(converter) as BusinessObjectFactory;
					AssertEquals("Converter should use same factory as the shipment", newFactory._Instance, converterFactory._Instance);

					AssertEquals("Converter and converter shipment should use the same factory", converter.Shipment.Factory._Instance, converterFactory._Instance);
				});
			}
			else
			{
				Assert("Converter has no constructor taking 2 arguments, this test case is skipped", true);
			}
		}

		public void TestTryConvert_ConvertingProgressFormCaption_Air()
		{
			AssertConvertingProgressFormCaption(TransportModes.Air);
		}

		public void TestTryConvert_ConvertingProgressFormCaption_Sea()
		{
			AssertConvertingProgressFormCaption(TransportModes.Sea);
		}

		public void TestTryConvert_ConvertingProgressFormCaption_Road()
		{
			AssertConvertingProgressFormCaption(TransportModes.Road);
		}

		void AssertConvertingProgressFormCaption(string transportMode)
		{
			if (SupportedTransportModes.Contains(transportMode))
			{
				var shipment = SetupTestShipment(transportMode);

				var trackLogs = new List<(string Caption, string Progress, int Percentage)>();
				var converter = CreateCustomsRelatedBusinessObjectConverter(shipment, trackLogs);

				using (RegistryItemToEnable?.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(LoginCountry))
				{
					converter.TryConvert(out var errorMessage);
					var logCaptions = trackLogs.Select(log => log.Caption);

					CombineAssertions($"Checking for converting {transportMode} shipment to {converter.TargetBusinessObjectDisplayName}", () =>
					{
						Assert(logCaptions.Any(c => c.Equals($"Reading into {converter.TargetBusinessObjectDisplayName}")));
						Assert(logCaptions.Any(c => c.Equals($"{converter.TargetBusinessObjectDisplayName} generated successfully.")));
					});
				}
			}
			else
			{
				PassAssertionForNonSupportedTransportMode(transportMode);
			}
		}

		public void TestTryConvert_UpdateProgressActions_Air()
		{
			AssertUpdateProgressActions(TransportModes.Air);
		}

		public void TestTryConvert_UpdateProgressActions_Sea()
		{
			AssertUpdateProgressActions(TransportModes.Sea);
		}

		public void TestTryConvert_UpdateProgressActions_Road()
		{
			AssertUpdateProgressActions(TransportModes.Road);
		}

		void AssertUpdateProgressActions(string transportMode)
		{
			if (SupportedTransportModes.Contains(transportMode))
			{
				var shipment = SetupTestShipment(transportMode);
				var trackLogs = new List<(string Caption, string Progress, int Percentage)>();
				var converter = CreateCustomsRelatedBusinessObjectConverter(shipment, trackLogs);

				using (RegistryItemToEnable?.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(LoginCountry))
				{
					Assert("precondition: Converter works", converter.TryConvert(out var errorMessage));

					CombineAssertions($"Checking for converting {transportMode} shipment to {converter.TargetBusinessObjectDisplayName}", () =>
					{
						AssertTrackLog(trackLogs[0], "Loading HVLV Consignment data", string.Empty, 0);
						AssertTrackLog(trackLogs[1], string.Empty, "Preparing data source", 0);
						AssertTrackLog(trackLogs[2], string.Empty, "[1 / 2] HVLV Consignments loaded", 50);
						AssertTrackLog(trackLogs[3], string.Empty, "[2 / 2] HVLV Consignments loaded", 100);
						AssertTrackLog(trackLogs[4], $"Reading into {converter.TargetBusinessObjectDisplayName}", string.Empty, 0);

						if (ShouldUpdateProgressForPopulatingHouseBills)
						{
							AssertTrackLog(trackLogs[5], string.Empty, $"[1 / 2] {GetBillHumanReadableName("HVC00001")} generated", 50);
							AssertTrackLog(trackLogs[6], string.Empty, $"[2 / 2] {GetBillHumanReadableName("HVC00002")} generated", 100);
							AssertTrackLog(trackLogs[7], string.Empty, $"{converter.CustomsRelatedBusinessCollection.First().HumanReadableShortcutName} generated", 100);
							AssertTrackLog(trackLogs[8], $"{converter.TargetBusinessObjectDisplayName} generated successfully.", string.Empty, 0);
							AssertEquals("9 logs recorded", 9, trackLogs.Count);
						}
						else
						{
							AssertTrackLog(trackLogs[5], string.Empty, $"{converter.CustomsRelatedBusinessCollection.First().HumanReadableShortcutName} generated", 100);
							AssertTrackLog(trackLogs[6], $"{converter.TargetBusinessObjectDisplayName} generated successfully.", string.Empty, 0);
							AssertEquals("7 logs recorded", 7, trackLogs.Count);
						}
					});
				}
			}
			else
			{
				PassAssertionForNonSupportedTransportMode(transportMode);
			}

			void AssertTrackLog((string Caption, string Progress, int Percentage) log, string expectedCaption, string expectedProgress, int expectedPercentage)
			{
				AssertEquals(expectedCaption, log.Caption);
				AssertEquals(expectedProgress, log.Progress);
				AssertEquals(expectedPercentage, log.Percentage);
			}
		}

		public void TestTryConvert_ExportedUniversalDataObjectShouldHaveDeclarantType_Air()
		{
			AssertExportedUniversalDataObjectShouldHaveDeclarantType(TransportModes.Air);
		}

		public void TestTryConvert_ExportedUniversalDataObjectShouldHaveDeclarantType_Sea()
		{
			AssertExportedUniversalDataObjectShouldHaveDeclarantType(TransportModes.Sea);
		}

		public void TestTryConvert_ExportedUniversalDataObjectShouldHaveDeclarantType_Road()
		{
			AssertExportedUniversalDataObjectShouldHaveDeclarantType(TransportModes.Road);
		}

		void AssertExportedUniversalDataObjectShouldHaveDeclarantType(string transportMode)
		{
			if (SupportedTransportModes.Contains(transportMode))
			{
				var shipment = SetupTestShipment(transportMode);
				shipment.ArrivalConsol.JK_AgentType = AgentType.CoLoad;
				Factory.Save();

				var exportedDataObject = GetExportedDataObject(shipment);
				AssertEquals(ExpectedDeclarantType, exportedDataObject.DeclarantType.Code);
			}
			else
			{
				PassAssertionForNonSupportedTransportMode(transportMode);
			}
		}

		public void TestTryConvert_ShouldPopulateEntryHeaderCollection_Air()
		{
			AssertPopulateEntryHeaderCollection(TransportModes.Air);
		}

		public void TestTryConvert_ShouldPopulateEntryHeaderCollection_Sea()
		{
			AssertPopulateEntryHeaderCollection(TransportModes.Sea);
		}

		public void TestTryConvert_ShouldPopulateEntryHeaderCollection_Road()
		{
			AssertPopulateEntryHeaderCollection(TransportModes.Road);
		}

		void AssertPopulateEntryHeaderCollection(string transportMode)
		{
			var shipment = SetupTestShipment(transportMode);
			var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

			using (RegistryItemToEnable?.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Latvia))
			{
				var exportedDataObject = GetExportedDataObject(shipment);

				CombineAssertions("Should populate entry header with country code of current company", () =>
				{
					AssertNotNull("EntryHeaderCollection should be added", exportedDataObject.EntryHeaderCollection);
					AssertEquals("EntryHeaderCollection should contain 1 element", 1, exportedDataObject.EntryHeaderCollection.Count);
					AssertEquals("EntryHeader should contain country code of current company",
						ShouldEntryHeaderContainDestinationCountryInsteadOfCurrentLoginCountry ? LoginCountry.ToString() : CountryCodes.Latvia,
						exportedDataObject.EntryHeaderCollection[0].Type.Code);
				});
			}
		}

		public void TestTryConvert_ShouldClearUnnecessaryJobDocAddress_Air()
		{
			AssertClearUnnecessaryJobDocAddress(TransportModes.Air);
		}

		public void TestTryConvert_ShouldClearUnnecessaryJobDocAddress_Sea()
		{
			AssertClearUnnecessaryJobDocAddress(TransportModes.Sea);
		}

		public void TestTryConvert_ShouldClearUnnecessaryJobDocAddress_Road()
		{
			AssertClearUnnecessaryJobDocAddress(TransportModes.Road);
		}

		void AssertClearUnnecessaryJobDocAddress(string transportMode)
		{
			if (SupportedTransportModes.Contains(transportMode))
			{
				var shipment = SetupTestShipment(transportMode);
				var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

				using (RegistryItemToEnable?.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(LoginCountry))
				{
					GetExportedDataObject(shipment, converter);

					var factoryGetter = typeof(CustomsRelatedBusinessObjectConverter).GetProperty("Factory", BindingFlags.Instance | BindingFlags.NonPublic);
					var converterFactory = factoryGetter.GetValue(converter) as BusinessObjectFactory;

					var query = new ZQuery();
					query.FetchOnlyFromLocalCache = true;
					var jobDocAddresses = converterFactory.Load<JobDocAddress>(query);

					Assert("No new JobDocAddress should exist in conversion factory", jobDocAddresses.All(j => j.IsInDatabase));
				}
			}
			else
			{
				PassAssertionForNonSupportedTransportMode(transportMode);
			}
		}

		public void TestTryConvert_ShouldCreateGenPivotLinkingConsignmentHeaderAndRelatedJob_Air()
		{
			AssertCreateGenPivotLinkingConsignmentHeaderAndRelatedJob(TransportModes.Air);
		}

		public void TestTryConvert_ShouldCreateGenPivotLinkingConsignmentHeaderAndRelatedJob_Sea()
		{
			AssertCreateGenPivotLinkingConsignmentHeaderAndRelatedJob(TransportModes.Sea);
		}

		public void TestTryConvert_ShouldCreateGenPivotLinkingConsignmentHeaderAndRelatedJob_Road()
		{
			AssertCreateGenPivotLinkingConsignmentHeaderAndRelatedJob(TransportModes.Road);
		}

		void AssertCreateGenPivotLinkingConsignmentHeaderAndRelatedJob(string transportMode)
		{
			if (SupportedTransportModes.Contains(transportMode))
			{
				var shipment = SetupTestShipment(transportMode);
				var header = shipment.GetHVLVConsignmentHeader();

				var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

				using (RegistryItemToEnable?.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(LoginCountry))
				{
					converter.TryConvert(out _);
					var factory = converter.CustomsRelatedBusinessCollection.First().Factory;
					factory.Save();

					var bizo = converter.CustomsRelatedBusinessCollection.Single();
					var genPivot = header.GenPivotCollection[0];

					CombineAssertions($"Checking for converting {transportMode} shipment to {converter.TargetBusinessObjectDisplayName}", () =>
					{
						AssertNotNull(genPivot);
						AssertEquals(genPivot.XX_RelationType, GenPivotTypes.HighVolumeLowValue);
						AssertEquals(genPivot.XX_Relation1ID, header.PK);
						AssertEquals(genPivot.XX_Relation2ID, bizo.PK);
						AssertEquals(genPivot.XX_Relation1TableCode, HVLVConsignmentHeaderSchema.Constants.Prefix);
						AssertEquals(genPivot.XX_Relation2TableCode, bizo.TablePrefix);
					});
				}
			}
			else
			{
				PassAssertionForNonSupportedTransportMode(transportMode);
			}
		}

		public void TestTryConvert_PopulatesHVI_LastUsageCode_Air()
		{
			AssertPopulatesHVI_LastUsageCode(TransportModes.Air);
		}

		public void TestTryConvert_PopulatesHVI_LastUsageCode_Sea()
		{
			AssertPopulatesHVI_LastUsageCode(TransportModes.Sea);
		}

		public void TestTryConvert_PopulatesHVI_LastUsageCode_Road()
		{
			AssertPopulatesHVI_LastUsageCode(TransportModes.Road);
		}

		void AssertPopulatesHVI_LastUsageCode(string transportMode)
		{
			if (SupportedTransportModes.Contains(transportMode))
			{
				var shipment = SetupTestShipment(transportMode);
				var items = shipment.HVLVItems.ToList();
				AssertEquals("Precondition: There are items on shipment.", 2, items.Count);

				var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

				using (RegistryItemToEnable?.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(LoginCountry))
				{
					converter.TryConvert(out _);

					AssertNotNullOrEmpty("The converter should be able to retrieve the UsageCode from RelatedJobCommand", converter.UsageCode);

					var resultsFactory = converter.CustomsRelatedBusinessCollection[0].Factory;

					var itemsInResultsFactory = items.Select(i => resultsFactory.Load<HVLVItem>(i.PK)).ToList();
					itemsInResultsFactory.ForEach(item =>
					{
						AssertEquals("Item LastUsageCode should be populated", converter.UsageCode, item.HVI_LastUsageCode);
					});
				}
			}
			else
			{
				PassAssertionForNonSupportedTransportMode(transportMode);
			}
		}

		public void TestTryConvert_GenerateTRFEventOnShipmentAndRelatedJob_Air()
		{
			AssertGenerateTRFEventOnShipmentAndRelatedJob(TransportModes.Air);
		}

		public void TestTryConvert_GenerateTRFEventOnShipmentAndRelatedJob_Sea()
		{
			AssertGenerateTRFEventOnShipmentAndRelatedJob(TransportModes.Sea);
		}

		public void TestTryConvert_GenerateTRFEventOnShipmentAndRelatedJob_Road()
		{
			AssertGenerateTRFEventOnShipmentAndRelatedJob(TransportModes.Road);
		}

		void AssertGenerateTRFEventOnShipmentAndRelatedJob(string transportMode)
		{
			if (SupportedTransportModes.Contains(transportMode))
			{
				var shipment = SetupTestShipment(transportMode);
				var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

				using (RegistryItemToEnable?.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(LoginCountry))
				{
					converter.TryConvert(out _);

					var converterFactory = converter.CustomsRelatedBusinessCollection.Single().Factory;
					converterFactory.Save();

					shipment.Logs.GetAllLogs().Reload(true);

					CombineAssertions($"Checking for converting {transportMode} shipment to {converter.TargetBusinessObjectDisplayName}", () =>
					{
						var trfLogs = shipment.Logs.GetAllLogs().OfType<StmALog>().Where(l => l.SL_SE_NKEvent == "TRF");
						AssertEquals("1 TRF event should be added to shipment", 1, trfLogs.Count());

						var rfnCode = ((ICodeDescription)converter.CustomsRelatedBusinessCollection.FirstOrDefault()).Code;
						AssertEquals("New TRF event should have reason Cargo Report Created", $"|MOD={transportMode}|RFN={rfnCode}|TYP={GetExpectedTRFEventType(converter)}", trfLogs.First().SL_Reference);

						if (converter.CustomsRelatedBusinessCollection.Single() is IStmALogParent logParent)
						{
							trfLogs = logParent.Logs.GetAllLogs().OfType<StmALog>().Where(l => l.SL_SE_NKEvent == "TRF");
							AssertEquals("1 TRF event should be added to related job", 1, trfLogs.Count());

							var trfLogsPattern = $@"\|(JOB|RFN)={shipment.JobNumber}\|TYP=HVL";
							AssertMatch(new System.Text.RegularExpressions.Regex(trfLogsPattern), trfLogs.First().SL_Reference);

							if (ShouldAddTransferredLogToRelatedJobOnEveryConversion)
							{
								converter.TryConvert(out _);
								converterFactory.Save();

								trfLogs = logParent.Logs.GetAllLogs().OfType<StmALog>().Where(l => l.SL_SE_NKEvent == "TRF");
								AssertEquals("A new TRF event should be added to related job", 2, trfLogs.Count());
							}
						}
					});
				}
			}
			else
			{
				PassAssertionForNonSupportedTransportMode(transportMode);
			}
		}

		public void TestTryConvert_Exceptions_ErrorDetails()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = SetupTestShipment(TransportModes.Air);
				shipment.JS_UniqueConsignRef = "SE000001";
				Factory.Save();

				var converter = new CustomsRelatedBusinessObjectConverterForTest(new HVLVRelatedJobCommandForTest(shipment), false);
				var success = converter.TryConvert(out var errorMsg);

				Assert(!success);
				AssertEquals("The method or operation is not implemented.", errorMsg);
			}
		}

		public void TestTryConvert_Exceptions_FailToMatchExistingJob()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = SetupTestShipment(TransportModes.Air);
				shipment.JS_UniqueConsignRef = "SE000001";

				var existingJob = Factory.New<Customs.AU.Declaration.Business.CusMAWB>();
				Factory.Save();

				var command = new HVLVRelatedJobCommandWithExistingJob(shipment, existingJob);
				var converter = command.Converter;

				existingJob.CM_MessageReference = "TestReference";
				var success = converter.TryConvert(out var errorMsg);

				Assert("TryConvert should fail", !success);
				AssertEquals("Match couldn't be found for AirManifest with Key TestReference", errorMsg);
			}
		}

		public void TestNonWesternEuropeanCharactersRemovalService()
		{
			var shipment = SetupTestShipment(TransportModes.Air);
			var converterWithoutService = new CustomsRelatedBusinessObjectConverterForTest(new HVLVRelatedJobCommandForTest(shipment), shouldStripNonWesternEuropeanCharacters: false);

			using (RegistryItemToEnable?.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				converterWithoutService.TryConvert(out _);
				AssertNull("Service should be removed after conversion", converterWithoutService.Factory_Exposed.ServiceContainer.GetService<NonWesternEuropeanCharactersRemovalService>());
				AssertEquals("Converter should not add service when ShouldStripNonWesternEuropeanCharacters is false", false, converterWithoutService.IsNonWesternEuropeanCharactersRemovalServiceAddedToFactory);

				var converterWithService = new CustomsRelatedBusinessObjectConverterForTest(new HVLVRelatedJobCommandForTest(shipment), shouldStripNonWesternEuropeanCharacters: true);
				converterWithService.TryConvert(out _);
				AssertNull("Service should be removed after conversion", converterWithService.Factory_Exposed.ServiceContainer.GetService<NonWesternEuropeanCharactersRemovalService>());
				AssertEquals("Converter should add service when ShouldStripNonWesternEuropeanCharacters is true", true, converterWithService.IsNonWesternEuropeanCharactersRemovalServiceAddedToFactory);
			}
		}

		protected void AssertConvertShipmentToCargoReport_EndToEnd(string transportMode, string houseBillTablePrefix, int houseBillLength = 2)
		{
			var shipment = SetupTestShipment(transportMode);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(LoginCountry))
			{
				var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

				var success = converter.TryConvert(out var errorMsg);
				Assert("Convert successfully", success);
				AssertNullOrEmpty("No error occured", errorMsg);

				var converterFactory = converter.CustomsRelatedBusinessCollection.Single().Factory;

				var masterBill = converter.CustomsRelatedBusinessCollection.Single();
				AssertNotNull("New masterBill should be created", masterBill);
				Assert("New masterBill should not be saved", !masterBill.IsInDatabase);

				var houseBillType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(houseBillTablePrefix);
				var houseBills = converterFactory.Load(houseBillType, new ZQuery());
				AssertEquals(houseBillLength + " houseBills should be created", houseBillLength, houseBills.Length);
				Assert("New houseBill should not be saved", houseBills.All(h => !h.IsInDatabase));

				converterFactory.Save();

				var hlrLogs = shipment.Logs.GetAllLogs().OfType<StmALog>().Where(l => l.SL_SE_NKEvent == "HLR");
				AssertEquals("1 HLR event should be added to shipment", 1, hlrLogs.Count());
				AssertEquals("New HLR event should have reason Cargo Report Created", "|RES=Cargo Report Created", hlrLogs.First().SL_Reference);
			}
		}

		public void AssertConvertShipmentToCargoReport_Cancel(string transportMode, string masterBillTablePrefix, string houseBillTablePrefix)
		{
			var shipment = SetupTestShipment(transportMode);
			var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);
			converter.SetUpProgressUpdate(null);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(LoginCountry))
			{
				converter.Cancel();
				var success = converter.TryConvert(out var errorMsg);
				Assert(!success);
				AssertEquals("The operation was canceled.", errorMsg);

				var masterBillType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(masterBillTablePrefix);
				var masterBill = Factory.Load(masterBillType, new ZQuery()).SingleOrDefault();
				AssertNull("No masterBill should be created", masterBill);

				var houseBillType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(houseBillTablePrefix);
				var houseBills = Factory.Load(houseBillType, new ZQuery());
				AssertEquals("No houseBills should be created", 0, houseBills.Length);

				var hlrLogs = shipment.Logs.GetAllLogs().OfType<StmALog>().Where(l => l.SL_SE_NKEvent == "HLR");
				AssertEquals("No HLR event should be added to shipment", 0, hlrLogs.Count());
			}
		}

		public void TestExportedUniversalShipmentMessageTypeCode()
		{
			using (RegistryItemToEnable?.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(LoginCountry))
			{
				foreach (var transportMode in SupportedTransportModes)
				{
					var shipment = SetupTestShipment(transportMode);
					AssertExportedUniversalShipmentMessageTypeCode(shipment, GetExpectedMessageTypeCode(shipment));
				}
			}
		}

		protected void AssertExportedUniversalShipmentMessageTypeCode(ForwardingShipment shipment, string expectedMessageTypeCode)
		{
			var tracker = new ConvertTracker(1, default, null);
			var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);
			var universalShipment = converter.ExportShipmentAsUniversalDataObject(tracker, checkSubShipments: false);
			AssertEquals($"Transport mode: {shipment.JS_TransportMode}", expectedMessageTypeCode, universalShipment.MessageType.Code.GetValueOrDefault());
		}

		public void TestExportedUniversalShipmentDataTarget()
		{
			using (RegistryItemToEnable?.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(LoginCountry))
			{
				foreach (var transportMode in SupportedTransportModes)
				{
					var shipment = SetupTestShipment(transportMode);
					var header = HVLVConsignmentHeader.GetOrCreate(shipment);

					var existingJob = SetupExistingRelatedCustomsJob(shipment);

					var pivot = Factory.New<GenPivot>();
					pivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
					pivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;
					pivot.XX_Relation1ID = header.PK;
					pivot.XX_Relation2TableCode = existingJob.TablePrefix;
					pivot.XX_Relation2ID = existingJob.PK;

					Factory.Save();
					
					var command = GetRelatedJobCommand(shipment);
					var universalShipment = GetExportedDataObject(shipment, (TConverter)command.Converter);

					CombineAssertions("Existing job reference should be populated to data target", () =>
					{
						var dataObjects = GetDataObjectsContainingDataTarget(universalShipment);
						if (dataObjects.Any())
						{
							foreach (var dataObject in dataObjects)
							{
								AssertHasDataTarget(dataObject, command.Converter.MasterBillDataContextType, GetExistingRelatedCustomsJobReference(existingJob));
							}
						}
						else
						{
							Assert(true);
						}
					});
				}
			}

			void AssertHasDataTarget(Shipment universalShipment, DataContextType dataContextType, ZString key)
			{
				AssertNotNull(universalShipment.DataContext);
				var dataTarget = universalShipment.DataContext.GetMatchingDataTarget(dataContextType);
				AssertNotNull(dataTarget);
				AssertEquals(key, dataTarget.Key);
			}
		}

		protected virtual IEnumerable<Shipment> GetDataObjectsContainingDataTarget(Shipment topLevelDataObject)
		{
			yield return topLevelDataObject;
		}

		public void TestWhenCustomsJobCancelled_CanCreateAgain_Air()
		{
			AssertWhenCustomsJobCancelled_CanCreateAgain(TransportModes.Air);
		}

		public void TestWhenCustomsJobCancelled_CanCreateAgain_Sea()
		{
			AssertWhenCustomsJobCancelled_CanCreateAgain(TransportModes.Sea);
		}

		public void TestWhenCustomsJobCancelled_CanCreateAgain_Road()
		{
			AssertWhenCustomsJobCancelled_CanCreateAgain(TransportModes.Road);
		}

		void AssertWhenCustomsJobCancelled_CanCreateAgain(string transportMode)
		{
			if (SupportedTransportModes.Contains(transportMode))
			{
				var shipment = SetupTestShipment(transportMode);
				var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

				using (RegistryItemToEnable?.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(LoginCountry))
				{
					converter.TryConvert(out _);

					var customsJob = converter.CustomsRelatedBusinessCollection.Single();
					customsJob.Factory.Save();

					var command = GetRelatedJobCommand(shipment);
					Assert("Precondition: CanCreate is false", !command.CanCreate);

					if (customsJob is ICancellable cancellableCustomsJob)
					{
						cancellableCustomsJob.IsCancelled = true;
						customsJob.Factory.Save();
						Assert("CanCreate is true after job is cancelled", command.CanCreate);
					}
				}
			}
			else
			{
				PassAssertionForNonSupportedTransportMode(transportMode);
			}
		}

		public void TestConvertToRelatedJob_ExistingJobIsDeactivited_ShouldGenerateNewJob()
		{
			var shipment = SetupTestShipment(SupportedTransportModes[0]);
			var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);

			using (RegistryItemToEnable?.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(LoginCountry))
			{
				var success = converter.TryConvert(out var message);
				Assert("Pre-req: Convert succeed.", success);
				var relatedJob = converter.CustomsRelatedBusinessCollection.Single();

				if (relatedJob is ICancellable cancellableJob)
				{
					cancellableJob.IsCancelled = true;
					relatedJob.Factory.Save();
					Assert("Job is cancelled.", cancellableJob.IsCancelled);

					converter = CreateCustomsRelatedBusinessObjectConverter(shipment);
					success = converter.TryConvert(out _);
					Assert("Pre-req: Convert succeed.", success);

					AssertNotEquals("Should generate new job", relatedJob.PK, converter.CustomsRelatedBusinessCollection.Single().PK);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestConvertCoreUsesPerformanceStatistics()
		{
			var stats = new PerformanceStatisticsCollectorForTest();

			AssertEquals("Should have no record for ExportShipmentAsUniversalDataObject", 0, stats.CollectedStats.Count(x => x.Contains("ExportShipmentAsUniversalDataObject")));
			AssertEquals("Should have no record for ReadIntoCustomsRelatedBusiness", 0, stats.CollectedStats.Count(x => x.Contains("ReadIntoCustomsRelatedBusiness")));

			var shipment = SetupTestShipment(TransportModes.Air);
			var converter = new CustomsRelatedBusinessObjectConverterForTest(new HVLVRelatedJobCommandForTest(shipment), true);

			using (ObjectFactory.Substitute<IPerformanceStatisticsCollector>(stats))
			using (RegistryItemToEnable?.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				PerformanceStatisticsCollector.ResetInstance();
				_ = converter.TryConvert(out _);
			}

			AssertNotNull("Precondition: CollectedStats has been made", stats.CollectedStats);
			AssertEquals("Should have 1 record added for ExportShipmentAsUniversalDataObject", 1, stats.CollectedStats.Count(x => x.Contains("ExportShipmentAsUniversalDataObject")));
			AssertEquals("Should have 1 record added for ReadIntoCustomsRelatedBusiness", 1, stats.CollectedStats.Count(x => x.Contains("ReadIntoCustomsRelatedBusiness")));
		}

		public virtual void TestExportedUniversalShipment_HasAdditionalReferenceCollection()
		{
			var forwardinghipment = SetupTestShipment(TransportModes.Air);
			var consignment = forwardinghipment.HVLVConsignments.First() as HVLVConsignment;

			var reference = consignment.CustomsReferenceNumbers.AddNew();
			reference.CE_EntryType = CustomsAdditionalReferenceNumbersCodes.ExporterEORINumber;
			reference.CE_EntryNum = "1234";

			Factory.Save();

			var exportedDataObject = GetExportedDataObject(forwardinghipment);

			var additionalReferenceCollection = exportedDataObject.SubShipmentCollection[0].SubShipmentCollection[0].AdditionalReferenceCollection;

			if (ShouldExportAdditionalReferenceCollection)
			{
				AssertNotNull("AdditionalReferenceCollection", additionalReferenceCollection);
				AssertEquals("1234", additionalReferenceCollection[0].ReferenceNumber);
			}
			else
			{
				AssertNull("AdditionalReferenceCollection", additionalReferenceCollection);
			}
		}

		public void TestTryConvert_LastUsageCodeCannotBeEmpty()
		{
			using (RegistryItemToEnable?.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(LoginCountry))
			{
				var shipment = SetupTestShipment(SupportedTransportModes[0]);
				shipment.JS_UniqueConsignRef = "SE000001";
				Factory.Save();

				var converter = CreateCustomsRelatedBusinessObjectConverter(shipment, relatedJobCommand: new HVLVRelatedJobCommandForTest(shipment));

				var result = converter.TryConvert(out _);

				var expectedErrorMessage = $@"Usage Code should not be null or empty.

-- Additional Information --

ShipmentPK: {shipment.PK}
Type of CustomsRelatedBusinessObjectConverter: {typeof(TConverter).Name}
TargetBusinessObjectDisplayName: RelatedJobCommandForTest_JobName
Type of relatedJobCommand: HVLVRelatedJobCommandForTest
";
				Assert("Precondition: The conversion should have succeeded.", result);
				AssertEquals(expectedErrorMessage, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestExportedUniversalShipment_TopLevelConsol_WhenImportAndExportFromLoginCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(LoginCountry))
			{
				AssertExportedUniversalShipment_TopLevelDataObjectIsRelevantConsol(isDestinationSameAsLoginCountry: true, GetExpectedPickupOrDeliveryRole(Directions.Import));
				AssertExportedUniversalShipment_TopLevelDataObjectIsRelevantConsol(isDestinationSameAsLoginCountry: false, GetExpectedPickupOrDeliveryRole(Directions.Export));
			}
		}

		protected void AssertExportedUniversalShipment_TopLevelDataObjectIsRelevantConsol(bool isDestinationSameAsLoginCountry, RecipientRoleType expectedPickupOrDeliveryRole)
		{
			var shipment = SetupTestShipment(TransportModes.Air, isDestinationSameAsLoginCountry: isDestinationSameAsLoginCountry, addAdditionalDepartureConsol: true);
			var dataObject = GetExportedDataObject(shipment);

			var expectArrivalConsol = expectedPickupOrDeliveryRole == RecipientRoleType.DCA;
			string expectedWaybillNumber;
			if (expectArrivalConsol)
			{
				expectedWaybillNumber = "123";
			}
			else
			{
				expectedWaybillNumber = "321";
			}

			AssertEquals($"test shipment IsImportToLoginCountry? {isDestinationSameAsLoginCountry}: Waybill Number", expectedWaybillNumber, dataObject.WayBillNumber.Value.Replace("-", ""));
		}

		#region Implementations

		protected abstract ZString LoginCountry { get; }

		protected abstract string[] SupportedTransportModes { get; }

		protected virtual RecipientRoleType GetExpectedPickupOrDeliveryRole(Directions shipmentDirection) => shipmentDirection == Directions.Import ? RecipientRoleType.DCA : RecipientRoleType.PCA;

		protected abstract BaseHVLVRelatedJobCommand GetRelatedJobCommand(ForwardingShipment shipment);

		protected virtual bool ShouldGenerateHLREventOnConversionFactorySaving => true;

		protected virtual string GetExpectedTRFEventType(TConverter converter) => converter.TargetBusinessObjectDisplayName;

		protected virtual bool ShouldEntryHeaderContainDestinationCountryInsteadOfCurrentLoginCountry => false;

		protected virtual string GetBillHumanReadableName(string billNumber) => billNumber;

		protected virtual IRegistryItem RegistryItemToEnable => null;

		protected virtual string GetExpectedMessageTypeCode(ForwardingShipment shipment) => string.Empty;

		protected virtual string ExpectedDeclarantType => AgentType.CoLoad;

		protected abstract BusinessObject SetupExistingRelatedCustomsJob(ForwardingShipment shipment);

		protected virtual ZString GetExistingRelatedCustomsJobReference(BusinessObject existingJob) => string.Empty;

		protected virtual bool ShouldExportAdditionalReferenceCollection => false;

		protected virtual bool ShouldUpdateProgressForPopulatingHouseBills => true;

		protected virtual bool ShouldAddTransferredLogToRelatedJobOnEveryConversion => true;

		protected ZString ForeignCountryForTestData => CountryCodes.Australia != LoginCountry ? CountryCodes.Australia : CountryCodes.NewZealand;

		protected ZString MidpointCountryForTestData => CountryCodes.China != LoginCountry ? CountryCodes.China : CountryCodes.Singapore;

		protected RefUNLOCO GetOriginPort(bool isDestinationSameAsLoginCountry) => isDestinationSameAsLoginCountry ? foreignCountryUNLOCO : loginCountryUNLOCO;

		protected RefUNLOCO GetDestinationPort(bool isDestinationSameAsLoginCountry) => isDestinationSameAsLoginCountry ? loginCountryUNLOCO : foreignCountryUNLOCO;

		protected ForwardingShipment SetupTestShipment(string transportMode, bool isDestinationSameAsLoginCountry = true, bool addAdditionalDepartureConsol = false)
		{
			var origin = GetOriginPort(isDestinationSameAsLoginCountry);
			var destination = GetDestinationPort(isDestinationSameAsLoginCountry);

			var arrivalConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			arrivalConsol.JK_TransportMode = transportMode;
			arrivalConsol.JK_MasterBillNum = "123";
			arrivalConsol.JK_RL_NKLoadPort = origin.Code;
			arrivalConsol.JK_RL_NKDischargePort = destination.Code;

			var container = arrivalConsol.Containers.AddNew();
			container.ContainerNumberForBinding = "Container001";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = transportMode;
			shipment.JS_HouseBill = "UWM97N872947";
			shipment.JS_RL_NKOrigin = origin.Code;
			shipment.JS_RL_NKDestination = destination.Code;

			arrivalConsol.Shipments.Add(shipment);

			var item1 = Factory.NewWithValidTestData<HVLVItem>();
			item1.HVI_JS_LoadedOnShipment = shipment.PK;
			item1.HVI_ContainerNumber = container.ContainerNumberForBinding;

			var consignment1 = item1.Consignment;
			consignment1.HVC_WaybillNumber = "HVC00001";
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;

			var item2 = Factory.NewWithValidTestData<HVLVItem>();
			item2.HVI_JS_LoadedOnShipment = shipment.PK;
			item2.HVI_ContainerNumber = container.ContainerNumberForBinding;

			var consignment2 = item2.Consignment;
			consignment2.HVC_WaybillNumber = "HVC00002";
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;

			if (addAdditionalDepartureConsol)
			{
				AddDepartureConsolToTestShipment(shipment);
			}

			Factory.Save();
			return shipment;
		}

		void AddDepartureConsolToTestShipment(ForwardingShipment shipment)
		{
			var arrivalConsol = shipment.ArrivalConsol;

			var departureConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			departureConsol.JK_TransportMode = shipment.TransportMode;
			departureConsol.JK_MasterBillNum = "321";

			var departureContainer = departureConsol.Containers.AddNew();
			departureContainer.ContainerNumberForBinding = "DEPARTURE";

			departureConsol.Shipments.Add(shipment);

			departureConsol.JK_RL_NKLoadPort = shipment.JS_RL_NKOrigin;

			departureConsol.JK_RL_NKDischargePort = midpointCountryUNLOCO.Code;
			arrivalConsol.JK_RL_NKLoadPort = midpointCountryUNLOCO.Code;

			AssertEquals("Precondition: Ensure correct departure consol is determined", departureConsol, shipment.Consols.GetEarliestConsol());
			AssertEquals("Precondition: Ensure correct arrival consol is determined", arrivalConsol, shipment.Consols.GetLatestConsol());
		}

		protected TConverter CreateCustomsRelatedBusinessObjectConverter(ForwardingShipment shipment, List<(string Caption, string Progress, int Percentage)> trackLogs = null, BaseHVLVRelatedJobCommand relatedJobCommand = null)
		{
			trackLogs ??= new List<(string Caption, string Progress, int Percentage)>();
			relatedJobCommand ??= GetRelatedJobCommand(shipment);

			var converterCtor = typeof(TConverter).GetConstructor(new[] { typeof(BaseHVLVRelatedJobCommand) });
			var converter = converterCtor.Invoke(new object[] { relatedJobCommand }) as TConverter;
			converter.SetUpProgressUpdate((caption, progress, percentage) => trackLogs.Add((caption, progress, percentage)));
			return converter;
		}

		protected BusinessObject ConvertToRelatedJob<TRelatedJob>(ForwardingShipment shipment) where TRelatedJob : class
		{
			var converter = CreateCustomsRelatedBusinessObjectConverter(shipment);
			var succeed = converter.TryConvert(out var errorMessage);

			CombineAssertions("Convert succeed", () =>
			{
				Assert(succeed);
				AssertNullOrEmpty(errorMessage);
			});

			var job = converter.CustomsRelatedBusinessCollection.Single();

			AssertNotNull("Job is created", job);

			return job;
		}

		protected Shipment GetExportedDataObject(ForwardingShipment shipment, TConverter converter = null)
		{
			if (converter == null)
			{
				converter = CreateCustomsRelatedBusinessObjectConverter(shipment);
			}

			var tracker = new ConvertTracker(1, default, default);
			return converter.ExportShipmentAsUniversalDataObject(tracker, checkSubShipments: true);
		}

		void PassAssertionForNonSupportedTransportMode(string transportMode)
		{
			Assert($"{nameof(TConverter)} doesn't support {transportMode} transport", true);
		}

		protected override void SetUp()
		{
			base.SetUp();

			loginCountryUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, $"{LoginCountry}TST"));
			if (loginCountryUNLOCO == null)
			{
				loginCountryUNLOCO = Factory.NewWithValidTestData<RefUNLOCO>();
				loginCountryUNLOCO.RL_Code = $"{LoginCountry}TST";
			}

			midpointCountryUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, $"{MidpointCountryForTestData}TST"));
			if (midpointCountryUNLOCO == null)
			{
				midpointCountryUNLOCO = Factory.NewWithValidTestData<RefUNLOCO>();
				midpointCountryUNLOCO.RL_Code = $"{MidpointCountryForTestData}TST";
			}

			foreignCountryUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, $"{ForeignCountryForTestData}TST"));
			if (foreignCountryUNLOCO == null)
			{
				foreignCountryUNLOCO = Factory.NewWithValidTestData<RefUNLOCO>();
				foreignCountryUNLOCO.RL_Code = $"{ForeignCountryForTestData}TST";
			}
		}

		RefUNLOCO loginCountryUNLOCO;
		RefUNLOCO midpointCountryUNLOCO;
		RefUNLOCO foreignCountryUNLOCO;

		class CustomsRelatedBusinessObjectConverterForTest : CustomsRelatedBusinessObjectConverter
		{
			public CustomsRelatedBusinessObjectConverterForTest(BaseHVLVRelatedJobCommand relatedJobCommand, bool shouldStripNonWesternEuropeanCharacters, DataContextType dataContextType = default)
				: base(relatedJobCommand)
			{
				ShouldStripNonWesternEuropeanCharacters = shouldStripNonWesternEuropeanCharacters;
				SetUpProgressUpdate((caption, progress, percentage) =>
				{
					IsNonWesternEuropeanCharactersRemovalServiceAddedToFactory = Factory_Exposed.ServiceContainer.GetService<NonWesternEuropeanCharactersRemovalService>() != null;
				});
				this.dataContextType = dataContextType;
			}

			public bool IsNonWesternEuropeanCharactersRemovalServiceAddedToFactory { get; private set; }

			public BusinessObjectFactory Factory_Exposed
			{
				get
				{
					var factoryProperty = typeof(CustomsRelatedBusinessObjectConverter).GetProperty("Factory", BindingFlags.Instance | BindingFlags.NonPublic);
					return factoryProperty.GetValue(this) as BusinessObjectFactory;
				}
			}

			protected override bool ShouldStripNonWesternEuropeanCharacters { get; }

			public override DataContextType MasterBillDataContextType => dataContextType;
			readonly DataContextType dataContextType;

			public RecipientRoleType PickupOrDeliveryCartageRole_ForTest => PickupOrDeliveryCartageRole;
		}

		[ApplicableLoginCountry(new string[] { })]
		class HVLVRelatedJobCommandForTest : BaseHVLVRelatedJobCommand
		{
			public HVLVRelatedJobCommandForTest(ForwardingShipment shipment)
				: base(shipment)
			{
			}

			public override MultilingualString RelatedJobName => (NoResString)"RelatedJobCommandForTest_JobName";

			public override CustomsRelatedBusinessObjectConverter Converter => throw new NotImplementedException();

			public override string UsageCode => string.Empty;

			protected override Type RelatedCustomsJobType => throw new NotImplementedException();
		}

		[ApplicableLoginCountry(new string[] { })]
		class HVLVRelatedJobCommandWithExistingJob : BaseHVLVRelatedJobCommand
		{
			public HVLVRelatedJobCommandWithExistingJob(ForwardingShipment shipment, BusinessObject existingJob)
				: base(shipment)
			{
				this.existingJob = existingJob;

				var header = shipment.GetOrCreateHVLVConsignmentHeader();
				header.GenPivotCollection.AddRelatedIfNotExist(existingJob);
			}

			readonly BusinessObject existingJob;

			public override MultilingualString RelatedJobName => (NoResString)"RelatedJobCommandWithExistingJob_JobName";

			public override CustomsRelatedBusinessObjectConverter Converter => new CustomsRelatedBusinessObjectConverterForTest(this, false, DataContextType.AirManifest);

			public override string UsageCode => "ACR";

			protected override Type RelatedCustomsJobType => existingJob.GetType();
		}

		#endregion
	}
}
