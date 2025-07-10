using System;
using CargoWise.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class PackingLineWithContainerDataObjectReader<TPackLine, TShipment, TConsol> : PackingLineDataObjectReader<TPackLine, TShipment>
		where TPackLine : PackLine
		where TShipment : CommonShipment
		where TConsol : CommonConsol
	{
		public PackingLineWithContainerDataObjectReader(PackingLine packingLineDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, TShipment parentShipment, IContainerLinkManager<TConsol> linkManager, Func<PackingLine, TPackLine> packLineBizObjProvider = null)
			: base(packingLineDataObject, logger, factory, parentShipment, packLineBizObjProvider)
		{
			this.linkManager = Argument.NotNull(linkManager, "linkManager");
		}

		readonly IContainerLinkManager<TConsol> linkManager;

		protected override void PopulateBusinessObject(TPackLine packingLineBO)
		{
			base.PopulateBusinessObject(packingLineBO);
			linkManager.PackIntoContainer(packingLineBO, dataObject);

			foreach (CommonContainer container in packingLineBO.Containers)
			{
				SetValue(container, JobContainerSchema.JC_GrossWeight, container.JC_GrossWeight);
			}
		}

		protected override UNDGDataObjectReader GetUNDGDataObjectReader(UNDG undgDataObject, Func<UNDGDataItem> undgDataItemBizObjProvider)
		{
			return factoryOverride == null ? new ShipmentUNDGDataObjectReader(parentShipment.TransportMode, undgDataObject, logger, factory, undgDataItemBizObjProvider) : new ShipmentUNDGDataObjectReader(parentShipment.TransportMode, undgDataObject, logger, factoryOverride, undgDataItemBizObjProvider);
		}
	}
}
