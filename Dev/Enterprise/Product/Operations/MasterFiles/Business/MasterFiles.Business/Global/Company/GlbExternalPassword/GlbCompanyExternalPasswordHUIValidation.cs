using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbCompanyExternalPasswordHUIValidation : GlbExternalPasswordValidation
	{
		public GlbCompanyExternalPasswordHUIValidation(GlbCompanyExternalPasswordHUI parent)
			: base(parent)
		{
		}

		protected new GlbCompanyExternalPasswordHUI Parent => (GlbCompanyExternalPasswordHUI)base.Parent;

		#region Public Validate methods

		public void ValidateLogin() => ValidateCalculatedProperty(Parent.LoginInfo);

		public void ValidatePasswordHash() => ValidateCalculatedProperty(Parent.PasswordHashInfo);

		public void ValidateSignatureKey() => ValidateCalculatedProperty(Parent.SignatureKeyInfo);

		public void ValidateReplacementKey() => ValidateCalculatedProperty(Parent.ReplacementKeyInfo);

		public void ValidateHungaryCredentials()
		{
			ValidateLogin();
			ValidatePasswordHash();
			ValidateSignatureKey();
			ValidateReplacementKey();
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateHungaryCredentials();
		}

		public override void ValidateDuplicateConstraint()
		{
			// Duplicate checking omits the GP_UserID field for Hungary; only one credential is allowed per company.
			if (!Parent.IsInDatabase ||
				Parent.GP_GSInfo.HasChanges ||
				Parent.GP_GGInfo.HasChanges ||
				Parent.GP_GCInfo.HasChanges ||
				Parent.GP_GBInfo.HasChanges ||
				Parent.GP_PasswordTypeInfo.HasChanges)
			{
				var filter = new ZQuery(GlbExternalPasswordSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				filter.AddEmptyAsNullToFilter(GlbExternalPasswordSchema.GP_GS, Parent.GP_GS);
				filter.AddEmptyAsNullToFilter(GlbExternalPasswordSchema.GP_GG, Parent.GP_GG);
				filter.AddEmptyAsNullToFilter(GlbExternalPasswordSchema.GP_GC, Parent.GP_GC);
				filter.AddEmptyAsNullToFilter(GlbExternalPasswordSchema.GP_GB, Parent.GP_GB);
				filter.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, Parent.GP_PasswordType);
				if (Parent.Factory.LoadTop1<GlbExternalPassword>(filter) != null)
				{
					Parent.AddRowError(DuplicateCredentialFoundMessage);
				}
			}
		}

		#endregion

		#region Protected Check methods

		protected void CheckLogin()
		{
			if (!Parent.PasswordHash.IsEmpty
				|| !Parent.SignatureKey.IsEmpty
				|| !Parent.ReplacementKey.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.LoginInfo);
			}
		}

		protected void CheckPasswordHash()
		{
			if (!Parent.Login.IsEmpty
				|| !Parent.SignatureKey.IsEmpty
				|| !Parent.ReplacementKey.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.PasswordHashInfo);
			}

			if (!Parent.PasswordHash.IsEmpty && !IsHexStringOfBytes(Parent.PasswordHash))
			{
				Parent.PasswordHashInfo.AddError(Res.GetString("ddc657f4-babd-40e8-84d1-597cd3da1f71", "{0} must be a hexadecimal string (e.g. 'F45D6BA0702E2E5F').", Parent.PasswordHashInfo.HumanReadableName));
			}
		}

		protected void CheckSignatureKey()
		{
			if (!Parent.Login.IsEmpty
				|| !Parent.PasswordHash.IsEmpty
				|| !Parent.ReplacementKey.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.SignatureKeyInfo);
			}
		}

		protected void CheckReplacementKey()
		{
			if (!Parent.Login.IsEmpty
				|| !Parent.PasswordHash.IsEmpty
				|| !Parent.SignatureKey.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.ReplacementKeyInfo);
			}
		}

		protected override void CheckCurrentDecryptedPassword()
		{
			// Hungary stores binary data in CurrentDecryptedPassword / GP_CurrentPassword.
			// Overriding this to avoid running the EnglishCharactersValidation.ErrorIfNotWesternEuropean rule.
		}

		#endregion

		#region Implementation

		internal static bool IsHexStringOfBytes(string s)
			=> s.Length % 2 == 0
			&& s.All(ch => ValidHexCharacters.Contains(ch));

		const string ValidHexCharacters = "0123456789ABCDEFabcdef";

		#endregion
	}
}