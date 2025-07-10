using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class NX201_01ImportMessageSendingObjectParentValidation : NX201_01MessageSendingObjectParentValidation
	{
		public NX201_01ImportMessageSendingObjectParentValidation(LicensingMessageSendingObjectParent parent, JobDeclaration declaration, IEnumerable<INotification> messageErrors, SecurityCheckpoint sendMessageWithErrorsSecurityCheckpoint, bool refreshValidation = true)
			: base(parent, declaration, messageErrors, sendMessageWithErrorsSecurityCheckpoint, refreshValidation)
		{
		}

		protected override MessageSendingNotificationCollection GetInvoiceLineNotificationCollection(JobComInvoiceLine invoiceLine, string prefix)
		{
			var result = base.GetInvoiceLineNotificationCollection(invoiceLine, prefix);
			AddErrorAfterValidation(result, invoiceLine.Validation.ValidateJI_CountryOfOrigin, invoiceLine.JI_CountryOfOriginInfo, prefix + (NoResString)": Goods Origin");
			AddErrorIfNotGreaterThan0(result, invoiceLine.Validation, invoiceLine.JI_EnteredUnitPriceInfo, prefix, columnName: (NoResString)"Unit Price");
			return result;
		}

		protected override MessageSendingNotificationCollection GetLicensingMessageSendingObjectNotificationCollection(LicensingMessageSendingObject sendingObject, CusTWControllingMessageHeader header, string prefix)
		{
			var result = base.GetLicensingMessageSendingObjectNotificationCollection(sendingObject, header, prefix);
			if (sendingObject.Action == NX201_01ActionCodeList.Codes._4 || sendingObject.Action == NX201_01ActionCodeList.Codes._17)
			{
				AddErrorIfEmpty(result, header.Validation, header.ProcessingNumberInfo, prefix, errorMessage: ValidationConstants.CusTWControllingMessageHeader.ProcessingNumberIsRequired);
			}
			return result;
		}
	}
}
