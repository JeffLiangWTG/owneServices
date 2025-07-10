using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class BlockControlGeneratorTest : TestCaseWithFactory
	{
		public void TestAddMessageBlock()
		{
			var obj = new BlockControlGeneratorForTesting();
			var zza1 = new ZZZA();
			var zzb1 = new ZZZB();
			var zzc1 = new ZZZC();
			var zzy1 = new ZZZY();
			var zzz1 = new ZZZZ();
			obj.AddMessageBlock(zza1);
			obj.AddMessageBlocks(new MessageBlock[] { zzb1, zzc1, zzy1 });
			obj.AddMessageBlock(zzz1);
			obj.AddMessageBlocks(new MessageBlock[] { zza1, zzz1 });
			AssertEquals(7, obj.MessageBlocks.Count);
			AssertEquals(zza1, obj.MessageBlocks[0]);
			AssertEquals(zzb1, obj.MessageBlocks[1]);
			AssertEquals(zzc1, obj.MessageBlocks[2]);
			AssertEquals(zzy1, obj.MessageBlocks[3]);
			AssertEquals(zzz1, obj.MessageBlocks[4]);
			AssertEquals(zza1, obj.MessageBlocks[5]);
			AssertEquals(zzz1, obj.MessageBlocks[6]);
			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot add more blocks - have already read them", () => obj.AddMessageBlock(zza1));
			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot add more blocks - have already read them", () => obj.AddMessageBlocks(new MessageBlock[] { zza1, zzz1 }));
		}

		public void TestDeserialise_UsingString()
		{
			var zza1 = new ZZZA() { StringA = "A1" };
			var zzb1 = new ZZZB() { StringB = "B1" };
			var zzc1 = new ZZZC() { StringC = "C1" };
			var zzy1 = new ZZZY() { StringY = "Y1" };
			var zzz1 = new ZZZZ() { StringZ = "Z1" };

			AssertExceptionThrown(typeof(InvalidMessageFormatException), "message does not start with a 'Z¿ºB' block", () => new BlockControlGeneratorForTesting().Deserialise(zza1.Serialise()));
			AssertExceptionThrown(typeof(InvalidMessageFormatException), "message does not end with a 'Z¿ºY' block", () => new BlockControlGeneratorForTesting().Deserialise(zzb1.Serialise() + zzc1.Serialise() + zzz1.Serialise()));
			var obj = new BlockControlGeneratorForTesting();
			AssertNoExceptionThrown(() => obj.Deserialise(zzb1.Serialise() + zzc1.Serialise() + zza1.Serialise() + zzz1.Serialise() + zzy1.Serialise()));

			AssertEquals(3, obj.MessageBlocks.Count);
			AssertEquals(zzb1, obj.B);
			AssertEquals(zzc1, obj.MessageBlocks[0]);
			AssertEquals(zza1, obj.MessageBlocks[1]);
			AssertEquals(zzz1, obj.MessageBlocks[2]);
			AssertEquals(zzy1, obj.Y);
		}

		public void TestDeserialise_UsingTextReader()
		{
			var zza1 = new ZZZA() { StringA = "A1" };
			var zzb1 = new ZZZB() { StringB = "B1" };
			var zzc1 = new ZZZC() { StringC = "C1" };
			var zzy1 = new ZZZY() { StringY = "Y1" };
			var zzz1 = new ZZZZ() { StringZ = "Z1" };

			AssertExceptionThrown(typeof(InvalidMessageFormatException), "message does not start with a 'Z¿ºB' block", () => new BlockControlGeneratorForTesting().Deserialise(zza1.Serialise()));
			AssertExceptionThrown(typeof(InvalidMessageFormatException), "message does not end with a 'Z¿ºY' block", () => new BlockControlGeneratorForTesting().Deserialise(zzb1.Serialise() + zzc1.Serialise() + zzz1.Serialise()));
			var obj = new BlockControlGeneratorForTesting();
			AssertNoExceptionThrown(() => obj.Deserialise(zzb1.Serialise() + zzc1.Serialise() + zza1.Serialise() + zzz1.Serialise() + zzy1.Serialise()));

			AssertEquals(3, obj.MessageBlocks.Count);
			AssertEquals(zzb1, obj.B);
			AssertEquals(zzc1, obj.MessageBlocks[0]);
			AssertEquals(zza1, obj.MessageBlocks[1]);
			AssertEquals(zzz1, obj.MessageBlocks[2]);
			AssertEquals(zzy1, obj.Y);
		}

		public void TestCreateMessage()
		{
			var obj = new BlockControlGeneratorForTesting();
			var zza1 = new ZZZA() { StringA = "A1", DateA = ZDate.BrettsBirthday.AddDays(1), DecimalA = 1m, IntA = 1, ShortA = 1 };
			var zzb1 = new ZZZB() { StringB = "B1", DateB = ZDate.BrettsBirthday.AddDays(2), DecimalB = 2m, IntB = 2, ShortB = 2 };
			var zzc1 = new ZZZC() { StringC = "C1" };
			var zzy1 = new ZZZY() { StringY = "Y1" };
			var zzz1 = new ZZZZ() { StringZ = "Z1" };

			obj.Deserialise(zzb1.Serialise() + zzc1.Serialise() + zza1.Serialise() + zzz1.Serialise() + zzy1.Serialise());
			var message = obj.CreateMessage<CBPMessageForTesting>(Factory);
			AssertEquals(CBPEDIInterchange.ApplicationCodeForTesting, message.EM_ApplicationCode);
			AssertEquals(ApplicationIdentifierCodeList.DummyForTesting1, message.EM_MessageType);
			AssertEquals(CBPEDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(CBPEDIMessage.Status.Queued, message.EM_Status);
			AssertEquals("Z¿ºB09207100000000020000200002B1                                                Z¿ºC                          C1                                                Z¿ºA09197100000000010000100001A1                                                Z¿ºZ                          Z1                                                Z¿ºY                          Y1", message.EM_MessageText);
		}

		public void TestSerialise()
		{
			var obj = new BlockControlGeneratorForTesting();
			var zza1 = new ZZZA() { StringA = "A1", DateA = ZDate.BrettsBirthday.AddDays(1), DecimalA = 1m, IntA = 1, ShortA = 1 };
			var zzb1 = new ZZZB() { StringB = "B1", DateB = ZDate.BrettsBirthday.AddDays(2), DecimalB = 2m, IntB = 2, ShortB = 2 };
			var zzc1 = new ZZZC() { StringC = "C1", DateC = ZDate.BrettsBirthday.AddDays(3), DecimalC = 3m, IntC = 3, ShortC = 3 };
			var zzy1 = new ZZZY() { StringY = "Y1", DateY = ZDate.BrettsBirthday.AddDays(4), DecimalY = 4m, IntY = 4, ShortY = 4 };
			var zzz1 = new ZZZZ() { StringZ = "Z1", DateZ = ZDate.BrettsBirthday.AddDays(5), DecimalZ = 5m, IntZ = 5, ShortZ = 5 };

			obj.Deserialise(zzb1.Serialise() + zzc1.Serialise() + zza1.Serialise() + zzz1.Serialise() + zzy1.Serialise());
			AssertEquals("Z¿ºB09207100000000020000200002B1                                                Z¿ºC09217100000000030000300003C1                                                Z¿ºA09197100000000010000100001A1                                                Z¿ºZ09237100000000050000500005Z1                                                Z¿ºY09227100000000040000400004Y1                                                ", obj.Serialise());
			AssertMultilineASCIIEquals("", @"------------------ZZZB------------------
 Date B (5-10)     :20-Sep-71
 Decimal B (11-20) :2
 Int B (21-25)     :2
 Short B (26-30)   :2
 String B (31-70)  :B1

------------------ZZZC------------------
 Date C (5-10)     :21-Sep-71
 Decimal C (11-20) :3
 Int C (21-25)     :3
 Short C (26-30)   :3
 String C (31-70)  :C1

------------------ZZZA------------------
 Date A (5-10)     :19-Sep-71
 Decimal A (11-20) :1
 Int A (21-25)     :1
 Short A (26-30)   :1
 String A (31-70)  :A1

------------------ZZZZ------------------
 Date Z (5-10)     :23-Sep-71
 Decimal Z (11-20) :5
 Int Z (21-25)     :5
 Short Z (26-30)   :5
 String Z (31-70)  :Z1

------------------ZZZY------------------
 Date Y (5-10)     :22-Sep-71
 Decimal Y (11-20) :4
 Int Y (21-25)     :4
 Short Y (26-30)   :4
 String Y (31-70)  :Y1
", obj.Serialise(true));
			AssertEquals("Z¿ºB09207100000000020000200002B1                                                Z¿ºC09217100000000030000300003C1                                                Z¿ºA09197100000000010000100001A1                                                Z¿ºZ09237100000000050000500005Z1                                                Z¿ºY09227100000000040000400004Y1                                                ", obj.Serialise(false));
		}

		public void TestSerialiseTo80ByteBlocks()
		{
			var obj = new BlockControlGeneratorForTesting();
			var zza1 = new ZZZA() { StringA = "A1", DateA = ZDate.BrettsBirthday.AddDays(1), DecimalA = 1m, IntA = 1, ShortA = 1 };
			var zzb1 = new ZZZB() { StringB = "B1", DateB = ZDate.BrettsBirthday.AddDays(2), DecimalB = 2m, IntB = 2, ShortB = 2 };
			var zzc1 = new ZZZC() { StringC = "C1", DateC = ZDate.BrettsBirthday.AddDays(3), DecimalC = 3m, IntC = 3, ShortC = 3 };
			var zzy1 = new ZZZY() { StringY = "Y1", DateY = ZDate.BrettsBirthday.AddDays(4), DecimalY = 4m, IntY = 4, ShortY = 4 };
			var zzz1 = new ZZZZ() { StringZ = "Z1", DateZ = ZDate.BrettsBirthday.AddDays(5), DecimalZ = 5m, IntZ = 5, ShortZ = 5 };

			obj.Deserialise(zzb1.Serialise() + zzc1.Serialise() + zza1.Serialise() + zzz1.Serialise() + zzy1.Serialise());
			AssertMultilineASCIIEquals("", @"Z¿ºB09207100000000020000200002B1                                                
Z¿ºC09217100000000030000300003C1                                                
Z¿ºA09197100000000010000100001A1                                                
Z¿ºZ09237100000000050000500005Z1                                                
Z¿ºY09227100000000040000400004Y1                                                ", obj.SerialiseTo80ByteBlocks());
		}

		public void TestSerialiseWithISerialiserWithOrigAppID()
		{
			var inpn03 = new INPN03()
			{
				CityName = "BOB",
				StateProvinceCode = "CA",
				PostalCode = "PO123",
				CountryCode = "US"
			};

			var inpn00 = new INPN00()
			{
				EntityCode = "N2",
				EntityName = "NOTIFIER"
			};
			var inpn01 = new INPN01()
			{
				NotifyPartyNameCode = "PARTY NAME",
				NotifyPartyAddressLine1 = "ADDRESS 1"
			};
			var inpn02 = new INPN02()
			{
				EntitysAddressLine = "ADDRESS SINGLE",
				EntitysAddressLine1 = "ADDRESS LINE 1"
			};

			var obj = new BlockControlGeneratorForTesting();

			obj.AddMessageBlock(inpn01);
			obj.AddMessageBlock(inpn02);
			obj.AddMessageBlock(inpn03);
			AssertMultilineASCIIEquals("N01 List", @"------------------ZZZB------------------

-----------------INPN01-----------------
 Notify Party Name Code (4-38)      :PARTY NAME
 Notify Party Address Line1 (39-73) :ADDRESS 1

-----------------INPN02-----------------
 Entitys Address Line (4-38)   :ADDRESS SINGLE
 Entitys Address Line1 (39-73) :ADDRESS LINE 1

--------------INPN03ForN01--------------
 Notify Party Telephone Or Telex Number (4-38) :BOB                CAPO123    US

------------------ZZZY------------------
", obj.Serialise(true, AMSApplicationIdentifierCodeList.Codes.ManifestAmendment));

			obj = new BlockControlGeneratorForTesting();
			obj.AddMessageBlock(inpn00);
			obj.AddMessageBlock(inpn02);
			obj.AddMessageBlock(inpn03);

			AssertMultilineASCIIEquals("N00 List", @"------------------ZZZB------------------

-----------------INPN00-----------------
 Entity Code (4-6)  :N2
 Entity Name (7-41) :NOTIFIER

-----------------INPN02-----------------
 Entitys Address Line (4-38)   :ADDRESS SINGLE
 Entitys Address Line1 (39-73) :ADDRESS LINE 1

-----------------INPN03-----------------
 City Name (4-22)            :BOB
 State Province Code (23-24) :CA
 Postal Code (25-33)         :PO123
 Country Code (34-35)        :US

------------------ZZZY------------------
", obj.Serialise(true, AMSApplicationIdentifierCodeList.Codes.ManifestAmendment));
		}
	}
}
