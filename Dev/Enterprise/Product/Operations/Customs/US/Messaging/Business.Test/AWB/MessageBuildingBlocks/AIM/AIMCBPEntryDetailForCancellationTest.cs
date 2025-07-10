using System.Linq;
using Enterprise.Messaging.Business.AWB;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM.Testing
{
	sealed class AIMCBPEntryDetailForCancellationTest : TestCase
	{
		public void TestAIMCBPEntryDetailForCancellation()
		{
			var fields = new AIMCBPEntryDetailForCancellation().GetFieldInfos().ToArray();
			AssertEquals(4, fields.Length);
			var info = fields[0] as AWBMessageBlock.SpecialFieldInfo;
			AssertEquals("CED", info.Value);
			info = fields[1] as AWBMessageBlock.SpecialFieldInfo;
			AssertEquals(SpecialChars.Slant, info.Value);
			info = fields[2] as AWBMessageBlock.SpecialFieldInfo;
			AssertEquals("000", info.Value);
			info = fields[3] as AWBMessageBlock.SpecialFieldInfo;
			AssertEquals(SpecialChars.CRLF, info.Value);
		}
	}
}
