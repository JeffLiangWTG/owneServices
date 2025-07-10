using System;
using System.IO;

namespace CargoWise.RefDbRepo.DEReferenceData.Services.DeTariffs
{
	public class DownloadSource
	{
		public DownloadSource(string fileName, Uri baseUri)
		{
			ArgumentNullException.ThrowIfNull(fileName);
			if (baseUri == null)
			{
				throw new ArgumentNullException(nameof(baseUri));
			}

			FileName = fileName;
			var withoutExtension = Path.GetFileNameWithoutExtension(fileName);
			BaseUri = baseUri;

			if (int.TryParse(withoutExtension[2..8], out var setNumber))
			{
				SetNumber = setNumber;
			}

			if (int.TryParse(withoutExtension[9..], out var version))
			{
				Version = version;
			}
		}

		public static DownloadSource CreateOrNull(Uri baseUri, string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName) || !fileName.StartsWith("XD", StringComparison.InvariantCulture) ||
				fileName.Length < 10 || baseUri == null)
			{
				return null;
			}

			return new DownloadSource(fileName, baseUri);
		}

		public string FileName { get; }
		public Uri BaseUri { get; }
		public Uri DownloadUri => new(BaseUri, FileName);
		public int SetNumber { get; }
		public int Version { get; }
	}
}
