using System;
using System.Drawing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Environment.Module
{
	public partial class PickFacesFilterControl : ZFilterStripControl
	{
		public PickFacesFilterControl()
			: this(null, null)
		{
		}

		public PickFacesFilterControl(WhsPickFaceViewCollection collection, PickFacesFilterBusinessObject strip)
			: base(collection, strip)
		{
			InitializeComponent();
			FilteredGrid.ColourDeciding += new EventHandler<ColourDecidingEventArgs>(FilteredPickFaces_ColorDeciding);
		}

		#region ColorDeciding

		internal void FilteredPickFaces_ColorDeciding(object sender, ColourDecidingEventArgs e)
		{
			var pickFace = (WhsPickFaceView)e.ObjectAtRow;
			if (pickFace.IsPickFaceAssigned)
			{
				e.Colour = Color.LightGreen;
			}
			else
			{
				e.Colour = pickFace.HasStockOrPendingTransactions ? Color.Orange : Color.DeepSkyBlue;
			}
		}

		#endregion
	}
}

