using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTemplateValidationCollection : ActiveBusinessObjectCollection<ProcessTemplateValidation>
	{
		public ProcessTemplateValidationCollection(BusinessObjectFactory factory) : base(factory)
		{
			ProcessTaskTemplates = Array.Empty<ProcessTaskTemplate>();
		}

		public ProcessTemplateValidationCollection(ProcessTaskTemplate parent) : base(parent.Factory, parent, new ZQuery(), ProcessTemplateValidationSchema.P0V_P0_WorkflowTemplate)
		{
			ProcessTaskTemplates = [parent];
		}

		public ProcessTemplateValidationCollection(IWorkflowProvider workflowProvider) : this(new ValidationToolLoader((IBusiness)workflowProvider))
		{
		}

		ProcessTemplateValidationCollection(ValidationToolLoader loader) : base(loader.Factory, GetJobAppliedFilter(loader))
		{
			ProcessTaskTemplates = loader.MatchedTemplates;
		}

		static ZQuery GetJobAppliedFilter(ValidationToolLoader validationToolLoader)
		{
			var appliedRulePKs = validationToolLoader
				.MatchedRules
				.Select(x => x.PK)
				.ToArray();

			return new ZQuery(ProcessTemplateValidationSchema.PK, appliedRulePKs);
		}

		public IReadOnlyList<ProcessTaskTemplate> ProcessTaskTemplates { get; }

		protected override void SetDefaultsForNewElementCore(ProcessTemplateValidation newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.P0V_Condition1 = newElement.WorkflowDescriptor?.ValidationToolSettings.GetProcessTemplateValidationDefaultCondition1() ?? ZString.Empty;
		}
	}
}
