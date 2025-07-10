using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.Business
{
	public class TemplateConditionsViewModel : NonPersistentBusinessObject<TemplateConditionsViewModelValidation>, ITemplateConditional, IAntlrMacroContextProvider
	{
		public TemplateConditionsViewModel(ITemplateConditionalWorkflowItem workflowItem, ProcessTaskTemplate template)
			: base(workflowItem.Factory)
		{
			Argument.NotNull(workflowItem, nameof(workflowItem));

			WorkflowItem = workflowItem;
			Template = template;

			ConditionValueStyleDecider = new DefaultTemplateConditionValueStyleDecider(template);
		}

		internal ITemplateConditionalWorkflowItem WorkflowItem { get; }

		internal ProcessTaskTemplate Template { get; }

		#region Schema

		public static class Schema
		{
			public const string TemplateCondition1 = nameof(TemplateConditionsViewModel.TemplateCondition1);
			public const string TemplateCondition2 = nameof(TemplateConditionsViewModel.TemplateCondition2);
			public const string TemplateCondition2Value = nameof(TemplateConditionsViewModel.TemplateCondition2Value);
			public const string TemplateCondition2ValueFieldType = nameof(TemplateConditionsViewModel.TemplateCondition2ValueFieldType);
			public const string OriginCountryCode = nameof(TemplateConditionsViewModel.OriginCountryCode);
			public const string DestinationCountryCode = nameof(TemplateConditionsViewModel.DestinationCountryCode);
		}

		#endregion

		#region Properties

		#region TemplateCondition1

		[MaxLength(3)]
		[List("Lookups.TemplateCondition1List")]
		[ResourceStringData("TemplateConditionsViewModel.TemplateCondition1", Caption = "Template Condition 1", ShortCaption = "Templ. Cond. 1", FullDescription = "The first condition which restricts whether this item is applicable to a job.")]
		public ZString TemplateCondition1
		{
			get
			{
				return WorkflowItem.TemplateCondition1;
			}
			set
			{
				if (WorkflowItem.TemplateCondition1 != value)
				{
					WorkflowItem.TemplateCondition1 = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateTemplateCondition1();
					}
				}

				TemplateCondition1Info.RefreshBinding();
			}
		}

		public ZPropertyInfo TemplateCondition1Info
		{
			get { return GetZPropertyInfo(Schema.TemplateCondition1); }
		}

		#endregion

		#region TemplateCondition2

		[MaxLength(3)]
		[List("Lookups.TemplateCondition2List")]
		[ResourceStringData("TemplateConditionsViewModel.TemplateCondition2", Caption = "Template Condition 2", ShortCaption = "Templ. Cond. 2", FullDescription = "The second condition which restricts whether this item is applicable to a job.")]
		public ZString TemplateCondition2
		{
			get
			{
				return WorkflowItem.TemplateCondition2;
			}
			set
			{
				WorkflowItem.TemplateCondition2 = value;

				if (Condition2ValueStyle == TemplateConditionValueStyle.Unused && !TemplateCondition2Value.IsEmpty)
				{
					TemplateCondition2Value = ZString.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateTemplateCondition2();
				}

				TemplateCondition2Info.RefreshBinding();
				TemplateCondition2ValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TemplateCondition2Info
		{
			get { return GetZPropertyInfo(Schema.TemplateCondition2); }
		}

		#endregion

		#region TemplateCondition2Value

		[ReadOnlyMember(nameof(TemplateCondition2Value_ReadOnly))]
		[List("Lookups.TemplateCondition2ValueList")]
		[ResourceStringData("TemplateConditionsViewModel.TemplateCondition2Value", Caption = "Template Condition 2 Value", ShortCaption = "Templ. Cond. 2 Value", FullDescription = "The value used in conjunction with the second condition restricting whether this item is applicable to a job.")]
		public ZString TemplateCondition2Value
		{
			get { return WorkflowItem.TemplateCondition2Value; }
			set
			{
				WorkflowItem.TemplateCondition2Value = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateTemplateCondition2Value();
				}

				TemplateCondition2ValueInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TemplateCondition2ValueInfo
		{
			get { return GetZPropertyInfo(Schema.TemplateCondition2Value); }
		}

		protected bool TemplateCondition2Value_ReadOnly
		{
			get { return Condition2ValueStyle == TemplateConditionValueStyle.Unused; }
		}

		public int TemplateCondition2Value_MaxLength
		{
			get { return ProcessTasksLookups.IsMacroCondition(TemplateCondition2) ? int.MaxValue : 3; }
		}

		#endregion

		#region TemplateCondition2ValueFieldType

		[ResourceStringData("TemplateConditionsViewModel.TemplateCondition2ValueFieldType", Caption = "Condition 2 Value Field Type")]
		public ZString TemplateCondition2ValueFieldType
		{
			get
			{
				switch (Condition2ValueStyle)
				{
					case TemplateConditionValueStyle.DropDown:
						return nameof(FieldType.TextDropEdit);

					case TemplateConditionValueStyle.UDFMacro:
						return nameof(FieldType.TextMacro);

					case TemplateConditionValueStyle.MCRMacro:
						return nameof(FieldType.AntlrMacro);

					default:
						return nameof(FieldType.Text);
				}
			}
		}

		internal TemplateConditionValueStyle Condition2ValueStyle
		{
			get
			{
				switch (TemplateCondition2)
				{
					case ProcessTasksLookups.UserDefinedCondition:
						return TemplateConditionValueStyle.UDFMacro;

					case ProcessTasksLookups.MacroCondition:
						return TemplateConditionValueStyle.MCRMacro;

					default:
						return ConditionValueStyleDecider.Decide(WorkflowItem);
				}
			}
		}

		public ZPropertyInfo TemplateCondition2ValueFieldTypeInfo
		{
			get { return GetZPropertyInfo(Schema.TemplateCondition2ValueFieldType); }
		}

		public ITemplateConditionValueStyleDecider ConditionValueStyleDecider { get; set; }

		#endregion

		#region OriginCountryCode

		[MaxLength(2)]
		[List("Lookups.OriginCountries")]
		[ResourceStringData("TemplateConditionsViewModel.OriginCountryCode", Caption = "Export Country/Region", ShortCaption = "Exp. Country/Region", FullDescription = "The country/region of export the job must belong to.\r\nThe current Task Template will only be used for jobs that export from that country/region.")]
		public ZString OriginCountryCode
		{
			get { return WorkflowItem.OriginCountryCode; }
			set
			{
				WorkflowItem.OriginCountryCode = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateOriginCountryCode();
				}

				OriginCountryCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo OriginCountryCodeInfo => GetZPropertyInfo(Schema.OriginCountryCode);

		#endregion

		#region DestinationCountryCode

		[MaxLength(2)]
		[List("Lookups.DestinationCountries")]
		[ResourceStringData("TemplateConditionsViewModel.DestinationCountryCode", Caption = "Import Country/Region", ShortCaption = "Imp. Country/Region", FullDescription = "The country/region of import the job must belong to.\r\nThe current Task Template will only be used for jobs that import from that country/region.")]
		public ZString DestinationCountryCode
		{
			get { return WorkflowItem.DestinationCountryCode; }
			set
			{
				WorkflowItem.DestinationCountryCode = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateDestinationCountryCode();
				}

				DestinationCountryCodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DestinationCountryCodeInfo => GetZPropertyInfo(Schema.DestinationCountryCode);

		#endregion

		#endregion

		#region Lookups

		public TemplateConditionsViewModelLookups Lookups
		{
			get
			{
				if (lookups == null || !IsLookupsCachedInBase)
				{
					lookups = new TemplateConditionsViewModelLookups(this);
				}

				return lookups;
			}
		}

		TemplateConditionsViewModelLookups lookups;

		#endregion

		#region Validation

		public override TemplateConditionsViewModelValidation GetNewValidation()
		{
			return new TemplateConditionsViewModelValidation(this);
		}

		#endregion

		IAntlrMacroContext IAntlrMacroContextProvider.GetSampleContext() => ObjectFactory.Get<IWorkflowMacroContextDecider>().GetContextForTemplateConditions(Factory, Template?.WorkflowDescriptor?.WorkflowProviderType, null, WorkflowItem);
	}
}
