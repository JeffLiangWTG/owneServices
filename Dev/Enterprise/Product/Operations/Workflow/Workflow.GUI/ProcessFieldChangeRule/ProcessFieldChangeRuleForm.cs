using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Workflow.GUI
{
	public partial class ProcessFieldChangeRuleForm : ZTemplateForm
	{
		ProcessFieldChangeRuleForm()
		{
			InitializeComponent();
		}

		public override string FormCaption => Res.GetString("84F442F8-A741-47E6-B606-F234E0345633", "Field Change Event");

		public ProcessFieldChangeRuleForm(ProcessFieldChangeRule rule)
			: base(rule)
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (DataSource != null)
			{
				DataSource.PFR_ProcessTypeInfo.ValueChanged += ClearFields;
			}
		}

		void ClearFields(object sender, EventArgs e)
		{
			if (e is ValueChangedEventArgs values)
			{
				var oldValue = (ZString)values.OldValue;
				var newValue = (ZString)values.NewValue;
				if (!oldValue.EqualsIgnoringCase(newValue))
				{
					DataSource.Fields.DeleteAll();
				}
			}
		}

		new ProcessFieldChangeRule DataSource => (ProcessFieldChangeRule)base.DataSource;

		protected override bool SupportsEDocs => false;

		protected override bool ShowAuditTab => true;
	}
}
