using CargoWise.Customs.PL.MessageContracts;
using CargoWise.Customs.PL.MessageContracts.DataProviders;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class MessageDataProvidersTest : TestCase
{
	const string TestResponseFile = "Enterprise.Customs.PL.Business.Testing.Message.InterchangeUnpacking.DocumentHandlingPort.TestFiles.CC529C.xml";

	public void TestNewOrNull() => CombineAssertions(() =>
	{
		var xml = ResourceTestHelper.GetTestFile(typeof(MessageDataProvidersTest).Assembly, TestResponseFile);
		AssertNotNull("CC525C", dataProviderFactory.NewOrNull(xml) as ICC529C);
		AssertNull("Not a message", dataProviderFactory.NewOrNull("<Test/>"));
	});

	public void TestNewOrNull_Reader() => CombineAssertions(() =>
	{
		var xml = ResourceTestHelper.GetTestFile(typeof(MessageDataProvidersTest).Assembly, TestResponseFile);
		var xmlReader = XmlHelper.CreateReaderAndGotoRootNode(xml);
		AssertNotNull("CC525C", dataProviderFactory.NewOrNull(xmlReader) as ICC529C);

		var xmlReader2 = XmlHelper.CreateReaderAndGotoRootNode("<Test/>");
		AssertNull("Not a message", dataProviderFactory.NewOrNull(xmlReader2));
	});

	public void TestNewOrNull_Generic() => CombineAssertions(() =>
	{
		var xml = ResourceTestHelper.GetTestFile(typeof(MessageDataProvidersTest).Assembly, TestResponseFile);
		AssertNotNull("CC525C", dataProviderFactory.NewOrNull<ICC529C>(xml));
		AssertNull("Not a message", dataProviderFactory.NewOrNull<ICC529C>("<Test/>"));
	});

	public void TestNewOrNull_ReaderGeneric() => CombineAssertions(() =>
	{
		var xml = ResourceTestHelper.GetTestFile(typeof(MessageDataProvidersTest).Assembly, TestResponseFile);
		var xmlReader = XmlHelper.CreateReaderAndGotoRootNode(xml);
		AssertNotNull("CC525C", dataProviderFactory.NewOrNull<ICC529C>(xmlReader));

		var xmlReader2 = XmlHelper.CreateReaderAndGotoRootNode("<Test/>");
		AssertNull("Not a message", dataProviderFactory.NewOrNull<ICC529C>(xmlReader2));
	});

	protected override void SetUp()
	{
		base.SetUp();

		dataProviderFactory = new DataProviderFactory(RecognizableMessages.All);
	}

	DataProviderFactory dataProviderFactory;
}
