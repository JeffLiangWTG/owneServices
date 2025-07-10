using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefCountryRulesHelperTest : TestCaseWithFactory
	{
		public void TestRulesTransportModeShouldHasSamePriorityAsRefCountry()
		{
			var rule1 = AddNewRule("CN", "US", "CN-US-AIR", true, Core.Constants.TransportModes.Air);

			var rule2 = AddNewRule("CN", "US", "CN-US-Empty", true);
			var rule3 = AddNewRule("CN", "", "CN-Empty-AIR", true, Core.Constants.TransportModes.Air);
			var rule4 = AddNewRule("", "US", "Empty-US-AIR", true, Core.Constants.TransportModes.Air);

			var rule5 = AddNewRule("CN", "", "CN-Empty-Empty", true);
			var rule6 = AddNewRule("", "US", "Empty-US-Empty", true);

			Factory.Save();

			var countryCN = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			var countryUS = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");
			var countryOther = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			AssertResult(countryCN, countryUS, Core.Constants.TransportModes.Air, new string[] { "CN-US-AIR" });

			rule1.Delete();
			Factory.Save();
			AssertResult(countryCN, countryUS, Core.Constants.TransportModes.Air, new string[] { "CN-US-Empty", "CN-Empty-AIR", "Empty-US-AIR" });

			AssertResult(countryOther, countryUS, Core.Constants.TransportModes.Air, new string[] { "Empty-US-AIR" });
			AssertResult(countryCN, countryOther, Core.Constants.TransportModes.Air, new string[] { "CN-Empty-AIR" });
			AssertResult(countryCN, countryUS, Core.Constants.TransportModes.Sea, new string[] { "CN-US-Empty" });

			AssertResult(countryCN, countryOther, Core.Constants.TransportModes.Sea, new string[] { "CN-Empty-Empty" });
			AssertResult(countryOther, countryUS, Core.Constants.TransportModes.Sea, new string[] { "Empty-US-Empty" });
			AssertResult(countryOther, countryOther, Core.Constants.TransportModes.Air, System.Array.Empty<string>());

			AssertResult(null, countryUS, Core.Constants.TransportModes.Air, new string[] { "Empty-US-AIR" });
			AssertResult(countryCN, null, Core.Constants.TransportModes.Air, new string[] { "CN-Empty-AIR" });
			AssertResult(countryCN, countryUS, "", new string[] { "CN-US-Empty" });

			AssertResult(null, countryUS, "", new string[] { "Empty-US-Empty" });
			AssertResult(countryCN, null, "", new string[] { "CN-Empty-Empty" });

			Assert(true);
		}

		void AssertResult(RefCountry origin, RefCountry destination, string transportMode, string[] expectNoteContents)
		{
			var notes = Factory.New<DummyEnterpriseBusinessObject>().Notes;
			RefCountryRulesHelper.AddRulesToNotes(origin, destination, notes, transportMode);
			if (expectNoteContents.Length > 0)
			{
				var countryRulesNote = notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRules.Description).First().ST_NoteText;
				foreach (var item in expectNoteContents)
				{
					Assert(countryRulesNote.Contains(item));
				}
			}
		}

		public void TestValidateRule()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			bizO.Z0_Description = "!!!aSdJdsakldfjgkldfg";
			var rule = AddNewRule("CN", "", "\"<ChangeCase(\"<Z0_Description>\",L)>\".Contains(\"asdj\")", false, Core.Constants.TransportModes.Sea, true, false);
			var rule2 = AddNewRule("CN", "", "\"<ChangeCase(\"<Z0_Description>\",L)>\".Contains(\"asdj\")", false, Core.Constants.TransportModes.Sea, true, true);
			var notes = bizO.Notes;
			Factory.Save();

			var countryCN = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			var countryAU = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			RefCountryRulesHelper.AddRulesToNotes(countryCN, null, notes, "SEA", false, true);
			var note = notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesValidation.Description).First(x => x.ST_NoteText.Contains(RefCountryRulesHelper.ValidationError));
			var note2 = notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesValidation.Description).First(x => !x.ST_NoteText.Contains(RefCountryRulesHelper.ValidationError));
			Assert(note.ReadOnly);
			Assert(note2.ReadOnly);

			note.RunPreSaveValidation();
			AssertHasError("should have error now", note.ST_NoteTextInfo, string.Format("The DummyBizo cannot be saved due to a violation of the following Country/Region Validation Rule: {0}", rule.R7_Notes));
			note2.RunPreSaveValidation();
			AssertHasWarning("should have warning now", note2.ST_NoteTextInfo, string.Format("The DummyBizo violates the following Country/Region Validation Rule: {0}", rule.R7_Notes));

			bizO.Z0_Description = "asd-jh";

			note.RunPreSaveValidation();
			AssertNoErrors(note.ST_NoteTextInfo);
			note2.RunPreSaveValidation();
			AssertNoErrors(note2.ST_NoteTextInfo);
		}

		public void TestAddRulesToNotes()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var notes = bizO.Notes;
			AddNewRule("CN", "US", "This is the text from CN to US.", true);
			AddNewRule("CN", "AU", "This is the text from CN to AU.", false, Core.Constants.TransportModes.Air);
			AddNewRule("CN", "", "This is the text from CN to Anywhere.", false, Core.Constants.TransportModes.Sea);
			var rule = AddNewRule("CN", "", "\"<Z0_Description>\" == \"asdj\"", false, Core.Constants.TransportModes.Sea, true, false);
			var rule2 = AddNewRule("CN", "", "\"<Z0_Description>\" == \"adfgdf\"", false, Core.Constants.TransportModes.Sea, true, true);
			Factory.Save();

			var countryCN = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			var countryAU = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			RefCountryRulesHelper.AddRulesToNotes(countryCN, null, notes, "SEA", false, false);
			RefCountryRulesHelper.AddRulesToNotes(countryCN, null, notes, "SEA", false, true);
			AssertNull(notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRules.Description).FirstOrDefault());
			var validationNoteA = notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesValidation.Description).First(x => x.ST_NoteText.Contains("\"<Z0_Description>\" == \"asdj\""));
			var validationNoteB = notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesValidation.Description).First(x => x.ST_NoteText.Contains("\"<Z0_Description>\" == \"adfgdf\""));
			Assert(notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesInternal.Description).FirstOrDefault().ST_NoteText.Contains("This is the text from CN to Anywhere."));
			Assert(!notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesInternal.Description).FirstOrDefault().ST_NoteText.Contains("This is the text from CN to AU."));
			AssertEquals(2, notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesValidation.Description).Length);

			RefCountryRulesHelper.AddRulesToNotes(countryCN, countryAU, notes, "SEA", false, false);
			RefCountryRulesHelper.AddRulesToNotes(countryCN, countryAU, notes, "SEA", false, true);

			AssertNull(notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRules.Description).FirstOrDefault());
			Assert(notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesInternal.Description).FirstOrDefault().ST_NoteText.Contains("This is the text from CN to Anywhere."));
			AssertEquals(2, notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesValidation.Description).Length);
			AssertEquals(validationNoteA, notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesValidation.Description).First(x => x.ST_NoteText.Contains("\"<Z0_Description>\" == \"asdj\"")));
			AssertEquals(validationNoteB, notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesValidation.Description).First(x => x.ST_NoteText.Contains("\"<Z0_Description>\" == \"adfgdf\"")));

			rule2.Delete();

			RefCountryRulesHelper.AddRulesToNotes(countryCN, countryAU, notes, "SEA", false, true);
			AssertEquals(1, notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesValidation.Description).Length);
			AssertEquals(validationNoteA, notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesValidation.Description).First(x => x.ST_NoteText.Contains("\"<Z0_Description>\" == \"asdj\"")));

			rule.R7_Notes = "\"<Z0_Description>\" == \"adfgddff\"";

			RefCountryRulesHelper.AddRulesToNotes(countryCN, countryAU, notes, "SEA", false, true);
			AssertEquals(1, notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesValidation.Description).Length);
			Assert(notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesValidation.Description).Select(x => x.ST_NoteText).Any(x => x.Contains("\"<Z0_Description>\" == \"adfgddff\"")));

			RefCountryRulesHelper.OverrideRule(bizO, bizO.Logs, rule.PK, bizO.Factory);

			RefCountryRulesHelper.AddRulesToNotes(countryCN, countryAU, notes, "SEA", false, true);
			AssertNull(notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesValidation.Description).FirstOrDefault());
		}

		public void TestAddRulesToNotesWithFilters()
		{
			var notes = Factory.New<DummyEnterpriseBusinessObject>().Notes;
			var rule1 = AddNewRule("CN", "US", "rule01", true);
			var rule2 = AddNewRule("CN", "US", "rule02", false);
			var rule3 = AddNewRule("CN", "US", "rule03", true, Core.Constants.TransportModes.Sea);
			var rule4 = AddNewRule("CN", "US", "rule04", false, Core.Constants.TransportModes.Air);
			var rule5 = AddNewRule("CN", "AU", "rule05", true);
			var rule6 = AddNewRule("CN", "AU", "rule06", false);
			var rule7 = AddNewRule("CN", "AU", "rule07", true, Core.Constants.TransportModes.Air);
			var rule8 = AddNewRule("CN", "AU", "rule08", false, Core.Constants.TransportModes.Air);
			var rule9 = AddNewRule("CN", "", "rule09", true);
			var rule10 = AddNewRule("CN", "", "rule10", true, Core.Constants.TransportModes.Air);
			var rule11 = AddNewRule("", "CN", "rule11", true);
			var rule12 = AddNewRule("", "CN", "rule12", true, Core.Constants.TransportModes.Air);
			Factory.Save();

			var countryCN = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			var countryAU = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var countryUS = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");

			#region CN to Anywhere

			notes.RemoveAndDeleteAll();
			RefCountryRulesHelper.AddRulesToNotes(countryCN, null, notes);
			AssertCountryRulesContains(notes, rule1, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule2, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);
			AssertCountryRulesContains(notes, rule3, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule4, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);
			AssertCountryRulesContains(notes, rule5, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule6, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);
			AssertCountryRulesContains(notes, rule7, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule8, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);
			AssertCountryRulesContains(notes, rule9, PredefinedNoteTypes.Instance.CountryRules.Description);
			AssertCountryRulesContains(notes, rule10, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule11, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule11, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);
			AssertCountryRulesContains(notes, rule12, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule12, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);

			#endregion

			#region CN to AU

			notes.RemoveAndDeleteAll();
			RefCountryRulesHelper.AddRulesToNotes(countryCN, countryAU, notes);
			AssertCountryRulesContains(notes, rule1, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule2, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);
			AssertCountryRulesContains(notes, rule3, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule4, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);
			AssertCountryRulesContains(notes, rule5, PredefinedNoteTypes.Instance.CountryRules.Description);
			AssertCountryRulesContains(notes, rule6, PredefinedNoteTypes.Instance.CountryRulesInternal.Description);
			AssertCountryRulesContains(notes, rule7, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule8, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);
			AssertCountryRulesContains(notes, rule9, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule10, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule11, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule11, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);
			AssertCountryRulesContains(notes, rule12, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule12, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);

			#endregion

			#region CN to US with TransportMode 'SEA'

			notes.RemoveAndDeleteAll();
			RefCountryRulesHelper.AddRulesToNotes(countryCN, countryUS, notes, Core.Constants.TransportModes.Sea);
			AssertCountryRulesContains(notes, rule1, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule2, PredefinedNoteTypes.Instance.CountryRulesInternal.Description);
			AssertCountryRulesContains(notes, rule3, PredefinedNoteTypes.Instance.CountryRules.Description);
			AssertCountryRulesContains(notes, rule4, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);
			AssertCountryRulesContains(notes, rule5, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule6, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);
			AssertCountryRulesContains(notes, rule7, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule8, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);
			AssertCountryRulesContains(notes, rule9, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule10, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule11, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule11, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);
			AssertCountryRulesContains(notes, rule12, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule12, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);

			#endregion

			#region Anywhere to CN

			notes.RemoveAndDeleteAll();
			RefCountryRulesHelper.AddRulesToNotes(null, countryCN, notes);
			AssertCountryRulesContains(notes, rule1, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule2, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);
			AssertCountryRulesContains(notes, rule3, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule4, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);
			AssertCountryRulesContains(notes, rule5, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule6, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);
			AssertCountryRulesContains(notes, rule7, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule8, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);
			AssertCountryRulesContains(notes, rule9, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule10, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule11, PredefinedNoteTypes.Instance.CountryRules.Description);
			AssertCountryRulesContains(notes, rule11, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);
			AssertCountryRulesContains(notes, rule12, PredefinedNoteTypes.Instance.CountryRules.Description, false);
			AssertCountryRulesContains(notes, rule12, PredefinedNoteTypes.Instance.CountryRulesInternal.Description, false);

			#endregion

			#region AU to US

			notes.RemoveAndDeleteAll();
			RefCountryRulesHelper.AddRulesToNotes(countryAU, countryUS, notes);
			AssertNull(notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRules.Description).FirstOrDefault());
			AssertNull(notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesInternal.Description).FirstOrDefault());

			#endregion

		}

		public void TestAddRulesToNotesWithExistingNotes()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			bizO.Notes.AddNew(false, PredefinedNoteTypes.Instance.CountryRules.Description, "test");
			AssertEquals("Pre-condition: country rule note already exists", 1, bizO.Notes.VisibleNotes.Count);

			AddNewRule("CN", "US", "This is the text from CN to US.", true);
			Factory.Save();

			var notes = bizO.Notes;
			var countryCN = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			var countryAU = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");

			RefCountryRulesHelper.AddRulesToNotes(countryCN, countryAU, notes);
			AssertEquals("The rule note should not be added", 1, notes.VisibleNotes.Count);
		}

		public void TestAddRulesToNotesWithSameOriDesBeMerged()
		{
			var notes = Factory.New<DummyEnterpriseBusinessObject>().Notes;
			AddNewRule("CN", "US", "CN to US 1, is client visible", true);
			AddNewRule("CN", "US", "CN to US 2, is client visible", true);
			AddNewRule("CN", "US", "CN to US 3, is client visible", true);
			AddNewRule("CN", "US", "CN to US 1, is not client visible", false);
			AddNewRule("CN", "US", "CN to US 2, is not client visible", false);
			AddNewRule("CN", "US", "CN to US 3, is not client visible", false);
			Factory.Save();

			var countryCN = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			var countryUS = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");

			RefCountryRulesHelper.AddRulesToNotes(countryCN, countryUS, notes);

			var countryRulesNote = notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRules.Description).First().ST_NoteText;
			AssertEquals(1, Regex.Matches(countryRulesNote, "China to United States:").Count);
			AssertEquals(1, Regex.Matches(countryRulesNote, "CN to US 1, is client visible").Count);
			AssertEquals(1, Regex.Matches(countryRulesNote, "CN to US 2, is client visible").Count);
			AssertEquals(1, Regex.Matches(countryRulesNote, "CN to US 3, is client visible").Count);

			var countryRulesInternalNote = notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRulesInternal.Description).First().ST_NoteText;
			AssertEquals(1, Regex.Matches(countryRulesInternalNote, "China to United States:").Count);
			AssertEquals(1, Regex.Matches(countryRulesInternalNote, "CN to US 1, is not client visible").Count);
			AssertEquals(1, Regex.Matches(countryRulesInternalNote, "CN to US 2, is not client visible").Count);
			AssertEquals(1, Regex.Matches(countryRulesInternalNote, "CN to US 3, is not client visible").Count);
		}

		public void TestEmptyNotesShouldNotBeAddedToNotes()
		{
			var notes = Factory.New<DummyEnterpriseBusinessObject>().Notes;
			AddNewRule("CN", "US", "", true);

			var countryCN = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			var countryUS = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");
			RefCountryRulesHelper.AddRulesToNotes(countryCN, countryUS, notes);

			AssertNull(notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRules.Description).FirstOrDefault());
		}

		public void TestTransportModeBeTranslated()
		{
			var notes = Factory.New<DummyEnterpriseBusinessObject>().Notes;
			AddNewRule("CN", "US", "CN to US 1, SEA", true, "SEA");
			AddNewRule("CN", "US", "CN to US 2, OTH", true, "OTH");
			Factory.Save();

			var countryCN = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			var countryUS = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");
			RefCountryRulesHelper.AddRulesToNotes(countryCN, countryUS, notes, "FSA");

			var countryRulesNote = notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRules.Description).First().ST_NoteText;
			Assert(countryRulesNote.Contains("CN to US 2, OTH"));
			Assert(!countryRulesNote.Contains("CN to US 1, SEA"));

			notes.RemoveAndDeleteAll();
			RefCountryRulesHelper.AddRulesToNotes(countryCN, countryUS, notes, "FAS");

			countryRulesNote = notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRules.Description).First().ST_NoteText;
			Assert(countryRulesNote.Contains("CN to US 2, OTH"));
			Assert(!countryRulesNote.Contains("CN to US 1, SEA"));
		}

		void AssertCountryRulesContains(Notes notes, RefCountryRules rule, string noteType, bool contain = true)
		{
			if (contain)
			{
				Assert(notes.FindByDescription(noteType).FirstOrDefault().ST_NoteText.Contains(rule.R7_Notes));
			}
			else
			{
				Assert(!notes.FindByDescription(noteType).FirstOrDefault()?.ST_NoteText.Contains(rule.R7_Notes) ?? true);
			}
		}

		public void TestNoNotesWhenNoRules()
		{
			var notes = Factory.New<DummyEnterpriseBusinessObject>().Notes;
			RefCountryRulesHelper.AddRulesToNotes(null, null, notes);

			AssertNull(notes.FindByDescription(PredefinedNoteTypes.Instance.CountryRules.Description).FirstOrDefault());
		}

		RefCountryRules AddNewRule(string original, string destination, string notes, bool isClientVisible, string transportMode = "", bool isValidation = false, bool validationError = false)
		{
			var rule = Factory.NewWithValidTestData<RefCountryRules>();
			rule.R7_RN_NKOrigin = original;
			rule.R7_RN_NKDestination = destination;
			rule.R7_Notes = notes;
			rule.R7_IsClientVisible = isClientVisible;
			rule.R7_TransportMode = transportMode;
			rule.R7_IsValidationRule = isValidation;
			rule.R7_IsError = validationError;
			return rule;
		}
	}
}
