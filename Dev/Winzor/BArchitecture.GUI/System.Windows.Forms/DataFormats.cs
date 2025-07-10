namespace System.Windows.Forms;

[Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context")]
public static class DataFormats
{
	public const string Text = "Text";

	public const string UnicodeText = "UnicodeText";

	public const string Html = "Html";

	public const string Rtf = "Rtf";

	public const string CommaSeparatedValue = "CommaSeparatedValue";

	public const string Bitmap = "Bitmap";

	public const string FileDrop = "FileDrop";

	public const string WaveAudio = "WaveAudio";

	public static readonly string MetafilePict = "MetaFilePict";

	public static readonly string Serializable = "BlazorPersistentObject";

	public static readonly string Dib = "DeviceIndependentBitmap";

	public static readonly string Tiff = "TaggedImageFileFormat";

	public static readonly string EnhancedMetafile = "EnhancedMetafile";

	public static string StringFormat => typeof(string).FullName!;

	public static Format GetFormat(string format) => new Format(format, 0);

	public class Format
	{
		public Format(string name, int id)
		{
			Name = name;
			Id = id;
		}

		public int Id { get; }

		public string Name { get; }
	}
}
