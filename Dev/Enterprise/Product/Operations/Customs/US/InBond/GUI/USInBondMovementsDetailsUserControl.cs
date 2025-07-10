using System;
using System.Windows.Forms;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.GUI
{
	public partial class USInBondMovementsDetailsUserControl : ZUserControl
	{
		public USInBondMovementsDetailsUserControl()
		{
			InitializeComponent();
			AddPartAttribColumns();
			MovementsTopSplitContainer.Panel2MinSize = 205;
			MovementsBottomSplitContainer.Panel2MinSize = 150;
			ContainersAndHazardousMaterialsSplitContainer.Panel2MinSize = 150;
			MoveDetailsAndBillsDispositonSplitContainer.Panel2MinSize = 150;
		}

		public CusInBondHeader BusinessEntity
		{
			get { return (CusInBondHeader)base.CurrentDataItem; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			UnHookControlVisibilityChangeEvents();
			base.SetDataBinding(dataSource, dataMember);
			if (BusinessEntity != null)
			{
				BusinessEntity.BH_OA_ImporterInfo.ValueChanged += BH_OA_ImporterInfo_ValueChanged;
				BusinessEntity.BH_FTZMoveInfo.ValueChanged += BH_FTZMoveInfo_ValueChanged;
				BusinessEntity.BH_ImportTransportModeInfo.ValueChanged += BH_ImportTransportModeInfo_ValueChanged;
				BH_FTZMoveInfo_ValueChanged(this, EventArgs.Empty);
				BH_ImportTransportModeInfo_ValueChanged(this, EventArgs.Empty);
			}
			MoveDetailContainerCommoditiesGrid.SetAvailability(true, [CusInBondCargoDesc.Schema.BY_InvoiceQuantity, CusInBondCargoDesc.Schema.BY_WarehouseEntryNumber, CusInBondCargoDesc.Schema.BY_WarehouseEntryLineNo]);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnHookControlVisibilityChangeEvents();
			}
			base.Dispose(disposing);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetPartAttributeCaptions();
		}

		void AddPartAttribColumns()
		{
			var partAttrib1TextBoxColumnStyleInfo = new ZDropEditColumnStyleInfo();
			partAttrib1TextBoxColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			partAttrib1TextBoxColumnStyleInfo.ColumnName = CusInBondCargoDesc.Schema.BY_PartAttrib1;
			partAttrib1TextBoxColumnStyleInfo.IsVisible = false;
			partAttrib1TextBoxColumnStyleInfo.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;

			var partAttrib2TextBoxColumnStyleInfo = new ZDropEditColumnStyleInfo();
			partAttrib2TextBoxColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			partAttrib2TextBoxColumnStyleInfo.ColumnName = CusInBondCargoDesc.Schema.BY_PartAttrib2;
			partAttrib2TextBoxColumnStyleInfo.IsVisible = false;
			partAttrib2TextBoxColumnStyleInfo.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;

			var partAttrib3TextBoxColumnStyleInfo = new ZDropEditColumnStyleInfo();
			partAttrib3TextBoxColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			partAttrib3TextBoxColumnStyleInfo.ColumnName = CusInBondCargoDesc.Schema.BY_PartAttrib3;
			partAttrib3TextBoxColumnStyleInfo.IsVisible = false;
			partAttrib3TextBoxColumnStyleInfo.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;

			var partNumberColumnStyle = MoveDetailContainerCommoditiesGrid.GetColumnStyle(CusInBondCargoDesc.Schema.BY_PartNumberForBinding);
			var partNumberColumnStyleIndex = MoveDetailContainerCommoditiesGrid.ColumnStyles.IndexOf(partNumberColumnStyle);

			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusInBondCargoDesc)((System.Collections.IList)((CusInBondContainer)((System.Collections.IList)((CusInBondMoveDetail)((System.Collections.IList)((CusInBondHeader)null).FilteredMovementDetails).SyncRoot).Containers).SyncRoot).Commodities).SyncRoot).BY_PartAttrib1);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusInBondCargoDesc)((System.Collections.IList)((CusInBondContainer)((System.Collections.IList)((CusInBondMoveDetail)((System.Collections.IList)((CusInBondHeader)null).FilteredMovementDetails).SyncRoot).Containers).SyncRoot).Commodities).SyncRoot).BY_PartAttrib2);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusInBondCargoDesc)((System.Collections.IList)((CusInBondContainer)((System.Collections.IList)((CusInBondMoveDetail)((System.Collections.IList)((CusInBondHeader)null).FilteredMovementDetails).SyncRoot).Containers).SyncRoot).Commodities).SyncRoot).BY_PartAttrib3);
			MoveDetailContainerCommoditiesGrid.ColumnStyles.Insert(++partNumberColumnStyleIndex, partAttrib1TextBoxColumnStyleInfo);
			MoveDetailContainerCommoditiesGrid.ColumnStyles.Insert(++partNumberColumnStyleIndex, partAttrib2TextBoxColumnStyleInfo);
			MoveDetailContainerCommoditiesGrid.ColumnStyles.Insert(++partNumberColumnStyleIndex, partAttrib3TextBoxColumnStyleInfo);

			var serialNumberTextBoxColumnStyleInfo = new ZDropEditColumnStyleInfo();
			serialNumberTextBoxColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			serialNumberTextBoxColumnStyleInfo.ColumnName = CusInBondCargoDesc.Schema.BY_SerialNumber;
			serialNumberTextBoxColumnStyleInfo.IsVisible = false;
			serialNumberTextBoxColumnStyleInfo.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;

			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusInBondCargoDesc)((System.Collections.IList)((CusInBondContainer)((System.Collections.IList)((CusInBondMoveDetail)((System.Collections.IList)((CusInBondHeader)null).FilteredMovementDetails).SyncRoot).Containers).SyncRoot).Commodities).SyncRoot).BY_SerialNumber);
			MoveDetailContainerCommoditiesGrid.ColumnStyles.Insert(++partNumberColumnStyleIndex, serialNumberTextBoxColumnStyleInfo);
		}

		void UnHookControlVisibilityChangeEvents()
		{
			if (BusinessEntity != null)
			{
				BusinessEntity.BH_OA_ImporterInfo.ValueChanged -= BH_OA_ImporterInfo_ValueChanged;
				BusinessEntity.BH_FTZMoveInfo.ValueChanged -= BH_FTZMoveInfo_ValueChanged;
				BusinessEntity.BH_ImportTransportModeInfo.ValueChanged -= BH_ImportTransportModeInfo_ValueChanged;
			}
		}

		void BH_OA_ImporterInfo_ValueChanged(object sender, EventArgs e)
		{
			SetPartAttributeCaptions();
			ShowOrHideInventorySelectionButton();
		}

		void SetPartAttributeCaptions()
		{
			var importer = BusinessEntity != null ? BusinessEntity.ImporterOrg : null;
			if (importer != null)
			{
				MoveDetailContainerCommoditiesGrid.SetColumnCaption(CusInBondCargoDescSchema.Constants.BY_PartAttrib1, importer.PartAttributeManager.PartAttributeName1);
				MoveDetailContainerCommoditiesGrid.SetColumnCaption(CusInBondCargoDescSchema.Constants.BY_PartAttrib2, importer.PartAttributeManager.PartAttributeName2);
				MoveDetailContainerCommoditiesGrid.SetColumnCaption(CusInBondCargoDescSchema.Constants.BY_PartAttrib3, importer.PartAttributeManager.PartAttributeName3);
			}
			else
			{
				MoveDetailContainerCommoditiesGrid.SetColumnCaption(CusInBondCargoDescSchema.Constants.BY_PartAttrib1, "Part Attrib. 1");
				MoveDetailContainerCommoditiesGrid.SetColumnCaption(CusInBondCargoDescSchema.Constants.BY_PartAttrib2, "Part Attrib. 2");
				MoveDetailContainerCommoditiesGrid.SetColumnCaption(CusInBondCargoDescSchema.Constants.BY_PartAttrib3, "Part Attrib. 3");
			}

			MoveDetailContainerCommoditiesGrid.SetColumnCaption(CusInBondCargoDescSchema.Constants.BY_SerialNumber, "Serial Number");
		}

		void MoveDetailContainersGrid_AfterBind(object sender, EventArgs e)
		{
			MoveDetailContainersGrid.ListManager.PositionChanged += MoveDetailContainersGridListManager_PositionChanged;
			MoveDetailContainersGridListManager_PositionChanged(null, null);
		}

		void MoveDetailContainersGridListManager_PositionChanged(object sender, EventArgs e)
		{
			var listManager = MoveDetailContainersGrid.ListManager;
			if (listManager != null)
			{
				if (listManager.Count > 0)
				{
					CusInBondContainer current = (CusInBondContainer)listManager.GetCurrent();
					if (current != null)
					{
						bool isDiff = currentContainer != current;
						if (isDiff)
						{
							currentContainer = current;
						}
					}
				}
				else
				{
					currentContainer = null;
				}
			}
			ShowOrHideInventorySelectionButton();
		}
		CusInBondContainer currentContainer;

		void ViewEditButton_Click(object sender, EventArgs e)
		{
			CusInBondCargoDesc commodity = null;

			if (MoveDetailContainerCommoditiesGrid.ListManager != null && MoveDetailContainerCommoditiesGrid.CurrentRowIndex >= 0)
			{
				commodity = (CusInBondCargoDesc)MoveDetailContainerCommoditiesGrid.ListManager.GetCurrent();
			}

			if (commodity == null)
			{
				Globals.Message.ShowInformation("Please select (highlight) a Commodity line.", "Edit Commodity Line");
			}
			else
			{
				using (var form = new USChildInBondCommodityForm(commodity))
				{
					form.ShowDialog();
				}
			}
		}

		void MoveDetailContainerCommoditiesGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			ViewEditButton_Click(sender, e);
		}

		void BH_FTZMoveInfo_ValueChanged(object sender, EventArgs e)
		{
			ShowOrHideInventorySelectionButton();
		}

		void BH_ImportTransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			var billColumnCaption = BusinessEntity is CusInBondHeader header && header.IsSea && US.Business.ZZCustomsFunctionality.IsAMSHBREffective ? "Bill" : "Master Bill";
			MovementDetailsGrid.SetColumnCaption(CusInBondMoveDetail.Schema.B9_B0, billColumnCaption);
		}

		void ShowOrHideInventorySelectionButton()
		{
			var moveHeader = currentContainer == null ? null : currentContainer.MoveHeader;
			InventorySelectionButton.Visible = moveHeader != null && moveHeader.SupportsBondedWarehousing;
		}

		void InventorySelectionButton_Click(object sender, EventArgs e)
		{
			if (currentContainer != null && currentContainer.MoveHeader != null)
			{
				ZFormModaliser.ShowDialogAndDispose(new Customs.GUI.InventorySelectionForm(new ContainerInventorySelectionHeader(currentContainer)));
			}
			else
			{
				ShowOrHideInventorySelectionButton();
			}
		}

		void MovementDetailsGrid_AfterBind(object sender, EventArgs e)
		{
			MovementDetailsGrid.ListManager.PositionChanged += MoveDetailContainersGridListManager_PositionChanged;
		}
	}
}
