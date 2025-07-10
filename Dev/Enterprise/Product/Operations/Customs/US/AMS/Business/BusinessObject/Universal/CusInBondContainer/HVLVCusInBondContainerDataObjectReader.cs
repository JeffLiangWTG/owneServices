using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business.Universal
{
	public class HVLVCusInBondContainerDataObjectReader : DataObjectReader<Container, CusInBondContainer>
	{
		public HVLVCusInBondContainerDataObjectReader(Container containerDataObject, Shipment consignmentDataObject, PackingLine packingLineDataObject, CusInBondMoveDetail moveDetail, ForwardingShipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(containerDataObject, logger, factory)
		{
			this.containerDataObject = containerDataObject;
			this.packingLineDataObject = packingLineDataObject;
			this.consignmentDataObject = consignmentDataObject;
			this.moveDetail = moveDetail;
			this.shipment = shipment;
		}

		readonly Container containerDataObject;
		readonly PackingLine packingLineDataObject;
		readonly Shipment consignmentDataObject;
		readonly CusInBondMoveDetail moveDetail;
		readonly ForwardingShipment shipment;

		protected override CusInBondContainer GetExistingBusinessObject() => moveDetail.Containers.FirstOrDefault(container => container.BC_ContainerNum == containerDataObject.ContainerNumber.Value);

		protected override CusInBondContainer GetNewBusinessObject() => moveDetail.Containers.AddNew();

		protected override void PopulateBusinessObject(CusInBondContainer targetBO)
		{
			var containerRow = GetColumnIndexer(targetBO);

			SetValue(containerRow, CusInBondContainerSchema.BC_ContainerNum, containerDataObject.ContainerNumber?.ToUpper());
			SetValue(containerRow, CusInBondContainerSchema.BC_Seal1, containerDataObject.Seal?.ToUpper());
			SetValue(containerRow, CusInBondContainerSchema.BC_TypeOfService, GetTypeOfService(containerDataObject.DeliveryMode.GetValueOrDefault()));

			if (!string.IsNullOrEmpty(containerDataObject.ContainerType?.Code))
			{
				var refContainer = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerDataObject.ContainerType.Code.Value);
				SetValue(containerRow, CusInBondContainerSchema.BC_RC, refContainer.PK);
			}

			FillCommodities(targetBO);
		}

		void FillCommodities(CusInBondContainer container)
		{
			new HVLVCusInBondCargeDescDataObjectReader(packingLineDataObject, consignmentDataObject, container, logger, factory).ReadIntoBusinessObject();
		}

		ZString GetTypeOfService(ZString deliveryMode)
		{
			var result = ZString.Empty;
			if (deliveryMode == Core.Constants.DeliveryModes.Codes.CY_CY)
			{
				result = ServiceTypeList.Codes.ContainerYard;
			}
			else if (deliveryMode == Core.Constants.DeliveryModes.Codes.CFS_CFS)
			{
				result = ServiceTypeList.Codes.ContainerStation;
			}
			else if (shipment.JS_PackingMode == Core.Constants.ContainerModes.BreakBulk || shipment.JS_PackingMode == Core.Constants.ContainerModes.Bulk)
			{
				result = ServiceTypeList.Codes.BreakBulk;
			}
			return result;
		}
	}
}
