using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Universal.PackageState.Writer
{
	internal class WhsTransitPackageStateDataObjectWriter : TopLevelDataObjectWriter<WhsItemPackageState, UniversalShipment>
	{
		public WhsTransitPackageStateDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.TransitPackage;
		}

		protected override void PopulateDataObject(WhsItemPackageState sourceBO, UniversalShipment dataObject)
		{
		}
	}
}
