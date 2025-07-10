using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;

namespace Enterprise.Services.ServiceHost.Tests.HttpXmlServices
{
	public class eAdaptorExternalReferenceNumberHelperTest : TestCaseWithFactory
	{
		public void TestGetExternalReferenceNumberHasExternalNumber()
		{
			var xmlContent = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
    <Event>
        <EventType>Z01</EventType>
        <MessageNumberCollection>
            <MessageNumber Type='External'>TestMessageNumber</MessageNumber>
        </MessageNumberCollection>
    </Event>
</UniversalEvent>";
			var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(xmlContent));
			var expectedNumber = eAdaptorExternalReferenceNumberHelper.GetExternalReferenceNumber(stream);

			AssertEquals("TestMessageNumber", expectedNumber);
		}

		public void TestGetExternalReferenceNumberNotHasExternalNumber()
		{
			var xmlContent1 = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
    <Event>
        <EventType>Z01</EventType>
        <MessageNumberCollection>
            <MessageNumber Type='Test'>TestMessageNumber</MessageNumber>
        </MessageNumberCollection>
    </Event>
</UniversalEvent>";
			var xmlContent2 = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
    <Event>
        <EventType>Z01</EventType>
    </Event>
</UniversalEvent>";
			var stream1 = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(xmlContent1));
			var stream2 = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(xmlContent2));
			var expectedNumber1 = eAdaptorExternalReferenceNumberHelper.GetExternalReferenceNumber(stream1);
			var expectedNumber2 = eAdaptorExternalReferenceNumberHelper.GetExternalReferenceNumber(stream2);

			AssertNull("Can not find externalMessageNumber", expectedNumber1);
			AssertNull("Can not find externalMessageNumber", expectedNumber2);
		}

		public void TestGetExternalReferenceNumberRequestHasNumberOutsideMessageNumberCollection()
		{
			var xmlContent = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
    <Event>
        <EventType>Z01</EventType>
        <MessageNumberCollection>
            <MessageNumber Type='Test'>0001</MessageNumber>
        </MessageNumberCollection>
		<MessageNumber Type='External'>TestMessageNumber</MessageNumber>
    </Event>
</UniversalEvent>";
			var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(xmlContent));
			var expectedNumber = eAdaptorExternalReferenceNumberHelper.GetExternalReferenceNumber(stream);

			AssertNull("Can not find externalMessageNumber", expectedNumber);
		}

		public void TestGetExternalReferenceNumberInvalidXml()
		{
			var xmlContent = @"xxx
        <MessageNumberCollection>
            <MessageNumber Type='Test'>0001</MessageNumber>
        </MessageNumberCollection>";
			var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(xmlContent));
			var expectedNumber = eAdaptorExternalReferenceNumberHelper.GetExternalReferenceNumber(stream);

			AssertNull("Can not find externalMessageNumber", expectedNumber);
		}
	}
}
