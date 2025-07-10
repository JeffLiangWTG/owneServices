using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;
using UniversalContainerMode = Enterprise.UniversalDataBuss.DataObjects.Universal.ContainerMode;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ContainerLoadListContainerDataObjectWriter : DataObjectWriter<ForwardingContainer, UniversalContainer>
	{
		readonly ContainerLoadListContainerLinkManager containerLinkManager;

		public ContainerLoadListContainerDataObjectWriter(IDataWritingManager manager, ContainerLoadListContainerLinkManager containerLinkManager) : base(manager)
		{
			this.containerLinkManager = containerLinkManager;
		}

		protected override UniversalContainer PopulateDataObject(ForwardingContainer sourceBO)
		{
			var containerData = new UniversalContainer();
			containerData.ContainerNumber = sourceBO.JC_ContainerNum;
			containerData.FCL_LCL_AIR = ListHelper.GetWithDescription<UniversalContainerMode>(sourceBO.JC_ContainerMode, sourceBO.JC_ContainerMode_List);
			containerData.DeliveryMode = sourceBO.JC_DeliveryMode;
			containerData.ContainerType = ContainerType.New(sourceBO.RefContainer);
			containerData.ContainerCount = sourceBO.JC_ContainerCount;

			containerData.Seal = sourceBO.JC_SealNum;
			containerData.SealPartyType = new CodeDescriptionPair() { Code = sourceBO.JC_SealParty };
			if (!sourceBO.JC_AdditionalSealNum.IsEmpty)
			{
				containerData.SecondSeal = sourceBO.JC_AdditionalSealNum;
				containerData.SecondSealPartyType = new CodeDescriptionPair() { Code = sourceBO.JC_AdditionalSealParty };
			}
			if (!sourceBO.JC_Additional2SealNum.IsEmpty)
			{
				containerData.ThirdSeal = sourceBO.JC_Additional2SealNum;
				containerData.ThirdSealPartyType = new CodeDescriptionPair() { Code = sourceBO.JC_Additional2SealParty };
			}

			containerData.Link = containerLinkManager?.GetContainerLink(sourceBO) ?? 0;
			return containerData;
		}
	}
}
