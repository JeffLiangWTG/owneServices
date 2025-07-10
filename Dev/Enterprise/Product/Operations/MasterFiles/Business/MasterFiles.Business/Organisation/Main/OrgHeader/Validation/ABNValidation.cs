using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// ABNValidation allows validation of Australian Business Numbers
	/// </summary>
	public static class ABNValidation
	{
		public static readonly ZString ValidChars = "0123456789";

		public static bool CheckValidABN(ZString aBNRaw)
		{
			ZString aBN = aBNRaw.KeepChars(ValidChars);

			if (aBN.Length != 11 && aBN.Length != 14)
			{
				return false;
			}
			else
			{
				return ((int.Parse(aBN[0].ToString()) - 1) * 10 +
					int.Parse(aBN[1].ToString()) +
					int.Parse(aBN[2].ToString()) * 3 +
					int.Parse(aBN[3].ToString()) * 5 +
					int.Parse(aBN[4].ToString()) * 7 +
					int.Parse(aBN[5].ToString()) * 9 +
					int.Parse(aBN[6].ToString()) * 11 +
					int.Parse(aBN[7].ToString()) * 13 +
					int.Parse(aBN[8].ToString()) * 15 +
					int.Parse(aBN[9].ToString()) * 17 +
					int.Parse(aBN[10].ToString()) * 19) % 89 == 0;
			}
		}
	}
}
