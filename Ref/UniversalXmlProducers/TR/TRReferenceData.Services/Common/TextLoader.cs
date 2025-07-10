using System;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.TRReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.TRReferenceData.Services
{
	public abstract class TextLoader : IDisposable
	{
		public static TextLoader New(Uri uri)
		{
			var schema = uri.Scheme;
			TextLoader result;
			if (schema == Uri.UriSchemeHttp || schema == Uri.UriSchemeHttps)
			{
				result = new HttpLoader(uri);
			}
			else if (schema == Uri.UriSchemeFile)
			{
				result = new LocalFileLoader(uri);
			}
			else
			{
				throw new InvalidOperationException($@"URI: ""{uri.OriginalString} "" is not supported yet. Please either add a support or use another type of URI.");
			}
			return result;
		}

		protected TextLoader(Uri uri) => Uri = uri;

		public abstract Task<string> LoadAsync();

		public abstract void Dispose();

		protected Uri Uri { get; }
	}
}
