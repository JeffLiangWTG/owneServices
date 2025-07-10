using CargoWise.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class MessageBlockDeserialiserTest : TestCase
	{
		public void TestMessageBlockWithoutAttribute()
		{
			AssertEquals(typeof(ZZZDForAll), new OutputMessageBlockDeserialiser().GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodeForTesting }, ApplicationIdentifierCodeList.DummyForTesting1, "Z¿ºD".PadRight(80)).GetType());
		}

		public void TestDeserialiseGoodBlock()
		{
			AssertEquals(typeof(ZZZB), new OutputMessageBlockDeserialiser().GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodeForTesting }, ApplicationIdentifierCodeList.DummyForTesting1, "Z¿ºB".PadRight(80)).GetType());
		}

		public void TestMatchingOneWithApplicationIdentifierFirst()
		{
			AssertEquals(typeof(ZZZD), new OutputMessageBlockDeserialiser().GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodeForTesting }, ApplicationIdentifierCodeList.DummyForTesting2, "Z¿ºD".PadRight(80)).GetType());
		}

		public void TestVersionForFTZBlocks()
		{
			var ftz10 = "10                                                                              ";
			var ftz10_01 = "10                    N                                                         ";
			var ftz40 = "40                                                                        ABCD  ";
			var ftz40_01 = "40                                                                              ";
			var inputBlockDeserialiser = new InputMessageBlockDeserialiser();
			AssertEquals(typeof(FTZFT10), inputBlockDeserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData, ftz10).GetType());
			AssertEquals(typeof(FTZFT10_01), inputBlockDeserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData, ftz10_01).GetType());
			AssertEquals(typeof(FTZFT40), inputBlockDeserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData, ftz40).GetType());
			AssertEquals(typeof(FTZFT40_01), inputBlockDeserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData, ftz40_01).GetType());

			var outputBlockDeserialiser = new OutputMessageBlockDeserialiser();
			AssertEquals(typeof(FTZZD10), outputBlockDeserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator, ftz10).GetType());
			AssertEquals(typeof(FTZZD10_01), outputBlockDeserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator, ftz10_01).GetType());
			AssertEquals(typeof(FTZZD40), outputBlockDeserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator, ftz40).GetType());
			AssertEquals(typeof(FTZZD40_01), outputBlockDeserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator, ftz40_01).GetType());

			var nf90 = "90                                                                              ";
			var nf90_01 = "90                      20                                                      ";
			AssertEquals(typeof(FTZNF90), outputBlockDeserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone, nf90).GetType());
			AssertEquals(typeof(FTZNF90_01), outputBlockDeserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone, nf90_01).GetType());

			var nf40 = "40                                                                        ABCD  ";
			var nf40_01 = "40                                                                 HK3202       ";
			AssertEquals(typeof(FTZFT40), outputBlockDeserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone, nf40).GetType());
			AssertEquals(typeof(FTZFT40_01), outputBlockDeserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone, nf40_01).GetType());

			var nf11 = "11JULIO R. CACERES                        7875685469     09                     ";
			var nf12 = "12JULIO R. CACERES                                       AA                     ";
			AssertEquals(typeof(FTZNF11), outputBlockDeserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone, nf11).GetType());
			AssertEquals(typeof(FTZNF12), outputBlockDeserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone, nf12).GetType());
		}

		public void TestVersionForEntrySummary()
		{
			var se60 = "SE607102310000          Y                                                       ";
			var se60_01 = "SE60Y                                                                           ";
			var inputBlockDeserialiser = new InputMessageBlockDeserialiser();
			AssertEquals(typeof(MessageBuildingBlocks.ACE.Input.Abstract.ASESE60), inputBlockDeserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.CargoRelease, se60).GetType().BaseType);
			AssertEquals(typeof(MessageBuildingBlocks.ACE.Input.ASESE60_01), inputBlockDeserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.EntrySummary, se60_01).GetType());
		}

		public void TestVersionForEntrySummaryQueryResponse()
		{
			var jc = "JC21091317 0       0      0            0 NO                                     ";
			var jc01 = "JC21091317 0       0                   0 NO                           43475165  ";

			var outputBlockDeserialiser = new OutputMessageBlockDeserialiser();
			AssertEquals(typeof(AENQJC), outputBlockDeserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse, jc).GetType());
			AssertEquals(typeof(AENQJC_01), outputBlockDeserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.EntrySummaryQueryResponse, jc01).GetType());
		}

		public void TestVersionForNewEntrySummaryQueryResponse()
		{
			var ji = "JI891N8NN990900083                                                              ";
			var ji_01 = "JI 891 NO BOND ON FILE IN ACE EBOND                                             ";

			var jk = "JK123456654320516231 1         12         15         13         14              ";
			var jk_01 = "JK BILLING DATA NOT ON FILE                                                     ";

			var jl = "JL051623         12                                                             ";
			var jl_01 = "JL COLLECTION DATA NOT ON FILE                                                  ";

			var jn = "JNSV9Y05162312345678912051623110         12         13         14         15    ";
			var jn_01 = "JN 037 Y BILLING DATA NOT ON FILE                                               ";

			var outputBlockDeserialiser = new OutputMessageBlockDeserialiser();
			AssertEquals(typeof(AENQJI), outputBlockDeserialiser.GetDeserialisedBlock(new string[] { MessageBlockDictionary.ACEApplicationCode }, ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQueryResponse, ji).GetType());
			AssertEquals(typeof(AENQJI_01), outputBlockDeserialiser.GetDeserialisedBlock(new string[] { MessageBlockDictionary.ACEApplicationCode }, ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQueryResponse, ji_01).GetType());
			AssertEquals(typeof(AENQJK), outputBlockDeserialiser.GetDeserialisedBlock(new string[] { MessageBlockDictionary.ACEApplicationCode }, ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQueryResponse, jk).GetType());
			AssertEquals(typeof(AENQJK_01), outputBlockDeserialiser.GetDeserialisedBlock(new string[] { MessageBlockDictionary.ACEApplicationCode }, ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQueryResponse, jk_01).GetType());
			AssertEquals(typeof(AENQJL), outputBlockDeserialiser.GetDeserialisedBlock(new string[] { MessageBlockDictionary.ACEApplicationCode }, ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQueryResponse, jl).GetType());
			AssertEquals(typeof(AENQJL_01), outputBlockDeserialiser.GetDeserialisedBlock(new string[] { MessageBlockDictionary.ACEApplicationCode }, ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQueryResponse, jl_01).GetType());
			AssertEquals(typeof(AENQJN), outputBlockDeserialiser.GetDeserialisedBlock(new string[] { MessageBlockDictionary.ACEApplicationCode }, ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQueryResponse, jn).GetType());
			AssertEquals(typeof(AENQJN_01), outputBlockDeserialiser.GetDeserialisedBlock(new string[] { MessageBlockDictionary.ACEApplicationCode }, ACEApplicationIdentifierCodeList.Codes.NewEntrySummaryQueryResponse, jn_01).GetType());
		}

		public void TEstVersionForFTZNF10()
		{
			var nf10 = "10A0610010A122SAS05184Y4909NB3M         66-053115000LBY066-094189600            ";
			var outputBlockDeserialiser = new OutputMessageBlockDeserialiser();
			AssertEquals(typeof(FTZZD10_01), outputBlockDeserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.FTZDownloadofDatatoZoneOperator, nf10).GetType());
		}

		public void TestACESpecificBlocks()
		{
			var deserialiser = new OutputMessageBlockDeserialiser();
			var block = deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ApplicationIdentifierCodeList.DummyForTesting2, "B00                                                        B                    ");
			AssertEquals(typeof(AABIOutputB), block.GetType());

			block = deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.EntrySummary, "PG04 MONOCHLORODIFLUOROMETHANE                          000000000156KG   0002600");
			AssertEquals(typeof(MessageBuildingBlocks.ACE.Input.AEPAPG04), block.GetType());

			block = deserialiser.GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsImport }, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse, "PG04 MONOCHLORODIFLUOROMETHANE                          000000000156KG   0002600");
			AssertEquals(typeof(MessageBuildingBlocks.ACE.Input.AEPAPG04), block.GetType());
		}

		public void TestDeserialiseBadBlock()
		{
			ErrorReporter.Clear();
			var data = "XX".PadRight(80);
			var messageBlock = new InputMessageBlockDeserialiser().GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodeForTesting }, ApplicationIdentifierCodeList.DummyForTesting1, data, true) as UnknownMessageBlock;
			AssertEquals(data, messageBlock.Data);
			AssertEquals("Cannot deserialise 'XX'" + System.Environment.NewLine + "Application Identifier : " + ApplicationIdentifierCodeList.DummyForTesting1 + " for Application Code(s) : " + CBPEDIInterchange.ApplicationCodeForTesting, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestDeserialiseEmptyBlock()
		{
			ErrorReporter.Clear();
			var data = "".PadRight(80);
			var messageBlock = new InputMessageBlockDeserialiser().GetDeserialisedBlock(new string[] { CBPEDIInterchange.ApplicationCodeForTesting }, ApplicationIdentifierCodeList.DummyForTesting1, data, true) as UnknownMessageBlock;
			AssertEquals(data, messageBlock.Data);
			AssertEquals("Cannot deserialise ''" + System.Environment.NewLine + "Application Identifier : " + ApplicationIdentifierCodeList.DummyForTesting1 + " for Application Code(s) : " + CBPEDIInterchange.ApplicationCodeForTesting, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
	}
}
