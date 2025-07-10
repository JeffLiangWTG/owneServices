using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Tracking.Business
{
	sealed class ShipmentToDeclarationFilterHelperTest : TestCaseWithFactory
	{
		public void TestCustomAttributesCopiedBetweenFilters()
		{
			var shipmentFilter = new TrackingShipmentFilterBusinessObject();
			var filter = (ModuleTextFilter)shipmentFilter["ComInvoiceLine.CustomAttribute1"];
			filter.IsActive = true;
			filter.Property = "Hello";

			var mapper = new ShipmentToDeclarationFilterHelper(shipmentFilter, declarationFilter);
			mapper.MapFilters();

			AssertContains("Shipment Filter", "JI_CustomAttrib1 like 'Hello%'", shipmentFilter.Filter.LiteralTextADO);
			AssertContains("Declaration Filter", "JI_CustomAttrib1 like 'Hello%'", declarationFilter.Filter.LiteralTextADO);
		}

		public void TestShipmentNumberIsMappedToDeclarationNumber()
		{
			var shipmentFilter = new TrackingShipmentFilterBusinessObject();
			var filter = (ModuleFountainFilter)shipmentFilter["Shipment #"];
			filter.IsActive = true;
			filter.Property = "S00001001";

			var mapper = new ShipmentToDeclarationFilterHelper(shipmentFilter, declarationFilter);
			mapper.MapFilters();

			AssertContains("Shipment Filter", "JS_UniqueConsignRef like 'S00001001%'", shipmentFilter.Filter.LiteralTextADO);
			AssertContains("Declaration Filter", "JE_DeclarationReference like 'S00001001%'", declarationFilter.Filter.LiteralTextADO);
		}

		public void TestCreatedTimeIsMappedToDeclarationNumber()
		{
			var shipmentFilter = new TrackingShipmentFilterBusinessObject();

			var filter = (ModuleDateFilter)shipmentFilter["Created Time"];
			filter.IsActive = true;
			filter.Property1 = new ZDateTime(2007, 1, 1, 0, 0, 0);
			filter.Property2 = new ZDateTime(2009, 1, 1, 0, 0, 0);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			var mapper = new ShipmentToDeclarationFilterHelper(shipmentFilter, declarationFilter);
			mapper.MapFilters();

			AssertContains("Shipment Filter", "(JS_SystemCreateTimeUtc >= #2007-01-01 00:00:00.000# and JS_SystemCreateTimeUtc < #2009-01-02 00:00:00.000#)", shipmentFilter.Filter.LiteralTextADO);
			AssertContains("Declaration Filter", "(JE_SystemCreateTimeUtc >= #2007-01-01 00:00:00.000# and JE_SystemCreateTimeUtc < #2009-01-02 00:00:00.000#)", declarationFilter.Filter.LiteralTextADO);
		}

		public void TestShipmentDeclarationCountryMappedToDeclarationCountry()
		{
			ZArchitecture.Environment.Globals.IsWeb = true;
			var shipmentFilter = new TrackingShipmentFilterBusinessObject();

			var shipmentDecCountryfilter = (ModuleNkFilter)shipmentFilter[TrackingDeclarationFilterConstants.DeclarationCountry];
			AssertNotNull(shipmentDecCountryfilter);
			shipmentDecCountryfilter.IsActive = true;
			shipmentDecCountryfilter.Property = "AU";

			var mapper = new ShipmentToDeclarationFilterHelper(shipmentFilter, declarationFilter);
			var declarationCountryFilter = (ModuleNkFilter)declarationFilter[DeclarationFilterConstants.Country];
			AssertNotNull(declarationCountryFilter);

			AssertEquals("Before mapped, declration country should be empty", "", declarationCountryFilter.Property);

			mapper.MapFilters();

			AssertEquals("After mapped, declration country should be the same as shipment's declaration country", shipmentDecCountryfilter.Property, declarationCountryFilter.Property);
		}

		public void TestShipmentTypeNotMapped()
		{
			var shipmentFilter = new TrackingShipmentFilterBusinessObject();
			var filter = (ModuleFlagsFilter)shipmentFilter["Shipment Type"];
			filter.IsActive = true;

			var mapper = new ShipmentToDeclarationFilterHelper(shipmentFilter, declarationFilter);
			AssertNoExceptionThrown("No Exception should be thrown", () => mapper.MapFilters());
		}

		public void TestStatusNotMapped()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;

			var def = template.GenCustomColumnDefinitions.AddNew();
			def.XC_Name = "Status";
			def.XC_Type = AddOnColumnDataType.Codes.Datetime;

			Factory.Save();

			var shipmentFilter = new TrackingShipmentFilterBusinessObject();

			var filter = (ModuleDateFilter)shipmentFilter["Status " + WorkflowCustomFieldsFilter.WorkflowCustomFieldDescriptionDuplicateSuffix];
			filter.IsActive = true;
			filter.Property1 = new ZDateTime(2011, 12, 12);
			filter.PropertySearch = "Date Range";

			var mapper = new ShipmentToDeclarationFilterHelper(shipmentFilter, declarationFilter);
			AssertNoExceptionThrown("No Exception should be thrown", () => mapper.MapFilters());
		}

		public void TestLastCompletedMilestoneIsMappedToDeclaration()
		{
			AssertWorkflowModuleFilterIsMappedToDeclaration("Last Completed Milestone");
		}

		public void TestMilestoneDateIsMappedToDeclaration()
		{
			AssertWorkflowModuleFilterIsMappedToDeclaration("Milestone Date");
		}

		public void TestNextMilestoneIsMappedToDeclaration()
		{
			AssertWorkflowModuleFilterIsMappedToDeclaration("Next Milestone");
		}

		void AssertWorkflowModuleFilterIsMappedToDeclaration(string filterName)
		{
			var shipmentFilter = new TrackingShipmentFilterBusinessObject();

			var shipmentMilestoneFilter = (WorkflowModuleFilter)shipmentFilter[filterName];
			shipmentMilestoneFilter.IsActive = true;
			shipmentMilestoneFilter.Property1 = new ZDateTime(2007, 1, 1, 0, 0, 0);
			shipmentMilestoneFilter.Property2 = new ZDateTime(2009, 1, 1, 0, 0, 0);
			shipmentMilestoneFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			var mapper = new ShipmentToDeclarationFilterHelper(shipmentFilter, declarationFilter);
			mapper.MapFilters();

			var declarationMilestoneFilter = (WorkflowModuleFilter)declarationFilter[filterName];

			CombineAssertions(() =>
			{
				AssertEquals(true, declarationMilestoneFilter.IsActive);
				AssertEquals(shipmentMilestoneFilter.Property1, declarationMilestoneFilter.Property1);
				AssertEquals(shipmentMilestoneFilter.Property2, declarationMilestoneFilter.Property2);
				AssertEquals(shipmentMilestoneFilter.PropertySearch, declarationMilestoneFilter.PropertySearch);
			});
		}

		public void TestMilestoneCompletedIsMappedToDeclaration()
		{
			var shipmentFilter = new TrackingShipmentFilterBusinessObject();

			var shipmentMilestoneFilter = (WorkflowModuleTextFilter)shipmentFilter["Milestone Completed"];
			shipmentMilestoneFilter.IsActive = true;
			shipmentMilestoneFilter.Property = "Value";

			var mapper = new ShipmentToDeclarationFilterHelper(shipmentFilter, declarationFilter);
			mapper.MapFilters();

			var declarationMilestoneFilter = (WorkflowModuleTextFilter)declarationFilter["Milestone Completed"];

			CombineAssertions(() =>
			{
				AssertEquals(true, declarationMilestoneFilter.IsActive);
				AssertEquals(shipmentMilestoneFilter.Property, declarationMilestoneFilter.Property);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declarationFilterBusinessObjectFactory = new TrackingJobDeclarationFilterBusinessObjectFactory();
			declarationFilter = declarationFilterBusinessObjectFactory.GetJobDeclarationFilterBusinessObject("AU");
			ShipmentJobDeclarationFilterDecorator.Decorate(declarationFilter);
		}

		JobDeclarationFilterBusinessObject declarationFilter;
	}
}
