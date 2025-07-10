using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.TransportBookings.Document.Testing
{
	[TestedType(typeof(DtbBookingConfirmationVisualizableDocumentSupporter))]
	class DtbBookingConfirmationVisualizableDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestGetMessageBroker()
		{
			var confirmation = CreateTestConfirmation();
			var supporter = confirmation.GetSupporter();
			AssertEquals("Supporter should have correct MessageBroker in order to be able to send via Direct XT", EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface, supporter.GetMessageBroker());
		}

		public void TestGetEventParent()
		{
			const string TestDocName = "TestDoc";
			var confirmation = CreateTestConfirmation();
			var jobDocumentData = (BusinessObject)Factory.New<IVisualizerDocumentData>();
			jobDocumentData[JobDocumentDataSchema.Constants.JDD_Name] = TestDocName;
			jobDocumentData[JobDocumentDataSchema.Constants.JDD_ParentID] = confirmation.PK;
			jobDocumentData[JobDocumentDataSchema.Constants.JDD_ParentTableCode] = DtbBookingConfirmationSchema.Constants.Prefix;
			jobDocumentData.FillWithValidTestData();
			var mockDocumentaryOverride = new Mock<IDocumentaryOverride>();
			mockDocumentaryOverride.SetupGet(dov => dov.DocumentName).Returns(TestDocName);
			var mockDataContext = new Mock<IDataContextDataObject>();
			mockDataContext.SetupGet(dc => dc.DocumentaryOverride).Returns(mockDocumentaryOverride.Object);
			var mockUniversalEvent = new Mock<IXmlEventValueObject>();
			mockUniversalEvent.SetupGet(e => e.DataContext).Returns(mockDataContext.Object);

			var supporter = confirmation.GetSupporter();
			var eventParent = (supporter.GetEventParent(mockUniversalEvent.Object) as BusinessObject);
			AssertNotNull("Supporter.GetEventParent() should return BusinessObject (jobDocumentData)", eventParent);
			AssertEquals("Supporter.GetEventParent() should return JobDocumentData record", JobDocumentDataSchema.Constants.TableName, eventParent.TableName);
			AssertEquals("Supporter.GetEventParent() should be able to find the correct document parent (jobDocumentData)", jobDocumentData.PK, eventParent.PK);
		}

		public void TestGetUniversalXmlDataObject()
		{
			var confirmation = CreateTestConfirmation();
			var mockDataObject = new Mock<ITopLevelDataObject>();
			var mockWriterStrategy = new Mock<IDataObjectWriterStrategy>();
			var mockDocument = new Mock<IDocument>();
			var mockWriter = new Mock<ITransportBookingsDocDataObjectUXmlWriter>();
			mockWriter.Setup(w => w.GetDataObject(It.IsAny<IDataObjectWriterStrategy>(), It.IsAny<IDocument>(), It.IsAny<MessageType>())).Returns(mockDataObject.Object);
			var supporter = confirmation.GetSupporter();

			using (ObjectFactory.Substitute(nameof(ITransportBookingsDocDataObjectUXmlWriter), mockWriter.Object))
			{
				var getDataObjectResult = supporter.GetUniversalXmlDataObject(mockWriterStrategy.Object, mockDocument.Object, MessageType.Unspecified);
				AssertEquals("Supporter.GetDataObject should return the dataObject returned by the writer", mockDataObject.Object, getDataObjectResult);
			}
		}

		public void TestShouldUseDraftWatermark()
		{
			var confirmation = CreateTestConfirmation();
			var supporter = confirmation.GetSupporter();
			var mockDocument = new Mock<IDocument>();

			AssertEquals("Supporter.ShouldUseDraftWatermark() should return false", false, supporter.ShouldUseDraftWatermark(mockDocument.Object));
		}

		DtbBookingConfirmation CreateTestConfirmation()
		{
			var helper = new TransportBookingTestHelper(Factory);
			var booking = helper.CreateBooking();
			var instruction = helper.CreateInstruction(booking, "PIC");
			return helper.CreateConfirmation(instruction, "PIC");
		}
	}
}
