using System;
using System.IO;

namespace CargoWise.RefDbRepo.ReferenceDataSqlProducers
{
	class Program
	{
		static void Main(string[] args)
		{
			var fileName = ApplicationConfig.Instance.ParserFileName;
			var fileOutputFile = ApplicationConfig.Instance.OutputFile;
			var sql = TextParser.GenerateInsertSQL(fileName);

			using (var fileStream = new FileStream(fileOutputFile, FileMode.OpenOrCreate, FileAccess.Write))
			using (var streamWriter = new StreamWriter(fileStream))
			{
				streamWriter.Write(sql);
			}

			Console.WriteLine("Press Any Key To Continue:");
			Console.ReadKey();
		}
	}
}
