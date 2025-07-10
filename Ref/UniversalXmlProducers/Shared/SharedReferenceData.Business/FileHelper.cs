using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business
{
	public static class FileHelper
	{
		public static void CreatDirectoryIfRequired(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}

		public static void MoveFile(string sourceFileName, string destFileName)
		{
#pragma warning disable CA1031 // Do not catch general exception types
			try
			{
				if (File.Exists(destFileName))
				{
					File.Delete(destFileName);
				}
				File.Move(sourceFileName, destFileName);
			}
			catch
			{
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}

		public static string[] GetFileNames(string inputDirectory, StringBuilder stringBuilder)
		{
			string[] filenames = null;
			if (!string.IsNullOrEmpty(inputDirectory))
			{
#pragma warning disable CA1031 // Do not catch general exception types
				try
				{
					filenames = Directory.GetFiles(inputDirectory, "*.json");
				}
				catch (Exception e)
				{
					stringBuilder.AppendLine("Failed load files, message:" + e.Message.Replace("\r\n", ""));
				}
#pragma warning restore CA1031 // Do not catch general exception types
			}
			return filenames;
		}

		public static string GetFileContents(string filename, StringBuilder stringBuilder)
		{
			string contents = "";
#pragma warning disable CA1031 // Do not catch general exception types
			try
			{
				contents = File.ReadAllText(filename).Replace("\r\n", "");
			}
			catch (Exception e)
			{
				stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Failed read file:{filename}, message:{e.Message.Replace("\r\n", "")}");
			}
#pragma warning restore CA1031 // Do not catch general exception types
			return contents;
		}

		public static void ExportToXMLFile<T>(string dataSource, string outputFile, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDateTime, IEnumerable<T> codeList, UpdateType updateType)
		{
			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(updateType);

			foreach (var code in codeList)
			{
				writer.PopulateData(code);
			}
			writer.SaveXml(outputFile);
		}
	}
}
