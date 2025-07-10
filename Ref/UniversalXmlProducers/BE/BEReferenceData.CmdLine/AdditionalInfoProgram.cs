using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.BEReferenceData.Business;
using CargoWise.RefDbRepo.BEReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BEReferenceData.CmdLine
{
	class AdditionalInfoProgram
	{
		public static void Run()
		{
			var downloadFileAbsolutePath = new List<string>();

			var downloadUrl = string.Empty;
			var cusCodeList1 = new List<RefCusCodeList>();
			var cusCodeList2 = new List<RefCusCodeList>();

			downloadUrl = "https://financien.belgium.be/sites/default/files/Customs/Ondernemingen/Douane/Enig-document/6_6_Bijvoegsel_6a_6.doc";
			downloadFileAbsolutePath.Add(WebClientHelper.DownloadFile(downloadUrl, "AdditionalInfo6a.doc"));
			downloadUrl = "https://finances.belgium.be/sites/default/files/Customs/Ondernemingen/Douane/Enig-document/6_6_Appendice_6a_4.doc";
			downloadFileAbsolutePath.Add(WebClientHelper.DownloadFile(downloadUrl, "AdditionalInfo6a_fr.doc"));

			cusCodeList1 = NPOIWordParser.ReadAdditionalInfoDocFileIntoResults(downloadFileAbsolutePath, new List<string>() { "Code" }, new List<string>() { "Onderwerp", "Objet" }, new List<string>() { @"^\d+" });
			downloadFileAbsolutePath.ForEach(File.Delete);
			downloadFileAbsolutePath.Clear();

			downloadUrl = "https://financien.belgium.be/sites/default/files/Customs/Ondernemingen/Douane/Enig-document/6_6_Bijvoegsel_6c_5.doc";
			downloadFileAbsolutePath.Add(WebClientHelper.DownloadFile(downloadUrl, "AdditionalInfo6c.doc"));
			downloadUrl = "https://finances.belgium.be/sites/default/files/Customs/Ondernemingen/Douane/Enig-document/6_6_Appendice_6c_4.doc";
			downloadFileAbsolutePath.Add(WebClientHelper.DownloadFile(downloadUrl, "AdditionalInfo6c_fr.doc"));

			cusCodeList2 = NPOIWordParser.ReadAdditionalInfoDocFileIntoResults(downloadFileAbsolutePath, new List<string>() { "Code" }, new List<string>() { "Champ d’application", "Toepassingsgebied", "Vermeldingen", "Mentions", "Vermelding vak 44 ED", "Vermelding vak 31 ED", "Mention case 31 D.A.U.", "Mention case 44 D.A.U." }, new List<string>() { @"^\d.*-.+", "^ALG.*" });

			downloadFileAbsolutePath.ForEach(File.Delete);
			downloadFileAbsolutePath.Clear();

			var downloadDir = ApplicationConfig.DownloadDir;

#pragma warning disable CA1031 // Rethrow to preserve stack details
			try
			{
				XMLGeneration.GenerateXml("BE Additional Info", "RefCusCodeListZZ_BE_ADDIN.xml", cusCodeList1.Union(cusCodeList2).ToList(), Constants.ZZRefCusCodeList.AdditionalInfo, DateTime.Now, downloadDir);
			}
			catch
			{
			}
#pragma warning restore CA1031 // Rethrow to preserve stack details
		}
	}
}
