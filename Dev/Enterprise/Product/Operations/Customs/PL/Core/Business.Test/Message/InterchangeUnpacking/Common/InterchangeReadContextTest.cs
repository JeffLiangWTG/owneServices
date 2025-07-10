using System.IO;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class InterchangeReadContextTest : TestCaseWithFactory
{
	public void TestDataStream() => CombineAssertions(() =>
	{
		Stream dataStream;
		AssertNotNull("NotNull", dataStream = readContext.DataStream);
		AssertSame("Same", dataStream, readContext.DataStream);
		Assert("Not empty", dataStream.Length > 0);
	});

	public void TestTextReader() => CombineAssertions(() =>
	{
		TextReader textReader;
		AssertNotNull("NotNull", textReader = readContext.TextReader);
		AssertSame("Same", textReader, readContext.TextReader);
		AssertNotEquals("Not empty", string.Empty, textReader.ReadToEnd());
	});

	public void TestPayloadXmlReader() => CombineAssertions(() =>
	{
		XmlReader xmlReader;
		AssertNotNull("NotNull", xmlReader = readContext.PayloadXmlReader);
		AssertSame("Same", xmlReader, readContext.PayloadXmlReader);
		AssertEquals("Root node", "TestNode", xmlReader.Name);
	});

	protected override void SetUp()
	{
		base.SetUp();

		interchange = Factory.New<EDIInterchange>();
		interchange.EI_BodyText = "<TestNode/>";

		readContext = interchange.GetReadContext();
	}

	protected override void TearDown()
	{
		readContext.Dispose();
		base.TearDown();
	}

	EDIInterchange interchange;
	InterchangeReadContext readContext;
}
