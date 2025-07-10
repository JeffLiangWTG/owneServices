using CargoWise.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ContainerWithPackLinesDataObjectWriter<TConsol> : ContainerDataObjectWriter where TConsol : CommonConsol
	{
		public ContainerWithPackLinesDataObjectWriter(IContainerLinkManager<TConsol> containerLinkManager, BindToLists listCache, IDataWritingManager manager)
			: base(listCache, manager)
		{
			this.containerLinkManager = Argument.NotNull(containerLinkManager, "containerLinkManager");
		}

		readonly IContainerLinkManager<TConsol> containerLinkManager;

		protected override Container PopulateDataObject(CommonContainer containerBO)
		{
			var containerDataObject = base.PopulateDataObject(containerBO);

			containerLinkManager.AllocateContainerLink(containerBO, containerDataObject);

			return containerDataObject;
		}
	}
}
