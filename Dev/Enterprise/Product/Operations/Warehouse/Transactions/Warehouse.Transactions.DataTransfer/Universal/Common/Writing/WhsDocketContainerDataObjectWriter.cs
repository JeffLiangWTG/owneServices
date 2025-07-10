using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsDocketContainerDataObjectWriter : DataObjectWriter<WhsDocketContainer, Container>
	{
		internal WhsDocketContainerDataObjectWriter(IDataWritingManager manager, DataObjectList<Container> parentOrderContainers = null)
			: base(manager)
		{
			this.parentOrderContainers = parentOrderContainers;
		}

		readonly DataObjectList<Container> parentOrderContainers;

		protected override Container PopulateDataObject(WhsDocketContainer containerBO)
		{
			var containerDataObject = GetContainer(containerBO);
			containerDataObject.ContainerType = ContainerType.New(containerBO.Container);
			containerDataObject.ContainerNumber = containerBO.WC_ContainerNum;
			containerDataObject.IsChargeable = containerBO.WC_IsChargeable;
			containerDataObject.IsPalletised = containerBO.WC_IsPalletised;
			containerDataObject.ItemCount = containerBO.WC_ItemCount;
			containerDataObject.PalletCount = containerBO.WC_PalletCount;
			containerDataObject.Seal = containerBO.WC_SealNum;

			return containerDataObject;
		}

		Container GetContainer(WhsDocketContainer containerBO)
		{
			if (parentOrderContainers != null)
			{
				foreach (var container in parentOrderContainers)
				{
					var containerNumber = container.ContainerNumber.GetValueOrDefault();
					if (!containerNumber.IsEmpty
						&& containerNumber == containerBO.WC_ContainerNum)
					{
						parentOrderContainers.Remove(container);
						return container;
					}
				}
			}

			return new Container(writeManager.WriterStrategy);
		}
	}
}