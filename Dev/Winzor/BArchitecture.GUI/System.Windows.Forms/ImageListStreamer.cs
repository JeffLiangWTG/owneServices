using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization;

namespace System.Windows.Forms;

[Serializable]
public class ImageListStreamer : ISerializable, IDisposable
{
	static readonly byte[] HEADER_MAGIC = new byte[] { 0x4D, 0x53, 0x46, 0X74 };
	static readonly byte[] BITMAP_MAGIC = new byte[] { 0x42, 0x4d };

	[Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Copied from Winforms")]
	readonly ImageList? imageList;

	internal ImageListStreamer(ImageList il)
	{
		// Copied from Winforms
		imageList = il;
	}

	ImageListStreamer(SerializationInfo info, StreamingContext context)
	{
		// Copied from Winforms
		if (info.GetEnumerator() is not { } enumerator)
		{
			return;
		}

		while (enumerator.MoveNext())
		{
			if (!string.Equals(enumerator.Name, "Data", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			try
			{
				if (enumerator.Value is byte[] data)
				{
					Deserialize(data);
				}
			}
			catch (Exception e)
			{
				Debug.Fail($"ImageList serialization failure: {e}");
				throw;
			}
		}
	}

	internal ImageListStreamer(Stream stream)
	{
		// Copied from Winforms
		if (stream is MemoryStream memoryStream
			&& memoryStream.TryGetBuffer(out ArraySegment<byte> buffer)
			&& buffer.Offset == 0
			&& buffer.Array is { } array)
		{
			Deserialize(array);
		}
		else
		{
			stream.Position = 0;
			using MemoryStream copyStream = new (checked((int)stream.Length));
			stream.CopyTo(copyStream);
			Deserialize(copyStream.GetBuffer());
		}
	}

	/// <summary>
	///  Decompresses the given input, returning a new array that represents
	///  the uncompressed data.
	/// </summary>
	byte[] Decompress(byte[] input)
	{
		// Copied from Winforms
		int finalLength = 0;
		int idx = 0;
		int outputIdx = 0;

		// Check for our header. If we don't have one,
		// we're not actually compressed, so just return
		// the original.
		//
		if (input.Length < HEADER_MAGIC.Length)
		{
			return input;
		}

		for (idx = 0; idx < HEADER_MAGIC.Length; idx++)
		{
			if (input[idx] != HEADER_MAGIC[idx])
			{
				return input;
			}
		}

		// Ok, we passed the magic header test.

		for (idx = HEADER_MAGIC.Length; idx < input.Length; idx += 2)
		{
			finalLength += input[idx];
		}

		byte[] output = new byte[finalLength];

		idx = HEADER_MAGIC.Length;

		while (idx < input.Length)
		{
			byte runLength = input[idx++];
			byte current = input[idx++];

			int startIdx = outputIdx;
			int endIdx = outputIdx + runLength;

			while (startIdx < endIdx)
			{
				output[startIdx++] = current;
			}

			outputIdx += runLength;
		}

		return output;
	}

	void Deserialize(byte[] data)
	{
		// Adapted from Winforms
		using (MemoryStream ms = new MemoryStream(Decompress(data)))
		{
			SetImageListData(ms);
		}
	}

	public void GetObjectData(SerializationInfo si, StreamingContext context)
	{
		throw new NotImplementedException();
	}

	void SetImageListData(Stream stream)
	{
		var reader = new BinaryReader(stream);
		var imageListHeader = new ImageListHeader(reader);
		var imageBytes = reader.ReadBytes((int)(stream.Length - stream.Position));
		ImageSize = new Size(imageListHeader.ImageWidth, imageListHeader.ImageHeight);
		ColorDepth = GetColorDepthFromFlags(imageListHeader.Flags);
		Images = ReadImages(imageListHeader, imageBytes);
	}

	Image[] ReadImages(ImageListHeader header, byte[] imageListBytes)
	{
		var images = new Image[header.ImageCount];
		var imageBytes = imageListBytes;
		byte[]? maskBytes = null;

		if ((header.Flags & ImageListFlags.Mask) == ImageListFlags.Mask)
		{
			// If the image list has a mask, then we need to split out both the bitmap for the images and the bitmap for the masks
			var maskStartPos = FindNextBitmapStartPosition(imageListBytes);

			if (maskStartPos != -1)
			{
				imageBytes = new byte[maskStartPos];
				Array.Copy(imageListBytes, imageBytes, imageBytes.Length);
				maskBytes = new byte[imageListBytes.Length - imageBytes.Length];
				Array.Copy(imageListBytes, imageBytes.Length, maskBytes, 0, maskBytes.Length);
			}
		}

		using (var ms = new MemoryStream(imageBytes))
		using (var image = new Bitmap(ms))
		using (var indexedImage = image.Clone(new Rectangle(0, 0, image.Width, image.Height), PixelFormat.Format24bppRgb))
		{
			if (maskBytes is not null)
			{
				using (var msMask = new MemoryStream(maskBytes))
				using (var mask = new Bitmap(msMask))
				{
					for (int y = 0; y < mask.Height; y++)
					{
						for (int x = 0; x < mask.Width; x++)
						{
							if (mask.GetPixel(x, y) != Color.FromArgb(255, 0, 0, 0))
							{
								indexedImage.SetPixel(x, y, Color.Transparent);
							}
						}
					}
				}
			}

			var imagesPerRow = image.Width / header.ImageWidth;
			for (var i = 0; i < header.ImageCount; i++)
			{
				var xIndex = i % imagesPerRow;
				var yIndex = i / imagesPerRow;
				var rect = new Rectangle(xIndex * header.ImageWidth, yIndex * header.ImageHeight, header.ImageWidth, header.ImageHeight);
				var item = indexedImage.Clone(rect, indexedImage.PixelFormat);
				item.MakeTransparent(Color.Transparent);
				images[i] = item;
			}
		}
		return images;
	}

	int FindNextBitmapStartPosition(byte[] bitmapArray)
	{
		// Bitmap images start with magic bytes BM (0x42 0x4d)
		var next = Array.IndexOf(bitmapArray, BITMAP_MAGIC[0], 1);
		while (next != -1)
		{
			if (bitmapArray[next + 1] == BITMAP_MAGIC[1])
			{
				return next;
			}
			next = Array.IndexOf(bitmapArray, BITMAP_MAGIC[0], next + 1);
		}
		return -1;
	}

	public void Dispose()
	{
		foreach (var image in Images)
		{
			image.Dispose();
		}
	}

	public record ImageListHeader
	{
		internal ushort MagicBytes { get; init; }
		internal ushort Version { get; init; }
		internal ushort ImageCount { get; init; }
		internal ushort MaxImageCount { get; init; }
		internal ushort Grow { get; init; }
		internal ushort ImageWidth { get; init; }
		internal ushort ImageHeight { get; init; }
		internal uint ColorReference { get; init; }
		internal ImageListFlags Flags { get; init; }
		internal byte[] Overlays { get; init; }

		public ImageListHeader(BinaryReader reader)
		{
			MagicBytes = reader.ReadUInt16();
			Version = reader.ReadUInt16();
			ImageCount = reader.ReadUInt16();
			MaxImageCount = reader.ReadUInt16();
			Grow = reader.ReadUInt16();
			ImageWidth = reader.ReadUInt16();
			ImageHeight = reader.ReadUInt16();
			ColorReference = reader.ReadUInt32();
			Flags = (ImageListFlags)reader.ReadInt16();
			Overlays = reader.ReadBytes(8);
		}
	}

	public Image[] Images { get; private set; } = Array.Empty<Image>();
	public ColorDepth ColorDepth { get; private set; } = ColorDepth.Depth8Bit;
	public Size ImageSize { get; private set; } = new Size(16, 16);

	[Flags]
	internal enum ImageListFlags
	{
		Color = 0,
		Mask = 1,
		ColorDDB = 254,
		Color4 = 4,
		Color8 = 8,
		Color16 = 16,
		Color24 = 24,
		Color32 = 32,
		Palette = 2048,
		Mirror = 8192,
		PerItemMirror = 32768,
	}

	ColorDepth GetColorDepthFromFlags(ImageListFlags flags)
	{
		foreach (ImageListFlags flag in Enum.GetValues(typeof(ImageListFlags)))
		{
			if ((flags & flag) == flag)
			{
				switch (flag)
				{
					case ImageListFlags.Color4:
						return ColorDepth.Depth4Bit;
					case ImageListFlags.Color8:
						return ColorDepth.Depth8Bit;
					case ImageListFlags.Color16:
						return ColorDepth.Depth16Bit;
					case ImageListFlags.Color24:
						return ColorDepth.Depth24Bit;
					case ImageListFlags.Color32:
						return ColorDepth.Depth32Bit;
					default:
						break;
				}
			}
		}
		return ColorDepth.Depth8Bit;
	}
}
