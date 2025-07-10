using System.Drawing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgRateTariffLevelControl : ZUserControl
	{
		public OrgRateTariffLevelControl()
		{
			InitializeComponent();
		}

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (!firstBound && dataSource != null)
			{
				TariffLevelGrid.ColourDeciding += EntriesGrid_ColourDeciding;
				firstBound = true;
			}
			base.SetDataBinding(dataSource, dataMember);
		}

		protected bool firstBound;

		#endregion

		#region EntriesGrid_ColourDeciding

		void EntriesGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			OrgRateTariffLevel entry = e.ObjectAtRow as OrgRateTariffLevel;
			if (entry != null && !entry.IsDeleted && entry.IsExpired)
			{
				e.Colour = Color.PaleGoldenrod;
			}
		}

		#endregion

		#region ReadOnly

		public bool ReadOnly
		{
			get { return TariffLevelGrid.ReadOnly; }
			set { TariffLevelGrid.ReadOnly = value; }
		}

		#endregion

		#region IDisposable Members

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				TariffLevelGrid.ColourDeciding -= EntriesGrid_ColourDeciding;
			}
			base.Dispose(disposing);
		}

		#endregion

#if DEBUG
		public void EntriesGrid_ColourDeciding_Exposed(ColourDecidingEventArgs e)
		{
			EntriesGrid_ColourDeciding(null, e);
		}
#endif
	}
}
