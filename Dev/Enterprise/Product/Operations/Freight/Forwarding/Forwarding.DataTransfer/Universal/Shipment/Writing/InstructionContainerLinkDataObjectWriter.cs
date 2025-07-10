using System.Collections.Generic;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class InstructionContainerLinkDataObjectWriter : DataObjectWriter<CommonPickupDeliveryConfirm, InstructionContainerLink>
	{
		public InstructionContainerLinkDataObjectWriter(IDataWritingManager writeManager, IContainerLinkManager<ForwardingConsol> containerLinkManager) : base(writeManager)
		{
			this.containerLinkManager = containerLinkManager;
		}

		readonly IContainerLinkManager<ForwardingConsol> containerLinkManager;

		protected override InstructionContainerLink PopulateDataObject(CommonPickupDeliveryConfirm sourceBO)
		{
			var link = new InstructionContainerLink();
			link.ContainerLink = containerLinkManager.GetContainerLink(sourceBO.Container);
			link.ConfirmationCollection = new List<Confirmation>();

			var confirmation = new PickupDeliveryConfirmationDataObjectWriter(writeManager).GetDataObject(sourceBO);
			link.ConfirmationCollection.Add(confirmation);

			return link;
		}
	}
}
