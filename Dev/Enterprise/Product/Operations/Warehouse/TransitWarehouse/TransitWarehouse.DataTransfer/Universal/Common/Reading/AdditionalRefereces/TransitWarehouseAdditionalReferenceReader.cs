using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class TransitWarehouseAdditionalReferenceReader<T> : DataObjectReader<TransitAdditionalReferenceInfo, T>
		where T : BusinessObject
	{
		public TransitWarehouseAdditionalReferenceReader(TransitAdditionalReferenceInfo additonalReference, IHaveCusEntryNumReferences parentBO, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(additonalReference, logger, factory)
		{
			ParentBO = parentBO;
		}
		readonly IHaveCusEntryNumReferences ParentBO;

		protected override T GetExistingBusinessObject()
		{
			var finder = new TransitWarehouseCusEntryNumReferenceBusinessObjectFinder<T>(dataObject);
			return finder.Find(ParentBO);
		}

		protected override void PopulateBusinessObject(T targetBO)
		{
			SetValue(targetBO, CusEntryNumSchema.CE_ParentID, ParentBO.PK);
			SetValue(targetBO, CusEntryNumSchema.CE_ParentTable, ParentBO.TableName);
			SetValue(targetBO, CusEntryNumSchema.CE_Category, dataObject.Category);
			SetValue(targetBO, CusEntryNumSchema.CE_EntryType, dataObject.Type);
			SetValue(targetBO, CusEntryNumSchema.CE_EntryNum, dataObject.Value);
			SetValue(targetBO, CusEntryNumSchema.CE_RN_NKCountryCode, dataObject.CountryCode);
		}
	}
}
