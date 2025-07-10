using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CustomsReferenceDataObjectReader : DataObjectReader<UniversalCustoms.CustomsReference>
	{
		public CustomsReferenceDataObjectReader(UniversalCustoms.CustomsReference customsReferenceDataObject, IXmlImportLogger logger, ZGuid parentPK, ZString parentTableCode, UniversalObjectFactory factory)
			: base(customsReferenceDataObject, logger, factory)
		{
			this.parentPK = parentPK;
			this.parentTableCode = parentTableCode;
		}

		public IColumnIndexer ReadIntoDataRowCusCodeData()
		{
			IColumnIndexer row = null;
			if (dataObject.Type != null && (dataObject.IsOverridden.HasValue || dataObject.Order.HasValue || dataObject.Reference.HasValue || dataObject.SubType != null))
			{
				var type = new CusCodeDataTypeDecider().GetTypeForLoad(dataObject.Type.GetCodeAsUpperCase(), parentTableCode, parentPK, factory.BOFactory);
				if (type != null)
				{
					row = CreateNewColumnIndexer(CusCodeDataSchema.PK, type);
					if (row != null)
					{
						SetValue(row, CusCodeDataSchema.CY_ParentID, parentPK);
						SetValue(row, CusCodeDataSchema.CY_ParentTableCode, parentTableCode);
						SetValue(row, CusCodeDataSchema.CY_Type, dataObject.Type);
						SetValue(row, CusCodeDataSchema.CY_Code, dataObject.SubType);
						SetValue(row, CusCodeDataSchema.CY_Data, dataObject.Reference);
						SetValue(row, CusCodeDataSchema.CY_IsOverridden, dataObject.IsOverridden);
						SetValue(row, CusCodeDataSchema.CY_Order, (ZShort?)dataObject.Order);
						SetValue(row, CusCodeDataSchema.CY_Date, GetSubmittedDateToCustoms(dataObject));
					}
				}
			}
			return row;
		}

		public virtual IColumnIndexer ReadIntoDataRowCusReference() => null;

		static ZDateTime GetSubmittedDateToCustoms(UniversalCustoms.CustomsReference customsReference)
		{
			return customsReference.DateCollection?.FirstOrDefault(x => x.Type.GetValueOrDefault() == DateType.DateAtOffice)?.Value.GetValueOrDefault() ?? ZDateTime.Empty;
		}

		protected readonly ZGuid parentPK;
		protected readonly ZString parentTableCode;
	}
}
