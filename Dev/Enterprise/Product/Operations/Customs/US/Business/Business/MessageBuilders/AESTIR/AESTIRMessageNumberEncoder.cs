namespace Enterprise.Customs.US.Business
{
	public static class AESTIRMessageNumberEncoder
	{
		const string AcceptedChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		const int NoOfAcceptedChars = 36;

		public static string Encode(int messageNumber)
		{
			string encodedString = "";
			while (messageNumber > 0)
			{
				encodedString += AcceptedChar[messageNumber % NoOfAcceptedChars];
				messageNumber /= NoOfAcceptedChars;
			}
			return encodedString;
		}

		public static int Decode(string messageNumberString)
		{
			int decordeNumber = 0;
			for (int i = messageNumberString.Length; i > 0; i--)
			{
				int value = AcceptedChar.IndexOf(messageNumberString[i - 1]);
				decordeNumber *= NoOfAcceptedChars;
				decordeNumber += value;
			}
			return decordeNumber;
		}
	}
}
