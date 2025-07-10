//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSNHTSADocumentAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSNHTSADocumentAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USNHTSADocumentAddInfoValidation : AutoUSNHTSADocumentAddInfoValidation
	{
		public USNHTSADocumentAddInfoValidation(AutoUSNHTSADocumentAddInfo parent) : base(parent)
		{
		}

		new USNHTSADocumentAddInfo Parent
		{
			get { return (USNHTSADocumentAddInfo)base.Parent; }
		}

		NHTSADocument Document
		{
			get { return Parent.Document; }
		}

		NHTSAHeader Header
		{
			get { return Document.Header; }
		}

		protected override void CheckUS_NHTDocumentType()
		{
			base.CheckUS_NHTDocumentType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NHTDocumentTypeInfo, Parent.Lookups.DocumentTypes);

			if (Header != null && Header.IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NHTDocumentTypeInfo);

				var documentType = Parent.US_NHTDocumentType;
				if (!documentType.IsEmpty)
				{
					var hasMultipleDocumentsWithSameType = Header.NHTSADocuments.HasMultipleDocumentsWithSameType(documentType);
					if (hasMultipleDocumentsWithSameType)
					{
						Parent.US_NHTDocumentTypeInfo.AddMessageError(ValidationConstants.NHTSA.DuplicateDocumentWithSameType(documentType));
					}
				}

				Header.AddInfo.Validation.ValidateUS_NHTBoxNumber();
			}
		}

		protected override void CheckUS_NHTDocumentOwner()
		{
			base.CheckUS_NHTDocumentOwner();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NHTDocumentOwnerInfo, Parent.Lookups.OrganizationTypes);

			if (Header != null && Header.IsPGAValidationOn)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NHTDocumentOwnerInfo);
			}
		}
	}
}
