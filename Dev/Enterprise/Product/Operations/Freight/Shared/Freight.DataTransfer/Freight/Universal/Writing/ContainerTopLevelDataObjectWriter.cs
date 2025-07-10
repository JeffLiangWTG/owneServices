using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ContainerTopLevelDataObjectWriter : TopLevelDataObjectWriter<CommonContainer, UniversalShipment>
	{
		public ContainerTopLevelDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override CargoWise.Types.ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.ForwardingContainer;
		}

		protected override void PopulateDataObject(CommonContainer sourceBO, UniversalShipment dataObject)
		{
			var parentBO = sourceBO.ContainerParent as BusinessObject;
			if (parentBO != null)
			{
				var parentDataWriter = GetContainerParentDataObjectWriter(sourceBO, parentBO);
				if (parentDataWriter != null)
				{
					parentDataWriter.PopulateContainer(parentBO, sourceBO, dataObject);
				}
			}
		}

		protected override void InsertParents(CommonContainer sourceBO, ref UniversalShipment dataObject)
		{
			var parentBO = sourceBO.ContainerParent as BusinessObject;
			if (parentBO != null)
			{
				var parentDataWriter = GetContainerParentDataObjectWriter(sourceBO, parentBO);
				if (parentDataWriter != null)
				{
					var parentDataObject = (UniversalShipment)parentDataWriter.GetContainerParentDataObject(parentBO);
					dataObject = GetTopLevelDataObjectWithParentLinked(dataObject, parentDataObject);
				}
			}
		}

		IContainerParentDataObjectWriter GetContainerParentDataObjectWriter(CommonContainer sourceBO, BusinessObject containerParentBO)
		{
			var parentDataContextManager = (IShipmentDataContextManager)containerParentBO.GetUniversalDataContextManager();
			return parentDataContextManager.GetShipmentDataObjectWriter(writeManager) as IContainerParentDataObjectWriter;
		}

		UniversalShipment GetTopLevelDataObjectWithParentLinked(UniversalShipment containerData, UniversalShipment parentDataObject)
		{
			parentDataObject.SetContainerCollection(() => containerData.ContainerCollection);
			return parentDataObject;
		}
	}
}
