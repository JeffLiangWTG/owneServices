using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.XmlCredential;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public class GlbExternalPassword_SGA : GlbExternalPassword_SGv4
	{
		public GlbExternalPassword_SGA(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GP_PasswordType = PasswordTypesList.Codes.SGA;
		}

		public override ZString GP_UserID
		{
			get => base.GP_UserID;
			set
			{
				if (GP_UserID != value)
				{
					base.GP_UserID = value;

					if (!IsCopying)
					{
						if (GP_CurrentPassword.IsEmpty && GP_NextPassword.IsEmpty)
						{
							DefaultPasswordIfNeeded();
						}
					}
				}
			}
		}

		public override ZString ConfigurationName => SGCustomsConfiguration;

		protected override ZPropertyInfo[] CredentialApplicableInfos()
		{
			var result = new List<ZPropertyInfo>();
			result.Add(GP_CurrentPasswordInfo);
			result.Add(GP_UserIDInfo);
			if (!IsInDatabase || !GP_NextPassword.IsEmpty)
			{
				result.Add(GP_NextPasswordInfo);
			}
			return result.ToArray();
		}

		protected override object[] CreateCredentialItems()
		{
			var result = new List<object>();

			var currentDecryptedPassword = CurrentDecryptedPassword;
			if (!currentDecryptedPassword.IsEmpty)
			{
				result.Add(CredentialSender.CreateCredential(Constants.CredentialDetails.Current, GP_UserID, currentDecryptedPassword));
			}

			var nextDecryptedPassword = NextDecryptedPassword;
			if (!nextDecryptedPassword.IsEmpty)
			{
				result.Add(CredentialSender.CreateCredential(Constants.CredentialDetails.NextPassword, GP_UserID, nextDecryptedPassword));
			}

			return result.ToArray();
		}

		protected override ZString GetCredentialStatus()
		{
			return GP_PasswordStatus == Core.Constants.PasswordOK ? PasswordStatusList.Codes.Valid : PasswordStatusList.Codes.Invalid;
		}

		const string SGCustomsConfiguration = "SGCustomsConfiguration";
		internal const string SGCustomsAccount = "SGCustomsAccount";

		protected override GlbExternalPasswordValidation GetNewValidation()
		{
			return new GlbExternalPasswordValidation_SGA(this);
		}

		protected override string GetCredentialType()
		{
			return GP_PasswordType == PasswordTypesList.Codes.SGA
				? (ZString)SGCustomsAccount
				: GP_PasswordType;
		}

		#region DefaultPasswordIfNeeded

		void DefaultPasswordIfNeeded()
		{
			var validPasswords = GetPasswordsWithSameUserID().Where(c => c.GP_PasswordStatus == Core.Constants.PasswordOK).ToArray();

			if (validPasswords.Any())
			{
				var currentPassword = validPasswords.Where(c => !c.GP_CurrentPassword.IsEmpty).Select(c => c.CurrentDecryptedPassword).Distinct();
				var nextPassword = validPasswords.Where(c => !c.GP_NextPassword.IsEmpty).Select(c => c.NextDecryptedPassword).Distinct();

				var isSingleCurrentPassword = currentPassword.Count() == 1;
				var isSingleNextPassword = nextPassword.Count() == 1;

				if ((isSingleCurrentPassword || isSingleNextPassword) && (ShouldDefaultPasswordsEvent?.Invoke() ?? false))
				{
					if (isSingleCurrentPassword)
					{
						CurrentDecryptedPassword = currentPassword.First();
					}

					if (isSingleNextPassword)
					{
						NextDecryptedPassword = nextPassword.First();
					}

					GP_PasswordStatus = Core.Constants.PasswordOK;
				}
			}
		}

		public ShouldDefaultPasswordsEventHandler ShouldDefaultPasswordsEvent;

		public delegate ZBool ShouldDefaultPasswordsEventHandler();

		#endregion

		#region SyncPassword

		public void SyncPassword(IEnumerable<GlbExternalPassword_SGA> passwords)
		{
			if (passwords != null)
			{
				foreach (var password in passwords)
				{
					password.CurrentDecryptedPassword = CurrentDecryptedPassword;
					password.NextDecryptedPassword = NextDecryptedPassword;
					password.GP_PasswordStatus = GP_PasswordStatus;
				}
			}
		}

		public IEnumerable<GlbExternalPassword_SGA> GetPasswordsForSync()
		{
			var currentDecryptedPassword = CurrentDecryptedPassword;
			var nextDecryptedPassword = NextDecryptedPassword;
			var status = GP_PasswordStatus;

			return GetPasswordsWithSameUserID().Where(c => c.CurrentDecryptedPassword != currentDecryptedPassword || c.NextDecryptedPassword != nextDecryptedPassword || c.GP_PasswordStatus != status);
		}

		IEnumerable<GlbExternalPassword_SGA> GetPasswordsWithSameUserID()
		{
			var query = new ZQuery();
			query.AddToFilter(GlbExternalPasswordSchema.PK, SQLComparisonOperator.NotEqual, PK);

			query.AddToFilter(GlbExternalPasswordSchema.GP_GC, GP_GC);
			query.AddToFilter(GlbExternalPasswordSchema.GP_UserID, GP_UserID);
			query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, GP_PasswordType);

			return Factory.Load<GlbExternalPassword_SGA>(query).Where(p => IsLinkedWithValidStaff(p));
		}

		bool IsLinkedWithValidStaff(GlbExternalPassword_SGA password)
		{
			var staff = password.Staff;
			if (staff != null)
			{
				return staff.GS_IsActive && !staff.GS_IsDevice && staff.GS_CanLogin;
			}
			return false;
		}

		#endregion
	}
}
