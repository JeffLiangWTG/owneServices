using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business
{
	public class MatchingTemplateView : NonPersistentBusinessObject
	{
		public MatchingTemplateView(IWorkflowItemCollection workflowProvider)
		{
			this.workflowProvider = Argument.NotNull(workflowProvider, nameof(workflowProvider));
		}

		readonly IWorkflowItemCollection workflowProvider;
		MatchingTemplateViewLineCollection templates;

		public MatchingTemplateViewLineCollection MatchingTemplates => templates ?? (templates = GetMatchingTemplates());

		public ZString Description => Res.GetString("MatchingTemplateView.Description", "These are the templates that currently match the {0} collection.", GetCollectionDescription(workflowProvider.TemplateEntityType));

		static ZString GetCollectionDescription(TemplateEntityType type)
		{
			switch (type)
			{
				case TemplateEntityType.Tasks:
					return Res.GetString("MatchingTemplateView.CollectionDescription.Task", "Tasks");
				case TemplateEntityType.Triggers:
					return Res.GetString("MatchingTemplateView.CollectionDescription.Triggers", "Triggers");
				case TemplateEntityType.Milestones:
					return Res.GetString("MatchingTemplateView.CollectionDescription.Milestones", "Milestones");
				default:
					throw new InvalidOperationException("Unsupported Type: " + type);
			}
		}

		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));

		MatchingTemplateViewLineCollection GetMatchingTemplates()
		{
			bool ContainsNonUdfTasks(ProcessTaskTemplate template, IWorkflowItemCollection workflowItems) => workflowItems.Items.Any(t => t.TemplateConditions.TemplateCondition1.IsEmpty && t.TemplateConditions.TemplateCondition2.IsEmpty);
			var collection = new MatchingTemplateViewLineCollection();
			var index = 1;
			var scopeEnforcer = workflowProvider.GetWorkflowTemplateScopeEnforcer();
			foreach (var itr in new TemplateFallbackIterator(new ProcessTaskTemplate.Loader(workflowProvider.WorkflowItems.Factory).FindMatches(workflowProvider).Where(template => scopeEnforcer.ShouldApplyTemplate(workflowProvider, template, workflowProvider.WorkflowItems.Factory)), workflowProvider.TemplateEntityType, ContainsNonUdfTasks))
			{
				collection.Add(new MatchingTemplateViewLine(itr.Template, itr.FallbackMethod, itr.Status, index++));
			}

			return collection;
		}
	}

	public class MatchingTemplateViewLine : NonPersistentBusinessObject
	{
		public MatchingTemplateViewLine(ProcessTaskTemplate template, string fallbackCode, string matchingWorkflowTemplateStatusCode, int applicationOrder)
			: base(template.Factory)
		{
			Template = Argument.NotNull(template, nameof(template));
			TemplateName = template.P0_Name;
			TemplateDescription = template.P0_Description;
			TemplateFallback = Argument.NotNull(fallbackCode, nameof(fallbackCode));
			this.matchingWorkflowTemplateStatusCode = Argument.NotNull(matchingWorkflowTemplateStatusCode, nameof(matchingWorkflowTemplateStatusCode));
			MatchOrder = applicationOrder;
		}

		readonly string matchingWorkflowTemplateStatusCode;

		#region Properties

		public ProcessTaskTemplate Template { get; }

		[ReadOnly(true)]
		[ResourceStringData("MatchingTemplateViewLine.TemplateName", Caption = "Template Name")]
		public ZString TemplateName { get; }
		public ZPropertyInfo TemplateNameInfo => GetZPropertyInfo(nameof(TemplateName));

		[ReadOnly(true)]
		[ResourceStringData("MatchingTemplateViewLine.Description", Caption = "Template Description")]
		public ZString TemplateDescription { get; }
		public ZPropertyInfo TemplateDescriptionInfo => GetZPropertyInfo(nameof(TemplateDescription));

		[ReadOnly(true)]
		[ResourceStringData("MatchingTemplateViewLine.Fallback", Caption = "Fallback Type")]
		public ZString TemplateFallback { get; }
		public ZPropertyInfo TemplateFallbackInfo => GetZPropertyInfo(nameof(TemplateFallback));

		[ReadOnly(true)]
		[ResourceStringData("MatchingTemplateViewLine.MatchOrder", Caption = "Match Order")]
		public ZInt MatchOrder { get; }
		public ZPropertyInfo MatchOrderInfo => GetZPropertyInfo(nameof(MatchOrder));

		[ReadOnly(true)]
		[ResourceStringData("MatchingTemplateViewLine.MatchDescription", Caption = "Status")]
		public ZString MatchDescription => new MatchingWorkflowTemplateStatusCodeList().GetDescriptionFromCode(matchingWorkflowTemplateStatusCode);
		public ZPropertyInfo MatchDescriptionInfo => GetZPropertyInfo(nameof(MatchDescription));

		#endregion
	}

	public class MatchingTemplateViewLineCollection : NonPersistentBusinessObjectCollection<MatchingTemplateViewLine>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new InvalidOperationException("This template is not editable");
		public override bool ReadOnly => true;
		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
	}
}
