using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public partial class StocktakeLineFilterControl : ZFilterStripCommonControl
	{
		public StocktakeLineFilterControl(WhsStocktake stocktake)
			: base(new StocktakeLineFilterBusinessObject(stocktake))
		{
			InitializeComponent();
			HookOnce();
		}

		#region ZFilterStripCommonControl Overloads

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			PartAttributeColumnManager = new PartAttributeColumnManager(this.Grid,
												 WhsStocktakeLineSchema.WU_ExpiryDate.Name,
												 WhsStocktakeLineSchema.WU_PackingDate.Name,
												 WhsStocktakeLineSchema.WU_PartAttrib1.Name,
												 WhsStocktakeLineSchema.WU_PartAttrib2.Name,
												 WhsStocktakeLineSchema.WU_PartAttrib3.Name,
												 WhsStocktakeLineSchema.WU_SerialNumber.Name);

			Stocktake.WS_OH_ClientInfo.ValueChanged += new EventHandler(WS_OH_ClientInfo_ValueChanged);
			WS_OH_ClientInfo_ValueChanged(this, null);

			var currencyManager = (CurrencyManager)GetBindingManager(Grid.BindTo);
			currencyManager.CurrentChanged += (sender, eventArgs) => Stocktake.Lines.ValidateDuplicateLine();
		}

		void WS_OH_ClientInfo_ValueChanged(object sender, EventArgs e)
		{
			PartAttributeColumnManager.SetColumns(Stocktake.Client);
		}

		protected override ZFilterGrid GetNewFilteredGrid()
		{
			return new ZFilterStocktakeLinesGrid();
		}

		protected override void InitialiseGridCore()
		{
			base.InitialiseGridCore();
			Grid.ReadOnly = false;
		}

		#endregion

		#region Hook / Unhook

		void HookOnce()
		{
			PerformSearch += new EventHandler<PerformSearchEventArgs>(StocktakeLineControl_PerformSearch);
		}

		void Unhook()
		{
			PerformSearch -= new EventHandler<PerformSearchEventArgs>(StocktakeLineControl_PerformSearch);
		}

		#endregion

		#region StocktakeLineControl_PerformSearch

		void StocktakeLineControl_PerformSearch(object sender, EventArgs e)
		{
			try
			{
				Grid.SuspendLayout();

				using (Stocktake.SuspendSettingHasChanges())
				{
					Stocktake.StocktakeLinesForFilter.AdditionalFilter = FilterBusinessObject.Filter;
					ResultCountMessage.UpdateResultCountMessage(Stocktake.StocktakeLinesForFilter.Count);
				}
			}
			finally
			{
				Grid.ResumeLayout();
			}
		}

		ResultCountMessage ResultCountMessage
			=> resultCountMessage ?? (resultCountMessage = new ResultCountMessage(this, MaximumAllowableQueriesPerSqlStatement, int.MaxValue)); // To make number of record search infinite.
		ResultCountMessage resultCountMessage;

		#endregion

		#region ShouldPerformSearch

		protected override ZBool ShouldPerformSearch()
		{
			var result = true;

			if (Stocktake.WS_StocktakeStatus == StocktakeStatus.Codes.New)
			{
				Globals.Message.Show(Res.GetString("2915f403-7b24-47d1-ae81-ee978757c9e0", "Stocktake Lines should be loaded and saved before searching."), Res.GetString("294a3850-48d9-4e55-a3b1-c8fb5e6a8612", "Please Load and Save"), MessageBoxButtons.OK, MessageBoxIcon.Error, DialogResult.OK);
				result = false;
			}
			else if (Stocktake.HasChanges) // ZDBonly filters won't work correctly unless lines are saved
			{
				Globals.Message.Show(Res.GetString("00c10e84-26ae-4649-bd51-a6d27dead33c", "Some stocktake lines have been modified. Save before searching again."), Res.GetString("a7c335c4-8c68-46ba-858e-81f3812264ce", "Please Save"), MessageBoxButtons.OK, MessageBoxIcon.Error, DialogResult.OK);
				result = false;
			}

			return result;
		}

		#endregion

		#region PersistCurrentControlValue

		protected override void PersistCurrentControlValue()
		{
			if (this.GetFrontMostActiveControl() is ZTextBox) // Stop base from changing focus to the grid which causes an uncommitted line to be added.
			{
				base.PersistCurrentControlValue();
			}
		}

		#endregion

		#region Properties

		WhsStocktake Stocktake => (WhsStocktake)DataSource;

		#endregion

		protected PartAttributeColumnManager PartAttributeColumnManager;
	}
}
