using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Workflow.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTemplateValidation : AutoProcessTemplateValidation, IAntlrMacroContextProvider, IProcessTemplateValidation
	{
		public ProcessTemplateValidation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("ProcessTemplateValidation|P0V_Description", Caption = "Rule Description", ShortCaption = "Desc.")]
		public override ZString P0V_Description { get => base.P0V_Description; set => base.P0V_Description = value; }

		[List(nameof(Lookups) + "." + nameof(ProcessTemplateValidationLookups.Condition1List))]
		[ResourceStringData("ProcessTemplateValidation|P0V_Condition1", Caption = "Condition 1", ShortCaption = "Cond. 1")]
		public override ZString P0V_Condition1 { get => base.P0V_Condition1; set => base.P0V_Condition1 = value; }

		#region P0V_Condition2

		[List(nameof(Lookups) + "." + nameof(ProcessTemplateValidationLookups.Condition2List))]
		[ResourceStringData("ProcessTemplateValidation|P0V_Condition2", Caption = "Condition 2", ShortCaption = "Cond. 2")]
		public override ZString P0V_Condition2
		{
			get => base.P0V_Condition2;
			set
			{
				var oldValue = P0V_Condition2;
				base.P0V_Condition2 = value;
				if (!IsCopying && P0V_Condition2 != oldValue)
				{
					RememberOrLoadCondition2Value(oldValue, P0V_Condition2);
				}
			}
		}

		[ReadOnlyMember(nameof(P0V_Condition2Value_Readonly))]
		[ResourceStringData("ProcessTemplateValidation|P0V_Condition2Value", Caption = "Condition 2 Value", ShortCaption = "Cond.2 Value ")]
		public override ZString P0V_Condition2Value { get => base.P0V_Condition2Value; set => base.P0V_Condition2Value = value; }

		bool P0V_Condition2Value_Readonly => !ProcessTasksLookups.IsMacroCondition(P0V_Condition2);

		ZString RememberedCondition2Value { get; set; }
		void RememberOrLoadCondition2Value(ZString lastCondition2, ZString newCondition2)
		{
			var lastCondition2Readonly = !ProcessTasksLookups.IsMacroCondition(lastCondition2);
			var newCondition2Readonly = !ProcessTasksLookups.IsMacroCondition(newCondition2);
			if (lastCondition2Readonly && !newCondition2Readonly)
			{
				P0V_Condition2Value = RememberedCondition2Value;
			}
			else if (!lastCondition2Readonly && newCondition2Readonly)
			{
				RememberedCondition2Value = P0V_Condition2Value;
				P0V_Condition2Value = ZString.Empty;
			}
		}

		#endregion

		[ResourceStringData("ProcessTemplateValidation|P0V_GC_Company", Caption = "Company", ShortCaption = "Company")]
		public override ZGuid P0V_GC_Company { get => base.P0V_GC_Company; set => base.P0V_GC_Company = value; }

		[ResourceStringData("ProcessTemplateValidation|P0V_Message", Caption = "Validation Message", ShortCaption = "Val. Msg.")]
		public override ZString P0V_Message { get => base.P0V_Message; set => base.P0V_Message = value; }

		[List(nameof(Lookups) + "." + nameof(ProcessTemplateValidationLookups.SeverityList))]
		[ResourceStringData("ProcessTemplateValidation|P0V_Severity", Caption = "Severity", ShortCaption = "Sev.")]
		public override ZString P0V_Severity { get => base.P0V_Severity; set => base.P0V_Severity = value; }

		[ResourceStringData("ProcessTemplateValidation|P0V_ValidationRule", Caption = "Validation Rule", ShortCaption = "Val. Rule")]
		public override ZString P0V_ValidationRule { get => base.P0V_ValidationRule; set => base.P0V_ValidationRule = value; }

		[ResourceStringData("ProcessTemplateValidation|P0V_FieldToDisplayValidation", Caption = "Display Validation On", ShortCaption = "Disp. Val.")]
		public override ZString P0V_FieldToDisplayValidation { get => base.P0V_FieldToDisplayValidation; set => base.P0V_FieldToDisplayValidation = value; }

		[ResourceStringData("499d6811-6169-4b90-8f98-e0324122aa0f", Caption = "Create Event On Failure")]
		public override ZBool P0V_LogValidationFailEvent
		{
			get => base.P0V_LogValidationFailEvent;
			set => base.P0V_LogValidationFailEvent = value;
		}

		[ResourceStringData("ProcessTemplateValidation|P0V_ContextType", Caption = "Context Type", ShortCaption = "Context Type")]
		[List("Lookups.ContextTypeList")]
		public override ZString P0V_ContextType
		{
			get => base.P0V_ContextType;
			set => base.P0V_ContextType = value;
		}

		[ResourceStringData("ProcessTemplateValidation|P0V_RQT_RequestTypeOnFailure", Caption = "Request Type On Failure", ShortCaption = "Request Type")]
		[RelatedBusinessObject("RequestTypeOnFailure")]
		[List("Lookups.RequestTypesOnFailure")]
		public override ZGuid P0V_RQT_RequestTypeOnFailure { get => base.P0V_RQT_RequestTypeOnFailure; set => base.P0V_RQT_RequestTypeOnFailure = value; }

		public ExternalRequestType RequestTypeOnFailure => Factory.Load<ExternalRequestType>(P0V_RQT_RequestTypeOnFailure);

		public ZString TemplateCountryCode => WorkflowTemplate?.Company?.GC_RN_NKCountryCode ?? ZString.Empty;

		ZGuid IProcessTemplateValidation.P0_GC => WorkflowTemplate?.P0_GC ?? ZGuid.Empty;

		#region IWorkflowTypeProvider

		public WorkflowDescriptor WorkflowDescriptor
		{
			get
			{
				if (workflowDescriptor == null || workflowDescriptor.Code != ((IWorkflowTypeProvider)this).WorkflowProcessType)
				{
					workflowDescriptor = this.GetWorkflowDescriptor();
				}
				return workflowDescriptor;
			}
		}

		WorkflowDescriptor workflowDescriptor;

		bool IWorkflowTypeProvider.IsTemplate => true;

		ZString IWorkflowTypeProvider.WorkflowProcessType => WorkflowTemplate?.P0_ProcessType ?? ZString.Empty;

		#endregion

		#region IRootTypeProvider

		IAntlrMacroContext IAntlrMacroContextProvider.GetSampleContext()
		{
			var countryCode = TemplateCountryCode;
			var dataModelType = WorkflowDescriptor?.ValidationToolSettings.GetProcessTemplateValidationRootObjectType(P0V_Condition1, countryCode);

			return ObjectFactory.Get<IWorkflowMacroContextDecider>().GetDefaultWorkflowMacroContext(Factory, dataModelType is null ? null : Factory.GetNull(dataModelType), dataModelType);
		}

		#endregion

		[ChildEditable(true)]
		public ProcessTemplateValidationActionCollection ProcessTemplateValidationActions
		{
			get
			{
				if (processTemplateValidationActions is null)
				{
					processTemplateValidationActions = new ProcessTemplateValidationActionCollection(this);
					RegisterEditableChildObject(processTemplateValidationActions);
				}

				return processTemplateValidationActions;
			}
		}
		ProcessTemplateValidationActionCollection processTemplateValidationActions;

		public override void Delete()
		{
			ProcessTemplateValidationActions.DeleteAll();
			base.Delete();
		}

		internal FiledToDisplayValidationStaticInfo FieldToDisplayValidationStaticInfo => Factory.GetCached(ref fieldToDisplayValidationStaticInfo, () =>
		{
			var fieldToDisplayValidation = P0V_FieldToDisplayValidation;
			if (fieldToDisplayValidation.IsEmpty || WorkflowDescriptor is null)
			{
				return null;
			}

			var antlrMacroContextProvider = ((IAntlrMacroContextProvider)this).GetSampleContext();
			var result = new FieldToDisplayValidationResolver(fieldToDisplayValidation).GetFieldToDisplayValidationStaticInfo(antlrMacroContextProvider.ParentType);
			return result;
		});
		CachedProperty<FiledToDisplayValidationStaticInfo> fieldToDisplayValidationStaticInfo;

		public ZBool HasValidationAction(ZString actionSourceCode) => !actionSourceCode.IsEmpty && ProcessTemplateValidationActions.Any(x => x.P0A_ActionSource == actionSourceCode);

		public ProcessTemplateValidationAction GetValidationAction(ZString actionSourceCode) => actionSourceCode.IsEmpty ? null : ProcessTemplateValidationActions.FirstOrDefault(x => x.P0A_ActionSource == actionSourceCode);

		public ProcessTemplateValidationConditionChecker GetValidationToolChecker(IBusiness rootBusinessObject) => WorkflowDescriptor?.ValidationToolSettings.GetProcessTemplateValidationConditionChecker(this, rootBusinessObject);
	}
}
