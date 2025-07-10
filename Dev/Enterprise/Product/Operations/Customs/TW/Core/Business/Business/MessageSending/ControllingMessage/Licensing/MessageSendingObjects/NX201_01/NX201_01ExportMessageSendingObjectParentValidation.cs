using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class NX201_01ExportMessageSendingObjectParentValidation : NX201_01MessageSendingObjectParentValidation
	{
		public NX201_01ExportMessageSendingObjectParentValidation(LicensingMessageSendingObjectParent parent, JobDeclaration declaration, IEnumerable<INotification> messageErrors, SecurityCheckpoint sendMessageWithErrorsSecurityCheckpoint, bool refreshValidation = true)
			: base(parent, declaration, messageErrors, sendMessageWithErrorsSecurityCheckpoint, refreshValidation)
		{
		}

		protected override MessageSendingNotificationCollection GetDeclarationgNotificationCollection(CusTWControllingMessageHeader header, string prefix)
		{
			var result = base.GetDeclarationgNotificationCollection(header, prefix);
			var importerDocumentaryAddress = Declaration.ImporterDocumentaryAddress;
			var finalDestinations = new JobDeclarationLookups(Declaration).FinalDestinations;
			finalDestinations.AdditionalFilter = new ZQuery(RefUNLOCOSchema.RL_Code, Declaration.JE_RL_NKFinalDestination);
			AddErrorIfEmptyOrNotInList(result, Declaration.Validation, Declaration.JE_RL_NKFinalDestinationInfo, finalDestinations.Select(c => c.RL_Code), prefix, errorMessageForEmpty: (NoResString)"You have not entered a Final Destination.");
			AddErrorIfEmpty(result, importerDocumentaryAddress.Validation, importerDocumentaryAddress.E2_CompanyNameInfo, prefix, columnName: (NoResString)"Importer Name");
			AddErrorIfEmptyOrNotInList(result, importerDocumentaryAddress.Validation, importerDocumentaryAddress.E2_RN_NKCountryCodeInfo, importerDocumentaryAddress.Lookups.Countries.Select(c => c.RN_Code), prefix);
			return result;
		}

		protected override MessageSendingNotificationCollection GetLicensingMessageSendingObjectNotificationCollection(LicensingMessageSendingObject sendingObject, CusTWControllingMessageHeader header, string prefix)
		{
			var result = base.GetLicensingMessageSendingObjectNotificationCollection(sendingObject, header, prefix);
			var applicantDocumentaryAddress = header.ApplicantDocumentaryAddress;
			AddErrorIfEmpty(result, applicantDocumentaryAddress.Validation, applicantDocumentaryAddress.E2_Address1Info, prefix, columnName: (NoResString)"Applicant Address");
			return result;
		}
	}
}
