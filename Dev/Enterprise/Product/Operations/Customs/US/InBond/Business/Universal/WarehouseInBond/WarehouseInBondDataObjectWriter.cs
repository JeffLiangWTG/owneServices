using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public class WarehouseInBondDataObjectWriter : TopLevelDataObjectWriter<CusInBondMoveHeader, UniversalXml.Shipment>
	{
		public WarehouseInBondDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override void PopulateDataObject(CusInBondMoveHeader moveHeaderBO, UniversalXml.Shipment headerData)
		{
			new CusInBondHeaderDataObjectWriter(writeManager).PopulateMainData(moveHeaderBO.Header, headerData, moveHeaderBO);
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.WarehouseInBond;
		}
	}
}
