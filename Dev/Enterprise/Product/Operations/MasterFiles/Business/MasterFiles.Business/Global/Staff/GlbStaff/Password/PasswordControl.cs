using System;
using System.Collections.Specialized;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class PasswordControl
	{
		public PasswordControl()
		{
			DataRegistry registry = EnvProxy.Instance.Registry;
			fMinLength = registry.PasswordMinLength;
			fMinUpperAlphas = registry.PasswordMinUpperAlphas;
			fMinLowerAlphas = registry.PasswordMinLowerAlphas;
			fMinNumeric = registry.PasswordMinNumeric;
			fMinNonAlphNums = registry.PasswordMinNonAlphNums;

			fErrors = new StringCollection();
		}

		public bool IsValidPassword(string value)
		{
			fErrors.Clear();
			CheckMinLength(value);
			CheckMinUpperAlphas(value);
			CheckMinLowerAlphas(value);
			CheckMinNumeric(value);
			CheckMinNonAlphNums(value);
			return fErrors.Count == 0;
		}

		internal StringCollection Errors => fErrors;

		#region implementation

		readonly int fMinLength;
		readonly int fMinUpperAlphas;
		readonly int fMinLowerAlphas;
		readonly int fMinNumeric;
		readonly int fMinNonAlphNums;
		readonly StringCollection fErrors;

		void CheckMinLength(string value)
		{
			if (fMinLength > 0)
			{
				if (value.Length < fMinLength)
				{
					fErrors.Add(Res.GetString("a16739d3-3dff-40fe-b4d7-841160468c06", "Password must be at least {0} character(s).", fMinLength));
				}
			}
		}

		void CheckMinUpperAlphas(string value)
		{
			if (fMinUpperAlphas > 0)
			{
				int upperCount = 0;
				for (int i = 0; i < value.Length; i++)
				{
					if (Char.IsUpper(value, i))
					{
						upperCount++;
					}
				}

				if (upperCount < fMinUpperAlphas)
				{
					fErrors.Add(Res.GetString("68a321b1-4758-4818-8e85-767e419b3a6f", "Password must contain at least {0} uppercase character(s).", fMinUpperAlphas));
				}
			}
		}

		void CheckMinLowerAlphas(string value)
		{
			if (fMinLowerAlphas > 0)
			{
				int lowerCount = 0;
				for (int i = 0; i < value.Length; i++)
				{
					if (Char.IsLower(value, i))
					{
						lowerCount++;
					}
				}

				if (lowerCount < fMinLowerAlphas)
				{
					fErrors.Add(Res.GetString("03d62191-062a-4a33-88c1-7bcd4da13ceb", "Password must contain at least {0} lowercase character(s).", fMinLowerAlphas));
				}
			}
		}

		void CheckMinNumeric(string value)
		{
			if (fMinNumeric > 0)
			{
				int numericCount = 0;
				for (int i = 0; i < value.Length; i++)
				{
					if (Char.IsNumber(value, i))
					{
						numericCount++;
					}
				}

				if (numericCount < fMinNumeric)
				{
					fErrors.Add(Res.GetString("333111c8-ea80-415b-822c-37dfa6fa4223", "Password must contain at least {0} numeric digit(s).", fMinNumeric));
				}
			}
		}

		void CheckMinNonAlphNums(string value)
		{
			if (fMinNonAlphNums > 0)
			{
				int nonAlphNumCount = 0;
				for (int i = 0; i < value.Length; i++)
				{
					if (!Char.IsLetterOrDigit(value, i))
					{
						nonAlphNumCount++;
					}
				}

				if (nonAlphNumCount < fMinNonAlphNums)
				{
					fErrors.Add(Res.GetString("2c247334-d7db-4e5f-af8f-8a95ddaff33d", "Password must contain at least {0} non-alphanumeric characters(s).", fMinNonAlphNums));
				}
			}
		}
		#endregion

		#region NewPassword Validation

		public bool NewPasswordIsValid(GlbStaff staff, string oldPassword, string newPassword1, string newPassword2, out string error)
		{
			bool result = false;
			error = "";
			if (newPassword1 != newPassword2)
			{
				// This need to be checked regardless it is AD or not
				error = Res.GetString("ba87f885-c7ab-4cc2-928c-36037e17bc40", "Confirm Password does not match New Password.");
				return false;
			}

			if (staff.ShouldUseADPasswordPolicy)
			{
				// AD will validate against domain password policy during password change/reset and should not check against local password policy
				return true;
			}

			if (OldPasswordIsValid(staff, oldPassword))
			{
				if (NewPasswordIsNotEmpty(newPassword1))
				{
					if (newPassword1 == newPassword2)
					{
						if (IsValidPassword(newPassword1))
						{
							if ((oldPassword == null) || !staff.HasPasswordBeenUsed(newPassword1))
							{
								result = true;
								error = "";
							}
							else
							{
								error = Res.GetString("b2cd19d1-8d7a-4f6a-a799-e80b34f3f9b6", "You previously used this password, please choose a new password.");
							}
						}
						else
						{
							error = Errors[0];
						}
					}
					else
					{
						error = Res.GetString("ba87f885-c7ab-4cc2-928c-36037e17bc40", "Confirm Password does not match New Password.");
					}
				}
				else
				{
					error = Res.GetString("4edf97ff-cd1f-48f4-9b10-d24edd1a0632", "Please enter New Password.");
				}
			}
			else
			{
				error = Res.GetString("cb5a9fbb-e22e-4258-8f8b-0dd94d0609ff", "Old Password is incorrect. Passwords are case-sensitive.");
			}

			return result;
		}

		bool OldPasswordIsValid(GlbStaff staff, string oldPassword)
			=> oldPassword == null || staff.VerifyPassword(oldPassword);

		bool NewPasswordIsNotEmpty(string password)
		{
			return !string.IsNullOrEmpty(password.Trim());
		}

		#endregion
	}
}
