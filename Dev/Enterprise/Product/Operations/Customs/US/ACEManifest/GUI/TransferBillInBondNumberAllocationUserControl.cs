using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.GUI
{
	public partial class TransferBillInBondNumberAllocationUserControl : ZUserControl
	{
		public TransferBillInBondNumberAllocationUserControl()
		{
			InitializeComponent();
		}

		AsycudaTransferBill TransferBill => CurrentDataItem as AsycudaTransferBill;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			var isReadOnly = TransferBill?.ReadOnly ?? false;
			InBondNumberAllocationButton.ReadOnly = isReadOnly;
			InBondNumberResetButton.ReadOnly = isReadOnly;
		}

		void InBondNumberAllocationButton_Click(object sender, EventArgs e)
		{
			var transferBill = TransferBill;
			if (transferBill != null)
			{
				if (transferBill.HasBeenDeleted)
				{
					Globals.Message.ShowInformation(Res.GetString("EC42ACD2-C1F7-4D53-9797-3DBF11AB5BBA", "This In-Bond Movement Header has been deleted by another user while you had the job open. Please close the job, re-open and try again.", "Allocate In-Bond Number"));
				}
				else
				{
					var message = transferBill.DisallowAllocateInBondNumber;
					if (!message.IsEmpty)
					{
						Globals.Message.ShowInformation(message, "Allocate In-Bond Number");
					}
					else
					{
						var mainForm = FindForm() as ZForm;
						if (mainForm != null)
						{
							var hasChanges = mainForm.BusinessEntityForHasChanges?.HasChanges ?? transferBill.HasChanges;
							if (!hasChanges || Globals.Message.Show(Res.GetString("6E8091F7-544D-4D03-AC71-17C9FF2D5954", "This Transfer must be saved before an In-Bond Number is allocated.\r\nDo you want to save and proceed?"), "Save Transfer", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
							{
								var continueWithSave = ContinueWithSave.Yes;

								if (hasChanges)
								{
									continueWithSave = mainForm.FireSaveButton();
								}

								if (continueWithSave == ContinueWithSave.Yes)
								{
									if (transferBill.LockInBondNumberAllocationMutex())
									{
										var allocator = new US.Business.AllocateInBondNumber(transferBill.HeaderBranch, false);
										using (var form = new US.GUI.AllocateInBondNumberForm(allocator))
										{
											if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
											{
												transferBill.AllocateInBondNumber(allocator.AI_InBondNumber);

												try
												{
													transferBill.Factory.Save();
												}
												catch (Exception exception) when (!exception.IsCriticalException())
												{
													ZExceptionReporting.HandleSaveException(exception);
												}
												finally
												{
													transferBill.UnLockInBondNumberAllocationMutex();
												}
											}
										}
									}
									else
									{
										Globals.Message.ShowInformation(AsycudaTransferBill.InBondNumberAllocationMutexLockText(transferBill.GetInBondNumberAllocationMutexLockInfo()), "Allocate In-Bond Number");
									}
								}
							}
						}
					}
				}
			}
		}

		void InBondNumberResetButton_Click(object sender, EventArgs e)
		{
			var transferBill = TransferBill;
			if (transferBill != null && !transferBill.InBondNumber.IsEmpty)
			{
				if (Env.Security.USInBondResetToOriginal.IsAllowed)
				{
					if (transferBill.IsInBondNumberResetable)
					{
						var mainForm = FindForm() as ZForm;
						if (mainForm != null)
						{
							var hasChanges = mainForm.BusinessEntityForHasChanges?.HasChanges ?? transferBill.HasChanges;
							if (!hasChanges || Globals.Message.Show(Res.GetString("6EF098B0-FF12-4CEC-9407-ED8310C32C55", "This Transfer must be saved before an In-Bond Number is reset.\r\nDo you want to save and proceed?"), "Save Transfer", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
							{
								var continueWithSave = ContinueWithSave.Yes;

								if (hasChanges)
								{
									continueWithSave = mainForm.FireSaveButton();
								}

								if (continueWithSave == ContinueWithSave.Yes)
								{
									var inBondResetReason = Globals.Message.QueryUserResponse(ResponseArgument);
									if (inBondResetReason.Length > 0)
									{
										transferBill.ResetInBondNumber(inBondResetReason);
										try
										{
											transferBill.Factory.Save();
										}
										catch (Exception exception) when (!exception.IsCriticalException())
										{
											ZExceptionReporting.HandleSaveException(exception);
										}
									}
								}
							}
						}
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("07C524A5-85E1-49DC-8843-5B9387AD78F6", "This Transfer is currently waiting or lodged at customs. The In-Bond Number cannot be reset at this time."));
					}
				}
				else
				{
					Env.Security.ShowError(Env.Security.USInBondResetToOriginal);
				}
			}
		}

		UserResponseArgument ResponseArgument
		{
			get
			{
				if (fResponseArgument == null)
				{
					fResponseArgument = new UserResponseArgument();
					fResponseArgument.MinimumResponseLength = 1;
					fResponseArgument.MaximumResponseLength = StmALogSchema.SL_Reference.MaxLength - AsycudaTransferBill.ResetInBondNumberMessage.Length - AsycudaTransferBill.Schema.InBondNumber.Length;
					fResponseArgument.Message = Res.GetString("D57A5997-3F2E-44C9-8DE4-755EEF9D9CA8", "Please enter the reason for resetting this In-Bond number.");
					fResponseArgument.Caption = "Warning";
					fResponseArgument.Buttons = ZMessageBoxButtons.OKCancel;
					fResponseArgument.Icon = ZMessageBoxIcon.Asterisk;
					fResponseArgument.DefaultButton = ZMessageBoxDefaultButton.Button2;
				}
				return fResponseArgument;
			}
		}
		UserResponseArgument fResponseArgument;
	}
}
