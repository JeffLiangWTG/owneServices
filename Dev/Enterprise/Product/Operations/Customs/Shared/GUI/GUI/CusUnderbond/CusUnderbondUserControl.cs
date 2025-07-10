using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GUI
{
	public partial class CusUnderbondUserControl : ZUserControl, ICusUnderbondMessageSender
	{
		public CusUnderbondUserControl()
		{
			InitializeComponent();
			AddCusUnderbondDetailsUserControl();
			DetailsUserControl.OutturnUserControl.RemoveStatusColumn();

			CreateNewUnderbondButton.AllowOverlap(UnderbondDetailsPanel);
		}

		void AddCusUnderbondDetailsUserControl()
		{
			this.DetailsUserControl = GetCusUnderbondDetailsUserControl();
			this.DetailsUserControl.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Bottom);
			this.DetailsUserControl.AllowDrop = true;
			this.DetailsUserControl.CurrentUnderbond = null;
			this.DetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 4, true);
			this.DetailsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 184, true);
			this.DetailsUserControl.Name = "DetailsUserControl";
			this.DetailsUserControl.OutturnDisabled = false;
			this.DetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 186, true);
			this.DetailsUserControl.TabIndex = 0;
			this.UnderbondDetailsPanel.Controls.Add(this.DetailsUserControl);
		}

		public bool OutturnDisabled
		{
			get { return DetailsUserControl.OutturnDisabled; }
			set { DetailsUserControl.OutturnDisabled = value; }
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		protected virtual CusUnderbondDetailsUserControl GetCusUnderbondDetailsUserControl()
		{
			return new CusUnderbondDetailsUserControl();
		}

		#region Binding

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			//HACK: Set the BindingContext manually as it can be reset when the tab page is removed.
			//TODO: Handle this at an architectural level - W00037278
			DetailsUserControl.OutturnTabPage.BindingContext = BindingContext;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (ParentCollection != null)
			{
				ParentCollection.CountChanged -= new CollectionCountChangedEventHandler(ParentCollection_CountChanged);
			}
			base.SetDataBinding(dataSource, dataMember);
			SetDataSourceFromCollection();
			if (ParentCollection != null)
			{
				ParentCollection.CountChanged += new CollectionCountChangedEventHandler(ParentCollection_CountChanged);
			}
			if (CurrentUnderbond != null)
			{
				DetailsUserControl.ChangeVisibility(CurrentUnderbond);
			}
			AllUnderbondsListManager = (CurrencyManager)GetBindingManager("AllUnderbonds");
		}

		CurrencyManager AllUnderbondsListManager
		{
			set
			{
				if (allUnderbondsListManager != null)
				{
					DetailsUserControl.DetailsTabPage.Unhook();
					DetailsUserControl.MessagesTabPage.Unhook();
					DetailsUserControl.OutturnTabPage.Unhook();
				}
				allUnderbondsListManager = value;
				if (allUnderbondsListManager != null)
				{
					DetailsUserControl.DetailsTabPage.Hook(allUnderbondsListManager);
					DetailsUserControl.MessagesTabPage.Hook(allUnderbondsListManager);
					DetailsUserControl.OutturnTabPage.Hook(allUnderbondsListManager);
				}
			}
		}
		CurrencyManager allUnderbondsListManager;

		CusUnderbondUnionCollectionParentCollection ParentCollection
		{
			get { return BindingSource.Current as CusUnderbondUnionCollectionParentCollection; }
		}

		void SetDataSourceFromCollection()
		{
			DataSource = (ParentCollection != null && ParentCollection.Count == 1) ? ParentCollection[0] : BindingSource.Current as ICusUnderbondUnionCollectionParent;
		}

		new ICusUnderbondUnionCollectionParent DataSource
		{
			get { return fDataSource; }
			set
			{
				if (fDataSource != value)
				{
					if (fDataSource != null)
					{
						this.DataSource.AllUnderbonds.ModeOfMovementChanged -= new EventHandler(AllUnderbonds_ModeOfMovementChanged);
						this.DataSource.AllUnderbonds.CanSendOutturnChanged -= new EventHandler(AllUnderbonds_CanSendOutturnChanged);
						this.DataSource.AllUnderbonds.MovementReasonChanged -= new EventHandler(AllUnderbonds_MovementReasonChanged);
					}
					fDataSource = value;
					DetailsUserControl.DataSource = value;
					if (fDataSource != null)
					{
						this.DataSource.AllUnderbonds.ModeOfMovementChanged += new EventHandler(AllUnderbonds_ModeOfMovementChanged);
						this.DataSource.AllUnderbonds.CanSendOutturnChanged += new EventHandler(AllUnderbonds_CanSendOutturnChanged);
						this.DataSource.AllUnderbonds.MovementReasonChanged += new EventHandler(AllUnderbonds_MovementReasonChanged);
					}
				}
			}
		}
		ICusUnderbondUnionCollectionParent fDataSource;

		#region Event Handlers

		void AllUnderbonds_ModeOfMovementChanged(object sender, EventArgs e)
		{
			DetailsUserControl.ChangeVisibility(CurrentUnderbond);
		}

		void AllUnderbonds_CanSendOutturnChanged(object sender, EventArgs e)
		{
			DetailsUserControl.ChangeVisibility(CurrentUnderbond);
		}

		void AllUnderbonds_MovementReasonChanged(object sender, EventArgs e)
		{
			DetailsUserControl.ChangeVisibility(CurrentUnderbond);
		}

		#endregion

		public virtual void SetBindPrepend(ZString bindPrepend)
		{
			UnderbondsGrid.BindTo = bindPrepend + UnderbondsGrid.BindTo;
			DetailsUserControl.SetBindPrepend(bindPrepend + "AllUnderbonds.");
		}

		#endregion

		#region New Underbond

		void CreateNewUnderbondButton_Click(object sender, EventArgs e)
		{
			CusUnderbondUnionCollectionParentCollection collection = BindingSource.Current as CusUnderbondUnionCollectionParentCollection;
			CusUnderbond newUnderbond = null;
			if (collection != null && collection.Count == 1)
			{
				newUnderbond = collection[0].AllUnderbonds.AddNewUnderbond(this);
			}
			else if (DataSource != null)
			{
				newUnderbond = DataSource.AllUnderbonds.AddNewUnderbond(this);
			}
			DetailsUserControl.CurrentUnderbond = newUnderbond;
			DetailsUserControl.DestinationIDTextBox.Focus();
		}

		#endregion

		#region Visibility

		#region Remove Columns

		public void RemoveColumnsForSea()
		{
			UnderbondsGrid.SetAvailability(false,
				[
					CusUnderbondSchema.C4_FlightNo.Name,
					CusUnderbondSchema.C4_ArrivalDate.Name,
					CusUnderbondSchema.C4_PiecesManifested.Name,
					CusUnderbondSchema.C4_PackageType.Name
				]);
			//DetailsUserControl.AirGhostGroupBox.Visible = false;
			DetailsUserControl.ChangePartShipmentVisibility(false);
#if DEBUG
			visibilitySelected = true;
#endif
		}

		public void RemoveColumnsForAir()
		{
			UnderbondsGrid.SetAvailability(false,
				[
					CusUnderbondSchema.C4_DateOfArrivalIntoDestinationPremise.Name,
					CusUnderbondSchema.C4_UnderbondBySeaLloydsIMONum.Name,
					CusUnderbondSchema.C4_UnderbondBySeaVoyage.Name,
					CusUnderbondSchema.C4_UnderbondBySeaVessel.Name
				]);
			DetailsUserControl.ChangePartShipmentVisibility(true);
			//DetailsUserControl.AirGhostGroupBox.Visible = true;
#if DEBUG
			visibilitySelected = true;
#endif
		}

		#endregion

		void UnderbondsGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			if (CurrentUnderbond != null)
			{
				DetailsUserControl.RowIndex = UnderbondsGrid.CurrentRowIndex;
				DetailsUserControl.ChangeVisibility(CurrentUnderbond);
			}
		}

