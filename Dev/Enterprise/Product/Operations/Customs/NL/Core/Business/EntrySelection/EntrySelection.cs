using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NL.Business;

public class EntrySelection : AutoEntrySelection
{
	public EntrySelection(CusEntryHeader cusEntryHeader) : base(cusEntryHeader?.Factory ?? new BusinessObjectFactory())
	{
		this.cusEntryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
		Declaration = Argument.NotNull(cusEntryHeader.Declaration, nameof(cusEntryHeader.Declaration));
	}
	readonly CusEntryHeader cusEntryHeader;

	public override ZString EntryNumber => cusEntryHeader.EntryNumber;

	public void SetStatusToCusEntryHeaderForCRE()
	{
		cusEntryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.CRE;
		cusEntryHeader.CH_Status = ZString.Empty;
	}

	public void SetStatusToCusEntryHeaderForCancelCRE()
	{
		var referenceDateForPhaseStatus = cusEntryHeader.GetMostRecentStatusChangeEventTime(Events.PhaseStatusChange, CustomsEntryPhaseStatusList.Codes.CRE);
		var referenceDateForMessageStatus = cusEntryHeader.GetMostRecentStatusChangeEventTime(Events.MessageStatusChange, ZString.Empty);

		cusEntryHeader.CH_PhaseStatus = cusEntryHeader.RetrievePreviousValueFromLog(Events.PhaseStatusChange, referenceDateForPhaseStatus);
		cusEntryHeader.CH_Status = cusEntryHeader.RetrievePreviousValueFromLog(Events.MessageStatusChange, referenceDateForMessageStatus);
	}

	public void SetStatusToCusEntryHeaderForSUP()
	{
		cusEntryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes.SUP;
		cusEntryHeader.CH_Status = ZString.Empty;
		SetInstructionSubStyle();
		SetPreviousDocumentNMRN();
	}

	public void SetStatusToCusEntryHeaderForCancelSUP()
	{
		var referenceDateForPhaseStatus = cusEntryHeader.GetMostRecentStatusChangeEventTime(Events.PhaseStatusChange, CustomsEntryPhaseStatusList.Codes.SUP);
		var referenceDateForMessageStatus = cusEntryHeader.GetMostRecentStatusChangeEventTime(Events.MessageStatusChange, ZString.Empty);

		cusEntryHeader.CH_PhaseStatus = cusEntryHeader.RetrievePreviousValueFromLog(Events.PhaseStatusChange, referenceDateForPhaseStatus);
		cusEntryHeader.CH_Status = cusEntryHeader.RetrievePreviousValueFromLog(Events.MessageStatusChange, referenceDateForMessageStatus);
	}

	public void SetStatusToCusEntryHeaderForAMD()
	{
		cusEntryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._513;
		cusEntryHeader.CH_Status = ZString.Empty;
	}

	public void SetStatusToCusEntryHeaderForCancelAMD()
	{
		var referenceDateForPhaseStatus = cusEntryHeader.GetMostRecentStatusChangeEventTime(Events.PhaseStatusChange, CustomsEntryPhaseStatusList.Codes._513);
		var referenceDateForMessageStatus = cusEntryHeader.GetMostRecentStatusChangeEventTime(Events.MessageStatusChange, ZString.Empty);

		cusEntryHeader.CH_PhaseStatus = cusEntryHeader.RetrievePreviousValueFromLog(Events.PhaseStatusChange, referenceDateForPhaseStatus);
		cusEntryHeader.CH_Status = cusEntryHeader.RetrievePreviousValueFromLog(Events.MessageStatusChange, referenceDateForMessageStatus);
	}

	public new EntrySelectionValidation Validation => new EntrySelectionValidation(this);

	void SetInstructionSubStyle()
	{
		var instruction = cusEntryHeader.EntryInstruction;
		
		switch (instruction.CEI_SubStyle)
		{
			case EntrySubStyleList.Codes.IncompleteDeclaration:
			case EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB:
				instruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE;
				break;
			case EntrySubStyleList.Codes.SimplifiedDeclaration:
			case EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC:
				instruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;
				break;
		}
	}

	void SetPreviousDocumentNMRN()
	{
		var previousDocument = cusEntryHeader.EntryInstruction.PreviousDocuments.Where(x => x.CSI_Code == NLConstants.PreviousDocumentTypes.AcknowledgmentOfMRN) as PreviousDocument;

		if (previousDocument == null)
		{
			previousDocument = cusEntryHeader.EntryInstruction.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = NLConstants.PreviousDocumentTypes.AcknowledgmentOfMRN;
		}
		previousDocument.CSI_ReferenceNumber = cusEntryHeader.MovementReferenceNumber;
		cusEntryHeader.EntryInstruction.PreviousDocuments.RefreshBinding();
	}

	internal JobDeclaration Declaration { get; }
}
