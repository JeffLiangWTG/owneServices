using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class WorkflowCustomFieldsFilterTest : TestCaseWithFactory
	{
		public void TestAddWorkflowCustomFieldsFilters()
		{
			ModuleFilterCollection filterCollection = new ModuleFilterCollection();

			AssertNull(filterCollection["C11"]);
			AssertNull(filterCollection["C12"]);
			AssertNull(filterCollection["C21"]);
			AssertNull(filterCollection["C22"]);
			AssertNull(filterCollection["Workflow Flags"]);
			AssertNull(filterCollection["C31"]);

			filterCollection.AddWorkflowCustomFieldsFilters(Factory, "XXX", typeof(DummyBusinessObject));

			AssertEquals(typeof(ModuleTextFilter), filterCollection["C11"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), filterCollection["C12"].GetType());
			AssertEquals(typeof(ModuleTextFilter), filterCollection["C13 Code"].GetType());
			AssertEquals(typeof(ModuleTextFilter), filterCollection["C13 Description"].GetType());
			AssertEquals(typeof(ModuleDateFilter), filterCollection["C21"].GetType());
			AssertEquals(typeof(ModuleFlagsFilter), filterCollection["Workflow Flags"].GetType());
			AssertEquals(typeof(ModuleTextFilter), filterCollection["C22"].GetType());
			AssertNull(filterCollection["C31"]);
		}

		public void TestAddWorkflowCustomFieldsFilters_HandlesDuplicates()
		{
			var filterCollection = new ModuleFilterCollection();
			filterCollection.AddWorkflowCustomFieldsFilters(Factory, "XXX", typeof(DummyBusinessObject));
			AssertEquals(typeof(ModuleTextFilter), filterCollection["Bung (STR)"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), filterCollection["bung (INT)"].GetType());
			AssertEquals(typeof(ModuleDateFilter), filterCollection["Bung (DAT)"].GetType());
		}

		public void TestAddWorkflowCustomFieldsFilters_HandlesDuplicates_FindOneType()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var value1 = Factory.New<GenCustomAddOnValue>();
			value1.XV_Name = "bung";
			value1.XV_Data = "22";
			value1.XV_Type = AddOnColumnDataType.Codes.String;
			value1.XV_ParentID = dummy1.PK;
			value1.XV_ParentTableCode = dummy1.TablePrefix;

			var dummy2 = Factory.New<DummyBusinessObject>();
			var value2 = Factory.New<GenCustomAddOnValue>();
			value2.XV_Name = "bung";
			value2.XV_Data = "22";
			value2.XV_Type = AddOnColumnDataType.Codes.Integer;
			value2.XV_ParentID = dummy2.PK;
			value2.XV_ParentTableCode = dummy2.TablePrefix;

			Factory.Save();

			var filterCollection = new ModuleFilterCollection();
			filterCollection.AddWorkflowCustomFieldsFilters(Factory, "XXX", typeof(DummyBusinessObject));
			var filter = (ModuleNumberRangeFilter)filterCollection["bung (INT)"];
			filter.IsActive = true;
			filter.Property1 = 22;

			AssertContainsExactElementsInAnyOrder(new[] { dummy2.PK }, Factory.Load<DummyBusinessObject>(filter.Query).Select(s => s.PK));
		}

		public void TestAddWorkflowCustomFieldsFiltersNoDuplicates()
		{
			ModuleFilterCollection filterCollection = new ModuleFilterCollection();

			filterCollection.AddTextFilter("C21", DummyBizoSchema.Z0_VarCharMax);

			AssertNull(filterCollection["C11"]);
			AssertNull(filterCollection["C12"]);
			AssertNotNull(filterCollection["C21"]);
			AssertNull(filterCollection["C21 (WF)"]);
			AssertNull(filterCollection["C22"]);
			AssertNull(filterCollection["Workflow Flags"]);
			AssertNull(filterCollection["C31"]);

			filterCollection.AddWorkflowCustomFieldsFilters(Factory, "XXX", typeof(DummyBusinessObject));

			AssertEquals(typeof(ModuleTextFilter), filterCollection["C11"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), filterCollection["C12"].GetType());
			AssertEquals(typeof(ModuleTextFilter), filterCollection["C21"].GetType());
			AssertEquals(typeof(ModuleDateFilter), filterCollection["C21 (WF)"].GetType());
			AssertEquals(typeof(ModuleFlagsFilter), filterCollection["Workflow Flags"].GetType());
			AssertEquals(typeof(ModuleTextFilter), filterCollection["C22"].GetType());
			AssertNull(filterCollection["C31"]);
		}

		public void TestFilterByString()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			GenCustomAddOnValue value1 = Factory.New<GenCustomAddOnValue>();
			value1.XV_Name = "C11";
			value1.XV_Data = "AAA";
			value1.XV_Type = AddOnColumnDataType.Codes.String;
			value1.XV_ParentID = dummy1.PK;
			value1.XV_ParentTableCode = dummy1.TablePrefix;

			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			GenCustomAddOnValue value2 = Factory.New<GenCustomAddOnValue>();
			value2.XV_Name = "C11";
			value2.XV_Data = "BBB";
			value2.XV_Type = AddOnColumnDataType.Codes.String;
			value2.XV_ParentID = dummy2.PK;
			value2.XV_ParentTableCode = dummy2.TablePrefix;

			Factory.Save();

			ModuleFilterCollection filterCollection = new ModuleFilterCollection();
			filterCollection.AddWorkflowCustomFieldsFilters(Factory, "XXX", typeof(DummyBusinessObject));
			ModuleTextFilter textFilter = (ModuleTextFilter)filterCollection["C11"];
			textFilter.Property = "AAA";

			DummyBusinessObject[] result = new BusinessObjectFactory().Load<DummyBusinessObject>(textFilter.Query);

			AssertEquals(1, result.Length);
			AssertEquals(dummy1.PK, result[0].PK);

			textFilter.Property = "BBB";

			result = new BusinessObjectFactory().Load<DummyBusinessObject>(textFilter.Query);

			AssertEquals(1, result.Length);
			AssertEquals(dummy2.PK, result[0].PK);
		}

		public void TestCustomTextFilterMaxLength()
		{
			ModuleFilterCollection filterCollection = new ModuleFilterCollection();
			filterCollection.AddWorkflowCustomFieldsFilters(Factory, "XXX", typeof(DummyBusinessObject));
			ModuleTextFilter textFilter = (ModuleTextFilter)filterCollection["C11"];
			AssertEquals("MaxLength of Pallet ID (Destination) should be set correctly.", GenCustomAddOnValueSchema.XV_Data.MaxLength, textFilter.MaxLength);
		}

		public void TestFilterByString_NotContain()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var value1 = Factory.New<GenCustomAddOnValue>();
			value1.XV_Name = "C11";
			value1.XV_Data = "AAA";
			value1.XV_Type = AddOnColumnDataType.Codes.String;
			value1.XV_ParentID = dummy1.PK;
			value1.XV_ParentTableCode = dummy1.TablePrefix;

			var dummy2 = Factory.New<DummyBusinessObject>();
			var value2 = Factory.New<GenCustomAddOnValue>();
			value2.XV_Name = "C11";
			value2.XV_Data = "BBB";
			value2.XV_Type = AddOnColumnDataType.Codes.String;
			value2.XV_ParentID = dummy2.PK;
			value2.XV_ParentTableCode = dummy2.TablePrefix;

			var dummy3 = Factory.New<DummyBusinessObject>();

			Factory.Save();

			var filterCollection = new ModuleFilterCollection();
			filterCollection.AddWorkflowCustomFieldsFilters(Factory, "XXX", typeof(DummyBusinessObject));
			var textFilter = (ModuleTextFilter)filterCollection["C11"];
			textFilter.Property = "AAA";

			textFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			AssertContainsExactElementsInAnyOrder(new[] { dummy2.PK, dummy3.PK }, new BusinessObjectFactory().Load<DummyBusinessObject>(textFilter.Query).Select(s => s.PK));

			textFilter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			AssertContainsExactElementsInAnyOrder(new[] { dummy2.PK, dummy3.PK }, new BusinessObjectFactory().Load<DummyBusinessObject>(textFilter.Query).Select(s => s.PK));
		}

		public void TestBigNumbersDoNotCrashTheQuery()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var value1 = Factory.New<GenCustomAddOnValue>();
			value1.XV_Name = "C12";
			value1.XV_Data = "10000000000000000000000000000000000000000000000000000";
			value1.XV_Type = AddOnColumnDataType.Codes.Integer;
			value1.XV_ParentID = dummy1.PK;
			value1.XV_ParentTableCode = dummy1.TablePrefix;

			Factory.Save();

			var filterCollection = new ModuleFilterCollection();
			filterCollection.AddWorkflowCustomFieldsFilters(Factory, "XXX", typeof(DummyBusinessObject));
			var numberRangeFilter = (ModuleNumberRangeFilter)filterCollection["C12"];
			numberRangeFilter.Property1 = 50;
			numberRangeFilter.Property2 = 150;

			AssertNoExceptionThrown(() => new BusinessObjectFactory().Load<DummyBusinessObject>(numberRangeFilter.Query));
			var result = new BusinessObjectFactory().Load<DummyBusinessObject>(numberRangeFilter.Query);
			AssertEquals(0, result.Length);
		}

		public void TestFilterByInteger()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			GenCustomAddOnValue value1 = Factory.New<GenCustomAddOnValue>();
			value1.XV_Name = "C12";
			value1.XV_Data = "100";
			value1.XV_Type = AddOnColumnDataType.Codes.Integer;
			value1.XV_ParentID = dummy1.PK;
			value1.XV_ParentTableCode = dummy1.TablePrefix;

			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			GenCustomAddOnValue value2 = Factory.New<GenCustomAddOnValue>();
			value2.XV_Name = "C12";
			value2.XV_Data = "200";
			value2.XV_Type = AddOnColumnDataType.Codes.Integer;
			value2.XV_ParentID = dummy2.PK;
			value2.XV_ParentTableCode = dummy2.TablePrefix;

			Factory.Save();

			ModuleFilterCollection filterCollection = new ModuleFilterCollection();
			filterCollection.AddWorkflowCustomFieldsFilters(Factory, "XXX", typeof(DummyBusinessObject));
			ModuleNumberRangeFilter numberRangeFilter = (ModuleNumberRangeFilter)filterCollection["C12"];
			numberRangeFilter.Property1 = 50;
			numberRangeFilter.Property2 = 150;

			DummyBusinessObject[] result = new BusinessObjectFactory().Load<DummyBusinessObject>(numberRangeFilter.Query);

			AssertEquals(1, result.Length);
			AssertEquals(dummy1.PK, result[0].PK);

			numberRangeFilter.Property1 = 150;
			numberRangeFilter.Property2 = 250;

			result = new BusinessObjectFactory().Load<DummyBusinessObject>(numberRangeFilter.Query);

			AssertEquals(1, result.Length);
			AssertEquals(dummy2.PK, result[0].PK);
		}

		public void TestFilterByInteger_DefaultProperty()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			GenCustomAddOnValue value1 = Factory.New<GenCustomAddOnValue>();
			value1.XV_Name = "C12";
			value1.XV_Data = "100";
			value1.XV_Type = AddOnColumnDataType.Codes.Integer;
			value1.XV_ParentID = dummy1.PK;
			value1.XV_ParentTableCode = dummy1.TablePrefix;

			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			// No custom field for dummy2 implies the value is default (0)
			// (or the custom field template doesn't apply to this job but this is the best we can do with module filters when default values are not saved to the DB)

			Factory.Save();

			ModuleFilterCollection filterCollection = new ModuleFilterCollection();
			filterCollection.AddWorkflowCustomFieldsFilters(Factory, "XXX", typeof(DummyBusinessObject));
			ModuleNumberRangeFilter numberRangeFilter = (ModuleNumberRangeFilter)filterCollection["C12"];
			numberRangeFilter.Property1 = 0;
			numberRangeFilter.Property2 = 0;

			DummyBusinessObject[] result = new BusinessObjectFactory().Load<DummyBusinessObject>(numberRangeFilter.Query);

			AssertEquals(1, result.Length);
			AssertEquals(dummy2.PK, result[0].PK);

			numberRangeFilter.Property1 = -50;
			numberRangeFilter.Property2 = 100;

			result = new BusinessObjectFactory().Load<DummyBusinessObject>(numberRangeFilter.Query);

			AssertEquals(2, result.Length);
		}

		public void TestFilterByDecimal()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var value1 = Factory.New<GenCustomAddOnValue>();
			value1.XV_Name = "C14";
			value1.XV_Data = "100000000.03";
			value1.XV_Type = AddOnColumnDataType.Codes.Decimal;
			value1.XV_ParentID = dummy1.PK;
			value1.XV_ParentTableCode = dummy1.TablePrefix;

			var dummy2 = Factory.New<DummyBusinessObject>();
			var value2 = Factory.New<GenCustomAddOnValue>();
			value2.XV_Name = "C14";
			value2.XV_Data = "100000000.01";
			value2.XV_Type = AddOnColumnDataType.Codes.Decimal;
			value2.XV_ParentID = dummy2.PK;
			value2.XV_ParentTableCode = dummy2.TablePrefix;

			Factory.Save();

			var filterCollection = new ModuleFilterCollection();
			filterCollection.AddWorkflowCustomFieldsFilters(Factory, "XXX", typeof(DummyBusinessObject));
			var numberRangeFilter = (ModuleNumberRangeFilter)filterCollection["C14"];
			numberRangeFilter.Property1 = 100000000.02;
			numberRangeFilter.Property2 = 100000000.04;

			var result = new BusinessObjectFactory().Load<DummyBusinessObject>(numberRangeFilter.Query);

			AssertEquals(1, result.Length);
			AssertEquals(dummy1.PK, result[0].PK);

			numberRangeFilter.Property1 = 0.1;
			numberRangeFilter.Property2 = 100000000.01;

			result = new BusinessObjectFactory().Load<DummyBusinessObject>(numberRangeFilter.Query);

			AssertEquals(1, result.Length);
			AssertEquals(dummy2.PK, result[0].PK);
		}

		public void TestFilterByDecimal_DefaultValue()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var value1 = Factory.New<GenCustomAddOnValue>();
			value1.XV_Name = "C14";
			value1.XV_Data = "100000000.03";
			value1.XV_Type = AddOnColumnDataType.Codes.Decimal;
			value1.XV_ParentID = dummy1.PK;
			value1.XV_ParentTableCode = dummy1.TablePrefix;

			var dummy2 = Factory.New<DummyBusinessObject>();
			// No custom field for dummy2 implies the value is default (0)
			// (or the custom field template doesn't apply to this job but this is the best we can do with module filters when default values are not saved to the DB)

			Factory.Save();

			var filterCollection = new ModuleFilterCollection();
			filterCollection.AddWorkflowCustomFieldsFilters(Factory, "XXX", typeof(DummyBusinessObject));
			var numberRangeFilter = (ModuleNumberRangeFilter)filterCollection["C14"];
			numberRangeFilter.Property1 = 0;
			numberRangeFilter.Property2 = 0;

			var result = new BusinessObjectFactory().Load<DummyBusinessObject>(numberRangeFilter.Query);

			AssertEquals(1, result.Length);
			AssertEquals(dummy2.PK, result[0].PK);

			numberRangeFilter.Property1 = -1;
			numberRangeFilter.Property2 = 100000000.03;

			result = new BusinessObjectFactory().Load<DummyBusinessObject>(numberRangeFilter.Query);
			AssertEquals(2, result.Length);
		}

		public void TestFilterByDate()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			GenCustomAddOnValue value1 = Factory.New<GenCustomAddOnValue>();
			value1.XV_Name = "C21";
			value1.XV_Data = ZDateTime.Today.SqlFormat;
			value1.XV_Type = AddOnColumnDataType.Codes.Datetime;
			value1.XV_ParentID = dummy1.PK;
			value1.XV_ParentTableCode = dummy1.TablePrefix;

			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			GenCustomAddOnValue value2 = Factory.New<GenCustomAddOnValue>();
			value2.XV_Name = "C21";
			value2.XV_Data = ZDateTime.Today.AddDays(2).SqlFormat;
			value2.XV_Type = AddOnColumnDataType.Codes.Datetime;
			value2.XV_ParentID = dummy2.PK;
			value2.XV_ParentTableCode = dummy2.TablePrefix;

			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();
			GenCustomAddOnValue value3 = Factory.New<GenCustomAddOnValue>();
			value3.XV_Name = "C21";
			value3.XV_Data = ZDateTime.Today.AddDays(7).SqlFormat;
			value3.XV_Type = AddOnColumnDataType.Codes.Datetime;
			value3.XV_ParentID = dummy3.PK;
			value3.XV_ParentTableCode = dummy3.TablePrefix;

			Factory.Save();

			ModuleFilterCollection filterCollection = new ModuleFilterCollection();
			filterCollection.AddWorkflowCustomFieldsFilters(Factory, "XXX", typeof(DummyBusinessObject));
			ModuleDateFilter dateFilter = (ModuleDateFilter)filterCollection["C21"];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = ZDateTime.Today.AddDays(-1);
			dateFilter.Property2 = ZDateTime.Today.AddDays(1);

			DummyBusinessObject[] result = new BusinessObjectFactory().Load<DummyBusinessObject>(dateFilter.Query);

			AssertEquals(1, result.Length);
			AssertEquals(dummy1.PK, result[0].PK);

			dateFilter.Property1 = ZDateTime.Today.AddDays(2);
			dateFilter.Property2 = ZDateTime.Today.AddDays(6);

			result = new BusinessObjectFactory().Load<DummyBusinessObject>(dateFilter.Query);

			AssertEquals(1, result.Length);
			AssertEquals(dummy2.PK, result[0].PK);
		}

		public void TestFlagFilterByBoolean()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			GenCustomAddOnValue value1 = Factory.New<GenCustomAddOnValue>();
			value1.XV_Name = "C22";
			value1.XV_Data = "Y";
			value1.XV_Type = AddOnColumnDataType.Codes.Boolean;
			value1.XV_ParentID = dummy1.PK;
			value1.XV_ParentTableCode = dummy1.TablePrefix;

			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			GenCustomAddOnValue value2 = Factory.New<GenCustomAddOnValue>();
			value2.XV_Name = "C22";
			value2.XV_Data = "N";
			value2.XV_Type = AddOnColumnDataType.Codes.Boolean;
			value2.XV_ParentID = dummy2.PK;
			value2.XV_ParentTableCode = dummy2.TablePrefix;

			Factory.Save();

			ModuleFilterCollection filterCollection = new ModuleFilterCollection();
			filterCollection.AddWorkflowCustomFieldsFilters(Factory, "XXX", typeof(DummyBusinessObject));

			ModuleFlagsFilter legacyFlagsFilter = (ModuleFlagsFilter)filterCollection["Workflow Flags"];
			legacyFlagsFilter["C22"] = true;

			var result = new BusinessObjectFactory().Load<DummyBusinessObject>(legacyFlagsFilter.Query);

			AssertEquals(1, result.Length);
			AssertEquals(dummy1.PK, result[0].PK);

			legacyFlagsFilter["C22"] = false;
			result = new BusinessObjectFactory().Load<DummyBusinessObject>(legacyFlagsFilter.Query);

			AssertEquals(1, result.Length);
			AssertEquals(dummy2.PK, result[0].PK);
		}

		public void TestFlagSetFilterByBoolean()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			GenCustomAddOnValue value1 = Factory.New<GenCustomAddOnValue>();
			value1.XV_Name = "C22";
			value1.XV_Data = "Y";
			value1.XV_Type = AddOnColumnDataType.Codes.Boolean;
			value1.XV_ParentID = dummy1.PK;
			value1.XV_ParentTableCode = dummy1.TablePrefix;

			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			GenCustomAddOnValue value2 = Factory.New<GenCustomAddOnValue>();
			value2.XV_Name = "C22";
			value2.XV_Data = "N";
			value2.XV_Type = AddOnColumnDataType.Codes.Boolean;
			value2.XV_ParentID = dummy2.PK;
			value2.XV_ParentTableCode = dummy2.TablePrefix;

			Factory.Save();

			ModuleFilterCollection filterCollection = new ModuleFilterCollection();
			filterCollection.AddWorkflowCustomFieldsFilters(Factory, "XXX", typeof(DummyBusinessObject));

			ModuleTextFilter flagsFilter = (ModuleTextFilter)filterCollection["C22"];
			flagsFilter.Property = "True";

			DummyBusinessObject[] result = new BusinessObjectFactory().Load<DummyBusinessObject>(flagsFilter.Query);

			AssertEquals(1, result.Length);
			AssertEquals(dummy1.PK, result[0].PK);

			flagsFilter.Property = "False";
			result = new BusinessObjectFactory().Load<DummyBusinessObject>(flagsFilter.Query);
			AssertEquals(1, result.Length);
			AssertEquals(dummy2.PK, result[0].PK);
		}

		public void TestManyBooleanDropdownForFlagFilter()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "BOO";

			var booleanNames = Enumerable.Range(1, 15).Select(x => $"B{x}");
			foreach (var name in booleanNames)
			{
				GenCustomColumnDefinition def11 = template.GenCustomColumnDefinitions.AddNew();
				def11.XC_Name = name;
				def11.XC_Type = AddOnColumnDataType.Codes.Boolean;
			}

			Factory.Save();

			ModuleFilterCollection filterCollection = new ModuleFilterCollection();
			filterCollection.AddWorkflowCustomFieldsFilters(Factory, "BOO", typeof(DummyBusinessObject));

			AssertNull(filterCollection["Workflow Flags"]);
			var firstGroup = filterCollection["Workflow Flags 1"];
			AssertNotNull(firstGroup);
			AssertNull(firstGroup.MultilingualDescription);
			AssertEquals(typeof(ModuleFlagsFilter), firstGroup.GetType());
			AssertEquals(10, ((ModuleFlagsFilter)firstGroup).FlagNames.Length);

			var secondGroup = filterCollection["Workflow Flags 2"];
			AssertNotNull(secondGroup);
			AssertNull(secondGroup.MultilingualDescription);
			AssertEquals(typeof(ModuleFlagsFilter), secondGroup.GetType());
			AssertEquals(5, ((ModuleFlagsFilter)secondGroup).FlagNames.Length);
		}

		public void TestHandlingEmptyXC_NameAndXC_Type()
		{
			using (new DisposableAction(ErrorReporter.Clear))
			{
				ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				template.P0_ProcessType = "BOO";
				GenCustomColumnDefinition emptyDef = template.GenCustomColumnDefinitions.AddNew();
				emptyDef.XC_Name = "";
				emptyDef.XC_Type = "";

				GenCustomColumnDefinition notEmptyDef = template.GenCustomColumnDefinitions.AddNew();
				emptyDef.XC_Name = "test";
				emptyDef.XC_Type = AddOnColumnDataType.Codes.String;

				Factory.Save();

				ModuleFilterCollection filterCollection = new ModuleFilterCollection();
				AssertNoExceptionThrown(() => filterCollection.AddWorkflowCustomFieldsFilters(Factory, "BOO", typeof(DummyBusinessObject)));
				AssertEquals("Should be one filter added", 1, filterCollection.Count());
				AssertNotNull("Error Report exists for invalid values", ErrorReporter.LastExceptionReported);
				AssertEquals("LastExceptionReported.Message", "Unable to create Custom Fields Filter due to invalid values.", ErrorReporter.LastMessageReported);
			}
		}

		public void TestManyBooleanForFlagSet()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "BOO";

			var booleanNames = Enumerable.Range(1, 15).Select(x => $"B{x}");
			foreach (var name in booleanNames)
			{
				GenCustomColumnDefinition def11 = template.GenCustomColumnDefinitions.AddNew();
				def11.XC_Name = name;
				def11.XC_Type = AddOnColumnDataType.Codes.Boolean;
			}

			Factory.Save();

			ModuleFilterCollection filterCollection = new ModuleFilterCollection();
			filterCollection.AddWorkflowCustomFieldsFilters(Factory, "BOO", typeof(DummyBusinessObject));

			foreach (var name in booleanNames)
			{
				AssertNotNull(filterCollection[name]);
				AssertEquals(typeof(ModuleTextFilter), filterCollection[name].GetType());

				var filter = ((ModuleTextFilter)filterCollection[name]);
				AssertEquals(2, filter.List.Count);

				var options = (CodeDescriptionPairList)filter.List;
				var trueCodeDescPair = options[0];
				AssertEquals("True", trueCodeDescPair.Code);
				AssertEquals("True", trueCodeDescPair.Description);

				var falseCodeDescPair = options[1];
				AssertEquals("False", falseCodeDescPair.Code);
				AssertEquals("False", falseCodeDescPair.Description);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			PrepareTemplates();
		}

		void PrepareTemplates()
		{
			ProcessTaskTemplate template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "XXX";

			GenCustomColumnDefinition def11 = template1.GenCustomColumnDefinitions.AddNew();
			def11.XC_Name = "C11";
			def11.XC_Type = AddOnColumnDataType.Codes.String;

			GenCustomColumnDefinition def12 = template1.GenCustomColumnDefinitions.AddNew();
			def12.XC_Name = "C12";
			def12.XC_Type = AddOnColumnDataType.Codes.Integer;

			GenCustomColumnDefinition def13 = template1.GenCustomColumnDefinitions.AddNew();
			def13.XC_Name = "C13";
			def13.XC_Type = AddOnColumnDataType.Codes.ComboBox;

			GenCustomColumnDefinition def14 = template1.GenCustomColumnDefinitions.AddNew();
			def14.XC_Name = "C14";
			def14.XC_Type = AddOnColumnDataType.Codes.Decimal;

			ProcessTaskTemplate template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "XXX";

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

			var dupWithDiffType1 = template1.GenCustomColumnDefinitions.AddNew();
			dupWithDiffType1.XC_Name = "Bung";
			dupWithDiffType1.XC_Type = AddOnColumnDataType.Codes.String;

			var dupWithDiffType2 = template2.GenCustomColumnDefinitions.AddNew();
			dupWithDiffType2.XC_Name = "Bung";
			dupWithDiffType2.XC_Type = AddOnColumnDataType.Codes.Datetime;

			var dupWithDiffType3 = template2.GenCustomColumnDefinitions.AddNew();
			dupWithDiffType3.XC_Name = "bung";
			dupWithDiffType3.XC_Type = AddOnColumnDataType.Codes.Integer;

			Factory.Save();
		}

		#endregion
	}
}
