using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.DataMapping.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateTransportZoneItemImportWizard))]
	public class RateTransportZoneItemImportWizardTest : NonPersistentBusinessObjectTestCase
	{
		public void TestImport_TransportZoneHasNewCityAndEmptyState_ImportWithoutErrors()
		{
			var zone = RateTransportZoneItemImportWizardForTest.GetZoneInfo(Factory);
			var zoneCollection = new RateTransportZoneItemCollection(zone);

			var wizard = new RateTransportZoneItemImportWizardForTest(zone);
			wizard.Mapping[0].AddFileColumnIndex(0);
			wizard.Mapping[1].AddFileColumnIndex(1);
			wizard.Mapping[2].AddFileColumnIndex(2);
			wizard.fakeFileContent.Add(new[] { "2015", "NEWCITY", "" }); //New-city with empty-state

			UnitTestUserNotification.Instance.AddYesAnswer();

			AssertNoExceptionThrown(() =>
			{
				wizard.ImportIntoCollection(zoneCollection);
			});

			AssertImportedResult(
				expectedPostcode: "2015",
				expectedCity: "NEWCITY",
				expectedState: "",
				rateTransportZoneItem: zoneCollection[0]);
		}

		public void TestImportIntoBizObjCore()
		{
			var zone = RateTransportZoneItemImportWizardForTest.GetZoneInfo(Factory);
			var collection = new RateTransportZoneItemCollection(zone);
			var wizard = new RateTransportZoneItemImportWizardForTest(zone);
			wizard.Mapping[0].AddFileColumnIndex(0);
			wizard.Mapping[1].AddFileColumnIndex(1);
			wizard.Mapping[2].AddFileColumnIndex(2);
			wizard.fakeFileContent.Add(new[] { "2015", "Alexandria", "NSW" }); //Normal one
			wizard.fakeFileContent.Add(new[] { "2015", "SOMEWHERE", "NSW" }); //Invalid city
			wizard.fakeFileContent.Add(new[] { "2015", "Mascot", "NSW" }); //postcode city can't match, will create new pivot

			UnitTestUserNotification.Instance.AddYesAnswer();
			UnitTestUserNotification.Instance.AddYesAnswer();

			wizard.ImportIntoCollection(collection);
			AssertImportedResult("2015", "ALEXANDRIA", "NSW", collection[0]);
			AssertImportedResult("2015", "SOMEWHERE", "NSW", collection[1]);
			AssertImportedResult("2015", "MASCOT", "NSW", collection[2]);

			Assert(!collection[1].CityTown.R9_IsSystem);
		}

		public void TestImportIntoBizObjCore_NoErrorWhenStateIndexIsZero()
		{
			var zone = RateTransportZoneItemImportWizardForTest.GetZoneInfo(Factory);
			var collection = new RateTransportZoneItemCollection(zone);
			var wizard = new RateTransportZoneItemImportWizardForTest(zone);
			wizard.Mapping[0].AddFileColumnIndex(1);
			wizard.Mapping[1].AddFileColumnIndex(2);
			wizard.Mapping[2].AddFileColumnIndex(0);
			wizard.fakeFileContent.Add(new[] { "NSW", "2015", "Alexandria" }); //Normal one
			wizard.fakeFileContent.Add(new[] { "NSW", "2015", "SOMEWHERE" }); //Invalid city
			wizard.fakeFileContent.Add(new[] { "NSW", "2015", "Mascot" }); //postcode city can't match, will create new pivot

			UnitTestUserNotification.Instance.AddYesAnswer();
			UnitTestUserNotification.Instance.AddYesAnswer();

			wizard.ImportIntoCollection(collection);
			AssertImportedResult("2015", "ALEXANDRIA", "NSW", collection[0]);
			AssertImportedResult("2015", "SOMEWHERE", "NSW", collection[1]);
			AssertImportedResult("2015", "MASCOT", "NSW", collection[2]);

			Assert(!collection[1].CityTown.R9_IsSystem);
		}

		public void TestImportIntoBizObjCore_CanAddLinkToPostCodeWhenCityTownExist()
		{
			InitState("NSW");
			GetOrCreatePostcode("2010");

			var cityTown = Factory.New<RefCityTown>();
			cityTown.R9_InternationalName = "A Not Exist City Town";
			cityTown.R9_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			cityTown.R9_RW_NKState = "NSW";

			Factory.Save();

			var zone = RateTransportZoneItemImportWizardForTest.GetZoneInfo(Factory);
			var collection = new RateTransportZoneItemCollection(zone);
			var wizard = new RateTransportZoneItemImportWizardForTest(zone);
			wizard.Mapping[0].AddFileColumnIndex(1);
			wizard.Mapping[1].AddFileColumnIndex(2);
			wizard.Mapping[2].AddFileColumnIndex(0);
			wizard.fakeFileContent.Add(new[] { "NSW", "2010", "A Not Exist City Town" }); // City town exist, post code exist, no RefCityPCodePivot

			UnitTestUserNotification.Instance.AddYesAnswer();
			UnitTestUserNotification.Instance.ClearMessages();

			wizard.ImportIntoCollection(collection);
			AssertImportedResult("2010", "A Not Exist City Town", "NSW", collection[0]);
			AssertContains(@"A Not Exist City Town NSW AU and postcode 2010 exist in your database, however they are not linked together.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestImportIntoBizObjCore_SameCityTownName_DifferentState()
		{
			InitState("NSW");
			InitState("ABC");

			var cityTown1 = Factory.New<RefCityTown>();
			cityTown1.R9_InternationalName = "A Not Exist City Town";
			cityTown1.R9_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			cityTown1.R9_RW_NKState = "NSW";

			var cityTown2 = Factory.New<RefCityTown>();
			cityTown2.R9_InternationalName = "A Not Exist City Town";
			cityTown2.R9_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			cityTown2.R9_RW_NKState = "ABC";

			var postcode = GetOrCreatePostcode("2010");
			var pivot1 = Factory.New<RefCityPCodePivot>();
			pivot1.R0_RK = postcode.PK;
			pivot1.R0_R9 = cityTown1.PK;
			pivot1.R0_IsSystem = false;

			var pivot2 = Factory.New<RefCityPCodePivot>();
			pivot2.R0_RK = postcode.PK;
			pivot2.R0_R9 = cityTown2.PK;
			pivot2.R0_IsSystem = false;

			Factory.Save();

			var wizardAndCollection = InitRateTransportZoneCollectionAndWizard("ABC", "2010", "A Not Exist City Town");
			UnitTestUserNotification.Instance.AddYesAnswer();
			wizardAndCollection.Wizard.ImportIntoCollection(wizardAndCollection.Collection);
			AssertImportedResult("2010", "A Not Exist City Town", "ABC", wizardAndCollection.Collection[0]);

			wizardAndCollection = InitRateTransportZoneCollectionAndWizard("NSW", "2010", "A Not Exist City Town");
			UnitTestUserNotification.Instance.AddYesAnswer();
			wizardAndCollection.Wizard.ImportIntoCollection(wizardAndCollection.Collection);
			AssertImportedResult("2010", "A Not Exist City Town", "NSW", wizardAndCollection.Collection[0]);

			wizardAndCollection = InitRateTransportZoneCollectionAndWizard(string.Empty, "2010", "A Not Exist City Town");
			UnitTestUserNotification.Instance.AddYesAnswer();
			wizardAndCollection.Wizard.ImportIntoCollection(wizardAndCollection.Collection);
			Assert("Import the first one when state is empty", new[] { "NSW", "ABC" }.Contains(wizardAndCollection.Collection[0].CityTown.State.RW_Code.ToString()));
		}

		public void TestImport_IsExcludePostCode()
		{
			var zone = RateTransportZoneItemImportWizardForTest.GetZoneInfo(Factory);
			var zoneCollection = new RateTransportZoneItemCollection(zone);

			var wizard = new RateTransportZoneItemImportWizardForTest(zone);
			wizard.Mapping[0].AddFileColumnIndex(0);
			wizard.Mapping[1].AddFileColumnIndex(1);
			wizard.Mapping[2].AddFileColumnIndex(2);
			wizard.Mapping[3].AddFileColumnIndex(3);
			wizard.fakeFileContent.Add(new[] { "2015", "NEWCITY", "NSW", "Y" });

			UnitTestUserNotification.Instance.AddYesAnswer();

			AssertNoExceptionThrown(() =>
			{
				wizard.ImportIntoCollection(zoneCollection);
			});

			var item = zoneCollection[0];
			AssertEquals("TQ_IsExcludingPostCode imported", true, item.TQ_IsExcludingPostCode);
		}

		#region Implementation

		(RateTransportZoneItemImportWizardForTest Wizard, RateTransportZoneItemCollection Collection) InitRateTransportZoneCollectionAndWizard(string state, string postcode, string cityTown)
		{
			var zone = RateTransportZoneItemImportWizardForTest.GetZoneInfo(Factory);
			var collection = new RateTransportZoneItemCollection(zone);
			var wizard = new RateTransportZoneItemImportWizardForTest(zone);
			wizard.Mapping[0].AddFileColumnIndex(1);
			wizard.Mapping[1].AddFileColumnIndex(2);
			wizard.Mapping[2].AddFileColumnIndex(0);
			wizard.fakeFileContent.Add(new[] { state, postcode, cityTown });

			return (wizard, collection);
		}

		void InitState(string stateCode)
		{
			var state = Factory.LoadTop1<RefCountryStates>(new ZQuery(RefCountryStatesSchema.RW_Code, stateCode)
				.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, Core.Constants.CountryCodes.Australia));

			if (state == null)
			{
				state = Factory.New<RefCountryStates>();
				state.RW_IsActive = true;
				state.RW_IsSystem = true;
				state.RW_Description = stateCode;
				state.RW_Code = stateCode;
				state.RW_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			}
		}

		RefPostCode GetOrCreatePostcode(string code)
		{
			var postCode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, code)
				.AddToFilter(RefPostCodeSchema.RK_RN_NKCountry, Core.Constants.CountryCodes.Australia));

			if (postCode == null)
			{
				postCode = Factory.New<RefPostCode>();
				postCode.RK_CityTownPostCode = code;
				postCode.RK_RN_NKCountry = Core.Constants.CountryCodes.Australia;
				postCode.RK_IsActive = true;
				postCode.RK_IsSystem = true;
			}

			return postCode;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RateTransportZoneItemImportWizardForTest(RateTransportZoneItemImportWizardForTest.GetZoneInfo(Factory));
		}

		void AssertImportedResult(string expectedPostcode, string expectedCity, string expectedState, RateTransportZoneItem rateTransportZoneItem)
		{
			AssertEquals(expectedPostcode, rateTransportZoneItem.TQ_FromPostCode);

			if (!string.IsNullOrEmpty(expectedCity))
			{
				AssertNotNull("City Town Should Exist", rateTransportZoneItem.CityTown);
				AssertEquals(expectedCity, rateTransportZoneItem.CityTown.R9_InternationalName);
			}

			if (rateTransportZoneItem.CityTown != null)
			{
				if (string.IsNullOrEmpty(expectedState))
				{
					AssertNull(expectedState, rateTransportZoneItem.CityTown.State);
				}
				else
				{
					AssertEquals(expectedState, rateTransportZoneItem.CityTown.State.RW_Code);
				}
			}
		}

		public class RateTransportZoneItemImportWizardForTest : RateTransportZoneItemImportWizard
		{
			public static RateTransportZone GetZoneInfo(BusinessObjectFactory factory)
			{
				var zone = factory.New<RateTransportZone>();
				zone.TZ_TP = factory.New<RateTransportProvider>().PK;

				return zone;
			}

			public RateTransportZoneItemImportWizardForTest(RateTransportZone zone) : base("AU", GetCollectionInfo(zone), GetSettingsStorage(), new FileMapperForTest())
			{
				this.zone = zone;
			}

			IImportCollectionInfo collectionInfo;
			public new IImportCollectionInfo CollectionInfo => collectionInfo ?? (collectionInfo = GetCollectionInfo(zone));

			readonly RateTransportZone zone;
			public List<string[]> fakeFileContent = new List<string[]>();

			public override List<string[]> LoadFile(int startingRow, int maximumRows, bool forceFileLoad = false)
			{
				return fakeFileContent;
			}

			static IImportCollectionInfo GetCollectionInfo(RateTransportZone zone)
			{
				return new ImportCollectionInfoImpl(new RateTransportZoneItemCollection(zone))
					{
						new ImportPropertyInfoImpl<RateTransportZoneItem>(RateTransportZoneItemSchema.Constants.TQ_FromPostCode) { HeaderText = "Postcode" },
						new ImportPropertyInfoImpl<RateTransportZoneItem>(RateTransportZoneItemSchema.Constants.TQ_R9_CityTown) { HeaderText = "City" },
						new ImportPropertyInfoImpl<RateTransportZoneItem>("CityTown+State+RW_Code") { HeaderText = "State" },
						new ImportPropertyInfoImpl<RateTransportZoneItem>(RateTransportZoneItemSchema.Constants.TQ_IsExcludingPostCode) { HeaderText = "Autorating NOT validate Related Postcodes" },
					};
			}

			static ISettingsStorage GetSettingsStorage()
			{
				var settingsStorageStub = new Mock<ISettingsStorage>();
				settingsStorageStub.Setup(m => m.GetSavedSettings()).Returns(
					new string[] {
						RateTransportZoneItemSchema.Constants.TQ_FromPostCode,
						RateTransportZoneItemSchema.Constants.TQ_R9_CityTown,
						"CityTown+State+RW_Code"
				});

				return settingsStorageStub.Object;
			}
		}

		#endregion
	}
}
