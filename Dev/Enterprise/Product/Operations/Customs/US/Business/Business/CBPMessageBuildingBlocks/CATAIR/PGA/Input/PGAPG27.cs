using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input
{
	public partial class PGAPG27 : Messaging.Business.MessageBuildingBlocks.Input.Abstract.PGAPG27, IBIRDOGALineRecord
	{
		#region IBIRDOGALineRecord Members

		void IBIRDOGALineRecord.Update(IOGALine ogaLine, CargoWise.ComponentModel.INotifications notifications)
		{
			PGA pga = (PGA)ogaLine;

			CreateContainerAndRelateToPGALine(pga, ContainerEquipmentID);
			CreateContainerAndRelateToPGALine(pga, ContainerEquipmentID1);
			CreateContainerAndRelateToPGALine(pga, ContainerEquipmentID2);
		}

		void CreateContainerAndRelateToPGALine(PGA pga, ZString containerNumber)
		{
			if (!containerNumber.IsEmpty)
			{
				JobComInvoiceLine invoiceLine = pga.InvoiceLine;

				if (invoiceLine != null)
				{
					CusContainer container = invoiceLine.Declaration.CusContainers.Find(containerNumber);

					if (container == null)
					{
						container = invoiceLine.Declaration.CusContainers.AddNew();
						container.CO_ContainerNumber = containerNumber;
					}

					invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(containerNumber).IsForInvoiceLine = true;

					var relatedContainer = pga.ContainersForInvoiceLine.FindByContainerNumber(containerNumber);
					if (relatedContainer != null)
					{
						relatedContainer.IsForPGALine = true;
					}
				}
			}
		}

		#endregion
	}
}
