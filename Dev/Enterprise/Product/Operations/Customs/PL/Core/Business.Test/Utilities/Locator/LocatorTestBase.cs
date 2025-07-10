using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Moq;

namespace Enterprise.Customs.PL.Business.Testing;

public abstract class LocatorTestBase<TLocator, TEDIMessage> : TestCaseWithFactory
	where TLocator : LocatorBase, new()
	where TEDIMessage : BaseEDIMessage
{
	public void TestApplicationCode() => AssertEquals(ExpectedApplicationCodes, testLocator.ApplicationCodes);

	public void TestFindBusinessObjectForIncomingMessage() => CombineAssertions(() =>
	{
		const string testMessageNum = "24IE000Num";
		const string testLRN = "TestLRN";
		const string testMRN = "TestMRN";

		const string correlationNumber = testMessageNum;

		const string testMessageNum2 = "24IE000Num2";
		const string testLRN2 = "TestLRN2";
		const string testMRN2 = "TestMRN2";

		var (linkedObject, _) = CreateTestData(testMessageNum, testLRN, testMRN);
		_ = CreateTestData(testMessageNum2, testLRN2, testMRN2);

		SetupDataProviderMock(correlationNum: null, mrn: null, lrn: null);
		AssertNull("Empty provider", testLocator.FindBusinessObjectForIncomingMessage(Factory, dataProviderMock.Object));

		SetupDataProviderMock(correlationNum: correlationNumber, mrn: null, lrn: null);
		AssertSame("Found by Correlation Number", linkedObject, testLocator.FindBusinessObjectForIncomingMessage(Factory, dataProviderMock.Object));

		SetupDataProviderMock(correlationNum: null, mrn: testMRN, lrn: null);
		AssertSame("Found by MRN", linkedObject, testLocator.FindBusinessObjectForIncomingMessage(Factory, dataProviderMock.Object));

		SetupDataProviderMock(correlationNum: null, mrn: null, lrn: testLRN);
		AssertSame("Found by LRN", linkedObject, IgnoreFindByLrn ? linkedObject : testLocator.FindBusinessObjectForIncomingMessage(Factory, dataProviderMock.Object));

		SetupDataProviderMock(correlationNum: testMessageNum2, mrn: testMRN, lrn: testLRN2);
		AssertSame("MRN Number has priority 1", linkedObject, testLocator.FindBusinessObjectForIncomingMessage(Factory, dataProviderMock.Object));

		SetupDataProviderMock(correlationNum: testMessageNum2, mrn: "0", lrn: testLRN);
		AssertSame("LRN has priority 2", linkedObject, IgnoreFindByLrn ? linkedObject : testLocator.FindBusinessObjectForIncomingMessage(Factory, dataProviderMock.Object));

		SetupDataProviderMock(correlationNum: correlationNumber, mrn: "0", lrn: "0");
		AssertSame("Correlation has priority 3", linkedObject, testLocator.FindBusinessObjectForIncomingMessage(Factory, dataProviderMock.Object));
	});

	public void TestFindTransmitMessage() => CombineAssertions(() =>
	{
		const string testMessageNum = "24IE000Num";
		const string testLRN = "TestLRN";
		const string testMRN = "TestMRN";

		const string correlationNumber = testMessageNum;

		const string testMessageNum2 = "24IE000Num2";
		const string testLRN2 = "TestLRN2";
		const string testMRN2 = "TestMRN2";

		var (_, message) = CreateTestData(testMessageNum, testLRN, testMRN);
		_ = CreateTestData(testMessageNum2, testLRN2, testMRN2);

		SetupDataProviderMock(correlationNum: null, mrn: null, lrn: null);
		AssertNull("Empty provider", testLocator.FindTransmitMessage(Factory, dataProviderMock.Object));

		SetupDataProviderMock(correlationNum: correlationNumber, mrn: null, lrn: null);
		AssertSame("Found by Correlation Number", message, testLocator.FindTransmitMessage(Factory, dataProviderMock.Object));

		SetupDataProviderMock(correlationNum: null, mrn: testMRN, lrn: null);
		AssertSame("Found by MRN", message, testLocator.FindTransmitMessage(Factory, dataProviderMock.Object));

		SetupDataProviderMock(correlationNum: null, mrn: null, lrn: testLRN);
		AssertSame("Found by LRN", message, IgnoreFindByLrn ? message : testLocator.FindTransmitMessage(Factory, dataProviderMock.Object));

		SetupDataProviderMock(correlationNum: correlationNumber, mrn: testMRN2, lrn: testLRN2);
		AssertSame("Correlation Number has priority 1", message, testLocator.FindTransmitMessage(Factory, dataProviderMock.Object));

		SetupDataProviderMock(correlationNum: "Wrong num", mrn: testMRN, lrn: testLRN2);
		AssertSame("MRN has priority 2", message, testLocator.FindTransmitMessage(Factory, dataProviderMock.Object));

		SetupDataProviderMock(correlationNum: "0", mrn: "0", lrn: testLRN);
		AssertSame("LRN has priority 3", message, IgnoreFindByLrn ? message : testLocator.FindTransmitMessage(Factory, dataProviderMock.Object));
	});

	protected abstract string ExpectedApplicationCodes { get; }

	protected abstract string MessageApplicationCode { get; }

	protected abstract BusinessObject CreateLinkedObject();

	protected virtual BusinessObject GetMessageLinkedObject(BusinessObject linkedObject) => linkedObject;

	protected virtual bool IgnoreFindByLrn => false;

	protected abstract void AllocateLRN(BusinessObject linkedObject, string lrn);

	protected virtual void AllocateMRN(BusinessObject linkedObject, string mrn)
	{
		var entryNumber = Factory.New<CusEntryNumber>();
		entryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
		entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Poland;
		entryNumber.CE_EntryNum = mrn;
		entryNumber.Parent = linkedObject;
	}

	protected override void SetUp()
	{
		base.SetUp();

		testLocator = new TLocator();
		dataProviderMock = new Mock<IIncomingMessage>();
	}

	TLocator testLocator;
	Mock<IIncomingMessage> dataProviderMock;

	void SetupDataProviderMock(string correlationNum = null, string mrn = null, string lrn = null)
	{
		dataProviderMock.As<ICorrelationProvider>().Setup(x => x.CorrelationIdentifier).Returns(correlationNum);
		var jobIdentificationMock = dataProviderMock.As<IJobIdentification>();
		jobIdentificationMock.Setup(x => x.MRN).Returns(mrn);
		jobIdentificationMock.Setup(x => x.LRN).Returns(lrn);
	}

	(BusinessObject LinkedObject, BaseEDIMessage Message) CreateTestData(string messageNum, string lrn, string mrn)
	{
		var linkedObject = CreateLinkedObject();
		AllocateLRN(linkedObject, lrn);
		AllocateMRN(linkedObject, mrn);
		var message = CreateMessage();
		var messageLinkedObject = GetMessageLinkedObject(linkedObject);
		message.EM_LinkedObject = messageLinkedObject;

		return (messageLinkedObject, message);

		TEDIMessage CreateMessage()
		{
			var result = Factory.New<TEDIMessage>();
			result.EM_ApplicationCode = MessageApplicationCode;
			result.EM_Status = EDIMessage.Status.Sent;
			result.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			result.EM_MessageNum = messageNum;
			return result;
		}
	}
}

