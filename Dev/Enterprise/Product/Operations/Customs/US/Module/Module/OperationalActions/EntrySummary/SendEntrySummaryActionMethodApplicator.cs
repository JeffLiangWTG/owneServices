using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.OperationalAction;
using Enterprise.Environment;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Module.OperationalActions
{
	public class SendEntrySummaryActionMethodApplicator : USDeclarationOperationalActionMethodApplicator
	{
		public SendEntrySummaryActionMethodApplicator(BusinessObjectFactory factory)
			: base("Send Entry Summary operational action", factory)
		{ }

		public new abstract class Schema : USDeclarationOperationalActionMethodApplicator.Schema
		{
			public const string ContactName = "ContactName";
			public const int ContactNameMaxLength = 40;
			public const string ContactPhone = "ContactPhone";
			public const int ContactPhoneMaxLength = 15;
		}

		[MaxLength(Schema.ContactNameMaxLength)]
		public ZString ContactName
		{
			get => contactName;
			set
			{
				SetNonPersistentPropertyValue(ContactNameInfo, ref contactName, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateContactName();
				}
			}
		}
		ZString contactName;

		public ZPropertyInfo ContactNameInfo => GetZPropertyInfo(Schema.ContactName);

		[MaxLength(Schema.ContactPhoneMaxLength)]
		public ZString ContactPhone
		{
			get => contactPhone;
			set
			{
				SetNonPersistentPropertyValue(ContactPhoneInfo, ref contactPhone, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateContactPhone();
				}
			}
		}
		ZString contactPhone;

		public ZPropertyInfo ContactPhoneInfo => GetZPropertyInfo(Schema.ContactPhone);

		public new SendEntrySummaryActionMethodApplicatorValidation Validation => (SendEntrySummaryActionMethodApplicatorValidation)base.Validation;

		public override ValidationModes ValidationMode => ValidationModes.EntrySummary;

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			IsCancelled = false;
			var sendWithMessageErrors = SendWithMessageErrors && IsSendWithMessageErrorsAllowed;

			Validation.ValidateContactName();
			Validation.ValidateContactPhone();
			if (ContactNameInfo.HasNotifications() || ContactPhoneInfo.HasNotifications())
			{
				if (Globals.Message.Show("There is a notification. Are you sure you wish to continue?", "Send messages", MessageBoxButtons.OKCancel, DialogResult.Cancel) == DialogResult.Cancel)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "There is a notification, Cancel send messages.");
					IsCancelled = true;
				}
			}

			if (!IsCancelled)
			{
				var runner = new SendEntrySummaryOperationalActionRunner(log, ContactName, ContactPhone);
				jobsPK = new List<ZGuid>();
				jobsPK.AddRange(runner.PerformFunctionOperationalAction(sendWithMessageErrors, targets, ApportionWeight));
			}
		}

		protected override ZString MessageDescriptionCore => "Entry Summary";

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			var companyPK = Env.CurrentCompanyPK;
			var branchPK = Env.CurrentBranchPK;
			var declarationPK = ZGuid.Empty;

			if (targets != null && targets.Length == 1)
			{
				var declaration = Factory.Load<JobDeclaration>(targets[0]);
				companyPK = declaration.CompanyPK.ToGuid();
				branchPK = declaration.Branch.PK.ToGuid();
				declarationPK = declaration.PK;
			}

			var contact = MessageSenderContactDetailsDefaultingHelper.GetBrokerContact(Factory, declarationPK, companyPK, branchPK);
			if (contact != null)
			{
				ContactName = contact.GS_FullName.Left(Schema.ContactNameMaxLength);
				ContactPhone = MessageSenderContactDetailsDefaultingHelper.GetPhoneNumber(contact).Left(Schema.ContactPhoneMaxLength);
			}
		}

		protected override USActionMethodApplicatorValidation GetValidation() => new SendEntrySummaryActionMethodApplicatorValidation(this);
	}
}
