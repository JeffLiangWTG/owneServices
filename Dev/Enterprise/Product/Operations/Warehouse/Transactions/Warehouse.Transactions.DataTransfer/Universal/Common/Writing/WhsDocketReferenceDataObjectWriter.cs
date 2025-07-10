using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsDocketReferenceDataObjectWriter : DataObjectWriter<WhsDocketReference, AdditionalReference>
	{
		internal WhsDocketReferenceDataObjectWriter(IDataWritingManager manager) : base(manager) { }

		protected override AdditionalReference PopulateDataObject(WhsDocketReference referenceBO)
		{
			var additionalReferenceDataObject = new AdditionalReference();
			additionalReferenceDataObject.Type = ListHelper.GetWithDescription<EntryType>(referenceBO.WX_RefType, referenceBO.Lookups.ReferenceTypes);
			additionalReferenceDataObject.ReferenceNumber = referenceBO.WX_Reference;

			return additionalReferenceDataObject;
		}
	}
}