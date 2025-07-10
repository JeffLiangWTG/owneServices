using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.MessageProcessors.Testing
{
	using NUnit.Framework;

	public class CusResD96BConverterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void Test96BReturns96B()
		{
			string testMessage = @"
UNH+2447+CUSRES:D:96B:UN+B01001001'
BGM+963+00000000'
UNT+3+2447'";
			var eDIMessage = Factory.New<EDIMessage>();
			eDIMessage.EM_MessageText = testMessage.Replace("\r", "").Replace("\n", "");
			eDIMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.NewZealandCustoms;
			eDIMessage.IsTransmitMessage = false;

			var cusResD96BConverter = new CusResD96BConverter(eDIMessage);
			var cUSRESMessage = cusResD96BConverter.CusResD96B;
		}

		[ExpectNoExceptions]
		public void Test03AReturns96B()
		{
			string testMessage = @"
UNH+2447+CUSRES:D:03A:UN+B01001001'
BGM+963+00000000'
UNT+3+2447'";
			var eDIMessage = Factory.New<EDIMessage>();
			eDIMessage.EM_MessageText = testMessage.Replace("\r", "").Replace("\n", "");
			eDIMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.NewZealandCustoms;
			eDIMessage.IsTransmitMessage = false;

			var cusResD96BConverter = new CusResD96BConverter(eDIMessage);
			var cUSRESMessage = cusResD96BConverter.CusResD96B;
		}

		[ExpectNoExceptions]
		public void TestUnsolicitedExampleReturns96B()
		{
			string testMessage = @"UNH+553211+CUSRES:D:96B:UN+22326971872015'
BGM+932+61352032:01'
FTX+DIN+++39 LOOSE PACKAGE(S) OR ITEM(S)'
TDT+20++4+++++:::NZ99'
LOC+9+NZAKL'
GIS+819:120:143'
NAD+AL+40342956C:ZZZ:143+FONTERRA LIMITED'
NAD+CB+40342956C:ZZZ:143+FONTERRA LIMITED'
DOC+964+1'
PAC+39++CT'
RFF+HWB:08651411091'
UNT+12+553211'";
			var eDIMessage = Factory.New<EDIMessage>();
			eDIMessage.EM_MessageText = testMessage.Replace("\r", "").Replace("\n", "");
			eDIMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.NewZealandCustoms;
			eDIMessage.IsTransmitMessage = false;

			var cusResD96BConverter = new CusResD96BConverter(eDIMessage);
			var cUSRESMessage = cusResD96BConverter.CusResD96B;
		}
	}
}
