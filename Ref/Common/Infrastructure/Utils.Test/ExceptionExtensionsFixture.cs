using System;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Utils.Test
{
	[TestFixture]
	class ExceptionExtensionsFixture
	{
		[Test]
		public void GetUnWrappedMessage()
		{
			var innerEx = new ArgumentException("Test Inner Exception.");
			var ex = new Exception("Test Exception1.", innerEx);

			Assert.AreEqual(@"System.Exception: Test Exception1.
System.ArgumentException: Test Inner Exception.
", ex.GetUnWrappedMessage());
		}

		[Test]
		public void GetUnWrappedMessage_AggregateException()
		{
			var innerEx1 = new ArgumentException("Inner Exception 1.");
			var innerEx2 = new InvalidOperationException("Inner Exception 2.");
			var aggregateEx = new AggregateException("Aggregate Exception.", innerEx1, innerEx2);

			var expectedMessage =
				@"System.AggregateException: Aggregate Exception. (Inner Exception 1.) (Inner Exception 2.)-->
System.ArgumentException: Inner Exception 1.
System.InvalidOperationException: Inner Exception 2.
";
			Assert.AreEqual(expectedMessage, aggregateEx.GetUnWrappedMessage());
		}

		[Test]
		public void DeserialiseException()
		{
			Exception exception = null;
			var exceptionJson = JsonConvert.SerializeObject(new ArgumentException("argument error.", new InvalidOperationException("invalid operation.")));
			var success = ExceptionExtensions.TryDeserialiseException(exceptionJson, ref exception);
			var expectedMessage = @"System.ArgumentException: argument error.
System.InvalidOperationException: invalid operation.
";
			Assert.IsTrue(success);
			Assert.AreEqual(expectedMessage, exception.GetUnWrappedMessage());
		}

		[Test]
		public void DeserialiseException_NonSystemException()
		{
			Exception exception = null;
			var exceptionJson = JsonConvert.SerializeObject(new ArgumentException("argument error.", new InvalidOperationException("invalid operation.")));
			var nonSystemExceptionJson = exceptionJson.Replace("System.ArgumentException", "OpenQA.Selenium.NoSuchElementException");
			var success = ExceptionExtensions.TryDeserialiseException(nonSystemExceptionJson, ref exception);
			var expectedMessage = "System.Exception: [OpenQA.Selenium.NoSuchElementException] argument error.\r\nSystem.InvalidOperationException: invalid operation.\r\n";
			Assert.IsTrue(success);
			Assert.AreEqual(expectedMessage, exception.GetUnWrappedMessage());
		}

		[Test]
		public void DeserialiseException_NonSupportedException()
		{
			Exception exception = null;
			var exceptionJson = JsonConvert.SerializeObject(new ArgumentException("argument error.", new InvalidOperationException("invalid operation.")));
			exceptionJson = exceptionJson.Replace("HelpURL", "HelpLink").Replace(@"""ClassName"":""System.ArgumentException"",", "");
			var success = ExceptionExtensions.TryDeserialiseException(exceptionJson, ref exception);
			Assert.IsFalse(success);
			Assert.IsNull(exception);
		}

		[Test]
		public void CreateException()
		{
			var exceptionJson = @"{""Message"":""argument error."",""Data"":null,""InnerException"":{""ClassName"":""System.InvalidOperationException"",""Message"":""invalid operation."",""Data"":null,""InnerException"":null,""HelpLink"":null,""StackTraceString"":null,""RemoteStackTraceString"":null,""RemoteStackIndex"":0,""ExceptionMethod"":null,""HResult"":-2146233079,""Source"":null,""WatsonBuckets"":null},""HelpLink"":null,""StackTraceString"":null,""RemoteStackTraceString"":null,""RemoteStackIndex"":0,""ExceptionMethod"":null,""HResult"":-2147024809,""Source"":null,""WatsonBuckets"":null,""ParamName"":null}";
			var exception = ExceptionExtensions.CreateException(exceptionJson);
			var expectedMessage = @"System.Exception: [UnknownExceptionType] argument error.
System.Exception: [System.InvalidOperationException] invalid operation.
";
			Assert.IsNotNull(exception);
			Assert.AreEqual(expectedMessage, exception.GetUnWrappedMessage());
		}

		[Test]
		public void CreateException_ComplicatedException()
		{
			var exceptionJson = @"{""ClassName"":""System.InvalidOperationException"",""Message"":""An error occurred while processing this request."",""Data"":null,""InnerException"":{""ResponseMessage"":null,""ClassName"":""Microsoft.OData.Client.DataServiceTransportException"",""Message"":""An error occurred while sending the request."",""Data"":null,""InnerException"":{""ClassName"":""System.Net.WebException"",""Message"":""An error occurred while sending the request."",""Data"":null,""InnerException"":{""StatusCode"":null,""Message"":""An error occurred while sending the request."",""Data"":{},""InnerException"":{""ClassName"":""System.IO.IOException"",""Message"":""Unable to read data from the transport connection: An existing connection was forcibly closed by the remote host.."",""Data"":null,""InnerException"":{""ClassName"":""System.Net.Sockets.SocketException"",""Message"":""An existing connection was forcibly closed by the remote host."",""Data"":null,""InnerException"":null,""HelpURL"":null,""StackTraceString"":null,""RemoteStackTraceString"":null,""RemoteStackIndex"":0,""ExceptionMethod"":null,""HResult"":-2147467259,""Source"":null,""WatsonBuckets"":null,""NativeErrorCode"":10054},""HelpURL"":null,""StackTraceString"":""   at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.ThrowException(SocketError error, CancellationToken cancellationToken)\r\n   at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.System.Threading.Tasks.Sources.IValueTaskSource<System.Int32>.GetResult(Int16 token)\r\n   at System.Net.Http.HttpConnection.SendAsyncCore(HttpRequestMessage request, Boolean async, CancellationToken cancellationToken)"",""RemoteStackTraceString"":null,""RemoteStackIndex"":0,""ExceptionMethod"":null,""HResult"":-2146232800,""Source"":""System.Net.Sockets"",""WatsonBuckets"":null},""HelpLink"":null,""Source"":""System.Net.Http"",""HResult"":-2146232800,""StackTrace"":""   at System.Net.Http.HttpConnection.SendAsyncCore(HttpRequestMessage request, Boolean async, CancellationToken cancellationToken)\r\n   at System.Net.Http.HttpConnectionPool.SendWithVersionDetectionAndRetryAsync(HttpRequestMessage request, Boolean async, Boolean doRequestAuth, CancellationToken cancellationToken)\r\n   at System.Net.Http.RedirectHandler.SendAsync(HttpRequestMessage request, Boolean async, CancellationToken cancellationToken)\r\n   at System.Net.Http.HttpClient.<SendAsync>g__Core|83_0(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationTokenSource cts, Boolean disposeCts, CancellationTokenSource pendingRequestsCts, CancellationToken originalCancellationToken)\r\n   at System.Net.HttpWebRequest.SendRequest(Boolean async)\r\n   at System.Net.HttpWebRequest.EndGetResponse(IAsyncResult asyncResult)""},""HelpURL"":null,""StackTraceString"":""   at System.Net.HttpWebRequest.EndGetResponse(IAsyncResult asyncResult)\r\n   at Microsoft.OData.Client.HttpWebRequestMessage.EndGetResponse(IAsyncResult asyncResult)"",""RemoteStackTraceString"":null,""RemoteStackIndex"":0,""ExceptionMethod"":null,""HResult"":-2146232800,""Source"":""System.Net.Requests"",""WatsonBuckets"":null},""HelpURL"":null,""StackTraceString"":""   at Microsoft.OData.Client.HttpWebRequestMessage.EndGetResponse(IAsyncResult asyncResult)\r\n   at Microsoft.OData.Client.DataServiceContext.GetResponseHelper(ODataRequestMessageWrapper request, IAsyncResult asyncResult, Boolean handleWebException)\r\n   at Microsoft.OData.Client.BaseSaveResult.AsyncEndGetResponse(IAsyncResult asyncResult)"",""RemoteStackTraceString"":null,""RemoteStackIndex"":0,""ExceptionMethod"":null,""HResult"":-2146233079,""Source"":""Microsoft.OData.Client"",""WatsonBuckets"":null},""HelpURL"":null,""StackTraceString"":""   at Microsoft.OData.Client.BaseAsyncResult.EndExecute[T](Object source, String method, IAsyncResult asyncResult)\r\n   at Microsoft.OData.Client.DataServiceContext.EndSaveChanges(IAsyncResult asyncResult)\r\n   at System.Threading.Tasks.TaskFactory`1.FromAsyncCoreLogic(IAsyncResult iar, Func`2 endFunction, Action`1 endAction, Task`1 promise, Boolean requiresSynchronization)\r\n--- End of stack trace from previous location ---\r\n   at CargoWise.RefDbRepo.Common.SafeDataClient.Default.Container.CargoWise.RefDbRepo.Common.SafeDataClient.IContainer.SaveChangesAsync(SaveChangesOptions options) in D:\\CodeBase2\\RefDataRepo\\RefDataRepo\\Common\\Infrastructure\\SafeDataClient\\ContainerPartial.cs:line 31\r\n   at CargoWise.RefDbRepo.Common.SafeDataClient.SafeRepository.SaveChangesAysnc() in D:\\CodeBase2\\RefDataRepo\\RefDataRepo\\Common\\Infrastructure\\SafeDataClient\\SafeRepository.cs:line 78\r\n   at CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.MergeProcessor.ProcessSingleBatch(ValueTuple`2[] entities) in D:\\CodeBase2\\RefDataRepo\\RefDataRepo\\Staging\\UniversalXmlProcessor\\UniversalXMLStagingDataProcessor\\MergeProcessor.cs:line 219"",""RemoteStackTraceString"":null,""RemoteStackIndex"":0,""ExceptionMethod"":null,""HResult"":-2146233079,""Source"":""Microsoft.OData.Client"",""WatsonBuckets"":null}";
			var exception = ExceptionExtensions.CreateException(exceptionJson);
			var expectedMessage = @"System.Exception: [System.InvalidOperationException] An error occurred while processing this request.
System.Exception: [Microsoft.OData.Client.DataServiceTransportException] An error occurred while sending the request.
System.Exception: [System.Net.WebException] An error occurred while sending the request.
System.Exception: [UnknownExceptionType] An error occurred while sending the request.
System.Exception: [System.IO.IOException] Unable to read data from the transport connection: An existing connection was forcibly closed by the remote host..
System.Exception: [System.Net.Sockets.SocketException] An existing connection was forcibly closed by the remote host.
";
			Assert.IsNotNull(exception);
			Assert.AreEqual(expectedMessage, exception.GetUnWrappedMessage());
		}

		[Test]
		public void CreateException_ErrorJson()
		{
			var exceptionJson = @"{""Message"":""argument error."",""Data"":null,""InnerException"":{""ClassName"":""System.InvalidOperationException"",""Message"":""invalid operation."",""Data"":null,""InnerException"":null,""HelpLink"":null,""StackTraceString"":null,""RemoteStackTraceString"":null,""RemoteStackIndex"":0,""ExceptionMethod"":null,""HResult"":-2146233079,""Source"":null,""WatsonBuckets"":null},""HelpLink"":null,""StackTraceString"":null,""RemoteStackTraceString"":null,""RemoteStackIndex"":0,""ExceptionMethod"":null,""HResult"":-2147024809,""Source"":null,""WatsonBuckets"":null,""ParamName"":null}}";
			var exception = ExceptionExtensions.CreateException(exceptionJson);
			var expectedMessage = "System.Exception: Exception with details in the StackTrace.\r\n";
			Assert.IsNotNull(exception);
			Assert.AreEqual(expectedMessage, exception.GetUnWrappedMessage());
			Assert.AreEqual(exceptionJson, exception.StackTrace);
		}
	}
}
