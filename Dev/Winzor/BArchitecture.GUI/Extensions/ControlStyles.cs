namespace WinzorFramework.Extensions;

public struct ControlStyleOptions
{
	public bool BackgroundImage
	{
		get => !noBackgroundImage;
		set => noBackgroundImage = !value;
	}

	bool noBackgroundImage;
}
