using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class BlockControlReaderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var zza1 = new ZZZA() { StringA = "A1", DateA = ZDate.BrettsBirthday.AddDays(1), DecimalA = 1m, IntA = 1, ShortA = 1 };
			var zzb1 = new ZZZB() { StringB = "B1", DateB = ZDate.BrettsBirthday.AddDays(2), DecimalB = 2m, IntB = 2, ShortB = 2 };
			var zzc1 = new ZZZC() { StringC = "C1", DateC = ZDate.BrettsBirthday.AddDays(3), DecimalC = 3m, IntC = 3, ShortC = 3 };
			var zzy1 = new ZZZY() { StringY = "Y1", DateY = ZDate.BrettsBirthday.AddDays(4), DecimalY = 4m, IntY = 4, ShortY = 4 };
			var zzz1 = new ZZZZ() { StringZ = "Z1", DateZ = ZDate.BrettsBirthday.AddDays(5), DecimalZ = 5m, IntZ = 5, ShortZ = 5 };

			var reader = new BlockControlReader("", new string[] { "BL!S" }, ApplicationIdentifierCodeList.DummyForTesting1);
			AssertNull(reader.B);
			AssertNull(reader.Y);

			var message = Factory.New<CBPMessageForTesting>();
			message.EM_MessageText = zzb1.Serialise() + zzc1.Serialise() + zza1.Serialise() + zzz1.Serialise() + zzy1.Serialise();

			reader = new BlockControlReader(message.EM_MessageText, new string[] { CBPEDIInterchange.ApplicationCodeForTesting }, ApplicationIdentifierCodeList.DummyForTesting1);
			AssertEquals("", ErrorReporter.LastKeyReported);
			AssertEquals("", ErrorReporter.LastMessageReported);
			zzb1 = (ZZZB)reader.B;
			AssertEquals(ApplicationIdentifierCodeList.DummyForTesting1, zzb1.ApplicationIdentifier);
			AssertEquals(ZDate.BrettsBirthday.AddDays(2), zzb1.DateB);
			AssertEquals(2m, zzb1.DecimalB);
			AssertEquals(2, zzb1.IntB);
			AssertEquals((short)2, zzb1.ShortB);
			AssertEquals("B1", zzb1.StringB);
			zzy1 = (ZZZY)reader.Y;
			AssertEquals(ZDate.Empty, zzy1.DateY);
			AssertEquals(ZDecimal.Zero, zzy1.DecimalY);
			AssertEquals(ZInt.Zero, zzy1.IntY);
			AssertEquals(ZShort.Zero, zzy1.ShortY);
			AssertEquals(ZString.Empty, zzy1.StringY);
			AssertEquals(typeof(BlockControlReader.OutputMessageBlockEnumerator), reader.GetEnumerator().GetType());
			AssertEquals(typeof(BlockControlReader.OutputMessageBlockEnumerator), reader.GetOutputMessageEnumerator().GetType());
			AssertEquals(typeof(BlockControlReader.InputMessageBlockEnumerator), reader.GetInputMessageEnumerator().GetType());

			using (var messageTextReader = message.GetEM_MessageTextReader())
			{
				reader = new BlockControlReader(messageTextReader, new string[] { CBPEDIInterchange.ApplicationCodeForTesting }, ApplicationIdentifierCodeList.DummyForTesting1);
				AssertEquals("", ErrorReporter.LastKeyReported);
				AssertEquals("", ErrorReporter.LastMessageReported);
				zzb1 = (ZZZB)reader.B;
				AssertEquals(ApplicationIdentifierCodeList.DummyForTesting1, zzb1.ApplicationIdentifier);
				AssertEquals(ZDate.BrettsBirthday.AddDays(2), zzb1.DateB);
				AssertEquals(2m, zzb1.DecimalB);
				AssertEquals(2, zzb1.IntB);
				AssertEquals((short)2, zzb1.ShortB);
				AssertEquals("B1", zzb1.StringB);
				zzy1 = (ZZZY)reader.Y;
				AssertEquals(ZDate.Empty, zzy1.DateY);
				AssertEquals(ZDecimal.Zero, zzy1.DecimalY);
				AssertEquals(ZInt.Zero, zzy1.IntY);
				AssertEquals(ZShort.Zero, zzy1.ShortY);
				AssertEquals(ZString.Empty, zzy1.StringY);
				AssertEquals(typeof(BlockControlReader.OutputMessageBlockEnumerator), reader.GetEnumerator().GetType());
				AssertEquals(typeof(BlockControlReader.OutputMessageBlockEnumerator), reader.GetOutputMessageEnumerator().GetType());
				AssertEquals(typeof(BlockControlReader.InputMessageBlockEnumerator), reader.GetInputMessageEnumerator().GetType());
			}
		}

		public void TestIsYBlockData()
		{
			var reader = new BlockControlReader(new ZZZB().Serialise(), new string[] { CBPEDIInterchange.ApplicationCodeForTesting }, ApplicationIdentifierCodeList.DummyForTesting1);
			AssertEquals(true, reader.IsYBlockData(new char[] { 'Z', '¿', 'º', 'Y' }));
			AssertEquals(false, reader.IsYBlockData(new char[] { 'Z', '¿', 'º', 'Z' }));
			AssertEquals(false, reader.IsYBlockData(new char[] { 'Z', '¿', 'º', 'B' }));
			AssertEquals(false, reader.IsYBlockData(new char[] { 'A', '¿', 'º', 'Y' }));
			AssertEquals(false, reader.IsYBlockData(new char[] { 'Z', 'A', '¿', 'Y' }));
		}
	}
}