#if DEBUG

		internal void DisableVisibilityCheck()
		{
			visibilitySelected = true;
		}

		internal bool visibilitySelected;

#endif

		#endregion

		#region ICusUnderbondMessageSender Members

		public ICusUnderbondDependentCollectionParent GetProviderToAddUnderbondTo(ICusUnderbondDependentCollectionParent[] allPossibleParents)
		{
			ICusUnderbondDependentCollectionParent result;
			if (allPossibleParents.Length == 1)
			{
				result = allPossibleParents[0];
			}
			else if (allPossibleParents.Length == 0)
			{
				result = null;
				Globals.Message.ShowError(Res.GetString("84e7e08e-67c6-4a02-a578-7da10f3d7a9b", "An underbond movement cannot be created, as there are no valid items for which one can be created."));
			}
			else
			{
				NewCusUnderbondNonPersistent bizo = new NewCusUnderbondNonPersistent(allPossibleParents);
				using (NewCusUnderbondDialog dialog = new NewCusUnderbondDialog(bizo))
				{
					result = NewCusUnderbondDialogResultWithoutDispose(dialog) ? bizo.SelectedParent : null;
				}
			}
			return result;
		}

		// This lets us override it in the test so we don't block.
		protected virtual bool NewCusUnderbondDialogResultWithoutDispose(NewCusUnderbondDialog dialog)
		{
			ZFormModaliser.ShowDialogWithoutDispose(dialog);
			return dialog.OKPressed;
		}

		#endregion

		#region Current Underbond

		internal protected CusUnderbond CurrentUnderbond
		{
			get
			{
				CusUnderbond result = null;
				int rowIndex = UnderbondsGrid.CurrentRowIndex;
				if (DataSource != null)
				{
					if (rowIndex >= 0 && rowIndex < DataSource.AllUnderbonds.Count)
					{
						result = DataSource.AllUnderbonds[rowIndex];
					}
					else if (DataSource.AllUnderbonds.Count > 0)
					{
						result = DataSource.AllUnderbonds[0];
					}
				}
				return result;
			}
		}

		void ParentCollection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			SetDataSourceFromCollection();
		}

		#endregion

		#region Nil Outturns

		internal void SetUnderbondParent(ICusUnderbondNilUnderbondPerformer parent)
		{
#if DEBUG
			UnderbondParentForTest = parent;
#endif
			DetailsUserControl.NilOutturnButton.Visible = parent != null;
			DetailsUserControl.SetUnderbondParent(parent);
		}

#if DEBUG
		internal ICusUnderbondNilUnderbondPerformer UnderbondParentForTest;
#endif

		#endregion

		void CusUnderbondUserControl_Load(object sender, EventArgs e)
		{
#if DEBUG
			if (!visibilitySelected && !DesignModeFinder.IsDesigning)
			{
				throw new ApplicationException("*** Please call either 'RemoveColumnsForSea()' or 'RemoveColumnsForAir()' on the 'CusUnderbondUserControl'. ***");
			}
#endif
		}

		protected internal ZButton CreateNewUnderbondButtonInternal => CreateNewUnderbondButton;

		protected internal void InitializeComponentInternal() => InitializeComponent();
	}
}
