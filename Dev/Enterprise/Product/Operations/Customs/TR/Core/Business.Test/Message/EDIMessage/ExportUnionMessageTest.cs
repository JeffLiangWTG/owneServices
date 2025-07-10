using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(ExportUnionMessage))]
	sealed class ExportUnionMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			var message = Factory.New<ExportUnionMessage>();
			AssertEquals("Default value: EM_ApplicationCode", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.TRCustoms, message.EM_ApplicationCode);
		}

		public void TestMessageNum()
		{
			var declaration = Factory.New<JobDeclaration>();
			var message1 = Factory.New<ExportUnionMessage>();
			message1.EM_LinkUniqueID = declaration.PK;
			message1.EM_LinkTable = JobDeclarationSchema.Constants.TableName;
			message1.EM_MessageType = TRMessageTypes.Codes.EUT;
			Factory.Save();
			AssertEquals("00000000000001", message1.EM_MessageNum);

			var message2 = Factory.New<ExportUnionMessage>();
			message2.EM_LinkUniqueID = declaration.PK;
			message2.EM_LinkTable = JobDeclarationSchema.Constants.TableName;
			message2.EM_MessageType = TRMessageTypes.Codes.EUT;
			Factory.Save();
			AssertEquals("00000000000002", message2.EM_MessageNum);
		}
	}
}
