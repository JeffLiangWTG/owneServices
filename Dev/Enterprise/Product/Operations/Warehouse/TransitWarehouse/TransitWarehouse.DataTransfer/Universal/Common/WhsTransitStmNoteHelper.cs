using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitStmNoteHelper : DataObjectReader
	{
		public WhsTransitStmNoteHelper(IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(logger)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
		}
		readonly UniversalObjectFactory Factory;

		public void AddOrUpdateUnrecognisedAdditionalReferenceNote(ZGuid parentPK, string tableName, IEnumerable<AdditionalReference> refNumbers)
		{
			var noteQuery = new ZQuery();
			noteQuery.AddToFilter(StmNoteSchema.ST_Table, tableName);
			noteQuery.AddToFilter(StmNoteSchema.ST_ParentID, parentPK);
			noteQuery.AddToFilter(StmNoteSchema.ST_Description, PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes.Code);

			var existingNotes = Factory.RowFactory.Load(StmNoteSchema.Constants.TableName, noteQuery);
			if (existingNotes.Length > 0)
			{
				var existingNote = GetColumnIndexerFromRow(existingNotes[0]);
				var noteText = existingNote.GetValue(StmNoteSchema.ST_NoteText);
				foreach (var refNumber in refNumbers)
				{
					var noteString = GenerateUnrecognisedAdditionalReferenceItemString(refNumber);
					if (!noteText.Contains(noteString, System.StringComparison.CurrentCulture))
					{
						noteText += $"{System.Environment.NewLine}{System.Environment.NewLine}{noteString}";
					}
				}
				existingNote.SetValue(StmNoteSchema.ST_NoteText, noteText);
			}
			else
			{
				var noteText = string.Join($"{System.Environment.NewLine}{System.Environment.NewLine}", refNumbers.Select(r => GenerateUnrecognisedAdditionalReferenceItemString(r)));
				PopulateStmNote(parentPK, tableName, noteText, PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes.Code, isCustomDescription: false);
			}
		}

		static string GenerateUnrecognisedAdditionalReferenceItemString(AdditionalReference refNumber)
		{
			return Res.GetString("473a7607-daf8-45fd-95a6-7730252b71f1", "Type: {0}", CusEntryNumberTransitLogHelper.GetEntryType(refNumber)) + "\r\n" +
				Res.GetString("371d3182-31f3-41be-9fe4-74a697147d18", "Number: {0}", CusEntryNumberTransitLogHelper.GetEntryNum(refNumber)) + "\r\n" +
				Res.GetString("1fc66640-ddb8-4cc1-9167-00fe23cbfa64", "Country: {0}", CusEntryNumberTransitLogHelper.GetCountryCode(refNumber));
		}

		public void AddOrUpdateLoadListWarningNote(ZGuid loadListPK, string noteWarnings)
		{
			var noteQuery = new ZQuery();
			noteQuery.AddToFilter(StmNoteSchema.ST_Table, WhsItemDispatchLoadListSchema.Constants.TableName);
			noteQuery.AddToFilter(StmNoteSchema.ST_ParentID, loadListPK);
			noteQuery.AddToFilter(StmNoteSchema.ST_Description, PredefinedNoteTypes.Instance.DispatchInstructionWarning.Code);

			var existingNoteRow = Factory.RowFactory.Load(StmNoteSchema.Constants.TableName, noteQuery).FirstOrDefault();
			if (existingNoteRow != null)
			{
				var existingNote = GetColumnIndexerFromRow(existingNoteRow);
				var noteText = existingNote.GetValue(StmNoteSchema.ST_NoteText);

				if (!noteText.Contains(noteWarnings))
				{
					noteText += $"{System.Environment.NewLine}{System.Environment.NewLine}{noteWarnings}";
					existingNote.SetValue(StmNoteSchema.ST_NoteText, noteText);
				}
			}
			else
			{
				PopulateStmNote(loadListPK, WhsItemDispatchLoadListSchema.Constants.TableName, noteWarnings, PredefinedNoteTypes.Instance.DispatchInstructionWarning.Code, isCustomDescription: false, noteType: nameof(StmNoteVisibility.PUB));
			}
		}

		public void PopulateStmNote(ZGuid parentPK, string tableName, ZString noteText, ZString noteDescription, bool isCustomDescription, string noteType = null, string noteContext = null)
		{
			if (!noteDescription.IsEmpty)
			{
				var row = Factory.RowFactory.NewRowWithPK(StmNoteSchema.Instance);
				SetValue(row, StmNoteSchema.ST_ParentID, parentPK);
				SetValue(row, StmNoteSchema.ST_Table, tableName);
				SetValue(row, StmNoteSchema.ST_NoteType, !string.IsNullOrEmpty(noteType) ? noteType : nameof(StmNoteVisibility.INT));
				SetValue(row, StmNoteSchema.ST_NoteText, noteText);
				SetValue(row, StmNoteSchema.ST_Description, noteDescription);
				SetValue(row, StmNoteSchema.ST_IsCustomDescription, false);
				SetValue(row, StmNoteSchema.ST_NoteContext, !string.IsNullOrEmpty(noteContext) ? noteContext : "AAA"); // particular value set for note context
				SetValue(row, StmNoteSchema.ST_SystemCreateTimeUtc, ZDateTime.UtcNow);
				SetValue(row, StmNoteSchema.ST_SystemCreateUser, User.InterchangeUserCode);
				SetValue(row, StmNoteSchema.ST_SystemLastEditTimeUtc, ZDateTime.UtcNow);
				SetValue(row, StmNoteSchema.ST_SystemLastEditUser, User.InterchangeUserCode);
			}
		}

		public static DataObjectList<Note> GetSupportedNotesByVisibilityTypes<T>(T noteParentBizo, DataObjectList<Note> notes, params string[] visibilities) where T : IStmNoteParent
		{
			var result = new DataObjectList<Note>();
			foreach (var note in notes)
			{
				if (note.Visibility != null
					&& visibilities.Any(v => v.Equals(note.Visibility.Code.GetValueOrDefault(), StringComparison.OrdinalIgnoreCase)))
				{
					var supportedNoteTypes = noteParentBizo.NoteTypes.Cast<PredefinedNoteType>().ToList();

					// Only import notes that are not readonly after creation unless it is a TWH supported type
					var isNoteReadonly = PredefinedNoteTypes.Instance.NoteTypeByDescription(note.Description)?.IsReadOnlyAfterAdd ?? false;
					if (!isNoteReadonly || supportedNoteTypes.Any(t => t.Description.Equals(note.Description)))
					{
						result.Add(note);
					}
				}
			}

			return result;
		}
	}
}
