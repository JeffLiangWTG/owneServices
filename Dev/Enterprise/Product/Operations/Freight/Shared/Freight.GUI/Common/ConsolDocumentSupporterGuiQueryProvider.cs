using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public class ConsolDocumentSupporterGuiQueryProvider : SharedGuiQueryProvider, ICommonConsolDocumentSupporterQueryProvider
	{
		public static void Register(BusinessObjectFactory factory)
		{
			if (factory != null)
			{
				factory.SetValue<ICommonConsolDocumentSupporterQueryProvider, ConsolDocumentSupporterGuiQueryProvider>();
			}
		}

		bool ICommonConsolDocumentSupporterQueryProvider.ConfirmBOLPrinting(CommonShipment shipment)
		{
			return base.ConfirmBOLPrinting(shipment);
		}

		DocumentCommonConsol ICommonConsolDocumentSupporterQueryProvider.GetConsolToPrint(DocumentCommonConsol documentConsol)
		{
			return ZFormModaliser.ShowDialogAndDispose(new DocumentCommonConsolForm(documentConsol)) == DialogResult.Yes ? documentConsol : null;
		}

		ContainersToPrintOptions ICommonConsolDocumentSupporterQueryProvider.GetContainersToPrint(ContainerToSelectFromForPrintingCollection containersToSelectFrom, bool includeUnContainerised)
		{
			return base.GetContainersToPrint(containersToSelectFrom, includeUnContainerised);
		}
	}
}
