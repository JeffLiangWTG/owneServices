using System;
using System.IO;
using System.Reflection;
using CargoWise.IO;

namespace Enterprise.Customs.Business.Testing
{
	public class TestFileReader
	{
		public TestFileReader(Type type)
		{
			assembly = type.Assembly;
		}

		readonly Assembly assembly;

		public byte[] GetEmbeddedFileData(string embeddedResourcePath, string filename)
		{
			return Retriever.GetBytes(CombineResourcePathAndFilename(embeddedResourcePath, filename));
		}

		public string GetEmbeddedFileText(string embeddedResourcePath, string filename)
		{
			using (var stream = assembly.GetManifestResourceStream(CombineResourcePathAndFilename(embeddedResourcePath, filename)))
			using (var reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

		public string ExtractEmbeddedResourceToFile(string embeddedResourcePath, string outputFilePath, string filename)
		{
			string outputFileFullPath = Path.Combine(outputFilePath, filename);
			using (var stream = assembly.GetManifestResourceStream(CombineResourcePathAndFilename(embeddedResourcePath, filename)))
			using (var writer = File.Create(outputFileFullPath))
			{
				stream.Seek(0, SeekOrigin.Begin);
				stream.CopyTo(writer);
			}
			return outputFileFullPath;
		}

		public static string GetAssemblyName(Type type)
		{
			return type.Assembly.GetName().Name;
		}

		string CombineResourcePathAndFilename(string embeddedResourcePath, string filename) => string.Concat(embeddedResourcePath, ".", filename);

		EmbeddedResourceRetriever Retriever => retriever ?? (retriever = new EmbeddedResourceRetriever(assembly));
		EmbeddedResourceRetriever retriever;
	}
}
