using CargoWise.EntityFramework;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal.Reader
{
	class ISFContainerDataObjectReader : DataObjectReader<Container, CusISFEquip>
	{
		public ISFContainerDataObjectReader(Container dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, CusISFHeader header)
			: base(dataObject, logger, factory)
		{
			this.header = header;
		}
		readonly CusISFHeader header;

		protected override CusISFEquip GetNewBusinessObject()
		{
			return header.Equipments.AddNew();
		}

		protected override CusISFEquip GetExistingBusinessObject()
		{
			CusISFEquip result = null;
			if (dataObject.ContainerNumber.HasValue)
			{
				var query = new ZQuery(CusISFEquipSchema.BE_BF, header.PK);
				query.AddToFilter(CusISFEquipSchema.BE_ContainerNum, dataObject.ContainerNumber.GetValueOrDefault());
				result = factory.LoadTop1<CusISFEquip>(query);
			}
			return result;
		}

		protected override void PopulateBusinessObject(CusISFEquip targetBO)
		{
			var equipRow = GetColumnIndexer(targetBO);
			SetValue(equipRow, CusISFEquipSchema.BE_ContainerNum, dataObject.ContainerNumber);
			if (dataObject.AddInfoCollection != null && dataObject.AddInfoCollection.Count > 0)
			{
				SetValue(equipRow, CusISFEquipSchema.BE_EquipCode, dataObject.AddInfoCollection.GetZStringValue(ISFConstants.ContainerConstants.USContainerType));
				SetValue(equipRow, CusISFEquipSchema.BE_ContainerISO, dataObject.AddInfoCollection.GetZStringValue(ISFConstants.ContainerConstants.ISOSizeTypeCode));
			}
		}
	}
}
