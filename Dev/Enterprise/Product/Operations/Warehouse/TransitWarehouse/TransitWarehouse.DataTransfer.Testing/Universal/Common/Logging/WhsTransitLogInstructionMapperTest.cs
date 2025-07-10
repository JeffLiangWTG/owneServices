using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class WhsTransitLogInstructionMapperTest : TestCaseWithFactory
	{
		public void TestGetMessageByCode()
		{
			var msg = WhsTransitLogInstructionMapper.GetMessageByCode(WhsTransitLogInstructionMapper.Codes.DGClassLimit, "test-UNDG-Class");
			var expectedMessage = "The packages on this Receive/Dispatch Instruction could not be created as the UNDG Class threshold on the Transit Warehouse is set to zero, and the warehouse is not currently permitted to handle UNDG Class test-UNDG-Class goods.";
			AssertEquals("Should be UNDG Class error message", expectedMessage, msg);

			msg = WhsTransitLogInstructionMapper.GetMessageByCode(WhsTransitLogInstructionMapper.Codes.DGCountryReferenceLimit, "test-DCR");
			expectedMessage = "The packages on this Receive/Dispatch Instruction could not be created as the UNDG Country Reference threshold on the Transit Warehouse is set to zero, and the warehouse is not currently permitted to handle UNDG Country Reference test-DCR goods.";
			AssertEquals("Should be DG Country Reference error message", expectedMessage, msg);

			msg = WhsTransitLogInstructionMapper.GetMessageByCode(WhsTransitLogInstructionMapper.Codes.DGSubstanceLimit, "test-DG");
			expectedMessage = "The packages on this Receive/Dispatch Instruction could not be created as the UNDG Substance threshold on the Transit Warehouse is set to zero, and the warehouse is not currently permitted to handle UNDG Substance test-DG goods.";
			AssertEquals("Should be DG Substance error message", expectedMessage, msg);
		}

		public void TestGetMessageFromLog()
		{
			var expectedLog = "WhsTransitLogInstructionMapper test message about log";
			var instructionMapper = new WhsTransitLogInstructionMapper();
			instructionMapper.AddMessageFormatsForTesting(expectedLog);

			var log = "Mock log to test WhsTransitLogInstructionMapper test message about log";
			var msg = instructionMapper.GetMessageFromLog(log);

			AssertEquals("Should be expectedLog", expectedLog, msg);
		}

		public void TestMultiLanguage()
		{
			var errorMessageInEnglish = "test{0}test";
			var errorMessageInChinese = "测试{0}测试";
			var eng = Res.GetLanguageInstance(Core.SharedConstants.Languages.EnglishAmerican).UseMockData();
			var chs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData();
			chs.SetResourceGetter(key => new ResourceStringData(key, errorMessageInChinese));
			eng.SetResourceGetter(key => new ResourceStringData(key, errorMessageInEnglish));

			var instructionMapper = new WhsTransitLogInstructionMapper();
			instructionMapper.AddMessageFormatsForTesting(string.Format(errorMessageInEnglish, "MultiLanguage"));
			var log = "TWH/r/n " + string.Format(errorMessageInEnglish, "MultiLanguage");
			var message = instructionMapper.GetMessageFromLog(log);

			AssertEquals("should be in English", "testMultiLanguagetest", message);

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				instructionMapper.AddMessageFormatsForTesting(string.Format(errorMessageInChinese, "多语言"));
				log = "仓库/r/n " + string.Format(errorMessageInChinese, "多语言");
				message = instructionMapper.GetMessageFromLog(log);

				AssertEquals("should be in ChineseSimplified", "测试多语言测试", message);
			}
		}
	}
}
