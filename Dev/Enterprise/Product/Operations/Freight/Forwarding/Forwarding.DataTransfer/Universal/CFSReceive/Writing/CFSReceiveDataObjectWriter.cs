using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class CFSReceiveDataObjectWriter : TopLevelDataObjectWriter<CFSReceive, UniversalShipment>
	{
		public CFSReceiveDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.CFSReceive;
		}

		protected override void PopulateDataObject(CFSReceive sourceBO, UniversalShipment dataObject) { }
	}
}
