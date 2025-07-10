using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Recruiter.Business;

namespace Enterprise.Recruiter.Module.Testing
{
	public class ExportTranslationsHelperTest : TestCaseWithFactory
	{
		public void TestTypeFilter()
		{
			var examCampaign1 = Factory.NewWithValidTestData<LearningCentreCampaign>();
			examCampaign1.G0_CampaignID = "TEM99900088";
			var examCampaign2 = Factory.NewWithValidTestData<LearningCentreCampaign>();
			examCampaign2.G0_CampaignID = "TEM99900099";
			using (var testHelper = new CustomizableDataTestHelper())
			{
				var tempDir = Temp.GetNewTempSubdirectory();
				try
				{
					var collection = new LearningCentreCampaignCollection(Factory);
					var helper = new ExportTranslationsHelperForTest(testHelper.CustomizableDataResourceStrings.Source);
					var message = helper.Run(tempDir, collection);
					AssertEquals($"Export has been completed. The files can be found at {tempDir}.", message);
					AssertEquals("Should create a sub folder for each campaign", 2, helper.CampaignSubFolder.Count);
					AssertEquals(true, helper.CampaignSubFolder.Any(c => c.EndsWith("TEM99900088")));
					AssertEquals(true, helper.CampaignSubFolder.Any(c => c.EndsWith("TEM99900099")));
					AssertEquals("Should create a sub folder for each campaign", 4, helper.SavedFiles.Count);
					var key1 = helper.SavedFiles.Keys.FirstOrDefault(s => s.Contains("TEM99900088") && s.Contains("FR-FR"));
					var value1 = helper.SavedFiles[key1];
					Assert(value1.Contains("quatre"));
					var key2 = helper.SavedFiles.Keys.FirstOrDefault(s => s.Contains("TEM99900099") && s.Contains("FR-FR"));
					var value2 = helper.SavedFiles[key2];
					Assert(value2.Contains("huit"));
					var key3 = helper.SavedFiles.Keys.FirstOrDefault(s => s.Contains("TEM99900099") && s.Contains("IT-IT"));
					AssertNull("Should not find untranslated resources", key3);
				}
				finally
				{
					Directory.Delete(tempDir, true);
				}
			}
		}
	}
}
