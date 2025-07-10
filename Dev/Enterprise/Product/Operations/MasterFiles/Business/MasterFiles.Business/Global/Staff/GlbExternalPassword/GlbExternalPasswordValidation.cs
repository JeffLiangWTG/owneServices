using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbExternalPasswordValidation : AutoGlbExternalPasswordValidation
	{
		public GlbExternalPasswordValidation(AutoGlbExternalPassword parent)
			: base(parent)
		{
		}

		protected new GlbExternalPassword Parent
		{
			get { return (GlbExternalPassword)base.Parent; }
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateCurrentDecryptedPassword();
				ValidateNextDecryptedPassword();
				ValidateCurrentDecryptedCertificatePassphrase();
				ValidateDuplicateConstraint();
			}
		}

		public virtual void ValidateDuplicateConstraint()
		{
			if (!Parent.IsInDatabase ||
				Parent.GP_GSInfo.HasChanges ||
				Parent.GP_GGInfo.HasChanges ||
				Parent.GP_GCInfo.HasChanges ||
				Parent.GP_GBInfo.HasChanges ||
				Parent.GP_PasswordTypeInfo.HasChanges ||
				Parent.GP_UserIDInfo.HasChanges)
			{
				var filter = new ZQuery(GlbExternalPasswordSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				filter.AddEmptyAsNullToFilter(GlbExternalPasswordSchema.GP_GS, Parent.GP_GS);
				filter.AddEmptyAsNullToFilter(GlbExternalPasswordSchema.GP_GG, Parent.GP_GG);
				filter.AddEmptyAsNullToFilter(GlbExternalPasswordSchema.GP_GC, Parent.GP_GC);
				filter.AddEmptyAsNullToFilter(GlbExternalPasswordSchema.GP_GB, Parent.GP_GB);
				filter.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, Parent.GP_PasswordType);
				filter.AddToFilter(GlbExternalPasswordSchema.GP_UserID, Parent.GP_UserID);
				if (Parent.Factory.LoadTop1<GlbExternalPassword>(filter) != null)
				{
					Parent.AddRowError(DuplicateCredentialFoundMessage);
				}
			}
		}

		protected string DuplicateCredentialFoundMessage => Res.GetString("1E6EA96A-04D7-43B0-8CA7-AA853450538B", "Duplicate credential found; there is already another {0} with the same data.", Parent.HumanReadableName);

		#region CurrentDecryptedCertificatePassphrase

		public void ValidateCurrentDecryptedCertificatePassphrase()
		{
			ValidateCalculatedProperty(Parent.CurrentDecryptedCertificatePassphraseInfo);
		}

		protected virtual void CheckCurrentDecryptedCertificatePassphrase()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.CurrentDecryptedCertificatePassphraseInfo);
		}

		#endregion

		#region NextDecryptedPassword

		public void ValidateNextDecryptedPassword()
		{
			ValidateCalculatedProperty(Parent.NextDecryptedPasswordInfo);
		}

		protected virtual void CheckNextDecryptedPassword()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.NextDecryptedPasswordInfo);
		}

		#endregion

		#region CurrentDecryptedPassword

		public void ValidateCurrentDecryptedPassword()
		{
			ValidateCalculatedProperty(Parent.CurrentDecryptedPasswordInfo);
		}

		protected virtual void CheckCurrentDecryptedPassword()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.CurrentDecryptedPasswordInfo);
		}

		#endregion
	}
}
