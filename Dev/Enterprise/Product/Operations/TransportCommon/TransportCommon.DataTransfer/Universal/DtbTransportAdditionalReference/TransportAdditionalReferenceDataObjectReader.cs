using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.TransportCommon;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ICusEntryNumber = Enterprise.Integration.Customs.ICusEntryNumber;

namespace Enterprise.TransportCommon.DataTransfer.Universal
{
	public class TransportAdditionalReferenceDataObjectReader<T> : DataObjectReader<AdditionalReference, BusinessObject>
		where T : BusinessObject, IStmNoteParent, ITransportAdditionalReferenceNumbers, IAdditionalReferenceNumberTypeProvider
	{
		internal TransportAdditionalReferenceDataObjectReader(AdditionalReference referenceDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, T parent)
			: base(referenceDataObject, logger, factory)
		{
			this.parent = Argument.NotNull(parent, typeof(T).Name + " parent");
		}

		readonly T parent;

		#region GetNewBusinessObject

		protected override BusinessObject GetNewBusinessObject()
		{
			return (BusinessObject)parent.AdditionalReferenceNumbers.AddNew();
		}

		#endregion

		#region GetExistingBusinessObject

		protected override BusinessObject GetExistingBusinessObject()
		{
			var type = dataObject.Type.GetCodeAsUpperCase();
			if (!type.IsEmpty)
			{
				foreach (ICusEntryNumber reference in parent.AdditionalReferenceNumbers)
				{
					var number = reference.CE_EntryNum;
					if (reference.CE_EntryType == type
						&& number == dataObject.ReferenceNumber.GetValueOrDefault())
					{
						return (BusinessObject)reference;
					}
				}
			}

			return null;
		}

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(BusinessObject additionalReference)
		{
			const string otherType = "OTH";

			SetValue(additionalReference, CusEntryNumSchema.CE_Category, (ZString)otherType);
			SetValue(additionalReference, CusEntryNumSchema.CE_EntryLineReference, dataObject.ContextInformation);
			SetValue(additionalReference, CusEntryNumSchema.CE_EntryNum, dataObject.ReferenceNumber);
			SetValue(additionalReference, CusEntryNumSchema.CE_EntryType, dataObject.Type);
			SetValue(additionalReference, CusEntryNumSchema.CE_IssueDate, dataObject.IssueDate);
		}

		#endregion
	}
}
