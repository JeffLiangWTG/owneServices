using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.IO;

namespace Enterprise.Customs.DataTransfer.Testing
{
	public class TestFileHelper : IDisposable
	{
		readonly Dictionary<string, (string FilePath, EmbeddedResourceRetriever EmbeddedResourceRetriever)> resources = new ();

		internal string GetPathForTesting(string fileName) => GetFilePath("Testing", fileName);

		internal string GetPathForTestFiles(string fileName) => GetFilePath("TestFiles", fileName);

		internal string GetPathForUniversalTestFiles(string fileName) => GetFilePath("Universal.TestFiles", fileName);

		internal string GetPathForUniversalHVLVAirTestFiles(string fileName) => GetFilePath("Universal.HVLV.Air.TestFiles", fileName);

		internal string GetPathForUniversalHVLVSeaTestFiles(string fileName) => GetFilePath("Universal.HVLV.Sea.TestFiles", fileName);

		string GetFilePath(string prefix, string fileName)
		{
			var baseFilePath = GetBaseFilePath();
			return Path.Combine(baseFilePath, $"Enterprise.Customs.DataTransfer.Testing.{prefix}.{fileName}");
		}

		public string GetPathForResourceName(string resourceName, Assembly assembly = null)
		{
			var baseFilePath = GetBaseFilePath(assembly);
			return Path.Combine(baseFilePath, resourceName);
		}

		public string ReadResourceFileContents(string resourceName, Assembly assembly = null)
		{
			return File.ReadAllText(GetPathForResourceName(resourceName, assembly));
		}

		public void Dispose()
		{
			foreach (var value in resources.Values)
			{
				value.EmbeddedResourceRetriever.Dispose();
			}
			resources.Clear();
		}

		string GetBaseFilePath(Assembly assembly = null)
		{
			if (assembly == null)
			{
				assembly = Assembly.GetExecutingAssembly();
			}
			var key = assembly.FullName;
			if (!resources.TryGetValue(key, out var value))
			{
				var retriever = new EmbeddedResourceRetriever(assembly);
				var filePath = retriever.SaveAllResourcesToFiles();
				resources[key] = value = (filePath, retriever);
			}
			return value.FilePath;
		}
	}
}
