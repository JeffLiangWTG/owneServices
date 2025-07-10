using System.Collections.Immutable;
using System.Drawing;
using System.Drawing.Imaging;

namespace WinzorFramework.Extensions;

public static class ImageToBase64Extension
{
	public static string? ToBase64DataUrl(this Image image)
	{
		if (!IsImageValid(image))
		{
			return string.Empty;
		}

		var imageInfo = GetImageInfo(image);
		if (imageInfo is null)
		{
			return null;
		}
		var base64 = Convert.ToBase64String(imageInfo.Data);
		mimeTypeLookup.Value.TryGetValue(imageInfo.ImageFormat.Guid, out var mime);
		return $"data:{mime};base64,{base64}";
	}

	public static string? ToBase64(this Image image)
	{
		if (!IsImageValid(image))
		{
			return string.Empty;
		}

		var imageInfo = GetImageInfo(image);
		if (imageInfo is null)
		{
			return null;
		}
		return Convert.ToBase64String(imageInfo.Data);
	}

	static bool IsImageValid(Image image)
	{
		return image?.IsDisposed() == false;
	}

	static ImageInfo? GetImageInfo(Image image)
	{
		var hashCode = image.GetHashCode().ToString();
		if (!cache.TryGet(hashCode, out var imageInfo))
		{
			lock (image)
			{
				var imageRawFormat = image.RawFormat;
				var imageFormat = (imageRawFormat.Equals(ImageFormat.MemoryBmp) || imageRawFormat.Equals(ImageFormat.Icon) || imageRawFormat.Equals(ImageFormat.Tiff)) ? ImageFormat.Png : imageRawFormat;
				var bytes = (byte[]?)(new ImageConverter()).ConvertTo(image, typeof(byte[]));
				if (bytes is null)
				{
					return null;
				}
				imageInfo = new ImageInfo(bytes, imageFormat);
			}
			cache.Put(hashCode, imageInfo);
		}
		return imageInfo;
	}

	static readonly WeakReferenceCache<string, ImageInfo> cache = new WeakReferenceCache<string, ImageInfo>();

	static readonly Lazy<ImmutableDictionary<Guid, string>> mimeTypeLookup = new Lazy<ImmutableDictionary<Guid, string>>(ImageCodecInfo.GetImageDecoders().Where(d => d.MimeType is not null).ToImmutableDictionary(d => d.FormatID, d => d.MimeType!));

	class ImageInfo
	{
		public ImageInfo(byte[] data, ImageFormat imageFormat)
		{
			Data = data;
			ImageFormat = imageFormat;
		}

		public byte[] Data { get; }
		public ImageFormat ImageFormat { get; }
	}
}
