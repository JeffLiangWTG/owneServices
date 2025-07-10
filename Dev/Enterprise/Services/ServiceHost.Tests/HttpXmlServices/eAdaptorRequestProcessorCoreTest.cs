using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.ServiceHost.Tests
{
	class eAdaptorRequestProcessorCoreTest : TestCaseWithFactory
	{
		public void TestProcessRequestManagesDbConnectionsCorrectlyInCaseOfExceptions()
		{
			IeAdaptorRequestProcessorResult RequestProcessor(SubStreamableStream incomingStream, IeAdaptorConfig eAdaptorConfig, string handlerName)
			{
				var factory = new BusinessObjectFactory();
				var row = factory.LoadTop1<GlbStaff>(new ZQuery());
				throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception("Dummy"), ((IBusinessObjectInternals)row).Row, Db.Connection), factory);
			}

			var thread = new Thread(() =>
			{
				using (var stream = new MemoryStream())
				using (var request = new HttpRequestMessage())
				{
					request.Content = new StreamContent(stream);
					eAdaptorRequestProcessorCore.ProcessRequestWithExceptionHandling(request, RequestProcessor, eAdaptorConfig.Instance, "");
				}
			});

			thread.Start();
			thread.Join();

			AssertEquals("No errors should have been reported.", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestPostRequest_SetsContentTypeAndCharset_ToExpectedValues()
		{
			var requestContent = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <Organization>
      <CriteriaGroup Type=""Key"">
        <Criteria FieldName=""Field"">
          AUSDISMEL
        </Criteria>
      </CriteriaGroup>
    </Organization>
  </Body>
</Native>";
			using (var adaptor = new eAdaptorController { Request = new HttpRequestMessage { Content = new StringContent(requestContent) } })
			using (var response = adaptor.Post())
			{
				AssertEquals("text/xml", response.Content.Headers.ContentType.MediaType);
				AssertEquals("utf-8", response.Content.Headers.ContentType.CharSet);
				response.Content.Dispose();
			}
		}
	}
}
