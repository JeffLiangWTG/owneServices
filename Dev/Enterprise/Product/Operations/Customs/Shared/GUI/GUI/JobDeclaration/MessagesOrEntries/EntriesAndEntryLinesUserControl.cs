using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.GUI.WarehouseExtensions;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GUI
{
	public partial class EntriesAndEntryLinesUserControl : BaseCustomsEntryUserControl, IBondedWarehouseMenuItemsForGridCreatorSupporter
	{
		public EntriesAndEntryLinesUserControl()
			: this(null)
		{
		}

		public EntriesAndEntryLinesUserControl(BaseJobDeclaration declaration)
			: base(declaration)
		{
			InitializeComponent();
			this.creator = new BondedWarehouseMenuItemsForGridCreator(this);
			AddWarehouseTransactionStatusColumnsIfSupported();
		}
		readonly BondedWarehouseMenuItemsForGridCreator creator;

		public new BaseJobDeclaration JobDeclaration { get { return base.JobDeclaration; } }

		protected override void HookControlVisibilityChangeEvents(BaseJobDeclaration declaration)
		{
			base.HookControlVisibilityChangeEvents(declaration);
			if (declaration != null)
			{
				declaration.JE_ApplicationCodeInfo.ValueChanged += JE_ApplicationCodeInfo_ValueChanged;
			}
			JE_ApplicationCodeInfo_ValueChanged(null, null);
		}

		protected override void UnHookControlVisibilityChangeEvents(BaseJobDeclaration declaration)
		{
			base.UnHookControlVisibilityChangeEvents(declaration);
			if (declaration != null)
			{
				declaration.JE_ApplicationCodeInfo.ValueChanged -= JE_ApplicationCodeInfo_ValueChanged;
			}
		}

		void JE_ApplicationCodeInfo_ValueChanged(object sender, System.EventArgs e)
		{
			var isInterfaced = (JobDeclaration?.JE_ApplicationCode ?? ZString.Empty) == DeclarationApplicationCodeList.Codes.Interfaced;
			using (EntriesBoundGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				EntriesBoundGrid.SetAvailability(!isInterfaced, [CusEntryHeader.Schema.VAT, CusEntryHeader.Schema.Duty]);
				EntriesBoundGrid.SetAvailability(isInterfaced, [CusEntryHeader.Schema.GSTAmount, CusEntryHeader.Schema.TotalDutyAmount]);
			}
		}

		void AddWarehouseTransactionStatusColumnsIfSupported()
		{
			if (SupportWarehouseTransactionStatusColumns)
			{
				var warehouseTransactionStatusTextBoxColumnStyleInfo = new ZTextBoxColumnStyleInfo();
				warehouseTransactionStatusTextBoxColumnStyleInfo.ColumnName = CusEntryHeader.Schema.CH_WarehouseTransactionStatus;
				warehouseTransactionStatusTextBoxColumnStyleInfo.IsReadOnly = true;
				warehouseTransactionStatusTextBoxColumnStyleInfo.IsVisible = false;
				warehouseTransactionStatusTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
				var textBoxColumnStyleInfoWarehouseTransactionStatusDescription = new ZTextBoxColumnStyleInfo();
				textBoxColumnStyleInfoWarehouseTransactionStatusDescription.ColumnName = CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription;
				textBoxColumnStyleInfoWarehouseTransactionStatusDescription.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(137);
				var hasManualWhsUpdateCheckBoxColumnStyleInfo = new ZCheckBoxColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_HasManualWhsUpdate,
					IsReadOnly = true,
					IsVisible = false,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180)
				};
				EntriesBoundGrid.ColumnStyles.Add(warehouseTransactionStatusTextBoxColumnStyleInfo);
				EntriesBoundGrid.ColumnStyles.Add(textBoxColumnStyleInfoWarehouseTransactionStatusDescription);
				EntriesBoundGrid.ColumnStyles.Add(hasManualWhsUpdateCheckBoxColumnStyleInfo);
			}
		}

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
				}
			}
			base.Dispose(disposing);
		}

		IDeclarationWarehouseIntegrationSupporter IBondedWarehouseMenuItemsForGridCreatorSupporter.GetSupporter(object data)
		{
			return data as IDeclarationWarehouseIntegrationSupporter;
		}

		protected virtual BondedWarehouseOperationDeterminer GetNewBondedWarehouseOperationDeterminer(IWarehouseIntegrationSupporter supporter)
		{
			return new BondedWarehouseOperationDeterminer(supporter);
		}

		BondedWarehouseOperationDeterminer IBondedWarehouseMenuItemsCreatorSupporter.GetNewBondedWarehouseOperationDeterminer(IWarehouseIntegrationSupporter supporter)
		{
			return GetNewBondedWarehouseOperationDeterminer(supporter);
		}

		ContinueWithSave IBondedWarehouseMenuItemsCreatorSupporter.FireSaveButton()
		{
			return this.FireSaveButton();
		}

		bool IBondedWarehouseMenuItemsCreatorSupporter.TopLevelBusinessObjectHasChanges()
		{
			var declaration = JobDeclaration;
			var topLevelBusinessObject = (BusinessObject)declaration?.Shipment ?? declaration;
			return topLevelBusinessObject?.HasChanges ?? false;
		}

		ZGrid IBondedWarehouseMenuItemsForGridCreatorSupporter.GetGrid()
		{
			return EntriesBoundGrid;
		}

		bool IBondedWarehouseMenuItemsForGridCreatorSupporter.IsEnabled
		{
			get
			{
				var declaration = JobDeclaration;
				return declaration != null && declaration.SupportMultipleWarehouseEntry;
			}
		}
	}
}
