using System;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class NAFTADutyDeferralProcessorTest : ABIProcessorTest<NAFTADutyDeferralProcessor, APLA, APLB, APLY>
	{
		protected override void EndToEndCore()
		{
			var generator = (ABIOutputBlockControlGenerator)OutputBlockControlGeneratorTestHelper.GetBlockControlGenerator(ApplicationIdentifierCodeList.Codes.NAFTADutyDeferralResponse,
			"D01",
			"DER           523ENTRY NBR MUST BE NUMERIC",
			"DER              DEFERRED CLAIM REJECTED");

			ProcessMessage(generator);

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate(EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("NAFTA Duty Deferral"); }));
			Assert("Mail was not sent in correct format", sentMail.Body.Contains("NAFTA Duty Deferral Response (Failure) for "));
		}
	}
}
