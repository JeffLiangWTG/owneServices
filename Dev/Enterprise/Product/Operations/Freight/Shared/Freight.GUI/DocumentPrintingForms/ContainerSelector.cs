using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Macros;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	sealed class ContainerSelector : IContainerSelector
	{
		public Either<string, CommonContainer[]> SelectContainers(CommonContainer[] containers, ContainerSelectorMode mode = ContainerSelectorMode.Print)
		{
			if (containers == null
				|| containers.Length == 0
				|| (containers.Length == 1 && mode != ContainerSelectorMode.ExportPreAdviceNotification))
			{
				return containers;
			}

			var containersToSelectFrom = new ContainerToSelectFromForPrintingCollection(containers.First().Factory);

			var isUnselectingContainers = false;

			void UnselectAllOtherContainers(object sender, EventArgs e)
			{
				if (isUnselectingContainers)
				{
					return;
				}

				if (sender is ContainerToSelectFromForPrinting clickedContainer
					&& clickedContainer.JC_Calc_PrintDocumentForContainer)
				{
					isUnselectingContainers = true;

					foreach (ContainerToSelectFromForPrinting container in containersToSelectFrom)
					{
						if (container != clickedContainer)
						{
							container.JC_Calc_PrintDocumentForContainer = false;
						}
					}
				}

				isUnselectingContainers = false;
			}

			foreach (var container in containers)
			{
				var containerToSelect = new ContainerToSelectFromForPrinting(container);
				containersToSelectFrom.Add(containerToSelect);

				if (mode == ContainerSelectorMode.PrintSingle || mode == ContainerSelectorMode.CMRConsignmentNote)
				{
					containerToSelect.JC_Calc_PrintDocumentForContainer = containersToSelectFrom.Count == 1;
					containerToSelect.JC_Calc_PrintDocumentForContainerInfo.ValueChanged += UnselectAllOtherContainers;
				}

				else
				{
					containerToSelect.JC_Calc_PrintDocumentForContainer = mode == ContainerSelectorMode.AMQ
																		 || mode == ContainerSelectorMode.LDE
																		 || mode == ContainerSelectorMode.LPD
																		 || mode == ContainerSelectorMode.CDM
																		 || mode == ContainerSelectorMode.TRC;
				}
			}

			var docContainersBizObject = new DocumentContainers(containersToSelectFrom);

			using (var form = new DocumentContainersForm(docContainersBizObject, mode))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.Yes)
				{
					return docContainersBizObject
						.ContainersToPrint
						.OfType<CommonContainer>()
						.ToArray();
				}

				return string.Empty;
			}
		}
	}
}
