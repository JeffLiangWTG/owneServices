using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class PortReferenceReader<T> : DataObjectReader<PortReference, T>
		where T : BusinessObject
	{
		public PortReferenceReader(PortReference reference, IHavePortReferences parentBO, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(reference, logger, factory)
		{
			ParentBO = parentBO;
		}
		readonly IHavePortReferences ParentBO;

		protected override T GetExistingBusinessObject()
		{
			var finder = new PortReferenceBusinessObjectFinder<T>(dataObject);
			return finder.Find(ParentBO);
		}

		protected override void PopulateBusinessObject(T targetBO)
		{
			SetValue(targetBO, CusEntryNumSchema.CE_Category, TransitWarehouseReferenceCategories.Codes.PortReference);
			SetValue(targetBO, CusEntryNumSchema.CE_EntryType, dataObject.Type.GetCodeAsUpperCase());
			SetValue(targetBO, CusEntryNumSchema.CE_EntryNum, dataObject.Reference);
			SetValue(targetBO, CusEntryNumSchema.CE_RN_NKCountryCode, dataObject.Country.GetCodeAsUpperCase());

			if (!string.IsNullOrEmpty(dataObject.Status?.Code))
			{
				SetValue(targetBO, CusEntryNumSchema.CE_EntryStatus, dataObject.Status.Code);
			}
		}
	}
}
