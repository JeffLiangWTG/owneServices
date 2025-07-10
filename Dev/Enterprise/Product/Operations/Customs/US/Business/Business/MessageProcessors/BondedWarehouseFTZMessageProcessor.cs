using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	class BondedWarehouseFTZMessageProcessor : BondedWarehouseDeclarationMessageProcessor
	{
		internal BondedWarehouseFTZMessageProcessor(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<JobDeclaration, EmailDef, bool> sendMail)
			: base(messagePK, emailReportThatHasBeenDelayed)
		{
			this.sendMail = Argument.NotNull(sendMail, "sendMail");
		}
		readonly Action<JobDeclaration, EmailDef, bool> sendMail;

		protected new JobDeclaration declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		protected override bool HasBeenWithdrawn
		{
			get { return (declaration?.AdmissionStatus ?? ZString.Empty) == FTZMessageStatusList.Codes.ClearFTZAdmissionDelete; }
		}

		protected override bool IsAmendmentError
		{
			get { return (declaration?.AdmissionStatus ?? ZString.Empty) == FTZMessageStatusList.Codes.ErrorFTZAdmissionAmend; }
		}

		protected override bool IsAmendmentClear
		{
			get { return (declaration?.AdmissionStatus ?? ZString.Empty) == FTZMessageStatusList.Codes.ClearFTZAdmissionAmend; }
		}

		protected override bool IsOriginalError
		{
			get { return (declaration?.AdmissionStatus ?? ZString.Empty) == FTZMessageStatusList.Codes.ErrorFTZAdmissionAdd; }
		}

		protected override bool IsWithdrawalError
		{
			get { return (declaration?.AdmissionStatus ?? ZString.Empty) == FTZMessageStatusList.Codes.ErrorFTZAdmissionDelete; }
		}

		protected override void SendEmailCore(EmailDef email)
		{
			if (declaration != null)
			{
				sendMail(declaration, email, IsOriginalError || IsAmendmentError || IsWithdrawalError);
			}
		}

		protected override string GetReferenceDetail()
		{
			if (declaration != null)
			{
				return string.Format(@"<strong>Declaration Reference: {0}<br />
Admision Number: {1}<br />", EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference), declaration.FTZAdmissionNumber);
			}
			return ZString.Empty;
		}
	}
}
