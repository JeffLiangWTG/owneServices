using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX201_01MessageSendingObject))]
	sealed class NX201_01MessageSendingObjectTest : LicensingMessageSendingObjectTest<NX201_01MessageSendingObject>
	{
		[ExpectNoExceptions]
		public void TestGoodsShipmentType()
		{
			(var messageSendingObject, _, var decl) = SetupData();
			decl.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			NUnit.Framework.Assert.That(messageSendingObject.GoodsShipment, NUnit.Framework.Is.TypeOf(typeof(NX201_01ImportLicensingMessageGoodsShipment)));

			decl.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			NUnit.Framework.Assert.That(messageSendingObject.GoodsShipment, NUnit.Framework.Is.TypeOf(typeof(NX201_01ExportLicensingMessageGoodsShipment)));
		}

		[ExpectNoExceptions]
		public override void TestAdditionalInformation()
		{
			(var messageSendingObject, var header, _) = SetupData();
			header.ProcessingNumber = "1234";
			messageSendingObject.ReasonDescription = "reason desc";
			CombineAssertions(() =>
			{
				messageSendingObject.Action = NX201_01ActionCodeList.Codes._17;
				NUnit.Framework.Assert.That(messageSendingObject.AdditionalInformation.DelProcessNumber, NUnit.Framework.Is.EqualTo("1234").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(messageSendingObject.AdditionalInformation.ProcessNumber, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(messageSendingObject.AdditionalInformation.Content, NUnit.Framework.Is.EqualTo("reason desc").Using(CustomComparers.TypeComparison));
			});
			CombineAssertions(() =>
			{
				messageSendingObject.Action = NX201_01ActionCodeList.Codes._4;
				NUnit.Framework.Assert.That(messageSendingObject.AdditionalInformation.DelProcessNumber, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(messageSendingObject.AdditionalInformation.ProcessNumber, NUnit.Framework.Is.EqualTo("1234").Using(CustomComparers.TypeComparison));
			});

			messageSendingObject.Action = NX201_01ActionCodeList.Codes._5;
			NUnit.Framework.Assert.That(messageSendingObject.AdditionalInformation.Content, NUnit.Framework.Is.EqualTo("reason desc").Using(CustomComparers.TypeComparison));

			messageSendingObject.Action = NX201_01ActionCodeList.Codes._52;
			NUnit.Framework.Assert.That(messageSendingObject.AdditionalInformation.Content, NUnit.Framework.Is.EqualTo("reason desc").Using(CustomComparers.TypeComparison));

			messageSendingObject.Action = NX201_01ActionCodeList.Codes._9;
			NUnit.Framework.Assert.That(messageSendingObject.AdditionalInformation, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IAdditionalInformation)));
		}

		[ExpectNoExceptions]
		public void TestAdditionalDeclaration()
		{
			(var messageSendingObject, var header, _) = SetupData();
			header.CustomsMessageIdentifier = "2234";
			messageSendingObject.Action = NX201_01ActionCodeList.Codes._9;
			NUnit.Framework.Assert.That(messageSendingObject.AdditionalDeclarationID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			messageSendingObject.Action = NX201_01ActionCodeList.Codes._52;
			NUnit.Framework.Assert.That(messageSendingObject.AdditionalDeclarationID, NUnit.Framework.Is.EqualTo("2234").Using(CustomComparers.TypeComparison));
		}

		protected override NX201_01MessageSendingObject GetMessageSendingObject(CusTWControllingMessageHeader header)
		{
			return new NX201_01MessageSendingObject(header);
		}

		[ExpectNoExceptions]
		protected override void TestReasonDescription_Readonly()
		{
			(var messageSendingObject, _, _) = SetupData();
			var actionForReasonDescriptionList = new ZString[] { "5", "17", "52" };
			foreach (var action in actionForReasonDescriptionList)
			{
				messageSendingObject.Action = action;
				NUnit.Framework.Assert.That(!messageSendingObject.ReasonDescriptionInfo.ReadOnly, NUnit.Framework.Is.True, $"Action is '{action}'");
			}
			messageSendingObject.Action = "9";
			NUnit.Framework.Assert.That(messageSendingObject.ReasonDescriptionInfo.ReadOnly, NUnit.Framework.Is.True, "Action is '9'");
		}

		[ExpectNoExceptions]
		public void TestActionForReasonDescriptionList()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.ActionForReasonDescriptions, NUnit.Framework.Is.EquivalentTo(new ZString[] { NX201_01ActionCodeList.Codes._5, NX201_01ActionCodeList.Codes._17, NX201_01ActionCodeList.Codes._52 }));
		}

		[ExpectNoExceptions]
		public void TestActionForProcessingNumberList()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.ActionForProcessingNumbers, NUnit.Framework.Is.EquivalentTo(new ZString[] { NX201_01ActionCodeList.Codes._4, NX201_01ActionCodeList.Codes._17 }));
		}

		[ExpectNoExceptions]
		public void TestGetTypeCode()
		{
			(var messageSendingObject, var header, _) = SetupData();
			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX201_01;
			header.TW1_BusinessType = "1";
			var sendingObj = new NX201_01MessageSendingObject(header);
			NUnit.Framework.Assert.That(sendingObj.TypeList.CodesAsString, NUnit.Framework.Is.EqualTo("7, 8, 11, 22, 25, 26, 27, 28, 31, 45, 46, 47, 48, 49"), "TW1_BusinessType is 1");

			header.TW1_BusinessType = "0";
			sendingObj = new NX201_01MessageSendingObject(header);
			NUnit.Framework.Assert.That(sendingObj.TypeList.CodesAsString, NUnit.Framework.Is.EqualTo("99"), "TW1_BusinessType is 0");

			header.TW1_BusinessType = "2";
			sendingObj = new NX201_01MessageSendingObject(header);
			NUnit.Framework.Assert.That(sendingObj.TypeList.CodesAsString, NUnit.Framework.Is.EqualTo("7, 8, 11, 22, 25, 26, 27, 28, 31, 45, 46, 47, 48, 49"), "TW1_BusinessType is 2");

			header.TW1_BusinessType = "3";
			sendingObj = new NX201_01MessageSendingObject(header);
			NUnit.Framework.Assert.That(sendingObj.TypeList.CodesAsString, NUnit.Framework.Is.EqualTo("99"), "TW1_BusinessType is 3");
		}

		[ExpectNoExceptions]
		public void TestDefaultAction()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.Action, NUnit.Framework.Is.EqualTo(NX201_01ActionCodeList.Codes._9).Using(CustomComparers.TypeComparison));
		}

		protected override string ExpectedEM_MessageType => "201";
	}
}
