using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	class AllocateEventHandlerArgs : EventArgs
	{
		public IBusiness TopBizObjForFormHasChanges;
		public Func<ContinueWithSave> FireSaveButton;
		public Action<AllocateNumberForm> PerformActionBeforeShowingForm;
	}

	class AllocateNumberButtonClickEventHandler
	{
		public void Allocate(IAllocateNumberSupporter supporter, AllocateEventHandlerArgs args)
		{
			if (CheckPrerequisiteConditions(supporter, args))
			{
				var allocateBizObj = supporter.GetNewAllocateNumber();

				var shouldTakeUserEnteredNumber = allocateBizObj != null;
				var allocateNewNumber = !shouldTakeUserEnteredNumber;
				var userEnteredNumber = ZString.Empty;

				if (shouldTakeUserEnteredNumber)
				{
					using (var form = new AllocateNumberForm(allocateBizObj))
					{
						if (args.PerformActionBeforeShowingForm != null)
						{
							args.PerformActionBeforeShowingForm(form);
						}

						if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
						{
							allocateNewNumber = true;
							userEnteredNumber = allocateBizObj.AE_Number;
						}
					}
				}

				if (allocateNewNumber)
				{
					supporter.DoAllocate(userEnteredNumber);

					if (args.FireSaveButton() == ContinueWithSave.Yes)
					{
						supporter.OnAllocatedNumberSaved();
					}
				}
				supporter.UnlockNumberAllocationMutex();
			}
		}

		bool CheckPrerequisiteConditions(IAllocateNumberSupporter supporter, AllocateEventHandlerArgs args)
		{
			bool result = false;

			var reasonToStopProceeding = supporter.GetReasonToStopProceeding();
			var numberType = supporter.NumberType;

			if (!reasonToStopProceeding.IsEmpty)
			{
				Globals.Message.ShowInformation(reasonToStopProceeding, "Allocate " + numberType);
			}
			else
			{
				var number = supporter.GetExistingNumber();

				if (number.IsEmpty || Globals.Message.Show(string.Format(NumberAlreadyAllocated, numberType, number), numberType + " exists", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					var topBizObjForHasChanges = args.TopBizObjForFormHasChanges;

					if (topBizObjForHasChanges != null)
					{
						if (!topBizObjForHasChanges.HasChanges || Globals.Message.Show(string.Format(FormNeedsToBeSaved, numberType), "Save", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
						{
							var continueWithSave = ContinueWithSave.Yes;

							if (topBizObjForHasChanges.HasChanges)
							{
								continueWithSave = args.FireSaveButton();
							}

							if (continueWithSave == ContinueWithSave.Yes)
							{
								if (supporter.LockNumberAllocationMutex)
								{
									result = true;
								}
								else
								{
									Globals.Message.ShowInformation(GetNumberAllocationMutexLockText(supporter.GetNumberAllocationMutexLockInfo(), numberType), "Allocate " + numberType);
								}
							}
						}
					}
				}
			}

			return result;
		}

		string GetNumberAllocationMutexLockText(string lockInfo, string numberType)
		{
			return string.Format("{0} is in the process of allocating {1} for this job.\r\nPlease re-open the job later.", lockInfo, numberType);
		}

		const string NumberAlreadyAllocated = "There is {0} ({1}) already allocated for this job. Are you sure you wish to continue?";
		const string FormNeedsToBeSaved = "This form must be saved before {0} is allocated.\n\nDo you want to save and proceed?";
	}
}
