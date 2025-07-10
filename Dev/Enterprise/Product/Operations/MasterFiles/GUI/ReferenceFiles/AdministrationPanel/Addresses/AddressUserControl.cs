using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AddressUserControl : ZUserControl
	{
		public AddressUserControl(AdministrationPanelManager manager)
		{
			Manager = manager;
			InitializeComponent();
			InitializeUserControl();
		}

		void InitializeUserControl()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				CreateAddressDetailControl();
				CreateFilterControl();
				CreateProcessedGrid();
				CreateTokenForBackgroundValidation();
			}
		}

		void CreateAddressDetailControl()
		{
			AddressDetailControl = new SingleAddressValidationControl(Manager);
			AddressDetailControl.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			AddressDetailControl.ParentControl = this;
			AddressDetailControl.RefreshGrid += RefreshGrid;
			BindingSource.SetBindingMember(AddressDetailControl, "");
			RightFlowPanel.Controls.Add(AddressDetailControl);
		}

		void CreateProcessedGrid()
		{
			ProcessedGrid = new ZDisplayGrid();
			ProcessedGrid.ReadOnly = true;
			ProcessedGrid.ShouldSetErrorsOnTabPage = false;
			ProcessedGrid.BorderStyle = BorderStyle.None;
			ProcessedGrid.IsWholeRowSelectedOnClick = true;
			ProcessedGrid.Dock = DockStyle.Fill;
			ProcessedGrid.ColumnStyles.AddRange(FilterControl.Grid.ColumnStyles);
			BindingSource.SetBindingMember(ProcessedGrid, "AdminPanelProcessedAddressCollection");
			GroupBox_ProcessedAddresses.Controls.Add(ProcessedGrid);
			GroupBox_ProcessedAddresses.Text = Res.GetString("43434035-4F05-4BF8-BFFA-E1E7A949130B", "Records modified in this session");
			LeftSplitContainer.Panel2.Controls.Add(GroupBox_ProcessedAddresses);

			ProcessedGrid.SelectedRowsChangedInMouseDown += delegate
			{
				if (Manager.AdminPanelProcessedAddressCollection.Count != 0)
				{
					Grid_SelectedRowsChangedInMouseDown(ProcessedGrid);
				}
			};

			Manager.AdminPanelProcessedAddressCollection.CountChanged += delegate
			{
				GroupBox_ProcessedAddresses.Text = Res.GetString("c2203f4b-52b1-4059-986b-e780ce7bb8b8", "Records modified in this session ({0})", Manager.AdminPanelProcessedAddressCollection.Count);
			};
		}

		void Grid_SelectedRowsChangedInMouseDown(ZGrid grid)
		{
			if (!afterSelectPreviousItem)
			{
				SetDataBindingForAddressDetailControl(grid?.Name == ProcessedGrid.Name);

				if (PreviousDataItemView != CurrentDataItemView)
				{
					CheckAndSaveChangedData();
				}
			}
			else
			{
				afterSelectPreviousItem = false;
			}
		}

		internal void RefreshGrid(object sender, ZGuid[] pks)
		{
			try
			{
				AddressDetailControl.AddressValidationSuspended = true;
				var originalGrid = ProcessedGrid.SelectedRowCount > 0 ? ProcessedGrid : FilterControl.Grid;
				for (int i = 0; i < pks.Length; i++)
				{
					var view = Manager.AdminPanelAddressCollection?.FindByPK(pks[i]) as MDMAdminPanelAddressView;
					if (view != null)
					{
						((IList)Manager.AdminPanelProcessedAddressCollection).Insert(0, view);
						ProcessedGrid?.UnSelectAll();
						Manager.AdminPanelAddressCollection.Remove(view);
					}

					if (i == pks.Length - 1)
					{
						AddressDetailControl.AddressValidationSuspended = false;
						if (CurrentDataItemView != null)
						{
							originalGrid.SelectSingleElement(CurrentDataItemView);
							if (originalGrid == ProcessedGrid)
							{
								var addressForValidation = AddressDetailControl.AddressForValidation;
								AddressDetailControl.SetUIStatus(reset: false, isVerifying: false);
								AddressDetailControl.RefreshSuggestionControl(addressForValidation.ValidationStatus == AddressValidationStatus.Verified
										? SingleAddressValidationControl.SuggestionControlStatus.Disabled
										: SingleAddressValidationControl.SuggestionControlStatus.HideListView);
							}
							else
							{
								ManuallyPerformDataItemChange();
							}
						}

						PreviousDataItemView = CurrentDataItemView;
					}
				}
			}
			finally
			{
				AddressDetailControl.AddressValidationSuspended = false;
			}
		}

		bool afterSelectPreviousItem; // prevent firing SelectedRowsChangedInMouseDown event twice
		void SelectPreviousItem()
		{
			var pk = PreviousDataItemView.PK;
			if (Manager.AdminPanelAddressCollection.FindByPK(pk) != null)
			{
				SetDataBindingForAddressDetailControl(false);
				FilterControl.Grid.SelectSingleElementByPK(pk);
			}
			else
			{
				SetDataBindingForAddressDetailControl(true);
				ProcessedGrid.SelectSingleElementByPK(pk);
			}

			afterSelectPreviousItem = true;
		}

		void ManuallyPerformDataItemChange()
		{
			AddressDetailControl.PerformDataItemChange();
		}

		internal void SetDataBindingForAddressDetailControl(bool bindProcessedGrid)
		{
			var bindingMember = AddressDetailControl.GetBindingMember();
			var needReBinding = string.IsNullOrEmpty(bindingMember) ||
								(bindProcessedGrid && bindingMember == "AdminPanelAddressCollection") ||
								(!bindProcessedGrid && bindingMember == "AdminPanelProcessedAddressCollection");
			if (needReBinding)
			{
				AddressDetailControl.BindingCompleted = false;
				if (bindProcessedGrid)
				{
					FilterControl.Grid.UnSelectAll();
					BindingSource.SetBindingMember(AddressDetailControl, "AdminPanelProcessedAddressCollection");
				}
				else
				{
					ProcessedGrid.UnSelectAll();
					BindingSource.SetBindingMember(AddressDetailControl, "AdminPanelAddressCollection");
					AddressDetailControl.SetDataBinding(Manager.AdminPanelAddressCollection, "");
				}

				AddressDetailControl.RecordsNavigator.BindToCurrencyManager(bindProcessedGrid ? ProcessedGrid.ListManager : FilterControl.Grid.ListManager);
				AddressDetailControl.BindingCompleted = true;
				ManuallyPerformDataItemChange();
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			MainSplitContainer.SplitterDistance = Width - AddressDetailControl.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(70);

			if (ParentAdminForm != null)
			{
				AddressDetailControl.CopyCompanyInformationMenuItem = ParentAdminForm.CopyCompanyInformationMenuItem;
				AddressDetailControl.SearchCompanyOnlineMenuItem = ParentAdminForm.SearchCompanyOnlineMenuItem;
				AddressDetailControl.CopyAddressInformationMenuItem = ParentAdminForm.CopyAddressInformationMenuItem;
				AddressDetailControl.SearchAddressOnlineMenuItem = ParentAdminForm.SearchAddressOnlineMenuItem;
			}
		}

		#region AddressFilterControl

		void CreateFilterControl()
		{
			FilterControl.Grid.IsWholeRowSelectedOnClick = true;
			FilterControl.Dock = DockStyle.Fill;
			FilterControl.ParentControl = this;
			FilterControl.PerformSearch += delegate
			{
				if (CheckAndSaveChangedData())
				{
					AddressDetailControl.RefreshCancellationToken();
					AddressDetailControl.AddressValidationSuspended = true;
					FilterControl.LoadData();
					StartBackgroudValidation();
					SetDataBindingForAddressDetailControl(false);
					PreviousDataItemView = CurrentDataItemView;
					if (PreviousDataItemView != null)
					{
						FilterControl.Grid.Select(0);
						SetNextAutoVerifyItem(CurrentDataItemView);
					}

					AddressDetailControl.AddressValidationSuspended = false;
					AddressDetailControl.PerformDataItemChange();
				}
			};

			FilterControl.Grid.SelectedRowsChangedInMouseDown += delegate
			{
				if (Manager.AdminPanelAddressCollection.Count != 0)
				{
					Grid_SelectedRowsChangedInMouseDown(FilterControl.Grid);
				}
			};

			LeftSplitContainer.Panel1.Controls.Add(FilterControl);
		}

		#endregion

		#region CheckAndSaveChangedData

		public bool CheckAndSaveChangedData(CancelEventArgs e = null)
		{
			bool result = true;
			if (PreviousDataItemView != null)
			{
				if (PreviousDataItemView.AddressEntity.IsInDatabase && PreviousDataItemView.HasAddressInfoChanges)
				{
					var question = Res.GetString("835FEF34-54BA-4CC4-B72E-5973B5E5368C",
						"This record has been modified.\r\nWould you like to save the changes?");
					var cancelOptions = Globals.Message.Show(question,
						Res.GetString("63796F60-0397-4DEB-AA1E-F1C858BE7730", "Warning"), MessageBoxButtons.YesNoCancel,
						MessageBoxIcon.Warning);

					switch (cancelOptions)
					{
						case DialogResult.Yes:
							if (!AddressDetailControl.SaveAddressInfo(PreviousDataItemView))
							{
								if (e == null)
								{
									SelectPreviousItem();
								}
								else
								{
									e.Cancel = true;
								}

								result = false;
							}
							else
							{
								RefreshGrid(null, new[] { PreviousDataItemView.PK });
							}

							break;

						case DialogResult.No:
							if (e == null)
							{
								AddressDetailControl.CancelChanges(PreviousDataItemView);
							}

							break;

						case DialogResult.Cancel:
							if (e != null)
							{
								e.Cancel = true;
							}

							break;
					}
				}
			}

			PreviousDataItemView = CurrentDataItemView;
			return result;
		}

		#endregion

		#region Validation Thread

		internal List<MDMAdminPanelAddressView> backgroundValidationList;

		internal void StartBackgroudValidation()
		{
			if (AllowAutoVerify)
			{
				if (AutoVerifying)
				{
					if (!TokenForBackgroundValidation.IsCancellationRequested)
					{
						CancelBackgroundValidation();
					}

					AutoVerifyingResetEvent.WaitOne();
				}

				if (TokenForBackgroundValidation.IsCancellationRequested)
				{
					CreateTokenForBackgroundValidation();
				}

				backgroundValidationList = new List<MDMAdminPanelAddressView>();
				foreach (var o in Manager.AdminPanelAddressCollection)
				{
					var view = (MDMAdminPanelAddressView)o;
					if (view.MDM_ValidationStatus == AddressValidationStatus.ToBeVerified)
					{
						if (view.AddressEntity is JobDocAddress || view.AddressEntity is OrgAddress orgaddress && orgaddress.AllowModify)
						{
							backgroundValidationList.Add(view);
						}
					}
				}

				if (backgroundValidationList.Count > 0)
				{
					BeginValidationTaskAsync(backgroundValidationList);
				}
			}
		}

		internal void CancelBackgroundValidation()
		{
			if (AutoVerifying)
			{
				TokenForBackgroundValidation.Cancel();
			}
		}

		async void BeginValidationTaskAsync(List<MDMAdminPanelAddressView> collection)
		{
			var token = TokenForBackgroundValidation;
			var validatedAddressesCount = 0;
			for (int index = 0; index < collection.Count; index++)
			{
				var addressView = collection[index];
				if (addressView != null)
				{
					var address = addressView.AddressEntity as ISupportWebAddressValidation;
					Exception exception = null;
					try
					{
						if (!token.IsCancellationRequested)
						{
							if (!AutoVerifying)
							{
								AutoVerifying = true;
								FilterControl.BackgroundValidationStatusIcon.Visible = true;
								SetStatusbarText(isErrorStatusBar: false, text: Res.GetString("1B690C03-6CFB-4B7C-AA9F-A74C2C5AB0EE", "The background validation has been started"));
							}

							validatedAddressesCount++;
							if (SetNextAutoVerifyItem(addressView))
							{
								await address.ValidateAddressAsync(token, WTG.AddressCleansing.Common.CleanseAction.QuickValidate);
							}
						}
						else
						{
							return;
						}
					}
					catch (Exception x)
					{
						exception = x;
					}
					finally
					{
						if (!token.IsCancellationRequested)
						{
							var isFinished = validatedAddressesCount >= collection.Count;
							AutoVerifying = !isFinished;
							var progressText = isFinished
								? Res.GetString("efecca5e-de85-4d5c-a0f8-77e1b80f92d5", "Auto verify finished")
								: Res.GetString("dcf5744a-33a1-420f-a201-bc2dad799420", "Auto verifying ({0}/{1})", validatedAddressesCount, collection.Count);
							var nextAddressView = index < collection.Count - 1 ? collection[index + 1] : null;
							ProcessAutoVerify(progressText, exception, isFinished, addressView, nextAddressView);
						}
					}
				}
			}

#if DEBUG
			AutoVerifyFinished?.Invoke(this, new EventArgs());
#endif
		}

		bool SetNextAutoVerifyItem(MDMAdminPanelAddressView addressView)
		{
			var needAutoVerify = false;
			if (addressView != null)
			{
				var address = addressView.AddressEntity as ISupportWebAddressValidation;
				needAutoVerify = AllowAutoVerify &&
								 address.ValidationStatus == AddressValidationStatus.ToBeVerified &&
								 addressView.AutoVerifyState != AddressAutoVerifyState.Skipped &&
								 !addressView.AddressEntity.HasChanges;

				if (AddressDetailControl?.CurrentAddressEntity != null && address.EntityPK == AddressDetailControl.CurrentAddressEntity.PK)
				{
					needAutoVerify &= !AddressDetailControl.IsVerifying;
				}

				addressView.AutoVerifyState = needAutoVerify ? AddressAutoVerifyState.Verifying : AddressAutoVerifyState.Skipped;
			}

			NextAutoVerifyItem = needAutoVerify ? addressView : null;
			return needAutoVerify;
		}

		internal MDMAdminPanelAddressView NextAutoVerifyItem { get; set; }

		void SetStatusbarText(bool isErrorStatusBar, string text)
		{
			var form = ParentForm as ZForm;
			if (form != null)
			{
				if (isErrorStatusBar)
				{
					form.ErrorStatusBarPanel.Text = text;
				}
				else
				{
					form.MessageStatusBarPanel.Text = text;
				}
			}
		}

		void ProcessAutoVerify(string progressText, Exception ex, bool isFinished, MDMAdminPanelAddressView currentAddressView, MDMAdminPanelAddressView nextAddressView)
		{
			if (ex != null)
			{
				SetStatusbarText(isErrorStatusBar: true, ex.Message);
			}

			if (TokenForBackgroundValidation != null && !TokenForBackgroundValidation.IsCancellationRequested)
			{
				SetStatusbarText(isErrorStatusBar: false, progressText);
				FilterControl.BackgroundValidationStatusIcon.Visible = !isFinished;
			}
			else
			{
				FilterControl.BackgroundValidationStatusIcon.Visible = false;
			}

			if (currentAddressView.AutoVerifyState == AddressAutoVerifyState.Verifying)
			{
				currentAddressView.AutoVerifyState = AddressAutoVerifyState.Verified;
				var address = currentAddressView.AddressEntity as ISupportWebAddressValidation;
				if (address.ValidationStatus != AddressValidationStatus.Verified)
				{
					address.ValidationStatus = AddressValidationStatus.Invalid;
				}

				SetNextAutoVerifyItem(nextAddressView);
				if (address.ValidationStatus == AddressValidationStatus.Verified)
				{
					AddressDetailControl.SaveSingleRecord(currentAddressView, isAutoVerify: true);
				}
				else
				{
					AddressDetailControl.SaveAddressInfo(currentAddressView, isAutoVerify: true);
					if (AddressDetailControl.CurrentAddressEntity != null && AddressDetailControl.CurrentAddressEntity.PK == currentAddressView.PK)
					{
						AddressDetailControl.SetUIStatus(reset: false, isVerifying: false);
					}
				}
			}
		}

		void CreateTokenForBackgroundValidation()
		{
			TokenForBackgroundValidation = new CancellationTokenSource();
			TokenForBackgroundValidation.Token.Register(() =>
			{
				AutoVerifying = false;
				NextAutoVerifyItem = null;
				AutoVerifyingResetEvent.Set();
				SetStatusbarText(false, Res.GetString("ef2cca5e-de85-4d5c-a0f8-77e1b80f92df", "The background validation has been suspended"));
				FilterControl.BackgroundValidationStatusIcon.Visible = false;
				backgroundValidationList?.Where(v => v.AutoVerifyState == AddressAutoVerifyState.Verifying).ForEach(
					view =>
					{
						view.AutoVerifyState = AddressAutoVerifyState.Waiting;
						if (view == AddressDetailControl.CurrentDataItemView)
						{
							AddressDetailControl.SetUIStatus(false, false);
						}
					}
				);
#if DEBUG
				AutoVerifyCanceled?.Invoke(this, new EventArgs());
#endif
			});
		}

#if DEBUG
		internal event EventHandler AutoVerifyFinished;
		internal event EventHandler AutoVerifyCanceled;
#endif

		internal bool AutoVerifying { get; set; }

		internal bool AllowAutoVerify => !ParentAdminForm?.SuspendBackgroundValidation ?? true;

		readonly AutoResetEvent AutoVerifyingResetEvent = new AutoResetEvent(false);

		CancellationTokenSource TokenForBackgroundValidation { get; set; }

		#endregion

		internal AdministrationPanelForm ParentAdminForm { get; set; }
		internal MDMAdminPanelAddressView PreviousDataItemView { get; set; }
		MDMAdminPanelAddressView CurrentDataItemView => AddressDetailControl.CurrentDataItemView;
		internal SingleAddressValidationControl AddressDetailControl { get; set; }
		internal AdministrationPanelManager Manager { get; }
		internal AddressesFilterControl FilterControl => filterControl ?? (filterControl = new AddressesFilterControl(Manager.AdminPanelAddressCollection, new AddressesFilterBusinessObject()));
		AddressesFilterControl filterControl;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		IContainer components;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			AddressDetailControl?.Dispose();

			if (TokenForBackgroundValidation != null)
			{
				TokenForBackgroundValidation.Cancel();
				TokenForBackgroundValidation.Dispose();
			}

			if (AutoVerifyingResetEvent != null)
			{
				AutoVerifyingResetEvent.Dispose();
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
