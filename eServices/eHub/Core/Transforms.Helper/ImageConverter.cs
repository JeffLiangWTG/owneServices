using System;
using System.Drawing.Imaging;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Image = System.Drawing.Image;

namespace CargoWise.eHub.Core.Transforms.Helper
{
    public class ImageConverter
    {
        public string ConvertTiff2Pdf(string tiff)
        {
            try
            {
                var pdfDocument = new Document();

                using (MemoryStream msIn = new MemoryStream(Convert.FromBase64String(tiff)), msOut = new MemoryStream())
                {
                    var pdfWriter = PdfWriter.GetInstance(pdfDocument, msOut);

                    using (var img = Image.FromStream(msIn))
                    {
                        var pageCount = img.GetFrameCount(FrameDimension.Page);

                        for (var i = 0; i < pageCount; i++)
                        {
                            img.SelectActiveFrame(FrameDimension.Page, i);

                            var pic = iTextSharp.text.Image.GetInstance(img, ImageFormat.Tiff);
                            pdfDocument.Add(pic);
                            pdfDocument.NewPage();
                        }
                    }

                    pdfWriter.Close();
                    return Convert.ToBase64String(msOut.ToArray());
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
