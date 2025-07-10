using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public class GlbExternalPasswordValidationBase_TW : GlbExternalPasswordWithCertificateValidation
	{
		public GlbExternalPasswordValidationBase_TW(GlbExternalPasswordBase_TW parent)
			: base(parent)
		{
		}

		protected new GlbExternalPasswordBase_TW Parent => (GlbExternalPasswordBase_TW)base.Parent;

		protected override void CheckGP_PasswordType()
		{
			base.CheckGP_PasswordType();
			if (Parent.GP_PasswordType.IsEmpty)
			{
				Parent.GP_PasswordTypeInfo.AddError(MandatoryValidation.MustBeEnteredMessage(Parent.GP_PasswordTypeInfo.HumanReadableName));
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.GP_PasswordTypeInfo);
			}
		}

		protected override void CheckCurrentDecryptedPassword()
		{
			base.CheckCurrentDecryptedPassword();
			CheckCurrentDecryptedPasswordCore();
		}

		protected virtual void CheckCurrentDecryptedPasswordCore()
		{
			MandatoryValidation.CheckEntered(Parent.CurrentDecryptedPasswordInfo);
		}

		public override void ValidateDuplicateConstraint()
		{
			if (!Parent.IsInDatabase ||
				Parent.GP_GSInfo.HasChanges ||
				Parent.GP_GGInfo.HasChanges ||
				Parent.GP_GCInfo.HasChanges ||
				Parent.GP_GBInfo.HasChanges ||
				Parent.GP_PasswordTypeInfo.HasChanges ||
				Parent.GP_UserIDInfo.HasChanges ||
				Parent.GP_MailBoxIDInfo.HasChanges)
			{
				var filter = new ZQuery(GlbExternalPasswordSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				filter.AddEmptyAsNullToFilter(GlbExternalPasswordSchema.GP_GS, Parent.GP_GS);
				filter.AddEmptyAsNullToFilter(GlbExternalPasswordSchema.GP_GG, Parent.GP_GG);
				filter.AddEmptyAsNullToFilter(GlbExternalPasswordSchema.GP_GC, Parent.GP_GC);
				filter.AddEmptyAsNullToFilter(GlbExternalPasswordSchema.GP_GB, Parent.GP_GB);
				filter.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, Parent.GP_PasswordType);
				filter.AddToFilter(GlbExternalPasswordSchema.GP_UserID, Parent.GP_UserID);
				filter.AddToFilter(GlbExternalPasswordSchema.GP_MailBoxID, Parent.GP_MailBoxID);
				if (Parent.Factory.LoadTop1<MasterFiles.Business.GlbExternalPassword>(filter) != null)
				{
					Parent.AddRowError(DuplicateCredentialFoundMessage);
				}
			}
		}
	}
}
