using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.ApiClient;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI.CO2e;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.ApiClient;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using CO2eTestHelper = Enterprise.Freight.DataTransfer.Universal.Testing.CO2eTestHelper;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class CO2eRequestProcessorTest : BaseFreightTest
	{
		#region Success

		public void TestSendApiRequestAsync_Success()
		{
			RunTest_SendApiRequest_Success(shipment =>
				new CO2eRequestProcessor().SendRequestAsync(shipment, null).GetAwaiter().GetResult());
		}

		public void TestSendApiRequestSync_Success()
		{
			RunTest_SendApiRequest_Success(shipment =>
				new CO2eRequestProcessor().SendRequest(shipment, null));
		}

		void RunTest_SendApiRequest_Success(Func<ForwardingShipment, CO2eProcessResult> getResult)
		{
			// Arrange
			var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			Factory.Save();
			ResponseGenerator = () => CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response);

			// Act
			var result = getResult.Invoke(shipment);

			// Assert
			AssertEquals(CO2eResultType.ApiSuccess, result.Type);

			shipment = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);

			CombineAssertions("CO2e Value", () =>
			{
				AssertEquals(10000m, shipment.GetTotalCO2e());
				AssertEquals(8000m, shipment.TransportsIncludingRelated[0].GetCO2ePerTonneInKg());
				AssertEquals(8000m, shipment.TransportsIncludingRelated[0].Sailing.GetCO2ePerTonneInKg());
				AssertEquals(2000m, shipment.TransportsIncludingRelated[1].GetCO2ePerTonneInKg());
			});

			CombineAssertions("GHG Event", () =>
			{
				AssertGHGEvent(shipment.Logs, "|NEW=10000|OLD=NA|TYP=Updated");
				AssertGHGEvent(shipment.TransportsIncludingRelated[0].Logs, "|NEW=8000|OLD=NA|TYP=Updated");
				AssertGHGEvent(shipment.TransportsIncludingRelated[1].Logs, "|NEW=2000|OLD=NA|TYP=Updated");
				AssertGHGEvent(shipment.TransportsIncludingRelated[0].Sailing.Logs, "|NEW=8000|OLD=NA|TYP=Updated");
				AssertNull(shipment.TransportsIncludingRelated[1].Sailing);
			});

			CombineAssertions("CO2e Status", () =>
			{
				AssertEquals(CO2eStatusList.Codes.Current, shipment.GetCO2eStatus());
				foreach (Transport leg in shipment.TransportsIncludingRelated)
				{
					AssertEquals(CO2eStatusList.Codes.Current, leg.GetCO2eStatus());
					if (leg.JW_IsLinked)
					{
						AssertEquals(CO2eStatusList.Codes.Current, leg.Sailing.GetCO2eStatus());
					}
				}
			});

			CombineAssertions("DIM & DEX & EDI Interchange & EDI message", () =>
			{
				AssertDataExportEvent(shipment.Logs, result.Result);
				AssertDataImportEvent(shipment.Logs, result.Result);
			});
		}

		#endregion

		#region Success with addresses validation

		[ExpectNoExceptions]
		public void TestSendApiRequestAsync_Success_WithAddressValidation()
		{
			RunTestSendApiRequest_Success_WithAddressValidation(
				(shipment, addressValidationManager) => new CO2eRequestProcessor(addressValidationManager: addressValidationManager).SendApiRequestAsync(shipment).GetAwaiter().GetResult(),
				(mock, times) => mock.Verify(manager => manager.ValidateAsync(It.IsAny<CancellationTokenSource>()), times));
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestSendApiRequestSync_Success_WithAddressValidation()
		{
			RunTestSendApiRequest_Success_WithAddressValidation(
				(shipment, addressValidationManager) => new CO2eRequestProcessor(addressValidationManager: addressValidationManager).SendApiRequest(shipment),
				(mock, times) => mock.Verify(manager => manager.Validate(), times));
		}

		void RunTestSendApiRequest_Success_WithAddressValidation(Func<ForwardingShipment, IAddressesValidationManager, CO2eProcessResult> getResult,
			Action<Mock<IAddressesValidationManager>, Func<Times>> verifyAction)
		{
			// Arrange
			var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			shipment.JS_OA_ImportReleaseDepot = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			shipment.JS_OA_ExportReceivingDepot = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			Factory.Save();
			ResponseGenerator = () => CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response);

			var addressValidationManagerMock = new Mock<IAddressesValidationManager>();
			addressValidationManagerMock
				.Setup(manager => manager.ValidateAsync(It.IsAny<CancellationTokenSource>()))
				.Returns(Task.CompletedTask);

			addressValidationManagerMock.Setup(manager => manager.Validate());

			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// Act
				var result = getResult.Invoke(shipment, addressValidationManagerMock.Object);

				// Assert
				AssertEquals(CO2eResultType.ApiSuccess, result.Type);
				verifyAction.Invoke(addressValidationManagerMock, Times.Once);

				result = getResult.Invoke(shipment, addressValidationManagerMock.Object);
				AssertEquals(CO2eResultType.NotRequired, result.Type);
			}

			shipment.SetCO2eStatus(CO2eStatusList.Codes.Pending);
			Factory.Save();

			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				// Act
				addressValidationManagerMock.Reset();
				var result = getResult.Invoke(shipment, addressValidationManagerMock.Object);

				// Assert
				AssertEquals(CO2eResultType.ApiSuccess, result.Type);
				verifyAction.Invoke(addressValidationManagerMock, Times.Never);
			}
		}

		#endregion

		#region Reject

		public void TestSendApiRequestAsync_Reject()
		{
			RunTestSendApiRequest_Reject(shipment =>
				new CO2eRequestProcessor().SendRequestAsync(shipment, null).GetAwaiter().GetResult());
		}

		public void TestSendApiRequestSync_Reject()
		{
			RunTestSendApiRequest_Reject(shipment => new CO2eRequestProcessor().SendRequest(shipment, null));
		}

		void RunTestSendApiRequest_Reject(Func<ForwardingShipment, CO2eProcessResult> getResult)
		{
			// Arrange
			var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			Factory.Save();
			ResponseGenerator = () => CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalEvent_Response);

			// Act
			var result = getResult.Invoke(shipment);

			// Assert
			AssertEquals(CO2eResultType.ApiRejected, result.Type);

			shipment = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);

			CombineAssertions("CO2e Value", () =>
			{
				AssertEquals(0m, shipment.GetTotalCO2e());
				AssertEquals(0m, shipment.TransportsIncludingRelated[0].GetCO2ePerTonneInKg());
				AssertEquals(0m, shipment.TransportsIncludingRelated[0].Sailing.GetCO2ePerTonneInKg());
				AssertEquals(0m, shipment.TransportsIncludingRelated[1].GetCO2ePerTonneInKg());
			});

			CombineAssertions("GHG Event", () =>
			{
				AssertGHGEvent(shipment.Logs, $"|RES={result.Message}|TYP=Rejected");
				AssertGHGEvent(shipment.TransportsIncludingRelated[0].Logs, $"|RES={result.Message}|TYP=Rejected");
				AssertGHGEvent(shipment.TransportsIncludingRelated[1].Logs, $"|RES={result.Message}|TYP=Rejected");
				AssertGHGEvent(shipment.TransportsIncludingRelated[0].Sailing.Logs, $"|RES={result.Message}|TYP=Rejected");
				AssertNull(shipment.TransportsIncludingRelated[1].Sailing);
			});

			CombineAssertions("CO2e Status", () =>
			{
				AssertEquals(CO2eStatusList.Codes.Rejected, shipment.GetCO2eStatus());
				foreach (Transport leg in shipment.TransportsIncludingRelated)
				{
					AssertEquals(CO2eStatusList.Codes.Rejected, leg.GetCO2eStatus());
					if (leg.JW_IsLinked)
					{
						AssertEquals(CO2eStatusList.Codes.Rejected, leg.Sailing.GetCO2eStatus());
					}
				}
			});

			AssertIntercangeRejectedEvent(shipment.Logs, result.Result);
		}

		#endregion

		#region Cancel

		public void TestCO2eRequestProcessor_SendApiRequestAsync_Cancel()
		{
			// Arrange
			var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			Factory.Save();

			var tokenSource = new CancellationTokenSource();
			var token = tokenSource.Token;

			var clientMock = new Mock<ICO2eApiClient>();
			clientMock.Setup(x => x.GetEmissionAsync(It.IsAny<BusinessObject>(), It.IsAny<CancellationToken>(), It.IsAny<ICO2eCalculationSupporter>())).Callback(tokenSource.Cancel).ThrowsAsync(new TaskCanceledException());

			var managerMock = new Mock<CO2eCalculationManager>(clientMock.Object) { CallBase = true };
			managerMock.SetupGet(x => x.Token).Returns(token);

			var manager = managerMock.Object;

			// Act
			var result = new CO2eRequestProcessor(manager).SendApiRequestAsync(shipment).GetAwaiter().GetResult();

			shipment = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);

			// Assert
			AssertEquals(CO2eResultType.ApiCancelled, result.Type);
			AssertGHGEvent(shipment.Logs, "|RES=Request cancelled by user.|TYP=Rejected", 1);
		}

		#endregion

		#region Timeout

		public void TestCO2eRequestProcessor_SendApiRequestAsync_Timeout()
		{
			RunTestSendApiRequestAsync_Timeout((shipment, manager) =>
				new CO2eRequestProcessor(manager).SendApiRequestAsync(shipment).GetAwaiter().GetResult());
		}

		public void TestCO2eRequestProcessor_SendApiRequestSync_Timeout()
		{
			RunTestSendApiRequestAsync_Timeout((shipment, manager) => new CO2eRequestProcessor(manager).SendApiRequest(shipment));
		}

		void RunTestSendApiRequestAsync_Timeout(Func<ForwardingShipment, CO2eCalculationManager, CO2eProcessResult> getResult)
		{
			// Arrange
			var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			Factory.Save();

			var tokenSource = new CancellationTokenSource();
			var token = tokenSource.Token;

			var clientMock = new Mock<ICO2eApiClient>();
			clientMock.Setup(x => x.GetEmissionAsync(It.IsAny<BusinessObject>(), It.IsAny<CancellationToken>(), It.IsAny<ICO2eCalculationSupporter>())).ThrowsAsync(new TaskCanceledException());
			clientMock.Setup(x => x.GetEmission(It.IsAny<BusinessObject>(), It.IsAny<ICO2eCalculationSupporter>())).Throws(new TaskCanceledException());

			var managerMock = new Mock<CO2eCalculationManager>(clientMock.Object) { CallBase = true };
			managerMock.SetupGet(x => x.Token).Returns(token);

			var manager = managerMock.Object;
			manager.Timeout += (s, _) => { ((ICO2eProvider)shipment).RecordLog(CO2eEventType.Rejected, "Request timeout."); };

			// Act
			var result = getResult.Invoke(shipment, manager);

			shipment = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);

			// Assert
			AssertEquals(CO2eResultType.Empty, result.Type);
			AssertGHGEvent(shipment.Logs, "|RES=Request timeout.|TYP=Rejected", 1);
		}

		#endregion

		#region Unauthorised

		public void TestCO2eRequestProcessor_SendApiRequestAsync_Unauthorised()
		{
			RunTestSendApiRequest_Unauthorised((shipment, manager)
				=> new CO2eRequestProcessor(manager).SendApiRequestAsync(shipment).GetAwaiter().GetResult());
		}

		public void TestCO2eRequestProcessor_SendApiRequestSync_Unauthorised()
		{
			RunTestSendApiRequest_Unauthorised((shipment, manager) => new CO2eRequestProcessor(manager).SendApiRequest(shipment));
		}

		void RunTestSendApiRequest_Unauthorised(Func<ForwardingShipment, CO2eCalculationManager, CO2eProcessResult> getResult)
		{
			// Arrange
			var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			Factory.Save();

			var tokenSource = new CancellationTokenSource();
			var token = tokenSource.Token;

			var clientMock = new Mock<ICO2eApiClient>();
			clientMock.Setup(x => x.GetEmissionAsync(It.IsAny<BusinessObject>(), It.IsAny<CancellationToken>(), It.IsAny<ICO2eCalculationSupporter>())).ThrowsAsync(new Exception("401 (Unauthorized. Invalid credentials.)."));
			clientMock.Setup(x => x.GetEmission(It.IsAny<BusinessObject>(), It.IsAny<ICO2eCalculationSupporter>())).Throws(new Exception("401 (Unauthorized. Invalid credentials.)."));

			var managerMock = new Mock<CO2eCalculationManager>(clientMock.Object) { CallBase = true };
			managerMock.SetupGet(x => x.Token).Returns(token);

			var manager = managerMock.Object;

			ExceptionReporterTestListener.Instance.Clear();

			// Act
			var result = getResult.Invoke(shipment, manager);

			shipment = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);

			// Assert
			AssertEquals("The result type should be CO2eResultType.Unauthorized", CO2eResultType.Unauthorized, result.Type);
			AssertGHGEvent(shipment.Logs, "|RES=Unauthorized Request: Ensure you are using a registered version of CargoWise|TYP=Unauthorized", 1);
			AssertEquals("No exception should be reported using ExceptionReporter for error 401", 0, ExceptionReporterTestListener.Instance.Count);

			ExceptionReporterTestListener.Instance.Clear();
		}

		#endregion

		#region NotFound

		public void TestCO2eRequestProcessor_SendApiRequestAsync_NotFound()
		{
			RunTestSendApiRequest_NotFound(shipment =>
			{
				return Task.Factory.StartNew(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						return new CO2eRequestProcessor(progressFormManager: new DummyProgressForm()).SendApiRequestAsync(shipment).GetAwaiter().GetResult();
					}
				}).GetAwaiter().GetResult();
			});
		}

		public void TestCO2eRequestProcessor_SendApiRequestSync_NotFound()
		{
			RunTestSendApiRequest_NotFound(shipment => new CO2eRequestProcessor().SendApiRequest(shipment));
		}

		void RunTestSendApiRequest_NotFound(Func<ForwardingShipment, CO2eProcessResult> getResult)
		{
			// Arrange
			var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			Factory.Save();

			var client = new Mock<IApiClient>();
			var notFoundResponse = new ApiResponse<EmissionResult>(
				new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.NotFound,
					Content = new StringContent("Not Found"),
					RequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.co2e.wtg.zone/shipment/")
				}, null, client.Object);
			client.Setup(x => x.PostAsync<EmissionResult>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(notFoundResponse);

			ExceptionReporterTestListener.Instance.Clear();
			ObjectFactory.DisposeSubstitutions();

			// Act
			using (ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest()))
			using (ObjectFactory.Substitute("HttpClient", client.Object))
			{
				var result = getResult.Invoke(shipment);

				// Assert
				AssertEquals("The result type should be CO2eResultType.ApiError", CO2eResultType.ApiError, result.Type);
				AssertEquals("Exception should be reported, including the endpoint", 1, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("Response status code does not indicate success: 404 (Not Found). Request endpoint: https://api.co2e.wtg.zone/shipment/", ExceptionReporterTestListener.Instance[0].Message);
			}

			ExceptionReporterTestListener.Instance.Clear();
		}

		#endregion

		#region ServiceUnavailable

		public void TestCO2eRequestProcessor_SendApiRequestAsync_ServiceUnavailable()
		{
			RunTestSendApiRequest_ServiceUnavailable((shipment, manager)
				=> new CO2eRequestProcessor(manager).SendApiRequestAsync(shipment).GetAwaiter().GetResult());
		}

		public void TestCO2eRequestProcessor_SendApiRequestSync_ServiceUnavailable()
		{
			RunTestSendApiRequest_ServiceUnavailable((shipment, manager)
				=> new CO2eRequestProcessor(manager).SendApiRequest(shipment));
		}

		void RunTestSendApiRequest_ServiceUnavailable(Func<ForwardingShipment, CO2eCalculationManager, CO2eProcessResult> getResult)
		{
			var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			Factory.Save();

			var tokenSrc = new CancellationTokenSource();
			var token = tokenSrc.Token;

			var clientMock = new Mock<ICO2eApiClient>();
			clientMock.Setup(x => x.GetEmissionAsync(It.IsAny<BusinessObject>(), It.IsAny<CancellationToken>(), It.IsAny<ICO2eCalculationSupporter>())).ThrowsAsync(new Exception("503 (Service Unavailable)."));
			clientMock.Setup(x => x.GetEmission(It.IsAny<BusinessObject>(),	It.IsAny<ICO2eCalculationSupporter>())).Throws(new Exception("503 (Service Unavailable)."));

			var managerMock = new Mock<CO2eCalculationManager>(clientMock.Object) { CallBase = true };
			managerMock.SetupGet(x => x.Token).Returns(token);
			var manager = managerMock.Object;

			ExceptionReporterTestListener.Instance.Clear();

			var result = getResult.Invoke(shipment, manager);
			shipment = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);

			AssertEquals("Result type should be ServiceUnavailable", CO2eResultType.ServiceUnavailable, result.Type);
			AssertGHGEvent(shipment.Logs, "|RES=Issue communicating with server. Please try again later.|TYP=ServiceUnavailable", 1);
			AssertEquals("No exception should be reported for 503", 0, ExceptionReporterTestListener.Instance.Count);

			ExceptionReporterTestListener.Instance.Clear();
		}

		#endregion

		#region SendApiRequestCore

		public void TestSendApiRequestCoreAsync()
		{
			RunTestSendApiRequestCore(
				(hostSupporter, manager, addressValidationManager) => new CO2eRequestProcessor(manager, addressValidationManager).SendApiRequestCoreAsync(hostSupporter).GetAwaiter().GetResult(),
				(managerMock, addressValidationMock) =>
				{
					managerMock.Verify(manager => manager.GetEmissionAsync(It.IsAny<BusinessObject>(), It.IsAny<ICO2eCalculationSupporter>()), Times.Exactly(3));
					addressValidationMock.Verify(manager => manager.ValidateAsync(It.IsAny<CancellationTokenSource>()), Times.Exactly(3));
				});
		}

		public void TestSendApiRequestCoreSync()
		{
			RunTestSendApiRequestCore(
				(hostSupporter, manager, addressValidationManager) => new CO2eRequestProcessor(manager, addressValidationManager).SendApiRequestCore(hostSupporter),
				(managerMock, addressValidationMock) =>
				{
					managerMock.Verify(manager => manager.GetEmission(It.IsAny<BusinessObject>(), It.IsAny<ICO2eCalculationSupporter>()), Times.Exactly(3));
					addressValidationMock.Verify(manager => manager.Validate(), Times.Exactly(3));
				});
		}

		void RunTestSendApiRequestCore(Func<ICO2eCalculationSupporter, ICO2eCalculationManager, IAddressesValidationManager, CO2eProcessResult> getResult,
			Action<Mock<ICO2eCalculationManager>, Mock<IAddressesValidationManager>> verifyAction)
		{
			// Arrange
			var address1 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var address2 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var address3 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;

			var hostSupporter = Factory.NewMoq<DummyCO2eCalculationSupporter>();
			hostSupporter.Setup(x => x.AddressesToValidate).Returns(new[] { address1 });

			var childSupporter1 = Factory.NewMoq<DummyCO2eCalculationSupporter>();
			childSupporter1.Setup(x => x.AddressesToValidate).Returns(new[] { address2 });

			var childSupporter2 = Factory.NewMoq<DummyCO2eCalculationSupporter>();
			childSupporter2.Setup(x => x.AddressesToValidate).Returns(new[] { address3 });

			hostSupporter.Setup(x => x.AdditionalCalculationSupporters)
				.Returns(new[]
				{
					new AdditionalCalculationSupporter(childSupporter1.Object, () => 0m),
					new AdditionalCalculationSupporter(childSupporter2.Object, () => 0m)
				});

			var calculationManagerMock = new Mock<ICO2eCalculationManager>();
			calculationManagerMock
				.Setup(manager => manager.GetEmissionAsync(It.IsAny<BusinessObject>(), It.IsAny<ICO2eCalculationSupporter>()))
				.Returns(Task.FromResult(new EmissionResult()));
			calculationManagerMock
				.Setup(manager => manager.GetEmission(It.IsAny<BusinessObject>(), It.IsAny<ICO2eCalculationSupporter>()))
				.Returns(new EmissionResult());

			var addressValidationManagerMock = new Mock<IAddressesValidationManager>();
			addressValidationManagerMock
				.Setup(manager => manager.ValidateAsync(It.IsAny<CancellationTokenSource>()))
				.Returns(Task.CompletedTask);
			addressValidationManagerMock.Setup(manager => manager.Validate());

			// Act
			getResult.Invoke(hostSupporter.Object, calculationManagerMock.Object, addressValidationManagerMock.Object);

			// Assert
			verifyAction.Invoke(calculationManagerMock, addressValidationManagerMock);
			Assert(true);
		}

		public void TestSendApiRequestCoreAsync_ChildSupportersAreCurrent()
		{
			RunTestSendApiRequestCore_ChildSupportersAreCurrent(
				(hostSupporter, manager, addressValidationManager) => new CO2eRequestProcessor(manager, addressValidationManager).SendApiRequestCoreAsync(hostSupporter).GetAwaiter().GetResult(),
				(managerMock, addressValidationMock) =>
				{
					managerMock.Verify(manager => manager.GetEmissionAsync(It.IsAny<BusinessObject>(), It.IsAny<ICO2eCalculationSupporter>()), Times.Exactly(1));
					addressValidationMock.Verify(manager => manager.ValidateAsync(It.IsAny<CancellationTokenSource>()), Times.Exactly(1));
				});
		}

		[CaptureMemoryDumpForDisposableLeak]
		public void TestSendApiRequestCoreSync_ChildSupportersAreCurrent()
		{
			DisposableLeakListener.Instance.StackTraceEnabled = true;
			RunTestSendApiRequestCore_ChildSupportersAreCurrent(
				(hostSupporter, manager, addressValidationManager) => new CO2eRequestProcessor(manager, addressValidationManager).SendApiRequestCore(hostSupporter),
				(managerMock, addressValidationMock) =>
				{
					managerMock.Verify(manager => manager.GetEmission(It.IsAny<BusinessObject>(), It.IsAny<ICO2eCalculationSupporter>()), Times.Exactly(1));
					addressValidationMock.Verify(manager => manager.Validate(), Times.Exactly(1));
				});
		}

		void RunTestSendApiRequestCore_ChildSupportersAreCurrent(Func<ICO2eCalculationSupporter, ICO2eCalculationManager, IAddressesValidationManager, CO2eProcessResult> getResult,
			Action<Mock<ICO2eCalculationManager>, Mock<IAddressesValidationManager>> verifyAction)
		{
			// Arrange
			var address1 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var address2 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			var address3 = Factory.NewWithValidTestData<OrgHeader>().MainAddress;

			var hostSupporter = Factory.NewMoq<DummyCO2eCalculationSupporter>();
			hostSupporter.Setup(x => x.AddressesToValidate).Returns(new[] { address1 });

			var childSupporter1 = Factory.NewMoq<DummyCO2eCalculationSupporter>();
			childSupporter1.Object.SetCO2eStatus(CO2eStatusList.Codes.Current);
			childSupporter1.Setup(x => x.AddressesToValidate).Returns(new[] { address2 });

			var childSupporter2 = Factory.NewMoq<DummyCO2eCalculationSupporter>();
			childSupporter2.Object.SetCO2eStatus(CO2eStatusList.Codes.Current);
			childSupporter2.Setup(x => x.AddressesToValidate).Returns(new[] { address3 });

			hostSupporter.Setup(x => x.AdditionalCalculationSupporters)
				.Returns(new[]
				{
					new AdditionalCalculationSupporter(childSupporter1.Object, () => 0m),
					new AdditionalCalculationSupporter(childSupporter2.Object, () => 0m)
				});

			var calculationManagerMock = new Mock<ICO2eCalculationManager>();
			calculationManagerMock
				.Setup(manager => manager.GetEmissionAsync(It.IsAny<BusinessObject>(), It.IsAny<ICO2eCalculationSupporter>()))
				.Returns(Task.FromResult(new EmissionResult()));
			calculationManagerMock
				.Setup(manager => manager.GetEmission(It.IsAny<BusinessObject>(), It.IsAny<ICO2eCalculationSupporter>()))
				.Returns(new EmissionResult());

			var addressValidationManagerMock = new Mock<IAddressesValidationManager>();
			addressValidationManagerMock
				.Setup(manager => manager.ValidateAsync(It.IsAny<CancellationTokenSource>()))
				.Returns(Task.CompletedTask);
			addressValidationManagerMock.Setup(manager => manager.Validate());

			// Act
			getResult.Invoke(hostSupporter.Object, calculationManagerMock.Object, addressValidationManagerMock.Object);

			// Assert
			verifyAction.Invoke(calculationManagerMock, addressValidationManagerMock);
			Assert("Only validate request and send request one for host supporter", true);
		}

		#endregion

		#region GetSupportersToSend

		public void TestGetSupportersToSend_CO2eStatusNotCurrent()
		{
			var hostSupporter = new Mock<ICO2eCalculationSupporter>();
			hostSupporter.Setup(x => x.AdditionalCalculationSupporters).Returns(Array.Empty<AdditionalCalculationSupporter>());
			hostSupporter.Setup(x => x.JobCO2eCollection).Returns(GetMockJobCO2eCollectionWithStatus(CO2eStatusList.Codes.Pending));

			var processor = new CO2eRequestProcessor();
			var supportersToSend = processor.GetSupportersToSend(hostSupporter.Object);

			AssertEquals(1, supportersToSend.Length);
			AssertEquals(hostSupporter.Object, supportersToSend[0]);
		}

		public void TestGetSupportersToSend_CO2eStatusCurrent_UserSelectsYes()
		{
			var childSupporter = new Mock<ICO2eCalculationSupporter>();
			childSupporter.Setup(x => x.JobCO2eCollection).Returns(GetMockJobCO2eCollectionWithStatus(CO2eStatusList.Codes.Pending));
			childSupporter.Setup(x => x.AdditionalCalculationSupporters).Returns(Array.Empty<AdditionalCalculationSupporter>());

			var additionalSupporters = new[]
			{
				new AdditionalCalculationSupporter(childSupporter.Object, () => 0m)
			};

			var hostSupporter = new Mock<ICO2eCalculationSupporter>();
			hostSupporter.Setup(x => x.JobCO2eCollection).Returns(GetMockJobCO2eCollectionWithStatus(CO2eStatusList.Codes.Current));
			hostSupporter.Setup(x => x.AdditionalCalculationSupporters).Returns(additionalSupporters);

			var factory = new BusinessObjectFactory();
			hostSupporter.Setup(x => x.Factory).Returns(factory);

			var recalculationChecker = new Mock<ICO2eRecalculationChecker>();
			recalculationChecker.Setup(x => x.ShouldRecalculate(hostSupporter.Object)).Returns(true);

			factory.SetValue(() => recalculationChecker.Object);
			var processor = new CO2eRequestProcessor();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			var supportersToSend = processor.GetSupportersToSend(hostSupporter.Object);

			AssertEquals(2, supportersToSend.Length);
			AssertEquals(hostSupporter.Object, supportersToSend[0]);
			AssertEquals(childSupporter.Object, supportersToSend[1]);
		}

		public void TestGetSupportersToSend_CO2eStatusCurrent_UserSelectsNo()
		{
			var childSupporter = new Mock<ICO2eCalculationSupporter>();
			childSupporter.Setup(x => x.JobCO2eCollection).Returns(GetMockJobCO2eCollectionWithStatus(CO2eStatusList.Codes.Pending));
			childSupporter.Setup(x => x.AdditionalCalculationSupporters).Returns(Array.Empty<AdditionalCalculationSupporter>());

			var additionalSupporters = new[]
			{
				new AdditionalCalculationSupporter(childSupporter.Object, () => 0m)
			};

			var hostSupporter = new Mock<ICO2eCalculationSupporter>();
			hostSupporter.Setup(x => x.AdditionalCalculationSupporters).Returns(additionalSupporters);
			hostSupporter.Setup(x => x.JobCO2eCollection).Returns(GetMockJobCO2eCollectionWithStatus(CO2eStatusList.Codes.Current));

			var factory = new BusinessObjectFactory();
			hostSupporter.Setup(x => x.Factory).Returns(factory);

			var recalculationChecker = new Mock<ICO2eRecalculationChecker>();
			recalculationChecker.Setup(x => x.ShouldRecalculate(hostSupporter.Object)).Returns(false);

			factory.SetValue(() => recalculationChecker.Object);

			var processor = new CO2eRequestProcessor();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			var supportersToSend = processor.GetSupportersToSend(hostSupporter.Object);
			AssertEquals(0, supportersToSend.Length);
		}

		#endregion

		#region ShouldRecalculate

		public void TestShouldRecalculate_StatusNotCurrent()
		{
			var supporter = new Mock<ICO2eCalculationSupporter>();
			supporter.Setup(x => x.JobCO2eCollection).Returns(GetMockJobCO2eCollectionWithStatus(CO2eStatusList.Codes.Pending));

			var processor = new CO2eRequestProcessor();
			var result = processor.ShouldRecalculate(supporter.Object);
			AssertEquals(true, result);
		}

		public void TestShouldRecalculate_StatusIsCurrent_WithoutCO2eRecalculationChecker()
		{
			var supporter = new Mock<ICO2eCalculationSupporter>();
			supporter.Setup(x => x.Factory).Returns(Factory);
			supporter.Setup(x => x.JobCO2eCollection).Returns(GetMockJobCO2eCollectionWithStatus(CO2eStatusList.Codes.Current));

			var processor = new CO2eRequestProcessor();
			var result = processor.ShouldRecalculate(supporter.Object);
			AssertEquals(false, result);
		}

		public void TestShouldRecalculate_StatusIsCurrent_WithCO2eRecalculationChecker_UserSelectsYes()
		{
			var supporter = new Mock<ICO2eCalculationSupporter>();
			supporter.Setup(x => x.Factory).Returns(Factory);
			supporter.Setup(x => x.JobCO2eCollection).Returns(GetMockJobCO2eCollectionWithStatus(CO2eStatusList.Codes.Current));

			var recalculationChecker = new Mock<ICO2eRecalculationChecker>();
			recalculationChecker.Setup(x => x.ShouldRecalculate(supporter.Object)).Returns(true);

			Factory.SetValue(() => recalculationChecker.Object);

			var processor = new CO2eRequestProcessor();
			var result = processor.ShouldRecalculate(supporter.Object);
			AssertEquals(true, result);
		}

		public void TestShouldRecalculate_StatusIsCurrent_WithICO2eRecalculationChecker_UserSelectsNo()
		{
			var supporter = new Mock<ICO2eCalculationSupporter>();
			supporter.Setup(x => x.Factory).Returns(Factory);
			supporter.Setup(x => x.JobCO2eCollection).Returns(GetMockJobCO2eCollectionWithStatus(CO2eStatusList.Codes.Current));

			var recalculationChecker = new Mock<ICO2eRecalculationChecker>();
			recalculationChecker.Setup(x => x.ShouldRecalculate(supporter.Object)).Returns(false);

			Factory.SetValue(() => recalculationChecker.Object);

			var processor = new CO2eRequestProcessor();
			var result = processor.ShouldRecalculate(supporter.Object);
			AssertEquals(false, result);
		}

		#endregion

		#region SendRequestAsync API Shipment with TBs

		public void TestSendRequestAsync_API_Shipment_TB1_TB2()
		{
			SendRequestAsync_API_ShipmentWithTBs_WithResponsesSpecificOrder((shipmentTask, dtbBookingPICTask, dtbBookingDLVTask) =>
			{
				shipmentTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response));
				dtbBookingPICTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response_TB));
				dtbBookingDLVTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response_TB));
			},
			12000m, 1000m, 1000m,
			CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Current);
		}

		public void TestSendRequestAsync_API_TB1_Shipment_TB2()
		{
			SendRequestAsync_API_ShipmentWithTBs_WithResponsesSpecificOrder((shipmentTask, dtbBookingPICTask, dtbBookingDLVTask) =>
			{
				dtbBookingPICTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response_TB));
				shipmentTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response));
				dtbBookingDLVTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response_TB));
			},
			12000m, 1000m, 1000m,
			CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Current);
		}

		public void TestSendRequestAsync_API_TB1_TB2_Shipment()
		{
			SendRequestAsync_API_ShipmentWithTBs_WithResponsesSpecificOrder((shipmentTask, dtbBookingPICTask, dtbBookingDLVTask) =>
			{
				dtbBookingPICTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response_TB));
				dtbBookingDLVTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response_TB));
				shipmentTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response));
			},
			12000m, 1000m, 1000m,
			CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Current);
		}

		public void TestSendRequestAsync_API_Shipment_TB1_RejectedTB2()
		{
			SendRequestAsync_API_ShipmentWithTBs_WithResponsesSpecificOrder((shipmentTask, dtbBookingPICTask, dtbBookingDLVTask) =>
			{
				shipmentTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response));
				dtbBookingPICTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response_TB));
				dtbBookingDLVTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalEvent_Response_TB));
			},
			11000m, 1000m, 0m,
			CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Rejected);
		}

		public void TestSendRequestAsync_API_Shipment_RejectedTB1_TB2()
		{
			SendRequestAsync_API_ShipmentWithTBs_WithResponsesSpecificOrder((shipmentTask, dtbBookingPICTask, dtbBookingDLVTask) =>
			{
				shipmentTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response));
				dtbBookingPICTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalEvent_Response_TB));
				dtbBookingDLVTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response_TB));
			},
			11000m, 0m, 1000m,
			CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Rejected, CO2eStatusList.Codes.Current);
		}

		public void TestSendRequestAsync_API_Shipment_RejectedTB1_RejectedTB2()
		{
			SendRequestAsync_API_ShipmentWithTBs_WithResponsesSpecificOrder((shipmentTask, dtbBookingPICTask, dtbBookingDLVTask) =>
			{
				shipmentTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response));
				dtbBookingPICTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalEvent_Response_TB));
				dtbBookingDLVTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalEvent_Response_TB));
			},
			10000m, 0m, 0m,
			CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Rejected, CO2eStatusList.Codes.Rejected);
		}

		public void TestSendRequestAsync_API_RejectedTB1_Shipment_TB2()
		{
			SendRequestAsync_API_ShipmentWithTBs_WithResponsesSpecificOrder((shipmentTask, dtbBookingPICTask, dtbBookingDLVTask) =>
			{
				dtbBookingPICTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalEvent_Response_TB));
				shipmentTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response));
				dtbBookingDLVTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response_TB));
			},
			11000m, 0m, 1000m,
			CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Rejected, CO2eStatusList.Codes.Current);
		}

		public void TestSendRequestAsync_API_TB1_Shipment_RejectedTB2()
		{
			SendRequestAsync_API_ShipmentWithTBs_WithResponsesSpecificOrder((shipmentTask, dtbBookingPICTask, dtbBookingDLVTask) =>
			{
				dtbBookingPICTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response_TB));
				shipmentTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response));
				dtbBookingDLVTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalEvent_Response_TB));
			},
			11000m, 1000m, 0m,
			CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Rejected);
		}

		public void TestSendRequestAsync_API_RejectedTB1_Shipment_RejectedTB2()
		{
			SendRequestAsync_API_ShipmentWithTBs_WithResponsesSpecificOrder((shipmentTask, dtbBookingPICTask, dtbBookingDLVTask) =>
			{
				dtbBookingPICTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalEvent_Response_TB));
				shipmentTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response));
				dtbBookingDLVTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalEvent_Response_TB));
			},
			10000m, 0m, 0m,
			CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Rejected, CO2eStatusList.Codes.Rejected);
		}

		public void TestSendRequestAsync_API_RejectedTB1_TB2_Shipment()
		{
			SendRequestAsync_API_ShipmentWithTBs_WithResponsesSpecificOrder((shipmentTask, dtbBookingPICTask, dtbBookingDLVTask) =>
			{
				dtbBookingPICTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalEvent_Response_TB));
				dtbBookingDLVTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response_TB));
				shipmentTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response));
			},
			11000m, 0m, 1000m,
			CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Rejected, CO2eStatusList.Codes.Current);
		}

		public void TestSendRequestAsync_API_TB1_RejectedTB2_Shipment()
		{
			SendRequestAsync_API_ShipmentWithTBs_WithResponsesSpecificOrder((shipmentTask, dtbBookingPICTask, dtbBookingDLVTask) =>
			{
				dtbBookingPICTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response_TB));
				dtbBookingDLVTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalEvent_Response_TB));
				shipmentTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response));
			},
			11000m, 1000m, 0m,
			CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Rejected);
		}

		public void TestSendRequestAsync_API_RejectedTB1_RejectedTB2_Shipment()
		{
			SendRequestAsync_API_ShipmentWithTBs_WithResponsesSpecificOrder((shipmentTask, dtbBookingPICTask, dtbBookingDLVTask) =>
			{
				dtbBookingPICTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalEvent_Response_TB));
				dtbBookingDLVTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalEvent_Response_TB));
				shipmentTask.SetResult(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response));
			},
			10000m, 0m, 0m,
			CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Rejected, CO2eStatusList.Codes.Rejected);
		}

		void SendRequestAsync_API_ShipmentWithTBs_WithResponsesSpecificOrder(
			Action<TaskCompletionSource<IApiResponse<EmissionResult>>, TaskCompletionSource<IApiResponse<EmissionResult>>, TaskCompletionSource<IApiResponse<EmissionResult>>> arrangeResponses,
			decimal expectedShipmentCO2e, decimal expectedDtbBookingPICCO2e, decimal expectedDtbBookingDLVCO2e,
			string expectedShipmentStatus, string expectedDtbBookingPICStatus, string expectedDtbBookingDLVStatus)
		{
			// Arrange
			ObjectFactory.DisposeSubstitutions();
			var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			var dtbBookingPIC = (ICO2eCalculationSupporter)CO2eTestHelper.CreateTransportBooking(shipment, "PIC", Factory);
			var dtbBookingDLV = (ICO2eCalculationSupporter)CO2eTestHelper.CreateTransportBooking(shipment, "DLV", Factory);
			Factory.Save();

			var shipmentTask = new TaskCompletionSource<IApiResponse<EmissionResult>>();
			var dtbBookingPICTask = new TaskCompletionSource<IApiResponse<EmissionResult>>();
			var dtbBookingDLVTask = new TaskCompletionSource<IApiResponse<EmissionResult>>();

			var taskQueue = new Queue<Task<IApiResponse<EmissionResult>>>(new[] { shipmentTask.Task, dtbBookingPICTask.Task, dtbBookingDLVTask.Task });
			var client = new Mock<IApiClient>();
			client.Setup(x => x.PostAsync<EmissionResult>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(taskQueue.Dequeue);

			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest()))
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Api))
			using (ObjectFactory.Substitute("HttpClient", client.Object))
			{
				// Act
				var mainTask = new CO2eRequestProcessor().SendRequestAsync(shipment, new NotificationBuffer());
				arrangeResponses.Invoke(shipmentTask, dtbBookingPICTask, dtbBookingDLVTask);
				mainTask.GetAwaiter().GetResult();

				// Assert
				AssertEquals("Shipment CO2eStatus should be set", expectedShipmentStatus, shipment.GetCO2eStatus());
				AssertEquals("Shipment TotalCO2e should be set with TB emissions", expectedShipmentCO2e, shipment.GetTotalCO2e());
				AssertEquals("Pickup TB CO2eStatus should be set", expectedDtbBookingPICStatus, dtbBookingPIC.GetCO2eStatus());
				AssertEquals("Pickup TB TotalCO2e should be set", expectedDtbBookingPICCO2e, dtbBookingPIC.GetTotalCO2e());
				AssertEquals("Delivery TB CO2eStatus should be set", expectedDtbBookingDLVStatus, dtbBookingDLV.GetCO2eStatus());
				AssertEquals("Delivery TB should be set", expectedDtbBookingDLVCO2e, dtbBookingDLV.GetTotalCO2e());
			}
		}

		#endregion

		#region SendRequest sync API Shipment with TBs

		public void TestSendRequestSync_API_ShipmentWithTBs_Success_Success()
		{
			SendRequestSync_API_ShipmentWithTBs(CO2eTestHelper.CO2eUniversalShipment_Response_TB,
				CO2eTestHelper.CO2eUniversalShipment_Response_TB,
				12000m, 1000m, 1000m,
				CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Current);
		}

		public void TestSendRequestSync_API_ShipmentWithTBs_Rejected_Success()
		{
			SendRequestSync_API_ShipmentWithTBs(CO2eTestHelper.CO2eUniversalEvent_Response_TB,
				CO2eTestHelper.CO2eUniversalShipment_Response_TB,
				11000m, 0m, 1000m,
				CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Rejected, CO2eStatusList.Codes.Current);
		}

		public void TestSendRequestSync_API_ShipmentWithTBs_Success_Rejected()
		{
			SendRequestSync_API_ShipmentWithTBs(CO2eTestHelper.CO2eUniversalShipment_Response_TB,
				CO2eTestHelper.CO2eUniversalEvent_Response_TB,
				11000m, 1000m, 0m,
				CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Rejected);
		}

		public void TestSendRequestSync_API_ShipmentWithTBs_Rejected_Rejected()
		{
			SendRequestSync_API_ShipmentWithTBs(CO2eTestHelper.CO2eUniversalEvent_Response_TB,
				CO2eTestHelper.CO2eUniversalEvent_Response_TB,
				10000m, 0m, 0m,
				CO2eStatusList.Codes.Current, CO2eStatusList.Codes.Rejected, CO2eStatusList.Codes.Rejected);
		}

		void SendRequestSync_API_ShipmentWithTBs(
			string tb1Response, string tb2Response,
			decimal expectedShipmentCO2e, decimal expectedDtbBookingPICCO2e, decimal expectedDtbBookingDLVCO2e,
			string expectedShipmentStatus, string expectedDtbBookingPICStatus, string expectedDtbBookingDLVStatus)
		{
			// Arrange
			ObjectFactory.DisposeSubstitutions();
			var shipment = (ForwardingShipment)CO2eTestHelper.CreateForwardingShipmentWithLegs(Factory);
			var dtbBookingPIC = (ICO2eCalculationSupporter)CO2eTestHelper.CreateTransportBooking(shipment, "PIC", Factory);
			var dtbBookingDLV = (ICO2eCalculationSupporter)CO2eTestHelper.CreateTransportBooking(shipment, "DLV", Factory);
			Factory.Save();

			var client = new Mock<IApiClient>();
			client.SetupSequence(x => x.PostAsync<EmissionResult>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response))
				.ReturnsAsync(CO2eTestHelper.GenerateEmissionResponse(tb1Response))
				.ReturnsAsync(CO2eTestHelper.GenerateEmissionResponse(tb2Response));

			using (FreightDataRegistry.Instance.EnablePrePostCarriageCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Api))
			using (ObjectFactory.Substitute("HttpClient", client.Object))
			{
				// Act
				new CO2eRequestProcessor().SendRequest(shipment, new NotificationBuffer());

				// Assert
				AssertEquals("Shipment CO2eStatus should be set", expectedShipmentStatus, shipment.GetCO2eStatus());
				AssertEquals("Shipment TotalCO2e should be set with TB emissions", expectedShipmentCO2e, shipment.GetTotalCO2e());
				AssertEquals("Pickup TB CO2eStatus should be set", expectedDtbBookingPICStatus, dtbBookingPIC.GetCO2eStatus());
				AssertEquals("Pickup TB TotalCO2e should be set", expectedDtbBookingPICCO2e, dtbBookingPIC.GetTotalCO2e());
				AssertEquals("Delivery TB CO2eStatus should be set", expectedDtbBookingDLVStatus, dtbBookingDLV.GetCO2eStatus());
				AssertEquals("Delivery TB should be set", expectedDtbBookingDLVCO2e, dtbBookingDLV.GetTotalCO2e());
			}
		}

		#endregion

		public void TestProcessResults()
		{
			// Arrange
			var supporter1 = new Mock<ICO2eCalculationSupporter>();
			supporter1.As<IBusiness>().Setup(x => x.HasChanges).Returns(false);
			supporter1.As<IBusiness>().Setup(x => x.HumanReadableName).Returns("First");

			var supporter2 = new Mock<ICO2eCalculationSupporter>();
			supporter2.As<IBusiness>().Setup(x => x.HumanReadableName).Returns("Second");

			var supporter3 = new Mock<ICO2eCalculationSupporter>();
			supporter3.As<IBusiness>().Setup(x => x.HumanReadableName).Returns("Third");

			var success = CO2eProcessResult.ApiSuccess(null);
			var error = CO2eProcessResult.ApiError(null);
			var fail = CO2eProcessResult.ApiFail("something went wrong.");
			var unauthorized = CO2eProcessResult.Unauthorized;

			// Act & Assert: only 1 result
			var result = CO2eRequestProcessor.ProcessResults(new[] { (success, supporter1.Object) });
			AssertEquals(success, result);
			result = CO2eRequestProcessor.ProcessResults(new[] { (error, supporter1.Object) });
			AssertEquals(error, result);
			result = CO2eRequestProcessor.ProcessResults(new[] { (fail, supporter1.Object) });
			AssertEquals(fail, result);
			result = CO2eRequestProcessor.ProcessResults(new[] { (unauthorized, supporter1.Object) });
			AssertEquals(unauthorized, result);

			// Act & Assert: 3 success
			result = CO2eRequestProcessor.ProcessResults(new[]
			{
				(success, supporter1.Object), (success, supporter2.Object), (success, supporter3.Object)
			});
			AssertEquals(CO2eResultType.ApiSuccess, result.Type);

			// Act & Assert: 1 unauthorized
			result = CO2eRequestProcessor.ProcessResults(new[]
			{
				(success, supporter1.Object), (unauthorized, supporter2.Object), (success, supporter3.Object)
			});
			AssertEquals(CO2eResultType.Unauthorized, result.Type);

			// Act & Assert: 3 error
			result = CO2eRequestProcessor.ProcessResults(new[]
			{
				(error, supporter1.Object), (error, supporter2.Object), (error, supporter3.Object)
			});
			AssertEquals(CO2eResultType.ApiFail, result.Type);
			AssertMultilineASCIIEquals(@"
First - Error requesting greenhouse gas emissions calculation service.
Second - Error requesting greenhouse gas emissions calculation service.
Third - Error requesting greenhouse gas emissions calculation service.", result.Message);

			// Act & Assert: 3 fail
			result = CO2eRequestProcessor.ProcessResults(new[]
			{
				(fail, supporter1.Object), (fail, supporter2.Object), (fail, supporter3.Object)
			});
			AssertEquals(CO2eResultType.ApiFail, result.Type);
			AssertMultilineASCIIEquals(@"
First - Error requesting greenhouse gas emissions calculation: something went wrong.
Second - Error requesting greenhouse gas emissions calculation: something went wrong.
Third - Error requesting greenhouse gas emissions calculation: something went wrong.", result.Message);

			// Act & Assert: 1 success, 1 fail, 1 error
			result = CO2eRequestProcessor.ProcessResults(new[]
			{
				(success, supporter1.Object), (fail, supporter2.Object), (error, supporter3.Object)
			});
			AssertEquals(CO2eResultType.ApiFail, result.Type);
			AssertMultilineASCIIEquals(@"
Second - Error requesting greenhouse gas emissions calculation: something went wrong.
Third - Error requesting greenhouse gas emissions calculation service.", result.Message);
		}

		#region Helpers

		IJobCO2eCollection GetMockJobCO2eCollectionWithStatus(ZString status)
		{
			var jobCO2e = new Mock<IJobCO2e>();
			jobCO2e.Setup(x => x.Status).Returns(status);

			var jobCO2eCollection = new Mock<IJobCO2eCollection>();
			jobCO2eCollection.Setup(x => x.Get(It.IsAny<ZString>())).Returns(jobCO2e.Object);

			return jobCO2eCollection.Object;
		}

		#endregion

		#region Implementation

		Func<IApiResponse<EmissionResult>> ResponseGenerator { get; set; }

		IDisposable registryDisposable;

		protected override void SetUp()
		{
			base.SetUp();

			var client = new Mock<IApiClient>();
			client.Setup(x => x.PostAsync<EmissionResult>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() => ResponseGenerator());

			ObjectFactory.Substitute("HttpClient", client.Object);

			registryDisposable = FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Api);
		}

		protected override void TearDown()
		{
			base.TearDown();
			ObjectFactory.DisposeSubstitutions();
			registryDisposable.Dispose();
		}

		void AssertGHGEvent(Logs logs, string reference, int count = 2)
		{
			AssertEquals("New GHG event created", count, logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GreenhouseGasEmissionsCalculationCode).Count());
			var transportGHGEvent = logs.MostRecentLogByEventTime(AutoEvents.GreenhouseGasEmissionsCalculation);
			AssertEquals("GHG event Reference", reference, transportGHGEvent.SL_Reference);
		}

		void AssertDataImportEvent(Logs logs, EmissionResult result)
		{
			AssertEquals($"New Data Import event created", 1, logs.Find(x => x.SL_SE_NKEvent == AutoEvents.DataImportCode).Count());
			var log = logs.MostRecentLogByEventTime(AutoEvents.DataImport);

			var message = log.RelatedEDIMessage.Message;

			Assert(message.IsInDatabase);
			AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
			AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
			AssertEquals("message.EM_ReceiveTransmit", EDICommunicationsModeCommsDirectionList.Codes.Receive, message.EM_ReceiveTransmit);
			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertXMLEquals(message.EM_MessageText, result.UXmlString);

			var interchange = message.Interchange;

			AssertNotNull(interchange);
			Assert(interchange.IsInDatabase);
			AssertEquals("EDIEDIDAT", interchange.EI_To);
			AssertEquals("EMISSION_CALCULATOR", interchange.EI_From);
			AssertXMLEquals(interchange.EI_BodyText, result.InterchangeString);
		}

		void AssertDataExportEvent(Logs logs, EmissionResult result)
		{
			AssertEquals($"New Data Export event created", 1, logs.Find(x => x.SL_SE_NKEvent == AutoEvents.DataExportCode).Count());
			var log = logs.MostRecentLogByEventTime(AutoEvents.DataExport);

			var message = log.RelatedEDIMessage.Message;

			Assert(message.IsInDatabase);
			AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
			AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
			AssertEquals("message.EM_ReceiveTransmit", EDICommunicationsModeCommsDirectionList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
			AssertXMLEquals(message.EM_MessageText, result.Request.UXmlString);

			AssertWorkflowSection(result.Request);

			var interchange = message.Interchange;

			AssertNotNull(interchange);
			Assert(interchange.IsInDatabase);
			AssertEquals("EMISSION_CALCULATOR", interchange.EI_To);
			AssertEquals("EDIEDIDAT", interchange.EI_From);
			AssertXMLEquals(interchange.EI_BodyText, result.Request.InterchangeString);
		}

		void AssertWorkflowSection(EmissionRequest request)
		{
			var workFlowSection = $@"
      <Workflow>
        <Company>
          <Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
          <Country Name=""{GlbCompany.CurrentCompany.Country.Description}"">{GlbCompany.CurrentCompany.Country.Code}</Country>
          <Name>{GlbCompany.CurrentCompany.CompanyName}</Name>
        </Company>
        <EventBranch Name=""{GlbBranch.CurrentBranch.GB_BranchName}"">{GlbBranch.CurrentBranch.GB_Code}</EventBranch>
        <EventDepartment Name=""{GlbDepartment.CurrentDepartment.GE_Desc}"">{GlbDepartment.CurrentDepartment.GE_Code}</EventDepartment>
        <EventReference>User Action</EventReference>
        <EventUser Name=""{GlbStaff.CurrentUser.GS_FullName}"">{GlbStaff.CurrentUser.GS_Code}</EventUser>
        <TriggerCount>0</TriggerCount>
        <TriggerDescription>Calculate: CO2e Greenhouse Gas Emission</TriggerDescription>
        <TriggerType>Manual</TriggerType>
      </Workflow>";

			AssertXMLContains(
				workFlowSection,
				request.UXmlString
			);
		}

		void AssertIntercangeRejectedEvent(Logs logs, EmissionResult result)
		{
			AssertEquals($"New Data Export event created", 1, logs.Find(x => x.SL_SE_NKEvent == AutoEvents.InterchangeRejectedCode).Count());
			var log = logs.MostRecentLogByEventTime(AutoEvents.InterchangeRejected);

			var message = log.RelatedEDIMessage.Message;

			Assert(message.IsInDatabase);
			AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
			AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, message.EM_MessageSubType);
			AssertEquals("message.EM_ReceiveTransmit", EDICommunicationsModeCommsDirectionList.Codes.Receive, message.EM_ReceiveTransmit);
			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertXMLEquals(message.EM_MessageText, result.UXmlString);

			var interchange = message.Interchange;

			AssertNotNull(interchange);
			Assert(interchange.IsInDatabase);
			AssertEquals("EDIEDIDAT", interchange.EI_To);
			AssertEquals("EMISSION_CALCULATOR", interchange.EI_From);
			AssertXMLEquals(interchange.EI_BodyText, result.InterchangeString);
		}

		#endregion
	}
}
