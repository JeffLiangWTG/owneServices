using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using static Enterprise.Integration.Forwarding;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.CarbonEmissions.Business.Testing;

public class SendCO2eCalculationRequestProcessorTest : TestCaseWithFactory
{
	public void TestProcess_WithAdditionalSupporters()
	{
		// Arrange
		var address1 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
		var address2 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
		var address3 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;

		var hostSupporter = Factory.NewMoq<DummyCO2eCalculationSupporter>();
		var hostSupporterOnRequestedCalled = false;
		hostSupporter.Setup(x => x.AddressesToValidate).Returns(new[] { address1 });
		hostSupporter
			.Setup(x => x.OnRequested())
			.Callback(() => hostSupporterOnRequestedCalled = true);

		var childSupporter1 = Factory.NewMoq<DummyCO2eCalculationSupporter>();
		var childSupporter1OnRequestedCalled = false;
		childSupporter1.Setup(x => x.AddressesToValidate).Returns(new[] { address2 });
		childSupporter1
			.Setup(x => x.OnRequested())
			.Callback(() => childSupporter1OnRequestedCalled = true);

		var childSupporter2 = Factory.NewMoq<DummyCO2eCalculationSupporter>();
		var childSupporter2OnRequestedCalled = false;
		childSupporter2.Setup(x => x.AddressesToValidate).Returns(new[] { address3 });
		childSupporter2
			.Setup(x => x.OnRequested())
			.Callback(() => childSupporter2OnRequestedCalled = true);

		hostSupporter.Setup(x => x.AdditionalCalculationSupporters)
			.Returns(new[]
			{
				new AdditionalCalculationSupporter(childSupporter1.Object, () => 0m),
				new AdditionalCalculationSupporter(childSupporter2.Object, () => 0m)
			});

		var addressValidationManagerMock = new Mock<IAddressesValidationManager>();
		var requestSender = new Mock<IUniversalXmlWorkflowProcessor>();

		// Act
		var processor = new SendCO2eCalculationRequestProcessor(hostSupporter.Object, new ManualCO2eCalculationActionInfo(hostSupporter.Object), null, addressValidationManagerMock.Object, requestSender.Object);
		processor.Process(new NotificationBuffer());

		// Assert
		addressValidationManagerMock
			.Verify(manager => manager.Validate(), Times.Exactly(3));
		requestSender
			.Verify(sender => sender.Process(It.IsAny<INotifications>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
		Assert(hostSupporterOnRequestedCalled);
		Assert(childSupporter1OnRequestedCalled);
		Assert(childSupporter2OnRequestedCalled);
	}

	public void TestProcess_WithAdditionalSupporters_Current()
	{
		// Arrange
		var hostSupporter = Factory.NewMoq<DummyCO2eCalculationSupporter>();
		var hostSupporterOnRequestedCalled = false;
		hostSupporter.Setup(x => x.ValidateInputs()).Returns(new List<string>());
		hostSupporter
			.Setup(x => x.OnRequested())
			.Callback(() => hostSupporterOnRequestedCalled = true);

		var childSupporter1 = Factory.NewMoq<DummyCO2eCalculationSupporter>();
		childSupporter1.Object.SetCO2eStatus(CO2eStatusList.Codes.Current);
		var childSupporter1OnRequestedCalled = false;
		childSupporter1
			.Setup(x => x.OnRequested())
			.Callback(() => childSupporter1OnRequestedCalled = true);

		var childSupporter2 = Factory.NewMoq<DummyCO2eCalculationSupporter>();
		childSupporter2.Object.SetCO2eStatus(CO2eStatusList.Codes.Current);
		var childSupporter2OnRequestedCalled = false;
		childSupporter2
			.Setup(x => x.OnRequested())
			.Callback(() => childSupporter2OnRequestedCalled = true);

		hostSupporter.Setup(x => x.AdditionalCalculationSupporters)
			.Returns(new[]
			{
				new AdditionalCalculationSupporter(childSupporter1.Object, () => 0m),
				new AdditionalCalculationSupporter(childSupporter2.Object, () => 0m)
			});

		var requestSender = new Mock<IUniversalXmlWorkflowProcessor>();

		// Act
		var processor = new SendCO2eCalculationRequestProcessor(hostSupporter.Object, new ManualCO2eCalculationActionInfo(hostSupporter.Object), null, null, requestSender.Object);
		processor.Process(new NotificationBuffer());

		// Assert
		requestSender
			.Verify(sender => sender.Process(It.IsAny<INotifications>(), It.IsAny<CancellationToken>()), Times.Once);
		Assert(hostSupporterOnRequestedCalled);
		Assert("Child support is not requested because it has CUR status", !childSupporter1OnRequestedCalled);
		Assert("Child support is not requested because it has CUR status", !childSupporter2OnRequestedCalled);
	}

	public void TestProcess_WithAdditionalSupporters_ValidateInputs()
	{
		// Arrange
		var hostSupporter = Factory.NewMoq<DummyCO2eCalculationSupporter>();
		var hostSupporterOnRequestedCalled = false;
		hostSupporter.Protected().Setup<ZString>("HumanReadableNameCore").Returns("HOST");
		hostSupporter.Setup(x => x.ValidateInputs()).Returns(new List<string> { "Weight", "Height" });
		hostSupporter
			.Setup(x => x.OnRequested())
			.Callback(() => hostSupporterOnRequestedCalled = true);

		var childSupporter1 = Factory.NewMoq<DummyCO2eCalculationSupporter>();
		var childSupporter1OnRequestedCalled = false;
		childSupporter1.Protected().Setup<ZString>("HumanReadableNameCore").Returns("FIRST CHILD");
		childSupporter1.Setup(x => x.ValidateInputs()).Returns(new List<string> { "Number", "Word" });
		childSupporter1
			.Setup(x => x.OnRequested())
			.Callback(() => childSupporter1OnRequestedCalled = true);

		var childSupporter2 = Factory.NewMoq<DummyCO2eCalculationSupporter>();
		var childSupporter2OnRequestedCalled = false;
		childSupporter2.Protected().Setup<ZString>("HumanReadableNameCore").Returns("SECOND CHILD");
		childSupporter2.Setup(x => x.ValidateInputs()).Returns(new List<string> { "City", "Postcode" });
		childSupporter2
			.Setup(x => x.OnRequested())
			.Callback(() => childSupporter2OnRequestedCalled = true);

		hostSupporter.Setup(x => x.AdditionalCalculationSupporters)
			.Returns(new[]
			{
				new AdditionalCalculationSupporter(childSupporter1.Object, () => 0m),
				new AdditionalCalculationSupporter(childSupporter2.Object, () => 0m)
			});

		var requestSender = new Mock<IUniversalXmlWorkflowProcessor>();

		// Act
		var notifications = new NotificationBuffer();
		var processor = new SendCO2eCalculationRequestProcessor(hostSupporter.Object, new ManualCO2eCalculationActionInfo(hostSupporter.Object), null, null, requestSender.Object);
		processor.Process(notifications);

		// Assert
		requestSender
			.Verify(sender => sender.Process(It.IsAny<INotifications>(), It.IsAny<CancellationToken>()), Times.Never);
		Assert(!hostSupporterOnRequestedCalled);
		Assert(!childSupporter1OnRequestedCalled);
		Assert(!childSupporter2OnRequestedCalled);
		AssertContains("The greenhouse gas emissions calculation cannot be requested because the following mandatory input is missing or invalid: HOST: Weight, HOST: Height, FIRST CHILD: Number, FIRST CHILD: Word, SECOND CHILD: City, SECOND CHILD: Postcode", notifications.AsString);
	}

	#region GetSupportersToSend

	public void TestGetSupportersToSend_CO2eStatusNotCurrent()
	{
		var (processor, hostSupporter) = CreateProcessorHostSupporterHelpers(CO2eStatusList.Codes.Pending, userSelectsYes: null);

		var methodInfo = typeof(SendCO2eCalculationRequestProcessor).GetMethod("GetSupportersToSend", BindingFlags.NonPublic | BindingFlags.Instance);
		var supportersToSend = (ICO2eCalculationSupporter[])methodInfo.Invoke(processor, new object[] { hostSupporter });

		AssertEquals(1, supportersToSend.Length);
		AssertEquals(hostSupporter, supportersToSend[0]);
	}

	public void TestGetSupportersToSend_CO2eStatusCurrent_UserSelectsYes()
	{
		var childSupporterMock = Factory.NewMoq<DummyCO2eCalculationSupporter>();
		var childSupporter = childSupporterMock.Object;
		childSupporter.SetCO2eStatus(CO2eStatusList.Codes.Pending);

		var additionalSupporters = new[]
		{
			new AdditionalCalculationSupporter(childSupporter, () => 0m)
		};

		var (processor, hostSupporter) = CreateProcessorHostSupporterHelpers(CO2eStatusList.Codes.Current, userSelectsYes: true, additionalSupporters: additionalSupporters);
		var methodInfo = typeof(SendCO2eCalculationRequestProcessor).GetMethod("GetSupportersToSend", BindingFlags.NonPublic | BindingFlags.Instance);
		var supportersToSend = (ICO2eCalculationSupporter[])methodInfo.Invoke(processor, new object[] { hostSupporter });

		AssertEquals(2, supportersToSend.Length);
		AssertEquals(hostSupporter, supportersToSend[0]);
		AssertEquals(childSupporter, supportersToSend[1]);
	}

	public void TestGetSupportersToSend_CO2eStatusCurrent_UserSelectsNo()
	{
		var childSupporterMock = Factory.NewMoq<DummyCO2eCalculationSupporter>();
		var childSupporter = childSupporterMock.Object;
		childSupporter.SetCO2eStatus(CO2eStatusList.Codes.Pending);

		var additionalSupporters = new[]
		{
			new AdditionalCalculationSupporter(childSupporter, () => 0m)
		};

		var (processor, hostSupporter) = CreateProcessorHostSupporterHelpers(CO2eStatusList.Codes.Current, userSelectsYes: false, additionalSupporters: additionalSupporters);

		var methodInfo = typeof(SendCO2eCalculationRequestProcessor).GetMethod("GetSupportersToSend", BindingFlags.NonPublic | BindingFlags.Instance);
		var supportersToSend = (ICO2eCalculationSupporter[])methodInfo.Invoke(processor, new object[] { hostSupporter });
		AssertEquals(0, supportersToSend.Length);
	}

	#endregion

	#region ShouldRecalculate

	public void TestShouldRecalculate_StatusNotCurrent()
	{
		var (processor, hostSupporter) = CreateProcessorHostSupporterHelpers(CO2eStatusList.Codes.Pending, userSelectsYes: null);
		var result = processor.ShouldRecalculate(hostSupporter);

		AssertEquals(true, result);
	}

	public void TestShouldRecalculate_StatusIsCurrent_WithoutCO2eRecalculationChecker()
	{
		var hostSupporter = Factory.New<IForwardingShipment>() as ICO2eCalculationSupporter;
		var logProvider = hostSupporter as IStmALogProvider;

		hostSupporter.SetCO2eStatus(CO2eStatusList.Codes.Current);

		var requestSenderMock = new Mock<IUniversalXmlWorkflowProcessor>();
		requestSenderMock.Setup(sender => sender.Process(It.IsAny<INotifications>(), It.IsAny<CancellationToken>()));

		var processor = new SendCO2eCalculationRequestProcessor(hostSupporter as BusinessObject, new ManualCO2eCalculationActionInfo(hostSupporter as BusinessObject), recalculationChecker: null, requestSender: requestSenderMock.Object);

		var initialLogCount = logProvider.Logs.DatabaseCount;

		var result = processor.ShouldRecalculate(hostSupporter);

		AssertEquals(false, result);

		var logs = logProvider.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.GreenhouseGasEmissionsCalculationCode));
		AssertEquals(initialLogCount + 1, logs.Length);

		var log = logs.Last();
		AssertEquals(nameof(CO2eEventType.NotRequired), log.Parameters[Params.Type]);
		AssertEquals("the CO2e value is current.", log.Parameters[Params.Reason]);
	}

