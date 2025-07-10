using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	public class ACEDrawbackNoticeOfIntentTest : TestCaseWithFactory
	{
		[TestDate(2016, 8, 16)]
		public void TestNoticeOfIntent()
		{
			var noticeOfIntent = new ACEDrawbackNoticeOfIntent("P", "ABC", "DEF", "11111111", ZDateTime.Today);
			var noticeOfIntentInfo = (IACEDrawbackNoticeOfIntent)noticeOfIntent;
			AssertEquals("P", noticeOfIntentInfo.RecordIndicator);
			AssertEquals("ABC", noticeOfIntentInfo.NameOfCBPPersonnel);
			AssertEquals("DEF", noticeOfIntentInfo.CBPPersonnelBadge);
			AssertEquals("11111111", noticeOfIntentInfo.CBPPersonnelPhone);
			AssertEquals(ZDateTime.Today, noticeOfIntentInfo.ProcessingExaminAtionDate);
		}
	}
}
