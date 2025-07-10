using System;
using System.IO;
using CargoWise.RefDbRepo.BEReferenceData.Business;
using CargoWise.RefDbRepo.BEReferenceData.Services;
using static CargoWise.RefDbRepo.BEReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.BEReferenceData.CmdLine
{
	public static class UCCCodeListProgram
	{
		public static void Run(DateTime generationDateTime, string jsonLocation, string downloadDir)
		{
			if (jsonLocation == null)
			{
				jsonLocation = Path.Combine(ApplicationConfig.ServiceDir, FolderNames.UCCCodeListData);
			}
			jsonLocation = Path.Combine(jsonLocation, "AdditionalInformation.json");

			if (downloadDir == null)
			{
				downloadDir = ApplicationConfig.DownloadDir;
			}

			var cusCodeList = JSONParser.ReadJSonIntoResult(jsonLocation, Constants.ZZRefCusCodeList.AdditionalInformationImport);

			XMLGeneration.GenerateXml("BE AdditionalInformation", "RefCusCodeListZZ_AI44I.xml", cusCodeList, Constants.ZZRefCusCodeList.AdditionalInformationImport, generationDateTime, downloadDir);
		}
	}
}
