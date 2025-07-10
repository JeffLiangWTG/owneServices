using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.WebAddressValidation.Testing
{
	sealed class AddressSuggestionControlHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestShowSuggestedAddressesParentControlIsDisposed()
		{
			using (var form = new Form())
			using (var parentControl = new NameAndAddressDetailsUserControl())
			using (var contactDetailsGroupBox = new ZGroupBox())
			{
				var result = new WebAddressValidationResult();
				result.Message = null;
				result.ResultAddress = new ValidationResultItem();
				result.ResultAddress.ErrorMessage = null;

				var suggestions = new List<ValidationResultItem>();
				var suggestion = new ValidationResultItem();
				suggestions.Add(suggestion);

				result.SuggestedResults = suggestions;

				var address = Factory.New<OrgAddress>();

				Action refreshAction = () => { };
				Action handleDestroyedAction = null;

				parentControl.Dispose();

				var methodInfo = typeof(AddressSuggestionControlHelper).GetMethod(
					"ShowSuggestedAddresses",
					BindingFlags.NonPublic | BindingFlags.Static,
					null,
					new[]
					{
						typeof(WebAddressValidationResult),
						typeof(ISupportWebAddressValidation),
						typeof(ISupportWebAddressValidationControl),
						typeof(Form),
						typeof(Control.ControlCollection),
						typeof(Control),
						typeof(Action),
						typeof(Action),
						typeof(int),
						typeof(bool),
						typeof(int)
					},
					null);

				methodInfo.Invoke(
					null,
					new object[]
					{
						result,
						address,
						parentControl,
						form,
						contactDetailsGroupBox.Controls,
						null,
						refreshAction,
						handleDestroyedAction,
						0,
						true,
						0
					});
			}
		}

		[ExpectNoExceptions]
		public void TestShowSuggestedAddresses_WhenValidationResultIsNull()
		{
			using (var form = new Form())
			using (var parentControl = new NameAndAddressDetailsUserControl())
			using (var contactDetailsGroupBox = new ZGroupBox())
			{
				WebAddressValidationResult validationResult = null;

				var address = Factory.New<OrgAddress>();

				Action refreshAction = () => { };
				Action handleDestroyedAction = null;

				var control = AddressSuggestionControlHelper.ShowSuggestedAddresses(validationResult, address, parentControl, form, contactDetailsGroupBox.Controls, null, refreshAction, handleDestroyedAction, 0, true, 0);

				AssertNotNull(control);
				AssertEquals(control.GetType(), typeof(AddressSuggestionControl));
			}
		}

		public void TestShowSuggestedAddressesHasNoMessage_WhenResultAddressIsNull()
		{
			using (var form = new Form())
			using (var parentControl = new NameAndAddressDetailsUserControl())
			using (var contactDetailsGroupBox = new ZGroupBox())
			{
				WebAddressValidationResult validationResult = new WebAddressValidationResult();
				validationResult.ResultAddress = null;
				var address = Factory.New<OrgAddress>();

				AssertNoExceptionThrown(() =>
				{
					AddressSuggestionControlHelper.ShowSuggestedAddresses(validationResult, address, parentControl, form, contactDetailsGroupBox.Controls, null, () => { }, null, 0, true, 0);
				});

				AssertEquals("Should not show messages", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestShowSuggestedAddressesHasMessage_WhenResultAddressIsNotNull_AndHasErrorMessage()
		{
			using (var form = new Form())
			using (var parentControl = new NameAndAddressDetailsUserControl())
			using (var contactDetailsGroupBox = new ZGroupBox())
			{
				WebAddressValidationResult validationResult = new WebAddressValidationResult();
				validationResult.ResultAddress = new ValidationResultItem();
				validationResult.ResultAddress.ErrorMessage = "Test Message";
				var address = Factory.New<OrgAddress>();

				AssertNoExceptionThrown(() =>
				{
					AddressSuggestionControlHelper.ShowSuggestedAddresses(validationResult, address, parentControl, form, contactDetailsGroupBox.Controls, null, () => { }, null, 0, true, 0);
				});

				AssertEquals("Should show Information message", true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
			}
		}

		public void TestShowSuggestedAddressesDoesNotThrowExceptionIfHolderControlCollectionIsNull()
		{
			using (var form = new Form())
			using (var parentControl = new NameAndAddressDetailsUserControl())
			{
				var validationResult = new WebAddressValidationResult();
				validationResult.ResultAddress = null;
				var address = Factory.New<OrgAddress>();

				AssertNoExceptionThrown(() =>
				{
					AddressSuggestionControlHelper.ShowSuggestedAddresses(validationResult, address, parentControl, form, null, null, () => { }, null, 0, true);
				});
			}
		}

		public void TestValidateAddressAsync_WhenAddressHasBeenDeleted_PopulateClosestPort_ShouldNotThrowException()
		{
			using (var form = new Form())
			using (var parentControl = new NameAndAddressDetailsUserControl())
			using (var contactDetailsGroupBox = new ZGroupBox())
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;

				var address = Factory.NewWithValidTestData<DeleteOrgAddressForTest>();
				address.ValidationStatus = AddressValidationStatus.ToBeVerified;

				address.Address1 = "1A Roslyn Street";
				address.OA_RN_NKCountryCode = "AU";
				address.City = "SYDNEY";
				address.State = "NSW";
				address.ClosestPort = "AU###";

				AssertEquals("Precondition - ClosesPort", "AU###", address.ClosestPort);
				address.Postcode = "2011";

				Factory.Save();

				AssertNoExceptionThrown(async () =>
				{
					await AddressSuggestionControlHelper.ValidateAddressAsync(new CancellationTokenSource(), address, parentControl, form, contactDetailsGroupBox.Controls, null, null, 0, CleanseAction.QuickValidate);
				});

				parentControl.Dispose();
			}
		}

		public void TestNeedShowSuggestedAddressesWhenSwitchAddress()
		{
			var address = Factory.New<OrgAddress>();
			var addressSelected = Factory.New<OrgAddress>();

			var needShow = AddressSuggestionControlHelper.CheckNeedShowSuggestedAddresses(address, addressSelected);
			AssertEquals(false, needShow);

			var needShow2 = AddressSuggestionControlHelper.CheckNeedShowSuggestedAddresses(address, address);
			AssertEquals(true, needShow2);
		}

		public void TestValidateAddressAsync_WhenAddressHasBeenDeleted_ShouldNotThrowException()
		{
			using (var form = new Form())
			using (var parentControl = new NameAndAddressDetailsUserControl())
			using (var contactDetailsGroupBox = new ZGroupBox())
			{
				var deleteAddress = Factory.NewWithValidTestData<DeleteOrgAddressForTest>();
				deleteAddress.OA_CompanyNameOverride = "HB AL FUTTAIM UAE TRADING LLC";
				deleteAddress.OA_Code = "BOSS M STORE YAS MALL, AB";
				deleteAddress.Address1 = "BOSS M STORE,ABU DHABI,YAS MALL,HOF";
				deleteAddress.Address2 = "YAS MALL";
				deleteAddress.OA_RN_NKCountryCode = "AE";
				deleteAddress.City = "ABU DHABI";

				Factory.Save();

				AssertNoExceptionThrown(async () =>
				{
					await AddressSuggestionControlHelper.ValidateAddressAsync(new CancellationTokenSource(), deleteAddress, parentControl, form, contactDetailsGroupBox.Controls, null, null);
				});

				parentControl.Dispose();
			}
		}

		public void TestValidateAddressAsyncDoesNotThrowExceptionIfAddressIsNotBizO()
		{
			using (var form = new Form())
			using (var parentControl = new NameAndAddressDetailsUserControl())
			using (var contactDetailsGroupBox = new ZGroupBox())
			{
				var noBizOAddress = new SupportWebAddressValidation();
				AssertNoExceptionThrown(async () =>
				{
					await AddressSuggestionControlHelper.ValidateAddressAsync(new CancellationTokenSource(), noBizOAddress, parentControl, form, contactDetailsGroupBox.Controls, null, null);
				});
			}
		}

		[RequiresSTA]
		public void TestValidateAddressAsyncIfAddressValidationWebServiceHasErrors()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			var webAddress = string.Format("http://localhost:{0}/addresscleansing/v2/", HttpServiceForTest.GetFreeTcpPort());
			AddressValidationService.SetAvailableWebServiceAddress(webAddress);
			var serviceUri = new Uri(webAddress + AddressValidationService.Constants.ValidationServiceName + "/");

			var testExternalValidationService = new HttpServiceForTest
			{
				Delay = 0,
				Methods = new[] { "GET", "PUT", "POST" },
				Processor = (_, request) => new Tuple<int, string>(401, string.Empty),
				Uri = serviceUri,
				ContentType = "application/json"
			};

			try
			{
				testExternalValidationService.Start();

				using (var form = new Form())
				using (var parentControl = new NameAndAddressDetailsUserControl())
				using (var contactDetailsGroupBox = new ZGroupBox())
				{
					var address = Factory.NewWithValidTestData<OrgAddress>();
					address.OA_CompanyNameOverride = "HB AL FUTTAIM UAE TRADING LLC";
					address.OA_Code = "BOSS M STORE YAS MALL, AB";
					address.Address1 = "BOSS M STORE,ABU DHABI,YAS MALL,HOF";
					address.Address2 = "YAS MALL";
					address.OA_RN_NKCountryCode = "AE";
					address.City = "ABU DHABI";
					UnitTestUserNotification.Instance.ClearMessages();

					AddressSuggestionControlHelper.ValidateAddressAsync(new CancellationTokenSource(), address, parentControl, form, contactDetailsGroupBox.Controls, null, null).GetAwaiter().GetResult();
					AssertEquals("CargoWise Address Validation Web Service responses with errors. Please contact your I.T. person and confirm that the service Uris are up to date. The remote server returned an error: (401)", UnitTestUserNotification.Instance.LastMessage.Text);
				}
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

		public void TestErrorMessageIfAddressValidationHasErrors()
		{
			using (var form = new Form())
			using (var parentControl = new NameAndAddressDetailsUserControl())
			using (var contactDetailsGroupBox = new ZGroupBox())
			{
				var address = Factory.NewWithValidTestData<OrgAddress>();
				UnitTestUserNotification.Instance.ClearMessages();

				AddressSuggestionControlHelper.ValidateAddressAsync(new CancellationTokenSource(), address, parentControl, form, contactDetailsGroupBox.Controls, null, null).GetAwaiter().GetResult();
				AssertEquals("Please fix errors on address before running validation.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestIfAddressValidationHasErrors()
		{
			using (var parentControl = new NameAndAddressDetailsUserControl())
			{
				var address = Factory.NewWithValidTestData<OrgAddress>();
				AssertEquals(true, AddressSuggestionControlHelper.AddressValidationHasErrors(address, parentControl));

				address.State = "CA";
				AssertEquals(false, AddressSuggestionControlHelper.AddressValidationHasErrors(address, parentControl));
			}
		}

		public void TestShouldNotShowErrorMessage_WhenFactoryInTransactionAndAddressValidationHasErrors()
		{
			using (var parentControl = new NameAndAddressDetailsUserControl())
			{
				Factory.Saving += (factory) =>
				{
					var address = Factory.NewWithValidTestData<OrgAddress>();
					AssertEquals(true, AddressSuggestionControlHelper.AddressValidationHasErrors(address, parentControl));
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				};

				Factory.Save();
			}
		}

		#region TestQuickValidate

		public void TestQuickValidate_WhenAddressIsVerified_ShouldSetClosestPortIfEmpty()
		{
			QuickValidate_WhenAddressIsValid(AddressValidationStatus.Verified);
		}

		public void TestQuickValidate_WhenAddressIsVerifiedToStreet_ShouldSetClosestPortIfEmpty()
		{
			QuickValidate_WhenAddressIsValid(AddressValidationStatus.VerifiedToStreet);
		}

		void QuickValidate_WhenAddressIsValid(string validationStatus)
		{
			var enableAVWebServiceOrigValue = Env.Instance.Registry.EnableAddressValidationWebService;

			using (OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.EnableDefaultingClosestPortOnAddressValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new Form())
			using (var parentControl = new NameAndAddressDetailsUserControl())
			using (var contactDetailsGroupBox = new ZGroupBox())
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;

				var address = Factory.NewWithValidTestData<OrgAddressForClosestPortTest>();
				address.ValidationStatus = AddressValidationStatus.ToBeVerified;

				address.Address1 = "1318 NORTH SICHUAN ROAD";
				address.OA_RN_NKCountryCode = "CN";
				address.City = "SHANGHAI";
				address.State = "31";

				AssertEquals("Precondition - ClosesPort is empty", true, string.IsNullOrWhiteSpace(address.ClosestPort));
				address.Postcode = "200085";

				address.ValidAddressValidationStatusToBeReturned = validationStatus;
				AssertNoExceptionThrown(async () =>
				{
					await AddressSuggestionControlHelper.ValidateAddressAsync(new CancellationTokenSource(), address, parentControl, form, contactDetailsGroupBox.Controls, null, null, 0, CleanseAction.QuickValidate);
				});
				CombineAssertions(() =>
				{
					AssertEquals("Validation status", validationStatus, address.ValidationStatus);
					AssertEquals("ClosesPort should not be empty", false, string.IsNullOrWhiteSpace(address.ClosestPort));
				});
			}

			Env.Instance.Registry.EnableAddressValidationWebService = enableAVWebServiceOrigValue;
		}

		public void TestQuickValidate_WhenAddressIsValid_ShouldNotSetClosestPortIfNotEmpty()
		{
			var enableAVWebServiceOrigValue = Env.Instance.Registry.EnableAddressValidationWebService;

			using (OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.EnableDefaultingClosestPortOnAddressValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new Form())
			using (var parentControl = new NameAndAddressDetailsUserControl())
			using (var contactDetailsGroupBox = new ZGroupBox())
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;

				var address = Factory.NewWithValidTestData<OrgAddressForClosestPortTest>();
				address.ValidationStatus = AddressValidationStatus.ToBeVerified;

				address.Address1 = "1318 NORTH SICHUAN ROAD";
				address.OA_RN_NKCountryCode = "CN";
				address.City = "SHANGHAI";
				address.State = "31";
				address.ClosestPort = "CN###";

				AssertEquals("Precondition - ClosesPort", "CN###", address.ClosestPort);
				address.Postcode = "200085";

				AssertNoExceptionThrown(async () =>
				{
					await AddressSuggestionControlHelper.ValidateAddressAsync(new CancellationTokenSource(), address, parentControl, form, contactDetailsGroupBox.Controls, null, null, 0, CleanseAction.QuickValidate);
				});
				CombineAssertions(() =>
				{
					AssertEquals("Validation status should be verified to street", AddressValidationStatus.VerifiedToStreet, address.ValidationStatus);
					AssertEquals("ClosesPort should not change", "CN###", address.ClosestPort);
				});
			}

			Env.Instance.Registry.EnableAddressValidationWebService = enableAVWebServiceOrigValue;
		}

		public void TestQuickValidate_WhenAddressIsNotValid_ShouldNotSetClosestPort()
		{
			var enableAVWebServiceOrigValue = Env.Instance.Registry.EnableAddressValidationWebService;

			using (OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.EnableDefaultingClosestPortOnAddressValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new Form())
			using (var parentControl = new NameAndAddressDetailsUserControl())
			using (var contactDetailsGroupBox = new ZGroupBox())
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;

				var address = Factory.NewWithValidTestData<OrgAddressForClosestPortTest>();
				address.ValidationStatus = AddressValidationStatus.ToBeVerified;

				address.Address1 = "XXX STREET";
				address.OA_RN_NKCountryCode = "CN";
				address.City = "SHANGHAI";
				address.State = "XX";

				address.IsValidAddress = false;
				AssertEquals("Precondition - ClosesPort is empty", true, string.IsNullOrWhiteSpace(address.ClosestPort));
				address.Postcode = "200085";

				AssertNoExceptionThrown(async () =>
				{
					await AddressSuggestionControlHelper.ValidateAddressAsync(new CancellationTokenSource(), address, parentControl, form, contactDetailsGroupBox.Controls, null, null, 0, CleanseAction.QuickValidate);
				});
				CombineAssertions(() =>
				{
					AssertEquals("Validation status should be invalid", AddressValidationStatus.Invalid, address.ValidationStatus);
					AssertEquals("ClosesPort should be empty", true, string.IsNullOrWhiteSpace(address.ClosestPort));
				});
			}

			Env.Instance.Registry.EnableAddressValidationWebService = enableAVWebServiceOrigValue;
		}

		#endregion

		class SupportWebAddressValidation : ISupportWebAddressValidation
		{
			public SupportWebAddressValidation()
			{
				TriggerWebAddressValidation?.Invoke(null, null);
				AddressValidationStatusChanged?.Invoke(null, null);
				TriggerWebGetCityTown?.Invoke(null, null);
			}

			public ZString Language { get; set; }
			public ZPropertyInfo LanguageInfo { get; }
			public int Language_MaxLength { get; }
			public CodeDescriptionPairList LanguageList { get; }
			public ZString UnrestrictedAdditionalAddressInformation { get; set; }
			public ZPropertyInfo UnrestrictedAdditionalAddressInformationInfo { get; }
			public CodeDescriptionPairList AdditionalAddressInfoList { get; }
			public ZString AddressCode { get; set; }
			public ZString Address1 { get; set; }
			public ZPropertyInfo Address1Info { get; }
			public int Address1_MaxLength { get; }
			public ZString Address2 { get; set; }
			public ZPropertyInfo Address2Info { get; }
			public int Address2_MaxLength { get; }
			public ZString City { get; set; }
			public ZPropertyInfo CityInfo { get; }
			public int City_MaxLength { get; }
			public ZString Postcode { get; set; }
			public ZPropertyInfo PostcodeInfo { get; }
			public int Postcode_MaxLength { get; }
			public ZString CompanyName { get; set; }
			public ZPropertyInfo CompanyNameInfo { get; }
			public int CompanyName_MaxLength { get; }
			public ZString StateCode { get; set; }
			public ZPropertyInfo StateCodeInfo { get; }
			public int StateCode_MaxLength { get; }
			public CodeDescriptionPairList StateCodeList { get; }
			public ZString State { get; set; }
			public int State_MaxLength { get; }
			public ZString CountryCodeISO2 { get; set; }
			public int CountryCodeISO2_MaxLength { get; }
			public RefCountry Country { get; }
			public RefCountryCollection CountryCodeList { get; }
			public ZString DisplayText { get; set; }
			public ZGuid EntityPK { get; }
			public bool GetReadOnlySecurity(PropertyDescriptor property) => true;
			public ZString AddressRecordGUID { get; }
			public ZString AddressSourceTable { get; }
			public ZString ValidationStatus { get; set; }
			public ZString AddressMap { get; set; }
			public ZString Addressee { get; }
			public ZGeography GeoLocation { get; set; }
			public ZString ClosestPort { get; set; }
			public bool NeedValidation { get; }
			public bool IsUpdatingCityTown { get; set; }
			public bool IsValidatingAddress { get; set; }
			public bool IsExactPointFound { get; set; }
			public bool IsErrorSuppressed { get; }
			public bool IsTSAKnownAddress { get; }
			public bool IsMIDAddress { get; }
			public bool IsJobDocAddress { get; }
			public bool IsRowDeletedOrDetachedOrNull { get; }
			public bool IsValidatedByBackgroundService { get; set; }

			public event EventHandler TriggerWebAddressValidation;
			public event EventHandler AddressValidationStatusChanged;
			public event EventHandler TriggerWebGetCityTown;
			public Task<WebAddressValidationResult> ValidateAddressAsync(
			  CancellationTokenSource cancellationToken,
			  CleanseAction cleanseAction = CleanseAction.ValidateAndSuggest) => Task.FromResult(new WebAddressValidationResult());
			public Task<CandidateCityTown[]> GetCityTownAsync(
				CancellationTokenSource cancellationToken) => Task.FromResult(Array.Empty<CandidateCityTown>());
			public void ResetValidationStatus(ZPropertyInfo propertyInfo) { }
			public void PreValidationForAddressValidationService() { }
			public void ValidatePostcodeAndStateForAddress() { }
			public void ClearWebAddressValidationHandler() { }
			public void ClearWebGetCityTownHandler() { }
		}

		class DeleteOrgAddressForTest : OrgAddress, ISupportWebAddressValidation
		{
			public DeleteOrgAddressForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			public new Task<WebAddressValidationResult> ValidateAddressAsync(CancellationTokenSource cancellationToken, CleanseAction cleanseAction = CleanseAction.ValidateAndSuggest)
			{
				Delete();

				return Task.FromResult(new WebAddressValidationResult());
			}
		}

		class OrgAddressForClosestPortTest : OrgAddress, ISupportWebAddressValidation
		{
			internal bool IsValidAddress { get; set; } = true;
			internal string ValidAddressValidationStatusToBeReturned { get; set; } = AddressValidationStatus.VerifiedToStreet;

			public OrgAddressForClosestPortTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new Task<WebAddressValidationResult> ValidateAddressAsync(CancellationTokenSource cancellationToken, CleanseAction cleanseAction = CleanseAction.QuickValidate)
			{
				var resultItem = new ValidationResultItem()
				{
					Address1 = this.Address1,
					Address2 = this.Address2,
					Country = this.Country.Code,
					City = this.City,
					State = this.State,
					Postcode = this.Postcode
				};

				if (IsValidAddress)
				{
					resultItem.Latitude = 31.254465;
					resultItem.Longitude = 121.4799;
					resultItem.MatchCode = "S4-PNTSCZAS";
					resultItem.MatchCodeFlag = 2166802;
					if (ValidAddressValidationStatusToBeReturned == AddressValidationStatus.VerifiedToStreet)
					{
						resultItem.ResultStatusCode = "SET";
						this.ValidationStatus = AddressValidationStatus.VerifiedToStreet;
					}
					else
					{
						resultItem.ResultStatusCode = "SCL";
						this.ValidationStatus = AddressValidationStatus.Verified;
					}
					this.GeoLocation = ZGeography.CreatePoint(resultItem.Longitude, resultItem.Latitude);
				}
				else
				{
					resultItem.ResultStatusCode = "INV";
					this.ValidationStatus = AddressValidationStatus.Invalid;
				}

				var validationResult = new WebAddressValidationResult()
				{
					ResultAddress = resultItem
				};

				return Task.FromResult(validationResult);
			}
		}
	}
}
