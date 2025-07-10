using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Workflow.Business
{
	public class ReapplyWorkflowTemplateUserOptions : NonPersistentBusinessObject
	{
		public ReapplyWorkflowTemplateUserOptions(IReapplyWorkflowTemplateConfiguration config, ZString reapplyWorkflowAndTasksOptions, ZString reapplyMilestones, ZString reapplyTriggers, bool releaseGroups)
			: this(config)
		{
			ReapplyWorkflowAndTasksOptions = reapplyWorkflowAndTasksOptions;
			ReapplyMilestonesOptions = reapplyMilestones;
			ReapplyTriggersOptions = reapplyTriggers;
			ReCalculateReleaseGroups = releaseGroups;
		}

		public ReapplyWorkflowTemplateUserOptions(IReapplyWorkflowTemplateConfiguration config)
		{
			Argument.NotNull(config, nameof(config));
			RemoveUnsupportedOptions(config);
			SetDefaultValuesIfEmpty(config);
			Configuration = config;
		}

		#region Properties

		[ResourceStringData("ReapplyWorkflowTemplateUserOptions|ReapplyWorkflowAndTasksOptions", Caption = "Workflows/Tasks")]
		[List(nameof(ReapplyWorkflowAndTasksOptionsLookup))]
		public ZString ReapplyWorkflowAndTasksOptions
		{
			get { return fReapplyWorkflowAndTasksOptions; }
			set
			{
				SetNonPersistentPropertyValue(ReapplyWorkflowAndTasksOptionsInfo, ref fReapplyWorkflowAndTasksOptions, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateReapplyWorkflowAndTasksOptions();
				}
			}
		}
		ZString fReapplyWorkflowAndTasksOptions;

		public ZPropertyInfo ReapplyWorkflowAndTasksOptionsInfo => GetZPropertyInfo(nameof(ReapplyWorkflowAndTasksOptions));

		[ResourceStringData("ReapplyWorkflowTemplateUserOptions|ReapplyMilestonesOptions", Caption = "Milestones")]
		[List(nameof(ReapplyMilestonesOptionsLookup))]
		public ZString ReapplyMilestonesOptions
		{
			get { return fReapplyMilestonesOptions; }
			set
			{
				SetNonPersistentPropertyValue(ReapplyMilestonesOptionsInfo, ref fReapplyMilestonesOptions, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateReapplyMilestonesOptions();
				}
			}
		}
		ZString fReapplyMilestonesOptions;

		public ZPropertyInfo ReapplyMilestonesOptionsInfo => GetZPropertyInfo(nameof(ReapplyMilestonesOptions));

		[ResourceStringData("ReapplyWorkflowTemplateUserOptions|ReapplyTriggersOptions", Caption = "Triggers")]
		[List(nameof(ReapplyTriggersOptionsLookup))]
		public ZString ReapplyTriggersOptions
		{
			get { return fReapplyTriggersOptions; }
			set
			{
				SetNonPersistentPropertyValue(ReapplyTriggersOptionsInfo, ref fReapplyTriggersOptions, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateReapplyTriggersOptions();
				}
			}
		}
		ZString fReapplyTriggersOptions;

		public ZPropertyInfo ReapplyTriggersOptionsInfo => GetZPropertyInfo(nameof(ReapplyTriggersOptions));

		public ZBool ReCalculateReleaseGroups { get; set; }

		public bool ShouldPerformReapplication { get; set; }

		public IReapplyWorkflowTemplateConfiguration Configuration { get; }

		#endregion

		public ZBool HasAtLeastOnOptionSelected()
		{
			return ReapplyWorkflowAndTasksOptions != ReapplyWorkflowAndTasksOptionsList.Codes.Exclude
				|| ReapplyMilestonesOptions != ReapplyMilestonesOptionsList.Codes.Exclude
				|| ReapplyTriggersOptions != ReapplyTriggersOptionsList.Codes.Exclude
				|| ReCalculateReleaseGroups;
		}

		public CodeDescriptionPairList ReapplyWorkflowAndTasksOptionsLookup { get; } = new ReapplyWorkflowAndTasksOptionsList();
		public CodeDescriptionPairList ReapplyMilestonesOptionsLookup { get; } = new ReapplyMilestonesOptionsList();
		public CodeDescriptionPairList ReapplyTriggersOptionsLookup { get; } = new ReapplyTriggersOptionsList();

		void RemoveUnsupportedOptions(IReapplyWorkflowTemplateConfiguration config)
		{
			if (config.DelayReapplyTemplatesToServiceTask)
			{
				//Not currently supported when config.DelayReapplyTemplatesToServiceTask
				ReapplyWorkflowAndTasksOptionsLookup.RemoveCode(ReapplyWorkflowAndTasksOptionsList.Codes.KeepsExistingAndReapply);
				ReapplyWorkflowAndTasksOptionsLookup.RemoveCode(ReapplyWorkflowAndTasksOptionsList.Codes.DeleteUnactionedAndReapply);
			}
		}

		void SetDefaultValuesIfEmpty(IReapplyWorkflowTemplateConfiguration config)
		{
			using (SuspendSettingHasChanges())
			{
				if (ReapplyWorkflowAndTasksOptions.IsEmpty)
				{
					if (config.DelayReapplyTemplatesToServiceTask)
					{
						ReapplyWorkflowAndTasksOptions = ReapplyWorkflowAndTasksOptionsList.Codes.Exclude;
					}
					else
					{
						ReapplyWorkflowAndTasksOptions = ReapplyWorkflowAndTasksOptionsList.Codes.KeepsExistingAndReapply;
					}
				}

				if (ReapplyMilestonesOptions.IsEmpty)
				{
					ReapplyMilestonesOptions = ReapplyMilestonesOptionsList.Codes.Exclude;
				}

				if (ReapplyTriggersOptions.IsEmpty)
				{
					ReapplyTriggersOptions = ReapplyTriggersOptionsList.Codes.Exclude;
				}
			}
		}

		#region Validation

		protected ReapplyWorkflowTemplateUserOptionsValidation Validation
		{
			get { return new ReapplyWorkflowTemplateUserOptionsValidation(this); }
		}

		#endregion
	}
}
