
using CargoWise.EntityFramework.Testing;
using Enterprise.Edifact.D98A.Messages.CUSRES;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.MessageProcessors.Testing
{
	using NUnit.Framework;

	public class CusResD98AConverterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void Test98AReturns98A()
		{
			string testMessage = @"
UNH+2447+CUSRES:D:98A:UN+B01001001'
BGM+963+00000000'
UNT+3+2447'";
			EDIMessage eDIMessage = Factory.New<EDIMessage>();
			eDIMessage.EM_MessageText = testMessage.Replace("\r", "").Replace("\n", "");
			eDIMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.NewZealandCustoms;
			eDIMessage.IsTransmitMessage = false;

			CusResD98AConverter cusResD98AConverter = new CusResD98AConverter(eDIMessage);
			CUSRESMessage cUSRESMessage = cusResD98AConverter.CusResD98A;
		}

		[ExpectNoExceptions]
		public void Test96BReturns98A()
		{
			string testMessage = @"
UNH+2447+CUSRES:D:96B:UN+B01001001'
BGM+963+00000000'
UNT+3+2447'";
			EDIMessage eDIMessage = Factory.New<EDIMessage>();
			eDIMessage.EM_MessageText = testMessage.Replace("\r", "").Replace("\n", "");
			eDIMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.NewZealandCustoms;
			eDIMessage.IsTransmitMessage = false;

			CusResD98AConverter cusResD98AConverter = new CusResD98AConverter(eDIMessage);
			CUSRESMessage cUSRESMessage = cusResD98AConverter.CusResD98A;
		}

		[ExpectNoExceptions]
		public void Test03AReturns98A()
		{
			string testMessage = @"
UNH+2447+CUSRES:D:03A:UN+B01001001'
BGM+963+00000000'
UNT+3+2447'";
			EDIMessage eDIMessage = Factory.New<EDIMessage>();
			eDIMessage.EM_MessageText = testMessage.Replace("\r", "").Replace("\n", "");
			eDIMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.NewZealandCustoms;
			eDIMessage.IsTransmitMessage = false;

			CusResD98AConverter cusResD98AConverter = new CusResD98AConverter(eDIMessage);
			CUSRESMessage cUSRESMessage = cusResD98AConverter.CusResD98A;
		}
	}
}
