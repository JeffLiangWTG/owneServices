using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using NUnit.Framework;
using WinzorTestFramework;

namespace WinzorFramework.Extensions;

public class ImageToBase64ExtensionTest
{
	[Test]
	public void TestToBase64DataUrlPreservesTransparency()
	{
		using var bitmap = new Bitmap(100, 100);
		Assume.That(bitmap.RawFormat, Is.EqualTo(ImageFormat.MemoryBmp));

		using (Graphics gfx = Graphics.FromImage(bitmap))
		using (SolidBrush brush = new SolidBrush(Color.FromArgb(50, Color.Aqua)))
		{
			gfx.FillRectangle(brush, 0, 0, 50, 50);
		}

		var converted = bitmap.ToBase64DataUrl();
		Assert.That(converted,
			Is.EqualTo(
				"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAGQAAABkCAYAAABw4pVUAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAD4SURBVHhe7c9RCcAAFIXQl2n9s40VuP9D8YABvHvfR5HGmiOmseaIaaw5YhprjpjGmiOmseaIaaw5YhprjpjGmiOmseaIaaw5YhprjpjGmiOmseaIaaw5YhprjpjGmiOmseaIaaw5YhprjpjGmiOmseaIaaw5YhprjpjGmiOmseaIaaw5YhprjpjGmiOmseaIaaw5YhprjpjGmiOmseaIaaw5YhprjpjGmiOmseaIaaw5YhprjpjGmiOmseaIaaw5YhprjpjGmiOmseaIaaw5YhprjpjGmiOWJEmSJEmSJEmSJEmSJEmSJEmSJEmSJEmSJEmSJMmP7j4rjkp0sf52lAAAAABJRU5ErkJggg=="
				));
	}

	[Test]
	public void TestToBase64DataUrlPreservesOpacity()
	{
		using var bitmap = new Bitmap(100, 100);
		Assume.That(bitmap.RawFormat, Is.EqualTo(ImageFormat.MemoryBmp));

		using (Graphics gfx = Graphics.FromImage(bitmap))
		using (SolidBrush brush = new SolidBrush(Color.Black))
		{
			gfx.FillRectangle(brush, 0, 0, 100, 100);
		}

		var converted = bitmap.ToBase64DataUrl();
		Assert.That(converted,
			Is.EqualTo(
				"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAGQAAABkCAYAAABw4pVUAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEKSURBVHhe7cixDQAwDICw/P90mgPY6QCSF+bafAVnPDjjwRkPznhwxoMzHpzx4IwHZzw448EZD854cMaDMx6c8eCMB2c8OOPBGQ/OeHDGgzMenPHgjAdnPDjjwRkPznhwxoMzHpzx4IwHZzw448EZD854cMaDMx6c8eCMB2c8OOPBGQ/OeHDGgzMenPHgjAdnPDjjwRkPznhwxoMzHpzx4IwHZzw448EZD854cMaDMx6c8eCMB2c8OOPBGQ/OeHDGgzMenPHgjAdnPDjjwRkPznhwxoMzHpzx4IwHZzw448EZD854cMaDMx6c8eCMB2c8OOPBGQ/OeHDGgzMenPHgjAdnPDjjwRnF7AOStOsr8WagqgAAAABJRU5ErkJggg=="
				));
	}

	[Test]
	public void TestToBase64()
	{
		using var bitmap = new Bitmap(100, 100);
		Assume.That(bitmap.RawFormat, Is.EqualTo(ImageFormat.MemoryBmp));

		using (Graphics gfx = Graphics.FromImage(bitmap))
		using (SolidBrush brush = new SolidBrush(Color.Black))
		{
			gfx.FillRectangle(brush, 0, 0, 100, 100);
		}

		var converted = bitmap.ToBase64();
		Assert.That(converted,
			Is.EqualTo(
				"iVBORw0KGgoAAAANSUhEUgAAAGQAAABkCAYAAABw4pVUAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEKSURBVHhe7cixDQAwDICw/P90mgPY6QCSF+bafAVnPDjjwRkPznhwxoMzHpzx4IwHZzw448EZD854cMaDMx6c8eCMB2c8OOPBGQ/OeHDGgzMenPHgjAdnPDjjwRkPznhwxoMzHpzx4IwHZzw448EZD854cMaDMx6c8eCMB2c8OOPBGQ/OeHDGgzMenPHgjAdnPDjjwRkPznhwxoMzHpzx4IwHZzw448EZD854cMaDMx6c8eCMB2c8OOPBGQ/OeHDGgzMenPHgjAdnPDjjwRkPznhwxoMzHpzx4IwHZzw448EZD854cMaDMx6c8eCMB2c8OOPBGQ/OeHDGgzMenPHgjAdnPDjjwRnF7AOStOsr8WagqgAAAABJRU5ErkJggg=="
				));
	}

	[Test]
	public void ToBase64DataUrlIsThreadSafe()
	{
		using var bitmap = new Bitmap(100, 100);
		Parallel.For(0, 10, (_, _) =>
		{
			Assert.That(bitmap.ToBase64DataUrl, Throws.Nothing);
		});
	}

	[Test]
	public void ToBase64IsThreadSafe()
	{
		using var bitmap = new Bitmap(100, 100);
		Parallel.For(0, 10, (_, _) =>
		{
			Assert.That(bitmap.ToBase64, Throws.Nothing);
		});
	}

	[Test]
	public void ToBase64DataUrlDisposedImageHandled()
	{
		using var bitmap = new Bitmap(100, 100);
		bitmap.Dispose();
		var result = bitmap.ToBase64DataUrl();
		Assert.That(result, Is.Empty);
	}

	[Test]
	public void ToBase64DisposedImageHandled()
	{
		using var bitmap = new Bitmap(100, 100);
		bitmap.Dispose();
		var result = bitmap.ToBase64();
		Assert.That(result, Is.Empty);
	}

	[Test]
	public void TestToBase64DataUrlSetsTiffImageFormatToPng()
	{
		var tiffData = "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";
		var tiff = tiffData.ToImage();

		Assert.That(tiff.RawFormat, Is.EqualTo(ImageFormat.Tiff));
		Assert.That(tiff.ToBase64DataUrl(), Does.StartWith("data:image/png;"));
	}

	[Test]
	public void TestNativeImagePtrFound()
	{
		// In different versions of System.Drawing.Common library, the private field name for image pointer may change
		// It's 'nativeImage' in 7.0.0 and '_nativeImage' 8.0.0.
		// This test is to ensure that the field name is not changed.
		Assert.That(ImageExtensions.NativeImagePtrFound(), Is.True);
	}
}
