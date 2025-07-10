using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestsSubclassesOf(typeof(LicensingMessageSendingObject))]
	abstract class LicensingMessageSendingObjectTest<T> : NonPersistentBusinessObjectTestCase
		where T : LicensingMessageSendingObject
	{
		[ExpectNoExceptions]
		protected virtual void TestAcceptanceDateTime()
		{
			(var messageSendingObject, _, var decl) = SetupData();
			decl.CusEntryInstruction.CEI_DateForDuty = ZDateTime.BrettsBirthday;
			NUnit.Framework.Assert.That(messageSendingObject.AcceptanceDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.BrettsBirthday.Date), "AcceptanceDateTime");
		}

		[TestDate(2023, 12, 25)]
		[ExpectNoExceptions]
		public void TestIssueDateTime()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.IssueDateTime, NUnit.Framework.Is.EqualTo(new ZDateTime(2023, 12, 25)));
		}

		[ExpectNoExceptions]
		public virtual void TestAdditionalInformation()
		{
			(var messageSendingObject, _, _) = SetupData();
			messageSendingObject.ReasonDescription = "Reason Desc.";
			NUnit.Framework.Assert.That(messageSendingObject.AdditionalInformation.Content, NUnit.Framework.Is.EqualTo("Reason Desc.").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public virtual void TestFunctionalReferenceID()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.FunctionalReferenceID, NUnit.Framework.Is.EqualTo(MessageConstants.FunctionalReferenceIDPlaceHolder).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		protected virtual void TestFunctionCode()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.FunctionCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "FunctionCode - should be [null] or [empty]");
		}

		[ExpectNoExceptions]
		protected virtual void TestID()
		{
			(var messageSendingObject, _, var declaration) = SetupData();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "CABB0999900001";
			entry.CH_Status = "AWO";
			NUnit.Framework.Assert.That(messageSendingObject.ID, NUnit.Framework.Is.EqualTo("CA/BB/09/999/00001").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		protected virtual void TestTypeCode()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.TypeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty, "TypeCode - should be [null] or [empty]");
		}

		[ExpectNoExceptions]
		protected virtual void TestEM_MessageType()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(string.IsNullOrEmpty(ExpectedEM_MessageType), NUnit.Framework.Is.True, "ExpectedEM_MessageType for tests should not be a empty string");
			NUnit.Framework.Assert.That(messageSendingObject.EM_MessageType.IsEmpty, NUnit.Framework.Is.EqualTo(ExpectedEM_MessageType).Using(CustomComparers.TypeComparison), "EM_MessageType is not expected");
		}

		[ExpectNoExceptions]
		protected virtual void TestAgent()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.Agent, NUnit.Framework.Is.TypeOf(typeof(LicensingMessageAgent)));
		}

		[ExpectNoExceptions]
		protected virtual void TestBorderTransortMeans()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.BorderTransportMeans, NUnit.Framework.Is.TypeOf(typeof(LicensingMessageTransportMeans)));
		}

		[ExpectNoExceptions]
		protected virtual void TestConsignment()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.Consignment, NUnit.Framework.Is.TypeOf(typeof(LicensingMessageConsignment)));
		}

		[ExpectNoExceptions]
		protected virtual void TestCurrencyExchange()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.CurrencyExchange, NUnit.Framework.Is.TypeOf(typeof(LicensingMessageCurrencyExchange)));
		}

		[ExpectNoExceptions]
		protected virtual void TestGoodsShipment()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.GoodsShipment, NUnit.Framework.Is.TypeOf(typeof(LicensingMessageGoodsShipment)));
		}

		[ExpectNoExceptions]
		protected virtual void TestImporter()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.Importer, NUnit.Framework.Is.TypeOf(typeof(LicensingMessageImporter)));
		}

		[ExpectNoExceptions]
		protected virtual void TestApplication()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.Application, NUnit.Framework.Is.TypeOf(typeof(LicensingMessageApplication)));
		}

		[ExpectNoExceptions]
		protected virtual void TestPackaging()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(messageSendingObject.Packaging, NUnit.Framework.Is.TypeOf(typeof(LicensingMessagePackaging)));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			return GetMessageSendingObject(header);
		}

		[ExpectNoExceptions]
		protected virtual void TestReasonDescription()
		{
			(var messageSendingObject, _, _) = SetupData();
			BusinessObjectCaptionTestHelper.CombineAssertCaptions(messageSendingObject.ReasonDescriptionInfo, "Reason Description", "Reason Description");
			NUnit.Framework.Assert.That(messageSendingObject.ReasonDescriptionInfo.MaxLength, NUnit.Framework.Is.EqualTo(240));
		}

		[ExpectNoExceptions]
		protected virtual void TestReasonDescription_Readonly()
		{
			(var messageSendingObject, _, _) = SetupData();
			NUnit.Framework.Assert.That(!messageSendingObject.ReasonDescriptionInfo.ReadOnly, NUnit.Framework.Is.True);
		}

		protected abstract T GetMessageSendingObject(CusTWControllingMessageHeader header);

		protected virtual string ExpectedEM_MessageType { get; } = string.Empty;

		protected (T messageSendingObject, CusTWControllingMessageHeader header, JobDeclaration declaration) SetupData()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			return (GetMessageSendingObject(header), header, declaration);
		}
	}

	public class LicensingMessageSendingObjectForTest : LicensingMessageSendingObject
	{
		public LicensingMessageSendingObjectForTest(CusTWControllingMessageHeader header) : base(header)
		{
		}

		protected override ZString GetEM_MessageTypeCore()
		{
			return ZString.Empty;
		}

		protected override ITWMessageBuilder GetMessageBuilder()
		{
			return null;
		}
	}
}
