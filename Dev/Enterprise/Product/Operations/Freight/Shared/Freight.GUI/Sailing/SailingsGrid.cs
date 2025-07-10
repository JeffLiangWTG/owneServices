using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public class SailingsGrid : ZGrid
	{
		public SailingsGrid()
		{
		}

		public string CopyToText
		{
			get { return CopyTo.Text; }
			set { CopyTo.Text = value; }
		}

		#region Grid Overrides

		protected override void HookContextMenu()
		{
			base.HookContextMenu();
			fOldMenu = ContextMenu;
			if (fOldMenu != null)
			{
				fOldMenu.MenuItems.Add(CopyTo);
			}
		}

		protected override void UnHookContextMenu()
		{
			if (fOldMenu != null)
			{
				fOldMenu.MenuItems.Remove(CopyTo);
				fOldMenu = null;
			}
			base.UnHookContextMenu();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				HitTestInfo info = HitTest(e.X, e.Y);

				if (info.Column < 0 || info.Row < 0)
				{
					CopyTo.Visible = false;
				}
				else
				{
					DataGridColumnStyle style = Columns[info.Column].ColumnStyle;
					fPropertyName = style.MappingName;
					CopyTo.Visible = !style.ReadOnly;
				}
			}
			base.OnMouseDown(e);
		}

		#endregion

		#region Events

		void CopyTo_Click(object sender, EventArgs e)
		{
			JobSailing sailing = (JobSailing)ListManager.List[CurrentRowIndex];
			sailing.CopyDetailToOtherSailingsWithSameOrigin(fPropertyName);
		}

		#endregion

		#region Implementation

		ContextMenu fOldMenu;
		string fPropertyName;

		#region CopyTo

		MenuItem fCopyTo;
		MenuItem CopyTo
		{
			get
			{
				if (fCopyTo == null)
				{
					fCopyTo = new ZMenuItem(ResString.GetMultilingualString("Freight.Sailing.CopyToSailingsWithSameOrigin", "Copy To Sailings With the Same Origin"));
					fCopyTo.Click += new EventHandler(CopyTo_Click);
				}

				return fCopyTo;
			}
		}

		#endregion

		#endregion

		#region TestHelper
#if DEBUG

		public class TestHelper
		{
			public readonly SailingsGrid Grid;

			public TestHelper(SailingsGrid grid)
			{
				this.Grid = grid;
			}

			public void FireRightClick(int row, int column)
			{
				Rectangle rect = Grid.GetCellBounds(row, column);
				Grid.OnMouseDown(new MouseEventArgs(MouseButtons.Right, 1, rect.X + (rect.Width >> 1), rect.Y + (rect.Height >> 1), 0));
			}

			public MenuItem CopyTo
			{
				get { return Grid.fCopyTo; }
			}
		}

#endif
		#endregion
	}
}
