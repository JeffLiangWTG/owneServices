namespace Enterprise.Freight.Agency.GUI
{
	using System;
	using System.Collections;
	using System.Reflection;
	using System.Windows.Forms;
	using CargoWise.Types;
	using CargoWise.Windows.UI;
	using Enterprise.Core.Forms;
	using Enterprise.Freight.Agency.Business;
	using Enterprise.ZArchitecture;
	using Enterprise.ZArchitecture.GUI;

	internal abstract partial class SplitGrid : ZUserControl
	{
		public SplitGrid()
		{
			InitializeComponent();

			gridLabel1.CaptionResourceString = Res.GetData("SplitGrid|ae921e90-56c3-4669-be15-9e42dc5dcdc0", "{0} On Original Booking").Format(ItemsName);
			gridLabel2.CaptionResourceString = Res.GetData("SplitGrid|2f7067cc-8e8a-49b8-9888-f70f106f9931", "{0} On New Booking").Format(ItemsName);

			grid1.ReadOnly = true;
			grid2.ReadOnly = true;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (DataSource != null)
			{
				InitializeGrids();
			}
		}

		/// <summary>
		///		Gets the plural name of the items being split.
		/// </summary>
		protected abstract string ItemsName
		{
			get;
		}

		/// <summary>
		///		Gets the names of properties from splitting items to show on grid. Possible properties are listed in [BusinessObject].Schema class.
		///		For example, <see cref="AgencyShipment.Schema"/>.
		/// </summary>
		protected abstract string[] Columns
		{
			get;
		}

		protected SplitBookingsHeader Header
		{
			get;
			private set;
		}

		/// <summary>
		///		Gets the name of the collection on <see cref="AgencyShipment"/> that has to be split.
		/// </summary>
		protected abstract string CollectionName
		{
			get;
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			Header = CurrentDataItem as SplitBookingsHeader;

			BindingSource.SetBindingMember(grid1, string.Concat(SplitBookingsHeader.Schema.OriginalShipment, ".", CollectionName));
			BindingSource.SetBindingMember(grid2, string.Concat(SplitBookingsHeader.Schema.NewShipment, ".", CollectionName));
		}

		void InitializeGrids()
		{
			grid1.ColumnStyles.Clear();
			grid2.ColumnStyles.Clear();

			foreach (var column in Columns)
			{
				grid1.ColumnStyles.Add(GetColumnControlCore(column));
				grid2.ColumnStyles.Add(GetColumnControlCore(column));
			}
		}

		/// <summary>
		///		Generates the <see cref="ZGridColumnInfo"/> for grids where the splitting items are listed.
		/// </summary>
		/// <param name="columnName">
		///		The name of the property for which to generate the column info. Possible properties are listed in [BusinessObject].Schema class.
		///		For example, <see cref="AgencyShipment.Schema"/>.
		/// </param>
		/// <returns></returns>
		/// <remarks>
		///		Override it to provide more specific column info to display items' properties in the way you want.
		/// </remarks>
		protected virtual ZGridColumnInfo GetColumnControl(string columnName)
		{
			ZGridColumnInfo control = null;

			if (BindingSource.DataSourceType != null)
			{
				var property = BindingSource.DataSourceType.GetProperty(columnName, BindingFlags.Public | BindingFlags.Instance);

				if (property.PropertyType == typeof(ZDecimal))
				{
					control = new ZCalcEditColumnStyleInfo
					{
						Decimals = 0,
						BindToDecimalPlaces = null,
					};
				}
			}

			if (control == null)
			{
				control = new ZTextBoxColumnStyleInfo();
			}

			ControlDpiScalingHelper.SetWidth(control, 60, true);

			return control;
		}

		ZGridColumnInfo GetColumnControlCore(string columnName)
		{
			var control = GetColumnControl(columnName);
			control.ColumnName = columnName;

			return control;
		}

		public void PerformMoveLeft()
		{
			MoveSelected(grid2, grid1, SplitBookingsHeader.MoveDirection.ToOriginal);
		}
		public void PerformMoveRight()
		{
			MoveSelected(grid1, grid2, SplitBookingsHeader.MoveDirection.ToNew);
		}

		protected virtual void PerformMove(object item, SplitBookingsHeader.MoveDirection direction)
		{
			// should be abstract but the designer doesn't like abstract base classes.
			throw new NotImplementedException("PerformMove needs to be overriden.");
		}

		void MoveSelected(ZGrid fromGrid, ZGrid toGrid, SplitBookingsHeader.MoveDirection direction)
		{
			CurrencyManager fromManager = fromGrid.ListManager;
			CurrencyManager toManager = toGrid.ListManager;

			if (fromManager != null && toManager != null)
			{
				MoveSpecific(fromManager, fromManager.Position, direction);
			}
		}

		protected virtual void MoveAtPoint(ZGrid fromGrid, ZGrid toGrid, int x, int y, SplitBookingsHeader.MoveDirection direction)
		{
			CurrencyManager fromManager = fromGrid.ListManager;
			CurrencyManager toManager = toGrid.ListManager;

			if (fromManager != null && toManager != null)
			{
				DataGrid.HitTestInfo info = fromGrid.HitTest(x, y);

				MoveSpecific(fromManager, info.Row, direction);
			}
		}

		void MoveSpecific(CurrencyManager fromManager, int fromIndex, SplitBookingsHeader.MoveDirection direction)
		{
			if (fromIndex >= 0)
			{
				IList fromCollection = fromManager.List;
				object item = fromCollection[fromIndex];

				PerformMove(item, direction);
			}
		}

		void movePackRightButton_Click(object sender, EventArgs e)
		{
			MoveSelected(grid1, grid2, SplitBookingsHeader.MoveDirection.ToNew);
		}

		void movePackLeftButton_Click(object sender, EventArgs e)
		{
			MoveSelected(grid2, grid1, SplitBookingsHeader.MoveDirection.ToOriginal);
		}

		void grid1_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && e.Clicks == 2)
			{
				MoveAtPoint(grid1, grid2, e.X, e.Y, SplitBookingsHeader.MoveDirection.ToNew);
			}
		}

		void grid2_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left && e.Clicks == 2)
			{
				MoveAtPoint(grid2, grid1, e.X, e.Y, SplitBookingsHeader.MoveDirection.ToOriginal);
			}
		}
	}
}
