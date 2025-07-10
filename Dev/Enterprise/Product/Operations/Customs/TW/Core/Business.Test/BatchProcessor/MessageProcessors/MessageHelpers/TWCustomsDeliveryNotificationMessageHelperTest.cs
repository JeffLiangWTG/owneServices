using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWCustomsDeliveryNotificationMessageHelper))]
	sealed class TWCustomsDeliveryNotificationMessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new TWCustomsDeliveryNotificationMessageHelper(Factory.New<TWMessage>());
		}

		[ExpectNoExceptions]
		public void TestToHtml()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			foreach (var messageType in new string[] { "ECD", "ICD", "ADM", "IEA", "FHM" })
			{
				testMessage.EM_MessageType = messageType;
				testMessage.EM_MessageText = TWXmlTestCaseWithFactory.GetExpectedMessageXML($"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.DeliveryNotificationMessages.{messageType}.xml");
				var helper = TWMessageHelper.NewIncomingHelper(testMessage);
				var actual = helper.ToHtml();
				var expected = TWXmlTestCaseWithFactory.GetExpectedMessageXML($"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.DeliveryNotificationMessages.{messageType}.html").ToString();
				CombineAssertions(() =>
				{
					NUnit.Framework.Assert.That(helper, NUnit.Framework.Is.TypeOf<TWCustomsDeliveryNotificationMessageHelper>());
					NUnit.Framework.Assert.That(actual, NUnit.Framework.Is.EqualTo(expected), messageType);
				});
			}
		}

		[ExpectNoExceptions]
		public void TestGetEntryNumberTypeDescriptionFromCode()
		{
			var helper = new TWCustomsDeliveryNotificationMessageHelper(Factory.New<TWMessage>());
			NUnit.Framework.Assert.That(helper.GetMessageTypeDescriptionFromCode("ECD"), NUnit.Framework.Is.EqualTo("出口報單 N5203"));
			NUnit.Framework.Assert.That(helper.GetMessageTypeDescriptionFromCode("ICD"), NUnit.Framework.Is.EqualTo("進口報單 NX5105"));
			NUnit.Framework.Assert.That(helper.GetMessageTypeDescriptionFromCode("ADM"), NUnit.Framework.Is.EqualTo("檢附申辦文件訊息 NX5901"));
			NUnit.Framework.Assert.That(helper.GetMessageTypeDescriptionFromCode("IEA"), NUnit.Framework.Is.EqualTo("進口貨物查驗申請書 N5167"));
			NUnit.Framework.Assert.That(helper.GetMessageTypeDescriptionFromCode("FHM"), NUnit.Framework.Is.EqualTo("進口貨物分艙單 N5101H"));
		}

		[ExpectNoExceptions]
		public void TestEntryNumberTypeDescriptionFromCode()
		{
			var helper = new TWCustomsDeliveryNotificationMessageHelper(Factory.New<TWMessage>());
			NUnit.Framework.Assert.That(helper.GetEntryNumberTypeDescriptionFromCode("IMP"), NUnit.Framework.Is.EqualTo("進口"));
			NUnit.Framework.Assert.That(helper.GetEntryNumberTypeDescriptionFromCode("EXP"), NUnit.Framework.Is.EqualTo("出口"));
		}

		[ExpectNoExceptions]
		public void TestGetEventTypeDescriptionFromCode()
		{
			var list = Factory.GetCachedValue<EventTypeCodeList>();
			var helper = new TWCustomsDeliveryNotificationMessageHelper(Factory.New<TWMessage>());
			foreach (var code in list.GetAllCodes())
			{
				NUnit.Framework.Assert.That(helper.GetEventTypeDescriptionFromCode(code), NUnit.Framework.Is.EqualTo(list.GetDescriptionFromCode(code)));
			}
		}

		[ExpectNoExceptions]
		public void TestGetErrorCodeAndDescriptionFromCode()
		{
			var list = Factory.GetCachedValue<ErrorCodeList>();
			var helper = new TWCustomsDeliveryNotificationMessageHelper(Factory.New<TWMessage>());
			foreach (var code in list.GetAllCodes())
			{
				NUnit.Framework.Assert.That(helper.GetErrorCodeAndDescriptionFromCode(code), NUnit.Framework.Is.EqualTo(code + " " + list.GetDescriptionFromCode(code)));
			}
		}

		[ExpectNoExceptions]
		public void TestGetErrorDescriptionFromCode_E0000()
		{
			var testCode = "E0000";
			var list = Factory.GetCachedValue<ErrorCodeList>();
			var expected = testCode + " " + list.GetDescriptionFromCode(testCode);
			var helper = new TWCustomsDeliveryNotificationMessageHelper(Factory.New<TWMessage>());
			var description = helper.GetErrorCodeAndDescriptionFromCode(testCode);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(description, NUnit.Framework.Is.EqualTo(expected));
				NUnit.Framework.Assert.That(description, NUnit.Framework.Is.EqualTo("E0000 eHub - Client registration不存在或無效"));
			});
		}

		[ExpectNoExceptions]
		public void TestGetErrorSuggestionFromCode_E0000()
		{
			var testCode = "E0000";
			var expected = Factory.GetCachedValue<ErrorSuggestionList>().GetDescriptionFromCode(testCode);
			var helper = new TWCustomsDeliveryNotificationMessageHelper(Factory.New<TWMessage>());
			var suggestion = helper.GetErrorSuggestionFromCode(testCode);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(suggestion, NUnit.Framework.Is.EqualTo(expected));
				NUnit.Framework.Assert.That(suggestion, NUnit.Framework.Is.EqualTo(@"eHub憑證註冊不存在或是無效。請確認用戶郵箱代碼和用戶郵箱密碼是否正確。請重新發送憑證，並確認EDI Interchange成功發送訊息類型:CFG的專責人員憑證 或 訊息類型:FCF的簽審憑證、法人憑證發送成功。如果問題仍然存在，請提交 eRequest，我們將有專業人員為您提供協助。"));
			});
		}
	}
}
