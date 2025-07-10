using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap.Util
{
	class InMemoryZip
	{
		public byte[] GetZippedBytesWithAttachments(FileInfo payload, IEnumerable<FileInfo> attachments, string postfix)
		{
			using (var innerMemStream = new MemoryStream())
			{
				using (var zipStream = new ZipOutputStream(innerMemStream))
				{
					var newEntry = new ZipEntry(payload.Name + postfix);
					newEntry.Comment = "isPayload";
					zipStream.PutNextEntry(newEntry);
					var payloadBytes = File.ReadAllBytes(payload.FullName);
					zipStream.Write(payloadBytes, 0, payloadBytes.Length);
					zipStream.CloseEntry();
					zipStream.IsStreamOwner = false;
					foreach (var attachment in attachments)
					{
						var entry = new ZipEntry(attachment.Name);
						zipStream.PutNextEntry(entry);
						var attachmentBytes = File.ReadAllBytes(attachment.FullName);
						zipStream.Write(attachmentBytes, 0, attachmentBytes.Length);
						zipStream.CloseEntry();
					}
				}

				using (var outerMemStream = new MemoryStream())
				{
					using (var zipStream = new ZipOutputStream(outerMemStream))
					{
						var newEntry = new ZipEntry(string.Format(System.Globalization.CultureInfo.InvariantCulture, "inner{0}.zip", payload.Name));
						zipStream.PutNextEntry(newEntry);
						var innerBytes = innerMemStream.ToArray();
						zipStream.Write(innerBytes, 0, innerBytes.Length);
						zipStream.CloseEntry();
						zipStream.IsStreamOwner = false;
					}

					return outerMemStream.ToArray();
				}
			}
		}

		public byte[] GetZippedBytes(FileInfo file, string postfix = "")
		{
			using (var resultStream = new MemoryStream())
			{
				using (var zipStream = new ZipOutputStream(resultStream))
				{
					var newEntry = new ZipEntry(file.Name + postfix);
					zipStream.PutNextEntry(newEntry);
					var fileBytes = File.ReadAllBytes(file.FullName);
					zipStream.Write(fileBytes, 0, fileBytes.Length);
					zipStream.CloseEntry();
					zipStream.IsStreamOwner = false;
				}

				return resultStream.ToArray();
			}
		}
	}
}
