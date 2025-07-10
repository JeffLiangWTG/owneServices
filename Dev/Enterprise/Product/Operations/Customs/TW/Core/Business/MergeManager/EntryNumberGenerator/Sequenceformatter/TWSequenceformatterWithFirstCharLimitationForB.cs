using System.Collections.Generic;

namespace Enterprise.Customs.TW.Business
{
	class TWSequenceformatterWithFirstCharLimitationForB : TWSequenceformatterWithFirstCharLimitation
	{
		public TWSequenceformatterWithFirstCharLimitationForB() : base(232110, 4,
			new List<char> { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' },
			new List<char> { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' })
		{
		}

		protected override int EnglishCharStartIndex => 10;
	}
}
