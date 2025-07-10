using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(JobShipmentModule))]
	public class JobShipmentModuleTest : ZModuleBasherTest
	{
		[RequiresSTA]
		public void TestSetGuiProviders()
		{
			using (JobShipmentModuleForTest module = new JobShipmentModuleForTest())
			{
				module.PerformSearch();

				var shipmentDocumentSupporterQueryProvider = module.Factory.GetValue<IForwardingShipmentDocumentSupporterQueryProvider>();
				AssertNotNull(shipmentDocumentSupporterQueryProvider);
				Assert(shipmentDocumentSupporterQueryProvider is ForwardingShipmentDocumentSupporterGuiQueryProvider);

				var servicesSelectionProvider = module.Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull(servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);

				module.PerformSearch();

				shipmentDocumentSupporterQueryProvider = module.Factory.GetValue<IForwardingShipmentDocumentSupporterQueryProvider>();
				AssertNotNull(shipmentDocumentSupporterQueryProvider);
				Assert(shipmentDocumentSupporterQueryProvider is ForwardingShipmentDocumentSupporterGuiQueryProvider);

				servicesSelectionProvider = module.Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull(servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);
			}
		}

		[RequiresSTA]
		public void TestDomainContextIsSet()
		{
			using (var module = new JobShipmentModuleForTest())
			{
				AssertEquals(FreightDomainContext.Forwarding, module.GridCollection.Factory.GetFreightDomainContext());

				module.PerformSearch();
				AssertEquals(FreightDomainContext.Forwarding, module.GridCollection.Factory.GetFreightDomainContext());

				module.PerformSearch();
				AssertEquals(FreightDomainContext.Forwarding, module.GridCollection.Factory.GetFreightDomainContext());
			}
		}

		[RequiresSTA]
		public void TestChildEditableServiceSetToShipment()
		{
			using (JobShipmentModuleForTest module = new JobShipmentModuleForTest())
			{
				AssertEquals(ChildEditableServiceStates.Shipment, ChildEditableService.GetState(module.GridCollection.Factory));

				module.PerformSearch();
				AssertEquals(ChildEditableServiceStates.Shipment, ChildEditableService.GetState(module.Factory));

				module.PerformSearch();
				AssertEquals(ChildEditableServiceStates.Shipment, ChildEditableService.GetState(module.Factory));
			}
		}

		public void TestModuleIDAndSupportsWorkflow()
		{
			using (JobShipmentModule module = new JobShipmentModule())
			{
				AssertEquals(ModuleIDs.JobShipment, module.ID);
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		public void TestBusinessContexts()
		{
			using (JobShipmentModule module = new JobShipmentModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("Shipment business context should be returned", BusinessContext.Shipment, module.BusinessContexts[0]);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
			}
		}

		public void TestImportUsesForwardingAdapter()
		{
			using (JobShipmentModuleForTest module = new JobShipmentModuleForTest())
			{
				module.OnImportFromXml_Click(module, EventArgs.Empty);
				AssertEquals(typeof(ForwardingShipmentValueObjectDataAdapter), module.LastExportTransferDataAdapter.GetType());
			}
		}

		public void TestQuickPODToolBarButtons()
		{
			using (JobShipmentModule module = new JobShipmentModule())
			{
				AssertNotNull("Should have found QuickPOD button", module.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems.FindByText("POD"));
			}
		}

		public void TestDeniedPartyScreeningMenuAdded()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var module = new JobShipmentModule())
			{
				AssertNotNull(module.FormActionMenu.FindByText("View Compliance Status", true));
			}
		}

		public void TestTransportBookingActionAdded()
		{
			using (var module = new JobShipmentModule())
			{
				AssertNotNull(module.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems.FindByText("Transport Booking"));
			}
		}
#if !WINZOR
		[SnailTest]
		public void TestHandlesLimitCheck()
		{
			//Opening a forwarding shipment form takes about 350 handles, 350*50 is well over the 10000 handle limit.
			ForwardingShipment[] selectedBusinessObjects = new ForwardingShipment[50];
			for (var i = 0; i < 50; ++i)
			{
				selectedBusinessObjects[i] = Factory.NewWithValidTestData<ForwardingShipment>();
			}
			Factory.Save();
			using (var module = new JobShipmentModule())
			{
				var result = module.ShowForms_Exposed(selectedBusinessObjects, false);
				AssertEquals("Warning shown.", true, UnitTestUserNotification.Instance.LastMessage.Contains("Too many records have been selected to show simultaneously."));
				AssertEquals("3 forms shown.", 3, result.Length);
				result[0].Dispose();
				result[1].Dispose();
				result[2].Dispose();
			}
		}
#endif
		public void TestFilterBusinessObjectCloneIncludesDeniedPartyFilters_CRTEnabled()
		{
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (var module = new JobShipmentModuleForTest())
			{
				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.FirstOrDefault(f => f.Description.StartsWith("Legacy Screening Status_"));
				AssertNotEquals(null, filter);

				var clone = module.FilterBusinessObject.Clone();
				var filter2 = (ModuleTextFilter)clone.ModuleFilters.FirstOrDefault(f => f.Description.StartsWith("Legacy Screening Status_"));
				AssertNotEquals(null, filter2);
			}
		}

		public void TestFilterBusinessObjectCloneIncludesDeniedPartyFilters_CRTDisabled()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var module = new JobShipmentModuleForTest())
			{
				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.FirstOrDefault(f => f.Description.StartsWith("Screening Status_"));
				AssertNotEquals(null, filter);

				var clone = module.FilterBusinessObject.Clone();
				var filter2 = (ModuleTextFilter)clone.ModuleFilters.FirstOrDefault(f => f.Description.StartsWith("Screening Status"));
				AssertNotEquals(null, filter2);
			}
		}

		public void TestComplianceFiltersWhenEnableComplianceRiskRegistryIsEnabled()
		{
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (var module = new JobShipmentModuleForTest())
			{
				var complianceFilters = module.FilterBusinessObject.ModuleFilters
					.Where(c => c.Category.Description == "Compliance")
					.Select(f => f.Description.Substring(0, f.Description.IndexOf("_"))).ToArray();

				AssertEquals("Compliance Category", 4, complianceFilters.Length);

				var expectedFilters = new string[] { "Overall", "Location", "Party", "Commodity" };
				AssertContainsExactElementsInAnyOrder(complianceFilters, expectedFilters);
			}
		}

		public void TestComplianceFiltersWhenEnableComplianceRiskRegistryIsDisabled()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var module = new JobShipmentModuleForTest())
			{
				var complianceFilters = module.FilterBusinessObject.ModuleFilters.Where(c => c.Category.Description == "Compliance").ToArray();
				AssertEquals("Compliance Category", 0, complianceFilters.Length);
			}
		}

		public void TestCO2eFiltersFunctionWorksWhenGreenhouseGasEmissionCalculationIsEnabled()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (var module = new JobShipmentModuleForTest())
			{
				var shipment1 = Factory.New<ForwardingShipment>();
				shipment1.SetTotalCO2e(111.114);
				shipment1.SetCO2eStatus(CO2eStatusList.Codes.Current);

				var shipment2 = Factory.New<ForwardingShipment>();
				shipment2.SetTotalCO2e(0.3m);
				shipment2.SetCO2eStatus(CO2eStatusList.Codes.Current);

				var shipment3 = Factory.New<ForwardingShipment>();
				shipment3.SetTotalCO2e(400);
				shipment3.SetCO2eStatus(CO2eStatusList.Codes.Current);

				var shipment4 = Factory.New<ForwardingShipment>();
				shipment4.SetTotalCO2e(500);
				shipment4.SetCO2eStatus(CO2eStatusList.Codes.Current);

				Factory.Save();

				var filter = (ModuleNumberRangeFilter)module.FilterBusinessObject.ModuleFilters.FirstOrDefault(f => f.Description == "CO2e");

				AssertWeightRangeResult(filter, 10, 1000, new[] { shipment1, shipment3, shipment4 });
				AssertWeightRangeResult(filter, 0, 10, new[] { shipment2 });
				AssertWeightRangeResult(filter, 600, 1000, Array.Empty<ForwardingShipment>());
				AssertWeightRangeResult(filter, 111.11, 400, new ForwardingShipment[] { shipment1, shipment3 });
				AssertWeightRangeResult(filter, 111.11, 111.12, new ForwardingShipment[] { shipment1 });
				AssertWeightRangeResult(filter, 111.114, 111.114, new ForwardingShipment[] { shipment1 });

				shipment1.SetCO2eStatus(CO2eStatusList.Codes.Pending);
				Factory.Save();
				AssertWeightRangeResult(filter, 10, 1000, new[] { shipment3, shipment4 });

				shipment1.SetCO2eStatus(CO2eStatusList.Codes.NotCalculated);
				Factory.Save();
				AssertWeightRangeResult(filter, 10, 1000, new[] { shipment3, shipment4 });

				shipment1.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
				Factory.Save();
				AssertWeightRangeResult(filter, 10, 1000, new[] { shipment3, shipment4 });

				shipment1.SetCO2eStatus(CO2eStatusList.Codes.Rejected);
				Factory.Save();
				AssertWeightRangeResult(filter, 10, 1000, new[] { shipment3, shipment4 });

				shipment1.SetCO2eStatus(CO2eStatusList.Codes.Current);
				Factory.Save();
				AssertWeightRangeResult(filter, 10, 1000, new[] { shipment1, shipment3, shipment4 });
			}

			void AssertWeightRangeResult(ModuleNumberRangeFilter filter, ZDecimal property1, ZDecimal property2, ForwardingShipment[] expectedShipments)
			{
				filter.IsActive = true;
				filter.Property1 = property1;
				filter.Property2 = property2;

				var collection = new ForwardingShipmentCollection(Factory);
				collection.Load(filter.Query);
				AssertContainsExactShipmentsInAnyOrder($"CO2e between {property1} and {property2}", expectedShipments, collection);
			}

			void AssertContainsExactShipmentsInAnyOrder(string message, IEnumerable<ForwardingShipment> expectedShipments, ForwardingShipmentCollection shipments)
			{
				AssertContainsExactElementsInAnyOrder(message, expectedShipments.Select(qb => qb.PK).ToArray(), shipments.Select(vqb => vqb.PK).ToArray());
			}
		}

		public void TestCO2eFiltersShouldOnlyExistWhenGreenhouseGasEmissionCalculationIsEnabled()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (var module = new JobShipmentModuleForTest())
			{
				var filter = (ModuleNumberRangeFilter)module.FilterBusinessObject.ModuleFilters.FirstOrDefault(f => f.Description == "CO2e");
				AssertNotNull(filter);
			}

			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			using (var module = new JobShipmentModuleForTest())
			{
				var filter = (ModuleNumberRangeFilter)module.FilterBusinessObject.ModuleFilters.FirstOrDefault(f => f.Description == "CO2e");
				AssertNull(filter);
			}
		}

		public void TestSetInspectionStatusMenuAdded()
		{
			using (var module = new JobShipmentModule())
			{
				var menuItem = module.FormActionMenu.FindByText("Set Inspection Status", true);
				AssertNotNull("A 'Set Inspection Status' menu item was expected to be present.", menuItem);
			}
		}

		#region Test records

		[RequiresSTA]
		public void TestLoadCollectionIncludeTemplateRecords()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_HouseBill = "XYZ";
			shipment1.JS_PackingMode = "123";
			shipment1.JS_RL_NKLoadPort = "ADALV";
			shipment1.JS_RL_NKDischargePort = "USORD";
			Factory.Save();

			using (var module = new JobShipmentModuleForTest())
			{
				var shipment2 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				shipment2.JS_HouseBill = "ABC";
				shipment2.JS_PackingMode = "234";
				shipment2.JS_RL_NKLoadPort = "AUSYD";
				shipment2.JS_RL_NKDischargePort = "AUBNE";
				shipment2.Factory.Save();
			}

			var templateReferenceId = (string)Db.Connection.ExecuteScalar("select top 1 STR_ReferenceId from dbo.stmtemplaterecord order by STR_SystemLastEditTimeUtc desc");

			using (var module = new JobShipmentModuleForTest())
			{
				var collection = new ForwardingShipmentCollection(new BusinessObjectFactory { RefreshEnabled = false });
				module.LoadCollectionExposed(collection, new ZQuery());

				AssertEquals(1, collection.Count);
				AssertEquals("XYZ", collection[0].JS_HouseBill);
			}

			using (var module = new JobShipmentModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;

				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobShipmentFilterBusinessObject.TemplateRecordsDescription);
				filter.IsActive = true;
				filter.Property = JobShipmentFilterBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;

				var collection = new ForwardingShipmentCollection(new BusinessObjectFactory { RefreshEnabled = false });
				module.LoadCollectionExposed(collection, new ZQuery());

				AssertEquals(1, collection.Count);
				AssertEquals("ABC", collection[0].JS_HouseBill);

				var filter2 = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobShipmentFilterBusinessObject.Descriptions.ShipmentNum);
				filter2.IsActive = true;
				filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				filter2.Property = templateReferenceId;

				module.LoadCollectionExposed(collection, new ZQuery());

				AssertEquals(1, collection.Count);
				AssertEquals("ABC", collection[0].JS_HouseBill);

				filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;

				module.LoadCollectionExposed(collection, new ZQuery());

				AssertEquals(0, collection.Count);

				filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				filter2.Property = templateReferenceId.Substring(1, 1);

				module.LoadCollectionExposed(collection, new ZQuery());

				AssertEquals(1, collection.Count);
				AssertEquals("ABC", collection[0].JS_HouseBill);

				filter2.Property = "";

				module.LoadCollectionExposed(collection, new ZQuery());

				AssertEquals(1, collection.Count);
				AssertEquals("ABC", collection[0].JS_HouseBill);

				filter.Property = JobShipmentFilterBusinessObject.TemplateRecordsFilterCodes.TemplatesExcluded;

				module.LoadCollectionExposed(collection, new ZQuery());

				AssertEquals(1, collection.Count);
				AssertEquals("XYZ", collection[0].JS_HouseBill);

				filter.IsActive = false;

				module.LoadCollectionExposed(collection, new ZQuery());

				AssertEquals(1, collection.Count);
				AssertEquals("XYZ", collection[0].JS_HouseBill);

				filter.IsActive = true;
				filter.Property = "asdfggf";

				module.LoadCollectionExposed(collection, new ZQuery());

				AssertEquals(1, collection.Count);
				AssertEquals("XYZ", collection[0].JS_HouseBill);

				filter.IsActive = true;
				filter.Property = JobShipmentFilterBusinessObject.TemplateRecordsFilterCodes.TemplatesIncluded;
				filter2.IsActive = false;
				var filter3 = (ModuleLocationFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobShipmentFilterBusinessObject.Descriptions.PlannedLoadDischarge);
				filter3.IsActive = true;
				filter3.Property1 = "ADALV";
				filter3.Property2 = "USORD";

				module.LoadCollectionExposed(collection, module.FilterBusinessObject.ModuleFilters.GetFilterQuery(new ModuleFilter[] { filter3 }));

				AssertEquals(1, collection.Count);
				AssertEquals("XYZ", collection[0].JS_HouseBill);

				filter3.Property1 = "AUSYD";
				filter3.Property2 = "AUBNE";

				module.LoadCollectionExposed(collection, module.FilterBusinessObject.ModuleFilters.GetFilterQuery(new ModuleFilter[] { filter3 }));

				AssertEquals(1, collection.Count);
				AssertEquals("ABC", collection[0].JS_HouseBill);

				filter3.IsActive = false;

				var filter4 = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == "Container Mode");
				filter4.IsActive = true;
				filter4.Property = "123";

				module.LoadCollectionExposed(collection, module.FilterBusinessObject.ModuleFilters.GetFilterQuery(new ModuleFilter[] { filter4 }));

				AssertEquals(1, collection.Count);
				AssertEquals("XYZ", collection[0].JS_HouseBill);

				filter4.Property = "234";

				module.LoadCollectionExposed(collection, module.FilterBusinessObject.ModuleFilters.GetFilterQuery(new ModuleFilter[] { filter4 }));

				AssertEquals(1, collection.Count);
				AssertEquals("ABC", collection[0].JS_HouseBill);
			}

			using (var module = new JobShipmentModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;

				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobShipmentFilterBusinessObject.TemplateRecordsDescription);
				filter.IsActive = true;
				filter.Property = JobShipmentFilterBusinessObject.TemplateRecordsFilterCodes.TemplatesIncluded;

				var collection = new ForwardingShipmentCollection(new BusinessObjectFactory { RefreshEnabled = false });
				module.LoadCollectionExposed(collection, new ZQuery());
				collection.Sort(JobShipmentSchema.Constants.JS_HouseBill);

				AssertEquals(2, collection.Count);
				AssertEquals("ABC", collection[0].JS_HouseBill);
				AssertEquals("XYZ", collection[1].JS_HouseBill);
			}
		}

		[RequiresSTA]
		public void TestLoadCollectionIncludeTemplateRecords_ShowActualSearchResult()
		{
			using (SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			using (var module = new JobShipmentModuleForTest())
			{
				var originalValue = EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult;

				try
				{
					EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult = true;
					module.AllowLoadTemplateRecords = true;

					var shipment1 = Factory.New<ForwardingShipment>();
					shipment1.JS_TransportMode = "AIR";
					shipment1.JS_HouseBill = "shipment1";
					Factory.Save();

					var shipment2 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					shipment2.JS_TransportMode = "SEA";
					shipment2.JS_HouseBill = "shipment2";
					shipment2.Factory.Save();

					var shipment3 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					shipment3.JS_TransportMode = "SEA";
					shipment3.JS_HouseBill = "shipment2";
					shipment3.Factory.Save();

					var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobShipmentFilterBusinessObject.TemplateRecordsDescription);
					filter.IsActive = true;
					filter.Property = JobShipmentFilterBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;
					((ZFilterStripBaseControl)module.EmbeddedControl).Find(true);

					var recordsLabel = module.EmbeddedControl.Controls.Find("ToolStripRecordsFoundLabel", true)[0] as ZLabel;
					AssertEquals("     Found\r\n  too many\r\n   records", recordsLabel.Text);
					AssertEquals("Too many records to display (2). Please fill in more of the search screen and then click 'Find'.", UnitTestUserNotification.Instance.LastMessage.Text);

					filter.Property = JobShipmentFilterBusinessObject.TemplateRecordsFilterCodes.TemplatesIncluded;
					((ZFilterStripBaseControl)module.EmbeddedControl).Find(true);
					AssertEquals("     Found\r\n  too many\r\n   records", recordsLabel.Text);
					AssertEquals("Too many records to display (3). Please fill in more of the search screen and then click 'Find'.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult = originalValue;
				}
			}
		}

		[RequiresSTA]
		public void TestLoadCollection_STR_ReferenceId()
		{
			using (var module = new JobShipmentModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;

				var shipment1 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				shipment1.Factory.Save();
				var referenceId1 = ((shipment1.Factory as TemplateRecordBusinessObjectFactory).TemplateRecordProvider.TemplateRecord as StmTemplateRecord).STR_ReferenceId;

				var shipment2 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				shipment2.Factory.Save();
				var referenceId2 = ((shipment2.Factory as TemplateRecordBusinessObjectFactory).TemplateRecordProvider.TemplateRecord as StmTemplateRecord).STR_ReferenceId;

				var shipment3 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				shipment3.Factory.Save();

				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobShipmentFilterBusinessObject.TemplateRecordsDescription);
				filter.IsActive = true;
				filter.Property = JobShipmentFilterBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;

				var filter2 = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobShipmentFilterBusinessObject.Descriptions.ShipmentNum);
				filter2.IsActive = true;
				filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				filter2.Property = $"{referenceId1},{referenceId2},S00000004";

				var collection = new ForwardingShipmentCollection(new BusinessObjectFactory { RefreshEnabled = false });
				module.LoadCollectionExposed(collection, new ZQuery());

				AssertEquals(2, collection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { referenceId1, referenceId2 }, collection.Cast<ForwardingShipment>().Select(shipment => shipment.JS_UniqueConsignRef));
			}
		}

		[RequiresSTA]
		public void TestLoadCollection_TemplateActive()
		{
			using (var module = new JobShipmentModuleForTest())
			{
				var shipmentTemplate_Active = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				shipmentTemplate_Active.JS_HouseBill = "ABC";
				shipmentTemplate_Active.Factory.Save();

				var shipmentTemplate_Inactive = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				shipmentTemplate_Inactive.JS_HouseBill = "DEF";
				shipmentTemplate_Inactive.TemplateRecord.IsCancelled = true;
				shipmentTemplate_Inactive.Factory.Save();
			}
			var allLanguages = LanguageHelper.GetDefaultLanguageForOLookUpEditType().Keys.ToList();
			allLanguages.ForEach(lan =>
			{
				using (Res.TemporarilySwitchLanguage(lan))
				{
					AssertTemplateRecordsFilter(
					FilterStripBusinessObject.TemplateRecordsActive,
					SQLComparisonOperator.Equal,
					FilterStripBusinessObject.StatusActive,
					new string[] { "ABC" },
					forceShowAllTemplates: true
				);

					AssertTemplateRecordsFilter(
						FilterStripBusinessObject.TemplateRecordsActive,
						SQLComparisonOperator.Equal,
						FilterStripBusinessObject.StatusInactive,
						new string[] { "DEF" },
						forceShowAllTemplates: true
					);

					AssertTemplateRecordsFilter(
						FilterStripBusinessObject.TemplateRecordsActive,
						SQLComparisonOperator.Equal,
						FilterStripBusinessObject.StatusAll,
						new string[] { "ABC", "DEF" },
						forceShowAllTemplates: true
					);
				}
			});
		}

		[RequiresSTA]
		public void TestLoadCollection_TemplateName()
		{
			using (var module = new JobShipmentModuleForTest())
			{
				var shipment1 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				shipment1.JS_HouseBill = "ABC";
				shipment1.TemplateRecord.STR_TemplateName = "My Template";
				shipment1.Factory.Save();

				var shipment2 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				shipment2.JS_HouseBill = "DEF";
				shipment2.TemplateRecord.STR_TemplateName = "Other Template";
				shipment2.Factory.Save();

				var shipment3 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				shipment3.JS_HouseBill = "GHI";
				shipment3.Factory.Save();
			}

			AssertTemplateRecordsFilter(
				FilterStripBusinessObject.TemplateRecordsTemplateName,
				SQLComparisonOperator.StartsWith,
				"My Template",
				new string[] { "ABC" },
				forceShowAllTemplates: true
			);

			AssertTemplateRecordsFilter(
				FilterStripBusinessObject.TemplateRecordsTemplateName,
				SQLComparisonOperator.StartsWith,
				"Other Template",
				new string[] { "DEF" },
				forceShowAllTemplates: true
			);

			AssertTemplateRecordsFilter(
				FilterStripBusinessObject.TemplateRecordsTemplateName,
				SQLComparisonOperator.StartsWith,
				ZString.Empty,
				new string[] { "ABC", "DEF", "GHI" },
				forceShowAllTemplates: true
			);

			AssertTemplateRecordsFilter(
				FilterStripBusinessObject.TemplateRecordsTemplateName,
				SQLComparisonOperator.IsBlank,
				ZString.Empty,
				new string[] { "GHI" },
				forceShowAllTemplates: true
			);

			AssertTemplateRecordsFilter(
				FilterStripBusinessObject.TemplateRecordsTemplateName,
				SQLComparisonOperator.IsNotBlank,
				ZString.Empty,
				new string[] { "ABC", "DEF" },
				forceShowAllTemplates: true
			);
		}

		public void TestGetNewTemplateRecordBusinessObjectCore()
		{
			using (var module = new JobShipmentModuleForTest())
			{
				var shipment = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				var templateRecordProvider = (ITemplateRecordProvider)shipment;

				Assert(templateRecordProvider.IsTemplateRecord);
				AssertNotNull(templateRecordProvider.TemplateRecord);
				AssertEquals(typeof(StmTemplateRecord), templateRecordProvider.TemplateRecord.GetType());
				var templateRecord = (StmTemplateRecord)templateRecordProvider.TemplateRecord;
				AssertNotEquals(shipment.Factory._Instance, templateRecord.Factory._Instance);
				AssertEquals(module.ID.Name, templateRecord.STR_ModuleID);

				AssertEquals(typeof(TemplateRecordBusinessObjectFactory), shipment.Factory.GetType());
				var templateFactory = (TemplateRecordBusinessObjectFactory)shipment.Factory;
				AssertSame(shipment, templateFactory.TemplateRecordProvider);
				AssertSame(templateRecord.Factory, templateFactory.TemplateRecordFactory);
			}
		}

		public void TestLoadFromTemplateRecordPkCore()
		{
			using (var module = new JobShipmentModuleForTest())
			{
				var shipment = module.GetNewTemplateRecordBusinessObjectCoreExposed();
				shipment.JS_HouseBill = "ABC";
				shipment.Factory.Save();

				var templateRecordProvider = (ITemplateRecordProvider)shipment;
				var templateRecord = (StmTemplateRecord)templateRecordProvider.TemplateRecord;

				var localFactory = new BusinessObjectFactory();

				var reloadedShipment = (ForwardingShipment)module.LoadFromTemplateRecordPk(localFactory, shipment.PK);
				AssertNull("Cannot reload template record by shipment's PK", reloadedShipment);

				reloadedShipment = (ForwardingShipment)module.LoadFromTemplateRecordPk(localFactory, templateRecord.PK);
				AssertNotNull("Sholud load template shipment by template record's PK", reloadedShipment);
				AssertEquals("ABC", reloadedShipment.JS_HouseBill);
				AssertNotEquals(shipment.PK, reloadedShipment.PK);
				AssertSame("Should load in specifid factory", localFactory, reloadedShipment.Factory);

				var reloadedTemplateRecordProvider = (ITemplateRecordProvider)reloadedShipment;
				Assert(reloadedTemplateRecordProvider.IsTemplateRecord);

				var reloadedTemplateRecord = (StmTemplateRecord)reloadedTemplateRecordProvider.TemplateRecord;
				AssertEquals(templateRecord.PK, reloadedTemplateRecord.PK);
				AssertSame("Should load in specifid factory", localFactory, reloadedTemplateRecord.Factory);
			}
		}

		#region Test Load Template Records By Filters

		[RequiresSTA]
		public void TestLoadTemplateRecordsByFilters_CreatingUser()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			var user2 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				using (var module = new JobShipmentModuleForTest())
				{
					var shipment = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					shipment.JS_HouseBill = "SHIPMENT1";
					shipment.Factory.Save();
				}
			}

			using (Env.SetTemporaryUserContext(user2.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				using (var module = new JobShipmentModuleForTest())
				{
					var shipment2 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					shipment2.JS_HouseBill = "SHIPMENT2";
					shipment2.Factory.Save();
				}
			}

			using (var module = new JobShipmentModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;
				var query = new ZQuery();
				var collection = new ForwardingShipmentCollection(new BusinessObjectFactory { RefreshEnabled = false });

				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobShipmentFilterBusinessObject.TemplateRecordsDescription);
				filter.IsActive = true;
				filter.Property = JobShipmentFilterBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;

				var filter2 = (ModuleNkFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == FilterDescriptions.CreatingUser);
				filter2.IsActive = true;
				filter2.Property = user.GS_Code;

				module.LoadCollectionExposed(collection, query);
				AssertEquals(1, collection.Count);
				AssertEquals("SHIPMENT1", collection[0].JS_HouseBill);

				collection.RemoveAll();
				filter2.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				module.LoadCollectionExposed(collection, query);
				AssertEquals(1, collection.Count);
				AssertEquals("SHIPMENT2", collection[0].JS_HouseBill);

				collection.RemoveAll();
				filter2.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
				module.LoadCollectionExposed(collection, query);
				AssertEquals(0, collection.Count);

				collection.RemoveAll();
				filter2.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
				module.LoadCollectionExposed(collection, query);
				AssertEquals(2, collection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "SHIPMENT1", "SHIPMENT2" }, collection.Cast<ForwardingShipment>().Select(shipment => shipment.JS_HouseBill));
			}
		}

		[RequiresSTA]
		public void TestLoadTemplateRecordsByFilters_TransportMode_Version_2012_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			{
				TestLoadTemplateRecordsByFilters_TransportMode();
			}
		}

		[RequiresSTA]
		public void TestLoadTemplateRecordsByFilters_TransportMode()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				using (var module = new JobShipmentModuleForTest())
				{
					var shipment = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					shipment.JS_HouseBill = "SHIPMENT1";
					shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
					shipment.Factory.Save();

					var shipment2 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					shipment2.JS_HouseBill = "SHIPMENT2";
					shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
					shipment2.Factory.Save();
				}
			}

			using (var module = new JobShipmentModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;
				var query = new ZQuery();
				query.AddToFilter(JobShipmentSchema.JS_SystemCreateUser, user.GS_Code);
				var collection = new ForwardingShipmentCollection(new BusinessObjectFactory { RefreshEnabled = false });

				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobShipmentFilterBusinessObject.TemplateRecordsDescription);
				filter.IsActive = true;
				filter.Property = JobShipmentFilterBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;

				var filter2 = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == "Transport Mode");
				filter2.IsActive = true;
				filter2.Property = Core.Constants.TransportModes.Sea;

				module.LoadCollectionExposed(collection, query);
				AssertEquals(1, collection.Count);
				AssertEquals("SHIPMENT1", collection[0].JS_HouseBill);

				collection.RemoveAll();
				filter2.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				module.LoadCollectionExposed(collection, query);
				AssertEquals(1, collection.Count);
				AssertEquals("SHIPMENT1", collection[0].JS_HouseBill);

				collection.RemoveAll();
				filter2.SqlComparisonOperator = SQLComparisonOperator.Contains;
				module.LoadCollectionExposed(collection, query);
				AssertEquals(1, collection.Count);
				AssertEquals("SHIPMENT1", collection[0].JS_HouseBill);

				collection.RemoveAll();
				filter2.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				module.LoadCollectionExposed(collection, query);
				AssertEquals(1, collection.Count);
				AssertEquals("SHIPMENT2", collection[0].JS_HouseBill);

				collection.RemoveAll();
				filter2.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
				module.LoadCollectionExposed(collection, query);
				AssertEquals(1, collection.Count);
				AssertEquals("SHIPMENT2", collection[0].JS_HouseBill);

				collection.RemoveAll();
				filter2.SqlComparisonOperator = SQLComparisonOperator.NotContains;
				module.LoadCollectionExposed(collection, query);
				AssertEquals(1, collection.Count);
				AssertEquals("SHIPMENT2", collection[0].JS_HouseBill);

				collection.RemoveAll();
				filter2.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
				module.LoadCollectionExposed(collection, query);
				AssertEquals(0, collection.Count);

				collection.RemoveAll();
				filter2.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
				module.LoadCollectionExposed(collection, query);
				AssertEquals(2, collection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "SHIPMENT1", "SHIPMENT2" }, collection.Cast<ForwardingShipment>().Select(shipment => shipment.JS_HouseBill));
			}
		}

		[RequiresSTA]
		public void TestLoadTemplateRecordsByFilters_ShipmentType_Version_2012_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			{
				TestLoadTemplateRecordsByFilters_ShipmentType();
			}
		}

		[RequiresSTA]
		public void TestLoadTemplateRecordsByFilters_ShipmentType()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				using (var module = new JobShipmentModuleForTest())
				{
					var shipment = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					shipment.JS_HouseBill = "SHIPMENT1";
					shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;
					shipment.Factory.Save();

					var shipment2 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					shipment2.JS_HouseBill = "SHIPMENT2";
					shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
					shipment2.Factory.Save();
				}
			}

			using (var module = new JobShipmentModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;
				var query = new ZQuery();
				query.AddToFilter(JobShipmentSchema.JS_SystemCreateUser, user.GS_Code);
				var collection = new ForwardingShipmentCollection(new BusinessObjectFactory { RefreshEnabled = false });

				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobShipmentFilterBusinessObject.TemplateRecordsDescription);
				filter.IsActive = true;
				filter.Property = JobShipmentFilterBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;

				var filter2 = (ModuleFlagsFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == "Shipment Type");
				filter2.IsActive = true;
				filter2.Property0 = true; // Assembly Master

				module.LoadCollectionExposed(collection, query);
				AssertEquals(1, collection.Count);
				AssertEquals("SHIPMENT1", collection[0].JS_HouseBill);

				collection.RemoveAll();
				filter2.Property2 = true; // Co-Load Master
				module.LoadCollectionExposed(collection, query);
				AssertEquals(2, collection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "SHIPMENT1", "SHIPMENT2" }, collection.Cast<ForwardingShipment>().Select(shipment => shipment.JS_HouseBill));

				collection.RemoveAll();
				filter2.Property0 = false;
				module.LoadCollectionExposed(collection, query);
				AssertEquals(1, collection.Count);
				AssertEquals("SHIPMENT2", collection[0].JS_HouseBill);
			}
		}

		[RequiresSTA]
		public void TestLoadTemplateRecordsByFilters_OriginDestination_Version_2012_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			{
				TestLoadTemplateRecordsByFilters_OriginDestination();
			}
		}

		[RequiresSTA]
		public void TestLoadTemplateRecordsByFilters_OriginDestination()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				using (var module = new JobShipmentModuleForTest())
				{
					var shipment = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					shipment.JS_HouseBill = "SHIPMENT1";
					shipment.JS_RL_NKOrigin = "AUSYD";
					shipment.JS_RL_NKDestination = "SGSIN";
					shipment.Factory.Save();

					var shipment2 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					shipment2.JS_HouseBill = "SHIPMENT2";
					shipment2.JS_RL_NKOrigin = "AUSYD";
					shipment2.JS_RL_NKDestination = "USCHI";
					shipment2.Factory.Save();

					var shipment3 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					shipment3.JS_HouseBill = "SHIPMENT3";
					shipment3.JS_RL_NKOrigin = "NZAKL";
					shipment3.JS_RL_NKDestination = "USCHI";
					shipment3.Factory.Save();
				}
			}

			using (var module = new JobShipmentModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;
				var query = new ZQuery();
				query.AddToFilter(JobShipmentSchema.JS_SystemCreateUser, user.GS_Code);
				var collection = new ForwardingShipmentCollection(new BusinessObjectFactory { RefreshEnabled = false });

				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobShipmentFilterBusinessObject.TemplateRecordsDescription);
				filter.IsActive = true;
				filter.Property = JobShipmentFilterBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;

				var filter2 = (ModuleLocationFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobShipmentFilterBusinessObject.Descriptions.OriginDestination);
				filter2.IsActive = true;
				filter2.Property1 = "AUSYD";

				module.LoadCollectionExposed(collection, query);
				AssertEquals(2, collection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "SHIPMENT1", "SHIPMENT2" }, collection.Cast<ForwardingShipment>().Select(shipment => shipment.JS_HouseBill));

				collection.RemoveAll();
				filter2.Property2 = "USCHI";
				module.LoadCollectionExposed(collection, query);
				AssertEquals(1, collection.Count);
				AssertEquals("SHIPMENT2", collection[0].JS_HouseBill);

				collection.RemoveAll();
				filter2.Property1 = "";
				module.LoadCollectionExposed(collection, query);
				AssertEquals(2, collection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "SHIPMENT2", "SHIPMENT3" }, collection.Cast<ForwardingShipment>().Select(shipment => shipment.JS_HouseBill));

				collection.RemoveAll();
				filter2.Property1 = "AU";
				filter2.Property2 = "";
				module.LoadCollectionExposed(collection, query);
				AssertEquals(2, collection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "SHIPMENT1", "SHIPMENT2" }, collection.Cast<ForwardingShipment>().Select(shipment => shipment.JS_HouseBill));

				collection.RemoveAll();
				filter2.Property1 = "";
				filter2.Property2 = "US";
				module.LoadCollectionExposed(collection, query);
				AssertEquals(2, collection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "SHIPMENT2", "SHIPMENT3" }, collection.Cast<ForwardingShipment>().Select(shipment => shipment.JS_HouseBill));
			}
		}

		[RequiresSTA]
		public void TestLoadTemplateRecordsByFilters_Organisation_Version_2012_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlSchema.Version_2012_11_DO_NOT_USE))
			{
				TestLoadTemplateRecordsByFilters_Organisation();
			}
		}

		[RequiresSTA]
		public void TestLoadTemplateRecordsByFilters_Organisation()
		{
			AssertLoadTemplateRecordsByFilters_Organisation(JobShipmentFilterBusinessObject.Descriptions.Consignor,
				(shipment, organisation) => shipment.ConsignorPK = organisation.PK);

			AssertLoadTemplateRecordsByFilters_Organisation(JobShipmentFilterBusinessObject.Descriptions.Consignee,
				(shipment, organisation) => shipment.ConsigneePK = organisation.PK);

			AssertLoadTemplateRecordsByFilters_Organisation(JobShipmentFilterBusinessObject.Descriptions.PickupAgent,
				(shipment, organisation) => shipment.PickupAgentPK = organisation.PK);

			AssertLoadTemplateRecordsByFilters_Organisation(JobShipmentFilterBusinessObject.Descriptions.DeliveryAgent,
				(shipment, organisation) => shipment.JS_OH_DeliveryAgent = organisation.PK);

			AssertLoadTemplateRecordsByFilters_Organisation("Controlling Agent",
				(shipment, organisation) => shipment.ControllingAgentDocumentaryAddress.OrganisationPK = organisation.PK);
		}

		void AssertLoadTemplateRecordsByFilters_Organisation(string description, Action<ForwardingShipment, OrgHeader> setOrganisationPK)
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				using (var module = new JobShipmentModuleForTest())
				{
					var shipment = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					shipment.JS_HouseBill = "SHIPMENT1";
					setOrganisationPK(shipment, org1);
					shipment.Factory.Save();

					var shipment2 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					shipment2.JS_HouseBill = "SHIPMENT2";
					setOrganisationPK(shipment2, org2);
					shipment2.Factory.Save();
				}
			}

			using (var module = new JobShipmentModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;
				var query = new ZQuery();
				query.AddToFilter(JobShipmentSchema.JS_SystemCreateUser, user.GS_Code);
				var collection = new ForwardingShipmentCollection(new BusinessObjectFactory { RefreshEnabled = false });

				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobShipmentFilterBusinessObject.TemplateRecordsDescription);
				filter.IsActive = true;
				filter.Property = JobShipmentFilterBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;

				var filter2 = (ModuleGuidFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == description);
				filter2.IsActive = true;
				filter2.Property = org1.PK;
				filter2.PropertyCode = org1.OH_Code;

				module.LoadCollectionExposed(collection, query);
				AssertEquals(1, collection.Count);
				AssertEquals("SHIPMENT1", collection[0].JS_HouseBill);

				collection.RemoveAll();
				filter2.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				module.LoadCollectionExposed(collection, query);
				AssertEquals(1, collection.Count);
				AssertEquals("SHIPMENT2", collection[0].JS_HouseBill);
			}
		}

		[RequiresSTA]
		public void TestLoadTemplateRecordsByFilters_ConsignorConsignee()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				using (var module = new JobShipmentModuleForTest())
				{
					var shipment1 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					shipment1.JS_HouseBill = "SHIPMENT1";
					shipment1.ConsignorPK = org1.PK;
					shipment1.ConsigneePK = org2.PK;
					shipment1.Factory.Save();

					var shipment2 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					shipment2.JS_HouseBill = "SHIPMENT2";
					shipment2.ConsignorPK = org2.PK;
					shipment2.ConsigneePK = org3.PK;
					shipment2.Factory.Save();

					var shipment3 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					shipment3.JS_HouseBill = "SHIPMENT3";
					shipment3.ConsignorPK = org1.PK;
					shipment3.ConsigneePK = org3.PK;
					shipment3.Factory.Save();
				}
			}

			using (var module = new JobShipmentModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;
				var query = new ZQuery();
				query.AddToFilter(JobShipmentSchema.JS_SystemCreateUser, user.GS_Code);
				var collection = new ForwardingShipmentCollection(new BusinessObjectFactory { RefreshEnabled = false });

				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobShipmentFilterBusinessObject.TemplateRecordsDescription);
				filter.IsActive = true;
				filter.Property = JobShipmentFilterBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;

				var filter2 = (ModuleGuidsFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobShipmentFilterBusinessObject.Descriptions.ConsignorConsignee);
				filter2.IsActive = true;
				filter2.Property1 = org1.PK;
				filter2.PropertyCode1 = org1.OH_Code;
				filter2.Property2 = org2.PK;
				filter2.PropertyCode2 = org2.OH_Code;

				module.LoadCollectionExposed(collection, query);
				AssertEquals("Shipments are filtered by both Consignor and Consignee", 1, collection.Count);
				AssertEquals("SHIPMENT1", collection[0].JS_HouseBill);

				collection.RemoveAll();
				filter2.Property2 = ZGuid.Empty;
				filter2.PropertyCode2 = "";
				module.LoadCollectionExposed(collection, query);
				AssertEquals("Shipments are filtered by Consignor when Consignee is empty", 2, collection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "SHIPMENT1", "SHIPMENT3" }, collection.Cast<ForwardingShipment>().Select(shipment => shipment.JS_HouseBill));

				collection.RemoveAll();
				filter2.Property1 = ZGuid.Empty;
				filter2.PropertyCode1 = "";
				filter2.Property2 = org3.PK;
				filter2.PropertyCode2 = org3.OH_Code;
				module.LoadCollectionExposed(collection, query);
				AssertEquals("Shipments are filtered by Consignee when Consignor is empty", 2, collection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "SHIPMENT2", "SHIPMENT3" }, collection.Cast<ForwardingShipment>().Select(shipment => shipment.JS_HouseBill));

				collection.RemoveAll();
				filter2.Property2 = ZGuid.Empty;
				filter2.PropertyCode2 = "";
				module.LoadCollectionExposed(collection, query);
				AssertEquals("Shipments are not filtered when both Consignor and Consignee are empty", 3, collection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "SHIPMENT1", "SHIPMENT2", "SHIPMENT3" }, collection.Cast<ForwardingShipment>().Select(shipment => shipment.JS_HouseBill));
			}
		}

		[RequiresSTA]
		public void TestLoadTemplateRecordsByFilters_MultipleFilters()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				using (var module = new JobShipmentModuleForTest())
				{
					var shipment = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					shipment.JS_HouseBill = "SHIPMENT1";
					shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
					shipment.Factory.Save();

					var shipment2 = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					shipment2.JS_HouseBill = "SHIPMENT2";
					shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
					shipment2.Factory.Save();
				}
			}

			using (var module = new JobShipmentModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;
				var query = new ZQuery();
				query.AddToFilter(JobShipmentSchema.JS_SystemCreateUser, user.GS_Code);
				var collection = new ForwardingShipmentCollection(new BusinessObjectFactory { RefreshEnabled = false });

				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobShipmentFilterBusinessObject.TemplateRecordsDescription);
				filter.IsActive = true;
				filter.Property = JobShipmentFilterBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;

				var filter2 = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == "Transport Mode");
				filter2.IsActive = true;
				filter2.Property = Core.Constants.TransportModes.Sea;
				filter2.OrCategory = FilterOrCategory.Blue;

				var filter3 = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.GetVisibleModuleFilterAndDuplicateAndDeactivateIfActive("Transport Mode");
				filter3.IsActive = true;
				filter3.Property = Core.Constants.TransportModes.Air;
				filter3.OrCategory = FilterOrCategory.Blue;

				module.LoadCollectionExposed(collection, query);
				AssertEquals(2, collection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "SHIPMENT1", "SHIPMENT2" }, collection.Cast<ForwardingShipment>().Select(shipment => shipment.JS_HouseBill));
			}
		}

		[RequiresSTA]
		public void TestLoadTemplateRecordsWithoutValidation()
		{
			using (var module = new JobShipmentModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;

				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobShipmentFilterBusinessObject.TemplateRecordsDescription);
				filter.IsActive = true;
				filter.Property = JobShipmentFilterBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;

				var collection = new ForwardingShipmentCollection(module.Factory);
				module.LoadCollectionExposed(collection, new ZQuery());

				Assert(collection.Factory.ValidationHasBeenSuspended);
			}
		}

		[RequiresSTA]
		public void TestLoadTemplateRecords_ShipmentNotesWereNotLoaded()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				using (var module = new JobShipmentModuleForTest())
				{
					var shipment = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					shipment.JS_HouseBill = "SHIPMENT1";
					shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
					shipment.Notes.AddNew();
					shipment.Factory.Save();
				}
			}

			using (var module = new JobShipmentModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;

				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobShipmentFilterBusinessObject.TemplateRecordsDescription);
				filter.IsActive = true;
				filter.Property = JobShipmentFilterBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;

				var collection = new ForwardingShipmentCollection(module.Factory);
				module.LoadCollectionExposed(collection, new ZQuery());

				AssertEquals(0, collection[0].Notes.GetAllNotes().Count);
			}
		}

		[RequiresSTA]
		public void TestLoadTemplateRecords_ShipmentAWBWasNotLoaded()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				using (var module = new JobShipmentModuleForTest())
				{
					var shipment = module.GetNewTemplateRecordBusinessObjectCoreExposed();
					shipment.JS_HouseBill = "SHIPMENT1";
					shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

					var awbHeader = shipment.Factory.New<ShipmentExportAWBHeader>();
					awbHeader.EH_ParentID = shipment.PK;
					awbHeader.EH_AWBType = "HWB";
					awbHeader.Populate();

					JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipment);
					shipment.DocsAndCartage.JP_PrintOptionForPackagesOnAWB = "ALL";
					shipment.Factory.Save();
				}
			}

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var module = new JobShipmentModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;

				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters.First(f => f.Description == JobShipmentFilterBusinessObject.TemplateRecordsDescription);
				filter.IsActive = true;
				filter.Property = JobShipmentFilterBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;

				var collection = new ForwardingShipmentCollection(module.Factory);
				module.LoadCollectionExposed(collection, new ZQuery());

				AssertEquals(0, collection[0].AWBHeaderManager.Count);
			}
		}

		#endregion

		public void TestInvalidColumnExceptionIsReported()
		{
			using (var module = new JobShipmentModuleForTest())
			{
				var query = new ZQuery(JobShipmentSchema.JS_E_DEP, SQLComparisonOperator.GreaterThan, ZDate.Today);
				query.AddToFilter(JobConShipLinkSchema.JN_JK, ZGuid.Empty);

				var collection = new ForwardingShipmentCollection(new BusinessObjectFactory { RefreshEnabled = false });

				ErrorReporter.Clear();

				try
				{
					module.LoadCollectionExposed(collection, query);
					Assert("LoadCollectionExposed should throw exception", false);
				}
				catch (Exception ex)
				{
					AssertEquals("SqlException", ex.GetType().Name);
					AssertEquals("Invalid column name 'JN_JK'.", ex.Message); // We know that exceptions thrown to the top level exception handler will be reported
				}
			}
		}

		#endregion

		#region TestDeactivateAndShowShippingInstructionRejectionMessage

		[RequiresSTA]
		public void TestDeactivateAndShowShippingInstructionRejectionMessage()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.GenerateStatusEvent("ESI", "Electronic Shipping Instruction Received");
			Factory.Save();

			using (var form = new ZForm())
			using (var module = new JobShipmentModuleForTest())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				module.PerformSearch();
				AssertEquals(1, module.GridCollection.Count);

				module.DisplayGrid.Select(0);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var deactivateButton = module.ToolBarButtons.FirstOrDefault(t => t.Text == "Delete") as ZToolBarButton;
				AssertNotNull(deactivateButton);
				deactivateButton.PerformClick();

				var message = UnitTestUserNotification.Instance.LastMessage;
				AssertNotNull(message);
				AssertEquals("Shipment Rejection", message.Caption);
				AssertEquals("This Shipment was created electronically, would you like to send a Shipping Instruction Rejection message to the booking party?", message.Text);

				var openedShipmentForms = ZApplication.GetOpenForms().OfType<ShipmentForm>();
				foreach (var openedShipmentForm in openedShipmentForms)
				{
					openedShipmentForm.Dispose();
				}
			}
		}

		#endregion

		#region TestLoadCollection_ElectronicBillOfLading

		[RequiresSTA]
		public void TestElectronicBillOfLadingFiltersExist()
		{
			AssertElectronicBillOfLadingFilterExists(false, false);

			AssertElectronicBillOfLadingFilterExists(true, true);

			void AssertElectronicBillOfLadingFilterExists(bool enableEBLIntegration, bool filtersExist)
			{
				var boleroEBLConfiguration = new BoleroEBLConfiguration()
				{
					EnableEBLIntegration = enableEBLIntegration,
					GalileoEndPointUrl = "http://test.test",
					GalileoAudience = Guid.NewGuid().ToString(),
					GalileoTestEndPointUrl = "http://test.test",
					GalileoTestAudience = Guid.NewGuid().ToString()
				};

				using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
				using (var form = new ZForm())
				using (var module = new JobShipmentModuleForTest())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var hblStatusFilterExists = module
						.FilterBusinessObject
						.ModuleFilters
						.Any(f => f.Description == JobShipmentFilterBusinessObject.Descriptions.HBLStatus);
					var hblTypeFilterExists = module
						.FilterBusinessObject
						.ModuleFilters
						.Any(f => f.Description == JobShipmentFilterBusinessObject.Descriptions.HBLType);
					var hblTermsFilterExists = module
						.FilterBusinessObject
						.ModuleFilters
						.Any(f => f.Description == JobShipmentFilterBusinessObject.Descriptions.HBLTerms);

					AssertEquals(filtersExist, hblStatusFilterExists);
					AssertEquals(filtersExist, hblTypeFilterExists);
					AssertEquals(filtersExist, hblTermsFilterExists);
				}
			}
		}

		[RequiresSTA]
		public void TestElectronicBillOfLadingFiltersQuery()
		{
			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			using (var form = new ZForm())
			using (var module = new JobShipmentModuleForTest())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var hblStatusFilter = module
					.FilterBusinessObject
					.ModuleFilters
					.First(f => f.Description == JobShipmentFilterBusinessObject.Descriptions.HBLStatus) as ModuleTextFilter;
				var hblTypeFilter = module
					.FilterBusinessObject
					.ModuleFilters
					.First(f => f.Description == JobShipmentFilterBusinessObject.Descriptions.HBLType) as ModuleTextFilter;
				var hblTermsFilter = module
					.FilterBusinessObject
					.ModuleFilters
					.First(f => f.Description == JobShipmentFilterBusinessObject.Descriptions.HBLTerms) as ModuleTextFilter;

				var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment1.JS_UniqueConsignRef = "S00000001";
				shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment1.JS_ElectronicBillOfLadingStatus = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillReceived;
				shipment1.JS_ElectronicBillOfLadingType = ZString.Empty;
				shipment1.JS_ElectronicBillOfLadingTerms = ZString.Empty;

				var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment2.JS_UniqueConsignRef = "S00000002";
				shipment2.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment2.JS_ElectronicBillOfLadingStatus = ZString.Empty;
				shipment2.JS_ElectronicBillOfLadingType = Enterprise.Core.Constants.BillOfLadingBillType.Codes.BlankEndorse;
				shipment2.JS_ElectronicBillOfLadingTerms = ZString.Empty;

				var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment3.JS_UniqueConsignRef = "S00000003";
				shipment3.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment3.JS_ElectronicBillOfLadingStatus = ZString.Empty;
				shipment3.JS_ElectronicBillOfLadingType = ZString.Empty;
				shipment3.JS_ElectronicBillOfLadingTerms = Enterprise.Core.Constants.BillOfLadingBillTerms.Codes.NonTransferable;
				Factory.Save();

				hblStatusFilter.IsActive = true;
				hblTypeFilter.IsActive = false;
				hblTermsFilter.IsActive = false;
				hblStatusFilter.Property = FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillReceived;
				module.PerformSearch();

				AssertEquals(1, module.GridCollection.Count);
				AssertEquals("S00000001", ((ForwardingShipment)module.GridCollection[0]).JS_UniqueConsignRef);

				hblStatusFilter.IsActive = false;
				hblTypeFilter.IsActive = true;
				hblTermsFilter.IsActive = false;
				hblTypeFilter.Property = Enterprise.Core.Constants.BillOfLadingBillType.Codes.BlankEndorse;
				module.PerformSearch();

				AssertEquals(1, module.GridCollection.Count);
				AssertEquals("S00000002", ((ForwardingShipment)module.GridCollection[0]).JS_UniqueConsignRef);

				hblStatusFilter.IsActive = false;
				hblTypeFilter.IsActive = false;
				hblTermsFilter.IsActive = true;
				hblTermsFilter.Property = Enterprise.Core.Constants.BillOfLadingBillTerms.Codes.NonTransferable;
				module.PerformSearch();

				AssertEquals(1, module.GridCollection.Count);
				AssertEquals("S00000003", ((ForwardingShipment)module.GridCollection[0]).JS_UniqueConsignRef);
			}
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.JobShipment;
		}

		void AssertTemplateRecordsFilter(
			string filterType,
			SQLComparisonOperator filterOperation,
			string filterValue,
			string[] expectedHouseBills,
			bool forceShowAllTemplates = false)
		{
			using (var module = new JobShipmentModuleForTest())
			{
				module.AllowLoadTemplateRecords = true;

				if (forceShowAllTemplates)
				{
					var showTemplatesFilter = module
						.FilterBusinessObject
						.ModuleFilters
						.First(f => f.Description == FilterStripBusinessObject.TemplateRecordsDescription) as ModuleTextFilter;

					showTemplatesFilter.IsActive = true;
					showTemplatesFilter.Property = FilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly;
				}

				var filter = module
					.FilterBusinessObject
					.ModuleFilters
					.First(f => f.Description == filterType) as ModuleTextFilter;

				filter.IsActive = true;
				filter.SqlComparisonOperator = filterOperation;
				filter.Property = filterValue;

				var collection = new ForwardingModuleShipmentCollection(new BusinessObjectFactory { RefreshEnabled = false });
				module.LoadCollectionExposed(collection, new ZQuery());

				CombineAssertions($"Should load expected shipments (Filter: {filterType}, Operation: {filterOperation.ToString()} Value: {filterValue})", () =>
				{
					AssertEquals(expectedHouseBills.Length, collection.Count);
					AssertContainsExactElementsInAnyOrder(expectedHouseBills, collection.Cast<ForwardingShipment>().ToArray().Select(s => s.JS_HouseBill));
				});
			}
		}

		#region class JobShipmentModuleForTest

		internal class JobShipmentModuleForTest : JobShipmentModule
		{
			public new void OnImportFromXml_Click(object sender, EventArgs e)
			{
				base.OnImportFromXml_Click(sender, e);
			}

			protected override XmlDataTransferDirector GetNewXmlDataTransferDirector(IValueObjectDataAdapter adapter, bool checkForLicence)
			{
				LastExportTransferDataAdapter = adapter;
				return new XmlDataTransferDirector(adapter, false);
			}

			public IValueObjectDataAdapter LastExportTransferDataAdapter;

			public new BusinessObjectFactory Factory
			{
				get { return base.Factory; }
			}

			public void PerformSearch()
			{
				base.PerformSearch();
			}

			internal ForwardingShipment GetNewTemplateRecordBusinessObjectCoreExposed()
			{
				return (ForwardingShipment)GetNewTemplateRecordBusinessObjectCore();
			}

			internal void LoadCollectionExposed(IBusinessObjectCollection collection, ZQuery query)
			{
				var result = PerformSearchCore(collection.TypeOfElements, query);
				BindSearchResultToGrid(collection, result);
			}
		}

		#endregion

		#endregion
	}
}
