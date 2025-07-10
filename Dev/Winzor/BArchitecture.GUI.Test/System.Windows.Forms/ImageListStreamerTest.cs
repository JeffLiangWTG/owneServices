using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;

namespace System.Windows.Forms;

public partial class ImageListStreamerTest
{
	[Test]
	public void ImageListStreamerCanBeDeserializedFromResourceWithCorrectData()
	{
		var resources = new ComponentModel.ComponentResourceManager(typeof(ImageListStreamerTest));
		var imageListStreamer = (ImageListStreamer)resources.GetObject("ImageListForTest.ImageStream");

		Assert.That(imageListStreamer, Is.Not.Null);
		Assert.That(imageListStreamer.Images.Count, Is.EqualTo(3));
		Assert.That(imageListStreamer.ImageSize, Is.EqualTo(new Size(20, 20)));
		Assert.That(imageListStreamer.ColorDepth, Is.EqualTo(ColorDepth.Depth8Bit));
	}

	[Test]
	public void ImageListStreamerSetsDataInImageList()
	{
		var resources = new ComponentModel.ComponentResourceManager(typeof(ImageListStreamerTest));
		var imageListStreamer = (ImageListStreamer)resources.GetObject("ImageListForTest.ImageStream");

		using var imageList = new ImageList();
		imageList.ImageStream = imageListStreamer;

		Assert.That(imageList.Images.Count, Is.EqualTo(3));
		Assert.That(imageList.ImageSize, Is.EqualTo(new Size(20, 20)));
		Assert.That(imageList.ColorDepth, Is.EqualTo(ColorDepth.Depth8Bit));
	}

	[Test]
	public void ImageListStreamerImagesRetainTransparencyFromAlphaMask()
	{
		var resources = new ComponentModel.ComponentResourceManager(typeof(ImageListStreamerTest));
		var imageListStreamer = (ImageListStreamer)resources.GetObject("ImageListForTest.ImageStream");

		Assert.That(((Bitmap)imageListStreamer.Images[0]).GetPixel(0, 0), Is.EqualTo(Color.FromArgb(0, 0, 0, 0)));
		Assert.That(((Bitmap)imageListStreamer.Images[0]).GetPixel(10, 10), Is.Not.EqualTo(Color.FromArgb(0, 0, 0, 0)));
		Assert.That(((Bitmap)imageListStreamer.Images[1]).GetPixel(0, 0), Is.EqualTo(Color.FromArgb(0, 0, 0, 0)));
		Assert.That(((Bitmap)imageListStreamer.Images[1]).GetPixel(10, 10), Is.Not.EqualTo(Color.FromArgb(0, 0, 0, 0)));
		Assert.That(((Bitmap)imageListStreamer.Images[2]).GetPixel(0, 0), Is.EqualTo(Color.FromArgb(0, 0, 0, 0)));
		Assert.That(((Bitmap)imageListStreamer.Images[2]).GetPixel(10, 10), Is.Not.EqualTo(Color.FromArgb(0, 0, 0, 0)));
	}
}
