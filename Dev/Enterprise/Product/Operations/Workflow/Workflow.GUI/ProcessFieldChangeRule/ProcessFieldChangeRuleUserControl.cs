using System;
using System.Windows.Forms;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Workflow.GUI
{
	public partial class ProcessFieldChangeRuleUserControl : ZUserControl
	{
		public ProcessFieldChangeRuleUserControl()
		{
			InitializeComponent();
			CaptionRenderingEnabled = true;
			referenceDescriptionLabel.Text = Res.GetString("733E2BE7-460A-4E97-BCC3-64971DDAD9F7", "The reference will contain 'CHG={Field Name} [Old value]->[New value]' for each field that changes in the field list in addition to the reference you provide here.");
		}

		#region Extra Info Button Click
		void ExtraInfoButton_Click(object sender, EventArgs e)
		{
			var rule = BindingSource.Current as ProcessFieldChangeRule;

			if (rule != null)
			{
				var eventReference = new EventReference(rule.PFR_SE_NKEvent, rule.PFR_Reference);

				using (var form = new EventReferenceForm(eventReference))
				{
					if (Globals.IsTest)
					{
						form.Show();
					}
					else if (form.ShowDialog() == DialogResult.OK)
					{
						rule.PFR_Reference = form.ReferenceText;
					}
				}
			}
		}
		#endregion
	}
}
