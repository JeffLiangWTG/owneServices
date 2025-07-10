using System;
using System.Drawing;
using System.IO;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ImageSelectionForm : ZChildForm
	{
		public ImageSelectionForm()
		{
			InitializeComponent();
		}

		protected ZBlob fLogo;
		public ZBlob Logo
		{
			get { return fLogo; }
			set { fLogo = value; }
		}

		#region Implementation

		#region Event Handlers

		void ImageSelectionForm_Load(object sender, EventArgs e)
		{
			ImageSelectionControl.Image = ConvertByteToImage(fLogo);
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			fLogo = ConvertImageToByte(ImageSelectionControl.Image);
		}

		#endregion

		#region Conversion methods

		protected Image ConvertByteToImage(byte[] imageInByteArrayFormat)
		{
			Image curImage = null;
			if (imageInByteArrayFormat != null)
			{
				MemoryStream stream = new MemoryStream(imageInByteArrayFormat);
				try
				{
					curImage = Image.FromStream(stream, false);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
				}
			}
			return curImage;
		}

		protected byte[] ConvertImageToByte(Image sourceImage)
		{
			if (sourceImage != null)
			{
				MemoryStream memStream = new MemoryStream();
				try
				{
					sourceImage.Save(memStream, System.Drawing.Imaging.ImageFormat.Tiff);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
				}
				return memStream.ToArray();
			}
			return null;
		}

		#endregion

		#endregion
	}
}
