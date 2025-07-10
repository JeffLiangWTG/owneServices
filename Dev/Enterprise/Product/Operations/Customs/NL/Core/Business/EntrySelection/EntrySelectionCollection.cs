using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class EntrySelectionCollection : NonPersistentBusinessObjectCollection<EntrySelection>
{
	public EntrySelectionCollection(JobDeclaration declaration) : base(declaration.Factory)
	{
		this.declaration = declaration;
	}
	readonly JobDeclaration declaration;

	public void PopulateCollectionForSupplement()
	{
		RemoveAll();
		foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
		{
			if (entry.EntryInstruction.CEI_SubStyle.In(new ZString[] { EntrySubStyleList.Codes.IncompleteDeclaration, EntrySubStyleList.Codes.SimplifiedDeclaration, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC }) &&
				(ValidStatusCombinationsForSUPModeOn.Any(c => c.entryHeaderStatus == entry.CH_EntryStatus && c.messageStatus == entry.CH_Status && c.phaseStatus == entry.CH_PhaseStatus)
				|| ValidStatusCombinationsForSUPModeOnNoEntryStatus.Any(c => c.messageStatus == entry.CH_Status && c.phaseStatus == entry.CH_PhaseStatus)))
			{
				Add(new EntrySelection(entry));
			}
		}
	}

	public void PopulateCollectionForCancelSupplement()
	{
		RemoveAll();
		foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
		{
			if (entry.CH_PhaseStatus == CustomsEntryPhaseStatusList.Codes.SUP &&
				(entry.CH_Status.IsEmpty || entry.CH_Status == NLConstants.StatusNew.Error || entry.CH_Status == NLConstants.StatusNew.Invalid))
			{
				Add(new EntrySelection(entry));
			}
		}
	}

	public void PopulateCollectionForAmendment()
	{
		RemoveAll();
		foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
		{
			if (ValidStatusCombinationsForAMDModeOn.Any(c => c.entryHeaderStatus == entry.CH_EntryStatus && c.messageStatus == entry.CH_Status && c.phaseStatus == entry.CH_PhaseStatus)
				|| ValidStatusCombinationsForAMDModeOnNoEntryStatus.Any(c => c.messageStatus == entry.CH_Status && c.phaseStatus == entry.CH_PhaseStatus))
			{
				Add(new EntrySelection(entry));
			}
		}
	}

	public void PopulateCollectionForCancelAmendment()
	{
		RemoveAll();
		foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
		{
			if (entry.CH_PhaseStatus == CustomsEntryPhaseStatusList.Codes._513 &&
				(entry.CH_Status.IsEmpty || entry.CH_Status == NLConstants.StatusNew.Rejection || entry.CH_Status == NLConstants.StatusNew.Invalid))
			{
				Add(new EntrySelection(entry));
			}
		}
	}

	public void PopulateCollectionForCRE()
	{
		RemoveAll();
		foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
		{
			if (ValidStatusCombinationsForCREModeOn.Any(c => c.entryHeaderStatus == entry.CH_EntryStatus && c.messageStatus == entry.CH_Status && c.phaseStatus == entry.CH_PhaseStatus))
			{
				Add(new EntrySelection(entry));
			}
		}
	}

	public void PopulateCollectionForCancelCRE()
	{
		RemoveAll();
		foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
		{
			if (entry.CH_PhaseStatus == CustomsEntryPhaseStatusList.Codes.CRE &&
				(entry.CH_Status.IsEmpty || entry.CH_Status == NLConstants.StatusNew.Error || entry.CH_Status == NLConstants.StatusNew.Invalid))
			{
				Add(new EntrySelection(entry));
			}
		}
	}

	protected override BusinessObject CreateNonPersistentBusinessObject()
	{
		throw new InvalidOperationException("Users cannot add a new member");
	}

	protected override bool AllowNewCore => false;

	protected override bool AllowRemoveCore => false;

	List<(ZString messageStatus, ZString entryHeaderStatus, ZString phaseStatus)> ValidStatusCombinationsForSUPModeOn => validStatusCombinationsForSUPModeOn ??=
	[
		(NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.ProvisionalRelease, CustomsEntryPhaseStatusList.Codes._515),
		(NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.RequestForInformation, CustomsEntryPhaseStatusList.Codes._515),
		(NLConstants.StatusNew.ReminderReceived, NLConstants.EntryStatusNew.RequestForInformation, CustomsEntryPhaseStatusList.Codes._515),
		(NLConstants.StatusNew.ReminderReceived, NLConstants.EntryStatusNew.ProvisionalRelease, CustomsEntryPhaseStatusList.Codes._515),
		(NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.Amended, CustomsEntryPhaseStatusList.Codes._515),
		(NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.Received, CustomsEntryPhaseStatusList.Codes._583),

	];
	List<(ZString, ZString, ZString)> validStatusCombinationsForSUPModeOn;

	List<(ZString messageStatus, ZString phaseStatus)> ValidStatusCombinationsForSUPModeOnNoEntryStatus => validStatusCombinationsForSUPModeOnNoEntryStatus ??=
	[
		(NLConstants.StatusNew.Accepted, CustomsEntryPhaseStatusList.Codes._513),
		(NLConstants.StatusNew.Accepted, CustomsEntryPhaseStatusList.Codes._514),
	];
	List<(ZString, ZString)> validStatusCombinationsForSUPModeOnNoEntryStatus;

	List<(ZString messageStatus, ZString entryHeaderStatus, ZString phaseStatus)> ValidStatusCombinationsForAMDModeOn => validStatusCombinationsForAMDModeOn ??=
[
		(NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.PreLodged, CustomsEntryPhaseStatusList.Codes.REG),
		(NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.DocumentsControl, CustomsEntryPhaseStatusList.Codes.REG),
		(NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.PhysicalInspection, CustomsEntryPhaseStatusList.Codes.REG),
		(NLConstants.StatusNew.Error, NLConstants.EntryStatusNew.PreLodged, CustomsEntryPhaseStatusList.Codes._511),
		(NLConstants.StatusNew.Invalid, NLConstants.EntryStatusNew.PreLodged, CustomsEntryPhaseStatusList.Codes._511),
		(NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.MRNAllocated, CustomsEntryPhaseStatusList.Codes._515),
		(NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.RequestForInformation, CustomsEntryPhaseStatusList.Codes._515),
		(NLConstants.StatusNew.ReminderReceived, NLConstants.EntryStatusNew.RequestForInformation, CustomsEntryPhaseStatusList.Codes._515),
		(NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.Amended, CustomsEntryPhaseStatusList.Codes._515),
		(NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.Received, CustomsEntryPhaseStatusList.Codes.CRE),
		(NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.Received, CustomsEntryPhaseStatusList.Codes.SUP),

	];
	List<(ZString, ZString, ZString)> validStatusCombinationsForAMDModeOn;

	List<(ZString messageStatus, ZString phaseStatus)> ValidStatusCombinationsForAMDModeOnNoEntryStatus => validStatusCombinationsForAMDModeOnNoEntryStatus ??=
	[
		(NLConstants.StatusNew.Accepted, CustomsEntryPhaseStatusList.Codes._513),
		(NLConstants.StatusNew.Accepted, CustomsEntryPhaseStatusList.Codes._514),
	];
	List<(ZString, ZString)> validStatusCombinationsForAMDModeOnNoEntryStatus;

	List<(ZString messageStatus, ZString entryHeaderStatus, ZString phaseStatus)> ValidStatusCombinationsForCREModeOn => validStatusCombinationsForCREModeOn ??=
[
		(NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.RequestForInformation, CustomsEntryPhaseStatusList.Codes._515),
		(NLConstants.StatusNew.ReminderReceived, NLConstants.EntryStatusNew.RequestForInformation, CustomsEntryPhaseStatusList.Codes._515),
		(NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.Amended, CustomsEntryPhaseStatusList.Codes._515),
		(NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.DocumentsControl, CustomsEntryPhaseStatusList.Codes._515),
		(NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.PhysicalInspection, CustomsEntryPhaseStatusList.Codes._515),
		(NLConstants.StatusNew.ReminderReceived, NLConstants.EntryStatusNew.ProvisionalRelease, CustomsEntryPhaseStatusList.Codes._515),
		(NLConstants.StatusNew.Accepted, NLConstants.EntryStatusNew.Received, CustomsEntryPhaseStatusList.Codes._583),
		(NLConstants.StatusNew.Invalid, NLConstants.EntryStatusNew.DeclarationRejected, CustomsEntryPhaseStatusList.Codes._514),

	];
	List<(ZString, ZString, ZString)> validStatusCombinationsForCREModeOn;
}
