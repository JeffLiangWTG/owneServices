using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class FormCustomisationSettingsTest : TestCaseWithFactory
	{
		public void TestReset()
		{
			AssertEquals(2, TaskTemplate.FormCustomisationSettings.DisplayTabs.Count);
			AssertEquals(3, TaskTemplate.FormCustomisationSettings.DisplayFields.Count);

			FormSettingsProvider.GetDisplayTabsImplementation = () =>
			{
				FormCustomisableElementCollection tabs = new FormCustomisableElementCollection();
				tabs.Add((NoResString)"new tab desc", "new tab name");
				return tabs;
			};

			FormSettingsProvider.GetDisplayFieldsImplementation = () =>
			{
				FormCustomisableElementCollection fields = new FormCustomisableElementCollection();
				fields.Add((NoResString)"new field 1 desc", "new field 1 name");
				fields.Add((NoResString)"new field 2 desc", "new field 2 name");
				return fields;
			};

			bool hasChangesFired = false;
			TaskTemplate.HasChangesChanged += (s, e) => hasChangesFired = true;

			TaskTemplate.FormCustomisationSettings.Reset();

			Assert("HasChangesChanged should fire when adding or removing elements on reset", hasChangesFired);
			AssertEquals(1, TaskTemplate.FormCustomisationSettings.DisplayTabs.Count);
			AssertEquals(2, TaskTemplate.FormCustomisationSettings.DisplayFields.Count);
			AssertEquals("new tab name", TaskTemplate.FormCustomisationSettings.DisplayTabs[0].ElementName);
			AssertEquals("new field 1 name", TaskTemplate.FormCustomisationSettings.DisplayFields[0].ElementName);
			AssertEquals("new field 2 name", TaskTemplate.FormCustomisationSettings.DisplayFields[1].ElementName);
		}

		public void TestRefresh()
		{
			FormSettingsProvider.GetPropertiesThatAffectWorkflowTemplateImplementation = () => new[] { ProcessTaskTemplate.Schema.P0_SubType1 };
			DummyWorkflowDescriptor.Instance.GetFormCustomisationSettingsProviderImplementation = () => FormSettingsProvider;

			AssertNotNull("prerequisite", TaskTemplate.WorkflowDescriptor);
			AssertEquals("prerequisite", DummyWorkflowDescriptor.Instance, TaskTemplate.WorkflowDescriptor);
			AssertEquals("prerequisite", FormSettingsProvider, TaskTemplate.WorkflowDescriptor.FormCustomisationSettings);
			AssertEquals("prerequisite", 2, TaskTemplate.FormCustomisationSettings.DisplayTabs.Count);
			AssertEquals("prerequisite", 3, TaskTemplate.FormCustomisationSettings.DisplayFields.Count);

			FormCustomisableElement[] preRefreshTabs = TaskTemplate.FormCustomisationSettings.DisplayTabs.ToArray<FormCustomisableElement>();
			FormCustomisableElement[] preRefreshFields = TaskTemplate.FormCustomisationSettings.DisplayFields.ToArray<FormCustomisableElement>();

			TaskTemplate.P0_SubType1 = "AAA";

			AssertContainsExactElementsInAnyOrder(preRefreshTabs, TaskTemplate.FormCustomisationSettings.DisplayTabs);
			AssertContainsExactElementsInAnyOrder(preRefreshFields, TaskTemplate.FormCustomisationSettings.DisplayFields);

			FormSettingsProvider.GetDisplayTabsImplementation = () =>
			{
				FormCustomisableElementCollection tabs = new FormCustomisableElementCollection();
				Tabs[0].IsAvailableFunction = () => false;
				tabs.Add(Tabs[0]);
				return tabs;
			};

			FormSettingsProvider.GetDisplayFieldsImplementation = () =>
			{
				FormCustomisableElementCollection fields = new FormCustomisableElementCollection();
				Fields[0].IsAvailableFunction = () => false;
				fields.Add(Fields[0]);
				fields.Add(Fields[1]);
				return fields;
			};

			TaskTemplate.P0_SubType1 = "BBB";

			AssertEquals(false, TaskTemplate.FormCustomisationSettings.DisplayTabs[0].IsAvailable);
			AssertEquals(true, TaskTemplate.FormCustomisationSettings.DisplayTabs[1].IsAvailable);

			AssertEquals(true, TaskTemplate.FormCustomisationSettings.DisplayTabs[0].IsInTemplate);
			AssertEquals(false, TaskTemplate.FormCustomisationSettings.DisplayTabs[1].IsInTemplate);

			AssertEquals(0, TaskTemplate.FormCustomisationSettings.DisplayTabsView.Count);

			AssertEquals(false, TaskTemplate.FormCustomisationSettings.DisplayFields[0].IsAvailable);
			AssertEquals(true, TaskTemplate.FormCustomisationSettings.DisplayFields[1].IsAvailable);
			AssertEquals(true, TaskTemplate.FormCustomisationSettings.DisplayFields[2].IsAvailable);

			AssertEquals(true, TaskTemplate.FormCustomisationSettings.DisplayFields[0].IsInTemplate);
			AssertEquals(true, TaskTemplate.FormCustomisationSettings.DisplayFields[1].IsInTemplate);
			AssertEquals(false, TaskTemplate.FormCustomisationSettings.DisplayFields[2].IsInTemplate);

			AssertContainsExactElementsInAnyOrder(new[] { TaskTemplate.FormCustomisationSettings.DisplayFields[1] }, TaskTemplate.FormCustomisationSettings.DisplayFieldsView);
		}

		public void TestHookProcessTaskTemplatePropertiesForRefresh()
		{
			FormSettingsProvider.GetDisplayTabsImplementation = () => { return new FormCustomisableElementCollection(); };
			TaskTemplate.FormCustomisationSettings.Reset();
			AssertEquals(0, TaskTemplate.FormCustomisationSettings.DisplayTabs.Count);

			FormSettingsProvider.GetDisplayTabsImplementation = () =>
			{
				FormCustomisableElementCollection tabs = new FormCustomisableElementCollection();
				tabs.Add((NoResString)"tab1", "tab1", true);
				return tabs;
			};

			TaskTemplate.P0_SubType1 = "XXX";

			AssertEquals(1, TaskTemplate.FormCustomisationSettings.DisplayTabs.Count);
			AssertEquals("tab1", TaskTemplate.FormCustomisationSettings.DisplayTabs[0].ElementName);

			FormSettingsProvider.GetDisplayTabsImplementation = () =>
			{
				FormCustomisableElementCollection tabs = new FormCustomisableElementCollection();
				tabs.Add((NoResString)"tab2", "tab2", true);
				tabs.Add((NoResString)"tab3", "tab3", true);
				return tabs;
			};

			TaskTemplate.P0_SubType1 = "YYY";

			AssertEquals(3, TaskTemplate.FormCustomisationSettings.DisplayTabs.Count);
			AssertEquals("tab1", TaskTemplate.FormCustomisationSettings.DisplayTabs[0].ElementName);
			AssertEquals("tab2", TaskTemplate.FormCustomisationSettings.DisplayTabs[1].ElementName);
			AssertEquals("tab3", TaskTemplate.FormCustomisationSettings.DisplayTabs[2].ElementName);

			TaskTemplate.FormCustomisationSettings.Reset();

			AssertEquals(2, TaskTemplate.FormCustomisationSettings.DisplayTabs.Count);
			AssertEquals("tab2", TaskTemplate.FormCustomisationSettings.DisplayTabs[0].ElementName);
			AssertEquals("tab3", TaskTemplate.FormCustomisationSettings.DisplayTabs[1].ElementName);
		}

		public void TestIsElementVisible()
		{
			ProcessTaskTemplate taskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			FormCustomisableElement elementTab = taskTemplate.FormCustomisationSettings.DisplayTabs.Add((NoResString)"Zasha", "Name");
			FormCustomisableElement elementField = taskTemplate.FormCustomisationSettings.DisplayFields.Add((NoResString)"Zasha", "Name");

			elementTab.Visible = false;
			elementField.Visible = false;
			AssertEquals(false, ((IFormCustomisationSettings)taskTemplate.FormCustomisationSettings).IsElementVisible("Name", ElementType.Field));
			AssertEquals(false, ((IFormCustomisationSettings)taskTemplate.FormCustomisationSettings).IsElementVisible("Name", ElementType.Tab));

			elementField.Visible = true;
			AssertEquals(true, ((IFormCustomisationSettings)taskTemplate.FormCustomisationSettings).IsElementVisible("Name", ElementType.Field));
			AssertEquals(false, ((IFormCustomisationSettings)taskTemplate.FormCustomisationSettings).IsElementVisible("Name", ElementType.Tab));

			AssertNull("indecisive result for element that is not on tabs and fields list",
				((IFormCustomisationSettings)taskTemplate.FormCustomisationSettings).IsElementVisible("SomeElementNotInAnyList", ElementType.Field));

			AssertNull("indecisive result for element that is not on tabs and fields list",
				((IFormCustomisationSettings)taskTemplate.FormCustomisationSettings).IsElementVisible("SomeElementNotInAnyList", ElementType.Tab));
		}

		public void TestGetTabPlacement()
		{
			ProcessTaskTemplate taskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			FormCustomisableElement element = taskTemplate.FormCustomisationSettings.DisplayFields.Add((NoResString)"Zasha", "Name", true, (NoResString)"Group1", "Tab1", "Nowhere", 4);
			TabPlacement placement = ((IFormCustomisationSettings)taskTemplate.FormCustomisationSettings).GetTabPlacement("Name");
			AssertEquals("Tab1", placement.TabPageName);
			AssertEquals("Nowhere", placement.Placement);
			AssertEquals(4, placement.RowNumber);
		}

		public void TestHasChanges()
		{
			ProcessTaskTemplate taskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			taskTemplate.P0_ProcessType = "SHP";
			Factory.Save();

			FormCustomisableElementCollection elements = taskTemplate.FormCustomisationSettings.DisplayFields;
			AssertEquals(false, taskTemplate.HasChanges);
			AssertEquals(false, taskTemplate.FormCustomisationSettings.DisplayFields.HasChanges);
		}

		public void TestGroupMultilingualStringBugFixedOnReload()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			Factory.Save();

			var element = template.FormCustomisationSettings.DisplayFields.Where(x => !x.ElementGroup.IsEmpty).FirstOrDefault();
			AssertNotNull("Predondition: Need 1 element with non empty ElementGroup", element);
			var elementName = element.ElementName;
			var nonTranslatedElementGroup = element.ElementGroup;
			var nonTranslatedElementDescription = element.ElementDescription;
			element.ElementGroup = "SOME OTHER LANGUAGE"; //a bug caused the translated valued to be saved
			element.ElementDescription = "SOME OTHER LANGUAGE";
			AssertEquals(true, element.Visible);
			AssertEquals(0, element.RowNumber);
			element.Visible = false;
			element.RowNumber = 1;
			Factory.Save();

			var reloadedTemplate = new BusinessObjectFactory().Load<ProcessTaskTemplate>(template.PK);
			element = reloadedTemplate.FormCustomisationSettings.DisplayFields.Where(x => x.ElementName == elementName).First();
			AssertEquals(@"Element Group is readonly and cannot be changed, take the value from the incode config, since a bug caused the multilingual string to be saved.
We don't need to save ElementGroup in the DB", nonTranslatedElementGroup, element.ElementGroup);
			AssertEquals(nonTranslatedElementDescription, element.ElementDescription);

			CombineAssertions("Configurable values should not be overridden", () =>
			{
				AssertEquals(false, element.Visible);
				AssertEquals(1, element.RowNumber);
			});
		}

		public void TestTabsDoNotHaveChangesOnSetup()
		{
			FormCustomisableElement element = new FormCustomisableElement { ElementName = "NAME", ElementDescription = "DESCRIPTION" };
			FormCustomisationSettingsStorageTab storageTab = new FormCustomisationSettingsStorageTab { Name = "NAME", Description = "DESCRIPTION" };

			var tabs = new FormCustomisableElementCollection();
			tabs.Add(element);

			var formSettingsProvider = new FormCustomisationSettingsProviderForTest();
			formSettingsProvider.GetDisplayTabsImplementation = () => tabs;
			DummyWorkflowDescriptor.Instance.GetFormCustomisationSettingsProviderImplementation = () => formSettingsProvider;
			DummyWorkflowDescriptor.Instance.ClearFormCustomisationSettingsCacheForTest();

			var taskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			taskTemplate.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			Factory.Save();

			var customisationSetting = new FormCustomisationSettings(taskTemplate);
			var tabElement = (FormCustomisableElement)customisationSetting.DisplayTabs.Single();
			tabElement.Hydrate(storageTab);
			using (tabElement.SuspendSettingHasChanges())
			{
				tabElement.IsAvailableFunction = () => !tabElement.IsAvailable;
				tabElement.ElementDescription = tabElement.ElementDescription + "n";
			}
			customisationSetting.UpdateFieldsFrom(taskTemplate.FormCustomisationSettings);
			Assert(!tabElement.HasChanges);
		}

		public void TestFieldsDoNotHaveChangesOnSetup()
		{
			FormCustomisableElement element = new FormCustomisableElement { ElementName = "NAME", ElementDescription = "DESCRIPTION" };
			FormCustomisationSettingsStorageField storageField = new FormCustomisationSettingsStorageField { Name = "NAME", Description = "DESCRIPTION" };

			var fields = new FormCustomisableElementCollection();
			fields.Add(element);

			var formSettingsProvider = new FormCustomisationSettingsProviderForTest();
			formSettingsProvider.GetDisplayFieldsImplementation = () => fields;
			DummyWorkflowDescriptor.Instance.GetFormCustomisationSettingsProviderImplementation = () => formSettingsProvider;
			DummyWorkflowDescriptor.Instance.ClearFormCustomisationSettingsCacheForTest();

			var taskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			taskTemplate.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			Factory.Save();

			var customisationSetting = new FormCustomisationSettings(taskTemplate);
			var fieldElement = (FormCustomisableElement)customisationSetting.DisplayFields.Single();
			AssertEquals(element.ElementName, fieldElement.ElementName);

			fieldElement.Hydrate(storageField);
			using (fieldElement.SuspendSettingHasChanges())
			{
				fieldElement.IsAvailableFunction = () => !fieldElement.IsAvailable;
			}
			customisationSetting.UpdateFieldsFrom(taskTemplate.FormCustomisationSettings);
			Assert(!fieldElement.HasChanges);
		}

		public void TestFieldValidationOnInit()
		{
			var fields = new FormCustomisableElementCollection
			{
				{ (NoResString)"Test Element No Group", "TestNoGroup", false, (NoResString)"", "TabPage", TabPlacement.Placements.TopRight, 1 },
				{ (NoResString)"Test Element 1", "Test1", false, (NoResString)"General", "TabPage", TabPlacement.Placements.TopRight, 2 }
			};
			var element2 = fields.Add((NoResString)"Test Element 2", "Test2", false, (NoResString)"General", "TabPage", TabPlacement.Placements.TopRight, 2);
			element2.IsAvailableFunction = () => false;

			var formSettingsProvider = new FormCustomisationSettingsProviderForTest();
			formSettingsProvider.GetDisplayFieldsImplementation = () => fields;
			DummyWorkflowDescriptor.Instance.GetFormCustomisationSettingsProviderImplementation = () => formSettingsProvider;
			DummyWorkflowDescriptor.Instance.ClearFormCustomisationSettingsCacheForTest();

			var taskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			taskTemplate.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

			Factory.Save();

			var templateReload = new BusinessObjectFactory().Load<ProcessTaskTemplate>(taskTemplate.PK);
			foreach (var element in templateReload.FormCustomisationSettings.DisplayFields)
			{
				AssertNoErrors(((FormCustomisableElement)element).RowNumberInfo);
			}
		}

		#region Xml Serialization

		public void TestSerializeDeserialize()
		{
			ProcessTaskTemplate taskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			taskTemplate.FormCustomisationSettings.DisplayTabs.Add((NoResString)"Tab 1 Desc", "Tab1");
			taskTemplate.FormCustomisationSettings.DisplayTabs.Add((NoResString)"Tab 2 Desc", "Tab2").Visible = false;
			taskTemplate.FormCustomisationSettings.DisplayTabs.Add((NoResString)"Tab 3 Desc", "Tab3").CanContainOtherElements = true;

			taskTemplate.FormCustomisationSettings.DisplayFields.Add((NoResString)"Field 1 Desc", "Field1", false, (NoResString)"", "Tab1", "X", 4);
			taskTemplate.FormCustomisationSettings.DisplayFields.Add((NoResString)"Field 2 Desc", "Field2", false, (NoResString)"Group1", "Tab2", "Y", 0);

			Factory.Save();

			ProcessTaskTemplate reloadedTemplate = (new BusinessObjectFactory()).Load<ProcessTaskTemplate>(taskTemplate.PK);

			AssertNotNull(reloadedTemplate.P0_FormState);

			AssertEquals(3, reloadedTemplate.FormCustomisationSettings.DisplayTabs.Count);
			AssertElementSetting(reloadedTemplate.FormCustomisationSettings.DisplayTabs[0], "Tab 1 Desc", "Tab1", true, false, "", "", 0);
			AssertElementSetting(reloadedTemplate.FormCustomisationSettings.DisplayTabs[1], "Tab 2 Desc", "Tab2", false, false, "", "", 0);
			AssertElementSetting(reloadedTemplate.FormCustomisationSettings.DisplayTabs[2], "Tab 3 Desc", "Tab3", true, true, "", "", 0);

			AssertEquals(2, reloadedTemplate.FormCustomisationSettings.DisplayFields.Count);
			AssertElementSetting(reloadedTemplate.FormCustomisationSettings.DisplayFields[0], "Field 1 Desc", "Field1", true, false, "Tab 1 Desc", "X", 4);
			AssertElementSetting(reloadedTemplate.FormCustomisationSettings.DisplayFields[1], "Field 2 Desc", "Field2", true, false, "Tab 2 Desc", "Y", 0);
		}

		void AssertElementSetting(FormCustomisableElement element, string expectedDesc, string expectedName, bool expectedVisible, bool expectedCanContainerElements, string expectedTab, string expectedPlacement, int expectedRow)
		{
			AssertEquals("ElementDescription", expectedDesc, element.ElementDescription);
			AssertEquals("ElementName", expectedName, element.ElementName);
			AssertEquals("Visible", expectedVisible, element.Visible);
			AssertEquals("there's no mapping in xml for CanContainOtherElements so it's always false", false, element.CanContainOtherElements);
			AssertEquals("DisplayTab", expectedTab, element.DisplayTab);
			AssertEquals("Placement", expectedPlacement, element.Placement);
			AssertEquals("RowNumber", expectedRow, element.RowNumber);
		}

		#endregion

		public void TestUpdateFields()
		{
			AssertEquals("prerequisite", 2, TaskTemplate.FormCustomisationSettings.DisplayTabs.Count);
			AssertEquals("prerequisite", 3, TaskTemplate.FormCustomisationSettings.DisplayFields.Count);

			TaskTemplate.FormCustomisationSettings.DisplayTabs.HasChanges = true;

			Factory.Save();

			FormSettingsProvider.GetDisplayTabsImplementation = () =>
			{
				FormCustomisableElementCollection tabs = new FormCustomisableElementCollection();
				tabs.Add(Tabs[0]);
				return tabs;
			};

			FormSettingsProvider.GetDisplayFieldsImplementation = () =>
			{
				FormCustomisableElementCollection fields = new FormCustomisableElementCollection();
				fields.Add(Fields[0]);
				fields.Add(Fields[1]);
				return fields;
			};

			ProcessTaskTemplate reloadedTemplate1 = (new BusinessObjectFactory()).Load<ProcessTaskTemplate>(TaskTemplate.PK);

			AssertReloadedTemplate(reloadedTemplate1);

			reloadedTemplate1.FormCustomisationSettings.Reset();

			AssertEquals(1, reloadedTemplate1.FormCustomisationSettings.DisplayTabs.Count);
			AssertEquals(2, reloadedTemplate1.FormCustomisationSettings.DisplayFields.Count);

			Assert("tabs that are not in template are removed when settings are reset",
				reloadedTemplate1.FormCustomisationSettings.DisplayTabs.Cast<FormCustomisableElement>().All(elem => elem.IsInTemplate));

			Assert("fields that are not in template are removed when settings are reset",
				reloadedTemplate1.FormCustomisationSettings.DisplayFields.Cast<FormCustomisableElement>().All(elem => elem.IsInTemplate));

			ProcessTaskTemplate reloadedTemplate2 = (new BusinessObjectFactory()).Load<ProcessTaskTemplate>(TaskTemplate.PK);

			AssertReloadedTemplate(reloadedTemplate2);
		}

		void AssertReloadedTemplate(ProcessTaskTemplate template)
		{
			AssertNotNull("reloaded template would have some data in form state", template.P0_FormState);
			AssertEquals("this method should be called by templates that are reloaded from database", true, template.IsInDatabase && !template.HasChanges);
			AssertEquals("tabs that are not in template are not removed when reloaded", 2, template.FormCustomisationSettings.DisplayTabs.Count);
			AssertEquals("fields that are not in template are not removed when reloaded", 3, template.FormCustomisationSettings.DisplayFields.Count);

			AssertEquals(true, template.FormCustomisationSettings.DisplayTabs[0].IsInTemplate);
			AssertEquals(false, template.FormCustomisationSettings.DisplayTabs[1].IsInTemplate);

			AssertEquals(true, template.FormCustomisationSettings.DisplayFields[0].IsInTemplate);
			AssertEquals(true, template.FormCustomisationSettings.DisplayFields[1].IsInTemplate);
			AssertEquals(false, template.FormCustomisationSettings.DisplayFields[2].IsInTemplate);
		}

		public void TestUpdateFieldDoesNotOverridePersistedProperties()
		{
			AssertEquals(2, TaskTemplate.FormCustomisationSettings.DisplayTabs.Count);
			AssertEquals(3, TaskTemplate.FormCustomisationSettings.DisplayFields.Count);

			AssertEquals("prerequisite", true, TaskTemplate.FormCustomisationSettings.DisplayTabs[0].Visible);
			AssertEquals("prerequisite", ZString.Empty, TaskTemplate.FormCustomisationSettings.DisplayFields[0].Placement);
			AssertEquals("prerequisite", 0, TaskTemplate.FormCustomisationSettings.DisplayFields[1].RowNumber);
			AssertEquals("prerequisite", ZString.Empty, TaskTemplate.FormCustomisationSettings.DisplayFields[2].ElementGroup);

			TaskTemplate.FormCustomisationSettings.DisplayTabs[0].Visible = false;
			TaskTemplate.FormCustomisationSettings.DisplayFields[0].Placement = "TopLeft";
			TaskTemplate.FormCustomisationSettings.DisplayFields[1].RowNumber = 3;
			TaskTemplate.FormCustomisationSettings.DisplayFields[2].ElementGroup = "Group3";

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			ProcessTaskTemplate reloadedTaskTemplate = newFactory.Load<ProcessTaskTemplate>(TaskTemplate.PK);

			AssertEquals(false, reloadedTaskTemplate.FormCustomisationSettings.DisplayTabs[0].Visible);
			AssertEquals("TopLeft", reloadedTaskTemplate.FormCustomisationSettings.DisplayFields[0].Placement);

			///
			/// ElementGroup is readonly and never changes, what ever is in the DB should match the default configuration in the code, if not we don't know what GUI control to put these fields
			/// There was a bug causing the translated ElementGroup to be saved. Now element group is always taken from the in code configuration
			/// In this case ElementGroup is empty which causes RowNumber = 0 and readonly
			///
			//AssertEquals(3, reloadedTaskTemplate.FormCustomisationSettings.DisplayFields[1].RowNumber);
			//AssertEquals("Group3", reloadedTaskTemplate.FormCustomisationSettings.DisplayFields[2].ElementGroup);
		}

		public void TestCorruptedTemplateIsReset()
		{
			var taskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			taskTemplate.P0_FormState = new ZBlob(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 });

			taskTemplate.Factory.Save();

			AssertNoExceptionThrown(() => { var settings = taskTemplate.FormCustomisationSettings; });
			AssertEquals(ZBlob.Empty, taskTemplate.P0_FormState);
		}

		public void TestAccessDeletedTemplateWontBlowup()
		{
			var taskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			taskTemplate.FormCustomisationSettings.DisplayTabs.Add((NoResString)"Tab 1 Desc", "Tab1");
			taskTemplate.FormCustomisationSettings.DisplayFields.Add((NoResString)"Field 1 Desc", "Field1", false, (NoResString)"", "Tab1", "X", 4);
			Factory.Save();

			var reloadedTemplate = (new BusinessObjectFactory()).Load<ProcessTaskTemplate>(taskTemplate.PK);
			AssertNotNull(reloadedTemplate.P0_FormState);

			taskTemplate.Delete();
			Factory.Save();

			AssertNoExceptionThrown(() => { var settings = reloadedTemplate.FormCustomisationSettings; });
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1199", Justification = "Testing Multilingual behavior")]
		public void TestMultilingualFieldsAreSetAfterReset()
		{
			var tabs = new FormCustomisableElementCollection();
			tabs.Add(ResString.GetMultilingualString("5DECA760-6948-42C0-BD6B-FB2139ED9F20", "Tab Description {0}", 1), "Tab1", false);
			tabs.Add(ResString.GetMultilingualString("5DECA760-6948-42C0-BD6B-FB2139ED9F20", "Tab Description {0}", 2), "Tab2", true);

			var fields = new FormCustomisableElementCollection();
			fields.Add(ResString.GetMultilingualString("E10E23A6-0607-4479-8709-45DBDC430D60", "Field Description {0}", 1), "Field1", false);
			fields.Add(ResString.GetMultilingualString("E10E23A6-0607-4479-8709-45DBDC430D60", "Field Description {0}", 2), "Field2", false);
			fields.Add(ResString.GetMultilingualString("E10E23A6-0607-4479-8709-45DBDC430D60", "Field Description {0}", 3), "Field3", false);

			var formSettingsProvider = new FormCustomisationSettingsProviderForTest();
			formSettingsProvider.GetDisplayTabsImplementation = () => tabs;
			formSettingsProvider.GetDisplayFieldsImplementation = () => fields;

			DummyWorkflowDescriptor.Instance.GetFormCustomisationSettingsProviderImplementation = () => formSettingsProvider;
			DummyWorkflowDescriptor.Instance.ClearFormCustomisationSettingsCacheForTest();

			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				var taskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				taskTemplate.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
				mockChs.Put("5DECA760-6948-42C0-BD6B-FB2139ED9F20", new ResourceStringData("5DECA760-6948-42C0-BD6B-FB2139ED9F20", "分页 {0}"));
				mockChs.Put("E10E23A6-0607-4479-8709-45DBDC430D60", new ResourceStringData("E10E23A6-0607-4479-8709-45DBDC430D60", "字段 {0}"));

				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					var customisationSetting = new FormCustomisationSettings(taskTemplate);
					AssertEquals(2, customisationSetting.DisplayTabs.Count);
					AssertEquals(3, customisationSetting.DisplayFields.Count);

					AssertArrayEqualsByElements("DescriptionMultilingual field should be set", new[] { "分页 1", "分页 2" }, customisationSetting.DisplayTabs.Cast<FormCustomisableElement>().Select(p => p.ElementDescriptionMultilingual).ToArray());
					AssertArrayEqualsByElements("DescriptionMultilingual field should be set", new[] { "字段 1", "字段 2", "字段 3" }, customisationSetting.DisplayFields.Cast<FormCustomisableElement>().Select(p => p.ElementDescriptionMultilingual).ToArray());

					customisationSetting.Reset();
					AssertEquals(2, customisationSetting.DisplayTabs.Count);
					AssertEquals(3, customisationSetting.DisplayFields.Count);

					AssertArrayEqualsByElements("DescriptionMultilingual field should be set", new[] { "分页 1", "分页 2" }, customisationSetting.DisplayTabs.Cast<FormCustomisableElement>().Select(p => p.ElementDescriptionMultilingual).ToArray());
					AssertArrayEqualsByElements("DescriptionMultilingual field should be set", new[] { "字段 1", "字段 2", "字段 3" }, customisationSetting.DisplayFields.Cast<FormCustomisableElement>().Select(p => p.ElementDescriptionMultilingual).ToArray());
				}
			}
		}

		#region Implementation

		FormCustomisableElementCollection Tabs;
		FormCustomisableElementCollection Fields;
		FormCustomisationSettingsProviderForTest FormSettingsProvider;
		ProcessTaskTemplate TaskTemplate;

		protected override void SetUp()
		{
			base.SetUp();

			Tabs = new FormCustomisableElementCollection();
			Tabs.Add((NoResString)"Desc1", "Tab1", true);
			Tabs.Add((NoResString)"Desc2", "Tab2", true);

			Fields = new FormCustomisableElementCollection();
			Fields.Add((NoResString)"Desc1", "Field1", false);
			Fields.Add((NoResString)"Desc2", "Field2", false);
			Fields.Add((NoResString)"Desc3", "Field3", false);

			FormSettingsProvider = new FormCustomisationSettingsProviderForTest();
			FormSettingsProvider.GetDisplayTabsImplementation = () => Tabs;
			FormSettingsProvider.GetDisplayFieldsImplementation = () => Fields;

			DummyWorkflowDescriptor.Instance.GetFormCustomisationSettingsProviderImplementation = () => FormSettingsProvider;
			FormSettingsProvider.GetPropertiesThatAffectWorkflowTemplateImplementation = () => new[] { ProcessTaskTemplate.Schema.P0_SubType1 };

			TaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			TaskTemplate.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
		}

		#endregion
	}
}
