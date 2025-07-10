using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module.Testing
{
	public abstract class EntryStatusListProviderTest : TestCaseWithFactory
	{
		public abstract void TestEntryStatusLists();

		protected void RunStatusCodeListForVariousCountriesTester(string countryCode, string[] expectedStrings, string[] shouldNotContainStrings, string messageType = "", bool isForShipment = false)
		{
			ICodeDescriptionPairList entryStatusList;
			if (isForShipment)
			{
				entryStatusList = Common.EntryStatusListHelper.EntryStatusListForShipments(Factory, countryCode);
			}
			else
			{
				entryStatusList = Common.EntryStatusListHelper.EntryStatusList(Factory, countryCode, messageType);
			}

			foreach (var expectedString in expectedStrings)
			{
				Assert(string.Format("Customs entry status list for country {0} should contain code value {1}", countryCode, expectedString), entryStatusList.ContainsCode(expectedString));
			}

			foreach (var shouldNotContainString in shouldNotContainStrings)
			{
				Assert(string.Format("Customs entry status list for country {0} should NOT contain code value {1}", countryCode, shouldNotContainString), !entryStatusList.ContainsCode(shouldNotContainString));
			}
		}

		protected CodeDescriptionPairList ReplaceNotSendCode(CodeDescriptionPairList codeDescriptionPairList)
		{
			var result = new CodeDescriptionPairList();
			foreach (CodeDescriptionPair pair in codeDescriptionPairList)
			{
				if (pair.Description == "Not Sent")
				{
					result.Add(new CodeDescriptionPair(DeclarationFilterConstants.EntryStatus.NotSentForFilter, pair.Description));
				}
				else
				{
					result.Add(new CodeDescriptionPair(pair.Code, pair.Description));
				}
			}

			return result;
		}
	}
}
