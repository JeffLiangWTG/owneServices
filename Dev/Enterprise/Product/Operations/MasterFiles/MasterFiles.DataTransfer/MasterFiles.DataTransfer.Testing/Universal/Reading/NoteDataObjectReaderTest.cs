using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class NoteDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestEmptyNoteContext()
		{
			var noteDataObject = SetupNote();
			noteDataObject.NoteContext = new NoteContext();
			var note = Factory.New<StmNote>();
			var reader = new NoteDataObjectReader(noteDataObject, Logger, Factory, Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>(), data => note);
			AssertEquals("???", reader.ReadIntoBusinessObject().ST_NoteContext);
		}

		public void TestBizObjectProvider()
		{
			var noteDataObject = SetupNote();
			noteDataObject.NoteText = "Feee-lix the cat, what a wonderful-wonderful cat.";

			var note = Factory.New<StmNote>();

			var reader = new NoteDataObjectReader(noteDataObject, Logger, Factory, Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>(), data => note);
			AssertEquals(note, reader.ReadIntoBusinessObject());
		}

		public void TestBasicNoteLevelFieldMappings()
		{
			var noteDataObject = SetupNote();
			noteDataObject.NoteText = "Feee-lix the cat, what a wonderful-wonderful cat.";
			noteDataObject.IsCustomDescription = true;

			var reader = new NoteDataObjectReader(noteDataObject, Logger, Factory, Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>());
			var noteBO = reader.ReadIntoBusinessObject();

			AssertNotNull(noteBO);

			#region Check Contents Of Business Object

			CombineAssertions(delegate
			{
				AssertContents(noteBO);
				AssertEquals("noteBO.ST_IsCustomDescription", true, noteBO.ST_IsCustomDescription);
				AssertEquals("noteBO.ST_NoteText", "Feee-lix the cat, what a wonderful-wonderful cat.", noteBO.ST_NoteDataAsText);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestIsCustomDescriptionSetByDODescription()
		{
			string expectedLogMessage = @"
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
";
			AssertIsCustomDescriptionSetByDODescription("Custom note desc", true, true, expectedLogMessage);

			expectedLogMessage = @"
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
Warning - Description(value: Custom note desc) is a custom note. IsCustomDescription(value: False/empty) was ignored.";
			AssertIsCustomDescriptionSetByDODescription("Custom note desc", false, true, expectedLogMessage);

			expectedLogMessage = @"
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
Warning - Description(value: Detailed Goods Description) is NOT a custom note. IsCustomDescription(value: True) was ignored.";
			AssertIsCustomDescriptionSetByDODescription("Detailed Goods Description", true, false, expectedLogMessage);

			expectedLogMessage = @"
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
";
			AssertIsCustomDescriptionSetByDODescription("Detailed Goods Description", false, false, expectedLogMessage);
		}

		void AssertIsCustomDescriptionSetByDODescription(string dataObjectdescription, bool dataObjectIsCustomDescription, bool expectedBOIsCustomDescription, string expectedLogMessage)
		{
			var noteDataObject = SetupNote(dataObjectdescription);
			noteDataObject.IsCustomDescription = dataObjectIsCustomDescription;

			Logger.ClearLogs();
			var noteParentBO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var reader = new NoteDataObjectReader(noteDataObject, Logger, Factory, noteParentBO);
			var noteBO = reader.ReadIntoBusinessObject();

			AssertNotNull(noteBO);
			AssertEquals("noteBO.ST_IsCustomDescription", expectedBOIsCustomDescription, noteBO.ST_IsCustomDescription);
			AssertMultilineASCIIEquals("logger.Logs", expectedLogMessage.Trim(), Logger.Logs);
		}

		public void TestIsCustomDescriptionSetByRegistry()
		{
			SetupCustomNotesRegistry();
			string expectedLogMessage = @"
Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
";
			AssertIsCustomDescriptionSetByDODescription("BarcodeNote", false, false, expectedLogMessage);
		}

		public void TestIsCustomDescriptionSetByMasterBizoNoteTypes_Include()
		{
			var dataObjectdescription = "HaveAGreatDay";

			var noteDataObject = SetupNote(dataObjectdescription);
			noteDataObject.IsCustomDescription = true;

			var noteType = new PredefinedNoteType((ZArchitecture.Core.NoResString)dataObjectdescription, StmNoteVisibility.PUB, false, false, false, false);
			var noteTypes = new NoteTypeCollection();
			noteTypes.Add(noteType);

			Logger.ClearLogs();

			var noteParentBO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var reader = new NoteDataObjectReader(noteDataObject, Logger, Factory, noteParentBO, masterBizoNoteTypes: noteTypes);
			var noteBO = reader.ReadIntoBusinessObject();

			AssertNotNull(noteBO);
			AssertEquals("noteBO.ST_IsCustomDescription", false, noteBO.ST_IsCustomDescription);
			AssertMultilineASCIIEquals("logger.Logs", @"Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
Warning - Description(value: HaveAGreatDay) is NOT a custom note. IsCustomDescription(value: True) was ignored.", Logger.Logs);
		}

		public void TestIsCustomDescriptionSetByMasterBizoNoteTypes_Exclude()
		{
			var noteDataObject = SetupNote("HaveAGreatDay");
			noteDataObject.IsCustomDescription = false;

			Logger.ClearLogs();

			var noteParentBO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var reader = new NoteDataObjectReader(noteDataObject, Logger, Factory, noteParentBO, masterBizoNoteTypes: new NoteTypeCollection());
			var noteBO = reader.ReadIntoBusinessObject();

			AssertNotNull(noteBO);
			AssertEquals("noteBO.ST_IsCustomDescription", true, noteBO.ST_IsCustomDescription);
			AssertMultilineASCIIEquals("logger.Logs", $@"Information - No matching StmNote found, creating new StmNote.
Information - Populating StmNote...
Warning - Description(value: HaveAGreatDay) is a custom note. IsCustomDescription(value: False/empty) was ignored.", Logger.Logs);
		}

		public void TestLoadingOfNotes()
		{
			var noteBO = Factory.New<StmNote>();
			noteBO.ST_NoteContext = "BEB";
			noteBO.ST_Description = "CAT EATER!!";
			noteBO.ST_NoteDataAsText = "THIS FIELD WILL NOT BE TOUCHED";

			var noteParentBO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			noteParentBO.Notes.Add(noteBO);

			Factory.SaveForTesting();

			var noteDataObject = SetupNote();
			noteDataObject.IsCustomDescription = true;

			var reader = new NoteDataObjectReader(noteDataObject, Logger, Factory, noteParentBO);
			noteBO = reader.ReadIntoBusinessObject();

			AssertNotNull(noteBO);

			#region Check Contents Of Business Object

			CombineAssertions(delegate
			{
				AssertContents(noteBO);
				AssertEquals("noteBO.ST_IsCustomDescription", true, noteBO.ST_IsCustomDescription);
				AssertEquals("noteBO.ST_NoteText", "THIS FIELD WILL NOT BE TOUCHED", noteBO.ST_NoteDataAsText);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching StmNote.
Information - Populating StmNote...
".Trim(), Logger.Logs);
			});

			#endregion
		}

		public void TestReadingVisibleCompanyCode()
		{
			var noteBO = Factory.New<StmNote>();
			noteBO.ST_NoteContext = "BEB";
			noteBO.ST_GC_RelatedCompany = Guid.Empty;

			var noteParentBO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			noteParentBO.Notes.Add(noteBO);

			Factory.SaveForTesting();

			var noteDataObject = SetupNote(Guid.Empty);
			var reader = new NoteDataObjectReader(noteDataObject, Logger, Factory, noteParentBO);
			var readerNote = reader.ReadIntoBusinessObject();

			AssertNotNull(readerNote);
			AssertContents(readerNote);
			AssertEquals("readerNtoe.ST_GC_RelatedCompany", Guid.Empty, readerNote.ST_GC_RelatedCompany);

			noteBO.ST_GC_RelatedCompany = GlbCompany.CurrentCompany.PK;
			Factory.SaveForTesting();

			noteDataObject = SetupNote(GlbCompany.CurrentCompany.PK.ToGuid());
			reader = new NoteDataObjectReader(noteDataObject, Logger, Factory, noteParentBO);
			readerNote = reader.ReadIntoBusinessObject();

			AssertNotNull(readerNote);
			AssertContents(readerNote);
			AssertEquals("readerNtoe.ST_GC_RelatedCompany", GlbCompany.CurrentCompany.PK, readerNote.ST_GC_RelatedCompany);
		}

		public void TestLoadingOfNotesWithEmptyStringForNoteContext()
		{
			Db.Connection.ExecuteNonQuery(@"IF (OBJECT_ID('Constraint_ST_NoteContext_NoCheck', 'C') IS NOT NULL)
										BEGIN
											ALTER TABLE dbo.StmNote NOCHECK CONSTRAINT Constraint_ST_NoteContext_NoCheck
										END");
			var noteBO = Factory.New<StmNote>();
			noteBO.ST_NoteContext = "";
			noteBO.ST_Description = "CAT EATER!!";
			noteBO.ST_NoteDataAsText = "THIS FIELD WILL NOT BE TOUCHED";

			var noteParentBO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			noteParentBO.Notes.Add(noteBO);

			Factory.SaveForTesting();

			var noteDataObject = SetupNote();
			noteDataObject.IsCustomDescription = true;
			noteDataObject.NoteContext = new NoteContext() { Code = "AAA", Description = "All Angels Anchor" };

			var reader = new NoteDataObjectReader(noteDataObject, Logger, Factory, noteParentBO);
			noteBO = reader.ReadIntoBusinessObject();

			AssertNotNull(noteBO);
			AssertEquals("noteBO.ST_NoteContext", "AAA", noteBO.ST_NoteContext);

			noteDataObject.NoteContext = new NoteContext() { Code = "", Description = "All Angels Anchor" };
			reader = new NoteDataObjectReader(noteDataObject, Logger, Factory, noteParentBO);
			noteBO = reader.ReadIntoBusinessObject();

			AssertNotNull(noteBO);

			#region Check Contents Of Business Object

			CombineAssertions(delegate
			{
				AssertEquals("noteBO.ST_NoteContext", "???", noteBO.ST_NoteContext);
				AssertEquals("noteBO.ST_IsCustomDescription", true, noteBO.ST_IsCustomDescription);
				AssertEquals("noteBO.ST_NoteText", "THIS FIELD WILL NOT BE TOUCHED", noteBO.ST_NoteDataAsText);
				AssertMultilineASCIIEquals("logger.Logs", @"
Information - Successfully loaded matching StmNote.
Information - Populating StmNote...
Information - Successfully loaded matching StmNote.
Information - Populating StmNote...
".Trim(), Logger.Logs);
			});

			#endregion

			ErrorReporter.Clear();
		}

		public void TestTextOnlyNoteWithExcessiveLengthTrimmed()
		{
			var predefined = PredefinedNoteTypes.Instance.DetailedGoodsDescription;
			var noteDataObject = SetupNote(predefined.Description); // must be predefined to be text only

			var nonsenseText = string.Empty.PadRight(predefined.TextOnlyMaxLength, 'X');

			var noteText = $"Feline, the 'other' white meat.\n{nonsenseText}\nIn keeping up with the unit test theme";
			Assert("Notetext length should be longer than max allowed for predefined note", noteText.Length > predefined.TextOnlyMaxLength);

			noteDataObject.IsCustomDescription = false;
			noteDataObject.NoteText = noteText;
			AssertEquals("Note.Notetext should not be modified on set", noteText, noteDataObject.NoteText);

			var note = Factory.New<StmNote>();

			var reader = new NoteDataObjectReader(noteDataObject, Logger, Factory, Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>());
			note = reader.ReadIntoBusinessObject();

			AssertEquals("StmNote should have predefined note type description", predefined.Description, note.ST_Description.ToString());
			AssertEquals("StmNote should have default Text Only Max length", note.ST_NoteTextInfo.MaxLength, predefined.TextOnlyMaxLength);
			Assert("StmNote.ST_NoteText should be trimmed to max length", note.ST_NoteText.Length == predefined.TextOnlyMaxLength);
			AssertContains("StmNote.ST_NoteText should contain '...trimmed to fit'", "...trimmed to fit", note.ST_NoteText);
			AssertContains("Warning not in Logs", "has been trimmed", Logger.Logs);
		}

		#region Implementation

		protected TestErrorLogger Logger
		{
			get
			{
				if (logger == null)
				{
					logger = new TestErrorLogger();
					var dataContext = new DataContext();
					var dataSource = new DataSource();
					dataSource.DataProvider = new DataProvider();
					dataSource.DataProvider.Code = GlbCompany.CurrentCompany.LicenceEnterpriseCode + GlbCompany.CurrentCompany.LicenceServerID + "XXX";
					dataSource.DataProvider.Type = DataProviderType.EnterpriseID;
					dataContext.DataSource = dataSource;
					logger.TopLevelDataObject = new Shipment { DataContext = dataContext };
				}
				return logger;
			}
		}
		TestErrorLogger logger;

		protected static Note SetupNote(string description = "CAT EATER!!")
		{
			var noteDataObject = new Note();

			noteDataObject.Description = description;
			noteDataObject.Visibility = new CodeDescriptionPair() { Code = nameof(StmNoteVisibility.PUB), Description = "Public" };
			noteDataObject.NoteContext = new NoteContext() { Code = "BEB", Description = "Baby Eats Banana" };

			return noteDataObject;
		}

		static Note SetupNote(Guid visibleCompanyPK)
		{
			var note = SetupNote();

			if (visibleCompanyPK != Guid.Empty)
			{
				var company = new BusinessObjectFactory().Load<GlbCompany>(visibleCompanyPK);
				note.VisibleCompany = new CodeDescriptionPair()
				{
					Code = company.GC_Code,
					Description = company.GC_Name,
				};
			}
			return note;
		}

		static void AssertContents(StmNote noteBO)
		{
			AssertEquals("noteBO.ST_Description", "CAT EATER!!", noteBO.ST_Description);
			AssertEquals("noteBO.ST_NoteContext", "BEB", noteBO.ST_NoteContext);
			AssertEquals("noteBO.ST_NoteType", "PUB", noteBO.ST_NoteType);
		}

		public void SetupCustomNotesRegistry()
		{
			var shipmentModule = new CustomNoteModuleAndCountry();
			shipmentModule.ModuleIDName = ModuleIDs.JobShipment.Name;

			var item = shipmentModule.CustomNoteTypesList.AddNew();
			item.IsTextOnly = true;
			item.IsAppendingNote = true;
			item.IsReadOnlyAfterAdd = false;
			item.ForceRead = false;
			item.DefaultVisibility = nameof(StmNoteVisibility.PUB);
			item.NoteName = "BarcodeNote";

			var collection = new CustomNoteTypes();
			collection.NoteModuleAndCountryList.Add(shipmentModule);

			SystemDataRegistry.Instance.CustomNotes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}
		#endregion
	}
}

