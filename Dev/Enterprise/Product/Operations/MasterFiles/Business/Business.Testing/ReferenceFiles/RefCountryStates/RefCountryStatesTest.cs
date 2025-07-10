using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefCountryStates))]
	sealed class RefCountryStatesTest : EnterpriseBusinessObjectTestCase
	{
		#region On Loaded
		public void TestOnLoaded()
		{
			CountryStates.RW_IsSystem = false;
			CountryStates.OnLoaded();
			Assert("RW_Code should not be read only", !CountryStates.RW_CodeInfo.ReadOnly);
			CountryStates.RW_IsSystem = true;
			CountryStates.OnLoaded();
			Assert("RW_Code should be read only", CountryStates.RW_CodeInfo.ReadOnly);
		}
		#endregion

		#region ICancellable

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(CountryStates.GetType()));
		}
		#endregion

		#region Translatable

		public void TestRW_Description_Translatable()
		{
			var bizO = Factory.NewWithValidTestData<RefCountryStates>();
			bizO.RW_Description = "Boom";

			var translateEnglish = Factory.New<RefLanguageText>();
			translateEnglish.RLT_ColumnName = "RW_Description";
			translateEnglish.RLT_Language = Core.SharedConstants.Languages.English;
			translateEnglish.RLT_ParentId = bizO.PK;
			translateEnglish.RLT_ParentTableCode = "RW";
			translateEnglish.RLT_Text = "Boom";

			var translateChinese = Factory.New<RefLanguageText>();
			translateChinese.RLT_ColumnName = "RW_Description";
			translateChinese.RLT_Language = Core.SharedConstants.Languages.ChineseSimplified;
			translateChinese.RLT_ParentId = bizO.PK;
			translateChinese.RLT_ParentTableCode = "RW";
			translateChinese.RLT_Text = "动";

			Factory.Save();

			AssertEquals("Boom", bizO.RW_DescriptionMultilingual);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				AssertEquals("动", bizO.RW_DescriptionMultilingual);
			}
		}

		#endregion

		#region ILocation

		public void TestAsILocation()
		{
			var unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var countryState = unloco.CountryStates;
			var location = countryState as ILocation;
			AssertNotNull("Should not be null", location);
			AssertNull("CityTown should be null", location.CityTown);
			AssertEquals("Code", countryState.RW_Code, location.Code);
			AssertEquals("Country", countryState.Country, location.Country);
			AssertEquals("Description", countryState.RW_DescriptionMultilingual, location.Description);
			AssertNull("IATACityCode", location.IATACityCode);
			AssertEquals("IsActive", countryState.RW_IsActive, location.IsActive);
			AssertEquals("State", countryState, location.State);
			AssertNull("UNLOCO", location.UNLOCO);
			AssertEquals("Zone", 0, location.Zones.Length);
			AssertEquals(true, location.IsLocalInRelationTo(unloco.RL_Code));
			AssertEquals(false, location.IsLocalInRelationTo("AUMEL"));
		}

		#endregion

		#region Implementation
		BusinessObjectFactory TestFactory;
		RefCountryStates CountryStates;

		protected override void SetUp()
		{
			base.SetUp();
			TestFactory = new BusinessObjectFactory();
			CountryStates = TestFactory.New(typeof(RefCountryStates)) as RefCountryStates;
		}
		#endregion

		public void TestCannotDeleteIsSystemRecords()
		{
			CountryStates.RW_IsSystem = true;
			Assert("Cannot delete dbo.RefCountryStates records marked as IsSystem", !CountryStates.CanDelete);
			AssertEquals("Cannot delete system defined states.", CountryStates.ReasonForNotAbleToDelete.ToString());
			CountryStates.RW_IsSystem = false;
			Assert("Can delete dbo.RefCountryStates records marked as IsSystem", CountryStates.CanDelete);
		}
	}
}
