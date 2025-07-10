using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Workflow.Business
{
	public class ProcessCompanyLinkRule : AutoProcessCompanyLinkRule,
		IProcessCompanyLinkRule,
		IRootTypeProvider,
		IWorkflowTypeProvider
	{
		public ProcessCompanyLinkRule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[List("Lookups.Types")]
		public override ZString PCR_Type { get => base.PCR_Type; set => base.PCR_Type = value; }

		[List("Lookups.Staffs")]
		public override ZString PCR_SystemCreateUser { get => base.PCR_SystemCreateUser; set => base.PCR_SystemCreateUser = value; }

		[List("Lookups.Staffs")]
		public override ZString PCR_SystemLastEditUser { get => base.PCR_SystemLastEditUser; set => base.PCR_SystemLastEditUser = value; }

		#endregion

		#region Implementation

		protected override ZString HumanReadableShortcutNameCore => Res.GetString("ProcessCompanyLinkRule|HumanReadableName", "Template Rule [{0}] [{1}]", Company?.GC_Code, PCR_Type);

		#endregion

		#region Cloning

		public ActiveBusinessObjectCollection<ProcessCompanyLinkRule> RulesForBinding
		{
			get
			{
				if (rules == null)
				{
					rules = new ProcessCompanyLinkRuleWithMultipleCompaniesCollection(this);
				}
				return rules;
			}
		}
		ActiveBusinessObjectCollection<ProcessCompanyLinkRule> rules;

		public void CloneForOtherCompanies()
		{
			foreach (var rule in RulesForBinding)
			{
				rule.PCR_IsActive = PCR_IsActive;
				rule.PCR_Type = PCR_Type;
				rule.PCR_Macro = PCR_Macro;
			}
		}

		public override bool CanDelete
		{
			get
			{
				return !lockDelete && base.CanDelete;
			}
		}
		bool lockDelete;

		public IDisposable PreventDeletionWhileCloningMasterRule()
		{
			lockDelete = true;
			return new DisposableAction(() => lockDelete = false);
		}

		public override MultilingualString ReasonForNotAbleToDelete => lockDelete
			? ResString.GetMultilingualString("FB473564-D427-477F-84F8-02A320BE35DF", "Cannot delete this template company rule when cloning it for other companies")
			: base.ReasonForNotAbleToDelete;

		#endregion

		#region IWorkflowTypeProvider

		ZString IWorkflowTypeProvider.WorkflowProcessType => PCR_Type;
		bool IWorkflowTypeProvider.IsTemplate => false;

		#endregion

		#region IRootTypeProvider

		Type[] IRootTypeProvider.RootTypes => this.GetRootTypes().Append(typeof(ProcessTaskTemplate)).ToArray();

		BusinessObject[] IRootTypeProvider.Roots => Array.Empty<BusinessObject>();

		#endregion

		#region Testing

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			PCR_Macro = "\"1\"==\"1\"";
			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif
		#endregion
	}
}
