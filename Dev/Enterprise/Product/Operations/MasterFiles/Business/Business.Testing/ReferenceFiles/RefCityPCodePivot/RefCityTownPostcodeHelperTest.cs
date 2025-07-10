using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefCityTownPostcodeHelperTest : TestCaseWithFactory
	{
		public void TestGetCityTownPKFromCodeWith3Parameters()
		{
			using (SystemDataRegistry.Instance.DisableNonVerifiableCityTownWarning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var helper = new RefCityTownPostcodeHelperForTest(new RefPostCodeCollection(Factory), "US", "", "", "");
				helper.GetCityTownPKFromCode("BigTe$$$stCity", "101$$$909", "BK");
				var newCity = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, "BigTe$$$stCity"));
				var newPostcode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "101$$$909"));

				AssertCollectionContains(newCity, newPostcode.CityTowns);
				AssertCollectionContains(newPostcode, newCity.PostCodes);

				Assert(!newCity.R9_IsSystem);
				Assert(!newPostcode.RK_IsSystem);

				AssertEquals("BK", newCity.R9_RW_NKState);
			}
		}

		public void TestPostcodeStripMatch()
		{
			var helper = new RefCityTownPostcodeHelperForTest(new RefPostCodeCollection(Factory), "US", "", "", "");
			var postcode = Factory.New<RefPostCode>();
			postcode.RK_RN_NKCountry = "US";
			postcode.RK_CityTownPostCode = "ABC 123";
			Factory.Save();
			helper.CandidateCityTownForTest = new[] { new CandidateCityTown() { City = "BigCity", Postcode = "ABC 123", State = "AB" } };
			var guid = helper.GetPostcodePKFromCode("A\\B C -//12-3");
			AssertEquals(postcode.PK, guid);
		}

		public void TestGetPostcodePKFromNonExistCode()
		{
			var helper = new RefCityTownPostcodeHelperForTest(new RefPostCodeCollection(Factory), "US", "", "", "");
			helper.CandidateCityTownForTest = new[] { new CandidateCityTown() { City = "BigCity", Postcode = "66666", State = "AB" } };
			var postcodePK = helper.GetPostcodePKFromCode("66666");
			Assert(postcodePK.IsValid);
			var city = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, "BigCity")
				.AddToFilter(RefCityTownSchema.R9_RW_NKState, "AB"));
			AssertNotNull("BigCity saved in RefCityTown", city);
			Assert("city created by system", city.R9_IsSystem);

			var postcode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "66666"));

			AssertNotNull("66666 saved in RefPostCode", postcode);
			Assert("postcode created by system", postcode.RK_IsSystem);

			var pivot = Factory.LoadTop1<RefCityPCodePivot>(new ZQuery(RefCityPCodePivotSchema.R0_RK, postcodePK));
			AssertNotNull("Pivot saved in RefCityPCodePivot", pivot);
			Assert("pivot created by system", pivot.R0_IsSystem);
		}

		public void TestGetPostcodePKFromNonExistCode_InactiveCityTown()
		{
			var cityTown = Factory.New<RefCityTown>();
			cityTown.R9_InternationalName = "BigCity";
			cityTown.R9_RN_NKCountry = "US";
			cityTown.R9_RW_NKState = "AB";
			cityTown.R9_IsActive = false;
			cityTown.R9_IsSystem = true;
			Factory.Save();

			Assert("BigCity saved in RefCityTown", cityTown.IsInDatabase);
			var helper = new RefCityTownPostcodeHelperForTest(new RefPostCodeCollection(Factory), "US", "", "", "");
			helper.CandidateCityTownForTest = new[] { new CandidateCityTown() { City = "BigCity", Postcode = "66666", State = "AB" } };
			var postcodePK = helper.GetPostcodePKFromCode("66666");
			Assert(postcodePK.IsValid);
			var city = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, "BigCity")
				.AddToFilter(RefCityTownSchema.R9_RW_NKState, "AB"));

			var pivot = Factory.LoadTop1<RefCityPCodePivot>(new ZQuery(RefCityPCodePivotSchema.R0_RK, postcodePK));
			AssertNotNull("Pivot saved in RefCityPCodePivot", pivot);
			Assert("pivot created by system", pivot.R0_IsSystem);

			cityTown = Factory.Load<RefCityTown>(cityTown.PK);
			Assert("CityTown is automatically re-activated.", cityTown.R9_IsActive);
		}

		public void TestGetPostcodePKFromExistCodeAndHasDuplicates()
		{
			var helper = new RefCityTownPostcodeHelperForTest(new RefPostCodeCollection(Factory), "US", "", "", "");
			helper.CandidateCityTownForTest = new[] { new CandidateCityTown() { City = "BigCity", Postcode = "66666", State = "AB" } };
			helper.UserInteraction.SelectionNeeded += delegate
			{
				Fail("Postcode should be unique in one Country, so should not get here!!!");
			};

			for (int i = 0; i < 3; i++)
			{
				var postcode = Factory.New<RefPostCode>();
				postcode.RK_CityTownPostCode = "66666";
				postcode.RK_IsActive = true;
				postcode.RK_IsSystem = true;
				postcode.RK_RN_NKCountry = "US";
			}
			Factory.Save();

			helper.GetPostcodePKFromCode("66666");
			Assert(true);
		}

		public void TestGetPostcodePKFromExistCodeAndIsInactive()
		{
			using (SystemDataRegistry.Instance.DisableNonVerifiablePostcodeWarning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var postcode = Factory.New<RefPostCode>();
				postcode.RK_CityTownPostCode = "POSTCODE";
				postcode.RK_IsActive = false;
				postcode.RK_IsSystem = true;
				postcode.RK_RN_NKCountry = "US";
				Factory.Save();

				var helper = new RefCityTownPostcodeHelperForTest(null, "US", "", "", "");
				helper.CandidateCityTownForTest = null;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

				var newPostCodePk = helper.GetPostcodePKFromCode("POSTCODE");
				Assert(newPostCodePk.IsValid);
				AssertNotEquals("The existed postcode is In-Active so that it will create a new one.", postcode.PK, newPostCodePk);

				var postcodes = Factory.Load<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "POSTCODE"));
				Assert(postcodes.Any(x => x.RK_CityTownPostCode == "POSTCODE" && x.RK_IsActive && x.PK == newPostCodePk));
				Assert(postcodes.Any(x => x.RK_CityTownPostCode == "POSTCODE" && !x.RK_IsActive && x.PK == postcode.PK));
			}
		}

		public void TestGetCityTownPKFromCode()
		{
			var helper = new RefCityTownPostcodeHelperForTest(new RefCityTownCollection(Factory), "US", "", "", "");
			helper.CandidateCityTownForTest = new[] { new CandidateCityTown() { City = "BigCity", Postcode = "66666", State = "AB" } };
			var cityTownPK = helper.GetCityTownPKFromCode("BigCity");
			Assert(cityTownPK.IsValid);
			var city = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, "BigCity")
				.AddToFilter(RefCityTownSchema.R9_RW_NKState, "AB"));
			AssertNotNull("BigCity saved in RefCityTown", city);
			Assert("city created by system", city.R9_IsSystem);

			var postcode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "66666"));

			AssertNotNull("66666 saved in RefPostCode", postcode);
			Assert("postcode created by system", postcode.RK_IsSystem);

			var pivot = Factory.LoadTop1<RefCityPCodePivot>(new ZQuery(RefCityPCodePivotSchema.R0_R9, cityTownPK));
			AssertNotNull("Pivot saved in RefCityPCodePivot", pivot);
			Assert("pivot created by system", pivot.R0_IsSystem);
		}

		public void TestGetCityTownPKFromCode_WithAccentedCity()
		{
			var helper = new RefCityTownPostcodeHelperForTest(new RefCityTownCollection(Factory), "CA", "", "", "");
			helper.CandidateCityTownForTest = new[] { new CandidateCityTown() { City = "BIG MONTREAL", Postcode = "H4Z 0A2", State = "QC" } };

			var cityTownPK = helper.GetCityTownPKFromCode("BIG MONTRÉAL");

			var city = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, "BIG MONTREAL")
					.AddToFilter(RefCityTownSchema.R9_RW_NKState, "QC"));
			AssertNotNull("BigMontreal saved in RefCityTown", city);
			Assert("City created by system", city.R9_IsSystem);
			AssertEquals("City name", "BIG MONTREAL", city.R9_InternationalName);

			Assert("cityTownPK should be valid", cityTownPK.IsValid);
		}

		public void TestGetPostcodePKFromNonExistCodeWithConfirmationYes()
		{
			using (SystemDataRegistry.Instance.DisableNonVerifiablePostcodeWarning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var helper = new RefCityTownPostcodeHelperForTest(null, "US", "", "", "");
				helper.CandidateCityTownForTest = null;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

				var postcodePK = helper.GetPostcodePKFromCode("POSTCODE");
				Assert(postcodePK.IsValid);
				var postcode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "POSTCODE"));
				AssertContains("Would you like to add this to reference data?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull("postcode saved in RefCityTown", postcode);
				Assert("postcode not created by system", !postcode.RK_IsSystem);

				helper = new RefCityTownPostcodeHelperForTest(null, "US", "", "BigCity", "AB");
				helper.CandidateCityTownForTest = null;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

				helper.GetPostcodePKFromCode("ZIPCODE");
				var city = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, "BigCity"));

				AssertContains("Would you like to add this to reference data?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull("BigCity saved in RefCityTown", city);
				Assert("city not created by system", !city.R9_IsSystem);
			}
		}

		public void TestGetPostcodePKFromNonExistCodeWhenIgnorePostCodeWarningOnTransportZoneSetsIsTrue()
		{
			using (SystemDataRegistry.Instance.DisableNonVerifiablePostcodeWarning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var helper = new RefCityTownPostcodeHelperForTest(null, "US", "", "", "");
				helper.CandidateCityTownForTest = null;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var postCodeString = "POSTCODE";
				AssertEquals("Precondition: postcode length is allowed", true, postCodeString.Length <= RefPostCodeSchema.RK_CityTownPostCode.MaxLength);

				var postcodePK = helper.GetPostcodePKFromCode(postCodeString);
				AssertEquals("Precondition: GetPostcodePKFromCode returned a valid PK", true, postcodePK.IsValid);

				var postcode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, postCodeString));
				AssertNotContains("Would you like to add this to reference data?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull("postcode saved in RefCityTown", postcode);
				Assert("postcode not created by system", !postcode.RK_IsSystem);
			}
		}

		public void TestGetPostcodePKFromNonExistCodeNotCreatedWhenCodeMoreThanMaxLength()
		{
			using (SystemDataRegistry.Instance.DisableNonVerifiablePostcodeWarning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var helper = new RefCityTownPostcodeHelperForTest(null, "US", "", "", "");
				helper.CandidateCityTownForTest = null;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var postCodeString = "POSTCODE_IS_TOO_LONG";
				AssertEquals("Precondition: postcode length is longer than the max length allowed", false, postCodeString.Length <= RefPostCodeSchema.RK_CityTownPostCode.MaxLength);

				var postcodePK = helper.GetPostcodePKFromCode(postCodeString);
				AssertEquals("Precondition: GetPostcodePKFromCode returned an invalid PK", false, postcodePK.IsValid);

				var postcode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, postCodeString));
				AssertNull("postcode not saved in RefCityTown", postcode);
			}
		}

		public void TestGetCityTownPKFromNonExistCodeWithConfirmationYes()
		{
			var helper = new RefCityTownPostcodeHelperForTest(null, "US", "", "", "");
			helper.CandidateCityTownForTest = null;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

			var cityTownPK = helper.GetCityTownPKFromCode("BigCity");
			Assert(cityTownPK.IsValid);
			var city = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, "BigCity"));
			AssertNotNull("BigCity saved in RefCityTown", city);
			Assert("city not created by system", !city.R9_IsSystem);

			helper = new RefCityTownPostcodeHelperForTest(null, "US", "66666", "", "");
			helper.CandidateCityTownForTest = null;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

			helper.GetCityTownPKFromCode("SmallCity");
			var postcode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "66666"));

			AssertNotNull("66666 saved in RefPostCode", postcode);
			Assert("postcode not created by system", !postcode.RK_IsSystem);
		}

		public void TestPostcodeTownPKFromNonExistCodeWithConfirmationNo()
		{
			var helper = new RefCityTownPostcodeHelperForTest(null, "US", "", "", "");
			helper.CandidateCityTownForTest = null;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);

			var postcodePK = helper.GetPostcodePKFromCode("POSTCODE");
			Assert(!postcodePK.IsValid);
		}

		public void TestGetCityTownPKFromNonExistCodeWithConfirmationNo()
		{
			var helper = new RefCityTownPostcodeHelperForTest(null, "US", "", "", "");
			helper.CandidateCityTownForTest = null;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);

			var cityTownPK = helper.GetCityTownPKFromCode("BigCity");
			Assert(!cityTownPK.IsValid);
		}

		public void TestPostcodeStripMatch_WithInvalidValues()
		{
			var helper = new RefCityTownPostcodeHelper(new RefPostCodeCollection(Factory), "US", "", "", "");

			AddressValidationService.SetAvailableWebServiceAddress();

			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value =>
			{
				value.Primary.ServiceUri = "https://invalid";
				value.Secondary.ServiceUri = "https://invalid";
			}, Factory))
			{
				var webServiceAddress = AddressValidationService.GetAvailableWebServiceAddressAsync().Result;
				AssertEquals("Invalid webservice address generates null result", string.Empty, webServiceAddress.Uri);
				AssertNoExceptionThrown(() => helper.GetCityTownPKFromCode("NULL"));
			}
		}

		public void TestConfirmationText()
		{
			var helper = new RefCityTownPostcodeHelper(null, "US", "", "", "");
			helper.ConfirmWithUser("Blah");
			var lastMessage = UnitTestUserNotification.Instance.LastMessage;
			AssertEquals("Warning", lastMessage.Caption);
			AssertEquals(@"Blah, US not found. Would you like to add this to reference data?
Note: If you choose not to add this you will not be able to save this row.", lastMessage.Text);
		}

		public void TestConfirmWithUser_NoPostCode_NoCityTown()
		{
			var postCode = Factory.New<RefPostCode>();
			postCode.RK_CityTownPostCode = "DUMMY";
			postCode.RK_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			postCode.RK_IsActive = true;
			postCode.RK_IsSystem = true;
			Factory.Save();

			var helper = new RefCityTownPostcodeHelper(null, Core.Constants.CountryCodes.Australia, string.Empty, string.Empty, string.Empty);
			helper.ConfirmWithUser("NSW", "WOW", postCode);
			var lastMessage = UnitTestUserNotification.Instance.LastMessage;
			AssertEquals("Warning", lastMessage.Caption);
			AssertEquals(@"DUMMY WOW, AU not found. Would you like to add this to reference data?
Note: If you choose not to add this you will not be able to save this row.", lastMessage.Text);

			var cityTown = Factory.New<RefCityTown>();
			cityTown.R9_InternationalName = "WOW";
			cityTown.R9_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			cityTown.R9_RW_NKState = "NSW";
			Factory.Save();

			helper.ConfirmWithUser("NSW", "WOW", postCode);
			lastMessage = UnitTestUserNotification.Instance.LastMessage;
			AssertEquals(@"WOW NSW AU and postcode DUMMY exist in your database, however they are not linked together. WOW is not associated with any postcode and DUMMY is not linked to any city/town. It may be a mistake in the import file. Are you sure you want to link these together?
Note: If you choose not to add this you will not be able to save this row.", lastMessage.Text);
		}

		public void TestConfirmWithUser_WithPostCodes()
		{
			var postCode = Factory.New<RefPostCode>();
			postCode.RK_CityTownPostCode = "DUMMY";
			postCode.RK_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			postCode.RK_IsActive = true;
			postCode.RK_IsSystem = true;

			var cityTown = Factory.New<RefCityTown>();
			cityTown.R9_InternationalName = "WOW";
			cityTown.R9_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			cityTown.R9_RW_NKState = "NSW";

			CreatePostcode("DUMMY-2", cityTown.PK);
			CreatePostcode("DUMMY-3", cityTown.PK);
			CreatePostcode("DUMMY-4", cityTown.PK);
			Factory.Save();

			var helper = new RefCityTownPostcodeHelper(null, Core.Constants.CountryCodes.Australia, string.Empty, string.Empty, string.Empty);
			helper.ConfirmWithUser("NSW", "WOW", postCode);
			var lastMessage = UnitTestUserNotification.Instance.LastMessage;
			AssertEquals(2, lastMessage.Text.AllIndexesOf("DUMMY-").Count());
			AssertContains("WOW is associated with DUMMY-", lastMessage.Text);
		}

		public void TestConfirmWithUser_WithCityTowns()
		{
			var postCode = Factory.New<RefPostCode>();
			postCode.RK_CityTownPostCode = "DUMMY";
			postCode.RK_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			postCode.RK_IsActive = true;
			postCode.RK_IsSystem = true;

			var cityTown = Factory.New<RefCityTown>();
			cityTown.R9_InternationalName = "WOW";
			cityTown.R9_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			cityTown.R9_RW_NKState = "NSW";

			CreateCityTown("A NEW DUMMY CITY 1", postCode.PK);
			CreateCityTown("A NEW DUMMY CITY 2", postCode.PK);
			Factory.Save();

			var helper = new RefCityTownPostcodeHelper(null, Core.Constants.CountryCodes.Australia, string.Empty, string.Empty, string.Empty);
			helper.ConfirmWithUser("NSW", "WOW", postCode);
			var lastMessage = UnitTestUserNotification.Instance.LastMessage;
			AssertEquals(2, lastMessage.Text.AllIndexesOf("A NEW DUMMY CITY").Count());
			AssertContains("DUMMY is linked to A NEW DUMMY CITY", lastMessage.Text);
		}

		public void TestFindStateCode_SetQueryUsingFullStateName()
		{
			var helper = new RefCityTownPostcodeHelperForTest(new RefPostCodeCollection(Factory), "IT", "", "", "");
			RefCountryStates tempCountryState = helper.GetOrCreateCountryState("Alessandria", "Castelletto D'Erro");

			CombineAssertions(() =>
			{
				AssertEquals("RW_Code should be state AL", "AL", tempCountryState.RW_Code);
				AssertEquals("Country code should be IT", "IT", tempCountryState.RW_RN_NKCountryCode);
			});
		}

		public void TestFindStateCode_SetQueryUsingStateCode()
		{
			var helper = new RefCityTownPostcodeHelperForTest(new RefPostCodeCollection(Factory), "IT", "", "", "");
			RefCountryStates tempCountryState = helper.GetOrCreateCountryState("AL", "Castelletto D'Erro");

			CombineAssertions(() =>
			{
				AssertEquals("RW_Code should be state AL", "AL", tempCountryState.RW_Code);
				AssertEquals("Country code should be IT", "IT", tempCountryState.RW_RN_NKCountryCode);
			});
		}

		public void TestFindStateCode_SetQueryUsingCity()
		{
			var helper = new RefCityTownPostcodeHelperForTest(new RefPostCodeCollection(Factory), "JP", "", "", "");
			RefCountryStates tempCountryState = helper.GetOrCreateCountryState("Kobe", "Hyogo");

			CombineAssertions(() =>
			{
				AssertEquals("RW_Code should be 28", "28", tempCountryState.RW_Code);
				AssertEquals("Country code should be JP", "JP", tempCountryState.RW_RN_NKCountryCode);
			});
		}

		public void TestFindStateCode_SetQueryUsingStateSubstring()
		{
			var helper = new RefCityTownPostcodeHelperForTest(new RefPostCodeCollection(Factory), "GB", "", "", "");
			RefCountryStates tempCountryState = helper.GetOrCreateCountryState("Aberdeen", "Aberdeen City");

			CombineAssertions(() =>
			{
				AssertEquals("RW_Code should be state ABE", "ABE", tempCountryState.RW_Code);
				AssertEquals("Country code should be GB", "GB", tempCountryState.RW_RN_NKCountryCode);
			});
		}

		public void TestNotifyUserAndRetry_OrganizeAndSaveCityTownsFailed()
		{
			var helper = new RefCityTownPostcodeHelperForRetryTest(new RefPostCodeCollection(Factory), "GB", "", "", "")
			{
				CandidateCityTownForTest = new[]
				{
					new CandidateCityTown
					{
						Postcode = "12345",
						State = "ABD",
						City = "WooHoo"
					}
				},
				ThrowZSaveExceptionTimes = 1,
			};

			var guid = helper.GetPostcodePKFromCode("12345");
			AssertEquals("When user click No, return invalid postcode pk", ZGuid.Invalid, guid);
			AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.Any(u => u.Text == "Failed to update Reference Data due to other people change the Reference Data at the same time, do you want to retry?"));

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
			helper.ThrowZSaveExceptionTimes = 1;
			guid = helper.GetPostcodePKFromCode("12345");
			AssertNotEquals("When user click retry yes, postcode saved to database successful", ZGuid.Invalid, guid);

			helper.ThrowZSaveExceptionTimes = 2;
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
			AssertExceptionThrown<AggregateException>("Only retry one time", () => helper.GetPostcodePKFromCode("12345"));
		}

		#region Implementation

		void CreatePostcode(string code, ZGuid cityTownPK)
		{
			var refPostCode = Factory.New<RefPostCode>();
			refPostCode.RK_CityTownPostCode = code;
			refPostCode.RK_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			refPostCode.RK_IsActive = true;
			refPostCode.RK_IsSystem = true;

			CreatePivot(refPostCode.PK, cityTownPK);
		}

		void CreateCityTown(string cityName, ZGuid postCodePK)
		{
			var refCityTown = Factory.New<RefCityTown>();
			refCityTown.R9_InternationalName = cityName;
			refCityTown.R9_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			refCityTown.R9_RW_NKState = "NSW";

			CreatePivot(postCodePK, refCityTown.PK);
		}

		void CreatePivot(ZGuid postCodePK, ZGuid cityTownPK)
		{
			var pivot = Factory.New<RefCityPCodePivot>();
			pivot.R0_RK = postCodePK;
			pivot.R0_R9 = cityTownPK;
			pivot.R0_IsSystem = true;
		}

		public class RefCityTownPostcodeHelperForTest : RefCityTownPostcodeHelper
		{
			public RefCityTownPostcodeHelperForTest(IBusinessObjectCollection collection, string countryCode, string postcode,
				string cityTown, string state) : base(collection, countryCode, postcode, cityTown, state)
			{
			}

			public CandidateCityTown[] CandidateCityTownForTest;

			protected override Task<CandidateCityTown[]> GetCityTownPostcodeListFromWebAsync(ISupportWebAddressValidation address)
			{
				return Task.Run(() => CandidateCityTownForTest);
			}
		}

		public class RefCityTownPostcodeHelperForRetryTest : RefCityTownPostcodeHelperForTest
		{
			readonly BusinessObjectFactory factory;

			public int ThrowZSaveExceptionTimes { get; set; }

			public RefCityTownPostcodeHelperForRetryTest(IBusinessObjectCollection collection, string countryCode, string postcode, string cityTown, string state) : base(collection, countryCode, postcode, cityTown, state)
			{
				factory = collection.Factory;
			}

			protected override void OrganizeAndSaveCityTownsCore(CandidateCityTown[] candidates)
			{
				if (ThrowZSaveExceptionTimes > 0)
				{
					ThrowZSaveExceptionTimes--;
					throw new ZSaveException(new ZDataException(new InvalidOperationException("Dummy"), null, null), factory);
				}

				base.OrganizeAndSaveCityTownsCore(candidates);
			}
		}

		#endregion
	}
}
