using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

using CargoWise.Application;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GenCustomAddOnRuleForm : ZTemplateForm
	{
		public GenCustomAddOnRuleForm()
		{
			InitializeComponent();
		}

		public GenCustomAddOnRuleForm(GenCustomAddOnRule businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
		}

		protected override bool ShowNotesTab { get { return false; } }
		protected override bool SupportsEDocs { get { return false; } }
		protected override bool ShowAuditTab => true;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			rulesGrid.ListManager.CurrentItemChanged += new EventHandler(ListManager_CurrentItemChanged);
			ListManager_CurrentItemChanged(rulesGrid.ListManager, EventArgs.Empty);
		}

		void ListManager_CurrentItemChanged(object sender, EventArgs e)
		{
			AvailableRule rule = (AvailableRule)((CurrencyManager)sender).GetCurrent();
			Control ruleControl = GetRuleControl(rule);
			foreach (Control ctrl in detailsPanel.Controls)
			{
				bool enabled = ctrl.Equals(ruleControl);
				ctrl.Enabled = enabled;
				ctrl.Visible = enabled;
			}
		}

		Control GetRuleControl(AvailableRule rule)
		{
			Control ctrl;
			if (!ruleControls.TryGetValue(rule.Rule.Code, out ctrl))
			{
				ctrl = GetNewRuleControl(rule.Rule.Code);
				if (ctrl != null)
				{
					ctrl.Size = detailsPanel.Size;
					ctrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
					detailsPanel.Controls.Add(ctrl);
				}

				ruleControls.Add(rule.Rule.Code, ctrl);
			}

			return ctrl;
		}

		Control GetNewRuleControl(string ruleCode)
		{
			Hashtable ruleTypes = ObjectFactory.Get<Hashtable>("CustomAddOnRuleControls");
			ObjectHandle objectHandle = (ObjectHandle)ruleTypes[ruleCode];
			if (objectHandle != null)
			{
				Control result = (Control)objectHandle.GetObject();
				return result;
			}
			else
			{
				return null;
			}
		}

		readonly Dictionary<string, Control> ruleControls = new Dictionary<string, Control>();
	}
}
