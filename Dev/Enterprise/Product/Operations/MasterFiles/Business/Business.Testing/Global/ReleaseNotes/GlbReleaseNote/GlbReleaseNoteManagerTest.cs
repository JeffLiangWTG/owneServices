using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbReleaseNoteManager))]
	public class GlbReleaseNoteManagerTest : NonPersistentBusinessObjectTestCase
	{
		[StressTest]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		public void TestClearFilter()
		{
			GlbReleaseNoteManager manager = new GlbReleaseNoteManager(Factory);
			manager.SearchText = "Hello";
			manager.ModuleToFilterFor = "SAL";
			manager.DateToFilterAfter = ZDateTime.Today;

			manager.ClearFilter();

			AssertEquals("", manager.SearchText);
			AssertEquals("", manager.ModuleToFilterFor);
			AssertEquals(ZDateTime.Empty, manager.DateToFilterAfter);
			AssertEquals(ZDateTime.Empty, manager.DateToFilterBefore);
		}

		public void TestDefaultDates()
		{
			GlbReleaseNoteManager manager = new GlbReleaseNoteManager(Factory);
			Env.Registry.EnterpriseCDDate = new DateTime(2005, 2, 1);
			manager.DateToFilterBefore = ZDateTime.Today;

			AssertEquals("Dates should be the same", ZDateTime.Today.AddMonths(-1), manager.DateToFilterAfter);
			AssertEquals("Dates should be the same", ZDateTime.Today, manager.DateToFilterBefore);
		}

		public virtual void TestDefaultValues()
		{
			Env.Registry.DoNotDisplayReleaseNotesVersion = null;
			Env.Registry.DoNotDisplayUpdatesOnLogin = false;
			Env.Registry.DoNotDisplayUpdatesOnModuleEntry = false;
			Env.Registry.ShowReadNotes = false;
			Env.Registry.ModuleToShowNotesFor = "";

			GlbReleaseNoteManager manager = new GlbReleaseNoteManager(Factory);
			AssertEquals("When reg item is null - ie never been set before, default this to true", true, manager.DoNotDisplayUntilNextUpgrade);
			AssertEquals(false, manager.DoNotDisplayUpdatesOnLogin);
			AssertEquals(false, manager.DoNotDisplayUpdatesOnModuleEntry);
			AssertEquals(false, manager.ShowReadNotes);
			AssertEquals("", manager.ModuleToFilterFor);

			Env.Registry.DoNotDisplayReleaseNotesVersion = "";
			manager = new GlbReleaseNoteManager(Factory);
			AssertEquals("When reg item is BLANK - ie explicitly set to off, set to false", false, manager.DoNotDisplayUntilNextUpgrade);

			Env.Registry.DoNotDisplayReleaseNotesVersion = "asfasf";
			Env.Registry.DoNotDisplayUpdatesOnLogin = true;
			Env.Registry.DoNotDisplayUpdatesOnModuleEntry = true;
			Env.Registry.ShowReadNotes = true;
			Env.Registry.ModuleToShowNotesFor = "SAL";

			manager = new GlbReleaseNoteManager(Factory);
			AssertEquals(true, manager.DoNotDisplayUntilNextUpgrade);
			AssertEquals(true, manager.DoNotDisplayUpdatesOnLogin);
			AssertEquals(true, manager.DoNotDisplayUpdatesOnModuleEntry);
			AssertEquals(true, manager.ShowReadNotes);
			AssertEquals("SAL", manager.ModuleToFilterFor);

			manager = new GlbReleaseNoteManager(Factory, false, "COR");
			AssertEquals("COR", manager.ModuleToFilterFor);
		}

		public void TestRegistryDefaultsUpdatedOnSave()
		{
			Env.Registry.DoNotDisplayReleaseNotesVersion = "";
			Env.Registry.DoNotDisplayUpdatesOnLogin = false;
			Env.Registry.DoNotDisplayUpdatesOnModuleEntry = false;
			Env.Registry.ShowReadNotes = false;
			Env.Registry.ModuleToShowNotesFor = "";

			GlbReleaseNoteManager manager = new GlbReleaseNoteManager(Factory);
			manager.ModuleToFilterFor = "SAL";
			manager.ShowReadNotes = true;
			manager.DoNotDisplayUpdatesOnModuleEntry = true;
			manager.DoNotDisplayUpdatesOnLogin = true;
			manager.DoNotDisplayUntilNextUpgrade = true;

			Factory.Save();

			AssertEquals(true, Env.Registry.DoNotDisplayUpdatesOnLogin);
			AssertEquals(true, Env.Registry.DoNotDisplayUpdatesOnModuleEntry);
			AssertEquals(true, Env.Registry.ShowReadNotes);
			AssertEquals("SAL", Env.Registry.ModuleToShowNotesFor);
			Assert(!string.IsNullOrEmpty(Env.Registry.DoNotDisplayReleaseNotesVersion));

			manager = new GlbReleaseNoteManager(Factory, false, "ABC");
			Factory.Save();
			AssertEquals("SAL", Env.Registry.ModuleToShowNotesFor);

			manager = new GlbReleaseNoteManager(Factory);
			manager.ModuleToFilterFor = "ABC";
			Factory.Save();
			AssertEquals("ABC", Env.Registry.ModuleToShowNotesFor);
		}

		public void TestModuleLookups()
		{
			GlbReleaseNoteManager manager = new GlbReleaseNoteManager(Factory);
			AssertNotNull(manager.Modules);
		}

		public void TestPropertiesReloadReleaseNotes()
		{
			AssertEquals("Precondition: The current logged in company's country should be Australia.", Core.Constants.CountryCodes.Australia, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			TestCaseHelper.ClearTable(GlbReleaseNoteReadSchema.Constants.TableName);
			TestCaseHelper.ClearTable(GlbReleaseNoteSchema.Constants.TableName);

			Env.Registry.EnterpriseCDDate = new DateTime(2005, 2, 1);

			GlbReleaseNote note1 = Factory.NewWithValidTestData<GlbReleaseNote>();
			note1.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;
			GlbReleaseNote note2 = Factory.NewWithValidTestData<GlbReleaseNote>();
			note2.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;
			GlbReleaseNote note3 = Factory.NewWithValidTestData<GlbReleaseNote>();
			note3.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;
			GlbReleaseNote note4 = Factory.NewWithValidTestData<GlbReleaseNote>();
			note4.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;
			GlbReleaseNote note5 = Factory.NewWithValidTestData<GlbReleaseNote>();
			note5.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;

			note1.GF_RN_NKCountryForReleaseNote = Core.Constants.CountryCodes.Australia;
			note2.GF_RN_NKCountryForReleaseNote = Core.Constants.CountryCodes.NewZealand;
			note3.GF_RN_NKCountryForReleaseNote = "";
			note4.GF_RN_NKCountryForReleaseNote = "";
			note5.GF_RN_NKCountryForReleaseNote = "";

			note1.GF_ReleaseNoteDate = ZDateTime.Now;
			note2.GF_ReleaseNoteDate = ZDateTime.Now;
			note3.GF_ReleaseNoteDate = ZDateTime.Now;
			note4.GF_ReleaseNoteDate = new ZDateTime(2005, 1, 1);
			note5.GF_ReleaseNoteDate = ZDateTime.Now;

			note2.GF_Category = "SAL";
			note3.GF_Category = "COR";

			GlbReleaseNoteRead read = Factory.New<GlbReleaseNoteRead>();
			read.GR_ReleaseNoteID = note5.PK;
			read.GR_GS_Staff = GlbStaff.CurrentUser.PK;

			Factory.Save();

			GlbReleaseNoteManager manager = new GlbReleaseNoteManager(Factory);
			manager.ShowReadNotes = false;
			manager.ShowAllCountryNotes = false;
			manager.ModuleToFilterFor = "COR";
			AssertEquals(1, manager.ReleaseNotes.Count);
			AssertEquals(note3.PK, manager.ReleaseNotes[0].PK);

			manager.ModuleToFilterFor = "SAL";
			AssertEquals(0, manager.ReleaseNotes.Count);

			manager.ShowAllCountryNotes = true;
			AssertEquals(1, manager.ReleaseNotes.Count);
			AssertEquals(note2.PK, manager.ReleaseNotes[0].PK);

			manager.ShowAllCountryNotes = false;
			manager.ModuleToFilterFor = "";
			manager.DateToFilterAfter = ZDateTime.Empty;
			AssertEquals(3, manager.ReleaseNotes.Count);
			AssertCollectionContains(note1, manager.ReleaseNotes);
			AssertCollectionContains(note3, manager.ReleaseNotes);
			AssertCollectionContains(note4, manager.ReleaseNotes);

			manager.ShowReadNotes = true;
			AssertEquals(4, manager.ReleaseNotes.Count);
			AssertCollectionContains(note1, manager.ReleaseNotes);
			AssertCollectionContains(note3, manager.ReleaseNotes);
			AssertCollectionContains(note4, manager.ReleaseNotes);
			AssertCollectionContains(note5, manager.ReleaseNotes);

			manager.DateToFilterAfter = new ZDateTime(2005, 2, 2);
			AssertEquals(3, manager.ReleaseNotes.Count);
			AssertCollectionContains(note1, manager.ReleaseNotes);
			AssertCollectionContains(note3, manager.ReleaseNotes);
			AssertCollectionContains(note5, manager.ReleaseNotes);

			manager.DateToFilterAfter = new ZDateTime(2004, 1, 1);
			manager.DateToFilterBefore = ZDateTime.Now.AddMonths(-1);

			AssertEquals(1, manager.ReleaseNotes.Count);
			AssertCollectionContains(note4, manager.ReleaseNotes);
		}

		public virtual void TestReleaseNotes()
		{
			AssertEquals("Precondition: The current logged in company's country should be Australia.", Core.Constants.CountryCodes.Australia, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			Env.Registry.DoNotDisplayUpdatesOnLogin = false;
			Env.Registry.DoNotDisplayUpdatesOnModuleEntry = false;
			Env.Registry.ModuleToShowNotesFor = "";
			Env.Registry.EnterpriseCDDate = new DateTime(2005, 2, 1);

			GlbReleaseNote note1 = Factory.NewWithValidTestData<GlbReleaseNote>();
			note1.GF_Summary = "this note zubin details on xyz";
			note1.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;
			GlbReleaseNote note2 = Factory.NewWithValidTestData<GlbReleaseNote>();
			note2.GF_Summary = "this note zubin ABC details";
			note2.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;
			GlbReleaseNote note3 = Factory.NewWithValidTestData<GlbReleaseNote>();
			note3.GF_Summary = "who knows zubin";
			note3.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;
			GlbReleaseNote note4 = Factory.NewWithValidTestData<GlbReleaseNote>();
			note4.GF_Summary = "who is this zubin";
			note4.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;

			note1.GF_RN_NKCountryForReleaseNote = Core.Constants.CountryCodes.Australia;
			note2.GF_RN_NKCountryForReleaseNote = Core.Constants.CountryCodes.NewZealand;
			note3.GF_RN_NKCountryForReleaseNote = "";
			note4.GF_RN_NKCountryForReleaseNote = "";

			note1.GF_ReleaseNoteDate = ZDateTime.Now;
			note2.GF_ReleaseNoteDate = ZDateTime.Now;
			note3.GF_ReleaseNoteDate = ZDateTime.Now;
			note4.GF_ReleaseNoteDate = new ZDateTime(2005, 1, 1);

			Factory.Save();

			GlbReleaseNoteManager managerWithDefaults = new GlbReleaseNoteManager(Factory);

			GlbReleaseNoteManager managerWithShowAll = new GlbReleaseNoteManager(Factory);
			managerWithShowAll.ShowReadNotes = true;
			managerWithShowAll.ShowAllCountryNotes = true;

			GlbReleaseNoteManager managerWithShowRead = new GlbReleaseNoteManager(Factory);
			managerWithShowRead.ShowReadNotes = true;
			managerWithShowRead.ShowAllCountryNotes = false;

			GlbReleaseNoteManager managerWithShowAllCountries = new GlbReleaseNoteManager(Factory);
			managerWithShowAllCountries.ShowReadNotes = false;
			managerWithShowAllCountries.ShowAllCountryNotes = true;

			GlbReleaseNoteManager managerWithShowUnreadAndCurrentCountries = new GlbReleaseNoteManager(Factory);
			managerWithShowUnreadAndCurrentCountries.ShowReadNotes = false;
			managerWithShowUnreadAndCurrentCountries.ShowAllCountryNotes = false;

			GlbReleaseNote[] allReleaseNotesForAustralia = new GlbReleaseNote[] { note1, note3 };
			GlbReleaseNote[] allReleaseNotes = new GlbReleaseNote[] { note1, note2, note3 };
			GlbReleaseNote[] releaseNotesForNewZealandOnly = new GlbReleaseNote[] { note2 };

			AssertEquals("ManagerWithDefaults.ReleaseNotes.ReadOnly", false, managerWithDefaults.ReleaseNotes.ReadOnly);
			AssertEquals("ManagerWithShowAll.ReleaseNotes.ReadOnly", false, managerWithShowAll.ReleaseNotes.ReadOnly);
			AssertEquals("ManagerWithShowRead.ReleaseNotes.ReadOnly", false, managerWithShowRead.ReleaseNotes.ReadOnly);
			AssertEquals("ManagerWithShowAllCountries.ReleaseNotes.ReadOnly", false, managerWithShowAllCountries.ReleaseNotes.ReadOnly);
			AssertEquals("ManagerWithShowUnreadAndCurrentCountries.ReleaseNotes.ReadOnly", false, managerWithShowUnreadAndCurrentCountries.ReleaseNotes.ReadOnly);

			AssertReleaseNotesContains(managerWithDefaults, allReleaseNotesForAustralia, releaseNotesForNewZealandOnly);
			AssertReleaseNotesContains(managerWithShowAll, allReleaseNotes, Array.Empty<GlbReleaseNote>());
			AssertReleaseNotesContains(managerWithShowRead, allReleaseNotesForAustralia, releaseNotesForNewZealandOnly);
			AssertReleaseNotesContains(managerWithShowAllCountries, allReleaseNotes, Array.Empty<GlbReleaseNote>());
			AssertReleaseNotesContains(managerWithShowUnreadAndCurrentCountries, allReleaseNotesForAustralia, releaseNotesForNewZealandOnly);

			AssertEquals("Notes older than the Enterprise CD Date should not be shown.", false, managerWithDefaults.ReleaseNotes.Contains(note4));
			AssertEquals("Notes older than the Enterprise CD Date should not be shown.", false, managerWithShowAll.ReleaseNotes.Contains(note4));
			AssertEquals("Notes older than the Enterprise CD Date should not be shown.", false, managerWithShowRead.ReleaseNotes.Contains(note4));
			AssertEquals("Notes older than the Enterprise CD Date should not be shown.", false, managerWithShowAllCountries.ReleaseNotes.Contains(note4));
			AssertEquals("Notes older than the Enterprise CD Date should not be shown.", false, managerWithShowUnreadAndCurrentCountries.ReleaseNotes.Contains(note4));

			managerWithShowAll.SearchText = "Bonjour Leigh";
			AssertEquals(0, managerWithShowAll.ReleaseNotes.Count);

			managerWithShowAll.SearchText = "this note zubin details";
			AssertEquals(2, managerWithShowAll.ReleaseNotes.Count);

			managerWithShowAll.SearchText = "\"this note zubin details\"";
			AssertEquals(1, managerWithShowAll.ReleaseNotes.Count);
		}

		[TestDate(2015, 5, 5)]
		public void TestReleaseNotes_HitsDbOnlyOnce()
		{
			for (var i = 0; i < 10; i++)
			{
				var readNote = Factory.NewWithValidTestData<GlbReleaseNote>();
				readNote.IsCurrentlyRead = true;
				readNote.GF_RN_NKCountryForReleaseNote = GlbReleaseNoteLookups.AllCountriesCode;
				readNote.GF_ReleaseNoteDate = new ZDateTime(1991, 5, 5);

				var unreadNote = Factory.NewWithValidTestData<GlbReleaseNote>();
				unreadNote.IsCurrentlyRead = false;
				unreadNote.GF_RN_NKCountryForReleaseNote = GlbReleaseNoteLookups.AllCountriesCode;
				unreadNote.GF_ReleaseNoteDate = new ZDateTime(1991, 5, 5);
			}
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var manager = new GlbReleaseNoteManager(anotherFactory);
			manager.DateToFilterAfter = new ZDateTime(1991, 4, 5);
			manager.DateToFilterBefore = new ZDateTime(1991, 6, 5);
			manager.ShowReadNotes = true;

			AssertEquals(20, manager.ReleaseNotes.Count);

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(GlbReleaseNoteSchema.Constants.TableName, 1);
			AssertDbHits(expectedDbHits, anotherFactory);

			anotherFactory.ResetDatabaseLoadCount();

			manager.ShowReadNotes = false;

			AssertEquals(10, manager.ReleaseNotes.Count);

			expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(GlbReleaseNoteSchema.Constants.TableName, 1);
			AssertDbHits(expectedDbHits, anotherFactory);
		}

		[TestDate(2013, 7, 1)]
		[ExpectNoExceptions]
		public void TestNoSmallDateTimeOverflowException()
		{
			GlbReleaseNote note1 = Factory.NewWithValidTestData<GlbReleaseNote>();
			note1.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;
			GlbReleaseNote note2 = Factory.NewWithValidTestData<GlbReleaseNote>();
			note2.GF_Section = NewsSectionTypeList.Codes.ProductUpdates;

			note1.GF_RN_NKCountryForReleaseNote = "";
			note2.GF_RN_NKCountryForReleaseNote = "";

			note1.GF_ReleaseNoteDate = new ZDateTime(2013, 5, 1);
			note2.GF_ReleaseNoteDate = new ZDateTime(2013, 6, 30);

			Factory.Save();

			Manager.DateToFilterAfter = new ZDateTime(3913, 6, 13);
			AssertEquals("Contains(note1)", true, Manager.ReleaseNotes.Contains(note1));
			AssertEquals("Contains(note2)", true, Manager.ReleaseNotes.Contains(note2));

			Manager.DateToFilterBefore = new ZDateTime(1800, 6, 13);
			AssertEquals("Contains(note1)", true, Manager.ReleaseNotes.Contains(note1));
			AssertEquals("Contains(note2)", true, Manager.ReleaseNotes.Contains(note2));
		}

		public void TestDoNotDisplayUntilNextUpgrade()
		{
			string currentVersion = ReleaseInfo.Instance.VersionNumber.ToString();

			Env.Registry.DoNotDisplayReleaseNotesVersion = "";
			GlbReleaseNoteManager manager = new GlbReleaseNoteManager(Factory);
			AssertEquals("DoNotDisplayUntilNextUpgrade", false, manager.DoNotDisplayUntilNextUpgrade);

			Env.Registry.DoNotDisplayReleaseNotesVersion = null;
			manager = new GlbReleaseNoteManager(Factory);
			AssertEquals("DoNotDisplayUntilNextUpgrade - if null it's never been set - so default to true.", true, manager.DoNotDisplayUntilNextUpgrade);

			Env.Registry.DoNotDisplayReleaseNotesVersion = "1.1.2.2";
			manager = new GlbReleaseNoteManager(Factory);
			AssertEquals("DoNotDisplayUntilNextUpgrade", true, manager.DoNotDisplayUntilNextUpgrade);

			manager.DoNotDisplayUntilNextUpgrade = false;
			Factory.Save();
			AssertEquals("Env.Registry.DoNotDisplayReleaseNotesVersion", "", Env.Registry.DoNotDisplayReleaseNotesVersion);

			manager.DoNotDisplayUntilNextUpgrade = true;
			Factory.Save();
			AssertEquals("Env.Registry.DoNotDisplayReleaseNotesVersion", currentVersion, Env.Registry.DoNotDisplayReleaseNotesVersion);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewManager();
		}

		void AssertReleaseNotesContains(GlbReleaseNoteManager manager, GlbReleaseNote[] contains, GlbReleaseNote[] notContains)
		{
			foreach (GlbReleaseNote note in contains)
			{
				AssertEquals("ReleaseNotes should contain Note with GF_RN_NKCountryForReleaseNote of '" + note.GF_RN_NKCountryForReleaseNote + "'.", true, manager.ReleaseNotes.Contains(note));
			}

			foreach (GlbReleaseNote note in notContains)
			{
				AssertEquals("ReleaseNotes should NOT contain Note with GF_RN_NKCountryForReleaseNote of '" + note.GF_RN_NKCountryForReleaseNote + "'.", false, manager.ReleaseNotes.Contains(note));
			}
		}

		#region Manager

		protected GlbReleaseNoteManager Manager
		{
			get
			{
				if (fManager == null)
				{
					fManager = GetNewManager();
				}

				return fManager;
			}
		}

		protected virtual GlbReleaseNoteManager GetNewManager()
		{
			return new GlbReleaseNoteManager(Factory);
		}

		GlbReleaseNoteManager fManager;

		#endregion

		#endregion
	}
}
