using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class ZFilterStocktakeLinesGrid : ZFilterGrid
	{
		public const string DescriptionVirtual = "_DescriptionVirtual";

		public ZFilterStocktakeLinesGrid()
		{
			SetupContextMenuItems();
		}

		void SetupContextMenuItems()
		{
			var assignAllSelectedLinesToUser = new ZMenuItem(ResString.GetMultilingualString("7eecb8f3-867f-4bf6-992c-12bd39f49a07", "Assign Selected Lines to User"), AssignSelectedLinesContextMenu_Click);
			ContextMenu.MenuItems.Add(assignAllSelectedLinesToUser);
			ContextMenu.Popup += delegate
			{
				var selectedBizO = this.SelectedElements;
				assignAllSelectedLinesToUser.Enabled = (selectedBizO.Length != 0 && selectedBizO.Cast<ILineStaffAssigner>().Any(l => l.CanAssignOrUnAssignLine()));
			};
		}

		void AssignSelectedLinesContextMenu_Click(object sender, EventArgs e)
		{
			var selectedBizO = SelectedElements;
			var selectedLines = selectedBizO.Cast<ILineStaffAssigner>();
			var attacher = new AssignLinesToUserAttacher<ILineStaffAssigner>(selectedLines, selectedBizO[0].Factory);
			attacher.Show((IZForm)this.FindForm());

#if DEBUG
			LastShownStocktakeLineAttachPopupForTesting = attacher.LastShownAttachPopupForTesting;
			StocktakeLineAttacherForTesting = attacher;
#endif
		}

#if DEBUG
		public EmbeddedModulePopup LastShownStocktakeLineAttachPopupForTesting;
		public AssignLinesToUserAttacher<ILineStaffAssigner> StocktakeLineAttacherForTesting;
#endif

		#region CountColumns

		ReadOnlyCollection<string[]> CountColumns => countColumns ?? (countColumns = GetCountColumns());
		ReadOnlyCollection<string[]> countColumns;

		ReadOnlyCollection<string[]> GetCountColumns()
		{
			return new List<string[]>()
			{
				new string[] { WhsStocktakeLineSchema.Constants.WU_LastCount,
								WhsStocktakeLineSchema.Constants.WU_DateVerified,
								WhsStocktakeLineSchema.Constants.WU_GS_NKVerifiedBy
							},
				new string[] { WhsStocktakeLineSchema.Constants.WU_Count2,
									WhsStocktakeLineSchema.Constants.WU_Count2DateVerified,
								WhsStocktakeLineSchema.Constants.WU_Count2VerifiedBy
							},
				new string[] { WhsStocktakeLineSchema.Constants.WU_Count3,
								WhsStocktakeLineSchema.Constants.WU_Count3DateVerified,
								WhsStocktakeLineSchema.Constants.WU_Count3VerifiedBy
							}
			}.AsReadOnly();
		}

		#endregion

		#region SetColumnsReadOnly

		public void SetColumnsReadOnly(params string[] columns)
		{
			foreach (var column in columns)
			{
				Columns[column].ColumnStyle.ReadOnly = true;
				DefaultColumns[column].ColumnStyle.ReadOnly = true;
			}

			RefreshTableStyles();
		}

		#endregion

		#region SetColumnsAvailability

		public void SetColumnsAvailability(bool isAvailable, params string[] columns)
		{
			SetAvailability(isAvailable, columns);
			RefreshTableStyles();
		}

		#endregion

		#region OrderCountColumns

		public void OrderCountColumns()
		{
			ReOrderGridColumns();
			Columns.HasLayoutChanged = true;
			BeginInvoke(new MethodInvoker(RefreshTableStyles));
		}

		void ReOrderGridColumns()
		{
			var defaultFirstColumns = CountColumns[0];
			var lastDefaultIndex = DefaultColumns.ToArray().ToList().FindIndex(c => c.ColumnName == defaultFirstColumns[2]);
			MoveColumns(DefaultColumns, 0, lastDefaultIndex); // Move Default columns

			var currentLastColumn = Columns.FirstOrDefault(c => c.ColumnName == CountColumns[0][2]
																|| c.ColumnName == CountColumns[1][2]
																|| c.ColumnName == CountColumns[2][2]);

			if (currentLastColumn != null)
			{
				var lastCurrentIndex = Columns.ToList().FindIndex(c => c.ColumnName == currentLastColumn.ColumnName);
				var startIndex = CountColumns.ToList().FindIndex(c => c.Contains(currentLastColumn.ColumnName));

				if (startIndex == -1)
				{
					throw new NotImplementedException("Count column in the grid is not found in the countColumns collection.");
				}

				MoveColumns(Columns, startIndex, lastCurrentIndex); // Move current columns
			}
		}

		void MoveColumns(ZGridColumns columns, int startIndex, int lastIndex)
		{
			for (int index = ++startIndex; index < CountColumns.Count; index++)
			{
				foreach (var countColumn in CountColumns[index])
				{
					if (columns.Contains(countColumn))
					{
						columns.Move(columns[countColumn], ++lastIndex);
					}
				}
			}
		}

		#endregion

		#region IsDeleteMenuItemVisible

		protected override bool IsDeleteMenuItemVisible
		{
			get
			{
				bool result = false;
				if (IsRowSelected)
				{
					var selectedElements = SelectedElements;
					if (selectedElements.Length > 0)
					{
						if (selectedElements.Cast<WhsStocktakeLine>().All(l => l.WU_Status == StocktakeLineStatus.Codes.Open))
						{
							result = true;
						}
					}
				}

				return result && base.IsDeleteMenuItemVisible;
			}
		}

		bool IsRowSelected => SelectedRowCount > 0;

		#endregion
	}
}
