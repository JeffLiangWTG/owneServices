using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using EntryType = Enterprise.UniversalDataBuss.DataObjects.Universal.EntryType;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class AdditionalReferenceCollectionReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var dummy = Factory.New<TestDummyEnterpriseBusinessObject>();
			var logger = new TestErrorLogger();
			AssertExceptionThrown(typeof(ArgumentNullException), () => new TestAdditionalReferenceCollectionReader(null, null, Factory, dummy));
			AssertExceptionThrown(typeof(ArgumentNullException), () => new TestAdditionalReferenceCollectionReader(null, logger, null, dummy));
			AssertExceptionThrown(typeof(ArgumentNullException), () => new TestAdditionalReferenceCollectionReader(null, logger, Factory, null));

			new TestAdditionalReferenceCollectionReader(null, logger, Factory, dummy);
			var expectedMessage = "Should not use 'AdditionalReferenceCollectionReader' if your BusinessObject does not Support Notes or does not support the 'Unrecognized Additional Reference Types' Note Type. Type that failed: TestDummyEnterpriseBusinessObject";
			AssertEquals("Should throw exception if doesn't support notes or contain proper note type", expectedMessage, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
			dummy.DoesSupportNotes = true;
			new TestAdditionalReferenceCollectionReader(null, logger, Factory, dummy);
			AssertEquals("Should throw exception if doesn't support notes or contain proper note type", expectedMessage, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
			dummy.DoesSupportNotes = false;
			dummy.AddToNoteTypeCollection(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes);
			new TestAdditionalReferenceCollectionReader(null, logger, Factory, dummy);
			AssertEquals("Should throw exception if doesn't support notes or contain proper note type", expectedMessage, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
			dummy.DoesSupportNotes = true;
			new TestAdditionalReferenceCollectionReader(null, logger, Factory, dummy);
			AssertEquals("Should not throw exception if supports notes and contains proper note type", "", ErrorReporter.LastMessageReported);
		}

		#endregion

		public void TestCreateAdditionalReferenceNote()
		{
			var unrecognisedAdditionalReferenceType = PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes;

			var dummy = Factory.New<TestDummyEnterpriseBusinessObject>();
			dummy.DoesSupportNotes = true;
			dummy.SupportNoteType();
			dummy.AddToNoteTypeCollection(unrecognisedAdditionalReferenceType);

			var expectedResult1 = "";
			var additionalReferences1 = BuildAdditionalReferencesByLength("XXX", 200, ref expectedResult1);

			var logger = new TestErrorLogger();
			var reader = new TestAdditionalReferenceCollectionReader(additionalReferences1, logger, Factory, dummy);
			reader.ReadIntoCollection();
			var notes = dummy.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes.Description);
			AssertEquals(expectedResult1, notes[0].ST_NoteText);

			Factory.SaveForTesting();
		}

		public void TestAdditionalReferenceTextExeedingLimit()
		{
			var unrecognisedAdditionalReferenceType = PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes;

			var noteString = "";
			var noteStringFuncQueue = new Queue<Func<string>>();
			var i = 0;

			while (noteString.Length <= 500)
			{
				var str = string.Format("Type: {0}\r\nNumber: {1}\r\n\r\n", "XXX", i.ToString());
				noteString += str;

				noteStringFuncQueue.Enqueue(() => str);

				i++;
			}

			var dummy = Factory.New<TestDummyEnterpriseBusinessObject>();
			dummy.DoesSupportNotes = true;
			dummy.SupportNoteType();
			dummy.AddToNoteTypeCollection(unrecognisedAdditionalReferenceType);

			var additionalReferences = new DataObjectList<AdditionalReference>();

			var logger = new TestErrorLogger();
			var reader = new TestAdditionalReferenceCollectionReader(additionalReferences, logger, Factory, dummy);

			var maxLength = 400;
			var resultString = reader.GetAdditionalReferenceStringWithLimitForTest(maxLength, noteStringFuncQueue, unrecognisedAdditionalReferenceType);
			Assert(logger.Logs.Contains($"{unrecognisedAdditionalReferenceType} notes exceeded limit of {maxLength} characters"));
			Assert(resultString.Length <= maxLength);
		}

		public void TestUpdateExistingAdditionalReferenceNote()
		{
			var unrecognisedAdditionalReferenceType = PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes;

			var dummy = Factory.New<TestDummyEnterpriseBusinessObject>();
			dummy.DoesSupportNotes = true;
			dummy.SupportNoteType();
			dummy.AddToNoteTypeCollection(unrecognisedAdditionalReferenceType);

			var expectedResult1 = "";
			var additionalReferences1 = BuildAdditionalReferencesByLength("XXX", 200, ref expectedResult1);

			var logger = new TestErrorLogger();
			var reader = new TestAdditionalReferenceCollectionReader(additionalReferences1, logger, Factory, dummy);
			reader.ReadIntoCollection();
			var notes = dummy.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes.Description);
			AssertEquals(expectedResult1, notes[0].ST_NoteText);

			var expectedResult2 = "";
			var additionalReferences2 = BuildAdditionalReferencesByLength("YYY", 100, ref expectedResult2);
			reader = new TestAdditionalReferenceCollectionReader(additionalReferences2, logger, Factory, dummy);

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => reader.ReadIntoCollection());

				notes = dummy.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes.Description);
				AssertEquals(1, notes.Length);
				AssertEquals(expectedResult1 + expectedResult2, notes[0].ST_NoteText);

				AssertNoExceptionThrown(() => Factory.SaveForTesting());
			});
		}

		DataObjectList<AdditionalReference> BuildAdditionalReferencesByLength(string code, int maxLength, ref string resultString)
		{
			var additionalReferences = new DataObjectList<AdditionalReference>();
			var builder = new ZStringBuilder();
			var i = 0;

			while (builder.Length < maxLength)
			{
				var str = string.Format("Type: {0}\r\nNumber: {1}\r\n\r\n", code, i.ToString());
				additionalReferences.Add(new AdditionalReference { Type = new EntryType { Code = code }, ReferenceNumber = i.ToString() });
				builder.Append(str);

				i++;
			}

			resultString = builder.ToString();
			return additionalReferences;
		}

		#region Implementation

		class TestAdditionalReferenceCollectionReader : AdditionalReferenceCollectionReader<DummyBusinessObject, TestDummyEnterpriseBusinessObject>
		{
			public TestAdditionalReferenceCollectionReader(DataObjectList<AdditionalReference> additionalReferenceDataObjects, IXmlImportLogger logger, UniversalObjectFactory factory, TestDummyEnterpriseBusinessObject parent)
				: base(additionalReferenceDataObjects, logger, factory, parent)
			{
			}

			public string GetAdditionalReferenceStringWithLimitForTest(int maxLength, Queue<Func<string>> additionalReferenceQueue, PredefinedNoteType noteType)
			{
				return GetAdditionalReferenceStringWithLimit(maxLength, additionalReferenceQueue, noteType);
			}

			protected override bool IsInvalidAdditionalReferenceType(AdditionalReference dataObject)
			{
				return true;
			}

			protected override DummyBusinessObject[] BusinessObjects
			{
				get { return new List<DummyBusinessObject>() { Parent }.ToArray(); }
			}

			protected override void AddToCollection(DummyBusinessObject businessObject)
			{
				throw new NotImplementedException();
			}

			protected override void RemoveFromCollection(DummyBusinessObject businessObject)
			{
				return;
			}

			protected override DummyBusinessObject FindMatchingBusinessObject(AdditionalReference dataObject)
			{
				return null;
			}

			protected override DummyBusinessObject ReadIntoBusinessObject(AdditionalReference dataObject, DummyBusinessObject businessObject)
			{
				return null;
			}
		}

		class TestDummyEnterpriseBusinessObject : DummyEnterpriseBusinessObject
		{
			public TestDummyEnterpriseBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void SupportNoteType()
			{
				noteTypes = base.NoteTypesCore;
			}

			public override bool SupportsNotes
			{
				get { return DoesSupportNotes; }
			}

			public bool DoesSupportNotes;

			protected override NoteTypeCollection NoteTypesCore
			{
				get { return noteTypes ?? (noteTypes = base.NoteTypesCore); }
			}

			NoteTypeCollection noteTypes;

			public void AddToNoteTypeCollection(PredefinedNoteType noteType)
			{
				noteTypes.Add(noteType);
			}
		}

		#endregion
	}
}

