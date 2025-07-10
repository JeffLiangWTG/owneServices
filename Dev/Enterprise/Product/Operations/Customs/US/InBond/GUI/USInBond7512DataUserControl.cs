using System;
using System.Linq;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.InBond.GUI
{
	public partial class USInBond7512DataUserControl : ZUserControl
	{
		public USInBond7512DataUserControl()
		{
			InitializeComponent();
			CBP7512MoveDetailAndLineSplitContainer.Panel2MinSize = 100;
			CBP7512MoveHeaderGrid.ContextMenu.MenuItems.Add(Print7512DepartureItemName, new EventHandler(Print7512Departure));
		}

		public CusInBondHeader BusinessEntity
		{
			get { return (CusInBondHeader)base.CurrentDataItem; }
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (currentMoveHeader != null)
			{
				currentMoveHeader.BM_OA_WarehouseAddressInfo.ValueChanged -= ChangeLinesBottomPanelVisibility;
			}
			UnHookCusInBondHeaderEvents();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			var bizEntity = BusinessEntity;
			if (bizEntity != null)
			{
				UnHookCusInBondHeaderEvents();
				bizEntity.BH_OA_ImporterInfo.ValueChanged += ChangeLinesBottomPanelVisibility;
				bizEntity.BH_FTZMoveInfo.ValueChanged += ChangeLinesBottomPanelVisibility;
			}
		}

		void UnHookCusInBondHeaderEvents()
		{
			var bizEntity = BusinessEntity;
			if (bizEntity != null)
			{
				bizEntity.BH_OA_ImporterInfo.ValueChanged -= ChangeLinesBottomPanelVisibility;
				bizEntity.BH_FTZMoveInfo.ValueChanged -= ChangeLinesBottomPanelVisibility;
			}
		}

		internal const string Print7512DepartureItemName = "Print 7512 Departure";

		void Print7512Departure(object sender, EventArgs e)
		{
			if (BusinessEntity.HasChanges)
			{
				Globals.Message.ShowError("Data must saved before printing.", "Data Not Saved");
			}
			else
			{
				USInBond7512DataHelper.Print7512DepartureBulk(CBP7512MoveHeaderGrid.SelectedElements.OfType<CusInBondMoveHeader>());
			}
		}

		void CBP7512MoveHeaderGrid_AfterBind(object sender, EventArgs e)
		{
			CBP7512MoveHeaderGrid.ListManager.PositionChanged += CBP7512MoveHeaderGridListManager_PositionChanged;
			CBP7512MoveHeaderGridListManager_PositionChanged(null, null);
		}

		void CBP7512MoveHeaderGridListManager_PositionChanged(object sender, EventArgs e)
		{
			var listManager = CBP7512MoveHeaderGrid.ListManager;
			if (listManager != null)
			{
				if (listManager.Count > 0)
				{
					var current = (CusInBondMoveHeader)listManager.GetCurrent();
					if (currentMoveHeader != current)
					{
						if (current != null)
						{
							current.BM_OA_WarehouseAddressInfo.ValueChanged -= ChangeLinesBottomPanelVisibility;
						}
						currentMoveHeader = current;
						if (currentMoveHeader != null)
						{
							currentMoveHeader.BM_OA_WarehouseAddressInfo.ValueChanged += ChangeLinesBottomPanelVisibility;
						}
						ChangeLinesBottomPanelVisibility(this, EventArgs.Empty);
					}
				}
				else
				{
					currentMoveHeader = null;
					ChangeLinesBottomPanelVisibility(this, EventArgs.Empty);
				}
			}
		}
		CusInBondMoveHeader currentMoveHeader;

		void ChangeLinesBottomPanelVisibility(object sender, EventArgs e)
		{
			LinesBottomPanel.Visible = currentMoveHeader != null && currentMoveHeader.IsExBondAutomationEnabled;
			CBP7512MoveDetailAndWarehouseDetailSplitContainer.Panel2Collapsed = !(BusinessEntity?.BH_FTZMove ?? false) || currentMoveHeader == null || currentMoveHeader.IsFTZWarehouse;
		}

		void DefaultBondedWhsDataButton_Click(object sender, EventArgs e)
		{
			if (currentMoveHeader != null)
			{
				currentMoveHeader.DefaultBondedWhsDataFor7512Document();
			}
		}
	}
}
