using CargoWise.EntityFramework;
using Enterprise.Integration.TransportCommon;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportCommon.DataTransfer.Universal
{
	public class TransportAdditionalReferenceCollectionReader<T> : AdditionalReferenceCollectionReader<BusinessObject, T>
		where T : BusinessObject, IStmNoteParent, ITransportAdditionalReferenceNumbers, IAdditionalReferenceNumberTypeProvider
	{
		public TransportAdditionalReferenceCollectionReader(DataObjectList<AdditionalReference> additionalReferenceDataObjects, IXmlImportLogger logger, UniversalObjectFactory factory, T parent)
			: base(additionalReferenceDataObjects, logger, factory, parent)
		{
		}

		#region IsInvalidAdditionalReferenceType

		protected override bool IsInvalidAdditionalReferenceType(AdditionalReference dataObject)
		{
			var isInvalidAdditionalReferenceType = true;
			var type = dataObject.Type.GetCodeAsUpperCase();
			var additionalReferenceNumberTypes = (Parent as IAdditionalReferenceNumberTypeProvider);

			if (additionalReferenceNumberTypes != null)
			{
				isInvalidAdditionalReferenceType = !additionalReferenceNumberTypes.GetAdditionalReferenceNumberTypeList("OTH", GlbCompany.CurrentCompany.GC_RN_NKCountryCode).ContainsCode(type);
			}
			return isInvalidAdditionalReferenceType;
		}

		#endregion

		#region Matching

		protected override BusinessObject[] BusinessObjects
		{
			get { return Parent.AdditionalReferenceNumbers.ToArray(); }
		}

		protected override BusinessObject FindMatchingBusinessObject(AdditionalReference dataObject)
		{
			return null;
		}

		#endregion

		#region Creating/Updating

		protected override void AddToCollection(BusinessObject additionalRefNumber)
		{
			Parent.AdditionalReferenceNumbers.Add(additionalRefNumber);
		}

		protected override void RemoveFromCollection(BusinessObject additionalRefNumber)
		{
			Parent.AdditionalReferenceNumbers.RemoveAndDelete((Integration.Customs.ICusEntryNumber)additionalRefNumber);
		}

		protected override BusinessObject ReadIntoBusinessObject(AdditionalReference dataObject, BusinessObject additionalRefNumber)
		{
			var reader = new TransportAdditionalReferenceDataObjectReader<T>(dataObject, Logger, Factory, Parent);
			return reader.ReadIntoBusinessObject();
		}

		#endregion
	}
}
