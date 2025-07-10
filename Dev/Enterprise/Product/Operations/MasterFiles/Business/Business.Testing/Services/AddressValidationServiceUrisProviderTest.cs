using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Registry.Business.Testing;
using Moq;
using Newtonsoft.Json;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AddressValidationServiceUrisProviderTest : TestCaseWithFactory
	{
		public void TestDisableAvsFeature()
		{
			ErrorReporter.Clear();
			var mockFeatureControlManager = new Mock<IFeatureControlManager>();
			mockFeatureControlManager
				.Setup(x => x.GetFeatureDataAsync("MDMAVSURL", CancellationToken.None))
				.Returns(Task.FromResult(default(IFeatureData)));
			using (SetTemporaryValueForAddressValidationWebServiceURIs())
			using (ObjectFactory.Substitute(mockFeatureControlManager.Object))
			{
				var serviceUris = AddressValidationServiceUrisProvider.GetAddressValidationServiceUris();

				TestFallbackToRegistry(serviceUris);
			}
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestEnableAvsFeature()
		{
			ErrorReporter.Clear();
			var mockFeatureData = new Mock<IFeatureData>();
			mockFeatureData
				.Setup(x => x.DeserializeParameterAsJson<AddressValidationServiceUris>())
				.Returns(new AddressValidationServiceUris
				{
					Primary = new AddressValidationServiceUri { Uri = "http://dummy.feature.net/primary/", EnableS2STAuth = false },
					Secondary = new AddressValidationServiceUri { Uri = "http://dummy.feature.net/secondary/", EnableS2STAuth = true },
					Background = new AddressValidationServiceUri { Uri = "http://dummy.feature.net/background/", EnableS2STAuth = true },
				});
			var mockFeatureControlManager = new Mock<IFeatureControlManager>();
			mockFeatureControlManager
				.Setup(x => x.GetFeatureDataAsync("MDMAVSURL", CancellationToken.None))
				.Returns(Task.FromResult(mockFeatureData.Object));
			using (SetTemporaryValueForAddressValidationWebServiceURIs())
			using (ObjectFactory.Substitute(mockFeatureControlManager.Object))
			{
				var serviceUris = AddressValidationServiceUrisProvider.GetAddressValidationServiceUris();

				AssertEquals("http://dummy.feature.net/primary/", serviceUris.Primary.Uri);
				AssertEquals(false, serviceUris.Primary.EnableS2STAuth);
				AssertEquals("http://dummy.feature.net/secondary/", serviceUris.Secondary.Uri);
				AssertEquals(true, serviceUris.Secondary.EnableS2STAuth);
				AssertEquals("http://dummy.feature.net/background/", serviceUris.Background.Uri);
				AssertEquals(true, serviceUris.Background.EnableS2STAuth);
			}
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestAvsFeature_ThrowUnexpectedException()
		{
			var mockFeatureData = new Mock<IFeatureData>();
			mockFeatureData
				.Setup(x => x.DeserializeParameterAsJson<AddressValidationServiceUris>())
				.Throws(new Exception("TestAvsFeature_ThrowUnexpectedException"));
			var mockFeatureControlManager = new Mock<IFeatureControlManager>();
			mockFeatureControlManager
				.Setup(x => x.GetFeatureDataAsync("MDMAVSURL", CancellationToken.None))
				.Returns(Task.FromResult(mockFeatureData.Object));
			using (SetTemporaryValueForAddressValidationWebServiceURIs())
			using (ObjectFactory.Substitute(mockFeatureControlManager.Object))
			{
				var exception = AssertExceptionThrown<Exception>(() => AddressValidationServiceUrisProvider.GetAddressValidationServiceUris());
				AssertEquals("TestAvsFeature_ThrowUnexpectedException", exception.Message);
			}
		}

		public void TestAvsFeature_ThrowJsonException()
		{
			ErrorReporter.Clear();
			var mockFeatureData = new Mock<IFeatureData>();
			mockFeatureData
				.Setup(x => x.DeserializeParameterAsJson<AddressValidationServiceUris>())
				.Throws(new JsonException("TestAvsFeature_ThrowJsonException"));
			var mockFeatureControlManager = new Mock<IFeatureControlManager>();
			mockFeatureControlManager
				.Setup(x => x.GetFeatureDataAsync("MDMAVSURL", CancellationToken.None))
				.Returns(Task.FromResult(mockFeatureData.Object));
			using (SetTemporaryValueForAddressValidationWebServiceURIs())
			using (ObjectFactory.Substitute(mockFeatureControlManager.Object))
			{
				var serviceUris = AddressValidationServiceUrisProvider.GetAddressValidationServiceUris();

				TestFallbackToRegistry(serviceUris);
			}
			AssertEquals("GetAddressValidationServiceUris", ErrorReporter.LastKeyReported);
			AssertEquals("TestAvsFeature_ThrowJsonException", ErrorReporter.LastMessageReported);
			AssertEquals("TestAvsFeature_ThrowJsonException", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestAvsFeature_NoParameter()
		{
			ErrorReporter.Clear();
			var mockFeatureData = new Mock<IFeatureData>();
			mockFeatureData
				.Setup(x => x.DeserializeParameterAsJson<AddressValidationServiceUris>())
				.Returns(default(AddressValidationServiceUris));
			var mockFeatureControlManager = new Mock<IFeatureControlManager>();
			mockFeatureControlManager
				.Setup(x => x.GetFeatureDataAsync("MDMAVSURL", CancellationToken.None))
				.Returns(Task.FromResult(mockFeatureData.Object));
			using (SetTemporaryValueForAddressValidationWebServiceURIs())
			using (ObjectFactory.Substitute(mockFeatureControlManager.Object))
			{
				var serviceUris = AddressValidationServiceUrisProvider.GetAddressValidationServiceUris();

				TestFallbackToRegistry(serviceUris);
			}
			AssertEquals("GetAddressValidationServiceUris", ErrorReporter.LastKeyReported);
			AssertEquals("Feature data [MDMAVSURL] requires a valid parameter!", ErrorReporter.LastMessageReported);
			AssertEquals("Feature data [MDMAVSURL] requires a valid parameter!", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestAvsFeature_InvalidParameter()
		{
			ErrorReporter.Clear();
			var mockFeatureData = new Mock<IFeatureData>();
			mockFeatureData
				.Setup(x => x.DeserializeParameterAsJson<AddressValidationServiceUris>())
				.Returns(new AddressValidationServiceUris
				{
					Primary = default(AddressValidationServiceUri),
					Secondary = new AddressValidationServiceUri { Uri = "http://dummy.feature.net/secondary", EnableS2STAuth = false },
					Background = new AddressValidationServiceUri { Uri = "http://dummy.feature.net/background/", EnableS2STAuth = true },
				});
			var mockFeatureControlManager = new Mock<IFeatureControlManager>();
			mockFeatureControlManager
				.Setup(x => x.GetFeatureDataAsync("MDMAVSURL", CancellationToken.None))
				.Returns(Task.FromResult(mockFeatureData.Object));
			using (SetTemporaryValueForAddressValidationWebServiceURIs())
			using (ObjectFactory.Substitute(mockFeatureControlManager.Object))
			{
				var serviceUris = AddressValidationServiceUrisProvider.GetAddressValidationServiceUris();

				TestFallbackToRegistry(serviceUris);
			}
			AssertEquals("GetAddressValidationServiceUris", ErrorReporter.LastKeyReported);
			AssertEquals("Feature data [MDMAVSURL] requires a valid parameter!", ErrorReporter.LastMessageReported);
			AssertEquals("Feature data [MDMAVSURL] requires a valid parameter!", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestAvsFeature_InvalidUriFormat()
		{
			ErrorReporter.Clear();
			var mockFeatureData = new Mock<IFeatureData>();
			mockFeatureData
				.Setup(x => x.DeserializeParameterAsJson<AddressValidationServiceUris>())
				.Returns(new AddressValidationServiceUris
				{
					Primary = new AddressValidationServiceUri { Uri = "http://dummy.feature.net/primary/", EnableS2STAuth = false },
					Secondary = new AddressValidationServiceUri { Uri = "http://dummy.feature.net/secondary", EnableS2STAuth = false },
					Background = new AddressValidationServiceUri { Uri = "http://dummy.feature.net/background/", EnableS2STAuth = true },
				});
			var mockFeatureControlManager = new Mock<IFeatureControlManager>();
			mockFeatureControlManager
				.Setup(x => x.GetFeatureDataAsync("MDMAVSURL", CancellationToken.None))
				.Returns(Task.FromResult(mockFeatureData.Object));
			using (SetTemporaryValueForAddressValidationWebServiceURIs())
			using (ObjectFactory.Substitute(mockFeatureControlManager.Object))
			{
				var serviceUris = AddressValidationServiceUrisProvider.GetAddressValidationServiceUris();

				TestFallbackToRegistry(serviceUris);
			}
			AssertEquals("GetAddressValidationServiceUris", ErrorReporter.LastKeyReported);
			AssertEquals("One or more AVS URIs from the feature data [MDMAVSURL] is invalid!", ErrorReporter.LastMessageReported);
			AssertEquals("One or more AVS URIs from the feature data [MDMAVSURL] is invalid!", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestAvsFeature_InvalidUri()
		{
			ErrorReporter.Clear();
			var mockFeatureData = new Mock<IFeatureData>();
			mockFeatureData
				.Setup(x => x.DeserializeParameterAsJson<AddressValidationServiceUris>())
				.Returns(new AddressValidationServiceUris
				{
					Primary = new AddressValidationServiceUri { Uri = "http://dummy.feature.net/primary/", EnableS2STAuth = false },
					Secondary = new AddressValidationServiceUri { Uri = "http://dummy.feature.net/secondary/", EnableS2STAuth = false },
					Background = new AddressValidationServiceUri { Uri = "ABAABA", EnableS2STAuth = true },
				});
			var mockFeatureControlManager = new Mock<IFeatureControlManager>();
			mockFeatureControlManager
				.Setup(x => x.GetFeatureDataAsync("MDMAVSURL", CancellationToken.None))
				.Returns(Task.FromResult(mockFeatureData.Object));
			using (SetTemporaryValueForAddressValidationWebServiceURIs())
			using (ObjectFactory.Substitute(mockFeatureControlManager.Object))
			{
				var serviceUris = AddressValidationServiceUrisProvider.GetAddressValidationServiceUris();

				TestFallbackToRegistry(serviceUris);
			}
			AssertEquals("GetAddressValidationServiceUris", ErrorReporter.LastKeyReported);
			AssertEquals("One or more AVS URIs from the feature data [MDMAVSURL] is invalid!", ErrorReporter.LastMessageReported);
			AssertEquals("One or more AVS URIs from the feature data [MDMAVSURL] is invalid!", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		IDisposable SetTemporaryValueForAddressValidationWebServiceURIs()
		{
			return OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value =>
			{
				value.Primary.ServiceUri = "http://dummy.registry.net/primary/";
				value.Primary.EnableSystemToSystemTrustAuthentication = true;
				value.Secondary.ServiceUri = "http://dummy.registry.net/secondary/";
				value.Secondary.EnableSystemToSystemTrustAuthentication = false;
				value.Background.ServiceUri = "http://dummy.registry.net/background/";
				value.Background.EnableSystemToSystemTrustAuthentication = false;
			}, Factory);
		}

		void TestFallbackToRegistry(AddressValidationServiceUris serviceUris)
		{
			AssertEquals("http://dummy.registry.net/primary/", serviceUris.Primary.Uri);
			AssertEquals(true, serviceUris.Primary.EnableS2STAuth);
			AssertEquals("http://dummy.registry.net/secondary/", serviceUris.Secondary.Uri);
			AssertEquals(false, serviceUris.Secondary.EnableS2STAuth);
			AssertEquals("http://dummy.registry.net/background/", serviceUris.Background.Uri);
			AssertEquals(false, serviceUris.Background.EnableS2STAuth);
		}
	}
}
