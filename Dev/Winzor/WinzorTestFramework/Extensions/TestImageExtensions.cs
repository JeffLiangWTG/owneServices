using System;
using System.Drawing;
using System.IO;

namespace WinzorTestFramework;

internal static class TestImageExtensions
{
	public static Image ToImage(this string str)
	{
		byte[] bytes = Convert.FromBase64String(str);
		return Image.FromStream(new MemoryStream(bytes));
	}
}
