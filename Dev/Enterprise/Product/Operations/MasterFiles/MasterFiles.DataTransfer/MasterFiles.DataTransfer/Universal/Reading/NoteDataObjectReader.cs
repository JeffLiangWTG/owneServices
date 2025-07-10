using System;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class NoteDataObjectReader : DataObjectReader<Note, StmNote>
	{
		public NoteDataObjectReader(Note noteDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IUniversalXMLNoteParent parent, Func<Note, StmNote> noteBusinessObjectFinder = null, INoteTypeCollection masterBizoNoteTypes = null)
			: base(noteDataObject, logger, factory)
		{
			this.parent = Argument.NotNull(parent, "IUniversalXMLNoteParent parent");
			this.noteBusinessObjectFinder = noteBusinessObjectFinder;
			this.masterBizoNoteTypes = masterBizoNoteTypes;
		}

		readonly IUniversalXMLNoteParent parent;
		readonly Func<Note, StmNote> noteBusinessObjectFinder;
		readonly INoteTypeCollection masterBizoNoteTypes;

		protected override StmNote GetExistingBusinessObject()
		{
			StmNote result = null;

			if (noteBusinessObjectFinder != null)
			{
				result = noteBusinessObjectFinder(dataObject);
			}
			else
			{
				var finder = new NoteBusinessObjectFinder(dataObject);
				result = finder.Find(parent);
			}

			return result;
		}

		protected override void PopulateBusinessObject(StmNote noteBO)
		{
			noteBO.Master = parent;
			CheckAndSetIsCustomDescription(noteBO);
			SetValue(noteBO, StmNoteSchema.ST_Description, dataObject.Description);

			if (noteBO.ST_IsTextOnly)
			{
				if ((dataObject.NoteText?.Length ?? 0) > noteBO.NoteTextMaxLength)
				{
					logger.Log(LogType.Warning, FormattableString.Invariant($"Note Text ({dataObject.NoteText}) with length: {dataObject.NoteText?.Length} has been trimmed to not exceed the maximum length of {noteBO.NoteTextMaxLength}"));
					var cleanText = TrimToFit(dataObject.NoteText, noteBO.NoteTextMaxLength);
					SetValue(noteBO, StmNoteSchema.ST_NoteText, cleanText);
				}
				else
				{
					SetValue(noteBO, StmNoteSchema.ST_NoteText, dataObject.NoteText);
				}
			}
			else if (dataObject.NoteText.HasValue)
			{
				SetValue(noteBO, StmNoteSchema.ST_NoteData, ORtfTextUtil.TextToRtfBytes(dataObject.NoteText.Value));
			}

			if (dataObject.NoteContext != null)
			{
				dataObject.NoteContext.Code = dataObject.NoteContext.Code?.ToString().Length == 3 ? dataObject.NoteContext.Code : "???";
			}

			SetValue(noteBO, StmNoteSchema.ST_NoteContext, dataObject.NoteContext);
			SetValue(noteBO, StmNoteSchema.ST_NoteType, dataObject.Visibility);

			if (dataObject.VisibleCompany != null && dataObject.VisibleCompany.Code.HasValue)
			{
				var iDs = logger.TopLevelDataContext?.GetEnterpriseServerAndCompanyIDs();
				if (iDs != null)
				{
					var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
					if (registrationKey.EnterpriseCode == iDs.EnterpriseID && registrationKey.ServerCode == iDs.ServerID)
					{
						var company = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, dataObject.VisibleCompany.Code.Value);
						if (company != null)
						{
							SetValue(noteBO, StmNoteSchema.ST_GC_RelatedCompany, company.PK);
						}
					}
				}
			}
		}

		void CheckAndSetIsCustomDescription(StmNote noteBO)
		{
			var isCustom = IsCustomByDescription(dataObject.Description);
			var isCustomDescription = dataObject.IsCustomDescription;

			if (isCustomDescription.HasValue && isCustomDescription.Value != isCustom)
			{
				string messageNot = isCustom ? string.Empty : "NOT ";
				string messageEmpty = isCustomDescription.Value ? string.Empty : "/empty";
				logger.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Description(value: {0}) is {1}a custom note. IsCustomDescription(value: {2}{3}) was ignored.", dataObject.Description, messageNot, ((bool)isCustomDescription.Value).ToString(), messageEmpty));
			}

			SetValue(noteBO, StmNoteSchema.ST_IsCustomDescription, isCustom);
		}

		ZBool IsCustomByDescription(string description)
		{
			if (masterBizoNoteTypes != null)
			{
				return !masterBizoNoteTypes.IsPredefinedNoteTypeByDescription(description);
			}

			if (parent.CustomNoteTypesDelegate == null)
			{
				parent.CustomNoteTypesDelegate = new GetValueDelegate<NoteTypeCollection>(delegate
				{ return SystemDataRegistry.Instance.CustomNotes.Value.AllCustomNoteTypes; });
			}
			var noteTypes = (INoteTypeCollection)parent.NoteTypes;
			return !noteTypes.IsPredefinedNoteTypeByDescription(description);
		}

		ZString TrimToFit(string text, int maxLength, string trimMessage = "...trimmed to fit")
		{
			return (maxLength > 0 && (text?.Length ?? 0) > maxLength) ? (text.Substring(0, maxLength - trimMessage.Length) + trimMessage) : text;
		}
	}
}


