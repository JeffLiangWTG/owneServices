using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWControllingAgencyDeliveryNotificationMessageHelper))]
	sealed class TWControllingAgencyDeliveryNotificationMessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new TWControllingAgencyDeliveryNotificationMessageHelper(Factory.New<TWMessage>());
		}

		[ExpectNoExceptions]
		public void TestToHtml()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			foreach (var messageType in new string[] { "101", "201", "207", "301", "31A", "31D", "401", "601", "603" })
			{
				testMessage.EM_MessageType = messageType;
				testMessage.EM_MessageText = TWXmlTestCaseWithFactory.GetExpectedMessageXML($"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.ControllingAgencyDeliveryNotificationMessages.{messageType}.xml");
				var helper = TWMessageHelper.NewIncomingHelper(testMessage);
				var actual = helper.ToHtml();
				var expected = TWXmlTestCaseWithFactory.GetExpectedMessageXML($"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.ControllingAgencyDeliveryNotificationMessages.{messageType}.html").ToString();
				CombineAssertions(() =>
				{
					NUnit.Framework.Assert.That(helper, NUnit.Framework.Is.TypeOf<TWControllingAgencyDeliveryNotificationMessageHelper>());
					NUnit.Framework.Assert.That(actual, NUnit.Framework.Is.EqualTo(expected), messageType);
				});
			}
		}

		[ExpectNoExceptions]
		public void TestGetErrorCodeAndDescriptionFromCode()
		{
			var list = Factory.GetCachedValue<ErrorCodeList>();
			var helper = new TWControllingAgencyDeliveryNotificationMessageHelper(Factory.New<TWMessage>());
			foreach (var code in list.GetAllCodes())
			{
				NUnit.Framework.Assert.That(helper.GetErrorCodeAndDescriptionFromCode(code), NUnit.Framework.Is.EqualTo(code + " " + list.GetDescriptionFromCode(code)));
			}
		}

		[ExpectNoExceptions]
		public void TestGetEventTypeDescriptionFromCode()
		{
			var list = Factory.GetCachedValue<EventTypeCodeList>();
			var helper = new TWControllingAgencyDeliveryNotificationMessageHelper(Factory.New<TWMessage>());
			foreach (var code in list.GetAllCodes())
			{
				NUnit.Framework.Assert.That(helper.GetEventTypeDescriptionFromCode(code), NUnit.Framework.Is.EqualTo(list.GetDescriptionFromCode(code)));
			}
		}

		[ExpectNoExceptions]
		public void TestGetMessageTypeDescriptionFromCode()
		{
			var list = Factory.GetCachedValue<EventTypeCodeList>();
			var helper = new TWControllingAgencyDeliveryNotificationMessageHelper(Factory.New<TWMessage>());
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(helper.GetMessageTypeDescriptionFromCode("101"), NUnit.Framework.Is.EqualTo("產地證明申辦訊息 NX101"));
				NUnit.Framework.Assert.That(helper.GetMessageTypeDescriptionFromCode("201"), NUnit.Framework.Is.EqualTo("同意文件申辦訊息 NX201_01"));
				NUnit.Framework.Assert.That(helper.GetMessageTypeDescriptionFromCode("207"), NUnit.Framework.Is.EqualTo("同意文件申辦註銷或展期訊息 NX201_07"));
				NUnit.Framework.Assert.That(helper.GetMessageTypeDescriptionFromCode("301"), NUnit.Framework.Is.EqualTo("報驗申辦訊息 NX301"));
				NUnit.Framework.Assert.That(helper.GetMessageTypeDescriptionFromCode("31A"), NUnit.Framework.Is.EqualTo("飼料輸入報驗申辦訊息 NX301_AX"));
				NUnit.Framework.Assert.That(helper.GetMessageTypeDescriptionFromCode("31D"), NUnit.Framework.Is.EqualTo("酒類查驗申辦訊息 NX301_DN"));
				NUnit.Framework.Assert.That(helper.GetMessageTypeDescriptionFromCode("401"), NUnit.Framework.Is.EqualTo("檢疫申辦訊息 NX401"));
				NUnit.Framework.Assert.That(helper.GetMessageTypeDescriptionFromCode("601"), NUnit.Framework.Is.EqualTo("輸入食品及中藥材報驗申辦訊息 NX601"));
				NUnit.Framework.Assert.That(helper.GetMessageTypeDescriptionFromCode("603"), NUnit.Framework.Is.EqualTo("輸入醫療器材及藥品報驗申辦訊息 NX603"));
			});
		}
	}
}
