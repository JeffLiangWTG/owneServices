using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class ProcessorTest : TestCaseWithFactory
	{
		public void TestCanAcceptBlockSimple()
		{
			var processor = new HookedMessageProcessorFactoryForTesting.HookedProcessor(2);
			AssertEquals(false, processor.CanAcceptBlock(new ZZZB()));
			AssertEquals(true, processor.CanAcceptBlock(new ZZZC2()));
			AssertEquals(false, processor.CanAcceptBlock(new ZZZC2()));
		}

		public void TestCanAcceptBlockComplex()
		{
			var processor = new ProcessorTestClass2();
			AssertEquals(false, processor.CanAcceptBlock(new ZZZD()));
			AssertEquals(true, processor.CanAcceptBlock(new ZZZDForAll()));
			AssertEquals(true, processor.CanAcceptBlock(new ZZZZ()));
		}

		public void TestMessage()
		{
			var processor = new ProcessorTestClass();
			var message = Factory.New<CBPMessageForTesting>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.Message = message;
			AssertEquals(message, processor.Message);
		}

		public void TestCopyMessageSubTypeFromOriginalMessage()
		{
			var processor = new ProcessorTestClass();
			var originalMessage = Factory.New<CBPMessageForTesting>();
			originalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalMessage.EM_MessageNum = "BL123";
			originalMessage.EM_MessageSubType = "YYY";

			var message = Factory.New<CBPMessageForTesting>();
			message.EM_MessageNum = "BL123";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.Message = message;
			AssertEquals("Message sub type is copied from original message", "YYY", message.EM_MessageSubType);
		}

		public void TestCopyGBFromOriginalMessage()
		{
			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Name = "Dummy $ Compay";
			newCompany.GC_Code = "D$$";
			newCompany.GC_OH_OrgProxy = Env.CurrentCompany.OrganisationPK;

			var newBranch = Factory.New<GlbBranch>();
			newBranch.GB_BranchName = "Dummy $$ Branch 1";
			newBranch.GB_Code = "X$1";
			newBranch.GB_GC = newCompany.PK;
			newBranch.GB_OH_OrgProxy = Env.CurrentBranch.OrganisationPK;

			var newBranch2 = Factory.New<GlbBranch>();
			newBranch2.GB_BranchName = "Dummy $$ Branch 2";
			newBranch2.GB_Code = "X$2";
			newBranch2.GB_GC = newCompany.PK;
			newBranch2.GB_OH_OrgProxy = Env.CurrentBranch.OrganisationPK;
			Factory.Save();

			var processor = new ProcessorTestClass();
			var originalMessage = Factory.New<CBPMessageForTesting>();
			originalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalMessage.EM_MessageNum = "BL123";
			originalMessage.EM_MessageSubType = "YYY";
			originalMessage.EM_GB = newBranch.PK;

			var message = Factory.New<CBPMessageForTesting>();
			message.EM_MessageNum = "BL123";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_GB = newBranch2.PK;
			processor.Message = message;
			AssertEquals("Message GB is copied from original message", newBranch.PK, message.EM_GB);
			AssertEquals("Message LinkedObject is not copied from original message", null, message.EM_LinkedObject);

			var objOrg = Factory.New<OrgHeader>();
			originalMessage.EM_LinkedObject = objOrg;
			processor.Message = message;
			AssertNotNull("Should set from Original emssage", message.EM_LinkedObject);
			AssertEquals("Message LinkedObject is copied from original message", objOrg.PK, message.EM_LinkedObject.PK);
		}
	}
}
