using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.Module
{
	public sealed partial class SundryChargesFilterControl : ZFilterStripControl<SundryChargesModuleStrip>
	{
		[Obsolete("Use the constructor that takes a collection and filter strip, this constructor is just for the designer", true)]
		public SundryChargesFilterControl()
		{
			InitializeComponent();
		}

		public SundryChargesFilterControl(IBusinessObjectCollection collection, FilterStripBusinessObject strip)
			: base(collection, strip)
		{
			InitializeComponent();
		}
	}
}
