using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Moq;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class CreateStandAloneDeclarationProcessorTest : TestCaseWithFactory
	{
		public void TestProcess_CreatesStandAloneDeclarationForConsignmentUsingService()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			AssertNull("Precondition: Declaration not created yet", consignment.StandAloneDeclarationForCurrentCompany);
			Factory.Save();

			var mockSuccessResponse = new Mock<IConvertToStandAloneDeclarationResponse>();
			mockSuccessResponse.SetupGet(mock => mock.ConversionSucceeded).Returns(true);

			var mockService = new Mock<IConvertToStandAloneDeclarationService>();
			mockService.Setup(mock => mock.ConvertToStandAloneDeclaration(It.IsAny<IHVLVConsignment>()))
				.Returns(mockSuccessResponse.Object);

			using (ObjectFactory.Substitute(mockService.Object))
			{
				var logs = new NotificationCollection();
				var processor = new CreateStandAloneDeclarationProcessor(consignment);
				processor.Process(logs);

				AssertNoExceptionThrown("Service should be called for consignment", () =>
				{
					mockService.Verify(mock => mock.ConvertToStandAloneDeclaration(consignment));
				});
				AssertEquals("No error logs", 0, logs.Count);
			}
		}

		public void TestProcess_WhenCannotCreateStandAloneDeclaration_LogsError()
		{
			const string ErrorMessage = "There's always some excuse...";
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			AssertNull("Precondition: Declaration not created yet", consignment.StandAloneDeclarationForCurrentCompany);
			Factory.Save();

			var mockFailureResponse = new Mock<IConvertToStandAloneDeclarationResponse>();
			mockFailureResponse.SetupGet(mock => mock.ConversionSucceeded).Returns(false);
			mockFailureResponse.SetupGet(mock => mock.ConversionFailureReason).Returns(ErrorMessage);

			var mockService = new Mock<IConvertToStandAloneDeclarationService>();
			mockService.Setup(mock => mock.ConvertToStandAloneDeclaration(It.IsAny<IHVLVConsignment>()))
				.Returns(mockFailureResponse.Object);

			using (ObjectFactory.Substitute(mockService.Object))
			{
				var logs = new NotificationCollection();
				var processor = new CreateStandAloneDeclarationProcessor(consignment);
				processor.Process(logs);

				AssertContainsExactElementsInAnyOrder(new[] { $"Stand Alone Declaration could not be created for HVLV Consignment ({consignment.HVC_WaybillNumber}): {ErrorMessage}" }, logs.Select(log => log.Message));
			}
		}
	}
}
