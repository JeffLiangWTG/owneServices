using System;
using System.IO;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;
using CargoWise.RefDbRepo.PLReferenceData.Business.Taric4.BaseFile;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Taric4;

[TestFixture]
sealed class TaricBaseTest
{
	[TestCase]
	public void TestUpdateAndSaveTaricBaseFile_SaveToFile()
	{
		var testData = new IsztarHistoryResponse { ResultsInfo = new ResultsInfo() { databaseDate = DateTime.UtcNow } };
		var testInputFilePath = Path.GetTempFileName();
		XmlParser.SerializeToFile(testData, testInputFilePath);
		var testFilePath = Path.GetTempFileName();
		var testFolderPath = Directory.CreateTempSubdirectory().FullName;

		try
		{
			Assert.Multiple(() =>
			{
				DateTime initialWriteTime = DateTime.UtcNow.AddMinutes(-1);
				File.SetLastWriteTimeUtc(testFilePath, initialWriteTime);
				var actualWriteTime = File.GetLastWriteTimeUtc(testFilePath);
				Assert.That(actualWriteTime, Is.EqualTo(initialWriteTime), "Before update and save: the LastWriteTime should be equal to initial value.");

				_ = TaricBase.MergeUpdatesAndSaveTaricBaseFile(testInputFilePath, testFolderPath, testFilePath);
				actualWriteTime = File.GetLastWriteTimeUtc(testFilePath);
				Assert.That(actualWriteTime, Is.GreaterThan(initialWriteTime) , "After update and save: the LastWriteTime should be greater than initial value.");
			});
		}
		finally
		{
			Directory.Delete(testFolderPath, true);
			File.Delete(testInputFilePath);
			File.Delete(testFilePath);
		}
	}	
}
