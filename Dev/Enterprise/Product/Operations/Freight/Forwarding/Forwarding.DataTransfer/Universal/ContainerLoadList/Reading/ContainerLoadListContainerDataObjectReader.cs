using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class ContainerLoadListContainerDataObjectReader : DataObjectReader<UniversalContainer, ForwardingContainer>
	{
		public ContainerLoadListContainerDataObjectReader(
			UniversalContainer containerDataObject,
			ForwardingConsol consol,
			ContainerLoadListContainerLinkManager containerLinkManager,
			IXmlImportLogger logger,
			UniversalObjectFactory factory) : base(containerDataObject, logger, factory)
		{
			this.consol = consol;
			this.containerLinkManager = containerLinkManager;
		}

		protected override LogType LogTypeForReasonNotAbleToUpdate => LogType.Warning;

		protected override ForwardingContainer GetExistingBusinessObject()
		{
			var matchedContainerList = new ConsolContainerMatcherForContainerLoadListLine(consol).GetMatchedList(dataObject);
			if (matchedContainerList.Count != 1)
			{
				return null;
			}

			return matchedContainerList[0];
		}

		protected override void PopulateBusinessObject(ForwardingContainer targetBO)
		{
			if (targetBO != null)
			{
				SetValue(targetBO, JobContainerSchema.JC_SealNum, dataObject.Seal);
				SetValue(targetBO, JobContainerSchema.JC_SealParty, dataObject.SealPartyType);
				SetValue(targetBO, JobContainerSchema.JC_AdditionalSealNum, dataObject.SecondSeal);
				SetValue(targetBO, JobContainerSchema.JC_AdditionalSealParty, dataObject.SecondSealPartyType);
				SetValue(targetBO, JobContainerSchema.JC_Additional2SealNum, dataObject.ThirdSeal);
				SetValue(targetBO, JobContainerSchema.JC_Additional2SealParty, dataObject.ThirdSealPartyType);

				containerLinkManager.CollectContainerLink(targetBO, dataObject);
			}
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(ForwardingContainer targetBO)
		{
			if (dataObject?.Link == null || dataObject.Link <= 0)
			{
				return Res.GetString("0888a167-e85a-4140-af88-3b99a417f364", "Container link must be valid value in {0}.", consol.JK_UniqueConsignRef);
			}

			if (containerLinkManager.GetContainer(dataObject.Link.Value) != null)
			{
				return Res.GetString("f635c8b9-43c8-4a0e-a7af-6872274bddf2", "Duplicated container link {0}.", dataObject.Link.Value);
			}

			if ((dataObject.ContainerNumber ?? ZString.Empty).IsEmpty)
			{
				return Res.GetString("0050c9ae-3bdf-4dff-8340-160c7258617e", "Container should have container number for container link {0}.", dataObject.Link.Value);
			}

			return base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
		}

		readonly ContainerLoadListContainerLinkManager containerLinkManager;
		readonly ForwardingConsol consol;
	}
}
