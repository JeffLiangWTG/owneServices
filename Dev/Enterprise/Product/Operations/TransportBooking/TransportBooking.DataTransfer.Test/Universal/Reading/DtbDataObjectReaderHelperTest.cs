using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.TransportCommon.Registry;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Moq;

namespace Enterprise.TransportBookings.DataTransfer.Test.Universal.Reading
{
	public class DtbDataObjectReaderHelperTest : TestCaseWithFactory
	{
		public void TestMessageIsFromTestCba()
		{
			var dataObjectBkp = NewDataObject(RecipientRoleType.BKP);
			var dataObjectTpc = NewDataObject(RecipientRoleType.TPC);

			using (TransportRegistry.Instance.TestCbaId.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TestCbaId))
			{
				AssertEquals("MessageIsFromTestCba() should return true if passed value of TestCbaId registry and data object has BKP recipient", true, DtbDataObjectReaderHelper.MessageIsFromTestCba(TestCbaId, dataObjectBkp));
				AssertEquals("MessageIsFromTestCba() should return false if data object does not have BKP recipient", false, DtbDataObjectReaderHelper.MessageIsFromTestCba(TestCbaId, dataObjectTpc));
				AssertEquals("MessageIsFromTestCba() should return false if passed value other than in the TestCbaId registry", false, DtbDataObjectReaderHelper.MessageIsFromTestCba(OtherCode, dataObjectBkp));
				AssertEquals("MessageIsFromTestCba() should return false if both passed value other than in the TestCbaId registry and data object does not have BKP recipient", false, DtbDataObjectReaderHelper.MessageIsFromTestCba(OtherCode, dataObjectTpc));
			}

			AssertEquals("MessageIsFromTestCba() should return false if TestCbaId registry not set", false, DtbDataObjectReaderHelper.MessageIsFromTestCba(TestCbaId, dataObjectBkp));
		}

		public void TestGetInterchangeTypeAndSenderFromLogger()
		{
			var testInterchangeType = "XXX";
			var testSender = "TESTSENDER";

			var xmlSessionTracker = new TestErrorLogger();
			var mockInterchange = new Mock<IEDIInterchange>();
			mockInterchange.SetupGet(i => i.EI_TransportType).Returns(testInterchangeType);
			mockInterchange.SetupGet(i => i.EI_From).Returns(testSender);
			var mockMessage = new Mock<IEDIMessage>();
			mockMessage.SetupGet(m => m.Interchange).Returns(mockInterchange.Object);
			xmlSessionTracker.SourceMessage = mockMessage.Object;

			(var actualInterchangeType, var actualSender) = DtbDataObjectReaderHelper.GetInterchangeTypeAndSenderFromLogger(xmlSessionTracker);
			AssertEquals("GetInterchangeTypeAndSenderFromLogger() should get correct interchange type", testInterchangeType, actualInterchangeType);
			AssertEquals("GetInterchangeTypeAndSenderFromLogger() should get correct sender", testSender, actualSender);
		}

		public void TestHasSourceContext()
		{
			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.TransportBooking, "TB");
			dataContext.AddDataSource(DataContextType.TransportBookingConsolidation, "TBC");
			dataObject.DataContext = dataContext;

			CombineAssertions("Should correctly identify whether data sources are present on the data context", () =>
			{
				Assert("Should have dataSource for TransportBooking", DtbDataObjectReaderHelper.HasSourceContext(dataContext, DataContextType.TransportBooking));
				Assert("Should have dataSource for TransportBookingConsolidation", DtbDataObjectReaderHelper.HasSourceContext(dataContext, DataContextType.TransportBookingConsolidation));
				Assert("Should not have dataSource for TransportBookingConfirmation", !DtbDataObjectReaderHelper.HasSourceContext(dataContext, DataContextType.TransportBookingConfirmation));
			});
		}

		Shipment NewDataObject(RecipientRoleType roleType)
		{
			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = DataContextFactory.New();
			dataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole() { Code = roleType } };
			dataObject.DataContext = dataContext;

			return dataObject;
		}
		const string TestCbaId = "TESTCBAID";
		const string OtherCode = "OTHERCODE";
	}
}
