using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;

namespace Enterprise.MarketingManager.GUI
{
	public static partial class WinHtmlEditorExtensionMethods
	{
		public static string GetFormatExtension(this ImageFormat imageFormat)
		{
			return extensionMapping.TryGetValue(imageFormat, out var extension) ? extension : ".unknown";
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "It is read only and an immutable dictionary")]
		static readonly ImmutableDictionary<ImageFormat, string> extensionMapping = new Dictionary<ImageFormat, string>()
		{
			{ ImageFormat.Jpeg, ".jpg" },
			{ ImageFormat.Png, ".png" },
			{ ImageFormat.Gif, ".gif" },
			{ ImageFormat.Bmp, ".bmp" },
			{ ImageFormat.Tiff, ".tif" },
			{ ImageFormat.Icon, ".ico" },
			{ ImageFormat.Emf, ".emf" },
			{ ImageFormat.Wmf, ".wmf" },
			{ ImageFormat.Exif, ".exif" },
			{ ImageFormat.MemoryBmp, ".bmp" },
		}.ToImmutableDictionary();
	}
}
