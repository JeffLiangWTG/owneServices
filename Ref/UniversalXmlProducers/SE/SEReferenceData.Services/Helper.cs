using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace CargoWise.RefDbRepo.SEReferenceData.Services
{
	public static class Helper
	{
		public static T DownloadKryptFile<T>(HttpClient client, string url, string temporaryDownloadPath = null)
			where T : class
		{
			using (var ciphertext = Download(client, url))
			{
				if (url.EndsWith("xml", StringComparison.InvariantCulture))
				{
					return Deserialize<T>(ciphertext);
				}

				using (var cleartext = Decrypt(ciphertext, out string filename))
				using (var deflated = Unzip(cleartext))
				using (var saved = SaveUnzippedToFile(deflated, temporaryDownloadPath, filename))
				{
					return Deserialize<T>(saved);
				}
			}
		}

		private static Stream Download(HttpClient client, string url)
		{
			var task = DownloadAsync(client, url);
			task.Wait();
			return task.Result;
		}

		private static async Task<Stream> DownloadAsync(HttpClient client, string url)
		{
			var uri = new Uri(url);
			var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
			return await response.Content.ReadAsStreamAsync();
		}

		/// <summary>
		/// See official example <see href="https://github.com/bcgit/bc-csharp/blob/master/crypto/test/src/openpgp/examples/SignedFileProcessor.cs#L32">SignedFileProcessor::VerifyFile</see>
		/// </summary>
		/// <param name="inputStream">ciphertext</param>
		/// <returns>cleartext</returns>
		public static Stream Decrypt(Stream inputStream, out string filename)
		{
			inputStream = PgpUtilities.GetDecoderStream(inputStream.ToSeekable());

			var pgpFact = new PgpObjectFactory(inputStream);
			var c1 = (PgpCompressedData)pgpFact.NextPgpObject();
			pgpFact = new PgpObjectFactory(c1.GetDataStream());

			// Extract key used to sign document
			var p1 = (PgpOnePassSignatureList)pgpFact.NextPgpObject();
			var ops = p1[0];

			// Extract encrypted content of document
			var p2 = (PgpLiteralData)pgpFact.NextPgpObject();
			var dIn = p2.GetInputStream();
			var keyIn = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.SEReferenceData.Services.Tulltaxan_Fildistribution.asc");
			var pgpRing = new PgpPublicKeyRingBundle(PgpUtilities.GetDecoderStream(keyIn));
			var key = pgpRing.GetPublicKey(ops.KeyId);

			// Decrypt content and construct expected signature
			ops.InitVerify(key);
			var outStream = dIn.Tee(ops.Update);

			// Verify signature
			var p3 = (PgpSignatureList)pgpFact.NextPgpObject();
			var firstSig = p3[0];
			if (!ops.Verify(firstSig))
			{
				throw new PgpException("Signature verification failed.");
			}

			filename = p2.FileName;
			return outStream;
		}

		public static Stream Unzip(Stream stream)
		{
			return new GZipStream(stream, CompressionMode.Decompress);
		}

		public static T Deserialize<T>(string filename)
			where T : class
		{
			using (var stream = File.OpenRead(filename))
			{
				return Deserialize<T>(stream);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5369:Use XmlReader for 'XmlSerializer.Deserialize()'", Justification = "Internal tool")]
		public static T Deserialize<T>(Stream stream)
			where T : class
		{
			var serializer = new XmlSerializer(typeof(T));
			return (T)serializer.Deserialize(stream);
		}

		public static Stream SaveUnzippedToFile(this Stream stream, string temporaryDownloadPath, string filename)
		{
			if (string.IsNullOrEmpty(temporaryDownloadPath) || string.IsNullOrEmpty(filename))
			{
				return stream;
			}
			var fileInfo = new FileInfo(Path.Combine(temporaryDownloadPath, filename));
			var currentFileName = fileInfo.FullName;
			var newFileName = currentFileName.Remove(currentFileName.Length - fileInfo.Extension.Length);
			return stream.Tee(newFileName);
		}

		public static Stream Tee(this Stream stream, string newFileName)
		{
			if (stream is MemoryStream ms && ms.Position == 0L)
			{
				return ms.Tee(newFileName);
			}
			ms = new MemoryStream();
			stream.CopyTo(ms);
			return ms.Tee(newFileName);
		}

		public static Stream Tee(this MemoryStream stream, string newFileName)
		{
			using (var fileStream = File.Create(newFileName))
			{
				stream.CopyTo(fileStream);
				stream.Position = 0L;
				return stream;
			}
		}

		public static Stream Tee(this Stream stream, Action<byte> onReadByte)
		{
			if (onReadByte == null)
			{
				return stream;
			}
			var outStream = new MemoryStream();
			int ch;
			while ((ch = stream.ReadByte()) >= 0)
			{
				onReadByte((byte)ch);
				outStream.WriteByte((byte)ch);
			}
			outStream.Position = 0L;
			return outStream;
		}

		private static Stream ToSeekable(this Stream stream)
		{
			if (stream.CanSeek)
			{
				return stream;
			}
			var ms = new MemoryStream();
			stream.CopyTo(ms);
			stream.Dispose();
			ms.Position = 0;
			return ms;
		}
	}
}
