using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class ABCCategoryWarehouseFilterControl : ZUserControl
	{
		public ABCCategoryWarehouseFilterControl()
		{
			InitializeComponent();
			WarehouseFindBox.ReadOnly = string.IsNullOrEmpty(ABCCategoryDropEdit.CodeBox.Text);
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			HookEvents();
		}

		void ABCCategoryDropEdit_Validated(object sender, EventArgs e)
		{
			WarehouseFindBox.ReadOnly = string.IsNullOrEmpty(ABCCategoryDropEdit.CodeBox.Text) || DataSource.HasErrors;
		}

		new ABCCategoryWarehouseFilter DataSource
		{
			get { return (ABCCategoryWarehouseFilter)base.DataSource; }
		}

		#region Hook Events

		void HookEvents()
		{
			ABCCategoryDropEdit.Validated += new EventHandler(ABCCategoryDropEdit_Validated);
		}

		void UnHookEvents()
		{
			ABCCategoryDropEdit.Validated -= new EventHandler(ABCCategoryDropEdit_Validated);
		}

		#endregion

		#region Dispose

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			UnHookEvents();
			base.Dispose(disposing);
		}

		#endregion
	}
}
