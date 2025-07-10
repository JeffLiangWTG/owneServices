using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Workflow;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.General
{
	[ToolboxItem(false)]
	public partial class CreateEventRuleControl : ZUserControl
	{
		public CreateEventRuleControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			GenCustomAddOnRule addOnRule = dataSource as GenCustomAddOnRule;
			if (addOnRule != null)
			{
				var stmALog = GetLogForBinding(addOnRule);

				var control = new StmALogCreateEventUserControl();
				try
				{
					control.Dock = System.Windows.Forms.DockStyle.Fill;
					Controls.Add(control);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					control.Dispose();
					throw;
				}

				base.SetDataBinding(stmALog, "");
			}
			else
			{
				base.SetDataBinding(dataSource, dataMember);
			}
		}

		static StmALogCreateEventRule GetLogForBinding(GenCustomAddOnRule addOnRule)
		{
			var availableRule = addOnRule.AllRules.Cast<AvailableRule>().First(r => r.Rule.Code.Equals(CustomAddOnRuleTypes.CreateEvent));
			var rule = (CreateEventRule)availableRule.Rule;
			var stmALog = new BusinessObjectFactory().New<StmALogCreateEventRule>();
			using (stmALog.SuspendSettingHasChanges())
			using (stmALog.GetValidationSuspender())
			{
				stmALog.SL_Parent = stmALog.PK;

				stmALog.HookupChanges(rule);
				availableRule.RegisterEditableChildObject(stmALog);
				return stmALog;
			}
		}
	}
}
