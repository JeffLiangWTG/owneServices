using System;
using System.Xml;
using Microsoft.XLANGs.BaseTypes;
using System.IO;

namespace CargoWise.eHub.Products.ZACustoms.Helpers
{
    public static class SupportingDocumentHelper
    {
        readonly static int bytes_max = 1024 * 1024 * 5; // 5MB

        public static void VerifyDocumentSize(XLANGMessage message)
        {
            var stream = (Stream) message[0].RetrieveAs(typeof(Stream));
            var reader = XmlReader.Create(stream);
            var buffer = new char[10000];
            var base64_bytes_count = 0;

            // Find the first (and hopefully only) ImageData element
            reader.ReadToFollowing("ImageData");
            reader.Read();

            var result = 1;
            while (result != 0)
            {
                result = reader.ReadValueChunk(buffer, 0, buffer.Length);
                base64_bytes_count += result;

                // Remove any padding from the count
                if (result >= 1 && buffer[result - 1] == '=')
                {
                    base64_bytes_count--;
                }
                if (result >= 2 && buffer[result - 2] == '=')
                {
                    base64_bytes_count--;
                }

                // Each character in a base64 string represents 6 bits of data.
                var document_bits_count = base64_bytes_count * 6;

                // Convert bits to bytes.
                // C# division rounds towards zero, this ensures we ignore the extra unused bits that may be in the final characters.
                var document_bytes_count = document_bits_count / 8;

                // fail if over the limit
                if (document_bytes_count > bytes_max)
                {
                    throw new Exception("Document is over 5MB, please submit a document less than 5MB. " + document_bytes_count + "/" + bytes_max + " bytes");
                }
            }
        }
    }
}
