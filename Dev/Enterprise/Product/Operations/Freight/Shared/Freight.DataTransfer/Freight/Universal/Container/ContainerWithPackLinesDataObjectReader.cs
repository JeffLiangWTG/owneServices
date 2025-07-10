using System;
using CargoWise.Common;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ContainerWithPackLinesDataObjectReader<TContainer, TConsol> : ContainerWithVGMDataObjectReader<TContainer>
		where TContainer : CommonContainer
		where TConsol : CommonConsol
	{
		public ContainerWithPackLinesDataObjectReader(Container containerDataObject
			, IXmlImportLogger logger
			, UniversalObjectFactory factory
			, IContainerLinkManager<TConsol> linkManager
			, Func<Container, TContainer> containerBizObjProvider = null
			, Func<Container, TContainer> containerBizObjCreator = null
			, Action<TContainer> containerTypeSetter = null)
			: base(containerDataObject, logger, factory, containerBizObjProvider, containerBizObjCreator)
		{
			this.linkManager = Argument.NotNull(linkManager, "linkManager");
			this.containerTypeSetter = containerTypeSetter;
		}

		readonly IContainerLinkManager<TConsol> linkManager;
		readonly Action<TContainer> containerTypeSetter;

		protected override void ImportReferenceContainerInfo(TContainer container)
		{
			if (containerTypeSetter != null)
			{
				containerTypeSetter(container);
				SetValue(container, JobContainerSchema.JC_ContainerCount, dataObject.ContainerCount);
			}
			else
			{
				base.ImportReferenceContainerInfo(container);
			}
		}

		protected override void PopulateBusinessObject(TContainer container)
		{
			base.PopulateBusinessObject(container);
			linkManager.CollectContainerLink(container, dataObject);
		}
	}
}
