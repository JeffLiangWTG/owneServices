using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX201_07MessageSendingObject))]
	sealed class NX201_07MessageSendingObjectTest : LicensingMessageSendingObjectTest<NX201_07MessageSendingObject>
	{
		protected override NX201_07MessageSendingObject GetMessageSendingObject(CusTWControllingMessageHeader header) => new NX201_07MessageSendingObject(header);

		[ExpectNoExceptions]
		public void TestINX201_07_AdditionalDocument()
		{
			(var messageSendingObject, var header, _) = SetupData();
			header.PermitNumber = "111";
			header.PermitNoExpirationDate = new ZDateTime(2023, 11, 30);
			var iNX201_07Instance = (INX201_07)messageSendingObject;
			var additionalDocument = iNX201_07Instance.AdditionalDocument;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(additionalDocument.ID, NUnit.Framework.Is.EqualTo("111").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(additionalDocument.LPCOExpirationDateTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2023, 11, 30)));
			});
		}

		[ExpectNoExceptions]
		protected override void TestApplication()
		{
			(var messageSendingObject, _, _) = SetupData();
			var iNX201_07Instance = (INX201_07)messageSendingObject;
			NUnit.Framework.Assert.That(iNX201_07Instance.Application, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IApplication)));
		}

		[ExpectNoExceptions]
		protected override void TestReasonDescription_Readonly()
		{
			(var messageSendingObject, _, _) = SetupData();
			var actionForReasonDescriptionList = new ZString[] { NX201_07ActionCodeList.Codes._1, NX201_07ActionCodeList.Codes._50 };
			foreach (var action in actionForReasonDescriptionList)
			{
				messageSendingObject.Action = action;
				NUnit.Framework.Assert.That(!messageSendingObject.ReasonDescriptionInfo.ReadOnly, NUnit.Framework.Is.True, $"Action is '{action}'");
			}
			messageSendingObject.Action = NX201_07ActionCodeList.Codes._5;
			NUnit.Framework.Assert.That(messageSendingObject.ReasonDescriptionInfo.ReadOnly, NUnit.Framework.Is.True, "Action is '5'");
		}

		[ExpectNoExceptions]
		public void TestIsReasonDescriptionRequired()
		{
			(var messageSendingObject, _, _) = SetupData();
			var actionForReasonDescriptionList = new ZString[] { NX201_07ActionCodeList.Codes._1, NX201_07ActionCodeList.Codes._50 };
			foreach (var action in actionForReasonDescriptionList)
			{
				messageSendingObject.Action = action;
				NUnit.Framework.Assert.That(messageSendingObject.IsReasonDescriptionRequired, NUnit.Framework.Is.True, $"Action is '{action}'");
			}
			messageSendingObject.Action = NX201_07ActionCodeList.Codes._5;
			NUnit.Framework.Assert.That(!messageSendingObject.IsReasonDescriptionRequired, NUnit.Framework.Is.True, "Action is '5'");
		}

		[ExpectNoExceptions]
		public void TestGetTypeCode()
		{
			(var messageSendingObject, var header, _) = SetupData();
			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX201_07;
			header.TW1_BusinessType = "2";
			var sendingObj = new NX201_01MessageSendingObject(header);
			NUnit.Framework.Assert.That(sendingObj.TypeList.CodesAsString, NUnit.Framework.Is.EqualTo("7, 8, 11, 22, 25, 26, 27, 28, 31, 45, 46, 47, 48, 49"), "TW1_BusinessType is 2");

			header.TW1_BusinessType = "0";
			sendingObj = new NX201_01MessageSendingObject(header);
			NUnit.Framework.Assert.That(sendingObj.TypeList.CodesAsString, NUnit.Framework.Is.EqualTo("99"), "TW1_BusinessType is 0");

			header.TW1_BusinessType = "3";
			sendingObj = new NX201_01MessageSendingObject(header);
			NUnit.Framework.Assert.That(sendingObj.TypeList.CodesAsString, NUnit.Framework.Is.EqualTo("99"), "TW1_BusinessType is 3");
		}

		[ExpectNoExceptions]
		public void TestDefaultValues()
		{
			(var messageSendingObject, var header, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.Action, NUnit.Framework.Is.EqualTo(NX201_07ActionCodeList.Codes._5).Using(CustomComparers.TypeComparison), "Action");
		}

		[ExpectNoExceptions]
		public override void TestAdditionalInformation()
		{
			(var messageSendingObject, _, _) = SetupData();
			messageSendingObject.ReasonDescription = "reason desc";
			messageSendingObject.Action = NX201_07ActionCodeList.Codes._50;
			NUnit.Framework.Assert.That(messageSendingObject.AdditionalInformation.Content, NUnit.Framework.Is.EqualTo("reason desc").Using(CustomComparers.TypeComparison), "Action: 50");

			messageSendingObject.Action = NX201_07ActionCodeList.Codes._5;
			NUnit.Framework.Assert.That(messageSendingObject.AdditionalInformation, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IAdditionalInformation)), "Should be null - should be [null]");

			messageSendingObject.Action = NX201_07ActionCodeList.Codes._1;
			NUnit.Framework.Assert.That(messageSendingObject.AdditionalInformation.Content, NUnit.Framework.Is.EqualTo("reason desc").Using(CustomComparers.TypeComparison), "Action: 1");
		}

		protected override string ExpectedEM_MessageType => "207";
	}
}
