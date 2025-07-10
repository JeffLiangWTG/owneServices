using System.Linq;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsDocketReferenceCollectionReader : AdditionalReferenceCollectionReader<WhsDocketReference, WhsDocket>
	{
		public WhsDocketReferenceCollectionReader(DataObjectList<AdditionalReference> additionalReferenceDataObjects, IXmlImportLogger logger, UniversalObjectFactory factory, WhsDocket parentDocket)
			: base(additionalReferenceDataObjects, logger, factory, parentDocket)
		{
		}

		protected override bool IsInvalidAdditionalReferenceType(AdditionalReference referenceDataObject)
		{
			var type = referenceDataObject.Type.GetCodeAsUpperCase();
			var additionalReferenceTypes = WarehouseDataRegistry.Instance.AdditionalReferenceType.Value;
			return !additionalReferenceTypes.ContainsCode(type);
		}

		#region Matching

		protected override WhsDocketReference[] BusinessObjects
		{
			get { return Parent.References.Cast<WhsDocketReference>().ToArray(); }
		}

		protected override WhsDocketReference FindMatchingBusinessObject(AdditionalReference dataObject)
		{
			return null;
		}

		#endregion

		#region Creating/Updating

		protected override WhsDocketReference ReadIntoBusinessObject(AdditionalReference referenceDataObject, WhsDocketReference businessObject)
		{
			var reader = new WhsDocketReferenceDataObjectReader(referenceDataObject, Logger, Factory, Parent);
			return reader.ReadIntoBusinessObject();
		}

		protected override void AddToCollection(WhsDocketReference docketReference)
		{
			Parent.References.Add(docketReference);
		}

		protected override void RemoveFromCollection(WhsDocketReference docketReference)
		{
			Parent.References.RemoveAndDelete(docketReference);
		}

		#endregion
	}
}