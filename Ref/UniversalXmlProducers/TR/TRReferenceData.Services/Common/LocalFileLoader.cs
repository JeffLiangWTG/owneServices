using System;
using System.IO;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.TRReferenceData.Services.Common
{
	public class LocalFileLoader : TextLoader
	{
		public LocalFileLoader(Uri uri) : base(uri)
		{
			GetText = Task.Run(() => File.ReadAllText(Uri.LocalPath));
		}

		public override async Task<string> LoadAsync()
		{
			try
			{
				return await GetText;
			}
			catch (IOException ex)
			{
				Console.Error.WriteLine(ex.Message);
				Console.Error.WriteLine(ex.StackTrace);
				return string.Empty;
			}
		}

		public override void Dispose() { }

		readonly Task<string> GetText;
	}
}
