using System.Drawing;
using System.Reflection;

namespace WinzorFramework.Extensions;

static class ImageExtensions
{
	internal static bool IsDisposed(this Image image)
	{
		if (getNativeImagePtr == null)
		{
			try
			{
				lock (image)
				{
					return image.Width < 0; // Will fail here with exception if image was disposed
				}
			}
			catch (ArgumentException)
			{
				return true;
			}
		}
		return (IntPtr?)getNativeImagePtr(image) == IntPtr.Zero;
	}

#if DEBUG // for tests
	internal static bool NativeImagePtrFound()
	{
		return GetNativeImagePtr() != null;
	}
#endif

	static GetNativeImagePtrDelegate? GetNativeImagePtr()
	{
		var field = typeof(Image).GetField("_nativeImage", BindingFlags.Instance | BindingFlags.NonPublic);
		return field != null ? field.GetValue : null;
	}

	delegate object? GetNativeImagePtrDelegate(object? component);

	static readonly GetNativeImagePtrDelegate? getNativeImagePtr = GetNativeImagePtr();
}
