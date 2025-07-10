using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AddressValidationServiceTest : TestCaseWithFactory
	{
		void AssertHasNoWebServiceError(bool expectedResult, AddressCleansingResultItem result)
		{
			var message = string.Format("HasAddressCleansingResultItem: {0}, ResultStatusCode: {1}",
				result.ValidationResultItem != null, result.ValidationResultItem?.ResultStatusCode);
			AssertEquals(message, expectedResult, AddressValidationService.HasNoWebServiceError(result));
		}

		public void TestIfHasWebServiceError()
		{
			var result = new AddressCleansingResultItem();
			AssertHasNoWebServiceError(false, result);

			result.ValidationResultItem = new ValidationResultItem
			{
				ResultStatusCode = ValidationResultStatusCode.StreetClose
			};
			AssertHasNoWebServiceError(true, result);

			result.ValidationResultItem.ResultStatusCode = ValidationResultStatusCode.Error;
			AssertHasNoWebServiceError(false, result);
		}

		public void TestSetSuggestedAddressNormalized()
		{
			var query = new ZQuery(RefCountryStatesSchema.RW_Code, "95")
				.AddToFilter(RefCountryStatesSchema.RW_Description, "Val-d'Oise")
				.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, "FR");

			if (!Factory.Exists(typeof(RefCountryStates), query))
			{
				var refCountryStates = Factory.NewWithValidTestData<RefCountryStates>();
				refCountryStates.RW_Code = "95";
				refCountryStates.RW_Description = "Val-d'Oise";
				refCountryStates.RW_RN_NKCountryCode = "FR";
			}

			var suggestedAddress = new ValidationResultItem() { State = "VAL-D’OISE" };
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_RN_NKCountryCode = "FR";
			AssertEquals("", orgAddress.State);

			AddressValidationService.UpdateSuggestedState(suggestedAddress, orgAddress);
			AssertEquals("Val-d'Oise", orgAddress.State);
		}

		public void TestSetSuggestedAddressToAddressForValidation_WhenAddressIsOrgAddress_ShouldUpdateClosestPort()
		{
			var suggestedAddress = new ValidationResultItem() { Latitude = 30.66, Longitude = 104.07 };
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

			orgAddress.OA_RN_NKCountryCode = "CN";
			orgAddress.ClosestPort = string.Empty;

			using (OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.EnableDefaultingClosestPortOnAddressValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AddressValidationService.SetSuggestedAddressToAddressForValidation(suggestedAddress, false, orgAddress);
				AssertEquals("The AddressForValidation is an OrgAddress. The ClosestPort should have been set.", "CNCDU", orgAddress.ClosestPort);
			}
		}

		public void TestSetSuggestedAddressToAddressForValidation_WhenAddressIsASubClassOfOrgAddress_ShouldUpdateClosestPort()
		{
			var suggestedAddress = new ValidationResultItem() { Latitude = 30.66, Longitude = 104.07 };
			var ediOrgAddress = Factory.NewWithValidTestData<OrgAddressSubClassForTest>();

			ediOrgAddress.OA_RN_NKCountryCode = "CN";
			ediOrgAddress.ClosestPort = string.Empty;

			AssertEquals("Precondition: ", true, ediOrgAddress.GetType().IsSubclassOf(typeof(OrgAddress)));

			using (OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.EnableDefaultingClosestPortOnAddressValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AddressValidationService.SetSuggestedAddressToAddressForValidation(suggestedAddress, false, ediOrgAddress);
				AssertEquals("The AddressForValidation is a subclass of OrgAddress. The ClosestPort should have been set.", "CNCDU", ediOrgAddress.ClosestPort);
			}
		}

		public void TestSetSuggestedAddressToAddressForValidation_WhenAddressIsNotSubClassOfOrgAddress_ShouldNotUpdateClosestPort()
		{
			var suggestedAddress = new ValidationResultItem() { Latitude = 30.66, Longitude = 104.07 };
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();

			jobDocAddress.E2_RN_NKCountryCode = "CN";
			jobDocAddress.ClosestPort = string.Empty;

			AssertEquals("Precondition: ", false, jobDocAddress.GetType().IsSubclassOf(typeof(OrgAddress)));

			using (OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.EnableDefaultingClosestPortOnAddressValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AddressValidationService.SetSuggestedAddressToAddressForValidation(suggestedAddress, false, jobDocAddress);
				AssertEquals("The AddressForValidation is not an OrgAddress or a subclass of OrgAddress. The ClosestPort should not have been set.", string.Empty, jobDocAddress.ClosestPort);
			}
		}

		public void TestIsAddressInValidStatus()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.ValidationStatus = AddressValidationStatus.Verified;
			Assert(AddressValidationService.IsAddressInValidStatus(address));
			address.ValidationStatus = AddressValidationStatus.VerifiedToStreet;
			Assert(AddressValidationService.IsAddressInValidStatus(address));
			address.ValidationStatus = AddressValidationStatus.ManuallyVerified;
			Assert(AddressValidationService.IsAddressInValidStatus(address));
			address.ValidationStatus = AddressValidationStatus.ToBeVerified;
			Assert(!AddressValidationService.IsAddressInValidStatus(address));
			address.ValidationStatus = AddressValidationStatus.ExcludeBackgroundValidation;
			Assert(!AddressValidationService.IsAddressInValidStatus(address));
			address.ValidationStatus = AddressValidationStatus.CountryNotAvailable;
			Assert(AddressValidationService.IsAddressInValidStatus(address));
			address.ValidationStatus = AddressValidationStatus.Invalid;
			Assert(!AddressValidationService.IsAddressInValidStatus(address));
			address.ValidationStatus = AddressValidationStatus.Unverifiable;
			Assert(!AddressValidationService.IsAddressInValidStatus(address));
			address.ValidationStatus = AddressValidationStatus.NotRequired;
			Assert(!AddressValidationService.IsAddressInValidStatus(address));
		}

		public void TestIsAddressInValidStatusWithNullAddress()
		{
			Assert(AddressValidationService.IsAddressInValidStatus(null));
		}

		public void TestIsAddressNeedValidation()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.ValidationStatus = AddressValidationStatus.Verified;
			Assert(!AddressValidationService.IsAddressNeedValidation(address));
			address.ValidationStatus = AddressValidationStatus.VerifiedToStreet;
			Assert(!AddressValidationService.IsAddressNeedValidation(address));
			address.ValidationStatus = AddressValidationStatus.ManuallyVerified;
			Assert(AddressValidationService.IsAddressNeedValidation(address));
			address.ValidationStatus = AddressValidationStatus.ToBeVerified;
			Assert(AddressValidationService.IsAddressNeedValidation(address));
			address.ValidationStatus = AddressValidationStatus.ExcludeBackgroundValidation;
			Assert(AddressValidationService.IsAddressNeedValidation(address));
			address.ValidationStatus = AddressValidationStatus.CountryNotAvailable;
			Assert(!AddressValidationService.IsAddressNeedValidation(address));
			address.ValidationStatus = AddressValidationStatus.Invalid;
			Assert(AddressValidationService.IsAddressNeedValidation(address));
			address.ValidationStatus = AddressValidationStatus.Unverifiable;
			Assert(AddressValidationService.IsAddressNeedValidation(address));
			address.ValidationStatus = AddressValidationStatus.NotRequired;
			Assert(!AddressValidationService.IsAddressNeedValidation(address));
		}

		public void TestIsAddressNeedValidationWithNullAddress()
		{
			Assert(!AddressValidationService.IsAddressNeedValidation(null));
		}

		public void TestAddressValidationServiceUriFailSafe()
		{
			AddressValidationService.SetAvailableWebServiceAddress();
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value =>
			{
				value.Primary.ServiceUri = string.Format("http://localhost:{0}/addresscleansing/v1/", HttpServiceForTest.GetFreeTcpPort());
				value.Secondary.ServiceUri = string.Format("http://localhost:{0}/addresscleansing/v2/", HttpServiceForTest.GetFreeTcpPort());
			}, Factory))
			{
				Assert("No service available", string.IsNullOrEmpty(AddressValidationService.GetAvailableWebServiceAddressAsync().Result.Uri));

				// Arrange
				var serviceUri = new Uri(OrganisationsDataRegistry.Instance.AvsWebServiceURIs.Secondary.ServiceUri + AddressValidationService.Constants.CheckServiceStatusName + "/");
				var testExternalValidationService = new HttpServiceForTest
				{
					Delay = 0,
					Methods = new string[] { "GET" },
					Processor = (_, request) => new Tuple<int, string>(200, @"{""ServiceName"":""WiseTechGlobal.AddressCleansing.Service"",""ServiceVersion"":2,""ServiceAvailable"":""true"",""AssemblyVersion"":""1.0.5767.30967"",""ActiveProviders"":[""PBO""]}"),
					Uri = serviceUri,
					ContentType = "application/json"
				};

				try
				{
					testExternalValidationService.Start();
					Assert(String.Format("The test external validation service cannot be started on {0}.", serviceUri.AbsoluteUri), testExternalValidationService.IsStarted);
					AddressValidationService.GetAvailableWebServiceAddressAsync();
					// Assert
					Assert("Service available", !string.IsNullOrEmpty(AddressValidationService.GetAvailableWebServiceAddressAsync().Result.Uri));
					AssertEquals(AddressValidationService.GetAvailableWebServiceAddressAsync().Result.Uri, OrganisationsDataRegistry.Instance.AvsWebServiceURIs.Secondary.ServiceUri);
				}
				finally
				{
					if (testExternalValidationService.IsStarted)
					{
						testExternalValidationService.Stop();
					}
					AddressValidationService.SetAvailableWebServiceAddress();
				}
			}
		}

		public void TestAvailableWebServiceAddress_WhenGettingAvailableEndpoint_ShouldNotUseCachedValue()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;
			AddressValidationService.SetAvailableWebServiceAddress();

			var primaryPort = HttpServiceForTest.GetFreeTcpPort();
			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value =>
			{
				value.Primary.ServiceUri = $"http://localhost:{primaryPort}/addresscleansing/v1/";
				value.Primary.EnableSystemToSystemTrustAuthentication = true;
			}, Factory))
			{
				var primaryTestService = new HttpServiceForTest
				{
					Delay = 0,
					Methods = new[] { "GET" },
					Processor = (_, request) => new Tuple<int, string>(200, @"{""ServiceName"":""WiseTechGlobal.AddressCleansing.Service"",""ServiceVersion"":2,""ServiceAvailable"":""true"",""AssemblyVersion"":""1.0.5767.30967"",""ActiveProviders"":[""PBO""]}"),
					Uri = new Uri(OrganisationsDataRegistry.Instance.AvsWebServiceURIs.Primary.ServiceUri + AddressValidationService.Constants.CheckServiceStatusName + "/"),
					ContentType = "application/json"
				};

				var secondaryTestService = default(HttpServiceForTest);

				try
				{
					primaryTestService.Start();

					Assert(
						"The test external validation service cannot be started on PRIMARY.",
						primaryTestService.IsStarted);

					AssertEquals("PRECONDITION: Should connect to primary endpoint",
						OrganisationsDataRegistry.Instance.AvsWebServiceURIs.Primary.ServiceUri,
						AddressValidationService.GetAvailableWebServiceAddressAsync().Result.Uri);

					AssertEquals("primary endpoint enables System to System Trust Authentication",
						true,
						OrganisationsDataRegistry.Instance.AvsWebServiceURIs.Primary.EnableSystemToSystemTrustAuthentication);

					primaryTestService.Stop();

					Assert(
						"The test external validation service cannot be stopped on PRIMARY.",
						!primaryTestService.IsStarted);

					var secondaryPort = HttpServiceForTest.GetFreeTcpPort();
					using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value =>
					{
						value.Secondary.ServiceUri = $"http://localhost:{secondaryPort}/addresscleansing/v2/";
						value.Secondary.EnableSystemToSystemTrustAuthentication = false;
					}, Factory))
					{
						secondaryTestService = new HttpServiceForTest
						{
							Delay = 0,
							Methods = new[] { "GET" },
							Processor = (_, request) => new Tuple<int, string>(200, @"{""ServiceName"":""WiseTechGlobal.AddressCleansing.Service"",""ServiceVersion"":2,""ServiceAvailable"":""true"",""AssemblyVersion"":""1.0.5767.30967"",""ActiveProviders"":[""PBO""]}"),
							Uri = new Uri(OrganisationsDataRegistry.Instance.AvsWebServiceURIs.Secondary.ServiceUri + AddressValidationService.Constants.CheckServiceStatusName + "/"),
							ContentType = "application/json"
						};

						secondaryTestService.Start();

						Assert(
							"The test external validation service cannot be started on SECONDARY.",
							secondaryTestService.IsStarted);

						// Act.

						AddressValidationService.SetAvailableWebServiceAddress();
						var finalUri = AddressValidationService.GetAvailableWebServiceAddressAsync().Result.Uri;

						// Assert.

						AssertEquals("Should connect to secondary endpoint",
							OrganisationsDataRegistry.Instance.AvsWebServiceURIs.Secondary.ServiceUri,
							finalUri);

						AssertEquals("secondary endpoint does not enable System to System Trust Authentication",
							false,
							OrganisationsDataRegistry.Instance.AvsWebServiceURIs.Secondary.EnableSystemToSystemTrustAuthentication);
					}
				}
				finally
				{
					primaryTestService.Stop();
					secondaryTestService?.Stop();

					AddressValidationService.SetAvailableWebServiceAddress();
				}
			}
		}

		public void TestValidateAddressAsyncOnUIThread()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var cancellationToken = new CancellationTokenSource();
			Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU")).RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.Address1 = "72 O'Riordan St";
			address.Address2 = "";
			address.City = "Alexandria";
			address.State = "NSW";
			address.Postcode = "2015";
			address.OA_RN_NKCountryCode = "AU";
			AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);

			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			// Arrange
			AddressValidationService.SetAvailableWebServiceAddress(string.Format("http://localhost:{0}/addresscleansing/v2/", HttpServiceForTest.GetFreeTcpPort()));
			var serviceUri = new Uri(AddressValidationService.GetAvailableWebServiceAddressAsync().Result.Uri + AddressValidationService.Constants.ValidationServiceName + "/");
			var testExternalValidationService = new HttpServiceForTest
			{
				Delay = 0,
				Methods = new string[] { "POST" },
				Processor = (_, request) => new Tuple<int, string>(200, @"{""Items"":[{""ValidationResultItem"":{""AddressRecordGUID"":""b681b902-685a-4afc-85e2-c312c0b29f4b"",""AddressSourceTable"":""E2"",""Addressee"":null,""Address1"":""72 O'RIORDAN STREET"",""Address2"":null,""City"":""ALEXANDRIA"",""County"":null,""State"":""NSW"",""Postcode"":""2015"",""PostcodeAddOn"":null,""Country"":""AU"",""Apartment"":null,""StreetNumber"":""72"",""Street"":""O'RIORDAN STREET"",""Latitude"":-33.916565139125929,""Longitude"":151.19541258,""MatchCode"":""S8HPNTSCZG"",""MatchCodeFlag"":4106,""ChangeCount"":0,""LocationPrecision"":16,""ResultStatusCode"":""PET"",""ServiceType"":1,""QueryType"":1,""AvailableData"":3,""AddressType"":8,""ErrorMessage"":null},""ProviderServiceCalls"":[],""Suggestions"":[]}]}"),
				Uri = serviceUri,
				ContentType = "application/json"
			};
			try
			{
				testExternalValidationService.Start();
				Assert(String.Format("The test external validation service cannot be started on {0}.", serviceUri.AbsoluteUri), testExternalValidationService.IsStarted);
				// Act
				var result = AddressValidationService.ValidateAddressAsync(address, cancellationToken).Result;
				// Assert
				AssertNotNull(result);
				AssertEquals(AddressValidationStatus.Verified, address.ValidationStatus);
			}
			finally
			{
				if (testExternalValidationService.IsStarted)
				{
					testExternalValidationService.Stop();
				}
				AddressValidationService.SetAvailableWebServiceAddress();
			}
		}

		public void TestValidateAddressSyncOnUIThread()
		{
			var cancellationToken = new CancellationTokenSource();
			Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU")).RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.Address1 = "72 O'Riordan St";
			address.Address2 = "";
			address.City = "Alexandria";
			address.State = "NSW";
			address.Postcode = "2015";
			address.OA_RN_NKCountryCode = "AU";
			AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);

			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			// Arrange
			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value => value.Background.ServiceUri = string.Format("http://localhost:{0}/addresscleansing/v2/", HttpServiceForTest.GetFreeTcpPort()), Factory))
			{
				var serviceUri = new Uri(AddressValidationService.AvailableBackgroundWebServiceAddress.Uri + AddressValidationService.Constants.ValidationServiceName + "/");
				var testExternalValidationService = new HttpServiceForTest
				{
					Delay = 0,
					Methods = new string[] { "POST" },
					Processor = (_, request) => new Tuple<int, string>(200, @"{""Items"":[{""ValidationResultItem"":{""AddressRecordGUID"":""b681b902-685a-4afc-85e2-c312c0b29f4b"",""AddressSourceTable"":""E2"",""Addressee"":null,""Address1"":""72 O'RIORDAN STREET"",""Address2"":null,""City"":""ALEXANDRIA"",""County"":null,""State"":""NSW"",""Postcode"":""2015"",""PostcodeAddOn"":null,""Country"":""AU"",""Apartment"":null,""StreetNumber"":""72"",""Street"":""O'RIORDAN STREET"",""Latitude"":-33.916565139125929,""Longitude"":151.19541258,""MatchCode"":""S8HPNTSCZG"",""MatchCodeFlag"":4106,""ChangeCount"":0,""LocationPrecision"":16,""ResultStatusCode"":""PET"",""ServiceType"":1,""QueryType"":1,""AvailableData"":3,""AddressType"":8,""ErrorMessage"":null},""ProviderServiceCalls"":[],""Suggestions"":[]}]}"),
					Uri = serviceUri,
					ContentType = "application/json"
				};
				try
				{
					testExternalValidationService.Start();
					Assert(String.Format("The test external validation service cannot be started on {0}.", serviceUri.AbsoluteUri), testExternalValidationService.IsStarted);
					// Act
					var result = AddressValidationService.ValidateAddressViaBackgroundEndpoint(address, cancellationToken);
					// Assert
					AssertNotNull(result);
					AssertEquals(AddressValidationStatus.Verified, address.ValidationStatus);
				}
				finally
				{
					if (testExternalValidationService.IsStarted)
					{
						testExternalValidationService.Stop();
					}
				}
			}
		}

		public void TestValidateAddressSyncOnBackgroundThread()
		{
			var cancellationToken = new CancellationTokenSource();
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			// Arrange

			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value => value.Background.ServiceUri = string.Format("http://localhost:{0}/addresscleansing/v2/", HttpServiceForTest.GetFreeTcpPort()), Factory))
			{
				var serviceUri = new Uri(AddressValidationService.AvailableBackgroundWebServiceAddress.Uri + AddressValidationService.Constants.ValidationServiceName + "/");
				var testExternalValidationService = new HttpServiceForTest
				{
					Delay = 0,
					Methods = new string[] { "POST" },
					Processor = (_, request) => new Tuple<int, string>(200, @"{""Items"":[{""ValidationResultItem"":{""AddressRecordGUID"":""b681b902-685a-4afc-85e2-c312c0b29f4b"",""AddressSourceTable"":""E2"",""Addressee"":null,""Address1"":""72 O'RIORDAN STREET"",""Address2"":null,""City"":""ALEXANDRIA"",""County"":null,""State"":""NSW"",""Postcode"":""2015"",""PostcodeAddOn"":null,""Country"":""AU"",""Apartment"":null,""StreetNumber"":""72"",""Street"":""O'RIORDAN STREET"",""Latitude"":-33.916565139125929,""Longitude"":151.19541258,""MatchCode"":""S8HPNTSCZG"",""MatchCodeFlag"":4106,""ChangeCount"":0,""LocationPrecision"":16,""ResultStatusCode"":""PET"",""ServiceType"":1,""QueryType"":1,""AvailableData"":3,""AddressType"":8,""ErrorMessage"":null},""ProviderServiceCalls"":[],""Suggestions"":[]}]}"),
					Uri = serviceUri,
					ContentType = "application/json"
				};
				try
				{
					testExternalValidationService.Start();
					Assert(String.Format("The test external validation service cannot be started on {0}.", serviceUri.AbsoluteUri), testExternalValidationService.IsStarted);
					// Act

					WebAddressValidationResult result = null;
					OrgAddress address = null;
					var thread = new Thread(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							var factory = new BusinessObjectFactory();
							factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU")).RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;
							address = factory.NewWithValidTestData<OrgAddress>();
							address.Address1 = "72 O'Riordan St";
							address.Address2 = "";
							address.City = "Alexandria";
							address.State = "NSW";
							address.Postcode = "2015";
							address.OA_RN_NKCountryCode = "AU";
							AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);

							result = AddressValidationService.ValidateAddressViaBackgroundEndpoint(address, cancellationToken);
						}
					});
					thread.Start();
					thread.Join();

					// Assert
					AssertNotNull(result);
					AssertEquals(AddressValidationStatus.Verified, address.ValidationStatus);
				}
				finally
				{
					if (testExternalValidationService.IsStarted)
					{
						testExternalValidationService.Stop();
					}
				}
			}
		}

		public void TestRequestAndResponseValues()
		{
			var address = (ISupportWebAddressValidation)Factory.NewWithValidTestData<OrgAddress>();
			address.Address1 = "_ADDRESS_1_";
			address.Address2 = "_ADDRESS_2_";
			address.City = "_CITY_";
			address.CompanyName = "_COMPANY_NAME_";
			address.Postcode = "_59200_";
			address.State = "_STATE_";
			address.CountryCodeISO2 = "AU";

			Env.Registry.EnableAddressValidationWebService = true;

			OrganisationsDataRegistry.Instance
				.DisabledAddressValidationCountries
				.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value => value.Background.ServiceUri = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}/addresscleansing/v2/", Factory))
			{
				AddressItem requestedAddress = null;

				var testService = new HttpServiceForTest
				{
					Delay = 0,
					Methods = new[] { "POST" },
					Processor = (url, request) =>
					{
						var requestData = Newtonsoft.Json.JsonConvert.DeserializeObject<ValidationRequest>(request);
						requestedAddress = requestData?.Items[0];

						return new Tuple<int, string>(200, @"{""Items"":[{""ValidationResultItem"":{""AddressRecordGUID"":""f4cfee42-72c1-4d9a-a74f-452924357cd7"",""AddressSourceTable"":""TS"",""Addressee"":null,""Address1"":""[_MOCK_ADDRESS_1_]"",""Address2"":""[_MOCK_ADDRESS_2_]"",""City"":""[_MOCK_CITY_]"",""Locality"":""2"",""County"":""[_MOCK_COUNTY_]"",""State"":""[_MOCK_STATE_]"",""Postcode"":""[_MOCK_POSTCODE_]"",""PostcodeAddOn"":null,""PostcodeBase"":"""",""Country"":""[_MOCK_COUNTRY_]"",""CompanyName"": ""[_MOCK_COMPANY_NAME_]"", ""Apartment"":null,""UnparsedAddressInformation"":null,""UnmatchedApartmentPrefix"":null,""UnmatchedApartmentSuffix"":null,""StreetNumber"":""42"",""Street"":""[_MOCK_STREET_]"",""Latitude"":-80.0,""Longitude"":80.0,""MatchCode"":""S8HPNTSCZAS"",""MatchCodeFlag"":4106,""ChangeCount"":0,""LocationPrecision"":16,""ResultStatusCode"":""PET"",""ServiceType"":1,""QueryType"":1,""AvailableData"":2,""AddressType"":8,""ErrorMessage"":null,""Group"":null},""ProviderServiceCalls"":[],""Suggestions"":[],""ValidateStartDateTimeUTC"":""2017-04-28T01:47:06.2480561Z"",""ValidateFinishDateTimeUTC"":""2017-04-28T01:47:07.2949386Z"",""SuggestStartDateTimeUTC"":""2017-04-28T01:47:07.2949386Z"",""SuggestFinishDateTimeUTC"":""2017-04-28T01:47:07.2949386Z""}]}");
					},
					Uri = new Uri(AddressValidationService.AvailableBackgroundWebServiceAddress.Uri + AddressValidationService.Constants.ValidationServiceName + "/"),
					ContentType = "application/json"
				};

				testService.Start();
				try
				{
					var cancellationToken = new CancellationTokenSource();
					var result = AddressValidationService.ValidateAddressViaBackgroundEndpoint(address, cancellationToken);

					AssertNotNull(result);
					AssertNotNull(result.ResultAddress);
					var suggestedAddress = result.ResultAddress;

					AssertEquals("[_MOCK_ADDRESS_1_]", suggestedAddress.Address1);
					AssertEquals("[_MOCK_ADDRESS_2_]", suggestedAddress.Address2);
					AssertEquals("[_MOCK_CITY_]", suggestedAddress.City);
					AssertEquals("[_MOCK_COUNTY_]", suggestedAddress.County);
					AssertEquals("[_MOCK_STATE_]", suggestedAddress.State);
					AssertEquals("[_MOCK_POSTCODE_]", suggestedAddress.Postcode);
					AssertEquals("[_MOCK_COUNTRY_]", suggestedAddress.Country);
					AssertEquals("S8HPNTSCZAS", suggestedAddress.MatchCode);
					AssertEquals("42", suggestedAddress.StreetNumber);
					AssertEquals("[_MOCK_STREET_]", suggestedAddress.Street);
					AssertEquals(-80d, suggestedAddress.Latitude);
					AssertEquals(80d, suggestedAddress.Longitude);
					AssertEquals("[_MOCK_COMPANY_NAME_]", suggestedAddress.CompanyName);

					AssertNotNull(requestedAddress);
					AssertEquals("_ADDRESS_1_", requestedAddress.Address1);
					AssertEquals("_ADDRESS_2_", requestedAddress.Address2);
					AssertEquals("_CITY_", requestedAddress.City);
					AssertEquals("_STATE_", requestedAddress.State);
					AssertEquals("_59200_", requestedAddress.Postcode);
					AssertEquals("AU", requestedAddress.CountryCode);
					AssertEquals("_COMPANY_NAME_", requestedAddress.CompanyName);
				}
				finally
				{
					if (testService.IsStarted)
					{
						testService.Stop();
					}
				}
			}
		}

		public void TestValidateAddressViaBackgroundEndpoint_WhenQueryingService_ShouldUseQuickValidateAction()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();

			Env.Registry.EnableAddressValidationWebService = true;

			OrganisationsDataRegistry.Instance
				.DisabledAddressValidationCountries
				.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value => value.Background.ServiceUri = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}/addresscleansing/v2/", Factory))
			{
				var cleanseAction = CleanseAction.ReverseGeocode;

				var testService = new HttpServiceForTest
				{
					Delay = 0,
					Methods = new[] { "POST" },
					Processor = (url, _) =>
					{
						cleanseAction = url.ExtractCleanseAction();
						return new Tuple<int, string>(200, @"{""Items"":[{""ValidationResultItem"":{""AddressRecordGUID"":""f4cfee42-72c1-4d9a-a74f-452924357cd7"",""AddressSourceTable"":""TS"",""Addressee"":null,""Address1"":""[_MOCK_ADDRESS_1_]"",""Address2"":""[_MOCK_ADDRESS_2_]"",""City"":""[_MOCK_CITY_]"",""Locality"":""2"",""County"":"""",""State"":""NSW"",""Postcode"":""2000"",""PostcodeAddOn"":null,""PostcodeBase"":"""",""Country"":""AU"",""Apartment"":null,""UnparsedAddressInformation"":null,""UnmatchedApartmentPrefix"":null,""UnmatchedApartmentSuffix"":null,""StreetNumber"":""42"",""Street"":"""",""Latitude"":0.0,""Longitude"":0.0,""MatchCode"":""S8HPNTSCZAS"",""MatchCodeFlag"":4106,""ChangeCount"":0,""LocationPrecision"":16,""ResultStatusCode"":""PET"",""ServiceType"":1,""QueryType"":1,""AvailableData"":2,""AddressType"":8,""ErrorMessage"":null,""Group"":null},""ProviderServiceCalls"":[],""Suggestions"":[],""ValidateStartDateTimeUTC"":""2017-04-28T01:47:06.2480561Z"",""ValidateFinishDateTimeUTC"":""2017-04-28T01:47:07.2949386Z"",""SuggestStartDateTimeUTC"":""2017-04-28T01:47:07.2949386Z"",""SuggestFinishDateTimeUTC"":""2017-04-28T01:47:07.2949386Z""}]}");
					},
					Uri = new Uri(AddressValidationService.AvailableBackgroundWebServiceAddress.Uri + AddressValidationService.Constants.ValidationServiceName + "/"),
					ContentType = "application/json"
				};

				var cancellationToken = new CancellationTokenSource();

				testService.Start();

				try
				{
					// Act.

					AddressValidationService.ValidateAddressViaBackgroundEndpoint(address, cancellationToken);

					// Assert.

					AssertEquals(CleanseAction.QuickValidate, cleanseAction);
				}
				finally
				{
					if (testService.IsStarted)
					{
						testService.Stop();
					}
				}
			}
		}

		public void TestValidateAddress_WhenGettingUnparsedInformation_ShouldIncludeThemInAdditionalAddressInformation()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();

			Env.Registry.EnableAddressValidationWebService = true;

			OrganisationsDataRegistry.Instance
				.DisabledAddressValidationCountries
				.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value => value.Background.ServiceUri = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}/addresscleansing/v2/", Factory))
			{
				var testService = new HttpServiceForTest
				{
					Delay = 0,
					Methods = new[] { "POST" },
					Processor = (_, request) => new Tuple<int, string>(200, @"{""Items"":[{""ValidationResultItem"":{""AddressRecordGUID"":""f4cfee42-72c1-4d9a-a74f-452924357cd7"",""AddressSourceTable"":""TS"",""Addressee"":null,""Address1"":""[_MOCK_ADDRESS_1_]"",""Address2"":""[_MOCK_ADDRESS_2_]"",""City"":""[_MOCK_CITY_]"",""Locality"":""2"",""County"":"""",""State"":""NSW"",""Postcode"":""2000"",""PostcodeAddOn"":null,""PostcodeBase"":"""",""Country"":""AU"",""Apartment"":null,""UnparsedAddressInformation"":""[_MOCK_UNPARSED_ADDRESS_INFORMATION_]"",""UnmatchedApartmentPrefix"":null,""UnmatchedApartmentSuffix"":null,""StreetNumber"":""42"",""Street"":"""",""Latitude"":0.0,""Longitude"":0.0,""MatchCode"":""S8HPNTSCZAS"",""MatchCodeFlag"":4106,""ChangeCount"":0,""LocationPrecision"":16,""ResultStatusCode"":""PET"",""ServiceType"":1,""QueryType"":1,""AvailableData"":2,""AddressType"":8,""ErrorMessage"":null,""Group"":null},""ProviderServiceCalls"":[],""Suggestions"":[],""ValidateStartDateTimeUTC"":""2017-04-28T01:47:06.2480561Z"",""ValidateFinishDateTimeUTC"":""2017-04-28T01:47:07.2949386Z"",""SuggestStartDateTimeUTC"":""2017-04-28T01:47:07.2949386Z"",""SuggestFinishDateTimeUTC"":""2017-04-28T01:47:07.2949386Z""}]}"),
					Uri = new Uri(AddressValidationService.AvailableBackgroundWebServiceAddress.Uri + AddressValidationService.Constants.ValidationServiceName + "/"),
					ContentType = "application/json"
				};

				var cancellationToken = new CancellationTokenSource();

				testService.Start();

				try
				{
					// Act.

					var result = AddressValidationService.ValidateAddressViaBackgroundEndpoint(address, cancellationToken);

					// Assert.

					AssertNotNull(result);
					AssertEquals("[_MOCK_UNPARSED_ADDRESS_INFORMATION_]", address.UnrestrictedAdditionalAddressInformation);
				}
				finally
				{
					if (testService.IsStarted)
					{
						testService.Stop();
					}
				}
			}
		}

		public void TestValidateAddress_WhenGettingUnparsedInformationAndAdditionalInformationExist_ShouldIncludeAppendIt()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.PrimaryOrgAddressAdditionalInfoDetail = "[_MOCK_EXISTING_ADDRESS_INFORMATION_]";

			Env.Registry.EnableAddressValidationWebService = true;

			OrganisationsDataRegistry.Instance
				.DisabledAddressValidationCountries
				.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value => value.Background.ServiceUri = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}/addresscleansing/v2/", Factory))
			{
				var testService = new HttpServiceForTest
				{
					Delay = 0,
					Methods = new[] { "POST" },
					Processor = (_, request) => new Tuple<int, string>(200, @"{""Items"":[{""ValidationResultItem"":{""AddressRecordGUID"":""f4cfee42-72c1-4d9a-a74f-452924357cd7"",""AddressSourceTable"":""TS"",""Addressee"":null,""Address1"":""[_MOCK_ADDRESS_1_]"",""Address2"":""[_MOCK_ADDRESS_2_]"",""City"":""[_MOCK_CITY_]"",""Locality"":""2"",""County"":"""",""State"":""NSW"",""Postcode"":""2000"",""PostcodeAddOn"":null,""PostcodeBase"":"""",""Country"":""AU"",""Apartment"":null,""UnparsedAddressInformation"":""[_MOCK_UNPARSED_ADDRESS_INFORMATION_]"",""UnmatchedApartmentPrefix"":null,""UnmatchedApartmentSuffix"":null,""StreetNumber"":""42"",""Street"":"""",""Latitude"":0.0,""Longitude"":0.0,""MatchCode"":""S8HPNTSCZAS"",""MatchCodeFlag"":4106,""ChangeCount"":0,""LocationPrecision"":16,""ResultStatusCode"":""PET"",""ServiceType"":1,""QueryType"":1,""AvailableData"":2,""AddressType"":8,""ErrorMessage"":null,""Group"":null},""ProviderServiceCalls"":[],""Suggestions"":[],""ValidateStartDateTimeUTC"":""2017-04-28T01:47:06.2480561Z"",""ValidateFinishDateTimeUTC"":""2017-04-28T01:47:07.2949386Z"",""SuggestStartDateTimeUTC"":""2017-04-28T01:47:07.2949386Z"",""SuggestFinishDateTimeUTC"":""2017-04-28T01:47:07.2949386Z""}]}"),
					Uri = new Uri(AddressValidationService.AvailableBackgroundWebServiceAddress.Uri + AddressValidationService.Constants.ValidationServiceName + "/"),
					ContentType = "application/json"
				};

				var cancellationToken = new CancellationTokenSource();

				testService.Start();

				try
				{
					// Act.

					var result = AddressValidationService.ValidateAddressViaBackgroundEndpoint(address, cancellationToken);

					// Assert.

					AssertNotNull(result);
					AssertEquals("[_MOCK_EXISTING_ADDRESS_INFORMATION_], [_MOCK_UNPARSED_ADDRESS_INFORMATION_]", address.UnrestrictedAdditionalAddressInformation);
				}
				finally
				{
					if (testService.IsStarted)
					{
						testService.Stop();
					}
				}
			}
		}

		public void TestValidateAddress_WhenGettingUnparsedInformationAndSameAdditionalInformationExist_ShouldIgnoreThem()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			var addressInfo = Factory.New<OrgAddressAdditionalInfo>();
			addressInfo.OAI_OA_Address = address.PK;
			addressInfo.OAI_IsPrimary = true;
			addressInfo.OAI_AdditionalInfo = "[_MOCK_UNPARSED_ADDRESS_INFORMATION_]";

			Env.Registry.EnableAddressValidationWebService = true;

			OrganisationsDataRegistry.Instance
				.DisabledAddressValidationCountries
				.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value => value.Background.ServiceUri = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}/addresscleansing/v2/", Factory))
			{
				var testService = new HttpServiceForTest
				{
					Delay = 0,
					Methods = new[] { "POST" },
					Processor = (_, request) => new Tuple<int, string>(200, @"{""Items"":[{""ValidationResultItem"":{""AddressRecordGUID"":""f4cfee42-72c1-4d9a-a74f-452924357cd7"",""AddressSourceTable"":""TS"",""Addressee"":null,""Address1"":""[_MOCK_ADDRESS_1_]"",""Address2"":""[_MOCK_ADDRESS_2_]"",""City"":""[_MOCK_CITY_]"",""Locality"":""2"",""County"":"""",""State"":""NSW"",""Postcode"":""2000"",""PostcodeAddOn"":null,""PostcodeBase"":"""",""Country"":""AU"",""Apartment"":null,""UnparsedAddressInformation"":""[_MOCK_UNPARSED_ADDRESS_INFORMATION_]"",""UnmatchedApartmentPrefix"":null,""UnmatchedApartmentSuffix"":null,""StreetNumber"":""42"",""Street"":"""",""Latitude"":0.0,""Longitude"":0.0,""MatchCode"":""S8HPNTSCZAS"",""MatchCodeFlag"":4106,""ChangeCount"":0,""LocationPrecision"":16,""ResultStatusCode"":""PET"",""ServiceType"":1,""QueryType"":1,""AvailableData"":2,""AddressType"":8,""ErrorMessage"":null,""Group"":null},""ProviderServiceCalls"":[],""Suggestions"":[],""ValidateStartDateTimeUTC"":""2017-04-28T01:47:06.2480561Z"",""ValidateFinishDateTimeUTC"":""2017-04-28T01:47:07.2949386Z"",""SuggestStartDateTimeUTC"":""2017-04-28T01:47:07.2949386Z"",""SuggestFinishDateTimeUTC"":""2017-04-28T01:47:07.2949386Z""}]}"),
					Uri = new Uri(AddressValidationService.AvailableBackgroundWebServiceAddress.Uri + AddressValidationService.Constants.ValidationServiceName + "/"),
					ContentType = "application/json"
				};

				var cancellationToken = new CancellationTokenSource();

				testService.Start();

				try
				{
					// Act.

					var result = AddressValidationService.ValidateAddressViaBackgroundEndpoint(address, cancellationToken);

					// Assert.

					AssertNotNull(result);
					AssertEquals("[_MOCK_UNPARSED_ADDRESS_INFORMATION_]", address.UnrestrictedAdditionalAddressInformation);
				}
				finally
				{
					if (testService.IsStarted)
					{
						testService.Stop();
					}
				}
			}
		}

		public void TestValidateAddress_WhenGettingNoUnparsedInformationAndAdditionalInformationExist_ShouldKeepExistingOne()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.PrimaryOrgAddressAdditionalInfoDetail = "[_MOCK_EXISTING_ADDRESS_INFORMATION_]";

			Env.Registry.EnableAddressValidationWebService = true;

			OrganisationsDataRegistry.Instance
				.DisabledAddressValidationCountries
				.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value => value.Background.ServiceUri = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}/addresscleansing/v2/", Factory))
			{
				var testService = new HttpServiceForTest
				{
					Delay = 0,
					Methods = new[] { "POST" },
					Processor = (_, request) => new Tuple<int, string>(200, @"{""Items"":[{""ValidationResultItem"":{""AddressRecordGUID"":""f4cfee42-72c1-4d9a-a74f-452924357cd7"",""AddressSourceTable"":""TS"",""Addressee"":null,""Address1"":""[_MOCK_ADDRESS_1_]"",""Address2"":""[_MOCK_ADDRESS_2_]"",""City"":""[_MOCK_CITY_]"",""Locality"":""2"",""County"":"""",""State"":""NSW"",""Postcode"":""2000"",""PostcodeAddOn"":null,""PostcodeBase"":"""",""Country"":""AU"",""Apartment"":null,""UnparsedAddressInformation"":null,""UnmatchedApartmentPrefix"":null,""UnmatchedApartmentSuffix"":null,""StreetNumber"":""42"",""Street"":"""",""Latitude"":0.0,""Longitude"":0.0,""MatchCode"":""S8HPNTSCZAS"",""MatchCodeFlag"":4106,""ChangeCount"":0,""LocationPrecision"":16,""ResultStatusCode"":""PET"",""ServiceType"":1,""QueryType"":1,""AvailableData"":2,""AddressType"":8,""ErrorMessage"":null,""Group"":null},""ProviderServiceCalls"":[],""Suggestions"":[],""ValidateStartDateTimeUTC"":""2017-04-28T01:47:06.2480561Z"",""ValidateFinishDateTimeUTC"":""2017-04-28T01:47:07.2949386Z"",""SuggestStartDateTimeUTC"":""2017-04-28T01:47:07.2949386Z"",""SuggestFinishDateTimeUTC"":""2017-04-28T01:47:07.2949386Z""}]}"),
					Uri = new Uri(AddressValidationService.AvailableBackgroundWebServiceAddress.Uri + AddressValidationService.Constants.ValidationServiceName + "/"),
					ContentType = "application/json"
				};

				var cancellationToken = new CancellationTokenSource();

				testService.Start();

				try
				{
					// Act.

					var result = AddressValidationService.ValidateAddressViaBackgroundEndpoint(address, cancellationToken);

					// Assert.

					AssertNotNull(result);
					AssertEquals("[_MOCK_EXISTING_ADDRESS_INFORMATION_]", address.UnrestrictedAdditionalAddressInformation);
				}
				finally
				{
					if (testService.IsStarted)
					{
						testService.Stop();
					}
				}
			}
		}

		public void TestValidateAddress_WhenUnparsedInformationContainingNonUsefulTerm_ShouldExcludeThem()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();

			Env.Registry.EnableAddressValidationWebService = true;

			OrganisationsDataRegistry.Instance
				.DisabledAddressValidationCountries
				.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value => value.Background.ServiceUri = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}/addresscleansing/v2/", Factory))
			{
				var testService = new HttpServiceForTest
				{
					Delay = 0,
					Methods = new[] { "POST" },
					Processor = (_, request) => new Tuple<int, string>(200, @"{""Items"":[{""ValidationResultItem"":{""AddressRecordGUID"":""f4cfee42-72c1-4d9a-a74f-452924357cd7"",""AddressSourceTable"":""TS"",""Addressee"":null,""Address1"":""[_MOCK_ADDRESS_1_]"",""Address2"":""[_MOCK_ADDRESS_2_]"",""City"":""[_MOCK_CITY_]"",""Locality"":""2"",""County"":"""",""State"":""NSW"",""Postcode"":""2000"",""PostcodeAddOn"":null,""PostcodeBase"":"""",""Country"":""AU"",""Apartment"":null,""UnparsedAddressInformation"":""1-2 BANANA BUILDING ---"",""UnmatchedApartmentPrefix"":null,""UnmatchedApartmentSuffix"":null,""StreetNumber"":""42"",""Street"":"""",""Latitude"":0.0,""Longitude"":0.0,""MatchCode"":""S8HPNTSCZAS"",""MatchCodeFlag"":4106,""ChangeCount"":0,""LocationPrecision"":16,""ResultStatusCode"":""PET"",""ServiceType"":1,""QueryType"":1,""AvailableData"":2,""AddressType"":8,""ErrorMessage"":null,""Group"":null},""ProviderServiceCalls"":[],""Suggestions"":[],""ValidateStartDateTimeUTC"":""2017-04-28T01:47:06.2480561Z"",""ValidateFinishDateTimeUTC"":""2017-04-28T01:47:07.2949386Z"",""SuggestStartDateTimeUTC"":""2017-04-28T01:47:07.2949386Z"",""SuggestFinishDateTimeUTC"":""2017-04-28T01:47:07.2949386Z""}]}"),
					Uri = new Uri(AddressValidationService.AvailableBackgroundWebServiceAddress.Uri + AddressValidationService.Constants.ValidationServiceName + "/"),
					ContentType = "application/json"
				};

				var cancellationToken = new CancellationTokenSource();

				testService.Start();

				try
				{
					// Act.

					var result = AddressValidationService.ValidateAddressViaBackgroundEndpoint(address, cancellationToken);

					// Assert.

					AssertNotNull(result);
					AssertEquals("1-2 BANANA BUILDING", address.UnrestrictedAdditionalAddressInformation);
				}
				finally
				{
					if (testService.IsStarted)
					{
						testService.Stop();
					}
				}
			}
		}

		public void TestIsExactPointFoundShouldBeTrueWhenAddressValidationReturnPointExact()
		{
			var cancellationToken = new CancellationTokenSource();
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			// Arrange

			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value => value.Background.ServiceUri = string.Format("http://localhost:{0}/addresscleansing/v2/", HttpServiceForTest.GetFreeTcpPort()), Factory))
			{
				var serviceUri = new Uri(AddressValidationService.AvailableBackgroundWebServiceAddress.Uri + AddressValidationService.Constants.ValidationServiceName + "/");
				var testExternalValidationService = new HttpServiceForTest
				{
					Delay = 0,
					Methods = new string[] { "POST" },
					Processor = (_, request) => new Tuple<int, string>(200, @"{""Items"":[{""ValidationResultItem"":{""AddressRecordGUID"":""b681b902-685a-4afc-85e2-c312c0b29f4b"",""AddressSourceTable"":""E2"",""Addressee"":null,""Address1"":""72 O'RIORDAN STREET"",""Address2"":null,""City"":""ALEXANDRIA"",""County"":null,""State"":""NSW"",""Postcode"":""2015"",""PostcodeAddOn"":null,""Country"":""AU"",""Apartment"":null,""StreetNumber"":""72"",""Street"":""O'RIORDAN STREET"",""Latitude"":-33.916565139125929,""Longitude"":151.19541258,""MatchCode"":""S8HPNTSCZG"",""MatchCodeFlag"":4106,""ChangeCount"":0,""LocationPrecision"":16,""ResultStatusCode"":""PET"",""ServiceType"":1,""QueryType"":1,""AvailableData"":3,""AddressType"":8,""ErrorMessage"":null},""ProviderServiceCalls"":[],""Suggestions"":[]}]}"),
					Uri = serviceUri,
					ContentType = "application/json"
				};
				try
				{
					testExternalValidationService.Start();
					Assert(String.Format("The test external validation service cannot be started on {0}.", serviceUri.AbsoluteUri), testExternalValidationService.IsStarted);
					// Act

					WebAddressValidationResult result = null;
					OrgAddress address = null;
					var thread = new Thread(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							var factory = new BusinessObjectFactory();
							factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU")).RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;
							address = factory.NewWithValidTestData<OrgAddress>();
							address.Address1 = "72 O'Riordan St";
							address.Address2 = "";
							address.City = "Alexandria";
							address.State = "NSW";
							address.Postcode = "2015";
							address.OA_RN_NKCountryCode = "AU";
							AssertEquals("Precondition", false, address.IsExactPointFound);
							AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);

							result = AddressValidationService.ValidateAddressViaBackgroundEndpoint(address, cancellationToken);
							AssertEquals(result.ResultAddress.ResultStatusCode, ValidationResultStatusCode.PointExact);
							AssertEquals(true, address.IsExactPointFound);
						}
					});
					thread.Start();
					thread.Join();

					// Assert
					AssertNotNull(result);
					AssertEquals(AddressValidationStatus.Verified, address.ValidationStatus);
				}
				finally
				{
					if (testExternalValidationService.IsStarted)
					{
						testExternalValidationService.Stop();
					}
				}

				var testExternalValidationService1 = new HttpServiceForTest
				{
					Delay = 0,
					Methods = new string[] { "POST" },
					Processor = (_, request) => new Tuple<int, string>(200, @"{""Items"":[{""ValidationResultItem"":{""AddressRecordGUID"":""b681b902-685a-4afc-85e2-c312c0b29f4b"",""AddressSourceTable"":""E2"",""Addressee"":null,""Address1"":""72 O'RIORDAN STREET"",""Address2"":null,""City"":""ALEXANDRIA"",""County"":null,""State"":""NSW"",""Postcode"":""2015"",""PostcodeAddOn"":null,""Country"":""AU"",""Apartment"":null,""StreetNumber"":""72"",""Street"":""O'RIORDAN STREET"",""Latitude"":-33.916565139125929,""Longitude"":151.19541258,""MatchCode"":""S8HPNTSCZG"",""MatchCodeFlag"":4106,""ChangeCount"":0,""LocationPrecision"":16,""ResultStatusCode"":""CCL"",""ServiceType"":1,""QueryType"":1,""AvailableData"":3,""AddressType"":8,""ErrorMessage"":null},""ProviderServiceCalls"":[],""Suggestions"":[]}]}"),
					Uri = serviceUri,
					ContentType = "application/json"
				};
				try
				{
					testExternalValidationService1.Start();
					Assert(String.Format("The test external validation service cannot be started on {0}.", serviceUri.AbsoluteUri), testExternalValidationService1.IsStarted);
					// Act

					WebAddressValidationResult result = null;
					OrgAddress address = null;
					var thread = new Thread(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							var factory = new BusinessObjectFactory();
							factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU")).RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;
							address = factory.NewWithValidTestData<OrgAddress>();
							address.Address1 = "72 O'Riordan St";
							address.Address2 = "";
							address.City = "Unknown";
							address.State = "NSW";
							address.Postcode = "2015";
							address.OA_RN_NKCountryCode = "AU";
							AssertEquals("Precondition", false, address.IsExactPointFound);
							AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);

							result = AddressValidationService.ValidateAddressViaBackgroundEndpoint(address, cancellationToken);
							AssertEquals(result.ResultAddress.ResultStatusCode, ValidationResultStatusCode.CityClose);
							AssertEquals(false, address.IsExactPointFound);
						}
					});
					thread.Start();
					thread.Join();

					// Assert
					AssertNotNull(result);
					AssertEquals(AddressValidationStatus.Invalid, address.ValidationStatus);
				}
				finally
				{
					if (testExternalValidationService1.IsStarted)
					{
						testExternalValidationService1.Stop();
					}
				}
			}
		}

		public void TestSuggestAddress()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var cancellationToken = new CancellationTokenSource();
			Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU")).RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.Address1 = "702 O'Riordan St";
			address.Address2 = "";
			address.City = "Alexandria";
			address.State = "NSW";
			address.Postcode = "2015";
			address.OA_RN_NKCountryCode = "AU";
			AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);

			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			// Arrange
			AddressValidationService.SetAvailableWebServiceAddress(string.Format("http://localhost:{0}/addresscleansing/v2/", HttpServiceForTest.GetFreeTcpPort()));
			var wsAddress = AddressValidationService.GetAvailableWebServiceAddressAsync().Result;
			var serviceUri = new Uri(wsAddress.Uri + AddressValidationService.Constants.ValidationServiceName + "/");
			var testExternalValidationService = new HttpServiceForTest
			{
				Delay = 0,
				Methods = new string[] { "POST" },
				Processor = (_, request) => new Tuple<int, string>(200, @"
					{""Items"":
						[
							{""ValidationResultItem"":{""AddressRecordGUID"":""b681b902-685a-4afc-85e2-c312c0b29f4b"",""AddressSourceTable"":""E2"",""Addressee"":null,""Address1"":""72 O'RIORDAN STREET"",""Address2"":null,""City"":""ALEXANDRIA"",""County"":null,""State"":""NSW"",""Postcode"":""2015"",""PostcodeAddOn"":null,""Country"":""AU"",""Apartment"":null,""StreetNumber"":""72"",""Street"":""O'RIORDAN STREET"",""Latitude"":-33.916565139125929,""Longitude"":151.19541258,""MatchCode"":""S8HPNTSCZG"",""MatchCodeFlag"":4106,""ChangeCount"":0,""LocationPrecision"":16,""ResultStatusCode"":""PCL"",""ServiceType"":1,""QueryType"":1,""AvailableData"":3,""AddressType"":8,""ErrorMessage"":null},
								""ProviderServiceCalls"":[],
								""Suggestions"":
								[
									{""AddressRecordGUID"":""b681b902-685a-4afc-85e2-c312c0b29f4b"",""AddressSourceTable"":""E2"",""Addressee"":null,""Address1"":""702 O Riordan St"",""Address2"":null,""City"":""Alexandria"",""County"":null,""State"":""NSW"",""Postcode"":""2015"",""PostcodeAddOn"":null,""Country"":""AU"",""Apartment"":null,""ChangeCount"":0,""StreetNumber"":null,""Street"":null,""MatchCode"":null,""MatchCodeFlag"":0,""LocationPrecision"":0,""ResultStatusCode"":null,""ServiceType"":0,""QueryType"":0,""AvailableData"":0,""AddressType"":0,""ErrorMessage"":null,""Group"":""O Riordan St,,Alexandria,NSW,AU,2015"",""Latitude"":0.0,""Longitude"":0.0},
									{""AddressRecordGUID"":""b681b902-685a-4afc-85e2-c312c0b29f4b"",""AddressSourceTable"":""E2"",""Addressee"":null,""Address1"":""U"",""Address2"":""702 O Riordan St"",""City"":""Alexandria"",""County"":null,""State"":""NSW"",""Postcode"":""2015"",""PostcodeAddOn"":null,""Country"":""AU"",""Apartment"":null,""ChangeCount"":0,""StreetNumber"":null,""Street"":null,""MatchCode"":null,""MatchCodeFlag"":0,""LocationPrecision"":0,""ResultStatusCode"":null,""ServiceType"":0,""QueryType"":0,""AvailableData"":0,""AddressType"":0,""ErrorMessage"":null,""Group"":""U,O Riordan St,Alexandria,NSW,AU,2015"",""Latitude"":0.0,""Longitude"":0.0},
									{""AddressRecordGUID"":""b681b902-685a-4afc-85e2-c312c0b29f4b"",""AddressSourceTable"":""E2"",""Addressee"":null,""Address1"":""U 4"",""Address2"":""74 O Riordan St"",""City"":""Alexandria"",""County"":null,""State"":""NSW"",""Postcode"":""2015"",""PostcodeAddOn"":null,""Country"":""AU"",""Apartment"":null,""ChangeCount"":0,""StreetNumber"":null,""Street"":null,""MatchCode"":null,""MatchCodeFlag"":0,""LocationPrecision"":0,""ResultStatusCode"":null,""ServiceType"":0,""QueryType"":0,""AvailableData"":0,""AddressType"":0,""ErrorMessage"":null,""Group"":""U,O Riordan St,Alexandria,NSW,AU,2015"",""Latitude"":0.0,""Longitude"":0.0}
								]
							}
						]
					}"),
				Uri = serviceUri,
				ContentType = "application/json"
			};
			try
			{
				testExternalValidationService.Start();
				Assert(String.Format("The test external validation service cannot be started on {0}.", serviceUri.AbsoluteUri), testExternalValidationService.IsStarted);
				// Act
				var result = AddressValidationService.ValidateAddressAsync(address, cancellationToken).Result;

				// Assert
				AssertNotNull(result);
				AssertEquals(3, result.SuggestedResults.Count);
			}
			finally
			{
				if (testExternalValidationService.IsStarted)
				{
					testExternalValidationService.Stop();
				}
				AddressValidationService.SetAvailableWebServiceAddress();
			}
		}

		public void TestGetQueryStringForValidationService_LanguageMapping()
		{
			var uriFull = AddressValidationService.GetQueryStringForValidationService(string.Empty, Core.SharedConstants.Languages.English);
			AssertContains("languageCode=ENG", uriFull);

			uriFull = AddressValidationService.GetQueryStringForValidationService(string.Empty, Core.SharedConstants.Languages.EnglishBritish);
			AssertContains("languageCode=EGB", uriFull);

			uriFull = AddressValidationService.GetQueryStringForValidationService(string.Empty, Core.SharedConstants.Languages.EnglishAmerican);
			AssertContains("languageCode=EUS", uriFull);

			uriFull = AddressValidationService.GetQueryStringForValidationService(string.Empty, Core.SharedConstants.Languages.Latvian);
			AssertContains("languageCode=LV-LV", uriFull);
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestSetSuggestedAddressToAddressForValidation()
		{
			Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU")).RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "AU";
			address.Address1 = "Add1";
			address.Address2 = "Add2";
			address.City = "City";
			address.Postcode = "P1";

			var addressItem = new ValidationResultItem();
			addressItem.Address1 = "New Add1";
			addressItem.Address2 = "New Add2";
			addressItem.City = "New City";
			addressItem.Postcode = "New P1";

			AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
			AssertEquals(addressItem.Address1, address.Address1);
			AssertEquals(addressItem.Address2, address.Address2);
			AssertEquals(addressItem.City, address.City);
			AssertEquals(addressItem.Postcode, address.Postcode);
			AssertEquals(AddressValidationStatus.Verified, address.ValidationStatus);

			AddressValidationService.SetSuggestedAddressToAddressForValidation(null, true, address);
			AssertEquals(AddressValidationStatus.ManuallyVerified, address.ValidationStatus);
		}

		[ExpectNoExceptions]
		public void TestSetSuggestedAddressToAddressForValidationMaxLength()
		{
			Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU")).RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "AU";

			var addressItem = new ValidationResultItem();
			addressItem.Address1 = "New Add1  New Add1  New Add1  New Add1  New Add1  New Add1  ";
			addressItem.Address2 = "New Add2  New Add2  New Add2  New Add2  New Add2  New Add2  ";
			addressItem.City = "New City  New City  New City  New City  New City  New City  ";
			addressItem.Postcode = "New P1 New P1 ";
			addressItem.State = "NSW  NSW  NSW  NSW  NSW  NSW  ";

			AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
		}

		public void TestSetClosestPort_MatchesActiveUNLOCO()
		{
			var org = Factory.NewWithValidTestData<OrgAddress>();
			org.OA_RN_NKCountryCode = "CN";
			var suggestedAddress = new ValidationResultItem() { Latitude = 30.66, Longitude = 104.07 }; // use a real UNLOCO: CNCDU

			using (OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.EnableDefaultingClosestPortOnAddressValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AddressValidationService.SetSuggestedAddressToAddressForValidation(suggestedAddress, false, org);
				AssertEquals("The UNLOCO should be CNCDU", "CNCDU", org.ClosestPort);
			}
		}

		public void TestSetClosestPort_NotMatchesInactiveUNLOCO()
		{
			var org = Factory.NewWithValidTestData<OrgAddress>();
			org.OA_RN_NKCountryCode = "CN";
			var suggestedAddress = new ValidationResultItem() { Latitude = 30.66, Longitude = 104.07 }; // use a real UNLOCO: CNCDU
			var chengdu = RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Chengdu", "CN");

			chengdu.IsCancelled = true;
			Factory.Save();

			using (OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.EnableDefaultingClosestPortOnAddressValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AddressValidationService.SetSuggestedAddressToAddressForValidation(suggestedAddress, false, org);
				AssertNotEquals("The UNLOCO should not be CNCDU, since CNCDU in not active", "CNCDU", org.ClosestPort);
			}
		}

		public void TestSetClosestPort_ForOrgAddress()
		{
			var factory = new BusinessObjectFactory();
			var org = factory.NewWithValidTestData<OrgAddress>();
			org.OA_RN_NKCountryCode = "CN";
			var suggestedAddress = new ValidationResultItem() { Latitude = 30.66, Longitude = 104.07 }; // use a real UNLOCO: CNCDU

			factory.ResetDatabaseLoadCount();
			var expectedConditionDbHits = new Dictionary<string, int>() // this requires a comprehensive DB hits list
			{
				{ RefCountryStatesSchema.Constants.TableName, 1 }, // CountryStates should be hit in this testcase
				{ RefUNLOCOSchema.Constants.TableName, 1 } // We should also hit UNLOCO as it is needed for OrgAddress
			};

			using (OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.EnableDefaultingClosestPortOnAddressValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AddressValidationService.SetSuggestedAddressToAddressForValidation(suggestedAddress, false, org);
				CombineAssertions(() =>
				{
					AssertEquals("Org should receive a ClosestPort", "CNCDU", org.ClosestPort);
					AssertDbHits(expectedConditionDbHits, factory);
				});
			}
		}

		public void TestSetClosestPort_ForGlbStaff()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_RN_NKCountryCode = "CN";
			var suggestedAddress = new ValidationResultItem() { Latitude = 30.66, Longitude = 104.07 }; // use a real UNLOCO: CNCDU

			factory.ResetDatabaseLoadCount();

			var expectedConditionDbHits = new Dictionary<string, int>() // this requires a comprehensive DB hits list
			{
				{ RefCountryStatesSchema.Constants.TableName, 1 }, // CountryStates should be hit in this testcase
				{ RefUNLOCOSchema.Constants.TableName, 0 } // We should not hit UNLOCO as it's not needed for GlbStaff
			};

			using (OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.EnableDefaultingClosestPortOnAddressValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AddressValidationService.SetSuggestedAddressToAddressForValidation(suggestedAddress, false, staff);
				CombineAssertions(() =>
				{
					AssertNullOrEmpty("GlbStaff should not receive a ClosestPort", staff.ClosestPort);
					AssertDbHits(expectedConditionDbHits, factory);
				});
			}
		}

		public void TestSetClosestPort()
		{
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_GeoLocation) values(newid(), 'AUPRT', 'AU', convert(geography, '{ZGeography.CreatePoint(0, 0).AsText()}'))");
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNPRT', 'CN', 1, convert(geography, '{ZGeography.CreatePoint(0, 1).AsText()}'))");

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "AU";
			address.OA_RL_NKRelatedPortCode = string.Empty;

			var addressItem = new ValidationResultItem();
			addressItem.Longitude = 0;
			addressItem.Latitude = 0.9;

			using (OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.EnableDefaultingClosestPortOnAddressValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
				AssertNullOrEmpty("Should not set closest port since registry is not enabled.", address.OA_RL_NKRelatedPortCode);
			}

			using (OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.EnableDefaultingClosestPortOnAddressValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				address.OA_RN_NKCountryCode = string.Empty;
				AssertNullOrEmpty("Precondition", address.OA_RN_NKCountryCode);
				AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
				AssertNullOrEmpty("Should not set closest port since Country is not set.", address.OA_RL_NKRelatedPortCode);

				address.OA_RL_NKRelatedPortCode = "AUPRT";
				address.OA_RN_NKCountryCode = "AU";
				AssertEquals("Precondition", "AUPRT", address.OA_RL_NKRelatedPortCode);
				AssertEquals("Precondition", "AU", address.OA_RN_NKCountryCode);
				AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
				AssertEquals("Should not set closest port since it already has a value.", "AUPRT", address.OA_RL_NKRelatedPortCode);

				address.OA_RL_NKRelatedPortCode = string.Empty;
				AssertNullOrEmpty("Precondition", address.OA_RL_NKRelatedPortCode);
				AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
				AssertNullOrEmpty("Should not set closest port since the closest port is in the different country.", address.OA_RL_NKRelatedPortCode);

				address.OA_RN_NKCountryCode = "CN";
				AssertEquals("Precondition", "CN", address.OA_RN_NKCountryCode);
				AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
				AssertEquals("The closest port should be CNPRT", "CNPRT", address.OA_RL_NKRelatedPortCode);

				address.OA_RL_NKRelatedPortCode = string.Empty;
				AssertNullOrEmpty("Precondition", address.OA_RL_NKRelatedPortCode);
				AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, true, address);
				AssertEquals("The closest port should be CNPRT", "CNPRT", address.OA_RL_NKRelatedPortCode);
			}
		}

		public void TestSetClosestPort_AssignsClosestPort_WithDefault_Identifier()
		{
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNPRT', 'CN', 1, convert(geography, '{ZGeography.CreatePoint(0, 1).AsText()}'))");

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "CN";
			address.OA_RL_NKRelatedPortCode = string.Empty;

			var addressItem = new ValidationResultItem();
			addressItem.Longitude = 0;
			addressItem.Latitude = 0.9;

			using (OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
				AssertEquals("The closest port should be CNPRT", "CNPRT", address.OA_RL_NKRelatedPortCode);
			}
		}

		public void TestSetClosestPort_AssignsClosestPort_WithValidNonDefault_Identifier()
		{
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_GeoLocation) values(newid(), 'CNPRT', 'CN', 1, convert(geography, '{ZGeography.CreatePoint(0, 1).AsText()}'))");

			var codeDescriptionValues = new CodeDescriptionPairList();

			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasAirport, "Has Airport");
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasRoad, "Has Road");

			var collection = new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection(new ReadOnlyCodeDescriptionPairList(codeDescriptionValues));

			collection[1].Bool = true;

			using (OrganisationsDataRegistry.Instance.UNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var address = Factory.NewWithValidTestData<OrgAddress>();
				address.OA_RN_NKCountryCode = "CN";
				address.OA_RL_NKRelatedPortCode = string.Empty;

				var addressItem = new ValidationResultItem();
				addressItem.Longitude = 0;
				addressItem.Latitude = 0.9;

				using (OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
					AssertEquals("The closest port should be CNPRT", "CNPRT", address.OA_RL_NKRelatedPortCode);
				}
			}
		}

		public void TestSetClosestPort_DoesNotAssignClosestPort_WithInvalid_Identifier()
		{
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_GeoLocation) values(newid(), 'CNPRT', 'CN', 1, convert(geography, '{ZGeography.CreatePoint(0, 1).AsText()}'))");

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "CN";
			address.OA_RL_NKRelatedPortCode = string.Empty;

			var addressItem = new ValidationResultItem();
			addressItem.Longitude = 0;
			addressItem.Latitude = 0.9;
			using (OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
				AssertNullOrEmpty("Should not set closest port since there is no enabled matching flag/identifier.", address.OA_RL_NKRelatedPortCode);
			}
		}

		public void TestSetClosestPort_AssignsClosestPort_RequireAllUNLOCOConditionsToBeMet()
		{
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNFAR', 'CN', 1, 1, convert(geography, '{ZGeography.CreatePoint(0, 1.5).AsText()}'))");
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNPRT', 'CN', 1, 1, convert(geography, '{ZGeography.CreatePoint(0, 1).AsText()}'))");
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNAIR', 'CN', 0, 1, convert(geography, '{ZGeography.CreatePoint(0, 0.9).AsText()}'))");
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNEMP', 'CN', 0, 0, convert(geography, '{ZGeography.CreatePoint(0, 0.8).AsText()}'))");

			var codeDescriptionValues = new CodeDescriptionPairList();

			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasAirport, "Has Airport");
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasRoad, "Has Road");

			var collection = new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection(new ReadOnlyCodeDescriptionPairList(codeDescriptionValues));

			collection[0].Bool = true;
			collection[1].Bool = true;

			using (OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.UNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var address = Factory.NewWithValidTestData<OrgAddress>();
				address.OA_RN_NKCountryCode = "CN";
				address.OA_RL_NKRelatedPortCode = string.Empty;

				var addressItem = new ValidationResultItem();
				addressItem.Longitude = 0;
				addressItem.Latitude = 0.7;

				AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
				AssertEquals("The closest port should be CNPRT", "CNPRT", address.OA_RL_NKRelatedPortCode);
			}
		}

		public void TestSetClosestPort_DoesNotAssignClosestPort_RequireAllUNLOCOConditionsToBeMet()
		{
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNFAR', 'CN', 1, 1, convert(geography, '{ZGeography.CreatePoint(0, 1.5).AsText()}'))");
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNPRT', 'CN', 1, 1, convert(geography, '{ZGeography.CreatePoint(0, 1).AsText()}'))");
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNEMP', 'CN', 0, 0, convert(geography, '{ZGeography.CreatePoint(0, 0.8).AsText()}'))");
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNAIR', 'CN', 0, 1, convert(geography, '{ZGeography.CreatePoint(0, 0.9).AsText()}'))");

			var codeDescriptionValues = new CodeDescriptionPairList();

			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasSeaport, "Has Seaport");
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasRoad, "Has Road");

			var collection = new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection(new ReadOnlyCodeDescriptionPairList(codeDescriptionValues));

			collection[0].Bool = true;

			using (OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.UNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var address = Factory.NewWithValidTestData<OrgAddress>();
				address.OA_RN_NKCountryCode = "CN";
				address.OA_RL_NKRelatedPortCode = string.Empty;

				var addressItem = new ValidationResultItem();
				addressItem.Longitude = 0;
				addressItem.Latitude = 0.7;

				AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
				AssertNullOrEmpty("Should not set closest port since there is no enabled matching flag/identifier.", address.OA_RL_NKRelatedPortCode);
			}
		}

		public void TestSetClosestPort_AssignsClosestPort_RequireAnyUNLOCOConditionsToBeMet()
		{
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNFAR', 'CN', 1, 1, convert(geography, '{ZGeography.CreatePoint(0, 1.5).AsText()}'))");
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNAIR', 'CN', 0, 1, convert(geography, '{ZGeography.CreatePoint(0, 0.9).AsText()}'))");
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNEMP', 'CN', 0, 0, convert(geography, '{ZGeography.CreatePoint(0, 0.8).AsText()}'))");
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNPRT', 'CN', 1, 1, convert(geography, '{ZGeography.CreatePoint(0, 1).AsText()}'))");

			var codeDescriptionValues = new CodeDescriptionPairList();

			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasAirport, "Has Airport");
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasRoad, "Has Road");

			var collection = new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection(new ReadOnlyCodeDescriptionPairList(codeDescriptionValues));

			collection[0].Bool = true;
			collection[1].Bool = true;

			using (OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.UNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var address = Factory.NewWithValidTestData<OrgAddress>();
				address.OA_RN_NKCountryCode = "CN";
				address.OA_RL_NKRelatedPortCode = string.Empty;

				var addressItem = new ValidationResultItem();
				addressItem.Longitude = 0;
				addressItem.Latitude = 0.7;

				AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
				AssertEquals("The closest port should be CNAIR", "CNAIR", address.OA_RL_NKRelatedPortCode);
			}
		}

		public void TestSetClosestPort_ByCountryStateCity_OnlyOneResult_ApplyRegistryRule()
		{
			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_RN_NKCountryCode = "CN";
			Factory.Save();

			var unlocoPk = Guid.NewGuid();
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_RW, RL_PortName, RL_NameWithDiacriticals, RL_GeoLocation) values('{unlocoPk}', 'CNNAJ', 'CN', '{state.PK}', 'Nanjing', 'Nanjing', convert(geography, '{ZGeography.CreatePoint(0, 0.9).AsText()}'))");
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_RW, RL_PortName, RL_NameWithDiacriticals, RL_GeoLocation, RL_HasAirport) values(newid(), 'CNTIJ', 'CN', '{state.PK}', 'Tianjin', 'Tianjin', convert(geography, '{ZGeography.CreatePoint(0, 0.9).AsText()}'), 1)");

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "CN";
			address.StateCode = state.RW_Code;
			address.City = "Nanjing";
			address.OA_RL_NKRelatedPortCode = string.Empty;

			var addressItem = new ValidationResultItem();
			addressItem.Longitude = 0;
			addressItem.Latitude = 0.9;
			addressItem.City = "Nanjing";

			var codeDescriptionValues = new CodeDescriptionPairList();
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasAirport, "Has Airport");
			var collection = new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection(new ReadOnlyCodeDescriptionPairList(codeDescriptionValues));
			collection[0].Bool = true;

			using (OrganisationsDataRegistry.Instance.EnableDefaultingClosestPortOnAddressValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.UNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
				AssertEquals("CNTIJ", address.OA_RL_NKRelatedPortCode);

				address.OA_RL_NKRelatedPortCode = ZString.Empty;
				TestConnection.ExecuteNonQuery($"update RefUNLOCO set RL_HasAirport = 1 where RL_PK = '{unlocoPk}'");
				AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
				AssertEquals("CNNAJ", address.OA_RL_NKRelatedPortCode);
			}
		}

		public void TestSetClosestPort_ByCountryStateCity_HaveMultipleResultsThenChooseClosestOne()
		{
			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_RN_NKCountryCode = "CN";
			Factory.Save();

			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_RW, RL_PortName, RL_NameWithDiacriticals, RL_GeoLocation) values(newid(), 'CNNAJ', 'CN', '{state.PK}', 'XXXXX', 'Nanjing', convert(geography, '{ZGeography.CreatePoint(0, 0).AsText()}'))");
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_RW, RL_PortName, RL_NameWithDiacriticals, RL_GeoLocation) values(newid(), 'CNNAK', 'CN', '{state.PK}', 'YYYYY', 'Nanjing', convert(geography, '{ZGeography.CreatePoint(0, 0.8).AsText()}'))");

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "CN";
			address.StateCode = state.RW_Code;
			address.City = "Nanjing";
			address.OA_RL_NKRelatedPortCode = string.Empty;

			var addressItem = new ValidationResultItem();
			addressItem.Longitude = 0;
			addressItem.Latitude = 0.9;
			addressItem.City = "Nanjing";

			var codeDescriptionValues = new CodeDescriptionPairList();
			var collection = new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection(new ReadOnlyCodeDescriptionPairList(codeDescriptionValues));

			using (OrganisationsDataRegistry.Instance.UNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (OrganisationsDataRegistry.Instance.EnableDefaultingClosestPortOnAddressValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
				AssertEquals("CNNAK is the closest one.", "CNNAK", address.OA_RL_NKRelatedPortCode);
			}
		}

		public void TestSetClosestPort_ByCountryCity_OnlyOneResult_ApplyRegistryRule()
		{
			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			country.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustNotBeEntered;
			Factory.Save();

			var unlocoPk = Guid.NewGuid();
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_PortName, RL_NameWithDiacriticals, RL_GeoLocation) values('{unlocoPk}', 'CNTAN', 'CN', 'XXXXX', 'Tianan', convert(geography, '{ZGeography.CreatePoint(0, 0).AsText()}'))");
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_PortName, RL_NameWithDiacriticals, RL_GeoLocation, RL_HasAirport) values(newid(), 'CNNAK', 'CN', 'YYYYY', 'Baixia', convert(geography, '{ZGeography.CreatePoint(0, 0.8).AsText()}'), 1)");

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "CN";
			address.City = "Tianan";
			address.OA_RL_NKRelatedPortCode = string.Empty;

			var addressItem = new ValidationResultItem();
			addressItem.Longitude = 0;
			addressItem.Latitude = 0.9;
			addressItem.City = "Tianan";

			var codeDescriptionValues = new CodeDescriptionPairList();
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasAirport, "Has Airport");
			var collection = new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection(new ReadOnlyCodeDescriptionPairList(codeDescriptionValues));
			collection[0].Bool = true;

			using (OrganisationsDataRegistry.Instance.EnableDefaultingClosestPortOnAddressValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.UNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
				AssertNullOrEmpty(address.State);
				AssertEquals("CNNAK", address.OA_RL_NKRelatedPortCode);

				address.OA_RL_NKRelatedPortCode = ZString.Empty;
				TestConnection.ExecuteNonQuery($"update RefUNLOCO set RL_HasAirport = 1 where RL_PK = '{unlocoPk}'");
				AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
				AssertEquals("CNTAN", address.OA_RL_NKRelatedPortCode);
			}
		}

		public void TestSetClosestPort_ByCountryCity_HaveMultipleResultsThenChooseClosestOne()
		{
			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			country.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustNotBeEntered;
			Factory.Save();

			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_PortName, RL_NameWithDiacriticals, RL_GeoLocation) values(newid(), 'CNNAJ', 'CN', 'XXXXX', 'Nanjing', convert(geography, '{ZGeography.CreatePoint(0, 0).AsText()}'))");
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_PortName, RL_NameWithDiacriticals, RL_GeoLocation) values(newid(), 'CNNAK', 'CN', 'YYYYY', 'Nanjing', convert(geography, '{ZGeography.CreatePoint(0, 0.8).AsText()}'))");

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "CN";
			address.City = "Nanjing";
			address.OA_RL_NKRelatedPortCode = string.Empty;

			var addressItem = new ValidationResultItem();
			addressItem.Longitude = 0;
			addressItem.Latitude = 0.9;
			addressItem.City = "Nanjing";

			var codeDescriptionValues = new CodeDescriptionPairList();
			var collection = new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection(new ReadOnlyCodeDescriptionPairList(codeDescriptionValues));

			using (OrganisationsDataRegistry.Instance.UNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (OrganisationsDataRegistry.Instance.EnableDefaultingClosestPortOnAddressValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
				AssertNullOrEmpty(address.State);
				AssertEquals("CNNAK is the closest one.", "CNNAK", address.OA_RL_NKRelatedPortCode);
			}
		}

		public void TestSetClosestPort_ByCountryState_OnlyOneResult_ApplyRegistryRule()
		{
			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_RN_NKCountryCode = "CN";
			Factory.Save();

			var unlocoPk = Guid.NewGuid();
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_RW, RL_PortName, RL_NameWithDiacriticals, RL_GeoLocation) values('{unlocoPk}', 'CNNAJ', 'CN', '{state.PK}', 'Nanjing', 'Nanjing', convert(geography, '{ZGeography.CreatePoint(0, 0).AsText()}'))");
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_PortName, RL_NameWithDiacriticals, RL_GeoLocation, RL_HasAirport) values(newid(), 'CNTIJ', 'CN', 'Tianjin', 'Tianjin', convert(geography, '{ZGeography.CreatePoint(0, 0.8).AsText()}'), 1)");

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "CN";
			address.StateCode = state.RW_Code;
			address.OA_RL_NKRelatedPortCode = string.Empty;

			var addressItem = new ValidationResultItem();
			addressItem.Longitude = 0;
			addressItem.Latitude = 0.9;

			var codeDescriptionValues = new CodeDescriptionPairList();
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasAirport, "Has Airport");
			var collection = new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection(new ReadOnlyCodeDescriptionPairList(codeDescriptionValues));
			collection[0].Bool = true;

			using (OrganisationsDataRegistry.Instance.UNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (OrganisationsDataRegistry.Instance.EnableDefaultingClosestPortOnAddressValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
				AssertNullOrEmpty(address.City);
				AssertEquals("CNTIJ", address.OA_RL_NKRelatedPortCode);

				address.OA_RL_NKRelatedPortCode = ZString.Empty;
				TestConnection.ExecuteNonQuery($"update RefUNLOCO set RL_HasAirport = 1 where RL_PK = '{unlocoPk}'");
				AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
				AssertEquals("CNNAJ", address.OA_RL_NKRelatedPortCode);
			}
		}

		public void TestSetClosestPort_ByCountryState_HaveMultipleResultsThenChooseClosestOne()
		{
			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_RN_NKCountryCode = "CN";
			Factory.Save();

			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_RW, RL_PortName, RL_NameWithDiacriticals, RL_GeoLocation) values(newid(), 'CNNAJ', 'CN', '{state.PK}', 'Nanjing', 'Nanjing', convert(geography, '{ZGeography.CreatePoint(0, 0).AsText()}'))");
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_RW, RL_PortName, RL_NameWithDiacriticals, RL_GeoLocation) values(newid(), 'CNTIJ', 'CN', '{state.PK}', 'Tianjin', 'Tianjin', convert(geography, '{ZGeography.CreatePoint(0, 0.8).AsText()}'))");

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "CN";
			address.StateCode = state.RW_Code;
			address.OA_RL_NKRelatedPortCode = string.Empty;

			var addressItem = new ValidationResultItem();
			addressItem.Longitude = 0;
			addressItem.Latitude = 0.9;

			var codeDescriptionValues = new CodeDescriptionPairList();
			var collection = new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection(new ReadOnlyCodeDescriptionPairList(codeDescriptionValues));

			using (OrganisationsDataRegistry.Instance.UNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (OrganisationsDataRegistry.Instance.EnableDefaultingClosestPortOnAddressValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
				AssertNullOrEmpty(address.City);
				AssertEquals("CNTIJ is the closest one.", "CNTIJ", address.OA_RL_NKRelatedPortCode);
			}
		}

		public void TestSetClosestPort_ByCountryStateCity_ApplyOrIgnoreUNLOCODefaultingRules()
		{
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNAIR', 'CN', 0, 1, convert(geography, '{ZGeography.CreatePoint(0, 0.9).AsText()}'))");
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNEMP', 'CN', 0, 0, convert(geography, '{ZGeography.CreatePoint(0, 0.8).AsText()}'))");

			var codeDescriptionValues = new CodeDescriptionPairList();
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasAirport, "Has Airport");

			var collection = new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection(new ReadOnlyCodeDescriptionPairList(codeDescriptionValues));
			collection[0].Bool = true;

			using (OrganisationsDataRegistry.Instance.UNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var state = Factory.NewWithValidTestData<RefCountryStates>();
				state.RW_RN_NKCountryCode = "CN";
				Factory.Save();

				var addressItem = new ValidationResultItem();
				addressItem.Longitude = 0;
				addressItem.Latitude = 0.7;
				addressItem.City = "Nanjing";

				using (OrganisationsDataRegistry.Instance.IgnoreUNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var address = Factory.NewWithValidTestData<OrgAddress>();
					address.OA_RN_NKCountryCode = "CN";
					address.StateCode = state.RW_Code;
					address.City = "Nanjing";
					address.OA_RL_NKRelatedPortCode = string.Empty;

					AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
					AssertNotNullOrEmpty(address.State);
					AssertNotNullOrEmpty(address.City);
					AssertEquals("CNAIR", address.OA_RL_NKRelatedPortCode);
				}

				using (OrganisationsDataRegistry.Instance.IgnoreUNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var address = Factory.NewWithValidTestData<OrgAddress>();
					address.OA_RN_NKCountryCode = "CN";
					address.StateCode = state.RW_Code;
					address.City = "Nanjing";
					address.OA_RL_NKRelatedPortCode = string.Empty;

					AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
					AssertNotNullOrEmpty(address.State);
					AssertNotNullOrEmpty(address.City);
					AssertEquals("CNEMP", address.OA_RL_NKRelatedPortCode);
				}
			}
		}

		public void TestSetClosestPort_ByCountryState_ApplyOrIgnoreUNLOCODefaultingRules()
		{
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNAIR', 'CN', 0, 1, convert(geography, '{ZGeography.CreatePoint(0, 0.9).AsText()}'))");
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNEMP', 'CN', 0, 0, convert(geography, '{ZGeography.CreatePoint(0, 0.8).AsText()}'))");

			var codeDescriptionValues = new CodeDescriptionPairList();
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasAirport, "Has Airport");

			var collection = new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection(new ReadOnlyCodeDescriptionPairList(codeDescriptionValues));
			collection[0].Bool = true;

			using (OrganisationsDataRegistry.Instance.UNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var state = Factory.NewWithValidTestData<RefCountryStates>();
				state.RW_RN_NKCountryCode = "CN";
				Factory.Save();

				var addressItem = new ValidationResultItem();
				addressItem.Longitude = 0;
				addressItem.Latitude = 0.7;

				using (OrganisationsDataRegistry.Instance.IgnoreUNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var address = Factory.NewWithValidTestData<OrgAddress>();
					address.OA_RN_NKCountryCode = "CN";
					address.StateCode = state.RW_Code;
					address.OA_RL_NKRelatedPortCode = string.Empty;

					AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
					AssertNotNullOrEmpty(address.State);
					AssertNullOrEmpty(address.City);
					AssertEquals("CNAIR", address.OA_RL_NKRelatedPortCode);
				}

				using (OrganisationsDataRegistry.Instance.IgnoreUNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var address = Factory.NewWithValidTestData<OrgAddress>();
					address.OA_RN_NKCountryCode = "CN";
					address.StateCode = state.RW_Code;
					address.OA_RL_NKRelatedPortCode = string.Empty;

					AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
					AssertNotNullOrEmpty(address.State);
					AssertNullOrEmpty(address.City);
					AssertEquals("CNEMP", address.OA_RL_NKRelatedPortCode);
				}
			}
		}

		public void TestSetClosestPort_ByCountryCity_ApplyOrIgnoreUNLOCODefaultingRules()
		{
			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			country.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustNotBeEntered;
			Factory.Save();

			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNAIR', 'CN', 0, 1, convert(geography, '{ZGeography.CreatePoint(0, 0.9).AsText()}'))");
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNEMP', 'CN', 0, 0, convert(geography, '{ZGeography.CreatePoint(0, 0.8).AsText()}'))");

			var codeDescriptionValues = new CodeDescriptionPairList();
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasAirport, "Has Airport");

			var collection = new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection(new ReadOnlyCodeDescriptionPairList(codeDescriptionValues));
			collection[0].Bool = true;

			using (OrganisationsDataRegistry.Instance.UNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var addressItem = new ValidationResultItem();
				addressItem.Longitude = 0;
				addressItem.Latitude = 0.7;
				addressItem.City = "Tianan";

				using (OrganisationsDataRegistry.Instance.IgnoreUNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var address = Factory.NewWithValidTestData<OrgAddress>();
					address.OA_RN_NKCountryCode = "CN";
					address.OA_RL_NKRelatedPortCode = string.Empty;
					address.City = "Tianan";

					AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
					AssertNullOrEmpty(address.State);
					AssertNotNullOrEmpty(address.City);
					AssertEquals("CNAIR", address.OA_RL_NKRelatedPortCode);
				}

				using (OrganisationsDataRegistry.Instance.IgnoreUNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var address = Factory.NewWithValidTestData<OrgAddress>();
					address.OA_RN_NKCountryCode = "CN";
					address.OA_RL_NKRelatedPortCode = string.Empty;

					AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
					AssertNullOrEmpty(address.State);
					AssertNotNullOrEmpty(address.City);
					AssertEquals("CNEMP", address.OA_RL_NKRelatedPortCode);
				}
			}
		}

		public void TestSetClosestPort_NoState_NoCity_ApplyOrIgnoreUNLOCODefaultingRules()
		{
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNAIR', 'CN', 0, 1, convert(geography, '{ZGeography.CreatePoint(0, 0.9).AsText()}'))");
			TestConnection.ExecuteNonQuery($"insert into RefUNLOCO (RL_PK, RL_Code, RL_RN_NKCountryCode, RL_HasRoad, RL_HasAirport, RL_GeoLocation) values(newid(), 'CNEMP', 'CN', 0, 0, convert(geography, '{ZGeography.CreatePoint(0, 0.8).AsText()}'))");

			var codeDescriptionValues = new CodeDescriptionPairList();
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasAirport, "Has Airport");

			var collection = new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection(new ReadOnlyCodeDescriptionPairList(codeDescriptionValues));
			collection[0].Bool = true;

			using (OrganisationsDataRegistry.Instance.UNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var addressItem = new ValidationResultItem();
				addressItem.Longitude = 0;
				addressItem.Latitude = 0.7;

				using (OrganisationsDataRegistry.Instance.IgnoreUNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var address = Factory.NewWithValidTestData<OrgAddress>();
					address.OA_RN_NKCountryCode = "CN";
					address.OA_RL_NKRelatedPortCode = string.Empty;

					AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
					AssertNullOrEmpty(address.State);
					AssertNullOrEmpty(address.City);
					AssertEquals("CNAIR", address.OA_RL_NKRelatedPortCode);
				}

				using (OrganisationsDataRegistry.Instance.IgnoreUNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var address = Factory.NewWithValidTestData<OrgAddress>();
					address.OA_RN_NKCountryCode = "CN";
					address.OA_RL_NKRelatedPortCode = string.Empty;

					AddressValidationService.SetSuggestedAddressToAddressForValidation(addressItem, false, address);
					AssertNullOrEmpty(address.State);
					AssertNullOrEmpty(address.City);
					AssertEquals("CNEMP", address.OA_RL_NKRelatedPortCode);
				}
			}
		}

		public void TestSetValidationStatus()
		{
			AssertValidationStatusForValidType(QueryType.AsEntered);
			AssertValidationStatusForValidType(QueryType.AllCombined);
			AssertValidationStatusForInValidType(QueryType.Address1Removed);
			AssertValidationStatusForInValidType(QueryType.Address2Removed);
			AssertValidationStatusForInValidType(QueryType.CityAsCounty);
			AssertValidationStatusForInValidType(QueryType.CityAsLocality);
			AssertValidationStatusForInValidType(QueryType.CityRemoved);
			AssertValidationStatusForInValidType(QueryType.StateRemoved);
			AssertValidationStatusForInValidType(QueryType.None);
		}

		void AssertValidationStatusForValidType(QueryType type)
		{
			AssertValidationStatus(type, ValidationResultStatusCode.PointExact, AvailableData.Street, AddressValidationStatus.Verified);
			AssertValidationStatus(type, ValidationResultStatusCode.PrivateAddressValid, AvailableData.Street, AddressValidationStatus.Verified);
			AssertValidationStatus(type, ValidationResultStatusCode.StreetExact, AvailableData.Street, AddressValidationStatus.VerifiedToStreet);
			AssertValidationStatus(type, ValidationResultStatusCode.StreetExact, AvailableData.StreetNumber, AddressValidationStatus.Invalid);
			AssertValidationStatus(type, ValidationResultStatusCode.Error, AvailableData.Street, AddressValidationStatus.ToBeVerified);
			AssertValidationStatus(type, ValidationResultStatusCode.PrivateAddressNotChecked, AvailableData.Street, AddressValidationStatus.Invalid);
			AssertValidationStatus(type, ValidationResultStatusCode.Invalid, AvailableData.Street, AddressValidationStatus.Invalid);
			AssertValidationStatus(type, ValidationResultStatusCode.PrivateAddressInvalid, AvailableData.Street, AddressValidationStatus.Invalid);
			AssertValidationStatus(type, ValidationResultStatusCode.PointClose, AvailableData.Street, AddressValidationStatus.Invalid);
			AssertValidationStatus(type, ValidationResultStatusCode.StreetClose, AvailableData.Street, AddressValidationStatus.Invalid);
			AssertValidationStatus(type, ValidationResultStatusCode.CityClose, AvailableData.Street, AddressValidationStatus.Invalid);
			AssertValidationStatus(type, ValidationResultStatusCode.CityExact, AvailableData.Street, AddressValidationStatus.Invalid);
		}

		void AssertValidationStatusForInValidType(QueryType type)
		{
			AssertValidationStatus(type, ValidationResultStatusCode.PointExact, AvailableData.Street, AddressValidationStatus.Invalid);
			AssertValidationStatus(type, ValidationResultStatusCode.PrivateAddressValid, AvailableData.Street, AddressValidationStatus.Invalid);
			AssertValidationStatus(type, ValidationResultStatusCode.StreetExact, AvailableData.Street, AddressValidationStatus.Invalid);
			AssertValidationStatus(type, ValidationResultStatusCode.Error, AvailableData.Street, AddressValidationStatus.Invalid);
			AssertValidationStatus(type, ValidationResultStatusCode.PrivateAddressNotChecked, AvailableData.Street, AddressValidationStatus.Invalid);
			AssertValidationStatus(type, ValidationResultStatusCode.Invalid, AvailableData.Street, AddressValidationStatus.Invalid);
			AssertValidationStatus(type, ValidationResultStatusCode.PrivateAddressInvalid, AvailableData.Street, AddressValidationStatus.Invalid);
			AssertValidationStatus(type, ValidationResultStatusCode.PointClose, AvailableData.Street, AddressValidationStatus.Invalid);
			AssertValidationStatus(type, ValidationResultStatusCode.StreetClose, AvailableData.Street, AddressValidationStatus.Invalid);
			AssertValidationStatus(type, ValidationResultStatusCode.CityClose, AvailableData.Street, AddressValidationStatus.Invalid);
			AssertValidationStatus(type, ValidationResultStatusCode.CityExact, AvailableData.Street, AddressValidationStatus.Invalid);
		}

		void AssertValidationStatus(QueryType queryType, string statusCode, AvailableData availableData, string expectedStatus)
		{
			Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU")).RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "AU";

			var validationResult = new ValidationResultItem();
			validationResult.QueryType = queryType;
			validationResult.ResultStatusCode = statusCode;
			validationResult.AvailableData = availableData;

			AddressValidationService.SetCoordinatesAndValidationStatus(address, validationResult);
			AssertEquals(expectedStatus, address.ValidationStatus);
		}

		public void TestSetAddressMap()
		{
			Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU")).RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "AU";
			address.Address1 = "Unit 3A";
			address.Address2 = "72 O'Riordan ST";

			var validationResult = new ValidationResultItem();
			validationResult.QueryType = QueryType.AllCombined;
			validationResult.ResultStatusCode = ValidationResultStatusCode.PointExact;
			validationResult.AvailableData = AvailableData.Street;
			validationResult.Address1 = "Unit 3A";
			validationResult.Address2 = "72 O'Riordan ST";
			validationResult.Apartment = "Unit 3A";
			validationResult.StreetNumber = "72";
			validationResult.Street = "O'Riordan ST";

			AddressValidationService.SetCoordinatesAndValidationStatus(address, validationResult);
			AssertEquals("AA1[0-6]SNA2[0-1]SA2[3-14]", address.AddressMap);
		}

		public void TestGetAddressMap()
		{
			Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU")).RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "AU";
			address.Address1 = "Unit 3A";
			address.Address2 = "72 O'Riordan ST";
			address.AddressMap = "AA1[0-6]SNA2[0-1]SA2[3-14]";

			AssertEquals("Unit 3A", AddressValidationService.GetApartment(address));
			AssertEquals("72", AddressValidationService.GetStreetNumber(address));
			AssertEquals("O'Riordan ST", AddressValidationService.GetStreet(address));
		}

		public void TestGetAddressMap_DoesNotThrowErrorWhenInvalid()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "AU";
			address.Address1 = "O'Riordan St";
			address.Address2 = string.Empty;
			address.AddressMap = "SNA1[0-0]SA1[2-20]";

			var street = string.Empty;
			AssertNoExceptionThrown(() => street = AddressValidationService.GetStreet(address));
			AssertEquals(string.Empty, street);

			address.AddressMap = "SA1[20-30]";
			AssertNoExceptionThrown(() => street = AddressValidationService.GetStreet(address));
			AssertEquals(string.Empty, street);

			address.Address1 = "Unit 3A";
			address.Address2 = "72 O'Riordan St";
			address.AddressMap = "AA1[0-20]SNA2[0-1]SA2[3-30]";
			var apartment = string.Empty;
			var streetNumber = string.Empty;
			AssertNoExceptionThrown(() => apartment = AddressValidationService.GetApartment(address));
			AssertNoExceptionThrown(() => streetNumber = AddressValidationService.GetStreetNumber(address));
			AssertNoExceptionThrown(() => street = AddressValidationService.GetStreet(address));
			CombineAssertions(() =>
			{
				AssertEquals(string.Empty, apartment);
				AssertEquals("72", streetNumber);
				AssertEquals(string.Empty, street);
			});

			address.Address1 = "72";
			address.Address2 = "O'Riordan St";
			address.AddressMap = "SNA1[0-10]SA2[0-30]";
			AssertNoExceptionThrown(() => streetNumber = AddressValidationService.GetStreetNumber(address));
			AssertNoExceptionThrown(() => street = AddressValidationService.GetStreet(address));
			CombineAssertions(() =>
			{
				AssertEquals(string.Empty, streetNumber);
				AssertEquals(string.Empty, street);
			});
		}

		public void TestHasTopRecommendedAddressReturnsTrueAsRequired()
		{
			var valResultItem = new ValidationResultItem()
			{
				ResultStatusCode = ValidationResultStatusCode.PointClose,
				AvailableData = AvailableData.StreetNumber
			};
			var result = new WebAddressValidationResult()
			{
				ResultAddress = valResultItem
			};

			AssertEquals("Old working case still works - PCL + Available Data = 3", true, result.HasTopRecommendedAddress);

			valResultItem.AvailableData = AvailableData.StreetNumberAndPrivateAddresses;
			AssertEquals("Old working case still works - PCL + Available Data = 4", true, result.HasTopRecommendedAddress);

			valResultItem.ResultStatusCode = ValidationResultStatusCode.StreetClose;
			valResultItem.AvailableData = AvailableData.Street;
			AssertEquals("Old working case still works - SCL + Available Data = 2", true, result.HasTopRecommendedAddress);

			valResultItem.ResultStatusCode = ValidationResultStatusCode.PointClose;
			valResultItem.AvailableData = AvailableData.NotChecked;
			AssertEquals("New working case - PCL + Available Data = 0", true, result.HasTopRecommendedAddress);

			valResultItem.AvailableData = AvailableData.NotAvailable;
			AssertEquals("New working case - PCL + Available Data = 1", true, result.HasTopRecommendedAddress);

			valResultItem.AvailableData = AvailableData.Street;
			AssertEquals("New working case - PCL + Available Data = 2", true, result.HasTopRecommendedAddress);
		}

		public void TestHasTopRecommendedAddressReturnsFalseAsRequired()
		{
			var valResultItem = new ValidationResultItem()
			{
				ResultStatusCode = ValidationResultStatusCode.StreetClose,
				AvailableData = AvailableData.NotChecked
			};
			var result = new WebAddressValidationResult()
			{
				ResultAddress = valResultItem
			};

			AssertEquals("SCL + Available Data = 0", false, result.HasTopRecommendedAddress);

			valResultItem.AvailableData = AvailableData.NotAvailable;
			AssertEquals("SCL + Available Data = 1", false, result.HasTopRecommendedAddress);

			valResultItem.AvailableData = AvailableData.StreetNumber;
			AssertEquals("SCL + Available Data = 3", false, result.HasTopRecommendedAddress);

			valResultItem.AvailableData = AvailableData.StreetNumberAndPrivateAddresses;
			AssertEquals("SCL + Available Data = 4", false, result.HasTopRecommendedAddress);

			valResultItem.ResultStatusCode = ValidationResultStatusCode.Invalid;
			AssertEquals("INV + Available Data = *", false, result.HasTopRecommendedAddress);
		}

		public void TestAvailableBackgroundWebServiceAddress()
		{
			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value => value.Background.ServiceUri = "https://_MOCK_AVS_BACKGROUND_URI_/", Factory))
			{
				AssertEquals("https://_MOCK_AVS_BACKGROUND_URI_/", AddressValidationService.AvailableBackgroundWebServiceAddress.Uri);
			}
		}

		public void TestSetSuggestedAddressToAddressForValidation_WhenSuggestedAddressFieldNull_ShouldSetRelatedAddressFieldEmpty()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "GB";
			address.OA_Address1 = "1 BARASSIE";
			address.OA_Address2 = string.Empty;
			address.OA_City = "GLASGOW";
			address.OA_PostCode = "G74 4SD";
			address.OA_State = "ISL";

			var resultItem = new ValidationResultItem
			{
				Country = "GB",
				Address1 = null,
				Address2 = null,
				City = null,
				Postcode = null,
				State = null
			};

			// Act.

			AddressValidationService.SetSuggestedAddressToAddressForValidation(resultItem, false, address);

			// Assert.

			AssertEquals("GB", address.OA_RN_NKCountryCode);
			AssertEquals(string.Empty, address.OA_Address1);
			AssertEquals(string.Empty, address.OA_Address2);
			AssertEquals(string.Empty, address.OA_City);
			AssertEquals(string.Empty, address.OA_PostCode);
			AssertEquals(string.Empty, address.OA_State);
		}

		public void TestSetSuggestedAddressToAddressForValidation_WhenSuggestedAddressValid_ShouldUpdateAddressAccordingly()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "AU";
			address.OA_Address1 = null;
			address.OA_Address2 = null;
			address.OA_City = null;
			address.OA_PostCode = null;
			address.OA_State = null;

			var resultItem = new ValidationResultItem
			{
				Country = "AU",
				Address1 = "72 O'RIORDAN ST",
				Address2 = string.Empty,
				City = "ALEXANDRIA",
				Postcode = "2015",
				State = "NSW"
			};

			// Act.

			AddressValidationService.SetSuggestedAddressToAddressForValidation(resultItem, false, address);

			// Assert.

			AssertEquals("AU", address.OA_RN_NKCountryCode);
			AssertEquals("72 O'RIORDAN ST", address.OA_Address1);
			AssertEquals(string.Empty, address.OA_Address2);
			AssertEquals("ALEXANDRIA", address.OA_City);
			AssertEquals("2015", address.OA_PostCode);
			AssertEquals("NSW", address.OA_State);
		}

		public void TestSetSuggestedAddressToAddressForValidation_WhenSuggestedAddressValidAndAddressIsTSAKnown_ShouldNotUpdateAddress()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "AU";
			address.OA_Address1 = "72 ORIORDAN ST";
			address.OA_Address2 = null;
			address.OA_City = "ALEXANDRIA";
			address.OA_PostCode = "2015";
			address.OA_State = "NSW";

			var orgCountryDataTSARecord = Factory.New<OrgCountryData>();
			orgCountryDataTSARecord.OV_OA_ApprovedLocation = address.PK;
			orgCountryDataTSARecord.OV_EXApprovedOrMajorExporter = "Yes";
			orgCountryDataTSARecord.OV_EXApprovalNumber = "1234";
			orgCountryDataTSARecord.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedStates;
			orgCountryDataTSARecord.OV_OH_OrgHeader = address.OA_OH;

			Factory.Save();

			var resultItem = new ValidationResultItem
			{
				Country = "AU",
				Address1 = "72 O'RIORDAN ST",
				Address2 = string.Empty,
				City = "ALEXANDRIA",
				Postcode = "2015",
				State = "NSW",
				ResultStatusCode = ValidationResultStatusCode.PointExact
			};

			// Act.

			AddressValidationService.SetSuggestedAddressToAddressForValidation(resultItem, false, address);

			// Assert.

			AssertEquals("72 ORIORDAN ST", address.OA_Address1);
			AssertEquals(AddressValidationStatus.Verified, address.ValidationStatus);
		}

		public void TestSetSuggestedAddressToAddressForValidation_WhenSuggestedAddressValidAndAddressIsMID_ShouldNotUpdateAddress()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "AU";
			address.OA_Address1 = "72 ORIORDAN ST";
			address.OA_Address2 = null;
			address.OA_City = "ALEXANDRIA";
			address.OA_PostCode = "2015";
			address.OA_State = "NSW";

			var orgCusCodeMIDRecord = Factory.New<OrgCusCode>();
			orgCusCodeMIDRecord.OK_OA_PremisesAddress = address.PK;
			orgCusCodeMIDRecord.OK_CodeType = "MID";
			orgCusCodeMIDRecord.OK_CustomsRegNo = "abcd1234";
			orgCusCodeMIDRecord.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			orgCusCodeMIDRecord.OK_OH = address.OA_OH;

			Factory.Save();

			var resultItem = new ValidationResultItem
			{
				Country = "AU",
				Address1 = "72 O'RIORDAN ST",
				Address2 = string.Empty,
				City = "ALEXANDRIA",
				Postcode = "2015",
				State = "NSW",
				ResultStatusCode = ValidationResultStatusCode.PointExact
			};

			// Act.

			AddressValidationService.SetSuggestedAddressToAddressForValidation(resultItem, false, address);

			// Assert.

			AssertEquals("72 ORIORDAN ST", address.OA_Address1);
			AssertEquals(AddressValidationStatus.Verified, address.ValidationStatus);
		}

		public void TestSetSuggestedAddressToAddressForValidation_WhenSuggestedStateMatchesDescription_ShouldUseStateCodeAccordingly()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "AU";
			address.OA_Address1 = null;
			address.OA_Address2 = null;
			address.OA_City = null;
			address.OA_PostCode = null;
			address.OA_State = null;

			var resultItem = new ValidationResultItem
			{
				Country = "AU",
				Address1 = "72 O'RIORDAN ST",
				Address2 = string.Empty,
				City = "ALEXANDRIA",
				Postcode = "2015",
				State = "New South Wales"
			};

			// Act.

			AddressValidationService.SetSuggestedAddressToAddressForValidation(resultItem, false, address);

			// Assert.

			AssertEquals("NSW", address.OA_State);

			address.OA_State = null;
			resultItem.State = "NEW SOUTH WALES";

			// Act.

			AddressValidationService.SetSuggestedAddressToAddressForValidation(resultItem, false, address);

			// Assert.

			AssertEquals("NSW", address.OA_State);

			address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "FI";
			address.OA_Address1 = null;
			address.OA_Address2 = null;
			address.OA_City = null;
			address.OA_PostCode = null;
			address.OA_State = "Uusimaa";

			resultItem = new ValidationResultItem
			{
				Country = "FI",
				Address1 = "Tietotie 11",
				Address2 = string.Empty,
				City = "Vantaa",
				Postcode = "01530",
				State = "Uusimaa"
			};

			// Act.

			AddressValidationService.SetSuggestedAddressToAddressForValidation(resultItem, false, address);

			// Assert.

			AssertEquals("18", address.OA_State);
		}

		public void TestSetSuggestedAddressToAddressForValidation_WhenSuggestedStateMatchesDescriptionWithDiacritics_ShouldUseStateCodeAccordingly()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "AU";
			address.OA_Address1 = null;
			address.OA_Address2 = null;
			address.OA_City = null;
			address.OA_PostCode = null;
			address.OA_State = null;

			var resultItem = new ValidationResultItem
			{
				Country = "AU",
				Address1 = "72 O'RIORDAN ST",
				Address2 = string.Empty,
				City = "ALEXANDRIA",
				Postcode = "2015",
				State = "Néw South Wàlés"
			};

			// Act.

			AddressValidationService.SetSuggestedAddressToAddressForValidation(resultItem, false, address);

			// Assert.

			AssertEquals("NSW", address.OA_State);
		}

		public void TestSetSuggestedAddressToAddressForValidation_WhenSuggestedStateMatchesDescriptionWithUUmlaut_ShouldUseStateCodeAccordingly()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "DE";
			address.OA_Address1 = null;
			address.OA_Address2 = null;
			address.OA_City = null;
			address.OA_PostCode = null;
			address.OA_State = null;

			var resultItem = new ValidationResultItem
			{
				Country = "DE",
				Address1 = "Arnulf-Klett-Platz 2",
				Address2 = string.Empty,
				City = "STUTTGART",
				Postcode = "70173",
				State = "Baden-Wuerttemberg"
			};

			// Act.

			AddressValidationService.SetSuggestedAddressToAddressForValidation(resultItem, false, address);

			// Assert.

			AssertEquals("BW", address.OA_State);
		}

		public void TestSetSuggestedAddressToAddressForValidation_WhenSuggestedStateMatchesDescriptionWithoutEnglish_ShouldUseStateCodeAccordingly()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "CN";
			address.OA_Language = "CHT";
			address.OA_Address1 = "ANY";
			address.OA_Address2 = "ANY";
			address.OA_City = "ANY";
			address.OA_PostCode = "ANY";
			address.OA_State = "ANY";

			var resultItem = new ValidationResultItem
			{
				Country = "CN",
				Address1 = "四川北路 1318",
				Address2 = "虹口区",
				City = "上海市",
				Postcode = "2000",
				State = "上海"
			};

			using (Res.TemporarilySwitchLanguage("CHT"))
			{
				var countryState = Factory.LoadTop1<RefCountryStates>(new ZQuery(RefCountryStatesSchema.RW_Description, "Shanghai"));
				if (countryState == null)
				{
					countryState = Factory.NewWithValidTestData<RefCountryStates>();
					countryState.RW_Description = "Shanghai";
				}

				var translateChinese = Factory.New<RefLanguageText>();
				translateChinese.RLT_ColumnName = "RW_Description";
				translateChinese.RLT_Language = "CHT";
				translateChinese.RLT_ParentId = countryState.PK;
				translateChinese.RLT_ParentTableCode = "RW";
				translateChinese.RLT_Text = "上海";

				Factory.Save();

				AddressValidationService.SetSuggestedAddressToAddressForValidation(resultItem, false, address);
			}

			AssertEquals("31", address.OA_State);
		}

		public void TestSetSuggestedAddressToAddressForValidation_WhenSuggestedStateMatchesDescriptionAfterRemovingDiacritics_ShouldUseStateCodeAccordingly()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "ES";
			address.OA_Address1 = null;
			address.OA_Address2 = null;
			address.OA_City = null;
			address.OA_PostCode = null;
			address.OA_State = null;

			var resultItem = new ValidationResultItem
			{
				Country = "ES",
				Address1 = "Paseo De Almeria 9",
				Address2 = string.Empty,
				City = "ALMERIA",
				Postcode = "04001",
				State = "ALMERIA"
			};

			// Act.

			AddressValidationService.SetSuggestedAddressToAddressForValidation(resultItem, false, address);

			// Assert.

			AssertEquals("AL", address.OA_State);
		}

		public void TestSetSuggestedAddressToAddressForValidation_WhenSuggestedStateDoesNotMatchesDescriptionWithDiacritics()
		{
			// Arrange.

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "AU";
			address.OA_Address1 = null;
			address.OA_Address2 = null;
			address.OA_City = null;
			address.OA_PostCode = null;
			address.OA_State = null;

			var resultItem = new ValidationResultItem
			{
				Country = "AU",
				Address1 = "72 O'RIORDAN ST",
				Address2 = string.Empty,
				City = "ALEXANDRIA",
				Postcode = "2015",
				State = "Néw Wàlés"
			};

			address.Language = Core.SharedConstants.Languages.French;

			// Act.

			AddressValidationService.SetSuggestedAddressToAddressForValidation(resultItem, false, address);

			// Assert.

			AssertEquals("Néw Wàlés", address.OA_State);

			address.OA_State = null;
			address.Language = Core.SharedConstants.Languages.EnglishAmerican;

			// Act.

			AddressValidationService.SetSuggestedAddressToAddressForValidation(resultItem, false, address);

			// Assert.

			AssertEquals("New Wales", address.OA_State);
		}

		public void TestSetSuggestedAddressToAddressForValidation_WhenGettingPointExactWithUpdatePrevention_ShouldNotUpdateAddressFields()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "[_MOCK_ADDRESS_1_]";
			address.OA_Address2 = "[_MOCK_ADDRESS_2_]";
			address.OA_City = "[_MOCK_CITY_]";
			address.OA_PostCode = "2000";
			address.OA_State = "NSW";
			address.OA_RN_NKCountryCode = "AU";

			var resultItem = new ValidationResultItem
			{
				Address1 = "[_VERIFIED_ADDRESS_1_]",
				Address2 = "[_VERIFIED_ADDRESS_2_]",
				City = "[_VERIFIED_CITY_]",
				Postcode = "3000",
				State = "VIC",
				Country = "AU",
				UnparsedAddressInformation = "[_MOCK_UNPARSED_INFORMATION_]"
			};

			AddressValidationService.SetSuggestedAddressToAddressForValidation(resultItem, false, address, true);

			AssertEquals("[_MOCK_ADDRESS_1_]", address.OA_Address1);
			AssertEquals("[_MOCK_ADDRESS_2_]", address.OA_Address2);
			AssertEquals("[_MOCK_CITY_]", address.OA_City);
			AssertEquals("2000", address.OA_PostCode);
			AssertEquals("NSW", address.OA_State);
			AssertEquals("AU", address.OA_RN_NKCountryCode);
			AssertEquals(string.Empty, address.OA_AdditionalAddressInformation);
		}

		public void TestSetSuggestedAddressToAddressForValidation_WhenGettingPointExactWithoutUpdatePrevention_ShouldUpdateAddressFields()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "[_MOCK_ADDRESS_1_]";
			address.OA_Address2 = "[_MOCK_ADDRESS_2_]";
			address.OA_City = "[_MOCK_CITY_]";
			address.OA_PostCode = "2000";
			address.OA_State = "NSW";
			address.OA_RN_NKCountryCode = "AU";

			var resultItem = new ValidationResultItem
			{
				Address1 = "[_VERIFIED_ADDRESS_1_]",
				Address2 = "[_VERIFIED_ADDRESS_2_]",
				City = "[_VERIFIED_CITY_]",
				Postcode = "3000",
				State = "VIC",
				Country = "AU",
				UnparsedAddressInformation = "[_MOCK_UNPARSED_INFORMATION_]"
			};

			AddressValidationService.SetSuggestedAddressToAddressForValidation(resultItem, false, address, false);

			AssertEquals("[_VERIFIED_ADDRESS_1_]", address.OA_Address1);
			AssertEquals("[_VERIFIED_ADDRESS_2_]", address.OA_Address2);
			AssertEquals("[_VERIFIED_CITY_]", address.OA_City);
			AssertEquals("3000", address.OA_PostCode);
			AssertEquals("VIC", address.OA_State);
			AssertEquals("AU", address.OA_RN_NKCountryCode);
			AssertEquals("[_MOCK_UNPARSED_INFORMATION_]", address.PrimaryOrgAddressAdditionalInfoDetail);
		}

		public void TestUpdateSuggestedStateWithoutNullExceptionWhenCountryIsNull()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "[_MOCK_ADDRESS_1_]";
			address.OA_Address2 = "[_MOCK_ADDRESS_2_]";
			address.OA_City = "[_MOCK_CITY_]";
			address.OA_PostCode = "2000";
			address.OA_State = "NSW";
			address.OA_RN_NKCountryCode = string.Empty;

			var resultItem = new ValidationResultItem
			{
				Address1 = "[_VERIFIED_ADDRESS_1_]",
				Address2 = "[_VERIFIED_ADDRESS_2_]",
				City = "[_VERIFIED_CITY_]",
				Postcode = "3000",
				State = "VIC",
				Country = "AU",
				UnparsedAddressInformation = "[_MOCK_UNPARSED_INFORMATION_]"
			};

			AssertNoExceptionThrown(() => AddressValidationService.SetSuggestedAddressToAddressForValidation(resultItem, false, address, false));

			AssertEquals("[_VERIFIED_ADDRESS_1_]", address.OA_Address1);
			AssertEquals("[_VERIFIED_ADDRESS_2_]", address.OA_Address2);
			AssertEquals("[_VERIFIED_CITY_]", address.OA_City);
			AssertEquals("3000", address.OA_PostCode);
			AssertEquals("NSW", address.OA_State);
			AssertEquals("", address.OA_RN_NKCountryCode);
			AssertEquals("[_MOCK_UNPARSED_INFORMATION_]", address.PrimaryOrgAddressAdditionalInfoDetail);
		}

		public void TestSetSuggestedAddressToAddressForValidation_WhenStateProvinceValidationRuleIsMustNotBeEnteredOrNot()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "AU";
			address.OA_Address1 = "1 BARASSIE";
			address.OA_Address2 = string.Empty;
			address.OA_City = "GLASGOW";
			address.OA_PostCode = "G74 4SD";
			address.OA_State = "ISL";

			var resultItem = new ValidationResultItem
			{
				Country = "AU",
				Address1 = null,
				Address2 = null,
				City = null,
				Postcode = null,
				State = "NSW"
			};

			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustNotBeEntered;

			AddressValidationService.SetSuggestedAddressToAddressForValidation(resultItem, false, address);
			AssertEquals("AU", address.OA_RN_NKCountryCode);
			AssertEquals(String.Empty, address.OA_State);

			address.OA_State = "ISL";
			AddressValidationService.SetSuggestedAddressToAddressForValidation(resultItem, true, address);
			AssertEquals("AU", address.OA_RN_NKCountryCode);
			AssertEquals("ISL", address.OA_State);

			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.NoValidationRule;

			AddressValidationService.SetSuggestedAddressToAddressForValidation(resultItem, false, address);
			AssertEquals("AU", address.OA_RN_NKCountryCode);
			AssertEquals("NSW", address.OA_State);

			AddressValidationService.SetSuggestedAddressToAddressForValidation(resultItem, true, address);
			AssertEquals("AU", address.OA_RN_NKCountryCode);
			AssertEquals("NSW", address.OA_State);
		}

		public void TestSetSuggestedCityTownToForm()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_RN_NKCountryCode = "AU";
			address.OA_Address1 = "1 BARASSIE";
			address.OA_Address2 = string.Empty;
			address.OA_City = "GLASGOW";
			address.OA_PostCode = "G74 4SD";
			address.OA_State = "ISL";

			var cityTown = new CandidateCityTown
			{
				City = "Sydney",
				Postcode = "2000",
				State = "NSW",
			};

			AssertEquals("Precondition", "ISL", address.State);

			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustNotBeEntered;

			AddressValidationService.SetSuggestedCityTownToForm(cityTown, address);
			AssertEquals(String.Empty, address.State);

			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.NoValidationRule;
			AddressValidationService.SetSuggestedCityTownToForm(cityTown, address);
			AssertEquals("NSW", address.OA_State);
		}

		public void TestWriteCorrectPostBodyToMessageWriterWhenCallValidateAddressAsync()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var cancellationToken = new CancellationTokenSource();
			Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU")).RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.Address1 = "702 O'Riordan St";
			address.Address2 = "";
			address.City = "Alexandria";
			address.State = "NSW";
			address.Postcode = "2015";
			address.OA_RN_NKCountryCode = "AU";

			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			// Arrange
			var messageWriter = new MessageWriterForTest();
			AddressValidationService.MessageWriter = messageWriter;
			AddressValidationService.SetAvailableWebServiceAddress(string.Format("http://localhost:{0}/addresscleansing/v2/", HttpServiceForTest.GetFreeTcpPort()));
			var serviceUri = new Uri(AddressValidationService.GetAvailableWebServiceAddressAsync().Result.Uri + AddressValidationService.Constants.ValidationServiceName + "/");
			var testExternalValidationService = new HttpServiceForTest
			{
				Delay = 0,
				Methods = new string[] { "POST" },
				Processor = (_, request) => new Tuple<int, string>(200, @"
					{""Items"":
						[
							{""ValidationResultItem"":{""AddressRecordGUID"":""b681b902-685a-4afc-85e2-c312c0b29f4b"",""AddressSourceTable"":""E2"",""Addressee"":null,""Address1"":""72 O'RIORDAN STREET"",""Address2"":null,""City"":""ALEXANDRIA"",""County"":null,""State"":""NSW"",""Postcode"":""2015"",""PostcodeAddOn"":null,""Country"":""AU"",""Apartment"":null,""StreetNumber"":""72"",""Street"":""O'RIORDAN STREET"",""Latitude"":-33.916565139125929,""Longitude"":151.19541258,""MatchCode"":""S8HPNTSCZG"",""MatchCodeFlag"":4106,""ChangeCount"":0,""LocationPrecision"":16,""ResultStatusCode"":""PCL"",""ServiceType"":1,""QueryType"":1,""AvailableData"":3,""AddressType"":8,""ErrorMessage"":null},
							 ""ProviderServiceCalls"":[],
							 ""Suggestions"":
								[
									{""AddressRecordGUID"":""b681b902-685a-4afc-85e2-c312c0b29f4b"",""AddressSourceTable"":""E2"",""Addressee"":null,""Address1"":""702 O Riordan St"",""Address2"":null,""City"":""Alexandria"",""County"":null,""State"":""NSW"",""Postcode"":""2015"",""PostcodeAddOn"":null,""Country"":""AU"",""Apartment"":null,""ChangeCount"":0,""StreetNumber"":null,""Street"":null,""MatchCode"":null,""MatchCodeFlag"":0,""LocationPrecision"":0,""ResultStatusCode"":null,""ServiceType"":0,""QueryType"":0,""AvailableData"":0,""AddressType"":0,""ErrorMessage"":null,""Group"":""O Riordan St,,Alexandria,NSW,AU,2015"",""Latitude"":0.0,""Longitude"":0.0}
								]
							}
						]
					}"),
				Uri = serviceUri,
				ContentType = "application/json"
			};

			try
			{
				testExternalValidationService.Start();
				Assert(String.Format("The test external validation service cannot be started on {0}.", serviceUri.AbsoluteUri), testExternalValidationService.IsStarted);
				// Act
				var result = AddressValidationService.ValidateAddressAsync(address, cancellationToken).Result;

				// Assert
				string expectBodyResult = string.Format("[{{\"AddressRecordGUID\":\"{0}\",\"AddressSourceTable\":\"{1}\",\"Addressee\":\"{2}\",\"Address1\":\"{3}\",\"Address2\":\"{4}\",\"City\":\"{5}\",\"Locality\":{6},\"State\":\"{7}\",\"Postcode\":\"{8}\",\"CountryCode\":\"{9}\",\"Group\":{10},\"Latitude\":{11},\"Longitude\":{12}", address.AddressRecordGUID, address.AddressSourceTable, address.Addressee, address.Address1, address.Address2, address.City, "null", address.State, address.Postcode, address.OA_RN_NKCountryCode, "null", "0.0", "0.0");

				AssertContains("Should write correct post body", expectBodyResult, messageWriter.MessageResult);
			}
			finally
			{
				if (testExternalValidationService.IsStarted)
				{
					testExternalValidationService.Stop();
				}
				AddressValidationService.SetAvailableWebServiceAddress();
				AddressValidationService.MessageWriter = null;
			}
		}

		public void TestGetAvailableWebServiceAddressAsyncNotThrowExceptionWhenDbConnectionFailure()
		{
			Db.Connection.Dispose();

			var messageWriter = new MessageWriterForTest();
			AddressValidationService.MessageWriter = messageWriter;
			AssertEquals(string.Empty, AddressValidationService.GetAvailableWebServiceAddressAsync().Result.Uri);
			AssertContains("Exception thrown while trying to get Address Validation WebService urls from registry item.", messageWriter.MessageResult);
		}

		public void TestValidateAddressAsyncWhenUrlsAreInvalid()
		{
			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value =>
			{
				value.Primary.ServiceUri = "https://invalid";
				value.Secondary.ServiceUri = "https://invalid";
			}, Factory))
			using (RawDataRegistry.Instance.AddressValidationWebServiceTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			{
				var address = Factory.NewWithValidTestData<OrgAddress>();
				var result = AddressValidationService.ValidateAddressAsync(address, null).Result;
				AssertEquals("None of the Address Validation services is available at the moment, please try again later. You can also increase the timeout value for calling the web service (current value is 10 seconds). If the issue persists please raise a Customer Service Incident.", result.Message);
			}
		}

		public void TestProcessAddressValidationResponseWithDetachedJobDocAddress()
		{
			var response = CreateHttpResponseTestData(out var result, out var resultItem);
			var detachedJobAddress = Factory.NewWithValidTestData<JobDocAddress>();
			detachedJobAddress.Delete();
			Assert("Precondition", ((INeedRow)detachedJobAddress).Row.RowState == DataRowState.Detached);

			AddressValidationService.ProcessAddressValidationResponse(detachedJobAddress, result, response, resultItem, null);
			AssertEquals("Should not have DeveloperNotificationException", ZString.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestProcessAddressValidationResponseWithDeletedJobDocAddress()
		{
			var response = CreateHttpResponseTestData(out var result, out var resultItem);
			var deletedJobAddress = Factory.NewWithValidTestData<JobDocAddress>();
			Factory.Save();
			deletedJobAddress.Delete();
			Assert("Precondition", ((INeedRow)deletedJobAddress).Row.RowState == DataRowState.Deleted);

			AddressValidationService.ProcessAddressValidationResponse(deletedJobAddress, result, response, resultItem, null);
			AssertEquals("Should not have DeveloperNotificationException", ZString.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestProcessAddressValidationResponseWithDetachedOrgAddress()
		{
			var response = CreateHttpResponseTestData(out var result, out var resultItem);
			var detachedOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
			detachedOrgAddress.Delete();
			Assert("Precondition", ((INeedRow)detachedOrgAddress).Row.RowState == DataRowState.Detached);

			AddressValidationService.ProcessAddressValidationResponse(detachedOrgAddress, result, response, resultItem, null);
			AssertEquals("Should not have DeveloperNotificationException", ZString.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestProcessAddressValidationResponseWithDeletedOrgAddress()
		{
			var response = CreateHttpResponseTestData(out var result, out var resultItem);
			var deletedOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();
			deletedOrgAddress.Delete();
			Assert("Precondition", ((INeedRow)deletedOrgAddress).Row.RowState == DataRowState.Deleted);

			AddressValidationService.ProcessAddressValidationResponse(deletedOrgAddress, result, response, resultItem, null);
			AssertEquals("Should not have DeveloperNotificationException", ZString.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestProcessAddressValidationResponseWithDetachedGlbStaff()
		{
			var response = CreateHttpResponseTestData(out var result, out var resultItem);
			var detachedGlbStaff = Factory.NewWithValidTestData<GlbStaff>();
			detachedGlbStaff.Delete();
			Assert("Precondition", ((INeedRow)detachedGlbStaff).Row.RowState == DataRowState.Detached);

			AddressValidationService.ProcessAddressValidationResponse(detachedGlbStaff, result, response, resultItem, null);
			AssertEquals("Should not have DeveloperNotificationException", ZString.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestProcessAddressValidationResponseWithDeletedGlbStaff()
		{
			var response = CreateHttpResponseTestData(out var result, out var resultItem);
			var deletedGlbStaff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			deletedGlbStaff.Delete();
			Assert("Precondition", ((INeedRow)deletedGlbStaff).Row.RowState == DataRowState.Deleted);

			AddressValidationService.ProcessAddressValidationResponse(deletedGlbStaff, result, response, resultItem, null);
			AssertEquals("Should not have DeveloperNotificationException", ZString.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestProcessAddressValidationResponse_WhenAddressLanguageIsEnglish_ShouldTransliterateResponseToEnglish()
		{
			const string testState = "Нижегоро́дская о́бласть";

			var response = CreateHttpResponseTestData(out var result, out var resultItem);
			var validationResult = resultItem.ValidationResultItem;
			validationResult.State = testState;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = org.PK;
			address.Language = Constants.Languages.English;

			AddressValidationService.ProcessAddressValidationResponse(address, result, response, resultItem, null);
			AssertEquals(WesternLanguageTransliterationHelper.TransliterateToEnglish(testState).ToUpper(), validationResult.State);
		}

		public void TestProcessAddressValidationResponse_WhenAddressLanguageIsNotEnglish_ShouldNotTransliterateResponseToEnglish()
		{
			const string testState = "Нижегоро́дская о́бласть";

			var response = CreateHttpResponseTestData(out var result, out var resultItem);
			var validationResult = resultItem.ValidationResultItem;
			validationResult.State = testState;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = org.PK;
			address.Language = Constants.Languages.Russian;

			AddressValidationService.ProcessAddressValidationResponse(address, result, response, resultItem, null);
			AssertEquals(testState.ToUpper(), validationResult.State);
		}

#pragma warning disable WTG2007
		public void TestProcessAddressValidationResponseBadRequest()
		{
			var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
			response.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "");
			response.ReasonPhrase = string.Empty;
			response.Content = new StringContent(@"{
    ""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.1"",
    ""title"": ""One or more validation errors occurred."",
    ""status"": 400,
    ""traceId"": ""00-d2651b39c81487d00d4607cfa0f7e8bd-bbb2a1d19dff6470-00"",
    ""errors"": {
        ""Items[0].CountryCode"": [
            ""The field CountryCode must be letters of length 2 or 3.""
        ]
    }
}");
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var result = new WebAddressValidationResult();
			var resultItem = new AddressCleansingResultItem();

			AddressValidationService.ProcessAddressValidationResponse(jobDocAddress, result, response, resultItem, null);
			AssertEquals(@"The remote server returned an error: (400)
One or more validation errors occurred.
Items[0].CountryCode: The field CountryCode must be letters of length 2 or 3.
", result.Message);
		}

		public void TestProcessAddressValidationResponseBadRequest_NoTitle()
		{
			var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
			response.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "");
			response.ReasonPhrase = string.Empty;
			response.Content = new StringContent(@"{
    ""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.1"",
    ""status"": 400,
    ""traceId"": ""00-d2651b39c81487d00d4607cfa0f7e8bd-bbb2a1d19dff6470-00"",
    ""errors"": {
        ""Items[0].CountryCode"": [
            ""The field CountryCode must be letters of length 2 or 3.""
        ]
    }
}");
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var result = new WebAddressValidationResult();
			var resultItem = new AddressCleansingResultItem();

			AddressValidationService.ProcessAddressValidationResponse(jobDocAddress, result, response, resultItem, null);
			AssertEquals(@"The remote server returned an error: (400)
Items[0].CountryCode: The field CountryCode must be letters of length 2 or 3.
", result.Message);
		}

		public void TestProcessAddressValidationResponseBadRequest_NoErrors()
		{
			var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
			response.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "");
			response.ReasonPhrase = "Bad request";
			response.Content = new StringContent(@"{
    ""title"": ""One or more validation errors occurred."",
    ""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.1"",
    ""status"": 400,
    ""traceId"": ""00-d2651b39c81487d00d4607cfa0f7e8bd-bbb2a1d19dff6470-00""
}");
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var result = new WebAddressValidationResult();
			var resultItem = new AddressCleansingResultItem();

			AddressValidationService.ProcessAddressValidationResponse(jobDocAddress, result, response, resultItem, null);
			AssertEquals(@"The remote server returned an error: (400)
One or more validation errors occurred.
", result.Message);
		}

		public void TestProcessAddressValidationResponseBadRequest_NoTitleErrors()
		{
			var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
			response.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "");
			response.ReasonPhrase = string.Empty;
			response.Content = new StringContent(@"{
    ""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.1"",
    ""status"": 400,
    ""traceId"": ""00-d2651b39c81487d00d4607cfa0f7e8bd-bbb2a1d19dff6470-00""
}");
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var result = new WebAddressValidationResult();
			var resultItem = new AddressCleansingResultItem();

			AddressValidationService.ProcessAddressValidationResponse(jobDocAddress, result, response, resultItem, null);
			AssertEquals(@"The remote server returned an error: (400)
{
    ""type"": ""https://tools.ietf.org/html/rfc7231#section-6.5.1"",
    ""status"": 400,
    ""traceId"": ""00-d2651b39c81487d00d4607cfa0f7e8bd-bbb2a1d19dff6470-00""
}
", result.Message);
		}

		public void TestProcessAddressValidationResponseBadRequest_NotJsonFormat()
		{
			var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
			response.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "");
			response.ReasonPhrase = string.Empty;
			response.Content = new StringContent("One or more validation errors occurred");
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var result = new WebAddressValidationResult();
			var resultItem = new AddressCleansingResultItem();

			AddressValidationService.ProcessAddressValidationResponse(jobDocAddress, result, response, resultItem, null);
			AssertEquals(@"The remote server returned an error: (400)
One or more validation errors occurred
", result.Message);
			AssertEquals("Error occurred when ProcessAddressValidationResponse", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
#pragma warning restore WTG2007

		public void TestProcessGetCityTownResponse_WhenAddressLanguageIsEnglish_ShouldTransliterateResponseToEnglish()
		{
			const string testState = "Моско́вская о́бласть";
			const string testCity1 = "Москва";
			const string testCity2 = "Клин";

			var mockResults = new List<CandidateCityTown>
			{
				new CandidateCityTown
				{
					State = testState,
					City = testCity1
				},
				new CandidateCityTown
				{
					State = testState,
					City = testCity2
				}
			};

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = org.PK;
			address.Language = Constants.Languages.English;

			AddressValidationService.ProcessGetCityTownResponse(address, new HttpResponseMessage(HttpStatusCode.OK), mockResults, null);
			CombineAssertions(() =>
			{
				AssertEquals(WesternLanguageTransliterationHelper.TransliterateToEnglish(testState), mockResults[0].State);
				AssertEquals(WesternLanguageTransliterationHelper.TransliterateToEnglish(testCity1).ToUpper(), mockResults[0].City);
				AssertEquals(WesternLanguageTransliterationHelper.TransliterateToEnglish(testState), mockResults[1].State);
				AssertEquals(WesternLanguageTransliterationHelper.TransliterateToEnglish(testCity2).ToUpper(), mockResults[1].City);
			});
		}

		public void TestProcessGetCityTownResponse_WhenAddressLanguageNotProvided_ShouldTransliterateResponseToEnglish()
		{
			const string testState = "Моско́вская о́бласть";
			const string testCity1 = "Москва";
			const string testCity2 = "Клин";

			var mockResults = new List<CandidateCityTown>
			{
				new CandidateCityTown
				{
					State = testState,
					City = testCity1
				},
				new CandidateCityTown
				{
					State = testState,
					City = testCity2
				}
			};

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = org.PK;
			address.Language = string.Empty;

			AddressValidationService.ProcessGetCityTownResponse(address, new HttpResponseMessage(HttpStatusCode.OK), mockResults, null);
			CombineAssertions(() =>
			{
				AssertEquals(WesternLanguageTransliterationHelper.TransliterateToEnglish(testState), mockResults[0].State);
				AssertEquals(WesternLanguageTransliterationHelper.TransliterateToEnglish(testCity1).ToUpper(), mockResults[0].City);
				AssertEquals(WesternLanguageTransliterationHelper.TransliterateToEnglish(testState), mockResults[1].State);
				AssertEquals(WesternLanguageTransliterationHelper.TransliterateToEnglish(testCity2).ToUpper(), mockResults[1].City);
			});
		}

		public void TestProcessGetCityTownResponse_WhenAddressLanguageIsNotEnglish_ShouldNotTransliterateResponseToEnglish()
		{
			const string testState = "Моско́вская о́бласть";
			const string testCity1 = "Москва";
			const string testCity2 = "Клин";

			var mockResults = new List<CandidateCityTown>
			{
				new CandidateCityTown
				{
					State = testState,
					City = testCity1
				},
				new CandidateCityTown
				{
					State = testState,
					City = testCity2
				}
			};

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = org.PK;
			address.Language = Constants.Languages.Russian;

			AddressValidationService.ProcessGetCityTownResponse(address, new HttpResponseMessage(HttpStatusCode.OK), mockResults, null);
			CombineAssertions(() =>
			{
				AssertEquals(testState, mockResults[0].State);
				AssertEquals(testCity1.ToUpper(), mockResults[0].City);
				AssertEquals(testState, mockResults[1].State);
				AssertEquals(testCity2.ToUpper(), mockResults[1].City);
			});
		}

		public void TestProcessGetCityTownResponse_ShouldNotExceptionWhenAddressIsDeleted()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.Delete();

			AssertNoExceptionThrown(() => AddressValidationService.ProcessGetCityTownResponse(address, new HttpResponseMessage(HttpStatusCode.OK), new List<CandidateCityTown>(), null));
		}

		public void TestValidateAddressAsyncWithDetachedOrDeletedAddress()
		{
			var deletedJobAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var deletedOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
			Factory.Save();

			deletedJobAddress.Delete();
			Assert("Precondition", ((INeedRow)deletedJobAddress).Row.RowState == DataRowState.Deleted);

			deletedOrgAddress.Delete();
			Assert("Precondition", ((INeedRow)deletedOrgAddress).Row.RowState == DataRowState.Deleted);

			var detachedJobAddress = Factory.NewWithValidTestData<JobDocAddress>();
			detachedJobAddress.Delete();
			Assert("Precondition", ((INeedRow)detachedJobAddress).Row.RowState == DataRowState.Detached);

			var detachedOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
			detachedOrgAddress.Delete();
			Assert("Precondition", ((INeedRow)detachedOrgAddress).Row.RowState == DataRowState.Detached);

			var addresses = new ISupportWebAddressValidation[] { deletedJobAddress, deletedOrgAddress, detachedOrgAddress, detachedJobAddress };
			AddressValidationService.SetAvailableWebServiceAddress($"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}/addresscleansing/v2/");
			try
			{
				var token = new CancellationTokenSource();
				CombineAssertions(() =>
				{
					foreach (var address in addresses)
					{
						AssertNoExceptionThrown("Should not throw Deleted/Detached exception.", () => AddressValidationService.ValidateAddressAsync(address, token).GetAwaiter().GetResult());
					}
				});
			}
			finally
			{
				AddressValidationService.SetAvailableWebServiceAddress();
			}
		}

		public void TestUseDbConnectionWithoutDisposableActionForDbConnection()
		{
			try
			{
				ErrorReporter.Clear();
				var thread = new Thread(() =>
				{
					AddressValidationService.GetAvailableWebServiceAddressSafe();
				});
				thread.Start();
				thread.Join();

				AssertEquals(ZString.Empty, ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestGetTranslatedLanguageCode()
		{
			AssertEquals("ENG", AddressValidationService.GetTranslatedLanguageCode(Core.SharedConstants.Languages.English));
			AssertEquals("EUS", AddressValidationService.GetTranslatedLanguageCode(Core.SharedConstants.Languages.EnglishAmerican));
			AssertEquals("EGB", AddressValidationService.GetTranslatedLanguageCode(Core.SharedConstants.Languages.EnglishBritish));
			AssertEquals("FR-FR", AddressValidationService.GetTranslatedLanguageCode(Core.SharedConstants.Languages.French));
			AssertEquals(string.Empty, AddressValidationService.GetTranslatedLanguageCode(null));
		}

		public void TestIsServiceAvailable_ThrowsNameResolutionException()
		{
			var messageWriter = new MessageWriterForTest();
			AddressValidationService.MessageWriter = messageWriter;
			var logger = new TestServiceLogger();
			using (RawDataRegistry.Instance.AddressValidationWebServiceTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 30))
			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value => value.Background.ServiceUri = "https://invalid.avsbackground.wisegrid.net/v2/", Factory))
			{
				AssertEquals(false, AddressValidationService.IsServiceAvailable(logger));

#if NETFRAMEWORK
				var expectedMessage = @"Inner Exception1(Type: System.Net.Http.HttpRequestException):
An error occurred while sending the request.
Inner Exception2(Type: System.Net.WebException):
The remote name could not be resolved: 'invalid.avsbackground.wisegrid.net'";
#else
				var expectedMessage = @"Inner Exception1(Type: System.Net.Http.HttpRequestException):
No such host is known. (invalid.avsbackground.wisegrid.net:443)
Inner Exception2(Type: System.Net.Sockets.SocketException):
No such host is known.";
#endif

				AssertContains("Should write error message", expectedMessage, messageWriter.MessageResult);
				AssertContains(expectedMessage, logger.ToString());
				AddressValidationService.MessageWriter = null;
			}
		}

		public void TestSuggestedPETOrPCLAddressDoesNotClearUserInputPostcodeIfCountryRuleIsMBE()
		{
			AssertPostcode(CountryAddressValidationRuleList.Codes.MustBeEntered, string.Empty, ValidationResultStatusCode.PointExact, "123456");
			AssertPostcode(CountryAddressValidationRuleList.Codes.MustBeEntered, string.Empty, ValidationResultStatusCode.PointClose, "123456");
			AssertPostcode(CountryAddressValidationRuleList.Codes.MustBeFormatted, string.Empty, ValidationResultStatusCode.PointExact, string.Empty);
			AssertPostcode(CountryAddressValidationRuleList.Codes.MustBeEntered, "ABC", ValidationResultStatusCode.PointExact, "ABC");
			AssertPostcode(CountryAddressValidationRuleList.Codes.MustBeEntered, string.Empty, ValidationResultStatusCode.StreetExact, string.Empty);
		}

		void AssertPostcode(string postcodeRule, string suggestedPostcode, string suggestedAddressStatusCode, string expectedPostcode)
		{
			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			au.RN_PostcodeValidationRule = postcodeRule;
			var suggestedAddress = new ValidationResultItem()
			{
				ResultStatusCode = suggestedAddressStatusCode,
				Postcode = suggestedPostcode
			};

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_RN_NKCountryCode = "AU";
			orgAddress.OA_PostCode = "123456";

			AddressValidationService.SetSuggestedAddressToAddressForValidation(suggestedAddress, false, orgAddress);
			AssertEquals(expectedPostcode, orgAddress.OA_PostCode);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var mockIProductRegistrationKey = new Mock<IProductRegistrationKey>();
			mockIProductRegistrationKey.Setup(x => x.SystemId).Returns("Fake-System-ID");
			mockIProductRegistrationKey.Setup(x => x.Password).Returns("Fake-Password");

			var mockProductRegistration = new Mock<IProductRegistration>();
			mockProductRegistration.Setup(x => x.Key).Returns(mockIProductRegistrationKey.Object);

			disposableMockedProductRegistration = ObjectFactory.Substitute(mockProductRegistration.Object);
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposableMockedProductRegistration?.Dispose();
		}

		static HttpResponseMessage CreateHttpResponseTestData(out WebAddressValidationResult result, out AddressCleansingResultItem resultItem)
		{
			var response = new HttpResponseMessage(HttpStatusCode.OK);

			var valResultItem = new ValidationResultItem()
			{
				ResultStatusCode = ValidationResultStatusCode.StreetClose,
				AvailableData = AvailableData.NotChecked
			};

			result = new WebAddressValidationResult()
			{
				ResultAddress = valResultItem
			};

			resultItem = new AddressCleansingResultItem
			{
				ValidationResultItem = new ValidationResultItem
				{
					UnparsedAddressInformation = "1. This is extreme-ly useful information! Don't throw it away..."
				}
			};

			return response;
		}

		IDisposable disposableMockedProductRegistration;
	}
}
