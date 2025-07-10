namespace Enterprise.Customs.SG.MHUB.Mhx4Soap.Encryption.Util
{
	class SecureRandomStringGenerator
	{
		const string initString = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456879_";

		public static string GetRandomString()
		{
			var randomString = new char[15];
			var initStringLength = initString.Length;

			for (int i = 0; i < 15; i++)
			{
				var nextCharPosition = SecureRandomNumber.Between(0, initStringLength - 1);
				randomString[i] = initString[nextCharPosition];
			}

			return new string(randomString);
		}
	}
}
