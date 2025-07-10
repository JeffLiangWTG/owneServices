using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public class SharedGuiQueryProvider : ISharedGuiQueryProvider
	{
		public bool ConfirmBOLPrinting(CommonShipment shipment)
		{
			var result = false;
			var shipmentHTSMaximumValue = (decimal)ObjectFactory.Get<Enterprise.Integration.Customs.US.IUSCustomsDataRegistry>().ShipmentHTSMaximumValue.Value;

			if (Env.Security.AllowPrintingOfAWBHBLIfNoExportDeclarationFiled.IsAllowed)
			{
				string message = Res.GetString("7590fec7-c224-4d41-8809-00e3e9e7e0df", @"The aggregate values of merchandise within shipment {0} that are covered by any single HTS number exceed ${1} and Customs Entry Number is not entered but you have the necessary security access to continue running this document.

Are you sure you want to run this document?", shipment.JS_UniqueConsignRef, shipmentHTSMaximumValue);
				result = Globals.Message.Show(message, Res.GetString("57e38435-cf38-4f66-996e-8a13522a5de8", "Confirm BOL Document"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question, DialogResult.OK) == DialogResult.OK;
			}
			else
			{
				string message = Res.GetString("4366eb86-1e17-4169-8ab4-541270884a6b", @"The aggregate values of merchandise within shipment {0} that are covered by any single HTS number exceed ${1} and Customs Entry Number is not entered.

You do not have the appropriate security rights to continue running this document.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: 

Operations -> Forwarding -> Shipments -> US Specifics -> Allow Printing of AWB/HBL if No Export Declaration Is Filed", shipment.JS_UniqueConsignRef, shipmentHTSMaximumValue);
				Globals.Message.ShowError(message);
			}

			return result;
		}

		public ContainersToPrintOptions GetContainersToPrint(ContainerToSelectFromForPrintingCollection containersToSelectFrom, bool includeUnContainerised)
		{
			ContainersToPrintOptions containersToPrintOptions = null;

			if (containersToSelectFrom != null)
			{
				if (containersToSelectFrom.Count == 0)
				{
					Globals.Message.ShowInformation(Res.GetString("845353a5-c1b8-4dc9-8668-3ea0abbfebae", "There are no containers."),
													Res.GetString("c6ae3f75-1748-4c90-b475-3dfb1b378aa6", "No Containers"));
				}
				else if (containersToSelectFrom.Count == 1 && !includeUnContainerised)
				{
					containersToPrintOptions = new ContainersToPrintOptions
					{
						ContainersToPrint = new[] { containersToSelectFrom[0].Container },
						IncludeUnContainerised = includeUnContainerised
					};
				}
				else
				{
					DocumentContainers docContainersBizObject = new DocumentContainers(containersToSelectFrom);

					var mode = includeUnContainerised
						? ContainerSelectorMode.PrintIncludingUncontainerised
						: ContainerSelectorMode.Print;

					using (DocumentContainersForm form = new DocumentContainersForm(docContainersBizObject, mode))
					{
						if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.Yes)
						{
							containersToPrintOptions = new ContainersToPrintOptions
							{
								ContainersToPrint = docContainersBizObject.ContainersToPrint.Cast<CommonContainer>().ToArray(),
								IncludeUnContainerised = includeUnContainerised && form.IncludeUnContainerised
							};
						}
					}
				}
			}

			return containersToPrintOptions;
		}
	}
}
