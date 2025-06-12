using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace CargoWise.eServices.Billing.Tests.Common
{
	public static class TestHelper
	{
		public static void CreateFileWithEmbeddedResource(string tempFolderPath, string resourceFolderName, string resourceFileName, Type type)
		{
			if (!Directory.Exists(tempFolderPath))
			{
				Directory.CreateDirectory(tempFolderPath);
			}

			using (var reader = GetEmbeddedResource(resourceFolderName + "." + resourceFileName, type))
			{
				using (var writer = new FileStream(Path.Combine(tempFolderPath, resourceFileName), FileMode.Create))
				{
					AddStream(reader, writer);
				}
			}
		}

		public static string PathToFileCreatedFromEmbeddedResource(Assembly assembly, string fileName, string valueToReplace = null, string valueAfterReplace = null)
		{
			var path = Path.GetTempFileName();
			File.Move(path, path = Path.ChangeExtension(path, Path.GetExtension(fileName)));
			using (var stream = assembly.GetManifestResourceStream(assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(fileName))))
			{
				if (stream == null)
				{
					throw new Exception($"Could not locate embedded resource '{fileName}'");
				}

				using (var reader = new StreamReader(stream))
				{
					using (var writer = new StreamWriter(path))
					{
						var result = reader.ReadToEnd();
						if (valueToReplace != null && valueAfterReplace != null)
						{
							result = result.Replace(valueToReplace, valueAfterReplace);
						}
						writer.Write(result);
					}
				}
			}

			return path;
		}

		public static XmlDocument LoadFromEmbeddedResource(Assembly assembly, string resourceFileName)
		{
			using (var stream = assembly.GetManifestResourceStream(assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(resourceFileName))))
			{
				stream.Seek(0L, SeekOrigin.Begin);
				XmlDocument doc = new XmlDocument();
				doc.Load(stream);

				return doc;
			}
		}

		public static Stream GetEmbeddedResource(string resourceName, Type type)
		{
			var assembly = Assembly.GetAssembly(type);
			var fullResourceName = assembly.GetName().Name + '.' + resourceName;
			var resource = assembly.GetManifestResourceStream(fullResourceName);
			if (resource == null)
			{
				throw new Exception($"Could not locate embedded resource '{fullResourceName}'");
			}
			return resource;
		}

		public static string GetEmbeddedResourceAsString(string resourceName, Type type)
		{
			using (var stream = GetEmbeddedResource(resourceName, type))
			{
				return new StreamReader(stream).ReadToEnd();
			}
		}

		public static void AddStream(Stream reader, Stream writer)
		{
			var buffer = new byte[32 * 1024];
			int read;
			do
			{
				read = reader.Read(buffer, 0, buffer.Length);
				writer.Write(buffer, 0, read);
			} while (read == buffer.Length);
			writer.Flush();
		}
	}
}
