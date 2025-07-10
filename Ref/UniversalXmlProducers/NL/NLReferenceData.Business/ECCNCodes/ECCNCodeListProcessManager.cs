using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.NLReferenceData.Services;
using FlexCel.XlsAdapter;
using Microsoft.IdentityModel.Tokens;
using static CargoWise.RefDbRepo.NLReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.NLReferenceData.Business
{
	public class ECCNCodeListProcessManager
	{
		public ECCNCodeListProcessManager()
		{
			ErrorCollector = new StringBuilder();
		}

		public ECCNCodeListProcessManager(StringBuilder errorCollector)
		{
			ErrorCollector = errorCollector;
		}

		public void RunBuilder()
		{
			var xlsFile = GetXlsFile();

			if (!xlsFile.IsNullOrEmpty())
			{
				var parser = new ECCNCodeListExcelParser();
				var eECNCodeListData = parser.ReadXlsFile(xlsFile);

				var builder = new ECCNCodeListBuilder(ErrorCollector);
				builder.GenerateUniversalReferenceDataXml(eECNCodeListData, DateTime.Now, ApplicationConfig.OutputPath);
			}
			else
			{
				ErrorCollector.AppendLine("No XLS or XLSX file was found in folder " + contentFolder);
			}
		}

		public List<string> GetXlsFile()
		{
			var xlsFiles = new List<string>();

			if (Directory.Exists(contentFolder))
			{
				var filenames = Directory.GetFiles(contentFolder, "*.xls*");
				if (filenames.Any())
				{
					xlsFiles.Add(filenames.First());
				}
			}
			return xlsFiles;
		}

		public virtual string contentFolder => Path.Combine(ApplicationConfig.ServiceDir, FolderNames.ECCNCodeListData);
		readonly StringBuilder ErrorCollector;
	}
}
