namespace Enterprise.Customs.TW.Business
{
	public partial class JobDeclarationMessageStatusList
	{
		public static bool IsAwaiting(string code)
		{
			return code == Codes.AWO ||
				code == Codes.AWC ||
				code == Codes.AWG ||
				code == Codes.AWE;
		}
	}
}
