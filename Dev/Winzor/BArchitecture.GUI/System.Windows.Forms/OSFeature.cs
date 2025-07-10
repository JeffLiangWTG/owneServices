namespace System.Windows.Forms;

public class OSFeature
{
	public static readonly object LayeredWindows = new ();

	public static OSFeature Feature => new OSFeature();

	public Version GetVersionPresent(object feature) => new Version(1, 0);
}
