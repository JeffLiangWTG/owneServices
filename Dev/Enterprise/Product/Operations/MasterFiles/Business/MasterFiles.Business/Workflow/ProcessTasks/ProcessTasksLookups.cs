using System;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Workflow.ProcessTasks.Milestones;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTasksLookups : AutoProcessTasksLookups
	{
		public ProcessTasksLookups(AutoProcessTasks parent)
			: base(parent)
		{
		}

		public ProcessTasksLookups(BusinessObjectFactory factory)
			: base(null)
		{
			this.factory = factory;
		}

		protected override BusinessObjectFactory Factory
		{
			get { return factory ?? Parent.Factory; }
		}

		readonly BusinessObjectFactory factory;

		protected new ProcessTask Parent
		{
			get { return (ProcessTask)base.Parent; }
		}

		#region Organisations

		public OrganisationsFindBoxCollection Organisations
		{
			get
			{
				if (fOrganisations == null)
				{
					fOrganisations = new OrganisationsFindBoxCollection(Factory);
				}

				return fOrganisations;
			}
		}

		OrganisationsFindBoxCollection fOrganisations;

		#endregion

		#region Contacts

		public override OrgContactCollection Contacts
		{
			get
			{
				OrgContactCollection result = new OrgContactCollection(Factory);
				if (!Parent.OrganisationPK.IsEmpty)
				{
					result.Load(new ZQuery(OrgContactSchema.OC_OH, Parent.OrganisationPK));
				}
				return result;
			}
		}

		#endregion

		#region Addresses

		public override OrgAddressCollection Addresses
		{
			get
			{
				OrgAddressCollection result = new OrgAddressCollection(Factory);
				if (!Parent.OrganisationPK.IsEmpty)
				{
					result.Load(new ZQuery(OrgAddressSchema.OA_OH, Parent.OrganisationPK));
				}
				return result;
			}
		}

		#endregion

		#region Conditions

		public const string UserDefinedCondition = "UDF";
		public const string MacroCondition = ProcessTaskConstants.MacroCondition;

		public static ResourceString MacroConditionDescription => ResString.GetMultilingualString("60736eb2-b073-4edd-97b5-13409c152fb8", "Condition with Macros");
		public static ResourceString UserDefinedConditionDescription => ResString.GetMultilingualString("4538c383-c2ee-49e0-898a-4d0a55ee8777", "User Defined");

		public static bool IsMacroCondition(ZString value)
		{
			return value == ProcessTasksLookups.UserDefinedCondition || value == ProcessTasksLookups.MacroCondition;
		}

		#endregion

		#region Types

		public WorkflowTaskTypeCollection Types
		{
			get
			{
				var companyPk = CompanyPK;
				var key = ("ProcessTaskTypesLookups", Parent.WorkflowType, companyPk);
				return Factory.GetCachedValue(key, () =>
				{
					var registryValue = WorkflowDataRegistry.Instance.TaskTypes.GetFallBackValueAtAllLevels(companyPk.ToGuid(), Guid.Empty, Guid.Empty);
					return registryValue.GetTaskTypesFromWorkflowCode(Parent.WorkflowType);
				});
			}
		}

		ZGuid CompanyPK => Parent.P9_GC.IsValid ? Parent.P9_GC : Env.CurrentCompanyPK;

		public void RefreshTypeList()
		{
			Factory.ClearCachedValue<WorkflowTaskTypeCollection>(("ProcessTaskTypesLookups", Parent.WorkflowType, CompanyPK));
		}

		#endregion

		#region Exception Types

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter strings")]
		public ProcessWorkflowExceptionTypeCollection ExceptionTypes
		{
			get
			{
				var collection = new ProcessWorkflowExceptionTypeCollection(Factory);

				collection.FilterBusinessObjectDefaults.Add(FilterBusinessObjectDefault.Create(
					filterName: "Job Type",
					propertyName: "Property",
					category: FilterOrCategory.Red,
					comparisonOperator: ModuleTextFilter.ComparisonConstants.IsBlank,
					instance: 1));
				collection.FilterBusinessObjectDefaults.Add(FilterBusinessObjectDefault.Create(
					filterName: "Job Type",
					propertyName: "Property",
					value: new ZString(Parent.WorkflowType),
					category: FilterOrCategory.Red,
					comparisonOperator: ModuleTextFilter.ComparisonConstants.Exact,
					instance: 2));

				if (!Parent.ExceptionTypeCategory.IsEmpty)
				{
					collection.FilterBusinessObjectDefaults.Add(FilterBusinessObjectDefault.Create("Category", "Property", Parent.ExceptionTypeCategory, comparisonOperator: ModuleTextFilter.ComparisonConstants.Exact));
				}

				return collection;
			}
		}

		public ICodeDescriptionPairList ExceptionTypeCategories
		{
			get
			{
				return Factory.GetCachedValue("ProcessTasksLookups.ProcessWorkflowExceptionTypeCategories", () =>
				{
					var list = new CodeDescriptionPairList(WorkflowDataRegistry.Instance.ExceptionCategories.Value);
					list.Sort();
					return list;
				});
			}
		}

		ZGuid ProcessWorkflowExceptionTypePK => Parent?.ExceptionType?.PK ?? ZGuid.Empty;

		public CodeDescriptionPairList ExceptionCauses
		{
			get
			{
				var typePK = ProcessWorkflowExceptionTypePK;

				var causes = Factory.GetCachedValue("ProcessTasksLookups.ExceptionCauses" + typePK, () =>
				{
					var result = new CodeDescriptionPairList();

					if (typePK.IsEmpty)
					{
						return result;
					}

					var query = new ZQuery(ProcessWorkflowExceptionCauseSchema.WEC_WET_Type, typePK);
					query.AddToFilter(ProcessWorkflowExceptionCauseSchema.WEC_IsActive, true);
					var causes = Factory.Load<ProcessWorkflowExceptionCause>(query);

					foreach (var cause in causes)
					{
						result.AddPair(cause.PK, cause.WEC_Code, cause.WEC_Description);
					}

					result.Sort();
					return result;
				});

				var currentCause = Parent.ProcessWorkflowException?.Cause;

				if (currentCause != null && !causes.ContainsCode(currentCause.WEC_Code))
				{
					causes = new CodeDescriptionPairList(causes);
					causes.AddPair(currentCause.PK, currentCause.WEC_Code, currentCause.WEC_Description);
					causes.Sort();
				}

				return causes;
			}
		}

		public CodeDescriptionPairList ExceptionResolutions
		{
			get
			{
				var typePK = ProcessWorkflowExceptionTypePK;

				var resolutions = Factory.GetCachedValue("ProcessTasksLookups.ExceptionResolutions" + typePK, () =>
				{
					var result = new CodeDescriptionPairList();

					if (typePK.IsEmpty)
					{
						return result;
					}

					var query = new ZQuery(ProcessWorkflowExceptionResolutionSchema.WER_WET_Type, typePK);
					query.AddToFilter(ProcessWorkflowExceptionResolutionSchema.WER_IsActive, true);
					var resolutions = Factory.Load<ProcessWorkflowExceptionResolution>(query);

					foreach (var resolution in resolutions)
					{
						result.AddPair(resolution.PK, resolution.WER_Code, resolution.WER_Description);
					}

					result.Sort();
					return result;
				});

				var currentResolution = Parent.ProcessWorkflowException?.Resolution;

				if (currentResolution != null && !resolutions.ContainsCode(currentResolution.WER_Code))
				{
					resolutions = new CodeDescriptionPairList(resolutions);
					resolutions.AddPair(currentResolution.PK, currentResolution.WER_Code, currentResolution.WER_Description);
					resolutions.Sort();
				}

				return resolutions;
			}
		}

		#endregion

		#region CompletionMilestones

		public CodeDescriptionPairList CompletionMilestones => Parent?.Parent?.WorkflowItems?.CompletionMilestoneCodeDescriptionPairList ?? new CodeDescriptionPairList();

		#endregion

		#region EstimateDefaultedFromList

		public CodeDescriptionPairList EstimateDefaultedFromList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				if (Parent != null)
				{
					result.AddRange(EstimateDefaultedFromPredecessorList);
					if (Parent.WorkflowDescriptor != null)
					{
						CodeDescriptionPairList additionalDefaultFromItems = Parent.WorkflowDescriptor.EstimateDefaultedFromList;
						if (additionalDefaultFromItems != null)
						{
							result.AddPair("", "");
							result.AddRange(additionalDefaultFromItems);
						}
					}
				}
				return result;
			}
		}

		CodeDescriptionPairList EstimateDefaultedFromPredecessorList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				if (Parent.Parent != null)
				{
					foreach (ProcessTask milestone in Parent.Parent.WorkflowItems.Milestones)
					{
						if (milestone.PK != Parent.PK)
						{
							result.AddPair(milestone.PredecessorsEstimateDefaultedFromString(false), milestone.P9_Description);
							result.AddPair(milestone.PredecessorsEstimateDefaultedFromString(true), milestone.P9_Description);
						}
					}
				}
				return result;
			}
		}

		#endregion

		#region Statuses

		public CodeDescriptionPairList Statuses
		{
			get { return (Parent != null && Parent.IsException) ? Factory.GetCachedValue<ExceptionStatusCodeList>() : Factory.GetCachedValue<ProcessTaskStatusCodeList>(); }
		}

		#endregion

		#region Actual Date Update Types

		public CodeDescriptionPairList ActualDateUpdateTypes => Factory.GetCachedValue<ActualDateUpdateTypeCodeList>();

		#endregion

		#region Process Headers

		public IProcessHeaderCollection ProcessHeaders
		{
			get => Factory.GetCachedValue(("ProcessTasksLookups.ProcessHeaders", Parent.P9_FH_ProcessHeader), () => ObjectFactory.Get<IProcessHeaderCollectionProvider>().GetForTask(Parent));
		}

		#endregion

		#region Templates

		public ProcessTaskTemplateCollection Templates
		{
			get
			{
				return Factory.GetCachedValue("ProcessTaskLookups.Templates", () =>
				{
					return new ProcessTaskTemplateCollection(Factory);
				});
			}
		}

		#endregion

		#region Line Trigger Types

		public CodeDescriptionPairList LineTriggerTypes
		{
			get
			{
				return Factory.GetCachedValue("LineTriggerTypes_" + ParentWorkflowType, () =>
				{
					var lineTriggerTypes = new CodeDescriptionPairList();
					var parentWorkflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(ParentWorkflowType);

					var parentWithLines = parentWorkflowDescriptor as IWorkflowParentWithLines;
					if (parentWithLines != null && parentWithLines.SupportedTriggerLineTypes != null)
					{
						foreach (var type in parentWithLines.SupportedTriggerLineTypes)
						{
							lineTriggerTypes.Add(TriggerLineTypes.All[type]);
						}
					}

					if (parentWorkflowDescriptor?.SupportsTaskLineTriggers ?? false)
					{
						lineTriggerTypes.AddPair(TaskLineTriggerCode, Res.GetString("4d1d399b-8eb6-476e-a103-78ad713d2b63", "Task Line Trigger (fires when a matching event is raised on a task in this job)"));
					}

					if (parentWorkflowDescriptor?.SupportsExceptionLineTriggers ?? false)
					{
						lineTriggerTypes.AddPair(ExceptionLineTriggerCode, Res.GetString("3e2a8bd9-2397-4230-b84a-dbf81ef33c91", "Exception Line Trigger (fires when a matching event is raised on a exception in this job)"));
					}

					return lineTriggerTypes;
				});
			}
		}

		protected virtual ZString ParentWorkflowType
		{
			get { return Parent.Parent != null ? Parent.Parent.WorkflowType : ZString.Empty; }
		}

		public const string TaskLineTriggerCode = "TSK";

		public const string ExceptionLineTriggerCode = "EXP";

		#endregion

		#region Iterations

		public CodeDescriptionPairList Iterations
		{
			get
			{
				var result = new CodeDescriptionPairList();

				if (Parent != null)
				{
					var iterations = Factory.Load<IProcessTaskIterationLink>(Parent.GetIterationsForThisTasksWorkflowQuery(ProcessTaskIterationLinkSchema.P9I_Sequence, orderByDesc: false));

					string GetUnknownWorkflowResString() => Res.GetString("b7a22af5-7a38-4acb-a662-948cb6afbe1d", "Unknown Workflow");

					foreach (var iteration in iterations)
					{
						var description = Res.GetString("302b1366-ec0b-409b-9d94-85ababb368ee", "Iteration {0} for [{1}]", iteration.P9I_Sequence.ToString(), Parent.ProcessHeader?.FH_CompletionStatement ?? GetUnknownWorkflowResString());
						result.AddPairIfNotExist(iteration.P9I_Sequence.ToString(), description);
					}
				}

				return result;
			}
		}

		#endregion
	}
}
