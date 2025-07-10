namespace Enterprise.MasterFiles.Business.Organisation.Other.OrgCusCode.Validation.IL
{
	public static class LuhnAlgorithm
	{
		#region Luhn algorithm interface
		public static bool IsValidLastDigitChecksum(string ilVAT)
		{
			return Validate(ilVAT);
		}
		#endregion

		#region Luhn algorithm implementation
		static int Checksum(string code)
		{
			int LuhnDouble(int digit) => (digit *= 2) > 9 ? digit - 9 : digit;

			var len = code.Length;
			var parity = len % 2;
			var sum = 0;
			for (var i = len - 1;  i >= 0; i--)
			{
				var d = code[i] - '0';
				if (d < 0 || d > 9)
				{
					return -1;
				}
				sum += i % 2 == parity ? LuhnDouble(d) : d;
			}
			return sum % 10;
		}

		public static int Calculate(string partcode)
		{
			var checksum = Checksum(partcode + "0");
			return checksum == 0 ? 0 : 10 - checksum;
		}

		static bool Validate(string fullcode)
		{
			if (string.IsNullOrEmpty(fullcode) || fullcode.Length < 2)
			{
				return false;
			}
			return Checksum(fullcode) == 0;
		}
		#endregion
	}
}
