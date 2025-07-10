using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DevTools;

namespace Enterprise.MasterFiles.GUI
{
	public class FormStateXmlExplorerTool : IDevTool
	{
		public bool AddAsButton
		{
			get { return false; }
		}

		public string Name
		{
			get { return (NoResString)"Form State Xml Explorer"; }
		}

		public void Show(Form form)
		{
			ProcessTaskTemplateForm processTaskTemplateForm = form as ProcessTaskTemplateForm;
			ProcessTaskTemplate processTaskTemplate = processTaskTemplateForm != null ? (ProcessTaskTemplate)processTaskTemplateForm.DataSource : null;

			if (processTaskTemplate != null)
			{
				FormStateXmlExplorerPresenter presenter = new FormStateXmlExplorerPresenter(processTaskTemplate);
				FormStateXmlExplorerForm explorerForm = new FormStateXmlExplorerForm(presenter);
				explorerForm.Show();
			}
		}
	}
}
