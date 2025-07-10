using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaMessage))]
	sealed class AsycudaMessageTest : Enterprise.Messaging.Testing.EDIMessageTest
	{
		public void TestShouldSupportChineseCharacters()
		{
			message.EM_MessageText = "嘿MESSAGE";
			message.Validation.ValidateEM_MessageText();
			Assert("No error.", !message.EM_MessageTextInfo.HasError(EnglishCharactersValidation.GetNotificationMessage(message.EM_MessageTextInfo)));
		}

		public void TestGetMessageReferenceNumber()
		{
			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				CombineAssertions(() =>
				{
					Factory.Save();
					AssertEquals("0000000001", message.EM_MessageNum);

					var message2 = Factory.New<AsycudaMessage>();
					Factory.Save();
					AssertEquals("0000000002", message2.EM_MessageNum);

					var expectedNumberFountain = Env.NumberFountains.GetOutgoingTWCustomsMessageNumber();
					expectedNumberFountain.SetNext(Factory, 13);

					var message3 = Factory.New<AsycudaMessage>();
					Factory.Save();
					AssertEquals("0000000013", message3.EM_MessageNum);
					var message4 = Factory.New<AsycudaMessage>();
					Factory.Save();
					AssertEquals("0000000014", message4.EM_MessageNum);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<AsycudaMessage>();
		}
		AsycudaMessage message;
	}
}
