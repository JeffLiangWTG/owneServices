using System;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business
{
	public sealed class ProcessManagementRegistry : RegistryItemSet
	{
		#region Singleton pattern

		ProcessManagementRegistry()
		{
		}

		public static ProcessManagementRegistry Instance
		{
			get { return instance ?? (instance = new ProcessManagementRegistry()); }
		}

		[ThreadStatic]
		static ProcessManagementRegistry instance;

		#endregion

		public override bool IsForProductivityWise => true;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories//
		{
			public static MultilingualString ProductivityTools_WorkItem { get { return CombineCategories(ProductivityTools, ResString.GetMultilingualString("8180E9AF-7910-40CC-B840-AD1BCC9CBD55", "Work Item")); } }
			public static MultilingualString ProductivityTools_WorkItem_GenericCaptionAttributes { get { return CombineCategories(ProductivityTools_WorkItem, ResString.GetMultilingualString("ebc00a9b-77b7-4702-879f-fd1283bab6cb", "Generic Caption Attributes")); } }
			public static MultilingualString ProductivityTools_WorkItem_MandatoryWorkItemTypes { get { return CombineCategories(ProductivityTools_WorkItem, ResString.GetMultilingualString("C059EBBB-5F32-4491-97B9-82DD7365B916", "Mandatory Work Item Types")); } }

			public static MultilingualString ProductivityTools_Project { get { return CombineCategories(ProductivityTools, ResString.GetMultilingualString("29241203-D179-4CA1-BFEE-9933B25DCE00", "Project")); } }
			public static MultilingualString ProductivityTools_Project_GenericCaptionAttributes { get { return CombineCategories(ProductivityTools_Project, ResString.GetMultilingualString("84a4de43-9ae4-490c-9bfa-b9b74beba41b", "Generic Caption Attributes")); } }
			public static MultilingualString ProductivityTools_Project_MandatoryProjectTypes { get { return CombineCategories(ProductivityTools_Project, ResString.GetMultilingualString("8996D97B-6485-45DD-A7FC-3BCB8838FDF7", "Mandatory Project Types")); } }

			public static MultilingualString ProductivityTools_Project_JiraIntegration { get { return CombineCategories(ProductivityTools_Project, ResString.GetMultilingualString("ababfaa8-e436-46b1-b166-8dcf18c4db91", "Jira Integration")); } }
			public static MultilingualString ProductivityTools_Project_JiraIntegration_DataMapping { get { return CombineCategories(ProductivityTools_Project_JiraIntegration, ResString.GetMultilingualString("3d42b441-7261-41e8-a111-1f0ae0059328", "Data Mapping")); } }
			public static MultilingualString ProductivityTools_Project_JiraIntegration_DataMapping_IssueStatus { get { return CombineCategories(ProductivityTools_Project_JiraIntegration_DataMapping, ResString.GetMultilingualString("6C869C8E-6D14-42C9-BACE-37257D6E6AA6", "Issue Status")); } }
			public static MultilingualString ProductivityTools_Project_JiraIntegration_DataMapping_LinkType { get { return CombineCategories(ProductivityTools_Project_JiraIntegration_DataMapping, ResString.GetMultilingualString("C64C90AE-4CA1-42B1-8EE6-5745413A3331", "Link Types")); } }

			public static MultilingualString ProductivityTools_CustomerServiceTickets { get { return CombineCategories(ProductivityTools, ResString.GetMultilingualString("A529896B-4696-4F3C-9192-473AB74F2443", "Customer Service Tickets")); } }
			public static MultilingualString ProductivityTools_CustomerServiceTickets_SelectionCriteria { get { return CombineCategories(ProductivityTools_CustomerServiceTickets, ResString.GetMultilingualString("1CE112A3-F2B5-4CCE-A1C2-092DFEF738EE", "Selection Criteria")); } }
			public static MultilingualString ProductivityTools_CustomerServiceTickets_SelectionCriteria_Captions { get { return CombineCategories(ProductivityTools_CustomerServiceTickets_SelectionCriteria, ResString.GetMultilingualString("F4473AEB-B4C3-49E6-B06E-F6736E5C06EF", "Captions")); } }
			public static MultilingualString ProductivityTools_CustomerServiceTickets_SelectionCriteria_Descriptions { get { return CombineCategories(ProductivityTools_CustomerServiceTickets_SelectionCriteria, ResString.GetMultilingualString("E5E3A692-45F0-4CC5-901A-508CF5C5E8F6", "Descriptions")); } }
			public static MultilingualString ProductivityTools_CustomerServiceTickets_SelectionCriteria_Values { get { return CombineCategories(ProductivityTools_CustomerServiceTickets_SelectionCriteria, ResString.GetMultilingualString("f53540ac-60b2-4422-a1d9-4f3c2e5ce31a", "Values")); } }
			public static MultilingualString ProductivityTools_CustomerServiceTickets_SelectionCriteria_WebPortal { get { return CombineCategories(ProductivityTools_CustomerServiceTickets_SelectionCriteria, ResString.GetMultilingualString("e1c9822d-5bfc-41d3-9cf6-182f4d4b04d4", "Web Portal")); } }
		}

		#endregion

		#region Work Item Registries

		public CodeDescriptionBoolTreeRegistryItem WorkItemTypeTree
		{
			get
			{
				return GetItem(
					"WorkItemTypeTree",
					delegate
					{
						var editorInfo = new CodeDescriptionBoolTreeRegistryEditorInfo(
							new MultilingualString[]
							{
								SelectionCriterion1ValuesString,
								SelectionCriterion2ValuesString,
								SelectionCriterion3ValuesString,
								SelectionCriterion4ValuesString,
								SelectionCriterion5ValuesString,
							},
							ActiveString,
							null, true, false);

						var allDescriptions = new MultilingualString[]
							{
								AllSelectionCriterion1Values,
								AllSelectionCriterion2Values,
								AllSelectionCriterion3Values,
								AllSelectionCriterion4Values,
							};

						var defaultValue = new CodeDescriptionBoolTreeNodeCollection(true, 3, 5, allDescriptions);
						defaultValue.AddSystemChildren(null);
						var caption = SelectionCriteriaValuesCaption;
						var undefined = ResString.GetMultilingualString("13B4A12D-43B5-4DC7-B804-876F35265E07", "Undefined - You can modify this in the System Registry, under {0}/{1}", Categories.ProductivityTools_WorkItem, caption);
						var all = CodeDescriptionBoolTreeNode.AllCode;
						defaultValue.AddSystemChildren(defaultValue.Add("UDF", undefined));
						defaultValue.AddSystemChildren(defaultValue.Add("UDF", undefined, defaultValue.Find(all)));
						defaultValue.AddSystemChildren(defaultValue.Add("UDF", undefined, defaultValue.Find(all, all)));
						defaultValue.AddSystemChildren(defaultValue.Add("UDF", undefined, defaultValue.Find(all, all, all)));
						defaultValue.AddSystemChildren(defaultValue.Add("UDF", undefined, defaultValue.Find(all, all, all, all)));

						return new CodeDescriptionBoolTreeRegistryItem(
							"WorkItemTypeTree",
							Categories.ProductivityTools_WorkItem,
							caption,
							SelectionCriteriaValuesHint,
							RegistryStorageFlags.System,
							editorInfo,
							defaultValue);
					});
			}
		}

		#endregion

		#region Identify Defect Cause

		public CodePairRegistryItem IdentifyDefectCauseTaskType
		{
			get
			{
				return GetItem("IdentifyDefectCauseTaskType", delegate
				{
					var lookUpListProvider = new CodeDescriptionPairListProvider(() => WorkflowDataRegistryHelper.GetTaskTypeList(WorkflowDescriptors.WorkItemWorkflowDescriptorCode));

					return new CodePairRegistryItem(
						"IdentifyDefectCauseTaskType",
						Categories.ProductivityTools_WorkItem,
						ResString.GetMultilingualString("C2079B7B-A547-47C0-8125-835969D0D67F", "Identify Defect Cause Task Type"),
						ResString.GetMultilingualString("0FBF0BA2-18F1-4152-AC3C-53613716D3C0", "This setting is to specify the task type that corresponds to 'Identify Defect Cause' concept."),
						lookUpListProvider,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						"IDC");
				});
			}
		}

		#endregion

		#region DefectWorkItemTypes

		public StringArrayRegistryItem DefectWorkItemTypes
		{
			get
			{
				return GetItem("DefectWorkItemTypes", delegate
				{
					var item = new StringArrayRegistryItem(
						"DefectWorkItemTypes",
						Categories.ProductivityTools_WorkItem,
						ResString.GetMultilingualString("1AB6A754-3468-4EC1-9EAA-63DEEC83D62C", "Defect Activity Sub Types"),
						ResString.GetMultilingualString("788664E1-DC3B-4E9F-B1FC-FAB368BB6983", "List of activity sub types which identify a Work Item as a Defect. The Defect Introduced fields will only be enabled for these values."),
						RegistryStorageFlags.System,
						new string[] { "UDF" });
					item.DataType.MaximumLength = 3;
					item.DataType.CharacterCase = CharacterCase.Upper;
					return item;
				});
			}
		}

		#endregion

		#region Defect Introduced Task Types

		public StringArrayRegistryItem DefectIntroducedTaskTypes
		{
			get
			{
				return GetItem("DefectIntroducedTaskTypes", delegate
				{
					var item = new StringArrayRegistryItem(
						"DefectIntroducedTaskTypes",
						Categories.ProductivityTools_WorkItem,
						ResString.GetMultilingualString("74AE8808-F4A2-454F-AAFB-4A07521D0272", "Defect Introduced Task Types"),
						ResString.GetMultilingualString("0BEDE27C-BFF3-4B25-970E-59477E08664D", "List of task types which can cause a defect. The Defect Introduced In Task field will only allow these types. Leave empty to include all task types."),
						RegistryStorageFlags.System,
						Array.Empty<string>());
					item.DataType.MaximumLength = 3;
					item.DataType.CharacterCase = CharacterCase.Upper;
					return item;
				});
			}
		}

		#endregion

		#region Mandatory Work Item Types

		public BooleanRegistryItem WorkItemTypeMandatory
		{
			get
			{
				return GetItem("WorkItemTypeMandatory", delegate
				{
					return new BooleanRegistryItem(
						"WorkItemTypeMandatory",
						Categories.ProductivityTools_WorkItem_MandatoryWorkItemTypes,
						SelectionCriterion1MandatoryCaption,
						SelectionCriterion1MandatoryHint,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public BooleanRegistryItem WorkItemAreaMandatory
		{
			get
			{
				return GetItem("WorkItemAreaMandatory", delegate
				{
					return new BooleanRegistryItem(
						"WorkItemAreaMandatory",
						Categories.ProductivityTools_WorkItem_MandatoryWorkItemTypes,
						SelectionCriterion2MandatoryCaption,
						SelectionCriterion2MandatoryHint,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public BooleanRegistryItem WorkItemActivityTypeMandatory
		{
			get
			{
				return GetItem("WorkItemActivityTypeMandatory", delegate
				{
					return new BooleanRegistryItem(
						"WorkItemActivityTypeMandatory",
						Categories.ProductivityTools_WorkItem_MandatoryWorkItemTypes,
						SelectionCriterion3MandatoryCaption,
						SelectionCriterion3MandatoryHint,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public BooleanRegistryItem WorkItemActivitySubTypeMandatory
		{
			get
			{
				return GetItem("WorkItemActivitySubTypeMandatory", delegate
				{
					return new BooleanRegistryItem(
						"WorkItemActivitySubTypeMandatory",
						Categories.ProductivityTools_WorkItem_MandatoryWorkItemTypes,
						SelectionCriterion4MandatoryCaption,
						SelectionCriterion4MandatoryHint,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public BooleanRegistryItem WorkItemPriorityMandatory
		{
			get
			{
				return GetItem("WorkItemPriorityMandatory", delegate
				{
					return new BooleanRegistryItem(
						"WorkItemPriorityMandatory",
						Categories.ProductivityTools_WorkItem_MandatoryWorkItemTypes,
						SelectionCriterion5MandatoryCaption,
						SelectionCriterion5MandatoryHint,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region Mandatory Project Types

		public BooleanRegistryItem ProjectTypeMandatory
		{
			get
			{
				return GetItem("ProjectTypeMandatory", delegate
				{
					return new BooleanRegistryItem(
						"ProjectTypeMandatory",
						Categories.ProductivityTools_Project_MandatoryProjectTypes,
						SelectionCriterion1MandatoryCaption,
						SelectionCriterion1MandatoryHint,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public BooleanRegistryItem ProjectSubTypeMandatory
		{
			get
			{
				return GetItem("ProjectSubTypeMandatory", delegate
				{
					return new BooleanRegistryItem(
						"ProjectSubTypeMandatory",
						Categories.ProductivityTools_Project_MandatoryProjectTypes,
						SelectionCriterion2MandatoryCaption,
						SelectionCriterion2MandatoryHint,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public BooleanRegistryItem ProjectModuleMandatory
		{
			get
			{
				return GetItem("ProjectModuleMandatory", delegate
				{
					return new BooleanRegistryItem(
						"ProjectModuleMandatory",
						Categories.ProductivityTools_Project_MandatoryProjectTypes,
						SelectionCriterion3MandatoryCaption,
						SelectionCriterion3MandatoryHint,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public BooleanRegistryItem ProjectPriorityMandatory
		{
			get
			{
				return GetItem("ProjectPriorityMandatory", delegate
				{
					return new BooleanRegistryItem(
						"ProjectPriorityMandatory",
						Categories.ProductivityTools_Project_MandatoryProjectTypes,
						SelectionCriterion4MandatoryCaption,
						SelectionCriterion4MandatoryHint,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region Project Registries

		public CodeDescriptionBoolTreeRegistryItem ProjectTypeTree
		{
			get
			{
				return GetItem(
					"ProjectTypeTree",
					delegate
					{
						var editorInfo = new CodeDescriptionBoolTreeRegistryEditorInfo(
							new MultilingualString[]
							{
								SelectionCriterion1ValuesString,
								SelectionCriterion2ValuesString,
								SelectionCriterion3ValuesString,
								SelectionCriterion4ValuesString,
							},
							ActiveString,
							null, true, false);

						var allDescriptions = new MultilingualString[]
							{
								AllSelectionCriterion1Values,
								AllSelectionCriterion2Values,
								AllSelectionCriterion3Values,
							};

						var defaultValue = new CodeDescriptionBoolTreeNodeCollection(true, 3, 4, allDescriptions);
						defaultValue.AddSystemChildren(null);
						var caption = SelectionCriteriaValuesCaption;
						var undefined = ResString.GetMultilingualString("0A9F1BEE-1CD5-4DBF-9E4A-0F8683281283", "Undefined - You can modify this in the System Registry, under {0}/{1}", Categories.ProductivityTools_Project, caption);
						var all = CodeDescriptionBoolTreeNode.AllCode;
						defaultValue.AddSystemChildren(defaultValue.Add("UDF", undefined));
						defaultValue.AddSystemChildren(defaultValue.Add("UDF", undefined, defaultValue.Find(all)));
						defaultValue.AddSystemChildren(defaultValue.Add("UDF", undefined, defaultValue.Find(all, all)));
						defaultValue.AddSystemChildren(defaultValue.Add("UDF", undefined, defaultValue.Find(all, all, all)));

						return new CodeDescriptionBoolTreeRegistryItem(
							"ProjectTypeTree",
							Categories.ProductivityTools_Project,
							caption,
							SelectionCriteriaValuesHint,
							RegistryStorageFlags.System,
							editorInfo,
							defaultValue);
					});
			}
		}

		const int MaxEmailAddressLength = 254;

		public MultilingualStringRegistryItem ProjectDefaultFromEmailAddress
		{
			get
			{
				return GetItem(
					"ProjectDefaultFromEmailAddress",
					delegate
					{
						return new MultilingualStringRegistryItem(
							"ProjectDefaultFromEmailAddress",
							Categories.ProductivityTools_Project,
							ResString.GetMultilingualString("4AA2C0BE-509D-43AE-AF3E-1467FA262D9B", "Project Default From Email Address"),
							ResString.GetMultilingualString("7194C3E5-C1D7-4596-BE67-C16EA896AEA7", "This is the 'From' address that will appear on emails sent via the Project module."),
							new StringRegistryDataType(1, MaxEmailAddressLength),
							RegistryStorageFlags.Company,
							RegistryOptions.Default);
					});
			}
		}

		#endregion

		#region Generic Caption Attributes - Work Items

		const int MaxCustomLabelLength = 20;

		public MultilingualStringRegistryItem WorkItemTypeLabel
		{
			get
			{
				return GetItem(
					"WorkItemTypeLabel",
					delegate
					{
						return new MultilingualStringRegistryItem(
							"WorkItemTypeLabel",
							Categories.ProductivityTools_WorkItem_GenericCaptionAttributes,
							SelectionCriterion1CaptionString,
							WorkItemSelectionCriteriaHint,
							new StringRegistryDataType(1, MaxCustomLabelLength),
							null,
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							ResString.GetMultilingualString("842150AE-5AD3-4910-8E4B-35EEDC06682A", "Type"));
					});
			}
		}

		public MultilingualStringRegistryItem WorkItemAreaLabel
		{
			get
			{
				return GetItem(
					"WorkItemAreaLabel",
					delegate
					{
						return new MultilingualStringRegistryItem(
							"WorkItemAreaLabel",
							Categories.ProductivityTools_WorkItem_GenericCaptionAttributes,
							SelectionCriterion2CaptionString,
							WorkItemSelectionCriteriaHint,
							new StringRegistryDataType(1, MaxCustomLabelLength),
							null,
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							ResString.GetMultilingualString("D4123C1F-5E96-4C2A-9C30-2C9930EA4071", "Area"));
					});
			}
		}

		public MultilingualStringRegistryItem WorkItemActivityTypeLabel
		{
			get
			{
				return GetItem(
					"WorkItemActivityTypeLabel",
					delegate
					{
						return new MultilingualStringRegistryItem(
							"WorkItemActivityTypeLabel",
							Categories.ProductivityTools_WorkItem_GenericCaptionAttributes,
							SelectionCriterion3CaptionString,
							WorkItemSelectionCriteriaHint,
							new StringRegistryDataType(1, MaxCustomLabelLength),
							null,
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							ResString.GetMultilingualString("9E93496A-3D58-471A-92D7-5F5933F6F35A", "Activity Type"));
					});
			}
		}

		public MultilingualStringRegistryItem WorkItemActivitySubTypeLabel
		{
			get
			{
				return GetItem(
					"WorkItemActivitySubTypeLabel",
					delegate
					{
						return new MultilingualStringRegistryItem(
							"WorkItemActivitySubTypeLabel",
							Categories.ProductivityTools_WorkItem_GenericCaptionAttributes,
							SelectionCriterion4CaptionString,
							WorkItemSelectionCriteriaHint,
							new StringRegistryDataType(1, MaxCustomLabelLength),
							null,
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							ResString.GetMultilingualString("C4885E75-61EB-456D-A6DE-08A4B511385F", "Activity Sub Type"));
					});
			}
		}

		public MultilingualStringRegistryItem WorkItemPriorityLabel
		{
			get
			{
				return GetItem(
					"WorkItemPriorityLabel",
					delegate
					{
						return new MultilingualStringRegistryItem(
							"WorkItemPriorityLabel",
							Categories.ProductivityTools_WorkItem_GenericCaptionAttributes,
							SelectionCriterion5CaptionString,
							WorkItemSelectionCriteriaHint,
							new StringRegistryDataType(1, MaxCustomLabelLength),
							null,
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							ResString.GetMultilingualString("E50CE0E7-DD7E-437E-B159-19A75FB87D31", "Priority"));
					});
			}
		}

		public MultilingualStringRegistryItem DefectIntroducedInWorkItemLabel
		{
			get
			{
				return GetItem(
					"DefectIntroducedInWorkItemLabel",
					delegate
					{
						return new MultilingualStringRegistryItem(
							"DefectIntroducedInWorkItemLabel",
							Categories.ProductivityTools_WorkItem_GenericCaptionAttributes,
							ResString.GetMultilingualString("0D7F856E-5E9E-4CC0-AD1F-BD6B38D3AFEA", "Defect Introduced in WI Label"),
							ResString.GetMultilingualString("CD78BA8A-AF61-4FF0-87CA-82CFFD2ADE61", "Allows you to customize the label for Defect Introduced in WI."),
							new StringRegistryDataType(1, 30),
							null,
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							ResString.GetMultilingualString("1125F112-737D-441C-B46D-9A5C535FCC77", "Defect Introduced in WI"));
					});
			}
		}

		public MultilingualStringRegistryItem DefectIntroducedInTaskLabel
		{
			get
			{
				return GetItem(
					"DefectIntroducedInTaskLabel",
					delegate
					{
						return new MultilingualStringRegistryItem(
							"DefectIntroducedInTaskLabel",
							Categories.ProductivityTools_WorkItem_GenericCaptionAttributes,
							ResString.GetMultilingualString("9F50AEE8-5FEB-412A-92B3-EADAE021F150", "Defect Introduced in Task Label"),
							ResString.GetMultilingualString("B31F55F8-0ED4-4386-B211-2485F1751E46", "Allows you to customize the label for Defect Introduced in Task."),
							new StringRegistryDataType(1, 30),
							null,
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							ResString.GetMultilingualString("65DE6860-0114-4A2F-8464-B0B04C3B2564", "Defect Introduced in Task"));
					});
			}
		}

		public MultilingualStringRegistryItem FirstCBThatMissedDefectLabel
		{
			get
			{
				return GetItem(
					"FirstCBThatMissedDefectLabel",
					delegate
					{
						return new MultilingualStringRegistryItem(
							"FirstCBThatMissedDefectLabel",
							Categories.ProductivityTools_WorkItem_GenericCaptionAttributes,
							ResString.GetMultilingualString("F174BC37-0004-4F23-BED1-C0661687E3E5", "First CB that Missed Defect Label"),
							ResString.GetMultilingualString("64DD9182-9CA1-415B-B132-7D2F9A92A516", "Allows you to customize the label for First CB that Missed Defect."),
							new StringRegistryDataType(1, 30),
							null,
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							ResString.GetMultilingualString("3355F99F-3BBB-4A0A-A046-FF47D7B0647C", "First CB that Missed Defect"));
					});
			}
		}

		#endregion

		#region Generic Caption Attributes - Projects

		public MultilingualStringRegistryItem ProjectTypeLabel
		{
			get
			{
				return GetItem(
					"ProjectTypeLabel",
					delegate
					{
						return new MultilingualStringRegistryItem(
							"ProjectTypeLabel",
							Categories.ProductivityTools_Project_GenericCaptionAttributes,
							SelectionCriterion1CaptionString,
							ProjectSelectionCriteriaHint,
							new StringRegistryDataType(1, MaxCustomLabelLength),
							null,
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							ResString.GetMultilingualString("89AA3254-4754-447C-97B8-E5001AEB55E7", "Project Type"));
					});
			}
		}

		public MultilingualStringRegistryItem ProjectSubtypeLabel
		{
			get
			{
				return GetItem(
					"ProjectSubtypeLabel",
					delegate
					{
						return new MultilingualStringRegistryItem(
							"ProjectSubtypeLabel",
							Categories.ProductivityTools_Project_GenericCaptionAttributes,
							SelectionCriterion2CaptionString,
							ProjectSelectionCriteriaHint,
							new StringRegistryDataType(1, MaxCustomLabelLength),
							null,
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							ResString.GetMultilingualString("4CC06B05-4CF2-4D35-82BD-6EE78E57FCCA", "Project Sub Type"));
					});
			}
		}

		public MultilingualStringRegistryItem ProjectModuleLabel
		{
			get
			{
				return GetItem(
					"ProjectModuleLabel",
					delegate
					{
						return new MultilingualStringRegistryItem(
							"ProjectModuleLabel",
							Categories.ProductivityTools_Project_GenericCaptionAttributes,
							SelectionCriterion3CaptionString,
							ProjectSelectionCriteriaHint,
							new StringRegistryDataType(1, MaxCustomLabelLength),
							null,
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							ResString.GetMultilingualString("0C72EFDA-5FF5-4194-B521-FE4B31AFEB2B", "Project Module"));
					});
			}
		}

		public MultilingualStringRegistryItem ProjectPriorityLabel
		{
			get
			{
				return GetItem(
					"ProjectPriorityLabel",
					delegate
					{
						return new MultilingualStringRegistryItem(
							"ProjectPriorityLabel",
							Categories.ProductivityTools_Project_GenericCaptionAttributes,
							SelectionCriterion4CaptionString,
							ProjectSelectionCriteriaHint,
							new StringRegistryDataType(1, MaxCustomLabelLength),
							null,
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							ResString.GetMultilingualString("42B9770E-0E8D-4549-A6E1-B30113E629CE", "Priority"));
					});
			}
		}

		#endregion

		#region Customer Service Tickets

		#region Selection Criteria

		#region Captions

		const int MaxCustomSelectionCriterionLabelLength = 30;

		public MultilingualStringRegistryItem SelectionCriterion1Caption
		{
			get
			{
				return GetItem(
					"SelectionCriterion1Caption",
					() => new MultilingualStringRegistryItem(
						"SelectionCriterion1Caption",
						Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Captions,
						SelectionCriterion1CaptionString,
						ResString.GetMultilingualString("AAEA0926-0C9C-4EEF-A269-285005C9CF7C", "Overrides the default caption for Selection Criterion 1 in Customer Service Tickets."),
						new StringRegistryDataType(1, MaxCustomSelectionCriterionLabelLength),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						ResString.GetMultilingualString("9C22A138-51E1-4C56-A607-7C8AE008D184", "Selection Criterion 1")));
			}
		}

		public MultilingualStringRegistryItem SelectionCriterion2Caption
		{
			get
			{
				return GetItem(
					"SelectionCriterion2Caption",
					() => new MultilingualStringRegistryItem(
						"SelectionCriterion2Caption",
						Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Captions,
						SelectionCriterion2CaptionString,
						ResString.GetMultilingualString("6A40E238-848F-4775-B716-4B6863A1FDEA", "Overrides the default caption for Selection Criterion 2 in Customer Service Tickets."),
						new StringRegistryDataType(1, MaxCustomSelectionCriterionLabelLength),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						ResString.GetMultilingualString("3CD4310E-194B-4744-A535-CDE2C84C3BAF", "Selection Criterion 2")));
			}
		}

		public MultilingualStringRegistryItem SelectionCriterion3Caption
		{
			get
			{
				return GetItem(
					"SelectionCriterion3Caption",
					() => new MultilingualStringRegistryItem(
						"SelectionCriterion3Caption",
						Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Captions,
						SelectionCriterion3CaptionString,
						ResString.GetMultilingualString("D5381090-7391-49D0-8F31-04516EBD2F4F", "Overrides the default caption for Selection Criterion 3 in Customer Service Tickets."),
						new StringRegistryDataType(1, MaxCustomSelectionCriterionLabelLength),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						ResString.GetMultilingualString("CA6A44E6-B3A5-4A66-99C8-A3398299EBAA", "Selection Criterion 3")));
			}
		}

		public MultilingualStringRegistryItem SelectionCriterion4Caption
		{
			get
			{
				return GetItem(
					"SelectionCriterion4Caption",
					() => new MultilingualStringRegistryItem(
						"SelectionCriterion4Caption",
						Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Captions,
						SelectionCriterion4CaptionString,
						ResString.GetMultilingualString("C00EFBE9-C166-417F-B74A-C859FFC5C8F8", "Overrides the default caption for Selection Criterion 4 in Customer Service Tickets."),
						new StringRegistryDataType(1, MaxCustomSelectionCriterionLabelLength),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						ResString.GetMultilingualString("724E35E7-C866-47E6-AFD0-3F6681005F98", "Selection Criterion 4")));
			}
		}

		public MultilingualStringRegistryItem SelectionCriterion5Caption
		{
			get
			{
				return GetItem(
					"SelectionCriterion5Caption",
					() => new MultilingualStringRegistryItem(
						"SelectionCriterion5Caption",
						Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Captions,
						SelectionCriterion5CaptionString,
						ResString.GetMultilingualString("58ADEB10-FA1D-4BE5-BC13-C6B16A492C70", "Overrides the default caption for Selection Criterion 5 in Customer Service Tickets."),
						new StringRegistryDataType(1, MaxCustomSelectionCriterionLabelLength),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						ResString.GetMultilingualString("7A754445-E210-4DFF-98B7-EDDFE1DFD050", "Selection Criterion 5")));
			}
		}

		#endregion

		#region Descriptions

		const int MaxCustomSelectionCriterionDescriptionLength = 100;

		public MultilingualStringRegistryItem SelectionCriterion1Description
		{
			get
			{
				return GetItem(
					"SelectionCriterion1Description",
					() => new MultilingualStringRegistryItem(
						"SelectionCriterion1Description",
						Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Descriptions,
						ResString.GetMultilingualString("367AADFC-C8DE-4375-A8AC-CE5203A52B80", "Selection Criterion 1 Description"),
						ResString.GetMultilingualString("1D3B0ECD-59B1-4436-8987-6C37C3837B56", "Overrides the default long description for Selection Criterion 1 in Customer Service Tickets."),
						new StringRegistryDataType(1, MaxCustomSelectionCriterionDescriptionLength),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						ResString.GetMultilingualString("F35C9AD9-21F3-40B7-8AEF-92F90D4E419E", "The first criterion for deciding which Workflow Template to use for this Customer Service Ticket.")));
			}
		}

		public MultilingualStringRegistryItem SelectionCriterion2Description
		{
			get
			{
				return GetItem(
					"SelectionCriterion2Description",
					() => new MultilingualStringRegistryItem(
						"SelectionCriterion2Description",
						Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Descriptions,
						ResString.GetMultilingualString("A2DE69FC-48CE-4C72-A36E-4922E80081F1", "Selection Criterion 2 Description"),
						ResString.GetMultilingualString("A4BD1D45-96F8-4412-AF7E-A901A08829BA", "Overrides the default long description for Selection Criterion 2 in Customer Service Tickets."),
						new StringRegistryDataType(1, MaxCustomSelectionCriterionDescriptionLength),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						ResString.GetMultilingualString("7CB54A0D-9EFF-4B79-8DFF-00013DBF346D", "The second criterion for deciding which Workflow Template to use for this Customer Service Ticket.")));
			}
		}

		public MultilingualStringRegistryItem SelectionCriterion3Description
		{
			get
			{
				return GetItem(
					"SelectionCriterion3Description",
					() => new MultilingualStringRegistryItem(
						"SelectionCriterion3Description",
						Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Descriptions,
						ResString.GetMultilingualString("BDB6329C-B550-4439-A795-D72F48738ABD", "Selection Criterion 3 Description"),
						ResString.GetMultilingualString("8DEC07A2-D932-4553-972F-F6765D729285", "Overrides the default long description for Selection Criterion 3 in Customer Service Tickets."),
						new StringRegistryDataType(1, MaxCustomSelectionCriterionDescriptionLength),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						ResString.GetMultilingualString("E6D17129-FC08-417B-8133-54B368B43871", "The third criterion for deciding which Workflow Template to use for this Customer Service Ticket.")));
			}
		}

		public MultilingualStringRegistryItem SelectionCriterion4Description
		{
			get
			{
				return GetItem(
					"SelectionCriterion4Description",
					() => new MultilingualStringRegistryItem(
						"SelectionCriterion4Description",
						Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Descriptions,
						ResString.GetMultilingualString("66DC5E23-8FEC-40F3-9D33-77CC3042CB83", "Selection Criterion 4 Description"),
						ResString.GetMultilingualString("E4618BB6-2600-41A1-B92C-5BCF2C9B9AD5", "Overrides the default long description for Selection Criterion 4 in Customer Service Tickets."),
						new StringRegistryDataType(1, MaxCustomSelectionCriterionDescriptionLength),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						ResString.GetMultilingualString("9AF0E7D6-4DC0-42D9-8777-0ECF2DF968D5", "The fourth criterion for deciding which Workflow Template to use for this Customer Service Ticket.")));
			}
		}

		public MultilingualStringRegistryItem SelectionCriterion5Description
		{
			get
			{
				return GetItem(
					"SelectionCriterion5Description",
					() => new MultilingualStringRegistryItem(
						"SelectionCriterion5Description",
						Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Descriptions,
						ResString.GetMultilingualString("7C3E8CC1-6DE1-4187-9382-A4EFA2E8A2EB", "Selection Criterion 5 Description"),
						ResString.GetMultilingualString("3997DF99-C1D7-439D-AAD8-7016B2586C1D", "Overrides the default long description for Selection Criterion 5 in Customer Service Tickets."),
						new StringRegistryDataType(1, MaxCustomSelectionCriterionDescriptionLength),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						ResString.GetMultilingualString("406B337B-568C-470C-8905-85C9FC5EFF1A", "The fifth criterion for deciding which Workflow Template to use for this Customer Service Ticket.")));
			}
		}

		#endregion

		#region Values

		public CodeDescriptionPairListRegistryItem SelectionCriterion1Values => GetItem("SelectionCriterion1Values", () =>
			new CodeDescriptionPairListRegistryItem(name: "SelectionCriterion1Values",
				category: Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Values,
				caption: SelectionCriterion1ValuesString,
				hint: ResString.GetMultilingualString("042f1fd9-1a12-4128-9d7b-a053c284d73b", "The values which can be selected in the relevant Selection Criterion field on the Customer Service Ticket form."),
				maxCodeLength: WorkRequestSchema.WKR_SelectionCriteria1.MaxLength,
				storage: RegistryStorageFlags.System
			));

		public CodeDescriptionPairListRegistryItem SelectionCriterion2Values => GetItem("SelectionCriterion2Values", () =>
			new CodeDescriptionPairListRegistryItem(name: "SelectionCriterion2Values",
				category: Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Values,
				caption: SelectionCriterion2ValuesString,
				hint: ResString.GetMultilingualString("042f1fd9-1a12-4128-9d7b-a053c284d73b", "The values which can be selected in the relevant Selection Criterion field on the Customer Service Ticket form."),
				maxCodeLength: WorkRequestSchema.WKR_SelectionCriteria2.MaxLength,
				storage: RegistryStorageFlags.System
			));

		public CodeDescriptionPairListRegistryItem SelectionCriterion3Values => GetItem("SelectionCriterion3Values", () =>
			new CodeDescriptionPairListRegistryItem(name: "SelectionCriterion3Values",
				category: Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Values,
				caption: SelectionCriterion3ValuesString,
				hint: ResString.GetMultilingualString("042f1fd9-1a12-4128-9d7b-a053c284d73b", "The values which can be selected in the relevant Selection Criterion field on the Customer Service Ticket form."),
				maxCodeLength: WorkRequestSchema.WKR_SelectionCriteria3.MaxLength,
				storage: RegistryStorageFlags.System
			));

		public CodeDescriptionPairListRegistryItem SelectionCriterion4Values => GetItem("SelectionCriterion4Values", () =>
			new CodeDescriptionPairListRegistryItem(name: "SelectionCriterion4Values",
				category: Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Values,
				caption: SelectionCriterion4ValuesString,
				hint: ResString.GetMultilingualString("042f1fd9-1a12-4128-9d7b-a053c284d73b", "The values which can be selected in the relevant Selection Criterion field on the Customer Service Ticket form."),
				maxCodeLength: WorkRequestSchema.WKR_SelectionCriteria4.MaxLength,
				storage: RegistryStorageFlags.System
			));

		public CodeDescriptionPairListRegistryItem SelectionCriterion5Values => GetItem("SelectionCriterion5Values", () =>
			new CodeDescriptionPairListRegistryItem(name: "SelectionCriterion5Values",
				category: Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_Values,
				caption: SelectionCriterion5ValuesString,
				hint: ResString.GetMultilingualString("042f1fd9-1a12-4128-9d7b-a053c284d73b", "The values which can be selected in the relevant Selection Criterion field on the Customer Service Ticket form."),
				maxCodeLength: WorkRequestSchema.WKR_SelectionCriteria5.MaxLength,
				storage: RegistryStorageFlags.System
			));

		#endregion

		#region Portal Visibility

		public BooleanRegistryItem ShowSelectionCriterion1OnWebPortal => GetItem("ShowSelectionCriterion1OnWebPortal", () =>
				new BooleanRegistryItem(
					name: "ShowSelectionCriterion1OnWebPortal",
					category: Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_WebPortal,
					caption: ResString.GetMultilingualString("ceff4839-32ee-46bb-a231-092c114188bb", "Show Selection Criterion 1 on Web Portal"),
					hint: ResString.GetMultilingualString("036e0f9f-7a8b-4ab8-9677-323cdea65d63", "Specifies whether the Selection Criterion 1 field is shown on the Customer Service Tickets web portal."),
					storage: RegistryStorageFlags.System,
					defaultValue: true
					)
			);

		public BooleanRegistryItem ShowSelectionCriterion2OnWebPortal => GetItem("ShowSelectionCriterion2OnWebPortal", () =>
				new BooleanRegistryItem(
					name: "ShowSelectionCriterion2OnWebPortal",
					category: Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_WebPortal,
					caption: ResString.GetMultilingualString("78f87d97-6d88-404f-a798-2f0c101c0ad4", "Show Selection Criterion 2 on Web Portal"),
					hint: ResString.GetMultilingualString("911b3659-d4d4-4d54-9f7c-33cf9736050a", "Specifies whether the Selection Criterion 2 field is shown on the Customer Service Tickets web portal."),
					storage: RegistryStorageFlags.System,
					defaultValue: true
					)
			);

		public BooleanRegistryItem ShowSelectionCriterion3OnWebPortal => GetItem("ShowSelectionCriterion3OnWebPortal", () =>
				new BooleanRegistryItem(
					name: "ShowSelectionCriterion3OnWebPortal",
					category: Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_WebPortal,
					caption: ResString.GetMultilingualString("b710e7a7-4349-4711-a640-79ab4c3471bc", "Show Selection Criterion 3 on Web Portal"),
					hint: ResString.GetMultilingualString("dbb6821d-d0d8-455d-bc7e-f244ba39c0eb", "Specifies whether the Selection Criterion 3 field is shown on the Customer Service Tickets web portal."),
					storage: RegistryStorageFlags.System,
					defaultValue: true
					)
			);

		public BooleanRegistryItem ShowSelectionCriterion4OnWebPortal => GetItem("ShowSelectionCriterion4OnWebPortal", () =>
				new BooleanRegistryItem(
					name: "ShowSelectionCriterion4OnWebPortal",
					category: Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_WebPortal,
					caption: ResString.GetMultilingualString("b27cbc04-4cfe-4c66-8f65-745553d8fb8d", "Show Selection Criterion 4 on Web Portal"),
					hint: ResString.GetMultilingualString("a32d0d43-47e1-4759-8af0-c8e9c11a849a", "Specifies whether the Selection Criterion 4 field is shown on the Customer Service Tickets web portal."),
					storage: RegistryStorageFlags.System,
					defaultValue: true
					)
			);

		public BooleanRegistryItem ShowSelectionCriterion5OnWebPortal => GetItem("ShowSelectionCriterion5OnWebPortal", () =>
				new BooleanRegistryItem(
					name: "ShowSelectionCriterion5OnWebPortal",
					category: Categories.ProductivityTools_CustomerServiceTickets_SelectionCriteria_WebPortal,
					caption: ResString.GetMultilingualString("08add22a-27c5-4359-8ec7-bf8ed0168101", "Show Selection Criterion 5 on Web Portal"),
					hint: ResString.GetMultilingualString("65cbd6a9-2436-45e9-88ca-d41014581fd0", "Specifies whether the Selection Criterion 5 field is shown on the Customer Service Tickets web portal."),
					storage: RegistryStorageFlags.System,
					defaultValue: true
					)
			);

		#endregion

		#endregion

		#region Notification Group

		public GuidRegistryItem CustomerServiceTicketNotificationGroup
		{
			get
			{
				return GetItem("CustomerServiceTicketNotificationGroup", () =>
					new GuidRegistryItem(
						"CustomerServiceTicketNotificationGroup",
						RawDataRegistry.Categories.Notification,
						ResString.GetMultilingualString("3dc9bc26-94f7-4965-a6e4-41330f2ef74f", "Customer Service Ticket Notification Group"),
						ResString.GetMultilingualString("64a49a18-4896-4eb2-a201-6ed6c610ab73", "The staff group that will receive notifications when configured to do so through Workflow triggers using the {0} Recipient.", MessageRecipientPartyTypeList.Codes.NotificationGroup),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
						RegistryOptions.CannotCallParameterlessValueGetter, // This registry item is designed to use context provided by the ticket, not the currently logged-in branch/department.
						RegistryFactory.Instance.GetGroupPK(GlbGroup.PostMastersGroupCode)
					)
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup)
					}
				);
			}
		}

		#endregion

		#region Email Recipient Fallback

		public RecipientSourceFallbackRegistryItem RecipientDeterminationFallbackForNonSubscribedCSTickets => GetItem("RecipientDeterminationFallbackForNonSubscribedCSTickets", () =>
			new RecipientSourceFallbackRegistryItem(
				name: "RecipientDeterminationFallbackForNonSubscribedCSTickets",
				category: Categories.ProductivityTools_CustomerServiceTickets,
				caption: ResString.GetMultilingualString("1e504a5a-813a-4f73-8fa8-661c4cf078fd", "Email Recipient Determination for Non-Subscribed Customer Service Tickets"),
				hint: ResString.GetMultilingualString("53fff25c-c4a4-4351-a7f6-99643d4b014e", "Specify the logic for determining to whom emails should be addressed when eConversation messages are received from clients for Customer Service Tickets which have no eConversation participants. Recipients are determined by finding the first valid recipients using the sources specified in this list (in sequence order, smallest to largest)."),
				storage: RegistryStorageFlags.System
				));

		#endregion

		#endregion

		#region Jira Integration

		#region Connection

		public CodeDescriptionPairListRegistryItem JiraSiteUrls
		{
			get
			{
				return GetItem("JiraSiteUrls", () =>
					new CodeDescriptionPairListRegistryItem(new RegistryItemImpl(
						name: "JiraSiteUrls",
						category: Categories.ProductivityTools_Project_JiraIntegration,
						caption: ResString.GetMultilingualString("2d05cfa0-0c1a-49eb-a1cb-41be6aeb37b7", "Jira Site URLs"),
						hint: ResString.GetMultilingualString("01e459ff-aae4-4e26-b512-723eb0c8d2a3", "The URL for each Jira system to which {0} will be integrated. 'System Code' is any unique 3-character code that will be used to identify the Jira system associated with the specified URL. Please note that the System Code cannot be changed once items are imported from the specified system.", Core.Constants.ProductName),
						dataType: new JiraSiteUrlsRegistryDataType(codeMaxLength: 3) { AllowDuplicateCodes = false, AllowDuplicateDescriptions = false, AllowEmptyCodes = false, AllowEmptyDescriptions = false },
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.MustOverrideDefaultValue,
						defaultValue: new CodeDescriptionPairList(),
						editorInfo: new CodeDescriptionPairListEditorInfo(
							showCodeColumn: true,
							showDescriptionColumn: true,
							codeFieldCasing: CodeDescriptionPairListEditorInfo.CharacterCasing.Upper,
							descriptionFieldCasing: CodeDescriptionPairListEditorInfo.CharacterCasing.Normal,
							codeColumnCaption: ResString.GetMultilingualString("206ce8bf-c0e5-4e78-aa15-45bf0655cf90", "System Code"),
							descriptionColumnCaption: ResString.GetMultilingualString("bc70db87-106e-427a-b72d-a79661cd9abd", "URL (https://)"))
					), isLocalizable: false, allDefaultValues: new CodeDescriptionPairList()));
			}
		}

		#endregion

		#region Mapping

		#region Link Type Mapping

		public StringArrayRegistryItem DependencyLinkTypeMapping
		{
			get
			{
				return GetItem("DependencyLinkTypeMapping", () =>
					new StringArrayRegistryItem(
						name: "DependencyLinkTypeMapping",
						category: Categories.ProductivityTools_Project_JiraIntegration_DataMapping_LinkType,
						caption: ResString.GetMultilingualString("90f49669-c8a9-438b-b795-8dd182b5c819", "Dependency Link Type Mapping"),
						hint: ResString.GetMultilingualString("56f659d8-070a-4bb9-8004-0e2ebd0ab446", "The Jira Issue Link types that will become Dependency Links between Work Items when imported."),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.Default));
			}
		}

		public StringArrayRegistryItem ParentChildLinkTypeMapping
		{
			get
			{
				return GetItem("ParentChildLinkTypeMapping", () =>
					new StringArrayRegistryItem(
						name: "ParentChildLinkTypeMapping",
						category: Categories.ProductivityTools_Project_JiraIntegration_DataMapping_LinkType,
						caption: ResString.GetMultilingualString("1e325aa7-fc8c-4314-a215-e9ca42fe890c", "Parent/Child Link Type Mapping"),
						hint: ResString.GetMultilingualString("d7746c9d-49ea-4dbc-ba7d-639133f6b436", "The Jira Issue Link types that will become Parent/Child Links between Work Items when imported."),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.Default));
			}
		}

		#endregion

		#region Issue Status Mapping

		public StringArrayRegistryItem CompletedIssueStatusTypesMapping
		{
			get
			{
				return GetItem("CompletedIssueStatusTypesMapping", () =>
					new StringArrayRegistryItem(
						name: "CompletedIssueStatusTypesMapping",
						category: Categories.ProductivityTools_Project_JiraIntegration_DataMapping_IssueStatus,
						caption: ResString.GetMultilingualString("42c7cdfb-86cd-4ac7-8032-6e955730933b", "Completed Issue Status Types Mapping"),
						hint: ResString.GetMultilingualString("5c71eb12-48d6-4b0c-911c-0922a2c0f6df", "The Jira Issue Status types that represent completed issues. This can be used to filter the issues that are imported from Jira. Note that this Registry Item uses the Jira Status, not the Jira Status Category."),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.Default));
			}
		}

		public StringArrayRegistryItem WorkInProgressIssueStatusTypesMapping
		{
			get
			{
				return GetItem("WorkInProgressIssueStatusTypesMapping", () =>
					new StringArrayRegistryItem(
						name: "WorkInProgressIssueStatusTypesMapping",
						category: Categories.ProductivityTools_Project_JiraIntegration_DataMapping_IssueStatus,
						caption: ResString.GetMultilingualString("40e1fb51-f873-43eb-89e9-aa7071d05185", "Work-in-Progress Issue Status Types Mapping"),
						hint: ResString.GetMultilingualString("567fd88a-bf9f-479e-a817-2080169b592e", "The Jira Issue Status types that represent work-in-progress issues. This can be used to filter the issues that are imported from Jira. Note that this Registry Item uses the Jira Status, not the Jira Status Category."),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.Default));
			}
		}

		#endregion

		public IssueTypeMappingRegistryItem JiraIssueTypesMapping
		{
			get
			{
				return GetItem("JiraIssueTypesMapping", () =>
					new IssueTypeMappingRegistryItem(
						name: "JiraIssueTypesMapping",
						category: Categories.ProductivityTools_Project_JiraIntegration_DataMapping,
						caption: ResString.GetMultilingualString("d764fe2b-607f-4e79-a460-c343b0635780", "Issue Types Mapping"),
						hint: ResString.GetMultilingualString("b931d320-7971-4dc5-8c24-f6216909d3d7", "Specify the field and value that will be set on imported Work Items which correspond to Jira Issue Types."),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.Default,
						defaultValue: new IssueTypeMap()));
			}
		}

		public JiraCustomFieldMappingRegistryItem JiraCustomFieldsMapping
		{
			get
			{
				return GetItem(nameof(JiraCustomFieldsMapping), () =>
					new JiraCustomFieldMappingRegistryItem(
						name: nameof(JiraCustomFieldsMapping),
						category: Categories.ProductivityTools_Project_JiraIntegration_DataMapping,
						caption: ResString.GetMultilingualString("9AC7D3D7-B040-423A-A426-02CC23681A55", "Jira Custom Fields"),
						hint: ResString.GetMultilingualString("291E0941-3284-4AE1-BE2E-D51782D62B28", "Specify the field and value that will be set on imported Work Items which correspond to Jira Custom Fields."),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.Default,
						defaultValue: new JiraCustomFieldMap()));
			}
		}

		public ProjectCategoryMappingRegistryItem JiraProjectCategoriesMapping
		{
			get
			{
				return GetItem("JiraProjectCategoriesMapping", () =>
					new ProjectCategoryMappingRegistryItem(
						name: "JiraProjectCategoriesMapping",
						category: Categories.ProductivityTools_Project_JiraIntegration_DataMapping,
						caption: ResString.GetMultilingualString("0c7e2605-191b-4906-a6b9-cee2d97f3851", "Project Categories Mapping"),
						hint: ResString.GetMultilingualString("0a5c27fe-7379-40eb-8b53-c7d42aca0e26", "Specify the field and value that will be set on imported Projects which correspond to Jira Project categories."),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.Default,
						defaultValue: new ProjectCategoryMap()));
			}
		}

		#endregion

		#region Import

		public IntRegistryItem JiraIssueImportBatchSize
		{
			get
			{
				return GetItem("JiraIssueImportBatchSize", () =>
					new IntRegistryItem(
						name: "JiraIssueImportBatchSize",
						category: Categories.ProductivityTools_Project_JiraIntegration,
						caption: ResString.GetMultilingualString("6812bef5-17e9-45ad-ad17-caa50f06f297", "Work Item Creation Batch Size"),
						hint: ResString.GetMultilingualString("c52f0fff-7dd2-41ed-9402-ec38d8c7a32f", "The number of Work Items to create per batch when importing issues for Jira projects."),
						RegistryStorageFlags.System,
						options: RegistryOptions.Default,
						defaultValue: 500,
						minValue: 50,
						maxValue: 2000
						));
			}
		}

		#endregion

		#endregion

		#region Common Strings

		static ResourceString SelectionCriterion1CaptionString => ResString.GetMultilingualString("A212689A-3DD6-4915-8021-6DB80E6E69A7", "Selection Criterion 1 Caption");
		static ResourceString SelectionCriterion2CaptionString => ResString.GetMultilingualString("550C4400-2C62-474A-B942-E8698F9727FD", "Selection Criterion 2 Caption");
		static ResourceString SelectionCriterion3CaptionString => ResString.GetMultilingualString("5110964B-C58A-4EBD-9699-DC039837A713", "Selection Criterion 3 Caption");
		static ResourceString SelectionCriterion4CaptionString => ResString.GetMultilingualString("A69A0F3E-AB25-4933-855F-994859ACD7AC", "Selection Criterion 4 Caption");
		static ResourceString SelectionCriterion5CaptionString => ResString.GetMultilingualString("8F916231-6CA7-4C46-A81B-A67DFC4DAE16", "Selection Criterion 5 Caption");

		static ResourceString SelectionCriterion1MandatoryCaption => ResString.GetMultilingualString("aad1b088-08cc-4681-a401-975e532d6532", "Selection Criterion 1 Mandatory");
		static ResourceString SelectionCriterion2MandatoryCaption => ResString.GetMultilingualString("d31eaef4-fc55-49d0-a756-c09d7a9e3980", "Selection Criterion 2 Mandatory");
		static ResourceString SelectionCriterion3MandatoryCaption => ResString.GetMultilingualString("2929143f-fc2e-489c-a912-1d51f57ff27f", "Selection Criterion 3 Mandatory");
		static ResourceString SelectionCriterion4MandatoryCaption => ResString.GetMultilingualString("574b8dee-6648-4523-80e7-47286e7fd78d", "Selection Criterion 4 Mandatory");
		static ResourceString SelectionCriterion5MandatoryCaption => ResString.GetMultilingualString("9a155346-576f-4794-883d-d796434e8ada", "Selection Criterion 5 Mandatory");

		static ResourceString SelectionCriterion1MandatoryHint => ResString.GetMultilingualString("d47bcd67-4739-48cf-a338-72ba89c53514", "Determines whether the Selection Criterion 1 field is mandatory.");
		static ResourceString SelectionCriterion2MandatoryHint => ResString.GetMultilingualString("8de5190c-5ad6-4f30-bafd-8fdbb98c610f", "Determines whether the Selection Criterion 2 field is mandatory.");
		static ResourceString SelectionCriterion3MandatoryHint => ResString.GetMultilingualString("2b89288e-eded-4859-9e77-72709eceb00a", "Determines whether the Selection Criterion 3 field is mandatory.");
		static ResourceString SelectionCriterion4MandatoryHint => ResString.GetMultilingualString("20fd8e69-fb5e-4cb0-8e2f-a98b53aae927", "Determines whether the Selection Criterion 4 field is mandatory.");
		static ResourceString SelectionCriterion5MandatoryHint => ResString.GetMultilingualString("77b9465f-435c-45a8-bbed-222bb468fb5b", "Determines whether the Selection Criterion 5 field is mandatory.");

		static ResourceString SelectionCriterion1ValuesString => ResString.GetMultilingualString("ceb21013-203b-4696-90fc-045efe0a594f", "Selection Criterion 1 Values");
		static ResourceString SelectionCriterion2ValuesString => ResString.GetMultilingualString("3040dbd7-1854-4db0-ae8a-030c129bc72d", "Selection Criterion 2 Values");
		static ResourceString SelectionCriterion3ValuesString => ResString.GetMultilingualString("b4b77fd2-b0e0-4ac2-8624-5f77adb69d0e", "Selection Criterion 3 Values");
		static ResourceString SelectionCriterion4ValuesString => ResString.GetMultilingualString("e37f9c99-a35c-409f-85ff-5f77fc8caee2", "Selection Criterion 4 Values");
		static ResourceString SelectionCriterion5ValuesString => ResString.GetMultilingualString("9091a1e6-a00e-459d-bfd1-20d01b1bc30b", "Selection Criterion 5 Values");

		static ResourceString AllSelectionCriterion1Values => ResString.GetMultilingualString("c683ba64-0776-4796-9837-41166b56c3c9", "All Selection Criterion 1 Values");
		static ResourceString AllSelectionCriterion2Values => ResString.GetMultilingualString("854a0cbf-2023-40d2-bb4b-78beeadc1f72", "All Selection Criterion 2 Values");
		static ResourceString AllSelectionCriterion3Values => ResString.GetMultilingualString("31f84ad7-6398-4c31-99d2-8ff4688e1850", "All Selection Criterion 3 Values");
		static ResourceString AllSelectionCriterion4Values => ResString.GetMultilingualString("bab49424-dab5-40d0-8f55-9c59bc39c7ab", "All Selection Criterion 4 Values");

		static ResourceString SelectionCriteriaValuesCaption => ResString.GetMultilingualString("4bb0f2c3-2f53-44bd-9dde-9a8494950ec4", "Selection Criteria Values");
		static ResourceString SelectionCriteriaValuesHint => ResString.GetMultilingualString("8e196b64-4c34-4816-a071-8924bbe2da67", "Defines a list of values that can be selected in the relevant Selection Criteria fields. The values can be filtered to be available hierarchically. For example, the values shown in Selection Criterion 2 can be different depending on the value chosen in Selection Criterion 1. Alternatively, the values can be the same regardless of the other Selection Criteria values by leaving the 'All Selection Criterion * Values' row selected.");

		static ResourceString WorkItemSelectionCriteriaHint => ResString.GetMultilingualString("77a94c32-9795-4088-80ba-9c31b6be489d", "Allows you to further categorize your work items, by an identifier with customizable label.");
		static ResourceString ProjectSelectionCriteriaHint => ResString.GetMultilingualString("1d2a615c-8901-4772-a2ab-bf05a0e6a1ed", "Allows you to further categorize your projects, by an identifier with customizable label.");

		static ResourceString ActiveString => ResString.GetMultilingualString("916EF446-DFDA-4D57-9180-997B5E14CDB2", "Active");

		#endregion
	}
}
