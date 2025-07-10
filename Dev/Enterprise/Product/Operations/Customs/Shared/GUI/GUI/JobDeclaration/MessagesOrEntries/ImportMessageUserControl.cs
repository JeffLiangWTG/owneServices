using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.GUI.WarehouseExtensions;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GUI
{
	public partial class ImportMessageUserControl : BaseCustomsEntryUserControl, IBondedWarehouseMenuItemsForGridCreatorSupporter
	{
		public ImportMessageUserControl()
			: this(null)
		{
		}

		public ImportMessageUserControl(BaseJobDeclaration declaration)
			: base(declaration)
		{
			InitializeComponent();
			EntriesBoundGrid.ReadOnly = true;

			if (!ShouldKeepMessageStatusDescriptionInEntriesGrid)
			{
				EntriesBoundGrid.RemoveFromAvailableColumns(CusEntryHeader.Schema.MessageStatusDescription);
			}
			creator = new BondedWarehouseMenuItemsForGridCreator(this);
			AddWarehouseTransactionStatusColumnsIfSupported();

			InitializeMessagesControl();
		}

		BondedWarehouseMenuItemsForGridCreator creator;

		public new BaseJobDeclaration CurrentDataItem { get { return (BaseJobDeclaration)base.CurrentDataItem; } }

		protected virtual bool SupportWarehouseTransactionStatusColumns => false;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (creator != null)
				{
					creator.Dispose();
					creator = null;
				}
			}
			base.Dispose(disposing);
		}

		protected virtual bool ShouldKeepMessageStatusDescriptionInEntriesGrid => false;

		protected virtual Type GetBaseMessagesTabUserControlType() => typeof(BaseMessagesTabUserControl);

		#region IBondedWarehouseMenuItemsForGridCreatorSupporter Members

		IDeclarationWarehouseIntegrationSupporter IBondedWarehouseMenuItemsForGridCreatorSupporter.GetSupporter(object data) => data as IDeclarationWarehouseIntegrationSupporter;

		protected virtual BondedWarehouseOperationDeterminer GetNewBondedWarehouseOperationDeterminer(IWarehouseIntegrationSupporter supporter) => new BondedWarehouseOperationDeterminer(supporter);

		BondedWarehouseOperationDeterminer IBondedWarehouseMenuItemsCreatorSupporter.GetNewBondedWarehouseOperationDeterminer(IWarehouseIntegrationSupporter supporter) => GetNewBondedWarehouseOperationDeterminer(supporter);

		bool IBondedWarehouseMenuItemsCreatorSupporter.TopLevelBusinessObjectHasChanges()
		{
			var declaration = CurrentDataItem;
			var topLevelBusinessObject = (BusinessObject)declaration?.Shipment ?? declaration;
			return topLevelBusinessObject?.HasChanges ?? false;
		}

		ZGrid IBondedWarehouseMenuItemsForGridCreatorSupporter.GetGrid() => EntriesBoundGrid;

		bool IBondedWarehouseMenuItemsForGridCreatorSupporter.IsEnabled
		{
			get
			{
				var declaration = CurrentDataItem;
				return declaration != null && declaration.SupportMultipleWarehouseEntry;
			}
		}

		ContinueWithSave IBondedWarehouseMenuItemsCreatorSupporter.FireSaveButton() => this.FireSaveButton();

		#endregion IBondedWarehouseMenuItemsForGridCreatorSupporter Members

		void InitializeMessagesControl()
		{
			BaseMessageUserControl.UserControlType = GetBaseMessagesTabUserControlType();
			BindingSource.SetBindingMember(BaseMessageUserControl, MessagesUserControlBindingPath);
		}

		protected virtual string MessagesUserControlBindingPath => "CustomsEntryHeaders.Messages";

		void AddWarehouseTransactionStatusColumnsIfSupported()
		{
			if (SupportWarehouseTransactionStatusColumns)
			{
				EntriesBoundGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_WarehouseTransactionStatus,
					IsReadOnly = true,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				});
				EntriesBoundGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(137)
				});
				EntriesBoundGrid.ColumnStyles.Add(new ZCheckBoxColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_HasManualWhsUpdate,
					IsReadOnly = true,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180)
				});
			}
		}
	}
}
