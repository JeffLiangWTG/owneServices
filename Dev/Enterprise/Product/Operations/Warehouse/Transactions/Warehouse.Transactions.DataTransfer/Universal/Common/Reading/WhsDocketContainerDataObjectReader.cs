using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsDocketContainerDataObjectReader : BaseContainerDataObjectReader<WhsDocketContainer>
	{
		internal WhsDocketContainerDataObjectReader(Container containerDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, WhsDocket parent)
			: base(containerDataObject, logger, factory)
		{
			this.parent = parent;
		}

		readonly WhsDocket parent;

		protected override WhsDocketContainer GetExistingBusinessObject()
		{
			if (parent != null)
			{
				var containerNumber = dataObject.ContainerNumber.GetValueOrDefault();
				if (!containerNumber.IsEmpty)
				{
					foreach (WhsDocketContainer container in parent.Containers)
					{
						if (container.WC_ContainerNum == containerNumber)
						{
							return container;
						}
					}
				}
			}

			return null;
		}

		protected override void PopulateBusinessObject(WhsDocketContainer containerBO)
		{
			SetValue(containerBO, WhsDocketContainerSchema.WC_ContainerNum, dataObject.ContainerNumber);
			SetValue(containerBO, WhsDocketContainerSchema.WC_IsChargeable, dataObject.IsChargeable);
			SetValue(containerBO, WhsDocketContainerSchema.WC_IsPalletised, dataObject.IsPalletised);
			SetValue(containerBO, WhsDocketContainerSchema.WC_ItemCount, dataObject.ItemCount);
			SetValue(containerBO, WhsDocketContainerSchema.WC_PalletCount, dataObject.PalletCount);
			SetValue(containerBO, WhsDocketContainerSchema.WC_SealNum, dataObject.Seal);
			SetValue(containerBO, WhsDocketContainerSchema.WC_RC, dataObject.ContainerType);
		}
	}
}