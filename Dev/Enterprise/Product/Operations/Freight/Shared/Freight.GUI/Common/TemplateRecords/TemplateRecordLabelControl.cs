using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI.Common.TemplateRecords
{
	public partial class TemplateRecordLabelControl : ZUserControl
	{
		public TemplateRecordLabelControl(StmTemplateRecord template = null)
		{
			InitializeComponent();

			if (template != null)
			{
				BindingSource.DataSource = template;
				templateNameTextBox.SetDataBinding(template, nameof(StmTemplateRecord.STR_TemplateName));
			}
			else
			{
				templateNameTextBox.Dispose();
			}

			Dock = DockStyle.Top;
		}
	}
}
