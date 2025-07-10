using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.MessageProcessors;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWIncomingMessageKeyInfomation))]
	sealed class TWIncomingMessageKeyInfomationTest : TWXmlTestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestReadFromXML()
		{
			var expectedMessageTypes = new Dictionary<string, string>();
			expectedMessageTypes["NX5106"] = "ARM";
			expectedMessageTypes["N5204"] = "ERM";
			expectedMessageTypes["N5109"] = "IEM";
			expectedMessageTypes["N5116"] = "IRM";
			expectedMessageTypes["N5107"] = "RFM";
			expectedMessageTypes["N5168"] = "UHC";
			expectedMessageTypes["N5110"] = "TPC";
			expectedMessageTypes["N5111"] = "TAD";

			var releaseDateTime = new ZDateTime(2011, 05, 23, 9, 30, 50);
			var dueDateTime = new ZDateTime(2019, 12, 13);
			CombineAssertions(() =>
			{
				foreach (var expectedMessageType in expectedMessageTypes)
				{
					var messageText = GetMessageText(expectedMessageType.Key + ".xml", "111111", "1", "C3M", "BT", "XX", "2011-05-23T09:30:50", false, "2019-12-13");
					var infomation = new TWIncomingMessageKeyInfomation(expectedMessageType.Value, messageText);
					NUnit.Framework.Assert.That(infomation.ErrorText.ToString(), NUnit.Framework.Is.Null.Or.Empty, "expectedMessageType.Key +  can read From XML - should be [null] or [empty]");
					if (!infomation.DeclarationID.IsEmpty)
					{
						NUnit.Framework.Assert.That(infomation.DeclarationID, NUnit.Framework.Is.EqualTo("111111").Using(CustomComparers.TypeComparison), "DeclarationID in " + expectedMessageType.Key);
					}
					if (!infomation.ReleaseDateTime.IsEmpty)
					{
						NUnit.Framework.Assert.That(infomation.ReleaseDateTime, NUnit.Framework.Is.EqualTo(releaseDateTime), "ReleaseDateTime in " + expectedMessageType.Key);
					}
					if (!infomation.DutyDueDateForTPC.IsEmpty)
					{
						NUnit.Framework.Assert.That(infomation.DutyDueDateForTPC, NUnit.Framework.Is.EqualTo(dueDateTime), "DueDateTime in " + expectedMessageType.Key);
					}
					if (!infomation.GovernmentProcedure.IsEmpty)
					{
						NUnit.Framework.Assert.That(infomation.GovernmentProcedure, NUnit.Framework.Is.EqualTo("1").Using(CustomComparers.TypeComparison), "GovernmentProcedure in " + expectedMessageType.Key);
					}
					if (!infomation.StatusNameCode.IsEmpty)
					{
						NUnit.Framework.Assert.That(infomation.StatusNameCode, NUnit.Framework.Is.EqualTo("C3M").Using(CustomComparers.TypeComparison), "StatusNameCode in " + expectedMessageType.Key);
					}
					if (!infomation.BorderTransportMeans.IsEmpty)
					{
						NUnit.Framework.Assert.That(infomation.BorderTransportMeans, NUnit.Framework.Is.EqualTo("BT").Using(CustomComparers.TypeComparison), "BorderTranspotMeans in " + expectedMessageType.Key);
					}
					if (infomation.GoodsShipmentValidationCode.Any())
					{
						NUnit.Framework.Assert.That(infomation.GoodsShipmentValidationCode, NUnit.Framework.Has.Some.EqualTo("XX").Using(CustomComparers.TypeComparison), "ShipmentValidationCode in " + expectedMessageType.Key);
					}
					if (infomation.GoodsShipmentNameCode.Any())
					{
						NUnit.Framework.Assert.That(infomation.GoodsShipmentNameCode, NUnit.Framework.Has.Some.EqualTo("C3M").Using(CustomComparers.TypeComparison), "GoodsShipmentNameCode in " + expectedMessageType.Key);
					}
					if (infomation.StatementCode.Any())
					{
						NUnit.Framework.Assert.That(infomation.StatementCode, NUnit.Framework.Has.Some.EqualTo("3").Using(CustomComparers.TypeComparison), "StatementCode in " + expectedMessageType.Key);
					}
				}
			});
		}

		[ExpectNoExceptions]
		public void TestTWCustomsDeliveryNotification()
		{
			var info = new TWIncomingMessageKeyInfomation("ECD", GetTWNotification("SNT", "IMP", "CA 0958000060", "001"));
			AssertTWCustomsDeliveryNotification("SNT", "IMP", "CA 0958000060", "001");

			info = new TWIncomingMessageKeyInfomation("ICD", GetTWNotification("FAL", "IMP", "CA 0958000061", "002"));
			AssertTWCustomsDeliveryNotification("FAL", "IMP", "CA 0958000061", "002");

			info = new TWIncomingMessageKeyInfomation("ADM", GetTWNotification("SNT", "EXP", "CA 0958000062", "001"));
			AssertTWCustomsDeliveryNotification("SNT", "EXP", "CA 0958000062", "001");

			info = new TWIncomingMessageKeyInfomation("IEA", GetTWNotification("FAL", "EXP", "CA 0958000063", "001"));
			AssertTWCustomsDeliveryNotification("FAL", "EXP", "CA 0958000063", "001");

			info = new TWIncomingMessageKeyInfomation("FCF", GetTWNotification("SNT", "FHM", "MA 1234567", "NUM1"));
			AssertTWCustomsDeliveryNotification("SNT", "FHM", "MA 1234567", "NUM1");

			info = new TWIncomingMessageKeyInfomation("UU", GetTWNotification("SNT", "IMP", "CA 0958000060", "001"));
			AssertTWCustomsDeliveryNotification("", "", "", "");
			NUnit.Framework.Assert.That(info.ErrorText.IsEmpty, NUnit.Framework.Is.True);

			info = new TWIncomingMessageKeyInfomation("ADM", "XXX");
			AssertTWCustomsDeliveryNotification("", "", "", "");
			NUnit.Framework.Assert.That(!info.ErrorText.IsEmpty, NUnit.Framework.Is.True);

			info = new TWIncomingMessageKeyInfomation("ADM", "<A></A>");
			AssertTWCustomsDeliveryNotification("", "", "", "");
			NUnit.Framework.Assert.That(info.ErrorText.IsEmpty, NUnit.Framework.Is.True);

			info = new TWIncomingMessageKeyInfomation("TRA", GetTWNotification("ERR", "TRS", "AWDT1112300004", "00000000001004230159"));
			AssertTWCustomsDeliveryNotification("ERR", "TRS", "AWDT1112300004", "00000000001004230159");

			info = new TWIncomingMessageKeyInfomation("FHM", GetTWNotification("ERR", "FHM", "11233527SW1812100001", "NUM1"));
			AssertTWCustomsDeliveryNotification("ERR", "FHM", "11233527SW1812100001", "NUM1");

			void AssertTWCustomsDeliveryNotification(string expectedEventType, string expectedEntryType, string expectedDeclarationID, string expectedInterchangeNumber)
			{
				CombineAssertions(() =>
				{
					NUnit.Framework.Assert.That(info.EventType, NUnit.Framework.Is.EqualTo(expectedEventType).Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(info.EntryType, NUnit.Framework.Is.EqualTo(expectedEntryType).Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(info.DeclarationID, NUnit.Framework.Is.EqualTo(expectedDeclarationID).Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(info.InterchangeNumber, NUnit.Framework.Is.EqualTo(expectedInterchangeNumber).Using(CustomComparers.TypeComparison));
				});
			}
		}

		[ExpectNoExceptions]
		public void TestTWControllingDeliveryNotification()
		{
			var messageTyeps = new ZString[] { "101", "201", "207", "301", "31A", "31D", "401", "601", "603" };
			var entryNumbers = new ZString[] { "21233527SW1812100001", "21233527SW1812100002", "21233527SW1812100003", "21233527SW1812100004", "21233527SW1812100005", "21233527SW1812100006", "21233527SW1812100007", "21233527SW1812100008", "21233527SW1812100009" };
			var expectedFunctionalReferenceIDs = new ZString[] { "21233527SW1812100001", "21233527SW1812100002", "21233527SW1812100003", "21233527SW1812100004", "21233527SW1812100005", "21233527SW1812100006", "21233527SW1812100007", "21233527SW1812100008", "21233527SW1812100009" };
			for (var i = 0; i < messageTyeps.Length; i++)
			{
				var messageType = messageTyeps[i];
				var interchangeNumber = $"00{i + 1}";
				var info = new TWIncomingMessageKeyInfomation(messageType, GetTWNotification("ERR", "NXM", entryNumbers[i], interchangeNumber, messageType));
				AssertTWControllingDeliveryNotification(info, expectedFunctionalReferenceIDs[i], interchangeNumber);
			}

			void AssertTWControllingDeliveryNotification(TWIncomingMessageKeyInfomation info, string expectedFunctionalReferenceID, string expectedInterchangeNumber)
			{
				CombineAssertions(() =>
				{
					NUnit.Framework.Assert.That(info.EventType, NUnit.Framework.Is.EqualTo("ERR").Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(info.EntryType, NUnit.Framework.Is.EqualTo("NXM").Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(info.FunctionalReferenceID, NUnit.Framework.Is.EqualTo(expectedFunctionalReferenceID).Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(info.InterchangeNumber, NUnit.Framework.Is.EqualTo(expectedInterchangeNumber).Using(CustomComparers.TypeComparison));
				});
			}
		}

		string TestMessageTextForN5108 => GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.N5108.N5108.xml");

		[ExpectNoExceptions]
		public void TestReadFromXMLForNX302_DN()
		{
			var xml = GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.NX302_DN.xml");
			var info = new TWIncomingMessageKeyInfomation(MessageTypeList.Codes._32D, xml);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(info.Result, NUnit.Framework.Is.TypeOf<CargoWise.Customs.TW.MessageDefinitions.NX302_DN.Response>());
				NUnit.Framework.Assert.That(info.FunctionalReferenceID, NUnit.Framework.Is.EqualTo("23322709001106110009").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(info.DeclarationID, NUnit.Framework.Is.EqualTo("CA  1245600072").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestReadFromXMLForN5108()
		{
			var info = new TWIncomingMessageKeyInfomation(MessageTypeList.Codes.FHR, TestMessageTextForN5108);
			NUnit.Framework.Assert.That(info.Result, NUnit.Framework.Is.TypeOf<CargoWise.Customs.TW.MessageDefinitions.N5108.Response>());
		}

		[ExpectNoExceptions]
		public void TestReadFromXMLForNX602()
		{
			var xml = GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.NX602.xml");
			var info = new TWIncomingMessageKeyInfomation(MessageTypeList.Codes._602, xml);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(info.Result, NUnit.Framework.Is.TypeOf<CargoWise.Customs.TW.MessageDefinitions.NX602.Response>());
				NUnit.Framework.Assert.That(info.FunctionalReferenceID, NUnit.Framework.Is.EqualTo("23322709001106120010").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(info.DeclarationID, NUnit.Framework.Is.EqualTo("CA  1245600072").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestFunctionalReferenceID()
		{
			var info = new TWIncomingMessageKeyInfomation(MessageTypeList.Codes.FHR, TestMessageTextForN5108);
			NUnit.Framework.Assert.That(info.FunctionalReferenceID, NUnit.Framework.Is.EqualTo("11233527SW1812100001").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestReadFromXMLForNX402()
		{
			var xml = GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.NX402.xml");
			var info = new TWIncomingMessageKeyInfomation(MessageTypeList.Codes._402, xml);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(info.Result, NUnit.Framework.Is.TypeOf<CargoWise.Customs.TW.MessageDefinitions.NX402.Response>());
				NUnit.Framework.Assert.That(info.FunctionalReferenceID, NUnit.Framework.Is.EqualTo("23322709001106120001").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(info.DeclarationID, NUnit.Framework.Is.EqualTo("CA  1245600072").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestReadFromXMLForNX901()
		{
			var xml = GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.NX901.xml");
			var info = new TWIncomingMessageKeyInfomation(MessageTypeList.Codes._901, xml);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(info.Result, NUnit.Framework.Is.TypeOf<CargoWise.Customs.TW.MessageDefinitions.NX901.Response>());
				NUnit.Framework.Assert.That(info.FunctionalReferenceID, NUnit.Framework.Is.EqualTo("96944490002307040002").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(info.DeclarationID, NUnit.Framework.Is.EqualTo("CA  1245600093").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestReadFromXMLForNX903()
		{
			var xml = GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.NX903.xml");
			var info = new TWIncomingMessageKeyInfomation(MessageTypeList.Codes._903, xml);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(info.Result, NUnit.Framework.Is.TypeOf<CargoWise.Customs.TW.MessageDefinitions.NX903.Response>());
				NUnit.Framework.Assert.That(info.FunctionalReferenceID, NUnit.Framework.Is.EqualTo("52889317002307280003").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(info.DeclarationID, NUnit.Framework.Is.EqualTo("CA  1248600825").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestReadFromXMLForNX202()
		{
			var xml = GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.NX202.xml");
			var info = new TWIncomingMessageKeyInfomation(MessageTypeList.Codes._202, xml);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(info.Result, NUnit.Framework.Is.TypeOf<CargoWise.Customs.TW.MessageDefinitions.NX202.Response>());
				NUnit.Framework.Assert.That(info.FunctionalReferenceID, NUnit.Framework.Is.EqualTo("23322708001106180001").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestReadFromXMLForNX102()
		{
			var xml = GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.NX102.xml");
			var info = new TWIncomingMessageKeyInfomation(MessageTypeList.Codes._102, xml);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(info.Result, NUnit.Framework.Is.TypeOf<CargoWise.Customs.TW.MessageDefinitions.NX102.Response>());
				NUnit.Framework.Assert.That(info.FunctionalReferenceID, NUnit.Framework.Is.EqualTo("22099131002309220001").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(info.DeclarationID, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestReadFromXMLForNX902()
		{
			var xml = GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.NX902.xml");
			var info = new TWIncomingMessageKeyInfomation(MessageTypeList.Codes._902, xml);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(info.Result, NUnit.Framework.Is.TypeOf<CargoWise.Customs.TW.MessageDefinitions.NX902.Response>());
				NUnit.Framework.Assert.That(info.FunctionalReferenceID, NUnit.Framework.Is.EqualTo("52889317002307280001").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(info.DeclarationID, NUnit.Framework.Is.EqualTo("CA  1248600825").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestReadFromXMLForNX302()
		{
			var xml = GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.NX302.xml");
			var info = new TWIncomingMessageKeyInfomation(MessageTypeList.Codes._302, xml);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(info.Result, NUnit.Framework.Is.TypeOf<CargoWise.Customs.TW.MessageDefinitions.NX302.Response>());
				NUnit.Framework.Assert.That(info.FunctionalReferenceID, NUnit.Framework.Is.EqualTo("23322708001106110001").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(info.DeclarationID, NUnit.Framework.Is.EqualTo("CA  1245600072").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestReadFromXMLForNX302_AX()
		{
			var xml = GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.NX302_AX.xml");
			var info = new TWIncomingMessageKeyInfomation(MessageTypeList.Codes._32A, xml);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(info.Result, NUnit.Framework.Is.TypeOf<CargoWise.Customs.TW.MessageDefinitions.NX302_AX.Response>());
				NUnit.Framework.Assert.That(info.FunctionalReferenceID, NUnit.Framework.Is.EqualTo("23322708001106110002").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(info.DeclarationID, NUnit.Framework.Is.EqualTo("AA 0710823476").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestEntryStatus()
		{
			AssertEntryStatus(MessageTypeCodeList.Codes.ARM, "ARM");
			AssertEntryStatus(MessageTypeCodeList.Codes.ERM, "ERM");
			AssertEntryStatus(MessageTypeCodeList.Codes.IRM, "IRM");
			AssertEntryStatus(MessageTypeCodeList.Codes.RFM, "RFM");
			AssertEntryStatus(MessageTypeCodeList.Codes.IEM, "IEM");
			AssertEntryStatus(MessageTypeCodeList.Codes.UHC, "UHC");
			AssertEntryStatus(MessageTypeCodeList.Codes.TAD, "TAD");
			AssertEntryStatus(MessageTypeCodeList.Codes.TPC, "TPC");
			AssertEntryStatus(MessageTypeCodeList.Codes.FHR, "");
		}

		[ExpectNoExceptions]
		void AssertEntryStatus(ZString messageType, ZString expectedEntryStatus)
		{
			var info = new TWIncomingMessageKeyInfomation(messageType, "");
			NUnit.Framework.Assert.That(info.EntryStatus, NUnit.Framework.Is.EqualTo(expectedEntryStatus));
		}

		[ExpectNoExceptions]
		public void TestClearanceStatus()
		{
			AssertClearanceStatus(MessageTypeCodeList.Codes.ERM, "C1");
			AssertClearanceStatus(MessageTypeCodeList.Codes.IRM, "C1");
			AssertClearanceStatus(MessageTypeCodeList.Codes.ARM, "");
			AssertClearanceStatus(MessageTypeCodeList.Codes.RFM, "");
		}

		[ExpectNoExceptions]
		void AssertClearanceStatus(ZString messageType, ZString expectedClearanceStatus)
		{
			var info = new TWIncomingMessageKeyInfomation(messageType, "");
			info.StatusNameCode = "C1";
			NUnit.Framework.Assert.That(info.ClearanceStatus, NUnit.Framework.Is.EqualTo(expectedClearanceStatus));
		}
	}
}
