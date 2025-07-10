using System.Linq;
using Enterprise.Environment;

namespace Enterprise.Freight.Business
{
	public class CommonConsolDocumentSupporterQueryProvider : ICommonConsolDocumentSupporterQueryProvider
	{
		bool ICommonConsolDocumentSupporterQueryProvider.ConfirmBOLPrinting(CommonShipment shipment)
		{
			return Env.Security.AllowPrintingOfAWBHBLIfNoExportDeclarationFiled.IsAllowed;
		}

		DocumentCommonConsol ICommonConsolDocumentSupporterQueryProvider.GetConsolToPrint(DocumentCommonConsol documentConsol)
		{
			return documentConsol;
		}

		ContainersToPrintOptions ICommonConsolDocumentSupporterQueryProvider.GetContainersToPrint(ContainerToSelectFromForPrintingCollection containersToSelectFrom, bool includeUnContainerised)
		{
			return new ContainersToPrintOptions { ContainersToPrint = containersToSelectFrom.Cast<ContainerToSelectFromForPrinting>().Select(c => c.Container).ToArray(), IncludeUnContainerised = includeUnContainerised };
		}
	}
}
