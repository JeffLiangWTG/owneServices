using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class PostcodeOrderComparer : AlphanumericStringComparer
	{
		public PostcodeOrderComparer()
		{
		}

		Regex nonAlphaNumericRegex;
		Regex NonAlphaNumericRegex => nonAlphaNumericRegex ?? (nonAlphaNumericRegex = new Regex(@"[^a-zA-Z0-9]+"));

		public override int Compare(ZString xString, ZString yString)
		{
			xString = ReplaceSpecialCharsWithWhitespace(xString).Trim();
			yString = ReplaceSpecialCharsWithWhitespace(yString).Trim();
			return base.Compare(xString.ToUpper(), yString.ToUpper());
		}

		string ReplaceSpecialCharsWithWhitespace(string s)
		{
			return NonAlphaNumericRegex.Replace(s, " ");
		}
	}
}
