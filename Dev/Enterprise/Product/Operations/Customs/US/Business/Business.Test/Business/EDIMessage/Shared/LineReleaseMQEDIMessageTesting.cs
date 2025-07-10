using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(LineReleaseMQEDIMessage))]
	sealed class LineReleaseMQEDIMessageTesting : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var lineReleaseMessage = Factory.New<LineReleaseMQEDIMessage>();
			AssertEquals("MessageType is set", ApplicationIdentifierCodeList.Codes.LineRelease, lineReleaseMessage.EM_MessageType);
		}

		public void TestLinkedDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarationReference = "TEST";
			var lineReleaseMessage = Factory.New<LineReleaseMQEDIMessage>();
			Factory.Save();
			AssertEquals("LinkedDeclarationReference should be empty", ZString.Empty, lineReleaseMessage.LinkedDeclarationReference);
			lineReleaseMessage.EM_LinkTable = "JobDeclaration";
			lineReleaseMessage.EM_LinkUniqueID = declaration.PK;
			AssertEquals("LinkedDeclarationReference should not be empty", declaration.JE_DeclarationReference, lineReleaseMessage.LinkedDeclarationReference);
		}
	}
}
