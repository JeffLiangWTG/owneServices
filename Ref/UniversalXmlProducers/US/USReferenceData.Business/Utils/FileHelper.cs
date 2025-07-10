using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.USReferenceData.Business
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
			try
			{
				if (File.Exists(destFileName))
				{
					File.Delete(destFileName);
				}
				File.Move(sourceFileName, destFileName);
			}
			catch (Exception)
			{
			}
		}

		public static string[] GetFileNames(string inputDirectory)
		{
			string[] filenames = null;
			if (!string.IsNullOrEmpty(inputDirectory))
			{
				try
				{
					filenames = Directory.GetFiles(inputDirectory, "*.json");
				}
				catch (Exception e)
				{
					Console.Error.WriteLine("Failed load files, message:" + e.Message.Replace("\r\n", ""));
				}
			}
			return filenames;
		}

		public static string GetFileContents(string filename)
		{
			string contents = "";
			try
			{
				contents = File.ReadAllText(filename).Replace("\r\n", "");
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"Failed read file:{filename}, message:{e.Message.Replace("\r\n", "")}");
			}
			return contents;
		}

		public static void ExportToXMLFile(string dataSource, string outputFile, XmlWriterConfiguration xmlWriterConfig, DateTime publicationDateTime, List<RefCusTariff> cusTariffList, UpdateType updateType)
		{
			var writer = new XmlWriter(xmlWriterConfig);
			writer.SetDataSource(dataSource);
			writer.SetPublicationTime(publicationDateTime);
			writer.SetUpdateType(updateType);

			foreach (var code in cusTariffList)
			{
				writer.PopulateData(code);
			}
			writer.SaveXml(outputFile);
		}
	}
}
