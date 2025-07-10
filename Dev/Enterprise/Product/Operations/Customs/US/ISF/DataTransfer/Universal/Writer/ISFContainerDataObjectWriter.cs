using System.Collections.Generic;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal.Writer
{
	class ISFContainerDataObjectWriter : DataObjectWriter<CusISFEquip, Container>
	{
		public ISFContainerDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override Container PopulateDataObject(CusISFEquip sourceBO)
		{
			var container = new Container(writeManager.WriterStrategy);
			PopulateDataObject(sourceBO, container);
			return container;
		}

		void PopulateDataObject(CusISFEquip sourceBO, Container container)
		{
			container.ContainerNumber = sourceBO.BE_ContainerNum;
			var addInfoCollection = new List<AddInfo>();
			if (!sourceBO.BE_EquipCode.IsEmpty)
			{
				addInfoCollection.Add(AddInfo.New(ISFConstants.ContainerConstants.USContainerType, sourceBO.BE_EquipCode));
			}
			if (!sourceBO.BE_ContainerISO.IsEmpty)
			{
				addInfoCollection.Add(AddInfo.New(ISFConstants.ContainerConstants.ISOSizeTypeCode, sourceBO.BE_ContainerISO));
			}
			container.AddInfoCollection = addInfoCollection.Count == 0 ? null : addInfoCollection;
		}
	}
}
