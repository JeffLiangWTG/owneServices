using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.DataTransfer.Universal;
using Enterprise.Freight.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public sealed class InBondDataContextManager : InBondDataContextManager<CusInBondHeader, CusInBondBill, CusInBondMoveHeader, CusInBondMoveDetail, CusInBondContainer, CusInBondCargoDesc>
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.InBond; }
		}

		protected override List<DataContextType> ParentDataContextTypes
		{
			get { return new List<DataContextType>() { DataContextType.ForwardingShipment, DataContextType.ForwardingConsol, DataContextType.CustomsDeclaration }; }
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new CusInBondHeaderDataObjectWriter(writeManager);
		}

		protected override CusInBondHeaderDataObjectReader<CusInBondHeader, CusInBondBill, CusInBondMoveHeader, CusInBondMoveDetail, CusInBondContainer, CusInBondCargoDesc> GetShipmentDataObjectReaderCore(Shipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory, ICusInBondParent parentBO)
		{
			return new CusInBondHeaderDataObjectReader(universalShipment, logger, factory, parentBO);
		}

		protected override ZBool IsSupportedTransportMode(ZString transportCode)
		{
			switch (transportCode)
			{
				case Enterprise.Customs.US.Business.TransportTypeList.Codes.Air:
				case Enterprise.Customs.US.Business.TransportTypeList.Codes.Rail:
				case Enterprise.Customs.US.Business.TransportTypeList.Codes.Truck:
				case Enterprise.Customs.US.Business.TransportTypeList.Codes.Sea:
				case Enterprise.Customs.US.Business.TransportTypeList.Codes.FixedTransportInstallations:
					return true;
				default:
					return false;
			}
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				new InBondDataEventContextReader(ParentBO).AddEventContextValues(result);
			}

			return result.Count == 0 ? null : result;
		}

		protected override ZString CusInBondApplicationCode
		{
			get { return CusInBondApplicationCodeList.Codes.InBond; }
		}
	}
}
