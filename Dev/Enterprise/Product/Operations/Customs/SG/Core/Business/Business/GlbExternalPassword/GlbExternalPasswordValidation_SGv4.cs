using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public class GlbExternalPasswordValidation_SGv4 : GlbExternalPasswordValidation_SG
	{
		const int NextDecryptedPasswordInfoMinLength = 12;
		const int NextDecryptedPasswordInfoMaxLength = 24;

		public GlbExternalPasswordValidation_SGv4(GlbExternalPassword_SGv4 parent)
			: base(parent)
		{
		}

		protected override void CheckGP_UserID()
		{
			base.CheckGP_UserID();
			if (!Parent.GP_MailBoxID.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.GP_UserIDInfo, Res.GetString("7e08ef90-1b82-49ee-a27f-7d1c76956b77", "Declarant Code"));
			}
		}

		protected override void CheckGP_MailBoxID()
		{
			base.CheckGP_MailBoxID();
			if (!Parent.GP_MailBoxID.IsEmpty)
			{
				ZQuery query = new ZQuery(GlbExternalPasswordSchema.GP_MailBoxID, Parent.GP_MailBoxID);
				query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, Parent.GP_PasswordType);
				query.AddToFilter(GlbExternalPasswordSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

				if (Parent.Factory.ExistsInDatabase(GlbExternalPasswordSchema.Constants.TableName, query))
				{
					Parent.GP_MailBoxIDInfo.AddError(Res.GetString("48ada89c-239a-426c-8fe2-e4cb77cbbf6d", "Mailbox ID is in use elsewhere in the system.\r\nMailbox should be unique."));
				}

				ZString declarantCodeFirstChar = Parent.GP_MailBoxID.Left(1);
				if (declarantCodeFirstChar != "S" && declarantCodeFirstChar != "T" && declarantCodeFirstChar != "M" && declarantCodeFirstChar != "P")
				{
					Parent.GP_MailBoxIDInfo.AddWarning(Res.GetString("b69836fe-82bb-4df7-b97b-fde413f32d4b", "Declarant Code should start with 'S', T', 'M' or 'P'"));
				}
			}
		}

		protected override void CheckCurrentDecryptedPassword()
		{
			base.CheckCurrentDecryptedPassword();
			if (!Parent.GP_MailBoxID.IsEmpty)
			{
				ValidatePasswordFormat(Parent.CurrentDecryptedPassword, Parent.CurrentDecryptedPasswordInfo);
			}

			ValidateNextDecryptedPassword();
		}

		protected override void CheckNextDecryptedPassword()
		{
			base.CheckNextDecryptedPassword();
			if (!Parent.NextDecryptedPassword.IsEmpty)
			{
				ValidatePasswordFormat(Parent.NextDecryptedPassword, Parent.NextDecryptedPasswordInfo);
			}
		}

		void ValidatePasswordFormat(ZString passwordEntered, ZPropertyInfo passwordField)
		{
			ZInt passwordLength = passwordEntered.Length;
			var password = passwordEntered.ToString();
			if (!passwordLength.IsInRange(NextDecryptedPasswordInfoMinLength, NextDecryptedPasswordInfoMaxLength) || password.All(x => char.IsLetter(x)) || !password.Any(x => char.IsUpper(x)) || !password.Any(x => char.IsLower(x)))
			{
				passwordField.AddMessageError(Res.GetString("738BC5C7-8EE1-49D3-9A4A-FA02FF7CC7C2", "Password is required to be at least {0} and not greater than {1} characters, must contain at least one of each of capital, lowercase and non-alphabetic characters.", NextDecryptedPasswordInfoMinLength, NextDecryptedPasswordInfoMaxLength));
			}
		}
	}
}