	public void TestShouldRecalculate_StatusIsCurrent_WithCO2eRecalculationChecker_UserSelectsYes()
	{
		var (processor, hostSupporter) = CreateProcessorHostSupporterHelpers(CO2eStatusList.Codes.Current, userSelectsYes: true);
		var result = processor.ShouldRecalculate(hostSupporter);

		AssertEquals(true, result);
	}

	public void TestShouldRecalculate_StatusIsCurrent_WithCO2eRecalculationChecker_UserSelectNo()
	{
		var (processor, hostSupporter) = CreateProcessorHostSupporterHelpers(CO2eStatusList.Codes.Current, userSelectsYes: false);
		var result = processor.ShouldRecalculate(hostSupporter);

		AssertEquals(false, result);
	}

	#endregion

	#region HasSupportersToSend

	public void TestProcessor_HasSupportersToSend_StatusNCU()
	{
		var (processor, _) = CreateProcessorHostSupporterHelpers(CO2eStatusList.Codes.Pending, userSelectsYes: null);
		processor.Process(new NotificationBuffer());

		AssertEquals(true, processor.HasSupportersToSend);
	}

	public void TestProcessor_HasSupportersToSend_StatusCUR_UserSelectNo()
	{
		var (processor, _) = CreateProcessorHostSupporterHelpers(CO2eStatusList.Codes.Current, userSelectsYes: false);
		processor.Process(new NotificationBuffer());

		AssertEquals(false, processor.HasSupportersToSend);
	}

