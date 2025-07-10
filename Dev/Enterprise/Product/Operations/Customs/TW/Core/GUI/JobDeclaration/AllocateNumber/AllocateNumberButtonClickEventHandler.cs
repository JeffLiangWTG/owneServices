using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public class AllocateEventHandlerArgs : EventArgs
	{
		public AllocateEventHandlerArgs(bool isWaitingForResponseOrHasBeenLodgedAtCustoms, IBusiness topBizObjForFormHasChanges, Func<ContinueWithSave> fireSaveButton)
		{
			IsWaitingForResponseOrHasBeenLodgedAtCustoms = isWaitingForResponseOrHasBeenLodgedAtCustoms;
			TopBizObjForFormHasChanges = topBizObjForFormHasChanges;
			FireSaveButton = fireSaveButton;
		}

		public bool IsWaitingForResponseOrHasBeenLodgedAtCustoms { get; }
		public IBusiness TopBizObjForFormHasChanges { get; }
		public Func<ContinueWithSave> FireSaveButton { get; }
	}

	public class AllocateNumberButtonClickEventHandler
	{
		public void Allocate(IAllocateNumberSupporter supporter, AllocateEventHandlerArgs args, ZString number)
		{
			var continueAllocateNumber = true;
			if (number.IsEmpty)
			{
				var reasonToStopProceeding = supporter.GetReasonToStopProceedingWhenEmptyNumber();
				if (!reasonToStopProceeding.IsEmpty)
				{
					Globals.Message.ShowInformation(reasonToStopProceeding, GetNumberAllocationCaption(supporter.NumberType));
					continueAllocateNumber = false;
				}
			}
			if (continueAllocateNumber)
			{
				supporter.DoAllocate(number);
				if (args.FireSaveButton() == ContinueWithSave.Yes)
				{
					supporter.OnAllocatedNumberSaved();
				}
			}
			supporter.UnlockNumberAllocationMutex();
		}

		public void Modify(IAllocateNumberSupporter supporter, AllocateEventHandlerArgs args)
		{
			if (CheckPrerequisiteConditions(supporter, args))
			{
				var allocateBizObj = supporter.GetNewAllocateNumber();
				using (var form = new AllocateNumberForm(allocateBizObj))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
					{
						Allocate(supporter, args, allocateBizObj.Number);
					}
				}
				supporter.UnlockNumberAllocationMutex();
			}
		}

		bool CheckPrerequisiteConditions(IAllocateNumberSupporter supporter, AllocateEventHandlerArgs args)
		{
			var result = false;
			var topBizObjForHasChanges = args.TopBizObjForFormHasChanges;
			var numberType = supporter.NumberType;
			var caption = GetNumberAllocationCaption(numberType);
			if (topBizObjForHasChanges != null)
			{
				topBizObjForHasChanges.RunPreSaveValidation();
				if (!topBizObjForHasChanges.HasChanges || Globals.Message.Show(GetFormNeedsToBeSavedText(numberType), caption, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					var continueWithSave = ContinueWithSave.Yes;

					if (topBizObjForHasChanges.HasChanges)
					{
						continueWithSave = args.FireSaveButton();
					}
					if (continueWithSave == ContinueWithSave.Yes)
					{
						var shouldContinue = true;
						if (args.IsWaitingForResponseOrHasBeenLodgedAtCustoms)
						{
							var confirmMessage = supporter.GetConfirmMessagesWhenIsWaitingForResponseOrHasBeenLodgedAtCustoms();
							if (!confirmMessage.IsEmpty)
							{
								if (Globals.Message.Show(confirmMessage, caption, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.No)
								{
									shouldContinue = false;
								}
							}
						}

						if (shouldContinue)
						{
							var reasonToStopProceeding = supporter.GetReasonToStopProceeding();
							if (!reasonToStopProceeding.IsEmpty)
							{
								Globals.Message.ShowInformation(reasonToStopProceeding, caption);
							}
							else
							{
								var number = supporter.GetExistingNumber();
								if (number.IsEmpty || Globals.Message.Show(GetNumberAlreadyAllocatedText(numberType, number), caption, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
								{
									if (supporter.LockNumberAllocationMutex)
									{
										result = true;
									}
									else
									{
										Globals.Message.ShowInformation(GetNumberAllocationMutexLockText(supporter.GetNumberAllocationMutexLockInfo(), numberType), caption);
									}
								}
							}
						}
					}
				}
			}
			return result;
		}

		string GetNumberAllocationCaption(string numberType) => Res.GetString("DBCBD1BA-98D6-450F-855A-3A321C115DB8", "Allocate {0}", numberType);

		string GetNumberAllocationMutexLockText(string lockInfo, string numberType) => Res.GetString("5F4E4BEF-A5DF-443F-8F6D-0FC033F19048", "{0} is in the process of allocating {1} for this job.\r\nPlease re-open the job later.", lockInfo, numberType);

		string GetNumberAlreadyAllocatedText(string numberType, string number) => Res.GetString("9374249F-AF20-4C98-ADE1-93544D7F6544", "There is {0} ({1}) already allocated for this job. Are you sure you wish to continue?", numberType, number);

		string GetFormNeedsToBeSavedText(string numberType) => Res.GetString("D0B6E24D-75AE-4136-943A-EBE01335F9B2", "This form must be saved before {0} is allocated.\n\nDo you want to save and proceed?", numberType);
	}
}
