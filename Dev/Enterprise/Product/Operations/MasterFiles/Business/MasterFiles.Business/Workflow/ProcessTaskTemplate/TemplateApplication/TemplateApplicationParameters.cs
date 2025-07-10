using Enterprise.MasterFiles.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[Immutable]
	public sealed class TemplateApplicationParameters : IWorkflowTemplateApplicationParameters
	{
		const ProcessTaskTemplate[] SpecificTemplatesToApply_DefaultValue = null;
		const TemplateEntityType EntityTypeFlagsToApply_DefaultValue = TemplateEntityType.All;
		const bool AlwaysApplyTemplatesEvenWhenExistingItemsPresent_DefaultValue = false;
		const bool JobAttributesMayHaveChangedSinceLastTemplateApplication_DefaultValue = false;
		const TriggerApplicationType TriggerApplicationType_DefaultValue = TriggerApplicationType.ApplyOnce;
		const bool IgnoreHasChanges_DefaultValue = false;
		const bool ReapplyProcessHeaders_DefaultValue = false;

		public static TemplateApplicationParameters ApplySpecificTemplates(ProcessTaskTemplate[] specificTemplatesToApply, bool jobAttributesMayHaveChangedSinceLastTemplateApplication = false, TriggerApplicationType triggerApplicationType = TriggerApplicationType.ApplyOnce, bool ignoreHasChanges = false)
		{
			return new TemplateApplicationParameters(
				specificTemplatesToApply: specificTemplatesToApply,
				entityTypeFlagsToApply: EntityTypeFlagsToApply_DefaultValue,
				alwaysApplyTemplatesEvenWhenExistingItemsPresent: true,
				jobAttributesMayHaveChangedSinceLastTemplateApplication: jobAttributesMayHaveChangedSinceLastTemplateApplication,
				triggerApplicationType: triggerApplicationType,
				ignoreHasChanges: ignoreHasChanges,
				reapplyProcessHeaders: ReapplyProcessHeaders_DefaultValue);
		}

		public static TemplateApplicationParameters ApplySpecificEntityTypes(TemplateEntityType entityTypeFlagsToApply, bool jobAttributesMayHaveChangedSinceLastTemplateApplication = false)
		{
			return new TemplateApplicationParameters(
				specificTemplatesToApply: SpecificTemplatesToApply_DefaultValue,
				entityTypeFlagsToApply: entityTypeFlagsToApply,
				alwaysApplyTemplatesEvenWhenExistingItemsPresent: AlwaysApplyTemplatesEvenWhenExistingItemsPresent_DefaultValue,
				jobAttributesMayHaveChangedSinceLastTemplateApplication: jobAttributesMayHaveChangedSinceLastTemplateApplication,
				triggerApplicationType: TriggerApplicationType_DefaultValue,
				ignoreHasChanges: IgnoreHasChanges_DefaultValue,
				reapplyProcessHeaders: ReapplyProcessHeaders_DefaultValue);
		}

		public static TemplateApplicationParameters ApplyIgnoreHasChanges(bool ignoreHasChanges = true)
		{
			return new TemplateApplicationParameters(
				specificTemplatesToApply: SpecificTemplatesToApply_DefaultValue,
				entityTypeFlagsToApply: EntityTypeFlagsToApply_DefaultValue,
				alwaysApplyTemplatesEvenWhenExistingItemsPresent: AlwaysApplyTemplatesEvenWhenExistingItemsPresent_DefaultValue,
				jobAttributesMayHaveChangedSinceLastTemplateApplication: JobAttributesMayHaveChangedSinceLastTemplateApplication_DefaultValue,
				triggerApplicationType: TriggerApplicationType_DefaultValue,
				ignoreHasChanges: ignoreHasChanges,
				reapplyProcessHeaders: ReapplyProcessHeaders_DefaultValue);
		}

		public static TemplateApplicationParameters ApplyIgnoreHasChangesToExistingParameters(TemplateApplicationParameters parameters)
		{
			return new TemplateApplicationParameters(
				specificTemplatesToApply: parameters.SpecificTemplatesToApply,
				entityTypeFlagsToApply: parameters.EntityTypeFlagsToApply,
				alwaysApplyTemplatesEvenWhenExistingItemsPresent: parameters.AlwaysApplyTemplatesEvenWhenExistingItemsPresent,
				jobAttributesMayHaveChangedSinceLastTemplateApplication: parameters.JobAttributesMayHaveChangedSinceLastTemplateApplication,
				triggerApplicationType: parameters.TriggerApplicationType,
				ignoreHasChanges: true,
				reapplyProcessHeaders: parameters.ReapplyProcessHeaders);
		}

		public static TemplateApplicationParameters ReapplyTemaplate()
		{
			return new TemplateApplicationParameters(
				specificTemplatesToApply: SpecificTemplatesToApply_DefaultValue,
				entityTypeFlagsToApply: EntityTypeFlagsToApply_DefaultValue,
				alwaysApplyTemplatesEvenWhenExistingItemsPresent: true,
				jobAttributesMayHaveChangedSinceLastTemplateApplication: JobAttributesMayHaveChangedSinceLastTemplateApplication_DefaultValue,
				triggerApplicationType: TriggerApplicationType.AlwaysApply,
				ignoreHasChanges: true,
				reapplyProcessHeaders: true);
		}

		public static TemplateApplicationParameters Default => ApplySpecificEntityTypes(EntityTypeFlagsToApply_DefaultValue);

		TemplateApplicationParameters(
			ProcessTaskTemplate[] specificTemplatesToApply,
			TemplateEntityType entityTypeFlagsToApply,
			bool alwaysApplyTemplatesEvenWhenExistingItemsPresent,
			bool jobAttributesMayHaveChangedSinceLastTemplateApplication,
			TriggerApplicationType triggerApplicationType,
			bool ignoreHasChanges,
			bool reapplyProcessHeaders)
		{
			SpecificTemplatesToApply = specificTemplatesToApply;
			EntityTypeFlagsToApply = entityTypeFlagsToApply;
			AlwaysApplyTemplatesEvenWhenExistingItemsPresent = alwaysApplyTemplatesEvenWhenExistingItemsPresent;
			JobAttributesMayHaveChangedSinceLastTemplateApplication = jobAttributesMayHaveChangedSinceLastTemplateApplication;
			TriggerApplicationType = triggerApplicationType;
			IgnoreHasChanges = ignoreHasChanges;
			ReapplyProcessHeaders = reapplyProcessHeaders;
		}

		public ProcessTaskTemplate[] SpecificTemplatesToApply { get; }
		public TemplateEntityType EntityTypeFlagsToApply { get; }
		public bool AlwaysApplyTemplatesEvenWhenExistingItemsPresent { get; }
		public bool JobAttributesMayHaveChangedSinceLastTemplateApplication { get; }
		public TriggerApplicationType TriggerApplicationType { get; }
		public bool IgnoreHasChanges { get; }
		public bool ReapplyProcessHeaders { get; }

		internal bool CanApply(TemplateEntityType type)
		{
			return EntityTypeFlagsToApply == TemplateEntityType.All || EntityTypeFlagsToApply.HasFlag(type);
		}
	}
}
