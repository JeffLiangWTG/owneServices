using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	class TWSequenceformatterOnlyFirstCharCanHaveEnglishForA1 : TWSequenceformatterOnlyFirstCharCanHaveEnglish
	{
		public TWSequenceformatterOnlyFirstCharCanHaveEnglishForA1() : base(3916215, 5,
			new List<char> { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' },
			new List<char> { 'E', 'F', 'G', 'H', 'J', 'M', 'P', 'Q', 'R', 'U', 'V', 'W', 'X', 'Y', 'Z' })
		{
		}

		protected override int EnglishCharStartIndex => 9;

		public override ZString AllowedFormatDescription => Res.GetString("946A3B81-5C37-4205-BAF0-22DAEDA35193", "When the Sea Office of Receipt transships to the Air Office of Lading, please enter the Entry number manually. The first alphabet cannot be {0}.", string.Join(", ", Alphabets.Except(FirstAlphabets)));
	}
}
