using System.Windows.Forms;
using Enterprise.DocumentEngine.GUI;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	internal class ReleaseDocumentCustomisationDevTool : DocumentCustomisationDevTool
	{
		protected override void ShowCore(Form form)
		{
			ZForm zForm;
			ReleaseHeader header;

			if ((zForm = form as ZForm) == null)
			{
				Globals.Message.Show((NoResString)"Not a ZForm");    // Developer Diagnostic Tool
			}
			else if ((header = zForm.BusinessEntity as ReleaseHeader) == null)
			{
				Globals.Message.Show((NoResString)"Could not find the ReleaseHeader");   // Developer Diagnostic Tool
			}
			else
			{
				ReleaseInstance instance = new ReleaseInstance(header);

				foreach (ReleaseDetail detail in header.Details)
				{
					if (detail.ReleaseCount > 0)
					{
						instance.ReleaseNumber = detail.Container.JC_ReleaseNum;
						instance.ContainerYardAddress = detail.Container.JC_OA_DepartureContainerYardAddress;
						break;
					}
				}

				ShowCustomisationForm(new ReleaseInstance(header));
			}
		}
	}
}
