using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using EZC = Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class NoteDataObjectWriter : DataObjectWriter<StmNote, Note>
	{
		public NoteDataObjectWriter(IDataWritingManager manager) : base(manager) { }

		protected override Note PopulateDataObject(StmNote noteBO)
		{
			var noteData = new Note();

			noteData.Description = noteBO.ST_DescriptionInDatabase;
			noteData.IsCustomDescription = noteBO.ST_IsCustomDescription;
			noteData.NoteText = noteBO.ST_NoteDataAsText;
			noteData.NoteContext = new NoteContext()
			{
				Code = noteBO.ST_NoteContext,
				Description = string.Format((EZC.NoResString)"Module: {0}, Direction: {1}, Freight: {2}", noteBO.ST_NoteContextModuleCaption, noteBO.ST_NoteContextDirectionCaption, noteBO.ST_NoteContextFreightModeCaption),
			};
			noteData.Visibility = new CodeDescriptionPair()
			{
				Code = noteBO.ST_NoteType,
				Description = noteBO.ST_NoteType_DescriptiveText,
			};

			if (noteBO.ST_GC_RelatedCompany != Guid.Empty)
			{
				var company = new BusinessObjectFactory().Load<GlbCompany>(noteBO.ST_GC_RelatedCompany);
				if (company != null)
				{
					noteData.VisibleCompany = new CodeDescriptionPair()
					{
						Code = company.GC_Code,
						Description = company.GC_Name,
					};
				}
			}

			return noteData;
		}
	}
}

