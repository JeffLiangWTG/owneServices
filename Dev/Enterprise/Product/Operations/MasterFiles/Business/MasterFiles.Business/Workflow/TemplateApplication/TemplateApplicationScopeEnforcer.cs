using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public sealed class TemplateApplicationScopeEnforcer : IService
	{
		public enum StrategyType
		{
			AlwaysTrue,
			AlwaysFalse,
			Binding,
			Message
		}

		abstract class Strategy
		{
			protected internal abstract bool ShouldApplyStrategy(IWorkflowProvider provider);

			protected internal abstract void TrackMember(ZGuid member);

			protected internal abstract StrategyType Type { get; }
		}

		class NullStrategy : Strategy
		{
			public NullStrategy(bool result)
			{
				this.result = result;
			}
			readonly bool result;

			protected internal override StrategyType Type => result ? StrategyType.AlwaysTrue : StrategyType.AlwaysFalse;

			protected internal override bool ShouldApplyStrategy(IWorkflowProvider provider) => result;

			protected internal override void TrackMember(ZGuid member)
			{
				// Do nothing.
			}
		}

		abstract class DefaultStrategy : Strategy
		{
			protected internal override bool ShouldApplyStrategy(IWorkflowProvider provider)
			{
				var parent = provider.WorkflowItems?.Parent;
				if (parent != null)
				{
					return parent.HasChanges
						|| !parent.IsInDatabase
						|| parent is IWorkflowProvider parentProvider && parentProvider.Logs.HasChanges;
				}
				return false;
			}
		}

		class MessageStrategy : DefaultStrategy
		{
			internal MessageStrategy()
			{
				keys = new HashSet<ZGuid>();
			}

			protected internal override void TrackMember(ZGuid member)
			{
				keys.Add(member);
			}

			protected internal override bool ShouldApplyStrategy(IWorkflowProvider provider)
			{
				return base.ShouldApplyStrategy(provider) || keys.Contains(provider.Identifier);
			}

			readonly HashSet<ZGuid> keys;
			protected internal override StrategyType Type => StrategyType.Message;
		}

		class FormStrategy : DefaultStrategy
		{
			protected internal override StrategyType Type => StrategyType.Binding;

			protected internal override bool ShouldApplyStrategy(IWorkflowProvider provider)
			{
				return base.ShouldApplyStrategy(provider) || (provider.WorkflowItems?.Parent?.IsBound ?? false);
			}

			protected internal override void TrackMember(ZGuid member)
			{
				// Do nothing.
			}
		}

		#region Types

		public static TemplateApplicationScopeEnforcer Default() => new TemplateApplicationScopeEnforcer(new NullStrategy(true));

		public static TemplateApplicationScopeEnforcer Null() => new TemplateApplicationScopeEnforcer(new NullStrategy(false));

		public static TemplateApplicationScopeEnforcer Message()
		{
			return new TemplateApplicationScopeEnforcer(new MessageStrategy());
		}

		public static TemplateApplicationScopeEnforcer Form()
		{
			return new TemplateApplicationScopeEnforcer(new FormStrategy());
		}

		#endregion

		#region Constructor

		TemplateApplicationScopeEnforcer(Strategy strat)
		{
			this.strat = strat;
		}

		readonly Strategy strat;

		public StrategyType Type => strat.Type;

		#endregion

		#region Get Scope

		public bool ShouldApplyTemplate(IWorkflowProvider provider, ProcessTaskTemplate template, BusinessObjectFactory factory)
		{
			if (provider != null && !WorkflowDataRegistry.Instance.EnableWorkflowTemplateScopeRestrictions.Value || strat.ShouldApplyStrategy(provider))
			{
				var filter = new ZQuery(ProcessCompanyLinkRuleSchema.PCR_GC_Company, GlbCompany.CurrentCompany.PK)
					.AddToFilter(ProcessCompanyLinkRuleSchema.PCR_Type, provider.WorkflowType)
					.AddToFilter(ProcessCompanyLinkRuleSchema.PCR_IsActive, true);

				var companyRules = factory.Load<IProcessCompanyLinkRule>(filter);
				if (!companyRules.Any())
				{
					return true;
				}
				else
				{
					var parent = provider.WorkflowItems?.Parent;
					if (parent != null)
					{
						BusinessObject jobHeader = null;
						if (ProcessJobHeaderProvider.SupportsPAVE(provider.WorkflowType, factory))
						{
							jobHeader = (BusinessObject)ProcessJobHeaderProvider.GetForParent((IWorkflowProvider)parent, factory, addDefaultProcessHeaderIfNone: false);
						}

						var evaluator = ObjectFactory.Get<IUserDefinedConditionEvaluator>();
						return companyRules.Any(rule => evaluator.IsTextMacroConditionMet(rule.PCR_Macro, parent, useTemplateCacheForConditions: false, dataContext: new BusinessObject[] { jobHeader, template }));
					}
				}
			}
			return false;
		}

		internal void TrackMember(IWorkflowProvider messageTarget)
		{
			Track(messageTarget.Identifier);
		}

		internal void Track(ZGuid messageTarget)
		{
			strat.TrackMember(messageTarget);
		}

		#endregion
	}

	class MessageTemplateScopeSubscriber : IMessageTemplateApplicationScopeEnforcerProvider
	{
		public void Track(BusinessObjectFactory factory, ZGuid guid)
		{
			var service = TemplateApplicationScopeEnforcer_Extensions.SetWorkflowTemplateScopeToMessage(factory);
			service.Track(guid);
		}
	}

	public static class TemplateApplicationScopeEnforcer_Extensions
	{
		public static void SetWorkflowTemplateScopeToDisallow(this BusinessObjectFactory factory)
		{
			OverriderService(factory, TemplateApplicationScopeEnforcer.Null());
		}

		public static void SetWorkflowTemplateScopeToDefault(this BusinessObjectFactory factory)
		{
			OverriderService(factory, TemplateApplicationScopeEnforcer.Default());
		}

		public static void SetWorkflowTemplateScopeToMessage(this IWorkflowProvider messageTarget)
		{
			var factory = messageTarget.LogsFactory;
			var service = SetWorkflowTemplateScopeToMessage(factory);
			service.TrackMember(messageTarget);
		}

		internal static TemplateApplicationScopeEnforcer SetWorkflowTemplateScopeToMessage(BusinessObjectFactory factory)
			=> factory.ServiceContainer.GetService<TemplateApplicationScopeEnforcer>()
				?? OverriderService(factory, TemplateApplicationScopeEnforcer.Message());

		public static void SetWorkflowTemplateScopeToForm(this IWorkflowProvider rootBindingMember)
		{
			OverriderService(rootBindingMember.LogsFactory, TemplateApplicationScopeEnforcer.Form());
		}

		public static TemplateApplicationScopeEnforcer GetWorkflowTemplateScopeEnforcer(this IWorkflowProvider workflowProvider)
		{
			var factory = workflowProvider?.LogsFactory;
			if (factory != null)
			{
				var service = factory.ServiceContainer.GetService<TemplateApplicationScopeEnforcer>();
				if (service == null)
				{
					if (Globals.IsTest || !Globals.IsUserInteractive)
					{
						return TemplateApplicationScopeEnforcer.Default();
					}
					else
					{
						return TemplateApplicationScopeEnforcer.Form();
					}
				}
				return service;
			}
			else
			{
				return TemplateApplicationScopeEnforcer.Null();
			}
		}

		static TemplateApplicationScopeEnforcer OverriderService(BusinessObjectFactory factory, TemplateApplicationScopeEnforcer service)
		{
			var serviceContainer = factory.ServiceContainer;
			serviceContainer.RemoveService<TemplateApplicationScopeEnforcer>();
			serviceContainer.AddService(service);
			return service;
		}
	}
}
