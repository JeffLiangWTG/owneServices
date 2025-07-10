using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.Business;
using WTG.Foundation.Http;
using WTG.RTUS.Interface;
using static CargoWise.EventReference.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public abstract class RTUSBookingProviderTest<P, T> : TestCaseWithFactory
		where P : RTUSBookingProvider<T>
		where T : BusinessObject, IStmALogProvider
	{
		protected void BookLastMileCarrier_Success(int expectedCount)
		{
			SetUpRTUSConfig();
			var responseCollection = provider.BookLastMileCarrier(bizo.PK.ToGuid());

			AssertEquals("Should return two responses", expectedCount, responseCollection.Count);
			Assert("HasError should be false", !responseCollection.HasError);
			AssertEquals("Error Messages should be blank", string.Empty, responseCollection.ErrorMessage);

			foreach (var response in responseCollection)
			{
				Assert(response.Successful);
				CombineAssertions(() =>
				{
					AssertEquals("I'm a label and I'm sticky.", Encoding.UTF8.GetString(response.BinaryData.ToArray()));
					AssertEquals("PDF", response.FileType);
					AssertEquals(string.Empty, response.ErrorMessage);
					AssertBookRequestedAndSucceededLogs(bizo.Logs.GetAllLogs().OfType<StmALog>());
					AssertBizoUpdatedAfterBookingSucceeded();
				});
			}
		}

		protected void BookLastMileCarrier_Failed_RTUSMalfunction(string[] failStrings, int expectedCount, string expectedError)
		{
			SetUpRTUSConfig();
			processor.FailStrings = failStrings;
			var responseCollection = provider.BookLastMileCarrier(bizo.PK.ToGuid());

			AssertEquals("Response count should match", expectedCount, responseCollection.Count);
			Assert("HasError should be true", responseCollection.HasError);
			AssertEquals("Aggregated error Messages should match", expectedError, responseCollection.ErrorMessage);
			AssertBookRequestedButFailedLogs(bizo.Logs.GetAllLogs().OfType<StmALog>());
			AssertBizoUpdatedAfterBookingFailed();
		}

		public void TestBookLastMileCarrier_Failed_BizoNotFound()
		{
			SetUpRTUSConfig();
			var guid = Guid.NewGuid();
			var expectedErrorMessage = $"Unable to find {typeof(T).Name} by PK [{guid}]";

			AssertExceptionThrown<InvalidOperationException>("Should throw exception", expectedErrorMessage, () => provider.BookLastMileCarrier(guid));
		}

		public void TestBookLastMileCarrier_Succeeded_SavesLabelsToeDocs()
		{
			SetUpRTUSConfig();
			var responseCollection = provider.BookLastMileCarrier(bizo.PK.ToGuid());
			var docManagerInfo = ((IDocManagerSupportBase)bizo).DocManagerInfoCore();

			AssertNotNull(docManagerInfo);
			AssertEquals(ExpectedeDocsCount, ((DocManagerInfo)docManagerInfo).AllEDocs.Count);
			AssertContainsExactElementsInAnyOrder(ExcpectedeDocsFileNames, ((DocManagerInfo)docManagerInfo).AllEDocs.Cast<IeDoc>().Select(x => x.FileNameOnly));
		}

		protected void BookLastMileCarrier_Failed_RTUSReturnsNull(string expectedError)
		{
			SetUpRTUSConfig();
			processor.ShouldReturnNull = true;
			var responseCollection = provider.BookLastMileCarrier(bizo.PK.ToGuid());

			Assert("HasError should be true", responseCollection.HasError);
			AssertEquals(expectedError, responseCollection.ErrorMessage);
			AssertBookRequestedButFailedLogs(bizo.Logs.GetAllLogs().OfType<StmALog>());
			AssertBizoUpdatedAfterBookingFailed();
		}

		protected void BookLastMileCarrier_Exception_Empty_CreateUniversalShipments(string expectedError)
		{
			SetUpRTUSConfig();
			DeleteItems();

			AssertExceptionThrown<InvalidOperationException>("Should throw exception", expectedError, () => provider.BookLastMileCarrier(bizo.PK.ToGuid()));
		}

		protected void CancellationNullResponse(string expectedReferenceNumber, int expectedNumberOfRejectedLogs)
		{
			SetUpRTUSConfig();

			processor.ShouldReturnNull = true;
			var responseCollection = provider.CancelBooking(bizo.PK.ToGuid());
			var logs = bizo.Logs.GetAllLogs();
			AssertEquals("Rejected logs count should match", expectedNumberOfRejectedLogs, logs.Count);

			var expectedReference = "|TYP=Last Mile Carrier|REF=" + expectedReferenceNumber;
			var log = logs.OfType<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == AutoEvents.BookingRejectedCode && l.SL_Reference == expectedReference);
			AssertNotNull("Should have Rejected log with expected reference format", log);
		}

		#region Implementation

		protected abstract void AssertBookRequestedButFailedLogs(IEnumerable<StmALog> logs);

		protected abstract void AssertBookRequestedAndSucceededLogs(IEnumerable<StmALog> logs);

		protected abstract void AssertBizoUpdatedAfterBookingSucceeded();

		protected virtual void AssertBizoUpdatedAfterBookingFailed() { }

		protected void SetUpRTUSConfig()
		{
			var rtusCollection = new OrganisationRTUSCollection();
			var rtus = rtusCollection.AddNew();
			rtus.CBACode = "SMA";
			rtus.OrganisationPK = lmcAgent.PK;
			rtus.Url = "https://book.carrier.com";
			disposables.Add(TransportRegistry.Instance.OrganisationRTUSOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rtusCollection));
		}

		protected override void SetUp()
		{
			processor = new RTUSProcessorForTest();
			disposables.Add(ObjectFactory.Substitute<IRTUSProcessor>(processor));
			lmcAgent = Factory.NewWithValidTestData<OrgHeader>();
			bizo = CreateBusinessObjectForTest();
			provider = GetLastMileCarrierBookingForTest();
		}

		protected override void TearDown()
		{
			foreach (var disposable in disposables)
			{
				disposable.Dispose();
			}

			base.TearDown();
		}

		protected RTUSProcessorForTest processor;

		protected ZGuid BookingAgentPK => lmcAgent.PK;

		protected abstract P GetLastMileCarrierBookingForTest();

		protected abstract T CreateBusinessObjectForTest();

		protected abstract void DeleteItems();

		protected abstract int ExpectedeDocsCount { get; }

		protected abstract string[] ExcpectedeDocsFileNames { get; }

		protected OrgHeader lmcAgent;

		protected P provider;

		protected T bizo;
		readonly List<IDisposable> disposables = new List<IDisposable>();

		#endregion
	}

	public class RTUSProcessorForTest : IRTUSProcessor
	{
		public bool ShouldReturnNull { get; set; }

		public string[] FailStrings { get; set; }

		public int IdealPackagesInBatchSegment => throw new NotImplementedException();

		public T PushMessage<T>(RequestType<T> requestType, IHttpClientFactory httpClientFactory, Stream universalXmlStream, RTUSCBA rtusType, Uri rtusUrl) where T : IRTUSResponse
		{
			T result = default;
			var reader = new StreamReader(universalXmlStream);
			var content = reader.ReadToEnd();
			var match = Regex.Match(content, @"<Key>(.*)<\/Key>");
			result = ShouldReturnNull
			? result :
			(
				FailStrings != null && FailStrings.Any(s => content.Contains(s))
				? (T)(IRTUSResponse)new RTUSResponseForTest("I don't know why but something terrible's happened.")
				: (T)(IRTUSResponse)new RTUSResponseForTest(FileType.PDF, Encoding.UTF8.GetBytes("I'm a label and I'm sticky."), match.Groups[1].ToString(), OrderTypes.Codes.Ecommerce, string.Empty)
			);

			return result;
		}
	}

	public class RTUSResponseForTest : ISingleBookingRTUSResponse
	{
		public RTUSResponseForTest(string errorMessage)
		{
			ErrorMessageForFailure = errorMessage;
		}

		public RTUSResponseForTest(FileType fileType, byte[] binaryData, string trackingNumber, string orderType, string errorMessage) : this(errorMessage)
		{
			FileType = fileType;
			BinaryData = binaryData;
			TrackingNumber = trackingNumber;
			OrderType = orderType;
		}

		public string ErrorMessageForFailure { get; }

		public FileType FileType { get; }

		public byte[] BinaryData { get; }

		public string ParentID { get; }

		public string RequestPackageID { get; }

		public string TrackingNumber { get; }

		public string TransportReference { get; }

		public string OrderType { get; }

		public bool IsSuccessful => string.IsNullOrEmpty(ErrorMessageForFailure);
	}
}
