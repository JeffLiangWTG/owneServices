using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.AMS.GUI
{
	public partial class USAMSInBondUserControl : ZUserControl
	{
		public USAMSInBondUserControl()
		{
			InitializeComponent();
			InBondSplitContainer.Panel2MinSize = 250;

			StatusGroupBox.AllowOverlap(InBondMovementHeaderTabControl);
		}

		public CusInBondHeader BusinessEntity
		{
			get { return (CusInBondHeader)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				ChangeInBondNumberAllocationButtonVisibility();
			}
		}

		void ChangeInBondNumberAllocationButtonVisibility()
		{
			InBondMovementHeaderInBondNumberAllocationButton.ReadOnly = currentMoveHeader == null;
		}

		void InBondMovementHeadersGrid_AfterBind(object sender, EventArgs e)
		{
			InBondMovementHeadersGrid.ListManager.PositionChanged += new EventHandler(InBondMovementHeadersGridListManager_PositionChanged);
			InBondMovementHeadersGridListManager_PositionChanged(null, null);
		}

		void InBondMovementHeadersGridListManager_PositionChanged(object sender, EventArgs e)
		{
			var listManager = InBondMovementHeadersGrid.ListManager;
			if (listManager != null)
			{
				if (listManager.Count > 0)
				{
					var current = (CusInBondMoveHeader)listManager.GetCurrent();
					if (current != null)
					{
						var isDiff = (currentMoveHeader != current);
						if (isDiff)
						{
							currentMoveHeader = current;
							ChangeInBondNumberAllocationButtonVisibility();
						}
					}
				}
				else
				{
					currentMoveHeader = null;
					ChangeInBondNumberAllocationButtonVisibility();
				}
			}
		}
		CusInBondMoveHeader currentMoveHeader;

		void InBondMovementHeaderInBondNumberAllocationButton_Click(object sender, EventArgs e)
		{
			if (currentMoveHeader != null)
			{
				ZString message = currentMoveHeader.DisallowAllocateInBondNumber;

				if (!message.IsEmpty)
				{
					Globals.Message.ShowInformation(message, "Allocate In-Bond Number");
				}
				else
				{
					var businessEntity = BusinessEntity;
					if (!businessEntity.HasChanges || Globals.Message.Show("The In-Bond Job must be saved before an In-Bond Number is allocated.\r\nDo you want to save and proceed?", "Save In-Bond Job", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
					{
						var mainForm = FindForm() as ZForm;
						if (mainForm != null)
						{
							var continueWithSave = ContinueWithSave.Yes;

							if (businessEntity.HasChanges)
							{
								continueWithSave = mainForm.FireSaveButton();
							}

							if (continueWithSave == ContinueWithSave.Yes)
							{
								if (currentMoveHeader.LockInBondNumberAllocationMutex())
								{
									var allocateInBondNumber = new US.Business.AllocateInBondNumber(businessEntity.Branch) { AllowPaperlessNumber = true };
									using (var form = new US.GUI.AllocateInBondNumberForm(allocateInBondNumber))
									{
										if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
										{
											currentMoveHeader.AllocateInBondNumber(allocateInBondNumber.AI_InBondNumber);

											try
											{
												businessEntity.Factory.Save();
											}
											catch (Exception exception) when (!exception.IsCriticalException())
											{
												ZExceptionReporting.HandleSaveException(exception);
											}
										}
									}
								}
								else
								{
									Globals.Message.ShowInformation(CusInBondMoveHeader.InBondNumberAllocationMutexLockText(currentMoveHeader.GetInBondNumberAllocationMutexLockInfo()), "Allocate In-Bond Number");
								}
							}
						}
					}
				}
			}
		}
	}
}
