using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace CargoWise.eHub.Core.Tests
{
    
    
    /// <summary>
    ///This is a test class for ImageConverterTest and is intended
    ///to contain all ImageConverterTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ImageConverterTest
    {

        /// <summary>
        ///A test for ConvertTiff2pdf
        ///</summary>
        //[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void ConvertTiff2PdfTest()
        {
            string tiff, expected, actual;
            ImageConverter target = new ImageConverter();

            tiff = GetEmbeddedResourceFileContents("TestFiles.Tiff_Base64_1.txt");
            expected = GetEmbeddedResourceFileContents("TestFiles.Tiff_Base64_1_Pdf.txt");
            actual = target.ConvertTiff2Pdf(tiff);
            try
            {
                Assert.AreEqual(expected.Remove(326000), actual.Remove(326000));
            }
            catch (Exception)
            {
                for (int i = 0; i < actual.Length; i++)
                {
                    Assert.AreEqual(expected[i], actual[i], "Bytes at position {0} are not equal.", i + 1);
                }
            }

            tiff = GetEmbeddedResourceFileContents("TestFiles.Tiff_Base64_2.txt");
            expected = GetEmbeddedResourceFileContents("TestFiles.Tiff_Base64_2_Pdf.txt");
            actual = target.ConvertTiff2Pdf(tiff);
            try
            {
                Assert.AreEqual(expected.Remove(101500), actual.Remove(101500));
            }
            catch (Exception)
            {
                for (int i = 0; i < actual.Length; i++)
                {
                    Assert.AreEqual(expected[i], actual[i], "Bytes at position {0} are not equal.", i + 1);
                }
            }

            tiff = String.Empty;
            expected = String.Empty;
            actual = target.ConvertTiff2Pdf(tiff);
            Assert.AreEqual(expected, actual);
        }

        string GetEmbeddedResourceFileContents(string resourceName)
        {
            string fullResourceName = Assembly.GetExecutingAssembly().GetName().Name + '.' + resourceName;
            using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(fullResourceName)))
            {
                return reader.ReadToEnd();
            }
        }

        string WriteBase64ToFile(string data, string fileExtn)
        {
            string outFile = Path.GetTempFileName();
            File.Move(outFile, outFile += "." + fileExtn);
            File.WriteAllBytes(outFile, Convert.FromBase64String(data));
            Debug.WriteLine(outFile);
            return outFile;
        }

    }
}
