using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Drawing;
using WinzorFramework;
using WinzorFramework.Extensions;

namespace System.Windows.Forms;

public class ImageCollection : WrappedList<Image>
{
	readonly List<ImageInfo> imageInfoCollection = new ();
	readonly ConcurrentDictionary<Image, string> imageSrcCollection = new ();

	internal ImageCollection()
	{
	}

	public Image? this[string key]
	{
		get
		{
			if (key == null || key.Length == 0)
			{
				return null;
			}

			int index = IndexOfKey(key);
			if (IsValidIndex(index))
			{
				return this[index];
			}

			return null;
		}
	}

	public void Add(Icon icon)
	{
		Add(icon.ToBitmap());
	}

	public void Add(string key, Image image)
	{
		Add(image);
		imageInfoCollection.Add(new ImageInfo { Name = key });
	}

	public void SetKeyName(int index, string name)
	{
		if (!IsValidIndex(index))
		{
			throw new IndexOutOfRangeException();
		}

		if (imageInfoCollection[index] == null)
		{
			imageInfoCollection[index] = new ImageInfo();
		}
		imageInfoCollection[index].Name = name;
	}

	public int IndexOfKey(string key)
	{
		if (key == null || key.Length == 0)
		{
			return -1;
		}

		foreach (var item in from ImageInfo item in imageInfoCollection
							 where key.Equals(item.Name, StringComparison.OrdinalIgnoreCase)
							 select item)
		{
			return imageInfoCollection.IndexOf(item);
		}
		return -1;
	}

	public StringCollection Keys
	{
		get
		{
			StringCollection stringCollection = new StringCollection();
			for (int i = 0; i < imageInfoCollection.Count; i++)
			{
				ImageInfo imageInfo = imageInfoCollection[i];
				if (imageInfo != null && imageInfo.Name != null && imageInfo.Name.Length != 0)
				{
					stringCollection.Add(imageInfo.Name);
				}
				else
				{
					stringCollection.Add(string.Empty);
				}
			}

			return stringCollection;
		}
	}

	public bool Empty => Count == 0;

	internal void ResetKeys()
	{
		imageInfoCollection.Clear();

		for (int i = 0; i < Count; i++)
		{
			imageInfoCollection.Add(new ImageInfo());
		}
	}

	bool IsValidIndex(int index) => (index >= 0) && index < Count;

	internal class ImageInfo
	{
		internal string? Name { get; set; }
	}

	protected override void OnAdd(Image image, int index)
	{
		base.OnAdd(image, index);
		var imageSrc = image.ToBase64DataUrl() ?? string.Empty;
		imageSrcCollection.AddOrUpdate(image, imageSrc, (_, _) => imageSrc);
	}

	protected override void OnRemove(Image image)
	{
		base.OnRemove(image);
		imageSrcCollection.Remove(image, out var _);
	}

	internal string GetImageSrc(int index)
	{
		if (!IsValidIndex(index))
		{
			throw new IndexOutOfRangeException();
		}

		imageSrcCollection.TryGetValue(this[index], out var imageSrc);
		return imageSrc ?? string.Empty;
	}
}
