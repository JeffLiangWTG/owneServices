namespace Enterprise.Customs.ZA.Business.BatchProcessor
{
	public class ZACharacterSetNoCasing : ZACharacterSet
	{
		public new const string ValidCharacters = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,-()/='+:=?""!%&*;<>";

		public ZACharacterSetNoCasing() : base()
		{
		}

		protected override string ReplaceIllegalCharacters(string element)
		{
			return KeepChars(element, ValidCharacters, " ");
		}

		protected override string EnforceCase(string dataPiece)
		{
			return dataPiece;
		}
	}
}
