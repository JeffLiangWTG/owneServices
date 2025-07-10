using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	class BondedWarehouseInBondMessageProcessor : Customs.Business.MessageProcessors.BondedWarehouseMessageProcessor
	{
		internal BondedWarehouseInBondMessageProcessor(ZGuid messagePK, EmailDef emailReportThatHasBeenDelayed, Action<IInBondWarehouseIntegrationSupporter, EmailDef, bool> sendMail)
			: base(messagePK, emailReportThatHasBeenDelayed)
		{
			this.sendMail = Argument.NotNull(sendMail, "sendMail");
		}
		readonly Action<IInBondWarehouseIntegrationSupporter, EmailDef, bool> sendMail;

		new IInBondWarehouseIntegrationSupporter supporter
		{
			get { return (IInBondWarehouseIntegrationSupporter)base.supporter; }
		}

		protected override bool NeedToPublishEntryDetailsForOutward => false;

		protected override bool HasBeenWithdrawn
		{
			get { return supporter.HasBeenWithdrawn; }
		}

		protected override bool IsAmendmentError
		{
			get { return supporter.IsAmendmentError; }
		}

		protected override bool IsAmendmentClear
		{
			get { return supporter.IsAmendmentClear; }
		}

		protected override bool IsOriginalError
		{
			get { return supporter.IsOriginalError; }
		}

		protected override bool IsWithdrawalError
		{
			get { return supporter.IsWithdrawalError; }
		}

		protected override void SendEmailCore(EmailDef email)
		{
			sendMail(supporter, email, IsOriginalError || IsAmendmentError || IsWithdrawalError);
		}

		protected override string GetReferenceDetail()
		{
			return string.Format(@"<strong>Job Reference: {0}<br />
In-Bond Number: {1}<br />", EmailDefBuilder.GetJobLink(supporter, supporter.JobReference), supporter.InBondNumber);
		}

		protected override string GetSubject(string subjectPrefix)
		{
			return string.Format("{0} for Job Reference: {1}", subjectPrefix, supporter.JobReference);
		}

		protected override Customs.Business.WarehouseExtensions.IWarehouseIntegrationSupporter GetSupporter()
		{
			return message.EM_LinkedObject as Customs.Business.WarehouseExtensions.IWarehouseIntegrationSupporter;
		}
	}
}
