using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Recruiter.Module.Testing
{
	class ExportTranslationsHelperForTest : ExportTranslationsHelper
	{
		readonly ICustomizableDataCaptionSource localSource;
		public List<string> CampaignSubFolder = new List<string>();
		public Dictionary<string, string> SavedFiles = new Dictionary<string, string>();
		public ExportTranslationsHelperForTest(ICustomizableDataCaptionSource source)
		{
			localSource = source;
		}

		protected override ICustomizableDataCaptionSource LoadSource(List<ZPropertyInfo> infos, MultilingualString campaignNameMultilingual)
		{
			return localSource;
		}

		protected override string GetCampaignSubFolder(string directoryFolder, string campaignID)
		{
			var folder = Path.Combine(directoryFolder, campaignID);
			CampaignSubFolder.Add(folder);
			return folder;
		}

		protected override bool SaveFile(string path, string content)
		{
			SavedFiles.Add(path, content);
			return true;
		}
	}
}
