using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public abstract class AdditionalReferenceCollectionReader<T1, T2> : DataObjectCollectionReader<AdditionalReference, T1>
		where T1 : BusinessObject
		where T2 : BusinessObject, IStmNoteParent
	{
		protected AdditionalReferenceCollectionReader(DataObjectList<AdditionalReference> additionalReferenceDataObjects, IXmlImportLogger logger, UniversalObjectFactory factory, T2 parent)
			: base(additionalReferenceDataObjects)
		{
			this.logger = Argument.NotNull(logger, "IXmlImportLogger logger");
			this.factory = Argument.NotNull(factory, "UniversalObjectFactory factory");
			this.parent = Argument.NotNull(parent, "T parent");

			if (!parent.SupportsNotes || !parent.NoteTypes.Cast<PredefinedNoteType>().Contains(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes))
			{
				ErrorReporter.ReportOnce("AdditionalReferenceCollectionReader", string.Format("Should not use 'AdditionalReferenceCollectionReader' if your BusinessObject does not Support Notes or does not support the '{0}' Note Type. Type that failed: {1}",
					PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes.Description, typeof(T2).Name));
			}
		}

		#region Logger

		protected IXmlImportLogger Logger
		{
			get { return logger; }
		}

		readonly IXmlImportLogger logger;

		#endregion

		#region Factory

		protected UniversalObjectFactory Factory
		{
			get { return factory; }
		}

		readonly UniversalObjectFactory factory;

		#endregion

		#region Parent

		protected T2 Parent
		{
			get { return parent; }
		}

		readonly T2 parent;

		protected virtual Notes ParentNotes
		{
			get { return Parent.Notes; }
		}

		#endregion

		#region ReadIntoCollectionCore

		protected override void ReadIntoCollectionCore()
		{
			base.ReadIntoCollectionCore();

			if (AdditionalReferenceNumbersToAddToNote.Count > 0)
			{
				var unrecognisedAdditionalReferenceType = PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes;
				var notes = ParentNotes.FindByDescription(unrecognisedAdditionalReferenceType.Description);
				Queue<Func<string>> additionalReferenceQueue;

				if (notes.Length > 0)
				{
					var removedExistingReferenceList = AdditionalReferenceNumbersToAddToNote.Where(referenceFunc => notes.Any(note => !note.ST_NoteText.Contains(referenceFunc(), StringComparison.CurrentCulture))).ToList();
					additionalReferenceQueue = new Queue<Func<string>>(removedExistingReferenceList);
					var minNote = notes.OrderBy(note => note.ST_NoteText.Length).First();
					minNote.ST_NoteText += GetAdditionalReferenceStringWithLimit(unrecognisedAdditionalReferenceType.TextOnlyMaxLength - minNote.ST_NoteText.Length, additionalReferenceQueue, unrecognisedAdditionalReferenceType);
				}
				else
				{
					additionalReferenceQueue = new Queue<Func<string>>(AdditionalReferenceNumbersToAddToNote);
					var noteText = GetAdditionalReferenceStringWithLimit(unrecognisedAdditionalReferenceType.TextOnlyMaxLength, additionalReferenceQueue, unrecognisedAdditionalReferenceType);
					ParentNotes.AddNew(false, unrecognisedAdditionalReferenceType.Description, noteText);
				}

				Logger.Log(LogType.Warning, Res.GetString("d169fbae-55f8-4fca-ac6d-23d6eb54c888", "Unknown Additional References were found for '{0}' and not imported. These were added to the '{1}' Note.",
					Parent.HumanReadableName, unrecognisedAdditionalReferenceType.Description));
			}
		}

		protected string GetAdditionalReferenceStringWithLimit(int maxLength, Queue<Func<string>> additionalReferenceQueue, PredefinedNoteType noteType)
		{
			var builder = new ZStringBuilder();
			var separator = "\r\n\r\n";

			while (additionalReferenceQueue.Count > 0)
			{
				if (additionalReferenceQueue.Peek()().Length + builder.Length + separator.Length < maxLength)
				{
					builder.Append(additionalReferenceQueue.Dequeue()() + separator);
				}
				else
				{
					Logger.Log(LogType.Warning, Res.GetString("492d7bbc-3148-4713-8d17-5a1143f0c889", "{0} notes exceeded limit of {1} characters", noteType.ToString(), maxLength));
					break;
				}
			}

			return builder.ToString();
		}

		#endregion

		#region SkipEntity

		protected override bool SkipEntity(AdditionalReference dataObject)
		{
			bool result = base.SkipEntity(dataObject);
			if (IsInvalidAdditionalReferenceType(dataObject))
			{
				AdditionalReferenceNumbersToAddToNote.Add(() => Res.GetString("1aa7a699-268f-4008-975b-c04faa48ab6e", "Type: {0}\r\nNumber: {1}", dataObject.Type.ToStringContents(), dataObject.ReferenceNumber));
				result = true;
			}

			return result;
		}

		protected abstract bool IsInvalidAdditionalReferenceType(AdditionalReference dataObject);

		#region AdditionalReferenceNumbersToAddToNote

		List<Func<string>> AdditionalReferenceNumbersToAddToNote
		{
			get { return additionalReferenceNumbersToAddToNote ?? (additionalReferenceNumbersToAddToNote = new List<Func<string>>()); }
		}

		List<Func<string>> additionalReferenceNumbersToAddToNote;

		#endregion

		#endregion
	}
}
