namespace Enterprise.Customs.US.Business
{
	partial class AffirmationCodeConstants
	{
		public static bool IsPhoneNumber(string code)
		{
			switch (code)
			{
				case Codes.SPN:
				case Codes.SFX:
				case StandAlonePriorNotice.Codes.ConsigneePhoneNumber:
				case StandAlonePriorNotice.Codes.ConsigneeFax:
				case StandAlonePriorNotice.Codes.ImporterPhoneNumber:
				case StandAlonePriorNotice.Codes.ImporterFax:
					return true;
				default:
					return false;
			}
		}
	}
}
