using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class AdditionalReferenceReader<T> : DataObjectReader<AdditionalReference, T>
		where T : BusinessObject
	{
		public AdditionalReferenceReader(AdditionalReference reference, IHaveCusEntryNumReferences parentBO, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(reference, logger, factory)
		{
			ParentBO = parentBO;
		}
		readonly IHaveCusEntryNumReferences ParentBO;

		protected override T GetExistingBusinessObject()
		{
			var finder = new AdditionalReferenceBusinessObjectFinder<T>(dataObject);
			return finder.Find(ParentBO);
		}

		protected override void PopulateBusinessObject(T targetBO)
		{
			SetValue(targetBO, CusEntryNumSchema.CE_Category, TransitWarehouseReferenceCategories.Codes.AdditionalReference);
			SetValue(targetBO, CusEntryNumSchema.CE_EntryType, dataObject.Type.GetCodeAsUpperCase());
			SetValue(targetBO, CusEntryNumSchema.CE_EntryNum, dataObject.ReferenceNumber);
		}
	}
}
