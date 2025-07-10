using System;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;

namespace Enterprise.Customs.PL.Business;

sealed class CC583ExportOperationProvider(BaseMessageSendingObject sendingObject, BaseMessageSendingObjectParent sendingObjectParent) : ICC583CExportOperation
{
	readonly BaseMessageSendingObject sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
	readonly BaseMessageSendingObjectParent sendingObjectParent = Argument.NotNull(sendingObjectParent, nameof(sendingObjectParent));

	public string Mrn => sendingObject.EntryNumber;

	public DateTime? ExitDate => sendingObjectParent.ExitDate.IsEmpty ? null : sendingObjectParent.ExitDate.ToDateTime();

	public string EnquiryInformationCode => sendingObjectParent.EnquiryInformationCode;
}
