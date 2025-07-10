using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using CargoWise.RefDbRepo.UniversalXmlParser.Resources;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Utilities
{
	public class FileTrace : IFileTrace
	{
		readonly string _sourcePath;

		public FileTrace(string sourcePath)
		{
			Argument.NotNullOrEmpty(sourcePath, nameof(sourcePath));
			_sourcePath = sourcePath;
		}

		public async Task<int> GetLastSuccessLineNumberAsync()
		{
			return await Task.Run(() =>
			{
				var lastSuccessLineNumber = 0;
				var traceFile = GetTraceFile(_sourcePath);
				if (File.Exists(traceFile))
				{
					using (TextReader reader = File.OpenText(traceFile))
					{
						var traceLine = reader.ReadLine();
						int lineNumber;
						if ((traceLine != null) && int.TryParse(traceLine, out lineNumber))
						{
							lastSuccessLineNumber = lineNumber;
						}
					}
				}

				return lastSuccessLineNumber;
			});
		}

		public void Remove()
		{
			var traceFile = GetTraceFile(_sourcePath);
			if (File.Exists(traceFile))
			{
				File.Delete(traceFile);
			}
		}

		public async Task SaveLastSuccessLineNumberAsync(int lineNumber)
		{
			await Task.Run(() =>
			{
				using (var writer = new StreamWriter(GetTraceFile(_sourcePath), false))
				{
					writer.Write(lineNumber.ToString(CultureInfo.InvariantCulture));
				}
			});
		}

		static string GetTraceFile(string filePath)
		{
			Argument.NotNullOrEmpty(filePath, nameof(filePath));

			var fileName = Path.GetFileNameWithoutExtension(filePath);

			if (string.IsNullOrEmpty(fileName))
			{
				throw new ArgumentNullException(filePath);
			}

			var traceFilePath = Path.ChangeExtension(filePath, Constants.ApplicationSettings.TraceFileExtension);
			return traceFilePath;
		}
	}
}
