using System;
using System.IO;
using System.Text;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;

namespace Enterprise.MasterFiles.Business
{
	public class AttachmentTextAppender
	{
		public void Append(string attachmentFilename, byte[] attachmentData, StringBuilder sb)
		{
			Append(attachmentFilename, attachmentData, sb, null);
		}

		public void Append(string attachmentFilename, byte[] attachmentData, StringBuilder sb, string password)
		{
			using (Stream zipStream = new MemoryStream(attachmentData))
			using (MemoryStream byteStream = new MemoryStream())
			{
				string rglFileName = Path.ChangeExtension(attachmentFilename, ".rgl");
				string xmlFileName = Path.ChangeExtension(attachmentFilename, ".xml");

				ZipExtractor extractor = new ZipExtractor(password);
				try
				{
					if (extractor.ContainsFile(zipStream, rglFileName))
					{
						extractor.ExtractZipStream(zipStream, byteStream, rglFileName);
						TwoWayEncoder decryptor = TwoWayEncoder.NewWithStandardInitialisationVector();
						Append(sb, decryptor.Decrypt(Encoding.UTF8.GetString(byteStream.ToArray())));
					}
					else if (extractor.ContainsFile(zipStream, xmlFileName))
					{
						extractor.ExtractZipStream(zipStream, byteStream, xmlFileName);
						Append(sb, Encoding.UTF8.GetString(byteStream.ToArray()));
					}
				}
				catch (Exception ex) when (ex is OutOfMemoryException || !ex.IsCriticalException())
				{
					// Invalid attachment - ignore.
				}
			}
		}

		protected virtual StringBuilder Append(StringBuilder sb, string value)
		{
			return sb.Append(value);
		}
	}
}
