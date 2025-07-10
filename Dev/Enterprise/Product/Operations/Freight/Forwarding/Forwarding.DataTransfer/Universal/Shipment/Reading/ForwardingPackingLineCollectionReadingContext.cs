using System.Collections.Generic;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingPackingLineCollectionReadingContext
	{
		public DataObjectList<PackingLine> PackingLineDataObjectCollection { get; set; }
		public IXmlImportLogger Logger { get; set; }
		public UniversalObjectFactory Factory { get; set; }
		public ForwardingShipment ShipmentBO { get; set; }
		public IContainerLinkManager<ForwardingConsol> ContainerLinkManager { get; set; }
		public IOrderLineLinkManager OrderLineLinkManager { get; set; }
		public Dictionary<PackingLine, ForwardingPackLine> PackLineBOToPackingLineDOMap { get; set; }
		public bool DisableMatchOfExistingPackLine { get; set; }
	}
}
