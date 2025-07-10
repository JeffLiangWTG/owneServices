using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business
{
	public class ProcessFieldChangeRule : AutoProcessFieldChangeRule,
		IProcessFieldChangeRule,
		IWorkflowTypeProvider,
		IAuditParent
	{
		public ProcessFieldChangeRule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		#region Properties

		[List("Lookups.Types")]
		public override ZString PFR_ProcessType { get => base.PFR_ProcessType; set => base.PFR_ProcessType = value; }

		[List("Lookups.Staffs")]
		public override ZString PFR_SystemCreateUser { get => base.PFR_SystemCreateUser; set => base.PFR_SystemCreateUser = value; }

		[List("Lookups.Staffs")]
		public override ZString PFR_SystemLastEditUser { get => base.PFR_SystemLastEditUser; set => base.PFR_SystemLastEditUser = value; }

		[ChildEditable(true)]
		public IActiveBusinessObjectCollection<IProcessFieldChangeRuleField> Fields => FieldsForBinding;

		[ChildEditable(true)]
		public ProcessFieldChangeRuleFieldCollection FieldsForBinding
		{
			get
			{
				if (fields == null)
				{
					fields = new ProcessFieldChangeRuleFieldCollection(this);
					RegisterEditableChildObject(fields);
				}
				return fields;
			}
		}
		ProcessFieldChangeRuleFieldCollection fields;

		public ZString FieldsDisplayString => string.Join(", ", FieldsForBinding.ToList());

		#endregion

		#region Implementation

		protected override ZString HumanReadableShortcutNameCore => Res.GetString("ProcessFieldChangeRule|HumanReadableName", "[{0}] [{1}] {2}", PFR_ProcessType, PFR_SE_NKEvent, PFR_GroupName);

		#endregion

		#region Delete

		public override void Delete()
		{
			fields?.DeleteAll();
			base.Delete();
		}
		#endregion

		#region IWorkflowTypeProvider

		ZString IWorkflowTypeProvider.WorkflowProcessType => PFR_ProcessType;
		bool IWorkflowTypeProvider.IsTemplate => false;

		#endregion

		#region ShouldCreateAutoLogIfOnlyChildrenHaveChanges

		protected override bool ShouldCreateAutoLogIfOnlyChildrenHaveChanges => true;

		#endregion

		#region ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges

		protected override bool ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges => true;

		#endregion

		#region Testing

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			PFR_ProcessType = "SHP";
			PFR_GroupName = "TEST";
			PFR_SE_NKEvent = "Z00";
			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif
		#endregion

		public override void OnSaving()
		{
			base.OnSaving();
			var ruleStore = ObjectFactory.Get<IProcessFieldChangeRuleStore>();
			ruleStore.ClearCache();
		}

		IEnumerable<AuditChildInfo> IAuditParent.RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(ProcessFieldChangeRuleFieldSchema.PFL_PFR, ProcessFieldChangeRuleFieldSchema.PFL_FieldName);
			}
		}
	}
}