	public void TestProcessor_HasSupportersToSend_StatusCUR_UserSelectYes()
	{
		var (processor, _) = CreateProcessorHostSupporterHelpers(CO2eStatusList.Codes.Current, userSelectsYes: true);
		processor.Process(new NotificationBuffer());

		AssertEquals(true, processor.HasSupportersToSend);
	}

	public void TestProcessor_HasSupportersToSend_HostCUR_AdditionalNCU_UserSelectNo()
	{
		var childSupporterMock = Factory.NewMoq<DummyCO2eCalculationSupporter>();
		var childSupporter = childSupporterMock.Object;
		childSupporter.SetCO2eStatus(CO2eStatusList.Codes.Pending);

		var additionalSupporters = new[]
		{
			new AdditionalCalculationSupporter(childSupporter, () => 0m)
		};

		var (processor, _) = CreateProcessorHostSupporterHelpers(CO2eStatusList.Codes.Current, userSelectsYes: false, additionalSupporters: additionalSupporters);
		processor.Process(new NotificationBuffer());

		AssertEquals(false, processor.HasSupportersToSend);
	}

	public void TestProcessor_HasSupportersToSend_HostCUR_AdditionalNCU_UserSelectYes()
	{
		var childSupporterMock = Factory.NewMoq<DummyCO2eCalculationSupporter>();
		var childSupporter = childSupporterMock.Object;
		childSupporter.SetCO2eStatus(CO2eStatusList.Codes.Pending);

		var additionalSupporters = new[]
		{
			new AdditionalCalculationSupporter(childSupporter, () => 0m)
		};

		var (processor, _) = CreateProcessorHostSupporterHelpers(CO2eStatusList.Codes.Current, userSelectsYes: true, additionalSupporters: additionalSupporters);
		processor.Process(new NotificationBuffer());

		AssertEquals(true, processor.HasSupportersToSend);
	}

