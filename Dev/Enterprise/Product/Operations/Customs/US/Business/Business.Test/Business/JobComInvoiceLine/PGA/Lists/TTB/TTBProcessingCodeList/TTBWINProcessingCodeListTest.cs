using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class TTBWINProcessingCodeListTest : TestCaseWithFactory
	{
		public void TestGetList()
		{
			var fullList = new TTBWINProcessingCodeList();
			var expectedList = new Dictionary<string, string>()
			{
				{ TTBWINProcessingCodeList.Codes.T04, "Still Wine not more than 14% Alcohol by Volume" },
				{ TTBWINProcessingCodeList.Codes.T05, "Still Wine more than 14% but not over 21% Alcohol by Volume" },
				{ TTBWINProcessingCodeList.Codes.T06, "Still Wine more than 21% but not over 24% Alcohol by Volume" },
				{ TTBWINProcessingCodeList.Codes.T07, "Port Wine" },
				{ TTBWINProcessingCodeList.Codes.T08, "Champagne" },
				{ TTBWINProcessingCodeList.Codes.T09, "Wine, artificially carbonated" },
				{ TTBWINProcessingCodeList.Codes.T10, "Other Sparkling Wines" },
				{ TTBWINProcessingCodeList.Codes.T11, "Hard Cider" }
			};
			AssertEquals(expectedList.Count, fullList.Count);
			foreach (var item in expectedList)
			{
				AssertEquals(item.Value, fullList.GetDescriptionFromCode(item.Key));
			}
		}
	}
}
