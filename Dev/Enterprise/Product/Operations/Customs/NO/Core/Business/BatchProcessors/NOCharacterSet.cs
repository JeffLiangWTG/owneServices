using Enterprise.Edifact;

namespace Enterprise.Customs.NO.Business;

public class NOCharacterSet : UNOACharacterSet
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Valid characters to use in EDIFACT")]
	public new const string ValidCharacters = "abcdefghijklmnopqrstuvwxyzæøåABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅàÀáÁäÄâÂèÈÊéêÉëËúÚüÜóÓöÖõÕ0123456789 .,-()/=\'+:=\\?!%&*;<>\"@#";

	public NOCharacterSet() : base()
	{
		validCharacters = ValidCharacters;
		ReplaceEscapedCharactersWithSpace = false;
	}

	protected override string ReplaceIllegalCharacters(string element)
	{
		return KeepChars(element, validCharacters, " ");
	}

	protected override string KeepCharsWithoutDiacritics(string value)
	{
		return KeepChars(value, validCharacters, " ");
	}

	protected override string EnforceCase(string dataPiece)
	{
		return dataPiece;
	}
}
