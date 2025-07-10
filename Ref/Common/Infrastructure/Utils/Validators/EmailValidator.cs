using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public static class EmailValidator
	{
		public static bool IsValidEmail(string email)
		{
			var emailPattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
			+ @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
			+ @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

			return Regex.IsMatch(email, emailPattern);
		}
	}
}
