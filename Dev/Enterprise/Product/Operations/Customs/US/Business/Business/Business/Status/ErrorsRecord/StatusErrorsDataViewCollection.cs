using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Integration.Customs.US;

namespace Enterprise.Customs.US.Business
{
	public class StatusErrorsDataViewCollection : Messaging.Business.StatusErrorsDataViewCollection, IBusinessObjectCollection<IErrorsRecord>, IStatusErrorsDataViewCollection
	{
		public StatusErrorsDataViewCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ErrorsRecord();
		}

		IErrorsRecord IBusinessObjectCollection<IErrorsRecord>.AddNew()
		{
			return (IErrorsRecord)base.AddNew();
		}

		IErrorsRecord IBusinessObjectCollection<IErrorsRecord>.this[int index]
		{
			get => (IErrorsRecord)base[index];
		}

		public IEnumerator<IErrorsRecord> GetEnumerator() => Elements.OfType<IErrorsRecord>().GetEnumerator();

		public void Populate(IEnumerable<DispositionData> dispositionCodes)
		{
			foreach (var data in dispositionCodes)
			{
				var record = (ErrorsRecord)AddNew();
				record.ErrorMessageIdentifier = data.US_Code;
				record.NarrativeMessage = GetAdditionalMessageText(data);
				record.StatusDate = data.US_DispositionDate;
				record.ReleaseDate = data.US_ReleaseDate;
				record.ReleaseOrigin = data.US_ReleaseOrigin;
				record.ReleaseOriginDescription = data.ReleaseOriginDesc;
			}
		}

		ZString GetAdditionalMessageText(DispositionData data)
		{
			var result = data.DispositionCodeDesc;
			if (data.US_Code == CargoReleaseProcessingResultList.Codes.DocumentRequired)
			{
				var documentType = data.US_DocumentType;
				if (documentType.IsEmpty)
				{
					result = "Document Required";
				}
				else
				{
					var documentTypeDesc = DocumentTypeCodeList.GetDescriptionFromDocumentType(Factory, documentType);
					result = ZString.Format("Doc Req./{0}", documentTypeDesc);
				}
			}
			return result;
		}
	}
}
