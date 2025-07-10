using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public class DocumentCustomizationTool : IDevTool
	{
		public bool AddAsButton
		{
			get { return false; }
		}

		public string Name
		{
			get { return (NoResString)"Customize Documents"; }
		}

		public void Show(Form form)
		{
			ZForm zForm = form as ZForm;
			IDocumentSupportable documentSupportable = zForm != null ? zForm.BusinessEntity as IDocumentSupportable : null;

			if (documentSupportable != null)
			{
				DocumentMenuCustomisation customisation = DocumentMenuCustomisation.New(documentSupportable, new UserControlProviderList());
				ZForm documentCustomizationForm = (ZForm)ObjectFactory.New<IMenuCustomisationForm>(customisation);
				ZFormModaliser.Show(documentCustomizationForm, form);
			}
		}
	}
}
