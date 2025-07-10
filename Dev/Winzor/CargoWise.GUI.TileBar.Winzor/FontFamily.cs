namespace System.Windows.Media;

public class FontFamily
{
	public FontFamily(string familyName)
	{
		FamilyName = familyName;
	}

	public readonly string FamilyName;

	public override string ToString()
	{
		if (FamilyName == null)
		{
			return string.Empty;
		}
		return FamilyName;
	}
}
