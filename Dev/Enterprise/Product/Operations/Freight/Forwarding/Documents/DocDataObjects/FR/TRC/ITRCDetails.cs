using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	public interface ITRCDetails
	{
		BusinessObject BusinessObject { get; }
		ZString SourceType { get; }
		ZString SourceID { get; }
		CommonContainer[] Containers { get; }
		ZString PortOfOrigin { get; }
		ZString PortOfDestination { get; }
		RefUNLOCO OperationalPortImport { get; }
		RefUNLOCO OperationalPortExport { get; }
		ZString BookingConfirmationReference { get; }
		CodeDescriptionPairList ContainerModeList { get; }
		ZString ContainerMode { get; }
		CodeDescriptionPairList ShipmentTypeList { get; }
		ZString ShipmentType { get; }
		ZString WaybillNumber { get; }
		OrgAddress ReceivingForwarder { get; }
		OrgAddress SendingForwarder { get; }
		OrgAddress Carrier { get; }
		ZDateTime ETD { get; }
		ZDateTime ETA { get; }
	}
}