	#endregion

	#region Helpers

	(SendCO2eCalculationRequestProcessor processor, DummyCO2eCalculationSupporter hostSupporter) CreateProcessorHostSupporterHelpers(ZString status, bool? userSelectsYes, AdditionalCalculationSupporter[] additionalSupporters = null)
	{
		var hostSupporterMock = Factory.NewMoq<DummyCO2eCalculationSupporter>();
		var hostSupporter = hostSupporterMock.Object;
		hostSupporter.SetCO2eStatus(status);
		hostSupporterMock.Setup(x => x.AdditionalCalculationSupporters).Returns(additionalSupporters ?? Array.Empty<AdditionalCalculationSupporter>());

		ICO2eRecalculationChecker recalculationChecker = null;
		if (userSelectsYes.HasValue)
		{
			var recalculationCheckerMock = new Mock<ICO2eRecalculationChecker>();
			recalculationCheckerMock.Setup(x => x.ShouldRecalculate(hostSupporter)).Returns(userSelectsYes.Value);
			recalculationChecker = recalculationCheckerMock.Object;
		}

		var requestSenderMock = new Mock<IUniversalXmlWorkflowProcessor>();
		requestSenderMock.Setup(sender => sender.Process(It.IsAny<INotifications>(), It.IsAny<CancellationToken>()));

		var processor = new SendCO2eCalculationRequestProcessor(hostSupporter, new ManualCO2eCalculationActionInfo(hostSupporter), recalculationChecker: recalculationChecker, requestSender: requestSenderMock.Object);
		return (processor, hostSupporter);
	}

	#endregion
}
