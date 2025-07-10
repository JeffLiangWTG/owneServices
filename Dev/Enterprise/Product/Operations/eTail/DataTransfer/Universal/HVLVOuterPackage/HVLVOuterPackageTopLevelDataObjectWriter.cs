using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Universal
{
	public class HVLVOuterPackageTopLevelDataObjectWriter : TopLevelDataObjectWriter<HVLVOuterPackage, UniversalShipment>
	{
		public HVLVOuterPackageTopLevelDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.HVLVOuterPackage;

		protected override void PopulateDataObject(HVLVOuterPackage sourceBO, UniversalShipment dataObject)
		{
			var outerPackageWriter = new HVLVOuterPackageWriter(writeManager, dataObject);
			var outerPackagePackingLine = outerPackageWriter.GetDataObject(sourceBO);

			dataObject.CarrierServiceLevel = new ServiceLevel() { Code = sourceBO.HVO_PL_NKLastMileCarrierServiceLevel };
			dataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { outerPackagePackingLine });
		}
	}
}
