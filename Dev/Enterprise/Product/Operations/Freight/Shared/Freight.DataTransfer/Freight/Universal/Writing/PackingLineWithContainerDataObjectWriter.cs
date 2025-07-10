using CargoWise.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class PackingLineWithContainerDataObjectWriter<TPackLine, TConsol> : PackingLineDataObjectWriter<TPackLine>
		where TPackLine : PackLine
		where TConsol : CommonConsol
	{
		public PackingLineWithContainerDataObjectWriter(IContainerLinkManager<TConsol> linkManager, BindToLists listCache, IDataWritingManager manager)
			: base(listCache, manager)
		{
			this.linkManager = Argument.NotNull(linkManager, "linkManager");
		}

		readonly IContainerLinkManager<TConsol> linkManager;

		protected override PackingLine PopulateDataObject(TPackLine packLineBO)
		{
			var packLineDataObject = base.PopulateDataObject(packLineBO);

			if (linkManager.Consol != null)
			{
				var container = packLineBO.GetContainer(linkManager.Consol);

				if (container == null
					&& linkManager.Consol.CouldBeAttachedToMultiAWBMaster
					&& linkManager.Consol.MasterConsol != null)
				{
					container = packLineBO.GetContainer(linkManager.Consol.MasterConsol);
				}
				if (container != null)
				{
					linkManager.SetContainerLink(container, packLineDataObject);
				}

				packLineDataObject.ContainerNumber = container != null
					? container.JC_ContainerNum
					: null;
			}

			return packLineDataObject;
		}
	}
}
