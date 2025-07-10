using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Warehouse.Transit.GUI
{
	public partial class CFSDetailsToolStripButton : ToolStripControlHost
	{
		public CFSDetailsToolStripButton() : base(new CFSTile() { })
		{
			InitializeComponent();
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public CFSDetailsToolStripButton(ViewPackagesCFSInfo details) : base(new CFSTile(details) { })
		{
			InitializeComponent();

			Details = details;
			this.BackColor = UnselectedTileColor;
			this.Enabled = Details.TransitWarehouse != null;

			Details.DeselectedEvent += (s, e) => { this.BackColor = UnselectedTileColor; };
		}

		public ViewPackagesCFSInfo Details { get; }

		#region Dispose

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Event Handlers

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			if (Details.TransitWarehouse != null)
			{
				Details.SelectCFS();
				this.BackColor = SelectedTileColor;
			}
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			this.BackColor = HoverOverTileColor;
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			if (!Details.IsSelected)
			{
				this.BackColor = UnselectedTileColor;
			}
			else
			{
				this.BackColor = SelectedTileColor;
			}
		}

		protected override void OnMouseHover(EventArgs e)
		{
			base.OnMouseHover(e);
			this.BackColor = HoverOverTileColor;
		}

		#endregion

		static Color UnselectedTileColor => Color.FromArgb(220, 225, 228);

		static Color SelectedTileColor => Color.FromArgb(176, 208, 233);

		static Color HoverOverTileColor => Color.FromArgb(198, 216, 231);
	}
}
