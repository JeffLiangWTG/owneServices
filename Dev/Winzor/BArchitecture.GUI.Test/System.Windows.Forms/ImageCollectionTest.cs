using NUnit.Framework;

namespace System.Windows.Forms;

public class ImageCollectionTest
{
	[Test]
	public void ImageCollectionTestTestSetKeyName()
	{
		var resources = new ComponentModel.ComponentResourceManager(typeof(ImageListStreamerTest));

		var imageListStreamer = (ImageListStreamer)resources.GetObject("ImageListForTest.ImageStream");
		using var imageList = new ImageList();
		imageList.ImageStream = imageListStreamer;

		var redKey = "FilterCategoryRed";
		imageList.Images.SetKeyName(0, redKey);

		Assert.That(imageList.Images[redKey], Is.EqualTo(imageList.Images[0]));
	}

	[Test]
	public void ImageCollectionGetImageSrcThrowWhenIndexOutOfRange()
	{
		var resources = new ComponentModel.ComponentResourceManager(typeof(ImageListStreamerTest));

		var imageListStreamer = (ImageListStreamer)resources.GetObject("ImageListForTest.ImageStream");
		using var imageList = new ImageList();
		imageList.ImageStream = imageListStreamer;

		var redKey = "FilterCategoryRed";
		imageList.Images.SetKeyName(0, redKey);

		Assert.DoesNotThrow(() => imageList.Images.GetImageSrc(0));
		Assert.Throws<IndexOutOfRangeException>(() => imageList.Images.GetImageSrc(3));
	}
}
