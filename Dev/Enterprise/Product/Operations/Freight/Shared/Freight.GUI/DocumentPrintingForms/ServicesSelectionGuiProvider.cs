using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public class ServicesSelectionGuiProvider : IServicesSelectionProvider
	{
		public static void Register(BusinessObjectFactory factory)
		{
			if (factory != null)
			{
				factory.SetValue<IServicesSelectionProvider, ServicesSelectionGuiProvider>();
			}
		}

		JobService[] IServicesSelectionProvider.GetServicesToPrint(IHaveServices parent)
		{
			JobService[] selectedServices = null;

			if (parent != null && parent.Services != null)
			{
				if (parent.Services.Count == 0)
				{
					Globals.Message.ShowInformation(Res.GetString("9e676cc4-374f-44c5-a5db-3d267f7b5138", "There are no services to print."),
						Res.GetString("ffeda4f1-52f0-404b-bfcc-0b2928d71830", "No Services"));
				}
				else if (parent.Services.Count == 1)
				{
					selectedServices = new[] { parent.Services[0] };
				}
				else
				{
					ServiceToSelectFromForPrintingCollection servicesToSelectFrom = new ServiceToSelectFromForPrintingCollection(parent.Services);
					DocumentServices docServicesBizObject = new DocumentServices(parent, servicesToSelectFrom);

					if (ZFormModaliser.ShowDialogAndDispose(new DocumentServicesForm(docServicesBizObject)) == DialogResult.Yes)
					{
						selectedServices = servicesToSelectFrom.Cast<ServiceToSelectFromForPrinting>()
							.Where(srv4Print => srv4Print.ES_Calc_PrintDocumentForService)
							.Select(srv4Print => srv4Print.Service).ToArray();
					}
				}
			}

			return selectedServices;
		}
	}
}
