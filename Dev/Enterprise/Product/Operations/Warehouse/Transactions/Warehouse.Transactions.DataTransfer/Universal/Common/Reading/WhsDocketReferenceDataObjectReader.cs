using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsDocketReferenceDataObjectReader : DataObjectReader<AdditionalReference, WhsDocketReference>
	{
		internal WhsDocketReferenceDataObjectReader(AdditionalReference referenceDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, WhsDocket parent)
			: base(referenceDataObject, logger, factory)
		{
			this.parent = Argument.NotNull(parent, nameof(WhsDocket) + " parent");
		}

		readonly WhsDocket parent;

		protected override WhsDocketReference GetExistingBusinessObject()
		{
			var type = dataObject.Type.GetCodeAsUpperCase();
			if (!type.IsEmpty)
			{
				foreach (WhsDocketReference reference in parent.References)
				{
					var number = reference.WX_Reference;
					if (reference.WX_RefType == type
						&& number == dataObject.ReferenceNumber.GetValueOrDefault())
					{
						return reference;
					}
				}
			}

			return null;
		}

		protected override void PopulateBusinessObject(WhsDocketReference referenceBO)
		{
			if (IsNewBO)
			{
				referenceBO.WX_WD = parent.PK;
				if (dataObject.Type.GetCodeAsUpperCase() == WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber
					&& parent.References.ToArray<WhsDocketReference>().Any(r => r.WX_RefType == WarehouseAdditionalReferenceTypes.Codes.ThirdPartyCarrierAccountNumber))
				{
					throw new DataObjectReadFailureException(Res.GetString("bc935146-e5ab-4139-8365-ab2b50219a63", "Unable to import duplicate TPC Reference: {0}.", dataObject.ReferenceNumber.GetValueOrDefault()));
				}
				else
				{
					SetValue(referenceBO, WhsDocketReferenceSchema.WX_Reference, dataObject.ReferenceNumber);
					SetValue(referenceBO, WhsDocketReferenceSchema.WX_RefType, dataObject.Type);
				}
			}
		}
	}
}
