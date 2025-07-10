using System;
using System.Collections;
using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	class TemplateFallbackIterator : IEnumerable<TemplateFallbackIterator.Result>
	{
		internal TemplateFallbackIterator(IEnumerable<ProcessTaskTemplate> allValidTemplates, TemplateEntityType templateEntityType, Func<ProcessTaskTemplate, IWorkflowItemCollection, bool> hasItemsAppliedFromTemplatePredicate)
		{
			this.allValidTemplates = allValidTemplates;
			this.templateEntityType = templateEntityType;
			this.hasItemsAppliedFromTemplatePredicate = hasItemsAppliedFromTemplatePredicate;
		}

		readonly IEnumerable<ProcessTaskTemplate> allValidTemplates;
		readonly TemplateEntityType templateEntityType;
		readonly Func<ProcessTaskTemplate, IWorkflowItemCollection, bool> hasItemsAppliedFromTemplatePredicate;

		#region IEnumerable Members

		public IEnumerator<Result> GetEnumerator()
		{
			var templateFallbackMatchStatus = MatchingWorkflowTemplateStatusCodeList.Codes.MatchAndApply;
			var lastFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			ProcessTaskTemplate lastTemplate = null;
			foreach (var template in allValidTemplates)
			{
				var fallbackMethod = GetFallbackMethod(template);
				templateFallbackMatchStatus = GetNextStatus(lastTemplate, templateEntityType, hasItemsAppliedFromTemplatePredicate, templateFallbackMatchStatus, lastFallbackMethod);
				yield return new Result(template, templateFallbackMatchStatus, fallbackMethod);
				lastFallbackMethod = fallbackMethod;
				lastTemplate = template;
			}
		}

		static string GetNextStatus(ProcessTaskTemplate lastTemplate, TemplateEntityType type, Func<ProcessTaskTemplate, IWorkflowItemCollection, bool> hasItemsAppliedFromTemplatePredicate, string templateFallbackMatchStatus, string fallbackMethod)
		{
			if (lastTemplate == null)
			{
				return MatchingWorkflowTemplateStatusCodeList.Codes.MatchAndApply;
			}
			else
			{
				switch (templateFallbackMatchStatus)
				{
					case MatchingWorkflowTemplateStatusCodeList.Codes.MatchAndApply:
					case MatchingWorkflowTemplateStatusCodeList.Codes.MatchBasedOnUdf:
						switch (fallbackMethod)
						{
							case FallbackTypeList.Codes.NeverFallback:
								return MatchingWorkflowTemplateStatusCodeList.Codes.MatchButNeverApplyBecauseOfNFB;

							case FallbackTypeList.Codes.EmptyFallback:
								if (hasItemsAppliedFromTemplatePredicate(lastTemplate, GetItems(lastTemplate, type)))
								{
									return MatchingWorkflowTemplateStatusCodeList.Codes.MatchButNeverApplyBecauseOfEFB;
								}
								else
								{
									return MatchingWorkflowTemplateStatusCodeList.Codes.MatchBasedOnUdf;
								}
							case FallbackTypeList.Codes.AlwaysFallback:
								return templateFallbackMatchStatus;
							default:
								throw new InvalidOperationException("Invalid fallback method: " + fallbackMethod);
						}

					case MatchingWorkflowTemplateStatusCodeList.Codes.MatchButNeverApplyBecauseOfNFB:
					case MatchingWorkflowTemplateStatusCodeList.Codes.MatchButNeverApplyBecauseOfEFB:
						return templateFallbackMatchStatus;

					default:
						throw new InvalidOperationException("Invalid Status: " + templateFallbackMatchStatus);
				}
			}
		}

		static IWorkflowItemCollection GetItems(ProcessTaskTemplate template, TemplateEntityType type)
		{
			return type switch
			{
				TemplateEntityType.Milestones => template.WorkflowItems.Milestones,
				TemplateEntityType.Tasks => template.WorkflowItems.Tasks,
				TemplateEntityType.Triggers => template.WorkflowItems.Triggers,
				TemplateEntityType.ValidationTool => ReturnNullSinceValidationToolCanCalculateHasItemsAppliedWithoutIWorkflowItemCollection(),
				_ => throw new InvalidOperationException("Invalid template type for calculating template fallback:" + type)
			};

			IWorkflowItemCollection ReturnNullSinceValidationToolCanCalculateHasItemsAppliedWithoutIWorkflowItemCollection() => null;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Implementation

		string GetFallbackMethod(ProcessTaskTemplate template) => templateEntityType switch
		{
			TemplateEntityType.Tasks => template.P0_TaskFallbackMethod,
			TemplateEntityType.Milestones => template.P0_MilestoneFallbackMethod,
			TemplateEntityType.Triggers => template.P0_TriggerFallbackMethod,
			TemplateEntityType.ValidationTool => template.P0_ValidationFallbackMethod,
			_ => throw new InvalidOperationException("Invalid templateEntityType: " + templateEntityType)
		};

		#endregion

		internal class Result
		{
			internal Result(ProcessTaskTemplate template, string matchingWorkflowTemplateStatus, string fallbackMethod)
			{
				this.template = template;
				this.matchingWorkflowTemplateStatus = matchingWorkflowTemplateStatus;
				this.fallbackMethod = fallbackMethod;
			}

			readonly string matchingWorkflowTemplateStatus;
			readonly string fallbackMethod;
			readonly ProcessTaskTemplate template;

			internal ProcessTaskTemplate Template => template;
			internal string Status => matchingWorkflowTemplateStatus;
			internal string FallbackMethod => fallbackMethod;

			internal bool IsApplicableToJob
			{
				get
				{
					switch (matchingWorkflowTemplateStatus)
					{
						case MatchingWorkflowTemplateStatusCodeList.Codes.MatchAndApply:
						case MatchingWorkflowTemplateStatusCodeList.Codes.MatchBasedOnUdf:
							return true;

						case MatchingWorkflowTemplateStatusCodeList.Codes.MatchButNeverApplyBecauseOfEFB:
						case MatchingWorkflowTemplateStatusCodeList.Codes.MatchButNeverApplyBecauseOfNFB:
							return false;

						default:
							throw new InvalidOperationException("Unrecognized code: " + matchingWorkflowTemplateStatus);
					}
				}
			}
		}
	}
}
