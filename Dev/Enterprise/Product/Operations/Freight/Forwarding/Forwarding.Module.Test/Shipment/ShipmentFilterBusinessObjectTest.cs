using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.Module;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using FluentAssertions;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(JobShipmentFilterBusinessObject))]
	public class ShipmentFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region ShipmentNumber

		public void TestShipmentNumber_EndsWith()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "SHP10A";

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "SHP10B";

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_UniqueConsignRef = "SHP20A";

			var shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment4.JS_UniqueConsignRef = "SHP20B";

			Factory.Save();

			var filter = (ModuleFountainFilter)FilterStripBizO["Shipment #"];
			AssertEquals(ModuleTextFilter.MultiplyMaxLength(JobShipmentSchema.JS_UniqueConsignRef.MaxLength), filter.MaxLength);

			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.EndsWith;
			filter.Property = "0A";

			var collection = new ForwardingShipmentCollection(Factory);
			collection.Load(FilterStripBizO.Filter);

			AssertMultilineASCIIEquals("generated filter SQL",
@"JS_UniqueConsignRef_Reversed LIKE 'A0%'", filter.Query.LiteralTextSqlFormatted.Trim());

			AssertContainsExactElementsInAnyOrder("loaded shipments", new[]
			{
				shipment1,
				shipment3
			}, collection);
		}

		#endregion

		#region HouseBill

		public void TestHouseBill_EndsWith()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_HouseBill = "HBL100";

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_HouseBill = "HBL101";

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_HouseBill = "HBL100";

			var shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment4.JS_HouseBill = "HBL103";

			Factory.Save();

			var filter = (ModuleNumberFilter)FilterStripBizO["House Bill"];
			AssertEquals(ModuleTextFilter.MultiplyMaxLength(JobShipmentSchema.JS_HouseBill.MaxLength), filter.MaxLength);

			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.EndsWith;
			filter.Property = "100";

			var collection = new ForwardingShipmentCollection(Factory);
			collection.Load(FilterStripBizO.Filter);

			AssertMultilineASCIIEquals("generated filter SQL",
@"JS_HouseBill_Reversed LIKE '001%'", filter.Query.LiteralTextSqlFormatted.Trim());

			AssertContainsExactElementsInAnyOrder("loaded shipments", new[]
			{
				shipment1,
				shipment3
			}, collection);
		}

		#endregion

		public void TestGetCommodityCodeMultiValueQuery()
		{
			var commodityCode1 = Factory.New<RefCommodityCode>();
			commodityCode1.RH_Code = "COM1";
			var commodityCode2 = Factory.New<RefCommodityCode>();
			commodityCode2.RH_Code = "COM2";
			var commodityCode3 = Factory.New<RefCommodityCode>();
			commodityCode3.RH_Code = "COM3";
			Factory.Save();

			var filter = (ModuleNkFilter)FilterStripBizO["Commodity Code"];
			filter.Property = "COM1";
			filter.IsActive = true;
			var multiValueFilter = (ISupportMultiValuesFilter)filter;
			filter.OrCategory = FilterOrCategory.Red;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			Assert("Force group filters with MultiValueQueryDelegate", multiValueFilter.CanGroup);
			var combinedValue = new List<ZString> { "COM1", "COM2", "COM3" };
			AssertEquals("MultiValueQuery: In OrCategory",
						"JS_PK IN (SELECT JL_JS FROM dbo.JobPackLines WHERE (JL_RH_NKCommodityCode in ('COM1', 'COM2', 'COM3'))) or (JS_PK IN (" +
						"SELECT JE_JS FROM dbo.JobDeclaration WITH (FORCESEEK, INDEX(FK_RX__JE_JS_JE_GC)) WHERE JE_ClusterKey IN (" +
						"SELECT JI_ClusterKey FROM dbo.JobComInvoiceLine WITH (FORCESEEK, INDEX(NR_RC__JI_ClusterKey)) WHERE (JI_RH_NKCommodity_Code in ('COM1', 'COM2', 'COM3'))) and JE_IsCancelled = 0))",
						multiValueFilter.GetCombinedQuery(combinedValue).LiteralTextADO);

			filter.OrCategory = FilterOrCategory.None;
			Assert("Can not group filters with with operator = and sql operator AND", !multiValueFilter.CanGroup);
		}

		public void TestGetCommodityCodeQuerySqlGeneration()
		{
			var commodityCode1 = Factory.New<RefCommodityCode>();
			commodityCode1.RH_Code = "COM1";
			Factory.Save();

			var filter = (ModuleNkFilter)FilterStripBizO["Commodity Code"];
			filter.Property = commodityCode1.RH_Code;

			AssertEquals("JI_RH_NKCommodity_Code has poor selectivity, and hence adding an index doesn't make sense. " +
						"Justification for FORCESEEK+INDEX is that Shipments should be sufficiently filtered anyway, so a FORCESEEK plan should be feasible." +
						"Any plan not seeking on JobDeclaration/JobComInvoiceLine (because Shipments are not properly filtered) is bound to cause performance issues anyway.",
						"JS_PK IN (SELECT JL_JS FROM dbo.JobPackLines WHERE JL_RH_NKCommodityCode = 'COM1') or (JS_PK IN (" +
						"SELECT JE_JS FROM dbo.JobDeclaration WITH (FORCESEEK, INDEX(FK_RX__JE_JS_JE_GC)) WHERE JE_ClusterKey IN (" +
						"SELECT JI_ClusterKey FROM dbo.JobComInvoiceLine WITH (FORCESEEK, INDEX(NR_RC__JI_ClusterKey)) WHERE JI_RH_NKCommodity_Code = 'COM1') and JE_IsCancelled = 0))",
						filter.Query.LiteralTextADO);
			AssertEquals("Default IsForceSeek for the main query", false, filter.Query.IsForceSeek);
			AssertContains("CompleteSQLStatement: no WITH (FORCESEEK) for main table JobShipment",
				@"	FROM dbo.JobShipment
	WHERE JS_PK IN (SELECT JL_JS FROM dbo.JobPackLines WHERE JL_RH_NKCommodityCode = @CWO1_) or (JS_PK IN (SELECT JE_JS FROM dbo.JobDeclaration WITH (FORCESEEK, INDEX(FK_RX__JE_JS_JE_GC)) WHERE JE_ClusterKey IN (SELECT JI_ClusterKey FROM dbo.JobComInvoiceLine WITH (FORCESEEK, INDEX(NR_RC__JI_ClusterKey)) WHERE JI_RH_NKCommodity_Code = @CWO2_) and JE_IsCancelled = @CWO3_))",
				filter.Query.GetAsCompleteSQLStatement("JobShipment", false));
		}

		#region Job Consol Shipments Filter

		class TestPopupFindBox : ZArchitecture.GUI.Internal.ZPopupFindBox
		{
			public IFindBoxPopup PopForm
			{
				get
				{
					return base.PopupForm;
				}
			}
		}

		[RequiresSTA]
		public void TestAddRelatedConsolidationsFilterWithNoRight()
		{
			AssertNoExceptionThrown(() =>
			{
				using (JobShipmentModule module = (JobShipmentModule)ZModuleFactory.Instance.Create(ModuleIDs.JobShipment))
				using (TestPopupFindBox pop = new TestPopupFindBox())
				{
					Env.Security.MaintainShipment.IsAllowed = false;
					var list = (module.FilterBusinessObject.ToList()
															.FirstOrDefault(o => o.GetType() == typeof(ConsolsOfShipmentFilter)) as ModuleFilterWithList).List;
					pop.ModuleID = ModuleIDs.JobShipment;
					pop.List = list;
					AssertNotNull(pop.List);

					var form = pop.PopForm;
				}
			});
		}

		#endregion

		#region Not Filters

		public void TestGetModuleFiltersWhenCustomFilterNamesClashWithReservedNames()
		{
			var customFieldName = "ETA";
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";

			var columnDef = template.GenCustomColumnDefinitions.AddNew();
			columnDef.XC_Name = customFieldName;
			columnDef.XC_Type = Enterprise.MasterFiles.Business.CustomValues.AddOnColumnDataType.Codes.String;

			Factory.Save();

			var collection = new JobShipmentFilterBusinessObject().ModuleFilters;

			AssertNotNull(collection[customFieldName]);
			AssertNotNull(collection[customFieldName + " " + WorkflowCustomFieldsFilter.WorkflowCustomFieldDescriptionDuplicateSuffix]);
		}

		public void TestConsolNumberNotFilters()
		{
			var shipment1 = GetShipment("0001");
			var shipment2 = GetShipment("0002");
			var shipment3 = GetShipment("0003");
			var shipment4 = GetShipment("0004");

			var consol1 = shipment1.Consols.AddNew();
			consol1.JK_UniqueConsignRef = "C0001";
			var consol2 = shipment2.Consols.AddNew();
			consol2.JK_UniqueConsignRef = "C0002";
			var consol3 = shipment2.Consols.AddNew();
			consol3.JK_UniqueConsignRef = "C0003";
			shipment3.Consols.Add(consol3);

			Factory.Save();

			var filter = (ModuleFountainFilter)FilterStripBizO["Consol #"];
			filter.IsActive = true;
			filter.Property = "C0003";

			var collection = new ForwardingShipmentCollection(Factory);

			Action<string> assertComparisonOperator = comparisonOperator =>
				{
					filter.ComparisonOperator = comparisonOperator;
					collection.Load(FilterStripBizO.Filter);

					AssertCollectionContains(shipment1, collection);
					AssertCollectionNotContains(shipment2, collection);
					AssertCollectionNotContains(shipment3, collection);
					AssertCollectionNotContains(shipment4, collection);
				};

			assertComparisonOperator(ModuleTextFilter.ComparisonConstants.NotEqual);
			assertComparisonOperator(ModuleTextFilter.ComparisonConstants.NotContain);
			assertComparisonOperator(ModuleTextFilter.ComparisonConstants.NotStartsWith);
		}

		public void TestConsolNumberFilter_MultipleEqual()
		{
			var shipment1 = GetShipment("0001");
			var shipment2 = GetShipment("0002");
			var shipment3 = GetShipment("0003");

			var consol1 = shipment1.Consols.AddNew();
			consol1.JK_UniqueConsignRef = "C0001";
			var consol2 = shipment2.Consols.AddNew();
			consol2.JK_UniqueConsignRef = "C0002";
			var consol3 = shipment3.Consols.AddNew();
			consol3.JK_UniqueConsignRef = "C0003";

			Factory.Save();

			var consolNumberFilter1 = (ModuleFountainFilter)FilterStripBizO["Consol #"];
			consolNumberFilter1.Property = "C0001";
			consolNumberFilter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			consolNumberFilter1.IsActive = true;

			var consolNumberFilter2 = (ModuleFountainFilter)FilterStripBizO.CreateDuplicateFor("Consol #");
			consolNumberFilter2.Property = "C0002";
			consolNumberFilter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			consolNumberFilter2.IsActive = true;

			var consolNumberFilter3 = (ModuleFountainFilter)FilterStripBizO.CreateDuplicateFor("Consol #");
			consolNumberFilter3.Property = "C0003";
			consolNumberFilter3.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			consolNumberFilter3.IsActive = true;

			consolNumberFilter1.OrCategory = FilterOrCategory.Red;
			consolNumberFilter2.OrCategory = FilterOrCategory.Red;
			consolNumberFilter3.OrCategory = FilterOrCategory.Red;

			AssertContains("Generated filter SQL",
				"JS_PK IN (SELECT JN_JS FROM dbo.JobConShipLink WHERE JN_JK IN (SELECT JK_PK FROM dbo.JobConsol WHERE (JK_UniqueConsignRef in (@CWO3_, @CWO4_, @CWO5_)))",
				FilterStripBizO.Filter.FilterString);

			var shipments = new ForwardingShipmentCollection(Factory);
			shipments.Load(FilterStripBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2, shipment3 }, shipments);
		}

		public void TestConsolNumberFilter_MultipleNotEqual()
		{
			var shipment1 = GetShipment("0001");
			var shipment2 = GetShipment("0002");
			var shipment3 = GetShipment("0003");

			var consol1 = shipment1.Consols.AddNew();
			consol1.JK_UniqueConsignRef = "C0001";
			var consol2 = shipment2.Consols.AddNew();
			consol2.JK_UniqueConsignRef = "C0002";
			var consol3 = shipment3.Consols.AddNew();
			consol3.JK_UniqueConsignRef = "C0003";

			Factory.Save();

			var consolNumberFilter1 = (ModuleFountainFilter)FilterStripBizO["Consol #"];
			consolNumberFilter1.Property = "C0001";
			consolNumberFilter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			consolNumberFilter1.IsActive = true;

			var consolNumberFilter2 = (ModuleFountainFilter)FilterStripBizO.CreateDuplicateFor("Consol #");
			consolNumberFilter2.Property = "C0002";
			consolNumberFilter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			consolNumberFilter2.IsActive = true;

			AssertContains("Generated filter SQL",
				"JS_PK IN (SELECT JN_JS FROM dbo.JobConShipLink WHERE JN_JK IN (SELECT JK_PK FROM dbo.JobConsol WHERE (JK_UniqueConsignRef not in (@CWO1_, @CWO2_)))",
				FilterStripBizO.Filter.FilterString);

			var shipments = new ForwardingShipmentCollection(Factory);
			shipments.Load(FilterStripBizO.Filter);

			AssertCollectionNotContains(shipment1, shipments);
			AssertCollectionNotContains(shipment2, shipments);
			AssertCollectionContains(shipment3, shipments);
		}

		#endregion

		#region Mode Filters

		public void TestConsolContainerMode()
		{
			ForwardingShipment shipment1 = GetShipment((++shipmentNumberIndex).ToString());
			ForwardingShipment shipment2 = GetShipment((++shipmentNumberIndex).ToString());
			ForwardingShipment shipment3 = GetShipment((++shipmentNumberIndex).ToString());

			ForwardingConsol consol11 = shipment1.Consols.AddNew();
			ForwardingConsol consol12 = shipment1.Consols.AddNew();
			ForwardingConsol consol21 = shipment2.Consols.AddNew();
			ForwardingConsol consol22 = shipment2.Consols.AddNew();
			ForwardingConsol consol31 = shipment3.Consols.AddNew();
			ForwardingConsol consol32 = shipment3.Consols.AddNew();

			consol11.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol12.JK_ConsolMode = Constants.ContainerModes.Other;
			consol21.JK_ConsolMode = Constants.ContainerModes.LCL;
			consol22.JK_ConsolMode = Constants.ContainerModes.Other;
			consol31.JK_ConsolMode = Constants.ContainerModes.Mail;
			consol32.JK_ConsolMode = Constants.ContainerModes.Other;

			Factory.Save();
			Asserter.AddFieldOfInterest(JobShipmentSchema.JS_UniqueConsignRef.Name);

			ModuleTextFilter shipmentFilter = (ModuleTextFilter)FilterStripBizO["Consol Container Mode"];
			shipmentFilter.IsActive = true;

			Asserter.AssertMatches("", shipmentFilter, shipment1, shipment2, shipment3);

			shipmentFilter.Property = Constants.ContainerModes.ULD;
			Asserter.AssertMatches("", shipmentFilter);

			shipmentFilter.Property = Constants.ContainerModes.Other;
			Asserter.AssertMatches("", shipmentFilter, shipment1, shipment2, shipment3);

			shipmentFilter.Property = Constants.ContainerModes.LCL;
			Asserter.AssertMatches("", shipmentFilter, shipment2);
		}

		public void TestConsolTransportMode()
		{
			foreach (ForwardingShipment junkShipment in Factory.Load<ForwardingShipment>(new ZQuery()))
			{
				junkShipment.JS_IsForwardRegistered = false;
				junkShipment.Delete();
			}

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();

			Func<string, ForwardingShipment[], ForwardingConsol> createConsol = (transportMode, attachedShipments) =>
				{
					ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
					consol.JK_TransportMode = transportMode;
					consol.Shipments.AddRange(attachedShipments);
					return consol;
				};

			ForwardingConsol airConsol = createConsol(Constants.TransportModes.Air, new ForwardingShipment[] { shipment1, shipment2 });
			ForwardingConsol seaConsol = createConsol(Constants.TransportModes.Sea, new ForwardingShipment[] { shipment2, shipment3 });
			ForwardingConsol roadConsol = createConsol(Constants.TransportModes.Road, new ForwardingShipment[] { shipment4 });

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Consol Transport Mode"];
			AssertContainsExactElementsInAnyOrder(FreightCodePairLists.LinkableTransportModeList(), filter.List);

			Action<string, ForwardingShipment[]> assertFiltering = (filterValue, expectedShipments) =>
				{
					filter.IsActive = true;
					filter.Property = filterValue;
					AssertContainsExactElementsInAnyOrder(expectedShipments, Factory.Load<ForwardingShipment>(FilterStripBizO.Filter));
				};

			assertFiltering("", new ForwardingShipment[] { shipment1, shipment2, shipment3, shipment4 });
			assertFiltering(Constants.TransportModes.Air, new ForwardingShipment[] { shipment1, shipment2 });
			assertFiltering(Constants.TransportModes.Sea, new ForwardingShipment[] { shipment2, shipment3 });
			assertFiltering(Constants.TransportModes.Road, new ForwardingShipment[] { shipment4 });
			assertFiltering(Constants.TransportModes.Rail, Array.Empty<ForwardingShipment>());
		}

		public void TestShipmentTypeFilter()
		{
			ForwardingShipment shipmentStandard = Factory.New<ForwardingShipment>();
			shipmentStandard.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			ForwardingShipment shipmentCoload = Factory.New<ForwardingShipment>();
			shipmentCoload.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			ForwardingShipment shipmentBlindCoload = Factory.New<ForwardingShipment>();
			shipmentBlindCoload.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;

			ForwardingShipment shipmentBuyers = Factory.New<ForwardingShipment>();
			shipmentBuyers.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;

			ForwardingShipment shipmentAssembly = Factory.New<ForwardingShipment>();
			shipmentAssembly.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			ForwardingShipment shipmentHighVolumeLowValue = Factory.New<ForwardingShipment>();
			shipmentHighVolumeLowValue.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;

			ForwardingShipment shipmentHighVolumeLowValueMaster = Factory.New<ForwardingShipment>();
			shipmentHighVolumeLowValueMaster.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValueMaster;

			ForwardingShipment shipmentThirdPartyOwnershipHouse = Factory.New<ForwardingShipment>();
			shipmentThirdPartyOwnershipHouse.JS_ShipmentType = Constants.ShipmentTypes.ThirdPartyOwnershipHouse;

			Factory.Save();

			var results = new ForwardingShipmentCollection(Factory);
			results.Load();

			CombineAssertions(() =>
			{
				AssertFilteredByShipmentTypeCollectionContainsOnlySpecifiedShipment(Constants.ShipmentTypes.CoLoadMaster, shipmentCoload);
				AssertFilteredByShipmentTypeCollectionContainsOnlySpecifiedShipment(Constants.ShipmentTypes.BlindCoLoadMaster, shipmentBlindCoload);
				AssertFilteredByShipmentTypeCollectionContainsOnlySpecifiedShipment(Constants.ShipmentTypes.BuyersConsolLead, shipmentBuyers);
				AssertFilteredByShipmentTypeCollectionContainsOnlySpecifiedShipment(Constants.ShipmentTypes.AssemblyMaster, shipmentAssembly);
				AssertFilteredByShipmentTypeCollectionContainsOnlySpecifiedShipment(Constants.ShipmentTypes.StandardHouse, shipmentStandard);
				AssertFilteredByShipmentTypeCollectionContainsOnlySpecifiedShipment(Constants.ShipmentTypes.HighVolumeLowValue, shipmentHighVolumeLowValue);
				AssertFilteredByShipmentTypeCollectionContainsOnlySpecifiedShipment(Constants.ShipmentTypes.HighVolumeLowValueMaster, shipmentHighVolumeLowValueMaster);
				AssertFilteredByShipmentTypeCollectionContainsOnlySpecifiedShipment(Constants.ShipmentTypes.ThirdPartyOwnershipHouse, shipmentThirdPartyOwnershipHouse);
			});
		}

		void AssertFilteredByShipmentTypeCollectionContainsOnlySpecifiedShipment(ZString shipmentType, ForwardingShipment shipment)
		{
			ModuleFlagsFilter shipmentFilter = (ModuleFlagsFilter)FilterStripBizO["Shipment Type"];
			shipmentFilter.IsActive = true;

			shipmentFilter.Property0 = shipmentType == Constants.ShipmentTypes.AssemblyMaster;
			shipmentFilter.Property1 = shipmentType == Constants.ShipmentTypes.BuyersConsolLead;
			shipmentFilter.Property2 = shipmentType == Constants.ShipmentTypes.CoLoadMaster;
			shipmentFilter.Property3 = shipmentType == Constants.ShipmentTypes.BlindCoLoadMaster;
			shipmentFilter.Property4 = shipmentType == Constants.ShipmentTypes.StandardHouse;
			shipmentFilter.Property5 = shipmentType == Constants.ShipmentTypes.HighVolumeLowValue;
			shipmentFilter.Property6 = shipmentType == Constants.ShipmentTypes.HighVolumeLowValueMaster;
			shipmentFilter.Property7 = shipmentType == Constants.ShipmentTypes.ThirdPartyOwnershipHouse;

			ForwardingShipmentCollection results = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			results.Load();

			AssertEquals("Shipment should be in the collection", true, results.Contains(shipment.PK));
			AssertEquals("And it's the only shipment in the collection", shipmentType == Constants.ShipmentTypes.StandardHouse ? 7 : 1, results.Count);
		}

		public void TestPhaseFilter()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_Phase = "ALL";

			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_Phase = "XXX";

			ForwardingShipment shipment3 = Factory.New<ForwardingShipment>();
			shipment3.JS_Phase = "XXX";

			Factory.Save();

			ModuleTextFilter shipmentFilter = (ModuleTextFilter)FilterStripBizO["Phase"];
			ForwardingShipmentCollection results = new ForwardingShipmentCollection(Factory);

			shipmentFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Should contain shipment1", true, results.Contains(shipment1));
			AssertEquals("Should contain shipment2", true, results.Contains(shipment2));
			AssertEquals("Should contain shipment3", true, results.Contains(shipment3));

			shipmentFilter.Property = "ALL";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Should contain shipment1", true, results.Contains(shipment1));
			AssertEquals("Should not contain shipment2", false, results.Contains(shipment2));
			AssertEquals("Should not contain shipment3", false, results.Contains(shipment3));

			shipmentFilter.Property = "XXX";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Should not contain shipment1", false, results.Contains(shipment1));
			AssertEquals("Should contain shipment2", true, results.Contains(shipment2));
			AssertEquals("Should contain shipment3", true, results.Contains(shipment3));
		}

		public void TestServiceTypeDateFilter()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var jobService = shipment.DocsAndCartage.Services.AddNew();
			jobService.ES_Booked = ZDateTime.BrettsBirthday;
			jobService.ES_Completed = ZDateTime.BrettsBirthday.AddDays(10);
			jobService.ES_ServiceCode = "FUM";
			Factory.Save();

			var bookedFilter = (ServiceTypeDateFilter)FilterStripBizO["Service Type / Date Booked"];
			bookedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			bookedFilter.IsActive = true;
			var completedFilter = (ServiceTypeDateFilter)FilterStripBizO["Service Type / Date Completed"];
			completedFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			completedFilter.IsActive = true;

			var results = new ForwardingShipmentCollection(Factory);

			bookedFilter.JobServiceType = "MUF";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("ES_ServiceCode is not matched so there is no data.", false, results.Any());

			bookedFilter.JobServiceType = "FUM";
			bookedFilter.Property2 = ZDateTime.BrettsBirthday.AddDays(-1);
			results.Load(FilterStripBizO.Filter);
			AssertEquals("ES_Booked is not matched so there is no data.", false, results.Any());

			bookedFilter.Property2 = ZDateTime.BrettsBirthday;
			completedFilter.Property1 = ZDateTime.BrettsBirthday.AddDays(11);
			results.Load(FilterStripBizO.Filter);
			AssertEquals("ES_Completed is not matched so there is no data.", false, results.Any());

			completedFilter.Property1 = ZDateTime.BrettsBirthday.AddDays(10);
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Should have one data", 1, results.Count);
			AssertEquals(true, results.Contains(shipment));
		}

		#region TestDGClassDGSubstance

		public void TestDGClassDGSubstance()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline1 = shipment1.OuterPackLines.AddNew();
			var undg1 = packline1.UNDGs.AddNew();
			undg1.DI_IMOClass = "1";

			var subsPivot1 = Factory.New<UNDGSubstancePivot>();
			subsPivot1.DP_UNNO = "9999";
			subsPivot1.DP_Variant = "a";
			subsPivot1.DP_ParentId = undg1.PK;
			subsPivot1.DP_ParentTableCode = undg1.TablePrefix;
			subsPivot1.DP_Standard = "IMO";
			subsPivot1.DP_IsDefault = true;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline2 = shipment2.OuterPackLines.AddNew();
			var undg2 = packline2.UNDGs.AddNew();
			undg2.DI_IMOClass = "1.2B";

			var subsPivot2 = Factory.New<UNDGSubstancePivot>();
			subsPivot2.DP_UNNO = "9999";
			subsPivot2.DP_Variant = "b";
			subsPivot2.DP_ParentId = undg2.PK;
			subsPivot2.DP_ParentTableCode = undg2.TablePrefix;
			subsPivot2.DP_Standard = "IMO";
			subsPivot2.DP_IsDefault = true;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline3 = shipment3.OuterPackLines.AddNew();
			var undg3 = packline3.UNDGs.AddNew();
			undg3.DI_IMOClass = "3";

			var subsPivot3 = Factory.New<UNDGSubstancePivot>();
			subsPivot3.DP_UNNO = "9999";
			subsPivot3.DP_Variant = "c";
			subsPivot3.DP_ParentId = undg3.PK;
			subsPivot3.DP_ParentTableCode = undg3.TablePrefix;
			subsPivot3.DP_Standard = "IMO";
			subsPivot3.DP_IsDefault = true;

			var shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline4 = shipment4.OuterPackLines.AddNew();
			packline4.JL_Description = "point blank";

			var shipment5 = Factory.NewWithValidTestData<ForwardingShipment>();

			Factory.Save();

			DGClassDGSubstanceFilter shipmentFilter = (DGClassDGSubstanceFilter)FilterStripBizO["DG Class / DG Substance"];
			ForwardingShipmentCollection results = new ForwardingShipmentCollection(Factory);

			shipmentFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Not filtered - Shipment1 should be included", true, results.Contains(shipment1.PK));
			AssertEquals("Not filtered - Shipment2 should be included", true, results.Contains(shipment2.PK));
			AssertEquals("Not filtered - Shipment3 should be included", true, results.Contains(shipment3.PK));
			AssertEquals("Not filtered - Shipment4 should be included", true, results.Contains(shipment4.PK));

			shipmentFilter.DGClass = "1";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Shipment1 should be in the collection", true, results.Contains(shipment1.PK));
			AssertEquals("Shipment2 should be in the collection", true, results.Contains(shipment2.PK));
			AssertEquals("Shipment3 does not have a 1 in its DG Class", false, results.Contains(shipment3.PK));

			shipmentFilter.DGClass = "1.2B";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Shipment1 does not have a 1.2B DG Class", false, results.Contains(shipment1.PK));
			AssertEquals("Shipment2 should be in the collection", true, results.Contains(shipment2.PK));
			AssertEquals("Shipment3 does not have a 1.2B DG Class", false, results.Contains(shipment3.PK));

			shipmentFilter.DGClass = "1";
			shipmentFilter.DGSubstance = "9999a";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Shipment1 should be in the collection", true, results.Contains(shipment1.PK));
			AssertEquals("Shipment2 doesnt have a 123a DG Substance", false, results.Contains(shipment2.PK));
			AssertEquals("Shipment3 does not have a 1 in its DG Class or 123a DG Substance", false, results.Contains(shipment3.PK));

			shipmentFilter.DGClass = "1.2B";
			shipmentFilter.DGSubstance = "9999b";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Shipment1 doesnt have a 123b DG Substance", false, results.Contains(shipment1.PK));
			AssertEquals("Shipment2 should be in the collection", true, results.Contains(shipment2.PK));
			AssertEquals("Shipment3 does not have a 1.2B DG Class or 123b DG Substance", false, results.Contains(shipment3.PK));

			shipmentFilter.DGClass = ZString.Empty;
			shipmentFilter.DGSubstance = ZString.Empty;

			shipmentFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Shipment1 should be in the collection", true, results.Contains(shipment1.PK));
			AssertEquals("Shipment2 should be in the collection", true, results.Contains(shipment2.PK));
			AssertEquals("Shipment3 should be in the collection", true, results.Contains(shipment3.PK));
			AssertEquals("Shipment4 should not be in the collection", false, results.Contains(shipment4.PK));
			AssertEquals("Shipment5 should not be in the collection", false, results.Contains(shipment5.PK));

			shipmentFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Shipment1 should not be in the collection", false, results.Contains(shipment1.PK));
			AssertEquals("Shipment2 should not be in the collection", false, results.Contains(shipment2.PK));
			AssertEquals("Shipment3 should not be in the collection", false, results.Contains(shipment3.PK));
			AssertEquals("Shipment4 should be in the collection", true, results.Contains(shipment4.PK));
			AssertEquals("Shipment5 should be in the collection", true, results.Contains(shipment5.PK));

			shipmentFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			shipmentFilter.DGClass = "1.2";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Shipment1 should be in the collection", true, results.Contains(shipment1.PK));
			AssertEquals("Shipment2 should not be in the collection because it has 1.2B DG Substance", false, results.Contains(shipment2.PK));
			AssertEquals("Shipment3 should be in the collection", true, results.Contains(shipment3.PK));
			AssertEquals("Shipment4 should be in the collection", true, results.Contains(shipment4.PK));
			AssertEquals("Shipment5 should be in the collection", true, results.Contains(shipment5.PK));

			shipmentFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			shipmentFilter.DGClass = "1.2";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Shipment1 should be in the collection", true, results.Contains(shipment1.PK));
			AssertEquals("Shipment2 should be in the collection", true, results.Contains(shipment2.PK));
			AssertEquals("Shipment3 should be in the collection", true, results.Contains(shipment3.PK));
			AssertEquals("Shipment4 should be in the collection", true, results.Contains(shipment4.PK));
			AssertEquals("Shipment5 should be in the collection", true, results.Contains(shipment5.PK));

			shipmentFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			shipmentFilter.DGClass = ZString.Empty;
			shipmentFilter.DGSubstance = "9999b";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Shipment1 should be in the collection", true, results.Contains(shipment1.PK));
			AssertEquals("Shipment2 should not be in the collection because it has 9999b substance", false, results.Contains(shipment2.PK));
			AssertEquals("Shipment3 should be in the collection", true, results.Contains(shipment3.PK));
			AssertEquals("Shipment4 should be in the collection", true, results.Contains(shipment4.PK));
			AssertEquals("Shipment5 should be in the collection", true, results.Contains(shipment5.PK));

			shipmentFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			shipmentFilter.DGClass = ZString.Empty;
			shipmentFilter.DGSubstance = "XXXX";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Shipment1 should be in the collection", true, results.Contains(shipment1.PK));
			AssertEquals("Shipment2 should be in the collection", true, results.Contains(shipment2.PK));
			AssertEquals("Shipment3 should be in the collection", true, results.Contains(shipment3.PK));
			AssertEquals("Shipment4 should not be in the collection because it is blank", false, results.Contains(shipment4.PK));
			AssertEquals("Shipment5 should not be in the collection", false, results.Contains(shipment5.PK));
		}

		#endregion

		#endregion

		#region Not published on Web Filters

		public void TestNotPublishedOnWebFilters()
		{
			if (FilterStripBizO["Sales Rep / Cartage Coordinator"] != null)
			{
				AssertEquals(false, FilterStripBizO["Sales Rep / Cartage Coordinator"].IsPublishedOnWeb);
			}
			if (FilterStripBizO["Customs Broker"] != null)
			{
				AssertEquals(false, FilterStripBizO["Customs Broker"].IsPublishedOnWeb);
			}
			if (FilterStripBizO["Branch (Current Co.)"] != null)
			{
				AssertEquals(false, FilterStripBizO["Branch (Current Co.)"].IsPublishedOnWeb);
			}
		}

		#endregion

		#region Number Filters

		public void TestOrderNumberFilter()
		{
			var orderShipment = Factory.New<ForwardingShipment>();
			var order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_OrderNumber = "ORDER1";
			orderShipment.AttachedOrders.Add(order1);

			var whsShipment = Factory.New<ForwardingShipment>();
			var warehouseOrder1 = (IAttachedOrder)Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsOrder>());
			whsShipment.AttachedWarehouseOrders.Add(warehouseOrder1);

			var mixedShipment = Factory.New<ForwardingShipment>();
			var order2 = Factory.NewWithValidTestData<Order>();
			order2.JD_OrderNumber = "PurchaseORDER1";
			mixedShipment.AttachedOrders.Add(order2);
			var warehouseOrder2 = (IAttachedOrder)Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsOrder>());
			mixedShipment.AttachedWarehouseOrders.Add(warehouseOrder2);

			var docsShipment = Factory.New<ForwardingShipment>();
			docsShipment.DocsAndCartage.JP_OrderItemsAsString = "ORDER-REF1, 12412, Order10";

			var orderlessShipment = Factory.New<ForwardingShipment>();

			Factory.Save();

			Asserter.AddToScope(orderShipment);
			Asserter.AddToScope(whsShipment);
			Asserter.AddToScope(mixedShipment);
			Asserter.AddToScope(docsShipment);
			Asserter.AddToScope(orderlessShipment);

			var results = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);

			var filter = (ModuleNumberFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.OrderNum];
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "";
			filter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Asserter.AssertMatches("No filter, should return all shipments", filter, orderShipment, whsShipment, mixedShipment, docsShipment, orderlessShipment);

			filter.Property = "ORDER1";
			results.Load(FilterStripBizO.Filter);
			Asserter.AssertMatches("Should return orders that contain ORDER1", filter, orderShipment, mixedShipment, docsShipment);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			results.Load(FilterStripBizO.Filter);
			Asserter.AssertMatches("Should return shipments with order numbers / references that don't contain ORDER1", filter, mixedShipment, whsShipment, docsShipment);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			results.Load(FilterStripBizO.Filter);
			Asserter.AssertMatches("Should return shipments with order numbers / references that don't equal ORDER1", filter, mixedShipment, whsShipment, docsShipment);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			results.Load(FilterStripBizO.Filter);
			Asserter.AssertMatches("Should return shipment that contains exactly ORDER1", filter, orderShipment);

			filter.Property = warehouseOrder1.JobNo;
			results.Load(FilterStripBizO.Filter);
			Asserter.AssertMatches("Should return shipment with warehouse order number", filter, whsShipment);

			filter.Property = "12412";
			results.Load(FilterStripBizO.Filter);
			Asserter.AssertMatches("Should return shipment with docs and cartage order ref 12412", filter, docsShipment);

			filter.Property = "ORDER1";
			filter.Property = "ORD";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			results.Load(FilterStripBizO.Filter);
			Asserter.AssertMatches("Should return shipments with order numbers / references that start with ORD", filter, orderShipment, docsShipment);

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			results.Load(FilterStripBizO.Filter);
			Asserter.AssertMatches("Should return shipments with order numbers / references that don't start with ORD", filter, mixedShipment, whsShipment, docsShipment);

			filter.Property = "";
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			results.Load(FilterStripBizO.Filter);
			Asserter.AssertMatches("Should only return shipments with no orders or order refs", filter, orderlessShipment);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			results.Load(FilterStripBizO.Filter);
			Asserter.AssertMatches("Should only return shipments with no orders or order refs", filter, orderShipment, mixedShipment, whsShipment, docsShipment);
		}

		public void TestOrderNumberFilter_WithSBKGeneratedShipment_ByPackLine()
		{
			var packLineShipment = Factory.New<ForwardingShipment>();
			var order1 = Factory.NewWithValidTestData<Order>();
			order1.JD_OrderNumber = "ORDERTESTPACKLINE";

			var orderLine1 = Factory.NewWithValidTestData<OrderLine>();
			orderLine1.JO_LineNo = 13;
			orderLine1.JO_JD = order1.PK;

			var sbk = Factory.NewWithValidTestData<JobSupplierBooking>();
			sbk.JSB_BookingId = "SBK0001";

			var sbkLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			sbkLine.JSL_BookingLineId = "SBL0001";
			sbkLine.JSL_JO_OrderLine = orderLine1.PK;
			sbk.SupplierBookingLines.Add(sbkLine);

			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			var packline = Factory.NewWithValidTestData<ForwardingPackLine>();

			var loadListLine = Factory.NewWithValidTestData<ContainerLoadListLine>();
			loadListLine.CLL_JC_Container = container.PK;
			loadListLine.CLL_JL_PackLine = packline.PK;
			loadListLine.CLL_JSL_BookingLine = sbkLine.PK;

			packline.JL_JS = packLineShipment.PK;

			Factory.Save();

			Asserter.AddToScope(packLineShipment);

			var results = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);

			var filter = (ModuleNumberFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.OrderNum];
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "";
			filter.IsActive = true;

			filter.Property = "ORDERTESTPACKLINE";
			results.Load(FilterStripBizO.Filter);

			Asserter.AssertMatches("Should return orders that contain ORDERTEST", filter, packLineShipment);
		}

		public void TestOrderNumberFilter_WithSPTGeneratedShipment()
		{
			var packLineShipment = Factory.New<ForwardingShipment>();
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "ORDERTESTSPT";

			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			orderLine.JO_LineNo = 13;
			orderLine.JO_JD = order.PK;

			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			booking.JSB_BookingId = "SBK0001";

			var bookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine.JSL_BookingLineId = "SBL0001";
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			booking.SupplierBookingLines.Add(bookingLine);
			Factory.Save();

			var packline = Factory.NewWithValidTestData<ForwardingPackLine>();
			packline.JL_JS = packLineShipment.PK;
			packline.JL_JSL_BookingLine = bookingLine.PK;

			Factory.Save();

			Asserter.AddToScope(packLineShipment);

			var results = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);

			var filter = (ModuleNumberFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.OrderNum];
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "";
			filter.IsActive = true;

			filter.Property = "ORDERTESTSPT";
			results.Load(FilterStripBizO.Filter);

			Asserter.AssertMatches("Should return orders that contain ORDERTESTSPT", filter, packLineShipment);
		}

		public void TestOrderNumberFilter_WithLooseCargoGeneratedShipment()
		{
			var packLineShipment = Factory.New<ForwardingShipment>();
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "ORDERTESTLOOSECARGO";

			var orderLine = Factory.NewWithValidTestData<OrderLine>();
			orderLine.JO_LineNo = 1;
			orderLine.JO_JD = order.PK;

			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			booking.JSB_BookingId = "SBK0001";

			var bookingLine = Factory.NewWithValidTestData<JobSupplierBookingLine>();
			bookingLine.JSL_BookingLineId = "SBL0001";
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			booking.SupplierBookingLines.Add(bookingLine);
			Factory.Save();

			var packline = Factory.NewWithValidTestData<ForwardingPackLine>();
			packline.JL_JS = packLineShipment.PK;
			Factory.Save();
			bookingLine.JSL_JL_LooseCargo = packline.PK;

			Factory.Save();

			Asserter.AddToScope(packLineShipment);

			var results = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);

			var filter = (ModuleNumberFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.OrderNum];
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "";
			filter.IsActive = true;

			filter.Property = "ORDERTESTLOOSECARGO";
			results.Load(FilterStripBizO.Filter);

			Asserter.AssertMatches("Should return orders that contain ORDERTESTLOOSECARGO", filter, packLineShipment);
		}

		public void TestCompanyTariffLevelOverrideFilter()
		{
			var glbTariff = Factory.New<GlobalTariff>();
			var glbTariff2 = Factory.New<GlobalTariff>();
			var glbTariff3 = Factory.New<GlobalTariff>();
			Factory.Save();
			var shipment1 = CreateShipmentWithCompanyTariffLevelOverride(1);
			var shipment2 = CreateShipmentWithCompanyTariffLevelOverride(1);
			var shipment3 = CreateShipmentWithCompanyTariffLevelOverride(1);
			var shipment4 = CreateShipmentWithCompanyTariffLevelOverride(0);
			var shipment5 = CreateShipmentWithCompanyTariffLevelOverride(2);
			var shipment6 = CreateShipmentWithCompanyTariffLevelOverride(3);

			var companyTariffLevelOverrideFilter = ((ModuleTextFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.CompanyTariffLevelOverride]);
			companyTariffLevelOverrideFilter.IsActive = true;
			Factory.Save();

			var results = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			companyTariffLevelOverrideFilter.Property = "0";
			results.Load(FilterStripBizO.Filter);
			AssertCollectionContains(shipment4, results);

			companyTariffLevelOverrideFilter.Property = "1";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new ForwardingShipment[] { shipment1, shipment2, shipment3 }, results);

			companyTariffLevelOverrideFilter.Property = "2";
			results.Load(FilterStripBizO.Filter);
			AssertCollectionContains(shipment5, results);

			companyTariffLevelOverrideFilter.Property = "3";
			results.Load(FilterStripBizO.Filter);
			AssertCollectionContains(shipment6, results);

			companyTariffLevelOverrideFilter.Property = "4";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("No shipments match", 0, results.Count);

			ForwardingShipment CreateShipmentWithCompanyTariffLevelOverride(ZByte companyTariffLevelOverride)
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_CompanyTariffLevelOverride = companyTariffLevelOverride;
				return shipment;
			}
		}

		public void TestFMCTariffIDFilter()
		{
			var shipment1 = CreateShipmentWithFMCTariffID("ABCD");
			var shipment2 = CreateShipmentWithFMCTariffID("");
			Factory.Save();
			var fmcTariffIDFilter = ((ModuleTextFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.FMCTariffID]);
			fmcTariffIDFilter.IsActive = true;

			var results = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);

			AssertEquals("FMCTariffID MaxLength", 4, fmcTariffIDFilter.MaxLength);
			ApplyFilterAndAssert("A", ModuleTextFilter.ComparisonConstants.StartsWith, shipment1);
			ApplyFilterAndAssert("", ModuleTextFilter.ComparisonConstants.IsBlank, shipment2);
			ApplyFilterAndAssert("ABCD", ModuleTextFilter.ComparisonConstants.Exact, shipment1);
			ApplyFilterAndAssert("", ModuleTextFilter.ComparisonConstants.IsNotBlank, shipment1);
			ApplyFilterAndAssert("BC", ModuleTextFilter.ComparisonConstants.Contains, shipment1);

			void ApplyFilterAndAssert(ZString fmcTariffID, ZString comparisonOperator, ForwardingShipment expectedShipment)
			{
				fmcTariffIDFilter.Property = fmcTariffID;
				fmcTariffIDFilter.ComparisonOperator = comparisonOperator;
				results.Load(FilterStripBizO.Filter);
				AssertCollectionContains(expectedShipment, results);
			}

			ForwardingShipment CreateShipmentWithFMCTariffID(ZString fmcTariffID)
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_FMCTariffID = fmcTariffID;
				return shipment;
			}
		}

		public void TestRateCommodityFilter()
		{
			var commodityCode1 = Factory.New<RefCommodityCode>();
			commodityCode1.RH_Code = "COM1";
			var commodityCode2 = Factory.New<RefCommodityCode>();
			commodityCode2.RH_Code = "COM2";
			var commodityCode3 = Factory.New<RefCommodityCode>();
			commodityCode3.RH_Code = "COM3";
			var shipment1 = CreateShipmentWithRateCommodity(commodityCode1);
			var shipment2 = CreateShipmentWithRateCommodity(commodityCode2);
			var shipment3 = CreateShipmentWithRateCommodity(commodityCode3);

			Factory.Save();

			var rateCommodityFilter = (ModuleNkFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.RateCommodity];
			rateCommodityFilter.Property = commodityCode1.RH_Code;
			rateCommodityFilter.IsActive = true;

			var shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertEquals(1, shipments.Count);
			shipments.Select(s => s.PK).Should().BeEquivalentTo(new[] { shipment1.PK }, "Only shipment1 is in Collection");
			ForwardingShipment CreateShipmentWithRateCommodity(RefCommodityCode rateCommodity)
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RH_NKRateCommodity = rateCommodity.RH_Code;
				return shipment;
			}
		}

		public void TestReferenceNumberFilter()
		{
			NewReferenceNumber(Shipment1, "AU", "COC", "MUNDANE");
			NewReferenceNumber(Shipment2, "US", "COC", "MAGIC");
			NewReferenceNumber(Shipment3, "AU", "COC", "MAGIC");
			NewReferenceNumber(Shipment4, "AU", "COC", "INSANE");

			NewReferenceNumber(Shipment1, "AU", "ASL", "MAGIC");
			NewReferenceNumber(Shipment2, "AU", "ASL", "");

			AssertNotNull("lazy load", Shipment5);

			Factory.Save();

			Asserter.AddToScope(Shipment1);
			Asserter.AddToScope(Shipment2);
			Asserter.AddToScope(Shipment3);
			Asserter.AddToScope(Shipment4);
			Asserter.AddToScope(Shipment5);

			Asserter.AddFieldOfInterest("AU:COC", (s) => GetValue(s, "AU", "COC"));
			Asserter.AddFieldOfInterest("US:COC", (s) => GetValue(s, "US", "COC"));
			Asserter.AddFieldOfInterest("AU:ASL", (s) => GetValue(s, "AU", "ASL"));

			ReferenceNumberFilter filter = (ReferenceNumberFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.AdditionalReferenceNumbers];
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			Asserter.AssertMatches("empty", filter, Shipment1, Shipment2, Shipment3, Shipment4, Shipment5);

			SetFilter(filter, "AU", "COC", "MAGIC");
			Asserter.AssertMatches("AU:COC:MAGIC*", filter, Shipment3);

			SetFilter(filter, "", "COC", "MAGIC");
			Asserter.AssertMatches("COC:MAGIC*", filter, Shipment2, Shipment3);

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;

			SetFilter(filter, "", "ASL", "");
			Asserter.AssertMatches("Without ASL", filter, Shipment2, Shipment3, Shipment4, Shipment5);

			SetFilter(filter, "", "ASL", "");
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;

			Asserter.AssertMatches("With ASL", filter, Shipment1);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;

			SetFilter(filter, "", "COC", "MAGIC");
			Asserter.AssertMatches("Without COC:MAGIC", filter, Shipment1, Shipment4, Shipment5);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;

			SetFilter(filter, "", "COC", "ANE");
			Asserter.AssertMatches("Without COC:*ANE", filter, Shipment2, Shipment3, Shipment5);

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;

			SetFilter(filter, "", "COC", "M");
			Asserter.AssertMatches("Without COC:M*", filter, Shipment4, Shipment5);
		}

		public void TestWarehouseLocationFilter_TrueLocation()
		{
			WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var header = Factory.NewWithValidTestData<OrgHeader>();
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse1 = (IWhsWarehouse)helper.CreateWarehouse("WS1", header.MainAddress, GlbBranch.CurrentBranch);
			var warehouse2 = (IWhsWarehouse)helper.CreateWarehouse("WS2", header.MainAddress, GlbCompany.CurrentCompany.Branches.First(g => g.PK != GlbBranch.CurrentBranch.PK));

			var area1 = (IWhsArea)warehouse1.Areas.AddNew();
			var area2 = (IWhsArea)warehouse2.Areas.AddNew();
			area1.WA_Name = "AREA";
			area2.WA_Name = "AREA";

			var row1 = (IWhsRow)warehouse1.Rows.AddNew();
			row1.WR_Name = "BOB";

			var row2 = (IWhsRow)warehouse2.Rows.AddNew();
			row2.WR_Name = "FRANK";

			Factory.Save();

			var location1 = (IWhsLocation)row1.Locations[0];
			location1.WLV_WA_PickingArea = area1.PK;

			var location2 = (IWhsLocation)row2.Locations[0];
			location2.WLV_WA_PickingArea = area2.PK;

			var shipment1 = Factory.New<ForwardingShipment>();
			var packline1 = shipment1.OuterPackLines.AddNew();
			packline1.PackLocations.AddNew().JQ_WL = location1.PK;

			var shipment2 = Factory.New<ForwardingShipment>();
			var packline2 = shipment1.OuterPackLines.AddNew();
			packline1.PackLocations.AddNew().JQ_WL = location2.PK;

			Factory.Save();

			ForwardingShipment[] results;
			var filter = (ModuleWarehouseLocationFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.WarehouseLocation];

			filter.Warehouse = ZGuid.Empty;
			filter.Location = "";
			results = Factory.Load<ForwardingShipment>(filter.Query);
			AssertCollectionContains(shipment1, results);
			AssertCollectionContains(shipment2, results);

			filter.Warehouse = warehouse1.PK;
			results = Factory.Load<ForwardingShipment>(filter.Query);
			AssertCollectionContains(shipment1, results);
			AssertCollectionNotContains(shipment2, results);
		}

		public void TestWarehouseLocationFilter_FreeTextLocation()
		{
			WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			PackLine packline1 = shipment1.OuterPackLines.AddNew();
			PackLocation location1 = packline1.PackLocations.AddNew();
			location1.JQ_WarehouseLocation = "WHS1";

			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			PackLine packline2 = shipment1.OuterPackLines.AddNew();
			PackLocation location2 = packline1.PackLocations.AddNew();
			location2.JQ_WarehouseLocation = "WHS2";

			Factory.Save();

			var results = new ForwardingShipmentCollection(Factory);
			var filter = (ModuleTextFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.WarehouseLocation];
			filter.IsActive = true;

			filter.Property = "";
			results.Load(FilterStripBizO.Filter);
			AssertCollectionContains(shipment1, results);
			AssertCollectionContains(shipment2, results);

			filter.Property = "WHS1";
			results.Load(FilterStripBizO.Filter);
			AssertCollectionContains(shipment1, results);
			AssertCollectionNotContains(shipment2, results);
		}

		public void TestCFSReferenceFilter()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_CFSReference = "111";

			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_CFSReference = "222";

			Factory.Save();

			ModuleNumberFilter shipmentFilter = (ModuleNumberFilter)FilterStripBizO["CFS Reference #"];
			ForwardingShipmentCollection results = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);

			shipmentFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Not filtered - should be included", true, results.Contains(shipment1.PK));
			AssertEquals("Not filtered - should be included", true, results.Contains(shipment2.PK));

			shipmentFilter.Property = "111";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Shipment1 should be in the collection", true, results.Contains(shipment1.PK));
			AssertEquals("Shipment1 should be not in the collection", false, results.Contains(shipment2.PK));
		}

		public void TestDirectMasterLeadShipmentNumber()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S00002001";
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S00002002";
			shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			ForwardingShipment shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_UniqueConsignRef = "S00002003";
			shipment3.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			shipment1.JS_JS_ColoadMasterShipment = shipment2.JS_JS_ColoadMasterShipment = shipment3.PK;

			ForwardingShipment shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment4.JS_UniqueConsignRef = "S00002004";
			shipment4.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;

			shipment3.JS_JS_ColoadMasterShipment = shipment4.PK;

			ForwardingShipment shipment5 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment5.JS_UniqueConsignRef = "S00002005";
			shipment5.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;

			shipment4.JS_JS_ColoadMasterShipment = shipment5.PK;

			Factory.Save();

			ModuleFountainFilter filter = (ModuleFountainFilter)FilterStripBizO["Direct Master/Lead Shipment #"];
			filter.IsActive = true;

			filter.Property = shipment1.JS_UniqueConsignRef;
			AssertEquals(0, Factory.Load<ForwardingShipment>(FilterStripBizO.Filter).Length);

			filter.Property = shipment2.JS_UniqueConsignRef;
			AssertEquals(0, Factory.Load<ForwardingShipment>(FilterStripBizO.Filter).Length);

			filter.Property = shipment3.JS_UniqueConsignRef;
			AssertContainsExactElementsInAnyOrder(new ForwardingShipment[] { shipment1, shipment2 }, Factory.Load<ForwardingShipment>(FilterStripBizO.Filter));

			filter.Property = shipment4.JS_UniqueConsignRef;
			AssertContainsExactElementsInAnyOrder(new ForwardingShipment[] { shipment3 }, Factory.Load<ForwardingShipment>(FilterStripBizO.Filter));

			filter.Property = shipment5.JS_UniqueConsignRef;
			AssertContainsExactElementsInAnyOrder(new ForwardingShipment[] { shipment4 }, Factory.Load<ForwardingShipment>(FilterStripBizO.Filter));
		}

		public void TestBlankMaster()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "ship1";
			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "ship2";
			shipment1.JS_JS_ColoadMasterShipment = shipment2.PK;

			Factory.Save();

			ModuleFountainFilter filter = (ModuleFountainFilter)FilterStripBizO["Direct Master/Lead Shipment #"];
			filter.IsActive = true;

			filter.Property = "";
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			var collection = Factory.Load<ForwardingShipment>(FilterStripBizO.Filter);

			AssertCollectionContains(shipment1, collection);
			AssertCollectionNotContains(shipment2, collection);

			filter.Property = "";
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			collection = Factory.Load<ForwardingShipment>(FilterStripBizO.Filter);

			AssertCollectionNotContains(shipment1, collection);
			AssertCollectionContains(shipment2, collection);
		}

		public void TestQuoteNumber()
		{
			var quotedBooking1 = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.BookingWithQuote, Factory);
			var quotedBooking2 = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.BookingWithQuote, Factory);
			var quotedBooking3 = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.BookingWithQuote, Factory);
			var quote = ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.SpotQuote, Factory);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "00001001";

			((CommonShipment)quotedBooking1.ForwardingShipment).JS_IsForwardRegistered = true;
			((CommonShipment)quotedBooking2.ForwardingShipment).JS_IsForwardRegistered = true;
			((CommonShipment)quotedBooking3.ForwardingShipment).JS_IsForwardRegistered = true;

			Factory.Save();

			var filter = (ModuleNumberFilter)FilterStripBizO["Quote #"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "00001001";
			filter.IsActive = true;

			var results = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			results.Load(FilterStripBizO.Filter);

			AssertEquals("Should load shipment for quotedBooking2", quotedBooking2.ForwardingShipment.PK, results[0].PK);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "00001";

			results.Load(FilterStripBizO.Filter);

			AssertEquals("Should load shipments for QuotedBookings only", 3, results.Count);
			Assert("Should not include shipment not related to quoted bookings", !results.Any(x => x.PK == shipment.PK));

			var job = new JobHeader.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_TH_NKQuoteNumber = "00001004";
			Factory.Save();

			filter.Property = "00001004";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Should load one shipment", 1, results.Count);
			AssertEquals("ShipmentPK should be correct", shipment.PK, results[0].PK);
		}

		public void TestOrgFilterBlankSupport()
		{
			Assert("Blank operators not supported for Carrier", !((ModuleGuidFilter)FilterStripBizO["Carrier"]).SupportsBlankComparisonOperators);
			Assert("Blank operators not supported for Overseas Agent", !((ModuleGuidFilter)FilterStripBizO["Overseas Agent (Billing)"]).SupportsBlankComparisonOperators);

			Assert("Blank operators are supported for Pickup CFS", ((ModuleGuidFilter)FilterStripBizO["PickupCFS"]).SupportsBlankComparisonOperators);
			Assert("Blank operators are supported for Delivery CFS", ((ModuleGuidFilter)FilterStripBizO["DeliveryCFS"]).SupportsBlankComparisonOperators);
			Assert("Blank operators are supported for Pickup Agent", ((ModuleGuidFilter)FilterStripBizO["Pickup Agent"]).SupportsBlankComparisonOperators);
			Assert("Blank operators are supported for Delivery Agent", ((ModuleGuidFilter)FilterStripBizO["Delivery Agent"]).SupportsBlankComparisonOperators);
			Assert("Blank operators are supported for Controlling Agent", ((ModuleGuidFilter)FilterStripBizO["Controlling Agent"]).SupportsBlankComparisonOperators);
			Assert("Blank operators are supported for Controlling Customer", ((ModuleGuidFilter)FilterStripBizO["Controlling Customer"]).SupportsBlankComparisonOperators);
			Assert("Blank operators are supported for Local Client", ((ModuleGuidFilter)FilterStripBizO["Local Client (Billing)"]).SupportsBlankComparisonOperators);
			Assert("Blank operators are supported for Gateway", ((ModuleGuidFilter)FilterStripBizO["Gateway"]).SupportsBlankComparisonOperators);

			Assert("Blank operators are supported for Export Broker", ((ModuleGuidFilter)FilterStripBizO["Export Broker"]).SupportsBlankComparisonOperators);
			Assert("Blank operators are supported for Import Broker", ((ModuleGuidFilter)FilterStripBizO["Import Broker"]).SupportsBlankComparisonOperators);

			Assert("Blank operators are supported for Pickup Transport Company", ((ModuleGuidFilter)FilterStripBizO["Pickup Transport Company"]).SupportsBlankComparisonOperators);
			Assert("Blank operators are supported for Delivery Transport Company", ((ModuleGuidFilter)FilterStripBizO["Delivery Transport Company"]).SupportsBlankComparisonOperators);
			Assert("Blank operators are supported for Consignee", ((ModuleGuidFilter)FilterStripBizO["Consignor"]).SupportsBlankComparisonOperators);
			Assert("Blank operators are supported for Consignor", ((ModuleGuidFilter)FilterStripBizO["Consignor"]).SupportsBlankComparisonOperators);

			Assert("Blank operators are supported for Transhipment Agent", ((ModuleGuidFilter)FilterStripBizO["Transhipment Agent"]).SupportsBlankComparisonOperators);
		}

		public void TestClientContractNumber()
		{
			using (FreightConfigurationRegistry.Instance.EnableClientContractNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
				var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
				var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
				var shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();

				var job1 = new JobHeader.Loader(shipment1).TryLoadOrCreate();
				var job2 = new JobHeader.Loader(shipment2).TryLoadOrCreate();
				var job3 = new JobHeader.Loader(shipment3).TryLoadOrCreate();
				var job4 = new JobHeader.Loader(shipment4).TryLoadOrCreate();

				job1.JH_ClientContractNumber = "12345";
				job2.JH_ClientContractNumber = "12399";
				job3.JH_ClientContractNumber = "54321";
				job4.JH_ClientContractNumber = "54399";

				Factory.Save();

				var results = new ForwardingShipmentCollection(Factory);
				var filter = (ModuleNumberFilter)FilterStripBizO["Client Contract #"];
				filter.IsActive = true;

				filter.Property = "123";
				results.Load(FilterStripBizO.Filter);
				AssertContainsExactElementsInAnyOrder("Should only include shipments 1 and 2", new[] { shipment1, shipment2 }, results);

				filter.Property = "543";
				results.Load(FilterStripBizO.Filter);
				AssertContainsExactElementsInAnyOrder("Should only include shipments 3 and 4", new[] { shipment3, shipment4 }, results);
			}
		}

		public void TestClientContractNumber_NumberIsLoadedFromCompanyContext()
		{
			using (FreightConfigurationRegistry.Instance.EnableClientContractNumber.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

				var job = new JobHeader.Loader(shipment).TryLoadOrCreate();

				job.JH_ClientContractNumber = "12345";

				Factory.Save();

				var results = new ForwardingShipmentCollection(Factory);
				var filter = (ModuleNumberFilter)FilterStripBizO["Client Contract #"];
				filter.IsActive = true;

				filter.Property = "123";
				results.Load(FilterStripBizO.Filter);
				AssertContainsExactElementsInAnyOrder("Should load single shipment", new[] { shipment }, results);

				var newBranch = Factory.NewWithValidTestData<GlbBranch>();
				var department = Factory.NewWithValidTestData<GlbDepartment>();

				Factory.Save();

				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), newBranch.PK.ToGuid(), department.PK.ToGuid()))
				{
					var shipment_loaded = Factory.Load<ForwardingShipment>(shipment.PK);
					var otherJob = new JobHeader.Loader(shipment_loaded).TryLoadOrCreate();

					AssertNotEquals("Job on Shipment has changed due to different User Context", otherJob.PK, job.PK);

					otherJob.JH_ClientContractNumber = "321";

					results = new ForwardingShipmentCollection(Factory);
					filter = (ModuleNumberFilter)FilterStripBizO["Client Contract #"];
					filter.IsActive = true;
					filter.Property = "123";

					AssertContainsExactElementsInAnyOrder("Should not load up shipment due to different User Context", Array.Empty<ForwardingShipment>(), results);
				}
			}
		}

		#region HVLV Filters

		public void TestHVLVFilters()
		{
			var filterBizO = new JobShipmentFilterBusinessObject();

			AssertNotNull(filterBizO["HVLV Item ID"]);
			AssertNotNull(filterBizO["HVLV Consignment ID"]);
			AssertNotNull(filterBizO["HVLV Shipper Reference"]);
		}

		public void TestHVLVBlankOperatorSupport()
		{
			var filterBizO = new JobShipmentFilterBusinessObject();

			Assert("Item ID is mandatory, blank operators not needed", !((ModuleTextFilter)FilterStripBizO["HVLV Item ID"]).SupportsBlankComparisonOperators);
			Assert("Consignment ID is mandatory, blank operators not needed", !((ModuleTextFilter)FilterStripBizO["HVLV Consignment ID"]).SupportsBlankComparisonOperators);
			Assert("Shipper Reference is optional, blank operators applicable", ((ModuleTextFilter)FilterStripBizO["HVLV Shipper Reference"]).SupportsBlankComparisonOperators);
		}

		public void TestHVLVItemID()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			var bookingHeader1 = Factory.New<IHVLVBookingHeader>();
			((BusinessObject)bookingHeader1).FillWithValidTestData();
			var consignment1 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_HVH_BookingHeader = bookingHeader1.PK;
			var item1 = Factory.New<IHVLVItem>();
			item1.HVI_HVC_Consignment = consignment1.PK;
			item1.HVI_JS_LoadedOnShipment = shipment1.PK;

			var bookingHeader2 = Factory.New<IHVLVBookingHeader>();
			((BusinessObject)bookingHeader2).FillWithValidTestData();
			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment2.HVC_HVH_BookingHeader = bookingHeader2.PK;
			var item2 = Factory.New<IHVLVItem>();
			item2.HVI_HVC_Consignment = consignment2.PK;
			item2.HVI_JS_LoadedOnShipment = shipment2.PK;

			Factory.Save();

			item1.HVI_ItemId = "ITEM1";
			item2.HVI_ItemId = "ITEM2";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["HVLV Item ID"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "ITEM2";
			filter.IsActive = true;

			var results = new ForwardingShipmentCollection(Factory);
			results.Load(FilterStripBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { shipment2 }, results);
		}

		public void TestHVLVConsignmentID()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
				var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

				var bookingHeader1 = Factory.New<IHVLVBookingHeader>();
				((BusinessObject)bookingHeader1).FillWithValidTestData();
				var consignment1 = Factory.New<IHVLVConsignment>();
				consignment1.HVC_HVH_BookingHeader = bookingHeader1.PK;
				consignment1.HVC_WaybillNumber = "CONSIGN1";
				var item1 = Factory.New<IHVLVItem>();
				item1.HVI_HVC_Consignment = consignment1.PK;
				item1.HVI_JS_LoadedOnShipment = shipment1.PK;

				var bookingHeader2 = Factory.New<IHVLVBookingHeader>();
				((BusinessObject)bookingHeader2).FillWithValidTestData();
				var consignment2 = Factory.New<IHVLVConsignment>();
				consignment2.HVC_HVH_BookingHeader = bookingHeader2.PK;
				consignment2.HVC_WaybillNumber = "CONSIGN2";
				var item2 = Factory.New<IHVLVItem>();
				item2.HVI_HVC_Consignment = consignment2.PK;
				item2.HVI_JS_LoadedOnShipment = shipment2.PK;

				Factory.Save();

				consignment1.HVC_WaybillNumber = "WAYBILL1";
				consignment2.HVC_WaybillNumber = "WAYBILL2";

				Factory.Save();

				AssertEquals("Precondition: Consignment ID defaulted", "CONSIGN1", consignment1.HVC_ConsignmentId);
				AssertEquals("Precondition: Consignment ID defaulted", "CONSIGN2", consignment2.HVC_ConsignmentId);

				var filter = (ModuleTextFilter)FilterStripBizO["HVLV Consignment ID"];
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "CONSIGN2";
				filter.IsActive = true;

				var results = new ForwardingShipmentCollection(Factory);
				results.Load(FilterStripBizO.Filter);

				AssertContainsExactElementsInAnyOrder(new[] { shipment2 }, results);
			}
		}

		public void TestHVLVShipperReference()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			var bookingHeader1 = Factory.New<IHVLVBookingHeader>();
			((BusinessObject)bookingHeader1).FillWithValidTestData();
			var consignment1 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_HVH_BookingHeader = bookingHeader1.PK;
			consignment1.HVC_ShipperReference = "SHPREF1";
			var item1 = Factory.New<IHVLVItem>();
			item1.HVI_HVC_Consignment = consignment1.PK;
			item1.HVI_JS_LoadedOnShipment = shipment1.PK;
			item1.HVI_ItemId = "ITEM1";

			var bookingHeader2 = Factory.New<IHVLVBookingHeader>();
			((BusinessObject)bookingHeader2).FillWithValidTestData();
			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment2.HVC_HVH_BookingHeader = bookingHeader2.PK;
			consignment2.HVC_ShipperReference = "SHPREF2";
			var item2 = Factory.New<IHVLVItem>();
			item2.HVI_HVC_Consignment = consignment2.PK;
			item2.HVI_JS_LoadedOnShipment = shipment2.PK;
			item2.HVI_ItemId = "ITEM2";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["HVLV Shipper Reference"];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "SHPREF2";
			filter.IsActive = true;

			var results = new ForwardingShipmentCollection(Factory);
			results.Load(FilterStripBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { shipment2 }, results);
		}

		#endregion

		#endregion

		#region Date Filters

		#region Interim Receipt Date

		public void TestInterimReceiptDate()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			shipment1.JS_A_RCV = new ZDateTime(2005, 1, 1);
			shipment2.JS_A_RCV = new ZDateTime(2005, 1, 5);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["Interim Receipt Date"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = new ZDateTime(2005, 1, 2);
			filter.IsActive = true;
			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
			Assert("Shipment2 is not in Collection", !shipments.Contains(shipment2.PK));

			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = ZDateTime.Empty;
			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
			Assert("Shipment2 is in Collection", shipments.Contains(shipment2.PK));

			filter.Property1 = new ZDateTime(2005, 1, 2);
			filter.Property2 = ZDateTime.Empty;
			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			Assert("Shipment1 is not in Collection", !shipments.Contains(shipment1.PK));
			Assert("Shipment2 is in Collection", shipments.Contains(shipment2.PK));

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2005, 1, 2);
			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
			Assert("Shipment2 is not in Collection", !shipments.Contains(shipment2.PK));
		}

		#endregion

		#region Revised Delivery Due Date

		public void TestRevisedDeliveryDueDateAvailability()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var filter = GetNewFilterStripBusinessObject();
				AssertNotNull("Revised Delivery Due Date should be available when CalculateDeliveryDueDate registry is enabled", filter["Revised Delivery Due Date"]);
			}
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = false, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var filter = GetNewFilterStripBusinessObject();
				AssertNull("Revised Delivery Due Date should be available when CalculateDeliveryDueDate registry is disabled", filter["Revised Delivery Due Date"]);
			}
		}

		public void TestRevisedDeliveryDueDate()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var shipment1 = Factory.New<ForwardingShipment>();
				var shipment2 = Factory.New<ForwardingShipment>();

				shipment1.GetReasonForChangingDeliveryDueDateEventHandler += (sender, arg) => arg.Reason = "mock reason";
				shipment2.GetReasonForChangingDeliveryDueDateEventHandler += (sender, arg) => arg.Reason = "mock reason";

				shipment1.JS_RevisedDeliveryDueDate = new ZDateTimeOffset(2005, 1, 1);
				shipment2.JS_RevisedDeliveryDueDate = new ZDateTimeOffset(2005, 1, 5);

				Factory.Save();

				var filter = (ModuleDateFilter)FilterStripBizO["Revised Delivery Due Date"];
				AssertNotNull("Pre-condition:Revised Delivery Due Date filter exists", filter);
				filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				filter.Property1 = new ZDateTime(2005, 1, 1);
				filter.Property2 = new ZDateTime(2005, 1, 2);
				filter.IsActive = true;
				var shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
				shipments.Load();
				Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
				Assert("Shipment2 is not in Collection", !shipments.Contains(shipment2.PK));

				filter.Property1 = new ZDateTime(2005, 1, 1);
				filter.Property2 = ZDateTime.Empty;
				shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
				shipments.Load();
				Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
				Assert("Shipment2 is in Collection", shipments.Contains(shipment2.PK));

				filter.Property1 = new ZDateTime(2005, 1, 2);
				filter.Property2 = ZDateTime.Empty;
				shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
				shipments.Load();
				Assert("Shipment1 is not in Collection", !shipments.Contains(shipment1.PK));
				Assert("Shipment2 is in Collection", shipments.Contains(shipment2.PK));

				filter.Property1 = ZDateTime.Empty;
				filter.Property2 = new ZDateTime(2005, 1, 2);
				shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
				shipments.Load();
				Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
				Assert("Shipment2 is not in Collection", !shipments.Contains(shipment2.PK));
			}
		}

		#endregion

		#region Delivery Due Date Filter

		public void TestDeliveryDueDateAvailability()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var filter = GetNewFilterStripBusinessObject();
				AssertNotNull("Delivery Due Date should be available when CalculateDeliveryDueDate registry is enabled", filter["Delivery Due Date"]);
			}
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = false, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var filter = GetNewFilterStripBusinessObject();
				AssertNull("Delivery Due Date should be available when CalculateDeliveryDueDate registry is disabled", filter["Delivery Due Date"]);
			}
		}

		public void TestDeliveryDueDate()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var shipment1 = Factory.New<ForwardingShipment>();
				var shipment2 = Factory.New<ForwardingShipment>();

				shipment1.GetReasonForChangingDeliveryDueDateEventHandler += (sender, arg) => arg.Reason = "mock reason";
				shipment2.GetReasonForChangingDeliveryDueDateEventHandler += (sender, arg) => arg.Reason = "mock reason";

				shipment1.JS_DeliveryDueDate = new ZDateTime(2005, 1, 1);
				shipment2.JS_DeliveryDueDate = new ZDateTime(2005, 1, 5);

				Factory.Save();

				var filter = (ModuleDateFilter)FilterStripBizO["Delivery Due Date"];
				AssertNotNull("Pre-condition: Delivery Due Date filter exists", filter);
				filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				filter.Property1 = new ZDateTime(2005, 1, 1);
				filter.Property2 = new ZDateTime(2005, 1, 2);
				filter.IsActive = true;
				var shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
				shipments.Load();
				Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
				Assert("Shipment2 is not in Collection", !shipments.Contains(shipment2.PK));

				filter.Property1 = new ZDateTime(2005, 1, 1);
				filter.Property2 = ZDateTime.Empty;
				shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
				shipments.Load();
				Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
				Assert("Shipment2 is in Collection", shipments.Contains(shipment2.PK));

				filter.Property1 = new ZDateTime(2005, 1, 2);
				filter.Property2 = ZDateTime.Empty;
				shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
				shipments.Load();
				Assert("Shipment1 is not in Collection", !shipments.Contains(shipment1.PK));
				Assert("Shipment2 is in Collection", shipments.Contains(shipment2.PK));

				filter.Property1 = ZDateTime.Empty;
				filter.Property2 = new ZDateTime(2005, 1, 2);
				shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
				shipments.Load();
				Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
				Assert("Shipment2 is not in Collection", !shipments.Contains(shipment2.PK));
			}
		}

		#endregion

		#region TestFirstPortOfArrivalDate

		public void TestFirstPortOfArrivalDate()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			ForwardingConsol consol1 = shipment1.Consols.AddNew();
			ForwardingConsol consol2 = shipment2.Consols.AddNew();

			consol1.JK_DatePortOfFirstArrival = new ZDateTime(2005, 1, 1);
			consol2.JK_DatePortOfFirstArrival = new ZDateTime(2005, 1, 5);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["First Port of Arrival Date"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = new ZDateTime(2005, 1, 2);
			filter.IsActive = true;

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertEquals("Shipment1 is in Collection", true, shipments.Contains(shipment1.PK));
			AssertEquals("Shipment2 is not in Collection", false, shipments.Contains(shipment2.PK));
		}

		#endregion

		#region Est Delivery Date

		public void TestEstDeliveryDate()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			shipment1.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2005, 1, 1);
			shipment2.DocsAndCartage.JP_EstimatedDelivery = new ZDateTime(2005, 1, 5);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["Est Delivery Date"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = new ZDateTime(2005, 1, 2);
			filter.IsActive = true;
			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
			Assert("Shipment2 is not in Collection", !shipments.Contains(shipment2.PK));

			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = ZDateTime.Empty;
			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
			Assert("Shipment2 is in Collection", shipments.Contains(shipment2.PK));

			filter.Property1 = new ZDateTime(2005, 1, 2);
			filter.Property2 = ZDateTime.Empty;
			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			Assert("Shipment1 is not in Collection", !shipments.Contains(shipment1.PK));
			Assert("Shipment2 is in Collection", shipments.Contains(shipment2.PK));

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2005, 1, 2);
			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
			Assert("Shipment2 is not in Collection", !shipments.Contains(shipment2.PK));
		}

		#endregion

		#region Actual Delivery Date

		public void TestActualDeliveryDate()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			shipment1.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2005, 1, 1);
			shipment2.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2005, 1, 5);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["Actual Delivery Date"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = new ZDateTime(2005, 1, 2);
			filter.IsActive = true;
			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
			Assert("Shipment2 is not in Collection", !shipments.Contains(shipment2.PK));

			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = ZDateTime.Empty;
			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
			Assert("Shipment2 is in Collection", shipments.Contains(shipment2.PK));

			filter.Property1 = new ZDateTime(2005, 1, 2);
			filter.Property2 = ZDateTime.Empty;
			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			Assert("Shipment1 is not in Collection", !shipments.Contains(shipment1.PK));
			Assert("Shipment2 is in Collection", shipments.Contains(shipment2.PK));

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2005, 1, 2);
			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
			Assert("Shipment2 is not in Collection", !shipments.Contains(shipment2.PK));
		}

		#endregion

		#region TestCommodityCode

		public void TestCommodityCode()
		{
			RefCommodityCode commodityCode1 = Factory.New<RefCommodityCode>();
			commodityCode1.RH_Code = "COM1";
			RefCommodityCode commodityCode2 = Factory.New<RefCommodityCode>();
			commodityCode2.RH_Code = "COM2";

			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			ForwardingShipment shipment3 = Factory.New<ForwardingShipment>();
			ForwardingShipment shipment4 = Factory.New<ForwardingShipment>();

			shipment1.OuterPackLines.AddNew();
			shipment1.OuterPackLines[0].JL_RH_NKCommodityCode = commodityCode1.RH_Code;

			shipment2.OuterPackLines.AddNew();
			shipment2.OuterPackLines[0].JL_RH_NKCommodityCode = commodityCode2.RH_Code;

			BusinessObject jobDeclaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			jobDeclaration[JobDeclarationSchema.JE_JS] = shipment3.PK;
			BusinessObject invoice = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader)));
			invoice[JobComInvoiceHeaderSchema.JZ_JE] = jobDeclaration.PK;
			BusinessObject invoiceLine = (BusinessObject)((Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader)invoice).AddNewInvoiceLine();
			invoiceLine[JobComInvoiceLineSchema.JI_RH_NKCommodity_Code] = commodityCode1.RH_Code;

			jobDeclaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			jobDeclaration[JobDeclarationSchema.JE_JS] = shipment4.PK;
			invoice = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader)));
			invoice[JobComInvoiceHeaderSchema.JZ_JE] = jobDeclaration.PK;
			invoiceLine = (BusinessObject)((Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader)invoice).AddNewInvoiceLine();
			invoiceLine[JobComInvoiceLineSchema.JI_RH_NKCommodity_Code] = commodityCode2.RH_Code;

			Factory.Save();

			var filter = (ModuleNkFilter)FilterStripBizO["Commodity Code"];
			filter.Property = commodityCode1.RH_Code;
			filter.IsActive = true;
			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			AssertEquals(2, shipments.Count);
			Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
			Assert("Shipment2 is not in Collection", !shipments.Contains(shipment2.PK));
			Assert("Shipment3 is in Collection", shipments.Contains(shipment3.PK));
			Assert("Shipment4 is not in Collection", !shipments.Contains(shipment4.PK));
		}

		#endregion

		#region Est Pickup Date

		public void TestPickupDate()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			shipment1.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2005, 1, 1);
			shipment2.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2005, 1, 5);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["Est Pickup Date"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = new ZDateTime(2005, 1, 2);
			filter.IsActive = true;
			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
			Assert("Shipment2 is not in Collection", !shipments.Contains(shipment2.PK));

			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = ZDateTime.Empty;
			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
			Assert("Shipment2 is in Collection", shipments.Contains(shipment2.PK));

			filter.Property1 = new ZDateTime(2005, 1, 2);
			filter.Property2 = ZDateTime.Empty;
			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			Assert("Shipment1 is not in Collection", !shipments.Contains(shipment1.PK));
			Assert("Shipment2 is in Collection", shipments.Contains(shipment2.PK));

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2005, 1, 2);
			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
			Assert("Shipment2 is not in Collection", !shipments.Contains(shipment2.PK));
		}

		#endregion

		#region Actual Pickup Date

		public void TestActualPickupDate()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			shipment1.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2005, 1, 1);
			shipment2.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2005, 1, 5);

			Factory.Save();

			ModuleDateFilter filter = (ModuleDateFilter)FilterStripBizO["Actual Pickup Date"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = new ZDateTime(2005, 1, 2);
			filter.IsActive = true;
			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
			Assert("Shipment2 is not in Collection", !shipments.Contains(shipment2.PK));

			filter.Property1 = new ZDateTime(2005, 1, 1);
			filter.Property2 = ZDateTime.Empty;
			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
			Assert("Shipment2 is in Collection", shipments.Contains(shipment2.PK));

			filter.Property1 = new ZDateTime(2005, 1, 2);
			filter.Property2 = ZDateTime.Empty;
			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			Assert("Shipment1 is not in Collection", !shipments.Contains(shipment1.PK));
			Assert("Shipment2 is in Collection", shipments.Contains(shipment2.PK));

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2005, 1, 2);
			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			Assert("Shipment1 is in Collection", shipments.Contains(shipment1.PK));
			Assert("Shipment2 is not in Collection", !shipments.Contains(shipment2.PK));
		}

		#endregion

		#region TestCustomsEntryAuthorisationDate

		public void TestCustomsEntryAuthorisationDate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
				ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
				ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory);

				BusinessObject jobDeclaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));

				jobDeclaration[JobDeclarationSchema.JE_JS] = shipment1.PK;
				jobDeclaration[JobDeclarationSchema.JE_EntryAuthorisationDate] = ZDateTime.Now;
				JobShipmentFilterBusinessObject filterStripBizOSG = new JobShipmentFilterBusinessObject();
				Factory.Save();
				ModuleDateFilter filter = (ModuleDateFilter)filterStripBizOSG["Customs Entry Authorisation"];
				filter.IsActive = true;
				filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				filter.Property1 = ZDateTime.Now.AddDays(-1);
				filter.Property2 = ZDateTime.Now.AddDays(1);
				shipments.Load(filterStripBizOSG.Filter);
				AssertEquals("Should contain shipment1", true, shipments.Contains(shipment1));
				AssertEquals("Should not contain shipment2", false, shipments.Contains(shipment2));
				filter.IsActive = false;
				shipments.Load(filterStripBizOSG.Filter);
				AssertEquals("Should contain shipment2", true, shipments.Contains(shipment2));
				Assert("Should contain 2 shipments or more", shipments.Count > 1);
			}
		}

		#endregion

		#region House Bill Issue Date

		public void TestHouseBillIssueDateWhenNoHAWBExists()
		{
			var today = ZDateTime.Today;
			Shipment1.JS_TransportMode = Constants.TransportModes.Air;
			Shipment2.JS_TransportMode = Constants.TransportModes.Air;
			Shipment3.JS_TransportMode = Constants.TransportModes.Sea;
			Shipment4.JS_TransportMode = Constants.TransportModes.Sea;
			Shipment5.JS_TransportMode = Constants.TransportModes.Sea;
			Shipment6.JS_TransportMode = Constants.TransportModes.Rail;
			Shipment1.JS_HouseBillIssueDate = today;
			Shipment2.JS_HouseBillIssueDate = today.AddDays(2);
			Shipment3.JS_HouseBillIssueDate = today;
			Shipment4.JS_HouseBillIssueDate = today.AddMonths(3);
			Shipment5.JS_HouseBillIssueDate = ZDateTime.Empty;
			Shipment6.JS_HouseBillIssueDate = ZDateTime.Empty;

			Factory.Save();

			var filter = (ModuleDateFilter)FilterStripBizO["House Bill Issue Date"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;

			var results = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			results.Load(filter.Query);
			AssertContainsExactElementsInAnyOrder("Pre-condition: Should return all shipments when the filter has no dates entered", new[] { Shipment1, Shipment2, Shipment3, Shipment4, Shipment5, Shipment6 }, results);

			filter.Property1 = today;
			filter.Property2 = today.AddDays(6);
			results.Load(filter.Query);
			AssertContainsExactElementsInAnyOrder("Should only find shipments with a house bill issue date set between today and six days time", new[] { Shipment1, Shipment2, Shipment3 }, results);

			filter.Property1 = today.AddMonths(3);
			filter.Property2 = today.AddMonths(3).AddDays(1);
			results.Load(filter.Query);
			AssertContainsExactElementsInAnyOrder("Should only find shipments with a house bill issue date set between three months and three months and a day from today", new[] { Shipment4 }, results);

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			results.Load(filter.Query);
			AssertContainsExactElementsInAnyOrder("Should only find shipments with a house bill issue date set", new[] { Shipment1, Shipment2, Shipment3, Shipment4 }, results);

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			results.Load(filter.Query);
			AssertContainsExactElementsInAnyOrder("Should only find shipments without a house bill issue date set", new[] { Shipment5, Shipment6 }, results);
		}

		public void TestHouseBillIssueDateWhenHAWBExists()
		{
			var today = ZDateTime.Today;
			Shipment1.JS_TransportMode = Constants.TransportModes.Air;
			Shipment2.JS_TransportMode = Constants.TransportModes.Air;
			Shipment3.JS_TransportMode = Constants.TransportModes.Air;
			Shipment4.JS_TransportMode = Constants.TransportModes.Air;
			Shipment5.JS_TransportMode = Constants.TransportModes.Air;
			Shipment6.JS_TransportMode = Constants.TransportModes.Air;
			Shipment1.JS_HouseBillIssueDate = today;
			Shipment2.JS_HouseBillIssueDate = today.AddDays(2);
			Shipment3.JS_HouseBillIssueDate = today.AddDays(5);

			Shipment3.PopulateAWB();
			Shipment4.PopulateAWB();
			Shipment5.PopulateAWB();
			Shipment3.IsAWBValuesOverriddenProperty = true;
			Shipment4.IsAWBValuesOverriddenProperty = true;
			Shipment5.IsAWBValuesOverriddenProperty = true;
			Shipment3.AWBHeader.EH_AWBIssueDate = today;
			Shipment4.AWBHeader.EH_AWBIssueDate = today;
			Shipment5.AWBHeader.EH_AWBIssueDate = today;

			Factory.Save();

			AssertEquals("Precondition: hawb issue date should be set to today", today, Shipment4.AWBHeader.EH_AWBIssueDate);
			AssertEquals("Precondition: hawb issue date should be set to today", today, Shipment5.AWBHeader.EH_AWBIssueDate);

			var filter = (ModuleDateFilter)FilterStripBizO["House Bill Issue Date"];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;

			var results = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			results.Load(filter.Query);
			AssertContainsExactElementsInAnyOrder("Pre-condition: Should return all shipments when the filter has no dates entered", new[] { Shipment1, Shipment2, Shipment3, Shipment4, Shipment5, Shipment6 }, results);

			filter.Property1 = today;
			filter.Property2 = today.AddDays(6);
			results.Load(filter.Query);
			AssertContainsExactElementsInAnyOrder("Should find shipments with an issue date on the shipment or HAWB set between today and six days time", new[] { Shipment1, Shipment2, Shipment3, Shipment4, Shipment5 }, results);

			filter.Property2 = today;
			results.Load(filter.Query);
			AssertCollectionNotContains("Shipment3 should not be in the results as issue date on the shipment takes precedence over executed date on the HAWB", new[] { Shipment3 }, results);
			AssertContainsExactElementsInAnyOrder("Should find shipments with an issue date on the shipment or HAWB set today", new[] { Shipment1, Shipment4, Shipment5 }, results);

			Shipment3.JS_HouseBillIssueDate = today.AddMonths(4);
			Factory.Save();

			filter.Property1 = today.AddMonths(4);
			filter.Property2 = today.AddMonths(4);
			results.Load(filter.Query);
			AssertContainsExactElementsInAnyOrder("Should find all shipments with an issue date on the shipment or HAWB set four months from today", new[] { Shipment3 }, results);

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			results.Load(filter.Query);
			AssertContainsExactElementsInAnyOrder("Should only find shipments with an issue date set or has a HAWB populated", new[] { Shipment1, Shipment2, Shipment3, Shipment4, Shipment5 }, results);

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			results.Load(filter.Query);
			AssertContainsExactElementsInAnyOrder("Should only find shipments without an issue date set on the shipment and no HAWB populated", new[] { Shipment6 }, results);

			Shipment5.AWBHeader.EH_AWBIssueDate = ZDateTime.Empty;
			Factory.Save();

			results.Load(filter.Query);
			AssertContainsExactElementsInAnyOrder("Should only find shipments without an issue date set on the shipment and no HAWB populated", new[] { Shipment5, Shipment6 }, results);

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			results.Load(filter.Query);
			AssertContainsExactElementsInAnyOrder("Should only find shipments with an issue date set on the shipment or has a HAWB populated", new[] { Shipment1, Shipment2, Shipment3, Shipment4 }, results);
		}

		#endregion

		#endregion

		#region TestDateOrganizationFilter

		public void TestCFS_ReceiptRequested_Import()
		{
			TestDateOrganizationFilter(JobShipmentFilterBusinessObject.Descriptions.PickupCFSReceiptRequested);
		}

		public void TestCFS_ReceiptRequested_Export()
		{
			TestDateOrganizationFilter(JobShipmentFilterBusinessObject.Descriptions.DeliveryCFSReceiptRequested);
		}

		public void TestCFS_DispatchRequested_Import()
		{
			TestDateOrganizationFilter(JobShipmentFilterBusinessObject.Descriptions.PickupCFSDispatchRequested);
		}

		public void TestCFS_DispatchRequested_Export()
		{
			TestDateOrganizationFilter(JobShipmentFilterBusinessObject.Descriptions.DeliveryCFSDispatchRequested);
		}

		public void TestDateOrganizationFilter(ZString filterStripDescription)
		{
			var filterToColumnMappings = new Dictionary<ZString, (SchemaGuidColumn, SchemaDateTimeColumn)>()
			{
				{
					JobShipmentFilterBusinessObject.Descriptions.PickupCFSReceiptRequested,
					(JobShipmentSchema.JS_OA_ExportReceivingDepot, JobShipmentSchema.JS_ExportReceivingDepotReceiptRequested)
				},
				{
					JobShipmentFilterBusinessObject.Descriptions.DeliveryCFSReceiptRequested,
					(JobShipmentSchema.JS_OA_ImportReleaseDepot, JobShipmentSchema.JS_ImportReleaseDepotReceiptRequested)
				},
				{
					JobShipmentFilterBusinessObject.Descriptions.PickupCFSDispatchRequested,
					(JobShipmentSchema.JS_OA_ExportReceivingDepot, JobShipmentSchema.JS_ExportReceivingDepotDispatchRequested)
				},
				{
					JobShipmentFilterBusinessObject.Descriptions.DeliveryCFSDispatchRequested,
					(JobShipmentSchema.JS_OA_ImportReleaseDepot, JobShipmentSchema.JS_ImportReleaseDepotDispatchRequested)
				}
			};

			var today = ZDateTime.Today;

			var cfs = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = cfs.Addresses.AddNew();
			address1.Address1 = "Some depot address";

			var someOtherCFS = Factory.NewWithValidTestData<OrgHeader>();
			var someOtherAddress = someOtherCFS.Addresses.AddNew();
			someOtherAddress.Address1 = "Some other depot address";

			var (targetDepot, targetDate) = filterToColumnMappings[filterStripDescription];

			Shipment1[targetDepot] = address1.PK;
			Shipment1[targetDate] = today.AddDays(-2);

			var shouldNotBeInResults = new List<CommonShipment>() { Shipment2, Shipment3, Shipment4 };
			var shipmentCounter = 0;

			foreach (var (depot, date) in filterToColumnMappings.Where(mapping => mapping.Key != filterStripDescription).Select(mapping => mapping.Value))
			{
				var shipment = shouldNotBeInResults[shipmentCounter];
				shipment[depot] = address1.PK;
				shipment[date] = today.AddDays(-2);

				shipmentCounter++;
			}

			Factory.Save();

			var filter = FilterStripBizO[filterStripDescription] as DateOrganizationFilter;
			filter.IsActive = true;

			var results = new ShipmentCollection(Factory);
			results.Load(FilterStripBizO.Filter);

			AssertContainsExactElementsInAnyOrder(
				"Pre-condition: Search is not filtered - should return all results",
				new[] { Shipment1, Shipment2, Shipment3, Shipment4, Shipment5, Shipment6 },
				results
			);

			filter.OrganizationPK = someOtherCFS.PK;
			results.Load(filter.Query);

			AssertContainsExactElementsInAnyOrder("No shipments should be returned: wrong cfs", new ShipmentCollection(Factory), results);

			filter.OrganizationPK = cfs.PK;
			filter.Property1 = today.AddDays(-2);
			filter.Property2 = today.AddDays(-2);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			results.Load(filter.Query);

			AssertCollectionContains("Should appear: matches CFS and date", Shipment1, results);
			AssertCollectionNotContains("Should not appear: does not match specific CFS", shouldNotBeInResults, results);
		}

		#endregion

		#region TestDateLocationFilter

		public void TestLoadPort_ETA()
		{
			TestDateLocationFilterCorrectlyHandlesLinkedOnlyDates(Transport.Schema.JW_JX_Load_ETA, "ETA / Load Port");
		}

		public void TestLoadPort_ATA()
		{
			TestDateLocationFilterCorrectlyHandlesLinkedOnlyDates(Transport.Schema.JW_JX_Load_ATA, "ATA / Load Port");
		}

		public void TestLoadPort_ETD()
		{
			TestDateLocationFilter(JobConsolTransportSchema.JW_ETD.Name, JobConsolTransportSchema.JW_RL_NKLoadPort.Name, SailingFilterBuilder.Dates.ETD);
		}

		public void TestLoadPort_ATD()
		{
			TestDateLocationFilter(JobConsolTransportSchema.JW_ATD.Name, JobConsolTransportSchema.JW_RL_NKLoadPort.Name, SailingFilterBuilder.Dates.ATD);
		}

		public void TestDischargePort_ETA()
		{
			TestDateLocationFilter(JobConsolTransportSchema.JW_ETA.Name, JobConsolTransportSchema.JW_RL_NKDiscPort.Name, SailingFilterBuilder.Dates.ETA);
		}

		public void TestDischargePort_ATA()
		{
			TestDateLocationFilter(JobConsolTransportSchema.JW_ATA.Name, JobConsolTransportSchema.JW_RL_NKDiscPort.Name, SailingFilterBuilder.Dates.ATA);
		}

		public void TestDateLocationFilter(ZString dateProperty, ZString locationProperty, SailingFilterBuilder.Dates dateEnum)
		{
			var today = ZDateTime.Today;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var transport1 = consol.Transports[0];
			transport1.JW_IsLinked = true;
			transport1[dateProperty] = today.AddDays(3);
			transport1[locationProperty] = "AUSYD";

			Shipment1.Consols.Add(consol);

			var transport2 = Shipment2.Transports.AddNew();
			transport2.JW_IsLinked = true;
			transport2[dateProperty] = today.AddDays(5);
			transport2[locationProperty] = "GBLON";

			var transport3 = Shipment3.Transports.AddNew();
			transport3.JW_IsLinked = true;
			transport3[dateProperty] = today.AddDays(1);
			transport3[locationProperty] = "AUMEL";

			Factory.Save();

			var direction = locationProperty == "JW_RL_NKDiscPort"
					? DateLocationFilter.LocationTypes.Discharge
					: DateLocationFilter.LocationTypes.Load;

			var filter = new DateLocationFilter("Test", dateEnum, BindToLists.GetCachedLists(Factory).RefLocation_List, direction, DateLocationFilter.TargetFilterTypes.Shipment, Factory);
			var results = new ShipmentCollection(Factory);
			results.Load(filter.Query);

			AssertContainsExactElementsInAnyOrder("Pre-condition: Search is not filtered - should return all results", new[] { Shipment1, Shipment2, Shipment3, Shipment4, Shipment5, Shipment6 }, results);

			filter.Property1 = today.AddDays(1);
			filter.Property2 = today.AddDays(2);
			filter.Property3 = "ITROM";
			results.Load(filter.Query);

			var emptyCollection = new ShipmentCollection(Factory);
			AssertContainsExactElementsInAnyOrder("No shipments should be in the collection", emptyCollection, results);

			filter.Property1 = today;
			filter.Property2 = today.AddDays(6);
			filter.Property3 = "AU";
			results.Load(filter.Query);

			AssertCollectionContains("Should appear - has matching transport via the consol", Shipment1, results);
			AssertCollectionContains("Should appear - has matching transport", Shipment3, results);
			AssertCollectionNotContains("Should NOT appear as shipment has no transport in Australia this week", new[] { Shipment2, Shipment4, Shipment5 }, results);
		}

		public void TestDateLocationFilterCorrectlyHandlesLinkedOnlyDates(ZString dateProperty, ZString filterProperty)
		{
			var today = ZDateTime.Today;

			var voyage1 = Factory.New<JobVoyage>();
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage1.Origins[0].JA_A_ARV = today;
			voyage1.Origins[0].JA_E_ARV = today;
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "HKHKG";
			voyage1.GenerateSailings();

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage2.GenerateSailings();

			var voyage3 = Factory.New<JobVoyage>();
			voyage3.Origins.AddNew().JA_RL_NKPortOfLoading = "GBLON";
			voyage3.Origins[0].JA_A_ARV = today;
			voyage3.Origins[0].JA_E_ARV = today;
			voyage3.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage3.GenerateSailings();

			Consol1.Shipments.RemoveAll();  //must detach consol from all shipments as it will effect the 'has no date' test
			Shipment1.Transports.AddNew();
			Shipment1.Transports[0].JW_IsLinked = true;
			Shipment1.Transports[0].JW_JX = voyage1.Sailings[0].PK;
			Shipment1.Transports[0][dateProperty] = today;

			Consol2.Transports[0].JW_IsLinked = true;
			Consol2.Transports[0].JW_JX = voyage1.Sailings[0].PK;
			Consol2.Transports[0][dateProperty] = today;

			Shipment3.Transports.AddNew();
			Shipment3.Transports[0].JW_IsLinked = true;
			Shipment3.Transports[0].JW_JX = voyage2.Sailings[0].PK;

			Shipment4.Transports.AddNew();
			Shipment4.Transports[0].JW_IsLinked = true;
			Shipment4.Transports[0].JW_JX = voyage3.Sailings[0].PK;
			Shipment4.Transports[0][dateProperty] = today;

			Shipment5.Transports.AddNew();
			Shipment5.Transports[0].JW_IsLinked = true;
			Shipment5.Transports[0].JW_JX = voyage1.Sailings[0].PK;
			Shipment5.Transports[0][dateProperty] = today;

			Factory.Save();

			Shipment5.Transports[0].JW_IsLinked = false;
			Shipment5.Transports[0][dateProperty] = ZDateTime.Empty;

			Factory.Save();

			var filter = (DateLocationFilter)FilterStripBizO[filterProperty];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			filter.Property3 = ZString.Empty;

			var results = new ShipmentCollection(Factory);

			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Pre-condition: Should return all shipments when filter is blank", new[] { Shipment1, Shipment2, Shipment3, Shipment4, Shipment5, Shipment6 }, results);

			filter.Property3 = "AUMEL";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Should return 4 shipments that are loaded from Melbourne", new[] { Shipment1, Shipment2, Shipment3, Shipment5 }, results);

			filter.Property1 = today.AddDays(-1);
			filter.Property2 = today.AddDays(1);
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Should return 2 shipments both still linked to voyage 1", new[] { Shipment1, Shipment2 }, results);

			filter.Property3 = ZString.Empty;
			results.Load(filter.Query);
			AssertContainsExactElementsInAnyOrder("Should return 3 matching shipments where the transports are linked, regardless of load port", new[] { Shipment1, Shipment2, Shipment4 }, results);

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Should return 3 shipments with no date or transports attached", new[] { Shipment3, Shipment5, Shipment6 }, results);

			filter.Property3 = "AUMEL";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Should return 2 shipments with no date and loads in Melbourne", new[] { Shipment3, Shipment5 }, results);

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Should return 2 shipment with an ETA and from the same port", new[] { Shipment1, Shipment2 }, results);

			filter.Property3 = ZString.Empty;
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Should return 3 shipments with an ETA entered", new[] { Shipment1, Shipment2, Shipment4 }, results);
		}

		#endregion

		#region Text Filters
		public void TestFDAMsgStatus()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();
			var shipment3 = Factory.New<ForwardingShipment>();

			var usDecType = ObjectFactory.GetType<Enterprise.Integration.Customs.US.IJobDeclaration>();
			var dec1 = Factory.New(usDecType);
			dec1[JobDeclarationSchema.JE_JS] = shipment1.PK;
			dec1["FDAMsgStatus"] = "ACC";
			dec1[JobDeclarationSchema.JE_GB] = GlbBranch.CurrentBranch.PK;

			var dec2 = Factory.New(usDecType);
			dec2[JobDeclarationSchema.JE_JS] = shipment2.PK;
			dec2["FDAMsgStatus"] = "ACP";
			dec2[JobDeclarationSchema.JE_GB] = GlbBranch.CurrentBranch.PK;

			Factory.Save();

			var filter = new JobShipmentFilterBusinessObject();
			var filterModule = (ModuleTextFilter)filter.ModuleFilters[filter.USDeclarationFilter.FDAMsgStatusDescription];
			filterModule.Property = "ACC";
			filterModule.IsActive = true;

			Assert("shipment1 matches", shipment1.MatchesFilter(filter.Filter));
			Assert("shipment2 does not match", !shipment2.MatchesFilter(filter.Filter));
			Assert("shipment3 does not match", !shipment3.MatchesFilter(filter.Filter));
		}

		public void TestFDAStatus()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();
			var shipment3 = Factory.New<ForwardingShipment>();

			var usDecType = ObjectFactory.GetType<Enterprise.Integration.Customs.US.IJobDeclaration>();
			var dec1 = Factory.New(usDecType);
			dec1[JobDeclarationSchema.JE_JS] = shipment1.PK;
			dec1["FDAStatus"] = "01";
			dec1[JobDeclarationSchema.JE_GB] = GlbBranch.CurrentBranch.PK;

			var dec2 = Factory.New(usDecType);
			dec2[JobDeclarationSchema.JE_JS] = shipment2.PK;
			dec2["FDAStatus"] = "02";
			dec2[JobDeclarationSchema.JE_GB] = GlbBranch.CurrentBranch.PK;

			Factory.Save();

			var filter = new JobShipmentFilterBusinessObject();
			var filterModule = (ModuleTextFilter)filter.ModuleFilters[filter.USDeclarationFilter.FDAStatusDescription];
			filterModule.Property = "01";
			filterModule.IsActive = true;

			Assert("shipment1 matches", shipment1.MatchesFilter(filter.Filter));
			Assert("shipment2 does not match", !shipment2.MatchesFilter(filter.Filter));
			Assert("shipment3 does not match", !shipment3.MatchesFilter(filter.Filter));
		}

		public void TestInvoiceStatusFilter()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();

			JobHeader job1 = new JobHeader.Loader(shipment1).TryLoadOrCreate();
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job1.JH_Status = "WRK";

			JobHeader job2 = new JobHeader.Loader(shipment2).TryLoadOrCreate();
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job2.JH_Status = "INV";

			JobHeader job3 = new JobHeader.Loader(shipment3).TryLoadOrCreate();
			job3.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job3.JH_Status = "INV";

			JobHeader job4 = new JobHeader.Loader(shipment4).TryLoadOrCreate();
			job4.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job4.JH_Status = "WRK";

			Factory.Save();

			var results = new ForwardingShipmentCollection(Factory);
			var filter = (ModuleTextFilter)FilterStripBizO["Invoice Status"];
			filter.IsActive = true;

			filter.Property = "WRK";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Shipments with WRK Invoice Status are filtered", new[] { shipment1, shipment4 }, results);

			filter.Property = "INV";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Shipments with INV Invoice Status are filtered", new[] { shipment2, shipment3 }, results);
		}

		void SetDeclaration(BusinessObject obj, ForwardingShipment shipment, string status)
		{
			obj[JobDeclarationSchema.JE_JS] = shipment.PK;
			obj["ReleaseStatus"] = status;
			obj[JobDeclarationSchema.JE_GB] = GlbBranch.CurrentBranch.PK;
		}

		public void TestReleaseStatusFilter()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();
			var shipment3 = Factory.New<ForwardingShipment>();
			var shipment4 = Factory.New<ForwardingShipment>();
			var shipment5 = Factory.New<ForwardingShipment>();
			var shipment6 = Factory.New<ForwardingShipment>();

			var usDecType = ObjectFactory.GetType<Enterprise.Integration.Customs.US.IJobDeclaration>();
			var dec1 = Factory.New(usDecType);
			SetDeclaration(dec1, shipment1, "REL");
			var dec2 = Factory.New(usDecType);
			SetDeclaration(dec2, shipment2, "EXM");
			var dec3 = Factory.New(usDecType);
			SetDeclaration(dec3, shipment3, "NRL");
			var dec4 = Factory.New(usDecType);
			SetDeclaration(dec4, shipment4, "HLD");
			var dec5 = Factory.New(usDecType);
			SetDeclaration(dec5, shipment5, "DEL");
			var dec6 = Factory.New(usDecType);
			SetDeclaration(dec6, shipment6, "CAN");

			Factory.Save();

			var filter = new JobShipmentFilterBusinessObject();
			var filterModule = (ModuleTextFilter)filter.ModuleFilters[filter.USDeclarationFilter.ReleaseStatusDescription];
			filterModule.IsActive = true;

			filterModule.Property = "REL";
			Assert("shipment1", shipment1.MatchesFilter(filter.Filter));
			Assert("shipment2", !shipment2.MatchesFilter(filter.Filter));
			Assert("shipment3", !shipment3.MatchesFilter(filter.Filter));
			Assert("shipment4", !shipment4.MatchesFilter(filter.Filter));
			Assert("shipment5", !shipment5.MatchesFilter(filter.Filter));
			Assert("shipment6", !shipment6.MatchesFilter(filter.Filter));
			filterModule.Property = "EXM";
			Assert("shipment1", !shipment1.MatchesFilter(filter.Filter));
			Assert("shipment2", shipment2.MatchesFilter(filter.Filter));
			Assert("shipment3", !shipment3.MatchesFilter(filter.Filter));
			Assert("shipment4", !shipment4.MatchesFilter(filter.Filter));
			Assert("shipment5", !shipment5.MatchesFilter(filter.Filter));
			Assert("shipment6", !shipment6.MatchesFilter(filter.Filter));
			filterModule.Property = "NRL";
			Assert("shipment1", !shipment1.MatchesFilter(filter.Filter));
			Assert("shipment2", !shipment2.MatchesFilter(filter.Filter));
			Assert("shipment3", shipment3.MatchesFilter(filter.Filter));
			Assert("shipment4", !shipment4.MatchesFilter(filter.Filter));
			Assert("shipment5", !shipment5.MatchesFilter(filter.Filter));
			Assert("shipment6", !shipment6.MatchesFilter(filter.Filter));
			filterModule.Property = "HLD";
			Assert("shipment1", !shipment1.MatchesFilter(filter.Filter));
			Assert("shipment2", !shipment2.MatchesFilter(filter.Filter));
			Assert("shipment3", !shipment3.MatchesFilter(filter.Filter));
			Assert("shipment4", shipment4.MatchesFilter(filter.Filter));
			Assert("shipment5", !shipment5.MatchesFilter(filter.Filter));
			Assert("shipment6", !shipment6.MatchesFilter(filter.Filter));
			filterModule.Property = "DEL";
			Assert("shipment1", !shipment1.MatchesFilter(filter.Filter));
			Assert("shipment2", !shipment2.MatchesFilter(filter.Filter));
			Assert("shipment3", !shipment3.MatchesFilter(filter.Filter));
			Assert("shipment4", !shipment4.MatchesFilter(filter.Filter));
			Assert("shipment5", shipment5.MatchesFilter(filter.Filter));
			Assert("shipment6", !shipment6.MatchesFilter(filter.Filter));
			filterModule.Property = "CAN";
			Assert("shipment1", !shipment1.MatchesFilter(filter.Filter));
			Assert("shipment2", !shipment2.MatchesFilter(filter.Filter));
			Assert("shipment3", !shipment3.MatchesFilter(filter.Filter));
			Assert("shipment4", !shipment4.MatchesFilter(filter.Filter));
			Assert("shipment5", !shipment5.MatchesFilter(filter.Filter));
			Assert("shipment6", shipment6.MatchesFilter(filter.Filter));
		}

		public void TestReleaseStatusFilterOptions()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var shipment1 = Factory.New<ForwardingShipment>();

			var usDecType = ObjectFactory.GetType<Enterprise.Integration.Customs.US.IJobDeclaration>();
			var dec1 = Factory.New(usDecType);
			SetDeclaration(dec1, shipment1, "REL");

			Factory.Save();

			var filter = new JobShipmentFilterBusinessObject();
			var cargoReleaseStatusFilter = (ModuleTextFilter)filter.ModuleFilters[filter.USDeclarationFilter.ReleaseStatusDescription];
			cargoReleaseStatusFilter.IsActive = true;
			cargoReleaseStatusFilter.Property = "";
			AssertEquals("cargoReleaseStatus Filter list should have 3 options", 3, cargoReleaseStatusFilter.ComparisonOperator_List.Count);
			AssertEquals("cargoReleaseStatus Filter list should have 'exact' as default", ModuleTextFilter.ComparisonConstants.Exact, cargoReleaseStatusFilter.ComparisonOperator);
			AssertEquals("cargoReleaseStatus Filter list should have 'starts with' as another option", true, cargoReleaseStatusFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.StartsWith));
			AssertEquals("cargoReleaseStatus Filter list should have 'not equal' as another option", true, cargoReleaseStatusFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotEqual));
		}

		public void TestCustomsEntryStatusFilter_CurrentCompanyParameter()
		{
			var filter = GetNewFilterStripBusinessObject();
			var customsEntryStatusFilterModule = filter["Customs Entry Status"] as EntryStatusFilter;

			customsEntryStatusFilterModule.Property = JobShipmentFilterBusinessObject.FilterStatus.NotSentCustomsStatusForFilter;
			customsEntryStatusFilterModule.IsActive = true;

			var invoicedFilter = filter["Invoiced / Charges"] as ModuleFlagsFilter;

			invoicedFilter.Property0 = true;
			invoicedFilter.IsActive = true;

			AssertNoExceptionThrown(() =>
			{
				var collection = new ForwardingShipmentCollection(Factory);
				collection.Load(filter.Filter);
			});
		}

		#region TestSendingarnumer

		public void TestSendingarnumer()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Iceland);
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "ISKEF";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_CRN = "F-098-2208-8-IS-KEF-8881-P";

			ForwardingShipment shipment = consol.Shipments.AddNew();
			CusEntryNumber cusEntryNum = shipment.CusEntryNumbers.AddNew();
			cusEntryNum.CE_EntryIsSystemGenerated = false;
			cusEntryNum.CE_ParentID = shipment.PK;
			cusEntryNum.CE_ParentTable = JobShipmentSchema.Constants.TableName;
			cusEntryNum.CE_EntryType = CusEntryNumberTypes.Iceland.CRN;
			cusEntryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			ShipmentSendingarnumerGenerator generator = new ShipmentSendingarnumerGenerator(consol);
			generator.GenerateSendingarnumers();

			Factory.Save();

			string sendingarnumer = shipment.CustomsEntryNumber;

			JobShipmentFilterBusinessObject filter = (JobShipmentFilterBusinessObject)GetNewFilterStripBusinessObject();
			((ModuleTextFilter)filter["Sendingarnumer"]).Property = sendingarnumer;
			((ModuleTextFilter)filter["Sendingarnumer"]).IsActive = true;

			ForwardingShipmentCollection collection = new ForwardingShipmentCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);
			AssertEquals(shipment.PK, collection[0].PK);
		}

		#endregion

		#region Invoice Line Product Code

		public void TestInvoiceLineProductCode()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			BusinessObject declaration1 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			declaration1[JobDeclarationSchema.Constants.JE_JS] = shipment1.PK;
			BusinessObject comInvoiceHeader1 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader)));
			comInvoiceHeader1[JobComInvoiceHeaderSchema.Constants.JZ_JE] = declaration1.PK;
			BusinessObject comInvoiceLine1 = (BusinessObject)((Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader)comInvoiceHeader1).AddNewInvoiceLine();
			comInvoiceLine1[JobComInvoiceLineSchema.Constants.JI_PartNo] = "blah";

			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			BusinessObject declaration2 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			declaration2[JobDeclarationSchema.Constants.JE_JS] = shipment2.PK;
			BusinessObject comInvoiceHeader2 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader)));
			comInvoiceHeader2[JobComInvoiceHeaderSchema.Constants.JZ_JE] = declaration2.PK;
			BusinessObject comInvoiceLine2 = (BusinessObject)((Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader)comInvoiceHeader2).AddNewInvoiceLine();
			comInvoiceLine2[JobComInvoiceLineSchema.Constants.JI_PartNo] = "halb";

			Factory.Save();

			BusinessObject[] collection = Factory.Load(typeof(ForwardingShipment), FilterStripBizO.Filter);
			int unfilteredLength = collection.Length;
			Assert("Records return", collection.Length > 0);

			string invoiceLineProductCode = "Invoice Line Product Code";
			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[invoiceLineProductCode];
			filter.Property = "zz";
			filter.IsActive = true;
			collection = Factory.Load(typeof(ForwardingShipment), FilterStripBizO.Filter);
			Assert("No records return", collection.Length == 0);

			filter = (ModuleTextFilter)FilterStripBizO[invoiceLineProductCode];
			filter.Property = "bl";
			filter.IsActive = false;
			collection = Factory.Load(typeof(ForwardingShipment), FilterStripBizO.Filter);
			Assert("Records return", collection.Length == unfilteredLength);

			filter.Property = "bl";
			filter.IsActive = true;
			collection = Factory.Load(typeof(ForwardingShipment), FilterStripBizO.Filter);
			AssertEquals(1, collection.Length);
			AssertEquals(shipment1, collection[0]);
		}

		#endregion

		#region Order Line Product Code

		public void TestOrderLineProductCode()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			Order order1 = shipment1.AttachedOrders.AddNew();
			OrderLine orderLine1 = order1.OrderLines.AddNew();
			OrderLine orderLine2 = order1.OrderLines.AddNew();
			order1.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			orderLine1.JO_Partno = "34.53.32";
			orderLine2.JO_Partno = "111111";

			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			Order order2 = shipment2.AttachedOrders.AddNew();
			OrderLine orderLine3 = order2.OrderLines.AddNew();
			OrderLine orderLine4 = order2.OrderLines.AddNew();
			order2.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			orderLine3.JO_Partno = "xxxxx";
			orderLine4.JO_Partno = "zzzzz";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Order Line Product Code"];
			filter.Property = "34.5";
			filter.IsActive = true;

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertEquals(1, shipments.Count);
			AssertEquals(shipment1, shipments[0]);
		}

		#endregion

		#region IsBlank And IsNotBlank Filters

		public void TestIsBlankFilter()
		{
			Factory.Save();

			var listAllShipments = new List<ForwardingShipment>(new ForwardingShipment[] { Shipment1, Shipment2, Shipment3, Shipment4, Shipment5 });

			// precondition
			AssertContainsExactElementsInAnyOrder("Precondition failed",
				listAllShipments.FindAll((x) => x.Consols.Count == 0).ToArray(),
				new ForwardingShipment[] { Shipment5 });

			ModuleFountainFilter filter = (ModuleFountainFilter)FilterStripBizO["Consol #"];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

			ForwardingShipmentCollection collection = new ForwardingShipmentCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			AssertCollectionNotContains(Shipment1, collection);
			AssertCollectionNotContains(Shipment2, collection);
			AssertCollectionNotContains(Shipment3, collection);
			AssertCollectionNotContains(Shipment4, collection);
			AssertCollectionContains(Shipment5, collection);
		}

		#endregion

		#region TestMasterBill

		public void TestMasterBillNumberFilter()
		{
			Consol1.JK_MasterBillNum = "ABC";
			Consol2.JK_MasterBillNum = "123";
			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStripBizO["Master Bill"];
			filter.IsActive = true;
			filter.Property = "123";

			ForwardingShipmentCollection collection = new ForwardingShipmentCollection(Factory);
			collection.Load(FilterStripBizO.Filter);
			AssertCollectionNotContains(Shipment1, collection);
			AssertCollectionContains(Shipment2, collection);
		}

		#endregion

		#region TestPodFilter

		public void TestPodFilter()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment5 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment5.JS_PackingMode = Constants.ContainerModes.FCL;
			ForwardingConsol consol = shipment4.Consols.AddNew();
			ForwardingContainer container = consol.Containers.AddNew();

			shipment1.OuterPackLines.AddNew();
			shipment2.OuterPackLines.AddNew();
			ForwardingPackLine shipment5PackLine = shipment5.OuterPackLines.AddNew();

			consol.Shipments.Add(shipment5);
			shipment5PackLine.SetContainer(consol, container);

			CommonPickupDeliveryConfirm forwardingContainerLeg1 = shipment1.DeliveryConfirms.AddNew();
			forwardingContainerLeg1.EU_PickupDeliveryTime = ZDateTime.Today;
			forwardingContainerLeg1.EU_GoodsSignForBy = "Someone";

			shipment1.OuterPackLines.AddNew();

			shipment1.InnerPackLines.AddNew();

			CommonPickupDeliveryConfirm forwardingContainerLeg2 = shipment2.DeliveryConfirms.AddNew();

			AssertEquals("shipment3 should not have any legs", 0, shipment3.DeliveryConfirms.Count);

			container.DestinationConfirm.EU_PickupDeliveryTime = ZDateTime.Today;
			container.DestinationConfirm.EU_GoodsSignForBy = "Someone";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Proof-of-Delivery (POD) information"];
			filter.Property = JobShipmentFilterBusinessObject.PodFilterItems.All;
			filter.IsActive = true;

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory);

			shipments.Load(FilterStripBizO.Filter);

			AssertEquals("Should contain shipment1", true, shipments.Contains(shipment1));
			AssertEquals("Should contain shipment2", true, shipments.Contains(shipment2));
			AssertEquals("Should contain shipment3", true, shipments.Contains(shipment3));
			AssertEquals("should contain shipment4", true, shipments.Contains(shipment4));
			AssertEquals("should contain shipment5", true, shipments.Contains(shipment5));

			filter.Property = JobShipmentFilterBusinessObject.PodFilterItems.Open;
			shipments.Load(FilterStripBizO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("Should not contain shipment1", false, shipments.Contains(shipment1));
				AssertEquals("Should contain shipment2", true, shipments.Contains(shipment2));
				AssertEquals("Should contain shipment3", true, shipments.Contains(shipment3));
				AssertEquals("should contain shipment4", true, shipments.Contains(shipment4));
				AssertEquals("should contain shipment5", false, shipments.Contains(shipment5));
			});
			filter.Property = JobShipmentFilterBusinessObject.PodFilterItems.Closed;
			shipments.Load(FilterStripBizO.Filter);

			AssertEquals("Should contain shipment1", true, shipments.Contains(shipment1));
			AssertEquals("Should not contain shipment2", false, shipments.Contains(shipment2));
			AssertEquals("Should not contain shipment3", false, shipments.Contains(shipment3));
			AssertEquals("should not contain shipment4", false, shipments.Contains(shipment4));
			AssertEquals("should not contain shipment5", true, shipments.Contains(shipment5));
		}

		public void TestPodFilter_SeaBCN()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment5 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment5.JS_TransportMode = Constants.TransportModes.Sea;
			shipment5.JS_PackingMode = Constants.ContainerModes.FCL;
			ForwardingConsol consol = shipment4.Consols.AddNew();
			ForwardingContainer container = consol.Containers.AddNew();

			ForwardingPackLine shipment1PackLine = shipment1.OuterPackLines.AddNew();
			ForwardingPackLine shipment2PackLine = shipment2.OuterPackLines.AddNew();
			ForwardingPackLine shipment5PackLine = shipment5.OuterPackLines.AddNew();

			consol.Shipments.Add(shipment5);
			shipment5PackLine.SetContainer(consol, container);

			CommonPickupDeliveryConfirm forwardingContainerLeg1 = shipment1.DeliveryConfirms.AddNew();
			forwardingContainerLeg1.EU_PickupDeliveryTime = ZDateTime.Today;
			forwardingContainerLeg1.EU_GoodsSignForBy = "Someone";

			ForwardingPackLine shipment1bPackLine = shipment1.OuterPackLines.AddNew();

			shipment1.InnerPackLines.AddNew();

			CommonPickupDeliveryConfirm forwardingContainerLeg2 = shipment2.DeliveryConfirms.AddNew();

			AssertEquals("shipment3 should not have any legs", 0, shipment3.DeliveryConfirms.Count);

			container.DestinationConfirm.EU_PickupDeliveryTime = ZDateTime.Today;
			container.DestinationConfirm.EU_GoodsSignForBy = "Someone";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Proof-of-Delivery (POD) information"];
			filter.Property = JobShipmentFilterBusinessObject.PodFilterItems.All;
			filter.IsActive = true;

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory);

			shipments.Load(FilterStripBizO.Filter);

			AssertEquals("Should contain shipment1", true, shipments.Contains(shipment1));
			AssertEquals("Should contain shipment2", true, shipments.Contains(shipment2));
			AssertEquals("Should contain shipment3", true, shipments.Contains(shipment3));
			AssertEquals("should contain shipment4", true, shipments.Contains(shipment4));
			AssertEquals("should contain shipment5", true, shipments.Contains(shipment5));

			filter.Property = JobShipmentFilterBusinessObject.PodFilterItems.Open;
			shipments.Load(FilterStripBizO.Filter);

			AssertEquals("Should not contain shipment1", false, shipments.Contains(shipment1));
			AssertEquals("Should contain shipment2", true, shipments.Contains(shipment2));
			AssertEquals("Should contain shipment3", true, shipments.Contains(shipment3));
			AssertEquals("should contain shipment4", true, shipments.Contains(shipment4));
			AssertEquals("should contain shipment5", false, shipments.Contains(shipment5));

			filter.Property = JobShipmentFilterBusinessObject.PodFilterItems.Closed;
			shipments.Load(FilterStripBizO.Filter);

			AssertEquals("Should contain shipment1", true, shipments.Contains(shipment1));
			AssertEquals("Should not contain shipment2", false, shipments.Contains(shipment2));
			AssertEquals("Should not contain shipment3", false, shipments.Contains(shipment3));
			AssertEquals("should not contain shipment4", false, shipments.Contains(shipment4));
			AssertEquals("should not contain shipment5", true, shipments.Contains(shipment5));
		}

		public void TestPodFilter_Complex()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingPackLine packLine1a = shipment1.OuterPackLines.AddNew();
			packLine1a.JL_PackageCount = 10;
			CommonPickupDeliveryConfirm confirm1a = shipment1.DeliveryConfirms.AddNew();
			CommonPickupDeliveryConfirm confirm1b = shipment1.DeliveryConfirms.AddNew();
			CommonConfirmDivot divot1a = confirm1a.GetDivot(packLine1a);
			CommonConfirmDivot divot1b = confirm1b.GetDivot(packLine1a);
			divot1a.J8_PackagesDelivered = 4;
			divot1b.J8_PackagesDelivered = 6;
			confirm1a.EU_PickupDeliveryTime = ZDateTime.Today;
			confirm1a.EU_GoodsSignForBy = "Someone";

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingPackLine packLine2a = shipment2.OuterPackLines.AddNew();
			packLine2a.JL_PackageCount = 10;
			CommonPickupDeliveryConfirm confirm2a = shipment2.DeliveryConfirms.AddNew();
			CommonPickupDeliveryConfirm confirm2b = shipment2.DeliveryConfirms.AddNew();
			CommonConfirmDivot divot2a = confirm2a.GetDivot(packLine2a);
			CommonConfirmDivot divot2b = confirm2b.GetDivot(packLine2a);
			divot2a.J8_PackagesDelivered = 4;
			divot2b.J8_PackagesDelivered = 6;
			confirm2a.EU_PickupDeliveryTime = ZDateTime.Today;
			confirm2a.EU_GoodsSignForBy = "Someone";
			confirm2b.EU_PickupDeliveryTime = ZDateTime.Today;
			confirm2b.EU_GoodsSignForBy = "Someone";
			shipment2.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Empty;

			ForwardingShipment shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_TransportMode = Constants.TransportModes.Air;
			shipment3.JS_PackingMode = Constants.ContainerModes.BuyersConsol;
			ForwardingPackLine packLine3a = shipment3.OuterPackLines.AddNew();
			packLine3a.JL_PackageCount = 10;
			CommonPickupDeliveryConfirm confirm3a = shipment3.DeliveryConfirms.AddNew();
			CommonPickupDeliveryConfirm confirm3b = shipment3.DeliveryConfirms.AddNew();
			CommonConfirmDivot divot3a = confirm3a.GetDivot(packLine3a);
			CommonConfirmDivot divot3b = confirm3b.GetDivot(packLine3a);
			divot3a.J8_PackagesDelivered = 4;
			divot3b.J8_PackagesDelivered = 6;
			confirm3a.EU_PickupDeliveryTime = ZDateTime.Today;
			confirm3a.EU_GoodsSignForBy = "Someone";
			confirm3b.EU_PickupDeliveryTime = ZDateTime.Today;
			confirm3b.EU_GoodsSignForBy = "Someone";
			shipment3.DocsAndCartage.JP_DeliveryCartageCompleted = ZDateTime.Today;

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Proof-of-Delivery (POD) information"];
			filter.Property = JobShipmentFilterBusinessObject.PodFilterItems.All;
			filter.IsActive = true;

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory);

			shipments.Load(FilterStripBizO.Filter);

			AssertEquals("Should contain shipment1", true, shipments.Contains(shipment1));
			AssertEquals("Should contain shipment2", true, shipments.Contains(shipment2));
			AssertEquals("Should contain shipment3", true, shipments.Contains(shipment3));

			filter.Property = JobShipmentFilterBusinessObject.PodFilterItems.Open;
			shipments.Load(FilterStripBizO.Filter);

			AssertEquals("Should not contain shipment1", true, shipments.Contains(shipment1));
			AssertEquals("Should contain shipment2", false, shipments.Contains(shipment2));
			AssertEquals("Should contain shipment3", false, shipments.Contains(shipment3));

			filter.Property = JobShipmentFilterBusinessObject.PodFilterItems.Closed;
			shipments.Load(FilterStripBizO.Filter);

			AssertEquals("Should contain shipment1", false, shipments.Contains(shipment1));
			AssertEquals("Should not contain shipment2", true, shipments.Contains(shipment2));
			AssertEquals("Should not contain shipment3", true, shipments.Contains(shipment3));
		}

		#endregion

		#region Shipment Status Filter

		public void TestShipmentStatusFilter_Enabled()
		{
			AssertNotNull("Filter is available as registry is enabled", FilterStripBizO["Booking Status"]);
		}

		public void TestShipmentStatusFilter()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment5 = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment1.JS_ShipmentStatus = "";
			shipment2.JS_ShipmentStatus = "EBK";
			shipment3.JS_ShipmentStatus = "EBK";
			shipment4.JS_ShipmentStatus = "CNF";
			shipment5.JS_ShipmentStatus = "WEB";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["Booking Status"];
			filter.Property = "";
			filter.IsActive = true;

			var shipments = new ForwardingShipmentCollection(Factory);

			shipments.Load(FilterStripBizO.Filter);

			AssertEquals("Should contain shipment1", true, shipments.Contains(shipment1));
			AssertEquals("Should contain shipment2", true, shipments.Contains(shipment2));
			AssertEquals("Should contain shipment3", true, shipments.Contains(shipment3));
			AssertEquals("should contain shipment4", true, shipments.Contains(shipment4));
			AssertEquals("should contain shipment5", true, shipments.Contains(shipment5));

			filter.Property = "EBK";
			shipments.Load(FilterStripBizO.Filter);

			AssertEquals("Should not contain shipment1", false, shipments.Contains(shipment1));
			AssertEquals("Should contain shipment2", true, shipments.Contains(shipment2));
			AssertEquals("Should contain shipment3", true, shipments.Contains(shipment3));
			AssertEquals("Should not contain shipment4", false, shipments.Contains(shipment4));
			AssertEquals("Should not contain shipment5", false, shipments.Contains(shipment5));

			filter.Property = "WEB";
			shipments.Load(FilterStripBizO.Filter);

			AssertEquals("Should not contain shipment1", false, shipments.Contains(shipment1));
			AssertEquals("Should not contain shipment2", false, shipments.Contains(shipment2));
			AssertEquals("Should not contain shipment3", false, shipments.Contains(shipment3));
			AssertEquals("Should not contain shipment4", false, shipments.Contains(shipment4));
			AssertEquals("Should contain shipment5", true, shipments.Contains(shipment5));
		}

		public void TestShipmentStatusList()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var filter = (JobShipmentFilterBusinessObject)FilterStripBizO;
			AssertEquals(shipment.Lookups.JS_ShipmentStatus_List.Count + 6, filter.ShipmentStatusList.Count);

			foreach (CodeDescriptionPair pair in shipment.Lookups.JS_ShipmentStatus_List)
			{
				AssertCollectionContains(pair.Code, filter.ShipmentStatusList.GetAllCodes());
				AssertEquals(pair.Description, filter.ShipmentStatusList.GetDescriptionFromCode(pair.Code));
			}

			AssertCollectionContains(ShipmentStatusList.Codes.WebBooking, filter.ShipmentStatusList.GetAllCodes());
			AssertEquals(ShipmentStatusList.Descriptions.WebBooking, filter.ShipmentStatusList.GetDescriptionFromCode(ShipmentStatusList.Codes.WebBooking));

			AssertCollectionContains(ShipmentStatusList.Codes.EBookingCancellationRequest, filter.ShipmentStatusList.GetAllCodes());
			AssertEquals(ShipmentStatusList.Descriptions.EBookingCancellationRequest, filter.ShipmentStatusList.GetDescriptionFromCode(ShipmentStatusList.Codes.EBookingCancellationRequest));

			AssertCollectionContains(ShipmentStatusList.Codes.BookingCancelled, filter.ShipmentStatusList.GetAllCodes());
			AssertEquals(ShipmentStatusList.Descriptions.BookingCancelled, filter.ShipmentStatusList.GetDescriptionFromCode(ShipmentStatusList.Codes.BookingCancelled));
		}

		#endregion

		/*Please save non-current company JobHeader via switching user context*/
		[SuspendToTestReportJobIsChangedByDifferentCompany]
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestJobHoldStatusFilter()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();

			var job1 = new JobHeader.Loader(shipment1).TryLoadOrCreate();
			var job2 = new JobHeader.Loader(shipment2).TryLoadOrCreate();
			var job3 = new JobHeader.Loader(shipment3).TryLoadOrCreate();
			var job4 = new JobHeader.Loader(shipment4).TryLoadOrCreate();

			job1.JH_HoldReason = "Reason ABC";
			job2.JH_HoldReason = "Reason DEF";
			job3.JH_HoldReason = "Reason GHI";
			job3.JH_GC = GlbCompany.GetDemoCompany(Factory).PK; // This Job3 belongs to another Company and should not appear in any of the results.
			job4.JH_HoldReason = "";

			Factory.Save();

			var filteredCollection = new ForwardingShipmentCollection(Factory);
			var filter = (ModuleTextFilter)FilterStripBizO["Job Status Hold Reason"];

			filter.Property = "Reason";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			filteredCollection.Load(FilterStripBizO.Filter);

			Assert("Expecting collection to contain Shipment1", filteredCollection.Contains(shipment1));
			Assert("Expecting collection to contain Shipment2", filteredCollection.Contains(shipment2));
			Assert("Expecting collection not to contain Shipment3", !filteredCollection.Contains(shipment3));
			Assert("Expecting collection not to contain Shipment4", !filteredCollection.Contains(shipment4));

			filter.Property = "Reason DEF";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filteredCollection.Load(FilterStripBizO.Filter);

			Assert("Expecting collection not to contain Shipment1", !filteredCollection.Contains(shipment1));
			Assert("Expecting collection to contain Shipment2", filteredCollection.Contains(shipment2));
			Assert("Expecting collection not to contain Shipment3", !filteredCollection.Contains(shipment3));
			Assert("Expecting collection not to contain Shipment4", !filteredCollection.Contains(shipment4));

			filter.Property = "DEF";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.IsActive = true;
			filteredCollection.Load(FilterStripBizO.Filter);

			Assert("Expecting collection to contain Shipment1", filteredCollection.Contains(shipment1));
			Assert("Expecting collection not to contain Shipment2", !filteredCollection.Contains(shipment2));
			Assert("Expecting collection not to contain Shipment3", !filteredCollection.Contains(shipment3));
			Assert("Expecting collection to contain Shipment4", filteredCollection.Contains(shipment4));

			filter.Property = "";
			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			filter.IsActive = true;
			filteredCollection.Load(FilterStripBizO.Filter);

			Assert("Expecting collection not to contain Shipment1", !filteredCollection.Contains(shipment1));
			Assert("Expecting collection not to contain Shipment2", !filteredCollection.Contains(shipment2));
			Assert("Expecting collection not to contain Shipment3", !filteredCollection.Contains(shipment3));
			Assert("Expecting collection to contain Shipment4", filteredCollection.Contains(shipment4));

			filter.Property = "";
			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			filter.IsActive = true;
			filteredCollection.Load(FilterStripBizO.Filter);

			Assert("Expecting collection to contain Shipment1", filteredCollection.Contains(shipment1));
			Assert("Expecting collection to contain Shipment2", filteredCollection.Contains(shipment2));
			Assert("Expecting collection not to contain Shipment3", !filteredCollection.Contains(shipment3));
			Assert("Expecting collection not to contain Shipment4", !filteredCollection.Contains(shipment4));
		}

		#endregion

		#region Billing Filters

		public void TestAPInvoiceNumberFilter()
		{
			AssertNotNull(FilterStripBizO["AP Invoice #"]);

			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();

			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = shipment1.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.Parent = shipment1;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "00001001";

			AccTransactionHeader newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			newInvoice.AH_OH = org.PK;
			newInvoice.AH_TransactionNum = "00001001";
			newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_ConsolidatedInvoiceRef = "S00009999";

			AccTransactionLines newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			newInvoiceLine.AL_AH = newInvoice.PK;
			newInvoiceLine.AL_JH = job.PK;
			newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)FilterStripBizO["AP Invoice #"];
			filter.Property = "00001001";
			filter.IsActive = true;

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertEquals("Shipment1 is in Collection", true, shipments.Contains(shipment1.PK));
			AssertEquals("Shipment2 is not in Collection", false, shipments.Contains(shipment2.PK));

			filter.Property = "00001002";
			filter.IsActive = true;

			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertEquals("Shipment1 is not in Collection", false, shipments.Contains(shipment1.PK));
			AssertEquals("Shipment2 is not in Collection", false, shipments.Contains(shipment2.PK));

			filter.Property = "";
			filter.IsActive = true;

			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertEquals("Shipment1 is in Collection", true, shipments.Contains(shipment1.PK));
			AssertEquals("Shipment2 is in Collection", true, shipments.Contains(shipment2.PK));
		}

		public void TestARTransactionFilter()
		{
			AssertNotNull(FilterStripBizO["AR Transaction #"]);

			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();

			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = shipment1.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.Parent = shipment1;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "00001005";

			AccTransactionHeader newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			newInvoice.AH_OH = org.PK;
			newInvoice.AH_TransactionNum = "00001005";
			newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_ConsolidatedInvoiceRef = "S00009999";

			AccTransactionLines newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			newInvoiceLine.AL_AH = newInvoice.PK;
			newInvoiceLine.AL_JH = job.PK;
			newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			Factory.Save();

			ModuleFountainFilter filter = (ModuleFountainFilter)FilterStripBizO["AR Transaction #"];
			filter.Property = "00001005";
			filter.IsActive = true;

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertEquals("Shipment1 is in Collection", true, shipments.Contains(shipment1.PK));
			AssertEquals("Shipment2 is not in Collection", false, shipments.Contains(shipment2.PK));

			filter.Property = "00001001";
			filter.IsActive = true;

			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertEquals("Shipment1 is not in Collection", false, shipments.Contains(shipment1.PK));
			AssertEquals("Shipment2 is not in Collection", false, shipments.Contains(shipment2.PK));

			filter.Property = "";
			filter.IsActive = true;

			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertEquals("Shipment1 is in Collection", true, shipments.Contains(shipment1.PK));
			AssertEquals("Shipment2 is in Collection", true, shipments.Contains(shipment2.PK));
		}

		#endregion

		#region Organisation Filters

		#region Pickup Agent

		public void TestPickupAgentFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_IsActive = false;

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			JobDocAddress agentAddress1 = shipment1.PickupAgentDocumentaryAddress;
			agentAddress1.OrganisationPK = org1.PK;

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			JobDocAddress agentAddress2 = shipment2.PickupAgentDocumentaryAddress;
			agentAddress2.OrganisationPK = org2.PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var agentAddress3 = shipment3.PickupAgentDocumentaryAddress;
			agentAddress3.OrganisationPK = org3.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Pickup Agent"];
			filter.IsActive = true;
			filter.Property = org2.PK;

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertEquals("Found 1 element", 1, shipments.Count);
			AssertEquals("Found shipment2", shipment2.PK, shipments[0].PK);

			filter.IsActive = true;
			filter.Property = org1.PK;

			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertEquals("Found 1 element", 1, shipments.Count);
			AssertEquals("Found shipment1", shipment1.PK, shipments[0].PK);

			AssertNoWarning(filter.PropertyInfo, "Organization is in-active.");
			filter.IsActive = true;
			filter.Property = org3.PK;

			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			AssertHasWarning(filter.PropertyInfo, "Organization is in-active.");
			AssertEquals("Found 1 element", 1, shipments.Count);
			AssertEquals("Found shipment3", shipment3.PK, shipments[0].PK);
		}

		public void TestPickupAgentFilter_BlankOperators()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment1.PickupAgentDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			shipment2.PickupAgentDocumentaryAddress.E2_OA_Address = org2.MainAddress.PK;

			Factory.Save();

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Pickup Agent"];
			filter.IsActive = true;

			filter.Property = ZGuid.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals("Contains 6 shipments from Setup()", 6, shipments.Count);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals("Contains shipment1 & shipment2", 2, shipments.Count);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);
		}

		public void TestPickupAgentFilter_MultipleEqual()
		{
			AssertDocAddressMultiValueQuery_Equal("PickupAgentDocumentaryAddress", "Pickup Agent");
		}

		public void TestPickupAgentFilter_MultipleNotEqual()
		{
			AssertDocAddressMultiValueQuery_NotEqual("PickupAgentDocumentaryAddress", "Pickup Agent");
		}

		public void TestPickupAgentFilter_MultipleOtherScenario()
		{
			AssertDocAddressMultiValueQuery_OtherScenario("Pickup Agent");
		}

		#endregion

		#region Controlling Customer

		public void TestControllingAgentFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			JobHeader job1 = new JobHeader.Loader(shipment1).TryCreate();
			job1.JH_OA_LocalChargesAddr = org1.MainAddress.PK;
			job1.FillWithValidTestData();

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			JobHeader job2 = new JobHeader.Loader(shipment2).TryCreate();
			job2.JH_OA_AgentCollectAddr = org1.MainAddress.PK;
			job2.FillWithValidTestData();

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Controlling Agent"];
			filter.Property = org2.PK;
			filter.IsActive = true;

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertEquals("Finded 0 elements", 0, shipments.Count);

			IDocAddresses addressProvider = shipment1;
			if (addressProvider != null)
			{
				JobDocAddress address = addressProvider.DocAddresses.AddNew(DocAddressType.ControllingAgent);
				address.E2_OA_Address = org2.MainAddress.PK;
			}

			Factory.Save();

			filter.Property = org1.PK;
			filter.IsActive = true;

			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertEquals("Finded 0 elements", 0, shipments.Count);

			filter.Property = org2.PK;
			filter.IsActive = true;

			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertEquals("Finded 1 elements", 1, shipments.Count);
			AssertEquals("Shipment1 is in Collection", true, shipments.Contains(shipment1.PK));
			AssertEquals("Shipment2 is not in Collection", false, shipments.Contains(shipment2.PK));
		}

		public void TestControllingAgentFilter_BlankOperators()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment1.ControllingAgentDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			shipment2.ControllingAgentDocumentaryAddress.E2_OA_Address = org2.MainAddress.PK;

			Factory.Save();

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Controlling Agent"];
			filter.IsActive = true;

			filter.Property = ZGuid.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals("Contains 6 shipments from Setup()", 6, shipments.Count);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals("Contains shipment1 & shipment2", 2, shipments.Count);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);
		}

		public void TestControllingAgentFilter_MultipleEqual()
		{
			AssertDocAddressMultiValueQuery_Equal("ControllingAgentDocumentaryAddress", "Controlling Agent");
		}

		public void TestControllingAgentFilter_MultipleNotEqual()
		{
			AssertDocAddressMultiValueQuery_NotEqual("ControllingAgentDocumentaryAddress", "Controlling Agent");
		}

		public void TestControllingAgentFilter_MultipleOtherScenario()
		{
			AssertDocAddressMultiValueQuery_OtherScenario("Controlling Agent");
		}

		#endregion

		#region Controlling Customer

		public void TestControllingCustomerFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			JobHeader job1 = new JobHeader.Loader(shipment1).TryCreate();
			job1.JH_OA_LocalChargesAddr = org1.MainAddress.PK;
			job1.FillWithValidTestData();

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			JobHeader job2 = new JobHeader.Loader(shipment2).TryCreate();
			job2.JH_OA_AgentCollectAddr = org1.MainAddress.PK;
			job2.FillWithValidTestData();

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Controlling Customer"];
			filter.Property = org2.PK;
			filter.IsActive = true;

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertEquals("Finded 0 elements", 0, shipments.Count);

			IDocAddresses addressProvider = shipment1;
			if (addressProvider != null)
			{
				JobDocAddress address = addressProvider.DocAddresses.AddNew(DocAddressType.ControllingCustomer);
				address.E2_OA_Address = org2.MainAddress.PK;
			}

			Factory.Save();

			filter.Property = org1.PK;
			filter.IsActive = true;

			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertEquals("Finded 0 elements", 0, shipments.Count);

			filter.Property = org2.PK;
			filter.IsActive = true;

			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertEquals("Finded 1 elements", 1, shipments.Count);
			AssertEquals("Shipment1 is in Collection", true, shipments.Contains(shipment1.PK));
			AssertEquals("Shipment2 is not in Collection", false, shipments.Contains(shipment2.PK));
		}

		public void TestControllingCustomerFilter_BlankOperators()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment1.ControllingCustomerAddress.E2_OA_Address = org1.MainAddress.PK;
			shipment2.ControllingCustomerAddress.E2_OA_Address = org2.MainAddress.PK;

			Factory.Save();

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Controlling Customer"];
			filter.IsActive = true;

			filter.Property = ZGuid.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals("Contains 6 shipments from Setup()", 6, shipments.Count);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals("Contains shipment1 & shipment2", 2, shipments.Count);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);
		}

		public void TestControllingCustomerFilter_MultipleEqual()
		{
			AssertDocAddressMultiValueQuery_Equal("ControllingCustomerAddress", "Controlling Customer");
		}

		public void TestControllingCustomerFilter_MultipleNotEqual()
		{
			AssertDocAddressMultiValueQuery_NotEqual("ControllingCustomerAddress", "Controlling Customer");
		}

		public void TestControllingCustomerFilter_MultipleOtherScenario()
		{
			AssertDocAddressMultiValueQuery_OtherScenario("Controlling Customer");
		}

		#endregion

		#region Consignee

		public void TestConsigneeFilter()
		{
			var org1 = GetOrgHeader("APRIS");
			org1.OH_IsConsignee = true;
			var org2 = GetOrgHeader("TECENT");
			org2.OH_IsConsignee = true;
			var org3 = GetOrgHeader("SINA");
			org3.OH_IsConsignor = true;
			var org4 = GetOrgHeader("INACTIVE");
			org4.OH_IsConsignee = true;
			org4.OH_IsActive = false;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.ConsigneeDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			shipment1.ConsignorDocumentaryAddress.E2_OA_Address = org3.MainAddress.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.ConsigneeDocumentaryAddress.E2_OA_Address = org2.MainAddress.PK;
			shipment2.ConsignorDocumentaryAddress.E2_OA_Address = org3.MainAddress.PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.ConsigneeDocumentaryAddress.E2_OA_Address = org4.MainAddress.PK;
			Factory.Save();

			var consigneeFilter = (ModuleGuidFilter)FilterStripBizO["Consignee"];
			consigneeFilter.IsActive = true;
			var shipments = new ForwardingShipmentCollection(Factory);

			consigneeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			consigneeFilter.Property = org2.PK;

			shipments.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment2 }, shipments);

			consigneeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;

			shipments.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment3 }, shipments);

			AssertNoWarning(consigneeFilter.PropertyInfo, "Organization is in-active.");
			consigneeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			consigneeFilter.Property = org4.PK;

			shipments.Load(FilterStripBizO.Filter);
			AssertHasWarning(consigneeFilter.PropertyInfo, "Organization is in-active.");
			AssertContainsExactElementsInAnyOrder(new[] { shipment3 }, shipments);
		}

		public void TestConsigneeFilter_BlankOperators()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment1.ConsigneeDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			shipment2.ConsigneeDocumentaryAddress.E2_OA_Address = org2.MainAddress.PK;

			Factory.Save();

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Consignee"];
			filter.IsActive = true;

			filter.Property = ZGuid.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals("Contains 6 shipments from Setup()", 6, shipments.Count);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals("Contains shipment1 & shipment2", 2, shipments.Count);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);
		}

		public void TestEmptyConsigneeFilter()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "C0000001";

			Factory.Save();

			var consigneeFilter = (ModuleGuidFilter)FilterStripBizO["Consignee"];
			consigneeFilter.IsActive = true;
			consigneeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

			var filter = (ModuleFountainFilter)FilterStripBizO["Consol #"];
			filter.IsActive = true;
			filter.Property = "C0000001";

			var shipments = new ForwardingShipmentCollection(Factory);
			shipments.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment }, shipments);
		}

		public void TestConsigneeFilterCombinedByOr()
		{
			var orgHeader1 = GetOrgHeader("APRIS");
			orgHeader1.OH_IsConsignee = true;

			var orgHeader2 = GetOrgHeader("KYZZZ");
			orgHeader2.OH_IsConsignee = true;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.ConsigneeDocumentaryAddress.E2_OA_Address = orgHeader1.MainAddress.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.ConsigneeDocumentaryAddress.E2_OA_Address = orgHeader2.MainAddress.PK;

			Factory.Save();

			var consigneeFilter1 = (ModuleGuidFilter)FilterStripBizO["Consignee"];
			consigneeFilter1.Property = orgHeader1.PK;
			consigneeFilter1.IsActive = true;

			var consigneeFilter2 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor("Consignee");
			consigneeFilter2.Property = orgHeader2.PK;
			consigneeFilter2.IsActive = true;

			consigneeFilter1.OrCategory = FilterOrCategory.Red;
			consigneeFilter2.OrCategory = FilterOrCategory.Red;

			var shipments = new ForwardingShipmentCollection(Factory);
			AssertNoExceptionThrown(() =>
			{
				shipments.Load(FilterStripBizO.Filter);
			});

			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);
		}

		public void TestConsigneeFilter_MultipleEqual()
		{
			AssertDocAddressMultiValueQuery_Equal("ConsigneeDocumentaryAddress", "Consignee");
		}

		public void	TestConsigneeFilter_MultipleNotEqual()
		{
			AssertDocAddressMultiValueQuery_NotEqual("ConsigneeDocumentaryAddress", "Consignee");
		}

		public void TestConsigneeFilter_MultipleOtherScenario()
		{
			AssertDocAddressMultiValueQuery_OtherScenario("Consignee");
		}

		#endregion

		#region Consignor

		public void TestConsignorFilter()
		{
			var org1 = GetOrgHeader("APRIS");
			org1.OH_IsConsignor = true;

			var org2 = GetOrgHeader("TECENT");
			org2.OH_IsConsignor = true;

			var org3 = GetOrgHeader("SINA");
			org3.OH_IsConsignee = true;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.ConsigneeDocumentaryAddress.E2_OA_Address = org3.MainAddress.PK;
			shipment1.ConsignorDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.ConsigneeDocumentaryAddress.E2_OA_Address = org3.MainAddress.PK;
			shipment2.ConsignorDocumentaryAddress.E2_OA_Address = org2.MainAddress.PK;

			Factory.Save();

			var consignorFilter = (ModuleGuidFilter)FilterStripBizO["Consignor"];
			consignorFilter.IsActive = true;
			var shipments = new ForwardingShipmentCollection(Factory);

			consignorFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			consignorFilter.Property = org2.PK;

			shipments.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment2 }, shipments);

			consignorFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;

			shipments.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1 }, shipments);
		}

		public void TestConsignorFilter_BlankOperators()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment1.ConsignorDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			shipment2.ConsignorDocumentaryAddress.E2_OA_Address = org2.MainAddress.PK;

			Factory.Save();

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Consignor"];
			filter.IsActive = true;

			filter.Property = ZGuid.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals("Contains 6 shipments from Setup()", 6, shipments.Count);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals("Contains shipment1 & shipment2", 2, shipments.Count);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);
		}

		public void TestEmptyConsignorFilter()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "C0000001";

			Factory.Save();

			var consignorFilter = (ModuleGuidFilter)FilterStripBizO["Consignor"];
			consignorFilter.IsActive = true;
			consignorFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

			var filter = (ModuleFountainFilter)FilterStripBizO["Consol #"];
			filter.IsActive = true;
			filter.Property = "C0000001";

			var shipments = new ForwardingShipmentCollection(Factory);
			shipments.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment }, shipments);
		}

		public void TestConsignorFilterCombinedByOr()
		{
			var orgHeader1 = GetOrgHeader("APRIS");
			orgHeader1.OH_IsConsignee = true;

			var orgHeader2 = GetOrgHeader("KYZZZ");
			orgHeader2.OH_IsConsignee = true;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.ConsignorDocumentaryAddress.E2_OA_Address = orgHeader1.MainAddress.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.ConsignorDocumentaryAddress.E2_OA_Address = orgHeader2.MainAddress.PK;

			Factory.Save();

			var consignorFilter1 = (ModuleGuidFilter)FilterStripBizO["Consignor"];
			consignorFilter1.Property = orgHeader1.PK;
			consignorFilter1.IsActive = true;

			var consignorFilter2 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor("Consignor");
			consignorFilter2.Property = orgHeader2.PK;
			consignorFilter2.IsActive = true;

			consignorFilter1.OrCategory = FilterOrCategory.Red;
			consignorFilter2.OrCategory = FilterOrCategory.Red;

			var shipments = new ForwardingShipmentCollection(Factory);
			AssertNoExceptionThrown(() =>
			{
				shipments.Load(FilterStripBizO.Filter);
			});

			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);
		}

		public void TestConsignorFilter_MultipleEqual()
		{
			AssertDocAddressMultiValueQuery_Equal("ConsignorDocumentaryAddress", "Consignor");
		}

		public void TestConsignorFilter_MultipleNotEqual()
		{
			AssertDocAddressMultiValueQuery_NotEqual("ConsignorDocumentaryAddress", "Consignor");
		}

		public void TestConsignorFilter_MultipleOtherScenario()
		{
			AssertDocAddressMultiValueQuery_OtherScenario("Consignor");
		}

		#endregion

		#region OrderBuyerFilter

		public void TestOrderBuyerFilter()
		{
			var shipmentA = Factory.New<ForwardingShipment>();
			var shipmentB = Factory.New<ForwardingShipment>();
			var shipmentC = Factory.New<ForwardingShipment>();

			var buyer1 = Factory.NewWithValidTestData<OrgHeader>();
			var buyer2 = Factory.NewWithValidTestData<OrgHeader>();
			var buyer3 = Factory.NewWithValidTestData<OrgHeader>();
			var buyer4 = Factory.NewWithValidTestData<OrgHeader>();

			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			var supplier3 = Factory.NewWithValidTestData<OrgHeader>();

			var orderA1 = shipmentA.AttachedOrders.AddNew();
			orderA1.BuyerPK = buyer1.PK;
			orderA1.SupplierPK = supplier1.PK;
			var orderA2 = shipmentA.AttachedOrders.AddNew();
			orderA2.BuyerPK = buyer2.PK;
			orderA2.SupplierPK = supplier2.PK;

			var orderB1 = shipmentB.AttachedOrders.AddNew();
			orderB1.BuyerPK = buyer3.PK;
			orderB1.SupplierPK = supplier1.PK;
			var orderB2 = shipmentB.AttachedOrders.AddNew();
			orderB2.BuyerPK = buyer3.PK;
			orderB2.SupplierPK = supplier3.PK;

			var orderC1 = shipmentC.AttachedOrders.AddNew();
			orderC1.BuyerPK = buyer1.PK;
			orderC1.SupplierPK = supplier3.PK;

			Factory.Save();

			var orderBuyerFilter = (ModuleGuidFilter)FilterStripBizO["Order - Buyer"];
			orderBuyerFilter.IsActive = true;
			orderBuyerFilter.Property = buyer1.PK;

			var shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertContainsExactElementsInAnyOrder(new[] { shipmentA, shipmentC }, shipments);

			orderBuyerFilter = (ModuleGuidFilter)FilterStripBizO["Order - Buyer"];
			orderBuyerFilter.IsActive = true;
			orderBuyerFilter.Property = buyer2.PK;

			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertContainsExactElementsInAnyOrder(new[] { shipmentA }, shipments);

			orderBuyerFilter = (ModuleGuidFilter)FilterStripBizO["Order - Buyer"];
			orderBuyerFilter.IsActive = true;
			orderBuyerFilter.Property = buyer4.PK;

			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertCollectionNotContains("Collection should not contain shipmentA", shipmentA, shipments);
			AssertCollectionNotContains("Collection should not contain shipmentB", shipmentB, shipments);
			AssertCollectionNotContains("Collection should not contain shipmentC", shipmentC, shipments);
		}

		#endregion

		#region OrderSupplierFilter

		public void TestOrderSupplierFilter()
		{
			var shipmentA = Factory.New<ForwardingShipment>();
			var shipmentB = Factory.New<ForwardingShipment>();
			var shipmentC = Factory.New<ForwardingShipment>();

			var buyer1 = Factory.NewWithValidTestData<OrgHeader>();
			var buyer2 = Factory.NewWithValidTestData<OrgHeader>();
			var buyer3 = Factory.NewWithValidTestData<OrgHeader>();
			var buyer4 = Factory.NewWithValidTestData<OrgHeader>();

			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			var supplier2 = Factory.NewWithValidTestData<OrgHeader>();
			var supplier3 = Factory.NewWithValidTestData<OrgHeader>();

			var orderA1 = shipmentA.AttachedOrders.AddNew();
			orderA1.BuyerPK = buyer1.PK;
			orderA1.SupplierPK = supplier1.PK;
			var orderA2 = shipmentA.AttachedOrders.AddNew();
			orderA2.BuyerPK = buyer2.PK;
			orderA2.SupplierPK = supplier2.PK;

			var orderB1 = shipmentB.AttachedOrders.AddNew();
			orderB1.BuyerPK = buyer3.PK;
			orderB1.SupplierPK = supplier1.PK;
			var orderB2 = shipmentB.AttachedOrders.AddNew();
			orderB2.BuyerPK = buyer3.PK;
			orderB2.SupplierPK = supplier3.PK;

			var orderC1 = shipmentC.AttachedOrders.AddNew();
			orderC1.BuyerPK = buyer1.PK;
			orderC1.SupplierPK = supplier3.PK;

			Factory.Save();

			var orderBuyerFilter = (ModuleGuidFilter)FilterStripBizO["Order - Supplier"];
			orderBuyerFilter.IsActive = true;
			orderBuyerFilter.Property = supplier3.PK;

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertContainsExactElementsInAnyOrder(new[] { shipmentB, shipmentC }, shipments);

			orderBuyerFilter = (ModuleGuidFilter)FilterStripBizO["Order - Supplier"];
			orderBuyerFilter.IsActive = true;
			orderBuyerFilter.Property = supplier1.PK;

			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertContainsExactElementsInAnyOrder(new[] { shipmentA, shipmentB }, shipments);
		}

		#endregion

		#region Gateway

		public void TestGatewayFilter()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();
			var shipment3 = Factory.New<ForwardingShipment>();

			var forwarder1 = Factory.NewWithValidTestData<OrgHeader>();
			var forwarder2 = Factory.NewWithValidTestData<OrgHeader>();
			var forwarder3 = Factory.NewWithValidTestData<OrgHeader>();
			var forwarder4 = Factory.NewWithValidTestData<OrgHeader>();

			shipment1.Gateways.AddNew().JSG_OA_ForwarderAddress = forwarder1.MainAddress.PK;
			shipment1.Gateways.AddNew().JSG_OA_ForwarderAddress = forwarder2.MainAddress.PK;
			shipment2.Gateways.AddNew().JSG_OA_ForwarderAddress = forwarder2.MainAddress.PK;
			shipment2.Gateways.AddNew().JSG_OA_ForwarderAddress = forwarder3.MainAddress.PK;

			Factory.Save();

			var gatewayFilter = (ModuleGuidFilter)FilterStripBizO["Gateway"];
			gatewayFilter.IsActive = true;
			gatewayFilter.Property = forwarder1.PK;

			var shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertContainsExactElementsInAnyOrder(new[] { shipment1 }, shipments);

			gatewayFilter.Property = forwarder2.PK;

			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);

			gatewayFilter.Property = forwarder3.PK;

			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertContainsExactElementsInAnyOrder(new[] { shipment2 }, shipments);

			gatewayFilter.Property = forwarder4.PK;

			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertCollectionNotContains("Collection should not contain shipment1", shipment1, shipments);
			AssertCollectionNotContains("Collection should not contain shipment2", shipment2, shipments);
			AssertCollectionNotContains("Collection should not contain shipment3", shipment3, shipments);
		}

		public void TestGatewayFilter_BlankOperators()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();

			var forwarder1 = Factory.NewWithValidTestData<OrgHeader>();
			var forwarder2 = Factory.NewWithValidTestData<OrgHeader>();
			var forwarder3 = Factory.NewWithValidTestData<OrgHeader>();

			shipment1.Gateways.AddNew().JSG_OA_ForwarderAddress = forwarder1.MainAddress.PK;
			shipment1.Gateways.AddNew().JSG_OA_ForwarderAddress = forwarder2.MainAddress.PK;
			shipment2.Gateways.AddNew().JSG_OA_ForwarderAddress = forwarder2.MainAddress.PK;
			shipment2.Gateways.AddNew().JSG_OA_ForwarderAddress = forwarder3.MainAddress.PK;

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterStripBizO["Gateway"];
			filter.IsActive = true;
			filter.Property = ZGuid.Empty;

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			var shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load(FilterStripBizO.Filter);

			AssertEquals("Found 6 default", 6, shipments.Count);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			shipments.Load(FilterStripBizO.Filter);

			AssertEquals("Found 2 from this test", 2, shipments.Count);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);
		}

		#endregion

		#region Local Client

		public void TestLocalClientFilter()
		{
			var org1 = GetOrgHeader("APRIS");
			var org2 = GetOrgHeader("TECENT");
			var org3 = GetOrgHeader("INACTIVE");
			org3.OH_IsActive = false;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job1 = new JobHeader.Loader(shipment1).TryCreate();
			job1.JH_OA_LocalChargesAddr = org1.MainAddress.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job2 = new JobHeader.Loader(shipment2).TryCreate();
			job2.JH_OA_LocalChargesAddr = org2.MainAddress.PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job3 = new JobHeader.Loader(shipment3).TryCreate();
			job3.JH_OA_LocalChargesAddr = org3.MainAddress.PK;

			Factory.Save();

			var localClientFilter = (ModuleGuidFilter)FilterStripBizO["Local Client (Billing)"];
			localClientFilter.IsActive = true;
			var shipments = new ForwardingShipmentCollection(Factory);

			localClientFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			localClientFilter.Property = org2.PK;

			shipments.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment2 }, shipments);

			localClientFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;

			shipments.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment3 }, shipments);

			AssertNoWarning(localClientFilter.PropertyInfo, "Organization is in-active.");
			localClientFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			localClientFilter.Property = org3.PK;

			shipments.Load(FilterStripBizO.Filter);
			AssertHasWarning(localClientFilter.PropertyInfo, "Organization is in-active.");
			AssertContainsExactElementsInAnyOrder(new[] { shipment3 }, shipments);
		}

		public void TestLocalClientFilter_BlankOperators()
		{
			var org1 = GetOrgHeader("APRIS");
			var org2 = GetOrgHeader("TECENT");

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job1 = new JobHeader.Loader(shipment1).TryCreate();
			job1.JH_OA_LocalChargesAddr = org1.MainAddress.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job2 = new JobHeader.Loader(shipment2).TryCreate();
			job2.JH_OA_LocalChargesAddr = org2.MainAddress.PK;

			Factory.Save();

			var localClientFilter = (ModuleGuidFilter)FilterStripBizO["Local Client (Billing)"];
			localClientFilter.IsActive = true;
			var shipments = new ForwardingShipmentCollection(Factory);

			localClientFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			localClientFilter.Property = org2.PK;

			shipments.Load(FilterStripBizO.Filter);
			AssertEquals("6 shipments from Setup()", 6, shipments.Count);

			localClientFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals("2 shipments from this test", 2, shipments.Count);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);
		}

		public void TestLocalClientFilterForEmptyBillingJob()
		{
			var data = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
			addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_UniqueConsignRef = "C0000001";

			Factory.Save();

			var localClientFilter = (ModuleGuidFilter)FilterStripBizO["Local Client (Billing)"];
			localClientFilter.IsActive = true;
			localClientFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

			var filter = (ModuleFountainFilter)FilterStripBizO["Consol #"];
			filter.IsActive = true;
			filter.Property = "C0000001";

			var shipments = new ForwardingShipmentCollection(Factory);
			shipments.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment }, shipments);
		}

		public void TestLocalClientFilter_MultipleEqual()
		{
			var orgHeader1 = GetOrgHeader("APRIS");
			var orgHeader2 = GetOrgHeader("KYZZZ");
			var orgHeader3 = GetOrgHeader("ABCDE");

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job1 = new JobHeader.Loader(shipment1).TryCreate();
			job1.JH_OA_LocalChargesAddr = orgHeader1.MainAddress.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job2 = new JobHeader.Loader(shipment2).TryCreate();
			job2.JH_OA_LocalChargesAddr = orgHeader2.MainAddress.PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job3 = new JobHeader.Loader(shipment3).TryCreate();
			job3.JH_OA_LocalChargesAddr = orgHeader3.MainAddress.PK;

			Factory.Save();

			var localClientFilter1 = (ModuleGuidFilter)FilterStripBizO["Local Client (Billing)"];
			localClientFilter1.Property = orgHeader1.PK;
			localClientFilter1.IsActive = true;

			var localClientFilter2 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor("Local Client (Billing)");
			localClientFilter2.Property = orgHeader2.PK;
			localClientFilter2.IsActive = true;

			var localClientFilter3 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor("Local Client (Billing)");
			localClientFilter3.Property = orgHeader3.PK;
			localClientFilter3.IsActive = true;

			localClientFilter1.OrCategory = FilterOrCategory.Red;
			localClientFilter2.OrCategory = FilterOrCategory.Red;
			localClientFilter3.OrCategory = FilterOrCategory.Red;

			AssertContains("Generated filter SQL",
				"JS_PK IN (SELECT JH_ParentID FROM dbo.JobHeader WHERE JH_OA_LocalChargesAddr IN (SELECT OA_PK FROM dbo.OrgAddress WHERE (OA_OH in (@CWO3_, @CWO4_, @CWO5_))) and JH_GC = @CWO6_",
				FilterStripBizO.Filter.FilterString);

			var shipments = new ForwardingShipmentCollection(Factory);
			shipments.Load(FilterStripBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2, shipment3 }, shipments);
		}

		public void TestLocalClientFilter_MultipleNotEqual()
		{
			var orgHeader1 = GetOrgHeader("APRIS");
			var orgHeader2 = GetOrgHeader("KYZZZ");
			var orgHeader3 = GetOrgHeader("ABCDE");

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job1 = new JobHeader.Loader(shipment1).TryCreate();
			job1.JH_OA_LocalChargesAddr = orgHeader1.MainAddress.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job2 = new JobHeader.Loader(shipment2).TryCreate();
			job2.JH_OA_LocalChargesAddr = orgHeader2.MainAddress.PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var job3 = new JobHeader.Loader(shipment3).TryCreate();
			job3.JH_OA_LocalChargesAddr = orgHeader3.MainAddress.PK;

			Factory.Save();

			var localClientFilter1 = (ModuleGuidFilter)FilterStripBizO["Local Client (Billing)"];
			localClientFilter1.Property = orgHeader1.PK;
			localClientFilter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			localClientFilter1.IsActive = true;

			var localClientFilter2 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor("Local Client (Billing)");
			localClientFilter2.Property = orgHeader2.PK;
			localClientFilter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			localClientFilter2.IsActive = true;

			AssertContains("Generated filter SQL",
				"JS_PK IN (SELECT JH_ParentID FROM dbo.JobHeader WHERE JH_OA_LocalChargesAddr IN (SELECT OA_PK FROM dbo.OrgAddress WHERE (OA_OH not in (@CWO1_, @CWO2_))) and JH_GC = @CWO3_",
				FilterStripBizO.Filter.FilterString);

			var shipments = new ForwardingShipmentCollection(Factory);
			shipments.Load(FilterStripBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { shipment3 }, shipments);
		}

		#endregion

		#region Sales Rep

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestSalesRepFilter()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = GlbStaff.CurrentUser;
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = ZString.Empty;

			JobHeader job1 = new JobHeader.Loader(Shipment1).TryLoadOrCreateWithoutMutexForTestOnly();
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job1.JH_GS_NKRepSales = staff1.GS_Code;

			JobHeader job2 = new JobHeader.Loader(Shipment2).TryLoadOrCreateWithoutMutexForTestOnly();
			job2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job2.JH_GS_NKRepSales = staff2.GS_Code;

			JobHeader job3 = new JobHeader.Loader(Shipment3).TryLoadOrCreateWithoutMutexForTestOnly();
			job3.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job3.JH_GS_NKRepSales = staff3.GS_Code;

			AssertEquals("Precondition: job1 is associated with the current company", GlbCompany.CurrentCompany.PK, job1.JH_GC);
			AssertEquals("Precondition: job2 is associated with the current company", GlbCompany.CurrentCompany.PK, job2.JH_GC);
			AssertEquals("Precondition: job3 is associated with the current company", GlbCompany.CurrentCompany.PK, job3.JH_GC);

			// shipments 4, 5, and 6 should not be included in the filter results because the JobHeader is not associated with the GlbCompany.CurrentCompany.
			var company1 = Factory.NewWithValidTestData<GlbCompany>();

			JobHeader job4 = new JobHeader.Loader(Shipment4).TryLoadOrCreateWithoutMutexForTestOnly();
			job4.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job4.JH_GS_NKRepSales = staff1.GS_Code;
			job4.JH_GC = company1.PK;

			JobHeader job5 = new JobHeader.Loader(Shipment5).TryLoadOrCreateWithoutMutexForTestOnly();
			job5.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job5.JH_GS_NKRepSales = staff2.GS_Code;
			job5.JH_GC = company1.PK;

			JobHeader job6 = new JobHeader.Loader(Shipment6).TryLoadOrCreateWithoutMutexForTestOnly();
			job6.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job6.JH_GS_NKRepSales = staff3.GS_Code;
			job6.JH_GC = company1.PK;

			Factory.Save();

			ModuleNkFilter filter = (ModuleNkFilter)FilterStripBizO["Sales Rep"];
			filter.IsActive = true;
			AssertEquals("The Sales Rep filter is expected to use comparison operators.", true, filter.HasComparisonOperator);
			AssertEquals("Precondition: default operator is equals", SQLComparisonOperator.Equal, filter.SqlComparisonOperator);

			string[] expectedOperators = new string[7];
			expectedOperators[0] = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			expectedOperators[1] = ModuleTextFilter.ComparisonConstants.IsBlank;
			expectedOperators[2] = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			expectedOperators[3] = ModuleTextFilter.ComparisonConstants.NotEqual;
			expectedOperators[4] = ModuleTextFilter.ComparisonConstants.Exact;
			expectedOperators[5] = ModuleTextFilter.ComparisonConstants.CurrentUser;
			expectedOperators[6] = string.Empty;
			AssertContainsExactElementsInAnyOrder(expectedOperators, filter.AllowedComparisonOperators);

			filter.Property = staff1.GS_Code;

			ForwardingShipment[] results = Factory.Load<ForwardingShipment>(filter.Query);
			AssertContainsExactElementsInAnyOrder("When using the Equal operator, only shipments with a matching Sales Rep for the current company should be returned",
				new ForwardingShipment[] { Shipment1 }, results);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			results = Factory.Load<ForwardingShipment>(filter.Query);
			AssertContainsExactElementsInAnyOrder("When using the NotEqual operator, all shipments EXCEPT ones with the matching Sales Rep for the current company are expected.",
				new ForwardingShipment[] { Shipment2, Shipment3, Shipment4, Shipment5, Shipment6 }, results);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			results = Factory.Load<ForwardingShipment>(filter.Query);
			AssertContainsExactElementsInAnyOrder("When using the IsBlank operator, only shipments with blank sales rep for the current company are expected",
				new ForwardingShipment[] { Shipment3, Shipment4, Shipment5, Shipment6 }, results);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			results = Factory.Load<ForwardingShipment>(filter.Query);
			AssertContainsExactElementsInAnyOrder("When using the IsNotBlank operator, only shipments with a SalesRep for the current company should be included",
				new ForwardingShipment[] { Shipment1, Shipment2 }, results);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.CurrentUser;
			results = Factory.Load<ForwardingShipment>(filter.Query);
			AssertContainsExactElementsInAnyOrder("When using the CurrentUser operator, only shipments where the SalesRep for the current company is the current user should be included",
				new ForwardingShipment[] { Shipment2 }, results);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			filter.SelectedFilters.AddTextFilterStrip("Code", staff1.GS_Code);
			results = Factory.Load<ForwardingShipment>(filter.Query);
			AssertContainsExactElementsInAnyOrder("When using the FiltersMatch operator, shipments matching the selected filters are included",
				new ForwardingShipment[] { Shipment1 }, results);
		}

		#endregion

		#region Overseas Agent

		public void TestOverseasAgentFilter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = new JobHeader.Loader(shipment).TryCreate();
			job.JH_OA_AgentCollectAddr = orgHeader.MainAddress.PK;

			Factory.Save();

			var filter = (ModuleGuidFilter)FilterStripBizO["Overseas Agent (Billing)"];
			filter.Property = orgHeader.PK;
			filter.IsActive = true;

			var shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertContainsExactElementsInAnyOrder("shipment is in Collection", new[] { shipment }, shipments);
		}

		#endregion

		#region Branch

		public void TestBranchFilter()
		{
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			JobHeader job1 = new JobHeader.Loader(shipment1).TryCreate();
			job1.JH_GB = branch1.PK;

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			JobHeader job2 = new JobHeader.Loader(shipment2).TryCreate();
			job2.JH_GB = branch2.PK;

			Factory.Save();

			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Branch (Current Co.)"];
			filter.Property = branch1.PK;
			filter.IsActive = true;

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertEquals("Shipment1 is in Collection", true, shipments.Contains(shipment1.PK));
			AssertEquals("Shipment2 is not in Collection", false, shipments.Contains(shipment2.PK));
		}

		#endregion

		#region TestDeliveryAgent

		public void TestDeliveryAgent()
		{
			TestOrgGuidFilter("Delivery Agent");
		}

		#endregion

		#region TestTranshipmentAgent

		public void TestTranshipmentAgent()
		{
			TestOrgGuidFilter("Transhipment Agent");
		}

		#endregion

		#region TestImportBroker

		public void TestImportBroker()
		{
			TestOrgGuidFilter("Import Broker");
		}

		#endregion

		#region TestExportBroker

		public void TestExportBroker()
		{
			TestOrgGuidFilter("Export Broker");
		}

		#endregion

		#region TestOrgGuidFilter

		void TestOrgGuidFilter(string filterName)
		{
			OrgHeader orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "6XP";

			OrgHeader orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "BBG";

			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_Code = "ABC";
			orgHeader3.OH_IsActive = false;

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_OH_DeliveryAgent = orgHeader1.PK;
			shipment1.JS_OH_TranshipAgent = orgHeader1.PK;
			shipment1.JS_OH_ImportBroker = orgHeader1.PK;
			shipment1.JS_OH_ExportBroker = orgHeader1.PK;

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_OH_DeliveryAgent = orgHeader2.PK;
			shipment2.JS_OH_TranshipAgent = orgHeader2.PK;
			shipment2.JS_OH_ImportBroker = orgHeader2.PK;
			shipment2.JS_OH_ExportBroker = orgHeader2.PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_OH_DeliveryAgent = orgHeader3.PK;
			shipment3.JS_OH_TranshipAgent = orgHeader3.PK;
			shipment3.JS_OH_ImportBroker = orgHeader3.PK;
			shipment3.JS_OH_ExportBroker = orgHeader3.PK;

			Factory.Save();

			JobShipmentFilterBusinessObject filter = (JobShipmentFilterBusinessObject)GetNewFilterStripBusinessObject();
			ModuleGuidFilter orgFilter = (ModuleGuidFilter)filter[filterName];
			orgFilter.Property = orgHeader1.PK;
			orgFilter.IsActive = true;

			ForwardingShipmentCollection collection = new ForwardingShipmentCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);
			AssertCollectionContains(shipment1, collection);

			orgFilter.Property = orgHeader2.PK;
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);
			AssertCollectionContains(shipment2, collection);

			orgFilter.Property = Guid.NewGuid();
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);

			AssertEquals(0, collection.Count);

			AssertNoWarning(orgFilter.PropertyInfo, "Organization is in-active.");
			orgFilter.Property = orgHeader3.PK;
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);
			AssertHasWarning(orgFilter.PropertyInfo, "Organization is in-active.");
			AssertEquals(1, collection.Count);
			AssertCollectionContains(shipment3, collection);
		}

		#endregion

		#region TestCarrier

		public void TestCarrier()
		{
			OrgHeader orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "6XP";

			OrgHeader orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "BBG";

			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_Code = "ABC";
			orgHeader3.OH_IsActive = false;

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.Consols.AddNew();
			shipment1.Consols[0].JK_OA_ShippingLineAddress = orgHeader1.MainAddress.PK;

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_IsBooking = true;
			shipment2.JS_OA_BookedShippingLineAddress = orgHeader2.MainAddress.PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_IsBooking = true;
			shipment3.JS_OA_BookedShippingLineAddress = orgHeader3.MainAddress.PK;

			Factory.Save();

			JobShipmentFilterBusinessObject filter = (JobShipmentFilterBusinessObject)GetNewFilterStripBusinessObject();
			ModuleGuidFilter orgFilter = (ModuleGuidFilter)filter["Carrier"];
			orgFilter.Property = orgHeader1.PK;
			orgFilter.IsActive = true;

			ForwardingShipmentCollection collection = new ForwardingShipmentCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);
			AssertCollectionContains(shipment1, collection);

			orgFilter.Property = orgHeader2.PK;
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);
			AssertCollectionContains(shipment2, collection);

			orgFilter.Property = Guid.NewGuid();
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);

			AssertEquals(0, collection.Count);

			AssertNoWarning(orgFilter.PropertyInfo, "Organization is in-active.");
			orgFilter.Property = orgHeader3.PK;
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);
			AssertHasWarning(orgFilter.PropertyInfo, "Organization is in-active.");
			AssertEquals(1, collection.Count);
			AssertCollectionContains(shipment3, collection);
		}

		public void TestCarrier_ShipmentAndConsolWithDifferentShippingLines()
		{
			OrgHeader orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "6XP";

			OrgHeader orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "BBG";

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.Consols.AddNew();
			shipment.Consols[0].JK_OA_ShippingLineAddress = orgHeader1.MainAddress.PK;
			shipment.JS_IsBooking = true;
			shipment.JS_OA_BookedShippingLineAddress = orgHeader2.MainAddress.PK;

			Factory.Save();

			JobShipmentFilterBusinessObject filter = (JobShipmentFilterBusinessObject)GetNewFilterStripBusinessObject();
			ModuleGuidFilter orgFilter = (ModuleGuidFilter)filter["Carrier"];
			orgFilter.Property = orgHeader1.PK;
			orgFilter.IsActive = true;

			ForwardingShipmentCollection collection = new ForwardingShipmentCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);
			AssertCollectionContains(shipment, collection);

			orgFilter.Property = orgHeader2.PK;
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);

			AssertEquals(0, collection.Count);

			orgFilter.Property = Guid.NewGuid();
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);

			AssertEquals(0, collection.Count);
		}
		#endregion

		#region TestPickupCFS

		public void TestPickupCFS()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "AAAA";

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "BBBB";

			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_Code = "CCCC";
			orgHeader3.OH_IsActive = false;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_OA_ExportReceivingDepot = orgHeader1.MainAddress.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_OA_ExportReceivingDepot = orgHeader2.MainAddress.PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_OA_ExportReceivingDepot = orgHeader3.MainAddress.PK;

			Factory.Save();

			JobShipmentFilterBusinessObject filter = (JobShipmentFilterBusinessObject)GetNewFilterStripBusinessObject();
			ModuleGuidFilter orgFilter = (ModuleGuidFilter)filter["PickupCFS"];
			orgFilter.Property = orgHeader1.PK;
			orgFilter.IsActive = true;

			ForwardingShipmentCollection collection = new ForwardingShipmentCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionContains(shipment1, collection);
			AssertEquals(1, collection.Count);

			orgFilter.Property = orgHeader2.PK;
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);

			AssertCollectionContains(shipment2, collection);
			AssertEquals(1, collection.Count);

			orgFilter.Property = Guid.NewGuid();
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);

			AssertEquals(0, collection.Count);

			AssertNoWarning(orgFilter.PropertyInfo, "Organization is in-active.");
			orgFilter.Property = orgHeader3.PK;
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);
			AssertHasWarning(orgFilter.PropertyInfo, "Organization is in-active.");
			AssertCollectionContains(shipment3, collection);
			AssertEquals(1, collection.Count);
		}

		public void TestPickupCFS_BlankOperators()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "AAAA";

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "BBBB";

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_OA_ExportReceivingDepot = orgHeader1.MainAddress.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_OA_ExportReceivingDepot = orgHeader2.MainAddress.PK;

			Factory.Save();

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory);
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["PickupCFS"];
			filter.IsActive = true;

			filter.Property = ZGuid.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals("Contains 6 shipments from Setup()", 6, shipments.Count);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals("Contains shipment1 & shipment2", 2, shipments.Count);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);
		}

		#endregion

		#region TestDeliveryCFS

		public void TestDeliveryCFS()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "AAAA";

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "BBBB";

			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_Code = "CCCC";
			orgHeader3.OH_IsActive = false;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_OA_ImportReleaseDepot = orgHeader1.MainAddress.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_OA_ImportReleaseDepot = orgHeader2.MainAddress.PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_OA_ImportReleaseDepot = orgHeader3.MainAddress.PK;

			Factory.Save();

			JobShipmentFilterBusinessObject filter = (JobShipmentFilterBusinessObject)GetNewFilterStripBusinessObject();
			ModuleGuidFilter orgFilter = (ModuleGuidFilter)filter["DeliveryCFS"];
			orgFilter.Property = orgHeader1.PK;
			orgFilter.IsActive = true;

			ForwardingShipmentCollection collection = new ForwardingShipmentCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionContains(shipment1, collection);
			AssertEquals(1, collection.Count);

			orgFilter.Property = orgHeader2.PK;
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);

			AssertCollectionContains(shipment2, collection);
			AssertEquals(1, collection.Count);

			orgFilter.Property = Guid.NewGuid();
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);

			AssertEquals(0, collection.Count);

			AssertNoWarning(orgFilter.PropertyInfo, "Organization is in-active.");
			orgFilter.Property = orgHeader3.PK;
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);
			AssertHasWarning(orgFilter.PropertyInfo, "Organization is in-active.");
			AssertCollectionContains(shipment3, collection);
			AssertEquals(1, collection.Count);
		}

		public void TestDeliveryCFS_BlankOperators()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "AAAA";

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "BBBB";

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_OA_ImportReleaseDepot = orgHeader1.MainAddress.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_OA_ImportReleaseDepot = orgHeader2.MainAddress.PK;

			Factory.Save();

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory);
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["DeliveryCFS"];
			filter.IsActive = true;

			filter.Property = ZGuid.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals("Contains 6 shipments from Setup()", 6, shipments.Count);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals("Contains shipment1 & shipment2", 2, shipments.Count);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);
		}

		#endregion

		#region TestPickupTransportCompany

		public void TestPickupTransportCompany()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "AAAA";
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "BBBB";
			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_Code = "CCCC";
			orgHeader3.OH_IsActive = false;

			var orgAddr1 = orgHeader1.MainAddress;
			var orgAddr2 = orgHeader2.MainAddress;
			var orgAddr3 = orgHeader3.MainAddress;

			shipment1.DocsAndCartage.JP_OA_PickupCartageCoAddr = orgAddr1.PK;
			shipment2.DocsAndCartage.JP_OA_PickupCartageCoAddr = orgAddr2.PK;
			shipment3.DocsAndCartage.JP_OA_PickupCartageCoAddr = orgAddr3.PK;

			Factory.Save();

			JobShipmentFilterBusinessObject filter = (JobShipmentFilterBusinessObject)GetNewFilterStripBusinessObject();
			ModuleGuidFilter orgFilter = (ModuleGuidFilter)filter["Pickup Transport Company"];
			orgFilter.Property = orgHeader1.PK;
			orgFilter.IsActive = true;

			ForwardingShipmentCollection collection = new ForwardingShipmentCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionContains(shipment1, collection);
			AssertEquals(1, collection.Count);

			orgFilter.Property = Guid.NewGuid();
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);

			AssertEquals(0, collection.Count);

			AssertNoWarning(orgFilter.PropertyInfo, "Organization is in-active.");
			orgFilter.Property = orgHeader3.PK;
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);
			AssertHasWarning(orgFilter.PropertyInfo, "Organization is in-active.");
			AssertCollectionContains(shipment3, collection);
			AssertEquals(1, collection.Count);
		}

		public void TestPickupTransportCompany_BlankOperators()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "AAAA";
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "BBBB";
			var orgAddr1 = orgHeader1.MainAddress;
			var orgAddr2 = orgHeader2.MainAddress;

			shipment1.DocsAndCartage.JP_OA_PickupCartageCoAddr = orgAddr1.PK;
			shipment2.DocsAndCartage.JP_OA_PickupCartageCoAddr = orgAddr2.PK;

			Factory.Save();

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory);
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Pickup Transport Company"];
			filter.IsActive = true;

			filter.Property = ZGuid.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals("Contains 6 shipments from Setup()", 6, shipments.Count);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals("Contains shipment1 & shipment2", 2, shipments.Count);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);
		}

		public void TestPickupTransportCompanyFilter_MultipleEqual()
		{
			AssertCartageCompanyMultiValueQuery_Equal("JP_OA_PickupCartageCoAddr", "Pickup Transport Company", JobDocsAndCartage.Schema.JP_OA_PickupCartageCoAddr);
		}

		public void TestPickupTransportCompanyFilter_MultipleNotEqual()
		{
			AssertCartageCompanyMultiValueQuery_NotEqual("JP_OA_PickupCartageCoAddr", "Pickup Transport Company", JobDocsAndCartage.Schema.JP_OA_PickupCartageCoAddr);
		}

		public void TestPickupTransportCompanyFilter_MultipleOtherScenario()
		{
			AssertCartageCompanyMultiValueQuery_OtherScenario("Pickup Transport Company", JobDocsAndCartage.Schema.JP_OA_PickupCartageCoAddr);
		}

		#endregion

		#region TestDeliveryTransportCompany

		public void TestDeliveryTransportCompany()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "AAAA";
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "BBBB";
			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_Code = "CCCC";
			orgHeader3.OH_IsActive = false;

			var orgAddr1 = orgHeader1.MainAddress;
			var orgAddr2 = orgHeader2.MainAddress;
			var orgAddr3 = orgHeader3.MainAddress;

			shipment1.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = orgAddr1.PK;
			shipment2.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = orgAddr2.PK;
			shipment3.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = orgAddr3.PK;

			Factory.Save();

			JobShipmentFilterBusinessObject filter = (JobShipmentFilterBusinessObject)GetNewFilterStripBusinessObject();
			ModuleGuidFilter orgFilter = (ModuleGuidFilter)filter["Delivery Transport Company"];
			orgFilter.Property = orgHeader1.PK;
			orgFilter.IsActive = true;

			ForwardingShipmentCollection collection = new ForwardingShipmentCollection(Factory);
			collection.Load(filter.Filter);

			AssertCollectionContains(shipment1, collection);
			AssertEquals(1, collection.Count);

			orgFilter.Property = Guid.NewGuid();
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);

			AssertEquals(0, collection.Count);

			AssertNoWarning(orgFilter.PropertyInfo, "Organization is in-active.");
			orgFilter.Property = orgHeader3.PK;
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);
			AssertHasWarning(orgFilter.PropertyInfo, "Organization is in-active.");
			AssertCollectionContains(shipment3, collection);
			AssertEquals(1, collection.Count);
		}

		public void TestDeliveryTransportCompany_BlankOperators()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "AAAA";
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "BBBB";
			var orgAddr1 = orgHeader1.MainAddress;
			var orgAddr2 = orgHeader2.MainAddress;

			shipment1.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = orgAddr1.PK;
			shipment2.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = orgAddr2.PK;

			Factory.Save();

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory);
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterStripBizO["Delivery Transport Company"];
			filter.IsActive = true;

			filter.Property = ZGuid.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals("Contains 6 shipments from Setup()", 6, shipments.Count);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals("Contains shipment1 & shipment2", 2, shipments.Count);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);
		}

		public void TestDeliveryTransportCompanyFilter_MultipleEqual()
		{
			AssertCartageCompanyMultiValueQuery_Equal("JP_OA_DeliveryCartageCoAddr", "Delivery Transport Company", JobDocsAndCartage.Schema.JP_OA_DeliveryCartageCoAddr);
		}

		public void TestDeliveryTransportCompanyFilter_MultipleNotEqual()
		{
			AssertCartageCompanyMultiValueQuery_NotEqual("JP_OA_DeliveryCartageCoAddr", "Delivery Transport Company", JobDocsAndCartage.Schema.JP_OA_DeliveryCartageCoAddr);
		}

		public void TestDeliveryTransportCompanyFilter_MultipleOtherScenario()
		{
			AssertCartageCompanyMultiValueQuery_OtherScenario("Delivery Transport Company", JobDocsAndCartage.Schema.JP_OA_DeliveryCartageCoAddr);
		}

		#endregion

		#region Test Client Assigned Staff Filter

		public void TestClientAssignedStaffFilter()
		{
			var shipment1 = GetShipmentWithConsignor("cnr1");
			var shipment2 = GetShipmentWithConsignor("cnr2");
			var shipment3 = GetShipmentWithConsignee("cne3");
			var shipment4 = GetShipmentWithConsignee("cne4");
			var shipment5 = GetShipmentWithLocalClient("loc5");
			var shipment6 = GetShipmentWithLocalClient("loc6");
			var shipment7 = GetShipmentWithControllingCustomer("cpy7");
			var shipment8 = GetShipmentWithControllingCustomer("cpy8");
			var emptyShipment = GetShipment("empty");

			GlbStaff staff1 = GetStaff("FRB", "Fred Bloggs", "Fred.Bloggs");
			GlbStaff staff2 = GetStaff("JOD", "John Doe", "John.Doe");
			GlbStaff staff3 = GetStaff("GEJ", "George Jones", "George.Jones");
			GlbStaff staff4 = GetStaff("SAS", "Sam Smith", "Sam.Smith");

			var controllingBranch = Factory.New<GlbBranch>();
			controllingBranch.GB_Code = "XYZ";
			controllingBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			AssignStaff(shipment1.Consignor, staff1, "ALL", "SAL", controllingBranch.PK);
			AssignStaff(shipment2.Consignor, staff2, "ALL", "SAL", ZGuid.Empty);
			AssignStaff(shipment3.Consignee, staff3, "AIR", "SAL", ZGuid.Empty);
			AssignStaff(shipment4.Consignee, staff4, "SEA", "SAL", ZGuid.Empty);
			AssignStaff(shipment5.Job.LocalCharges, staff1, "ALL", "CON", ZGuid.Empty);
			AssignStaff(shipment6.Job.LocalCharges, staff2, "ALL", "CON", ZGuid.Empty);
			AssignStaff(shipment7.ControllingCustomer, staff3, "ALL", "ACT", ZGuid.Empty);
			AssignStaff(shipment8.ControllingCustomer, staff4, "ALL", "ACT", ZGuid.Empty);

			Factory.Save();

			OrgClientAssignedStaffModuleFilter filter = (OrgClientAssignedStaffModuleFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.ClientAssignedStaff];

			Asserter.AssertMatches("Empty Filter", filter, shipment1, shipment2, shipment3, shipment4, shipment5, shipment6, shipment7, shipment8, emptyShipment);

			filter.ClientType = "CNR";
			filter.StaffRole = "SAL";
			Asserter.AssertMatches("Matches CNR shipments with SAL staff assigned", filter, shipment1, shipment2);

			filter.StaffRole = ZString.Empty;
			filter.AssignedStaff = "FRB";
			Asserter.AssertMatches("Matches CNR shipments with FRB assigned", filter, shipment1);

			filter.AssignedStaff = ZString.Empty;
			filter.ClientType = "CNE";
			filter.Department = "AIR";
			Asserter.AssertMatches("Matches CNE shipments with AIR department assigned", filter, shipment3);

			filter.ClientType = "CNR";
			filter.StaffRole = "SAL";
			filter.AssignedStaff = "FRB";
			filter.Department = ZString.Empty;
			Asserter.AssertMatches("CNR SAL FRB", filter, shipment1);

			filter.AssignedStaff = "JOD";
			Asserter.AssertMatches("CNR SAL JOD", filter, shipment2);

			filter.Department = "ALL";
			Asserter.AssertMatches("CNR SAL JOD ALL", filter, shipment2);

			filter.Department = "AIR";
			Asserter.AssertMatches("CNR SAL JOD AIR", filter);

			filter.ClientType = "CNE";
			Asserter.AssertMatches("CNE SAL JOD AIR", filter);

			filter.AssignedStaff = "GEJ";
			Asserter.AssertMatches("CNE SAL GEJ AIR", filter, shipment3);

			filter.ClientType = "LOC";
			filter.AssignedStaff = "FRB";
			filter.StaffRole = "CON";
			filter.Department = "ALL";
			Asserter.AssertMatches("LOC CON FRB ALL", filter, shipment5);

			filter.ClientType = "CPY";
			filter.AssignedStaff = "SAS";
			filter.StaffRole = "ACT";
			Asserter.AssertMatches("CPY ACT SAS ALL", filter, shipment8);

			filter.ClientType = "CNR";
			filter.AssignedStaff = ZString.Empty;
			filter.StaffRole = ZString.Empty;
			filter.ControllingBranch = controllingBranch.PK;
			Asserter.AssertMatches("XYZ", filter, shipment1);
		}

		public void TestClientAssignedStaffFilter_AllowedComparisonOperators()
		{
			OrgClientAssignedStaffModuleFilter filter = (OrgClientAssignedStaffModuleFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.ClientAssignedStaff];

			List<string> expectedOperators = new List<string>();
			if (filter.SupportsFiltersMatchComparisonOperator)
			{
				expectedOperators.Add(ModuleTextFilter.ComparisonConstants.FiltersMatch);
			}
			expectedOperators.Add(ModuleTextFilter.ComparisonConstants.IsBlank);
			expectedOperators.Add(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			expectedOperators.Add(ModuleTextFilter.ComparisonConstants.NotEqual);
			expectedOperators.Add(ModuleTextFilter.ComparisonConstants.Exact);
			expectedOperators.Add(ModuleTextFilter.ComparisonConstants.CurrentUser);
			expectedOperators.Add(string.Empty);

			AssertContainsExactElementsInAnyOrder(expectedOperators, filter.AllowedComparisonOperators);
		}

		public void TestClientAssignedStaffFilter_AssignedStaffComparisonOperators()
		{
			#region Setup
			var shipment1 = GetShipmentWithConsignor("cnr1");
			var shipment2 = GetShipmentWithConsignor("cnr2");
			var shipment3 = GetShipmentWithConsignee("cne3");
			var shipment4 = GetShipmentWithConsignee("cne4");
			var shipment5 = GetShipmentWithLocalClient("loc5");
			var shipment6 = GetShipmentWithLocalClient("loc6");
			var shipment7 = GetShipmentWithControllingCustomer("cpy7");
			var shipment8 = GetShipmentWithControllingCustomer("cpy8");
			var emptyShipment = GetShipment("empty");

			GlbStaff staff1 = GetStaff("FRB", "Fred Bloggs", "Fred.Bloggs");
			GlbStaff staff2 = GetStaff("JOD", "John Doe", "John.Doe");
			GlbStaff staff3 = GetStaff("GEJ", "George Jones", "George.Jones");
			GlbStaff staff4 = GlbStaff.CurrentUser;

			var controllingBranch = Factory.New<GlbBranch>();
			controllingBranch.GB_Code = "XYZ";
			controllingBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			AssignStaff(shipment1.Consignor, staff1, "ALL", "SAL", controllingBranch.PK);
			AssignStaff(shipment2.Consignor, staff2, "ALL", "SAL", ZGuid.Empty);
			AssignStaff(shipment3.Consignee, staff3, "AIR", "SAL", ZGuid.Empty);
			AssignStaff(shipment4.Consignee, staff4, "SEA", "SAL", ZGuid.Empty);
			AssignStaff(shipment5.Job.LocalCharges, staff1, "ALL", "CON", ZGuid.Empty);
			AssignStaff(shipment6.Job.LocalCharges, staff2, "ALL", "CON", ZGuid.Empty);
			AssignStaff(shipment7.ControllingCustomer, staff3, "ALL", "ACT", ZGuid.Empty);
			AssignStaff(shipment8.ControllingCustomer, staff4, "ALL", "ACT", ZGuid.Empty);

			Factory.Save();

			OrgClientAssignedStaffModuleFilter filter = (OrgClientAssignedStaffModuleFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.ClientAssignedStaff];

			#endregion

			#region Exact
			filter.StaffRole = ZString.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

			filter.ClientType = "CNR";
			filter.AssignedStaff = staff1.GS_Code;
			Asserter.AssertMatches("Matches CNR shipments with staff1 assigned", filter, shipment1);

			filter.AssignedStaff = staff2.GS_Code;
			Asserter.AssertMatches("Matches CNR shipments with staff2 assigned", filter, shipment2);

			filter.ClientType = "CNE";
			filter.AssignedStaff = staff3.GS_Code;
			Asserter.AssertMatches("Matches CNE shipments with staff3 assigned", filter, shipment3);

			filter.ClientType = "LOC";
			filter.AssignedStaff = staff2.GS_Code;
			Asserter.AssertMatches("Matches LOC shipments with staff2 assigned", filter, shipment6);

			filter.ClientType = "CPY";
			filter.AssignedStaff = staff3.GS_Code;
			Asserter.AssertMatches("Matches CPY shipments with staff3 assigned", filter, shipment7);
			#endregion

			#region NotEqual
			filter.ClientType = "CNR";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.AssignedStaff = staff1.GS_Code;
			Asserter.AssertMatches("NotEqual operator matches CNR shipments without staff1 assigned", filter, shipment2);

			filter.ClientType = "CNE";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.AssignedStaff = staff3.GS_Code;
			Asserter.AssertMatches("NotEqual operator matches CNR shipments without staff3 assigned", filter, shipment4);

			filter.ClientType = "LOC";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.AssignedStaff = staff1.GS_Code;
			Asserter.AssertMatches("NotEqual operator matches LOC shipments without staff1 assigned", filter, shipment6);

			filter.ClientType = "CPY";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.AssignedStaff = staff4.GS_Code;
			Asserter.AssertMatches("NotEqual operator matches CPY shipments without staff4 assigned", filter, shipment7);
			#endregion

			#region IsBlank
			filter.ClientType = "CNR";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			AssertEquals("Precondition: AssignedStaff is empty when using the IsBlank comparison", ZString.Empty, filter.AssignedStaff);
			Asserter.AssertMatches("When using the IsBlank operator, no shipments are expected to be returned since there is a constraint preventing blank values for O8_GS_NKPersonResponsible.",
				filter);
			#endregion

			#region IsNotBlank
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;

			filter.ClientType = "CNR";
			AssertEquals("Precondition: AssignedStaff is empty when using the IsNotBlank comparison", ZString.Empty, filter.AssignedStaff);
			Asserter.AssertMatches("When using the IsNotBlank operator, all shipments with a PersonResponsible assigned should be included",
				filter, shipment1, shipment2);

			filter.ClientType = "CNE";
			AssertEquals("Precondition: AssignedStaff is empty when using the IsNotBlank comparison", ZString.Empty, filter.AssignedStaff);
			Asserter.AssertMatches("When using the IsNotBlank operator, all shipments with a PersonResponsible assigned should be included",
				filter, shipment3, shipment4);

			filter.ClientType = "LOC";
			AssertEquals("Precondition: AssignedStaff is empty when using the IsNotBlank comparison", ZString.Empty, filter.AssignedStaff);
			Asserter.AssertMatches("When using the IsNotBlank operator, all shipments with a PersonResponsible assigned should be included",
				filter, shipment5, shipment6);

			filter.ClientType = "CPY";
			AssertEquals("Precondition: AssignedStaff is empty when using the IsNotBlank comparison", ZString.Empty, filter.AssignedStaff);
			Asserter.AssertMatches("When using the IsNotBlank operator, all shipments with a PersonResponsible assigned should be included",
				filter, shipment7, shipment8);
			#endregion

			#region CurrentUser
			filter.ClientType = "CNE";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.CurrentUser;
			AssertEquals("Precondition: AssignedStaff is automatically set when using the CurrentUser operator", GlbStaff.CurrentUser.GS_Code, filter.AssignedStaff);
			Asserter.AssertMatches("When using the CurrentUser operator, all shipments with the CurrentUser assigned should be included",
				filter, shipment4);

			filter.ClientType = "CPY";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.CurrentUser;
			Asserter.AssertMatches("When using the CurrentUser operator, all shipments with the CurrentUser assigned should be included",
				filter, shipment8);
			#endregion

			#region FiltersMatch
			filter.ClientType = "CNR";
			filter.SupportsFiltersMatchComparisonOperator = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			filter.SelectedFilters.AddTextFilterStrip("Code", "FRB");
			Asserter.AssertMatches("Matches CNR shipments with FRB assigned", filter, shipment1);
			#endregion
		}

		public void TestClientAssignedStaff_BlankDetailsChecksStaff()
		{
			var shipment1 = GetShipmentWithConsignor("cnr1");
			var shipment2 = GetShipmentWithConsignor("cnr2");

			var staff = GetStaff("BOB", "Bobby McFlannery", "Bob.McFlannery");
			AssignStaff(shipment1.Consignor, staff, "ALL", "SAL", ZGuid.Empty);

			Factory.Save();

			var filter = (OrgClientAssignedStaffModuleFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.ClientAssignedStaff];
			filter.ClientType = "CNR";

			Asserter.AssertMatches("Should only match shipments with consignors that have staff assignments", filter, shipment1);
		}

		static void AssignStaff(OrgHeader org, GlbStaff staff, ZString department, ZString role, ZGuid controllingBranchPK)
		{
			var assignment = org.StaffAssignments.AddNew();
			assignment.O8_GS_NKPersonResponsible = staff.GS_Code;
			assignment.O8_Department = department;
			assignment.O8_Role = role;
			org.CompanyData.OB_GB_ControllingBranch = controllingBranchPK;
		}

		GlbStaff GetStaff(ZString code, ZString name, ZString loginName)
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_FullName = name;
			staff.GS_LoginName = loginName;
			staff.GS_Code = code;

			return staff;
		}

		#endregion

		#region Test Related Parties Filters

		public void TestConsignorRelatedPartiesFilter()
		{
			var shipment1 = GetShipmentWithConsignor("con1");
			var shipment2 = GetShipmentWithConsignor("con2");
			var emptyShipment = GetShipment("Empty");

			OrgHeader party1 = GetOrgHeader("party1");
			OrgHeader party2 = GetOrgHeader("party2");
			OrgHeader party3 = GetOrgHeader("party3");
			OrgHeader emptyParty = GetOrgHeader("empty");

			OrgRelatedParty relatedParty1 = GetOrgRelatedParty(shipment1.Consignor, party1);
			OrgRelatedParty relatedParty2 = GetOrgRelatedParty(shipment1.Consignor, party2);
			OrgRelatedParty relatedParty3 = GetOrgRelatedParty(shipment2.Consignor, party2);
			OrgRelatedParty relatedParty4 = GetOrgRelatedParty(shipment2.Consignor, party3);

			var newShipment1 = GetShipmentWithConsignor("consignor1");
			var newShipment2 = GetShipmentWithConsignor("consignor2");
			var newShipment3 = GetShipmentWithConsignor("consignor3");
			var newShipment4 = GetShipmentWithConsignor("consignor4");
			var newShipment5 = GetShipmentWithConsignor("consignor5");
			var newShipment6 = GetShipmentWithConsignor("consignor6");

			OrgHeader newParty = GetOrgHeader("newParty");

			OrgRelatedParty newRelatedParty1 = GetOrgRelatedParty(newShipment1.Consignor, newParty);
			OrgRelatedParty newRelatedParty2 = GetOrgRelatedParty(newShipment2.Consignor, newParty);
			OrgRelatedParty newRelatedParty3 = GetOrgRelatedParty(newShipment3.Consignor, newParty);
			OrgRelatedParty newRelatedParty4 = GetOrgRelatedParty(newShipment4.Consignor, newParty);
			OrgRelatedParty newRelatedParty5 = GetOrgRelatedParty(newShipment5.Consignor, newParty);
			OrgRelatedParty newRelatedParty6 = GetOrgRelatedParty(newShipment6.Consignor, newParty);

			newRelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;

			newRelatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;

			newRelatedParty3.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty3.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;

			newRelatedParty4.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty4.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;

			newRelatedParty5.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty5.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty5.PR_FreightTransportMode = Constants.TransportModes.Sea;
			newRelatedParty5.PR_FreightContainerMode = Constants.ContainerModes.FCL;

			newRelatedParty6.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty6.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty6.PR_FreightTransportMode = Constants.TransportModes.Sea;
			newRelatedParty6.PR_FreightContainerMode = Constants.ContainerModes.LCL;

			Factory.Save();

			Asserter.AddFieldOfInterest("JS_UniqueConsignRef");

			OrgRelatedPartiesModuleFilter filter = (OrgRelatedPartiesModuleFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.ConsignorRelatedParties];

			Asserter.AssertMatches("Empty Filter", filter, shipment1, shipment2, emptyShipment, newShipment1, newShipment2, newShipment3, newShipment4, newShipment5, newShipment6);

			filter.RelatedParty = party1.PK;
			Asserter.AssertMatches("party1", filter, shipment1);

			filter.RelatedParty = party2.PK;
			Asserter.AssertMatches("party2", filter, shipment1, shipment2);

			filter.RelatedParty = party3.PK;
			Asserter.AssertMatches("party3", filter, shipment2);

			filter.RelatedParty = emptyParty.PK;
			Asserter.AssertMatches("empty", filter);

			filter.RelatedParty = newParty.PK;
			Asserter.AssertMatches("newParty", filter, newShipment1, newShipment2, newShipment3, newShipment4, newShipment5, newShipment6);

			filter.PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			Asserter.AssertMatches("PartyType = APNettingGroup", filter, newShipment1);

			filter.PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			Asserter.AssertMatches("PartyType = APSettlementGroup", filter, newShipment2, newShipment3, newShipment4, newShipment5, newShipment6);

			filter.Direction = RelatedPartyDirectionList.Codes.Delivery;
			Asserter.AssertMatches("Direction = Delivery", filter, newShipment3);

			filter.Direction = RelatedPartyDirectionList.Codes.Pickup;
			Asserter.AssertMatches("Direction = Pickup", filter, newShipment4, newShipment5, newShipment6);

			filter.TransportMode = Constants.TransportModes.Sea;
			filter.ContainerMode = Constants.ContainerModes.FCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = FCL", filter, newShipment5);

			filter.TransportMode = Constants.TransportModes.Sea;
			filter.ContainerMode = Constants.ContainerModes.LCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = LCL", filter, newShipment6);
		}

		public void TestConsigneeRelatedPartiesFilter()
		{
			var shipment1 = GetShipmentWithConsignee("con1");
			var shipment2 = GetShipmentWithConsignee("con2");
			var emptyShipment = GetShipment("Empty");

			OrgHeader party1 = GetOrgHeader("party1");
			OrgHeader party2 = GetOrgHeader("party2");
			OrgHeader party3 = GetOrgHeader("party3");
			OrgHeader emptyParty = GetOrgHeader("empty");

			OrgRelatedParty relatedParty1 = GetOrgRelatedParty(shipment1.Consignee, party1);
			OrgRelatedParty relatedParty2 = GetOrgRelatedParty(shipment1.Consignee, party2);
			OrgRelatedParty relatedParty3 = GetOrgRelatedParty(shipment2.Consignee, party2);
			OrgRelatedParty relatedParty4 = GetOrgRelatedParty(shipment2.Consignee, party3);

			var newShipment1 = GetShipmentWithConsignee("consignee1");
			var newShipment2 = GetShipmentWithConsignee("consignee2");
			var newShipment3 = GetShipmentWithConsignee("consignee3");
			var newShipment4 = GetShipmentWithConsignee("consignee4");
			var newShipment5 = GetShipmentWithConsignee("consignee5");
			var newShipment6 = GetShipmentWithConsignee("consignee6");

			OrgHeader newParty = GetOrgHeader("newParty");

			OrgRelatedParty newRelatedParty1 = GetOrgRelatedParty(newShipment1.Consignee, newParty);
			OrgRelatedParty newRelatedParty2 = GetOrgRelatedParty(newShipment2.Consignee, newParty);
			OrgRelatedParty newRelatedParty3 = GetOrgRelatedParty(newShipment3.Consignee, newParty);
			OrgRelatedParty newRelatedParty4 = GetOrgRelatedParty(newShipment4.Consignee, newParty);
			OrgRelatedParty newRelatedParty5 = GetOrgRelatedParty(newShipment5.Consignee, newParty);
			OrgRelatedParty newRelatedParty6 = GetOrgRelatedParty(newShipment6.Consignee, newParty);

			newRelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;

			newRelatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;

			newRelatedParty3.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty3.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;

			newRelatedParty4.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty4.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;

			newRelatedParty5.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty5.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty5.PR_FreightTransportMode = Constants.TransportModes.Sea;
			newRelatedParty5.PR_FreightContainerMode = Constants.ContainerModes.FCL;

			newRelatedParty6.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty6.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty6.PR_FreightTransportMode = Constants.TransportModes.Sea;
			newRelatedParty6.PR_FreightContainerMode = Constants.ContainerModes.LCL;

			Factory.Save();

			Asserter.AddFieldOfInterest("JS_UniqueConsignRef");

			OrgRelatedPartiesModuleFilter filter = (OrgRelatedPartiesModuleFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.ConsigneeRelatedParties];

			Asserter.AssertMatches("Empty Filter", filter, shipment1, shipment2, emptyShipment, newShipment1, newShipment2, newShipment3, newShipment4, newShipment5, newShipment6);

			filter.RelatedParty = party1.PK;
			Asserter.AssertMatches("party1", filter, shipment1);

			filter.RelatedParty = party2.PK;
			Asserter.AssertMatches("party2", filter, shipment1, shipment2);

			filter.RelatedParty = party3.PK;
			Asserter.AssertMatches("party3", filter, shipment2);

			filter.RelatedParty = emptyParty.PK;
			Asserter.AssertMatches("empty", filter);

			filter.RelatedParty = newParty.PK;
			Asserter.AssertMatches("newParty", filter, newShipment1, newShipment2, newShipment3, newShipment4, newShipment5, newShipment6);

			filter.PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			Asserter.AssertMatches("PartyType = APNettingGroup", filter, newShipment1);

			filter.PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			Asserter.AssertMatches("PartyType = APSettlementGroup", filter, newShipment2, newShipment3, newShipment4, newShipment5, newShipment6);

			filter.Direction = RelatedPartyDirectionList.Codes.Delivery;
			Asserter.AssertMatches("Direction = Delivery", filter, newShipment3);

			filter.Direction = RelatedPartyDirectionList.Codes.Pickup;
			Asserter.AssertMatches("Direction = Pickup", filter, newShipment4, newShipment5, newShipment6);

			filter.TransportMode = Constants.TransportModes.Sea;
			filter.ContainerMode = Constants.ContainerModes.FCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = FCL", filter, newShipment5);

			filter.TransportMode = Constants.TransportModes.Sea;
			filter.ContainerMode = Constants.ContainerModes.LCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = LCL", filter, newShipment6);
		}

		public void TestLocalClientRelatedPartiesFilter()
		{
			var shipment1 = GetShipmentWithLocalClient("client1");
			var shipment2 = GetShipmentWithLocalClient("client2");
			var emptyShipment = GetShipment("Empty");

			OrgHeader party1 = GetOrgHeader("party1");
			OrgHeader party2 = GetOrgHeader("party2");
			OrgHeader party3 = GetOrgHeader("party3");
			OrgHeader emptyParty = GetOrgHeader("empty");

			OrgRelatedParty relatedParty1 = GetOrgRelatedParty(shipment1.Job.LocalCharges, party1);
			OrgRelatedParty relatedParty2 = GetOrgRelatedParty(shipment1.Job.LocalCharges, party2);
			OrgRelatedParty relatedParty3 = GetOrgRelatedParty(shipment2.Job.LocalCharges, party2);
			OrgRelatedParty relatedParty4 = GetOrgRelatedParty(shipment2.Job.LocalCharges, party3);

			var newShipment1 = GetShipmentWithLocalClient("newClient1");
			var newShipment2 = GetShipmentWithLocalClient("newClient2");
			var newShipment3 = GetShipmentWithLocalClient("newClient3");
			var newShipment4 = GetShipmentWithLocalClient("newClient4");
			var newShipment5 = GetShipmentWithLocalClient("newClient5");
			var newShipment6 = GetShipmentWithLocalClient("newClient6");

			OrgHeader newParty = GetOrgHeader("newParty");

			OrgRelatedParty newRelatedParty1 = GetOrgRelatedParty(newShipment1.Job.LocalCharges, newParty);
			OrgRelatedParty newRelatedParty2 = GetOrgRelatedParty(newShipment2.Job.LocalCharges, newParty);
			OrgRelatedParty newRelatedParty3 = GetOrgRelatedParty(newShipment3.Job.LocalCharges, newParty);
			OrgRelatedParty newRelatedParty4 = GetOrgRelatedParty(newShipment4.Job.LocalCharges, newParty);
			OrgRelatedParty newRelatedParty5 = GetOrgRelatedParty(newShipment5.Job.LocalCharges, newParty);
			OrgRelatedParty newRelatedParty6 = GetOrgRelatedParty(newShipment6.Job.LocalCharges, newParty);

			newRelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.APNettingGroup;

			newRelatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;

			newRelatedParty3.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty3.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;

			newRelatedParty4.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty4.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;

			newRelatedParty5.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty5.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty5.PR_FreightTransportMode = Constants.TransportModes.Sea;
			newRelatedParty5.PR_FreightContainerMode = Constants.ContainerModes.FCL;

			newRelatedParty6.PR_PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			newRelatedParty6.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
			newRelatedParty6.PR_FreightTransportMode = Constants.TransportModes.Sea;
			newRelatedParty6.PR_FreightContainerMode = Constants.ContainerModes.LCL;

			Factory.Save();

			OrgRelatedPartiesModuleFilter filter = (OrgRelatedPartiesModuleFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.LocalClientRelatedParties];

			Asserter.AssertMatches("Empty Filter", filter, shipment1, shipment2, emptyShipment, newShipment1, newShipment2, newShipment3, newShipment4, newShipment5, newShipment6);

			filter.RelatedParty = party1.PK;
			Asserter.AssertMatches("party1", filter, shipment1);

			filter.RelatedParty = party2.PK;
			Asserter.AssertMatches("party2", filter, shipment1, shipment2);

			filter.RelatedParty = party3.PK;
			Asserter.AssertMatches("party3", filter, shipment2);

			filter.RelatedParty = emptyParty.PK;
			Asserter.AssertMatches("empty", filter);

			filter.RelatedParty = newParty.PK;
			Asserter.AssertMatches("newParty", filter, newShipment1, newShipment2, newShipment3, newShipment4, newShipment5, newShipment6);

			filter.PartyType = RelatedPartyTypeList.Codes.APNettingGroup;
			Asserter.AssertMatches("PartyType = APNettingGroup", filter, newShipment1);

			filter.PartyType = RelatedPartyTypeList.Codes.APSettlementGroup;
			Asserter.AssertMatches("PartyType = APSettlementGroup", filter, newShipment2, newShipment3, newShipment4, newShipment5, newShipment6);

			filter.Direction = RelatedPartyDirectionList.Codes.Delivery;
			Asserter.AssertMatches("Direction = Delivery", filter, newShipment3);

			filter.Direction = RelatedPartyDirectionList.Codes.Pickup;
			Asserter.AssertMatches("Direction = Pickup", filter, newShipment4, newShipment5, newShipment6);

			filter.TransportMode = Constants.TransportModes.Sea;
			filter.ContainerMode = Constants.ContainerModes.FCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = FCL", filter, newShipment5);

			filter.TransportMode = Constants.TransportModes.Sea;
			filter.ContainerMode = Constants.ContainerModes.LCL;
			Asserter.AssertMatches("TransportMode = SEA, ContainerMode = LCL", filter, newShipment6);
		}

		#endregion

		public void TestConsignorConsigneeFilter()
		{
			OrgHeader orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "6XP";

			OrgHeader orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "BBG";

			OrgHeader orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_Code = "7XP";

			OrgHeader orgHeader4 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader4.OH_Code = "BBZ";

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.ConsignorPK = orgHeader1.PK;
			shipment1.ConsigneePK = orgHeader3.PK;

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.ConsignorPK = orgHeader2.PK;
			shipment2.ConsigneePK = orgHeader4.PK;

			Factory.Save();

			JobShipmentFilterBusinessObject filter = (JobShipmentFilterBusinessObject)GetNewFilterStripBusinessObject();
			ModuleGuidsFilter orgFilter = (ModuleGuidsFilter)filter[JobShipmentFilterBusinessObject.Descriptions.ConsignorConsignee];
			orgFilter.Property1 = orgHeader1.PK;
			orgFilter.IsActive = true;

			ForwardingShipmentCollection collection = new ForwardingShipmentCollection(Factory);
			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);
			AssertCollectionContains(shipment1, collection);

			orgFilter.Property2 = orgHeader3.PK;
			orgFilter.IsActive = true;
			collection.Load(filter.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(shipment1, collection);

			orgFilter.Property1 = orgHeader2.PK;
			orgFilter.Property2 = Guid.Empty;
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);
			AssertCollectionContains(shipment2, collection);

			orgFilter.Property2 = orgHeader4.PK;
			orgFilter.IsActive = true;

			collection.Load(filter.Filter);

			AssertEquals(1, collection.Count);
			AssertCollectionContains(shipment2, collection);

			orgFilter.Property1 = Guid.NewGuid();
			orgFilter.IsActive = true;
			collection.Load(filter.Filter);
			AssertEquals(0, collection.Count);

			orgFilter.Property1 = Guid.Empty;
			orgFilter.Property2 = Guid.NewGuid();
			orgFilter.IsActive = true;
			collection.Load(filter.Filter);
			AssertEquals(0, collection.Count);
		}

		public void TestCombineConsignorAndConsigneeFilter()
		{
			var orgHeader1 = GetOrgHeader("APRIS");
			orgHeader1.OH_IsConsignee = true;

			var orgHeader2 = GetOrgHeader("KYZZZ");
			orgHeader2.OH_IsConsignor = true;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = orgHeader1.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = orgHeader2.MainAddress.PK;

			Factory.Save();

			var consigneeFilter1 = (ModuleGuidFilter)FilterStripBizO["Consignee"];
			consigneeFilter1.Property = orgHeader1.PK;
			consigneeFilter1.IsActive = true;

			var consigneeFilter2 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor("Consignor");
			consigneeFilter2.Property = orgHeader2.PK;
			consigneeFilter2.IsActive = true;

			var shipments = new ForwardingShipmentCollection(Factory);
			shipments.Load(FilterStripBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { shipment }, shipments);
		}

		#region AssertDocAddressMultiValueQuery

		void AssertDocAddressMultiValueQuery_Equal(ZString docAddressName, ZString moduleGuidFilterName)
		{
			var orgHeader1 = GetOrgHeader("APRIS");
			orgHeader1.OH_IsConsignee = true;
			orgHeader1.OH_IsForwarder = true;
			orgHeader1.OH_IsConsignor = true;

			var orgHeader2 = GetOrgHeader("KYZZZ");
			orgHeader2.OH_IsConsignee = true;
			orgHeader2.OH_IsForwarder = true;
			orgHeader2.OH_IsConsignor = true;

			var orgHeader3 = GetOrgHeader("ABCDE");
			orgHeader3.OH_IsConsignee = true;
			orgHeader3.OH_IsForwarder = true;
			orgHeader3.OH_IsConsignor = false;
			orgHeader3.OH_IsActive = false;

			var orgHeader4 = GetOrgHeader("XYZAB");
			orgHeader4.OH_IsConsignee = true;
			orgHeader4.OH_IsForwarder = true;
			orgHeader4.OH_IsConsignor = true;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var address1 = shipment1[docAddressName] as JobDocAddress;
			address1.OrganisationPK = orgHeader1.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var address2 = shipment2[docAddressName] as JobDocAddress;
			address2.OrganisationPK = orgHeader2.PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var address3 = shipment3[docAddressName] as JobDocAddress;
			address3.OrganisationPK = orgHeader3.PK;

			var shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			var address4 = shipment4[docAddressName] as JobDocAddress;
			address4.OrganisationPK = orgHeader4.PK;

			Factory.Save();

			var filter1 = (ModuleGuidFilter)FilterStripBizO[moduleGuidFilterName];
			filter1.Property = orgHeader1.PK;
			filter1.IsActive = true;

			var filter2 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(moduleGuidFilterName);
			filter2.Property = orgHeader2.PK;
			filter2.IsActive = true;

			var filter3 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(moduleGuidFilterName);
			filter3.Property = orgHeader3.PK;
			filter3.IsActive = true;

			filter1.OrCategory = FilterOrCategory.Red;
			filter2.OrCategory = FilterOrCategory.Red;
			filter3.OrCategory = FilterOrCategory.Red;

			AssertContains("Generated filter SQL",
				"JS_PK IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_AddressType = @CWO3_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE (OA_OH in (@CWO4_, @CWO5_, @CWO6_))))",
				FilterStripBizO.Filter.FilterString);

			var shipments = new ForwardingShipmentCollection(Factory);
			shipments.Load(FilterStripBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2, shipment3 }, shipments);

			var pickupAgentFilter4 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(moduleGuidFilterName);
			pickupAgentFilter4.Property = orgHeader4.PK;
			pickupAgentFilter4.IsActive = true;
			pickupAgentFilter4.OrCategory = FilterOrCategory.Red;

			AssertContains("Generated filter SQL",
				"JS_PK IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_AddressType = @CWO3_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE (OA_OH in (@CWO4_, @CWO5_, @CWO6_, @CWO7_))))",
				FilterStripBizO.Filter.FilterString);

			shipments.Load(FilterStripBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2, shipment3, shipment4 }, shipments);
		}

		void AssertDocAddressMultiValueQuery_NotEqual(ZString docAddressName, ZString moduleGuidFilterName)
		{
			var orgHeader1 = GetOrgHeader("APRIS");
			orgHeader1.OH_IsConsignee = true;
			orgHeader1.OH_IsForwarder = true;
			orgHeader1.OH_IsConsignor = true;

			var orgHeader2 = GetOrgHeader("KYZZZ");
			orgHeader2.OH_IsConsignee = true;
			orgHeader2.OH_IsForwarder = true;
			orgHeader2.OH_IsConsignor = true;

			var orgHeader3 = GetOrgHeader("ABCDE");
			orgHeader3.OH_IsConsignee = true;
			orgHeader3.OH_IsForwarder = true;
			orgHeader3.OH_IsConsignor = false;
			orgHeader3.OH_IsActive = false;

			var orgHeader4 = GetOrgHeader("XYZAB");
			orgHeader4.OH_IsConsignee = true;
			orgHeader4.OH_IsForwarder = true;
			orgHeader4.OH_IsConsignor = true;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var address1 = shipment1[docAddressName] as JobDocAddress;
			address1.OrganisationPK = orgHeader1.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var address2 = shipment2[docAddressName] as JobDocAddress;
			address2.OrganisationPK = orgHeader2.PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var address3 = shipment3[docAddressName] as JobDocAddress;
			address3.OrganisationPK = orgHeader3.PK;

			var shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			var address4 = shipment4[docAddressName] as JobDocAddress;
			address4.OrganisationPK = orgHeader4.PK;

			Factory.Save();

			var filter1 = (ModuleGuidFilter)FilterStripBizO[moduleGuidFilterName];
			filter1.Property = orgHeader1.PK;
			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter1.IsActive = true;

			var filter2 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(moduleGuidFilterName);
			filter2.Property = orgHeader2.PK;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter2.IsActive = true;

			AssertContains("Generated filter SQL",
				"JS_PK IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_AddressType = @CWO1_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE (OA_OH not in (@CWO2_, @CWO3_))))",
				FilterStripBizO.Filter.FilterString);

			var shipments = new ForwardingShipmentCollection(Factory);
			shipments.Load(FilterStripBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { shipment3, shipment4 }, shipments);

			var filter3 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(moduleGuidFilterName);
			filter3.Property = orgHeader3.PK;
			filter3.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter3.IsActive = true;

			var filter4 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(moduleGuidFilterName);
			filter4.Property = orgHeader4.PK;
			filter4.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter4.IsActive = true;

			AssertContains("Generated filter SQL",
				"JS_PK IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_AddressType = @CWO1_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE (OA_OH not in (@CWO2_, @CWO3_, @CWO4_, @CWO5_))))",
				FilterStripBizO.Filter.FilterString);

			shipments.Load(FilterStripBizO.Filter);

			AssertEquals(0, shipments.Count);
		}

		void AssertDocAddressMultiValueQuery_OtherScenario(ZString moduleGuidFilterName)
		{
			// The CanGroup condition for ModuleTextFilter is
			// OrCategory != FilterOrCategory.None && SqlComparisonOperator == SQLComparisonOperator.Equal || OrCategory == FilterOrCategory.None && SqlComparisonOperator == SQLComparisonOperator.NotEqual

			var orgHeader1 = GetOrgHeader("APRIS");
			orgHeader1.OH_IsConsignee = true;
			orgHeader1.OH_IsForwarder = true;
			orgHeader1.OH_IsConsignor = true;

			var orgHeader2 = GetOrgHeader("KYZZZ");
			orgHeader2.OH_IsConsignee = true;
			orgHeader2.OH_IsForwarder = true;
			orgHeader2.OH_IsConsignor = true;

			Factory.Save();

			var filter1 = (ModuleGuidFilter)FilterStripBizO[moduleGuidFilterName];
			filter1.Property = orgHeader1.PK;
			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter1.IsActive = true;
			filter1.OrCategory = FilterOrCategory.Red;

			var filter2 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(moduleGuidFilterName);
			filter2.Property = orgHeader2.PK;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter2.IsActive = true;
			filter1.OrCategory = FilterOrCategory.Red;

			AssertContains("Generated filter SQL",
				"(JS_PK IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_AddressType = @CWO1_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH <> @CWO2_)))",
				FilterStripBizO.Filter.FilterString);
			AssertContains("Generated filter SQL",
				"(JS_PK IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_AddressType = @CWO1_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH <> @CWO6_)))",
				FilterStripBizO.Filter.FilterString);

			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

			AssertEquals("Generated filter SQL", 2,
				Regex.Matches(FilterStripBizO.Filter.FilterString, Regex.Escape("(JS_PK NOT IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_AddressType = @CWO1_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress)))")).Count);

			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;

			AssertEquals("Generated filter SQL", 2,
				Regex.Matches(FilterStripBizO.Filter.FilterString, Regex.Escape("(JS_PK IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_AddressType = @CWO1_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress)))")).Count);

			filter1.OrCategory = FilterOrCategory.None;
			filter2.OrCategory = FilterOrCategory.None;

			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

			AssertContains("Generated filter SQL",
				"(JS_PK IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_AddressType = @CWO1_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH = @CWO2_)))",
				FilterStripBizO.Filter.FilterString);
			AssertContains("Generated filter SQL",
				"(JS_PK IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_AddressType = @CWO1_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH = @CWO4_)))",
				FilterStripBizO.Filter.FilterString);

			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

			AssertEquals("Generated filter SQL", 2,
				Regex.Matches(FilterStripBizO.Filter.FilterString, Regex.Escape("(JS_PK NOT IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_AddressType = @CWO1_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress)))")).Count);

			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;

			AssertEquals("Generated filter SQL", 2,
				Regex.Matches(FilterStripBizO.Filter.FilterString, Regex.Escape("(JS_PK IN (SELECT E2_ParentID FROM dbo.JobDocAddress WHERE E2_AddressType = @CWO1_ and E2_OA_Address IN (SELECT OA_PK FROM dbo.OrgAddress)))")).Count);
		}

		#endregion

		#region AssertCartageCompanyMultiValueQuery

		void AssertCartageCompanyMultiValueQuery_Equal(ZString cartageCoAddressName, ZString moduleGuidFilterName, ZString schemaName)
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "AAAA";
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "BBBB";
			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_Code = "CCCC";
			orgHeader3.OH_IsActive = false;
			var orgHeader4 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader4.OH_Code = "DDDD";

			var orgAddr1 = orgHeader1.MainAddress;
			var orgAddr2 = orgHeader2.MainAddress;
			var orgAddr3 = orgHeader3.MainAddress;
			var orgAddr4 = orgHeader4.MainAddress;

			shipment1.DocsAndCartage[cartageCoAddressName] = orgAddr1.PK;
			shipment2.DocsAndCartage[cartageCoAddressName] = orgAddr2.PK;
			shipment3.DocsAndCartage[cartageCoAddressName] = orgAddr3.PK;
			shipment4.DocsAndCartage[cartageCoAddressName] = orgAddr4.PK;

			Factory.Save();

			var filter1 = (ModuleGuidFilter)FilterStripBizO[moduleGuidFilterName];
			filter1.Property = orgHeader1.PK;
			filter1.IsActive = true;

			var filter2 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(moduleGuidFilterName);
			filter2.Property = orgHeader2.PK;
			filter2.IsActive = true;

			var filter3 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(moduleGuidFilterName);
			filter3.Property = orgHeader3.PK;
			filter3.IsActive = true;

			filter1.OrCategory = FilterOrCategory.Red;
			filter2.OrCategory = FilterOrCategory.Red;
			filter3.OrCategory = FilterOrCategory.Red;

			AssertContains("Generated filter SQL",
				$"JS_PK IN (SELECT JP_ParentID FROM dbo.JobDocsAndCartage WHERE {schemaName} IN (SELECT OA_PK FROM dbo.OrgAddress WHERE (OA_OH in (@CWO3_, @CWO4_, @CWO5_))))",
				FilterStripBizO.Filter.FilterString);

			var shipments = new ForwardingShipmentCollection(Factory);
			shipments.Load(FilterStripBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2, shipment3 }, shipments);

			var pickupAgentFilter4 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(moduleGuidFilterName);
			pickupAgentFilter4.Property = orgHeader4.PK;
			pickupAgentFilter4.IsActive = true;
			pickupAgentFilter4.OrCategory = FilterOrCategory.Red;

			AssertContains("Generated filter SQL",
				$"JS_PK IN (SELECT JP_ParentID FROM dbo.JobDocsAndCartage WHERE {schemaName} IN (SELECT OA_PK FROM dbo.OrgAddress WHERE (OA_OH in (@CWO3_, @CWO4_, @CWO5_, @CWO6_))))",
				FilterStripBizO.Filter.FilterString);

			shipments.Load(FilterStripBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2, shipment3, shipment4 }, shipments);
		}

		void AssertCartageCompanyMultiValueQuery_NotEqual(ZString cartageCoAddressName, ZString moduleGuidFilterName, ZString schemaName)
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "AAAA";
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "BBBB";
			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader3.OH_Code = "CCCC";
			orgHeader3.OH_IsActive = false;
			var orgHeader4 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader4.OH_Code = "DDDD";

			var orgAddr1 = orgHeader1.MainAddress;
			var orgAddr2 = orgHeader2.MainAddress;
			var orgAddr3 = orgHeader3.MainAddress;
			var orgAddr4 = orgHeader4.MainAddress;

			shipment1.DocsAndCartage[cartageCoAddressName] = orgAddr1.PK;
			shipment2.DocsAndCartage[cartageCoAddressName] = orgAddr2.PK;
			shipment3.DocsAndCartage[cartageCoAddressName] = orgAddr3.PK;
			shipment4.DocsAndCartage[cartageCoAddressName] = orgAddr4.PK;

			Factory.Save();

			var filter1 = (ModuleGuidFilter)FilterStripBizO[moduleGuidFilterName];
			filter1.Property = orgHeader1.PK;
			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter1.IsActive = true;

			var filter2 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(moduleGuidFilterName);
			filter2.Property = orgHeader2.PK;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter2.IsActive = true;

			AssertContains("Generated filter SQL",
				$"JS_PK IN (SELECT JP_ParentID FROM dbo.JobDocsAndCartage WHERE {schemaName} IN (SELECT OA_PK FROM dbo.OrgAddress WHERE (OA_OH not in (@CWO1_, @CWO2_))))",
				FilterStripBizO.Filter.FilterString);

			var shipments = new ForwardingShipmentCollection(Factory);
			shipments.Load(FilterStripBizO.Filter);

			AssertContainsExactElementsInAnyOrder(new[] { shipment3, shipment4 }, shipments);

			var filter3 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(moduleGuidFilterName);
			filter3.Property = orgHeader3.PK;
			filter3.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter3.IsActive = true;

			var filter4 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(moduleGuidFilterName);
			filter4.Property = orgHeader4.PK;
			filter4.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter4.IsActive = true;

			AssertContains("Generated filter SQL",
				$"JS_PK IN (SELECT JP_ParentID FROM dbo.JobDocsAndCartage WHERE {schemaName} IN (SELECT OA_PK FROM dbo.OrgAddress WHERE (OA_OH not in (@CWO1_, @CWO2_, @CWO3_, @CWO4_))))",
				FilterStripBizO.Filter.FilterString);

			shipments.Load(FilterStripBizO.Filter);

			AssertEquals(0, shipments.Count);
		}

		void AssertCartageCompanyMultiValueQuery_OtherScenario(ZString moduleGuidFilterName, ZString schemaName)
		{
			// The CanGroup condition for ModuleTextFilter is
			// OrCategory != FilterOrCategory.None && SqlComparisonOperator == SQLComparisonOperator.Equal || OrCategory == FilterOrCategory.None && SqlComparisonOperator == SQLComparisonOperator.NotEqual

			var orgHeader1 = GetOrgHeader("APRIS");
			orgHeader1.OH_IsConsignee = true;
			orgHeader1.OH_IsForwarder = true;
			orgHeader1.OH_IsConsignor = true;

			var orgHeader2 = GetOrgHeader("KYZZZ");
			orgHeader2.OH_IsConsignee = true;
			orgHeader2.OH_IsForwarder = true;
			orgHeader2.OH_IsConsignor = true;

			Factory.Save();

			var filter1 = (ModuleGuidFilter)FilterStripBizO[moduleGuidFilterName];
			filter1.Property = orgHeader1.PK;
			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter1.IsActive = true;
			filter1.OrCategory = FilterOrCategory.Red;

			var filter2 = (ModuleGuidFilter)FilterStripBizO.CreateDuplicateFor(moduleGuidFilterName);
			filter2.Property = orgHeader2.PK;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter2.IsActive = true;
			filter1.OrCategory = FilterOrCategory.Red;

			AssertContains("Generated filter SQL",
				$"JS_PK IN (SELECT JP_ParentID FROM dbo.JobDocsAndCartage WHERE {schemaName} IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH <> @CWO1_))",
				FilterStripBizO.Filter.FilterString);
			AssertContains("Generated filter SQL",
				$"JS_PK IN (SELECT JP_ParentID FROM dbo.JobDocsAndCartage WHERE {schemaName} IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH <> @CWO4_))",
				FilterStripBizO.Filter.FilterString);

			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

			AssertEquals("Generated filter SQL", 2,
				Regex.Matches(FilterStripBizO.Filter.FilterString, Regex.Escape($"JS_PK NOT IN (SELECT JP_ParentID FROM dbo.JobDocsAndCartage WHERE {schemaName} IN (SELECT OA_PK FROM dbo.OrgAddress))")).Count);

			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;

			AssertEquals("Generated filter SQL", 2,
				Regex.Matches(FilterStripBizO.Filter.FilterString, Regex.Escape($"JS_PK IN (SELECT JP_ParentID FROM dbo.JobDocsAndCartage WHERE {schemaName} IN (SELECT OA_PK FROM dbo.OrgAddress))")).Count);

			filter1.OrCategory = FilterOrCategory.None;
			filter2.OrCategory = FilterOrCategory.None;

			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

			AssertContains("Generated filter SQL",
				$"JS_PK IN (SELECT JP_ParentID FROM dbo.JobDocsAndCartage WHERE {schemaName} IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH = @CWO1_))",
				FilterStripBizO.Filter.FilterString);
			AssertContains("Generated filter SQL",
				$"JS_PK IN (SELECT JP_ParentID FROM dbo.JobDocsAndCartage WHERE {schemaName} IN (SELECT OA_PK FROM dbo.OrgAddress WHERE OA_OH = @CWO2_))",
				FilterStripBizO.Filter.FilterString);

			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

			AssertEquals("Generated filter SQL", 2,
				Regex.Matches(FilterStripBizO.Filter.FilterString, Regex.Escape($"JS_PK NOT IN (SELECT JP_ParentID FROM dbo.JobDocsAndCartage WHERE {schemaName} IN (SELECT OA_PK FROM dbo.OrgAddress))")).Count);

			filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;

			AssertEquals("Generated filter SQL", 2,
				Regex.Matches(FilterStripBizO.Filter.FilterString, Regex.Escape($"JS_PK IN (SELECT JP_ParentID FROM dbo.JobDocsAndCartage WHERE {schemaName} IN (SELECT OA_PK FROM dbo.OrgAddress))")).Count);
		}

		#endregion

		#endregion

		#region TestCustomsBroker

		public void TestCustomsBroker()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
				ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
				ForwardingShipment shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
				ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory);

				GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
				staff1.GS_Code = "AAA";
				GlbStaff staff2 = GlbStaff.CurrentUser;

				BusinessObject jobDeclaration = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
				jobDeclaration[JobDeclarationSchema.JE_GS_NKCusAgent] = staff1.GS_Code;
				jobDeclaration[JobDeclarationSchema.JE_JS] = shipment1.PK;

				BusinessObject jobDeclaration2 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
				jobDeclaration2[JobDeclarationSchema.JE_GS_NKCusAgent] = staff2.GS_Code;
				jobDeclaration2[JobDeclarationSchema.JE_JS] = shipment2.PK;

				BusinessObject jobDeclaration3 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
				jobDeclaration3[JobDeclarationSchema.JE_JS] = shipment3.PK;

				JobShipmentFilterBusinessObject filterStripBizOSG = new JobShipmentFilterBusinessObject();
				Factory.Save();
				ModuleNkFilter filter = (ModuleNkFilter)filterStripBizOSG["Customs Broker"];

				filter.IsActive = true;
				AssertEquals("The Customs Broker filter is expected to use comparison operators.", true, filter.HasComparisonOperator);

				string[] expectedOperators = new string[7];
				expectedOperators[0] = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				expectedOperators[1] = ModuleTextFilter.ComparisonConstants.IsBlank;
				expectedOperators[2] = ModuleTextFilter.ComparisonConstants.IsNotBlank;
				expectedOperators[3] = ModuleTextFilter.ComparisonConstants.NotEqual;
				expectedOperators[4] = ModuleTextFilter.ComparisonConstants.Exact;
				expectedOperators[5] = ModuleTextFilter.ComparisonConstants.CurrentUser;
				expectedOperators[6] = string.Empty;
				AssertContainsExactElementsInAnyOrder(expectedOperators, filter.AllowedComparisonOperators);

				AssertEquals("Precondition: default operator is equals", SQLComparisonOperator.Equal, filter.SqlComparisonOperator);
				filter.Property = staff1.GS_Code;
				shipments.Load(filterStripBizOSG.Filter);
				AssertContainsExactElementsInAnyOrder("When using the Equal operator, only shipments with a matching Customs Broker should be returned",
					new ForwardingShipment[] { shipment1 },
					shipments);

				filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
				shipments.Load(filterStripBizOSG.Filter);
				AssertContainsExactElementsInAnyOrder("When using the NotEqual operator, all shipments EXCEPT ones with the matching Customs Broker are expected",
					new ForwardingShipment[] { Shipment1, Shipment2, Shipment3, Shipment4, Shipment5, Shipment6, shipment2, shipment3 },
					shipments);

				filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
				shipments.Load(filterStripBizOSG.Filter);
				AssertContainsExactElementsInAnyOrder("When using the IsBlank operator, all shipment without a Customs Broker are expected",
					new ForwardingShipment[] { Shipment1, Shipment2, Shipment3, Shipment4, Shipment5, Shipment6, shipment3 },
					shipments);

				filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
				shipments.Load(filterStripBizOSG.Filter);
				AssertContainsExactElementsInAnyOrder("When using the IsNotBlank operator, only shipments with a Customs Broker should be included",
					new ForwardingShipment[] { shipment1, shipment2 },
					shipments);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.CurrentUser;
				shipments.Load(filterStripBizOSG.Filter);
				AssertContainsExactElementsInAnyOrder("When using the CurrentUser operator, only shipments where the Customs Broker is the current user should be included",
					new ForwardingShipment[] { shipment2 },
					shipments);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				filter.SelectedFilters.AddTextFilterStrip("Code", staff1.GS_Code);
				shipments.Load(filterStripBizOSG.Filter);
				AssertContainsExactElementsInAnyOrder("When using the FiltersMatch operator, shipments matching the selected filters are included",
					new ForwardingShipment[] { shipment1 },
					shipments);
			}
		}

		#endregion

		#region Test Cartage Coordinator Filter

		public void TestCartageCoordinatorFilter()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff2 = GlbStaff.CurrentUser;
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.OH_Code = "XXXYYY";
			org2.OH_Code = "XXXXXX";

			org1.OH_IsConsignee = true;
			org2.OH_IsConsignor = true;
			AssignStaff(org1, staff1, "TIA", StaffAssignmentRoles.Codes.CartageCoordinator);
			AssignStaff(org2, staff2, "TIA", StaffAssignmentRoles.Codes.CartageCoordinator);

			// shipment1 is associated with staff1 and staff2
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.ConsigneeDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			shipment1.ConsignorDocumentaryAddress.E2_OA_Address = org2.MainAddress.PK;
			JobDocAddress jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_AddressType = DocAddressTypes.GetCode(Factory, DocAddressType.ConsigneePickupDeliveryAddress);
			jobDocAddress.E2_OA_Address = org1.MainAddress.PK;
			jobDocAddress.E2_ParentID = shipment1.PK;
			JobDocAddress jobDocAddress2 = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress2.E2_AddressType = DocAddressTypes.GetCode(Factory, DocAddressType.ConsignorPickupDeliveryAddress);
			jobDocAddress2.E2_OA_Address = org2.MainAddress.PK;
			jobDocAddress2.E2_ParentID = shipment1.PK;

			// shipment2 is associated with staff1
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.ConsigneeDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			JobDocAddress jobDocAddress3 = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress3.E2_AddressType = DocAddressTypes.GetCode(Factory, DocAddressType.ConsigneePickupDeliveryAddress);
			jobDocAddress3.E2_OA_Address = org1.MainAddress.PK;
			jobDocAddress3.E2_ParentID = shipment2.PK;

			// shipment3 is not associated with staff1 or staff2
			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory);

			JobShipmentFilterBusinessObject filterStripBizOSG = new JobShipmentFilterBusinessObject();
			Factory.Save();
			ModuleNkFilter filter = (ModuleNkFilter)filterStripBizOSG["Cartage Coordinator"];
			filter.IsActive = true;
			filter.Property = staff1.GS_Code;
			shipments.Load(filterStripBizOSG.Filter);
			AssertContainsExactElementsInAnyOrder(new ForwardingShipment[] { shipment1, shipment2 }, shipments);

			filter.Property = staff2.GS_Code;
			shipments.Load(filterStripBizOSG.Filter);
			AssertContainsExactElementsInAnyOrder(new ForwardingShipment[] { shipment1 }, shipments);

			AssertEquals("The Cartage Coordinator filter is expected to use comparison operators.", true, filter.HasComparisonOperator);

			string[] expectedOperators = new string[7];
			expectedOperators[0] = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			expectedOperators[1] = ModuleTextFilter.ComparisonConstants.IsBlank;
			expectedOperators[2] = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			expectedOperators[3] = ModuleTextFilter.ComparisonConstants.NotEqual;
			expectedOperators[4] = ModuleTextFilter.ComparisonConstants.Exact;
			expectedOperators[5] = ModuleTextFilter.ComparisonConstants.CurrentUser;
			expectedOperators[6] = string.Empty;
			AssertContainsExactElementsInAnyOrder(expectedOperators, filter.AllowedComparisonOperators);

			AssertEquals("Precondition: default operator is equals", SQLComparisonOperator.Equal, filter.SqlComparisonOperator);
			filter.Property = staff1.GS_Code;
			shipments.Load(filterStripBizOSG.Filter);
			AssertContainsExactElementsInAnyOrder("When using the Equal operator, only shipments with a matching Cartage Coordinator should be returned",
				new ForwardingShipment[] { shipment1, shipment2 },
				shipments);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = staff2.GS_Code;
			shipments.Load(filterStripBizOSG.Filter);
			AssertContainsExactElementsInAnyOrder("When using the NotEqual operator, all shipments EXCEPT ones with the matching Cartage Coordinator are expected",
				new ForwardingShipment[] { Shipment1, Shipment2, Shipment3, Shipment4, Shipment5, Shipment6, shipment2, shipment3 },
				shipments);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			shipments.Load(filterStripBizOSG.Filter);
			AssertContainsExactElementsInAnyOrder("When using the IsBlank operator, no shipments are expected to be returned since there is a constraint preventing blank values for O8_GS_NKPersonResponsible.",
				Array.Empty<ForwardingShipment>(),
				shipments);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			shipments.Load(filterStripBizOSG.Filter);
			AssertContainsExactElementsInAnyOrder("When using the IsNotBlank operator, only shipments with a Cartage Coordinator should be included",
				new ForwardingShipment[] { shipment1, shipment2 },
				shipments);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.CurrentUser;
			shipments.Load(filterStripBizOSG.Filter);
			AssertContainsExactElementsInAnyOrder("When using the CurrentUser operator, only shipments where the Cartage Coordinator is the current user should be included",
				new ForwardingShipment[] { shipment1 },
				shipments);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			filter.SelectedFilters.AddTextFilterStrip("Code", staff1.GS_Code);
			shipments.Load(filterStripBizOSG.Filter);
			AssertContainsExactElementsInAnyOrder("When using the FiltersMatch operator, shipments matching the selected filters are included",
				new ForwardingShipment[] { shipment1, shipment2 },
				shipments);
		}

		void AssignStaff(OrgHeader org, GlbStaff staff, ZString department, ZString role)
		{
			var assignment = Factory.NewWithValidTestData<OrgStaffAssignments>();
			assignment.O8_OH = org.PK;
			assignment.O8_GS_NKPersonResponsible = staff.GS_Code;
			assignment.O8_Department = department;
			assignment.O8_Role = role;
			assignment.O8_GC = GlbCompany.CurrentCompany.PK;
		}

		#endregion

		#region Location Filters

		#region Domestic / International

		public void TestDomesticInternationalFilter()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "ABDEM";

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_RL_NKOrigin = "USMEM";
			shipment2.JS_RL_NKDestination = "USLAX";

			ForwardingShipment shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_RL_NKOrigin = "INBOM";

			Factory.Save();

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			AssertEquals("Shipment1 is in Collection", true, shipments.Contains(shipment1.PK));
			AssertEquals("Shipment2 is in Collection", true, shipments.Contains(shipment2.PK));
			AssertEquals("Shipment3 is in Collection", true, shipments.Contains(shipment3.PK));

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Domestic / International"];
			filter.IsActive = true;

			filter.Property = "DOM";
			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			AssertEquals("Shipment1 is not in Collection", false, shipments.Contains(shipment1.PK));
			AssertEquals("Shipment2 is in Collection", true, shipments.Contains(shipment2.PK));
			AssertEquals("Shipment3 is not in Collection", false, shipments.Contains(shipment3.PK));

			filter.Property = "INT";
			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			AssertEquals("Shipment1 is in Collection", true, shipments.Contains(shipment1.PK));
			AssertEquals("Shipment2 is not in Collection", false, shipments.Contains(shipment2.PK));
			AssertEquals("Shipment3 is in Collection", true, shipments.Contains(shipment3.PK));

			filter.Property = "ALL";
			shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();
			AssertEquals("Shipment1 is in Collection", true, shipments.Contains(shipment1.PK));
			AssertEquals("Shipment2 is in Collection", true, shipments.Contains(shipment2.PK));
			AssertEquals("Shipment3 is in Collection", true, shipments.Contains(shipment3.PK));
		}

		#endregion

		#region TestOriginDestinationFilter

		public void TestOriginDestinationFilter()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";

			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_RL_NKOrigin = "AUBNE";
			shipment2.JS_RL_NKDestination = "SGSIN";

			Factory.Save();

			ModuleLocationFilter shipmentFilter = (ModuleLocationFilter)FilterStripBizO["Origin / Destination"];
			ForwardingShipmentCollection results = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);

			shipmentFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Not filtered - should be included", true, results.Contains(shipment1.PK));
			AssertEquals("Not filtered - should be included", true, results.Contains(shipment2.PK));

			shipmentFilter.Property1 = "AU";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Shipment1 should be in the collection", true, results.Contains(shipment1.PK));
			AssertEquals("Shipment1 should be in the collection", true, results.Contains(shipment2.PK));

			shipmentFilter.Property1 = "AUBNE";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Shipment1 has nothing to do with brisbane", false, results.Contains(shipment1.PK));
			AssertEquals("Shipment2 should be in the collection", true, results.Contains(shipment2.PK));

			shipmentFilter.Property1 = "";
			shipmentFilter.Property2 = "SGSIN";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Shipment1 isn't going to Singapore", false, results.Contains(shipment1.PK));
			AssertEquals("Shipment2 should be in collection", true, results.Contains(shipment2.PK));

			shipmentFilter.Property2 = "US";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Shipment1 should be in collection", true, results.Contains(shipment1.PK));
			AssertEquals("Shipment2 isn't going to US", false, results.Contains(shipment2.PK));

			shipmentFilter.Property1 = "AUSR";
			shipmentFilter.Property2 = "";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Shipment1 should be in AU Zone", true, results.Contains(shipment1.PK));
			AssertEquals("Shipment2 should be in AU Zone", true, results.Contains(shipment2.PK));

			shipmentFilter.Property2 = "USAR";
			results.Load(FilterStripBizO.Filter);
			AssertEquals("Shipment1 should be in US Zone", true, results.Contains(shipment1.PK));
			AssertEquals("Shipment2 should NOT be in US Zone", false, results.Contains(shipment2.PK));
		}

		public virtual void TestLoadDischargeAlwaysVisible()
		{
			bool initialShipment = Env.Security.MaintainShipmentAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed;
			bool initialConsol = Env.Security.MaintainConsolAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed;
			try
			{
				Env.Security.MaintainConsolAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed = false;
				Env.Security.MaintainShipmentAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed = false;
				ResetFilterStripBizO();
				AssertLoadDischargeFilter(true);

				Env.Security.MaintainConsolAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed = true;
				Env.Security.MaintainShipmentAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed = false;
				ResetFilterStripBizO();
				AssertLoadDischargeFilter(true);

				Env.Security.MaintainConsolAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed = false;
				Env.Security.MaintainShipmentAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed = true;
				ResetFilterStripBizO();
				AssertLoadDischargeFilter(false);

				Env.Security.MaintainConsolAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed = true;
				Env.Security.MaintainShipmentAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed = true;
				ResetFilterStripBizO();
				AssertLoadDischargeFilter(false);
			}
			finally
			{
				Env.Security.MaintainConsolAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed = initialConsol;
				Env.Security.MaintainShipmentAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed = initialShipment;
			}
		}

		void AssertLoadDischargeFilter(bool equals)
		{
			ModuleLocationFilter shipmentFilter = (ModuleLocationFilter)FilterStripBizO["Load / Discharge"];
			AssertNotNull(shipmentFilter);
			AssertEquals(equals, shipmentFilter.Visibility == FilterVisibility.AlwaysVisible);
		}
		#endregion

		#region Postcodes

		public void TestDeliveryPostCodeFilters()
		{
			var shipment1 = CreateShipmentWithDeliveryAddress("ABC1", "2045");
			var shipment2 = CreateShipmentWithDeliveryAddress("ABC2", "2000");
			var shipment3 = CreateShipmentWithDeliveryAddress("ABC3", "4325");

			Factory.Save();

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);

			ModuleTextFilter postcodeFilter = (ModuleTextFilter)FilterStripBizO["Delivery Address Post Code"];
			postcodeFilter.IsActive = true;

			postcodeFilter.Property = "2045";
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals(1, shipments.Count);
			AssertContainsExactElementsInAnyOrder(new ForwardingShipment[] { shipment1 }, shipments);

			postcodeFilter.Property = "20";
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals(2, shipments.Count);
			AssertContainsExactElementsInAnyOrder(new ForwardingShipment[] { shipment1, shipment2 }, shipments);

			postcodeFilter.Property = "4325";
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals(1, shipments.Count);
			AssertContainsExactElementsInAnyOrder(new ForwardingShipment[] { shipment3 }, shipments);

			shipment3.ConsigneeDeliveryAddress.E2_AddressOverride = true;
			shipment3.ConsigneeDeliveryAddress.E2_Postcode = "2076";

			Factory.Save();

			postcodeFilter.Property = "20";
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals(3, shipments.Count);
			AssertContainsExactElementsInAnyOrder(new ForwardingShipment[] { shipment1, shipment2, shipment3 }, shipments);
		}

		ForwardingShipment CreateShipmentWithDeliveryAddress(string orgHeaderCode, string postcode)
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_Code = orgHeaderCode;

			shipment.ConsigneePK = header.PK;

			OrgAddress address = header.MainAddress;
			address.AddressCapability.SetCapabilityEnabled(OrgAddressType.Delivery);
			address.AddressCapability.SetIsMainAddress(OrgAddressType.Delivery);
			address.OA_Address1 = "28 Long St";
			address.OA_PostCode = postcode;

			return shipment;
		}

		public void TestPickupPostCodeFilters()
		{
			var shipment1 = CreateShipmentWithPickupAddress("ABC1", "2045");
			var shipment2 = CreateShipmentWithPickupAddress("ABC2", "2000");
			var shipment3 = CreateShipmentWithPickupAddress("ABC3", "4325");

			Factory.Save();

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);

			ModuleTextFilter postcodeFilter = (ModuleTextFilter)FilterStripBizO["Pickup Address Post Code"];
			postcodeFilter.IsActive = true;

			postcodeFilter.Property = "2045";
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals(1, shipments.Count);
			AssertContainsExactElementsInAnyOrder(new ForwardingShipment[] { shipment1 }, shipments);

			postcodeFilter.Property = "20";
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals(2, shipments.Count);
			AssertContainsExactElementsInAnyOrder(new ForwardingShipment[] { shipment1, shipment2 }, shipments);

			postcodeFilter.Property = "4325";
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals(1, shipments.Count);
			AssertContainsExactElementsInAnyOrder(new ForwardingShipment[] { shipment3 }, shipments);

			shipment3.ConsignorPickupAddress.E2_AddressOverride = true;
			shipment3.ConsignorPickupAddress.E2_Postcode = "2076";

			Factory.Save();

			postcodeFilter.Property = "20";
			shipments.Load(FilterStripBizO.Filter);
			AssertEquals(3, shipments.Count);
			AssertContainsExactElementsInAnyOrder(new ForwardingShipment[] { shipment1, shipment2, shipment3 }, shipments);
		}

		ForwardingShipment CreateShipmentWithPickupAddress(string orgHeaderCode, string postcode)
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_Code = orgHeaderCode;

			shipment.ConsignorPK = header.PK;

			OrgAddress address = header.MainAddress;
			address.AddressCapability.SetCapabilityEnabled(OrgAddressType.Pickup);
			address.AddressCapability.SetIsMainAddress(OrgAddressType.Pickup);
			address.OA_Address1 = "28 Long St";
			address.OA_PostCode = postcode;

			return shipment;
		}

		#endregion

		#region TestPlannedLoadDischargeFilter

		public void TestPlannedLoadDischargeFilter()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_RL_NKLoadPort = "AUSYD";
			shipment1.JS_RL_NKDischargePort = "USLAX";

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_RL_NKLoadPort = "AUBNE";
			shipment2.JS_RL_NKDischargePort = "SGSIN";

			Factory.Save();

			var shipmentFilter = (ModuleLocationFilter)FilterStripBizO["Planned Load / Planned Discharge"];
			var results = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);

			shipmentFilter.IsActive = true;
			results.Load(FilterStripBizO.Filter);
			Assert("Not filtered - should be included", results.Contains(shipment1.PK));
			Assert("Not filtered - should be included", results.Contains(shipment2.PK));

			shipmentFilter.Property1 = "AU";
			results.Load(FilterStripBizO.Filter);
			Assert("Shipment1 should be in the collection", results.Contains(shipment1.PK));
			Assert("Shipment2 should be in the collection", results.Contains(shipment2.PK));

			shipmentFilter.Property1 = "AUBNE";
			results.Load(FilterStripBizO.Filter);
			Assert("Shipment1 should NOT be in collection", !results.Contains(shipment1.PK));
			Assert("Shipment2 should be in the collection", results.Contains(shipment2.PK));

			shipmentFilter.Property1 = "AUSYD";
			shipmentFilter.Property2 = "USLAX";
			results.Load(FilterStripBizO.Filter);
			Assert("Shipment1 should be in collection", results.Contains(shipment1.PK));
			Assert("Shipment2 should NOT be in collection", !results.Contains(shipment2.PK));
		}

		#endregion

		#endregion

		#region StatusAndFlags Filters

		public void TestEFreightStatusFilter()
		{
			Shipment1.JS_EFreightStatus = "";
			Shipment2.JS_EFreightStatus = "ECC";
			Shipment3.JS_EFreightStatus = "NON";
			Shipment4.JS_EFreightStatus = "NON";
			Shipment5.JS_EFreightStatus = "EAP";
			Shipment6.JS_EFreightStatus = "EAW";

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["EFreight Status"];
			filter.IsActive = true;
			filter.Property = "";

			var results = Factory.Load<ForwardingShipment>(filter.Query);
			AssertEquals("Empty filter matches all shipments", 6, results.Length);

			filter.Property = "ECC";
			results = Factory.Load<ForwardingShipment>(filter.Query);
			AssertCollectionContains("Matches shipment2 as its efreight status is ECC", Shipment2, results);
			AssertEquals("ECC matches 1 shipment", 1, results.Length);

			filter.Property = "NON";
			results = Factory.Load<ForwardingShipment>(filter.Query);
			AssertCollectionContains("Matches shipment3 as its efreight status is NON", Shipment3, results);
			AssertCollectionContains("Matches shipment4 as its efreight status is NON", Shipment4, results);
			AssertEquals("NON matches 2 shipments", 2, results.Length);

			filter.Property = "ALL";
			results = Factory.Load<ForwardingShipment>(filter.Query);
			AssertCollectionNotContains("Not matche shipment1 as its efreight status is empty", Shipment1, results);
			AssertEquals("ALL matches 5 shipments", 5, results.Length);
		}

		public void TestIsTemperatureControlledFilter()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();

			var packLine1 = shipment1.OuterPackLines.AddNew();
			var packLine2 = shipment1.OuterPackLines.AddNew();
			var packLine3 = shipment2.InnerPackLines.AddNew();
			packLine1.JL_RequiresTemperatureControl = false;
			packLine2.JL_RequiresTemperatureControl = true;
			packLine3.JL_RequiresTemperatureControl = true;
			Factory.Save();

			var filter = (ModuleFlagsFilter)FilterStripBizO["Is Temperature Controlled"];
			filter.IsActive = true;
			filter.Property0 = true;

			var results = Factory.Load<ForwardingShipment>(filter.Query);
			AssertCollectionContains("Matches shipment1 because shipment1 has an outer packline (packLine2) with temperature control enabled", shipment1, results);
			AssertCollectionNotContains("No match on shipment2 because NO packline with temperature control", shipment2, results);
			AssertEquals("ALL matches 1 shipments", 1, results.Length);
		}

		public void TestSecurityInspectionFilter()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Japan))
			{
				Shipment1.JS_InspectionTypeCode = "AVI";
				Shipment3.JS_InspectionTypeCode = "SAF";
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				Shipment1.JS_InspectionTypeCode = "HLD";
				Shipment2.JS_InspectionTypeCode = "XRY";
				Shipment4.JS_InspectionTypeCode = "WEB";
			}

			CusEntryNumber fallbackNumber = Factory.New<CusEntryNumber>();
			fallbackNumber.CE_ParentTable = JobShipmentSchema.Constants.TableName;
			fallbackNumber.CE_ParentID = Shipment3.PK;
			fallbackNumber.CE_Category = CusEntryNumber.Categories.InspectionStatus;
			fallbackNumber.CE_EntryType = CusEntryNumber.EntryType.InspectionStatus;
			fallbackNumber.CE_RN_NKCountryCode = "";
			fallbackNumber.CE_EntryNum = "EXM";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Security Inspection"];
			filter.IsActive = true;
			filter.Property = "";

			ForwardingShipment[] results = Factory.Load<ForwardingShipment>(filter.Query);
			AssertEquals("Empty filter matches all shipments", 6, results.Length);

			filter.Property = "UNK";
			results = Factory.Load<ForwardingShipment>(filter.Query);
			AssertCollectionContains("Matches all shipments with no inspection type defined for AU", Shipment1, results);
			AssertCollectionContains("Matches all shipments with no inspection type defined for AU", Shipment2, results);
			AssertCollectionContains("Matches all shipments with no inspection type defined for AU", Shipment4, results);
			AssertCollectionContains("Matches default inspection type", Shipment5, results);
			AssertCollectionContains("Matches default inspection type", Shipment6, results);
			AssertEquals("UNK matches 2 shipments", 5, results.Length);

			filter.Property = "EXM";
			results = Factory.Load<ForwardingShipment>(filter.Query);
			AssertCollectionContains("Matches AU Shipment with fallback inspection type", Shipment3, results);
			AssertEquals("EXM matches 1 shipment", 1, results.Length);

			filter.Property = "HLD";
			results = Factory.Load<ForwardingShipment>(filter.Query);
			AssertEquals("No matches as HLD only applies to the US", 0, results.Length);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				filter.Property = "HLD";
				results = Factory.Load<ForwardingShipment>(filter.Query);
				AssertCollectionContains("HLD is matched for the US", Shipment1, results);
				AssertEquals("HLD matches 1 shipment", 1, results.Length);

				filter.Property = "AVI";
				results = Factory.Load<ForwardingShipment>(filter.Query);
				AssertEquals("No matches for AVI in the US", 0, results.Length);

				filter.Property = "EXM";
				results = Factory.Load<ForwardingShipment>(filter.Query);
				AssertCollectionContains("EXM shipment is matched for the US as there is no country specific override", Shipment3, results);
				AssertEquals("EXM matches 1 shipment", 1, results.Length);

				filter.Property = "UNK";
				results = Factory.Load<ForwardingShipment>(filter.Query);
				AssertCollectionContains("UNK matches shipments with Inspection Type of 'WEB'", Shipment4, results);
				AssertCollectionContains("UNK matches shipments with no US Inspection Type", Shipment5, results);
				AssertCollectionContains("UNK matches shipments with no US Inspection Type", Shipment6, results);
				AssertEquals("3 shipments are matched for UNK", 3, results.Length);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Japan))
			{
				filter.Property = "HLD";
				results = Factory.Load<ForwardingShipment>(filter.Query);
				AssertEquals("HLD is not matched for Japan as it only applies to the US", 0, results.Length);

				filter.Property = "SAF";
				results = Factory.Load<ForwardingShipment>(filter.Query);
				AssertCollectionContains("SAF is matched for Japan", Shipment3, results);
				AssertEquals("SAF matches 1 shipment", 1, results.Length);

				filter.Property = "EXM";
				results = Factory.Load<ForwardingShipment>(filter.Query);
				AssertEquals("EXM is not matched as there is a Japan specific override", 0, results.Length);
			}
		}

		public void TestSecurityInspectionFilterList()
		{
			JobShipmentFilterBusinessObject job = (JobShipmentFilterBusinessObject)GetNewFilterStripBusinessObject();
			CodeDescriptionPairList pairList = job.SecurityInspectionTypeList;

			Assert(pairList.ContainsCode(BaseJobShipmentLookups.InspectionType_Screened));
			Assert(pairList.ContainsCode(BaseJobShipmentLookups.InspectionType_Approved));
			Assert(pairList.ContainsCode(FreightDataRegistry.AviationSecurity_Unknown_Code));
			Assert(pairList.Count > 2);
		}

		public void TestSecurityInspectionFilterForEUSecurityStandards()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "DEHAM";
			shipment.JS_RL_NKDestination = "JPOSA";
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				var newFactory = new BusinessObjectFactory();
				var reloadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
				reloadedShipment.JS_InspectionTypeCode = "APP";
				newFactory.Save();
			}

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["Security Inspection"];
			filter.IsActive = true;
			filter.Property = "APP";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Antarctica))
			{
				var results = Factory.Load<ForwardingShipment>(filter.Query);
				AssertEquals("APP is not matched for a non-European Country", 0, results.Length);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany))
			{
				var results = Factory.Load<ForwardingShipment>(filter.Query);
				AssertContainsExactElementsInAnyOrder("APP is matched for a European Country", shipment, results);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Liechtenstein))
			{
				var results = Factory.Load<ForwardingShipment>(filter.Query);
				AssertContainsExactElementsInAnyOrder("APP is matched for a country with the same EU Security status", shipment, results);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				var results = Factory.Load<ForwardingShipment>(filter.Query);
				AssertEquals("APP is not matched for a non-European Country", 0, results.Length);
			}
		}

		public void TestColoadStatusFilter()
		{
			ForwardingShipment coLoadMaster = Factory.New<ForwardingShipment>();
			coLoadMaster.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			coLoadMaster.JS_HouseBill = "CoLoad Master";

			ForwardingShipment coLoadSub = Factory.New<ForwardingShipment>();
			coLoadSub.JS_JS_ColoadMasterShipment = coLoadMaster.PK;
			coLoadSub.JS_HouseBill = "CoLoad Sub";

			ForwardingShipment blindCoLoadMaster = Factory.New<ForwardingShipment>();
			blindCoLoadMaster.JS_ShipmentType = Constants.ShipmentTypes.BlindCoLoadMaster;
			blindCoLoadMaster.JS_HouseBill = "Blind CoLoad Master";

			ForwardingShipment blindCoLoadSub = Factory.New<ForwardingShipment>();
			blindCoLoadSub.JS_JS_ColoadMasterShipment = blindCoLoadMaster.PK;
			blindCoLoadSub.JS_HouseBill = "Blind CoLoad Sub";

			ForwardingShipment nonCoLoad = Factory.New<ForwardingShipment>();
			nonCoLoad.JS_HouseBill = "Standard";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.CoLoadStatus];
			filter.Property = FreightConstants.CoLoadStatus.All;
			var expectedFoundShipments = new ForwardingShipment[] { coLoadMaster, coLoadSub, blindCoLoadMaster, blindCoLoadSub, nonCoLoad };
			var expectedNotFoundShipments = Array.Empty<ForwardingShipment>();

			AssertFilterResultsContainCorrectShipments(filter, expectedFoundShipments, expectedNotFoundShipments);

			filter.Property = FreightConstants.CoLoadStatus.CoLoadMaster;
			expectedFoundShipments = new ForwardingShipment[] { coLoadMaster, blindCoLoadMaster };
			expectedNotFoundShipments = new ForwardingShipment[] { coLoadSub, blindCoLoadSub, nonCoLoad };

			AssertFilterResultsContainCorrectShipments(filter, expectedFoundShipments, expectedNotFoundShipments);

			filter.Property = FreightConstants.CoLoadStatus.CoLoad;
			expectedFoundShipments = new ForwardingShipment[] { coLoadSub, blindCoLoadSub };
			expectedNotFoundShipments = new ForwardingShipment[] { coLoadMaster, blindCoLoadMaster, nonCoLoad };

			AssertFilterResultsContainCorrectShipments(filter, expectedFoundShipments, expectedNotFoundShipments);

			filter.Property = FreightConstants.CoLoadStatus.CoLoadAndCoLoadMaster;
			expectedFoundShipments = new ForwardingShipment[] { coLoadMaster, coLoadSub, blindCoLoadMaster, blindCoLoadSub };
			expectedNotFoundShipments = new ForwardingShipment[] { nonCoLoad };

			AssertFilterResultsContainCorrectShipments(filter, expectedFoundShipments, expectedNotFoundShipments);

			filter.Property = FreightConstants.CoLoadStatus.NeitherCoLoadNorCoLoadMaster;
			expectedFoundShipments = new ForwardingShipment[] { nonCoLoad };
			expectedNotFoundShipments = new ForwardingShipment[] { coLoadMaster, coLoadSub, blindCoLoadMaster, blindCoLoadSub };

			AssertFilterResultsContainCorrectShipments(filter, expectedFoundShipments, expectedNotFoundShipments);
		}

		void AssertFilterResultsContainCorrectShipments(ModuleTextFilter filter, ForwardingShipment[] expectedFoundShipments, ForwardingShipment[] expectedNotFoundShipments)
		{
			var results = Factory.Load<ForwardingShipment>(filter.Query);

			foreach (var expectedShipment in expectedFoundShipments)
			{
				AssertCollectionContains("Expected to find " + expectedShipment.HumanReadableName, expectedShipment, results);
			}

			foreach (var notExpectedShipment in expectedNotFoundShipments)
			{
				AssertCollectionNotContains("Expected NOT to find " + notExpectedShipment.HumanReadableName, notExpectedShipment, results);
			}
		}

		public void TestNoChargesFilterExists()
		{
			JobShipmentFilterBusinessObject filterBizObj = (JobShipmentFilterBusinessObject)GetNewFilterStripBusinessObject();
			ModuleFlagsFilter filter = (ModuleFlagsFilter)filterBizObj["Invoiced / Charges"];
			AssertNotNull("Filter should exist", filter);
			AssertNotNull("No Charges filter should exist", filter["No Charges"]);
		}

		public void TestNoCostsFilterExists()
		{
			JobShipmentFilterBusinessObject filterBizObj = (JobShipmentFilterBusinessObject)GetNewFilterStripBusinessObject();
			ModuleFlagsFilter filter = (ModuleFlagsFilter)filterBizObj["Invoiced / Charges"];
			AssertNotNull("Filter should exist", filter);
			AssertNotNull("No Charges filter should exist", filter["No Costs"]);
		}

		public void TestCostsNotPostedFilterExists()
		{
			JobShipmentFilterBusinessObject filterBizObj = (JobShipmentFilterBusinessObject)GetNewFilterStripBusinessObject();
			ModuleFlagsFilter filter = (ModuleFlagsFilter)filterBizObj["Invoiced / Charges"];
			AssertNotNull("Filter should exist", filter);
			AssertNotNull("Costs Not Posted filter should exist", filter["Costs Not Posted"]);
		}

		public void TestReleaseTypeFilter()
		{
			ForwardingShipment shipment1 = GetShipment((++shipmentNumberIndex).ToString());
			ForwardingShipment shipment2 = GetShipment((++shipmentNumberIndex).ToString());
			ForwardingShipment shipment3 = GetShipment((++shipmentNumberIndex).ToString());

			shipment1.JS_ReleaseType = "CAD";
			shipment2.JS_ReleaseType = "OBR";
			shipment3.JS_ReleaseType = "EBL";

			Factory.Save();
			Asserter.AddFieldOfInterest(JobShipmentSchema.JS_UniqueConsignRef.Name);

			ModuleTextFilter shipmentFilter = (ModuleTextFilter)FilterStripBizO["Release Type"];
			shipmentFilter.IsActive = true;

			Asserter.AssertMatches("", shipmentFilter, shipment1, shipment2, shipment3);

			shipmentFilter.Property = "CAD";
			Asserter.AssertMatches("", shipmentFilter, shipment1);

			shipmentFilter.Property = "OBR";
			Asserter.AssertMatches("", shipmentFilter, shipment2);
		}

		public void TestINCOTerm()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment1.JS_INCO = "FOB";
			shipment2.JS_INCO = "EXW";

			Factory.Save();

			ForwardingShipment[] results;
			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["INCO Term"];

			filter.Property = "FOB";
			results = Factory.Load<ForwardingShipment>(filter.Query);
			AssertCollectionContains(shipment1, results);
			AssertCollectionNotContains(shipment2, results);

			filter.Property = "EXW";
			results = Factory.Load<ForwardingShipment>(filter.Query);
			AssertCollectionContains(shipment2, results);
			AssertCollectionNotContains(shipment1, results);

			filter.Property = "DEQ";
			results = Factory.Load<ForwardingShipment>(filter.Query);
			AssertCollectionNotContains(shipment2, results);
			AssertCollectionNotContains(shipment1, results);
		}

		#endregion

		#region Pickup/Delivery Drop Mode Filters

		public void TestPickupDeliveryDropModeAvailability()
		{
			var pickupDropModefilter = GetNewFilterStripBusinessObject();
			AssertNotNull("Pickup Drop Mode should be available", pickupDropModefilter["Pickup Drop Mode"]);

			var deliveryDropModefilter = GetNewFilterStripBusinessObject();
			AssertNotNull("Delivery Drop Mode should be available", deliveryDropModefilter["Delivery Drop Mode"]);
		}

		public void TestPickupDropModeFilter()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();
			var shipment3 = Factory.New<ForwardingShipment>();
			var shipment4 = Factory.New<ForwardingShipment>();
			shipment1.DocsAndCartage.JP_FCLPickupEquipmentNeeded = Constants.FCLEquipmentNeeded.SideLoader;
			shipment2.DocsAndCartage.JP_FCLPickupEquipmentNeeded = Constants.FCLEquipmentNeeded.WaitForUnpack;
			shipment3.DocsAndCartage.JP_FCLPickupEquipmentNeeded = Constants.FCLEquipmentNeeded.SideLoader;
			shipment4.DocsAndCartage.JP_FCLPickupEquipmentNeeded = Constants.FCLEquipmentNeeded.LiftOffOn;

			Factory.Save();

			var pickupDropModefilter = (ModuleTextFilter)FilterStripBizO["Pickup Drop Mode"];
			AssertNotNull("Pre-condition: Pickup Drop Mode filter exists", pickupDropModefilter);
			pickupDropModefilter.Property = "";
			pickupDropModefilter.IsActive = true;

			var shipments = new ForwardingShipmentCollection(Factory);

			shipments.Load(FilterStripBizO.Filter);

			AssertEquals("Should contain shipment1", true, shipments.Contains(shipment1));
			AssertEquals("Should contain shipment2", true, shipments.Contains(shipment2));
			AssertEquals("Should contain shipment3", true, shipments.Contains(shipment3));
			AssertEquals("should contain shipment4", true, shipments.Contains(shipment4));

			pickupDropModefilter.Property = "SDL";
			shipments.Load(FilterStripBizO.Filter);

			AssertEquals("Should not contain shipment1", true, shipments.Contains(shipment1));
			AssertEquals("Should contain shipment2", false, shipments.Contains(shipment2));
			AssertEquals("Should contain shipment3", true, shipments.Contains(shipment3));
			AssertEquals("Should not contain shipment4", false, shipments.Contains(shipment4));
		}

		public void TestDeliveryDropModeFilter()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();
			var shipment3 = Factory.New<ForwardingShipment>();
			var shipment4 = Factory.New<ForwardingShipment>();
			shipment1.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = Constants.FCLEquipmentNeeded.WaitForUnpack;
			shipment2.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = Constants.FCLEquipmentNeeded.WaitForUnpack;
			shipment3.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = Constants.FCLEquipmentNeeded.Trailer;
			shipment4.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = Constants.FCLEquipmentNeeded.LiftOffOn;

			Factory.Save();

			var deliveryDropModefilter = (ModuleTextFilter)FilterStripBizO["Delivery Drop Mode"];
			AssertNotNull("Pre-condition: Delivery Drop Mode filter exists", deliveryDropModefilter);
			deliveryDropModefilter.Property = "";
			deliveryDropModefilter.IsActive = true;

			var shipments = new ForwardingShipmentCollection(Factory);

			shipments.Load(FilterStripBizO.Filter);

			AssertEquals("Should contain shipment1", true, shipments.Contains(shipment1));
			AssertEquals("Should contain shipment2", true, shipments.Contains(shipment2));
			AssertEquals("Should contain shipment3", true, shipments.Contains(shipment3));
			AssertEquals("should contain shipment4", true, shipments.Contains(shipment4));

			deliveryDropModefilter.Property = "WUP";
			shipments.Load(FilterStripBizO.Filter);

			AssertEquals("Should not contain shipment1", true, shipments.Contains(shipment1));
			AssertEquals("Should contain shipment2", true, shipments.Contains(shipment2));
			AssertEquals("Should contain shipment3", false, shipments.Contains(shipment3));
			AssertEquals("Should not contain shipment4", false, shipments.Contains(shipment4));
		}

		#endregion

		#region Company Name

		public void TestCompanyName()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "AAAAAA ORG";

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "BBBAAA ORG";

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.ConsigneeDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.ConsigneeDocumentaryAddress.E2_OA_Address = org2.MainAddress.PK;

			Factory.Save();

			((ModuleTextFilter)FilterStripBizO["Company Name"]).Property = "AAAAAA ORG";
			((ModuleTextFilter)FilterStripBizO["Company Name"]).IsActive = true;
			((ModuleTextFilter)FilterStripBizO["Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			AssertShipmentsInCollection(shipment1, shipment2, true, false);

			((ModuleTextFilter)FilterStripBizO["Company Name"]).Property = "AAA";
			AssertShipmentsInCollection(shipment1, shipment2, true, true);

			((ModuleTextFilter)FilterStripBizO["Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			AssertShipmentsInCollection(shipment1, shipment2, false, false);

			((ModuleTextFilter)FilterStripBizO["Company Name"]).Property = "BBB";
			((ModuleTextFilter)FilterStripBizO["Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			AssertShipmentsInCollection(shipment1, shipment2, true, false);

			((ModuleTextFilter)FilterStripBizO["Company Name"]).Property = "CCC";
			((ModuleTextFilter)FilterStripBizO["Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			AssertShipmentsInCollection(shipment1, shipment2, false, false);

			((ModuleTextFilter)FilterStripBizO["Company Name"]).Property = "BAA";
			((ModuleTextFilter)FilterStripBizO["Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			AssertShipmentsInCollection(shipment1, shipment2, true, false);
		}

		void AssertShipmentsInCollection(ForwardingShipment shipment1, ForwardingShipment shipment2, bool contains1, bool contains2)
		{
			ZQuery filter = FilterStripBizO.Filter;
			ForwardingShipment[] shipments = Factory.Load<ForwardingShipment>(FilterStripBizO.Filter);
			if (contains1)
			{
				AssertCollectionContains(shipment1, shipments);
			}
			else
			{
				AssertCollectionNotContains(shipment1, shipments);
			}
			if (contains2)
			{
				AssertCollectionContains(shipment2, shipments);
			}
			else
			{
				AssertCollectionNotContains(shipment2, shipments);
			}
		}

		public void TestConsigneeNameQuery()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "ZAYDEN ORG";

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.ConsigneeDocumentaryAddress.E2_OA_Address = org.MainAddress.PK;

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment2.ConsigneeDocumentaryAddress.E2_CompanyName = "ZAYDEN ORG 123";

			Factory.Save();

			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).Property = "ZAYDEN ORG";
			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).IsActive = true;

			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			ForwardingShipment[] shipments = Factory.Load<ForwardingShipment>(FilterStripBizO.Filter);
			AssertCollectionContains(shipment1, shipments);
			AssertCollectionContains(shipment2, shipments);

			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			shipments = Factory.Load<ForwardingShipment>(FilterStripBizO.Filter);
			AssertCollectionContains(shipment1, shipments);
			AssertCollectionContains(shipment2, shipments);

			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			shipments = Factory.Load<ForwardingShipment>(FilterStripBizO.Filter);
			AssertCollectionNotContains(shipment1, shipments);
			AssertCollectionContains(shipment2, shipments);

			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			shipments = Factory.Load<ForwardingShipment>(FilterStripBizO.Filter);
			AssertCollectionNotContains(shipment1, shipments);
			AssertCollectionNotContains(shipment2, shipments);

			((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			shipments = Factory.Load<ForwardingShipment>(FilterStripBizO.Filter);
			AssertCollectionNotContains(shipment1, shipments);
			AssertCollectionNotContains(shipment2, shipments);
		}

		public void TestNotifyPartyContactName()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "JIMBOS COMPANY";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "BILLS COMPANY";
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_FullName = "RUSSELLS COMPANY";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			shipment.NotifyParty2DocumentaryAddress.E2_OA_Address = org2.MainAddress.PK;
			shipment.NotifyParty3DocumentaryAddress.E2_OA_Address = org3.MainAddress.PK;

			Factory.Save();

			var filter = (ModuleTextFilter)FilterStripBizO["Notify Party Company Name"];
			filter.IsActive = true;

			Action<string, bool> assertFilterReturnsShipment = (companyName, returnsShipment) =>
			{
				filter.Property = companyName;
				var results = Factory.Load<ForwardingShipment>(FilterStripBizO.Filter);
				AssertEquals(returnsShipment, results.Contains(shipment));
			};

			assertFilterReturnsShipment("JIMBOS COMPANY", true);
			assertFilterReturnsShipment("BILLS COMPANY", true);
			assertFilterReturnsShipment("RUSSELLS COMPANY", true);
			assertFilterReturnsShipment("GREGS COMPANY", false);
		}

		public void TestPackLineRefNumber()
		{
			AssertPackLineRefNumber(JobPackLinesSchema.JL_ExportRefNumber, "Pack Line Export Reference #");
			AssertPackLineRefNumber(JobPackLinesSchema.JL_ImportRefNumber, "Pack Line Import Reference #");
		}

		void AssertPackLineRefNumber(SchemaStringColumn refNumColumn, string filterName)
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();

			var packline1 = shipment1.OuterPackLines.AddNew();
			packline1[refNumColumn] = "REF-00001";

			var packline2 = shipment1.OuterPackLines.AddNew();
			packline2[refNumColumn] = "REF-00002";

			var packline3 = shipment1.OuterPackLines.AddNew();
			packline3[refNumColumn] = "REF-00002";

			var packline4 = shipment1.OuterPackLines.AddNew();
			packline4[refNumColumn] = "REF-00003";

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline5 = shipment2.OuterPackLines.AddNew();
			packline5[refNumColumn] = "REF-00002";

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline6 = shipment3.OuterPackLines.AddNew();
			packline6[refNumColumn] = "REF-00003";

			Factory.Save();

			FilterStripBizO.ResetModuleFilters();

			var filter = (ModuleTextFilter)FilterStripBizO[filterName];
			filter.IsActive = true;

			filter.Property = "REF-00001";

			var shipments = Factory.Load<ForwardingShipment>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1 }, shipments);

			filter.Property = "REF-00002";

			shipments = Factory.Load<ForwardingShipment>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);

			filter.Property = "REF-00003";

			shipments = Factory.Load<ForwardingShipment>(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment3 }, shipments);
		}

		public void TestCompanyNameBlankOperatorSupport()
		{
			var filterBizO = new JobShipmentFilterBusinessObject();

			Assert("Consignor Company Name is mandatory, blank operators not needed", !((ModuleTextFilter)FilterStripBizO["Consignor Company Name"]).SupportsBlankComparisonOperators);
			Assert("Consignee Company Name is mandatory, blank operators not needed", !((ModuleTextFilter)FilterStripBizO["Consignee Company Name"]).SupportsBlankComparisonOperators);
		}

		public void TestSearchCompanyNameFilterOnShipmentForm()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "AAAAAA ORG";

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.ConsigneeDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.ConsignorDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.ConsigneeDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;

			var shipment4 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment4.ConsignorDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;

			Factory.Save();

			var shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);

			var companyNameFilter = (ModuleTextFilter)FilterStripBizO["Company Name"];
			companyNameFilter.IsActive = true;

			AssertContainsExactShipmentsInAnyOrder(shipments, new ModuleTextFilter[] { companyNameFilter }, new ZString[] { "AAAAAA" }, new ZString[] { ModuleTextFilter.ComparisonConstants.StartsWith }, 4,
				new ForwardingShipment[] { shipment1, shipment2, shipment3, shipment4 });

			AssertContainsExactShipmentsInAnyOrder(shipments, new ModuleTextFilter[] { companyNameFilter }, new ZString[] { "AAAAAA" }, new ZString[] { ModuleTextFilter.ComparisonConstants.Exact }, 0,
				Array.Empty<ForwardingShipment>());

			AssertContainsExactShipmentsInAnyOrder(shipments, new ModuleTextFilter[] { companyNameFilter }, new ZString[] { "BBBAAA" }, new ZString[] { ModuleTextFilter.ComparisonConstants.StartsWith }, 0,
				Array.Empty<ForwardingShipment>());

			AssertContainsExactShipmentsInAnyOrder(shipments, new ModuleTextFilter[] { companyNameFilter }, new ZString[] { "BBBAAA" }, new ZString[] { ModuleTextFilter.ComparisonConstants.Exact }, 0,
				Array.Empty<ForwardingShipment>());

			var cneCompanyNameFilter = (ModuleTextFilter)FilterStripBizO["Consignee Company Name"];
			cneCompanyNameFilter.IsActive = true;

			AssertContainsExactShipmentsInAnyOrder(shipments, new ModuleTextFilter[] { companyNameFilter, cneCompanyNameFilter }, new ZString[] { "AAAAAA", "AAAAAA" },
				new ZString[] { ModuleTextFilter.ComparisonConstants.StartsWith, ModuleTextFilter.ComparisonConstants.StartsWith }, 2, new ForwardingShipment[] { shipment1, shipment3 });

			AssertContainsExactShipmentsInAnyOrder(shipments, new ModuleTextFilter[] { companyNameFilter, cneCompanyNameFilter }, new ZString[] { "AAAAAA", "AAAAAA" },
				new ZString[] { ModuleTextFilter.ComparisonConstants.StartsWith, ModuleTextFilter.ComparisonConstants.Exact }, 0, Array.Empty<ForwardingShipment>());

			var cnrCompanyNameFilter = (ModuleTextFilter)FilterStripBizO["Consignor Company Name"];
			cnrCompanyNameFilter.IsActive = true;
			cneCompanyNameFilter.IsActive = false;

			AssertContainsExactShipmentsInAnyOrder(shipments, new ModuleTextFilter[] { companyNameFilter, cnrCompanyNameFilter }, new ZString[] { "AAAAAA", "AAAAAA" },
				new ZString[] { ModuleTextFilter.ComparisonConstants.StartsWith, ModuleTextFilter.ComparisonConstants.StartsWith }, 2, new ForwardingShipment[] { shipment2, shipment4 });

			AssertContainsExactShipmentsInAnyOrder(shipments, new ModuleTextFilter[] { companyNameFilter, cnrCompanyNameFilter }, new ZString[] { "AAAAAA", "AAAAAA" },
				new ZString[] { ModuleTextFilter.ComparisonConstants.StartsWith, ModuleTextFilter.ComparisonConstants.Exact }, 0, Array.Empty<ForwardingShipment>());

			shipment3.ConsigneeDocumentaryAddress.E2_AddressOverride = true;
			shipment3.ConsigneeDocumentaryAddress.E2_CompanyName = "BBBAAA ORG";

			shipment4.ConsignorDocumentaryAddress.E2_AddressOverride = true;
			shipment4.ConsignorDocumentaryAddress.E2_CompanyName = "BBBAAA ORG";

			Factory.Save();

			cnrCompanyNameFilter.IsActive = false;

			AssertContainsExactShipmentsInAnyOrder(shipments, new ModuleTextFilter[] { companyNameFilter }, new ZString[] { "AAAAAA" }, new ZString[] { ModuleTextFilter.ComparisonConstants.StartsWith }, 2,
				new ForwardingShipment[] { shipment1, shipment2 });

			AssertContainsExactShipmentsInAnyOrder(shipments, new ModuleTextFilter[] { companyNameFilter }, new ZString[] { "AAAAAA" }, new ZString[] { ModuleTextFilter.ComparisonConstants.Exact }, 0,
				Array.Empty<ForwardingShipment>());

			AssertContainsExactShipmentsInAnyOrder(shipments, new ModuleTextFilter[] { companyNameFilter }, new ZString[] { "BBBAAA" }, new ZString[] { ModuleTextFilter.ComparisonConstants.StartsWith }, 2,
				new ForwardingShipment[] { shipment3, shipment4 });

			AssertContainsExactShipmentsInAnyOrder(shipments, new ModuleTextFilter[] { companyNameFilter }, new ZString[] { "BBBAAA" }, new ZString[] { ModuleTextFilter.ComparisonConstants.Exact }, 0,
				Array.Empty<ForwardingShipment>());

			cneCompanyNameFilter.IsActive = true;

			AssertContainsExactShipmentsInAnyOrder(shipments, new ModuleTextFilter[] { companyNameFilter, cneCompanyNameFilter }, new ZString[] { "AAAAAA", "AAAAAA" },
				new ZString[] { ModuleTextFilter.ComparisonConstants.StartsWith, ModuleTextFilter.ComparisonConstants.StartsWith }, 1, new ForwardingShipment[] { shipment1 });

			AssertContainsExactShipmentsInAnyOrder(shipments, new ModuleTextFilter[] { companyNameFilter, cneCompanyNameFilter }, new ZString[] { "AAAAAA", "AAAAAA" },
				new ZString[] { ModuleTextFilter.ComparisonConstants.StartsWith, ModuleTextFilter.ComparisonConstants.Exact }, 0, Array.Empty<ForwardingShipment>());

			cnrCompanyNameFilter.IsActive = true;
			cneCompanyNameFilter.IsActive = false;

			AssertContainsExactShipmentsInAnyOrder(shipments, new ModuleTextFilter[] { companyNameFilter, cnrCompanyNameFilter }, new ZString[] { "AAAAAA", "AAAAAA" },
				new ZString[] { ModuleTextFilter.ComparisonConstants.StartsWith, ModuleTextFilter.ComparisonConstants.StartsWith }, 1, new ForwardingShipment[] { shipment2 });

			AssertContainsExactShipmentsInAnyOrder(shipments, new ModuleTextFilter[] { companyNameFilter, cnrCompanyNameFilter }, new ZString[] { "AAAAAA", "AAAAAA" },
				new ZString[] { ModuleTextFilter.ComparisonConstants.StartsWith, ModuleTextFilter.ComparisonConstants.Exact }, 0, Array.Empty<ForwardingShipment>());
		}

		void AssertContainsExactShipmentsInAnyOrder(ForwardingShipmentCollection shipmentCollection, ModuleTextFilter[] filters, ZString[] properties,
			ZString[] comparisonOperators, int shipmentsCount, ForwardingShipment[] shipments)
		{
			if (filters.Length == properties.Length && filters.Length == comparisonOperators.Length)
			{
				for (int i = 0; i < filters.Length; i++)
				{
					filters[i].Property = properties[i];
					filters[i].ComparisonOperator = comparisonOperators[i];
				}
			}

			shipmentCollection.Load(FilterStripBizO.Filter);

			AssertEquals(shipmentsCount, shipmentCollection.Count);
			AssertContainsExactElementsInAnyOrder(shipments, shipmentCollection);
		}

		#endregion

		#region Related Consolidations

		public void TestRelatedConsolidationsFilter()
		{
			var shipments = new ForwardingShipmentCollection(Factory);

			var shipment1 = Factory.New<ForwardingShipment>();
			var consol1 = shipment1.Consols.AddNew();
			consol1.JK_AgentsReference = "AgentRef";
			consol1.JK_TransportMode = Constants.TransportModes.Air;

			var consol2 = shipment1.Consols.AddNew();
			consol2.JK_AgentsReference = "AgentRef";
			consol2.JK_TransportMode = Constants.TransportModes.Air;

			var shipment2 = Factory.New<ForwardingShipment>();
			var consol3 = shipment2.Consols.AddNew();
			consol3.JK_AgentsReference = "AgentRef";
			consol3.JK_TransportMode = Constants.TransportModes.Air;

			var consol4 = shipment2.Consols.AddNew();
			consol4.JK_AgentsReference = "AgentRefOther";
			consol4.JK_TransportMode = Constants.TransportModes.Air;

			var shipment3 = Factory.New<ForwardingShipment>();
			var consol5 = shipment3.Consols.AddNew();
			consol5.JK_AgentsReference = "AgentRefOther";
			consol5.JK_TransportMode = Constants.TransportModes.Sea;

			var consol6 = shipment3.Consols.AddNew();
			consol6.JK_AgentsReference = "AgentRefOther";
			consol6.JK_TransportMode = Constants.TransportModes.Sea;

			var shipment4 = Factory.New<ForwardingShipment>();

			Factory.Save();

			var relatedConsolsFilter = (ConsolsOfShipmentFilter)FilterStripBizO["Related Consolidations"];
			relatedConsolsFilter.IsActive = true;

			var consolTransportFilter = relatedConsolsFilter.SelectedFilters.AddTextFilterStrip("Transport Mode", Constants.TransportModes.Air);
			consolTransportFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			var consolAgentRefFilter = relatedConsolsFilter.SelectedFilters.AddTextFilterStrip("Agent Reference #", "AgentRef");
			consolAgentRefFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			relatedConsolsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

			shipments.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);

			relatedConsolsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;

			shipments.Load(FilterStripBizO.Filter);

			var pks = shipments.Select(c => c.PK);
			AssertCollectionContains(shipment1.PK, pks);
			AssertCollectionContains(shipment4.PK, pks);
			AssertCollectionNotContains(shipment2.PK, pks);
			AssertCollectionNotContains(shipment3.PK, pks);

			relatedConsolsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;

			shipments.Load(FilterStripBizO.Filter);
			pks = shipments.Select(c => c.PK);

			AssertCollectionContains(shipment3.PK, pks);
			AssertCollectionContains(shipment4.PK, pks);
			AssertCollectionNotContains(shipment1.PK, pks);
			AssertCollectionNotContains(shipment2.PK, pks);
		}

		#endregion

		public void TestWorkflowFiltersPresent()
		{
			JobShipmentFilterBusinessObject milestoneFilter = (JobShipmentFilterBusinessObject)GetNewFilterStripBusinessObject();
			AssertNotNull("You must use WorkflowFilterStripsHelper to add Workflow filter strips", milestoneFilter["Milestone Date"]);
		}

		public void TestRegistryCustomFieldsFiltersPresent()
		{
			FreightDataRegistry.Instance.ShipmentCustomText1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Custom 1", "cusom field 1"));
			FreightDataRegistry.Instance.ShipmentCustomDate2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Date 2", "cusom date 2"));
			FreightDataRegistry.Instance.ShipmentCustomFlag1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Bool 1", "cusom flag 1"));
			FreightDataRegistry.Instance.ShipmentCustomDecimalNo2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Decimal 2", "cusom decimal 2"));

			var filterStripBizO = (JobShipmentFilterBusinessObject)GetNewFilterStripBusinessObject();
			AssertNotNull(filterStripBizO["Custom 1 (Custom)"]);
			AssertNotNull(filterStripBizO["Date 2 (Custom)"]);
			AssertNotNull(filterStripBizO["Bool 1 (Custom)"]);
			AssertNotNull(filterStripBizO["Decimal 2 (Custom)"]);
		}

		[ExpectNoExceptions]
		public void TestWorkflowFilter_ConfilctedWithSystemDefinedFilter()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";
			template.P0_IsActive = true;

			var def = Factory.New<GenCustomColumnDefinition>();
			def.XC_ParentTableCode = "JS";
			def.XC_Name = "Milestones";
			def.XC_Type = "STR";
			def.XC_ParentID = template.PK;

			Factory.Save();

			var milestoneFilter = (JobShipmentFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filters = milestoneFilter.ModuleFilters;

			var milestonesFilter = filters["Milestones (1)"];
			AssertNotNull(milestonesFilter);
		}

		[UseSnapshotProtection(true)]
		public void TestColorScheme_HandleSQL8623Error()
		{
			var collection = new ShipmentCollection(Factory);

			for (var i = 0; i < 5; i++)
			{
				Factory.NewWithValidTestData<ProcessTask>();
			}

			for (var i = 0; i < 5; i++)
			{
				var item = Factory.NewWithValidTestData<ForwardingShipment>();
				collection.Add(item);
			}

			var colorScheme = Factory.New<GridColourScheme>();
			Factory.Save();

			var columns = JobShipmentSchema.All.Where(s => s.DotNetType.Equals(typeof(string))).ToList();
			using (var form = new ZForm(collection))
			using (SystemDataRegistry.Instance.QueryTimeoutForColourSchemeManager.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 60))
			{
				var grid = new ZGrid();
				var colorManager = new ZArchitecture.Testing.GridColourSchemeManagerForTest(grid);

				foreach (var column in columns)
				{
					grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(column.Name, 80));
				}
				form.Controls.Add(grid);

				colorManager.SetLastUsedSchemeForCurrentUser(colorScheme);
				var jsFilterStrip = new JobShipmentFilterBusinessObject();
				var colorStrip = colorManager.AddColorStrip(jsFilterStrip, typeof(ForwardingShipment), System.Drawing.Color.Red);
				AddFilter(colorStrip.AddFilterStrip<WorkflowModuleTextFilterWithRoutingSupport>("Milestone Completed"), "AED");
				AddFilter(colorStrip.AddFilterStrip<WorkflowModuleTextFilterWithRoutingSupport>("Milestone Completed"), "SVR");
				AddFilter(colorStrip.AddFilterStrip<WorkflowModuleTextFilterWithRoutingSupport>("Milestone Completed"), "CCC");
				AddFilter(colorStrip.AddFilterStrip<WorkflowModuleTextFilterWithRoutingSupport>("Milestone Completed"), "WHA");
				AddFilter(colorStrip.AddFilterStrip<WorkflowModuleTextFilterWithRoutingSupport>("Milestone Completed"), "EXV");
				AddFilterPurple(colorStrip.AddFilterStrip<WorkflowModuleTextFilterWithRoutingSupport>("Milestone Completed"), "CAD");
				AddFilterPurple(colorStrip.AddFilterStrip<WorkflowModuleTextFilterWithRoutingSupport>("Milestone Completed"), "PCF");
				AddFilterBlue(colorStrip.AddFilterStrip<WorkflowModuleTextFilterWithRoutingSupport>("Milestone Completed"), "Z50");
				AddFilterBlue(colorStrip.AddFilterStrip<WorkflowModuleTextFilterWithRoutingSupport>("Milestone Completed"), "SCM");
				AddFilterBlue(colorStrip.AddFilterStrip<WorkflowModuleTextFilterWithRoutingSupport>("Milestone Completed"), "DDI");
				AddFilterBlue(colorStrip.AddFilterStrip<WorkflowModuleTextFilterWithRoutingSupport>("Milestone Completed"), "CIP");
				AddFilter(colorStrip.AddFilterStrip<WorkflowModuleTextFilterWithRoutingSupport>("Milestone Completed"), "BKQ");
				AddFilter(colorStrip.AddFilterStrip<WorkflowModuleTextFilterWithRoutingSupport>("Milestone Completed"), "QCT");
				AddFilter(colorStrip.AddFilterStrip<WorkflowModuleTextFilterWithRoutingSupport>("Milestone Completed"), "TRC");
				AddFilter(colorStrip.AddFilterStrip<WorkflowModuleTextFilterWithRoutingSupport>("Milestone Completed"), "WKS");
				AddFilter(colorStrip.AddFilterStrip<WorkflowModuleTextFilterWithRoutingSupport>("Milestone Completed"), "XX3");
				AddFilter(colorStrip.AddFilterStrip<WorkflowModuleTextFilterWithRoutingSupport>("Milestone Completed"), "JOP");
				AddFilter(colorStrip.AddFilterStrip<WorkflowModuleTextFilterWithRoutingSupport>("Milestone Completed"), "FIN");
				AddFilter(colorStrip.AddFilterStrip<WorkflowModuleTextFilterWithRoutingSupport>("Milestone Completed"), "X7R");

				grid.SetDataBinding(collection, "");

				void AddFilter(WorkflowModuleTextFilterWithRoutingSupport filter, string milestoneEvent)
				{
					filter.IsActive = true;
					filter.MilestoneEvent = milestoneEvent;
					filter.Property = "All";
				}

				void AddFilterPurple(WorkflowModuleTextFilterWithRoutingSupport filter, string milestoneEvent)
				{
					AddFilter(filter, milestoneEvent);
					filter.GroupName = "GROUP1";
					filter.OrCategory = FilterOrCategory.Purple;
				}

				void AddFilterBlue(WorkflowModuleTextFilterWithRoutingSupport filter, string milestoneEvent)
				{
					AddFilter(filter, milestoneEvent);
					filter.GroupName = "GROUP2";
					filter.OrCategory = FilterOrCategory.Blue;
				}
				ZArchitecture.Testing.GridColourSchemeManagerForTest.RecreateGridColourSchemeManager(grid, colorScheme, true);
				Factory.Save();

				form.Show();
				grid.StartLoadBackgroundColour();
				while (!grid.IsBackgroundColourLoaded)
				{
					Thread.Sleep(1000);
					Application.DoEvents();
				}

				AssertContains(
					"Failed to apply color scheme \"\" to grid rows.\r\nRule Name: Unknown.\r\nReason: Color scheme configuration may be too complicated.\r\nPlease simplify it and try again.",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNextTaskAssignedTo()
		{
			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			ProcessTask task1a = shipment1.WorkflowItems.Tasks.AddNew();
			task1a.P9_GS_NKAssignedStaffMember = "ZZ";
			task1a.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			ProcessTask task1b = shipment1.WorkflowItems.Tasks.AddNew();
			task1b.P9_GS_NKAssignedStaffMember = "";
			task1b.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			ProcessTask task2a = shipment2.WorkflowItems.Tasks.AddNew();
			task2a.P9_GS_NKAssignedStaffMember = "C";
			task2a.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			ProcessTask task2b = shipment2.WorkflowItems.Tasks.AddNew();
			task2b.P9_GS_NKAssignedStaffMember = "ZZ";
			task2b.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			ForwardingShipment shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			((ModuleNkFilter)FilterStripBizO["Next Task Assigned To"]).Property = "ZZ";
			ZQuery filter = FilterStripBizO.Filter;
			((ModuleNkFilter)FilterStripBizO["Next Task Assigned To"]).IsActive = true;

			ForwardingShipment[] shipments = Factory.Load<ForwardingShipment>(FilterStripBizO.Filter);
			AssertEquals(1, shipments.Length);
			AssertEquals(shipment2, shipments[0]);
		}

		public void TestModeListHasSameElementsAsShipmentHas()
		{
			var shipment = GetShipment("Empty");
			List<string> possibleModes = new List<string>();

			foreach (ICodeDescription transportMode in shipment.Lookups.JS_TransportMode_List)
			{
				shipment.JS_TransportMode = transportMode.Code;
				foreach (ICodeDescription mode in shipment.Lookups.JS_PackingMode_List)
				{
					if (!possibleModes.Contains(mode.Code))
					{
						possibleModes.Add(mode.Code);
					}
				}
			}

			Assert(possibleModes.Count > 0);
			AssertContainsExactElementsInAnyOrder(possibleModes, FreightCodePairLists.JS_PackingModeList(string.Empty).Cast<ICodeDescription>().Select(x => x.Code));
		}

		#region TestIsHazardous

		public void TestIsHazardous_MasterShipmentWithRelatedShipments()
		{
			var masterShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var relatedShipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var relatedShipment2 = Factory.NewWithValidTestData<ForwardingShipment>();

			relatedShipment1.JS_JS_ColoadMasterShipment = masterShipment.PK;
			relatedShipment2.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var packLine1 = relatedShipment1.OuterPackLines.AddNew();
			var packLine2 = relatedShipment2.OuterPackLines.AddNew();

			var safeCommodity = Factory.New<RefCommodityCode>();
			safeCommodity.RH_Code = "SAF";
			safeCommodity.RH_IsHazardous = false;

			var hazardousCommodity = Factory.New<RefCommodityCode>();
			hazardousCommodity.RH_Code = "DED";
			hazardousCommodity.RH_IsHazardous = true;

			packLine1.JL_RH_NKCommodityCode = safeCommodity.RH_Code;
			packLine2.JL_RH_NKCommodityCode = hazardousCommodity.RH_Code;

			Factory.Save();

			var results = new ForwardingShipmentCollection(Factory);
			var shipmentFilter = (ModuleFlagsFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.IsHazardous];
			shipmentFilter.IsActive = true;

			shipmentFilter.Property0 = true;
			results.Load(FilterStripBizO.Filter);

			CombineAssertions("Filtering shipments for 'Is Hazardous' = true", () =>
			{
				AssertEquals("Result should contain the master shipment as relatedShipment2 has packLine2 which has a hazardous commodity.", true, results.Contains(masterShipment.PK));
				AssertEquals("Result should not contain relatedShipment1 as packLine1 has a safe commodity.", false, results.Contains(relatedShipment1.PK));
				AssertEquals("Result should contain relatedShipment2 as packLine2 has a hazardous commodity.", true, results.Contains(relatedShipment2.PK));
			});

			shipmentFilter.Property0 = false;
			results.Load(FilterStripBizO.Filter);

			CombineAssertions("Filtering shipments for 'Is Hazardous' = false", () =>
			{
				AssertEquals("Result should not contain the master shipment as relatedShipment2 has packLine2 which has a hazardous commodity.", false, results.Contains(masterShipment.PK));
				AssertEquals("Result should contain relatedShipment1 as packLine1 has a safe commodity.", true, results.Contains(relatedShipment1.PK));
				AssertEquals("Result should not contain relatedShipment2 as packLine2 has a hazardous commodity.", false, results.Contains(relatedShipment2.PK));
			});
		}

		#endregion

		#region TestCountrySpecificFilterList

		public void TestCountrySpecificFilterList()
		{
			ZString originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.NewZealand;
			try
			{
				ResetFilterStripBizO();
				AssertNotNull(FilterStripBizO["Container Mode"]);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = originalCountryCode;
			}
		}

		#endregion

		#region TestBaseFilter

		public virtual void TestBaseFilter()
		{
			SetupBaseFilterTestData();

			AssertNotNull(FilterStripBizO.Filter);
			AssertNotEquals("Filter not empty", "", FilterStripBizO.Filter.LiteralTextADO);

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			// all forward registered shipments should show regardless of other flags.
			Assert("FC-: Expect collection to contain Shipment1", shipments.Contains(Shipment1));
			Assert("FCB: Expect collection to contain Shipment2", shipments.Contains(Shipment2));
			Assert("F--: Expect collection to contain Shipment3", shipments.Contains(Shipment3));
			Assert("--B: Expect collection not to contain Shipment4", !shipments.Contains(Shipment4));
			Assert("-C-: Expect collection not to contain Shipment5", !shipments.Contains(Shipment5));
		}

		#endregion

		#region TestShowCrossTradeShipmentsQuery

		public void TestShowCrossTradeShipmentsQuery()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_RL_NKOrigin = "USCHI";
			shipment1.JS_RL_NKDestination = "NZAKL";

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_RL_NKOrigin = "USCHI";
			shipment2.JS_RL_NKDestination = "NZAKL";
			shipment2.JS_IsBooking = true;

			Factory.Save();

			var filter = (ModuleFlagsFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.ShowTranshipCrossTradeNoConsol];
			filter["Cross Trade Shipments Only"] = true;

			var collection = Factory.Load<ForwardingShipment>(filter.Query);

			// will not include bookings that are not yet forward registered.
			Assert("Expect collection to contain Shipment1", collection.Contains(Shipment1));
			Assert("Expect collection not to contain Shipment2", collection.Contains(Shipment2));
		}

		#endregion

		#region TestCSTerminFilter

		public void TestCSTerminFilter()
		{
			FreightDataRegistry.Instance.ConsignorShipperTerminology.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue);

			AssertBizOFilterText(FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue);

			FreightDataRegistry.Instance.ConsignorShipperTerminology.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TestTestTest");
			AssertBizOFilterText("TestTestTest");
		}

		void AssertBizOFilterText(string expectedValue)
		{
			var filter = new JobShipmentFilterBusinessObject();
			AssertNotNull("FilterCollection must be contained filter with name \"" + expectedValue + " / Consignee\"", filter[expectedValue + " / Consignee"]);
		}

		#endregion

		#region Order Custom Fields Filters

		public void TestGetOrderHeaderQuery()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			Order order1 = shipment1.AttachedOrders.AddNew();
			order1.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			order1.JD_CustomAttrib1 = "blah";

			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			Order order2 = shipment2.AttachedOrders.AddNew();
			order2.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			order2.JD_CustomAttrib1 = "halb";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["OrderHeader.CustomAttrib1"];
			filter.Property = "bl";
			filter.IsActive = true;

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertEquals(1, shipments.Count);
			AssertEquals(shipment1, shipments[0]);
		}

		public void TestGetOrderLineQuery()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			Order order1 = shipment1.AttachedOrders.AddNew();
			order1.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			OrderLine orderLine1 = order1.OrderLines.AddNew();
			orderLine1.JO_CustomAttrib1 = "blah";

			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			Order order2 = shipment2.AttachedOrders.AddNew();
			order2.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			OrderLine orderLine2 = order2.OrderLines.AddNew();
			orderLine2.JO_CustomAttrib1 = "halb";

			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)FilterStripBizO["OrderLine.CustomAttrib1"];
			filter.Property = "bl";
			filter.IsActive = true;

			ForwardingShipmentCollection shipments = new ForwardingShipmentCollection(Factory, FilterStripBizO.Filter);
			shipments.Load();

			AssertEquals(1, shipments.Count);
			AssertEquals(shipment1, shipments[0]);
		}

		#endregion

		#region TestHasDamagedPackages

		public void TestHasDamagedPackages()
		{
			var relatedShipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine1 = relatedShipment1.OuterPackLines.AddNew();
			packLine1.JL_Damaged = 0;

			var relatedShipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine2 = relatedShipment2.OuterPackLines.AddNew();
			packLine2.JL_Damaged = 1;
			var packLine3 = relatedShipment2.OuterPackLines.AddNew();
			packLine3.JL_Damaged = 0;

			Factory.Save();

			var shipmentFilter = (ModuleFlagsFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.HasDamagedPackages];
			shipmentFilter.IsActive = true;
			shipmentFilter.Property0 = true;

			var results = new ForwardingShipmentCollection(Factory);
			results.Load(FilterStripBizO.Filter);
			Assert("Result should not contain relatedShipment1", !results.Contains(relatedShipment1.PK));
			Assert("Result should contain relatedShipment2", results.Contains(relatedShipment2.PK));

			shipmentFilter.Property0 = false;
			results.Load(FilterStripBizO.Filter);
			Assert("Result should contain relatedShipment1", results.Contains(relatedShipment1.PK));
			Assert("Result should not contain relatedShipment2", !results.Contains(relatedShipment2.PK));
		}

		#endregion

		#region TestHasPillagedPackages

		public void TestHasPillagedPackages()
		{
			var relatedShipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine1 = relatedShipment1.OuterPackLines.AddNew();
			packLine1.JL_Pillaged = 0;

			var relatedShipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var packLine2 = relatedShipment2.OuterPackLines.AddNew();
			packLine2.JL_Pillaged = 1;
			var packLine3 = relatedShipment2.OuterPackLines.AddNew();
			packLine3.JL_Pillaged = 0;

			Factory.Save();

			var shipmentFilter = (ModuleFlagsFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.HasPillagedPackages];
			shipmentFilter.IsActive = true;
			shipmentFilter.Property0 = true;

			var results = new ForwardingShipmentCollection(Factory);
			results.Load(FilterStripBizO.Filter);
			Assert("Result should not contain relatedShipment1", !results.Contains(relatedShipment1.PK));
			Assert("Result should contain relatedShipment2", results.Contains(relatedShipment2.PK));

			shipmentFilter.Property0 = false;
			results.Load(FilterStripBizO.Filter);
			Assert("Result should contain relatedShipment1", results.Contains(relatedShipment1.PK));
			Assert("Result should not contain relatedShipment2", !results.Contains(relatedShipment2.PK));
		}

		#endregion

		#region TestOriginTransitWarehouseStatus

		public void TestOriginTransitWarehouseStatus()
		{
			var relatedShipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline1 = relatedShipment1.OuterPackLines.AddNew();
			packline1.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.ShortShipped;

			var relatedShipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline2 = relatedShipment2.OuterPackLines.AddNew();
			packline2.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown;
			var packline2Inner = packline2.InnerPackLines.AddNew();
			packline2Inner.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus;

			Factory.Save();

			var shipmentFilter = (ModuleTextFilter)FilterStripBizO[JobShipmentFilterBusinessObject.Descriptions.OriginTransitWarehouseStatus];
			shipmentFilter.IsActive = true;
			shipmentFilter.Property = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.ShortShipped;

			var results = new ForwardingShipmentCollection(Factory);
			results.Load(FilterStripBizO.Filter);
			Assert("Result should contain relatedShipment1", results.Contains(relatedShipment1.PK));
			Assert("Result should not contain relatedShipment2", !results.Contains(relatedShipment2.PK));

			shipmentFilter.Property = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown;
			results.Load(FilterStripBizO.Filter);
			Assert("Result should not contain relatedShipment1", !results.Contains(relatedShipment1.PK));
			Assert("Result should contain relatedShipment2", results.Contains(relatedShipment2.PK));

			shipmentFilter.Property = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus;
			results.Load(FilterStripBizO.Filter);
			Assert("Result should not contain relatedShipment1", !results.Contains(relatedShipment1.PK));
			Assert("Result should not contain relatedShipment2", !results.Contains(relatedShipment2.PK));
		}

		#endregion

		public void TestSearchOfUnlocoOutsideLoginBranchesRestriction()
		{
			Action<ModuleLocationFilter, bool, bool> assertFilterValidation = (moduleLocationFilter, errorsExpectedOnProperty1, errorsExpectedOnProperty2) =>
			{
				moduleLocationFilter.Validation.ValidateAll();
				AssertEquals(errorsExpectedOnProperty1, moduleLocationFilter.Property1Info.HasErrors());
				AssertEquals(errorsExpectedOnProperty2, moduleLocationFilter.Property2Info.HasErrors());
			};

			Env.Security.MaintainShipmentAllowSearchOfUnlocoOutsideLoginBranches.IsAllowed = false;

			JobShipmentFilterBusinessObject filterStripBizOSG = new JobShipmentFilterBusinessObject();
			Factory.Save();
			ModuleLocationFilter originDestinationFilter = (ModuleLocationFilter)filterStripBizOSG["Origin / Destination"];
			originDestinationFilter.IsActive = true;
			ModuleLocationFilter loadDischargeFilter = (ModuleLocationFilter)filterStripBizOSG["Load / Discharge"];
			loadDischargeFilter.IsActive = true;

			AssertEquals(FilterVisibility.AlwaysVisible, originDestinationFilter.Visibility);
			AssertEquals(FilterVisibility.AlwaysVisible, loadDischargeFilter.Visibility);

			originDestinationFilter.Property1 = "AUSYD";
			originDestinationFilter.Property2 = ZString.Empty;
			loadDischargeFilter.Property1 = ZString.Empty;
			loadDischargeFilter.Property2 = ZString.Empty;
			assertFilterValidation(loadDischargeFilter, false, false);
			assertFilterValidation(originDestinationFilter, false, false);

			originDestinationFilter.Property1 = ZString.Empty;
			originDestinationFilter.Property2 = ZString.Empty;
			loadDischargeFilter.Property1 = "AUSYD";
			loadDischargeFilter.Property2 = ZString.Empty;
			assertFilterValidation(loadDischargeFilter, false, false);
			assertFilterValidation(originDestinationFilter, false, false);

			originDestinationFilter.Property1 = "AUSYD";
			originDestinationFilter.Property2 = ZString.Empty;
			loadDischargeFilter.Property1 = "AUSYD";
			loadDischargeFilter.Property2 = ZString.Empty;
			assertFilterValidation(loadDischargeFilter, false, false);
			assertFilterValidation(originDestinationFilter, false, false);

			originDestinationFilter.Property1 = "AUMEL";
			originDestinationFilter.Property2 = ZString.Empty;
			loadDischargeFilter.Property1 = "AUSYD";
			loadDischargeFilter.Property2 = ZString.Empty;
			assertFilterValidation(loadDischargeFilter, false, false);
			assertFilterValidation(originDestinationFilter, false, false);

			ModuleLocationFilter originDestinationFilter2 = (ModuleLocationFilter)FilterStripBizO.CreateDuplicateFor("Origin / Destination");
			originDestinationFilter2.IsActive = true;

			originDestinationFilter.OrCategory = FilterOrCategory.Red;
			loadDischargeFilter.OrCategory = FilterOrCategory.Red;
			originDestinationFilter2.OrCategory = FilterOrCategory.Red;

			originDestinationFilter.Property1 = "AUSYD";
			originDestinationFilter.Property2 = ZString.Empty;
			loadDischargeFilter.Property1 = ZString.Empty;
			loadDischargeFilter.Property2 = "AUSYD";
			originDestinationFilter2.Property1 = ZString.Empty;
			originDestinationFilter2.Property2 = "AUSYD";
			assertFilterValidation(loadDischargeFilter, false, false);
			assertFilterValidation(originDestinationFilter, false, false);
			assertFilterValidation(originDestinationFilter2, false, false);

			ModuleLocationFilter loadDischargeFilter2 = (ModuleLocationFilter)FilterStripBizO.CreateDuplicateFor("Load / Discharge");
			originDestinationFilter2.IsActive = false;
			loadDischargeFilter2.IsActive = true;

			loadDischargeFilter2.OrCategory = FilterOrCategory.Red;

			originDestinationFilter.Property1 = "AUSYD";
			originDestinationFilter.Property2 = ZString.Empty;
			loadDischargeFilter.Property1 = ZString.Empty;
			loadDischargeFilter.Property2 = "AUSYD";
			loadDischargeFilter2.Property1 = "AUSYD";
			loadDischargeFilter2.Property2 = "AUSYD";
			loadDischargeFilter2.Property2 = ZString.Empty;
			assertFilterValidation(loadDischargeFilter, false, false);
			assertFilterValidation(originDestinationFilter, false, false);
			assertFilterValidation(loadDischargeFilter2, false, false);

			originDestinationFilter2.IsActive = true;

			originDestinationFilter.Property1 = "AUSYD";
			originDestinationFilter.Property2 = ZString.Empty;
			loadDischargeFilter.Property1 = ZString.Empty;
			loadDischargeFilter.Property2 = "AUSYD";
			originDestinationFilter2.Property1 = ZString.Empty;
			originDestinationFilter2.Property2 = "AUSYD";
			loadDischargeFilter2.Property1 = "AUSYD";
			loadDischargeFilter2.Property2 = ZString.Empty;
			assertFilterValidation(loadDischargeFilter, false, false);
			assertFilterValidation(originDestinationFilter, false, false);
			assertFilterValidation(loadDischargeFilter2, false, false);
			assertFilterValidation(originDestinationFilter2, false, false);

			originDestinationFilter.Property1 = "AUSYD";
			originDestinationFilter.Property2 = ZString.Empty;
			loadDischargeFilter.Property1 = ZString.Empty;
			loadDischargeFilter.Property2 = "AUSYD";
			originDestinationFilter2.Property1 = "AUBNE";
			originDestinationFilter2.Property2 = ZString.Empty;
			loadDischargeFilter2.Property1 = ZString.Empty;
			loadDischargeFilter2.Property2 = "AUBNE";
			assertFilterValidation(loadDischargeFilter, false, false);
			assertFilterValidation(originDestinationFilter, false, false);
			assertFilterValidation(loadDischargeFilter2, false, false);
			assertFilterValidation(originDestinationFilter2, false, false);
		}

		public void TestDirectMasterAndLeadShipmentFilterMaxLength()
		{
			var filter = FilterStripBizO["Direct Master/Lead Shipment #"];
			AssertEquals(ModuleNumberFilter.MultiplyMaxLength(JobShipmentSchema.JS_UniqueConsignRef.MaxLength), filter.MaxLength);
		}

		public void TestRelatedTransportBookingsFilter()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();
			var shipment3 = Factory.New<ForwardingShipment>();

			CreateTransportBooking(shipment1, "AVL");
			CreateTransportBooking(shipment1, "AVL");

			CreateTransportBooking(shipment2, "AVL");
			CreateTransportBooking(shipment2, "DLV");

			CreateTransportBooking(shipment3, "DLV");

			Factory.Save();

			var shipmentFilter = (ModuleGuidForeignCollectionFilter)FilterStripBizO["Related Transport Booking"];
			shipmentFilter.IsActive = true;
			shipmentFilter.SelectedFilters.AddTextFilterStrip("Booking Status", "AVL");
			shipmentFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

			var shipments = new ForwardingShipmentCollection(Factory);
			shipments.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);
		}

		void CreateTransportBooking(BusinessObject parent, string status)
		{
			var booking = Factory.NewWithValidTestData(ObjectFactory.GetType<IDtbBooking>(), TestBusinessObjectKind.MinimumRequiredToSave);
			booking[DtbBookingSchema.KM_Status] = status;
			var bookingConsolidation = (BusinessObject)Factory.LoadTop1<IDtbBookingConsolidation>(new ZQuery(DtbBookingConsolidationSchema.PK, booking[DtbBookingSchema.KM_KB_Booking]));
			using (bookingConsolidation.GetValidationSuspender())
			{
				bookingConsolidation[DtbBookingConsolidationSchema.KB_IsOverridden] = 1;
				bookingConsolidation[DtbBookingConsolidationSchema.KB_ParentID] = parent.PK;
				bookingConsolidation[DtbBookingConsolidationSchema.KB_ParentTableCode] = parent.TablePrefix;
			}
		}

		#region Workflow Custom Fields Filters

		public void TestAddWorkflowCustomFieldsFilters()
		{
			ModuleFilterCollection filterCollection = new JobShipmentFilterBusinessObject().ModuleFilters;

			AssertNull(filterCollection["C11"]);
			AssertNull(filterCollection["C12"]);
			AssertNull(filterCollection["C21"]);
			AssertNull(filterCollection["C22"]);
			AssertNull(filterCollection["Workflow Flags"]);
			AssertNull(filterCollection["C31"]);
			AssertNull(filterCollection["C41"]);
			AssertNull(filterCollection["C42"]);

			PrepareTemplates();

			filterCollection = new JobShipmentFilterBusinessObject().ModuleFilters;

			AssertEquals(typeof(ModuleTextFilter), filterCollection["C11"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), filterCollection["C12"].GetType());
			AssertEquals(typeof(ModuleDateFilter), filterCollection["C21"].GetType());
			AssertEquals(typeof(ModuleTextFilter), filterCollection["C22"].GetType());
			AssertEquals(typeof(ModuleFlagsFilter), filterCollection["Workflow Flags"].GetType());
			AssertNull(filterCollection["C31"]);

			// Consol templates and custom fields cannot  displayed
			AssertNull(filterCollection["C41"]);
			AssertNull(filterCollection["C42"]);
		}

		void PrepareTemplates()
		{
			ProcessTaskTemplate template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;

			GenCustomColumnDefinition def11 = template1.GenCustomColumnDefinitions.AddNew();
			def11.XC_Name = "C11";
			def11.XC_Type = AddOnColumnDataType.Codes.String;

			GenCustomColumnDefinition def12 = template1.GenCustomColumnDefinitions.AddNew();
			def12.XC_Name = "C12";
			def12.XC_Type = AddOnColumnDataType.Codes.Integer;

			ProcessTaskTemplate template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;

			GenCustomColumnDefinition def21 = template2.GenCustomColumnDefinitions.AddNew();
			def21.XC_Name = "C21";
			def21.XC_Type = AddOnColumnDataType.Codes.Datetime;

			GenCustomColumnDefinition def22 = template2.GenCustomColumnDefinitions.AddNew();
			def22.XC_Name = "C22";
			def22.XC_Type = AddOnColumnDataType.Codes.Boolean;

			GenCustomColumnDefinition defDuplicate = template2.GenCustomColumnDefinitions.AddNew();
			defDuplicate.XC_Name = "C11";
			defDuplicate.XC_Type = AddOnColumnDataType.Codes.String;

			ProcessTaskTemplate template3 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template3.P0_ProcessType = "YYY";

			GenCustomColumnDefinition def31 = template3.GenCustomColumnDefinitions.AddNew();
			def31.XC_Name = "C31";
			def31.XC_Type = AddOnColumnDataType.Codes.String;

			ProcessTaskTemplate template4 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template4.P0_ProcessType = JobInvoicingConsumerTypes.Consol.Code;
			GenCustomColumnDefinition def41 = template4.GenCustomColumnDefinitions.AddNew();
			def41.XC_Name = "C41";
			def41.XC_Type = AddOnColumnDataType.Codes.String;

			GenCustomColumnDefinition def42 = template4.GenCustomColumnDefinitions.AddNew();
			def42.XC_Name = "C42";
			def42.XC_Type = AddOnColumnDataType.Codes.Boolean;

			Factory.Save();

			WorkflowCustomFieldsFilter.ClearCache();
		}

		#endregion

		#region CRM Security

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<ForwardingShipment>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.MaintainShipmentCRMSecurity);
		}

		#endregion

		public void TestCTStatusFilter()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
				shipment1.JS_CommunityTransitStatus = ExportCommunityTransitStatusList.Codes.C;

				Factory.Save();

				ModuleTextFilter shipmentFilter = (ModuleTextFilter)FilterStripBizO["CT Status"];
				ForwardingShipmentCollection results = new ForwardingShipmentCollection(Factory);

				shipmentFilter.IsActive = true;
				shipmentFilter.Property = "C";
				results.Load(FilterStripBizO.Filter);
				AssertEquals("Should contain shipment1", true, results.Contains(shipment1));
			}
		}

		public void TestDisplayCTStatusFilter_WhenIsCountryEuOrCtCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Denmark))
			{
				var shipmentFilter = new JobShipmentFilterBusinessObject();
				var filter = (ModuleTextFilter)shipmentFilter[JobShipmentFilterBusinessObject.Descriptions.CTStatus];

				AssertNotNull("If country is part of EU, CT Status filter should be available", filter);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Switzerland))
			{
				var shipmentFilter = new JobShipmentFilterBusinessObject();
				var filter = (ModuleTextFilter)shipmentFilter[JobShipmentFilterBusinessObject.Descriptions.CTStatus];

				AssertNotNull("If country is part of Common Transit not included in EU, CT Status filter should be available", filter);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var shipmentFilter = new JobShipmentFilterBusinessObject();
				var filter = (ModuleTextFilter)shipmentFilter[JobShipmentFilterBusinessObject.Descriptions.CTStatus];

				AssertNull("If country is not part of EU nor Common Transit countries, CT Status filter should not be available", filter);
			}
		}

		public void TestProfitLossReasonFilterWithOperators()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var org1 = GetOrgHeader("APRIS");
				var org2 = GetOrgHeader("TECENT");
				var org3 = GetOrgHeader("INACTIVE");
				org3.OH_IsActive = false;

				var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
				var job1 = new JobHeader.Loader(shipment1).TryCreate();
				job1.JH_OA_LocalChargesAddr = org1.MainAddress.PK;
				job1.JH_ProfitLossReasonCode = "ND1";

				var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
				var job2 = new JobHeader.Loader(shipment2).TryCreate();
				job2.JH_OA_LocalChargesAddr = org2.MainAddress.PK;
				job2.JH_ProfitLossReasonCode = "CD1";

				var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
				var job3 = new JobHeader.Loader(shipment3).TryCreate();
				job3.JH_OA_LocalChargesAddr = org3.MainAddress.PK;
				job3.JH_ProfitLossReasonCode = string.Empty;

				Factory.Save();

				var profitLossReasonFilter = (ModuleTextFilter)FilterStripBizO["Profit/Loss Reason"];
				profitLossReasonFilter.IsActive = true;
				var shipments = new ForwardingShipmentCollection(Factory);

				profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				profitLossReasonFilter.Property = "ND1";

				shipments.Load(FilterStripBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { shipment1 }, shipments);

				profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				profitLossReasonFilter.Property = "N";

				shipments.Load(FilterStripBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { shipment1 }, shipments);

				profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
				profitLossReasonFilter.Property = "D";

				shipments.Load(FilterStripBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);

				profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
				profitLossReasonFilter.Property = "N";

				shipments.Load(FilterStripBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { shipment2, shipment3 }, shipments);

				profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
				profitLossReasonFilter.Property = "N";

				shipments.Load(FilterStripBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { shipment2, shipment3 }, shipments);

				profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
				profitLossReasonFilter.Property = "ND1";

				shipments.Load(FilterStripBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { shipment2, shipment3 }, shipments);

				profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
				profitLossReasonFilter.Property = "ND1";

				shipments.Load(FilterStripBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { shipment3 }, shipments);

				profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
				profitLossReasonFilter.Property = "ND1";

				shipments.Load(FilterStripBizO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);
			}
		}

		public void TestExitStatusFilter()
		{
			JobShipmentFilterBusinessObject filter = new JobShipmentFilterBusinessObject();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var header1 = Factory.NewWithValidTestData<ForwardingShipment>();
				header1.JS_RL_NKOrigin = "DEWIB";
				header1.JS_RL_NKDestination = "CNSHA";

				var (_, _, exitReports1) =
					ExitControlTestHelper.CreateCusExitReportWithStatus(header1, new ZString[] { "ADD" });

				var header2 = Factory.NewWithValidTestData<ForwardingShipment>();
				header2.JS_RL_NKOrigin = "DEWIB";
				header2.JS_RL_NKDestination = "CNSHB";

				var (_, _, exitReports2) =
					ExitControlTestHelper.CreateCusExitReportWithStatus(header2, new ZString[] { "ADD" });

				var header3 = Factory.NewWithValidTestData<ForwardingShipment>();
				header3.JS_RL_NKOrigin = "DEWIB";
				header3.JS_RL_NKDestination = "CNSHA";

				var (_, _, exitReports3) =
					ExitControlTestHelper.CreateCusExitReportWithStatus(header3, new ZString[] { "RED" });

				Factory.Save();

				var moduleFilter = (ModuleTextFilter)filter[JobShipmentFilterBusinessObject.Descriptions.ExitStatus];
				moduleFilter.IsActive = true;

				CombineAssertions(() =>
				{
					moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
					moduleFilter.Property = "ADD";
					AssertExitHeadersMatchFilter("Exact", (header1, true), (header2, true), (header3, false));

					moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
					moduleFilter.Property = "A";
					AssertExitHeadersMatchFilter("StartsWith", (header1, true), (header2, true), (header3, false));

					moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
					moduleFilter.Property = "D";
					AssertExitHeadersMatchFilter("Contains", (header1, true), (header2, true), (header3, true));

					moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
					moduleFilter.Property = "ADD";
					AssertExitHeadersMatchFilter("NotEqual", (header1, false), (header2, false), (header3, true));

					moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
					moduleFilter.Property = "R";
					AssertExitHeadersMatchFilter("NotStartsWith", (header1, true), (header2, true), (header3, false));

					moduleFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
					moduleFilter.Property = "D";
					AssertExitHeadersMatchFilter("NotContain", (header1, false), (header2, false), (header3, false));
				});
			}

			void AssertExitHeadersMatchFilter(ZString message, params (ForwardingShipment header, bool expected)[] headerMatches)
			{
				var headerIndex = 1;

				foreach (var (header, expected) in headerMatches)
				{
					AssertEquals($"{message}->header{headerIndex++}", expected, header.MatchesFilter(filter.Filter));
				}
			}
		}

		public static class ExitControlTestHelper
		{
			public static EUExitControl.ICusExitHeader CreateCusExitHeader(ForwardingShipment declaration) => CreateCusExitHeader(declaration.Factory, declaration.PK, declaration.TablePrefix);

			public static (EUExitControl.ICusExitHeader header, EUExitControl.ICusExitConsignment consignment, EUExitControl.ICusExitReport[] reports) CreateCusExitReportWithStatus(ForwardingShipment declaration, ZString[] reportStatus)
			{
				var header = ExitControlTestHelper.CreateCusExitHeader(declaration);
				var factory = declaration.Factory;
				var consignment = ExitControlTestHelper.CreateCusExitConsignment(factory, header.PK, header.CXH_ClusterKey, "AAAAA");
				var reports = new List<EUExitControl.ICusExitReport>();
				foreach (var status in reportStatus)
				{
					var report = ExitControlTestHelper.CreateCusExitReport(factory, header.PK, header.CXH_ClusterKey, consignment.PK, ExitReportTypeList.Codes.Presentation, "IEDUB100");
					report.CER_Status = status;
					reports.Add(report);
				}
				return (header, consignment, reports.ToArray());
			}

			public static EUExitControl.ICusExitHeader CreateCusExitHeader(BusinessObjectFactory factory, ZGuid parentId, ZString parentTableCode)
			{
				var header = factory.New<EUExitControl.ICusExitHeader>();
				header.CXH_ParentID = parentId;
				header.CXH_ParentTableCode = parentTableCode;
				return header;
			}

			public static EUExitControl.ICusExitConsignment CreateCusExitConsignment(BusinessObjectFactory factory, ZGuid headerPK, ZInt clusterKey, ZString movementReference)
			{
				var consignment = factory.New<EUExitControl.ICusExitConsignment>();
				consignment.CXC_CXH_Header = headerPK;
				consignment.CXC_ClusterKey = clusterKey;
				consignment.CXC_MovementReference = movementReference;
				return consignment;
			}

			public static EUExitControl.ICusExitReport CreateCusExitReport(BusinessObjectFactory factory, ZGuid headerPK, ZInt clusterKey, ZGuid consignmentPK, ZString type, ZString officeOfExit)
			{
				var report = factory.New<EUExitControl.ICusExitReport>();
				report.CER_CXH_Header = headerPK;
				report.CER_ClusterKey = clusterKey;
				report.CER_CXC_Consignment = consignmentPK;
				report.CER_Type = type;
				report.CER_OfficeOfExit = officeOfExit;
				report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
				return report;
			}
		}

		#region Implementation

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}

		static string GetValue(ForwardingShipment shipment, string country, string type)
		{
			foreach (CusEntryNumber number in shipment.Numbers)
			{
				if (number.CE_RN_NKCountryCode == country && number.CE_EntryType == type)
				{
					return number.CE_EntryNum;
				}
			}

			return null;
		}

		static void SetFilter(ReferenceNumberFilter filter, string country, string type, string number)
		{
			filter.Country = country;
			filter.Type = type;
			filter.Property = number;
		}

		static CusEntryNumber NewReferenceNumber(ForwardingShipment shipment, string countryCode, string type, string number)
		{
			CusEntryNumber result = shipment.Numbers.AddNew();
			result.CE_RN_NKCountryCode = countryCode;
			result.CE_EntryType = type;
			result.CE_EntryNum = number;
			return result;
		}

		#region Data Setup

		protected void SetupBaseFilterTestData()
		{
			foreach (ForwardingShipment shipment in Factory.Load<ForwardingShipment>(new ZQuery()))
			{
				shipment.JS_IsForwardRegistered = false;
				shipment.Delete();
			}

			foreach (ForwardingConsol consol in Factory.Load<ForwardingConsol>(new ZQuery()))
			{
				consol.Delete();
			}

			Shipment1 = Factory.New<ForwardingShipment>();
			Shipment2 = Factory.New<ForwardingShipment>();
			Shipment3 = Factory.New<ForwardingShipment>();
			Shipment4 = Factory.New<ForwardingShipment>();
			Shipment5 = Factory.New<ForwardingShipment>();
			Shipment6 = Factory.New<ForwardingShipment>();

			Consol1 = Shipment1.Consols.AddNew();
			Consol2 = Shipment2.Consols.AddNew();
			Shipment2.Consols.Add(Consol1);
			Shipment3.Consols.Add(Consol1);
			Shipment4.Consols.Add(Consol1);
			Shipment6.Consols.Add(Consol1);

			MasterFiles.Business.GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";

			Shipment1.JS_IsForwardRegistered = true;
			Shipment2.JS_IsForwardRegistered = true;
			Shipment3.JS_IsForwardRegistered = true;
			Shipment4.JS_IsForwardRegistered = false;
			Shipment5.JS_IsForwardRegistered = false;
			Shipment6.JS_IsForwardRegistered = true;

			Shipment1.JS_IsCFSRegistered = true;
			Shipment2.JS_IsCFSRegistered = true;
			Shipment3.JS_IsCFSRegistered = false;
			Shipment4.JS_IsCFSRegistered = false;
			Shipment5.JS_IsCFSRegistered = true;
			Shipment6.JS_IsCFSRegistered = false;

			Shipment1.JS_IsBooking = false;
			Shipment2.JS_IsBooking = true;
			Shipment3.JS_IsBooking = false;
			Shipment4.JS_IsBooking = true;
			Shipment5.JS_IsBooking = false;
			Shipment6.JS_IsBooking = false;

			Factory.Save();

			Shipment4.JS_IsForwardRegistered = false;
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			Shipment1 = Factory.New<ForwardingShipment>();
			Shipment2 = Factory.New<ForwardingShipment>();
			Shipment3 = Factory.New<ForwardingShipment>();
			Shipment4 = Factory.New<ForwardingShipment>();
			Shipment5 = Factory.New<ForwardingShipment>();
			Shipment6 = Factory.New<ForwardingShipment>();

			Consol1 = Shipment1.Consols.AddNew();
			Consol2 = Shipment2.Consols.AddNew();
			Shipment2.Consols.Add(Consol1);
			Shipment3.Consols.Add(Consol1);
			Shipment4.Consols.Add(Consol1);
			Shipment6.Consols.Add(Consol1);

			MasterFiles.Business.GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUBNE";

			Factory.Save();
		}

		ForwardingConsol Consol1;
		ForwardingConsol Consol2;
		protected ForwardingShipment Shipment1;
		protected ForwardingShipment Shipment2;
		protected ForwardingShipment Shipment3;
		protected ForwardingShipment Shipment4;
		protected ForwardingShipment Shipment5;
		protected ForwardingShipment Shipment6;

		#endregion

		#region Setup Related Parties

		OrgHeader GetOrgHeader(ZString fullName)
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			result.OH_FullName = fullName;

			return result;
		}

		OrgRelatedParty GetOrgRelatedParty(OrgHeader parent, OrgHeader relatedParty)
		{
			OrgRelatedParty result = Factory.NewWithValidTestData<OrgRelatedParty>();
			result.PR_OH_Parent = parent.PK;
			result.PR_OH_RelatedParty = relatedParty.PK;
			result.PR_PartyType = ZString.Empty;
			result.PR_FreightDirection = ZString.Empty;
			result.PR_FreightTransportMode = ZString.Empty;
			result.PR_FreightContainerMode = ZString.Empty;

			return result;
		}

		ForwardingShipment GetShipmentWithConsignor(ZString companyName)
		{
			ForwardingShipment result = GetShipment((++shipmentNumberIndex).ToString());
			JobDocAddress docAddress = result.ConsignorDocumentaryAddress;
			docAddress.E2_OA_Address = GetOrgHeader(companyName).MainAddress.PK;

			return result;
		}
		ForwardingShipment GetShipmentWithConsignee(ZString companyName)
		{
			ForwardingShipment result = GetShipment((++shipmentNumberIndex).ToString());
			JobDocAddress docAddress = result.ConsigneeDocumentaryAddress;
			docAddress.E2_OA_Address = GetOrgHeader(companyName).MainAddress.PK;

			return result;
		}
		ForwardingShipment GetShipmentWithLocalClient(ZString companyName)
		{
			ForwardingShipment result = GetShipment((++shipmentNumberIndex).ToString());

			JobHeader job = new JobHeader.Loader(result).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_OA_LocalChargesAddr = GetOrgHeader(companyName).MainAddress.PK;

			return result;
		}

		ForwardingShipment GetShipmentWithControllingCustomer(ZString companyName)
		{
			ForwardingShipment result = GetShipment((++shipmentNumberIndex).ToString());
			JobDocAddress docAddress = result.ControllingCustomerAddress;
			docAddress.E2_OA_Address = GetOrgHeader(companyName).MainAddress.PK;

			return result;
		}

		ForwardingShipment GetShipment(ZString numberSuffix)
		{
			ForwardingShipment result = Factory.NewWithValidTestData<ForwardingShipment>();
			result.JS_UniqueConsignRef = "Shipment" + numberSuffix;
			Asserter.AddToScope(result);

			return result;
		}

		int shipmentNumberIndex;

		FilterStripAsserter<ForwardingShipment> Asserter
		{
			get
			{
				if (asserter == null)
				{
					asserter = new FilterStripAsserter<ForwardingShipment>(Factory, (s) => s.JS_UniqueConsignRef);
				}
				return asserter;
			}
		}
		FilterStripAsserter<ForwardingShipment> asserter;

		#endregion

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new JobShipmentFilterBusinessObject();
		}

		protected void ResetFilterStripBizO()
		{
			fFilterStripBizO = null;
		}

		protected FilterStripBusinessObject FilterStripBizO
		{
			get
			{
				if (fFilterStripBizO == null)
				{
					fFilterStripBizO = GetNewFilterStripBusinessObject();
				}
				return fFilterStripBizO;
			}
		}

		FilterStripBusinessObject fFilterStripBizO;

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			return FiltersExcludedFromSubgroupCheckForCommonTables;
		}

		internal static List<Tuple<string, string>> FiltersExcludedFromSubgroupCheckForCommonTables
		{
			get
			{
				var result = new List<Tuple<string, string>>();
				result.Add(new Tuple<string, string>("JobComInvoiceHeader", "Invoice Line Product Code"));
				result.Add(new Tuple<string, string>("JobComInvoiceLine", "Invoice Line Product Code"));
				result.Add(new Tuple<string, string>("JobDeclaration", "Invoice Line Product Code"));
				result.Add(new Tuple<string, string>("JobHeader", "Quote #"));
				result.Add(new Tuple<string, string>("ViewQuotedBooking", "Quote #"));
				result.Add(new Tuple<string, string>("JobConShipLink", "Consol #"));
				result.Add(new Tuple<string, string>("JobConsol", "Consol #"));
				result.Add(new Tuple<string, string>("JobContainer", "Container #"));
				result.Add(new Tuple<string, string>("JobContainerPackPivot", "Container #"));
				result.Add(new Tuple<string, string>("JobPackLines", "Container #"));
				result.Add(new Tuple<string, string>("CusEntryHeader", "Customs Entry #"));
				result.Add(new Tuple<string, string>("CusEntryNum", "Customs Entry #"));
				result.Add(new Tuple<string, string>("JobDeclaration", "Customs Entry #"));
				result.Add(new Tuple<string, string>("JobShipment", "Customs Entry #"));
				result.Add(new Tuple<string, string>("JobConShipLink", "Master Bill"));
				result.Add(new Tuple<string, string>("JobConsol", "Master Bill"));
				result.Add(new Tuple<string, string>("JobDocsAndCartage", "Order #"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "Order #"));
				result.Add(new Tuple<string, string>("JobOrderItem", "Order #"));
				result.Add(new Tuple<string, string>("WhsDocket", "Order #"));
				result.Add(new Tuple<string, string>("WhsDocketJobPivot", "Order #"));
				result.Add(new Tuple<string, string>("JobConShipLink", "Flight/Voyage # and Vessel"));
				result.Add(new Tuple<string, string>("JobConsolTransport", "Flight/Voyage # and Vessel"));
				result.Add(new Tuple<string, string>("JobSailing", "Flight/Voyage # and Vessel"));
				result.Add(new Tuple<string, string>("JobShipment", "Flight/Voyage # and Vessel"));
				result.Add(new Tuple<string, string>("JobVoyage", "Flight/Voyage # and Vessel"));
				result.Add(new Tuple<string, string>("JobVoyDestination", "Flight/Voyage # and Vessel"));
				result.Add(new Tuple<string, string>("JobPackLines", "Commodity Code"));
				result.Add(new Tuple<string, string>("JobShipment", "Direct Master/Lead Shipment #"));
				result.Add(new Tuple<string, string>("CusEntryNum", "Additional Reference #"));
				result.Add(new Tuple<string, string>("JobHeader", "Job Local Reference"));
				result.Add(new Tuple<string, string>("JobDocAddress", "Consignor / Consignee"));
				result.Add(new Tuple<string, string>("OrgAddress", "Consignor / Consignee"));
				result.Add(new Tuple<string, string>("JobConShipLink", "Consol Send / Receive Agents"));
				result.Add(new Tuple<string, string>("JobConsol", "Consol Send / Receive Agents"));
				result.Add(new Tuple<string, string>("OrgAddress", "Consol Send / Receive Agents"));
				result.Add(new Tuple<string, string>("JobDocAddress", "Shipment Send / Receive Forwarders"));
				result.Add(new Tuple<string, string>("OrgAddress", "Shipment Send / Receive Forwarders"));
				result.Add(new Tuple<string, string>("JobDocAddress", "Company Name"));
				result.Add(new Tuple<string, string>("OrgAddress", "Company Name"));
				result.Add(new Tuple<string, string>("OrgHeader", "Company Name"));
				result.Add(new Tuple<string, string>("JobDocAddress", "Consignor Company Name"));
				result.Add(new Tuple<string, string>("OrgAddress", "Consignor Company Name"));
				result.Add(new Tuple<string, string>("OrgHeader", "Consignor Company Name"));
				result.Add(new Tuple<string, string>("JobDocAddress", "Consignee Company Name"));
				result.Add(new Tuple<string, string>("OrgAddress", "Consignee Company Name"));
				result.Add(new Tuple<string, string>("OrgHeader", "Consignee Company Name"));
				result.Add(new Tuple<string, string>("JobDocAddress", "Notify Party Company Name"));
				result.Add(new Tuple<string, string>("OrgAddress", "Notify Party Company Name"));
				result.Add(new Tuple<string, string>("OrgHeader", "Notify Party Company Name"));
				result.Add(new Tuple<string, string>("GlbStaff", "Cartage Coordinator"));
				result.Add(new Tuple<string, string>("JobDocAddress", "Cartage Coordinator"));
				result.Add(new Tuple<string, string>("OrgAddress", "Cartage Coordinator"));
				result.Add(new Tuple<string, string>("OrgHeader", "Cartage Coordinator"));
				result.Add(new Tuple<string, string>("OrgStaffAssignments", "Cartage Coordinator"));
				result.Add(new Tuple<string, string>("JobHeader", "Sales Rep"));
				result.Add(new Tuple<string, string>("JobDocAddress", "Consignor Related Parties"));
				result.Add(new Tuple<string, string>("OrgAddress", "Consignor Related Parties"));
				result.Add(new Tuple<string, string>("OrgHeader", "Consignor Related Parties"));
				result.Add(new Tuple<string, string>("OrgRelatedParty", "Consignor Related Parties"));
				result.Add(new Tuple<string, string>("JobDocAddress", "Consignee Related Parties"));
				result.Add(new Tuple<string, string>("OrgAddress", "Consignee Related Parties"));
				result.Add(new Tuple<string, string>("OrgHeader", "Consignee Related Parties"));
				result.Add(new Tuple<string, string>("OrgRelatedParty", "Consignee Related Parties"));
				result.Add(new Tuple<string, string>("JobHeader", "Local Client Related Parties"));
				result.Add(new Tuple<string, string>("OrgAddress", "Local Client Related Parties"));
				result.Add(new Tuple<string, string>("OrgHeader", "Local Client Related Parties"));
				result.Add(new Tuple<string, string>("OrgRelatedParty", "Local Client Related Parties"));
				result.Add(new Tuple<string, string>("JobDocAddress", "Client Assigned Staff"));
				result.Add(new Tuple<string, string>("OrgAddress", "Client Assigned Staff"));
				result.Add(new Tuple<string, string>("OrgStaffAssignments", "Client Assigned Staff"));
				result.Add(new Tuple<string, string>("JobHeader", "Job Branch"));
				result.Add(new Tuple<string, string>("JobHeader", "Job Department"));
				result.Add(new Tuple<string, string>("JobHeader", "Job Operation Staff"));
				result.Add(new Tuple<string, string>("JobHeader", "Job Sales Staff"));
				result.Add(new Tuple<string, string>("GlbBranch", "Job Branch Management Code"));
				result.Add(new Tuple<string, string>("JobHeader", "Job Branch Management Code"));
				result.Add(new Tuple<string, string>("JobConShipLink", "Load / Discharge"));
				result.Add(new Tuple<string, string>("JobConsolTransport", "Load / Discharge"));
				result.Add(new Tuple<string, string>("JobSailing", "Load / Discharge"));
				result.Add(new Tuple<string, string>("JobVoyOrigin", "Load / Discharge"));
				result.Add(new Tuple<string, string>("JobDocAddress", "Pickup Address Post Code"));
				result.Add(new Tuple<string, string>("OrgAddress", "Pickup Address Post Code"));
				result.Add(new Tuple<string, string>("JobDocAddress", "Delivery Address Post Code"));
				result.Add(new Tuple<string, string>("OrgAddress", "Delivery Address Post Code"));
				result.Add(new Tuple<string, string>("JobConShipLink", "Consol Container Mode"));
				result.Add(new Tuple<string, string>("JobConsol", "Consol Container Mode"));
				result.Add(new Tuple<string, string>("JobConShipLink", "Consol Transport Mode"));
				result.Add(new Tuple<string, string>("JobConsol", "Consol Transport Mode"));
				result.Add(new Tuple<string, string>("CusHAWB", "Air Cargo Customs Status"));
				result.Add(new Tuple<string, string>("CusMAWB", "Air Cargo Customs Status"));
				result.Add(new Tuple<string, string>("CusHAWB", "Air Cargo Message Status"));
				result.Add(new Tuple<string, string>("CusMAWB", "Air Cargo Message Status"));
				result.Add(new Tuple<string, string>("CusSCAHouse", "Sea Cargo Customs Status"));
				result.Add(new Tuple<string, string>("CusSCAOceanBill", "Sea Cargo Customs Status"));
				result.Add(new Tuple<string, string>("CusSCAPivot", "Sea Cargo Customs Status"));
				result.Add(new Tuple<string, string>("CusSCAHouse", "Sea Cargo Message Status"));
				result.Add(new Tuple<string, string>("CusSCAOceanBill", "Sea Cargo Message Status"));
				result.Add(new Tuple<string, string>("GlbBranch", "Customs Entry Status"));
				result.Add(new Tuple<string, string>("JobDeclaration", "Customs Entry Status"));
				result.Add(new Tuple<string, string>("CusEntryNum", "ISF Bill Status"));
				result.Add(new Tuple<string, string>("CusISFBill", "ISF Bill Status"));
				result.Add(new Tuple<string, string>("CusISFHeader", "ISF Bill Status"));
				result.Add(new Tuple<string, string>("JobConShipLink", "ISF Bill Status"));
				result.Add(new Tuple<string, string>("JobConsol", "ISF Bill Status"));
				result.Add(new Tuple<string, string>("JobShipment", "ISF Bill Status"));
				result.Add(new Tuple<string, string>("OrgAddress", "ISF Bill Status"));
				result.Add(new Tuple<string, string>("OrgCusCode", "ISF Bill Status"));
				result.Add(new Tuple<string, string>("CusEntryNum", "Security Inspection"));
				result.Add(new Tuple<string, string>("JobShipment", "Security Inspection"));
				result.Add(new Tuple<string, string>("JobConShipLink", "AFR Bill Status"));
				result.Add(new Tuple<string, string>("JobConsol", "AFR Bill Status"));
				result.Add(new Tuple<string, string>("JobShipment", "AFR Bill Status"));
				result.Add(new Tuple<string, string>("JPAFRBills", "AFR Bill Status"));
				result.Add(new Tuple<string, string>("JPAFRHeader", "AFR Bill Status"));
				result.Add(new Tuple<string, string>("CusCAeMHHouse", "ACI Cargo Status"));
				result.Add(new Tuple<string, string>("CusSCAHouse", "ACI Cargo Status"));
				result.Add(new Tuple<string, string>("CusCAeMHHouse", "ACI Message Status"));
				result.Add(new Tuple<string, string>("CusSCAHouse", "ACI Message Status"));
				result.Add(new Tuple<string, string>("JobConShipLink", "Related Consolidations"));
				result.Add(new Tuple<string, string>("JobConsol", "Related Consolidations"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "Part Attribute 1"));
				result.Add(new Tuple<string, string>("JobOrderLine", "Part Attribute 1"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "Part Attribute 2"));
				result.Add(new Tuple<string, string>("JobOrderLine", "Part Attribute 2"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "Part Attribute 3"));
				result.Add(new Tuple<string, string>("JobOrderLine", "Part Attribute 3"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderLine.CustomAttrib1"));
				result.Add(new Tuple<string, string>("JobOrderLine", "OrderLine.CustomAttrib1"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderLine.CustomAttrib2"));
				result.Add(new Tuple<string, string>("JobOrderLine", "OrderLine.CustomAttrib2"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderLine.CustomAttrib3"));
				result.Add(new Tuple<string, string>("JobOrderLine", "OrderLine.CustomAttrib3"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderLine.CustomAttrib4"));
				result.Add(new Tuple<string, string>("JobOrderLine", "OrderLine.CustomAttrib4"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderLine.CustomAttrib5"));
				result.Add(new Tuple<string, string>("JobOrderLine", "OrderLine.CustomAttrib5"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderLine.CustomAttrib6"));
				result.Add(new Tuple<string, string>("JobOrderLine", "OrderLine.CustomAttrib6"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderLine.CustomText1"));
				result.Add(new Tuple<string, string>("JobOrderLine", "OrderLine.CustomText1"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderLine.CustomFlag1"));
				result.Add(new Tuple<string, string>("JobOrderLine", "OrderLine.CustomFlag1"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderLine.CustomFlag2"));
				result.Add(new Tuple<string, string>("JobOrderLine", "OrderLine.CustomFlag2"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderLine.CustomFlag3"));
				result.Add(new Tuple<string, string>("JobOrderLine", "OrderLine.CustomFlag3"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderLine.CustomFlag4"));
				result.Add(new Tuple<string, string>("JobOrderLine", "OrderLine.CustomFlag4"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderLine.CustomFlag5"));
				result.Add(new Tuple<string, string>("JobOrderLine", "OrderLine.CustomFlag5"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderLine.CustomDecimal1"));
				result.Add(new Tuple<string, string>("JobOrderLine", "OrderLine.CustomDecimal1"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderLine.CustomDecimal2"));
				result.Add(new Tuple<string, string>("JobOrderLine", "OrderLine.CustomDecimal2"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderLine.CustomDecimal3"));
				result.Add(new Tuple<string, string>("JobOrderLine", "OrderLine.CustomDecimal3"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderLine.CustomDecimal4"));
				result.Add(new Tuple<string, string>("JobOrderLine", "OrderLine.CustomDecimal4"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderLine.CustomDecimal5"));
				result.Add(new Tuple<string, string>("JobOrderLine", "OrderLine.CustomDecimal5"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderHeader.CustomAttrib1"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderHeader.CustomAttrib2"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderHeader.CustomAttrib3"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderHeader.CustomAttrib4"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderHeader.CustomAttrib5"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderHeader.CustomFlag1"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderHeader.CustomFlag2"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderHeader.CustomFlag3"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderHeader.CustomFlag4"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderHeader.CustomFlag5"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderHeader.CustomDecimal1"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderHeader.CustomDecimal2"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderHeader.CustomDecimal3"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderHeader.CustomDecimal4"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderHeader.CustomDecimal5"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderHeader.CustomContact1"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderHeader.CustomContact2"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderHeader.GoodsOrigin"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderHeader.GoodsDestination"));
				result.Add(new Tuple<string, string>("JobOrderHeader", "OrderHeader.GoodsDestination"));
				result.Add(new Tuple<string, string>("JobOrderLine", "OrderHeader.GoodsDestination"));
				result.Add(new Tuple<string, string>("JobComInvoiceHeader", "ComInvoiceLine.CustomAttribute1"));
				result.Add(new Tuple<string, string>("JobComInvoiceLine", "ComInvoiceLine.CustomAttribute1"));
				result.Add(new Tuple<string, string>("JobDeclaration", "ComInvoiceLine.CustomAttribute1"));
				result.Add(new Tuple<string, string>("JobComInvoiceHeader", "ComInvoiceLine.CustomAttribute2"));
				result.Add(new Tuple<string, string>("JobComInvoiceLine", "ComInvoiceLine.CustomAttribute2"));
				result.Add(new Tuple<string, string>("JobDeclaration", "ComInvoiceLine.CustomAttribute2"));
				result.Add(new Tuple<string, string>("JobComInvoiceHeader", "ComInvoiceLine.CustomAttribute3"));
				result.Add(new Tuple<string, string>("JobComInvoiceLine", "ComInvoiceLine.CustomAttribute3"));
				result.Add(new Tuple<string, string>("JobDeclaration", "ComInvoiceLine.CustomAttribute3"));
				result.Add(new Tuple<string, string>("JobComInvoiceHeader", "ComInvoiceLine.CustomAttribute4"));
				result.Add(new Tuple<string, string>("JobComInvoiceLine", "ComInvoiceLine.CustomAttribute4"));
				result.Add(new Tuple<string, string>("JobDeclaration", "ComInvoiceLine.CustomAttribute4"));
				result.Add(new Tuple<string, string>("JobComInvoiceHeader", "ComInvoiceLine.CustomAttribute5"));
				result.Add(new Tuple<string, string>("JobComInvoiceLine", "ComInvoiceLine.CustomAttribute5"));
				result.Add(new Tuple<string, string>("JobDeclaration", "ComInvoiceLine.CustomAttribute5"));
				result.Add(new Tuple<string, string>("JobComInvoiceHeader", "ComInvoiceLine.CustomAttribute6"));
				result.Add(new Tuple<string, string>("JobComInvoiceLine", "ComInvoiceLine.CustomAttribute6"));
				result.Add(new Tuple<string, string>("JobDeclaration", "ComInvoiceLine.CustomAttribute6"));
				result.Add(new Tuple<string, string>("JobComInvoiceHeader", "ComInvoiceLine.CustomText1"));
				result.Add(new Tuple<string, string>("JobComInvoiceLine", "ComInvoiceLine.CustomText1"));
				result.Add(new Tuple<string, string>("JobDeclaration", "ComInvoiceLine.CustomText1"));
				result.Add(new Tuple<string, string>("JobComInvoiceHeader", "ComInvoiceLine.CustomFlag1"));
				result.Add(new Tuple<string, string>("JobComInvoiceLine", "ComInvoiceLine.CustomFlag1"));
				result.Add(new Tuple<string, string>("JobDeclaration", "ComInvoiceLine.CustomFlag1"));
				result.Add(new Tuple<string, string>("JobComInvoiceHeader", "ComInvoiceLine.CustomFlag2"));
				result.Add(new Tuple<string, string>("JobComInvoiceLine", "ComInvoiceLine.CustomFlag2"));
				result.Add(new Tuple<string, string>("JobDeclaration", "ComInvoiceLine.CustomFlag2"));
				result.Add(new Tuple<string, string>("JobComInvoiceHeader", "ComInvoiceLine.CustomFlag3"));
				result.Add(new Tuple<string, string>("JobComInvoiceLine", "ComInvoiceLine.CustomFlag3"));
				result.Add(new Tuple<string, string>("JobDeclaration", "ComInvoiceLine.CustomFlag3"));
				result.Add(new Tuple<string, string>("JobComInvoiceHeader", "ComInvoiceLine.CustomDecimal1"));
				result.Add(new Tuple<string, string>("JobComInvoiceLine", "ComInvoiceLine.CustomDecimal1"));
				result.Add(new Tuple<string, string>("JobDeclaration", "ComInvoiceLine.CustomDecimal1"));
				result.Add(new Tuple<string, string>("JobComInvoiceHeader", "ComInvoiceLine.CustomDecimal2"));
				result.Add(new Tuple<string, string>("JobComInvoiceLine", "ComInvoiceLine.CustomDecimal2"));
				result.Add(new Tuple<string, string>("JobDeclaration", "ComInvoiceLine.CustomDecimal2"));
				result.Add(new Tuple<string, string>("JobComInvoiceHeader", "ComInvoiceLine.CustomDecimal3"));
				result.Add(new Tuple<string, string>("JobComInvoiceLine", "ComInvoiceLine.CustomDecimal3"));
				result.Add(new Tuple<string, string>("JobDeclaration", "ComInvoiceLine.CustomDecimal3"));
				result.Add(new Tuple<string, string>("JobComInvoiceHeader", "Any Commercial Invoice Text Attribute'"));
				result.Add(new Tuple<string, string>("JobComInvoiceLine", "Any Commercial Invoice Text Attribute'"));
				result.Add(new Tuple<string, string>("JobDeclaration", "Any Commercial Invoice Text Attribute'"));
				result.Add(new Tuple<string, string>("AccTransactionHeader", "AP Invoice #"));
				result.Add(new Tuple<string, string>("AccTransactionLines", "AP Invoice #"));
				result.Add(new Tuple<string, string>("JobHeader", "AP Invoice #"));
				result.Add(new Tuple<string, string>("AccTransactionHeader", "AR Transaction #"));
				result.Add(new Tuple<string, string>("AccTransactionLines", "AR Transaction #"));
				result.Add(new Tuple<string, string>("JobHeader", "AR Transaction #"));
				result.Add(new Tuple<string, string>("JobCharge", "Supplier Cost Reference"));
				result.Add(new Tuple<string, string>("JobHeader", "Supplier Cost Reference"));
				result.Add(new Tuple<string, string>("AccTransactionLines", "Job Cost Amount"));
				result.Add(new Tuple<string, string>("JobHeader", "Job Cost Amount"));
				result.Add(new Tuple<string, string>("AccTransactionLines", "Job Accrual Amount"));
				result.Add(new Tuple<string, string>("JobCharge", "Job Accrual Amount"));
				result.Add(new Tuple<string, string>("JobHeader", "Job Accrual Amount"));
				result.Add(new Tuple<string, string>("AccTransactionLines", "Job Profit Amount"));
				result.Add(new Tuple<string, string>("JobHeader", "Job Profit Amount"));
				result.Add(new Tuple<string, string>("AccTransactionLines", "Job Revenue Amount"));
				result.Add(new Tuple<string, string>("JobHeader", "Job Revenue Amount"));
				result.Add(new Tuple<string, string>("AccTransactionLines", "Job WIP Amount"));
				result.Add(new Tuple<string, string>("JobCharge", "Job WIP Amount"));
				result.Add(new Tuple<string, string>("JobHeader", "Job WIP Amount"));
				result.Add(new Tuple<string, string>("AccTransactionLines", "Job WIP Amount (Excluding Deferred Charges)"));
				result.Add(new Tuple<string, string>("JobCharge", "Job WIP Amount (Excluding Deferred Charges)"));
				result.Add(new Tuple<string, string>("JobHeader", "Job WIP Amount (Excluding Deferred Charges)"));
				result.Add(new Tuple<string, string>("AccTransactionLines", "Job WIP Amount (Deferred Charges Only)"));
				result.Add(new Tuple<string, string>("JobCharge", "Job WIP Amount (Deferred Charges Only)"));
				result.Add(new Tuple<string, string>("JobHeader", "Job WIP Amount (Deferred Charges Only)"));
				result.Add(new Tuple<string, string>("AccTransactionLines", "Job Margin %"));
				result.Add(new Tuple<string, string>("JobCharge", "Job Margin %"));
				result.Add(new Tuple<string, string>("JobHeader", "Job Margin %"));

				result.Add(new Tuple<string, string>("JobOrderHeader", "Any Order Manager Text Attribute"));
				result.Add(new Tuple<string, string>("JobOrderLine", "Any Order Manager Text Attribute"));
				result.Add(new Tuple<string, string>("JobComInvoiceHeader", "Any Commercial Invoice Text Attribute"));
				result.Add(new Tuple<string, string>("JobComInvoiceLine", "Any Commercial Invoice Text Attribute"));
				result.Add(new Tuple<string, string>("JobDeclaration", "Any Commercial Invoice Text Attribute"));
				result.Add(new Tuple<string, string>("ProcessTasks", "Milestone Completed"));
				result.Add(new Tuple<string, string>("ProcessTasks", "Any Open Task Assigned To"));
				result.Add(new Tuple<string, string>("ProcessTasks", "Next Task Assigned To"));
				result.Add(new Tuple<string, string>("ProcessTasks", "Tasks"));
				result.Add(new Tuple<string, string>("ProcessTasks", "Exceptions"));
				result.Add(new Tuple<string, string>("ProcessTasks", "Milestones"));
				result.Add(new Tuple<string, string>("ProcessTasks", "Triggers"));
				result.Add(new Tuple<string, string>("JobConShipLink", "Milestone Completed (Related)"));
				result.Add(new Tuple<string, string>("ProcessTasks", "Milestone Completed (Related)"));

				result.Add(new Tuple<string, string>("JobHeader", "Overseas Agent (Billing)"));
				result.Add(new Tuple<string, string>("JobHeader", "Branch (Current Co.)"));
				result.Add(new Tuple<string, string>("JobHeader", "Invoice Status"));
				result.Add(new Tuple<string, string>("OrgAddress", "PickupCFS"));
				result.Add(new Tuple<string, string>("OrgAddress", "DeliveryCFS"));
				result.Add(new Tuple<string, string>("OrgAddress", "Pickup Agent"));
				result.Add(new Tuple<string, string>("OrgAddress", "Local Client (Billing)"));
				result.Add(new Tuple<string, string>("OrgAddress", "Overseas Agent (Billing)"));
				result.Add(new Tuple<string, string>("OrgAddress", "Controlling Agent"));
				result.Add(new Tuple<string, string>("OrgAddress", "Controlling Customer"));
				result.Add(new Tuple<string, string>("OrgAddress", "Consignor"));
				result.Add(new Tuple<string, string>("OrgAddress", "Consignee"));
				result.Add(new Tuple<string, string>("OrgHeader", "DeliveryCFS"));

				result.Add(new Tuple<string, string>("JobDocAddress", "Pickup Agent"));
				result.Add(new Tuple<string, string>("JobDocAddress", "Controlling Agent"));
				result.Add(new Tuple<string, string>("JobDocAddress", "Controlling Customer"));
				result.Add(new Tuple<string, string>("JobDocAddress", "Consignee"));
				result.Add(new Tuple<string, string>("JobDocAddress", "Consignor"));

				return result;
			}
		}
	}

	#endregion
}
