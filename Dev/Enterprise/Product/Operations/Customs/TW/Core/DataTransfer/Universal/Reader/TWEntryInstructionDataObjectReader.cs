using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.DataTransfer.Universal
{
	public class TWEntryInstructionDataObjectReader : CustomsEntryInstructionDataObjectReader
	{
		public TWEntryInstructionDataObjectReader(EntryInstruction entryInstructionDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, UniversalObjectFactory factory, BaseJobDeclaration declaration) : base(entryInstructionDataObject, logger, helper, factory, declaration)
		{
		}

		protected override void FillCountrySpecificDetails(CusEntryInstruction targetBO)
		{
			base.FillCountrySpecificDetails(targetBO);

			if (helper.IsSourceAndTargetCountrySame && dataObject.AddInfoCollection != null)
			{
				var entryInstructionPK = targetBO.PK;
				var tradersRemarksAddInfo = dataObject.AddInfoCollection.GetZStringValue(PredefinedNoteTypes.Instance.TWTradersRemarks.Description, logger);
				if (tradersRemarksAddInfo.HasValue)
				{
					var tradersRemarksNoteRow = GetColumnIndexer(helper.LoadOrCreateStmNoteForReaderUpdate(entryInstructionPK, CusEntryInstructionSchema.Constants.TableName, targetBO.IsInDatabase, PredefinedNoteTypes.Instance.TWTradersRemarks.Description, CargoWise.Definitions.StmNoteVisibility.PRV));
					SetValue(tradersRemarksNoteRow, StmNoteSchema.ST_NoteText, tradersRemarksAddInfo.Value);
				}
			}
		}
	}
}
