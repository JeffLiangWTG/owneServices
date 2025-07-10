using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.Module;

partial class CarrierChargeCodeFilterControl : ZFilterStripControl
{
	public CarrierChargeCodeFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
		: base(gridCollection, filterBusinessObject)
	{
		InitializeComponent();
		new AssignCarrierMenuItem().Attach(Grid);
	}

	#region Dispose

	readonly System.ComponentModel.Container components;

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}

		base.Dispose(disposing);
	}

	#endregion
}
